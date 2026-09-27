using System;
using System.Collections.Generic;
using System.Numerics;
using FlowPuzzle.Core;

namespace FightMatch.Core
{
    // Closed comparison schema for the accepted candidate models. No reflection or byte-offset paths.
    internal sealed class CandidateReplayComparison
    {
        private readonly ExactMathBudget math;
        private readonly int step;
        private readonly string operation;
        private CandidateReplayDivergence difference;
        private CandidateReplayComparison(ExactMathBudget math, int step, string operation)
        { this.math = math; this.step = step; this.operation = operation; }

        internal static CandidateReplayDivergence Compare(CandidateBattleOperationRecord expected,
            CandidateBattleOperationRecord actual, CandidateFinalAttemptReport expectedReport,
            CandidateFinalAttemptReport actualReport, ExactMathBudget math, int step = 0)
        {
            var c = new CandidateReplayComparison(math, step, expected.OperationId);
            c.Record("Record", expected, actual); c.Report("FinalReport", expectedReport, actualReport);
            return c.difference;
        }
        internal static bool SameRecords(IReadOnlyList<CandidateBattleOperationRecord> a,
            IReadOnlyList<CandidateBattleOperationRecord> b, ExactMathBudget math)
        { var c = new CandidateReplayComparison(math, 0, null); c.S("Records", a, b, c.Record); return c.difference == null; }
        internal static bool SameReport(CandidateFinalAttemptReport a, CandidateFinalAttemptReport b, ExactMathBudget math)
        { var c = new CandidateReplayComparison(math, 0, null); c.Report("FinalReport", a, b); return c.difference == null; }

        private void V(string p, object a, object b)
        {
            if (difference != null) return;
            if (a is BigInteger ai) math.CheckInteger(ai);
            if (b is BigInteger bi) math.CheckInteger(bi);
            if (a is int an) math.CheckInteger(an);
            if (b is int bn) math.CheckInteger(bn);
            if (a is uint au) math.CheckInteger(au);
            if (b is uint bu) math.CheckInteger(bu);
            if (a is ulong al) math.CheckInteger(al);
            if (b is ulong bl) math.CheckInteger(bl);
            var equal = a is string || b is string ? StringComparer.Ordinal.Equals(a as string, b as string) : Equals(a, b);
            if (!equal) difference = new CandidateReplayDivergence(step, operation, p, a, b);
        }
        private bool Open(string p, object a, object b)
        {
            if (difference != null || ReferenceEquals(a, b)) return false;
            if (a == null || b == null) { V(p + ".Present", a != null, b != null); return false; }
            return true;
        }
        private void S<T>(string p, IReadOnlyList<T> a, IReadOnlyList<T> b, Action<string, T, T> compare)
        {
            if (!Open(p, a, b)) return;
            V(p + ".Count", a.Count, b.Count);
            for (var i = 0; i < a.Count && difference == null; i++) compare(p + "[" + i + "]", a[i], b[i]);
        }
        private void R(string p, ExactRational a, ExactRational b)
        { if (!Open(p, a, b)) return; V(p + ".Numerator", a.Numerator, b.Numerator); V(p + ".Denominator", a.Denominator, b.Denominator); }
        private void Position(string p, FlowPos a, FlowPos b)
        { V(p + ".x", a.x, b.x); V(p + ".y", a.y, b.y); }
        private void Stream(string p, Pcg32StreamState a, Pcg32StreamState b)
        {
            if (!Open(p, a, b)) return;
            V(p + ".Initial.State", a.Initial.State, b.Initial.State); V(p + ".Initial.Increment", a.Initial.Increment, b.Initial.Increment);
            V(p + ".Current.State", a.Current.State, b.Current.State); V(p + ".Current.Increment", a.Current.Increment, b.Current.Increment);
            V(p + ".WordsConsumed", a.WordsConsumed, b.WordsConsumed);
        }

