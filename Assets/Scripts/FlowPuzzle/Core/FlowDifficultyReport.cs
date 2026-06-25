using System;

namespace FlowPuzzle.Core
{
    [Serializable]
    public sealed class FlowDifficultyReport
    {
        public FlowDifficultyTier difficulty;
        public float totalScore;
        public float boardSizeScore;
        public float colorCountScore;
        public float coverageScore;
        public float turnScore;
        public float detourScore;
        public float interactionScore;
        public float endpointDistanceScore;
        public float bottleneckScore;
        public int totalTurnCount;
        public int totalDetour;
        public int differentColorAdjacentCount;
        public int totalEndpointManhattanDistance;
        public int bottleneckCount;
    }
}
