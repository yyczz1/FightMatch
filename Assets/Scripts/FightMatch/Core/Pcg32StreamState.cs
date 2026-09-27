using System;
using System.Numerics;

namespace FightMatch.Core
{
    // Mathematical stream state. Business binding and ownership belong to its caller.
    public sealed class Pcg32StreamState
    {
        public Pcg32CoreState Initial { get; }
        public Pcg32CoreState Current { get; }
        public BigInteger WordsConsumed { get; }

        internal Pcg32StreamState(Pcg32CoreState initial, Pcg32CoreState current, BigInteger wordsConsumed)
        {
            Initial = initial;
            Current = current;
            WordsConsumed = wordsConsumed;
        }

        public static Pcg32StreamState Initialize(ulong seed, ulong sequence)
        {
            var initial = Pcg32Core.Initialize(seed, sequence);
            return new Pcg32StreamState(initial, initial, BigInteger.Zero);
        }

        public static Pcg32StreamState Restore(Pcg32CoreState initial, Pcg32CoreState current,
            BigInteger wordsConsumed, ExactMathBudget mathBudget)
        {
            if (initial == null)
                throw new ArgumentNullException(nameof(initial));
            if (current == null)
                throw new ArgumentNullException(nameof(current));
            if (mathBudget == null)
                throw new ArgumentNullException(nameof(mathBudget));
            if (wordsConsumed.Sign < 0)
                throw new ArgumentOutOfRangeException(nameof(wordsConsumed));
            if (initial.Increment != current.Increment ||
                (wordsConsumed.IsZero && initial.State != current.State))
                throw new ArgumentException("Initial and current stream states are inconsistent.");

            var state = new Pcg32StreamState(initial, current, wordsConsumed);
            state.Validate(mathBudget);
            return state;
        }

        internal void Validate(ExactMathBudget budget)
        {
            budget.CheckInteger(Initial.State);
            budget.CheckInteger(Initial.Increment);
            budget.CheckInteger(Current.State);
            budget.CheckInteger(Current.Increment);
            budget.CheckInteger(WordsConsumed);
        }
    }
}
