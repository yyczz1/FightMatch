using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Security.Cryptography;
using System.Text;
using FightMatch.Core;

namespace FightMatch.Platform
{
    public sealed partial class LocalSaveStore
    {
        public LocalSaveResult<SaveEnvelope> Load(SaveStoreBudget budget)
        {
            return Execute(budget, "Load", () =>
            {
                var head = SaveHeadInspection.Read(storage, budget);
                LocalSaveFailure.Need(head.Status != SaveHeadStatus.RecoveryBlocked, "RecoveryBlocked", "Head");
                LocalSaveFailure.Need(pending == null && head.Status != SaveHeadStatus.Pending, "CommitUnknown", "Head");
                LocalSaveFailure.Need(head.Status == SaveHeadStatus.Ready && head.Current != null, "NoSave", "Head");
                var envelope = RecoveryRead(SaveFileNames.Snapshot(head.Current.Descriptor.CommitId), "Load",
                    stream => LocalSaveFailure.Core(SaveEnvelopeCodec.Read(stream, head.Current.Descriptor, budget.Codec)));
                return LocalSaveResult<SaveEnvelope>.Accept("Loaded", envelope);
            });
        }

        public LocalSaveResult<SaveRecoveryView> ReadRecovery(SaveStoreBudget budget)
        { return Execute(budget, "ReadRecovery", () => LocalSaveResult<SaveRecoveryView>.Accept("RecoveryObserved", ObserveRecovery(budget))); }

        public LocalSaveResult<bool> WithVerifiedRecovery(SaveRecoveryView expected, SaveRecoveryCapabilities capabilities,
            Action<SaveRecoveryView> action, SaveStoreBudget budget)
        {
            if (expected == null) throw new ArgumentNullException(nameof(expected));
            if (capabilities == null) throw new ArgumentNullException(nameof(capabilities));
            if (action == null) throw new ArgumentNullException(nameof(action));
            ExceptionDispatchInfo callbackError = null;
            var result = Execute(budget, "WithVerifiedRecovery", () =>
            {
                var fresh = RecheckRecovery(expected, budget); RequireComplete(fresh);
                LocalSaveFailure.Need(fresh.Status != SaveHeadStatus.NoSave || mode == SaveOpenMode.CreateNew, "InitializationRequired", "OpenMode");
                capabilities.CheckBudget(budget);
                foreach (var root in (fresh.Current == null ? fresh.RetainedRoots : new[] { fresh.Current }.Concat(fresh.RetainedRoots)))
                    CheckCapabilities(root, capabilities);
                // Even I/O exceptions from caller code must escape unchanged, after Execute releases the gate.
                try { action(fresh); }
                catch (Exception error) { callbackError = ExceptionDispatchInfo.Capture(error); }
                return LocalSaveResult<bool>.Accept("Verified", true);
            });
            if (callbackError != null) callbackError.Throw();
            return result;
        }

        public LocalSaveResult<SaveCleanupResult> Cleanup(SaveRecoveryView expected, SaveStoreBudget budget)
        {
            if (expected == null) throw new ArgumentNullException(nameof(expected));
            return Execute(budget, "Cleanup", () =>
            {
                var fresh = RecheckRecovery(expected, budget); RequireComplete(fresh);
                LocalSaveFailure.Need(fresh.Status == SaveHeadStatus.Ready && fresh.Current != null, "RecoveryBlocked", "Cleanup.Head");
                LocalSaveFailure.Need(!fresh.HasUnresolvedCandidate && pending == null, "CommitUnknown", "Cleanup.Candidate");
                var head = fresh.Current.Descriptor; var removed = new List<string>(); var stage = "Cleanup.Select";
                try
                {
                    foreach (var row in fresh.Summary.CommitIndex)
                    {
                        if (row.CommitId == head.CommitId || row.CommitId == head.ParentCommitId) continue;
                        foreach (var name in new[] { SaveFileNames.Marker(row.CommitId), SaveFileNames.Snapshot(row.CommitId) })
                        {
                            var old = fresh.Files.FirstOrDefault(x => x.Name == name);
                            if (old == null) continue;
                            LocalSaveFailure.Need(old.Disposition == SaveFileDisposition.IndexedOldCopy && old.Sha256 != null,
                                "RecoveryBlocked", "Cleanup.Target");
                            stage = "Cleanup." + name + ".Verify"; var now = HashRecoveryFile(old, budget);
                            LocalSaveFailure.Need(old.Length == now.Length && LocalSaveValues.Bytes(old.Sha256, now.Sha256), "StaleContext", "Cleanup.Target");
                            stage = "Cleanup." + name + ".Delete"; storage.DeleteIndexedOld(name); removed.Add(name);
                        }
                    }
                }
                catch (Exception error) when (LocalSaveFailure.Expected(error))
                {
                    var f = LocalSaveFailure.From(error, stage);
                    return LocalSaveResult<SaveCleanupResult>.Reject(new LocalSaveFailure(f.Code, f.Path, f.Reason,
                        f.Required, f.Allowed, f.Cause, stage), "CleanupInterrupted");
                }
                return LocalSaveResult<SaveCleanupResult>.Accept("Cleaned", new SaveCleanupResult(head, removed));
            });
        }

