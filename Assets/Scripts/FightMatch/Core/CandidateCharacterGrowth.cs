using System;
using System.Collections.Generic;
using System.Numerics;
using static FightMatch.Core.CandidateGrowthRejectionCode;

namespace FightMatch.Core
{
    public static class CandidateCharacterGrowth
    {
        public static CandidateGrowthDefinitionResult PrepareDefinition(GrowthDefinitionInput input, ExactMathBudget budget)
        { return PrepareDefinition(input, budget, false); }

        public static CandidateGrowthDefinitionResult PreparePermanentDefinition(GrowthDefinitionInput input, ExactMathBudget budget)
        { return PrepareDefinition(input, budget, true); }

        private static CandidateGrowthDefinitionResult PrepareDefinition(GrowthDefinitionInput input, ExactMathBudget budget, bool permanent)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            var c = new GrowthChecks(budget);
            if (!c.Context(input.Context, "Context") || !c.Text(input.ClassId, "ClassId") ||
                !c.Text(input.PassiveDefinitionId, "PassiveDefinitionId")) return new CandidateGrowthDefinitionResult(c);
            if (input.ClassKind != CharacterClassKind.Warrior && (!permanent || input.ClassKind != CharacterClassKind.Mage))
            { c.Fail(input.ClassKind == CharacterClassKind.Unspecified ? MissingField : UnsupportedBinding, "ClassKind"); return new CandidateGrowthDefinitionResult(c); }
            var s = input.BaseStats;
            if (s == null) { c.Fail(MissingField, "BaseStats"); return new CandidateGrowthDefinitionResult(c); }
            if (!c.Number(s.MaxHp, "BaseStats.MaxHp", true) || !c.Number(s.Attack, "BaseStats.Attack") ||
                !c.Number(s.PhysicalDefense, "BaseStats.PhysicalDefense") || !c.Number(s.MagicDefense, "BaseStats.MagicDefense") ||
                !c.Number(s.Evasion, "BaseStats.Evasion", false, true)) return new CandidateGrowthDefinitionResult(c);
            if (!s.Evasion.Numerator.IsZero) { c.Fail(UnsupportedBinding, "BaseStats.Evasion"); return new CandidateGrowthDefinitionResult(c); }
            if (s.AttackRange <= 0) { c.Fail(InvalidValue, "BaseStats.AttackRange"); return new CandidateGrowthDefinitionResult(c); }
            if (!c.Number(input.GrowthHp, "GrowthHp") || !c.Number(input.GrowthAttack, "GrowthAttack") ||
                !c.Number(input.GrowthDefense, "GrowthDefense") ||
                (input.ClassKind == CharacterClassKind.Warrior &&
                    (!c.Number(input.CritBase, "CritBase", true, true) || !c.Number(input.CritStep, "CritStep") ||
                     !c.Number(input.CritCap, "CritCap", true, true) || !c.Number(input.CritMultiplier, "CritMultiplier", true))) ||
                !c.Integer(input.XpBase, "XpBase", true) ||
                !c.Integer(input.XpLinear, "XpLinear") || !c.Integer(input.XpQuadratic, "XpQuadratic") ||
                !c.Number(input.RecoveryDurationMilliseconds, "RecoveryDurationMilliseconds", true)) return new CandidateGrowthDefinitionResult(c);
            if (input.ClassKind == CharacterClassKind.Warrior)
            {
                if (input.CritBase.Compare(input.CritCap, budget) > 0)
                { c.Fail(InvalidValue, "CritCap"); return new CandidateGrowthDefinitionResult(c); }
            }
            else if (input.CritBase != null || input.CritStep != null || input.CritCap != null || input.CritMultiplier != null)
            {
                c.Fail(UnsupportedBinding, "Mage.CombatDefinition");
                return new CandidateGrowthDefinitionResult(c);
            }
            return new CandidateGrowthDefinitionResult(new CandidateGrowthDefinition(input));
        }

