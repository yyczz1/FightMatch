using System;
using System.Collections.Generic;
using System.Linq;
using FlowPuzzle.Core;
using FlowPuzzle.Difficulty;
using FlowPuzzle.Validation;

namespace FlowPuzzle.Generation
{
    public sealed class FlowSolutionGenerator
    {
        private readonly FlowPathLengthAllocator allocator;
        private readonly IFlowPathGenerationStrategy pathStrategy;
        private readonly FlowSolutionValidator validator;
        private readonly FlowDifficultyEvaluator difficultyEvaluator;

        public FlowSolutionGenerator(
            FlowPathLengthAllocator allocator,
            IFlowPathGenerationStrategy pathStrategy,
            FlowSolutionValidator validator,
            FlowDifficultyEvaluator difficultyEvaluator)
        {
            this.allocator = allocator ?? throw new ArgumentNullException(nameof(allocator));
            this.pathStrategy = pathStrategy ?? throw new ArgumentNullException(nameof(pathStrategy));
            this.validator = validator ?? throw new ArgumentNullException(nameof(validator));
            this.difficultyEvaluator = difficultyEvaluator ?? throw new ArgumentNullException(nameof(difficultyEvaluator));
        }

        public FlowGenerationResult Generate(
            int levelId,
            FlowGenerationConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            // Resolve seed before configuration checks
            var resolvedSeed = config.useRandomSeed
                ? Environment.TickCount ^ (levelId * 9973)
                : config.seed;

            // Configuration checks — use resolvedSeed and attemptCount==0
            if (config.width <= 0 || config.height <= 0 || config.colorCount <= 0)
                return FlowGenerationResult.Failure(levelId, resolvedSeed, 0,
                    "InvalidDimensions", "Width, height, and colorCount must be positive.");

            if (config.minCoverageRatio < 0f || config.maxCoverageRatio > 1f ||
                config.minCoverageRatio > config.maxCoverageRatio)
                return FlowGenerationResult.Failure(levelId, resolvedSeed, 0,
                    "ImpossibleCoverageRange",
                    $"Coverage range [{config.minCoverageRatio}, {config.maxCoverageRatio}] is impossible.");

            if (config.minPathLength < 2 || config.minPathLength > config.maxPathLength)
                return FlowGenerationResult.Failure(levelId, resolvedSeed, 0,
                    "InvalidPathLengthRange",
                    $"Path length range [{config.minPathLength}, {config.maxPathLength}] is invalid.");

            if (config.maxPathAttempt <= 0 || config.maxLevelAttempt <= 0)
                return FlowGenerationResult.Failure(levelId, resolvedSeed, 0,
                    "InvalidAttemptBudget", "maxPathAttempt and maxLevelAttempt must be positive.");

            var random = new SystemFlowRandom(resolvedSeed);

            var boardCapacity = config.width * config.height;
            string lastRejection = null;

            for (var attempt = 1; attempt <= config.maxLevelAttempt; attempt++)
            {
                // Sample target coverage
                var targetCoverage = (config.minCoverageRatio == config.maxCoverageRatio)
                    ? config.minCoverageRatio
                    : random.NextFloat(config.minCoverageRatio, config.maxCoverageRatio);

                var targetUsedCells = Math.Max(1, (int)Math.Round(boardCapacity * targetCoverage));

                // Allocate path lengths
                var allocResult = allocator.Allocate(config, targetUsedCells, random);

                if (!allocResult.success)
                {
                    return FlowGenerationResult.Failure(levelId, resolvedSeed, attempt,
                        allocResult.diagnostic.errorCode, allocResult.diagnostic.errorMessage);
                }

                // Generate paths in allocation generation order
                var board = new FlowBoard(config.width, config.height);
                var pathsByColor = new Dictionary<int, FlowPathData>();
                var allSucceeded = true;

                foreach (var colorId in allocResult.generationOrderColorIds)
                {
                    var targetLen = allocResult.pathLengthsByColorId[colorId];
                    var ok = pathStrategy.TryGeneratePath(board, colorId, targetLen,
                        config.maxPathAttempt,
                        config.turnPreference, config.interactionPreference,
                        random, out var path);

                    if (!ok)
                    {
                        allSucceeded = false;
                        lastRejection = "PathGenerationFailed";
                        break;
                    }

                    pathsByColor[colorId] = path;
                }

                if (!allSucceeded)
                    continue;

                // Compute actual coverage
                var usedCellCount = board.OccupiedCellCount;
                var actualCoverage = (float)usedCellCount / boardCapacity;

                if (actualCoverage < config.minCoverageRatio || actualCoverage > config.maxCoverageRatio)
                {
                    lastRejection = "CoverageOutOfRange";
                    continue;
                }

                // Build level and solution data
                var levelData = new FlowLevelData
                {
                    levelId = levelId,
                    width = config.width,
                    height = config.height
                };

                var solutionData = new FlowSolutionData { levelId = levelId };

                // Sorted color IDs for pairs and paths
                var sortedIds = allocResult.generationOrderColorIds.OrderBy(id => id).ToList();

                foreach (var colorId in sortedIds)
                {
                    var path = pathsByColor[colorId];
                    levelData.pairs.Add(new FlowPairData
                    {
                        colorId = colorId,
                        endpointA = path.cells[0],
                        endpointB = path.cells[path.cells.Count - 1]
                    });
                    solutionData.paths.Add(new FlowPathData
                    {
                        colorId = colorId,
                        cells = new List<FlowPos>(path.cells)
                    });
                }

                // Validate
                var validation = validator.Validate(levelData, solutionData);
                if (!validation.isValid)
                {
                    lastRejection = "ValidationFailed";
                    continue;
                }

                // Evaluate difficulty
                var difficultyReport = difficultyEvaluator.Evaluate(levelData, solutionData);

                // Sync difficulty summary into level data
                levelData.difficulty = difficultyReport.difficulty;
                levelData.difficultyScore = difficultyReport.totalScore;

                // Target difficulty filter
                if (config.useTargetDifficulty)
                {
                    if (difficultyReport.difficulty != config.targetDifficulty)
                    {
                        lastRejection = "DifficultyOutOfRange";
                        continue;
                    }
                }

                if (config.useTargetScoreRange)
                {
                    if (difficultyReport.totalScore < config.minTargetDifficultyScore ||
                        difficultyReport.totalScore > config.maxTargetDifficultyScore)
                    {
                        lastRejection = "DifficultyOutOfRange";
                        continue;
                    }
                }

                // Success
                var generatedLevel = new FlowGeneratedLevel
                {
                    levelData = levelData,
                    solutionData = solutionData,
                    difficultyReport = difficultyReport,
                    usedSeed = resolvedSeed,
                    coverageRatio = actualCoverage
                };

                return FlowGenerationResult.Success(levelId, resolvedSeed, attempt, generatedLevel);
            }

            return FlowGenerationResult.Failure(levelId, resolvedSeed, config.maxLevelAttempt,
                "MaxLevelAttemptsReached",
                lastRejection != null
                    ? $"Failed after {config.maxLevelAttempt} attempts. Last rejection: {lastRejection}."
                    : $"Failed after {config.maxLevelAttempt} attempts.");
        }
    }
}
