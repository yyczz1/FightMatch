using System.Collections.Generic;
using System.Numerics;
using FightMatch.Core;
using FlowPuzzle.Core;

namespace FightMatch.Application
{
    public sealed class CandidateBattleDraft
    {
        public string PlayerId { get; set; }
        public string ExpectedCommitId { get; set; }
        public RuleContext Context { get; set; }
        public CandidateApplicationKind? Kind { get; set; }
        public CandidateApplicationAttackInput Attack { get; set; }
        public CandidateApplicationLinkInput Link { get; set; }
        public CandidateApplicationRollbackInput Rollback { get; set; }
    }

    public sealed class PreparedCandidateBattleRequest
    {
        public PreparedCandidateApplicationIntent Intent { get; }
        public CandidateApplicationKind Kind => Intent.Kind;
        public string OperationId => Intent.OperationId;
        internal string AttemptId { get; }
        internal BigInteger SceneRevision { get; }
        internal BattleCombatantKey Actor { get; }
        internal BattlePairKey Pair { get; }
        internal IReadOnlyList<FlowPos> Route { get; }
        internal BigInteger? PreferenceRevision { get; }
        internal bool? ItemUseEnabled { get; }
        internal string HistoryAnchorId { get; }
        internal string TargetOperationId { get; }
        internal IReadOnlyList<string> ConfirmedRemovedOperationIds { get; }

        // input is the factory's private, budget-checked copy, also used by PrepareIntent.
        internal PreparedCandidateBattleRequest(PreparedCandidateApplicationIntent intent, CandidateApplicationIntentInput input)
        {
            Intent = intent;
            AttemptId = input.Attack?.AttemptId ?? input.Link?.AttemptId ?? input.Rollback.AttemptId;
            SceneRevision = (input.Attack?.ExpectedSceneRevision ?? input.Link?.ExpectedSceneRevision ??
                input.Rollback.ExpectedSceneRevision).Value;
            Actor = input.Attack?.Actor;
            Pair = input.Attack?.Pair ?? input.Link?.Pair;
            Route = input.Attack?.Route ?? input.Link?.Route;
            PreferenceRevision = input.Attack?.ExpectedPreferenceRevision;
            ItemUseEnabled = input.Attack?.ItemUseEnabled;
            HistoryAnchorId = input.Rollback?.HistoryAnchorId;
            TargetOperationId = input.Rollback?.TargetOperationId;
            ConfirmedRemovedOperationIds = input.Rollback?.ConfirmedRemovedOperationIds;
        }
    }

    public sealed class CandidateBattlePrepareResult
    {
        public bool IsAccepted => Request != null;
        public PreparedCandidateBattleRequest Request { get; }
        public string Code { get; }
        public CandidateApplicationDiagnostic Diagnostic { get; }
        internal CandidateBattlePrepareResult(PreparedCandidateBattleRequest request, CandidateApplicationDiagnostic diagnostic = null)
        { Request = request; Diagnostic = diagnostic; Code = diagnostic?.Code ?? "Prepared"; }
    }

    public sealed class CandidateBattleDomainRejection
    {
        public string Code { get; }
        public string FieldPath { get; }
        public CandidateBattleRejectionStage? RejectionStage { get; }
        public string RouteReasonCode { get; }
        public int? RouteCellIndex { get; }
        public string LimitReason { get; }
        public ulong? RequiredAtLeast { get; }
        public ulong? Allowed { get; }
        internal CandidateBattleDomainRejection(string code, string path, CandidateBattleRejectionStage? stage = null,
            string routeReason = null, int? routeIndex = null, string limitReason = null,
            ulong? required = null, ulong? allowed = null)
        {
            Code = code; FieldPath = path; RejectionStage = stage; RouteReasonCode = routeReason;
            RouteCellIndex = routeIndex; LimitReason = limitReason; RequiredAtLeast = required; Allowed = allowed;
        }
    }

    public sealed class CandidateBattleCallResult
    {
        public string Code { get; }
        public CandidateApplicationCallResult Application { get; }
        public CandidateDemoView View { get; }
        public CandidateBattleDomainRejection DomainRejection { get; }
        public CandidatePresentationDisposition PresentationDisposition { get; }
        public CandidateBattlePresentation Presentation { get; }
        internal CandidateBattleCallResult(string code, CandidateApplicationCallResult application, CandidateDemoView view,
            CandidateBattleDomainRejection rejection = null, CandidatePresentationDisposition disposition = CandidatePresentationDisposition.None,
            CandidateBattlePresentation presentation = null)
        {
            Code = code; Application = application; View = view; DomainRejection = rejection;
            PresentationDisposition = disposition; Presentation = presentation;
        }
    }

    public sealed class CandidateBattlePreviewResult
    {
        public bool IsAccepted => Range != null;
        public string Code { get; }
        public CandidateDemoView View { get; }
        public CandidateRollbackRange Range { get; }
        public CandidateApplicationDiagnostic Diagnostic { get; }
        internal CandidateBattlePreviewResult(CandidateDemoView view, CandidateRollbackRange range,
            CandidateApplicationDiagnostic diagnostic = null)
        { View = view; Range = range; Diagnostic = diagnostic; Code = diagnostic?.Code ?? "RangeFound"; }
    }
}
