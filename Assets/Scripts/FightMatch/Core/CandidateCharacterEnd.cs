using System;
using System.Collections.Generic;
using System.Numerics;
using static FightMatch.Core.CandidateGrowthRejectionCode;

namespace FightMatch.Core
{
    public enum CandidateCharacterEndKind { Unspecified, NormalVictory, NormalExit, ImmediateRestart }

    // A projection of future closed report facts, not proof that a report has been closed.
    public sealed class CandidateCharacterEndFacts
    {
        private string recoveryId;
        private CandidateTimeSample timeSample;
        public string PlayerId { get; set; }
        public string CharacterId { get; set; }
        public string AttemptId { get; set; }
        public string EntryBaselineId { get; set; }
        public string EndReceiptId { get; set; }
        public RuleContext Context { get; set; }
        public CandidateCharacterEndKind Kind { get; set; }
        public bool? WasParticipant { get; set; }
        public bool? WasDown { get; set; }
        public string RecoveryId { get => recoveryId; set { recoveryId = value; HasRecoveryId = true; } }
        public CandidateTimeSample TimeSample { get => timeSample; set { timeSample = value; HasTimeSample = true; } }
        internal bool HasRecoveryId { get; private set; }
        internal bool HasTimeSample { get; private set; }
    }

    public sealed class CandidateCharacterEndReceipt
    {
        public string PlayerId { get; }
        public string CharacterId { get; }
        public string AttemptId { get; }
        public string EntryBaselineId { get; }
        public string EndReceiptId { get; }
        public PreparedRuleContext Context { get; }
        public CandidateCharacterEndKind Kind { get; }
        public bool WasParticipant { get; }
        public bool WasDown { get; }
        public string RecoveryId { get; }
        public PreparedCandidateTimeSample TimeSample { get; }
        internal CandidateCharacterEndReceipt(CandidateCharacterEndFacts input, PreparedRuleContext context,
            PreparedCandidateTimeSample time)
        {
            PlayerId = input.PlayerId; CharacterId = input.CharacterId; AttemptId = input.AttemptId;
            EntryBaselineId = input.EntryBaselineId; EndReceiptId = input.EndReceiptId; Context = context;
            Kind = input.Kind; WasParticipant = input.WasParticipant.Value; WasDown = input.WasDown.Value;
            RecoveryId = input.RecoveryId; TimeSample = time;
        }
    }

    public static class CandidateCharacterEnd
    {
        public static CandidateCharacterResult Propose(CandidateCharacterState state, CandidateCharacterEndFacts endFacts,
            BigInteger expectedRevision, ExactMathBudget budget)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (endFacts == null) throw new ArgumentNullException(nameof(endFacts));
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            var c = new GrowthChecks(budget); c.CheckState(state);
            var f = endFacts;
            if (!c.Target(state, f.PlayerId, f.CharacterId, f.Context) || !c.Text(f.AttemptId, "AttemptId") ||
                !c.Text(f.EntryBaselineId, "EntryBaselineId") || !c.Text(f.EndReceiptId, "EndReceiptId")) return new CandidateCharacterResult(c);
            if (f.Kind == CandidateCharacterEndKind.Unspecified) return Reject(c, MissingField, "Kind");
            if (f.Kind != CandidateCharacterEndKind.NormalVictory && f.Kind != CandidateCharacterEndKind.NormalExit &&
                f.Kind != CandidateCharacterEndKind.ImmediateRestart) return Reject(c, UnsupportedBinding, "Kind");
            if (!f.WasParticipant.HasValue) return Reject(c, MissingField, "WasParticipant");
            if (!f.WasDown.HasValue) return Reject(c, MissingField, "WasDown");
            if (!f.WasParticipant.Value && f.WasDown.Value) return Reject(c, InconsistentBinding, "WasDown");
            if (!f.HasRecoveryId) return Reject(c, MissingField, "RecoveryId");
            if (!f.HasTimeSample) return Reject(c, MissingField, "TimeSample");
            PreparedCandidateTimeSample time = null;
            if (f.TimeSample != null && !CandidateRecoveryClock.PrepareTime(f.TimeSample, c, out time)) return new CandidateCharacterResult(c);
            foreach (var prior in state.ProcessedEnds)
            {
                if (!string.Equals(prior.AttemptId, f.AttemptId, StringComparison.Ordinal) &&
                    !string.Equals(prior.EndReceiptId, f.EndReceiptId, StringComparison.Ordinal)) continue;
                if (!c.Same(prior.AttemptId, f.AttemptId, "AttemptId") ||
                    !c.Same(prior.EndReceiptId, f.EndReceiptId, "EndReceiptId") ||
                    !c.Same(prior.EntryBaselineId, f.EntryBaselineId, "EntryBaselineId")) return new CandidateCharacterResult(c);
                if (prior.Kind != f.Kind) return Reject(c, InconsistentBinding, "Kind");
                if (prior.WasParticipant != f.WasParticipant.Value) return Reject(c, InconsistentBinding, "WasParticipant");
                if (prior.WasDown != f.WasDown.Value) return Reject(c, InconsistentBinding, "WasDown");
                if (!c.Same(prior.RecoveryId, f.RecoveryId, "RecoveryId")) return new CandidateCharacterResult(c);
                if (!CandidateRecoveryClock.SameTime(prior.TimeSample, time, c)) return Reject(c, InconsistentBinding, "TimeSample");
                return new CandidateCharacterResult(state, CandidateGrowthOutcome.AlreadyIncluded, end: prior);
            }
            if (!c.Revision(state, expectedRevision)) return new CandidateCharacterResult(c);
            if (f.WasParticipant.Value && !state.IsReady) return Reject(c, InconsistentBinding, "WasParticipant");
            var startRecovery = f.Kind != CandidateCharacterEndKind.ImmediateRestart && f.WasParticipant.Value && f.WasDown.Value;
            if (startRecovery)
            {
                if (!c.Text(f.RecoveryId, "RecoveryId")) return new CandidateCharacterResult(c);
                if (time == null) return Reject(c, MissingField, "TimeSample");
                foreach (var prior in state.RecoveryPeriods)
                    if (string.Equals(prior.RecoveryId, f.RecoveryId, StringComparison.Ordinal)) return Reject(c, InconsistentBinding, "RecoveryId");
            }
            else
            {
                if (f.RecoveryId != null) return Reject(c, InvalidValue, "RecoveryId");
                if (time != null) return Reject(c, InvalidValue, "TimeSample");
            }
            var revision = budget.Add(state.StateRevision, BigInteger.One);
            var receipt = new CandidateCharacterEndReceipt(f, state.Definition.Context, time);
            var ends = new List<CandidateCharacterEndReceipt>(state.ProcessedEnds) { receipt };
            var periods = new List<CandidateRecoveryPeriod>(state.RecoveryPeriods);
            CandidateRecoveryPeriod period = null;
            if (startRecovery)
            {
                period = new CandidateRecoveryPeriod(receipt, state.Definition, time,
                    ExactRational.Create(BigInteger.Zero, BigInteger.One, budget), time.Anomaly, budget);
                periods.Add(period);
            }
            var next = new CandidateCharacterState(state.Definition, state.PlayerId, state.CharacterId, state.Level,
                state.Experience, state.OriginalSlot, revision, state.BaseRewards, ends, periods);
            return new CandidateCharacterResult(next, CandidateGrowthOutcome.Applied, end: receipt, period: period);
        }

        private static CandidateCharacterResult Reject(GrowthChecks check, CandidateGrowthRejectionCode code, string path)
        { check.Fail(code, path); return new CandidateCharacterResult(check); }
    }
}
