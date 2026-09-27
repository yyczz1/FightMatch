using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FightMatch.Application;
using FightMatch.Platform;
using NUnit.Framework;
using QFramework;
using static FightMatch.Core.Tests.LocalSaveTestFiles;
using static FightMatch.Core.Tests.ApplicationRuntimeRig;

namespace FightMatch.Core.Tests
{
    [TestFixture]
    public sealed class CandidateApplicationRecoveryTests
    {
        [TestCase("work")]
        [TestCase("snapshot")]
        [TestCase("pair")]
        public void A06_NewInstanceResumesOriginalSixSliceBytesIdentityAndMetadataWithoutBuilder(string copies)
        {
            var root = NewCase();
            SaveCommitTicket original;
            PreparedCandidateApplicationIntent intent;
            byte[] bytes;
            using (var writer = new PendingRig("player:015b", root))
            {
                original = writer.Application(out intent);
                bytes = PendingRig.Bytes(original.Envelope);
                writer.FailWrite(original, copies == "work" ? "Snapshot.Flush.after" : "Snapshot.Promote.after");
                if (copies == "pair") writer.Put(PendingRig.Work(original), bytes);
            }
            using (var r = new ApplicationRuntimeRig(root))
            {
                Is(r.Open(SaveOpenMode.Existing), "Pending");
                Assert.IsNull(r.Head);
                Assert.IsNull(r.Model.View.PendingOperationId);
                var ids = r.Model.View.ObservedCandidateCommitIds;
                CollectionAssert.AreEqual(new[] { original.Metadata.CommitId }, ids);
                Assert.Throws<NotSupportedException>(() => ((IList<string>)ids).Clear());
                Is(r.Query(intent), "ResolutionRequired");
                var events = 0;
                var handle = r.Architecture.RegisterEvent<CandidateApplicationPublished>(_ => events++);
                try
                {
                    var result = Is(r.Resume(original.Metadata.CommitId), "Completed");
                    Assert.AreEqual(original.Metadata.CommitId, result.OriginalCommitId);
                    SameDescriptor(PendingRig.Descriptor(original), r.Head.Descriptor);
                    Assert.AreEqual(original.Metadata.SaveGeneration, r.Head.Header.SaveGeneration);
                    Assert.AreEqual(original.Metadata.ParentCommitId, r.Head.Header.ParentCommitId);
                    CollectionAssert.AreEqual(original.OperationIds, r.Head.Header.CommitIndex.Last().OperationIds);
                    CollectionAssert.AreEqual(bytes, File.ReadAllBytes(r.Path(SnapshotName(original.Metadata.CommitId))));
                    Assert.AreEqual(1, r.Head.Records.Count);
                    Assert.AreEqual("init", r.Head.Records[0].OperationId);
                    Assert.AreEqual(0, r.Builds);
                    Assert.AreEqual(1, events);
                    Assert.AreEqual(1, r.Storage.Base.RealMarkerPromotionsFor(original.Metadata.CommitId));
                    Is(r.Query(intent), "Completed");
                    var disk = r.Disk();
                    var deletes = r.Storage.Deletes;
                    Is(r.EndObserved(original.Metadata.CommitId), "ObservedAlreadyCommitted");
                    Is(r.Resume(original.Metadata.CommitId), "ObservedAlreadyCommitted");
                    r.SameDisk(disk);
                    Assert.AreEqual(deletes, r.Storage.Deletes);
                    Assert.AreEqual(1, events);
                }
                finally { handle.UnRegister(); }
            }
        }

