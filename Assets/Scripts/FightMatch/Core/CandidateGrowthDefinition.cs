using System;
using System.Numerics;
using static FightMatch.Core.CandidateGrowthRejectionCode;

namespace FightMatch.Core
{
    public enum CandidateGrowthRejectionCode
    {
        None, MissingField, InvalidValue, InconsistentBinding, UnsupportedBinding, StaleContext, NoReadyMember
    }

    // Mutable input shells must not be changed concurrently with any candidate call.
    public sealed class GrowthDefinitionInput
    {
        public RuleContext Context { get; set; }
        public string ClassId { get; set; }
        public CharacterClassKind ClassKind { get; set; }
        public string PassiveDefinitionId { get; set; }
        public StatsInput BaseStats { get; set; }
        public ExactRational GrowthHp { get; set; }
        public ExactRational GrowthAttack { get; set; }
        public ExactRational GrowthDefense { get; set; }
        public ExactRational CritBase { get; set; }
        public ExactRational CritStep { get; set; }
        public ExactRational CritCap { get; set; }
        public ExactRational CritMultiplier { get; set; }
        public BigInteger? XpBase { get; set; }
        public BigInteger? XpLinear { get; set; }
        public BigInteger? XpQuadratic { get; set; }
        public ExactRational RecoveryDurationMilliseconds { get; set; }
    }

    public sealed class CandidateGrowthDefinition
    {
        public PreparedRuleContext Context { get; }
        public string ClassId { get; }
        public CharacterClassKind ClassKind { get; }
        public string PassiveDefinitionId { get; }
        public PreparedStats BaseStats { get; }
        public ExactRational GrowthHp { get; }
        public ExactRational GrowthAttack { get; }
        public ExactRational GrowthDefense { get; }
        public ExactRational CritBase { get; }
        public ExactRational CritStep { get; }
        public ExactRational CritCap { get; }
        public ExactRational CritMultiplier { get; }
        public BigInteger XpBase { get; }
        public BigInteger XpLinear { get; }
        public BigInteger XpQuadratic { get; }
        public ExactRational RecoveryDurationMilliseconds { get; }

        internal CandidateGrowthDefinition(GrowthDefinitionInput input)
        {
            Context = RuleContextChecks.Freeze(input.Context);
            ClassId = input.ClassId; ClassKind = input.ClassKind;
            PassiveDefinitionId = input.PassiveDefinitionId;
            BaseStats = new PreparedStats(input.BaseStats);
            GrowthHp = input.GrowthHp; GrowthAttack = input.GrowthAttack; GrowthDefense = input.GrowthDefense;
            CritBase = input.CritBase; CritStep = input.CritStep; CritCap = input.CritCap;
            CritMultiplier = input.CritMultiplier;
            XpBase = input.XpBase.Value; XpLinear = input.XpLinear.Value; XpQuadratic = input.XpQuadratic.Value;
            RecoveryDurationMilliseconds = input.RecoveryDurationMilliseconds;
        }
    }

    public sealed class CandidateGrowthDefinitionResult
    {
        public bool IsAccepted => Definition != null;
        public CandidateGrowthDefinition Definition { get; }
        public CandidateGrowthRejectionCode RejectionCode { get; }
        public string FieldPath { get; }
        internal CandidateGrowthDefinitionResult(CandidateGrowthDefinition definition) { Definition = definition; }
        internal CandidateGrowthDefinitionResult(GrowthChecks check)
        { RejectionCode = check.Code; FieldPath = check.Path; }
    }

