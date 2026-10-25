using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static FightMatch.Core.BusinessFields;

namespace FightMatch.Core
{
    public enum CandidateChapterMilestoneKind { AcquireClass = 1, FirstClearItem = 2 }

    public sealed class CandidateChapterMilestoneEffect
    {
        public CandidateChapterMilestoneKind Kind { get; }
        public CandidateProgressionFirstClear SourceClear { get; }
        public string OperationId { get; }
        public string CharacterId { get; }
        public string ClassId { get; }
        public BigInteger? InitialLevel { get; }
        public string ItemId { get; }
        public BigInteger? Amount { get; }
        public string PlayerId => SourceClear.PlayerId;
        public string LevelId => SourceClear.LevelId;
        public string SettlementId => SourceClear.End.SettlementId;
        public ContentBinding Binding => (SourceClear.Context as PreparedPublishedRuleContext)?.Binding;
        internal CandidateChapterMilestoneEffect(CandidateChapterMilestoneKind kind, CandidateProgressionFirstClear sourceClear,
            string operationId, string characterId = null, string classId = null, BigInteger? initialLevel = null,
            string itemId = null, BigInteger? amount = null)
        {
            Kind = kind; SourceClear = sourceClear; OperationId = operationId; CharacterId = characterId;
            ClassId = classId; InitialLevel = initialLevel; ItemId = itemId; Amount = amount;
        }
    }

    public sealed class CandidateChapterVictory
    {
        public CandidateRosterState Roster { get; }
        public CandidateInventoryState Inventory { get; }
        public CandidateProgressionState Progression { get; }
        public IReadOnlyList<CandidateChapterMilestoneEffect> AddedEffects { get; }
        internal CandidateChapterVictory(CandidateRosterState roster, CandidateInventoryState inventory,
            CandidateProgressionState progression, IReadOnlyList<CandidateChapterMilestoneEffect> added)
        { Roster = roster; Inventory = inventory; Progression = progression; AddedEffects = new List<CandidateChapterMilestoneEffect>(added).AsReadOnly(); }
    }

