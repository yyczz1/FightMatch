using System;
using System.Collections.Generic;
using System.Numerics;
using static FightMatch.Core.BattleEntryRejectionCode;

namespace FightMatch.Core
{
    public static class CandidateWarriorCritEvaluator
    {
        // The battle rules caller must already have established a real warrior direct-hit opportunity.
        public static CandidateCritResult EvaluateOpportunity(CandidateRandomBinding binding, BattleRandomSnapshot before,
            BattleCombatantKey actor, BattleCombatantKey target, BigInteger opportunityOrdinal, RandomSamplingBudget budget)
        {
            if (binding == null) throw new ArgumentNullException(nameof(binding));
            if (before == null) throw new ArgumentNullException(nameof(before));
            if (actor == null) throw new ArgumentNullException(nameof(actor));
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            var math = budget.Math;
            CheckBindingNumbers(binding, math);
            var entry = binding.Start.Baseline.Entry;
            PreparedMember actingMember = null;
            foreach (var member in entry.ReadyParticipants)
                if (actor.Equals(BattleCombatantKey.ForParticipant(entry.AttemptId, member.CharacterId))) actingMember = member;
            if (actingMember == null) return new CandidateCritResult(InconsistentBinding, "Actor");
            var foundTarget = false;
            foreach (var face in entry.Level.Faces)
                foreach (var pair in face.Pairs)
                    if (target.Equals(BattleCombatantKey.ForEnemy(entry.AttemptId, face.FaceId, pair.Enemy.EnemyInstanceKey)))
                        foundTarget = true;
            if (!foundTarget) return new CandidateCritResult(InconsistentBinding, "Target");
            math.CheckInteger(opportunityOrdinal);
            if (math.Compare(opportunityOrdinal, BigInteger.Zero) < 0) return new CandidateCritResult(InvalidValue, "OpportunityOrdinal");
            before.Stream.Validate(math);
            if (math.Compare(before.Stream.Initial.State, binding.Battle.Initial.Initial.State) != 0 ||
                math.Compare(before.Stream.Initial.Increment, binding.Battle.Initial.Initial.Increment) != 0)
                return new CandidateCritResult(InconsistentBinding, "Before.Stream.Initial");
            var rejection = CheckPrd(entry, before.PrdStates, math);
            if (rejection != null) return rejection;

            BattlePrdState actorPrd = null;
            foreach (var row in before.PrdStates) if (row.CombatantKey.Equals(actor)) actorPrd = row;
            var sample = ExactRandomSampler.PrdOpportunity(actingMember.Crit.C, actorPrd.FailureCount, before.Stream, budget);
            var nextPrd = new List<BattlePrdState>();
            foreach (var row in before.PrdStates)
                nextPrd.Add(row.CombatantKey.Equals(actor)
                    ? new BattlePrdState(actor, actingMember.Crit, sample.Value.Failures) : row);
            var next = new BattleRandomSnapshot(sample.NextState, nextPrd);
            var fact = new CandidateCritFact(actor, target, opportunityOrdinal, actingMember.Crit, actorPrd, before.Stream, sample);
            return new CandidateCritResult(next, fact);
        }

