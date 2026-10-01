using System;
using System.Collections.Generic;
using System.Linq;
using FightMatch.Application;
using FightMatch.Core;
using UnityEngine;
using NavigationView = FightMatch.Application.PlayerNavigationView;

namespace FightMatch.Presentation
{
    public sealed class PlayerNavigationRecoveryView : MonoBehaviour, IDisposable
    {
        [SerializeField] private GameObject confirmationRoot;
        [SerializeField] private GameObject recoveryRoot;
        [SerializeField] private RectTransform confirmationRows;
        [SerializeField] private RectTransform recoveryRows;
        [SerializeField] private LocalizedTmpText title;
        [SerializeField] private LocalizedTmpText status;
        [SerializeField] private UnityEngine.UI.Button confirmButton;
        [SerializeField] private UnityEngine.UI.Button endButton;
        [SerializeField] private UnityEngine.UI.Button returnButton;
        [SerializeField] private UnityEngine.UI.Button resumeObservedButton;
        [SerializeField] private UnityEngine.UI.Button retryButton;
        [SerializeField] private UnityEngine.UI.Button resolveButton;
        [SerializeField] private UnityEngine.UI.Button refreshButton;
        [SerializeField] private UnityEngine.UI.Button endReviewButton;
        [SerializeField] private UnityEngine.UI.Button originalResultButton;
        [SerializeField] private UnityEngine.UI.Button buttonTemplate;
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
        private void Action(NavigationView view, UnityEngine.UI.Button button, PlayerNavigationAction action, string key)
        {
            var currentGeneration = generation;
            var intent = controller.ActionHandler(action);
            bindings.Button(button, localization, key, () => { if (currentGeneration == generation) intent(); }, view.ReasonFor(action),
                reasonDomain: NavigationReasonDomain.Recovery);
        }
        private void Row(Transform parent, string identity, string key, params KeyValuePair<string, string>[] args)
        { bindings.Row(textTemplate, parent, FightMatchViewId.Row(identity), localization, key, args); }
        internal void Render(NavigationView view)
        {
            bindings.Dispose();
            confirmationRoot.SetActive(false); recoveryRoot.SetActive(false);
            foreach (var button in new[] { confirmButton, endButton, returnButton, resumeObservedButton, retryButton,
                resolveButton, refreshButton, endReviewButton, originalResultButton }) button.gameObject.SetActive(false);
            if (controller == null || view.Read?.Application.Phase == CandidateApplicationPhase.Disposed || view.Route != PlayerNavigationRoute.Confirmation &&
                view.Route != PlayerNavigationRoute.Recovery && view.Route != PlayerNavigationRoute.CommittedResult) return;
            var confirmation = view.Route == PlayerNavigationRoute.Confirmation;
            var parent = confirmation ? confirmationRows : recoveryRows;
            title.transform.SetParent(parent, false); status.transform.SetParent(parent, false);
            originalResultButton.transform.SetParent(parent, false);
            var app = view.Read?.Application;
            var value = view.Confirmation;
            var permanentKey = value?.Quote == null ? null : NavigationBindings.OperationKey(CandidateApplicationKind.PermanentRequest,
                value.Quote.Kind, value.Quote.Kind == CandidatePermanentKind.Equip && value.Quote.DefinitionId == null);
            var verified = view.Route == PlayerNavigationRoute.CommittedResult && NavigationBindings.VerifiedResult(view.Result);
            var titleKey = verified ? "fm.save_result.title" : view.Route == PlayerNavigationRoute.Recovery || value?.IsEndConfirmation == true ? "fm.save_recovery.title" :
                value?.Kind == PlayerNavigationDraftKind.Formation ? "fm.party.change_preview.title" :
                value?.Kind == PlayerNavigationDraftKind.Migration ? "fm.profile.data_upgrade.title" :
                permanentKey == null ? null : "fm.inventory.change_preview.title";
            title.gameObject.SetActive(titleKey != null);
            if (titleKey == "fm.inventory.change_preview.title")
                bindings.DynamicText(title, localization, titleKey, () => new[] {
                    NavigationBindings.Arg("operationName", NavigationBindings.Resolve(localization, permanentKey)) });
            else if (titleKey != null) bindings.Text(title, localization, titleKey);
            bindings.States(status, textTemplate, parent, localization, view, "save");
            // Published head identity is never a player row (N-P02).
            var operationKey = PendingOperationKey(view);
            if (app?.PendingOperationId != null && operationKey != null)
            {
                bindings.DynamicRow(textTemplate, parent, FightMatchViewId.Row("save.Pending"), localization,
                    "fm.save_recovery.operation_summary", () => new[] {
                        NavigationBindings.Arg("operationName", NavigationBindings.Resolve(localization, operationKey)) });
            }
            if (confirmation)
            {
                if (value?.IsEndConfirmation == true)
                {
                    Row(parent, "save.EndConfirmation", "fm.save_recovery.end_uncommitted_confirm");
                    if (view.ReasonFor(PlayerNavigationAction.End) == null)
                        Action(view, endButton, PlayerNavigationAction.End, "fm.save_recovery.end_uncommitted_button");
                }
                else if (value != null)
                {
                    PlayerPermanentDetailView.Quote(bindings, textTemplate, parent, localization, value.Quote);
                    if (value.Kind == PlayerNavigationDraftKind.Formation)
                        bindings.DynamicRow(textTemplate, parent, FightMatchViewId.Row("save.FormationChange"), localization,
                            "fm.party.change_preview.body", () => new[] {
                                NavigationBindings.Arg("beforeParty", NavigationBindings.Party(localization, view.Read.Roster.Slots)),
                                NavigationBindings.Arg("afterParty", NavigationBindings.Party(localization, value.Slots)) });
                    if (value.Kind == PlayerNavigationDraftKind.Migration) Row(parent, "save.Migration", "fm.profile.data_upgrade.body");
                    Action(view, confirmButton, PlayerNavigationAction.Confirm, value.Kind == PlayerNavigationDraftKind.Formation ?
                        "fm.party.change.confirm_button" : "fm.common.action.confirm");
                }
            }
            else if (view.Route == PlayerNavigationRoute.CommittedResult)
            {
                if (verified)
                {
                    var savedKey = NavigationBindings.OperationKey(view.Result.OriginalLookup.Record.Intent);
                    if (savedKey != null)
                        bindings.DynamicRow(textTemplate, parent, FightMatchViewId.Row("save.ConfirmedResult"), localization,
                            "fm.save_result.confirmed", () => new[] {
                                NavigationBindings.Arg("operationName", NavigationBindings.Resolve(localization, savedKey)) });
                    if (NavigationBindings.PreferenceSaved(view.Result)) Row(parent, "save.Preference", "fm.inventory.preference.saved");
                }
                Action(view, returnButton, PlayerNavigationAction.Return, "fm.common.action.back");
            }
            else
            {
                var candidates = (app?.ObservedCandidateCommitIds ?? Array.Empty<string>()).OrderBy(x => x, StringComparer.Ordinal).ToArray();
                for (var i = 0; i < candidates.Length; i++)
                {
                    var commit = candidates[i]; var number = i + 1;
                    bindings.CloneButton(buttonTemplate, parent, FightMatchViewId.Row("save.candidate." + commit), localization, "fm.save_recovery.candidate_row",
                        controller.NavigationHandler(new PlayerNavigationTarget { Kind = PlayerNavigationTargetKind.ObservedCandidate, CommitId = commit }),
                        captionArgs: () => new[] { NavigationBindings.Arg("candidateNumber", number) });
                }
                Action(view, resumeObservedButton, PlayerNavigationAction.ResumeObserved, "fm.save_recovery.continue_candidate_button");
                Action(view, retryButton, PlayerNavigationAction.Retry, "fm.save_recovery.retry_button");
                Action(view, resolveButton, PlayerNavigationAction.Resolve, "fm.save_recovery.confirm_result_button");
                Action(view, refreshButton, PlayerNavigationAction.Refresh, "fm.save_recovery.refresh_button");
                bindings.Button(endReviewButton, localization, "fm.save_recovery.review_end_button", controller.NavigationHandler(new PlayerNavigationTarget { Kind = PlayerNavigationTargetKind.EndConfirmation }));
            }
            if (app?.IsPublishedHeadVerified == true)
            {
                foreach (var record in app.PublishedSnapshot.Records)
                {
                    var recordKey = NavigationBindings.OperationKey(record.Intent);
                    if (recordKey == null) continue;
                    bindings.CloneButton(buttonTemplate, parent, FightMatchViewId.Row("save.operation." + record.OperationId), localization,
                        recordKey,
                        controller.NavigationHandler(new PlayerNavigationTarget { Kind = PlayerNavigationTargetKind.OriginalOperation, OperationId = record.OperationId }));
                }
                Action(view, originalResultButton, PlayerNavigationAction.SelectOriginalOperation, "fm.save_recovery.lookup_button");
            }
            (confirmation ? confirmationRoot : recoveryRoot).SetActive(true);
        }
        internal static string PendingOperationKey(NavigationView view)
        {
            var pending = view.Read?.Application.PendingOperationId;
            var value = view.Confirmation;
            if (pending == null || value?.OperationId != pending) return null;
            return NavigationBindings.OperationKey(value.OriginalKind, value.OriginalPermanentKind, value.OriginalClearsEquipment);
        }
        public void Unbind()
        {
            generation++;
            bindings.Dispose();
            controller = null; localization = null;
            if (confirmationRoot != null) confirmationRoot.SetActive(false);
            if (recoveryRoot != null) recoveryRoot.SetActive(false);
        }
        public void Dispose() { Unbind(); }
        private void OnDestroy() { Unbind(); }
    }
}
