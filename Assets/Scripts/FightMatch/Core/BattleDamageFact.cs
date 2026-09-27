using System.Numerics;

namespace FightMatch.Core
{
    public sealed class BattleDamageFact
    {
        public BattleEntryBaseline Baseline { get; }
        public string PlayerId { get; }
        public string AttemptId { get; }
        public string FaceId { get; }
        public string OperationId { get; }
        public BigInteger SceneRevision { get; }
        public BigInteger ActionOrdinal { get; }
        public BattleCombatantKey Actor { get; }
        public BattleCombatantKey Target { get; }
        public BattlePairKey Pair { get; }
        public int EffectIndex => 0;
        public EntryDamageKind DamageKind => EntryDamageKind.Physical;
        public ExactRational Attack { get; }
        public ExactRational PhysicalDefense { get; }
        public ExactRational Multiplier { get; }
        public ExactRational RawDamage { get; }
        public ExactRational MitigatedDamage { get; }
        public BigInteger RoundedDamage { get; }
        // The validated baseline's supported domain has neither blocking nor shields.
        public ExactRational BlockPrevented { get; }
        public ExactRational ShieldAbsorbed { get; }
        public ExactRational HpBefore { get; }
        public ExactRational HpAfter { get; }
        public ExactRational HpLoss { get; }
        public ExactRational Overflow { get; }
        public bool DefeatedTarget => HpBefore.Numerator.Sign > 0 && HpAfter.Numerator.IsZero;
        public CandidateCritFact Crit { get; }

        internal BattleDamageFact(BattleSnapshot before, CandidateAttackSource action, BattleEnemyState target,
            ExactRational attack, ExactRational multiplier, ExactRational raw, ExactRational mitigated,
            BigInteger rounded, ExactRational hpAfter, ExactRational hpLoss, ExactRational overflow,
            ExactRational zero, CandidateCritFact crit)
        {
            Baseline = before.Baseline;
            PlayerId = action.PlayerId;
            AttemptId = action.AttemptId;
            FaceId = target.PairKey.FaceId;
            OperationId = action.OperationId;
            SceneRevision = before.SceneRevision;
            ActionOrdinal = action.ActionOrdinal;
            Actor = action.Actor;
            Target = target.CombatantKey;
            Pair = action.Pair;
            Attack = attack;
            PhysicalDefense = target.Enemy.Stats.PhysicalDefense;
            Multiplier = multiplier;
            RawDamage = raw;
            MitigatedDamage = mitigated;
            RoundedDamage = rounded;
            BlockPrevented = zero;
            ShieldAbsorbed = zero;
            HpBefore = target.Hp;
            HpAfter = hpAfter;
            HpLoss = hpLoss;
            Overflow = overflow;
            Crit = crit;
        }
    }
}
