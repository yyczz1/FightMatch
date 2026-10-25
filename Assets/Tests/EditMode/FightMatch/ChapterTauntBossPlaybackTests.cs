using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FightMatch.Application;
using FightMatch.Host;
using FightMatch.Presentation;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using static FightMatch.Core.Tests.ChapterBattleTestData;

namespace FightMatch.Core.Tests
{
    public sealed class ChapterTauntBossPlaybackTests
    {
        private ILocalizedTextSource textSource;
        private ChapterTransientTestFont font;

        [SetUp]
        public void CreateIsolatedFont() { font = new ChapterTransientTestFont(); }
        [TearDown]
        public void ReleaseIsolatedFont() { font?.Dispose(); font = null; }

        [Serializable]
        private sealed class TextManifest
        {
            public string sourceRevision;
            public string formalSourceSha256;
            public string canonicalArtifactSha256;
            public int recordCount;
        }

        [OneTimeSetUp]
        public void LoadActualGenerated310Text()
        {
            const string root = "Generated/FightMatch/Localization/ChapterOneTrial-r3/";
            var manifest = JsonUtility.FromJson<TextManifest>(File.ReadAllText(root + "fm-text-v1.manifest.json"));
            Assert.AreEqual("chapter-one-trial-r3", manifest.sourceRevision);
            Assert.AreEqual("42823e4e841aba88e0ca2740a561c326252fc80c3c23e08d3572426a37ffcab8", manifest.formalSourceSha256);
            Assert.AreEqual(310, manifest.recordCount);
            Assert.IsTrue(FightMatchLocalizationCatalogParser.TryParse(File.ReadAllBytes(root + "fm-text-v1.json"),
                manifest.canonicalArtifactSha256, 310, out var source, out var error), error);
            textSource = source;
        }

        [TestCase(15)]
        [TestCase(5)]
        [TestCase(101)]
        public void WindPlaybackUsesCommittedFractionalOrZeroHealingAndIndependentRemainingUses(int damage)
        {
            var input = WindInput(damage);
            var run = Act(Run(Bind(input)), 0, "wind");
            var record = run.Records.Single();
            var presentation = Present(record);
            var frame = CandidateBattlePlaybackFrame.Initial(presentation, record.Request.Route);
            var random = record.AfterSnapshot.Random;
            var winds = 0;
            foreach (var fact in record.OrderedFacts)
            {
                frame = frame.Apply(presentation, fact);
                if (fact.EnemyIntent?.Wind == null) continue;
                winds++;
                var wind = fact.EnemyIntent.Wind;
                Same(wind.HpAfter, Actor(frame, wind.TargetEnemyKey).Hp);
                Assert.AreEqual(wind.UsesAfter, Actor(frame, fact.EnemyIntent.EnemyKey).RemainingWindUses);
                if (damage == 15 && winds == 1) Value(Actor(frame, wind.TargetEnemyKey).Hp, 961, 10);
                if (damage != 15 && winds == 2)
                {
                    Value(wind.ActualHealing, 0);
                    Assert.AreEqual(wind.UsesBefore, wind.UsesAfter);
                    Same(wind.HpBefore, Actor(frame, wind.TargetEnemyKey).Hp);
                }
                foreach (var locale in Locales)
                {
                    var service = Service(locale);
                    ExpectBeat(service, frame, presentation, "fm.battle.playback.wind",
                        BattleText.Arg("enemyName", BattleText.Enemy(service, record.BeforeSnapshot, fact.EnemyIntent.Pair)),
                        BattleText.Arg("targetName", BattleText.Enemy(service, record.BeforeSnapshot, wind.TargetEnemyKey)),
                        BattleText.Arg("hpBefore", BattleText.Hp(wind.HpBefore)), BattleText.Arg("hpAfter", BattleText.Hp(wind.HpAfter)),
                        BattleText.Arg("remainingUses", wind.UsesAfter.ToString()));
                }
            }
            Assert.AreEqual(2, winds);
            AssertFrameMatches(record.AfterSnapshot, frame);
            Assert.AreSame(random, record.AfterSnapshot.Random, "Display projection must not sample or replace committed state.");
        }

