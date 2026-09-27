using System;
using System.Collections.Generic;
using System.Linq;
using FightMatch.Core;

namespace FightMatch.Platform
{
    public enum SaveRecoveryRootKind { Current, Backup, Pending }

    public sealed class SaveRecoveryRoot
    {
        public SaveRecoveryRootKind Kind { get; }
        public string SourceName { get; }
        public SnapshotDescriptor Descriptor { get; }
        public IReadOnlyList<RequiredSliceContract> RequiredSliceContracts { get; }
        public SaveRequirements Requirements { get; }
        internal SaveRecoveryRoot(SaveRecoveryRootKind kind, string source, SnapshotDescriptor descriptor,
            IReadOnlyList<RequiredSliceContract> slices, SaveRequirements requirements)
        {
            Kind = kind; SourceName = source; Descriptor = LocalSaveValues.Freeze(descriptor);
            RequiredSliceContracts = Array.AsReadOnly(slices.Select(SaveRecoveryCapabilities.CopySlice).ToArray());
            Requirements = SaveRecoveryCapabilities.CopyRequirements(requirements);
        }
    }

    public sealed class SaveRecoveryView
    {
        public SaveHeadStatus Status { get; }
        public SaveRecoveryRoot Current { get; }
        public IReadOnlyList<SaveRecoveryRoot> RetainedRoots { get; }
        public IReadOnlyList<SaveFileObservation> Files { get; }
        public IReadOnlyList<LocalSaveDiagnostic> Diagnostics { get; }
        public bool RequirementsComplete { get; }
        public bool EvidenceComplete { get; }
        public bool HasUnresolvedCandidate { get; }
        public bool RedundancyDegraded { get; }
        internal LocalSaveStore Owner { get; }
        internal IReadOnlyList<byte> Evidence { get; }
        internal SaveCommitTicket Ticket { get; }
        internal SaveRecoverySummary Summary { get; }
        internal SaveRecoveryView(LocalSaveStore owner, SaveHeadStatus status, SaveRecoveryRoot current,
            IEnumerable<SaveRecoveryRoot> retained, IEnumerable<SaveFileObservation> files,
            IEnumerable<LocalSaveDiagnostic> diagnostics, bool requirements, bool evidenceComplete, bool unresolved,
            bool degraded, byte[] evidence, SaveCommitTicket ticket, SaveRecoverySummary summary)
        {
            Owner = owner; Status = status; Current = current; RetainedRoots = Array.AsReadOnly(retained.ToArray());
            Files = Array.AsReadOnly(files.ToArray()); Diagnostics = Array.AsReadOnly(diagnostics.ToArray());
            RequirementsComplete = requirements; EvidenceComplete = evidenceComplete; HasUnresolvedCandidate = unresolved;
            RedundancyDegraded = degraded; Evidence = LocalSaveValues.Digest(evidence); Ticket = ticket; Summary = summary;
        }
    }

    public sealed class SaveCleanupResult
    {
        public SnapshotDescriptor CurrentHead { get; }
        public string PreviousCommitId { get; }
        public IReadOnlyList<string> RemovedNames { get; }
        internal SaveCleanupResult(SnapshotDescriptor head, IEnumerable<string> removed)
        {
            CurrentHead = LocalSaveValues.Freeze(head); PreviousCommitId = head.ParentCommitId;
            RemovedNames = Array.AsReadOnly(removed.ToArray());
        }
    }

