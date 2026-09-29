using System;
using System.Collections.Generic;
using System.Linq;
using FightMatch.Application;
using FightMatch.Core;
using FightMatch.Platform;
using BattleView = FightMatch.Application.PlayerBattleView;

namespace FightMatch.Presentation
{
    public sealed class PlayerBattlePageToken
    {
        internal PlayerBattleController Owner { get; }
        internal long Epoch { get; }
        internal PlayerBattlePageToken(PlayerBattleController owner, long epoch) { Owner = owner; Epoch = epoch; }
    }

    // Owned by the application host, never by a VisualElement tree.
    public sealed class PlayerBattleController : IDisposable
    {
        private readonly SaveStoreBudget budget;
        private readonly Func<CandidateTimeSample> clock;
        private long pageEpoch;
        private bool disposed, refreshing;
        private CandidateRollbackRange publishedRange;
        private PreparedCandidateBattleRequest publishedInputRequest;
        public PlayerBattleSession Session { get; }
        public CandidateBoardInputController Input { get; }
        public CandidateBattlePlaybackController Playback { get; }
        public BattleView View { get; private set; }
        public bool IsDisposed => disposed;
        public int DisposeCount { get; private set; }
        public event Action Changed;
        public IReadOnlyList<CandidateHistoryEntry> HistoryEntries => Input.View.History == null ? Array.Empty<CandidateHistoryEntry>() :
            Input.View.History.EffectiveAnchors.Select(a => Input.View.History.Archive.Single(x => x.HistoryAnchorId == a)).ToList().AsReadOnly();

