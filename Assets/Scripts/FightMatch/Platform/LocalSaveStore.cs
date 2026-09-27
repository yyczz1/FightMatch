using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;
using FightMatch.Core;

namespace FightMatch.Platform
{
    public sealed partial class LocalSaveStore : IDisposable
    {
        private readonly ILocalSaveStorage storage;
        private readonly SaveOpenMode mode;
        private IDisposable lease;
        private SaveCommitTicket pending;
        private int active;
        private volatile bool disposed;
        public LocalSaveResult<SaveHeadInspection> InitialInspection { get; private set; }
        public bool HasPendingTicket { get { return Volatile.Read(ref pending) != null; } }
        private LocalSaveStore(ILocalSaveStorage storage, SaveOpenMode mode, IDisposable lease)
        { this.storage = storage; this.mode = mode; this.lease = lease; }

        public static LocalSaveResult<LocalSaveStore> Open(ILocalSaveStorage storage, string playerId, SavePurpose purpose,
            SaveOpenMode mode, SaveFaultModel faultModel, SaveStoreBudget budget)
        {
            if (storage == null) throw new ArgumentNullException(nameof(storage));
            if (playerId == null) throw new ArgumentNullException(nameof(playerId));
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            try
            {
                LocalSaveFailure.Need(!string.IsNullOrEmpty(playerId), "InvalidValue", "PlayerId");
                LocalSaveFailure.Limit((ulong)playerId.Length, (ulong)budget.Codec.MaxStringCodeUnits, "PlayerId", "StringCodeUnits");
                LocalSaveFailure.Need(purpose == SavePurpose.CandidateValidation || purpose == SavePurpose.PlayerSave, "InvalidValue", "Purpose");
                LocalSaveFailure.Need(mode == SaveOpenMode.CreateNew || mode == SaveOpenMode.Existing, "InvalidValue", "OpenMode");
                LocalSaveFailure.Need(storage.Profile != null && storage.Profile.PlayerId == playerId && storage.Profile.Purpose == purpose,
                    "InconsistentBinding", "Storage.Profile");
                LocalSaveFailure.Need(faultModel == SaveFaultModel.EditorProcessCrash && storage.Profile.SupportedFaultModel == faultModel,
                    "StorageCapabilityUnavailable", "Storage.FaultModel");
                IDisposable acquired;
                try { acquired = storage.AcquireWriterLease(mode == SaveOpenMode.CreateNew); }
                catch (Exception error) when (LocalSaveFailure.Expected(error))
                {
                    var code = mode == SaveOpenMode.Existing && (error is FileNotFoundException || error is DirectoryNotFoundException) ? "NoSave" :
                        error is IOException && ((error.HResult & 65535) == 32 || (error.HResult & 65535) == 33) ? "Busy" : "StorageFailure";
                    return LocalSaveResult<LocalSaveStore>.Reject(new LocalSaveFailure(code, "Storage.Lease", cause: error));
                }
                LocalSaveFailure.Need(acquired != null, "StorageFailure", "Storage.Lease");
                var store = new LocalSaveStore(storage, mode, acquired);
                store.InitialInspection = store.Inspect(budget);
                return LocalSaveResult<LocalSaveStore>.Accept("Opened", store);
            }
            catch (Exception error) when (LocalSaveFailure.Expected(error))
            { return LocalSaveResult<LocalSaveStore>.Reject(LocalSaveFailure.From(error, "Open")); }
        }

