using FlowPuzzle.Core;

namespace FlowPuzzle.Application
{
    public sealed class FlowPerfectCandidateScore
    {
        public float coveragePenalty;
        public float pathLengthPenalty;
        public float tierPenalty;
        public float scoreRangePenalty;
        public float detourPenalty;
        public float preferenceScore;
        public int totalDetour;
        public int totalTurnCount;
        public int totalPathCells;
        public float pathBalancePenalty;
        public float selectionScore;
        public float coverageRatio;
        public FlowDifficultyReport difficultyReport;
    }
}
