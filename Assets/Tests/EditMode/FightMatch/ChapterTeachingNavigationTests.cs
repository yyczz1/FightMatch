using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using FightMatch.Application;
using FightMatch.Host;
using FightMatch.Presentation;
using NUnit.Framework;
using UnityEngine;
using Data = FightMatch.Core.Tests.PlayerSessionTestData;

namespace FightMatch.Core.Tests
{
    public sealed class ChapterTeachingNavigationTests
    {
        private static PlayerNavigationTarget Boss(ChapterTeachingRig rig) => new PlayerNavigationTarget {
            Kind = PlayerNavigationTargetKind.Preparation, LevelId = rig.LevelBinding.LevelId,
            LevelVersion = rig.LevelBinding.CanonicalLevelVersion, Binding = rig.Fixture.Publication.Binding };

        private static ILocalizedTextSource TextSource()
        {
            const string root = "Generated/FightMatch/Localization/ChapterOneTrial-r3/";
            Assert.IsTrue(File.Exists(root + "fm-text-v1.manifest.json"), "C must generate the actual independent r3 localization pair before this test.");
            var bytes = File.ReadAllBytes(root + "fm-text-v1.json");
            string hash;
            using (var sha = SHA256.Create()) hash = BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", "").ToLowerInvariant();
            Assert.IsTrue(FightMatchLocalizationCatalogParser.TryParse(bytes, hash, 310, out var source, out var error), error);
            return source;
        }

        [Test]
        public void QueriesDoNotGiftAndExplicitEntryUnknownResolvesOneOriginalOperation()
        {
            var fixture = new ChapterTeachingFixture(initiallyOpenBoss: true, initialWarriorLevel: 4);
            using (var rig = new ChapterTeachingRig(fixture))
            using (var controller = new PlayerNavigationController(rig.Session, Data.Budget()))
            {
                var head = rig.Head; var files = Data.CopyFiles(rig.Storage.Files);
                controller.Refresh(); rig.Session.QueryNavigation(Data.Codec());
                Assert.AreSame(head, rig.Head); Data.SameFiles(files, rig.Storage.Files);
                var enter = controller.NavigationHandler(Boss(rig));
                rig.Storage.Fault = "marker-after";
                enter();
                Assert.AreEqual(PlayerNavigationRoute.Recovery, controller.View.Route);
                Assert.AreEqual("CommitUnknown", controller.View.Status);
                Assert.AreSame(head, rig.Head);
                Assert.AreEqual("fm.operation.begin_teaching_gift", PlayerNavigationRecoveryView.PendingOperationKey(controller.View));
                var pendingFiles = Data.CopyFiles(rig.Storage.Files);
                enter(); controller.Refresh();
                Data.SameFiles(pendingFiles, rig.Storage.Files);
                controller.ActionHandler(PlayerNavigationAction.Resolve)();
                Assert.AreEqual(PlayerNavigationRoute.CommittedResult, controller.View.Route);
                controller.ActionHandler(PlayerNavigationAction.Return)();
                Assert.AreEqual(PlayerNavigationRoute.Preparation, controller.View.Route);
                Assert.AreEqual("TeachingLearningRequired", controller.View.TeachingAdmissionReason);
                Assert.AreEqual(1, (int)rig.CertificateCount); Assert.IsNull(rig.Head.Business.ActiveHistory);
                var completed = rig.Head; var saved = Data.CopyFiles(rig.Storage.Files);
                controller.Refresh(); controller.NavigationHandler(new PlayerNavigationTarget { Kind = PlayerNavigationTargetKind.MapAdventure })();
                controller.NavigationHandler(Boss(rig))(); controller.Refresh();
                Assert.AreSame(completed, rig.Head); Data.SameFiles(saved, rig.Storage.Files);
                Assert.AreEqual(4, (int)rig.Head.Business.Roster.Find(rig.WarriorId).Level);
            }
        }

