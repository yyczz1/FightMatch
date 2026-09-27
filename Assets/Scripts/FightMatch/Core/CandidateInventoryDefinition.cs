using System;
using System.Collections.Generic;
using System.Numerics;
using static FightMatch.Core.CandidateInventoryRejectionCode;

namespace FightMatch.Core
{
    public enum CandidateInventoryRejectionCode
    { None, MissingField, InvalidValue, InconsistentBinding, UnsupportedBinding, StaleContext, InsufficientFree, ActiveAttemptConflict }
    public enum CandidateInventoryItemKind { Unspecified, OrdinaryMaterial, OrdinaryTactical }
    public enum CandidateInventoryOutcome { Unchanged, Changed, AlreadyIncluded }

    public sealed class CandidateInventoryDefinitionInput
    {
        public RuleContext Context { get; set; }
        public List<CandidateInventoryItemInput> Items { get; set; }
    }
    public sealed class CandidateInventoryItemInput
    {
        public string ItemId { get; set; }
        public CandidateInventoryItemKind Kind { get; set; }
        public string EquipClassId { get; set; }
    }
    public sealed class CandidateInventoryItem
    {
        public string ItemId { get; }
        public CandidateInventoryItemKind Kind { get; }
        public string EquipClassId { get; }
        internal CandidateInventoryItem(CandidateInventoryItemInput input)
        { ItemId = input.ItemId; Kind = input.Kind; EquipClassId = input.EquipClassId; }
    }
    public sealed class CandidateInventoryDefinition
    {
        public PreparedRuleContext Context { get; }
        public IReadOnlyList<CandidateInventoryItem> Items { get; }
        internal CandidateInventoryDefinition(RuleContext context, List<CandidateInventoryItem> items)
        { Context = RuleContextChecks.Freeze(context); Items = new List<CandidateInventoryItem>(items).AsReadOnly(); }
    }
    public sealed class CandidateInventoryDefinitionResult
    {
        public bool IsAccepted => Definition != null;
        public CandidateInventoryDefinition Definition { get; }
        public CandidateInventoryRejectionCode RejectionCode { get; }
        public string FieldPath { get; }
        internal CandidateInventoryDefinitionResult(CandidateInventoryDefinition definition) { Definition = definition; }
        internal CandidateInventoryDefinitionResult(InventoryChecks check) { RejectionCode = check.Code; FieldPath = check.Path; }
    }

