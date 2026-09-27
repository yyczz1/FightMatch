using System.Numerics;

namespace FightMatch.Core
{
    // Caller-owned request shell, copied into the fixed source relations only after complete validation.
    public sealed class CandidateRewardFixRequest
    {
        public string PlayerId { get; set; }
        public string AttemptId { get; set; }
        public string ChallengeId { get; set; }
        public string EntryBaselineId { get; set; }
        public string SettlementId { get; set; }
        public string FinalReportFingerprint { get; set; }
        public BigInteger? ExpectedStateRevision { get; set; }
    }
}
