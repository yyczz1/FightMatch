using System;
using System.Numerics;
using FightMatch.Core;
using NUnit.Framework;

namespace FightMatch.Core.Tests
{
    [TestFixture]
    public class ExactScoreCalculatorTests
    {
        [TestCase(30, 1)]
        [TestCase(125, 4)]
        public void DesignWitness_G01BothContributionBranchesGiveFourteen(int numerator, int denominator)
        {
            var budget = Budget();
            using (var scope = new ExactEvaluationScope(budget))
            {
                var result = ExactScoreCalculator.Evaluate(20, Q(9, 16), Q(1, 2), Q(1, 2),
                    Q(numerator, denominator), Q(15), scope);
                AssertResult(14, result);
                Assert.Greater(result.TermsUsed, 0);
                Assert.AreEqual(2 * result.TermsUsed, budget.LogTermsUsed);
                Assert.Greater(budget.ReservedIntegerBits, 0);
                Assert.LessOrEqual(budget.PeakReservedIntegerBits, budget.MaxLiveIntegerBits);
            }
            Assert.AreEqual(0, budget.ReservedIntegerBits);
        }

        [TestCase(399, 100, 0)]
        [TestCase(4, 1, 1)]
        [TestCase(401, 100, 1)]
        public void DesignWitness_NR04IntegerBoundaryUsesExactLogarithm(int numerator, int denominator, int expected)
        {
            var budget = Budget();
            using (var scope = new ExactEvaluationScope(budget))
            {
                // F=(1+log2(x))/3; x below/at/above 4 lies below/at/above F=1.
                var result = Boundary(numerator, denominator, scope);
                AssertResult(expected, result);
                if (denominator == 1)
                {
                    Assert.AreEqual(0, result.TermsUsed);
                    Assert.AreEqual(0, budget.LogTermsUsed);
                }
                else
                {
                    Assert.Greater(result.TermsUsed, 0);
                }
            }
        }

        [TestCase(200)]
        [TestCase(2000)]
        public void ExactPowerOfTwo_AboveMachineWordRangeNeedsNoSeries(int exponent)
        {
            var budget = Budget(logTerms: 0);
            using (var scope = new ExactEvaluationScope(budget))
            {
                var contribution = ExactRational.Create((BigInteger.One << exponent) - 1, 1,
                    new ExactMathBudget());
                var result = ExactScoreCalculator.Evaluate(1, Q(1), Q(0), Q(1), contribution, Q(1), scope);
                AssertResult(exponent, result);
                AssertEqual(result.LowerBound, result.UpperBound);
                Assert.AreEqual(0, result.TermsUsed);
                Assert.AreEqual(0, budget.LogTermsUsed);
            }
        }

        [Test]
        public void LogOfOneAndZeroSlope_ReturnExactRationalFloorWithoutSeries()
        {
            var budget = Budget(logTerms: 0);
            using (var scope = new ExactEvaluationScope(budget))
            {
                var logOne = ExactScoreCalculator.Evaluate(3, Q(1), Q(1, 3), Q(2, 3), Q(0), Q(7), scope);
                AssertResult(1, logOne);
                var zeroSlope = ExactScoreCalculator.Evaluate(14, Q(1), Q(11, 5), Q(0), Q(99), Q(7), scope);
                AssertResult(30, zeroSlope);
                AssertEqual(Q(154, 5), zeroSlope.LowerBound);
                AssertEqual(zeroSlope.LowerBound, zeroSlope.UpperBound);
                Assert.AreEqual(0, zeroSlope.TermsUsed);
                Assert.AreEqual(0, budget.LogTermsUsed);
            }
        }

        [Test]
        public void ZeroExperienceOrLevelMultiplier_ReturnsZeroAfterValidation()
        {
            using (var scope = new ExactEvaluationScope(Budget(logTerms: 0)))
            {
                AssertResult(0, ExactScoreCalculator.Evaluate(0, Q(1), Q(1), Q(1), Q(30), Q(15), scope));
                AssertResult(0, ExactScoreCalculator.Evaluate(20, Q(0), Q(1), Q(1), Q(30), Q(15), scope));
            }
        }

