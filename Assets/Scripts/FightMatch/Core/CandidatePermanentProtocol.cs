using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static FightMatch.Core.BusinessFields;

namespace FightMatch.Core
{
    public static class CandidatePermanentProtocol
    {
        public static SaveCodecResult<CandidatePermanentQuote> Preview(CandidateApplicationSnapshot basis,
            PublishedRuleDefinitions definitions, CandidatePermanentDraft draft, SaveCodecBudget budget)
        {
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            return SaveCodecResult<CandidatePermanentQuote>.Run(() =>
            {
                Need(basis != null && definitions != null && draft != null, "Permanent", "MissingField");
                var business = basis.Business;
                Need(basis.Continuation == null, "Permanent.Continuation", "ResolutionRequired");
                Need(business.Format == CandidateBusinessFormat.PublishedPermanentV4, "Permanent.Format", "MigrationRequired");
                Need(business.Inventory.Definition.Context is PreparedPublishedRuleContext context &&
                    context.Binding.Same(definitions.Binding), "Permanent.Binding", "InconsistentBinding");
                Need((int)draft.Kind >= 1 && (int)draft.Kind <= 7, "Permanent.Kind", "UnsupportedBinding");
                var permanent = definitions.GetPermanentDefinitions();
                var character = draft.CharacterId == null ? null : business.Roster.Find(draft.CharacterId);
                Need(draft.CharacterId == null || character != null, "Permanent.Character", "InconsistentBinding");
                var costs = new List<CandidateInventoryQuantity>();
                var outputs = new List<CandidateInventoryQuantity>();
                var version = BigInteger.One;
                var unitXp = BigInteger.Zero;
                var xp = BigInteger.Zero;
                var level = character?.Level ?? BigInteger.Zero;
                var remainder = character?.Experience ?? BigInteger.Zero;
                DefinitionBinding teaching = null;
                string learning = null;
                CandidatePermanentDefinition definition = null;
                if (draft.Kind != CandidatePermanentKind.Equip && draft.Kind != CandidatePermanentKind.SetPreference)
                {
                    var kind = draft.Kind == CandidatePermanentKind.UseExperienceCards ? CandidatePermanentDefinitionKind.Card :
                        draft.Kind == CandidatePermanentKind.LearnSkill ? CandidatePermanentDefinitionKind.Skill :
                        draft.Kind == CandidatePermanentKind.Craft ? CandidatePermanentDefinitionKind.Recipe : CandidatePermanentDefinitionKind.Teaching;
                    definition = permanent?.Find(kind, draft.DefinitionId);
                    Need(definition != null, "Permanent.Definition", "NoPublishedDefinition");
                    version = definition.RecordVersion;
                }
                switch (draft.Kind)
                {
                    case CandidatePermanentKind.UseExperienceCards:
                        Need(character != null, "Permanent.Character", "MissingField");
                        var target = CandidatePermanentDefinitions.Number(draft.Quantity, budget);
                        var requirement = Take(CandidatePermanentGrowth.CalculateCards(character, permanent, draft.DefinitionId, target, budget), "Permanent.Growth");
                        unitXp = definition.UnitExperience;
                        xp = requirement.FixedExperience;
                        level = requirement.FinalLevel;
                        remainder = requirement.FinalExperience;
                        costs.Add(new CandidateInventoryQuantity(draft.DefinitionId, requirement.Cards));
                        break;
                    case CandidatePermanentKind.LearnSkill:
                        Need(character != null && character.ClassId == definition.ClassId, "Permanent.Skill", "InconsistentBinding");
                        Need(character.Level >= 5, "Permanent.Skill.Level", "LevelRequirement");
                        var prior = CandidatePermanentGrowth.FindLearning(business.Roster, character.CharacterId, definition.Id);
                        learning = prior?.OperationId;
                        if (prior == null) costs.Add(new CandidateInventoryQuantity(definition.ItemId, BigInteger.One));
                        var tutorial = CandidatePermanentProgression.TeachingForSkill(permanent, definition.Id);
                        if (tutorial != null && CandidatePermanentProgression.Find(business.Progression, CandidatePermanentKind.BeginTeachingGift, tutorial.Id) != null)
                            teaching = tutorial.Level;
                        break;
                    case CandidatePermanentKind.Craft:
                        var batches = CandidatePermanentDefinitions.Number(draft.Quantity, budget);
                        foreach (var input in definition.Inputs)
                            costs.Add(new CandidateInventoryQuantity(input.ItemId, budget.Math.Multiply(input.Quantity, batches)));
                        foreach (var output in definition.Outputs)
                            outputs.Add(new CandidateInventoryQuantity(output.ItemId, budget.Math.Multiply(output.Quantity, batches)));
                        break;
                    case CandidatePermanentKind.Equip:
                    case CandidatePermanentKind.SetPreference:
                        Need(character != null, "Permanent.Character", "MissingField");
                        CandidatePermanentInventory.CheckLoadout(business, draft, budget);
                        break;
                    case CandidatePermanentKind.BeginTeachingGift:
                        CandidatePermanentProgression.CheckEntry(business, definition, budget);
                        teaching = definition.Level;
                        if (CandidatePermanentProgression.Find(business.Progression, draft.Kind, definition.Id) == null)
                            outputs.Add(new CandidateInventoryQuantity(definition.ItemId, BigInteger.One));
                        break;
                    case CandidatePermanentKind.ConfirmTeachingExplanation:
                        teaching = definition.Level;
                        var learned = CandidatePermanentProgression.Find(business.Progression, CandidatePermanentKind.LearnSkill, definition.SkillId);
                        Need(learned != null && draft.StepId == definition.ExplanationStepId, "Permanent.Teaching", "LearningUnavailable");
                        learning = learned.RelatedOperationId ?? learned.OperationId;
                        break;
                }
                var selected = CandidatePermanentInventory.Select(basis, costs, draft,
                    portion => CandidatePermanentProgression.AllowsCertificate(business, permanent, draft, portion), budget);
                foreach (var portion in selected)
                    Need(portion.Source == null || definitions.Binding.Same(portion.Source.Grant.Binding), "Permanent.Source.Binding", "UnsupportedSourceProof");
                var quote = new CandidatePermanentQuote(draft, business.PlayerId, character?.ClassId, definitions.Binding,
                    basis.Header.SaveGeneration, basis.Descriptor.TotalLength, basis.Descriptor.Sha256, version,
                    character?.StateRevision ?? BigInteger.Zero, business.Inventory.StateRevision, business.Inventory.PreferenceRevision,
                    business.Progression.StateRevision, unitXp, xp, character?.Level ?? BigInteger.Zero,
                    character?.Experience ?? BigInteger.Zero, level, remainder, teaching, selected, costs, outputs, learning);
                Shape(quote, budget);
                permanent?.CheckQuote(quote, budget);
                return quote;
            });
        }

