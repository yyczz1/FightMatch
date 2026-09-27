using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FightMatch.Application;
using NUnit.Framework;
using QFramework;
using static FightMatch.Core.Tests.BusinessSaveScenario;
using static FightMatch.Core.Tests.LocalSaveTestFiles;
using static FightMatch.Core.Tests.LifecycleRig;

namespace FightMatch.Core.Tests
{
    public sealed class CandidateLifecyclePreparationTests
    {
        [Test]
        public void ProfileAndContentAreFrozenAndQueryDoesNotTouchStorage()
        {
            using (var r = new LifecycleRig(false))
            {
                Assert.AreEqual(32, r.Profile.PlayerId.Length); Assert.AreEqual(32, r.Profile.OperationId.Length);
                var original = r.Profile.Content.Levels.Count;
                ((List<PreparedLevel>)r.Content.Levels).Clear(); ((List<CandidateCritCoefficient>)r.Content.CritCoefficients).Clear();
                Assert.AreEqual(original, r.Profile.Content.Levels.Count);
                var calls = r.Storage.Base.Calls;
                for (var i = 0; i < 3; i++) { r.System.QueryView(); r.System.QueryOperation(r.Profile, B()); }
                Assert.AreEqual(calls, r.Storage.Base.Calls);
                Is(r.System.OpenNewProfile(r.Profile, r.Storage, r.Caps, B()), "Completed");
                Assert.AreEqual(r.Profile.PlayerId, r.Head.Business.PlayerId);
            }
        }
        [TestCase("levels")] [TestCase("rewards")] [TestCase("coefficients")]
        public void OversizedCallerCollectionIsRejectedBeforeEnumeration(string part)
        {
            using (var r = new LifecycleRig(false))
            {
                var levels = new UnvisitedList<PreparedLevel>(10); var rewards = new UnvisitedList<CandidateRewardDefinition>(10);
                var coefficients = new UnvisitedList<CandidateCritCoefficient>(10);
                if (part == "levels") r.Content.Levels = levels;
                if (part == "rewards") r.Content.Rewards = rewards;
                if (part == "coefficients") r.Content.CritCoefficients = coefficients;
                var p = r.System.PrepareNewProfile(r.Content, new CandidateApplicationInitializeInput(), new SaveCodecBudget(Math(), maxCollectionEntries: 4));
                Assert.AreEqual("Limit", p.Code); Assert.AreEqual("CollectionEntries", p.Diagnostic.LimitReason);
                Assert.AreEqual(0, levels.Visits + rewards.Visits + coefficients.Visits);
            }
        }
        [TestCase("duplicate-level")] [TestCase("duplicate-reward")] [TestCase("duplicate-probability")]
        [TestCase("missing-reward")] [TestCase("wrong-context")]
        public void ConflictingContentIsRejected(string conflict)
        {
            using (var r = new LifecycleRig(false))
            {
                if (conflict == "duplicate-level") r.Content.Levels = new[] { r.Content.Levels[0], r.Content.Levels[0] };
                if (conflict == "duplicate-reward") r.Content.Rewards = new[] { r.Content.Rewards[0], r.Content.Rewards[0] };
                if (conflict == "duplicate-probability") r.Content.CritCoefficients = new[] { r.Content.CritCoefficients[0], r.Content.CritCoefficients[0] };
                if (conflict == "missing-reward") r.Content.Rewards = Array.Empty<CandidateRewardDefinition>();
                if (conflict == "wrong-context") r.Content.Growth = New(3).Character.Definition;
                var p = r.System.PrepareNewProfile(r.Content, new CandidateApplicationInitializeInput(), B().Codec);
                Assert.AreEqual("InconsistentBinding", p.Code); Assert.IsNull(p.Request);
            }
        }
        [Test]
        public void OwnerThreadAndClosedFacadeRejectBeforeInspectingCallerInput()
        {
            using (var r = new LifecycleRig(false))
            {
                Assert.AreEqual("WrongThread", Task.Run(() => r.System.Prepare(null, null).Code).Result);
                Assert.AreEqual("WrongThread", Task.Run(() => r.System.Submit(null, null).Code).Result);
                r.Close();
                Assert.AreEqual("Disposed", r.System.PrepareNewProfile(null, null, null).Code);
                Assert.AreEqual("Disposed", r.System.OpenNewProfile(null, null, null, null).Code);
                Assert.AreEqual("Disposed", r.System.QueryView().Battle.Code);
            }
        }
        [Test]
        public void PublicationReentryCannotPrepareOrMutateAndQueriesSeeOneHead()
        {
            using (var r = new LifecycleRig(false))
            {
                var seen = 0;
                var registration = r.Architecture.RegisterEvent<CandidateApplicationPublished>(e => {
                    seen++; Assert.AreEqual("Busy", r.System.Prepare(null, null).Code);
                    Is(r.System.Submit(r.Profile, B()), "Busy");
                    Assert.AreSame(e.View.PublishedSnapshot, r.System.QueryView().Application.PublishedSnapshot);
                });
                try { Assert.IsNull(Is(r.System.OpenNewProfile(r.Profile, r.Storage, r.Caps, B()), "Completed").NotificationFailure); Assert.AreEqual(1, seen); }
                finally { registration.UnRegister(); }
            }
        }
        [TestCase("math")] [TestCase("string")] [TestCase("number")]
        public void PreparationPreservesExactBudgetDiagnostics(string kind)
        {
            using (var r = new LifecycleRig())
            {
                var b = kind == "math" ? new SaveCodecBudget(new ExactMathBudget(1)) : kind == "string" ?
                    new SaveCodecBudget(Math(), maxStringCodeUnits: 1) : new SaveCodecBudget(Math(), maxNumericTokenBytes: 1);
                var result = r.System.Prepare(r.EntryDraft(), b);
                Assert.AreEqual("Limit", result.Code); Assert.IsNotEmpty(result.Diagnostic.LimitReason); Assert.IsNull(result.Request);
            }
        }
        [TestCase(0, "TimeSample.MonotonicElapsedMilliseconds")]
        [TestCase(1, "TimeSample.MonotonicScopeId")]
        [TestCase(2, "TimeSample.MonotonicElapsedMilliseconds")]
        public void F01_MissingRecoveryFieldsPreserveCoreRejectionAndDoNotWrite(int provided, string field)
        {
            using (var r = new LifecycleRig(level: 3, hp: 1))
            {
                r.DownAndExit(); var before = r.Head; var character = before.Business.Character;
                Assert.IsFalse(character.ActiveRecovery.IsCompleted);
                var time = F01Sample(provided); var disk = r.Disk(); var calls = r.Storage.Base.Calls;
                var expected = CandidateRecoveryClock.Advance(character, character.ActiveRecovery.RecoveryId, time, character.StateRevision, Math());
                Assert.IsFalse(expected.IsAccepted); Assert.AreEqual(CandidateGrowthRejectionCode.MissingField, expected.RejectionCode);
                Assert.AreEqual(field, expected.FieldPath);
                var result = r.System.Prepare(r.RecoveryDraft(time), B().Codec);
                Assert.AreEqual("MissingField", result.Code); Assert.AreEqual(field, result.Diagnostic.FieldPath);
                Assert.IsFalse(result.IsAccepted); Assert.IsNull(result.Request);
                Assert.AreSame(before, r.Head); Assert.AreEqual(calls, r.Storage.Base.Calls); r.SameDisk(disk);
                Assert.AreSame(character.ActiveRecovery, r.System.QueryView().Recovery);
            }
        }
        internal static CandidateTimeSample F01Sample(int provided)
        {
            var time = new CandidateTimeSample { WallUtcMilliseconds = 110, ObservedAtUtcMilliseconds = 111,
                Source = "explicit-candidate-device-clock", Trust = CandidateTimeTrust.DeviceUntrusted, Anomaly = CandidateTimeAnomaly.None };
            if ((provided & 1) != 0) time.MonotonicElapsedMilliseconds = null;
            if ((provided & 2) != 0) time.MonotonicScopeId = null;
            return time;
        }
    }
}
