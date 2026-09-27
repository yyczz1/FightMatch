using System.Collections.Generic;
using System.Numerics;
using FlowPuzzle.Core;

namespace FightMatch.Core
{
    public enum EntryCarryMode { Unspecified, Empty, NonEmpty }
    public enum CharacterClassKind { Unspecified, Warrior, Mage = 2 }
    public enum BaseStatsOrigin { Unspecified, ComputedBaseStats }
    public enum EnemyBehavior { Unspecified, NormalStrike, ChargeHeavy }
    public enum EnemyIntentKind { Unspecified, Charge, Strike }
    public enum EnemyTargeting { Unspecified, FirstLiving }
    public enum EntryDamageKind { Unspecified, Physical }

    // Callers must not modify any part of this input during PrepareCandidate.
    public sealed class BattleEntryInput
    {
        public string PlayerId { get; set; }
        public string ChallengeId { get; set; }
        public string AttemptId { get; set; }
        public string EntryBaselineId { get; set; }
        public RuleContext Context { get; set; }
        public LevelInput Level { get; set; }
        public List<MemberInput> Members { get; set; }
        public EntryCarryMode CarryMode { get; set; }
        public List<string> RequiredFeatures { get; set; }
    }

    public sealed class CandidateContext : RuleContext
    {
        public override string DraftId { get; set; }
        public override BigInteger DraftRevision { get; set; }
        public override string ContentFingerprint { get; set; }
        public override string RuleVersion { get; set; }
        public override string NumericContractVersion { get; set; }
        public override string RandomContractVersion { get; set; }
        public override List<string> SourceNotes { get; set; }
    }

    public sealed class LevelInput
    {
        public string LevelId { get; set; }
        public string LevelVersion { get; set; }
        public BigInteger RecommendedLevel { get; set; }
        public List<FaceInput> Faces { get; set; }
    }

    public sealed class FaceInput
    {
        public string FaceId { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public List<PairInput> Pairs { get; set; }
    }

    public sealed class PairInput
    {
        private int geometryColorId;
        private FlowPos endpointA;
        private FlowPos endpointB;
        public string PairId { get; set; }
        public int GeometryColorId { get => geometryColorId; set { geometryColorId = value; HasColor = true; } }
        public FlowPos EndpointA { get => endpointA; set { endpointA = value; HasEndpointA = true; } }
        public FlowPos EndpointB { get => endpointB; set { endpointB = value; HasEndpointB = true; } }
        public EnemyInput Enemy { get; set; }
        internal bool HasColor { get; private set; }
        internal bool HasEndpointA { get; private set; }
        internal bool HasEndpointB { get; private set; }
    }

    public sealed class EnemyInput
    {
        private int originalSlot;
        private int stableOrder;
        public string EnemyInstanceKey { get; set; }
        public string EnemyDefinitionId { get; set; }
        public int OriginalSlot { get => originalSlot; set { originalSlot = value; HasSlot = true; } }
        public int StableOrder { get => stableOrder; set { stableOrder = value; HasOrder = true; } }
        public EnemyBehavior Behavior { get; set; }
        public StatsInput Stats { get; set; }
        public List<EnemyIntentInput> IntentCycle { get; set; }
        internal bool HasSlot { get; private set; }
        internal bool HasOrder { get; private set; }
    }

    public sealed class EnemyIntentInput
    {
        private EntryDamageKind? damageKind;
        private ExactRational damageCoefficient;
        public EnemyIntentKind Kind { get; set; }
        public EnemyTargeting Targeting { get; set; }
        public EntryDamageKind? DamageKind { get => damageKind; set { damageKind = value; HasDamageKind = true; } }
        public ExactRational DamageCoefficient
        {
            get => damageCoefficient;
            set { damageCoefficient = value; HasDamageCoefficient = true; }
        }
        internal bool HasDamageKind { get; private set; }
        internal bool HasDamageCoefficient { get; private set; }
    }

    public sealed class StatsInput
    {
        public ExactRational MaxHp { get; set; }
        public ExactRational Attack { get; set; }
        public ExactRational PhysicalDefense { get; set; }
        public ExactRational MagicDefense { get; set; }
        public ExactRational Evasion { get; set; }
        public int AttackRange { get; set; }
    }

    public sealed class MemberInput
    {
        private int originalSlot;
        private bool isReady;
        public string CharacterId { get; set; }
        public string ClassId { get; set; }
        public CharacterClassKind ClassKind { get; set; }
        public int OriginalSlot { get => originalSlot; set { originalSlot = value; HasSlot = true; } }
        public BigInteger Level { get; set; }
        public bool IsReady { get => isReady; set { isReady = value; HasReadiness = true; } }
        public BaseStatsOrigin StatsOrigin { get; set; }
        public RuleContext StatsContext { get; set; }
        public StatsInput Stats { get; set; }
        public ExactRational EntryHp { get; set; }
        public List<string> LearnedSkills { get; set; }
        public WarriorCritInput Crit { get; set; }
        internal bool HasSlot { get; private set; }
        internal bool HasReadiness { get; private set; }
    }

    public sealed class WarriorCritInput
    {
        public string PassiveDefinitionId { get; set; }
        public ExactRational TargetProbability { get; set; }
        public ExactRational C { get; set; }
        public ExactRational Multiplier { get; set; }
    }
}
