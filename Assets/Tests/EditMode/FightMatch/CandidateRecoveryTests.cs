using System;
using System.Collections.Generic;
using System.Numerics;
using NUnit.Framework;
using static FightMatch.Core.Tests.GrowthFixture;
using static FightMatch.Core.CandidateGrowthRejectionCode;

namespace FightMatch.Core.Tests
{
    [TestFixture]
    public sealed class CandidateRecoveryTests
    {
        [TestCase(CandidateCharacterEndKind.NormalVictory)]
        [TestCase(CandidateCharacterEndKind.NormalExit)]
        public void OrdinaryDownParticipant_StartsOneOriginalDurationAndKeepsSlot(CandidateCharacterEndKind kind)
        {
            var state = State(4, 157); var facts = End(kind: kind); var result = Propose(state, facts);
            Assert.IsTrue(state.IsReady); Assert.IsEmpty(state.RecoveryPeriods); Assert.IsFalse(result.Next.IsReady);
            Assert.AreEqual(2, result.Next.OriginalSlot); Assert.AreEqual(new BigInteger(2), result.Next.StateRevision);
            Assert.AreEqual(state.Level, result.Next.Level); Assert.AreEqual(state.Experience, result.Next.Experience); Assert.IsEmpty(result.Next.BaseRewards);
            var period = result.RecoveryPeriod;
            Value(period.Duration, 180000); Value(period.Elapsed, 0); Assert.IsFalse(period.IsCompleted);
            Assert.AreEqual("recovery-first", period.RecoveryId); Assert.AreSame(result.EndReceipt, period.EndReceipt);
            Assert.AreSame(state.Definition, period.Definition); Assert.AreSame(period.StartSample, period.LastAcceptedSample);
            var stats = CandidateCharacterGrowth.ComputeBaseStats(result.Next, new ExactMathBudget());
            Assert.IsFalse(stats.IsReady); Assert.IsNull(stats.EntryHp); Value(stats.Stats.MaxHp, 124);
            var entry = new BattleEntryPreparer().PrepareCandidate(Entry(stats), new ExactMathBudget());
            Assert.IsFalse(entry.IsAccepted); Assert.IsNull(entry.Entry); Assert.AreEqual("Members[0].EntryHp", entry.FieldPath);
            // The old entry validator still independently rejects an all-unready roster with otherwise complete fields.
            var complete = Entry(stats); complete.Members[0].EntryHp = stats.Stats.MaxHp;
            var noReady = new BattleEntryPreparer().PrepareCandidate(complete, new ExactMathBudget());
            Assert.AreEqual(BattleEntryRejectionCode.NoReadyMember, noReady.RejectionCode);
        }

        [TestCase(CandidateCharacterEndKind.NormalVictory, true, false)]
        [TestCase(CandidateCharacterEndKind.NormalExit, true, false)]
        [TestCase(CandidateCharacterEndKind.ImmediateRestart, true, true)]
        [TestCase(CandidateCharacterEndKind.ImmediateRestart, true, false)]
        [TestCase(CandidateCharacterEndKind.NormalExit, false, false)]
        public void NoNewRecovery_StillRecordsEndAndNextOrdinaryEntryHasFullHp(CandidateCharacterEndKind kind, bool participant, bool down)
        {
            var result = Propose(State(4), End(kind: kind, participant: participant, down: down));
            Assert.IsTrue(result.Next.IsReady); Assert.IsEmpty(result.Next.RecoveryPeriods); Assert.IsNull(result.RecoveryPeriod);
            Assert.AreEqual(1, result.Next.ProcessedEnds.Count); Assert.AreEqual(new BigInteger(2), result.Next.StateRevision);
            var stats = CandidateCharacterGrowth.ComputeBaseStats(result.Next, new ExactMathBudget()); Value(stats.EntryHp, 124);
        }

        [TestCase(CandidateCharacterEndKind.NormalVictory)]
        [TestCase(CandidateCharacterEndKind.NormalExit)]
        [TestCase(CandidateCharacterEndKind.ImmediateRestart)]
        public void ExistingNonParticipantRecovery_IsNotRestarted(CandidateCharacterEndKind kind)
        {
            var state = Advance(Propose(State(), End()).Next, Time(10000)).Next;
            var original = state.ActiveRecovery; var result = Propose(state, End("second", kind, false, false));
            Assert.AreSame(original, result.Next.ActiveRecovery); Value(result.Next.ActiveRecovery.Elapsed, 10000);
            Assert.AreEqual(1, result.Next.RecoveryPeriods.Count); Assert.AreEqual(2, result.Next.ProcessedEnds.Count);
        }

