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

            foreach (var p in level.levelData.pairs.OrderBy(p => p.colorId))
                draft.pairs.Add(new FlowDraftPairData
                {
                    colorId = p.colorId,
                    endpointA = new FlowPos(p.endpointA.x, p.endpointA.y),
                    endpointB = new FlowPos(p.endpointB.x, p.endpointB.y)
                });

            draft.currentSolution = DeepCopySolution(level.solutionData);
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

            var levelData = new FlowLevelData { levelId = draft.levelId, width = draft.width, height = draft.height };
            foreach (var pair in draft.pairs.OrderBy(p => p.colorId))
            {
                if (pair.endpointA == null || pair.endpointB == null) continue;
                levelData.pairs.Add(new FlowPairData
                {
                    colorId = pair.colorId,
                    endpointA = new FlowPos(pair.endpointA.Value.x, pair.endpointA.Value.y),
                    endpointB = new FlowPos(pair.endpointB.Value.x, pair.endpointB.Value.y)
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
                solutionData = DeepCopySolution(draft.currentSolution),
                difficultyReport = DeepCopyDifficulty(draft.currentDifficulty),
                usedSeed = draft.seed
            };
        }

        private static FlowSolutionData DeepCopySolution(FlowSolutionData src)
        {
            if (src == null) return null;
            var c = new FlowSolutionData { levelId = src.levelId };
            if (src.paths != null)
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
