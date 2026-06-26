using FlowPuzzle.Core;
using FlowPuzzle.Difficulty;
using FlowPuzzle.Generation;
using FlowPuzzle.Validation;

namespace FlowPuzzle.Application
{
    public sealed class FlowLevelGenerationService
    {
        private readonly FlowSolutionGenerator generator;
        private readonly FlowBatchGenerator batchGenerator;
        private readonly FlowSolutionValidator validator;

        public FlowLevelGenerationService()
        {
            var allocator = new FlowPathLengthAllocator();
            var pathStrategy = new RandomizedDfsPathGenerationStrategy();
            validator = new FlowSolutionValidator();
            var difficultyEvaluator = new FlowDifficultyEvaluator();
            generator = new FlowSolutionGenerator(allocator, pathStrategy, validator, difficultyEvaluator);
            batchGenerator = new FlowBatchGenerator(generator);
        }

        public FlowGenerationResult GenerateOne(
            int levelId,
            FlowGenerationConfig config)
        {
            return generator.Generate(levelId, config);
        }

        public FlowBatchReport GenerateBatch(
            FlowBatchRequest request)
        {
            return batchGenerator.Generate(request);
        }

        public FlowValidationResult Validate(
            FlowGeneratedLevel level)
        {
            if (level == null)
                return FlowValidationResult.Invalid("NullLevel", "Level is null.");
            return validator.Validate(level.levelData, level.solutionData);
        }
    }
}
