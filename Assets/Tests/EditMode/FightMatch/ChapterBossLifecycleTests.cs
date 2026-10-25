using System.Linq;
using System.Numerics;
using FightMatch.Application;
using NUnit.Framework;
using static FightMatch.Core.Tests.ChapterBattleTestData;
using static FightMatch.Core.Tests.PlayerSessionTestData;

namespace FightMatch.Core.Tests
{
    public sealed class ChapterBossLifecycleTests
    {
        [Test]
        public void ActiveWindAndCooldownColdRestoreThenFinalRewardAndSecondCertificateCommitOnce()
        {
            var fixture = new ChapterTeachingFixture(true);
            MemorySave storage;
            PublishedContentTestData.MemoryStorage locator;
            byte[] snapshot;
            using (var rig = new ChapterTeachingRig(fixture))
            {
                rig.CompleteTeaching();
                rig.Enter(16);
                rig.Act();
                var state = rig.Head.Business.ActiveHistory.CurrentRun.CurrentSnapshot;
                Assert.AreEqual(2, state.ChapterState.MemberSkills.Single().CooldownRemaining);
                Assert.AreEqual(1, state.Enemies[1].RemainingWindUses);
                Assert.NotNull(rig.Head.Business.ActiveHistory.CurrentRun.Records.Single().EnemyPhase.OrderedIntents[1].Wind);
                snapshot = CandidateBattleReportFingerprint.Encode(state, ChapterBattleTestData.Math());
                storage = rig.Storage;
                locator = rig.LocatorStorage;
            }
            using (var rig = new ChapterTeachingRig(fixture, storage, locator))
            {
                CollectionAssert.AreEqual(snapshot, CandidateBattleReportFingerprint.Encode(
                    rig.Head.Business.ActiveHistory.CurrentRun.CurrentSnapshot, ChapterBattleTestData.Math()));
                var attempt = rig.Head.Business.ActiveHistory.CurrentRun.Baseline.Entry.AttemptId;
                rig.Win();
                var run = rig.Head.Business.ActiveHistory.CurrentRun;
                Assert.AreEqual(1, run.CurrentSnapshot.CurrentFaceIndex);
                Assert.AreEqual(attempt, run.FinalReport.AttemptId);
                var request = Prepared(rig.Session.PrepareVictory(PlayerRosterTestData.Time(), Codec()));
                rig.Storage.Fault = "marker-after";
                Is(rig.Lifecycle.Submit(request, Budget()), "CommitUnknown");
                Is(rig.Lifecycle.Resolve(request, Budget()));
                Assert.AreEqual(BigInteger.One, rig.CertificateCount);
                Assert.AreEqual(1, rig.Head.Business.Rewards.BaseRewards.Count(x => x.Report.Level.LevelId == ChapterTeachingFixture.LevelId(16)));
                var files = CopyFiles(rig.Storage.Files);
                Is(rig.Lifecycle.Submit(request, Budget()));
                SameFiles(files, rig.Storage.Files);
                rig.Enter(16);
                rig.Win();
                rig.Settle();
                Assert.AreEqual(BigInteger.One, rig.CertificateCount, "A later attempt cannot grant the first-clear certificate again.");
            }
        }

