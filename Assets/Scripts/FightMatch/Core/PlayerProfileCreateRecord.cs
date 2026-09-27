using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Security.Cryptography;
using static FightMatch.Core.BusinessFields;

namespace FightMatch.Core
{
    public sealed class PlayerProfileCreateMaterial
    {
        public string Kind { get; }
        public string Name { get; }
        public string SourceCapabilityId { get; }
        public IReadOnlyList<byte> ValueBytes { get; }
        public PlayerProfileCreateMaterial(string kind, string name, string sourceCapabilityId, byte[] valueBytes, SaveCodecBudget budget)
        {
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            var f = new BusinessFields(Stream.Null, false, budget) { StrictUnicode = true };
            Kind = f.Text(kind, "Material.Kind"); Name = f.Text(name, "Material.Name");
            SourceCapabilityId = f.Text(sourceCapabilityId, "Material.SourceCapabilityId");
            Need(valueBytes != null, "Material.ValueBytes", "MissingField"); f.Space((ulong)valueBytes.Length, "Material.ValueBytes");
            ValueBytes = Array.AsReadOnly((byte[])valueBytes.Clone());
        }
    }

    public sealed class PlayerProfileCreateRecord
    {
        public int RecordFormatVersion { get; }
        public string PlayerId { get; }
        public string OperationId { get; }
        public ContentBinding ContentBinding { get; }
        public string NewProfileDefinitionId { get; }
        public BigInteger NewProfileDefinitionVersion { get; }
        public IReadOnlyList<byte> FrozenNewProfileDefinitionBytes { get; }
        public int IntentFormatVersion => RecordFormatVersion + 1;
        public IReadOnlyList<byte> CanonicalInitializeIntentBytes { get; }
        public string IntentSha256 { get; }
        public string DefinitionSha256 { get; }
        public IReadOnlyList<PlayerProfileCreateMaterial> GeneratedMaterials { get; }
        public string RecordSha256 { get; }
        internal PlayerProfileCreateRecord(string player, string operation, ContentBinding binding, string definitionId,
            BigInteger version, byte[] definition, byte[] intent, string intentSha, string definitionSha, string recordSha, int recordFormat = 1)
        {
            RecordFormatVersion = recordFormat; PlayerId = player; OperationId = operation; ContentBinding = binding; NewProfileDefinitionId = definitionId;
            NewProfileDefinitionVersion = version; FrozenNewProfileDefinitionBytes = Array.AsReadOnly(definition);
            CanonicalInitializeIntentBytes = Array.AsReadOnly(intent); IntentSha256 = intentSha; DefinitionSha256 = definitionSha;
            GeneratedMaterials = Array.AsReadOnly(new PlayerProfileCreateMaterial[0]); RecordSha256 = recordSha;
        }
    }

    public sealed class PlayerProfileInitializationCommit
    {
        public string RecordSha256 { get; }
        public string PlayerId { get; }
        public string OperationId { get; }
        public ContentBinding ContentBinding { get; }
        public string NewProfileDefinitionId { get; }
        public BigInteger NewProfileDefinitionVersion { get; }
        public string IntentSha256 { get; }
        public string OriginalInitializationCommitId { get; }
        public SnapshotDescriptor ObservedHeadDescriptor { get; }
        internal PlayerProfileInitializationCommit(PlayerProfileCreateRecord record, CandidateApplicationRecord initialization, SnapshotDescriptor head)
        {
            RecordSha256 = record.RecordSha256; PlayerId = record.PlayerId; OperationId = record.OperationId;
            ContentBinding = record.ContentBinding; NewProfileDefinitionId = record.NewProfileDefinitionId;
            NewProfileDefinitionVersion = record.NewProfileDefinitionVersion; IntentSha256 = record.IntentSha256;
            OriginalInitializationCommitId = initialization.CommitId; ObservedHeadDescriptor = head;
        }
    }

