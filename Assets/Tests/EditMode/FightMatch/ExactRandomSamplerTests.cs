using System;
using System.Collections.Generic;
using System.Numerics;
using FightMatch.Core;
using NUnit.Framework;

namespace FightMatch.Core.Tests
{
    [TestFixture]
    public class ExactRandomSamplerTests
    {
        private static readonly BigInteger WordRange = BigInteger.One << 32;
        private static readonly uint[] ReferenceWords =
        {
            0xa15c02b7U, 0x7b47f409U, 0xba1d3330U,
            0x83d2f293U, 0xbfa4784bU, 0xcbed606eU
        };

        [Test]
        public void RecordedVectors_CountRejectionWordsAndUseStrictThreshold()
        {
            var budget = Budget();
            var value = ExactRandomSampler.UniformRecorded(3, new uint[] { 0, 1 }, budget, out var used);
            Assert.AreEqual(BigInteger.One, value);
            Assert.AreEqual(2, used);
            Assert.AreEqual(2, budget.WordsUsed);

            value = ExactRandomSampler.UniformRecorded(WordRange + 1, new uint[] { 0, 0, 0, 1 },
                budget, out used);
            Assert.AreEqual(BigInteger.One, value);
            Assert.AreEqual(4, used);
            Assert.AreEqual(6, budget.WordsUsed);
        }

        [TestCase(1U, 1)]
        [TestCase(2U, 2)]
        [TestCase(3U, 0)]
        [TestCase(uint.MaxValue, 0)]
        public void RecordedVectors_ThresholdAndModuloBoundaries(uint word, int expected)
        {
            var value = ExactRandomSampler.UniformRecorded(3, new[] { 0U, word }, Budget(), out var used);
            Assert.AreEqual(new BigInteger(expected), value);
            Assert.AreEqual(2, used);
        }

        [TestCase(0U)]
        [TestCase(uint.MaxValue)]
        public void Uniform_TwoTo32_UsesExactlyOneUnchangedWord(uint word)
        {
            var value = ExactRandomSampler.UniformRecorded(WordRange, new[] { word }, Budget(), out var used);
            Assert.AreEqual(new BigInteger(word), value);
            Assert.AreEqual(1, used);
        }

        [Test]
        public void Uniform_MultiWordInputPlacesFirstWordAtHighEnd()
        {
            var value = ExactRandomSampler.UniformRecorded(BigInteger.One << 64,
                new[] { 0x12345678U, 0x9abcdef0U }, Budget(), out var used);
            Assert.AreEqual(new BigInteger(0x123456789abcdef0UL), value);
            Assert.AreEqual(2, used);

            value = ExactRandomSampler.UniformRecorded(BigInteger.One << 96,
                new[] { 0x12345678U, 0x9abcdef0U, 0xfedcba98U }, Budget(), out used);
            Assert.AreEqual((new BigInteger(0x123456789abcdef0UL) << 32) + 0xfedcba98U, value);
            Assert.AreEqual(3, used);
        }

        [Test]
        public void PcgStream_ReferenceWordsAndRestoreContinueWithoutReseeding()
        {
            var original = Pcg32StreamState.Initialize(42, 54);
            var state = original;
            for (var i = 0; i < ReferenceWords.Length; i++)
            {
                if (i == 3)
                    state = Pcg32StreamState.Restore(state.Initial, state.Current, state.WordsConsumed,
                        new ExactMathBudget());
                var result = ExactRandomSampler.Uniform(WordRange, state, Budget());
                Assert.AreEqual(new BigInteger(ReferenceWords[i]), result.Value);
                CollectionAssert.AreEqual(new[] { ReferenceWords[i] }, result.Words);
                Assert.AreEqual(new BigInteger(i + 1), result.NextState.WordsConsumed);
                Assert.AreSame(original.Initial, result.NextState.Initial);
                Assert.AreEqual(new BigInteger(i), state.WordsConsumed);
                state = result.NextState;
            }
            Assert.AreSame(original.Initial, original.Current);
            Assert.AreEqual(BigInteger.Zero, original.WordsConsumed);
        }

