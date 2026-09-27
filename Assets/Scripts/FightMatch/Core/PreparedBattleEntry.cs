using System.Collections.Generic;
using System.Numerics;
using FlowPuzzle.Core;

namespace FightMatch.Core
{
    // Validated candidate data only: no battle state, random state or commit capability.
    public sealed class PreparedBattleEntry
    {
        public string PlayerId { get; }
        public string ChallengeId { get; }
        public string AttemptId { get; }
        public string EntryBaselineId { get; }
        public PreparedRuleContext Context { get; }
        public PreparedLevel Level { get; }
        public IReadOnlyList<PreparedMember> Members { get; }
        public IReadOnlyList<PreparedMember> ReadyParticipants { get; }
        public EntryCarryMode CarryMode { get; }
        public IReadOnlyList<string> RequiredFeatures { get; }

        private readonly DefinitionBinding definitionBinding;
        // A method preserves the existing candidate v1 property projection.
        public DefinitionBinding GetDefinitionBinding() { return definitionBinding; }

        internal PreparedBattleEntry(BattleEntryInput input, DefinitionBinding binding = null)
        {
            definitionBinding = binding;
            PlayerId = input.PlayerId;
            ChallengeId = input.ChallengeId;
            AttemptId = input.AttemptId;
            EntryBaselineId = input.EntryBaselineId;
            Context = RuleContextChecks.Freeze(input.Context);
            Level = new PreparedLevel(input.Level);
            var members = new List<PreparedMember>();
            var ready = new List<PreparedMember>();
            foreach (var source in input.Members)
            {
                var member = new PreparedMember(source);
                members.Add(member);
                if (member.IsReady) ready.Add(member);
            }
            Members = members.AsReadOnly();
            ReadyParticipants = ready.AsReadOnly();
            CarryMode = input.CarryMode;
            RequiredFeatures = new List<string>(input.RequiredFeatures).AsReadOnly();
        }
    }

    public sealed class PreparedCandidateContext : PreparedRuleContext
    {
        public override string DraftId { get; }
        public override BigInteger DraftRevision { get; }
        public override string ContentFingerprint { get; }
        public override string RuleVersion { get; }
        public override string NumericContractVersion { get; }
        public override string RandomContractVersion { get; }
        public override IReadOnlyList<string> SourceNotes { get; }

        internal PreparedCandidateContext(CandidateContext input)
        {
            DraftId = input.DraftId;
            DraftRevision = input.DraftRevision;
            ContentFingerprint = input.ContentFingerprint;
            RuleVersion = input.RuleVersion;
            NumericContractVersion = input.NumericContractVersion;
            RandomContractVersion = input.RandomContractVersion;
            SourceNotes = new List<string>(input.SourceNotes).AsReadOnly();
        }
    }

    public sealed class PreparedLevel
    {
        public string LevelId { get; }
        public string LevelVersion { get; }
        public BigInteger RecommendedLevel { get; }
        public IReadOnlyList<PreparedFace> Faces { get; }

        internal PreparedLevel(LevelInput input)
        {
            LevelId = input.LevelId;
            LevelVersion = input.LevelVersion;
            RecommendedLevel = input.RecommendedLevel;
            Faces = input.Faces.ConvertAll(face => new PreparedFace(face)).AsReadOnly();
        }
    }

    public sealed class PreparedFace
    {
        public string FaceId { get; }
        public int Width { get; }
        public int Height { get; }
        public IReadOnlyList<PreparedPair> Pairs { get; }

        internal PreparedFace(FaceInput input)
        {
            FaceId = input.FaceId;
            Width = input.Width;
            Height = input.Height;
            Pairs = input.Pairs.ConvertAll(pair => new PreparedPair(pair)).AsReadOnly();
        }
    }

    public sealed class PreparedPair
    {
        public string PairId { get; }
        public int GeometryColorId { get; }
        public FlowPos EndpointA { get; }
        public FlowPos EndpointB { get; }
        public PreparedEnemy Enemy { get; }