        public LocalSaveResult<SaveCommitTicket> Prepare(SnapshotDescriptor expectedHead, IReadOnlyList<string> operationIds,
            Func<SaveCommitMetadata, SaveCodecResult<SaveEnvelope>> buildEnvelope, SaveStoreBudget budget)
        {
            if (operationIds == null) throw new ArgumentNullException(nameof(operationIds));
            if (buildEnvelope == null) throw new ArgumentNullException(nameof(buildEnvelope));
            return Execute(budget, "Prepare", () =>
            {
                LocalSaveFailure.Need(pending == null, "Busy", "PendingTicket");
                var observation = SaveHeadInspection.Read(storage, budget);
                LocalSaveFailure.Need(observation.Status != SaveHeadStatus.RecoveryBlocked, "RecoveryBlocked", "Head");
                LocalSaveFailure.Need(observation.Status != SaveHeadStatus.Pending, "Pending", "Head");
                var current = observation.Current;
                LocalSaveFailure.Need(current != null || mode == SaveOpenMode.CreateNew, "InitializationRequired", "OpenMode");
                LocalSaveFailure.Need(LocalSaveValues.Same(current?.Descriptor, expectedHead, budget), "StaleContext", "ExpectedHead");
                LocalSaveFailure.Limit((ulong)operationIds.Count, (ulong)budget.Codec.MaxCollectionEntries, "OperationIds", "CollectionEntries");
                var operations = new string[operationIds.Count]; var seen = new HashSet<string>(StringComparer.Ordinal);
                for (var i = 0; i < operations.Length; i++)
                {
                    var value = operationIds[i]; LocalSaveFailure.Need(!string.IsNullOrWhiteSpace(value), "InvalidValue", "OperationIds[" + i + "]");
                    LocalSaveFailure.Limit((ulong)value.Length, (ulong)budget.Codec.MaxStringCodeUnits, "OperationIds[" + i + "]", "StringCodeUnits");
                    LocalSaveFailure.Need(seen.Add(value), "InvalidValue", "OperationIds[" + i + "]"); operations[i] = value;
                }
                if (current != null)
                    foreach (var row in current.CommitIndex)
                        foreach (var operation in row.OperationIds)
                            LocalSaveFailure.Need(!seen.Contains(operation), "OperationAlreadyIndexed", "OperationIds");
                var indexCount = current == null ? 1UL : (ulong)current.CommitIndex.Count + 1;
                LocalSaveFailure.Limit(indexCount, (ulong)budget.Codec.MaxCollectionEntries, "CommitIndex", "CollectionEntries");
                var generation = current == null ? ExactRational.Create(1, 1, budget.Codec.Math).Numerator :
                    ExactRational.Create(current.Descriptor.SaveGeneration, 1, budget.Codec.Math).Add(ExactRational.Create(1, 1, budget.Codec.Math), budget.Codec.Math).Numerator;
                var commit = Guid.NewGuid().ToString("N"); var index = new List<SaveCommitIndexEntry>();
                if (current != null)
                    foreach (var row in current.CommitIndex)
                        index.Add(row.CommitId == current.Descriptor.CommitId ? new SaveCommitIndexEntry(row.Generation, row.CommitId, row.ParentCommitId,
                            current.Descriptor.TotalLength, current.Descriptor.Sha256, row.OperationIds) : row);
                index.Add(new SaveCommitIndexEntry(generation, commit, current?.Descriptor.CommitId, null, null, Array.AsReadOnly(operations)));
                var metadata = new SaveCommitMetadata(storage.Profile.PlayerId, storage.Profile.Purpose, generation, commit, current?.Descriptor.CommitId, index);
                var expected = LocalSaveValues.Freeze(expectedHead);
                SaveCodecResult<SaveEnvelope> encoded;
                try { encoded = buildEnvelope(metadata); }
                catch (Exception error) { throw new LocalSaveFailure("CallbackFailed", "BuildEnvelope", cause: error); }
                var envelope = LocalSaveFailure.Core(encoded);
                LocalSaveFailure.Need(envelope != null, "MissingField", "BuildEnvelope.Value");
                CheckMetadata(metadata, envelope, budget);
                var descriptor = LocalSaveFailure.Core(SaveEnvelopeCodec.Write(Stream.Null, envelope, budget.Codec));
                pending = new SaveCommitTicket(this, metadata, expected, operations, envelope, descriptor);
                return LocalSaveResult<SaveCommitTicket>.Accept("Prepared", pending);
            });
        }

        public LocalSaveResult<SaveHeadInspection> Inspect(SaveStoreBudget budget)
        { return Execute(budget, "Inspect", () => LocalSaveResult<SaveHeadInspection>.Accept("Inspected", SaveHeadInspection.Read(storage, budget))); }

