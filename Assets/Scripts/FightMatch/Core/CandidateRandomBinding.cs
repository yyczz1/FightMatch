using System.Collections.Generic;
using System.Numerics;

namespace FightMatch.Core
{
    public enum CandidateRandomPurpose { Battle, BaseReward, Bonus }

    // Part of the complete candidate entry slice, not a separately mutable seed ledger.
    public sealed class CandidateRandomBinding
    {
        public CandidateBattleStart Start { get; }
        public PreparedRuleContext Context => Start.Baseline.Entry.Context;
        public string GeneratedForPlayerId => Start.Baseline.Entry.PlayerId;
        public string GeneratedForChallengeId => Start.Baseline.Entry.ChallengeId;
        public string GeneratedForAttemptId => Start.Baseline.Entry.AttemptId;
        public string GeneratedForEntryBaselineId => Start.Baseline.Entry.EntryBaselineId;
        public string SourceCapabilityId { get; }
        public string MappingId { get; }
        public CandidateRandomDomain Battle { get; }
        public CandidateRandomDomain BaseReward { get; }
        public CandidateRandomDomain Bonus { get; }

        internal CandidateRandomBinding(CandidateBattleStart start, string sourceCapabilityId, string mappingId,
            CandidateRandomDomain battle, CandidateRandomDomain baseReward, CandidateRandomDomain bonus)
        {
            Start = start;
            SourceCapabilityId = sourceCapabilityId;
            MappingId = mappingId;
            Battle = battle;
            BaseReward = baseReward;
            Bonus = bonus;
        }
    }

    public sealed class CandidateRandomDomain
    {
        public CandidateRandomPurpose Purpose { get; }
        public ulong InitState { get; }
        public ulong InitSequence { get; }
        public Pcg32StreamState Initial { get; }

        internal CandidateRandomDomain(CandidateRandomPurpose purpose, ulong initState, ulong initSequence,
            Pcg32StreamState initial)
        {
            Purpose = purpose;
            InitState = initState;
            InitSequence = initSequence;
            Initial = initial;
        }
    }

    public sealed class CandidateRandomPreparationResult
    {
        public bool IsAccepted => Binding != null;
        public CandidateRandomBinding Binding { get; }
        public BattleEntryRejectionCode? RejectionCode { get; }
        public string FieldPath { get; }

        internal CandidateRandomPreparationResult(CandidateRandomBinding binding) { Binding = binding; }
        internal CandidateRandomPreparationResult(BattleEntryRejectionCode code, string fieldPath)
        {
            RejectionCode = code;
            FieldPath = fieldPath;
        }
    }

    public sealed class CandidateCritFact
    {
        public BattleCombatantKey Actor { get; }
        public BattleCombatantKey Target { get; }
        public BigInteger OpportunityOrdinal { get; }
        public PreparedWarriorCrit Parameters { get; }
        public ExactRational Probability { get; }
        public BigInteger FailureCountBefore { get; }
        public BigInteger FailureCountAfter { get; }
        public bool Triggered { get; }
        public Pcg32StreamState StreamBefore { get; }
        public Pcg32StreamState StreamAfter { get; }
        public IReadOnlyList<uint> Words { get; }

        internal CandidateCritFact(BattleCombatantKey actor, BattleCombatantKey target, BigInteger ordinal,
            PreparedWarriorCrit parameters, BattlePrdState before, Pcg32StreamState stream, RandomSample<PrdOutcome> sample)
        {
            Actor = actor;
            Target = target;
            OpportunityOrdinal = ordinal;
            Parameters = parameters;
            Probability = sample.Value.Probability;
            FailureCountBefore = before.FailureCount;
            FailureCountAfter = sample.Value.Failures;
            Triggered = sample.Value.Triggered;
            StreamBefore = stream;
            StreamAfter = sample.NextState;
            Words = new List<uint>(sample.Words).AsReadOnly();
        }
    }

    public sealed class CandidateCritResult
    {
        public bool IsAccepted => Next != null;
        public BattleRandomSnapshot Next { get; }
        public CandidateCritFact Fact { get; }
        public BattleEntryRejectionCode? RejectionCode { get; }
        public string FieldPath { get; }

        internal CandidateCritResult(BattleRandomSnapshot next, CandidateCritFact fact) { Next = next; Fact = fact; }
        internal CandidateCritResult(BattleEntryRejectionCode code, string fieldPath)
        {
            RejectionCode = code;
            FieldPath = fieldPath;
        }
    }
}
