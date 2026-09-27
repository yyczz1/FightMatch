using System.Collections.Generic;
using System.Numerics;

namespace FightMatch.Core
{
    public enum CandidateBattleOperationKind { Attack, Link }
    public enum CandidateBattleFactKind { DirectAttack, EnemyIntent, Stage }
    public enum CandidateContributionKind { DamageDealtHp, DamageTakenHp }
    public enum CandidateConsumptionCoverage { EmptyCarryNoUse }

    public sealed class CandidateBattleOrderedFact
    {
        public int Index { get; }
        public CandidateBattleFactKind Kind { get; }
        public BattleDamageFact DirectAttack { get; }
        public CandidateEnemyIntentFact EnemyIntent { get; }
        public CandidateStageFact Stage { get; }
        internal CandidateBattleOrderedFact(int index, BattleDamageFact direct)
        { Index = index; Kind = CandidateBattleFactKind.DirectAttack; DirectAttack = direct; }
        internal CandidateBattleOrderedFact(int index, CandidateEnemyIntentFact enemy)
        { Index = index; Kind = CandidateBattleFactKind.EnemyIntent; EnemyIntent = enemy; }
        internal CandidateBattleOrderedFact(int index, CandidateStageFact stage)
        { Index = index; Kind = CandidateBattleFactKind.Stage; Stage = stage; }
    }

    public sealed class CandidateContributionSegment
    {
        public string OperationId { get; }
        public BigInteger SceneRevision { get; }
        public string FaceId { get; }
        public CandidateBattleFactKind RuleSegment { get; }
        public int SegmentIndex { get; }
        public BattleCombatantKey Actor { get; }
        public BattleCombatantKey Target { get; }
        public BattleCombatantKey Beneficiary { get; }
        public CandidateContributionKind Kind { get; }
        public ExactRational HpLoss { get; }
        public int FactIndex { get; }
        internal CandidateContributionSegment(string operation, BigInteger revision, string face,
            CandidateBattleFactKind segment, int segmentIndex, BattleCombatantKey actor, BattleCombatantKey target,
            BattleCombatantKey beneficiary, CandidateContributionKind kind, ExactRational loss, int factIndex)
        {
            OperationId = operation; SceneRevision = revision; FaceId = face; RuleSegment = segment;
            SegmentIndex = segmentIndex; Actor = actor; Target = target; Beneficiary = beneficiary;
            Kind = kind; HpLoss = loss; FactIndex = factIndex;
        }
    }

    public sealed class CandidateBattleOperationRecord
    {
        public CandidateBattleOperationKind Kind { get; }
        public string OperationId => Request.OperationId;
        public BigInteger OccurredAtUnixMilliseconds { get; }
        public BattleSnapshot BeforeSnapshot { get; }
        public BattleSnapshot AfterSnapshot { get; }
        public CandidateStageSource Request => StageDecision.Source;
        public CandidateBattleConditionValues Conditions { get; }
        public CandidateCombatFrame DirectAttack { get; }
        public CandidateEnemyPhaseFrame EnemyPhase { get; }
        public CandidateStageDecision StageDecision { get; }
        public IReadOnlyList<CandidateBattleOrderedFact> OrderedFacts { get; }
        public IReadOnlyList<CandidateContributionSegment> ContributionSegments { get; }
        public CandidateConsumptionCoverage ConsumptionCoverage { get; }

        internal CandidateBattleOperationRecord(CandidateBattleOperationKind kind, BigInteger time,
            BattleSnapshot before, BattleSnapshot after, CandidateBattleConditionValues conditions,
            CandidateCombatFrame direct, CandidateEnemyPhaseFrame enemy, CandidateStageDecision stage,
            IEnumerable<CandidateBattleOrderedFact> facts, IEnumerable<CandidateContributionSegment> contributions,
            CandidateConsumptionCoverage coverage = CandidateConsumptionCoverage.EmptyCarryNoUse)
        {
            Kind = kind; OccurredAtUnixMilliseconds = time; BeforeSnapshot = before; AfterSnapshot = after;
            Conditions = conditions; DirectAttack = direct; EnemyPhase = enemy; StageDecision = stage;
            OrderedFacts = new List<CandidateBattleOrderedFact>(facts).AsReadOnly();
            ContributionSegments = new List<CandidateContributionSegment>(contributions).AsReadOnly(); ConsumptionCoverage = coverage;
        }
    }
}
