using System;
using System.Numerics;

namespace FightMatch.Core
{
    // Caller-owned and shared across one evaluation. This is not a live-memory budget.
    public sealed class ExactMathBudget
    {
        private readonly BigInteger maxMagnitude;
        private readonly BigInteger minMagnitude;

        public int MaxIntegerBits { get; }
        public int MaxPrimitiveSteps { get; }
        public long PrimitiveStepsUsed { get; private set; }

        public ExactMathBudget(int maxIntegerBits = 32768, int maxPrimitiveSteps = 1048576)
        {
            if (maxIntegerBits <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxIntegerBits));
            if (maxPrimitiveSteps < 0)
                throw new ArgumentOutOfRangeException(nameof(maxPrimitiveSteps));

            MaxIntegerBits = maxIntegerBits;
            MaxPrimitiveSteps = maxPrimitiveSteps;

            // Build the limit without first allocating an out-of-limit shifted integer.
            var magnitudeBytes = (maxIntegerBits - 1) / 8 + 1;
            var bytes = new byte[magnitudeBytes + 1];
            for (var i = 0; i < magnitudeBytes - 1; i++)
                bytes[i] = 255;
            bytes[magnitudeBytes - 1] = (byte)((1 << ((maxIntegerBits - 1) % 8 + 1)) - 1);
            maxMagnitude = new BigInteger(bytes);
            minMagnitude = -maxMagnitude;
        }

        internal void CheckInteger(BigInteger value)
        {
            if (value.Sign < 0 ? Compare(value, minMagnitude) < 0 : Compare(value, maxMagnitude) > 0)
                ThrowIntegerLimit();
        }

        internal BigInteger Add(BigInteger left, BigInteger right)
        {
            if (left.Sign != 0 && left.Sign == right.Sign)
            {
                TakeStep();
                var available = (left.Sign > 0 ? maxMagnitude : minMagnitude) - right;
                var comparison = Compare(left, available);
                if (left.Sign > 0 ? comparison > 0 : comparison < 0)
                    ThrowIntegerLimit();
            }

            TakeStep();
            return left + right;
        }

        internal BigInteger Subtract(BigInteger left, BigInteger right)
        {
            return Add(left, Negate(right));
        }

        internal BigInteger Multiply(BigInteger left, BigInteger right)
        {
            if (!left.IsZero && !right.IsZero)
            {
                var leftMagnitude = Abs(left);
                var rightMagnitude = Abs(right);
                var available = Divide(maxMagnitude, rightMagnitude);
                if (Compare(leftMagnitude, available) > 0)
                    ThrowIntegerLimit();
            }

            TakeStep();
            return left * right;
        }

        internal BigInteger Divide(BigInteger left, BigInteger right)
        {
            TakeStep();
            return left / right;
        }

        internal BigInteger Remainder(BigInteger left, BigInteger right)
        {
            TakeStep();
            return left % right;
        }

        internal bool IsPowerOfTwo(BigInteger value)
        {
            if (value.Sign <= 0)
                return false;
            TakeStep(2);
            return (value & (value - BigInteger.One)).IsZero;
        }

        internal BigInteger ShiftLeft(BigInteger value, int bits)
        {
            if (bits < 0)
                throw new ArgumentOutOfRangeException(nameof(bits));
            TakeStep();
            var available = maxMagnitude >> bits;
            if (Compare(Abs(value), available) > 0)
                ThrowIntegerLimit();
            TakeStep();
            return value << bits;
        }

        internal void ChargePcgWord(ulong state)
        {
            // Six fixed operations, plus four for a nonzero output rotation.
            TakeStep(state < (1UL << 59) ? 6 : 10);
        }

        internal BigInteger DivRem(BigInteger left, BigInteger right, out BigInteger remainder)
        {
            TakeStep(2);
            return BigInteger.DivRem(left, right, out remainder);
        }

        internal BigInteger Negate(BigInteger value)
        {
            TakeStep();
            return -value;
        }

        internal int Compare(BigInteger left, BigInteger right)
        {
            TakeStep();
            return BigInteger.Compare(left, right);
        }

        internal BigInteger Gcd(BigInteger left, BigInteger right)
        {
            left = Abs(left);
            right = Abs(right);
            while (!right.IsZero)
            {
                TakeStep();
                var remainder = left % right;
                left = right;
                right = remainder;
            }
            return left;
        }

        private BigInteger Abs(BigInteger value)
        {
            return value.Sign < 0 ? Negate(value) : value;
        }

        private void TakeStep(int count = 1)
        {
            var required = PrimitiveStepsUsed + count;
            if (required > MaxPrimitiveSteps)
                throw new ExactMathLimitException("PrimitiveSteps", required, MaxPrimitiveSteps);
            PrimitiveStepsUsed = required;
        }

        private void ThrowIntegerLimit()
        {
            throw new ExactMathLimitException("IntegerBits", (long)MaxIntegerBits + 1, MaxIntegerBits);
        }
    }
}
