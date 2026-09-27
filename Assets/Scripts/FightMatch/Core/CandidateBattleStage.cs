using System;
using System.Collections.Generic;
using System.Numerics;
using FlowPuzzle.Core;
using static FightMatch.Core.CandidateStageRejectionCode;

namespace FightMatch.Core
{
    public static class CandidateBattleStage
    {
        public static CandidateStageResult AfterAttack(BattleSnapshot before, CandidateAttackRequest attack,
            CandidateFinalHpProjection finalHp, ExactMathBudget budget)
        {
            if (before == null) throw new ArgumentNullException(nameof(before));
            if (attack == null) throw new ArgumentNullException(nameof(attack));
            if (finalHp == null) throw new ArgumentNullException(nameof(finalHp));
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            var rejection = RequiredRequest(attack.PlayerId, attack.AttemptId, attack.OperationId,
                attack.ExpectedSceneRevision, attack.Pair, attack.Route);
            if (rejection != null) return rejection;
            if (attack.Actor == null) return Reject(MissingField, "Actor");
            rejection = RequiredHp(finalHp.MemberHp, "FinalHp.MemberHp") ?? RequiredHp(finalHp.EnemyHp, "FinalHp.EnemyHp");
            if (rejection != null) return rejection;
            rejection = CheckNumbers(before, attack.ExpectedSceneRevision.Value, attack.Route, budget);
            if (rejection != null) return rejection;
            foreach (var row in finalHp.MemberHp) CheckNumber(row.Hp, budget);
            foreach (var row in finalHp.EnemyHp) CheckNumber(row.Hp, budget);
            rejection = CheckIdentity(before, attack.PlayerId, attack.AttemptId, attack.ExpectedSceneRevision.Value, budget)
                ?? CheckBefore(before, BattlePhase.AwaitAction, budget);
            if (rejection != null) return rejection;
            BattleMemberState actor = null;
            foreach (var member in before.Members) if (member.CombatantKey.Equals(attack.Actor)) actor = member;
            if (actor == null) return Reject(InconsistentBinding, "Actor");
            if (actor.Hp.Numerator.IsZero) return Reject(ActorUnavailable, "Actor");
            var target = FindEnemy(before, attack.Pair);
            if (target == null || target.Hp.Numerator.IsZero) return Reject(TargetUnavailable, "Pair");
            rejection = CheckProjection(before, finalHp, budget) ?? CheckRoute(before, attack.Pair, attack.Route);
            if (rejection != null) return rejection;

            var source = new CandidateStageSource(CandidateStageOperation.AfterAttack, attack.PlayerId,
                attack.AttemptId, attack.OperationId, attack.ExpectedSceneRevision.Value, attack.Actor, attack.Pair, attack.Route);
            var values = new CandidateFinalHpValues(finalHp);
            var locked = new List<BattleLockedRoute>(before.Board.LockedRoutes);
            var facts = new List<CandidateStageFact>();
            var targetDead = HpFor(values.EnemyHp, target.CombatantKey).Numerator.IsZero;
            if (targetDead) locked.Add(new BattleLockedRoute(attack.Pair, attack.Route));
            AddFact(facts, targetDead ? CandidateStageFactKind.RouteLocked : CandidateStageFactKind.TemporaryRouteRemoved,
                before, source, attack.Pair);
            var pending = new List<BattlePairKey>();
            foreach (var pair in before.Board.Face.Pairs)
            {
                var key = PairKey(before, pair);
                if (HpFor(values.EnemyHp, FindEnemy(before, key).CombatantKey).Numerator.IsZero && !IsLocked(locked, key))
                {
                    pending.Add(key);
                    AddFact(facts, CandidateStageFactKind.PendingLinkAdded, before, source, key);
                }
            }
            return Finish(before, source, values, locked, pending, facts);
        }

