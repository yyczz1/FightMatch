using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using FightMatch.Platform;
using NUnit.Framework;
using static FightMatch.Core.Tests.LocalSaveTestFiles;

namespace FightMatch.Core.Tests
{
    [TestFixture]
    public class SaveCommitMarkerTests
    {
        private const string FixedId = "00000000000000000000000000000001";
        // Independent protocol concatenation: 60-byte header + 192-byte empty-envelope metadata.
        private const string SnapshotHex =
            "464d53415645303101000000c0000000000000000000000000000000e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855" +
            "0001000000700001000000312000000030003000300030003000300030003000300030003000300030003000300030003000300030003000300030003000300030003000300030003000300030003100" +
            "00000000000000000001000000010000003120000000300030003000300030003000300030003000300030003000300030003000300030003000300030003000300030003000300030003000300030003000300031000000000000000000000000000000000000000000000000000000";
        private const string SnapshotSha = "6de3d0102b5f261399b5355eb454f13d7a84c4caf27e7158498bcc67b4a98041";
        private const string MarkerHex =
            "464d434d5430303101000000000101000000700001000000312000000030003000300030003000300030003000300030003000300030003000300030003000300030003000300030003000300030003000300030003000300030003100" +
            "002b00000063002d00300030003000300030003000300030003000300030003000300030003000300030003000300030003000300030003000300030003000300030003000300031002e0073006e0061007000730068006f007400" +
            "fc000000000000006de3d0102b5f261399b5355eb454f13d7a84c4caf27e7158498bcc67b4a98041d4949c9bbbfa6ce51632a553b6d6410805eb357529f98f5555ef344e88607d60";
        private const string MarkerSha = "dc0cc1dfa707b0cb8c5610f5e386b9748663625b8688b2494dcf4b9cb8847e9e";
        private static byte[] Bytes(string hex) { return BusinessSaveScenario.Hex(hex); }
        private static SnapshotDescriptor FixedDescriptor()
        { return new SnapshotDescriptor(SavePurpose.CandidateValidation, "p", 1, FixedId, null, 252, Bytes(SnapshotSha)); }

        [Test]
        public void FixedFullSnapshotAndMarkerBytesAreReadableWithoutEitherProductionWriter()
        {
            var snapshot = Bytes(SnapshotHex); var marker = Bytes(MarkerHex);
            Assert.AreEqual(252, snapshot.Length); Assert.AreEqual(SnapshotSha, Hex(Hash(snapshot)));
            Assert.AreEqual(256, marker.Length); Assert.AreEqual(MarkerSha, Hex(Hash(marker)));
            CollectionAssert.AreEqual(marker, IndependentMarker(FixedDescriptor()));
            using (var rig = new SaveRig())
            {
                rig.Store.Dispose(); rig.Put(SnapshotName(FixedId), snapshot); rig.Put(MarkerName(FixedId), marker);
                rig.Store = Open(rig.Storage, SaveOpenMode.Existing);
                var observed = rig.Store.InitialInspection.Value; Assert.AreEqual(SaveHeadStatus.Ready, observed.Status);
                SameDescriptor(FixedDescriptor(), observed.Current.Descriptor);
                SameDescriptor(FixedDescriptor(), Ok(rig.Store.Lookup(FixedId, null, B()), "Committed").Descriptor);
                Assert.AreEqual(0, rig.Storage.WriteCalls);
            }
        }

        [Test]
        public void ThreeWrittenEmptyEnvelopesAndMarkersMatchIndependentByteRulesIncludingOldDigests()
        {
            using (var rig = new SaveRig())
            {
                SnapshotDescriptor head = null;
                for (var i = 0; i < 3; i++)
                {
                    var ticket = rig.Prepare(head, i == 0 ? new string[0] : new[] { "operation:" + i });
                    var independent = IndependentSnapshot(ticket.Metadata); var result = Ok(rig.Store.Write(ticket, B()), "Committed");
                    CollectionAssert.AreEqual(independent, File.ReadAllBytes(rig.Path(SnapshotName(result.Descriptor.CommitId))));
                    Assert.AreEqual((ulong)independent.Length, result.Descriptor.TotalLength);
                    CollectionAssert.AreEqual(Hash(independent), result.Descriptor.Sha256);
                    CollectionAssert.AreEqual(IndependentMarker(result.Descriptor), File.ReadAllBytes(rig.Path(MarkerName(result.Descriptor.CommitId))));
                    head = result.CurrentHead;
                }
                Assert.AreEqual(6, rig.Storage.RealFlushes);
            }
        }

