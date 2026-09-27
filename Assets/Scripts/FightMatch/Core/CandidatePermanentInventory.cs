using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using static FightMatch.Core.BusinessFields;

namespace FightMatch.Core
{
    public static class CandidatePermanentInventory
    {
        internal static List<CandidatePermanentPortion> Held(CandidateBusinessSnapshot business,
            IReadOnlyList<CandidateApplicationRecord> records, SaveCodecBudget budget, bool includeTerminated = false)
        {
            var state = business.Inventory;
            var held = new List<CandidatePermanentPortion>();
            foreach (var source in state.GetPermanentLedger().Sources)
            {
                CheckSource(source, state.PlayerId, budget);
                Need(source.Grant.Kind == CandidatePermanentSourceKind.ExistingAdvertisementHeld,
                    "Permanent.Source", "UnsupportedSourceProof");
                held.Add(new CandidatePermanentPortion(source, null, 0, source.ItemId,
                    source.Inclusion.UnitStart, source.Inclusion.UnitCount));
            }
            for (var index = 0; index < records.Count; index++)
            {
                var record = records[index];
                if (record.Intent.Kind != CandidateApplicationKind.SettleVictory ||
                    (index == records.Count - 1 && !record.Generation.HasValue && record.CommitId == null)) continue;
                foreach (var grant in state.OrdinaryGrants)
                {
                    if (grant.SettlementId != record.Result.SettlementId) continue;
                    Need(record.CommitId != null && grant.Context is PreparedPublishedRuleContext, "Permanent.OriginalCommit", "UnsupportedSourceProof");
                    var binding = ((PreparedPublishedRuleContext)grant.Context).Binding;
                    for (var line = 0; line < grant.Items.Count; line++)
                    {
                        var item = grant.Items[line];
                        if (item.Quantity.IsZero) continue;
                        var original = new CandidateOriginalGrantRef(CandidatePermanentSourceKind.OrdinaryBaseReward,
                            state.PlayerId, binding, grant.AttemptId, grant.SettlementId, null, null, null, null);
                        var source = new CandidatePermanentSourceLine(original, line, item.ItemId, item.Quantity,
                            index, record.OperationId, record.CommitId, null, null);
                        held.Add(new CandidatePermanentPortion(source, null, 0, item.ItemId, BigInteger.Zero, item.Quantity));
                    }
                }
            }
            foreach (var effect in state.GetPermanentLedger().Effects)
            {
                foreach (var input in effect.Quote.Inputs)
                {
                    Consume(held, input, budget);
                    if (includeTerminated) held.Add(new CandidatePermanentPortion(input.Source, input.OutputOperationId, input.OutputLine,
                        input.ItemId, input.UnitStart, input.UnitCount, effect.Quote.Kind == CandidatePermanentKind.Craft
                            ? CandidatePermanentEndpoint.Transformed : CandidatePermanentEndpoint.Consumed, effect.OperationId));
                }
                for (var line = 0; line < effect.Quote.Outputs.Count; line++)
                {
                    var item = effect.Quote.Outputs[line];
                    held.Add(new CandidatePermanentPortion(null, effect.OperationId, line, item.ItemId, BigInteger.Zero, item.Quantity));
                }
            }
            return held;
        }

        public static SaveCodecResult<IReadOnlyList<CandidatePermanentPortion>> ReadEndpoints(CandidateApplicationSnapshot basis, SaveCodecBudget budget)
        {
            return SaveCodecResult<IReadOnlyList<CandidatePermanentPortion>>.Run(() => Held(basis.Business, basis.Records, budget, true).AsReadOnly());
        }

        internal static void CheckSource(CandidatePermanentSourceLine source, string player, SaveCodecBudget budget)
        {
            Need(source != null && source.Grant != null && source.Grant.PlayerId == player, "Permanent.Source", "UnsupportedSourceProof");
            Need(source.RetractionOperationId == null, "Permanent.RetractionIndex", "UnsupportedSourceProof");
            var grant = source.Grant;
            new CandidatePermanentCodec(new BusinessFields(Stream.Null, false, budget) { StrictUnicode = true }).Source(source, "Permanent.Source");
            if (grant.Kind == CandidatePermanentSourceKind.OrdinaryBaseReward)
            {
                Need(grant.GrantId == null && grant.Purpose == null && grant.FactId == null && grant.UseId == null &&
                    source.Inclusion == null && source.OriginalBranchId == null, "Permanent.OrdinarySource");
                return;
            }
            Need(grant.Kind == CandidatePermanentSourceKind.ExistingAdvertisementHeld, "Permanent.Source.Kind", "UnsupportedSourceProof");
            Need(grant.AttemptId == null && grant.SettlementId == null, "Permanent.AdvertisementSource");
            var evidence = source.Inclusion;
            Need(evidence != null && evidence.Grant != null &&
                CandidatePermanentCodec.SameGrant(grant, evidence.Grant, budget), "Permanent.Inclusion", "UnsupportedSourceProof");
            Need(source.OriginalBranchId == evidence.BranchId && evidence.GrantLine == source.GrantLine &&
                evidence.Disposition == CandidateInclusionDisposition.SelectedHolding && evidence.EndpointOperationId == null,
                "Permanent.Inclusion.Endpoint", "UnsupportedSourceProof");
            Range(evidence.UnitStart, evidence.UnitCount, source.OriginalQuantity, budget);
        }

