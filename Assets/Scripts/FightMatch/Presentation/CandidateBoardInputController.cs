using System;
using System.Collections.Generic;
using System.Linq;
using FightMatch.Application;
using FightMatch.Core;
using FightMatch.Input;
using FightMatch.Platform;
using FlowPuzzle.Core;

namespace FightMatch.Presentation
{
    // Owns transient selection/gestures only. All mutations go through the injected 019 facade.
    public sealed class CandidateBoardInputController
    {
        private readonly CandidateBattleApplicationSystem system;
        private readonly SaveStoreBudget budget;
        private readonly RouteGesture gesture = new RouteGesture();
        private double threshold = 6;
        private CandidateDemoView pressedView;
        private CandidateDemoView confirmationView;
        private CandidateHistoryLocator confirmationLocator;
        private string confirmationAnchor;
        private CandidatePresentationToken deliveredToken;

        public CandidateDemoView View { get; private set; }
        public GestureContext InputContext { get; private set; }
        public GestureView Gesture => gesture.Read();
        public string SelectedCharacterId { get; private set; }
        public string Status { get; private set; }
        public PreparedCandidateBattleRequest LastRequest { get; private set; }
        public CandidateBattlePrepareResult LastPreparation { get; private set; }
        public CandidateBattlePreviewResult LastPreview { get; private set; }
        public CandidateBattleCallResult LastResult { get; private set; }
        public CandidateRollbackRange RollbackPreview { get; private set; }
        public bool CanRetry => OwnsPending && View.Phase == CandidateApplicationPhase.SaveFailed;
        public bool CanResolve => OwnsPending && View.Phase == CandidateApplicationPhase.CommitUnknown;
        private bool OwnsPending => LastRequest != null && View.ApplicationView.PendingOperationId == LastRequest.OperationId;
        public event Action Changed;
        public event Action<CandidateBattleCallResult> ResultReceived;
        public event Action<CandidateBattlePresentation> PresentationReady;
        public event Action<CandidateRollbackRange> RollbackConfirmationRequired;

        public CandidateBoardInputController(CandidateBattleApplicationSystem system, SaveStoreBudget budget)
        {
            this.system = system ?? throw new ArgumentNullException(nameof(system));
            this.budget = budget ?? throw new ArgumentNullException(nameof(budget));
            Refresh();
        }

        public CandidateDemoView Refresh()
        {
            Apply(system.QueryView());
            Changed?.Invoke();
            return View;
        }

        private void Apply(CandidateDemoView next)
        {
            var state = next.BattleSnapshot;
            if (View?.BattleSnapshot?.Baseline.Entry.AttemptId != state?.Baseline.Entry.AttemptId ||
                !IsLiving(state, SelectedCharacterId)) SelectedCharacterId = null;
            var context = Map(next);
            var changed = !SameContext(InputContext, context) || !SameHead(View, next) ||
                View.Attack.Reason != next.Attack.Reason || View.Link.Reason != next.Link.Reason || View.Rollback.Reason != next.Rollback.Reason;
            View = next;
            if (!changed) return;
            gesture.Cancel(); pressedView = null;
            InputContext = context;
            if (context != null) gesture.Bind(context, threshold);
            if (RollbackPreview != null && !SameHead(confirmationView, next))
            { ClearConfirmation(); Status = "StaleContext"; }
        }

