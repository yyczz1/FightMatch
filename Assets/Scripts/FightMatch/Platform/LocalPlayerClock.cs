using System;
using System.Diagnostics;
using System.Numerics;
using FightMatch.Core;

namespace FightMatch.Platform
{
    public sealed class LocalPlayerClock
    {
        private static readonly string ProcessScope = Guid.NewGuid().ToString("N");
        private static readonly long Origin = Stopwatch.GetTimestamp();
        public CandidateTimeSample Read()
        {
            var utc = new BigInteger(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            var elapsed = new BigInteger(Stopwatch.GetTimestamp() - Origin) * 1000;
            return new CandidateTimeSample { WallUtcMilliseconds = utc, ObservedAtUtcMilliseconds = utc,
                MonotonicElapsedMilliseconds = ExactRational.Create(elapsed, Stopwatch.Frequency, new ExactMathBudget()),
                MonotonicScopeId = ProcessScope, Source = "local-device-clock", Trust = CandidateTimeTrust.DeviceUntrusted, Anomaly = CandidateTimeAnomaly.None };
        }
    }
}