        [Test]
        public void ExhaustedWindShowsExecutedStrikeAndActualMageTarget()
        {
            var run = Run(Bind(WindInput(15)));
            for (var i = 0; i < 4; i++) run = Act(run, 0, "wind:" + i);
            var exhausted = CandidateBattlePlaybackFrame.From(run.CurrentSnapshot);
            foreach (var worker in run.CurrentSnapshot.Enemies.Skip(1))
                foreach (var locale in Locales)
                {
                    var service = Service(locale);
                    Assert.AreEqual(0, worker.RemainingWindUses);
                    Assert.AreEqual(4, (int)worker.IntentCursor);
                    var hud = BattleText.EnemyIntent(service, worker.Enemy, Actor(exhausted, worker.CombatantKey));
                    Assert.AreEqual(service.Resolve("fm.battle.intent.ranged", null).Text, hud.Arguments.Single().Value);
                    BindActualText(service, hud);
                }
            run = Act(run, 0, "wind:4");
            var record = run.Records.Last();
            var presentation = Present(record);
            var frame = CandidateBattlePlaybackFrame.Initial(presentation, record.Request.Route);
            var fallbacks = 0;
            foreach (var fact in record.OrderedFacts)
            {
                frame = frame.Apply(presentation, fact);
                var intent = fact.EnemyIntent;
                if (intent?.IntentKind != EnemyIntentKind.Wind) continue;
                fallbacks++;
                Assert.IsNull(intent.Wind);
                Assert.AreEqual(1, intent.ExecutedIntentIndex);
                Assert.AreEqual("mage", intent.Damage.TargetMember.CharacterId);
                Assert.AreEqual(0, Actor(frame, intent.EnemyKey).RemainingWindUses);
                Same(intent.Damage.HpAfter, Actor(frame, intent.Damage.TargetMember).Hp);
                foreach (var locale in Locales)
                {
                    var service = Service(locale);
                    ExpectBeat(service, frame, presentation, "fm.battle.playback.enemy_strike",
                        BattleText.Arg("characterName", BattleText.Character(service, record.BeforeSnapshot, intent.Damage.TargetMember)),
                        BattleText.Arg("enemyName", BattleText.Enemy(service, record.BeforeSnapshot, intent.Pair)),
                        BattleText.Arg("hpBefore", BattleText.Hp(intent.Damage.HpBefore)), BattleText.Arg("hpAfter", BattleText.Hp(intent.Damage.HpAfter)));
                }
            }
            Assert.AreEqual(2, fallbacks);
            AssertFrameMatches(record.AfterSnapshot, frame);
        }

        [Test]
        public void SkillOnlyTauntAndLaterCooldownFactsProjectWithoutDamageOrStageAssumptions()
        {
            var input = Named(StateInput());
            input.Level.Faces[0].Pairs[2].Enemy.IntentCycle[0] = Strike(EnemyTargeting.PreferMage);
            var run = Act(Run(Bind(input)), 2, "taunt", "warrior");
            var taunt = run.Records.Single();
            Assert.IsEmpty(taunt.DirectAttack.DamageFacts);
            Assert.AreEqual("warrior", taunt.EnemyPhase.OrderedIntents[2].TauntCaster.CharacterId);
            run = Act(run, 0, "cooldown:1");
            run = Act(run, 0, "cooldown:0");
            var casts = 0;
            var cooldowns = new List<int>();
            foreach (var record in run.Records)
            {
                var presentation = Present(record);
                var frame = CandidateBattlePlaybackFrame.Initial(presentation, record.Request.Route);
                foreach (var fact in record.OrderedFacts)
                {
                    frame = frame.Apply(presentation, fact);
                    if (fact.SkillCast != null)
                    {
                        casts++;
                        Assert.AreEqual(2, Actor(frame, fact.SkillCast.CasterKey).CooldownRemaining);
                        foreach (var locale in Locales)
                        {
                            var service = Service(locale);
                            ExpectBeat(service, frame, presentation, "fm.battle.playback.taunt",
                                BattleText.Arg("characterName", BattleText.Character(service, record.BeforeSnapshot, fact.SkillCast.CasterKey)),
                                BattleText.Arg("enemyName", BattleText.Enemy(service, record.BeforeSnapshot, fact.SkillCast.TargetEnemyKey)));
                        }
                    }
                    if (fact.Cooldown != null)
                    {
                        cooldowns.Add(fact.Cooldown.After);
                        Assert.AreEqual(fact.Cooldown.After, Actor(frame, fact.Cooldown.CombatantKey).CooldownRemaining);
                        foreach (var locale in Locales)
                        {
                            var service = Service(locale);
                            var skillName = service.Resolve("fm.name.skill.warrior_taunt", null).Text;
                            if (fact.Cooldown.After == 0)
                                ExpectBeat(service, frame, presentation, "fm.battle.skill.ready", BattleText.Arg("skillName", skillName));
                            else ExpectBeat(service, frame, presentation, "fm.battle.skill.cooldown",
                                BattleText.Arg("skillName", skillName), BattleText.Arg("remainingTurns", fact.Cooldown.After.ToString()));
                        }
                    }
                }
                AssertFrameMatches(record.AfterSnapshot, frame);
                AssertLocalizedFrame(record.AfterSnapshot, frame);
            }
            Assert.AreEqual(1, casts);
            CollectionAssert.AreEqual(new[] { 1, 0 }, cooldowns);
        }

