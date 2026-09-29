using System;
using System.Linq;
using FightMatch.Core;
using FightMatch.Platform;
using QFramework;

namespace FightMatch.Application
{
    public sealed partial class CandidateBattleApplicationSystem : AbstractSystem
    {
        private readonly CandidateApplicationSystem application;
        private CandidatePresentationToken presentationToken;
        private string pendingPresentationOperation;
        private bool executing;
        internal CandidateBattleApplicationSystem(CandidateApplicationSystem application) { this.application = application; }
        protected override void OnInit() { }

        private static bool Refused(CandidateApplicationCallResult query)
        { return query.Code == "WrongThread" || query.Code == "Disposed"; }

        public CandidateBattlePrepareResult Prepare(CandidateBattleDraft draft, SaveCodecBudget budget)
        {
            var query = application.QueryView();
            if (Refused(query)) return new CandidateBattlePrepareResult(null, query.Diagnostic);
            if (executing) return new CandidateBattlePrepareResult(null, new CandidateApplicationDiagnostic("Busy", "Application"));
            return CandidateBattleRequestFactory.Prepare(draft, budget);
        }

        public CandidateBattleCallResult Submit(PreparedCandidateBattleRequest request, SaveStoreBudget budget, int maxRandomWords = 4096)
        {
            var query = application.QueryView();
            if (Refused(query)) return Wrap(query);
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            if (maxRandomWords < 0) throw new ArgumentOutOfRangeException(nameof(maxRandomWords));
            var build = new BuildObservation();
            return Invoke(() => application.SubmitTrusted(request.Intent,
                (basis, intent, codec) => Build(basis, request, codec, maxRandomWords, build), budget), build);
        }

        public CandidateBattleCallResult Retry(PreparedCandidateBattleRequest request, SaveStoreBudget budget)
        { return Continue(request, budget, application.Retry); }

        public CandidateBattleCallResult Resolve(PreparedCandidateBattleRequest request, SaveStoreBudget budget)
        { return Continue(request, budget, application.Resolve); }

        public CandidateBattleCallResult End(PreparedCandidateBattleRequest request, SaveStoreBudget budget)
        { return Continue(request, budget, application.End); }

        private CandidateBattleCallResult Continue(PreparedCandidateBattleRequest request, SaveStoreBudget budget,
            Func<PreparedCandidateApplicationIntent, SaveStoreBudget, CandidateApplicationCallResult> action)
        {
            var query = application.QueryView();
            if (Refused(query)) return Wrap(query);
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            return Invoke(() => action(request.Intent, budget));
        }

        private CandidateBattleCallResult Invoke(Func<CandidateApplicationCallResult> action, BuildObservation build = null)
        {
            // The original runtime rejects nested mutations. Do not disturb the outer admission.
            if (executing) return Wrap(action());
            CandidateApplicationCallResult result;
            executing = true;
            try { result = action(); }
            finally { executing = false; }
            return Wrap(result, build?.Rejection, true);
        }

        public CandidateBattleCallResult QueryOperation(PreparedCandidateBattleRequest request, SaveStoreBudget budget)
        {
            var query = application.QueryView();
            if (Refused(query)) return Wrap(query);
            if (request == null) throw new ArgumentNullException(nameof(request));
            return Wrap(application.QueryOperation(request.Intent, budget));
        }

        public CandidateDemoView QueryView()
        {
            var query = application.QueryView();
            if (!Refused(query)) Synchronize(query.View.PublishedSnapshot);
            return new CandidateDemoView(query, presentationToken, executing);
        }

        public CandidateBattlePreviewResult PreviewRollback(CandidateHistoryLocator locator, string expectedCommitId, SaveCodecBudget budget)
        {
            var view = QueryView();
            if (view.Code == "WrongThread" || view.Code == "Disposed")
                return new CandidateBattlePreviewResult(view, null, view.Diagnostic);
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            if (!view.IsPublishedHeadVerified || view.Phase != CandidateApplicationPhase.Ready)
                return PreviewRejected(view, "ResolutionRequired", "Application.View");
            if (view.CommitId != expectedCommitId) return PreviewRejected(view, "StaleContext", "ExpectedCommitId");
            if (view.History == null) return PreviewRejected(view, "NoActiveBattle", "ActiveHistory");
            try
            {
                var located = CandidateHistoryOperations.Locate(view.History, locator, budget.Math);
                if (!located.IsAccepted) return PreviewRejected(view, located.RejectionCode.ToString(), located.FieldPath);
                var range = CandidateHistoryOperations.ReadRange(view.History, new CandidateHistoryRangeRequest
                {
                    PlayerId = located.Range.PlayerId, AttemptId = located.Range.AttemptId,
                    ExpectedSceneRevision = located.Range.SceneRevision, HistoryAnchorId = located.Range.HistoryAnchorId
                }, budget.Math);
                return range.IsAccepted ? new CandidateBattlePreviewResult(view, range.Range) :
                    PreviewRejected(view, range.RejectionCode.ToString(), range.FieldPath);
            }
            catch (ExactMathLimitException error)
            { return new CandidateBattlePreviewResult(view, null, CandidateApplicationDiagnostic.From("Limit", "Preview", error)); }
        }

        private static CandidateBattlePreviewResult PreviewRejected(CandidateDemoView view, string code, string path)
        { return new CandidateBattlePreviewResult(view, null, new CandidateApplicationDiagnostic(code, path, stage: "Preview")); }

