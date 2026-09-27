using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FightMatch.Core;

namespace FightMatch.Platform
{
    public sealed class SaveFileObservation
    {
        public string Name { get; }
        public SaveFileKind Kind { get; }
        public string CommitId { get; }
        public SaveFileDisposition Disposition { get; }
        public ulong? Length { get; }
        public IReadOnlyList<byte> Sha256 { get; }
        public string Detail { get; }
        public LocalSaveDiagnostic Diagnostic { get; }
        internal SaveFileObservation(string name, SaveFileKind kind, string commit, SaveFileDisposition disposition,
            ulong? length, IReadOnlyList<byte> sha, string detail, LocalSaveFailure failure)
        {
            Name = name; Kind = kind; CommitId = commit; Disposition = disposition; Length = length;
            Sha256 = LocalSaveValues.Digest(sha); Detail = detail; Diagnostic = failure == null ? null : new LocalSaveDiagnostic(failure);
        }
    }

    public sealed class SaveHeadInspection
    {
        public SaveHeadStatus Status { get; }
        public SaveRecoverySummary Current { get; }
        public IReadOnlyList<SaveFileObservation> Files { get; }
        public IReadOnlyList<LocalSaveDiagnostic> Diagnostics { get; }
        internal SaveHeadInspection(SaveHeadStatus status, SaveRecoverySummary current, IEnumerable<SaveFileObservation> files)
        {
            Status = status; Current = current; Files = Array.AsReadOnly(files.ToArray());
            Diagnostics = Array.AsReadOnly(Files.Where(x => x.Diagnostic != null).Select(x => x.Diagnostic).ToArray());
        }

        private sealed class Observed
        {
            internal string Name, Commit, Detail;
            internal SaveFileKind Kind;
            internal SaveFileDisposition Disposition = SaveFileDisposition.Unknown;
            internal SaveCommitMarker Marker;
            internal LocalSaveFailure Failure;
            internal ulong? Length;
            internal IReadOnlyList<byte> Sha;
            internal SaveFileObservation Freeze()
            { return new SaveFileObservation(Name, Kind, Commit, Disposition, Length, Sha, Detail, Failure); }
        }