        [Test]
        public void HeavyRestAndVulnerabilityWindowAppearAndExpireFromActualFacts()
        {
            var input = Named(StateInput(2, false));
            var heavy = Strike(range: 1, attack: 12);
            heavy.DamageCoefficient = R(2);
            heavy.NextActionVulnerability = R(3, 2);
            input.Level.Faces[0].Pairs[0].Enemy.IntentCycle = new List<EnemyIntentInput> {
                heavy, new EnemyIntentInput { Kind = EnemyIntentKind.Rest } };
            var run = Run(Bind(input));
            foreach (var locale in Locales)
            {
                var service = Service(locale);
                var initial = CandidateBattlePlaybackFrame.From(run.CurrentSnapshot);
                var enemy = run.CurrentSnapshot.Enemies[0];
                var hud = BattleText.EnemyIntent(service, enemy.Enemy, Actor(initial, enemy.CombatantKey));
                Assert.AreEqual(service.Resolve("fm.battle.intent.heavy", null).Text, hud.Arguments.Single().Value);
                BindActualText(service, hud);
            }
            run = Act(run, 1, "expose");
            run = Act(run, 1, "expire");
            var changes = new List<ChapterVulnerabilityChange>();
            var rests = 0;
            foreach (var record in run.Records)
            {
                var presentation = Present(record);
                var frame = CandidateBattlePlaybackFrame.Initial(presentation, record.Request.Route);
                foreach (var fact in record.OrderedFacts)
                {
                    frame = frame.Apply(presentation, fact);
                    if (fact.Vulnerability != null)
                    {
                        var change = fact.Vulnerability;
                        changes.Add(change.Change);
                        Assert.AreEqual(change.Change == ChapterVulnerabilityChange.Created,
                            Actor(frame, change.Window.TargetEnemyKey).IsVulnerable);
                        AssertLocalizedFrame(record.BeforeSnapshot, frame);
                    }
                    if (fact.EnemyIntent?.IntentKind == EnemyIntentKind.Rest) rests++;
                    foreach (var locale in Locales) BeatText(Service(locale), frame, presentation);
                }
                AssertFrameMatches(record.AfterSnapshot, frame);
            }
            Assert.AreEqual(1, rests);
            CollectionAssert.AreEqual(new[] { ChapterVulnerabilityChange.Created, ChapterVulnerabilityChange.Expired }, changes);
        }

