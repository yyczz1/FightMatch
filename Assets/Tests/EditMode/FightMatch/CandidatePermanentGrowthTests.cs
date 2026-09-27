using System;
using System.Linq;
using System.Numerics;
using NUnit.Framework;
using static FightMatch.Core.Tests.PlayerSessionTestData;

namespace FightMatch.Core.Tests
{
    [TestFixture]
    public sealed class CandidatePermanentGrowthTests
    {
        [TestCase("W", 5, 50, 1, 5, 42)]
        [TestCase("M", 6, 50, 3, 6, 22)]
        [TestCase("W", 5, 7, 2, 5, 6)]
        [TestCase("W", 5, 500, 1, 6, 272)]
        public void CB02_MinimalCardsAndFullRemainder(string id, int target, int q, int cards, int level, int remainder)
        {
            using (var data = new CandidatePermanentTestData(q: q, farm: false))
            {
                var state = data.Head.Business.Roster.Find(id);
                var result = TakeCore(CandidatePermanentGrowth.CalculateCards(state, data.Definitions.GetPermanentDefinitions(), "card", target, Codec()));
                Assert.AreEqual(new BigInteger(cards), result.Cards);
                Assert.AreEqual(new BigInteger(cards * q), result.FixedExperience);
                Assert.AreEqual(new BigInteger(level), result.FinalLevel);
                Assert.AreEqual(new BigInteger(remainder), result.FinalExperience);
                Assert.AreSame(state, data.Head.Business.Roster.Find(id));
            }
        }

