using System;
using LocalSaveTestFiles = FightMatch.Core.Tests.LocalSaveTestFiles;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FightMatch.Core;
using FightMatch.Platform;
using NUnit.Framework;
using static FightMatch.Tests.SaveRecoveryProcessCases;

namespace FightMatch.Tests
{
    [TestFixture]
    public sealed class SaveRecoveryTests
    {
        [Test]
        public void FourBusinessGenerationsLoadRewardHistoryRollbackAndAllFiveOriginalBodiesAfterReopen()
        {
            using (var r = new RecoveryRig("player:015b"))
            {
                var s = FightMatch.Core.Tests.BusinessSaveScenario.New(3); s.Begin(); s.Win(); s.EndVictory();
                Assert.AreEqual(1, s.Rewards.BaseRewards.Count);
                var tickets = new List<SaveCommitTicket>(); SnapshotDescriptor head = null;
                for (var i = 1; i <= 4; i++)
                {
                    if (i == 2) { s.EntryInput.AttemptId = "second"; s.EntryInput.ChallengeId = "second-challenge"; s.EntryInput.EntryBaselineId = "second-baseline"; s.Begin(); }
                    if (i == 3) s.Attack("attack-again", 0);
                    if (i == 4) s.Rollback("rollback-again", "attack-again");
                    var snapshot = FightMatch.Core.Tests.BusinessSaveScenario.Prepare(s);
                    var ticket = PrepareBusiness(r.Store, head, "save:" + i, snapshot); tickets.Add(ticket);
                    head = Ok(r.Store.Write(ticket, Budget()), "Committed").CurrentHead;
                    var loaded = Ok(r.Store.Load(Budget()), "Loaded");
                    for (var slice = 0; slice < 5; slice++) CollectionAssert.AreEqual(ticket.Envelope.SliceBytes[slice], loaded.SliceBytes[slice]);
                    var decoded = SaveRecoveryProcessCases.Core(CandidateBusinessSaveCodec.Decode(loaded, Budget().Codec));
                    Assert.AreEqual(s.Character.Experience, decoded.Character.Experience);
                    Assert.AreEqual(s.Inventory.Holdings[0].T, decoded.Inventory.Holdings[0].T);
                    Assert.AreEqual(s.Rewards.BaseRewards[0].SettlementId, decoded.Rewards.BaseRewards[0].SettlementId);
                    if (s.History != null) FightMatch.Core.Tests.BusinessSaveScenario.Same(s.History.CurrentRun.CurrentSnapshot, decoded.ActiveHistory.CurrentRun.CurrentSnapshot);
                }
                r.Reopen(); var final = SaveRecoveryProcessCases.Core(CandidateBusinessSaveCodec.Decode(Ok(r.Store.Load(Budget()), "Loaded"), Budget().Codec));
                Assert.AreEqual(1, final.ActiveHistory.Archive.Count); Assert.AreEqual(1, final.ActiveHistory.RollbackRecords.Count);
                Assert.AreEqual(1, final.Rewards.BaseRewards.Count); Assert.AreEqual(head.CommitId, Ok(r.Store.Lookup(tickets[0].Metadata.CommitId, "save:1", Budget()), "Committed").CurrentHead.CommitId);
                var view = Ok(r.Store.ReadRecovery(Budget()), "RecoveryObserved");
                Assert.AreEqual(3, view.RetainedRoots.Count); Assert.IsTrue(view.RequirementsComplete); Assert.IsTrue(view.EvidenceComplete);
                Assert.IsTrue(view.RetainedRoots.All(x => x.Kind == SaveRecoveryRootKind.Backup));
                foreach (var root in view.RetainedRoots.Concat(new[] { view.Current }))
                    Assert.IsTrue(root.Requirements.Bindings.Count > 0);
            }
        }

        [TestCase("marker")] [TestCase("missing-body")] [TestCase("digest")] [TestCase("version")] [TestCase("fork")]
        public void LatestDamageNeverFallsBackAndNeverAuthorizes(string fault)
        {
            using (var r = new RecoveryRig())
            {
                var first = r.Commit(); var latest = r.Commit(first.Descriptor);
                var body = "c-" + latest.Descriptor.CommitId + ".snapshot"; var marker = "c-" + latest.Descriptor.CommitId + ".commit";
                if (fault == "marker") r.Put(marker, new byte[] { 1 });
                if (fault == "missing-body") File.Delete(r.Path(body));
                if (fault == "digest" || fault == "version") { var b = File.ReadAllBytes(r.Path(body)); b[fault == "version" ? 8 : b.Length - 1] ^= 1; r.Put(body, b); }
                if (fault == "fork") r.Put("c-" + Guid.NewGuid().ToString("N") + ".commit", File.ReadAllBytes(r.Path(marker)));
                Bad(r.Store.Load(Budget()), "RecoveryBlocked");
                var view = Ok(r.Store.ReadRecovery(Budget()), "RecoveryObserved"); Assert.AreEqual(SaveHeadStatus.RecoveryBlocked, view.Status);
                var calls = 0; Bad(r.Store.WithVerifiedRecovery(view, Capabilities(view), _ => calls++, Budget()), "RecoveryBlocked");
                Bad(r.Store.Cleanup(view, Budget()), "RecoveryBlocked"); Assert.AreEqual(0, calls); Assert.AreEqual(0, r.Storage.Deletes);
            }
        }