        [Test]
        public void RangeReduction_AdjustsInitialExponentEstimateAndPreservesLogScaling()
        {
            // Independent integer inequalities prove floor(100*log2(5/3))=73.
            var numerator = BigInteger.Pow(5, 100);
            var denominator = BigInteger.Pow(3, 100);
            Assert.Greater(numerator, denominator << 73);
            Assert.Less(numerator, denominator << 74);
            using (var scope = new ExactEvaluationScope(Budget()))
            {
                var first = ExactScoreCalculator.Evaluate(100, Q(1), Q(0), Q(1), Q(2), Q(3), scope);
                var second = ExactScoreCalculator.Evaluate(100, Q(1), Q(0), Q(1), Q(7), Q(3), scope);
                AssertResult(73, first);
                AssertResult(173, second);
                Assert.AreEqual(first.TermsUsed, second.TermsUsed);
                var math = new ExactMathBudget();
                AssertEqual(Q(100), second.LowerBound.Subtract(first.LowerBound, math));
                AssertEqual(Q(100), second.UpperBound.Subtract(first.UpperBound, math));
            }
        }

        [TestCase(399L, 100L)]
        [TestCase(3999999999999L, 1000000000000L)]
        public void NearIntegerBoundary_RefinesInsteadOfReturningAnApproximation(long numerator, long denominator)
        {
            var limited = Budget(logTerms: 2);
            using (var scope = new ExactEvaluationScope(limited))
            {
                ExactScoreResult rejected = null;
                AssertLimit("LogTerms", () => rejected = Boundary(numerator, denominator, scope));
                Assert.IsNull(rejected);
                Assert.AreEqual(2, limited.LogTermsUsed);
                Assert.AreEqual(0, limited.ReservedIntegerBits);
            }
            using (var scope = new ExactEvaluationScope(Budget(logTerms: 128)))
            {
                var result = Boundary(numerator, denominator, scope);
                AssertResult(0, result);
                Assert.Greater(result.TermsUsed, 1);
            }
        }

        [Test]
        public void LogTerms_AreSharedAcrossCallsAndFailurePreservesEarlierResult()
        {
            var measured = Budget();
            int terms;
            using (var scope = new ExactEvaluationScope(measured))
                terms = Witness(scope).TermsUsed * 2;
            var budget = Budget(logTerms: terms);
            using (var scope = new ExactEvaluationScope(budget))
            {
                var first = Witness(scope);
                var retained = budget.ReservedIntegerBits;
                var steps = budget.Math.PrimitiveStepsUsed;
                AssertLimit("LogTerms", () => Witness(scope));
                Assert.AreEqual(terms, budget.LogTermsUsed);
                Assert.AreEqual(retained, budget.ReservedIntegerBits);
                Assert.Greater(budget.Math.PrimitiveStepsUsed, steps);
                AssertResult(14, first);
            }
            Assert.AreEqual(0, budget.ReservedIntegerBits);
        }

        [Test]
        public void LogTerms_PairRequiresCapacityForBothSeries()
        {
            var budget = Budget(logTerms: 1);
            using (var scope = new ExactEvaluationScope(budget))
            {
                var error = Assert.Throws<ExactMathLimitException>(() => Witness(scope));
                Assert.AreEqual("LogTerms", error.ReasonCode);
                Assert.AreEqual(2, error.RequiredAtLeast);
                Assert.AreEqual(1, error.Allowed);
                Assert.AreEqual(0, budget.LogTermsUsed);
                Assert.AreEqual(0, budget.ReservedIntegerBits);
            }
        }

