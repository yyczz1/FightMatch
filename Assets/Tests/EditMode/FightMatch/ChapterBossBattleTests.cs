using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using NUnit.Framework;
using static FightMatch.Core.Tests.ChapterBattleTestData;

namespace FightMatch.Core.Tests
{
    public sealed class ChapterBossBattleTests
    {
        internal static BattleEntryInput WindInput(int attack = 15)
        {
            var input = StateInput(3, false);
            input.Members[1].Stats.Attack = R(attack);
            input.Level.Faces[0].Pairs[0].Enemy.Stats.MaxHp = R(101);
            for (var i = 1; i <= 2; i++)
            {
                var enemy = input.Level.Faces[0].Pairs[i].Enemy;
                enemy.WindTargetEnemyInstanceKey = "enemy:0";
                enemy.WindUses = 2;
                enemy.WindFraction = R(1, 10);
                enemy.IntentCycle = new List<EnemyIntentInput> {
                    new EnemyIntentInput { Kind = EnemyIntentKind.Wind, ExhaustedFallbackIndex = 1 },
                    Strike(EnemyTargeting.PreferMage, attack: 26) };
            }
            return input;
        }

        [Test]
        public void InitialIntentCursorSelectsTheFirstActionAndAdvancesOnlyThatEnemy()
        {
            var input = StateInput(2, false);
            var enemy = input.Level.Faces[0].Pairs[1].Enemy;
            enemy.IntentCycle = new List<EnemyIntentInput> { Strike(), new EnemyIntentInput { Kind = EnemyIntentKind.Rest } };
            enemy.InitialIntentCursor = 1;
            var run = Act(Run(Bind(input)), 0, "first");
            var fact = run.Records.Single().EnemyPhase.OrderedIntents[1];
            Assert.AreEqual(EnemyIntentKind.Rest, fact.IntentKind);
            Assert.AreEqual(1, fact.ExecutedIntentIndex);
            Assert.AreEqual(BigInteger.One, fact.CursorBefore);
            Assert.AreEqual(new BigInteger(2), fact.CursorAfter);
            Assert.IsNull(fact.Damage);
            Assert.AreEqual(BigInteger.One, run.CurrentSnapshot.Enemies[0].IntentCursor);
        }

        [Test]
        public void WindWorkersHealSequentialExactFractionsAndSpendIndependentUses()
        {
            var run = Act(Run(Bind(WindInput())), 0, "hit");
            var facts = run.Records.Single().EnemyPhase.OrderedIntents;
            Value(facts[1].Wind.HpBefore, 86);
            Value(facts[1].Wind.ActualHealing, 101, 10);
            Value(facts[1].Wind.HpAfter, 961, 10);
            Value(facts[2].Wind.HpBefore, 961, 10);
            Value(facts[2].Wind.ActualHealing, 49, 10);
            Value(facts[2].Wind.HpAfter, 101);
            Assert.AreEqual(1, run.CurrentSnapshot.Enemies[1].RemainingWindUses);
            Assert.AreEqual(1, run.CurrentSnapshot.Enemies[2].RemainingWindUses);
            Value(run.CurrentSnapshot.Contributions.Single(x => x.CombatantKey.CharacterId == "mage").EffectiveDamageDealtHp, 15);
            Assert.AreEqual(2, run.Records.Single().ContributionSegments.Count,
                "Wind contributes neither damage nor healing score; only the normal hit and boss strike contribute.");
        }

        [TestCase(5, 1, 2)]
        [TestCase(101, 2, 2)]
        public void FullOrDeadWindTargetDoesNotSpendAUseOrRevive(int damage, int firstUses, int secondUses)
        {
            var run = Act(Run(Bind(WindInput(damage))), 0, "hit");
            var facts = run.Records.Single().EnemyPhase.OrderedIntents;
            var winds = facts.Where(x => x.Wind != null).ToArray();
            Assert.AreEqual(2, winds.Length);
            Value(winds[1].Wind.ActualHealing, 0);
            Assert.AreEqual(firstUses, run.CurrentSnapshot.Enemies[1].RemainingWindUses);
            Assert.AreEqual(secondUses, run.CurrentSnapshot.Enemies[2].RemainingWindUses);
            Value(run.CurrentSnapshot.Enemies[0].Hp, damage == 101 ? 0 : 101);
        }

