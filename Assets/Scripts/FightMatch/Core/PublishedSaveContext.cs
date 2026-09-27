using System;
using System.Collections.Generic;

namespace FightMatch.Core
{
    // A resolved definition closure, not a publication receipt or a player session.
    public sealed class PublishedRuleDefinitions
    {
        private readonly CandidatePermanentDefinitions permanent;
        public CandidatePermanentDefinitions GetPermanentDefinitions() { return permanent; }
        public ContentBinding Binding { get; }
        public CandidateGrowthDefinition Growth => CandidateRosterState.Single(Growths);
        public IReadOnlyList<CandidateGrowthDefinition> Growths { get; }
        public CandidateGrowthDefinition FindGrowth(string classId)
        { foreach (var growth in Growths) if (growth.ClassId == classId) return growth; return null; }
        public bool TryGetSingle(out CandidateGrowthDefinition growth)
        { growth = Growths.Count == 1 ? Growths[0] : null; return growth != null; }
        public CandidateInventoryDefinition Inventory { get; }
        public CandidateProgressionDefinition Progression { get; }
        public IReadOnlyList<PreparedLevel> Levels { get; }
        public IReadOnlyList<CandidateRewardDefinition> Rewards { get; }

        private PublishedRuleDefinitions(ContentBinding binding, IReadOnlyList<CandidateGrowthDefinition> growths, CandidateInventoryDefinition inventory,
            CandidateProgressionDefinition progression, IReadOnlyList<PreparedLevel> levels, IReadOnlyList<CandidateRewardDefinition> rewards,
            CandidatePermanentDefinitions permanentDefinitions = null)
        {
            permanent = permanentDefinitions;
            Binding = binding; Growths = new List<CandidateGrowthDefinition>(growths).AsReadOnly(); Inventory = inventory; Progression = progression;
            Levels = new List<PreparedLevel>(levels).AsReadOnly(); Rewards = new List<CandidateRewardDefinition>(rewards).AsReadOnly();
        }

        public static SaveCodecResult<PublishedRuleDefinitions> Prepare(ContentBinding binding, CandidateGrowthDefinition growth,
            CandidateInventoryDefinition inventory, CandidateProgressionDefinition progression, IReadOnlyList<PreparedLevel> levels,
            IReadOnlyList<CandidateRewardDefinition> rewards, SaveCodecBudget budget)
        { return Prepare(binding, new[] { growth }, inventory, progression, levels, rewards, budget); }

        // A constrained overload keeps the existing untyped-null single-definition call unambiguous.
        public static SaveCodecResult<PublishedRuleDefinitions> Prepare<TGrowths>(ContentBinding binding, TGrowths growths,
            CandidateInventoryDefinition inventory, CandidateProgressionDefinition progression, IReadOnlyList<PreparedLevel> levels,
            IReadOnlyList<CandidateRewardDefinition> rewards, SaveCodecBudget budget)
            where TGrowths : IReadOnlyList<CandidateGrowthDefinition>
        {
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            return SaveCodecResult<PublishedRuleDefinitions>.Run(() =>
            {
                RuleContextChecks.BindingBudget(binding, budget);
                SaveCodecFailure.Require(inventory != null && progression != null, "MissingField", "Definitions");
                SaveEnvelopeCodec.CheckList(growths, budget, "Definitions.Growths");
                SaveCodecFailure.Require(growths.Count > 0, "MissingField", "Definitions.Growths");
                var classIds = new HashSet<string>(StringComparer.Ordinal);
                foreach (var growth in growths)
                {
                    SaveCodecFailure.Require(growth != null, "MissingField", "Definitions");
                    RuleContextChecks.Text(growth.ClassId, "Definitions.ClassId", budget, true);
                    SaveCodecFailure.Require(classIds.Add(growth.ClassId), "InconsistentBinding", "Definitions.ClassId");
                    CheckContext(binding, growth.Context, budget);
                }
                SaveEnvelopeCodec.CheckList(levels, budget, "Definitions.Levels");
                SaveEnvelopeCodec.CheckList(rewards, budget, "Definitions.Rewards");
                CheckContext(binding, inventory.Context, budget); CheckContext(binding, progression.Context, budget);
                SaveCodecFailure.Require(levels.Count == progression.Levels.Count && levels.Count > 0, "InconsistentBinding", "Definitions.Levels");
                var seen = new HashSet<string>(StringComparer.Ordinal);
                foreach (var level in levels)
                {
                    SaveCodecFailure.Require(level != null, "MissingField", "Definitions.Levels");
                    RuleContextChecks.Text(level.LevelId, "Definitions.LevelId", budget, true);
                    var version = BusinessFields.Take(ExactSaveValueCodec.DecodeInteger(level.LevelVersion, budget), "Definitions.LevelVersion");
                    SaveCodecFailure.Require(version.HasValue && version.Value.Sign > 0, "InvalidValue", "Definitions.LevelVersion");
                    SaveCodecFailure.Require(seen.Add(level.LevelId), "InconsistentBinding", "Definitions.LevelId");
                    var expected = ProgressionChecks.FindLevel(progression.Levels, level.LevelId);
                    SaveCodecFailure.Require(expected != null && expected.LevelVersion == level.LevelVersion, "InconsistentBinding", "Definitions.Progression");
                }
                var rewardIds = new HashSet<string>(StringComparer.Ordinal);
                foreach (var reward in rewards)
                {
                    SaveCodecFailure.Require(reward != null, "MissingField", "Definitions.Rewards");
                    CheckContext(binding, reward.Context, budget);
                    SaveCodecFailure.Require(rewardIds.Add(reward.RewardDefinitionId), "InconsistentBinding", "Definitions.RewardDefinitionId");
                    var level = ProgressionChecks.FindLevel(progression.Levels, reward.LevelId);
                    SaveCodecFailure.Require(level != null && level.LevelVersion == reward.LevelVersion, "InconsistentBinding", "Definitions.RewardLevel");
                }
                return new PublishedRuleDefinitions(binding, growths, inventory, progression, levels, rewards);
            });
        }

