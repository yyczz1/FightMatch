using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using FightMatch.Content;
using NUnit.Framework;
using static FightMatch.Core.Tests.ChapterContentTestData;

namespace FightMatch.Core.Tests
{
    public sealed class ChapterTauntBossContentTests
    {
        private const string R2Sha = "3127954bccf58fee33f983c6a5c37a307f282da5d6d66ab48fc687596cf25544";
        private const string R3AuthorRoot = "Config/FightMatch/ChapterOneTrial-r3";

        private static PublishedSource ReadR3()
        {
            var result = ChapterContentTables.Read("Generated/FightMatch/ChapterOneTrial/r3", R3AuthorRoot,
                PublishedContentTestData.Source(), ContentConsumerCapabilities.Current, Math());
            Assert.IsTrue(result.IsAccepted, result.RejectionCode + " " + result.FieldPath);
            return result.Value;
        }

        private static DemoContentJob Job(PublishedSource source)
        {
            var draft = new DemoContentDraft(source.DraftId);
            for (var revision = BigInteger.One; revision < source.Revision; revision++) draft.Revise();
            return draft.BeginJob();
        }

        private static EnemyInput WindEnemy(PublishedSource source)
        {
            return source.Levels.SelectMany(l => l.Level.Faces).SelectMany(f => f.Pairs)
                .Select(p => p.Enemy).First(e => e.WindUses.HasValue);
        }

        [Test]
        public void ActualR2KeepsCanonicalBytesAndColdAdmissionWithSevenCapabilities()
        {
            var legacy = new ContentConsumerCapabilities(ChapterRuleDefinitions.LegacyCapabilities);
            var result = ChapterContentTables.Read("Generated/FightMatch/ChapterOneTrial/r2", "Config/FightMatch/ChapterOneTrial",
                PublishedContentTestData.Source(), legacy, Math());
            Assert.IsTrue(result.IsAccepted, result.RejectionCode + " " + result.FieldPath);
            var encoded = PublishedContentTestData.Take(PublishedContentCodec.EncodeSource(result.Value, legacy, Math()));
            Assert.AreEqual(R2Sha, PublishedContentCodec.Sha256(encoded));
            var prepared = PublishedContentCompiler.Prepare(result.Value, Job(result.Value), legacy, Math());
            Assert.IsTrue(prepared.IsAccepted, prepared.RejectionCode + " " + prepared.FieldPath);
            Assert.AreEqual(R2Sha, prepared.Prepared.Binding.ContentFingerprint);
            Assert.IsFalse(prepared.Prepared.Definitions.Chapter.HasTauntBossState);
            Assert.IsEmpty(prepared.Prepared.Definitions.Chapter.Skills);
            CollectionAssert.AreEquivalent(ChapterRuleDefinitions.LegacyCapabilities, Source().RequiredCapabilities);

            const string root = "Assets/StreamingAssets/FightMatchChapterOne/";
            var cold = FirstReleaseContentStorage.Create(File.ReadAllBytes(root + "first-release.fmsource.json"),
                File.ReadAllBytes(root + "first-release.fmpackage.bytes"), File.ReadAllBytes(root + "first-release.fmvalidation.bytes"),
                File.ReadAllBytes(root + "first-release.fmreview.json"), File.ReadAllBytes(root + "first-release.fmpublish.json"),
                File.ReadAllBytes(root + "first-release.fmrelease.json"), legacy, PublishedContentTestData.StoreBudget());
            Assert.IsTrue(cold.IsAccepted, cold.RejectionCode + " " + cold.FieldPath);
            var reopened = new PublishedContentCatalog(cold.Value, legacy).ResolveExact(prepared.Prepared.Binding, legacy);
            Assert.IsTrue(reopened.IsAccepted, reopened.RejectionCode + " " + reopened.FieldPath);
            Assert.AreEqual(9, reopened.Value.Definitions.Levels.Count);
        }