        public static SaveCodecResult<bool> ValidatePreview(CandidateApplicationSnapshot basis,
            PublishedRuleDefinitions definitions, CandidatePermanentQuote quote, SaveCodecBudget budget)
        {
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            return SaveCodecResult<bool>.Run(() =>
            {
                Need(quote != null, "Permanent.Quote", "MissingField");
                var current = Take(Preview(basis, definitions, quote.Draft, budget), "Permanent.Preview");
                Need(CandidatePermanentCodec.SameQuote(current, quote, budget), "Permanent.Quote", "StaleContext");
                return true;
            });
        }

        public static SaveCodecResult<CandidateBusinessSnapshot> Build(CandidateApplicationSnapshot basis,
            PublishedRuleDefinitions definitions, CandidatePermanentQuote quote, string operationId, SaveCodecBudget budget)
        {
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            return SaveCodecResult<CandidateBusinessSnapshot>.Run(() =>
            {
                CandidatePermanentDefinitions.Text(operationId, budget);
                Need(quote != null, "Permanent.Quote", "MissingField");
                var current = Take(Preview(basis, definitions, quote.Draft, budget), "Permanent.Preview");
                Need(CandidatePermanentCodec.SameQuote(current, quote, budget), "Permanent.Quote", "StaleContext");
                var outcome = quote.Kind == CandidatePermanentKind.LearnSkill && quote.OriginalLearningOperation != null
                    ? "AlreadyLearned" : "Applied";
                if (quote.Kind == CandidatePermanentKind.Equip || quote.Kind == CandidatePermanentKind.SetPreference)
                    outcome = CandidatePermanentInventory.CheckLoadout(basis.Business, quote.Draft, budget) ? "Applied" : "Unchanged";
                if ((quote.Kind == CandidatePermanentKind.BeginTeachingGift || quote.Kind == CandidatePermanentKind.ConfirmTeachingExplanation) &&
                    CandidatePermanentProgression.Find(basis.Business.Progression, quote.Kind, quote.DefinitionId) != null) outcome = "Unchanged";
                var effect = new CandidatePermanentEffect(operationId, quote, outcome, quote.OriginalLearningOperation);
                var permanent = definitions.GetPermanentDefinitions();
                var roster = CandidatePermanentGrowth.Apply(basis.Business.Roster, effect, permanent, budget);
                var inventory = CandidatePermanentInventory.Apply(basis, effect, budget);
                var progression = CandidatePermanentProgression.Apply(basis.Business, effect, permanent, budget);
                return Copy(basis.Business, roster, inventory, progression, CandidateBusinessFormat.PublishedPermanentV4, budget);
            });
        }

