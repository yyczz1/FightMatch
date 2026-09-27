using System;
using System.Threading;
using System.Threading.Tasks;
using FlowPuzzle.Core;
using FlowPuzzle.Difficulty;
using FlowPuzzle.Solving;
using FlowPuzzle.Validation;

namespace FlowPuzzle.Application
{
    public sealed class FlowLevelCompletionService
    {
        private readonly IFlowLevelCompletionProvider provider;
        private readonly FlowSolutionValidator validator;
        private readonly FlowDifficultyEvaluator evaluator;

        public FlowLevelCompletionService(IFlowLevelCompletionProvider provider, FlowSolutionValidator validator, FlowDifficultyEvaluator evaluator)
        {
            this.provider = provider ?? throw new ArgumentNullException(nameof(provider));
            this.validator = validator ?? throw new ArgumentNullException(nameof(validator));
            this.evaluator = evaluator ?? throw new ArgumentNullException(nameof(evaluator));
        }

        public async Task<FlowCompletionResult> CompleteAsync(FlowCompletionRequest request, IProgress<FlowCompletionProgress> progress, CancellationToken ct)
        {
            FlowCompletionResult result;
            try
            {
                result = await provider.CompleteAsync(request, progress, ct);
            }
            catch (OperationCanceledException)
            {
                return new FlowCompletionResult
                {
                    status = FlowSolveStatus.Cancelled,
                    errorCode = "Cancelled",
                    errorMessage = "Completion was cancelled."
                };
            }
            catch (Exception ex)
            {
                return new FlowCompletionResult
                {
                    status = FlowSolveStatus.Error,
                    errorCode = "ProviderException",
                    errorMessage = ex.Message
                };
            }

            if (result == null)
                return new FlowCompletionResult
                {
                    status = FlowSolveStatus.Error,
                    errorCode = "NullProviderResult",
                    errorMessage = "Completion provider returned no result."
                };

            if (result.status == FlowSolveStatus.Solved && result.generatedLevel != null)
            {
                var v = validator.Validate(result.generatedLevel.levelData, result.generatedLevel.solutionData);
                if (!v.isValid)
                {
                    result.status = FlowSolveStatus.Error;
                    result.errorCode = "InvalidCompletedSolution";
                    result.errorMessage = $"{v.errorCode}: {v.errorMessage}";
                    return result;
                }
                var report = evaluator.Evaluate(result.generatedLevel.levelData, result.generatedLevel.solutionData);
                result.generatedLevel.difficultyReport = report;
                result.generatedLevel.levelData.difficulty = report.difficulty;
                result.generatedLevel.levelData.difficultyScore = report.totalScore;
            }
            return result;
        }
    }
}
