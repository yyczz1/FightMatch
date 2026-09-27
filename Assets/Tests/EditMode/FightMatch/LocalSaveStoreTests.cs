using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using FightMatch.Platform;
using NUnit.Framework;
using static FightMatch.Core.Tests.LocalSaveTestFiles;

namespace FightMatch.Core.Tests
{
    [TestFixture]
    public class LocalSaveStoreTests
    {
        [Test]
        public void ThreeRealBusinessGenerationsRoundTripAndOldKeysUseTheLatestIndexAfterReopen()
        {
            using (var rig = new SaveRig("player:015b"))
            {
                var scenario = BusinessSaveScenario.New(3); scenario.Begin();
                var tickets = new List<SaveCommitTicket>(); var results = new List<SaveCommittedReference>();
                SnapshotDescriptor head = null;
                for (var generation = 1; generation <= 3; generation++)
                {
                    if (generation == 2) scenario.Attack("attack", 0);
                    if (generation == 3) scenario.Rollback("rollback", "attack");
                    var frozen = BusinessSaveScenario.Prepare(scenario); var budget = B(); var callbacks = 0;
                    var ticket = Ok(rig.Store.Prepare(head, new[] { "save:" + generation }, metadata =>
                    {
                        callbacks++;
                        return CandidateBusinessSaveCodec.Encode(frozen, new CandidateBusinessSaveHeader(metadata.SaveGeneration,
                            metadata.CommitId, metadata.ParentCommitId, metadata.CommitIndex), budget.Codec);
                    }, budget), "Prepared");
                    tickets.Add(ticket); var committed = Ok(rig.Store.Write(ticket, B()), "Committed"); results.Add(committed); head = committed.CurrentHead;
                    Assert.AreEqual(1, callbacks); Assert.AreEqual(new BigInteger(generation), head.SaveGeneration);
                    Assert.AreEqual(SaveFaultModel.EditorProcessCrash, committed.FaultModel);
                    var decoded = ReadBusiness(rig.Storage, head);
                    BusinessSaveScenario.Same(scenario.History.CurrentRun.CurrentSnapshot, decoded.ActiveHistory.CurrentRun.CurrentSnapshot);
                    var reencoded = BusinessSaveScenario.Accept(CandidateBusinessSaveCodec.Encode(decoded, new CandidateBusinessSaveHeader(
                        ticket.Metadata.SaveGeneration, ticket.Metadata.CommitId, ticket.Metadata.ParentCommitId, ticket.Metadata.CommitIndex), B().Codec));
                    for (var slice = 0; slice < 5; slice++) CollectionAssert.AreEqual(ticket.Envelope.SliceBytes[slice], reencoded.SliceBytes[slice]);
                    Assert.IsNull(ticket.Metadata.CommitIndex.Last().SnapshotLength);
                    Assert.AreEqual(5, ticket.Envelope.SliceDirectory.Count);
                }
                var writes = rig.Storage.WriteCalls;
                var repeated = Ok(rig.Store.Write(tickets[0], B()), "Committed");
                SameDescriptor(results[0].Descriptor, repeated.Descriptor); SameDescriptor(head, repeated.CurrentHead);
                Assert.AreEqual(writes, rig.Storage.WriteCalls); Bad(rig.Store.Lookup(tickets[0].Metadata.CommitId, "save:3", B()), "KeyMismatch");
                rig.Store.Dispose();
                rig.Delete(SnapshotName(tickets[0].Metadata.CommitId)); rig.Delete(MarkerName(tickets[0].Metadata.CommitId));
                rig.Store = Open(rig.Storage, SaveOpenMode.Existing);
                var original = Ok(rig.Store.Lookup(tickets[0].Metadata.CommitId, "save:1", B()), "Committed");
                SameDescriptor(results[0].Descriptor, original.Descriptor); SameDescriptor(head, original.CurrentHead);
                Assert.AreEqual(new BigInteger(3), original.CurrentHead.SaveGeneration);
                Assert.AreEqual(SaveHeadStatus.Ready, Ok(rig.Store.Inspect(B()), "Inspected").Status);
                var b = B(); Bad(rig.Store.Prepare(head, new[] { "save:1" }, m => Empty(m, b.Codec), b), "OperationAlreadyIndexed");
                Assert.AreEqual(6, rig.Storage.SnapshotAndMarkerPromotions);
                Assert.Greater(rig.Storage.RealFlushes, 0);
            }
        }

        [Test]
        public void TicketCopiesMutableInputsAndFreezesEveryMetadataList()
        {
            using (var rig = new SaveRig())
            {
                var first = rig.Commit(null, "first"); var digest = first.Descriptor.Sha256.ToArray();
                var supplied = Copy(first.Descriptor, digest); var operations = new List<string> { "second" };
                var b = B(); SaveCommitMetadata delivered = null;
                var ticket = Ok(rig.Store.Prepare(supplied, operations, m =>
                {
                    delivered = m;
                    Assert.Throws<NotSupportedException>(() => ((IList<string>)m.CommitIndex.Last().OperationIds).Clear());
                    Assert.Throws<NotSupportedException>(() => ((IList<byte>)m.CommitIndex[0].SnapshotSha256).Clear());
                    Assert.Throws<NotSupportedException>(() => ((IList<SaveCommitIndexEntry>)m.CommitIndex).Clear());
                    return Empty(m, b.Codec);
                }, b), "Prepared");
                operations[0] = "changed"; digest[0] ^= 1;
                SameDescriptor(first.Descriptor, ticket.ExpectedHead); CollectionAssert.AreEqual(new[] { "second" }, ticket.OperationIds);
                Assert.AreSame(delivered, ticket.Metadata);
                Ok(rig.Store.Write(ticket, B()), "Committed"); Bad(rig.Store.Lookup(null, "changed", B()), "ConfirmedNotCommitted");
                Ok(rig.Store.Lookup(null, "second", B()), "Committed");
            }
        }

