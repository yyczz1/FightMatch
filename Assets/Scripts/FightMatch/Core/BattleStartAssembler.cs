using System;
using System.Collections.Generic;
using System.Numerics;
using static FightMatch.Core.BattleEntryRejectionCode;

namespace FightMatch.Core
{
    public static class BattleStartAssembler
    {
        public static BattleStartResult CreateCandidate(PreparedBattleEntry entry,
            CandidateRandomInitials randomInitials, ExactMathBudget budget)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));
            if (randomInitials == null) throw new ArgumentNullException(nameof(randomInitials));
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            CheckPreparedNumbers(entry, budget);
            var rejection = CheckStream(randomInitials.Battle, "RandomInitials.Battle", budget)
                ?? CheckStream(randomInitials.BaseReward, "RandomInitials.BaseReward", budget)
                ?? CheckStream(randomInitials.Bonus, "RandomInitials.Bonus", budget)
                ?? CheckPrd(entry, randomInitials.PrdStates, budget);
            if (rejection != null) return rejection;

            // Finish numeric work before constructing any output. This also checks the 0/1 counters.
            var zero = ExactRational.Create(BigInteger.Zero, BigInteger.One, budget);
            var prdStates = new List<BattlePrdState>();
            foreach (var member in entry.ReadyParticipants)
                prdStates.Add(new BattlePrdState(
                    BattleCombatantKey.ForParticipant(entry.AttemptId, member.CharacterId), member.Crit, BigInteger.Zero));
            var baseline = new BattleEntryBaseline(entry, randomInitials, prdStates);
            var snapshot = new BattleSnapshot(baseline, zero);
            return new BattleStartResult(new CandidateBattleStart(baseline, snapshot));
        }

        private static void CheckPreparedNumbers(PreparedBattleEntry entry, ExactMathBudget budget)
        {
            RuleContextChecks.CheckBudget(entry, budget);
            budget.CheckInteger(entry.Level.RecommendedLevel);
            foreach (var face in entry.Level.Faces)
                foreach (var pair in face.Pairs)
                {
                    CheckStats(pair.Enemy.Stats, budget);
                    foreach (var intent in pair.Enemy.IntentCycle)
                        if (intent.DamageCoefficient != null) CheckNumber(intent.DamageCoefficient, budget);
                }
            foreach (var member in entry.Members)
            {
                budget.CheckInteger(member.Level);
                RuleContextChecks.CheckBudget(member.StatsContext, budget);
                CheckStats(member.Stats, budget);
                CheckNumber(member.EntryHp, budget);
                CheckNumber(member.Crit.TargetProbability, budget);
                CheckNumber(member.Crit.C, budget);
                CheckNumber(member.Crit.Multiplier, budget);
            }
        }

        private static void CheckStats(PreparedStats stats, ExactMathBudget budget)
        {
            CheckNumber(stats.MaxHp, budget);
            CheckNumber(stats.Attack, budget);
            CheckNumber(stats.PhysicalDefense, budget);
            CheckNumber(stats.MagicDefense, budget);
            CheckNumber(stats.Evasion, budget);
        }

        private static void CheckNumber(ExactRational value, ExactMathBudget budget)
        {
            budget.CheckInteger(value.Numerator);
            budget.CheckInteger(value.Denominator);
        }

        private static BattleStartResult CheckStream(Pcg32StreamState stream, string path, ExactMathBudget budget)
        {
            if (stream == null) return new BattleStartResult(MissingField, path);
            stream.Validate(budget);
            if (budget.Compare(stream.WordsConsumed, BigInteger.Zero) != 0 ||
                budget.Compare(stream.Initial.State, stream.Current.State) != 0 ||
                budget.Compare(stream.Initial.Increment, stream.Current.Increment) != 0)
                return new BattleStartResult(InvalidValue, path);
            return null;
        }

        private static BattleStartResult CheckPrd(PreparedBattleEntry entry,
            List<CandidatePrdInitial> rows, ExactMathBudget budget)
        {
            const string path = "RandomInitials.PrdStates";
            if (rows == null) return new BattleStartResult(MissingField, path);
            var seen = new HashSet<(string Character, string Passive)>();
            for (var i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                var p = path + $"[{i}]";
                if (row == null) return new BattleStartResult(MissingField, p);
                if (string.IsNullOrWhiteSpace(row.CharacterId)) return new BattleStartResult(MissingField, p + ".CharacterId");
                if (string.IsNullOrWhiteSpace(row.PassiveDefinitionId)) return new BattleStartResult(MissingField, p + ".PassiveDefinitionId");
                if (!row.FailureCount.HasValue) return new BattleStartResult(MissingField, p + ".FailureCount");
                if (!seen.Add((row.CharacterId, row.PassiveDefinitionId))) return new BattleStartResult(DuplicateIdentity, p);
                PreparedMember matched = null;
                foreach (var member in entry.ReadyParticipants)
                    if (string.Equals(member.CharacterId, row.CharacterId, StringComparison.Ordinal))
                    {
                        matched = member;
                        break;
                    }
                if (matched == null) return new BattleStartResult(InconsistentBinding, p + ".CharacterId");
                if (!string.Equals(matched.Crit.PassiveDefinitionId, row.PassiveDefinitionId, StringComparison.Ordinal))
                    return new BattleStartResult(InconsistentBinding, p + ".PassiveDefinitionId");
                budget.CheckInteger(row.FailureCount.Value);
                if (budget.Compare(row.FailureCount.Value, BigInteger.Zero) != 0)
                    return new BattleStartResult(InvalidValue, p + ".FailureCount");
            }
            foreach (var member in entry.ReadyParticipants)
                if (!seen.Contains((member.CharacterId, member.Crit.PassiveDefinitionId)))
                    return new BattleStartResult(MissingField, path);
            return null;
        }
    }
}
