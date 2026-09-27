using System;
using System.Collections;
using System.Linq;
using FightMatch.Application;
using FightMatch.Core;
using FightMatch.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
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
                StringAssert.Contains("pair0 HP 15/15", r.Label("playback-hp")); Assert.AreEqual(-1, controller.Frame.OriginalFactIndex);
                Assert.IsEmpty(controller.Frame.LockedRoutes); CollectionAssert.AreEqual(r.Route(), controller.Frame.TemporaryRoute);
                r.Host.Advance(180); r.Board.panel.Pick(Vector2.zero);
                StringAssert.Contains("pair0 HP 0/15", r.Label("playback-hp")); StringAssert.Contains("命中", r.Label("playback-beat"));
                Assert.AreSame(result.Presentation.OrderedFacts[0], controller.Frame.OriginalFact);
                while (controller.IsPlaying && controller.Frame.OriginalFact?.Stage?.Kind != CandidateStageFactKind.RouteLocked) r.Host.Advance(180);
                Assert.IsTrue(controller.IsPlaying); Assert.AreEqual(1, r.Board.PlaybackOverride.LockedRoutes.Count); Assert.IsEmpty(r.Board.PlaybackOverride.TemporaryRoute);
                StringAssert.Contains("RouteLocked", r.Label("playback-stage")); r.Finish();
                StringAssert.Contains("W HP 95/100", r.Label("playback-hp")); Assert.IsNull(r.Board.PlaybackOverride); Assert.IsTrue(r.Input.View.Attack.IsAvailable);
                var second = r.Attack(1); Assert.AreEqual(2, controller.Starts); Assert.IsTrue(controller.IsPlaying); r.Finish();
                Assert.AreEqual(BattlePhase.WonPendingSettlement, controller.Frame.Phase); Assert.AreSame(second.Presentation.Token, controller.LastReportedToken);
                Assert.AreEqual(0, r.Battle.Head.Business.Rewards.BaseRewards.Count); Assert.IsFalse(r.Input.View.Attack.IsAvailable);
                TestContext.Out.WriteLine("P27 panel complete chain: " + r.Label("playback-hp") + " | " + r.Label("playback-stage"));
            }
        }
        [UnityTest]
        public IEnumerator P27_05_RealUIToolkitScheduleAdvancesActualFacts()
        {
            using (var r = new PlaybackPanelRig())
            {
                yield return r.Ready(); r.Attack(); var before = r.Playback.Frame.OriginalFactIndex;
                var until = Time.realtimeSinceStartupAsDouble + 3;
                while (r.Playback.IsPlaying && r.Playback.Frame.OriginalFactIndex == before && Time.realtimeSinceStartupAsDouble < until)
                { r.Board.panel.Pick(Vector2.zero); yield return null; }
                Assert.IsTrue(!r.Playback.IsPlaying || r.Playback.Frame.OriginalFactIndex > before, "Actual UI Toolkit scheduled callback must progress");
                r.Finish(); Assert.IsNull(r.Board.PlaybackOverride);
            }
        }
        [UnityTest]
        public IEnumerator P27_03_SkipCloseDetachBlurLowMemoryAndSchedulingFailureReleaseOnlyOwnResources()
        {
            foreach (var mode in new[] { "skip", "close", "detach", "blur", "low-memory-equivalent", "schedule-error-equivalent" })
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
                else if (mode == "detach") r.Host.RemoveFromHierarchy();
                else if (mode == "blur") r.Peer.Focus();
                else if (mode == "low-memory-equivalent") r.Host.NotifyLowMemory();
                else r.Host.ReportSchedulingFailure(new InvalidOperationException("observed cancellation test"));
                controller.Changed -= observe; Assert.IsTrue(clearedBeforeReport, mode + " clears override and reads latest before reporting its token");
                Assert.IsFalse(controller.IsPlaying, mode); Assert.AreSame(first.Presentation.Token, controller.LastReportedToken);
                Assert.AreEqual(1, controller.CompletionReports); Assert.IsNull(r.Board.PlaybackOverride); Assert.IsNull(r.Battle.System.QueryView().PresentationToken);
                Assert.AreSame(head, r.Battle.Head); r.Battle.Runtime.SameDisk(disk);
                if (mode == "schedule-error-equivalent") StringAssert.Contains("InvalidOperationException: observed cancellation test", r.Label("playback-diagnostic"));
                var second = r.Battle.Attack(1, false); var token = second.Presentation.Token;
                r.Host.Close(); controller.Advance(100000); controller.Dispose();
                Assert.AreSame(token, r.Battle.System.QueryView().PresentationToken, mode + " old callback must not clear A2");
                yield return null; Assert.AreSame(token, r.Battle.System.QueryView().PresentationToken); TestContext.Out.WriteLine("P27 interruption " + mode + " latest A2 remains pending");
            }
        }
        [UnityTest]
        public IEnumerator P27_03_ReattachUnsubscribesOldControllerAndDoesNotReplayPriorFacts()
        {
            using (var r = new PlaybackPanelRig())
            {
                yield return r.Ready(); r.Attack(); var old = r.Playback; var oldInput = r.Input;
                r.Host.Attach(r.Battle.System, B()); r.Resize(); yield return r.Ready();
                var current = r.Playback; Assert.AreNotSame(old, current); Assert.AreEqual(0, current.Starts); Assert.IsFalse(current.IsPlaying);
                Assert.IsFalse(old.IsPlaying); oldInput.QueryLastOperation(); Assert.AreEqual(0, current.Starts);
                r.Attack(1); Assert.AreEqual(1, current.Starts); Assert.AreEqual(1, old.Starts); Assert.AreEqual(1, old.CompletionReports);
                oldInput.Refresh(); old.Advance(100000); old.Dispose(); Assert.IsTrue(current.IsPlaying); Assert.IsNotNull(r.Input.View.PresentationToken);
                r.Finish(); r.Host.Close(); r.Host.Close(); Assert.AreEqual(1, current.CompletionReports);
            }
        }
    }
}
