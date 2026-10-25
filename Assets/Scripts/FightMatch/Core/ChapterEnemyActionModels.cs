using System.Numerics;

namespace FightMatch.Core
{
    public sealed class ChapterVulnerabilityWindow
    {
        public BattleCombatantKey TargetEnemyKey { get; }
        public ExactRational Multiplier { get; }
        public BigInteger EffectiveActionOrdinal { get; }

        internal ChapterVulnerabilityWindow(BattleCombatantKey targetEnemyKey, ExactRational multiplier,
            BigInteger effectiveActionOrdinal)
        {
            TargetEnemyKey = targetEnemyKey;
            Multiplier = multiplier;
            EffectiveActionOrdinal = effectiveActionOrdinal;
        }
    }

    public enum ChapterVulnerabilityChange { Created, Expired }

    public sealed class ChapterVulnerabilityFact
    {
        public ChapterVulnerabilityChange Change { get; }
        public ChapterVulnerabilityWindow Window { get; }

        internal ChapterVulnerabilityFact(ChapterVulnerabilityChange change, ChapterVulnerabilityWindow window)
        {
            Change = change;
            Window = window;
        }
    }

    public sealed class ChapterWindFact
    {
        public BattleCombatantKey TargetEnemyKey { get; }
        public ExactRational Fraction { get; }
        public ExactRational MaximumHp { get; }
        public ExactRational HpBefore { get; }
        public ExactRational HpAfter { get; }
        public ExactRational ActualHealing { get; }
        public int UsesBefore { get; }
        public int UsesAfter { get; }

        internal ChapterWindFact(BattleCombatantKey targetEnemyKey, ExactRational fraction,
            ExactRational maximumHp, ExactRational hpBefore, ExactRational hpAfter,
            ExactRational actualHealing, int usesBefore, int usesAfter)
        {
            TargetEnemyKey = targetEnemyKey;
            Fraction = fraction;
            MaximumHp = maximumHp;
            HpBefore = hpBefore;
            HpAfter = hpAfter;
            ActualHealing = actualHealing;
            UsesBefore = usesBefore;
            UsesAfter = usesAfter;
        }
    }
}
