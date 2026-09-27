using System;
using System.Collections.Generic;
using System.Numerics;
using FlowPuzzle.Core;
using static FightMatch.Core.CandidateAttackRejectionCode;

namespace FightMatch.Core
{
    public static class CandidateDirectAttack
    {
        public static CandidateDirectAttackResult Evaluate(CandidateRandomBinding binding, BattleSnapshot before,
            CandidateAttackRequest request, RandomSamplingBudget budget)
        {
            if (binding == null) throw new ArgumentNullException(nameof(binding));
            if (before == null) throw new ArgumentNullException(nameof(before));
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            if (string.IsNullOrWhiteSpace(request.PlayerId)) return Reject(MissingField, "PlayerId");
            if (string.IsNullOrWhiteSpace(request.AttemptId)) return Reject(MissingField, "AttemptId");
            if (string.IsNullOrWhiteSpace(request.OperationId)) return Reject(MissingField, "OperationId");
            if (!request.ExpectedSceneRevision.HasValue) return Reject(MissingField, "ExpectedSceneRevision");
            if (request.Actor == null) return Reject(MissingField, "Actor");
            if (request.Pair == null) return Reject(MissingField, "Pair");
            if (request.Route == null) return Reject(MissingField, "Route");
            var math = budget.Math;
            CheckNumbers(binding, before, request.ExpectedSceneRevision.Value, math);
            if (!ReferenceEquals(binding.Start.Baseline, before.Baseline)) return Reject(InconsistentBinding, "Before.Baseline");
            var entry = before.Baseline.Entry;
            if (!string.Equals(request.PlayerId, entry.PlayerId, StringComparison.Ordinal)) return Reject(InconsistentBinding, "PlayerId");
            if (!string.Equals(request.AttemptId, entry.AttemptId, StringComparison.Ordinal)) return Reject(InconsistentBinding, "AttemptId");
            if (request.ExpectedSceneRevision.Value.Sign < 0) return Reject(InvalidValue, "ExpectedSceneRevision");
            if (math.Compare(request.ExpectedSceneRevision.Value, before.SceneRevision) != 0) return Reject(StaleContext, "ExpectedSceneRevision");
            var rejection = CheckState(before, request, math, out var actor, out var target);
            if (rejection != null) return rejection;

            var face = before.Board.Face;
            var level = new FlowLevelData { width = face.Width, height = face.Height };
            var pairDefinitions = new Dictionary<BattlePairKey, PreparedPair>();
            foreach (var pair in face.Pairs)
            {
                pairDefinitions.Add(BattlePairKey.Create(entry.AttemptId, face.FaceId, pair.PairId), pair);
                level.pairs.Add(new FlowPairData { colorId = pair.GeometryColorId, endpointA = pair.EndpointA, endpointB = pair.EndpointB });
            }
            var lockedPaths = new List<FlowPathData>();
            var lockedKeys = new HashSet<BattlePairKey>();
            foreach (var route in before.Board.LockedRoutes)
            {
                if (!pairDefinitions.TryGetValue(route.PairKey, out var pair) || !lockedKeys.Add(route.PairKey))
                    return Reject(InconsistentBinding, "Before.Board.LockedRoutes");
                lockedPaths.Add(new FlowPathData { colorId = pair.GeometryColorId, cells = new List<FlowPos>(route.Route) });
            }
            var routeResult = new BattleRouteValidator().Validate(level, lockedPaths, Array.Empty<FlowPos>(),
                pairDefinitions[request.Pair].GeometryColorId, request.Route);
            if (!routeResult.IsValid) return new CandidateDirectAttackResult(InvalidRoute, "Route", routeResult);
            var livingOrdinal = 1;
            foreach (var enemy in before.Enemies)
                if (enemy.Hp.Numerator.Sign > 0 && enemy.OriginalSlot < target.OriginalSlot) livingOrdinal++;
            if (livingOrdinal > actor.Member.Stats.AttackRange) return Reject(OutOfRange, "Pair");

            // Evasion is explicitly zero in this supported domain, so even floor-zero damage is a real hit.
            var crit = CandidateWarriorCritEvaluator.EvaluateOpportunity(binding, before.Random, actor.CombatantKey,
                target.CombatantKey, before.EffectiveActionsCompleted, budget);
            if (!crit.IsAccepted)
            {
                var code = crit.RejectionCode == BattleEntryRejectionCode.InvalidValue ? InvalidValue : InconsistentBinding;
                var path = crit.FieldPath.StartsWith("Before.", StringComparison.Ordinal)
                    ? "Before.Random." + crit.FieldPath.Substring(7) : crit.FieldPath;
                return Reject(code, path);
            }
            var one = ExactRational.Create(1, 1, math);
            var hundred = ExactRational.Create(100, 1, math);
            var zero = ExactRational.Create(0, 1, math);
            var multiplier = crit.Fact.Triggered ? actor.Member.Crit.Multiplier : one;
            var raw = actor.Member.Stats.Attack.Multiply(multiplier, math);
            var mitigated = raw.Multiply(hundred, math).Divide(hundred.Add(target.Enemy.Stats.PhysicalDefense, math), math);
            var rounded = mitigated.Floor(math);
            var incoming = ExactRational.Create(rounded, 1, math);
            var hpLoss = target.Hp.Compare(incoming, math) < 0 ? target.Hp : incoming;
            var overflow = incoming.Subtract(hpLoss, math);
            var hpAfter = target.Hp.Subtract(hpLoss, math);
            var action = new CandidateAttackSource(request, math.Add(before.EffectiveActionsCompleted, BigInteger.One));
            var fact = new BattleDamageFact(before, action, target, actor.Member.Stats.Attack, multiplier,
                raw, mitigated, rounded, hpAfter, hpLoss, overflow, zero, crit.Fact);
            var enemies = new List<BattleEnemyState>();
            foreach (var enemy in before.Enemies)
                enemies.Add(enemy.CombatantKey.Equals(target.CombatantKey)
                    ? new BattleEnemyState(enemy.CombatantKey, enemy.PairKey, enemy.Enemy, hpAfter, enemy.IntentCursor) : enemy);
            var contributions = new List<BattleContributionTotals>();
            foreach (var total in before.Contributions)
                contributions.Add(total.CombatantKey.Equals(actor.CombatantKey)
                    ? new BattleContributionTotals(total.CombatantKey, total.EffectiveDamageDealtHp.Add(hpLoss, math),
                        total.EffectiveDamageTakenHp) : total);
            return new CandidateDirectAttackResult(new CandidateCombatFrame(binding, before, action, enemies,
                crit.Next, contributions, fact), fact);
        }