        private void Record(string p, CandidateBattleOperationRecord a, CandidateBattleOperationRecord b)
        {
            if (!Open(p, a, b)) return;
            V(p + ".Kind", a.Kind, b.Kind); V(p + ".OperationId", a.OperationId, b.OperationId);
            V(p + ".OccurredAtUnixMilliseconds", a.OccurredAtUnixMilliseconds, b.OccurredAtUnixMilliseconds);
            Snapshot(p + ".BeforeSnapshot", a.BeforeSnapshot, b.BeforeSnapshot); Source(p + ".Request", a.Request, b.Request);
            Conditions(p + ".Conditions", a.Conditions, b.Conditions);
            V(p + ".ConsumptionCoverage", a.ConsumptionCoverage, b.ConsumptionCoverage);
            var da = a.DirectAttack; var db = b.DirectAttack;
            var direct = Open(p + ".DirectAttack", da, db);
            if (direct)
            {
                Binding(p + ".DirectAttack.Binding", da.Binding, db.Binding);
                Snapshot(p + ".DirectAttack.BeforeSnapshot", da.BeforeSnapshot, db.BeforeSnapshot);
                AttackSource(p + ".DirectAttack.Action", da.Action, db.Action);
                // Random evidence precedes damage: every rejected word, then stream states/counters.
                S(p + ".DirectAttack.DamageFacts", da.DamageFacts, db.DamageFacts,
                    (q, x, y) => Crit(q + ".Crit", x.Crit, y.Crit));
                Random(p + ".DirectAttack.Random", da.Random, db.Random);
            }
            Random(p + ".AfterSnapshot.Random", a.AfterSnapshot.Random, b.AfterSnapshot.Random);
            if (direct)
            {
                S(p + ".DirectAttack.DamageFacts", da.DamageFacts, db.DamageFacts, Damage);
                S(p + ".DirectAttack.Members", da.Members, db.Members, MemberState);
                S(p + ".DirectAttack.Enemies", da.Enemies, db.Enemies, EnemyState);
                S(p + ".DirectAttack.Contributions", da.Contributions, db.Contributions, Totals);
            }
            var ea = a.EnemyPhase; var eb = b.EnemyPhase;
            if (Open(p + ".EnemyPhase", ea, eb))
            {
                S(p + ".EnemyPhase.Members", ea.Members, eb.Members, MemberState);
                S(p + ".EnemyPhase.Enemies", ea.Enemies, eb.Enemies, EnemyState);
                S(p + ".EnemyPhase.Contributions", ea.Contributions, eb.Contributions, Totals);
                Random(p + ".EnemyPhase.Random", ea.Random, eb.Random);
                V(p + ".EnemyPhase.EnemyPhaseOrdinal", ea.EnemyPhaseOrdinal, eb.EnemyPhaseOrdinal);
                S(p + ".EnemyPhase.OrderedIntents", ea.OrderedIntents, eb.OrderedIntents, EnemyFact);
            }
            Stage(p + ".StageDecision", a.StageDecision, b.StageDecision);
            S(p + ".OrderedFacts", a.OrderedFacts, b.OrderedFacts, Ordered);
            S(p + ".ContributionSegments", a.ContributionSegments, b.ContributionSegments, Segment);
            Snapshot(p + ".AfterSnapshot", a.AfterSnapshot, b.AfterSnapshot);
        }

        private void Context(string p, PreparedRuleContext a, PreparedRuleContext b)
        {
            if (!Open(p, a, b)) return;
            V(p + ".Kind", a is PreparedPublishedRuleContext ? "Published" : "Candidate", b is PreparedPublishedRuleContext ? "Published" : "Candidate");
            if (a is PreparedCandidateContext x && b is PreparedCandidateContext y)
            {
                V(p + ".DraftId", x.DraftId, y.DraftId); V(p + ".DraftRevision", x.DraftRevision, y.DraftRevision);
                S(p + ".SourceNotes", x.SourceNotes, y.SourceNotes, (q, u, v) => V(q, u, v));
            }
            else if (a is PreparedPublishedRuleContext u && b is PreparedPublishedRuleContext v)
                V(p + ".PackageId", u.Binding.PackageId, v.Binding.PackageId);
            V(p + ".ContentFingerprint", a.ContentFingerprint, b.ContentFingerprint);
            V(p + ".RuleVersion", a.RuleVersion, b.RuleVersion);
            V(p + ".NumericContractVersion", a.NumericContractVersion, b.NumericContractVersion);
            V(p + ".RandomContractVersion", a.RandomContractVersion, b.RandomContractVersion);
        }

        private void Stats(string p, PreparedStats a, PreparedStats b)
        {
            if (!Open(p, a, b)) return;
            R(p + ".MaxHp", a.MaxHp, b.MaxHp);
            R(p + ".Attack", a.Attack, b.Attack);
            R(p + ".PhysicalDefense", a.PhysicalDefense, b.PhysicalDefense);
            R(p + ".MagicDefense", a.MagicDefense, b.MagicDefense);
            R(p + ".Evasion", a.Evasion, b.Evasion);
            V(p + ".AttackRange", a.AttackRange, b.AttackRange);
        }

