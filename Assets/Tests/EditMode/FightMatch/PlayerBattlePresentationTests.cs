using System.Collections;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using FightMatch.Application;
using FightMatch.Presentation;
using NUnit.Framework;
using UnityEngine.TestTools;
using static FightMatch.Core.Tests.PlayerSessionTestData;

namespace FightMatch.Core.Tests
{
    public sealed class PlayerBattlePresentationTests
    {
        [UnityTest]
        public IEnumerator UGUI_COPY_D02_DurationUsesAcceptedSnapshotAndInvariantCeiling()
        {
            var values = new[] { ExactRational.Create(0, 1, new ExactMathBudget()), ExactRational.Create(1, 3, new ExactMathBudget()),
                ExactRational.Create(999, 1, new ExactMathBudget()), ExactRational.Create(1000, 1, new ExactMathBudget()),
                ExactRational.Create(60000, 1, new ExactMathBudget()), ExactRational.Create(7384000, 1, new ExactMathBudget()),
                ExactRational.Create(BigInteger.Parse("3600000000000000000000000", CultureInfo.InvariantCulture), 1, new ExactMathBudget()) };
            var expected = new[] { "0:00", "0:01", "0:01", "0:01", "1:00", "123:04", "60000000000000000000:00" };
            var service = new LocalizationService(new UguiTestTextSource(), UnityEngine.SystemLanguage.English);
            var previousCulture = CultureInfo.CurrentCulture;
            try
            {
                for (var i = 0; i < values.Length; i++)
                {
                    byte[] first = null;
                    foreach (var locale in new[] { LocaleId.ZhHans, LocaleId.En })
                    {
                        CultureInfo.CurrentCulture = new CultureInfo(locale == LocaleId.En ? "en-US" : "zh-CN"); service.SetLocale(locale);
                        var parameter = BattleText.Duration(values[i]); Assert.AreEqual(expected[i], parameter);
                        var bytes = Encoding.UTF8.GetBytes(parameter);
                        if (first == null) first = bytes; else CollectionAssert.AreEqual(first, bytes);
                        var text = service.Resolve("fm.party.member.recovering", new[] { BattleText.Arg("remainingTime", parameter) });
                        Assert.IsTrue(text.IsSuccess); StringAssert.Contains(parameter, text.Text);
                    }
                }
            }
            finally { CultureInfo.CurrentCulture = previousCulture; }
            Assert.IsNull(BattleText.Duration(null)); Assert.IsNull(BattleText.Duration(ExactRational.Create(-1, 1, new ExactMathBudget())));
            var tooLarge = BigInteger.One << new ExactMathBudget().MaxIntegerBits;
            foreach (var invalid in new[] { ExactRational.Create(tooLarge, 1, new ExactMathBudget(maxIntegerBits: 32769)),
                ExactRational.Create(1, tooLarge, new ExactMathBudget(maxIntegerBits: 32769)) })
                Assert.IsNull(BattleText.Duration(invalid));
            using (var r = PlayerBattleRig.Recoverable())
            {
                yield return r.Ready();
                r.Host.Input.SelectMember("W"); r.Draw(r.Route(r.State.Enemies[1].PairKey)); Is(r.Host.Input.LastResult.Application);
                r.Host.Playback.SkipToFinal(); var ended = r.End(); Is(ended.Result); var receipt = ended.Receipt;
                var member = r.Head.Business.Roster.Find("W"); var period = member.ActiveRecovery;
                Assert.IsFalse(member.IsReady); Assert.IsNotNull(period); Assert.AreEqual(0, period.Elapsed.Numerator.Sign);
                var remaining = BattleText.Duration(period.Duration.Subtract(period.Elapsed, new ExactMathBudget()));
                var fullDuration = BattleText.Duration(period.Duration); var head = r.Head; var files = r.N.Files(); var clocks = r.ClockReads;
                r.Elapsed = 1000000; // A later clock sample is available, but render must never read it.
                foreach (var locale in new[] { LocaleId.En, LocaleId.ZhHans })
                {
                    r.Host.Input.Refresh(); r.Host.Refresh(); r.Canvas.Localization.SetLocale(locale);
                    BattleCopyAssert.Localized(r.Page, "battle-hud:member:W:recovery", r.Canvas.Localization,
                        "fm.party.member.recovering", BattleText.Arg("remainingTime", remaining));
                    var historical = r.Canvas.Localization.Resolve("fm.result.recovery_started", new[] { BattleText.Arg("duration", fullDuration) });
                    Assert.IsTrue(historical.IsSuccess);
                    BattleCopyAssert.Localized(r.Page, "receipt-fallen:W", r.Canvas.Localization, "fm.result.member_fallen",
                        BattleText.Arg("characterName", BattleText.Character(r.Canvas.Localization, "W")), BattleText.Arg("recoveryStatus", historical.Text));
                    BattleCopyAssert.Localized(r.Page, "receipt-slot:W", r.Canvas.Localization, "fm.result.member.original_slot",
                        BattleText.Arg("characterName", BattleText.Character(r.Canvas.Localization, "W")),
                        BattleText.Arg("slotNumber", BattleText.Number(receipt.Members.Single().OriginalSlot + 1)));
                    Assert.AreSame(period, r.Head.Business.Roster.Find("W").ActiveRecovery); r.Unchanged(head, files, clocks);
                }
                r.Enter(); var complete = r.Head.Business.Roster.Find("W").RecoveryPeriods.Single(x => x.RecoveryId == period.RecoveryId);
                Assert.IsTrue(r.Head.Business.Roster.Find("W").IsReady);
                Assert.AreEqual("0:00", BattleText.Duration(complete.Duration.Subtract(complete.Elapsed, new ExactMathBudget())));
                r.Host.OpenOriginalResult(receipt.OperationId, r.View.Context);
                var historicalAfter = r.Canvas.Localization.Resolve("fm.result.recovery_started", new[] { BattleText.Arg("duration", fullDuration) });
                BattleCopyAssert.Localized(r.Page, "receipt-fallen:W", r.Canvas.Localization, "fm.result.member_fallen",
                    BattleText.Arg("characterName", BattleText.Character(r.Canvas.Localization, "W")), BattleText.Arg("recoveryStatus", historicalAfter.Text));
                var invalidBinding = r.Page.Find<LocalizedTmpText>(FightMatchViewId.Row("battle-status"));
                invalidBinding.Bind(r.Canvas.Localization, "fm.party.member.recovering", BattleText.Arg("remainingTime", BattleText.Duration(null)));
                Assert.IsNotNull(invalidBinding.DiagnosticCode); Assert.AreEqual(LocalizedTmpText.Placeholder, invalidBinding.Target.text);
            }
        }

