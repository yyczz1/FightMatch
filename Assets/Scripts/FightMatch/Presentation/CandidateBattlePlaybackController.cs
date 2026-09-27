using System;
using FightMatch.Application;
using FightMatch.Core;

namespace FightMatch.Presentation
{
    public sealed class CandidateBattlePlaybackController : IDisposable
    {
        public const double FactMilliseconds = 180; // Technical trial value, not an accepted feel measurement.
        private readonly CandidateBoardInputController input;
        private readonly CandidateBattleApplicationSystem system;
        private CandidateBattlePresentation original;
        private bool changing, disposed;
        private double elapsed;
        private int nextIndex;
        public CandidateBattlePlaybackFrame Frame { get; private set; }
        public CandidateDemoView LatestView { get; private set; }
        public CandidateBattlePresentation Original => original;
        public bool IsPlaying => original != null;
        public long Generation { get; private set; }
        public int Starts { get; private set; }
        public int CompletionReports { get; private set; }
        public CandidatePresentationToken LastReportedToken { get; private set; }
        public string Diagnostic { get; private set; }
        public event Action Changed;

        public CandidateBattlePlaybackController(CandidateBoardInputController input, CandidateBattleApplicationSystem system)
        {
            this.input = input ?? throw new ArgumentNullException(nameof(input));
            this.system = system ?? throw new ArgumentNullException(nameof(system));
            LatestView = system.QueryView(); Frame = CandidateBattlePlaybackFrame.From(LatestView.BattleSnapshot);
            input.PresentationReady += OnPresentation; input.ResultReceived += OnResult; input.Changed += OnInputChanged;
        }
        private static bool Same(CandidatePresentationToken a, CandidatePresentationToken b)
        { return a != null && b != null && a.AttemptId == b.AttemptId && a.SceneRevision == b.SceneRevision && a.OperationId == b.OperationId; }
        private static bool Current(CandidateDemoView view, CandidatePresentationToken token)
        { return view.IsPublishedHeadVerified && view.Phase == CandidateApplicationPhase.Ready && view.Code != "Busy" && Same(view.PresentationToken, token) &&
            view.BattleSnapshot?.Baseline.Entry.AttemptId == token.AttemptId && view.BattleSnapshot?.SceneRevision == token.SceneRevision; }

        private void OnPresentation(CandidateBattlePresentation presentation)
        {
            if (disposed || changing) return;
            var result = input.LastResult;
            if (result?.PresentationDisposition != CandidatePresentationDisposition.PlayOriginal || !ReferenceEquals(result.Presentation, presentation)) return;
            LatestView = system.QueryView();
            if (!Current(LatestView, presentation.Token)) { OnInputChanged(); return; }
            if (original != null && Same(original.Token, presentation.Token)) return;
            if (original != null) Finish("Replaced");
            var record = result.Application.OriginalLookup?.BattleOperation;
            if (record == null || record.OperationId != presentation.Token.OperationId ||
                !ReferenceEquals(record.OrderedFacts, presentation.OrderedFacts) || !ReferenceEquals(record.BeforeSnapshot, presentation.BeforeSnapshot) ||
                !ReferenceEquals(record.AfterSnapshot, presentation.AfterSnapshot)) return;
            original = presentation; Frame = CandidateBattlePlaybackFrame.Initial(presentation, record.Request.Route);
            nextIndex = 0; elapsed = 0; Diagnostic = null; Generation++; Starts++;
            Changed?.Invoke();
            if (presentation.OrderedFacts.Count == 0) Finish("Completed");
        }
        private void OnResult(CandidateBattleCallResult result)
        {
            if (disposed || changing) return;
            if (result.PresentationDisposition == CandidatePresentationDisposition.PlayOriginal) { OnPresentation(result.Presentation); return; }
            if (result.PresentationDisposition == CandidatePresentationDisposition.RebuildLatest)
            {
                // A duplicate/current query may require acknowledging a waiting token even without local progress.
                var current = system.QueryView(); var token = current.PresentationToken; var record = result.Application.OriginalLookup?.BattleOperation;
                var ownsResult = token != null && record != null && record.OperationId == token.OperationId &&
                    record.AfterSnapshot.Baseline.Entry.AttemptId == token.AttemptId && record.AfterSnapshot.SceneRevision == token.SceneRevision;
                if (original != null && ownsResult && Same(original.Token, token)) Finish("RebuildLatest");
                else if (ownsResult && Current(current, token)) Finish("RebuildLatest", token);
                else OnInputChanged();
                return;
            }
            OnInputChanged();
        }
        private void OnInputChanged()
        {
            if (disposed || changing) return;
            LatestView = system.QueryView();
            if (original != null)
            {
                var state = LatestView.BattleSnapshot;
                if (state?.Baseline.Entry.AttemptId != original.Token.AttemptId || state.SceneRevision != original.Token.SceneRevision ||
                    !Same(LatestView.PresentationToken, original.Token)) { Finish("StalePresentation"); return; }
                // A same Attempt/Scene permanent commit or preference refresh does not finish this playback.
            }
            else Frame = CandidateBattlePlaybackFrame.From(LatestView.BattleSnapshot);
            Changed?.Invoke();
        }
        public void Advance(double deltaMilliseconds)
        {
            if (double.IsNaN(deltaMilliseconds) || double.IsInfinity(deltaMilliseconds) || deltaMilliseconds < 0)
                throw new ArgumentOutOfRangeException(nameof(deltaMilliseconds));
            if (disposed || changing || original == null) return;
            OnInputChanged(); if (original == null) return;
            // One initial committed beat, then one full interval for each original fact, including the last.
            elapsed += deltaMilliseconds;
            while (original != null && elapsed >= FactMilliseconds)
            {
                elapsed -= FactMilliseconds;
                if (nextIndex == original.OrderedFacts.Count) { Finish("Completed"); break; }
                Frame = Frame.Apply(original, original.OrderedFacts[nextIndex++]); Changed?.Invoke();
            }
        }
        public void SkipToFinal() { if (!disposed) Finish("Skipped"); }
        public void ReportSchedulingFailure(Exception error)
        {
            if (error == null) throw new ArgumentNullException(nameof(error));
            if (disposed) return;
            Diagnostic = error.GetType().FullName + ": " + error.Message; Finish("PlaybackError");
        }
        private void Finish(string reason, CandidatePresentationToken waiting = null)
        {
            if (changing) return;
            changing = true;
            try
            {
                var token = original?.Token ?? waiting;
                original = null; elapsed = 0; Generation++;
                LatestView = system.QueryView(); Frame = CandidateBattlePlaybackFrame.From(LatestView.BattleSnapshot, reason);
                input.CancelGesture(); input.Refresh();
                Changed?.Invoke(); // The host clears its overlay before the original token is reported.
                if (token != null)
                { LastReportedToken = token; CompletionReports++; system.ReportPresentationCompleted(token); }
                LatestView = input.Refresh(); Frame = CandidateBattlePlaybackFrame.From(LatestView.BattleSnapshot, reason);
                Changed?.Invoke();
            }
            finally { changing = false; }
        }
        public void Dispose()
        {
            if (disposed) return;
            input.PresentationReady -= OnPresentation; input.ResultReceived -= OnResult; input.Changed -= OnInputChanged;
            Finish("Closed"); disposed = true;
        }
    }
}
