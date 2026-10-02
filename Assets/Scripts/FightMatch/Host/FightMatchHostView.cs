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
        [SerializeField] private GameObject languagePopup, licensePopup, quitPopup, loadingOverlay, blockingDiagnostic;
        [SerializeField] private PlayerNavigationView navigationView;
        [SerializeField] private PlayerBattleView battleView;
        [SerializeField] private CandidateBoardElement board;
        [SerializeField] private FightMatchResponsiveLayout responsiveLayout;
        [SerializeField] private GameObject confirmationPopupRoot, confirmationRootMask, navigationConfirmationPanel, battleConfirmationPanel;
        [SerializeField] private GameObject recoveryScreenRoot, recoveryRootMask, navigationRecoveryPanel, battleRecoveryPanel;
        [SerializeField] private LocalizedTmpText profileTitle, startupStatus, languageTitle, languageFeedback;
        [SerializeField] private LocalizedTmpText licenseTitle, quitTitle, quitBody, loadingLabel, diagnosticText;
        [SerializeField] private TextMeshProUGUI licenseBody;
        [SerializeField] private UnityEngine.UI.Button createButton, continueButton, reloadButton, backButton;
        [SerializeField] private UnityEngine.UI.Button languageButton, languageEnglishButton, languageChineseButton, languageCloseButton;
        [SerializeField] private UnityEngine.UI.Button licenseButton, licenseCloseButton, quitConfirmButton, quitCancelButton;
        private readonly List<KeyValuePair<UnityEngine.UI.Button, UnityAction>> listeners = new List<KeyValuePair<UnityEngine.UI.Button, UnityAction>>();
        private readonly List<LocalizedTmpText> labels = new List<LocalizedTmpText>();
        private FightMatchHostSession session;
        private LocalizationService localization;
        private FightMatchHostPage contentPage = FightMatchHostPage.Startup;
        private long generation;
        private string infrastructureDiagnostic;
        private bool rendering, bound;
        private bool overlayConflict;
        public PlayerBattleView BattleView => battleView;
        internal PlayerNavigationView NavigationView => navigationView;
        internal string DiagnosticCode => infrastructureDiagnostic ?? (overlayConflict ? "OverlappingPanelOwners" : localization?.BindingDiagnostic);
        internal bool DiagnosticVisible => blockingDiagnostic != null && blockingDiagnostic.activeSelf;

        internal void Bind(FightMatchHostSession owner, LocalizationService service, string fontLicense)
        {
            Unbind();
            if (service == null) throw new ArgumentNullException(nameof(service));
            if (startupPage == null || navigationPage == null || battlePage == null || resultPage == null ||
                languagePopup == null || licensePopup == null || quitPopup == null || loadingOverlay == null || blockingDiagnostic == null ||
                navigationView == null || battleView == null || board == null || licenseBody == null || responsiveLayout == null ||
                confirmationPopupRoot == null || confirmationRootMask == null || navigationConfirmationPanel == null || battleConfirmationPanel == null ||
                recoveryScreenRoot == null || recoveryRootMask == null || navigationRecoveryPanel == null || battleRecoveryPanel == null ||
                profileTitle == null || startupStatus == null || languageTitle == null || languageFeedback == null ||
                licenseTitle == null || quitTitle == null || quitBody == null || loadingLabel == null || diagnosticText == null ||
                createButton == null || continueButton == null || reloadButton == null || backButton == null ||
                languageButton == null || languageEnglishButton == null || languageChineseButton == null || languageCloseButton == null ||
                licenseButton == null || licenseCloseButton == null || quitConfirmButton == null || quitCancelButton == null)
                throw new InvalidOperationException("Host serialized bindings are incomplete.");
            session = owner; localization = service; bound = true;
            responsiveLayout.ValidityChanged += OnLayoutValidityChanged;
            responsiveLayout.Bind(service);
            Canvas.willRenderCanvases -= SynchronizeOverlayRoots;
            Canvas.willRenderCanvases += SynchronizeOverlayRoots;
            if (!FightMatchViewId.Validate(transform, out var diagnostic)) infrastructureDiagnostic = diagnostic;
            if (!service.IsReady) infrastructureDiagnostic = "LocalizationNotReady";
            licenseBody.text = string.IsNullOrEmpty(fontLicense) ? LocalizedTmpText.Placeholder : fontLicense;
            if (string.IsNullOrEmpty(fontLicense)) infrastructureDiagnostic = "MissingFontLicense";
            BindText(profileTitle, "fm.profile.title");
            BindText(languageTitle, "fm.language.entry.label");
            BindText(licenseTitle, "fm.font_licenses.title");
            BindText(quitTitle, "fm.exit_app.title"); BindText(quitBody, "fm.exit_app.body");
            BindText(loadingLabel, "fm.common.state.loading");
            languageFeedback.gameObject.SetActive(false);
            AddButton(createButton, "fm.profile.create.button", () => session?.CreateProfile());
            AddButton(continueButton, "fm.profile.creation_pending.continue_button", () => session?.ContinueCreation());
            AddButton(reloadButton, "fm.profile.reload.button", () => session?.ObserveStartup());
            AddButton(backButton, "fm.common.action.back", () => session?.Back());
            AddButton(languageButton, "fm.language.entry.label", () => ShowLanguage(true));
            AddButton(languageEnglishButton, "fm.language.option.en", () => SelectLocale(LocaleId.En));
            AddButton(languageChineseButton, "fm.language.option.zh_cn", () => SelectLocale(LocaleId.ZhHans));
            AddButton(languageCloseButton, "fm.common.action.close", () => ShowLanguage(false));
            AddButton(licenseButton, "fm.font_licenses.open_button", () => ShowLicense(true));
            AddButton(licenseCloseButton, "fm.common.action.back", () => ShowLicense(false));
            AddButton(quitConfirmButton, "fm.exit_app.confirm_button", () => session?.ConfirmQuit());
            AddButton(quitCancelButton, "fm.exit_app.stay_button", () => session?.CancelQuit());
            if (session != null) session.Changed += Render;
            localization.DiagnosticsChanged += RefreshDiagnostic;
            languagePopup.SetActive(false); licensePopup.SetActive(false); loadingOverlay.SetActive(false);
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
            if (!bound || !languagePopup.activeSelf) return;
            localization.SetLocale(locale);
            BindText(languageFeedback, "fm.language.changed");
            languageFeedback.gameObject.SetActive(true);
        }
        private void ShowLanguage(bool show)
        {
            if (!bound || (show && (board.HasActivePointer || recoveryScreenRoot.activeSelf || overlayConflict))) return;
            languagePopup.SetActive(show);
            if (show) licensePopup.SetActive(false);
        }
        private void ShowLicense(bool show)
        {
            if (!bound || (show && (recoveryScreenRoot.activeSelf || overlayConflict))) return;
            if (show) { PausePresentation(); languagePopup.SetActive(false); }
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
                if (languagePopup != null) languagePopup.SetActive(false);
                if (licensePopup != null) licensePopup.SetActive(false);
                if (quitPopup != null) quitPopup.SetActive(false);
            }
            recoveryRootMask.SetActive(recovery); recoveryScreenRoot.SetActive(recovery);
            RefreshDiagnostic();
        }
        private void RefreshDiagnostic()
        {
            if (rendering || blockingDiagnostic == null) return;
            var code = DiagnosticCode;
            blockingDiagnostic.SetActive(code != null);
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
        internal void PausePresentation() => PausePresentation(PointerCancellationCause.Cancelled);
        internal void PausePresentation(PointerCancellationCause cause)
        {
            if (board != null) board.CancelPointer(cause);
            session?.PausePresentation();
        }
        private void Update()
        { if (bound && languageButton != null) languageButton.interactable = !board.HasActivePointer && DiagnosticCode == null; }
        public void Unbind() => Unbind(PointerCancellationCause.Cancelled);
        internal void Unbind(PointerCancellationCause cause)
        {
            bound = false; generation++;
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
            if (localization != null) localization.DiagnosticsChanged -= RefreshDiagnostic;
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
            if (languagePopup != null) languagePopup.SetActive(false);
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
