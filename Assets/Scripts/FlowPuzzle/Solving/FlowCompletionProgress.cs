namespace FlowPuzzle.Solving
{
    public sealed class FlowCompletionProgress
    {
        public string phase;
        public long visitedNodes;
        public long elapsedMs;
        public int currentColorId;
        public int candidateCount;
    }
}
