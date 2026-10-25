using System.Linq;
using FightMatch.Application;
using FightMatch.Presentation;
using NUnit.Framework;
using UnityEngine;
using static FightMatch.Core.Tests.ChapterBattleTestData;

namespace FightMatch.Core.Tests
{
    public sealed class ChapterMagePlaybackTests
    {
        [TestCase(false)]
        [TestCase(true)]
        public void MageFactsPlayWithoutCritAndPreserveUnboundNameDiagnostics(bool penetration)
        {
            var input = ChapterBattleTestData.Input();
            input.Members[0].Penetration.C = penetration ? R(1) : R(1, 10);
            var record = Record(input, "mage"); var hit = record.DirectAttack.DamageFacts.Single();
            Assert.That(hit.Crit, Is.Null); Assert.That(hit.Penetration.Triggered, Is.EqualTo(penetration));
            Assert.That(hit.DamageKind, Is.EqualTo(EntryDamageKind.Magic));
            var presentation = new CandidateBattlePresentation("fixture:mage-playback", record, null);
            var frame = CandidateBattlePlaybackFrame.Initial(presentation, record.Request.Route);
            var direct = frame.Apply(presentation, record.OrderedFacts.First(f => f.Kind == CandidateBattleFactKind.DirectAttack));
            Assert.That(direct.Actors.Single(a => a.Key.Equals(hit.Target)).Hp.Compare(hit.HpAfter, Math()), Is.Zero);
            StringAssert.DoesNotContain("暴击", direct.Beat);
            foreach (var locale in new[] { LocaleId.En, LocaleId.ZhHans })
            {
                var service = new LocalizationService(new UguiTestTextSource(), SystemLanguage.English); service.SetLocale(locale);
                BattleTextLine line = null;
                Assert.DoesNotThrow(() => line = BattleText.Beat(service, direct, presentation));
                Assert.That(line.Key, Is.EqualTo("fm.battle.playback.beat_row"));
                // The current catalog has no Mage name binding. Preserve that explicit failure instead of disguising Mage as W.
                var text = service.Resolve(line.Key, line.Arguments);
                Assert.That(text.IsSuccess, Is.False); Assert.That(text.DiagnosticCode, Is.EqualTo("InvalidNamedArguments"));
            }
            foreach (var fact in record.OrderedFacts) frame = frame.Apply(presentation, fact);
            Assert.That(frame.Phase, Is.EqualTo(record.AfterSnapshot.Phase));
            Assert.That(frame.Actors.Single(a => a.Key.Equals(hit.Target)).Hp.Compare(hit.HpAfter, Math()), Is.Zero);
            Assert.That(record.BeforeSnapshot.Enemies.Single().Hp.Compare(R(20), Math()), Is.Zero, "Playback does not mutate its original facts.");
        }

        [Test]
        public void WarriorCriticalFactStillUsesTheExistingBilingualCriticalFeedback()
        {
            var input = ChapterBattleTestData.Input(warrior: true); input.Members[0].CharacterId = "W"; input.Members[0].Crit.C = R(1);
            input.Level.Faces[0].Pairs[0].Enemy.EnemyDefinitionId = "enemy:clockwork-infantry";
            input.Level.Faces[0].Pairs[0].Enemy.Stats.PhysicalDefense = R(0);
            var record = Record(input, "W"); var hit = record.DirectAttack.DamageFacts.Single();
            Assert.That(hit.Crit.Triggered, Is.True); Assert.That(hit.Penetration, Is.Null);
            var presentation = new CandidateBattlePresentation("fixture:warrior-playback", record, null);
            var direct = CandidateBattlePlaybackFrame.Initial(presentation, record.Request.Route)
                .Apply(presentation, record.OrderedFacts.First(f => f.Kind == CandidateBattleFactKind.DirectAttack));
            StringAssert.Contains("暴击", direct.Beat);
            foreach (var locale in new[] { LocaleId.En, LocaleId.ZhHans })
            {
                var service = new LocalizationService(new UguiTestTextSource(), SystemLanguage.English); service.SetLocale(locale);
                var expectedHit = service.Resolve("fm.battle.playback.critical_hit", new[] {
                    BattleText.Arg("characterName", BattleText.Character(service, record.BeforeSnapshot, hit.Actor)),
                    BattleText.Arg("enemyName", BattleText.Enemy(service, record.BeforeSnapshot, hit.Pair)),
                    BattleText.Arg("hpBefore", BattleText.Hp(hit.HpBefore)), BattleText.Arg("hpAfter", BattleText.Hp(hit.HpAfter)) });
                Assert.That(expectedHit.IsSuccess, Is.True, expectedHit.DiagnosticCode);
                var line = BattleText.Beat(service, direct, presentation);
                Assert.That(line.Arguments.Single(a => a.Key == "beat").Value, Is.EqualTo(expectedHit.Text));
                var actual = service.Resolve(line.Key, line.Arguments); Assert.That(actual.IsSuccess, Is.True, actual.DiagnosticCode);
            }
        }

        private static CandidateBattleOperationRecord Record(BattleEntryInput input, string actor)
        {
            // Actual rule evaluation with explicit synthetic C for branch coverage; no fabricated hit or PRD result.
            var run = Run(Bind(input));
            var result = CandidateBattleOperations.EvaluateAttack(run, Attack(run.CurrentSnapshot, actor: actor),
                new CandidateBattleConditions { PreferenceRevision = 1, ItemUseEnabled = false }, 100, new RandomSamplingBudget(Math()));
            Assert.That(result.IsAccepted, Is.True, result.RejectionCode + " " + result.FieldPath); return result.Record;
        }
    }
}