        [Test]
        public void CrossFacePlaybackKeepsKnownFinalHpCooldownAndNewEnemyIntent()
        {
            var input = StateInput(1);
            input.Level.Faces[0].Pairs[0].Enemy.Stats.MaxHp = R(20);
            AddStateFace(input);
            Named(input);
            var run = Act(Run(Bind(input)), 0, "face1:0", "warrior");
            run = Act(run, 0, "face1:1", "warrior");
            var record = run.Records.Last();
            Assert.AreNotEqual(record.BeforeSnapshot.Board.Face.FaceId, record.AfterSnapshot.Board.Face.FaceId);
            Assert.AreEqual(1, record.AfterSnapshot.ChapterState.MemberSkills.Single().CooldownRemaining);
            var presentation = Present(record);
            var frame = CandidateBattlePlaybackFrame.Initial(presentation, record.Request.Route);
            Assert.IsTrue(frame.UsesKnownFinalState);
            foreach (var fact in record.OrderedFacts)
            {
                frame = frame.Apply(presentation, fact);
                AssertFrameMatches(record.AfterSnapshot, frame);
                AssertLocalizedFrame(record.AfterSnapshot, frame);
                foreach (var locale in Locales) BeatText(Service(locale), frame, presentation);
            }
        }

        [Test]
        public void AllNewEnemyNamesAndWindIntentUseRealBilingualBindings()
        {
            var input = WindInput(15);
            var names = new[] { "enemy:spring-thrower", "enemy:mechanic", "enemy:bear-shell", "enemy:heavy-guard", "enemy:bear-core" };
            foreach (var name in names)
            {
                input.Level.Faces[0].Pairs[0].Enemy.EnemyDefinitionId = name;
                var state = Run(Bind(input)).CurrentSnapshot;
                var frame = CandidateBattlePlaybackFrame.From(state);
                foreach (var locale in Locales)
                {
                    var service = Service(locale);
                    BindActualText(service, BattleText.EnemyName(service, state.Enemies[0].Enemy));
                    var worker = state.Enemies[1];
                    var wind = BattleText.EnemyIntent(service, worker.Enemy, Actor(frame, worker.CombatantKey));
                    Assert.AreEqual(service.Resolve("fm.battle.intent.wind", null).Text, wind.Arguments.Single().Value);
                    BindActualText(service, wind);
                }
            }
        }

