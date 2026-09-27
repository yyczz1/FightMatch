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