        [Test]
        public void RealUguiLearningCancelConfirmAndIndependentExplanationUseActualR3Text()
        {
            var fixture = new ChapterTeachingFixture(initiallyOpenBoss: true);
            using (var rig = new ChapterTeachingRig(fixture))
            using (var controller = new PlayerNavigationController(rig.Session, Data.Budget()))
            using (var font = new ChapterTransientTestFont())
            using (var panel = new NavigationPanel(controller, font.BindRoot))
            {
                var service = new LocalizationService(TextSource(), SystemLanguage.English);
                panel.View.Bind(controller, service);
                controller.NavigationHandler(Boss(rig))();
                controller.ActionHandler(PlayerNavigationAction.Return)();
                var before = rig.Head; var files = Data.CopyFiles(rig.Storage.Files);
                panel.Click(NavigationPanel.Row("teaching.Learn"));
                Assert.AreEqual(PlayerNavigationRoute.Detail, controller.View.Route);
                Assert.IsFalse(panel.Find<TMPro.TMP_InputField>(NavigationPanel.Row("permanent.Quantity")).gameObject.activeInHierarchy);
                panel.Click(NavigationPanel.Row("permanent.Preview"));
                Assert.AreEqual(PlayerNavigationRoute.Confirmation, controller.View.Route);
                Assert.AreEqual(1, (int)controller.View.Confirmation.Quote.Costs.Single().Quantity);
                foreach (var locale in new[] { LocaleId.En, LocaleId.ZhHans })
                {
                    service.SetLocale(locale);
                    var line = panel.VisibleTexts.Single(x => x.Key == "fm.teaching.taunt.learn_confirm");
                    Assert.IsNull(line.DiagnosticCode);
                    StringAssert.Contains(service.Resolve("fm.name.character.w", null).Text, line.Target.text);
                    StringAssert.Contains(service.Resolve("fm.name.item.profession_certificate", null).Text, line.Target.text);
                }
                controller.ActionHandler(PlayerNavigationAction.Cancel)();
                Assert.AreSame(before, rig.Head); Data.SameFiles(files, rig.Storage.Files);
                panel.Click(NavigationPanel.Row("teaching.Learn"));
                panel.Click(NavigationPanel.Row("permanent.Preview")); panel.Click(NavigationPanel.Row("save.Confirm"));
                controller.ActionHandler(PlayerNavigationAction.Return)();
                Assert.AreEqual("TeachingExplanationRequired", controller.View.TeachingAdmissionReason);
                Assert.IsNull(rig.Head.Business.ActiveHistory);
                Assert.AreEqual(0, (int)rig.CertificateCount);
                panel.Click(NavigationPanel.Row("teaching.Explain")); panel.Click(NavigationPanel.Row("permanent.Preview"));
                Assert.AreEqual(CandidatePermanentKind.ConfirmTeachingExplanation, controller.View.Confirmation.Quote.Kind);
                Assert.IsNull(controller.View.Confirmation.Quote.CharacterId);
                Assert.IsTrue(panel.VisibleTexts.Any(x => x.Key == "fm.ch1.tutorial.taunt.explanation" && x.DiagnosticCode == null));
                panel.Click(NavigationPanel.Row("save.Confirm")); controller.ActionHandler(PlayerNavigationAction.Return)();
                Assert.IsNull(controller.View.TeachingAdmissionReason);
                Assert.IsNull(rig.Head.Business.ActiveHistory, "Completing teaching does not allocate an Attempt.");
                var completed = rig.Head;
                controller.NavigationHandler(new PlayerNavigationTarget { Kind = PlayerNavigationTargetKind.BattleSelectionRequested })();
                Assert.AreEqual(PlayerNavigationTargetKind.BattleSelectionRequested, controller.View.HostRequest.Kind);
                Assert.AreSame(completed, rig.Head);
            }
        }

