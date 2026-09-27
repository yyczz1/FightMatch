using System.Numerics;

namespace FightMatch.Core
{
    public sealed class ExactScoreResult
    {
        public BigInteger Amount { get; }
        public ExactRational LowerBound { get; }
        public ExactRational UpperBound { get; }
        public int TermsUsed { get; }

        internal ExactScoreResult(BigInteger amount, ExactRational lowerBound,
            ExactRational upperBound, int termsUsed)
        {
            Amount = amount;
            LowerBound = lowerBound;
            UpperBound = upperBound;
            TermsUsed = termsUsed;
        }
    }
}
