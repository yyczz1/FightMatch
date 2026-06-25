using System;

namespace FlowPuzzle.Generation
{
    public sealed class FlowBatchGenerator
    {
        private readonly FlowSolutionGenerator generator;

        public FlowBatchGenerator(FlowSolutionGenerator generator)
        {
            this.generator = generator ?? throw new ArgumentNullException(nameof(generator));
        }

        public FlowBatchReport Generate(FlowBatchRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (request.config == null)
                throw new ArgumentNullException(nameof(request.config));
            if (request.count <= 0)
                throw new ArgumentOutOfRangeException(nameof(request.count),
                    "Count must be positive.");

            var report = new FlowBatchReport { requestedCount = request.count };

            for (var i = 0; i < request.count; i++)
            {
                var levelId = request.startLevelId + i;
                var levelSeed = unchecked(request.baseSeed + levelId * 9973);

                // Deep-copy config and force fixed seed
                var config = DeepCopyConfig(request.config);
                config.useRandomSeed = false;
                config.seed = levelSeed;

                var result = generator.Generate(levelId, config);

                var item = new FlowBatchItemResult
                {
                    levelId = levelId,
                    usedSeed = levelSeed,
                    success = result.success,
                    generationResult = result,
                    message = result.success
                        ? $"Generated level {levelId}"
                        : result.diagnostic != null
                            ? $"{result.diagnostic.errorCode}: {result.diagnostic.errorMessage}"
                            : $"Generation failed for level {levelId}"
                };

                if (result.success)
                    report.successfulCount++;
                else
                    report.failedCount++;

                report.items.Add(item);
            }

            return report;
        }

        private static Core.FlowGenerationConfig DeepCopyConfig(Core.FlowGenerationConfig src)
        {
            return new Core.FlowGenerationConfig
            {
                width = src.width,
                height = src.height,
                colorCount = src.colorCount,
                minCoverageRatio = src.minCoverageRatio,
                maxCoverageRatio = src.maxCoverageRatio,
                minPathLength = src.minPathLength,
                maxPathLength = src.maxPathLength,
                maxPathAttempt = src.maxPathAttempt,
                maxLevelAttempt = src.maxLevelAttempt,
                useRandomSeed = src.useRandomSeed,
                seed = src.seed,
                useTargetDifficulty = src.useTargetDifficulty,
                targetDifficulty = src.targetDifficulty,
                useTargetScoreRange = src.useTargetScoreRange,
                minTargetDifficultyScore = src.minTargetDifficultyScore,
                maxTargetDifficultyScore = src.maxTargetDifficultyScore,
                turnPreference = src.turnPreference,
                interactionPreference = src.interactionPreference,
                minEndpointDistance = src.minEndpointDistance,
                maxEndpointDistance = src.maxEndpointDistance,
                minDetour = src.minDetour,
                maxDetour = src.maxDetour,
                bottleneckPreference = src.bottleneckPreference,
                solverTimeoutMilliseconds = src.solverTimeoutMilliseconds,
                solverNodeBudget = src.solverNodeBudget
            };
        }
    }
}
