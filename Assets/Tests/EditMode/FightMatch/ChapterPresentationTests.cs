using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using FightMatch.Application;
using FightMatch.Content;
using FightMatch.Host;
using FightMatch.Platform;
using FightMatch.Presentation;
using FlowPuzzle.Core;
using NUnit.Framework;
using QFramework;
using UnityEditor;
using UnityEngine;
using Data = FightMatch.Core.Tests.PlayerSessionTestData;

namespace FightMatch.Core.Tests
{
    public sealed class ChapterPresentationTests
    {
        private const string TextRoot = "Generated/FightMatch/Localization/ChapterOneTrial-r2/";
        private const string TextHash = "038a9fed918495f04bb259ab48bf157d55df0e1a81a37e649dd7469c1953177c";
        private const string ManifestHash = "4072b5f149e609b8fdb810ebc9c1fb46d5bda6374bf77a12d872188ce091a8b5";
        private static ILocalizedTextSource TextSource()
        {
            var bytes = File.ReadAllBytes(TextRoot + "fm-text-v1.json");
            using (var sha = SHA256.Create())
                Assert.AreEqual(ManifestHash, BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(TextRoot + "fm-text-v1.manifest.json"))).Replace("-", "").ToLowerInvariant());
            Assert.IsTrue(FightMatchLocalizationCatalogParser.TryParse(bytes, TextHash, 279, out var source, out var error), error);
            return source;
        }

        [TestCase(false)]
        [TestCase(true)]
        public void ActualMageHitUsesItsOwnClassAndCommittedPenetrationFact(bool penetrated)
        {
            var input = ChapterBattleTestData.Input();
            input.Members[0].CharacterId = "new-mage-9f503";
            input.Members[0].ClassId = "class:mage";
            input.Members[0].Penetration.C = penetrated ? ChapterBattleTestData.R(1) : ChapterBattleTestData.R(1, 10);
            input.Level.Faces[0].Pairs[0].Enemy.EnemyDefinitionId = "enemy:tin-armor-guard";
            var run = ChapterBattleTestData.Run(ChapterBattleTestData.Bind(input));
            var action = CandidateBattleOperations.EvaluateAttack(run,
                ChapterBattleTestData.Attack(run.CurrentSnapshot, actor: input.Members[0].CharacterId),
                new CandidateBattleConditions { PreferenceRevision = 1, ItemUseEnabled = false }, 100,
                new RandomSamplingBudget(ChapterBattleTestData.Math()));
            Assert.IsTrue(action.IsAccepted, action.FieldPath);
            var record = action.Record; var hit = record.DirectAttack.DamageFacts.Single();
            Assert.AreEqual(penetrated, hit.Penetration.Triggered);
            var presentation = new CandidateBattlePresentation("fixture:chapter-text", record, null);
            var frame = CandidateBattlePlaybackFrame.Initial(presentation, record.Request.Route)
                .Apply(presentation, record.OrderedFacts.First(x => x.Kind == CandidateBattleFactKind.DirectAttack));
            var service = new LocalizationService(TextSource(), SystemLanguage.English);
            foreach (var locale in new[] { LocaleId.En, LocaleId.ZhHans })
            {
                service.SetLocale(locale);
                Assert.AreEqual(service.Resolve("fm.name.character.mage", null).Text,
                    BattleText.Character(service, record.BeforeSnapshot, hit.Actor));
                var line = BattleText.Beat(service, frame, presentation);
                var expected = service.Resolve(penetrated ? "fm.battle.playback.penetrating_hit" : "fm.battle.playback.direct_hit", new[] {
                    BattleText.Arg("characterName", service.Resolve("fm.name.character.mage", null).Text),
                    BattleText.Arg("enemyName", BattleText.Enemy(service, record.BeforeSnapshot, hit.Pair)),
                    BattleText.Arg("hpBefore", BattleText.Hp(hit.HpBefore)), BattleText.Arg("hpAfter", BattleText.Hp(hit.HpAfter)) });
                Assert.IsTrue(expected.IsSuccess, expected.DiagnosticCode);
                Assert.AreEqual(expected.Text, line.Arguments.Single(x => x.Key == "beat").Value);
                BindActualText(service, line);
                var actor = record.BeforeSnapshot.Members.Single();
                BindActualText(service, BattleText.MemberHp(service, actor.Member, actor.Hp, actor.Member.Stats.MaxHp));
            }
        }

        [Test]
        public void PublishedNamesAndNineLevelLabelsDoNotInferClassFromCharacterIdentity()
        {
            var service = new LocalizationService(TextSource(), SystemLanguage.English);
            var source = ChapterBattleTestData.Input(); source.Members[0].CharacterId = "W"; source.Members[0].ClassId = "class:mage";
            var entry = ChapterBattleTestData.Prepare(source).Entry;
            foreach (var locale in new[] { LocaleId.En, LocaleId.ZhHans })
            {
                service.SetLocale(locale);
                Assert.AreEqual(service.Resolve("fm.name.character.mage", null).Text, BattleText.Character(service, entry.Members.Single()));
                Assert.AreEqual("", BattleText.CharacterKey("class:mage", CharacterClassKind.Warrior));
                Assert.AreEqual("", BattleText.CharacterKey("unpublished:class", CharacterClassKind.Mage));
                Assert.AreEqual(service.Resolve("fm.name.item.xp_card_50", null).Text, BattleText.Item(service, "item:xp-card-50"));
                foreach (var number in Enumerable.Range(1, 9))
                    Assert.AreEqual(service.Resolve("fm.map.level.card_title", new[] { BattleText.Arg("levelNumber", number.ToString()) }).Text,
                        BattleText.Level(service, "level:ch01-0" + number, "1"));
                Assert.IsNull(BattleText.Level(service, "level:ch01-09", "2"));
                Assert.IsNull(BattleText.Level(service, "level:ch01-10", "1"));
                foreach (var enemy in new[] { "enemy:heavy-tin-soldier", "enemy:tin-armor-guard" })
                {
                    source.Level.Faces[0].Pairs[0].Enemy.EnemyDefinitionId = enemy;
                    var prepared = ChapterBattleTestData.Prepare(source).Entry.Level.Faces[0].Pairs[0].Enemy;
                    BindActualText(service, BattleText.EnemyName(service, prepared));
                }
            }
        }

        [Test]
        public void ChapterPreviewAndOriginalReceiptUseTheirOwnDefinitionsAndEffectsWithoutDuplicatingItems()
        {
            // A two-level protocol fixture exercises rendering through real commits; it is not the production 1–9 acceptance run.
            using (var rig = new PresentationRig())
            using (var controller = new PlayerNavigationController(rig.Session, Data.Budget()))
            using (var panel = new NavigationPanel(controller))
            {
                var service = new LocalizationService(TextSource(), SystemLanguage.English);
                panel.View.Bind(controller, service);
                var read = rig.Session.QueryNavigation(Data.Codec());
                Assert.IsTrue(read.IsAvailable, read.Code);
                Assert.AreSame(rig.Head, read.Head); Assert.IsTrue(rig.Publication.Binding.Same(read.Chapter.Binding));
                controller.NavigationHandler(new PlayerNavigationTarget { Kind = PlayerNavigationTargetKind.MapAdventure })();
                Assert.AreEqual(1, panel.VisibleTexts.Count(x => x.Key == "fm.map.level.locked"));
                Assert.AreEqual(1, panel.VisibleTexts.Count(x => x.Key == "fm.map.level.requires_clear"));
                controller.NavigationHandler(new PlayerNavigationTarget { Kind = PlayerNavigationTargetKind.Preparation,
                    LevelId = rig.Level.LevelId, LevelVersion = rig.Level.LevelVersion, Binding = rig.Publication.Binding })();
                foreach (var locale in new[] { LocaleId.En, LocaleId.ZhHans })
                {
                    service.SetLocale(locale);
                    var preview = NavigationBindings.FirstClearPreview(service, read, rig.Level.LevelId, rig.Level.LevelVersion);
                    Assert.AreEqual(3, preview.Count);
                    Assert.AreEqual(1, preview.Count(x => x.Key == "fm.first_clear_reward.character"));
                    Assert.AreEqual(1, preview.Count(x => x.Key == "fm.first_clear_reward.item"));
                    foreach (var line in preview) BindActualText(service, line);
                    var displayed = panel.VisibleTexts.Where(x => x.Key == "fm.first_clear_reward.character" || x.Key == "fm.first_clear_reward.item").ToArray();
                    Assert.AreEqual(2, displayed.Length);
                    Assert.IsTrue(displayed.All(x => x.DiagnosticCode == null));
                }
                var formation = rig.Head.Business.Roster.Formation.ToArray();
                rig.WinAndSettle(); var firstIntent = rig.Head.Records.Last().Intent;
                var first = Receipt(rig.Head, firstIntent);
                Assert.AreEqual(1, first.Members.Count);
                var mage = rig.Head.Business.Roster.Characters.Single(x => x.Definition.ClassKind == CharacterClassKind.Mage);
                Assert.IsFalse(first.Members.Any(x => x.CharacterId == mage.CharacterId));
                CollectionAssert.AreEqual(formation, rig.Head.Business.Roster.Formation);
                Assert.IsEmpty(NavigationBindings.FirstClearPreview(service, rig.Session.QueryNavigation(Data.Codec()), rig.Level.LevelId, "1"));
                controller.Refresh();
                controller.NavigationHandler(new PlayerNavigationTarget { Kind = PlayerNavigationTargetKind.MapAdventure })();
                Assert.AreEqual(1, panel.VisibleTexts.Count(x => x.Key == "fm.map.level.cleared"));
                Assert.IsFalse(panel.VisibleTexts.Any(x => x.Key == "fm.map.level.locked"));
                foreach (var locale in new[] { LocaleId.En, LocaleId.ZhHans })
                {
                    service.SetLocale(locale);
                    var rows = BattleText.ReceiptRewards(service, first, rig.Head);
                    Assert.AreEqual(1, rows.Count(x => x.Key == "fm.result.character_acquired"));
                    Assert.AreEqual(1, rows.Count(x => x.Identity == "receipt-first-clear-item:item:xp-card-50"));
                    Assert.AreEqual(1, rows.Count(x => x.Arguments.Any(a => a.Key == "itemName" && a.Value == BattleText.Item(service, "item:xp-card-50"))));
                    foreach (var row in rows) BindActualText(service, row);
                    Assert.AreEqual(service.Resolve("fm.name.character.w", null).Text, BattleText.Character(service, first, first.Members[0].CharacterId));
                    Assert.IsNull(BattleText.Character(service, first, mage.CharacterId));
                }
                rig.WinAndSettle(); var repeated = Receipt(rig.Head, rig.Head.Records.Last().Intent);
                Assert.IsFalse(BattleText.ReceiptRewards(service, repeated, rig.Head).Any(x => x.Key == "fm.result.character_acquired"));
                Assert.IsFalse(BattleText.ReceiptRewards(service, repeated, rig.Head).Any(x => x.Identity.StartsWith("receipt-first-clear", StringComparison.Ordinal)));
                first = Receipt(rig.Head, firstIntent);
                Assert.AreEqual(1, BattleText.ReceiptRewards(service, first, rig.Head).Count(x => x.Key == "fm.result.character_acquired"));
                controller.Refresh();
                controller.NavigationHandler(new PlayerNavigationTarget { Kind = PlayerNavigationTargetKind.Team })();
                foreach (var locale in new[] { LocaleId.En, LocaleId.ZhHans })
                {
                    service.SetLocale(locale);
                    var caption = panel.Find<UnityEngine.UI.Button>(FightMatchViewId.Member(mage.CharacterId)).GetComponentInChildren<LocalizedTmpText>();
                    Assert.IsNull(caption.DiagnosticCode);
                    StringAssert.Contains(service.Resolve("fm.name.character.mage", null).Text, caption.Target.text);
                }
            }
        }

        private static PlayerBattleReceipt Receipt(CandidateApplicationSnapshot head, PreparedCandidateApplicationIntent intent) =>
            new PlayerBattleReceipt(Data.TakeCore(CandidateApplicationProtocol.Lookup(head, intent, Data.Codec())));
        private static void BindActualText(LocalizationService service, BattleTextLine line)
        {
            var go = new GameObject("ChapterText", typeof(RectTransform), typeof(TMPro.TextMeshProUGUI), typeof(LocalizedTmpText));
            try
            {
                go.GetComponent<TMPro.TextMeshProUGUI>().font = AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>("Assets/UI/FightMatch/Fonts/NotoSansCJKsc-Regular-TMP.asset");
                var text = go.GetComponent<LocalizedTmpText>(); line.Bind(text, service);
                Assert.IsNull(text.DiagnosticCode, line.Key); Assert.AreEqual(line.Resolve(service), text.Target.text);
                StringAssert.DoesNotContain(LocalizedTmpText.Placeholder, text.Target.text);
            }
            finally
            {
                go.GetComponent<LocalizedTmpText>().Unbind();
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        private sealed class PresentationRig : IDisposable
        {
            private readonly IArchitecture architecture;
            private readonly PublishedSource source;
            internal readonly ResolvedPublication Publication;
            internal readonly PlayerSessionSystem Session;
            private readonly CandidateApplicationSystem application;
            private readonly CandidateLifecycleApplicationSystem lifecycle;
            private readonly CandidateBattleApplicationSystem battle;
            internal CandidateApplicationSnapshot Head => application.QueryView().View.PublishedSnapshot;
            internal PreparedLevel Level => Publication.Definitions.Levels[0];
            internal PresentationRig()
            {
                source = ChapterContentTestData.Source();
                source.Chapter.ReleaseSetId = "fixture:presentation-release";
                source.Levels[0].Level.LevelId = source.Levels[0].Reward.LevelId = source.Progression.Levels[0].LevelId = "level:ch01-08";
                source.NewProfile.OpenLevels = new List<string> { "level:ch01-08" };
                source.Chapter.CharacterUnlocks[0].AfterLevelId = source.Chapter.FirstClearItems[0].LevelId = "level:ch01-08";
                source.Inventory.Items.Single(x => x.ItemId == "item:experience-card-50").ItemId = "item:xp-card-50";
                source.Chapter.FirstClearItems[0].ItemId = source.Chapter.Cards[0].ItemId = "item:xp-card-50";
                var next = PublishedContentTestData.Copy(source).Levels[0];
                next.Level.LevelId = next.Reward.LevelId = "level:ch01-09"; next.Reward.RewardDefinitionId = "fixture:reward:9";
                source.Levels.Add(next);
                source.Progression.Levels.Add(new CandidateProgressionLevelInput { LevelId = "level:ch01-09", LevelVersion = "1",
                    UnlockRuleId = "fixture:unlock:9", EntryKind = CandidateProgressionEntryKind.Ordinary,
                    UnlockKind = CandidateProgressionUnlockKind.AfterWholeLevelClear, UnlockAfterLevelId = "level:ch01-08", RequiredFeatures = new List<string>() });
                var storage = new PublishedContentTestData.MemoryStorage();
                var catalog = new PublishedContentCatalog(storage, ContentConsumerCapabilities.Current);
                var job = new DemoContentDraft(source.DraftId).BeginJob(); var prepared = ChapterContentTestData.Prepare(source, job);
                Publication = Data.Content(catalog.Publish(prepared, prepared.Validation, PublishedContentTestData.Review(prepared), job,
                    "fixture:presentation-publication", PublishedContentTestData.StoreBudget())).Publication;
                storage.Blobs[PublishedContentCatalog.ReleaseSetKey("player", source.Chapter.ReleaseSetId)] = Data.Content(PublishedContentCodec.EncodeReleaseSet(new ContentReleaseSet {
                    SchemaVersion = 1, Scope = "player", ReleaseSetId = source.Chapter.ReleaseSetId, Binding = ContentBindingRecord.From(Publication.Binding),
                    PublicationReceiptSha256 = PublishedContentCodec.Sha256(Publication.ReceiptBytes.ToArray()) }, PublishedContentTestData.StoreBudget()));
                architecture = FightMatchDemoArchitecture.Interface;
                application = architecture.GetSystem<CandidateApplicationSystem>(); lifecycle = architecture.GetSystem<CandidateLifecycleApplicationSystem>();
                battle = architecture.GetSystem<CandidateBattleApplicationSystem>(); Session = architecture.GetSystem<PlayerSessionSystem>();
                try
                {
                    var profile = Data.Prepared(Session.PrepareNewProfile(catalog, source.Chapter.ReleaseSetId, Data.Codec()));
                    Data.Is(Session.CreateNew(profile, new Data.MemorySave(profile.PlayerId), new LocalPlayerProfileLocator(new PublishedContentTestData.MemoryStorage()),
                        PlayerRosterTestData.RosterCaps(Publication), Data.Budget()));
                }
                catch { architecture.Deinit(); throw; }
            }
            internal void WinAndSettle()
            {
                Data.Is(lifecycle.Submit(Data.Prepared(Session.PrepareFormationEntry(PlayerRosterTestData.EntryDraft(Head, Level), PlayerRosterTestData.Time(), Data.Codec())), Data.Budget()));
                var actions = 0;
                while (Head.Business.ActiveHistory.CurrentRun.CurrentSnapshot.Phase != BattlePhase.WonPendingSettlement)
                {
                    Assert.Less(actions++, 32); var state = Head.Business.ActiveHistory.CurrentRun.CurrentSnapshot;
                    var target = state.Enemies.First(x => x.Hp.Numerator.Sign > 0);
                    var route = source.Levels[0].SourceRoutes.Single(x => x.FaceId == target.PairKey.FaceId && x.PairId == target.PairKey.PairId).Cells
                        .Select(c => new FlowPos(c.x - 1, state.Board.Face.Height - c.y)).ToArray();
                    var request = battle.Prepare(new CandidateBattleDraft { PlayerId = Head.Business.PlayerId, ExpectedCommitId = Head.Header.CommitId,
                        Context = new PublishedRuleContext(Publication.Binding), Kind = CandidateApplicationKind.Attack,
                        Attack = new CandidateApplicationAttackInput { AttemptId = state.Baseline.Entry.AttemptId, ExpectedSceneRevision = state.SceneRevision,
                            Actor = state.Members[0].CombatantKey, Pair = target.PairKey, Route = route,
                            ExpectedPreferenceRevision = Head.Business.Inventory.PreferenceRevision, ItemUseEnabled = false } }, Data.Codec());
                    Assert.IsTrue(request.IsAccepted, request.Code);
                    var result = battle.Submit(request.Request, Data.Budget()); Data.Is(result.Application);
                    if (result.Presentation != null) battle.ReportPresentationCompleted(result.Presentation.Token);
                }
                Data.Is(lifecycle.Submit(Data.Prepared(Session.PrepareVictory(PlayerRosterTestData.Time(), Data.Codec())), Data.Budget()));
            }
            public void Dispose() => architecture.Deinit();
        }
    }
}