        public static SaveCodecResult<CandidateBusinessSnapshot> Migrate(CandidateApplicationSnapshot basis,
            CandidateRosterMigrationInput input, SaveCodecBudget budget)
        {
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            return SaveCodecResult<CandidateBusinessSnapshot>.Run(() =>
            {
                Need(basis != null && input != null, "Permanent.Migration", "MissingField");
                Need(basis.Business.Format == CandidateBusinessFormat.PublishedRosterV3 && basis.Continuation == null,
                    "Permanent.Migration", "UnsupportedBinding");
                Need(input.SourceGeneration == basis.Header.SaveGeneration && input.SourceDescriptorLength == basis.Descriptor.TotalLength &&
                    CandidateApplicationIntentCodec.SameBytes(input.SourceDescriptorSha256, basis.Descriptor.Sha256),
                    "Permanent.Migration.Source", "StaleContext");
                return Copy(basis.Business, basis.Business.Roster, basis.Business.Inventory, basis.Business.Progression,
                    CandidateBusinessFormat.PublishedPermanentV4, budget);
            });
        }

        internal static CandidateBusinessSnapshot Copy(CandidateBusinessSnapshot basis, CandidateRosterState roster,
            CandidateInventoryState inventory, CandidateProgressionState progression, CandidateBusinessFormat format, SaveCodecBudget budget)
        {
            return Take(CandidateBusinessSaveCodec.Prepare(new CandidateBusinessInput(basis.PlayerId, roster, inventory, progression,
                basis.Rewards, basis.ActiveHistory, basis.RetainedRuns, basis.RetainedRollbacks, format), SavePurpose.PlayerSave, budget), "Permanent.Business");
        }

        internal static CandidatePermanentEffect Find(IReadOnlyList<CandidatePermanentEffect> effects, string operation)
        {
            foreach (var effect in effects) if (effect.OperationId == operation) return effect;
            return null;
        }