        [TestCase("player")] [TestCase("purpose")] [TestCase("generation")] [TestCase("commit")]
        [TestCase("parent")] [TestCase("index-operation")] [TestCase("index-length")] [TestCase("index-sha")]
        public void ChangedCallbackMetadataCannotAdmitATicketOrWriteCandidateFiles(string mutation)
        {
            using (var rig = new SaveRig())
            {
                var first = rig.Commit(null, "first"); var before = Directory.GetFiles(rig.Storage.Profile.DirectoryPath).OrderBy(x => x).ToArray();
                var calls = 0; var b = B();
                var result = rig.Store.Prepare(first.CurrentHead, new[] { "second" }, m =>
                {
                    calls++; var input = EmptyInput(m); var rows = m.CommitIndex.ToArray();
                    if (mutation == "player") input.PlayerId = "other";
                    if (mutation == "purpose") input.Purpose = SavePurpose.PlayerSave;
                    if (mutation == "commit") { input.CommitId = Guid.NewGuid().ToString("N"); rows[1] = Row(rows[1], commit: input.CommitId); }
                    if (mutation == "generation")
                    {
                        input.SaveGeneration = 1; input.ParentCommitId = null;
                        rows = new[] { new SaveCommitIndexEntry(1, m.CommitId, null, null, null, new[] { "second" }) };
                    }
                    if (mutation == "parent")
                    {
                        var parent = Guid.NewGuid().ToString("N"); input.ParentCommitId = parent; rows[0] = Row(rows[0], commit: parent);
                        rows[1] = new SaveCommitIndexEntry(2, m.CommitId, parent, null, null, new[] { "second" });
                    }
                    if (mutation == "index-operation") rows[1] = Row(rows[1], operations: new[] { "different" });
                    if (mutation == "index-length") rows[0] = Row(rows[0], length: rows[0].SnapshotLength.Value + 1);
                    if (mutation == "index-sha") { var sha = rows[0].SnapshotSha256.ToArray(); sha[0] ^= 1; rows[0] = Row(rows[0], sha: sha); }
                    input.CommitIndex = rows;
                    var encoded = SaveEnvelopeCodec.Prepare(input, b.Codec); Assert.IsTrue(encoded.IsAccepted, encoded.FieldPath); return encoded;
                }, b);
                Bad(result, "InconsistentBinding"); Assert.AreEqual(1, calls); Assert.IsFalse(rig.Store.HasPendingTicket);
                CollectionAssert.AreEqual(before, Directory.GetFiles(rig.Storage.Profile.DirectoryPath).OrderBy(x => x));
                rig.Commit(first.CurrentHead, "valid");
            }
        }

        [Test]
        public void InitializationKeysExpectedHeadAndTicketOwnershipHaveSeparateGates()
        {
            using (var rig = new SaveRig())
            using (var other = new SaveRig())
            {
                var ticket = rig.Prepare(null, new string[0]); Assert.IsEmpty(ticket.OperationIds);
                var foreignIo = other.Storage.Calls;
                Bad(other.Store.Write(ticket, B()), "TicketOwnerMismatch"); Bad(other.Store.EndUncommitted(ticket, B()), "TicketOwnerMismatch");
                Assert.AreEqual(foreignIo, other.Storage.Calls);
                Bad(rig.Store.Prepare(null, new[] { "x" }, m => Empty(m, B().Codec), B()), "Busy");
                var committed = Ok(rig.Store.Write(ticket, B()), "Committed");
                Bad(rig.Store.Prepare(null, new[] { "next" }, m => Empty(m, B().Codec), B()), "StaleContext");
                Bad(rig.Store.Prepare(committed.CurrentHead, new[] { "same", "same" }, m => Empty(m, B().Codec), B()), "InvalidValue");
                Bad(rig.Store.Prepare(committed.CurrentHead, new[] { " " }, m => Empty(m, B().Codec), B()), "InvalidValue");
                Bad(rig.Store.Lookup(null, null, B()), "MissingField"); Bad(rig.Store.Lookup("bad", null, B()), "InvalidValue");
                Bad(rig.Store.Lookup(ticket.Metadata.CommitId, "missing", B()), "KeyMismatch");
                Bad(rig.Store.Lookup(Guid.NewGuid().ToString("N"), "missing", B()), "ConfirmedNotCommitted");
                Bad(rig.Store.EndUncommitted(ticket, B()), "AlreadyCommitted");
                rig.Store.Dispose(); Bad(rig.Store.Write(ticket, B()), "Disposed");
            }
            var root = NewCase(); var storage = new WindowsEditorSaveStorage(root, "p", SavePurpose.CandidateValidation);
            Bad(LocalSaveStore.Open(storage, "p", SavePurpose.CandidateValidation, SaveOpenMode.Existing, SaveFaultModel.EditorProcessCrash, B()), "NoSave");
            Assert.IsFalse(Directory.Exists(storage.Profile.DirectoryPath));
            Directory.CreateDirectory(storage.Profile.DirectoryPath);
            using (var existing = Open(storage, SaveOpenMode.Existing))
            {
                Assert.AreEqual(SaveHeadStatus.NoSave, existing.InitialInspection.Value.Status);
                Bad(existing.Prepare(null, new string[0], m => Empty(m, B().Codec), B()), "InitializationRequired");
            }
        }

