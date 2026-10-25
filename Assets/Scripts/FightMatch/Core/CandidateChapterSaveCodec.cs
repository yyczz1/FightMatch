using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static FightMatch.Core.BusinessFields;

namespace FightMatch.Core
{
    internal sealed class CandidateChapterSaveCodec
    {
        private readonly BusinessFields f;
        internal CandidateChapterSaveCodec(BusinessFields fields) { f = fields; }

        internal IReadOnlyList<CandidateChapterMilestoneEffect> Effects(IReadOnlyList<CandidateChapterMilestoneEffect> effects,
            IReadOnlyList<CandidateProgressionFirstClear> clears, string path)
        {
            return f.List(effects, (effect, p) =>
            {
                f.Required(effect, p);
                var kind = (CandidateChapterMilestoneKind)f.Enum((int)(effect?.Kind ?? 0), 1, 2, p + ".Kind");
                var source = f.Text(effect?.SourceClear.End.EndReceiptId, p + ".SourceClear");
                var clear = CandidatePermanentSaveCodec.Find(clears, x => x.End.EndReceiptId == source, p + ".SourceClear");
                if (!f.Reading) Need(ReferenceEquals(clear, effect.SourceClear), p + ".SourceClear", "ReceiptConflict");
                var operation = f.Text(effect?.OperationId, p + ".OperationId");
                if (kind == CandidateChapterMilestoneKind.AcquireClass)
                {
                    var character = f.Text(effect?.CharacterId, p + ".CharacterId");
                    var classId = f.Text(effect?.ClassId, p + ".ClassId");
                    var level = f.Integer(effect?.InitialLevel ?? 0, p + ".InitialLevel", 1);
                    return f.Reading ? new CandidateChapterMilestoneEffect(kind, clear, operation, character, classId, level) : effect;
                }
                var item = f.Text(effect?.ItemId, p + ".ItemId");
                var amount = f.Integer(effect?.Amount ?? 0, p + ".Amount", 1);
                return f.Reading ? new CandidateChapterMilestoneEffect(kind, clear, operation, itemId: item, amount: amount) : effect;
            }, path).AsReadOnly();
        }

        internal static void Check(CandidateBusinessSnapshot state, PublishedSaveContext resolved, SaveCodecBudget budget)
        {
            var effects = state.Progression.GetChapterEffects();
            SaveEnvelopeCodec.CheckList(effects, budget, "Chapter.Effects");
            if (state.Format != CandidateBusinessFormat.PublishedChapterV5)
            {
                Need(effects.Count == 0, "Chapter.Legacy", "UnsupportedSchema");
                return;
            }
            var businessKeys = new HashSet<string>(StringComparer.Ordinal);
            foreach (var effect in effects)
            {
                Need(effect != null && effect.SourceClear != null && state.Progression.FirstClears.Contains(effect.SourceClear) &&
                    effect.PlayerId == state.PlayerId && effect.SourceClear.End.IsFirstClear && effect.Binding != null &&
                    effect.Binding.RuleVersion == ChapterRuleDefinitions.RuleVersion, "Chapter.SourceClear", "ReceiptConflict");
                RuleContextChecks.Text(effect.OperationId, "Chapter.OperationId", budget, true);
                var reward = state.Rewards.BaseRewards.SingleOrDefault(x => x.SettlementId == effect.SettlementId);
                Need(reward != null && ReferenceEquals(reward.Ending, effect.SourceClear.End), "Chapter.Settlement", "ReceiptConflict");
                if (effect.Kind == CandidateChapterMilestoneKind.AcquireClass)
                {
                    RuleContextChecks.Text(effect.ClassId, "Chapter.ClassId", budget, true);
                    RuleContextChecks.Text(effect.CharacterId, "Chapter.CharacterId", budget, true);
                    var character = state.Roster.Find(effect.CharacterId);
                    Need(businessKeys.Add("class:" + effect.ClassId) && character != null && character.ClassId == effect.ClassId &&
                        character.Definition.ClassKind == CharacterClassKind.Mage && effect.ItemId == null && effect.Amount == null &&
                        effect.InitialLevel == reward.Report.Level.RecommendedLevel && character.OriginalSlot == 0 &&
                        character.Definition.Context is PreparedPublishedRuleContext context && context.Binding.Same(effect.Binding) &&
                        !reward.Report.Baseline.Entry.ReadyParticipants.Any(x => x.CharacterId == effect.CharacterId),
                        "Chapter.AcquireClass", "ReceiptConflict");
                }
                else
                {
                    RuleContextChecks.Text(effect.ItemId, "Chapter.ItemId", budget, true);
                    Need(effect.Kind == CandidateChapterMilestoneKind.FirstClearItem &&
                        businessKeys.Add("item:" + effect.LevelId.Length + ":" + effect.LevelId + effect.ItemId) &&
                        effect.CharacterId == null && effect.ClassId == null && effect.InitialLevel == null && effect.Amount > 0 &&
                        InventoryChecks.FindItem(state.Inventory.Definition, effect.ItemId)?.Kind == CandidateInventoryItemKind.OrdinaryMaterial,
                        "Chapter.FirstClearItem", "ReceiptConflict");
                }
            }
            foreach (var clear in state.Progression.FirstClears)
            {
                var actual = effects.Where(x => ReferenceEquals(x.SourceClear, clear)).ToList();
                if (clear.Context.RuleVersion != ChapterRuleDefinitions.RuleVersion)
                { Need(actual.Count == 0, "Chapter.LegacyClear", "UnsupportedBinding"); continue; }
                if (resolved == null) continue;
                var definitions = resolved.FindExact(((PreparedPublishedRuleContext)clear.Context).Binding);
                Need(definitions?.Chapter != null, "Chapter.Definitions", "UnsupportedBinding");
                var unlocks = definitions.Chapter.CharacterUnlocks.Where(x => x.AfterLevelId == clear.LevelId).ToList();
                var items = definitions.Chapter.FirstClearItems.Where(x => x.LevelId == clear.LevelId).ToList();
                Need(actual.Count == unlocks.Count + items.Count, "Chapter.EffectCoverage", "IncompleteOperationHistory");
                foreach (var unlock in unlocks)
                    Need(actual.Any(x => x.Kind == CandidateChapterMilestoneKind.AcquireClass && x.ClassId == unlock.ClassId),
                        "Chapter.AcquireClass.Definition", "InconsistentBinding");
                foreach (var item in items)
                    Need(actual.Any(x => x.Kind == CandidateChapterMilestoneKind.FirstClearItem && x.ItemId == item.ItemId && x.Amount == item.Amount),
                        "Chapter.FirstClearItem.Definition", "InconsistentBinding");
            }
        }