        [Test]
        public void Workspace_RetainsResultsAcrossCallsAndReleasesOnDispose()
        {
            var budget = Budget(integerBits: 64);
            var scope = new ExactEvaluationScope(budget);
            for (var i = 1; i <= 10; i++)
            {
                AssertResult(2, ExactTwo(scope));
                // Each result retains 2/1 twice (3 bits each) and integer 2 (2 bits).
                Assert.AreEqual(8 * i, budget.ReservedIntegerBits);
            }
            var steps = budget.Math.PrimitiveStepsUsed;
            var peak = budget.PeakReservedIntegerBits;
            scope.Dispose();
            scope.Dispose();
            Assert.AreEqual(0, budget.ReservedIntegerBits);
            Assert.AreEqual(steps, budget.Math.PrimitiveStepsUsed);
            Assert.AreEqual(peak, budget.PeakReservedIntegerBits);
            Assert.Throws<ObjectDisposedException>(() => ExactTwo(scope));
            Assert.AreEqual(steps, budget.Math.PrimitiveStepsUsed);
        }

        [Test]
        public void Workspace_MultipleScopesShareOneTotalReservation()
        {
            var budget = Budget(integerBits: 64);
            using (var first = new ExactEvaluationScope(budget))
            {
                ExactTwo(first);
                using (var second = new ExactEvaluationScope(budget))
                {
                    ExactTwo(second);
                    Assert.AreEqual(16, budget.ReservedIntegerBits);
                }
                Assert.AreEqual(8, budget.ReservedIntegerBits);
                AssertResult(2, ExactTwo(first));
                Assert.AreEqual(16, budget.ReservedIntegerBits);
            }
            Assert.AreEqual(0, budget.ReservedIntegerBits);
        }

        [Test]
        public void Workspace_ScratchReservationFailsBeforeCalculationAndCanRetryWithMoreCapacity()
        {
            var budget = Budget(integerBits: 64, liveBits: 1000);
            using (var scope = new ExactEvaluationScope(budget))
            {
                for (var i = 0; i < 3; i++)
                {
                    ExactScoreResult result = null;
                    var error = Assert.Throws<ExactMathLimitException>(() => result = ExactTwo(scope));
                    Assert.AreEqual("LiveIntegerBits", error.ReasonCode);
                    Assert.GreaterOrEqual(error.RequiredAtLeast, 24 * 64);
                    Assert.AreEqual(1000, error.Allowed);
                    Assert.IsNull(result);
                    Assert.AreEqual(0, budget.ReservedIntegerBits);
                }
                Assert.LessOrEqual(budget.PeakReservedIntegerBits, 1000);
            }
            using (var scope = new ExactEvaluationScope(Budget(integerBits: 64, liveBits: 2000)))
                AssertResult(2, ExactTwo(scope));
        }

        [Test]
        public void Workspace_FailedSecondCalculationKeepsFirstResultAndItsReservation()
        {
            var measurement = Budget(integerBits: 64);
            using (var scope = new ExactEvaluationScope(measurement))
                ExactTwo(scope);
            var capacity = (int)measurement.PeakReservedIntegerBits;
            var limited = Budget(integerBits: 64, liveBits: capacity);
            using (var scope = new ExactEvaluationScope(limited))
            {
                var first = ExactTwo(scope);
                AssertLimit("LiveIntegerBits", () => ExactTwo(scope));
                Assert.AreEqual(8, limited.ReservedIntegerBits);
                AssertResult(2, first);
            }
            using (var scope = new ExactEvaluationScope(Budget(integerBits: 64, liveBits: capacity + 8)))
            {
                AssertResult(2, ExactTwo(scope));
                AssertResult(2, ExactTwo(scope));
            }
        }

        [Test]
        public void StepBudget_FailureAfterComputationReturnsNoResultAndReleasesWorkspace()
        {
            var measurement = Budget();
            using (var scope = new ExactEvaluationScope(measurement))
                Witness(scope);
            var limited = Budget(steps: (int)measurement.Math.PrimitiveStepsUsed - 1);
            using (var scope = new ExactEvaluationScope(limited))
            {
                ExactScoreResult result = null;
                AssertLimit("PrimitiveSteps", () => result = Witness(scope));
                Assert.IsNull(result);
                Assert.AreEqual(0, limited.ReservedIntegerBits);
                Assert.Greater(limited.LogTermsUsed, 0);
            }
            using (var scope = new ExactEvaluationScope(Budget()))
                AssertResult(14, Witness(scope));
        }

