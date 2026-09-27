using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Security.Cryptography;

namespace FightMatch.Core
{
    public static class SaveEnvelopeCodec
    {
        private const ulong HeaderLength = 60;
        private static readonly byte[] Magic = { 70, 77, 83, 65, 86, 69, 48, 49 };

        public static SaveCodecResult<SaveEnvelope> Prepare(SaveEnvelopeInput input, SaveCodecBudget budget)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            return SaveCodecResult<SaveEnvelope>.Run(() =>
            {
                CheckList(input.Slices, budget, "Slices");
                var directoryMinimum = (ulong)input.Slices.Count * 84;
                Total(directoryMinimum, 0, budget);
                var bodyLength = 0UL;
                for (var i = 0; i < input.Slices.Count; i++)
                {
                    var slice = input.Slices[i];
                    Need(slice != null && slice.Bytes != null, "MissingField", "Slices[" + i + "].Bytes");
                    bodyLength = Sum(bodyLength, (ulong)slice.Bytes.Length, "BodyLength");
                    Total(directoryMinimum, bodyLength, budget);
                }
                var directory = new SaveSliceDirectoryEntry[input.Slices.Count];
                var offset = 0UL;
                using (var bodyHash = SHA256.Create())
                {
                    for (var i = 0; i < directory.Length; i++)
                    {
                        var slice = input.Slices[i];
                        var length = (ulong)slice.Bytes.Length;
                        directory[i] = new SaveSliceDirectoryEntry(slice.Contract, offset, length,
                            Hash(slice.Bytes), slice.Requirements);
                        Feed(bodyHash, slice.Bytes, slice.Bytes.Length);
                        offset += length;
                    }
                    var digest = Finish(bodyHash);
                    using (var buffer = new MemoryStream())
                    {
                        var fields = new SaveFields(buffer, false, budget, bodyLength: bodyLength);
                        var frozen = Metadata(fields, input, directory, null, bodyLength, digest);
                        Total(fields.Used, bodyLength, budget);
                        var bodies = new byte[input.Slices.Count][];
                        for (var i = 0; i < bodies.Length; i++) bodies[i] = (byte[])input.Slices[i].Bytes.Clone();
                        return WithBodies(frozen, bodies);
                    }
                }
            });
        }

        public static SaveCodecResult<SnapshotDescriptor> Write(Stream destination, SaveEnvelope envelope, SaveCodecBudget budget)
        {
            if (destination == null) throw new ArgumentNullException(nameof(destination));
            if (envelope == null) throw new ArgumentNullException(nameof(envelope));
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            return SaveCodecResult<SnapshotDescriptor>.Run(() =>
            {
                Need(destination.CanWrite, "InvalidValue", "Destination.CanWrite");
                Total(0, envelope.BodyLength, budget);
                using (var metadata = new MemoryStream())
                using (var fullHash = SHA256.Create())
                {
                    var fields = new SaveFields(metadata, false, budget, bodyLength: envelope.BodyLength);
                    Metadata(fields, InputOf(envelope), envelope.SliceDirectory, envelope.RecoveryRequirements,
                        envelope.BodyLength, CopyDigest(envelope.BodySha256, "BodySha256"));
                    var total = Total(fields.Used, envelope.BodyLength, budget);
                    var header = Header(fields.Used, envelope.BodyLength, envelope.BodySha256);
                    WritePart(destination, fullHash, header, header.Length);
                    WritePart(destination, fullHash, metadata.GetBuffer(), (int)metadata.Length);
                    foreach (var bytes in envelope.Bodies) WritePart(destination, fullHash, bytes, bytes.Length);
                    return Descriptor(envelope, total, Finish(fullHash));
                }
            });
        }

        public static SaveCodecResult<SaveEnvelope> Read(Stream source, SnapshotDescriptor expected, SaveCodecBudget budget)
        {
            CheckRoots(source, expected, budget);
            return SaveCodecResult<SaveEnvelope>.Run(() => ReadCore(source, expected, budget, true).Envelope);
        }

        public static SaveCodecResult<SaveRecoverySummary> ReadRequirements(Stream source,
            SnapshotDescriptor expected, SaveCodecBudget budget)
        {
            CheckRoots(source, expected, budget);
            return SaveCodecResult<SaveRecoverySummary>.Run(() =>
            {
                var read = ReadCore(source, expected, budget, false);
                return new SaveRecoverySummary(read.Descriptor, read.Envelope);
            });
        }

