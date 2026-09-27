using System;
using System.Collections.Generic;
using System.Numerics;

namespace FightMatch.Core
{
    // These values contain only the immutable candidate graph; there is no live model or commit handle.
    public sealed class CandidateReplayContextKey
    {
        public CandidateIsolationPurpose Purpose { get; }
        public string PlayerId { get; }
        public string AttemptId { get; }
        public BigInteger SourceSceneRevision { get; }
        public CandidateBattleRun StartingRun { get; }
        public PreparedRuleContext Context => StartingRun.Baseline.Entry.Context;
        public CandidateRandomBinding Binding => StartingRun.Binding;
        public BattleEntryBaseline Baseline => StartingRun.Baseline;
        public BattleSnapshot StartingSnapshot => StartingRun.CurrentSnapshot;
        public CandidateBattleConditionValues Conditions { get; }
        public CandidateIsolationRights RightsMode { get; }
        public string RecordedOperationId { get; }
        public bool RecordedHasFinalReport { get; }
        public IReadOnlyList<string> UsedOperationIds { get; }
        public bool CommitEligible => false;

        internal CandidateReplayContextKey(CandidateIsolationPurpose purpose, string player, string attempt,
            BigInteger revision, CandidateBattleRun run, CandidateBattleConditionValues conditions,
            CandidateIsolationRights rights, string recordedOperation, IEnumerable<string> used, bool recordedHasFinalReport = false)
        {
            Purpose = purpose; PlayerId = player; AttemptId = attempt; SourceSceneRevision = revision;
            StartingRun = run; Conditions = conditions; RightsMode = rights; RecordedOperationId = recordedOperation;
            RecordedHasFinalReport = recordedHasFinalReport;
            UsedOperationIds = new List<string>(used).AsReadOnly();
        }

        // The caller's budget also covers the complete definition/baseline and history comparison.
        public bool HasSameValue(CandidateReplayContextKey other, ExactMathBudget budget)
        {
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            if (other == null) return false;
            budget.CheckInteger(SourceSceneRevision); budget.CheckInteger(other.SourceSceneRevision);
            return Purpose == other.Purpose && StringComparer.Ordinal.Equals(PlayerId, other.PlayerId) &&
                StringComparer.Ordinal.Equals(AttemptId, other.AttemptId) && SourceSceneRevision == other.SourceSceneRevision &&
                RightsMode == other.RightsMode && RecordedHasFinalReport == other.RecordedHasFinalReport && StringComparer.Ordinal.Equals(RecordedOperationId, other.RecordedOperationId) &&
                CandidateBattleReportFingerprint.Equal(Binding, other.Binding, budget) &&
                CandidateBattleReportFingerprint.Equal(Baseline, other.Baseline, budget) &&
                CandidateBattleReportFingerprint.Equal(StartingRun.InitialSnapshot, other.StartingRun.InitialSnapshot, budget) &&
                CandidateBattleReportFingerprint.Equal(StartingSnapshot, other.StartingSnapshot, budget) &&
                CandidateReplayComparison.SameRecords(StartingRun.Records, other.StartingRun.Records, budget) &&
                CandidateReplayComparison.SameReport(StartingRun.FinalReport, other.StartingRun.FinalReport, budget) &&
                CandidateBattleReportFingerprint.Equal(Conditions, other.Conditions, budget) &&
                CandidateBattleReportFingerprint.Equal(UsedOperationIds, other.UsedOperationIds, budget);
        }
    }

    public sealed class CandidateBattleIsolation
    {
        public CandidateReplayContextKey ContextKey { get; }
        public CandidateBattleRun Run => ContextKey.StartingRun;
        public CandidateBattleConditionValues Conditions => ContextKey.Conditions;
        public CandidateBattleOperationRecord RecordedOperation { get; }
        public CandidateFinalAttemptReport RecordedReport { get; }
        public bool CommitEligible => false;

        // Internal evidence construction permits tests to disprove a complete recorded claim.
        // It grants no bypass around ReplayRecorded's structure/binding checks or real 012 evaluation.
        internal CandidateBattleIsolation(CandidateReplayContextKey key, CandidateBattleOperationRecord record = null,
            CandidateFinalAttemptReport report = null)
        { ContextKey = key; RecordedOperation = record; RecordedReport = report; }
    }

    public sealed class CandidateReplayPlanEvidence
    {
        public CandidateReplayContextKey ContextKey { get; }
        public IReadOnlyList<CandidateBattleOperationRecord> Steps { get; }
        public bool CommitEligible => false;
        internal CandidateReplayPlanEvidence(CandidateReplayContextKey key, IEnumerable<CandidateBattleOperationRecord> steps)
        { ContextKey = key; Steps = new List<CandidateBattleOperationRecord>(steps).AsReadOnly(); }

        public bool HasSameValue(CandidateReplayPlanEvidence other, ExactMathBudget budget)
        {
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            return other != null && ContextKey.HasSameValue(other.ContextKey, budget) &&
                CandidateReplayComparison.SameRecords(Steps, other.Steps, budget);
        }
    }
}
