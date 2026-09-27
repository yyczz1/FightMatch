using System;
using System.Collections.Generic;
using System.Numerics;

namespace FightMatch.Core
{
    public static class ExactRandomSampler
    {
        public static RandomSample<BigInteger> Uniform(BigInteger bound, Pcg32StreamState state,
            RandomSamplingBudget budget)
        {
            var cursor = CreateCursor(state, budget);
            return cursor.Complete(UniformCore(bound, cursor));
        }

        public static RandomSample<bool> Bernoulli(ExactRational probability, Pcg32StreamState state,
            RandomSamplingBudget budget)
        {
            var cursor = CreateCursor(state, budget);
            ValidateProbability(probability, budget.Math, nameof(probability), false);
            return cursor.Complete(BernoulliCore(probability, cursor));
        }

        public static RandomSample<PrdOutcome> PrdOpportunity(ExactRational constant, BigInteger failures,
            Pcg32StreamState state, RandomSamplingBudget budget)
        {
            var cursor = CreateCursor(state, budget);
            var math = budget.Math;
            ValidateProbability(constant, math, nameof(constant), true);
            math.CheckInteger(failures);
            if (failures.Sign < 0)
                throw new ArgumentOutOfRangeException(nameof(failures));
            var guaranteedAt = math.DivRem(constant.Denominator, constant.Numerator, out var remainder);
            if (!remainder.IsZero)
                guaranteedAt = math.Add(guaranteedAt, BigInteger.One);
            if (math.Compare(failures, guaranteedAt) >= 0)
                throw new ArgumentOutOfRangeException(nameof(failures));

            var nextFailures = math.Add(failures, BigInteger.One);
            var numerator = math.Multiply(nextFailures, constant.Numerator);
            var probability = math.Compare(numerator, constant.Denominator) >= 0
                ? ExactRational.Create(1, 1, math)
                : ExactRational.Create(numerator, constant.Denominator, math);
            var triggered = BernoulliCore(probability, cursor);
            var outcome = new PrdOutcome(triggered, triggered ? BigInteger.Zero : nextFailures, probability);
            return cursor.Complete(outcome);
        }

        // A finite recorded-word adapter for verification; it never fabricates a PCG state.
        public static BigInteger UniformRecorded(BigInteger bound, IReadOnlyList<uint> words,
            RandomSamplingBudget budget, out int wordsConsumed)
        {
            if (words == null)
                throw new ArgumentNullException(nameof(words));
            if (budget == null)
                throw new ArgumentNullException(nameof(budget));
            var cursor = new SamplingCursor(null, words, budget);
            var value = UniformCore(bound, cursor);
            wordsConsumed = cursor.Words.Count;
            return value;
        }

        private static SamplingCursor CreateCursor(Pcg32StreamState state, RandomSamplingBudget budget)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));
            if (budget == null)
                throw new ArgumentNullException(nameof(budget));
            state.Validate(budget.Math);
            return new SamplingCursor(state, null, budget);
        }

        private static void ValidateProbability(ExactRational probability, ExactMathBudget math,
            string parameterName, bool positiveOnly)
        {
            if (probability == null)
                throw new ArgumentNullException(parameterName);
            math.CheckInteger(probability.Numerator);
            math.CheckInteger(probability.Denominator);
            if (probability.Numerator.Sign < 0 || (positiveOnly && probability.Numerator.IsZero) ||
                math.Compare(probability.Numerator, probability.Denominator) > 0)
                throw new ArgumentOutOfRangeException(parameterName);
        }

        private static bool BernoulliCore(ExactRational probability, SamplingCursor cursor)
        {
            if (probability.Numerator.IsZero)
                return false;
            if (cursor.Budget.Math.Compare(probability.Numerator, probability.Denominator) == 0)
                return true;
            return cursor.Budget.Math.Compare(UniformCore(probability.Denominator, cursor),
                probability.Numerator) < 0;
        }

        private static BigInteger UniformCore(BigInteger bound, SamplingCursor cursor)
        {
            var math = cursor.Budget.Math;
            math.CheckInteger(bound);
            if (bound.Sign <= 0)
                throw new ArgumentOutOfRangeException(nameof(bound));
            if (math.Compare(bound, BigInteger.One) == 0)
                return BigInteger.Zero;

            var modulus = BigInteger.One;
            var groupSize = 0;
            do
            {
                modulus = math.ShiftLeft(modulus, 32);
                groupSize++;
            } while (math.Compare(bound, modulus) > 0);
            var threshold = math.Remainder(modulus, bound);

            while (true)
            {
                var candidate = BigInteger.Zero;
                for (var i = 0; i < groupSize; i++)
                {
                    var word = cursor.ReadWord();
                    candidate = math.Add(math.ShiftLeft(candidate, 32), word);
                }
                if (math.Compare(candidate, threshold) >= 0)
                    return math.Remainder(candidate, bound);
            }
        }

        private sealed class SamplingCursor
        {
            private readonly IReadOnlyList<uint> recorded;
            private Pcg32StreamState state;

            internal RandomSamplingBudget Budget { get; }
            internal List<uint> Words { get; } = new List<uint>();

            internal SamplingCursor(Pcg32StreamState state, IReadOnlyList<uint> recorded,
                RandomSamplingBudget budget)
            {
                this.state = state;
                this.recorded = recorded;
                Budget = budget;
            }

            internal uint ReadWord()
            {
                if (recorded != null && Words.Count >= recorded.Count)
                    throw new InvalidOperationException("Recorded random words are exhausted.");
                Budget.CheckNextWord();
                uint word;
                if (recorded == null)
                {
                    var count = Budget.Math.Add(state.WordsConsumed, BigInteger.One);
                    Budget.Math.ChargePcgWord(state.Current.State);
                    word = Pcg32Core.Next32(state.Current, out var next);
                    state = new Pcg32StreamState(state.Initial, next, count);
                }
                else
                {
                    word = recorded[Words.Count];
                }
                Budget.RecordWord();
                Words.Add(word);
                return word;
            }

            internal RandomSample<T> Complete<T>(T value)
            {
                Budget.Math.CheckInteger(state.Current.State);
                return new RandomSample<T>(value, state, Words);
            }
        }
    }
}
