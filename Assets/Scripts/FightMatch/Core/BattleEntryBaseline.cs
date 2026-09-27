using System.Collections.Generic;
using System.Numerics;

namespace FightMatch.Core
{
    // Candidate entry evidence and initial values; no freeze or commit receipt.
    public sealed class BattleEntryBaseline
    {
        public PreparedBattleEntry Entry { get; }
        public BattleRandomInitials RandomInitials { get; }
        public IReadOnlyList<BattlePrdState> PrdInitialStates { get; }

        internal BattleEntryBaseline(PreparedBattleEntry entry, CandidateRandomInitials initials,
            IEnumerable<BattlePrdState> prdStates)
        {
            Entry = entry;
            RandomInitials = new BattleRandomInitials(initials.Battle, initials.BaseReward, initials.Bonus);
            PrdInitialStates = new List<BattlePrdState>(prdStates).AsReadOnly();
        }
    }

    public sealed class BattleRandomInitials
    {
        public Pcg32StreamState Battle { get; }
        public Pcg32StreamState BaseReward { get; }
        public Pcg32StreamState Bonus { get; }

        internal BattleRandomInitials(Pcg32StreamState battle, Pcg32StreamState baseReward, Pcg32StreamState bonus)
        {
            Battle = battle;
            BaseReward = baseReward;
            Bonus = bonus;
        }
    }

    public sealed class BattlePrdState
    {
        public BattleCombatantKey CombatantKey { get; }
        public PreparedWarriorCrit Crit { get; }
        public BigInteger FailureCount { get; }

        internal BattlePrdState(BattleCombatantKey combatantKey, PreparedWarriorCrit crit, BigInteger failureCount)
        {
            CombatantKey = combatantKey;
            Crit = crit;
            FailureCount = failureCount;
        }
    }

    public sealed class CandidateBattleStart
    {
        public BattleEntryBaseline Baseline { get; }
        public BattleSnapshot Snapshot { get; }

        internal CandidateBattleStart(BattleEntryBaseline baseline, BattleSnapshot snapshot)
        {
            Baseline = baseline;
            Snapshot = snapshot;
        }
    }

    public sealed class BattleStartResult
    {
        public bool IsAccepted => Start != null;
        public CandidateBattleStart Start { get; }
        public BattleEntryRejectionCode? RejectionCode { get; }
        public string FieldPath { get; }

        internal BattleStartResult(CandidateBattleStart start) { Start = start; }
        internal BattleStartResult(BattleEntryRejectionCode code, string fieldPath)
        {
            RejectionCode = code;
            FieldPath = fieldPath;
        }
    }
}
