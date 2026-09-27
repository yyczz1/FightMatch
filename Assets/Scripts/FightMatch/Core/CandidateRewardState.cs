using System.Collections.Generic;
using System.Numerics;

namespace FightMatch.Core
{
    public sealed class CandidateRewardExperience
    {
        public BattleCombatantKey CombatantKey { get; }
        public string CharacterId { get; }
        public int OriginalSlot { get; }
        public BigInteger EntryLevel { get; }
        public ExactRational Dealt { get; }
        public ExactRational Taken { get; }
        public ExactRational Contribution { get; }
        public ExactRational Reference { get; }
        public ExactRational LevelMultiplier { get; }
        public ExactScoreResult Score { get; }
        public BigInteger Amount => Score.Amount;
        internal CandidateRewardExperience(PreparedMember member, BattleContributionTotals totals,
            ExactRational contribution, ExactRational reference, ExactRational multiplier, ExactScoreResult score)
        {
            CombatantKey = totals.CombatantKey; CharacterId = member.CharacterId; OriginalSlot = member.OriginalSlot;
            EntryLevel = member.Level; Dealt = totals.EffectiveDamageDealtHp; Taken = totals.EffectiveDamageTakenHp;
            Contribution = contribution; Reference = reference; LevelMultiplier = multiplier; Score = score;
        }
    }
    public enum CandidateRewardRandomUse { NotUsedFixedTable }
    public sealed class CandidateRewardRandomEvidence
    {
        public CandidateRandomDomain Domain { get; }
        public string SourceCapabilityId { get; }
        public string MappingId { get; }
        public CandidateRewardRandomUse Use => CandidateRewardRandomUse.NotUsedFixedTable;
        public Pcg32StreamState Before => Domain.Initial;
        public Pcg32StreamState After => Domain.Initial;
        public IReadOnlyList<uint> Words { get; }
        public BigInteger WordsConsumed => BigInteger.Zero;
        internal CandidateRewardRandomEvidence(CandidateRandomBinding binding)
        {
            Domain = binding.BaseReward; SourceCapabilityId = binding.SourceCapabilityId; MappingId = binding.MappingId;
            Words = new List<uint>().AsReadOnly();
        }
    }
    public sealed class CandidateFixedBaseReward
    {
        public string PlayerId => Report.PlayerId;
        public string AttemptId => Report.AttemptId;
        public string ChallengeId => Report.ChallengeId;
        public string EntryBaselineId => Report.EntryBaselineId;
        public string SettlementId => Ending.SettlementId;
        public CandidateFinalAttemptReport Report { get; }
        public string FinalReportFingerprint => Report.Fingerprint;
        public CandidateRewardDefinition Definition { get; }
        public CandidateProgressionEndReceipt Ending { get; }
        public IReadOnlyList<CandidateRewardExperience> Experience { get; }
        public IReadOnlyList<CandidateRewardMaterial> Materials => Definition.Materials;
        public CandidateConsumptionCoverage ConsumptionCoverage => Report.ConsumptionCoverage;
        public CandidateRewardRandomEvidence Random { get; }
        public bool CommitEligible => false;
        internal CandidateFixedBaseReward(CandidateFinalAttemptReport report, CandidateRewardDefinition definition,
            CandidateProgressionEndReceipt ending, IEnumerable<CandidateRewardExperience> experience)
        {
            Report = report; Definition = definition; Ending = ending; Experience = new List<CandidateRewardExperience>(experience).AsReadOnly();
            Random = new CandidateRewardRandomEvidence(report.Binding);
        }
    }
    public sealed class CandidateRewardState
    {
        public string PlayerId { get; }
        public BigInteger StateRevision { get; }
        public IReadOnlyList<CandidateFixedBaseReward> BaseRewards { get; }
        public bool CommitEligible => false;
        internal CandidateRewardState(string player, BigInteger revision, IEnumerable<CandidateFixedBaseReward> rewards)
        { PlayerId = player; StateRevision = revision; BaseRewards = new List<CandidateFixedBaseReward>(rewards).AsReadOnly(); }
    }
    public sealed class CandidateRewardResult
    {
        public bool IsAccepted => Next != null;
        public CandidateRewardState Next { get; }
        public CandidateRewardOutcome Outcome { get; }
        public CandidateFixedBaseReward BaseReward { get; }
        public CandidateRewardRejectionCode RejectionCode { get; }
        public string FieldPath { get; }
        internal CandidateRewardResult(RewardChecks check) { RejectionCode = check.Code; FieldPath = check.Path; }
        internal CandidateRewardResult(CandidateRewardState next, CandidateRewardOutcome outcome, CandidateFixedBaseReward reward = null)
        { Next = next; Outcome = outcome; BaseReward = reward; }
    }
}