        public static CandidateStageResult CompleteLink(BattleSnapshot before, CandidateLinkRequest link, ExactMathBudget budget)
        {
            if (before == null) throw new ArgumentNullException(nameof(before));
            if (link == null) throw new ArgumentNullException(nameof(link));
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            var rejection = RequiredRequest(link.PlayerId, link.AttemptId, link.OperationId,
                link.ExpectedSceneRevision, link.Pair, link.Route);
            if (rejection != null) return rejection;
            rejection = CheckNumbers(before, link.ExpectedSceneRevision.Value, link.Route, budget);
            if (rejection != null) return rejection;
            rejection = CheckIdentity(before, link.PlayerId, link.AttemptId, link.ExpectedSceneRevision.Value, budget)
                ?? CheckBefore(before, BattlePhase.AwaitLinks, budget);
            if (rejection != null) return rejection;
            if (!Contains(before.Board.PendingLinks, link.Pair)) return Reject(NotPendingLink, "Pair");
            rejection = CheckRoute(before, link.Pair, link.Route);
            if (rejection != null) return rejection;
            var source = new CandidateStageSource(CandidateStageOperation.CompleteLink, link.PlayerId,
                link.AttemptId, link.OperationId, link.ExpectedSceneRevision.Value, null, link.Pair, link.Route);
            var projection = new CandidateFinalHpProjection
            { MemberHp = new List<CandidateHpInput>(), EnemyHp = new List<CandidateHpInput>() };
            foreach (var row in before.Members) projection.MemberHp.Add(new CandidateHpInput { CombatantKey = row.CombatantKey, Hp = row.Hp });
            foreach (var row in before.Enemies) projection.EnemyHp.Add(new CandidateHpInput { CombatantKey = row.CombatantKey, Hp = row.Hp });
            var locked = new List<BattleLockedRoute>(before.Board.LockedRoutes) { new BattleLockedRoute(link.Pair, link.Route) };
            var pending = new List<BattlePairKey>();
            foreach (var pair in before.Board.Face.Pairs)
            {
                var key = PairKey(before, pair);
                if (!key.Equals(link.Pair) && Contains(before.Board.PendingLinks, key)) pending.Add(key);
            }
            var facts = new List<CandidateStageFact>();
            AddFact(facts, CandidateStageFactKind.RouteLocked, before, source, link.Pair);
            AddFact(facts, CandidateStageFactKind.PendingLinkRemoved, before, source, link.Pair);
            return Finish(before, source, new CandidateFinalHpValues(projection), locked, pending, facts);
        }

        private static CandidateStageResult Finish(BattleSnapshot before, CandidateStageSource source,
            CandidateFinalHpValues hp, List<BattleLockedRoute> locked, List<BattlePairKey> pending, List<CandidateStageFact> facts)
        {
            var liveMembers = false; var liveEnemies = false;
            foreach (var row in hp.MemberHp) if (row.Hp.Numerator.Sign > 0) liveMembers = true;
            foreach (var row in hp.EnemyHp) if (row.Hp.Numerator.Sign > 0) liveEnemies = true;
            var index = before.CurrentFaceIndex;
            PreparedFace nextFace = null;
            BattlePhase phase;
            if (pending.Count > 0) phase = BattlePhase.AwaitLinks;
            else if (liveEnemies) phase = liveMembers ? BattlePhase.AwaitAction : BattlePhase.AwaitRescue;
            else if (index + 1 < before.Baseline.Entry.Level.Faces.Count)
            {
                nextFace = before.Baseline.Entry.Level.Faces[++index];
                phase = liveMembers ? BattlePhase.AwaitAction : BattlePhase.AwaitRescue;
                AddFact(facts, CandidateStageFactKind.FaceChanged, before, source, null, nextFace.FaceId);
            }
            else phase = BattlePhase.WonPendingSettlement;
            AddFact(facts, CandidateStageFactKind.PhaseSelected, before, source, null, null, phase);
            var board = nextFace == null ? new BattleBoardState(before.Board.Face, locked, pending) : new BattleBoardState(nextFace);
            return new CandidateStageResult(new CandidateStageDecision(before, source, hp, board, index, phase, nextFace, facts));
        }

