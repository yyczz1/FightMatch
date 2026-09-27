using FlowPuzzle.Core;

namespace FlowPuzzle.Solving
{
    public sealed class FlowCompletionResult
    {
        public FlowSolveStatus status;
        public FlowGeneratedLevel generatedLevel;
        public long visitedNodes;
        public long elapsedMs;
        public string errorCode;
        public string errorMessage;
        public int candidateCount;
        public float candidateScore;
        public int candidateTotalPathCells;
        public int candidateTotalDetour;
        public int candidateTotalTurns;
        public bool searchLimitReached;
    }
}
