using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using FightMatch.Core;

namespace FightMatch.Platform
{
    public enum SaveOpenMode { CreateNew, Existing }
    public enum SaveFaultModel : byte { EditorProcessCrash = 1, PowerLossDurable = 2 }
    public enum SaveHeadStatus { NoSave, Ready, Pending, RecoveryBlocked }
    public enum SaveFileKind { WriterLock, Snapshot, Marker, SnapshotWork, MarkerWork, Unknown }
    public enum SaveFileDisposition { Current, IndexedOldCopy, Pending, Unknown }

    public sealed class SaveStoreBudget
    {
        public SaveCodecBudget Codec { get; }
        public ulong MaxMarkerBytes { get; }
        public int MaxDirectoryEntries { get; }
        public SaveStoreBudget(SaveCodecBudget codec, ulong maxMarkerBytes = 4UL * 1024 * 1024, int maxDirectoryEntries = 65536)
        {
            Codec = codec ?? throw new ArgumentNullException(nameof(codec));
            if (maxMarkerBytes == 0 || maxDirectoryEntries <= 0) throw new ArgumentOutOfRangeException(nameof(maxMarkerBytes));
            MaxMarkerBytes = maxMarkerBytes; MaxDirectoryEntries = maxDirectoryEntries;
        }
    }

    public sealed class SaveStorageProfile
    {
        public string PlayerId { get; }
        public SavePurpose Purpose { get; }
        public string DirectoryPath { get; }
        public SaveFaultModel SupportedFaultModel { get; }
        public SaveStorageProfile(string playerId, SavePurpose purpose, string directoryPath, SaveFaultModel supportedFaultModel)
        {
            PlayerId = playerId ?? throw new ArgumentNullException(nameof(playerId));
            DirectoryPath = directoryPath ?? throw new ArgumentNullException(nameof(directoryPath));
            Purpose = purpose; SupportedFaultModel = supportedFaultModel;
        }
    }

    public sealed class LocalSaveDiagnostic
    {
        internal LocalSaveFailure Failure { get; }
        public string Code { get; }
        public string FieldPath { get; }
        public string Stage { get; }
        public string ExceptionType { get; }
        public string ExceptionMessage { get; }
        internal LocalSaveDiagnostic(LocalSaveFailure failure)
        {
            Failure = failure;
            Code = failure.Code; FieldPath = failure.Path; Stage = failure.Stage;
            ExceptionType = failure.Cause?.GetType().FullName; ExceptionMessage = failure.Cause?.Message;
        }
    }

    public sealed class LocalSaveResult<T>
    {
        internal LocalSaveFailure Failure { get; }
        public bool IsAccepted { get; }
        public T Value { get; }
        public string Code { get; }
        public string FieldPath { get; }
        public string LimitReason { get; }
        public ulong? RequiredAtLeast { get; }
        public ulong? Allowed { get; }
        public LocalSaveDiagnostic Diagnostic { get; }
        private LocalSaveResult(bool accepted, T value, string code, LocalSaveFailure failure)
        {
            Failure = failure;
            IsAccepted = accepted; Value = value; Code = code; FieldPath = failure?.Path;
            LimitReason = failure?.Reason; RequiredAtLeast = failure?.Required; Allowed = failure?.Allowed;
            Diagnostic = failure == null ? null : new LocalSaveDiagnostic(failure);
        }
        internal static LocalSaveResult<T> Accept(string code, T value) { return new LocalSaveResult<T>(true, value, code, null); }
        internal static LocalSaveResult<T> Reject(LocalSaveFailure failure, string code = null)
        { return new LocalSaveResult<T>(false, default(T), code ?? failure.Code, failure); }
    }

    public sealed class SaveCommitMetadata
    {
        public string PlayerId { get; }
        public SavePurpose Purpose { get; }
        public BigInteger SaveGeneration { get; }
        public string CommitId { get; }
        public string ParentCommitId { get; }
        public IReadOnlyList<SaveCommitIndexEntry> CommitIndex { get; }
        internal SaveCommitMetadata(string playerId, SavePurpose purpose, BigInteger generation, string commitId,
            string parentCommitId, IEnumerable<SaveCommitIndexEntry> index)
        {
            PlayerId = playerId; Purpose = purpose; SaveGeneration = generation; CommitId = commitId; ParentCommitId = parentCommitId;
            CommitIndex = Array.AsReadOnly(index.Select(LocalSaveValues.Freeze).ToArray());
        }
    }

    public sealed class SaveCommitTicket
    {
        public SaveCommitMetadata Metadata { get; }
        public SnapshotDescriptor ExpectedHead { get; }
        public IReadOnlyList<string> OperationIds { get; }
        public SaveEnvelope Envelope { get; }
        internal LocalSaveStore Owner { get; }
        internal SnapshotDescriptor Descriptor { get; }
        internal bool Ended;
        internal bool Committed;
        internal SaveCommitTicket(LocalSaveStore owner, SaveCommitMetadata metadata, SnapshotDescriptor expected,
            IReadOnlyList<string> operations, SaveEnvelope envelope, SnapshotDescriptor descriptor)
        {
            Owner = owner; Metadata = metadata; ExpectedHead = LocalSaveValues.Freeze(expected);
            OperationIds = Array.AsReadOnly(operations.ToArray()); Envelope = envelope;
            Descriptor = LocalSaveValues.Freeze(descriptor);
        }
    }

