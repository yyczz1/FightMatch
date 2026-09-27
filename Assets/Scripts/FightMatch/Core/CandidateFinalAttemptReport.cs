using System.Collections.Generic;
using System.Numerics;

namespace FightMatch.Core
{
    public enum CandidateBattleOutcome { NormalVictory }

    public sealed class CandidateFinalAttemptReport
    {
        public CandidateRandomBinding Binding { get; }
        public BattleEntryBaseline Baseline { get; }
        public BattleSnapshot InitialSnapshot { get; }
        public IReadOnlyList<CandidateBattleOperationRecord> Operations { get; }
        public BattleSnapshot FinalSnapshot { get; }
        public IReadOnlyList<CandidateContributionSegment> Contributions { get; }
        public CandidateBattleOutcome Outcome { get; }
        public BigInteger EndedAtUnixMilliseconds { get; }
        public string TerminalOperationId { get; }
        public CandidateConsumptionCoverage ConsumptionCoverage { get; }
        public ExactRational WholeLevelInitialEnemyHp { get; }
        public bool CommitEligible => false;
        public string Fingerprint { get; }
        public string PlayerId => Baseline.Entry.PlayerId;
        public string AttemptId => Baseline.Entry.AttemptId;
        public string ChallengeId => Baseline.Entry.ChallengeId;
        public string EntryBaselineId => Baseline.Entry.EntryBaselineId;
        public PreparedLevel Level => Baseline.Entry.Level;

        internal CandidateFinalAttemptReport(CandidateRandomBinding binding, BattleEntryBaseline baseline,
            BattleSnapshot initial, IEnumerable<CandidateBattleOperationRecord> operations, BattleSnapshot final,
            IEnumerable<CandidateContributionSegment> contributions, CandidateBattleOutcome outcome, BigInteger endedAt,
            string terminalOperation, CandidateConsumptionCoverage coverage, ExactRational wholeHp, string fingerprint)
        {
            Binding = binding; Baseline = baseline; InitialSnapshot = initial;
            Operations = new List<CandidateBattleOperationRecord>(operations).AsReadOnly(); FinalSnapshot = final;
            Contributions = new List<CandidateContributionSegment>(contributions).AsReadOnly(); Outcome = outcome;
            EndedAtUnixMilliseconds = endedAt; TerminalOperationId = terminalOperation; ConsumptionCoverage = coverage;
            WholeLevelInitialEnemyHp = wholeHp; Fingerprint = fingerprint;
        }
    }
}