        [Test]
        public void PcgStream_ZeroStateAndLargeWordCountersAreLegal()
        {
            var zero = Pcg32Core.Restore(0, 1);
            foreach (var count in new[] { new BigInteger(7), (BigInteger.One << 64) + 7 })
            {
                var state = Pcg32StreamState.Restore(zero, zero, count, new ExactMathBudget());
                var result = ExactRandomSampler.Uniform(WordRange, state, Budget());
                Assert.AreEqual(BigInteger.Zero, result.Value);
                Assert.AreEqual(1UL, result.NextState.Current.State);
                Assert.AreEqual(1UL, result.NextState.Current.Increment);
                Assert.AreEqual(count + 1, result.NextState.WordsConsumed);
                Assert.AreEqual(count, state.WordsConsumed);
                Assert.AreEqual(0UL, state.Current.State);
            }
        }

        [Test]
        public void PcgStream_ActualRejectionMatchesRecordedAdapterAndBernoulliUsesLessThan()
        {
            // These actual PCG states emit 0 then 1; this is not a fabricated PCG trace.
            var core = Pcg32Core.Restore(0, (1UL << 27) + 1);
            var state = Pcg32StreamState.Restore(core, core, 7, new ExactMathBudget());
            var result = ExactRandomSampler.Uniform(3, state, Budget());
            CollectionAssert.AreEqual(new uint[] { 0, 1 }, result.Words);
            Assert.AreEqual(new BigInteger(9), result.NextState.WordsConsumed);
            var recorded = ExactRandomSampler.UniformRecorded(3, result.Words, Budget(), out var used);
            Assert.AreEqual(result.Value, recorded);
            Assert.AreEqual(2, used);
            var low = ExactRandomSampler.Bernoulli(Rational(1, 3), state, Budget());
            var high = ExactRandomSampler.Bernoulli(Rational(2, 3), state, Budget());
            Assert.IsFalse(low.Value);
            Assert.IsTrue(high.Value);
            AssertState(result.NextState, low.NextState);
            AssertState(result.NextState, high.NextState);
            Assert.AreEqual(new BigInteger(7), state.WordsConsumed);
        }

        [Test]
        public void CertainResults_ConsumeNoWordsEvenWithZeroWordBudget()
        {
            var state = Pcg32StreamState.Initialize(42, 54);
            var budget = Budget(0);
            var integer = ExactRandomSampler.Uniform(1, state, budget);
            var never = ExactRandomSampler.Bernoulli(Rational(0, 7), state, budget);
            var always = ExactRandomSampler.Bernoulli(Rational(7, 7), state, budget);
            Assert.AreEqual(BigInteger.Zero, integer.Value);
            Assert.IsFalse(never.Value);
            Assert.IsTrue(always.Value);
            Assert.AreSame(state, integer.NextState);
            Assert.AreSame(state, never.NextState);
            Assert.AreSame(state, always.NextState);
            Assert.IsEmpty(integer.Words);
            Assert.IsEmpty(never.Words);
            Assert.IsEmpty(always.Words);
            Assert.AreEqual(0, budget.WordsUsed);
            Assert.AreEqual(BigInteger.Zero,
                ExactRandomSampler.UniformRecorded(1, new uint[0], budget, out var used));
            Assert.AreEqual(0, used);
        }

