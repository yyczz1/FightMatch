using System.Collections.Generic;
using System.Numerics;

namespace FightMatch.Core
{
    public enum CandidateGrowthOutcome { Rejected, Applied, AlreadyIncluded, Unchanged, IgnoredTimeRegression }

    // New isolated candidate only. This is not a saved-character loader or repair path.
    public sealed class CandidateCharacterState
    {
        public string PlayerId { get; }
        public string CharacterId { get; }
        public string ClassId => Definition.ClassId;
        public CandidateGrowthDefinition Definition { get; }
        public BigInteger Level { get; }
        public BigInteger Experience { get; }
        public int OriginalSlot { get; }
        public BigInteger StateRevision { get; }
        public IReadOnlyList<CandidateBaseExperienceReceipt> BaseRewards { get; }
        public IReadOnlyList<CandidateCharacterEndReceipt> ProcessedEnds { get; }
        public IReadOnlyList<CandidateRecoveryPeriod> RecoveryPeriods { get; }
        public CandidateRecoveryPeriod ActiveRecovery
        {
            get
            {
                foreach (var period in RecoveryPeriods) if (!period.IsCompleted) return period;
                return null;
            }
        }
        public bool IsReady => ActiveRecovery == null;

        internal CandidateCharacterState(CandidateGrowthDefinition definition, string player, string character,
            BigInteger level, BigInteger experience, int slot, BigInteger revision,
            IEnumerable<CandidateBaseExperienceReceipt> rewards, IEnumerable<CandidateCharacterEndReceipt> ends,
            IEnumerable<CandidateRecoveryPeriod> periods)
        {
            Definition = definition; PlayerId = player; CharacterId = character;
            Level = level; Experience = experience; OriginalSlot = slot; StateRevision = revision;
            BaseRewards = new List<CandidateBaseExperienceReceipt>(rewards).AsReadOnly();
            ProcessedEnds = new List<CandidateCharacterEndReceipt>(ends).AsReadOnly();
            RecoveryPeriods = new List<CandidateRecoveryPeriod>(periods).AsReadOnly();
        }
    }

    public sealed class CandidateBaseExperience
    {
        public string PlayerId { get; set; }
        public string CharacterId { get; set; }
        public string AttemptId { get; set; }
        public string SettlementId { get; set; }
        public RuleContext Context { get; set; }
        public BigInteger? Amount { get; set; }
    }

    public sealed class CandidateBaseExperienceReceipt
    {
        public string PlayerId { get; }
        public string CharacterId { get; }
        public string AttemptId { get; }
        public string SettlementId { get; }
        public PreparedRuleContext Context { get; }
        public BigInteger Amount { get; }
        internal CandidateBaseExperienceReceipt(CandidateBaseExperience input, PreparedRuleContext context)
        {
            PlayerId = input.PlayerId; CharacterId = input.CharacterId;
            AttemptId = input.AttemptId; SettlementId = input.SettlementId;
            Context = context; Amount = input.Amount.Value;
        }
    }

    public sealed class CandidateCharacterResult
    {
        public bool IsAccepted => Next != null;
        public CandidateGrowthOutcome Outcome { get; }
        public CandidateCharacterState Next { get; }
        public CandidateGrowthRejectionCode RejectionCode { get; }
        public string FieldPath { get; }
        public CandidateBaseExperienceReceipt RewardReceipt { get; }
        public CandidateCharacterEndReceipt EndReceipt { get; }
        public CandidateRecoveryPeriod RecoveryPeriod { get; }
        public CandidateTimeAnomaly Anomaly { get; }
        public BigInteger ExperienceAdded { get; }
        internal CandidateCharacterResult(GrowthChecks check)
        { RejectionCode = check.Code; FieldPath = check.Path; Outcome = CandidateGrowthOutcome.Rejected; }
        internal CandidateCharacterResult(CandidateCharacterState next, CandidateGrowthOutcome outcome,
            CandidateBaseExperienceReceipt reward = null, CandidateCharacterEndReceipt end = null,
            CandidateRecoveryPeriod period = null, CandidateTimeAnomaly anomaly = CandidateTimeAnomaly.None)
        {
            Next = next; Outcome = outcome; RewardReceipt = reward; EndReceipt = end;
            RecoveryPeriod = period; Anomaly = anomaly;
            ExperienceAdded = outcome == CandidateGrowthOutcome.Applied && reward != null ? reward.Amount : BigInteger.Zero;
        }
    }
}
