using System.Numerics;

namespace FightMatch.Core
{
    public sealed class PrdOutcome
    {
        public bool Triggered { get; }
        public BigInteger Failures { get; }
        public ExactRational Probability { get; }

        internal PrdOutcome(bool triggered, BigInteger failures, ExactRational probability)
        {
            Triggered = triggered;
            Failures = failures;
            Probability = probability;
        }
    }
}
