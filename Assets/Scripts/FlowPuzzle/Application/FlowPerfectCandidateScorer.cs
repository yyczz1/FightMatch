using System;
using System.Linq;
using FlowPuzzle.Core;
using FlowPuzzle.Difficulty;

namespace FlowPuzzle.Application
{
    public sealed class FlowPerfectCandidateScorer
    {
        private const float Epsilon = 0.0001f;
        private readonly FlowLevelData level;
        private readonly FlowGenerationConfig config;
        private readonly FlowDifficultyEvaluator evaluator;

        public FlowPerfectCandidateScorer(
            FlowLevelData level,
            FlowGenerationConfig config,
            FlowDifficultyEvaluator evaluator)
        {
            this.level = level ?? throw new ArgumentNullException(nameof(level));
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            this.evaluator = evaluator ?? throw new ArgumentNullException(nameof(evaluator));
        }

        public FlowPerfectCandidateScore Evaluate(FlowSolutionData solution)
        {
            if (solution == null)
                throw new ArgumentNullException(nameof(solution));

            var report = evaluator.Evaluate(level, solution);
            var boardCells = Math.Max(1, level.width * level.height);
            var occupiedCells = solution.paths.Sum(path => path.cells?.Count ?? 0);
            var coverage = (float)occupiedCells / boardCells;
            var coveragePenalty = RangePenalty(coverage, config.minCoverageRatio, config.maxCoverageRatio);

            var pathLengthPenalty = 0f;
            var minLength = int.MaxValue;
            var maxLength = 0;
            foreach (var path in solution.paths)
            {
                var length = path.cells?.Count ?? 0;
                pathLengthPenalty += RangePenalty(length, config.minPathLength, config.maxPathLength);
                minLength = Math.Min(minLength, length);
                maxLength = Math.Max(maxLength, length);
            }

            if (solution.paths.Count == 0)
                minLength = 0;

            var tierPenalty = config.useTargetDifficulty
                ? Math.Abs((int)report.difficulty - (int)config.targetDifficulty)
                : 0f;
            var scoreRangePenalty = config.useTargetScoreRange
                ? RangePenalty(report.totalScore, config.minTargetDifficultyScore, config.maxTargetDifficultyScore)
                : 0f;
            var detourRangeEnabled = config.minDetour != 0 || config.maxDetour != 0;
            var detourPenalty = detourRangeEnabled
                ? RangePenalty(report.totalDetour, Math.Max(0, config.minDetour), Math.Max(config.minDetour, config.maxDetour))
                : 0f;

            var normalizedTurns = (float)report.totalTurnCount / boardCells;
            var normalizedInteractions = (float)report.differentColorAdjacentCount / Math.Max(1, boardCells * 2);
            var normalizedBottlenecks = (float)report.bottleneckCount / boardCells;
            var preferenceScore = ClampPreference(config.turnPreference) * normalizedTurns
                                  + ClampPreference(config.interactionPreference) * normalizedInteractions
                                  + ClampPreference(config.bottleneckPreference) * normalizedBottlenecks;
            var balancePenalty = (float)(maxLength - minLength) / boardCells;

            return new FlowPerfectCandidateScore
            {
                coveragePenalty = coveragePenalty,
                pathLengthPenalty = pathLengthPenalty,
                tierPenalty = tierPenalty,
                scoreRangePenalty = scoreRangePenalty,
                detourPenalty = detourPenalty,
                preferenceScore = preferenceScore,
                totalDetour = report.totalDetour,
                totalTurnCount = report.totalTurnCount,
                totalPathCells = occupiedCells,
                pathBalancePenalty = balancePenalty,
                selectionScore = 1000f
                                 - coveragePenalty * 100f
                                 - pathLengthPenalty * 10f
                                 - tierPenalty * 5f
                                 - scoreRangePenalty
                                 - detourPenalty
                                 + preferenceScore
                                 - report.totalDetour
                                 - report.totalTurnCount * 0.1f
                                 - occupiedCells * 0.01f
                                 - balancePenalty,
                coverageRatio = coverage,
                difficultyReport = report
            };
        }

        public int Compare(FlowPerfectCandidateScore left, FlowPerfectCandidateScore right)
        {
            if (left == null) return right == null ? 0 : -1;
            if (right == null) return 1;

            var comparison = CompareLower(left.coveragePenalty, right.coveragePenalty);
            if (comparison != 0) return comparison;
            comparison = CompareLower(left.pathLengthPenalty, right.pathLengthPenalty);
            if (comparison != 0) return comparison;
            comparison = CompareLower(left.tierPenalty, right.tierPenalty);
            if (comparison != 0) return comparison;
            comparison = CompareLower(left.scoreRangePenalty, right.scoreRangePenalty);
            if (comparison != 0) return comparison;
            comparison = CompareLower(left.detourPenalty, right.detourPenalty);
            if (comparison != 0) return comparison;
            comparison = CompareHigher(left.preferenceScore, right.preferenceScore);
            if (comparison != 0) return comparison;
            comparison = CompareLower(left.totalDetour, right.totalDetour);
            if (comparison != 0) return comparison;
            comparison = CompareLower(left.totalTurnCount, right.totalTurnCount);
            if (comparison != 0) return comparison;
            comparison = CompareLower(left.totalPathCells, right.totalPathCells);
            if (comparison != 0) return comparison;
            return CompareLower(left.pathBalancePenalty, right.pathBalancePenalty);
        }

        private static float RangePenalty(float value, float min, float max)
        {
            if (value < min) return min - value;
            if (value > max) return value - max;
            return 0f;
        }

        private static float ClampPreference(float value)
        {
            return Math.Max(-1f, Math.Min(1f, value));
        }

        private static int CompareLower(float left, float right)
        {
            if (Math.Abs(left - right) <= Epsilon) return 0;
            return left < right ? 1 : -1;
        }

        private static int CompareHigher(float left, float right)
        {
            if (Math.Abs(left - right) <= Epsilon) return 0;
            return left > right ? 1 : -1;
        }
    }
}
