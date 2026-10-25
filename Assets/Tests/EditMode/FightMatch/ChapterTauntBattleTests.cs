using System.Linq;
using System.Numerics;
using NUnit.Framework;
using static FightMatch.Core.Tests.ChapterBattleTestData;

namespace FightMatch.Core.Tests
{
    public sealed class ChapterTauntBattleTests
    {
        [Test]
        public void NormalKillSkipsTauntAndCooldown()
        {
            var input = StateInput(2);
            input.Level.Faces[0].Pairs[0].Enemy.Stats.MaxHp = R(10);
            var next = Act(Run(Bind(input)), 0, "kill", "warrior");
            var record = next.Records.Single();
            Assert.IsTrue(record.DirectAttack.DamageFacts.Single().DefeatedTarget);
            Assert.IsEmpty(record.DirectAttack.SkillCasts);
            Assert.AreEqual(0, next.CurrentSnapshot.ChapterState.MemberSkills.Single().CooldownRemaining);
            Assert.IsEmpty(record.OrderedFacts.Where(x => x.Kind == CandidateBattleFactKind.SkillCast));
            Assert.AreEqual(1, next.CurrentSnapshot.Board.LockedRoutes.Count);
        }

        [Test]
        public void SkillOnlyActionUsesNoPrdAndCooldownTicksOnlyOnLaterValidTeamActions()
        {
            var input = StateInput();
            input.Level.Faces[0].Pairs[2].Enemy.IntentCycle[0] = Strike(EnemyTargeting.PreferMage);
            var initial = Run(Bind(input));
            var next = Act(initial, 2, "taunt", "warrior");
            var record = next.Records.Single();
            Assert.IsEmpty(record.DirectAttack.DamageFacts);
            Assert.AreEqual(1, record.DirectAttack.SkillCasts.Count);
            Assert.IsTrue(CandidateBattleReportFingerprint.Equal(initial.CurrentSnapshot.Random, next.CurrentSnapshot.Random, Math()));
            Assert.AreEqual("warrior", record.EnemyPhase.OrderedIntents[2].Damage.TargetMember.CharacterId);
            Assert.AreEqual("warrior", record.EnemyPhase.OrderedIntents[2].TauntCaster.CharacterId);
            Assert.AreEqual(2, next.CurrentSnapshot.ChapterState.MemberSkills.Single().CooldownRemaining);
            Assert.IsEmpty(next.CurrentSnapshot.ChapterState.Taunts);
            Assert.IsEmpty(record.EnemyPhase.CooldownFacts);
            Assert.IsEmpty(record.ContributionSegments.Where(x => x.RuleSegment == CandidateBattleFactKind.DirectAttack));
            var rejected = CandidateBattleOperations.EvaluateAttack(next, Attack(next.CurrentSnapshot, 2, "warrior", "busy"),
                new CandidateBattleConditions { PreferenceRevision = 1, ItemUseEnabled = false }, 100, new RandomSamplingBudget(Math()));
            Assert.IsFalse(rejected.IsAccepted);
            Assert.AreEqual("OutOfRange", rejected.RejectionCode);
            Assert.AreEqual(BigInteger.One, next.CurrentSnapshot.EffectiveActionsCompleted);
            next = Act(next, 0, "mage:1");
            Assert.AreEqual(1, next.CurrentSnapshot.ChapterState.MemberSkills.Single().CooldownRemaining);
            Assert.AreEqual("mage", next.Records.Last().EnemyPhase.OrderedIntents[2].Damage.TargetMember.CharacterId);
            Assert.IsNull(next.Records.Last().EnemyPhase.OrderedIntents[2].TauntCaster);
            next = Act(next, 0, "mage:2");
            Assert.AreEqual(0, next.CurrentSnapshot.ChapterState.MemberSkills.Single().CooldownRemaining);
            next = Act(next, 2, "taunt:again", "warrior");
            Assert.AreEqual(2, next.CurrentSnapshot.ChapterState.MemberSkills.Single().CooldownRemaining);
            Assert.AreEqual(new BigInteger(4), next.CurrentSnapshot.EnemyPhasesCompleted);
        }

        [TestCase(EnemyIntentKind.Charge)]
        [TestCase(EnemyIntentKind.Rest)]
        public void NonStrikePhaseConsumesTauntWindow(EnemyIntentKind kind)
        {
            var input = StateInput(2);
            input.Level.Faces[0].Pairs[1].Enemy.IntentCycle = new System.Collections.Generic.List<EnemyIntentInput> {
                new EnemyIntentInput { Kind = kind }, Strike(EnemyTargeting.PreferMage) };
            var next = Act(Run(Bind(input)), 1, "taunt", "warrior");
            Assert.IsNull(next.Records[0].EnemyPhase.OrderedIntents[1].Damage);
            Assert.IsEmpty(next.CurrentSnapshot.ChapterState.Taunts);
            next = Act(next, 0, "next");
            var fact = next.Records.Last().EnemyPhase.OrderedIntents[1];
            Assert.AreEqual("mage", fact.Damage.TargetMember.CharacterId);
            Assert.IsNull(fact.TauntCaster);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void DeadOrOutOfLegalRangeCasterCannotOverrideDefaultTarget(bool outOfRange)
        {
            var input = StateInput(2);
            var target = input.Level.Faces[0].Pairs[1].Enemy;
            target.IntentCycle[0] = Strike(EnemyTargeting.PreferMage, outOfRange ? 1 : 2);
            if (outOfRange)
            {
                input.Members[0].OriginalSlot = 1;
                input.Members[1].OriginalSlot = 0;
            }
            else input.Level.Faces[0].Pairs[0].Enemy.IntentCycle[0] = Strike(attack: 1000);
            var next = Act(Run(Bind(input)), 1, "taunt", "warrior");
            var fact = next.Records.Single().EnemyPhase.OrderedIntents[1];
            Assert.IsNull(fact.TauntCaster);
            Assert.AreEqual("mage", fact.Damage.TargetMember.CharacterId);
            if (!outOfRange) Value(next.CurrentSnapshot.Members.Single(x => x.Member.CharacterId == "warrior").Hp, 0);
        }

        [Test]
        public void StateContractMustBeExplicitAndLegacyEntriesKeepTheirEmptySkillContract()
        {
            var old = ChapterBattleTestData.Input(1, true);
            Assert.IsTrue(Prepare(old).IsAccepted);
            Assert.IsNull(Run(Bind(old)).CurrentSnapshot.ChapterState);
            var input = StateInput();
            input.RequiredFeatures.Clear();
            Assert.IsFalse(Prepare(input).IsAccepted);
            input = StateInput();
            input.Members[0].BattleSkills[0].BattleCapabilityId = ChapterRuleDefinitions.StateFeatureId;
            Assert.IsFalse(Prepare(input).IsAccepted, "The state token must not replace the skill's battle capability.");
        }
    }
}
