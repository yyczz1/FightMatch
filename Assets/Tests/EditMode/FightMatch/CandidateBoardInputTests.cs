using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using FightMatch.Application;
using FightMatch.Input;
using FightMatch.Platform;
using FightMatch.Presentation;
using FlowPuzzle.Core;
using NUnit.Framework;
using QFramework;
using UnityEngine.TestTools;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;
using static FightMatch.Core.Tests.BusinessSaveScenario;
using static FightMatch.Core.Tests.LocalSaveTestFiles;
using static FightMatch.Core.Tests.BattleApplicationRig;

namespace FightMatch.Core.Tests
{
    public sealed class CandidateBoardInputTests
    {
        [Test]
        public void CH1_L5_PendingRetryRollbackAndOriginalQueryUseIndependentEvaluations()
        {
            using (var r = new BattleApplicationRig())
            {
                var template = new SaveStoreBudget(new SaveCodecBudget(new ExactMathBudget(maxPrimitiveSteps: 0)));
                var evaluations = new List<SaveStoreBudget>();
                var input = new CandidateBoardInputController(r.System, template, () => {
                    var next = B(); evaluations.Add(next); return next;
                });
                Assert.IsTrue(input.SelectMember("W")); var initial = r.State; var records = r.Head.Records.Count;
                r.Runtime.Storage.Base.Arm("Snapshot.Flush.after", null);
                DrawBudgetRoute(input, r.AttackDraft().Attack.Route);
                Is(input.LastResult, "SaveFailed"); Assert.IsTrue(r.Runtime.Storage.Base.FaultUsed); Assert.AreEqual(1, evaluations.Count);
                var pendingRequest = input.LastRequest; var pendingBytes = pendingRequest.Intent.CanonicalBytes.ToArray();
                var pendingCommit = input.View.ApplicationView.PendingCommitId;
                Assert.AreEqual(pendingRequest.OperationId, input.View.ApplicationView.PendingOperationId);
                Assert.IsNotNull(pendingCommit); Assert.IsFalse(input.LastResult.Application.IsCommitted);
                Assert.AreEqual(records, r.Head.Records.Count);
                var attackUsed = evaluations[0].Codec.Math.PrimitiveStepsUsed; Assert.Greater(attackUsed, 0);
                var pendingDisk = r.Runtime.Disk(); var pendingCalls = r.Runtime.Storage.Base.Calls;
                input.Refresh(); Assert.AreSame(pendingRequest, input.LastRequest);
                CollectionAssert.AreEqual(pendingBytes, input.LastRequest.Intent.CanonicalBytes);
                Assert.AreEqual(pendingCommit, input.View.ApplicationView.PendingCommitId);
                r.Runtime.SameDisk(pendingDisk); Assert.AreEqual(pendingCalls, r.Runtime.Storage.Base.Calls);
                var retried = Is(input.RetryLast(), "Completed"); Assert.AreEqual(2, evaluations.Count);
                Assert.IsTrue(retried.Application.IsCommitted); Assert.AreEqual(pendingCommit, retried.Application.OriginalCommitId);
                Assert.AreSame(pendingRequest, input.LastRequest); CollectionAssert.AreEqual(pendingBytes, input.LastRequest.Intent.CanonicalBytes);
                Assert.IsNull(input.View.ApplicationView.PendingOperationId); Assert.AreEqual(records + 1, r.Head.Records.Count);
                Assert.AreEqual(1, r.Runtime.Storage.Base.RealMarkerPromotionsFor(pendingCommit));
                Assert.AreEqual(attackUsed, evaluations[0].Codec.Math.PrimitiveStepsUsed);
                var retryUsed = evaluations[1].Codec.Math.PrimitiveStepsUsed; Assert.Greater(retryUsed, 0);
                Is(r.System.ReportPresentationCompleted(input.LastResult.Presentation.Token), "PresentationCompleted"); input.Refresh();
                var cell = r.State.Board.LockedRoutes.Single().Route[1]; var point = BudgetPointer(cell);
                input.Down(point, 6); input.Up(point);
                Is(input.LastResult, "Completed"); Assert.AreEqual(CandidateApplicationKind.Rollback, input.LastRequest.Kind);
                Assert.AreEqual(3, evaluations.Count, "Tap preview, automatic confirmation, recheck, prepare and submit share one evaluation.");
                Assert.AreEqual(attackUsed, evaluations[0].Codec.Math.PrimitiveStepsUsed);
                Assert.AreEqual(retryUsed, evaluations[1].Codec.Math.PrimitiveStepsUsed);
                Assert.Greater(evaluations[2].Codec.Math.PrimitiveStepsUsed, 0);
                Assert.AreEqual(records + 2, r.Head.Records.Count); Same(initial.Members, r.State.Members); Same(initial.Enemies, r.State.Enemies);
                var original = input.LastRequest; var bytes = original.Intent.CanonicalBytes.ToArray();
                var commit = input.LastResult.Application.OriginalCommitId; var head = r.Head; var disk = r.Runtime.Disk();
                var writes = r.Runtime.Storage.Base.WriteCalls; var rollbackUsed = evaluations[2].Codec.Math.PrimitiveStepsUsed;
                var lookup = Is(input.QueryLastOperation(), "Completed");
                Assert.IsTrue(lookup.Application.IsCommitted); Assert.AreEqual(commit, lookup.Application.OriginalCommitId);
                Assert.AreSame(original, input.LastRequest); CollectionAssert.AreEqual(bytes, input.LastRequest.Intent.CanonicalBytes);
                Assert.AreEqual(4, evaluations.Count); Assert.AreEqual(rollbackUsed, evaluations[2].Codec.Math.PrimitiveStepsUsed);
                Assert.AreEqual(0, template.Codec.Math.PrimitiveStepsUsed);
                Assert.AreSame(head, r.Head); r.Runtime.SameDisk(disk); Assert.AreEqual(writes, r.Runtime.Storage.Base.WriteCalls);
            }
        }

        [Test]
        public void CH1_L5_PrepareAndSubmitCannotAcquireSeparateBudgets()
        {
            using (var r = new BattleApplicationRig())
            {
                var probe = B(); var draft = r.AttackDraft();
                Assert.IsTrue(r.System.Prepare(draft, probe.Codec).IsAccepted);
                var steps = checked((int)probe.Codec.Math.PrimitiveStepsUsed); Assert.Greater(steps, 0);
                var limited = new SaveStoreBudget(new SaveCodecBudget(new ExactMathBudget(maxPrimitiveSteps: steps)));
                var allocations = 0;
                var input = new CandidateBoardInputController(r.System, B(), () => ++allocations == 1 ? limited : B());
                var head = r.Head; var disk = r.Runtime.Disk(); var calls = r.Runtime.Storage.Base.Calls;
                Assert.IsTrue(input.SelectMember("W")); DrawBudgetRoute(input, draft.Attack.Route);
                Assert.IsTrue(input.LastPreparation.IsAccepted, input.LastPreparation.Code);
                Assert.AreSame(input.LastPreparation.Request, input.LastRequest);
                Is(input.LastResult, "Limit"); Assert.AreEqual("PrimitiveSteps", input.LastResult.Application.Diagnostic.LimitReason);
                Assert.IsFalse(input.LastResult.Application.IsCommitted); Assert.IsNull(input.View.ApplicationView.PendingOperationId);
                Assert.AreEqual(1, allocations, "A fresh second meter would incorrectly let submission continue after preparation exhausted this one.");
                Assert.AreEqual(steps, limited.Codec.Math.PrimitiveStepsUsed);
                Assert.AreEqual((ulong)steps + 1, input.LastResult.Application.Diagnostic.RequiredAtLeast);
                Assert.AreEqual((ulong)steps, input.LastResult.Application.Diagnostic.Allowed);
                Assert.AreSame(head, r.Head); r.Runtime.SameDisk(disk); Assert.AreEqual(calls, r.Runtime.Storage.Base.Calls);
            }
        }