        [Test]
        public void LeaseIsARealExclusiveHandleAndCapabilitiesAreCheckedBeforeWriting()
        {
            var root = NewCase(); var storage = new FaultStorage(new WindowsEditorSaveStorage(root, "p", SavePurpose.CandidateValidation));
            Bad(LocalSaveStore.Open(storage, "p", SavePurpose.CandidateValidation, SaveOpenMode.CreateNew, SaveFaultModel.PowerLossDurable, B()), "StorageCapabilityUnavailable");
            Assert.AreEqual(0, storage.Calls); Assert.IsFalse(Directory.Exists(storage.Profile.DirectoryPath));
            Bad(LocalSaveStore.Open(storage, "other", SavePurpose.CandidateValidation, SaveOpenMode.CreateNew, SaveFaultModel.EditorProcessCrash, B()), "InconsistentBinding");
            using (var first = Open(storage))
            {
                var other = new WindowsEditorSaveStorage(root, "p", SavePurpose.CandidateValidation);
                Bad(LocalSaveStore.Open(other, "p", SavePurpose.CandidateValidation, SaveOpenMode.Existing, SaveFaultModel.EditorProcessCrash, B()), "Busy");
                Assert.IsTrue(File.Exists(Path.Combine(storage.Profile.DirectoryPath, "writer.lock")));
            }
            using (var reopened = Open(storage, SaveOpenMode.Existing)) Assert.AreEqual(SaveHeadStatus.NoSave, reopened.InitialInspection.Value.Status);
        }

        [Test]
        public void CallbackReentryConcurrencyExceptionAndCodecRejectionReleaseOnlyPreparingState()
        {
            using (var rig = new SaveRig())
            {
                var b = B();
                var rejected = rig.Store.Prepare(null, new[] { "a" }, m =>
                {
                    Bad(rig.Store.Inspect(B()), "Busy"); Bad(rig.Store.Lookup(m.CommitId, null, B()), "Busy");
                    var parallel = Task.Run(() => rig.Store.Prepare(null, new[] { "parallel" }, x => Empty(x, B().Codec), B()));
                    Assert.IsTrue(parallel.Wait(5000), "Concurrent entry must not wait on the encoding callback.");
                    Bad(parallel.Result, "Busy"); throw new InvalidOperationException("encoding stopped");
                }, b);
                Bad(rejected, "CallbackFailed"); Assert.AreEqual(typeof(InvalidOperationException).FullName, rejected.Diagnostic.ExceptionType);
                Assert.AreEqual("encoding stopped", rejected.Diagnostic.ExceptionMessage); Assert.IsFalse(rig.Store.HasPendingTicket);
                var tiny = new SaveStoreBudget(new SaveCodecBudget(new ExactMathBudget(), maxEnvelopeBytes: 1));
                Bad(rig.Store.Prepare(null, new[] { "codec" }, m => Empty(m, tiny.Codec), tiny), "Limit");
                Assert.IsFalse(rig.Store.HasPendingTicket); rig.Commit(null, "valid");
            }
        }

        internal static IEnumerable<TestCaseData> FaultCases()
        {
            foreach (var owner in new[] { "Snapshot", "Marker" })
            {
                foreach (var operation in new[] { "Create", "Write", "Flush", "Close", "Verify.Open", "Verify.Read", "Verify.Close", "Promote" })
                    foreach (var timing in new[] { "before", "after" }) yield return new TestCaseData(owner + "." + operation + "." + timing, false);
                yield return new TestCaseData(owner + ".Write.partial", false);
                yield return new TestCaseData(owner + ".Write.short", false);
            }
            foreach (var operation in new[] { "Enumerate", "EnumerateItem", "Marker.Open", "Marker.Read", "Marker.Close", "Snapshot.Open", "Snapshot.Read", "Snapshot.Close" })
                foreach (var timing in new[] { "before", "after" }) yield return new TestCaseData("Final." + operation + "." + timing, false);
            yield return new TestCaseData("Snapshot.Write.partial", true);
            yield return new TestCaseData("Marker.Promote.before", true);
            yield return new TestCaseData("Marker.Promote.after", true);
        }

        [TestCaseSource(nameof(FaultCases))]
        public void RealFileFaultsKeepTheSameTicketBytesAndClassifyThePublicationBoundary(string point, bool firstGeneration)
        {
            using (var rig = new SaveRig("player:015b"))
            {
                SnapshotDescriptor head = firstGeneration ? null : rig.Commit(null, "old").CurrentHead;
                var scenario = BusinessSaveScenario.New(3); scenario.Begin(); scenario.Attack("attack", 0);
                var candidate = BusinessSaveScenario.Prepare(scenario); var b = B(); var calls = 0;
                var ticket = Ok(rig.Store.Prepare(head, new[] { "new" }, m => {
                    calls++; return CandidateBusinessSaveCodec.Encode(candidate,
                        new CandidateBusinessSaveHeader(m.SaveGeneration, m.CommitId, m.ParentCommitId, m.CommitIndex), b.Codec); }, b), "Prepared");
                var originalBytes = EnvelopeBytes(ticket); var id = ticket.Metadata.CommitId;
                rig.Storage.Arm(point, id);
                var failed = rig.Store.Write(ticket, B()); Assert.IsTrue(rig.Storage.FaultUsed, point);
                var publication = point.StartsWith("Final.", StringComparison.Ordinal) || point.StartsWith("Marker.Promote.", StringComparison.Ordinal);
                Bad(failed, publication ? "CommitUnknown" : "SaveFailed");
                if (!point.EndsWith(".short", StringComparison.Ordinal))
                { Assert.AreEqual(typeof(IOException).FullName, failed.Diagnostic.ExceptionType); StringAssert.Contains(point, failed.Diagnostic.ExceptionMessage); }
                Assert.IsTrue(rig.Store.HasPendingTicket);
                Bad(rig.Store.Prepare(head, new[] { "bypass" }, m => Empty(m, B().Codec), B()), "Busy");
                var found = rig.Store.Lookup(id, "new", B());
                var published = point.StartsWith("Final.", StringComparison.Ordinal) || point == "Marker.Promote.after";
                if (published) { Ok(found, "Committed"); Assert.IsFalse(rig.Store.HasPendingTicket); }
                else { Bad(found, "ConfirmedNotCommitted"); Assert.IsTrue(rig.Store.HasPendingTicket); }
                var completed = Ok(rig.Store.Write(ticket, B()), "Committed");
                Assert.AreEqual(id, completed.Descriptor.CommitId); Assert.AreEqual(1, calls);
                CollectionAssert.AreEqual(originalBytes, File.ReadAllBytes(rig.Path(SnapshotName(id))));
                var restored = ReadBusiness(rig.Storage, completed.Descriptor);
                BusinessSaveScenario.Same(candidate.ActiveHistory.CurrentRun.CurrentSnapshot, restored.ActiveHistory.CurrentRun.CurrentSnapshot);
                Assert.AreEqual(1, rig.Storage.RealMarkerPromotionsFor(id));
            }
        }

