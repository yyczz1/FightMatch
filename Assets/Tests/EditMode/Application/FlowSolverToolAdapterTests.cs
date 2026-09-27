using System;
using System.Collections.Generic;
using System.Threading;
using FlowPuzzle.Application.Tools;
using FlowPuzzle.Core;
using FlowPuzzle.Difficulty;
using FlowPuzzle.Solving;
using FlowPuzzle.Validation;
using NUnit.Framework;

namespace FlowPuzzle.Tests.Application
{
    [TestFixture]
    public class FlowSolverToolAdapterTests
    {
        private FlowSolverToolAdapter CreateAdapter()
        {
            var solver = new BacktrackingFlowPuzzleSolver();
            var validator = new FlowSolutionValidator();
            var evaluator = new FlowDifficultyEvaluator();
            return new FlowSolverToolAdapter(solver, validator, evaluator);
        }

        private static FlowLevelData MakeSimpleLevel(int width = 4, int height = 4, int colors = 2)
        {
            var level = new FlowLevelData { levelId = 1, width = width, height = height };

            // Place color 0 on the bottom row.
            level.pairs.Add(new FlowPairData
            {
                colorId = 0,
                endpointA = new FlowPos(0, 0),
                endpointB = new FlowPos(width - 1, 0)
            });

            if (colors >= 2)
            {
                // Place color 1 on the top row so both pairs are jointly solvable.
                level.pairs.Add(new FlowPairData
                {
                    colorId = 1,
                    endpointA = new FlowPos(0, height - 1),
                    endpointB = new FlowPos(width - 1, height - 1)
                });
            }

            return level;
        }

        private static FlowLevelData MakeValidImpossibleLevel()
        {
            var level = new FlowLevelData { levelId = 2, width = 2, height = 2 };
            level.pairs.Add(new FlowPairData
            {
                colorId = 0,
                endpointA = new FlowPos(0, 0),
                endpointB = new FlowPos(1, 1)
            });
            level.pairs.Add(new FlowPairData
            {
                colorId = 1,
                endpointA = new FlowPos(1, 0),
                endpointB = new FlowPos(0, 1)
            });
            return level;
        }

        [Test]
        public void Solve_SimpleLevel_ReturnsSolved()
        {
            var adapter = CreateAdapter();
            var level = MakeSimpleLevel(3, 3, 1);

            var result = adapter.Solve(level);

            Assert.That(result.status, Is.EqualTo(FlowSolveStatus.Solved));
            Assert.That(result.solution, Is.Not.Null);
            Assert.That(result.solution.paths.Count, Is.EqualTo(1));
        }

        [Test]
        public void Solve_WithPrefixesHonoured_ReturnsSolved()
        {
            var adapter = CreateAdapter();
            var level = MakeSimpleLevel(4, 4, 1);

            // Provide a fixed prefix for color 0: first two steps
            var prefix = new FlowPathData
            {
                colorId = 0,
                cells = new System.Collections.Generic.List<FlowPos>
                {
                    new FlowPos(0, 0),
                    new FlowPos(0, 1)
                }
            };
            var prefixes = new[] { prefix };

            var result = adapter.SolveWithPrefixes(level, prefixes);

            Assert.That(result.status, Is.EqualTo(FlowSolveStatus.Solved));
            Assert.That(result.solution, Is.Not.Null);

            // The solution path for color 0 should start with the prefix cells
            var path0 = result.solution.paths.Find(p => p.colorId == 0);
            Assert.That(path0, Is.Not.Null);
            Assert.That(path0.cells[0].x, Is.EqualTo(0));
            Assert.That(path0.cells[0].y, Is.EqualTo(0));
            Assert.That(path0.cells[1].x, Is.EqualTo(0));
            Assert.That(path0.cells[1].y, Is.EqualTo(1));
        }

        [Test]
        public void Solve_ImpossibleLevel_ReturnsNoSolution()
        {
            var adapter = CreateAdapter();
            var level = MakeValidImpossibleLevel();

            var result = adapter.Solve(level);

            Assert.That(result.status, Is.EqualTo(FlowSolveStatus.NoSolution));
        }

        [Test]
        public void Solve_NullLevel_ReturnsInvalidInput()
        {
            var adapter = CreateAdapter();
            var result = adapter.Solve(null);
            Assert.That(result.status, Is.EqualTo(FlowSolveStatus.InvalidInput));
        }