        [TestCase("bad-business")]
        [TestCase("partial-snapshot")]
        [TestCase("partial-marker")]
        public void A06_UnsafeCandidatesNeverWriteOrExposeDecodedCompletionAndRequireExplicitEnd(string shape)
        {
            var root = NewCase();
            SaveCommitTicket original;
            PreparedCandidateApplicationIntent intent;
            using (var writer = new PendingRig("player:015b", root))
            {
                original = writer.Application(out intent, shape == "bad-business");
                writer.FailWrite(original, shape == "partial-snapshot" ? "Snapshot.Write.partial" :
                    shape == "partial-marker" ? "Marker.Write.partial" : "Snapshot.Flush.after");
            }
            using (var r = new ApplicationRuntimeRig(root))
            {
                var opened = r.Open(SaveOpenMode.Existing);
                Is(opened, shape == "bad-business" ? "Pending" : "RecoveryBlocked");
                var before = r.Disk();
                var resumed = r.Resume(original.Metadata.CommitId);
                Assert.AreNotEqual("Completed", resumed.Code);
                Assert.IsNull(resumed.OriginalLookup);
                Assert.IsNull(r.Head);
                Assert.IsNull(r.Model.View.PendingOperationId);
                if (shape == "bad-business") Assert.AreEqual("Candidate.Decode", resumed.Diagnostic.Stage);
                else Is(resumed, "RecoveryBlocked");
                Assert.AreEqual(0, r.Builds);
                Assert.AreEqual(0, r.Storage.Base.WriteCalls);
                Assert.AreEqual(0, r.Storage.Deletes);
                r.SameDisk(before);
                Is(r.Query(intent), "ResolutionRequired");
                Is(r.EndObserved(original.Metadata.CommitId), "Ended");
                Assert.AreEqual(0, r.Model.View.ObservedCandidateCommitIds.Count);
                Assert.AreEqual(CandidateApplicationPhase.RecoveryBlocked, r.Model.View.Phase, "Existing cannot initialize after End");
                Is(r.EndObserved(original.Metadata.CommitId), "CandidateNotFound");
            }
        }

        [Test]
        public void A06_PendingDiskCapabilityDenialKeepsFrozenObservationAndOriginalFiles()
        {
            var root = NewCase();
            SaveCommitTicket original;
            using (var writer = new PendingRig("player:015b", root))
            {
                original = writer.Application(out _);
                writer.FailWrite(original, "Snapshot.Flush.after");
            }
            using (var r = new ApplicationRuntimeRig(root))
            {
                Is(r.Open(SaveOpenMode.Existing, Without(Capabilities(), "slices")), "UnsupportedCapability");
                var before = r.Disk();
                var failed = Is(r.Resume(original.Metadata.CommitId), "UnsupportedCapability");
                Assert.IsNull(failed.OriginalLookup);
                Assert.IsNull(r.Head);
                Assert.IsNull(r.Model.View.PendingCommitId);
                Assert.AreEqual(0, r.Storage.Base.WriteCalls);
                r.SameDisk(before);
            }
        }

        [Test]
        public void A06_MultipleGroupsRequireExplicitSelectionAndEndOnlyRemovesSelectedGroup()
        {
            var root = NewCase();
            SaveCommitTicket first, second;
            using (var writer = new PendingRig("player:015b", root))
            {
                first = writer.Application(out _);
                writer.FailWrite(first, "Snapshot.Flush.after");
            }
            using (var other = new PendingRig("player:015b"))
            {
                second = other.Application(out _);
            }
            using (var r = new ApplicationRuntimeRig(root))
            {
                File.WriteAllBytes(r.Path(PendingRig.Work(second)), PendingRig.Bytes(second.Envelope));
                Is(r.Open(SaveOpenMode.Existing), "Pending");
                CollectionAssert.AreEquivalent(new[] { first.Metadata.CommitId, second.Metadata.CommitId }, r.Model.View.ObservedCandidateCommitIds);
                var frozen = r.Model.View.ObservedCandidateCommitIds;
                var before = r.Disk();
                Is(r.Resume(first.Metadata.CommitId), "Pending");
                r.SameDisk(before);
                Assert.AreEqual(0, r.Storage.Deletes);
                Is(r.EndObserved(first.Metadata.CommitId), "Ended");
                Assert.AreEqual(2, frozen.Count);
                CollectionAssert.AreEqual(new[] { second.Metadata.CommitId }, r.Model.View.ObservedCandidateCommitIds);
                CollectionAssert.AreEqual(before[PendingRig.Work(second)], File.ReadAllBytes(r.Path(PendingRig.Work(second))));
                Assert.AreEqual(1, r.Storage.Deletes, "only selected group removed");
                Is(r.Resume(second.Metadata.CommitId), "Completed");
                Assert.AreEqual(0, r.Builds);
                Assert.AreEqual(2, r.Storage.Deletes, "resumed Write replaces its own original work file");
            }
        }