        private GestureContext Map(CandidateDemoView view)
        {
            var state = view.BattleSnapshot;
            if (state == null || view.Context == null || view.CommitId == null || !view.PreferenceRevision.HasValue) return null;
            var board = state.Board; var face = board.Face;
            var link = state.Phase == BattlePhase.AwaitLinks;
            var attack = view.Attack.IsAvailable && view.RequiredAttackItemUseEnabledFor(SelectedCharacterId).HasValue && IsLiving(state, SelectedCharacterId);
            var pairs = new List<GesturePair>();
            foreach (var pair in face.Pairs)
            {
                var locked = board.LockedRoutes.FirstOrDefault(x => x.PairKey.PairId == pair.PairId);
                var canDraw = locked == null && (link ? view.Link.IsAvailable && board.PendingLinks.Any(x => x.PairId == pair.PairId) :
                    attack && state.Enemies.Any(x => x.PairKey.PairId == pair.PairId && x.Hp.Numerator.Sign > 0));
                pairs.Add(new GesturePair(pair.PairId, pair.EndpointA, pair.EndpointB, canDraw, locked?.Route ?? Array.Empty<FlowPos>()));
            }
            return new GestureContext(state.Baseline.Entry.AttemptId, state.SceneRevision, view.PreferenceRevision.Value,
                face.FaceId, face.Width, face.Height, pairs.Any(x => x.CanDraw) || view.Rollback.IsAvailable,
                view.Rollback.IsAvailable, link ? GestureMode.FreeLink : GestureMode.Attack, link ? null : SelectedCharacterId, pairs);
        }

        private static bool IsLiving(BattleSnapshot state, string id)
        { return id != null && state != null && state.Members.Any(x => x.Member.CharacterId == id && x.Hp.Numerator.Sign > 0); }

        private static bool SameHead(CandidateDemoView a, CandidateDemoView b)
        {
            return a != null && b != null && a.CommitId == b.CommitId && a.Phase == b.Phase &&
                a.IsPublishedHeadVerified == b.IsPublishedHeadVerified && a.PreferenceRevision == b.PreferenceRevision &&
                a.BattleSnapshot?.Baseline.Entry.AttemptId == b.BattleSnapshot?.Baseline.Entry.AttemptId &&
                a.BattleSnapshot?.SceneRevision == b.BattleSnapshot?.SceneRevision;
        }

        private static bool SameContext(GestureContext a, GestureContext b)
        {
            if (a == null || b == null) return a == b;
            if (a.AttemptId != b.AttemptId || a.SceneRevision != b.SceneRevision || a.PreferenceRevision != b.PreferenceRevision ||
                a.FaceId != b.FaceId || a.Width != b.Width || a.Height != b.Height || a.Enabled != b.Enabled ||
                a.AllowHistoryTap != b.AllowHistoryTap || a.Mode != b.Mode || a.SelectedCharacterId != b.SelectedCharacterId || a.Pairs.Count != b.Pairs.Count) return false;
            for (var i = 0; i < a.Pairs.Count; i++)
            {
                var x = a.Pairs[i]; var y = b.Pairs[i];
                if (x.PairId != y.PairId || !x.EndpointA.Equals(y.EndpointA) || !x.EndpointB.Equals(y.EndpointB) ||
                    x.CanDraw != y.CanDraw || !x.LockedCells.SequenceEqual(y.LockedCells)) return false;
            }
            return true;
        }

        public bool SelectMember(string characterId)
        {
            Refresh();
            if (characterId != null && !IsLiving(View.BattleSnapshot, characterId)) return false;
            if (SelectedCharacterId == characterId) return true;
            SelectedCharacterId = characterId;
            CancelGesture(); ClearConfirmation(); Apply(View); Changed?.Invoke();
            return true;
        }

        public void CancelGesture()
        { gesture.Cancel(); pressedView = null; Changed?.Invoke(); }

        internal void Down(PointerSample sample, double dragThreshold)
        {
            Refresh();
            if (Gesture.ActivePointerId.HasValue || RollbackPreview != null || InputContext == null) return;
            if (threshold != dragThreshold) { threshold = dragThreshold; gesture.Bind(InputContext, threshold); }
            gesture.Down(sample);
            if (Gesture.ActivePointerId == sample.PointerId) { pressedView = View; Status = null; }
            Changed?.Invoke();
        }

        internal void Move(PointerSample sample)
        {
            Refresh(); gesture.Move(sample);
            if (!Gesture.ActivePointerId.HasValue) pressedView = null;
            Changed?.Invoke();
        }