        private void CritParameters(string p, PreparedWarriorCrit a, PreparedWarriorCrit b)
        {
            if (!Open(p, a, b)) return;
            V(p + ".PassiveDefinitionId", a.PassiveDefinitionId, b.PassiveDefinitionId);
            R(p + ".TargetProbability", a.TargetProbability, b.TargetProbability);
            R(p + ".C", a.C, b.C);
            R(p + ".Multiplier", a.Multiplier, b.Multiplier);
        }

        private void Member(string p, PreparedMember a, PreparedMember b)
        {
            if (!Open(p, a, b)) return;
            V(p + ".CharacterId", a.CharacterId, b.CharacterId);
            V(p + ".ClassId", a.ClassId, b.ClassId);
            V(p + ".ClassKind", a.ClassKind, b.ClassKind);
            V(p + ".OriginalSlot", a.OriginalSlot, b.OriginalSlot);
            V(p + ".Level", a.Level, b.Level);
            V(p + ".IsReady", a.IsReady, b.IsReady);
            V(p + ".StatsOrigin", a.StatsOrigin, b.StatsOrigin);
            Context(p + ".StatsContext", a.StatsContext, b.StatsContext);
            Stats(p + ".Stats", a.Stats, b.Stats);
            R(p + ".EntryHp", a.EntryHp, b.EntryHp);
            S(p + ".LearnedSkills", a.LearnedSkills, b.LearnedSkills, (q, x, y) => V(q, x, y));
            CritParameters(p + ".Crit", a.Crit, b.Crit);
        }

        private void Intent(string p, PreparedEnemyIntent a, PreparedEnemyIntent b)
        {
            if (!Open(p, a, b)) return;
            V(p + ".Kind", a.Kind, b.Kind);
            V(p + ".Targeting", a.Targeting, b.Targeting);
            V(p + ".DamageKind", a.DamageKind, b.DamageKind);
            R(p + ".DamageCoefficient", a.DamageCoefficient, b.DamageCoefficient);
        }

        private void Enemy(string p, PreparedEnemy a, PreparedEnemy b)
        {
            if (!Open(p, a, b)) return;
            V(p + ".EnemyInstanceKey", a.EnemyInstanceKey, b.EnemyInstanceKey);
            V(p + ".EnemyDefinitionId", a.EnemyDefinitionId, b.EnemyDefinitionId);
            V(p + ".OriginalSlot", a.OriginalSlot, b.OriginalSlot);
            V(p + ".StableOrder", a.StableOrder, b.StableOrder);
            V(p + ".Behavior", a.Behavior, b.Behavior);
            Stats(p + ".Stats", a.Stats, b.Stats);
            S(p + ".IntentCycle", a.IntentCycle, b.IntentCycle, Intent);
        }

        private void Pair(string p, PreparedPair a, PreparedPair b)
        {
            if (!Open(p, a, b)) return;
            V(p + ".PairId", a.PairId, b.PairId);
            V(p + ".GeometryColorId", a.GeometryColorId, b.GeometryColorId);
            Position(p + ".EndpointA", a.EndpointA, b.EndpointA);
            Position(p + ".EndpointB", a.EndpointB, b.EndpointB);
            Enemy(p + ".Enemy", a.Enemy, b.Enemy);
        }

        private void Face(string p, PreparedFace a, PreparedFace b)
        {
            if (!Open(p, a, b)) return;
            V(p + ".FaceId", a.FaceId, b.FaceId);
            V(p + ".Width", a.Width, b.Width);
            V(p + ".Height", a.Height, b.Height);
            S(p + ".Pairs", a.Pairs, b.Pairs, Pair);
        }

        private void Level(string p, PreparedLevel a, PreparedLevel b)
        {
            if (!Open(p, a, b)) return;
            V(p + ".LevelId", a.LevelId, b.LevelId);
            V(p + ".LevelVersion", a.LevelVersion, b.LevelVersion);
            V(p + ".RecommendedLevel", a.RecommendedLevel, b.RecommendedLevel);
            S(p + ".Faces", a.Faces, b.Faces, Face);
        }

