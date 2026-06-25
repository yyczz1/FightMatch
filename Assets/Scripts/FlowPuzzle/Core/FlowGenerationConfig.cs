using System;

namespace FlowPuzzle.Core
{
    [Serializable]
    public sealed class FlowGenerationConfig
    {
        public int width;
        public int height;
        public int colorCount;
        public float minCoverageRatio;
        public float maxCoverageRatio;
        public int minPathLength;
        public int maxPathLength;
        public int maxPathAttempt;
        public int maxLevelAttempt;
        public bool useRandomSeed;
        public int seed;
        public bool useTargetDifficulty;
        public FlowDifficultyTier targetDifficulty;
        public bool useTargetScoreRange;
        public float minTargetDifficultyScore;
        public float maxTargetDifficultyScore;
        public float turnPreference;
        public float interactionPreference;
        public int minEndpointDistance;
        public int maxEndpointDistance;
        public int minDetour;
        public int maxDetour;
        public float bottleneckPreference;
        public int solverTimeoutMilliseconds;
        public int solverNodeBudget;
    }
}
