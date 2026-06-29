using FlowPuzzle.Core;

namespace FlowPuzzle.Solving
{
    public sealed class FlowSolveResult
    {
        public FlowSolveStatus status;
        public FlowSolutionData solution;
        public long visitedNodes;
        public long elapsedMs;
    }
}
