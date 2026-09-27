using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FlowPuzzle.Core;
using FlowPuzzle.Difficulty;
using FlowPuzzle.Solving;

namespace FlowPuzzle.Application
{
    public sealed class FlowPerfectCompletionProvider : IFlowLevelCompletionProvider
    {
        private readonly FlowDifficultyEvaluator evaluator;

        public FlowPerfectCompletionProvider(FlowDifficultyEvaluator evaluator)
        {
            this.evaluator = evaluator ?? throw new ArgumentNullException(nameof(evaluator));
        }

        public string DisplayName => "Local Perfect Solver";

        public async Task<FlowCompletionResult> CompleteAsync(
            FlowCompletionRequest request,
            IProgress<FlowCompletionProgress> progress,
            CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return Failure(FlowSolveStatus.Cancelled, "Cancelled", "Perfect completion was cancelled before it started.");
            if (request?.levelData == null || request.qualityConfig == null)
                return Failure(FlowSolveStatus.InvalidInput, "MissingQualityConfig", "Perfect completion requires level data and quality parameters.");

            var snapshot = Snapshot(request);
            return await Task.Run(() => FindBest(snapshot, progress, cancellationToken), CancellationToken.None)
                .ConfigureAwait(false);
        }

        private FlowCompletionResult FindBest(
            FlowCompletionRequest request,
            IProgress<FlowCompletionProgress> progress,
            CancellationToken cancellationToken)
        {
            var scorer = new FlowPerfectCandidateScorer(request.levelData, request.qualityConfig, evaluator);
            var bestSolution = CloneSolution(request.currentSolution);
            var bestScore = bestSolution != null ? scorer.Evaluate(bestSolution) : null;
            var bestIsCurrentSolution = bestSolution != null;
            var solver = new BacktrackingFlowPuzzleSolver();
            var solveRequest = new FlowSolveRequest
            {
                levelData = request.levelData,
                fixedPrefixes = request.fixedPrefixes,
                nodeBudget = request.nodeBudget,
                timeoutMs = request.timeoutMs,
                progressIntervalNodes = request.progressIntervalNodes
            };

            var enumeration = solver.EnumerateSolutions(
                solveRequest,
                candidate =>
                {
                    var score = scorer.Evaluate(candidate);
                    var comparison = scorer.Compare(score, bestScore);
                    if (comparison > 0
                        || comparison == 0 && !bestIsCurrentSolution && IsCanonicalEarlier(candidate, bestSolution))
                    {
                        bestSolution = CloneSolution(candidate);
                        bestScore = score;
                        bestIsCurrentSolution = false;
                    }
                },
                new CompletionProgressAdapter(progress),
                cancellationToken);

            if (enumeration.status == FlowSolveStatus.Cancelled)
                return Failure(FlowSolveStatus.Cancelled, "Cancelled", "Perfect completion was cancelled.", enumeration);

            if (bestSolution != null)
            {
                return new FlowCompletionResult
                {
                    status = FlowSolveStatus.Solved,
                    generatedLevel = new FlowGeneratedLevel
                    {
                        levelData = CloneLevel(request.levelData),
                        solutionData = bestSolution,
                        difficultyReport = bestScore.difficultyReport,
                        coverageRatio = bestScore.coverageRatio,
                        usedSeed = 0
                    },
                    visitedNodes = enumeration.visitedNodes,
                    elapsedMs = enumeration.elapsedMs,
                    candidateCount = enumeration.candidateCount,
                    candidateScore = bestScore.selectionScore,
                    candidateTotalPathCells = bestScore.totalPathCells,
                    candidateTotalDetour = bestScore.totalDetour,
                    candidateTotalTurns = bestScore.totalTurnCount,
                    searchLimitReached = enumeration.status == FlowSolveStatus.Timeout
                };
            }

            return Failure(
                enumeration.status,
                enumeration.status == FlowSolveStatus.Timeout ? "SolverLimitReached" : "NoSolution",
                enumeration.status == FlowSolveStatus.Timeout
                    ? "Perfect completion reached its solver limit before finding a legal candidate."
                    : "No legal solution exists for the current endpoints and fixed constraints.",
                enumeration);
        }

        private static FlowCompletionResult Failure(
            FlowSolveStatus status,
            string code,
            string message,
            FlowSolutionEnumerationResult enumeration = null)
        {
            return new FlowCompletionResult
            {
                status = status,
                errorCode = code,
                errorMessage = message,
                candidateCount = enumeration?.candidateCount ?? 0,
                visitedNodes = enumeration?.visitedNodes ?? 0,
                elapsedMs = enumeration?.elapsedMs ?? 0,
                searchLimitReached = enumeration?.status == FlowSolveStatus.Timeout
            };
        }

