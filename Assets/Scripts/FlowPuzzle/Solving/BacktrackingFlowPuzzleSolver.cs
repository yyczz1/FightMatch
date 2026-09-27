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
            var context = new SolveContext(
                request?.nodeBudget ?? FlowSolveRequest.DefaultNodeBudget,
                request?.timeoutMs ?? FlowSolveRequest.DefaultTimeoutMs,
                request?.progressIntervalNodes ?? FlowSolveRequest.DefaultProgressIntervalNodes,
                progress,
                cancellationToken);

            if (context.IsStopped())
                return context.Result(context.StopStatus.Value);

            if (!TryBuildProblem(request, out var problem))
                return context.Result(FlowSolveStatus.InvalidInput);

            context.Report("start");

            var solvedPaths = new Dictionary<int, List<FlowPos>>();
            var status = Search(problem, context, solvedPaths, 0);
            if (status != FlowSolveStatus.Solved)
                return context.Result(status);

            var solution = BuildSolution(problem.Level, solvedPaths);
            return context.Result(FlowSolveStatus.Solved, solution);
        }

        public FlowSolutionEnumerationResult EnumerateSolutions(
            FlowSolveRequest request,
            Action<FlowSolutionData> onSolution,
            IProgress<FlowSolveProgress> progress,
            CancellationToken cancellationToken)
        {
            var context = new SolveContext(
                request?.nodeBudget ?? FlowSolveRequest.DefaultNodeBudget,
                request?.timeoutMs ?? FlowSolveRequest.DefaultTimeoutMs,
                request?.progressIntervalNodes ?? FlowSolveRequest.DefaultProgressIntervalNodes,
                progress,
                cancellationToken);

            if (context.IsStopped())
                return context.EnumerationResult(context.StopStatus.Value, false);
            if (!TryBuildProblem(request, out var problem))
                return context.EnumerationResult(FlowSolveStatus.InvalidInput, false);

            context.Report("start");
            var solvedPaths = new Dictionary<int, List<FlowPos>>();
            var searchStatus = EnumerateSearch(problem, context, solvedPaths, 0, onSolution);
            if (searchStatus != FlowSolveStatus.NoSolution)
                return context.EnumerationResult(searchStatus, false);

            return context.EnumerationResult(
                context.CandidateCount > 0 ? FlowSolveStatus.Solved : FlowSolveStatus.NoSolution,
                true);
        }

        private static FlowSolveStatus Search(Problem problem, SolveContext context, Dictionary<int, List<FlowPos>> solvedPaths, int colorIndex)
        {
            if (context.IsStopped())
                return context.StopStatus.Value;

            if (colorIndex >= problem.ColorOrder.Count)
                return FlowSolveStatus.Solved;

            var colorId = problem.ColorOrder[colorIndex];
            var prefix = problem.PrefixByColor[colorId];

            context.Report("color", colorId);

            if (prefix.IsComplete)
            {
                solvedPaths[colorId] = CopyPath(prefix.Cells);
                var completeStatus = Search(problem, context, solvedPaths, colorIndex + 1);
                if (completeStatus == FlowSolveStatus.Solved)
                    return completeStatus;
                solvedPaths.Remove(colorId);
                return completeStatus == FlowSolveStatus.NoSolution ? FlowSolveStatus.NoSolution : completeStatus;
            }

            if (!BfsReachable(problem, prefix.Start, prefix.Target, colorId))
                return FlowSolveStatus.NoSolution;

            var path = CopyPath(prefix.Cells);
            var pathSet = new HashSet<FlowPos>(path);
            var result = TryPaths(problem, context, solvedPaths, colorIndex, colorId, prefix, path, pathSet);
            return result;
        }

        private static FlowSolveStatus TryPaths(
            Problem problem,
            SolveContext context,
            Dictionary<int, List<FlowPos>> solvedPaths,
            int colorIndex,
            int colorId,
            PrefixInfo prefix,
            List<FlowPos> path,
            HashSet<FlowPos> pathSet)
        {
            if (!context.Visit("dfs", colorId))
                return context.StopStatus.Value;

            var current = path[path.Count - 1];
            if (current == prefix.Target)
            {
                CommitPath(problem, colorId, path);
                solvedPaths[colorId] = CopyPath(path);

                var status = Search(problem, context, solvedPaths, colorIndex + 1);
                if (status == FlowSolveStatus.Solved)
                    return FlowSolveStatus.Solved;

                solvedPaths.Remove(colorId);
                ClearCommittedPath(problem, path);
                return status == FlowSolveStatus.NoSolution ? FlowSolveStatus.NoSolution : status;
            }

            var sawOnlyNoSolution = false;
            foreach (var next in problem.Board.GetNeighbors(current))
            {
                if (!CanEnter(problem, next, colorId, prefix.Target, pathSet))
                    continue;

                path.Add(next);
                pathSet.Add(next);

                var status = TryPaths(problem, context, solvedPaths, colorIndex, colorId, prefix, path, pathSet);
                if (status == FlowSolveStatus.Solved)
                    return FlowSolveStatus.Solved;
                if (status != FlowSolveStatus.NoSolution)
                    return status;

                sawOnlyNoSolution = true;
                pathSet.Remove(next);
                path.RemoveAt(path.Count - 1);
            }

            return sawOnlyNoSolution ? FlowSolveStatus.NoSolution : FlowSolveStatus.NoSolution;
        }

        private static FlowSolveStatus EnumerateSearch(
            Problem problem,
            SolveContext context,
            Dictionary<int, List<FlowPos>> solvedPaths,
            int colorIndex,
            Action<FlowSolutionData> onSolution)
        {
            if (context.IsStopped())
                return context.StopStatus.Value;

            if (colorIndex >= problem.ColorOrder.Count)
            {
                var candidate = BuildSolution(problem.Level, solvedPaths);
                context.RecordCandidate();
                onSolution?.Invoke(candidate);
                return context.IsStopped() ? context.StopStatus.Value : FlowSolveStatus.NoSolution;
            }

            var colorId = problem.ColorOrder[colorIndex];
            var prefix = problem.PrefixByColor[colorId];
            context.Report("color", colorId);

            if (prefix.IsComplete)
            {
                solvedPaths[colorId] = CopyPath(prefix.Cells);
                var completeStatus = EnumerateSearch(problem, context, solvedPaths, colorIndex + 1, onSolution);
                solvedPaths.Remove(colorId);
                return completeStatus;
            }

            if (!BfsReachable(problem, prefix.Start, prefix.Target, colorId))
                return FlowSolveStatus.NoSolution;

            var path = CopyPath(prefix.Cells);
            var pathSet = new HashSet<FlowPos>(path);
            return EnumeratePaths(problem, context, solvedPaths, colorIndex, colorId, prefix, path, pathSet, onSolution);
        }

        private static FlowSolveStatus EnumeratePaths(
            Problem problem,
            SolveContext context,
            Dictionary<int, List<FlowPos>> solvedPaths,
            int colorIndex,
            int colorId,
            PrefixInfo prefix,
            List<FlowPos> path,
            HashSet<FlowPos> pathSet,
            Action<FlowSolutionData> onSolution)
        {
            if (!context.Visit("dfs", colorId))
                return context.StopStatus.Value;

            var current = path[path.Count - 1];
            if (current == prefix.Target)
            {
                CommitPath(problem, colorId, path);
                solvedPaths[colorId] = CopyPath(path);
                var status = EnumerateSearch(problem, context, solvedPaths, colorIndex + 1, onSolution);
                solvedPaths.Remove(colorId);
                ClearCommittedPath(problem, path);
                return status;
            }

            foreach (var next in problem.Board.GetNeighbors(current))
            {
                if (!CanEnter(problem, next, colorId, prefix.Target, pathSet))
                    continue;

                path.Add(next);
                pathSet.Add(next);
                var status = EnumeratePaths(
                    problem, context, solvedPaths, colorIndex, colorId, prefix, path, pathSet, onSolution);
                pathSet.Remove(next);
                path.RemoveAt(path.Count - 1);
                if (status != FlowSolveStatus.NoSolution)
                    return status;
            }

            return FlowSolveStatus.NoSolution;
        }

        private static void CommitPath(Problem problem, int colorId, List<FlowPos> path)
        {
            foreach (var cell in path)
            {
                if (problem.Board.IsEmpty(cell))
                    problem.Board.Set(cell, colorId);
            }
        }

        private static void ClearCommittedPath(Problem problem, List<FlowPos> path)
        {
            foreach (var cell in path)
            {
                if (!problem.ProtectedCells.Contains(cell) && !problem.Board.IsEmpty(cell))
                    problem.Board.Clear(cell);
            }
        }

        private static bool BfsReachable(Problem problem, FlowPos start, FlowPos target, int colorId)
        {
            var visited = new HashSet<FlowPos> { start };
            var queue = new Queue<FlowPos>();
            queue.Enqueue(start);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                foreach (var next in problem.Board.GetNeighbors(current))
                {
                    if (visited.Contains(next))
                        continue;
                    if (!CanEnter(problem, next, colorId, target, null))
                        continue;
                    if (next == target)
                        return true;

                    visited.Add(next);
                    queue.Enqueue(next);
                }
            }

            return start == target;
        }

        private static bool CanEnter(Problem problem, FlowPos cell, int colorId, FlowPos target, HashSet<FlowPos> pathSet)
        {
            if (pathSet != null && pathSet.Contains(cell))
                return false;

            if (cell == target)
                return true;

            if (problem.EndpointColorByCell.TryGetValue(cell, out var endpointColor) && endpointColor != colorId)
                return false;

            return problem.Board.IsEmpty(cell);
        }

        private static FlowSolutionData BuildSolution(FlowLevelData level, Dictionary<int, List<FlowPos>> solvedPaths)
        {
            var solution = new FlowSolutionData { levelId = level.levelId };
            foreach (var pair in level.pairs.OrderBy(p => p.colorId))
            {
                if (!solvedPaths.TryGetValue(pair.colorId, out var cells))
                    continue;

                solution.paths.Add(new FlowPathData
                {
                    colorId = pair.colorId,
                    cells = CopyPath(cells)
                });
            }

            return solution;
        }

        private static bool TryBuildProblem(FlowSolveRequest request, out Problem problem)
        {
            problem = null;

            if (request == null || request.levelData == null)
                return false;
            if (request.nodeBudget <= 0 || request.timeoutMs <= 0 || request.progressIntervalNodes <= 0)
                return false;

            var level = request.levelData;
            if (level.width <= 0 || level.height <= 0 || level.pairs == null || level.pairs.Count == 0)
                return false;

            var board = new FlowBoard(level.width, level.height);
            var pairByColor = new Dictionary<int, FlowPairData>();
            var endpointColorByCell = new Dictionary<FlowPos, int>();
            var protectedCells = new HashSet<FlowPos>();

            foreach (var pair in level.pairs)
            {
                if (pair == null || pair.colorId < 0)
                    return false;
                if (pairByColor.ContainsKey(pair.colorId))
                    return false;
                pairByColor.Add(pair.colorId, pair);
                if (!board.IsInside(pair.endpointA) || !board.IsInside(pair.endpointB))
                    return false;
                if (pair.endpointA == pair.endpointB)
                    return false;
                if (endpointColorByCell.ContainsKey(pair.endpointA))
                    return false;
                endpointColorByCell.Add(pair.endpointA, pair.colorId);
                if (endpointColorByCell.ContainsKey(pair.endpointB))
                    return false;
                endpointColorByCell.Add(pair.endpointB, pair.colorId);

                board.Set(pair.endpointA, pair.colorId);
                board.Set(pair.endpointB, pair.colorId);
                protectedCells.Add(pair.endpointA);
                protectedCells.Add(pair.endpointB);
            }

            var prefixByColor = pairByColor.Keys.ToDictionary(
                colorId => colorId,
                colorId =>
                {
                    var pair = pairByColor[colorId];
                    return PrefixInfo.None(colorId, pair.endpointA, pair.endpointB);
                });

            var occupiedPrefixCells = new Dictionary<FlowPos, int>();
            if (request.fixedPrefixes != null)
            {
                foreach (var fixedPrefix in request.fixedPrefixes)
                {
                    if (!TryValidatePrefix(fixedPrefix, board, pairByColor, endpointColorByCell, occupiedPrefixCells, out var prefix))
                        return false;

                    prefixByColor[prefix.ColorId] = prefix;
                    foreach (var cell in prefix.Cells)
                    {
                        if (board.IsEmpty(cell))
                            board.Set(cell, prefix.ColorId);
                        protectedCells.Add(cell);
                    }
                }
            }

            var colorOrder = level.pairs
                .Select(pair => pair.colorId)
                .OrderBy(colorId => prefixByColor[colorId].HasFixedPrefix ? 0 : 1)
                .ThenBy(colorId => FlowPathUtility.GetManhattanDistance(pairByColor[colorId].endpointA, pairByColor[colorId].endpointB))
                .ThenBy(colorId => colorId)
                .ToList();

            problem = new Problem(level, board, pairByColor, prefixByColor, endpointColorByCell, protectedCells, colorOrder);
            return true;
        }

        private static bool TryValidatePrefix(
            FlowPathData fixedPrefix,
            FlowBoard board,
            Dictionary<int, FlowPairData> pairByColor,
            Dictionary<FlowPos, int> endpointColorByCell,
            Dictionary<FlowPos, int> occupiedPrefixCells,
            out PrefixInfo prefix)
        {
            prefix = null;
            if (fixedPrefix == null || fixedPrefix.cells == null || fixedPrefix.cells.Count < 2)
                return false;
            if (!pairByColor.TryGetValue(fixedPrefix.colorId, out var pair))
                return false;

            var first = fixedPrefix.cells[0];
            FlowPos target;
            if (first == pair.endpointA)
                target = pair.endpointB;
            else if (first == pair.endpointB)
                target = pair.endpointA;
            else
                return false;

            var seen = new HashSet<FlowPos>();
            for (var i = 0; i < fixedPrefix.cells.Count; i++)
            {
                var cell = fixedPrefix.cells[i];
                if (!board.IsInside(cell))
                    return false;
                if (!seen.Add(cell))
                    return false;
                if (i > 0 && !FlowPathUtility.AreAdjacent(fixedPrefix.cells[i - 1], cell))
                    return false;
                if (cell == target && i != fixedPrefix.cells.Count - 1)
                    return false;
                if (endpointColorByCell.TryGetValue(cell, out var endpointColor) && endpointColor != fixedPrefix.colorId)
                    return false;
                if (occupiedPrefixCells.TryGetValue(cell, out var existingColor) && existingColor != fixedPrefix.colorId)
                    return false;
                if (occupiedPrefixCells.TryGetValue(cell, out existingColor) && existingColor == fixedPrefix.colorId)
                    return false;

                occupiedPrefixCells[cell] = fixedPrefix.colorId;
            }

            prefix = PrefixInfo.Fixed(fixedPrefix.colorId, CopyPath(fixedPrefix.cells), target);
            return true;
        }

        private static List<FlowPos> CopyPath(List<FlowPos> source)
        {
            var copy = new List<FlowPos>(source.Count);
            foreach (var cell in source)
                copy.Add(new FlowPos(cell.x, cell.y));
            return copy;
        }

        private sealed class Problem
        {
            public Problem(
                FlowLevelData level,
                FlowBoard board,
                Dictionary<int, FlowPairData> pairByColor,
                Dictionary<int, PrefixInfo> prefixByColor,
                Dictionary<FlowPos, int> endpointColorByCell,
                HashSet<FlowPos> protectedCells,
                List<int> colorOrder)
            {
                Level = level;
                Board = board;
                PairByColor = pairByColor;
                PrefixByColor = prefixByColor;
                EndpointColorByCell = endpointColorByCell;
                ProtectedCells = protectedCells;
                ColorOrder = colorOrder;
            }

            public FlowLevelData Level { get; }
            public FlowBoard Board { get; }
            public Dictionary<int, FlowPairData> PairByColor { get; }
            public Dictionary<int, PrefixInfo> PrefixByColor { get; }
            public Dictionary<FlowPos, int> EndpointColorByCell { get; }
            public HashSet<FlowPos> ProtectedCells { get; }
            public List<int> ColorOrder { get; }
        }

        private sealed class PrefixInfo
        {
            private PrefixInfo(int colorId, List<FlowPos> cells, FlowPos target, bool hasFixedPrefix)
            {
                ColorId = colorId;
                Cells = cells;
                Target = target;
                HasFixedPrefix = hasFixedPrefix;
            }

            public int ColorId { get; }
            public List<FlowPos> Cells { get; }
            public FlowPos Start => Cells[Cells.Count - 1];
            public FlowPos Target { get; }
            public bool HasFixedPrefix { get; }
            public bool IsComplete => Start == Target;

            public static PrefixInfo None(int colorId, FlowPos start, FlowPos target)
            {
                return new PrefixInfo(colorId, new List<FlowPos> { start }, target, false);
            }

            public static PrefixInfo Fixed(int colorId, List<FlowPos> cells, FlowPos target)
            {
                return new PrefixInfo(colorId, cells, target, true);
            }
        }

        private sealed class SolveContext
        {
            private readonly Stopwatch stopwatch = Stopwatch.StartNew();
            private readonly long nodeBudget;
            private readonly int timeoutMs;
            private readonly int progressIntervalNodes;
            private readonly IProgress<FlowSolveProgress> progress;
            private readonly CancellationToken cancellationToken;

            public SolveContext(long nodeBudget, int timeoutMs, int progressIntervalNodes, IProgress<FlowSolveProgress> progress, CancellationToken cancellationToken)
            {
                this.nodeBudget = nodeBudget;
                this.timeoutMs = timeoutMs;
                this.progressIntervalNodes = progressIntervalNodes;
                this.progress = progress;
                this.cancellationToken = cancellationToken;
            }

            public long VisitedNodes { get; private set; }
            public int CandidateCount { get; private set; }
            public FlowSolveStatus? StopStatus { get; private set; }

            public void RecordCandidate()
            {
                CandidateCount++;
                if (CandidateCount == 1 || CandidateCount % 10 == 0)
                    Report("candidate");
            }

            public bool Visit(string phase, int colorId)
            {
                if (IsStopped())
                    return false;

                VisitedNodes++;
                if (VisitedNodes == 1 || VisitedNodes % progressIntervalNodes == 0)
                    Report(phase, colorId);

                return !IsStopped();
            }

            public bool IsStopped()
            {
                if (StopStatus.HasValue)
                    return true;
                if (cancellationToken.IsCancellationRequested)
                {
                    StopStatus = FlowSolveStatus.Cancelled;
                    return true;
                }
                if (VisitedNodes >= nodeBudget)
                {
                    StopStatus = FlowSolveStatus.Timeout;
                    return true;
                }
                if (stopwatch.ElapsedMilliseconds > timeoutMs)
                {
                    StopStatus = FlowSolveStatus.Timeout;
                    return true;
                }

                return false;
            }

            public void Report(string phase, int colorId = -1)
            {
                progress?.Report(new FlowSolveProgress
                {
                    phase = phase,
                    currentColorId = colorId,
                    visitedNodes = VisitedNodes,
                    elapsedMs = stopwatch.ElapsedMilliseconds,
                    candidateCount = CandidateCount
                });
            }

            public FlowSolveResult Result(FlowSolveStatus status, FlowSolutionData solution = null)
            {
                return new FlowSolveResult
                {
                    status = status,
                    solution = solution,
                    visitedNodes = VisitedNodes,
                    elapsedMs = stopwatch.ElapsedMilliseconds
                };
            }

            public FlowSolutionEnumerationResult EnumerationResult(FlowSolveStatus status, bool searchExhausted)
            {
                return new FlowSolutionEnumerationResult
                {
                    status = status,
                    candidateCount = CandidateCount,
                    visitedNodes = VisitedNodes,
                    elapsedMs = stopwatch.ElapsedMilliseconds,
                    searchExhausted = searchExhausted
                };
            }
        }
    }
}
