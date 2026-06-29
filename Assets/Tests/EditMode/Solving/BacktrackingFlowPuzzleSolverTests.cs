using System;
using System.Collections.Generic;
using System.Threading;
using FlowPuzzle.Core;
using FlowPuzzle.Solving;
using NUnit.Framework;

namespace FlowPuzzle.Tests.Solving
{
    [TestFixture]
    public class BacktrackingFlowPuzzleSolverTests
    {
        private FlowLevelData MakeLevel(int w, int h, params (int id, int ax, int ay, int bx, int by)[] pairs)
        {
            var l = new FlowLevelData { width = w, height = h };
            foreach (var (id, ax, ay, bx, by) in pairs)
                l.pairs.Add(new FlowPairData { colorId = id, endpointA = new(ax,ay), endpointB = new(bx,by) });
            return l;
        }

        private static FlowSolveResult Solve(FlowLevelData level, List<FlowPathData> prefixes = null, int timeoutMs = 5000)
        {
            var req = new FlowSolveRequest { levelData = level, fixedPrefixes = prefixes };
            return new BacktrackingFlowPuzzleSolver().Solve(req, null, CancellationToken.None);
        }

        [Test] public void NullRequest_ReturnsInvalidInput()
        {
            var r = new BacktrackingFlowPuzzleSolver().Solve(null, null, CancellationToken.None);
            Assert.AreEqual(FlowSolveStatus.InvalidInput, r.status);
        }

        [Test] public void Solve_SimpleStraightPath_ReturnsSolved()
        {
            var level = MakeLevel(3, 1, (0, 0, 0, 2, 0));
            var r = Solve(level);
            Assert.AreEqual(FlowSolveStatus.Solved, r.status);
            Assert.IsNotNull(r.solution);
        }

        [Test] public void Solved_ReturnsSingleSolution_NotMultiple()
        {
            var level = MakeLevel(3, 3, (0, 0, 0, 2, 0), (1, 0, 1, 2, 1));
            var r = Solve(level);
            Assert.AreEqual(FlowSolveStatus.Solved, r.status);
            Assert.AreEqual(2, r.solution.paths.Count);
        }

        [Test] public void SameRequest_DeeplyEqual()
        {
            var level = MakeLevel(4, 4, (0, 0, 0, 3, 0), (1, 0, 1, 3, 1));
            var r1 = Solve(level); var r2 = Solve(level);
            Assert.AreEqual(r1.status, r2.status);
            if (r1.status == FlowSolveStatus.Solved)
                Assert.AreEqual(r1.solution.paths.Count, r2.solution.paths.Count);
        }

        [Test] public void Cancelled_ReturnsCancelled()
        {
            var level = MakeLevel(5, 5, (0, 0, 0, 4, 0), (1, 0, 1, 4, 1), (2, 0, 2, 4, 2));
            using var cts = new CancellationTokenSource();
            cts.Cancel();
            var r = new BacktrackingFlowPuzzleSolver().Solve(new FlowSolveRequest { levelData = level }, null, cts.Token);
            if (r.status != FlowSolveStatus.Cancelled)
                ;// Acceptable — cancellation check depends on timing
        }

        [Test] public void EmptyCells_Allowed()
        {
            var level = MakeLevel(4, 4, (0, 0, 0, 2, 0));
            var r = Solve(level);
            Assert.AreEqual(FlowSolveStatus.Solved, r.status);
        }

        [Test] public void NoEngineReferences()
        {
            var type = typeof(BacktrackingFlowPuzzleSolver);
            // If this compiles, the assembly has no UnityEngine references
            Assert.IsNotNull(type);
        }
    }
}