        [Test]
        public void Prd_ThreeActualFailuresThenGuaranteedSuccessResetWithoutTakingWord()
        {
            // Fixture derived independently from the fixed PCG recurrence: seed 10 / sequence 54.
            var expected = new[] { 0xe188288bU, 0xbd53b535U, 0xaaa5f027U };
            var state = Pcg32StreamState.Initialize(10, 54);
            var constant = Rational(1, 4);
            var failures = BigInteger.Zero;
            var budget = Budget(3);
            for (var i = 0; i < 3; i++)
            {
                var result = ExactRandomSampler.PrdOpportunity(constant, failures, state, budget);
                Assert.IsFalse(result.Value.Triggered);
                Assert.AreEqual(new BigInteger(i + 1), result.Value.Failures);
                AssertRational(Rational(i + 1, 4), result.Value.Probability);
                CollectionAssert.AreEqual(new[] { expected[i] }, result.Words);
                state = result.NextState;
                failures = result.Value.Failures;
            }
            var guaranteed = ExactRandomSampler.PrdOpportunity(constant, failures, state, budget);
            Assert.IsTrue(guaranteed.Value.Triggered);
            Assert.AreEqual(BigInteger.Zero, guaranteed.Value.Failures);
            AssertRational(Rational(1, 1), guaranteed.Value.Probability);
            Assert.AreSame(state, guaranteed.NextState);
            Assert.IsEmpty(guaranteed.Words);
            Assert.AreEqual(3, budget.WordsUsed);
            Assert.AreEqual(new BigInteger(3), state.WordsConsumed);
            AssertRational(Rational(1, 4), constant);
        }

        [Test]
        public void Prd_OrdinarySuccessAndCeilingBoundaryUseOriginalConstant()
        {
            var state = Pcg32StreamState.Initialize(42, 54);
            var ordinary = ExactRandomSampler.PrdOpportunity(Rational(1, 4), 2, state, Budget());
            Assert.IsFalse(ordinary.Value.Triggered); // First word modulo 4 is 3, q is 3/4.
            var success = ExactRandomSampler.PrdOpportunity(Rational(2, 3), 1, state, Budget(0));
            Assert.IsTrue(success.Value.Triggered);
            Assert.AreEqual(BigInteger.Zero, success.Value.Failures);
            Assert.IsEmpty(success.Words);

            var zero = Pcg32Core.Restore(0, 1);
            var zeroState = Pcg32StreamState.Restore(zero, zero, 0, new ExactMathBudget());
            success = ExactRandomSampler.PrdOpportunity(Rational(1, 4), 1, zeroState, Budget());
            Assert.IsTrue(success.Value.Triggered);
            Assert.AreEqual(BigInteger.Zero, success.Value.Failures);
            Assert.AreEqual(1, success.Words.Count);
            AssertRational(Rational(1, 2), success.Value.Probability);
            success = ExactRandomSampler.PrdOpportunity(Rational(1, 1), 0, state, Budget(0));
            Assert.IsTrue(success.Value.Triggered);
            Assert.AreEqual(BigInteger.Zero, success.Value.Failures);
        }

        [Test]
        public void Prd_NormalizesOpportunityProbabilityBeforeSampling()
        {
            var core = Pcg32Core.Restore(2UL << 27, 1);
            var state = Pcg32StreamState.Restore(core, core, 0, new ExactMathBudget());
            var result = ExactRandomSampler.PrdOpportunity(Rational(1, 4), 1, state, Budget());
            CollectionAssert.AreEqual(new uint[] { 2 }, result.Words);
            Assert.IsTrue(result.Value.Triggered); // 2 modulo 2 is 0; using unreduced 2/4 would fail.
            AssertRational(Rational(1, 2), result.Value.Probability);
            Assert.AreEqual(BigInteger.Zero, result.Value.Failures);
        }

        [Test]
        public void InterleavedStreams_DoNotConsumeEachOthersWords()
        {
            var battle = Pcg32StreamState.Initialize(42, 54);
            var reward = Pcg32StreamState.Initialize(10, 54);
            var first = ExactRandomSampler.Uniform(WordRange, battle, Budget());
            var other = ExactRandomSampler.Uniform(WordRange, reward, Budget());
            var second = ExactRandomSampler.Uniform(WordRange, first.NextState, Budget());
            Assert.AreEqual(new BigInteger(ReferenceWords[0]), first.Value);
            Assert.AreEqual(new BigInteger(0xe188288bU), other.Value);
            Assert.AreEqual(new BigInteger(ReferenceWords[1]), second.Value);
            Assert.AreEqual(BigInteger.Zero, battle.WordsConsumed);
            Assert.AreEqual(BigInteger.Zero, reward.WordsConsumed);
        }

