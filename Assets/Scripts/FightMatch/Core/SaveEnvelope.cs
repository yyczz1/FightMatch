using System.Collections.Generic;
using System.Numerics;

namespace FightMatch.Core
{
    public enum SavePurpose : byte { CandidateValidation = 0, PlayerSave = 1 }
    public enum SaveBindingKind : byte { Content = 1, Definition = 2, CandidateContent = 3, CandidateDefinition = 4 }

    // These are technical values only. They neither resolve nor instantiate business definitions.
    public sealed class SaveBinding
    {
        public SaveBindingKind Kind { get; }
        public string PackageId { get; }
        public string DraftId { get; }
        public BigInteger? DraftRevision { get; }
        public string ContentFingerprint { get; }
        public string RuleVersion { get; }
        public string NumericContractVersion { get; }
        public string RandomContractVersion { get; }
        public IReadOnlyList<string> SourceNotes { get; }
        public string LevelId { get; }
        public string LevelVersion { get; }

        public SaveBinding(SaveBindingKind kind, string packageId, string draftId, BigInteger? draftRevision,
            string contentFingerprint, string ruleVersion, string numericContractVersion,
            string randomContractVersion, IReadOnlyList<string> sourceNotes, string levelId, string levelVersion)
        {
            Kind = kind; PackageId = packageId; DraftId = draftId; DraftRevision = draftRevision;
            ContentFingerprint = contentFingerprint; RuleVersion = ruleVersion;
            NumericContractVersion = numericContractVersion; RandomContractVersion = randomContractVersion;
            SourceNotes = sourceNotes; LevelId = levelId; LevelVersion = levelVersion;
        }
    }

    public sealed class SaveRequirements
    {
        public IReadOnlyList<SaveBinding> Bindings { get; }
        public IReadOnlyList<string> RuleVersions { get; }
        public IReadOnlyList<string> NumericContractVersions { get; }
        public IReadOnlyList<string> RandomContractVersions { get; }
        public IReadOnlyList<string> FeatureIds { get; }

        public SaveRequirements(IReadOnlyList<SaveBinding> bindings, IReadOnlyList<string> ruleVersions,
            IReadOnlyList<string> numericContractVersions, IReadOnlyList<string> randomContractVersions,
            IReadOnlyList<string> featureIds)
        {
            Bindings = bindings; RuleVersions = ruleVersions; NumericContractVersions = numericContractVersions;
            RandomContractVersions = randomContractVersions; FeatureIds = featureIds;
        }
    }

    public sealed class RequiredSliceContract
    {
        public string SliceId { get; }
        public string OwnerId { get; }
        public uint SchemaVersion { get; }
        public RequiredSliceContract(string sliceId, string ownerId, uint schemaVersion)
        { SliceId = sliceId; OwnerId = ownerId; SchemaVersion = schemaVersion; }
    }

    public sealed class SaveSliceDirectoryEntry
    {
        public RequiredSliceContract Contract { get; }
        public ulong BodyOffset { get; }
        public ulong Length { get; }
        public IReadOnlyList<byte> Sha256 { get; }
        public SaveRequirements Requirements { get; }
        internal SaveSliceDirectoryEntry(RequiredSliceContract contract, ulong offset, ulong length,
            IReadOnlyList<byte> sha256, SaveRequirements requirements)
        { Contract = contract; BodyOffset = offset; Length = length; Sha256 = sha256; Requirements = requirements; }
    }

    public sealed class SaveCommitIndexEntry
    {
        public BigInteger Generation { get; }
        public string CommitId { get; }
        public string ParentCommitId { get; }
        public ulong? SnapshotLength { get; }
        public IReadOnlyList<byte> SnapshotSha256 { get; }
        public IReadOnlyList<string> OperationIds { get; }
        public SaveCommitIndexEntry(BigInteger generation, string commitId, string parentCommitId,
            ulong? snapshotLength, IReadOnlyList<byte> snapshotSha256, IReadOnlyList<string> operationIds)
        {
            Generation = generation; CommitId = commitId; ParentCommitId = parentCommitId;
            SnapshotLength = snapshotLength; SnapshotSha256 = snapshotSha256; OperationIds = operationIds;
        }
    }

    public sealed class SnapshotDescriptor
    {
        public SavePurpose Purpose { get; }
        public string PlayerId { get; }
        public BigInteger SaveGeneration { get; }
        public string CommitId { get; }
        public string ParentCommitId { get; }
        public ulong TotalLength { get; }
        public IReadOnlyList<byte> Sha256 { get; }
        public SnapshotDescriptor(SavePurpose purpose, string playerId, BigInteger generation, string commitId,
            string parentCommitId, ulong totalLength, IReadOnlyList<byte> sha256)
        {
            Purpose = purpose; PlayerId = playerId; SaveGeneration = generation; CommitId = commitId;
            ParentCommitId = parentCommitId; TotalLength = totalLength; Sha256 = sha256;
        }
    }

    public sealed class SaveEnvelope
    {
        public SavePurpose Purpose { get; }
        public string PlayerId { get; }
        public BigInteger SaveGeneration { get; }
        public string CommitId { get; }
        public string ParentCommitId { get; }
        public IReadOnlyList<RequiredSliceContract> RequiredSliceContracts { get; }
        public IReadOnlyList<SaveSliceDirectoryEntry> SliceDirectory { get; }
        public IReadOnlyList<SaveCommitIndexEntry> CommitIndex { get; }
        public SaveRequirements RecoveryRequirements { get; }
        public IReadOnlyList<IReadOnlyList<byte>> SliceBytes { get; }
        public ulong BodyLength { get; }
        public IReadOnlyList<byte> BodySha256 { get; }
        internal byte[][] Bodies { get; }

        internal SaveEnvelope(SavePurpose purpose, string playerId, BigInteger generation, string commitId,
            string parent, RequiredSliceContract[] contracts, SaveSliceDirectoryEntry[] directory,
            SaveCommitIndexEntry[] index, SaveRequirements recovery, ulong bodyLength,
            byte[] bodySha256, byte[][] bodies)
        {
            Purpose = purpose; PlayerId = playerId; SaveGeneration = generation; CommitId = commitId;
            ParentCommitId = parent; RequiredSliceContracts = System.Array.AsReadOnly(contracts);
            SliceDirectory = System.Array.AsReadOnly(directory); CommitIndex = System.Array.AsReadOnly(index);
            RecoveryRequirements = recovery; BodyLength = bodyLength;
            BodySha256 = System.Array.AsReadOnly(bodySha256); Bodies = bodies;
            var views = new IReadOnlyList<byte>[bodies.Length];
            for (var i = 0; i < bodies.Length; i++) views[i] = System.Array.AsReadOnly(bodies[i]);
            SliceBytes = System.Array.AsReadOnly(views);
        }
    }

    public sealed class SaveRecoverySummary
    {
        public SnapshotDescriptor Descriptor { get; }
        public IReadOnlyList<RequiredSliceContract> RequiredSliceContracts { get; }
        public IReadOnlyList<SaveSliceDirectoryEntry> SliceDirectory { get; }
        public IReadOnlyList<SaveCommitIndexEntry> CommitIndex { get; }
        public SaveRequirements RecoveryRequirements { get; }
        internal SaveRecoverySummary(SnapshotDescriptor descriptor, SaveEnvelope metadata)
        {
            Descriptor = descriptor; RequiredSliceContracts = metadata.RequiredSliceContracts;
            SliceDirectory = metadata.SliceDirectory; CommitIndex = metadata.CommitIndex;
            RecoveryRequirements = metadata.RecoveryRequirements;
        }
    }
}
