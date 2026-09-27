using System;

namespace FightMatch.Core
{
    public sealed class ExactEvaluationBudget
    {
        public ExactMathBudget Math { get; }
        public int MaxLiveIntegerBits { get; }
        public long ReservedIntegerBits { get; private set; }
        public long PeakReservedIntegerBits { get; private set; }
        public int MaxLogTerms { get; }
        public long LogTermsUsed { get; private set; }

        public ExactEvaluationBudget(ExactMathBudget math, int maxLiveIntegerBits = 1048576,
            int maxLogTerms = 4096)
        {
            if (math == null)
                throw new ArgumentNullException(nameof(math));
            if (maxLiveIntegerBits < 0)
                throw new ArgumentOutOfRangeException(nameof(maxLiveIntegerBits));
            if (maxLogTerms < 0)
                throw new ArgumentOutOfRangeException(nameof(maxLogTerms));
            Math = math;
            MaxLiveIntegerBits = maxLiveIntegerBits;
            MaxLogTerms = maxLogTerms;
        }

        internal IDisposable Reserve(long bits)
        {
            var required = ReservedIntegerBits + bits;
            if (required > MaxLiveIntegerBits)
                throw new ExactMathLimitException("LiveIntegerBits", required, MaxLiveIntegerBits);
            var reservation = new Reservation(this, bits);
            ReservedIntegerBits = required;
            if (required > PeakReservedIntegerBits)
                PeakReservedIntegerBits = required;
            return reservation;
        }

        internal void TakeLogPair()
        {
            var required = LogTermsUsed + 2;
            if (required > MaxLogTerms)
                throw new ExactMathLimitException("LogTerms", required, MaxLogTerms);
            LogTermsUsed = required;
        }

        private sealed class Reservation : IDisposable
        {
            private ExactEvaluationBudget owner;
            private readonly long bits;

            internal Reservation(ExactEvaluationBudget owner, long bits)
            {
                this.owner = owner;
                this.bits = bits;
            }

            public void Dispose()
            {
                if (owner == null)
                    return;
                owner.ReservedIntegerBits -= bits;
                owner = null;
            }
        }
    }
}
