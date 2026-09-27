using System;
using System.Collections.Generic;
using System.Numerics;

namespace FightMatch.Core
{
    public static class RuleContextChecks
    {
        public static SaveCodecResult<PreparedRuleContext> Prepare(RuleContext input, SaveCodecBudget budget)
        {
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            return SaveCodecResult<PreparedRuleContext>.Run(() =>
            {
                // Counts and string limits precede any copying of caller-owned lists.
                if (input is CandidateContext candidate && candidate.SourceNotes != null)
                    SaveEnvelopeCodec.CheckList(candidate.SourceNotes, budget, "Context.SourceNotes");
                var rejection = Validate(input, budget.Math, "Context");
                if (rejection != null) throw new SaveCodecFailure(rejection.RejectionCode.ToString(), rejection.FieldPath);
                CheckFields(input, budget);
                return Freeze(input);
            });
        }

        internal static PreparedRuleContext Freeze(RuleContext input)
        {
            if (input is CandidateContext candidate) return new PreparedCandidateContext(candidate);
            if (input is PublishedRuleContext published && published.Binding != null) return new PreparedPublishedRuleContext(published.Binding);
            throw new ArgumentException("A validated candidate or published context is required.", nameof(input));
        }

        public static RuleContext Copy(PreparedRuleContext input)
        {
            if (input is PreparedPublishedRuleContext published) return new PublishedRuleContext(published.Binding);
            if (input is PreparedCandidateContext c) return new CandidateContext { DraftId = c.DraftId, DraftRevision = c.DraftRevision,
                ContentFingerprint = c.ContentFingerprint, RuleVersion = c.RuleVersion, NumericContractVersion = c.NumericContractVersion,
                RandomContractVersion = c.RandomContractVersion, SourceNotes = new List<string>(c.SourceNotes) };
            throw new ArgumentException("A prepared candidate or published context is required.", nameof(input));
        }

        public static RuleContext Copy(RuleContext input)
        {
            if (input is PublishedRuleContext published) return new PublishedRuleContext(published.Binding);
            if (input is CandidateContext c) return new CandidateContext { DraftId = c.DraftId, DraftRevision = c.DraftRevision,
                ContentFingerprint = c.ContentFingerprint, RuleVersion = c.RuleVersion, NumericContractVersion = c.NumericContractVersion,
                RandomContractVersion = c.RandomContractVersion, SourceNotes = c.SourceNotes == null ? null : new List<string>(c.SourceNotes) };
            throw new ArgumentException("A candidate or published context is required.", nameof(input));
        }

        public static bool Same(PreparedRuleContext a, PreparedRuleContext b)
        { return Known(a) && Known(b) && Same(Copy(a), Copy(b)); }
        public static bool Same(PreparedRuleContext a, RuleContext b)
        { return Known(a) && Same(Copy(a), b); }
        public static bool Same(RuleContext a, RuleContext b)
        { return Difference(a, b, "Context") == null; }
        private static bool Known(PreparedRuleContext value)
        { return value is PreparedCandidateContext || value is PreparedPublishedRuleContext; }

        internal static string Difference(RuleContext a, RuleContext b, string path, ExactMathBudget budget = null)
        {
            if (a == null || b == null || a.GetType() != b.GetType()) return path;
            if (a is CandidateContext x && b is CandidateContext y)
            {
                if (x.DraftId != y.DraftId) return path + ".DraftId";
                if (budget == null ? x.DraftRevision != y.DraftRevision : budget.Compare(x.DraftRevision, y.DraftRevision) != 0) return path + ".DraftRevision";
                if (x.SourceNotes == null || y.SourceNotes == null || x.SourceNotes.Count != y.SourceNotes.Count) return path + ".SourceNotes";
                for (var i = 0; i < x.SourceNotes.Count; i++) if (x.SourceNotes[i] != y.SourceNotes[i]) return path + $".SourceNotes[{i}]";
            }
            else if (a is PublishedRuleContext p && b is PublishedRuleContext q)
            { if (p.Binding == null || q.Binding == null || p.Binding.PackageId != q.Binding.PackageId) return path + ".PackageId"; }
            else return path;
            if (a.ContentFingerprint != b.ContentFingerprint) return path + ".ContentFingerprint";
            if (a.RuleVersion != b.RuleVersion) return path + ".RuleVersion";
            if (a.NumericContractVersion != b.NumericContractVersion) return path + ".NumericContractVersion";
            return a.RandomContractVersion == b.RandomContractVersion ? null : path + ".RandomContractVersion";
        }

        internal static BattleEntryPreparationResult Validate(RuleContext input, ExactMathBudget budget, string path)
        {
            if (input == null) return Failure(BattleEntryRejectionCode.MissingField, path);
            if (input is PublishedRuleContext published)
                return published.Binding == null ? Failure(BattleEntryRejectionCode.MissingField, path + ".Binding") : null;
            if (!(input is CandidateContext c)) return Failure(BattleEntryRejectionCode.UnsupportedBinding, path);
            if (string.IsNullOrWhiteSpace(c.DraftId)) return Failure(BattleEntryRejectionCode.MissingField, path + ".DraftId");
            budget.CheckInteger(c.DraftRevision);
            if (budget.Compare(c.DraftRevision, BigInteger.One) < 0) return Failure(BattleEntryRejectionCode.InvalidValue, path + ".DraftRevision");
            var fields = new[] { c.ContentFingerprint, c.RuleVersion, c.NumericContractVersion, c.RandomContractVersion };
            var names = new[] { "ContentFingerprint", "RuleVersion", "NumericContractVersion", "RandomContractVersion" };
            for (var i = 0; i < fields.Length; i++)
                if (string.IsNullOrWhiteSpace(fields[i])) return Failure(BattleEntryRejectionCode.MissingField, path + "." + names[i]);
            if (c.SourceNotes == null) return Failure(BattleEntryRejectionCode.MissingField, path + ".SourceNotes");
            if (c.SourceNotes.Count == 0) return Failure(BattleEntryRejectionCode.InvalidValue, path + ".SourceNotes");
            for (var i = 0; i < c.SourceNotes.Count; i++)
                if (string.IsNullOrWhiteSpace(c.SourceNotes[i])) return Failure(BattleEntryRejectionCode.MissingField, path + $".SourceNotes[{i}]");
            return null;
        }
        private static BattleEntryPreparationResult Failure(BattleEntryRejectionCode code, string path)
        { return new BattleEntryPreparationResult(code, path); }