        private static readonly LocaleId[] Locales = { LocaleId.En, LocaleId.ZhHans };
        private LocalizationService Service(LocaleId locale)
        {
            var service = new LocalizationService(textSource, SystemLanguage.English);
            service.SetLocale(locale);
            return service;
        }
        private static BattleEntryInput Named(BattleEntryInput input)
        {
            foreach (var face in input.Level.Faces)
                for (var i = 0; i < face.Pairs.Count; i++)
                {
                    face.Pairs[i].Enemy.OriginalSlot = i;
                    face.Pairs[i].Enemy.EnemyDefinitionId = i == 0 ? "enemy:bear-core" : "enemy:mechanic";
                }
            return input;
        }
        private static BattleEntryInput WindInput(int damage)
        {
            var input = Named(StateInput(3, false));
            input.Members[1].Stats.Attack = R(damage);
            input.Level.Faces[0].Pairs[0].Enemy.Stats.MaxHp = R(101);
            foreach (var pair in input.Level.Faces[0].Pairs.Skip(1))
            {
                pair.Enemy.WindTargetEnemyInstanceKey = "enemy:0";
                pair.Enemy.WindUses = 2;
                pair.Enemy.WindFraction = R(1, 10);
                pair.Enemy.IntentCycle = new List<EnemyIntentInput> {
                    new EnemyIntentInput { Kind = EnemyIntentKind.Wind, ExhaustedFallbackIndex = 1 },
                    Strike(EnemyTargeting.PreferMage, attack: 26) };
            }
            return input;
        }
        private static CandidateBattlePresentation Present(CandidateBattleOperationRecord record) =>
            new CandidateBattlePresentation("fixture:taunt-boss-playback", record, null);
        private static CandidatePlaybackActor Actor(CandidateBattlePlaybackFrame frame, BattleCombatantKey key) =>
            frame.Actors.Single(x => x.Key.Equals(key));
        private static void Same(ExactRational expected, ExactRational actual)
        {
            Assert.AreEqual(expected.Numerator, actual.Numerator);
            Assert.AreEqual(expected.Denominator, actual.Denominator);
        }
        private static void AssertFrameMatches(BattleSnapshot state, CandidateBattlePlaybackFrame frame)
        {
            Assert.AreEqual(state.Board.Face.FaceId, frame.Face.FaceId);
            foreach (var member in state.Members)
            {
                var actor = Actor(frame, member.CombatantKey);
                Same(member.Hp, actor.Hp);
                var skill = state.ChapterState.MemberSkills.SingleOrDefault(x => x.CombatantKey.Equals(member.CombatantKey));
                Assert.AreEqual(skill?.CooldownRemaining, actor.CooldownRemaining);
            }
            foreach (var enemy in state.Enemies)
            {
                var actor = Actor(frame, enemy.CombatantKey);
                Same(enemy.Hp, actor.Hp);
                Assert.AreEqual(enemy.IntentCursor, actor.IntentCursor);
                Assert.AreEqual(enemy.RemainingWindUses, actor.RemainingWindUses);
                Assert.AreEqual(state.ChapterState.Vulnerabilities.Any(x => x.TargetEnemyKey.Equals(enemy.CombatantKey)), actor.IsVulnerable);
            }
        }
        private void AssertLocalizedFrame(BattleSnapshot state, CandidateBattlePlaybackFrame frame)
        {
            foreach (var locale in Locales)
            {
                var service = Service(locale);
                foreach (var enemy in state.Enemies)
                {
                    var actor = Actor(frame, enemy.CombatantKey);
                    var line = BattleText.EnemyIntent(service, enemy.Enemy, actor);
                    BindActualText(service, line);
                    var vulnerability = service.Resolve("fm.battle.enemy.vulnerable", null).Text;
                    if (actor.IsVulnerable && !actor.Hp.Numerator.IsZero)
                        StringAssert.Contains(vulnerability, line.Resolve(service));
                    else StringAssert.DoesNotContain(vulnerability, line.Resolve(service));
                }
                foreach (var actor in frame.Actors.Where(x => x.CooldownRemaining.HasValue))
                    BindActualText(service, BattleText.SkillCooldown(service, actor.SkillId, actor.CooldownRemaining));
            }
        }
        private void ExpectBeat(LocalizationService service, CandidateBattlePlaybackFrame frame,
            CandidateBattlePresentation original, string key, params KeyValuePair<string, string>[] arguments)
        {
            var expected = service.Resolve(key, arguments);
            Assert.IsTrue(expected.IsSuccess, expected.DiagnosticCode);
            Assert.AreEqual(expected.Text, BeatText(service, frame, original));
        }
        private string BeatText(LocalizationService service, CandidateBattlePlaybackFrame frame, CandidateBattlePresentation original)
        {
            var line = BattleText.Beat(service, frame, original);
            BindActualText(service, line);
            return line.Arguments.Single(x => x.Key == "beat").Value;
        }
        private void BindActualText(LocalizationService service, BattleTextLine line)
        {
            var go = new GameObject("ChapterFactText", typeof(RectTransform), typeof(TMPro.TextMeshProUGUI), typeof(LocalizedTmpText));
            try
            {
                font.Bind(go.GetComponent<TMPro.TextMeshProUGUI>());
                var label = go.GetComponent<LocalizedTmpText>();
                line.Bind(label, service);
                Assert.IsNull(label.DiagnosticCode, line.Key);
                Assert.AreEqual(line.Resolve(service), label.Target.text);
                StringAssert.DoesNotContain(LocalizedTmpText.Placeholder, label.Target.text);
            }
            finally
            {
                go.GetComponent<LocalizedTmpText>().Unbind();
                UnityEngine.Object.DestroyImmediate(go);
            }
        }
    }

    // The original source font and metrics stay the same; all writable TMP objects belong to this test.
    internal sealed class ChapterTransientTestFont : IDisposable
    {
        internal TMPro.TMP_FontAsset Font { get; private set; }
        private readonly List<UnityEngine.Object> owned = new List<UnityEngine.Object>();

