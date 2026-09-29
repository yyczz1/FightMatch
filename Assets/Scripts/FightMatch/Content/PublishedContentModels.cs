using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using FightMatch.Core;

namespace FightMatch.Content
{
    // Mutable authoring shells. Callers keep them stable for the duration of a synchronous call.
    public sealed class PublishedSource
    {
        public int SchemaVersion { get; set; }
        public string PackageId { get; set; }
        public string DraftId { get; set; }
        public BigInteger Revision { get; set; }
        public string RuleVersion { get; set; }
        public string NumericContractVersion { get; set; }
        public string RandomContractVersion { get; set; }
        public List<string> RequiredCapabilities { get; set; }
        public DemoCoordinateCandidate Coordinates { get; set; }
        public List<DemoContentSourceInput> Sources { get; set; }
        public List<string> SourceNotes { get; set; }
        public GrowthDefinitionInput Growth { get; set; }
        public CandidateInventoryDefinitionInput Inventory { get; set; }
        public CandidateProgressionDefinitionInput Progression { get; set; }
        public List<PublishedLevelInput> Levels { get; set; }
        public List<DemoPrdParameterInput> Parameters { get; set; }
        public NewProfileDefinitionInput NewProfile { get; set; }
        internal byte[] OriginalBytes;
        internal string OriginalCanonicalSha;
    }
    public sealed class PublishedLevelInput
    {
        public LevelInput Level { get; set; }
        public List<DemoContentRouteInput> SourceRoutes { get; set; }
        public CandidateRewardDefinitionInput Reward { get; set; }
    }
    public sealed class NewProfileDefinitionInput
    {
        public string Id { get; set; }
        public BigInteger RecordVersion { get; set; }
        public string CharacterId { get; set; }
        public string ClassId { get; set; }
        public BigInteger Level { get; set; }
        public BigInteger Experience { get; set; }
        public ExactRational Hp { get; set; }
        public int OriginalSlot { get; set; }
        public List<int> EmptySlots { get; set; }
        public List<CandidateInventoryQuantityInput> Inventory { get; set; }
        public List<string> Carry { get; set; }
        public List<string> LearnedActiveSkills { get; set; }
        public string Recovery { get; set; }
        public List<string> OpenLevels { get; set; }
    }
    public sealed class NewProfileDefinition
    {
        public string Id { get; }
        public BigInteger RecordVersion { get; }
        public string CharacterId { get; }
        public string ClassId { get; }
        public BigInteger Level { get; }
        public BigInteger Experience { get; }
        public ExactRational Hp { get; }
        public int OriginalSlot { get; }
        public IReadOnlyList<byte> CanonicalBytes { get; }
        public string Sha256 { get; }
        internal NewProfileDefinition(NewProfileDefinitionInput input, byte[] bytes)
        { Id = input.Id; RecordVersion = input.RecordVersion; CanonicalBytes = Array.AsReadOnly((byte[])bytes.Clone()); Sha256 = PublishedContentCodec.Sha256(bytes);
            CharacterId = input.CharacterId; ClassId = input.ClassId; Level = input.Level; Experience = input.Experience; Hp = input.Hp; OriginalSlot = input.OriginalSlot; }
    }
    public sealed class ContentConsumerCapabilities
    {
        public IReadOnlyList<string> Capabilities { get; }
        public int MaxSourceBytes { get; }
        public int MaxCollectionEntries { get; }
        public int MaxStringCodeUnits { get; }
        public ContentConsumerCapabilities(IEnumerable<string> capabilities, int maxSourceBytes = 262144,
            int maxCollectionEntries = 512, int maxStringCodeUnits = 8192)
        {
            if (capabilities == null || maxSourceBytes < 0 || maxCollectionEntries < 0 || maxStringCodeUnits < 0) throw new ArgumentException("Capabilities/budget");
            Capabilities = new List<string>(capabilities).AsReadOnly(); MaxSourceBytes = maxSourceBytes;
            MaxCollectionEntries = maxCollectionEntries; MaxStringCodeUnits = maxStringCodeUnits;
        }
        public static ContentConsumerCapabilities Current => new ContentConsumerCapabilities(new[] {
            "NC01.ExactRational", "RC01.PCG32.PRD", "PC01.ExactParameterEvidence", "SC01.sc01-pcg32-le128-v1" });
        internal void Check(PublishedSource source)
        {
            ContentChecks.Need(source.SchemaVersion == 1, "UnsupportedSchema", "SchemaVersion");
            ContentChecks.Need(source.RuleVersion == "demo-r1" && source.NumericContractVersion == "RC01" &&
                source.RandomContractVersion == "PC01+SC01", "UnsupportedBinding", "ContractVersions");
            ContentChecks.Root(source.RequiredCapabilities, "RequiredCapabilities");
            var required = Current.Capabilities;
            ContentChecks.Need(source.RequiredCapabilities.Count == required.Count && required.All(source.RequiredCapabilities.Contains) &&
                source.RequiredCapabilities.All(Capabilities.Contains), "UnsupportedCapability", "RequiredCapabilities");
        }
    }
    public sealed class ContentStoreBudget
    {
        public int MaxRecordBytes { get; }
        public ExactMathBudget Math { get; }
        public ContentStoreBudget(ExactMathBudget math, int maxRecordBytes = 16 * 1024 * 1024)
        { Math = math ?? throw new ArgumentNullException(nameof(math)); if (maxRecordBytes < 0) throw new ArgumentOutOfRangeException(nameof(maxRecordBytes)); MaxRecordBytes = maxRecordBytes; }
    }
    public sealed class PublicationResult<T>
    {
        public T Value { get; }
        public bool IsAccepted => RejectionCode == null;
        public string RejectionCode { get; }
        public string FieldPath { get; }
        internal PublicationResult(T value) { Value = value; }
        internal PublicationResult(string code, string path) { RejectionCode = code; FieldPath = path; }
        internal static PublicationResult<T> Run(Func<T> action)
        {
            try { return new PublicationResult<T>(action()); }
            catch (ContentFailure e) { return new PublicationResult<T>(e.Code, e.Path); }
            catch (ExactMathLimitException) { return new PublicationResult<T>("BudgetExceeded", "Math"); }
        }
    }
    public sealed class PreparedPublicationResult
    {
        public PreparedPublication Prepared { get; }
        public bool IsAccepted => Prepared != null;
        public string RejectionCode { get; }
        public string FieldPath { get; }
        internal PreparedPublicationResult(PublicationResult<PreparedPublication> result)
        { Prepared = result.Value; RejectionCode = result.RejectionCode; FieldPath = result.FieldPath; }
    }
    public sealed class ContentValidationEvidence
    {
        public IReadOnlyList<byte> Bytes { get; }
        public string Sha256 { get; }
        internal ContentValidationEvidence(byte[] bytes)
        { Bytes = Array.AsReadOnly((byte[])bytes.Clone()); Sha256 = PublishedContentCodec.Sha256(bytes); }
    }
    public sealed class PreparedPublication
    {
        internal DemoContentJob Job { get; }
        public string DraftId { get; }
        public BigInteger Revision { get; }
        public ContentBinding Binding { get; }
        public PublishedRuleDefinitions Definitions { get; }
        public NewProfileDefinition NewProfile { get; }
        public IReadOnlyList<DefinitionBinding> DefinitionBindings { get; }
        public IReadOnlyList<byte> SourceBytes { get; }
        public IReadOnlyList<byte> PayloadBytes { get; }
        public string SourceSha256 { get; }
        public string PayloadSha256 { get; }
        public ContentValidationEvidence Validation { get; }
        public IReadOnlyList<DemoContentReplayResult> Replays { get; }
        internal PreparedPublication(PublishedSource source, DemoContentJob job, ContentBinding binding, PublishedRuleDefinitions definitions,
            NewProfileDefinition profile, List<DefinitionBinding> levels, byte[] original, byte[] payload, byte[] validation, List<DemoContentReplayResult> replays)
        {
            Job = job; DraftId = source.DraftId; Revision = source.Revision; Binding = binding; Definitions = definitions; NewProfile = profile;
            DefinitionBindings = levels.AsReadOnly(); SourceBytes = Array.AsReadOnly(original); PayloadBytes = Array.AsReadOnly(payload);
            SourceSha256 = PublishedContentCodec.Sha256(original); PayloadSha256 = PublishedContentCodec.Sha256(payload);
            Validation = new ContentValidationEvidence(validation); Replays = replays.AsReadOnly();
        }
    }
    // Explicit review data, never a boolean on a candidate. Independent authority is supplied by the caller.
    public sealed class ContentReviewEvidence
    {
        internal byte[] OriginalBytes;
        internal string OriginalCanonicalSha;
        public int SchemaVersion { get; set; }
        public string Verdict { get; set; }
        public string AuthorTaskId { get; set; }
        public string AuthorTurnId { get; set; }
        public string ReviewerTaskId { get; set; }
        public string ReviewerTurnId { get; set; }
        public string SourcePacketRangeSha256 { get; set; }
        public string ApprovalBasis { get; set; }
        public string ApprovedMappingSha256 { get; set; }
        public string DraftId { get; set; }
        public BigInteger Revision { get; set; }
        public ContentBindingRecord Binding { get; set; }
        public List<DefinitionBindingRecord> DefinitionBindings { get; set; }
        public int SourceBytes { get; set; }
        public string SourceSha256 { get; set; }
        public int PayloadBytes { get; set; }
        public string PayloadSha256 { get; set; }
        public int ValidationBytes { get; set; }
        public string ValidationSha256 { get; set; }
    }
    public sealed class ContentBindingRecord
    {
        public string PackageId { get; set; }
        public string ContentFingerprint { get; set; }
        public string RuleVersion { get; set; }
        public string NumericContractVersion { get; set; }
        public string RandomContractVersion { get; set; }
        public static ContentBindingRecord From(ContentBinding b) => new ContentBindingRecord { PackageId = b.PackageId,
            ContentFingerprint = b.ContentFingerprint, RuleVersion = b.RuleVersion, NumericContractVersion = b.NumericContractVersion, RandomContractVersion = b.RandomContractVersion };
        internal ContentBinding Take(ExactMathBudget math)
        {
            var r = ContentBinding.Prepare(PackageId, ContentFingerprint, RuleVersion, NumericContractVersion, RandomContractVersion, new SaveCodecBudget(math));
            ContentChecks.Need(r.IsAccepted, r.RejectionCode, r.FieldPath); return r.Value;
        }
    }
    public sealed class DefinitionBindingRecord
    {
        public string LevelId { get; set; }
        public BigInteger LevelVersion { get; set; }
        public static DefinitionBindingRecord From(DefinitionBinding b) => new DefinitionBindingRecord { LevelId = b.LevelId, LevelVersion = b.LevelVersion };
    }
    public sealed class ContentReleaseSet
    {
        public int SchemaVersion { get; set; }
        public string Scope { get; set; }
        public string ReleaseSetId { get; set; }
        public ContentBindingRecord Binding { get; set; }
        public string PublicationReceiptSha256 { get; set; }
    }
    public sealed class ResolvedPublication
    {
        private readonly IReadOnlyList<DemoContentReplayResult> defaultReferences;
        public IReadOnlyList<DemoContentReplayResult> GetDefaultReferences() => defaultReferences;
        public ContentBinding Binding => Definitions.Binding;
        public PublishedRuleDefinitions Definitions { get; }
        public NewProfileDefinition NewProfile { get; }
        public IReadOnlyList<byte> ReceiptBytes { get; }
        public IReadOnlyList<DemoParameterEvidence> Parameters { get; }
        internal ResolvedPublication(PublishedRuleDefinitions definitions, NewProfileDefinition profile, byte[] receipt)
            : this(definitions, profile, receipt, Array.Empty<DemoParameterEvidence>()) { }
        internal ResolvedPublication(PublishedRuleDefinitions definitions, NewProfileDefinition profile, byte[] receipt,
            IReadOnlyList<DemoParameterEvidence> parameters, IReadOnlyList<DemoContentReplayResult> references = null)
        { Definitions = definitions; NewProfile = profile; ReceiptBytes = Array.AsReadOnly(receipt);
            Parameters = new List<DemoParameterEvidence>(parameters).AsReadOnly();
            defaultReferences = new List<DemoContentReplayResult>(references ?? Array.Empty<DemoContentReplayResult>()).AsReadOnly(); }
    }
    public sealed class ContentPublicationOutcome
    {
        public string Status { get; }
        public string OperationId { get; }
        public ResolvedPublication Publication { get; }
        internal ContentPublicationOutcome(string operation, ResolvedPublication publication)
        { Status = "Completed"; OperationId = operation; Publication = publication; }
    }
}