        private void Entry(string p, PreparedBattleEntry a, PreparedBattleEntry b)
        {
            if (!Open(p, a, b)) return;
            V(p + ".PlayerId", a.PlayerId, b.PlayerId);
            V(p + ".ChallengeId", a.ChallengeId, b.ChallengeId);
            V(p + ".AttemptId", a.AttemptId, b.AttemptId);
            V(p + ".EntryBaselineId", a.EntryBaselineId, b.EntryBaselineId);
            Context(p + ".Context", a.Context, b.Context);
            Level(p + ".Level", a.Level, b.Level);
            S(p + ".Members", a.Members, b.Members, Member);
            S(p + ".ReadyParticipants", a.ReadyParticipants, b.ReadyParticipants, Member);
            V(p + ".CarryMode", a.CarryMode, b.CarryMode);
            S(p + ".RequiredFeatures", a.RequiredFeatures, b.RequiredFeatures, (q, x, y) => V(q, x, y));
        }

        private void Combatant(string p, BattleCombatantKey a, BattleCombatantKey b)
        {
            if (!Open(p, a, b)) return;
            V(p + ".AttemptId", a.AttemptId, b.AttemptId);
            V(p + ".Kind", a.Kind, b.Kind);
            V(p + ".CharacterId", a.CharacterId, b.CharacterId);
            V(p + ".FaceId", a.FaceId, b.FaceId);
            V(p + ".EnemyInstanceKey", a.EnemyInstanceKey, b.EnemyInstanceKey);
        }

        private void PairKey(string p, BattlePairKey a, BattlePairKey b)
        {
            if (!Open(p, a, b)) return;
            V(p + ".AttemptId", a.AttemptId, b.AttemptId);
            V(p + ".FaceId", a.FaceId, b.FaceId);
            V(p + ".PairId", a.PairId, b.PairId);
        }

        private void Prd(string p, BattlePrdState a, BattlePrdState b)
        {
            if (!Open(p, a, b)) return;
            Combatant(p + ".CombatantKey", a.CombatantKey, b.CombatantKey);
            CritParameters(p + ".Crit", a.Crit, b.Crit);
            V(p + ".FailureCount", a.FailureCount, b.FailureCount);
        }

        private void Initials(string p, BattleRandomInitials a, BattleRandomInitials b)
        {
            if (!Open(p, a, b)) return;
            Stream(p + ".Battle", a.Battle, b.Battle);
            Stream(p + ".BaseReward", a.BaseReward, b.BaseReward);
            Stream(p + ".Bonus", a.Bonus, b.Bonus);
        }

        private void Baseline(string p, BattleEntryBaseline a, BattleEntryBaseline b)
        {
            if (!Open(p, a, b)) return;
            Entry(p + ".Entry", a.Entry, b.Entry);
            Initials(p + ".RandomInitials", a.RandomInitials, b.RandomInitials);
            S(p + ".PrdInitialStates", a.PrdInitialStates, b.PrdInitialStates, Prd);
        }

        private void Domain(string p, CandidateRandomDomain a, CandidateRandomDomain b)
        {
            if (!Open(p, a, b)) return;
            V(p + ".Purpose", a.Purpose, b.Purpose);
            V(p + ".InitState", a.InitState, b.InitState);
            V(p + ".InitSequence", a.InitSequence, b.InitSequence);
            Stream(p + ".Initial", a.Initial, b.Initial);
        }

        private void Binding(string p, CandidateRandomBinding a, CandidateRandomBinding b)
        {
            if (!Open(p, a, b)) return;
            Baseline(p + ".Start.Baseline", a.Start.Baseline, b.Start.Baseline);
            Snapshot(p + ".Start.Snapshot", a.Start.Snapshot, b.Start.Snapshot);
            Context(p + ".Context", a.Context, b.Context);
            V(p + ".GeneratedForPlayerId", a.GeneratedForPlayerId, b.GeneratedForPlayerId);
            V(p + ".GeneratedForChallengeId", a.GeneratedForChallengeId, b.GeneratedForChallengeId);
            V(p + ".GeneratedForAttemptId", a.GeneratedForAttemptId, b.GeneratedForAttemptId);
            V(p + ".GeneratedForEntryBaselineId", a.GeneratedForEntryBaselineId, b.GeneratedForEntryBaselineId);
            V(p + ".SourceCapabilityId", a.SourceCapabilityId, b.SourceCapabilityId);
            V(p + ".MappingId", a.MappingId, b.MappingId);
            Domain(p + ".Battle", a.Battle, b.Battle);
            Domain(p + ".BaseReward", a.BaseReward, b.BaseReward);
            Domain(p + ".Bonus", a.Bonus, b.Bonus);
        }