        [Test]
        public void DuplicateEnd_ReturnsCurrentStateAndOriginalFactsBeforeRevision()
        {
            var facts = End(); var first = Propose(State(), facts);
            var current = Apply(first.Next, Reward(14)).Next;
            var result = CandidateCharacterEnd.Propose(current, End(), 0, new ExactMathBudget());
            Assert.AreEqual(CandidateGrowthOutcome.AlreadyIncluded, result.Outcome); Assert.AreSame(current, result.Next);
            Assert.AreSame(first.EndReceipt, result.EndReceipt); Assert.AreEqual(1, result.Next.RecoveryPeriods.Count);
        }

        [TestCase("EndReceiptId")]
        [TestCase("AttemptId")]
        [TestCase("EntryBaselineId")]
        [TestCase("RecoveryId")]
        [TestCase("PlayerId")]
        [TestCase("CharacterId")]
        [TestCase("Context.ContentFingerprint")]
        [TestCase("Kind")]
        [TestCase("WasParticipant")]
        [TestCase("WasDown")]
        [TestCase("TimeSample")]
        public void ChangedEndFactsCannotCreateAnotherRecovery(string field)
        {
            var state = Propose(State(), End()).Next; var facts = End();
            if (field == "Kind") facts.Kind = CandidateCharacterEndKind.NormalVictory;
            else if (field == "WasParticipant") { facts.WasParticipant = false; facts.WasDown = false; }
            else if (field == "WasDown") facts.WasDown = false;
            else if (field == "TimeSample") facts.TimeSample = Time(1);
            else Set(facts, field, "changed");
            Reject(CandidateCharacterEnd.Propose(state, facts, 0, new ExactMathBudget()), InconsistentBinding, field);
            Assert.AreEqual(1, state.RecoveryPeriods.Count); Value(state.ActiveRecovery.Elapsed, 0);
        }

        [Test]
        public void RecoveryIdCannotBeReassignedAfterCompletion_AndRecoveringCharacterCannotClaimParticipation()
        {
            var state = Propose(State(), End()).Next;
            Reject(Propose(state, End("another"), false), InconsistentBinding, "WasParticipant");
            var ready = Advance(state, Time(180000)).Next;
            var conflicting = End("another"); conflicting.RecoveryId = "recovery-first";
            Reject(Propose(ready, conflicting, false), InconsistentBinding, "RecoveryId");
            var missing = End("another"); missing.WasParticipant = false;
            Reject(Propose(ready, missing, false), InconsistentBinding, "WasDown");
        }

        [TestCase("PlayerId")]
        [TestCase("CharacterId")]
        [TestCase("AttemptId")]
        [TestCase("EntryBaselineId")]
        [TestCase("EndReceiptId")]
        [TestCase("Context")]
        [TestCase("WasParticipant")]
        [TestCase("WasDown")]
        [TestCase("RecoveryId")]
        [TestCase("TimeSample")]
        public void NewRecoveryRequiresEveryFact(string path)
        {
            var facts = End(); Set(facts, path, null);
            Reject(Propose(State(), facts, false), MissingField, path);
        }

        [Test]
        public void OptionalEndValuesRequireExplicitNullAndCannotSupplyUnusedParameters()
        {
            var facts = new CandidateCharacterEndFacts { PlayerId = "player", CharacterId = "warrior", AttemptId = "a", EntryBaselineId = "b",
                EndReceiptId = "e", Context = Context(), Kind = CandidateCharacterEndKind.NormalExit, WasParticipant = true, WasDown = false };
            Reject(Propose(State(), facts, false), MissingField, "RecoveryId"); facts.RecoveryId = null;
            Reject(Propose(State(), facts, false), MissingField, "TimeSample"); facts.TimeSample = null;
            Assert.IsTrue(Propose(State(), facts).Next.IsReady);
            facts.RecoveryId = "unused"; Reject(Propose(State(), facts, false), InvalidValue, "RecoveryId"); facts.RecoveryId = null;
            facts.TimeSample = Time(0); Reject(Propose(State(), facts, false), InvalidValue, "TimeSample");
            facts = End(kind: CandidateCharacterEndKind.ImmediateRestart); facts.RecoveryId = "unused";
            Reject(Propose(State(), facts, false), InvalidValue, "RecoveryId");
        }