        [TestCase("bytes")]
        [TestCase("unknown")]
        public void A06_ObservationChangesRejectWithoutAutomaticDeletionOrWrite(string change)
        {
            var root = NewCase();
            SaveCommitTicket original;
            using (var writer = new PendingRig("player:015b", root))
            {
                original = writer.Application(out _);
                writer.FailWrite(original, "Snapshot.Flush.after");
            }
            using (var r = new ApplicationRuntimeRig(root))
            {
                Is(r.Open(SaveOpenMode.Existing), "Pending");
                File.WriteAllBytes(r.Path(change == "bytes" ? PendingRig.Work(original) : "foreign"), new byte[] { 9 });
                var changed = r.Disk();
                Is(r.Resume(original.Metadata.CommitId), "StaleContext");
                Is(r.EndObserved(original.Metadata.CommitId), "StaleContext");
                Assert.AreEqual(0, r.Storage.Base.WriteCalls);
                Assert.AreEqual(0, r.Storage.Deletes);
                r.SameDisk(changed);
            }
        }

        [Test]
        public void A06_ChangeAfterReadBeforeResumeCannotInstallTicket()
        {
            var root = NewCase();
            SaveCommitTicket original;
            using (var writer = new PendingRig("player:015b", root))
            {
                original = writer.Application(out _);
                writer.FailWrite(original, "Snapshot.Flush.after");
            }
            using (var r = new ApplicationRuntimeRig(root))
            {
                Is(r.Open(SaveOpenMode.Existing), "Pending");
                var opens = 0;
                var changed = false;
                r.Storage.BeforeOpen = name =>
                {
                    if (name == PendingRig.Work(original) && ++opens == 6)
                    {
                        changed = true;
                        File.WriteAllBytes(r.Path(name), new byte[] { 8 });
                    }
                };
                Is(r.Resume(original.Metadata.CommitId), "StaleContext");
                Assert.IsTrue(changed);
                Assert.IsNull(r.Model.View.PendingCommitId);
                Assert.IsNull(r.Head);
                Assert.AreEqual(0, r.Storage.Base.WriteCalls);
                Assert.AreEqual(0, r.Storage.Deletes);
            }
        }

        [Test]
        public void A06_EndInterruptedRefreshesObservationBeforeFinishingRemainingFiles()
        {
            var root = NewCase();
            SaveCommitTicket original;
            using (var writer = new PendingRig("player:015b", root))
            {
                original = writer.Application(out _);
                writer.FailWrite(original, "Marker.Write.partial");
                writer.Put(PendingRig.Work(original), PendingRig.Bytes(original.Envelope));
            }
            using (var r = new ApplicationRuntimeRig(root))
            {
                Is(r.Open(SaveOpenMode.Existing), "RecoveryBlocked");
                var before = r.Disk();
                r.Storage.AfterDelete = name => { throw new IOException("end interrupted after real delete"); };
                var failed = Is(r.EndObserved(original.Metadata.CommitId), "EndInterrupted");
                Assert.AreEqual("StorageFailure", failed.Diagnostic.Code);
                StringAssert.Contains(PendingRig.MarkerWork(original), failed.Diagnostic.Stage);
                Assert.AreEqual(1, r.Storage.Deletes);
                Assert.IsFalse(File.Exists(r.Path(PendingRig.MarkerWork(original))));
                CollectionAssert.AreEqual(before[PendingRig.Work(original)], File.ReadAllBytes(r.Path(PendingRig.Work(original))));
                Assert.AreEqual(1, r.Model.View.ObservedCandidateCommitIds.Count);
                r.Storage.AfterDelete = null;
                Is(r.EndObserved(original.Metadata.CommitId), "Ended");
                Assert.AreEqual(3, r.Storage.Deletes);
                Assert.AreEqual(0, r.Model.View.ObservedCandidateCommitIds.Count);
                Is(r.EndObserved(original.Metadata.CommitId), "CandidateNotFound");
                Assert.AreEqual(0, r.Storage.Base.WriteCalls);
            }
        }