        internal void Up(PointerSample sample)
        {
            Refresh();
            var basis = pressedView;
            var intent = gesture.Up(sample);
            if (!Gesture.ActivePointerId.HasValue) pressedView = null;
            Changed?.Invoke();
            if (intent == null || basis == null) return;
            if (intent.Kind == GestureIntentKind.TapLocator) { Preview(basis, intent); return; }
            var draft = Draft(basis);
            var pair = BattlePairKey.Create(intent.Context.AttemptId, intent.Context.FaceId, intent.PairId);
            if (intent.Kind == GestureIntentKind.AttackRoute)
            {
                var actor = basis.BattleSnapshot.Members.Single(x => x.Member.CharacterId == intent.CharacterId);
                draft.Kind = CandidateApplicationKind.Attack;
                draft.Attack = new CandidateApplicationAttackInput { AttemptId = intent.Context.AttemptId,
                    ExpectedSceneRevision = intent.Context.SceneRevision, Actor = actor.CombatantKey, Pair = pair, Route = intent.Cells.ToList(),
                    ExpectedPreferenceRevision = basis.PreferenceRevision, ItemUseEnabled = basis.RequiredAttackItemUseEnabledFor(SelectedCharacterId) };
            }
            else
            {
                draft.Kind = CandidateApplicationKind.Link;
                draft.Link = new CandidateApplicationLinkInput { AttemptId = intent.Context.AttemptId,
                    ExpectedSceneRevision = intent.Context.SceneRevision, Pair = pair, Route = intent.Cells.ToList() };
            }
            Submit(draft);
        }

        private void Preview(CandidateDemoView basis, GestureIntent intent)
        {
            var locator = new CandidateHistoryLocator { PlayerId = basis.ApplicationView.PlayerId, AttemptId = intent.Context.AttemptId,
                ExpectedSceneRevision = intent.Context.SceneRevision, Pair = BattlePairKey.Create(intent.Context.AttemptId, intent.Context.FaceId, intent.PairId),
                Kind = intent.OriginKind == OriginKind.Endpoint ? CandidateHistoryLocatorKind.EndpointLatestAttack : CandidateHistoryLocatorKind.LockedRouteKillingAttack };
            LastPreview = system.PreviewRollback(locator, basis.CommitId, budget.Codec);
            Status = LastPreview.Code; Apply(LastPreview.View);
            if (!LastPreview.IsAccepted) { Changed?.Invoke(); return; }
            RollbackPreview = LastPreview.Range; confirmationView = basis; confirmationLocator = locator;
            if (RollbackPreview.Entries.Count == 1) { ConfirmRollback(); return; }
            Status = "RollbackConfirmationRequired"; Changed?.Invoke();
            RollbackConfirmationRequired?.Invoke(RollbackPreview);
        }

        public CandidateBattleCallResult ConfirmRollback()
        {
            var range = RollbackPreview; var basis = confirmationView; var locator = confirmationLocator;
            Refresh();
            if (range == null || RollbackPreview != range || !SameHead(basis, View) || !View.Rollback.IsAvailable)
            { ClearConfirmation(); Status = "StaleContext"; Changed?.Invoke(); return null; }
            var checkedRange = confirmationAnchor == null ? system.PreviewRollback(locator, basis.CommitId, budget.Codec) :
                system.PreviewHistoryRollback(confirmationAnchor, basis.CommitId, budget.Codec);
            if (!checkedRange.IsAccepted || checkedRange.Range.AttemptId != range.AttemptId || checkedRange.Range.SceneRevision != range.SceneRevision ||
                checkedRange.Range.OperationId != range.OperationId || checkedRange.Range.HistoryAnchorId != range.HistoryAnchorId ||
                !checkedRange.Range.Entries.Select(x => x.OperationId).SequenceEqual(range.Entries.Select(x => x.OperationId)))
            { ClearConfirmation(); Status = "StaleContext"; Changed?.Invoke(); return null; }
            var draft = Draft(basis); draft.Kind = CandidateApplicationKind.Rollback;
            draft.Rollback = new CandidateApplicationRollbackInput { AttemptId = range.AttemptId, ExpectedSceneRevision = range.SceneRevision,
                HistoryAnchorId = range.HistoryAnchorId, TargetOperationId = range.OperationId,
                ConfirmedRemovedOperationIds = range.Entries.Select(x => x.OperationId).ToList() };
            ClearConfirmation();
            return Submit(draft);
        }

