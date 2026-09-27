using System.Collections.Generic;
using System.Numerics;
using FlowPuzzle.Core;

namespace FightMatch.Core
{
    public enum BattlePhase
    {
        AwaitAction, // No pending links; living enemies and an available participant.
        AwaitLinks, // Dead pairs require manual links before another attack or face transition.
        AwaitRescue, // No pending links; living enemies but every participant is down.
        WonPendingSettlement, // Final face cleared and linked; battle finished, settlement pending.
        Closed // Attempt ended; only its existing result can be read.
    }

    public sealed class BattleSnapshot
    {
        public BattleEntryBaseline Baseline { get; }
        public BigInteger SceneRevision { get; }
        public BigInteger EffectiveActionsCompleted { get; }
        public BigInteger EnemyPhasesCompleted { get; }
        public int CurrentFaceIndex { get; }
        public BattlePhase Phase { get; }
        public EntryCarryMode CarryMode { get; }
        public BattleBoardState Board { get; }
        public IReadOnlyList<BattleMemberState> Members { get; }
        public IReadOnlyList<BattleEnemyState> Enemies { get; }
        public BattleRandomSnapshot Random { get; }
        public IReadOnlyList<BattleContributionTotals> Contributions { get; }

        internal BattleSnapshot(BattleEntryBaseline baseline, ExactRational zero)
        {
            Baseline = baseline;
            SceneRevision = BigInteger.One;
            EffectiveActionsCompleted = BigInteger.Zero;
            EnemyPhasesCompleted = BigInteger.Zero;
            CurrentFaceIndex = 0;
            Phase = BattlePhase.AwaitAction;
            // This supported domain has empty C/U/D; it says nothing about inventory T or a commit.
            CarryMode = EntryCarryMode.Empty;
            var entry = baseline.Entry;
            var face = entry.Level.Faces[0];
            Board = new BattleBoardState(face);
            var members = new List<BattleMemberState>();
            var contributions = new List<BattleContributionTotals>();
            foreach (var member in entry.ReadyParticipants)
            {
                var key = BattleCombatantKey.ForParticipant(entry.AttemptId, member.CharacterId);
                members.Add(new BattleMemberState(key, member));
                contributions.Add(new BattleContributionTotals(key, zero));
            }
            Members = members.AsReadOnly();
            Contributions = contributions.AsReadOnly();
            var enemies = new List<BattleEnemyState>();
            foreach (var pair in face.Pairs)
                enemies.Add(new BattleEnemyState(
                    BattleCombatantKey.ForEnemy(entry.AttemptId, face.FaceId, pair.Enemy.EnemyInstanceKey),
                    BattlePairKey.Create(entry.AttemptId, face.FaceId, pair.PairId), pair.Enemy));
            Enemies = enemies.AsReadOnly();
            Random = new BattleRandomSnapshot(baseline.RandomInitials.Battle, baseline.PrdInitialStates);
        }

        // The operation creating this value must validate its complete state with its own budget.
        internal BattleSnapshot(BattleEntryBaseline baseline, BigInteger sceneRevision,
            BigInteger effectiveActionsCompleted, BigInteger enemyPhasesCompleted, int currentFaceIndex,
            BattlePhase phase, BattleBoardState board, IEnumerable<BattleMemberState> members,
            IEnumerable<BattleEnemyState> enemies, BattleRandomSnapshot random,
            IEnumerable<BattleContributionTotals> contributions)
        {
            Baseline = baseline;
            SceneRevision = sceneRevision;
            EffectiveActionsCompleted = effectiveActionsCompleted;
            EnemyPhasesCompleted = enemyPhasesCompleted;
            CurrentFaceIndex = currentFaceIndex;
            Phase = phase;
            CarryMode = EntryCarryMode.Empty;
            Board = board;
            Members = new List<BattleMemberState>(members).AsReadOnly();
            Enemies = new List<BattleEnemyState>(enemies).AsReadOnly();
            Random = random;
            Contributions = new List<BattleContributionTotals>(contributions).AsReadOnly();
        }
    }

    public sealed class BattleBoardState
    {
        public PreparedFace Face { get; }
        public IReadOnlyList<BattleLockedRoute> LockedRoutes { get; }
        public IReadOnlyList<BattlePairKey> PendingLinks { get; }

