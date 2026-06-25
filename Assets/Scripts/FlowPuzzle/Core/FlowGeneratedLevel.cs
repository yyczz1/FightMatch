using System;

namespace FlowPuzzle.Core
{
    [Serializable]
    public sealed class FlowGeneratedLevel
    {
        public FlowLevelData levelData = new FlowLevelData();
        public FlowSolutionData solutionData = new FlowSolutionData();
        public FlowDifficultyReport difficultyReport = new FlowDifficultyReport();
        public int usedSeed;
        public float coverageRatio;
    }
}
