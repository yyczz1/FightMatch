using System.Linq;
using FlowPuzzle.Core;
using NUnit.Framework;

namespace FlowPuzzle.Tests.Core
{
    [TestFixture]
    public class FlowDiagnosticMapperTests
    {
        private static FlowGenerationConfig MakeConfig(
            int width = 6, int height = 6, int colorCount = 4,
            int minPath = 3, int maxPath = 12,
            int maxPathAttempt = 100, int maxLevelAttempt = 50,
            float minCoverage = 0.3f, float maxCoverage = 0.8f,
            int seed = 42,
            float turnPreference = 0.5f,
            int solverTimeoutMilliseconds = 5000,
            int solverNodeBudget = 100000)
        {
            return new FlowGenerationConfig
            {
                width = width,
                height = height,
                colorCount = colorCount,
                minPathLength = minPath,
                maxPathLength = maxPath,
                maxPathAttempt = maxPathAttempt,
                maxLevelAttempt = maxLevelAttempt,
                minCoverageRatio = minCoverage,
                maxCoverageRatio = maxCoverage,
                seed = seed,
                turnPreference = turnPreference,
                solverTimeoutMilliseconds = solverTimeoutMilliseconds,
                solverNodeBudget = solverNodeBudget
            };
        }

        [Test]
        public void InvalidDimensions_ProducesMultipleSuggestions()
        {
            var config = MakeConfig(width: 0, height: 0, colorCount: 0);
            var suggestions = FlowDiagnosticMapper.Map(FlowDiagnosticCodes.InvalidDimensions, config);

            Assert.That(suggestions.Count, Is.GreaterThanOrEqualTo(3),
                "should suggest fixing width, height, and colorCount");

            Assert.That(suggestions.Any(s => s.parameterName == "width"), "should mention width");
            Assert.That(suggestions.Any(s => s.parameterName == "height"), "should mention height");
            Assert.That(suggestions.Any(s => s.parameterName == "colorCount"), "should mention colorCount");

            Assert.That(suggestions.All(s => s.suggestedDirection == "Positive"),
                "all suggestions should be positive direction for zero params");
        }

        [Test]
        public void InvalidDimensions_NegativeValues_SuggestsValidPositiveValues()
        {
            var config = MakeConfig(width: -20, height: -10, colorCount: -3);
            var suggestions = FlowDiagnosticMapper.Map(FlowDiagnosticCodes.InvalidDimensions, config);

            var width = suggestions.Single(s => s.parameterName == "width");
            var height = suggestions.Single(s => s.parameterName == "height");
            var colorCount = suggestions.Single(s => s.parameterName == "colorCount");

            Assert.That(int.Parse(width.suggestedValue), Is.GreaterThan(0));
            Assert.That(int.Parse(height.suggestedValue), Is.GreaterThan(0));
            Assert.That(int.Parse(colorCount.suggestedValue), Is.GreaterThan(0));
        }

        [Test]
        public void ImpossibleMinimumOccupancy_SuggestsReduceColorsEnlargeBoard()
        {
            var config = MakeConfig(width: 4, height: 4, colorCount: 8, minPath: 3);
            var suggestions = FlowDiagnosticMapper.Map(FlowDiagnosticCodes.ImpossibleMinimumOccupancy, config);

            Assert.That(suggestions.Count, Is.GreaterThanOrEqualTo(2));

            Assert.That(suggestions.Any(s =>
                s.parameterName == "colorCount" && s.suggestedDirection == "Decrease"),
                "should suggest decreasing colorCount");
            Assert.That(suggestions.Any(s =>
                s.parameterName == "width" && s.suggestedDirection == "Increase"),
                "should suggest increasing board width");
        }

        [Test]
        public void ImpossibleCoverageRange_SuggestsAdjustingCoverage()
        {
            var config = MakeConfig(minCoverage: 0.9f, maxCoverage: 0.2f);
            var suggestions = FlowDiagnosticMapper.Map(FlowDiagnosticCodes.ImpossibleCoverageRange, config);

            Assert.That(suggestions.Count, Is.GreaterThanOrEqualTo(1));
            Assert.That(suggestions.Any(s => s.parameterName == "minCoverageRatio"),
                "should mention minCoverageRatio");
        }