        private static CandidateDirectAttackResult CheckState(BattleSnapshot before, CandidateAttackRequest request,
            ExactMathBudget math, out BattleMemberState actor, out BattleEnemyState target)
        {
            actor = null; target = null;
            if (before.Phase != BattlePhase.AwaitAction) return Reject(InvalidPhase, "Before.Phase");
            if (before.SceneRevision.Sign <= 0) return Reject(InvalidValue, "Before.SceneRevision");
            if (before.EffectiveActionsCompleted.Sign < 0) return Reject(InvalidValue, "Before.EffectiveActionsCompleted");
            if (before.EnemyPhasesCompleted.Sign < 0) return Reject(InvalidValue, "Before.EnemyPhasesCompleted");
            var entry = before.Baseline.Entry;
            if (before.CarryMode != EntryCarryMode.Empty || entry.CarryMode != EntryCarryMode.Empty || entry.RequiredFeatures.Count != 0)
                return Reject(InconsistentBinding, "Before.CarryMode");
            if (before.CurrentFaceIndex < 0 || before.CurrentFaceIndex >= entry.Level.Faces.Count)
                return Reject(InvalidValue, "Before.CurrentFaceIndex");
            var face = entry.Level.Faces[before.CurrentFaceIndex];
            if (!ReferenceEquals(before.Board.Face, face)) return Reject(InconsistentBinding, "Before.Board.Face");
            if (before.Board.PendingLinks.Count != 0) return Reject(InvalidPhase, "Before.Board.PendingLinks");
            if (before.Members.Count != entry.ReadyParticipants.Count) return Reject(InconsistentBinding, "Before.Members");
            var memberKeys = new HashSet<BattleCombatantKey>();
            for (var i = 0; i < before.Members.Count; i++)
            {
                var row = before.Members[i]; var path = $"Before.Members[{i}]";
                PreparedMember original = null;
                foreach (var member in entry.ReadyParticipants)
                    if (row.CombatantKey.Equals(BattleCombatantKey.ForParticipant(entry.AttemptId, member.CharacterId))) original = member;
                if (original == null || !memberKeys.Add(row.CombatantKey) || !ReferenceEquals(original, row.Member))
                    return Reject(InconsistentBinding, path);
                if (original.ClassKind != CharacterClassKind.Warrior || original.LearnedSkills.Count != 0 ||
                    !original.Stats.Evasion.Numerator.IsZero) return Reject(InconsistentBinding, path + ".Member");
                if (!HpInRange(row.Hp, original.Stats.MaxHp, math)) return Reject(InvalidValue, path + ".Hp");
                if (row.CombatantKey.Equals(request.Actor)) actor = row;
            }
            if (actor == null) return Reject(InconsistentBinding, "Actor");
            if (actor.Hp.Numerator.IsZero) return Reject(ActorUnavailable, "Actor");
            if (before.Enemies.Count != face.Pairs.Count) return Reject(InconsistentBinding, "Before.Enemies");
            var enemyKeys = new HashSet<BattleCombatantKey>();
            for (var i = 0; i < before.Enemies.Count; i++)
            {
                var row = before.Enemies[i]; var path = $"Before.Enemies[{i}]";
                PreparedPair original = null;
                foreach (var pair in face.Pairs)
                    if (row.PairKey.Equals(BattlePairKey.Create(entry.AttemptId, face.FaceId, pair.PairId))) original = pair;
                if (original == null || !ReferenceEquals(original.Enemy, row.Enemy) || !enemyKeys.Add(row.CombatantKey) ||
                    !row.CombatantKey.Equals(BattleCombatantKey.ForEnemy(entry.AttemptId, face.FaceId, original.Enemy.EnemyInstanceKey)))
                    return Reject(InconsistentBinding, path);
                if (!row.Enemy.Stats.Evasion.Numerator.IsZero) return Reject(InconsistentBinding, path + ".Enemy");
                if (!HpInRange(row.Hp, row.Enemy.Stats.MaxHp, math)) return Reject(InvalidValue, path + ".Hp");
                if (row.IntentCursor.Sign < 0) return Reject(InvalidValue, path + ".IntentCursor");
                if (row.PairKey.Equals(request.Pair)) target = row;
            }
            if (!string.Equals(request.Pair.AttemptId, entry.AttemptId, StringComparison.Ordinal)) return Reject(InconsistentBinding, "Pair");
            if (target == null || target.Hp.Numerator.IsZero) return Reject(TargetUnavailable, "Pair");
            if (before.Contributions.Count != memberKeys.Count) return Reject(InconsistentBinding, "Before.Contributions");
            var totals = new HashSet<BattleCombatantKey>();
            for (var i = 0; i < before.Contributions.Count; i++)
            {
                var row = before.Contributions[i]; var path = $"Before.Contributions[{i}]";
                if (!memberKeys.Contains(row.CombatantKey) || !totals.Add(row.CombatantKey)) return Reject(InconsistentBinding, path);
                if (row.EffectiveDamageDealtHp.Numerator.Sign < 0 || row.EffectiveDamageTakenHp.Numerator.Sign < 0)
                    return Reject(InvalidValue, path);
            }
            return null;
        }