        [Test]
        public void StepBudget_RemainsSharedAcrossScores()
        {
            var measurement = Budget(integerBits: 64);
            using (var scope = new ExactEvaluationScope(measurement))
                ExactTwo(scope);
            var budget = Budget(integerBits: 64, steps: (int)measurement.Math.PrimitiveStepsUsed);
            using (var scope = new ExactEvaluationScope(budget))
            {
                var first = ExactTwo(scope);
                AssertLimit("PrimitiveSteps", () => ExactTwo(scope));
                Assert.AreEqual(8, budget.ReservedIntegerBits);
                AssertResult(2, first);
            }
        }

        [Test]
        public void IntegerLimit_IsEnforcedForInputsAndArithmeticResults()
        {
            var budget = Budget(integerBits: 8);
            using (var scope = new ExactEvaluationScope(budget))
            {
                AssertLimit("IntegerBits", () => ExactScoreCalculator.Evaluate(256, Q(1), Q(1), Q(0),
                    Q(0), Q(1), scope));
                AssertLimit("IntegerBits", () => ExactScoreCalculator.Evaluate(128, Q(2), Q(1), Q(0),
                    Q(0), Q(1), scope));
                Assert.AreEqual(0, budget.ReservedIntegerBits);
            }
        }

        [TestCase("experience")]
        [TestCase("levelMultiplier")]
        [TestCase("baseCoefficient")]
        [TestCase("logCoefficient")]
        [TestCase("contribution")]
        [TestCase("reference")]
        public void InvalidNegativeDomains_AreRejectedWithoutKeepingValues(string field)
        {
            var budget = Budget();
            using (var scope = new ExactEvaluationScope(budget))
            {
                Assert.Throws<ArgumentOutOfRangeException>(() => ExactScoreCalculator.Evaluate(
                    field == "experience" ? -1 : 1,
                    Q(field == "levelMultiplier" ? -1 : 1), Q(field == "baseCoefficient" ? -1 : 1),
                    Q(field == "logCoefficient" ? -1 : 1), Q(field == "contribution" ? -1 : 1),
                    Q(field == "reference" ? -1 : 1), scope));
                Assert.AreEqual(0, budget.ReservedIntegerBits);
            }
        }

        [TestCase(0)]
        [TestCase(20)]
        public void MissingOrZeroReference_IsRejectedEvenForZeroExperienceOrSlope(int experience)
        {
            var budget = Budget();
            using (var scope = new ExactEvaluationScope(budget))
            {
                Assert.Throws<ArgumentNullException>(() => ExactScoreCalculator.Evaluate(experience,
                    Q(1), Q(1), Q(0), Q(0), null, scope));
                Assert.Throws<ArgumentOutOfRangeException>(() => ExactScoreCalculator.Evaluate(experience,
                    Q(1), Q(1), Q(0), Q(0), Q(0), scope));
                Assert.AreEqual(0, budget.ReservedIntegerBits);
            }
        }