        [TestCase("magic")] [TestCase("version")] [TestCase("purpose")] [TestCase("fault")]
        [TestCase("presence")] [TestCase("string-length")] [TestCase("integer-length")]
        [TestCase("zero-generation")] [TestCase("commit-case")] [TestCase("snapshot-name")]
        [TestCase("zero-snapshot-length")] [TestCase("snapshot-sha")] [TestCase("marker-sha")]
        [TestCase("tail")] [TestCase("truncated")] [TestCase("file-identity")]
        public void IndependentlyCorruptedMarkersNeverBecomeCurrent(string mutation)
        {
            using (var rig = new SaveRig())
            {
                var marker = Bytes(MarkerHex); var name = MarkerName(FixedId);
                if (mutation == "magic") marker[0] ^= 1;
                if (mutation == "version") marker[8] = 2;
                if (mutation == "purpose") marker[12] = 9;
                if (mutation == "fault") marker[13] = 2;
                if (mutation == "presence") marker[93] = 2;
                if (mutation == "string-length") marker[14] = 200;
                if (mutation == "integer-length") marker[20] = 2;
                if (mutation == "zero-generation") marker[24] = (byte)'0';
                if (mutation == "commit-case") marker[29] = (byte)'A';
                if (mutation == "snapshot-name") marker[98] = (byte)'x';
                if (mutation == "zero-snapshot-length") for (var i = 184; i < 192; i++) marker[i] = 0;
                if (mutation == "snapshot-sha") marker[192] ^= 1;
                if (mutation == "tail") marker = marker.Take(marker.Length - 32).Concat(new byte[] { 1 }).Concat(new byte[32]).ToArray();
                if (mutation == "file-identity") name = MarkerName("00000000000000000000000000000002");
                marker = Rehash(marker);
                if (mutation == "marker-sha") marker[marker.Length - 1] ^= 1;
                if (mutation == "truncated") marker = marker.Take(marker.Length - 1).ToArray();
                rig.Put(SnapshotName(FixedId), Bytes(SnapshotHex)); rig.Put(name, marker);
                var observed = Ok(rig.Store.Inspect(B()), "Inspected");
                Assert.AreEqual(SaveHeadStatus.RecoveryBlocked, observed.Status); Assert.IsNull(observed.Current); Assert.IsNotEmpty(observed.Diagnostics);
                Bad(rig.Store.Lookup(FixedId, null, B()), "RecoveryBlocked");
                Assert.IsTrue(File.Exists(rig.Path(name)));
            }
        }

        [TestCase("01")] [TestCase("+1")] [TestCase("-0")] [TestCase(" 1")] [TestCase("1e0")] [TestCase("1.0")] [TestCase("")]
        public void NonCanonicalGenerationTokensRejectAfterTheIndependentMarkerHashIsRebuilt(string token)
        {
            using (var rig = new SaveRig())
            {
                rig.Put(SnapshotName(FixedId), Bytes(SnapshotHex)); rig.Put(MarkerName(FixedId), IndependentMarker(FixedDescriptor(), token));
                var observed = Ok(rig.Store.Inspect(B()), "Inspected");
                Assert.AreEqual(SaveHeadStatus.RecoveryBlocked, observed.Status); Assert.IsNull(observed.Current);
                Assert.IsTrue(observed.Diagnostics.Any(x => x.Code == "Malformed"));
            }
        }

        [Test]
        public void CandidatePlayerAndOriginalUtf16IdentitiesHaveSeparatePhysicalDirectories()
        {
            var root = NewCase(); var directories = new HashSet<string>(StringComparer.Ordinal);
            foreach (var player in new[] { "\ud800", "\ufffd", " A中\udfff\0 ", " " })
                foreach (var purpose in new[] { SavePurpose.CandidateValidation, SavePurpose.PlayerSave })
                {
                    var storage = CreateStorage(root, player, purpose);
                    var units = new byte[player.Length * 2];
                    for (var i = 0; i < player.Length; i++) { units[2 * i] = (byte)player[i]; units[2 * i + 1] = (byte)(player[i] >> 8); }
                    Assert.AreEqual(Path.Combine(root, "p-" + Hex(Hash(units)), purpose == SavePurpose.PlayerSave ? "player" : "candidate"), storage.Profile.DirectoryPath);
                    Assert.IsTrue(directories.Add(storage.Profile.DirectoryPath));
                    using (var store = Open(storage))
                    {
                        var b = B(); var ticket = Ok(store.Prepare(null, new string[0], m => Empty(m, b.Codec), b), "Prepared");
                        var committed = Ok(store.Write(ticket, B()), "Committed"); Assert.AreEqual(player, committed.Descriptor.PlayerId); Assert.AreEqual(purpose, committed.Descriptor.Purpose);
                    }
                }
        }

