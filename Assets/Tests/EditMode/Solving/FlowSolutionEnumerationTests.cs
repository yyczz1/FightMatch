using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FlowPuzzle.Core;
using FlowPuzzle.Solving;
using NUnit.Framework;

namespace FlowPuzzle.Tests.Solving
{
    [TestFixture]
    public sealed class FlowSolutionEnumerationTests
    {
        [Test]
        public void EnumerateSolutions_ReturnsEveryLegalCandidateInDeterministicOrder()
        {
            var first = Enumerate(CreateRequest());
            var second = Enumerate(CreateRequest());

            Assert.Greater(first.paths.Count, 1);
            CollectionAssert.AreEqual(first.paths, second.paths);
            Assert.AreEqual(FlowSolveStatus.Solved, first.result.status);
            Assert.IsTrue(first.result.searchExhausted);
            Assert.AreEqual(first.paths.Count, first.result.candidateCount);
        }

        [Test]
        public void EnumerateSolutions_AllCandidatesRespectFixedPrefix()
        {
            var request = CreateRequest();
            request.fixedPrefixes = new List<FlowPathData>
            {
                new FlowPathData
                {
                    colorId = 0,
                    cells = new List<FlowPos> { new FlowPos(0, 0), new FlowPos(0, 1) }
                }
            };

            var run = Enumerate(request);

            Assert.Greater(run.paths.Count, 0);
            foreach (var path in run.paths)
            {
                Assert.That(path, Does.StartWith("0,0|0,1"));
            }
        }

        [Test]
        public void EnumerateSolutions_NodeBudgetAfterCandidate_ReturnsTimeoutWithBestSoFarAvailable()
        {
            var request = CreateRequest();
            request.nodeBudget = 4;
            var candidates = new List<FlowSolutionData>();

            var result = new BacktrackingFlowPuzzleSolver().EnumerateSolutions(
                request,
                candidate => candidates.Add(candidate),
                null,
                CancellationToken.None);

            Assert.AreEqual(FlowSolveStatus.Timeout, result.status);
            Assert.IsFalse(result.searchExhausted);
            Assert.Greater(candidates.Count, 0);
            Assert.AreEqual(candidates.Count, result.candidateCount);
        }

        private static (FlowSolutionEnumerationResult result, List<string> paths) Enumerate(FlowSolveRequest request)
        {
            var paths = new List<string>();
            var result = new BacktrackingFlowPuzzleSolver().EnumerateSolutions(
                request,
                candidate => paths.Add(Canonical(candidate)),
                null,
                CancellationToken.None);
            return (result, paths);
        }

        private static FlowSolveRequest CreateRequest()
        {
            var level = new FlowLevelData { levelId = 1, width = 3, height = 2 };
            level.pairs.Add(new FlowPairData
            {
                colorId = 0,
                endpointA = new FlowPos(0, 0),
                endpointB = new FlowPos(2, 0)
            });
            return new FlowSolveRequest
            {
                levelData = level,
                nodeBudget = 10000,
                timeoutMs = 10000,
                progressIntervalNodes = 1000
            };
        }

        private static string Canonical(FlowSolutionData solution)
        {
            return string.Join("/", solution.paths
                .OrderBy(path => path.colorId)
                .Select(path => string.Join("|", path.cells.Select(cell => $"{cell.x},{cell.y}"))));
        }
    }
}
