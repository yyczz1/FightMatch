using System;
using System.Numerics;
using FightMatch.Core;
using NUnit.Framework;

namespace FightMatch.Core.Tests
{
    [TestFixture]
    public class ExactRationalTests
    {
        private ExactMathBudget budget;

        [SetUp]
        public void SetUp()
        {
            budget = new ExactMathBudget();
        }

        [TestCase(4, 50, 2, 25)]
        [TestCase(-4, 50, -2, 25)]
        [TestCase(4, -50, -2, 25)]
        [TestCase(-4, -50, 2, 25)]
        [TestCase(0, -50, 0, 1)]
        [TestCase(50, 50, 1, 1)]
        public void Create_NormalizesSignZeroAndCommonFactors(int n, int d, int expectedN, int expectedD)
        {
            AssertValue(expectedN, expectedD, Value(n, d));
        }

        [TestCase(1, 3, 1, 6, 1, 2)]
        [TestCase(-1, 3, 1, 6, -1, 6)]
        [TestCase(1, 3, -1, 3, 0, 1)]
        [TestCase(0, 1, -3, 7, -3, 7)]
        public void Add_ReturnsExactReducedResult(int n, int d, int otherN, int otherD, int resultN, int resultD)
        {
            AssertValue(resultN, resultD, Value(n, d).Add(Value(otherN, otherD), budget));
        }

        [TestCase(1, 3, 1, 2, -1, 6)]
        [TestCase(-1, 3, -1, 2, 1, 6)]
        [TestCase(1, 3, -1, 6, 1, 2)]
        [TestCase(2, 3, 2, 3, 0, 1)]
        public void Subtract_ReturnsExactReducedResult(int n, int d, int otherN, int otherD, int resultN, int resultD)
        {
            AssertValue(resultN, resultD, Value(n, d).Subtract(Value(otherN, otherD), budget));
        }

        [TestCase(2, 3, 9, 4, 3, 2)]
        [TestCase(-2, 3, 9, 4, -3, 2)]
        [TestCase(-2, 3, -9, 4, 3, 2)]
        [TestCase(0, 1, -9, 4, 0, 1)]
        [TestCase(-9, 4, 0, 1, 0, 1)]
        public void Multiply_ReturnsExactReducedResult(int n, int d, int otherN, int otherD, int resultN, int resultD)
        {
            AssertValue(resultN, resultD, Value(n, d).Multiply(Value(otherN, otherD), budget));
        }

        [TestCase(1, 2, 3, 4, 2, 3)]
        [TestCase(-1, 2, 3, 4, -2, 3)]
        [TestCase(1, 2, -3, 4, -2, 3)]
        [TestCase(-1, 2, -3, 4, 2, 3)]
        [TestCase(0, 1, -3, 4, 0, 1)]
        public void Divide_ReturnsExactReducedResult(int n, int d, int otherN, int otherD, int resultN, int resultD)
        {
            AssertValue(resultN, resultD, Value(n, d).Divide(Value(otherN, otherD), budget));
        }

        [Test]
        public void Arithmetic_ReducesBeforeProductsToFitSmallBudget()
        {
            budget = new ExactMathBudget(8);
            var first = Value(127, 128);
            var small = Value(1, 128);
            AssertValue(1, 1, first.Add(small, budget));
            AssertValue(63, 64, first.Subtract(small, budget));
            AssertValue(1, 1, first.Multiply(Value(128, 127), budget));
            AssertValue(1, 1, first.Divide(first, budget));
            AssertValue(127, 128, first);
            AssertValue(1, 128, small);
        }

        [TestCase(1, 3, 2, 6, 0)]
        [TestCase(1, 3, 1, 2, -1)]
        [TestCase(-1, 3, -1, 2, 1)]
        [TestCase(-1, 3, 1, 3, -1)]
        [TestCase(1, 3, 0, 1, 1)]
        [TestCase(0, 1, 0, 1, 0)]
        [TestCase(0, 1, -1, 3, 1)]
        public void Compare_OrdersSignedExactValues(int n, int d, int otherN, int otherD, int expected)
        {
            Assert.AreEqual(expected, Value(n, d).Compare(Value(otherN, otherD), budget));
        }

        [Test]
        public void Arithmetic_AboveBinary64IntegerPrecision_PreservesUnitDifference()
        {
            var first = Value(9007199254740992L, 1);
            var second = Value(9007199254740993L, 1);
            Assert.AreEqual(-1, first.Compare(second, budget));
            AssertValue(1, 1, second.Subtract(first, budget));
            AssertValue(9007199254740993L, 1, first.Add(Value(1, 1), budget));
            Assert.AreEqual(-1, Value(10000000000L, 10000000001L)
                .Compare(Value(10000000001L, 10000000002L), budget));
        }