    public sealed class SaveRecoveryCapabilities
    {
        public IReadOnlyList<RequiredSliceContract> ReadableSlices { get; }
        public IReadOnlyList<SaveBinding> Bindings { get; }
        public IReadOnlyList<string> RuleVersions { get; }
        public IReadOnlyList<string> NumericContractVersions { get; }
        public IReadOnlyList<string> RandomContractVersions { get; }
        public IReadOnlyList<string> FeatureIds { get; }
        public SaveRecoveryCapabilities(IReadOnlyList<RequiredSliceContract> readableSlices, IReadOnlyList<SaveBinding> bindings,
            IReadOnlyList<string> ruleVersions, IReadOnlyList<string> numericContractVersions,
            IReadOnlyList<string> randomContractVersions, IReadOnlyList<string> featureIds)
        {
            if (readableSlices == null) throw new ArgumentNullException(nameof(readableSlices));
            if (bindings == null) throw new ArgumentNullException(nameof(bindings));
            RuleVersions = Strings(ruleVersions, nameof(ruleVersions));
            NumericContractVersions = Strings(numericContractVersions, nameof(numericContractVersions));
            RandomContractVersions = Strings(randomContractVersions, nameof(randomContractVersions));
            FeatureIds = Strings(featureIds, nameof(featureIds));
            var slices = new List<RequiredSliceContract>();
            for (var i = 0; i < readableSlices.Count; i++)
            {
                var x = readableSlices[i]; var p = "ReadableSlices[" + i + "]";
                Argument(x != null, p); Text(x.SliceId, p + ".SliceId"); Text(x.OwnerId, p + ".OwnerId");
                Argument(x.SchemaVersion > 0, p + ".SchemaVersion");
                Argument(!slices.Any(y => SameSlice(x, y)), p);
                slices.Add(CopySlice(x));
            }
            ReadableSlices = Array.AsReadOnly(slices.ToArray());
            var frozen = new List<SaveBinding>();
            for (var i = 0; i < bindings.Count; i++)
            {
                var x = bindings[i]; var p = "Bindings[" + i + "]"; Argument(x != null, p);
                var candidate = x.Kind == SaveBindingKind.CandidateContent || x.Kind == SaveBindingKind.CandidateDefinition;
                var definition = x.Kind == SaveBindingKind.Definition || x.Kind == SaveBindingKind.CandidateDefinition;
                Argument(candidate || x.Kind == SaveBindingKind.Content || x.Kind == SaveBindingKind.Definition, p + ".Kind");
                Text(x.ContentFingerprint, p + ".ContentFingerprint"); Text(x.RuleVersion, p + ".RuleVersion");
                Text(x.NumericContractVersion, p + ".NumericContractVersion"); Text(x.RandomContractVersion, p + ".RandomContractVersion");
                if (candidate)
                {
                    Argument(x.PackageId == null, p + ".PackageId"); Text(x.DraftId, p + ".DraftId");
                    Argument(x.DraftRevision.HasValue && x.DraftRevision.Value.Sign > 0, p + ".DraftRevision");
                    Argument(x.SourceNotes != null, p + ".SourceNotes");
                    for (var j = 0; j < x.SourceNotes.Count; j++) Text(x.SourceNotes[j], p + ".SourceNotes[" + j + "]");
                }
                else
                {
                    Text(x.PackageId, p + ".PackageId"); Argument(x.DraftId == null, p + ".DraftId");
                    Argument(x.DraftRevision == null, p + ".DraftRevision"); Argument(x.SourceNotes == null, p + ".SourceNotes");
                }
                if (definition) { Text(x.LevelId, p + ".LevelId"); Text(x.LevelVersion, p + ".LevelVersion"); }
                else { Argument(x.LevelId == null, p + ".LevelId"); Argument(x.LevelVersion == null, p + ".LevelVersion"); }
                Argument(!frozen.Any(y => x.Kind == y.Kind && x.PackageId == y.PackageId && x.DraftId == y.DraftId &&
                    x.DraftRevision == y.DraftRevision && x.LevelId == y.LevelId && x.LevelVersion == y.LevelVersion), p);
                frozen.Add(CopyBinding(x));
            }
            Bindings = Array.AsReadOnly(frozen.ToArray());
        }
        private static void Argument(bool condition, string path)
        { if (!condition) throw new ArgumentException("Invalid, duplicate or conflicting capability.", path); }
        private static void Text(string value, string path) { Argument(!string.IsNullOrWhiteSpace(value), path); }
        private static IReadOnlyList<string> Strings(IReadOnlyList<string> values, string path)
        {
            if (values == null) throw new ArgumentNullException(path);
            var seen = new HashSet<string>(StringComparer.Ordinal); var copy = new string[values.Count];
            for (var i = 0; i < copy.Length; i++) { Text(values[i], path + "[" + i + "]"); Argument(seen.Add(values[i]), path + "[" + i + "]"); copy[i] = values[i]; }
            return Array.AsReadOnly(copy);
        }
        internal static RequiredSliceContract CopySlice(RequiredSliceContract x)
        { return new RequiredSliceContract(x.SliceId, x.OwnerId, x.SchemaVersion); }
        internal static SaveBinding CopyBinding(SaveBinding x)
        { return new SaveBinding(x.Kind, x.PackageId, x.DraftId, x.DraftRevision, x.ContentFingerprint, x.RuleVersion,
            x.NumericContractVersion, x.RandomContractVersion, x.SourceNotes == null ? null : Array.AsReadOnly(x.SourceNotes.ToArray()), x.LevelId, x.LevelVersion); }
        internal static SaveRequirements CopyRequirements(SaveRequirements x)
        { return new SaveRequirements(Array.AsReadOnly(x.Bindings.Select(CopyBinding).ToArray()), Array.AsReadOnly(x.RuleVersions.ToArray()),
            Array.AsReadOnly(x.NumericContractVersions.ToArray()), Array.AsReadOnly(x.RandomContractVersions.ToArray()), Array.AsReadOnly(x.FeatureIds.ToArray())); }
        internal static bool SameSlice(RequiredSliceContract a, RequiredSliceContract b)
        { return a.SliceId == b.SliceId && a.OwnerId == b.OwnerId && a.SchemaVersion == b.SchemaVersion; }
        internal static bool SameBinding(SaveBinding a, SaveBinding b)
        { return a.Kind == b.Kind && a.PackageId == b.PackageId && a.DraftId == b.DraftId && a.DraftRevision == b.DraftRevision &&
            a.ContentFingerprint == b.ContentFingerprint && a.RuleVersion == b.RuleVersion && a.NumericContractVersion == b.NumericContractVersion &&
            a.RandomContractVersion == b.RandomContractVersion && a.LevelId == b.LevelId && a.LevelVersion == b.LevelVersion &&
            (a.SourceNotes == null ? b.SourceNotes == null : b.SourceNotes != null && a.SourceNotes.SequenceEqual(b.SourceNotes, StringComparer.Ordinal)); }
        internal void CheckBudget(SaveStoreBudget budget)
        {
            Action<int, string> count = (n, p) => LocalSaveFailure.Limit((ulong)n, (ulong)budget.Codec.MaxCollectionEntries, p, "CollectionEntries");
            Action<string, string> text = (s, p) => { if (s != null) LocalSaveFailure.Limit((ulong)s.Length, (ulong)budget.Codec.MaxStringCodeUnits, p, "StringCodeUnits"); };
            count(ReadableSlices.Count, "Capabilities.ReadableSlices"); count(Bindings.Count, "Capabilities.Bindings");
            foreach (var s in ReadableSlices) { text(s.SliceId, "Capabilities.SliceId"); text(s.OwnerId, "Capabilities.OwnerId"); }
            foreach (var b in Bindings)
            {
                foreach (var s in new[] { b.PackageId, b.DraftId, b.ContentFingerprint, b.RuleVersion, b.NumericContractVersion, b.RandomContractVersion, b.LevelId, b.LevelVersion }) text(s, "Capabilities.Binding");
                if (b.DraftRevision.HasValue) ExactRational.Create(b.DraftRevision.Value, 1, budget.Codec.Math);
                if (b.SourceNotes != null) { count(b.SourceNotes.Count, "Capabilities.SourceNotes"); foreach (var s in b.SourceNotes) text(s, "Capabilities.SourceNotes"); }
            }
            foreach (var set in new[] { RuleVersions, NumericContractVersions, RandomContractVersions, FeatureIds })
            { count(set.Count, "Capabilities.Versions"); foreach (var s in set) text(s, "Capabilities.Versions"); }
        }
    }
}
