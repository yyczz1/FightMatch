using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using FightMatch.Content;
using NUnit.Framework;
using static FightMatch.Core.Tests.DemoContentTestData;

namespace FightMatch.Core.Tests
{
    public class DemoContentParameterTests
    {
        [TestCase(1, 1, 1, 1, 1, 1)]
        [TestCase(1, 2, 3, 2, 2, 3)]
        [TestCase(1, 4, 71, 32, 32, 71)]
        public void KnownExactWaitingTimeAndRate(int cn, int cd, int en, int ed, int rn, int rd)
        {
            var result = DemoContentParameters.EvaluateParameterEvidence(R(rn, rd), R(cn, cd), R(0), Job(), Math());
            Assert.IsTrue(result.IsAccepted); Same(result.Evidence.ExpectedWait, R(en, ed)); Same(result.Evidence.ModelRate, R(rn, rd));
            Same(result.Evidence.SignedError, R(0)); Assert.IsTrue(result.Evidence.WithinProposedTolerance);
            Same(result.Evidence.FirstOpportunityRate, R(cn, cd)); Assert.AreEqual(cd, (int)result.Evidence.N);
        }
        [Test]
        public void ResetModelCountsAllSuccessesRatherThanOnlyFirst()
        {
            var result = Proof(R(2, 3), R(1, 2), R(0));
            Same(result.ShortTerm[0].AtLeastOne, R(1)); Same(result.ShortTerm[0].ExpectedSuccessesPerOpportunity, R(5, 8));
            Same(result.ShortTerm[1].ExpectedSuccessesPerOpportunity, R(5, 8));
        }
        [Test]
        public void ZeroToleranceDoesNotPretendDecimalEqualsIrrationalRoot()
        {
            var proof = Proof(R(3, 5), R(422649730810, Grid), R(0));
            Assert.IsFalse(proof.WithinProposedTolerance); Assert.AreNotEqual(BigInteger.Zero, proof.SignedError.Numerator);
        }
        private static IEnumerable<int> Indices() { return Enumerable.Range(0, 31); }
        [TestCaseSource(nameof(Indices))]
        public void AllThirtyOneAdjacentBracketsAndResetMetricsRecompute(int index)
        {
            var row = Candidates()[index]; var p = row.Proposed.Target;
            Assert.AreEqual(BigInteger.One, row.UpperGridNumerator - row.LowerGridNumerator);
            Same(p, R(40 + index, 200));
            Same(row.LowerRate, ModelRate(R(row.LowerGridNumerator, Grid)));
            Same(row.UpperRate, ModelRate(R(row.UpperGridNumerator, Grid)));
            Assert.LessOrEqual(row.LowerRate.Compare(p, Math()), 0); Assert.GreaterOrEqual(row.UpperRate.Compare(p, Math()), 0);
            var proof = Proof(p, row.Proposed.C, row.Proposed.Epsilon); Same(proof.ModelRate, ModelRate(row.Proposed.C));
            Assert.IsTrue(proof.WithinProposedTolerance);
            var lowerError = p.Subtract(row.LowerRate, Math()); var upperError = row.UpperRate.Subtract(p, Math());
            Same(row.Proposed.C, R(lowerError.Compare(upperError, Math()) <= 0 ? row.LowerGridNumerator : row.UpperGridNumerator, Grid));
            foreach (var metric in proof.ShortTerm)
            {
                // Independent exhaustive 2^m success/failure paths, all sharing denominator d^m.
                BigInteger weightedSuccesses = 0, any = 0; var m = metric.Opportunities; var c = row.Proposed.C;
                for (var mask = 0; mask < 1 << m; mask++)
                {
                    BigInteger weight = 1; var failures = 0; var successes = 0;
                    for (var step = 0; step < m; step++)
                    {
                        var q = BigInteger.Min(c.Denominator, (failures + 1) * c.Numerator);
                        if ((mask & (1 << step)) != 0) { weight *= q; failures = 0; successes++; }
                        else { weight *= c.Denominator - q; failures++; }
                    }
                    weightedSuccesses += successes * weight; if (successes > 0) any += weight;
                }
                Same(metric.AtLeastOne, R(any, BigInteger.Pow(c.Denominator, m)));
                Same(metric.ExpectedSuccessesPerOpportunity, R(weightedSuccesses, m * BigInteger.Pow(c.Denominator, m)));
            }
        }
        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)] [TestCase(5)] [TestCase(6)] [TestCase(7)]
        public void InvalidOrMissingDomainsReturnNoHalfProof(int kind)
        {
            var p = R(1, 5); var c = R(1, 10); var epsilon = R(0);
            if (kind == 0) p = null; if (kind == 1) c = null; if (kind == 2) epsilon = null;
            if (kind == 3) p = R(0); if (kind == 4) p = R(2); if (kind == 5) c = R(-1); if (kind == 6) c = R(2); if (kind == 7) epsilon = R(-1);
            var r = DemoContentParameters.EvaluateParameterEvidence(p, c, epsilon, Job(), Math());
            Assert.IsFalse(r.IsAccepted); Assert.IsNull(r.Evidence); Assert.AreEqual(kind < 3 ? "MissingField" : "InvalidValue", r.RejectionCode);
        }
        [TestCase(0)] [TestCase(25)] [TestCase(500)]
        public void SharedBudgetExhaustionNeverReturnsPartialMetrics(int steps)
        {
            var result = DemoContentParameters.EvaluateParameterEvidence(R(1, 5), R(1, 100), R(0), Job(), new ExactMathBudget(maxPrimitiveSteps: steps));
            Assert.AreEqual("BudgetExceeded", result.RejectionCode); Assert.IsNull(result.Evidence);
        }
        [Test]
        public void TinyConstantIsRejectedBeforeUnboundedLoop()
        {
            var r = DemoContentParameters.EvaluateParameterEvidence(R(1, 5), R(1, BigInteger.Pow(10, 100)), R(1), Job(), Math());
            Assert.AreEqual("BudgetExceeded", r.RejectionCode); Assert.AreEqual("N", r.FieldPath); Assert.IsNull(r.Evidence);
        }
        [Test]
        public void ExistingBudgetConsumptionIsNotReset()
        {
            var probe = Math(); Assert.IsTrue(DemoContentParameters.EvaluateParameterEvidence(R(1), R(1), R(0), Job(), probe).IsAccepted);
            var budget = new ExactMathBudget(maxPrimitiveSteps: (int)probe.PrimitiveStepsUsed);
            R(1).Add(R(1), budget);
            Assert.AreEqual("BudgetExceeded", DemoContentParameters.EvaluateParameterEvidence(R(1), R(1), R(0), Job(), budget).RejectionCode);
        }
        [Test]
        public void NullJobBudgetAndCancelledJobAreDistinct()
        {
            Assert.AreEqual("MissingField", DemoContentParameters.EvaluateParameterEvidence(R(1), R(1), R(0), null, Math()).RejectionCode);
            Assert.AreEqual("MissingField", DemoContentParameters.EvaluateParameterEvidence(R(1), R(1), R(0), Job(), null).RejectionCode);
            using (var cancellation = new CancellationTokenSource())
            { var job = new DemoContentDraft("cancel").BeginJob(cancellation.Token); cancellation.Cancel();
                Assert.AreEqual("Cancelled", DemoContentParameters.EvaluateParameterEvidence(R(1), R(1), R(0), job, Math()).RejectionCode); }
        }
        [Test]
        public void ExportCandidatesAndBothHistoricalTwentyPercentTexts()
        {
            var shortC = R(557040429, BigInteger.Pow(10, 10)); var longC = R(BigInteger.Parse("5570404294978187"), BigInteger.Pow(10, 17));
            Same(longC.Subtract(shortC, Math()), R(4978187, BigInteger.Pow(10, 17)));
            var oldShort = Proof(R(1, 5), shortC, R(1, 1000000000)); var oldLong = Proof(R(1, 5), longC, R(1, 1000000000));
            Assert.AreEqual(18, (int)oldShort.N); Assert.AreEqual(18, (int)oldLong.N);
            WriteEvidence("prd-candidates.json", new { Status = "unreviewed-proposal-only", Grid, TieRule = "minimum absolute model error, ties lower",
                Epsilon = R(1, 1000000000), LevelAbove31 = "same cap as level 31", OtherProfessionExtraTargets = "10 outside this Demo closure",
                Rows = Candidates().Select(r => new { Bracket = r, Evidence = Proof(r.Proposed.Target, r.Proposed.C, r.Proposed.Epsilon) }).ToArray(),
                OldTwentyPercentShortText = "0.0557040429", OldTwentyPercentLongText = "0.05570404294978187", OldShort = oldShort, OldLong = oldLong,
                Difference = longC.Subtract(shortC, Math()), Meaning = "analytic reset PRD model, not measured PCG frequency or balance approval" });
        }
        private static DemoParameterEvidence Proof(ExactRational p, ExactRational c, ExactRational epsilon)
        { var r = DemoContentParameters.EvaluateParameterEvidence(p, c, epsilon, Job(), Math()); Assert.IsTrue(r.IsAccepted, r.RejectionCode + " " + r.FieldPath); return r.Evidence; }
    }
}