        [Test]
        public void WordLimit_RejectedAndPartialGroupsFailWithoutPublishingCandidate()
        {
            var core = Pcg32Core.Restore(0, (1UL << 27) + 1);
            var state = Pcg32StreamState.Restore(core, core, 7, new ExactMathBudget());
            var budget = Budget(1);
            RandomSample<BigInteger> result = null;
            AssertLimit("RandomWords", () => result = ExactRandomSampler.Uniform(3, state, budget));
            Assert.IsNull(result);
            Assert.AreEqual(1, budget.WordsUsed);
            Assert.AreEqual(0UL, state.Current.State);
            Assert.AreEqual(new BigInteger(7), state.WordsConsumed);
            result = ExactRandomSampler.Uniform(3, state, Budget(2));
            CollectionAssert.AreEqual(new uint[] { 0, 1 }, result.Words);

            var multiBudget = Budget(3);
            AssertLimit("RandomWords", () => ExactRandomSampler.UniformRecorded(WordRange + 1,
                new uint[] { 0, 0, 0, 1 }, multiBudget, out _));
            Assert.AreEqual(3, multiBudget.WordsUsed);
            Assert.AreEqual(BigInteger.One, ExactRandomSampler.UniformRecorded(WordRange + 1,
                new uint[] { 0, 0, 0, 1 }, Budget(4), out var consumed));
            Assert.AreEqual(4, consumed);

            var prefixBudget = Budget(1);
            result = null;
            AssertLimit("RandomWords", () => result = ExactRandomSampler.Uniform(WordRange + 1,
                state, prefixBudget));
            Assert.IsNull(result);
            Assert.AreEqual(1, prefixBudget.WordsUsed);
            Assert.AreEqual(new BigInteger(7), state.WordsConsumed);
            Assert.AreEqual(0UL, state.Current.State);
        }

        [Test]
        public void WordLimit_IsSharedAcrossCallsAndDoesNotAlterPreviousResults()
        {
            var original = Pcg32StreamState.Initialize(42, 54);
            var budget = Budget(1);
            var first = ExactRandomSampler.Uniform(WordRange, original, budget);
            AssertLimit("RandomWords", () => ExactRandomSampler.Uniform(WordRange, first.NextState, budget));
            Assert.AreEqual(1, budget.WordsUsed);
            CollectionAssert.AreEqual(new[] { ReferenceWords[0] }, first.Words);
            Assert.Throws<NotSupportedException>(() => ((IList<uint>)first.Words)[0] = 0);
            var again = ExactRandomSampler.Uniform(WordRange, original, Budget());
            Assert.AreEqual(first.Value, again.Value);
            AssertState(first.NextState, again.NextState);
            CollectionAssert.AreEqual(first.Words, again.Words);
        }

        [Test]
        public void StepLimit_AfterDrawingDoesNotPublishFalseBernoulliOrPrdResult()
        {
            var state = Pcg32StreamState.Initialize(42, 54);
            var probability = Rational(1, 3);
            var measurement = Budget();
            var expected = ExactRandomSampler.Bernoulli(probability, state, measurement);
            var limited = new RandomSamplingBudget(new ExactMathBudget(32768,
                (int)measurement.Math.PrimitiveStepsUsed - 1));
            RandomSample<bool> result = null;
            AssertLimit("PrimitiveSteps", () => result = ExactRandomSampler.Bernoulli(probability, state, limited));
            Assert.IsNull(result);
            Assert.Greater(limited.WordsUsed, 0);
            Assert.AreEqual(BigInteger.Zero, state.WordsConsumed);
            var retry = ExactRandomSampler.Bernoulli(probability, state, Budget());
            Assert.AreEqual(expected.Value, retry.Value);
            AssertState(expected.NextState, retry.NextState);

            var prdBudget = Budget(0);
            RandomSample<PrdOutcome> prdResult = null;
            AssertLimit("RandomWords", () => prdResult = ExactRandomSampler.PrdOpportunity(
                Rational(1, 4), 2, state, prdBudget));
            Assert.IsNull(prdResult);
            Assert.AreEqual(BigInteger.Zero, state.WordsConsumed);

            measurement = Budget();
            var constant = Rational(1, 4);
            var expectedPrd = ExactRandomSampler.PrdOpportunity(constant, 2, state, measurement);
            limited = new RandomSamplingBudget(new ExactMathBudget(32768,
                (int)measurement.Math.PrimitiveStepsUsed - 1));
            AssertLimit("PrimitiveSteps", () => prdResult = ExactRandomSampler.PrdOpportunity(
                constant, 2, state, limited));
            Assert.IsNull(prdResult);
            Assert.Greater(limited.WordsUsed, 0);
            var prdRetry = ExactRandomSampler.PrdOpportunity(constant, 2, state, Budget());
            Assert.AreEqual(expectedPrd.Value.Triggered, prdRetry.Value.Triggered);
            Assert.AreEqual(expectedPrd.Value.Failures, prdRetry.Value.Failures);
            AssertState(expectedPrd.NextState, prdRetry.NextState);
        }