        private static CandidateStageResult RequiredRequest(string player, string attempt, string operation,
            BigInteger? revision, BattlePairKey pair, List<FlowPos> route)
        {
            if (string.IsNullOrWhiteSpace(player)) return Reject(MissingField, "PlayerId");
            if (string.IsNullOrWhiteSpace(attempt)) return Reject(MissingField, "AttemptId");
            if (string.IsNullOrWhiteSpace(operation)) return Reject(MissingField, "OperationId");
            if (!revision.HasValue) return Reject(MissingField, "ExpectedSceneRevision");
            if (pair == null) return Reject(MissingField, "Pair");
            return route == null ? Reject(MissingField, "Route") : null;
        }

        private static CandidateStageResult RequiredHp(List<CandidateHpInput> rows, string path)
        {
            if (rows == null) return Reject(MissingField, path);
            for (var i = 0; i < rows.Count; i++)
            {
                if (rows[i] == null) return Reject(MissingField, path + $"[{i}]");
                if (rows[i].CombatantKey == null) return Reject(MissingField, path + $"[{i}].CombatantKey");
                if (rows[i].Hp == null) return Reject(MissingField, path + $"[{i}].Hp");
            }
            return null;
        }

        private static CandidateStageResult CheckIdentity(BattleSnapshot before, string player, string attempt,
            BigInteger revision, ExactMathBudget math)
        {
            if (!string.Equals(player, before.Baseline.Entry.PlayerId, StringComparison.Ordinal)) return Reject(InconsistentBinding, "PlayerId");
            if (!string.Equals(attempt, before.Baseline.Entry.AttemptId, StringComparison.Ordinal)) return Reject(InconsistentBinding, "AttemptId");
            if (revision.Sign < 0) return Reject(InvalidValue, "ExpectedSceneRevision");
            return math.Compare(revision, before.SceneRevision) == 0 ? null : Reject(StaleContext, "ExpectedSceneRevision");
        }

