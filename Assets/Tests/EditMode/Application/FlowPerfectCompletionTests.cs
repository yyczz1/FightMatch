using System.Collections.Generic;
using System.Threading;
using FlowPuzzle.Application;
using FlowPuzzle.Core;
using FlowPuzzle.Difficulty;
using FlowPuzzle.Solving;
using NUnit.Framework;

namespace FlowPuzzle.Tests.Application
{
    [TestFixture]
    public sealed class FlowPerfectCompletionTests
    {
        [Test]
        public void Score_CoverageRangeHasPriorityOverPathLengthRange()
        {
            var config = BaseConfig();
            config.minCoverageRatio = 0.4f;
            config.maxCoverageRatio = 0.6f;
            config.minPathLength = 5;
            config.maxPathLength = 5;
            var scorer = new FlowPerfectCandidateScorer(Level(), config, new FlowDifficultyEvaluator());

            var direct = scorer.Evaluate(DirectSolution());
            var detour = scorer.Evaluate(DetourSolution());

            Assert.AreEqual(0f, direct.coveragePenalty, 0.001f);
            Assert.Greater(detour.coveragePenalty, 0f);
            Assert.Greater(scorer.Compare(direct, detour), 0,
                "Coverage match is ranked before a lower-priority path-length match.");
        }

        [Test]
        public void Score_PathLengthRangeSelectsLaterLongerCandidateWhenCoverageAllowsBoth()
        {
            var config = BaseConfig();
            config.minCoverageRatio = 0f;
            config.maxCoverageRatio = 1f;
            config.minPathLength = 5;
            config.maxPathLength = 5;
            var scorer = new FlowPerfectCandidateScorer(Level(), config, new FlowDifficultyEvaluator());

            var direct = scorer.Evaluate(DirectSolution());
            var detour = scorer.Evaluate(DetourSolution());

            Assert.Greater(direct.pathLengthPenalty, 0f);
            Assert.AreEqual(0f, detour.pathLengthPenalty, 0.001f);
            Assert.Greater(scorer.Compare(detour, direct), 0);
        }

        [Test]
        public void Score_TargetScoreRangeUsesDifficultyEvaluatorResult()
        {
            var config = BaseConfig();
            config.minCoverageRatio = 0f;
            config.maxCoverageRatio = 1f;
            config.minPathLength = 2;
            config.maxPathLength = 6;
            config.useTargetScoreRange = true;
            config.minTargetDifficultyScore = 60f;
            config.maxTargetDifficultyScore = 70f;
            var scorer = new FlowPerfectCandidateScorer(Level(), config, new FlowDifficultyEvaluator());

            var direct = scorer.Evaluate(DirectSolution());
            var detour = scorer.Evaluate(DetourSolution());

            Assert.Greater(direct.scoreRangePenalty, 0f);
            Assert.AreEqual(0f, detour.scoreRangePenalty, 0.001f);
            Assert.Greater(scorer.Compare(detour, direct), 0);
        }

        [Test]
        public void Score_NeutralPreferencesPreferCleanerShorterPath()
        {
            var config = BaseConfig();
            config.minCoverageRatio = 0f;
            config.maxCoverageRatio = 1f;
            config.minPathLength = 2;
            config.maxPathLength = 6;
            var scorer = new FlowPerfectCandidateScorer(Level(), config, new FlowDifficultyEvaluator());

            var direct = scorer.Evaluate(DirectSolution());
            var detour = scorer.Evaluate(DetourSolution());

            Assert.AreEqual(0f, direct.coveragePenalty, 0.001f);
            Assert.AreEqual(0f, detour.coveragePenalty, 0.001f);
            Assert.AreEqual(0f, direct.pathLengthPenalty, 0.001f);
            Assert.AreEqual(0f, detour.pathLengthPenalty, 0.001f);
            Assert.Greater(scorer.Compare(direct, detour), 0,
                "When explicit parameters are tied, the path with less detour and fewer occupied cells must win.");
        }