        [UnityTest]
        public IEnumerator UGUI_COPY_WIRE03_ExistingHistorySaveSkipAndEndKeysFollowRealStateWithoutChecking()
        {
            using (var r = new PlayerBattleRig())
            {
                Assert.IsNull(r.Page.Find<LocalizedTmpText>(FightMatchViewId.Row("history-title")));
                yield return r.Ready();
                Is(r.Step(false).Application);
                Assert.AreEqual("fm.battle.playback.skip_button", r.Page.Find<UnityEngine.UI.Button>("fm.action.battle.skip").GetComponentInChildren<LocalizedTmpText>(true).Key);
                r.Host.Playback.SkipToFinal();
                BattleCopyAssert.Localized(r.Page, "history-title", r.Canvas.Localization, "fm.history.title");
                r.N.Storage.Fault = "snapshot-before"; var failed = r.Step(false); Assert.IsFalse(failed.Application.IsCommitted);
                var input = r.Host.Input; var request = input.LastRequest; var pending = r.View.Read.Application.PendingOperationId;
                Assert.AreEqual(request.OperationId, pending); Assert.AreNotEqual(r.View.OriginalIntent.OperationId, pending);
                var head = r.Head; var files = r.N.Files(); var clocks = r.ClockReads; var page = r.Page.Page;
                foreach (var locale in new[] { LocaleId.ZhHans, LocaleId.En })
                {
                    r.Canvas.Localization.SetLocale(locale); Assert.AreSame(page, r.Page.Page);
                    BattleCopyAssert.Localized(r.Page, "history-title", r.Canvas.Localization, "fm.history.title");
                    BattleCopyAssert.Localized(r.Page, "history-blocked-by-save", r.Canvas.Localization, "fm.history.undo.blocked_by_save");
                    BattleCopyAssert.Localized(r.Page, "battle-recovery-operation", r.Canvas.Localization, "fm.save_recovery.operation_summary",
                        BattleText.Arg("operationName", BattleText.Operation(r.Canvas.Localization, CandidateApplicationKind.Attack)));
                    Assert.IsFalse(r.Page.Find<UnityEngine.UI.Button>(FightMatchViewId.Row("history:" + r.Host.HistoryEntries[0].HistoryAnchorId)).interactable);
                    PlayerBattlePanel.Click(r.Page.Find<UnityEngine.UI.Button>(FightMatchViewId.Row("query-input-original")));
                    Assert.IsFalse(r.Page.GetComponentsInChildren<LocalizedTmpText>(true).Any(x => x.gameObject.activeInHierarchy && x.Key == "fm.save_recovery.checking"));
                    Assert.AreSame(request, input.LastRequest); r.Unchanged(head, files, clocks);
                }
                Is(input.RetryLast().Application); Assert.AreEqual(BattlePhase.WonPendingSettlement, r.State.Phase);
                var skip = r.Page.Find<UnityEngine.UI.Button>("fm.action.battle.skip");
                Assert.IsTrue(skip.interactable); Assert.AreEqual("fm.victory.playback.skip_button", skip.GetComponentInChildren<LocalizedTmpText>(true).Key);
                PlayerBattlePanel.Click(skip); Assert.IsNull(input.View.PresentationToken); Assert.IsTrue(r.View.CanSettle);
                var result = r.Settle(); r.Receipt(result);
                Assert.AreEqual("level:ch01-01", result.Receipt.LevelId);
                Assert.AreEqual("b2247d3f951626edfdf25753520f3421dc20e8d8cab7731c9ab3ba6ece1a5129", result.Context.Binding.ContentFingerprint);
                BattleCopyAssert.Localized(r.Page, "receipt-no-first-clear", r.Canvas.Localization, "fm.result.no_first_clear_bonus");
                Assert.AreEqual(1, r.Head.Business.Rewards.BaseRewards.Count);
            }
            // N-P03 is specifically the Navigation owner flow; Battle's existing direct End admission stays intact.
            using (var r = new NavigationRig())
            using (var controller = new PlayerNavigationController(r.Player, Budget()))
            using (var panel = new NavigationPanel(controller))
            {
                panel.Click(NavigationPanel.Row("navigation.Team")); panel.Click("fm.action.party.confirm");
                r.Storage.Fault = "snapshot-before"; panel.Click(NavigationPanel.Row("save.Confirm"));
                var head = r.Head; var files = r.Files(); var pending = controller.View.Read.Application.PendingOperationId;
                Assert.IsNotNull(pending); panel.AssertCaption(NavigationPanel.Row("save.EndReview"), "fm.save_recovery.review_end_button");
                Assert.IsFalse(panel.Find<UnityEngine.UI.Button>(NavigationPanel.Row("save.End")).gameObject.activeInHierarchy);
                panel.Click(NavigationPanel.Row("save.EndReview")); Assert.IsTrue(controller.View.Confirmation.IsEndConfirmation);
                Assert.IsNull(controller.View.ReasonFor(PlayerNavigationAction.End));
                panel.AssertVisible("fm.save_recovery.end_uncommitted_confirm");
                panel.AssertCaption(NavigationPanel.Row("save.End"), "fm.save_recovery.end_uncommitted_button");
                Assert.IsFalse(panel.VisibleTexts.Any(x => x.Key == "fm.save_recovery.checking"));
                NavigationAssertions.Unchanged(r, head, files);
                panel.Click(NavigationPanel.Row("save.End")); Assert.IsNull(controller.View.Read.Application.PendingOperationId);
                Assert.IsFalse(r.Head.Records.Any(x => x.OperationId == pending));
            }
        }

