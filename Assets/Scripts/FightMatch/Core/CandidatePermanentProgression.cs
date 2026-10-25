using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static FightMatch.Core.BusinessFields;

namespace FightMatch.Core
{
    public sealed class CandidateTeachingProgress
    {
        public CandidatePermanentDefinition Definition { get; }
        public CandidatePermanentEffect Gift { get; }
        public CandidatePermanentEffect Learning { get; }
        public CandidatePermanentEffect Explanation { get; }
        public bool IsComplete => Gift != null && Learning != null && Explanation != null;

        internal CandidateTeachingProgress(CandidatePermanentDefinition definition, CandidatePermanentEffect gift,
            CandidatePermanentEffect learning, CandidatePermanentEffect explanation)
        { Definition = definition; Gift = gift; Learning = learning; Explanation = explanation; }
    }

    public static class CandidatePermanentProgression
    {
        internal static CandidatePermanentDefinitions OriginalDefinitions(CandidatePermanentQuote quote,
            CandidatePermanentDefinitions current, PublishedSaveContext sources)
        {
            var original = current.Binding.Same(quote.Binding) ? current : sources?.FindExact(quote.Binding)?.GetPermanentDefinitions();
            Need(original != null, "Permanent.Teaching.OriginalDefinition", "UnsupportedSourceProof");
            return original;
        }

        private static CandidatePermanentDefinition OriginalTeaching(CandidatePermanentQuote quote,
            CandidatePermanentDefinitions current, PublishedSaveContext sources)
        {
            var definitions = OriginalDefinitions(quote, current, sources);
            var originalKind = quote.Kind == CandidatePermanentKind.LearnSkill
                ? CandidatePermanentDefinitionKind.Skill : CandidatePermanentDefinitionKind.Teaching;
            var original = definitions.Find(originalKind, quote.DefinitionId);
            Need(original != null && original.RecordVersion == quote.DefinitionVersion,
                "Permanent.Teaching.OriginalVersion", "UnsupportedSourceProof");
            var teaching = quote.Kind == CandidatePermanentKind.LearnSkill
                ? TeachingForSkill(definitions, quote.DefinitionId)
                : definitions.Find(CandidatePermanentDefinitionKind.Teaching, quote.DefinitionId);
            Need(teaching != null && quote.TeachingLevel != null && teaching.Level.Same(quote.TeachingLevel),
                "Permanent.Teaching.OriginalBinding", "UnsupportedSourceProof");
            return teaching;
        }

        private static bool SameTeaching(CandidatePermanentDefinition original, CandidatePermanentDefinition current)
        {
            if (current.GiftOnceKey == null || original.GiftOnceKey == null)
                return current.GiftOnceKey == original.GiftOnceKey && original.Id == current.Id && original.Level.Same(current.Level);
            if (original.GiftOnceKey != current.GiftOnceKey) return false;
            Need(original.Level.LevelId == current.Level.LevelId && original.ClassId == current.ClassId &&
                original.SkillId == current.SkillId && original.ItemId == current.ItemId,
                "Permanent.Teaching.Identity", "UnsupportedSourceProof");
            return true;
        }

        internal static CandidatePermanentEffect FindTeaching(CandidateProgressionState state, CandidatePermanentKind kind,
            CandidatePermanentDefinition teaching, CandidatePermanentDefinitions definitions, PublishedSaveContext sources)
        {
            CandidatePermanentEffect found = null;
            foreach (var effect in state.GetPermanentEffects())
            {
                if (effect.Quote.Kind != kind || effect.Quote.TeachingLevel == null) continue;
                if (!SameTeaching(OriginalTeaching(effect.Quote, definitions, sources), teaching)) continue;
                Need(found == null, "Permanent.Teaching.Once", "ReceiptConflict");
                found = effect;
            }
            return found;
        }