        public void CancelRollback() { ClearConfirmation(); Status = null; Changed?.Invoke(); }
        private void ClearConfirmation() { RollbackPreview = null; confirmationView = null; confirmationLocator = null; confirmationAnchor = null; }

        public CandidateBattlePreviewResult SelectHistoryAnchor(string anchor, string expectedCommitId)
        {
            Refresh();
            LastPreview = system.PreviewHistoryRollback(anchor, expectedCommitId, budget.Codec);
            Status = LastPreview.Code; Apply(LastPreview.View);
            if (!LastPreview.IsAccepted) { Changed?.Invoke(); return LastPreview; }
            CancelGesture(); ClearConfirmation();
            RollbackPreview = LastPreview.Range; confirmationView = View; confirmationAnchor = anchor;
            Status = "RollbackConfirmationRequired"; Changed?.Invoke();
            RollbackConfirmationRequired?.Invoke(RollbackPreview); return LastPreview;
        }

        public CandidateBattleCallResult ConfirmRollback(CandidateRollbackRange expected)
        {
            if (expected == null || !ReferenceEquals(expected, RollbackPreview))
            { Status = "StaleContext"; Changed?.Invoke(); return null; }
            return ConfirmRollback();
        }

        private static CandidateBattleDraft Draft(CandidateDemoView basis)
        {
            var c = basis.Context;
            return new CandidateBattleDraft { PlayerId = basis.ApplicationView.PlayerId, ExpectedCommitId = basis.CommitId,
                Context = RuleContextChecks.Copy(c) };
        }

        private CandidateBattleCallResult Submit(CandidateBattleDraft draft)
        {
            LastPreparation = system.Prepare(draft, budget.Codec);
            if (!LastPreparation.IsAccepted) { Status = LastPreparation.Code; Refresh(); return null; }
            LastRequest = LastPreparation.Request;
            return Receive(system.Submit(LastRequest, budget));
        }

        public CandidateBattleCallResult RetryLast() { return LastRequest == null ? null : Receive(system.Retry(LastRequest, budget)); }
        public CandidateBattleCallResult ResolveLast() { return LastRequest == null ? null : Receive(system.Resolve(LastRequest, budget)); }
        public CandidateBattleCallResult EndLast() { return LastRequest == null ? null : Receive(system.End(LastRequest, budget)); }
        public CandidateBattleCallResult QueryLastOperation() { return LastRequest == null ? null : Receive(system.QueryOperation(LastRequest, budget)); }
        public CandidateBattleCallResult RebuildLatest() { CancelGesture(); ClearConfirmation(); return Receive(system.RebuildLatest()); }

        private CandidateBattleCallResult Receive(CandidateBattleCallResult result)
        {
            LastResult = result; Status = result.Code; Apply(result.View); Changed?.Invoke();
            var token = result.Presentation?.Token;
            if (result.PresentationDisposition == CandidatePresentationDisposition.PlayOriginal && token != null &&
                (deliveredToken == null || deliveredToken.AttemptId != token.AttemptId || deliveredToken.SceneRevision != token.SceneRevision || deliveredToken.OperationId != token.OperationId))
            { deliveredToken = token; PresentationReady?.Invoke(result.Presentation); }
            ResultReceived?.Invoke(result);
            return result;
        }
    }
}
