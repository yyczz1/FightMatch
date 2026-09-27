using System;
using System.Collections.Generic;
using System.Numerics;

namespace FightMatch.Core
{
    public enum CandidateRewardRejectionCode
    { None, MissingField, InvalidValue, UnsupportedBinding, InconsistentBinding, StaleContext, IncompleteReport, RewardConflict }
    public enum CandidateRewardOutcome { None, Fixed, AlreadyIncluded, NotFound }
    public enum CandidateZeroContributionPolicy { Unspecified, CurveIntercept }
    public enum CandidateRewardDropMode { Unspecified, FixedOrdinaryMaterials }

    public sealed class CandidateRewardMaterialInput
    {
        public string ItemId { get; set; }
        public BigInteger? Amount { get; set; }
    }
    public sealed class CandidateRewardDefinitionInput
    {
        public RuleContext Context { get; set; }
        public string RewardDefinitionId { get; set; }
        public string Version { get; set; }
        public string LevelId { get; set; }
        public string LevelVersion { get; set; }
        public BigInteger? BaseExperience { get; set; }
        public ExactRational DamageWeight { get; set; }
        public ExactRational TakenWeight { get; set; }
        public ExactRational CurveBase { get; set; }
        public ExactRational CurveLog { get; set; }
        public ExactRational ReferenceHpDivisor { get; set; }
        public ExactRational LevelPenaltyBase { get; set; }
        public BigInteger? OverlevelGrace { get; set; }
        public CandidateZeroContributionPolicy ZeroContributionPolicy { get; set; }
        public CandidateRewardDropMode DropMode { get; set; }
        public List<string> RequiredFeatures { get; set; }
        public List<CandidateRewardMaterialInput> Materials { get; set; }
    }
    public sealed class CandidateRewardMaterial
    {
        public string ItemId { get; }
        public BigInteger Amount { get; }
        internal CandidateRewardMaterial(CandidateRewardMaterialInput input) { ItemId = input.ItemId; Amount = input.Amount.Value; }
    }
    public sealed class CandidateRewardDefinition
    {
        public PreparedRuleContext Context { get; }
        public string RewardDefinitionId { get; }
        public string Version { get; }
        public string LevelId { get; }
        public string LevelVersion { get; }
        public BigInteger BaseExperience { get; }
        public ExactRational DamageWeight { get; }
        public ExactRational TakenWeight { get; }
        public ExactRational CurveBase { get; }
        public ExactRational CurveLog { get; }
        public ExactRational ReferenceHpDivisor { get; }
        public ExactRational LevelPenaltyBase { get; }
        public BigInteger OverlevelGrace { get; }
        public CandidateZeroContributionPolicy ZeroContributionPolicy { get; }
        public CandidateRewardDropMode DropMode { get; }
        public IReadOnlyList<string> RequiredFeatures { get; }
        public IReadOnlyList<CandidateRewardMaterial> Materials { get; }
        internal CandidateRewardDefinition(CandidateRewardDefinitionInput input)
        {
            Context = RuleContextChecks.Freeze(input.Context); RewardDefinitionId = input.RewardDefinitionId;
            Version = input.Version; LevelId = input.LevelId; LevelVersion = input.LevelVersion;
            BaseExperience = input.BaseExperience.Value; DamageWeight = input.DamageWeight; TakenWeight = input.TakenWeight;
            CurveBase = input.CurveBase; CurveLog = input.CurveLog; ReferenceHpDivisor = input.ReferenceHpDivisor;
            LevelPenaltyBase = input.LevelPenaltyBase; OverlevelGrace = input.OverlevelGrace.Value;
            ZeroContributionPolicy = input.ZeroContributionPolicy; DropMode = input.DropMode;
            RequiredFeatures = new List<string>(input.RequiredFeatures).AsReadOnly();
            Materials = input.Materials.ConvertAll(row => new CandidateRewardMaterial(row)).AsReadOnly();
        }
    }
    public sealed class CandidateRewardDefinitionResult
    {
        public bool IsAccepted => Definition != null;
        public CandidateRewardDefinition Definition { get; }
        public CandidateRewardRejectionCode RejectionCode { get; }
        public string FieldPath { get; }
        internal CandidateRewardDefinitionResult(CandidateRewardDefinition definition) { Definition = definition; }
        internal CandidateRewardDefinitionResult(RewardChecks check) { RejectionCode = check.Code; FieldPath = check.Path; }
    }

    internal sealed class RewardChecks
    {
        internal readonly ExactMathBudget Math;
        internal CandidateRewardRejectionCode Code { get; private set; }
        internal string Path { get; private set; }
        internal RewardChecks(ExactMathBudget math) { Math = math; }
        internal bool Need(bool value, CandidateRewardRejectionCode code, string path)
        { if (value) return true; Code = code; Path = path; return false; }
        internal bool Text(string value, string path)
        { return Need(!string.IsNullOrWhiteSpace(value), CandidateRewardRejectionCode.MissingField, path); }
        internal bool Number(BigInteger? value, string path, bool positive = false)
        {
            if (!Need(value.HasValue, CandidateRewardRejectionCode.MissingField, path)) return false;
            Math.CheckInteger(value.Value);
            return Need(value.Value >= (positive ? BigInteger.One : BigInteger.Zero), CandidateRewardRejectionCode.InvalidValue, path);
        }
        internal bool Rational(ExactRational value, string path, bool positive = false)
        {
            if (!Need(value != null, CandidateRewardRejectionCode.MissingField, path)) return false;
            Math.CheckInteger(value.Numerator); Math.CheckInteger(value.Denominator);
            return Need(positive ? value.Numerator.Sign > 0 : value.Numerator.Sign >= 0, CandidateRewardRejectionCode.InvalidValue, path);
        }
        internal bool Context(RuleContext value)
        {
            var rejection = RuleContextChecks.Validate(value, Math, "Context");
            return rejection == null || Need(false, (CandidateRewardRejectionCode)Enum.Parse(typeof(CandidateRewardRejectionCode),
                rejection.RejectionCode.ToString()), rejection.FieldPath);
        }
        internal static bool Same(string a, string b) { return string.Equals(a, b, StringComparison.Ordinal); }
    }
}
