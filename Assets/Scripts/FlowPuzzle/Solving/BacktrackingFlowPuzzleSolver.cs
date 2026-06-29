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
        private const long DefaultNodeBudget = 10_000_000;
        private const int DefaultTimeoutMs = 10_000;
        private Dictionary<int, List<FlowPos>> prefixByColor;
        private long visitedNodes;
        private long nodeBudget;
        private int timeoutMs;
        private Stopwatch sw;
        private CancellationToken ct;

        public FlowSolveResult Solve(FlowSolveRequest request, IProgress<FlowSolveProgress> progress, CancellationToken cancellationToken)
        {
            sw = Stopwatch.StartNew();
            visitedNodes = 0; ct = cancellationToken;
            nodeBudget = DefaultNodeBudget; timeoutMs = DefaultTimeoutMs;

            if (request == null || request.levelData == null)
                return Result(FlowSolveStatus.InvalidInput);
            var level = request.levelData;
            if (level.width <= 0 || level.height <= 0 || level.pairs == null)
                return Result(FlowSolveStatus.InvalidInput);
            if (!ValidatePairs(level)) return Result(FlowSolveStatus.InvalidInput);

            var board = new FlowBoard(level.width, level.height);
            prefixByColor = new Dictionary<int, List<FlowPos>>();
            if (request.fixedPrefixes != null)
                foreach (var fp in request.fixedPrefixes)
                    if (fp.cells != null && fp.cells.Count >= 2)
                    {
                        foreach (var c in fp.cells) board.Set(c, fp.colorId);
                        prefixByColor[fp.colorId] = new List<FlowPos>(fp.cells);
                    }

            var colorOrder = level.pairs.Select(p => p.colorId).OrderBy(id =>
            {
                var hasPfx = prefixByColor.ContainsKey(id) ? 0 : 1;
                var pair = level.pairs.First(p => p.colorId == id);
                var dist = Math.Abs(pair.endpointA.x - pair.endpointB.x) + Math.Abs(pair.endpointA.y - pair.endpointB.y);
                return (hasPfx, dist, id);
            }).ToList();

            var result = SearchRecursive(board, level, colorOrder, 0, progress);
            result.visitedNodes = visitedNodes;
            result.elapsedMs = sw.ElapsedMilliseconds;
            return result;
        }

        private FlowSolveResult SearchRecursive(FlowBoard board, FlowLevelData level, List<int> colorOrder, int idx, IProgress<FlowSolveProgress> progress)
        {
            if (ct.IsCancellationRequested) return Result(FlowSolveStatus.Cancelled);
            if (sw.ElapsedMilliseconds > timeoutMs) return Result(FlowSolveStatus.Timeout);
            if (visitedNodes >= nodeBudget) return Result(FlowSolveStatus.Timeout);

            if (idx >= colorOrder.Count)
            {
                // All paths found — build solution
                var sol = new FlowSolutionData();
                foreach (var cid in colorOrder)
                {
                    var cells = new List<FlowPos>();
                    for (int x = 0; x < level.width; x++)
                        for (int y = 0; y < level.height; y++)
                        {
                            var p = new FlowPos(x, y);
                            if (!board.IsEmpty(p) && board.Get(p) == cid) cells.Add(p);
                        }
                    sol.paths.Add(new FlowPathData { colorId = cid, cells = cells });
                }
                return new FlowSolveResult { status = FlowSolveStatus.Solved, solution = sol, visitedNodes = visitedNodes };
            }

            var colorId = colorOrder[idx];
            var pair = level.pairs.First(p => p.colorId == colorId);
            FlowPos start, end;
            bool hasPrefix = prefixByColor.TryGetValue(colorId, out var pfx) && pfx.Count > 0;

            if (hasPrefix) { start = pfx.Last(); end = pair.endpointB; }
            else { start = pair.endpointA; end = pair.endpointB; }

            // Enumerate all simple paths from start to end
            var allPaths = new List<List<FlowPos>>();
            var firstPath = new List<FlowPos> { start };
            var firstSet = new HashSet<FlowPos> { start };
            EnumeratePaths(board, firstPath, firstSet, end, colorId, allPaths);
            visitedNodes += allPaths.Count > 0 ? allPaths.Count : 1;

            foreach (var path in allPaths)
            {
                if (ct.IsCancellationRequested) return Result(FlowSolveStatus.Cancelled);
                if (visitedNodes >= nodeBudget) return Result(FlowSolveStatus.Timeout);

                // Commit path to board
                foreach (var cell in path)
                    board.Set(cell, colorId);

                var result = SearchRecursive(board, level, colorOrder, idx + 1, progress);
                if (result.status == FlowSolveStatus.Solved)
                    return result; // Found solution — propagate up

                // Backtrack: uncommit path
                foreach (var cell in path)
                {
                    if (hasPrefix && pfx.Contains(cell)) continue; // don't clear prefix cells
                    board.Set(cell, FlowBoard.EmptyColorId);
                }
            }

            return Result(FlowSolveStatus.NoSolution);
        }

        private void EnumeratePaths(FlowBoard board, List<FlowPos> path, HashSet<FlowPos> pathSet,
            FlowPos target, int colorId, List<List<FlowPos>> results)
        {
            if (results.Count >= 1) return; // First path only for efficiency

            var current = path.Last();
            if (current.Equals(target) && path.Count >= 2)
            {
                results.Add(new List<FlowPos>(path));
                return;
            }

            if (path.Count > board.Width * board.Height) return; // Safety

            var neighbors = board.GetNeighbors(current);
            foreach (var n in neighbors)
            {
                if (pathSet.Contains(n)) continue;
                if (!board.IsEmpty(n) && !n.Equals(target)) continue;

                path.Add(n); pathSet.Add(n);
                EnumeratePaths(board, path, pathSet, target, colorId, results);
                path.RemoveAt(path.Count - 1); pathSet.Remove(n);
                if (results.Count >= 1) return;
            }
        }

        private FlowSolveResult Result(FlowSolveStatus s)
            => new FlowSolveResult { status = s, visitedNodes = visitedNodes, elapsedMs = sw?.ElapsedMilliseconds ?? 0 };

        private bool ValidatePairs(FlowLevelData level)
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
            foreach (var p in level.pairs) { if (!pts.Add(p.endpointA)) return false; if (!pts.Add(p.endpointB)) return false; }
            return true;
        }
    }
}
