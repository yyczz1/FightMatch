using System;
using System.Collections.Generic;
using System.Numerics;
using static FightMatch.Core.BusinessFields;

namespace FightMatch.Core
{
    public enum CandidatePermanentDefinitionKind { Card = 1, Skill = 2, Recipe = 3, Teaching = 4 }

    public sealed class CandidatePermanentDefinitionInput
    {
        public CandidatePermanentDefinitionKind Kind { get; set; }
        public string Id { get; set; }
        public BigInteger? RecordVersion { get; set; }
        public BigInteger? UnitExperience { get; set; }
        public string ClassId { get; set; }
        public string ItemId { get; set; }
        public string SkillId { get; set; }
        public string BattleCapabilityId { get; set; }
        public DefinitionBinding Level { get; set; }
        public string LearningStepId { get; set; }
        public string ExplanationStepId { get; set; }
        public IReadOnlyList<CandidateInventoryQuantityInput> Inputs { get; set; }
        public IReadOnlyList<CandidateInventoryQuantityInput> Outputs { get; set; }
    }

    public sealed class CandidatePermanentDefinition
    {
        public CandidatePermanentDefinitionKind Kind { get; }
        public string Id { get; }
        public BigInteger RecordVersion { get; }
        public BigInteger UnitExperience { get; }
        public string ClassId { get; }
        public string ItemId { get; }
        public string SkillId { get; }
        public string BattleCapabilityId { get; }
        public DefinitionBinding Level { get; }
        public string LearningStepId { get; }
        public string ExplanationStepId { get; }
        public IReadOnlyList<CandidateInventoryQuantity> Inputs { get; }
        public IReadOnlyList<CandidateInventoryQuantity> Outputs { get; }

        internal CandidatePermanentDefinition(CandidatePermanentDefinitionInput input,
            IReadOnlyList<CandidateInventoryQuantity> costs, IReadOnlyList<CandidateInventoryQuantity> output)
        {
            Kind = input.Kind;
            Id = input.Id;
            RecordVersion = input.RecordVersion.Value;
            UnitExperience = input.UnitExperience ?? BigInteger.Zero;
            ClassId = input.ClassId;
            ItemId = input.ItemId;
            SkillId = input.SkillId;
            BattleCapabilityId = input.BattleCapabilityId;
            Level = input.Level;
            LearningStepId = input.LearningStepId;
            ExplanationStepId = input.ExplanationStepId;
            Inputs = costs;
            Outputs = output;
        }
    }

    // Pure definitions do not prove publication or authorize a player-session write.
    public sealed class CandidatePermanentDefinitions
    {
        public ContentBinding Binding { get; }
        public IReadOnlyList<CandidatePermanentDefinition> Records { get; }

        private CandidatePermanentDefinitions(ContentBinding binding, List<CandidatePermanentDefinition> records)
        {
            Binding = binding;
            Records = records.AsReadOnly();
        }

        public CandidatePermanentDefinition Find(CandidatePermanentDefinitionKind kind, string id)
        {
            foreach (var record in Records)
                if (record.Kind == kind && record.Id == id) return record;
            return null;
        }

