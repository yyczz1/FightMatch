using System;
using System.Collections.Generic;
using FightMatch.Core;
using FightMatch.Platform;

namespace FightMatch.Application
{
    public enum CandidateApplicationPhase
    {
        Unconfigured, RestoreRequired, InitializationReady, Ready, PendingPreparation,
        SaveFailed, CommitUnknown, RecoveryBlocked, Disposed, CreationConfirmationRequired
    }

    public sealed class CandidateApplicationDiagnostic
    {
        public string Code { get; }
        public string FieldPath { get; }
        public string LimitReason { get; }
        public ulong? RequiredAtLeast { get; }
        public ulong? Allowed { get; }
        public string Stage { get; }
        public string ExceptionType { get; }
        public string ExceptionMessage { get; }

        internal CandidateApplicationDiagnostic(string code, string fieldPath, string limitReason = null,
            ulong? requiredAtLeast = null, ulong? allowed = null, string stage = null,
            string exceptionType = null, string exceptionMessage = null)
        {
            Code = code;
            FieldPath = fieldPath;
            LimitReason = limitReason;
            RequiredAtLeast = requiredAtLeast;
            Allowed = allowed;
            Stage = stage;
            ExceptionType = exceptionType;
            ExceptionMessage = exceptionMessage;
        }

        internal static CandidateApplicationDiagnostic From<T>(SaveCodecResult<T> result, string stage)
        {
            return new CandidateApplicationDiagnostic(result.RejectionCode, result.FieldPath,
                result.LimitReason, result.RequiredAtLeast, result.Allowed, stage);
        }

        internal static CandidateApplicationDiagnostic From<T>(LocalSaveResult<T> result)
        {
            return new CandidateApplicationDiagnostic(result.Diagnostic?.Code ?? result.Code, result.FieldPath,
                result.LimitReason, result.RequiredAtLeast, result.Allowed, result.Diagnostic?.Stage,
                result.Diagnostic?.ExceptionType, result.Diagnostic?.ExceptionMessage);
        }

        internal static CandidateApplicationDiagnostic From(string code, string stage, Exception error)
        {
            var limit = error as ExactMathLimitException;
            return new CandidateApplicationDiagnostic(code, limit == null ? stage : "Budget.Math",
                limit?.ReasonCode, limit == null ? (ulong?)null : (ulong)limit.RequiredAtLeast,
                limit == null ? (ulong?)null : (ulong)limit.Allowed, stage,
                error.GetType().FullName, error.Message);
        }
    }

    public sealed class CandidateApplicationView
    {
        public string PlayerId { get; }
        public CandidateApplicationPhase Phase { get; }
        public CandidateApplicationSnapshot PublishedSnapshot { get; }
        public SavePurpose Purpose { get; }
        public bool IsPublishedHeadVerified { get; }
        public string PendingOperationId { get; }
        public string PendingCommitId { get; }
        public IReadOnlyList<string> ObservedCandidateCommitIds { get; }
        public CandidateApplicationDiagnostic Diagnostic { get; }

        internal CandidateApplicationView(string playerId, CandidateApplicationPhase phase,
            CandidateApplicationSnapshot snapshot, bool verified, string pendingOperation, string pendingCommit,
            IEnumerable<string> observed, CandidateApplicationDiagnostic diagnostic, SavePurpose purpose = SavePurpose.CandidateValidation)
        {
            PlayerId = playerId;
            Phase = phase;
            PublishedSnapshot = snapshot;
            Purpose = snapshot?.Descriptor.Purpose ?? purpose;
            IsPublishedHeadVerified = verified;
            PendingOperationId = pendingOperation;
            PendingCommitId = pendingCommit;
            ObservedCandidateCommitIds = new List<string>(observed).AsReadOnly();
            Diagnostic = diagnostic;
        }
    }

