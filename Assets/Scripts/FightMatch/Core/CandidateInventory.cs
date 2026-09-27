using System;
using System.Collections.Generic;
using System.Numerics;
using static FightMatch.Core.CandidateInventoryRejectionCode;
using static FightMatch.Core.InventoryChecks;

namespace FightMatch.Core
{
    public static class CandidateInventory
    {
        public static CandidateInventoryDefinitionResult PrepareDefinition(CandidateInventoryDefinitionInput input, ExactMathBudget budget)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            var c = new InventoryChecks(budget);
            if (!c.Context(input.Context, "Context")) return new CandidateInventoryDefinitionResult(c);
            if (input.Items == null) { c.Fail(MissingField, "Items"); return new CandidateInventoryDefinitionResult(c); }
            var items = new List<CandidateInventoryItem>(); var seen = new HashSet<string>(StringComparer.Ordinal);
            for (var i = 0; i < input.Items.Count; i++)
            {
                var item = input.Items[i]; var p = $"Items[{i}]";
                if (item == null) { c.Fail(MissingField, p); return new CandidateInventoryDefinitionResult(c); }
                if (!c.Text(item.ItemId, p + ".ItemId")) return new CandidateInventoryDefinitionResult(c);
                if (!seen.Add(item.ItemId)) { c.Fail(InvalidValue, p + ".ItemId"); return new CandidateInventoryDefinitionResult(c); }
                if (item.Kind == CandidateInventoryItemKind.Unspecified) { c.Fail(MissingField, p + ".Kind"); return new CandidateInventoryDefinitionResult(c); }
                if (item.Kind == CandidateInventoryItemKind.OrdinaryTactical)
                { if (!c.Text(item.EquipClassId, p + ".EquipClassId")) return new CandidateInventoryDefinitionResult(c); }
                else if (item.Kind == CandidateInventoryItemKind.OrdinaryMaterial)
                { if (item.EquipClassId != null) { c.Fail(InvalidValue, p + ".EquipClassId"); return new CandidateInventoryDefinitionResult(c); } }
                else { c.Fail(UnsupportedBinding, p + ".Kind"); return new CandidateInventoryDefinitionResult(c); }
                items.Add(new CandidateInventoryItem(item));
            }
            items.Sort((a, b) => StringComparer.Ordinal.Compare(a.ItemId, b.ItemId));
            return new CandidateInventoryDefinitionResult(new CandidateInventoryDefinition(input.Context, items));
        }

