using System;
using System.Collections.Generic;

namespace FlowPuzzle.Core
{
    [Serializable]
    public sealed class FlowLevelData
    {
        public int levelId;
        public int width;
        public int height;
        public FlowDifficultyTier difficulty;
        public float difficultyScore;
        public List<FlowPairData> pairs = new List<FlowPairData>();
    }
}
