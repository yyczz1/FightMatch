using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using FightMatch.Application;
using FightMatch.Core;
using UnityEngine.UIElements;
using NavigationView = FightMatch.Application.PlayerNavigationView;

namespace FightMatch.Presentation
{
    public sealed class PlayerPermanentDetailView : VisualElement
    {
        private readonly PlayerNavigationController controller;
        private readonly NavigationBindings bindings = new NavigationBindings();
        public PlayerPermanentDetailView(PlayerNavigationController controller)
        { this.controller = controller ?? throw new ArgumentNullException(nameof(controller)); name = "permanent-detail"; }
        internal void ClearBindings() { bindings.Dispose(); }
        private bool Integer(string text, Label error, out BigInteger value)
        {
            value = 0;
            if (text == null || text.Length == 0 || text.Length > controller.NumericTokenLimit)
            { error.text = "Limit: quantity"; return false; }
            if (text.Any(x => x < '0' || x > '9') || text.Length > 1 && text[0] == '0' ||
                !BigInteger.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out value))
            { error.text = "InvalidValue: use a nonnegative whole number"; return false; }
            var bytes = value.ToByteArray();
            var top = bytes.Length - 1;
            while (top > 0 && bytes[top] == 0) top--;
            var high = bytes[top]; var bits = top * 8L;
            while (high != 0) { bits++; high >>= 1; }
            if (bits > controller.IntegerBitLimit)
            { error.text = "Limit: quantity"; return false; }
            return true;
        }
        public void Render(NavigationView view)
        {
            bindings.Dispose(); Clear();
            Add(new Label(view.DetailKind + " / " + (view.DefinitionId ?? "Empty")));
            Add(new Label("Character: " + (view.Context.SelectedCharacterId ?? "None")));
            var quantity = new TextField("Quantity") { name = "permanent-quantity", value = view.DetailKind == CandidatePermanentKind.Craft ? "1" : "0" };
            var enabled = new Toggle("Use equipped item") { name = "permanent-enabled" };
            var error = new Label { name = "permanent-input-error" };
            if (view.DetailKind != CandidatePermanentKind.SetPreference) Add(quantity);
            else Add(enabled);
            if (view.DetailKind == CandidatePermanentKind.Equip && view.DefinitionId == null) quantity.SetEnabled(false);
            var explicitSources = new Toggle("Select exact source portions") { name = "permanent-explicit-sources" };
            var rows = new List<Tuple<CandidatePermanentPortion, TextField, TextField>>();
            if (view.DetailKind == CandidatePermanentKind.Craft)
            {
                Add(explicitSources);
                foreach (var source in view.Permanent?.SourceChoices ?? Array.Empty<CandidatePermanentPortion>())
                {
                    var key = SourceKey(source);
                    var row = new VisualElement { name = "source-" + key };
                    row.Add(new Label(source.ItemId + " / " + key));
                    var start = new TextField("Start") { name = "source-start", value = source.UnitStart.ToString(CultureInfo.InvariantCulture) };
                    var count = new TextField("Count") { name = "source-count", value = "0" };
                    row.Add(start); row.Add(count); Add(row); rows.Add(Tuple.Create(source, start, count));
                }
            }
            Add(error);
            var preview = controller.PreviewHandler(() => {
                error.text = "";
                BigInteger? amount = null;
                if (view.DetailKind != CandidatePermanentKind.SetPreference)
                {
                    if (!Integer(quantity.value, error, out var number)) return null;
                    amount = number;
                }
                List<CandidatePermanentPortion> selected = null;
                if (view.DetailKind == CandidatePermanentKind.Craft && explicitSources.value)
                {
                    selected = new List<CandidatePermanentPortion>();
                    foreach (var row in rows)
                    {
                        if (!Integer(row.Item2.value, error, out var start) || !Integer(row.Item3.value, error, out var count)) return null;
                        if (count.IsZero) continue;
                        var source = row.Item1;
                        selected.Add(new CandidatePermanentPortion(source.Source, source.OutputOperationId, source.OutputLine,
                            source.ItemId, start, count, source.Endpoint, source.EndpointOperationId));
                    }
                }
                return new PlayerNavigationDraft { Kind = PlayerNavigationDraftKind.Permanent, Permanent = new PlayerPermanentDraft {
                    Kind = view.DetailKind, CharacterId = view.Context.SelectedCharacterId, DefinitionId = view.DefinitionId,
                    Quantity = amount, Enabled = view.DetailKind == CandidatePermanentKind.SetPreference ? (bool?)enabled.value : null,
                    PreferenceRevision = view.DetailKind == CandidatePermanentKind.SetPreference ? view.Read.Inventory.State.PreferenceRevision : (BigInteger?)null,
                    SelectedInputs = selected } };
            });
            bindings.Button(this, "permanent-preview", "Review transaction", preview,
                view.Permanent?.UnavailabilityReason ?? (view.Context.SelectedCharacterId == null ? "ActorSelectionRequired" :
                    view.DetailKind == CandidatePermanentKind.Equip && view.Read.Head.Business.ActiveHistory != null ? "ActiveAttemptConflict" : null));
        }
        internal static void Quote(VisualElement parent, CandidatePermanentQuote quote)
        {
            if (quote == null) return;
            parent.Add(new Label(quote.DefinitionId + " / " + quote.DefinitionVersion + " × " + quote.Quantity));
            foreach (var cost in quote.Costs) parent.Add(new Label("Cost " + cost.ItemId + " × " + cost.Quantity));
            foreach (var portion in quote.Inputs)
                parent.Add(new Label("Source " + portion.ItemId + " / " + SourceKey(portion)));
            foreach (var output in quote.Outputs) parent.Add(new Label("Output " + output.ItemId + " × " + output.Quantity));
        }
        private static string SourceKey(CandidatePermanentPortion portion)
        {
            var source = portion.Source; var grant = source?.Grant; var inclusion = source?.Inclusion;
            return PlayerNavigationView.Key(grant?.Kind.ToString(), grant?.PlayerId, grant?.Binding.PackageId,
                grant?.Binding.ContentFingerprint, grant?.Binding.RuleVersion, grant?.Binding.NumericContractVersion,
                grant?.Binding.RandomContractVersion, grant?.AttemptId, grant?.SettlementId, grant?.GrantId,
                grant?.Purpose, grant?.FactId, grant?.UseId, source?.OriginalCommitId, source?.OriginalOperationId,
                source?.OriginalBranchId, source?.GrantLine.ToString(), source?.AcquisitionOrder.ToString(),
                source?.OriginalQuantity.ToString(), source?.RetractionOperationId, inclusion?.CheckpointId,
                inclusion?.BranchId, inclusion?.SourceCommitId, inclusion?.EvidenceViewRevision.ToString(),
                inclusion?.Disposition.ToString(), inclusion?.EndpointOperationId, portion.OutputOperationId,
                portion.OutputLine.ToString(), portion.ItemId, portion.UnitStart.ToString(), portion.UnitCount.ToString(),
                portion.Endpoint.ToString(), portion.EndpointOperationId);
        }
    }
}
