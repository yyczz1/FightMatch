using System.Numerics;

namespace FightMatch.Core
{
    public sealed class CandidateEnemyIntentFact
    {
        public BattleEntryBaseline Baseline { get; }
        public string PlayerId { get; }
        public string AttemptId { get; }
        public string OperationId { get; }
        public string FaceId { get; }
        public BigInteger SceneRevision { get; }
        public BigInteger ActionOrdinal { get; }
        public BigInteger EnemyPhaseOrdinal { get; }
        public int SegmentIndex { get; }
        public BattleCombatantKey EnemyKey { get; }
        public BattlePairKey Pair { get; }
        public int StableOrder { get; }
        public EnemyIntentKind IntentKind { get; }
        public BigInteger CursorBefore { get; }
        public BigInteger CursorAfter { get; }
        public CandidateEnemyDamageFact Damage { get; }

        internal CandidateEnemyIntentFact(CandidateCombatFrame direct, BigInteger phase, int index,
            BattleEnemyState enemy, EnemyIntentKind kind, BigInteger cursorAfter, CandidateEnemyDamageFact damage)
        {
            Baseline = direct.BeforeSnapshot.Baseline;
            PlayerId = direct.Action.PlayerId; AttemptId = direct.Action.AttemptId;
            OperationId = direct.Action.OperationId; FaceId = enemy.PairKey.FaceId;
            SceneRevision = direct.BeforeSnapshot.SceneRevision; ActionOrdinal = direct.Action.ActionOrdinal;
            EnemyPhaseOrdinal = phase; SegmentIndex = index;
            EnemyKey = enemy.CombatantKey; Pair = enemy.PairKey; StableOrder = enemy.StableOrder;
            IntentKind = kind; CursorBefore = enemy.IntentCursor; CursorAfter = cursorAfter; Damage = damage;
        }
    }

    public sealed class CandidateEnemyDamageFact
    {
        public BattleCombatantKey ActorEnemy { get; }
        public BattleCombatantKey TargetMember { get; }
        public EntryDamageKind DamageKind => EntryDamageKind.Physical;
        public ExactRational Attack { get; }
        public ExactRational DamageCoefficient { get; }
        public ExactRational PhysicalDefense { get; }
        public ExactRational RawDamage { get; }
        public ExactRational MitigatedDamage { get; }
        public BigInteger RoundedDamage { get; }
        public ExactRational BlockPrevented { get; }
        public ExactRational ShieldAbsorbed { get; }
        public ExactRational HpBefore { get; }
        public ExactRational HpAfter { get; }
        public ExactRational HpLoss { get; }
        public ExactRational Overflow { get; }
        public bool DefeatedTarget => HpBefore.Numerator.Sign > 0 && HpAfter.Numerator.IsZero;

        internal CandidateEnemyDamageFact(BattleEnemyState enemy, BattleMemberState target,
            ExactRational coefficient, ExactRational raw, ExactRational mitigated, BigInteger rounded,
            ExactRational hpAfter, ExactRational loss, ExactRational overflow, ExactRational zero)
        {
            ActorEnemy = enemy.CombatantKey; TargetMember = target.CombatantKey;
            Attack = enemy.Enemy.Stats.Attack; DamageCoefficient = coefficient;
            PhysicalDefense = target.Member.Stats.PhysicalDefense;
            RawDamage = raw; MitigatedDamage = mitigated; RoundedDamage = rounded;
            BlockPrevented = zero; ShieldAbsorbed = zero;
            HpBefore = target.Hp; HpAfter = hpAfter; HpLoss = loss; Overflow = overflow;
        }
    }
}