        public static CandidateCharacterResult CreateCandidate(CandidateGrowthDefinition definition, string playerId,
            string characterId, BigInteger level, BigInteger experience, int originalSlot, ExactMathBudget budget)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            var c = new GrowthChecks(budget); c.CheckDefinition(definition);
            if (!c.Text(playerId, "PlayerId") || !c.Text(characterId, "CharacterId") ||
                !c.Integer(level, "Level", true) || !c.Integer(experience, "Experience")) return new CandidateCharacterResult(c);
            if (originalSlot < 0 || originalSlot > 2) { c.Fail(InvalidValue, "OriginalSlot"); return new CandidateCharacterResult(c); }
            if (budget.Compare(experience, Need(definition, level, budget)) >= 0)
            { c.Fail(InvalidValue, "Experience"); return new CandidateCharacterResult(c); }
            budget.CheckInteger(BigInteger.One);
            return new CandidateCharacterResult(new CandidateCharacterState(definition, playerId, characterId, level,
                experience, originalSlot, BigInteger.One, new CandidateBaseExperienceReceipt[0],
                new CandidateCharacterEndReceipt[0], new CandidateRecoveryPeriod[0]), CandidateGrowthOutcome.Applied);
        }

        public static CandidateCharacterResult ApplyBaseReward(CandidateCharacterState state, CandidateBaseExperience reward,
            BigInteger expectedRevision, ExactMathBudget budget)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (reward == null) throw new ArgumentNullException(nameof(reward));
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            var c = new GrowthChecks(budget); c.CheckState(state);
            if (!c.Target(state, reward.PlayerId, reward.CharacterId, reward.Context) || !c.Text(reward.AttemptId, "AttemptId") ||
                !c.Text(reward.SettlementId, "SettlementId") || !c.Integer(reward.Amount, "Amount")) return new CandidateCharacterResult(c);
            foreach (var prior in state.BaseRewards)
            {
                if (!string.Equals(prior.SettlementId, reward.SettlementId, StringComparison.Ordinal)) continue;
                if (!c.Same(prior.AttemptId, reward.AttemptId, "AttemptId")) return new CandidateCharacterResult(c);
                if (budget.Compare(prior.Amount, reward.Amount.Value) != 0)
                { c.Fail(InconsistentBinding, "Amount"); return new CandidateCharacterResult(c); }
                return new CandidateCharacterResult(state, CandidateGrowthOutcome.AlreadyIncluded, prior);
            }
            if (!c.Revision(state, expectedRevision)) return new CandidateCharacterResult(c);
            Accumulate(state, reward.Amount.Value, budget, out var level, out var remaining);
            var revision = budget.Add(state.StateRevision, BigInteger.One);
            var receipt = new CandidateBaseExperienceReceipt(reward, state.Definition.Context);
            var rewards = new List<CandidateBaseExperienceReceipt>(state.BaseRewards) { receipt };
            var next = new CandidateCharacterState(state.Definition, state.PlayerId, state.CharacterId, level, remaining,
                state.OriginalSlot, revision, rewards, state.ProcessedEnds, state.RecoveryPeriods);
            return new CandidateCharacterResult(next, CandidateGrowthOutcome.Applied, receipt);
        }

        public static CandidateComputedStats ComputeBaseStats(CandidateCharacterState state, ExactMathBudget budget)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            new GrowthChecks(budget).CheckState(state);
            var d = state.Definition;
            var offset = ExactRational.Create(budget.Subtract(state.Level, BigInteger.One), BigInteger.One, budget);
            var one = ExactRational.Create(BigInteger.One, BigInteger.One, budget);
            var defense = d.GrowthDefense.Multiply(offset, budget);
            var stats = new PreparedStats(new StatsInput
            {
                MaxHp = d.BaseStats.MaxHp.Multiply(one.Add(d.GrowthHp.Multiply(offset, budget), budget), budget),
                Attack = d.BaseStats.Attack.Multiply(one.Add(d.GrowthAttack.Multiply(offset, budget), budget), budget),
                PhysicalDefense = d.BaseStats.PhysicalDefense.Add(defense, budget),
                MagicDefense = d.BaseStats.MagicDefense.Add(defense, budget),
                AttackRange = d.BaseStats.AttackRange, Evasion = d.BaseStats.Evasion
            });
            if (d.ClassKind == CharacterClassKind.Mage) return new CandidateComputedStats(state, stats, null);
            var probability = d.CritBase.Add(d.CritStep.Multiply(offset, budget), budget);
            if (probability.Compare(d.CritCap, budget) > 0) probability = d.CritCap;
            return new CandidateComputedStats(state, stats, probability);
        }

        internal static void Accumulate(CandidateCharacterState state, BigInteger amount, ExactMathBudget budget,
            out BigInteger level, out BigInteger experience)
        {
            budget.CheckInteger(amount);
            if (amount.Sign < 0) throw new ArgumentOutOfRangeException(nameof(amount));
            level = state.Level;
            experience = budget.Add(state.Experience, amount);
            var need = Need(state.Definition, level, budget);
            while (budget.Compare(experience, need) >= 0)
            {
                experience = budget.Subtract(experience, need);
                level = budget.Add(level, BigInteger.One);
                need = Need(state.Definition, level, budget);
            }
        }

        internal static BigInteger Need(CandidateGrowthDefinition d, BigInteger level, ExactMathBudget budget)
        {
            var offset = budget.Subtract(level, BigInteger.One);
            // Horner form avoids an unused square when the quadratic coefficient is zero.
            return budget.Add(d.XpBase, budget.Multiply(offset, budget.Add(d.XpLinear, budget.Multiply(d.XpQuadratic, offset))));
        }
    }
}