        [Test]
        public void FailureWithoutACompleteOldHeadIsBlockedAndEndingNeverDeletesAnotherCandidate()
        {
            using (var rig = new SaveRig())
            {
                var head = rig.Commit(null, "old").CurrentHead; var ticket = rig.Prepare(head, new[] { "new" });
                rig.Storage.Arm("Snapshot.Write.partial", ticket.Metadata.CommitId); rig.Storage.FailEnumerationAfterFault = true;
                Bad(rig.Store.Write(ticket, B()), "RecoveryBlocked");
                Bad(rig.Store.EndUncommitted(ticket, B()), "StorageFailure"); Assert.IsTrue(rig.Store.HasPendingTicket);
                rig.Storage.FailEnumerationAfterFault = false;
                var stranger = "w-" + Guid.NewGuid().ToString("N") + ".snapshot.tmp"; rig.Put(stranger, new byte[] { 8 });
                Bad(rig.Store.Write(ticket, B()), "Pending");
                rig.Storage.FailDeleteOnce = true; Bad(rig.Store.EndUncommitted(ticket, B()), "StorageFailure");
                Assert.IsTrue(rig.Store.HasPendingTicket); Assert.IsTrue(File.Exists(rig.Path(stranger)));
                Assert.IsTrue(Ok(rig.Store.EndUncommitted(ticket, B()), "Ended")); Assert.IsFalse(rig.Store.HasPendingTicket);
                Bad(rig.Store.Write(ticket, B()), "TicketEnded"); Assert.IsTrue(File.Exists(rig.Path(stranger)));
                Assert.IsTrue(File.Exists(rig.Path(MarkerName(head.CommitId))));
                Assert.AreEqual(SaveHeadStatus.Pending, Ok(rig.Store.Inspect(B()), "Inspected").Status);
                Bad(rig.Store.Prepare(head, new[] { "later" }, m => Empty(m, B().Codec), B()), "Pending");
            }
        }

        [TestCase("snapshot")] [TestCase("marker")] [TestCase("unknown")] [TestCase("directory")] [TestCase("fork")] [TestCase("higher")]
        public void DamagedOrUnattributedFinalEvidenceNeverSelectsAnOlderHead(string mutation)
        {
            using (var rig = new SaveRig())
            {
                var first = rig.Commit(null, "one"); var second = rig.Commit(first.CurrentHead, "two"); var third = rig.Commit(second.CurrentHead, "three");
                if (mutation == "snapshot") rig.Delete(SnapshotName(third.Descriptor.CommitId));
                if (mutation == "marker") rig.Put(MarkerName(third.Descriptor.CommitId), new byte[] { 1, 2, 3 });
                if (mutation == "unknown") rig.Put("c-" + Guid.NewGuid().ToString("N") + ".commit", new byte[] { 1 });
                if (mutation == "directory") Directory.CreateDirectory(rig.Path("unrecognized-directory"));
                if (mutation == "fork" || mutation == "higher")
                    using (var branch = new SaveRig())
                    {
                        SnapshotDescriptor head = null;
                        for (var i = 0; i < (mutation == "fork" ? 3 : 4); i++) head = branch.Commit(head, "branch:" + i).CurrentHead;
                        rig.Put(MarkerName(head.CommitId), File.ReadAllBytes(branch.Path(MarkerName(head.CommitId))));
                        rig.Put(SnapshotName(head.CommitId), File.ReadAllBytes(branch.Path(SnapshotName(head.CommitId))));
                    }
                var observed = Ok(rig.Store.Inspect(B()), "Inspected"); Assert.AreEqual(SaveHeadStatus.RecoveryBlocked, observed.Status); Assert.IsNull(observed.Current);
                Bad(rig.Store.Lookup(first.Descriptor.CommitId, null, B()), "RecoveryBlocked");
                rig.Store.Dispose(); rig.Store = Open(rig.Storage, SaveOpenMode.Existing);
                Assert.IsTrue(rig.Store.InitialInspection.IsAccepted); Assert.AreEqual(SaveHeadStatus.RecoveryBlocked, rig.Store.InitialInspection.Value.Status);
                Bad(rig.Store.Prepare(second.CurrentHead, new[] { "bad" }, m => Empty(m, B().Codec), B()), "RecoveryBlocked");
            }
        }