        private static bool HpInRange(ExactRational hp, ExactRational max, ExactMathBudget math)
        { return hp.Numerator.Sign >= 0 && hp.Compare(max, math) <= 0; }

        private static CandidateDirectAttackResult Reject(CandidateAttackRejectionCode code, string path)
        { return new CandidateDirectAttackResult(code, path); }

        private static void CheckNumbers(CandidateRandomBinding binding, BattleSnapshot before, BigInteger expected,
            ExactMathBudget math)
        {
            math.CheckInteger(expected);
            foreach (var domain in new[] { binding.Battle, binding.BaseReward, binding.Bonus })
            {
                math.CheckInteger(domain.InitState); math.CheckInteger(domain.InitSequence);
                domain.Initial.Validate(math);
            }
            CheckBaseline(binding.Start.Baseline, math);
            CheckSnapshot(binding.Start.Snapshot, math);
            if (!ReferenceEquals(before.Baseline, binding.Start.Baseline)) CheckBaseline(before.Baseline, math);
            if (!ReferenceEquals(before, binding.Start.Snapshot)) CheckSnapshot(before, math);
        }

        private static void CheckBaseline(BattleEntryBaseline baseline, ExactMathBudget math)
        {
            var entry = baseline.Entry;
            RuleContextChecks.CheckBudget(entry, math);
            math.CheckInteger(entry.Level.RecommendedLevel);
            foreach (var member in entry.Members) CheckMember(member, math);
            foreach (var face in entry.Level.Faces) CheckFace(face, math);
            baseline.RandomInitials.Battle.Validate(math);
            baseline.RandomInitials.BaseReward.Validate(math);
            baseline.RandomInitials.Bonus.Validate(math);
            CheckPrdNumbers(baseline.PrdInitialStates, math);
        }

