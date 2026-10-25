using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace FightMatch.Core
{
    public enum ChapterPassiveKind { WarriorCrit = 1, ArcanePenetration = 2 }

    public sealed class ArcanePenetrationInput
    {
        public string PassiveDefinitionId { get; set; }
        public ExactRational TargetProbability { get; set; }
        public ExactRational C { get; set; }
        public ExactRational IgnoreMagicDefenseFraction { get; set; }
    }

    public sealed class PreparedArcanePenetration
    {
        public string PassiveDefinitionId { get; }
        public ExactRational TargetProbability { get; }
        public ExactRational C { get; }
        public ExactRational IgnoreMagicDefenseFraction { get; }
        internal PreparedArcanePenetration(ArcanePenetrationInput input)
        { PassiveDefinitionId = input.PassiveDefinitionId; TargetProbability = input.TargetProbability;
            C = input.C; IgnoreMagicDefenseFraction = input.IgnoreMagicDefenseFraction; }
    }

    public sealed class ChapterPassiveInput
    {
        public string Id { get; set; }
        public string ClassId { get; set; }
        public ChapterPassiveKind Kind { get; set; }
        public ExactRational ProbabilityBase { get; set; }
        public ExactRational ProbabilityStep { get; set; }
        public ExactRational ProbabilityCap { get; set; }
        public ExactRational CritMultiplier { get; set; }
        public ExactRational IgnoreMagicDefenseFraction { get; set; }
    }

    public sealed class ChapterPassiveDefinition
    {
        public string Id { get; }
        public string ClassId { get; }
        public ChapterPassiveKind Kind { get; }
        public ExactRational ProbabilityBase { get; }
        public ExactRational ProbabilityStep { get; }
        public ExactRational ProbabilityCap { get; }
        public ExactRational CritMultiplier { get; }
        public ExactRational IgnoreMagicDefenseFraction { get; }
        internal ChapterPassiveDefinition(ChapterPassiveInput input)
        { Id = input.Id; ClassId = input.ClassId; Kind = input.Kind; ProbabilityBase = input.ProbabilityBase;
            ProbabilityStep = input.ProbabilityStep; ProbabilityCap = input.ProbabilityCap; CritMultiplier = input.CritMultiplier;
            IgnoreMagicDefenseFraction = input.IgnoreMagicDefenseFraction; }
        public ExactRational ProbabilityAt(BigInteger level, ExactMathBudget math)
        {
            if (level.Sign <= 0) throw new ArgumentOutOfRangeException(nameof(level));
            var value = ProbabilityBase.Add(ProbabilityStep.Multiply(ExactRational.Create(level - 1, 1, math), math), math);
            return value.Compare(ProbabilityCap, math) > 0 ? ProbabilityCap : value;
        }
    }

    public sealed class ChapterCharacterUnlockInput
    {
        public string ClassId { get; set; }
        public string AfterLevelId { get; set; }
        public string InitialLevelSource { get; set; }
        public BigInteger InitialExperience { get; set; }
        public List<string> InitialActiveSkills { get; set; }
    }
    public sealed class ChapterCharacterUnlock
    {
        public string ClassId { get; }
        public string AfterLevelId { get; }
        internal ChapterCharacterUnlock(ChapterCharacterUnlockInput input)
        { ClassId = input.ClassId; AfterLevelId = input.AfterLevelId; }
    }
    public sealed class ChapterFirstClearItemInput
    {
        public string LevelId { get; set; }
        public string ItemId { get; set; }
        public BigInteger Amount { get; set; }
    }
    public sealed class ChapterFirstClearItem
    {
        public string LevelId { get; }
        public string ItemId { get; }
        public BigInteger Amount { get; }
        internal ChapterFirstClearItem(ChapterFirstClearItemInput input)
        { LevelId = input.LevelId; ItemId = input.ItemId; Amount = input.Amount; }
    }

    public sealed class ChapterSkillDefinitionInput
    {
        public string SkillId { get; set; }
        public BigInteger RecordVersion { get; set; }
        public string ClassId { get; set; }
        public string CostItemId { get; set; }
        public string BattleCapabilityId { get; set; }
        public BigInteger MinimumLevel { get; set; }
        public BigInteger CostCount { get; set; }
        public int CooldownTurns { get; set; }
    }

    public sealed class ChapterSkillDefinition
    {
        public string SkillId { get; }
        public BigInteger RecordVersion { get; }
        public string ClassId { get; }
        public string CostItemId { get; }
        public string BattleCapabilityId { get; }
        public BigInteger MinimumLevel { get; }
        public BigInteger CostCount { get; }
        public int CooldownTurns { get; }

        internal ChapterSkillDefinition(ChapterSkillDefinitionInput input)
        {
            SkillId = input.SkillId;
            RecordVersion = input.RecordVersion;
            ClassId = input.ClassId;
            CostItemId = input.CostItemId;
            BattleCapabilityId = input.BattleCapabilityId;
            MinimumLevel = input.MinimumLevel;
            CostCount = input.CostCount;
            CooldownTurns = input.CooldownTurns;
        }
    }

    public sealed class ChapterTeachingDefinitionInput
    {
        public string TutorialId { get; set; }
        public BigInteger RecordVersion { get; set; }
        public DefinitionBinding Level { get; set; }
        public string ClassId { get; set; }
        public string SkillId { get; set; }
        public string GiftItemId { get; set; }
        public string LearningStepId { get; set; }
        public string ExplanationStepId { get; set; }
        public string GiftOnceKey { get; set; }
        public BigInteger GiftCount { get; set; }
        public string ExplanationTextKey { get; set; }
    }

    public sealed class ChapterTeachingDefinition
    {
        public string TutorialId { get; }
        public BigInteger RecordVersion { get; }
        public DefinitionBinding Level { get; }
        public string ClassId { get; }
        public string SkillId { get; }
        public string GiftItemId { get; }
        public string LearningStepId { get; }
        public string ExplanationStepId { get; }
        public string GiftOnceKey { get; }
        public BigInteger GiftCount { get; }
        public string ExplanationTextKey { get; }

        internal ChapterTeachingDefinition(ChapterTeachingDefinitionInput input)
        {
            TutorialId = input.TutorialId;
            RecordVersion = input.RecordVersion;
            Level = input.Level;
            ClassId = input.ClassId;
            SkillId = input.SkillId;
            GiftItemId = input.GiftItemId;
            LearningStepId = input.LearningStepId;
            ExplanationStepId = input.ExplanationStepId;
            GiftOnceKey = input.GiftOnceKey;
            GiftCount = input.GiftCount;
            ExplanationTextKey = input.ExplanationTextKey;
        }
    }

    // Immutable chapter rules share the exact publication binding with every definition in the closure.
    public sealed class ChapterRuleDefinitions
    {
        public const string RuleVersion = "ch1-slice-r1";
        public const string StateFeatureId = "CH1.TauntBossStateV1";
        public static IReadOnlyList<string> LegacyCapabilities { get; } = Array.AsReadOnly(new[] {
            "NC01.ExactRational", "RC01.PCG32.PRD", "PC01.ExactParameterEvidence", "SC01.sc01-pcg32-le128-v1",
            "CH1.MageTauntBoss", "CH1.Milestones", "CH1.BusinessV5" });
        public static IReadOnlyList<string> StateCapabilities { get; } = Array.AsReadOnly(LegacyCapabilities.Concat(new[] { StateFeatureId }).ToArray());
        public ContentBinding Binding { get; }
        public IReadOnlyList<string> RequiredCapabilities { get; }
        public bool HasTauntBossState => RequiredCapabilities.Contains(StateFeatureId);
        public IReadOnlyList<ChapterPassiveDefinition> Passives { get; }
        public IReadOnlyList<ChapterCharacterUnlock> CharacterUnlocks { get; }
        public IReadOnlyList<ChapterFirstClearItem> FirstClearItems { get; }
        public IReadOnlyList<ChapterSkillDefinition> Skills { get; }
        public IReadOnlyList<ChapterTeachingDefinition> Teachings { get; }
        private ChapterRuleDefinitions(ContentBinding binding, List<ChapterPassiveDefinition> passives,
            List<ChapterCharacterUnlock> unlocks, List<ChapterFirstClearItem> items,
            List<ChapterSkillDefinition> skills = null, List<ChapterTeachingDefinition> teachings = null)
        {
            Binding = binding;
            Passives = passives.AsReadOnly();
            CharacterUnlocks = unlocks.AsReadOnly();
            FirstClearItems = items.AsReadOnly();
            RequiredCapabilities = skills == null ? LegacyCapabilities : StateCapabilities;
            Skills = (skills ?? new List<ChapterSkillDefinition>()).AsReadOnly();
            Teachings = (teachings ?? new List<ChapterTeachingDefinition>()).AsReadOnly();
        }
        public ChapterPassiveDefinition FindPassive(string classId)
        { foreach (var item in Passives) if (item.ClassId == classId) return item; return null; }
        public ChapterSkillDefinition FindSkill(string skillId)
        { foreach (var item in Skills) if (item.SkillId == skillId) return item; return null; }
        public ChapterTeachingDefinition FindTeaching(string tutorialId)
        { foreach (var item in Teachings) if (item.TutorialId == tutorialId) return item; return null; }
        public ChapterTeachingDefinition FindTeaching(DefinitionBinding level)
        { foreach (var item in Teachings) if (item.Level.Same(level)) return item; return null; }

        public static SaveCodecResult<ChapterRuleDefinitions> Prepare(ContentBinding binding,
            IReadOnlyList<ChapterPassiveInput> passives, IReadOnlyList<ChapterCharacterUnlockInput> unlocks,
            IReadOnlyList<ChapterFirstClearItemInput> items, IReadOnlyList<ChapterSkillDefinitionInput> skills,
            IReadOnlyList<ChapterTeachingDefinitionInput> teachings, IReadOnlyList<string> requiredCapabilities,
            SaveCodecBudget budget)
        {
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            return SaveCodecResult<ChapterRuleDefinitions>.Run(() => {
                var basis = Prepare(binding, passives, unlocks, items, budget);
                SaveCodecFailure.Require(basis.IsAccepted, basis.RejectionCode, basis.FieldPath);
                SaveEnvelopeCodec.CheckList(requiredCapabilities, budget, "Chapter.RequiredCapabilities");
                SaveEnvelopeCodec.CheckList(skills, budget, "Chapter.Skills");
                SaveEnvelopeCodec.CheckList(teachings, budget, "Chapter.Teachings");
                var stateful = requiredCapabilities.Contains(StateFeatureId);
                var expected = stateful ? StateCapabilities : LegacyCapabilities;
                SaveCodecFailure.Require(requiredCapabilities.Count == expected.Count && expected.All(requiredCapabilities.Contains),
                    "UnsupportedCapability", "Chapter.RequiredCapabilities");
                if (!stateful)
                {
                    SaveCodecFailure.Require(skills.Count == 0 && teachings.Count == 0, "UnsupportedCapability", "Chapter.TeachingAndSkill");
                    return basis.Value;
                }
                SaveCodecFailure.Require(skills.Count == 1 && teachings.Count == 1, "UnsupportedBinding", "Chapter.TeachingAndSkill");
                var skill = skills[0];
                SaveCodecFailure.Require(skill != null, "MissingField", "Chapter.Skill");
                Text(skill.SkillId, budget); Text(skill.ClassId, budget); Text(skill.CostItemId, budget);
                budget.Math.CheckInteger(skill.RecordVersion);
                SaveCodecFailure.Require(skill.SkillId == "skill:warrior-taunt" && skill.RecordVersion.Sign > 0 &&
                    skill.MinimumLevel == 5 && skill.CostCount.IsOne && skill.CooldownTurns == 2 &&
                    skill.BattleCapabilityId == "CH1.MageTauntBoss", "UnsupportedBinding", "Chapter.Skill");
                var teaching = teachings[0];
                SaveCodecFailure.Require(teaching != null, "MissingField", "Chapter.Teaching");
                Text(teaching.TutorialId, budget); Text(teaching.LearningStepId, budget); Text(teaching.ExplanationStepId, budget);
                Text(teaching.ExplanationTextKey, budget);
                budget.Math.CheckInteger(teaching.RecordVersion);
                SaveCodecFailure.Require(teaching.RecordVersion.Sign > 0 && teaching.Level != null &&
                    binding.Same(teaching.Level.Content) && teaching.Level.LevelId == "level:ch01-16" &&
                    teaching.ClassId == skill.ClassId && teaching.SkillId == skill.SkillId && teaching.GiftItemId == skill.CostItemId &&
                    teaching.LearningStepId != teaching.ExplanationStepId && teaching.GiftOnceKey == "Level16EntryGift" &&
                    teaching.GiftCount.IsOne && teaching.ExplanationTextKey.StartsWith("fm.", StringComparison.Ordinal),
                    "InconsistentBinding", "Chapter.Teaching");
                return new ChapterRuleDefinitions(binding, basis.Value.Passives.ToList(), basis.Value.CharacterUnlocks.ToList(),
                    basis.Value.FirstClearItems.ToList(), new List<ChapterSkillDefinition> { new ChapterSkillDefinition(skill) },
                    new List<ChapterTeachingDefinition> { new ChapterTeachingDefinition(teaching) });
            });
        }

        public static SaveCodecResult<ChapterRuleDefinitions> Prepare(ContentBinding binding,
            IReadOnlyList<ChapterPassiveInput> passives, IReadOnlyList<ChapterCharacterUnlockInput> unlocks,
            IReadOnlyList<ChapterFirstClearItemInput> items, SaveCodecBudget budget)
        {
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            return SaveCodecResult<ChapterRuleDefinitions>.Run(() => {
                RuleContextChecks.BindingBudget(binding, budget);
                SaveCodecFailure.Require(binding.RuleVersion == RuleVersion, "UnsupportedBinding", "Chapter.RuleVersion");
                SaveEnvelopeCodec.CheckList(passives, budget, "Chapter.Passives");
                SaveEnvelopeCodec.CheckList(unlocks, budget, "Chapter.CharacterUnlocks");
                SaveEnvelopeCodec.CheckList(items, budget, "Chapter.FirstClearItems");
                var ps = new List<ChapterPassiveDefinition>(); var us = new List<ChapterCharacterUnlock>(); var fs = new List<ChapterFirstClearItem>();
                var ids = new HashSet<string>(StringComparer.Ordinal); var classes = new HashSet<string>(StringComparer.Ordinal);
                foreach (var p in passives)
                {
                    SaveCodecFailure.Require(p != null, "MissingField", "Chapter.Passive");
                    Text(p.Id, budget); Text(p.ClassId, budget);
                    SaveCodecFailure.Require(ids.Add(p.Id) && classes.Add(p.ClassId), "InconsistentBinding", "Chapter.Passive.Duplicate");
                    var check = new GrowthChecks(budget.Math);
                    SaveCodecFailure.Require(check.Number(p.ProbabilityBase, "ProbabilityBase", true, true) &&
                        check.Number(p.ProbabilityStep, "ProbabilityStep") && check.Number(p.ProbabilityCap, "ProbabilityCap", true, true),
                        "InvalidValue", "Chapter.Passive.Probability");
                    SaveCodecFailure.Require(p.ProbabilityBase.Compare(p.ProbabilityCap, budget.Math) <= 0, "InvalidValue", "Chapter.Passive.ProbabilityCap");
                    if (p.Kind == ChapterPassiveKind.WarriorCrit)
                        SaveCodecFailure.Require(p.IgnoreMagicDefenseFraction == null && check.Number(p.CritMultiplier, "CritMultiplier", true) &&
                            p.CritMultiplier.Compare(ExactRational.Create(1, 1, budget.Math), budget.Math) >= 0, "InvalidValue", "Chapter.Passive.CritMultiplier");
                    else
                        SaveCodecFailure.Require(p.Kind == ChapterPassiveKind.ArcanePenetration && p.CritMultiplier == null &&
                            check.Number(p.IgnoreMagicDefenseFraction, "IgnoreMagicDefenseFraction", false, true), "UnsupportedBinding", "Chapter.Passive.Kind");
                    ps.Add(new ChapterPassiveDefinition(p));
                }
                classes.Clear();
                foreach (var u in unlocks)
                {
                    SaveCodecFailure.Require(u != null, "MissingField", "Chapter.CharacterUnlock"); Text(u.ClassId, budget); Text(u.AfterLevelId, budget);
                    SaveCodecFailure.Require(classes.Add(u.ClassId), "InconsistentBinding", "Chapter.CharacterUnlock.ClassId");
                    SaveEnvelopeCodec.CheckList(u.InitialActiveSkills, budget, "Chapter.CharacterUnlock.InitialActiveSkills");
                    SaveCodecFailure.Require(u.InitialLevelSource == "ClearedLevelRecommended" && u.InitialExperience.IsZero &&
                        u.InitialActiveSkills.Count == 0, "UnsupportedBinding", "Chapter.CharacterUnlock.InitialState");
                    us.Add(new ChapterCharacterUnlock(u));
                }
                ids.Clear();
                foreach (var f in items)
                {
                    SaveCodecFailure.Require(f != null, "MissingField", "Chapter.FirstClearItem"); Text(f.LevelId, budget); Text(f.ItemId, budget);
                    budget.Math.CheckInteger(f.Amount);
                    SaveCodecFailure.Require(f.Amount.Sign > 0 && ids.Add(f.LevelId.Length + ":" + f.LevelId + f.ItemId), "InvalidValue", "Chapter.FirstClearItem");
                    fs.Add(new ChapterFirstClearItem(f));
                }
                ps.Sort((a, b) => StringComparer.Ordinal.Compare(a.ClassId, b.ClassId));
                us.Sort((a, b) => StringComparer.Ordinal.Compare(a.ClassId, b.ClassId));
                fs.Sort((a, b) => { var c = StringComparer.Ordinal.Compare(a.LevelId, b.LevelId); return c != 0 ? c : StringComparer.Ordinal.Compare(a.ItemId, b.ItemId); });
                return new ChapterRuleDefinitions(binding, ps, us, fs);
            });
        }
        private static void Text(string value, SaveCodecBudget budget)
        { RuleContextChecks.Text(value, "Chapter.Id", budget, true); SaveCodecFailure.Require(!value.Contains("|"), "InvalidValue", "Chapter.Id"); }
        internal void CheckClosure(PublishedRuleDefinitions definitions, SaveCodecBudget budget)
        {
            SaveCodecFailure.Require(Binding.Same(definitions.Binding) && Passives.Count == definitions.Growths.Count,
                "InconsistentBinding", "Chapter.Binding");
            foreach (var p in Passives)
            {
                var growth = definitions.FindGrowth(p.ClassId);
                SaveCodecFailure.Require(growth != null && growth.PassiveDefinitionId == p.Id &&
                    (growth.ClassKind == CharacterClassKind.Warrior && p.Kind == ChapterPassiveKind.WarriorCrit ||
                     growth.ClassKind == CharacterClassKind.Mage && p.Kind == ChapterPassiveKind.ArcanePenetration), "UnsupportedBinding", "Chapter.Passive.ClassId");
                var check = new GrowthChecks(budget.Math);
                SaveCodecFailure.Require(check.SameNumber(p.ProbabilityBase, p.Kind == ChapterPassiveKind.WarriorCrit ? growth.CritBase : growth.PenetrationBase) &&
                    check.SameNumber(p.ProbabilityStep, p.Kind == ChapterPassiveKind.WarriorCrit ? growth.CritStep : growth.PenetrationStep) &&
                    check.SameNumber(p.ProbabilityCap, p.Kind == ChapterPassiveKind.WarriorCrit ? growth.CritCap : growth.PenetrationCap) &&
                    check.SameNumber(p.CritMultiplier, growth.CritMultiplier) && check.SameNumber(p.IgnoreMagicDefenseFraction, growth.IgnoreFraction),
                    "InconsistentBinding", "Chapter.Passive.Growth");
            }
            foreach (var u in CharacterUnlocks)
                SaveCodecFailure.Require(definitions.FindGrowth(u.ClassId)?.ClassKind == CharacterClassKind.Mage &&
                    ProgressionChecks.FindLevel(definitions.Progression.Levels, u.AfterLevelId) != null, "UnsupportedBinding", "Chapter.CharacterUnlock.Reference");
            foreach (var item in FirstClearItems)
                SaveCodecFailure.Require(ProgressionChecks.FindLevel(definitions.Progression.Levels, item.LevelId) != null &&
                    InventoryChecks.FindItem(definitions.Inventory, item.ItemId)?.Kind == CandidateInventoryItemKind.OrdinaryMaterial,
                    "UnsupportedBinding", "Chapter.FirstClearItem.Reference");
            foreach (var skill in Skills)
                SaveCodecFailure.Require(definitions.FindGrowth(skill.ClassId)?.ClassKind == CharacterClassKind.Warrior &&
                    InventoryChecks.FindItem(definitions.Inventory, skill.CostItemId)?.Kind == CandidateInventoryItemKind.OrdinaryMaterial,
                    "UnsupportedBinding", "Chapter.Skill.Reference");
            foreach (var teaching in Teachings)
                SaveCodecFailure.Require(definitions.Levels.Any(level => level.LevelId == teaching.Level.LevelId &&
                    level.LevelVersion == teaching.Level.CanonicalLevelVersion), "InconsistentBinding", "Chapter.Teaching.Level");
        }
    }
}