        [Test]
        public void ExhaustedWindExecutesFallbackDamageButAdvancesOriginalCursorOnlyOnce()
        {
            var run = Run(Bind(WindInput()));
            for (var i = 0; i < 5; i++) run = Act(run, 0, "hit:" + i);
            foreach (var fact in run.Records.Last().EnemyPhase.OrderedIntents.Skip(1))
            {
                Assert.AreEqual(EnemyIntentKind.Wind, fact.IntentKind);
                Assert.AreEqual(1, fact.ExecutedIntentIndex);
                Assert.IsNull(fact.Wind);
                Assert.AreEqual("mage", fact.Damage.TargetMember.CharacterId);
                Value(fact.Damage.Attack, 26);
                Assert.AreEqual(new BigInteger(5), fact.CursorAfter);
            }
            Assert.AreEqual(0, run.CurrentSnapshot.Enemies[1].RemainingWindUses);
            Assert.AreEqual(0, run.CurrentSnapshot.Enemies[2].RemainingWindUses);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void HeavyWindowAppliesExactlyToNextActionThenExpiresEvenWhenAnotherEnemyIsChosen(bool hitOther)
        {
            var input = StateInput(2, false);
            var heavy = Strike(attack: 12);
            heavy.DamageCoefficient = R(2);
            heavy.NextActionVulnerability = R(3, 2);
            input.Level.Faces[0].Pairs[0].Enemy.IntentCycle = new List<EnemyIntentInput> { heavy, new EnemyIntentInput { Kind = EnemyIntentKind.Rest } };
            var run = Act(Run(Bind(input)), 1, "expose");
            var window = run.CurrentSnapshot.ChapterState.Vulnerabilities.Single();
            Assert.AreEqual(new BigInteger(2), window.EffectiveActionOrdinal);
            Value(run.Records.Single().EnemyPhase.OrderedIntents[0].Damage.RawDamage, 24);
            run = Act(run, hitOther ? 1 : 0, "consume");
            var hit = run.Records.Last().DirectAttack.DamageFacts.Single();
            Value(hit.RawDamage, hitOther ? 10 : 15);
            Assert.AreEqual(!hitOther, hit.Vulnerability != null);
            Assert.IsEmpty(run.CurrentSnapshot.ChapterState.Vulnerabilities);
            Assert.AreEqual(ChapterVulnerabilityChange.Expired, run.Records.Last().EnemyPhase.VulnerabilityFacts.Single().Change);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void PendingLinksPrecedeAllDownAndCompletingThemDoesNotAdvanceAnotherAction(bool hasNextFace)
        {
            // Exercise the stage owner's HP-projection boundary directly; no battle save or damage fact is fabricated.
            var input = StateInput(2);
            if (hasNextFace) AddStateFace(input);
            var before = Run(Bind(input)).CurrentSnapshot;
            var attack = Attack(before, 0, "warrior", "projection");
            var projected = new CandidateFinalHpProjection {
                MemberHp = before.Members.Select(x => new CandidateHpInput { CombatantKey = x.CombatantKey, Hp = R(0) }).ToList(),
                EnemyHp = before.Enemies.Select(x => new CandidateHpInput { CombatantKey = x.CombatantKey, Hp = R(0) }).ToList() };
            var result = CandidateBattleStage.AfterAttack(before, attack, projected, Math());
            Assert.IsTrue(result.IsAccepted, result.FieldPath);
            Assert.AreEqual(BattlePhase.AwaitLinks, result.Decision.NextPhase);
            Assert.AreEqual(0, result.Decision.NextFaceIndex);
            var state = new BattleSnapshot(before.Baseline, 2, 1, 1, 0, BattlePhase.AwaitLinks, result.Decision.Board,
                before.Members.Select(x => new BattleMemberState(x.CombatantKey, x.Member, R(0))),
                before.Enemies.Select(x => new BattleEnemyState(x.CombatantKey, x.PairKey, x.Enemy, R(0), x.IntentCursor)),
                before.Random, before.Contributions, new ChapterBattleState(1, before.ChapterState.MemberSkills,
                    System.Array.Empty<ChapterTauntState>(), System.Array.Empty<ChapterVulnerabilityWindow>()));
            var pair = state.Board.PendingLinks.Single();
            var route = Attack(state, 1).Route;
            var link = CandidateBattleStage.CompleteLink(state, new CandidateLinkRequest { PlayerId = "player", AttemptId = "attempt",
                ExpectedSceneRevision = state.SceneRevision, OperationId = "complete", Pair = pair, Route = route }, Math());
            Assert.IsTrue(link.IsAccepted, link.FieldPath);
            Assert.AreEqual(hasNextFace ? BattlePhase.AwaitRescue : BattlePhase.WonPendingSettlement, link.Decision.NextPhase);
            var record = CandidateBattleOperations.AssembleRecord(CandidateBattleOperationKind.Link, 100, state, null, null, null, link.Decision, Math());
            Assert.AreEqual(BigInteger.One, record.AfterSnapshot.EffectiveActionsCompleted);
            Assert.AreEqual(BigInteger.One, record.AfterSnapshot.EnemyPhasesCompleted);
            Assert.AreEqual(hasNextFace ? BigInteger.Zero : BigInteger.One, record.AfterSnapshot.ChapterState.FaceEnemyPhasesCompleted);
            Assert.IsNull(record.EnemyPhase);
            Assert.IsTrue(record.OrderedFacts.All(x => x.Kind == CandidateBattleFactKind.Stage));
        }

        [Test]
        public void SkillOnlyActionAlsoExpiresHeavyWindowWithoutDamageOrPrd()
        {
            var input = StateInput(3);
            var heavy = Strike(); heavy.NextActionVulnerability = R(3, 2);
            input.Level.Faces[0].Pairs[0].Enemy.IntentCycle = new List<EnemyIntentInput> { heavy, new EnemyIntentInput { Kind = EnemyIntentKind.Rest } };
            var run = Act(Run(Bind(input)), 0, "expose");
            var random = run.CurrentSnapshot.Random;
            run = Act(run, 2, "taunt", "warrior");
            Assert.IsEmpty(run.Records.Last().DirectAttack.DamageFacts);
            Assert.IsTrue(CandidateBattleReportFingerprint.Equal(random, run.CurrentSnapshot.Random, Math()));
            Assert.IsEmpty(run.CurrentSnapshot.ChapterState.Vulnerabilities);
            Assert.AreEqual(1, run.Records.Last().EnemyPhase.VulnerabilityFacts.Count);
        }
    }
}