        internal static CandidatePermanentResult Result(CandidateBusinessSnapshot business, PreparedCandidateApplicationIntent intent)
        {
            if (intent.Kind != CandidateApplicationKind.PermanentRequest) return null;
            var character = Find(business.Roster.GetPermanentEffects(), intent.OperationId);
            var inventory = Find(business.Inventory.GetPermanentLedger().Effects, intent.OperationId);
            var progression = Find(business.Progression.GetPermanentEffects(), intent.OperationId);
            var learned = intent.GetPermanent().OriginalLearningOperation;
            var outcome = intent.GetPermanent().Kind == CandidatePermanentKind.LearnSkill && learned != null ? "AlreadyLearned" :
                character == null && inventory == null && progression == null ? "Unchanged" : "Applied";
            return new CandidatePermanentResult(outcome, character?.OperationId, inventory?.OperationId, progression?.OperationId, learned);
        }

        internal static bool Transition(CandidateApplicationSnapshot basis, CandidateBusinessSnapshot after,
            PreparedCandidateApplicationIntent intent, SaveCodecBudget budget)
        {
            if (intent.Kind != CandidateApplicationKind.PermanentRequest && intent.Kind != CandidateApplicationKind.MigratePermanent) return false;
            var before = basis.Business;
            Need(basis.Continuation == null && after.Format == CandidateBusinessFormat.PublishedPermanentV4 &&
                ReferenceEquals(before.Rewards, after.Rewards) && ReferenceEquals(before.ActiveHistory, after.ActiveHistory),
                "Permanent.Transition", "InconsistentBinding");
            if (intent.Kind == CandidateApplicationKind.MigratePermanent)
            {
                Take(Migrate(basis, intent.GetPermanentMigration(), budget), "Permanent.Migration");
                Need(ReferenceEquals(before.Roster, after.Roster) && ReferenceEquals(before.Inventory, after.Inventory) &&
                    ReferenceEquals(before.Progression, after.Progression), "Permanent.Migration.Owners", "InconsistentBinding");
                return true;
            }
            var q = intent.GetPermanent();
            Need(before.Format == after.Format && q.SourceGeneration == basis.Header.SaveGeneration &&
                q.SourceDescriptorLength == basis.Descriptor.TotalLength &&
                CandidateApplicationIntentCodec.SameBytes(q.SourceDescriptorSha256, basis.Descriptor.Sha256), "Permanent.Head", "StaleContext");
            Need(q.InventoryRevision == before.Inventory.StateRevision && q.PreferenceRevision == before.Inventory.PreferenceRevision &&
                q.ProgressionRevision == before.Progression.StateRevision &&
                (q.CharacterId == null || before.Roster.Find(q.CharacterId)?.StateRevision == q.CharacterRevision), "Permanent.Owners", "StaleContext");
            var result = Result(after, intent);
            var effect = new CandidatePermanentEffect(intent.OperationId, q, result.Outcome, q.OriginalLearningOperation);
            var roster = result.CharacterEffectOperation == null ? before.Roster : CandidatePermanentGrowth.ApplyFrozen(before.Roster, effect, budget);
            var inventory = CandidatePermanentInventory.Apply(basis, effect, budget);
            Need(CandidateBusinessSaveCodec.SamePermanentOwners(roster, inventory, after.Roster, after.Inventory, budget),
                "Permanent.Transition.Owners", "InconsistentBinding");
            Need(ReferenceEquals(before.Progression.Definition, after.Progression.Definition) &&
                before.Progression.OpenFacts.SequenceEqual(after.Progression.OpenFacts) &&
                before.Progression.FirstClears.SequenceEqual(after.Progression.FirstClears), "Permanent.Transition.Progression");
            if (q.Kind != CandidatePermanentKind.BeginTeachingGift)
                Need(before.Progression.Challenges.SequenceEqual(after.Progression.Challenges), "Permanent.Transition.Challenges");
            else
            {
                Need(after.Progression.Challenges.Count >= before.Progression.Challenges.Count &&
                    after.Progression.Challenges.Count <= before.Progression.Challenges.Count + 1, "Permanent.Transition.Challenges");
                for (var i = 0; i < before.Progression.Challenges.Count; i++)
                    Need(ReferenceEquals(before.Progression.Challenges[i], after.Progression.Challenges[i]), "Permanent.Transition.Challenges");
            }
            return true;
        }

