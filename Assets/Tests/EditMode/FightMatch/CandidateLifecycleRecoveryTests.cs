using System.IO;
using System.Linq;
using FightMatch.Application;
using NUnit.Framework;
using static FightMatch.Core.Tests.BusinessSaveScenario;
using static FightMatch.Core.Tests.LocalSaveTestFiles;
using static FightMatch.Core.Tests.LifecycleRig;

namespace FightMatch.Core.Tests
{
    public sealed class CandidateLifecycleRecoveryTests
    {
        [TestCase(99, 0, "clock:other", CandidateGrowthOutcome.IgnoredTimeRegression)]
        [TestCase(110, 10, "clock:024", CandidateGrowthOutcome.Applied)]
        [TestCase(180100, 180000, "clock:024", CandidateGrowthOutcome.Applied)]
        [TestCase(110, 10, "clock:other", CandidateGrowthOutcome.Applied)]
        [TestCase(100, 0, "clock:024", CandidateGrowthOutcome.Unchanged)]
        public void NaturalRecoveryUsesCoreTimeRulesAndPersistsOriginalResult(int wall, int mono, string scope, CandidateGrowthOutcome outcome)
        {
            using (var r = new LifecycleRig(level: 3, hp: 1))
            {
                r.DownAndExit(); var before = r.Head.Business.Character; var time = Time(wall, mono, scope);
                var expected = CandidateRecoveryClock.Advance(before, before.ActiveRecovery.RecoveryId, time, before.StateRevision, Math());
                Assert.IsTrue(expected.IsAccepted); Assert.AreEqual(outcome, expected.Outcome);
                var request = r.Freeze(r.RecoveryDraft(time)); var originalTime = request.Intent.CanonicalBytes.ToArray();
                time.WallUtcMilliseconds = 999999; time.MonotonicScopeId = "mutated-after-prepare";
                Is(r.System.Submit(request, B()), "Completed");
                var receipt = r.Head.Records.Last().Result.Recovery;
                Assert.AreEqual(outcome, receipt.Outcome); Assert.AreEqual(expected.Anomaly, receipt.ResultAnomaly);
                Same(expected.RecoveryPeriod.Elapsed, receipt.Period.Elapsed);
                Assert.AreEqual(expected.Next.IsReady, r.Head.Business.Character.IsReady);
                var head = r.Head; Is(r.System.Submit(request, B()), "Completed"); Assert.AreSame(head, r.Head);
                CollectionAssert.AreEqual(originalTime, request.Intent.CanonicalBytes);
                r.Restore(); Assert.AreEqual(outcome, r.Head.Records.Last().Result.Recovery.Outcome);
                Assert.AreEqual(before.RecoveryPeriods[0].StartSample.WallUtcMilliseconds, r.Head.Business.Character.RecoveryPeriods[0].StartSample.WallUtcMilliseconds);
            }
        }
        [Test]
        public void RecoveryRejectsStaleRevisionButCompletedPeriodKeepsCoreAlreadyIncludedRule()
        {
            using (var r = new LifecycleRig(level: 3, hp: 1))
            {
                r.DownAndExit(); var draft = r.RecoveryDraft(Time(110, 10)); draft.AdvanceRecovery.ExpectedCharacterRevision++;
                var before = r.Head; BuilderRefusal(r.System.Submit(r.Freeze(draft), B()), "StaleContext"); Assert.AreSame(before, r.Head);
                Is(r.System.Submit(r.Freeze(r.RecoveryDraft(Time(180100, 180000))), B()), "Completed");
                Assert.IsTrue(r.Head.Business.Character.IsReady);
                var again = r.RecoveryDraft(Time(100, 0)); again.AdvanceRecovery.ExpectedCharacterRevision = 1;
                Is(r.System.Submit(r.Freeze(again), B()), "Completed");
                Assert.AreEqual(CandidateGrowthOutcome.AlreadyIncluded, r.Head.Records.Last().Result.Recovery.Outcome);
            }
        }
        [TestCase("enter", "Snapshot.Flush.after", "SaveFailed")]
        [TestCase("enter", "Marker.Promote.after", "CommitUnknown")]
        [TestCase("victory", "Snapshot.Flush.after", "SaveFailed")]
        [TestCase("victory", "Marker.Promote.after", "CommitUnknown")]
        [TestCase("recovery", "Snapshot.Flush.after", "SaveFailed")]
        [TestCase("recovery", "Marker.Promote.after", "CommitUnknown")]
        public void OriginalCandidateSurvivesRealSaveFailureAndResolution(string kind, string point, string code)
        {
            using (var r = new LifecycleRig(level: kind == "recovery" ? 3 : 1, hp: kind == "recovery" ? 1 : 100))
            {
                PreparedCandidateLifecycleRequest request;
                if (kind == "victory") { r.Enter(); r.Win(); request = r.Victory(); }
                else if (kind == "recovery") { r.DownAndExit(); request = r.Freeze(r.RecoveryDraft(Time(200, 100))); }
                else request = r.Freeze(r.EntryDraft());
                var before = r.Head; r.Storage.PublishedCommit = null; r.Storage.Base.Arm(point, null);
                var fail = Is(r.System.Submit(request, B()), code); Assert.IsTrue(r.Storage.Base.FaultUsed);
                Assert.AreSame(before, r.Head); Assert.IsFalse(fail.View.IsPublishedHeadVerified);
                Assert.AreEqual(request.OperationId, fail.View.PendingOperationId);
                var commit = fail.View.PendingCommitId; var disk = r.Disk();
                var original = disk[code == "SaveFailed" ? "w-" + commit + ".snapshot.tmp" : SnapshotName(commit)];
                var calls = r.Storage.Base.Calls;
                Is(r.System.Submit(request, B()), code); Is(r.System.QueryOperation(request, B()), code);
                Assert.AreEqual(calls, r.Storage.Base.Calls);
                var another = r.Freeze(r.EntryDraft()); Assert.IsFalse(r.System.Submit(another, B()).IsCommitted);
                var completed = code == "SaveFailed" ? r.System.Retry(request, B()) : r.System.Resolve(request, B());
                Is(completed, "Completed"); Assert.AreEqual(commit, completed.OriginalCommitId);
                CollectionAssert.AreEqual(original, File.ReadAllBytes(Path.Combine(r.Storage.Profile.DirectoryPath, SnapshotName(commit))));
                Assert.AreEqual(1, r.Storage.Base.RealMarkerPromotionsFor(commit)); Assert.AreEqual(before.Records.Count + 1, r.Head.Records.Count);
                var head = r.Head; Is(r.System.Submit(request, B()), "Completed"); Assert.AreSame(head, r.Head); r.Restore();
                if (kind == "victory") Assert.AreEqual(1, r.Head.Business.Rewards.BaseRewards.Count);
                if (kind == "recovery") Same(R(100), r.Head.Business.Character.RecoveryPeriods[0].Elapsed);
                if (kind == "enter") Assert.AreEqual("local:System.Security.Cryptography.RandomNumberGenerator", r.Head.Business.ActiveHistory.Binding.SourceCapabilityId);
            }
        }
        [Test]
        public void QueryDoesNotAdvanceRecoveryOrResampleItsStart()
        {
            using (var r = new LifecycleRig(level: 3, hp: 1))
            {
                r.DownAndExit(); var before = r.Head; var disk = r.Disk(); var calls = r.Storage.Base.Calls;
                for (var i = 0; i < 10; i++) { Assert.AreSame(before.Business.Character.ActiveRecovery, r.System.QueryView().Recovery); }
                Assert.AreSame(before, r.Head); Assert.AreEqual(calls, r.Storage.Base.Calls); r.SameDisk(disk);
                Assert.IsTrue(r.System.QueryView().AdvanceRecovery.IsAvailable);
            }
        }
        [TestCase("null")] [TestCase("zero")] [TestCase("same")] [TestCase("changed")]
        public void C1_ExplicitTimeSamplesKeepCoreRulesAndFreezeCallerMutation(string kind)
        {
            using (var r = new LifecycleRig(level: 3, hp: 1))
            {
                r.DownAndExit(); var before = r.Head.Business.Character;
                var time = kind == "null" ? CandidateLifecyclePreparationTests.F01Sample(3) :
                    Time(110, kind == "zero" ? 0 : 10, kind == "changed" ? "clock:other" : "clock:024");
                var expected = CandidateRecoveryClock.Advance(before, before.ActiveRecovery.RecoveryId, time, before.StateRevision, Math());
                Assert.IsTrue(expected.IsAccepted);
                var request = r.Freeze(r.RecoveryDraft(time)); var bytes = request.Intent.CanonicalBytes.ToArray();
                time.WallUtcMilliseconds = 999999; time.MonotonicElapsedMilliseconds = R(999999);
                time.MonotonicScopeId = "mutated"; time.Trust = CandidateTimeTrust.ServerTrusted;
                Is(r.System.Submit(request, B()), "Completed");
                var actual = r.Head.Records.Last().Result.Recovery;
                Assert.AreEqual(expected.Outcome, actual.Outcome); Assert.AreEqual(expected.Anomaly, actual.ResultAnomaly);
                Same(expected.RecoveryPeriod.Elapsed, actual.Period.Elapsed);
                Assert.AreEqual(expected.RecoveryPeriod.LastAcceptedSample.MonotonicScopeId, actual.Period.LastAcceptedSample.MonotonicScopeId);
                Assert.AreEqual(expected.RecoveryPeriod.LastAcceptedSample.WallUtcMilliseconds, actual.Period.LastAcceptedSample.WallUtcMilliseconds);
                CollectionAssert.AreEqual(bytes, request.Intent.CanonicalBytes);
            }
        }
        [TestCase("Snapshot.Flush.after", "SaveFailed")]
        [TestCase("Marker.Promote.after", "CommitUnknown")]
        public void C1_ExplicitNullTimeKeepsFrozenIdentityAcrossRetryOrResolve(string point, string code)
        {
            using (var r = new LifecycleRig(level: 3, hp: 1))
            {
                r.DownAndExit(); var time = CandidateLifecyclePreparationTests.F01Sample(3);
                var request = r.Freeze(r.RecoveryDraft(time)); var id = request.OperationId; var bytes = request.Intent.CanonicalBytes.ToArray();
                time.WallUtcMilliseconds = 999999; time.MonotonicElapsedMilliseconds = R(999999); time.MonotonicScopeId = "mutated";
                var before = r.Head; r.Storage.PublishedCommit = null; r.Storage.Base.Arm(point, null);
                var failure = Is(r.System.Submit(request, B()), code); Assert.IsTrue(r.Storage.Base.FaultUsed);
                Assert.AreSame(before, r.Head); Assert.AreEqual(id, failure.View.PendingOperationId);
                var commit = failure.View.PendingCommitId;
                var result = code == "SaveFailed" ? r.System.Retry(request, B()) : r.System.Resolve(request, B());
                Is(result, "Completed"); Assert.AreEqual(commit, result.OriginalCommitId); Assert.AreEqual(id, request.OperationId);
                CollectionAssert.AreEqual(bytes, request.Intent.CanonicalBytes);
                var period = r.Head.Business.Character.ActiveRecovery;
                Same(R(10), period.Elapsed); Assert.IsNull(period.LastAcceptedSample.MonotonicElapsedMilliseconds);
                Assert.IsNull(period.LastAcceptedSample.MonotonicScopeId); Assert.AreEqual(new System.Numerics.BigInteger(110), period.LastAcceptedSample.WallUtcMilliseconds);
                Assert.AreEqual(1, r.Storage.Base.RealMarkerPromotionsFor(commit)); r.Restore();
                Same(R(10), r.Head.Business.Character.ActiveRecovery.Elapsed);
            }
        }
    }
}