        [TestCase(139999999995L, 10000000000L, 13L, 14L)]
        [TestCase(20000000005L, 10000000000L, 2L, 3L)]
        [TestCase(-1L, 3L, -1L, 0L)]
        [TestCase(-4L, 3L, -2L, -1L)]
        [TestCase(-6L, 3L, -2L, -2L)]
        [TestCase(0L, 3L, 0L, 0L)]
        public void Rounding_UsesMathematicalFloorAndCeil(long n, long d, long floor, long ceil)
        {
            var value = Value(n, d);
            Assert.AreEqual(new BigInteger(floor), value.Floor(budget));
            Assert.AreEqual(new BigInteger(ceil), value.Ceil(budget));
        }

        [Test]
        public void DesignWitness_DamageAndRewardsUseExactIntegerExits()
        {
            Assert.AreEqual(new BigInteger(23), Value(4720, 203).Floor(budget));
            var multiplier = Value(11, 5);
            var experience = Value(14, 1).Multiply(multiplier, budget).Floor(budget);
            var material = Value(2, 1).Multiply(multiplier, budget).Ceil(budget);
            Assert.AreEqual(new BigInteger(30), experience);
            Assert.AreEqual(new BigInteger(16), experience - 14);
            Assert.AreEqual(new BigInteger(5), material);
            Assert.AreEqual(new BigInteger(3), material - 2);
        }

        [Test]
        public void DesignWitness_ShieldAndOverkillConserveExactShares()
        {
            var poison = Value(4, 1);
            var burn = Value(6, 1);
            var total = poison.Add(burn, budget);
            var shield = Value(3, 1);
            var poisonShield = shield.Multiply(poison, budget).Divide(total, budget);
            var burnShield = shield.Multiply(burn, budget).Divide(total, budget);
            AssertValue(6, 5, poisonShield);
            AssertValue(9, 5, burnShield);
            AssertValue(3, 1, poisonShield.Add(burnShield, budget));

            var poisonDamage = poison.Subtract(poisonShield, budget);
            var burnDamage = burn.Subtract(burnShield, budget);
            AssertValue(14, 5, poisonDamage);
            AssertValue(21, 5, burnDamage);
            var damage = poisonDamage.Add(burnDamage, budget);
            var hp = Value(5, 1);
            var poisonContribution = hp.Multiply(poisonDamage, budget).Divide(damage, budget);
            var burnContribution = hp.Multiply(burnDamage, budget).Divide(damage, budget);
            AssertValue(2, 1, poisonContribution);
            AssertValue(3, 1, burnContribution);
            AssertValue(5, 1, poisonContribution.Add(burnContribution, budget));
            AssertValue(4, 5, poisonDamage.Subtract(poisonContribution, budget));
            AssertValue(6, 5, burnDamage.Subtract(burnContribution, budget));
        }

        [TestCase(1)]
        [TestCase(7)]
        [TestCase(8)]
        [TestCase(9)]
        [TestCase(32768)]
        public void IntegerLimit_AcceptsExactMagnitudeBoundaryAndRejectsNextBit(int bits)
        {
            budget = new ExactMathBudget(bits);
            var outside = BigInteger.One << bits;
            var inside = outside - 1;
            Assert.AreEqual(inside, ExactRational.Create(inside, 1, budget).Numerator);
            Assert.AreEqual(-inside, ExactRational.Create(-inside, 1, budget).Numerator);
            AssertLimit("IntegerBits", (long)bits + 1, bits,
                () => ExactRational.Create(outside, 1, budget));
            AssertLimit("IntegerBits", (long)bits + 1, bits,
                () => ExactRational.Create(-outside, 1, budget));
        }

        [Test]
        public void IntegerLimit_ChecksInputsBeforeReductionOrZeroShortcut()
        {
            budget = new ExactMathBudget(8);
            AssertLimit("IntegerBits", 9, 8, () => Value(256, 256));
            AssertLimit("IntegerBits", 9, 8, () => Value(0, 256));
            AssertLimit("IntegerBits", 9, 8,
                () => ExactRational.Create(BigInteger.One << 1024, 1, budget));
        }

        [Test]
        public void IntegerLimit_RevalidatesValuesInEverySmallerBudgetEntry()
        {
            var large = Value(256, 1);
            var small = Value(1, 1);
            var smallBudget = new ExactMathBudget(8);
            TestDelegate[] operations =
            {
                () => large.Add(small, smallBudget), () => small.Subtract(large, smallBudget),
                () => Value(0, 1).Multiply(large, smallBudget), () => large.Divide(small, smallBudget),
                () => small.Compare(large, smallBudget), () => large.Floor(smallBudget),
                () => large.Ceil(smallBudget)
            };
            foreach (var operation in operations)
                AssertLimit("IntegerBits", 9, 8, operation);
            AssertValue(256, 1, large);
        }

        [TestCase(1)]
        [TestCase(-1)]
        public void IntegerLimit_AdditionAndMultiplicationRejectBeforeReturningOversizedResult(int sign)
        {
            budget = new ExactMathBudget(8);
            var value = Value(128 * sign, 1);
            AssertLimit("IntegerBits", 9, 8, () => value.Add(value, budget));
            AssertLimit("IntegerBits", 9, 8, () => Value(16 * sign, 1).Multiply(Value(16, 1), budget));
            AssertValue(128 * sign, 1, value.Multiply(Value(1, 1), budget));
            AssertValue(128 * sign, 1, Value(2 * sign, 1).Multiply(Value(64, 1), budget));
            AssertValue(128 * sign, 1, value);
            AssertValue(256 * sign, 1, value.Add(value, new ExactMathBudget(9)));
        }