        [Test]
        public void PathGenerationFailed_SuggestsLargerBoardMoreAttempts()
        {
            var config = MakeConfig(maxPathAttempt: 10, maxCoverage: 0.9f,
                width: 4, height: 4, turnPreference: 1.0f);
            var suggestions = FlowDiagnosticMapper.Map(FlowDiagnosticCodes.PathGenerationFailed, config);

            Assert.That(suggestions.Count, Is.GreaterThanOrEqualTo(2));
            Assert.That(suggestions.Any(s =>
                s.parameterName == "maxPathAttempt" && s.suggestedDirection == "Increase"),
                "should suggest increasing maxPathAttempt");
            Assert.That(suggestions.Any(s =>
                s.parameterName == "turnPreference" && s.suggestedDirection == "Decrease"),
                "should suggest decreasing turnPreference");
        }

        [Test]
        public void MaxLevelAttemptsReached_SuggestsMoreAttemptsEasierConfig()
        {
            var config = MakeConfig(maxLevelAttempt: 5, width: 4, height: 4, colorCount: 6);
            var suggestions = FlowDiagnosticMapper.Map(FlowDiagnosticCodes.MaxLevelAttemptsReached, config);

            Assert.That(suggestions.Count, Is.GreaterThanOrEqualTo(1));
            Assert.That(suggestions.Any(s =>
                s.parameterName == "maxLevelAttempt" && s.suggestedDirection == "Increase"),
                "should suggest increasing maxLevelAttempt");
        }

        [Test]
        public void SolverTimeout_SuggestsIncreasingBudget()
        {
            var config = MakeConfig(solverTimeoutMilliseconds: 2000, solverNodeBudget: 5000);
            var suggestions = FlowDiagnosticMapper.Map(FlowDiagnosticCodes.SolverTimeout, config);

            Assert.That(suggestions.Count, Is.GreaterThanOrEqualTo(2));
            Assert.That(suggestions.Any(s =>
                s.parameterName == "solverTimeoutMilliseconds" && s.suggestedDirection == "Increase"),
                "should suggest increasing timeout");
            Assert.That(suggestions.Any(s =>
                s.parameterName == "solverNodeBudget" && s.suggestedDirection == "Increase"),
                "should suggest increasing node budget");
        }

        [Test]
        public void SolverCancelled_ProducesNoSuggestions()
        {
            var config = MakeConfig();
            var suggestions = FlowDiagnosticMapper.Map(FlowDiagnosticCodes.SolverCancelled, config);

            Assert.That(suggestions.Count, Is.EqualTo(0),
                "user cancellation should not produce parameter suggestions");
        }

        [Test]
        public void NoSolution_SuggestsRemovingConstraints()
        {
            var config = MakeConfig();
            var suggestions = FlowDiagnosticMapper.Map(FlowDiagnosticCodes.NoSolution, config);

            Assert.That(suggestions.Count, Is.GreaterThanOrEqualTo(1));
            Assert.That(suggestions.Any(s => s.parameterName == "constraint"),
                "should mention constraint");
        }

        [Test]
        public void DifficultyOutOfRange_WithTargetDifficulty_SuggestsAdjustingTarget()
        {
            var config = MakeConfig();
            config.useTargetDifficulty = true;
            config.targetDifficulty = FlowDifficultyTier.Hard;
            var suggestions = FlowDiagnosticMapper.Map(FlowDiagnosticCodes.DifficultyOutOfRange, config);

            Assert.That(suggestions.Any(s =>
                s.parameterName == "targetDifficulty" && s.suggestedDirection == "Adjust"),
                "should suggest adjusting target difficulty");
        }

