using System;
using System.Collections;
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
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;
using static FightMatch.Core.Tests.BusinessSaveScenario;
using static FightMatch.Core.Tests.LocalSaveTestFiles;
using static FightMatch.Core.Tests.BattleApplicationRig;

namespace FightMatch.Core.Tests
{
    public sealed class CandidateBoardInputTests
    {
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
                r.Down(r.Point(route[0])); Assert.IsTrue(r.Board.HasPointerCapture(0)); Assert.AreEqual(0, r.Controller.Gesture.ActivePointerId);
                Assert.AreSame(before, r.Battle.Head); Assert.AreEqual(calls, r.Battle.Runtime.Storage.Base.Calls); Assert.IsNull(r.Controller.LastRequest);
                r.Controller.Refresh(); r.Controller.Refresh(); Assert.AreEqual(GestureStage.Pressed, r.Controller.Gesture.Stage);
                for (var i = 1; i < route.Count - 1; i++) r.Move(r.Point(route[i]));
                Assert.AreSame(before, r.Battle.Head); r.Up(r.Point(route.Last()));
                Is(r.Controller.LastResult, "Completed"); Assert.AreEqual(1, results); Assert.IsFalse(r.Board.HasPointerCapture(0));
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
                r.Down(r.Point(route[0]), 1, PointerType.touch); Assert.IsTrue(r.Board.HasPointerCapture(1));
                r.Down(r.Point(route.Last()), 2, PointerType.touch); r.Move(r.Point(route.Last()), 2, PointerType.touch);
                r.Up(r.Point(route.Last()), 2, PointerType.touch); r.Cancel(2, PointerType.touch);
                Assert.AreEqual(1, r.Controller.Gesture.ActivePointerId); Assert.IsTrue(r.Board.HasPointerCapture(1)); Assert.IsFalse(r.Board.HasPointerCapture(2));
                for (var i = 1; i < route.Count - 1; i++) r.Move(r.Point(route[i]), 1, PointerType.touch);
                r.Up(r.Point(route.Last()), 1, PointerType.touch); Is(r.Controller.LastResult, "Completed");
                Assert.IsFalse(r.Board.HasPointerCapture(1));
            }
        }

        [UnityTest]
        public IEnumerator B16B04_CancelCaptureOutBlurDetachCloseGeometrySelectionAndOutsideNeverSubmit()
        {
            foreach (var cause in new[] { "cancel", "captureout", "blur", "detach", "close", "geometry", "transform", "selection", "outside" })
            using (var r = new BoardPanelRig())
            {
                yield return r.Ready(); r.Controller.SelectMember("W"); var route = r.Route();
                var head = r.Battle.Head; var calls = r.Battle.Runtime.Storage.Base.Calls;
                r.Down(r.Point(route[0])); r.Move(r.Point(route[1])); Assert.AreEqual(GestureStage.Dragging, r.Controller.Gesture.Stage, cause);
                if (cause == "cancel") r.Cancel();
                if (cause == "captureout")
                {
                    r.Board.ReleasePointer(0);
                    using (var e = PointerCaptureOutEvent.GetPooled(r.Board, r.Peer, 0)) { e.target = r.Board; r.Board.SendEvent(e); }
                }
                if (cause == "blur") { r.Peer.Focus(); yield return null; }
                if (cause == "detach") r.Board.RemoveFromHierarchy();
                if (cause == "close") r.UI.Close();
                if (cause == "geometry") { r.Resize(50); yield return r.Ready(); }
                if (cause == "transform") { r.Board.transform.scale = new Vector3(2, 1, 1); r.Move(r.Point(route[1])); r.Board.transform.scale = Vector3.one; }
                if (cause == "selection") r.Controller.SelectMember(null);
                if (cause == "outside") r.Move(r.Local(-1, -1));
                Assert.AreEqual(GestureStage.Idle, r.Controller.Gesture.Stage, cause); Assert.IsFalse(r.Board.HasPointerCapture(0), cause);
                if (cause == "detach") { r.UI.Add(r.Board); yield return r.Ready(); }
                r.Up(r.Point(route.Last())); r.Up(r.Point(route.Last()));
                Assert.IsNull(r.Controller.LastRequest, cause); Assert.AreSame(head, r.Battle.Head, cause); Assert.AreEqual(calls, r.Battle.Runtime.Storage.Base.Calls, cause);
                TestContext.Out.WriteLine("Real panel cancellation: " + cause);
            }
        }

        [UnityTest]
        public IEnumerator B16B03_BottomLeftHalfOpenBoundariesAndOutsideMapThroughHandlers()
        {
            using (var r = new BoardPanelRig())
            {
                yield return r.Ready(); var h = r.Battle.State.Board.Face.Height * r.CellSize; var w = r.Battle.State.Board.Face.Width * r.CellSize;
                r.Down(r.Local(0, h)); Assert.AreEqual(new FlowPos(0, 0), r.Controller.Gesture.OriginCell); r.Controller.CancelGesture();
                foreach (var p in new[] { r.Local(-0.25f, h), r.Local(0, h + 0.25f), r.Local(w, h - 20), r.Local(20, 0) })
                { r.Down(p); Assert.IsNull(r.Controller.Gesture.ActivePointerId); }
                r.Down(r.Local(20, 0.25f)); Assert.AreEqual(new FlowPos(0, 3), r.Controller.Gesture.OriginCell); r.Controller.CancelGesture();
                r.Down(r.Local(40, h - 60)); Assert.AreEqual(new FlowPos(1, 1), r.Controller.Gesture.OriginCell); r.Controller.CancelGesture();
                r.Down(r.Local(39.75f, h - 60)); Assert.IsNull(r.Controller.Gesture.ActivePointerId);
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
                        var type = touch ? PointerType.touch : PointerType.mouse; var id = touch ? 1 : 0;
                        var limit = System.Math.Max(size * 0.15f, System.Math.Min(touch ? 10 : 6, size * 0.25f)); var p = r.Point(r.Route()[0]);
                        r.Down(p, id, type); r.Move(p + new Vector2(limit, 0), id, type);
                        Assert.AreEqual(GestureStage.Pressed, r.Controller.Gesture.Stage, size + "/" + type);
                        r.Move(p + new Vector2(limit + 0.25f, 0), id, type); Assert.AreEqual(GestureStage.Dragging, r.Controller.Gesture.Stage);
                        r.Cancel(id, type);
                    }
                }
                r.Resize(40); r.Board.transform.scale = new Vector3(2, 2, 1); yield return r.Ready();
                var start = r.Point(r.Route()[0]); r.Down(start); r.Move(start + new Vector2(12, 0)); Assert.AreEqual(GestureStage.Pressed, r.Controller.Gesture.Stage);
                r.Move(start + new Vector2(12.25f, 0)); Assert.AreEqual(GestureStage.Dragging, r.Controller.Gesture.Stage); r.Cancel();
                r.Board.transform.scale = new Vector3(2, 1, 1); r.Down(r.Point(r.Route()[0])); Assert.IsNull(r.Controller.Gesture.ActivePointerId);
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
                Assert.IsNull(r.Controller.LastRequest); Assert.AreSame(head, r.Battle.Head); Assert.IsFalse(r.Board.HasPointerCapture(0));
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
                StringAssert.Contains(code, r.UI.Q<Label>("save-status").text);
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
                Assert.IsFalse(r.Controller.SelectMember("W")); Assert.IsFalse(r.UI.Q<Button>("member-W").enabledSelf);
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
                r.UI.Attach(r.Battle.System, new SaveStoreBudget(new SaveCodecBudget(new ExactMathBudget(maxPrimitiveSteps: 0))));
                r.Resize(40); yield return r.Ready(); r.Controller.SelectMember("W"); var head = r.Battle.Head; var calls = r.Battle.Runtime.Storage.Base.Calls;
                r.Draw(); Assert.IsNotNull(r.Controller.LastPreparation); Assert.AreEqual("Limit", r.Controller.LastPreparation.Code);
                Assert.IsNull(r.Controller.LastRequest); Assert.AreSame(head, r.Battle.Head); Assert.AreEqual(calls, r.Battle.Runtime.Storage.Base.Calls);
            }
        }
    }
}