        [Test]
        public void LevelFourCanOpenTargetLevelCardInputOrReturnToAnOpenLevelWithoutAttempt()
        {
            var fixture = new ChapterTeachingFixture(initiallyOpenBoss: true, initialWarriorLevel: 4);
            using (var rig = new ChapterTeachingRig(fixture))
            using (var controller = new PlayerNavigationController(rig.Session, Data.Budget()))
            using (var font = new ChapterTransientTestFont())
            using (var panel = new NavigationPanel(controller, font.BindRoot))
            {
                var service = new LocalizationService(TextSource(), SystemLanguage.English);
                panel.View.Bind(controller, service);
                controller.NavigationHandler(Boss(rig))(); controller.ActionHandler(PlayerNavigationAction.Return)();
                var card = fixture.Publication.Definitions.GetPermanentDefinitions().Records.Single(x => x.Kind == CandidatePermanentDefinitionKind.Card);
                panel.Click(NavigationPanel.Row("teaching.Card." + card.Id));
                var field = panel.Find<TMPro.TMP_InputField>(NavigationPanel.Row("permanent.Quantity"));
                Assert.AreEqual("5", field.text);
                Assert.IsTrue(panel.VisibleTexts.Any(x => x.Key == "fm.growth.target_level.label" && x.DiagnosticCode == null));
                var before = rig.Head;
                panel.Click(NavigationPanel.Row("permanent.Preview"));
                Assert.AreEqual("InsufficientResources", controller.View.Status);
                Assert.AreSame(before, rig.Head);
                controller.NavigationHandler(new PlayerNavigationTarget { Kind = PlayerNavigationTargetKind.MapAdventure })();
                Assert.IsTrue(controller.View.Read.Progression.Levels.Any(x => x.OpenFact != null && x.Level.LevelId == ChapterTeachingFixture.LevelId(1)));
                Assert.IsNull(rig.Head.Business.ActiveHistory);
                Assert.AreEqual(4, (int)rig.Head.Business.Roster.Find(rig.WarriorId).Level);
            }
        }