    public sealed class CandidateApplicationCallResult
    {
        public string Code { get; }
        public bool IsCommitted { get; }
        public string OriginalCommitId { get; }
        public CandidateApplicationLookup OriginalLookup { get; }
        public string LookupViewCommitId { get; }
        public CandidateApplicationView View { get; }
        public CandidateApplicationDiagnostic Diagnostic { get; }
        public CandidateApplicationDiagnostic NotificationFailure { get; }

        internal CandidateApplicationCallResult(string code, bool committed, string commit,
            CandidateApplicationLookup lookup, string lookupViewCommit, CandidateApplicationView view,
            CandidateApplicationDiagnostic diagnostic, CandidateApplicationDiagnostic notificationFailure)
        {
            Code = code;
            IsCommitted = committed;
            OriginalCommitId = commit;
            OriginalLookup = lookup;
            LookupViewCommitId = lookupViewCommit;
            View = view;
            Diagnostic = diagnostic;
            NotificationFailure = notificationFailure;
        }
    }

    public delegate CandidateApplicationBuildResult CandidateApplicationBuilder(
        CandidateApplicationSnapshot basis, PreparedCandidateApplicationIntent intent, SaveCodecBudget budget);

    public sealed class CandidateApplicationBuildResult
    {
        private readonly CandidateApplicationResultInput result;
        public bool IsAccepted { get; }
        public CandidateBusinessSnapshot NextBusiness { get; }
        public bool CommitEligible => false;
        public string NextSettlementOperationId { get; }
        public CandidateApplicationDiagnostic Diagnostic { get; }

        private CandidateApplicationBuildResult(CandidateBusinessSnapshot business,
            CandidateApplicationResultInput input, string reserved, CandidateApplicationDiagnostic diagnostic)
        {
            IsAccepted = diagnostic == null;
            NextBusiness = business;
            result = input == null ? null : Copy(input);
            NextSettlementOperationId = reserved;
            Diagnostic = diagnostic;
        }

        public static CandidateApplicationBuildResult Success(CandidateBusinessSnapshot nextBusiness,
            CandidateApplicationResultInput resultInput, string nextSettlementOperationId = null)
        {
            if (nextBusiness == null) throw new ArgumentNullException(nameof(nextBusiness));
            if (resultInput == null) throw new ArgumentNullException(nameof(resultInput));
            return new CandidateApplicationBuildResult(nextBusiness, resultInput, nextSettlementOperationId, null);
        }

        public static CandidateApplicationBuildResult Rejected(string code, string fieldPath,
            string limitReason = null, ulong? requiredAtLeast = null, ulong? allowed = null)
        {
            if (code == null) throw new ArgumentNullException(nameof(code));
            if (fieldPath == null) throw new ArgumentNullException(nameof(fieldPath));
            return new CandidateApplicationBuildResult(null, null, null,
                new CandidateApplicationDiagnostic(code, fieldPath, limitReason, requiredAtLeast, allowed, "Builder"));
        }

        internal CandidateApplicationResultInput CopyResult() { return Copy(result); }

        private static CandidateApplicationResultInput Copy(CandidateApplicationResultInput input)
        {
            return new CandidateApplicationResultInput
            {
                ChallengeId = input.ChallengeId,
                AttemptId = input.AttemptId,
                EntryBaselineId = input.EntryBaselineId,
                HistoryAnchorId = input.HistoryAnchorId,
                EndReceiptId = input.EndReceiptId,
                SettlementId = input.SettlementId,
                NewAttemptId = input.NewAttemptId,
                RecoveryResult = input.RecoveryResult,
                RecoveryResults = input.RecoveryResults == null ? null : new List<CandidateCharacterResult>(input.RecoveryResults).AsReadOnly()
            };
        }
    }

    public sealed class CandidateApplicationPublished
    {
        public CandidateApplicationView View { get; }
        public SnapshotDescriptor Descriptor => View.PublishedSnapshot.Descriptor;

        internal CandidateApplicationPublished(CandidateApplicationView view) { View = view; }
    }
}