        [Test]
        public void NewEndRequiresCurrentRevisionAndSupportedKind()
        {
            var state = Apply(State(), Reward(0)).Next;
            Reject(CandidateCharacterEnd.Propose(state, End(), 1, new ExactMathBudget()), StaleContext, "ExpectedRevision");
            var facts = End(); facts.Kind = CandidateCharacterEndKind.Unspecified; Reject(Propose(state, facts, false), MissingField, "Kind");
            facts.Kind = (CandidateCharacterEndKind)99; Reject(Propose(state, facts, false), UnsupportedBinding, "Kind");
        }

        [Test]
        public void SameMonotonicDomainIgnoresWallRegression_AndKeepsHighWaterMark()
        {
            var facts = End(); facts.TimeSample = Time(1000, R(1000), "boot"); var start = Propose(State(), facts).Next;
            var sample = Time(-9000, R(11000), "boot"); var ten = Advance(start, sample);
            Value(ten.RecoveryPeriod.Elapsed, 10000); Assert.AreEqual(CandidateTimeAnomaly.None, ten.Anomaly);
            var duplicate = Advance(ten.Next, sample); Assert.AreSame(ten.Next, duplicate.Next); Assert.AreEqual(CandidateGrowthOutcome.Unchanged, duplicate.Outcome);
            var backwards = Advance(ten.Next, Time(50000, R(10500), "boot"));
            Assert.AreSame(ten.Next, backwards.Next); Assert.AreEqual(CandidateGrowthOutcome.IgnoredTimeRegression, backwards.Outcome);
            Assert.AreEqual(CandidateTimeAnomaly.ClockBackward, backwards.Anomaly);
            Assert.AreSame(ten.RecoveryPeriod.LastAcceptedSample, backwards.RecoveryPeriod.LastAcceptedSample);
            var resumed = Advance(backwards.Next, Time(60000, R(12000), "boot")); Value(resumed.RecoveryPeriod.Elapsed, 11000);
            Value(start.ActiveRecovery.Elapsed, 0); Value(ten.Next.ActiveRecovery.Elapsed, 10000);
        }

        [TestCase(null, null)]
        [TestCase("boot", "other")]
        [TestCase("boot", "Boot")]
        [TestCase("boot", null)]
        [TestCase(null, "boot")]
        public void CrossDomainUsesUtcAndPreservesDeviceTrust(string previousScope, string nextScope)
        {
            var facts = End(); facts.TimeSample = Time(1000, previousScope == null ? null : R(99999), previousScope);
            var state = Propose(State(), facts).Next;
            var result = Advance(state, Time(11000, nextScope == null ? null : R(1), nextScope));
            Value(result.RecoveryPeriod.Elapsed, 10000); Assert.AreEqual(CandidateTimeAnomaly.DomainChanged, result.Anomaly);
            Assert.AreEqual(CandidateTimeTrust.DeviceUntrusted, result.RecoveryPeriod.LastAcceptedSample.Trust);
            Assert.AreEqual("device", result.RecoveryPeriod.LastAcceptedSample.Source);
        }

        [Test]
        public void UtcRegressionNeverAddsWait_AndLargeForwardJumpCapsAtOriginalDuration()
        {
            var start = Propose(State(), End()).Next; var ten = Advance(start, Time(10000));
            var back = Advance(ten.Next, Time(5000)); Assert.AreSame(ten.Next, back.Next);
            Assert.AreEqual(CandidateTimeAnomaly.ClockBackward | CandidateTimeAnomaly.DomainChanged, back.Anomaly);
            var rebound = Advance(back.Next, Time(11000)); Value(rebound.RecoveryPeriod.Elapsed, 11000);
            var forward = Advance(rebound.Next, Time(BigInteger.One << 60));
            Value(forward.RecoveryPeriod.Elapsed, 180000); Assert.IsTrue(forward.Next.IsReady); Assert.IsTrue(forward.RecoveryPeriod.IsCompleted);
            var after = Advance(forward.Next, Time(-1)); Assert.AreSame(forward.Next, after.Next);
            Assert.AreEqual(CandidateGrowthOutcome.AlreadyIncluded, after.Outcome); Assert.IsTrue(after.Next.IsReady);
        }

