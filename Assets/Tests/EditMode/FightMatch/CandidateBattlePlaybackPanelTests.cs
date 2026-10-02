using System;
using System.Collections;
using System.Linq;
using FightMatch.Application;
using FightMatch.Core;
using FightMatch.Presentation;
using NUnit.Framework;
using QFramework;
using UnityEngine;
using UnityEngine.TestTools;
using static FightMatch.Core.Tests.PlaybackPanelRig;
using static FightMatch.Core.Tests.LocalSaveTestFiles;

namespace FightMatch.Core.Tests
{
    public class CandidateBattlePlaybackPanelTests
    {
        [TearDown] public void Cleanup() { CloseAll(); }

        [UnityTest]
        public IEnumerator P27_05_ActualPanelLabelsBoardOverrideAndNextAttackChain()
        {
            using (var r = new PlaybackPanelRig())
            {
                yield return r.Ready(); var result = r.Attack(); var controller = r.Playback;
                Assert.NotNull(r.Board.PlaybackOverride); Assert.AreSame(controller.Frame, r.Board.PlaybackOverride);
                Hp(controller.Frame.Actors.Single(a => a.Key.Equals(result.Presentation.BeforeSnapshot.Enemies[0].CombatantKey)).Hp, 15);
                BattleCopyAssert.Diagnostic(r.Host, "enemy-hp-" + result.Presentation.BeforeSnapshot.Enemies[0].Enemy.OriginalSlot); Assert.AreEqual(-1, controller.Frame.OriginalFactIndex);
                Assert.IsEmpty(controller.Frame.LockedRoutes); CollectionAssert.AreEqual(r.Route(), controller.Frame.TemporaryRoute);
                r.Host.Advance(180); UnityEngine.Canvas.ForceUpdateCanvases();
                Hp(controller.Frame.Actors.Single(a => a.Key.Equals(result.Presentation.BeforeSnapshot.Enemies[0].CombatantKey)).Hp, 0);
                BattleCopyAssert.Diagnostic(r.Host, "enemy-hp-" + result.Presentation.BeforeSnapshot.Enemies[0].Enemy.OriginalSlot); BattleCopyAssert.Diagnostic(r.Host, "playback-beat");
                Assert.AreSame(result.Presentation.OrderedFacts[0], controller.Frame.OriginalFact);
                while (controller.IsPlaying && controller.Frame.OriginalFact?.Stage?.Kind != CandidateStageFactKind.RouteLocked) r.Host.Advance(180);
                Assert.IsTrue(controller.IsPlaying); Assert.AreEqual(1, r.Board.PlaybackOverride.LockedRoutes.Count); Assert.IsEmpty(r.Board.PlaybackOverride.TemporaryRoute);
                Assert.AreEqual(CandidateStageFactKind.RouteLocked, controller.Frame.OriginalFact.Stage.Kind);
                BattleCopyAssert.Diagnostic(r.Host, "playback-stage"); r.Finish();
                BattleCopyAssert.Localized(r.Host, "ally-hp-0", r.Canvas.Localization, "fm.battle.hud.member_hp",
                    BattleText.Arg("characterName", "战士"), BattleText.Arg("currentHp", "95"), BattleText.Arg("maxHp", "100")); Assert.IsNull(r.Board.PlaybackOverride); Assert.IsTrue(r.Input.View.Attack.IsAvailable);
                var second = r.Attack(1); Assert.AreEqual(2, controller.Starts); Assert.IsTrue(controller.IsPlaying); r.Finish();
                Assert.AreEqual(BattlePhase.WonPendingSettlement, controller.Frame.Phase); Assert.AreSame(second.Presentation.Token, controller.LastReportedToken);
                Assert.AreEqual(0, r.Battle.Head.Business.Rewards.BaseRewards.Count); Assert.IsFalse(r.Input.View.Attack.IsAvailable);
                TestContext.Out.WriteLine("P27 panel complete chain: " + r.Label("ally-hp-0") + " | " + r.Label("playback-stage"));
            }
            using (var r = new PlayerBattleRig()) using (var panel = new PlayerBattlePanel(r))
            {
                yield return panel.Ready(); var result = r.Step(false); var controller = r.Host.Playback; var page = r.Page.Page;
                var token = result.Presentation.Token; var generation = controller.Generation;
                BattleCopyAssert.Localized(r.Page, "enemy-hp-" + result.Presentation.BeforeSnapshot.Enemies[0].Enemy.OriginalSlot, r.Canvas.Localization, "fm.battle.hud.enemy_hp",
                    BattleText.Arg("enemyName", "发条步兵 1"), BattleText.Arg("currentHp", "15"), BattleText.Arg("maxHp", "15"));
                Assert.IsNull(r.Canvas.Localization.BindingDiagnostic);
                r.Page.PlaybackView.Advance(180); var fact = controller.Frame.OriginalFact.DirectAttack;
                Assert.IsNotNull(fact); Hp(fact.HpAfter, 0);
                StringAssert.Contains("发条步兵 1", BattleCopyAssert.VisibleText(r.Page));
                var hit = r.Canvas.Localization.Resolve(fact.Crit.Triggered ? "fm.battle.playback.critical_hit" : "fm.battle.playback.direct_hit",
                    new[] { BattleText.Arg("characterName", "战士"), BattleText.Arg("enemyName", "发条步兵 1"),
                        BattleText.Arg("hpBefore", "15"), BattleText.Arg("hpAfter", "0") });
                Assert.IsTrue(hit.IsSuccess, hit.DiagnosticCode);
                BattleCopyAssert.Localized(r.Page, "playback-beat", r.Canvas.Localization, "fm.battle.playback.beat_row",
                    BattleText.Arg("beatNumber", "1"), BattleText.Arg("beat", hit.Text));
                r.Canvas.Localization.SetLocale(LocaleId.En);
                Assert.AreSame(page, r.Page.Page); Assert.AreSame(token, controller.Original.Token); Assert.AreEqual(generation, controller.Generation);
                Assert.AreEqual(0, controller.Frame.OriginalFactIndex); Assert.AreEqual(0, controller.CompletionReports);
                BattleCopyAssert.Localized(r.Page, "enemy-hp-" + result.Presentation.BeforeSnapshot.Enemies[0].Enemy.OriginalSlot, r.Canvas.Localization, "fm.battle.hud.enemy_hp",
                    BattleText.Arg("enemyName", "Clockwork Infantry 1"), BattleText.Arg("currentHp", "0"), BattleText.Arg("maxHp", "15"));
                StringAssert.Contains("Warrior", r.Page.Find<TMPro.TextMeshProUGUI>(FightMatchViewId.Row("playback-beat")).text);
                StringAssert.Contains("Clockwork Infantry 1", r.Page.Find<TMPro.TextMeshProUGUI>(FightMatchViewId.Row("playback-beat")).text);
                while (controller.IsPlaying && controller.Frame.OriginalFact?.Stage?.Kind != CandidateStageFactKind.RouteLocked) r.Page.PlaybackView.Advance(180);
                BattleCopyAssert.Localized(r.Page, "playback-stage", r.Canvas.Localization, "fm.battle.stage.route_locked", BattleText.Arg("enemyName", "Clockwork Infantry 1"));
                r.Host.Playback.SkipToFinal(); Assert.AreSame(token, controller.LastReportedToken);
                BattleCopyAssert.Localized(r.Page, "ally-hp-0", r.Canvas.Localization, "fm.battle.hud.member_hp",
                    BattleText.Arg("characterName", "Warrior"), BattleText.Arg("currentHp", "95"), BattleText.Arg("maxHp", "100"));
                Assert.IsNull(r.Canvas.Localization.BindingDiagnostic);
                r.Step(); Assert.AreEqual(BattlePhase.WonPendingSettlement, r.State.Phase);
            }
        }
        [UnityTest]
        public IEnumerator P27_05_RealUIToolkitScheduleAdvancesActualFacts()
        {
            UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene,
                UnityEditor.SceneManagement.NewSceneMode.Single);
            yield return new EnterPlayMode();
            using (var r = new PlaybackPanelRig())
            {
                yield return r.Ready(); r.Attack(); var before = r.Playback.Frame.OriginalFactIndex;
                var until = Time.realtimeSinceStartupAsDouble + 3;
                while (r.Playback.IsPlaying && r.Playback.Frame.OriginalFactIndex == before && Time.realtimeSinceStartupAsDouble < until)
                { UnityEngine.Canvas.ForceUpdateCanvases(); yield return null; }
                Assert.IsTrue(!r.Playback.IsPlaying || r.Playback.Frame.OriginalFactIndex > before, "Actual uGUI Update handler must progress");
                r.Finish(); Assert.IsNull(r.Board.PlaybackOverride);
            }
            yield return new ExitPlayMode();
        }
        [UnityTest]
        public IEnumerator P27_03_SkipCloseDetachBlurLowMemoryAndSchedulingFailureReleaseOnlyOwnResources()
        {
            UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene,
                UnityEditor.SceneManagement.NewSceneMode.Single);
            yield return new EnterPlayMode();
            yield return BattleCancellationCases.AssertDisableAndExplicitCancellationOrdersBody();
            foreach (var mode in new[] { "skip", "close", "detach", "blur", "low-memory-equivalent", "schedule-error-equivalent" })
            {
                if (mode == "blur")
                {
                    using (var focus = new UguiHostRig(true))
                    {
                        focus.Enter(); yield return focus.Ready(); focus.BeginRoute(); focus.EndRoute();
                        var input = focus.Session.Battle.Input; var controller = focus.Session.Battle.Playback;
                        var first = input.LastResult; Assert.IsTrue(first.Application.IsCommitted);
                        var head = focus.Head; var files = PlayerSessionTestData.CopyFiles(focus.Storage.Files);
                        var calls = focus.Storage.SnapshotCreates; var page = focus.View.BattleView.Page;
                        controller.Advance(180); var frame = controller.Frame; var reports = controller.CompletionReports;
                        focus.Host.HandleApplicationFocus(true);
                        Assert.IsTrue(controller.IsPlaying); Assert.AreSame(frame, controller.Frame); Assert.AreSame(frame, focus.Board.PlaybackOverride);
                        Assert.AreSame(first.Presentation.Token, input.View.PresentationToken); Assert.AreSame(page, focus.View.BattleView.Page);
                        Assert.AreEqual(reports, controller.CompletionReports); Assert.AreSame(head, focus.Head);
                        Assert.AreEqual(calls, focus.Storage.SnapshotCreates); PlayerSessionTestData.SameFiles(files, focus.Storage.Files);
                        var clearedBeforeReport = false;
                        Action observe = () =>
                        {
                            if (!controller.IsPlaying && ReferenceEquals(first.Presentation.Token, input.View.PresentationToken))
                                clearedBeforeReport = focus.Board.PlaybackOverride == null && ReferenceEquals(head, controller.LatestView.PublishedSnapshot);
                        };
                        controller.Changed += observe;
                        try { focus.Host.HandleApplicationFocus(false); }
                        finally { controller.Changed -= observe; }
                        Assert.IsTrue(clearedBeforeReport); Assert.IsFalse(controller.IsPlaying); Assert.IsNull(focus.Board.PlaybackOverride);
                        Assert.AreSame(first.Presentation.Token, controller.LastReportedToken); Assert.AreEqual(reports + 1, controller.CompletionReports);
                        Assert.IsNull(input.View.PresentationToken); Assert.AreSame(head, focus.Head);
                        Assert.AreEqual(calls, focus.Storage.SnapshotCreates); PlayerSessionTestData.SameFiles(files, focus.Storage.Files);
                        focus.BeginRoute(); focus.EndRoute(); var second = input.LastResult;
                        Assert.IsTrue(second.Application.IsCommitted); Assert.AreNotSame(first.Presentation.Token, second.Presentation.Token);
                        var system = FightMatchDemoArchitecture.Interface.GetSystem<CandidateBattleApplicationSystem>();
                        system.ReportPresentationCompleted(first.Presentation.Token);
                        Assert.AreSame(second.Presentation.Token, system.QueryView().PresentationToken);
                        Assert.AreSame(second.Presentation.Token, controller.Original.Token); Assert.IsTrue(controller.IsPlaying);
                        yield return null;
                        Assert.AreSame(second.Presentation.Token, system.QueryView().PresentationToken);
                        TestContext.Out.WriteLine("FIX-04 production handler directly exercised; OS focus delivery remains Q7.");
                    }
                    continue;
                }
            using (var r = new PlaybackPanelRig())
            {
                yield return r.Ready(); var first = r.Attack(); var controller = r.Playback; var head = r.Battle.Head; var disk = r.Battle.Runtime.Disk();
                r.Host.Advance(180);
                var clearedBeforeReport = false;
                Action observe = () => {
                    if (!controller.IsPlaying && ReferenceEquals(first.Presentation.Token, r.Battle.System.QueryView().PresentationToken))
                        clearedBeforeReport = r.Board.PlaybackOverride == null && ReferenceEquals(head, controller.LatestView.PublishedSnapshot);
                };
                controller.Changed += observe;
                if (mode == "skip") r.Host.SkipToFinal();
                else if (mode == "close") r.Host.Close();
                else if (mode == "detach") r.Host.gameObject.SetActive(false);
                else if (mode == "low-memory-equivalent") r.Host.NotifyLowMemory();
                else r.Host.ReportSchedulingFailure(new InvalidOperationException("observed cancellation test"));
                controller.Changed -= observe; Assert.IsTrue(clearedBeforeReport, mode + " clears override and reads latest before reporting its token");
                Assert.IsFalse(controller.IsPlaying, mode); Assert.AreSame(first.Presentation.Token, controller.LastReportedToken);
                Assert.AreEqual(1, controller.CompletionReports); Assert.IsNull(r.Board.PlaybackOverride); Assert.IsNull(r.Battle.System.QueryView().PresentationToken);
                Assert.AreSame(head, r.Battle.Head); r.Battle.Runtime.SameDisk(disk);
                if (mode == "schedule-error-equivalent")
                {
                    StringAssert.Contains("InvalidOperationException: observed cancellation test", controller.Diagnostic);
                    BattleCopyAssert.Localized(r.Host, "playback-diagnostic", r.Canvas.Localization, "fm.battle.playback.interrupted",
                        BattleText.Arg("errorCode", "PlaybackInterrupted"));
                    StringAssert.DoesNotContain("InvalidOperationException", r.Label("playback-diagnostic"));
                    StringAssert.DoesNotContain("observed cancellation test", r.Label("playback-diagnostic"));
                }
                var second = r.Battle.Attack(1, false); var token = second.Presentation.Token;
                r.Host.Close(); controller.Advance(100000); controller.Dispose();
                Assert.AreSame(token, r.Battle.System.QueryView().PresentationToken, mode + " old callback must not clear A2");
                yield return null; Assert.AreSame(token, r.Battle.System.QueryView().PresentationToken); TestContext.Out.WriteLine("P27 interruption " + mode + " latest A2 remains pending");
            }
            }
            yield return new ExitPlayMode();
        }
        [UnityTest]
        public IEnumerator P27_03_ReattachUnsubscribesOldControllerAndDoesNotReplayPriorFacts()
        {
            using (var r = new PlaybackPanelRig())
            {
                yield return r.Ready(); r.Attack(); var old = r.Playback; var oldInput = r.Input;
                r.Host.Attach(r.Battle.System, B(), r.Canvas.Localization); r.Resize(); yield return r.Ready();
                var current = r.Playback; Assert.AreNotSame(old, current); Assert.AreEqual(0, current.Starts); Assert.IsFalse(current.IsPlaying);
                Assert.IsFalse(old.IsPlaying); oldInput.QueryLastOperation(); Assert.AreEqual(0, current.Starts);
                r.Attack(1); Assert.AreEqual(1, current.Starts); Assert.AreEqual(1, old.Starts); Assert.AreEqual(1, old.CompletionReports);
                oldInput.Refresh(); old.Advance(100000); old.Dispose(); Assert.IsTrue(current.IsPlaying); Assert.IsNotNull(r.Input.View.PresentationToken);
                r.Finish(); r.Host.Close(); r.Host.Close(); Assert.AreEqual(1, current.CompletionReports);
            }
        }
    }
}
