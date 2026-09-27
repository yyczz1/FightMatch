using System;
using System.Collections.Generic;
using System.Numerics;
using static FightMatch.Core.BusinessFields;

namespace FightMatch.Core
{
    public sealed class CandidatePermanentCardRequirement
    {
        public BigInteger Cards { get; }
        public BigInteger FixedExperience { get; }
        public BigInteger FinalLevel { get; }
        public BigInteger FinalExperience { get; }
        internal CandidatePermanentCardRequirement(BigInteger cards, BigInteger xp, BigInteger level, BigInteger remainder)
        {
            Cards = cards;
            FixedExperience = xp;
            FinalLevel = level;
            FinalExperience = remainder;
        }
    }

    public static class CandidatePermanentGrowth
    {
        public static SaveCodecResult<CandidatePermanentCardRequirement> CalculateCards(CandidateCharacterState state,
            CandidatePermanentDefinitions definitions, string cardId, BigInteger targetLevel, SaveCodecBudget budget)
        {
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            return SaveCodecResult<CandidatePermanentCardRequirement>.Run(() =>
            {
                Need(state != null && definitions != null, "Permanent.Growth", "MissingField");
                new GrowthChecks(budget.Math).CheckState(state);
                Need(state.Definition.Context is PreparedPublishedRuleContext context &&
                    definitions.Binding.Same(context.Binding), "Permanent.Binding", "InconsistentBinding");
                var card = definitions.Find(CandidatePermanentDefinitionKind.Card, cardId);
                Need(card != null, "Permanent.Card", "NoPublishedDefinition");
                return CalculateCards(state, card.UnitExperience, targetLevel, budget);
            });
        }

        internal static CandidatePermanentCardRequirement CalculateCards(CandidateCharacterState state,
            BigInteger unitExperience, BigInteger targetLevel, SaveCodecBudget budget)
        {
            budget.Math.CheckInteger(targetLevel);
            Need(targetLevel > state.Level, "Permanent.TargetLevel");
            var need = budget.Math.Subtract(CandidateCharacterGrowth.Need(state.Definition, state.Level, budget.Math), state.Experience);
            for (var level = budget.Math.Add(state.Level, BigInteger.One); level < targetLevel;
                level = budget.Math.Add(level, BigInteger.One))
                need = budget.Math.Add(need, CandidateCharacterGrowth.Need(state.Definition, level, budget.Math));
            var cards = budget.Math.DivRem(need, unitExperience, out var remainder);
            if (!remainder.IsZero) cards = budget.Math.Add(cards, BigInteger.One);
            var xp = budget.Math.Multiply(cards, unitExperience);
            CandidateCharacterGrowth.Accumulate(state, xp, budget.Math, out var finalLevel, out var finalExperience);
            return new CandidatePermanentCardRequirement(cards, xp, finalLevel, finalExperience);
        }

        public static CandidatePermanentEffect FindLearning(CandidateRosterState roster, string character, string skill)
        {
            if (roster == null) return null;
            foreach (var effect in roster.GetPermanentEffects())
                if (effect.Quote.Kind == CandidatePermanentKind.LearnSkill && effect.Quote.CharacterId == character &&
                    effect.Quote.DefinitionId == skill) return effect;
            return null;
        }

        internal static CandidateRosterState Apply(CandidateRosterState roster, CandidatePermanentEffect effect,
            CandidatePermanentDefinitions definitions, SaveCodecBudget budget)
        {
            var quote = effect.Quote;
            if (quote.Kind != CandidatePermanentKind.UseExperienceCards && quote.Kind != CandidatePermanentKind.LearnSkill)
                return roster;
            if (effect.Outcome != "Applied") return roster;
            var state = roster.Find(quote.CharacterId);
            Need(state != null && state.PlayerId == quote.PlayerId && state.ClassId == quote.ClassId,
                "Permanent.Character", "InconsistentBinding");
            Need(state.StateRevision == quote.CharacterRevision && state.Level == quote.BeforeLevel &&
                state.Experience == quote.BeforeExperience, "Permanent.CharacterRevision", "StaleContext");
            if (quote.Kind == CandidatePermanentKind.UseExperienceCards)
            {
                var required = Take(CalculateCards(state, definitions, quote.DefinitionId, quote.Quantity.Value, budget), "Permanent.Cards");
                Need(required.FixedExperience == quote.FixedExperience && required.FinalLevel == quote.FinalLevel &&
                    required.FinalExperience == quote.FinalExperience && quote.Costs.Count == 1 &&
                    quote.Costs[0].ItemId == quote.DefinitionId && quote.Costs[0].Quantity == required.Cards,
                    "Permanent.CardEffect", "InconsistentBinding");
            }
            else
            {
                var skill = definitions.Find(CandidatePermanentDefinitionKind.Skill, quote.DefinitionId);
                Need(skill != null && skill.ClassId == state.ClassId && state.Level >= 5, "Permanent.Skill", "LearningUnavailable");
                Need(FindLearning(roster, state.CharacterId, skill.Id) == null, "Permanent.Skill", "AlreadyLearned");
                Need(quote.FixedExperience.IsZero && quote.FinalLevel == state.Level &&
                    quote.FinalExperience == state.Experience, "Permanent.LearningEffect", "InconsistentBinding");
            }
            return ApplyFrozen(roster, effect, budget);
        }

        internal static CandidateRosterState ApplyFrozen(CandidateRosterState roster, CandidatePermanentEffect effect, SaveCodecBudget budget)
        {
            var quote = effect.Quote;
            var state = roster.Find(quote.CharacterId);
            var next = new CandidateCharacterState(state.Definition, state.PlayerId, state.CharacterId,
                quote.FinalLevel, quote.FinalExperience, state.OriginalSlot, budget.Math.Add(state.StateRevision, BigInteger.One),
                state.BaseRewards, state.ProcessedEnds, state.RecoveryPeriods);
            var replaced = roster.Replace(next);
            var effects = new List<CandidatePermanentEffect>(roster.GetPermanentEffects()) { effect };
            effects.Sort((a, b) => a.Quote.CharacterId == b.Quote.CharacterId ? StringComparer.Ordinal.Compare(a.OperationId, b.OperationId) : StringComparer.Ordinal.Compare(a.Quote.CharacterId, b.Quote.CharacterId));
            return new CandidateRosterState(roster.PlayerId, replaced.Characters, roster.Formation,
                roster.FormationRevision, roster.FormationReceipts, effects);
        }

        public static SaveCodecResult<bool> CheckBattleCapability(CandidateRosterState roster, SaveCodecBudget budget)
        {
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            return SaveCodecResult<bool>.Run(() =>
            {
                EnsureBattleCapability(roster, budget);
                return true;
            });
        }

        private static void EnsureBattleCapability(CandidateRosterState roster, SaveCodecBudget budget)
        {
            roster.Check(budget);
            foreach (var id in roster.Formation)
            {
                var character = id == null ? null : roster.Find(id);
                if (character == null || !character.IsReady) continue;
                Need(character.Definition.ClassKind == CharacterClassKind.Warrior,
                    "Permanent.Battle.Class", "UnsupportedBattleCapability");
                foreach (var effect in roster.GetPermanentEffects())
                    Need(effect.Quote.Kind != CandidatePermanentKind.LearnSkill || effect.Quote.CharacterId != id,
                        "Permanent.Battle.Skill", "UnsupportedBattleCapability");
            }
        }
    }
}
