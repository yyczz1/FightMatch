using System.Numerics;

namespace FightMatch.Core
{
    public enum CandidateHistoryLocatorKind { EndpointLatestAttack, LockedRouteKillingAttack }

    // Mutable input shells must remain stable during a call. Results retain only immutable values.
    public sealed class CandidateHistoryLocator
    {
        public string PlayerId { get; set; }
        public string AttemptId { get; set; }
        public BigInteger? ExpectedSceneRevision { get; set; }
        public CandidateHistoryLocatorKind? Kind { get; set; }
        public BattlePairKey Pair { get; set; }
    }

    public sealed class CandidateHistoryRangeRequest
    {
        public string PlayerId { get; set; }
        public string AttemptId { get; set; }
        public BigInteger? ExpectedSceneRevision { get; set; }
        public string HistoryAnchorId { get; set; }
    }

    public sealed class CandidateRollbackRequest
    {
        public string PlayerId { get; set; }
        public string AttemptId { get; set; }
        public string OperationId { get; set; }
        public BigInteger? ExpectedSceneRevision { get; set; }
        public string HistoryAnchorId { get; set; }
    }

    public enum CandidateHistoryRejectionCode
    {
        MissingField, InvalidValue, UnsupportedBinding, InconsistentBinding,
        StaleContext, InvalidPhase, IncompleteHistory, OperationConflict, NotFound
    }

    public enum CandidateHistoryOutcome { Created, Appended, RangeFound, RollbackPrepared, OperationFound }

    public sealed class CandidateHistoryResult
    {
        public bool IsAccepted => Outcome.HasValue;
        public CandidateHistoryOutcome? Outcome { get; }
        public CandidateBattleHistory Next { get; }
        public CandidateHistoryEntry Entry { get; }
        public CandidateRollbackRange Range { get; }
        public CandidateRollbackRecord RollbackRecord { get; }
        public CandidateHistoryOperation Operation { get; }
        public CandidateHistoryRejectionCode? RejectionCode { get; }
        public string FieldPath { get; }
        public bool CommitEligible => false;
        internal CandidateHistoryResult(CandidateHistoryOutcome outcome, CandidateBattleHistory next = null,
            CandidateHistoryEntry entry = null, CandidateRollbackRange range = null,
            CandidateRollbackRecord rollback = null, CandidateHistoryOperation operation = null)
        { Outcome = outcome; Next = next; Entry = entry; Range = range; RollbackRecord = rollback; Operation = operation; }
        internal CandidateHistoryResult(CandidateHistoryRejectionCode code, string path)
        { RejectionCode = code; FieldPath = path; }
    }
}