        internal PreparedPair(PairInput input)
        {
            PairId = input.PairId;
            GeometryColorId = input.GeometryColorId;
            EndpointA = input.EndpointA;
            EndpointB = input.EndpointB;
            Enemy = new PreparedEnemy(input.Enemy);
        }
    }

    public sealed class PreparedEnemy
    {
        public string EnemyInstanceKey { get; }
        public string EnemyDefinitionId { get; }
        public int OriginalSlot { get; }
        public int StableOrder { get; }
        public EnemyBehavior Behavior { get; }
        public PreparedStats Stats { get; }
        public IReadOnlyList<PreparedEnemyIntent> IntentCycle { get; }

        internal PreparedEnemy(EnemyInput input)
        {
            EnemyInstanceKey = input.EnemyInstanceKey;
            EnemyDefinitionId = input.EnemyDefinitionId;
            OriginalSlot = input.OriginalSlot;
            StableOrder = input.StableOrder;
            Behavior = input.Behavior;
            Stats = new PreparedStats(input.Stats);
            IntentCycle = input.IntentCycle.ConvertAll(intent => new PreparedEnemyIntent(intent)).AsReadOnly();
        }
    }

    public sealed class PreparedEnemyIntent
    {
        public EnemyIntentKind Kind { get; }
        public EnemyTargeting Targeting { get; }
        public EntryDamageKind? DamageKind { get; }
        public ExactRational DamageCoefficient { get; }

        internal PreparedEnemyIntent(EnemyIntentInput input)
        {
            Kind = input.Kind;
            Targeting = input.Targeting;
            DamageKind = input.DamageKind;
            DamageCoefficient = input.DamageCoefficient;
        }
    }

    public sealed class PreparedStats
    {
        public ExactRational MaxHp { get; }
        public ExactRational Attack { get; }
        public ExactRational PhysicalDefense { get; }
        public ExactRational MagicDefense { get; }
        public ExactRational Evasion { get; }
        public int AttackRange { get; }

        internal PreparedStats(StatsInput input)
        {
            MaxHp = input.MaxHp;
            Attack = input.Attack;
            PhysicalDefense = input.PhysicalDefense;
            MagicDefense = input.MagicDefense;
            Evasion = input.Evasion;
            AttackRange = input.AttackRange;
        }
    }

    public sealed class PreparedMember
    {
        public string CharacterId { get; }
        public string ClassId { get; }
        public CharacterClassKind ClassKind { get; }
        public int OriginalSlot { get; }
        public BigInteger Level { get; }
        public bool IsReady { get; }
        public BaseStatsOrigin StatsOrigin { get; }
        public PreparedRuleContext StatsContext { get; }
        public PreparedStats Stats { get; }
        public ExactRational EntryHp { get; }
        public IReadOnlyList<string> LearnedSkills { get; }
        public PreparedWarriorCrit Crit { get; }

        internal PreparedMember(MemberInput input)
        {
            CharacterId = input.CharacterId;
            ClassId = input.ClassId;
            ClassKind = input.ClassKind;
            OriginalSlot = input.OriginalSlot;
            Level = input.Level;
            IsReady = input.IsReady;
            StatsOrigin = input.StatsOrigin;
            StatsContext = RuleContextChecks.Freeze(input.StatsContext);
            Stats = new PreparedStats(input.Stats);
            EntryHp = input.EntryHp;
            LearnedSkills = new List<string>(input.LearnedSkills).AsReadOnly();
            Crit = new PreparedWarriorCrit(input.Crit);
        }
    }

    public sealed class PreparedWarriorCrit
    {
        public string PassiveDefinitionId { get; }
        public ExactRational TargetProbability { get; }
        public ExactRational C { get; }
        public ExactRational Multiplier { get; }

        internal PreparedWarriorCrit(WarriorCritInput input)
        {
            PassiveDefinitionId = input.PassiveDefinitionId;
            TargetProbability = input.TargetProbability;
            C = input.C;
            Multiplier = input.Multiplier;
        }
    }
}
