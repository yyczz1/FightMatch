using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using FightMatch.Platform;
using FightMatch.Tests;
using NUnit.Framework;
using static FightMatch.Core.Tests.LocalSaveTestFiles;

namespace FightMatch.Core.Tests
{
    [TestFixture]
    public sealed class SavePendingRecoveryTests
    {
        [TestCase(false, "work")]
        [TestCase(false, "snapshot")]
        [TestCase(false, "pair")]
        [TestCase(true, "work")]
        [TestCase(true, "snapshot")]
        [TestCase(true, "pair")]
        public void OriginalIdentityAndAllBytesSurviveExistingReopen(bool successor, string copies)
        {
            using (var r = new PendingRig())
            {
                var head = successor ? r.Commit(null, "old").CurrentHead : null;
                var ticket = r.Leave(head, "next", copies == "work" ? "Snapshot.Flush.after" : "Snapshot.Promote.after");
                var original = PendingRig.Bytes(ticket.Envelope);
                if (copies == "pair") r.Put(PendingRig.Work(ticket), original);
                r.Reopen();
                var before = r.Disk();
                var view = r.View();
                var candidate = Ok(r.Store.ReadUncommittedCandidate(view, ticket.Metadata.CommitId, B()), "CandidateRead");
                r.SameDisk(before);
                Bad(r.Store.Load(B()), "CommitUnknown");
                Bad(r.Store.Write(ticket, B()), "TicketOwnerMismatch");
                SameDescriptor(PendingRig.Descriptor(ticket), candidate.Descriptor);
                Assert.AreEqual(ticket.Metadata.SaveGeneration, candidate.Metadata.SaveGeneration);
                Assert.AreEqual(ticket.Metadata.ParentCommitId, candidate.Metadata.ParentCommitId);
                CollectionAssert.AreEqual(ticket.OperationIds, candidate.OperationIds);
                CollectionAssert.AreEqual(original, PendingRig.Bytes(candidate.Envelope));
                CollectionAssert.AreEqual(Hash(original), candidate.Descriptor.Sha256);
                Assert.AreEqual(head?.CommitId, candidate.ExpectedHead?.CommitId);
                AssertFrozen(candidate);
                var resumed = Ok(r.Store.ResumeRecoveredCandidate(candidate, r.Caps(view), B()), "Resumed");
                r.SameDisk(before);
                Assert.AreSame(candidate.Envelope, resumed.Envelope);
                Bad(r.Store.ResumeRecoveredCandidate(candidate, r.Caps(view), B()), "Busy");
                Bad(r.Store.ReadUncommittedCandidate(view, ticket.Metadata.CommitId, B()), "Busy");
                Bad(r.Store.EndObservedCandidate(view, ticket.Metadata.CommitId, B()), "Busy");
                var committed = Ok(r.Store.Write(resumed, B()), "Committed");
                SameDescriptor(PendingRig.Descriptor(ticket), committed.Descriptor);
                CollectionAssert.AreEqual(original, File.ReadAllBytes(r.Path(SnapshotName(ticket.Metadata.CommitId))));
                Assert.AreEqual(ticket.Metadata.CommitId, Ok(r.Store.Lookup(null, "next", B()), "Committed").Descriptor.CommitId);
                CollectionAssert.AreEqual(original, PendingRig.Bytes(Ok(r.Store.Load(B()), "Loaded")));
                Assert.AreEqual(successor ? 2 : 1, r.Builds);
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void SixSliceBusinessDecodePrecedesResumeAndOriginalOperationLookup(bool corruptBusiness)
        {
            using (var r = new PendingRig("player:015b"))
            {
                PreparedCandidateApplicationIntent intent;
                var ticket = r.Application(out intent, corruptBusiness);
                r.FailWrite(ticket, "Snapshot.Flush.after");
                r.Reopen();
                var before = r.Disk();
                var view = r.View();
                var recovered = Ok(r.Store.ReadUncommittedCandidate(view, ticket.Metadata.CommitId, B()), "CandidateRead");
                Assert.AreEqual(6, recovered.Envelope.SliceBytes.Count);
                Assert.AreEqual(32, recovered.Metadata.CommitId.Length);
                var decoded = CandidateApplicationSaveCodec.Decode(recovered.Envelope, B().Codec);
                if (corruptBusiness)
                {
                    Assert.IsFalse(decoded.IsAccepted);
                    Assert.IsNull(decoded.Value);
                    r.SameDisk(before);
                    Assert.IsFalse(r.View().RetainedRoots.Any(x => x.SourceName == "pending-ticket"));
                    Bad(r.Store.Load(B()), "CommitUnknown");
                    Ok(r.Store.EndObservedCandidate(view, ticket.Metadata.CommitId, B()), "Ended");
                    return;
                }
                var snapshot = BusinessSaveScenario.Accept(decoded);
                var lookup = BusinessSaveScenario.Accept(CandidateApplicationProtocol.Lookup(snapshot, intent, B().Codec));
                Assert.IsTrue(lookup.IsFound);
                Assert.AreEqual(ticket.Metadata.CommitId, lookup.OriginalCommitId);
                Assert.AreEqual(BigInteger.One, lookup.OriginalGeneration);
                Assert.AreEqual(snapshot.Business.Character.CharacterId, lookup.Initialization.CharacterId);
                r.SameDisk(before);
                var resumed = Ok(r.Store.ResumeRecoveredCandidate(recovered, r.Caps(view), B()), "Resumed");
                var saved = Ok(r.Store.Write(resumed, B()), "Committed");
                Assert.AreEqual(lookup.OriginalCommitId, saved.Descriptor.CommitId);
                var loaded = BusinessSaveScenario.Accept(CandidateApplicationSaveCodec.Decode(Ok(r.Store.Load(B()), "Loaded"), B().Codec));
                var again = BusinessSaveScenario.Accept(CandidateApplicationProtocol.Lookup(loaded, intent, B().Codec));
                Assert.AreEqual(lookup.OriginalCommitId, again.OriginalCommitId);
                Assert.AreEqual(1, r.Builds);
                var publications = 0;
                Ok(r.Store.WithVerifiedRecovery(r.View(), r.Caps(r.View()), _ => publications++, B()), "Verified");
                Assert.AreEqual(1, publications, "fixture witness only; no application gate implemented");
            }
        }

        [Test]
        public void ForeignOwnerAndOldInMemoryTicketCannotBeRebound()
        {
            using (var a = new PendingRig())
            using (var b = new PendingRig())
            {
                var ticket = a.Leave(null, "original");
                a.Reopen();
                var view = a.View();
                var candidate = Ok(a.Store.ReadUncommittedCandidate(view, ticket.Metadata.CommitId, B()), "CandidateRead");
                var before = b.Disk();
                Bad(b.Store.ReadUncommittedCandidate(view, ticket.Metadata.CommitId, B()), "StaleContext");
                Bad(b.Store.EndObservedCandidate(view, ticket.Metadata.CommitId, B()), "StaleContext");
                Bad(b.Store.ResumeRecoveredCandidate(candidate, a.Caps(view), B()), "StaleContext");
                Bad(b.Store.Write(ticket, B()), "TicketOwnerMismatch");
                b.SameDisk(before);
                Assert.IsFalse(b.View().RetainedRoots.Any(x => x.SourceName == "pending-ticket"));
            }
        }

        [TestCase("bytes")]
        [TestCase("file")]
        [TestCase("directory")]
        [TestCase("head")]
        public void ChangedEvidenceRejectsReadResumeAndEndWithoutWrites(string change)
        {
            using (var r = new PendingRig())
            {
                var head = change == "head" ? r.Commit(null, "first").CurrentHead : null;
                var ticket = r.Leave(head, "candidate");
                r.Reopen();
                var view = r.View();
                var candidate = Ok(r.Store.ReadUncommittedCandidate(view, ticket.Metadata.CommitId, B()), "CandidateRead");
                if (change == "bytes") r.Put(PendingRig.Work(ticket), new byte[] { 9 });
                if (change == "file") r.Put("unknown", new byte[] { 7 });
                if (change == "directory") Directory.CreateDirectory(r.Path("unknown-directory"));
                if (change == "head")
                {
                    var bytes = File.ReadAllBytes(r.Path(PendingRig.Work(ticket)));
                    File.Delete(r.Path(PendingRig.Work(ticket)));
                    r.Commit(head, "intervening");
                    r.Put(PendingRig.Work(ticket), bytes);
                }
                var before = r.Disk();
                Bad(r.Store.ReadUncommittedCandidate(view, ticket.Metadata.CommitId, B()), "StaleContext");
                Bad(r.Store.ResumeRecoveredCandidate(candidate, r.Caps(view), B()), "StaleContext");
                Bad(r.Store.EndObservedCandidate(view, ticket.Metadata.CommitId, B()), "StaleContext");
                r.SameDisk(before);
                Assert.AreEqual(0, r.Storage.Deletes);
                if (change == "head")
                    Bad(r.Store.ReadUncommittedCandidate(r.View(), ticket.Metadata.CommitId, B()), "StaleContext");
            }
        }

        [TestCase("history-operations")]
        [TestCase("history-sha")]
        [TestCase("history-length")]
        [TestCase("reused-operation")]
        public void CandidateHistoryMustExactlyExtendTheCurrentIndex(string mutation)
        {
            using (var r = new PendingRig())
            {
                var head = r.Commit(null, "indexed").CurrentHead;
                var ticket = r.Leave(head, "candidate");
                r.Reopen();
                var input = EmptyInput(ticket.Metadata);
                var rows = input.CommitIndex.ToArray();
                var old = rows[0];
                rows[0] = new SaveCommitIndexEntry(old.Generation, old.CommitId, old.ParentCommitId,
                    mutation == "history-length" ? old.SnapshotLength + 1 : old.SnapshotLength,
                    mutation == "history-sha" ? Enumerable.Repeat((byte)3, 32).ToArray() : old.SnapshotSha256,
                    mutation == "history-operations" ? new[] { "wrong" } : old.OperationIds);
                if (mutation == "reused-operation")
                {
                    rows[0] = new SaveCommitIndexEntry(old.Generation, old.CommitId, old.ParentCommitId,
                        old.SnapshotLength, old.SnapshotSha256, new[] { "different-history" });
                    rows[1] = new SaveCommitIndexEntry(rows[1].Generation, rows[1].CommitId, rows[1].ParentCommitId, null, null, new[] { "indexed" });
                }
                input.CommitIndex = rows;
                var invalid = SaveEnvelopeCodec.Prepare(input, B().Codec);
                // Internally valid metadata must still match the actual current head and its operations.
                Assert.IsTrue(invalid.IsAccepted, invalid.RejectionCode + ":" + invalid.FieldPath);
                r.Put(PendingRig.Work(ticket), PendingRig.Bytes(invalid.Value));
                var view = r.View();
                Assert.IsTrue(view.RequirementsComplete);
                var before = r.Disk();
                Bad(r.Store.ReadUncommittedCandidate(view, ticket.Metadata.CommitId, B()), "InconsistentBinding");
                r.SameDisk(before);
                Assert.IsFalse(r.View().RetainedRoots.Any(x => x.SourceName == "pending-ticket"));
            }
        }

        [TestCase("backup", "binding")]
        [TestCase("current", "binding")]
        [TestCase("pending", "binding")]
        [TestCase("backup", "slice")]
        [TestCase("current", "slice")]
        [TestCase("pending", "slice")]
        [TestCase("pending", "notes")]
        [TestCase("pending", "rule")]
        [TestCase("pending", "numeric")]
        [TestCase("pending", "random")]
        [TestCase("pending", "feature")]
        public void EveryReadableRootAndCompleteCapabilityValueMustBeSupported(string root, string dimension)
        {
            using (var r = new PendingRig())
            {
                var first = r.Commit(null, "backup").CurrentHead;
                var second = r.Commit(first, "current").CurrentHead;
                var ticket = r.Leave(second, "pending");
                r.Reopen();
                var view = r.View();
                var candidate = Ok(r.Store.ReadUncommittedCandidate(view, ticket.Metadata.CommitId, B()), "CandidateRead");
                var caps = r.Caps(view);
                var bindings = caps.Bindings.ToList();
                if (dimension == "binding") bindings.RemoveAll(x => x.DraftId == "draft:" + root);
                if (dimension == "notes")
                {
                    var old = bindings.Single(x => x.DraftId == "draft:pending");
                    bindings.Remove(old);
                    bindings.Add(new SaveBinding(old.Kind, old.PackageId, old.DraftId, old.DraftRevision, old.ContentFingerprint,
                        old.RuleVersion, old.NumericContractVersion, old.RandomContractVersion, old.SourceNotes.Reverse().ToArray(),
                        old.LevelId, old.LevelVersion));
                }
                var changed = new SaveRecoveryCapabilities(
                    caps.ReadableSlices.Where(x => dimension != "slice" || x.SliceId != "slice:" + root).ToArray(), bindings,
                    caps.RuleVersions.Where(x => dimension != "rule" || x != "r:pending").ToArray(),
                    caps.NumericContractVersions.Where(x => dimension != "numeric" || x != "n:pending").ToArray(),
                    caps.RandomContractVersions.Where(x => dimension != "random" || x != "q:pending").ToArray(),
                    caps.FeatureIds.Where(x => dimension != "feature" || x != "f:pending").ToArray());
                var before = r.Disk();
                Bad(r.Store.ResumeRecoveredCandidate(candidate, changed, B()),
                    dimension == "binding" || dimension == "notes" ? "UnsupportedBinding" : "UnsupportedCapability");
                r.SameDisk(before);
                Assert.IsFalse(r.View().RetainedRoots.Any(x => x.SourceName == "pending-ticket"));
                Ok(r.Store.ResumeRecoveredCandidate(candidate, caps, B()), "Resumed");
            }
        }

        [Test]
        public void SharedMathAndStorageBudgetsFailWithoutAPendingSlotOrWrites()
        {
            using (var r = new PendingRig())
            {
                var ticket = r.Leave(null, "candidate");
                r.Reopen();
                var view = r.View();
                var before = r.Disk();
                var measured = B();
                var candidate = Ok(r.Store.ReadUncommittedCandidate(view, ticket.Metadata.CommitId, measured), "CandidateRead");
                var steps = measured.Codec.Math.PrimitiveStepsUsed;
                Assert.Greater(steps, 1);
                var shortMath = new SaveStoreBudget(new SaveCodecBudget(new ExactMathBudget(maxPrimitiveSteps: (int)steps - 1)));
                var failure = r.Store.ReadUncommittedCandidate(view, ticket.Metadata.CommitId, shortMath);
                Bad(failure, "Limit");
                Assert.IsNotEmpty(failure.LimitReason);
                Assert.GreaterOrEqual(shortMath.Codec.Math.PrimitiveStepsUsed, steps - 1);
                r.SameDisk(before);
                Assert.IsFalse(r.View().RetainedRoots.Any(x => x.SourceName == "pending-ticket"));
                var resumeBudget = B();
                Ok(r.Store.ResumeRecoveredCandidate(candidate, r.Caps(view), resumeBudget), "Resumed");
                var resumeSteps = resumeBudget.Codec.Math.PrimitiveStepsUsed;
                r.Reopen();
                view = r.View();
                candidate = Ok(r.Store.ReadUncommittedCandidate(view, ticket.Metadata.CommitId, B()), "CandidateRead");
                var shortResume = new SaveStoreBudget(new SaveCodecBudget(new ExactMathBudget(maxPrimitiveSteps: (int)resumeSteps - 1)));
                Bad(r.Store.ResumeRecoveredCandidate(candidate, r.Caps(view), shortResume), "Limit");
                r.SameDisk(before);
                Assert.IsFalse(r.View().RetainedRoots.Any(x => x.SourceName == "pending-ticket"));
                var resumed = Ok(r.Store.ResumeRecoveredCandidate(candidate, r.Caps(view), B()), "Resumed");
                Ok(r.Store.EndUncommitted(resumed, B()), "Ended");
            }
        }

        [TestCase("read")]
        [TestCase("resume")]
        [TestCase("end")]
        public void LimitIsNeverReportedAsAnIoInterruption(string action)
        {
            using (var r = new PendingRig())
            {
                var ticket = r.Leave(null, "candidate");
                r.Reopen();
                var view = r.View();
                var candidate = Ok(r.Store.ReadUncommittedCandidate(view, ticket.Metadata.CommitId, B()), "CandidateRead");
                var small = new SaveStoreBudget(new SaveCodecBudget(new ExactMathBudget(), maxEnvelopeBytes: 1));
                var before = r.Disk();
                if (action == "read") Bad(r.Store.ReadUncommittedCandidate(view, ticket.Metadata.CommitId, small), "Limit");
                if (action == "resume") Bad(r.Store.ResumeRecoveredCandidate(candidate, r.Caps(view), small), "Limit");
                if (action == "end") Bad(r.Store.EndObservedCandidate(view, ticket.Metadata.CommitId, small), "Limit");
                r.SameDisk(before);
                Assert.IsFalse(r.View().RetainedRoots.Any(x => x.SourceName == "pending-ticket"));
            }
        }

        [TestCase("partial")]
        [TestCase("conflict")]
        [TestCase("partial-marker")]
        [TestCase("marker-only")]
        [TestCase("unknown")]
        [TestCase("bad-marker")]
        [TestCase("two-commits")]
        public void ClassificationNeverPromotesIncompleteOrAmbiguousEvidence(string kind)
        {
            using (var r = new PendingRig())
            {
                var ticket = r.Leave(null, "candidate", "Snapshot.Promote.after");
                var id = ticket.Metadata.CommitId;
                r.Reopen();
                if (kind == "partial") r.Put(PendingRig.Work(ticket), new byte[] { 1, 2 });
                if (kind == "conflict")
                {
                    var changed = BusinessSaveScenario.Slices(ticket.Envelope);
                    changed[0].Bytes = new byte[] { 8 };
                    r.Put(PendingRig.Work(ticket), PendingRig.Bytes(BusinessSaveScenario.Repack(ticket.Envelope, changed)));
                }
                if (kind == "partial-marker" || kind == "marker-only") r.Put(PendingRig.MarkerWork(ticket), new byte[] { 1 });
                if (kind == "marker-only") File.Delete(r.Path(SnapshotName(id)));
                if (kind == "unknown") r.Put("unattributed", new byte[] { 1 });
                if (kind == "bad-marker") r.Put(MarkerName(id), new byte[] { 1 });
                if (kind == "two-commits")
                {
                    using (var other = new PendingRig())
                    {
                        var t = other.Leave(null, "other");
                        r.Put(PendingRig.Work(t), PendingRig.Bytes(t.Envelope));
                    }
                }
                var view = r.View();
                var before = r.Disk();
                Bad(r.Store.ReadUncommittedCandidate(view, id, B()), kind == "two-commits" ? "Pending" : "RecoveryBlocked");
                r.SameDisk(before);
                if (kind == "unknown" || kind == "bad-marker")
                {
                    Bad(r.Store.EndObservedCandidate(view, id, B()), "RecoveryBlocked");
                    r.SameDisk(before);
                    Assert.AreEqual(0, r.Storage.Deletes);
                }
                else Ok(r.Store.EndObservedCandidate(view, id, B()), "Ended");
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void CommittedAndIndexedAncestorsAreNeverResumedOrEnded(bool ancestor)
        {
            using (var r = new PendingRig())
            {
                var first = r.Commit(null, "first");
                if (ancestor) r.Commit(first.CurrentHead, "second");
                var id = first.Descriptor.CommitId;
                r.Reopen();
                var view = r.View();
                var before = r.Disk();
                Bad(r.Store.ReadUncommittedCandidate(view, id, B()), "AlreadyCommitted");
                Bad(r.Store.EndObservedCandidate(view, id, B()), "AlreadyCommitted");
                r.SameDisk(before);
                Ok(r.Store.Lookup(id, "first", B()), "Committed");
            }
        }

        [Test]
        public void EndOnlyTheSelectedPartialGroupAndPreserveCurrentHistoryAndOtherGroup()
        {
            using (var r = new PendingRig())
            {
                var first = r.Commit(null, "first").CurrentHead;
                var head = r.Commit(first, "second").CurrentHead;
                var ticket = r.Leave(head, "third", "Marker.Write.partial");
                r.Put(PendingRig.Work(ticket), new byte[] { 8, 9 });
                var other = "w-" + Guid.NewGuid().ToString("N") + ".snapshot.tmp";
                r.Put(other, new byte[] { 4 });
                r.Reopen();
                var view = r.View();
                Assert.IsTrue(view.EvidenceComplete);
                Assert.IsFalse(view.RequirementsComplete);
                var before = r.Disk();
                var result = Ok(r.Store.EndObservedCandidate(view, ticket.Metadata.CommitId, B()), "Ended");
                var names = PendingRig.Targets(ticket);
                CollectionAssert.AreEqual(names, result.RemovedNames);
                Assert.AreEqual(ticket.Metadata.CommitId, result.CommitId);
                SameDescriptor(head, result.CurrentHead);
                Assert.Throws<NotSupportedException>(() => ((IList<string>)result.RemovedNames).Add("bad"));
                r.SameDisk(before, names);
                Assert.AreEqual(SaveHeadStatus.Pending, r.View().Status);
                Bad(r.Store.Load(B()), "CommitUnknown");
                Bad(r.Store.EndObservedCandidate(view, ticket.Metadata.CommitId, B()), "StaleContext");
                Bad(r.Store.EndObservedCandidate(r.View(), ticket.Metadata.CommitId, B()), "CandidateNotFound");
                Ok(r.Store.Lookup(first.CommitId, "first", B()), "Committed");
            }
        }

        [TestCase(0, false)]
        [TestCase(0, true)]
        [TestCase(1, false)]
        [TestCase(1, true)]
        [TestCase(2, false)]
        [TestCase(2, true)]
        public void EachDeleteIoBoundaryPreservesRemainderAndRequiresFreshObservation(int index, bool after)
        {
            using (var r = new PendingRig())
            {
                var head = r.Commit(null, "head").CurrentHead;
                var ticket = r.Leave(head, "candidate", "Marker.Write.partial");
                r.Put(PendingRig.Work(ticket), PendingRig.Bytes(ticket.Envelope));
                r.Reopen();
                var view = r.View();
                var before = r.Disk();
                var names = PendingRig.Targets(ticket);
                var fired = false;
                r.Storage.Hook = point =>
                {
                    if (!fired && point == "Delete." + (after ? "after." : "before.") + names[index])
                    {
                        fired = true;
                        throw new IOException("delete-boundary");
                    }
                };
                var failed = r.Store.EndObservedCandidate(view, ticket.Metadata.CommitId, B());
                Bad(failed, "EndInterrupted");
                Assert.IsTrue(fired);
                Assert.AreEqual("StorageFailure", failed.Diagnostic.Code);
                Assert.AreEqual(typeof(IOException).FullName, failed.Diagnostic.ExceptionType);
                Assert.AreEqual("EndCandidate." + names[index] + ".Delete", failed.Diagnostic.Stage);
                var removed = index + (after ? 1 : 0);
                r.SameDisk(before, names.Take(removed));
                Assert.AreEqual(removed, r.Storage.Deletes);
                r.Storage.Hook = null;
                if (removed != 0) Bad(r.Store.EndObservedCandidate(view, ticket.Metadata.CommitId, B()), "StaleContext");
                var fresh = r.View();
                if (removed == 3)
                    Bad(r.Store.EndObservedCandidate(fresh, ticket.Metadata.CommitId, B()), "CandidateNotFound");
                else
                    CollectionAssert.AreEqual(names.Skip(removed),
                        Ok(r.Store.EndObservedCandidate(fresh, ticket.Metadata.CommitId, B()), "Ended").RemovedNames);
                SameDescriptor(head, r.View().Current.Descriptor);
                Assert.AreEqual(SaveHeadStatus.Ready, r.View().Status);
                r.SameDisk(before, names);
            }
        }

        [TestCase("target")]
        [TestCase("other")]
        [TestCase("formal-marker")]
        public void EvidenceChangesBetweenDeletesStopBeforeTheNextDeletion(string change)
        {
            using (var r = new PendingRig())
            {
                var ticket = r.Leave(null, "candidate", "Marker.Write.partial");
                r.Put(PendingRig.Work(ticket), PendingRig.Bytes(ticket.Envelope));
                r.Reopen();
                var view = r.View();
                var names = PendingRig.Targets(ticket);
                r.Storage.Hook = point =>
                {
                    if (point != "Delete.after." + names[0]) return;
                    if (change == "target") r.Put(names[1], new byte[] { 2 });
                    if (change == "other") r.Put("w-" + Guid.NewGuid().ToString("N") + ".snapshot.tmp", new byte[] { 3 });
                    if (change == "formal-marker") r.Put(MarkerName(ticket.Metadata.CommitId), new byte[] { 4 });
                };
                Bad(r.Store.EndObservedCandidate(view, ticket.Metadata.CommitId, B()), "StaleContext");
                Assert.AreEqual(1, r.Storage.Deletes);
                Assert.IsTrue(File.Exists(r.Path(names[1])));
                Assert.IsTrue(File.Exists(r.Path(names[2])));
                r.Storage.Hook = null;
                Bad(r.Store.EndObservedCandidate(view, ticket.Metadata.CommitId, B()), "StaleContext");
                if (change == "formal-marker")
                    Bad(r.Store.EndObservedCandidate(r.View(), ticket.Metadata.CommitId, B()), "RecoveryBlocked");
                else Ok(r.Store.EndObservedCandidate(r.View(), ticket.Metadata.CommitId, B()), "Ended");
            }
        }

        [TestCase("Open")]
        [TestCase("Read")]
        [TestCase("Close")]
        public void EndRetainsReadIoStageAndNeverDeletesUnreadableEvidence(string point)
        {
            using (var r = new PendingRig())
            {
                var ticket = r.Leave(null, "candidate");
                r.Reopen();
                var view = r.View();
                var before = r.Disk();
                r.Storage.Hook = p => { if (p == point + "." + PendingRig.Work(ticket)) throw new IOException("read-boundary"); };
                var result = r.Store.EndObservedCandidate(view, ticket.Metadata.CommitId, B());
                Bad(result, "EndInterrupted");
                Assert.AreEqual("StorageFailure", result.Diagnostic.Code);
                StringAssert.Contains(PendingRig.Work(ticket), result.Diagnostic.Stage);
                StringAssert.EndsWith("." + point, result.Diagnostic.Stage);
                Assert.AreEqual(0, r.Storage.Deletes);
                r.Storage.Hook = null;
                r.SameDisk(before);
                Ok(r.Store.EndObservedCandidate(r.View(), ticket.Metadata.CommitId, B()), "Ended");
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void CompleteCandidateStreamCloseFailureLeavesNoTicket(bool resume)
        {
            using (var r = new PendingRig())
            {
                var ticket = r.Leave(null, "candidate");
                r.Reopen();
                var view = r.View();
                var candidate = Ok(r.Store.ReadUncommittedCandidate(view, ticket.Metadata.CommitId, B()), "CandidateRead");
                var closes = 0;
                var before = r.Disk();
                r.Storage.Hook = point =>
                {
                    if (point == "Close." + PendingRig.Work(ticket) && ++closes == 3)
                        throw new IOException("candidate-close");
                };
                LocalSaveDiagnostic diagnostic;
                if (resume)
                {
                    var result = r.Store.ResumeRecoveredCandidate(candidate, r.Caps(view), B());
                    Bad(result, "StorageFailure");
                    diagnostic = result.Diagnostic;
                }
                else
                {
                    var result = r.Store.ReadUncommittedCandidate(view, ticket.Metadata.CommitId, B());
                    Bad(result, "StorageFailure");
                    diagnostic = result.Diagnostic;
                }
                Assert.AreEqual("Candidate.Read." + PendingRig.Work(ticket) + ".Close", diagnostic.Stage);
                r.Storage.Hook = null;
                r.SameDisk(before);
                Assert.IsFalse(r.View().RetainedRoots.Any(x => x.SourceName == "pending-ticket"));
            }
        }

        [TestCase("Snapshot.Flush.after", "SaveFailed")]
        [TestCase("Marker.Promote.after", "CommitUnknown")]
        public void ResumedWriteRetainsOriginalFailureAndResolutionSemantics(string fault, string code)
        {
            using (var r = new PendingRig())
            {
                var original = r.Leave(null, "candidate");
                r.Reopen();
                var view = r.View();
                var candidate = Ok(r.Store.ReadUncommittedCandidate(view, original.Metadata.CommitId, B()), "CandidateRead");
                var resumed = Ok(r.Store.ResumeRecoveredCandidate(candidate, r.Caps(view), B()), "Resumed");
                r.Storage.Base.Arm(fault, resumed.Metadata.CommitId);
                Bad(r.Store.Write(resumed, B()), code);
                Assert.IsTrue(r.Storage.Base.FaultUsed);
                var resolved = Ok(r.Store.Write(resumed, B()), "Committed");
                SameDescriptor(candidate.Descriptor, resolved.Descriptor);
                Assert.AreEqual(1, r.Storage.Base.RealMarkerPromotionsFor(original.Metadata.CommitId));
                Ok(r.Store.Lookup(original.Metadata.CommitId, "candidate", B()), "Committed");
            }
        }

        [Test]
        public void OldPendingEndAndPrepareRemainUsableAndReentrantCallsAreBusy()
        {
            using (var r = new PendingRig())
            {
                var original = r.Leave(null, "candidate");
                var view = r.View();
                Bad(r.Store.ReadUncommittedCandidate(view, original.Metadata.CommitId, B()), "Busy");
                Bad(r.Store.EndObservedCandidate(view, original.Metadata.CommitId, B()), "Busy");
                Ok(r.Store.EndUncommitted(original, B()), "Ended");
                var next = r.Leave(null, "new");
                r.Reopen();
                view = r.View();
                var candidate = Ok(r.Store.ReadUncommittedCandidate(view, next.Metadata.CommitId, B()), "CandidateRead");
                var calls = 0;
                r.Storage.Hook = point =>
                {
                    if (!point.StartsWith("Open.", StringComparison.Ordinal) || calls++ != 0) return;
                    Bad(r.Store.ReadUncommittedCandidate(view, next.Metadata.CommitId, B()), "Busy");
                    Bad(r.Store.EndObservedCandidate(view, next.Metadata.CommitId, B()), "Busy");
                    Bad(r.Store.ResumeRecoveredCandidate(candidate, r.Caps(view), B()), "Busy");
                };
                Ok(r.Store.ReadUncommittedCandidate(view, next.Metadata.CommitId, B()), "CandidateRead");
                Assert.Greater(calls, 0);
                r.Storage.Hook = null;
                Ok(r.Store.EndObservedCandidate(view, next.Metadata.CommitId, B()), "Ended");
                Assert.AreEqual(SaveHeadStatus.NoSave, r.View().Status);
                Bad(r.Store.Prepare(null, new[] { "explicit-new" }, m => Empty(m, B().Codec), B()), "InitializationRequired");
                r.Reopen(SaveOpenMode.CreateNew);
                r.Commit(null, "explicit-new");
            }
        }

        [Test]
        public void NullInvalidPurposeMissingAndDisposedFollowThePublicContract()
        {
            using (var r = new PendingRig())
            {
                var ticket = r.Leave(null, "candidate");
                r.Reopen();
                var view = r.View();
                var candidate = Ok(r.Store.ReadUncommittedCandidate(view, ticket.Metadata.CommitId, B()), "CandidateRead");
                var caps = r.Caps(view);
                Assert.Throws<ArgumentNullException>(() => r.Store.ReadUncommittedCandidate(null, ticket.Metadata.CommitId, B()));
                Assert.Throws<ArgumentNullException>(() => r.Store.ReadUncommittedCandidate(view, null, B()));
                Assert.Throws<ArgumentNullException>(() => r.Store.ReadUncommittedCandidate(view, ticket.Metadata.CommitId, null));
                Assert.Throws<ArgumentNullException>(() => r.Store.ResumeRecoveredCandidate(null, caps, B()));
                Assert.Throws<ArgumentNullException>(() => r.Store.ResumeRecoveredCandidate(candidate, null, B()));
                Assert.Throws<ArgumentNullException>(() => r.Store.ResumeRecoveredCandidate(candidate, caps, null));
                Assert.Throws<ArgumentNullException>(() => r.Store.EndObservedCandidate(null, ticket.Metadata.CommitId, B()));
                Assert.Throws<ArgumentNullException>(() => r.Store.EndObservedCandidate(view, null, B()));
                Assert.Throws<ArgumentNullException>(() => r.Store.EndObservedCandidate(view, ticket.Metadata.CommitId, null));
                foreach (var id in new[] { "", " ", "not-guid", ticket.Metadata.CommitId.ToUpperInvariant() })
                {
                    Bad(r.Store.ReadUncommittedCandidate(view, id, B()), "InvalidValue");
                    Bad(r.Store.EndObservedCandidate(view, id, B()), "InvalidValue");
                }
                var absent = Guid.NewGuid().ToString("N");
                Bad(r.Store.ReadUncommittedCandidate(view, absent, B()), "CandidateNotFound");
                Bad(r.Store.EndObservedCandidate(view, absent, B()), "CandidateNotFound");
                using (var player = new PendingRig("player", purpose: SavePurpose.PlayerSave))
                {
                    var pv = player.View();
                    Bad(player.Store.ReadUncommittedCandidate(pv, absent, B()), "UnsupportedBinding");
                    Bad(player.Store.ResumeRecoveredCandidate(candidate, caps, B()), "UnsupportedBinding");
                    Bad(player.Store.EndObservedCandidate(pv, absent, B()), "UnsupportedBinding");
                }
                r.Store.Dispose();
                Bad(r.Store.ReadUncommittedCandidate(view, absent, B()), "Disposed");
                Bad(r.Store.ResumeRecoveredCandidate(candidate, caps, B()), "Disposed");
                Bad(r.Store.EndObservedCandidate(view, absent, B()), "Disposed");
            }
        }

        private static void AssertFrozen(SaveRecoveredCandidate candidate)
        {
            Assert.Throws<NotSupportedException>(() => ((IList<string>)candidate.OperationIds).Add("bad"));
            Assert.Throws<NotSupportedException>(() => ((IList<byte>)candidate.Descriptor.Sha256)[0] = 0);
            Assert.Throws<NotSupportedException>(() => ((IList<SaveCommitIndexEntry>)candidate.Metadata.CommitIndex).Clear());
            Assert.Throws<NotSupportedException>(() => ((IList<string>)candidate.Metadata.CommitIndex.Last().OperationIds).Clear());
            Assert.Throws<NotSupportedException>(() => ((IList<byte>)candidate.Envelope.SliceBytes[0])[0] = 0);
            if (candidate.ExpectedHead != null)
                Assert.Throws<NotSupportedException>(() => ((IList<byte>)candidate.ExpectedHead.Sha256)[0] = 0);
        }
    }

    internal sealed class PendingRig : IDisposable
    {
        internal readonly PendingFaultStorage Storage;
        internal LocalSaveStore Store;
        internal int Builds;

        internal PendingRig(string player = "pending-player", string root = null,
            SavePurpose purpose = SavePurpose.CandidateValidation, SaveOpenMode mode = SaveOpenMode.CreateNew)
        {
            root = root ?? SavePendingRecoveryProcessCases.TestRoot();
            Storage = new PendingFaultStorage(new WindowsEditorSaveStorage(root, player, purpose));
            Store = Open(Storage, mode);
        }
        internal string Path(string name) { return System.IO.Path.Combine(Storage.Profile.DirectoryPath, name); }
        internal void Put(string name, byte[] bytes) { File.WriteAllBytes(Path(name), bytes); }
        internal SaveRecoveryView View() { return Ok(Store.ReadRecovery(B()), "RecoveryObserved"); }
        internal SaveRecoveryCapabilities Caps(SaveRecoveryView view) { return SaveRecoveryProcessCases.Capabilities(view); }
        internal void Reopen(SaveOpenMode mode = SaveOpenMode.Existing) { Store.Dispose(); Store = Open(Storage, mode); }
        internal SaveCommitTicket Prepare(SnapshotDescriptor head, string operation)
        {
            var budget = B();
            return Ok(Store.Prepare(head, new[] { operation }, m => { Builds++; return Raw(m, operation, budget.Codec); }, budget), "Prepared");
        }
        internal SaveCommittedReference Commit(SnapshotDescriptor head, string operation)
        { return Ok(Store.Write(Prepare(head, operation), B()), "Committed"); }
        internal SaveCommitTicket Leave(SnapshotDescriptor head, string operation, string fault = "Snapshot.Flush.after")
        {
            var ticket = Prepare(head, operation);
            FailWrite(ticket, fault);
            return ticket;
        }
        internal void FailWrite(SaveCommitTicket ticket, string fault)
        {
            Storage.Base.Arm(fault, ticket.Metadata.CommitId);
            Bad(Store.Write(ticket, B()), fault == "Marker.Promote.after" ? "CommitUnknown" : "SaveFailed");
            Assert.IsTrue(Storage.Base.FaultUsed);
        }
        internal SaveCommitTicket Application(out PreparedCandidateApplicationIntent intent, bool corrupt = false)
        {
            var domain = BusinessSaveScenario.New();
            var input = new CandidateApplicationIntentInput
            {
                PlayerId = domain.Character.PlayerId, OperationId = "init", Kind = CandidateApplicationKind.InitializeProfile,
                Context = BusinessFields.ContextInput(domain.Character.Definition.Context),
                InitializeProfile = new CandidateApplicationInitializeInput
                {
                    CharacterId = domain.Character.CharacterId, ClassId = domain.Character.ClassId,
                    InitialLevel = domain.Character.Level, InitialExperience = domain.Character.Experience,
                    OriginalSlot = domain.Character.OriginalSlot
                }
            };
            intent = BusinessSaveScenario.Accept(CandidateApplicationProtocol.PrepareIntent(input, B().Codec));
            var candidate = BusinessSaveScenario.Accept(CandidateApplicationProtocol.Propose(null, BusinessSaveScenario.Prepare(domain),
                intent, new CandidateApplicationResultInput(), null, B().Codec));
            var budget = B();
            return Ok(Store.Prepare(null, new[] { "init" }, m =>
            {
                Builds++;
                var encoded = CandidateApplicationSaveCodec.Encode(candidate,
                    new CandidateBusinessSaveHeader(m.SaveGeneration, m.CommitId, m.ParentCommitId, m.CommitIndex), budget.Codec);
                if (!corrupt) return encoded;
                var slices = BusinessSaveScenario.Slices(BusinessSaveScenario.Accept(encoded));
                slices[0].Bytes = new byte[] { 1 };
                var inputEnvelope = EmptyInput(m);
                inputEnvelope.RequiredSliceContracts = slices.Select(x => x.Contract).ToArray();
                inputEnvelope.Slices = slices;
                return SaveEnvelopeCodec.Prepare(inputEnvelope, budget.Codec);
            }, budget), "Prepared");
        }
        internal Dictionary<string, byte[]> Disk()
        {
            return Directory.GetFiles(Storage.Profile.DirectoryPath).Where(x => System.IO.Path.GetFileName(x) != "writer.lock")
                .ToDictionary(System.IO.Path.GetFileName, File.ReadAllBytes, StringComparer.Ordinal);
        }
        internal void SameDisk(Dictionary<string, byte[]> before, IEnumerable<string> removed = null)
        {
            var gone = new HashSet<string>(removed ?? Array.Empty<string>(), StringComparer.Ordinal);
            var after = Disk();
            CollectionAssert.AreEquivalent(before.Keys.Where(x => !gone.Contains(x)), after.Keys);
            foreach (var pair in before.Where(x => !gone.Contains(x.Key))) CollectionAssert.AreEqual(pair.Value, after[pair.Key], pair.Key);
        }
        internal static string Work(SaveCommitTicket t) { return "w-" + t.Metadata.CommitId + ".snapshot.tmp"; }
        internal static string MarkerWork(SaveCommitTicket t) { return "w-" + t.Metadata.CommitId + ".commit.tmp"; }
        internal static string[] Targets(SaveCommitTicket t) { return new[] { MarkerWork(t), Work(t), SnapshotName(t.Metadata.CommitId) }; }
        internal static byte[] Bytes(SaveEnvelope envelope)
        {
            using (var stream = new MemoryStream())
            {
                BusinessSaveScenario.Accept(SaveEnvelopeCodec.Write(stream, envelope, B().Codec));
                return stream.ToArray();
            }
        }
        internal static SnapshotDescriptor Descriptor(SaveCommitTicket ticket)
        {
            return BusinessSaveScenario.Accept(SaveEnvelopeCodec.Write(Stream.Null, ticket.Envelope, B().Codec));
        }
        internal static SaveCodecResult<SaveEnvelope> Raw(SaveCommitMetadata m, string tag, SaveCodecBudget budget)
        {
            var contract = new RequiredSliceContract("slice:" + tag, "owner", 1);
            var binding = new SaveBinding(SaveBindingKind.CandidateContent, null, "draft:" + tag, 1, "fingerprint",
                "r:" + tag, "n:" + tag, "q:" + tag, new[] { "first", "second" }, null, null);
            var input = EmptyInput(m);
            input.RequiredSliceContracts = new[] { contract };
            input.Slices = new[] { new SaveSliceInput
            {
                Contract = contract, Bytes = new byte[] { 5, 7, 9 },
                Requirements = new SaveRequirements(new[] { binding }, new[] { "r:" + tag }, new[] { "n:" + tag },
                    new[] { "q:" + tag }, new[] { "f:" + tag })
            } };
            return SaveEnvelopeCodec.Prepare(input, budget);
        }
        public void Dispose() { Store.Dispose(); }
    }

    internal sealed class PendingFaultStorage : ILocalSaveStorage
    {
        internal readonly FaultStorage Base;
        internal Action<string> Hook;
        internal int Deletes;
        internal PendingFaultStorage(WindowsEditorSaveStorage real) { Base = new FaultStorage(real); }
        public SaveStorageProfile Profile => Base.Profile;
        public IDisposable AcquireWriterLease(bool createDirectory) { return Base.AcquireWriterLease(createDirectory); }
        public IEnumerable<string> EnumerateNames() { Hook?.Invoke("Enumerate"); return Base.EnumerateNames(); }
        public Stream OpenRead(string name)
        {
            Hook?.Invoke("Open." + name);
            return new TrackingStream(Base.OpenRead(name), point => Hook?.Invoke(point + "." + name));
        }
        public Stream CreateWork(string name) { return Base.CreateWork(name); }
        public void FlushFile(Stream stream) { Base.FlushFile(stream); }
        public void PromoteNoReplace(string work, string final) { Base.PromoteNoReplace(work, final); }
        public void DeleteUncommitted(string name)
        {
            Hook?.Invoke("Delete.before." + name);
            Base.DeleteUncommitted(name);
            Deletes++;
            Hook?.Invoke("Delete.after." + name);
        }
        public void DeleteIndexedOld(string name) { Base.DeleteIndexedOld(name); }
    }
}
