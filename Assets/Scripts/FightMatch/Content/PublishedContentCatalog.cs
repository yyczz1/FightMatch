using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using FightMatch.Core;
using FightMatch.Platform;
using static FightMatch.Content.ContentChecks;

namespace FightMatch.Content
{
    public sealed class PublishedContentCatalog
    {
        private readonly IContentPublicationStorage storage;
        private readonly ContentConsumerCapabilities capabilities;
        private volatile ReadAdmission readAdmission;
        private sealed class ReadAdmission
        {
            private readonly ContentBinding binding;
            private readonly ContentConsumerCapabilities consumer;
            private readonly byte[] index, payload, source, validation, review;
            private readonly int maxRecordBytes, maxIntegerBits;
            private readonly long suffixSteps;
            internal readonly ResolvedPublication Publication;
            internal ReadAdmission(ContentBinding binding, ContentConsumerCapabilities consumer, ContentStoreBudget budget,
                byte[] index, byte[] payload, byte[] source, byte[] validation, byte[] review, long suffixSteps, ResolvedPublication publication)
            {
                this.binding = binding; this.consumer = consumer; this.index = index; this.payload = payload; this.source = source;
                this.validation = validation; this.review = review; this.suffixSteps = suffixSteps; Publication = publication;
                maxRecordBytes = budget.MaxRecordBytes; maxIntegerBits = budget.Math.MaxIntegerBits;
            }
            internal bool Matches(ContentBinding expected, ContentConsumerCapabilities capabilities, ContentStoreBudget budget,
                byte[] index, byte[] payload, byte[] source, byte[] validation, byte[] review)
            {
                return binding.Same(expected) && maxRecordBytes == budget.MaxRecordBytes && maxIntegerBits == budget.Math.MaxIntegerBits &&
                    (long)budget.Math.MaxPrimitiveSteps - budget.Math.PrimitiveStepsUsed >= suffixSteps &&
                    consumer.MaxSourceBytes == capabilities.MaxSourceBytes && consumer.MaxCollectionEntries == capabilities.MaxCollectionEntries &&
                    consumer.MaxStringCodeUnits == capabilities.MaxStringCodeUnits && consumer.Capabilities.SequenceEqual(capabilities.Capabilities) &&
                    ContentPublicationStorage.Equal(this.index, index) && ContentPublicationStorage.Equal(this.payload, payload) &&
                    ContentPublicationStorage.Equal(this.source, source) && ContentPublicationStorage.Equal(this.validation, validation) &&
                    ContentPublicationStorage.Equal(this.review, review);
            }
        }
        public PublishedContentCatalog(IContentPublicationStorage storage, ContentConsumerCapabilities capabilities)
        { this.storage = storage ?? throw new ArgumentNullException(nameof(storage)); this.capabilities = capabilities ?? throw new ArgumentNullException(nameof(capabilities)); }
        public PublicationResult<ContentPublicationOutcome> Publish(PreparedPublication prepared, ContentValidationEvidence validation,
            ContentReviewEvidence review, DemoContentJob currentJob, string operationId, ContentStoreBudget budget)
        {
            return Io(() => {
                Root(prepared, "Prepared"); Root(validation, "Validation"); Root(review, "Review"); Root(budget, "Budget");
                Text(operationId, "OperationId"); Need(operationId.Length <= capabilities.MaxStringCodeUnits, "BudgetExceeded", "OperationId");
                var reviewBytes = ReviewBytes(review, budget); var frozenReview = PublishedContentCodec.Decode<ContentReviewEvidence>(reviewBytes,
                    Math.Min(budget.MaxRecordBytes, 65536), capabilities, budget.Math);
                Need(validation.Bytes.Count <= budget.MaxRecordBytes && prepared.PayloadBytes.Count <= budget.MaxRecordBytes &&
                    prepared.SourceBytes.Count <= budget.MaxRecordBytes, "BudgetExceeded", "Publication");
                var validationBytes = validation.Bytes.ToArray();
                Need(validation.Sha256 == prepared.Validation.Sha256 && ContentPublicationStorage.Equal(validationBytes, prepared.Validation.Bytes.ToArray()),
                    "InconsistentBinding", "Validation");
                CheckReview(frozenReview, prepared.Binding, prepared.DraftId, prepared.Revision, prepared.SourceBytes.Count, prepared.SourceSha256,
                    prepared.PayloadBytes.Count, prepared.PayloadSha256, validationBytes.Length, validation.Sha256, prepared.DefinitionBindings, budget.Math);
                var record = new PublicationRecord { SchemaVersion = 1, OperationId = operationId, Binding = ContentBindingRecord.From(prepared.Binding),
                    DraftId = prepared.DraftId, Revision = prepared.Revision, SourceSha256 = prepared.SourceSha256,
                    PayloadSha256 = prepared.PayloadSha256, ValidationSha256 = validation.Sha256, ReviewSha256 = PublishedContentCodec.Sha256(reviewBytes) };
                var intent = Bytes(record, budget); var operationKey = Key("operation", operationId); var receiptKey = Key("receipt", operationId);
                var bindingKey = BindingKey(prepared.Binding);
                using (storage.AcquireWriter())
                {
                    var old = storage.Read(operationKey, budget.MaxRecordBytes);
                    if (old != null) Need(ContentPublicationStorage.Equal(old, intent), "OperationConflict", "OperationId");
                    var receipt = storage.Read(receiptKey, budget.MaxRecordBytes); var index = storage.Read(bindingKey, budget.MaxRecordBytes);
                    // Completed facts are queried before the revision/cancellation gate.
                    if (receipt != null && index != null)
                    {
                        Need(old != null && ContentPublicationStorage.Equal(receipt, intent) && ContentPublicationStorage.Equal(index, intent), "RecoveryBlocked", "Receipt");
                        return new ContentPublicationOutcome(operationId, Resolve(prepared.Binding, capabilities, budget));
                    }
                    Root(currentJob, "CurrentJob"); Need(ReferenceEquals(prepared.Job, currentJob), "StaleContext", "Prepared.Job");
                    return currentJob.Commit(() => {
                        currentJob.Check();
                        Put(operationKey, intent, currentJob, budget);
                        Put(Key("source", record.SourceSha256), prepared.SourceBytes.ToArray(), currentJob, budget);
                        Put(Key("payload", record.PayloadSha256), prepared.PayloadBytes.ToArray(), currentJob, budget);
                        Put(Key("validation", record.ValidationSha256), validationBytes, currentJob, budget);
                        Put(Key("review", record.ReviewSha256), reviewBytes, currentJob, budget);
                        Put(receiptKey, intent, currentJob, budget);
                        Put(bindingKey, intent, currentJob, budget);
                        // A durable marker is still not success until every exact record is read back and admitted.
                        return new ContentPublicationOutcome(operationId, Resolve(prepared.Binding, capabilities, budget));
                    });
                }
            });
        }
        public PublicationResult<ResolvedPublication> ResolveExact(ContentBinding binding, ContentConsumerCapabilities consumer)
        { return Io(() => ResolveReadOnly(binding, consumer, new ContentStoreBudget(new ExactMathBudget(maxPrimitiveSteps: 16000000)))); }
        public PublicationResult<PreparedLevel> ResolveExact(DefinitionBinding binding, ContentConsumerCapabilities consumer)
        {
            return Io(() => {
                Root(binding, "DefinitionBinding"); var resolved = ResolveReadOnly(binding.Content, consumer, new ContentStoreBudget(new ExactMathBudget(maxPrimitiveSteps: 16000000)));
                var level = resolved.Definitions.Levels.SingleOrDefault(l => l.LevelId == binding.LevelId && l.LevelVersion == binding.CanonicalLevelVersion);
                Need(level != null, "UnsupportedBinding", "DefinitionBinding"); return level;
            });
        }
        public PublicationResult<ContentBinding> GetCurrentBinding(string scope, string expectedReleaseSetId, ContentConsumerCapabilities consumer)
        { return Io(() => ReadCurrentBinding(scope, expectedReleaseSetId, consumer, out _)); }
        internal PublicationResult<ResolvedPublication> ReadInitialPublication(string scope, string expectedReleaseSetId, ContentConsumerCapabilities consumer)
        {
            return Io(() => {
                ReadCurrentBinding(scope, expectedReleaseSetId, consumer, out var publication); return publication;
            });
        }
        private ContentBinding ReadCurrentBinding(string scope, string expectedReleaseSetId, ContentConsumerCapabilities consumer, out ResolvedPublication publication)
        {
            Text(scope, "Scope"); Text(expectedReleaseSetId, "ReleaseSetId"); var budget = new ContentStoreBudget(new ExactMathBudget(maxPrimitiveSteps: 16000000));
            var bytes = storage.Read(ReleaseSetKey(scope, expectedReleaseSetId), Math.Min(65536, budget.MaxRecordBytes));
            Need(bytes != null, "UnsupportedBinding", "ReleaseSet");
            var set = PublishedContentCodec.Decode<ContentReleaseSet>(bytes, 65536, consumer, budget.Math);
            Need(set.SchemaVersion == 1, "UnsupportedSchema", "ReleaseSet.SchemaVersion");
            Need(set.Scope == scope && set.ReleaseSetId == expectedReleaseSetId && set.Binding != null, "RecoveryBlocked", "ReleaseSet.Identity");
            var binding = set.Binding.Take(budget.Math); publication = ResolveReadOnly(binding, consumer, budget);
            Need(PublishedContentCodec.Sha256(publication.ReceiptBytes.ToArray()) == set.PublicationReceiptSha256, "RecoveryBlocked", "ReleaseSet.Receipt"); return binding;
        }
        private ResolvedPublication ResolveReadOnly(ContentBinding binding, ContentConsumerCapabilities consumer, ContentStoreBudget budget)
        {
            // First-release admission remains fixed by Create. Publish never enables catalog reuse.
            if (storage is FirstReleaseContentStorage first)
                return first.GetAdmitted(binding, consumer) ?? Resolve(binding, consumer, budget);
            return Resolve(binding, consumer, budget, true);
        }
        public static string ReleaseSetKey(string scope, string releaseSetId)
        { return Key("release", PublishedContentCodec.Sha256(PublishedContentCodec.Encode(new { Scope = scope, ReleaseSetId = releaseSetId },
            65536, ContentConsumerCapabilities.Current, new ExactMathBudget()))); }
        public static string BindingKey(ContentBinding binding)
        {
            Root(binding, "Binding"); return Key("binding", PublishedContentCodec.Sha256(PublishedContentCodec.Encode(ContentBindingRecord.From(binding),
                65536, ContentConsumerCapabilities.Current, new ExactMathBudget())));
        }
        public static string Key(string kind, string identity)
        { return PublishedContentCodec.Sha256(PublishedContentCodec.Utf8.GetBytes(kind + "\0" + identity)); }
        private byte[] Read(string key, ContentStoreBudget budget, bool owned = false)
        { var bytes = storage.Read(key, budget.MaxRecordBytes); Need(bytes != null, "RecoveryBlocked", "MissingCommittedRecord"); return owned ? (byte[])bytes.Clone() : bytes; }
        private void Put(string key, byte[] value, DemoContentJob job, ContentStoreBudget budget)
        {
            job.Check(); storage.WriteImmutable(key, value, budget.MaxRecordBytes);
            Need(ContentPublicationStorage.Equal(Read(key, budget), value), "RecoveryBlocked", "ReadBack");
        }
        private ResolvedPublication Resolve(ContentBinding expected, ContentConsumerCapabilities consumer, ContentStoreBudget budget, bool reuse = false)
        {
            var admitted = reuse ? readAdmission : null;
            Root(expected, "Binding"); Root(consumer, "Capabilities"); var index = storage.Read(BindingKey(expected), budget.MaxRecordBytes);
            Need(index != null, "UnsupportedBinding", "Binding"); if (reuse) index = (byte[])index.Clone();
            var r = PublishedContentCodec.Decode<PublicationRecord>(index, 65536, consumer, budget.Math);
            Need(r.SchemaVersion == 1, "UnsupportedSchema", "Receipt.SchemaVersion"); Root(r.Binding, "Receipt.Binding");
            Need(expected.Same(r.Binding.Take(budget.Math)), "RecoveryBlocked", "Receipt.Binding");
            Need(ContentPublicationStorage.Equal(Read(Key("operation", r.OperationId), budget, reuse), index) &&
                ContentPublicationStorage.Equal(Read(Key("receipt", r.OperationId), budget, reuse), index), "RecoveryBlocked", "Receipt.Identity");
            var payload = ReadHash("payload", r.PayloadSha256, budget, reuse); var original = ReadHash("source", r.SourceSha256, budget, reuse);
            var validationBytes = ReadHash("validation", r.ValidationSha256, budget, reuse); var reviewBytes = ReadHash("review", r.ReviewSha256, budget, reuse);
            var suffixStart = budget.Math.PrimitiveStepsUsed;
            if (admitted != null && admitted.Matches(expected, consumer, budget, index, payload, original, validationBytes, reviewBytes))
                return admitted.Publication;
            var source = PublishedContentCodec.Decode<PublishedSource>(payload, consumer.MaxSourceBytes, consumer, budget.Math);
            PublishedContentCompiler.Normalize(source, consumer, budget.Math);
            Need(ContentPublicationStorage.Equal(payload, PublishedContentCodec.Encode(source, consumer.MaxSourceBytes, consumer, budget.Math)), "RecoveryBlocked", "Payload.Canonical");
            var raw = PublishedContentCodec.Decode<PublishedSource>(original, consumer.MaxSourceBytes, consumer, budget.Math);
            PublishedContentCompiler.Normalize(raw, consumer, budget.Math);
            Need(ContentPublicationStorage.Equal(payload, PublishedContentCodec.Encode(raw, consumer.MaxSourceBytes, consumer, budget.Math)), "RecoveryBlocked", "Source.Payload");
            Need(expected.Same(PublishedContentCompiler.Bind(source, payload, budget.Math)) && source.DraftId == r.DraftId && source.Revision == r.Revision,
                "RecoveryBlocked", "Payload.Binding");
            var v = PublishedContentCodec.Decode<ValidationRecord>(validationBytes, 65536, consumer, budget.Math);
            Need(v.SchemaVersion == 1 && v.Binding != null && expected.Same(v.Binding.Take(budget.Math)) && v.DraftId == r.DraftId && v.Revision == r.Revision &&
                v.SourceBytes == original.Length && v.SourceSha256 == r.SourceSha256 && v.PayloadBytes == payload.Length && v.PayloadSha256 == r.PayloadSha256,
                "RecoveryBlocked", "Validation.Binding");
            var job = new DemoContentDraft("exact-read:" + source.DraftId).BeginJob();
            var built = PublishedContentCompiler.Build(source, expected, job, consumer, budget.Math, true);
            Need(SameLevels(v.DefinitionBindings, built.LevelBindings) && v.EvidenceSha256 == PublishedContentCodec.Sha256(PublishedContentCompiler.EvidenceBytes(built.Replays, budget.Math)),
                "RecoveryBlocked", "Validation.Evidence");
            var review = PublishedContentCodec.Decode<ContentReviewEvidence>(reviewBytes, 65536, consumer, budget.Math);
            CheckReview(review, expected, r.DraftId, r.Revision, original.Length, r.SourceSha256, payload.Length, r.PayloadSha256,
                validationBytes.Length, r.ValidationSha256, built.LevelBindings, budget.Math);
            var publication = new ResolvedPublication(built.Definitions, built.Profile, index, built.Replays[0].Candidate.Parameters, built.Replays);
            if (reuse) readAdmission = new ReadAdmission(expected, consumer, budget, index, payload, original, validationBytes, reviewBytes,
                budget.Math.PrimitiveStepsUsed - suffixStart, publication);
            return publication;
        }
        private byte[] ReadHash(string kind, string sha, ContentStoreBudget budget, bool owned = false)
        {
            Need(IsSha(sha), "RecoveryBlocked", kind + ".Sha256"); var bytes = Read(Key(kind, sha), budget, owned);
            Need(PublishedContentCodec.Sha256(bytes) == sha, "RecoveryBlocked", kind + ".Identity"); return bytes;
        }
        private byte[] Bytes(object value, ContentStoreBudget budget) => PublishedContentCodec.Encode(value, Math.Min(65536, budget.MaxRecordBytes), capabilities, budget.Math);
        private byte[] ReviewBytes(ContentReviewEvidence review, ContentStoreBudget budget)
        {
            var canonical = Bytes(review, budget);
            Need(review.OriginalBytes == null || review.OriginalBytes.Length <= Math.Min(65536, budget.MaxRecordBytes), "BudgetExceeded", "Review.OriginalBytes");
            return review.OriginalBytes != null && review.OriginalCanonicalSha == PublishedContentCodec.Sha256(canonical)
                ? (byte[])review.OriginalBytes.Clone() : canonical;
        }
        internal static bool SameLevels(List<DefinitionBindingRecord> records, IReadOnlyList<DefinitionBinding> bindings)
        { return records != null && records.Count == bindings.Count && records.Zip(bindings, (r, b) => r != null && r.LevelId == b.LevelId && r.LevelVersion == b.LevelVersion).All(x => x); }
        internal static bool IsSha(string s) => s != null && s.Length == 64 && s.All(c => c >= '0' && c <= '9' || c >= 'a' && c <= 'f');
        private static void CheckReview(ContentReviewEvidence r, ContentBinding binding, string draft, BigInteger revision,
            int sourceBytes, string sourceSha, int payloadBytes, string payloadSha, int validationBytes, string validationSha,
            IReadOnlyList<DefinitionBinding> levels, ExactMathBudget math)
        {
            Need(r.SchemaVersion == 1, "UnsupportedSchema", "Review.SchemaVersion");
            Need(r.Verdict == "ACCEPT", "ReviewRequired", "Review.Verdict");
            Text(r.AuthorTaskId, "Review.AuthorTaskId"); Text(r.AuthorTurnId, "Review.AuthorTurnId"); Text(r.ReviewerTaskId, "Review.ReviewerTaskId");
            Text(r.ReviewerTurnId, "Review.ReviewerTurnId"); Text(r.ApprovalBasis, "Review.ApprovalBasis");
            Need(r.AuthorTaskId != r.ReviewerTaskId && r.AuthorTurnId != r.ReviewerTurnId && IsSha(r.SourcePacketRangeSha256) && IsSha(r.ApprovedMappingSha256),
                "ReviewRequired", "Review.IndependentAuthority");
            Need(r.Binding != null && binding.Same(r.Binding.Take(math)) && r.DraftId == draft && r.Revision == revision &&
                r.SourceBytes == sourceBytes && r.SourceSha256 == sourceSha && r.PayloadBytes == payloadBytes && r.PayloadSha256 == payloadSha &&
                r.ValidationBytes == validationBytes && r.ValidationSha256 == validationSha && SameLevels(r.DefinitionBindings, levels),
                "InconsistentBinding", "Review.Binding");
        }
        private static PublicationResult<T> Io<T>(Func<T> action)
        {
            try { return PublicationResult<T>.Run(action); }
            catch (ContentStorageException e) { return new PublicationResult<T>(e.Code, "Storage"); }
            catch (IOException) { return new PublicationResult<T>("Pending", "Storage.UnknownIO"); }
            catch (UnauthorizedAccessException) { return new PublicationResult<T>("RecoveryBlocked", "Storage.Access"); }
        }
    }
    internal sealed class PublicationRecord
    {
        public int SchemaVersion { get; set; }
        public string OperationId { get; set; }
        public ContentBindingRecord Binding { get; set; }
        public string DraftId { get; set; }
        public BigInteger Revision { get; set; }
        public string SourceSha256 { get; set; }
        public string PayloadSha256 { get; set; }
        public string ValidationSha256 { get; set; }
        public string ReviewSha256 { get; set; }
    }
}
