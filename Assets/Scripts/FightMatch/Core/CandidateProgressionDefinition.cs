using System;
using System.Collections.Generic;
using System.Numerics;
using static FightMatch.Core.CandidateProgressionRejectionCode;

namespace FightMatch.Core
{
    public enum CandidateProgressionRejectionCode
    { None, MissingField, InvalidValue, UnsupportedBinding, InconsistentBinding, StaleContext, Locked, NoReadyMember, ActiveAttemptConflict, ChallengeConflict, AttemptAlreadyClosed }
    public enum CandidateProgressionOutcome { Changed, AlreadyIncluded }
    public enum CandidateProgressionEntryKind { Unspecified, Ordinary }
    public enum CandidateProgressionUnlockKind { Unspecified, InitiallyOpen, AfterWholeLevelClear }
    public sealed class CandidateProgressionDefinitionInput
    {
        public RuleContext Context { get; set; }
        public List<CandidateProgressionLevelInput> Levels { get; set; }
    }
    public sealed class CandidateProgressionLevelInput
    {
        private string prerequisite;
        public string LevelId { get; set; }
        public string LevelVersion { get; set; }
        public string UnlockRuleId { get; set; }
        public CandidateProgressionEntryKind EntryKind { get; set; }
        public CandidateProgressionUnlockKind UnlockKind { get; set; }
        public string UnlockAfterLevelId { get => prerequisite; set { prerequisite = value; HasPrerequisite = true; } }
        public List<string> RequiredFeatures { get; set; }
        internal bool HasPrerequisite { get; private set; }
    }
    public sealed class CandidateProgressionLevel
    {
        public string LevelId { get; }
        public string LevelVersion { get; }
        public string UnlockRuleId { get; }
        public CandidateProgressionEntryKind EntryKind { get; }
        public CandidateProgressionUnlockKind UnlockKind { get; }
        public string UnlockAfterLevelId { get; }
        public IReadOnlyList<string> RequiredFeatures { get; }
        internal CandidateProgressionLevel(CandidateProgressionLevelInput input)
        {
            LevelId = input.LevelId; LevelVersion = input.LevelVersion; UnlockRuleId = input.UnlockRuleId;
            EntryKind = input.EntryKind; UnlockKind = input.UnlockKind; UnlockAfterLevelId = input.UnlockAfterLevelId;
            RequiredFeatures = new List<string>(input.RequiredFeatures).AsReadOnly();
        }
    }
    public sealed class CandidateProgressionDefinition
    {
        public PreparedRuleContext Context { get; }
        public IReadOnlyList<CandidateProgressionLevel> Levels { get; }
        internal CandidateProgressionDefinition(RuleContext context, IEnumerable<CandidateProgressionLevel> levels)
        { Context = RuleContextChecks.Freeze(context); Levels = new List<CandidateProgressionLevel>(levels).AsReadOnly(); }
    }
    public sealed class CandidateProgressionDefinitionResult
    {
        public bool IsAccepted => Definition != null;
        public CandidateProgressionDefinition Definition { get; }
        public CandidateProgressionRejectionCode RejectionCode { get; }
        public string FieldPath { get; }
        internal CandidateProgressionDefinitionResult(CandidateProgressionDefinition definition) { Definition = definition; }
        internal CandidateProgressionDefinitionResult(ProgressionChecks check) { RejectionCode = check.Code; FieldPath = check.Path; }
    }

    internal sealed class ProgressionChecks
    {
        internal readonly ExactMathBudget Math;
        internal CandidateProgressionRejectionCode Code { get; private set; }
        internal string Path { get; private set; }
        internal ProgressionChecks(ExactMathBudget budget) { Math = budget; }
        internal bool Fail(CandidateProgressionRejectionCode code, string path) { Code = code; Path = path; return false; }
        internal bool Text(string value, string path) { return !string.IsNullOrWhiteSpace(value) || Fail(MissingField, path); }
        internal static bool Equal(string a, string b) { return string.Equals(a, b, StringComparison.Ordinal); }
        internal bool Same(string a, string b, string path) { return Equal(a, b) || Fail(InconsistentBinding, path); }
        internal bool Number(BigInteger? value, string path, bool positive = false)
        {
            if (!value.HasValue) return Fail(MissingField, path);
            Math.CheckInteger(value.Value);
            return Math.Compare(value.Value, positive ? BigInteger.One : BigInteger.Zero) >= 0 || Fail(InvalidValue, path);
        }
        internal bool Context(RuleContext value)
        {
            var rejection = RuleContextChecks.Validate(value, Math, "Context");
            return rejection == null || Fail((CandidateProgressionRejectionCode)Enum.Parse(typeof(CandidateProgressionRejectionCode), rejection.RejectionCode.ToString()), rejection.FieldPath);
        }
        internal bool ContextFields(PreparedRuleContext a, RuleContext b, string path)
        {
            var difference = RuleContextChecks.Difference(RuleContextChecks.Copy(a), b, path, Math);
            return difference == null || Fail(InconsistentBinding, difference);
        }
        internal bool ContextFields(PreparedRuleContext a, PreparedRuleContext b, string path)
        { return ContextFields(a, RuleContextChecks.Copy(b), path); }
        internal bool Binding(CandidateProgressionState state, string player, string id, string version,
            RuleContext context, out CandidateProgressionLevel level)
        {
            level = null;
            if (!Text(player, "PlayerId") || !Same(state.PlayerId, player, "PlayerId") || !Text(id, "LevelId") ||
                !Text(version, "LevelVersion") || !Context(context)) return false;
            if (!ContextFields(state.Definition.Context, context, "Context")) return false;
            level = FindLevel(state.Definition.Levels, id);
            return level == null ? Fail(UnsupportedBinding, "LevelId") : Same(level.LevelVersion, version, "LevelVersion");
        }
        internal bool Request(CandidateProgressionState state, CandidateProgressionLevelRequest request, out CandidateProgressionLevel level)
        {
            if (!Binding(state, request.PlayerId, request.LevelId, request.LevelVersion, request.Context, out level) ||
                !Text(request.CharacterId, "CharacterId") || !Number(request.ExpectedCharacterRevision, "ExpectedCharacterRevision") ||
                !Number(request.ExpectedOriginalSlot, "ExpectedOriginalSlot")) return false;
            return request.ExpectedOriginalSlot.Value <= 2 || Fail(InvalidValue, "ExpectedOriginalSlot");
        }
        internal bool Revision(BigInteger actual, BigInteger expected, string path = "ExpectedRevision")
        { Math.CheckInteger(expected); return Math.Compare(actual, expected) == 0 || Fail(StaleContext, path); }
        internal void CheckState(CandidateProgressionState state)
        {
            RuleContextChecks.CheckBudget(state.Definition.Context, Math); Math.CheckInteger(state.StateRevision);
            foreach (var challenge in state.Challenges) foreach (var attempt in challenge.Attempts)
            {
                RuleContextChecks.CheckBudget(attempt.Begin.Context, Math);
                foreach (var participant in attempt.Begin.Participants)
                { Math.CheckInteger(participant.CharacterRevision); Math.CheckInteger(participant.OriginalSlot); }
                if (attempt.Begin.FormationRevision.HasValue) Math.CheckInteger(attempt.Begin.FormationRevision.Value);
            }
        }
        internal static CandidateProgressionLevel FindLevel(IReadOnlyList<CandidateProgressionLevel> levels, string id)
        { foreach (var level in levels) if (Equal(level.LevelId, id)) return level; return null; }
    }
}