        [Test]
        public void CH1_L5_PublicInputConstructorRetainsFixedLowBudgetRejection()
        {
            using (var r = new BattleApplicationRig())
            {
                var budget = new SaveStoreBudget(new SaveCodecBudget(new ExactMathBudget(maxPrimitiveSteps: 0)));
                var input = new CandidateBoardInputController(r.System, budget);
                var head = r.Head; var disk = r.Runtime.Disk(); var calls = r.Runtime.Storage.Base.Calls;
                Assert.IsTrue(input.SelectMember("W")); DrawBudgetRoute(input, r.AttackDraft().Attack.Route);
                Assert.AreEqual("Limit", input.LastPreparation.Code); Assert.AreEqual("PrimitiveSteps", input.LastPreparation.Diagnostic.LimitReason);
                Assert.IsNull(input.LastRequest); Assert.IsNull(input.LastResult); Assert.IsNull(input.View.ApplicationView.PendingOperationId);
                Assert.AreEqual(0, budget.Codec.Math.PrimitiveStepsUsed);
                Assert.AreEqual(1UL, input.LastPreparation.Diagnostic.RequiredAtLeast);
                Assert.AreEqual(0UL, input.LastPreparation.Diagnostic.Allowed);
                Assert.AreSame(head, r.Head); r.Runtime.SameDisk(disk); Assert.AreEqual(calls, r.Runtime.Storage.Base.Calls);
            }
        }

        private static PointerSample BudgetPointer(FlowPos cell) => new PointerSample(1, cell.x * 20, cell.y * 20, cell);
        private static void DrawBudgetRoute(CandidateBoardInputController input, IReadOnlyList<FlowPos> route)
        {
            input.Down(BudgetPointer(route[0]), 6);
            foreach (var cell in route.Skip(1)) input.Move(BudgetPointer(cell));
            input.Up(BudgetPointer(route[route.Count - 1]));
        }

        [UnityTest]
        public IEnumerator IQOO_UI01_SelectedMemberOutlineSurvivesFocusRefreshAndLocaleWithoutBusinessWrite()
        {
            using (var r = new BoardPanelRig())
            {
                yield return r.Ready();
                var member = r.UI.Find<UnityEngine.UI.Button>(FightMatchViewId.Member("W"));
                var outline = member.GetComponent<UnityEngine.UI.Outline>();
                Assert.IsNotNull(outline); Assert.IsFalse(outline.enabled); Assert.IsNull(r.Controller.SelectedCharacterId);
                var head = r.Battle.Head; var files = r.Battle.Runtime.Disk(); var calls = r.Battle.Runtime.Storage.Base.Calls;
                r.Canvas.Driver.Click(member);
                Assert.AreEqual("W", r.Controller.SelectedCharacterId); Assert.IsTrue(outline.enabled);
                r.Canvas.Driver.Move(r.Point(r.Route()[0]));
                r.Canvas.Driver.EventSystem.SetSelectedGameObject(r.Board.gameObject);
                foreach (var locale in new[] { LocaleId.En, LocaleId.ZhHans })
                {
                    r.Controller.Refresh(); r.Canvas.Localization.SetLocale(locale); yield return null;
                    Assert.AreSame(member, r.UI.Find<UnityEngine.UI.Button>(FightMatchViewId.Member("W")));
                    Assert.AreNotSame(member.gameObject, r.Canvas.Driver.EventSystem.currentSelectedGameObject);
                    Assert.AreEqual("W", r.Controller.SelectedCharacterId); Assert.IsTrue(outline.enabled);
                    var caption = member.GetComponentInChildren<LocalizedTmpText>(true);
                    Assert.IsNull(caption.DiagnosticCode);
                    StringAssert.Contains(BattleText.Character(r.Canvas.Localization, "W"), caption.Target.text);
                    Assert.AreSame(head, r.Battle.Head); Assert.AreEqual(calls, r.Battle.Runtime.Storage.Base.Calls);
                    PlayerSessionTestData.SameFiles(files, r.Battle.Runtime.Disk()); Assert.IsNull(r.Controller.LastRequest);
                }
                Assert.IsTrue(r.Controller.SelectMember(null)); r.Controller.Refresh();
                Assert.IsNull(r.Controller.SelectedCharacterId); Assert.IsFalse(outline.enabled);
                Assert.AreSame(head, r.Battle.Head); Assert.AreEqual(calls, r.Battle.Runtime.Storage.Base.Calls);
                PlayerSessionTestData.SameFiles(files, r.Battle.Runtime.Disk()); Assert.IsNull(r.Controller.LastRequest);
            }
        }

