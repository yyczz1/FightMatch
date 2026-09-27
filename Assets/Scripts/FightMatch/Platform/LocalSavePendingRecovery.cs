using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FightMatch.Core;

namespace FightMatch.Platform
{
    public sealed partial class LocalSaveStore
    {
        public LocalSaveResult<SaveRecoveredCandidate> ReadUncommittedCandidate(SaveRecoveryView expected,
            string commitId, SaveStoreBudget budget)
        { return ReadUncommittedCandidate(expected, commitId, SavePurpose.CandidateValidation, budget); }
        public LocalSaveResult<SaveRecoveredCandidate> ReadUncommittedCandidate(SaveRecoveryView expected,
            string commitId, SavePurpose purpose, SaveStoreBudget budget)
        {
            if (expected == null) throw new ArgumentNullException(nameof(expected));
            if (commitId == null) throw new ArgumentNullException(nameof(commitId));
            return Execute(budget, "ReadUncommittedCandidate", () =>
            {
                CheckPendingRecovery(commitId, purpose);
                var fresh = RecheckRecovery(expected, budget);
                var candidate = ReadObservedCandidate(fresh, commitId, budget);
                RecheckRecovery(expected, budget);
                return LocalSaveResult<SaveRecoveredCandidate>.Accept("CandidateRead", candidate);
            });
        }

        public LocalSaveResult<SaveCommitTicket> ResumeRecoveredCandidate(SaveRecoveredCandidate candidate,
            SaveRecoveryCapabilities capabilities, SaveStoreBudget budget)
        { return ResumeRecoveredCandidate(candidate, capabilities, SavePurpose.CandidateValidation, budget); }
        public LocalSaveResult<SaveCommitTicket> ResumeRecoveredCandidate(SaveRecoveredCandidate candidate,
            SaveRecoveryCapabilities capabilities, SavePurpose purpose, SaveStoreBudget budget)
        {
            if (candidate == null) throw new ArgumentNullException(nameof(candidate));
            if (capabilities == null) throw new ArgumentNullException(nameof(capabilities));
            return Execute(budget, "ResumeRecoveredCandidate", () =>
            {
                CheckPendingRecovery(candidate.Metadata.CommitId, purpose);
                LocalSaveFailure.Need(ReferenceEquals(candidate.Owner, this), "StaleContext", "Candidate.Owner");
                var fresh = RecheckRecovery(candidate.Observation, budget);
                var reread = ReadObservedCandidate(fresh, candidate.Metadata.CommitId, budget);
                CheckMetadata(candidate.Metadata, reread.Envelope, budget);
                LocalSaveFailure.Need(LocalSaveValues.Same(candidate.ExpectedHead, reread.ExpectedHead, budget) &&
                    LocalSaveValues.Same(candidate.Descriptor, reread.Descriptor, budget) &&
                    candidate.OperationIds.SequenceEqual(reread.OperationIds, StringComparer.Ordinal) &&
                    SameCandidateBytes(candidate.Envelope, reread.Envelope, budget), "StaleContext", "Candidate.Envelope");
                capabilities.CheckBudget(budget);
                foreach (var root in fresh.Current == null ? fresh.RetainedRoots : new[] { fresh.Current }.Concat(fresh.RetainedRoots))
                    CheckCapabilities(root, capabilities);
                RecheckRecovery(candidate.Observation, budget);
                var ticket = new SaveCommitTicket(this, candidate.Metadata, candidate.ExpectedHead,
                    candidate.OperationIds, candidate.Envelope, candidate.Descriptor);
                pending = ticket;
                return LocalSaveResult<SaveCommitTicket>.Accept("Resumed", ticket);
            });
        }