        public static SaveCodecResult<CandidatePermanentDefinitions> Prepare(ContentBinding binding,
            IReadOnlyList<CandidatePermanentDefinitionInput> inputs, SaveCodecBudget budget)
        {
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            return SaveCodecResult<CandidatePermanentDefinitions>.Run(() =>
            {
                RuleContextChecks.BindingBudget(binding, budget);
                SaveEnvelopeCodec.CheckList(inputs, budget, "Permanent.Definitions");
                var records = new List<CandidatePermanentDefinition>();
                var seen = new HashSet<string>(StringComparer.Ordinal);
                foreach (var input in inputs)
                {
                    Need(input != null, "Permanent.Definition", "MissingField");
                    Text(input.Id, budget);
                    Number(input.RecordVersion, budget);
                    Need((int)input.Kind >= 1 && (int)input.Kind <= 4, "Permanent.Kind", "UnsupportedBinding");
                    Need(seen.Add(((int)input.Kind).ToString() + ":" + input.Id), "Permanent.Definition", "ReceiptConflict");
                    var recipe = input.Kind == CandidatePermanentDefinitionKind.Recipe;
                    var skill = input.Kind == CandidatePermanentDefinitionKind.Skill;
                    var teaching = input.Kind == CandidatePermanentDefinitionKind.Teaching;
                    Need(recipe || input.Inputs == null && input.Outputs == null, "Permanent.Vector", "InvalidValue");
                    var costs = recipe ? Vector(input.Inputs, budget) : new List<CandidateInventoryQuantity>().AsReadOnly();
                    var output = recipe ? Vector(input.Outputs, budget) : new List<CandidateInventoryQuantity>().AsReadOnly();
                    if (input.Kind == CandidatePermanentDefinitionKind.Card) Number(input.UnitExperience, budget);
                    else Need(input.UnitExperience == null, "Permanent.UnitExperience");
                    if (skill || teaching)
                    {
                        Text(input.ClassId, budget);
                        Text(input.ItemId, budget);
                    }
                    else Need(input.ClassId == null && input.ItemId == null, "Permanent.ClassOrItem");
                    if (skill) Text(input.BattleCapabilityId, budget);
                    else Need(input.BattleCapabilityId == null, "Permanent.BattleCapability");
                    if (teaching)
                    {
                        Text(input.SkillId, budget);
                        Text(input.LearningStepId, budget);
                        Text(input.ExplanationStepId, budget);
                        Need(input.LearningStepId != input.ExplanationStepId, "Permanent.TeachingSteps");
                        Need(input.Level != null && binding.Same(input.Level.Content), "Permanent.TeachingBinding", "InconsistentBinding");
                    }
                    else Need(input.Level == null && input.SkillId == null &&
                        input.LearningStepId == null && input.ExplanationStepId == null, "Permanent.TeachingFields");
                    records.Add(new CandidatePermanentDefinition(input, costs, output));
                }
                records.Sort((a, b) => a.Kind != b.Kind ? a.Kind.CompareTo(b.Kind) : StringComparer.Ordinal.Compare(a.Id, b.Id));
                return new CandidatePermanentDefinitions(binding, records);
            });
        }

        internal static void Text(string value, SaveCodecBudget budget)
        {
            RuleContextChecks.Text(value, "Permanent.Id", budget, true);
        }

        internal static BigInteger Number(BigInteger? value, SaveCodecBudget budget, bool positive = true)
        {
            Need(value.HasValue, "Permanent.Quantity", "MissingField");
            budget.Math.CheckInteger(value.Value);
            Need(value.Value >= (positive ? BigInteger.One : BigInteger.Zero), "Permanent.Quantity");
            return value.Value;
        }

        internal static IReadOnlyList<CandidateInventoryQuantity> Vector(
            IReadOnlyList<CandidateInventoryQuantityInput> input, SaveCodecBudget budget)
        {
            SaveEnvelopeCodec.CheckList(input, budget, "Permanent.Vector");
            Need(input.Count > 0, "Permanent.Vector", "MissingField");
            var totals = new SortedDictionary<string, BigInteger>(StringComparer.Ordinal);
            foreach (var row in input)
            {
                Need(row != null, "Permanent.Vector.Row", "MissingField");
                Text(row.ItemId, budget);
                var amount = Number(row.Quantity, budget);
                totals.TryGetValue(row.ItemId, out var current);
                totals[row.ItemId] = budget.Math.Add(current, amount);
            }
            var result = new List<CandidateInventoryQuantity>();
            foreach (var row in totals) result.Add(new CandidateInventoryQuantity(row.Key, row.Value));
            return result.AsReadOnly();
        }

