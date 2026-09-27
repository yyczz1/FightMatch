using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static FightMatch.Core.BusinessFields;

namespace FightMatch.Core
{
    public static class CandidatePermanentProgression
    {
        internal static CandidatePermanentEffect Find(CandidateProgressionState state,
            CandidatePermanentKind kind, string definition, string character = null)
        {
            foreach (var effect in state.GetPermanentEffects())
                if (effect.Quote.Kind == kind && effect.Quote.DefinitionId == definition &&
                    (character == null || effect.Quote.CharacterId == character)) return effect;
            return null;
        }

        internal static CandidatePermanentDefinition TeachingForSkill(CandidatePermanentDefinitions definitions, string skill)
        {
            foreach (var definition in definitions.Records)
                if (definition.Kind == CandidatePermanentDefinitionKind.Teaching && definition.SkillId == skill) return definition;
            return null;
        }

        internal static bool AllowsCertificate(CandidateBusinessSnapshot business, CandidatePermanentDefinitions definitions,
            CandidatePermanentDraft draft, CandidatePermanentPortion portion)
        {
            if (portion.OutputOperationId == null) return true;
            CandidatePermanentEffect original = null;
            foreach (var effect in business.Inventory.GetPermanentLedger().Effects)
                if (effect.OperationId == portion.OutputOperationId) original = effect;
            if (original == null) return false;
            if (original.Quote.Kind != CandidatePermanentKind.BeginTeachingGift) return true;
            var teaching = definitions.Find(CandidatePermanentDefinitionKind.Teaching, original.Quote.DefinitionId);
            if (teaching == null || !teaching.Level.Same(original.Quote.TeachingLevel)) return false;
            if (Find(business.Progression, CandidatePermanentKind.ConfirmTeachingExplanation, teaching.Id) != null) return true;
            var character = business.Roster.Find(draft.CharacterId);
            return draft.Kind == CandidatePermanentKind.LearnSkill && draft.DefinitionId == teaching.SkillId &&
                character != null && character.ClassId == teaching.ClassId;
        }

        internal static List<CandidatePermanentPortion> PreferTeachingCertificate(List<CandidatePermanentPortion> available,
            CandidateBusinessSnapshot business, CandidatePermanentDraft draft)
        {
            if (draft.Kind != CandidatePermanentKind.LearnSkill) return available;
            var gift = available.FindAll(x => {
                var original = CandidatePermanentProtocol.Find(business.Inventory.GetPermanentLedger().Effects, x.OutputOperationId);
                return original?.Quote.Kind == CandidatePermanentKind.BeginTeachingGift && (draft.SelectedInputs == null ||
                    Find(business.Progression, CandidatePermanentKind.ConfirmTeachingExplanation, original.Quote.DefinitionId) == null);
            });
            return gift.Count == 0 ? available : gift;
        }

        internal static string MissingSourceCode(List<CandidatePermanentPortion> held, string item,
            Func<CandidatePermanentPortion, bool> allowed)
        {
            foreach (var portion in held)
                if (portion.ItemId == item && !allowed(portion)) return "FirstCertificateReserved";
            return "UnsupportedSourceProof";
        }

        internal static void CheckHistory(CandidatePermanentQuote quote,
            IEnumerable<(string Operation, CandidatePermanentQuote Quote)> records, CandidatePermanentDefinitions definitions)
        {
            var prior = records.Where(x => x.Quote != null).ToList();
            if (quote.Kind == CandidatePermanentKind.LearnSkill)
            {
                var teaching = TeachingForSkill(definitions, quote.DefinitionId);
                var given = teaching != null && prior.Any(x => x.Quote.Kind == CandidatePermanentKind.BeginTeachingGift &&
                    x.Quote.DefinitionId == teaching.Id && x.Quote.Outputs.Count == 1);
                Need(given == (quote.TeachingLevel != null), "Permanent.History.TeachingBinding", "TeachingRequired");
            }
            foreach (var input in quote.Inputs)
            {
                if (input.OutputOperationId == null) continue;
                var source = prior.Find(x => x.Operation == input.OutputOperationId).Quote;
                Need(source != null, "Permanent.History.Input", "UnsupportedSourceProof");
                if (source.Kind != CandidatePermanentKind.BeginTeachingGift) continue;
                var teaching = definitions.Find(CandidatePermanentDefinitionKind.Teaching, source.DefinitionId);
                Need(teaching != null && teaching.Level.Same(source.TeachingLevel),
                    "Permanent.History.GiftDefinition", "UnsupportedSourceProof");
                var explained = prior.Any(x => x.Quote.Kind == CandidatePermanentKind.ConfirmTeachingExplanation &&
                    x.Quote.DefinitionId == teaching.Id && x.Quote.TeachingLevel.Same(teaching.Level));
                Need(explained || quote.Kind == CandidatePermanentKind.LearnSkill &&
                    quote.DefinitionId == teaching.SkillId && quote.ClassId == teaching.ClassId &&
                    quote.TeachingLevel != null && quote.TeachingLevel.Same(teaching.Level),
                    "Permanent.History.CertificatePurpose", "FirstCertificateReserved");
            }
        }