        [Test]
        public void StepLimit_RemainsSharedAcrossDistinctSamples()
        {
            var words = new uint[] { 1 };
            var measurement = Budget();
            ExactRandomSampler.UniformRecorded(3, words, measurement, out _);
            var steps = (int)measurement.Math.PrimitiveStepsUsed;
            var shared = new RandomSamplingBudget(new ExactMathBudget(32768, steps));
            ExactRandomSampler.UniformRecorded(3, words, shared, out _);
            AssertLimit("PrimitiveSteps", () => ExactRandomSampler.UniformRecorded(3, words, shared, out _));
            Assert.AreEqual(steps, shared.Math.PrimitiveStepsUsed);
            Assert.AreEqual(1, shared.WordsUsed);
        }

        [Test]
        public void IntegerLimit_ChecksModulusAndCounterBeforeOversizedResult()
        {
            var budget = new RandomSamplingBudget(new ExactMathBudget(32));
            AssertLimit("IntegerBits", () => ExactRandomSampler.UniformRecorded(3, new uint[] { 1 }, budget, out _));
            Assert.AreEqual(0, budget.WordsUsed);
            budget = new RandomSamplingBudget(new ExactMathBudget(64));
            AssertLimit("IntegerBits", () => ExactRandomSampler.UniformRecorded(WordRange + 1,
                new uint[] { 0, 1 }, budget, out _));
            Assert.AreEqual(0, budget.WordsUsed);

            var core = Pcg32Core.Restore(0, 1);
            var state = Pcg32StreamState.Restore(core, core, (BigInteger.One << 64) - 1, new ExactMathBudget());
            budget = new RandomSamplingBudget(new ExactMathBudget(64));
            AssertLimit("IntegerBits", () => ExactRandomSampler.Uniform(2, state, budget));
            Assert.AreEqual(0, budget.WordsUsed);
            Assert.AreEqual((BigInteger.One << 64) - 1, state.WordsConsumed);
        }

