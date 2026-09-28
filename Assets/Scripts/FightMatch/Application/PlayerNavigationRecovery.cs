using System;
using System.Linq;
using FightMatch.Core;
using FightMatch.Platform;

namespace FightMatch.Application
{
    public sealed partial class PlayerNavigationSession
    {
        private bool OriginalToken(PlayerNavigationToken token) => token != null &&
            ReferenceEquals(token.Owner, this) && token.Confirmation == confirmation;
        private bool HasRecovery() => read?.Application.PendingOperationId != null ||
            read?.Application.ObservedCandidateCommitIds.Count > 0 || operation.Intent != null ||
            read?.Application.Phase == CandidateApplicationPhase.RecoveryBlocked ||
            read?.Application.Phase == CandidateApplicationPhase.RestoreRequired;
        private string GateReason()
        {
            if (read == null) return "ResolutionRequired";
            if (player.CreationPending) return "CreationPending";
            if (!read.IsAvailable) return read.Code;
            if (read.Head?.Continuation != null) return "SettlementRequired";
            if (read.Application.PendingOperationId != null) return "ResolutionRequired";
            if (read.Application.ObservedCandidateCommitIds.Count != 0) return "ResolutionRequired";
            return null;
        }
        private static bool MenuOperation(CandidateApplicationKind kind) => kind == CandidateApplicationKind.SetFormation ||
            kind == CandidateApplicationKind.MigrateRoster || kind == CandidateApplicationKind.MigratePermanent ||
            kind == CandidateApplicationKind.PermanentRequest;
        private string EndReason()
        {
            if (player.CreationPending) return "CreationPending";
            if (read?.Head?.Continuation != null) return "SettlementRequired";
            if (operation.Intent != null)
                return MenuOperation(operation.Intent.Kind) ? null : "OriginalOwnerRequired";
            if (read?.Application.PendingOperationId != null) return "OriginalOwnerRequired";
            if (selectedCommit == null) return "CandidateSelectionRequired";
            // Without a retained head, the menu cannot rule out a reserved settlement.
            return read.Head == null ? "ResolutionRequired" : null;
        }
        private string ActionReason(PlayerNavigationAction action)
        {
            if (read == null) return "ResolutionRequired";
            if (OwnerRefusal(read.Code)) return read.Code;
            if (action == PlayerNavigationAction.Back || action == PlayerNavigationAction.Cancel) return null;
            if (action == PlayerNavigationAction.Refresh) return player.CreationPending ? "CreationPending" : null;
            if (action == PlayerNavigationAction.Return) return operation.HasVerifiedResult ? null : "ResultRequired";
            if (action == PlayerNavigationAction.SelectOriginalOperation)
                return selectedOperation != null && read.Application.IsPublishedHeadVerified ? null : "OperationSelectionRequired";
            if (action == PlayerNavigationAction.End) return player.CreationPending ? "CreationPending" : endConfirmation ? EndReason() : "EndConfirmationRequired";
            if (action == PlayerNavigationAction.Confirm)
                return endConfirmation ? "EndConfirmationRequired" : operation.Intent != null ? null :
                    frozenContext == null ? "PreviewRequired" : GateReason();
            if (action == PlayerNavigationAction.ResumeObserved)
                return player.CreationPending ? "CreationPending" : read.Application.PendingOperationId != null ? "OriginalOwnerRequired" :
                    selectedCommit == null ? "CandidateSelectionRequired" : read.Head?.Continuation != null ? "SettlementRequired" : null;
            if (action == PlayerNavigationAction.Retry || action == PlayerNavigationAction.Resolve)
            {
                if (player.CreationPending) return "CreationPending";
                if (operation.Intent == null) return "OriginalOwnerRequired";
                if (operation.HasVerifiedResult) return "ResultAvailable";
                if (action == PlayerNavigationAction.Resolve && read.Application.PendingOperationId != null &&
                    read.Application.PendingCommitId == null) return "RetryRequired";
                return null;
            }
            return "UnsupportedNavigationAction";
        }
        private void ReturnToAnchor()
        {
            parents.Clear();
            if (level != null && read?.Levels.Any(x => x.LevelId == level && x.LevelVersion == levelVersion) != true)
            { level = levelVersion = null; anchor = PlayerNavigationRoute.MapAdventure; }
            if (anchor == PlayerNavigationRoute.Preparation && level != null) parents.Add(PlayerNavigationRoute.MapAdventure);
            else if (anchor == PlayerNavigationRoute.Preparation) anchor = PlayerNavigationRoute.MapAdventure;
            else if (anchor == PlayerNavigationRoute.Team)
            { parents.Add(PlayerNavigationRoute.MapAdventure); if (level != null) parents.Add(PlayerNavigationRoute.Preparation); }
            route = anchor;
            if (route == PlayerNavigationRoute.Team) anchor = level == null ? PlayerNavigationRoute.MapAdventure : PlayerNavigationRoute.Preparation;
        }
        private void LeaveUnconfirmed(bool cancel)
        {
            if (endConfirmation) { endConfirmation = false; route = PlayerNavigationRoute.Recovery; }
            else if (operation.Intent != null)
                route = operation.HasVerifiedResult ? PlayerNavigationRoute.CommittedResult : PlayerNavigationRoute.Recovery;
            else
            {
                var confirmationPage = route == PlayerNavigationRoute.Confirmation;
                ClearDraft(); operation.Clear();
                if (cancel) ReturnToAnchor();
                else if (confirmationPage) route = draftPage;
                else if (parents.Count != 0)
                {
                    route = parents[parents.Count - 1]; parents.RemoveAt(parents.Count - 1);
                    if (route == PlayerNavigationRoute.MapAdventure) { level = levelVersion = null; anchor = route; }
                    else if (route == PlayerNavigationRoute.Team || route == PlayerNavigationRoute.Preparation)
                        anchor = level == null ? PlayerNavigationRoute.MapAdventure : PlayerNavigationRoute.Preparation;
                }
                else if (GateReason() == null) RequestHost(PlayerNavigationTargetKind.RootBackRequested);
            }
            revision++;
        }
        internal PlayerNavigationView Receive(PlayerNavigationToken token, CandidateApplicationCallResult actual)
        {
            if (!OriginalToken(token) || !operation.Receive(token.Confirmation, actual)) return Refuse("InconsistentBinding", "Navigation.Result");
            diagnostic = actual.Diagnostic ?? actual.NotificationFailure; status = actual.Code;
            route = operation.HasVerifiedResult ? PlayerNavigationRoute.CommittedResult : PlayerNavigationRoute.Recovery;
            revision++; return View();
        }
        private void ReceiveOriginal(CandidateApplicationCallResult actual)
        { Receive(new PlayerNavigationToken(this, revision, confirmation), actual); }