    public static class CandidateChapterMilestones
    {
        // Compose after the original participants' XP and end receipts have been fixed.
        // This method only builds a candidate; the existing application commit owns persistence.
        public static SaveCodecResult<CandidateChapterVictory> ApplyVictory(CandidateBusinessSnapshot before,
            CandidateRosterState roster, CandidateInventoryState inventory, CandidateProgressionState progression,
            CandidateFixedBaseReward reward, PublishedRuleDefinitions definitions, string operationId,
            string acquiredCharacterId, SaveCodecBudget budget)
        {
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            return SaveCodecResult<CandidateChapterVictory>.Run(() =>
            {
                Need(before != null && roster != null && inventory != null && progression != null && reward != null && definitions?.Chapter != null,
                    "Chapter.Victory", "MissingField");
                Need(before.Format == CandidateBusinessFormat.PublishedChapterV5 &&
                    reward.Definition.Context is PreparedPublishedRuleContext context && context.Binding.Same(definitions.Binding) &&
                    reward.Ending.Kind == CandidateProgressionEndKind.NormalVictory, "Chapter.Victory.Binding", "UnsupportedBinding");
                RuleContextChecks.Text(operationId, "Chapter.OperationId", budget, true);
                var added = new List<CandidateChapterMilestoneEffect>();
                if (!reward.Ending.IsFirstClear) return new CandidateChapterVictory(roster, inventory, progression, added);
                var clear = progression.FirstClears.SingleOrDefault(x => ReferenceEquals(x.End, reward.Ending));
                Need(clear != null && !before.Progression.FirstClears.Any(x => x.LevelId == clear.LevelId), "Chapter.FirstClear", "ReceiptConflict");
                var characters = new List<CandidateCharacterState>(roster.Characters);
                var loadouts = new List<CandidateInventoryLoadout>(inventory.Loadouts);
                foreach (var unlock in definitions.Chapter.CharacterUnlocks.Where(x => x.AfterLevelId == clear.LevelId))
                {
                    Need(!characters.Any(x => x.ClassId == unlock.ClassId) &&
                        !before.Progression.GetChapterEffects().Any(x => x.Kind == CandidateChapterMilestoneKind.AcquireClass && x.ClassId == unlock.ClassId),
                        "Chapter.AcquireClass", "AcquisitionEvidenceConflict");
                    RuleContextChecks.Text(acquiredCharacterId, "Chapter.CharacterId", budget, true);
                    Need(!characters.Any(x => x.CharacterId == acquiredCharacterId), "Chapter.CharacterId", "ReceiptConflict");
                    var growth = definitions.FindGrowth(unlock.ClassId);
                    Need(growth != null && growth.ClassKind == CharacterClassKind.Mage, "Chapter.Growth", "UnsupportedBinding");
                    var created = CandidateCharacterGrowth.CreateCandidate(growth, before.PlayerId, acquiredCharacterId,
                        reward.Report.Level.RecommendedLevel, BigInteger.Zero, 0, budget.Math);
                    Need(created.IsAccepted, "Chapter.Character." + created.FieldPath, created.RejectionCode.ToString());
                    characters.Add(created.Next);
                    loadouts.Add(new CandidateInventoryLoadout(new CandidateInventoryActor(new CandidateInventoryActorInput {
                        CharacterId = acquiredCharacterId, ClassId = unlock.ClassId, ClassKind = CharacterClassKind.Mage, OriginalSlot = 0 }), null, BigInteger.Zero, null));
                    added.Add(new CandidateChapterMilestoneEffect(CandidateChapterMilestoneKind.AcquireClass, clear, operationId,
                        acquiredCharacterId, unlock.ClassId, reward.Report.Level.RecommendedLevel));
                }
                var extras = new Dictionary<string, BigInteger>(StringComparer.Ordinal);
                foreach (var item in definitions.Chapter.FirstClearItems.Where(x => x.LevelId == clear.LevelId))
                {
                    Need(!before.Progression.GetChapterEffects().Any(x => x.Kind == CandidateChapterMilestoneKind.FirstClearItem &&
                        x.LevelId == clear.LevelId && x.ItemId == item.ItemId), "Chapter.FirstClearItem", "ReceiptConflict");
                    extras.Add(item.ItemId, item.Amount);
                    added.Add(new CandidateChapterMilestoneEffect(CandidateChapterMilestoneKind.FirstClearItem, clear, operationId,
                        itemId: item.ItemId, amount: item.Amount));
                }
                characters.Sort((a, b) => StringComparer.Ordinal.Compare(a.CharacterId, b.CharacterId));
                loadouts.Sort((a, b) => StringComparer.Ordinal.Compare(a.Actor.CharacterId, b.Actor.CharacterId));
                roster = new CandidateRosterState(roster.PlayerId, characters, roster.Formation, roster.FormationRevision,
                    roster.FormationReceipts, roster.GetPermanentEffects());
                var grants = new List<CandidateOrdinaryGrantReceipt>(inventory.OrdinaryGrants);
                var ends = new List<CandidateInventoryEndReceipt>(inventory.Ends);
                var holdings = new List<CandidateInventoryHolding>(inventory.Holdings);
                if (extras.Count > 0)
                {
                    var end = ends.SingleOrDefault(x => x.SettlementId == reward.SettlementId);
                    Need(end != null && end.RewardReceipt != null, "Chapter.InventoryEnd", "ReceiptConflict");
                    var items = end.Rewards.ToDictionary(x => x.ItemId, x => x.Quantity, StringComparer.Ordinal);
                    foreach (var item in extras)
                    {
                        Need(InventoryChecks.FindItem(inventory.Definition, item.Key)?.Kind == CandidateInventoryItemKind.OrdinaryMaterial,
                            "Chapter.FirstClearItem.Kind", "UnsupportedBinding");
                        items[item.Key] = budget.Math.Add(items.TryGetValue(item.Key, out var old) ? old : BigInteger.Zero, item.Value);
                        var index = holdings.FindIndex(x => x.ItemId == item.Key);
                        Need(index >= 0, "Chapter.FirstClearItem.Holding", "InconsistentBinding");
                        holdings[index] = new CandidateInventoryHolding(item.Key, budget.Math.Add(holdings[index].T, item.Value));
                    }
                    var vector = items.OrderBy(x => x.Key, StringComparer.Ordinal).Select(x => new CandidateInventoryQuantity(x.Key, x.Value)).ToList();
                    var grant = new CandidateOrdinaryGrantReceipt(inventory.PlayerId, reward.AttemptId, reward.SettlementId,
                        end.RewardReceipt.Context, vector);
                    var indexOfGrant = grants.IndexOf(end.RewardReceipt);
                    Need(indexOfGrant >= 0, "Chapter.InventoryGrant", "ReceiptConflict"); grants[indexOfGrant] = grant;
                    ends[ends.IndexOf(end)] = new CandidateInventoryEndReceipt(end.OriginalCarry, new CandidateInventoryEndIntent {
                        EndReceiptId = end.EndReceiptId, Kind = end.Kind, SettlementId = end.SettlementId, NewAttemptId = end.NewAttemptId },
                        end.Remaining, vector, grant);
                }
                inventory = new CandidateInventoryState(inventory.Definition, inventory.PlayerId, holdings, loadouts, inventory.ActiveCarry,
                    grants, ends, inventory.StateRevision, inventory.PreferenceRevision, true, inventory.GetPermanentLedger());
                var effects = new List<CandidateChapterMilestoneEffect>(progression.GetChapterEffects()); effects.AddRange(added);
                progression = new CandidateProgressionState(progression.PlayerId, progression.Definition, progression.StateRevision,
                    progression.OpenFacts, progression.FirstClears, progression.Challenges, progression.GetPermanentEffects(), effects);
                return new CandidateChapterVictory(roster, inventory, progression, added);
            });
        }
    }
}
