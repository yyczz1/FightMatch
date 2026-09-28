using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FightMatch.Content;
using FightMatch.Platform;
using NUnit.Framework;
using static FightMatch.Core.Tests.PublishedContentTestData;

namespace FightMatch.Core.Tests
{
    public class PublishedContentCatalogTests
    {
        private PreparedPublication one;
        private PreparedPublication two;
        private DemoContentJob job;
        [OneTimeSetUp] public void PrepareFixtures()
        { var s = Fixture(); job = new DemoContentDraft(s.DraftId).BeginJob(); one = Prepare(s, job); two = Prepare(Fixture(2, true)); }
        private static PublishedContentCatalog Catalog(MemoryStorage store) => new PublishedContentCatalog(store, Caps());
        private PublicationResult<ContentPublicationOutcome> Publish(MemoryStorage store, string operation = "fixture:op")
            => Catalog(store).Publish(one, one.Validation, Review(one), job, operation, StoreBudget());
        [Test] public void CompletedPublicationReadsOriginalBytesAndDoesNotActivate()
        {
            var store = new MemoryStorage(); var c = Catalog(store); Assert.IsFalse(c.ResolveExact(one.Binding, Caps()).IsAccepted);
            var completed = Take(Publish(store)); Assert.AreEqual("Completed", completed.Status); Assert.AreEqual("fixture:op", completed.OperationId);
            Assert.AreEqual(7, store.Blobs.Count); var original = store.Blobs.ToDictionary(x => x.Key, x => (byte[])x.Value.Clone());
            var again = Take(Publish(store)); Assert.IsTrue(again.Publication.Binding.Same(one.Binding)); Assert.AreEqual(7, store.Writes);
            foreach (var file in original) CollectionAssert.AreEqual(file.Value, store.Blobs[file.Key]);
            var resolved = Take(c.ResolveExact(one.DefinitionBindings.Single(), Caps())); Assert.AreEqual("fixture:alpha", resolved.LevelId);
            CollectionAssert.AreEqual(one.NewProfile.CanonicalBytes, completed.Publication.NewProfile.CanonicalBytes);
            Assert.AreEqual("UnsupportedBinding", c.GetCurrentBinding("fixture:scope", "fixture:release", Caps()).RejectionCode);
        }
        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)] [TestCase(5)] [TestCase(6)] [TestCase(7)] [TestCase(8)] [TestCase(9)]
        public void MissingOrWrongIndependentReviewCannotPublish(int kind)
        {
            var store = new MemoryStorage(); var review = Review(one);
            if (kind == 0) review.Verdict = "UNREVIEWED"; if (kind == 1) review.AuthorTaskId = review.ReviewerTaskId;
            if (kind == 2) review.Revision++; if (kind == 3) review.Binding.ContentFingerprint = new string('b', 64);
            if (kind == 4) review.ValidationSha256 = new string('b', 64); if (kind == 5) review.SourceSha256 = new string('c', 64);
            if (kind == 6) review.DefinitionBindings[0].LevelVersion++; if (kind == 7) review.ApprovalBasis = null;
            if (kind == 8) review.SourcePacketRangeSha256 = null; if (kind == 9) review = null;
            var r = Catalog(store).Publish(one, one.Validation, review, job, "fixture:op", StoreBudget());
            Assert.IsFalse(r.IsAccepted); Assert.IsEmpty(store.Blobs); Assert.AreEqual(0, store.Writes);
        }
        [Test] public void OtherValidationAndOtherJobCannotBeSubstituted()
        {
            var store = new MemoryStorage(); var c = Catalog(store);
            Assert.IsFalse(c.Publish(one, two.Validation, Review(one), job, "op", StoreBudget()).IsAccepted);
            Assert.AreEqual("StaleContext", c.Publish(one, one.Validation, Review(one), new DemoContentDraft(one.DraftId).BeginJob(), "op", StoreBudget()).RejectionCode);
            Assert.IsEmpty(store.Blobs);
        }
        [Test] public void SameOperationDifferentValidIntentConflictsBeforeAnyRewrite()
        {
            var store = new MemoryStorage(); Take(Publish(store)); var c = Catalog(store);
            var r = c.Publish(two, two.Validation, Review(two), null, "fixture:op", StoreBudget());
            Assert.AreEqual("OperationConflict", r.RejectionCode); Assert.AreEqual(7, store.Writes); Assert.AreEqual(7, store.Blobs.Count);
        }
        [TestCase(false)] [TestCase(true)] public void CompletedFactSurvivesLaterCancellationOrRevision(bool cancel)
        {
            var s = Fixture(); var draft = new DemoContentDraft(s.DraftId);
            using (var token = new CancellationTokenSource())
            {
                var owned = draft.BeginJob(token.Token); var prepared = Prepare(s, owned); var store = new MemoryStorage(); var c = Catalog(store);
                Take(c.Publish(prepared, prepared.Validation, Review(prepared), owned, "op", StoreBudget()));
                if (cancel) token.Cancel(); else draft.Revise();
                Assert.AreEqual("Completed", Take(c.Publish(prepared, prepared.Validation, Review(prepared), owned, "op", StoreBudget())).Status);
                Assert.AreEqual(7, store.Writes);
            }
        }
        [TestCase(false)] [TestCase(true)] public void IncompleteCommitMustRecheckCurrentDraftGate(bool cancel)
        {
            var s = Fixture(); var draft = new DemoContentDraft(s.DraftId);
            using (var token = new CancellationTokenSource())
            {
                var owned = draft.BeginJob(token.Token); var p = Prepare(s, owned); var store = new MemoryStorage { FailAt = 3, Fault = "before" }; var c = Catalog(store);
                Assert.AreEqual("Pending", c.Publish(p, p.Validation, Review(p), owned, "op", StoreBudget()).RejectionCode);
                var original = store.Blobs.ToDictionary(x => x.Key, x => (byte[])x.Value.Clone()); store.FailAt = -1;
                if (cancel) token.Cancel(); else draft.Revise();
                Assert.AreEqual(cancel ? "Cancelled" : "StaleContext", c.Publish(p, p.Validation, Review(p), owned, "op", StoreBudget()).RejectionCode);
                foreach (var f in original) CollectionAssert.AreEqual(f.Value, store.Blobs[f.Key]);
                Assert.IsFalse(store.Blobs.ContainsKey(PublishedContentCatalog.BindingKey(p.Binding)));
            }
        }
        [Test] public void RevisionCannotCrossTheSameDraftPublicationSerialGate()
        {
            var s = Fixture(); var draft = new DemoContentDraft(s.DraftId); var owned = draft.BeginJob(); var p = Prepare(s, owned);
            var store = new MemoryStorage(); Task revision = null;
            using (var started = new ManualResetEventSlim())
            {
                store.BeforeWrite = i => { if (i != 2) return; revision = Task.Run(() => { started.Set(); draft.Revise(); });
                    Assert.IsTrue(started.Wait(5000)); Assert.IsFalse(revision.Wait(50), "revision must wait for the same draft gate"); };
                Take(Catalog(store).Publish(p, p.Validation, Review(p), owned, "op", StoreBudget())); Assert.IsTrue(revision.Wait(5000));
            }
            Assert.AreEqual(new BigInteger(2), draft.Revision); Assert.IsTrue(Catalog(store).ResolveExact(p.Binding, Caps()).IsAccepted);
        }
        [Test] public void CancellationDuringIncompleteWritesStopsBeforeCommitMarker()
        {
            var s = Fixture(); using (var token = new CancellationTokenSource())
            {
                var owned = new DemoContentDraft(s.DraftId).BeginJob(token.Token); var p = Prepare(s, owned); var store = new MemoryStorage();
                store.BeforeWrite = i => { if (i == 2) token.Cancel(); };
                var r = Catalog(store).Publish(p, p.Validation, Review(p), owned, "op", StoreBudget());
                Assert.AreEqual("Cancelled", r.RejectionCode); Assert.IsFalse(store.Blobs.ContainsKey(PublishedContentCatalog.BindingKey(p.Binding)));
            }
        }
        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)] [TestCase(5)] [TestCase(6)]
        public void UnknownWriteAtEveryBoundaryRetainsSameOperationAndCanReadOriginalResult(int boundary)
        {
            var store = new MemoryStorage { FailAt = boundary, Fault = "after" }; Assert.AreEqual("Pending", Publish(store).RejectionCode);
            var written = store.Blobs.ToDictionary(x => x.Key, x => (byte[])x.Value.Clone()); store.FailAt = -1;
            Assert.AreEqual("Completed", Take(Publish(store)).Status); foreach (var row in written) CollectionAssert.AreEqual(row.Value, store.Blobs[row.Key]);
        }
        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)] [TestCase(5)] [TestCase(6)]
        public void PartialWriteAtEveryBoundaryIsRetainedAndNeverCompletedOrOverwritten(int boundary)
        {
            var store = new MemoryStorage { FailAt = boundary, Fault = "partial" }; Assert.AreEqual("Pending", Publish(store).RejectionCode);
            var written = store.Blobs.ToDictionary(x => x.Key, x => (byte[])x.Value.Clone()); store.FailAt = -1;
            Assert.IsFalse(Publish(store).IsAccepted); foreach (var row in written) CollectionAssert.AreEqual(row.Value, store.Blobs[row.Key]);
        }
        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)] [TestCase(5)] [TestCase(6)]
        public void BadImmediateReadBackNeverClaimsCompleted(int boundary)
        {
            var store = new MemoryStorage { FailAt = boundary, Fault = "bad-read" }; Assert.AreEqual("RecoveryBlocked", Publish(store).RejectionCode);
            store.FailAt = -1; Assert.AreEqual("Completed", Take(Publish(store)).Status);
        }
        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)]
        public void ExactLookupRequiresEveryBindingField(int field)
        {
            var store = new MemoryStorage(); Take(Publish(store)); var b = one.Binding;
            var parts = new[] { b.PackageId, b.ContentFingerprint, b.RuleVersion, b.NumericContractVersion, b.RandomContractVersion }; parts[field] += ":wrong";
            var changed = ContentBinding.Prepare(parts[0], parts[1], parts[2], parts[3], parts[4], new SaveCodecBudget(Math())); Assert.IsTrue(changed.IsAccepted);
            Assert.AreEqual("UnsupportedBinding", Catalog(store).ResolveExact(changed.Value, Caps()).RejectionCode);
        }
        [TestCase("source")] [TestCase("payload")] [TestCase("validation")] [TestCase("review")] [TestCase("receipt")] [TestCase("operation")]
        public void CorruptCommittedRecordIsNeverMissingOrLatest(string kind)
        {
            var store = new MemoryStorage(); Take(Publish(store)); var identity = kind == "source" ? one.SourceSha256 : kind == "payload" ? one.PayloadSha256 :
                kind == "validation" ? one.Validation.Sha256 : kind == "review" ? PublishedContentCodec.Sha256(Take(PublishedContentCodec.EncodeReview(Review(one), StoreBudget()))) : "fixture:op";
            store.Blobs[PublishedContentCatalog.Key(kind, identity)][0] ^= 1;
            Assert.AreEqual("RecoveryBlocked", Catalog(store).ResolveExact(one.Binding, Caps()).RejectionCode);
        }
        [Test] public void MissingCommittedRecordAndUnknownIoCannotBecomeNoSave()
        {
            var store = new MemoryStorage(); Take(Publish(store)); store.Blobs.Remove(PublishedContentCatalog.Key("validation", one.Validation.Sha256));
            Assert.AreEqual("RecoveryBlocked", Catalog(store).ResolveExact(one.Binding, Caps()).RejectionCode);
            store.ThrowRead = true; Assert.AreEqual("Pending", Catalog(store).ResolveExact(one.Binding, Caps()).RejectionCode);
        }
        [Test] public void UnsupportedCapabilityAndUnknownReceiptSchemaAreRejected()
        {
            var store = new MemoryStorage(); Take(Publish(store)); var c = Catalog(store);
            Assert.AreEqual("UnsupportedCapability", c.ResolveExact(one.Binding, new ContentConsumerCapabilities(new string[0])).RejectionCode);
            var key = PublishedContentCatalog.BindingKey(one.Binding); store.Blobs[key] = Encoding.UTF8.GetBytes(Encoding.UTF8.GetString(store.Blobs[key]).Replace("\"SchemaVersion\":1", "\"SchemaVersion\":99"));
            Assert.AreEqual("UnsupportedSchema", c.ResolveExact(one.Binding, Caps()).RejectionCode);
        }
        [Test] public void MultiLevelV2AndOldV1UseSameCatalogAndExactOldDeletionFails()
        {
            var store = new MemoryStorage(); Take(Publish(store)); var s = Fixture(2, true); var owned = new DemoContentDraft(s.DraftId).BeginJob(); var p = Prepare(s, owned); var c = Catalog(store);
            Take(c.Publish(p, p.Validation, Review(p), owned, "fixture:v2-op", StoreBudget()));
            Assert.AreEqual(2, Take(c.ResolveExact(p.Binding, Caps())).Definitions.Levels.Count);
            foreach (var d in p.DefinitionBindings) Assert.AreEqual("2", Take(c.ResolveExact(d, Caps())).LevelVersion);
            Assert.AreEqual("1", Take(c.ResolveExact(one.DefinitionBindings[0], Caps())).LevelVersion);
            var wrong = DefinitionBinding.Prepare(one.Binding, "fixture:alpha", 2, new SaveCodecBudget(Math())); Assert.IsTrue(wrong.IsAccepted);
            Assert.AreEqual("UnsupportedBinding", c.ResolveExact(wrong.Value, Caps()).RejectionCode);
            store.Blobs.Remove(PublishedContentCatalog.BindingKey(one.Binding)); Assert.AreEqual("UnsupportedBinding", c.ResolveExact(one.Binding, Caps()).RejectionCode);
            Assert.AreEqual(2, Take(c.ResolveExact(p.Binding, Caps())).Definitions.Levels.Count);
        }
        [Test] public void OnlyExplicitExactReleaseSetChoosesCurrentAndReceiptMustMatch()
        {
            var store = new MemoryStorage(); var publication = Take(Publish(store)); var c = Catalog(store);
            var set = new ContentReleaseSet { SchemaVersion = 1, Scope = "fixture:scope", ReleaseSetId = "fixture:release",
                Binding = ContentBindingRecord.From(one.Binding), PublicationReceiptSha256 = PublishedContentCodec.Sha256(publication.Publication.ReceiptBytes.ToArray()) };
            var key = PublishedContentCatalog.ReleaseSetKey(set.Scope, set.ReleaseSetId);
            store.Blobs.Add(key, Take(PublishedContentCodec.EncodeReleaseSet(set, StoreBudget())));
            Assert.IsTrue(Take(c.GetCurrentBinding(set.Scope, set.ReleaseSetId, Caps())).Same(one.Binding));
            Assert.AreEqual("UnsupportedBinding", c.GetCurrentBinding(set.Scope, "other", Caps()).RejectionCode);
            set.PublicationReceiptSha256 = new string('c', 64); store.Blobs[key] = Take(PublishedContentCodec.EncodeReleaseSet(set, StoreBudget()));
            Assert.AreEqual("RecoveryBlocked", c.GetCurrentBinding(set.Scope, set.ReleaseSetId, Caps()).RejectionCode);
        }
        [Test] public void ReviewOriginalJsonBytesArePreservedAndTinyStoreBudgetWritesNothing()
        {
            var canonical = Take(PublishedContentCodec.EncodeReview(Review(one), StoreBudget())); var raw = Encoding.UTF8.GetBytes(" \n" + Encoding.UTF8.GetString(canonical) + "\n");
            var review = Take(PublishedContentCodec.DecodeReview(raw, StoreBudget())); var store = new MemoryStorage(); var c = Catalog(store);
            Assert.IsFalse(c.Publish(one, one.Validation, review, job, "op", new ContentStoreBudget(Math(), 10)).IsAccepted); Assert.IsEmpty(store.Blobs);
            Take(c.Publish(one, one.Validation, review, job, "op", StoreBudget()));
            CollectionAssert.AreEqual(raw, store.Blobs[PublishedContentCatalog.Key("review", PublishedContentCodec.Sha256(raw))]);
        }
        [Test] public void WindowsExactAuthorizedProbeUsesRealLeaseFlushPromotionAndReadBack()
        {
            var root = LocalSaveTestFiles.IsMac ? LocalSaveTestFiles.NewCase() : "D:/Unity/UnityProj/FightMatch/TestArtifacts/FMDemo025P2/p2b-prepare/io-probe";
            var beforeBlob = File.Exists(root + "/" + new string('a', 64) + ".blob"); var key = new string('a', 64); var bytes = new byte[] { 0, 1, 2, 3 };
            Func<IContentPublicationStorage> create = () => LocalSaveTestFiles.IsMac ? (IContentPublicationStorage)new MacContentPublicationStorage(root) : new WindowsContentPublicationStorage(root);
            var store = create(); TestContext.WriteLine("Physical backend: " + store.GetType().FullName);
            using (store.AcquireWriter())
            {
                Assert.Throws<IOException>(() => create().AcquireWriter());
                store.WriteImmutable(key, bytes, 4); CollectionAssert.AreEqual(bytes, store.Read(key, 4));
                store.WriteImmutable(key, bytes, 4); CollectionAssert.AreEqual(bytes, store.Read(key, 4));
                Assert.Throws<ContentStorageException>(() => store.WriteImmutable(key, new byte[] { 3, 2, 1, 0 }, 4));
            }
            Assert.AreEqual(0, new FileInfo(root + "/writer.lock").Length); Assert.AreEqual(4, new FileInfo(root + "/" + key + ".blob").Length);
            Assert.IsFalse(File.Exists(root + "/" + key + ".work")); CollectionAssert.AreEqual(bytes, File.ReadAllBytes(root + "/" + key + ".blob"));
            TestContext.WriteLine(beforeBlob ? "Windows probe retained original exact blob and checked idempotency; no overwrite." : "Windows probe created work, flushed, promoted without replace and read 00010203; lease exclusive.");
        }
        [TestCase(false)] [TestCase(true)]
        public void NullPublishedIndexOrReleaseIsRejectedWithoutWritesOrFallback(bool release)
        {
            var store = new MemoryStorage(); var c = Catalog(store); var first = Take(Publish(store));
            var laterSource = Fixture(2); var laterJob = new DemoContentDraft(laterSource.DraftId).BeginJob(); var later = Prepare(laterSource, laterJob);
            var second = Take(c.Publish(later, later.Validation, Review(later), laterJob, "fixture:later-op", StoreBudget()));
            var set = new ContentReleaseSet { SchemaVersion = 1, Scope = "fixture:scope", ReleaseSetId = "fixture:target",
                Binding = ContentBindingRecord.From(one.Binding), PublicationReceiptSha256 = PublishedContentCodec.Sha256(first.Publication.ReceiptBytes.ToArray()) };
            var releaseKey = PublishedContentCatalog.ReleaseSetKey(set.Scope, set.ReleaseSetId);
            store.Blobs.Add(releaseKey, Take(PublishedContentCodec.EncodeReleaseSet(set, StoreBudget())));
            set.ReleaseSetId = "fixture:later"; set.Binding = ContentBindingRecord.From(later.Binding);
            set.PublicationReceiptSha256 = PublishedContentCodec.Sha256(second.Publication.ReceiptBytes.ToArray());
            store.Blobs.Add(PublishedContentCatalog.ReleaseSetKey(set.Scope, set.ReleaseSetId), Take(PublishedContentCodec.EncodeReleaseSet(set, StoreBudget())));
            store.Blobs[release ? releaseKey : PublishedContentCatalog.BindingKey(one.Binding)] = Encoding.UTF8.GetBytes("null");
            var original = store.Blobs.ToDictionary(x => x.Key, x => (byte[])x.Value.Clone()); var writes = store.Writes;
            var accepted = true; string code = null; string field = null;
            Assert.DoesNotThrow(() => {
                if (release) { var r = c.GetCurrentBinding("fixture:scope", "fixture:target", Caps()); accepted = r.IsAccepted; code = r.RejectionCode; field = r.FieldPath; }
                else { var r = c.ResolveExact(one.Binding, Caps()); accepted = r.IsAccepted; code = r.RejectionCode; field = r.FieldPath; }
            });
            Assert.IsFalse(accepted); Assert.AreEqual("InvalidSchema", code); Assert.AreEqual("Root", field);
            Assert.IsTrue(Take(c.ResolveExact(later.Binding, Caps())).Binding.Same(later.Binding));
            Assert.IsTrue(Take(c.GetCurrentBinding("fixture:scope", "fixture:later", Caps())).Same(later.Binding));
            Assert.AreEqual(writes, store.Writes); CollectionAssert.AreEquivalent(original.Keys, store.Blobs.Keys);
            foreach (var item in original) CollectionAssert.AreEqual(item.Value, store.Blobs[item.Key]);
        }
        [TestCase("../escape")] [TestCase("C:/escape")] [TestCase("abc:stream")] [TestCase("*")]
        public void StorageRejectsUnsafeKeysWithoutWriting(string key)
        { Assert.Throws<ArgumentException>(() => ContentPublicationStorage.CheckKey(key)); }
    }
}