        [Test]
        public void ExactCompletionBoundaryAndFractionalMonotonicMilliseconds()
        {
            var start = Propose(State(), End()).Next; var before = Advance(start, Time(179999));
            Assert.IsFalse(before.Next.IsReady); Value(before.RecoveryPeriod.Elapsed, 179999);
            var done = Advance(before.Next, Time(180000)); Assert.IsTrue(done.Next.IsReady); Value(done.RecoveryPeriod.Elapsed, 180000);
            var facts = End(); facts.TimeSample = Time(0, R(0), "boot"); start = Propose(State(), facts).Next;
            var fraction = Advance(start, Time(0, R(1, 3), "boot")); Value(fraction.RecoveryPeriod.Elapsed, 1, 3);
            var near = Advance(fraction.Next, Time(0, R(539999, 3), "boot")); Value(near.RecoveryPeriod.Elapsed, 539999, 3);
            Assert.IsFalse(near.Next.IsReady); done = Advance(near.Next, Time(0, R(180000), "boot")); Assert.IsTrue(done.Next.IsReady);
        }

        [Test]
        public void FractionalOriginalDurationAndLargeUtcAreExact()
        {
            var input = DefinitionInput(); input.RecoveryDurationMilliseconds = R(1, 3);
            var huge = (BigInteger.One << 60) + 1; var facts = End(); facts.TimeSample = Time(huge, R(0), "boot");
            var state = Propose(State(definition: Definition(input)), facts).Next;
            Assert.AreEqual(huge, state.ActiveRecovery.StartSample.WallUtcMilliseconds);
            var half = Advance(state, Time(huge, R(1, 6), "boot")); Value(half.RecoveryPeriod.Elapsed, 1, 6);
            Assert.IsFalse(half.Next.IsReady); var full = Advance(half.Next, Time(huge, R(1, 3), "boot"));
            Assert.IsTrue(full.Next.IsReady); Value(full.RecoveryPeriod.Elapsed, 1, 3);
            CheckEveryStateEntryRejectsSmallBudget(full.Next, 53);
        }

        [Test]
        public void ZeroDeltaWithChangedFieldsUpdatesOnce_AndRetainsInputAnomaly()
        {
            var state = Propose(State(), End()).Next; var sample = Time(0); sample.Source = "other-device-source";
            sample.ObservedAtUtcMilliseconds = 100; sample.Anomaly = CandidateTimeAnomaly.ClockBackward;
            var result = Advance(state, sample); Value(result.RecoveryPeriod.Elapsed, 0);
            Assert.AreEqual(state.StateRevision + 1, result.Next.StateRevision);
            Assert.AreEqual(CandidateTimeAnomaly.ClockBackward | CandidateTimeAnomaly.DomainChanged, result.Anomaly);
            Assert.AreEqual(CandidateTimeAnomaly.ClockBackward, result.RecoveryPeriod.LastAcceptedSample.Anomaly);
            Assert.AreSame(result.Next, Advance(result.Next, sample).Next);
        }

        [Test]
        public void OldCompletedRecoveryAndEndCannotTouchNewActivePeriod()
        {
            var first = Propose(State(), End()); var completed = Advance(first.Next, Time(180000));
            var second = Propose(completed.Next, End("second"));
            var old = CandidateRecoveryClock.Advance(second.Next, "recovery-first", Time(-10000), 0, new ExactMathBudget());
            Assert.AreSame(second.Next, old.Next); Assert.AreSame(completed.RecoveryPeriod, old.RecoveryPeriod);
            Assert.AreSame(second.RecoveryPeriod, old.Next.ActiveRecovery); Assert.AreEqual(CandidateGrowthOutcome.AlreadyIncluded, old.Outcome);
            Assert.IsFalse(old.Next.IsReady); Value(old.Next.ActiveRecovery.Elapsed, 0);
            var oldEnd = CandidateCharacterEnd.Propose(second.Next, End(), 0, new ExactMathBudget());
            Assert.AreSame(second.Next, oldEnd.Next); Assert.AreSame(first.EndReceipt, oldEnd.EndReceipt);
            Reject(CandidateRecoveryClock.Advance(second.Next, "Recovery-first", Time(0), second.Next.StateRevision,
                new ExactMathBudget()), InconsistentBinding, "RecoveryId");
            Reject(CandidateRecoveryClock.Advance(second.Next, "recovery-second", Time(1), 0, new ExactMathBudget()), StaleContext, "ExpectedRevision");
        }