        [Test]
        public void ActualR3PreparesSixteenLevelsAndFreezesTheCompleteTeachingAndBossDefinitions()
        {
            var source = ReadR3();
            var set = string.Concat(Directory.GetFiles(R3AuthorRoot, "*.csv").OrderBy(Path.GetFileName, StringComparer.Ordinal)
                .Select(path => Path.GetFileName(path) + ":" + PublishedContentCodec.Sha256(File.ReadAllBytes(path)) + "\n"));
            Assert.AreEqual("867a9ab630e8d6d6201fe3d5d8b71c8db4cdec016c5b348967242523eebf7462",
                PublishedContentCodec.Sha256(Encoding.UTF8.GetBytes(set)));
            Assert.AreEqual(new BigInteger(3), source.Revision);
            CollectionAssert.AreEquivalent(ChapterRuleDefinitions.StateCapabilities, source.RequiredCapabilities);
            Assert.AreEqual(17, source.Sources.Count(s => s.Path.StartsWith("Generated/FightMatch/ChapterOneTrial/r3/", StringComparison.Ordinal)));
            var outcome = PublishedContentCompiler.Prepare(source, Job(source), ContentConsumerCapabilities.Current, Math());
            Assert.IsTrue(outcome.IsAccepted, outcome.RejectionCode + " " + outcome.FieldPath);
            var prepared = outcome.Prepared;
            var chapter = prepared.Definitions.Chapter;
            Assert.AreEqual(16, prepared.Definitions.Levels.Count);
            Assert.AreEqual(17, prepared.Definitions.Levels.Sum(l => l.Faces.Count));
            Assert.IsTrue(chapter.HasTauntBossState);
            Assert.IsTrue(chapter.Binding.Same(prepared.Binding));
            Assert.IsEmpty(prepared.Replays, "Geometry and parameter proofs do not claim a completed battle replay.");
            var skill = chapter.FindSkill("skill:warrior-taunt");
            Assert.AreEqual("CH1.MageTauntBoss", skill.BattleCapabilityId);
            Assert.AreEqual(2, skill.CooldownTurns);
            Assert.AreEqual(new BigInteger(5), skill.MinimumLevel);
            Assert.AreEqual(BigInteger.One, skill.CostCount);
            var teaching = chapter.FindTeaching("tutorial:l16-taunt");
            Assert.AreSame(teaching, chapter.FindTeaching(teaching.Level));
            Assert.AreEqual("Level16EntryGift", teaching.GiftOnceKey);
            Assert.AreEqual("fm.ch1.tutorial.taunt.explanation", teaching.ExplanationTextKey);
            Assert.AreEqual(BigInteger.One, teaching.GiftCount);
            var permanent = prepared.Definitions.GetPermanentDefinitions();
            Assert.AreEqual(teaching.GiftOnceKey, permanent.Find(CandidatePermanentDefinitionKind.Teaching, teaching.TutorialId).GiftOnceKey);
            Assert.AreEqual(skill.BattleCapabilityId, permanent.Find(CandidatePermanentDefinitionKind.Skill, skill.SkillId).BattleCapabilityId);

            var boss = prepared.Definitions.Levels.Single(l => l.LevelId == "level:ch01-16");
            Assert.AreEqual(2, boss.Faces.Count);
            var enemies = boss.Faces.SelectMany(f => f.Pairs).Select(p => p.Enemy).ToList();
            Assert.IsTrue(enemies.All(e => e.Behavior == EnemyBehavior.ChapterCycle && e.InitialIntentCursor == BigInteger.Zero));
            Assert.IsTrue(enemies.Any(e => e.WindUses == 2 && e.WindTargetEnemyInstanceKey != null));
            Assert.IsTrue(enemies.SelectMany(e => e.IntentCycle).Any(i => i.Targeting == EnemyTargeting.PreferMage &&
                i.AttackRange == 3 && i.AttackOverride != null && i.AttackOverride.Compare(Q(26), Math()) == 0));
            Assert.IsTrue(enemies.SelectMany(e => e.IntentCycle).Any(i => i.NextActionVulnerability != null &&
                i.NextActionVulnerability.Compare(Q(5, 4), Math()) == 0));
            source.Chapter.Skills[0].CooldownTurns = 99;
            source.Chapter.Teachings[0].GiftOnceKey = "changed";
            Assert.AreEqual(2, skill.CooldownTurns);
            Assert.AreEqual("Level16EntryGift", teaching.GiftOnceKey);
        }

        [Test]
        public void R3SourceRoundTripPreservesEveryFrozenEnemyAndTeachingField()
        {
            var source = ReadR3();
            var bytes = PublishedContentTestData.Take(PublishedContentCodec.EncodeSource(source, ContentConsumerCapabilities.Current, Math()));
            var restored = PublishedContentTestData.Take(PublishedContentCodec.DecodeSource(bytes, ContentConsumerCapabilities.Current, Math()));
            var again = PublishedContentTestData.Take(PublishedContentCodec.EncodeSource(restored, ContentConsumerCapabilities.Current, Math()));
            CollectionAssert.AreEqual(bytes, again);
            var before = WindEnemy(source);
            var after = WindEnemy(restored);
            Assert.AreEqual(before.InitialIntentCursor, after.InitialIntentCursor);
            Assert.AreEqual(before.WindTargetEnemyInstanceKey, after.WindTargetEnemyInstanceKey);
            Assert.AreEqual(before.WindUses, after.WindUses);
            Assert.Zero(before.WindFraction.Compare(after.WindFraction, Math()));
            Assert.AreEqual(before.IntentCycle.Single(i => i.Kind == EnemyIntentKind.Wind).ExhaustedFallbackIndex,
                after.IntentCycle.Single(i => i.Kind == EnemyIntentKind.Wind).ExhaustedFallbackIndex);
            Assert.AreEqual(source.Chapter.Teachings[0].GiftOnceKey, restored.Chapter.Teachings[0].GiftOnceKey);
            var legacy = new ContentConsumerCapabilities(ChapterRuleDefinitions.LegacyCapabilities);
            var unsupported = PublishedContentCompiler.Prepare(restored, Job(restored), legacy, Math());
            Assert.IsFalse(unsupported.IsAccepted);
            Assert.AreEqual("UnsupportedCapability", unsupported.RejectionCode);
        }