        internal static SaveHeadInspection Read(ILocalSaveStorage storage, SaveStoreBudget budget)
        {
            var files = new List<Observed>(); var names = new HashSet<string>(StringComparer.Ordinal);
            try
            {
                foreach (var name in storage.EnumerateNames())
                {
                    LocalSaveFailure.Limit((ulong)files.Count + 1, (ulong)budget.MaxDirectoryEntries, "Directory", "DirectoryEntries");
                    LocalSaveFailure.Need(name != null && names.Add(name), "RecoveryBlocked", "Directory.Names");
                    string commit; var kind = SaveFileNames.Kind(name, out commit);
                    files.Add(new Observed { Name = name, Kind = kind, Commit = commit, Detail = "NotRead" });
                }
            }
            catch (Exception error) when (LocalSaveFailure.Expected(error)) { throw LocalSaveFailure.From(error, "Inspect.Enumerate"); }

            foreach (var file in files.Where(x => x.Kind == SaveFileKind.Marker))
            {
                try
                {
                    file.Marker = ReadFile(storage, file.Name, stream => SaveCommitMarkerCodec.Read(stream, file.Name, budget));
                    var descriptor = file.Marker.Descriptor;
                    LocalSaveFailure.Need(descriptor.PlayerId == storage.Profile.PlayerId && descriptor.Purpose == storage.Profile.Purpose,
                        "InconsistentBinding", "Marker.Profile");
                    file.Length = file.Marker.Length; file.Sha = file.Marker.Sha256; file.Detail = "MarkerVerified";
                }
                catch (Exception error) when (LocalSaveFailure.Expected(error)) { RecordFailure(file, error); file.Marker = null; }
            }

            // A lower complete candidate is usable only if its self-contained index explains EVERY other final marker as an old copy.
            // Thus a missing/bad latest commit never silently falls back, while a damaged indexed ancestor need not be readable.
            var candidates = files.Where(x => x.Marker != null).ToList(); SaveRecoverySummary current = null; Observed selected = null;
            while (candidates.Count > 0)
            {
                var best = 0;
                for (var i = 1; i < candidates.Count; i++)
                    if (LocalSaveValues.Compare(candidates[i].Marker.Descriptor.SaveGeneration, candidates[best].Marker.Descriptor.SaveGeneration, budget) > 0) best = i;
                var candidate = candidates[best]; candidates.RemoveAt(best);
                try
                {
                    current = ReadSnapshot(storage, SaveFileNames.Snapshot(candidate.Commit), candidate.Marker.Descriptor, budget);
                    selected = candidate; break;
                }
                catch (Exception error) when (LocalSaveFailure.Expected(error)) { RecordFailure(candidate, error); }
            }

            var index = current == null ? new Dictionary<string, SaveCommitIndexEntry>(StringComparer.Ordinal) :
                current.CommitIndex.ToDictionary(x => x.CommitId, StringComparer.Ordinal);
            var blocked = false; var pending = false;
            foreach (var file in files)
            {
                if (file.Kind == SaveFileKind.WriterLock) { file.Detail = "LeaseFile"; continue; }
                if (file.Kind == SaveFileKind.Unknown) { blocked = true; file.Detail = "UnrecognizedDirectoryEntry"; continue; }
                SaveCommitIndexEntry row; var indexed = index.TryGetValue(file.Commit, out row);
                if (file.Kind == SaveFileKind.Marker)
                {
                    if (file == selected) { file.Disposition = SaveFileDisposition.Current; continue; }
                    if (!indexed || row.CommitId == current.Descriptor.CommitId) { blocked = true; file.Detail = "UnattributedFinalMarker"; continue; }
                    file.Disposition = SaveFileDisposition.IndexedOldCopy;
                    var expected = LocalSaveValues.Descriptor(current, row);
                    if (file.Marker == null || !LocalSaveValues.Same(file.Marker.Descriptor, expected, budget))
                        file.Detail = "IndexedOldCopyDamaged";
                    else file.Detail = "IndexedOldMarker";
                    continue;
                }
                if (file.Kind == SaveFileKind.Snapshot && indexed)
                {
                    file.Disposition = row.CommitId == current.Descriptor.CommitId ? SaveFileDisposition.Current : SaveFileDisposition.IndexedOldCopy;
                    if (file.Disposition == SaveFileDisposition.Current)
                    { file.Length = current.Descriptor.TotalLength; file.Sha = current.Descriptor.Sha256; file.Detail = "SnapshotVerified"; }
                    else file.Detail = "IndexedOldSnapshotNotRead";
                    continue;
                }
                if (indexed && (file.Kind == SaveFileKind.SnapshotWork || file.Kind == SaveFileKind.MarkerWork))
                {
                    try
                    {
                        var expected = LocalSaveValues.Descriptor(current, row);
                        if (file.Kind == SaveFileKind.SnapshotWork)
                        {
                            var summary = ReadSnapshot(storage, file.Name, expected, budget);
                            file.Length = summary.Descriptor.TotalLength; file.Sha = summary.Descriptor.Sha256;
                        }
                        else
                        {
                            var marker = ReadFile(storage, file.Name, stream => SaveCommitMarkerCodec.Read(stream, file.Name, budget));
                            LocalSaveFailure.Need(LocalSaveValues.Same(marker.Descriptor, expected, budget), "InconsistentBinding", "WorkMarker.Descriptor");
                            file.Length = marker.Length; file.Sha = marker.Sha256;
                        }
                        file.Disposition = SaveFileDisposition.IndexedOldCopy; file.Detail = "IndexedCompleteWorkCopy"; continue;
                    }
                    catch (Exception error) when (LocalSaveFailure.Expected(error)) { RecordFailure(file, error); }
                }
                pending = true; file.Disposition = SaveFileDisposition.Pending; file.Detail = "UncommittedCandidateNotPromoted";
            }
            // Defer I/O rejection until the complete current index can attribute an unreadable old copy.
            var incomplete = files.FirstOrDefault(x => x.Kind == SaveFileKind.Marker && x.Disposition != SaveFileDisposition.IndexedOldCopy &&
                x.Failure?.Code == "StorageFailure" && !(x.Failure.Cause is FileNotFoundException) && !(x.Failure.Cause is DirectoryNotFoundException));
            if (incomplete != null) throw incomplete.Failure;
            if (blocked) return new SaveHeadInspection(SaveHeadStatus.RecoveryBlocked, null, files.Select(x => x.Freeze()));
            return new SaveHeadInspection(pending ? SaveHeadStatus.Pending : current == null ? SaveHeadStatus.NoSave : SaveHeadStatus.Ready,
                current, files.Select(x => x.Freeze()));
        }
        private static T ReadFile<T>(ILocalSaveStorage storage, string name, Func<Stream, T> read)
        {
            Stream stream;
            try { stream = storage.OpenRead(name); }
            catch (Exception error) when (LocalSaveFailure.Expected(error)) { throw LocalSaveFailure.From(error, "Inspect." + name + ".Open"); }
            try
            {
                try { return read(stream); }
                catch (Exception error) when (LocalSaveFailure.Expected(error)) { throw LocalSaveFailure.From(error, "Inspect." + name + ".Read"); }
            }
            finally
            {
                try { stream.Dispose(); }
                catch (Exception error) when (LocalSaveFailure.Expected(error)) { throw LocalSaveFailure.From(error, "Inspect." + name + ".Close"); }
            }
        }
        private static void RecordFailure(Observed file, Exception error)
        {
            var failure = LocalSaveFailure.From(error, "Inspect." + file.Name);
            if (failure.Code == "Limit") throw failure;
            file.Failure = failure; file.Detail = "UnreadableOrInvalid";
        }
        internal static SaveRecoverySummary ReadSnapshot(ILocalSaveStorage storage, string name, SnapshotDescriptor expected, SaveStoreBudget budget)
        {
            var summary = ReadFile(storage, name, stream => LocalSaveFailure.Core(SaveEnvelopeCodec.ReadRequirements(stream, expected, budget.Codec)));
            foreach (var row in summary.CommitIndex)
                LocalSaveFailure.Need(SaveFileNames.Commit(row.CommitId) && (row.ParentCommitId == null || SaveFileNames.Commit(row.ParentCommitId)),
                    "InconsistentBinding", "CommitIndex.Identity");
            return summary;
        }
    }
}
