using System.Collections.Generic;
using System.Numerics;

namespace FightMatch.Core
{
    public sealed class CandidateBattleRun
    {
        public CandidateRandomBinding Binding { get; }
        public BattleEntryBaseline Baseline { get; }
        public BattleSnapshot InitialSnapshot { get; }
        public BattleSnapshot CurrentSnapshot { get; }
        public IReadOnlyList<CandidateBattleOperationRecord> Records { get; }
        public CandidateFinalAttemptReport FinalReport { get; }
        public bool CommitEligible => false;

        // Recovery must supply the complete effective prefix, never just a replacement snapshot.
        internal CandidateBattleRun(CandidateRandomBinding binding, BattleEntryBaseline baseline,
            BattleSnapshot initial, BattleSnapshot current, IEnumerable<CandidateBattleOperationRecord> records,
            CandidateFinalAttemptReport report)
        {
            Binding = binding; Baseline = baseline; InitialSnapshot = initial; CurrentSnapshot = current;
            Records = new List<CandidateBattleOperationRecord>(records).AsReadOnly(); FinalReport = report;
        }
    }

    public sealed class CandidateBattleConditions
    {
        public BigInteger? PreferenceRevision { get; set; }
        public bool? ItemUseEnabled { get; set; }
    }

    public sealed class CandidateBattleConditionValues
    {
        public BigInteger PreferenceRevision { get; }
        public bool ItemUseEnabled { get; }
        internal CandidateBattleConditionValues(BigInteger revision, bool enabled)
        { PreferenceRevision = revision; ItemUseEnabled = enabled; }
    }

    public enum CandidateBattleRejectionStage { Run, DirectAttack, EnemyPhase, Stage }

    public sealed class CandidateBattleResult
    {
        public bool IsAccepted => Run != null || NextRun != null;
        public CandidateBattleRun Run { get; }
        public CandidateBattleRun NextRun { get; }
        public CandidateBattleOperationRecord Record { get; }
        public CandidateBattleRejectionStage? RejectionStage { get; }
        // Original segment codes are preserved verbatim, including geometry-specific rejections.
        public string RejectionCode { get; }
        public string FieldPath { get; }
        public string RouteReasonCode { get; }
        public int? RouteCellIndex { get; }
        internal CandidateBattleResult(CandidateBattleRun run, CandidateBattleOperationRecord record, bool created = false)
        { if (created) Run = run; else NextRun = run; Record = record; }
        internal CandidateBattleResult(string code, string path, CandidateBattleRejectionStage stage = CandidateBattleRejectionStage.Run,
            string routeReason = null, int? routeIndex = null)
        { RejectionCode = code; FieldPath = path; RejectionStage = stage; RouteReasonCode = routeReason; RouteCellIndex = routeIndex; }
    }
}