        [Test]
        public void IndexedOldCorruptionIsRedundancyLossButUnfinishedRootsStillCloseTheWriteGate()
        {
            using (var rig = new SaveRig())
            {
                var first = rig.Commit(null, "one"); var second = rig.Commit(first.CurrentHead, "two"); var third = rig.Commit(second.CurrentHead, "three");
                rig.Put(MarkerName(first.Descriptor.CommitId), SaveCommitMarkerTests.IndependentMarker(first.Descriptor, generationToken: "99"));
                rig.Put(SnapshotName(first.Descriptor.CommitId), new byte[] { 1 });
                var current = Ok(rig.Store.Inspect(B()), "Inspected");
                Assert.AreEqual(SaveHeadStatus.Ready, current.Status); SameDescriptor(third.CurrentHead, current.Current.Descriptor);
                Assert.IsTrue(current.Files.Any(x => x.CommitId == first.Descriptor.CommitId && x.Detail == "IndexedOldCopyDamaged"));
                Ok(rig.Store.Lookup(null, "one", B()), "Committed");
                var pendingName = "w-" + Guid.NewGuid().ToString("N") + ".snapshot.tmp"; rig.Put(pendingName, new byte[] { 3 });
                var pending = Ok(rig.Store.Inspect(B()), "Inspected");
                Assert.AreEqual(SaveHeadStatus.Pending, pending.Status); SameDescriptor(third.CurrentHead, pending.Current.Descriptor);
                Bad(rig.Store.Lookup(Guid.NewGuid().ToString("N"), null, B()), "ConfirmedNotCommitted");
                Bad(rig.Store.Prepare(third.CurrentHead, new[] { "new" }, m => Empty(m, B().Codec), B()), "Pending");
                rig.Store.Dispose(); rig.Store = Open(rig.Storage, SaveOpenMode.Existing);
                Assert.AreEqual(SaveHeadStatus.Pending, rig.Store.InitialInspection.Value.Status);
            }
        }

        [TestCase("directory")] [TestCase("marker")] [TestCase("math")] [TestCase("enumeration")]
        public void IncompleteObservationsRetainADiagnosticSessionAndLargerBudgetsCanRetry(string limit)
        {
            using (var rig = new SaveRig())
            {
                var head = rig.Commit(null, "one").CurrentHead; rig.Store.Dispose();
                var budget = limit == "directory" ? new SaveStoreBudget(B().Codec, maxDirectoryEntries: 1) :
                    limit == "marker" ? new SaveStoreBudget(B().Codec, maxMarkerBytes: 1) :
                    limit == "math" ? new SaveStoreBudget(new SaveCodecBudget(new ExactMathBudget(maxPrimitiveSteps: 0))) : B();
                if (limit == "enumeration") rig.Storage.FailEnumerationAlways = true;
                rig.Store = Ok(LocalSaveStore.Open(rig.Storage, "p", SavePurpose.CandidateValidation, SaveOpenMode.Existing, SaveFaultModel.EditorProcessCrash, budget), "Opened");
                Bad(rig.Store.InitialInspection, limit == "enumeration" ? "StorageFailure" : "Limit");
                Assert.IsNull(rig.Store.InitialInspection.Value);
                var again = limit == "math" ? new SaveStoreBudget(new SaveCodecBudget(new ExactMathBudget(maxPrimitiveSteps: 0))) : budget;
                Assert.IsFalse(rig.Store.Prepare(head, new[] { "new" }, m => Empty(m, B().Codec), again).IsAccepted);
                rig.Storage.FailEnumerationAlways = false;
                SameDescriptor(head, Ok(rig.Store.Lookup(null, "one", B()), "Committed").Descriptor);
                Assert.AreEqual(SaveHeadStatus.Ready, Ok(rig.Store.Inspect(B()), "Inspected").Status);
            }
        }

        [Test]
        public void CurrentHeadReadIoFailureKeepsTheDiagnosticSessionAndOriginalCause(
            [Values("Open", "Inspect", "Prepare")] string entry,
            [Values("Marker", "Snapshot")] string file,
            [Values("Open", "Read", "Close")] string operation,
            [Values("before", "after")] string timing)
        {
            using (var rig = new SaveRig())
            {
                var head = rig.Commit(null, "one").CurrentHead;
                var snapshot = File.ReadAllBytes(rig.Path(SnapshotName(head.CommitId)));
                var marker = File.ReadAllBytes(rig.Path(MarkerName(head.CommitId)));
                var names = Directory.GetFiles(rig.Storage.Profile.DirectoryPath).OrderBy(x => x).ToArray();
                var point = "Final." + file + "." + operation + "." + timing;
                var name = file == "Marker" ? MarkerName(head.CommitId) : SnapshotName(head.CommitId);
                var callbacks = 0; Action checkFailure;
                rig.Storage.ArmHeadRead(point, head.CommitId);
                if (entry == "Prepare")
                {
                    var budget = B();
                    var result = rig.Store.Prepare(head, new[] { "next" }, m => { callbacks++; return Empty(m, budget.Codec); }, budget);
                    checkFailure = () => HeadReadFailure(result, point, name, operation);
                }
                else
                {
                    if (entry == "Open") { rig.Store.Dispose(); rig.Store = Open(rig.Storage, SaveOpenMode.Existing); }
                    var result = entry == "Open" ? rig.Store.InitialInspection : rig.Store.Inspect(B());
                    checkFailure = () => HeadReadFailure(result, point, name, operation);
                }
                Assert.IsTrue(rig.Storage.FaultUsed, point); Assert.AreEqual(0, callbacks); Assert.IsFalse(rig.Store.HasPendingTicket);
                CollectionAssert.AreEqual(names, Directory.GetFiles(rig.Storage.Profile.DirectoryPath).OrderBy(x => x));
                CollectionAssert.AreEqual(snapshot, File.ReadAllBytes(rig.Path(SnapshotName(head.CommitId))));
                CollectionAssert.AreEqual(marker, File.ReadAllBytes(rig.Path(MarkerName(head.CommitId))));
                Bad(LocalSaveStore.Open(rig.Storage, "p", SavePurpose.CandidateValidation, SaveOpenMode.Existing, SaveFaultModel.EditorProcessCrash, B()), "Busy");
                SameDescriptor(head, Ok(rig.Store.Lookup(head.CommitId, "one", B()), "Committed").CurrentHead);
                var next = rig.Prepare(head, new[] { "next" }); Ok(rig.Store.EndUncommitted(next, B()), "Ended");
                // Check the disputed result last: the intact files, retained lease and recovery must already be proven.
                checkFailure();
            }
        }