        private static bool IsCanonicalEarlier(FlowSolutionData candidate, FlowSolutionData currentBest)
        {
            if (candidate == null) return false;
            if (currentBest == null) return true;
            var left = candidate.paths.OrderBy(path => path.colorId).ToList();
            var right = currentBest.paths.OrderBy(path => path.colorId).ToList();
            for (var pathIndex = 0; pathIndex < Math.Min(left.Count, right.Count); pathIndex++)
            {
                if (left[pathIndex].colorId != right[pathIndex].colorId)
                    return left[pathIndex].colorId < right[pathIndex].colorId;
                var leftCells = left[pathIndex].cells;
                var rightCells = right[pathIndex].cells;
                for (var cellIndex = 0; cellIndex < Math.Min(leftCells.Count, rightCells.Count); cellIndex++)
                {
                    if (leftCells[cellIndex].x != rightCells[cellIndex].x)
                        return leftCells[cellIndex].x < rightCells[cellIndex].x;
                    if (leftCells[cellIndex].y != rightCells[cellIndex].y)
                        return leftCells[cellIndex].y < rightCells[cellIndex].y;
                }
                if (leftCells.Count != rightCells.Count)
                    return leftCells.Count < rightCells.Count;
            }
            return left.Count < right.Count;
        }

        private static FlowCompletionRequest Snapshot(FlowCompletionRequest request)
        {
            return new FlowCompletionRequest
            {
                levelData = CloneLevel(request.levelData),
                currentSolution = CloneSolution(request.currentSolution),
                fixedPrefixes = ClonePaths(request.fixedPrefixes),
                qualityConfig = CloneConfig(request.qualityConfig),
                timeoutMs = request.timeoutMs,
                nodeBudget = request.nodeBudget,
                progressIntervalNodes = request.progressIntervalNodes
            };
        }

        private static FlowGenerationConfig CloneConfig(FlowGenerationConfig source)
        {
            return new FlowGenerationConfig
            {
                width = source.width,
                height = source.height,
                colorCount = source.colorCount,
                minCoverageRatio = source.minCoverageRatio,
                maxCoverageRatio = source.maxCoverageRatio,
                minPathLength = source.minPathLength,
                maxPathLength = source.maxPathLength,
                maxPathAttempt = source.maxPathAttempt,
                maxLevelAttempt = source.maxLevelAttempt,
                useRandomSeed = source.useRandomSeed,
                seed = source.seed,
                useTargetDifficulty = source.useTargetDifficulty,
                targetDifficulty = source.targetDifficulty,
                useTargetScoreRange = source.useTargetScoreRange,
                minTargetDifficultyScore = source.minTargetDifficultyScore,
                maxTargetDifficultyScore = source.maxTargetDifficultyScore,
                turnPreference = source.turnPreference,
                interactionPreference = source.interactionPreference,
                minEndpointDistance = source.minEndpointDistance,
                maxEndpointDistance = source.maxEndpointDistance,
                minDetour = source.minDetour,
                maxDetour = source.maxDetour,
                bottleneckPreference = source.bottleneckPreference,
                solverTimeoutMilliseconds = source.solverTimeoutMilliseconds,
                solverNodeBudget = source.solverNodeBudget
            };
        }

        private static FlowLevelData CloneLevel(FlowLevelData source)
        {
            var copy = new FlowLevelData
            {
                levelId = source.levelId,
                width = source.width,
                height = source.height,
                difficulty = source.difficulty,
                difficultyScore = source.difficultyScore
            };
            foreach (var pair in source.pairs)
            {
                copy.pairs.Add(new FlowPairData
                {
                    colorId = pair.colorId,
                    endpointA = new FlowPos(pair.endpointA.x, pair.endpointA.y),
                    endpointB = new FlowPos(pair.endpointB.x, pair.endpointB.y)
                });
            }
            return copy;
        }

        private static FlowSolutionData CloneSolution(FlowSolutionData source)
        {
            if (source == null) return null;
            return new FlowSolutionData
            {
                levelId = source.levelId,
                paths = ClonePaths(source.paths)
            };
        }

        private static List<FlowPathData> ClonePaths(List<FlowPathData> source)
        {
            if (source == null) return null;
            var copy = new List<FlowPathData>(source.Count);
            foreach (var path in source)
            {
                copy.Add(new FlowPathData
                {
                    colorId = path.colorId,
                    cells = path.cells == null
                        ? null
                        : path.cells.Select(cell => new FlowPos(cell.x, cell.y)).ToList()
                });
            }
            return copy;
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
                progress?.Report(new FlowCompletionProgress
                {
                    phase = value.phase,
                    visitedNodes = value.visitedNodes,
                    elapsedMs = value.elapsedMs,
                    currentColorId = value.currentColorId,
                    candidateCount = value.candidateCount
                });
            }
        }
    }
}