        [TestCase("SaveFailed")]
        [TestCase("CommitUnknown")]
        public void A06_ResumedFailureKeepsOriginalSlotForResolveRetryAndNeverRebuilds(string code)
        {
            var root = NewCase();
            SaveCommitTicket original;
            PreparedCandidateApplicationIntent intent;
            using (var writer = new PendingRig("player:015b", root))
            {
                original = writer.Application(out intent);
                writer.FailWrite(original, "Snapshot.Flush.after");
            }
            using (var r = new ApplicationRuntimeRig(root))
            {
                Is(r.Open(SaveOpenMode.Existing), "Pending");
                r.Storage.Base.Arm(code == "SaveFailed" ? "Snapshot.Flush.after" : "Marker.Promote.after", original.Metadata.CommitId);
                var failed = Is(r.Resume(original.Metadata.CommitId), code);
                Assert.AreEqual(original.Metadata.CommitId, failed.View.PendingCommitId);
                Assert.AreEqual(intent.OperationId, failed.View.PendingOperationId);
                Assert.IsNull(failed.OriginalLookup);
                var resolved = r.Resolve(intent);
                if (code == "SaveFailed")
                {
                    Is(resolved, "ConfirmedNotCommitted");
                    resolved = r.Retry(intent);
                }
                Is(resolved, "Completed");
                Assert.AreEqual(original.Metadata.CommitId, resolved.OriginalCommitId);
                CollectionAssert.AreEqual(PendingRig.Bytes(original.Envelope), File.ReadAllBytes(r.Path(SnapshotName(original.Metadata.CommitId))));
                Assert.AreEqual(0, r.Builds);
                Assert.AreEqual(1, r.Storage.Base.RealMarkerPromotionsFor(original.Metadata.CommitId));
            }
        }