        [TestCase("length")] [TestCase("version")] [TestCase("directory")] [TestCase("body-digest")]
        [TestCase("trailing")] [TestCase("identity")] [TestCase("budget")]
        public void UncommittedReaderRejectsIndependentWireDamage(string damage)
        {
            using (var r = new RecoveryRig())
            {
                var t = r.Prepare(null); var bytes = Bytes(t); var b = Budget().Codec;
                if (damage == "length") bytes[12] ^= 1;
                if (damage == "version") bytes[8] = 99;
                if (damage == "directory")
                {
                    var marker = Encoding.Unicode.GetBytes("slice-b13"); var pos = Find(bytes, marker, Find(bytes, marker, 0) + marker.Length);
                    // Directory contract: SliceId text, OwnerId text, schema u32, body offset u64.
                    var ownerLength = BitConverter.ToUInt32(bytes, pos + marker.Length);
                    var offset = pos + marker.Length + 4 + (int)ownerLength * 2 + 4; bytes[offset] = 1;
                }
                if (damage == "body-digest") bytes[bytes.Length - 1] ^= 1;
                if (damage == "trailing") bytes = bytes.Concat(new byte[] { 0 }).ToArray();
                if (damage == "identity") bytes[60] = 99;
                if (damage == "budget") b = new SaveCodecBudget(new ExactMathBudget(), maxEnvelopeBytes: (ulong)bytes.Length - 1);
                using (var stream = new MemoryStream(bytes))
                { var result = SaveEnvelopeCodec.ReadUncommittedRequirements(stream, b); Assert.IsFalse(result.IsAccepted); Assert.IsNull(result.Value); Assert.IsNotEmpty(result.FieldPath); }
            }
        }
        [Test]
        public void UncommittedReaderSharesStrictParserWithoutOwningStreamOrWeakeningExpectedChecks()
        {
            using (var r = new RecoveryRig())
            {
                var t = r.Prepare(null); var bytes = Bytes(t); var b = Budget().Codec;
                using (var stream = new TrackingStream(new MemoryStream(bytes)))
                {
                    var summary = SaveRecoveryProcessCases.Core(SaveEnvelopeCodec.ReadUncommittedRequirements(stream, b));
                    Assert.AreEqual(t.Metadata.CommitId, summary.Descriptor.CommitId); Assert.AreEqual(Hash(bytes), string.Concat(summary.Descriptor.Sha256.Select(x => x.ToString("x2"))));
                    Assert.AreEqual(0, stream.Flushes); Assert.IsFalse(stream.Closed);
                    Assert.Throws<ArgumentNullException>(() => SaveEnvelopeCodec.Read(stream, null, b));
                    Assert.Throws<ArgumentNullException>(() => SaveEnvelopeCodec.ReadRequirements(stream, null, b));
                    var bad = new SnapshotDescriptor(summary.Descriptor.Purpose, summary.Descriptor.PlayerId, summary.Descriptor.SaveGeneration,
                        summary.Descriptor.CommitId, null, summary.Descriptor.TotalLength, new byte[32]);
                    stream.Position = 0; Assert.AreEqual("DigestMismatch", SaveEnvelopeCodec.Read(stream, bad, b).RejectionCode);
                    stream.Position = 0; Assert.AreEqual("DigestMismatch", SaveEnvelopeCodec.ReadRequirements(stream, bad, b).RejectionCode);
                    stream.Position = 0; stream.FailRead = true; Assert.Throws<IOException>(() => SaveEnvelopeCodec.ReadUncommittedRequirements(stream, b));
                    Assert.IsFalse(stream.Closed);
                }
            }
        }

        [TestCase("complete")] [TestCase("partial")] [TestCase("memory")] [TestCase("unknown")] [TestCase("wrong-player")] [TestCase("wrong-commit")]
        public void PendingAndUnknownRootsAreExplicitAndNeverOpenLoadOrCleanup(string kind)
        {
            using (var r = new RecoveryRig())
            {
                var saved = r.Commit(); var ticket = r.Prepare(saved.CurrentHead);
                if (kind != "memory")
                {
                    var bytes = Bytes(ticket); var name = "c-" + ticket.Metadata.CommitId + ".snapshot";
                    if (kind == "partial") bytes = bytes.Take(31).ToArray();
                    if (kind == "unknown") name = "unknown.file";
                    if (kind == "wrong-commit") name = "c-" + Guid.NewGuid().ToString("N") + ".snapshot";
                    if (kind == "wrong-player") { var p = Find(bytes, Encoding.Unicode.GetBytes(r.Storage.Profile.PlayerId), 0); bytes[p] ^= 1; }
                    r.Put(name, bytes); r.Reopen();
                }
                var view = Ok(r.Store.ReadRecovery(Budget()), "RecoveryObserved");
                var complete = kind == "complete" || kind == "memory";
                Assert.AreEqual(complete, view.RequirementsComplete); Assert.AreEqual(kind != "unknown", view.EvidenceComplete);
                Assert.AreEqual(complete ? 1 : 0, view.RetainedRoots.Count(x => x.Kind == SaveRecoveryRootKind.Pending));
                Assert.IsFalse(r.Store.Load(Budget()).IsAccepted); Assert.IsFalse(r.Store.Cleanup(view, Budget()).IsAccepted);
                var calls = 0; var check = r.Store.WithVerifiedRecovery(view, Capabilities(view), _ => calls++, Budget());
                Assert.AreEqual(complete, check.IsAccepted); Assert.AreEqual(complete ? 1 : 0, calls);
                Assert.AreEqual(0, r.Storage.Deletes);
                if (kind == "memory") Assert.AreEqual("pending-ticket", view.RetainedRoots.Last().SourceName);
            }
        }
        [Test]
        public void CompleteWorkMarkerMustMatchTheCompleteCandidateDescriptor()
        {
            using (var r = new RecoveryRig())
            {
                var first = r.Commit(); var ticket = r.Prepare(first.CurrentHead);
                r.Put("c-" + ticket.Metadata.CommitId + ".snapshot", Bytes(ticket));
                var marker = File.ReadAllBytes(r.Path("c-" + first.Descriptor.CommitId + ".commit"));
                var oldId = Encoding.Unicode.GetBytes(first.Descriptor.CommitId); var newId = Encoding.Unicode.GetBytes(ticket.Metadata.CommitId);
                for (var i = 0; i <= marker.Length - oldId.Length; i++)
                    if (marker.Skip(i).Take(oldId.Length).SequenceEqual(oldId)) { Buffer.BlockCopy(newId, 0, marker, i, newId.Length); i += oldId.Length - 1; }
                using (var hash = SHA256.Create()) Buffer.BlockCopy(hash.ComputeHash(marker, 0, marker.Length - 32), 0, marker, marker.Length - 32, 32);
                r.Put("w-" + ticket.Metadata.CommitId + ".commit.tmp", marker); r.Reopen();
                var view = Ok(r.Store.ReadRecovery(Budget()), "RecoveryObserved");
                Assert.IsTrue(view.EvidenceComplete); Assert.IsFalse(view.RequirementsComplete);
                Assert.AreEqual(1, view.RetainedRoots.Count(x => x.Kind == SaveRecoveryRootKind.Pending));
                Assert.IsTrue(view.Diagnostics.Any(x => x.Code == "InconsistentBinding"));
                Bad(r.Store.WithVerifiedRecovery(view, Capabilities(view), _ => Assert.Fail(), Budget()), "RecoveryBlocked");
                Bad(r.Store.Cleanup(view, Budget()), "RecoveryBlocked"); Assert.AreEqual(0, r.Storage.Deletes);
            }
        }
        [Test]
        public void EmptyDirectoryRequiresExplicitCreateNewAndReadCallsNeverInitialize()
        {
            using (var r = new RecoveryRig())
            {
                var empty = Ok(r.Store.ReadRecovery(Budget()), "RecoveryObserved"); Assert.AreEqual(SaveHeadStatus.NoSave, empty.Status);
                var calls = 0; Ok(r.Store.WithVerifiedRecovery(empty, Capabilities(empty), _ => calls++, Budget()), "Verified");
                Bad(r.Store.Load(Budget()), "NoSave"); Assert.IsFalse(r.Store.Cleanup(empty, Budget()).IsAccepted);
                r.Reopen(); var existing = Ok(r.Store.ReadRecovery(Budget()), "RecoveryObserved");
                Bad(r.Store.WithVerifiedRecovery(existing, Capabilities(existing), _ => calls++, Budget()), "InitializationRequired");
                Assert.AreEqual(1, calls); Assert.AreEqual(1, Directory.GetFiles(r.Storage.Profile.DirectoryPath).Length);
            }
        }