        internal BattleBoardState(PreparedFace face)
        {
            Face = face;
            LockedRoutes = new List<BattleLockedRoute>().AsReadOnly();
            PendingLinks = new List<BattlePairKey>().AsReadOnly();
        }

        internal BattleBoardState(PreparedFace face, IEnumerable<BattleLockedRoute> lockedRoutes,
            IEnumerable<BattlePairKey> pendingLinks)
        {
            Face = face;
            LockedRoutes = new List<BattleLockedRoute>(lockedRoutes).AsReadOnly();
            PendingLinks = new List<BattlePairKey>(pendingLinks).AsReadOnly();
        }
    }

    public sealed class BattleLockedRoute
    {
        public BattlePairKey PairKey { get; }
        public IReadOnlyList<FlowPos> Route { get; }

        internal BattleLockedRoute(BattlePairKey pairKey, IEnumerable<FlowPos> route)
        {
            PairKey = pairKey;
            Route = new List<FlowPos>(route).AsReadOnly();
        }
    }

    public sealed class BattleMemberState
    {
        public BattleCombatantKey CombatantKey { get; }
        public PreparedMember Member { get; }
        public int OriginalSlot { get; }
        public ExactRational Hp { get; }

        internal BattleMemberState(BattleCombatantKey combatantKey, PreparedMember member)
        {
            CombatantKey = combatantKey;
            Member = member;
            OriginalSlot = member.OriginalSlot;
            Hp = member.EntryHp;
        }

        internal BattleMemberState(BattleCombatantKey combatantKey, PreparedMember member, ExactRational hp)
        {
            CombatantKey = combatantKey;
            Member = member;
            OriginalSlot = member.OriginalSlot;
            Hp = hp;
        }
    }

    public sealed class BattleEnemyState
    {
        public BattleCombatantKey CombatantKey { get; }
        public BattlePairKey PairKey { get; }
        public PreparedEnemy Enemy { get; }
        public int OriginalSlot { get; }
        public int StableOrder { get; }
        public ExactRational Hp { get; }
        // Number of processed intents; next is Enemy.IntentCycle[IntentCursor % count].
        public BigInteger IntentCursor { get; }

        internal BattleEnemyState(BattleCombatantKey combatantKey, BattlePairKey pairKey, PreparedEnemy enemy)
        {
            CombatantKey = combatantKey;
            PairKey = pairKey;
            Enemy = enemy;
            OriginalSlot = enemy.OriginalSlot;
            StableOrder = enemy.StableOrder;
            Hp = enemy.Stats.MaxHp;
            IntentCursor = BigInteger.Zero;
        }

        internal BattleEnemyState(BattleCombatantKey combatantKey, BattlePairKey pairKey, PreparedEnemy enemy,
            ExactRational hp, BigInteger intentCursor)
        {
            CombatantKey = combatantKey;
            PairKey = pairKey;
            Enemy = enemy;
            OriginalSlot = enemy.OriginalSlot;
            StableOrder = enemy.StableOrder;
            Hp = hp;
            IntentCursor = intentCursor;
        }
    }

    public sealed class BattleRandomSnapshot
    {
        public Pcg32StreamState Stream { get; }
        public IReadOnlyList<BattlePrdState> PrdStates { get; }

        internal BattleRandomSnapshot(Pcg32StreamState stream, IEnumerable<BattlePrdState> prdStates)
        {
            Stream = stream;
            PrdStates = new List<BattlePrdState>(prdStates).AsReadOnly();
        }
    }

    public sealed class BattleContributionTotals
    {
        public BattleCombatantKey CombatantKey { get; }
        public ExactRational EffectiveDamageDealtHp { get; }
        public ExactRational EffectiveDamageTakenHp { get; }

        internal BattleContributionTotals(BattleCombatantKey combatantKey, ExactRational zero)
        {
            CombatantKey = combatantKey;
            EffectiveDamageDealtHp = zero;
            EffectiveDamageTakenHp = zero;
        }

        internal BattleContributionTotals(BattleCombatantKey combatantKey, ExactRational damageDealt,
            ExactRational damageTaken)
        {
            CombatantKey = combatantKey;
            EffectiveDamageDealtHp = damageDealt;
            EffectiveDamageTakenHp = damageTaken;
        }
    }
}