        internal static IReadOnlyList<CandidatePermanentPortion> Select(CandidateApplicationSnapshot basis,
            IReadOnlyList<CandidateInventoryQuantity> costs, CandidatePermanentDraft draft,
            Func<CandidatePermanentPortion, bool> allowed, SaveCodecBudget budget)
        {
            var held = Held(basis.Business, basis.Records, budget);
            var selected = new List<CandidatePermanentPortion>();
            var explicitInputs = draft.SelectedInputs;
            var selectedCount = 0;
            if (explicitInputs != null) SaveEnvelopeCodec.CheckList(explicitInputs, budget, "Permanent.SelectedInputs");
            foreach (var cost in costs)
            {
                var view = CandidateInventory.Read(basis.Business.Inventory, budget.Math);
                var free = BigInteger.Zero;
                foreach (var item in view.Items) if (item.ItemId == cost.ItemId) free = item.F;
                Need(free >= cost.Quantity, "Permanent.Cost", "InsufficientResources");
                // Legacy battle consumption has aggregate identity only; it cannot select a surviving source unit.
                foreach (var end in basis.Business.Inventory.Ends)
                    foreach (var row in end.OriginalCarry.Rows)
                        if (end.Kind == CandidateInventoryEndKind.NormalVictory && row.ItemId == cost.ItemId)
                            foreach (var remaining in end.Remaining)
                                Need(remaining.CharacterId != row.Actor.CharacterId || remaining.ItemId != row.ItemId || remaining.U == row.C,
                                    "Permanent.LegacyConsumption", "ReconciliationRequired");
                var available = CandidatePermanentProgression.PreferTeachingCertificate(
                    held.FindAll(x => x.ItemId == cost.ItemId && allowed(x)), basis.Business, draft);
                var preferred = available;
                if (explicitInputs != null) available = new List<CandidatePermanentPortion>(explicitInputs).FindAll(x => x != null && x.ItemId == cost.ItemId);
                else if (draft.Kind != CandidatePermanentKind.UseExperienceCards)
                    Need(available.Count <= 1 || available.Aggregate(BigInteger.Zero,
                        (sum, x) => budget.Math.Add(sum, x.UnitCount)) == cost.Quantity, "Permanent.SelectedInputs", "CostSelectionRequired");
                if (draft.Kind == CandidatePermanentKind.UseExperienceCards) available = available
                    .OrderBy(x => x.Source?.Grant.Kind == CandidatePermanentSourceKind.ExistingAdvertisementHeld ? 0 : 1)
                    .ThenBy(x => Acquired(basis, x)).ThenBy(x => x, Comparer<CandidatePermanentPortion>.Create(Compare)).ToList();
                else available.Sort(Compare);
                var needed = cost.Quantity;
                foreach (var portion in available)
                {
                    if (needed.IsZero) break;
                    Need(allowed(portion), "Permanent.Source.Purpose", "UnsupportedSourceProof");
                    if (explicitInputs != null && draft.Kind == CandidatePermanentKind.LearnSkill)
                        Need(preferred.Any(x => CandidatePermanentCodec.SameRoot(x, portion, budget)),
                            "Permanent.SelectedInputs.Priority");
                    if (draft.Kind == CandidatePermanentKind.UseExperienceCards && portion.OutputOperationId != null)
                        CheckOrdinaryOutput(basis.Business.Inventory, portion.OutputOperationId, new HashSet<string>(), budget);
                    var count = explicitInputs == null && portion.UnitCount > needed ? needed : portion.UnitCount;
                    Need(count <= needed, "Permanent.SelectedInputs");
                    var chosen = portion.Slice(portion.UnitStart, count);
                    selectedCount++;
                    Consume(held, chosen, budget);
                    var prior = selected.Count == 0 ? null : selected[selected.Count - 1];
                    if (prior != null && CandidatePermanentCodec.SameRoot(prior, chosen, budget) &&
                        budget.Math.Add(prior.UnitStart, prior.UnitCount) == chosen.UnitStart)
                        selected[selected.Count - 1] = prior.Slice(prior.UnitStart, budget.Math.Add(prior.UnitCount, chosen.UnitCount));
                    else selected.Add(chosen);
                    needed = budget.Math.Subtract(needed, count);
                }
                Need(needed.IsZero, "Permanent.Source", CandidatePermanentProgression.MissingSourceCode(held, cost.ItemId, allowed));
            }
            Need(explicitInputs == null || selectedCount == explicitInputs.Count, "Permanent.SelectedInputs");
            selected.Sort(Compare);
            if (explicitInputs != null && draft.Kind == CandidatePermanentKind.UseExperienceCards)
            {
                var canonical = Select(basis, costs, new CandidatePermanentDraft { Kind = draft.Kind,
                    CharacterId = draft.CharacterId, DefinitionId = draft.DefinitionId, Quantity = draft.Quantity }, allowed, budget);
                Need(canonical.Count == selected.Count, "Permanent.SelectedInputs.Priority");
                for (var i = 0; i < selected.Count; i++)
                    Need(CandidatePermanentCodec.SameRoot(canonical[i], selected[i], budget) &&
                        canonical[i].UnitStart == selected[i].UnitStart && canonical[i].UnitCount == selected[i].UnitCount,
                        "Permanent.SelectedInputs.Priority");
            }
            return selected.AsReadOnly();
        }

