using System;
using System.Collections.Generic;
using System.Linq;
using FlowPuzzle.Core;

namespace FlowPuzzle.Editor.Draft
{
    [Serializable]
    public sealed class FlowLevelDraft
    {
        public int levelId;
        public int width;
        public int height;
        public int colorCount;
        public int seed;
        public bool isSolutionDirty;
        public bool isValidated;

        public List<FlowDraftPairData> pairs = new List<FlowDraftPairData>();
        public List<FlowDraftConstraintData> fixedConstraints = new List<FlowDraftConstraintData>();
        public FlowSolutionData currentSolution;
        public FlowDifficultyReport currentDifficulty;

        public bool HasCompleteEndpoints
        {
            get
            {
                for (int i = 0; i < colorCount; i++)
                {
                    var pair = pairs.FirstOrDefault(p => p.colorId == i);
                    if (pair == null || pair.endpointA == null || pair.endpointB == null)
                        return false;
                }
                return true;
            }
        }

        public FlowLevelDraft Clone()
        {
            var c = new FlowLevelDraft
            {
                levelId = levelId, width = width, height = height,
                colorCount = colorCount, seed = seed,
                isSolutionDirty = isSolutionDirty, isValidated = isValidated
            };
            foreach (var p in pairs) c.pairs.Add(p.Clone());
            foreach (var fc in fixedConstraints) c.fixedConstraints.Add(fc.Clone());
            if (currentSolution != null) c.currentSolution = DeepCopySolution(currentSolution);
            if (currentDifficulty != null) c.currentDifficulty = DeepCopyDifficulty(currentDifficulty);
            return c;
        }

        public void RestoreFrom(FlowLevelDraft snapshot)
        {
            levelId = snapshot.levelId; width = snapshot.width; height = snapshot.height;
            colorCount = snapshot.colorCount; seed = snapshot.seed;
            isSolutionDirty = snapshot.isSolutionDirty; isValidated = snapshot.isValidated;
            pairs.Clear(); foreach (var p in snapshot.pairs) pairs.Add(p.Clone());
            fixedConstraints.Clear(); foreach (var fc in snapshot.fixedConstraints) fixedConstraints.Add(fc.Clone());
            currentSolution = snapshot.currentSolution != null ? DeepCopySolution(snapshot.currentSolution) : null;
            currentDifficulty = snapshot.currentDifficulty != null ? DeepCopyDifficulty(snapshot.currentDifficulty) : null;
        }

        // ── atomic mutation API ──

        public FlowDraftMutationResult AddColor()
        {
            var snapshot = Clone();
            var newId = LowestUnusedColorId();
            // Must keep colorCount updated
            try
            {
                pairs.Add(new FlowDraftPairData { colorId = newId });
                colorCount = Math.Max(colorCount, newId + 1);
                MarkDirty();
                return FlowDraftMutationResult.Ok();
            }
            catch { RestoreFrom(snapshot); throw; }
        }

        public FlowDraftMutationResult RemoveColor(int colorId)
        {
            var snapshot = Clone();
            try
            {
                var pair = pairs.FirstOrDefault(p => p.colorId == colorId);
                if (pair == null) return FlowDraftMutationResult.Fail("ColorNotFound", $"Color {colorId} not found.");
                pairs.Remove(pair);
                fixedConstraints.RemoveAll(c => c.colorId == colorId);
                // Recompute colorCount: max ID + 1, or 0 if empty
                colorCount = pairs.Count == 0 ? 0 : pairs.Max(p => p.colorId) + 1;
                MarkDirty();
                return FlowDraftMutationResult.Ok();
            }
            catch { RestoreFrom(snapshot); throw; }
        }

        public FlowDraftMutationResult PlaceEndpoint(int colorId, bool isEndpointA, FlowPos position)
        {
            if (!IsInside(position)) return FlowDraftMutationResult.Fail("OutOfBounds", "Position outside board.");
            var snapshot = Clone();
            try
            {
                var pair = EnsurePair(colorId);
                // Check overlap with another endpoint
                foreach (var p in pairs)
                {
                    if (p.colorId == colorId) continue;
                    if ((p.endpointA.HasValue && p.endpointA.Value.Equals(position)) ||
                        (p.endpointB.HasValue && p.endpointB.Value.Equals(position)))
                        return FlowDraftMutationResult.Fail("EndpointOverlap", "Position already occupied by another endpoint.");
                }
                if (isEndpointA) pair.endpointA = position; else pair.endpointB = position;
                MarkDirty();
                return FlowDraftMutationResult.Ok();
            }
            catch { RestoreFrom(snapshot); return FlowDraftMutationResult.Fail("UnexpectedError", "Placement failed."); }
        }