    internal sealed class GrowthChecks
    {
        internal readonly ExactMathBudget Budget;
        internal CandidateGrowthRejectionCode Code { get; private set; }
        internal string Path { get; private set; }
        internal GrowthChecks(ExactMathBudget budget) { Budget = budget; }
        internal bool Fail(CandidateGrowthRejectionCode code, string path)
        { Code = code; Path = path; return false; }
        internal bool Text(string value, string path)
        { return !string.IsNullOrWhiteSpace(value) || Fail(MissingField, path); }
        internal bool Same(string a, string b, string path)
        { return string.Equals(a, b, StringComparison.Ordinal) || Fail(InconsistentBinding, path); }
        internal bool Integer(BigInteger? value, string path, bool positive = false)
        {
            if (!value.HasValue) return Fail(MissingField, path);
            Budget.CheckInteger(value.Value);
            return Budget.Compare(value.Value, positive ? BigInteger.One : BigInteger.Zero) >= 0 || Fail(InvalidValue, path);
        }
        internal bool Number(ExactRational value, string path, bool positive = false, bool probability = false)
        {
            if (value == null) return Fail(MissingField, path);
            Check(value);
            var sign = Budget.Compare(value.Numerator, BigInteger.Zero);
            if (sign < 0 || (positive && sign == 0)) return Fail(InvalidValue, path);
            return !probability || Budget.Compare(value.Numerator, value.Denominator) <= 0 || Fail(InvalidValue, path);
        }
        internal void Check(ExactRational value)
        { Budget.CheckInteger(value.Numerator); Budget.CheckInteger(value.Denominator); }
        internal bool Context(RuleContext value, string path)
        {
            var rejection = RuleContextChecks.Validate(value, Budget, path);
            return rejection == null || Fail((CandidateGrowthRejectionCode)Enum.Parse(typeof(CandidateGrowthRejectionCode), rejection.RejectionCode.ToString()), rejection.FieldPath);
        }
        internal bool ContextMatches(PreparedRuleContext expected, RuleContext value, string path)
        {
            if (!Context(value, path)) return false;
            var difference = RuleContextChecks.Difference(RuleContextChecks.Copy(expected), value, path, Budget);
            return difference == null || Fail(InconsistentBinding, difference);
        }
        internal bool Target(CandidateCharacterState state, string player, string character, RuleContext context)
        {
            return Text(player, "PlayerId") && Text(character, "CharacterId") &&
                Same(state.PlayerId, player, "PlayerId") && Same(state.CharacterId, character, "CharacterId") &&
                ContextMatches(state.Definition.Context, context, "Context");
        }
        internal bool Revision(CandidateCharacterState state, BigInteger expected)
        {
            Budget.CheckInteger(expected);
            return Budget.Compare(state.StateRevision, expected) == 0 || Fail(StaleContext, "ExpectedRevision");
        }
        internal void CheckStats(PreparedStats stats)
        { Check(stats.MaxHp); Check(stats.Attack); Check(stats.PhysicalDefense); Check(stats.MagicDefense); Check(stats.Evasion); }
        internal void CheckDefinition(CandidateGrowthDefinition definition)
        {
            RuleContextChecks.CheckBudget(definition.Context, Budget); CheckStats(definition.BaseStats);
            Check(definition.GrowthHp); Check(definition.GrowthAttack); Check(definition.GrowthDefense);
            if (definition.ClassKind == CharacterClassKind.Warrior)
            {
                Check(definition.CritBase); Check(definition.CritStep); Check(definition.CritCap); Check(definition.CritMultiplier);
            }
            Budget.CheckInteger(definition.XpBase); Budget.CheckInteger(definition.XpLinear); Budget.CheckInteger(definition.XpQuadratic);
            Check(definition.RecoveryDurationMilliseconds);
        }
        internal void CheckTime(PreparedCandidateTimeSample time)
        {
            Budget.CheckInteger(time.WallUtcMilliseconds); Budget.CheckInteger(time.ObservedAtUtcMilliseconds);
            if (time.MonotonicElapsedMilliseconds != null) Check(time.MonotonicElapsedMilliseconds);
        }
        internal void CheckState(CandidateCharacterState state)
        {
            CheckDefinition(state.Definition);
            Budget.CheckInteger(state.Level); Budget.CheckInteger(state.Experience); Budget.CheckInteger(state.StateRevision);
            foreach (var receipt in state.BaseRewards)
            { Budget.CheckInteger(receipt.Amount); RuleContextChecks.CheckBudget(receipt.Context, Budget); }
            foreach (var end in state.ProcessedEnds)
            { RuleContextChecks.CheckBudget(end.Context, Budget); if (end.TimeSample != null) CheckTime(end.TimeSample); }
            foreach (var period in state.RecoveryPeriods)
            {
                CheckDefinition(period.Definition); Check(period.Duration); Check(period.Elapsed);
                CheckTime(period.StartSample); CheckTime(period.LastAcceptedSample);
            }
        }
        internal bool SameNumber(ExactRational left, ExactRational right)
        {
            return left == null || right == null ? left == right :
                Budget.Compare(left.Numerator, right.Numerator) == 0 && Budget.Compare(left.Denominator, right.Denominator) == 0;
        }
    }
}
