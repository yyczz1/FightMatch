using System;
using System.Collections.Generic;
using FightMatch.Platform;
using FightMatch.Presentation;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace FightMatch.Host
{
    public sealed class FightMatchHostView : MonoBehaviour, IDisposable
    {
        [SerializeField] private GameObject startupPage, navigationPage, battlePage, resultPage;
        [SerializeField] private GameObject languageFeedbackPanel, licensePopup, quitPopup, loadingOverlay, blockingDiagnostic;
        [SerializeField] private PlayerNavigationView navigationView;
        [SerializeField] private PlayerBattleView battleView;
        [SerializeField] private CandidateBoardElement board;
        [SerializeField] private FightMatchResponsiveLayout responsiveLayout;
        [SerializeField] private GameObject confirmationPopupRoot, confirmationRootMask, navigationConfirmationPanel, battleConfirmationPanel;
        [SerializeField] private GameObject recoveryScreenRoot, recoveryRootMask, navigationRecoveryPanel, battleRecoveryPanel;
        [SerializeField] private LocalizedTmpText profileTitle, startupStatus, languageCaption, languageFeedback;
        [SerializeField] private LocalizedTmpText languageEnglishOption, languageChineseOption;
        [SerializeField] private LocalizedTmpText licenseTitle, quitTitle, quitBody, loadingLabel, diagnosticText;
        [SerializeField] private TextMeshProUGUI licenseBody;
        [SerializeField] private UnityEngine.UI.Button createButton, continueButton, reloadButton, backButton;
        [SerializeField] private FightMatchLocaleDropdown languageDropdown;
        [SerializeField] private UnityEngine.UI.Button licenseButton, licenseCloseButton, quitConfirmButton, quitCancelButton;
        private readonly List<KeyValuePair<UnityEngine.UI.Button, UnityAction>> listeners = new List<KeyValuePair<UnityEngine.UI.Button, UnityAction>>();
        private readonly List<LocalizedTmpText> labels = new List<LocalizedTmpText>();
        private FightMatchHostSession session;
        private LocalizationService localization;
        private Func<LocaleId, LocalePreferenceSaveResult> selectLocalePreference;
        private UnityAction<int> localeSelection;
        private FightMatchHostPage contentPage = FightMatchHostPage.Startup;
        private long generation;
        private string infrastructureDiagnostic;
        private bool rendering, bound;
        private bool overlayConflict;
        private float? languageFeedbackDeadline;
        public PlayerBattleView BattleView => battleView;
        internal PlayerNavigationView NavigationView => navigationView;
        internal string DiagnosticCode => infrastructureDiagnostic ?? (overlayConflict ? "OverlappingPanelOwners" : localization?.BindingDiagnostic);
        internal bool DiagnosticVisible => blockingDiagnostic != null && blockingDiagnostic.activeSelf;

        internal void Bind(FightMatchHostSession owner, LocalizationService service, string fontLicense) =>
            Bind(owner, service, fontLicense, null);

        internal void Bind(FightMatchHostSession owner, LocalizationService service, string fontLicense,
            Func<LocaleId, LocalePreferenceSaveResult> selectLocale)
        {
            Unbind();
            if (service == null) throw new ArgumentNullException(nameof(service));
            if (startupPage == null || navigationPage == null || battlePage == null || resultPage == null ||
                languageFeedbackPanel == null || licensePopup == null || quitPopup == null || loadingOverlay == null || blockingDiagnostic == null ||
                navigationView == null || battleView == null || board == null || licenseBody == null || responsiveLayout == null ||
                confirmationPopupRoot == null || confirmationRootMask == null || navigationConfirmationPanel == null || battleConfirmationPanel == null ||
                recoveryScreenRoot == null || recoveryRootMask == null || navigationRecoveryPanel == null || battleRecoveryPanel == null ||
                profileTitle == null || startupStatus == null || languageCaption == null || languageFeedback == null ||
                languageEnglishOption == null || languageChineseOption == null ||
                licenseTitle == null || quitTitle == null || quitBody == null || loadingLabel == null || diagnosticText == null ||
                createButton == null || continueButton == null || reloadButton == null || backButton == null ||
                languageDropdown == null || languageDropdown.placeholder != languageCaption.Target ||
                licenseButton == null || licenseCloseButton == null || quitConfirmButton == null || quitCancelButton == null)
                throw new InvalidOperationException("Host serialized bindings are incomplete.");
            session = owner; localization = service; selectLocalePreference = selectLocale; bound = true;
            responsiveLayout.ValidityChanged += OnLayoutValidityChanged;
            responsiveLayout.Bind(service);
            Canvas.willRenderCanvases -= SynchronizeOverlayRoots;
            Canvas.willRenderCanvases += SynchronizeOverlayRoots;
            if (!FightMatchViewId.Validate(transform, out var diagnostic)) infrastructureDiagnostic = diagnostic;
            if (!service.IsReady) infrastructureDiagnostic = "LocalizationNotReady";
            licenseBody.text = string.IsNullOrEmpty(fontLicense) ? LocalizedTmpText.Placeholder : fontLicense;
            if (string.IsNullOrEmpty(fontLicense)) infrastructureDiagnostic = "MissingFontLicense";
            BindText(profileTitle, "fm.profile.title");
            RefreshLocaleOptions(service.CurrentLocale);
            BindText(licenseTitle, "fm.font_licenses.title");
            BindText(quitTitle, "fm.exit_app.title"); BindText(quitBody, "fm.exit_app.body");
            BindText(loadingLabel, "fm.common.state.loading");
            SetLanguageFeedbackVisible(false);
            AddButton(createButton, "fm.profile.create.button", () => session?.CreateProfile());
            AddButton(continueButton, "fm.profile.creation_pending.continue_button", () => session?.ContinueCreation());
            AddButton(reloadButton, "fm.profile.reload.button", () => session?.ObserveStartup());
            AddButton(backButton, "fm.common.action.back", () => session?.Back());
            var epoch = generation;
            languageDropdown.CanOpen = () => bound && epoch == generation && CanSelectLocale();
            localeSelection = index =>
            {
                if (!bound || epoch != generation) return;
                if (!CanSelectLocale() || index < 0 || index > 1)
                { languageDropdown.Close(); languageDropdown.SetValueWithoutNotify(-1); return; }
                SelectLocale(index == 0 ? LocaleId.En : LocaleId.ZhHans);
                if (bound && epoch == generation) languageDropdown.SetValueWithoutNotify(-1);
            };
            languageDropdown.onValueChanged.AddListener(localeSelection);
            AddButton(licenseButton, "fm.font_licenses.open_button", () => ShowLicense(true));
            AddButton(licenseCloseButton, "fm.common.action.back", () => ShowLicense(false));
            AddButton(quitConfirmButton, "fm.exit_app.confirm_button", () => session?.ConfirmQuit());
            AddButton(quitCancelButton, "fm.exit_app.stay_button", () => session?.CancelQuit());
            if (session != null) session.Changed += Render;
            localization.DiagnosticsChanged += RefreshDiagnostic;
            localization.LocaleChanged += RefreshLocaleOptions;
            licensePopup.SetActive(false); loadingOverlay.SetActive(false);
            Render();
        }

        private void BindText(LocalizedTmpText label, string key, params KeyValuePair<string, string>[] arguments)
        {
            if (label == null) { infrastructureDiagnostic = "MissingTmpBinding"; return; }
            if (!labels.Contains(label)) labels.Add(label);
            label.Bind(localization, key, arguments);
        }
        private void AddButton(UnityEngine.UI.Button button, string key, Action action)
        {
            if (button == null) { infrastructureDiagnostic = "MissingButtonBinding"; return; }
            var texts = button.GetComponentsInChildren<LocalizedTmpText>(true);
            if (texts.Length == 0) infrastructureDiagnostic = "MissingTmpBinding";
            else BindText(texts[0], key);
            for (var i = 1; i < texts.Length; i++) texts[i].gameObject.SetActive(false);
            var epoch = generation;
            UnityAction callback = () => { if (bound && epoch == generation && DiagnosticCode == null) action(); };
            button.onClick.AddListener(callback);
            listeners.Add(new KeyValuePair<UnityEngine.UI.Button, UnityAction>(button, callback));
        }
        private void SelectLocale(LocaleId locale)
        {
            if (!bound) return;
            var epoch = generation;
            var select = selectLocalePreference;
            LocalePreferenceSaveResult result = null;
            if (select == null) localization.SetLocale(locale);
            else result = select(locale);
            if (!bound || epoch != generation || (select != null && result == null)) return;
            BindText(languageFeedback, result?.LocalizationKey ?? "fm.language.changed",
                result?.ErrorCode == null ? Array.Empty<KeyValuePair<string, string>>() :
                new[] { new KeyValuePair<string, string>("errorCode", result.ErrorCode) });
            SetLanguageFeedbackVisible(true);
            if (result == null || result.Disposition == LocalePreferenceSaveDisposition.Saved)
                languageFeedbackDeadline = Time.unscaledTime + 2f;
        }
        private bool CanSelectLocale() => bound && DiagnosticCode == null && !board.HasActivePointer &&
            !recoveryScreenRoot.activeSelf && !overlayConflict && !confirmationPopupRoot.activeSelf &&
            !navigationRecoveryPanel.activeSelf && !battleRecoveryPanel.activeSelf &&
            !navigationConfirmationPanel.activeSelf && !battleConfirmationPanel.activeSelf &&
            !licensePopup.activeSelf && !quitPopup.activeSelf && !loadingOverlay.activeSelf;
        private void RefreshLocaleOptions(LocaleId locale)
        {
            if (!bound) return;
            languageDropdown.Close();
            BindText(languageEnglishOption, "fm.language.option.en");
            BindText(languageChineseOption, "fm.language.option.zh_cn");
            BindText(languageCaption, locale == LocaleId.En ? "fm.language.option.en" : "fm.language.option.zh_cn");
            languageDropdown.options = new List<TMP_Dropdown.OptionData> {
                new TMP_Dropdown.OptionData(languageEnglishOption.Target.text),
                new TMP_Dropdown.OptionData(languageChineseOption.Target.text) };
            languageDropdown.SetValueWithoutNotify(-1);
            languageDropdown.RefreshShownValue();
        }
        private void SetLanguageFeedbackVisible(bool visible)
        {
            languageFeedbackDeadline = null;
            if (languageFeedback != null) languageFeedback.gameObject.SetActive(visible);
            if (languageFeedbackPanel != null) languageFeedbackPanel.SetActive(visible);
        }
        private void ShowLicense(bool show)
        {
            if (!bound || (show && (recoveryScreenRoot.activeSelf || overlayConflict))) return;
            if (show) { PausePresentation(); SetLanguageFeedbackVisible(false); }
            licensePopup.SetActive(show);
        }
        private void Render()
        {
            if (!bound || rendering) return;
            rendering = true;
            try
            {
                if (session == null)
                {
                    startupPage.SetActive(false); navigationPage.SetActive(false); battlePage.SetActive(false); resultPage.SetActive(false);
                    quitPopup.SetActive(false); createButton.interactable = false; continueButton.interactable = false;
                    reloadButton.interactable = false; backButton.interactable = false;
                    return;
                }
                if (session.Page != FightMatchHostPage.QuitConfirmation) contentPage = session.Page;
                var navigation = contentPage == FightMatchHostPage.Navigation;
                var battle = contentPage == FightMatchHostPage.Battle;
                var battleAvailable = battleView != null && !battleView.IsDisposed;
                if (battle && !battleAvailable)
                {
                    infrastructureDiagnostic = "BattleViewDisposed";
                    if (battlePage != null) battlePage.SetActive(false);
                    if (resultPage != null) resultPage.SetActive(false);
                    return;
                }
                if (!navigation && navigationView.Controller != null) navigationView.Unbind();
                if (!battle && battleAvailable && battleView.Controller != null) battleView.Unbind();
                if (navigation && navigationView.Controller != session.Navigation)
                { navigationPage.SetActive(false); navigationView.Bind(session.Navigation, localization); }
                if (battle && battleAvailable && battleView.Controller != session.Battle)
                { battlePage.SetActive(false); battleView.Bind(session.Battle, localization); }
                startupPage.SetActive(contentPage == FightMatchHostPage.Startup);
                navigationPage.SetActive(navigation); battlePage.SetActive(battle);
                if (!battle) resultPage.SetActive(false);
                quitPopup.SetActive(session.Page == FightMatchHostPage.QuitConfirmation);
                if (session.CanCreate) BindText(startupStatus, "fm.profile.empty.body");
                else if (session.Status == "ContinueCreation") BindText(startupStatus, "fm.profile.creation_pending.body");
                else if (session.Status == "UnclaimedData") BindText(startupStatus, "fm.profile.unclaimed_data.blocking");
                else if (session.Status == "CommitUnknown") BindText(startupStatus, "fm.profile.create_unknown.body");
                else if (session.Observation?.State == LocalPlayerProfileState.Active) BindText(startupStatus, "fm.profile.existing.body");
                else BindText(startupStatus, "fm.profile.load_failed.body", new KeyValuePair<string, string>("errorCode", session.Status ?? "ProfileStateUnavailable"));
                createButton.interactable = session.CanCreate;
                continueButton.interactable = session.OriginalProfile != null || session.Observation?.State == LocalPlayerProfileState.CreateIntentRecorded;
                reloadButton.interactable = true; backButton.interactable = true;
                backButton.gameObject.SetActive(!navigation || session.AtNavigationRoot);
            }
            finally { rendering = false; SynchronizeOverlayRoots(); RefreshDiagnostic(); }
        }
        private void OnLayoutValidityChanged(bool valid)
        { if (!valid && board != null) board.CancelPointer(); }
        internal void SynchronizeOverlayRoots()
        {
            if (confirmationPopupRoot == null || confirmationRootMask == null || navigationConfirmationPanel == null || battleConfirmationPanel == null ||
                recoveryScreenRoot == null || recoveryRootMask == null || navigationRecoveryPanel == null || battleRecoveryPanel == null) return;
            var nc = navigationConfirmationPanel.activeSelf; var bc = battleConfirmationPanel.activeSelf;
            var nr = navigationRecoveryPanel.activeSelf; var br = battleRecoveryPanel.activeSelf;
            overlayConflict = nc && bc || nr && br;
            var recovery = !overlayConflict && (nr ^ br);
            var confirmation = !overlayConflict && !recovery && (nc ^ bc);
            // Close lower blockers before enabling Recovery; the child views never write these outer roots.
            confirmationRootMask.SetActive(confirmation); confirmationPopupRoot.SetActive(confirmation);
            if (recovery || overlayConflict)
            {
                if (languageDropdown != null) languageDropdown.Close();
                SetLanguageFeedbackVisible(false);
                if (licensePopup != null) licensePopup.SetActive(false);
                if (quitPopup != null) quitPopup.SetActive(false);
            }
            recoveryRootMask.SetActive(recovery); recoveryScreenRoot.SetActive(recovery);
            if (bound && !CanSelectLocale() && languageDropdown != null && languageDropdown.IsExpanded) languageDropdown.Close();
            RefreshDiagnostic();
        }
        private void RefreshDiagnostic()
        {
            if (rendering || blockingDiagnostic == null) return;
            var code = DiagnosticCode;
            blockingDiagnostic.SetActive(code != null);
            if (code != null && languageDropdown != null) languageDropdown.Close();
            if (code != null && board != null) board.CancelPointer();
            if (bound && diagnosticText != null && code != null && diagnosticText.DiagnosticCode == null)
            {
                rendering = true;
                try { BindText(diagnosticText, "fm.diagnostic.missing_binding", new KeyValuePair<string, string>("errorCode", code)); }
                finally { rendering = false; }
            }
        }
        internal void ShowFailure(string code)
        {
            infrastructureDiagnostic = string.IsNullOrEmpty(code) ? "HostInitializationFailed" : code;
            if (loadingOverlay != null) loadingOverlay.SetActive(false);
            RefreshDiagnostic();
        }
        internal void SetLoading(bool loading)
        { if (loadingOverlay != null) loadingOverlay.SetActive(loading); }
        internal bool TryCloseLanguageDropdown()
        {
            if (!bound || languageDropdown == null || !languageDropdown.IsExpanded) return false;
            // TMP Cancel defers list destruction, so this also catches EventSystem running first.
            languageDropdown.Close();
            return true;
        }
        internal void PausePresentation() => PausePresentation(PointerCancellationCause.Cancelled);
        internal void PausePresentation(PointerCancellationCause cause)
        {
            if (languageDropdown != null) languageDropdown.Close();
            if (board != null) board.CancelPointer(cause);
            session?.PausePresentation();
        }
        private void Update()
        {
            if (!bound || languageDropdown == null) return;
            if (languageFeedbackDeadline.HasValue && Time.unscaledTime >= languageFeedbackDeadline.Value)
                SetLanguageFeedbackVisible(false);
            languageDropdown.interactable = CanSelectLocale();
            if (!languageDropdown.interactable && languageDropdown.IsExpanded) languageDropdown.Close();
        }
        public void Unbind() => Unbind(PointerCancellationCause.Cancelled);
        internal void Unbind(PointerCancellationCause cause)
        {
            bound = false; generation++; selectLocalePreference = null;
            if (languageDropdown != null)
            {
                languageDropdown.CanOpen = null;
                if (localeSelection != null) languageDropdown.onValueChanged.RemoveListener(localeSelection);
                languageDropdown.Close();
            }
            localeSelection = null;
            Canvas.willRenderCanvases -= SynchronizeOverlayRoots;
            if (!ReferenceEquals(responsiveLayout, null))
            {
                responsiveLayout.ValidityChanged -= OnLayoutValidityChanged;
                if (responsiveLayout != null) responsiveLayout.Unbind();
            }
            if (board != null) board.CancelPointer(cause);
            if (battleView != null) battleView.Unbind(cause);
            if (navigationView != null) navigationView.Unbind();
            foreach (var listener in listeners) if (listener.Key != null) listener.Key.onClick.RemoveListener(listener.Value);
            listeners.Clear();
            if (session != null) session.Changed -= Render;
            if (localization != null)
            {
                localization.DiagnosticsChanged -= RefreshDiagnostic;
                localization.LocaleChanged -= RefreshLocaleOptions;
            }
            foreach (var label in labels) if (label != null) label.Unbind();
            labels.Clear();
            if (startupStatus != null) startupStatus.Unbind();
            if (languageFeedback != null) languageFeedback.Unbind();
            if (diagnosticText != null) diagnosticText.Unbind();
            if (licenseBody != null) licenseBody.text = LocalizedTmpText.Placeholder;
            session = null; localization = null; infrastructureDiagnostic = null;
            overlayConflict = false;
            if (confirmationRootMask != null) confirmationRootMask.SetActive(false);
            if (confirmationPopupRoot != null) confirmationPopupRoot.SetActive(false);
            if (recoveryRootMask != null) recoveryRootMask.SetActive(false);
            if (recoveryScreenRoot != null) recoveryScreenRoot.SetActive(false);
            if (resultPage != null) resultPage.SetActive(false);
            SetLanguageFeedbackVisible(false);
            if (licensePopup != null) licensePopup.SetActive(false);
            if (quitPopup != null) quitPopup.SetActive(false);
            if (loadingOverlay != null) loadingOverlay.SetActive(false);
            if (blockingDiagnostic != null) blockingDiagnostic.SetActive(false);
        }
        private void OnDisable() { Unbind(PointerCancellationCause.FocusLost); }
        private void OnDestroy() { Unbind(); }
        public void Dispose() { Unbind(); }
    }
}