        private static void HeadReadFailure<T>(LocalSaveResult<T> result, string point, string name, string operation)
        {
            var actual = "Accepted=" + result.IsAccepted + "; Code=" + result.Code + "; Value=" + result.Value +
                "; Type=" + result.Diagnostic?.ExceptionType + "; Message=" + result.Diagnostic?.ExceptionMessage + "; Stage=" + result.Diagnostic?.Stage;
            Assert.IsFalse(result.IsAccepted, actual);
            Assert.AreEqual(default(T), result.Value, actual);
            Assert.AreEqual("StorageFailure", result.Code, actual);
            Assert.AreEqual(typeof(IOException).FullName, result.Diagnostic?.ExceptionType, actual);
            Assert.AreEqual("Injected " + point, result.Diagnostic?.ExceptionMessage, actual);
            Assert.AreEqual("Inspect." + name + "." + operation, result.Diagnostic?.Stage, actual);
            Assert.AreEqual("Inspect." + name + "." + operation, result.FieldPath, actual);
        }

        [Test]
        public void IndexedOldCopyReadIoFailureDoesNotBlockTheCompleteCurrentHead(
            [Values("Marker", "Snapshot")] string file,
            [Values("Open", "Read", "Close")] string operation,
            [Values("before", "after")] string timing)
        {
            using (var rig = new SaveRig())
            {
                var first = rig.Commit(null, "one"); var head = rig.Commit(first.CurrentHead, "two").CurrentHead;
                if (file == "Snapshot") rig.Put(MarkerName(first.Descriptor.CommitId), SaveCommitMarkerTests.IndependentMarker(first.Descriptor, "99"));
                var point = "Final." + file + "." + operation + "." + timing;
                rig.Storage.ArmHeadRead(point, first.Descriptor.CommitId);
                var inspection = Ok(rig.Store.Inspect(B()), "Inspected");
                Assert.IsTrue(rig.Storage.FaultUsed, point); Assert.AreEqual(SaveHeadStatus.Ready, inspection.Status);
                SameDescriptor(head, inspection.Current.Descriptor);
                Assert.IsTrue(inspection.Diagnostics.Any(x => x.ExceptionType == typeof(IOException).FullName && x.ExceptionMessage == "Injected " + point));
                SameDescriptor(first.Descriptor, Ok(rig.Store.Lookup(first.Descriptor.CommitId, "one", B()), "Committed").Descriptor);
                var next = rig.Prepare(head, new[] { "next" }); Ok(rig.Store.EndUncommitted(next, B()), "Ended");
            }
        }

        [Test]
        public void LookupOnlyReleasesTheMatchingPendingTicketAndCommittedEvidenceCannotBeCancelled()
        {
            using (var rig = new SaveRig())
            {
                var first = rig.Commit(null, "old"); var ticket = rig.Prepare(first.CurrentHead, new[] { "new" });
                rig.Storage.Arm("Marker.Promote.after", ticket.Metadata.CommitId);
                Bad(rig.Store.Write(ticket, B()), "CommitUnknown");
                Ok(rig.Store.Lookup(first.Descriptor.CommitId, null, B()), "Committed"); Assert.IsTrue(rig.Store.HasPendingTicket);
                Bad(rig.Store.EndUncommitted(ticket, B()), "AlreadyCommitted");
                Assert.IsTrue(File.Exists(rig.Path(MarkerName(ticket.Metadata.CommitId)))); Assert.IsFalse(rig.Store.HasPendingTicket);
                var head = Ok(rig.Store.Lookup(ticket.Metadata.CommitId, null, B()), "Committed").CurrentHead;
                rig.Commit(head, "later"); var writes = rig.Storage.WriteCalls;
                var original = Ok(rig.Store.Write(ticket, B()), "Committed");
                Assert.AreEqual(new BigInteger(2), original.Descriptor.SaveGeneration); Assert.AreEqual(new BigInteger(3), original.CurrentHead.SaveGeneration); Assert.AreEqual(writes, rig.Storage.WriteCalls);
            }
        }
    }