        [TestCase("Content")] [TestCase("Definition")] [TestCase("CandidateContent")] [TestCase("CandidateDefinition")]
        [TestCase("slice")] [TestCase("owner")] [TestCase("schema")] [TestCase("rule")] [TestCase("numeric")] [TestCase("random")] [TestCase("feature")]
        [TestCase("fingerprint")] [TestCase("notes")]
        public void EveryExactCapabilityIsRequiredBeforeTheAction(string missing)
        {
            using (var r = new RecoveryRig())
            {
                r.Commit(); var view = Ok(r.Store.ReadRecovery(Budget()), "RecoveryObserved"); var caps = Capabilities(view);
                var slices = caps.ReadableSlices.ToArray(); var bindings = caps.Bindings.ToArray();
                if (Enum.TryParse<SaveBindingKind>(missing, out var kind)) bindings = bindings.Where(x => x.Kind != kind).ToArray();
                if (missing == "slice") slices = new RequiredSliceContract[0];
                if (missing == "owner" || missing == "schema") slices[0] = new RequiredSliceContract(slices[0].SliceId, missing == "owner" ? "other" : slices[0].OwnerId, missing == "schema" ? 2U : 1U);
                if (missing == "fingerprint" || missing == "notes")
                {
                    var x = bindings[2]; bindings[2] = new SaveBinding(x.Kind, x.PackageId, x.DraftId, x.DraftRevision,
                        missing == "fingerprint" ? "different" : x.ContentFingerprint, x.RuleVersion, x.NumericContractVersion, x.RandomContractVersion,
                        missing == "notes" ? x.SourceNotes.Reverse().ToArray() : x.SourceNotes, x.LevelId, x.LevelVersion);
                }
                var incomplete = new SaveRecoveryCapabilities(slices, bindings, missing == "rule" ? new string[0] : caps.RuleVersions,
                    missing == "numeric" ? new string[0] : caps.NumericContractVersions, missing == "random" ? new string[0] : caps.RandomContractVersions,
                    missing == "feature" ? new string[0] : caps.FeatureIds);
                var called = 0; var result = r.Store.WithVerifiedRecovery(view, incomplete, _ => called++, Budget());
                Bad(result, kind != 0 || missing == "fingerprint" || missing == "notes" ? "UnsupportedBinding" : "UnsupportedCapability"); Assert.AreEqual(0, called);
                Ok(r.Store.WithVerifiedRecovery(view, caps, _ => called++, Budget()), "Verified"); Assert.AreEqual(1, called);
            }
        }
        [TestCase("candidate")] [TestCase("old-bytes")] [TestCase("set")] [TestCase("ticket")]
        public void SameHeadDoesNotAuthorizeAfterFileOrPendingEvidenceChanges(string change)
        {
            using (var r = new RecoveryRig())
            {
                var first = r.Commit(); var last = r.Commit(first.CurrentHead);
                if (change == "candidate") { var t = r.Prepare(last.CurrentHead); r.Put("c-" + t.Metadata.CommitId + ".snapshot", Bytes(t)); r.Reopen(); }
                var view = Ok(r.Store.ReadRecovery(Budget()), "RecoveryObserved"); var oldHead = view.Current.Descriptor.CommitId;
                if (change == "candidate") { var f = view.Files.Single(x => x.Disposition == SaveFileDisposition.Pending); r.Put(f.Name, new byte[] { 0 }); }
                if (change == "old-bytes") r.Put("c-" + first.Descriptor.CommitId + ".snapshot", new byte[] { 1, 2 });
                if (change == "set") r.Put("w-" + Guid.NewGuid().ToString("N") + ".snapshot.tmp", new byte[] { 1 });
                if (change == "ticket") r.Prepare(last.CurrentHead);
                Assert.AreEqual(oldHead, Ok(r.Store.ReadRecovery(Budget()), "RecoveryObserved").Current.Descriptor.CommitId);
                var count = 0; Bad(r.Store.WithVerifiedRecovery(view, Capabilities(view), _ => count++, Budget()), "StaleContext");
                Bad(r.Store.Cleanup(view, Budget()), "StaleContext"); Assert.AreEqual(0, count); Assert.AreEqual(0, r.Storage.Deletes);
            }
        }
        [Test]
        public void ForeignDisposedReentrantConcurrentAndThrowingActionsCannotBypassTheSingleGate()
        {
            using (var r = new RecoveryRig()) using (var other = new RecoveryRig())
            {
                var current = r.Commit(); var view = Ok(r.Store.ReadRecovery(Budget()), "RecoveryObserved"); var caps = Capabilities(view);
                Bad(other.Store.WithVerifiedRecovery(view, caps, _ => Assert.Fail(), Budget()), "StaleContext");
                Ok(r.Store.WithVerifiedRecovery(view, caps, fresh => {
                    Bad(r.Store.ReadRecovery(Budget()), "Busy"); Bad(r.Store.Load(Budget()), "Busy"); Bad(r.Store.Cleanup(fresh, Budget()), "Busy");
                    Assert.AreEqual("Busy", Task.Run(() => r.Store.Prepare(current.CurrentHead, new[] { "intrude" }, m => Raw(m), Budget())).Result.Code);
                    Assert.Throws<InvalidOperationException>(() => r.Store.Dispose());
                }, Budget()), "Verified");
                var original = new IOException("callback exact identity");
                Assert.AreSame(original, Assert.Throws<IOException>(() => r.Store.WithVerifiedRecovery(view, caps, _ => { throw original; }, Budget())));
                Ok(r.Store.Load(Budget()), "Loaded"); r.Store.Dispose();
                Bad(r.Store.WithVerifiedRecovery(view, caps, _ => Assert.Fail(), Budget()), "Disposed");
            }
        }
        [Test]
        public void CapabilityAndRecoveryModelsDefensivelyFreezeNestedCollections()
        {
            var notes = new List<string> { "a", "b" }; var binding = Binding(SaveBindingKind.CandidateContent, notes);
            var bindings = new List<SaveBinding> { binding }; var ids = new List<string> { "r" };
            var caps = new SaveRecoveryCapabilities(new[] { new RequiredSliceContract("s", "o", 1) }, bindings, ids, new[] { "n" }, new[] { "q" }, new[] { "f" });
            notes[0] = "changed"; bindings.Clear(); ids.Clear(); Assert.AreEqual("a", caps.Bindings[0].SourceNotes[0]); Assert.AreEqual("r", caps.RuleVersions[0]);
            Assert.Throws<ArgumentNullException>(() => new SaveRecoveryCapabilities(null, bindings, ids, ids, ids, ids));
            Assert.Throws<ArgumentException>(() => new SaveRecoveryCapabilities(new RequiredSliceContract[0], new[] { binding, binding }, ids, ids, ids, ids));
            Assert.Throws<ArgumentException>(() => new SaveRecoveryCapabilities(new RequiredSliceContract[0], bindings, new[] { "x", "x" }, ids, ids, ids));
            Assert.Throws<ArgumentException>(() => new SaveRecoveryCapabilities(new[] { new RequiredSliceContract(" ", "o", 1) }, bindings, ids, ids, ids, ids));
            using (var r = new RecoveryRig())
            {
                r.Commit(); var view = Ok(r.Store.ReadRecovery(Budget()), "RecoveryObserved");
                Assert.Throws<NotSupportedException>(() => ((IList<byte>)view.Current.Descriptor.Sha256)[0] = 0);
                Assert.Throws<NotSupportedException>(() => ((IList<SaveFileObservation>)view.Files).Clear());
                Assert.Throws<NotSupportedException>(() => ((IList<string>)view.Current.Requirements.Bindings[2].SourceNotes).Clear());
            }
        }

