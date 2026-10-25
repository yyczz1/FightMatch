using System.Collections.Generic;
using System.Numerics;
using FlowPuzzle.Core;
using NUnit.Framework;
using static FightMatch.Core.Tests.ChapterBattleTestData;

namespace FightMatch.Core.Tests
{
    [TestFixture]
    public sealed class ChapterMageBattleTests
    {
        [Test]
        public void ChapterMageUsesMagicDefenseAndRetainsThreeDistinctDamageValues()
        {
            var binding = Bind(); var before = binding.Start.Snapshot;
            var result = CandidateDirectAttack.Evaluate(binding, before, Attack(before), new RandomSamplingBudget(Math()));
            Assert.IsTrue(result.IsAccepted, result.FieldPath);
            var hit = result.Fact;
            Assert.AreEqual(EntryDamageKind.Magic, hit.DamageKind); Assert.IsNull(hit.Crit);
            Assert.IsFalse(hit.Penetration.Triggered, "PCG reference first word modulo10 is3; q is1/10.");
            Value(hit.Attack, 616, 25); Value(hit.PhysicalDefense, 200); Value(hit.MagicDefense, 5);
            Value(hit.EffectiveMagicDefense, 5); Value(hit.MitigatedDamage, 352, 15);
            Assert.AreEqual(new BigInteger(23), hit.RoundedDamage); Value(hit.HpLoss, 20); Value(hit.Overflow, 3);
            Assert.AreEqual(BigInteger.One, result.Frame.Random.PrdStates[0].FailureCount);
            Assert.IsTrue(CandidateChapterBattleRules.ValidateDirectFrame(result.Frame, Math()));
        }

        [Test]
        public void TriggeredPenetrationChangesDefenseBeforeFloorAndResetsItsOwnCounter()
        {
            var input = ChapterBattleTestData.Input(warrior: true); input.Members[1].Penetration.C = R(1);
            var binding = Bind(input); var before = binding.Start.Snapshot;
            var result = CandidateDirectAttack.Evaluate(binding, before, Attack(before), new RandomSamplingBudget(Math()));
            Assert.IsTrue(result.IsAccepted, result.FieldPath); Assert.IsTrue(result.Fact.Penetration.Triggered);
            Value(result.Fact.EffectiveMagicDefense, 5, 2); Value(result.Fact.MitigatedDamage, 4928, 205);
            Assert.AreEqual(new BigInteger(24), result.Fact.RoundedDamage);
            Assert.AreEqual(BigInteger.Zero, result.Frame.Random.PrdStates[1].FailureCount);
            Assert.AreSame(before.Random.PrdStates[0], result.Frame.Random.PrdStates[0]);
            Assert.IsEmpty(result.Fact.Penetration.Words); Assert.AreEqual(before.Random.Stream.WordsConsumed, result.Frame.Random.Stream.WordsConsumed);
        }