        public static void CheckBudget(PreparedRuleContext context, ExactMathBudget budget)
        {
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            if (context is PreparedCandidateContext candidate) { budget.CheckInteger(candidate.DraftRevision); return; }
            if (!(context is PreparedPublishedRuleContext)) throw new ArgumentException("Unknown prepared context.", nameof(context));
        }

        internal static void CheckBudget(PreparedBattleEntry entry, ExactMathBudget budget)
        {
            CheckBudget(entry.Context, budget);
            entry.GetDefinitionBinding()?.CheckBudget(budget);
        }

        public static SaveCodecResult<PreparedRuleContext> CheckBudget(PreparedRuleContext context, SaveCodecBudget budget)
        {
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            return SaveCodecResult<PreparedRuleContext>.Run(() =>
            {
                SaveCodecFailure.Require(Known(context), context == null ? "MissingField" : "UnsupportedBinding", "Context");
                if (context is PreparedCandidateContext candidate)
                    SaveEnvelopeCodec.CheckList(candidate.SourceNotes, budget, "Context.SourceNotes");
                CheckFields(Copy(context), budget);
                return context;
            });
        }

        private static void CheckFields(RuleContext context, SaveCodecBudget budget)
        {
            if (context is PublishedRuleContext published) { BindingBudget(published.Binding, budget); return; }
            var c = (CandidateContext)context;
            SaveEnvelopeCodec.CheckList(c.SourceNotes, budget, "Context.SourceNotes");
            Text(c.DraftId, "Context.DraftId", budget);
            BusinessFields.Take(ExactSaveValueCodec.EncodeInteger(c.DraftRevision, budget), "Context.DraftRevision");
            Text(c.ContentFingerprint, "Context.ContentFingerprint", budget); Text(c.RuleVersion, "Context.RuleVersion", budget);
            Text(c.NumericContractVersion, "Context.NumericContractVersion", budget); Text(c.RandomContractVersion, "Context.RandomContractVersion", budget);
            foreach (var note in c.SourceNotes) Text(note, "Context.SourceNotes", budget);
        }

        internal static void BindingBudget(ContentBinding binding, SaveCodecBudget budget)
        {
            SaveCodecFailure.Require(binding != null, "MissingField", "Binding");
            Text(binding.PackageId, "Binding.PackageId", budget, true);
            Text(binding.ContentFingerprint, "Binding.ContentFingerprint", budget, true); Text(binding.RuleVersion, "Binding.RuleVersion", budget, true);
            Text(binding.NumericContractVersion, "Binding.NumericContractVersion", budget, true); Text(binding.RandomContractVersion, "Binding.RandomContractVersion", budget, true);
        }

        internal static void Text(string value, string path, SaveCodecBudget budget, bool strictUnicode = false)
        {
            SaveCodecFailure.Require(!string.IsNullOrWhiteSpace(value), "MissingField", path);
            SaveCodecFailure.Limit((ulong)value.Length, (ulong)budget.MaxStringCodeUnits, path, "StringCodeUnits");
            if (!strictUnicode) return; // Candidate v1 deliberately preserves original UTF-16 units.
            for (var i = 0; i < value.Length; i++)
            {
                if (!char.IsSurrogate(value[i])) continue;
                SaveCodecFailure.Require(char.IsHighSurrogate(value[i]) && i + 1 < value.Length && char.IsLowSurrogate(value[i + 1]), "InvalidValue", path);
                i++;
            }
        }

        internal static SaveBinding SaveBinding(PreparedRuleContext context, SaveCodecBudget budget, string level = null, string version = null)
        {
            if (context is PreparedPublishedRuleContext p)
            {
                BindingBudget(p.Binding, budget);
                if (level != null)
                {
                    Text(version, "Binding.LevelVersion", budget, true);
                    var number = BusinessFields.Take(ExactSaveValueCodec.DecodeInteger(version, budget), "Binding.LevelVersion");
                    BusinessFields.Take(DefinitionBinding.Prepare(p.Binding, level, number.Value, budget), "Binding");
                }
                else SaveCodecFailure.Require(version == null, "InvalidValue", "Binding.LevelVersion");
                return new SaveBinding(level == null ? SaveBindingKind.Content : SaveBindingKind.Definition, p.Binding.PackageId, null, null,
                    p.ContentFingerprint, p.RuleVersion, p.NumericContractVersion, p.RandomContractVersion, null, level, version);
            }
            if (context is PreparedCandidateContext c)
                return new SaveBinding(level == null ? SaveBindingKind.CandidateContent : SaveBindingKind.CandidateDefinition, null, c.DraftId, c.DraftRevision,
                    c.ContentFingerprint, c.RuleVersion, c.NumericContractVersion, c.RandomContractVersion, c.SourceNotes, level, version);
            throw new SaveCodecFailure("UnsupportedBinding", "Context");
        }
    }
}
