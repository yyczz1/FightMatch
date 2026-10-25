using System;
using System.IO;
using System.Linq;
using System.Text;
using FightMatch.Content;
using NUnit.Framework;
using static FightMatch.Core.Tests.ChapterContentTestData;

namespace FightMatch.Core.Tests
{
    public sealed class ChapterContentTests
    {
        [Test]
        public void SchemaOneCanonicalBytesKeepTheOriginalFieldGraph()
        {
            var source = PublishedContentTestData.Source();
            var encoded = PublishedContentCodec.EncodeSource(source, ContentConsumerCapabilities.Current, Math());
            Assert.That(encoded.IsAccepted, Is.True, encoded.RejectionCode);
            Assert.That(PublishedContentCodec.Sha256(encoded.Value), Is.EqualTo("a07fd03f2c9db7f2aa69e67af3722b449f753eb172355d564ff39af4c987a906"));
            StringAssert.DoesNotContain("Penetration", Encoding.UTF8.GetString(encoded.Value));
            StringAssert.DoesNotContain("Growths", Encoding.UTF8.GetString(encoded.Value));
        }

        [TestCase("24.64", "616", "25")]
        [TestCase("616/25", "616", "25")]
        [TestCase("0", "0", "1")]
        [TestCase("0.005", "1", "200")]
        public void TableRationalsRetainExactValues(string token, string numerator, string denominator)
        {
            var result = ChapterContentTables.ParseRational(token, Math());
            Assert.That(result.IsAccepted, Is.True, result.RejectionCode);
            Assert.That(result.Value.Numerator.ToString(), Is.EqualTo(numerator)); Assert.That(result.Value.Denominator.ToString(), Is.EqualTo(denominator));
        }
        [TestCase("1/0")]
        [TestCase("50%")]
        [TestCase("1e-3")]
        [TestCase("1,000")]
        [TestCase("NaN")]
        [TestCase("")]
        public void TableRationalsRejectAmbiguousOrInvalidTokens(string token)
        { Assert.That(ChapterContentTables.ParseRational(token, Math()).IsAccepted, Is.False); }

        [Test]
        public void AuthorEndpointConvertsTopLeftOnlyOnce()
        {
            var cell = ChapterContentTables.Endpoint(1, 4, 5, 5);
            Assert.That(cell.x, Is.EqualTo(3)); Assert.That(cell.y, Is.EqualTo(4));
        }

        [Test]
        public void FrozenNormalChapterTablesPrepareTheCompleteOneThroughNineClosure()
        {
            const string directory = "Config/FightMatch/ChapterOneTrial";
            var seed = PublishedContentTestData.Source();
            var source = ChapterContentTables.Read("Generated/FightMatch/ChapterOneTrial/r2", directory, seed, ContentConsumerCapabilities.Current, Math());
            Assert.That(source.IsAccepted, Is.True, source.RejectionCode + " " + source.FieldPath);
            Assert.That(source.Value.Levels.Count, Is.EqualTo(9));
            Assert.That(source.Value.Chapter.ParameterStatus, Is.EqualTo(ChapterParameterStatus.Trial));
            Assert.That(source.Value.Revision.ToString(), Is.EqualTo("2"));
            Assert.That(seed.SchemaVersion, Is.EqualTo(1)); Assert.That(seed.Growths, Is.Null);
            var draft = new DemoContentDraft(source.Value.DraftId); draft.Revise();
            var prepared = Prepare(source.Value, draft.BeginJob());
            Assert.That(prepared.Definitions.Levels.Select(l => l.LevelId), Is.EquivalentTo(Enumerable.Range(1, 9).Select(i => "level:ch01-" + i.ToString("00"))));
            Assert.That(prepared.Definitions.Chapter.CharacterUnlocks.Single().AfterLevelId, Is.EqualTo("level:ch01-08"));
            Assert.That(prepared.Definitions.Progression.Levels.Single(l => l.LevelId == "level:ch01-09").UnlockAfterLevelId, Is.EqualTo("level:ch01-08"));
            Assert.That(prepared.Binding.PackageId, Is.EqualTo("fightmatch:chapter-one-trial"));
            Assert.That(prepared.Binding.RuleVersion, Is.EqualTo(ChapterRuleDefinitions.RuleVersion));
            Assert.That(prepared.NewProfile.CharacterId, Is.EqualTo("W"));
        }