    internal sealed class InventoryChecks
    {
        internal readonly ExactMathBudget Math;
        internal CandidateInventoryRejectionCode Code { get; private set; }
        internal string Path { get; private set; }
        internal InventoryChecks(ExactMathBudget math) { Math = math; }
        internal bool Fail(CandidateInventoryRejectionCode code, string path) { Code = code; Path = path; return false; }
        internal bool Text(string value, string path) { return !string.IsNullOrWhiteSpace(value) || Fail(MissingField, path); }
        internal bool Same(string a, string b, string path) { return Equal(a, b) || Fail(InconsistentBinding, path); }
        internal static bool Equal(string a, string b) { return string.Equals(a, b, StringComparison.Ordinal); }
        internal bool Number(BigInteger? value, string path, bool positive = false)
        {
            if (!value.HasValue) return Fail(MissingField, path);
            Math.CheckInteger(value.Value);
            return Math.Compare(value.Value, positive ? BigInteger.One : BigInteger.Zero) >= 0 || Fail(InvalidValue, path);
        }
        internal bool Context(RuleContext value, string path)
        {
            var rejection = RuleContextChecks.Validate(value, Math, path);
            return rejection == null || Fail((CandidateInventoryRejectionCode)Enum.Parse(typeof(CandidateInventoryRejectionCode), rejection.RejectionCode.ToString()), rejection.FieldPath);
        }
        internal bool Binding(CandidateInventoryState state, string player, RuleContext context)
        {
            if (!Text(player, "PlayerId") || !Same(state.PlayerId, player, "PlayerId") || !Context(context, "Context")) return false;
            var difference = RuleContextChecks.Difference(RuleContextChecks.Copy(state.Definition.Context), context, "Context", Math);
            return difference == null || Fail(InconsistentBinding, difference);
        }
        internal bool Actor(CandidateInventoryActorInput input, string path)
        {
            if (input == null) return Fail(MissingField, path);
            if (!Text(input.CharacterId, path + ".CharacterId") || !Text(input.ClassId, path + ".ClassId")) return false;
            if (input.ClassKind == CharacterClassKind.Unspecified) return Fail(MissingField, path + ".ClassKind");
            if (input.ClassKind != CharacterClassKind.Warrior) return Fail(UnsupportedBinding, path + ".ClassKind");
            if (!Number(input.OriginalSlot, path + ".OriginalSlot")) return false;
            return input.OriginalSlot.Value <= 2 || Fail(InvalidValue, path + ".OriginalSlot");
        }
        internal bool Revision(BigInteger actual, BigInteger expected, string path = "ExpectedRevision")
        { Math.CheckInteger(expected); return Math.Compare(actual, expected) == 0 || Fail(StaleContext, path); }
        internal bool Vector(List<CandidateInventoryQuantityInput> input, CandidateInventoryDefinition definition, string path,
            out List<CandidateInventoryQuantity> vector)
        {
            vector = new List<CandidateInventoryQuantity>();
            if (input == null) return Fail(MissingField, path);
            var seen = new HashSet<string>(StringComparer.Ordinal);
            for (var i = 0; i < input.Count; i++)
            {
                var row = input[i]; var p = path + $"[{i}]";
                if (row == null) return Fail(MissingField, p);
                if (!Text(row.ItemId, p + ".ItemId") || !Number(row.Quantity, p + ".Quantity")) return false;
                if (!seen.Add(row.ItemId)) return Fail(InvalidValue, p + ".ItemId");
                if (FindItem(definition, row.ItemId) == null) return Fail(UnsupportedBinding, p + ".ItemId");
                vector.Add(new CandidateInventoryQuantity(row.ItemId, row.Quantity.Value));
            }
            vector.Sort((a, b) => StringComparer.Ordinal.Compare(a.ItemId, b.ItemId));
            return true;
        }
        internal bool SameVector(IReadOnlyList<CandidateInventoryQuantity> a, IReadOnlyList<CandidateInventoryQuantity> b, string path)
        {
            if (a.Count != b.Count) return Fail(InconsistentBinding, path);
            for (var i = 0; i < a.Count; i++)
                if (!Equal(a[i].ItemId, b[i].ItemId) || Math.Compare(a[i].Quantity, b[i].Quantity) != 0) return Fail(InconsistentBinding, path);
            return true;
        }
        internal static CandidateInventoryItem FindItem(CandidateInventoryDefinition definition, string id)
        { foreach (var item in definition.Items) if (Equal(item.ItemId, id)) return item; return null; }
        internal void CheckVector(IReadOnlyList<CandidateInventoryQuantity> items)
        { foreach (var item in items) Math.CheckInteger(item.Quantity); }
        internal void CheckPlan(CandidateCarryPlan plan)
        {
            RuleContextChecks.CheckBudget(plan.Context, Math);
            foreach (var actor in plan.ReadyParticipants) Math.CheckInteger(actor.OriginalSlot);
            foreach (var row in plan.Rows) Math.CheckInteger(row.C);
        }
        internal void CheckState(CandidateInventoryState state)
        {
            RuleContextChecks.CheckBudget(state.Definition.Context, Math); Math.CheckInteger(state.StateRevision);
            Math.CheckInteger(state.PreferenceRevision);
            foreach (var loadout in state.Loadouts) { Math.CheckInteger(loadout.Actor.OriginalSlot); Math.CheckInteger(loadout.L); }
            foreach (var holding in state.Holdings) Math.CheckInteger(holding.T);
            if (state.ActiveCarry != null) CheckPlan(state.ActiveCarry);
            foreach (var receipt in state.OrdinaryGrants)
            { RuleContextChecks.CheckBudget(receipt.Context, Math); CheckVector(receipt.Items); }
            foreach (var end in state.Ends)
            {
                CheckPlan(end.OriginalCarry); CheckVector(end.Rewards);
                foreach (var row in end.Remaining) Math.CheckInteger(row.U);
                if (end.RewardReceipt != null)
                { RuleContextChecks.CheckBudget(end.RewardReceipt.Context, Math); CheckVector(end.RewardReceipt.Items); }
            }
        }
    }
}
