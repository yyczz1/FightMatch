using System.Numerics;

namespace FightMatch.Core
{
    public sealed class CandidateComputedStats
    {
        public string PlayerId { get; }
        public string CharacterId { get; }
        public string ClassId { get; }
        public CharacterClassKind ClassKind { get; }
        public BigInteger Level { get; }
        public int OriginalSlot { get; }
        public PreparedRuleContext Context { get; }
        public BaseStatsOrigin StatsOrigin => BaseStatsOrigin.ComputedBaseStats;
        public PreparedStats Stats { get; }
        public ExactRational TargetProbability { get; }
        public string PassiveDefinitionId { get; }
        public ExactRational CritMultiplier { get; }
        public bool IsReady { get; }
        public ExactRational EntryHp { get; }

        internal CandidateComputedStats(CandidateCharacterState state, PreparedStats stats, ExactRational probability)
        {
            PlayerId = state.PlayerId; CharacterId = state.CharacterId; ClassId = state.ClassId;
            ClassKind = state.Definition.ClassKind; Level = state.Level; OriginalSlot = state.OriginalSlot;
            Context = state.Definition.Context; Stats = stats; TargetProbability = probability;
            PassiveDefinitionId = state.Definition.PassiveDefinitionId; CritMultiplier = state.Definition.CritMultiplier;
            IsReady = state.IsReady; EntryHp = IsReady ? stats.MaxHp : null;
        }
    }
}
