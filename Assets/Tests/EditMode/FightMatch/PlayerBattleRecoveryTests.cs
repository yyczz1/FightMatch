using System;
using System.Linq;
using System.Threading.Tasks;
using FightMatch.Application;
using NUnit.Framework;
using static FightMatch.Core.Tests.PlayerSessionTestData;

namespace FightMatch.Core.Tests
{
    public sealed class PlayerBattleRecoveryTests
    {
        [TestCase("snapshot-before", "SaveFailed")] [TestCase("snapshot-promoted", "SaveFailed")]
        [TestCase("marker-before", "CommitUnknown")] [TestCase("marker-after", "CommitUnknown")]
        public void B06_LifecycleFaultRetriesOnlyTheRetainedOriginalCandidate(string fault, string code)
        {
            using (var r = new PlayerBattleRig())
            {
                var before = r.Head; r.N.Storage.Fault = fault; var failed = r.End();
                Assert.AreEqual(code, failed.Result.Code); var request = r.Session.OriginalRequest; var bytes = request.Intent.CanonicalBytes.ToArray();
                var candidate = failed.Read.Application.PendingCommitId; var candidateBytes = PlayerRosterSessionTests.CandidateBytes(r.N.Storage.Inner, candidate);
                var clocks = r.ClockReads; var files = r.N.Files(); var creates = r.N.Storage.Inner.SnapshotCreates;
                r.RebuildView(); Assert.AreSame(request, r.Session.OriginalRequest); CollectionAssert.AreEqual(bytes, r.View.OriginalIntent.CanonicalBytes);
                Assert.AreEqual(candidate, r.View.Read.Application.PendingCommitId); r.Unchanged(before, files, clocks);
                var done = r.Continue(PlayerBattleRecoveryAction.Resolve);
                if (!done.Result.IsCommitted) done = r.Continue(PlayerBattleRecoveryAction.Retry);
                r.Receipt(done); Assert.AreSame(request, r.Session.OriginalRequest); Assert.AreEqual(candidate, done.Receipt.CommitId);
                Assert.AreEqual(before.Records.Count + 1, r.Head.Records.Count); Assert.AreEqual(clocks, r.ClockReads);
                CollectionAssert.AreEqual(bytes, done.Result.OriginalLookup.Record.Intent.CanonicalBytes);
                if (candidateBytes != null) CollectionAssert.AreEqual(candidateBytes, r.N.Storage.Inner.Files["c-" + candidate + ".snapshot"]);
                Assert.LessOrEqual(r.N.Storage.Inner.SnapshotCreates, creates + (fault == "snapshot-before" ? 1 : 0));
                files = r.N.Files(); var head = r.Head; r.Continue(PlayerBattleRecoveryAction.Retry); r.Continue(PlayerBattleRecoveryAction.Query);
                r.Unchanged(head, files, clocks);
            }
        }
        [Test] public void B06_PrepareFailureWithoutTicketStillKeepsAnExplicitRetry()
        {
            using (var r = new PlayerBattleRig())
            {
                r.N.Storage.FailPrepare = true; var failed = r.End(); Assert.AreEqual(CandidateApplicationPhase.PendingPreparation, failed.Read.Application.Phase);
                Assert.IsNull(failed.Read.Application.PendingCommitId); Assert.IsTrue(failed.CanRetry);
                var operation = failed.OriginalIntent.OperationId; var clocks = r.ClockReads;
                var done = r.Continue(PlayerBattleRecoveryAction.Retry); r.Receipt(done);
                Assert.AreEqual(operation, done.Receipt.OperationId); Assert.AreEqual(clocks, r.ClockReads);
            }
        }
        private static Tuple<string, string, byte[], byte[]> PersistExit(PlayerBattleRig r)
        {
            r.N.Storage.Fault = "snapshot-promoted"; var failed = r.End();
            Assert.AreEqual("SaveFailed", failed.Result.Code); var commit = failed.Read.Application.PendingCommitId;
            var decoded = TakeCore(CandidateApplicationSaveCodec.DecodePublished(Envelope(r.N.Storage.Inner, commit), Closure(r.N.Publication), Codec()));
            return Tuple.Create(commit, decoded.Records.Last().OperationId, decoded.Records.Last().Intent.CanonicalBytes.ToArray(),
                PlayerRosterSessionTests.CandidateBytes(r.N.Storage.Inner, commit));
        }
        [TestCase("save", PlayerBattleRecoveryAction.Retry)] [TestCase("unknown-before", PlayerBattleRecoveryAction.Resolve)]
        [TestCase("unknown-after", PlayerBattleRecoveryAction.Resolve)] [TestCase("save", PlayerBattleRecoveryAction.End)]
        public void B06_WholeApplicationRebuildReadsOriginalIntentFromObservedDiskCandidate(string fault, PlayerBattleRecoveryAction finish)
        {
            using (var r = new PlayerBattleRig())
            {
                var identity = PersistExit(r); var count = r.Head.Records.Count; var creates = r.N.Storage.Inner.SnapshotCreates; var clocks = r.ClockReads;
                var opened = r.RebuildObjects(); CollectionAssert.Contains(opened.View.ObservedCandidateCommitIds, identity.Item1);
                Assert.IsNull(r.Session.OriginalRequest); Assert.IsNull(r.View.OriginalIntent);
                if (fault == "save") r.N.Storage.FailMarkerWork = true; else r.N.Storage.Fault = fault == "unknown-before" ? "marker-before" : "marker-after";
                var resumed = r.Host.ResumeObserved(r.Page.Page, identity.Item1, r.View.Context);
                Assert.AreEqual(fault == "save" ? "SaveFailed" : "CommitUnknown", resumed.Result.Code);
                Assert.IsNull(r.Session.OriginalRequest); Assert.IsNotNull(resumed.OriginalIntent);
                Assert.AreEqual(identity.Item2, resumed.OriginalIntent.OperationId); CollectionAssert.AreEqual(identity.Item3, resumed.OriginalIntent.CanonicalBytes);
                var capture = r.N.Player.ReadBattleResumedIntent(identity.Item1, identity.Item2, out var actual);
                Assert.AreSame(resumed.OriginalIntent, actual, capture.Code);
                var done = r.Continue(finish);
                if (finish == PlayerBattleRecoveryAction.End)
                { Assert.AreEqual(count, r.Head.Records.Count); Assert.IsNull(done.Read.Application.PendingOperationId); Assert.IsNull(done.OriginalIntent); }
                else
                {
                    if (!done.Result.IsCommitted) done = r.Continue(PlayerBattleRecoveryAction.Retry);
                    r.Receipt(done); Assert.AreEqual(count + 1, r.Head.Records.Count); Assert.AreEqual(identity.Item1, done.Receipt.CommitId);
                    CollectionAssert.AreEqual(identity.Item3, done.Result.OriginalLookup.Record.Intent.CanonicalBytes);
                    CollectionAssert.AreEqual(identity.Item4, r.N.Storage.Inner.Files["c-" + identity.Item1 + ".snapshot"]);
                }
                Assert.AreEqual(creates, r.N.Storage.Inner.SnapshotCreates); Assert.AreEqual(clocks, r.ClockReads);
            }
        }
        [Test] public void B06_CommittedResponseLostRebuildUsesSavedRecordIntentAndExactLookup()
        {
            using (var r = new PlayerBattleRig())
            {
                r.N.Storage.Fault = "marker-after"; var failed = r.End(); Assert.AreEqual("CommitUnknown", failed.Result.Code);
                var op = failed.OriginalIntent.OperationId; var commit = failed.Read.Application.PendingCommitId;
                r.RebuildObjects(); Assert.IsNull(r.View.OriginalIntent); Assert.IsNull(r.Session.OriginalRequest);
                var head = r.Head; var files = r.N.Files(); var clocks = r.ClockReads;
                var done = r.Host.OpenOriginalResult(op, r.View.Context); r.Receipt(done);
                Assert.AreEqual(commit, done.Receipt.CommitId); Assert.AreSame(head.Records.Single(x => x.OperationId == op).Intent, done.OriginalIntent);
                r.Unchanged(head, files, clocks);
            }
        }
        [Test] public void B04_B06_PersistedWpsRebuildContinuesItsReservedSettlementExactlyOnce()
        {
            using (var r = new PlayerBattleRig())
            {
                r.Win(false); var reserved = r.Head.Continuation.ReservedOperationId; var attempt = r.State.Baseline.Entry.AttemptId;
                r.RebuildObjects(); Assert.IsNull(r.N.Battle.QueryView().PresentationToken); Assert.IsNull(r.View.OriginalIntent);
                Assert.AreEqual(reserved, r.Head.Continuation.ReservedOperationId); Assert.AreEqual(attempt, r.State.Baseline.Entry.AttemptId);
                var host = r.N.Go(PlayerNavigationTargetKind.SettlementRequired).HostRequest; var before = r.Head; var files = r.N.Files(); var clocks = r.ClockReads;
                Assert.AreEqual(PlayerBattleRoute.Battle, r.Host.AcceptHost(host).Route); r.Unchanged(before, files, clocks);
                var done = r.Settle(); r.Receipt(done); Assert.AreEqual(reserved, done.Receipt.OperationId);
                var head = r.Head; files = r.N.Files(); clocks = r.ClockReads; r.Settle(); r.Continue(PlayerBattleRecoveryAction.Query);
                r.Unchanged(head, files, clocks); Assert.AreEqual(1, head.Business.Rewards.BaseRewards.Count);
            }
        }
        [Test] public void B06_B07_RebuiltS17CandidateKeepsReservedIdAndCannotBeEndedOrNavigatedAway()
        {
            using (var r = new PlayerBattleRig())
            {
                r.Win(); var reserved = r.Head.Continuation.ReservedOperationId;
                r.N.Storage.Fault = "snapshot-promoted"; var failed = r.Settle(); var commit = failed.Read.Application.PendingCommitId;
                Assert.AreEqual(reserved, failed.OriginalIntent.OperationId); var bytes = failed.OriginalIntent.CanonicalBytes.ToArray();
                r.RebuildObjects(); Assert.IsNull(r.View.OriginalIntent); r.N.Storage.FailMarkerWork = true;
                var resumed = r.Host.ResumeObserved(r.Page.Page, commit, r.View.Context); Assert.AreEqual("SaveFailed", resumed.Result.Code);
                Assert.AreEqual(reserved, resumed.OriginalIntent.OperationId); CollectionAssert.AreEqual(bytes, resumed.OriginalIntent.CanonicalBytes);
                var head = r.Head; var files = r.N.Files(); var clocks = r.ClockReads;
                Assert.AreEqual("SettlementRequired", r.Continue(PlayerBattleRecoveryAction.End).Status);
                r.Host.ReturnTo(r.Page.Page, PlayerNavigationTargetKind.MapAdventure, r.View.Context);
                Assert.IsFalse(r.N.Player.PrepareRosterMigration(resumed.OriginalIntent.ExpectedCommitId, Codec()).IsAccepted); r.Unchanged(head, files, clocks);
                var done = r.Continue(PlayerBattleRecoveryAction.Retry); r.Receipt(done); Assert.AreEqual(reserved, done.Receipt.OperationId);
                Assert.AreEqual(1, r.Head.Business.Rewards.BaseRewards.Count); Assert.IsNull(r.Head.Continuation);
            }
        }
        [Test] public void B07_F2DelegatesOriginalCreationAndNeverStartsBattleOrASecondProfile()
        {
            using (var r = new PlayerBattleRig(false, false))
            {
                r.N.ProfileStorage.FailConfirmation = true;
                var created = r.N.Player.CreateNew(r.N.Profile, r.N.Storage, r.N.Locator, r.N.Capabilities, Budget());
                Assert.AreEqual(CandidateApplicationPhase.CreationConfirmationRequired, created.View.Phase);
                var head = r.Head; var files = r.N.Files(); var clocks = r.ClockReads;
                var host = r.N.Go(PlayerNavigationTargetKind.CreationRequired).HostRequest;
                Assert.AreEqual(PlayerBattleRoute.HostNavigation, r.Host.AcceptHost(host).Route);
                Assert.AreEqual("CreationRequired", r.Session.Settle(r.View.Context, r.Clock, Budget()).Status);
                Assert.AreEqual("CreationRequired", r.Session.ResumeObserved("foreign", r.View.Context, Budget()).Status);
                r.N.Player.ReadBattleResumedIntent("foreign", "foreign", out var intent); Assert.IsNull(intent);
                r.Unchanged(head, files, clocks); Assert.IsNull(r.Session.OriginalRequest);
            }
        }
        [Test] public void B07_IdentityWrongThreadBusyAndDisposedRejectBeforeClockOrStorage()
        {
            using (var r = new PlayerBattleRig()) using (var other = new PlayerBattleRig())
            {
                var head = r.Head; var files = r.N.Files(); var clocks = r.ClockReads; var context = r.View.Context;
                Assert.AreEqual("StaleContext", r.Session.PreviewEnd(CandidateApplicationKind.ExitAttempt, other.View.Context, Codec()).Status);
                Assert.AreEqual("UnsupportedBinding", r.Session.PreviewEnd(CandidateApplicationKind.Attack, context, Codec()).Status);
                Assert.AreEqual("StaleCandidate", r.Session.ResumeObserved("foreign", context, Budget()).Status);
                Assert.AreEqual("WrongThread", Task.Run(() => r.Session.PreviewEnd(CandidateApplicationKind.ExitAttempt, context, Codec()).Status).GetAwaiter().GetResult());
                r.Unchanged(head, files, clocks);
                string busy = null; var called = false;
                r.N.Storage.DuringWrite = () => { if (called) return; called = true; busy = r.Session.Settle(context, r.Clock, Budget()).Status; };
                Is(r.End().Result); Assert.IsTrue(called); Assert.AreEqual("Busy", busy); Assert.AreEqual(clocks + 1, r.ClockReads);
                r.N.Storage.DuringWrite = null; head = r.Head; files = r.N.Files(); clocks = r.ClockReads;
                r.N.Runtime.Close(); Assert.AreEqual("Disposed", r.Session.Settle(r.View.Context, r.Clock, Budget()).Status); r.Unchanged(head, files, clocks);
            }
        }
        [Test] public void B07_NarrowResumedBridgeRejectsWrongCommitOperationOwnerThreadAndBusy()
        {
            using (var r = new PlayerBattleRig())
            {
                var original = PersistExit(r); r.RebuildObjects(); r.N.Storage.FailMarkerWork = true;
                r.Host.ResumeObserved(r.Page.Page, original.Item1, r.View.Context); var files = r.N.Files();
                Assert.AreEqual("StaleContext", r.N.Player.ReadBattleResumedIntent("wrong", original.Item2, out var intent).Code); Assert.IsNull(intent);
                Assert.AreEqual("StaleContext", r.N.Player.ReadBattleResumedIntent(original.Item1, "wrong", out intent).Code); Assert.IsNull(intent);
                var other = new PlayerSessionSystem(r.N.Runtime, r.N.App, r.N.Life);
                Assert.AreEqual("InconsistentBinding", other.ReadBattleResumedIntent(original.Item1, original.Item2, out intent).Code); Assert.IsNull(intent);
                Assert.AreEqual("WrongThread", Task.Run(() => r.N.Player.ReadBattleResumedIntent(original.Item1, original.Item2, out var ignored).Code).GetAwaiter().GetResult());
                SameFiles(files, r.N.Storage.Inner.Files); string busy = null;
                r.N.Storage.DuringWrite = () => busy = r.N.Player.ReadBattleResumedIntent(original.Item1, original.Item2, out var ignored).Code;
                r.Receipt(r.Continue(PlayerBattleRecoveryAction.Retry)); Assert.AreEqual("Busy", busy); r.N.Storage.DuringWrite = null;
                r.N.Runtime.Close(); Assert.AreEqual("Disposed", r.N.Player.ReadBattleResumedIntent(original.Item1, original.Item2, out intent).Code); Assert.IsNull(intent);
            }
        }
        [Test] public void B08_NotificationFailureAndRepeatedQueriesNeverResubmitOrReplayRewards()
        {
            using (var r = new PlayerBattleRig())
            {
                r.Win(); r.N.OnPublish = e => throw new InvalidOperationException("028 isolated notification failure");
                var done = r.Settle(); r.Receipt(done); Assert.IsNotNull(done.Result.NotificationFailure);
                var head = r.Head; var files = r.N.Files(); var clocks = r.ClockReads; var starts = r.Host.Playback.Starts;
                var request = r.Session.OriginalRequest;
                r.Continue(PlayerBattleRecoveryAction.Query); r.Continue(PlayerBattleRecoveryAction.Retry);
                r.Host.OpenOriginalResult(done.Receipt.OperationId, r.View.Context);
                r.Unchanged(head, files, clocks); Assert.AreEqual(starts, r.Host.Playback.Starts);
                Assert.AreEqual(1, head.Business.Rewards.BaseRewards.Count); Assert.AreEqual(request.OperationId, r.View.Receipt.OperationId);
            }
        }
        [Test] public void B08_FullObjectRebuildReopensOriginalReceiptAndLaterGrowthDoesNotRewriteIt()
        {
            using (var r = new PlayerBattleRig())
            {
                r.Win(); var first = r.Settle().Receipt; var operation = first.OperationId; var amounts = first.Members.Select(x => x.Experience.Amount).ToArray();
                r.RebuildObjects(); Assert.IsNull(r.View.OriginalIntent); Assert.IsNull(r.View.Receipt);
                var head = r.Head; var files = r.N.Files(); var clocks = r.ClockReads;
                var original = r.Host.OpenOriginalResult(operation, r.View.Context); r.Receipt(original); r.Unchanged(head, files, clocks);
                r.Host.ReturnTo(r.Page.Page, PlayerNavigationTargetKind.MapAdventure, r.View.Context);
                r.Receipt(r.Host.OpenOriginalResult(operation, r.View.Context)); r.Unchanged(head, files, clocks);
                Is(r.Host.Replay(r.Page.Page, first.LevelId, first.LevelVersion, r.View.Context).Result); r.Win(); r.Receipt(r.Settle());
                Assert.AreEqual(2, r.Head.Business.Rewards.BaseRewards.Count); head = r.Head; files = r.N.Files(); clocks = r.ClockReads;
                var reopened = r.Host.OpenOriginalResult(operation, r.View.Context); r.Receipt(reopened);
                Assert.AreEqual(first.CommitId, reopened.Receipt.CommitId); Assert.AreEqual(first.AttemptId, reopened.Receipt.AttemptId);
                Assert.AreEqual(first.SettlementId, reopened.Receipt.SettlementId); CollectionAssert.AreEqual(amounts, reopened.Receipt.Members.Select(x => x.Experience.Amount));
                Assert.AreEqual(head.Header.CommitId, reopened.Result.LookupViewCommitId); Assert.AreSame(head, reopened.Read.Head); r.Unchanged(head, files, clocks);
            }
        }
        [Test] public void B08_OriginalResultRejectsWrongKindMissingForeignAndStaleContextWithoutWrites()
        {
            using (var r = new PlayerBattleRig()) using (var other = new PlayerBattleRig())
            {
                var old = r.View.Context; r.Win(); var receipt = r.Settle().Receipt; var head = r.Head; var files = r.N.Files(); var clocks = r.ClockReads;
                Assert.AreEqual("OriginalResultUnavailable", r.Session.OpenOriginalResult(head.Records[0].OperationId, r.View.Context, Budget()).Status);
                Assert.AreEqual("OriginalResultUnavailable", r.Session.OpenOriginalResult("missing", r.View.Context, Budget()).Status);
                Assert.AreEqual("StaleContext", r.Session.OpenOriginalResult(receipt.OperationId, other.View.Context, Budget()).Status);
                Assert.AreEqual("StaleContext", r.Session.OpenOriginalResult(receipt.OperationId, old, Budget()).Status); r.Unchanged(head, files, clocks);
            }
        }
    }
}
