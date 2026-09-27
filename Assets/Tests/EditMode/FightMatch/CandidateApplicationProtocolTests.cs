using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using NUnit.Framework;
using static FightMatch.Core.Tests.BusinessSaveScenario;

namespace FightMatch.Core.Tests
{
    [TestFixture]
    public class CandidateApplicationProtocolTests
    {
        [Test]
        public void ThreeGenerationsBindOnlyAtEncodeAndCompleteTheSixSliceParentDigest()
        {
            var s = new ApplicationScenario();
            var first = s.Head;
            Assert.AreEqual(BigInteger.One, first.Header.SaveGeneration);
            Assert.IsNull(first.Header.ParentCommitId);
            Assert.IsNull(first.Header.CommitIndex[0].SnapshotLength);
            s.Enter();
            var second = s.Head;
            Assert.AreEqual(new BigInteger(2), second.Records.Last().Generation);
            Assert.AreEqual(first.Descriptor.TotalLength, second.Header.CommitIndex[0].SnapshotLength);
            CollectionAssert.AreEqual(first.Descriptor.Sha256, second.Header.CommitIndex[0].SnapshotSha256);
            Assert.IsNull(first.Header.CommitIndex[0].SnapshotLength);
            s.Attack("a", 0);
            Assert.AreEqual(new BigInteger(3), s.Head.Header.SaveGeneration);
            Assert.AreEqual(second.Header.CommitId, s.Head.Header.ParentCommitId);
            Assert.AreEqual(second.Descriptor.TotalLength, s.Head.Header.CommitIndex[1].SnapshotLength);
            CollectionAssert.AreEqual(second.Descriptor.Sha256, s.Head.Header.CommitIndex[1].SnapshotSha256);
            Assert.IsNull(s.Head.Header.CommitIndex[2].SnapshotLength);
            Assert.IsFalse(s.Candidate.CommitEligible);
            Assert.IsFalse(s.Head.CommitEligible);
            Assert.Throws<NotSupportedException>(() => ((IList<CandidateApplicationRecord>)s.Head.Records).Clear());
            Assert.Throws<NotSupportedException>(() => ((IList<string>)s.Head.Header.CommitIndex[0].OperationIds).Clear());
        }

        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)] [TestCase(5)] [TestCase(6)]
        [TestCase(7)] [TestCase(8)] [TestCase(9)] [TestCase(10)] [TestCase(11)] [TestCase(12)]
        public void ChangedIndexHistoryAndFalseHeaderAreRejectedWithoutChangingTheBasis(int mutation)
        {
            var s = new ApplicationScenario();
            s.Enter();
            var basis = s.Head;
            var input = s.AttackInput("a", 0);
            s.Domain.Attack("a", 0);
            var candidate = Accept(CandidateApplicationProtocol.Propose(basis, Prepare(s.Domain),
                Accept(CandidateApplicationProtocol.PrepareIntent(input, Budget())), new CandidateApplicationResultInput { HistoryAnchorId = "anchor:a" }, null, Budget()));
            var good = ApplicationScenario.Header(basis, "a");
            var rows = good.CommitIndex.ToList();
            var generation = good.SaveGeneration;
            var commit = good.CommitId;
            var parent = good.ParentCommitId;
            if (mutation == 0) rows.RemoveAt(0);
            if (mutation == 1) rows.Add(rows[0]);
            if (mutation == 2) rows.Reverse();
            if (mutation == 3) parent = "wrong";
            if (mutation == 4) generation++;
            if (mutation == 5) commit = basis.Header.CommitId;
            var row = rows[0];
            if (mutation == 6) rows[0] = new SaveCommitIndexEntry(row.Generation, row.CommitId, "wrong", row.SnapshotLength, row.SnapshotSha256, row.OperationIds);
            if (mutation == 7) rows[0] = new SaveCommitIndexEntry(row.Generation + 1, row.CommitId, row.ParentCommitId, row.SnapshotLength, row.SnapshotSha256, row.OperationIds);
            if (mutation == 8) rows[0] = new SaveCommitIndexEntry(row.Generation, "other", row.ParentCommitId, row.SnapshotLength, row.SnapshotSha256, row.OperationIds);
            if (mutation == 9) rows[0] = new SaveCommitIndexEntry(row.Generation, row.CommitId, row.ParentCommitId, row.SnapshotLength + 1, row.SnapshotSha256, row.OperationIds);
            if (mutation == 10)
            {
                var current = rows[1];
                rows[1] = new SaveCommitIndexEntry(current.Generation, current.CommitId, current.ParentCommitId, current.SnapshotLength, new byte[32], current.OperationIds);
            }
            if (mutation == 11) rows[2] = new SaveCommitIndexEntry(3, good.CommitId, good.ParentCommitId, null, null, new[] { "a", "extra" });
            if (mutation == 12) rows[2] = new SaveCommitIndexEntry(3, good.CommitId, good.ParentCommitId, 1, new byte[32], new[] { "a" });
            Rejected(CandidateApplicationSaveCodec.Encode(candidate, new CandidateBusinessSaveHeader(generation, commit, parent, rows), Budget()));
            Assert.AreSame(basis, s.Head);
            Assert.AreEqual(2, basis.Records.Count);
            Assert.IsNull(basis.Header.CommitIndex[1].SnapshotLength);
        }

        [Test]
        public void OldResultWinsAfterLevelAndAttemptChangeAndNewStaleOperationDoesNot()
        {
            var s = new ApplicationScenario(entryLevel: 4);
            var init = s.Intents["init"];
            s.Enter();
            var entryCommit = s.Head.Header.CommitId;
            s.Win();
            var closing = s.Head.Continuation.ClosingOperationId;
            var settling = s.Head.Continuation.ReservedOperationId;
            s.Settle();
            Assert.Greater(s.Domain.Character.Level, new BigInteger(4));
            s.Enter("enter2");
            var result = s.Lookup("enter");
            Assert.AreEqual(entryCommit, result.OriginalCommitId);
            Assert.AreEqual(new BigInteger(2), result.OriginalGeneration);
            Assert.AreEqual(new BigInteger(4), result.Baseline.Entry.Members[0].Level);
            Assert.AreSame(result.Binding.Start.Snapshot, result.InitialSnapshot);
            Assert.AreNotEqual(result.Begin.AttemptId, s.Domain.EntryInput.AttemptId);
            Assert.IsNotNull(s.Lookup(settling).Reward);
            Assert.IsNotNull(s.Lookup(closing).BattleOperation);
            Assert.AreEqual(new BigInteger(4), s.Lookup("init").Initialization.InitialLevel);
            Assert.IsTrue(s.Lookup("init").Initialization.Created);
            Rejected(CandidateApplicationProtocol.Propose(s.Head, Prepare(s.Domain), init, new CandidateApplicationResultInput(), null, Budget()), "OperationAlreadyRecorded");
            var changed = CandidateApplicationIntentCodec.Read(init.CanonicalBytes.ToArray(), Budget()).Data;
            changed.InitializeProfile.InitialLevel++;
            Rejected(CandidateApplicationProtocol.Lookup(s.Head, Accept(CandidateApplicationProtocol.PrepareIntent(changed, Budget())), Budget()), "OperationConflict");
            var unknown = s.AttackInput("unknown", 0);
            var prepared = Accept(CandidateApplicationProtocol.PrepareIntent(unknown, Budget()));
            Assert.IsFalse(Accept(CandidateApplicationProtocol.Lookup(s.Head, prepared, Budget())).IsFound);
            unknown.ExpectedCommitId = entryCommit;
            Rejected(CandidateApplicationProtocol.Propose(s.Head, Prepare(s.Domain), Accept(CandidateApplicationProtocol.PrepareIntent(unknown, Budget())),
                new CandidateApplicationResultInput { HistoryAnchorId = "anchor:unknown" }, null, Budget()), "StaleContext");
        }

        [Test]
        public void TwoRealRollbacksKeepFirstSupersededRelationsAcrossExitNewAttemptAndRestart()
        {
            var s = new ApplicationScenario(3);
            s.Enter();
            s.Attack("a0", 0);
            s.Attack("a1", 0);
            Assert.IsEmpty(s.Head.Business.RetainedRuns);
            s.Rollback("r1", "a1");
            var r1Revision = s.Lookup("r1").Rollback.SceneRevision;
            s.Attack("branch", 1);
            s.Rollback("r2", "a0");
            s.Exit();
            Assert.AreEqual(1, s.Head.Business.RetainedRuns.Count);
            Assert.AreEqual(2, s.Head.Business.RetainedRollbacks.Count);
            s.Enter("enter2");
            s.Attack("effective", 0);
            s.Restart();
            var a1 = s.Lookup("a1");
            Assert.AreEqual(CandidateApplicationRelation.Superseded, a1.Relation);
            Assert.AreEqual("r1", a1.SupersededBy.OperationId);
            Assert.AreEqual(r1Revision, a1.SupersededBy.SceneRevision);
            Assert.AreEqual("r2", s.Lookup("a0").SupersededBy.OperationId);
            Assert.AreEqual("r2", s.Lookup("branch").SupersededBy.OperationId);
            Assert.AreEqual(CandidateApplicationRelation.RollbackRecorded, s.Lookup("r1").Relation);
            Assert.AreEqual(CandidateApplicationRelation.Effective, s.Lookup("effective").Relation);
            var exit = s.Lookup("exit");
            Assert.AreEqual(exit.End.EndReceiptId, exit.CharacterEnd.EndReceiptId);
            Assert.AreEqual(exit.End.EndReceiptId, exit.InventoryEnd.EndReceiptId);
            Assert.AreEqual(exit.Begin.AttemptId, exit.TerminalRun.Baseline.Entry.AttemptId);
            var restart = s.Lookup("restart");
            Assert.AreEqual(restart.End.NewAttemptId, restart.NewBegin.AttemptId);
            Assert.AreEqual(restart.NewBegin.AttemptId, restart.NewCarry.AttemptId);
            Assert.AreEqual(restart.NewBegin.AttemptId, restart.NewBinding.Start.Baseline.Entry.AttemptId);
            Assert.AreEqual(restart.Begin.ChallengeId, restart.NewBegin.ChallengeId);
            Assert.AreEqual(restart.Begin.EntryBaselineId, restart.NewBegin.EntryBaselineId);
            Assert.AreEqual(2, s.Head.Business.RetainedRuns.Count);
            Assert.AreEqual(2, s.Head.Business.RetainedRollbacks.Count);
        }

        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)] [TestCase(5)] [TestCase(6)]
        public void OneActionCannotHideOtherOperationsForeignOwnersOrFalseAnchors(int mutation)
        {
            var s = new ApplicationScenario(3);
            s.Enter();
            var input = s.AttackInput("a", 0);
            s.Domain.Attack("a", 0);
            var result = new CandidateApplicationResultInput { HistoryAnchorId = "anchor:a" };
            if (mutation == 0) s.Domain.Attack("extra", 1);
            if (mutation == 1) result.HistoryAnchorId = "false";
            if (mutation == 2) input.Attack.ExpectedSceneRevision++;
            if (mutation == 3) input.Attack.ExpectedPreferenceRevision++;
            if (mutation == 4) input.Attack.ItemUseEnabled = false;
            if (mutation == 5) result.SettlementId = "extraneous";
            if (mutation == 6) s.Domain.Character = BusinessSaveScenario.RoundTrip(Prepare(s.Domain)).Character;
            Rejected(CandidateApplicationProtocol.Propose(s.Head, Prepare(s.Domain), Accept(CandidateApplicationProtocol.PrepareIntent(input, Budget())), result, null, Budget()));
            Assert.AreEqual(2, s.Head.Records.Count);
        }

        [Test]
        public void RecoveryRecordsKeepRealFourOutcomesAndHistoricalSampleAndBothAnomalies()
        {
            var s = new ApplicationScenario(3, hp: 1);
            s.Enter();
            s.Attack("down", 0);
            s.Exit(down: true);
            var initial = s.Domain.Character.RecoveryPeriods[0];
            var sample = ApplicationScenario.Clock(-500, R(1, 3));
            var applied = s.Recover("advance", sample);
            Assert.AreEqual(CandidateGrowthOutcome.Applied, applied.Outcome);
            Same(R(1, 3), applied.RecoveryPeriod.Elapsed);
            var revision = applied.Next.StateRevision;
            Assert.AreEqual(CandidateGrowthOutcome.Unchanged, s.Recover("same", sample).Outcome);
            var ignored = s.Recover("backward", ApplicationScenario.Clock(-1000));
            Assert.AreEqual(CandidateGrowthOutcome.IgnoredTimeRegression, ignored.Outcome);
            Assert.AreEqual(CandidateTimeAnomaly.ClockBackward | CandidateTimeAnomaly.DomainChanged, ignored.Anomaly);
            Assert.AreEqual(CandidateTimeAnomaly.None, ignored.RecoveryPeriod.Anomaly);
            s.Recover("later", ApplicationScenario.Clock(-100, R(2, 3)));
            s.Recover("complete", ApplicationScenario.Clock(180100));
            var hugeStale = BigInteger.One << 80;
            Assert.AreEqual(CandidateGrowthOutcome.AlreadyIncluded, s.Recover("included", ApplicationScenario.Clock(-900), hugeStale).Outcome);
            var old = s.Lookup("advance").Recovery;
            Assert.AreEqual(revision, old.AfterCharacterRevision);
            Assert.AreEqual(CandidateGrowthOutcome.Applied, old.Outcome);
            Assert.AreEqual(new BigInteger(-500), old.Period.LastAcceptedSample.WallUtcMilliseconds);
            Same(R(1, 3), old.Period.Elapsed);
            Assert.IsFalse(old.Period.IsCompleted);
            Assert.IsTrue(s.Domain.Character.RecoveryPeriods[0].IsCompleted);
            Assert.AreSame(s.Domain.Character.ProcessedEnds[0], old.Period.EndReceipt);
            Assert.AreEqual(initial.EndReceipt.EndReceiptId, old.EndReceiptId);
            Assert.AreEqual(CandidateTimeAnomaly.None, s.Lookup("backward").Recovery.Period.Anomaly);
            Assert.AreEqual(ignored.Anomaly, s.Lookup("backward").Recovery.ResultAnomaly);
            var included = s.Lookup("included").Recovery;
            Assert.AreEqual(CandidateTimeAnomaly.DomainChanged, included.Period.Anomaly);
            Assert.AreEqual(CandidateTimeAnomaly.None, included.ResultAnomaly);
        }

        [Test]
        public void RealIgnoredRecoveryCannotCompleteAnIntentWhoseTimeWouldAdvance()
        {
            var s = new ApplicationScenario(3, hp: 1);
            s.Enter();
            s.Attack("down", 0);
            s.Exit(down: true);
            var basis = s.Head;
            var character = basis.Business.Character;
            var period = character.RecoveryPeriods[0];
            Assert.AreEqual(new BigInteger(2), character.StateRevision);
            Assert.AreEqual(new BigInteger(100), period.LastAcceptedSample.WallUtcMilliseconds);
            var actual = CandidateRecoveryClock.Advance(character, "recovery", ApplicationScenario.Clock(99), character.StateRevision, Math());
            Assert.IsTrue(actual.IsAccepted, actual.FieldPath);
            Assert.AreEqual(CandidateGrowthOutcome.IgnoredTimeRegression, actual.Outcome);
            Assert.AreSame(character, actual.Next);
            Assert.AreSame(period, actual.RecoveryPeriod);
            Assert.AreEqual(CandidateTimeAnomaly.ClockBackward | CandidateTimeAnomaly.DomainChanged, actual.Anomaly);
            var time = ApplicationScenario.Clock(101);
            var oracle = CandidateRecoveryClock.Advance(character, "recovery", time, character.StateRevision, Math());
            Assert.IsTrue(oracle.IsAccepted, oracle.FieldPath);
            Assert.AreEqual(CandidateGrowthOutcome.Applied, oracle.Outcome);
            Assert.AreEqual(new BigInteger(3), oracle.Next.StateRevision);
            Same(R(1), oracle.RecoveryPeriod.Elapsed);
            var input = s.Intent(CandidateApplicationKind.AdvanceRecovery, "recover-future");
            input.AdvanceRecovery = new CandidateApplicationRecoveryInput { CharacterId = character.CharacterId, RecoveryId = "recovery",
                ExpectedCharacterRevision = character.StateRevision, TimeSample = time };
            var prepared = Accept(CandidateApplicationProtocol.PrepareIntent(input, Budget()));
            Rejected(CandidateApplicationProtocol.Propose(basis, basis.Business, prepared,
                new CandidateApplicationResultInput { RecoveryResult = actual }, null, Budget()), "InconsistentBinding");
            Assert.AreSame(basis, s.Head);
            Assert.AreEqual(4, basis.Records.Count);
            Assert.AreSame(character, actual.Next);
            Assert.AreSame(period, actual.RecoveryPeriod);
            Same(R(0), period.Elapsed);
            Assert.IsFalse(Accept(CandidateApplicationProtocol.Lookup(basis, prepared, Budget())).IsFound);
        }

        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)]
        public void IgnoredRecoveryRejectsSameSampleOrNonDecreasingMonotonicTime(int kind)
        {
            var s = RecoveryScenario();
            if (kind >= 2) s.Recover("prime", ApplicationScenario.Clock(-500, R(2, 3)));
            var basis = s.Head;
            var character = basis.Business.Character;
            var source = kind < 2 ? ApplicationScenario.Clock(99) : ApplicationScenario.Clock(900, R(1, 3));
            var actual = CandidateRecoveryClock.Advance(character, "recovery", source, character.StateRevision, Math());
            Assert.IsTrue(actual.IsAccepted, actual.FieldPath);
            Assert.AreEqual(CandidateGrowthOutcome.IgnoredTimeRegression, actual.Outcome);
            var claimed = kind == 0 ? ApplicationScenario.Clock(100, R(0)) : kind == 1 ? ApplicationScenario.Clock(99, R(1, 3)) :
                kind == 2 ? ApplicationScenario.Clock(-600, R(1)) : ApplicationScenario.Clock(-600, R(2, 3));
            var oracle = CandidateRecoveryClock.Advance(character, "recovery", claimed, character.StateRevision, Math());
            Assert.IsTrue(oracle.IsAccepted, oracle.FieldPath);
            Assert.AreEqual(kind == 0 ? CandidateGrowthOutcome.Unchanged : CandidateGrowthOutcome.Applied, oracle.Outcome);
            var input = s.Intent(CandidateApplicationKind.AdvanceRecovery, "false-ignored");
            input.AdvanceRecovery = new CandidateApplicationRecoveryInput { CharacterId = character.CharacterId, RecoveryId = "recovery",
                ExpectedCharacterRevision = character.StateRevision, TimeSample = claimed };
            Rejected(CandidateApplicationProtocol.Propose(basis, basis.Business, Accept(CandidateApplicationProtocol.PrepareIntent(input, Budget())),
                new CandidateApplicationResultInput { RecoveryResult = actual }, null, Budget()), "InconsistentBinding");
            Assert.AreSame(basis, s.Head);
            Assert.AreSame(character, actual.Next);
            Assert.AreSame(character.RecoveryPeriods[0], actual.RecoveryPeriod);
        }

        [TestCase(false)] [TestCase(true)]
        public void IgnoredRecoveryRejectsMissingOrExtraAnomalyBitsFromAnotherRealResult(bool extra)
        {
            var s = RecoveryScenario();
            s.Recover("prime", ApplicationScenario.Clock(-500, R(2, 3)));
            var basis = s.Head;
            var character = basis.Business.Character;
            var source = ApplicationScenario.Clock(900, R(1, 3));
            source.Anomaly = extra ? CandidateTimeAnomaly.DomainChanged : CandidateTimeAnomaly.None;
            var actual = CandidateRecoveryClock.Advance(character, "recovery", source, character.StateRevision, Math());
            Assert.IsTrue(actual.IsAccepted, actual.FieldPath);
            Assert.AreEqual(CandidateGrowthOutcome.IgnoredTimeRegression, actual.Outcome);
            Assert.AreEqual(extra ? CandidateTimeAnomaly.ClockBackward | CandidateTimeAnomaly.DomainChanged : CandidateTimeAnomaly.ClockBackward, actual.Anomaly);
            var claimed = ApplicationScenario.Clock(900, R(1, 3));
            claimed.Anomaly = extra ? CandidateTimeAnomaly.None : CandidateTimeAnomaly.DomainChanged;
            var oracle = CandidateRecoveryClock.Advance(character, "recovery", claimed, character.StateRevision, Math());
            Assert.IsTrue(oracle.IsAccepted, oracle.FieldPath);
            Assert.AreEqual(CandidateGrowthOutcome.IgnoredTimeRegression, oracle.Outcome);
            Assert.AreNotEqual(oracle.Anomaly, actual.Anomaly);
            var input = s.Intent(CandidateApplicationKind.AdvanceRecovery, "wrong-anomaly");
            input.AdvanceRecovery = new CandidateApplicationRecoveryInput { CharacterId = character.CharacterId, RecoveryId = "recovery",
                ExpectedCharacterRevision = character.StateRevision, TimeSample = claimed };
            Rejected(CandidateApplicationProtocol.Propose(basis, basis.Business, Accept(CandidateApplicationProtocol.PrepareIntent(input, Budget())),
                new CandidateApplicationResultInput { RecoveryResult = actual }, null, Budget()), "InconsistentBinding");
            Assert.AreSame(basis, s.Head);
            Assert.AreSame(character.RecoveryPeriods[0], actual.RecoveryPeriod);
            Assert.AreEqual(CandidateTimeAnomaly.None, actual.RecoveryPeriod.Anomaly);
        }

        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)] [TestCase(5)]
        public void LegalIgnoredRecoveryKeepsNegativeWallAndExactDomainPriority(int kind)
        {
            var s = RecoveryScenario();
            s.Recover("prime", ApplicationScenario.Clock(-500, R(2, 3)));
            var original = s.Domain.Character.RecoveryPeriods[0];
            var time = kind == 0 ? ApplicationScenario.Clock(-600, R(1, 3)) : kind == 1 ? ApplicationScenario.Clock(900, R(1, 3)) :
                kind == 2 ? ApplicationScenario.Clock(-600, R(10), "clock-other") : kind == 3 ? ApplicationScenario.Clock(-600) :
                kind == 4 ? ApplicationScenario.Clock(-600, R(10), "Clock-original") : ApplicationScenario.Clock(-(BigInteger.One << 80));
            var actual = s.Recover("valid-ignored", time);
            Assert.AreEqual(CandidateGrowthOutcome.IgnoredTimeRegression, actual.Outcome);
            Assert.AreSame(original, actual.RecoveryPeriod);
            var restored = s.Lookup("valid-ignored").Recovery;
            Assert.AreEqual(CandidateGrowthOutcome.IgnoredTimeRegression, restored.Outcome);
            Assert.AreEqual(new BigInteger(-500), restored.Period.LastAcceptedSample.WallUtcMilliseconds);
            Assert.AreEqual("clock-original", restored.Period.LastAcceptedSample.MonotonicScopeId);
            Same(R(2, 3), restored.Period.LastAcceptedSample.MonotonicElapsedMilliseconds);
            Same(R(2, 3), restored.Period.Elapsed);
            Assert.AreEqual(CandidateTimeAnomaly.None, restored.Period.Anomaly);
            Assert.AreEqual(kind < 2 ? CandidateTimeAnomaly.ClockBackward : CandidateTimeAnomaly.ClockBackward | CandidateTimeAnomaly.DomainChanged,
                restored.ResultAnomaly);
        }

        private static ApplicationScenario RecoveryScenario()
        {
            var s = new ApplicationScenario(3, hp: 1);
            s.Enter();
            s.Attack("down", 0);
            s.Exit(down: true);
            return s;
        }

        [Test]
        public void VictoryContinuationSurvivesDecodeAndIsConsumedOnlyByItsSeparateSettlement()
        {
            var s = new ApplicationScenario();
            s.Enter();
            s.Win();
            var pending = s.Head;
            var route = pending.Continuation;
            Assert.AreEqual(CandidateApplicationContinuationStage.AwaitBaseSettlement, route.Stage);
            Assert.IsEmpty(pending.Business.Rewards.BaseRewards);
            var closing = s.Lookup(route.ClosingOperationId);
            Assert.IsNotNull(closing.BattleOperation);
            Assert.IsEmpty(s.Head.Business.Rewards.BaseRewards);
            s.Settle();
            Assert.IsNull(s.Head.Continuation);
            Assert.AreSame(route, pending.Continuation);
            var settlement = s.Lookup(route.ReservedOperationId);
            Assert.AreEqual(closing.OriginalGeneration + 1, settlement.OriginalGeneration);
            Assert.AreNotEqual(closing.OriginalCommitId, settlement.OriginalCommitId);
            Assert.AreEqual(route.FinalReportFingerprint, settlement.Reward.FinalReportFingerprint);
            Assert.AreSame(settlement.End, settlement.Reward.Ending);
            Assert.AreSame(s.Head.Business.Character.BaseRewards[0], settlement.CharacterExperience);
            Assert.AreSame(s.Head.Business.Inventory.OrdinaryGrants[0], settlement.InventoryGrant);
            Assert.AreEqual(settlement.End.EndReceiptId, settlement.CharacterEnd.EndReceiptId);
            Assert.AreEqual(settlement.End.EndReceiptId, settlement.InventoryEnd.EndReceiptId);
            Assert.AreEqual(1, s.Head.Business.Rewards.BaseRewards.Count);
        }

        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)]
        public void WrongSettlementIdentityOrMissingReceiptsCannotConsumeContinuation(int mutation)
        {
            var s = new ApplicationScenario();
            s.Enter();
            s.Win();
            var input = s.VictoryInput();
            var route = s.Head.Continuation;
            if (mutation == 0) input.OperationId = "unauthorized-substitute";
            if (mutation == 1) input.SettleVictory.FinalReportFingerprint = "wrong";
            if (mutation == 2) input.SettleVictory.TerminalOperationId = "wrong";
            if (mutation == 3) input.SettleVictory.AttemptId = "wrong";
            if (mutation != 4) s.Domain.EndVictory();
            var result = new CandidateApplicationResultInput { EndReceiptId = "end:" + route.AttemptId, SettlementId = "settlement:" + route.AttemptId };
            Rejected(CandidateApplicationProtocol.Propose(s.Head, Prepare(s.Domain), Accept(CandidateApplicationProtocol.PrepareIntent(input, Budget())), result, null, Budget()));
            Assert.AreSame(route, s.Head.Continuation);
            Assert.IsEmpty(s.Head.Business.Rewards.BaseRewards);
        }

        [TestCase(null)] [TestCase("init")] [TestCase("terminal")] [TestCase(" ")]
        public void TerminalActionRequiresOneFreshFixedReservedId(string reserved)
        {
            var s = new ApplicationScenario();
            s.Enter();
            s.Attack("first", 0);
            var input = s.AttackInput("terminal", 1);
            s.Domain.Attack("terminal", 1);
            Assert.IsNotNull(s.Domain.History.CurrentRun.FinalReport);
            Rejected(CandidateApplicationProtocol.Propose(s.Head, Prepare(s.Domain), Accept(CandidateApplicationProtocol.PrepareIntent(input, Budget())),
                new CandidateApplicationResultInput { HistoryAnchorId = "anchor:terminal" }, reserved, Budget()));
            Assert.IsNull(s.Head.Continuation);
        }

        [Test]
        public void TopLevelNullsThrowAndCompletedRecoveryCannotUseAnUnrelatedActualResult()
        {
            Assert.Throws<ArgumentNullException>(() => CandidateApplicationProtocol.PrepareIntent(null, Budget()));
            Assert.Throws<ArgumentNullException>(() => CandidateApplicationProtocol.PrepareIntent(ApplicationScenario.Simple(CandidateApplicationKind.Link), null));
            Assert.Throws<ArgumentNullException>(() => CandidateApplicationProtocol.Lookup(null, null, Budget()));
            Assert.Throws<ArgumentNullException>(() => CandidateApplicationSaveCodec.Encode(null, null, Budget()));
            Assert.Throws<ArgumentNullException>(() => CandidateApplicationSaveCodec.Decode(null, Budget()));
            var s = new ApplicationScenario(3, hp: 1);
            s.Enter();
            s.Attack("down", 0);
            s.Exit(down: true);
            var input = s.Intent(CandidateApplicationKind.AdvanceRecovery, "recovery");
            input.AdvanceRecovery = new CandidateApplicationRecoveryInput { CharacterId = s.Domain.Character.CharacterId, RecoveryId = "recovery",
                ExpectedCharacterRevision = s.Domain.Character.StateRevision, TimeSample = ApplicationScenario.Clock(102, R(2)) };
            var actual = CandidateRecoveryClock.Advance(s.Domain.Character, "recovery", input.AdvanceRecovery.TimeSample, s.Domain.Character.StateRevision, Math());
            Assert.IsTrue(actual.IsAccepted);
            Rejected(CandidateApplicationProtocol.Propose(s.Head, Prepare(s.Domain), Accept(CandidateApplicationProtocol.PrepareIntent(input, Budget())),
                new CandidateApplicationResultInput { RecoveryResult = actual }, null, Budget()), "InconsistentBinding");
        }
    }
}