        [Test]
        public void DifficultyOutOfRange_WithScoreRange_SuggestsWiderRange()
        {
            var config = MakeConfig();
            config.useTargetScoreRange = true;
            config.minTargetDifficultyScore = 70f;
            config.maxTargetDifficultyScore = 80f;
            var suggestions = FlowDiagnosticMapper.Map(FlowDiagnosticCodes.DifficultyOutOfRange, config);

            Assert.That(suggestions.Any(s =>
                s.parameterName == "minTargetDifficultyScore" && s.suggestedDirection == "Decrease"),
                "should suggest lowering min score");
            Assert.That(suggestions.Any(s =>
                s.parameterName == "maxTargetDifficultyScore" && s.suggestedDirection == "Increase"),
                "should suggest raising max score");
        }

        [Test]
        public void NullConfig_ReturnsEmptyList()
        {
            var suggestions = FlowDiagnosticMapper.Map(FlowDiagnosticCodes.InvalidDimensions, null);
            Assert.That(suggestions.Count, Is.EqualTo(0));
        }

        [Test]
        public void UnknownCode_ReturnsEmptyList()
        {
            var config = MakeConfig();
            var suggestions = FlowDiagnosticMapper.Map("SomeUnknownCode", config);
            Assert.That(suggestions.Count, Is.EqualTo(0));
        }

        [Test]
        public void AssetAlreadyExists_SuggestsChangeNameOrOverwrite()
        {
            var config = MakeConfig();
            var suggestions = FlowDiagnosticMapper.Map(FlowDiagnosticCodes.AssetAlreadyExists, config);

            Assert.That(suggestions.Count, Is.GreaterThanOrEqualTo(1));
            Assert.That(suggestions.Any(s => s.parameterName == "assetName"),
                "should mention assetName");
        }

        [Test]
        public void InvalidOutputFolder_SuggestsValidPath()
        {
            var config = MakeConfig();
            var suggestions = FlowDiagnosticMapper.Map(FlowDiagnosticCodes.InvalidOutputFolder, config);

            Assert.That(suggestions.Count, Is.GreaterThanOrEqualTo(1));
            Assert.That(suggestions.Any(s => s.parameterName == "outputFolder"),
                "should mention outputFolder");
        }

        [Test]
        public void FailureDiagnostic_WithConfig_AutoPopulatesSuggestions()
        {
            var config = MakeConfig(width: 0, height: 0, colorCount: 0);
            var result = FlowGenerationResult.Failure(0, 42, 0,
                FlowDiagnosticCodes.InvalidDimensions,
                "Bad dimensions.", config);

            Assert.That(result.diagnostic, Is.Not.Null);
            Assert.That(result.diagnostic.suggestions.Count, Is.GreaterThan(0),
                "diagnostic should have auto-populated suggestions");
            Assert.That(result.diagnostic.errorCode,
                Is.EqualTo(FlowDiagnosticCodes.InvalidDimensions));
        }

        [Test]
        public void FailureDiagnostic_WithoutConfig_HasEmptySuggestions()
        {
            var result = FlowGenerationResult.Failure(0, 42, 0,
                FlowDiagnosticCodes.InvalidDimensions,
                "Bad dimensions.");

            Assert.That(result.diagnostic, Is.Not.Null);
            Assert.That(result.diagnostic.suggestions.Count, Is.EqualTo(0),
                "diagnostic without config should have empty suggestions");
        }

        [Test]
        public void Generator_FailureResult_HasDiagnosticSuggestions()
        {
            var config = MakeConfig(width: 0, height: 0, colorCount: 0);
            // Create generator through its dependencies (same as production wiring)
            var allocator = new FlowPuzzle.Generation.FlowPathLengthAllocator();
            var pathStrategy = new FlowPuzzle.Generation.RandomizedDfsPathGenerationStrategy();
            var validator = new FlowPuzzle.Validation.FlowSolutionValidator();
            var evaluator = new FlowPuzzle.Difficulty.FlowDifficultyEvaluator();
            var generator = new FlowPuzzle.Generation.FlowSolutionGenerator(
                allocator, pathStrategy, validator, evaluator);

            var result = generator.Generate(0, config);

            Assert.That(result.success, Is.False);
            Assert.That(result.diagnostic, Is.Not.Null);
            Assert.That(result.diagnostic.suggestions.Count, Is.GreaterThan(0),
                "real generator failure should produce diagnostic suggestions");
        }
    }
}