        internal static void Shape(CandidatePermanentQuote quote, SaveCodecBudget budget)
        {
            var kind = quote.Kind;
            var hasCharacter = kind == CandidatePermanentKind.UseExperienceCards || kind == CandidatePermanentKind.LearnSkill ||
                kind == CandidatePermanentKind.Equip || kind == CandidatePermanentKind.SetPreference;
            Need(hasCharacter == (quote.CharacterId != null) && hasCharacter == (quote.ClassId != null), "Permanent.Character");
            Need(hasCharacter ? quote.BeforeLevel > 0 && quote.FinalLevel > 0 && quote.CharacterRevision > 0 :
                quote.BeforeLevel.IsZero && quote.FinalLevel.IsZero && quote.CharacterRevision.IsZero, "Permanent.CharacterState");
            Need((kind == CandidatePermanentKind.UseExperienceCards || kind == CandidatePermanentKind.Craft || kind == CandidatePermanentKind.Equip)
                == quote.Quantity.HasValue, "Permanent.Quantity");
            Need((kind == CandidatePermanentKind.SetPreference) == quote.Enabled.HasValue, "Permanent.Enabled");
            Need(kind == CandidatePermanentKind.SetPreference || quote.Draft.PreferenceRevision == null, "Permanent.PreferenceRevision");
            Need(kind == CandidatePermanentKind.LearnSkill || kind == CandidatePermanentKind.ConfirmTeachingExplanation ||
                quote.StepId == null, "Permanent.Step");
            Need(kind == CandidatePermanentKind.UseExperienceCards ? quote.UnitExperience > 0 && quote.FixedExperience > 0 :
                quote.UnitExperience.IsZero && quote.FixedExperience.IsZero && quote.FinalLevel == quote.BeforeLevel &&
                quote.FinalExperience == quote.BeforeExperience, "Permanent.FixedEffect");
            Need(kind == CandidatePermanentKind.Craft || kind == CandidatePermanentKind.BeginTeachingGift || quote.Outputs.Count == 0, "Permanent.Outputs");
            Need(kind == CandidatePermanentKind.UseExperienceCards || kind == CandidatePermanentKind.LearnSkill ||
                kind == CandidatePermanentKind.Craft || quote.Costs.Count == 0, "Permanent.Costs");
            Need(kind == CandidatePermanentKind.LearnSkill || kind == CandidatePermanentKind.ConfirmTeachingExplanation ||
                quote.OriginalLearningOperation == null, "Permanent.OriginalLearning");
            Need(kind == CandidatePermanentKind.LearnSkill || kind == CandidatePermanentKind.BeginTeachingGift ||
                kind == CandidatePermanentKind.ConfirmTeachingExplanation || quote.TeachingLevel == null, "Permanent.Teaching");
            var totals = new SortedDictionary<string, BigInteger>(StringComparer.Ordinal);
            foreach (var input in quote.Inputs)
            {
                Need(input.Endpoint == CandidatePermanentEndpoint.Held && input.EndpointOperationId == null, "Permanent.Input.Endpoint");
                totals.TryGetValue(input.ItemId, out var old);
                totals[input.ItemId] = budget.Math.Add(old, input.UnitCount);
            }
            Need(totals.Count == quote.Costs.Count, "Permanent.Costs");
            foreach (var cost in quote.Costs) Need(totals.TryGetValue(cost.ItemId, out var value) && value == cost.Quantity, "Permanent.Costs");
            if (kind == CandidatePermanentKind.UseExperienceCards)
                Need(quote.Costs.Count == 1 && quote.Costs[0].ItemId == quote.DefinitionId &&
                    budget.Math.Multiply(quote.Costs[0].Quantity, quote.UnitExperience) == quote.FixedExperience, "Permanent.FixedExperience");
        }
    }
}