        [UnityTest]
        public IEnumerator UGUI_COPY_WIRE01_ExactInputOutcomeBindsOneExistingFeedbackWithoutBusinessWrite()
        {
            UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene,
                UnityEditor.SceneManagement.NewSceneMode.Single);
            yield return new EnterPlayMode();
            var kinds = new[] { CandidateBoardVisibleFeedbackKind.InvalidStart, CandidateBoardVisibleFeedbackKind.NotAdjacent,
                CandidateBoardVisibleFeedbackKind.Crossed, CandidateBoardVisibleFeedbackKind.WrongEndpoint,
                CandidateBoardVisibleFeedbackKind.Cancelled, CandidateBoardVisibleFeedbackKind.FocusLost,
                CandidateBoardVisibleFeedbackKind.SecondPointerIgnored, CandidateBoardVisibleFeedbackKind.ActionAccepted };
            var keys = new[] { "fm.battle.gesture.invalid_start", "fm.battle.gesture.not_adjacent", "fm.battle.gesture.crossed",
                "fm.battle.gesture.wrong_endpoint", "fm.battle.gesture.cancelled", "fm.battle.gesture.focus_lost",
                "fm.battle.gesture.second_pointer_ignored", "fm.battle.action.accepted" };
            for (var i = 0; i < kinds.Length; i++)
            using (var r = new PlayerBattleRig())
            {
                yield return r.Ready();
                var input = r.Host.Input; var board = r.Page.PlaybackView.InputView.Board; var driver = r.Canvas.Driver;
                Assert.IsTrue(input.SelectMember("W")); UnityEngine.Canvas.ForceUpdateCanvases();
                var route = r.Route(r.State.Enemies[0].PairKey); var head = r.Head; var files = r.N.Files(); var clocks = r.ClockReads;
                Func<FlowPos, Vector2> point = cell => UguiPointerDriver.Point(board, cell);
                using (var observed = new BattleFeedbackObserver(input))
                {
                    if (kinds[i] == CandidateBoardVisibleFeedbackKind.InvalidStart) driver.Down(point(new FlowPos(3, 0)), 1);
                    else
                    {
                        driver.Down(point(route[0]), 1);
                        if (kinds[i] == CandidateBoardVisibleFeedbackKind.NotAdjacent) driver.Move(point(new FlowPos(3, 0)), 1);
                        else if (kinds[i] == CandidateBoardVisibleFeedbackKind.Crossed)
                        {
                            foreach (var cell in new[] { new FlowPos(0, 1), new FlowPos(0, 2), new FlowPos(1, 2),
                                new FlowPos(2, 2), new FlowPos(2, 1), new FlowPos(2, 0), new FlowPos(1, 0), new FlowPos(0, 0) })
                                driver.Move(point(cell), 1);
                        }
                        else
                        {
                            driver.Move(point(route[1]), 1);
                            if (kinds[i] == CandidateBoardVisibleFeedbackKind.WrongEndpoint) driver.Up(point(route[1]), 1);
                            else if (kinds[i] == CandidateBoardVisibleFeedbackKind.Cancelled) driver.Cancel(1, point(route[1]));
                            else if (kinds[i] == CandidateBoardVisibleFeedbackKind.FocusLost) board.enabled = false;
                            else if (kinds[i] == CandidateBoardVisibleFeedbackKind.SecondPointerIgnored)
                            {
                                var draft = input.Gesture.DraftCells.ToArray(); driver.Down(point(route[0]), 2);
                                driver.Cancel(2, point(route[0])); Assert.AreEqual(1, input.Gesture.ActivePointerId);
                                CollectionAssert.AreEqual(draft, input.Gesture.DraftCells); Assert.IsTrue(board.HasActivePointer);
                            }
                            else driver.Up(point(route.Last()), 1);
                        }
                    }
                    Assert.AreEqual(kinds[i], input.VisibleFeedback); CollectionAssert.AreEqual(new[] { kinds[i] }, observed.Published);
                    BattleCopyAssert.Feedback(r.Page.PlaybackView.InputView, input, r.Canvas.Localization, keys[i]);
                    if (kinds[i] == CandidateBoardVisibleFeedbackKind.ActionAccepted)
                    {
                        var result = input.LastResult.Application;
                        Assert.IsTrue(input.LastPreparation.IsAccepted); Assert.IsTrue(result.IsCommitted); Assert.IsTrue(result.OriginalLookup.IsFound);
                        Assert.AreEqual(input.LastRequest.OperationId, result.OriginalLookup.Record.OperationId);
                        CollectionAssert.AreEqual(input.LastRequest.Intent.CanonicalBytes, result.OriginalLookup.Record.Intent.CanonicalBytes);
                        Assert.AreEqual(result.OriginalCommitId, result.OriginalLookup.OriginalCommitId);
                        Assert.AreEqual(result.LookupViewCommitId, result.View.PublishedSnapshot.Header.CommitId);
                        Assert.IsTrue(result.View.IsPublishedHeadVerified); Assert.AreEqual(CandidateApplicationPhase.Ready, result.View.Phase);
                        Assert.AreEqual(head.Records.Count + 1, r.Head.Records.Count);
                    }
                    else { Assert.IsNull(input.LastRequest); r.Unchanged(head, files, clocks); }
                    var revision = input.VisibleFeedbackRevision; var page = r.Page.Page;
                    input.Refresh(); input.Refresh();
                    // Real PlayerLoop crosses the InputView's 100ms refresh; no hand-driven Update or timer hook.
                    yield return new UnityEngine.WaitForSecondsRealtime(.12f);
                    Assert.AreEqual(kinds[i], input.VisibleFeedback); Assert.AreEqual(revision, input.VisibleFeedbackRevision);
                    var token = input.View.PresentationToken;
                    r.Canvas.Localization.SetLocale(LocaleId.En);
                    Assert.AreSame(page, r.Page.Page); Assert.AreSame(token, input.View.PresentationToken);
                    Assert.AreEqual(revision, input.VisibleFeedbackRevision); BattleCopyAssert.Feedback(r.Page.PlaybackView.InputView, input, r.Canvas.Localization, keys[i]);
                    input.Down(new PointerSample(9, -1, -1, null), 6);
                    input.Down(new PointerSample(9, -1, -1, new FlowPos(-1, 0)), 6);
                    Assert.AreEqual(kinds[i], input.VisibleFeedback); Assert.AreEqual(revision, input.VisibleFeedbackRevision);
                    if (kinds[i] == CandidateBoardVisibleFeedbackKind.ActionAccepted)
                    {
                        r.Host.Playback.SkipToFinal(); input.QueryLastOperation(); input.RetryLast(); input.ResolveLast(); input.RebuildLatest();
                        Assert.AreEqual(revision, input.VisibleFeedbackRevision); CollectionAssert.AreEqual(new[] { kinds[i] }, observed.Published);
                        BattleCopyAssert.Feedback(r.Page.PlaybackView.InputView, input, r.Canvas.Localization, keys[i]);
                    }
                    if (kinds[i] == CandidateBoardVisibleFeedbackKind.InvalidStart || kinds[i] == CandidateBoardVisibleFeedbackKind.ActionAccepted)
                    {
                        if (kinds[i] == CandidateBoardVisibleFeedbackKind.InvalidStart) driver.Up(point(new FlowPos(3, 0)), 1);
                        var next = r.State.Enemies.First(x => x.Hp.Numerator.Sign > 0).PairKey;
                        driver.Down(point(r.Route(next)[0]), 1);
                        Assert.IsNull(input.VisibleFeedback); Assert.AreEqual(revision + 1, input.VisibleFeedbackRevision);
                        BattleCopyAssert.Feedback(r.Page.PlaybackView.InputView, input, r.Canvas.Localization, null);
                    }
                }
            }
            foreach (var fault in new[] { "snapshot-before", "marker-after" })
            using (var r = new PlayerBattleRig())
            using (var observed = new BattleFeedbackObserver(r.Host.Input))
            {
                yield return r.Ready();
                r.N.Storage.Fault = fault; var failed = r.Step(false); var input = r.Host.Input;
                Assert.IsTrue(input.LastPreparation.IsAccepted); Assert.IsFalse(failed.Application.IsCommitted);
                Assert.IsFalse(observed.Published.Contains(CandidateBoardVisibleFeedbackKind.ActionAccepted));
                Assert.AreNotEqual(CandidateBoardVisibleFeedbackKind.ActionAccepted, input.VisibleFeedback);
                var request = input.LastRequest; var bytes = request.Intent.CanonicalBytes.ToArray();
                input.Refresh(); Assert.IsFalse(observed.Published.Contains(CandidateBoardVisibleFeedbackKind.ActionAccepted));
                var done = fault == "snapshot-before" ? input.RetryLast() : input.ResolveLast();
                Assert.IsTrue(done.Application.IsCommitted); Assert.AreSame(request, input.LastRequest); CollectionAssert.AreEqual(bytes, request.Intent.CanonicalBytes);
                Assert.AreEqual(1, observed.Published.Count(x => x == CandidateBoardVisibleFeedbackKind.ActionAccepted));
                var revision = input.VisibleFeedbackRevision; input.QueryLastOperation(); input.RetryLast(); input.ResolveLast(); input.RebuildLatest();
                Assert.AreEqual(revision, input.VisibleFeedbackRevision);
                Assert.AreEqual(1, observed.Published.Count(x => x == CandidateBoardVisibleFeedbackKind.ActionAccepted));
                BattleCopyAssert.Feedback(r.Page.PlaybackView.InputView, input, r.Canvas.Localization, "fm.battle.action.accepted");
            }
            // Every upper teardown reaches the same clearing point; none may retain an accepted arm for a later retry.
            foreach (var mode in new[] { "input-close", "input-unbind", "owner-replace", "playback-dispose", "battle-dispose", "host-dispose" })
            foreach (var pending in new[] { false, true })
            using (var r = new UguiHostRig(true))
            {
                r.Enter(); yield return r.Ready(); var input = r.Session.Battle.Input; var inputView = r.View.BattleView.PlaybackView.InputView;
                var system = FightMatchDemoArchitecture.Interface.GetSystem<CandidateBattleApplicationSystem>();
                r.Driver.Down(UguiPointerDriver.Point(r.Board, new FlowPos(3, 0)), 1);
                Assert.AreEqual(CandidateBoardVisibleFeedbackKind.InvalidStart, input.VisibleFeedback);
                r.Driver.Up(UguiPointerDriver.Point(r.Board, new FlowPos(3, 0)), 1);
                if (pending)
                {
                    r.Storage.Fault = "snapshot-before"; r.BeginRoute(); r.EndRoute();
                    Assert.IsTrue(input.LastPreparation.IsAccepted); Assert.IsFalse(input.LastResult.Application.IsCommitted);
                }
                var head = r.Head; var files = PlayerSessionTestData.CopyFiles(r.Storage.Files);
                if (mode == "input-close") inputView.Close();
                else if (mode == "input-unbind") inputView.Unbind();
                else if (mode == "owner-replace") inputView.Attach(system, PlayerSessionTestData.Budget(), r.Localization);
                else if (mode == "playback-dispose") r.View.BattleView.PlaybackView.Dispose();
                else if (mode == "battle-dispose") r.View.BattleView.Dispose();
                else r.View.Dispose();
                Assert.IsNull(input.VisibleFeedback, mode); Assert.AreSame(head, r.Head); PlayerSessionTestData.SameFiles(files, r.Storage.Files);
                if (pending) using (var observed = new BattleFeedbackObserver(input))
                {
                    var originalRequest = input.LastRequest;
                    var retried = input.RetryLast();
                    Assert.IsTrue(retried.Application.IsCommitted);
                    Assert.IsFalse(observed.Published.Contains(CandidateBoardVisibleFeedbackKind.ActionAccepted), mode + " clears the old acceptance arm");
                    if (mode == "battle-dispose")
                    {
                        Assert.AreSame(originalRequest, input.LastRequest);
                        Assert.AreEqual(originalRequest.OperationId, retried.Application.OriginalLookup.Record.OperationId);
                        CollectionAssert.AreEqual(originalRequest.Intent.CanonicalBytes, retried.Application.OriginalLookup.Record.Intent.CanonicalBytes);
                        Assert.AreEqual(head.Records.Count + 1, r.Head.Records.Count);
                        Assert.AreEqual(1, r.Head.Records.Count(x => x.OperationId == originalRequest.OperationId));
                        Assert.IsTrue(r.View.BattleView.IsDisposed); Assert.IsNull(r.View.BattleView.Controller); Assert.IsNull(r.View.BattleView.Page);
                        Assert.IsNull(input.VisibleFeedback);
                        Assert.AreEqual("BattleViewDisposed", r.View.DiagnosticCode); Assert.IsTrue(r.View.DiagnosticVisible);
                        Assert.IsFalse(r.Find<FightMatchViewId>("fm.page.battle").gameObject.activeInHierarchy);
                        TestContext.Out.WriteLine("FIX13 battle-dispose + pending: original operation committed once; terminal view remains unbound; BattleViewDisposed visible.");
                    }
                }
            }
            yield return new ExitPlayMode();
        }

