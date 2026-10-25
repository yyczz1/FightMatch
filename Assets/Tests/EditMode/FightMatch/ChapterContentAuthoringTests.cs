using System.Linq;
using System.Text;
using FightMatch.Content;
using NUnit.Framework;

namespace FightMatch.Core.Tests
{
    public sealed class ChapterContentAuthoringTests
    {
        private PreparedPublication prepared;
        private System.Collections.Generic.Dictionary<string, byte[]> files;

        [OneTimeSetUp]
        public void PrepareTheActualFrozenInputsWithoutPublishing()
        {
            var result = ChapterContentAuthoring.Prepare();
            Assert.IsTrue(result.IsAccepted, result.RejectionCode + " " + result.FieldPath);
            prepared = result.Prepared; files = ChapterContentAuthoring.PreparationFiles(prepared);
        }

        [Test]
        public void ActualR2EvidenceMatchesValidationAndDoesNotClaimBattleReplay()
        {
            Assert.AreEqual("fightmatch:chapter-one-trial", prepared.Binding.PackageId);
            Assert.AreEqual("2", prepared.Revision.ToString());
            Assert.AreEqual(9, prepared.DefinitionBindings.Count);
            Assert.AreEqual(ChapterParameterStatus.Trial, prepared.ParameterStatus);
            Assert.AreEqual(8, files.Count);
            CollectionAssert.AreEqual(prepared.SourceBytes, files["source-copy.json"]);
            CollectionAssert.AreEqual(prepared.PayloadBytes, files["prepared.fmpackage.bytes"]);
            Assert.AreEqual(2, prepared.Definitions.Growths.Count);
            var fieldMap = Encoding.UTF8.GetString(files["field-map.json"]);
            StringAssert.Contains("\"Growths\":", fieldMap);
            StringAssert.DoesNotContain("\"Growth\":", fieldMap);
            Assert.Throws<System.InvalidOperationException>(() => prepared.Definitions.Growth.ToString());
            var validation = PublishedContentCodec.Decode<ValidationRecord>(files["prepared.fmvalidation.bytes"], 65536,
                ContentConsumerCapabilities.Current, ChapterContentTestData.Math());
            Assert.AreEqual(validation.EvidenceSha256, PublishedContentCodec.Sha256(files["chapter-evidence.json"]));
            var evidence = Encoding.UTF8.GetString(files["chapter-evidence.json"]);
            StringAssert.Contains("\"BattleReplayPerformed\":false", evidence);
            StringAssert.Contains("\"Geometry\":", evidence);
            StringAssert.Contains("\"Parameters\":", evidence);
            var summary = Encoding.UTF8.GetString(files["result.json"]);
            StringAssert.Contains("\"ExactParameterRows\":31", summary);
            StringAssert.Contains("\"GeometryValidatedLevels\":9", summary);
        }

        [Test]
        public void ActualPreparationTemplateCannotPublishOrWriteAnyCatalogRecord()
        {
            var review = PublishedContentTestData.Take(PublishedContentCodec.DecodeReview(files["review-template.json"], PublishedContentTestData.StoreBudget()));
            Assert.AreEqual("UNREVIEWED", review.Verdict); Assert.IsNull(review.ReviewerTaskId); Assert.IsNull(review.ApprovalBasis);
            var storage = new PublishedContentTestData.MemoryStorage();
            var catalog = new PublishedContentCatalog(storage, ContentConsumerCapabilities.Current);
            var result = catalog.Publish(prepared, prepared.Validation, review, prepared.Job, "fixture:refused-real-publish", PublishedContentTestData.StoreBudget());
            Assert.IsFalse(result.IsAccepted); Assert.AreEqual("ReviewRequired", result.RejectionCode);
            Assert.AreEqual(0, storage.Writes); Assert.IsEmpty(storage.Blobs);
        }

        [Test]
        public void FixedEntryRejectsAlternateSourceArgumentsBeforeAnyOperation()
        {
            Assert.Throws<ContentFailure>(() => ChapterContentAuthoring.RunArguments(new[] {
                "-fmChapterMode", "publish", "-fmSource", "fixture:other" }));
            Assert.Throws<ContentFailure>(() => ChapterContentAuthoring.RunArguments(new[] { "-fmChapterMode", "other" }));
        }

        [Test]
        public void IsolatedFixtureExportUsesExactReviewAndAllSixFilesForColdAdmission()
        {
            var source = ChapterContentTestData.Source(); var job = new DemoContentDraft(source.DraftId).BeginJob();
            var fixture = ChapterContentTestData.Prepare(source, job);
            var review = PublishedContentTestData.Review(fixture); // The helper refuses every real package identity.
            var store = new PublishedContentTestData.MemoryStorage(); var catalog = new PublishedContentCatalog(store, ContentConsumerCapabilities.Current);
            review.PayloadSha256 = new string('f', 64);
            var wrong = catalog.Publish(fixture, fixture.Validation, review, job, "fixture:chapter-export", PublishedContentTestData.StoreBudget());
            Assert.IsFalse(wrong.IsAccepted); Assert.AreEqual("InconsistentBinding", wrong.RejectionCode); Assert.AreEqual(0, store.Writes);
            review = PublishedContentTestData.Review(fixture);
            var outcome = PublishedContentTestData.Take(catalog.Publish(fixture, fixture.Validation, review, job,
                "fixture:chapter-export", PublishedContentTestData.StoreBudget()));
            var receipt = outcome.Publication.ReceiptBytes.ToArray();
            var record = PublishedContentCodec.Decode<PublicationRecord>(receipt, 65536, ContentConsumerCapabilities.Current, ChapterContentTestData.Math());
            var release = PublishedContentTestData.Take(PublishedContentCodec.EncodeReleaseSet(new ContentReleaseSet { SchemaVersion = 1,
                Scope = "fixture:player", ReleaseSetId = source.Chapter.ReleaseSetId, Binding = ContentBindingRecord.From(fixture.Binding),
                PublicationReceiptSha256 = PublishedContentCodec.Sha256(receipt) }, PublishedContentTestData.StoreBudget()));
            var reviewBytes = store.Read(PublishedContentCatalog.Key("review", record.ReviewSha256), 65536);
            var cold = FirstReleaseContentStorage.Create(fixture.SourceBytes.ToArray(), fixture.PayloadBytes.ToArray(), fixture.Validation.Bytes.ToArray(),
                reviewBytes, receipt, release, ContentConsumerCapabilities.Current, PublishedContentTestData.StoreBudget());
            Assert.IsTrue(cold.IsAccepted, cold.RejectionCode + " " + cold.FieldPath);
            var reopened = PublishedContentTestData.Take(new PublishedContentCatalog(cold.Value, ContentConsumerCapabilities.Current)
                .ResolveExact(fixture.Binding, ContentConsumerCapabilities.Current));
            Assert.IsTrue(fixture.Binding.Same(reopened.Binding)); Assert.AreEqual(2, reopened.Definitions.Growths.Count);
            var missing = FirstReleaseContentStorage.Create(fixture.SourceBytes.ToArray(), fixture.PayloadBytes.ToArray(), fixture.Validation.Bytes.ToArray(),
                null, receipt, release, ContentConsumerCapabilities.Current, PublishedContentTestData.StoreBudget());
            Assert.IsFalse(missing.IsAccepted); Assert.AreEqual("MissingField", missing.RejectionCode);
        }
    }
}