        [TestCaseSource(nameof(InvalidTimes))]
        public void InvalidTimeHasNoNewState(string field, object value, CandidateGrowthRejectionCode code, string path)
        {
            var state = Propose(State(), End()).Next; var sample = Time(1, R(1), "boot"); Set(sample, field, value);
            Reject(Advance(state, sample, false), code, path);
            var facts = End(); facts.TimeSample = sample; Reject(Propose(State(), facts, false), code, path);
        }
        private static IEnumerable<TestCaseData> InvalidTimes()
        {
            foreach (var p in new[] { "WallUtcMilliseconds", "ObservedAtUtcMilliseconds", "Source", "Anomaly" })
                yield return new TestCaseData(p, null, MissingField, "TimeSample." + p);
            yield return new TestCaseData("MonotonicElapsedMilliseconds", R(-1), InvalidValue, "TimeSample.MonotonicElapsedMilliseconds");
            yield return new TestCaseData("MonotonicElapsedMilliseconds", null, InconsistentBinding, "TimeSample.MonotonicScopeId");
            yield return new TestCaseData("MonotonicScopeId", null, InconsistentBinding, "TimeSample.MonotonicScopeId");
            yield return new TestCaseData("MonotonicScopeId", " ", MissingField, "TimeSample.MonotonicScopeId");
            yield return new TestCaseData("Trust", CandidateTimeTrust.Unspecified, MissingField, "TimeSample.Trust");
            yield return new TestCaseData("Trust", CandidateTimeTrust.ServerTrusted, UnsupportedBinding, "TimeSample.Trust");
            yield return new TestCaseData("Anomaly", (CandidateTimeAnomaly)8, InvalidValue, "TimeSample.Anomaly");
        }

        [Test]
        public void AbsentNullableTimeFieldsDifferFromExplicitNull()
        {
            var state = Propose(State(), End()).Next;
            var time = new CandidateTimeSample { WallUtcMilliseconds = 0, ObservedAtUtcMilliseconds = 0,
                Source = "device", Trust = CandidateTimeTrust.DeviceUntrusted, Anomaly = CandidateTimeAnomaly.None };
            Reject(Advance(state, time, false), MissingField, "TimeSample.MonotonicElapsedMilliseconds");
            time.MonotonicElapsedMilliseconds = null; Reject(Advance(state, time, false), MissingField, "TimeSample.MonotonicScopeId");
            time.MonotonicScopeId = null; Assert.AreSame(state, Advance(state, time).Next);
        }

        [TestCase("WallUtcMilliseconds")]
        [TestCase("ObservedAtUtcMilliseconds")]
        [TestCase("MonotonicElapsedMilliseconds")]
        public void EndAndLastAcceptedTimeNumbersAreRecheckedIncludingRationalDenominators(string field)
        {
            var huge = BigInteger.One << 160; var sample = Time(0, R(0), "boot");
            Set(sample, field, field == "MonotonicElapsedMilliseconds" ? (object)R(1, huge) : huge);
            var facts = End(); facts.TimeSample = sample;
            var fromEnd = Propose(State(), facts).Next; CheckEveryStateEntryRejectsSmallBudget(fromEnd, 64);
            var normal = End(); normal.TimeSample = Time(0, R(0), "boot");
            var fromAdvance = Advance(Propose(State(), normal).Next, sample).Next;
            CheckEveryStateEntryRejectsSmallBudget(fromAdvance, 64);
        }

        [Test]
        public void SharedBudgetCoversEndAndRecovery_AndRetryMatches()
        {
            var state = State(); var facts = End(); var budget = new ExactMathBudget();
            var first = CandidateCharacterEnd.Propose(state, facts, state.StateRevision, budget);
            Assert.Throws<ExactMathLimitException>(() => CandidateCharacterEnd.Propose(state, facts, state.StateRevision,
                new ExactMathBudget(32768, (int)budget.PrimitiveStepsUsed - 1)));
            Assert.AreEqual(Describe(first), Describe(Propose(state, facts))); Assert.IsEmpty(state.ProcessedEnds);
            budget = new ExactMathBudget(); var sample = Time(10000);
            var next = CandidateRecoveryClock.Advance(first.Next, "recovery-first", sample, first.Next.StateRevision, budget);
            Assert.Throws<ExactMathLimitException>(() => CandidateRecoveryClock.Advance(first.Next, "recovery-first", sample, first.Next.StateRevision,
                new ExactMathBudget(32768, (int)budget.PrimitiveStepsUsed - 1)));
            Assert.AreEqual(Describe(next), Describe(Advance(first.Next, sample))); Value(first.Next.ActiveRecovery.Elapsed, 0);
        }