        public PlayerNavigationView Act(PlayerNavigationAction action, PlayerNavigationToken token, SaveStoreBudget budget)
        {
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            if (executing) return View("Busy");
            var current = Query(budget.Codec); if (OwnerRefusal(current.Read?.Code)) return current;
            if (!OriginalToken(token) || token.Revision != revision && !(action == PlayerNavigationAction.Confirm && operation.Intent != null))
                return Refuse("StaleNavigationToken");
            var reason = ActionReason(action); if (reason != null) return Refuse(reason);
            if (action == PlayerNavigationAction.Confirm)
            { Confirm(token, budget); return Query(budget.Codec); }
            if (action == PlayerNavigationAction.Back || action == PlayerNavigationAction.Cancel)
            { LeaveUnconfirmed(action == PlayerNavigationAction.Cancel); return Query(budget.Codec); }
            if (action == PlayerNavigationAction.Return)
            {
                lastResult = result;
                ClearDraft(); operation.Clear(); diagnostic = null; status = null; selectedOperation = null;
                route = PlayerNavigationRoute.ReturnRevalidate; ReturnToAnchor(); revision++; return Query(budget.Codec);
            }
            executing = true;
            try
            {
                if (action == PlayerNavigationAction.Retry || action == PlayerNavigationAction.Resolve)
                {
                    var original = application.QueryOperation(operation.Intent, budget);
                    if (original.OriginalLookup?.IsFound != true)
                        original = action == PlayerNavigationAction.Retry ? application.Retry(operation.Intent, budget) :
                            application.Resolve(operation.Intent, budget);
                    ReceiveOriginal(original);
                }
                else if (action == PlayerNavigationAction.ResumeObserved)
                {
                    var commit = selectedCommit;
                    var actual = application.ResumeObserved(commit, budget);
                    if (actual.View.PendingCommitId == commit && actual.View.PendingOperationId != null)
                    {
                        var captured = player.ReadNavigationResumedIntent(commit, actual.View.PendingOperationId, out var intent);
                        if (intent != null) { operation.Adopt(intent); ReceiveOriginal(actual); }
                        else { diagnostic = captured.Diagnostic; status = captured.Code; }
                    }
                    else if (actual.IsCommitted) FindCommitted(commit, actual, budget);
                    else { diagnostic = actual.Diagnostic; status = actual.Code; }
                    revision++;
                }
                else if (action == PlayerNavigationAction.SelectOriginalOperation)
                {
                    var records = read.Head.Records.Where(x => x.OperationId == selectedOperation).ToArray();
                    if (records.Length == 1)
                    { operation.Adopt(records[0].Intent); ReceiveOriginal(application.QueryOperation(operation.Intent, budget)); }
                    else Refuse("ResolutionRequired", "Navigation.OriginalRecord");
                }
                else if (action == PlayerNavigationAction.End)
                {
                    var actual = operation.Intent != null ? application.End(operation.Intent, budget) : application.EndObserved(selectedCommit, budget);
                    endConfirmation = false;
                    if (actual.Code == "Ended" && !actual.IsCommitted)
                    { ClearDraft(); operation.Clear(); selectedCommit = null; diagnostic = actual.Diagnostic; status = actual.Code; }
                    else if (operation.Intent != null) ReceiveOriginal(actual);
                    else if (actual.IsCommitted) FindCommitted(selectedCommit, actual, budget);
                    else { diagnostic = actual.Diagnostic; status = actual.Code; }
                    route = PlayerNavigationRoute.Recovery; revision++;
                }
                else if (action == PlayerNavigationAction.Refresh)
                {
                    var actual = operation.Intent != null ? application.QueryOperation(operation.Intent, budget) : application.Restore(budget);
                    if (operation.Intent != null) ReceiveOriginal(actual);
                    else { diagnostic = actual.Diagnostic; status = actual.Code; }
                    revision++;
                }
            }
            finally { executing = false; }
            return Query(budget.Codec);
        }
        private void FindCommitted(string commit, CandidateApplicationCallResult actual, SaveStoreBudget budget)
        {
            var records = actual.View.PublishedSnapshot?.Records.Where(x => x.CommitId == commit).ToArray();
            if (actual.View.IsPublishedHeadVerified && records?.Length == 1)
            { operation.Adopt(records[0].Intent); ReceiveOriginal(application.QueryOperation(operation.Intent, budget)); }
            else { diagnostic = actual.Diagnostic ?? new CandidateApplicationDiagnostic("ResolutionRequired", "Navigation.OriginalRecord"); status = diagnostic.Code; }
        }
    }
}