        internal ChapterTransientTestFont()
        {
            try
            {
                var source = AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>(
                    "Assets/UI/FightMatch/Fonts/NotoSansCJKsc-Regular-TMP.asset");
                Assert.IsNotNull(source); Assert.IsNotNull(source.material);
                // This project's font has no linked fallback or alternate typeface assets to isolate.
                Assert.IsTrue(source.fallbackFontAssetTable == null || source.fallbackFontAssetTable.Count == 0);
                Assert.IsTrue(source.fontWeightTable.All(x => x.regularTypeface == null && x.italicTypeface == null));
                var sourceAtlases = source.atlasTextures;
                Assert.IsNotNull(sourceAtlases);
                var activeAtlasCount = source.atlasTextureCount;
                Assert.GreaterOrEqual(activeAtlasCount, 1);
                Assert.LessOrEqual(activeAtlasCount, sourceAtlases.Length);
                for (var i = 0; i < activeAtlasCount; i++)
                    Assert.IsTrue(sourceAtlases[i] != null, "An active atlas slot must contain a texture: " + i);
                foreach (var glyph in source.glyphTable)
                {
                    Assert.GreaterOrEqual(glyph.atlasIndex, 0);
                    Assert.Less(glyph.atlasIndex, activeAtlasCount);
                    Assert.IsTrue(sourceAtlases[glyph.atlasIndex] != null, "A glyph must reference an assigned atlas.");
                }
                Font = Own(UnityEngine.Object.Instantiate(source));
                Font.name = source.name + " (Chapter test transient)";
                // TMP doubles this array's capacity but assigns only the active slots. Keep glyph atlas indices stable.
                var atlases = new Texture2D[sourceAtlases.Length];
                for (var i = 0; i < sourceAtlases.Length; i++)
                    if (sourceAtlases[i] != null) atlases[i] = Own(UnityEngine.Object.Instantiate(sourceAtlases[i]));
                Font.atlasTextures = atlases;
                Font.material = Own(UnityEngine.Object.Instantiate(source.material));
                Font.material.mainTexture = atlases[0];
                Font.ReadFontAssetDefinition();
                Assert.AreNotSame(source.characterTable, Font.characterTable);
                Assert.AreNotSame(source.glyphTable, Font.glyphTable);
                Assert.IsTrue(Font.characterTable.All(character => character.textAsset == Font));
                Assert.AreSame(atlases[0], Font.atlasTexture);
                Assert.AreSame(source.sourceFontFile, Font.sourceFontFile);
                Assert.IsFalse(EditorUtility.IsPersistent(Font));
                Assert.IsFalse(EditorUtility.IsPersistent(Font.material));
                Assert.AreEqual(activeAtlasCount, Font.atlasTextureCount);
                for (var i = 0; i < atlases.Length; i++)
                {
                    if (sourceAtlases[i] == null) { Assert.IsTrue(atlases[i] == null); continue; }
                    Assert.IsTrue(atlases[i] != null);
                    Assert.AreNotSame(sourceAtlases[i], atlases[i]);
                    Assert.IsFalse(EditorUtility.IsPersistent(atlases[i]));
                }
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        private T Own<T>(T value) where T : UnityEngine.Object
        {
            Assert.IsNotNull(value);
            value.hideFlags = HideFlags.HideAndDontSave;
            owned.Add(value);
            return value;
        }

        internal void Bind(TMPro.TMP_Text text)
        {
            text.font = Font;
            text.fontSharedMaterial = Font.material;
            Assert.AreSame(Font, text.font);
            Assert.AreSame(Font.material, text.fontSharedMaterial);
        }

        internal void BindRoot(GameObject root)
        {
            Assert.IsFalse(root.activeInHierarchy, "Isolate the prefab instance before its first localization or rendering.");
            foreach (var text in root.GetComponentsInChildren<TMPro.TMP_Text>(true)) Bind(text);
            foreach (var input in root.GetComponentsInChildren<TMPro.TMP_InputField>(true)) input.fontAsset = Font;
            Assert.IsTrue(root.GetComponentsInChildren<TMPro.TMP_Text>(true).All(text => text.font == Font));
        }

        public void Dispose()
        {
            // Dynamic addition can allocate more atlas pages after construction. They are owned by this transient font too.
            if (Font != null)
                foreach (var atlas in Font.atlasTextures)
                    if (atlas != null && !EditorUtility.IsPersistent(atlas) && !owned.Contains(atlas)) owned.Add(atlas);
            for (var i = owned.Count - 1; i >= 0; i--)
                if (owned[i] != null && !EditorUtility.IsPersistent(owned[i])) UnityEngine.Object.DestroyImmediate(owned[i]);
            owned.Clear();
            Font = null;
        }
    }
}
