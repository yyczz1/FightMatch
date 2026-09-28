using System;
using System.Linq;
using FightMatch.Application;
using UnityEngine.UIElements;
using NavigationView = FightMatch.Application.PlayerNavigationView;

namespace FightMatch.Presentation
{
    public sealed class PlayerNavigationRecoveryView : VisualElement
    {
        private readonly PlayerNavigationController controller;
        private readonly NavigationBindings bindings = new NavigationBindings();
        public PlayerNavigationRecoveryView(PlayerNavigationController controller)
        { this.controller = controller ?? throw new ArgumentNullException(nameof(controller)); name = "navigation-recovery"; }
        internal void ClearBindings() { bindings.Dispose(); }
        private void Action(NavigationView view, PlayerNavigationAction action, string text)
        { bindings.Button(this, "save-" + action, text, controller.ActionHandler(action), view.ReasonFor(action)); }
        private void Diagnostic(CandidateApplicationDiagnostic diagnostic, string name)
        {
            if (diagnostic == null) return;
            Add(new Label(diagnostic.Code + " / " + diagnostic.Stage + " / " + diagnostic.FieldPath + " / " +
                diagnostic.LimitReason + " (" + diagnostic.RequiredAtLeast + " / " + diagnostic.Allowed + ") " +
                diagnostic.ExceptionType + " " + diagnostic.ExceptionMessage) { name = name });
        }
        public void Render(NavigationView view)
        {
            bindings.Dispose(); Clear();
            var app = view.Read?.Application;
            Add(new Label("Current head: " + app?.PublishedSnapshot?.Header.CommitId +
                "; verified: " + app?.IsPublishedHeadVerified) { name = "save-head" });
            Add(new Label("Pending operation: " + app?.PendingOperationId + "; candidate: " + app?.PendingCommitId));
            Diagnostic(view.Diagnostic, "save-diagnostic");
            Diagnostic(view.Result?.NotificationFailure, "save-notification-failure");
            if (view.Route == PlayerNavigationRoute.Confirmation)
            {
                var confirmation = view.Confirmation;
                if (confirmation?.IsEndConfirmation == true)
                {
                    Add(new Label("End this unfinished save only if the save system confirms it was not committed. This cannot undo a completed transaction."));
                    Action(view, PlayerNavigationAction.End, "Confirm end of unfinished save");
                }
                else if (confirmation != null)
                {
                    Add(new Label(confirmation.Kind.ToString()));
                    PlayerPermanentDetailView.Quote(this, confirmation.Quote);
                    if (confirmation.Kind == PlayerNavigationDraftKind.Formation)
                    {
                        Add(new Label("Before: " + string.Join(" / ", view.Read.Roster.Slots.Select(x => x ?? "Empty"))));
                        Add(new Label("After: " + string.Join(" / ", confirmation.Slots.Select(x => x ?? "Empty"))));
                    }
                    if (confirmation.Kind == PlayerNavigationDraftKind.Migration)
                        Add(new Label(confirmation.FromFormat + " → " + confirmation.ToFormat));
                    Action(view, PlayerNavigationAction.Confirm, "Confirm");
                }
            }
            else if (view.Route == PlayerNavigationRoute.CommittedResult)
            {
                Add(new Label("Original committed operation: " + view.Result?.OriginalLookup?.Record.OperationId +
                    "; commit: " + view.Result?.OriginalCommitId + "; lookup head: " + view.Result?.LookupViewCommitId) { name = "save-original-result" });
                Action(view, PlayerNavigationAction.Return, "Return");
            }
            else
            {
                foreach (var commit in app?.ObservedCandidateCommitIds ?? Array.Empty<string>())
                    bindings.Button(this, "candidate-" + PlayerNavigationView.Key(commit), "Select candidate " + commit,
                        controller.NavigationHandler(new PlayerNavigationTarget { Kind = PlayerNavigationTargetKind.ObservedCandidate, CommitId = commit }));
                Action(view, PlayerNavigationAction.ResumeObserved, "Continue selected original save");
                Action(view, PlayerNavigationAction.Retry, "Retry original save");
                Action(view, PlayerNavigationAction.Resolve, "Resolve original outcome");
                Action(view, PlayerNavigationAction.Refresh, "Read save recovery state");
                bindings.Button(this, "save-end-review", "Review ending unfinished save",
                    controller.NavigationHandler(new PlayerNavigationTarget { Kind = PlayerNavigationTargetKind.EndConfirmation }));
            }
            if (app?.IsPublishedHeadVerified == true)
            {
                foreach (var record in app.PublishedSnapshot.Records)
                    bindings.Button(this, "operation-" + PlayerNavigationView.Key(record.OperationId), "Select original " + record.OperationId,
                        controller.NavigationHandler(new PlayerNavigationTarget { Kind = PlayerNavigationTargetKind.OriginalOperation, OperationId = record.OperationId }));
                Action(view, PlayerNavigationAction.SelectOriginalOperation, "Read selected original result");
            }
        }
    }
}
