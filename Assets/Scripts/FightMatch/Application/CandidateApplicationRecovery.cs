using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
using FightMatch.Core;
using FightMatch.Platform;

namespace FightMatch.Application
{
    internal sealed partial class CandidateApplicationRuntime
    {
        internal CandidateApplicationCallResult Open(ILocalSaveStorage storage, string playerId, SaveOpenMode mode,
            SaveRecoveryCapabilities available, SaveStoreBudget budget, PlayerSessionSystem session = null)
        {
            return Execute(() =>
            {
                if (storage == null) throw new ArgumentNullException(nameof(storage));
                if (playerId == null) throw new ArgumentNullException(nameof(playerId));
                if (available == null) throw new ArgumentNullException(nameof(available));
                if (budget == null) throw new ArgumentNullException(nameof(budget));
                if (store != null) return Reject("AlreadyOpen", "Application.Store");
                var purpose = session == null ? SavePurpose.CandidateValidation : SavePurpose.PlayerSave;
                var opened = LocalSaveStore.Open(storage, playerId, purpose,
                    mode, SaveFaultModel.EditorProcessCrash, budget);
                if (!opened.IsAccepted) return Result(opened.Code, CandidateApplicationDiagnostic.From(opened));
                store = opened.Value;
                recoveryStorage = storage;
                playerSession = session;
                capabilities = available;
                view = new CandidateApplicationView(playerId, CandidateApplicationPhase.RestoreRequired,
                    null, false, null, null, Array.Empty<string>(), null, purpose);
                return RestoreCore(budget);
            });
        }

        internal CandidateApplicationCallResult Restore(SaveStoreBudget budget)
        {
            return Execute(() =>
            {
                if (budget == null) throw new ArgumentNullException(nameof(budget));
                if (pending != null) return PendingResult();
                if (store == null) return Reject("ResolutionRequired", "Application.Store");
                return RestoreCore(budget);
            });
        }

        private CandidateApplicationCallResult RestoreFailure(string code, CandidateApplicationDiagnostic diagnostic)
        {
            SetState(pending?.Committed == true ? CandidateApplicationPhase.RestoreRequired :
                CandidateApplicationPhase.RecoveryBlocked, diagnostic);
            return pending?.Committed == true ? PendingResult(code, diagnostic) : Result(code, diagnostic);
        }