        [Test]
        public void NullArgumentsAndInvalidBudgets_AreRejected()
        {
            Assert.Throws<ArgumentNullException>(() => new ExactEvaluationBudget(null));
            Assert.Throws<ArgumentOutOfRangeException>(() => Budget(liveBits: -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => Budget(logTerms: -1));
            Assert.Throws<ArgumentNullException>(() => new ExactEvaluationScope(null));
            Assert.Throws<ArgumentNullException>(() => ExactScoreCalculator.Evaluate(1, Q(1), Q(1),
                Q(1), Q(1), Q(1), null));
            var budget = Budget();
            using (var scope = new ExactEvaluationScope(budget))
            {
                for (var missing = 0; missing < 5; missing++)
                {
                    var values = new[] { Q(1), Q(1), Q(1), Q(1), Q(1) };
                    values[missing] = null;
                    Assert.Throws<ArgumentNullException>(() => ExactScoreCalculator.Evaluate(0,
                        values[0], values[1], values[2], values[3], values[4], scope));
                    Assert.AreEqual(0, budget.ReservedIntegerBits);
                }
            }
        }

        [Test]
        public void ZeroResourceBudgets_RejectWithoutLeakingReservations()
        {
            foreach (var budget in new[] { Budget(liveBits: 0), Budget(steps: 0) })
            {
                using (var scope = new ExactEvaluationScope(budget))
                {
                    Assert.Throws<ExactMathLimitException>(() => ExactTwo(scope));
                    Assert.AreEqual(0, budget.ReservedIntegerBits);
                    Assert.AreEqual(0, budget.LogTermsUsed);
                }
            }
        }

        [Test]
        public void RepeatedEvaluation_PreservesInputsAndReturnsIdenticalEvidence()
        {
            var contribution = Q(125, 4);
            var reference = Q(15);
            using (var scope = new ExactEvaluationScope(Budget()))
            {
                var first = ExactScoreCalculator.Evaluate(20, Q(9, 16), Q(1, 2), Q(1, 2),
                    contribution, reference, scope);
                var second = ExactScoreCalculator.Evaluate(20, Q(9, 16), Q(1, 2), Q(1, 2),
                    contribution, reference, scope);
                Assert.AreEqual(first.Amount, second.Amount);
                Assert.AreEqual(first.TermsUsed, second.TermsUsed);
                AssertEqual(first.LowerBound, second.LowerBound);
                AssertEqual(first.UpperBound, second.UpperBound);
                AssertEqual(Q(125, 4), contribution);
                AssertEqual(Q(15), reference);
            }
        }

        private static ExactScoreResult Boundary(long numerator, long denominator, ExactEvaluationScope scope)
        {
            return ExactScoreCalculator.Evaluate(1, Q(1), Q(1, 3), Q(1, 3),
                Q(numerator - denominator, denominator), Q(1), scope);
        }

        private static ExactScoreResult Witness(ExactEvaluationScope scope)
        {
            return ExactScoreCalculator.Evaluate(20, Q(9, 16), Q(1, 2), Q(1, 2), Q(30), Q(15), scope);
        }

        private static ExactScoreResult ExactTwo(ExactEvaluationScope scope)
        {
            return ExactScoreCalculator.Evaluate(1, Q(1), Q(2), Q(0), Q(0), Q(1), scope);
        }

        private static ExactEvaluationBudget Budget(int integerBits = 32768, int liveBits = 1048576,
            int logTerms = 4096, int steps = 1048576)
        {
            return new ExactEvaluationBudget(new ExactMathBudget(integerBits, steps), liveBits, logTerms);
        }

        private static ExactRational Q(long numerator, long denominator = 1)
        {
            return ExactRational.Create(numerator, denominator, new ExactMathBudget());
        }

        private static void AssertResult(long expected, ExactScoreResult result)
        {
            var math = new ExactMathBudget();
            Assert.AreEqual(new BigInteger(expected), result.Amount);
            Assert.AreEqual(result.Amount, result.LowerBound.Floor(math));
            Assert.AreEqual(result.Amount, result.UpperBound.Floor(math));
            Assert.LessOrEqual(result.LowerBound.Compare(result.UpperBound, math), 0);
        }

        private static void AssertEqual(ExactRational expected, ExactRational actual)
        {
            Assert.AreEqual(expected.Numerator, actual.Numerator);
            Assert.AreEqual(expected.Denominator, actual.Denominator);
        }

        private static void AssertLimit(string reason, TestDelegate action)
        {
            var error = Assert.Throws<ExactMathLimitException>(action);
            Assert.AreEqual(reason, error.ReasonCode);
            Assert.Greater(error.RequiredAtLeast, error.Allowed);
        }
    }
}
