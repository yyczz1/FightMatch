using System;
using System.Collections.Generic;
using System.Threading;
using FlowPuzzle.Core;
using FlowPuzzle.Difficulty;
using FlowPuzzle.Solving;
using FlowPuzzle.Solving.Tools;
using FlowPuzzle.Validation;

namespace FlowPuzzle.Application.Tools
{
    /// <summary>
    /// Pure DTO-in/DTO-out adapter that wraps the local solver, validator, and difficulty
    /// evaluator behind stable tool-style contracts. Does not implement HTTP, API keys,
    /// model selection, or an LLM provider.
    /// </summary>
    public sealed class FlowSolverToolAdapter
    {
        private readonly IFlowPuzzleSolver solver;
        private readonly FlowSolutionValidator validator;
        private readonly FlowDifficultyEvaluator evaluator;

        public FlowSolverToolAdapter(
            IFlowPuzzleSolver solver,
            FlowSolutionValidator validator,
            FlowDifficultyEvaluator evaluator)
        {
            this.solver = solver ?? throw new ArgumentNullException(nameof(solver));
            this.validator = validator ?? throw new ArgumentNullException(nameof(validator));
            this.evaluator = evaluator ?? throw new ArgumentNullException(nameof(evaluator));
        }

        // ---- Solve ----

        public FlowSolveResult Solve(
            FlowLevelData level,
            IProgress<FlowSolveProgress> progress = null,
            CancellationToken ct = default)
        {
            if (level == null)
                return new FlowSolveResult { status = FlowSolveStatus.InvalidInput };

            var request = new FlowSolveRequest { levelData = CloneLevelData(level) };
            return CloneSolveResult(solver.Solve(request, progress, ct));
        }

        public FlowSolveResult SolveWithPrefixes(
            FlowLevelData level,
            IReadOnlyList<FlowPathData> fixedPrefixes,
            IProgress<FlowSolveProgress> progress = null,
            CancellationToken ct = default)
        {
            if (level == null)
                return new FlowSolveResult { status = FlowSolveStatus.InvalidInput };

            var request = new FlowSolveRequest
            {
                levelData = CloneLevelData(level),
                fixedPrefixes = ClonePaths(fixedPrefixes)
            };

            return CloneSolveResult(solver.Solve(request, progress, ct));
        }

        // ---- Validate ----

        public FlowValidationResult Validate(FlowLevelData level, FlowSolutionData solution)
        {
            if (level == null)
                return FlowValidationResult.Invalid("NullLevel", "Level data is null.");

            if (solution == null)
                return FlowValidationResult.Invalid("NullSolution", "Solution data is null.");

            return validator.Validate(level, solution);
        }

        public FlowValidationResult ValidateLevel(FlowGeneratedLevel generatedLevel)
        {
            if (generatedLevel == null)
                return FlowValidationResult.Invalid("NullGeneratedLevel", "Generated level is null.");

            return validator.Validate(generatedLevel.levelData, generatedLevel.solutionData);
        }

        // ---- Evaluate Difficulty ----

        public FlowDifficultyReport EvaluateDifficulty(FlowLevelData level, FlowSolutionData solution)
        {
            if (level == null || solution == null)
                return new FlowDifficultyReport(); // empty/default report

            return evaluator.Evaluate(level, solution);
        }

        public FlowDifficultyReport EvaluateLevelDifficulty(FlowGeneratedLevel generatedLevel)
        {
            if (generatedLevel == null)
                return new FlowDifficultyReport();

            return evaluator.Evaluate(generatedLevel.levelData, generatedLevel.solutionData);
        }

        // ---- Analyze Failure ----