        private void Random(string p, BattleRandomSnapshot a, BattleRandomSnapshot b)
        {
            if (!Open(p, a, b)) return;
            Stream(p + ".Stream", a.Stream, b.Stream);
            S(p + ".PrdStates", a.PrdStates, b.PrdStates, Prd);
        }

        private void Locked(string p, BattleLockedRoute a, BattleLockedRoute b)
        {
            if (!Open(p, a, b)) return;
            PairKey(p + ".PairKey", a.PairKey, b.PairKey);
            S(p + ".Route", a.Route, b.Route, Position);
        }

        private void Board(string p, BattleBoardState a, BattleBoardState b)
        {
            if (!Open(p, a, b)) return;
            Face(p + ".Face", a.Face, b.Face);
            S(p + ".LockedRoutes", a.LockedRoutes, b.LockedRoutes, Locked);
            S(p + ".PendingLinks", a.PendingLinks, b.PendingLinks, PairKey);
        }

        private void MemberState(string p, BattleMemberState a, BattleMemberState b)
        {
            if (!Open(p, a, b)) return;
            Combatant(p + ".CombatantKey", a.CombatantKey, b.CombatantKey);
            Member(p + ".Member", a.Member, b.Member);
            V(p + ".OriginalSlot", a.OriginalSlot, b.OriginalSlot);
            R(p + ".Hp", a.Hp, b.Hp);
        }

        private void EnemyState(string p, BattleEnemyState a, BattleEnemyState b)
        {
            if (!Open(p, a, b)) return;
            Combatant(p + ".CombatantKey", a.CombatantKey, b.CombatantKey);
            PairKey(p + ".PairKey", a.PairKey, b.PairKey);
            Enemy(p + ".Enemy", a.Enemy, b.Enemy);
            V(p + ".OriginalSlot", a.OriginalSlot, b.OriginalSlot);
            V(p + ".StableOrder", a.StableOrder, b.StableOrder);
            R(p + ".Hp", a.Hp, b.Hp);
            V(p + ".IntentCursor", a.IntentCursor, b.IntentCursor);
        }

        private void Totals(string p, BattleContributionTotals a, BattleContributionTotals b)
        {
            if (!Open(p, a, b)) return;
            Combatant(p + ".CombatantKey", a.CombatantKey, b.CombatantKey);
            R(p + ".EffectiveDamageDealtHp", a.EffectiveDamageDealtHp, b.EffectiveDamageDealtHp);
            R(p + ".EffectiveDamageTakenHp", a.EffectiveDamageTakenHp, b.EffectiveDamageTakenHp);
        }

        private void Snapshot(string p, BattleSnapshot a, BattleSnapshot b)
        {
            if (!Open(p, a, b)) return;
            Baseline(p + ".Baseline", a.Baseline, b.Baseline);
            V(p + ".SceneRevision", a.SceneRevision, b.SceneRevision);
            V(p + ".EffectiveActionsCompleted", a.EffectiveActionsCompleted, b.EffectiveActionsCompleted);
            V(p + ".EnemyPhasesCompleted", a.EnemyPhasesCompleted, b.EnemyPhasesCompleted);
            V(p + ".CurrentFaceIndex", a.CurrentFaceIndex, b.CurrentFaceIndex);
            V(p + ".Phase", a.Phase, b.Phase);
            V(p + ".CarryMode", a.CarryMode, b.CarryMode);
            Board(p + ".Board", a.Board, b.Board);
            S(p + ".Members", a.Members, b.Members, MemberState);
            S(p + ".Enemies", a.Enemies, b.Enemies, EnemyState);
            Random(p + ".Random", a.Random, b.Random);
            S(p + ".Contributions", a.Contributions, b.Contributions, Totals);
        }

        private void Conditions(string p, CandidateBattleConditionValues a, CandidateBattleConditionValues b)
        {
            if (!Open(p, a, b)) return;
            V(p + ".PreferenceRevision", a.PreferenceRevision, b.PreferenceRevision);
            V(p + ".ItemUseEnabled", a.ItemUseEnabled, b.ItemUseEnabled);
        }