        internal void CheckQuote(CandidatePermanentQuote quote, SaveCodecBudget budget)
        {
            var kind = quote.Kind;
            if (kind == CandidatePermanentKind.Equip || kind == CandidatePermanentKind.SetPreference) return;
            var recordKind = kind == CandidatePermanentKind.UseExperienceCards ? CandidatePermanentDefinitionKind.Card :
                kind == CandidatePermanentKind.LearnSkill ? CandidatePermanentDefinitionKind.Skill :
                kind == CandidatePermanentKind.Craft ? CandidatePermanentDefinitionKind.Recipe : CandidatePermanentDefinitionKind.Teaching;
            var record = Find(recordKind, quote.DefinitionId);
            Need(record != null && Binding.Same(quote.Binding) && record.RecordVersion == quote.DefinitionVersion,
                "Permanent.Definition", "UnsupportedBinding");
            if (kind == CandidatePermanentKind.UseExperienceCards) Need(record.UnitExperience == quote.UnitExperience, "Permanent.UnitExperience");
            if (kind == CandidatePermanentKind.Craft)
            {
                Need(quote.Quantity > 0, "Permanent.Batches");
                CheckVector(record.Inputs, quote.Costs, quote.Quantity.Value, budget);
                CheckVector(record.Outputs, quote.Outputs, quote.Quantity.Value, budget);
            }
            if (kind == CandidatePermanentKind.LearnSkill)
            {
                Need(record.ClassId == quote.ClassId && quote.BeforeLevel >= 5, "Permanent.Skill");
                Need(quote.OriginalLearningOperation != null ? quote.Costs.Count == 0 : quote.Costs.Count == 1 &&
                    quote.Costs[0].ItemId == record.ItemId && quote.Costs[0].Quantity.IsOne, "Permanent.Certificate");
                var teaching = CandidatePermanentProgression.TeachingForSkill(this, record.Id);
                if (quote.TeachingLevel != null) Need(teaching != null && teaching.Level.Same(quote.TeachingLevel) &&
                    quote.StepId == teaching.LearningStepId, "Permanent.LearningStep", "TeachingRequired");
            }
            if (kind == CandidatePermanentKind.BeginTeachingGift) Need(quote.Outputs.Count == 0 || quote.Outputs.Count == 1 &&
                quote.Outputs[0].ItemId == record.ItemId && quote.Outputs[0].Quantity.IsOne, "Permanent.Gift");
            if (recordKind == CandidatePermanentDefinitionKind.Teaching)
                Need(record.Level.Same(quote.TeachingLevel) && (kind != CandidatePermanentKind.ConfirmTeachingExplanation ||
                    quote.StepId == record.ExplanationStepId), "Permanent.TeachingDefinition");
        }

        private static void CheckVector(IReadOnlyList<CandidateInventoryQuantity> definition,
            IReadOnlyList<CandidateInventoryQuantity> actual, BigInteger count, SaveCodecBudget budget)
        {
            Need(definition.Count == actual.Count, "Permanent.Vector");
            for (var i = 0; i < definition.Count; i++)
                Need(definition[i].ItemId == actual[i].ItemId &&
                    budget.Math.Multiply(definition[i].Quantity, count) == actual[i].Quantity, "Permanent.Vector");
        }

        internal void CheckClosure(PublishedRuleDefinitions definitions, SaveCodecBudget budget)
        {
            Need(definitions != null && Binding.Same(definitions.Binding), "Permanent.Binding", "InconsistentBinding");
            foreach (var record in Records)
            {
                if (record.Kind == CandidatePermanentDefinitionKind.Card)
                    Need(InventoryChecks.FindItem(definitions.Inventory, record.Id) != null, "Permanent.Card", "UnsupportedBinding");
                if (record.ClassId != null)
                    Need(definitions.FindGrowth(record.ClassId) != null, "Permanent.ClassId", "UnsupportedBinding");
                if (record.ItemId != null)
                    Need(InventoryChecks.FindItem(definitions.Inventory, record.ItemId) != null, "Permanent.ItemId", "UnsupportedBinding");
                foreach (var row in record.Inputs)
                    Need(InventoryChecks.FindItem(definitions.Inventory, row.ItemId) != null, "Permanent.Input", "UnsupportedBinding");
                foreach (var row in record.Outputs)
                    Need(InventoryChecks.FindItem(definitions.Inventory, row.ItemId) != null, "Permanent.Output", "UnsupportedBinding");
                if (record.Kind != CandidatePermanentDefinitionKind.Teaching) continue;
                var level = ProgressionChecks.FindLevel(definitions.Progression.Levels, record.Level.LevelId);
                var skill = Find(CandidatePermanentDefinitionKind.Skill, record.SkillId);
                Need(level != null && level.LevelVersion == record.Level.CanonicalLevelVersion &&
                    definitions.FindGrowth(record.ClassId).ClassKind == CharacterClassKind.Warrior &&
                    skill != null && skill.ClassId == record.ClassId && skill.ItemId == record.ItemId,
                    "Permanent.Teaching", "InconsistentBinding");
            }
        }
    }
}
