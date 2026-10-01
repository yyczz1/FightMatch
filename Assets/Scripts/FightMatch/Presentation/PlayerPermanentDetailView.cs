using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using FightMatch.Application;
using FightMatch.Core;
using UnityEngine;
using NavigationView = FightMatch.Application.PlayerNavigationView;

namespace FightMatch.Presentation
{
    public sealed class PlayerPermanentDetailView : MonoBehaviour, IDisposable
    {
        [SerializeField] private LocalizedTmpText title;
        [SerializeField] private LocalizedTmpText character;
        [SerializeField] private LocalizedTmpText quantityLabel;
        [SerializeField] private LocalizedTmpText preferenceLabel;
        [SerializeField] private LocalizedTmpText explicitSourcesLabel;
        [SerializeField] private LocalizedTmpText inputError;
        [SerializeField] private TMPro.TMP_InputField quantity;
        [SerializeField] private UnityEngine.UI.Toggle enabledToggle;
        [SerializeField] private UnityEngine.UI.Toggle explicitSources;
        [SerializeField] private RectTransform sourceRows;
        [SerializeField] private GameObject sourceTemplate;
        [SerializeField] private UnityEngine.UI.Button previewButton;
        [SerializeField] private LocalizedTmpText textTemplate;
        private readonly NavigationBindings bindings = new NavigationBindings();
        private PlayerNavigationController controller;
        private LocalizationService localization;
        private int generation;

        internal void Bind(PlayerNavigationController navigation, LocalizationService service)
        {
            Unbind();
            controller = navigation ?? throw new ArgumentNullException(nameof(navigation));
            localization = service ?? throw new ArgumentNullException(nameof(service));
            generation++;
        }
        internal void ClearBindings() { bindings.Dispose(); }
        private bool Integer(string text, out BigInteger value)
        {
            value = 0;
            if (text == null || text.Length == 0 || text.Length > controller.NumericTokenLimit)
            { InputError(); return false; }
            if (text.Any(x => x < '0' || x > '9') || text.Length > 1 && text[0] == '0' ||
                !BigInteger.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out value))
            { InputError(); return false; }
            var bytes = value.ToByteArray();
            var top = bytes.Length - 1;
            while (top > 0 && bytes[top] == 0) top--;
            var high = bytes[top]; var bits = top * 8L;
            while (high != 0) { bits++; high >>= 1; }
            if (bits > controller.IntegerBitLimit)
            { InputError(); return false; }
            return true;
        }
        private void InputError()
        {
            bindings.Text(inputError, localization, "fm.input.nonnegative_integer");
            inputError.gameObject.SetActive(true);
        }
        internal void Render(NavigationView view)
        {
            bindings.Dispose();
            gameObject.SetActive(false);
            if (controller == null || view.Read?.Application.Phase == CandidateApplicationPhase.Disposed ||
                view.Read?.IsAvailable != true || view.Route != PlayerNavigationRoute.Detail) return;
            var operationKey = NavigationBindings.OperationKey(CandidateApplicationKind.PermanentRequest, view.DetailKind,
                view.DetailKind == CandidatePermanentKind.Equip && view.DefinitionId == null);
            title.gameObject.SetActive(operationKey != null);
            if (operationKey != null) bindings.Text(title, localization, operationKey);
            bindings.DynamicText(character, localization, "fm.entry.member.selected", () => new[] {
                NavigationBindings.Arg("characterName", NavigationBindings.Resolve(localization,
                    NavigationBindings.CharacterKey(view.Context.SelectedCharacterId))) });
            var preference = view.DetailKind == CandidatePermanentKind.SetPreference;
            quantity.gameObject.SetActive(!preference); quantityLabel.gameObject.SetActive(!preference);
            enabledToggle.gameObject.SetActive(preference); preferenceLabel.gameObject.SetActive(preference);
            quantity.SetTextWithoutNotify(view.DetailKind == CandidatePermanentKind.Craft ? "1" : "0");
            quantity.interactable = view.DetailKind != CandidatePermanentKind.Equip || view.DefinitionId != null;
            enabledToggle.SetIsOnWithoutNotify(false);
            explicitSources.SetIsOnWithoutNotify(false);
            // N-D03: no published player source numbering or range contract. Do not clone or expose sources.
            explicitSources.interactable = false;
            explicitSources.gameObject.SetActive(false);
            explicitSourcesLabel.gameObject.SetActive(false);
            sourceRows.gameObject.SetActive(false);
            inputError.gameObject.SetActive(false);
            // Non-L1 preference on/off wording is not in the accepted snapshot; keep an explicit binding diagnostic.
            if (preference) bindings.Text(preferenceLabel, localization, "");
            else bindings.Text(quantityLabel, localization, "fm.common.field.quantity");
            var currentGeneration = generation;
            var preview = controller.PreviewHandler(() => {
                if (controller == null || generation != currentGeneration) return null;
                inputError.Unbind(); inputError.gameObject.SetActive(false);
                BigInteger? amount = null;
                if (view.DetailKind != CandidatePermanentKind.SetPreference)
                {
                    if (!Integer(quantity.text, out var number)) return null;
                    amount = number;
                }
                return new PlayerNavigationDraft { Kind = PlayerNavigationDraftKind.Permanent, Permanent = new PlayerPermanentDraft {
                    Kind = view.DetailKind, CharacterId = view.Context.SelectedCharacterId, DefinitionId = view.DefinitionId,
                    Quantity = amount, Enabled = view.DetailKind == CandidatePermanentKind.SetPreference ? (bool?)enabledToggle.isOn : null,
                    PreferenceRevision = view.DetailKind == CandidatePermanentKind.SetPreference ? view.Read.Inventory.State.PreferenceRevision : (BigInteger?)null } };
            });
            var reason = view.Permanent?.UnavailabilityReason ?? (view.Context.SelectedCharacterId == null ? "ActorSelectionRequired" :
                view.DetailKind == CandidatePermanentKind.Equip && view.Read.Head.Business.ActiveHistory != null ? "ActiveAttemptConflict" : null);
            bindings.Button(previewButton, localization, "fm.common.action.review_changes", preview, reason,
                reasonDomain: view.DetailKind == CandidatePermanentKind.Equip || preference ? NavigationReasonDomain.Equipment : NavigationReasonDomain.Permanent);
            gameObject.SetActive(true);
        }
        internal static void Quote(NavigationBindings bindings, LocalizedTmpText template, Transform parent,
            LocalizationService localization, CandidatePermanentQuote quote)
        {
            if (quote == null) return;
            if (quote.Kind == CandidatePermanentKind.Equip && quote.DefinitionId == null)
                bindings.DynamicRow(template, parent, FightMatchViewId.Row("quote.Summary"), localization,
                    "fm.inventory.equipment.clear_preview", () => new[] {
                        NavigationBindings.Arg("characterName", NavigationBindings.Resolve(localization, NavigationBindings.CharacterKey(quote.CharacterId))) });
            // N-P05: Craft costs, portions and output previews are deferred, not diagnostic rows.
        }
        public void Unbind()
        {
            generation++;
            bindings.Dispose();
            controller = null; localization = null;
            gameObject.SetActive(false);
        }
        public void Dispose() { Unbind(); }
        private void OnDestroy() { Unbind(); }
    }
}