        public FlowFailureDiagnostic AnalyzeFailure(
            FlowSolveResult result,
            FlowLevelData level,
            FlowGenerationConfig config = null)
        {
            if (result == null)
                return new FlowFailureDiagnostic
                {
                    errorCode = "NullResult",
                    errorMessage = "Solve result is null."
                };

            string errorCode;
            string errorMessage;

            switch (result.status)
            {
                case FlowSolveStatus.NoSolution:
                    errorCode = FlowDiagnosticCodes.NoSolution;
                    errorMessage = "No solution exists for the given endpoints and constraints.";
                    break;

                case FlowSolveStatus.Timeout:
                    errorCode = FlowDiagnosticCodes.SolverTimeout;
                    errorMessage = $"Solver timed out after visiting {result.visitedNodes} nodes " +
                                    $"({result.elapsedMs} ms).";
                    break;

                case FlowSolveStatus.Cancelled:
                    errorCode = FlowDiagnosticCodes.SolverCancelled;
                    errorMessage = "Solver was cancelled.";
                    break;

                case FlowSolveStatus.InvalidInput:
                    errorCode = "InvalidInput";
                    errorMessage = "Solver received invalid input.";
                    break;

                case FlowSolveStatus.Error:
                    errorCode = "SolverError";
                    errorMessage = "Solver encountered an internal error.";
                    break;

                case FlowSolveStatus.Solved:
                    return null; // No failure to diagnose

                default:
                    errorCode = "UnknownStatus";
                    errorMessage = $"Unknown solver status: {result.status}";
                    break;
            }

            var diagnostic = new FlowFailureDiagnostic
            {
                errorCode = errorCode,
                errorMessage = errorMessage
            };

            if (config != null)
                diagnostic.suggestions = FlowDiagnosticMapper.Map(errorCode, config);

            return diagnostic;
        }

        // ---- Convenience: Solve + Validate + Evaluate in one call ----

        public FlowGeneratedLevel SolveAndValidate(
            FlowLevelData level,
            IProgress<FlowSolveProgress> progress = null,
            CancellationToken ct = default)
        {
            if (level == null)
                return null;

            var levelSnapshot = CloneLevelData(level);
            var solveResult = solver.Solve(new FlowSolveRequest { levelData = levelSnapshot }, progress, ct);
            if (solveResult.status != FlowSolveStatus.Solved)
                return null;

            var solutionSnapshot = CloneSolutionData(solveResult.solution);
            var validation = validator.Validate(levelSnapshot, solutionSnapshot);
            if (!validation.isValid)
                return null;

            var difficulty = evaluator.Evaluate(levelSnapshot, solutionSnapshot);

            int totalCells = 0;
            foreach (var path in solutionSnapshot.paths)
            {
                if (path.cells != null)
                    totalCells += path.cells.Count;
            }

            return new FlowGeneratedLevel
            {
                levelData = levelSnapshot,
                solutionData = solutionSnapshot,
                difficultyReport = difficulty,
                coverageRatio = (float)totalCells / (levelSnapshot.width * levelSnapshot.height)
            };
        }

        private static FlowSolveResult CloneSolveResult(FlowSolveResult source)
        {
            if (source == null)
                return null;

            return new FlowSolveResult
            {
                status = source.status,
                solution = CloneSolutionData(source.solution),
                visitedNodes = source.visitedNodes,
                elapsedMs = source.elapsedMs
            };
        }

        private static FlowLevelData CloneLevelData(FlowLevelData source)
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

        private static FlowSolutionData CloneSolutionData(FlowSolutionData source)
        {
            if (source == null)
                return null;

            return new FlowSolutionData
            {
                levelId = source.levelId,
                paths = ClonePaths(source.paths)
            };
        }

        private static List<FlowPathData> ClonePaths(IReadOnlyList<FlowPathData> source)
        {
            if (source == null)
                return null;

            var copy = new List<FlowPathData>(source.Count);
            foreach (var path in source)
            {
                if (path == null)
                    continue;

                var pathCopy = new FlowPathData { colorId = path.colorId };
                if (path.cells != null)
                {
                    pathCopy.cells = new List<FlowPos>(path.cells.Count);
                    foreach (var cell in path.cells)
                        pathCopy.cells.Add(new FlowPos(cell.x, cell.y));
                }
                else
                {
                    pathCopy.cells = null;
                }
                copy.Add(pathCopy);
            }

            return copy;
        }
    }
}