        public static CandidateInventoryResult CreateCandidate(CandidateInventoryDefinition definition, string playerId,
            List<CandidateInventoryActorInput> actors, ExactMathBudget budget)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            if (actors == null) throw new ArgumentNullException(nameof(actors));
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            var c = new InventoryChecks(budget); RuleContextChecks.CheckBudget(definition.Context, budget);
            if (!c.Text(playerId, "PlayerId")) return new CandidateInventoryResult(c);
            if (actors.Count != 1) return Reject(c, actors.Count == 0 ? InvalidValue : UnsupportedBinding, "Actors");
            if (!c.Actor(actors[0], "Actors[0]")) return new CandidateInventoryResult(c);
            budget.CheckInteger(BigInteger.Zero); budget.CheckInteger(BigInteger.One);
            var actor = new CandidateInventoryActor(actors[0]); var holdings = new List<CandidateInventoryHolding>();
            foreach (var item in definition.Items) holdings.Add(new CandidateInventoryHolding(item.ItemId, BigInteger.Zero));
            return new CandidateInventoryResult(new CandidateInventoryState(definition, playerId, actor, holdings,
                new CandidateInventoryLoadout(actor, null, BigInteger.Zero, null), null, new CandidateOrdinaryGrantReceipt[0],
                new CandidateInventoryEndReceipt[0], BigInteger.One, BigInteger.One), CandidateInventoryOutcome.Changed);
        }

        public static SaveCodecResult<CandidateInventoryState> CreateCandidate(CandidateInventoryDefinition definition,
            CandidateRosterState roster, SaveCodecBudget budget)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            if (roster == null) throw new ArgumentNullException(nameof(roster));
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            return SaveCodecResult<CandidateInventoryState>.Run(() =>
            {
                roster.Check(budget); RuleContextChecks.CheckBudget(definition.Context, budget.Math);
                var loadouts = new List<CandidateInventoryLoadout>();
                foreach (var character in roster.Characters)
                {
                    var actor = new CandidateInventoryActor(new CandidateInventoryActorInput { CharacterId = character.CharacterId,
                        ClassId = character.ClassId, ClassKind = character.Definition.ClassKind, OriginalSlot = character.OriginalSlot });
                    loadouts.Add(new CandidateInventoryLoadout(actor, null, BigInteger.Zero, null));
                }
                var holdings = new List<CandidateInventoryHolding>();
                foreach (var item in definition.Items) holdings.Add(new CandidateInventoryHolding(item.ItemId, BigInteger.Zero));
                return new CandidateInventoryState(definition, roster.PlayerId, holdings, loadouts, null,
                    new CandidateOrdinaryGrantReceipt[0], new CandidateInventoryEndReceipt[0], BigInteger.One, BigInteger.One, true);
            });
        }

        public static CandidateInventoryView Read(CandidateInventoryState state, ExactMathBudget budget)
        {
            Begin(state, budget); budget.CheckInteger(99);
            var rows = new List<CandidateInventoryItemView>();
            foreach (var holding in state.Holdings)
            {
                var allocated = Allocated(state, holding.ItemId, budget);
                var reserved = Reserved(state, holding.ItemId, budget);
                var free = budget.Subtract(budget.Subtract(holding.T, allocated), reserved);
                var full = budget.DivRem(free, 99, out var remainder);
                rows.Add(new CandidateInventoryItemView(state.PlayerId, holding.ItemId, holding.T, allocated, reserved, free, full, remainder));
            }
            return new CandidateInventoryView(state, rows);
        }

        public static CandidateInventoryResult GrantOrdinary(CandidateInventoryState state, CandidateOrdinaryGrant grant,
            BigInteger expectedRevision, ExactMathBudget budget)
        {
            if (grant == null) throw new ArgumentNullException(nameof(grant));
            var c = Begin(state, budget);
            if (!c.Binding(state, grant.PlayerId, grant.Context) || !c.Text(grant.AttemptId, "AttemptId") ||
                !c.Text(grant.SettlementId, "SettlementId")) return new CandidateInventoryResult(c);
            if (grant.Source == CandidateOrdinaryGrantSource.Unspecified) return Reject(c, MissingField, "Source");
            if (grant.Source != CandidateOrdinaryGrantSource.OrdinaryBaseReward) return Reject(c, UnsupportedBinding, "Source");
            if (!c.Vector(grant.Items, state.Definition, "Items", out var vector) ||
                !FindGrant(state, grant.AttemptId, grant.SettlementId, vector, c, out var prior)) return new CandidateInventoryResult(c);
            if (prior != null) return new CandidateInventoryResult(state, CandidateInventoryOutcome.AlreadyIncluded, grant: prior);
            if (!c.Revision(state.StateRevision, expectedRevision)) return new CandidateInventoryResult(c);
            var totals = Totals(state); Add(totals, vector, budget);
            var receipt = new CandidateOrdinaryGrantReceipt(state.PlayerId, grant.AttemptId, grant.SettlementId, state.Definition.Context, vector);
            var grants = new List<CandidateOrdinaryGrantReceipt>(state.OrdinaryGrants) { receipt };
            return new CandidateInventoryResult(Change(state, totals, state.Loadouts, state.ActiveCarry, grants, state.Ends,
                state.PreferenceRevision, budget), CandidateInventoryOutcome.Changed, grant: receipt, added: vector);
        }

        public static CandidateInventoryResult Equip(CandidateInventoryState state, CandidateEquipIntent intent,
            BigInteger expectedRevision, ExactMathBudget budget)
        {
            if (intent == null) throw new ArgumentNullException(nameof(intent));
            var c = Begin(state, budget);
            if (!c.Text(intent.CharacterId, "CharacterId")) return new CandidateInventoryResult(c);
            var oldLoadout = state.FindLoadout(intent.CharacterId);
            if (oldLoadout == null) return Reject(c, InconsistentBinding, "CharacterId");
            if (!intent.HasItem) return Reject(c, MissingField, "ItemId");
            if (!c.Number(intent.L, "L")) return new CandidateInventoryResult(c);
            budget.CheckInteger(99);
            if (budget.Compare(intent.L.Value, 99) > 0) return Reject(c, InvalidValue, "L");
            if (state.ActiveCarry != null) return Reject(c, ActiveAttemptConflict, "ActiveCarry");
            if (!c.Revision(state.StateRevision, expectedRevision)) return new CandidateInventoryResult(c);
            if (intent.ItemId == null)
            { if (!intent.L.Value.IsZero) return Reject(c, InvalidValue, "L"); }
            else
            {
                if (!c.Text(intent.ItemId, "ItemId")) return new CandidateInventoryResult(c);
                var item = FindItem(state.Definition, intent.ItemId);
                if (item == null || item.Kind != CandidateInventoryItemKind.OrdinaryTactical) return Reject(c, UnsupportedBinding, "ItemId");
                if (!c.Same(oldLoadout.Actor.ClassId, item.EquipClassId, "ItemId")) return new CandidateInventoryResult(c);
                var available = Free(state, item.ItemId, budget);
                if (Equal(oldLoadout.ItemId, item.ItemId)) available = budget.Add(available, oldLoadout.L);
                if (budget.Compare(available, intent.L.Value) < 0) return Reject(c, InsufficientFree, "L");
            }
            var sameItem = Equal(oldLoadout.ItemId, intent.ItemId);
            if (sameItem && budget.Compare(oldLoadout.L, intent.L.Value) == 0) return new CandidateInventoryResult(state, CandidateInventoryOutcome.Unchanged);
            var preferenceRevision = sameItem ? state.PreferenceRevision : budget.Add(state.PreferenceRevision, BigInteger.One);
            bool? enabled = intent.ItemId == null ? (bool?)null : sameItem ? oldLoadout.Enabled : true;
            var loadout = new CandidateInventoryLoadout(oldLoadout.Actor, intent.ItemId, intent.L.Value, enabled);
            return new CandidateInventoryResult(Change(state, Totals(state), ReplaceLoadout(state, loadout), null, state.OrdinaryGrants, state.Ends,
                preferenceRevision, budget), CandidateInventoryOutcome.Changed);
        }

        public static CandidateInventoryResult SetPreference(CandidateInventoryState state, CandidateInventoryPreferenceIntent intent,
            BigInteger expectedPreferenceRevision, ExactMathBudget budget)
        {
            if (intent == null) throw new ArgumentNullException(nameof(intent));
            var c = Begin(state, budget);
            if (!c.Text(intent.CharacterId, "CharacterId")) return new CandidateInventoryResult(c);
            var oldLoadout = state.FindLoadout(intent.CharacterId);
            if (oldLoadout == null) return Reject(c, InconsistentBinding, "CharacterId");
            if (!c.Text(intent.ItemId, "ItemId") || !c.Same(oldLoadout.ItemId, intent.ItemId, "ItemId")) return new CandidateInventoryResult(c);
            if (!intent.Enabled.HasValue) return Reject(c, MissingField, "Enabled");
            if (!c.Revision(state.PreferenceRevision, expectedPreferenceRevision, "ExpectedPreferenceRevision")) return new CandidateInventoryResult(c);
            if (oldLoadout.Enabled == intent.Enabled) return new CandidateInventoryResult(state, CandidateInventoryOutcome.Unchanged);
            var preference = budget.Add(state.PreferenceRevision, BigInteger.One);
            var loadout = new CandidateInventoryLoadout(oldLoadout.Actor, oldLoadout.ItemId, oldLoadout.L, intent.Enabled);
            return new CandidateInventoryResult(Change(state, Totals(state), ReplaceLoadout(state, loadout), state.ActiveCarry, state.OrdinaryGrants, state.Ends,
                preference, budget), CandidateInventoryOutcome.Changed);
        }

        public static CandidateInventoryResult Freeze(CandidateInventoryState state, CandidateInventoryFreezeIntent intent,
            BigInteger expectedRevision, ExactMathBudget budget)
        { return Freeze(state, intent, null, expectedRevision, budget); }

        public static CandidateInventoryResult Freeze(CandidateInventoryState state, CandidateInventoryFreezeIntent intent,
            CandidateRosterState roster, BigInteger expectedRevision, ExactMathBudget budget)
        {
            if (intent == null) throw new ArgumentNullException(nameof(intent));
            var c = Begin(state, budget);
            if (!c.Binding(state, intent.PlayerId, intent.Context) || !c.Text(intent.AttemptId, "AttemptId") ||
                !c.Text(intent.EntryBaselineId, "EntryBaselineId")) return new CandidateInventoryResult(c);
            if (intent.ReadyParticipants == null) return Reject(c, MissingField, "ReadyParticipants");
            if (intent.ReadyParticipants.Count < 1 || intent.ReadyParticipants.Count > (state.IsRoster ? 3 : 1))
                return Reject(c, InvalidValue, "ReadyParticipants");
            if (state.IsRoster && (roster == null || roster.PlayerId != state.PlayerId)) return Reject(c, InconsistentBinding, "Roster");
            var actors = new List<CandidateInventoryActor>(); var ids = new HashSet<string>(); var slots = new HashSet<int>();
            foreach (var participant in intent.ReadyParticipants)
            {
                if (!c.Actor(participant, "ReadyParticipants")) return new CandidateInventoryResult(c);
                var owned = state.FindLoadout(participant.CharacterId);
                if (owned == null || !ids.Add(participant.CharacterId) || !slots.Add(participant.OriginalSlot.Value))
                    return Reject(c, InconsistentBinding, "ReadyParticipants");
                if (!c.Same(owned.Actor.ClassId, participant.ClassId, "ReadyParticipants.ClassId")) return new CandidateInventoryResult(c);
                if (state.IsRoster)
                {
                    var character = roster.Find(participant.CharacterId);
                    if (roster.Formation[participant.OriginalSlot.Value] != participant.CharacterId || character == null ||
                        !character.IsReady || character.ClassId != participant.ClassId) return Reject(c, InconsistentBinding, "ReadyParticipants");
                }
                else if (participant.OriginalSlot.Value != owned.Actor.OriginalSlot) return Reject(c, InconsistentBinding, "ReadyParticipants[0].OriginalSlot");
                actors.Add(new CandidateInventoryActor(participant));
            }
            actors.Sort((a, b) => a.OriginalSlot.CompareTo(b.OriginalSlot));
            var previous = FindCarry(state, intent.AttemptId);
            if (previous != null)
            {
                if (!c.Same(previous.EntryBaselineId, intent.EntryBaselineId, "EntryBaselineId")) return new CandidateInventoryResult(c);
                if (previous.ReadyParticipants.Count != actors.Count) return Reject(c, InconsistentBinding, "ReadyParticipants");
                for (var i = 0; i < actors.Count; i++)
                    if (previous.ReadyParticipants[i].CharacterId != actors[i].CharacterId || previous.ReadyParticipants[i].OriginalSlot != actors[i].OriginalSlot)
                        return Reject(c, InconsistentBinding, "ReadyParticipants");
                return new CandidateInventoryResult(state, CandidateInventoryOutcome.AlreadyIncluded, carry: previous);
            }
            if (state.ActiveCarry != null) return Reject(c, ActiveAttemptConflict, "AttemptId");
            if (!c.Revision(state.StateRevision, expectedRevision)) return new CandidateInventoryResult(c);
            var rows = new List<CandidateCarryRow>(); var loadouts = new List<CandidateInventoryLoadout>(state.Loadouts);
            foreach (var actor in actors)
            {
                var loadout = state.FindLoadout(actor.CharacterId);
                if (loadout.ItemId == null) continue;
                budget.CheckInteger(99);
                var count = budget.Add(loadout.L, Free(state, loadout.ItemId, budget));
                if (budget.Compare(count, 99) > 0) count = 99;
                rows.Add(new CandidateCarryRow(actor, loadout.ItemId, count, new CandidateOrdinaryPool(state.PlayerId, loadout.ItemId)));
                loadouts[loadouts.IndexOf(loadout)] = new CandidateInventoryLoadout(loadout.Actor, loadout.ItemId, BigInteger.Zero, loadout.Enabled);
            }
            var carry = new CandidateCarryPlan(state.PlayerId, intent.AttemptId, intent.EntryBaselineId, state.Definition.Context, actors, rows);
            return new CandidateInventoryResult(Change(state, Totals(state), loadouts, carry, state.OrdinaryGrants, state.Ends,
                state.PreferenceRevision, budget), CandidateInventoryOutcome.Changed, carry: carry);
        }

        public static CandidateInventoryResult End(CandidateInventoryState state, CandidateInventoryEndIntent intent,
            BigInteger expectedRevision, ExactMathBudget budget)
        {
            if (intent == null) throw new ArgumentNullException(nameof(intent));
            var c = Begin(state, budget);
            if (!c.Binding(state, intent.PlayerId, intent.Context) || !c.Text(intent.AttemptId, "AttemptId") ||
                !c.Text(intent.EntryBaselineId, "EntryBaselineId") || !c.Text(intent.EndReceiptId, "EndReceiptId")) return new CandidateInventoryResult(c);
            if (intent.Kind == CandidateInventoryEndKind.Unspecified) return Reject(c, MissingField, "Kind");
            if (intent.Kind != CandidateInventoryEndKind.NormalVictory && intent.Kind != CandidateInventoryEndKind.NormalExit &&
                intent.Kind != CandidateInventoryEndKind.ImmediateRestart) return Reject(c, UnsupportedBinding, "Kind");
            if (!intent.HasSettlement) return Reject(c, MissingField, "SettlementId");
            if (!intent.HasNewAttempt) return Reject(c, MissingField, "NewAttemptId");
            if (!c.Vector(intent.Rewards, state.Definition, "Rewards", out var rewards)) return new CandidateInventoryResult(c);
            if (intent.Kind == CandidateInventoryEndKind.NormalVictory)
            { if (!c.Text(intent.SettlementId, "SettlementId")) return new CandidateInventoryResult(c); }
            else
            {
                if (intent.SettlementId != null) return Reject(c, InvalidValue, "SettlementId");
                if (rewards.Count != 0) return Reject(c, InvalidValue, "Rewards");
            }
            if (intent.Kind == CandidateInventoryEndKind.ImmediateRestart)
            {
                if (!c.Text(intent.NewAttemptId, "NewAttemptId")) return new CandidateInventoryResult(c);
                if (Equal(intent.NewAttemptId, intent.AttemptId)) return Reject(c, InconsistentBinding, "NewAttemptId");
            }
            else if (intent.NewAttemptId != null) return Reject(c, InvalidValue, "NewAttemptId");
            CandidateInventoryEndReceipt prior = null;
            foreach (var end in state.Ends)
            {
                if (!Equal(end.OriginalCarry.AttemptId, intent.AttemptId) && !Equal(end.EndReceiptId, intent.EndReceiptId)) continue;
                if (!c.Same(end.OriginalCarry.AttemptId, intent.AttemptId, "AttemptId") ||
                    !c.Same(end.EndReceiptId, intent.EndReceiptId, "EndReceiptId")) return new CandidateInventoryResult(c);
                prior = end; break;
            }
            var carry = prior != null ? prior.OriginalCarry : state.ActiveCarry;
            if (carry == null || !Equal(carry.AttemptId, intent.AttemptId)) return Reject(c, ActiveAttemptConflict, "AttemptId");
            if (!c.Same(carry.EntryBaselineId, intent.EntryBaselineId, "EntryBaselineId") ||
                !Remaining(intent.Remaining, carry, c, out var remaining)) return new CandidateInventoryResult(c);
            if (prior != null)
            {
                if (prior.Kind != intent.Kind) return Reject(c, InconsistentBinding, "Kind");
                if (!c.Same(prior.SettlementId, intent.SettlementId, "SettlementId") || !c.Same(prior.NewAttemptId, intent.NewAttemptId, "NewAttemptId") ||
                    !c.SameVector(prior.Rewards, rewards, "Rewards")) return new CandidateInventoryResult(c);
                for (var i = 0; i < remaining.Count; i++)
                    if (budget.Compare(prior.Remaining[i].U, remaining[i].U) != 0) return Reject(c, InconsistentBinding, "Remaining");
                return new CandidateInventoryResult(state, CandidateInventoryOutcome.AlreadyIncluded, grant: prior.RewardReceipt, end: prior);
            }
            if (!c.Revision(state.StateRevision, expectedRevision)) return new CandidateInventoryResult(c);
            if (intent.Kind == CandidateInventoryEndKind.ImmediateRestart && FindCarry(state, intent.NewAttemptId) != null)
                return Reject(c, InconsistentBinding, "NewAttemptId");
            CandidateOrdinaryGrantReceipt grant = null;
            if (intent.Kind == CandidateInventoryEndKind.NormalVictory &&
                !FindGrant(state, intent.AttemptId, intent.SettlementId, rewards, c, out grant)) return new CandidateInventoryResult(c);
            var totals = Totals(state); var loadouts = new List<CandidateInventoryLoadout>(state.Loadouts); CandidateCarryPlan nextCarry = null;
            if (intent.Kind == CandidateInventoryEndKind.ImmediateRestart)
                nextCarry = new CandidateCarryPlan(state.PlayerId, intent.NewAttemptId, carry.EntryBaselineId, carry.Context, carry.ReadyParticipants, carry.Rows);
            else if (carry.Rows.Count != 0)
            {
                foreach (var row in carry.Rows)
                {
                    var u = remaining.Find(x => x.CharacterId == row.Actor.CharacterId).U;
                    var loadout = state.FindLoadout(row.Actor.CharacterId);
                    if (intent.Kind == CandidateInventoryEndKind.NormalVictory) totals[row.ItemId] = budget.Subtract(totals[row.ItemId], budget.Subtract(row.C, u));
                    loadouts[loadouts.IndexOf(loadout)] = new CandidateInventoryLoadout(loadout.Actor, loadout.ItemId,
                        intent.Kind == CandidateInventoryEndKind.NormalVictory ? u : row.C, loadout.Enabled);
                }
            }
            var grants = new List<CandidateOrdinaryGrantReceipt>(state.OrdinaryGrants); var added = new List<CandidateInventoryQuantity>();
            if (intent.Kind == CandidateInventoryEndKind.NormalVictory && grant == null)
            {
                Add(totals, rewards, budget); added.AddRange(rewards);
                grant = new CandidateOrdinaryGrantReceipt(state.PlayerId, intent.AttemptId, intent.SettlementId, state.Definition.Context, rewards);
                grants.Add(grant);
            }
            var receipt = new CandidateInventoryEndReceipt(carry, intent, remaining, rewards, grant);
            var ends = new List<CandidateInventoryEndReceipt>(state.Ends) { receipt };
            return new CandidateInventoryResult(Change(state, totals, loadouts, nextCarry, grants, ends, state.PreferenceRevision, budget),
                CandidateInventoryOutcome.Changed, grant: grant, end: receipt, carry: nextCarry, added: added);
        }

        private static InventoryChecks Begin(CandidateInventoryState state, ExactMathBudget budget)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            var check = new InventoryChecks(budget); check.CheckState(state); return check;
        }
        private static CandidateInventoryResult Reject(InventoryChecks check, CandidateInventoryRejectionCode code, string path)
        { check.Fail(code, path); return new CandidateInventoryResult(check); }
        private static BigInteger Reserved(CandidateInventoryState state, string item, ExactMathBudget budget)
        {
            var count = BigInteger.Zero;
            if (state.ActiveCarry != null) foreach (var row in state.ActiveCarry.Rows) if (Equal(row.ItemId, item)) count = budget.Add(count, row.C);
            return count;
        }
        private static BigInteger Free(CandidateInventoryState state, string item, ExactMathBudget budget)
        {
            foreach (var holding in state.Holdings) if (Equal(holding.ItemId, item))
                return budget.Subtract(budget.Subtract(holding.T, Allocated(state, item, budget)), Reserved(state, item, budget));
            return BigInteger.Zero;
        }
        private static BigInteger Allocated(CandidateInventoryState state, string item, ExactMathBudget budget)
        {
            var count = BigInteger.Zero;
            foreach (var loadout in state.Loadouts) if (Equal(loadout.ItemId, item)) count = budget.Add(count, loadout.L);
            return count;
        }
        private static List<CandidateInventoryLoadout> ReplaceLoadout(CandidateInventoryState state, CandidateInventoryLoadout loadout)
        {
            var result = new List<CandidateInventoryLoadout>();
            foreach (var prior in state.Loadouts) result.Add(prior.Actor.CharacterId == loadout.Actor.CharacterId ? loadout : prior);
            return result;
        }
        private static Dictionary<string, BigInteger> Totals(CandidateInventoryState state)
        { var values = new Dictionary<string, BigInteger>(StringComparer.Ordinal); foreach (var h in state.Holdings) values.Add(h.ItemId, h.T); return values; }
        private static void Add(Dictionary<string, BigInteger> totals, IReadOnlyList<CandidateInventoryQuantity> vector, ExactMathBudget budget)
        { foreach (var row in vector) totals[row.ItemId] = budget.Add(totals[row.ItemId], row.Quantity); }
        private static CandidateInventoryState Change(CandidateInventoryState state, Dictionary<string, BigInteger> totals,
            IEnumerable<CandidateInventoryLoadout> loadouts, CandidateCarryPlan carry, IEnumerable<CandidateOrdinaryGrantReceipt> grants,
            IEnumerable<CandidateInventoryEndReceipt> ends, BigInteger preferenceRevision, ExactMathBudget budget)
        {
            var revision = budget.Add(state.StateRevision, BigInteger.One); var holdings = new List<CandidateInventoryHolding>();
            foreach (var item in state.Definition.Items) holdings.Add(new CandidateInventoryHolding(item.ItemId, totals[item.ItemId]));
            return new CandidateInventoryState(state.Definition, state.PlayerId, holdings, loadouts, carry, grants, ends, revision, preferenceRevision, state.IsRoster, state.GetPermanentLedger());
        }
        private static CandidateCarryPlan FindCarry(CandidateInventoryState state, string attempt)
        {
            if (state.ActiveCarry != null && Equal(state.ActiveCarry.AttemptId, attempt)) return state.ActiveCarry;
            foreach (var end in state.Ends) if (Equal(end.OriginalCarry.AttemptId, attempt)) return end.OriginalCarry;
            return null;
        }
        private static bool FindGrant(CandidateInventoryState state, string attempt, string settlement,
            IReadOnlyList<CandidateInventoryQuantity> vector, InventoryChecks c, out CandidateOrdinaryGrantReceipt receipt)
        {
            receipt = null;
            foreach (var prior in state.OrdinaryGrants)
            {
                if (!Equal(prior.SettlementId, settlement)) continue;
                if (!c.Same(prior.AttemptId, attempt, "AttemptId") || !c.SameVector(prior.Items, vector, "Items")) return false;
                receipt = prior; break;
            }
            return true;
        }
        private static bool Remaining(List<CandidateInventoryRemainingInput> input, CandidateCarryPlan carry, InventoryChecks c,
            out List<CandidateInventoryRemaining> result)
        {
            result = new List<CandidateInventoryRemaining>();
            if (input == null) return c.Fail(MissingField, "Remaining");
            var seen = new HashSet<string>(StringComparer.Ordinal);
            for (var i = 0; i < input.Count; i++)
            {
                var row = input[i]; var p = $"Remaining[{i}]";
                if (row == null) return c.Fail(MissingField, p);
                if (!c.Text(row.CharacterId, p + ".CharacterId") || !c.Text(row.ItemId, p + ".ItemId") || !c.Number(row.U, p + ".U")) return false;
                if (!seen.Add(row.CharacterId)) return c.Fail(InvalidValue, p + ".CharacterId");
                CandidateCarryRow original = null;
                foreach (var candidate in carry.Rows) if (Equal(candidate.Actor.CharacterId, row.CharacterId) && Equal(candidate.ItemId, row.ItemId)) original = candidate;
                if (original == null) return c.Fail(InconsistentBinding, p);
                if (c.Math.Compare(row.U.Value, original.C) > 0) return c.Fail(InvalidValue, p + ".U");
                result.Add(new CandidateInventoryRemaining(row.CharacterId, row.ItemId, row.U.Value));
            }
            return result.Count == carry.Rows.Count || c.Fail(InconsistentBinding, "Remaining");
        }
    }
}
