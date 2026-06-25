using FlowPuzzle.Core;
using UnityEngine;

namespace FlowPuzzle.Persistence
{
    public sealed class FlowLevelAsset : ScriptableObject
    {
        public FlowLevelData levelData = new FlowLevelData();
        public FlowSolutionData solutionData = new FlowSolutionData();
        public FlowDifficultyReport difficultyReport = new FlowDifficultyReport();
        public int generationSeed;
        public float coverageRatio;
    }
}
