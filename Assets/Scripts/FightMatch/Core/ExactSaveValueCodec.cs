using System;
using System.Globalization;
using System.Numerics;

namespace FightMatch.Core
{
    public static class ExactSaveValueCodec
    {
        public static SaveCodecResult<string> EncodeInteger(BigInteger value, SaveCodecBudget budget)
        {
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            return SaveCodecResult<string>.Run(() => IntegerToken(value, budget));
        }

        public static SaveCodecResult<BigInteger?> DecodeInteger(string token, SaveCodecBudget budget)
        {
            if (token == null) throw new ArgumentNullException(nameof(token));
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            return SaveCodecResult<BigInteger?>.Run(() => IntegerValue(token, budget, "Integer"));
        }

        public static SaveCodecResult<string> EncodeRational(ExactRational value, SaveCodecBudget budget)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            return SaveCodecResult<string>.Run(() =>
            {
                var numerator = IntegerToken(value.Numerator, budget);
                var denominator = IntegerToken(value.Denominator, budget);
                SaveCodecFailure.Limit((ulong)numerator.Length + 1 + (ulong)denominator.Length,
                    (ulong)budget.MaxNumericTokenBytes, "Rational", "NumericTokenBytes");
                return numerator + "/" + denominator;
            });
        }

        public static SaveCodecResult<ExactRational> DecodeRational(string token, SaveCodecBudget budget)
        {
            if (token == null) throw new ArgumentNullException(nameof(token));
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            return SaveCodecResult<ExactRational>.Run(() =>
            {
                TokenLimit(token, budget, "Rational");
                var slash = token.IndexOf('/');
                SaveCodecFailure.Require(slash > 0 && slash == token.LastIndexOf('/') && slash < token.Length - 1,
                    "Malformed", "Rational");
                var n = IntegerValue(token.Substring(0, slash), budget, "Rational.Numerator");
                var d = IntegerValue(token.Substring(slash + 1), budget, "Rational.Denominator");
                SaveCodecFailure.Require(d.Sign > 0, "Malformed", "Rational.Denominator");
                var value = ExactRational.Create(n, d, budget.Math);
                SaveCodecFailure.Require(value.Numerator == n && value.Denominator == d, "Malformed", "Rational");
                return value;
            });
        }

        internal static string IntegerToken(BigInteger value, SaveCodecBudget budget, Action<int> beforeFormat = null)
        {
            budget.Math.CheckInteger(value);
            // Count digits before formatting, so a smaller token budget does not allocate a large string.
            var rest = value;
            var count = value.Sign < 0 ? 1 : 0;
            do
            {
                count++;
                SaveCodecFailure.Limit((ulong)count, (ulong)budget.MaxNumericTokenBytes, "Integer", "NumericTokenBytes");
                rest = budget.Math.Divide(rest, 10);
            } while (!rest.IsZero);
            beforeFormat?.Invoke(count);
            return value.ToString(CultureInfo.InvariantCulture);
        }

        internal static BigInteger IntegerValue(string token, SaveCodecBudget budget, string path)
        {
            TokenLimit(token, budget, path);
            SaveCodecFailure.Require(token.Length > 0, "Malformed", path);
            var negative = token[0] == '-';
            var first = negative ? 1 : 0;
            SaveCodecFailure.Require(first < token.Length, "Malformed", path);
            if (token[first] == '0')
                SaveCodecFailure.Require(!negative && token.Length == 1, "Malformed", path);
            else SaveCodecFailure.Require(token[first] >= '1' && token[first] <= '9', "Malformed", path);
            // Validate all lexical input before any arithmetic, then use bounded primitives for every digit.
            for (var i = first; i < token.Length; i++)
                SaveCodecFailure.Require(token[i] >= '0' && token[i] <= '9', "Malformed", path);
            var value = BigInteger.Zero;
            for (var i = first; i < token.Length; i++)
            {
                var digit = new BigInteger(token[i] - '0');
                budget.Math.CheckInteger(digit);
                value = budget.Math.Add(budget.Math.Multiply(value, 10), digit);
            }
            budget.Math.CheckInteger(value);
            return negative ? budget.Math.Negate(value) : value;
        }

        private static void TokenLimit(string token, SaveCodecBudget budget, string path)
        { SaveCodecFailure.Limit((ulong)token.Length, (ulong)budget.MaxNumericTokenBytes, path, "NumericTokenBytes"); }
    }
}
