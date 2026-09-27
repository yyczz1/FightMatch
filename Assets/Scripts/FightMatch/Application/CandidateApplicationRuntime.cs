using System;
using System.Linq;
using System.Threading;
using FightMatch.Core;
using FightMatch.Platform;

namespace FightMatch.Application
{
    internal sealed partial class CandidateApplicationRuntime
    {
        private sealed class PendingApplication
        {
            internal readonly PreparedCandidateApplicationIntent Intent;
            internal readonly CandidateApplicationCandidate Candidate;
            internal readonly SnapshotDescriptor ExpectedHead;
            internal SaveCommitTicket Ticket;
            internal bool Committed;

            internal PendingApplication(PreparedCandidateApplicationIntent intent,
                CandidateApplicationCandidate candidate, SnapshotDescriptor expectedHead, SaveCommitTicket ticket = null)
            {
                Intent = intent;
                Candidate = candidate;
                ExpectedHead = expectedHead;
                Ticket = ticket;
            }
        }

        private readonly int ownerThread = Thread.CurrentThread.ManagedThreadId;
        private Action<CandidateApplicationPublished> publish;
        private LocalSaveStore store;
        private ILocalSaveStorage recoveryStorage;
        private PlayerSessionSystem playerSession;
        private SaveRecoveryCapabilities capabilities;
        private SaveRecoveryView observation;
        private PendingApplication pending;
        private bool active;
        private bool disposed;
        private CandidateApplicationDiagnostic notificationFailure;
        private volatile CandidateApplicationView view;

        internal CandidateApplicationRuntime(Action<CandidateApplicationPublished> publish)
        {
            this.publish = publish;
            view = new CandidateApplicationView(null, CandidateApplicationPhase.Unconfigured,
                null, false, null, null, Array.Empty<string>(), null);
        }

        internal CandidateApplicationView View => view;
        internal CandidateApplicationCallResult PlayerAdmission() { return Guard(true); }

        private CandidateApplicationCallResult Result(string code, CandidateApplicationDiagnostic diagnostic = null,
            bool committed = false, string commit = null, CandidateApplicationLookup lookup = null,
            string lookupViewCommit = null)
        {
            return new CandidateApplicationCallResult(code, committed, commit, lookup, lookupViewCommit,
                View, diagnostic, notificationFailure);
        }

        private CandidateApplicationCallResult Reject(string code, string path)
        {
            return Result(code, new CandidateApplicationDiagnostic(code, path));
        }

        private CandidateApplicationCallResult Guard(bool mutation)
        {
            if (Thread.CurrentThread.ManagedThreadId != ownerThread) return Reject("WrongThread", "Application.Thread");
            if (disposed) return Reject("Disposed", "Application");
            if (mutation && active) return Reject("Busy", "Application");
            return null;
        }

        private CandidateApplicationCallResult Execute(Func<CandidateApplicationCallResult> action)
        {
            var refusal = Guard(true);
            if (refusal != null) return refusal;
            active = true;
            notificationFailure = null;
            try { return action(); }
            catch (ExactMathLimitException error)
            {
                var diagnostic = CandidateApplicationDiagnostic.From("Limit", "Application", error);
                if (pending != null)
                {
                    SetState(pending.Committed ? CandidateApplicationPhase.RestoreRequired :
                        CandidateApplicationPhase.RecoveryBlocked, diagnostic);
                    return PendingResult("Limit", diagnostic);
                }
                return Result("Limit", diagnostic);
            }
            finally { active = false; }
        }

        private void SetState(CandidateApplicationPhase phase, CandidateApplicationDiagnostic diagnostic = null)
        {
            view = new CandidateApplicationView(View.PlayerId, phase, View.PublishedSnapshot, false,
                pending?.Intent.OperationId, pending?.Ticket?.Metadata.CommitId,
                View.ObservedCandidateCommitIds, diagnostic, View.Purpose);
        }