        public LocalSaveResult<SaveCommittedReference> Lookup(string commitId, string operationId, SaveStoreBudget budget)
        {
            return Execute(budget, "Lookup", () =>
            {
                ValidateKeys(commitId, operationId, budget);
                SaveHeadInspection observation;
                try { observation = SaveHeadInspection.Read(storage, budget); }
                catch (Exception error) when (LocalSaveFailure.Expected(error))
                { return LocalSaveResult<SaveCommittedReference>.Reject(LocalSaveFailure.From(error, "Lookup.Inspect"), "RecoveryBlocked"); }
                return Find(observation, commitId, operationId);
            });
        }

        public LocalSaveResult<SaveCommittedReference> Write(SaveCommitTicket ticket, SaveStoreBudget budget)
        {
            if (ticket == null) throw new ArgumentNullException(nameof(ticket));
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            if (!ReferenceEquals(ticket.Owner, this)) return Reject<SaveCommittedReference>("TicketOwnerMismatch", "Ticket.Owner");
            return Execute(budget, "Write", () =>
            {
                LocalSaveFailure.Need(!ticket.Ended, "TicketEnded", "Ticket");
                SaveHeadInspection observation;
                try { observation = SaveHeadInspection.Read(storage, budget); }
                catch (Exception error) when (LocalSaveFailure.Expected(error))
                { return LocalSaveResult<SaveCommittedReference>.Reject(LocalSaveFailure.From(error, "Write.Inspect"), "RecoveryBlocked"); }
                var found = Find(observation, ticket.Metadata.CommitId, null);
                if (found.IsAccepted || ticket.Committed || found.Code != "ConfirmedNotCommitted") return found;
                LocalSaveFailure.Need(ReferenceEquals(pending, ticket), "TicketEnded", "Ticket");
                LocalSaveFailure.Need(LocalSaveValues.Same(observation.Current?.Descriptor, ticket.ExpectedHead, budget), "StaleContext", "ExpectedHead");
                LocalSaveFailure.Need(observation.Files.All(x => x.Disposition != SaveFileDisposition.Pending || x.CommitId == ticket.Metadata.CommitId),
                    "Pending", "OtherCandidate");
                var commit = ticket.Metadata.CommitId; var attemptedPublication = false; var stage = "Snapshot.Work";
                try
                {
                    foreach (var name in new[] { SaveFileNames.SnapshotWork(commit), SaveFileNames.MarkerWork(commit) })
                        if (observation.Files.Any(x => x.Name == name)) { stage = "Candidate.DeleteWork"; storage.DeleteUncommitted(name); }
                    var snapshot = SaveFileNames.Snapshot(commit);
                    if (observation.Files.Any(x => x.Name == snapshot))
                    {
                        stage = "Snapshot.Reuse";
                        SaveHeadInspection.ReadSnapshot(storage, snapshot, ticket.Descriptor, budget);
                    }
                    else
                    {
                        stage = "Snapshot.Create";
                        using (var stream = storage.CreateWork(SaveFileNames.SnapshotWork(commit)))
                        {
                            stage = "Snapshot.Write"; var written = LocalSaveFailure.Core(SaveEnvelopeCodec.Write(stream, ticket.Envelope, budget.Codec));
                            LocalSaveFailure.Need(LocalSaveValues.Same(written, ticket.Descriptor, budget), "InconsistentBinding", "Ticket.Descriptor");
                            stage = "Snapshot.Flush"; storage.FlushFile(stream); stage = "Snapshot.Close";
                        }
                        stage = "Snapshot.Verify"; SaveHeadInspection.ReadSnapshot(storage, SaveFileNames.SnapshotWork(commit), ticket.Descriptor, budget);
                        stage = "Snapshot.Promote"; storage.PromoteNoReplace(SaveFileNames.SnapshotWork(commit), snapshot);
                    }
                    stage = "Marker.Encode"; var marker = SaveCommitMarkerCodec.Encode(ticket.Descriptor, budget);
                    stage = "Marker.Create";
                    using (var stream = storage.CreateWork(SaveFileNames.MarkerWork(commit)))
                    {
                        stage = "Marker.Write"; stream.Write(marker, 0, marker.Length);
                        stage = "Marker.Flush"; storage.FlushFile(stream); stage = "Marker.Close";
                    }
                    stage = "Marker.Verify";
                    using (var stream = storage.OpenRead(SaveFileNames.MarkerWork(commit)))
                    {
                        var checkedMarker = SaveCommitMarkerCodec.Read(stream, SaveFileNames.MarkerWork(commit), budget);
                        LocalSaveFailure.Need(LocalSaveValues.Same(checkedMarker.Descriptor, ticket.Descriptor, budget), "InconsistentBinding", "Marker.Descriptor");
                    }
                    stage = "Marker.Promote"; attemptedPublication = true;
                    storage.PromoteNoReplace(SaveFileNames.MarkerWork(commit), SaveFileNames.Marker(commit));
                    stage = "Commit.Verify";
                    var result = Find(SaveHeadInspection.Read(storage, budget), commit, null);
                    if (!result.IsAccepted) return LocalSaveResult<SaveCommittedReference>.Reject(result.Failure, "CommitUnknown");
                    return result;
                }
                catch (Exception error) when (LocalSaveFailure.Expected(error))
                {
                    var original = LocalSaveFailure.From(error, stage);
                    var failure = new LocalSaveFailure(original.Code, original.Path, original.Reason, original.Required, original.Allowed, original.Cause, stage);
                    if (attemptedPublication) return LocalSaveResult<SaveCommittedReference>.Reject(failure, "CommitUnknown");
                    try
                    {
                        var checkedHead = SaveHeadInspection.Read(storage, budget);
                        if (checkedHead.Status != SaveHeadStatus.RecoveryBlocked &&
                            !checkedHead.Files.Any(x => x.Name == SaveFileNames.Marker(commit)) &&
                            LocalSaveValues.Same(checkedHead.Current?.Descriptor, ticket.ExpectedHead, budget))
                            return LocalSaveResult<SaveCommittedReference>.Reject(failure, stage == "Snapshot.Reuse" ? "RecoveryBlocked" : "SaveFailed");
                    }
                    catch (Exception checkError) when (LocalSaveFailure.Expected(checkError)) { }
                    return LocalSaveResult<SaveCommittedReference>.Reject(failure, "RecoveryBlocked");
                }
            });
        }