        private static BigInteger Acquired(CandidateApplicationSnapshot basis, CandidatePermanentPortion portion)
        {
            if (portion.Source != null) return portion.Source.AcquisitionOrder;
            for (var i = 0; i < basis.Records.Count; i++)
                if (basis.Records[i].OperationId == portion.OutputOperationId) return i;
            Need(false, "Permanent.Output.Acquisition", "UnsupportedSourceProof");
            return BigInteger.MinusOne;
        }

        internal static int Compare(CandidatePermanentPortion a, CandidatePermanentPortion b)
        {
            var ak = a.Source?.Grant.Kind == CandidatePermanentSourceKind.ExistingAdvertisementHeld ? 0 : 1;
            var bk = b.Source?.Grant.Kind == CandidatePermanentSourceKind.ExistingAdvertisementHeld ? 0 : 1;
            var result = ak.CompareTo(bk);
            if (result == 0) result = (a.Source?.AcquisitionOrder ?? BigInteger.Zero).CompareTo(b.Source?.AcquisitionOrder ?? BigInteger.Zero);
            if (result == 0) result = StringComparer.Ordinal.Compare(a.Source?.OriginalOperationId ?? a.OutputOperationId, b.Source?.OriginalOperationId ?? b.OutputOperationId);
            if (result == 0) result = (a.Source?.GrantLine ?? a.OutputLine).CompareTo(b.Source?.GrantLine ?? b.OutputLine);
            return result == 0 ? a.UnitStart.CompareTo(b.UnitStart) : result;
        }

        internal static void Range(BigInteger start, BigInteger count, BigInteger total, SaveCodecBudget budget)
        {
            CandidatePermanentDefinitions.Number(start, budget, false);
            CandidatePermanentDefinitions.Number(count, budget);
            Need(budget.Math.Add(start, count) <= total, "Permanent.Source.Range");
        }

        internal static void Consume(List<CandidatePermanentPortion> held, CandidatePermanentPortion input, SaveCodecBudget budget)
        {
            Need(input != null && (input.Source == null) != (input.OutputOperationId == null), "Permanent.Input", "UnsupportedSourceProof");
            Need(input.Endpoint == CandidatePermanentEndpoint.Held && input.EndpointOperationId == null, "Permanent.Input.Endpoint");
            CandidatePermanentDefinitions.Number(input.UnitStart, budget, false);
            CandidatePermanentDefinitions.Number(input.UnitCount, budget);
            var end = budget.Math.Add(input.UnitStart, input.UnitCount);
            for (var index = 0; index < held.Count; index++)
            {
                var item = held[index];
                if (item.Endpoint != CandidatePermanentEndpoint.Held) continue;
                if (!CandidatePermanentCodec.SameRoot(item, input, budget)) continue;
                var heldEnd = budget.Math.Add(item.UnitStart, item.UnitCount);
                if (input.UnitStart < item.UnitStart || end > heldEnd) continue;
                held.RemoveAt(index);
                if (input.UnitStart > item.UnitStart) held.Add(item.Slice(item.UnitStart, budget.Math.Subtract(input.UnitStart, item.UnitStart)));
                if (end < heldEnd) held.Add(item.Slice(end, budget.Math.Subtract(heldEnd, end)));
                return;
            }
            Need(false, "Permanent.Input.Endpoint", "SourceAlreadyConsumedOrUnproven");
        }

