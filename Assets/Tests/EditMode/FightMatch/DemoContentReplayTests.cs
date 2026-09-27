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
    public class DemoContentReplayTests
    {
        [TestCase(1, DemoCoordinateCandidate.AssumedBottomLeft)] [TestCase(1, DemoCoordinateCandidate.AssumedTopLeft)]
        [TestCase(3, DemoCoordinateCandidate.AssumedBottomLeft)] [TestCase(3, DemoCoordinateCandidate.AssumedTopLeft)]
        public void RealSC01WholeLevelRecordedReplayAnd023Reward(int stage, DemoCoordinateCandidate mapping)
        {
            var c = Prepare(stage, mapping); var r = Replay(c); Accepted(r);
            Assert.AreEqual(BattlePhase.WonPendingSettlement, r.Run.CurrentSnapshot.Phase); Assert.AreEqual(2, r.Run.CurrentSnapshot.Board.LockedRoutes.Count);
            Assert.AreEqual(CandidateBattleOutcome.NormalVictory, r.Report.Outcome); Assert.AreSame(r.Report, r.Reward.Report);
            Assert.AreEqual(c.Fingerprint, r.Binding.Context.ContentFingerprint); Assert.AreEqual(r.ExecutedSteps.Count, r.RecordedReplays.Count);
            Assert.IsTrue(r.RecordedReplays.All(x => x.Matched == true)); Assert.IsFalse(r.CommitEligible); Assert.IsFalse(r.Reward.CommitEligible);
            foreach (var state in r.Binding.Start.Baseline.PrdInitialStates) Assert.AreEqual(BigInteger.Zero, state.FailureCount);
            Assert.AreEqual(BigInteger.Zero, r.Binding.Battle.Initial.WordsConsumed); Assert.AreEqual(BigInteger.Zero, r.Binding.BaseReward.Initial.WordsConsumed);
            Assert.AreEqual(BigInteger.Zero, r.Binding.Bonus.Initial.WordsConsumed); Assert.IsEmpty(r.Reward.Random.Words);
            Assert.AreEqual(CandidateRewardRandomUse.NotUsedFixedTable, r.Reward.Random.Use); Same(r.Reward.Experience[0].Dealt, R(stage == 1 ? 30 : 35));
            Same(r.Reward.Experience[0].Taken, R(r.ExecutedSteps.Count == 3 ? 10 : 5)); Assert.AreEqual(new BigInteger(2), r.Reward.Materials[0].Amount);
            Assert.AreEqual(BigInteger.Zero, r.Reward.Materials[1].Amount); Assert.AreEqual(BigInteger.One, c.Character.Level); Assert.AreEqual(BigInteger.Zero, c.Character.Experience);
            if (stage == 3) Assert.AreEqual(r.ExecutedSteps[0].DirectAttack.DamageFacts[0].Crit.Triggered ? 2 : 3, r.ExecutedSteps.Count);
            var again = Replay(c); Accepted(again); Assert.AreEqual(Json(r), Json(again));
        }
        [Test]
        public void ExportOriginalStatesFactsWordsRejectionGroupsReportsAndRewards()
        {
            var rows = new List<object>();
            foreach (var stage in new[] { 1, 3 }) foreach (var mapping in new[] { DemoCoordinateCandidate.AssumedBottomLeft, DemoCoordinateCandidate.AssumedTopLeft })
            {
                var c = Prepare(stage, mapping); var result = Replay(c); Accepted(result);
                var groups = result.ExecutedSteps.Select(r => Groups(r.DirectAttack.DamageFacts[0].Crit)).ToArray();
                rows.Add(new { Stage = stage, Mapping = mapping, FixedMaterialSelection = "00..2f declared before execution; no seed search; isolated, not player origin",
                    Result = result, RejectionGroups = groups });
            }
            WriteEvidence("replay-evidence.json", new { Status = "actual fixed-source candidate replay; not official approval or all-seeds guarantee", Rows = rows });
        }
        [Test]
        public void PredeterminedSixtyFourSourceCohortCoversDeadTargetSkipAndSurvivorRepeat()
        {
            var c = Prepare(3); var observed = new List<object>(); var crit = 0; var ordinary = 0;
            // Complete fixed matrix, not stop-at-first-success searching or an unconditional win-rate claim.
            for (var firstByte = 0; firstByte < 64; firstByte++)
            {
                var seed = Seed(); seed.Bytes[0] = (byte)firstByte; seed.SourceCapabilityId = "isolated-cohort-first-byte-" + firstByte;
                DemoContentReplayResult r;
                using (var scope = Scope()) r = DemoContentReplay.ReplayCandidate(c, seed, Conditions(), s => Select(c, s), 3, c.Job, scope);
                Accepted(r); var triggered = r.ExecutedSteps[0].DirectAttack.DamageFacts[0].Crit.Triggered;
                Assert.AreEqual(triggered ? 2 : 3, r.ExecutedSteps.Count);
                Assert.AreEqual(triggered ? "B" : "A", r.ExecutedSteps[1].Request.Pair.PairId);
                if (triggered) crit++; else ordinary++;
                observed.Add(new { FirstByte = firstByte, FirstCrit = triggered, Steps = r.ExecutedSteps.Select(x => x.Request.Pair.PairId).ToArray(),
                    ReportFingerprint = r.Report.Fingerprint, BattleWords = r.Run.CurrentSnapshot.Random.Stream.WordsConsumed });
            }
            Assert.Greater(crit, 0); Assert.Greater(ordinary, 0);
            WriteEvidence("replay-cohort.json", new { Meaning = "64 declared isolated inputs for branch coverage; not a frequency estimate", Crit = crit, Ordinary = ordinary, Rows = observed });
        }
        [Test]
        public void IllegalFirstStepConsumesNoWordsAndReportsFirstFailure()
        {
            var c = Prepare();
            using (var scope = Scope())
            {
                var result = DemoContentReplay.ReplayCandidate(c, Seed(), Conditions(), s => { var step = Select(c, s); step.Route.Clear(); return step; }, 3, c.Job, scope);
                Assert.IsFalse(result.IsAccepted); Assert.AreEqual(0, result.FirstFailureStep); Assert.IsEmpty(result.ExecutedSteps);
                Assert.IsNotEmpty(result.RejectionCode); Assert.IsNull(result.Reward); Assert.IsNull(result.Report);
                Assert.AreEqual(BigInteger.Zero, result.Run.CurrentSnapshot.Random.Stream.WordsConsumed);
            }
        }
        [TestCase(false)] [TestCase(true)]
        public void EndTokenRejectsRevisionOrCancellationDuringSelector(bool cancel)
        {
            var draft = new DemoContentDraft("live-job"); using (var cancelled = new CancellationTokenSource())
            {
                var job = draft.BeginJob(cancelled.Token); var prepared = DemoContentCompiler.PrepareCandidate(MakeInput(), job, Math()); Assert.IsTrue(prepared.IsAccepted);
                using (var scope = Scope())
                {
                    var r = DemoContentReplay.ReplayCandidate(prepared.Candidate, Seed(), Conditions(), s =>
                    { var step = Select(prepared.Candidate, s); if (cancel) cancelled.Cancel(); else { draft.Revise(); draft.Revise(); } return step; }, 3, job, scope);
                    Assert.AreEqual(cancel ? "Cancelled" : "StaleContext", r.RejectionCode); Assert.IsNull(r.Reward); Assert.IsEmpty(r.ExecutedSteps);
                    Assert.AreEqual(BigInteger.Zero, r.Run.CurrentSnapshot.Random.Stream.WordsConsumed);
                }
            }
        }
        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)] [TestCase(5)]
        public void MissingBindingsAndBudgetsNeverFabricateVictory(int kind)
        {
            var c = Prepare(); var seed = Seed(); var conditions = Conditions(); var job = c.Job; var max = 3;
            if (kind == 0) seed.Bytes = new byte[47]; if (kind == 1) conditions.ItemUseEnabled = null; if (kind == 2) conditions.ItemUseEnabled = true;
            if (kind == 3) job = Job(); if (kind == 4) max = 1;
            using (var scope = Scope())
            {
                var r = DemoContentReplay.ReplayCandidate(c, seed, conditions, s => Select(c, s), max, job, scope, kind == 5 ? 0 : 4096);
                Assert.IsFalse(r.IsAccepted); Assert.IsNull(r.Reward); Assert.IsNotEmpty(r.RejectionCode);
                if (kind >= 4) Assert.AreEqual("BudgetExceeded", r.RejectionCode);
            }
        }
        [Test]
        public void CallerSeedConditionsAndSubmittedRouteAreNotRetained()
        {
            var c = Prepare(); var seed = Seed(); var conditions = Conditions(); CandidateReplayStep submitted = null;
            DemoContentReplayResult r;
            using (var scope = Scope()) r = DemoContentReplay.ReplayCandidate(c, seed, conditions, s => submitted = Select(c, s), 3, c.Job, scope);
            Accepted(r); var original = Json(r); Array.Clear(seed.Bytes, 0, seed.Bytes.Length); conditions.ItemUseEnabled = true; submitted.Route.Clear();
            Assert.AreEqual(original, Json(r)); Assert.Throws<NotSupportedException>(() => ((IList<byte>)r.SeedBytes).Clear());
        }
        private static CandidateBattleConditions Conditions() { return new CandidateBattleConditions { PreferenceRevision = 1, ItemUseEnabled = false }; }
        private static void Accepted(DemoContentReplayResult r) { Assert.IsTrue(r.IsAccepted, r.RejectionCode + " " + r.FieldPath + " step=" + r.FirstFailureStep); }
        private static object Groups(CandidateCritFact fact)
        {
            var state = fact.StreamBefore.Current;
            foreach (var word in fact.Words) { Assert.AreEqual(word, Pcg32Core.Next32(state, out var next)); state = next; }
            Assert.AreEqual(fact.StreamAfter.Current.State, state.State); Assert.AreEqual(fact.StreamAfter.Current.Increment, state.Increment);
            if (fact.Probability.Numerator == fact.Probability.Denominator) { Assert.IsEmpty(fact.Words); return new { Guaranteed = true }; }
            var bound = fact.Probability.Denominator; BigInteger modulus = 1; var size = 0;
            do { modulus <<= 32; size++; } while (modulus < bound);
            var threshold = modulus % bound; var rows = new List<object>();
            Assert.AreEqual(0, fact.Words.Count % size);
            for (var offset = 0; offset < fact.Words.Count; offset += size)
            {
                BigInteger value = 0; for (var j = 0; j < size; j++) value = (value << 32) + fact.Words[offset + j];
                var accepted = value >= threshold; Assert.AreEqual(offset + size == fact.Words.Count, accepted);
                if (accepted) Assert.AreEqual(fact.Triggered, value % bound < fact.Probability.Numerator);
                rows.Add(new { Offset = offset, Size = size, Value = value, Threshold = threshold, Accepted = accepted, Remainder = value % bound });
            }
            return new { Bound = bound, Modulus = modulus, Rows = rows };
        }
    }
}
