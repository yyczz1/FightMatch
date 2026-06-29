using FlowPuzzle.Core;

namespace FlowPuzzle.Solving
{
    public sealed class FlowCompletionRequest
    {
        public FlowLevelData levelData;
        public FlowSolutionData currentSolution;
        public int timeoutMs = 10000;
        public long nodeBudget = 10000000;
    }
}