        public FlowDraftMutationResult MoveEndpoint(int colorId, bool isEndpointA, FlowPos position)
            => PlaceEndpoint(colorId, isEndpointA, position);

        public FlowDraftMutationResult RemoveEndpoint(int colorId, bool isEndpointA)
        {
            var pair = pairs.FirstOrDefault(p => p.colorId == colorId);
            if (pair == null) return FlowDraftMutationResult.Fail("ColorNotFound", $"Color {colorId} not found.");
            var snapshot = Clone();
            try
            {
                if (isEndpointA) pair.endpointA = null; else pair.endpointB = null;
                MarkDirty();
                return FlowDraftMutationResult.Ok();
            }
            catch { RestoreFrom(snapshot); return FlowDraftMutationResult.Fail("UnexpectedError", "Removal failed."); }
        }

        public FlowDraftMutationResult Resize(int newWidth, int newHeight)
        {
            if (newWidth <= 0 || newHeight <= 0)
                return FlowDraftMutationResult.Fail("InvalidDimensions", "New dimensions must be positive.");
            var snapshot = Clone();
            try
            {
                width = newWidth; height = newHeight;
                // Remove out-of-bounds endpoints
                foreach (var p in pairs)
                {
                    if (p.endpointA.HasValue && !IsInside(p.endpointA.Value)) p.endpointA = null;
                    if (p.endpointB.HasValue && !IsInside(p.endpointB.Value)) p.endpointB = null;
                }
                // Clear constraints with out-of-bounds cells
                for (int i = fixedConstraints.Count - 1; i >= 0; i--)
                {
                    var fc = fixedConstraints[i];
                    if (fc.cells == null || fc.cells.Any(cell => !IsInside(cell)))
                        fixedConstraints.RemoveAt(i);
                }
                MarkDirty();
                return FlowDraftMutationResult.Ok();
            }
            catch { RestoreFrom(snapshot); return FlowDraftMutationResult.Fail("UnexpectedError", "Resize failed."); }
        }

        // ── helpers ──

        public int LowestUnusedColorId()
        {
            var used = new HashSet<int>(pairs.Select(p => p.colorId));
            for (int i = 0; ; i++) if (!used.Contains(i)) return i;
        }

        public FlowDraftPairData GetPair(int colorId) => pairs.FirstOrDefault(p => p.colorId == colorId);

        public void MarkDirty() { isSolutionDirty = true; isValidated = false; }

        private bool IsInside(FlowPos pos) => pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height;

        private FlowDraftPairData EnsurePair(int colorId)
        {
            var p = GetPair(colorId);
            if (p == null) { p = new FlowDraftPairData { colorId = colorId }; pairs.Add(p); return p; }
            return p;
        }

        private static FlowSolutionData DeepCopySolution(FlowSolutionData src)
        {
            var c = new FlowSolutionData { levelId = src.levelId };
            if (src.paths == null) return c;
            foreach (var p in src.paths)
            {
                var cc = new List<FlowPos>(p.cells.Count);
                foreach (var cell in p.cells) cc.Add(new FlowPos(cell.x, cell.y));
                c.paths.Add(new FlowPathData { colorId = p.colorId, cells = cc });
            }
            return c;
        }

        private static FlowDifficultyReport DeepCopyDifficulty(FlowDifficultyReport src)
        {
            return new FlowDifficultyReport
            {
                difficulty = src.difficulty, totalScore = src.totalScore,
                boardSizeScore = src.boardSizeScore, colorCountScore = src.colorCountScore,
                coverageScore = src.coverageScore, turnScore = src.turnScore,
                detourScore = src.detourScore, interactionScore = src.interactionScore,
                endpointDistanceScore = src.endpointDistanceScore, bottleneckScore = src.bottleneckScore,
                totalTurnCount = src.totalTurnCount, totalDetour = src.totalDetour,
                differentColorAdjacentCount = src.differentColorAdjacentCount,
                totalEndpointManhattanDistance = src.totalEndpointManhattanDistance,
                bottleneckCount = src.bottleneckCount
            };
        }
    }
}