        [Test]
        public void Validate_ValidSolution_ReturnsValid()
        {
            var adapter = CreateAdapter();
            var level = MakeSimpleLevel(3, 3, 1);

            var solveResult = adapter.Solve(level);
            Assert.That(solveResult.status, Is.EqualTo(FlowSolveStatus.Solved));

            var validation = adapter.Validate(level, solveResult.solution);
            Assert.That(validation.isValid, Is.True);
        }

        [Test]
        public void Validate_NullLevel_ReturnsInvalid()
        {
            var adapter = CreateAdapter();
            var validation = adapter.Validate(null, null);
            Assert.That(validation.isValid, Is.False);
            Assert.That(validation.errorCode, Is.EqualTo("NullLevel"));
        }

        [Test]
        public void EvaluateDifficulty_ReturnsReport()
        {
            var adapter = CreateAdapter();
            var level = MakeSimpleLevel(4, 4, 2);

            var solveResult = adapter.Solve(level);
            Assert.That(solveResult.status, Is.EqualTo(FlowSolveStatus.Solved));

            var report = adapter.EvaluateDifficulty(level, solveResult.solution);
            Assert.That(report, Is.Not.Null);
            Assert.That(report.difficulty, Is.Not.Null);
        }

        [Test]
        public void AnalyzeFailure_NoSolution_ProducesCorrectDiagnostic()
        {
            var adapter = CreateAdapter();
            var level = MakeValidImpossibleLevel();

            var solveResult = adapter.Solve(level);
            Assert.That(solveResult.status, Is.EqualTo(FlowSolveStatus.NoSolution));

            var diagnostic = adapter.AnalyzeFailure(solveResult, level);

            Assert.That(diagnostic, Is.Not.Null);
            Assert.That(diagnostic.errorCode, Is.EqualTo(FlowDiagnosticCodes.NoSolution));
            Assert.That(diagnostic.errorMessage, Does.Contain("No solution"));
        }

        [Test]
        public void AnalyzeFailure_Solved_ReturnsNull()
        {
            var adapter = CreateAdapter();
            var level = MakeSimpleLevel(3, 3, 1);

            var solveResult = adapter.Solve(level);
            Assert.That(solveResult.status, Is.EqualTo(FlowSolveStatus.Solved));

            var diagnostic = adapter.AnalyzeFailure(solveResult, level);
            Assert.That(diagnostic, Is.Null, "solved should not produce failure diagnostic");
        }

        [Test]
        public void AnalyzeFailure_Timeout_WithConfig_PopulatesSuggestions()
        {
            var adapter = CreateAdapter();
            var level = MakeSimpleLevel(4, 4, 2);
            var config = new FlowGenerationConfig
            {
                width = 4, height = 4, colorCount = 2,
                solverTimeoutMilliseconds = 1, solverNodeBudget = 10
            };

            var solveResult = new FlowSolveResult
            {
                status = FlowSolveStatus.Timeout,
                visitedNodes = 123,
                elapsedMs = 1
            };

            var diagnostic = adapter.AnalyzeFailure(solveResult, level, config);

            Assert.That(diagnostic, Is.Not.Null);
            Assert.That(diagnostic.errorCode, Is.EqualTo(FlowDiagnosticCodes.SolverTimeout));
            Assert.That(diagnostic.suggestions.Count, Is.GreaterThan(0),
                "timeout diagnostic with config should produce suggestions");
            Assert.That(diagnostic.errorMessage, Does.Contain("123"));
        }