        public static SaveCodecResult<PublishedRuleDefinitions> PreparePermanent(PublishedRuleDefinitions basis,
            CandidatePermanentDefinitions permanent, SaveCodecBudget budget)
        {
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            return SaveCodecResult<PublishedRuleDefinitions>.Run(() =>
            {
                SaveCodecFailure.Require(basis != null && permanent != null, "MissingField", "Permanent.Definitions");
                permanent.CheckClosure(basis, budget);
                return new PublishedRuleDefinitions(basis.Binding, basis.Growths, basis.Inventory,
                    basis.Progression, basis.Levels, basis.Rewards, permanent);
            });
        }

        private static void CheckContext(ContentBinding binding, PreparedRuleContext context, SaveCodecBudget budget)
        {
            SaveCodecFailure.Require(context is PreparedPublishedRuleContext p && binding.Same(p.Binding), "InconsistentBinding", "Definitions.Context");
            BusinessFields.Take(RuleContextChecks.CheckBudget(context, budget), "Definitions.Context");
        }
    }

    public sealed class PublishedSaveContext
    {
        public IReadOnlyList<PublishedRuleDefinitions> Definitions { get; }
        private PublishedSaveContext(IReadOnlyList<PublishedRuleDefinitions> definitions)
        { Definitions = new List<PublishedRuleDefinitions>(definitions).AsReadOnly(); }

        public static SaveCodecResult<PublishedSaveContext> Prepare(IReadOnlyList<PublishedRuleDefinitions> definitions, SaveCodecBudget budget)
        {
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            return SaveCodecResult<PublishedSaveContext>.Run(() =>
            {
                SaveEnvelopeCodec.CheckList(definitions, budget, "Definitions");
                SaveCodecFailure.Require(definitions.Count > 0, "MissingField", "Definitions");
                for (var i = 0; i < definitions.Count; i++)
                {
                    var entry = definitions[i];
                    SaveCodecFailure.Require(entry != null, "MissingField", "Definitions");
                    RuleContextChecks.BindingBudget(entry.Binding, budget);
                    for (var j = 0; j < i; j++)
                        SaveCodecFailure.Require(!entry.Binding.Same(definitions[j].Binding), "InconsistentBinding", "Definitions.Binding");
                }
                return new PublishedSaveContext(definitions);
            });
        }

        public PublishedRuleDefinitions FindExact(ContentBinding binding)
        {
            if (binding == null) return null;
            foreach (var entry in Definitions) if (entry.Binding.Same(binding)) return entry;
            return null;
        }

        public PreparedLevel FindExact(DefinitionBinding binding)
        {
            var entry = FindExact(binding?.Content);
            if (entry == null) return null;
            foreach (var level in entry.Levels)
                if (level.LevelId == binding.LevelId && level.LevelVersion == binding.CanonicalLevelVersion) return level;
            return null;
        }
    }
}
