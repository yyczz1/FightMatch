using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using FightMatch.Core;
using FlowPuzzle.Core;

namespace FightMatch.Content
{
    public enum DemoCoordinateCandidate { Unspecified, AssumedBottomLeft, AssumedTopLeft }

    // One owner per draft. Returning to equal bytes still advances the revision.
    public sealed class DemoContentDraft
    {
        private readonly object sync = new object();
        private BigInteger revision = BigInteger.One;
        public string DraftId { get; }
        public BigInteger Revision { get { lock (sync) return revision; } }
        public DemoContentDraft(string draftId)
        { if (string.IsNullOrWhiteSpace(draftId)) throw new ArgumentException("Missing DraftId", nameof(draftId)); DraftId = draftId; }
        public void Revise() { lock (sync) revision += BigInteger.One; }
        public DemoContentJob BeginJob(CancellationToken cancellation = default)
        { lock (sync) return new DemoContentJob(this, revision, cancellation); }
        internal T Commit<T>(DemoContentJob job, Func<T> action)
        { lock (sync) { job.Check(); return action(); } }
    }

    public sealed class DemoContentJob
    {
        private readonly DemoContentDraft owner;
        private readonly CancellationToken cancellation;
        public string DraftId => owner.DraftId;
        public BigInteger Revision { get; }
        public bool IsCurrent => !cancellation.IsCancellationRequested && owner.Revision == Revision;
        internal DemoContentJob(DemoContentDraft owner, BigInteger revision, CancellationToken cancellation)
        { this.owner = owner; Revision = revision; this.cancellation = cancellation; }
        internal T Commit<T>(Func<T> action) { return owner.Commit(this, action); }
        internal void Check()
        {
            if (cancellation.IsCancellationRequested) throw new ContentFailure("Cancelled", "Job");
            if (owner.Revision != Revision) throw new ContentFailure("StaleContext", "Job.Revision");
        }
    }

