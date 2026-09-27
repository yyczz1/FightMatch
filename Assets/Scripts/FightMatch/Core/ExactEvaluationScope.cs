using System;
using System.Collections.Generic;
using System.Numerics;

namespace FightMatch.Core
{
    public sealed class ExactEvaluationScope : IDisposable
    {
        // Conservative bound on local integer temporaries in the existing rational operations.
        private const int ScratchIntegerSlots = 24;
        private readonly List<RetainedInteger> retained = new List<RetainedInteger>();
        private bool disposed;

        public ExactEvaluationBudget Budget { get; }

        public ExactEvaluationScope(ExactEvaluationBudget budget)
        {
            Budget = budget ?? throw new ArgumentNullException(nameof(budget));
        }

        public void Dispose()
        {
            if (disposed)
                return;
            Rewind(0);
            disposed = true;
        }

        internal int Mark()
        {
            EnsureActive();
            return retained.Count;
        }

        internal void Rewind(int mark)
        {
            for (var i = retained.Count - 1; i >= mark; i--)
            {
                retained[i].Reservation.Dispose();
                retained.RemoveAt(i);
            }
        }

        internal void KeepSince(int mark, params ExactRational[] values)
        {
            Rewind(mark);
            foreach (var value in values)
                Keep(value);
        }

        internal ExactRational Keep(ExactRational value)
        {
            KeepInteger(value.Numerator);
            KeepInteger(value.Denominator);
            return value;
        }

        internal BigInteger KeepInteger(BigInteger value)
        {
            EnsureActive();
            Budget.Math.CheckInteger(value);
            var reservation = Budget.Reserve(MagnitudeBits(value));
            try
            {
                retained.Add(new RetainedInteger(value, reservation));
            }
            catch
            {
                reservation.Dispose();
                throw;
            }
            return value;
        }

        internal ExactRational Create(BigInteger numerator, BigInteger denominator)
        {
            return Calculate(() => ExactRational.Create(numerator, denominator, Budget.Math));
        }

        internal ExactRational Add(ExactRational left, ExactRational right)
        {
            return Calculate(() => left.Add(right, Budget.Math));
        }

        internal ExactRational Subtract(ExactRational left, ExactRational right)
        {
            return Calculate(() => left.Subtract(right, Budget.Math));
        }

        internal ExactRational Multiply(ExactRational left, ExactRational right)
        {
            return Calculate(() => left.Multiply(right, Budget.Math));
        }

        internal ExactRational Divide(ExactRational left, ExactRational right)
        {
            return Calculate(() => left.Divide(right, Budget.Math));
        }

        internal BigInteger Floor(ExactRational value)
        {
            BigInteger result;
            using (Scratch())
                result = value.Floor(Budget.Math);
            return KeepInteger(result);
        }

        internal BigInteger ShiftLeft(BigInteger value, int bits)
        {
            BigInteger result;
            using (Scratch())
                result = Budget.Math.ShiftLeft(value, bits);
            return KeepInteger(result);
        }

        internal int Compare(BigInteger left, BigInteger right)
        {
            using (Scratch())
                return Budget.Math.Compare(left, right);
        }

        internal bool IsPowerOfTwo(BigInteger value)
        {
            using (Scratch())
                return Budget.Math.IsPowerOfTwo(value);
        }

        internal static int MagnitudeBits(BigInteger value)
        {
            // Accounting metadata; not a floating-point logarithm or a gameplay conversion.
            var bytes = BigInteger.Abs(value).ToByteArray();
            var count = bytes.Length;
            while (count > 1 && bytes[count - 1] == 0)
                count--;
            var top = bytes[count - 1];
            var bits = 0;
            while (top != 0)
            {
                bits++;
                top >>= 1;
            }
            return Math.Max(1, (count - 1) * 8 + bits);
        }

        private ExactRational Calculate(Func<ExactRational> operation)
        {
            ExactRational result;
            using (Scratch())
                result = operation();
            return Keep(result);
        }

        private IDisposable Scratch()
        {
            EnsureActive();
            return Budget.Reserve((long)ScratchIntegerSlots * Budget.Math.MaxIntegerBits);
        }

        private void EnsureActive()
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(ExactEvaluationScope));
        }

        private readonly struct RetainedInteger
        {
            // Keep the integer alive for the same duration as its accounting reservation.
            public readonly BigInteger Value;
            public readonly IDisposable Reservation;

            public RetainedInteger(BigInteger value, IDisposable reservation)
            {
                Value = value;
                Reservation = reservation;
            }
        }
    }
}
