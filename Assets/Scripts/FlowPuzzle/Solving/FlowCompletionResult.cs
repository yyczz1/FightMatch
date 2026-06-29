using FlowPuzzle.Core;

namespace FlowPuzzle.Solving
{
    public sealed class FlowCompletionResult
    {
        public FlowSolveStatus status;
        public FlowGeneratedLevel generatedLevel;
        public long visitedNodes;
        public long elapsedMs;
    }
}
