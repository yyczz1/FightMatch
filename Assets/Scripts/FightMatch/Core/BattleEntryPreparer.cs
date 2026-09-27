using System;
using System.Collections.Generic;
using System.Numerics;
using FlowPuzzle.Core;
using static FightMatch.Core.BattleEntryRejectionCode;

namespace FightMatch.Core
{
    public sealed class BattleEntryPreparer
    {
        public BattleEntryPreparationResult PrepareCandidate(BattleEntryInput input, ExactMathBudget budget)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            if (input.Context != null && !(input.Context is CandidateContext))
                return new BattleEntryPreparationResult(UnsupportedBinding, "Context");
            return Prepare(input, null, null, budget);
        }

        public BattleEntryPreparationResult PreparePublished(BattleEntryInput input, DefinitionBinding binding, PreparedLevel resolvedLevel, ExactMathBudget budget)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            if (input.Context == null || binding == null || resolvedLevel == null)
                return new BattleEntryPreparationResult(MissingField, input.Context == null ? "Context" : binding == null ? "Binding" : "ResolvedLevel");
            if (!(input.Context is PublishedRuleContext context)) return new BattleEntryPreparationResult(UnsupportedBinding, "Context");
            if (context.Binding == null) return new BattleEntryPreparationResult(MissingField, "Context.Binding");
            binding.CheckBudget(budget);
            if (!binding.Content.Same(context.Binding)) return new BattleEntryPreparationResult(InconsistentBinding, "Binding.Content");
            if (binding.LevelId != resolvedLevel.LevelId || binding.CanonicalLevelVersion != resolvedLevel.LevelVersion)
                return new BattleEntryPreparationResult(InconsistentBinding, "ResolvedLevel.Binding");
            return Prepare(input, binding, resolvedLevel, budget);
        }

        private static BattleEntryPreparationResult Prepare(BattleEntryInput input, DefinitionBinding binding, PreparedLevel resolvedLevel, ExactMathBudget budget)
        {
            var validation = new Validation(budget);
            if (!validation.Check(input)) return validation.Rejection;
            var prepared = new PreparedBattleEntry(input, binding);
            if (binding != null && (binding.LevelId != prepared.Level.LevelId || binding.CanonicalLevelVersion != prepared.Level.LevelVersion ||
                !CandidateBattleReportFingerprint.Equal(prepared.Level, resolvedLevel, budget)))
                return new BattleEntryPreparationResult(InconsistentBinding, "Level");
            return new BattleEntryPreparationResult(prepared);
        }

        private sealed class Validation
        {
            private readonly ExactMathBudget budget;
            internal BattleEntryPreparationResult Rejection { get; private set; }

            internal Validation(ExactMathBudget budget) { this.budget = budget; }

            internal bool Check(BattleEntryInput input)
            {
                if (!Text(input.PlayerId, "PlayerId") || !Text(input.ChallengeId, "ChallengeId") ||
                    !Text(input.AttemptId, "AttemptId") || !Text(input.EntryBaselineId, "EntryBaselineId"))
                    return false;
                if (input.CarryMode == EntryCarryMode.Unspecified) return Fail(MissingField, "CarryMode");
                if (input.CarryMode != EntryCarryMode.Empty) return Fail(UnsupportedBinding, "CarryMode");
                if (!Empty(input.RequiredFeatures, "RequiredFeatures") || !Context(input.Context, "Context"))
                    return false;
                if (!Level(input.Level)) return false;
                if (input.Members == null) return Fail(MissingField, "Members");
                var characters = new HashSet<string>(StringComparer.Ordinal);
                var classes = new HashSet<string>(StringComparer.Ordinal);
                var slots = new HashSet<int>();
                var ready = false;
                for (var i = 0; i < input.Members.Count; i++)
                {
                    var member = input.Members[i];
                    var path = $"Members[{i}]";
                    if (member == null) return Fail(MissingField, path);
                    if (!Text(member.CharacterId, path + ".CharacterId") ||
                        !Unique(characters, member.CharacterId, path + ".CharacterId") ||
                        !Text(member.ClassId, path + ".ClassId") ||
                        !Unique(classes, member.ClassId, path + ".ClassId")) return false;
                    if (member.ClassKind == CharacterClassKind.Unspecified) return Fail(MissingField, path + ".ClassKind");
                    if (member.ClassKind != CharacterClassKind.Warrior) return Fail(UnsupportedBinding, path + ".ClassKind");
                    if (!member.HasSlot) return Fail(MissingField, path + ".OriginalSlot");
                    if (member.OriginalSlot < 0 || member.OriginalSlot > 2) return Fail(InvalidValue, path + ".OriginalSlot");
                    if (!Unique(slots, member.OriginalSlot, path + ".OriginalSlot") ||
                        !PositiveInteger(member.Level, path + ".Level")) return false;
                    if (!member.HasReadiness) return Fail(MissingField, path + ".IsReady");
                    if (member.StatsOrigin == BaseStatsOrigin.Unspecified) return Fail(MissingField, path + ".StatsOrigin");
                    if (member.StatsOrigin != BaseStatsOrigin.ComputedBaseStats) return Fail(UnsupportedBinding, path + ".StatsOrigin");
                    if (!Context(member.StatsContext, path + ".StatsContext") ||
                        !SameContext(input.Context, member.StatsContext, path + ".StatsContext") ||
                        !Stats(member.Stats, path + ".Stats") || !Number(member.EntryHp, path + ".EntryHp", true))
                        return false;
                    if (member.EntryHp.Compare(member.Stats.MaxHp, budget) > 0) return Fail(InvalidValue, path + ".EntryHp");
                    if (!Empty(member.LearnedSkills, path + ".LearnedSkills") || !Crit(member.Crit, path + ".Crit"))
                        return false;
                    ready |= member.IsReady;
                }
                if (!(input.Context is PublishedRuleContext) && input.Members.Count > 1) return Fail(UnsupportedBinding, "Members[1]");
                if (input.Context is PublishedRuleContext && (input.Members.Count > 3 || input.Members.Exists(x => !x.IsReady)))
                    return Fail(input.Members.Count > 3 ? UnsupportedBinding : NoReadyMember, "Members");
                return ready || Fail(NoReadyMember, "Members");
            }

            private bool Context(RuleContext context, string path)
            {
                var rejection = RuleContextChecks.Validate(context, budget, path);
                return rejection == null || Fail(rejection.RejectionCode.Value, rejection.FieldPath);
            }

            private bool SameContext(RuleContext root, RuleContext stats, string path)
            {
                var difference = RuleContextChecks.Difference(root, stats, path, budget);
                return difference == null || Fail(InconsistentBinding, difference);
            }

            private bool Level(LevelInput level)
            {
                if (level == null) return Fail(MissingField, "Level");
                if (!Text(level.LevelId, "Level.LevelId") || !Text(level.LevelVersion, "Level.LevelVersion") ||
                    !PositiveInteger(level.RecommendedLevel, "Level.RecommendedLevel")) return false;
                if (level.Faces == null) return Fail(MissingField, "Level.Faces");
                if (level.Faces.Count == 0) return Fail(InvalidValue, "Level.Faces");
                var faces = new HashSet<string>(StringComparer.Ordinal);
                for (var i = 0; i < level.Faces.Count; i++)
                {
                    var face = level.Faces[i];
                    var path = $"Level.Faces[{i}]";
                    if (face == null) return Fail(MissingField, path);
                    if (!Text(face.FaceId, path + ".FaceId") || !Unique(faces, face.FaceId, path + ".FaceId")) return false;
                    if (face.Width <= 0) return Fail(InvalidValue, path + ".Width");
                    if (face.Height <= 0) return Fail(InvalidValue, path + ".Height");
                    if (face.Pairs == null) return Fail(MissingField, path + ".Pairs");
                    if (face.Pairs.Count == 0) return Fail(InvalidValue, path + ".Pairs");
                    if (!Pairs(face, path + ".Pairs")) return false;
                }
                return true;
            }

            private bool Pairs(FaceInput face, string path)
            {
                var pairs = new HashSet<string>(StringComparer.Ordinal);
                var colors = new HashSet<int>();
                var endpoints = new HashSet<FlowPos>();
                var enemies = new HashSet<string>(StringComparer.Ordinal);
                var slots = new HashSet<int>();
                var orders = new HashSet<int>();
                for (var i = 0; i < face.Pairs.Count; i++)
                {
                    var pair = face.Pairs[i];
                    var p = path + $"[{i}]";
                    if (pair == null) return Fail(MissingField, p);
                    if (!Text(pair.PairId, p + ".PairId") || !Unique(pairs, pair.PairId, p + ".PairId")) return false;
                    if (!pair.HasColor) return Fail(MissingField, p + ".GeometryColorId");
                    if (!Unique(colors, pair.GeometryColorId, p + ".GeometryColorId")) return false;
                    if (!pair.HasEndpointA) return Fail(MissingField, p + ".EndpointA");
                    if (!Cell(pair.EndpointA, face, p + ".EndpointA") ||
                        !Unique(endpoints, pair.EndpointA, p + ".EndpointA")) return false;
                    if (!pair.HasEndpointB) return Fail(MissingField, p + ".EndpointB");
                    if (!Cell(pair.EndpointB, face, p + ".EndpointB")) return false;
                    if (pair.EndpointA == pair.EndpointB) return Fail(InvalidValue, p + ".EndpointB");
                    if (!Unique(endpoints, pair.EndpointB, p + ".EndpointB")) return false;
                    var enemy = pair.Enemy;
                    p += ".Enemy";
                    if (enemy == null) return Fail(MissingField, p);
                    if (!Text(enemy.EnemyInstanceKey, p + ".EnemyInstanceKey") ||
                        !Unique(enemies, enemy.EnemyInstanceKey, p + ".EnemyInstanceKey") ||
                        !Text(enemy.EnemyDefinitionId, p + ".EnemyDefinitionId")) return false;
                    if (!enemy.HasSlot) return Fail(MissingField, p + ".OriginalSlot");
                    if (enemy.OriginalSlot < 0) return Fail(InvalidValue, p + ".OriginalSlot");
                    if (!Unique(slots, enemy.OriginalSlot, p + ".OriginalSlot")) return false;
                    if (!enemy.HasOrder) return Fail(MissingField, p + ".StableOrder");
                    if (enemy.StableOrder < 0) return Fail(InvalidValue, p + ".StableOrder");
                    if (!Unique(orders, enemy.StableOrder, p + ".StableOrder")) return false;
                    if (enemy.Behavior == EnemyBehavior.Unspecified) return Fail(MissingField, p + ".Behavior");
                    if (enemy.Behavior != EnemyBehavior.NormalStrike && enemy.Behavior != EnemyBehavior.ChargeHeavy)
                        return Fail(UnsupportedBinding, p + ".Behavior");
                    if (!Stats(enemy.Stats, p + ".Stats") || !Intents(enemy, p + ".IntentCycle")) return false;
                }
                return true;
            }

            private bool Intents(EnemyInput enemy, string path)
            {
                if (enemy.IntentCycle == null) return Fail(MissingField, path);
                var expectedCount = enemy.Behavior == EnemyBehavior.NormalStrike ? 1 : 2;
                if (enemy.IntentCycle.Count != expectedCount) return Fail(InvalidValue, path);
                for (var i = 0; i < enemy.IntentCycle.Count; i++)
                {
                    var intent = enemy.IntentCycle[i];
                    var p = path + $"[{i}]";
                    if (intent == null) return Fail(MissingField, p);
                    if (intent.Kind == EnemyIntentKind.Unspecified) return Fail(MissingField, p + ".Kind");
                    if (intent.Kind != EnemyIntentKind.Charge && intent.Kind != EnemyIntentKind.Strike)
                        return Fail(UnsupportedBinding, p + ".Kind");
                    var expectedKind = enemy.Behavior == EnemyBehavior.ChargeHeavy && i == 0
                        ? EnemyIntentKind.Charge : EnemyIntentKind.Strike;
                    if (intent.Kind != expectedKind) return Fail(InvalidValue, p + ".Kind");
                    if (intent.Targeting == EnemyTargeting.Unspecified) return Fail(MissingField, p + ".Targeting");
                    if (intent.Targeting != EnemyTargeting.FirstLiving) return Fail(UnsupportedBinding, p + ".Targeting");
                    if (!intent.HasDamageKind) return Fail(MissingField, p + ".DamageKind");
                    if (intent.Kind == EnemyIntentKind.Charge)
                    {
                        if (intent.DamageKind != null) return Fail(InvalidValue, p + ".DamageKind");
                        if (!intent.HasDamageCoefficient) return Fail(MissingField, p + ".DamageCoefficient");
                        if (intent.DamageCoefficient != null)
                        {
                            budget.CheckInteger(intent.DamageCoefficient.Numerator);
                            budget.CheckInteger(intent.DamageCoefficient.Denominator);
                            return Fail(InvalidValue, p + ".DamageCoefficient");
                        }
                    }
                    else
                    {
                        if (intent.DamageKind == null || intent.DamageKind == EntryDamageKind.Unspecified)
                            return Fail(MissingField, p + ".DamageKind");
                        if (intent.DamageKind != EntryDamageKind.Physical) return Fail(UnsupportedBinding, p + ".DamageKind");
                        if (!Number(intent.DamageCoefficient, p + ".DamageCoefficient", true)) return false;
                    }
                }
                return true;
            }

            private bool Stats(StatsInput stats, string path)
            {
                if (stats == null) return Fail(MissingField, path);
                if (!Number(stats.MaxHp, path + ".MaxHp", true) || !Number(stats.Attack, path + ".Attack") ||
                    !Number(stats.PhysicalDefense, path + ".PhysicalDefense") ||
                    !Number(stats.MagicDefense, path + ".MagicDefense") ||
                    !Number(stats.Evasion, path + ".Evasion", false, true)) return false;
                if (!stats.Evasion.Numerator.IsZero) return Fail(UnsupportedBinding, path + ".Evasion");
                return stats.AttackRange > 0 || Fail(InvalidValue, path + ".AttackRange");
            }

            private bool Crit(WarriorCritInput crit, string path)
            {
                if (crit == null) return Fail(MissingField, path);
                return Text(crit.PassiveDefinitionId, path + ".PassiveDefinitionId") &&
                    Number(crit.TargetProbability, path + ".TargetProbability", true, true) &&
                    Number(crit.C, path + ".C", true, true) && Number(crit.Multiplier, path + ".Multiplier", true);
            }

            private bool Number(ExactRational value, string path, bool positive = false, bool probability = false)
            {
                if (value == null) return Fail(MissingField, path);
                budget.CheckInteger(value.Numerator);
                budget.CheckInteger(value.Denominator);
                var sign = budget.Compare(value.Numerator, BigInteger.Zero);
                if (sign < 0 || (positive && sign == 0)) return Fail(InvalidValue, path);
                return !probability || budget.Compare(value.Numerator, value.Denominator) <= 0 || Fail(InvalidValue, path);
            }

            private bool PositiveInteger(BigInteger value, string path)
            {
                budget.CheckInteger(value);
                return budget.Compare(value, BigInteger.Zero) > 0 || Fail(InvalidValue, path);
            }

            private bool Empty(List<string> values, string path)
            {
                if (values == null) return Fail(MissingField, path);
                return values.Count == 0 || (Text(values[0], path + "[0]") && Fail(UnsupportedBinding, path + "[0]"));
            }

            private bool Cell(FlowPos cell, FaceInput face, string path)
            {
                return (cell.x >= 0 && cell.y >= 0 && cell.x < face.Width && cell.y < face.Height) || Fail(InvalidValue, path);
            }

            private bool Text(string value, string path) { return !string.IsNullOrWhiteSpace(value) || Fail(MissingField, path); }
            private bool Same(string a, string b, string path) { return string.Equals(a, b, StringComparison.Ordinal) || Fail(InconsistentBinding, path); }
            private bool Unique<T>(HashSet<T> seen, T value, string path) { return seen.Add(value) || Fail(DuplicateIdentity, path); }
            private bool Fail(BattleEntryRejectionCode code, string path)
            {
                Rejection = new BattleEntryPreparationResult(code, path);
                return false;
            }
        }
    }
}
