using System;
using System.Collections.Generic;
using System.Numerics;
using FightMatch.Core;
using static FightMatch.Content.ContentChecks;

namespace FightMatch.Content
{
    // A proof for supplied rationals; candidate searching belongs to the making/test side.
    public static class DemoContentParameters
    {
        public static DemoParameterResult EvaluateParameterEvidence(ExactRational p, ExactRational c,
            ExactRational epsilon, DemoContentJob job, ExactMathBudget budget)
        {
            try
            {
                Root(job, "Job"); job.Check(); Root(budget, "Budget");
                Root(p, "Target"); Root(c, "C"); Root(epsilon, "Epsilon");
                var zero = ExactRational.Create(0, 1, budget); var one = ExactRational.Create(1, 1, budget);
                Need(p.Compare(zero, budget) > 0 && p.Compare(one, budget) <= 0, "InvalidValue", "Target");
                Need(c.Compare(zero, budget) > 0 && c.Compare(one, budget) <= 0, "InvalidValue", "C");
                Need(epsilon.Compare(zero, budget) >= 0, "InvalidValue", "Epsilon");
                var n = one.Divide(c, budget).Ceil(budget);
                // Every iteration costs at least one primitive. Reject enormous N before looping or allocating.
                Need(n <= budget.MaxPrimitiveSteps - budget.PrimitiveStepsUsed && n <= int.MaxValue, "BudgetExceeded", "N");
                var survival = one; var wait = zero;
                for (var k = 0; k < (int)n; k++)
                {
                    job.Check(); wait = wait.Add(survival, budget);
                    var q = Chance(k + 1, c, one, budget);
                    survival = survival.Multiply(one.Subtract(q, budget), budget);
                }
                var rate = one.Divide(wait, budget); var error = rate.Subtract(p, budget);
                var absolute = error.Numerator.Sign < 0 ? zero.Subtract(error, budget) : error;
                var within = absolute.Compare(epsilon, budget) <= 0;
                var shortTerm = new List<DemoShortTermEvidence>();
                // D[f] is mass at failure count f after each opportunity, with successes resetting to zero.
                var distribution = new[] { one }; var total = zero; var firstSurvival = one;
                for (var step = 1; step <= 10; step++)
                {
                    job.Check(); var next = new ExactRational[step + 1];
                    for (var f = 0; f < next.Length; f++) next[f] = zero;
                    for (var f = 0; f < distribution.Length; f++)
                    {
                        var q = Chance(f + 1, c, one, budget); var success = distribution[f].Multiply(q, budget);
                        next[0] = next[0].Add(success, budget); total = total.Add(success, budget);
                        next[f + 1] = next[f + 1].Add(distribution[f].Multiply(one.Subtract(q, budget), budget), budget);
                    }
                    distribution = next;
                    firstSurvival = firstSurvival.Multiply(one.Subtract(Chance(step, c, one, budget), budget), budget);
                    if (step == 2 || step == 3 || step == 10)
                        shortTerm.Add(new DemoShortTermEvidence(step, one.Subtract(firstSurvival, budget),
                            total.Divide(ExactRational.Create(step, 1, budget), budget)));
                }
                job.Check();
                return new DemoParameterResult(new DemoParameterEvidence(p, c, epsilon, n, wait, rate, error, within, shortTerm));
            }
            catch (ContentFailure ex) { return new DemoParameterResult(ex.Code, ex.Path); }
            catch (ExactMathLimitException) { return new DemoParameterResult("BudgetExceeded", "ExactMathBudget"); }
        }
        private static ExactRational Chance(int opportunity, ExactRational c, ExactRational one, ExactMathBudget budget)
        { var value = c.Multiply(ExactRational.Create(opportunity, 1, budget), budget); return value.Compare(one, budget) < 0 ? value : one; }
    }
}