        private static CandidateCritResult CheckPrd(PreparedBattleEntry entry, IReadOnlyList<BattlePrdState> rows, ExactMathBudget math)
        {
            if (rows.Count != entry.ReadyParticipants.Count) return new CandidateCritResult(InconsistentBinding, "Before.PrdStates");
            var seen = new HashSet<BattleCombatantKey>();
            for (var i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                var path = $"Before.PrdStates[{i}]";
                PreparedMember matched = null;
                foreach (var member in entry.ReadyParticipants)
                    if (row.CombatantKey.Equals(BattleCombatantKey.ForParticipant(entry.AttemptId, member.CharacterId))) matched = member;
                if (matched == null || !seen.Add(row.CombatantKey)) return new CandidateCritResult(InconsistentBinding, path + ".CombatantKey");
                CheckCrit(row.Crit, math);
                math.CheckInteger(row.FailureCount);
                if (!string.Equals(row.Crit.PassiveDefinitionId, matched.Crit.PassiveDefinitionId, StringComparison.Ordinal))
                    return new CandidateCritResult(InconsistentBinding, path + ".Crit.PassiveDefinitionId");
                if (!Same(row.Crit.TargetProbability, matched.Crit.TargetProbability, math))
                    return new CandidateCritResult(InconsistentBinding, path + ".Crit.TargetProbability");
                if (!Same(row.Crit.C, matched.Crit.C, math)) return new CandidateCritResult(InconsistentBinding, path + ".Crit.C");
                if (!Same(row.Crit.Multiplier, matched.Crit.Multiplier, math)) return new CandidateCritResult(InconsistentBinding, path + ".Crit.Multiplier");
                var limit = math.DivRem(matched.Crit.C.Denominator, matched.Crit.C.Numerator, out var remainder);
                if (!remainder.IsZero) limit = math.Add(limit, BigInteger.One);
                if (math.Compare(row.FailureCount, BigInteger.Zero) < 0 || math.Compare(row.FailureCount, limit) >= 0)
                    return new CandidateCritResult(InvalidValue, path + ".FailureCount");
            }
            return null;
        }

        private static bool Same(ExactRational a, ExactRational b, ExactMathBudget math)
        {
            // ExactRational guarantees canonical components, so this avoids unnecessary cross-products.
            return math.Compare(a.Numerator, b.Numerator) == 0 && math.Compare(a.Denominator, b.Denominator) == 0;
        }

        private static void CheckBindingNumbers(CandidateRandomBinding binding, ExactMathBudget math)
        {
            foreach (var domain in new[] { binding.Battle, binding.BaseReward, binding.Bonus })
            {
                math.CheckInteger(domain.InitState);
                math.CheckInteger(domain.InitSequence);
                domain.Initial.Validate(math);
            }
            var start = binding.Start;
            var entry = start.Baseline.Entry;
            RuleContextChecks.CheckBudget(entry, math);
            math.CheckInteger(entry.Level.RecommendedLevel);
            foreach (var member in entry.Members)
            {
                math.CheckInteger(member.Level);
                RuleContextChecks.CheckBudget(member.StatsContext, math);
                CheckStats(member.Stats, math);
                CheckNumber(member.EntryHp, math);
                CheckCrit(member.Crit, math);
            }
            foreach (var face in entry.Level.Faces)
                foreach (var pair in face.Pairs)
                {
                    CheckStats(pair.Enemy.Stats, math);
                    foreach (var intent in pair.Enemy.IntentCycle)
                        if (intent.DamageCoefficient != null) CheckNumber(intent.DamageCoefficient, math);
                }
            math.CheckInteger(start.Snapshot.SceneRevision);
            math.CheckInteger(start.Snapshot.EffectiveActionsCompleted);
            math.CheckInteger(start.Snapshot.EnemyPhasesCompleted);
            foreach (var row in start.Baseline.PrdInitialStates) math.CheckInteger(row.FailureCount);
            foreach (var enemy in start.Snapshot.Enemies) math.CheckInteger(enemy.IntentCursor);
            foreach (var total in start.Snapshot.Contributions)
            {
                CheckNumber(total.EffectiveDamageDealtHp, math);
                CheckNumber(total.EffectiveDamageTakenHp, math);
            }
        }

        private static void CheckStats(PreparedStats stats, ExactMathBudget math)
        {
            CheckNumber(stats.MaxHp, math); CheckNumber(stats.Attack, math);
            CheckNumber(stats.PhysicalDefense, math); CheckNumber(stats.MagicDefense, math); CheckNumber(stats.Evasion, math);
        }

        private static void CheckCrit(PreparedWarriorCrit crit, ExactMathBudget math)
        {
            CheckNumber(crit.TargetProbability, math); CheckNumber(crit.C, math); CheckNumber(crit.Multiplier, math);
        }

        private static void CheckNumber(ExactRational value, ExactMathBudget math)
        {
            math.CheckInteger(value.Numerator); math.CheckInteger(value.Denominator);
        }
    }
}