        private CandidateApplicationCallResult RestoreCore(SaveStoreBudget budget)
        {
            SetState(CandidateApplicationPhase.RestoreRequired);
            observation = null;
            var read = store.ReadRecovery(budget);
            if (!read.IsAccepted) return RestoreFailure(read.Code, CandidateApplicationDiagnostic.From(read));
            observation = read.Value;
            var candidates = new List<string>();
            foreach (var file in observation.Files)
                if (file.Disposition == SaveFileDisposition.Pending && file.CommitId != null &&
                    !candidates.Contains(file.CommitId, StringComparer.Ordinal))
                    candidates.Add(file.CommitId);
            candidates.Sort(StringComparer.Ordinal);
            view = new CandidateApplicationView(View.PlayerId, View.Phase, View.PublishedSnapshot, false,
                pending?.Intent.OperationId, pending?.Ticket?.Metadata.CommitId, candidates, null, View.Purpose);
            if (observation.Status == SaveHeadStatus.RecoveryBlocked)
            {
                var original = observation.Diagnostics.FirstOrDefault();
                var diagnostic = original == null ? new CandidateApplicationDiagnostic("RecoveryBlocked", "Recovery.Status") :
                    new CandidateApplicationDiagnostic(original.Code, original.FieldPath, stage: original.Stage,
                        exceptionType: original.ExceptionType, exceptionMessage: original.ExceptionMessage);
                return RestoreFailure("RecoveryBlocked", diagnostic);
            }
            var resolution = CheckPlayerRoots(observation, budget.Codec);
            if (resolution != null) return RestoreFailure(resolution.Code, resolution);
            if (observation.HasUnresolvedCandidate || observation.Status == SaveHeadStatus.Pending)
            {
                var checkedRoots = store.WithVerifiedRecovery(observation, capabilities, _ => { }, budget);
                if (!checkedRoots.IsAccepted)
                    return RestoreFailure(checkedRoots.Code, CandidateApplicationDiagnostic.From(checkedRoots));
                return RestoreFailure("Pending", new CandidateApplicationDiagnostic("Pending", "Recovery.Candidates"));
            }
            if (observation.Status == SaveHeadStatus.NoSave)
            {
                if (pending?.Committed == true || View.PublishedSnapshot != null || playerSession != null && !playerSession.CreationPending)
                    return RestoreFailure("NoSave", new CandidateApplicationDiagnostic("NoSave", "Recovery.Current"));
                var empty = store.WithVerifiedRecovery(observation, capabilities, _ =>
                {
                    view = new CandidateApplicationView(View.PlayerId, CandidateApplicationPhase.InitializationReady,
                        null, true, null, null, Array.Empty<string>(), null, View.Purpose);
                }, budget);
                return empty.IsAccepted ? Result("InitializationReady") :
                    RestoreFailure(empty.Code, CandidateApplicationDiagnostic.From(empty));
            }
            var loaded = store.Load(budget);
            if (!loaded.IsAccepted) return RestoreFailure(loaded.Code, CandidateApplicationDiagnostic.From(loaded));
            var decoded = DecodeApplication(loaded.Value, budget.Codec);
            if (!decoded.IsAccepted)
                return RestoreFailure(decoded.RejectionCode, CandidateApplicationDiagnostic.From(decoded, "Decode"));
            if (!SameDescriptor(decoded.Value.Descriptor, observation.Current?.Descriptor, budget))
                return RestoreFailure("StaleContext", new CandidateApplicationDiagnostic("StaleContext", "Recovery.Current"));
            var admission = playerSession?.CheckSnapshot(decoded.Value, budget.Codec);
            if (admission != null) return RestoreFailure(admission.Code, admission);
            var finishing = pending;
            CandidateApplicationLookup completed = null;
            if (finishing != null)
            {
                var lookup = CandidateApplicationProtocol.Lookup(decoded.Value, finishing.Intent, budget.Codec);
                if (!lookup.IsAccepted)
                    return RestoreFailure(lookup.RejectionCode, CandidateApplicationDiagnostic.From(lookup, "Restore.Lookup"));
                if (!lookup.Value.IsFound || lookup.Value.OriginalCommitId != finishing.Ticket.Metadata.CommitId)
                    return RestoreFailure("IncompleteOperationHistory",
                        new CandidateApplicationDiagnostic("IncompleteOperationHistory", "Restore.OriginalOperation"));
                completed = lookup.Value;
            }
            var changed = !SameDescriptor(View.PublishedSnapshot?.Descriptor, decoded.Value.Descriptor, budget);
            var next = new CandidateApplicationView(View.PlayerId, playerSession?.CreationPending == true
                ? CandidateApplicationPhase.CreationConfirmationRequired : CandidateApplicationPhase.Ready,
                decoded.Value, true, null, null, Array.Empty<string>(), null);
            var verified = store.WithVerifiedRecovery(observation, capabilities, _ =>
            {
                pending = null;
                view = next;
            }, budget);
            if (!verified.IsAccepted) return RestoreFailure(verified.Code, CandidateApplicationDiagnostic.From(verified));
            // The store gate is released. The application admission still excludes writes.
            if (changed)
            {
                try { publish?.Invoke(new CandidateApplicationPublished(next)); }
                catch (Exception error)
                {
                    notificationFailure = CandidateApplicationDiagnostic.From("NotificationFailure", "Published", error);
                }
            }
            return completed == null ? Result(next.Phase.ToString()) :
                Result("Completed", committed: true, commit: completed.OriginalCommitId,
                    lookup: completed, lookupViewCommit: next.PublishedSnapshot.Header.CommitId);
        }

        private SaveCodecResult<CandidateApplicationSnapshot> DecodeApplication(SaveEnvelope envelope, SaveCodecBudget budget)
        { return playerSession == null ? CandidateApplicationSaveCodec.Decode(envelope, budget)
            : CandidateApplicationSaveCodec.DecodePublished(envelope, playerSession.Resolved, budget); }