        private void Source(string p, CandidateStageSource a, CandidateStageSource b)
        {
            if (!Open(p, a, b)) return;
            V(p + ".Kind", a.Kind, b.Kind);
            V(p + ".PlayerId", a.PlayerId, b.PlayerId);
            V(p + ".AttemptId", a.AttemptId, b.AttemptId);
            V(p + ".OperationId", a.OperationId, b.OperationId);
            V(p + ".ExpectedSceneRevision", a.ExpectedSceneRevision, b.ExpectedSceneRevision);
            Combatant(p + ".Actor", a.Actor, b.Actor);
            PairKey(p + ".Pair", a.Pair, b.Pair);
            S(p + ".Route", a.Route, b.Route, Position);
        }

        private void AttackSource(string p, CandidateAttackSource a, CandidateAttackSource b)
        {
            if (!Open(p, a, b)) return;
            V(p + ".PlayerId", a.PlayerId, b.PlayerId);
            V(p + ".AttemptId", a.AttemptId, b.AttemptId);
            V(p + ".OperationId", a.OperationId, b.OperationId);
            V(p + ".ExpectedSceneRevision", a.ExpectedSceneRevision, b.ExpectedSceneRevision);
            V(p + ".ActionOrdinal", a.ActionOrdinal, b.ActionOrdinal);
            Combatant(p + ".Actor", a.Actor, b.Actor);
            PairKey(p + ".Pair", a.Pair, b.Pair);
            S(p + ".Route", a.Route, b.Route, Position);
        }

        private void Crit(string p, CandidateCritFact a, CandidateCritFact b)
        {
            if (!Open(p, a, b)) return;
            Combatant(p + ".Actor", a.Actor, b.Actor);
            Combatant(p + ".Target", a.Target, b.Target);
            V(p + ".OpportunityOrdinal", a.OpportunityOrdinal, b.OpportunityOrdinal);
            CritParameters(p + ".Parameters", a.Parameters, b.Parameters);
            R(p + ".Probability", a.Probability, b.Probability);
            V(p + ".FailureCountBefore", a.FailureCountBefore, b.FailureCountBefore);
            V(p + ".FailureCountAfter", a.FailureCountAfter, b.FailureCountAfter);
            V(p + ".Triggered", a.Triggered, b.Triggered);
            S(p + ".Words", a.Words, b.Words, (q, x, y) => V(q, x, y));
            Stream(p + ".StreamBefore", a.StreamBefore, b.StreamBefore);
            Stream(p + ".StreamAfter", a.StreamAfter, b.StreamAfter);
        }

        private void Damage(string p, BattleDamageFact a, BattleDamageFact b)
        {
            if (!Open(p, a, b)) return;
            Baseline(p + ".Baseline", a.Baseline, b.Baseline);
            V(p + ".PlayerId", a.PlayerId, b.PlayerId);
            V(p + ".AttemptId", a.AttemptId, b.AttemptId);
            V(p + ".FaceId", a.FaceId, b.FaceId);
            V(p + ".OperationId", a.OperationId, b.OperationId);
            V(p + ".SceneRevision", a.SceneRevision, b.SceneRevision);
            V(p + ".ActionOrdinal", a.ActionOrdinal, b.ActionOrdinal);
            Combatant(p + ".Actor", a.Actor, b.Actor);
            Combatant(p + ".Target", a.Target, b.Target);
            PairKey(p + ".Pair", a.Pair, b.Pair);
            V(p + ".EffectIndex", a.EffectIndex, b.EffectIndex);
            V(p + ".DamageKind", a.DamageKind, b.DamageKind);
            R(p + ".Attack", a.Attack, b.Attack);
            R(p + ".PhysicalDefense", a.PhysicalDefense, b.PhysicalDefense);
            R(p + ".Multiplier", a.Multiplier, b.Multiplier);
            R(p + ".RawDamage", a.RawDamage, b.RawDamage);
            R(p + ".MitigatedDamage", a.MitigatedDamage, b.MitigatedDamage);
            V(p + ".RoundedDamage", a.RoundedDamage, b.RoundedDamage);
            R(p + ".BlockPrevented", a.BlockPrevented, b.BlockPrevented);
            R(p + ".ShieldAbsorbed", a.ShieldAbsorbed, b.ShieldAbsorbed);
            R(p + ".HpBefore", a.HpBefore, b.HpBefore);
            R(p + ".HpAfter", a.HpAfter, b.HpAfter);
            R(p + ".HpLoss", a.HpLoss, b.HpLoss);
            R(p + ".Overflow", a.Overflow, b.Overflow);
            V(p + ".DefeatedTarget", a.DefeatedTarget, b.DefeatedTarget);
            Crit(p + ".Crit", a.Crit, b.Crit);
        }

