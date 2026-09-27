using System.Collections.Generic;
using FlowPuzzle.Core;

namespace FlowPuzzle.Solving
{
    public sealed class FlowCompletionRequest
    {
        public FlowLevelData levelData;
        public FlowSolutionData currentSolution;
        public List<FlowPathData> fixedPrefixes;
        public FlowGenerationConfig qualityConfig;
        public int timeoutMs = FlowSolveRequest.DefaultTimeoutMs;
        public long nodeBudget = FlowSolveRequest.DefaultNodeBudget;
        public int progressIntervalNodes = FlowSolveRequest.DefaultProgressIntervalNodes;
    }
}