        public static SaveCodecResult<SaveRecoverySummary> ReadUncommittedRequirements(Stream source, SaveCodecBudget budget)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            return SaveCodecResult<SaveRecoverySummary>.Run(() =>
            {
                var read = ReadCore(source, null, budget, false);
                return new SaveRecoverySummary(read.Descriptor, read.Envelope);
            });
        }

        private sealed class ReadValue
        {
            internal SaveEnvelope Envelope;
            internal SnapshotDescriptor Descriptor;
        }

        private static ReadValue ReadCore(Stream source, SnapshotDescriptor expected, SaveCodecBudget budget, bool retain)
        {
            Need(source.CanRead, "InvalidValue", "Source.CanRead");
            byte[] expectedHash = null;
            if (expected != null)
            {
                expectedHash = CopyDigest(expected.Sha256, "Expected.Sha256");
                Need(expected.Purpose == SavePurpose.CandidateValidation || expected.Purpose == SavePurpose.PlayerSave,
                    "InvalidValue", "Expected.Purpose");
                Need(expected.PlayerId != null && expected.CommitId != null,
                    "MissingField", "Expected.Identity");
                Need(expected.PlayerId.Length > 0 && expected.CommitId.Length > 0, "InvalidValue", "Expected.Identity");
                foreach (var value in new[] { expected.PlayerId, expected.CommitId, expected.ParentCommitId })
                    if (value != null) SaveCodecFailure.Limit((ulong)value.Length, (ulong)budget.MaxStringCodeUnits,
                        "Expected.Identity", "StringCodeUnits");
                budget.Math.CheckInteger(expected.SaveGeneration);
                Need(expected.SaveGeneration.Sign > 0, "InvalidValue", "Expected.SaveGeneration");
            }
            Total(0, 0, budget);
            using (var fullHash = SHA256.Create())
            using (var bodyHash = SHA256.Create())
            {
                var header = new byte[HeaderLength];
                ReadPart(source, header, 0, header.Length, "Header");
                Feed(fullHash, header, header.Length);
                for (var i = 0; i < Magic.Length; i++) Need(header[i] == Magic[i], "Malformed", "Header.Magic");
                Need(Number(header, 8, 4) == 1, "UnsupportedEnvelopeVersion", "Header.Version");
                var metadataLength = Number(header, 12, 8);
                var bodyLength = Number(header, 20, 8);
                var total = Total(metadataLength, bodyLength, budget);
                if (expected != null) Need(total == expected.TotalLength, "LengthMismatch", "Expected.TotalLength");
                SaveCodecFailure.Limit(metadataLength, int.MaxValue, "Metadata", "ArrayLength");
                var metadataBytes = new byte[(int)metadataLength];
                ReadPart(source, metadataBytes, 0, metadataBytes.Length, "Metadata");
                Feed(fullHash, metadataBytes, metadataBytes.Length);
                var bodyDigest = new byte[32];
                Array.Copy(header, 28, bodyDigest, 0, 32);
                SaveEnvelope envelope;
                using (var metadata = new MemoryStream(metadataBytes, false))
                {
                    var fields = new SaveFields(metadata, true, budget, metadataLength);
                    envelope = Metadata(fields, null, null, null, bodyLength, bodyDigest);
                    Need(fields.Used == metadataLength, "LengthMismatch", "Metadata");
                }
                if (expected != null) Need(envelope.Purpose == expected.Purpose && Same(envelope.PlayerId, expected.PlayerId) &&
                    envelope.SaveGeneration == expected.SaveGeneration && Same(envelope.CommitId, expected.CommitId) &&
                    Same(envelope.ParentCommitId, expected.ParentCommitId), "InvalidValue", "Expected.Identity");
                var bodies = retain ? new byte[envelope.SliceDirectory.Count][] : Array.Empty<byte[]>();
                var chunk = new byte[8192];
                for (var i = 0; i < envelope.SliceDirectory.Count; i++)
                {
                    var entry = envelope.SliceDirectory[i];
                    if (retain)
                    {
                        SaveCodecFailure.Limit(entry.Length, int.MaxValue, "SliceDirectory[" + i + "].Length", "ArrayLength");
                        bodies[i] = new byte[(int)entry.Length];
                    }
                    using (var sliceHash = SHA256.Create())
                    {
                        var consumed = 0UL;
                        while (consumed < entry.Length)
                        {
                            var count = (int)Math.Min((ulong)chunk.Length, entry.Length - consumed);
                            var bytes = retain ? bodies[i] : chunk;
                            var start = retain ? (int)consumed : 0;
                            ReadPart(source, bytes, start, count, "SliceBytes[" + i + "]");
                            fullHash.TransformBlock(bytes, start, count, bytes, start);
                            bodyHash.TransformBlock(bytes, start, count, bytes, start);
                            sliceHash.TransformBlock(bytes, start, count, bytes, start);
                            consumed += (ulong)count;
                        }
                        Need(BytesEqual(Finish(sliceHash), entry.Sha256), "DigestMismatch", "SliceDirectory[" + i + "].Sha256");
                    }
                }
                Need(BytesEqual(Finish(bodyHash), envelope.BodySha256), "DigestMismatch", "Header.BodySha256");
                Need(source.Read(chunk, 0, 1) == 0, "LengthMismatch", "Snapshot.TrailingBytes");
                var digest = Finish(fullHash);
                if (expected != null) Need(BytesEqual(digest, expectedHash), "DigestMismatch", "Expected.Sha256");
                return new ReadValue { Envelope = WithBodies(envelope, bodies), Descriptor = Descriptor(envelope, total, digest) };
            }
        }

        // One field walk defines the wire order for writing and bounded reading. It also freezes every list.
        private static SaveEnvelope Metadata(SaveFields f, SaveEnvelopeInput input,
            IReadOnlyList<SaveSliceDirectoryEntry> suppliedDirectory, SaveRequirements suppliedRecovery,
            ulong bodyLength, byte[] bodyDigest)
        {
            if (!f.Reading) Need(input.Purpose.HasValue && input.SaveGeneration.HasValue, "MissingField", "Identity");
            var purpose = (SavePurpose)f.Byte((byte)(input?.Purpose ?? 0), "Purpose");
            Need(purpose == SavePurpose.CandidateValidation || purpose == SavePurpose.PlayerSave, "InvalidValue", "Purpose");
            var player = f.Text(input?.PlayerId, "PlayerId");
            var generation = f.Integer(input?.SaveGeneration ?? 0, "SaveGeneration");
            Need(generation.Sign > 0, "InvalidValue", "SaveGeneration");
            var commit = f.Text(input?.CommitId, "CommitId");
            var parent = f.OptionalText(input?.ParentCommitId, "ParentCommitId");
            var contracts = f.List(input?.RequiredSliceContracts,
                (v, p) => Contract(f, v, p), "RequiredSliceContracts", 16);
            var directory = f.List(suppliedDirectory, (v, p) =>
            {
                var contract = Contract(f, v?.Contract, p + ".Contract");
                var offset = f.Unsigned(v?.BodyOffset ?? 0, 8, p + ".BodyOffset");
                var length = f.Unsigned(v?.Length ?? 0, 8, p + ".Length");
                var sha = f.Digest(v?.Sha256, p + ".Sha256");
                var requirements = Requirements(f, v?.Requirements, purpose, p + ".Requirements");
                return new SaveSliceDirectoryEntry(contract, offset, length, Array.AsReadOnly(sha), requirements);
            }, "SliceDirectory", 84);
            Need(contracts.Length == directory.Length, "DirectoryMismatch", "SliceDirectory.Count");
            var ids = new HashSet<string>(StringComparer.Ordinal);
            var end = 0UL;
            for (var i = 0; i < directory.Length; i++)
            {
                var a = contracts[i]; var b = directory[i]; var p = "SliceDirectory[" + i + "]";
                Need(ids.Add(a.SliceId) && Same(a.SliceId, b.Contract.SliceId) && Same(a.OwnerId, b.Contract.OwnerId) &&
                    a.SchemaVersion == b.Contract.SchemaVersion, "DirectoryMismatch", p + ".Contract");
                Need(b.BodyOffset == end && b.Length <= bodyLength - Math.Min(end, bodyLength), "DirectoryMismatch", p + ".BodyOffset");
                end = Sum(end, b.Length, p + ".Length");
            }
            Need(end == bodyLength, "DirectoryMismatch", "BodyLength");
            var index = f.List(input?.CommitIndex, (v, p) =>
            {
                if (!f.Reading) Need(v != null, "MissingField", p);
                var g = f.Integer(v?.Generation ?? 0, p + ".Generation");
                var id = f.Text(v?.CommitId, p + ".CommitId");
                var previous = f.OptionalText(v?.ParentCommitId, p + ".ParentCommitId");
                if (!f.Reading) Need(v.SnapshotLength.HasValue == (v.SnapshotSha256 != null), "IndexMismatch", p + ".Snapshot");
                var present = f.Present(v?.SnapshotLength != null, p + ".Snapshot");
                ulong? length = present ? f.Unsigned(v?.SnapshotLength ?? 0, 8, p + ".SnapshotLength") : (ulong?)null;
                var sha = present ? Array.AsReadOnly(f.Digest(v?.SnapshotSha256, p + ".SnapshotSha256")) : null;
                var operations = f.Strings(v?.OperationIds, p + ".OperationIds");
                return new SaveCommitIndexEntry(g, id, previous, length, sha, operations);
            }, "CommitIndex", 17);
            Need(index.Length > 0 && generation == index.Length, "IndexMismatch", "CommitIndex.Count");
            var commits = new HashSet<string>(StringComparer.Ordinal);
            var operationsSeen = new HashSet<string>(StringComparer.Ordinal);
            for (var i = 0; i < index.Length; i++)
            {
                var row = index[i]; var p = "CommitIndex[" + i + "]";
                Need(row.Generation == i + BigInteger.One && commits.Add(row.CommitId) &&
                    Same(row.ParentCommitId, i == 0 ? null : index[i - 1].CommitId), "IndexMismatch", p + ".Identity");
                Need(i == index.Length - 1 ? row.SnapshotLength == null : row.SnapshotLength > 0, "IndexMismatch", p + ".Snapshot");
                foreach (var id in row.OperationIds) Need(operationsSeen.Add(id), "IndexMismatch", p + ".OperationIds");
            }
            var last = index[index.Length - 1];
            Need(Same(last.CommitId, commit) && Same(last.ParentCommitId, parent), "IndexMismatch", "CommitIndex.Head");
            var union = Union(directory, f.Budget);
            var recovery = Requirements(f, suppliedRecovery ?? union, purpose, "RecoveryRequirements");
            Need(RequirementsEqual(union, recovery), "RequirementsMismatch", "RecoveryRequirements");
            return new SaveEnvelope(purpose, player, generation, commit, parent, contracts, directory, index,
                recovery, bodyLength, bodyDigest, Array.Empty<byte[]>());
        }

        private static RequiredSliceContract Contract(SaveFields f, RequiredSliceContract v, string p)
        {
            if (!f.Reading) Need(v != null, "MissingField", p);
            var id = f.Text(v?.SliceId, p + ".SliceId");
            var owner = f.Text(v?.OwnerId, p + ".OwnerId");
            var version = (uint)f.Unsigned(v?.SchemaVersion ?? 0, 4, p + ".SchemaVersion");
            Need(version > 0, "InvalidValue", p + ".SchemaVersion");
            return new RequiredSliceContract(id, owner, version);
        }

        private static SaveRequirements Requirements(SaveFields f, SaveRequirements v, SavePurpose purpose, string p)
        {
            if (!f.Reading) Need(v != null, "MissingField", p);
            var bindings = f.List(v?.Bindings, (b, path) => Binding(f, b, purpose, path), p + ".Bindings", 31);
            var seen = new HashSet<SaveBinding>(BindingComparer.Instance);
            foreach (var binding in bindings) Need(seen.Add(binding), "RequirementsMismatch", p + ".Bindings");
            var rules = f.Strings(v?.RuleVersions, p + ".RuleVersions");
            var numeric = f.Strings(v?.NumericContractVersions, p + ".NumericContractVersions");
            var random = f.Strings(v?.RandomContractVersions, p + ".RandomContractVersions");
            var features = f.Strings(v?.FeatureIds, p + ".FeatureIds");
            var ruleSet = new HashSet<string>(rules, StringComparer.Ordinal);
            var numericSet = new HashSet<string>(numeric, StringComparer.Ordinal);
            var randomSet = new HashSet<string>(random, StringComparer.Ordinal);
            foreach (var b in bindings)
                Need(ruleSet.Contains(b.RuleVersion) && numericSet.Contains(b.NumericContractVersion) &&
                    randomSet.Contains(b.RandomContractVersion), "RequirementsMismatch", p + ".Capabilities");
            return new SaveRequirements(Array.AsReadOnly(bindings), rules, numeric, random, features);
        }

        private static SaveBinding Binding(SaveFields f, SaveBinding b, SavePurpose purpose, string p)
        {
            if (!f.Reading) Need(b != null, "MissingField", p);
            var kind = (SaveBindingKind)f.Byte((byte)(b?.Kind ?? 0), p + ".Kind");
            Need((byte)kind >= 1 && (byte)kind <= 4, "InvalidValue", p + ".Kind");
            var candidate = kind == SaveBindingKind.CandidateContent || kind == SaveBindingKind.CandidateDefinition;
            var level = kind == SaveBindingKind.Definition || kind == SaveBindingKind.CandidateDefinition;
            Need(!candidate || purpose == SavePurpose.CandidateValidation, "InvalidValue", p + ".Kind");
            if (!f.Reading)
            {
                Need(candidate ? b.PackageId == null : b.DraftId == null && b.DraftRevision == null && b.SourceNotes == null,
                    "InvalidValue", p + ".Identity");
                Need(level || b.LevelId == null && b.LevelVersion == null, "InvalidValue", p + ".Level");
                Need(!candidate || b.DraftRevision.HasValue, "MissingField", p + ".DraftRevision");
            }
            var id = f.Text(candidate ? b?.DraftId : b?.PackageId, p + (candidate ? ".DraftId" : ".PackageId"));
            BigInteger? revision = candidate ? f.Integer(b?.DraftRevision ?? 0, p + ".DraftRevision") : (BigInteger?)null;
            Need(!candidate || revision > 0, "InvalidValue", p + ".DraftRevision");
            var fingerprint = f.Text(b?.ContentFingerprint, p + ".ContentFingerprint");
            var rule = f.Text(b?.RuleVersion, p + ".RuleVersion");
            var numeric = f.Text(b?.NumericContractVersion, p + ".NumericContractVersion");
            var random = f.Text(b?.RandomContractVersion, p + ".RandomContractVersion");
            // SourceNotes is an ordered provenance list, unlike the five requirement sets.
            var notes = candidate ? Array.AsReadOnly(f.List(b?.SourceNotes, (s, path) => f.Text(s, path), p + ".SourceNotes", 6)) : null;
            var levelId = level ? f.Text(b?.LevelId, p + ".LevelId") : null;
            var levelVersion = level ? f.Text(b?.LevelVersion, p + ".LevelVersion") : null;
            return new SaveBinding(kind, candidate ? null : id, candidate ? id : null, revision,
                fingerprint, rule, numeric, random, notes, levelId, levelVersion);
        }

        private static SaveRequirements Union(IReadOnlyList<SaveSliceDirectoryEntry> directory, SaveCodecBudget budget)
        {
            var bindings = new List<SaveBinding>(); var seen = new HashSet<SaveBinding>(BindingComparer.Instance);
            var strings = new[] { new List<string>(), new List<string>(), new List<string>(), new List<string>() };
            var sets = new[] { new HashSet<string>(StringComparer.Ordinal), new HashSet<string>(StringComparer.Ordinal),
                new HashSet<string>(StringComparer.Ordinal), new HashSet<string>(StringComparer.Ordinal) };
            foreach (var row in directory)
            {
                foreach (var b in row.Requirements.Bindings)
                    if (!seen.Contains(b)) { CheckUnionCount(bindings.Count, budget); seen.Add(b); bindings.Add(b); }
                var lists = new[] { row.Requirements.RuleVersions, row.Requirements.NumericContractVersions,
                    row.Requirements.RandomContractVersions, row.Requirements.FeatureIds };
                for (var i = 0; i < lists.Length; i++)
                    foreach (var value in lists[i])
                        if (!sets[i].Contains(value)) { CheckUnionCount(strings[i].Count, budget); sets[i].Add(value); strings[i].Add(value); }
            }
            return new SaveRequirements(bindings.AsReadOnly(), strings[0].AsReadOnly(), strings[1].AsReadOnly(),
                strings[2].AsReadOnly(), strings[3].AsReadOnly());
        }

        private static void CheckUnionCount(int count, SaveCodecBudget budget)
        { SaveCodecFailure.Limit((ulong)count + 1, (ulong)budget.MaxCollectionEntries, "RecoveryRequirements", "CollectionEntries"); }

        private static bool RequirementsEqual(SaveRequirements a, SaveRequirements b)
        {
            if (a.Bindings.Count != b.Bindings.Count) return false;
            for (var i = 0; i < a.Bindings.Count; i++) if (!BindingComparer.Instance.Equals(a.Bindings[i], b.Bindings[i])) return false;
            return StringsEqual(a.RuleVersions, b.RuleVersions) && StringsEqual(a.NumericContractVersions, b.NumericContractVersions) &&
                StringsEqual(a.RandomContractVersions, b.RandomContractVersions) && StringsEqual(a.FeatureIds, b.FeatureIds);
        }

        private sealed class BindingComparer : IEqualityComparer<SaveBinding>
        {
            internal static readonly BindingComparer Instance = new BindingComparer();
            public bool Equals(SaveBinding a, SaveBinding b)
            {
                return a.Kind == b.Kind && Same(a.PackageId, b.PackageId) && Same(a.DraftId, b.DraftId) &&
                    a.DraftRevision == b.DraftRevision && Same(a.ContentFingerprint, b.ContentFingerprint) &&
                    Same(a.RuleVersion, b.RuleVersion) && Same(a.NumericContractVersion, b.NumericContractVersion) &&
                    Same(a.RandomContractVersion, b.RandomContractVersion) && StringsEqual(a.SourceNotes, b.SourceNotes) &&
                    Same(a.LevelId, b.LevelId) && Same(a.LevelVersion, b.LevelVersion);
            }
            public int GetHashCode(SaveBinding b)
            {
                // Hashing only selects a bucket. Complete component equality always decides identity.
                unchecked
                {
                    var hash = (int)b.Kind;
                    foreach (var value in new[] { b.PackageId, b.DraftId, b.ContentFingerprint, b.RuleVersion,
                        b.NumericContractVersion, b.RandomContractVersion, b.LevelId, b.LevelVersion })
                        hash = hash * 31 + (value == null ? 0 : StringComparer.Ordinal.GetHashCode(value));
                    if (b.DraftRevision.HasValue) hash = hash * 31 + b.DraftRevision.Value.GetHashCode();
                    if (b.SourceNotes != null) foreach (var note in b.SourceNotes) hash = hash * 31 + StringComparer.Ordinal.GetHashCode(note);
                    return hash;
                }
            }
        }

        private static bool StringsEqual(IReadOnlyList<string> a, IReadOnlyList<string> b)
        {
            if (a == null || b == null) return a == b;
            if (a.Count != b.Count) return false;
            for (var i = 0; i < a.Count; i++) if (!Same(a[i], b[i])) return false;
            return true;
        }

        internal static void CheckList<T>(IReadOnlyList<T> values, SaveCodecBudget budget, string path)
        {
            Need(values != null, "MissingField", path);
            SaveCodecFailure.Limit((ulong)values.Count, (ulong)budget.MaxCollectionEntries, path, "CollectionEntries");
        }

        private static SaveEnvelopeInput InputOf(SaveEnvelope value)
        {
            return new SaveEnvelopeInput { Purpose = value.Purpose, PlayerId = value.PlayerId, SaveGeneration = value.SaveGeneration,
                CommitId = value.CommitId, ParentCommitId = value.ParentCommitId,
                RequiredSliceContracts = value.RequiredSliceContracts, CommitIndex = value.CommitIndex };
        }

        private static SaveEnvelope WithBodies(SaveEnvelope value, byte[][] bodies)
        {
            return new SaveEnvelope(value.Purpose, value.PlayerId, value.SaveGeneration, value.CommitId,
                value.ParentCommitId, Copy(value.RequiredSliceContracts), Copy(value.SliceDirectory), Copy(value.CommitIndex),
                value.RecoveryRequirements, value.BodyLength, Copy(value.BodySha256), bodies);
        }

        private static T[] Copy<T>(IReadOnlyList<T> values)
        { var result = new T[values.Count]; for (var i = 0; i < result.Length; i++) result[i] = values[i]; return result; }

        private static SnapshotDescriptor Descriptor(SaveEnvelope value, ulong total, byte[] digest)
        { return new SnapshotDescriptor(value.Purpose, value.PlayerId, value.SaveGeneration, value.CommitId, value.ParentCommitId, total, Array.AsReadOnly(digest)); }

        private static void CheckRoots(Stream source, SnapshotDescriptor expected, SaveCodecBudget budget)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (expected == null) throw new ArgumentNullException(nameof(expected));
            if (budget == null) throw new ArgumentNullException(nameof(budget));
        }

        private static ulong Total(ulong metadata, ulong body, SaveCodecBudget budget)
        {
            SaveCodecFailure.Limit(metadata, budget.MaxMetadataBytes, "Metadata", "MetadataBytes");
            var total = Sum(Sum(HeaderLength, metadata, "TotalLength"), body, "TotalLength");
            SaveCodecFailure.Limit(total, budget.MaxEnvelopeBytes, "TotalLength", "EnvelopeBytes");
            return total;
        }

        private static ulong Sum(ulong a, ulong b, string path)
        { Need(b <= ulong.MaxValue - a, "LengthMismatch", path); return a + b; }
        private static void Need(bool condition, string code, string path) { SaveCodecFailure.Require(condition, code, path); }
        private static bool Same(string a, string b) { return string.Equals(a, b, StringComparison.Ordinal); }
        private static byte[] CopyDigest(IReadOnlyList<byte> digest, string path)
        { Need(digest != null, "MissingField", path); Need(digest.Count == 32, "InvalidValue", path); return Copy(digest); }
        private static bool BytesEqual(IReadOnlyList<byte> a, IReadOnlyList<byte> b)
        {
            if (a.Count != b.Count) return false;
            for (var i = 0; i < a.Count; i++) if (a[i] != b[i]) return false;
            return true;
        }
        private static byte[] Hash(byte[] bytes) { using (var sha = SHA256.Create()) return sha.ComputeHash(bytes); }
        private static void Feed(HashAlgorithm hash, byte[] bytes, int count) { hash.TransformBlock(bytes, 0, count, bytes, 0); }
        private static byte[] Finish(HashAlgorithm hash) { hash.TransformFinalBlock(Array.Empty<byte>(), 0, 0); return hash.Hash; }
        private static void WritePart(Stream stream, HashAlgorithm hash, byte[] bytes, int count)
        { stream.Write(bytes, 0, count); Feed(hash, bytes, count); }
        private static void ReadPart(Stream source, byte[] bytes, int offset, int count, string path)
        {
            while (count > 0)
            {
                var read = source.Read(bytes, offset, count);
                Need(read > 0, "LengthMismatch", path);
                offset += read; count -= read;
            }
        }
        private static byte[] Header(ulong metadata, ulong body, IReadOnlyList<byte> digest)
        {
            var result = new byte[HeaderLength]; Array.Copy(Magic, result, Magic.Length);
            result[8] = 1;
            for (var i = 0; i < 8; i++) { result[12 + i] = (byte)(metadata >> (8 * i)); result[20 + i] = (byte)(body >> (8 * i)); }
            for (var i = 0; i < 32; i++) result[28 + i] = digest[i];
            return result;
        }
        private static ulong Number(byte[] bytes, int offset, int length)
        { var result = 0UL; for (var i = 0; i < length; i++) result |= (ulong)bytes[offset + i] << (8 * i); return result; }
    }

    internal sealed class SaveFields
    {
        private readonly Stream stream;
        private readonly ulong boundary;
        private readonly ulong envelopeSpace;
        private readonly byte[] scratch = new byte[8];
        internal bool Reading { get; }
        internal SaveCodecBudget Budget { get; }
        internal ulong Used { get; private set; }
        internal SaveFields(Stream stream, bool reading, SaveCodecBudget budget, ulong length = 0, ulong bodyLength = 0)
        {
            this.stream = stream; Reading = reading; Budget = budget; boundary = reading ? length : budget.MaxMetadataBytes;
            envelopeSpace = budget.MaxEnvelopeBytes >= 60 && bodyLength <= budget.MaxEnvelopeBytes - 60
                ? budget.MaxEnvelopeBytes - 60 - bodyLength : 0;
        }

        private void Reserve(ulong count, string path)
        {
            if (Reading) SaveCodecFailure.Require(count <= boundary - Used, "LengthMismatch", path);
            else
            {
                SaveCodecFailure.Limit(count, boundary - Used, path, "MetadataBytes");
                SaveCodecFailure.Limit(Used + count, int.MaxValue, path, "ArrayLength");
                SaveCodecFailure.Limit(Used + count, envelopeSpace, path, "EnvelopeBytes");
            }
        }
        internal ulong Unsigned(ulong value, int width, string path)
        {
            Reserve((ulong)width, path);
            if (Reading)
            {
                value = 0;
                for (var i = 0; i < width; i++)
                { var b = stream.ReadByte(); SaveCodecFailure.Require(b >= 0, "LengthMismatch", path); value |= (ulong)b << (i * 8); }
            }
            else
            {
                for (var i = 0; i < width; i++) scratch[i] = (byte)(value >> (i * 8));
                stream.Write(scratch, 0, width);
            }
            Used += (ulong)width;
            return value;
        }
        internal byte Byte(byte value, string path) { return (byte)Unsigned(value, 1, path); }
        internal bool Present(bool value, string path)
        {
            var flag = Byte(value ? (byte)1 : (byte)0, path);
            SaveCodecFailure.Require(flag <= 1, "Malformed", path);
            return flag == 1;
        }
        internal string OptionalText(string value, string path)
        { return Present(value != null, path) ? Text(value, path, false) : null; }
        internal string Text(string value, string path, bool required = true)
        {
            if (!Reading) SaveCodecFailure.Require(value != null, "MissingField", path);
            var length = Unsigned((ulong)(value?.Length ?? 0), 4, path + ".Length");
            SaveCodecFailure.Limit(length, (ulong)Budget.MaxStringCodeUnits, path, "StringCodeUnits");
            SaveCodecFailure.Require(!required || length > 0, "InvalidValue", path);
            Reserve(length * 2, path);
            if (Reading)
            {
                var chars = new char[(int)length];
                for (var i = 0; i < chars.Length; i++) chars[i] = (char)Unsigned(0, 2, path);
                return new string(chars);
            }
            for (var i = 0; i < value.Length; i++) Unsigned(value[i], 2, path);
            return value;
        }
        internal BigInteger Integer(BigInteger value, string path)
        {
            var token = Reading ? null : ExactSaveValueCodec.IntegerToken(value, Budget, count => Reserve((ulong)count + 4, path));
            var length = Unsigned((ulong)(token?.Length ?? 0), 4, path + ".Length");
            SaveCodecFailure.Limit(length, (ulong)Budget.MaxNumericTokenBytes, path, "NumericTokenBytes");
            Reserve(length, path);
            if (Reading)
            {
                var chars = new char[(int)length];
                for (var i = 0; i < chars.Length; i++) chars[i] = (char)Byte(0, path);
                return ExactSaveValueCodec.IntegerValue(new string(chars), Budget, path);
            }
            for (var i = 0; i < token.Length; i++) Byte((byte)token[i], path);
            return value;
        }
        internal byte[] Digest(IReadOnlyList<byte> digest, string path)
        {
            Reserve(32, path);
            if (!Reading) SaveCodecFailure.Require(digest != null && digest.Count == 32, "InvalidValue", path);
            var result = new byte[32];
            for (var i = 0; i < 32; i++) result[i] = Byte(Reading ? (byte)0 : digest[i], path);
            return result;
        }
        internal T[] List<T>(IReadOnlyList<T> values, Func<T, string, T> visit, string path, ulong minimumBytes)
        {
            if (!Reading) SaveEnvelopeCodec.CheckList(values, Budget, path);
            var count = Unsigned((ulong)(values?.Count ?? 0), 4, path + ".Count");
            SaveCodecFailure.Limit(count, (ulong)Budget.MaxCollectionEntries, path, "CollectionEntries");
            Reserve(count * minimumBytes, path);
            var result = new T[(int)count];
            for (var i = 0; i < result.Length; i++) result[i] = visit(Reading ? default(T) : values[i], path + "[" + i + "]");
            return result;
        }
        internal IReadOnlyList<string> Strings(IReadOnlyList<string> values, string path)
        {
            var result = List(values, (s, p) => Text(s, p), path, 6);
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var value in result)
                SaveCodecFailure.Require(seen.Add(value), path.Contains("OperationIds") ? "IndexMismatch" : "RequirementsMismatch", path);
            return Array.AsReadOnly(result);
        }
    }
}