        public LocalSaveResult<bool> EndUncommitted(SaveCommitTicket ticket, SaveStoreBudget budget)
        {
            if (ticket == null) throw new ArgumentNullException(nameof(ticket));
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            if (!ReferenceEquals(ticket.Owner, this)) return Reject<bool>("TicketOwnerMismatch", "Ticket.Owner");
            return Execute(budget, "EndUncommitted", () =>
            {
                LocalSaveFailure.Need(!ticket.Ended, "TicketEnded", "Ticket");
                LocalSaveFailure.Need(!ticket.Committed && ReferenceEquals(pending, ticket), "AlreadyCommitted", "Ticket");
                var observation = SaveHeadInspection.Read(storage, budget); var lookup = Find(observation, ticket.Metadata.CommitId, null);
                LocalSaveFailure.Need(lookup.Code == "ConfirmedNotCommitted", lookup.IsAccepted ? "AlreadyCommitted" : lookup.Code, "Ticket.Commit");
                var commit = ticket.Metadata.CommitId;
                foreach (var name in new[] { SaveFileNames.SnapshotWork(commit), SaveFileNames.MarkerWork(commit), SaveFileNames.Snapshot(commit) })
                    if (observation.Files.Any(x => x.Name == name)) storage.DeleteUncommitted(name);
                ticket.Ended = true; pending = null; return LocalSaveResult<bool>.Accept("Ended", true);
            });
        }

        public void Dispose()
        {
            if (disposed) return;
            if (Interlocked.CompareExchange(ref active, 1, 0) != 0) throw new InvalidOperationException("Busy");
            try { if (!disposed) { disposed = true; lease.Dispose(); lease = null; } }
            finally { Volatile.Write(ref active, 0); }
        }