        [TestCase("relative/path")] [TestCase(@"\\server\share")] [TestCase(@"\\?\C:\device")] [TestCase(@"\\.\C:\device")]
        public void UnprovenOrDeviceRootsAreRejectedBeforeAnyDirectoryCreation(string root)
        { Assert.Throws<ArgumentException>(() => CreateStorage(root, "p", SavePurpose.CandidateValidation)); }

        [Test]
        public void StorageNamesCannotEscapeOrOverwriteAndFlushRequiresTheActualOwnedFileStream()
        {
            using (var rig = new SaveRig())
            {
                var real = CreateStorage(rig.Root, "p", SavePurpose.CandidateValidation);
                Assert.Throws<ArgumentException>(() => real.CreateWork("../outside"));
                Assert.Throws<ArgumentException>(() => real.OpenRead("C:/outside"));
                Assert.Throws<ArgumentException>(() => real.DeleteUncommitted("writer.lock"));
                Assert.Throws<ArgumentException>(() => real.DeleteUncommitted(MarkerName(FixedId)));
                Assert.Throws<ArgumentException>(() => real.FlushFile(new MemoryStream()));
                var work = "w-" + FixedId + ".snapshot.tmp";
                using (var stream = real.CreateWork(work)) { stream.WriteByte(1); real.FlushFile(stream); }
                real.PromoteNoReplace(work, SnapshotName(FixedId));
                using (var stream = real.CreateWork(work)) { stream.WriteByte(2); real.FlushFile(stream); }
                Assert.Throws<IOException>(() => real.PromoteNoReplace(work, SnapshotName(FixedId)));
                CollectionAssert.AreEqual(new byte[] { 1 }, File.ReadAllBytes(rig.Path(SnapshotName(FixedId))));
            }
        }

        [Test]
        public void FinalReadMathExhaustionIsUnknownAndASeparateQueryCanConfirmTheOriginalCommit()
        {
            long used;
            using (var measure = new SaveRig())
            {
                var ticket = measure.Prepare(null, new[] { "one" }); var budget = B();
                Ok(measure.Store.Write(ticket, budget), "Committed"); used = budget.Codec.Math.PrimitiveStepsUsed;
            }
            using (var rig = new SaveRig())
            {
                var ticket = rig.Prepare(null, new[] { "one" });
                var budget = new SaveStoreBudget(new SaveCodecBudget(new ExactMathBudget(maxPrimitiveSteps: checked((int)used - 1))));
                var result = rig.Store.Write(ticket, budget); Bad(result, "CommitUnknown"); Assert.IsNotEmpty(result.LimitReason);
                Assert.IsTrue(File.Exists(rig.Path(MarkerName(ticket.Metadata.CommitId)))); Assert.IsTrue(rig.Store.HasPendingTicket);
                Ok(rig.Store.Lookup(ticket.Metadata.CommitId, "one", B()), "Committed"); Assert.IsFalse(rig.Store.HasPendingTicket);
            }
        }

        [Test]
        public void ConflictingOwnSnapshotCannotBeOverwrittenAndUnknownFinalMarkerCannotBeDeleted()
        {
            using (var rig = new SaveRig())
            {
                var head = rig.Commit(null, "old").CurrentHead; var ticket = rig.Prepare(head, new[] { "new" });
                rig.Storage.Arm("Snapshot.Promote.after", ticket.Metadata.CommitId); Bad(rig.Store.Write(ticket, B()), "SaveFailed");
                rig.Put(SnapshotName(ticket.Metadata.CommitId), new byte[] { 7 });
                Bad(rig.Store.Write(ticket, B()), "RecoveryBlocked");
                CollectionAssert.AreEqual(new byte[] { 7 }, File.ReadAllBytes(rig.Path(SnapshotName(ticket.Metadata.CommitId))));
                Ok(rig.Store.EndUncommitted(ticket, B()), "Ended");
                var next = rig.Prepare(head, new[] { "next" }); rig.Storage.Arm("Marker.Promote.after", next.Metadata.CommitId);
                Bad(rig.Store.Write(next, B()), "CommitUnknown"); var name = MarkerName(next.Metadata.CommitId); var original = File.ReadAllBytes(rig.Path(name));
                rig.Put(name, new byte[] { 4 }); Bad(rig.Store.EndUncommitted(next, B()), "RecoveryBlocked");
                Assert.IsTrue(File.Exists(rig.Path(name))); Assert.IsTrue(rig.Store.HasPendingTicket);
                rig.Put(name, original); Ok(rig.Store.Lookup(next.Metadata.CommitId, null, B()), "Committed");
                Bad(rig.Store.EndUncommitted(next, B()), "AlreadyCommitted");
            }
        }