        private static CandidateStageResult CheckBefore(BattleSnapshot before, BattlePhase required, ExactMathBudget math)
        {
            if (before.Phase != required) return Reject(InvalidPhase, "Before.Phase");
            if (before.SceneRevision.Sign <= 0) return Reject(InvalidValue, "Before.SceneRevision");
            if (before.EffectiveActionsCompleted.Sign < 0) return Reject(InvalidValue, "Before.EffectiveActionsCompleted");
            if (before.EnemyPhasesCompleted.Sign < 0) return Reject(InvalidValue, "Before.EnemyPhasesCompleted");
            var entry = before.Baseline.Entry;
            if (before.CarryMode != EntryCarryMode.Empty || entry.CarryMode != EntryCarryMode.Empty || entry.RequiredFeatures.Count != 0)
                return Reject(InconsistentBinding, "Before.CarryMode");
            if (before.CurrentFaceIndex < 0 || before.CurrentFaceIndex >= entry.Level.Faces.Count)
                return Reject(InvalidValue, "Before.CurrentFaceIndex");
            var face = entry.Level.Faces[before.CurrentFaceIndex];
            if (!ReferenceEquals(face, before.Board.Face)) return Reject(InconsistentBinding, "Before.Board.Face");
            if (before.Members.Count != entry.ReadyParticipants.Count) return Reject(InconsistentBinding, "Before.Members");
            var memberKeys = new HashSet<BattleCombatantKey>();
            for (var i = 0; i < before.Members.Count; i++)
            {
                var row = before.Members[i]; var path = $"Before.Members[{i}]";
                PreparedMember original = null;
                foreach (var member in entry.ReadyParticipants)
                    if (BattleCombatantKey.ForParticipant(entry.AttemptId, member.CharacterId).Equals(row.CombatantKey)) original = member;
                if (original == null || !memberKeys.Add(row.CombatantKey) || !ReferenceEquals(original, row.Member)) return Reject(InconsistentBinding, path);
                if (!InRange(row.Hp, original.Stats.MaxHp, math)) return Reject(InvalidValue, path + ".Hp");
            }
            if (before.Enemies.Count != face.Pairs.Count) return Reject(InconsistentBinding, "Before.Enemies");
            var enemyKeys = new HashSet<BattleCombatantKey>();
            for (var i = 0; i < before.Enemies.Count; i++)
            {
                var row = before.Enemies[i]; var path = $"Before.Enemies[{i}]";
                var original = FindPair(before, row.PairKey);
                if (original == null || !ReferenceEquals(original.Enemy, row.Enemy) || !enemyKeys.Add(row.CombatantKey) ||
                    !BattleCombatantKey.ForEnemy(entry.AttemptId, face.FaceId, original.Enemy.EnemyInstanceKey).Equals(row.CombatantKey))
                    return Reject(InconsistentBinding, path);
                if (!InRange(row.Hp, original.Enemy.Stats.MaxHp, math)) return Reject(InvalidValue, path + ".Hp");
                if (row.IntentCursor.Sign < 0) return Reject(InvalidValue, path + ".IntentCursor");
            }
            if (before.Contributions.Count != memberKeys.Count) return Reject(InconsistentBinding, "Before.Contributions");
            var totals = new HashSet<BattleCombatantKey>();
            foreach (var row in before.Contributions)
            {
                if (!memberKeys.Contains(row.CombatantKey) || !totals.Add(row.CombatantKey)) return Reject(InconsistentBinding, "Before.Contributions");
                if (row.EffectiveDamageDealtHp.Numerator.Sign < 0 || row.EffectiveDamageTakenHp.Numerator.Sign < 0)
                    return Reject(InvalidValue, "Before.Contributions");
            }
            var lockedKeys = new HashSet<BattlePairKey>(); var lockedPaths = new List<FlowPathData>();
            var level = Geometry(face);
            for (var i = 0; i < before.Board.LockedRoutes.Count; i++)
            {
                var row = before.Board.LockedRoutes[i]; var path = $"Before.Board.LockedRoutes[{i}]";
                var enemy = FindEnemy(before, row.PairKey);
                if (enemy == null || !enemy.Hp.Numerator.IsZero || !lockedKeys.Add(row.PairKey)) return Reject(InconsistentBinding, path);
                var pair = FindPair(before, row.PairKey);
                var result = new BattleRouteValidator().Validate(level, lockedPaths, Array.Empty<FlowPos>(), pair.GeometryColorId, row.Route);
                if (!result.IsValid) return new CandidateStageResult(InvalidRoute, path + ".Route", result);
                lockedPaths.Add(new FlowPathData { colorId = pair.GeometryColorId, cells = new List<FlowPos>(row.Route) });
            }
            var pending = new HashSet<BattlePairKey>();
            foreach (var key in before.Board.PendingLinks)
            {
                var enemy = FindEnemy(before, key);
                if (enemy == null || !enemy.Hp.Numerator.IsZero || lockedKeys.Contains(key) || !pending.Add(key))
                    return Reject(InconsistentBinding, "Before.Board.PendingLinks");
            }
            foreach (var enemy in before.Enemies)
                if (enemy.Hp.Numerator.IsZero != (lockedKeys.Contains(enemy.PairKey) || pending.Contains(enemy.PairKey)))
                    return Reject(InconsistentBinding, "Before.Board");
            if (required == BattlePhase.AwaitAction ? pending.Count != 0 : pending.Count == 0)
                return Reject(InvalidPhase, "Before.Board.PendingLinks");
            return null;
        }

        private static CandidateStageResult CheckProjection(BattleSnapshot before, CandidateFinalHpProjection hp, ExactMathBudget math)
        {
            var members = new Dictionary<BattleCombatantKey, ExactRational>();
            foreach (var row in before.Members) members.Add(row.CombatantKey, row.Hp);
            var enemies = new Dictionary<BattleCombatantKey, ExactRational>();
            foreach (var row in before.Enemies) enemies.Add(row.CombatantKey, row.Hp);
            return CheckHpRows(hp.MemberHp, members, "FinalHp.MemberHp", math)
                ?? CheckHpRows(hp.EnemyHp, enemies, "FinalHp.EnemyHp", math);
        }

