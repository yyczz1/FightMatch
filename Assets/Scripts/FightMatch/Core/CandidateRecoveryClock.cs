using System;
using System.Collections.Generic;
using System.Numerics;
using static FightMatch.Core.CandidateGrowthRejectionCode;

namespace FightMatch.Core
{
    public enum CandidateTimeTrust { Unspecified, DeviceUntrusted, ServerTrusted }
    [Flags]
    public enum CandidateTimeAnomaly { None = 0, ClockBackward = 1, DomainChanged = 2 }

    public sealed class CandidateTimeSample
    {
        private ExactRational monotonic;
        private string scope;
        public BigInteger? WallUtcMilliseconds { get; set; }
        public BigInteger? ObservedAtUtcMilliseconds { get; set; }
        public ExactRational MonotonicElapsedMilliseconds { get => monotonic; set { monotonic = value; HasMonotonic = true; } }
        public string MonotonicScopeId { get => scope; set { scope = value; HasScope = true; } }
        public string Source { get; set; }
        public CandidateTimeTrust Trust { get; set; }
        public CandidateTimeAnomaly? Anomaly { get; set; }
        public bool HasMonotonic { get; private set; }
        public bool HasScope { get; private set; }
    }

    public sealed class PreparedCandidateTimeSample
    {
        public BigInteger WallUtcMilliseconds { get; }
        public BigInteger ObservedAtUtcMilliseconds { get; }
        public ExactRational MonotonicElapsedMilliseconds { get; }
        public string MonotonicScopeId { get; }
        public string Source { get; }
        public CandidateTimeTrust Trust { get; }
        public CandidateTimeAnomaly Anomaly { get; }
        internal PreparedCandidateTimeSample(CandidateTimeSample input)
        {
            WallUtcMilliseconds = input.WallUtcMilliseconds.Value; ObservedAtUtcMilliseconds = input.ObservedAtUtcMilliseconds.Value;
            MonotonicElapsedMilliseconds = input.MonotonicElapsedMilliseconds; MonotonicScopeId = input.MonotonicScopeId;
            Source = input.Source; Trust = input.Trust; Anomaly = input.Anomaly.Value;
        }
    }

    public sealed class CandidateRecoveryPeriod
    {
        public string RecoveryId => EndReceipt.RecoveryId;
        public string CharacterId => EndReceipt.CharacterId;
        public CandidateCharacterEndReceipt EndReceipt { get; }
        public CandidateGrowthDefinition Definition { get; }
        public ExactRational Duration => Definition.RecoveryDurationMilliseconds;
        public PreparedCandidateTimeSample StartSample => EndReceipt.TimeSample;
        public PreparedCandidateTimeSample LastAcceptedSample { get; }
        public ExactRational Elapsed { get; }
        public bool IsCompleted { get; }
        public CandidateTimeAnomaly Anomaly { get; }
        internal CandidateRecoveryPeriod(CandidateCharacterEndReceipt end, CandidateGrowthDefinition definition,
            PreparedCandidateTimeSample last, ExactRational elapsed, CandidateTimeAnomaly anomaly, ExactMathBudget budget)
        {
            EndReceipt = end; Definition = definition; LastAcceptedSample = last; Elapsed = elapsed; Anomaly = anomaly;
            IsCompleted = elapsed.Compare(Duration, budget) == 0;
        }
    }

