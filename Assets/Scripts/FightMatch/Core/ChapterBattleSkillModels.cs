using System;
using System.Collections.Generic;
using System.Numerics;

namespace FightMatch.Core
{
    public sealed class ChapterBattleSkillInput
    {
        public string SkillId { get; set; }
        public BigInteger RecordVersion { get; set; }
        public string BattleCapabilityId { get; set; }
        public int CooldownTurns { get; set; }
    }

    public sealed class PreparedChapterBattleSkill
    {
        public string SkillId { get; }
        public BigInteger RecordVersion { get; }
        public string BattleCapabilityId { get; }
        public int CooldownTurns { get; }

        internal PreparedChapterBattleSkill(ChapterBattleSkillInput input)
        {
            SkillId = input.SkillId;
            RecordVersion = input.RecordVersion;
            BattleCapabilityId = input.BattleCapabilityId;
            CooldownTurns = input.CooldownTurns;
        }
    }

    public sealed class ChapterMemberSkillState
    {
        public BattleCombatantKey CombatantKey { get; }
        public string SkillId { get; }
        public int CooldownRemaining { get; }

        internal ChapterMemberSkillState(BattleCombatantKey combatantKey, string skillId, int cooldownRemaining)
        {
            CombatantKey = combatantKey;
            SkillId = skillId;
            CooldownRemaining = cooldownRemaining;
        }
    }

    public sealed class ChapterTauntState
    {
        public BattleCombatantKey TargetEnemyKey { get; }
        public BattleCombatantKey CasterKey { get; }
        public BigInteger AppliesToEnemyPhaseOrdinal { get; }

        internal ChapterTauntState(BattleCombatantKey targetEnemyKey, BattleCombatantKey casterKey,
            BigInteger appliesToEnemyPhaseOrdinal)
        {
            TargetEnemyKey = targetEnemyKey;
            CasterKey = casterKey;
            AppliesToEnemyPhaseOrdinal = appliesToEnemyPhaseOrdinal;
        }
    }

    // One immutable value travels through direct, enemy and completed-action projections.
    // Its presence is selected only by the frozen entry capability, never by a latest table.
    public sealed class ChapterBattleState
    {
        public BigInteger FaceEnemyPhasesCompleted { get; }
        public IReadOnlyList<ChapterMemberSkillState> MemberSkills { get; }
        public IReadOnlyList<ChapterTauntState> Taunts { get; }
        public IReadOnlyList<ChapterVulnerabilityWindow> Vulnerabilities { get; }

        internal ChapterBattleState(BigInteger faceEnemyPhasesCompleted,
            IEnumerable<ChapterMemberSkillState> memberSkills, IEnumerable<ChapterTauntState> taunts,
            IEnumerable<ChapterVulnerabilityWindow> vulnerabilities)
        {
            FaceEnemyPhasesCompleted = faceEnemyPhasesCompleted;
            MemberSkills = new List<ChapterMemberSkillState>(memberSkills).AsReadOnly();
            Taunts = new List<ChapterTauntState>(taunts).AsReadOnly();
            Vulnerabilities = new List<ChapterVulnerabilityWindow>(vulnerabilities).AsReadOnly();
        }
    }

    public sealed class ChapterSkillCastFact
    {
        public BattleCombatantKey CasterKey { get; }
        public BattleCombatantKey TargetEnemyKey { get; }
        public PreparedChapterBattleSkill Skill { get; }
        public int CooldownBefore { get; }
        public int CooldownAfter { get; }
        public BigInteger EnemyPhaseOrdinal { get; }

        internal ChapterSkillCastFact(BattleCombatantKey casterKey, BattleCombatantKey targetEnemyKey,
            PreparedChapterBattleSkill skill, int cooldownBefore, int cooldownAfter, BigInteger enemyPhaseOrdinal)
        {
            CasterKey = casterKey;
            TargetEnemyKey = targetEnemyKey;
            Skill = skill;
            CooldownBefore = cooldownBefore;
            CooldownAfter = cooldownAfter;
            EnemyPhaseOrdinal = enemyPhaseOrdinal;
        }
    }

    public sealed class ChapterCooldownFact
    {
        public BattleCombatantKey CombatantKey { get; }
        public string SkillId { get; }
        public int Before { get; }
        public int After { get; }

        // Cast changes are recorded by ChapterSkillCastFact. These are whole-action tail decrements only.
        internal ChapterCooldownFact(BattleCombatantKey combatantKey, string skillId, int before, int after)
        {
            CombatantKey = combatantKey;
            SkillId = skillId;
            Before = before;
            After = after;
        }
    }
}