        [TearDown]
        public void ClosePanelsAfterInterruptedCoroutines() { BoardPanelRig.CloseAll(); }

        [Test]
        public void B16B01_ExactFriendAndSeparateRuntimeAssemblies()
        {
            var friends = typeof(RouteGesture).Assembly.GetCustomAttributes(typeof(InternalsVisibleToAttribute), false)
                .Cast<InternalsVisibleToAttribute>().Select(x => x.AssemblyName).ToArray();
            CollectionAssert.AreEqual(new[] { "FightMatch.Core.Tests" }, friends);
            Assert.IsFalse(typeof(RouteGesture).Assembly.GetReferencedAssemblies().Any(x => x.Name.StartsWith("Unity", StringComparison.Ordinal)));
            Assert.IsFalse(typeof(CandidateBoardElement).Assembly.GetReferencedAssemblies().Any(x => x.Name.StartsWith("UnityEditor", StringComparison.Ordinal)));
            Assert.AreEqual(83, RouteGestureCases.CaseIds.Count); Assert.AreEqual(83, RouteGestureCases.CaseIds.Distinct().Count());
        }

        [Test]
        public void B16B02_RealViewSelectionIsReadOnlyAndNewAttemptClearsIt()
        {
            using (var r = new BattleApplicationRig())
            {
                var c = new CandidateBoardInputController(r.System, B()); var head = r.Head; var calls = r.Runtime.Storage.Base.Calls;
                Assert.AreSame(r.State, c.View.BattleSnapshot); Assert.IsNull(c.SelectedCharacterId);
                Assert.AreEqual(r.State.Baseline.Entry.AttemptId, c.InputContext.AttemptId);
                Assert.AreEqual(r.State.SceneRevision, c.InputContext.SceneRevision);
                Assert.AreEqual(head.Business.Inventory.PreferenceRevision, c.InputContext.PreferenceRevision);
                Assert.AreEqual(r.State.Board.Face.FaceId, c.InputContext.FaceId);
                CollectionAssert.AreEqual(r.State.Board.Face.Pairs.Select(x => x.EndpointA), c.InputContext.Pairs.Select(x => x.EndpointA));
                Assert.IsTrue(c.InputContext.AllowHistoryTap); Assert.IsTrue(c.InputContext.Pairs.All(x => !x.CanDraw));
                Assert.IsTrue(c.SelectMember("W")); Assert.AreEqual("W", c.InputContext.SelectedCharacterId);
                Assert.IsTrue(c.InputContext.Pairs.All(x => x.CanDraw)); Assert.IsFalse(c.SelectMember("missing"));
                for (var i = 0; i < 5; i++) c.Refresh();
                Assert.AreSame(head, r.Head); Assert.AreEqual(calls, r.Runtime.Storage.Base.Calls);
                Assert.IsNull(c.LastRequest); Assert.IsFalse(c.View.PreferenceApplicable); Assert.AreEqual(false, c.View.RequiredAttackItemUseEnabled);
                r.ExitAndEnter(); c.Refresh(); Assert.IsNull(c.SelectedCharacterId); Assert.IsNull(c.InputContext.SelectedCharacterId);
            }
        }

        [Test]
        public void B16B02_UnconfiguredAndDisposedRemainUnavailableWithoutSyntheticContext()
        {
            CandidateBoardInputController c;
            using (var r = new BattleApplicationRig(ready: false))
            {
                c = new CandidateBoardInputController(r.System, B());
                Assert.IsNull(c.InputContext); Assert.IsFalse(c.View.Attack.IsAvailable); Assert.IsFalse(c.SelectMember("W"));
                Assert.AreEqual("Unconfigured", c.View.Attack.Reason);
            }
            Assert.AreEqual("Disposed", c.Refresh().Attack.Reason); Assert.IsNull(c.InputContext);
        }

        [UnityTest]
        public IEnumerator B16B03_RealPanelDownCapturesOnlyAndLastUpCellSubmitsOnce()
        {
            using (var r = new BoardPanelRig())
            {
                yield return r.Ready(); r.Controller.SelectMember("W");
                var before = r.Battle.Head; var calls = r.Battle.Runtime.Storage.Base.Calls; var route = r.Route(); var results = 0;
                r.Controller.ResultReceived += x => results++;
                r.Down(r.Point(route[0])); Assert.IsTrue(r.HasCapture(-1)); Assert.AreEqual(-1, r.Controller.Gesture.ActivePointerId);
                Assert.AreSame(before, r.Battle.Head); Assert.AreEqual(calls, r.Battle.Runtime.Storage.Base.Calls); Assert.IsNull(r.Controller.LastRequest);
                r.Controller.Refresh(); r.Controller.Refresh(); Assert.AreEqual(GestureStage.Pressed, r.Controller.Gesture.Stage);
                for (var i = 1; i < route.Count - 1; i++) r.Move(r.Point(route[i]));
                Assert.AreSame(before, r.Battle.Head); r.Up(r.Point(route.Last()));
                Is(r.Controller.LastResult, "Completed"); Assert.AreEqual(1, results); Assert.IsFalse(r.HasCapture(-1));
                CollectionAssert.AreEqual(route, r.Battle.Head.Business.ActiveHistory.CurrentRun.Records.Last().Request.Route);
                Assert.AreEqual(before.Records.Count + 1, r.Battle.Head.Records.Count);
                var head = r.Battle.Head; r.Up(r.Point(route.Last())); Assert.AreSame(head, r.Battle.Head); Assert.AreEqual(1, results);
                Assert.IsNotNull(r.Controller.View.PresentationToken); Assert.AreEqual("PresentationPending", r.Controller.View.Attack.Reason);
            }
        }

        [UnityTest]
        public IEnumerator B16B04_SecondTouchCannotStealMoveEndOrCancelFirstCapture()
        {
            using (var r = new BoardPanelRig())
            {
                yield return r.Ready(); r.Controller.SelectMember("W"); var route = r.Route();
                r.Down(r.Point(route[0]), 1, PointerType.touch); Assert.IsTrue(r.HasCapture(1));
                r.Down(r.Point(route.Last()), 2, PointerType.touch); r.Move(r.Point(route.Last()), 2, PointerType.touch);
                r.Up(r.Point(route.Last()), 2, PointerType.touch); r.Cancel(2, PointerType.touch);
                Assert.AreEqual(1, r.Controller.Gesture.ActivePointerId); Assert.IsTrue(r.HasCapture(1)); Assert.IsFalse(r.HasCapture(2));
                for (var i = 1; i < route.Count - 1; i++) r.Move(r.Point(route[i]), 1, PointerType.touch);
                r.Up(r.Point(route.Last()), 1, PointerType.touch); Is(r.Controller.LastResult, "Completed");
                Assert.IsFalse(r.HasCapture(1));
            }
        }

