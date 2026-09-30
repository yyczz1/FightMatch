using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using FightMatch.Content;
using FightMatch.Platform;
using NUnit.Framework;
using static FightMatch.Core.Tests.PublishedContentTestData;

namespace FightMatch.Core.Tests
{
    public class FirstReleaseContentStorageTests
    {
        private PreparedPublication prepared;
        private byte[][] six;
        private Dictionary<string, byte[]> records;
        private const string Scope = "fixture:cold-scope", Release = "fixture:cold-release", Operation = "fixture:cold-op";
        [OneTimeSetUp] public void PublishIsolatedFixture()
        {
            var source = Fixture(); var job = new DemoContentDraft(source.DraftId).BeginJob(); prepared = Prepare(source, job);
            var writable = new MemoryStorage(); var catalog = new PublishedContentCatalog(writable, Caps());
            var review = Review(prepared); var publication = Take(catalog.Publish(prepared, prepared.Validation, review, job, Operation, StoreBudget()));
            Assert.AreEqual("Completed", publication.Status); Assert.AreEqual(7, writable.Blobs.Count);
            var receipt = publication.Publication.ReceiptBytes.ToArray();
            var set = new ContentReleaseSet { SchemaVersion = 1, Scope = Scope, ReleaseSetId = Release, Binding = ContentBindingRecord.From(prepared.Binding),
                PublicationReceiptSha256 = PublishedContentCodec.Sha256(receipt) };
            var release = Take(PublishedContentCodec.EncodeReleaseSet(set, StoreBudget()));
            writable.Blobs.Add(PublishedContentCatalog.ReleaseSetKey(Scope, Release), release);
            six = new[] { prepared.SourceBytes.ToArray(), prepared.PayloadBytes.ToArray(), prepared.Validation.Bytes.ToArray(),
                Take(PublishedContentCodec.EncodeReview(review, StoreBudget())), receipt, release };
            records = writable.Blobs.ToDictionary(p => p.Key, p => (byte[])p.Value.Clone());
        }
        private byte[][] CopyFiles() => six.Select(x => (byte[])x.Clone()).ToArray();
        private static PublicationResult<FirstReleaseContentStorage> Load(byte[][] files, ContentConsumerCapabilities caps = null, ContentStoreBudget budget = null)
            => FirstReleaseContentStorage.Create(files[0], files[1], files[2], files[3], files[4], files[5], caps ?? Caps(), budget ?? StoreBudget());
        private static void Reject(byte[][] files, string code = null, string field = null, ContentConsumerCapabilities caps = null, ContentStoreBudget budget = null)
        {
            PublicationResult<FirstReleaseContentStorage> result = null;
            Assert.DoesNotThrow(() => result = Load(files, caps, budget)); Assert.IsFalse(result.IsAccepted); Assert.IsNull(result.Value);
            Assert.IsNotEmpty(result.RejectionCode); Assert.IsNotEmpty(result.FieldPath);
            if (code != null) Assert.AreEqual(code, result.RejectionCode); if (field != null) Assert.AreEqual(field, result.FieldPath);
        }
        [Test] public void SixOriginalFilesResolveEightExactKeysLevelAndFrozenProfile()
        {
            var storage = Take(Load(CopyFiles())); var catalog = new PublishedContentCatalog(storage, Caps());
            Assert.AreEqual(8, records.Count); foreach (var p in records) CollectionAssert.AreEqual(p.Value, storage.Read(p.Key, 16 * 1024 * 1024));
            Assert.IsNull(storage.Read(new string('f', 64), 16 * 1024 * 1024));
            Assert.IsTrue(Take(catalog.GetCurrentBinding(Scope, Release, Caps())).Same(prepared.Binding));
            var resolved = Take(catalog.ResolveExact(prepared.Binding, Caps())); Assert.IsTrue(resolved.Binding.Same(prepared.Binding));
            CollectionAssert.AreEqual(prepared.NewProfile.CanonicalBytes, resolved.NewProfile.CanonicalBytes);
            CollectionAssert.AreEqual(six[4], resolved.ReceiptBytes);
            Assert.AreEqual("fixture:alpha", Take(catalog.ResolveExact(prepared.DefinitionBindings[0], Caps())).LevelId);
        }
        [Test] public void InputAndEveryReturnedArrayAreIndependentOwnedCopies()
        {
            var files = CopyFiles(); var storage = Take(Load(files)); foreach (var f in files) f[0] ^= 1;
            foreach (var pair in records)
            {
                var bytes = storage.Read(pair.Key, 16 * 1024 * 1024); bytes[0] ^= 1;
                CollectionAssert.AreEqual(pair.Value, storage.Read(pair.Key, 16 * 1024 * 1024));
            }
            Assert.IsTrue(Take(new PublishedContentCatalog(storage, Caps()).GetCurrentBinding(Scope, Release, Caps())).Same(prepared.Binding));
        }
        [Test] public void BothWriteAndLeaseRejectAndExistingBytesRemainIntact()
        {
            var storage = Take(Load(CopyFiles()));
            Assert.AreEqual("ReadOnly", Assert.Throws<ContentStorageException>(() => storage.AcquireWriter()).Code);
            Assert.AreEqual("ReadOnly", Assert.Throws<ContentStorageException>(() => storage.WriteImmutable(records.Keys.First(), new byte[] { 9 }, 1)).Code);
            foreach (var p in records) CollectionAssert.AreEqual(p.Value, storage.Read(p.Key, 16 * 1024 * 1024));
        }
        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)] [TestCase(5)]
        public void EveryMissingOrEmptyFileRejects(int index)
        { var files = CopyFiles(); files[index] = null; Reject(files, "MissingField"); files[index] = new byte[0]; Reject(files, "MissingField"); }
        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)] [TestCase(5)]
        public void TamperingAnyOneFileCannotEnterCatalog(int index)
        { var files = CopyFiles(); files[index][0] ^= 1; Reject(files); }
        [TestCase(4, "null")] [TestCase(5, "null")] [TestCase(4, " \r\n null\t ")] [TestCase(5, " \r\n null\t ")]
        public void NullPublicationOrReleaseRootUsesStableC2Rejection(int index, string text)
        { var files = CopyFiles(); files[index] = Encoding.UTF8.GetBytes(text); Reject(files, "InvalidSchema", "Root"); }
        [TestCase(4)] [TestCase(5)] public void UnknownRecordSchemaRejects(int index)
        {
            var files = CopyFiles(); var text = Encoding.UTF8.GetString(files[index]);
            Assert.That(text, Does.Contain("\"SchemaVersion\":1"));
            files[index] = Encoding.UTF8.GetBytes(text.Replace("\"SchemaVersion\":1", "\"SchemaVersion\":2"));
            Reject(files, "UnsupportedSchema", "FirstRelease.SchemaVersion");
        }
        [Test] public void WrongReceiptHashAndWrongReleaseBindingReject()
        {
            var set = new ContentReleaseSet { SchemaVersion = 1, Scope = Scope, ReleaseSetId = Release,
                Binding = ContentBindingRecord.From(prepared.Binding), PublicationReceiptSha256 = new string('a', 64) };
            var files = CopyFiles(); files[5] = Take(PublishedContentCodec.EncodeReleaseSet(set, StoreBudget())); Reject(files, "RecoveryBlocked", "ReleaseSet.Receipt");
            set.PublicationReceiptSha256 = PublishedContentCodec.Sha256(files[4]); set.Binding.PackageId = "fixture:other";
            files[5] = Take(PublishedContentCodec.EncodeReleaseSet(set, StoreBudget())); Reject(files, "UnsupportedBinding", "Binding");
        }
        [Test] public void RehashedNoncanonicalPayloadStillRejects()
        {
            var files = CopyFiles(); files[1] = Encoding.UTF8.GetBytes(" " + Encoding.UTF8.GetString(files[1]));
            var changed = PublishedContentCodec.Sha256(files[1]);
            files[4] = Encoding.UTF8.GetBytes(Encoding.UTF8.GetString(files[4]).Replace(prepared.PayloadSha256, changed));
            var binding = ContentBindingRecord.From(prepared.Binding); binding.ContentFingerprint = changed;
            files[5] = Take(PublishedContentCodec.EncodeReleaseSet(new ContentReleaseSet { SchemaVersion = 1, Scope = Scope, ReleaseSetId = Release,
                Binding = binding, PublicationReceiptSha256 = PublishedContentCodec.Sha256(files[4]) }, StoreBudget()));
            Reject(files, "RecoveryBlocked", "Payload.Canonical");
        }
        [Test] public void RehashedSourceThatDoesNotDescribePayloadRejects()
        {
            var files = CopyFiles(); var source = Take(PublishedContentCodec.DecodeSource(files[0], Caps(), Math()));
            source.SourceNotes.Add("Different isolated source"); files[0] = Encode(source);
            files[4] = Encoding.UTF8.GetBytes(Encoding.UTF8.GetString(files[4]).Replace(prepared.SourceSha256, PublishedContentCodec.Sha256(files[0])));
            files[5] = Take(PublishedContentCodec.EncodeReleaseSet(new ContentReleaseSet { SchemaVersion = 1, Scope = Scope, ReleaseSetId = Release,
                Binding = ContentBindingRecord.From(prepared.Binding), PublicationReceiptSha256 = PublishedContentCodec.Sha256(files[4]) }, StoreBudget()));
            Reject(files, "RecoveryBlocked", "Source.Payload");
        }
        [Test] public void RecordMathSourceAndCapabilityBudgetsAreEnforced()
        {
            Reject(CopyFiles(), "BudgetExceeded", budget: new ContentStoreBudget(Math(), 1));
            Reject(CopyFiles(), "BudgetExceeded", budget: new ContentStoreBudget(new ExactMathBudget(maxPrimitiveSteps: 0)));
            Reject(CopyFiles(), "BudgetExceeded", caps: new ContentConsumerCapabilities(Caps().Capabilities, maxSourceBytes: 1));
            Reject(CopyFiles(), "UnsupportedCapability", caps: new ContentConsumerCapabilities(new string[0]));
            var storage = Take(Load(CopyFiles()));
            Assert.AreEqual("BudgetExceeded", Assert.Throws<ContentStorageException>(() => storage.Read(records.Keys.First(), 0)).Code);
            Assert.AreEqual("BudgetExceeded", Assert.Throws<ContentStorageException>(() => storage.Read(records.Keys.First(), -1)).Code);
        }
        [Test] public void OtherExplicitReleaseOrLevelDoesNotFallback()
        {
            var catalog = new PublishedContentCatalog(Take(Load(CopyFiles())), Caps());
            Assert.AreEqual("UnsupportedBinding", catalog.GetCurrentBinding(Scope, "fixture:unknown", Caps()).RejectionCode);
            Assert.AreEqual("UnsupportedBinding", catalog.GetCurrentBinding("fixture:other", Release, Caps()).RejectionCode);
            var bad = DefinitionBinding.Prepare(prepared.Binding, "fixture:alpha", 999, new SaveCodecBudget(Math())); Assert.IsTrue(bad.IsAccepted);
            Assert.AreEqual("UnsupportedBinding", catalog.ResolveExact(bad.Value, Caps()).RejectionCode);
        }
        [TestCase("unknown")] [TestCase("duplicate")] [TestCase("source")] [TestCase("review")]
        [TestCase("operation")] [TestCase("mode")] [TestCase("missing")] [TestCase("release")]
        public void CliRejectsUnapprovedArgumentsBeforeAnyWrite(string kind)
        {
            const string project = "D:/Unity/UnityProj/FightMatch", root = project + "/TestArtifacts/FMDemo025P2/p2b-publish";
            var args = new List<string> { "-fmMode", "publish", "-fmEvidenceRoot", root, "-fmSource", project + "/Assets/FightMatchContent/demo-r1.source.json",
                "-fmReview", project + "/docs/system-design/2026-09-17/demo-025-p2b-content-review.json", "-fmStoreRoot", root + "/publication-store",
                "-fmOperationId", "publish:fightmatch-demo-r1:1" };
            var expected = "InvalidValue: Arguments";
            if (kind == "unknown") args.AddRange(new[] { "-fmUnknown", "x" });
            if (kind == "duplicate") args.AddRange(new[] { "-fmMode", "publish" });
            if (kind == "source") { args[5] = project + "/Assets/FightMatchContent/other.json"; expected = "UnsupportedBinding: -fmSource"; }
            if (kind == "review") { args[7] = project + "/docs/system-design/2026-09-17/unreviewed.json"; expected = "UnsupportedBinding: -fmReview"; }
            if (kind == "operation") { args[11] = "fixture:wrong-operation"; expected = "UnsupportedBinding: -fmOperationId"; }
            if (kind == "mode") { args[1] = "unknown"; expected = "UnsupportedBinding: Mode"; }
            if (kind == "missing") args.RemoveAt(args.Count - 1);
            if (kind == "release") { args = new List<string> { "-fmMode", "verify-release", "-fmEvidenceRoot", root, "-fmReleaseRoot", project + "/Assets/StreamingAssets/FightMatch",
                "-fmScope", "player", "-fmReleaseSetId", "fixture:wrong-release" }; expected = "UnsupportedBinding: -fmReleaseSetId"; }
            var before = StoreFiles(root + "/publication-store");
            var run = typeof(PublishedContentAuthoring).GetMethod("RunArguments", BindingFlags.Static | BindingFlags.NonPublic);
            Assert.IsNotNull(run); var error = Assert.Throws<TargetInvocationException>(() => run.Invoke(null, new object[] { args.ToArray() }));
            Assert.AreEqual(expected, error.InnerException.Message); CollectionAssert.AreEqual(before, StoreFiles(root + "/publication-store"));
        }
        [TestCase(0)] [TestCase(1)] [TestCase(2)]
        public void PublishedBytesRemainUnchangedThroughSyncRoot(int kind)
        {
            var catalog = new PublishedContentCatalog(Take(Load(CopyFiles())), Caps());
            var publication = Take(catalog.ResolveExact(prepared.Binding, Caps()));
            var expected = kind == 0 ? six[4] : kind == 1 ? prepared.NewProfile.CanonicalBytes.ToArray()
                : prepared.Replays[0].SeedBytes.ToArray();
            var view = PublicationBytes(publication, kind);
            var escaped = (view as System.Collections.ICollection)?.SyncRoot as byte[];
            if (escaped != null) escaped[0] ^= 127;
            CollectionAssert.AreEqual(expected, PublicationBytes(publication, kind), "Current publication bytes escaped through SyncRoot");
            CollectionAssert.AreEqual(expected, PublicationBytes(Take(catalog.ResolveExact(prepared.Binding, Caps())), kind));
            Assert.IsTrue(Take(catalog.GetCurrentBinding(Scope, Release, Caps())).Same(prepared.Binding));
        }
        private static IReadOnlyList<byte> PublicationBytes(ResolvedPublication publication, int kind)
            => kind == 0 ? publication.ReceiptBytes : kind == 1 ? publication.NewProfile.CanonicalBytes : publication.GetDefaultReferences()[0].SeedBytes;
        [Test] public void EqualCapabilityValuesInNewObjectsPreserveExactContentAndLevel()
        {
            var storage = Take(Load(CopyFiles())); var catalog = new PublishedContentCatalog(storage, Caps());
            var original = Take(catalog.ResolveExact(prepared.Binding, Caps()));
            var values = Caps(); var equal = new ContentConsumerCapabilities(values.Capabilities.ToArray(), values.MaxSourceBytes,
                values.MaxCollectionEntries, values.MaxStringCodeUnits);
            var again = Take(new PublishedContentCatalog(storage, equal).ResolveExact(prepared.Binding, equal));
            Assert.IsTrue(again.Binding.Same(original.Binding)); CollectionAssert.AreEqual(six[4], again.ReceiptBytes);
            CollectionAssert.AreEqual(prepared.NewProfile.CanonicalBytes, again.NewProfile.CanonicalBytes);
            Assert.AreEqual("fixture:alpha", Take(catalog.ResolveExact(prepared.DefinitionBindings[0], equal)).LevelId);
            Assert.IsTrue(Take(catalog.GetCurrentBinding(Scope, Release, equal)).Same(prepared.Binding));
            var reordered = new ContentConsumerCapabilities(values.Capabilities.Reverse());
            CollectionAssert.AreEqual(Take(new PublishedContentCatalog(WritableCopy(), Caps()).ResolveExact(prepared.Binding, reordered)).ReceiptBytes,
                Take(catalog.ResolveExact(prepared.Binding, reordered)).ReceiptBytes);
        }
        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)]
        public void ChangedConsumerConstraintsKeepTheFullPathRejection(int kind)
        {
            var immutable = new PublishedContentCatalog(Take(Load(CopyFiles())), Caps());
            Take(immutable.ResolveExact(prepared.Binding, Caps()));
            var full = new PublishedContentCatalog(WritableCopy(), Caps()); var values = Caps();
            var changed = kind == 4 ? null : new ContentConsumerCapabilities(
                kind == 0 ? values.Capabilities.Take(values.Capabilities.Count - 1) : values.Capabilities,
                kind == 1 ? 1 : values.MaxSourceBytes, kind == 2 ? 1 : values.MaxCollectionEntries,
                kind == 3 ? 1 : values.MaxStringCodeUnits);
            SameRejection(full.ResolveExact(prepared.Binding, changed), immutable.ResolveExact(prepared.Binding, changed));
            SameRejection(full.ResolveExact(prepared.DefinitionBindings[0], changed), immutable.ResolveExact(prepared.DefinitionBindings[0], changed));
            if (changed == null)
            {
                Assert.Throws<NullReferenceException>(() => full.GetCurrentBinding(Scope, Release, changed));
                Assert.Throws<NullReferenceException>(() => immutable.GetCurrentBinding(Scope, Release, changed));
            }
            else SameRejection(full.GetCurrentBinding(Scope, Release, changed), immutable.GetCurrentBinding(Scope, Release, changed));
            CollectionAssert.AreEqual(six[4], Take(immutable.ResolveExact(prepared.Binding, Caps())).ReceiptBytes);
        }
        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)]
        public void EveryChangedBindingFieldStillRejectsAfterSuccessfulRead(int field)
        {
            var immutable = new PublishedContentCatalog(Take(Load(CopyFiles())), Caps());
            Take(immutable.ResolveExact(prepared.Binding, Caps())); var full = new PublishedContentCatalog(WritableCopy(), Caps());
            var b = prepared.Binding; var fields = new[] { b.PackageId, b.ContentFingerprint, b.RuleVersion, b.NumericContractVersion, b.RandomContractVersion };
            fields[field] = field == 1 ? new string('f', 64) : "fixture:other-value";
            var other = ContentBinding.Prepare(fields[0], fields[1], fields[2], fields[3], fields[4], new SaveCodecBudget(Math()));
            Assert.IsTrue(other.IsAccepted);
            SameRejection(full.ResolveExact(other.Value, Caps()), immutable.ResolveExact(other.Value, Caps()));
        }
        [Test] public void NullBindingAndOtherLevelNeverUseTheSuccessfulPublication()
        {
            var immutable = new PublishedContentCatalog(Take(Load(CopyFiles())), Caps());
            Take(immutable.ResolveExact(prepared.Binding, Caps())); var full = new PublishedContentCatalog(WritableCopy(), Caps());
            SameRejection(full.ResolveExact((ContentBinding)null, Caps()), immutable.ResolveExact((ContentBinding)null, Caps()));
            var wrong = DefinitionBinding.Prepare(prepared.Binding, "fixture:missing-level", 1, new SaveCodecBudget(Math()));
            Assert.IsTrue(wrong.IsAccepted);
            SameRejection(full.ResolveExact(wrong.Value, Caps()), immutable.ResolveExact(wrong.Value, Caps()));
        }
        [Test] public void DifferentImmutableInstancesKeepTheirOwnPublication()
        {
            var first = new PublishedContentCatalog(Take(Load(CopyFiles())), Caps());
            var source = Fixture(2); var job = new DemoContentDraft(source.DraftId).BeginJob(); var secondPrepared = Prepare(source, job);
            var memory = new MemoryStorage(); var review = Review(secondPrepared);
            var published = Take(new PublishedContentCatalog(memory, Caps()).Publish(secondPrepared, secondPrepared.Validation, review,
                job, "fixture:second-operation", StoreBudget())).Publication;
            var receipt = published.ReceiptBytes.ToArray();
            var release = Take(PublishedContentCodec.EncodeReleaseSet(new ContentReleaseSet { SchemaVersion = 1, Scope = Scope, ReleaseSetId = Release,
                Binding = ContentBindingRecord.From(secondPrepared.Binding), PublicationReceiptSha256 = PublishedContentCodec.Sha256(receipt) }, StoreBudget()));
            var files = new[] { secondPrepared.SourceBytes.ToArray(), secondPrepared.PayloadBytes.ToArray(), secondPrepared.Validation.Bytes.ToArray(),
                Take(PublishedContentCodec.EncodeReview(review, StoreBudget())), receipt, release };
            var second = new PublishedContentCatalog(Take(Load(files)), Caps());
            CollectionAssert.AreEqual(six[4], Take(first.ResolveExact(prepared.Binding, Caps())).ReceiptBytes);
            CollectionAssert.AreEqual(receipt, Take(second.ResolveExact(secondPrepared.Binding, Caps())).ReceiptBytes);
            Assert.IsTrue(Take(first.GetCurrentBinding(Scope, Release, Caps())).Same(prepared.Binding));
            Assert.IsTrue(Take(second.GetCurrentBinding(Scope, Release, Caps())).Same(secondPrepared.Binding));
            Assert.AreEqual("UnsupportedBinding", first.ResolveExact(secondPrepared.Binding, Caps()).RejectionCode);
            Assert.AreEqual("UnsupportedBinding", second.ResolveExact(prepared.Binding, Caps()).RejectionCode);
        }
        [TestCase("missing")] [TestCase("tampered")] [TestCase("io")]
        public void MutableStorageStillDetectsChangesAfterSuccessfulRead(string change)
        {
            var memory = WritableCopy(); var catalog = new PublishedContentCatalog(memory, Caps());
            Take(catalog.ResolveExact(prepared.Binding, Caps())); Take(catalog.GetCurrentBinding(Scope, Release, Caps()));
            var key = PublishedContentCatalog.Key("source", prepared.SourceSha256);
            if (change == "missing") memory.Blobs.Remove(key);
            if (change == "tampered") memory.Blobs[key][0] ^= 127;
            if (change == "io") memory.ThrowRead = true;
            var rejected = catalog.ResolveExact(prepared.Binding, Caps()); Assert.IsFalse(rejected.IsAccepted);
            Assert.AreEqual(change == "io" ? "Pending" : "RecoveryBlocked", rejected.RejectionCode);
            SameRejection(new PublishedContentCatalog(memory, Caps()).GetCurrentBinding(Scope, Release, Caps()),
                catalog.GetCurrentBinding(Scope, Release, Caps()));
        }
        [Test] public void CompletedPublicationStillUsesItsCallerBudget()
        {
            var catalog = new PublishedContentCatalog(WritableCopy(), Caps()); Take(catalog.ResolveExact(prepared.Binding, Caps()));
            var result = catalog.Publish(prepared, prepared.Validation, Review(prepared), null, Operation,
                new ContentStoreBudget(new ExactMathBudget(maxPrimitiveSteps: 0)));
            Assert.IsFalse(result.IsAccepted); Assert.AreEqual("BudgetExceeded", result.RejectionCode);
        }
        private MemoryStorage WritableCopy()
        { var storage = new MemoryStorage(); foreach (var pair in records) storage.Blobs.Add(pair.Key, (byte[])pair.Value.Clone()); return storage; }
        private static void SameRejection<T>(PublicationResult<T> full, PublicationResult<T> actual)
        { Assert.IsFalse(full.IsAccepted); Assert.IsFalse(actual.IsAccepted); Assert.IsNull(actual.Value);
            Assert.AreEqual(full.RejectionCode, actual.RejectionCode); Assert.AreEqual(full.FieldPath, actual.FieldPath); }
        private static string[] StoreFiles(string path)
        {
            var keys = new[] { "7b8acbc1fa35a17bfc6a0d91cda365950b6a18a84a0993e62bcd23feee447699", "650eb34931383b21667229e89a1425263ac630f69caaa54855017c2ab3d80647",
                "8a0e0e3670bd572145ba1cd91de5cbb94845cbe17b0c956d2b65375b5d1137d6", "1840e0f9165293b29c4079d93332232f322f35513de2afc2acd3d793075aa733",
                "5a3bd91413ade67b7b12d04150b73a50e6af00809078133443cba32d9a3838d1", "52583ff605b83c6d1937084035712b5762af3a76a8932758676edb1bd268cd8e",
                "ad761a08f736d147a3c6b8ba2499fb5864c387912f21d9bd20197c2401b422e7", "74353dfdcc4c823f20aaf05587adbe02291a6153f806cf4a906fc0356f1cdfb0" };
            return keys.SelectMany(k => new[] { k + ".work", k + ".blob" }).Concat(new[] { "writer.lock" }).Select(n => {
                var full = Path.Combine(path, n); if (!File.Exists(full)) return n + ":absent";
                var f = new FileInfo(full); Assert.LessOrEqual(f.Length, 16 * 1024 * 1024);
                return n + ":" + f.Length + ":" + PublishedContentCodec.Sha256(File.ReadAllBytes(full)); }).ToArray();
        }
    }
}