        [Test]
        public void TwoFacesKeepOneAttemptAndRollbackRestoresSkillAndFaceStateForRecordedReplay()
        {
            var input = StateInput(1);
            input.Level.Faces[0].Pairs[0].Enemy.Stats.MaxHp = R(20);
            input.Members[0].Crit.C = R(1, 10);
            AddStateFace(input);
            var created = CandidateHistoryOperations.CreateCandidate(Run(Bind(input)), ChapterBattleTestData.Math());
            Assert.IsTrue(created.IsAccepted, created.FieldPath);
            var history = created.Next;
            for (var i = 0; i < 2; i++)
            {
                var run = Act(history.CurrentRun, 0, "face1:" + i, "warrior");
                var appended = CandidateHistoryOperations.Append(history, run, "anchor:" + i, ChapterBattleTestData.Math());
                Assert.IsTrue(appended.IsAccepted, appended.RejectionCode + " " + appended.FieldPath);
                history = appended.Next;
            }
            var crossed = history.CurrentRun.CurrentSnapshot;
            Assert.AreEqual(1, crossed.CurrentFaceIndex);
            Assert.AreEqual(BigInteger.Zero, crossed.ChapterState.FaceEnemyPhasesCompleted);
            Assert.AreEqual(new BigInteger(2), crossed.EnemyPhasesCompleted);
            Assert.AreEqual(1, crossed.ChapterState.MemberSkills.Single().CooldownRemaining);
            Assert.AreEqual("attempt", crossed.Baseline.Entry.AttemptId);
            Value(crossed.Members.Single(x => x.Member.CharacterId == "warrior").Hp, 995);
            Value(crossed.Enemies.Single().Hp, 100);
            Assert.AreEqual(BigInteger.Zero, crossed.Enemies.Single().IntentCursor);
            Assert.Greater(crossed.Random.Stream.WordsConsumed, BigInteger.Zero);
            Assert.AreSame(history.CurrentRun.Records.Last().EnemyPhase.Random, crossed.Random);
            Assert.IsEmpty(crossed.ChapterState.Taunts);
            Assert.IsEmpty(crossed.ChapterState.Vulnerabilities);
            var range = CandidateHistoryOperations.ReadRange(history, new CandidateHistoryRangeRequest { PlayerId = "player", AttemptId = "attempt",
                ExpectedSceneRevision = crossed.SceneRevision, HistoryAnchorId = "anchor:1" }, ChapterBattleTestData.Math());
            Assert.IsTrue(range.IsAccepted, range.FieldPath);
            var rolled = CandidateHistoryOperations.PrepareRollback(history, new CandidateRollbackRequest { PlayerId = "player", AttemptId = "attempt",
                ExpectedSceneRevision = crossed.SceneRevision, HistoryAnchorId = "anchor:1", OperationId = "rollback" }, range.Range, ChapterBattleTestData.Math());
            Assert.IsTrue(rolled.IsAccepted, rolled.FieldPath);
            history = rolled.Next;
            Assert.AreEqual(0, history.CurrentRun.CurrentSnapshot.CurrentFaceIndex);
            Assert.AreEqual(2, history.CurrentRun.CurrentSnapshot.ChapterState.MemberSkills.Single().CooldownRemaining);
            Assert.AreEqual(BigInteger.One, history.CurrentRun.CurrentSnapshot.ChapterState.FaceEnemyPhasesCompleted);
            var isolation = CandidateReplayOperations.BuildIsolationInput(history, new CandidateIsolationRequest { PlayerId = "player", AttemptId = "attempt",
                ExpectedSceneRevision = history.CurrentRun.CurrentSnapshot.SceneRevision, Purpose = CandidateIsolationPurpose.RecordedOperation,
                RightsMode = CandidateIsolationRights.Empty, RecordedOperationId = "face1:1" }, ChapterBattleTestData.Math());
            Assert.IsTrue(isolation.IsAccepted, isolation.RejectionCode + " " + isolation.FieldPath);
            var replay = CandidateReplayOperations.ReplayRecorded(isolation.Input, new RandomSamplingBudget(ChapterBattleTestData.Math()));
            Assert.IsTrue(replay.IsAccepted, replay.RejectionCode + " " + replay.FieldPath);
            Assert.AreEqual(CandidateReplayOutcome.Matched, replay.Outcome);
            Assert.IsTrue(CandidateBattleReportFingerprint.Equal(crossed, replay.Run.CurrentSnapshot, ChapterBattleTestData.Math()));
        }

        [Test]
        public void ActualR3AuthoringNormalNewProfileReachesTeachingAndBothBossFacesThroughApplication()
        {
            // Only package/draft/release locator identity is fixture scoped. Authoring values, normal unlocks and routes remain r3.
            using (var rig = new ChapterTeachingRig(new ChapterTeachingFixture(actualAuthoring: true)))
            {
                Assert.AreEqual(BigInteger.One, rig.Head.Business.Roster.Find(rig.WarriorId).Level);
                for (var level = 1; level <= 15; level++)
                {
                    rig.Enter(level);
                    rig.Win(true);
                    rig.Settle();
                    if (level == 8) rig.AddMageToFormation();
                    if (level == 4 || level == 8 || level == 12) rig.TryUseCardsToNextLevel();
                    var warrior = rig.Head.Business.Roster.Find(rig.WarriorId);
                    TestContext.WriteLine("r3 cleared " + level + ": warrior Lv" + warrior.Level + " XP " + warrior.Experience +
                        "; commit " + rig.Head.Header.CommitId);
                }
                rig.TryUseCardsToNextLevel();
                Assert.GreaterOrEqual(rig.Head.Business.Roster.Find(rig.WarriorId).Level, new BigInteger(5),
                    "The real rewards/cards must satisfy the teaching level gate; this test does not inject levels.");
                Assert.AreEqual("TeachingGiftRequired", rig.PrepareEntry(16).Code);
                rig.CompleteTeaching();
                Assert.AreEqual(BigInteger.Zero, rig.CertificateCount);
                rig.Enter(16);
                var attempt = rig.Head.Business.ActiveHistory.CurrentRun.Baseline.Entry.AttemptId;
                rig.Act(rig.WarriorId, 2);
                var opening = rig.Head.Business.ActiveHistory.CurrentRun.Records.Single().DirectAttack;
                Assert.IsEmpty(opening.DamageFacts);
                Assert.AreEqual(1, opening.SkillCasts.Count);
                rig.Win(true);
                var run = rig.Head.Business.ActiveHistory.CurrentRun;
                Assert.AreEqual(2, run.Baseline.Entry.Level.Faces.Count);
                Assert.AreEqual(1, run.CurrentSnapshot.CurrentFaceIndex);
                Assert.AreEqual(attempt, run.FinalReport.AttemptId);
                Assert.IsTrue(run.Records.Any(x => x.StageDecision.DidFlipFace));
                rig.Settle();
                Assert.AreEqual(BigInteger.One, rig.CertificateCount);
                Assert.IsTrue(rig.Head.Business.Progression.GetChapterEffects().Any(x => x.LevelId == ChapterTeachingFixture.LevelId(16)));
            }
        }
    }
}
