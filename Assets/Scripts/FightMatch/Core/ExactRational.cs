using System;
using System.Numerics;

namespace FightMatch.Core
{
    public sealed class ExactRational
    {
        public BigInteger Numerator { get; }
        public BigInteger Denominator { get; }

        private ExactRational(BigInteger numerator, BigInteger denominator)
        {
            Numerator = numerator;
            Denominator = denominator;
        }

        public static ExactRational Create(BigInteger numerator, BigInteger denominator,
            ExactMathBudget budget)
        {
            if (budget == null)
                throw new ArgumentNullException(nameof(budget));
            budget.CheckInteger(numerator);
            budget.CheckInteger(denominator);
            if (denominator.IsZero)
                throw new DivideByZeroException();
            if (denominator.Sign < 0)
            {
                numerator = budget.Negate(numerator);
                denominator = budget.Negate(denominator);
            }
            if (numerator.IsZero)
                return new ExactRational(BigInteger.Zero, BigInteger.One);

            var gcd = budget.Gcd(numerator, denominator);
            return new ExactRational(budget.Divide(numerator, gcd), budget.Divide(denominator, gcd));
        }

        public ExactRational Add(ExactRational other, ExactMathBudget budget)
        {
            return AddOrSubtract(other, budget, false);
        }

        public ExactRational Subtract(ExactRational other, ExactMathBudget budget)
        {
            return AddOrSubtract(other, budget, true);
        }

        public ExactRational Multiply(ExactRational other, ExactMathBudget budget)
        {
            ValidateBoth(other, budget);
            if (Numerator.IsZero || other.Numerator.IsZero)
                return new ExactRational(BigInteger.Zero, BigInteger.One);

            var firstGcd = budget.Gcd(Numerator, other.Denominator);
            var secondGcd = budget.Gcd(other.Numerator, Denominator);
            var numerator = budget.Multiply(budget.Divide(Numerator, firstGcd),
                budget.Divide(other.Numerator, secondGcd));
            var denominator = budget.Multiply(budget.Divide(Denominator, secondGcd),
                budget.Divide(other.Denominator, firstGcd));
            return new ExactRational(numerator, denominator);
        }

        public ExactRational Divide(ExactRational other, ExactMathBudget budget)
        {
            ValidateBoth(other, budget);
            if (other.Numerator.IsZero)
                throw new DivideByZeroException();
            if (Numerator.IsZero)
                return new ExactRational(BigInteger.Zero, BigInteger.One);

            var firstGcd = budget.Gcd(Numerator, other.Numerator);
            var secondGcd = budget.Gcd(Denominator, other.Denominator);
            var numerator = budget.Multiply(budget.Divide(Numerator, firstGcd),
                budget.Divide(other.Denominator, secondGcd));
            var denominator = budget.Multiply(budget.Divide(Denominator, secondGcd),
                budget.Divide(other.Numerator, firstGcd));
            if (denominator.Sign < 0)
            {
                numerator = budget.Negate(numerator);
                denominator = budget.Negate(denominator);
            }
            return new ExactRational(numerator, denominator);
        }

        public int Compare(ExactRational other, ExactMathBudget budget)
        {
            ValidateBoth(other, budget);
            if (Numerator.Sign != other.Numerator.Sign || Numerator.IsZero)
                return budget.Compare(Numerator, other.Numerator);

            var left = budget.Multiply(Numerator, other.Denominator);
            var right = budget.Multiply(other.Numerator, Denominator);
            return budget.Compare(left, right);
        }

        public BigInteger Floor(ExactMathBudget budget)
        {
            Validate(budget);
            var quotient = budget.DivRem(Numerator, Denominator, out var remainder);
            return remainder.Sign < 0 ? budget.Subtract(quotient, BigInteger.One) : quotient;
        }

        public BigInteger Ceil(ExactMathBudget budget)
        {
            Validate(budget);
            var quotient = budget.DivRem(Numerator, Denominator, out var remainder);
            return remainder.Sign > 0 ? budget.Add(quotient, BigInteger.One) : quotient;
        }

        private ExactRational AddOrSubtract(ExactRational other, ExactMathBudget budget, bool subtract)
        {
            ValidateBoth(other, budget);
            var gcd = budget.Gcd(Denominator, other.Denominator);
            var leftFactor = budget.Divide(other.Denominator, gcd);
            var rightFactor = budget.Divide(Denominator, gcd);
            var left = budget.Multiply(Numerator, leftFactor);
            var right = budget.Multiply(other.Numerator, rightFactor);
            var numerator = subtract ? budget.Subtract(left, right) : budget.Add(left, right);
            var denominator = budget.Multiply(Denominator, leftFactor);
            return Create(numerator, denominator, budget);
        }

        private void Validate(ExactMathBudget budget)
        {
            if (budget == null)
                throw new ArgumentNullException(nameof(budget));
            budget.CheckInteger(Numerator);
            budget.CheckInteger(Denominator);
        }

        private void ValidateBoth(ExactRational other, ExactMathBudget budget)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));
            Validate(budget);
            other.Validate(budget);
        }
    }
}
