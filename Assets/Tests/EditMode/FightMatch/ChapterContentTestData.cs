using System.Collections.Generic;
using System.Linq;
using FightMatch.Content;
using FlowPuzzle.Core;
using NUnit.Framework;

namespace FightMatch.Core.Tests
{
    // Isolated compiler fixtures, never a production chapter closure or a review of authoring tables.
    internal static class ChapterContentTestData
    {
        internal static ExactMathBudget Math() => new ExactMathBudget(maxPrimitiveSteps: 16000000);
        internal static ExactRational Q(int numerator, int denominator = 1) => ExactRational.Create(numerator, denominator, Math());
        internal static PublishedSource Source()
        {
            var source = PublishedContentTestData.Fixture(); var warrior = source.Growth;
            source.SchemaVersion = 2; source.PackageId = "fixture:chapter"; source.RuleVersion = ChapterRuleDefinitions.RuleVersion;
            source.RequiredCapabilities = ChapterRuleDefinitions.LegacyCapabilities.ToList();
            source.Growths = new List<GrowthDefinitionInput> { warrior, new GrowthDefinitionInput { ClassId = "class:mage", ClassKind = CharacterClassKind.Mage,
                PassiveDefinitionId = "passive:arcane-penetration", BaseStats = new StatsInput { MaxHp = Q(72), Attack = Q(22), PhysicalDefense = Q(4),
                    MagicDefense = Q(12), Evasion = Q(0), AttackRange = 2 }, GrowthHp = warrior.GrowthHp, GrowthAttack = warrior.GrowthAttack,
                GrowthDefense = warrior.GrowthDefense, PenetrationBase = warrior.CritBase, PenetrationStep = warrior.CritStep, PenetrationCap = warrior.CritCap,
                IgnoreFraction = Q(1, 2), XpBase = warrior.XpBase, XpLinear = warrior.XpLinear, XpQuadratic = warrior.XpQuadratic,
                RecoveryDurationMilliseconds = warrior.RecoveryDurationMilliseconds } };
            source.Growth = null;
            foreach (var level in source.Levels)
                foreach (var route in level.SourceRoutes)
                {
                    var height = level.Level.Faces.Single(f => f.FaceId == route.FaceId).Height;
                    route.Cells = route.Cells.Select(c => new FlowPos(c.x, height + 1 - c.y)).ToList();
                }
            source.Coordinates = DemoCoordinateCandidate.AssumedTopLeft;
            source.Inventory.Items.Add(new CandidateInventoryItemInput { ItemId = "item:experience-card-50", Kind = CandidateInventoryItemKind.OrdinaryMaterial });
            source.Chapter = new ChapterContentInput { ReleaseSetId = "fixture:chapter-release", ParameterStatus = ChapterParameterStatus.Trial,
                SourceLocator = "ChapterContentTestData", Passives = new List<ChapterPassiveInput> {
                    new ChapterPassiveInput { Id = warrior.PassiveDefinitionId, ClassId = warrior.ClassId, Kind = ChapterPassiveKind.WarriorCrit,
                        ProbabilityBase = warrior.CritBase, ProbabilityStep = warrior.CritStep, ProbabilityCap = warrior.CritCap, CritMultiplier = warrior.CritMultiplier },
                    new ChapterPassiveInput { Id = "passive:arcane-penetration", ClassId = "class:mage", Kind = ChapterPassiveKind.ArcanePenetration,
                        ProbabilityBase = warrior.CritBase, ProbabilityStep = warrior.CritStep, ProbabilityCap = warrior.CritCap, IgnoreMagicDefenseFraction = Q(1, 2) } },
                CharacterUnlocks = new List<ChapterCharacterUnlockInput> { new ChapterCharacterUnlockInput { ClassId = "class:mage", AfterLevelId = "fixture:alpha",
                    InitialLevelSource = "ClearedLevelRecommended", InitialExperience = 0, InitialActiveSkills = new List<string>() } },
                FirstClearItems = new List<ChapterFirstClearItemInput> { new ChapterFirstClearItemInput { LevelId = "fixture:alpha", ItemId = "item:experience-card-50", Amount = 1 } },
                Cards = new List<ChapterCardInput> { new ChapterCardInput { ItemId = "item:experience-card-50", RecordVersion = 1, UnitExperience = 50 } },
                Skills = new List<ChapterSkillInput>(), Teachings = new List<ChapterTeachingInput>() };
            return source;
        }
        internal static PreparedPublication Prepare(PublishedSource source = null, DemoContentJob job = null)
        {
            source = source ?? Source();
            var result = PublishedContentCompiler.Prepare(source, job ?? new DemoContentDraft(source.DraftId).BeginJob(), ContentConsumerCapabilities.Current, Math());
            Assert.That(result.IsAccepted, Is.True, result.RejectionCode + " " + result.FieldPath); return result.Prepared;
        }
    }
}