        public LocalSaveResult<SaveCandidateEndResult> EndObservedCandidate(SaveRecoveryView expected,
            string commitId, SaveStoreBudget budget)
        { return EndObservedCandidate(expected, commitId, SavePurpose.CandidateValidation, budget); }
        public LocalSaveResult<SaveCandidateEndResult> EndObservedCandidate(SaveRecoveryView expected,
            string commitId, SavePurpose purpose, SaveStoreBudget budget)
        {
            if (expected == null) throw new ArgumentNullException(nameof(expected));
            if (commitId == null) throw new ArgumentNullException(nameof(commitId));
            return Execute(budget, "EndObservedCandidate", () =>
            {
                CheckPendingRecovery(commitId, purpose);
                LocalSaveFailure.Need(ReferenceEquals(expected.Owner, this), "StaleContext", "Recovery.Owner");
                var stage = "EndCandidate.Observe";
                try
                {
                    var fresh = ObserveEndingCandidate(budget);
                    LocalSaveFailure.Need(ReferenceEquals(expected.Ticket, fresh.Ticket) &&
                        LocalSaveValues.Bytes(expected.Evidence, fresh.Evidence), "StaleContext", "Recovery.Evidence");
                    RequireEndEvidence(fresh);
                    RequireUnindexed(fresh, commitId);
                    var targets = fresh.Files.Where(x => x.CommitId == commitId && x.Disposition == SaveFileDisposition.Pending)
                        .OrderBy(x => x.Kind == SaveFileKind.MarkerWork ? 0 : x.Kind == SaveFileKind.SnapshotWork ? 1 : 2).ToArray();
                    LocalSaveFailure.Need(targets.Length != 0, "CandidateNotFound", "Candidate.CommitId");
                    LocalSaveFailure.Need(targets.All(x => x.Kind == SaveFileKind.MarkerWork ||
                        x.Kind == SaveFileKind.SnapshotWork || x.Kind == SaveFileKind.Snapshot), "RecoveryBlocked", "Candidate.Files");
                    var removed = new List<string>();
                    foreach (var target in targets)
                    {
                        RecheckEndingCandidate(fresh, commitId, removed, budget);
                        stage = "EndCandidate." + target.Name + ".Verify";
                        var bytes = HashRecoveryFile(target, budget);
                        MatchBytes(target, bytes.Length.Value, bytes.Sha256);
                        stage = "EndCandidate." + target.Name + ".Delete";
                        storage.DeleteUncommitted(target.Name);
                        removed.Add(target.Name);
                    }
                    var final = RecheckEndingCandidate(fresh, commitId, removed, budget);
                    LocalSaveFailure.Need(!final.Files.Any(x => x.CommitId == commitId), "StaleContext", "Candidate.Remaining");
                    return LocalSaveResult<SaveCandidateEndResult>.Accept("Ended",
                        new SaveCandidateEndResult(commitId, final.Current?.Descriptor, removed));
                }
                catch (Exception error) when (LocalSaveFailure.Expected(error))
                {
                    var failure = LocalSaveFailure.From(error, stage);
                    return LocalSaveResult<SaveCandidateEndResult>.Reject(failure,
                        failure.Code == "StorageFailure" ? "EndInterrupted" : null);
                }
            });
        }

        private void CheckPendingRecovery(string commitId, SavePurpose purpose)
        {
            LocalSaveFailure.Need(pending == null, "Busy", "PendingTicket");
            LocalSaveFailure.Need((purpose == SavePurpose.CandidateValidation || purpose == SavePurpose.PlayerSave) && storage.Profile.Purpose == purpose, "UnsupportedBinding", "Profile.Purpose");
            LocalSaveFailure.Need(SaveFileNames.Commit(commitId), "InvalidValue", "CommitId");
        }

        private static void RequireUnindexed(SaveRecoveryView view, string commitId)
        {
            LocalSaveFailure.Need(view.Summary == null || !view.Summary.CommitIndex.Any(x => x.CommitId == commitId),
                "AlreadyCommitted", "Candidate.CommitId");
            LocalSaveFailure.Need(!view.Files.Any(x => x.CommitId == commitId && x.Kind == SaveFileKind.Marker),
                "RecoveryBlocked", "Candidate.Marker");
        }

        private SaveRecoveredCandidate ReadObservedCandidate(SaveRecoveryView view, string commitId, SaveStoreBudget budget)
        {
            RequireComplete(view);
            RequireUnindexed(view, commitId);
            var files = view.Files.Where(x => x.Disposition == SaveFileDisposition.Pending && x.CommitId == commitId &&
                (x.Kind == SaveFileKind.Snapshot || x.Kind == SaveFileKind.SnapshotWork))
                .OrderBy(x => x.Kind == SaveFileKind.Snapshot ? 0 : 1).ToArray();
            LocalSaveFailure.Need(files.Length != 0, "CandidateNotFound", "Candidate.CommitId");
            LocalSaveFailure.Need(!view.Files.Any(x => x.Disposition == SaveFileDisposition.Pending && x.CommitId != commitId),
                "Pending", "Candidate.OtherCommit");
            SaveEnvelope envelope = null;
            SnapshotDescriptor descriptor = null;
            foreach (var file in files)
            {
                var root = view.RetainedRoots.FirstOrDefault(x => x.Kind == SaveRecoveryRootKind.Pending && x.SourceName == file.Name);
                LocalSaveFailure.Need(root != null, "RecoveryBlocked", "Candidate.Root");
                var original = RecoveryRead(file.Name, "Candidate.Read",
                    stream => LocalSaveFailure.Core(SaveEnvelopeCodec.Read(stream, root.Descriptor, budget.Codec)));
                LocalSaveFailure.Need(original.CommitId == file.CommitId && original.PlayerId == storage.Profile.PlayerId &&
                    original.Purpose == storage.Profile.Purpose, "InconsistentBinding", "Candidate.Identity");
                MatchBytes(file, root.Descriptor.TotalLength, root.Descriptor.Sha256);
                if (envelope != null)
                    LocalSaveFailure.Need(LocalSaveValues.Same(descriptor, root.Descriptor, budget) &&
                        SameCandidateBytes(envelope, original, budget), "InconsistentBinding", "Candidate.Copies");
                else
                {
                    envelope = original;
                    descriptor = root.Descriptor;
                }
            }
            CheckCandidateChain(view, envelope, budget);
            return new SaveRecoveredCandidate(this, view, envelope, descriptor);
        }