        private void EnemyDamage(string p, CandidateEnemyDamageFact a, CandidateEnemyDamageFact b)
        {
            if (!Open(p, a, b)) return;
            Combatant(p + ".ActorEnemy", a.ActorEnemy, b.ActorEnemy);
            Combatant(p + ".TargetMember", a.TargetMember, b.TargetMember);
            V(p + ".DamageKind", a.DamageKind, b.DamageKind);
            R(p + ".Attack", a.Attack, b.Attack);
            R(p + ".DamageCoefficient", a.DamageCoefficient, b.DamageCoefficient);
            R(p + ".PhysicalDefense", a.PhysicalDefense, b.PhysicalDefense);
            R(p + ".RawDamage", a.RawDamage, b.RawDamage);
            R(p + ".MitigatedDamage", a.MitigatedDamage, b.MitigatedDamage);
            V(p + ".RoundedDamage", a.RoundedDamage, b.RoundedDamage);
            R(p + ".BlockPrevented", a.BlockPrevented, b.BlockPrevented);
            R(p + ".ShieldAbsorbed", a.ShieldAbsorbed, b.ShieldAbsorbed);
            R(p + ".HpBefore", a.HpBefore, b.HpBefore);
            R(p + ".HpAfter", a.HpAfter, b.HpAfter);
            R(p + ".HpLoss", a.HpLoss, b.HpLoss);
            R(p + ".Overflow", a.Overflow, b.Overflow);
            V(p + ".DefeatedTarget", a.DefeatedTarget, b.DefeatedTarget);
        }

        private void EnemyFact(string p, CandidateEnemyIntentFact a, CandidateEnemyIntentFact b)
        {
            if (!Open(p, a, b)) return;
            Baseline(p + ".Baseline", a.Baseline, b.Baseline);
            V(p + ".PlayerId", a.PlayerId, b.PlayerId);
            V(p + ".AttemptId", a.AttemptId, b.AttemptId);
            V(p + ".OperationId", a.OperationId, b.OperationId);
            V(p + ".FaceId", a.FaceId, b.FaceId);
            V(p + ".SceneRevision", a.SceneRevision, b.SceneRevision);
            V(p + ".ActionOrdinal", a.ActionOrdinal, b.ActionOrdinal);
            V(p + ".EnemyPhaseOrdinal", a.EnemyPhaseOrdinal, b.EnemyPhaseOrdinal);
            V(p + ".SegmentIndex", a.SegmentIndex, b.SegmentIndex);
            Combatant(p + ".EnemyKey", a.EnemyKey, b.EnemyKey);
            PairKey(p + ".Pair", a.Pair, b.Pair);
            V(p + ".StableOrder", a.StableOrder, b.StableOrder);
            V(p + ".IntentKind", a.IntentKind, b.IntentKind);
            V(p + ".CursorBefore", a.CursorBefore, b.CursorBefore);
            V(p + ".CursorAfter", a.CursorAfter, b.CursorAfter);
            EnemyDamage(p + ".Damage", a.Damage, b.Damage);
        }

        private void Hp(string p, CandidateHpValue a, CandidateHpValue b)
        {
            if (!Open(p, a, b)) return;
            Combatant(p + ".CombatantKey", a.CombatantKey, b.CombatantKey);
            R(p + ".Hp", a.Hp, b.Hp);
        }

        private void FinalHp(string p, CandidateFinalHpValues a, CandidateFinalHpValues b)
        {
            if (!Open(p, a, b)) return;
            S(p + ".MemberHp", a.MemberHp, b.MemberHp, Hp);
            S(p + ".EnemyHp", a.EnemyHp, b.EnemyHp, Hp);
        }

        private void StageFact(string p, CandidateStageFact a, CandidateStageFact b)
        {
            if (!Open(p, a, b)) return;
            V(p + ".Kind", a.Kind, b.Kind);
            V(p + ".OperationId", a.OperationId, b.OperationId);
            V(p + ".SceneRevision", a.SceneRevision, b.SceneRevision);
            V(p + ".FaceId", a.FaceId, b.FaceId);
            PairKey(p + ".Pair", a.Pair, b.Pair);
            V(p + ".SegmentIndex", a.SegmentIndex, b.SegmentIndex);
            V(p + ".NextFaceId", a.NextFaceId, b.NextFaceId);
            V(p + ".Phase", a.Phase, b.Phase);
        }