        [Test]
        public void CB02_MultipleLevelsAreNotClampedToTarget()
        {
            using (var data = new CandidatePermanentTestData(q: 5000, farm: false))
            {
                var result = TakeCore(CandidatePermanentGrowth.CalculateCards(data.W, data.Definitions.GetPermanentDefinitions(), "card", 5, Codec()));
                Assert.AreEqual(BigInteger.One, result.Cards);
                Assert.Greater(result.FinalLevel, new BigInteger(8));
                var total = result.FinalExperience;
                for (var level = data.W.Level; level < result.FinalLevel; level++)
                    total += CandidateCharacterGrowth.Need(data.W.Definition, level, Codec().Math);
                Assert.AreEqual(data.W.Experience + 5000, total);
            }
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void CB02_NonpositiveCardDefinitionRejected(int q)
        {
            using (var data = new CandidatePermanentTestData(farm: false))
                Assert.IsFalse(CandidatePermanentDefinitions.Prepare(data.Binding, data.DefinitionInputs(q), Codec()).IsAccepted);
        }

        [Test]
        public void CB02_InvalidTargetMissingDefinitionAndLowBudgetLeaveOwnersUnchanged()
        {
            using (var data = new CandidatePermanentTestData(farm: false))
            {
                var before = Encode(data.Envelope);
                Assert.IsFalse(CandidatePermanentGrowth.CalculateCards(data.W, data.Definitions.GetPermanentDefinitions(), "card", 4, Codec()).IsAccepted);
                Assert.AreEqual("NoPublishedDefinition", CandidatePermanentGrowth.CalculateCards(data.W,
                    data.Definitions.GetPermanentDefinitions(), "missing", 5, Codec()).RejectionCode);
                Assert.AreEqual("InsufficientResources", CandidatePermanentProtocol.Preview(data.Head, data.Definitions,
                    CandidatePermanentTestData.Card("W", 5), Codec()).RejectionCode);
                var limited = CandidatePermanentGrowth.CalculateCards(data.W, data.Definitions.GetPermanentDefinitions(), "card", 5,
                    new SaveCodecBudget(new ExactMathBudget(maxPrimitiveSteps: 0)));
                Assert.IsFalse(limited.IsAccepted);
                CollectionAssert.AreEqual(before, Encode(data.Envelope));
                Assert.AreEqual(0, data.IdCalls);
                Assert.AreEqual(0, data.EntropyCalls);
            }
        }

        [Test]
        public void CB02_MageFactoryDoesNotWidenOldWarriorFactoryOrCreateCombatCrit()
        {
            var binding = SharedRuleFixture.Binding();
            var input = CandidatePermanentTestData.RawGrowth(new PublishedRuleContext(binding), "mage", CharacterClassKind.Mage);
            Assert.IsFalse(CandidateCharacterGrowth.PrepareDefinition(input, Codec().Math).IsAccepted);
            var prepared = CandidateCharacterGrowth.PreparePermanentDefinition(input, Codec().Math);
            Assert.IsTrue(prepared.IsAccepted);
            var state = CandidateCharacterGrowth.CreateCandidate(prepared.Definition, "p", "M", 5, 92, 0, Codec().Math).Next;
            Assert.IsNull(CandidateCharacterGrowth.ComputeBaseStats(state, Codec().Math).TargetProbability);
            input.CritBase = CandidatePermanentTestData.R(1, 5);
            Assert.IsFalse(CandidateCharacterGrowth.PreparePermanentDefinition(input, Codec().Math).IsAccepted);
        }

        [Test]
        public void CB03_G01UsesThreeMageCardsThenM07FourteenThenExplicitLearningAndExplanation()
        {
            using (var data = new CandidatePermanentTestData(certificates: 0))
            {
                data.Gift();
                data.Apply(CandidatePermanentTestData.Card("M", 6));
                Assert.AreEqual(new BigInteger(6), data.M.Level);
                Assert.AreEqual(new BigInteger(22), data.M.Experience);
                Assert.AreEqual(new BigInteger(157), data.W.Experience);
                Assert.AreEqual(BigInteger.Zero, data.Total("card"));
                Assert.AreEqual(BigInteger.One, data.Total("certificate"));
                Assert.IsNull(CandidatePermanentGrowth.FindLearning(data.Head.Business.Roster, "W", "taunt"));
                data.Formation("W", null, null);
                data.Farm("test:L1");
                Assert.AreEqual(new BigInteger(14), data.W.BaseRewards.Last().Amount);
                Assert.AreEqual(new BigInteger(5), data.W.Level);
                Assert.AreEqual(new BigInteger(6), data.W.Experience);
                Assert.AreEqual(BigInteger.Zero, data.Total("card"));
                data.Apply(data.Learn("W"));
                Assert.AreEqual(BigInteger.Zero, data.Total("certificate"));
                Assert.IsNull(CandidatePermanentProgression.Find(data.Head.Business.Progression, CandidatePermanentKind.ConfirmTeachingExplanation, "tutorial"));
                data.Explain();
                data.Reopen();
                Assert.IsNotNull(CandidatePermanentGrowth.FindLearning(data.Head.Business.Roster, "W", "taunt"));
                Assert.AreEqual(BigInteger.Zero, data.Total("certificate"));
            }
        }

        [Test]
        public void CB04_NonparticipantGrowthKeepsOtherCharacterFormationAndActiveBaseline()
        {
            using (var data = new CandidatePermanentTestData())
            {
                data.Commit(data.Entry());
                var before = data.Head.Business;
                var baseline = before.ActiveHistory.CurrentRun.Baseline;
                var bytes = before.ActiveHistory.CurrentRun.CurrentSnapshot.Random.Stream.WordsConsumed;
                data.Apply(CandidatePermanentTestData.Card("M", 6));
                Assert.AreEqual(before.Roster.Find("W").Experience, data.W.Experience);
                CollectionAssert.AreEqual(before.Roster.Formation, data.Head.Business.Roster.Formation);
                Assert.AreEqual(before.Roster.FormationRevision, data.Head.Business.Roster.FormationRevision);
                Assert.AreEqual(bytes, data.Head.Business.ActiveHistory.CurrentRun.CurrentSnapshot.Random.Stream.WordsConsumed);
                Assert.IsTrue(CandidateBattleReportFingerprint.Equal(baseline, data.Head.Business.ActiveHistory.CurrentRun.Baseline, Codec().Math));
                Assert.IsFalse(data.Head.Business.ActiveHistory.CurrentRun.Baseline.Entry.ReadyParticipants.Any(x => x.CharacterId == "M"));
            }
        }

        [Test]
        public void CB04_RecoveringWarriorGrowthPreservesPeriodAndNeverJoinsCurrentAttempt()
        {
            using (var data = new CandidatePermanentTestData(warriorHp: 3))
            {
                data.Formation("W", null, null);
                data.Commit(data.Entry());
                data.Attack(0);
                data.Commit(data.End(false));
                Assert.IsFalse(data.W.IsReady);
                var recovery = data.W.ActiveRecovery;
                Assert.IsNotNull(recovery);
                data.Formation("F", null, null);
                data.Commit(data.Entry());
                var history = CandidatePermanentTestData.BattleBytes(data.Head.Business);
                var other = data.M;
                data.Apply(CandidatePermanentTestData.Card("W", 5));
                Assert.AreEqual(new BigInteger(5), data.W.Level);
                Assert.IsFalse(data.W.IsReady);
                Assert.AreEqual(recovery.RecoveryId, data.W.ActiveRecovery.RecoveryId);
                Assert.AreEqual(recovery.Anomaly, data.W.ActiveRecovery.Anomaly);
                Assert.AreEqual(0, recovery.Elapsed.Compare(data.W.ActiveRecovery.Elapsed, Codec().Math));
                Assert.IsTrue(CandidateRecoveryClock.SameTime(recovery.LastAcceptedSample,
                    data.W.ActiveRecovery.LastAcceptedSample, new GrowthChecks(Codec().Math)));
                CollectionAssert.AreEqual(history, CandidatePermanentTestData.BattleBytes(data.Head.Business));
                CollectionAssert.AreEqual(CandidatePermanentTestData.CharacterBytes(other), CandidatePermanentTestData.CharacterBytes(data.M));
                Assert.IsFalse(data.Head.Business.ActiveHistory.CurrentRun.Baseline.Entry.ReadyParticipants.Any(x => x.CharacterId == "W"));
                data.Reopen();
                Assert.AreEqual(recovery.RecoveryId, data.W.ActiveRecovery.RecoveryId);
                Assert.IsFalse(data.W.ActiveRecovery.IsCompleted);
            }
        }

        [Test]
        public void CB18_ActiveGrowthLearningCraftAndPreferenceKeepHistoryAndRestartBaseline()
        {
            using (var data = new CandidatePermanentTestData(warriorLevel: 5, warriorXp: 0))
            {
                data.Apply(new CandidatePermanentDraft { Kind = CandidatePermanentKind.Craft, DefinitionId = "weapons", Quantity = 1 });
                data.Apply(new CandidatePermanentDraft { Kind = CandidatePermanentKind.Equip, CharacterId = "W", DefinitionId = "weapon", Quantity = 2 });
                data.Commit(data.Entry());
                var before = data.Head.Business.ActiveHistory;
                var history = CandidatePermanentTestData.BattleBytes(data.Head.Business);
                var entry = before.CurrentRun.Baseline.Entry;
                var farmer = data.Head.Business.Roster.Find("F");
                data.Apply(CandidatePermanentTestData.Card("F", 2));
                Assert.Greater(data.Head.Business.Roster.Find("F").Level, farmer.Level);
                data.Apply(data.Learn("W"));
                data.Apply(new CandidatePermanentDraft { Kind = CandidatePermanentKind.Craft, DefinitionId = "weapons", Quantity = 1 });
                data.Apply(new CandidatePermanentDraft { Kind = CandidatePermanentKind.SetPreference, CharacterId = "W",
                    DefinitionId = "weapon", Enabled = false, PreferenceRevision = data.Head.Business.Inventory.PreferenceRevision });
                CollectionAssert.AreEqual(history, CandidatePermanentTestData.BattleBytes(data.Head.Business));
                data.Attack(0);
                var attack = data.Head.Records.Last().OperationId;
                data.Apply(new CandidatePermanentDraft { Kind = CandidatePermanentKind.SetPreference, CharacterId = "W",
                    DefinitionId = "weapon", Enabled = true, PreferenceRevision = data.Head.Business.Inventory.PreferenceRevision });
                var preference = data.Head.Business.Inventory.PreferenceRevision;
                data.RollbackLastAttack();
                Assert.IsTrue(data.Head.Business.Inventory.FindLoadout("W").Enabled.Value);
                Assert.AreEqual(preference, data.Head.Business.Inventory.PreferenceRevision);
                Assert.IsNotNull(data.Head.Records.Single(x => x.OperationId == attack).Intent.Data.Attack);
                data.Reopen();
                Assert.IsTrue(data.Head.Business.Inventory.FindLoadout("W").Enabled.Value);
                data.Commit(data.End(true));
                var restarted = data.Head.Business.ActiveHistory.CurrentRun.Baseline.Entry;
                Assert.IsTrue(CandidateBattleReportFingerprint.Equal(entry.ReadyParticipants, restarted.ReadyParticipants, Codec().Math));
                Assert.AreEqual(entry.EntryBaselineId, restarted.EntryBaselineId);
                Assert.IsNotNull(CandidatePermanentGrowth.FindLearning(data.Head.Business.Roster, "W", "taunt"));
                Assert.AreEqual(new BigInteger(2), data.Head.Business.Roster.Find("F").Level);
            }
        }
    }
}