        private CandidateApplicationCallResult PendingResult(string code = null,
            CandidateApplicationDiagnostic diagnostic = null)
        {
            return Result(pending.Committed ? "CommittedRestoreRequired" : code ?? View.Phase.ToString(),
                diagnostic ?? View.Diagnostic, pending.Committed, pending.Ticket?.Metadata.CommitId);
        }

        private CandidateApplicationCallResult ValidateIntent(PreparedCandidateApplicationIntent intent, SaveStoreBudget budget)
        {
            if (intent == null) throw new ArgumentNullException(nameof(intent));
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            if (View.PlayerId != null && intent.PlayerId != View.PlayerId) return Reject("InconsistentBinding", "Intent.PlayerId");
            return null;
        }

        private CandidateApplicationCallResult LookupPublished(PreparedCandidateApplicationIntent intent, SaveStoreBudget budget)
        {
            var snapshot = View.PublishedSnapshot;
            if (snapshot == null) return null;
            var lookup = CandidateApplicationProtocol.Lookup(snapshot, intent, budget.Codec);
            if (!lookup.IsAccepted)
                return Result(lookup.RejectionCode, CandidateApplicationDiagnostic.From(lookup, "Lookup"));
            if (!lookup.Value.IsFound) return null;
            return Result("Completed", committed: true, commit: lookup.Value.OriginalCommitId,
                lookup: lookup.Value, lookupViewCommit: snapshot.Header.CommitId);
        }

        private bool SamePending(PreparedCandidateApplicationIntent intent)
        {
            return pending.Intent.CanonicalBytes.SequenceEqual(intent.CanonicalBytes);
        }

        internal CandidateApplicationCallResult QueryResumedIntent(
            PlayerSessionSystem owner, string commitId, string operationId,
            out PreparedCandidateApplicationIntent intent)
        {
            intent = null;
            var refusal = Guard(true);
            if (refusal != null) return refusal;
            if (owner == null || !ReferenceEquals(owner, playerSession) ||
                View.Purpose != SavePurpose.PlayerSave)
                return Reject("InconsistentBinding", "Navigation.Owner");
            if (playerSession.CreationPending)
                return Reject("CreationPending", "CreateRecord");
            if (pending?.Ticket == null || commitId == null || operationId == null ||
                pending.Ticket.Metadata.CommitId != commitId || pending.Intent.OperationId != operationId)
                return Reject("StaleContext", "Navigation.ResumedCandidate");
            intent = pending.Intent;
            return PendingResult();
        }

        internal CandidateApplicationCallResult QueryView()
        {
            return Guard(false) ?? Result(View.Phase.ToString(), View.Diagnostic);
        }

        internal CandidateApplicationCallResult QueryOperation(PreparedCandidateApplicationIntent intent, SaveStoreBudget budget)
        {
            var refusal = Guard(false);
            if (refusal != null) return refusal;
            refusal = ValidateIntent(intent, budget);
            if (refusal != null) return refusal;
            return ReadOperation(intent, budget);
        }

        private CandidateApplicationCallResult ReadOperation(PreparedCandidateApplicationIntent intent, SaveStoreBudget budget)
        {
            var original = LookupPublished(intent, budget);
            if (original != null) return original;
            if (pending != null && pending.Intent.OperationId == intent.OperationId)
                return SamePending(intent) ? PendingResult() : Reject("OperationConflict", "Intent");
            return IsReady() ? Result("NotFound") : Reject("ResolutionRequired", "Application.View");
        }

        private bool IsReady()
        {
            return View.IsPublishedHeadVerified && (View.Phase == CandidateApplicationPhase.Ready ||
                View.Phase == CandidateApplicationPhase.InitializationReady);
        }

