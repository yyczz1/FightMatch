using System;
using System.Collections.Generic;
using System.Linq;
using FightMatch.Core;
using FightMatch.Platform;
using static FightMatch.Content.ContentChecks;

namespace FightMatch.Content
{
    // Owns six bounded byte arrays. File loading and platform paths belong to the caller.
    public sealed class FirstReleaseContentStorage : IContentPublicationStorage
    {
        private readonly Dictionary<string, byte[]> records;
        // Assigned only by Create, after complete admission and before this instance is returned.
        private ResolvedPublication admitted;
        private ContentConsumerCapabilities admittedCapabilities;
        private FirstReleaseContentStorage(Dictionary<string, byte[]> records) { this.records = records; }
        public static PublicationResult<FirstReleaseContentStorage> Create(byte[] source, byte[] payload, byte[] validation,
            byte[] review, byte[] publication, byte[] release, ContentConsumerCapabilities capabilities, ContentStoreBudget budget)
        {
            return PublicationResult<FirstReleaseContentStorage>.Run(() => {
                Root(capabilities, "Capabilities"); Root(budget, "Budget");
                var files = new[] { source, payload, validation, review, publication, release }; long total = 0;
                for (var i = 0; i < files.Length; i++)
                {
                    Need(files[i] != null && files[i].Length > 0, "MissingField", "FirstRelease.Files[" + i + "]");
                    Need(files[i].Length <= Math.Min(16 * 1024 * 1024, budget.MaxRecordBytes), "BudgetExceeded", "FirstRelease.Files[" + i + "]");
                    total += files[i].Length; Need(total <= 32 * 1024 * 1024, "BudgetExceeded", "FirstRelease.Total");
                    files[i] = (byte[])files[i].Clone();
                }
                var record = PublishedContentCodec.Decode<PublicationRecord>(files[4], Math.Min(65536, budget.MaxRecordBytes), capabilities, budget.Math);
                var set = PublishedContentCodec.Decode<ContentReleaseSet>(files[5], Math.Min(65536, budget.MaxRecordBytes), capabilities, budget.Math);
                Need(record.SchemaVersion == 1 && set.SchemaVersion == 1, "UnsupportedSchema", "FirstRelease.SchemaVersion");
                Text(record.OperationId, "Receipt.OperationId"); Root(record.Binding, "Receipt.Binding");
                Text(set.Scope, "ReleaseSet.Scope"); Text(set.ReleaseSetId, "ReleaseSet.ReleaseSetId");
                var binding = record.Binding.Take(budget.Math);
                var keys = new[] { PublishedContentCatalog.Key("source", record.SourceSha256), PublishedContentCatalog.Key("payload", record.PayloadSha256),
                    PublishedContentCatalog.Key("validation", record.ValidationSha256), PublishedContentCatalog.Key("review", record.ReviewSha256),
                    PublishedContentCatalog.Key("operation", record.OperationId), PublishedContentCatalog.Key("receipt", record.OperationId),
                    PublishedContentCatalog.BindingKey(binding), PublishedContentCatalog.ReleaseSetKey(set.Scope, set.ReleaseSetId) };
                var indexes = new[] { 0, 1, 2, 3, 4, 4, 4, 5 }; var map = new Dictionary<string, byte[]>(StringComparer.Ordinal);
                for (var i = 0; i < keys.Length; i++) { Need(!map.ContainsKey(keys[i]), "RecoveryBlocked", "FirstRelease.Keys"); map.Add(keys[i], files[indexes[i]]); }
                var storage = new FirstReleaseContentStorage(map);
                // The same Catalog owns hash, canonical source, review, receipt, capability and replay admission.
                var admitted = new PublishedContentCatalog(storage, capabilities).ReadInitialPublication(set.Scope, set.ReleaseSetId, capabilities);
                Need(admitted.IsAccepted, admitted.RejectionCode, admitted.FieldPath);
                Need(binding.Same(admitted.Value.Binding), "RecoveryBlocked", "FirstRelease.Binding");
                storage.admitted = admitted.Value; storage.admittedCapabilities = capabilities; return storage;
            });
        }
        internal ResolvedPublication GetAdmitted(ContentBinding binding, ContentConsumerCapabilities consumer)
        {
            if (admitted == null || binding == null || consumer == null) return null;
            return admitted.Binding.Same(binding) && consumer.MaxSourceBytes == admittedCapabilities.MaxSourceBytes &&
                consumer.MaxCollectionEntries == admittedCapabilities.MaxCollectionEntries &&
                consumer.MaxStringCodeUnits == admittedCapabilities.MaxStringCodeUnits &&
                consumer.Capabilities.SequenceEqual(admittedCapabilities.Capabilities) ? admitted : null;
        }
        public byte[] Read(string key, int maxBytes)
        {
            ContentPublicationStorage.CheckKey(key);
            if (maxBytes < 0) throw new ContentStorageException("BudgetExceeded", "Negative read budget");
            if (!records.TryGetValue(key, out var bytes)) return null;
            if (bytes.Length > maxBytes) throw new ContentStorageException("BudgetExceeded", "First release record exceeds read budget");
            return (byte[])bytes.Clone();
        }
        public IDisposable AcquireWriter() { throw new ContentStorageException("ReadOnly", "First release content is immutable"); }
        public void WriteImmutable(string key, byte[] bytes, int maxBytes) { throw new ContentStorageException("ReadOnly", "First release content is immutable"); }
    }
}