        internal static void CheckEntry(CandidateBusinessSnapshot business, CandidatePermanentDefinition teaching, SaveCodecBudget budget)
        {
            Need(teaching != null && teaching.Kind == CandidatePermanentDefinitionKind.Teaching,
                "Permanent.Teaching", "NoPublishedDefinition");
            Need(business.Progression.ActiveAttempt == null && business.ActiveHistory == null,
                "Permanent.TeachingEntry", "AttemptActive");
            var open = false;
            foreach (var fact in business.Progression.OpenFacts)
                if (fact.Level.LevelId == teaching.Level.LevelId &&
                    fact.Level.LevelVersion == teaching.Level.CanonicalLevelVersion &&
                    fact.Context is PreparedPublishedRuleContext context && teaching.Level.Content.Same(context.Binding)) open = true;
            Need(open, "Permanent.TeachingEntry", "LevelNotOpen");
            RuleContextChecks.BindingBudget(teaching.Level.Content, budget);
        }

        internal static CandidateProgressionState Apply(CandidateBusinessSnapshot business, CandidatePermanentEffect effect,
            CandidatePermanentDefinitions definitions, SaveCodecBudget budget)
        {
            var state = business.Progression;
            var quote = effect.Quote;
            Need(quote.ProgressionRevision == state.StateRevision, "Permanent.ProgressionRevision", "StaleContext");
            if (quote.Kind != CandidatePermanentKind.LearnSkill && quote.Kind != CandidatePermanentKind.BeginTeachingGift &&
                quote.Kind != CandidatePermanentKind.ConfirmTeachingExplanation) return state;
            var teaching = quote.Kind == CandidatePermanentKind.LearnSkill
                ? TeachingForSkill(definitions, quote.DefinitionId)
                : definitions.Find(CandidatePermanentDefinitionKind.Teaching, quote.DefinitionId);
            var gift = teaching == null ? null : Find(state, CandidatePermanentKind.BeginTeachingGift, teaching.Id);
            if (quote.Kind == CandidatePermanentKind.LearnSkill && (teaching == null || gift == null)) return state;
            var prior = Find(state, quote.Kind, quote.DefinitionId, quote.CharacterId);
            if (prior != null) return state;
            Need(teaching != null && teaching.Level.Same(quote.TeachingLevel), "Permanent.Teaching.Binding", "InconsistentBinding");
            var challenges = new List<CandidateProgressionChallenge>(state.Challenges);
            if (quote.Kind == CandidatePermanentKind.BeginTeachingGift)
            {
                CheckEntry(business, teaching, budget);
                Need(quote.Outputs.Count == 1 && quote.Outputs[0].ItemId == teaching.ItemId &&
                    quote.Outputs[0].Quantity.IsOne && quote.Inputs.Count == 0, "Permanent.Teaching.Gift");
                CandidateProgressionChallenge challenge = null;
                foreach (var value in challenges)
                    if (value.Level.LevelId == teaching.Level.LevelId && !value.IsClosed) challenge = value;
                if (challenge == null)
                    challenges.Add(new CandidateProgressionChallenge(effect.OperationId,
                        ProgressionChecks.FindLevel(state.Definition.Levels, teaching.Level.LevelId),
                        new CandidateProgressionAttempt[0], null, teaching.Level));
            }
            else
            {
                Need(gift != null && gift.Quote.TeachingLevel.Same(teaching.Level), "Permanent.Teaching.Gift", "UnsupportedSourceProof");
                Need(quote.StepId == (quote.Kind == CandidatePermanentKind.LearnSkill
                    ? teaching.LearningStepId : teaching.ExplanationStepId), "Permanent.Teaching.Step", "InconsistentBinding");
                if (quote.Kind == CandidatePermanentKind.ConfirmTeachingExplanation)
                {
                    var learning = Find(state, CandidatePermanentKind.LearnSkill, teaching.SkillId);
                    Need(learning != null && quote.OriginalLearningOperation ==
                        (learning.RelatedOperationId ?? learning.OperationId), "Permanent.Teaching.Learning", "LearningUnavailable");
                }
            }
            var effects = new List<CandidatePermanentEffect>(state.GetPermanentEffects()) { effect };
            return new CandidateProgressionState(state.PlayerId, state.Definition,
                budget.Math.Add(state.StateRevision, BigInteger.One), state.OpenFacts, state.FirstClears, challenges, effects);
        }
    }
}
