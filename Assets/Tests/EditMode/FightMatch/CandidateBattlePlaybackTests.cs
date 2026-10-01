using System;
using System.Collections;
using System.Linq;
using FightMatch.Application;
using FightMatch.Core;
using FightMatch.Platform;
using FightMatch.Presentation;
using NUnit.Framework;
using QFramework;
using UnityEngine.TestTools;
using static FightMatch.Core.Tests.PlaybackPanelRig;
using static FightMatch.Core.Tests.LocalSaveTestFiles;

namespace FightMatch.Core.Tests
{
    public class CandidateBattlePlaybackTests
    {
        [TearDown] public void Cleanup() { CloseAll(); }

        [UnityTest]
        public IEnumerator P27_01_RealOriginalFactTimelineDoesNotChangeBusinessRandomOrDisk()
        {
            foreach (var level in new[] { 1, 3 }) using (var r = new PlaybackPanelRig(level))
            {
                yield return r.Ready(); var original = r.Attack(); var p = original.Presentation; var playback = r.Playback;
                var head = r.Battle.Head; var disk = r.Battle.Runtime.Disk(); var calls = r.Battle.Runtime.Storage.Base.Calls;
                var before = p.BeforeSnapshot; var after = p.AfterSnapshot; var facts = p.OrderedFacts;
                Assert.IsTrue(playback.IsPlaying); Assert.AreEqual(-1, playback.Frame.OriginalFactIndex);
                Assert.AreSame(before, playback.Original.BeforeSnapshot); Assert.AreSame(after, playback.Original.AfterSnapshot);
                Assert.AreSame(facts, playback.Original.OrderedFacts); Assert.AreEqual("PresentationPending", r.Input.View.Attack.Reason);
                Same(playback.Frame.Actors.Single(a => a.Key.Kind == BattleCombatantKind.Participant).Hp, before.Members[0].Hp);
                r.Host.Advance(0); r.Host.Advance(179); Assert.AreEqual(-1, playback.Frame.OriginalFactIndex);
                r.Host.Advance(1);
                for (var i = 0; i < facts.Count; i++)
                {
                    if (i > 0) r.Host.Advance(180);
                    var f = facts[i]; Assert.AreEqual(i, playback.Frame.OriginalFactIndex); Assert.AreSame(f, playback.Frame.OriginalFact);
                    Assert.AreSame(head, r.Battle.Head); Assert.AreSame(after.Random, r.Battle.State.Random); Assert.AreSame(before, p.BeforeSnapshot); Assert.AreSame(after, p.AfterSnapshot);
                    Assert.AreEqual(calls, r.Battle.Runtime.Storage.Base.Calls); r.Battle.Runtime.SameDisk(disk);
                    if (f.DirectAttack != null)
                    { Same(playback.Frame.Actors.Single(a => a.Key.Equals(f.DirectAttack.Target)).Hp, f.DirectAttack.HpAfter);
                        StringAssert.Contains("命中", playback.Frame.Beat); if (f.DirectAttack.Crit.Triggered) StringAssert.Contains("暴击", playback.Frame.Beat); }
                    if (f.EnemyIntent != null)
                    {
                        var enemy = playback.Frame.Actors.Single(a => a.Key.Equals(f.EnemyIntent.EnemyKey)); Assert.AreEqual(f.EnemyIntent.CursorAfter, enemy.IntentCursor);
                        if (f.EnemyIntent.IntentKind == EnemyIntentKind.Charge) StringAssert.Contains("蓄力", playback.Frame.Beat);
                        if (f.EnemyIntent.Damage != null) Same(playback.Frame.Actors.Single(a => a.Key.Equals(f.EnemyIntent.Damage.TargetMember)).Hp, f.EnemyIntent.Damage.HpAfter);
                    }
                    if (f.Stage?.Kind == CandidateStageFactKind.TemporaryRouteRemoved) Assert.IsEmpty(playback.Frame.TemporaryRoute);
                    if (f.Stage?.Kind == CandidateStageFactKind.RouteLocked)
                        Assert.AreSame(after.Board.LockedRoutes.Single(x => x.PairKey.Equals(f.Stage.Pair)), playback.Frame.LockedRoutes.Single(x => x.PairKey.Equals(f.Stage.Pair)));
                    TestContext.Out.WriteLine("P27 timeline L" + level + " index=" + i + " kind=" + f.Kind + " beat=" + playback.Frame.Beat + " stage=" + playback.Frame.StageFeedback);
                }
                Assert.IsTrue(playback.IsPlaying, "last fact remains visible for its own full beat");
                r.Host.Advance(179); Assert.IsTrue(playback.IsPlaying); r.Host.Advance(1);
                Assert.IsFalse(playback.IsPlaying); Assert.AreSame(p.Token, playback.LastReportedToken); Assert.AreEqual(1, playback.CompletionReports);
                Assert.IsNull(r.Battle.System.QueryView().PresentationToken); Assert.IsTrue(r.Input.View.Attack.IsAvailable); Assert.IsNull(r.Board.PlaybackOverride);
                r.Battle.Runtime.SameDisk(disk); Assert.AreEqual(calls, r.Battle.Runtime.Storage.Base.Calls); Assert.AreSame(head, r.Battle.Head);
            }
        }
        [UnityTest]
        public IEnumerator P27_01_RealSC01CriticalFactShowsCriticalFeedbackWithoutNewSampling()
        {
            using (var r = new PlaybackPanelRig())
            {
                yield return r.Ready(); r.EnterCriticalSeedWitness(); var result = r.Attack();
                var direct = result.Presentation.OrderedFacts.Single(f => f.DirectAttack != null).DirectAttack;
                Assert.IsTrue(direct.Crit.Triggered); Assert.IsNotEmpty(direct.Crit.Words);
                var random = r.Battle.State.Random; var head = r.Battle.Head; var disk = r.Battle.Runtime.Disk(); var calls = r.Battle.Runtime.Storage.Base.Calls;
                r.Host.Advance(180); StringAssert.Contains("暴击", r.Playback.Frame.Beat);
                BattleCopyAssert.Diagnostic(r.Host, "playback-beat");
                Same(direct.HpAfter, r.Playback.Frame.Actors.Single(a => a.Key.Equals(direct.Target)).Hp);
                r.Finish(); Assert.AreSame(random, r.Battle.State.Random); Assert.AreSame(head, r.Battle.Head);
                Assert.AreEqual(calls, r.Battle.Runtime.Storage.Base.Calls); r.Battle.Runtime.SameDisk(disk);
            }
        }
        [UnityTest]
        public IEnumerator P27_02_QueryDuplicateRebuildCompletesOnlyCurrentTokenWithoutReplay()
        {
            using (var r = new PlaybackPanelRig())
            {
                yield return r.Ready(); var first = r.Attack(); var head = r.Battle.Head; Assert.AreEqual(1, r.Playback.Starts);
                var query = r.Input.QueryLastOperation(); Assert.AreEqual(CandidatePresentationDisposition.RebuildLatest, query.PresentationDisposition);
                Assert.IsFalse(r.Playback.IsPlaying); Assert.AreEqual(1, r.Playback.CompletionReports); Assert.AreSame(first.Presentation.Token, r.Playback.LastReportedToken);
                r.Input.QueryLastOperation(); r.Input.RetryLast(); r.Input.ResolveLast(); r.Input.RebuildLatest();
                Assert.AreEqual(1, r.Playback.Starts); Assert.AreEqual(1, r.Playback.CompletionReports); Assert.AreSame(head, r.Battle.Head);
                Assert.IsNull(r.Input.View.PresentationToken); Assert.IsNull(r.Board.PlaybackOverride);
            }
        }
        [UnityTest]
        public IEnumerator P27_03_OldControllerAdvanceCloseAndOriginalQueryCannotReleaseA2()
        {
            using (var r = new PlaybackPanelRig())
            {
                yield return r.Ready(); var first = r.Attack(); var old = r.Playback; var oldInput = r.Input;
                r.Finish(); r.Host.Attach(r.Battle.System, B(), r.Canvas.Localization); r.Resize(); yield return r.Ready();
                var second = r.Attack(1); var token = second.Presentation.Token; var latest = r.Battle.Head;
                old.Advance(100000); old.SkipToFinal(); old.Dispose(); oldInput.QueryLastOperation();
                r.Battle.System.ReportPresentationCompleted(first.Presentation.Token);
                r.Input.Refresh(); Assert.AreSame(token, r.Battle.System.QueryView().PresentationToken); Assert.IsTrue(r.Playback.IsPlaying);
                Assert.AreSame(latest, r.Playback.LatestView.PublishedSnapshot); Assert.AreSame(token, r.Playback.Original.Token);
                r.Finish(); Assert.AreSame(token, r.Playback.LastReportedToken); Assert.AreEqual(1, r.Playback.CompletionReports);
            }
        }
        [UnityTest]
        public IEnumerator P27_03_StaleActiveA1DropKeepsNewA2LatchAndLatestState()
        {
            using (var r = new PlaybackPanelRig())
            {
                yield return r.Ready(); var first = r.Attack(); var a1 = first.Presentation.Token;
                r.Battle.System.ReportPresentationCompleted(a1); var second = r.Battle.Attack(1, false); var a2 = second.Presentation.Token;
                var latest = r.Battle.Head; r.Host.Advance(180);
                Assert.IsFalse(r.Playback.IsPlaying); Assert.AreSame(a1, r.Playback.LastReportedToken); Assert.AreSame(a2, r.Battle.System.QueryView().PresentationToken);
                Assert.AreSame(latest, r.Playback.LatestView.PublishedSnapshot); Assert.AreEqual(BattlePhase.WonPendingSettlement, r.Playback.Frame.Phase);
                r.Host.Close(); Assert.AreSame(a2, r.Battle.System.QueryView().PresentationToken); Assert.IsNull(r.Board.PlaybackOverride);
            }
        }
        [UnityTest]
        public IEnumerator P27_03_NewAttemptDiscardsOldDisplayAndDoesNotRestoreOldHP()
        {
            using (var r = new PlaybackPanelRig())
            {
                yield return r.Ready(); var first = r.Attack(); r.Host.Advance(180); r.Battle.ExitAndEnter(); var latest = r.Battle.Head;
                r.Input.Refresh(); Assert.IsFalse(r.Playback.IsPlaying); Assert.AreNotEqual(first.Presentation.Token.AttemptId, r.Battle.State.Baseline.Entry.AttemptId);
                Assert.AreSame(latest, r.Playback.LatestView.PublishedSnapshot); Hp(r.Playback.Frame.Actors.Single(a => a.Key.Kind == BattleCombatantKind.Participant).Hp, 100);
                Assert.IsEmpty(r.Playback.Frame.LockedRoutes); Assert.IsNull(r.Board.PlaybackOverride); Assert.IsTrue(r.Input.View.Attack.IsAvailable);
                var disk = r.Battle.Runtime.Disk(); r.Host.Advance(100000); r.Battle.Runtime.SameDisk(disk);
            }
        }
        [UnityTest]
        public IEnumerator P27_04_SaveFailedUnknownRealRetryResolveProduceOnePlayback()
        {
            foreach (var failure in new[] { "SaveFailed", "CommitUnknown" }) using (var r = new PlaybackPanelRig())
            {
                yield return r.Ready(); r.Input.SelectMember("W"); var oldHead = r.Battle.Head;
                r.Battle.Runtime.Storage.PublishedCommit = null;
                r.Battle.Runtime.Storage.Base.Arm(failure == "SaveFailed" ? "Snapshot.Flush.after" : "Marker.Promote.after", null);
                r.Draw(); Assert.AreEqual(failure, r.Input.LastResult.Code); Assert.IsTrue(r.Battle.Runtime.Storage.Base.FaultUsed);
                Assert.AreEqual(0, r.Playback.Starts); Assert.IsFalse(r.Playback.IsPlaying); Assert.IsNull(r.Board.PlaybackOverride);
                Assert.AreSame(oldHead, r.Playback.LatestView.PublishedSnapshot); BattleCopyAssert.Localized(r.Host, "playback-diagnostic", r.Canvas.Localization,
                    failure == "SaveFailed" ? "fm.save_recovery.failed" : "fm.save_recovery.unknown",
                    failure == "SaveFailed" ? new[] { BattleText.Arg("errorCode", failure) } : Array.Empty<System.Collections.Generic.KeyValuePair<string, string>>());
                var request = r.Input.LastRequest; var bytes = request.Intent.CanonicalBytes.ToArray();
                var result = failure == "SaveFailed" ? r.Input.RetryLast() : r.Input.ResolveLast(); Assert.AreEqual("Completed", result.Code);
                Assert.AreSame(request, r.Input.LastRequest); CollectionAssert.AreEqual(bytes, request.Intent.CanonicalBytes);
                Assert.AreEqual(1, r.Playback.Starts); Assert.IsTrue(r.Playback.IsPlaying);
                Assert.AreEqual(1, r.Battle.Runtime.Storage.Base.RealMarkerPromotionsFor(result.Application.OriginalCommitId));
                r.Finish(); var head = r.Battle.Head; r.Input.QueryLastOperation(); r.Input.RetryLast(); r.Input.ResolveLast();
                Assert.AreSame(head, r.Battle.Head); Assert.AreEqual(1, r.Playback.Starts); Assert.AreEqual(1, r.Playback.CompletionReports);
                TestContext.Out.WriteLine("P27 real file recovery " + failure + " original=" + request.OperationId + " plays=1");
            }
        }
        [UnityTest]
        public IEnumerator P27_04_CloseWhileUnknownPreservesRealSavePhaseAndRetiresNoForeignToken()
        {
            using (var r = new PlaybackPanelRig())
            {
                yield return r.Ready(); r.Input.SelectMember("W"); var old = r.Battle.Head;
                r.Battle.Runtime.Storage.PublishedCommit = null; r.Battle.Runtime.Storage.Base.Arm("Marker.Promote.after", null);
                r.Draw(); Assert.AreEqual("CommitUnknown", r.Input.LastResult.Code); r.Host.Close();
                Assert.AreEqual(CandidateApplicationPhase.CommitUnknown, r.Playback.LatestView.Phase); Assert.AreSame(old, r.Playback.LatestView.PublishedSnapshot);
                Assert.AreEqual(0, r.Playback.CompletionReports); BattleCopyAssert.Localized(r.Host, "playback-diagnostic", r.Canvas.Localization, "fm.save_recovery.unknown");
            }
        }
        [UnityTest]
        public IEnumerator P27_04_RollbackAndNewPageTakeoverOnlyRebuild()
        {
            using (var r = new PlaybackPanelRig())
            {
                yield return r.Ready(); r.Attack(); r.Finish(); var rollback = r.Battle.System.Submit(r.Battle.Freeze(r.Battle.RollbackDraft(r.Battle.Preview().Range)), B());
                Assert.AreEqual("Completed", rollback.Code); Assert.AreEqual(CandidatePresentationDisposition.RebuildLatest, rollback.PresentationDisposition);
                r.Input.Refresh(); Assert.AreEqual(1, r.Playback.Starts); Assert.IsFalse(r.Playback.IsPlaying); Assert.IsEmpty(r.Playback.Frame.LockedRoutes);
                Hp(r.Playback.Frame.Actors.Single(a => a.Key.Kind == BattleCombatantKind.Participant).Hp, 100);
                var pending = r.Battle.Attack(finish: false); r.Host.Attach(r.Battle.System, B(), r.Canvas.Localization); r.Resize(); yield return r.Ready();
                Assert.AreEqual(0, r.Playback.Starts); Assert.IsNull(r.Battle.System.QueryView().PresentationToken);
                Assert.AreSame(r.Battle.Head, r.Playback.LatestView.PublishedSnapshot); Assert.IsNull(r.Board.PlaybackOverride);
                Assert.AreEqual("PresentationIgnored", r.Battle.System.ReportPresentationCompleted(pending.Presentation.Token).Code);
            }
        }
        [UnityTest]
        public IEnumerator P27_02_WaitingCurrentTokenWithoutLocalProgressRebuildsAndAcknowledgesOnce()
        {
            using (var r = new BoardPanelRig())
            {
                yield return r.Ready(); r.Controller.SelectMember("W"); r.Draw();
                var original = r.Controller.LastResult.Presentation; Assert.NotNull(original);
                using (var playback = new CandidateBattlePlaybackController(r.Controller, r.Battle.System))
                {
                    Assert.IsFalse(playback.IsPlaying); Assert.AreSame(original.Token, playback.LatestView.PresentationToken);
                    r.Controller.QueryLastOperation(); Assert.AreEqual(0, playback.Starts); Assert.AreEqual(1, playback.CompletionReports);
                    Assert.AreSame(original.Token, playback.LastReportedToken); Assert.IsNull(r.Battle.System.QueryView().PresentationToken);
                    r.Controller.QueryLastOperation(); Assert.AreEqual(1, playback.CompletionReports);
                }
            }
        }
        [UnityTest]
        public IEnumerator P27_02_RealBusyPublicationCallbackDoesNotStartBeforeReadyOriginalDelivery()
        {
            using (var r = new PlaybackPanelRig())
            {
                yield return r.Ready(); CandidateBattleCallResult busy = null; var startsDuringCallback = -1;
                var handle = r.Battle.Runtime.Architecture.RegisterEvent<CandidateApplicationPublished>(e =>
                { busy = r.Input.RebuildLatest(); startsDuringCallback = r.Playback.Starts; });
                CandidateBattleCallResult result;
                try { result = r.Attack(); } finally { handle.UnRegister(); }
                Assert.AreEqual("Busy", busy.Code); Assert.AreEqual(CandidatePresentationDisposition.None, busy.PresentationDisposition);
                Assert.IsNull(busy.Presentation); Assert.AreEqual(0, startsDuringCallback);
                Assert.AreEqual(CandidatePresentationDisposition.PlayOriginal, result.PresentationDisposition);
                Assert.AreEqual(1, r.Playback.Starts); Assert.AreSame(result.Presentation, r.Playback.Original); r.Finish();
            }
        }
        [TestCase(false)]
        [TestCase(true)]
        public void P27_04_NewInstanceRestoreOrObservedResumeRebuildsWithoutPlaying(bool pending)
        {
            string root; PreparedCandidateBattleRequest request; CandidateBattleApplicationSystem old;
            using (var r = new BattleApplicationRig())
            {
                root = r.Runtime.Root; old = r.System; request = r.Freeze(r.AttackDraft());
                if (pending) r.Runtime.Storage.Base.Arm("Snapshot.Flush.after", null);
                BattleApplicationRig.Is(r.System.Submit(request, B()), pending ? "SaveFailed" : "Completed");
            }
            using (var r = new BattleApplicationRig(false, root: root))
            {
                Assert.AreNotSame(old, r.System);
                ApplicationRuntimeRig.Is(r.Runtime.Open(SaveOpenMode.Existing), pending ? "Pending" : "Ready");
                if (pending) ApplicationRuntimeRig.Is(r.Runtime.Resume(r.Runtime.Model.View.ObservedCandidateCommitIds.Single()), "Completed");
                var head = r.Head; var disk = r.Runtime.Disk(); var calls = r.Runtime.Storage.Base.Calls;
                using (var ui = new BattleUguiRoot())
                {
                    var host = ui.Playback(); host.Attach(r.System, B(), ui.Localization); var result = r.System.QueryOperation(request, B());
                    Assert.AreEqual(CandidatePresentationDisposition.RebuildLatest, result.PresentationDisposition); Assert.IsNull(result.Presentation);
                    host.InputView.Controller.Refresh(); host.Advance(100000);
                    Assert.AreEqual(0, host.Controller.Starts); Assert.AreEqual(0, host.Controller.CompletionReports); Assert.IsFalse(host.Controller.IsPlaying);
                    Assert.AreSame(head, host.Controller.LatestView.PublishedSnapshot); Assert.IsNull(host.InputView.Board.PlaybackOverride);
                    foreach (var member in r.State.Members) Same(member.Hp, host.Controller.Frame.Actors.Single(a => a.Key.Equals(member.CombatantKey)).Hp);
                }
                Assert.AreEqual(calls, r.Runtime.Storage.Base.Calls); r.Runtime.SameDisk(disk);
            }
        }
        [UnityTest]
        public IEnumerator P27_01_InvalidManualDeltaDoesNotAdvanceOrAcknowledge()
        {
            using (var r = new PlaybackPanelRig())
            {
                yield return r.Ready(); r.Attack(); var frame = r.Playback.Frame;
                foreach (var delta in new[] { -1d, double.NaN, double.PositiveInfinity, double.NegativeInfinity })
                    Assert.Throws<ArgumentOutOfRangeException>(() => r.Host.Advance(delta));
                Assert.AreSame(frame, r.Playback.Frame); Assert.AreEqual(0, r.Playback.CompletionReports); Assert.IsTrue(r.Playback.IsPlaying); r.Finish();
            }
        }
    }
}