    public static class CandidateRecoveryClock
    {
        public static CandidateCharacterResult Advance(CandidateCharacterState state, string recoveryId, CandidateTimeSample sample,
            BigInteger expectedRevision, ExactMathBudget budget)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (sample == null) throw new ArgumentNullException(nameof(sample));
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            var c = new GrowthChecks(budget); c.CheckState(state);
            if (!c.Text(recoveryId, "RecoveryId")) return new CandidateCharacterResult(c);
            CandidateRecoveryPeriod period = null;
            var index = -1;
            for (var i = 0; i < state.RecoveryPeriods.Count; i++)
                if (string.Equals(state.RecoveryPeriods[i].RecoveryId, recoveryId, StringComparison.Ordinal))
                { period = state.RecoveryPeriods[i]; index = i; break; }
            if (period == null) { c.Fail(InconsistentBinding, "RecoveryId"); return new CandidateCharacterResult(c); }
            if (period.IsCompleted) return new CandidateCharacterResult(state, CandidateGrowthOutcome.AlreadyIncluded, period: period);
            if (!PrepareTime(sample, c, out var time) || !c.Revision(state, expectedRevision)) return new CandidateCharacterResult(c);
            if (SameTime(period.LastAcceptedSample, time, c))
                return new CandidateCharacterResult(state, CandidateGrowthOutcome.Unchanged, period: period, anomaly: period.Anomaly);
            var last = period.LastAcceptedSample;
            var sameDomain = last.MonotonicScopeId != null && string.Equals(last.MonotonicScopeId, time.MonotonicScopeId, StringComparison.Ordinal);
            var delta = sameDomain ? time.MonotonicElapsedMilliseconds.Subtract(last.MonotonicElapsedMilliseconds, budget)
                : ExactRational.Create(budget.Subtract(time.WallUtcMilliseconds, last.WallUtcMilliseconds), BigInteger.One, budget);
            var anomaly = period.Anomaly | time.Anomaly | (sameDomain ? CandidateTimeAnomaly.None : CandidateTimeAnomaly.DomainChanged);
            if (budget.Compare(delta.Numerator, BigInteger.Zero) < 0)
                return new CandidateCharacterResult(state, CandidateGrowthOutcome.IgnoredTimeRegression,
                    period: period, anomaly: anomaly | CandidateTimeAnomaly.ClockBackward);
            var elapsed = period.Elapsed.Add(delta, budget);
            if (elapsed.Compare(period.Duration, budget) >= 0) elapsed = period.Duration;
            var nextPeriod = new CandidateRecoveryPeriod(period.EndReceipt, period.Definition, time, elapsed, anomaly, budget);
            var periods = new List<CandidateRecoveryPeriod>(state.RecoveryPeriods); periods[index] = nextPeriod;
            var revision = budget.Add(state.StateRevision, BigInteger.One);
            var next = new CandidateCharacterState(state.Definition, state.PlayerId, state.CharacterId, state.Level, state.Experience,
                state.OriginalSlot, revision, state.BaseRewards, state.ProcessedEnds, periods);
            return new CandidateCharacterResult(next, CandidateGrowthOutcome.Applied, period: nextPeriod, anomaly: anomaly);
        }

        internal static bool PrepareTime(CandidateTimeSample input, GrowthChecks c, out PreparedCandidateTimeSample time)
        {
            time = null;
            if (!input.WallUtcMilliseconds.HasValue) return c.Fail(MissingField, "TimeSample.WallUtcMilliseconds");
            if (!input.ObservedAtUtcMilliseconds.HasValue) return c.Fail(MissingField, "TimeSample.ObservedAtUtcMilliseconds");
            c.Budget.CheckInteger(input.WallUtcMilliseconds.Value); c.Budget.CheckInteger(input.ObservedAtUtcMilliseconds.Value);
            if (!input.HasMonotonic) return c.Fail(MissingField, "TimeSample.MonotonicElapsedMilliseconds");
            if (!input.HasScope) return c.Fail(MissingField, "TimeSample.MonotonicScopeId");
            if ((input.MonotonicElapsedMilliseconds == null) != (input.MonotonicScopeId == null))
                return c.Fail(InconsistentBinding, "TimeSample.MonotonicScopeId");
            if (input.MonotonicElapsedMilliseconds != null &&
                (!c.Number(input.MonotonicElapsedMilliseconds, "TimeSample.MonotonicElapsedMilliseconds") ||
                 !c.Text(input.MonotonicScopeId, "TimeSample.MonotonicScopeId"))) return false;
            if (!c.Text(input.Source, "TimeSample.Source")) return false;
            if (input.Trust == CandidateTimeTrust.Unspecified) return c.Fail(MissingField, "TimeSample.Trust");
            if (input.Trust != CandidateTimeTrust.DeviceUntrusted) return c.Fail(UnsupportedBinding, "TimeSample.Trust");
            if (!input.Anomaly.HasValue) return c.Fail(MissingField, "TimeSample.Anomaly");
            if ((input.Anomaly.Value & ~(CandidateTimeAnomaly.ClockBackward | CandidateTimeAnomaly.DomainChanged)) != 0)
                return c.Fail(InvalidValue, "TimeSample.Anomaly");
            time = new PreparedCandidateTimeSample(input);
            return true;
        }

        internal static bool SameTime(PreparedCandidateTimeSample a, PreparedCandidateTimeSample b, GrowthChecks c)
        {
            if (a == null || b == null) return a == b;
            return c.Budget.Compare(a.WallUtcMilliseconds, b.WallUtcMilliseconds) == 0 &&
                c.Budget.Compare(a.ObservedAtUtcMilliseconds, b.ObservedAtUtcMilliseconds) == 0 &&
                c.SameNumber(a.MonotonicElapsedMilliseconds, b.MonotonicElapsedMilliseconds) &&
                string.Equals(a.MonotonicScopeId, b.MonotonicScopeId, StringComparison.Ordinal) &&
                string.Equals(a.Source, b.Source, StringComparison.Ordinal) && a.Trust == b.Trust && a.Anomaly == b.Anomaly;
        }
    }
}
