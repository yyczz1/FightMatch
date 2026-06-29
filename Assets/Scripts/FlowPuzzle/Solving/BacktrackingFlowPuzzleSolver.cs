using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using FlowPuzzle.Core;

namespace FlowPuzzle.Solving
{
    public sealed class BacktrackingFlowPuzzleSolver : IFlowPuzzleSolver
    {
        public FlowSolveResult Solve(FlowSolveRequest request, IProgress<FlowSolveProgress> progress, CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();
            var ctx = new SolveContext
            {
                nodeBudget = 10_000_000L, timeoutMs = 10000, sw = sw, ct = cancellationToken,
                progress = progress, visitedNodes = 0
            };

            if (request == null || request.levelData == null) return ctx.Result(FlowSolveStatus.InvalidInput);
            var level = request.levelData;
            if (level.width <= 0 || level.height <= 0 || level.pairs == null || level.pairs.Count == 0)
                return ctx.Result(FlowSolveStatus.InvalidInput);
            if (!ValidateInput(level)) return ctx.Result(FlowSolveStatus.InvalidInput);

            var board = new FlowBoard(level.width, level.height);
            var prefixByColor = new Dictionary<int, List<FlowPos>>();
            if (request.fixedPrefixes != null)
                foreach (var fp in request.fixedPrefixes)
                    if (fp.cells != null && fp.cells.Count >= 2)
                    {
                        foreach (var c in fp.cells) board.Set(c, fp.colorId);
                        prefixByColor[fp.colorId] = new List<FlowPos>(fp.cells);
                    }

            // Color ordering: prefix first, then reachable area, Manhattan, colorId
            var colorOrder = level.pairs.Select(p => p.colorId).OrderBy(id =>
            {
                var hasPf = prefixByColor.ContainsKey(id) ? 0 : 1;
                var pair = level.pairs.First(p => p.colorId == id);
                var dist = Math.Abs(pair.endpointA.x - pair.endpointB.x)
                         + Math.Abs(pair.endpointA.y - pair.endpointB.y);
                return (hasPf, dist, id);
            }).ToList();

            ctx.Report("search");

            var result = Search(board, level, colorOrder, 0, prefixByColor, ctx);
            result.visitedNodes = ctx.visitedNodes;
            result.elapsedMs = sw.ElapsedMilliseconds;
            return result;
        }

        private FlowSolveResult Search(FlowBoard board, FlowLevelData level, List<int> order, int idx,
            Dictionary<int, List<FlowPos>> prefixes, SolveContext ctx)
        {
            if (ctx.ct.IsCancellationRequested) return ctx.Result(FlowSolveStatus.Cancelled);
            if (ctx.sw.ElapsedMilliseconds > ctx.timeoutMs) return ctx.Result(FlowSolveStatus.Timeout);
            if (ctx.visitedNodes >= ctx.nodeBudget) return ctx.Result(FlowSolveStatus.Timeout);

            if (idx >= order.Count)
                return BuildSolution(board, level, order, prefixes, ctx);

            int colorId = order[idx];
            var pair = level.pairs.First(p => p.colorId == colorId);
            FlowPos start, end;
            bool hasPrefix = prefixes.TryGetValue(colorId, out var pfx) && pfx.Count > 0;
            if (hasPrefix) { start = pfx.Last(); end = pair.endpointB; }
            else { start = pair.endpointA; end = pair.endpointB; }

            // BFS precheck
            if (!BfsReachable(board, start, end, colorId, prefixes)) return ctx.Result(FlowSolveStatus.NoSolution);

            // Enumerate all simple paths from start to end
            var allPaths = new List<List<FlowPos>>();
            var initPath = new List<FlowPos> { start };
            var initSet = new HashSet<FlowPos> { start };
            DfsPaths(board, initPath, initSet, end, colorId, allPaths, ctx);

            if (allPaths.Count == 0)
                return ctx.Result(FlowSolveStatus.NoSolution);

            ctx.visitedNodes += (long)allPaths.Count;

            foreach (var path in allPaths)
            {
                // Commit
                foreach (var c in path) board.Set(c, colorId);
                var result = Search(board, level, order, idx + 1, prefixes, ctx);
                if (result.status == FlowSolveStatus.Solved) return result;
                // Backtrack
                foreach (var c in path)
                {
                    if (hasPrefix && pfx.Contains(c)) continue;
                    board.Clear(c);
                }
            }
            return ctx.Result(FlowSolveStatus.NoSolution);
        }

