using System.Collections.Generic;
using System.Numerics;
using FlowPuzzle.Core;

namespace FightMatch.Core
{
    public enum CandidateIsolationPurpose { CurrentPlan, RecordedOperation }
    public enum CandidateIsolationRights { Empty }
    public enum CandidateReplayOutcome { IsolationBuilt, Matched, Diverged, PlanEvaluated }

    // Mutable request shells must remain stable during a call; none are retained by results.
    public sealed class CandidateIsolationRequest
    {
        public string PlayerId { get; set; }
        public string AttemptId { get; set; }
        public BigInteger? ExpectedSceneRevision { get; set; }
        public CandidateIsolationPurpose? Purpose { get; set; }
        public CandidateIsolationRights? RightsMode { get; set; }
        public CandidateBattleConditions CurrentConditions { get; set; }
        public string RecordedOperationId { get; set; }
    }

    public sealed class CandidateReplayStep
    {
        public CandidateBattleOperationKind? Kind { get; set; }
        public string OperationId { get; set; }
        public BigInteger? OccurredAtUnixMilliseconds { get; set; }
        public BattleCombatantKey Actor { get; set; }
        public BattlePairKey Pair { get; set; }
        public List<FlowPos> Route { get; set; }
    }

    public sealed class CandidateReplayDivergence
    {
        public int StepIndex { get; }
        public string OperationId { get; }
        public string FieldPath { get; }
        // Only immutable scalar/enum/exact-number values are emitted, never whole business objects.
        public object Expected { get; }
        public object Actual { get; }
        internal CandidateReplayDivergence(int step, string operation, string path, object expected, object actual)
        { StepIndex = step; OperationId = operation; FieldPath = path; Expected = expected; Actual = actual; }
    }

    public sealed class CandidateReplayResult
    {
        public bool IsAccepted => Outcome.HasValue;
        public CandidateReplayOutcome? Outcome { get; }
        public CandidateBattleIsolation Input { get; }
        public CandidateBattleRun Run { get; }
        public IReadOnlyList<CandidateBattleOperationRecord> ActualRecords { get; }
        public CandidateFinalAttemptReport FinalReport => Run?.FinalReport;
        public bool NormalVictory => FinalReport?.Outcome == CandidateBattleOutcome.NormalVictory;
        public bool? Matched => Outcome == CandidateReplayOutcome.Matched ? true :
            Outcome == CandidateReplayOutcome.Diverged ? (bool?)false : null;
        public CandidateReplayDivergence FirstDivergence { get; }
        public CandidateReplayPlanEvidence PlanEvidence { get; }
        public string RejectionCode { get; }
        public string FieldPath { get; }
        public int? StepIndex { get; }
        public CandidateBattleRejectionStage? RejectionStage { get; }
        public string RouteReasonCode { get; }
        public int? RouteCellIndex { get; }
        public bool CommitEligible => false;
        internal CandidateReplayResult(CandidateBattleIsolation input)
        { Outcome = CandidateReplayOutcome.IsolationBuilt; Input = input; }
        internal CandidateReplayResult(CandidateReplayOutcome outcome, CandidateBattleRun run,
            IEnumerable<CandidateBattleOperationRecord> records, CandidateReplayDivergence divergence = null,
            CandidateReplayPlanEvidence evidence = null)
        {
            Outcome = outcome; Run = run; ActualRecords = new List<CandidateBattleOperationRecord>(records).AsReadOnly();
            FirstDivergence = divergence; PlanEvidence = evidence;
        }
        internal CandidateReplayResult(string code, string path, int? step = null,
            CandidateBattleRejectionStage? stage = null, string routeReason = null, int? routeIndex = null)
        { RejectionCode = code; FieldPath = path; StepIndex = step; RejectionStage = stage; RouteReasonCode = routeReason; RouteCellIndex = routeIndex; }
    }
}
