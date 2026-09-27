using System;
using System.Collections.Generic;
using System.Numerics;

namespace FightMatch.Core
{
    public abstract class RuleContext
    {
        public abstract string ContentFingerprint { get; set; }
        public abstract string RuleVersion { get; set; }
        public abstract string NumericContractVersion { get; set; }
        public abstract string RandomContractVersion { get; set; }
        // Legacy candidate accessors are deliberately unavailable on the published branch.
        public virtual string DraftId { get => throw CandidateOnly(); set => throw CandidateOnly(); }
        public virtual BigInteger DraftRevision { get => throw CandidateOnly(); set => throw CandidateOnly(); }
        public virtual List<string> SourceNotes { get => throw CandidateOnly(); set => throw CandidateOnly(); }
        private static NotSupportedException CandidateOnly() { return new NotSupportedException("Candidate context required."); }
    }

    public abstract class PreparedRuleContext
    {
        public abstract string ContentFingerprint { get; }
        public abstract string RuleVersion { get; }
        public abstract string NumericContractVersion { get; }
        public abstract string RandomContractVersion { get; }
        public virtual string DraftId => throw CandidateOnly();
        public virtual BigInteger DraftRevision => throw CandidateOnly();
        public virtual IReadOnlyList<string> SourceNotes => throw CandidateOnly();
        private static NotSupportedException CandidateOnly() { return new NotSupportedException("Candidate context required."); }
    }

    public sealed class PublishedRuleContext : RuleContext
    {
        public ContentBinding Binding { get; }
        public PublishedRuleContext(ContentBinding binding) { Binding = binding; }
        public override string ContentFingerprint { get => Binding?.ContentFingerprint; set => throw Immutable(); }
        public override string RuleVersion { get => Binding?.RuleVersion; set => throw Immutable(); }
        public override string NumericContractVersion { get => Binding?.NumericContractVersion; set => throw Immutable(); }
        public override string RandomContractVersion { get => Binding?.RandomContractVersion; set => throw Immutable(); }
        private static NotSupportedException Immutable() { return new NotSupportedException("Published bindings are immutable."); }
    }

    public sealed class PreparedPublishedRuleContext : PreparedRuleContext
    {
        public ContentBinding Binding { get; }
        public override string ContentFingerprint => Binding.ContentFingerprint;
        public override string RuleVersion => Binding.RuleVersion;
        public override string NumericContractVersion => Binding.NumericContractVersion;
        public override string RandomContractVersion => Binding.RandomContractVersion;
        internal PreparedPublishedRuleContext(ContentBinding binding) { Binding = binding; }
    }
}