        [Test]
        public void IntegerLimit_DenominatorAndComparisonIntermediatesAlsoCount()
        {
            budget = new ExactMathBudget(8);
            AssertLimit("IntegerBits", 9, 8, () => Value(1, 127).Add(Value(1, 128), budget));
            AssertLimit("IntegerBits", 9, 8, () => Value(128, 129).Compare(Value(127, 129), budget));
        }

        [Test]
        public void StepLimit_IsSharedAcrossOperationsAndFailureDoesNotResetIt()
        {
            var first = Value(1, 3);
            var second = Value(1, 6);
            var measurement = new ExactMathBudget();
            first.Add(second, measurement);
            var steps = (int)measurement.PrimitiveStepsUsed;
            Assert.Greater(steps, 0);
            var shared = new ExactMathBudget(32768, steps * 2);
            AssertValue(1, 2, first.Add(second, shared));
            Assert.AreEqual(steps, shared.PrimitiveStepsUsed);
            AssertValue(1, 2, first.Add(second, shared));
            Assert.AreEqual(steps * 2, shared.PrimitiveStepsUsed);
            AssertLimit("PrimitiveSteps", steps * 2 + 1, steps * 2, () => first.Add(second, shared));
            AssertLimit("PrimitiveSteps", steps * 2 + 1, steps * 2, () => first.Floor(shared));
            Assert.AreEqual(steps * 2, shared.PrimitiveStepsUsed);
            AssertValue(1, 3, first);
            AssertValue(1, 6, second);
            AssertValue(1, 2, first.Add(second, new ExactMathBudget()));
        }

        [Test]
        public void StepLimit_ZeroBudgetDoesNotPerformFirstOperation()
        {
            var empty = new ExactMathBudget(32768, 0);
            AssertLimit("PrimitiveSteps", 1, 0, () => ExactRational.Create(0, 1, empty));
            Assert.AreEqual(0, empty.PrimitiveStepsUsed);
        }

        [Test]
        public void StepLimit_ReservesQuotientAndRemainderTogetherBeforeRounding()
        {
            var value = Value(1, 3);
            var insufficient = new ExactMathBudget(32768, 3);
            AssertLimit("PrimitiveSteps", 4, 3, () => value.Floor(insufficient));
            Assert.AreEqual(2, insufficient.PrimitiveStepsUsed);
            var sufficient = new ExactMathBudget(32768, 4);
            Assert.AreEqual(BigInteger.Zero, value.Floor(sufficient));
            Assert.AreEqual(4, sufficient.PrimitiveStepsUsed);
        }

        [Test]
        public void InvalidInputs_RejectZeroDenominatorsDivisorsAndInvalidBudgets()
        {
            Assert.Throws<DivideByZeroException>(() => Value(1, 0));
            Assert.Throws<DivideByZeroException>(() => Value(0, 0));
            Assert.Throws<DivideByZeroException>(() => Value(1, 2).Divide(Value(0, 1), budget));
            Assert.Throws<DivideByZeroException>(() => Value(0, 1).Divide(Value(0, 1), budget));
            Assert.Throws<ArgumentOutOfRangeException>(() => new ExactMathBudget(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new ExactMathBudget(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new ExactMathBudget(8, -1));
        }

        [Test]
        public void InvalidInputs_RejectNullArguments()
        {
            var value = Value(1, 2);
            TestDelegate[] operations =
            {
                () => ExactRational.Create(1, 2, null), () => value.Add(value, null),
                () => value.Subtract(value, null), () => value.Multiply(value, null),
                () => value.Divide(value, null), () => value.Compare(value, null),
                () => value.Floor(null), () => value.Ceil(null), () => value.Add(null, budget),
                () => value.Subtract(null, budget), () => value.Multiply(null, budget),
                () => value.Divide(null, budget), () => value.Compare(null, budget)
            };
            foreach (var operation in operations)
                Assert.Throws<ArgumentNullException>(operation);
        }

        private ExactRational Value(long numerator, long denominator)
        {
            return ExactRational.Create(numerator, denominator, budget);
        }

        private static void AssertValue(long numerator, long denominator, ExactRational actual)
        {
            Assert.AreEqual(new BigInteger(numerator), actual.Numerator);
            Assert.AreEqual(new BigInteger(denominator), actual.Denominator);
        }

        private static void AssertLimit(string reason, long required, long allowed, TestDelegate action)
        {
            var error = Assert.Throws<ExactMathLimitException>(action);
            Assert.AreEqual(reason, error.ReasonCode);
            Assert.AreEqual(required, error.RequiredAtLeast);
            Assert.AreEqual(allowed, error.Allowed);
        }
    }
}
