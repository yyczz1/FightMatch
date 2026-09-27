using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FlowPuzzle.Core;

namespace FlowPuzzle.Solving
{
    public sealed class LocalExactCompletionProvider : IFlowLevelCompletionProvider
    {
        public string DisplayName => "Local Exact Solver";

        public async Task<FlowCompletionResult> CompleteAsync(FlowCompletionRequest request, IProgress<FlowCompletionProgress> progress, CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
                return Failure(FlowSolveStatus.Cancelled, "Cancelled", "Completion was cancelled before it started.");
            if (request == null || request.levelData == null)
                return Failure(FlowSolveStatus.InvalidInput, "MissingLevelData", "Completion requires level data with endpoints.");

            var snapshot = CloneRequest(request);
            return await Task.Run(() =>
            {
                var solver = new BacktrackingFlowPuzzleSolver();
                var solverProgress = new CompletionProgressAdapter(progress);

                var solveReq = new FlowSolveRequest
                {
                    levelData = snapshot.levelData,
                    fixedPrefixes = snapshot.fixedPrefixes,
                    nodeBudget = snapshot.nodeBudget,
                    timeoutMs = snapshot.timeoutMs,
                    progressIntervalNodes = snapshot.progressIntervalNodes
                };

                var result = solver.Solve(solveReq, solverProgress, ct);

                var completionResult = new FlowCompletionResult { status = result.status, visitedNodes = result.visitedNodes, elapsedMs = result.elapsedMs };
                if (result.status == FlowSolveStatus.Solved && result.solution != null)
                {
                    completionResult.generatedLevel = new FlowGeneratedLevel
                    {
                        levelData = CloneLevel(snapshot.levelData),
                        solutionData = CloneSolution(result.solution),
                        usedSeed = 0,
                        coverageRatio = ComputeCoverage(snapshot.levelData, result.solution)
                    };
                }
                else
                {
                    PopulateFailure(completionResult);
                }
                return completionResult;
            }, CancellationToken.None).ConfigureAwait(false);
        }

        private static FlowCompletionResult Failure(FlowSolveStatus status, string code, string message)
        {
            return new FlowCompletionResult { status = status, errorCode = code, errorMessage = message };
        }

        private static void PopulateFailure(FlowCompletionResult result)
        {
            switch (result.status)
            {
                case FlowSolveStatus.NoSolution:
                    result.errorCode = "NoSolution";
                    result.errorMessage = "No legal solution exists for the current endpoints and fixed constraints. Remove or redraw constraints, or move endpoints.";
                    break;
                case FlowSolveStatus.Timeout:
                    result.errorCode = "SolverLimitReached";
                    result.errorMessage = $"Solver stopped after {result.elapsedMs} ms and {result.visitedNodes} visited nodes. Increase Solver Timeout/Budget or simplify the draft.";
                    break;
                case FlowSolveStatus.Cancelled:
                    result.errorCode = "Cancelled";
                    result.errorMessage = "Completion was cancelled.";
                    break;
                case FlowSolveStatus.InvalidInput:
                    result.errorCode = "InvalidInput";
                    result.errorMessage = "The draft contains invalid dimensions, endpoints, or fixed constraints.";
                    break;
                default:
                    result.errorCode = "SolverError";
                    result.errorMessage = "The local solver encountered an internal error.";
                    break;
            }
        }

        private static float ComputeCoverage(FlowLevelData level, FlowSolutionData solution)
        {
            var board = new FlowBoard(level.width, level.height);
            foreach (var path in solution.paths)
                foreach (var cell in path.cells)
                    board.Set(cell, path.colorId);
            return (float)board.OccupiedCellCount / (level.width * level.height);
        }

        private static FlowCompletionRequest CloneRequest(FlowCompletionRequest request)
        {
            return new FlowCompletionRequest
            {
                levelData = CloneLevel(request.levelData),
                currentSolution = CloneSolution(request.currentSolution),
                fixedPrefixes = ClonePaths(request.fixedPrefixes),
                timeoutMs = request.timeoutMs,
                nodeBudget = request.nodeBudget,
                progressIntervalNodes = request.progressIntervalNodes
            };
        }

        private static FlowLevelData CloneLevel(FlowLevelData source)
        {
            if (source == null)
                return null;

            var copy = new FlowLevelData
            {
                levelId = source.levelId,
                width = source.width,
                height = source.height,
                difficulty = source.difficulty,
                difficultyScore = source.difficultyScore
            };

            if (source.pairs != null)
            {
                foreach (var pair in source.pairs)
                {
                    if (pair == null)
                        continue;
                    copy.pairs.Add(new FlowPairData
                    {
                        colorId = pair.colorId,
                        endpointA = new FlowPos(pair.endpointA.x, pair.endpointA.y),
                        endpointB = new FlowPos(pair.endpointB.x, pair.endpointB.y)
                    });
                }
            }

            return copy;
        }

        private static FlowSolutionData CloneSolution(FlowSolutionData source)
        {
            if (source == null)
                return null;

            return new FlowSolutionData
            {
                levelId = source.levelId,
                paths = ClonePaths(source.paths)
            };
        }

        private static List<FlowPathData> ClonePaths(List<FlowPathData> source)
        {
            if (source == null)
                return null;

            var paths = new List<FlowPathData>(source.Count);
            foreach (var path in source)
            {
                if (path == null)
                    continue;

                var pathCopy = new FlowPathData { colorId = path.colorId };
                pathCopy.cells = new List<FlowPos>();
                if (path.cells != null)
                {
                    foreach (var cell in path.cells)
                        pathCopy.cells.Add(new FlowPos(cell.x, cell.y));
                }
                paths.Add(pathCopy);
            }

            return paths;
        }

        private sealed class CompletionProgressAdapter : IProgress<FlowSolveProgress>
        {
            private readonly IProgress<FlowCompletionProgress> progress;

            public CompletionProgressAdapter(IProgress<FlowCompletionProgress> progress)
            {
                this.progress = progress;
            }

            public void Report(FlowSolveProgress value)
            {
                if (value == null)
                    return;

                progress?.Report(new FlowCompletionProgress
                {
                    phase = value.phase,
                    visitedNodes = value.visitedNodes,
                    elapsedMs = value.elapsedMs,
                    currentColorId = value.currentColorId
                });
            }
        }
    }
}
