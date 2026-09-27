using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FlowPuzzle.Core;
using FlowPuzzle.Solving;
using FlowPuzzle.Validation;
using NUnit.Framework;

namespace FlowPuzzle.Tests.Solving
{
    [TestFixture]
    public sealed class LocalExactCompletionProviderTests
    {
        [Test]
        public void CompleteAsync_UsesFixedPrefixes_NotCurrentSolution()
        {
            var level = MakeLevel(5, 1, (0, 0, 0, 4, 0));
            var request = new FlowCompletionRequest
            {
                levelData = level,
                currentSolution = new FlowSolutionData
                {
                    levelId = level.levelId,
                    paths = new List<FlowPathData> { Path(0, new FlowPos(1, 0), new FlowPos(2, 0)) }
                },
                fixedPrefixes = new List<FlowPathData> { Path(0, new FlowPos(0, 0), new FlowPos(1, 0), new FlowPos(2, 0)) },
                timeoutMs = 5000,
                nodeBudget = 100000,
                progressIntervalNodes = 1
            };

            var result = Await(new LocalExactCompletionProvider().CompleteAsync(request, null, CancellationToken.None));

            Assert.AreEqual(FlowSolveStatus.Solved, result.status);
            Assert.IsNotNull(result.generatedLevel);
            AssertValidated(result.generatedLevel.levelData, result.generatedLevel.solutionData);
            var path = result.generatedLevel.solutionData.paths.Single(p => p.colorId == 0);
            CollectionAssert.AreEqual(request.fixedPrefixes[0].cells, path.cells.Take(request.fixedPrefixes[0].cells.Count).ToList());
        }

        [Test]
        public void CompleteAsync_DeepCopiesInputAndResultData()
        {
            var level = MakeLevel(5, 1, (0, 0, 0, 4, 0));
            var request = new FlowCompletionRequest
            {
                levelData = level,
                fixedPrefixes = new List<FlowPathData> { Path(0, new FlowPos(0, 0), new FlowPos(1, 0)) }
            };

            var result = Await(new LocalExactCompletionProvider().CompleteAsync(request, null, CancellationToken.None));

            Assert.AreEqual(FlowSolveStatus.Solved, result.status);
            Assert.AreNotSame(request.levelData, result.generatedLevel.levelData);
            Assert.AreNotSame(request.levelData.pairs[0], result.generatedLevel.levelData.pairs[0]);
            Assert.AreNotSame(request.fixedPrefixes[0], result.generatedLevel.solutionData.paths[0]);
            Assert.AreNotSame(request.fixedPrefixes[0].cells, result.generatedLevel.solutionData.paths[0].cells);

            request.levelData.width = 99;
            request.levelData.pairs[0].endpointA = new FlowPos(9, 9);
            request.fixedPrefixes[0].cells[0] = new FlowPos(8, 8);

            Assert.AreEqual(5, result.generatedLevel.levelData.width);
            Assert.AreEqual(new FlowPos(0, 0), result.generatedLevel.levelData.pairs[0].endpointA);
            Assert.AreEqual(new FlowPos(0, 0), result.generatedLevel.solutionData.paths[0].cells[0]);
        }

        [Test]
        public void CompleteAsync_ReportsProgressSynchronously()
        {
            var level = MakeLevel(4, 3, (0, 0, 0, 2, 0), (1, 1, 0, 2, 1));
            var seen = new List<FlowCompletionProgress>();
            var progress = new ImmediateProgress(p => seen.Add(new FlowCompletionProgress
            {
                phase = p.phase,
                currentColorId = p.currentColorId,
                visitedNodes = p.visitedNodes,
                elapsedMs = p.elapsedMs
            }));

            var result = Await(new LocalExactCompletionProvider().CompleteAsync(new FlowCompletionRequest
            {
                levelData = level,
                progressIntervalNodes = 1
            }, progress, CancellationToken.None));

            Assert.AreEqual(FlowSolveStatus.Solved, result.status);
            Assert.IsNotEmpty(seen);
            for (var i = 1; i < seen.Count; i++)
                Assert.GreaterOrEqual(seen[i].visitedNodes, seen[i - 1].visitedNodes);
        }

        [Test]
        public void CompleteAsync_PreCancelled_ReturnsCancelledResult()
        {
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            var result = Await(new LocalExactCompletionProvider().CompleteAsync(new FlowCompletionRequest
            {
                levelData = MakeLevel(3, 1, (0, 0, 0, 2, 0))
            }, null, cts.Token));

            Assert.AreEqual(FlowSolveStatus.Cancelled, result.status);
            Assert.IsNull(result.generatedLevel);
        }

        [Test]
        public void CompleteAsync_NullRequest_ReturnsInvalidInput()
        {
            var result = Await(new LocalExactCompletionProvider().CompleteAsync(null, null, CancellationToken.None));
            Assert.AreEqual(FlowSolveStatus.InvalidInput, result.status);
            Assert.IsNull(result.generatedLevel);
        }

        private static FlowLevelData MakeLevel(int width, int height, params (int id, int ax, int ay, int bx, int by)[] pairs)
        {
            var level = new FlowLevelData { levelId = 810, width = width, height = height };
            foreach (var (id, ax, ay, bx, by) in pairs)
                level.pairs.Add(new FlowPairData { colorId = id, endpointA = new FlowPos(ax, ay), endpointB = new FlowPos(bx, by) });
            return level;
        }

        private static FlowPathData Path(int colorId, params FlowPos[] cells)
        {
            return new FlowPathData { colorId = colorId, cells = cells.ToList() };
        }

        private static void AssertValidated(FlowLevelData level, FlowSolutionData solution)
        {
            var validation = new FlowSolutionValidator().Validate(level, solution);
            Assert.IsTrue(validation.isValid, $"{validation.errorCode}: {validation.errorMessage}");
        }

        private static FlowCompletionResult Await(Task<FlowCompletionResult> task)
        {
            return task.GetAwaiter().GetResult();
        }

        private sealed class ImmediateProgress : IProgress<FlowCompletionProgress>
        {
            private readonly Action<FlowCompletionProgress> onReport;

            public ImmediateProgress(Action<FlowCompletionProgress> onReport)
            {
                this.onReport = onReport;
            }

            public void Report(FlowCompletionProgress value)
            {
                onReport(value);
            }
        }
    }
}