        private SaveRecoveryView RecheckRecovery(SaveRecoveryView expected, SaveStoreBudget budget)
        {
            LocalSaveFailure.Need(ReferenceEquals(expected.Owner, this), "StaleContext", "Recovery.Owner");
            var fresh = ObserveRecovery(budget);
            LocalSaveFailure.Need(ReferenceEquals(expected.Ticket, fresh.Ticket) && LocalSaveValues.Bytes(expected.Evidence, fresh.Evidence),
                "StaleContext", "Recovery.Evidence");
            return fresh;
        }
        private static void RequireComplete(SaveRecoveryView view)
        {
            LocalSaveFailure.Need(view.Status != SaveHeadStatus.RecoveryBlocked, "RecoveryBlocked", "Recovery.Status");
            LocalSaveFailure.Need(view.EvidenceComplete, "RecoveryBlocked", "Recovery.EvidenceComplete");
            LocalSaveFailure.Need(view.RequirementsComplete, "RecoveryBlocked", "Recovery.RequirementsComplete");
        }
        private static void CheckCapabilities(SaveRecoveryRoot root, SaveRecoveryCapabilities caps)
        {
            var prefix = "Recovery[" + root.SourceName + "]";
            for (var i = 0; i < root.RequiredSliceContracts.Count; i++)
                LocalSaveFailure.Need(caps.ReadableSlices.Any(x => SaveRecoveryCapabilities.SameSlice(x, root.RequiredSliceContracts[i])),
                    "UnsupportedCapability", prefix + ".RequiredSliceContracts[" + i + "]");
            var requirements = root.Requirements;
            for (var i = 0; i < requirements.Bindings.Count; i++)
                LocalSaveFailure.Need(caps.Bindings.Any(x => SaveRecoveryCapabilities.SameBinding(x, requirements.Bindings[i])),
                    "UnsupportedBinding", prefix + ".Bindings[" + i + "]");
            CheckVersions(requirements.RuleVersions, caps.RuleVersions, prefix + ".RuleVersions");
            CheckVersions(requirements.NumericContractVersions, caps.NumericContractVersions, prefix + ".NumericContractVersions");
            CheckVersions(requirements.RandomContractVersions, caps.RandomContractVersions, prefix + ".RandomContractVersions");
            CheckVersions(requirements.FeatureIds, caps.FeatureIds, prefix + ".FeatureIds");
        }
        private static void CheckVersions(IReadOnlyList<string> needs, IReadOnlyList<string> available, string path)
        {
            for (var i = 0; i < needs.Count; i++) LocalSaveFailure.Need(available.Contains(needs[i], StringComparer.Ordinal),
                "UnsupportedCapability", path + "[" + i + "]");
        }