        [Test]
        public void FloorZeroNormalMagicHitStillCreatesExactlyOneOpportunity()
        {
            var input = ChapterBattleTestData.Input(); input.Members[0].Stats.Attack = R(1, 10);
            var binding = Bind(input); var before = binding.Start.Snapshot;
            var result = CandidateDirectAttack.Evaluate(binding, before, Attack(before), new RandomSamplingBudget(Math()));
            Assert.IsTrue(result.IsAccepted, result.FieldPath); Value(result.Fact.HpLoss, 0);
            Assert.NotNull(result.Fact.Penetration); Assert.AreEqual(BigInteger.One, result.Frame.Random.PrdStates[0].FailureCount);
            Assert.AreEqual(BigInteger.One, result.Frame.Random.Stream.WordsConsumed);
            var complete = Act(Run(binding), 0, "zero-hit");
            Assert.AreEqual(BigInteger.One, complete.CurrentSnapshot.EffectiveActionsCompleted);
            Value(complete.CurrentSnapshot.Contributions[0].EffectiveDamageDealtHp, 0);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void InvalidRouteOrRangeDoesNotConsumeAnOpportunity(bool invalidRoute)
        {
            var binding = Bind(ChapterBattleTestData.Input(3)); var before = binding.Start.Snapshot;
            var request = Attack(before, invalidRoute ? 0 : 2);
            if (invalidRoute) request.Route = new List<FlowPos> { new FlowPos(0, 0), new FlowPos(2, 0) };
            var budget = new RandomSamplingBudget(Math());
            var result = CandidateDirectAttack.Evaluate(binding, before, request, budget);
            Assert.IsFalse(result.IsAccepted);
            Assert.AreEqual(invalidRoute ? CandidateAttackRejectionCode.InvalidRoute : CandidateAttackRejectionCode.OutOfRange, result.RejectionCode);
            Assert.AreEqual(0, budget.WordsUsed); Assert.AreEqual(BigInteger.Zero, before.Random.PrdStates[0].FailureCount);
            Assert.AreEqual(BigInteger.Zero, before.Random.Stream.WordsConsumed);
        }

        [Test]
        public void RangeCountsLivingEnemiesAndCompleteReportAcceptsMageContributions()
        {
            var run = Run(Bind(ChapterBattleTestData.Input(3)));
            run = Act(run, 0, "first");
            run = Act(run, 2, "third-now-second-living");
            run = Act(run, 1, "last");
            Assert.AreEqual(BattlePhase.WonPendingSettlement, run.CurrentSnapshot.Phase);
            Assert.AreEqual(3, run.Records.Count); Assert.NotNull(run.FinalReport);
            Value(run.CurrentSnapshot.Contributions[0].EffectiveDamageDealtHp, 60);
            var checks = new RewardChecks(Math());
            Assert.IsTrue(new RewardReportChecks(checks).Report(run.FinalReport), checks.Code + " " + checks.Path);
            Assert.AreEqual(run.FinalReport.Fingerprint, CandidateBattleReportFingerprint.Compute(run.FinalReport, Math()));
        }

        [Test]
        public void MixedPartyKeepsWarriorCriticalAndMagePenetrationIndependent()
        {
            var input = ChapterBattleTestData.Input(2, true); input.Level.Faces[0].Pairs[0].Enemy.Stats.PhysicalDefense = R(0);
            var run = Run(Bind(input));
            run = Act(run, 0, "warrior-hit", "warrior");
            var warriorFailures = run.CurrentSnapshot.Random.PrdStates[0].FailureCount;
            Assert.IsNotNull(run.Records[0].DirectAttack.DamageFacts[0].Crit);
            Assert.IsNull(run.Records[0].DirectAttack.DamageFacts[0].Penetration);
            run = Act(run, 1, "mage-hit");
            Assert.AreEqual(warriorFailures, run.CurrentSnapshot.Random.PrdStates[0].FailureCount);
            Assert.IsNotNull(run.Records[1].DirectAttack.DamageFacts[0].Penetration);
            var checks = new RewardChecks(Math());
            Assert.IsTrue(new RewardReportChecks(checks).Report(run.FinalReport), checks.Code + " " + checks.Path);
        }

        [Test]
        public void LegacyRuleRejectsMageAndMixedPassiveDefinitionsRemainInvalid()
        {
            Assert.AreEqual(BattleEntryRejectionCode.UnsupportedBinding, Prepare(ChapterBattleTestData.Input(rule: "demo-r1")).RejectionCode);
            var input = ChapterBattleTestData.Input(); input.Members[0].Crit = new WarriorCritInput { PassiveDefinitionId = "crit", TargetProbability = R(1), C = R(1), Multiplier = R(2) };
            Assert.IsFalse(Prepare(input).IsAccepted);
            input.Members[0].Crit = null; input.Members[0].Penetration = null;
            Assert.AreEqual(BattleEntryRejectionCode.MissingField, Prepare(input).RejectionCode);
        }

        [Test]
        public void RetainedMageEvidenceRejectsAChangedEffectiveDefenseWithoutSampling()
        {
            var binding = Bind(); var before = binding.Start.Snapshot;
            var result = CandidateDirectAttack.Evaluate(binding, before, Attack(before), new RandomSamplingBudget(Math()));
            Assert.IsTrue(result.IsAccepted, result.FieldPath); var original = result.Fact;
            var altered = new BattleDamageFact(before, result.Frame.Action, before.Enemies[0], original.Attack, original.Multiplier,
                original.RawDamage, original.MitigatedDamage, original.RoundedDamage, original.HpAfter, original.HpLoss,
                original.Overflow, R(0), null, original.Penetration, R(0));
            var frame = new CandidateCombatFrame(binding, before, result.Frame.Action, result.Frame.Enemies, result.Frame.Random, result.Frame.Contributions, altered);
            Assert.IsFalse(CandidateChapterBattleRules.ValidateDirectFrame(frame, Math()));
            Assert.IsFalse(CandidateBattleReportFingerprint.Equal(original, altered, Math()));
            Assert.AreEqual(BigInteger.Zero, before.Random.Stream.WordsConsumed);
        }

        [Test]
        public void ChapterMageGrowthComputesItsOwnProbabilityWithoutWarriorCrit()
        {
            var input = new GrowthDefinitionInput { Context = ChapterBattleTestData.Input().Context, ClassId = "mage", ClassKind = CharacterClassKind.Mage,
                PassiveDefinitionId = "fixture:penetration", BaseStats = Stats(72, R(22), 2), GrowthHp = R(2, 25), GrowthAttack = R(3, 50),
                GrowthDefense = R(1), PenetrationBase = R(1, 5), PenetrationStep = R(1, 200), PenetrationCap = R(3, 10), IgnoreFraction = R(1, 2),
                XpBase = 30, XpLinear = 15, XpQuadratic = 5, RecoveryDurationMilliseconds = R(300000) };
            var definition = CandidateCharacterGrowth.PrepareDefinition(input, Math()); Assert.IsTrue(definition.IsAccepted, definition.FieldPath);
            var created = CandidateCharacterGrowth.CreateCandidate(definition.Definition, "player", "mage", 3, 0, 0, Math());
            Assert.IsTrue(created.IsAccepted, created.FieldPath);
            Value(CandidateCharacterGrowth.ComputeBaseStats(created.Next, Math()).Stats.Attack, 616, 25);
            Assert.IsNull(CandidateCharacterGrowth.ComputeBaseStats(created.Next, Math()).TargetProbability);
            Value(CandidateCharacterGrowth.ComputePenetrationProbability(created.Next, Math()), 21, 100);
        }

        [Test]
        public void MageRollbackRestoresRandomAndRecordedReplayRetainsSupersededExactEvidence()
        {
            var run = Run(Bind(ChapterBattleTestData.Input(3)));
            var initial = run.CurrentSnapshot;
            var created = CandidateHistoryOperations.CreateCandidate(run, Math());
            Assert.IsTrue(created.IsAccepted, created.FieldPath);
            var nextRun = Act(run, 0, "recorded-mage-hit");
            var appended = CandidateHistoryOperations.Append(created.Next, nextRun, "anchor:mage", Math());
            Assert.IsTrue(appended.IsAccepted, appended.RejectionCode + " " + appended.FieldPath);
            var range = CandidateHistoryOperations.ReadRange(appended.Next, new CandidateHistoryRangeRequest {
                PlayerId = "player", AttemptId = "attempt", ExpectedSceneRevision = nextRun.CurrentSnapshot.SceneRevision,
                HistoryAnchorId = "anchor:mage" }, Math());
            Assert.IsTrue(range.IsAccepted, range.FieldPath);
            var rollback = CandidateHistoryOperations.PrepareRollback(appended.Next, new CandidateRollbackRequest {
                PlayerId = "player", AttemptId = "attempt", ExpectedSceneRevision = nextRun.CurrentSnapshot.SceneRevision,
                HistoryAnchorId = "anchor:mage", OperationId = "rollback:mage" }, range.Range, Math());
            Assert.IsTrue(rollback.IsAccepted, rollback.RejectionCode + " " + rollback.FieldPath);
            var restored = rollback.Next.CurrentRun.CurrentSnapshot;
            Assert.IsTrue(CandidateBattleReportFingerprint.Equal(initial.Random, restored.Random, Math()));
            Value(restored.Enemies[0].Hp, 20); Value(restored.Contributions[0].EffectiveDamageDealtHp, 0);
            var isolated = CandidateReplayOperations.BuildIsolationInput(rollback.Next, new CandidateIsolationRequest {
                PlayerId = "player", AttemptId = "attempt", ExpectedSceneRevision = restored.SceneRevision,
                Purpose = CandidateIsolationPurpose.RecordedOperation, RightsMode = CandidateIsolationRights.Empty,
                RecordedOperationId = "recorded-mage-hit" }, Math());
            Assert.IsTrue(isolated.IsAccepted, isolated.RejectionCode + " " + isolated.FieldPath);
            var replay = CandidateReplayOperations.ReplayRecorded(isolated.Input, new RandomSamplingBudget(Math()));
            Assert.IsTrue(replay.IsAccepted, replay.RejectionCode + " " + replay.FieldPath);
            Assert.AreEqual(true, replay.Matched, replay.FirstDivergence?.FieldPath);
            Assert.IsNotNull(replay.ActualRecords[0].DirectAttack.DamageFacts[0].Penetration);
            Assert.AreSame(restored, rollback.Next.CurrentRun.CurrentSnapshot);
            Assert.AreEqual(BigInteger.Zero, restored.Random.Stream.WordsConsumed);
        }
    }
}