        [UnityTest] public IEnumerator B09_ReadOnlyRefreshKeepsTheOriginalActionButtonAndContextClickable()
        {
            using (var r = new PlayerBattleRig()) using (var panel = new PlayerBattlePanel(r))
            {
                yield return panel.Ready(); var button = r.Page.Find<UnityEngine.UI.Button>(FightMatchViewId.Row("battle-exit")); var context = r.Host.View.Context;
                var head = r.Head; var files = r.N.Files(); var clocks = r.ClockReads;
                for (var i = 0; i < 3; i++) { r.Host.Input.Refresh(); r.Host.Refresh(); }
                Assert.AreSame(button, r.Page.Find<UnityEngine.UI.Button>(FightMatchViewId.Row("battle-exit"))); Assert.AreSame(context, r.Host.View.Context);
                PlayerBattlePanel.Click(button); Assert.IsNotNull(r.Host.View.Confirmation); r.Unchanged(head, files, clocks);
            }
        }
        [UnityTest] public IEnumerator B09_AnotherPageOrHostDisposalDetachesEveryOldBorrowedCallback()
        {
            foreach (var mode in new[] { "new-page", "host-dispose" })
            using (var r = new PlayerBattleRig()) using (var panel = new PlayerBattlePanel(r))
            using (var replacementCanvas = new BattleUguiRoot())
            {
                yield return panel.Ready(); r.N.Storage.Fault = "snapshot-promoted"; r.Step(false);
                var oldPage = r.Page; var old = oldPage.Find<UnityEngine.UI.Button>("fm.action.battle.retry"); Assert.IsTrue(old.interactable); panel.Keep(old);
                var request = r.Host.Input.LastRequest; var head = r.Head; var files = r.N.Files(); var clocks = r.ClockReads;
                if (mode == "new-page") replacementCanvas.Page().Bind(r.Host, replacementCanvas.Localization); else r.Host.Dispose();
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
                Assert.AreNotEqual(request.OperationId, r.View.OriginalIntent.OperationId);
                BattleCopyAssert.Localized(r.Page, "battle-recovery-operation", r.Canvas.Localization, "fm.save_recovery.operation_summary",
                    BattleText.Arg("operationName", BattleText.Operation(r.Canvas.Localization, request.Kind)));
                Assert.IsNull(r.Page.Find<UnityEngine.UI.Button>(FightMatchViewId.Row("query-battle-original"))); Assert.IsNotNull(r.Page.Find<UnityEngine.UI.Button>(FightMatchViewId.Row("query-input-original")));
                PlayerBattlePanel.Click(r.Page.Find<UnityEngine.UI.Button>(FightMatchViewId.Row("end-input-original")));
                Assert.AreEqual("Ended", r.Host.Input.LastResult.Application.Code); Assert.IsNull(r.View.Read.Application.PendingOperationId);
                Assert.AreEqual(head.Header.CommitId, r.Head.Header.CommitId); Assert.AreEqual(head.Records.Count, r.Head.Records.Count);
                Assert.AreSame(request, r.Host.Input.LastRequest); Assert.AreEqual(clocks, r.ClockReads);
            }
        }
        [UnityTest] public IEnumerator B09_OriginalAttachRebuildsMemberButtonsForItsNewOwnedController()
        {
            using (var r = new PlaybackPanelRig())
            {
                yield return r.Ready(); var old = r.Host.Find<UnityEngine.UI.Button>(FightMatchViewId.Member("W")); var previous = r.Input; r.Canvas.Keep(old);
                r.Host.Attach(r.Battle.System, Budget(), r.Canvas.Localization); r.Resize(); yield return r.Ready();
                Assert.AreNotSame(previous, r.Input); Assert.IsNull(r.Input.SelectedCharacterId);
                old.gameObject.SetActive(true); PlayerBattlePanel.Click(old); old.gameObject.SetActive(false);
                Assert.IsNull(previous.SelectedCharacterId); Assert.IsNull(r.Input.SelectedCharacterId);
                PlayerBattlePanel.Click(r.Host.Find<UnityEngine.UI.Button>(FightMatchViewId.Member("W"))); Assert.AreEqual("W", r.Input.SelectedCharacterId);
                Assert.IsNull(previous.SelectedCharacterId);
            }
        }
        private static readonly string[] ViewTreeRebuildFaults = { "snapshot-promoted", "marker-before", "marker-after" };
        [UnityTest]
        public IEnumerator B06_B09_EntireViewTreeRebuildPreservesHostInputPlaybackAndOriginalAttack([ValueSource(nameof(ViewTreeRebuildFaults))] string fault)
        {
            using (var r = new PlayerBattleRig())
            {
                yield return r.Ready();
                r.N.Storage.Fault = fault; var failed = r.Step(false); Assert.IsFalse(failed.Application.IsCommitted);
                var input = r.Host.Input; var playback = r.Host.Playback; var session = r.Session;
                var request = input.LastRequest; var intent = request.Intent; var bytes = intent.CanonicalBytes.ToArray();
                var view = r.View; var commit = view.Read.Application.PendingCommitId; var head = r.Head; var files = r.N.Files(); var clocks = r.ClockReads;
                var generation = playback.Generation; var starts = playback.Starts; var completions = playback.CompletionReports;
                Assert.IsNull(r.Page.Find<UnityEngine.UI.Button>(FightMatchViewId.Row("query-battle-original")), "Completed H02 is not the current attack recovery owner");
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
        [UnityTest] public IEnumerator B09_BorrowedDetachDoesNotFinishButExplicitCloseAndFinalHostDisposeReportOnce()
        {
            using (var r = new PlayerBattleRig())
            {
                yield return r.Ready();
                Is(r.Step(false).Application); var playback = r.Host.Playback; var token = r.N.Battle.QueryView().PresentationToken;
                var original = playback.Original; var generation = playback.Generation; var reports = playback.CompletionReports;
                var head = r.Head; var files = r.N.Files(); var clocks = r.ClockReads;
                r.Page.Detach(); Assert.AreSame(original, playback.Original); Assert.AreEqual(generation, playback.Generation);
                Assert.AreEqual(reports, playback.CompletionReports); Assert.AreSame(token, r.N.Battle.QueryView().PresentationToken); r.Unchanged(head, files, clocks);
                r.Page.Bind(r.Host, r.Canvas.Localization); Assert.AreSame(original, playback.Original); Assert.AreEqual(generation, playback.Generation);
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
                PlayerBattlePanel.Click(r.Page.Find<UnityEngine.UI.Button>(FightMatchViewId.Row("battle-exit"))); var old = r.Page.Find<UnityEngine.UI.Button>(FightMatchViewId.Row("confirm-battle-end")); Assert.IsNotNull(old); panel.Keep(old);
                var original = r.View.Confirmation; r.RebuildView(); panel.ShowPage(); yield return panel.Ready();
                Assert.AreSame(original, r.View.Confirmation); panel.ClickStale(old); r.Unchanged(head, files, clocks);
                PlayerBattlePanel.Click(r.Page.Find<UnityEngine.UI.Button>(FightMatchViewId.Row("cancel-battle-end"))); Assert.IsNull(r.View.Confirmation); r.Unchanged(head, files, clocks);
            }
        }
        [UnityTest] public IEnumerator B09_ActualBorrowedInputStaleRetryButtonCannotCommitAfterRebind()
        {
            using (var r = new PlayerBattleRig()) using (var panel = new PlayerBattlePanel(r))
            {
                yield return panel.Ready(); r.N.Storage.Fault = "snapshot-promoted"; r.Step(false);
                var old = r.Page.Find<UnityEngine.UI.Button>("fm.action.battle.retry"); Assert.IsTrue(old.interactable); panel.Keep(old);
                var head = r.Head; var files = r.N.Files(); var clocks = r.ClockReads; var request = r.Host.Input.LastRequest;
                r.RebuildView(); panel.ShowPage(); yield return panel.Ready(); panel.ClickStale(old); r.Unchanged(head, files, clocks);
                Assert.AreSame(request, r.Host.Input.LastRequest); PlayerBattlePanel.Click(r.Page.Find<UnityEngine.UI.Button>("fm.action.battle.retry"));
                Is(r.Host.Input.LastResult.Application); Assert.AreEqual(head.Records.Count + 1, r.Head.Records.Count);
            }
        }
        [UnityTest] public IEnumerator B02_ActualHistoryListButtonShowsExactRangeAndStaleConfirmCannotSelectAnother()
        {
            using (var r = PlayerBattleRig.TwoFaces()) using (var panel = new PlayerBattlePanel(r))
            {
                yield return panel.Ready(); Is(r.Step().Application); Is(r.Step().Application);
                var entries = r.Host.HistoryEntries; Assert.GreaterOrEqual(entries.Count, 2);
                PlayerBattlePanel.Click(r.Page.Find<UnityEngine.UI.Button>(FightMatchViewId.Row("history:" + entries[0].HistoryAnchorId))); var first = r.Page.DisplayedRange;
                var old = r.Page.Find<UnityEngine.UI.Button>(FightMatchViewId.Row("confirm-history")); Assert.AreSame(first, r.Host.Input.RollbackPreview); panel.Keep(old);
                PlayerBattlePanel.Click(r.Page.Find<UnityEngine.UI.Button>(FightMatchViewId.Row("history:" + entries[1].HistoryAnchorId))); var second = r.Page.DisplayedRange;
                var head = r.Head; var files = r.N.Files(); var clocks = r.ClockReads;
                panel.ClickStale(old); Assert.AreSame(second, r.Host.Input.RollbackPreview); r.Unchanged(head, files, clocks);
                PlayerBattlePanel.Click(r.Page.Find<UnityEngine.UI.Button>(FightMatchViewId.Row("confirm-history"))); Is(r.Host.Input.LastResult.Application);
                Assert.AreEqual(CandidateApplicationKind.Rollback, r.Host.Input.LastRequest.Kind);
            }
        }
        [UnityTest] public IEnumerator B04_ButtonAvailabilityTracksRealTokenAndActualReceiptAfterHistoryRemoved()
        {
            using (var r = new PlayerBattleRig())
            {
                yield return r.Ready();
                r.Win(false); Assert.IsFalse(r.Page.Find<UnityEngine.UI.Button>("fm.action.victory.settle").interactable);
                StringAssert.Contains("待结算", r.Page.Find<TMPro.TextMeshProUGUI>(FightMatchViewId.Row("battle-status")).text); Assert.IsNull(r.Page.Find<TMPro.TextMeshProUGUI>(FightMatchViewId.Row("receipt-title")));
                r.Host.Playback.SkipToFinal(); Assert.IsTrue(r.Page.Find<UnityEngine.UI.Button>("fm.action.victory.settle").interactable);
                var done = r.Settle(); Assert.IsNull(done.Battle.History);
                BattleCopyAssert.Localized(r.Page, "receipt-title", r.Canvas.Localization, "fm.result.already_settled");
                BattleCopyAssert.Localized(r.Page, "receipt-level", r.Canvas.Localization, "fm.result.level", BattleText.Arg("levelName", "第 1 关"));
                foreach (var m in done.Receipt.Members)
                {
                    Assert.AreEqual("W", m.CharacterId); Assert.IsNotNull(m.Experience); Assert.AreEqual(m.Reward.Amount, m.Experience.Amount);
                    BattleCopyAssert.Localized(r.Page, "receipt-member:" + m.CharacterId, r.Canvas.Localization, "fm.result.member_xp",
                        BattleText.Arg("characterName", "战士"), BattleText.Arg("amount", BattleText.Number(m.Experience.Amount)));
                }
                BattleCopyAssert.Localized(r.Page, "receipt-inventory", r.Canvas.Localization, "fm.result.base_reward_section");
                foreach (var item in done.Receipt.InventoryGrant.Items)
                {
                    Assert.That(item.ItemId, NUnit.Framework.Is.EqualTo("item:tin").Or.EqualTo("item:wood"));
                    BattleCopyAssert.Localized(r.Page, "receipt-item:" + item.ItemId, r.Canvas.Localization, "fm.result.item_reward",
                        BattleText.Arg("itemName", item.ItemId == "item:tin" ? "锡片" : "木材"), BattleText.Arg("amount", BattleText.Number(item.Quantity)));
                }
                Assert.IsNull(r.Canvas.Localization.BindingDiagnostic);
                var page = r.Page.Page; var receipt = done.Receipt; r.Canvas.Localization.SetLocale(LocaleId.En);
                Assert.AreSame(page, r.Page.Page); Assert.AreSame(receipt, r.Host.View.Receipt);
                foreach (var m in receipt.Members) BattleCopyAssert.Localized(r.Page, "receipt-member:" + m.CharacterId,
                    r.Canvas.Localization, "fm.result.member_xp", BattleText.Arg("characterName", "Warrior"), BattleText.Arg("amount", BattleText.Number(m.Experience.Amount)));
                Assert.IsNull(r.Canvas.Localization.BindingDiagnostic);
            }
        }
        [UnityTest] public IEnumerator B09_StalePageEpochAndDisposedHostCannotSubmitOrAcknowledgePlayback()
        {
            using (var r = new PlayerBattleRig())
            {
                var page = r.Page.Page; var context = r.View.Context; r.RebuildView(); var head = r.Head; var files = r.N.Files(); var clocks = r.ClockReads;
                r.Host.PreviewEnd(page, CandidateApplicationKind.ExitAttempt, context); Assert.IsNull(r.View.Confirmation);
                r.Host.Settle(page, context); r.Unchanged(head, files, clocks);
                yield return r.Ready();
                Is(r.Step(false).Application); var token = r.N.Battle.QueryView().PresentationToken;
                r.Host.SkipPlayback(page); Assert.AreSame(token, r.N.Battle.QueryView().PresentationToken);
                var current = r.Page.Page; r.Host.Dispose(); head = r.Head; files = r.N.Files(); clocks = r.ClockReads;
                r.Host.PreviewEnd(current, CandidateApplicationKind.ExitAttempt, context); r.Host.AcceptHost(null); r.Unchanged(head, files, clocks);
                Assert.AreEqual(1, r.Host.DisposeCount);
            }
        }
    }
}
