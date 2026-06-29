using System;
using System.Collections.Generic;
using System.Linq;
using FlowPuzzle.Core;
using FlowPuzzle.Persistence;
using FlowPuzzle.Validation;

namespace FlowPuzzle.Editor.Draft
{
    public static class FlowDraftMapper
    {
        public static FlowLevelDraft FromGeneratedLevel(FlowGeneratedLevel level)
        {
            if (level == null || level.levelData == null)
                throw new ArgumentNullException(nameof(level));

            var draft = new FlowLevelDraft
            {
                levelId = level.levelData.levelId,
                width = level.levelData.width,
                height = level.levelData.height,
                colorCount = level.levelData.pairs.Count,
                seed = level.usedSeed,
                isSolutionDirty = false,
                isValidated = true
            };

            for (int i = 0; i < draft.colorCount; i++)
            {
                var p = level.levelData.pairs.FirstOrDefault(x => x.colorId == i);
                draft.pairs.Add(new FlowDraftPairData
                {
                    colorId = i,
                    endpointA = p != null ? new FlowPos(p.endpointA.x, p.endpointA.y) : null,
                    endpointB = p != null ? new FlowPos(p.endpointB.x, p.endpointB.y) : null
                });
            }

            if (level.solutionData != null)
                draft.currentSolution = DeepCopySolution(level.solutionData);
            if (level.difficultyReport != null)
                draft.currentDifficulty = DeepCopyDifficulty(level.difficultyReport);
            draft.isValidated = true;
            draft.isSolutionDirty = false;

            return draft;
        }

        public static FlowLevelDraft FromAsset(FlowLevelAsset asset)
        {
            if (asset == null) throw new ArgumentNullException(nameof(asset));
            return FromGeneratedLevel(new FlowGeneratedLevel
            {
                levelData = asset.levelData,
                solutionData = asset.solutionData,
                difficultyReport = asset.difficultyReport,
                usedSeed = asset.generationSeed,
                coverageRatio = asset.coverageRatio
            });
        }

        public static FlowGeneratedLevel ToGeneratedLevel(FlowLevelDraft draft)
        {
            if (draft == null) throw new ArgumentNullException(nameof(draft));
            if (!draft.HasCompleteEndpoints)
                throw new InvalidOperationException("Draft does not have complete endpoints.");
            if (draft.currentSolution == null)
                throw new InvalidOperationException("Draft has no current solution.");
            if (draft.isSolutionDirty)
                throw new InvalidOperationException("Draft solution is dirty.");
            if (!draft.isValidated)
                throw new InvalidOperationException("Draft is not validated.");

            var levelData = new FlowLevelData
            {
                levelId = draft.levelId, width = draft.width, height = draft.height
            };
            foreach (var pair in draft.pairs.OrderBy(p => p.colorId))
            {
                if (pair.endpointA == null || pair.endpointB == null) continue;
                levelData.pairs.Add(new FlowPairData
                {
                    colorId = pair.colorId,
                    endpointA = pair.endpointA.Value,
                    endpointB = pair.endpointB.Value
                });
            }
            if (draft.currentDifficulty != null)
            {
                levelData.difficulty = draft.currentDifficulty.difficulty;
                levelData.difficultyScore = draft.currentDifficulty.totalScore;
            }

            return new FlowGeneratedLevel
            {
                levelData = levelData,
                solutionData = draft.currentSolution,
                difficultyReport = draft.currentDifficulty,
                usedSeed = draft.seed
            };
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