        internal static void CheckOrdinaryOutput(CandidateInventoryState state, string operation, HashSet<string> chain, SaveCodecBudget budget)
        {
            Need(chain.Add(operation), "Permanent.Output.Cycle");
            CandidatePermanentEffect found = null;
            foreach (var effect in state.GetPermanentLedger().Effects) if (effect.OperationId == operation) found = effect;
            Need(found != null, "Permanent.Output", "UnsupportedSourceProof");
            foreach (var input in found.Quote.Inputs)
            {
                Need(input.Source?.Grant.Kind != CandidatePermanentSourceKind.ExistingAdvertisementHeld,
                    "Permanent.MixedOutput", "UnsupportedSourceProof");
                if (input.OutputOperationId != null) CheckOrdinaryOutput(state, input.OutputOperationId, chain, budget);
            }
            chain.Remove(operation);
        }

        internal static bool CheckLoadout(CandidateBusinessSnapshot business, CandidatePermanentDraft draft, SaveCodecBudget budget)
        {
            var state = business.Inventory;
            CandidateInventoryResult result;
            if (draft.Kind == CandidatePermanentKind.Equip)
            {
                Need(business.ActiveHistory == null && business.Progression.ActiveAttempt == null,
                    "Permanent.Equip", "AttemptActive");
                result = CandidateInventory.Equip(state, new CandidateEquipIntent { CharacterId = draft.CharacterId,
                    ItemId = draft.DefinitionId, L = draft.Quantity }, state.StateRevision, budget.Math);
            }
            else
            {
                Need(draft.PreferenceRevision == state.PreferenceRevision, "Permanent.PreferenceRevision", "StaleContext");
                result = CandidateInventory.SetPreference(state, new CandidateInventoryPreferenceIntent { CharacterId = draft.CharacterId,
                    ItemId = draft.DefinitionId, Enabled = draft.Enabled }, state.PreferenceRevision, budget.Math);
            }
            Need(result.IsAccepted, result.FieldPath ?? "Permanent.Loadout", result.RejectionCode.ToString());
            return !ReferenceEquals(result.Next, state);
        }

        internal static CandidateInventoryState Apply(CandidateApplicationSnapshot basis, CandidatePermanentEffect effect, SaveCodecBudget budget)
        {
            var state = basis.Business.Inventory;
            var quote = effect.Quote;
            Need(state.StateRevision == quote.InventoryRevision && state.PreferenceRevision == quote.PreferenceRevision,
                "Permanent.InventoryRevision", "StaleContext");
            if (effect.Outcome != "Applied") return state;
            var held = Held(basis.Business, basis.Records, budget);
            foreach (var portion in quote.Inputs) Consume(held, portion, budget);
            var next = state;
            if (quote.Kind == CandidatePermanentKind.Equip || quote.Kind == CandidatePermanentKind.SetPreference)
            {
                var result = quote.Kind == CandidatePermanentKind.Equip
                    ? CandidateInventory.Equip(state, new CandidateEquipIntent { CharacterId = quote.CharacterId,
                        ItemId = quote.DefinitionId, L = quote.Quantity }, quote.InventoryRevision, budget.Math)
                    : CandidateInventory.SetPreference(state, new CandidateInventoryPreferenceIntent { CharacterId = quote.CharacterId,
                        ItemId = quote.DefinitionId, Enabled = quote.Enabled }, quote.PreferenceRevision, budget.Math);
                Need(result.IsAccepted, result.FieldPath ?? "Permanent.Inventory", result.RejectionCode.ToString());
                next = result.Next;
            }
            else
            {
                var values = new List<CandidateInventoryHolding>();
                foreach (var holding in state.Holdings)
                {
                    var total = holding.T;
                    foreach (var cost in quote.Costs) if (cost.ItemId == holding.ItemId) total = budget.Math.Subtract(total, cost.Quantity);
                    foreach (var output in quote.Outputs) if (output.ItemId == holding.ItemId) total = budget.Math.Add(total, output.Quantity);
                    Need(total.Sign >= 0, "Permanent.Inventory.Total");
                    values.Add(new CandidateInventoryHolding(holding.ItemId, total));
                }
                if (quote.Costs.Count != 0 || quote.Outputs.Count != 0)
                    next = new CandidateInventoryState(state.Definition, state.PlayerId, values, state.Loadouts, state.ActiveCarry,
                        state.OrdinaryGrants, state.Ends, budget.Math.Add(state.StateRevision, BigInteger.One),
                        state.PreferenceRevision, true, state.GetPermanentLedger());
            }
            if (ReferenceEquals(next, state) && quote.Costs.Count == 0 && quote.Outputs.Count == 0) return state;
            var effects = new List<CandidatePermanentEffect>(state.GetPermanentLedger().Effects) { effect };
            return new CandidateInventoryState(next.Definition, next.PlayerId, next.Holdings, next.Loadouts, next.ActiveCarry,
                next.OrdinaryGrants, next.Ends, next.StateRevision, next.PreferenceRevision, true,
                new CandidatePermanentInventoryLedger(state.GetPermanentLedger().Sources, effects));
        }
    }
}
