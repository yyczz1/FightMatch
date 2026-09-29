using System.Collections;
using System.Linq;
using FightMatch.Application;
using FightMatch.Presentation;
using NUnit.Framework;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using static FightMatch.Core.Tests.PlayerSessionTestData;

namespace FightMatch.Core.Tests
{
    public sealed class PlayerBattlePresentationTests
    {
        [UnityTest] public IEnumerator B09_ReadOnlyRefreshKeepsTheOriginalActionButtonAndContextClickable()
        {
            using (var r = new PlayerBattleRig()) using (var panel = new PlayerBattlePanel(r))
            {
                yield return panel.Ready(); var button = r.Page.Q<Button>("battle-exit"); var context = r.Host.View.Context;
                var head = r.Head; var files = r.N.Files(); var clocks = r.ClockReads;
                for (var i = 0; i < 3; i++) { r.Host.Input.Refresh(); r.Host.Refresh(); }
                Assert.AreSame(button, r.Page.Q<Button>("battle-exit")); Assert.AreSame(context, r.Host.View.Context);
                PlayerBattlePanel.Click(button); Assert.IsNotNull(r.Host.View.Confirmation); r.Unchanged(head, files, clocks);
            }
        }
        [UnityTest] public IEnumerator B09_AnotherPageOrHostDisposalDetachesEveryOldBorrowedCallback()
        {
            foreach (var mode in new[] { "new-page", "host-dispose" })
            using (var r = new PlayerBattleRig()) using (var panel = new PlayerBattlePanel(r))
            using (var replacement = new FightMatch.Presentation.PlayerBattleView())
            {
                yield return panel.Ready(); r.N.Storage.Fault = "snapshot-promoted"; r.Step(false);
                var oldPage = r.Page; var old = oldPage.Q<Button>("retry-save"); Assert.IsTrue(old.enabledSelf);
                var request = r.Host.Input.LastRequest; var head = r.Head; var files = r.N.Files(); var clocks = r.ClockReads;
                if (mode == "new-page") replacement.Bind(r.Host); else r.Host.Dispose();
                Assert.IsNull(oldPage.Controller, mode); panel.ClickStale(old); r.Unchanged(head, files, clocks);
                Assert.AreSame(request, r.Host.Input.LastRequest, mode);
            }
        }
        [UnityTest] public IEnumerator B06_B09_RecoveryPageEndsOnlyTheActualInputOwnedPendingRequest()
        {
            using (var r = new PlayerBattleRig()) using (var panel = new PlayerBattlePanel(r))
            {
                yield return panel.Ready(); r.N.Storage.Fault = "snapshot-before"; r.Step(false);
                var request = r.Host.Input.LastRequest; var head = r.Head; var clocks = r.ClockReads;
                Assert.AreEqual(request.OperationId, r.View.Read.Application.PendingOperationId);
                Assert.IsNull(r.Page.Q<Button>("query-battle-original")); Assert.IsNotNull(r.Page.Q<Button>("query-input-original"));
                PlayerBattlePanel.Click(r.Page.Q<Button>("end-input-original"));
                Assert.AreEqual("Ended", r.Host.Input.LastResult.Application.Code); Assert.IsNull(r.View.Read.Application.PendingOperationId);
                Assert.AreEqual(head.Header.CommitId, r.Head.Header.CommitId); Assert.AreEqual(head.Records.Count, r.Head.Records.Count);
                Assert.AreSame(request, r.Host.Input.LastRequest); Assert.AreEqual(clocks, r.ClockReads);
            }
        }
        [UnityTest] public IEnumerator B09_OriginalAttachRebuildsMemberButtonsForItsNewOwnedController()
        {
            using (var r = new PlaybackPanelRig())
            {
                yield return r.Ready(); var old = r.Host.Q<Button>("member-W"); var previous = r.Input;
                r.Host.Attach(r.Battle.System, Budget()); r.Resize(); yield return r.Ready();
                Assert.AreNotSame(previous, r.Input); Assert.IsNull(r.Input.SelectedCharacterId);
                r.Root.Add(old); PlayerBattlePanel.Click(old); old.RemoveFromHierarchy();
                Assert.IsNull(previous.SelectedCharacterId); Assert.IsNull(r.Input.SelectedCharacterId);
                PlayerBattlePanel.Click(r.Host.Q<Button>("member-W")); Assert.AreEqual("W", r.Input.SelectedCharacterId);
                Assert.IsNull(previous.SelectedCharacterId);
            }
        }
        [TestCase("snapshot-promoted")] [TestCase("marker-before")] [TestCase("marker-after")]
        public void B06_B09_EntireViewTreeRebuildPreservesHostInputPlaybackAndOriginalAttack(string fault)
        {
            using (var r = new PlayerBattleRig())
            {
                r.N.Storage.Fault = fault; var failed = r.Step(false); Assert.IsFalse(failed.Application.IsCommitted);
                var input = r.Host.Input; var playback = r.Host.Playback; var session = r.Session;
                var request = input.LastRequest; var intent = request.Intent; var bytes = intent.CanonicalBytes.ToArray();
                var view = r.View; var commit = view.Read.Application.PendingCommitId; var head = r.Head; var files = r.N.Files(); var clocks = r.ClockReads;
                var generation = playback.Generation; var starts = playback.Starts; var completions = playback.CompletionReports;
                Assert.IsNull(r.Page.Q<Button>("query-battle-original"), "Completed H02 is not the current attack recovery owner");
                r.RebuildView(); Assert.AreSame(input, r.Page.PlaybackView.InputView.Controller); Assert.AreSame(playback, r.Page.PlaybackView.Controller);
                Assert.AreSame(session, r.Session); Assert.AreSame(request, r.Host.Input.LastRequest); Assert.AreSame(intent, r.Host.Input.LastRequest.Intent);
                CollectionAssert.AreEqual(bytes, r.Host.Input.LastRequest.Intent.CanonicalBytes); Assert.AreEqual(commit, r.View.Read.Application.PendingCommitId);
                Assert.AreEqual(generation, playback.Generation); Assert.AreEqual(starts, playback.Starts); Assert.AreEqual(completions, playback.CompletionReports);
                r.Unchanged(head, files, clocks);
                var done = input.ResolveLast(); if (!done.Application.IsCommitted) done = input.RetryLast();
                Is(done.Application); Assert.AreEqual(commit, done.Application.OriginalCommitId); Assert.AreSame(request, input.LastRequest);
                Assert.AreEqual(head.Records.Count + 1, r.Head.Records.Count);
            }
        }
        [Test] public void B09_BorrowedDetachDoesNotFinishButExplicitCloseAndFinalHostDisposeReportOnce()
        {
            using (var r = new PlayerBattleRig())
            {
                Is(r.Step(false).Application); var playback = r.Host.Playback; var token = r.N.Battle.QueryView().PresentationToken;
                var original = playback.Original; var generation = playback.Generation; var reports = playback.CompletionReports;
                var head = r.Head; var files = r.N.Files(); var clocks = r.ClockReads;
                r.Page.Detach(); Assert.AreSame(original, playback.Original); Assert.AreEqual(generation, playback.Generation);
                Assert.AreEqual(reports, playback.CompletionReports); Assert.AreSame(token, r.N.Battle.QueryView().PresentationToken); r.Unchanged(head, files, clocks);
                r.Page.Bind(r.Host); Assert.AreSame(original, playback.Original); Assert.AreEqual(generation, playback.Generation);
                r.Page.ClosePresentation(); Assert.IsNull(r.N.Battle.QueryView().PresentationToken); Assert.AreEqual(reports + 1, playback.CompletionReports);
                r.Host.Dispose(); r.Host.Dispose(); Assert.AreEqual(1, r.Host.DisposeCount); Assert.AreEqual(reports + 1, playback.CompletionReports);
                r.Unchanged(head, files, clocks);
            }
        }
        [UnityTest] public IEnumerator B09_ActualPanelDetachAndOldConfirmationButtonCannotEndReboundBattle()
        {
            using (var r = new PlayerBattleRig()) using (var panel = new PlayerBattlePanel(r))
            {
                yield return panel.Ready(); var head = r.Head; var files = r.N.Files(); var clocks = r.ClockReads;
                PlayerBattlePanel.Click(r.Page.Q<Button>("battle-exit")); var old = r.Page.Q<Button>("confirm-battle-end"); Assert.IsNotNull(old);
                var original = r.View.Confirmation; r.RebuildView(); panel.ShowPage(); yield return panel.Ready();
                Assert.AreSame(original, r.View.Confirmation); panel.ClickStale(old); r.Unchanged(head, files, clocks);
                PlayerBattlePanel.Click(r.Page.Q<Button>("cancel-battle-end")); Assert.IsNull(r.View.Confirmation); r.Unchanged(head, files, clocks);
            }
        }
        [UnityTest] public IEnumerator B09_ActualBorrowedInputStaleRetryButtonCannotCommitAfterRebind()
        {
            using (var r = new PlayerBattleRig()) using (var panel = new PlayerBattlePanel(r))
            {
                yield return panel.Ready(); r.N.Storage.Fault = "snapshot-promoted"; r.Step(false);
                var old = r.Page.Q<Button>("retry-save"); Assert.IsTrue(old.enabledSelf);
                var head = r.Head; var files = r.N.Files(); var clocks = r.ClockReads; var request = r.Host.Input.LastRequest;
                r.RebuildView(); panel.ShowPage(); yield return panel.Ready(); panel.ClickStale(old); r.Unchanged(head, files, clocks);
                Assert.AreSame(request, r.Host.Input.LastRequest); PlayerBattlePanel.Click(r.Page.Q<Button>("retry-save"));
                Is(r.Host.Input.LastResult.Application); Assert.AreEqual(head.Records.Count + 1, r.Head.Records.Count);
            }
        }
        [UnityTest] public IEnumerator B02_ActualHistoryListButtonShowsExactRangeAndStaleConfirmCannotSelectAnother()
        {
            using (var r = PlayerBattleRig.TwoFaces()) using (var panel = new PlayerBattlePanel(r))
            {
                yield return panel.Ready(); Is(r.Step().Application); Is(r.Step().Application);
                var entries = r.Host.HistoryEntries; Assert.GreaterOrEqual(entries.Count, 2);
                PlayerBattlePanel.Click(r.Page.Q<Button>("history-" + entries[0].HistoryAnchorId)); var first = r.Page.DisplayedRange;
                var old = r.Page.Q<Button>("confirm-history"); Assert.AreSame(first, r.Host.Input.RollbackPreview);
                PlayerBattlePanel.Click(r.Page.Q<Button>("history-" + entries[1].HistoryAnchorId)); var second = r.Page.DisplayedRange;
                var head = r.Head; var files = r.N.Files(); var clocks = r.ClockReads;
                panel.ClickStale(old); Assert.AreSame(second, r.Host.Input.RollbackPreview); r.Unchanged(head, files, clocks);
                PlayerBattlePanel.Click(r.Page.Q<Button>("confirm-history")); Is(r.Host.Input.LastResult.Application);
                Assert.AreEqual(CandidateApplicationKind.Rollback, r.Host.Input.LastRequest.Kind);
            }
        }
        [Test] public void B04_ButtonAvailabilityTracksRealTokenAndActualReceiptAfterHistoryRemoved()
        {
            using (var r = new PlayerBattleRig())
            {
                r.Win(false); Assert.IsFalse(r.Page.Q<Button>("battle-settle").enabledSelf);
                StringAssert.Contains("待结算", r.Page.Q<Label>("battle-status").text); Assert.IsNull(r.Page.Q<Label>("receipt-title"));
                r.Host.Playback.SkipToFinal(); Assert.IsTrue(r.Page.Q<Button>("battle-settle").enabledSelf);
                var done = r.Settle(); Assert.IsNull(done.Battle.History); StringAssert.Contains("已到账", r.Page.Q<Label>("receipt-title").text);
                foreach (var m in done.Receipt.Members) StringAssert.Contains(m.CharacterId, r.Page.Q<Label>("receipt-member-" + m.CharacterId).text);
                StringAssert.Contains("原材料到账", r.Page.Q<Label>("receipt-inventory").text);
            }
        }
        [Test] public void B09_StalePageEpochAndDisposedHostCannotSubmitOrAcknowledgePlayback()
        {
            using (var r = new PlayerBattleRig())
            {
                var page = r.Page.Page; var context = r.View.Context; r.RebuildView(); var head = r.Head; var files = r.N.Files(); var clocks = r.ClockReads;
                r.Host.PreviewEnd(page, CandidateApplicationKind.ExitAttempt, context); Assert.IsNull(r.View.Confirmation);
                r.Host.Settle(page, context); r.Unchanged(head, files, clocks);
                Is(r.Step(false).Application); var token = r.N.Battle.QueryView().PresentationToken;
                r.Host.SkipPlayback(page); Assert.AreSame(token, r.N.Battle.QueryView().PresentationToken);
                var current = r.Page.Page; r.Host.Dispose(); head = r.Head; files = r.N.Files(); clocks = r.ClockReads;
                r.Host.PreviewEnd(current, CandidateApplicationKind.ExitAttempt, context); r.Host.AcceptHost(null); r.Unchanged(head, files, clocks);
                Assert.AreEqual(1, r.Host.DisposeCount);
            }
        }
    }
}