    public sealed class SaveCommittedReference
    {
        public SaveCommitIndexEntry IndexEntry { get; }
        public SnapshotDescriptor Descriptor { get; }
        public SnapshotDescriptor CurrentHead { get; }
        public SaveFaultModel FaultModel { get; }
        internal SaveCommittedReference(SaveCommitIndexEntry row, SnapshotDescriptor descriptor, SnapshotDescriptor current)
        {
            IndexEntry = LocalSaveValues.Freeze(row); Descriptor = LocalSaveValues.Freeze(descriptor);
            CurrentHead = LocalSaveValues.Freeze(current); FaultModel = SaveFaultModel.EditorProcessCrash;
        }
    }

    internal sealed class LocalSaveFailure : Exception
    {
        internal readonly string Code, Path, Reason, Stage;
        internal readonly ulong? Required, Allowed;
        internal readonly Exception Cause;
        internal LocalSaveFailure(string code, string path, string reason = null, ulong? required = null,
            ulong? allowed = null, Exception cause = null, string stage = null)
        { Code = code; Path = path; Reason = reason; Required = required; Allowed = allowed; Cause = cause; Stage = stage ?? path; }
        internal static void Need(bool condition, string code, string path)
        { if (!condition) throw new LocalSaveFailure(code, path); }
        internal static void Limit(ulong required, ulong allowed, string path, string reason)
        { if (required > allowed) throw new LocalSaveFailure("Limit", path, reason, required, allowed); }
        internal static T Core<T>(SaveCodecResult<T> result)
        {
            if (result == null) throw new LocalSaveFailure("MissingField", "BuildEnvelope.Result");
            if (!result.IsAccepted) throw new LocalSaveFailure(result.RejectionCode, result.FieldPath,
                result.LimitReason, result.RequiredAtLeast, result.Allowed);
            return result.Value;
        }
        internal static LocalSaveFailure From(Exception error, string stage)
        {
            if (error is LocalSaveFailure local) return local;
            if (error is ExactMathLimitException math)
                return new LocalSaveFailure("Limit", "Budget.Math", math.ReasonCode, (ulong)math.RequiredAtLeast, (ulong)math.Allowed, error, stage);
            return new LocalSaveFailure("StorageFailure", stage, cause: error, stage: stage);
        }
        internal static bool Expected(Exception error)
        {
            return error is LocalSaveFailure || error is ExactMathLimitException || error is IOException ||
                error is UnauthorizedAccessException || error is System.Security.SecurityException ||
                error is NotSupportedException || error is ObjectDisposedException;
        }
    }

    internal static class LocalSaveValues
    {
        internal static IReadOnlyList<byte> Digest(IReadOnlyList<byte> value)
        { return value == null ? null : Array.AsReadOnly(value.ToArray()); }
        internal static SnapshotDescriptor Freeze(SnapshotDescriptor value)
        {
            return value == null ? null : new SnapshotDescriptor(value.Purpose, value.PlayerId, value.SaveGeneration,
                value.CommitId, value.ParentCommitId, value.TotalLength, Digest(value.Sha256));
        }
        internal static SaveCommitIndexEntry Freeze(SaveCommitIndexEntry value)
        {
            return new SaveCommitIndexEntry(value.Generation, value.CommitId, value.ParentCommitId, value.SnapshotLength,
                Digest(value.SnapshotSha256), Array.AsReadOnly(value.OperationIds.ToArray()));
        }
        internal static bool Bytes(IReadOnlyList<byte> a, IReadOnlyList<byte> b)
        { return a == null || b == null ? a == b : a.SequenceEqual(b); }
        internal static int Compare(BigInteger a, BigInteger b, SaveStoreBudget budget)
        { return ExactRational.Create(a, 1, budget.Codec.Math).Compare(ExactRational.Create(b, 1, budget.Codec.Math), budget.Codec.Math); }
        internal static bool Same(SnapshotDescriptor a, SnapshotDescriptor b, SaveStoreBudget budget)
        {
            return a == null || b == null ? a == b : a.Purpose == b.Purpose && a.PlayerId == b.PlayerId &&
                Compare(a.SaveGeneration, b.SaveGeneration, budget) == 0 && a.CommitId == b.CommitId &&
                a.ParentCommitId == b.ParentCommitId && a.TotalLength == b.TotalLength && Bytes(a.Sha256, b.Sha256);
        }
        internal static bool Same(SaveCommitIndexEntry a, SaveCommitIndexEntry b, SaveStoreBudget budget)
        {
            return Compare(a.Generation, b.Generation, budget) == 0 && a.CommitId == b.CommitId && a.ParentCommitId == b.ParentCommitId &&
                a.SnapshotLength == b.SnapshotLength && Bytes(a.SnapshotSha256, b.SnapshotSha256) && a.OperationIds.SequenceEqual(b.OperationIds, StringComparer.Ordinal);
        }
        internal static SnapshotDescriptor Descriptor(SaveRecoverySummary head, SaveCommitIndexEntry row)
        {
            return row.CommitId == head.Descriptor.CommitId ? head.Descriptor : new SnapshotDescriptor(head.Descriptor.Purpose,
                head.Descriptor.PlayerId, row.Generation, row.CommitId, row.ParentCommitId, row.SnapshotLength.Value, row.SnapshotSha256);
        }
    }
}
