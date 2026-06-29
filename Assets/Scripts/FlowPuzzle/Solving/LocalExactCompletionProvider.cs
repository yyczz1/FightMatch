using System;
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
            return await Task.Run(() =>
            {
                var solver = new BacktrackingFlowPuzzleSolver();
                var solverProgress = new Progress<FlowSolveProgress>(sp =>
                    progress?.Report(new FlowCompletionProgress { phase = sp.phase, visitedNodes = sp.visitedNodes, elapsedMs = sp.elapsedMs, currentColorId = sp.currentColorId }));

                var solveReq = new FlowSolveRequest { levelData = request.levelData };
                if (request.currentSolution?.paths != null)
                    solveReq.fixedPrefixes = new System.Collections.Generic.List<FlowPathData>(request.currentSolution.paths);

                var result = solver.Solve(solveReq, solverProgress, ct);

                var completionResult = new FlowCompletionResult { status = result.status, visitedNodes = result.visitedNodes, elapsedMs = result.elapsedMs };
                if (result.status == FlowSolveStatus.Solved && result.solution != null)
                {
                    completionResult.generatedLevel = new FlowGeneratedLevel
                    {
                        levelData = request.levelData,
                        solutionData = result.solution,
                        usedSeed = 0,
                        coverageRatio = ComputeCoverage(request.levelData, result.solution)
                    };
                }
                return completionResult;
            }, ct);
        }

        private static float ComputeCoverage(FlowLevelData level, FlowSolutionData solution)
        {
            var board = new FlowBoard(level.width, level.height);
            foreach (var path in solution.paths)
                foreach (var cell in path.cells)
                    board.Set(cell, path.colorId);
            return (float)board.OccupiedCellCount / (level.width * level.height);
        }
    }
}