        internal CandidateApplicationCallResult Submit(PreparedCandidateApplicationIntent intent,
            CandidateApplicationBuilder builder, SaveStoreBudget budget, bool trusted = false)
        {
            return Execute(() =>
            {
                var refusal = ValidateIntent(intent, budget);
                if (refusal != null) return refusal;
                if (playerSession == null ? !(intent.Context is PreparedCandidateContext) :
                    !trusted || !(intent.Context is PreparedPublishedRuleContext)) return Reject("UnsupportedBinding", "Intent.Context");
                var original = LookupPublished(intent, budget);
                if (original != null) return original;
                if (playerSession?.CreationPending == true && !playerSession.IsOriginalCreation(intent)) return Reject("CreationPending", "CreateRecord");
                if (pending != null)
                {
                    if (pending.Intent.OperationId != intent.OperationId) return Reject("Busy", "Application.Pending");
                    return SamePending(intent) ? PendingResult() : Reject("OperationConflict", "Intent");
                }
                if (!IsReady()) return Reject("ResolutionRequired", "Application.View");
                var basis = View.PublishedSnapshot;
                if (intent.ExpectedCommitId != basis?.Header.CommitId) return Reject("StaleContext", "Intent.ExpectedCommitId");
                var route = basis?.Continuation;
                if (route != null && (intent.Kind != CandidateApplicationKind.SettleVictory ||
                    intent.OperationId != route.ReservedOperationId))
                    return Reject("InvalidContinuation", "Continuation");
                if (builder == null) return Reject("BuilderUnavailable", "Builder");
                CandidateApplicationBuildResult built;
                try { built = builder(basis, intent, budget.Codec); }
                catch (ExactMathLimitException) { throw; }
                catch (Exception error)
                {
                    return Result("BuilderFailed", CandidateApplicationDiagnostic.From("BuilderFailed", "Builder", error));
                }
                if (built == null) return Reject("BuilderFailed", "Builder.Result");
                if (!built.IsAccepted) return Result("BuilderRejected", built.Diagnostic);
                var proposed = CandidateApplicationProtocol.Propose(basis, built.NextBusiness, intent,
                    built.CopyResult(), built.NextSettlementOperationId, budget.Codec);
                if (!proposed.IsAccepted)
                    return Result(proposed.RejectionCode, CandidateApplicationDiagnostic.From(proposed, "Propose"));
                pending = new PendingApplication(intent, proposed.Value, basis?.Descriptor);
                SetState(CandidateApplicationPhase.PendingPreparation);
                return PrepareAndWrite(budget);
            });
        }

        private CandidateApplicationCallResult PrepareAndWrite(SaveStoreBudget budget)
        {
            var prepared = store.Prepare(pending.ExpectedHead, new[] { pending.Intent.OperationId },
                metadata => EncodePending(metadata, budget.Codec), budget);
            if (!prepared.IsAccepted)
            {
                var diagnostic = CandidateApplicationDiagnostic.From(prepared);
                SetState(CandidateApplicationPhase.PendingPreparation, diagnostic);
                return PendingResult(prepared.Code, diagnostic);
            }
            pending.Ticket = prepared.Value;
            SetState(CandidateApplicationPhase.PendingPreparation);
            return WriteOriginal(budget);
        }

        private SaveCodecResult<SaveEnvelope> EncodePending(SaveCommitMetadata metadata, SaveCodecBudget budget)
        {
            var header = new CandidateBusinessSaveHeader(metadata.SaveGeneration, metadata.CommitId, metadata.ParentCommitId, metadata.CommitIndex);
            return playerSession == null ? CandidateApplicationSaveCodec.Encode(pending.Candidate, header, budget)
                : CandidateApplicationSaveCodec.EncodePublished(pending.Candidate, header, playerSession.Resolved, budget);
        }

        private CandidateApplicationCallResult WriteOriginal(SaveStoreBudget budget)
        {
            var read = store.ReadRecovery(budget);
            if (!read.IsAccepted) return PendingFailure(read);
            var resolution = CheckPlayerRoots(read.Value, budget.Codec);
            if (resolution != null) return RestoreFailure(resolution.Code, resolution);
            // Permission is used only in this synchronous application admission.
            // The callback does not call the store while its own gate is held.
            var verified = store.WithVerifiedRecovery(read.Value, capabilities, _ => { }, budget);
            if (!verified.IsAccepted) return PendingFailure(verified);
            var written = store.Write(pending.Ticket, budget);
            if (!written.IsAccepted) return PendingFailure(written);
            pending.Committed = true;
            SetState(CandidateApplicationPhase.RestoreRequired);
            return RestoreCore(budget);
        }