        private static void CheckSnapshot(BattleSnapshot snapshot, ExactMathBudget math)
        {
            math.CheckInteger(snapshot.SceneRevision);
            math.CheckInteger(snapshot.EffectiveActionsCompleted);
            math.CheckInteger(snapshot.EnemyPhasesCompleted);
            CheckFace(snapshot.Board.Face, math);
            foreach (var row in snapshot.Members) { CheckNumber(row.Hp, math); CheckMember(row.Member, math); }
            foreach (var row in snapshot.Enemies)
            { CheckNumber(row.Hp, math); math.CheckInteger(row.IntentCursor); CheckEnemy(row.Enemy, math); }
            snapshot.Random.Stream.Validate(math);
            CheckPrdNumbers(snapshot.Random.PrdStates, math);
            foreach (var total in snapshot.Contributions)
            { CheckNumber(total.EffectiveDamageDealtHp, math); CheckNumber(total.EffectiveDamageTakenHp, math); }
        }

        private static void CheckMember(PreparedMember member, ExactMathBudget math)
        {
            math.CheckInteger(member.Level); RuleContextChecks.CheckBudget(member.StatsContext, math);
            CheckStats(member.Stats, math); CheckNumber(member.EntryHp, math); CheckCrit(member.Crit, math);
        }

        private static void CheckFace(PreparedFace face, ExactMathBudget math)
        { foreach (var pair in face.Pairs) CheckEnemy(pair.Enemy, math); }

        private static void CheckEnemy(PreparedEnemy enemy, ExactMathBudget math)
        {
            CheckStats(enemy.Stats, math);
            foreach (var intent in enemy.IntentCycle) if (intent.DamageCoefficient != null) CheckNumber(intent.DamageCoefficient, math);
        }

        private static void CheckStats(PreparedStats stats, ExactMathBudget math)
        {
            CheckNumber(stats.MaxHp, math); CheckNumber(stats.Attack, math);
            CheckNumber(stats.PhysicalDefense, math); CheckNumber(stats.MagicDefense, math); CheckNumber(stats.Evasion, math);
        }

        private static void CheckPrdNumbers(IReadOnlyList<BattlePrdState> states, ExactMathBudget math)
        { foreach (var row in states) { CheckCrit(row.Crit, math); math.CheckInteger(row.FailureCount); } }

        private static void CheckCrit(PreparedWarriorCrit crit, ExactMathBudget math)
        { CheckNumber(crit.TargetProbability, math); CheckNumber(crit.C, math); CheckNumber(crit.Multiplier, math); }

        private static void CheckNumber(ExactRational number, ExactMathBudget math)
        { math.CheckInteger(number.Numerator); math.CheckInteger(number.Denominator); }
    }
}