        public static SaveCodecResult<CandidateTeachingProgress> ReadTeaching(CandidateBusinessSnapshot business,
            DefinitionBinding level, PublishedRuleDefinitions definitions, SaveCodecBudget budget, PublishedSaveContext sources = null)
        {
            return SaveCodecResult<CandidateTeachingProgress>.Run(() =>
            {
                Need(business != null && level != null && definitions != null && level.Content.Same(definitions.Binding),
                    "Permanent.Teaching.Binding", "InconsistentBinding");
                var permanent = definitions.GetPermanentDefinitions();
                var teaching = permanent?.Records.FirstOrDefault(x => x.Kind == CandidatePermanentDefinitionKind.Teaching && x.Level.Same(level));
                if (teaching == null) return new CandidateTeachingProgress(null, null, null, null);
                var gift = FindTeaching(business.Progression, CandidatePermanentKind.BeginTeachingGift, teaching, permanent, sources);
                var learning = FindTeaching(business.Progression, CandidatePermanentKind.LearnSkill, teaching, permanent, sources);
                var explanation = FindTeaching(business.Progression, CandidatePermanentKind.ConfirmTeachingExplanation, teaching, permanent, sources);
                Need(gift == null || gift.Quote.PlayerId == business.PlayerId && gift.Quote.Outputs.Count == 1 &&
                    gift.Quote.Outputs[0].ItemId == teaching.ItemId && gift.Quote.Outputs[0].Quantity.IsOne &&
                    CandidatePermanentProtocol.Find(business.Inventory.GetPermanentLedger().Effects, gift.OperationId) != null,
                    "Permanent.Teaching.Gift", "UnsupportedSourceProof");
                Need(learning == null || gift != null && CandidatePermanentGrowth.FindLearning(business.Roster,
                    learning.Quote.CharacterId, teaching.SkillId)?.OperationId == (learning.RelatedOperationId ?? learning.OperationId),
                    "Permanent.Teaching.Learning", "UnsupportedSourceProof");
                Need(explanation == null || learning != null && explanation.Quote.OriginalLearningOperation ==
                    (learning.RelatedOperationId ?? learning.OperationId), "Permanent.Teaching.Explanation", "UnsupportedSourceProof");
                return new CandidateTeachingProgress(teaching, gift, learning, explanation);
            });
        }

        public static SaveCodecResult<bool> CheckTeachingAdmission(CandidateBusinessSnapshot business,
            DefinitionBinding level, PublishedRuleDefinitions definitions, SaveCodecBudget budget, PublishedSaveContext sources = null)
        {
            return SaveCodecResult<bool>.Run(() =>
            {
                var progress = Take(ReadTeaching(business, level, definitions, budget, sources), "Permanent.Teaching");
                if (progress.Definition == null) return true;
                CheckEntry(business, progress.Definition, budget);
                Need(progress.Gift != null, "Permanent.Teaching.Gift", "TeachingGiftRequired");
                Need(progress.Learning != null, "Permanent.Teaching.Learning", "TeachingLearningRequired");
                Need(progress.Explanation != null, "Permanent.Teaching.Explanation", "TeachingExplanationRequired");
                return true;
            });
        }
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
            CandidatePermanentDraft draft, CandidatePermanentPortion portion, PublishedSaveContext sources = null)
        {
            if (portion.OutputOperationId == null) return true;
            CandidatePermanentEffect original = null;
            foreach (var effect in business.Inventory.GetPermanentLedger().Effects)
                if (effect.OperationId == portion.OutputOperationId) original = effect;
            if (original == null) return false;
            if (original.Quote.Kind != CandidatePermanentKind.BeginTeachingGift) return true;
            var teaching = OriginalTeaching(original.Quote, definitions, sources);
            if (FindTeaching(business.Progression, CandidatePermanentKind.ConfirmTeachingExplanation, teaching, definitions, sources) != null) return true;
            var character = business.Roster.Find(draft.CharacterId);
            return draft.Kind == CandidatePermanentKind.LearnSkill && draft.DefinitionId == teaching.SkillId &&
                character != null && character.ClassId == teaching.ClassId;
        }