        [Test]
        public void TableCoordinateFailureNamesTheTableRowAndColumn()
        {
            const string frozen = "Config/FightMatch/ChapterOneTrial";
            var directory = Path.Combine(Path.GetTempPath(), "FightMatch-ChapterContent-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            try
            {
                foreach (var file in Directory.GetFiles(frozen, "*.csv")) File.Copy(file, Path.Combine(directory, Path.GetFileName(file)));
                var path = Path.Combine(directory, "PairEnemy.csv"); var lines = File.ReadAllLines(path); var cells = lines[1].Split(',');
                cells[4] = "0"; lines[1] = string.Join(",", cells); File.WriteAllLines(path, lines);
                var result = ChapterContentTables.ValidateAuthoringCsv(directory, PublishedContentTestData.Source(), ContentConsumerCapabilities.Current, Math());
                Assert.That(result.IsAccepted, Is.False); Assert.That(result.FieldPath, Is.EqualTo("PairEnemy[2].a_row"));
            }
            finally { Directory.Delete(directory, true); }
        }

        [Test]
        public void SchemaTwoProducesBoundMageMilestoneAndCardDefinitions()
        {
            var prepared = Prepare(); var definitions = prepared.Definitions;
            Assert.That(definitions.Growths.Count, Is.EqualTo(2)); Assert.That(definitions.FindGrowth("class:mage").BaseStats.AttackRange, Is.EqualTo(2));
            Assert.That(definitions.Chapter.Binding.Same(prepared.Binding), Is.True);
            var passive = definitions.Chapter.FindPassive("class:mage");
            Assert.That(passive.Kind, Is.EqualTo(ChapterPassiveKind.ArcanePenetration));
            Assert.That(passive.IgnoreMagicDefenseFraction.Compare(Q(1, 2), Math()), Is.Zero);
            Assert.That(definitions.Chapter.CharacterUnlocks.Single().AfterLevelId, Is.EqualTo("fixture:alpha"));
            var card = definitions.GetPermanentDefinitions().Find(CandidatePermanentDefinitionKind.Card, "item:experience-card-50");
            Assert.That(card.UnitExperience.ToString(), Is.EqualTo("50")); Assert.That(card.ItemId, Is.Null);
            Assert.That(prepared.Replays, Is.Empty, "Definition validation does not pretend to have replayed a chapter victory.");
        }

        [Test]
        public void SchemaTwoCatalogRoundTripUsesTheExactChapterClosure()
        {
            var source = Source(); var job = new DemoContentDraft(source.DraftId).BeginJob(); var prepared = Prepare(source, job);
            var store = new PublishedContentTestData.MemoryStorage(); var catalog = new PublishedContentCatalog(store, ContentConsumerCapabilities.Current);
            var published = catalog.Publish(prepared, prepared.Validation, PublishedContentTestData.Review(prepared), job, "fixture:chapter-publish", PublishedContentTestData.StoreBudget());
            Assert.That(published.IsAccepted, Is.True, published.RejectionCode + " " + published.FieldPath);
            var result = catalog.ResolveExact(prepared.Binding, ContentConsumerCapabilities.Current);
            Assert.That(result.IsAccepted, Is.True, result.RejectionCode + " " + result.FieldPath);
            Assert.That(result.Value.Definitions.Chapter.FindPassive("class:mage").Id, Is.EqualTo("passive:arcane-penetration"));
            Assert.That(result.Value.Parameters.Count, Is.EqualTo(31)); Assert.That(result.Value.GetDefaultReferences(), Is.Empty);
            var wrong = ContentBinding.Prepare(prepared.Binding.PackageId, new string('f', 64), prepared.Binding.RuleVersion,
                prepared.Binding.NumericContractVersion, prepared.Binding.RandomContractVersion, new SaveCodecBudget(Math()));
            Assert.That(catalog.ResolveExact(wrong.Value, ContentConsumerCapabilities.Current).IsAccepted, Is.False);
        }

        [TestCase("CH1.Milestones")]
        [TestCase("CH1.BusinessV5")]
        [TestCase("CH1.MageTauntBoss")]
        public void MissingConsumerCapabilityCannotCompileOrPartiallyActivate(string missing)
        {
            var source = Source(); var caps = new ContentConsumerCapabilities(ContentConsumerCapabilities.Current.Capabilities.Where(c => c != missing));
            var result = PublishedContentCompiler.Prepare(source, new DemoContentDraft(source.DraftId).BeginJob(), caps, Math());
            Assert.That(result.IsAccepted, Is.False); Assert.That(result.RejectionCode, Is.EqualTo("UnsupportedCapability"));
        }

        [TestCase("MissingPredecessor")]
        [TestCase("UnknownUnlockLevel")]
        [TestCase("UnknownItem")]
        [TestCase("WrongPassiveClass")]
        [TestCase("PenetrationFraction")]
        [TestCase("MissingProof")]
        [TestCase("InvalidRoute")]
        [TestCase("UnknownSkill")]
        public void NewDefinitionFailuresAreRejectedBeforePublication(string error)
        {
            var source = Source();
            switch (error)
            {
                case "MissingPredecessor": source.Progression.Levels[0].UnlockKind = CandidateProgressionUnlockKind.AfterWholeLevelClear;
                    source.Progression.Levels[0].UnlockAfterLevelId = "level:absent"; break;
                case "UnknownUnlockLevel": source.Chapter.CharacterUnlocks[0].AfterLevelId = "level:absent"; break;
                case "UnknownItem": source.Chapter.FirstClearItems[0].ItemId = "item:absent"; break;
                case "WrongPassiveClass": source.Chapter.Passives[1].ClassId = "class:warrior"; break;
                case "PenetrationFraction": source.Chapter.Passives[1].IgnoreMagicDefenseFraction = Q(50); break;
                case "MissingProof": source.Parameters.RemoveAt(0); break;
                case "InvalidRoute": source.Levels[0].SourceRoutes[0].Cells.RemoveAt(1); break;
                case "UnknownSkill": source.Chapter.Skills.Add(new ChapterSkillInput { SkillId = "unknown", ClassId = "class:warrior", RecordVersion = 1,
                    MinimumLevel = 5, CostCount = 1, CooldownTurns = 2, BattleCapabilityId = "unimplemented", CostItemId = "item:experience-card-50" }); break;
            }
            var result = PublishedContentCompiler.Prepare(source, new DemoContentDraft(source.DraftId).BeginJob(), ContentConsumerCapabilities.Current, Math());
            Assert.That(result.IsAccepted, Is.False, error);
        }
    }
}