        [TestCase("MissingState")]
        [TestCase("Unknown")]
        [TestCase("Duplicate")]
        public void CapabilityDeclarationsCannotDowngradeOrPartiallyEnableNewContent(string mutation)
        {
            var source = ReadR3();
            if (mutation == "MissingState") source.RequiredCapabilities.Remove(ChapterRuleDefinitions.StateFeatureId);
            if (mutation == "Unknown") source.RequiredCapabilities.Add("CH1.Unknown");
            if (mutation == "Duplicate") source.RequiredCapabilities.Add(ChapterRuleDefinitions.StateFeatureId);
            var result = PublishedContentCompiler.Prepare(source, Job(source), ContentConsumerCapabilities.Current, Math());
            Assert.IsFalse(result.IsAccepted, mutation);
            Assert.AreEqual("UnsupportedCapability", result.RejectionCode);
        }

        [TestCase("SkillCapability")]
        [TestCase("MinimumLevel")]
        [TestCase("CostCount")]
        [TestCase("Cooldown")]
        [TestCase("Class")]
        [TestCase("GiftItem")]
        [TestCase("GiftOnceKey")]
        [TestCase("GiftCount")]
        [TestCase("TeachingLevel")]
        [TestCase("TextNamespace")]
        [TestCase("SharedStep")]
        public void TeachingAndSkillRowsMustMatchTheExistingPermanentContract(string mutation)
        {
            var source = ReadR3();
            var skill = source.Chapter.Skills[0];
            var teaching = source.Chapter.Teachings[0];
            switch (mutation)
            {
                case "SkillCapability": skill.BattleCapabilityId = ChapterRuleDefinitions.StateFeatureId; break;
                case "MinimumLevel": skill.MinimumLevel = 3; break;
                case "CostCount": skill.CostCount = 2; break;
                case "Cooldown": skill.CooldownTurns = 3; break;
                case "Class": skill.ClassId = "class:mage"; break;
                case "GiftItem": teaching.GiftItemId = "item:missing"; break;
                case "GiftOnceKey": teaching.GiftOnceKey = "Level16EntryGift-r3"; break;
                case "GiftCount": teaching.GiftCount = 2; break;
                case "TeachingLevel": teaching.LevelVersion = 99; break;
                case "TextNamespace": teaching.ExplanationTextKey = "ch1.tutorial.taunt.explanation"; break;
                case "SharedStep": teaching.ExplanationStepId = teaching.LearningStepId; break;
            }
            var result = PublishedContentCompiler.Prepare(source, Job(source), ContentConsumerCapabilities.Current, Math());
            Assert.IsFalse(result.IsAccepted, mutation);
        }

        [TestCase("Cursor")]
        [TestCase("WindOtherFace")]
        [TestCase("WindSelf")]
        [TestCase("WindUses")]
        [TestCase("WindFraction")]
        [TestCase("FallbackChain")]
        [TestCase("MissingRange")]
        [TestCase("Vulnerability")]
        public void EnemyRowsRejectIncompleteOrCrossInstanceStateBindings(string mutation)
        {
            var source = ReadR3();
            var enemy = WindEnemy(source);
            switch (mutation)
            {
                case "Cursor": enemy.InitialIntentCursor = enemy.IntentCycle.Count; break;
                case "WindOtherFace": enemy.WindTargetEnemyInstanceKey = source.Levels.Last().Level.Faces.Last().Pairs.Last().Enemy.EnemyInstanceKey; break;
                case "WindSelf": enemy.WindTargetEnemyInstanceKey = enemy.EnemyInstanceKey; break;
                case "WindUses": enemy.WindUses = 0; break;
                case "WindFraction": enemy.WindFraction = Q(3, 2); break;
                case "FallbackChain": enemy.IntentCycle[1].ExhaustedFallbackIndex = 1; break;
                case "MissingRange": enemy.IntentCycle[0].AttackRange = null; break;
                case "Vulnerability": enemy.IntentCycle[0].NextActionVulnerability = Q(1); break;
            }
            var result = PublishedContentCompiler.Prepare(source, Job(source), ContentConsumerCapabilities.Current, Math());
            Assert.IsFalse(result.IsAccepted, mutation);
        }
    }
}
