using System;
using System.Collections.Generic;

namespace FlowPuzzle.Core
{
    /// <summary>
    /// Maps diagnostic error codes to human-readable parameter suggestions.
    /// Suggestions are derived from the error code and the originating configuration.
    /// Probabilistic failures only produce candidate suggestions — no claim of guaranteed cause.
    /// </summary>
    public static class FlowDiagnosticMapper
    {
        public static List<FlowParameterSuggestion> Map(
            string errorCode,
            FlowGenerationConfig config)
        {
            var suggestions = new List<FlowParameterSuggestion>();

            if (config == null)
                return suggestions;

            switch (errorCode)
            {
                case FlowDiagnosticCodes.InvalidDimensions:
                    suggestions.Add(ConfigParam("width", config.width.ToString(),
                        "Positive", Math.Max(1, config.width + 5).ToString(), "set positive width"));
                    suggestions.Add(ConfigParam("height", config.height.ToString(),
                        "Positive", Math.Max(1, config.height + 5).ToString(), "set positive height"));
                    suggestions.Add(ConfigParam("colorCount", config.colorCount.ToString(),
                        "Positive", Math.Max(1, config.colorCount + 2).ToString(), "set positive colorCount"));
                    break;

                case FlowDiagnosticCodes.ImpossibleMinimumOccupancy:
                    suggestions.Add(ConfigParam("colorCount", config.colorCount.ToString(),
                        "Decrease", (Math.Max(1, config.colorCount - 1)).ToString(),
                        "reduce colors to fit board"));
                    suggestions.Add(ConfigParam("minPathLength", config.minPathLength.ToString(),
                        "Decrease", (Math.Max(1, config.minPathLength - 1)).ToString(),
                        "reduce minimum path lengths"));
                    suggestions.Add(ConfigParam("width", config.width.ToString(),
                        "Increase", (config.width + 1).ToString(), "enlarge board"));
                    suggestions.Add(ConfigParam("height", config.height.ToString(),
                        "Increase", (config.height + 1).ToString(), "enlarge board"));
                    break;

                case FlowDiagnosticCodes.ImpossibleCoverageRange:
                    suggestions.Add(ConfigParam("minCoverageRatio", config.minCoverageRatio.ToString(),
                        "Decrease", (Math.Max(0f, config.minCoverageRatio - 0.1f)).ToString("F2"),
                        "lower minimum coverage"));
                    suggestions.Add(ConfigParam("maxCoverageRatio", config.maxCoverageRatio.ToString(),
                        "Increase", (Math.Min(1f, config.maxCoverageRatio + 0.1f)).ToString("F2"),
                        "raise maximum coverage"));
                    break;

                case FlowDiagnosticCodes.PathGenerationFailed:
                    suggestions.Add(ConfigParam("maxPathAttempt", config.maxPathAttempt.ToString(),
                        "Increase", (config.maxPathAttempt + 50).ToString(),
                        "more attempts per path"));
                    suggestions.Add(ConfigParam("maxCoverageRatio", config.maxCoverageRatio.ToString(),
                        "Decrease", (Math.Max(0.1f, config.maxCoverageRatio - 0.05f)).ToString("F2"),
                        "lower coverage eases path finding"));
                    suggestions.Add(ConfigParam("turnPreference", config.turnPreference.ToString("F2"),
                        "Decrease", "0.0", "reduce turns for straighter paths"));
                    suggestions.Add(ConfigParam("width", config.width.ToString(),
                        "Increase", (config.width + 1).ToString(), "enlarge board"));
                    break;

                case FlowDiagnosticCodes.CoverageOutOfRange:
                    suggestions.Add(ConfigParam("minCoverageRatio", config.minCoverageRatio.ToString(),
                        "Decrease", "0.1", "relax minimum coverage"));
                    suggestions.Add(ConfigParam("maxCoverageRatio", config.maxCoverageRatio.ToString(),
                        "Increase", "0.9", "relax maximum coverage"));
                    break;

                case FlowDiagnosticCodes.ValidationFailed:
                    suggestions.Add(new FlowParameterSuggestion
                    {
                        parameterName = "seed",
                        currentValue = config.seed.ToString(),
                        suggestedDirection = "Retry",
                        reason = "validation failed — retry with a different seed"
                    });
                    break;

                case FlowDiagnosticCodes.DifficultyOutOfRange:
                    if (config.useTargetDifficulty)
                    {
                        suggestions.Add(ConfigParam("targetDifficulty", config.targetDifficulty.ToString(),
                            "Adjust", "Normal", "relax target difficulty"));
                    }
                    if (config.useTargetScoreRange)
                    {
                        suggestions.Add(ConfigParam("minTargetDifficultyScore",
                            config.minTargetDifficultyScore.ToString("F1"), "Decrease",
                            (Math.Max(0f, config.minTargetDifficultyScore - 5f)).ToString("F1"),
                            "widen score range"));
                        suggestions.Add(ConfigParam("maxTargetDifficultyScore",
                            config.maxTargetDifficultyScore.ToString("F1"), "Increase",
                            (config.maxTargetDifficultyScore + 5f).ToString("F1"),
                            "widen score range"));
                    }
                    suggestions.Add(ConfigParam("colorCount", config.colorCount.ToString(),
                        "Decrease", (Math.Max(2, config.colorCount - 1)).ToString(),
                        "fewer colors reduce difficulty"));
                    break;

                case FlowDiagnosticCodes.MaxLevelAttemptsReached:
                    suggestions.Add(ConfigParam("maxLevelAttempt", config.maxLevelAttempt.ToString(),
                        "Increase", (config.maxLevelAttempt * 2).ToString(),
                        "more level attempts"));
                    suggestions.Add(ConfigParam("width", config.width.ToString(),
                        "Increase", (config.width + 1).ToString(), "enlarge board"));
                    suggestions.Add(ConfigParam("colorCount", config.colorCount.ToString(),
                        "Decrease", (Math.Max(2, config.colorCount - 1)).ToString(),
                        "fewer colors ease generation"));
                    break;

                case FlowDiagnosticCodes.InvalidFixedConstraint:
                    suggestions.Add(new FlowParameterSuggestion
                    {
                        parameterName = "constraint",
                        suggestedDirection = "RemoveOrFix",
                        reason = "fixed constraint is invalid — remove or adjust in draft editor"
                    });
                    break;

                case FlowDiagnosticCodes.SolverTimeout:
                    suggestions.Add(ConfigParam("solverTimeoutMilliseconds",
                        config.solverTimeoutMilliseconds.ToString(),
                        "Increase", (config.solverTimeoutMilliseconds * 2).ToString(),
                        "increase solver time budget"));
                    suggestions.Add(ConfigParam("solverNodeBudget",
                        config.solverNodeBudget.ToString(),
                        "Increase", (config.solverNodeBudget * 2).ToString(),
                        "increase solver node budget"));
                    suggestions.Add(ConfigParam("colorCount", config.colorCount.ToString(),
                        "Decrease", (Math.Max(2, config.colorCount - 1)).ToString(),
                        "fewer colors reduce search space"));
                    break;

                case FlowDiagnosticCodes.SolverCancelled:
                    // No actionable parameter suggestions for user cancellation.
                    break;

                case FlowDiagnosticCodes.NoSolution:
                    suggestions.Add(new FlowParameterSuggestion
                    {
                        parameterName = "constraint",
                        suggestedDirection = "Remove",
                        reason = "no solution exists — remove fixed constraints or reduce endpoint count"
                    });
                    break;

                case FlowDiagnosticCodes.AssetAlreadyExists:
                    suggestions.Add(new FlowParameterSuggestion
                    {
                        parameterName = "assetName",
                        suggestedDirection = "ChangeOrOverwrite",
                        reason = "asset already exists — use a different name or enable overwrite"
                    });
                    break;

                case FlowDiagnosticCodes.InvalidOutputFolder:
                    suggestions.Add(new FlowParameterSuggestion
                    {
                        parameterName = "outputFolder",
                        suggestedDirection = "SetValid",
                        reason = "output folder is invalid — set a valid writable path"
                    });
                    break;

                default:
                    // Unknown code: no suggestions.
                    break;
            }

            return suggestions;
        }

        private static FlowParameterSuggestion ConfigParam(
            string parameterName,
            string currentValue,
            string suggestedDirection,
            string suggestedValue,
            string reason)
        {
            return new FlowParameterSuggestion
            {
                parameterName = parameterName,
                currentValue = currentValue,
                suggestedDirection = suggestedDirection,
                suggestedValue = suggestedValue,
                reason = reason
            };
        }
    }
}
