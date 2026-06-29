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
                    if (pairs.FirstOrDefault(p => p.colorId == i)?.endpointA == null
                        || pairs.FirstOrDefault(p => p.colorId == i)?.endpointB == null)
                        return false;
                return true;
            }
        }

        public FlowLevelDraft Clone()
        {
            var c = new FlowLevelDraft { levelId = levelId, width = width, height = height,
                colorCount = colorCount, seed = seed, isSolutionDirty = isSolutionDirty, isValidated = isValidated };
            foreach (var p in pairs) c.pairs.Add(p.Clone());
            foreach (var fc in fixedConstraints) c.fixedConstraints.Add(fc.Clone());
            c.currentSolution = currentSolution != null ? DeepCopySolution(currentSolution) : null;
            c.currentDifficulty = currentDifficulty != null ? DeepCopyDifficulty(currentDifficulty) : null;
            return c;
        }

        public void RestoreFrom(FlowLevelDraft s)
        {
            levelId = s.levelId; width = s.width; height = s.height;
            colorCount = s.colorCount; seed = s.seed;
            isSolutionDirty = s.isSolutionDirty; isValidated = s.isValidated;
            pairs.Clear(); foreach (var p in s.pairs) pairs.Add(p.Clone());
            fixedConstraints.Clear(); foreach (var fc in s.fixedConstraints) fixedConstraints.Add(fc.Clone());
            currentSolution = s.currentSolution != null ? DeepCopySolution(s.currentSolution) : null;
            currentDifficulty = s.currentDifficulty != null ? DeepCopyDifficulty(s.currentDifficulty) : null;
        }

        // ── atomic mutation API ──

        public FlowDraftMutationResult AddColor()
        {
            var snap = Clone();
            try
            {
                var newId = LowestUnusedColorId();
                pairs.Add(new FlowDraftPairData { colorId = newId });
                colorCount = Math.Max(colorCount, newId + 1);
                MarkDirty(); return FlowDraftMutationResult.Ok();
            }
            catch { RestoreFrom(snap); throw; }
        }

        public FlowDraftMutationResult RemoveColor(int colorId)
        {
            var pair = pairs.FirstOrDefault(p => p.colorId == colorId);
            if (pair == null) return FlowDraftMutationResult.Fail("ColorNotFound", $"Color {colorId} not found.");
            var snap = Clone();
            try
            {
                pairs.Remove(pair);
                fixedConstraints.RemoveAll(c => c.colorId == colorId);
                // Compact higher IDs
                for (int i = 0; i < pairs.Count; i++)
                    if (pairs[i].colorId > colorId) pairs[i].colorId--;
                for (int i = 0; i < fixedConstraints.Count; i++)
                    if (fixedConstraints[i].colorId > colorId) fixedConstraints[i].colorId--;
                colorCount = pairs.Count == 0 ? 0 : pairs.Max(p => p.colorId) + 1;
                MarkDirty(); return FlowDraftMutationResult.Ok();
            }
            catch { RestoreFrom(snap); throw; }
        }

        public FlowDraftMutationResult PlaceEndpoint(int colorId, bool isA, FlowPos pos)
        {
            if (!IsInside(pos))
                return FlowDraftMutationResult.Fail("OutOfBounds", "Position outside board.");
            var pair = EnsurePair(colorId);
            if (pair == null)
                return FlowDraftMutationResult.Fail("ColorNotFound", $"Color {colorId} not found.");
            var snap = Clone();
            try
            {
                // Cross-color overlap
                foreach (var p in pairs)
                {
                    if (p.colorId == colorId) continue;
                    if ((p.endpointA.HasValue && p.endpointA.Value.Equals(pos))
                        || (p.endpointB.HasValue && p.endpointB.Value.Equals(pos)))
                        return FlowDraftMutationResult.Fail("EndpointOverlap", "Position occupied by another color.");
                }
                // Same-color A/B overlap
                var other = isA ? pair.endpointB : pair.endpointA;
                if (other.HasValue && other.Value.Equals(pos))
                    return FlowDraftMutationResult.Fail("EndpointOverlap", "Endpoints of same color must not overlap.");
                var old = isA ? pair.endpointA : pair.endpointB;
                if (isA) pair.endpointA = pos; else pair.endpointB = pos;
                MarkDirty(); return FlowDraftMutationResult.Ok();
            }
            catch { RestoreFrom(snap); return FlowDraftMutationResult.Fail("Error", "Placement failed."); }
        }

        public FlowDraftMutationResult MoveEndpoint(int colorId, bool isA, FlowPos pos)
            => PlaceEndpoint(colorId, isA, pos);

        public FlowDraftMutationResult RemoveEndpoint(int colorId, bool isA)
        {
            var pair = pairs.FirstOrDefault(p => p.colorId == colorId);
            if (pair == null) return FlowDraftMutationResult.Fail("ColorNotFound", $"Color {colorId} not found.");
            var snap = Clone();
            try
            {
                if (isA) pair.endpointA = null; else pair.endpointB = null;
                MarkDirty(); return FlowDraftMutationResult.Ok();
            }
            catch { RestoreFrom(snap); return FlowDraftMutationResult.Fail("Error", "Removal failed."); }
        }

        public FlowDraftMutationResult Resize(int newW, int newH)
        {
            if (newW <= 0 || newH <= 0)
                return FlowDraftMutationResult.Fail("InvalidDimensions", "New dimensions must be positive.");
            var snap = Clone();
            try
            {
                width = newW; height = newH;
                foreach (var p in pairs)
                {
                    if (p.endpointA.HasValue && !IsInside(p.endpointA.Value)) p.endpointA = null;
                    if (p.endpointB.HasValue && !IsInside(p.endpointB.Value)) p.endpointB = null;
                }
                for (int i = fixedConstraints.Count - 1; i >= 0; i--)
                    if (fixedConstraints[i].cells == null || fixedConstraints[i].cells.Any(c => !IsInside(c)))
                        fixedConstraints.RemoveAt(i);
                MarkDirty(); return FlowDraftMutationResult.Ok();
            }
            catch { RestoreFrom(snap); return FlowDraftMutationResult.Fail("Error", "Resize failed."); }
        }

        // ── constraint validation ──

        public FlowDraftMutationResult ValidateConstraint(int colorId, List<FlowPos> cells)
        {
            if (cells == null || cells.Count < 2)
                return FlowDraftMutationResult.Fail("InvalidConstraint", "Constraint needs at least 2 cells.");
            var pair = pairs.FirstOrDefault(p => p.colorId == colorId);
            if (pair == null) return FlowDraftMutationResult.Fail("ColorNotFound", $"Color {colorId} not found.");

            // Must start at own endpoint
            var first = cells[0];
            if (!(pair.endpointA.HasValue && pair.endpointA.Value.Equals(first))
                && !(pair.endpointB.HasValue && pair.endpointB.Value.Equals(first)))
                return FlowDraftMutationResult.Fail("ConstraintNotAnchored", "Must start at its own endpoint.");

            // Must not reach the second endpoint
            var other = pair.endpointA.HasValue && pair.endpointA.Value.Equals(first)
                ? pair.endpointB : pair.endpointA;
            var last = cells.Last();
            if (other.HasValue && other.Value.Equals(last))
                return FlowDraftMutationResult.Fail("CompleteEndpointToEndpoint", "Constraint may not reach second endpoint.");

            var seen = new HashSet<FlowPos>();
            for (int i = 0; i < cells.Count; i++)
            {
                var c = cells[i];
                if (!IsInside(c))
                    return FlowDraftMutationResult.Fail("CellOutOfBounds", $"Cell {c.x},{c.y} out of bounds.");
                if (!seen.Add(c))
                    return FlowDraftMutationResult.Fail("DuplicateCell", "Constraint has duplicate cell.");
                if (i > 0 && !FlowPathUtility.AreAdjacent(cells[i - 1], c))
                    return FlowDraftMutationResult.Fail("InvalidAdjacency", "Cells must be orthogonally adjacent.");

                // May not traverse foreign endpoint
                foreach (var p in pairs)
                {
                    if (p.colorId == colorId) continue;
                    if ((p.endpointA.HasValue && p.endpointA.Value.Equals(c))
                        || (p.endpointB.HasValue && p.endpointB.Value.Equals(c)))
                        return FlowDraftMutationResult.Fail("ForeignEndpointTraversal",
                            $"Cell crosses foreign endpoint of color {p.colorId}.");
                }
                // May not cross other constraints (except own)
                foreach (var fc in fixedConstraints)
                {
                    if (fc.colorId == colorId || fc.cells == null) continue;
                    if (fc.cells.Any(x => x.Equals(c)))
                        return FlowDraftMutationResult.Fail("ConstraintOverlap",
                            $"Overlaps constraint of color {fc.colorId}.");
                }
            }
            return FlowDraftMutationResult.Ok();
        }

        public FlowDraftMutationResult ApplyConstraint(int colorId, List<FlowPos> cells)
        {
            var valid = ValidateConstraint(colorId, cells);
            if (!valid.success) return valid;
            var snap = Clone();
            try
            {
                fixedConstraints.RemoveAll(c => c.colorId == colorId);
                if (cells != null && cells.Count > 0)
                    fixedConstraints.Add(new FlowDraftConstraintData
                    { colorId = colorId, cells = new List<FlowPos>(cells) });
                MarkDirty(); return FlowDraftMutationResult.Ok();
            }
            catch { RestoreFrom(snap); return FlowDraftMutationResult.Fail("Error", "Apply failed."); }
        }

        public FlowDraftMutationResult EraseConstraint(int colorId, int fromIndex)
        {
            var fc = fixedConstraints.FirstOrDefault(c => c.colorId == colorId);
            if (fc == null || fc.cells == null)
                return FlowDraftMutationResult.Fail("NoConstraint", $"No constraint for color {colorId}.");
            if (fromIndex < 0 || fromIndex >= fc.cells.Count)
                return FlowDraftMutationResult.Fail("InvalidIndex", "Index out of range.");
            var snap = Clone();
            try
            {
                if (fromIndex == 0) { fixedConstraints.Remove(fc); }
                else
                {
                    fc.cells = fc.cells.Take(fromIndex).ToList();
                }
                MarkDirty(); return FlowDraftMutationResult.Ok();
            }
            catch { RestoreFrom(snap); return FlowDraftMutationResult.Fail("Error", "Erase failed."); }
        }

        // ── helpers ──

        public int LowestUnusedColorId() { var u = new HashSet<int>(pairs.Select(p => p.colorId)); int i = 0; while (u.Contains(i)) i++; return i; }
        public FlowDraftPairData GetPair(int colorId) => pairs.FirstOrDefault(p => p.colorId == colorId);
        public FlowDraftConstraintData GetConstraint(int colorId) => fixedConstraints.FirstOrDefault(c => c.colorId == colorId);
        public void MarkDirty() { isSolutionDirty = true; isValidated = false; }
        private bool IsInside(FlowPos pos) => pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height;

        private FlowDraftPairData EnsurePair(int colorId)
        {
            var p = GetPair(colorId);
            if (p == null) { p = new FlowDraftPairData { colorId = colorId }; pairs.Add(p); }
            return p;
        }

        private static FlowSolutionData DeepCopySolution(FlowSolutionData src)
        {
            if (src == null) return null;
            var c = new FlowSolutionData { levelId = src.levelId };
            if (src.paths != null) foreach (var p in src.paths)
                {
                    var cc = new List<FlowPos>(p.cells.Count);
                    foreach (var cell in p.cells) cc.Add(new FlowPos(cell.x, cell.y));
                    c.paths.Add(new FlowPathData { colorId = p.colorId, cells = cc });
                }
            return c;
        }

        private static FlowDifficultyReport DeepCopyDifficulty(FlowDifficultyReport src)
        {
            if (src == null) return null;
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
