using System;
using System.Threading;
using System.Threading.Tasks;
using FlowPuzzle.Application;
using FlowPuzzle.Core;
using FlowPuzzle.Difficulty;
using FlowPuzzle.Solving;
using FlowPuzzle.Validation;
using NUnit.Framework;

namespace FlowPuzzle.Tests.Application
{
    [TestFixture]
    public sealed class FlowLevelCompletionServiceTests
    {
        [Test]
        public void CompleteAsync_SolvedResult_ValidatesAndEvaluatesDifficulty()
        {
            var level = MakeLevel();
            var solution = MakeSolution();
            var service = new FlowLevelCompletionService(
                new StubProvider(new FlowCompletionResult
                {
                    status = FlowSolveStatus.Solved,
                    generatedLevel = new FlowGeneratedLevel
                    {
                        levelData = level,
                        solutionData = solution,
                        coverageRatio = 1f
                    }
                }),
                new FlowSolutionValidator(),
                new FlowDifficultyEvaluator());

            var result = Await(service.CompleteAsync(new FlowCompletionRequest { levelData = level }, null, CancellationToken.None));

            Assert.AreEqual(FlowSolveStatus.Solved, result.status);
            Assert.IsNotNull(result.generatedLevel.difficultyReport);
            Assert.AreEqual(result.generatedLevel.difficultyReport.difficulty, result.generatedLevel.levelData.difficulty);
            Assert.AreEqual(result.generatedLevel.difficultyReport.totalScore, result.generatedLevel.levelData.difficultyScore);
        }

        [Test]
        public void CompleteAsync_InvalidProviderSolution_ReturnsError()
        {
            var level = MakeLevel();
            var badSolution = new FlowSolutionData { levelId = level.levelId };
            var service = new FlowLevelCompletionService(
                new StubProvider(new FlowCompletionResult
                {
                    status = FlowSolveStatus.Solved,
                    generatedLevel = new FlowGeneratedLevel { levelData = level, solutionData = badSolution }
                }),
                new FlowSolutionValidator(),
                new FlowDifficultyEvaluator());

            var result = Await(service.CompleteAsync(new FlowCompletionRequest { levelData = level }, null, CancellationToken.None));

            Assert.AreEqual(FlowSolveStatus.Error, result.status);
            Assert.AreEqual("InvalidCompletedSolution", result.errorCode);
            Assert.That(result.errorMessage, Does.Contain("MissingPath"));
        }

        [Test]
        public void CompleteAsync_ProviderCancellation_ReturnsCancelled()
        {
            var service = new FlowLevelCompletionService(
                new CancellingProvider(),
                new FlowSolutionValidator(),
                new FlowDifficultyEvaluator());

            var result = Await(service.CompleteAsync(new FlowCompletionRequest { levelData = MakeLevel() }, null, CancellationToken.None));

            Assert.AreEqual(FlowSolveStatus.Cancelled, result.status);
        }

        [Test]
        public void CompleteAsync_ProviderException_ReturnsError()
        {
            var service = new FlowLevelCompletionService(
                new ThrowingProvider(),
                new FlowSolutionValidator(),
                new FlowDifficultyEvaluator());

            var result = Await(service.CompleteAsync(new FlowCompletionRequest { levelData = MakeLevel() }, null, CancellationToken.None));

            Assert.AreEqual(FlowSolveStatus.Error, result.status);
            Assert.AreEqual("ProviderException", result.errorCode);
            Assert.That(result.errorMessage, Does.Contain("provider exploded"));
        }

        [Test]
        public void CompleteAsync_NullProviderResult_ReturnsError()
        {
            var service = new FlowLevelCompletionService(
                new StubProvider(null),
                new FlowSolutionValidator(),
                new FlowDifficultyEvaluator());

            var result = Await(service.CompleteAsync(new FlowCompletionRequest { levelData = MakeLevel() }, null, CancellationToken.None));

            Assert.AreEqual(FlowSolveStatus.Error, result.status);
            Assert.AreEqual("NullProviderResult", result.errorCode);
            Assert.IsNotEmpty(result.errorMessage);
        }

        private static FlowLevelData MakeLevel()
        {
            return new FlowLevelData
            {
                levelId = 811,
                width = 3,
                height = 1,
                pairs =
                {
                    new FlowPairData { colorId = 0, endpointA = new FlowPos(0, 0), endpointB = new FlowPos(2, 0) }
                }
            };
        }

        private static FlowSolutionData MakeSolution()
        {
            return new FlowSolutionData
            {
                levelId = 811,
                paths =
                {
                    new FlowPathData
                    {
                        colorId = 0,
                        cells =
                        {
                            new FlowPos(0, 0),
                            new FlowPos(1, 0),
                            new FlowPos(2, 0)
                        }
                    }
                }
            };
        }

        private static FlowCompletionResult Await(Task<FlowCompletionResult> task)
        {
            return task.GetAwaiter().GetResult();
        }

        private sealed class StubProvider : IFlowLevelCompletionProvider
        {
            private readonly FlowCompletionResult result;

            public StubProvider(FlowCompletionResult result)
            {
                this.result = result;
            }

            public string DisplayName => "Stub";

            public Task<FlowCompletionResult> CompleteAsync(FlowCompletionRequest request, IProgress<FlowCompletionProgress> progress, CancellationToken ct)
            {
                return Task.FromResult(result);
            }
        }

        private sealed class CancellingProvider : IFlowLevelCompletionProvider
        {
            public string DisplayName => "Cancelling";

            public Task<FlowCompletionResult> CompleteAsync(FlowCompletionRequest request, IProgress<FlowCompletionProgress> progress, CancellationToken ct)
            {
                throw new OperationCanceledException();
            }
        }

        private sealed class ThrowingProvider : IFlowLevelCompletionProvider
        {
            public string DisplayName => "Throwing";

            public Task<FlowCompletionResult> CompleteAsync(FlowCompletionRequest request, IProgress<FlowCompletionProgress> progress, CancellationToken ct)
            {
                throw new InvalidOperationException("provider exploded");
            }
        }
    }
}
