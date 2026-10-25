using System.Collections.Generic;
using System.Numerics;
using FightMatch.Core;

namespace FightMatch.Content
{
    public enum ChapterParameterStatus { Trial = 1, Approved = 2 }
    public sealed class ChapterContentInput
    {
        public string ReleaseSetId { get; set; }
        public ChapterParameterStatus ParameterStatus { get; set; }
        public string SourceLocator { get; set; }
        public List<ChapterPassiveInput> Passives { get; set; }
        public List<ChapterCharacterUnlockInput> CharacterUnlocks { get; set; }
        public List<ChapterFirstClearItemInput> FirstClearItems { get; set; }
        public List<ChapterCardInput> Cards { get; set; }
        public List<ChapterSkillInput> Skills { get; set; }
        public List<ChapterTeachingInput> Teachings { get; set; }
    }
    public sealed class ChapterCardInput
    {
        public string ItemId { get; set; }
        public BigInteger RecordVersion { get; set; }
        public BigInteger UnitExperience { get; set; }
    }
    public sealed class ChapterSkillInput
    {
        public string SkillId { get; set; }
        public BigInteger RecordVersion { get; set; }
        public string ClassId { get; set; }
        public string CostItemId { get; set; }
        public string BattleCapabilityId { get; set; }
        public BigInteger MinimumLevel { get; set; }
        public BigInteger CostCount { get; set; }
        public BigInteger CooldownTurns { get; set; }
    }
    public sealed class ChapterTeachingInput
    {
        public string TutorialId { get; set; }
        public BigInteger RecordVersion { get; set; }
        public string LevelId { get; set; }
        public BigInteger LevelVersion { get; set; }
        public string ClassId { get; set; }
        public string SkillId { get; set; }
        public string GiftItemId { get; set; }
        public string LearningStepId { get; set; }
        public string ExplanationStepId { get; set; }
        public string GiftOnceKey { get; set; }
        public BigInteger GiftCount { get; set; }
        public string ExplanationTextKey { get; set; }
    }
}