    public static class PlayerProfileCreateRecordCodec
    {
        public static SaveCodecResult<PlayerProfileCreateRecord> Freeze(PreparedCandidateApplicationIntent intent,
            string newProfileDefinitionId, BigInteger newProfileDefinitionVersion, byte[] canonicalNewProfileDefinition,
            IReadOnlyList<PlayerProfileCreateMaterial> generatedMaterials, SaveCodecBudget budget)
        {
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            return SaveCodecResult<PlayerProfileCreateRecord>.Run(() =>
            {
                Need(intent != null, "CreateRecord.Intent", "MissingField");
                var original = CandidateApplicationIntentCodec.ReadFrozenPublishedInitialize(intent.Bytes, budget);
                SaveEnvelopeCodec.CheckList(generatedMaterials, budget, "CreateRecord.GeneratedMaterials");
                Need(generatedMaterials.Count == 0, "CreateRecord.GeneratedMaterials", "InconsistentCreateIntent");
                var f = new BusinessFields(Stream.Null, false, budget) { StrictUnicode = true };
                f.Text(newProfileDefinitionId, "CreateRecord.NewProfileDefinitionId");
                f.Integer(newProfileDefinitionVersion, "CreateRecord.NewProfileDefinitionVersion", 1);
                Need(canonicalNewProfileDefinition != null && canonicalNewProfileDefinition.Length > 0, "CreateRecord.Definition", "MissingField");
                f.Space((ulong)canonicalNewProfileDefinition.Length, "CreateRecord.Definition");
                var provisional = new PlayerProfileCreateRecord(original.PlayerId, original.OperationId,
                    ((PreparedPublishedRuleContext)original.Context).Binding, newProfileDefinitionId, newProfileDefinitionVersion,
                    canonicalNewProfileDefinition, original.Bytes, Hash(original.Bytes), Hash(canonicalNewProfileDefinition), null, (int)original.FormatVersion - 1);
                // Measure the whole record, including its digest, before allocating any copied body.
                Measure(provisional, budget, true);
                var digest = Hash(Encode(provisional, budget, false));
                var frozen = new PlayerProfileCreateRecord(provisional.PlayerId, provisional.OperationId, provisional.ContentBinding,
                    newProfileDefinitionId, newProfileDefinitionVersion, (byte[])canonicalNewProfileDefinition.Clone(),
                    (byte[])original.Bytes.Clone(), provisional.IntentSha256, provisional.DefinitionSha256, digest, provisional.RecordFormatVersion);
                return Take(Read(Encode(frozen, budget, true), budget), "CreateRecord");
            });
        }

        public static SaveCodecResult<byte[]> Write(PlayerProfileCreateRecord record, SaveCodecBudget budget)
        {
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            return SaveCodecResult<byte[]>.Run(() =>
            {
                Need(record != null, "CreateRecord", "MissingField");
                var bytes = Encode(record, budget, true); Take(Read(bytes, budget), "CreateRecord"); return bytes;
            });
        }

        public static SaveCodecResult<PlayerProfileCreateRecord> Read(byte[] encoded, SaveCodecBudget budget)
        {
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            return SaveCodecResult<PlayerProfileCreateRecord>.Run(() =>
            {
                Need(encoded != null, "CreateRecord", "MissingField");
                SaveCodecFailure.Limit((ulong)encoded.Length, budget.MaxEnvelopeBytes, "CreateRecord", "EnvelopeBytes");
                PlayerProfileCreateRecord value;
                using (var stream = new MemoryStream(encoded, false))
                {
                    var f = new BusinessFields(stream, true, budget, (ulong)encoded.Length) { StrictUnicode = true };
                    value = Visit(f, null, true); f.End("CreateRecord");
                }
                var intent = CandidateApplicationIntentCodec.ReadFrozenPublishedInitialize(Copy(value.CanonicalInitializeIntentBytes), budget);
                Need(intent.FormatVersion == value.IntentFormatVersion && intent.PlayerId == value.PlayerId && intent.OperationId == value.OperationId &&
                    value.ContentBinding.Same(((PreparedPublishedRuleContext)intent.Context).Binding), "CreateRecord.Intent.Identity", "InconsistentCreateIntent");
                Need(value.IntentSha256 == Hash(value.CanonicalInitializeIntentBytes) && value.DefinitionSha256 == Hash(value.FrozenNewProfileDefinitionBytes),
                    "CreateRecord.PayloadSha256", "InconsistentCreateIntent");
                Need(value.RecordSha256 == Hash(Encode(value, budget, false)), "CreateRecord.RecordSha256", "CorruptCreateRecord");
                Need(CandidateApplicationIntentCodec.SameBytes(encoded, Encode(value, budget, true)), "CreateRecord.Canonical", "CorruptCreateRecord");
                return value;
            });
        }

        public static SaveCodecResult<PreparedCandidateApplicationIntent> RestoreIntent(PlayerProfileCreateRecord record, SaveCodecBudget budget)
        {
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            return SaveCodecResult<PreparedCandidateApplicationIntent>.Run(() =>
            {
                var checkedRecord = Take(Read(Take(Write(record, budget), "CreateRecord"), budget), "CreateRecord");
                return CandidateApplicationIntentCodec.ReadFrozenPublishedInitialize(Copy(checkedRecord.CanonicalInitializeIntentBytes), budget);
            });
        }