        private CandidateApplicationCallResult PendingFailure<T>(LocalSaveResult<T> result)
        {
            var diagnostic = CandidateApplicationDiagnostic.From(result);
            var phase = result.Code == "SaveFailed" ? CandidateApplicationPhase.SaveFailed :
                result.Code == "CommitUnknown" ? CandidateApplicationPhase.CommitUnknown : CandidateApplicationPhase.RecoveryBlocked;
            SetState(pending.Committed ? CandidateApplicationPhase.RestoreRequired : phase, diagnostic);
            return PendingResult(result.Code, diagnostic);
        }

        private CandidateApplicationCallResult MatchPending(PreparedCandidateApplicationIntent intent, SaveStoreBudget budget)
        {
            var refusal = ValidateIntent(intent, budget);
            if (refusal != null) return refusal;
            if (pending == null) return ReadOperation(intent, budget);
            if (pending.Intent.OperationId != intent.OperationId) return Reject("Busy", "Application.Pending");
            if (!SamePending(intent)) return Reject("OperationConflict", "Intent");
            return null;
        }

        internal CandidateApplicationCallResult Resolve(PreparedCandidateApplicationIntent intent, SaveStoreBudget budget)
        {
            return Execute(() =>
            {
                var refusal = MatchPending(intent, budget);
                if (refusal != null) return refusal;
                if (pending.Committed) return RestoreCore(budget);
                if (pending.Ticket == null) return PendingResult();
                return ResolveTicket(budget, false);
            });
        }

        internal CandidateApplicationCallResult Retry(PreparedCandidateApplicationIntent intent, SaveStoreBudget budget)
        {
            return Execute(() =>
            {
                var refusal = MatchPending(intent, budget);
                if (refusal != null) return refusal;
                if (pending.Committed) return RestoreCore(budget);
                return pending.Ticket == null ? PrepareAndWrite(budget) : ResolveTicket(budget, true);
            });
        }

        private CandidateApplicationCallResult ResolveTicket(SaveStoreBudget budget, bool retry)
        {
            var found = store.Lookup(pending.Ticket.Metadata.CommitId, pending.Intent.OperationId, budget);
            if (found.IsAccepted)
            {
                pending.Committed = true;
                SetState(CandidateApplicationPhase.RestoreRequired);
                return RestoreCore(budget);
            }
            if (found.Code != "ConfirmedNotCommitted") return PendingFailure(found);
            if (retry) return WriteOriginal(budget);
            var diagnostic = CandidateApplicationDiagnostic.From(found);
            SetState(CandidateApplicationPhase.SaveFailed, diagnostic);
            return PendingResult(found.Code, diagnostic);
        }

        internal CandidateApplicationCallResult End(PreparedCandidateApplicationIntent intent, SaveStoreBudget budget)
        {
            return Execute(() =>
            {
                if (playerSession?.CreationPending == true) return Reject("CreationPending", "CreateRecord");
                var refusal = MatchPending(intent, budget);
                if (refusal != null) return refusal;
                if (pending.Committed) return RestoreCore(budget);
                var commit = pending.Ticket?.Metadata.CommitId;
                if (pending.Ticket != null)
                {
                    var ended = store.EndUncommitted(pending.Ticket, budget);
                    if (!ended.IsAccepted)
                    {
                        if (ended.Code == "AlreadyCommitted") return ResolveTicket(budget, false);
                        return PendingFailure(ended);
                    }
                }
                pending = null;
                var restored = RestoreCore(budget);
                return Result("Ended", restored.Diagnostic, commit: commit);
            });
        }
    }
}