        private static CandidateStageResult CheckHpRows(List<CandidateHpInput> rows,
            Dictionary<BattleCombatantKey, ExactRational> originals, string path, ExactMathBudget math)
        {
            if (rows.Count != originals.Count) return Reject(InconsistentBinding, path);
            var seen = new HashSet<BattleCombatantKey>();
            for (var i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                if (!originals.TryGetValue(row.CombatantKey, out var current) || !seen.Add(row.CombatantKey))
                    return Reject(InconsistentBinding, path + $"[{i}].CombatantKey");
                // Current HP was checked against original MaxHp. This supported domain cannot heal or revive.
                if (!InRange(row.Hp, current, math)) return Reject(InvalidValue, path + $"[{i}].Hp");
            }
            return null;
        }

        private static CandidateStageResult CheckRoute(BattleSnapshot before, BattlePairKey key, IReadOnlyList<FlowPos> route)
        {
            var paths = new List<FlowPathData>();
            foreach (var locked in before.Board.LockedRoutes)
                paths.Add(new FlowPathData { colorId = FindPair(before, locked.PairKey).GeometryColorId, cells = new List<FlowPos>(locked.Route) });
            var result = new BattleRouteValidator().Validate(Geometry(before.Board.Face), paths, Array.Empty<FlowPos>(),
                FindPair(before, key).GeometryColorId, route);
            return result.IsValid ? null : new CandidateStageResult(InvalidRoute, "Route", result);
        }

        private static FlowLevelData Geometry(PreparedFace face)
        {
            var level = new FlowLevelData { width = face.Width, height = face.Height };
            foreach (var pair in face.Pairs)
                level.pairs.Add(new FlowPairData { colorId = pair.GeometryColorId, endpointA = pair.EndpointA, endpointB = pair.EndpointB });
            return level;
        }

        private static PreparedPair FindPair(BattleSnapshot before, BattlePairKey key)
        { foreach (var pair in before.Board.Face.Pairs) if (PairKey(before, pair).Equals(key)) return pair; return null; }
        private static BattleEnemyState FindEnemy(BattleSnapshot before, BattlePairKey key)
        { foreach (var row in before.Enemies) if (row.PairKey.Equals(key)) return row; return null; }
        private static BattlePairKey PairKey(BattleSnapshot before, PreparedPair pair)
        { return BattlePairKey.Create(before.Baseline.Entry.AttemptId, before.Board.Face.FaceId, pair.PairId); }
        private static bool IsLocked(IReadOnlyList<BattleLockedRoute> rows, BattlePairKey key)
        { foreach (var row in rows) if (row.PairKey.Equals(key)) return true; return false; }
        private static bool Contains(IReadOnlyList<BattlePairKey> rows, BattlePairKey key)
        { foreach (var row in rows) if (row.Equals(key)) return true; return false; }
        private static ExactRational HpFor(IReadOnlyList<CandidateHpValue> rows, BattleCombatantKey key)
        { foreach (var row in rows) if (row.CombatantKey.Equals(key)) return row.Hp; throw new InvalidOperationException("Validated HP key missing."); }
        private static bool InRange(ExactRational hp, ExactRational maximum, ExactMathBudget math)
        { return hp.Numerator.Sign >= 0 && hp.Compare(maximum, math) <= 0; }
        private static CandidateStageResult Reject(CandidateStageRejectionCode code, string path)
        { return new CandidateStageResult(code, path); }
        private static void AddFact(List<CandidateStageFact> facts, CandidateStageFactKind kind, BattleSnapshot before,
            CandidateStageSource source, BattlePairKey pair, string nextFaceId = null, BattlePhase? phase = null)
        { facts.Add(new CandidateStageFact(kind, before, source, pair, facts.Count, nextFaceId, phase)); }

