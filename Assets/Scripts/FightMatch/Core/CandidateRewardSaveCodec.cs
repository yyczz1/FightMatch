using System.Collections.Generic;
using static FightMatch.Core.BusinessFields;

namespace FightMatch.Core
{
    internal sealed class CandidateRewardSaveCodec
    {
        private readonly BusinessFields f;
        private readonly CandidateBattleSaveCodec battle;
        private readonly Dictionary<string, CandidateProgressionEndReceipt> endings;
        internal CandidateRewardSaveCodec(BusinessFields fields, CandidateBattleSaveCodec battle,
            Dictionary<string, CandidateProgressionEndReceipt> endings)
        { f = fields; this.battle = battle; this.endings = endings; }
        internal CandidateRewardState State(CandidateRewardState x, string p)
        {
            f.Required(x, p); var player = f.Text(x?.PlayerId, p + ".PlayerId"); var revision = f.Integer(x?.StateRevision ?? 0, p + ".StateRevision", 1);
            var rewards = f.List(x?.BaseRewards, Reward, p + ".BaseRewards"); return f.Reading ? new CandidateRewardState(player, revision, rewards) : x;
        }
        private CandidateFixedBaseReward Reward(CandidateFixedBaseReward x, string p)
        {
            f.Required(x, p); var report = battle.Reports.Ref(f, x?.Report, p + ".Report"); var definition = Definition(x?.Definition, p + ".Definition");
            var endId = f.Text(x?.Ending.EndReceiptId, p + ".Ending"); Need(endings.TryGetValue(endId, out var ending), p + ".Ending", "ReceiptConflict");
            if (!f.Reading) Need(ReferenceEquals(x.Ending, ending), p + ".Ending", "ReceiptConflict");
            var experience = f.List(x?.Experience, (v, vp) => Experience(v, report, vp), p + ".Experience");
            Random(x?.Random, report.Binding, p + ".Random");
            return f.Reading ? new CandidateFixedBaseReward(report, definition, ending, experience) : x;
        }
        private CandidateRewardDefinition Definition(CandidateRewardDefinition x, string p)
        {
            if (f.Resolved != null)
            {
                f.Required(x, p); var context = (PublishedRuleContext)f.Context(x?.Context, p + ".Context");
                var id = f.Text(x?.RewardDefinitionId, p + ".RewardDefinitionId");
                var version = f.Integer(f.Reading ? 0 : Take(ExactSaveValueCodec.DecodeInteger(x.Version, f.Budget), p + ".Version").Value, p + ".Version", 1);
                var level = f.LevelBinding(context.Binding, x?.LevelId, x?.LevelVersion, p + ".Level");
                var definitions = f.Resolved.FindExact(context.Binding);
                var definition = CandidatePermanentSaveCodec.Find(definitions.Rewards, r => r.RewardDefinitionId == id &&
                    r.Version == version.ToString(System.Globalization.CultureInfo.InvariantCulture), p);
                Need(definition.LevelId == level.LevelId && definition.LevelVersion == level.CanonicalLevelVersion, p + ".Level", "InconsistentBinding");
                f.SameDefinition(x, definition, (fields, v) => new CandidateRewardSaveCodec(fields, battle, endings).Definition(v, p), p);
                return f.Reading ? definition : x;
            }
            f.Required(x, p); var input = new CandidateRewardDefinitionInput { Context = f.Context(x?.Context, p + ".Context"),
                RewardDefinitionId = f.Text(x?.RewardDefinitionId, p + ".RewardDefinitionId"), Version = f.Text(x?.Version, p + ".Version"),
                LevelId = f.Text(x?.LevelId, p + ".LevelId"), LevelVersion = f.Text(x?.LevelVersion, p + ".LevelVersion"),
                BaseExperience = f.Integer(x?.BaseExperience ?? 0, p + ".BaseExperience"), DamageWeight = f.Rational(x?.DamageWeight, p + ".DamageWeight"),
                TakenWeight = f.Rational(x?.TakenWeight, p + ".TakenWeight"), CurveBase = f.Rational(x?.CurveBase, p + ".CurveBase"),
                CurveLog = f.Rational(x?.CurveLog, p + ".CurveLog"), ReferenceHpDivisor = f.Rational(x?.ReferenceHpDivisor, p + ".ReferenceHpDivisor"),
                LevelPenaltyBase = f.Rational(x?.LevelPenaltyBase, p + ".LevelPenaltyBase"), OverlevelGrace = f.Integer(x?.OverlevelGrace ?? 0, p + ".OverlevelGrace"),
                ZeroContributionPolicy = (CandidateZeroContributionPolicy)f.Enum((int)(x?.ZeroContributionPolicy ?? 0), 1, 1, p + ".ZeroContributionPolicy"),
                DropMode = (CandidateRewardDropMode)f.Enum((int)(x?.DropMode ?? 0), 1, 1, p + ".DropMode"), RequiredFeatures = f.Strings(x?.RequiredFeatures, p + ".RequiredFeatures") };
            var materials = f.List(x?.Materials, (v, vp) => { f.Required(v, vp); return new CandidateRewardMaterial(new CandidateRewardMaterialInput {
                ItemId = f.Text(v?.ItemId, vp + ".ItemId"), Amount = f.Integer(v?.Amount ?? 0, vp + ".Amount") }); }, p + ".Materials");
            input.Materials = new List<CandidateRewardMaterialInput>(); foreach (var v in materials) input.Materials.Add(new CandidateRewardMaterialInput { ItemId = v.ItemId, Amount = v.Amount });
            var result = CandidateBaseRewards.PrepareDefinition(input, f.Budget.Math); Need(result.IsAccepted, p + "." + result.FieldPath, result.RejectionCode.ToString());
            return f.Reading ? result.Definition : x;
        }
        private CandidateRewardExperience Experience(CandidateRewardExperience x, CandidateFinalAttemptReport report, string p)
        {
            f.Required(x, p); var key = f.Combatant(x?.CombatantKey, p + ".CombatantKey");
            var member = CandidateBattleSaveCodec.MemberDefinition(report.Baseline.Entry, key, p + ".CombatantKey");
            Need(f.Text(x?.CharacterId, p + ".CharacterId") == member.CharacterId, p + ".CharacterId", "ReceiptConflict");
            Need(f.I(x?.OriginalSlot ?? 0, p + ".OriginalSlot") == member.OriginalSlot, p + ".OriginalSlot", "ReceiptConflict");
            Need(f.Integer(x?.EntryLevel ?? 0, p + ".EntryLevel", 1) == member.Level, p + ".EntryLevel", "ReceiptConflict");
            var totals = CandidatePermanentSaveCodec.Find(report.FinalSnapshot.Contributions, t => t.CombatantKey.Equals(key), p + ".Totals");
            Equal(f.Rational(x?.Dealt, p + ".Dealt"), totals.EffectiveDamageDealtHp, p + ".Dealt");
            Equal(f.Rational(x?.Taken, p + ".Taken"), totals.EffectiveDamageTakenHp, p + ".Taken");
            var contribution = f.Rational(x?.Contribution, p + ".Contribution"); var reference = f.Rational(x?.Reference, p + ".Reference");
            var multiplier = f.Rational(x?.LevelMultiplier, p + ".LevelMultiplier"); f.Required(x?.Score, p + ".Score");
            var score = new ExactScoreResult(f.Integer(x?.Score.Amount ?? 0, p + ".Score.Amount"), f.Rational(x?.Score.LowerBound, p + ".Score.LowerBound"),
                f.Rational(x?.Score.UpperBound, p + ".Score.UpperBound"), f.I(x?.Score.TermsUsed ?? 0, p + ".Score.TermsUsed"));
            return f.Reading ? new CandidateRewardExperience(member, totals, contribution, reference, multiplier, score) : x;
        }
        private void Random(CandidateRewardRandomEvidence x, CandidateRandomBinding binding, string p)
        {
            f.Required(x, p); Equal(f.Domain(x?.Domain, p + ".Domain"), binding.BaseReward, p + ".Domain");
            Need(f.Text(x?.SourceCapabilityId, p + ".SourceCapabilityId") == binding.SourceCapabilityId, p + ".SourceCapabilityId", "InconsistentBinding");
            Need(f.Text(x?.MappingId, p + ".MappingId") == binding.MappingId, p + ".MappingId", "InconsistentBinding");
            f.Enum((int)(x?.Use ?? 0), 0, 0, p + ".Use");
            Equal(f.RandomStream(x?.Before, p + ".Before"), binding.BaseReward.Initial, p + ".Before");
            Equal(f.RandomStream(x?.After, p + ".After"), binding.BaseReward.Initial, p + ".After");
            var words = f.List(x?.Words, (v, vp) => (uint)f.U(v, 4, vp), p + ".Words", 4);
            Need(words.Count == 0 && f.Integer(x?.WordsConsumed ?? 0, p + ".WordsConsumed").IsZero, p + ".Words", "InconsistentBinding");
        }
        private void Equal(object x, object y, string p)
        { Need(CandidateBattleReportFingerprint.Equal(x, y, f.Budget.Math), p, "ReceiptConflict"); }
    }
}