    internal static class LocalSaveTestFiles
    {
        private static readonly string Project = System.IO.Path.GetFullPath(@"D:\Unity\UnityProj\FightMatch");
        internal static readonly string RunRoot = System.IO.Path.Combine(Project, "TestArtifacts", "FMDemoB12", Guid.NewGuid().ToString("N"));
        internal static string NewCase()
        {
            var path = System.IO.Path.GetFullPath(System.IO.Path.Combine(RunRoot, Guid.NewGuid().ToString("N")));
            Safe(path); Directory.CreateDirectory(path); TestContext.Out.WriteLine("FMDemoB12 case: " + path); return path;
        }
        internal static string Safe(string path)
        {
            var full = System.IO.Path.GetFullPath(path);
            Assert.IsTrue(full.StartsWith(RunRoot + System.IO.Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase), full);
            for (var p = full; !string.IsNullOrEmpty(p); p = System.IO.Path.GetDirectoryName(p))
                if (File.Exists(p) || Directory.Exists(p)) Assert.AreEqual((FileAttributes)0, File.GetAttributes(p) & FileAttributes.ReparsePoint, p);
            return full;
        }
        internal static SaveStoreBudget B() { return new SaveStoreBudget(new SaveCodecBudget(new ExactMathBudget())); }
        internal static T Ok<T>(LocalSaveResult<T> result, string code)
        { Assert.IsTrue(result.IsAccepted, result.Code + " " + result.FieldPath + " " + result.Diagnostic?.ExceptionMessage); Assert.AreEqual(code, result.Code); return result.Value; }
        internal static void Bad<T>(LocalSaveResult<T> result, string code)
        { Assert.IsFalse(result.IsAccepted); Assert.AreEqual(default(T), result.Value); Assert.AreEqual(code, result.Code, result.FieldPath + " " + result.Diagnostic?.ExceptionMessage); Assert.IsNotEmpty(result.FieldPath); }
        internal static LocalSaveStore Open(ILocalSaveStorage storage, SaveOpenMode mode = SaveOpenMode.CreateNew)
        { return Ok(LocalSaveStore.Open(storage, storage.Profile.PlayerId, storage.Profile.Purpose, mode, SaveFaultModel.EditorProcessCrash, B()), "Opened"); }
        internal static SaveEnvelopeInput EmptyInput(SaveCommitMetadata metadata)
        { return new SaveEnvelopeInput { PlayerId = metadata.PlayerId, Purpose = metadata.Purpose, SaveGeneration = metadata.SaveGeneration,
            CommitId = metadata.CommitId, ParentCommitId = metadata.ParentCommitId, CommitIndex = metadata.CommitIndex,
            RequiredSliceContracts = new RequiredSliceContract[0], Slices = new SaveSliceInput[0] }; }
        internal static SaveCodecResult<SaveEnvelope> Empty(SaveCommitMetadata metadata, SaveCodecBudget budget)
        { return SaveEnvelopeCodec.Prepare(EmptyInput(metadata), budget); }
        internal static string SnapshotName(string id) { return "c-" + id + ".snapshot"; }
        internal static string MarkerName(string id) { return "c-" + id + ".commit"; }
        internal static SnapshotDescriptor Copy(SnapshotDescriptor d, byte[] sha)
        { return new SnapshotDescriptor(d.Purpose, d.PlayerId, d.SaveGeneration, d.CommitId, d.ParentCommitId, d.TotalLength, sha); }
        internal static SaveCommitIndexEntry Row(SaveCommitIndexEntry r, string commit = null, ulong? length = null, byte[] sha = null, string[] operations = null)
        { return new SaveCommitIndexEntry(r.Generation, commit ?? r.CommitId, r.ParentCommitId, length ?? r.SnapshotLength, sha ?? r.SnapshotSha256, operations ?? r.OperationIds); }
        internal static void SameDescriptor(SnapshotDescriptor a, SnapshotDescriptor b)
        {
            Assert.AreEqual(a.Purpose, b.Purpose); Assert.AreEqual(a.PlayerId, b.PlayerId); Assert.AreEqual(a.SaveGeneration, b.SaveGeneration);
            Assert.AreEqual(a.CommitId, b.CommitId); Assert.AreEqual(a.ParentCommitId, b.ParentCommitId); Assert.AreEqual(a.TotalLength, b.TotalLength);
            CollectionAssert.AreEqual(a.Sha256, b.Sha256);
        }
        internal static byte[] EnvelopeBytes(SaveCommitTicket ticket)
        { using (var memory = new MemoryStream()) { BusinessSaveScenario.Accept(SaveEnvelopeCodec.Write(memory, ticket.Envelope, B().Codec)); return memory.ToArray(); } }
        internal static CandidateBusinessSnapshot ReadBusiness(ILocalSaveStorage storage, SnapshotDescriptor expected)
        {
            using (var stream = storage.OpenRead(SnapshotName(expected.CommitId)))
            {
                var budget = B().Codec; var envelope = BusinessSaveScenario.Accept(SaveEnvelopeCodec.Read(stream, expected, budget));
                return BusinessSaveScenario.Accept(CandidateBusinessSaveCodec.Decode(envelope, budget));
            }
        }
        internal static byte[] Hash(byte[] bytes) { using (var sha = SHA256.Create()) return sha.ComputeHash(bytes); }
        internal static string Hex(byte[] bytes) { return string.Concat(bytes.Select(x => x.ToString("x2"))); }
    }

    internal sealed class SaveRig : IDisposable
    {
        internal readonly string Root;
        internal readonly FaultStorage Storage;
        internal LocalSaveStore Store;
        internal SaveRig(string player = "p")
        { Root = NewCase(); Storage = new FaultStorage(new WindowsEditorSaveStorage(Root, player, SavePurpose.CandidateValidation)); Store = Open(Storage); }
        internal SaveCommitTicket Prepare(SnapshotDescriptor head, IReadOnlyList<string> operations)
        { var budget = B(); return Ok(Store.Prepare(head, operations, m => Empty(m, budget.Codec), budget), "Prepared"); }
        internal SaveCommittedReference Commit(SnapshotDescriptor head, string operation)
        { return Ok(Store.Write(Prepare(head, new[] { operation }), B()), "Committed"); }
        internal string Path(string name) { return Safe(System.IO.Path.Combine(Storage.Profile.DirectoryPath, name)); }
        internal void Put(string name, byte[] bytes) { File.WriteAllBytes(Path(name), bytes); }
        internal void Delete(string name) { File.Delete(Path(name)); }
        public void Dispose() { Store.Dispose(); }
    }