        [Test]
        public void CompleteIndexedTemporaryCopiesAreRecognizedWhileIncompleteCopiesRemainPending()
        {
            using (var rig = new SaveRig())
            {
                var first = rig.Commit(null, "one"); var second = rig.Commit(first.CurrentHead, "two");
                var snapshotWork = "w-" + first.Descriptor.CommitId + ".snapshot.tmp"; var markerWork = "w-" + first.Descriptor.CommitId + ".commit.tmp";
                rig.Put(snapshotWork, File.ReadAllBytes(rig.Path(SnapshotName(first.Descriptor.CommitId))));
                rig.Put(markerWork, File.ReadAllBytes(rig.Path(MarkerName(first.Descriptor.CommitId))));
                var ready = Ok(rig.Store.Inspect(B()), "Inspected"); Assert.AreEqual(SaveHeadStatus.Ready, ready.Status);
                Assert.AreEqual(2, ready.Files.Count(x => x.Detail == "IndexedCompleteWorkCopy"));
                rig.Put(snapshotWork, new byte[] { 8 });
                var pending = Ok(rig.Store.Inspect(B()), "Inspected"); Assert.AreEqual(SaveHeadStatus.Pending, pending.Status);
                SameDescriptor(second.CurrentHead, pending.Current.Descriptor);
                Bad(rig.Store.Prepare(second.CurrentHead, new[] { "later" }, m => Empty(m, B().Codec), B()), "Pending");
            }
        }

        internal static byte[] IndependentMarker(SnapshotDescriptor descriptor, string generationToken = null)
        {
            using (var memory = new MemoryStream())
            using (var writer = new BinaryWriter(memory, Encoding.ASCII, true))
            {
                writer.Write(Encoding.ASCII.GetBytes("FMCMT001")); writer.Write(1U); writer.Write((byte)descriptor.Purpose); writer.Write((byte)1);
                Text(writer, descriptor.PlayerId); Integer(writer, generationToken ?? descriptor.SaveGeneration.ToString(CultureInfo.InvariantCulture));
                Text(writer, descriptor.CommitId); OptionalText(writer, descriptor.ParentCommitId); Text(writer, SnapshotName(descriptor.CommitId));
                writer.Write(descriptor.TotalLength); writer.Write(descriptor.Sha256.ToArray());
                writer.Write(Hash(memory.ToArray())); return memory.ToArray();
            }
        }
        private static byte[] IndependentSnapshot(SaveCommitMetadata metadata)
        {
            byte[] fields;
            using (var memory = new MemoryStream())
            using (var writer = new BinaryWriter(memory, Encoding.ASCII, true))
            {
                writer.Write((byte)metadata.Purpose); Text(writer, metadata.PlayerId);
                Integer(writer, metadata.SaveGeneration.ToString(CultureInfo.InvariantCulture)); Text(writer, metadata.CommitId); OptionalText(writer, metadata.ParentCommitId);
                writer.Write(0U); writer.Write(0U); writer.Write((uint)metadata.CommitIndex.Count);
                foreach (var row in metadata.CommitIndex)
                {
                    Integer(writer, row.Generation.ToString(CultureInfo.InvariantCulture)); Text(writer, row.CommitId); OptionalText(writer, row.ParentCommitId);
                    writer.Write((byte)(row.SnapshotLength.HasValue ? 1 : 0));
                    if (row.SnapshotLength.HasValue) { writer.Write(row.SnapshotLength.Value); writer.Write(row.SnapshotSha256.ToArray()); }
                    writer.Write((uint)row.OperationIds.Count); foreach (var operation in row.OperationIds) Text(writer, operation);
                }
                for (var i = 0; i < 5; i++) writer.Write(0U); fields = memory.ToArray();
            }
            using (var memory = new MemoryStream())
            using (var writer = new BinaryWriter(memory, Encoding.ASCII, true))
            {
                writer.Write(Encoding.ASCII.GetBytes("FMSAVE01")); writer.Write(1U); writer.Write((ulong)fields.Length); writer.Write(0UL);
                writer.Write(Hash(new byte[0])); writer.Write(fields); return memory.ToArray();
            }
        }
        private static void Text(BinaryWriter writer, string value)
        { writer.Write((uint)value.Length); foreach (var codeUnit in value) writer.Write((ushort)codeUnit); }
        private static void OptionalText(BinaryWriter writer, string value)
        { writer.Write((byte)(value == null ? 0 : 1)); if (value != null) Text(writer, value); }
        private static void Integer(BinaryWriter writer, string token)
        { var bytes = Encoding.ASCII.GetBytes(token); writer.Write((uint)bytes.Length); writer.Write(bytes); }
        private static byte[] Rehash(byte[] marker)
        {
            var body = marker.Take(marker.Length - 32).ToArray(); return body.Concat(Hash(body)).ToArray();
        }
    }
}