        private SaveRecoveryView ObserveRecovery(SaveStoreBudget budget)
        {
            var head = SaveHeadInspection.Read(storage, budget);
            var files = new List<SaveFileObservation>(); var diagnostics = new List<LocalSaveDiagnostic>(head.Diagnostics);
            var roots = new List<SaveRecoveryRoot>(); var requirements = head.Status != SaveHeadStatus.RecoveryBlocked;
            var evidenceComplete = true; var degraded = false; var unresolved = pending != null;
            var current = head.Current == null ? null : Root(SaveRecoveryRootKind.Current, SaveFileNames.Snapshot(head.Current.Descriptor.CommitId), head.Current);
            foreach (var file in head.Files.OrderBy(x => x.Name, StringComparer.Ordinal))
            {
                if (file.Kind == SaveFileKind.WriterLock) { files.Add(file); continue; }
                if (file.Kind == SaveFileKind.Unknown)
                { files.Add(file); requirements = false; evidenceComplete = false; continue; }
                try
                {
                    var observed = HashRecoveryFile(file, budget);
                    if (file.Sha256 != null) LocalSaveFailure.Need(file.Length == observed.Length && LocalSaveValues.Bytes(file.Sha256, observed.Sha256),
                        "StaleContext", "Recovery." + file.Name);
                    files.Add(observed);
                }
                catch (Exception error) when (LocalSaveFailure.Expected(error))
                {
                    var f = LocalSaveFailure.From(error, "Recovery." + file.Name);
                    if (f.Code == "Limit" || f.Code == "StaleContext" || file.Disposition == SaveFileDisposition.Current) throw f;
                    files.Add(new SaveFileObservation(file.Name, file.Kind, file.CommitId, file.Disposition, null, null, "EvidenceUnreadable", f));
                    diagnostics.Add(new LocalSaveDiagnostic(f)); evidenceComplete = false;
                }
            }
            if (head.Current != null)
            {
                foreach (var row in head.Current.CommitIndex)
                {
                    if (row.CommitId == head.Current.Descriptor.CommitId) continue;
                    var marker = files.FirstOrDefault(x => x.Name == SaveFileNames.Marker(row.CommitId));
                    var body = files.FirstOrDefault(x => x.Name == SaveFileNames.Snapshot(row.CommitId));
                    if (marker == null && body == null && row.CommitId != head.Current.Descriptor.ParentCommitId) continue;
                    try
                    {
                        LocalSaveFailure.Need(marker != null && body != null, "RedundancyDegraded", "Backup[" + row.CommitId + "].Pair");
                        var expected = LocalSaveValues.Descriptor(head.Current, row);
                        var m = ReadRecoveryMarker(marker, budget);
                        LocalSaveFailure.Need(LocalSaveValues.Same(m.Descriptor, expected, budget), "InconsistentBinding", "Backup.Marker");
                        var summary = RecoveryRead(body.Name, "Recovery", stream => LocalSaveFailure.Core(SaveEnvelopeCodec.ReadRequirements(stream, expected, budget.Codec)));
                        MatchBytes(body, summary.Descriptor.TotalLength, summary.Descriptor.Sha256);
                        roots.Add(Root(SaveRecoveryRootKind.Backup, body.Name, summary));
                    }
                    catch (Exception error) when (LocalSaveFailure.Expected(error))
                    {
                        var f = LocalSaveFailure.From(error, "Recovery.Backup[" + row.CommitId + "]");
                        if (f.Code == "Limit" || f.Code == "StaleContext") throw f;
                        diagnostics.Add(new LocalSaveDiagnostic(f)); degraded = true;
                    }
                }
            }
            var candidates = new Dictionary<string, SaveRecoverySummary>(StringComparer.Ordinal);
            foreach (var file in files.Where(x => x.Disposition == SaveFileDisposition.Pending &&
                (x.Kind == SaveFileKind.SnapshotWork || x.Kind == SaveFileKind.Snapshot)))
            {
                unresolved = true;
                try
                {
                    var summary = RecoveryRead(file.Name, "Recovery", stream => LocalSaveFailure.Core(SaveEnvelopeCodec.ReadUncommittedRequirements(stream, budget.Codec)));
                    var d = summary.Descriptor;
                    LocalSaveFailure.Need(d.PlayerId == storage.Profile.PlayerId && d.Purpose == storage.Profile.Purpose &&
                        d.CommitId == file.CommitId, "InconsistentBinding", "Pending.Identity");
                    foreach (var row in summary.CommitIndex) LocalSaveFailure.Need(SaveFileNames.Commit(row.CommitId) &&
                        (row.ParentCommitId == null || SaveFileNames.Commit(row.ParentCommitId)), "InconsistentBinding", "Pending.CommitIndex");
                    MatchBytes(file, d.TotalLength, d.Sha256);
                    SaveRecoverySummary other;
                    LocalSaveFailure.Need(!candidates.TryGetValue(file.CommitId, out other) || LocalSaveValues.Same(other.Descriptor, d, budget),
                        "InconsistentBinding", "Pending.Copies");
                    candidates[file.CommitId] = summary; roots.Add(Root(SaveRecoveryRootKind.Pending, file.Name, summary));
                }
                catch (Exception error) when (LocalSaveFailure.Expected(error))
                {
                    var f = LocalSaveFailure.From(error, "Recovery.Pending[" + file.Name + "]");
                    if (f.Code == "Limit" || f.Code == "StaleContext") throw f;
                    diagnostics.Add(new LocalSaveDiagnostic(f)); requirements = false;
                }
            }
            foreach (var file in files.Where(x => x.Kind == SaveFileKind.MarkerWork && x.Disposition == SaveFileDisposition.Pending))
            {
                unresolved = true;
                try
                {
                    var marker = ReadRecoveryMarker(file, budget); SaveRecoverySummary candidate;
                    var known = candidates.TryGetValue(file.CommitId, out candidate) ? candidate.Descriptor : null;
                    LocalSaveFailure.Need(known != null && LocalSaveValues.Same(marker.Descriptor, known, budget), "InconsistentBinding", "Pending.Marker");
                }
                catch (Exception error) when (LocalSaveFailure.Expected(error))
                {
                    var f = LocalSaveFailure.From(error, "Recovery.Pending[" + file.Name + "]");
                    if (f.Code == "Limit" || f.Code == "StaleContext") throw f;
                    diagnostics.Add(new LocalSaveDiagnostic(f)); requirements = false;
                }
            }
            if (pending != null)
            {
                var checkedDescriptor = LocalSaveFailure.Core(SaveEnvelopeCodec.Write(Stream.Null, pending.Envelope, budget.Codec));
                LocalSaveFailure.Need(LocalSaveValues.Same(checkedDescriptor, pending.Descriptor, budget), "InconsistentBinding", "Pending.Descriptor");
                roots.Add(new SaveRecoveryRoot(SaveRecoveryRootKind.Pending, "pending-ticket", pending.Descriptor,
                    pending.Envelope.RequiredSliceContracts, pending.Envelope.RecoveryRequirements));
            }
            var endNames = RecoveryNames(budget);
            LocalSaveFailure.Need(files.Select(x => x.Name).SequenceEqual(endNames, StringComparer.Ordinal), "StaleContext", "Recovery.Directory");
            var status = head.Status == SaveHeadStatus.RecoveryBlocked ? head.Status : unresolved ? SaveHeadStatus.Pending : head.Status;
            var evidence = RecoveryEvidence(status, files, requirements, evidenceComplete, degraded);
            return new SaveRecoveryView(this, status, current, roots, files, diagnostics, requirements, evidenceComplete,
                unresolved, degraded, evidence, pending, head.Current);
        }

