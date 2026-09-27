using System.Numerics;
using System.Collections.Generic;

namespace FightMatch.Core
{
    public sealed class CandidateProgressionRosterIntent
    {
        public string PlayerId { get; set; }
        public string LevelId { get; set; }
        public string LevelVersion { get; set; }
        public RuleContext Context { get; set; }
        public IReadOnlyList<CandidateRosterEntry> Participants { get; set; }
        public BigInteger? FormationRevision { get; set; }
        public string ChallengeId { get; set; }
        public string AttemptId { get; set; }
        public string EntryBaselineId { get; set; }
    }
    // Mutable request shells must remain unchanged during a call.
    public class CandidateProgressionLevelRequest
    {
        public string PlayerId { get; set; }
        public string LevelId { get; set; }
        public string LevelVersion { get; set; }
        public RuleContext Context { get; set; }
        public string CharacterId { get; set; }
        public BigInteger? ExpectedCharacterRevision { get; set; }
        public int? ExpectedOriginalSlot { get; set; }
    }
    public sealed class CandidateProgressionBeginIntent : CandidateProgressionLevelRequest
    {
        public string ChallengeId { get; set; }
        public string AttemptId { get; set; }
        public string EntryBaselineId { get; set; }
    }
    public enum CandidateProgressionEndKind { Unspecified, NormalVictory, NormalExit, ImmediateRestart }
    public sealed class CandidateProgressionEndFacts
    {
        private string settlementId;
        private string fingerprint;
        private string newAttemptId;
        public string PlayerId { get; set; }
        public string LevelId { get; set; }
        public string LevelVersion { get; set; }
        public RuleContext Context { get; set; }
        public string ChallengeId { get; set; }
        public string AttemptId { get; set; }
        public string EntryBaselineId { get; set; }
        public string EndReceiptId { get; set; }
        public CandidateProgressionEndKind Kind { get; set; }
        public string SettlementId { get => settlementId; set { settlementId = value; HasSettlement = true; } }
        public string FinalReportFingerprint { get => fingerprint; set { fingerprint = value; HasFingerprint = true; } }
        public string NewAttemptId { get => newAttemptId; set { newAttemptId = value; HasNewAttempt = true; } }
        internal bool HasSettlement { get; private set; }
        internal bool HasFingerprint { get; private set; }
        internal bool HasNewAttempt { get; private set; }
    }
}