        internal static List<CandidatePermanentPortion> PreferTeachingCertificate(List<CandidatePermanentPortion> available,
            CandidateBusinessSnapshot business, CandidatePermanentDraft draft,
            CandidatePermanentDefinitions definitions = null, PublishedSaveContext sources = null)
        {
            if (draft.Kind != CandidatePermanentKind.LearnSkill) return available;
            var gift = available.FindAll(x => {
                var original = CandidatePermanentProtocol.Find(business.Inventory.GetPermanentLedger().Effects, x.OutputOperationId);
                return original?.Quote.Kind == CandidatePermanentKind.BeginTeachingGift && (draft.SelectedInputs == null ||
                    (definitions == null ? Find(business.Progression, CandidatePermanentKind.ConfirmTeachingExplanation, original.Quote.DefinitionId) :
                        FindTeaching(business.Progression, CandidatePermanentKind.ConfirmTeachingExplanation,
                            OriginalTeaching(original.Quote, definitions, sources), definitions, sources)) == null);
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
            IEnumerable<(string Operation, CandidatePermanentQuote Quote)> records, CandidatePermanentDefinitions definitions,
            PublishedSaveContext sources = null)
        {
            var prior = records.Where(x => x.Quote != null).ToList();
            var currentTeaching = quote.Kind == CandidatePermanentKind.LearnSkill ? TeachingForSkill(definitions, quote.DefinitionId) :
                quote.Kind == CandidatePermanentKind.BeginTeachingGift || quote.Kind == CandidatePermanentKind.ConfirmTeachingExplanation
                    ? definitions.Find(CandidatePermanentDefinitionKind.Teaching, quote.DefinitionId) : null;
            bool MatchesTeaching(CandidatePermanentQuote candidate, CandidatePermanentDefinition teaching) =>
                candidate.TeachingLevel != null && SameTeaching(OriginalTeaching(candidate, definitions, sources), teaching);
            var gifts = currentTeaching == null ? new List<CandidatePermanentQuote>() : prior.Where(x => x.Quote.Kind == CandidatePermanentKind.BeginTeachingGift &&
                x.Quote.Outputs.Count == 1 && MatchesTeaching(x.Quote, currentTeaching)).Select(x => x.Quote).ToList();
            Need(gifts.Count <= 1, "Permanent.History.GiftOnce", "ReceiptConflict");
            var originalGift = gifts.Count == 0 ? null : gifts[0];
            if (quote.Kind == CandidatePermanentKind.BeginTeachingGift)
                Need((originalGift == null) == (quote.Outputs.Count == 1), "Permanent.History.GiftOnce", "ReceiptConflict");
            if (quote.Kind == CandidatePermanentKind.LearnSkill)
            {
                Need((originalGift != null) == (quote.TeachingLevel != null), "Permanent.History.TeachingBinding", "TeachingRequired");
            }
            if (quote.Kind == CandidatePermanentKind.ConfirmTeachingExplanation)
            {
                Need(originalGift != null, "Permanent.History.Gift", "UnsupportedSourceProof");
                Need(prior.Any(x => x.Quote.Kind == CandidatePermanentKind.LearnSkill && MatchesTeaching(x.Quote, currentTeaching) &&
                    (x.Quote.OriginalLearningOperation ?? x.Operation) == quote.OriginalLearningOperation),
                    "Permanent.History.Explanation", "LearningUnavailable");
            }
            foreach (var input in quote.Inputs)
            {
                if (input.OutputOperationId == null) continue;
                var source = prior.Find(x => x.Operation == input.OutputOperationId).Quote;
                Need(source != null, "Permanent.History.Input", "UnsupportedSourceProof");
                if (source.Kind != CandidatePermanentKind.BeginTeachingGift) continue;
                var teaching = OriginalTeaching(source, definitions, sources);
                var explained = prior.Any(x => x.Quote.Kind == CandidatePermanentKind.ConfirmTeachingExplanation &&
                    MatchesTeaching(x.Quote, teaching));
                Need(explained || quote.Kind == CandidatePermanentKind.LearnSkill &&
                    quote.DefinitionId == teaching.SkillId && quote.ClassId == teaching.ClassId &&
                    currentTeaching != null && MatchesTeaching(quote, teaching),
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
            CandidatePermanentDefinitions definitions, SaveCodecBudget budget, PublishedSaveContext sources = null)
        {
            var state = business.Progression;
            var quote = effect.Quote;
            Need(quote.ProgressionRevision == state.StateRevision, "Permanent.ProgressionRevision", "StaleContext");
            if (quote.Kind != CandidatePermanentKind.LearnSkill && quote.Kind != CandidatePermanentKind.BeginTeachingGift &&
                quote.Kind != CandidatePermanentKind.ConfirmTeachingExplanation) return state;
            var teaching = quote.Kind == CandidatePermanentKind.LearnSkill
                ? TeachingForSkill(definitions, quote.DefinitionId)
                : definitions.Find(CandidatePermanentDefinitionKind.Teaching, quote.DefinitionId);
            var gift = teaching == null ? null : FindTeaching(state, CandidatePermanentKind.BeginTeachingGift, teaching, definitions, sources);
            if (quote.Kind == CandidatePermanentKind.LearnSkill && (teaching == null || gift == null)) return state;
            var prior = teaching == null ? null : FindTeaching(state, quote.Kind, teaching, definitions, sources);
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
                Need(gift != null, "Permanent.Teaching.Gift", "UnsupportedSourceProof");
                Need(quote.StepId == (quote.Kind == CandidatePermanentKind.LearnSkill
                    ? teaching.LearningStepId : teaching.ExplanationStepId), "Permanent.Teaching.Step", "InconsistentBinding");
                if (quote.Kind == CandidatePermanentKind.ConfirmTeachingExplanation)
                {
                    var learning = FindTeaching(state, CandidatePermanentKind.LearnSkill, teaching, definitions, sources);
                    Need(learning != null && quote.OriginalLearningOperation ==
                        (learning.RelatedOperationId ?? learning.OperationId), "Permanent.Teaching.Learning", "LearningUnavailable");
                }
            }
            var effects = new List<CandidatePermanentEffect>(state.GetPermanentEffects()) { effect };
            return new CandidateProgressionState(state.PlayerId, state.Definition,
                budget.Math.Add(state.StateRevision, BigInteger.One), state.OpenFacts, state.FirstClears, challenges, effects, state.GetChapterEffects());
        }
    }
}