        [TestCase(4)]
        [TestCase(5)]
        public void TeachingCardsKeepSelectedMageAndLearningStillTargetsWarrior(int warriorLevel)
        {
            // This synthetic authoring option moves the normal unlock and three-card reward to L1;
            // the real r3 L8 unlock remains covered by its separate authoring lifecycle test.
            var fixture = new ChapterTeachingFixture(initiallyOpenBoss: true, initialWarriorLevel: warriorLevel,
                earlyMageCardReward: true);
            using (var rig = new ChapterTeachingRig(fixture))
            {
                rig.Enter(1); rig.Win(); rig.Settle();
                var card = fixture.Publication.Definitions.GetPermanentDefinitions().Records.Single(x => x.Kind == CandidatePermanentDefinitionKind.Card);
                Assert.AreEqual(3, (int)rig.Head.Business.Inventory.Holdings.Single(x => x.ItemId == card.Id).T);
                var warrior = rig.Head.Business.Roster.Find(rig.WarriorId);
                Assert.AreEqual(warriorLevel, (int)warrior.Level);
                using (var controller = new PlayerNavigationController(rig.Session, Data.Budget()))
                using (var font = new ChapterTransientTestFont())
                using (var panel = new NavigationPanel(controller, font.BindRoot))
                {
                    var service = new LocalizationService(TextSource(), SystemLanguage.English);
                    panel.View.Bind(controller, service);
                    controller.NavigationHandler(new PlayerNavigationTarget { Kind = PlayerNavigationTargetKind.Team })();
                    panel.Click(FightMatchViewId.Member(rig.MageId));
                    controller.NavigationHandler(Boss(rig))(); controller.ActionHandler(PlayerNavigationAction.Return)();
                    Assert.AreEqual(rig.MageId, controller.View.Context.SelectedCharacterId);

                    var before = rig.Head; var files = Data.CopyFiles(rig.Storage.Files);
                    panel.Click(NavigationPanel.Row("teaching.Card." + card.Id));
                    Assert.AreEqual(PlayerNavigationRoute.Detail, controller.View.Route);
                    Assert.AreEqual(rig.MageId, controller.View.Context.SelectedCharacterId);
                    var field = panel.Find<TMPro.TMP_InputField>(NavigationPanel.Row("permanent.Quantity"));
                    Assert.AreEqual("2", field.text, "The target-level default must come from the selected Lv1 Mage.");
                    field.text = "3";
                    panel.Click(NavigationPanel.Row("permanent.Preview"));
                    Assert.AreEqual(PlayerNavigationRoute.Confirmation, controller.View.Route);
                    var quote = controller.View.Confirmation.Quote;
                    Assert.AreEqual(rig.MageId, quote.CharacterId);
                    Assert.AreEqual(3, (int)quote.Quantity.Value, "Quantity is the requested target level.");
                    Assert.AreEqual(3, (int)quote.Costs.Single(x => x.ItemId == card.Id).Quantity);
                    Assert.AreEqual(150, (int)quote.FixedExperience);
                    foreach (var locale in new[] { LocaleId.En, LocaleId.ZhHans })
                    {
                        service.SetLocale(locale);
                        var preview = panel.VisibleTexts.Single(x => x.Key == "fm.growth.cards.change_preview");
                        Assert.IsNull(preview.DiagnosticCode);
                        StringAssert.Contains(service.Resolve("fm.name.character.mage", null).Text, preview.Target.text);
                    }
                    controller.ActionHandler(PlayerNavigationAction.Cancel)();
                    Assert.AreSame(before, rig.Head); Data.SameFiles(files, rig.Storage.Files);
                    Assert.AreEqual(rig.MageId, controller.View.Context.SelectedCharacterId);

                    panel.Click(NavigationPanel.Row("teaching.Card." + card.Id));
                    panel.Find<TMPro.TMP_InputField>(NavigationPanel.Row("permanent.Quantity")).text = "3";
                    panel.Click(NavigationPanel.Row("permanent.Preview"));
                    Assert.AreEqual(rig.MageId, controller.View.Confirmation.Quote.CharacterId);
                    panel.Click(NavigationPanel.Row("save.Confirm"));
                    Assert.AreEqual(PlayerNavigationRoute.CommittedResult, controller.View.Route);
                    controller.ActionHandler(PlayerNavigationAction.Return)();
                    Assert.AreEqual(rig.MageId, controller.View.Context.SelectedCharacterId);
                    var mage = rig.Head.Business.Roster.Find(rig.MageId);
                    Assert.AreEqual(quote.FinalLevel, mage.Level); Assert.AreEqual(quote.FinalExperience, mage.Experience);
                    Assert.AreEqual(warrior.Level, rig.Head.Business.Roster.Find(rig.WarriorId).Level);
                    Assert.AreEqual(warrior.Experience, rig.Head.Business.Roster.Find(rig.WarriorId).Experience);
                    Assert.AreEqual(0, (int)rig.Head.Business.Inventory.Holdings.Single(x => x.ItemId == card.Id).T);
                    Assert.AreEqual(1, (int)rig.CertificateCount);
                    Assert.IsNull(rig.Head.Business.ActiveHistory);

                    var learn = panel.Find<UnityEngine.UI.Button>(NavigationPanel.Row("teaching.Learn"));
                    if (warriorLevel == 5)
                    {
                        panel.Click(learn);
                        Assert.AreEqual(rig.WarriorId, controller.View.Context.SelectedCharacterId);
                        panel.Click(NavigationPanel.Row("permanent.Preview"));
                        Assert.AreEqual(CandidatePermanentKind.LearnSkill, controller.View.Confirmation.Quote.Kind);
                        Assert.AreEqual(rig.WarriorId, controller.View.Confirmation.Quote.CharacterId);
                        Assert.AreEqual(1, (int)controller.View.Confirmation.Quote.Costs.Single().Quantity);
                        controller.ActionHandler(PlayerNavigationAction.Cancel)();
                    }
                    else
                    {
                        Assert.IsFalse(learn.interactable);
                        var required = panel.VisibleTexts.First(x => x.Key == "fm.teaching.taunt.requires_level");
                        Assert.IsNull(required.DiagnosticCode);
                        StringAssert.Contains(service.Resolve("fm.name.character.w", null).Text, required.Target.text);
                        var afterCards = rig.Head;
                        controller.NavigationHandler(new PlayerNavigationTarget { Kind = PlayerNavigationTargetKind.MapAdventure })();
                        panel.Click("fm.action.map.level01");
                        Assert.AreEqual(PlayerNavigationRoute.Preparation, controller.View.Route);
                        Assert.AreEqual(ChapterTeachingFixture.LevelId(1), controller.View.Context.LevelId);
                        Assert.AreSame(afterCards, rig.Head);
                        Assert.IsNull(rig.Head.Business.ActiveHistory, "Returning to an open level does not allocate an Attempt.");
                    }
                }
            }
        }
    }
}
