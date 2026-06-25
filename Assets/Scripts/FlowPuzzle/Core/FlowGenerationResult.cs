using System;

namespace FlowPuzzle.Core
{
    [Serializable]
    public sealed class FlowGenerationResult
    {
        public bool success;
        public int levelId;
        public int usedSeed;
        public int attemptCount;
        public FlowGeneratedLevel generatedLevel;
        public FlowFailureDiagnostic diagnostic;

        public static FlowGenerationResult Success(
            int levelId,
            int usedSeed,
            int attemptCount,
            FlowGeneratedLevel generatedLevel)
        {
            return new FlowGenerationResult
            {
                success = true,
                levelId = levelId,
                usedSeed = usedSeed,
                attemptCount = attemptCount,
                generatedLevel = generatedLevel,
                diagnostic = null
            };
        }

        public static FlowGenerationResult Failure(
            int levelId,
            int usedSeed,
            int attemptCount,
            string errorCode,
            string errorMessage)
        {
            return new FlowGenerationResult
            {
                success = false,
                levelId = levelId,
                usedSeed = usedSeed,
                attemptCount = attemptCount,
                generatedLevel = null,
                diagnostic = new FlowFailureDiagnostic
                {
                    errorCode = errorCode,
                    errorMessage = errorMessage,
                    usedSeed = usedSeed,
                    attemptCount = attemptCount
                }
            };
        }
    }
}