        public PlayerBattleController(PlayerSessionSystem player, CandidateBattleApplicationSystem battle, SaveStoreBudget budget,
            Func<CandidateTimeSample> clock = null)
        {
            if (player == null || battle == null || budget == null) throw new ArgumentNullException();
            this.budget = budget; this.clock = clock ?? new LocalPlayerClock().Read;
            Session = player.GetBattleSession(); Input = new CandidateBoardInputController(battle, budget);
            Playback = new CandidateBattlePlaybackController(Input, battle);
            Input.Changed += OnChanged; Playback.Changed += OnChanged;
            View = Session.Query(budget.Codec);
        }
        public PlayerBattlePageToken BindPage()
        { if (disposed) throw new ObjectDisposedException(nameof(PlayerBattleController));
            var page = new PlayerBattlePageToken(this, ++pageEpoch); Changed?.Invoke(); return page; }
        public void DetachPage(PlayerBattlePageToken page) { if (Owns(page)) pageEpoch++; }
        public bool Owns(PlayerBattlePageToken page) => !disposed && page != null && ReferenceEquals(page.Owner, this) && page.Epoch == pageEpoch;
        private void OnChanged()
        {
            if (disposed || refreshing) return;
            Publish(Session.Query(budget.Codec));
        }
        public BattleView Refresh()
        {
            if (disposed) return View;
            BattleView actual; refreshing = true;
            try { Input.Refresh(); actual = Session.Query(budget.Codec); }
            finally { refreshing = false; }
            return Publish(actual);
        }
        private BattleView Receive(BattleView actual)
        {
            refreshing = true;
            try { Input.Refresh(); }
            finally { refreshing = false; }
            return Publish(actual);
        }
        private BattleView Publish(BattleView actual)
        {
            // Preserve the page's callbacks and any pointer-down while a periodic read changes no displayed facts.
            if (View != null && View.Context.Revision == actual.Context.Revision && ReferenceEquals(View.Context.Head, actual.Context.Head) &&
                View.Route == actual.Route && View.Status == actual.Status && View.SettlementReason == actual.SettlementReason &&
                ReferenceEquals(View.Confirmation, actual.Confirmation) && ReferenceEquals(View.OriginalIntent, actual.OriginalIntent) &&
                ReferenceEquals(View.Result, actual.Result) && ReferenceEquals(View.Receipt, actual.Receipt) &&
                ReferenceEquals(View.Battle.PresentationToken, actual.Battle.PresentationToken) &&
                ReferenceEquals(publishedRange, Input.RollbackPreview) && ReferenceEquals(publishedInputRequest, Input.LastRequest)) return View;
            View = actual; publishedRange = Input.RollbackPreview; publishedInputRequest = Input.LastRequest;
            Changed?.Invoke(); return View;
        }
        public BattleView AcceptHost(PlayerNavigationHostRequest request)
        { return disposed ? View : Receive(Session.AcceptHost(request, clock, budget)); }
        public BattleView PreviewEnd(PlayerBattlePageToken page, CandidateApplicationKind kind, PlayerBattleContext context)
        { return !Owns(page) ? View : Receive(Session.PreviewEnd(kind, context, budget.Codec)); }
        public BattleView Confirm(PlayerBattlePageToken page, PlayerBattleConfirmation confirmation)
        { return !Owns(page) ? View : Receive(Session.Confirm(confirmation, clock, budget)); }
        public BattleView Cancel(PlayerBattlePageToken page, PlayerBattleConfirmation confirmation)
        { return !Owns(page) ? View : Receive(Session.Cancel(confirmation, budget.Codec)); }
        public BattleView Settle(PlayerBattlePageToken page, PlayerBattleContext context)
        { return !Owns(page) ? View : Receive(Session.Settle(context, clock, budget)); }
        public BattleView Continue(PlayerBattlePageToken page, PlayerBattleRecoveryAction action,
            PreparedCandidateApplicationIntent original, PlayerBattleContext context)
        { return !Owns(page) ? View : Receive(Session.Continue(action, original, context, budget)); }
        public BattleView ResumeObserved(PlayerBattlePageToken page, string commit, PlayerBattleContext context)
        { return !Owns(page) ? View : Receive(Session.ResumeObserved(commit, context, budget)); }
        public BattleView OpenOriginalResult(string operation, PlayerBattleContext context)
        { return disposed ? View : Receive(Session.OpenOriginalResult(operation, context, budget)); }
        public BattleView ReturnTo(PlayerBattlePageToken page, PlayerNavigationTargetKind target, PlayerBattleContext context)
        { return !Owns(page) ? View : Receive(Session.ReturnTo(target, context, budget.Codec)); }
        public BattleView Replay(PlayerBattlePageToken page, string level, string version, PlayerBattleContext context)
        { return !Owns(page) ? View : Receive(Session.Replay(level, version, context, clock, budget)); }
        public CandidateBattlePreviewResult SelectHistory(PlayerBattlePageToken page, string anchor, PlayerBattleContext context)
        {
            if (!Owns(page) || !Current(context)) return null;
            return Input.SelectHistoryAnchor(anchor, context.CommitId);
        }
        private bool Current(PlayerBattleContext context)
        {
            var current = Session.Query(budget.Codec);
            return context != null && current.Context.Revision == context.Revision && ReferenceEquals(context.Head, current.Context.Head) &&
                ReferenceEquals(context, View.Context) && current.Status != "Busy" && current.Status != "WrongThread" && current.Status != "Disposed";
        }
        public CandidateBattleCallResult ConfirmHistory(PlayerBattlePageToken page, CandidateRollbackRange range)
        { return Owns(page) ? Input.ConfirmRollback(range) : null; }
        public void CancelHistory(PlayerBattlePageToken page, CandidateRollbackRange range)
        { if (Owns(page) && range != null && ReferenceEquals(Input.RollbackPreview, range)) Input.CancelRollback(); }
        public PlayerDefaultReferenceResult ReadReference(PlayerBattlePageToken page, string level, string version, ContentBinding binding)
        {
            if (!Owns(page)) return null;
            return Session.ReadDefaultReference(level, version, binding, View.Context, budget.Codec);
        }
        public void SkipPlayback(PlayerBattlePageToken page) { if (Owns(page)) Playback.SkipToFinal(); }
        public void RebuildLatest(PlayerBattlePageToken page) { if (Owns(page)) Input.RebuildLatest(); }
        public void Dispose()
        {
            if (disposed) return;
            disposed = true; pageEpoch++; DisposeCount++; Changed?.Invoke();
            Input.Changed -= OnChanged; Playback.Changed -= OnChanged;
            Playback.Dispose();
        }
    }
}