        private static CandidateStageResult CheckNumbers(BattleSnapshot before, BigInteger revision,
            IReadOnlyList<FlowPos> route, ExactMathBudget math)
        {
            math.CheckInteger(revision);
            if (before.Baseline == null) return Reject(MissingField, "Before.Baseline");
            if (before.Board?.Face == null) return Reject(MissingField, "Before.Board.Face");
            if (before.Random == null) return Reject(MissingField, "Before.Random");
            var entry = before.Baseline.Entry;
            RuleContextChecks.CheckBudget(entry, math); math.CheckInteger(entry.Level.RecommendedLevel);
            foreach (var member in entry.Members) CheckMember(member, math);
            foreach (var face in entry.Level.Faces) CheckFace(face, math);
            before.Baseline.RandomInitials.Battle.Validate(math);
            before.Baseline.RandomInitials.BaseReward.Validate(math);
            before.Baseline.RandomInitials.Bonus.Validate(math);
            foreach (var prd in before.Baseline.PrdInitialStates) { math.CheckInteger(prd.FailureCount); CheckCrit(prd.Crit, math); }
            math.CheckInteger(before.SceneRevision); math.CheckInteger(before.EffectiveActionsCompleted);
            math.CheckInteger(before.EnemyPhasesCompleted); math.CheckInteger(before.CurrentFaceIndex);
            CheckFace(before.Board.Face, math); CheckCells(route, math);
            for (var i = 0; i < before.Board.LockedRoutes.Count; i++)
            {
                var row = before.Board.LockedRoutes[i];
                if (row == null) return Reject(MissingField, $"Before.Board.LockedRoutes[{i}]");
                CheckCells(row.Route, math);
            }
            for (var i = 0; i < before.Members.Count; i++)
            {
                var row = before.Members[i];
                if (row?.Hp == null) return Reject(MissingField, $"Before.Members[{i}].Hp");
                CheckNumber(row.Hp, math); CheckMember(row.Member, math);
            }
            for (var i = 0; i < before.Enemies.Count; i++)
            {
                var row = before.Enemies[i];
                if (row?.Hp == null) return Reject(MissingField, $"Before.Enemies[{i}].Hp");
                CheckNumber(row.Hp, math); math.CheckInteger(row.IntentCursor); CheckEnemy(row.Enemy, math);
            }
            before.Random.Stream.Validate(math);
            foreach (var prd in before.Random.PrdStates) { math.CheckInteger(prd.FailureCount); CheckCrit(prd.Crit, math); }
            foreach (var row in before.Contributions)
            { CheckNumber(row.EffectiveDamageDealtHp, math); CheckNumber(row.EffectiveDamageTakenHp, math); }
            return null;
        }

        private static void CheckMember(PreparedMember member, ExactMathBudget math)
        {
            math.CheckInteger(member.Level); RuleContextChecks.CheckBudget(member.StatsContext, math); math.CheckInteger(member.OriginalSlot);
            CheckStats(member.Stats, math); CheckNumber(member.EntryHp, math); CheckCrit(member.Crit, math);
        }
        private static void CheckFace(PreparedFace face, ExactMathBudget math)
        {
            math.CheckInteger(face.Width); math.CheckInteger(face.Height);
            foreach (var pair in face.Pairs)
            {
                math.CheckInteger(pair.GeometryColorId);
                CheckCells(new[] { pair.EndpointA, pair.EndpointB }, math); CheckEnemy(pair.Enemy, math);
            }
        }
        private static void CheckEnemy(PreparedEnemy enemy, ExactMathBudget math)
        {
            math.CheckInteger(enemy.OriginalSlot); math.CheckInteger(enemy.StableOrder); CheckStats(enemy.Stats, math);
            foreach (var intent in enemy.IntentCycle) if (intent.DamageCoefficient != null) CheckNumber(intent.DamageCoefficient, math);
        }
        private static void CheckStats(PreparedStats stats, ExactMathBudget math)
        {
            CheckNumber(stats.MaxHp, math); CheckNumber(stats.Attack, math); CheckNumber(stats.PhysicalDefense, math);
            CheckNumber(stats.MagicDefense, math); CheckNumber(stats.Evasion, math); math.CheckInteger(stats.AttackRange);
        }
        private static void CheckCrit(PreparedWarriorCrit crit, ExactMathBudget math)
        { CheckNumber(crit.TargetProbability, math); CheckNumber(crit.C, math); CheckNumber(crit.Multiplier, math); }
        private static void CheckCells(IEnumerable<FlowPos> cells, ExactMathBudget math)
        { foreach (var cell in cells) { math.CheckInteger(cell.x); math.CheckInteger(cell.y); } }
        private static void CheckNumber(ExactRational number, ExactMathBudget math)
        { math.CheckInteger(number.Numerator); math.CheckInteger(number.Denominator); }
    }
}
