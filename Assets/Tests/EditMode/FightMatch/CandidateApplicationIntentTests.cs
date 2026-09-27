using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using NUnit.Framework;
using FlowPuzzle.Core;
using static FightMatch.Core.Tests.BusinessSaveScenario;

namespace FightMatch.Core.Tests
{
    [TestFixture]
    public class CandidateApplicationIntentTests
    {
        [Test]
        public void InitializationMatchesHandCalculatedCompleteBytesAndIndependentSha256()
        {
            // FMINT001, schema, p/i/1/null, seven complete context fields, x/w/1/0/slot0.
            const string hex = "464d494e543030310100000001000000700001000000690001000000000100000064000100000031" +
                "010000006300010000007200010000006e00010000007100010000000100000073000100000078000100000077000100000031010000003000000000";
            var prepared = Accept(CandidateApplicationProtocol.PrepareIntent(ApplicationScenario.Simple(CandidateApplicationKind.InitializeProfile), Budget()));
            Assert.AreEqual(100, prepared.CanonicalBytes.Count);
            CollectionAssert.AreEqual(Hex(hex), prepared.CanonicalBytes);
            CollectionAssert.AreEqual(Hex("dc9d0a7e42a08310d68fd546199dd0f484f818da5deee3f4d5568e799a95383d"), prepared.Sha256);
            Assert.AreEqual(BigInteger.One, prepared.Context.DraftRevision);
        }

        [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)] [TestCase(5)]
        [TestCase(6)] [TestCase(7)] [TestCase(8)] [TestCase(9)]
        public void AllNineKindsFreezeTheirCanonicalIdentityAndRejectIrrelevantPayloads(int kind)
        {
            var input = ApplicationScenario.Simple((CandidateApplicationKind)kind);
            var prepared = Accept(CandidateApplicationProtocol.PrepareIntent(input, Budget()));
            Assert.AreEqual((CandidateApplicationKind)kind, prepared.Kind);
            Assert.AreEqual("p", prepared.PlayerId);
            Assert.AreEqual("i", prepared.OperationId);
            CollectionAssert.AreEqual(new byte[] { 70, 77, 73, 78, 84, 48, 48, 49, 1, 0, 0, 0 }, prepared.CanonicalBytes.Take(12));
            var copy = prepared.CanonicalBytes.ToArray();
            input.OperationId = "changed";
            input.Context.DraftRevision = 99;
            ((List<string>)input.Context.SourceNotes).Add("later");
            if (input.Attack != null) ((List<FlowPos>)input.Attack.Route).Reverse();
            if (input.Link != null) ((List<FlowPos>)input.Link.Route).Reverse();
            if (input.Rollback != null) ((List<string>)input.Rollback.ConfirmedRemovedOperationIds).Reverse();
            if (input.AdvanceRecovery != null) input.AdvanceRecovery.TimeSample.Source = "later";
            CollectionAssert.AreEqual(copy, prepared.CanonicalBytes);
            Assert.AreEqual(BigInteger.One, prepared.Context.DraftRevision);
            Assert.AreEqual("i", prepared.OperationId);
            Assert.Throws<NotSupportedException>(() => ((IList<byte>)prepared.CanonicalBytes)[0] = 0);
            Assert.Throws<NotSupportedException>(() => ((IList<byte>)prepared.Sha256)[0] = 0);
            Assert.Throws<NotSupportedException>(() => ((IList<string>)prepared.Context.SourceNotes).Add("no"));
            if (kind == 1) input.Link = ApplicationScenario.Simple(CandidateApplicationKind.Link).Link;
            else input.InitializeProfile = ApplicationScenario.Simple(CandidateApplicationKind.InitializeProfile).InitializeProfile;
            Rejected(CandidateApplicationProtocol.PrepareIntent(input, Budget()), "InvalidValue");
        }

        private static IEnumerable<TestCaseData> KeyFields()
        {
            var counts = new[] { 5, 6, 9, 5, 6, 7, 4, 4, 10 };
            for (var kind = 1; kind <= 9; kind++) for (var field = 0; field < counts[kind - 1]; field++)
                yield return new TestCaseData(kind, field);
        }

        [TestCaseSource(nameof(KeyFields))]
        public void EveryPayloadFieldAndOrderedSequenceParticipatesInIdentity(int kind, int field)
        {
            var input = ApplicationScenario.Simple((CandidateApplicationKind)kind);
            var original = Accept(CandidateApplicationProtocol.PrepareIntent(input, Budget()));
            Mutate(input, field);
            var changed = Accept(CandidateApplicationProtocol.PrepareIntent(input, Budget()));
            Assert.IsFalse(original.CanonicalBytes.SequenceEqual(changed.CanonicalBytes));
            Assert.IsFalse(original.Sha256.SequenceEqual(changed.Sha256));
        }