        [Test]
        public void SolveWithPrefixes_PassesOwnedRequestToSolver()
        {
            var solver = new CapturingSolver(new FlowSolveResult { status = FlowSolveStatus.InvalidInput });
            var adapter = new FlowSolverToolAdapter(
                solver,
                new FlowSolutionValidator(),
                new FlowDifficultyEvaluator());
            var level = MakeSimpleLevel(4, 4, 1);
            var prefix = new FlowPathData
            {
                colorId = 0,
                cells = new List<FlowPos> { new FlowPos(0, 0), new FlowPos(0, 1) }
            };
            var prefixes = new List<FlowPathData> { prefix };

            adapter.SolveWithPrefixes(level, prefixes);

            Assert.That(solver.CapturedRequest.levelData, Is.Not.SameAs(level));
            Assert.That(solver.CapturedRequest.levelData.pairs[0], Is.Not.SameAs(level.pairs[0]));
            Assert.That(solver.CapturedRequest.fixedPrefixes, Is.Not.SameAs(prefixes));
            Assert.That(solver.CapturedRequest.fixedPrefixes[0], Is.Not.SameAs(prefix));
            Assert.That(solver.CapturedRequest.fixedPrefixes[0].cells, Is.Not.SameAs(prefix.cells));

            level.width = 99;
            level.pairs[0].endpointA = new FlowPos(9, 9);
            prefix.cells[0] = new FlowPos(8, 8);

            Assert.That(solver.CapturedRequest.levelData.width, Is.EqualTo(4));
            Assert.That(solver.CapturedRequest.levelData.pairs[0].endpointA, Is.EqualTo(new FlowPos(0, 0)));
            Assert.That(solver.CapturedRequest.fixedPrefixes[0].cells[0], Is.EqualTo(new FlowPos(0, 0)));
        }

        [Test]
        public void SolveAndValidate_ReturnsGeneratedLevelWithOwnedData()
        {
            var level = MakeSimpleLevel(3, 1, 1);
            level.pairs[0].endpointB = new FlowPos(2, 0);
            var solution = new FlowSolutionData { levelId = level.levelId };
            solution.paths.Add(new FlowPathData
            {
                colorId = 0,
                cells = new List<FlowPos>
                {
                    new FlowPos(0, 0),
                    new FlowPos(1, 0),
                    new FlowPos(2, 0)
                }
            });
            var solver = new CapturingSolver(new FlowSolveResult
            {
                status = FlowSolveStatus.Solved,
                solution = solution
            });
            var adapter = new FlowSolverToolAdapter(
                solver,
                new FlowSolutionValidator(),
                new FlowDifficultyEvaluator());

            var generated = adapter.SolveAndValidate(level);

            Assert.That(generated, Is.Not.Null);
            Assert.That(generated.levelData, Is.Not.SameAs(level));
            Assert.That(generated.solutionData, Is.Not.SameAs(solution));
            Assert.That(generated.solutionData.paths[0], Is.Not.SameAs(solution.paths[0]));
            Assert.That(generated.solutionData.paths[0].cells, Is.Not.SameAs(solution.paths[0].cells));

            level.width = 99;
            level.pairs[0].endpointA = new FlowPos(9, 9);
            solution.paths[0].cells[0] = new FlowPos(8, 8);

            Assert.That(generated.levelData.width, Is.EqualTo(3));
            Assert.That(generated.levelData.pairs[0].endpointA, Is.EqualTo(new FlowPos(0, 0)));
            Assert.That(generated.solutionData.paths[0].cells[0], Is.EqualTo(new FlowPos(0, 0)));
        }

        [Test]
        public void SolveAndValidate_ReturnsGeneratedLevel()
        {
            var adapter = CreateAdapter();
            var level = MakeSimpleLevel(4, 4, 2);

            var generated = adapter.SolveAndValidate(level);

            Assert.That(generated, Is.Not.Null);
            Assert.That(generated.levelData, Is.Not.Null);
            Assert.That(generated.solutionData, Is.Not.Null);
            Assert.That(generated.difficultyReport, Is.Not.Null);
            Assert.That(generated.coverageRatio, Is.GreaterThan(0f));
        }

        [Test]
        public void Solve_RespectsCancellationToken()
        {
            var adapter = CreateAdapter();
            var level = MakeSimpleLevel(6, 6, 4);

            using (var cts = new CancellationTokenSource())
            {
                cts.Cancel();
                var result = adapter.Solve(level, null, cts.Token);
                Assert.That(result.status, Is.EqualTo(FlowSolveStatus.Cancelled));
            }
        }

        private sealed class CapturingSolver : IFlowPuzzleSolver
        {
            private readonly FlowSolveResult result;

            public CapturingSolver(FlowSolveResult result)
            {
                this.result = result;
            }

            public FlowSolveRequest CapturedRequest { get; private set; }

            public FlowSolveResult Solve(
                FlowSolveRequest request,
                IProgress<FlowSolveProgress> progress,
                CancellationToken cancellationToken)
            {
                CapturedRequest = request;
                return result;
            }
        }
    }
}