        private void DfsPaths(FlowBoard board, List<FlowPos> path, HashSet<FlowPos> pathSet,
            FlowPos target, int colorId, List<List<FlowPos>> results, SolveContext ctx)
        {
            var cur = path.Last();
            if (cur.Equals(target) && path.Count >= 2) { results.Add(new List<FlowPos>(path)); return; }
            if (path.Count > board.Width * board.Height) return;
            var neighbors = board.GetNeighbors(cur);
            foreach (var n in neighbors)
            {
                if (pathSet.Contains(n)) continue;
                if (!board.IsEmpty(n) && !n.Equals(target)) continue;
                path.Add(n); pathSet.Add(n);
                DfsPaths(board, path, pathSet, target, colorId, results, ctx);
                path.RemoveAt(path.Count - 1); pathSet.Remove(n);
                if (results.Count >= 1) return; // first valid path only
            }
        }

        private static bool BfsReachable(FlowBoard board, FlowPos start, FlowPos end,
            int colorId, Dictionary<int, List<FlowPos>> prefixes)
        {
            var visited = new HashSet<FlowPos>();
            var q = new Queue<FlowPos>();
            q.Enqueue(start); visited.Add(start);
            bool hasPrefix = prefixes.TryGetValue(colorId, out var pfx);
            while (q.Count > 0)
            {
                var cur = q.Dequeue();
                foreach (var n in board.GetNeighbors(cur))
                {
                    if (visited.Contains(n)) continue;
                    if (n.Equals(end)) return true;
                    if (!board.IsEmpty(n) && !(hasPrefix && pfx.Contains(n))) continue;
                    visited.Add(n); q.Enqueue(n);
                }
            }
            return false;
        }

        private FlowSolveResult BuildSolution(FlowBoard board, FlowLevelData level, List<int> order,
            Dictionary<int, List<FlowPos>> prefixes, SolveContext ctx)
        {
            var sol = new FlowSolutionData { levelId = level.levelId };
            foreach (var cid in order)
            {
                var cells = new List<FlowPos>();
                for (int x = 0; x < level.width; x++)
                for (int y = 0; y < level.height; y++)
                {
                    var p = new FlowPos(x, y);
                    if (!board.IsEmpty(p) && board.Get(p) == cid) cells.Add(p);
                }
                if (cells.Count >= 2) sol.paths.Add(new FlowPathData { colorId = cid, cells = cells });
            }
            return new FlowSolveResult { status = FlowSolveStatus.Solved, solution = sol, visitedNodes = ctx.visitedNodes };
        }

        private static bool ValidateInput(FlowLevelData level)
        {
            var ids = new HashSet<int>();
            foreach (var p in level.pairs)
            {
                if (!ids.Add(p.colorId)) return false;
                if (p.endpointA.x < 0 || p.endpointA.x >= level.width) return false;
                if (p.endpointA.y < 0 || p.endpointA.y >= level.height) return false;
                if (p.endpointB.x < 0 || p.endpointB.x >= level.width) return false;
                if (p.endpointB.y < 0 || p.endpointB.y >= level.height) return false;
            }
            var pts = new HashSet<FlowPos>();
            foreach (var p in level.pairs)
            { if (!pts.Add(p.endpointA)) return false; if (!pts.Add(p.endpointB)) return false; }
            return true;
        }

        private sealed class SolveContext
        {
            public long nodeBudget, timeoutMs, visitedNodes;
            public Stopwatch sw;
            public CancellationToken ct;
            public IProgress<FlowSolveProgress> progress;
            public FlowSolveResult Result(FlowSolveStatus s) => new() { status = s, visitedNodes = visitedNodes, elapsedMs = sw.ElapsedMilliseconds };
            public void Report(string phase, int cid = -1) => progress?.Report(new FlowSolveProgress { phase = phase, visitedNodes = visitedNodes, elapsedMs = sw.ElapsedMilliseconds, currentColorId = cid });
        }
    }
}