        private static void Mutate(CandidateApplicationIntentInput input, int field)
        {
            switch (input.Kind)
            {
                case CandidateApplicationKind.InitializeProfile:
                    var a = input.InitializeProfile;
                    new Action[] { () => a.CharacterId += "2", () => a.ClassId += "2", () => a.InitialLevel++,
                        () => a.InitialExperience++, () => a.OriginalSlot++ }[field]();
                    break;
                case CandidateApplicationKind.EnterAttempt:
                    var b = input.EnterAttempt;
                    new Action[] { () => b.LevelId += "2", () => b.LevelVersion += "2", () => b.CharacterId += "2",
                        () => b.ExpectedCharacterRevision++, () => b.OriginalSlot++, () => b.ExpectedCharacterRevision = BigInteger.One << 80 }[field]();
                    break;
                case CandidateApplicationKind.Attack:
                    var c = input.Attack;
                    new Action[] { () => { c.AttemptId = "a2"; c.Actor = BattleCombatantKey.ForParticipant("a2", "x"); c.Pair = new BattlePairKey("a2", "f", "p"); },
                        () => c.ExpectedSceneRevision++, () => c.Actor = BattleCombatantKey.ForParticipant("a", "y"),
                        () => c.Pair = new BattlePairKey("a", "f2", "p"), () => c.Pair = new BattlePairKey("a", "f", "p2"),
                        () => ((List<FlowPos>)c.Route).Reverse(), () => ((List<FlowPos>)c.Route).Add(new FlowPos(2, 0)),
                        () => c.ExpectedPreferenceRevision++, () => c.ItemUseEnabled = true }[field]();
                    break;
                case CandidateApplicationKind.Link:
                    var d = input.Link;
                    new Action[] { () => { d.AttemptId = "a2"; d.Pair = new BattlePairKey("a2", "f", "p"); }, () => d.ExpectedSceneRevision++,
                        () => d.Pair = new BattlePairKey("a", "f2", "p"), () => d.Pair = new BattlePairKey("a", "f", "p2"),
                        () => ((List<FlowPos>)d.Route).Reverse() }[field]();
                    break;
                case CandidateApplicationKind.Rollback:
                    var e = input.Rollback;
                    new Action[] { () => e.AttemptId += "2", () => e.ExpectedSceneRevision++, () => e.HistoryAnchorId += "2",
                        () => e.TargetOperationId += "2", () => ((List<string>)e.ConfirmedRemovedOperationIds).Reverse(),
                        () => ((List<string>)e.ConfirmedRemovedOperationIds).Add("o3") }[field]();
                    break;
                case CandidateApplicationKind.SettleVictory:
                    var v = input.SettleVictory;
                    new Action[] { () => v.AttemptId += "2", () => v.ChallengeId += "2", () => v.EntryBaselineId += "2",
                        () => v.FinalReportFingerprint += "2", () => v.TerminalOperationId += "2", () => v.RewardDefinitionId += "2",
                        () => v.RewardDefinitionVersion += "2" }[field]();
                    break;
                case CandidateApplicationKind.ExitAttempt:
                case CandidateApplicationKind.RestartAttempt:
                    var end = input.ExitAttempt ?? input.RestartAttempt;
                    new Action[] { () => end.AttemptId += "2", () => end.ChallengeId += "2", () => end.EntryBaselineId += "2", () => end.ExpectedSceneRevision++ }[field]();
                    break;
                case CandidateApplicationKind.AdvanceRecovery:
                    var r = input.AdvanceRecovery;
                    var t = r.TimeSample;
                    new Action[] { () => r.CharacterId += "2", () => r.RecoveryId += "2", () => r.ExpectedCharacterRevision = BigInteger.One << 80,
                        () => t.WallUtcMilliseconds--, () => t.ObservedAtUtcMilliseconds++, () => t.MonotonicElapsedMilliseconds = R(2, 3),
                        () => t.MonotonicScopeId += "2", () => t.Source += "2", () => t.Anomaly = CandidateTimeAnomaly.ClockBackward,
                        () => { t.MonotonicElapsedMilliseconds = null; t.MonotonicScopeId = null; } }[field]();
                    break;
            }
        }

        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)] [TestCase(5)]
        [TestCase(6)] [TestCase(7)] [TestCase(8)] [TestCase(9)] [TestCase(10)]
        public void CommonFieldsAndEveryContextFieldRemainExact(int field)
        {
            var input = ApplicationScenario.Simple(CandidateApplicationKind.InitializeProfile);
            var original = Accept(CandidateApplicationProtocol.PrepareIntent(input, Budget()));
            var c = input.Context;
            new Action[] { () => input.PlayerId += " ", () => input.OperationId = "I", () => input.ExpectedCommitId = "prior",
                () => c.DraftId += "2", () => c.DraftRevision++, () => c.ContentFingerprint += "2", () => c.RuleVersion += "2",
                () => c.NumericContractVersion += "2", () => c.RandomContractVersion += "2", () => ((List<string>)c.SourceNotes).Add("second"),
                () => c.SourceNotes = new List<string> { "second", "s" } }[field]();
            Assert.IsFalse(original.CanonicalBytes.SequenceEqual(Accept(CandidateApplicationProtocol.PrepareIntent(input, Budget())).CanonicalBytes));
        }

        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)] [TestCase(5)] [TestCase(6)] [TestCase(7)]
        [TestCase(8)] [TestCase(9)] [TestCase(10)] [TestCase(11)] [TestCase(12)] [TestCase(13)] [TestCase(14)] [TestCase(15)]
        public void MissingOrInvalidClosedFieldsAreStructuredRejections(int mutation)
        {
            var input = ApplicationScenario.Simple(CandidateApplicationKind.Attack);
            if (mutation == 0) input.Kind = null;
            if (mutation == 1) input.Kind = (CandidateApplicationKind)10;
            if (mutation == 2) input.Attack = null;
            if (mutation == 3) input.Context = null;
            if (mutation == 4) input.PlayerId = "  ";
            if (mutation == 5) input.Attack.ExpectedSceneRevision = null;
            if (mutation == 6) input.Attack.ExpectedSceneRevision = 0;
            if (mutation == 7) input.Attack.ItemUseEnabled = null;
            if (mutation == 8) input.Attack.ExpectedPreferenceRevision = null;
            if (mutation == 9) input.Attack.Actor = BattleCombatantKey.ForParticipant("foreign", "x");
            if (mutation == 10) input.Attack.Actor = new BattleCombatantKey("a", BattleCombatantKind.Enemy, null, "f", "e");
            if (mutation == 11) input.Attack.Pair = new BattlePairKey("foreign", "f", "p");
            if (mutation == 12) input.Attack.Route = new[] { new FlowPos(0, 0) };
            if (mutation == 13) input.Attack.Route = new[] { new FlowPos(-1, 0), new FlowPos(0, 0) };
            if (mutation == 14) input.Context.SourceNotes = null;
            if (mutation == 15) input.Kind = CandidateApplicationKind.Link;
            Rejected(CandidateApplicationProtocol.PrepareIntent(input, Budget()));
        }

        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)] [TestCase(5)] [TestCase(6)] [TestCase(7)]
        public void TimeRequiresExplicitNullablePairAndUntrustedCanonicalFields(int mutation)
        {
            var input = ApplicationScenario.Simple(CandidateApplicationKind.AdvanceRecovery);
            var t = input.AdvanceRecovery.TimeSample;
            if (mutation == 0) t.WallUtcMilliseconds = null;
            if (mutation == 1) t.ObservedAtUtcMilliseconds = null;
            if (mutation == 2) t.MonotonicScopeId = null;
            if (mutation == 3) t.MonotonicElapsedMilliseconds = null;
            if (mutation == 4) t.Trust = CandidateTimeTrust.ServerTrusted;
            if (mutation == 5) t.Anomaly = null;
            if (mutation == 6) t.Anomaly = (CandidateTimeAnomaly)4;
            if (mutation == 7) input.AdvanceRecovery.TimeSample = new CandidateTimeSample { WallUtcMilliseconds = -5,
                ObservedAtUtcMilliseconds = -4, Source = "clock", Trust = CandidateTimeTrust.DeviceUntrusted, Anomaly = CandidateTimeAnomaly.None };
            Rejected(CandidateApplicationProtocol.PrepareIntent(input, Budget()));
        }

        [Test]
        public void UnpairedSurrogatesAndOrderedNotesAreNotNormalized()
        {
            var input = ApplicationScenario.Simple(CandidateApplicationKind.InitializeProfile);
            input.PlayerId = "p\ud800";
            input.Context.SourceNotes = new List<string> { "a", "b", "a" };
            var first = Accept(CandidateApplicationProtocol.PrepareIntent(input, Budget()));
            Assert.AreEqual(input.PlayerId, first.PlayerId);
            CollectionAssert.AreEqual(new byte[] { 2, 0, 0, 0, 112, 0, 0, 216 }, first.CanonicalBytes.Skip(12).Take(8));
            input.Context.SourceNotes = new List<string> { "b", "a", "a" };
            Assert.IsFalse(first.CanonicalBytes.SequenceEqual(Accept(CandidateApplicationProtocol.PrepareIntent(input, Budget())).CanonicalBytes));
        }

        [Test]
        public void RollbackRequiresNonemptyOrdinalUniqueConfirmation()
        {
            var input = ApplicationScenario.Simple(CandidateApplicationKind.Rollback);
            input.Rollback.ConfirmedRemovedOperationIds = new string[0];
            Rejected(CandidateApplicationProtocol.PrepareIntent(input, Budget()));
            input.Rollback.ConfirmedRemovedOperationIds = new[] { "a", "a" };
            Rejected(CandidateApplicationProtocol.PrepareIntent(input, Budget()));
            input.Rollback.ConfirmedRemovedOperationIds = new[] { "a", "A" };
            Accept(CandidateApplicationProtocol.PrepareIntent(input, Budget()));
        }
    }
}