        private LocalSaveResult<T> Execute<T>(SaveStoreBudget budget, string stage, Func<LocalSaveResult<T>> action)
        {
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            if (disposed) return Reject<T>("Disposed", "Store");
            if (Interlocked.CompareExchange(ref active, 1, 0) != 0) return Reject<T>("Busy", "Store");
            try
            {
                if (disposed) return Reject<T>("Disposed", "Store");
                return action();
            }
            catch (Exception error) when (LocalSaveFailure.Expected(error)) { return LocalSaveResult<T>.Reject(LocalSaveFailure.From(error, stage)); }
            finally { Volatile.Write(ref active, 0); }
        }
        private LocalSaveResult<SaveCommittedReference> Find(SaveHeadInspection observation, string commit, string operation)
        {
            if (observation.Status == SaveHeadStatus.RecoveryBlocked)
                return LocalSaveResult<SaveCommittedReference>.Reject(observation.Diagnostics.FirstOrDefault()?.Failure ??
                    new LocalSaveFailure("RecoveryBlocked", "Head"), "RecoveryBlocked");
            var head = observation.Current;
            var byCommit = head?.CommitIndex.FirstOrDefault(x => x.CommitId == commit);
            var byOperation = operation == null ? null : head?.CommitIndex.FirstOrDefault(x => x.OperationIds.Contains(operation, StringComparer.Ordinal));
            if (commit != null && operation != null && (byCommit != null || byOperation != null) && !ReferenceEquals(byCommit, byOperation))
                return Reject<SaveCommittedReference>("KeyMismatch", "Lookup.Keys");
            var row = byCommit ?? byOperation;
            if (row == null) return Reject<SaveCommittedReference>("ConfirmedNotCommitted", "Lookup.Keys");
            if (pending != null && pending.Metadata.CommitId == row.CommitId) { pending.Committed = true; pending = null; }
            return LocalSaveResult<SaveCommittedReference>.Accept("Committed", new SaveCommittedReference(row, LocalSaveValues.Descriptor(head, row), head.Descriptor));
        }
        private static void ValidateKeys(string commit, string operation, SaveStoreBudget budget)
        {
            LocalSaveFailure.Need(commit != null || operation != null, "MissingField", "Lookup.Keys");
            LocalSaveFailure.Need(commit == null || SaveFileNames.Commit(commit), "InvalidValue", "Lookup.CommitId");
            LocalSaveFailure.Need(operation == null || !string.IsNullOrWhiteSpace(operation), "InvalidValue", "Lookup.OperationId");
            if (operation != null) LocalSaveFailure.Limit((ulong)operation.Length, (ulong)budget.Codec.MaxStringCodeUnits, "Lookup.OperationId", "StringCodeUnits");
        }
        private static void CheckMetadata(SaveCommitMetadata expected, SaveEnvelope actual, SaveStoreBudget budget)
        {
            LocalSaveFailure.Need(expected.PlayerId == actual.PlayerId, "InconsistentBinding", "Envelope.PlayerId");
            LocalSaveFailure.Need(expected.Purpose == actual.Purpose, "InconsistentBinding", "Envelope.Purpose");
            LocalSaveFailure.Need(LocalSaveValues.Compare(expected.SaveGeneration, actual.SaveGeneration, budget) == 0, "InconsistentBinding", "Envelope.SaveGeneration");
            LocalSaveFailure.Need(expected.CommitId == actual.CommitId, "InconsistentBinding", "Envelope.CommitId");
            LocalSaveFailure.Need(expected.ParentCommitId == actual.ParentCommitId, "InconsistentBinding", "Envelope.ParentCommitId");
            LocalSaveFailure.Need(actual.CommitIndex != null && expected.CommitIndex.Count == actual.CommitIndex.Count, "InconsistentBinding", "Envelope.CommitIndex");
            for (var i = 0; i < expected.CommitIndex.Count; i++)
                LocalSaveFailure.Need(actual.CommitIndex[i] != null && LocalSaveValues.Same(expected.CommitIndex[i], actual.CommitIndex[i], budget),
                    "InconsistentBinding", "Envelope.CommitIndex[" + i + "]");
        }
        private static LocalSaveResult<T> Reject<T>(string code, string path)
        { return LocalSaveResult<T>.Reject(new LocalSaveFailure(code, path)); }
    }
}