    // Every operation delegates to real System.IO storage. Faults never synthesize an in-memory filesystem.
    internal sealed class FaultStorage : ILocalSaveStorage
    {
        private readonly WindowsEditorSaveStorage real;
        private string point, target;
        private bool published;
        internal bool FaultUsed, FailEnumerationAfterFault, FailEnumerationAlways, FailDeleteOnce;
        internal int Calls, WriteCalls, RealFlushes, SnapshotAndMarkerPromotions;
        private readonly Dictionary<string, int> markerPromotions = new Dictionary<string, int>(StringComparer.Ordinal);
        public SaveStorageProfile Profile => real.Profile;
        internal FaultStorage(WindowsEditorSaveStorage real) { this.real = real; }
        internal void Arm(string point, string commit) { this.point = point; target = commit; FaultUsed = false; published = false; }
        internal void ArmHeadRead(string point, string commit) { Arm(point, commit); published = true; }
        internal bool Take(string key)
        {
            if (point != key || FaultUsed) return false;
            FaultUsed = true; return true;
        }
        internal void Hit(string key) { if (Take(key)) throw new IOException("Injected " + key); }
        public IDisposable AcquireWriterLease(bool createDirectory) { Calls++; return real.AcquireWriterLease(createDirectory); }
        public IEnumerable<string> EnumerateNames()
        {
            Calls++;
            if (FailEnumerationAlways || (FailEnumerationAfterFault && FaultUsed)) throw new IOException("Injected incomplete enumeration");
            if (published) Hit("Final.Enumerate.before");
            foreach (var name in real.EnumerateNames())
            {
                if (published) Hit("Final.EnumerateItem.before"); yield return name;
                if (published) Hit("Final.EnumerateItem.after");
            }
            if (published) Hit("Final.Enumerate.after");
        }
        private string ReadPrefix(string name)
        {
            if (target == null || !name.Contains(target)) return "Unarmed";
            var type = name.Contains(".snapshot") ? "Snapshot" : "Marker";
            return name.EndsWith(".tmp", StringComparison.Ordinal) ? type + ".Verify" : published ? "Final." + type : "Unarmed";
        }
        public Stream OpenRead(string name)
        {
            Calls++; var prefix = ReadPrefix(name); Hit(prefix + ".Open.before"); var stream = real.OpenRead(name);
            try { Hit(prefix + ".Open.after"); return new FaultStream(this, stream, prefix); }
            catch { stream.Dispose(); throw; }
        }
        public Stream CreateWork(string name)
        {
            Calls++; var prefix = name.Contains(".snapshot") ? "Snapshot" : "Marker";
            Hit(prefix + ".Create.before"); var stream = real.CreateWork(name);
            try { Hit(prefix + ".Create.after"); return new FaultStream(this, stream, prefix); }
            catch { stream.Dispose(); throw; }
        }
        public void FlushFile(Stream stream)
        {
            Calls++; var wrapper = (FaultStream)stream; Hit(wrapper.Prefix + ".Flush.before");
            Assert.IsInstanceOf<FileStream>(wrapper.Inner); real.FlushFile(wrapper.Inner); RealFlushes++; Hit(wrapper.Prefix + ".Flush.after");
        }
        public void PromoteNoReplace(string work, string final)
        {
            Calls++; var marker = final.EndsWith(".commit", StringComparison.Ordinal); var prefix = marker ? "Marker" : "Snapshot";
            Hit(prefix + ".Promote.before"); real.PromoteNoReplace(work, final); SnapshotAndMarkerPromotions++;
            if (marker)
            {
                var id = final.Substring(2, 32); markerPromotions[id] = markerPromotions.ContainsKey(id) ? markerPromotions[id] + 1 : 1;
                if (id == target) published = true;
            }
            Hit(prefix + ".Promote.after");
        }
        internal int RealMarkerPromotionsFor(string id) { return markerPromotions.ContainsKey(id) ? markerPromotions[id] : 0; }
        public void DeleteIndexedOld(string name) { real.DeleteIndexedOld(name); }
        public void DeleteUncommitted(string name)
        {
            Calls++; if (FailDeleteOnce) { FailDeleteOnce = false; throw new IOException("Injected delete failure"); }
            real.DeleteUncommitted(name);
        }
        private sealed class FaultStream : Stream
        {
            private readonly FaultStorage storage;
            private bool closed;
            internal readonly Stream Inner;
            internal readonly string Prefix;
            internal FaultStream(FaultStorage storage, Stream inner, string prefix)
            { this.storage = storage; Inner = inner; Prefix = prefix; }
            public override bool CanRead => Inner.CanRead;
            public override bool CanWrite => Inner.CanWrite;
            public override bool CanSeek => Inner.CanSeek;
            public override long Length => Inner.Length;
            public override long Position { get => Inner.Position; set => Inner.Position = value; }
            public override void Flush() { Inner.Flush(); }
            public override long Seek(long offset, SeekOrigin origin) { return Inner.Seek(offset, origin); }
            public override void SetLength(long value) { Inner.SetLength(value); }
            public override int Read(byte[] buffer, int offset, int count)
            {
                storage.Hit(Prefix + ".Read.before"); var read = Inner.Read(buffer, offset, Math.Min(count, 97));
                storage.Hit(Prefix + ".Read.after"); return read;
            }
            public override void Write(byte[] buffer, int offset, int count)
            {
                storage.WriteCalls++; storage.Hit(Prefix + ".Write.before");
                if (storage.Take(Prefix + ".Write.partial"))
                { Inner.Write(buffer, offset, Math.Max(1, count / 2)); throw new IOException("Injected " + Prefix + ".Write.partial"); }
                if (storage.Take(Prefix + ".Write.short")) { Inner.Write(buffer, offset, count / 2); return; }
                Inner.Write(buffer, offset, count); storage.Hit(Prefix + ".Write.after");
            }
            protected override void Dispose(bool disposing)
            {
                if (!disposing || closed) return;
                closed = true;
                try { storage.Hit(Prefix + ".Close.before"); }
                finally { Inner.Dispose(); }
                storage.Hit(Prefix + ".Close.after"); base.Dispose(disposing);
            }
        }
    }
}