        public CandidateBattlePreviewResult PreviewHistoryRollback(string anchor, string expectedCommitId, SaveCodecBudget budget)
        {
            var view = QueryView();
            if (view.Code == "WrongThread" || view.Code == "Disposed" || view.Code == "Busy")
                return PreviewRejected(view, view.Code, "Application");
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            if (!view.IsPublishedHeadVerified || view.Phase != CandidateApplicationPhase.Ready)
                return PreviewRejected(view, "ResolutionRequired", "Application.View");
            if (view.CommitId != expectedCommitId) return PreviewRejected(view, "StaleContext", "ExpectedCommitId");
            if (!view.Rollback.IsAvailable) return PreviewRejected(view, view.Rollback.Reason, "Rollback");
            if (anchor == null || !view.History.EffectiveAnchors.Contains(anchor))
                return PreviewRejected(view, "StaleContext", "HistoryAnchorId");
            try
            {
                var range = CandidateHistoryOperations.ReadRange(view.History, new CandidateHistoryRangeRequest {
                    PlayerId = view.ApplicationView.PlayerId, AttemptId = view.BattleSnapshot.Baseline.Entry.AttemptId,
                    ExpectedSceneRevision = view.BattleSnapshot.SceneRevision, HistoryAnchorId = anchor }, budget.Math);
                return range.IsAccepted ? new CandidateBattlePreviewResult(view, range.Range) :
                    PreviewRejected(view, range.RejectionCode.ToString(), range.FieldPath);
            }
            catch (ExactMathLimitException error)
            { return new CandidateBattlePreviewResult(view, null, CandidateApplicationDiagnostic.From("Limit", "Preview", error)); }
        }

        public CandidateBattleCallResult ReportPresentationCompleted(CandidatePresentationToken token)
        {
            var query = application.QueryView();
            if (Refused(query)) return Wrap(query);
            if (executing) return new CandidateBattleCallResult("Busy", query, new CandidateDemoView(query, presentationToken, true));
            Synchronize(query.View.PublishedSnapshot);
            var matched = presentationToken != null && presentationToken.Matches(token);
            if (matched) presentationToken = null;
            return new CandidateBattleCallResult(matched ? "PresentationCompleted" : "PresentationIgnored", query,
                new CandidateDemoView(query, presentationToken, false));
        }

        public CandidateBattleCallResult RebuildLatest()
        {
            var query = application.QueryView();
            if (Refused(query)) return Wrap(query);
            if (executing) return new CandidateBattleCallResult("Busy", query, new CandidateDemoView(query, presentationToken, true));
            presentationToken = null;
            pendingPresentationOperation = null;
            var view = new CandidateDemoView(query, null, false);
            return new CandidateBattleCallResult(view.IsPublishedHeadVerified ? "RebuildLatest" : "ResolutionRequired", query, view,
                disposition: view.IsPublishedHeadVerified ? CandidatePresentationDisposition.RebuildLatest : CandidatePresentationDisposition.None);
        }

        private void Synchronize(CandidateApplicationSnapshot snapshot)
        {
            var battle = snapshot?.Business.ActiveHistory?.CurrentRun.CurrentSnapshot;
            if (presentationToken != null && (battle == null || presentationToken.AttemptId != battle.Baseline.Entry.AttemptId ||
                presentationToken.SceneRevision != battle.SceneRevision)) presentationToken = null;
        }

        private CandidateBattleCallResult Wrap(CandidateApplicationCallResult result,
            CandidateBattleDomainRejection rejection = null, bool canDeliver = false)
        {
            var disposition = CandidatePresentationDisposition.None;
            CandidateBattlePresentation presentation = null;
            if (!Refused(result))
            {
                var snapshot = result.View.PublishedSnapshot;
                Synchronize(snapshot);
                var ready = result.View.IsPublishedHeadVerified && result.View.Phase == CandidateApplicationPhase.Ready;
                if (result.IsCommitted && ready) disposition = CandidatePresentationDisposition.RebuildLatest;
                var lookup = result.OriginalLookup;
                var record = lookup?.BattleOperation;
                var run = snapshot?.Business.ActiveHistory?.CurrentRun;
                if (canDeliver && ready && result.IsCommitted && result.LookupViewCommitId == snapshot.Header.CommitId &&
                    pendingPresentationOperation != null && lookup?.Record.OperationId == pendingPresentationOperation &&
                    lookup.Relation == CandidateApplicationRelation.Effective && record != null && run != null &&
                    run.Records.Count > 0 && run.Records[run.Records.Count - 1].OperationId == record.OperationId &&
                    run.CurrentSnapshot.SceneRevision == record.AfterSnapshot.SceneRevision &&
                    run.Baseline.Entry.AttemptId == record.AfterSnapshot.Baseline.Entry.AttemptId)
                {
                    presentationToken = new CandidatePresentationToken(run.Baseline.Entry.AttemptId,
                        run.CurrentSnapshot.SceneRevision, record.OperationId);
                    pendingPresentationOperation = null;
                    presentation = new CandidateBattlePresentation(result.OriginalCommitId, record, presentationToken);
                    disposition = CandidatePresentationDisposition.PlayOriginal;
                }
                if (canDeliver && pendingPresentationOperation != null &&
                    result.View.PendingOperationId != pendingPresentationOperation) pendingPresentationOperation = null;
            }
            return new CandidateBattleCallResult(rejection?.Code ?? result.Code, result,
                new CandidateDemoView(result, presentationToken, executing), rejection, disposition, presentation);
        }
    }
}