    // Mutable shells must remain stable during each synchronous call. Contexts are compiler-owned.
    public sealed class DemoContentInput
    {
        public string RuleVersion { get; set; }
        public string NumericContractVersion { get; set; }
        public string RandomContractVersion { get; set; }
        public List<DemoContentSourceInput> Sources { get; set; }
        public List<string> SourceNotes { get; set; }
        public DemoCoordinateCandidate Coordinates { get; set; }
        public GrowthDefinitionInput Growth { get; set; }
        public string PlayerId { get; set; }
        public string CharacterId { get; set; }
        public BigInteger CharacterLevel { get; set; }
        public BigInteger CharacterExperience { get; set; }
        public int OriginalSlot { get; set; }
        public string ChallengeId { get; set; }
        public string AttemptId { get; set; }
        public string EntryBaselineId { get; set; }
        public LevelInput Level { get; set; }
        public List<DemoContentRouteInput> SourceRoutes { get; set; }
        public EntryCarryMode CarryMode { get; set; }
        public List<string> RequiredFeatures { get; set; }
        public List<string> LearnedSkills { get; set; }
        public List<DemoPrdParameterInput> Parameters { get; set; }
        public CandidateRewardDefinitionInput Reward { get; set; }
    }
    public sealed class DemoContentSourceInput
    {
        public string Path { get; set; }
        public string Locator { get; set; }
        public string Sha256 { get; set; }
        public byte[] Bytes { get; set; }
    }
    public sealed class DemoContentRouteInput
    {
        public string FaceId { get; set; }
        public string PairId { get; set; }
        public List<FlowPos> Cells { get; set; }
    }
    public sealed class DemoPrdParameterInput
    {
        public ExactRational Target { get; set; }
        public ExactRational C { get; set; }
        public ExactRational Epsilon { get; set; }
    }
    public sealed class DemoContentSource
    {
        public string Path { get; }
        public string Locator { get; }
        public string Sha256 { get; }
        internal DemoContentSource(string path, string locator, string sha) { Path = path; Locator = locator; Sha256 = sha; }
    }
    public sealed class DemoContentRoute
    {
        public string FaceId { get; }
        public string PairId { get; }
        public IReadOnlyList<FlowPos> SourceCells { get; }
        public IReadOnlyList<FlowPos> Cells { get; }
        internal DemoContentRoute(DemoContentRouteInput input, IEnumerable<FlowPos> cells)
        { FaceId = input.FaceId; PairId = input.PairId; SourceCells = input.Cells.AsReadOnlyCopy(); Cells = new List<FlowPos>(cells).AsReadOnly(); }
    }
    public sealed class DemoPreparedContent
    {
        public DemoContentJob Job { get; }
        public string Fingerprint => Entry.Context.ContentFingerprint;
        public bool IsCurrent => Job.IsCurrent;
        public bool ReviewRequired => true;
        public bool CommitEligible => false;
        public DemoCoordinateCandidate Coordinates { get; }
        public PreparedBattleEntry Entry { get; }
        public CandidateCharacterState Character { get; }
        public CandidateComputedStats ComputedStats { get; }
        public CandidateRewardDefinition Reward { get; }
        public IReadOnlyList<DemoContentSource> Sources { get; }
        public IReadOnlyList<DemoContentRoute> Routes { get; }
        public IReadOnlyList<DemoParameterEvidence> Parameters { get; }
        internal DemoPreparedContent(DemoContentJob job, DemoCoordinateCandidate coordinates, PreparedBattleEntry entry,
            CandidateCharacterState character, CandidateComputedStats stats, CandidateRewardDefinition reward,
            List<DemoContentSource> sources, List<DemoContentRoute> routes, List<DemoParameterEvidence> parameters)
        { Job = job; Coordinates = coordinates; Entry = entry; Character = character; ComputedStats = stats; Reward = reward;
            Sources = sources.AsReadOnly(); Routes = routes.AsReadOnly(); Parameters = parameters.AsReadOnly(); }
    }
    public sealed class DemoContentPreparationResult
    {
        public bool IsAccepted => Candidate != null;
        public DemoPreparedContent Candidate { get; }
        public string RejectionCode { get; }
        public string FieldPath { get; }
        internal DemoContentPreparationResult(DemoPreparedContent candidate) { Candidate = candidate; }
        internal DemoContentPreparationResult(string code, string path) { RejectionCode = code; FieldPath = path; }
    }
    public sealed class DemoShortTermEvidence
    {
        public int Opportunities { get; }
        public ExactRational AtLeastOne { get; }
        public ExactRational ExpectedSuccessesPerOpportunity { get; }
        internal DemoShortTermEvidence(int count, ExactRational atLeastOne, ExactRational mean)
        { Opportunities = count; AtLeastOne = atLeastOne; ExpectedSuccessesPerOpportunity = mean; }
    }
    public sealed class DemoParameterEvidence
    {
        public ExactRational Target { get; }
        public ExactRational C { get; }
        public ExactRational Epsilon { get; }
        public BigInteger N { get; }
        public ExactRational ExpectedWait { get; }
        public ExactRational ModelRate { get; }
        public ExactRational SignedError { get; }
        public ExactRational FirstOpportunityRate => C;
        public bool WithinProposedTolerance { get; }
        public IReadOnlyList<DemoShortTermEvidence> ShortTerm { get; }
        internal DemoParameterEvidence(ExactRational p, ExactRational c, ExactRational epsilon, BigInteger n,
            ExactRational wait, ExactRational rate, ExactRational error, bool within, List<DemoShortTermEvidence> shortTerm)
        { Target = p; C = c; Epsilon = epsilon; N = n; ExpectedWait = wait; ModelRate = rate; SignedError = error;
            WithinProposedTolerance = within; ShortTerm = shortTerm.AsReadOnly(); }
    }
    public sealed class DemoParameterResult
    {
        public bool IsAccepted => Evidence != null;
        public DemoParameterEvidence Evidence { get; }
        public string RejectionCode { get; }
        public string FieldPath { get; }
        internal DemoParameterResult(DemoParameterEvidence evidence) { Evidence = evidence; }
        internal DemoParameterResult(string code, string path) { RejectionCode = code; FieldPath = path; }
    }
    internal sealed class ContentFailure : Exception
    {
        internal readonly string Code;
        internal readonly string Path;
        internal ContentFailure(string code, string path) : base(code + ": " + path) { Code = code; Path = path; }
    }
    internal static class ContentChecks
    {
        internal static void Need(bool condition, string code, string path) { if (!condition) throw new ContentFailure(code, path); }
        internal static void Text(string value, string path) { Need(!string.IsNullOrWhiteSpace(value), "MissingField", path); }
        internal static void Root(object value, string path) { Need(value != null, "MissingField", path); }
        internal static IReadOnlyList<T> AsReadOnlyCopy<T>(this List<T> source) { return new List<T>(source).AsReadOnly(); }
    }
}