        private CandidateApplicationDiagnostic CheckPlayerRoots(SaveRecoveryView roots, SaveCodecBudget budget)
        {
            if (playerSession == null) return null;
            var resolution = playerSession.ResolveRoots(roots, budget); if (resolution != null) return resolution;
            try
            {
                foreach (var root in roots.Current == null ? roots.RetainedRoots : new[] { roots.Current }.Concat(roots.RetainedRoots))
                {
                    SaveEnvelope envelope;
                    if (root.SourceName == "pending-ticket")
                    {
                        envelope = pending?.Ticket?.Envelope;
                        if (envelope == null) return new CandidateApplicationDiagnostic("RecoveryBlocked", "Recovery.PendingTicket");
                    }
                    else using (var stream = recoveryStorage.OpenRead(root.SourceName))
                    {
                        var read = SaveEnvelopeCodec.Read(stream, root.Descriptor, budget);
                        if (!read.IsAccepted) return CandidateApplicationDiagnostic.From(read, "Recovery.Root");
                        envelope = read.Value;
                    }
                    var decoded = DecodeApplication(envelope, budget);
                    if (!decoded.IsAccepted) return CandidateApplicationDiagnostic.From(decoded, "Recovery.Root");
                    var admission = playerSession.CheckSnapshot(decoded.Value, budget); if (admission != null) return admission;
                }
                return null;
            }
            catch (Exception error) when (error is IOException || error is UnauthorizedAccessException)
            { return CandidateApplicationDiagnostic.From("RecoveryBlocked", "Recovery.Root", error); }
        }

        internal CandidateApplicationCallResult ConfirmProfile(LocalPlayerProfileLocator locator, LocalPlayerProfileObservation expected, SaveStoreBudget budget)
        {
            return Execute(() =>
            {
                if (playerSession == null || !View.IsPublishedHeadVerified || View.PublishedSnapshot == null)
                    return Reject("ResolutionRequired", "Initialization.Head");
                var proof = PlayerProfileCreateRecordCodec.VerifyInitializationCommit(playerSession.CreateRecord, View.PublishedSnapshot, budget.Codec);
                if (!proof.IsAccepted) return Result(proof.RejectionCode, CandidateApplicationDiagnostic.From(proof, "Initialization"));
                var fresh = store.ReadRecovery(budget);
                if (!fresh.IsAccepted) return Result(fresh.Code, CandidateApplicationDiagnostic.From(fresh));
                if (!SameDescriptor(proof.Value.ObservedHeadDescriptor, fresh.Value.Current?.Descriptor, budget)) return Reject("StaleContext", "Initialization.Head");
                LocalPlayerProfileResult confirmed = null;
                var checkedHead = store.WithVerifiedRecovery(fresh.Value, capabilities, _ =>
                {
                    confirmed = locator.ConfirmCommit(expected, proof.Value, budget);
                    if (confirmed.IsAccepted) playerSession.AcceptProfile(confirmed.Observation.ActiveProfile);
                }, budget);
                if (!checkedHead.IsAccepted) return RestoreFailure(checkedHead.Code, CandidateApplicationDiagnostic.From(checkedHead));
                if (!confirmed.IsAccepted) return Result(confirmed.Code, PlayerSessionSystem.Diagnostic(confirmed));
                var restored = RestoreCore(budget);
                return restored.View.Phase == CandidateApplicationPhase.Ready
                    ? Result("Completed", committed: true, commit: proof.Value.OriginalInitializationCommitId) : restored;
            });
        }

        private static bool SameDescriptor(SnapshotDescriptor a, SnapshotDescriptor b, SaveStoreBudget budget)
        {
            if (a == null || b == null) return a == b;
            return a.Purpose == b.Purpose && a.PlayerId == b.PlayerId &&
                ExactRational.Create(a.SaveGeneration, 1, budget.Codec.Math)
                    .Compare(ExactRational.Create(b.SaveGeneration, 1, budget.Codec.Math), budget.Codec.Math) == 0 &&
                a.CommitId == b.CommitId && a.ParentCommitId == b.ParentCommitId &&
                a.TotalLength == b.TotalLength && a.Sha256.SequenceEqual(b.Sha256);
        }

        private CandidateApplicationCallResult ObservedGate(string commitId, SaveStoreBudget budget)
        {
            if (commitId == null) throw new ArgumentNullException(nameof(commitId));
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            if (pending != null) return Reject("Busy", "Application.Pending");
            if (store == null || observation == null) return Reject("ResolutionRequired", "Recovery.Observation");
            return null;
        }