        [Test]
        public void EndTimeAndDefinitionInputsCannotMutateOriginalPeriodOrAnyOutput()
        {
            var input = DefinitionInput(); var state = State(definition: Definition(input)); var facts = End();
            var first = Propose(state, facts); var sample = Time(10000); var next = Advance(first.Next, sample);
            var oldText = Describe(first); var nextText = Describe(next);
            input.RecoveryDurationMilliseconds = R(1); facts.Context.SourceNotes.Clear(); facts.AttemptId = "changed";
            facts.TimeSample.WallUtcMilliseconds = 999; facts.RecoveryId = "changed"; sample.WallUtcMilliseconds = 0;
            sample.Source = "changed"; sample.Anomaly = CandidateTimeAnomaly.ClockBackward;
            Assert.AreEqual(oldText, Describe(first)); Assert.AreEqual(nextText, Describe(next)); Immutable(first); Immutable(next);
            Assert.Throws<NotSupportedException>(() => ((IList<CandidateCharacterEndReceipt>)next.Next.ProcessedEnds).Clear());
            Assert.Throws<NotSupportedException>(() => ((IList<CandidateRecoveryPeriod>)next.Next.RecoveryPeriods).Clear());
            Assert.AreEqual("attempt-first", next.RecoveryPeriod.EndReceipt.AttemptId); Value(next.RecoveryPeriod.Duration, 180000);
        }

        [Test]
        public void RootNullsAndMissingRecoveryIdentityAreExplicit()
        {
            var state = State(); var budget = new ExactMathBudget();
            Assert.Throws<ArgumentNullException>(() => CandidateCharacterEnd.Propose(null, End(), 1, budget));
            Assert.Throws<ArgumentNullException>(() => CandidateCharacterEnd.Propose(state, null, 1, budget));
            Assert.Throws<ArgumentNullException>(() => CandidateCharacterEnd.Propose(state, End(), 1, null));
            Assert.Throws<ArgumentNullException>(() => CandidateRecoveryClock.Advance(null, "id", Time(0), 1, budget));
            Assert.Throws<ArgumentNullException>(() => CandidateRecoveryClock.Advance(state, "id", null, 1, budget));
            Assert.Throws<ArgumentNullException>(() => CandidateRecoveryClock.Advance(state, "id", Time(0), 1, null));
            Reject(CandidateRecoveryClock.Advance(state, null, Time(0), 1, budget), MissingField, "RecoveryId");
            Reject(CandidateRecoveryClock.Advance(state, "unknown", Time(0), 1, budget), InconsistentBinding, "RecoveryId");
        }

        internal static CandidateTimeSample Time(BigInteger wall, ExactRational monotonic = null, string scope = null)
        {
            return new CandidateTimeSample { WallUtcMilliseconds = wall, ObservedAtUtcMilliseconds = wall,
                MonotonicElapsedMilliseconds = monotonic, MonotonicScopeId = scope, Source = "device",
                Trust = CandidateTimeTrust.DeviceUntrusted, Anomaly = CandidateTimeAnomaly.None };
        }
        internal static CandidateCharacterEndFacts End(string id = "first", CandidateCharacterEndKind kind = CandidateCharacterEndKind.NormalExit,
            bool participant = true, bool down = true)
        {
            var needsTime = kind != CandidateCharacterEndKind.ImmediateRestart && participant && down;
            return new CandidateCharacterEndFacts { PlayerId = "player", CharacterId = "warrior", Context = Context(),
                AttemptId = "attempt-" + id, EntryBaselineId = "baseline-" + id, EndReceiptId = "end-" + id,
                Kind = kind, WasParticipant = participant, WasDown = down, RecoveryId = needsTime ? "recovery-" + id : null,
                TimeSample = needsTime ? Time(0) : null };
        }
        private static CandidateCharacterResult Propose(CandidateCharacterState state, CandidateCharacterEndFacts facts, bool accepted = true)
        {
            var result = CandidateCharacterEnd.Propose(state, facts, state.StateRevision, new ExactMathBudget());
            if (accepted) Assert.IsTrue(result.IsAccepted, result.FieldPath); return result;
        }
        private static CandidateCharacterResult Advance(CandidateCharacterState state, CandidateTimeSample sample, bool accepted = true)
        {
            var result = CandidateRecoveryClock.Advance(state, "recovery-first", sample, state.StateRevision, new ExactMathBudget());
            if (accepted) Assert.IsTrue(result.IsAccepted, result.FieldPath); return result;
        }
    }
}
