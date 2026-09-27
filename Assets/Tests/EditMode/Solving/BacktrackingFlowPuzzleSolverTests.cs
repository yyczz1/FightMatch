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
    public class BacktrackingFlowPuzzleSolverTests
    {
        private static FlowLevelData MakeLevel(int width, int height, params (int id, int ax, int ay, int bx, int by)[] pairs)
        {
            var level = new FlowLevelData { levelId = 701, width = width, height = height };
            foreach (var (id, ax, ay, bx, by) in pairs)
                level.pairs.Add(new FlowPairData { colorId = id, endpointA = new FlowPos(ax, ay), endpointB = new FlowPos(bx, by) });
            return level;
        }

        private static FlowPathData Prefix(int colorId, params FlowPos[] cells)
        {
            return new FlowPathData { colorId = colorId, cells = cells.ToList() };
        }

        private static FlowSolveResult Solve(FlowLevelData level, List<FlowPathData> prefixes = null, long budget = 1000000, int timeoutMs = 5000, IProgress<FlowSolveProgress> progress = null, CancellationToken token = default)
        {
            return new BacktrackingFlowPuzzleSolver().Solve(new FlowSolveRequest
            {
                levelData = level,
                fixedPrefixes = prefixes,
                nodeBudget = budget,
                timeoutMs = timeoutMs,
                progressIntervalNodes = 1
            }, progress, token);
        }

        private static void AssertValidated(FlowLevelData level, FlowSolveResult result)
        {
            Assert.AreEqual(FlowSolveStatus.Solved, result.status);
            Assert.IsNotNull(result.solution);
            var validation = new FlowSolutionValidator().Validate(level, result.solution);
            Assert.IsTrue(validation.isValid, $"{validation.errorCode}: {validation.errorMessage}");
        }

        private static FlowPathData PathFor(FlowSolveResult result, int colorId)
        {
            return result.solution.paths.First(p => p.colorId == colorId);
        }

        private static string PathKey(FlowPathData path)
        {
            return string.Join(";", path.cells.Select(c => $"{c.x},{c.y}"));
        }

        private static FlowSolveRequest MakeRequest(FlowLevelData level, List<FlowPathData> prefixes = null)
        {
            return new FlowSolveRequest
            {
                levelData = level,
                fixedPrefixes = prefixes,
                nodeBudget = 12345,
                timeoutMs = 4321,
                progressIntervalNodes = 7
            };
        }

        private static string Snapshot(FlowSolveRequest request)
        {
            var level = request.levelData;
            var pairs = level == null ? "" : string.Join("|", level.pairs.Select(p => $"{p.colorId}:{p.endpointA.x},{p.endpointA.y}->{p.endpointB.x},{p.endpointB.y}"));
            var prefixes = request.fixedPrefixes == null ? "" : string.Join("|", request.fixedPrefixes.Select(p => $"{p.colorId}:{PathKey(p)}"));
            return $"{level?.levelId}:{level?.width}x{level?.height}:{pairs}:{prefixes}:{request.nodeBudget}:{request.timeoutMs}:{request.progressIntervalNodes}";
        }

        [Test]
        public void Solve_SimpleStraightPath_ReturnsValidatedSolution()
        {
            var level = MakeLevel(3, 1, (0, 0, 0, 2, 0));
            var result = Solve(level);
            AssertValidated(level, result);
            CollectionAssert.AreEqual(new[] { new FlowPos(0, 0), new FlowPos(1, 0), new FlowPos(2, 0) }, PathFor(result, 0).cells);
        }

        [Test]
        public void Solve_FirstCandidateBlocksLaterColor_BacktracksToLaterPath()
        {
            var level = MakeLevel(4, 3, (0, 0, 0, 2, 0), (1, 1, 0, 2, 1));
            var result = Solve(level);
            AssertValidated(level, result);
            CollectionAssert.DoesNotContain(PathFor(result, 0).cells, new FlowPos(1, 1), "Solver must backtrack away from the first valid candidate that blocks color 1.");
            CollectionAssert.AreEqual(new[] { new FlowPos(1, 0), new FlowPos(1, 1), new FlowPos(2, 1) }, PathFor(result, 1).cells);
        }

        [Test]
        public void Solve_ExhaustedSearch_ReturnsNoSolution()
        {
            var level = MakeLevel(3, 3, (0, 0, 0, 2, 2), (1, 0, 2, 2, 0), (2, 1, 0, 1, 2));
            var result = Solve(level);
            Assert.AreEqual(FlowSolveStatus.NoSolution, result.status);
            Assert.IsNull(result.solution);
        }

        [Test]
        public void Solve_DoesNotRequireFullBoardOccupancy()
        {
            var level = MakeLevel(5, 5, (0, 0, 0, 2, 0));
            var result = Solve(level);
            AssertValidated(level, result);
            Assert.Less(PathFor(result, 0).cells.Count, level.width * level.height);
        }

        [Test]
        public void FixedPrefix_FromEndpointA_ContinuesToEndpointB()
        {
            var level = MakeLevel(5, 1, (0, 0, 0, 4, 0));
            var prefix = Prefix(0, new FlowPos(0, 0), new FlowPos(1, 0), new FlowPos(2, 0));
            var result = Solve(level, new List<FlowPathData> { prefix });
            AssertValidated(level, result);
            CollectionAssert.AreEqual(prefix.cells, PathFor(result, 0).cells.Take(prefix.cells.Count).ToList());
        }

        [Test]
        public void FixedPrefix_FromEndpointB_ContinuesToEndpointA()
        {
            var level = MakeLevel(5, 1, (0, 0, 0, 4, 0));
            var prefix = Prefix(0, new FlowPos(4, 0), new FlowPos(3, 0), new FlowPos(2, 0));
            var result = Solve(level, new List<FlowPathData> { prefix });
            AssertValidated(level, result);
            CollectionAssert.AreEqual(prefix.cells, PathFor(result, 0).cells.Take(prefix.cells.Count).ToList());
            Assert.AreEqual(new FlowPos(0, 0), PathFor(result, 0).cells.Last());
        }

        [Test]
        public void FixedPrefix_InvalidCases_ReturnInvalidInput()
        {
            var level = MakeLevel(4, 4, (0, 0, 0, 3, 0), (1, 0, 1, 3, 1));
            var cases = new[]
            {
                new List<FlowPathData> { Prefix(9, new FlowPos(0, 0), new FlowPos(1, 0)) },
                new List<FlowPathData> { new FlowPathData { colorId = 0, cells = null } },
                new List<FlowPathData> { Prefix(0, new FlowPos(0, 0)) },
                new List<FlowPathData> { Prefix(0, new FlowPos(0, 0), new FlowPos(9, 0)) },
                new List<FlowPathData> { Prefix(0, new FlowPos(0, 0), new FlowPos(2, 0)) },
                new List<FlowPathData> { Prefix(0, new FlowPos(0, 0), new FlowPos(1, 0), new FlowPos(1, 0)) },
                new List<FlowPathData> { Prefix(0, new FlowPos(1, 0), new FlowPos(2, 0)) },
                new List<FlowPathData> { Prefix(0, new FlowPos(0, 0), new FlowPos(3, 0), new FlowPos(2, 0)) },
                new List<FlowPathData> { Prefix(0, new FlowPos(0, 0), new FlowPos(0, 1)) },
                new List<FlowPathData> { Prefix(0, new FlowPos(0, 0), new FlowPos(1, 0)), Prefix(1, new FlowPos(0, 1), new FlowPos(1, 0)) }
            };

            foreach (var prefixes in cases)
                Assert.AreEqual(FlowSolveStatus.InvalidInput, Solve(level, prefixes).status);
        }

        [Test]
        public void BudgetExceeded_ReturnsTimeout()
        {
            var level = MakeLevel(5, 5, (0, 0, 0, 4, 4), (1, 0, 4, 4, 0));
            var result = Solve(level, budget: 1, timeoutMs: 10000);
            Assert.AreEqual(FlowSolveStatus.Timeout, result.status);
            Assert.GreaterOrEqual(result.visitedNodes, 1);
        }

        [Test]
        public void PreCancelled_ReturnsCancelled()
        {
            var level = MakeLevel(5, 5, (0, 0, 0, 4, 4));
            using var cts = new CancellationTokenSource();
            cts.Cancel();
            Assert.AreEqual(FlowSolveStatus.Cancelled, Solve(level, token: cts.Token).status);
        }

        [Test]
        public void MidSearchCancellation_ReturnsCancelled()
        {
            var level = MakeLevel(6, 6, (0, 0, 0, 5, 5), (1, 0, 5, 5, 0));
            using var cts = new CancellationTokenSource();
            var progress = new ImmediateProgress(p => { if (p.visitedNodes > 5) cts.Cancel(); });
            Assert.AreEqual(FlowSolveStatus.Cancelled, Solve(level, budget: 1000000, timeoutMs: 10000, progress: progress, token: cts.Token).status);
        }

        [Test]
        public void Progress_IsReportedWithNondecreasingNodesAndTime()
        {
            var level = MakeLevel(4, 4, (0, 0, 0, 3, 3), (1, 0, 3, 3, 0));
            var seen = new List<FlowSolveProgress>();
            var progress = new ImmediateProgress(p => seen.Add(new FlowSolveProgress { phase = p.phase, currentColorId = p.currentColorId, visitedNodes = p.visitedNodes, elapsedMs = p.elapsedMs }));
            var result = Solve(level, progress: progress);
            Assert.AreNotEqual(FlowSolveStatus.InvalidInput, result.status);
            Assert.IsNotEmpty(seen);
            for (var i = 1; i < seen.Count; i++)
            {
                Assert.GreaterOrEqual(seen[i].visitedNodes, seen[i - 1].visitedNodes);
                Assert.GreaterOrEqual(seen[i].elapsedMs, seen[i - 1].elapsedMs);
            }
        }

        [Test]
        public void SameRequest_DeeplyEqualAndDoesNotMutateRequest()
        {
            var level = MakeLevel(4, 4, (0, 0, 0, 3, 0), (1, 0, 1, 3, 1));
            var request = MakeRequest(level, new List<FlowPathData> { Prefix(0, new FlowPos(0, 0), new FlowPos(1, 0)) });
            var before = Snapshot(request);
            var solver = new BacktrackingFlowPuzzleSolver();
            var a = solver.Solve(request, null, CancellationToken.None);
            var b = solver.Solve(request, null, CancellationToken.None);
            Assert.AreEqual(before, Snapshot(request));
            Assert.AreEqual(a.status, b.status);
            Assert.AreEqual(a.solution?.paths.Count, b.solution?.paths.Count);
            if (a.status == FlowSolveStatus.Solved)
                for (var i = 0; i < a.solution.paths.Count; i++)
                    Assert.AreEqual(PathKey(a.solution.paths[i]), PathKey(b.solution.paths[i]));
        }

        [Test]
        public void SameSolverInstance_ConcurrentCalls_AreIsolated()
        {
            var solver = new BacktrackingFlowPuzzleSolver();
            var a = MakeLevel(4, 4, (0, 0, 0, 3, 0));
            var b = MakeLevel(4, 4, (0, 0, 1, 3, 1));
            var results = Task.WhenAll(
                Task.Run(() => solver.Solve(new FlowSolveRequest { levelData = a }, null, CancellationToken.None)),
                Task.Run(() => solver.Solve(new FlowSolveRequest { levelData = b }, null, CancellationToken.None))).Result;
            AssertValidated(a, results[0]);
            AssertValidated(b, results[1]);
        }

        [Test]
        public void SolvedPaths_AreInTraversalOrder_NotBoardScanOrder()
        {
            var level = MakeLevel(3, 3, (0, 0, 2, 2, 0));
            var prefix = Prefix(0, new FlowPos(0, 2), new FlowPos(1, 2), new FlowPos(1, 1));
            var result = Solve(level, new List<FlowPathData> { prefix });
            AssertValidated(level, result);
            var path = PathFor(result, 0);
            CollectionAssert.AreEqual(prefix.cells, path.cells.Take(prefix.cells.Count).ToList());
            for (var i = 1; i < path.cells.Count; i++)
                Assert.IsTrue(FlowPathUtility.AreAdjacent(path.cells[i - 1], path.cells[i]));
        }

        [Test]
        public void NoUnityEngineReferencesInSolvingCore()
        {
            var references = typeof(BacktrackingFlowPuzzleSolver).Assembly.GetReferencedAssemblies().Select(a => a.Name).ToList();
            CollectionAssert.DoesNotContain(references, "UnityEngine");
            CollectionAssert.DoesNotContain(references, "UnityEditor");
        }

        private sealed class ImmediateProgress : IProgress<FlowSolveProgress>
        {
            private readonly Action<FlowSolveProgress> onReport;

            public ImmediateProgress(Action<FlowSolveProgress> onReport)
            {
                this.onReport = onReport;
            }

            public void Report(FlowSolveProgress value)
            {
                onReport(value);
            }
        }
    }
}
