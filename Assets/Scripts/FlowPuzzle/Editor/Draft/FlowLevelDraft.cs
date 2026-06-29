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
        public FlowDraftConstraintData constraint;
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
            var clone = new FlowLevelDraft
            {
                levelId = levelId, width = width, height = height,
                colorCount = colorCount, seed = seed,
                isSolutionDirty = isSolutionDirty, isValidated = isValidated,
                constraint = constraint?.Clone()
            };
            foreach (var pair in pairs)
                clone.pairs.Add(pair.Clone());
            if (currentSolution != null)
                clone.currentSolution = DeepCopySolution(currentSolution);
            if (currentDifficulty != null)
                clone.currentDifficulty = DeepCopyDifficulty(currentDifficulty);
            return clone;
        }

        public void RestoreFrom(FlowLevelDraft snapshot)
        {
            levelId = snapshot.levelId; width = snapshot.width; height = snapshot.height;
            colorCount = snapshot.colorCount; seed = snapshot.seed;
            isSolutionDirty = snapshot.isSolutionDirty; isValidated = snapshot.isValidated;
            pairs.Clear();
            foreach (var p in snapshot.pairs) pairs.Add(p.Clone());
            constraint = snapshot.constraint?.Clone();
            currentSolution = snapshot.currentSolution != null
                ? DeepCopySolution(snapshot.currentSolution) : null;
            currentDifficulty = snapshot.currentDifficulty != null
                ? DeepCopyDifficulty(snapshot.currentDifficulty) : null;
        }

        public void MarkDirty() { isSolutionDirty = true; isValidated = false; }

        public int LowestUnusedColorId()
        {
            var used = new HashSet<int>(pairs.Select(p => p.colorId));
            for (int i = 0; i <= colorCount; i++)
                if (!used.Contains(i)) return i;
            return colorCount;
        }

        public FlowDraftPairData GetPair(int colorId)
            => pairs.FirstOrDefault(p => p.colorId == colorId);

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