        [TestCase("intact")] [TestCase("missing-parent")] [TestCase("bad-parent")] [TestCase("bad-earliest")]
        public void CleanupKeepsCurrentAndExactPreviousAndPreservesHistoricalKeys(string damage)
        {
            using (var r = new RecoveryRig())
            {
                var commits = r.Four(); var previous = commits[2].Descriptor.CommitId;
                if (damage == "missing-parent") { File.Delete(r.Path("c-" + previous + ".commit")); File.Delete(r.Path("c-" + previous + ".snapshot")); }
                if (damage == "bad-parent") r.Put("c-" + previous + ".snapshot", new byte[] { 1 });
                if (damage == "bad-earliest") r.Put("c-" + commits[0].Descriptor.CommitId + ".snapshot", new byte[] { 1 });
                var view = Ok(r.Store.ReadRecovery(Budget()), "RecoveryObserved");
                Assert.AreEqual(damage != "intact", view.RedundancyDegraded); Ok(r.Store.Load(Budget()), "Loaded");
                var result = Ok(r.Store.Cleanup(view, Budget()), "Cleaned"); Assert.AreEqual(previous, result.PreviousCommitId); Assert.AreEqual(4, result.RemovedNames.Count);
                CollectionAssert.AreEqual(new[] { "c-" + commits[0].Descriptor.CommitId + ".commit", "c-" + commits[0].Descriptor.CommitId + ".snapshot",
                    "c-" + commits[1].Descriptor.CommitId + ".commit", "c-" + commits[1].Descriptor.CommitId + ".snapshot" }, result.RemovedNames);
                r.Reopen(); var fresh = Ok(r.Store.ReadRecovery(Budget()), "RecoveryObserved");
                Assert.AreEqual(damage == "missing-parent" || damage == "bad-parent" ? 0 : 1, fresh.RetainedRoots.Count);
                Assert.AreEqual(commits[3].Descriptor.CommitId, Ok(r.Store.Lookup(commits[0].Descriptor.CommitId, "op:1", Budget()), "Committed").CurrentHead.CommitId);
                Assert.AreEqual(0, Ok(r.Store.Cleanup(fresh, Budget()), "Cleaned").RemovedNames.Count);
            }
        }
        [TestCase("before")] [TestCase("after")]
        public void PartialDeleteFailurePreservesExactExceptionAndNeedsFreshEvidence(string phase)
        {
            using (var r = new RecoveryRig())
            {
                var commits = r.Four(); var view = Ok(r.Store.ReadRecovery(Budget()), "RecoveryObserved");
                r.Storage.Hook = point => { if (point == "Delete." + phase && r.Storage.Deletes == (phase == "before" ? 1 : 2)) throw new IOException("delete " + phase); };
                var result = r.Store.Cleanup(view, Budget()); Bad(result, "CleanupInterrupted");
                Assert.AreEqual(typeof(IOException).FullName, result.Diagnostic.ExceptionType); Assert.AreEqual("delete " + phase, result.Diagnostic.ExceptionMessage);
                StringAssert.EndsWith(".Delete", result.Diagnostic.Stage); r.Storage.Hook = null;
                Bad(r.Store.Cleanup(view, Budget()), "StaleContext"); r.Reopen(); Ok(r.Store.Load(Budget()), "Loaded");
                var fresh = Ok(r.Store.ReadRecovery(Budget()), "RecoveryObserved");
                var clean = Ok(r.Store.Cleanup(fresh, Budget()), "Cleaned"); Assert.AreEqual(phase == "before" ? 3 : 2, clean.RemovedNames.Count);
                Assert.AreEqual(commits[3].Descriptor.CommitId, clean.CurrentHead.CommitId); Assert.AreEqual(commits[2].Descriptor.CommitId, clean.PreviousCommitId);
            }
        }
        [TestCase("Open")] [TestCase("Read")] [TestCase("Close")] [TestCase("Enumerate")]
        public void CurrentIoNeverProducesALoadOrRecoverySuccess(string phase)
        {
            using (var r = new RecoveryRig())
            {
                var commit = r.Commit(); var target = "c-" + commit.Descriptor.CommitId + ".snapshot";
                r.Storage.Hook = point => { if (point == phase || point == phase + "." + target) throw new UnauthorizedAccessException("current " + phase); };
                foreach (var result in new[] { Failure(r.Store.Load(Budget())), Failure(r.Store.ReadRecovery(Budget())) })
                { Assert.AreEqual("StorageFailure", result.Code); Assert.AreEqual(typeof(UnauthorizedAccessException).FullName, result.ExceptionType); Assert.AreEqual("current " + phase, result.ExceptionMessage); }
                r.Storage.Hook = null; Ok(r.Store.Load(Budget()), "Loaded");
            }
        }
        [Test]
        public void RealSharingDenialDirectoryChangeBudgetsAndSingleWriterStayFailClosed()
        {
            using (var r = new RecoveryRig())
            {
                var first = r.Commit(); var view = Ok(r.Store.ReadRecovery(Budget()), "RecoveryObserved");
                var second = LocalSaveStore.Open(LocalSaveTestFiles.CreateStorage(r.Root, r.Storage.Profile.PlayerId, SavePurpose.CandidateValidation),
                    r.Storage.Profile.PlayerId, SavePurpose.CandidateValidation, SaveOpenMode.Existing, SaveFaultModel.EditorProcessCrash, Budget()); Bad(second, "Busy");
                using (var held = new FileStream(r.Path("c-" + first.Descriptor.CommitId + ".snapshot"), FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                { Bad(r.Store.Load(Budget()), "StorageFailure"); Bad(r.Store.ReadRecovery(Budget()), "StorageFailure"); }
                Bad(r.Store.ReadRecovery(new SaveStoreBudget(new SaveCodecBudget(new ExactMathBudget(), maxEnvelopeBytes: 10))), "Limit");
                Bad(r.Store.ReadRecovery(new SaveStoreBudget(Budget().Codec, maxDirectoryEntries: 1)), "Limit");
                var calls = 0; r.Storage.Hook = point => { if (point == "Enumerate" && ++calls == 2) r.Put("w-" + Guid.NewGuid().ToString("N") + ".snapshot.tmp", new byte[] { 1 }); };
                Bad(r.Store.ReadRecovery(Budget()), "StaleContext"); r.Storage.Hook = null;
                var unsupported = LocalSaveStore.Open(r.Storage, r.Storage.Profile.PlayerId, SavePurpose.CandidateValidation,
                    SaveOpenMode.CreateNew, SaveFaultModel.PowerLossDurable, Budget()); Bad(unsupported, "StorageCapabilityUnavailable");
            }
        }
        [TestCase("collection")] [TestCase("string")] [TestCase("numeric")] [TestCase("envelope")]
        public void MemoryOnlyPendingRootUsesTheNewCallsBudget(string limit)
        {
            using (var r = new RecoveryRig())
            {
                r.Prepare(null);
                var codec = new SaveCodecBudget(new ExactMathBudget(), maxEnvelopeBytes: limit == "envelope" ? 1UL : 67108864UL,
                    maxCollectionEntries: limit == "collection" ? 0 : 65536, maxStringCodeUnits: limit == "string" ? 0 : 65536,
                    maxNumericTokenBytes: limit == "numeric" ? 0 : 4096);
                Bad(r.Store.ReadRecovery(new SaveStoreBudget(codec)), "Limit");
                var view = Ok(r.Store.ReadRecovery(Budget()), "RecoveryObserved");
                Assert.AreEqual("pending-ticket", view.RetainedRoots.Single().SourceName); Assert.AreEqual(1, Directory.GetFiles(r.Storage.Profile.DirectoryPath).Length);
            }
        }
        [TestCase("writer.lock")] [TestCase("w-0123456789abcdef0123456789abcdef.snapshot.tmp")] [TestCase("../outside")]
        public void AdapterDeleteOnlyAcceptsFinalProtocolNames(string name)
        { using (var r = new RecoveryRig()) Assert.Throws<ArgumentException>(() => r.Storage.Real.DeleteIndexedOld(name)); }
        [TestCase("marker", 0)] [TestCase("snapshot", 0)] [TestCase("both", 0)]
        [TestCase("marker", 2)] [TestCase("snapshot", 2)] [TestCase("both", 2)]
        [TestCase("marker", 3)] [TestCase("snapshot", 3)] [TestCase("both", 3)]
        public void IndexedCompleteWorkKeepsAllRecoveryGatesConsistentAndSurvivesCleanup(string kind, int generation)
        {
            using (var r = new RecoveryRig())
            {
                var commits = r.Four(); var id = commits[generation].Descriptor.CommitId;
                var copies = new Dictionary<string, byte[]>();
                foreach (var suffix in kind == "both" ? new[] { "commit", "snapshot" } : new[] { kind == "marker" ? "commit" : "snapshot" })
                { var name = "w-" + id + "." + suffix + ".tmp"; copies.Add(name, File.ReadAllBytes(r.Path("c-" + id + "." + suffix))); r.Put(name, copies[name]); }
                var head = Ok(r.Store.Inspect(Budget()), "Inspected"); Assert.AreEqual(SaveHeadStatus.Ready, head.Status);
                Assert.AreEqual(commits[3].Descriptor.CommitId, Ok(r.Store.Load(Budget()), "Loaded").CommitId);
                var ticket = r.Prepare(commits[3].CurrentHead); Ok(r.Store.EndUncommitted(ticket, Budget()), "Ended");
                var view = Ok(r.Store.ReadRecovery(Budget()), "RecoveryObserved");
                TestContext.Out.WriteLine("F1 original Inspect=" + head.Status + "; ReadRecovery=" + view.Status + "; work=" + kind + "; generation=" + (generation + 1));
                foreach (var copy in copies)
                {
                    var original = head.Files.Single(x => x.Name == copy.Key); var file = view.Files.Single(x => x.Name == copy.Key);
                    Assert.AreEqual(SaveFileDisposition.IndexedOldCopy, file.Disposition); Assert.AreEqual("IndexedCompleteWorkCopy", file.Detail);
                    Assert.AreEqual(original.Disposition, file.Disposition); Assert.AreEqual(id, file.CommitId);
                    Assert.AreEqual((ulong)copy.Value.Length, file.Length); CollectionAssert.AreEqual(original.Sha256, file.Sha256);
                    Assert.AreEqual(Hash(copy.Value), string.Concat(file.Sha256.Select(x => x.ToString("x2"))));
                }
                Assert.AreEqual(SaveHeadStatus.Ready, view.Status, "complete indexed work must preserve Inspect Ready");
                Assert.IsFalse(view.HasUnresolvedCandidate); Assert.IsTrue(view.RequirementsComplete); Assert.IsTrue(view.EvidenceComplete);
                Assert.AreEqual(3, view.RetainedRoots.Count); Assert.IsTrue(view.RetainedRoots.All(x => x.Kind == SaveRecoveryRootKind.Backup));
                Assert.AreEqual(commits[3].Descriptor.CommitId, view.Current.Descriptor.CommitId);
                var calls = 0; Ok(r.Store.WithVerifiedRecovery(view, Capabilities(view), _ => calls++, Budget()), "Verified"); Assert.AreEqual(1, calls);
                var keep = head.Files.Where(x => x.Disposition == SaveFileDisposition.Current || x.CommitId == commits[2].Descriptor.CommitId)
                    .ToDictionary(x => x.Name, x => File.ReadAllBytes(r.Path(x.Name)));
                var clean = Ok(r.Store.Cleanup(view, Budget()), "Cleaned");
                CollectionAssert.AreEqual(OldFinalNames(commits), clean.RemovedNames);
                Assert.AreEqual(commits[2].Descriptor.CommitId, clean.PreviousCommitId);
                foreach (var copy in copies.Concat(keep)) CollectionAssert.AreEqual(copy.Value, File.ReadAllBytes(r.Path(copy.Key)));
                r.Reopen(); var fresh = Ok(r.Store.ReadRecovery(Budget()), "RecoveryObserved"); Assert.AreEqual(SaveHeadStatus.Ready, fresh.Status);
                Assert.AreEqual(1, fresh.RetainedRoots.Count); Assert.AreEqual(0, Ok(r.Store.Cleanup(fresh, Budget()), "Cleaned").RemovedNames.Count);
                foreach (var copy in copies) CollectionAssert.AreEqual(copy.Value, File.ReadAllBytes(r.Path(copy.Key)));
                for (var i = 0; i < commits.Length; i++) Assert.AreEqual(commits[3].Descriptor.CommitId,
                    Ok(r.Store.Lookup(commits[i].Descriptor.CommitId, "op:" + (i + 1), Budget()), "Committed").CurrentHead.CommitId);
                var damaged = copies.First(); var bytes = (byte[])damaged.Value.Clone(); bytes[bytes.Length - 1] ^= 1; r.Put(damaged.Key, bytes);
                Bad(r.Store.WithVerifiedRecovery(fresh, Capabilities(fresh), _ => Assert.Fail(), Budget()), "StaleContext");
                Bad(r.Store.Cleanup(fresh, Budget()), "StaleContext");
                var pending = Ok(r.Store.ReadRecovery(Budget()), "RecoveryObserved"); Assert.AreEqual(SaveHeadStatus.Pending, pending.Status);
                Assert.IsTrue(pending.HasUnresolvedCandidate); Assert.IsFalse(pending.RequirementsComplete);
                Bad(r.Store.Load(Budget()), "CommitUnknown"); Bad(r.Store.Prepare(commits[3].CurrentHead, new[] { "blocked" }, Raw, Budget()), "Pending");
                Bad(r.Store.WithVerifiedRecovery(pending, Capabilities(pending), _ => Assert.Fail(), Budget()), "RecoveryBlocked");
                Bad(r.Store.Cleanup(pending, Budget()), "RecoveryBlocked"); Assert.AreEqual(4, r.Storage.Deletes);
            }
        }
        [TestCase("commit", "partial")] [TestCase("snapshot", "partial")]
        [TestCase("commit", "digest")] [TestCase("snapshot", "digest")]
        [TestCase("commit", "other-commit")] [TestCase("snapshot", "other-commit")]
        [TestCase("commit", "unknown-commit")] [TestCase("snapshot", "unknown-commit")]
        public void IncompleteDamagedOrUnattributedWorkStillClosesConflictingGates(string suffix, string damage)
        {
            using (var r = new RecoveryRig())
            {
                var commits = r.Four(); var id = damage == "unknown-commit" ? Guid.NewGuid().ToString("N") : commits[0].Descriptor.CommitId;
                var source = commits[damage == "other-commit" ? 1 : 0].Descriptor.CommitId;
                var bytes = File.ReadAllBytes(r.Path("c-" + source + "." + suffix));
                if (damage == "partial") bytes = bytes.Take(17).ToArray();
                if (damage == "digest") bytes[bytes.Length - 1] ^= 1;
                var name = "w-" + id + "." + suffix + ".tmp"; r.Put(name, bytes);
                Assert.AreEqual(SaveHeadStatus.Pending, Ok(r.Store.Inspect(Budget()), "Inspected").Status);
                var view = Ok(r.Store.ReadRecovery(Budget()), "RecoveryObserved"); Assert.AreEqual(SaveHeadStatus.Pending, view.Status);
                Assert.IsTrue(view.HasUnresolvedCandidate); Assert.IsFalse(view.RequirementsComplete); Assert.IsTrue(view.EvidenceComplete);
                Assert.AreEqual(SaveFileDisposition.Pending, view.Files.Single(x => x.Name == name).Disposition);
                Assert.AreEqual(commits[3].Descriptor.CommitId, view.Current.Descriptor.CommitId);
                Bad(r.Store.Load(Budget()), "CommitUnknown"); Bad(r.Store.Prepare(commits[3].CurrentHead, new[] { "blocked" }, Raw, Budget()), "Pending");
                Bad(r.Store.WithVerifiedRecovery(view, Capabilities(view), _ => Assert.Fail(), Budget()), "RecoveryBlocked");
                Bad(r.Store.Cleanup(view, Budget()), "RecoveryBlocked"); Assert.AreEqual(0, r.Storage.Deletes);
                CollectionAssert.AreEqual(bytes, File.ReadAllBytes(r.Path(name)));
            }
        }
        [TestCase(0, true)] [TestCase(1, true)] [TestCase(0, false)]
        public void CleanupRechecksActualTargetBytesAfterFullViewRecheck(int targetIndex, bool replace)
        {
            using (var r = new RecoveryRig())
            {
                var commits = r.Four(); var names = OldFinalNames(commits); var target = names[targetIndex];
                var view = Ok(r.Store.ReadRecovery(Budget()), "RecoveryObserved");
                var keep = view.Files.Where(x => x.Disposition == SaveFileDisposition.Current || x.CommitId == commits[2].Descriptor.CommitId)
                    .ToDictionary(x => x.Name, x => File.ReadAllBytes(r.Path(x.Name)));
                var original = File.ReadAllBytes(r.Path(target)); var changed = (byte[])original.Clone(); changed[changed.Length - 1] ^= 1;
                var trace = new List<string>(); var enumerations = 0; var injected = false; var targetOpened = false;
                r.Storage.Hook = point =>
                {
                    if (!point.StartsWith("Read.", StringComparison.Ordinal)) trace.Add(point);
                    if (point == "Enumerate.after") enumerations++;
                    if (point != "Open." + target || enumerations != 2) return;
                    Assert.IsFalse(targetOpened); targetOpened = true; Assert.AreEqual(targetIndex, r.Storage.Deletes);
                    Assert.AreEqual(2, trace.Count(x => x == "Enumerate.after"));
                    // The last enumeration ends ObserveRecovery; this next target Open is inside Cleanup's per-file verification.
                    Assert.AreEqual(targetIndex, trace.Skip(trace.LastIndexOf("Enumerate.after") + 1).Count(x => x == "Delete.after"));
                    CollectionAssert.AreEqual(original, File.ReadAllBytes(r.Path(target)));
                    if (replace)
                    {
                        r.Put(target, changed); injected = true; trace.Add("ActualBytesReplaced." + target);
                        Assert.AreEqual(original.Length, new FileInfo(r.Path(target)).Length); Assert.AreNotEqual(Hash(original), FileHash(r.Path(target)));
                    }
                };
                var result = r.Store.Cleanup(view, Budget()); r.Storage.Hook = null;
                TestContext.Out.WriteLine("F2 target=" + target + "; replace=" + replace + "; originalSHA=" + Hash(original) + "; replacementSHA=" + Hash(changed));
                TestContext.Out.WriteLine(string.Join("\n", trace)); Assert.IsTrue(targetOpened); Assert.AreEqual(replace, injected);
                if (replace)
                {
                    Bad(result, "CleanupInterrupted"); Assert.AreEqual("StaleContext", result.Diagnostic.Code);
                    Assert.AreEqual("Cleanup.Target", result.FieldPath); Assert.AreEqual("Cleanup." + target + ".Verify", result.Diagnostic.Stage);
                    Assert.IsNull(result.Diagnostic.ExceptionType); Assert.AreEqual(targetIndex, r.Storage.Deletes);
                    CollectionAssert.AreEqual(changed, File.ReadAllBytes(r.Path(target)));
                    for (var i = 0; i < names.Length; i++) Assert.AreEqual(i >= targetIndex, File.Exists(r.Path(names[i])), names[i]);
                    Bad(r.Store.Cleanup(view, Budget()), "StaleContext"); Assert.AreEqual(targetIndex, r.Storage.Deletes);
                    r.Reopen(); var fresh = Ok(r.Store.ReadRecovery(Budget()), "RecoveryObserved");
                    Assert.AreEqual(SaveHeadStatus.Ready, fresh.Status); Assert.IsTrue(fresh.RedundancyDegraded);
                    var clean = Ok(r.Store.Cleanup(fresh, Budget()), "Cleaned"); CollectionAssert.AreEqual(names.Skip(targetIndex), clean.RemovedNames);
                }
                else CollectionAssert.AreEqual(names, Ok(result, "Cleaned").RemovedNames);
                Assert.AreEqual(4, r.Storage.Deletes);
                foreach (var copy in keep) CollectionAssert.AreEqual(copy.Value, File.ReadAllBytes(r.Path(copy.Key)));
                Assert.AreEqual(commits[3].Descriptor.CommitId, Ok(r.Store.Load(Budget()), "Loaded").CommitId);
                for (var i = 0; i < commits.Length; i++) Assert.AreEqual(commits[3].Descriptor.CommitId,
                    Ok(r.Store.Lookup(commits[i].Descriptor.CommitId, "op:" + (i + 1), Budget()), "Committed").CurrentHead.CommitId);
            }
        }
        private static string[] OldFinalNames(SaveCommittedReference[] commits)
        { return commits.Take(2).SelectMany(x => new[] { "c-" + x.Descriptor.CommitId + ".commit", "c-" + x.Descriptor.CommitId + ".snapshot" }).ToArray(); }
        private static LocalSaveDiagnostic Failure<T>(LocalSaveResult<T> r) { Assert.IsFalse(r.IsAccepted); Assert.AreEqual(default(T), r.Value); return r.Diagnostic; }
        private static void Bad<T>(LocalSaveResult<T> r, string code) { Assert.IsFalse(r.IsAccepted, r.Code); Assert.AreEqual(code, r.Code, r.FieldPath); Assert.AreEqual(default(T), r.Value); }
        private static int Find(byte[] b, byte[] needle, int start)
        { for (var i = start; i <= b.Length - needle.Length; i++) if (b.Skip(i).Take(needle.Length).SequenceEqual(needle)) return i; throw new InvalidOperationException("wire field not found"); }
        internal static byte[] Bytes(SaveCommitTicket t)
        { using (var s = new MemoryStream()) { SaveRecoveryProcessCases.Core(SaveEnvelopeCodec.Write(s, t.Envelope, Budget().Codec)); return s.ToArray(); } }
        internal static SaveBinding Binding(SaveBindingKind kind, IReadOnlyList<string> notes = null)
        {
            var candidate = kind == SaveBindingKind.CandidateContent || kind == SaveBindingKind.CandidateDefinition;
            var definition = kind == SaveBindingKind.Definition || kind == SaveBindingKind.CandidateDefinition;
            return new SaveBinding(kind, candidate ? null : "pkg", candidate ? "draft" : null, candidate ? (BigInteger?)1 : null,
                "fingerprint", "r", "n", "q", candidate ? notes ?? new[] { "first", "second" } : null, definition ? "level" : null, definition ? "v1" : null);
        }
        internal static SaveCodecResult<SaveEnvelope> Raw(SaveCommitMetadata m)
        {
            var c = new RequiredSliceContract("slice-b13", "owner-b13", 1);
            var requirements = new SaveRequirements(new[] { Binding(SaveBindingKind.Content), Binding(SaveBindingKind.Definition),
                Binding(SaveBindingKind.CandidateContent), Binding(SaveBindingKind.CandidateDefinition) }, new[] { "r" }, new[] { "n" }, new[] { "q" }, new[] { "f" });
            return SaveEnvelopeCodec.Prepare(new SaveEnvelopeInput { PlayerId = m.PlayerId, Purpose = m.Purpose, SaveGeneration = m.SaveGeneration,
                CommitId = m.CommitId, ParentCommitId = m.ParentCommitId, CommitIndex = m.CommitIndex, RequiredSliceContracts = new[] { c },
                Slices = new[] { new SaveSliceInput { Contract = c, Bytes = new byte[] { 5, 7, 9 }, Requirements = requirements } } }, Budget().Codec);
        }
    }

    internal sealed class RecoveryRig : IDisposable
    {
        private static readonly string RunRoot = LocalSaveTestFiles.IsMac ? LocalSaveTestFiles.RunRoot : NewRun();
        internal readonly string Root;
        internal readonly RecoveryFaultStorage Storage;
        internal LocalSaveStore Store;
        private int sequence;
        internal RecoveryRig(string player = "b13-player")
        {
            Root = LocalSaveTestFiles.IsMac ? LocalSaveTestFiles.NewCase() : Safe(RunRoot, System.IO.Path.Combine(RunRoot, Guid.NewGuid().ToString("N"))); Directory.CreateDirectory(Root);
            TestContext.Out.WriteLine("FMDemoB13 case: " + Root);
            Storage = new RecoveryFaultStorage(LocalSaveTestFiles.CreateStorage(Root, player, SavePurpose.CandidateValidation)); Store = Open(Storage);
        }
        internal SaveCommitTicket Prepare(SnapshotDescriptor head)
        { return Ok(Store.Prepare(head, new[] { "op:" + ++sequence }, SaveRecoveryTests.Raw, Budget()), "Prepared"); }
        internal SaveCommittedReference Commit(SnapshotDescriptor head = null) { return Ok(Store.Write(Prepare(head), Budget()), "Committed"); }
        internal SaveCommittedReference[] Four()
        { var result = new List<SaveCommittedReference>(); SnapshotDescriptor head = null; for (var i = 0; i < 4; i++) { var c = Commit(head); result.Add(c); head = c.CurrentHead; } return result.ToArray(); }
        internal string Path(string name) { var path = System.IO.Path.Combine(Storage.Profile.DirectoryPath, name); return LocalSaveTestFiles.IsMac ? LocalSaveTestFiles.Safe(path) : Safe(RunRoot, path); }
        internal void Put(string name, byte[] bytes) { File.WriteAllBytes(Path(name), bytes); }
        internal void Reopen() { Store.Dispose(); Store = Open(Storage, SaveOpenMode.Existing); }
        public void Dispose() { Store.Dispose(); }
    }
    internal sealed class RecoveryFaultStorage : ILocalSaveStorage
    {
        internal readonly ILocalSaveStorage Real;
        internal Action<string> Hook;
        internal int Deletes;
        internal RecoveryFaultStorage(ILocalSaveStorage real) { Real = real; }
        public SaveStorageProfile Profile => Real.Profile;
        public IDisposable AcquireWriterLease(bool createDirectory) { return Real.AcquireWriterLease(createDirectory); }
        public IEnumerable<string> EnumerateNames() { Hook?.Invoke("Enumerate"); var names = Real.EnumerateNames().ToArray(); Hook?.Invoke("Enumerate.after"); return names; }
        public Stream OpenRead(string name) { Hook?.Invoke("Open." + name); return new TrackingStream(Real.OpenRead(name), point => Hook?.Invoke(point + "." + name)); }
        public Stream CreateWork(string name) { return Real.CreateWork(name); }
        public void FlushFile(Stream stream) { Real.FlushFile(stream); }
        public void PromoteNoReplace(string work, string final) { Real.PromoteNoReplace(work, final); }
        public void DeleteUncommitted(string name) { Real.DeleteUncommitted(name); }
        public void DeleteIndexedOld(string name) { Hook?.Invoke("Delete.before"); Real.DeleteIndexedOld(name); Deletes++; Hook?.Invoke("Delete.after"); }
    }
    internal sealed class TrackingStream : Stream
    {
        private readonly Stream inner; private readonly Action<string> hook;
        internal int Flushes; internal bool Closed, FailRead;
        internal TrackingStream(Stream inner, Action<string> hook = null) { this.inner = inner; this.hook = hook; }
        public override bool CanRead => inner.CanRead; public override bool CanWrite => inner.CanWrite; public override bool CanSeek => inner.CanSeek;
        public override long Length => inner.Length; public override long Position { get => inner.Position; set => inner.Position = value; }
        public override int Read(byte[] b, int o, int c) { hook?.Invoke("Read"); if (FailRead) throw new IOException("reader I/O"); return inner.Read(b, o, c); }
        public override void Write(byte[] b, int o, int c) { inner.Write(b, o, c); }
        public override void Flush() { Flushes++; inner.Flush(); }
        public override long Seek(long o, SeekOrigin origin) { return inner.Seek(o, origin); }
        public override void SetLength(long n) { inner.SetLength(n); }
        protected override void Dispose(bool disposing) { if (disposing) { Closed = true; inner.Dispose(); hook?.Invoke("Close"); } base.Dispose(disposing); }
    }
}
