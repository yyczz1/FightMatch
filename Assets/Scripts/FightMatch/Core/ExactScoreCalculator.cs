using System;
using System.Numerics;

namespace FightMatch.Core
{
    public static class ExactScoreCalculator
    {
        public static ExactScoreResult Evaluate(BigInteger baseExperience, ExactRational levelMultiplier,
            ExactRational baseCoefficient, ExactRational logCoefficient, ExactRational contribution,
            ExactRational reference, ExactEvaluationScope scope)
        {
            if (scope == null)
                throw new ArgumentNullException(nameof(scope));
            var mark = scope.Mark();
            try
            {
                if (baseExperience.Sign < 0)
                    throw new ArgumentOutOfRangeException(nameof(baseExperience));
                scope.KeepInteger(baseExperience);
                Validate(levelMultiplier, nameof(levelMultiplier), false, scope);
                Validate(baseCoefficient, nameof(baseCoefficient), false, scope);
                Validate(logCoefficient, nameof(logCoefficient), false, scope);
                Validate(contribution, nameof(contribution), false, scope);
                Validate(reference, nameof(reference), true, scope);

                var eg = scope.Multiply(scope.Create(baseExperience, 1), levelMultiplier);
                var a = scope.Multiply(eg, baseCoefficient);
                var b = scope.Multiply(eg, logCoefficient);
                ExactScoreResult result;
                if (b.Numerator.IsZero)
                {
                    result = new ExactScoreResult(scope.Floor(a), a, a, 0);
                }
                else
                {
                    var x = scope.Add(scope.Create(1, 1), scope.Divide(contribution, reference));
                    result = EvaluateLog(a, b, x, scope);
                }
                scope.KeepSince(mark, result.LowerBound, result.UpperBound);
                scope.KeepInteger(result.Amount);
                return result;
            }
            catch
            {
                scope.Rewind(mark);
                throw;
            }
        }

        private static void Validate(ExactRational value, string parameterName, bool positive,
            ExactEvaluationScope scope)
        {
            if (value == null)
                throw new ArgumentNullException(parameterName);
            if (value.Numerator.Sign < 0 || (positive && value.Numerator.IsZero))
                throw new ArgumentOutOfRangeException(parameterName);
            scope.Keep(value);
        }

        private static ExactScoreResult EvaluateLog(ExactRational a, ExactRational b, ExactRational x,
            ExactEvaluationScope scope)
        {
            var exponent = ExactEvaluationScope.MagnitudeBits(x.Numerator) -
                ExactEvaluationScope.MagnitudeBits(x.Denominator);
            if (scope.IsPowerOfTwo(x.Numerator) && scope.IsPowerOfTwo(x.Denominator))
            {
                var exact = scope.Add(a, scope.Multiply(b, scope.Create(exponent, 1)));
                return new ExactScoreResult(scope.Floor(exact), exact, exact, 0);
            }

            var scaledDenominator = scope.ShiftLeft(x.Denominator, exponent);
            if (scope.Compare(x.Numerator, scaledDenominator) < 0)
                exponent--;
            var one = scope.Create(1, 1);
            var two = scope.Create(2, 1);
            var powerOfTwo = scope.Create(scope.ShiftLeft(BigInteger.One, exponent), 1);
            var y = scope.Divide(x, powerOfTwo);
            var t = scope.Divide(scope.Subtract(y, one), scope.Add(y, one));
            var logY = new LogSeries(t, one, scope);
            var logTwo = new LogSeries(scope.Create(1, 3), one, scope);
            var integerPart = scope.Create(exponent, 1);
            var iterationMark = scope.Mark();

            for (var n = 1; ; n++)
            {
                scope.Budget.TakeLogPair();
                var yBounds = logY.Advance(n, two, scope);
                var twoBounds = logTwo.Advance(n, two, scope);
                var lowerLog = scope.Add(integerPart, scope.Divide(yBounds.Lower, twoBounds.Upper));
                var upperLog = scope.Add(integerPart, scope.Divide(yBounds.Upper, twoBounds.Lower));
                var lower = scope.Add(a, scope.Multiply(b, lowerLog));
                var upper = scope.Add(a, scope.Multiply(b, upperLog));
                var lowerFloor = scope.Floor(lower);
                var upperFloor = scope.Floor(upper);
                if (scope.Compare(lowerFloor, upperFloor) == 0)
                    return new ExactScoreResult(lowerFloor, lower, upper, n);

                scope.KeepSince(iterationMark, logY.Power, logY.Sum, logTwo.Power, logTwo.Sum);
            }
        }

        private sealed class LogSeries
        {
            private readonly ExactRational squared;
            private readonly ExactRational oneMinusSquared;

            internal ExactRational Power { get; private set; }
            internal ExactRational Sum { get; private set; }

            internal LogSeries(ExactRational t, ExactRational one, ExactEvaluationScope scope)
            {
                squared = scope.Multiply(t, t);
                oneMinusSquared = scope.Subtract(one, squared);
                Power = t;
                Sum = scope.Create(0, 1);
            }

            internal Bounds Advance(int n, ExactRational two, ExactEvaluationScope scope)
            {
                var denominator = scope.Create(2 * n - 1, 1);
                Sum = scope.Add(Sum, scope.Divide(Power, denominator));
                Power = scope.Multiply(Power, squared);
                var lower = scope.Multiply(two, Sum);
                var tailDenominator = scope.Multiply(scope.Create(2 * n + 1, 1), oneMinusSquared);
                var upper = scope.Add(lower, scope.Divide(scope.Multiply(two, Power), tailDenominator));
                return new Bounds(lower, upper);
            }
        }

        private readonly struct Bounds
        {
            internal readonly ExactRational Lower;
            internal readonly ExactRational Upper;

            internal Bounds(ExactRational lower, ExactRational upper)
            {
                Lower = lower;
                Upper = upper;
            }
        }
    }
}