        [Test]
        public void Provider_EnumeratesPastFirstSolutionAndReturnsBestCandidate()
        {
            var request = Request();
            request.qualityConfig.minPathLength = 5;
            request.qualityConfig.maxPathLength = 5;

            var result = new FlowPerfectCompletionProvider(new FlowDifficultyEvaluator())
                .CompleteAsync(request, null, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(FlowSolveStatus.Solved, result.status);
            Assert.Greater(result.candidateCount, 1);
            Assert.AreEqual(5, result.generatedLevel.solutionData.paths[0].cells.Count,
                "Perfect Complete must be able to select a candidate found after the first solution.");
            Assert.AreEqual(5, result.candidateTotalPathCells);
            Assert.AreEqual(2, result.candidateTotalDetour);
            Assert.IsFalse(result.searchLimitReached);
        }

        [Test]
        public void Provider_BudgetAfterFirstCandidateReturnsBestSoFar()
        {
            var request = Request();
            request.nodeBudget = 4;
            request.qualityConfig.minPathLength = 5;
            request.qualityConfig.maxPathLength = 5;

            var result = new FlowPerfectCompletionProvider(new FlowDifficultyEvaluator())
                .CompleteAsync(request, null, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(FlowSolveStatus.Solved, result.status);
            Assert.AreEqual(1, result.candidateCount);
            Assert.IsTrue(result.searchLimitReached);
            Assert.AreEqual(3, result.generatedLevel.solutionData.paths[0].cells.Count);
        }

        [Test]
        public void Provider_RepeatedRequestSelectsSameCanonicalLayout()
        {
            var request = Request();
            var provider = new FlowPerfectCompletionProvider(new FlowDifficultyEvaluator());

            var first = provider.CompleteAsync(request, null, CancellationToken.None).GetAwaiter().GetResult();
            var second = provider.CompleteAsync(request, null, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(FlowSolveStatus.Solved, first.status);
            Assert.AreEqual(FlowSolveStatus.Solved, second.status);
            Assert.AreEqual(first.candidateCount, second.candidateCount);
            CollectionAssert.AreEqual(
                first.generatedLevel.solutionData.paths[0].cells,
                second.generatedLevel.solutionData.paths[0].cells);
        }

        [Test]
        public void Provider_EqualQualityKeepsCurrentValidatedRecommendation()
        {
            var level = TieLevel();
            var current = TieCurrentSolution();
            var request = new FlowCompletionRequest
            {
                levelData = level,
                currentSolution = current,
                fixedPrefixes = new List<FlowPathData>(),
                qualityConfig = BaseConfig(),
                nodeBudget = 10000,
                timeoutMs = 10000,
                progressIntervalNodes = 1000
            };

            var result = new FlowPerfectCompletionProvider(new FlowDifficultyEvaluator())
                .CompleteAsync(request, null, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(FlowSolveStatus.Solved, result.status);
            CollectionAssert.AreEqual(current.paths[0].cells, result.generatedLevel.solutionData.paths[0].cells,
                "An equally-ranked candidate must not replace the current validated recommendation just because its coordinates sort first.");
        }

        private static FlowCompletionRequest Request()
        {
            return new FlowCompletionRequest
            {
                levelData = Level(),
                fixedPrefixes = new List<FlowPathData>(),
                qualityConfig = BaseConfig(),
                nodeBudget = 10000,
                timeoutMs = 10000,
                progressIntervalNodes = 1000
            };
        }

        private static FlowGenerationConfig BaseConfig()
        {
            return new FlowGenerationConfig
            {
                width = 3,
                height = 2,
                colorCount = 1,
                minCoverageRatio = 0f,
                maxCoverageRatio = 1f,
                minPathLength = 2,
                maxPathLength = 6,
                maxPathAttempt = 10,
                maxLevelAttempt = 10,
                solverNodeBudget = 10000,
                solverTimeoutMilliseconds = 10000
            };
        }

        private static FlowLevelData Level()
        {
            var level = new FlowLevelData { levelId = 1, width = 3, height = 2 };
            level.pairs.Add(new FlowPairData
            {
                colorId = 0,
                endpointA = new FlowPos(0, 0),
                endpointB = new FlowPos(2, 0)
            });
            return level;
        }

        private static FlowSolutionData DirectSolution()
        {
            var solution = new FlowSolutionData { levelId = 1 };
            solution.paths.Add(new FlowPathData
            {
                colorId = 0,
                cells = new List<FlowPos>
                {
                    new FlowPos(0, 0),
                    new FlowPos(1, 0),
                    new FlowPos(2, 0)
                }
            });
            return solution;
        }

        private static FlowSolutionData DetourSolution()
        {
            var solution = new FlowSolutionData { levelId = 1 };
            solution.paths.Add(new FlowPathData
            {
                colorId = 0,
                cells = new List<FlowPos>
                {
                    new FlowPos(0, 0),
                    new FlowPos(0, 1),
                    new FlowPos(1, 1),
                    new FlowPos(2, 1),
                    new FlowPos(2, 0)
                }
            });
            return solution;
        }

        private static FlowLevelData TieLevel()
        {
            var level = new FlowLevelData { levelId = 2, width = 3, height = 2 };
            level.pairs.Add(new FlowPairData
            {
                colorId = 0,
                endpointA = new FlowPos(0, 0),
                endpointB = new FlowPos(2, 1)
            });
            return level;
        }

        private static FlowSolutionData TieCurrentSolution()
        {
            var solution = new FlowSolutionData { levelId = 2 };
            solution.paths.Add(new FlowPathData
            {
                colorId = 0,
                cells = new List<FlowPos>
                {
                    new FlowPos(0, 0),
                    new FlowPos(1, 0),
                    new FlowPos(2, 0),
                    new FlowPos(2, 1)
                }
            });
            return solution;
        }
    }
}