        private void Stage(string p, CandidateStageDecision a, CandidateStageDecision b)
        {
            if (!Open(p, a, b)) return;
            Snapshot(p + ".BeforeSnapshot", a.BeforeSnapshot, b.BeforeSnapshot);
            Baseline(p + ".Baseline", a.Baseline, b.Baseline);
            Source(p + ".Source", a.Source, b.Source);
            FinalHp(p + ".FinalHp", a.FinalHp, b.FinalHp);
            Board(p + ".Board", a.Board, b.Board);
            V(p + ".NextFaceIndex", a.NextFaceIndex, b.NextFaceIndex);
            V(p + ".NextPhase", a.NextPhase, b.NextPhase);
            V(p + ".DidFlipFace", a.DidFlipFace, b.DidFlipFace);
            Face(p + ".NextFace", a.NextFace, b.NextFace);
            S(p + ".OrderedFacts", a.OrderedFacts, b.OrderedFacts, StageFact);
        }

        private void Segment(string p, CandidateContributionSegment a, CandidateContributionSegment b)
        {
            if (!Open(p, a, b)) return;
            V(p + ".OperationId", a.OperationId, b.OperationId);
            V(p + ".SceneRevision", a.SceneRevision, b.SceneRevision);
            V(p + ".FaceId", a.FaceId, b.FaceId);
            V(p + ".RuleSegment", a.RuleSegment, b.RuleSegment);
            V(p + ".SegmentIndex", a.SegmentIndex, b.SegmentIndex);
            Combatant(p + ".Actor", a.Actor, b.Actor);
            Combatant(p + ".Target", a.Target, b.Target);
            Combatant(p + ".Beneficiary", a.Beneficiary, b.Beneficiary);
            V(p + ".Kind", a.Kind, b.Kind);
            R(p + ".HpLoss", a.HpLoss, b.HpLoss);
            V(p + ".FactIndex", a.FactIndex, b.FactIndex);
        }

        private void Ordered(string p, CandidateBattleOrderedFact a, CandidateBattleOrderedFact b)
        {
            if (!Open(p, a, b)) return;
            V(p + ".Index", a.Index, b.Index);
            V(p + ".Kind", a.Kind, b.Kind);
            Damage(p + ".DirectAttack", a.DirectAttack, b.DirectAttack);
            EnemyFact(p + ".EnemyIntent", a.EnemyIntent, b.EnemyIntent);
            StageFact(p + ".Stage", a.Stage, b.Stage);
        }

        private void Report(string p, CandidateFinalAttemptReport a, CandidateFinalAttemptReport b)
        {
            if (!Open(p, a, b)) return;
            Binding(p + ".Binding", a.Binding, b.Binding);
            Baseline(p + ".Baseline", a.Baseline, b.Baseline);
            Snapshot(p + ".InitialSnapshot", a.InitialSnapshot, b.InitialSnapshot);
            S(p + ".Operations", a.Operations, b.Operations, Record);
            Snapshot(p + ".FinalSnapshot", a.FinalSnapshot, b.FinalSnapshot);
            S(p + ".Contributions", a.Contributions, b.Contributions, Segment);
            V(p + ".Outcome", a.Outcome, b.Outcome);
            V(p + ".EndedAtUnixMilliseconds", a.EndedAtUnixMilliseconds, b.EndedAtUnixMilliseconds);
            V(p + ".TerminalOperationId", a.TerminalOperationId, b.TerminalOperationId);
            V(p + ".ConsumptionCoverage", a.ConsumptionCoverage, b.ConsumptionCoverage);
            R(p + ".WholeLevelInitialEnemyHp", a.WholeLevelInitialEnemyHp, b.WholeLevelInitialEnemyHp);
            V(p + ".CommitEligible", a.CommitEligible, b.CommitEligible);
            V(p + ".Fingerprint", a.Fingerprint, b.Fingerprint);
            V(p + ".PlayerId", a.PlayerId, b.PlayerId);
            V(p + ".AttemptId", a.AttemptId, b.AttemptId);
            V(p + ".ChallengeId", a.ChallengeId, b.ChallengeId);
            V(p + ".EntryBaselineId", a.EntryBaselineId, b.EntryBaselineId);
            Level(p + ".Level", a.Level, b.Level);
        }
    }
}

