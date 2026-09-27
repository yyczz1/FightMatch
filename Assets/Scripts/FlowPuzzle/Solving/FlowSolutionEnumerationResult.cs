namespace FlowPuzzle.Solving
{
    public sealed class FlowSolutionEnumerationResult
    {
        public FlowSolveStatus status;
        public int candidateCount;
        public long visitedNodes;
        public long elapsedMs;
        public bool searchExhausted;
    }
}