        private static SaveRecoveryRoot Root(SaveRecoveryRootKind kind, string source, SaveRecoverySummary summary)
        { return new SaveRecoveryRoot(kind, source, summary.Descriptor, summary.RequiredSliceContracts, summary.RecoveryRequirements); }
        private SaveCommitMarker ReadRecoveryMarker(SaveFileObservation file, SaveStoreBudget budget)
        {
            var marker = RecoveryRead(file.Name, "Recovery", stream => SaveCommitMarkerCodec.Read(stream, file.Name, budget));
            MatchBytes(file, marker.Length, marker.Sha256); return marker;
        }
        private static void MatchBytes(SaveFileObservation file, ulong length, IReadOnlyList<byte> hash)
        { LocalSaveFailure.Need(file.Length == length && LocalSaveValues.Bytes(file.Sha256, hash), "StaleContext", "Recovery." + file.Name); }
        private string[] RecoveryNames(SaveStoreBudget budget)
        {
            try
            {
                var names = new HashSet<string>(StringComparer.Ordinal);
                foreach (var name in storage.EnumerateNames())
                {
                    LocalSaveFailure.Limit((ulong)names.Count + 1, (ulong)budget.MaxDirectoryEntries, "Directory", "DirectoryEntries");
                    LocalSaveFailure.Need(name != null && names.Add(name), "StaleContext", "Recovery.Directory");
                }
                return names.OrderBy(x => x, StringComparer.Ordinal).ToArray();
            }
            catch (Exception error) when (LocalSaveFailure.Expected(error)) { throw LocalSaveFailure.From(error, "Recovery.Enumerate"); }
        }
        private SaveFileObservation HashRecoveryFile(SaveFileObservation file, SaveStoreBudget budget)
        {
            return RecoveryRead(file.Name, "Recovery.Hash", stream =>
            {
                var limit = file.Kind == SaveFileKind.Marker || file.Kind == SaveFileKind.MarkerWork ? budget.MaxMarkerBytes : budget.Codec.MaxEnvelopeBytes;
                using (var hash = SHA256.Create())
                {
                    var bytes = new byte[8192]; var used = 0UL;
                    while (true)
                    {
                        var left = limit - used; var count = left >= (ulong)bytes.Length ? bytes.Length : (int)left + 1;
                        var read = stream.Read(bytes, 0, count); if (read == 0) break;
                        LocalSaveFailure.Need(read > 0 && read <= count, "StorageFailure", "Recovery.ReadCount");
                        LocalSaveFailure.Limit((ulong)read, left, "Recovery." + file.Name, "FileBytes");
                        used += (ulong)read; hash.TransformBlock(bytes, 0, read, bytes, 0);
                    }
                    hash.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
                    return new SaveFileObservation(file.Name, file.Kind, file.CommitId, file.Disposition, used, hash.Hash, file.Detail, file.Diagnostic?.Failure);
                }
            });
        }
        private T RecoveryRead<T>(string name, string stage, Func<Stream, T> read)
        {
            Stream stream;
            try { stream = storage.OpenRead(name); }
            catch (Exception error) when (LocalSaveFailure.Expected(error)) { throw LocalSaveFailure.From(error, stage + "." + name + ".Open"); }
            try
            {
                try { return read(stream); }
                catch (Exception error) when (LocalSaveFailure.Expected(error)) { throw LocalSaveFailure.From(error, stage + "." + name + ".Read"); }
            }
            finally
            {
                try { stream.Dispose(); }
                catch (Exception error) when (LocalSaveFailure.Expected(error)) { throw LocalSaveFailure.From(error, stage + "." + name + ".Close"); }
            }
        }
        private byte[] RecoveryEvidence(SaveHeadStatus status, List<SaveFileObservation> files, bool requirements, bool complete, bool degraded)
        {
            using (var buffer = new MemoryStream())
            using (var writer = new BinaryWriter(buffer, Encoding.Unicode, true))
            using (var hash = SHA256.Create())
            {
                writer.Write(storage.Profile.PlayerId); writer.Write((byte)storage.Profile.Purpose); writer.Write(storage.Profile.DirectoryPath);
                writer.Write((byte)storage.Profile.SupportedFaultModel); writer.Write((int)status); writer.Write(requirements); writer.Write(complete); writer.Write(degraded);
                writer.Write(files.Count);
                foreach (var file in files)
                {
                    writer.Write(file.Name); writer.Write((int)file.Kind); writer.Write((int)file.Disposition); writer.Write(file.CommitId ?? "");
                    writer.Write(file.Length.HasValue); if (file.Length.HasValue) writer.Write(file.Length.Value);
                    writer.Write(file.Sha256 != null); if (file.Sha256 != null) writer.Write(file.Sha256.ToArray());
                    writer.Write(file.Detail ?? ""); writer.Write(file.Diagnostic?.Code ?? ""); writer.Write(file.Diagnostic?.Stage ?? "");
                    writer.Write(file.Diagnostic?.ExceptionType ?? ""); writer.Write(file.Diagnostic?.ExceptionMessage ?? "");
                }
                writer.Write(pending != null);
                if (pending != null)
                { writer.Write(pending.Metadata.CommitId); writer.Write(pending.Ended); writer.Write(pending.Committed); writer.Write(pending.Descriptor.Sha256.ToArray()); }
                writer.Flush(); return hash.ComputeHash(buffer.GetBuffer(), 0, (int)buffer.Length);
            }
        }
    }
}