        public static SaveCodecResult<PlayerProfileInitializationCommit> VerifyInitializationCommit(PlayerProfileCreateRecord record,
            CandidateApplicationSnapshot verifiedSnapshot, SaveCodecBudget budget)
        {
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            return SaveCodecResult<PlayerProfileInitializationCommit>.Run(() =>
            {
                var intent = Take(RestoreIntent(record, budget), "CreateRecord");
                Need(verifiedSnapshot != null && verifiedSnapshot.Descriptor.Purpose == SavePurpose.PlayerSave &&
                    verifiedSnapshot.Business.PlayerId == record.PlayerId, "Initialization.Head", "InconsistentCreateIntent");
                var found = Take(CandidateApplicationProtocol.Lookup(verifiedSnapshot, intent, budget), "Initialization");
                Need(found.IsFound && found.Initializations.Count > 0 && found.Record.Generation == BigInteger.One &&
                    found.Record.CommitId == verifiedSnapshot.Header.CommitIndex[0].CommitId &&
                    ReferenceEquals(found.Record, verifiedSnapshot.Records[0]), "Initialization.OriginalCommit", "InconsistentCreateIntent");
                return new PlayerProfileInitializationCommit(record, found.Record, verifiedSnapshot.Descriptor);
            });
        }

        private static int Measure(PlayerProfileCreateRecord value, SaveCodecBudget budget, bool includeDigest)
        {
            var f = new BusinessFields(Stream.Null, false, budget) { StrictUnicode = true };
            Visit(f, value, includeDigest); return (int)f.Used;
        }
        private static byte[] Encode(PlayerProfileCreateRecord value, SaveCodecBudget budget, bool includeDigest)
        {
            var bytes = new byte[Measure(value, budget, includeDigest)];
            using (var stream = new MemoryStream(bytes, true)) Visit(new BusinessFields(stream, false, budget) { StrictUnicode = true }, value, includeDigest);
            return bytes;
        }
        private static PlayerProfileCreateRecord Visit(BusinessFields f, PlayerProfileCreateRecord value, bool includeDigest)
        {
            foreach (var b in new byte[] { 70, 77, 80, 82, 79, 70, 48 })
                Need(f.U(b, 1, "CreateRecord.Magic") == b, "CreateRecord.Magic", "UnsupportedCreateRecord");
            var format = f.U((ulong)(48 + (value?.RecordFormatVersion ?? 1)), 1, "CreateRecord.Magic") - 48;
            Need(format == 1 || format == 2, "CreateRecord.Magic", "UnsupportedCreateRecord");
            Need(f.U(format, 4, "CreateRecord.RecordFormatVersion") == format, "CreateRecord.RecordFormatVersion", "UnsupportedCreateRecord");
            var player = f.Text(value?.PlayerId, "CreateRecord.PlayerId"); var operation = f.Text(value?.OperationId, "CreateRecord.OperationId");
            var binding = ((PublishedRuleContext)f.PublishedContext(value?.ContentBinding, "CreateRecord.ContentBinding")).Binding;
            var id = f.Text(value?.NewProfileDefinitionId, "CreateRecord.NewProfileDefinitionId");
            var version = f.Integer(value?.NewProfileDefinitionVersion ?? 0, "CreateRecord.NewProfileDefinitionVersion", 1);
            var definition = Bytes(f, value?.FrozenNewProfileDefinitionBytes, "CreateRecord.Definition");
            Need(f.U(format + 1, 4, "CreateRecord.IntentFormatVersion") == format + 1, "CreateRecord.IntentFormatVersion", "UnsupportedSchema");
            var intent = Bytes(f, value?.CanonicalInitializeIntentBytes, "CreateRecord.Intent");
            var intentSha = f.Text(value?.IntentSha256, "CreateRecord.IntentSha256");
            var definitionSha = f.Text(value?.DefinitionSha256, "CreateRecord.DefinitionSha256");
            var count = f.U(0, 4, "CreateRecord.GeneratedMaterials.Count");
            SaveCodecFailure.Limit(count, (ulong)f.Budget.MaxCollectionEntries, "CreateRecord.GeneratedMaterials", "CollectionEntries");
            Need(count == 0, "CreateRecord.GeneratedMaterials", "InconsistentCreateIntent");
            var digest = includeDigest ? f.Text(value?.RecordSha256 ?? (f.Reading ? null : new string('0', 64)), "CreateRecord.RecordSha256") : null;
            return f.Reading ? new PlayerProfileCreateRecord(player, operation, binding, id, version, definition, intent, intentSha, definitionSha, digest, (int)format) : value;
        }
        private static byte[] Bytes(BusinessFields f, IReadOnlyList<byte> value, string path)
        {
            var count = f.U((ulong)(value?.Count ?? 0), 4, path + ".Length"); Need(count > 0, path, "MissingField");
            f.Space(count, path); var result = f.Reading ? new byte[(int)count] : null;
            for (var i = 0; i < (int)count; i++) { var b = (byte)f.U(f.Reading ? 0UL : value[i], 1, path); if (f.Reading) result[i] = b; }
            return result;
        }
        private static byte[] Copy(IReadOnlyList<byte> value)
        { var bytes = new byte[value.Count]; for (var i = 0; i < bytes.Length; i++) bytes[i] = value[i]; return bytes; }
        private static string Hash(IReadOnlyList<byte> value)
        { using (var sha = SHA256.Create()) return CandidateBattleReportFingerprint.Hex(sha.ComputeHash(value as byte[] ?? Copy(value))); }
    }
}
