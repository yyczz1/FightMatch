using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using FlowPuzzle.Core;

namespace FightMatch.Core
{
    public static class CandidateBattleReportFingerprint
    {
        public static string Compute(CandidateFinalAttemptReport report, ExactMathBudget budget)
        {
            if (report == null) throw new ArgumentNullException(nameof(report));
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            using (var hash = SHA256.Create())
                return Hex(hash.ComputeHash(Encode(report, budget)));
        }

        internal static string Hex(byte[] bytes)
        {
            var text = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes) text.Append(b.ToString("x2", CultureInfo.InvariantCulture));
            return text.ToString();
        }

        internal static byte[] Encode(object value, ExactMathBudget math, bool ignoreRevision = false)
        {
            using (var bytes = new MemoryStream())
            { new Fmbr01Writer(bytes, math, ignoreRevision).Value(value); return bytes.ToArray(); }
        }

        internal static void Check(object value, ExactMathBudget math)
        { new Fmbr01Writer(Stream.Null, math).Value(value); }

        // Compare complete explicit values, not their hashes. This does not authenticate their source.
        internal static bool Equal(object a, object b, ExactMathBudget math, bool ignoreRevision = false)
        {
            var left = Encode(a, math, ignoreRevision); var right = Encode(b, math, ignoreRevision);
            if (left.Length != right.Length) return false;
            for (var i = 0; i < left.Length; i++) if (left[i] != right[i]) return false;
            return true;
        }
    }

    // The hand-written FMBR01 schema, shared with numeric inspection and exact history comparison.
    // No reflection, object summaries, platform endianness, Unicode normalization or dictionary order.
    internal sealed class Fmbr01Writer
    {
        private readonly Stream output;
        private readonly ExactMathBudget math;
        private readonly bool ignoreRevision;
        internal Fmbr01Writer(Stream output, ExactMathBudget math, bool ignoreRevision = false)
        {
            this.output = output; this.math = math; this.ignoreRevision = ignoreRevision;
            foreach (var b in new byte[] { 70, 77, 66, 82, 48, 49, 10 }) output.WriteByte(b);
        }
        private void U32(uint value)
        { for (var i = 0; i < 4; i++) output.WriteByte((byte)(value >> (8 * i))); }
        private void Length(int count) { math.CheckInteger(count); U32((uint)count); }
        private void Integer(BigInteger value)
        {
            math.CheckInteger(value); output.WriteByte(2);
            var bytes = Encoding.ASCII.GetBytes(value.ToString(CultureInfo.InvariantCulture));
            Length(bytes.Length); output.Write(bytes, 0, bytes.Length);
        }
        internal void Record(params object[] fields)
        {
            output.WriteByte(6); Length(fields.Length / 2);
            for (var i = 0; i < fields.Length; i += 2) { Value((string)fields[i]); Value(fields[i + 1]); }
        }
        private byte[] Core(Pcg32CoreState state)
        {
            math.CheckInteger(state.State); math.CheckInteger(state.Increment);
            var bytes = new byte[16];
            for (var i = 0; i < 8; i++) { bytes[i] = (byte)(state.State >> (8 * i)); bytes[i + 8] = (byte)(state.Increment >> (8 * i)); }
            return bytes;
        }
        private byte[] Words(IReadOnlyList<uint> words)
        {
            var bytes = new byte[checked(words.Count * 4)];
            for (var i = 0; i < words.Count; i++)
            { math.CheckInteger(words[i]); for (var j = 0; j < 4; j++) bytes[4 * i + j] = (byte)(words[i] >> (8 * j)); }
            return bytes;
        }

        internal void Value(object value)
        {
            if (value == null) { output.WriteByte(0); return; }
            switch (value)
            {
                case bool v: output.WriteByte(1); output.WriteByte(v ? (byte)1 : (byte)0); return;
                case BigInteger v: Integer(v); return;
                case int v: Integer(v); return;
                case uint v: Integer(v); return;
                case ulong v: Integer(v); return;
                case ExactRational v: output.WriteByte(3); Integer(v.Numerator); Integer(v.Denominator); return;
                case string v:
                    output.WriteByte(4); Length(v.Length);
                    foreach (var unit in v) { output.WriteByte((byte)unit); output.WriteByte((byte)(unit >> 8)); }
                    return;
                case byte[] v: output.WriteByte(7); Length(v.Length); output.Write(v, 0, v.Length); return;
                case CandidateFinalAttemptReport v:
                    Record("Binding", v.Binding, "Baseline", v.Baseline, "InitialSnapshot", v.InitialSnapshot,
                        "Operations", v.Operations, "FinalSnapshot", v.FinalSnapshot, "Contributions", v.Contributions,
                        "Outcome", v.Outcome, "EndedAtUnixMilliseconds", v.EndedAtUnixMilliseconds,
                        "TerminalOperationId", v.TerminalOperationId, "ConsumptionCoverage", v.ConsumptionCoverage,
                        "WholeLevelInitialEnemyHp", v.WholeLevelInitialEnemyHp, "CommitEligible", v.CommitEligible); return;
                case CandidateRandomBinding v:
                    Record("SourceCapabilityId", v.SourceCapabilityId, "MappingId", v.MappingId, "Battle", v.Battle,
                        "BaseReward", v.BaseReward, "Bonus", v.Bonus); return;
                case CandidateRandomDomain v:
                    Record("Purpose", v.Purpose, "InitState", v.InitState, "InitSequence", v.InitSequence, "Initial", v.Initial); return;
                case BattleEntryBaseline v:
                    Record("Entry", v.Entry, "RandomInitials", v.RandomInitials, "PrdInitialStates", v.PrdInitialStates); return;
                case BattleRandomInitials v: Record("Battle", v.Battle, "BaseReward", v.BaseReward, "Bonus", v.Bonus); return;
                case PreparedBattleEntry v:
                    var keys = new List<BattleCombatantKey>();
                    foreach (var member in v.ReadyParticipants) keys.Add(BattleCombatantKey.ForParticipant(v.AttemptId, member.CharacterId));
                    if (v.Context is PreparedPublishedRuleContext)
                    {
                        Record("Format", "fm.published.entry.v1", "PlayerId", v.PlayerId, "ChallengeId", v.ChallengeId, "AttemptId", v.AttemptId,
                            "EntryBaselineId", v.EntryBaselineId, "Context", v.Context, "Level", v.Level, "Members", v.Members,
                            "ReadyParticipants", keys, "CarryMode", v.CarryMode, "RequiredFeatures", v.RequiredFeatures,
                            "DefinitionBinding", v.GetDefinitionBinding()); return;
                    }
                    Record("PlayerId", v.PlayerId, "ChallengeId", v.ChallengeId, "AttemptId", v.AttemptId,
                        "EntryBaselineId", v.EntryBaselineId, "Context", v.Context, "Level", v.Level, "Members", v.Members,
                        "ReadyParticipants", keys, "CarryMode", v.CarryMode, "RequiredFeatures", v.RequiredFeatures); return;
                case PreparedPublishedRuleContext v:
                    Record("Format", "fm.published.context.v1", "Binding", v.Binding); return;
                case ContentBinding v:
                    Record("PackageId", v.PackageId, "ContentFingerprint", v.ContentFingerprint, "RuleVersion", v.RuleVersion,
                        "NumericContractVersion", v.NumericContractVersion, "RandomContractVersion", v.RandomContractVersion); return;
                case DefinitionBinding v:
                    Record("Content", v.Content, "LevelId", v.LevelId, "LevelVersion", v.LevelVersion); return;
                case PreparedCandidateContext v:
                    Record("DraftId", v.DraftId, "DraftRevision", v.DraftRevision, "ContentFingerprint", v.ContentFingerprint,
                        "RuleVersion", v.RuleVersion, "NumericContractVersion", v.NumericContractVersion,
                        "RandomContractVersion", v.RandomContractVersion, "SourceNotes", v.SourceNotes); return;
                case PreparedLevel v:
                    Record("LevelId", v.LevelId, "LevelVersion", v.LevelVersion, "RecommendedLevel", v.RecommendedLevel, "Faces", v.Faces); return;
                case PreparedFace v: Record("FaceId", v.FaceId, "Width", v.Width, "Height", v.Height, "Pairs", v.Pairs); return;
                case PreparedPair v:
                    Record("PairId", v.PairId, "GeometryColorId", v.GeometryColorId, "EndpointA", v.EndpointA,
                        "EndpointB", v.EndpointB, "Enemy", v.Enemy); return;
                case PreparedEnemy v:
                    Record("EnemyInstanceKey", v.EnemyInstanceKey, "EnemyDefinitionId", v.EnemyDefinitionId,
                        "OriginalSlot", v.OriginalSlot, "StableOrder", v.StableOrder, "Behavior", v.Behavior,
                        "Stats", v.Stats, "IntentCycle", v.IntentCycle); return;
                case PreparedEnemyIntent v:
                    Record("Kind", v.Kind, "Targeting", v.Targeting, "DamageKind", v.DamageKind, "DamageCoefficient", v.DamageCoefficient); return;
                case PreparedStats v:
                    Record("MaxHp", v.MaxHp, "Attack", v.Attack, "PhysicalDefense", v.PhysicalDefense,
                        "MagicDefense", v.MagicDefense, "Evasion", v.Evasion, "AttackRange", v.AttackRange); return;
                case PreparedMember v:
                    Record("CharacterId", v.CharacterId, "ClassId", v.ClassId, "ClassKind", v.ClassKind,
                        "OriginalSlot", v.OriginalSlot, "Level", v.Level, "IsReady", v.IsReady, "StatsOrigin", v.StatsOrigin,
                        "StatsContext", v.StatsContext, "Stats", v.Stats, "EntryHp", v.EntryHp,
                        "LearnedSkills", v.LearnedSkills, "Crit", v.Crit); return;
                case PreparedWarriorCrit v:
                    Record("PassiveDefinitionId", v.PassiveDefinitionId, "TargetProbability", v.TargetProbability,
                        "C", v.C, "Multiplier", v.Multiplier); return;
                case BattleSnapshot v:
                    Record("EntryBaselineId", v.Baseline.Entry.EntryBaselineId, "SceneRevision", ignoreRevision ? BigInteger.Zero : v.SceneRevision,
                        "EffectiveActionsCompleted", v.EffectiveActionsCompleted, "EnemyPhasesCompleted", v.EnemyPhasesCompleted,
                        "CurrentFaceIndex", v.CurrentFaceIndex, "Phase", v.Phase, "CarryMode", v.CarryMode,
                        "Board", v.Board, "Members", v.Members, "Enemies", v.Enemies, "Random", v.Random, "Contributions", v.Contributions); return;
                case BattleBoardState v: Record("FaceId", v.Face.FaceId, "LockedRoutes", v.LockedRoutes, "PendingLinks", v.PendingLinks); return;
                case BattleLockedRoute v: Record("PairKey", v.PairKey, "Route", v.Route); return;
                case BattleMemberState v: Record("CombatantKey", v.CombatantKey, "OriginalSlot", v.OriginalSlot, "Hp", v.Hp); return;
                case BattleEnemyState v:
                    Record("CombatantKey", v.CombatantKey, "PairKey", v.PairKey, "OriginalSlot", v.OriginalSlot,
                        "StableOrder", v.StableOrder, "Hp", v.Hp, "IntentCursor", v.IntentCursor); return;
                case BattleContributionTotals v:
                    Record("CombatantKey", v.CombatantKey, "EffectiveDamageDealtHp", v.EffectiveDamageDealtHp,
                        "EffectiveDamageTakenHp", v.EffectiveDamageTakenHp); return;
                case BattleRandomSnapshot v: Record("Stream", v.Stream, "PrdStates", v.PrdStates); return;
                case BattlePrdState v: Record("CombatantKey", v.CombatantKey, "Crit", v.Crit, "FailureCount", v.FailureCount); return;
                case Pcg32StreamState v:
                    Record("InitialCore", Core(v.Initial), "CurrentCore", Core(v.Current), "WordsConsumed", v.WordsConsumed); return;
                case BattleCombatantKey v:
                    Record("AttemptId", v.AttemptId, "Kind", v.Kind, "CharacterId", v.CharacterId,
                        "FaceId", v.FaceId, "EnemyInstanceKey", v.EnemyInstanceKey); return;
                case BattlePairKey v: Record("AttemptId", v.AttemptId, "FaceId", v.FaceId, "PairId", v.PairId); return;
                case FlowPos v: Record("X", v.x, "Y", v.y); return;
                case CandidateBattleOperationRecord v:
                    Record("Kind", v.Kind, "OperationId", v.OperationId, "OccurredAtUnixMilliseconds", v.OccurredAtUnixMilliseconds,
                        "BeforeSnapshot", v.BeforeSnapshot, "AfterSnapshot", v.AfterSnapshot, "Request", v.Request,
                        "Conditions", v.Conditions, "DirectFacts", v.DirectAttack == null ? (object)Array.Empty<BattleDamageFact>() : v.DirectAttack.DamageFacts,
                        "EnemyFacts", v.EnemyPhase == null ? (object)Array.Empty<CandidateEnemyIntentFact>() : v.EnemyPhase.OrderedIntents,
                        "StageFacts", v.StageDecision.OrderedFacts, "ContributionSegments", v.ContributionSegments,
                        "ConsumptionCoverage", v.ConsumptionCoverage); return;
                case CandidateStageSource v:
                    Record("PlayerId", v.PlayerId, "AttemptId", v.AttemptId, "OperationId", v.OperationId,
                        "ExpectedSceneRevision", v.ExpectedSceneRevision, "Actor", v.Actor, "Pair", v.Pair, "Route", v.Route); return;
                case CandidateBattleConditionValues v:
                    Record("PreferenceRevision", v.PreferenceRevision, "ItemUseEnabled", v.ItemUseEnabled); return;
                case BattleDamageFact v:
                    Record("Baseline", v.Baseline.Entry.EntryBaselineId, "PlayerId", v.PlayerId, "AttemptId", v.AttemptId,
                        "FaceId", v.FaceId, "OperationId", v.OperationId, "SceneRevision", v.SceneRevision,
                        "ActionOrdinal", v.ActionOrdinal, "Actor", v.Actor, "Target", v.Target, "Pair", v.Pair,
                        "EffectIndex", v.EffectIndex, "DamageKind", v.DamageKind, "Attack", v.Attack,
                        "PhysicalDefense", v.PhysicalDefense, "Multiplier", v.Multiplier, "RawDamage", v.RawDamage,
                        "MitigatedDamage", v.MitigatedDamage, "RoundedDamage", v.RoundedDamage,
                        "BlockPrevented", v.BlockPrevented, "ShieldAbsorbed", v.ShieldAbsorbed, "HpBefore", v.HpBefore,
                        "HpAfter", v.HpAfter, "HpLoss", v.HpLoss, "Overflow", v.Overflow,
                        "DefeatedTarget", v.DefeatedTarget, "Crit", v.Crit); return;
                case CandidateCritFact v:
                    Record("Actor", v.Actor, "Target", v.Target, "OpportunityOrdinal", v.OpportunityOrdinal,
                        "Parameters", v.Parameters, "Probability", v.Probability, "FailureCountBefore", v.FailureCountBefore,
                        "FailureCountAfter", v.FailureCountAfter, "Triggered", v.Triggered, "StreamBefore", v.StreamBefore,
                        "StreamAfter", v.StreamAfter, "Words", Words(v.Words)); return;
                case CandidateEnemyIntentFact v:
                    Record("Baseline", v.Baseline.Entry.EntryBaselineId, "PlayerId", v.PlayerId, "AttemptId", v.AttemptId,
                        "OperationId", v.OperationId, "FaceId", v.FaceId, "SceneRevision", v.SceneRevision,
                        "ActionOrdinal", v.ActionOrdinal, "EnemyPhaseOrdinal", v.EnemyPhaseOrdinal, "SegmentIndex", v.SegmentIndex,
                        "EnemyKey", v.EnemyKey, "Pair", v.Pair, "StableOrder", v.StableOrder, "IntentKind", v.IntentKind,
                        "CursorBefore", v.CursorBefore, "CursorAfter", v.CursorAfter, "Damage", v.Damage); return;
                case CandidateEnemyDamageFact v:
                    Record("ActorEnemy", v.ActorEnemy, "TargetMember", v.TargetMember, "DamageKind", v.DamageKind,
                        "Attack", v.Attack, "DamageCoefficient", v.DamageCoefficient, "PhysicalDefense", v.PhysicalDefense,
                        "RawDamage", v.RawDamage, "MitigatedDamage", v.MitigatedDamage, "RoundedDamage", v.RoundedDamage,
                        "BlockPrevented", v.BlockPrevented, "ShieldAbsorbed", v.ShieldAbsorbed, "HpBefore", v.HpBefore,
                        "HpAfter", v.HpAfter, "HpLoss", v.HpLoss, "Overflow", v.Overflow, "DefeatedTarget", v.DefeatedTarget); return;
                case CandidateStageFact v:
                    Record("Kind", v.Kind, "OperationId", v.OperationId, "SceneRevision", v.SceneRevision, "FaceId", v.FaceId,
                        "Pair", v.Pair, "SegmentIndex", v.SegmentIndex, "NextFaceId", v.NextFaceId, "Phase", v.Phase); return;
                case CandidateContributionSegment v:
                    Record("OperationId", v.OperationId, "SceneRevision", v.SceneRevision, "FaceId", v.FaceId,
                        "RuleSegment", v.RuleSegment, "SegmentIndex", v.SegmentIndex, "Actor", v.Actor, "Target", v.Target,
                        "Beneficiary", v.Beneficiary, "Kind", v.Kind, "HpLoss", v.HpLoss, "FactIndex", v.FactIndex); return;
                // These three fragment shapes are only for internal exact comparisons, not additional report fields.
                case CandidateEnemyPhaseFrame v:
                    Record("EnemyPhaseOrdinal", v.EnemyPhaseOrdinal, "Members", v.Members, "Enemies", v.Enemies,
                        "Contributions", v.Contributions, "Random", v.Random, "OrderedIntents", v.OrderedIntents); return;
                case CandidateStageDecision v:
                    Record("Kind", v.Source.Kind, "Source", v.Source, "FinalHp", v.FinalHp, "Board", v.Board,
                        "NextFaceIndex", v.NextFaceIndex, "NextPhase", v.NextPhase, "NextFace", v.NextFace, "OrderedFacts", v.OrderedFacts); return;
                case CandidateFinalHpValues v: Record("MemberHp", v.MemberHp, "EnemyHp", v.EnemyHp); return;
                case CandidateHpValue v: Record("CombatantKey", v.CombatantKey, "Hp", v.Hp); return;
                case IEnumerable values:
                    var rows = new List<object>(); foreach (var row in values) rows.Add(row);
                    output.WriteByte(5); Length(rows.Count); foreach (var row in rows) Value(row); return;
            }
            Value(EnumName(value));
        }

        private static string EnumName(object value)
        {
            switch (value)
            {
                case EntryCarryMode.Unspecified: return "Unspecified";
                case EntryCarryMode.Empty: return "Empty";
                case EntryCarryMode.NonEmpty: return "NonEmpty";
                case CharacterClassKind.Unspecified: return "Unspecified";
                case CharacterClassKind.Warrior: return "Warrior";
                case BaseStatsOrigin.Unspecified: return "Unspecified";
                case BaseStatsOrigin.ComputedBaseStats: return "ComputedBaseStats";
                case EnemyBehavior.Unspecified: return "Unspecified";
                case EnemyBehavior.NormalStrike: return "NormalStrike";
                case EnemyBehavior.ChargeHeavy: return "ChargeHeavy";
                case EnemyIntentKind.Unspecified: return "Unspecified";
                case EnemyIntentKind.Charge: return "Charge";
                case EnemyIntentKind.Strike: return "Strike";
                case EnemyTargeting.Unspecified: return "Unspecified";
                case EnemyTargeting.FirstLiving: return "FirstLiving";
                case EntryDamageKind.Unspecified: return "Unspecified";
                case EntryDamageKind.Physical: return "Physical";
                case BattlePhase.AwaitAction: return "AwaitAction";
                case BattlePhase.AwaitLinks: return "AwaitLinks";
                case BattlePhase.AwaitRescue: return "AwaitRescue";
                case BattlePhase.WonPendingSettlement: return "WonPendingSettlement";
                case BattlePhase.Closed: return "Closed";
                case BattleCombatantKind.Participant: return "Participant";
                case BattleCombatantKind.Enemy: return "Enemy";
                case CandidateRandomPurpose.Battle: return "Battle";
                case CandidateRandomPurpose.BaseReward: return "BaseReward";
                case CandidateRandomPurpose.Bonus: return "Bonus";
                case CandidateBattleOperationKind.Attack: return "Attack";
                case CandidateBattleOperationKind.Link: return "Link";
                case CandidateBattleFactKind.DirectAttack: return "DirectAttack";
                case CandidateBattleFactKind.EnemyIntent: return "EnemyIntent";
                case CandidateBattleFactKind.Stage: return "Stage";
                case CandidateContributionKind.DamageDealtHp: return "DamageDealtHp";
                case CandidateContributionKind.DamageTakenHp: return "DamageTakenHp";
                case CandidateConsumptionCoverage.EmptyCarryNoUse: return "EmptyCarryNoUse";
                case CandidateBattleOutcome.NormalVictory: return "NormalVictory";
                case CandidateStageOperation.AfterAttack: return "AfterAttack";
                case CandidateStageOperation.CompleteLink: return "CompleteLink";
                case CandidateStageFactKind.TemporaryRouteRemoved: return "TemporaryRouteRemoved";
                case CandidateStageFactKind.RouteLocked: return "RouteLocked";
                case CandidateStageFactKind.PendingLinkAdded: return "PendingLinkAdded";
                case CandidateStageFactKind.PendingLinkRemoved: return "PendingLinkRemoved";
                case CandidateStageFactKind.FaceChanged: return "FaceChanged";
                case CandidateStageFactKind.PhaseSelected: return "PhaseSelected";
                default: throw new ArgumentException("Unknown value in the fixed FMBR01 schema.", nameof(value));
            }
        }
    }
}