        [Test]
        public void RecordedWords_ExhaustionRejectsInsteadOfSupplyingZeros()
        {
            var words = new uint[] { 0 };
            var budget = Budget();
            Assert.Throws<InvalidOperationException>(() =>
                ExactRandomSampler.UniformRecorded(3, words, budget, out _));
            Assert.AreEqual(1, budget.WordsUsed);
            CollectionAssert.AreEqual(new uint[] { 0 }, words);
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void InvalidBounds_RejectBeforeTakingWords(int bound)
        {
            var budget = Budget();
            Assert.Throws<ArgumentOutOfRangeException>(() => ExactRandomSampler.Uniform(bound,
                Pcg32StreamState.Initialize(42, 54), budget));
            Assert.AreEqual(0, budget.WordsUsed);
        }

        [TestCase(-1, 3)]
        [TestCase(4, 3)]
        public void InvalidProbabilities_RejectBeforeTakingWords(int n, int d)
        {
            var budget = Budget();
            Assert.Throws<ArgumentOutOfRangeException>(() => ExactRandomSampler.Bernoulli(Rational(n, d),
                Pcg32StreamState.Initialize(42, 54), budget));
            Assert.AreEqual(0, budget.WordsUsed);
        }

        [TestCase(0, 1, 0)]
        [TestCase(-1, 4, 0)]
        [TestCase(5, 4, 0)]
        [TestCase(1, 4, -1)]
        [TestCase(1, 4, 4)]
        [TestCase(2, 3, 2)]
        [TestCase(1, 1, 1)]
        public void InvalidPrd_RejectsConstantAndCompletedCounterOutsideDomain(int n, int d, int failures)
        {
            var budget = Budget();
            Assert.Throws<ArgumentOutOfRangeException>(() => ExactRandomSampler.PrdOpportunity(Rational(n, d),
                failures, Pcg32StreamState.Initialize(42, 54), budget));
            Assert.AreEqual(0, budget.WordsUsed);
        }

        [Test]
        public void InvalidRestores_RejectInconsistentCoresNegativeCountAndNulls()
        {
            var core = Pcg32Core.Restore(0, 1);
            var math = new ExactMathBudget();
            Assert.Throws<ArgumentException>(() => Pcg32StreamState.Restore(core, Pcg32Core.Restore(0, 3), 1, math));
            Assert.Throws<ArgumentException>(() => Pcg32StreamState.Restore(core, Pcg32Core.Restore(1, 1), 0, math));
            Assert.Throws<ArgumentOutOfRangeException>(() => Pcg32StreamState.Restore(core, core, -1, math));
            Assert.Throws<ArgumentNullException>(() => Pcg32StreamState.Restore(null, core, 0, math));
            Assert.Throws<ArgumentNullException>(() => Pcg32StreamState.Restore(core, null, 0, math));
            Assert.Throws<ArgumentNullException>(() => Pcg32StreamState.Restore(core, core, 0, null));
        }

        [Test]
        public void NullInputsAndInvalidBudgets_AreRejected()
        {
            var state = Pcg32StreamState.Initialize(42, 54);
            var budget = Budget();
            Assert.Throws<ArgumentNullException>(() => new RandomSamplingBudget(null));
            Assert.Throws<ArgumentOutOfRangeException>(() => new RandomSamplingBudget(new ExactMathBudget(), -1));
            Assert.Throws<ArgumentNullException>(() => ExactRandomSampler.Uniform(1, null, budget));
            Assert.Throws<ArgumentNullException>(() => ExactRandomSampler.Uniform(1, state, null));
            Assert.Throws<ArgumentNullException>(() => ExactRandomSampler.Bernoulli(null, state, budget));
            Assert.Throws<ArgumentNullException>(() => ExactRandomSampler.PrdOpportunity(null, 0, state, budget));
            Assert.Throws<ArgumentNullException>(() => ExactRandomSampler.UniformRecorded(1, null, budget, out _));
            Assert.Throws<ArgumentNullException>(() => ExactRandomSampler.UniformRecorded(1, new uint[0], null, out _));
        }

        private static RandomSamplingBudget Budget(int maxWords = 4096)
        {
            return new RandomSamplingBudget(new ExactMathBudget(), maxWords);
        }

        private static ExactRational Rational(long numerator, long denominator)
        {
            return ExactRational.Create(numerator, denominator, new ExactMathBudget());
        }

        private static void AssertRational(ExactRational expected, ExactRational actual)
        {
            Assert.AreEqual(expected.Numerator, actual.Numerator);
            Assert.AreEqual(expected.Denominator, actual.Denominator);
        }

        private static void AssertState(Pcg32StreamState expected, Pcg32StreamState actual)
        {
            Assert.AreEqual(expected.Initial.State, actual.Initial.State);
            Assert.AreEqual(expected.Initial.Increment, actual.Initial.Increment);
            Assert.AreEqual(expected.Current.State, actual.Current.State);
            Assert.AreEqual(expected.Current.Increment, actual.Current.Increment);
            Assert.AreEqual(expected.WordsConsumed, actual.WordsConsumed);
        }

        private static void AssertLimit(string reason, TestDelegate action)
        {
            var error = Assert.Throws<ExactMathLimitException>(action);
            Assert.AreEqual(reason, error.ReasonCode);
            Assert.Greater(error.RequiredAtLeast, error.Allowed);
        }
    }
}