        internal CandidateApplicationCallResult ResumeObserved(string commitId, SaveStoreBudget budget)
        {
            return Execute(() =>
            {
                var refusal = ObservedGate(commitId, budget);
                if (refusal != null) return refusal;
                var read = store.ReadUncommittedCandidate(observation, commitId, playerSession == null ? SavePurpose.CandidateValidation : SavePurpose.PlayerSave, budget);
                if (!read.IsAccepted)
                {
                    if (read.Code == "AlreadyCommitted") return ObservedCommitted(commitId, budget);
                    return RestoreFailure(read.Code, CandidateApplicationDiagnostic.From(read));
                }
                var candidate = read.Value;
                var resolution = CheckPlayerRoots(observation, budget.Codec);
                if (resolution != null) return RestoreFailure(resolution.Code, resolution);
                var decoded = DecodeApplication(candidate.Envelope, budget.Codec);
                if (!decoded.IsAccepted)
                    return RestoreFailure(decoded.RejectionCode, CandidateApplicationDiagnostic.From(decoded, "Candidate.Decode"));
                var snapshot = decoded.Value;
                var last = snapshot.Records.Last();
                if (playerSession?.CreationPending == true && !playerSession.IsOriginalCreation(last.Intent))
                    return Reject("InconsistentCreateIntent", "Candidate.Intent");
                var metadata = candidate.Metadata;
                if (last.Intent.PlayerId != View.PlayerId || last.CommitId != metadata.CommitId ||
                    last.Generation != metadata.SaveGeneration || candidate.OperationIds.Count != 1 ||
                    last.OperationId != candidate.OperationIds[0] || snapshot.Header.CommitId != metadata.CommitId ||
                    snapshot.Header.ParentCommitId != metadata.ParentCommitId ||
                    !SameDescriptor(snapshot.Descriptor, candidate.Descriptor, budget))
                    return RestoreFailure("InconsistentBinding",
                        new CandidateApplicationDiagnostic("InconsistentBinding", "Candidate.ApplicationIdentity"));
                var resumed = store.ResumeRecoveredCandidate(candidate, capabilities, playerSession == null ? SavePurpose.CandidateValidation : SavePurpose.PlayerSave, budget);
                if (!resumed.IsAccepted) return RestoreFailure(resumed.Code, CandidateApplicationDiagnostic.From(resumed));
                pending = new PendingApplication(last.Intent, null, candidate.ExpectedHead, resumed.Value);
                SetState(CandidateApplicationPhase.PendingPreparation);
                return WriteOriginal(budget);
            });
        }

        internal CandidateApplicationCallResult EndObserved(string commitId, SaveStoreBudget budget)
        {
            return Execute(() =>
            {
                if (playerSession?.CreationPending == true) return Reject("CreationPending", "CreateRecord");
                var refusal = ObservedGate(commitId, budget);
                if (refusal != null) return refusal;
                var ended = store.EndObservedCandidate(observation, commitId, playerSession == null ? SavePurpose.CandidateValidation : SavePurpose.PlayerSave, budget);
                if (ended.Code == "AlreadyCommitted") return ObservedCommitted(commitId, budget);
                var restored = RestoreCore(budget);
                if (ended.IsAccepted) return Result("Ended", restored.Diagnostic, commit: commitId);
                return Result(ended.Code, CandidateApplicationDiagnostic.From(ended), commit: commitId);
            });
        }

        private CandidateApplicationCallResult ObservedCommitted(string commitId, SaveStoreBudget budget)
        {
            var restored = RestoreCore(budget);
            return Result("ObservedAlreadyCommitted", restored.Diagnostic, true, commitId);
        }

        internal void Close()
        {
            if (Thread.CurrentThread.ManagedThreadId != ownerThread) throw new InvalidOperationException("WrongThread");
            if (active) throw new InvalidOperationException("Busy");
            if (disposed) return;
            var diagnostic = View.Diagnostic;
            try { store?.Dispose(); }
            catch (Exception error)
            {
                diagnostic = CandidateApplicationDiagnostic.From("CloseFailed", "Application.Lease", error);
                throw;
            }
            finally
            {
                disposed = true;
                publish = null;
                SetState(CandidateApplicationPhase.Disposed, diagnostic);
            }
        }
    }
}