        internal static IReadOnlyList<CandidateInventoryQuantity> RewardVector(CandidateBusinessSnapshot state,
            CandidateFixedBaseReward reward, ExactMathBudget math)
        {
            var totals = reward.Materials.ToDictionary(x => x.ItemId, x => x.Amount, StringComparer.Ordinal);
            foreach (var effect in state.Progression.GetChapterEffects())
                if (effect.Kind == CandidateChapterMilestoneKind.FirstClearItem && effect.SettlementId == reward.SettlementId)
                    totals[effect.ItemId] = math.Add(totals.TryGetValue(effect.ItemId, out var old) ? old : BigInteger.Zero, effect.Amount.Value);
            return totals.OrderBy(x => x.Key, StringComparer.Ordinal).Select(x => new CandidateInventoryQuantity(x.Key, x.Value)).ToList();
        }

        internal static void RejectUnmappedMigration(CandidateApplicationSnapshot basis, PreparedCandidateApplicationIntent intent)
        {
            Need(basis != null && basis.Business.Format == CandidateBusinessFormat.PublishedPermanentV4,
                "Chapter.Migration.Source", "UnsupportedSchema");
            Need(basis.Business.ActiveHistory == null && basis.Continuation == null && basis.Business.Progression.ActiveAttempt == null,
                "Chapter.Migration.Source", "ActiveAttemptConflict");
            var source = intent.GetChapterMigration();
            Need(source != null && intent.ExpectedCommitId == basis.Header.CommitId && source.SourceGeneration == basis.Header.SaveGeneration &&
                source.SourceDescriptorLength == basis.Descriptor.TotalLength &&
                CandidateApplicationIntentCodec.SameBytes(source.SourceDescriptorSha256, basis.Descriptor.Sha256),
                "Chapter.Migration.SourceDescriptor", "StaleContext");
            Need(intent.Context is PreparedPublishedRuleContext target && target.Binding.RuleVersion == ChapterRuleDefinitions.RuleVersion,
                "Chapter.Migration.Target", "UnsupportedBinding");
            // Rebinding historical owner definitions needs an explicit verified mapping. Never fabricate one from latest content.
            throw new SaveCodecFailure("MigrationMappingRequired", "Chapter.Migration.DefinitionMapping");
        }
    }
}