        [Test]
        public void A07_H04AndH06AreSeparateCommitsAndH06FailureCannotRollbackPublishedVictory()
        {
            using (var r = new ApplicationRuntimeRig())
            {
                var publications = new List<CandidateApplicationView>();
                var handle = r.Architecture.RegisterEvent<CandidateApplicationPublished>(e => publications.Add(e.View));
                try
                {
                    Is(r.Open(), "InitializationReady");
                    Is(r.Initialize(), "Completed");
                    Is(r.Enter(), "Completed");
                    var h04 = r.Win();
                    var victory = r.Head;
                    Assert.AreEqual(CandidateApplicationContinuationStage.AwaitBaseSettlement, victory.Continuation.Stage);
                    Assert.AreEqual(h04.OriginalCommitId, publications.Last().PublishedSnapshot.Header.CommitId);
                    var terminal = r.Intents[victory.Continuation.ClosingOperationId];
                    var builds = r.Builds;
                    var writes = r.Storage.Base.WriteCalls;
                    Is(r.Submit(terminal, null), "Completed");
                    Is(r.Query(terminal), "Completed");
                    Is(r.Retry(terminal), "Completed");
                    Assert.AreEqual(builds, r.Builds);
                    Assert.AreEqual(writes, r.Storage.Base.WriteCalls);
                    var wrong = r.VictoryInput();
                    wrong.OperationId = "wrong-reservation";
                    Is(r.Submit(r.Freeze(wrong), r.SettleBuild), "InvalidContinuation");
                    Assert.AreEqual(builds, r.Builds);
                    var input = r.VictoryInput();
                    Assert.AreEqual(victory.Header.CommitId, input.ExpectedCommitId);
                    var settlement = r.Freeze(input);
                    r.Storage.Base.Arm("Snapshot.Flush.after", null);
                    var failed = Is(r.Submit(settlement, r.SettleBuild), "SaveFailed");
                    Assert.AreSame(victory, failed.View.PublishedSnapshot);
                    Assert.AreEqual(victory.Continuation.ReservedOperationId, failed.View.PendingOperationId);
                    Assert.AreEqual(h04.OriginalCommitId, Is(r.Query(terminal), "Completed").OriginalCommitId);
                    var completed = Is(r.Retry(settlement), "Completed");
                    Assert.AreNotEqual(h04.OriginalCommitId, completed.OriginalCommitId);
                    Assert.AreEqual(victory.Header.CommitId, r.Head.Header.ParentCommitId);
                    Assert.IsNull(r.Head.Continuation);
                    Assert.IsNull(r.Head.Business.ActiveHistory);
                    Assert.AreEqual(builds + 1, r.Builds);
                    Assert.AreEqual(1, r.Head.Business.Rewards.BaseRewards.Count);
                    Assert.AreEqual(1, r.Head.Business.Character.BaseRewards.Count);
                    Assert.IsNotNull(completed.OriginalLookup.Reward);
                    Assert.AreEqual(completed.OriginalCommitId, Is(r.Query(settlement), "Completed").OriginalCommitId);
                    Assert.AreEqual(h04.OriginalCommitId, Is(r.Query(terminal), "Completed").OriginalCommitId);
                    Assert.AreEqual(1, r.Storage.Base.RealMarkerPromotionsFor(h04.OriginalCommitId));
                    Assert.AreEqual(1, r.Storage.Base.RealMarkerPromotionsFor(completed.OriginalCommitId));
                    Assert.AreEqual(victory.Header.CommitId, publications[publications.Count - 2].PublishedSnapshot.Header.CommitId);
                }
                finally { handle.UnRegister(); }
            }
        }

        [Test]
        public void A07_RestoreAwaitBaseSettlementDoesNotAutomaticallyInvokeSettlementOwner()
        {
            string root, reserved, closing, commit;
            PreparedCandidateApplicationIntent terminal;
            using (var r = new ApplicationRuntimeRig())
            {
                root = r.Root;
                Is(r.Open(), "InitializationReady");
                Is(r.Initialize(), "Completed");
                Is(r.Enter(), "Completed");
                r.Win();
                reserved = r.Head.Continuation.ReservedOperationId;
                closing = r.Head.Continuation.ClosingOperationId;
                commit = r.Head.Header.CommitId;
                terminal = r.Intents[closing];
            }
            using (var fresh = new ApplicationRuntimeRig(root))
            {
                Is(fresh.Open(SaveOpenMode.Existing), "Ready");
                Assert.AreEqual(commit, fresh.Head.Header.CommitId);
                Assert.AreEqual(reserved, fresh.Head.Continuation.ReservedOperationId);
                Assert.AreEqual(closing, fresh.Head.Continuation.ClosingOperationId);
                Assert.AreEqual(0, fresh.Builds);
                Assert.AreEqual(0, fresh.Storage.Base.WriteCalls);
                Is(fresh.Query(terminal), "Completed");
                var result = Is(fresh.Submit(fresh.Freeze(fresh.VictoryInput()), fresh.SettleBuild), "Completed");
                Assert.AreNotEqual(commit, result.OriginalCommitId);
                Assert.AreEqual(1, fresh.Builds);
            }
        }
    }
}