        private static void CheckCandidateChain(SaveRecoveryView view, SaveEnvelope envelope, SaveStoreBudget budget)
        {
            var head = view.Summary;
            LocalSaveFailure.Need(envelope.ParentCommitId == head?.Descriptor.CommitId, "StaleContext", "Candidate.ParentCommitId");
            var one = ExactRational.Create(1, 1, budget.Codec.Math);
            var generation = head == null ? one.Numerator :
                ExactRational.Create(head.Descriptor.SaveGeneration, 1, budget.Codec.Math).Add(one, budget.Codec.Math).Numerator;
            LocalSaveFailure.Need(LocalSaveValues.Compare(envelope.SaveGeneration, generation, budget) == 0,
                "InconsistentBinding", "Candidate.SaveGeneration");
            var count = head?.CommitIndex.Count ?? 0;
            LocalSaveFailure.Need(envelope.CommitIndex.Count == count + 1, "InconsistentBinding", "Candidate.CommitIndex");
            var current = envelope.CommitIndex[count];
            LocalSaveFailure.Need(current.CommitId == envelope.CommitId && current.ParentCommitId == envelope.ParentCommitId &&
                LocalSaveValues.Compare(current.Generation, generation, budget) == 0 &&
                current.SnapshotLength == null && current.SnapshotSha256 == null, "InconsistentBinding", "Candidate.CurrentRow");
            var operations = new HashSet<string>(current.OperationIds, StringComparer.Ordinal);
            for (var i = 0; i < count; i++)
            {
                var old = head.CommitIndex[i];
                var expected = old.CommitId == head.Descriptor.CommitId ?
                    new SaveCommitIndexEntry(old.Generation, old.CommitId, old.ParentCommitId,
                        head.Descriptor.TotalLength, head.Descriptor.Sha256, old.OperationIds) : old;
                LocalSaveFailure.Need(LocalSaveValues.Same(expected, envelope.CommitIndex[i], budget),
                    "InconsistentBinding", "Candidate.CommitIndex[" + i + "]");
                LocalSaveFailure.Need(!old.OperationIds.Any(operations.Contains), "InconsistentBinding", "Candidate.OperationIds");
            }
        }

        private static bool SameCandidateBytes(SaveEnvelope first, SaveEnvelope second, SaveStoreBudget budget)
        {
            using (var a = new MemoryStream())
            using (var b = new MemoryStream())
            {
                LocalSaveFailure.Core(SaveEnvelopeCodec.Write(a, first, budget.Codec));
                LocalSaveFailure.Core(SaveEnvelopeCodec.Write(b, second, budget.Codec));
                return a.Length == b.Length && a.ToArray().SequenceEqual(b.ToArray());
            }
        }

        private SaveRecoveryView ObserveEndingCandidate(SaveStoreBudget budget)
        {
            var view = ObserveRecovery(budget);
            // Observation retains unreadable orphan diagnostics instead of throwing them.
            // Ending must preserve that original I/O failure, before comparing evidence.
            var io = view.Diagnostics.FirstOrDefault(x => x.Code == "StorageFailure");
            if (io != null) throw io.Failure;
            return view;
        }

        private static void RequireEndEvidence(SaveRecoveryView view)
        {
            LocalSaveFailure.Need(view.Status != SaveHeadStatus.RecoveryBlocked, "RecoveryBlocked", "Recovery.Status");
            LocalSaveFailure.Need(view.EvidenceComplete, "RecoveryBlocked", "Recovery.EvidenceComplete");
        }

        private SaveRecoveryView RecheckEndingCandidate(SaveRecoveryView original, string commitId,
            IReadOnlyList<string> removed, SaveStoreBudget budget)
        {
            var fresh = ObserveEndingCandidate(budget);
            var expectedFiles = original.Files.Where(x => !removed.Contains(x.Name, StringComparer.Ordinal)).ToArray();
            LocalSaveFailure.Need(expectedFiles.Length == fresh.Files.Count, "StaleContext", "Recovery.Directory");
            for (var i = 0; i < expectedFiles.Length; i++)
            {
                var a = expectedFiles[i];
                var b = fresh.Files[i];
                LocalSaveFailure.Need(a.Name == b.Name && a.Kind == b.Kind && a.CommitId == b.CommitId &&
                    a.Disposition == b.Disposition && a.Length == b.Length && LocalSaveValues.Bytes(a.Sha256, b.Sha256),
                    "StaleContext", "Recovery.Evidence");
            }
            LocalSaveFailure.Need(LocalSaveValues.Same(original.Current?.Descriptor, fresh.Current?.Descriptor, budget),
                "StaleContext", "Candidate.CurrentHead");
            if (original.Summary != null)
            {
                LocalSaveFailure.Need(original.Summary.CommitIndex.Count == fresh.Summary.CommitIndex.Count,
                    "StaleContext", "Candidate.CurrentIndex");
                for (var i = 0; i < original.Summary.CommitIndex.Count; i++)
                    LocalSaveFailure.Need(LocalSaveValues.Same(original.Summary.CommitIndex[i], fresh.Summary.CommitIndex[i], budget),
                        "StaleContext", "Candidate.CurrentIndex");
            }
            RequireEndEvidence(fresh);
            RequireUnindexed(fresh, commitId);
            return fresh;
        }
    }
}