        [UnityTest]
        public IEnumerator B16B04_CancelCaptureOutBlurDetachCloseGeometrySelectionAndOutsideNeverSubmit()
        {
            UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene,
                UnityEditor.SceneManagement.NewSceneMode.Single);
            yield return new EnterPlayMode();
            yield return BattleCancellationCases.AssertDisableAndExplicitCancellationOrdersBody();
            foreach (var cause in new[] { "cancel", "captureout", "blur", "detach", "close", "geometry", "transform", "selection", "outside" })
            {
                if (cause == "blur")
                {
                    using (var focus = new UguiHostRig(true))
                    {
                        focus.Enter(); var head = focus.Head; var calls = focus.Storage.SnapshotCreates;
                        var files = PlayerSessionTestData.CopyFiles(focus.Storage.Files);
                        yield return focus.Ready();
                        focus.BeginRoute(); var input = focus.Session.Battle.Input;
                        Assert.AreEqual(GestureStage.Dragging, input.Gesture.Stage); Assert.IsTrue(focus.Board.HasActivePointer);
                        var cells = input.Gesture.DraftCells.ToArray(); var pointer = input.Gesture.ActivePointerId;
                        focus.Host.HandleApplicationFocus(true);
                        Assert.AreEqual(GestureStage.Dragging, input.Gesture.Stage); Assert.AreEqual(pointer, input.Gesture.ActivePointerId);
                        CollectionAssert.AreEqual(cells, input.Gesture.DraftCells); Assert.IsTrue(focus.Board.HasActivePointer);
                        Assert.AreSame(head, focus.Head); Assert.AreEqual(calls, focus.Storage.SnapshotCreates);
                        PlayerSessionTestData.SameFiles(files, focus.Storage.Files);
                        using (var observed = new BattleFeedbackObserver(input))
                        {
                            focus.Host.HandleApplicationFocus(false);
                            CollectionAssert.AreEqual(new[] { CandidateBoardVisibleFeedbackKind.FocusLost }, observed.Published);
                        }
                        Assert.AreEqual(GestureStage.Idle, input.Gesture.Stage); Assert.IsFalse(focus.Board.HasActivePointer);
                        Assert.IsNull(input.Gesture.ActivePointerId); focus.EndRoute(); focus.EndRoute();
                        Assert.IsNull(input.LastRequest); Assert.AreSame(head, focus.Head); Assert.AreEqual(calls, focus.Storage.SnapshotCreates);
                        PlayerSessionTestData.SameFiles(files, focus.Storage.Files);
                    }
                    continue;
                }
            using (var r = new BoardPanelRig())
            {
                yield return r.Ready(); r.Controller.SelectMember("W"); var route = r.Route();
                var head = r.Battle.Head; var calls = r.Battle.Runtime.Storage.Base.Calls;
                r.Down(r.Point(route[0])); r.Move(r.Point(route[1])); Assert.AreEqual(GestureStage.Dragging, r.Controller.Gesture.Stage, cause);
                var releasePoint = r.Point(route.Last());
                if (cause == "cancel") r.Cancel();
                if (cause == "captureout")
                {
                    UnityEngine.EventSystems.ExecuteEvents.Execute(r.Board.gameObject,
                        new UnityEngine.EventSystems.PointerEventData(r.Canvas.Driver.EventSystem) { pointerId = -1 },
                        UnityEngine.EventSystems.ExecuteEvents.pointerExitHandler);
                }
                if (cause == "detach") r.Board.gameObject.SetActive(false);
                if (cause == "close") r.UI.Close();
                if (cause == "geometry") { r.Resize(50); yield return r.Ready(); }
                if (cause == "transform") { var local = r.Board.CellCenter(route[1]); r.Board.transform.localScale = new Vector3(2, 1, 1); r.Move(r.Local(local.x, local.y)); r.Board.transform.localScale = Vector3.one; }
                if (cause == "selection") r.Controller.SelectMember(null);
                if (cause == "outside") r.Move(r.Local(-1, -1));
                Assert.AreEqual(GestureStage.Idle, r.Controller.Gesture.Stage, cause); Assert.IsFalse(r.HasCapture(-1), cause);
                if (cause == "detach") { r.Board.gameObject.SetActive(true); yield return r.Ready(); }
                if (cause == "close") Assert.IsFalse(r.Board.raycastTarget);
                r.Up(releasePoint); r.Up(releasePoint);
                Assert.IsNull(r.Controller.LastRequest, cause); Assert.AreSame(head, r.Battle.Head, cause); Assert.AreEqual(calls, r.Battle.Runtime.Storage.Base.Calls, cause);
                TestContext.Out.WriteLine("uGUI cancellation: " + cause);
            }
            }
            yield return new ExitPlayMode();
        }

        [UnityTest]
        public IEnumerator B16B03_BottomLeftHalfOpenBoundariesAndOutsideMapThroughHandlers()
        {
            using (var r = new BoardPanelRig())
            {
                yield return r.Ready(); var h = r.Battle.State.Board.Face.Height * r.CellSize; var w = r.Battle.State.Board.Face.Width * r.CellSize;
                r.Down(r.Local(0, 0)); Assert.AreEqual(new FlowPos(0, 0), r.Controller.Gesture.OriginCell); r.Controller.CancelGesture();
                foreach (var p in new[] { r.Local(-0.25f, 0), r.Local(0, -0.25f), r.Local(w, 20), r.Local(20, h) })
                { r.Down(p); Assert.IsNull(r.Controller.Gesture.ActivePointerId); }
                r.Down(r.Local(20, h - 0.25f)); Assert.AreEqual(new FlowPos(0, 3), r.Controller.Gesture.OriginCell); r.Controller.CancelGesture();
                r.Down(r.Local(40, 60)); Assert.AreEqual(new FlowPos(1, 1), r.Controller.Gesture.OriginCell); r.Controller.CancelGesture();
                r.Down(r.Local(39.75f, 60)); Assert.IsNull(r.Controller.Gesture.ActivePointerId);
                Assert.IsNull(r.Controller.LastRequest);
            }
        }

        [UnityTest]
        public IEnumerator B16B03_MouseTouchClampsAndUniformScaleUsePanelPointDistances()
        {
            using (var r = new BoardPanelRig())
            {
                yield return r.Ready(); r.Controller.SelectMember("W");
                foreach (var size in new[] { 20f, 40f, 80f })
                {
                    r.Resize(size); yield return r.Ready();
                    foreach (var touch in new[] { false, true })
                    {
                        var type = touch ? PointerType.touch : PointerType.mouse; var id = touch ? 1 : -1;
                        var limit = System.Math.Max(size * 0.15f, System.Math.Min(touch ? 10 : 6, size * 0.25f)); var p = r.Point(r.Route()[0]);
                        r.Down(p, id, type); r.Move(p + new Vector2(limit, 0), id, type);
                        Assert.AreEqual(GestureStage.Pressed, r.Controller.Gesture.Stage, size + "/" + type);
                        r.Move(p + new Vector2(limit + 0.25f, 0), id, type); Assert.AreEqual(GestureStage.Dragging, r.Controller.Gesture.Stage);
                        r.Cancel(id, type);
                    }
                }
                r.Resize(40); r.Board.transform.localScale = new Vector3(2, 2, 1); yield return r.Ready();
                var start = r.Point(r.Route()[0]); r.Down(start); r.Move(start + new Vector2(12, 0)); Assert.AreEqual(GestureStage.Pressed, r.Controller.Gesture.Stage);
                r.Move(start + new Vector2(12.25f, 0)); Assert.AreEqual(GestureStage.Dragging, r.Controller.Gesture.Stage); r.Cancel();
                var invalidStart = r.Board.CellCenter(r.Route()[0]); r.Board.transform.localScale = new Vector3(2, 1, 1); r.Down(r.Local(invalidStart.x, invalidStart.y)); Assert.IsNull(r.Controller.Gesture.ActivePointerId);
                Assert.IsNull(r.Controller.LastRequest);
            }
        }

        [UnityTest]
        public IEnumerator B16B03_UnselectedOrNonAdjacentRouteDoesNotPrepare()
        {
            using (var r = new BoardPanelRig())
            {
                yield return r.Ready(); var before = r.Battle.Head;
                r.Draw(); Assert.IsNull(r.Controller.LastRequest); Assert.IsNull(r.Controller.LastPreview);
                r.Controller.SelectMember("W"); r.Down(r.Point(r.Route()[0])); r.Up(r.Point(r.Route().Last()));
                Assert.IsNull(r.Controller.LastRequest); Assert.AreSame(before, r.Battle.Head);
            }
        }

        [UnityTest]
        public IEnumerator B16B03_UpDisplacementWithoutMoveCannotBecomeHistoryTap()
        {
            using (var r = new BoardPanelRig(level: 3))
            {
                yield return r.Ready(); r.Battle.Attack(); r.Controller.Refresh(); r.Controller.SelectMember("W");
                var before = r.Battle.Head; var p = r.Point(r.Route()[0]);
                r.Down(p); r.Up(p + new Vector2(7, 0));
                Assert.IsNull(r.Controller.LastPreview); Assert.IsNull(r.Controller.LastRequest); Assert.AreSame(before, r.Battle.Head);
            }
        }

        [UnityTest]
        public IEnumerator B16B03_ExternalSceneChangeCancelsFrozenRouteWithoutRetagging()
        {
            using (var r = new BoardPanelRig())
            {
                yield return r.Ready(); r.Controller.SelectMember("W");
                r.Down(r.Point(r.Route()[0])); r.Move(r.Point(r.Route()[1]));
                r.Battle.Attack(); var head = r.Battle.Head; r.Up(r.Point(r.Route().Last()));
                Assert.IsNull(r.Controller.LastRequest); Assert.AreSame(head, r.Battle.Head); Assert.IsFalse(r.HasCapture(-1));
                Assert.AreEqual(r.Battle.State.SceneRevision, r.Controller.InputContext.SceneRevision);
            }
        }

        [UnityTest]
        public IEnumerator B16B06_ExternalPendingCancelsRouteAndRecoveryDoesNotQueueIt()
        {
            using (var r = new BoardPanelRig())
            {
                yield return r.Ready(); r.Controller.SelectMember("W"); r.Down(r.Point(r.Route()[0])); r.Move(r.Point(r.Route()[1]));
                var request = r.Battle.Freeze(r.Battle.AttackDraft()); r.Battle.Runtime.Storage.Base.Arm("Snapshot.Flush.after", null);
                Is(r.Battle.System.Submit(request, B()), "SaveFailed"); r.Controller.Refresh();
                Assert.AreEqual(GestureStage.Idle, r.Controller.Gesture.Stage); Assert.IsFalse(r.Controller.View.IsPublishedHeadVerified);
                r.Up(r.Point(r.Route().Last())); Assert.IsNull(r.Controller.LastRequest);
                Is(r.Battle.System.Retry(request, B()), "Completed"); var head = r.Battle.Head;
                r.Controller.RebuildLatest(); r.Up(r.Point(r.Route().Last())); Assert.AreSame(head, r.Battle.Head); Assert.IsNull(r.Controller.LastRequest);
            }
        }

        [UnityTest]
        public IEnumerator B16B05_LockedLineTapNeedsNoActorAndSingleHistoryCommitsOnlyOnUp()
        {
            using (var r = new BoardPanelRig())
            {
                yield return r.Ready(); var initial = r.Battle.State; r.Battle.Attack(); r.Controller.Refresh();
                Assert.IsNull(r.Controller.SelectedCharacterId); var route = r.Battle.State.Board.LockedRoutes.Single().Route;
                CollectionAssert.AreEqual(route, r.Controller.InputContext.Pairs[0].LockedCells);
                var before = r.Battle.Head; var p = r.Point(route[1]); var count = 0; r.Controller.ResultReceived += x => count++;
                r.Down(p); Assert.AreSame(before, r.Battle.Head); Assert.IsNull(r.Controller.LastPreview); r.Up(p);
                Is(r.Controller.LastResult, "Completed"); Assert.AreEqual(CandidateApplicationKind.Rollback, r.Controller.LastRequest.Kind);
                Assert.AreEqual(1, count); Same(initial.Members, r.Battle.State.Members); Same(initial.Enemies, r.Battle.State.Enemies);
                Assert.AreEqual(BigInteger.Zero, r.Battle.State.EffectiveActionsCompleted); Assert.AreEqual(0, r.Battle.State.Board.LockedRoutes.Count);
                Assert.Greater(r.Battle.State.SceneRevision, before.Business.ActiveHistory.CurrentRun.CurrentSnapshot.SceneRevision);
                var head = r.Battle.Head; r.Up(p); Assert.AreSame(head, r.Battle.Head); Assert.AreEqual(1, count);
            }
        }

        [UnityTest]
        public IEnumerator B16B05_EmptyCellDoesNotSearchGlobalHistory()
        {
            using (var r = new BoardPanelRig())
            {
                yield return r.Ready(); r.Battle.Attack(); r.Controller.Refresh(); var head = r.Battle.Head;
                var calls = r.Battle.Runtime.Storage.Base.Calls; r.Tap(new FlowPos(3, 0));
                Assert.IsNull(r.Controller.LastPreview); Assert.IsNull(r.Controller.LastRequest); Assert.AreSame(head, r.Battle.Head);
                Assert.AreEqual(calls, r.Battle.Runtime.Storage.Base.Calls);
            }
        }

        [UnityTest]
        public IEnumerator B16B05_MultipleHistoryHandsOffExactRangeAndConfirmsOnce()
        {
            using (var r = new BoardPanelRig(level: 3))
            {
                yield return r.Ready(); r.Battle.Attack(0); r.Battle.Attack(1); r.Controller.Refresh(); var head = r.Battle.Head; var confirmations = 0;
                r.Controller.RollbackConfirmationRequired += x => confirmations++;
                r.Tap(r.Route()[0]); var range = r.Controller.RollbackPreview;
                Assert.AreEqual(2, range.Entries.Count); Assert.AreEqual(1, confirmations); Assert.IsNull(r.Controller.LastRequest); Assert.AreSame(head, r.Battle.Head);
                var ids = range.Entries.Select(x => x.OperationId).ToArray();
                Is(r.Controller.ConfirmRollback(), "Completed"); Assert.IsNull(r.Controller.RollbackPreview);
                Same(range.BeforeSnapshot.Members, r.Battle.State.Members); Same(range.BeforeSnapshot.Enemies, r.Battle.State.Enemies); Same(range.BeforeSnapshot.Random, r.Battle.State.Random);
                Assert.AreEqual(range.BeforeSnapshot.EffectiveActionsCompleted, r.Battle.State.EffectiveActionsCompleted);
                CollectionAssert.AreEqual(ids, r.Battle.Head.Business.ActiveHistory.RollbackRecords.Last().Range.Entries.Select(x => x.OperationId));
                var final = r.Battle.Head; Assert.IsNull(r.Controller.ConfirmRollback()); Assert.AreSame(final, r.Battle.Head);
            }
        }

        [UnityTest]
        public IEnumerator B16B05_ChangedSceneClosesOriginalConfirmationWithoutChangingItsRange()
        {
            using (var r = new BoardPanelRig(level: 3))
            {
                yield return r.Ready(); r.Battle.Attack(0); r.Battle.Attack(1); r.Controller.Refresh(); r.Tap(r.Route()[0]);
                var range = r.Controller.RollbackPreview; var ids = range.Entries.Select(x => x.OperationId).ToArray();
                r.Battle.Attack(0); var head = r.Battle.Head; var calls = r.Battle.Runtime.Storage.Base.Calls;
                Assert.IsNull(r.Controller.ConfirmRollback()); Assert.IsNull(r.Controller.RollbackPreview); Assert.AreEqual("StaleContext", r.Controller.Status);
                Assert.AreSame(head, r.Battle.Head); Assert.AreEqual(calls, r.Battle.Runtime.Storage.Base.Calls);
                CollectionAssert.AreEqual(ids, range.Entries.Select(x => x.OperationId)); Assert.IsNull(r.Controller.LastRequest);
            }
        }

        [UnityTest]
        public IEnumerator B16B05_CancellingConfirmationOnlyClearsLocalPreview()
        {
            using (var r = new BoardPanelRig(level: 3))
            {
                yield return r.Ready(); r.Battle.Attack(0); r.Battle.Attack(1); r.Controller.Refresh(); r.Tap(r.Route()[0]);
                Assert.IsNotNull(r.Controller.RollbackPreview); var head = r.Battle.Head; var calls = r.Battle.Runtime.Storage.Base.Calls;
                r.Controller.CancelRollback(); Assert.IsNull(r.Controller.RollbackPreview); Assert.IsNull(r.Controller.ConfirmRollback());
                Assert.AreSame(head, r.Battle.Head); Assert.AreEqual(calls, r.Battle.Runtime.Storage.Base.Calls);
            }
        }

        [UnityTest]
        public IEnumerator B16B06_SaveFailedAndUnknownKeepOriginalPreparedBytesAndOnePlay()
        {
            foreach (var code in new[] { "SaveFailed", "CommitUnknown" })
            using (var r = new BoardPanelRig())
            {
                yield return r.Ready(); r.Controller.SelectMember("W"); var head = r.Battle.Head; var plays = 0;
                r.Controller.PresentationReady += p => plays++;
                r.Battle.Runtime.Storage.PublishedCommit = null;
                r.Battle.Runtime.Storage.Base.Arm(code == "SaveFailed" ? "Snapshot.Flush.after" : "Marker.Promote.after", null);
                r.Draw(); Is(r.Controller.LastResult, code); Assert.IsTrue(r.Battle.Runtime.Storage.Base.FaultUsed);
                Assert.AreSame(head, r.Controller.View.PublishedSnapshot); Assert.AreEqual(0, plays); Assert.IsFalse(r.Controller.View.IsPublishedHeadVerified);
                BattleCopyAssert.Localized(r.UI, "save-status", r.Canvas.Localization,
                    code == "SaveFailed" ? "fm.save_recovery.failed" : "fm.save_recovery.unknown",
                    code == "SaveFailed" ? new[] { BattleText.Arg("errorCode", code) } : Array.Empty<System.Collections.Generic.KeyValuePair<string, string>>());
                Assert.AreEqual(code == "SaveFailed", r.Controller.CanRetry); Assert.AreEqual(code == "CommitUnknown", r.Controller.CanResolve);
                var prepared = r.Controller.LastRequest; var canonical = prepared.Intent.CanonicalBytes.ToArray();
                var commit = r.Controller.View.ApplicationView.PendingCommitId; var files = r.Battle.Runtime.Disk();
                var original = files[code == "SaveFailed" ? "w-" + commit + ".snapshot.tmp" : SnapshotName(commit)]; var calls = r.Battle.Runtime.Storage.Base.Calls;
                r.Draw(); r.Controller.QueryLastOperation(); Assert.AreSame(prepared, r.Controller.LastRequest); Assert.AreEqual(calls, r.Battle.Runtime.Storage.Base.Calls);
                var result = code == "SaveFailed" ? r.Controller.RetryLast() : r.Controller.ResolveLast(); Is(result, "Completed");
                Assert.AreEqual(commit, result.Application.OriginalCommitId); Assert.AreEqual(1, plays); Assert.AreSame(prepared, r.Controller.LastRequest);
                CollectionAssert.AreEqual(canonical, prepared.Intent.CanonicalBytes);
                CollectionAssert.AreEqual(original, File.ReadAllBytes(r.Battle.Runtime.Path(SnapshotName(commit))));
                Assert.AreEqual(1, r.Battle.Runtime.Storage.Base.RealMarkerPromotionsFor(commit));
                r.Controller.QueryLastOperation(); r.Controller.RetryLast(); r.Controller.ResolveLast(); Assert.AreEqual(1, plays);
                Assert.IsNotNull(r.Controller.View.PresentationToken); TestContext.Out.WriteLine("Real UI recovery: " + code);
            }
        }

        [UnityTest]
        public IEnumerator CH1_L5_RecoveryBlockedKeepsOriginalAttackAndBilingualBlockingNotice()
        {
            using (var r = new BoardPanelRig(classId: "class:warrior"))
            {
                yield return r.Ready(); Assert.IsTrue(r.Controller.SelectMember("W"));
                Assert.AreEqual("class:warrior", r.Battle.State.Members.Single().Member.ClassId);
                Assert.IsNull(r.Canvas.Localization.BindingDiagnostic, "The normal ready view must bind before injecting a recovery fault.");
                var head = r.Battle.Head;
                r.Battle.Runtime.Storage.Base.Arm("Snapshot.Flush.after", null);
                r.Draw(); Is(r.Controller.LastResult, "SaveFailed");
                Assert.IsTrue(r.Battle.Runtime.Storage.Base.FaultUsed);
                var original = r.Controller.LastRequest; var canonical = original.Intent.CanonicalBytes.ToArray();
                var commit = r.Controller.View.ApplicationView.PendingCommitId; var readBlocked = false;
                try
                {
                    r.Battle.Runtime.Storage.BeforeOpen = _ => { readBlocked = true; throw new IOException("CH1-L5 original candidate lookup blocked"); };
                    Is(r.Controller.ResolveLast(), "RecoveryBlocked");
                }
                finally { r.Battle.Runtime.Storage.BeforeOpen = null; }
                Assert.IsTrue(readBlocked);
                Assert.AreSame(original, r.Controller.LastRequest);
                CollectionAssert.AreEqual(canonical, original.Intent.CanonicalBytes);
                Assert.AreEqual(commit, r.Controller.View.ApplicationView.PendingCommitId);
                AssertRecoveryBlockingNoticeWithoutBusinessWrites(r, CandidateApplicationPhase.RecoveryBlocked, false, head);
            }
        }

        [UnityTest]
        public IEnumerator CH1_L5_RestoreRequiredKeepsCommittedAttackAndBilingualBlockingNotice()
        {
            using (var r = new BoardPanelRig(classId: "class:warrior"))
            {
                yield return r.Ready(); Assert.IsTrue(r.Controller.SelectMember("W"));
                Assert.AreEqual("class:warrior", r.Battle.State.Members.Single().Member.ClassId);
                Assert.IsNull(r.Canvas.Localization.BindingDiagnostic, "The normal ready view must bind before injecting a recovery fault.");
                var head = r.Battle.Head;
                r.Battle.Runtime.Storage.PublishedCommit = null; var loadBlocked = false;
                try
                {
                    // Existing B15_05 boundary: the marker is committed before the fourth enumeration loads it.
                    r.Battle.Runtime.Storage.AfterMarkerEnumeration = count =>
                    {
                        if (count != 4) return;
                        loadBlocked = true; throw new IOException("CH1-L5 committed candidate load blocked");
                    };
                    r.Draw();
                }
                finally { r.Battle.Runtime.Storage.AfterMarkerEnumeration = null; }
                Assert.IsTrue(loadBlocked);
                Is(r.Controller.LastResult, "CommittedRestoreRequired");
                AssertRecoveryBlockingNoticeWithoutBusinessWrites(r, CandidateApplicationPhase.RestoreRequired, true, head);
            }
        }

        private static void AssertRecoveryBlockingNoticeWithoutBusinessWrites(BoardPanelRig r,
            CandidateApplicationPhase phase, bool committed, CandidateApplicationSnapshot head)
        {
            var input = r.Controller; var result = input.LastResult; var prepared = input.LastRequest;
            Assert.IsNotNull(prepared); Assert.IsNotNull(result.Application.Diagnostic);
            Assert.AreEqual(phase, input.View.Phase);
            Assert.AreEqual(phase.ToString(), input.View.AttackFor("W").Reason);
            Assert.AreEqual("W", input.SelectedCharacterId);
            Assert.AreSame(head, input.View.PublishedSnapshot);
            Assert.AreEqual(committed, result.Application.IsCommitted);
            Assert.IsNull(result.Presentation); Assert.IsNull(input.View.PresentationToken);
            var commit = input.View.ApplicationView.PendingCommitId; var canonical = prepared.Intent.CanonicalBytes.ToArray();
            Assert.IsNotNull(commit);
            Assert.AreEqual(commit, result.Application.OriginalCommitId);
            Assert.AreEqual(prepared.OperationId, input.View.ApplicationView.PendingOperationId);
            Assert.AreEqual(committed ? 1 : 0, r.Battle.Runtime.Storage.Base.RealMarkerPromotionsFor(commit));
            Assert.AreEqual(committed, File.Exists(r.Battle.Runtime.Path(MarkerName(commit))));
            if (committed) Assert.IsTrue(File.Exists(r.Battle.Runtime.Path(SnapshotName(commit))));
            var files = r.Battle.Runtime.Disk(); var calls = r.Battle.Runtime.Storage.Base.Calls;
            var writes = r.Battle.Runtime.Storage.Base.WriteCalls;
            foreach (var locale in new[] { LocaleId.En, LocaleId.ZhHans })
            {
                r.Canvas.Localization.SetLocale(locale); input.Refresh(); UnityEngine.Canvas.ForceUpdateCanvases();
                var line = BattleCopyAssert.Localized(r.UI, "board-availability:fm.save_recovery.blocking_notice",
                    r.Canvas.Localization, "fm.save_recovery.blocking_notice");
                Assert.IsTrue(line.gameObject.activeInHierarchy);
                Assert.IsNull(r.Canvas.Localization.BindingDiagnostic);
                Assert.AreSame(prepared, input.LastRequest); Assert.AreSame(result, input.LastResult);
                CollectionAssert.AreEqual(canonical, input.LastRequest.Intent.CanonicalBytes);
                Assert.AreEqual(prepared.OperationId, input.View.ApplicationView.PendingOperationId);
                Assert.AreEqual(commit, input.View.ApplicationView.PendingCommitId);
                Assert.AreEqual(committed, input.LastResult.Application.IsCommitted);
                Assert.AreEqual(phase, input.View.Phase);
                Assert.AreEqual(phase.ToString(), input.View.AttackFor("W").Reason);
                Assert.AreSame(head, input.View.PublishedSnapshot);
                // This correction must not turn unrecognized Input reasons into generic recovery copy.
                var unknown = BattleText.Reason(BattleTextDomain.Input, "FutureUnmappedInputReason");
                var diagnostic = r.Canvas.Localization.Resolve(unknown.Key, unknown.Arguments);
                Assert.IsFalse(diagnostic.IsSuccess); Assert.AreEqual("InvalidLocalizationKey", diagnostic.DiagnosticCode);
            }
            Assert.AreEqual(calls, r.Battle.Runtime.Storage.Base.Calls);
            Assert.AreEqual(writes, r.Battle.Runtime.Storage.Base.WriteCalls);
            Assert.AreEqual(committed ? 1 : 0, r.Battle.Runtime.Storage.Base.RealMarkerPromotionsFor(commit));
            r.Battle.Runtime.SameDisk(files);
        }

        [UnityTest]
        public IEnumerator B16B06_PlayLatchOldTokenAndRebuildNeverQueueOrSettleRewards()
        {
            using (var r = new BoardPanelRig())
            {
                yield return r.Ready(); r.Controller.SelectMember("W"); var plays = 0; r.Controller.PresentationReady += p => plays++;
                r.Draw(); Is(r.Controller.LastResult, "Completed"); var firstToken = r.Controller.LastResult.Presentation.Token; var head = r.Battle.Head;
                r.Controller.QueryLastOperation(); r.Draw(1); Assert.AreSame(head, r.Battle.Head); Assert.AreEqual(1, plays);
                Is(r.Battle.System.ReportPresentationCompleted(firstToken), "PresentationCompleted"); r.Controller.Refresh(); Assert.AreSame(head, r.Battle.Head);
                r.Draw(1); Is(r.Controller.LastResult, "Completed"); Assert.AreEqual(2, plays); var token = r.Controller.View.PresentationToken; var latest = r.Battle.Head;
                Is(r.Battle.System.ReportPresentationCompleted(firstToken), "PresentationIgnored"); r.Controller.Refresh(); Assert.AreSame(token, r.Controller.View.PresentationToken);
                r.Controller.RebuildLatest(); Assert.IsNull(r.Controller.View.PresentationToken); Assert.AreSame(latest, r.Battle.Head); Assert.AreEqual(2, plays);
                Assert.AreEqual(BattlePhase.WonPendingSettlement, r.Battle.State.Phase); Assert.IsNotNull(r.Battle.Head.Continuation);
                Assert.AreEqual(0, r.Battle.Head.Business.Rewards.BaseRewards.Count); Assert.IsFalse(r.Controller.View.Attack.IsAvailable);
            }
        }

        [UnityTest]
        public IEnumerator B16B06_OldOperationQueryRetainsLatestPublishedHeadAndDoesNotReplay()
        {
            using (var r = new BoardPanelRig())
            {
                yield return r.Ready(); r.Controller.SelectMember("W"); var plays = 0; r.Controller.PresentationReady += p => plays++;
                r.Draw(); var original = r.Controller.LastResult; Is(original, "Completed");
                Is(r.Battle.System.ReportPresentationCompleted(original.Presentation.Token), "PresentationCompleted");
                r.Battle.Attack(1); var head = r.Battle.Head; var result = r.Controller.QueryLastOperation();
                Assert.AreEqual(original.Application.OriginalCommitId, result.Application.OriginalCommitId);
                Assert.AreEqual(head.Header.CommitId, result.View.CommitId); Assert.AreSame(head, r.Controller.View.PublishedSnapshot); Assert.AreEqual(1, plays);
                Assert.AreEqual(CandidatePresentationDisposition.RebuildLatest, result.PresentationDisposition);
            }
        }

        [UnityTest]
        public IEnumerator B16B02_DownedMemberIsUnselectableButActorlessHistoryStillWorks()
        {
            using (var r = new BoardPanelRig(level: 3, hp: 1))
            {
                yield return r.Ready(); r.Controller.SelectMember("W"); r.Battle.Attack(); r.Controller.Refresh();
                Assert.AreEqual(BattlePhase.AwaitRescue, r.Battle.State.Phase); Assert.IsNull(r.Controller.SelectedCharacterId);
                Assert.IsFalse(r.Controller.SelectMember("W")); Assert.IsFalse(r.UI.Find<UnityEngine.UI.Button>(FightMatchViewId.Member("W")).interactable);
                Assert.IsTrue(r.Controller.InputContext.Pairs.All(x => !x.CanDraw)); Assert.IsTrue(r.Controller.InputContext.AllowHistoryTap);
                r.Draw(); Assert.IsNull(r.Controller.LastRequest); r.Tap(r.Route()[0]);
                Is(r.Controller.LastResult, "Completed"); Assert.Greater(r.Battle.State.Members[0].Hp.Numerator.Sign, 0); Assert.IsNull(r.Controller.SelectedCharacterId);
            }
        }

        [UnityTest]
        public IEnumerator B16B06_EndFailedSaveRestoresOriginalHeadWithoutResubmission()
        {
            using (var r = new BoardPanelRig())
            {
                yield return r.Ready(); r.Controller.SelectMember("W"); var head = r.Battle.Head;
                r.Battle.Runtime.Storage.Base.Arm("Snapshot.Flush.after", null); r.Draw(); Is(r.Controller.LastResult, "SaveFailed");
                var oldId = r.Controller.LastRequest.OperationId; Is(r.Controller.EndLast(), "Ended");
                Assert.AreEqual(head.Header.CommitId, r.Controller.View.CommitId); Assert.IsNull(r.Controller.View.ApplicationView.PendingOperationId);
                r.Controller.Refresh(); Assert.AreEqual(head.Records.Count, r.Battle.Head.Records.Count);
                r.Draw(); Is(r.Controller.LastResult, "Completed"); Assert.AreNotEqual(oldId, r.Controller.LastRequest.OperationId);
            }
        }

        [UnityTest]
        public IEnumerator B16B06_PreparationBudgetRefusalDoesNotPublishOrFabricateSuccess()
        {
            using (var r = new BoardPanelRig())
            {
                r.UI.Attach(r.Battle.System, new SaveStoreBudget(new SaveCodecBudget(new ExactMathBudget(maxPrimitiveSteps: 0))), r.Canvas.Localization);
                r.Resize(40); yield return r.Ready(); r.Controller.SelectMember("W"); var head = r.Battle.Head; var calls = r.Battle.Runtime.Storage.Base.Calls;
                r.Draw(); Assert.IsNotNull(r.Controller.LastPreparation); Assert.AreEqual("Limit", r.Controller.LastPreparation.Code);
                Assert.IsNull(r.Controller.LastRequest); Assert.AreSame(head, r.Battle.Head); Assert.AreEqual(calls, r.Battle.Runtime.Storage.Base.Calls);
            }
        }
    }
}
