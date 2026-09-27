using System;

namespace FightMatch.Core
{
    public sealed class ExactMathLimitException : Exception
    {
        public string ReasonCode { get; }
        public long RequiredAtLeast { get; }
        public long Allowed { get; }

        internal ExactMathLimitException(string reasonCode, long requiredAtLeast, long allowed)
            : base($"{reasonCode} requires at least {requiredAtLeast}; allowed {allowed}.")
        {
            ReasonCode = reasonCode;
            RequiredAtLeast = requiredAtLeast;
            Allowed = allowed;
        }
    }
}
