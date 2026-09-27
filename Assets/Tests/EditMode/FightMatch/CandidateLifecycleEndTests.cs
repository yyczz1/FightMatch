using System.Linq;
using FightMatch.Application;
using NUnit.Framework;
using QFramework;
using static FightMatch.Core.Tests.BusinessSaveScenario;
using static FightMatch.Core.Tests.LocalSaveTestFiles;
using static FightMatch.Core.Tests.LifecycleRig;

namespace FightMatch.Core.Tests
{
    public sealed class CandidateLifecycleEndTests
    {
        [Test]
        public void StoredS17SettlesOnceWithoutWaitingForPresentationAndNextEntryUsesNewGrowth()
        {
            using (var r = new LifecycleRig(experience: 59))
            {
                r.Enter(); r.Win(false); var report = r.Head.Business.ActiveHistory.CurrentRun.FinalReport;
                Assert.IsNotNull(r.Battle.QueryView().PresentationToken); Assert.IsTrue(r.System.QueryView().SettleVictory.IsAvailable);
                var reserved = r.Head.Continuation.ReservedOperationId; var request = r.Victory();
                Assert.AreEqual(reserved, request.OperationId); Assert.AreEqual(reserved, r.Victory().OperationId);
                var completed = Is(r.System.Submit(request, B()), "Completed");
                Assert.IsNull(r.Head.Business.ActiveHistory); Assert.IsNull(r.Head.Continuation); Assert.IsNull(r.Battle.QueryView().PresentationToken);
                Assert.IsTrue(r.System.QueryView().HasEndedAttemptWithoutActiveHistory);
                Assert.AreEqual(1, r.Head.Business.Rewards.BaseRewards.Count); Assert.AreEqual(1, r.Head.Business.Progression.FirstClears.Count);
                Assert.AreEqual(new System.Numerics.BigInteger(2), r.Head.Business.Inventory.Holdings.Single(x => x.ItemId == "candidate:tin").T);
                Assert.AreEqual(new System.Numerics.BigInteger(2), r.Head.Business.Character.Level);
                var endedHead = r.Head; Is(r.System.Retry(request, B()), "Completed"); Is(r.System.Resolve(request, B()), "Completed");
                Assert.AreSame(endedHead, r.Head); r.Restore();
                r.Enter(); var fresh = r.Head;
                Assert.AreEqual(new System.Numerics.BigInteger(2), r.State.Baseline.Entry.Members[0].Level); Same(R(2, 1000), r.State.Baseline.Entry.Members[0].Crit.C);
                Same(CandidateCharacterGrowth.ComputeBaseStats(r.Head.Business.Character, Math()).Stats, r.State.Baseline.Entry.Members[0].Stats);
                Assert.AreEqual(System.Numerics.BigInteger.One, report.Baseline.Entry.Members[0].Level);
                Assert.AreEqual(report.Fingerprint, r.Head.Business.Rewards.BaseRewards[0].FinalReportFingerprint);
                Assert.AreEqual(completed.OriginalCommitId, Is(r.System.Submit(request, B()), "Completed").OriginalCommitId);
                Assert.AreSame(fresh, r.Head); var root = r.Root; var storage = r.Storage; var caps = r.Caps;
                r.Close(); var arch = FightMatchDemoArchitecture.Interface;
                try
                {
                    var app = arch.GetSystem<CandidateApplicationSystem>(); var lifecycle = arch.GetSystem<CandidateLifecycleApplicationSystem>();
                    Is(app.Open(storage, request.PlayerId, FightMatch.Platform.SaveOpenMode.Existing, caps, B()), "Ready");
                    var before = app.QueryView().View.PublishedSnapshot;
                    Is(lifecycle.Submit(request, B()), "Completed"); Assert.AreSame(before, app.QueryView().View.PublishedSnapshot);
                }
                finally { arch.Deinit(); }
            }
        }
        [TestCase(false)] [TestCase(true)]
        public void VictoryPendingCannotBeRewrittenAsExitOrRestart(bool restart)
        {
            using (var r = new LifecycleRig())
            {
                r.Enter(); r.Win(); var request = r.Freeze(r.EndDraft(restart)); var before = r.Head;
                var result = r.System.Submit(request, B()); Assert.IsFalse(result.IsCommitted); Assert.AreSame(before, r.Head);
                Assert.AreEqual(0, r.Head.Business.Rewards.BaseRewards.Count);
            }
        }
        [Test]
        public void ExitWithoutRewardPreservesChallengeAndActualEndAcrossRestore()
        {
            using (var r = new LifecycleRig())
            {
                r.Enter(); r.Attack(); var challenge = r.State.Baseline.Entry.ChallengeId;
                var request = r.Freeze(r.EndDraft()); Is(r.System.Submit(request, B()), "Completed"); r.Restore();
                Assert.IsNull(r.Head.Business.ActiveHistory); Assert.IsNull(r.Head.Business.Character.ActiveRecovery);
                Assert.AreEqual(0, r.Head.Business.Rewards.BaseRewards.Count); Assert.AreEqual(0, r.Head.Business.Progression.FirstClears.Count);
                Assert.AreEqual(CandidateCharacterEndKind.NormalExit, r.Head.Business.Character.ProcessedEnds.Single().Kind);
                Assert.IsTrue(r.Head.Business.Inventory.Holdings.All(x => x.T.IsZero));
                var old = r.Head; Is(r.System.Submit(request, B()), "Completed"); Assert.AreSame(old, r.Head);
                r.Enter(); Assert.AreEqual(challenge, r.State.Baseline.Entry.ChallengeId);
            }
        }
        [Test]
        public void ImmediateRestartUsesOriginalEntryAndAllThreeInitialRandomDomains()
        {
            using (var r = new LifecycleRig(level: 3))
            {
                r.Enter(); var original = r.Head.Business.ActiveHistory.CurrentRun; r.Attack();
                Assert.Less(r.State.Members[0].Hp.Compare(original.InitialSnapshot.Members[0].Hp, Math()), 0);
                Assert.AreNotEqual(original.InitialSnapshot.Random.Stream.WordsConsumed, r.State.Random.Stream.WordsConsumed);
                var oldCharacter = r.Head.Business.Character; var request = r.Freeze(r.EndDraft(true));
                Is(r.System.Submit(request, B()), "Completed"); var next = r.Head.Business.ActiveHistory.CurrentRun;
                Assert.AreNotEqual(original.Baseline.Entry.AttemptId, next.Baseline.Entry.AttemptId);
                Assert.AreEqual(original.Baseline.Entry.ChallengeId, next.Baseline.Entry.ChallengeId);
                Assert.AreEqual(original.Baseline.Entry.EntryBaselineId, next.Baseline.Entry.EntryBaselineId);
                Same(original.InitialSnapshot.Members[0].Hp, next.CurrentSnapshot.Members[0].Hp);
                Same(original.InitialSnapshot.Members[0].Member.Stats, next.CurrentSnapshot.Members[0].Member.Stats);
                Assert.AreEqual(original.Binding.SourceCapabilityId, next.Binding.SourceCapabilityId);
                foreach (var pair in new[] { new[] { original.Binding.Battle, next.Binding.Battle }, new[] { original.Binding.BaseReward, next.Binding.BaseReward }, new[] { original.Binding.Bonus, next.Binding.Bonus } })
                { Assert.AreEqual(pair[0].InitState, pair[1].InitState); Assert.AreEqual(pair[0].InitSequence, pair[1].InitSequence); Same(pair[0].Initial, pair[1].Initial); }
                Assert.IsTrue(next.CurrentSnapshot.Random.PrdStates.All(x => x.FailureCount.IsZero));
                Assert.AreEqual(0, next.Records.Count); Assert.AreEqual(2, r.Head.Business.Progression.Challenges.Single().Attempts.Count);
                Assert.AreEqual(oldCharacter.Level, r.Head.Business.Character.Level); Assert.AreEqual(oldCharacter.Experience, r.Head.Business.Character.Experience);
                Assert.IsNull(r.Head.Business.Character.ActiveRecovery); Assert.AreEqual(0, r.Head.Business.Rewards.BaseRewards.Count);
                r.Restore(); Assert.AreEqual(next.Baseline.Entry.AttemptId, r.State.Baseline.Entry.AttemptId);
            }
        }
        [TestCase(false)] [TestCase(true)]
        public void EndRequestsRespectPlaybackAndNeverQueue(bool restart)
        {
            using (var r = new LifecycleRig())
            {
                r.Enter(); var action = r.Attack(finish: false); var request = r.Freeze(r.EndDraft(restart)); var before = r.Head;
                BuilderRefusal(r.System.Submit(request, B()), "Busy"); Assert.AreSame(before, r.Head);
                r.Battle.ReportPresentationCompleted(action.Presentation.Token); Assert.AreSame(before, r.Head);
                Is(r.System.Submit(request, B()), "Completed");
            }
        }
        [Test]
        public void RewardEvaluationUsesTheSuppliedSaveMathAndTightenedScopeLimits()
        {
            using (var r = new LifecycleRig())
            {
                r.Enter(); r.Win(); var request = r.Victory(); var before = r.Head;
                var result = Is(r.System.Submit(request, B(), maxLiveIntegerBits: 0), "Limit");
                Assert.AreEqual("LiveIntegerBits", result.Diagnostic.LimitReason); Assert.AreSame(before, r.Head);
                Is(r.System.Submit(request, B()), "Completed");
            }
        }
        [TestCase(false, 0, "TimeSample.MonotonicElapsedMilliseconds")]
        [TestCase(false, 1, "TimeSample.MonotonicScopeId")]
        [TestCase(false, 2, "TimeSample.MonotonicElapsedMilliseconds")]
        [TestCase(true, 0, "TimeSample.MonotonicElapsedMilliseconds")]
        [TestCase(true, 1, "TimeSample.MonotonicScopeId")]
        [TestCase(true, 2, "TimeSample.MonotonicElapsedMilliseconds")]
        public void F01_MissingEndFieldsRejectBeforePublishingExitOrVictory(bool victory, int provided, string field)
        {
            using (var r = new LifecycleRig())
            {
                r.Enter(); if (victory) r.Win();
                var before = r.Head; var disk = r.Disk(); var calls = r.Storage.Base.Calls;
                var time = CandidateLifecyclePreparationTests.F01Sample(provided);
                var result = victory ? r.System.PrepareVictory(r.Content, time, B().Codec) : r.System.Prepare(r.EndDraft(time: time), B().Codec);
                Assert.AreEqual("MissingField", result.Code); Assert.AreEqual(field, result.Diagnostic.FieldPath);
                Assert.IsFalse(result.IsAccepted); Assert.IsNull(result.Request);
                Assert.AreSame(before, r.Head); Assert.AreEqual(calls, r.Storage.Base.Calls); r.SameDisk(disk);
                Assert.IsNull(r.Head.Business.Character.ActiveRecovery); Assert.AreEqual(0, r.Head.Business.Rewards.BaseRewards.Count);
            }
        }
    }
}
