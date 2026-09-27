using System;
using System.Collections.Generic;
using System.Numerics;

namespace FightMatch.Core
{
    public enum CandidatePermanentKind
    {
        UseExperienceCards = 1, LearnSkill, Craft, Equip, SetPreference,
        ConfirmTeachingExplanation, BeginTeachingGift
    }
    public enum CandidatePermanentSourceKind { OrdinaryBaseReward = 1, ExistingAdvertisementHeld = 2 }
    public enum CandidatePermanentEndpoint { Held = 1, Consumed, Transformed }
    public enum CandidateInclusionDisposition { Absent = 1, SameEffect, DescendantConsumed, SelectedConsumption, SelectedHolding, Unresolved }

    public sealed class CandidatePermanentDraft
    {
        public CandidatePermanentKind Kind { get; set; }
        public string CharacterId { get; set; }
        public string DefinitionId { get; set; }
        public BigInteger? Quantity { get; set; }
        public bool? Enabled { get; set; }
        public BigInteger? PreferenceRevision { get; set; }
        public string StepId { get; set; }
        public IReadOnlyList<CandidatePermanentPortion> SelectedInputs { get; set; }
    }

    public sealed class CandidateOriginalGrantRef
    {
        public CandidatePermanentSourceKind Kind { get; }
        public string PlayerId { get; }
        public string AttemptId { get; }
        public string SettlementId { get; }
        public string GrantId { get; }
        public string Purpose { get; }
        public string FactId { get; }
        public string UseId { get; }
        public ContentBinding Binding { get; }

        public CandidateOriginalGrantRef(CandidatePermanentSourceKind kind, string playerId, ContentBinding binding,
            string attemptId, string settlementId, string grantId, string purpose, string factId, string useId)
        {
            Kind = kind;
            PlayerId = playerId;
            Binding = binding;
            AttemptId = attemptId;
            SettlementId = settlementId;
            GrantId = grantId;
            Purpose = purpose;
            FactId = factId;
            UseId = useId;
        }
    }

    // An existing evidence reference is data, never a grant or a trust flag.
    public sealed class CandidateExistingInclusionRef
    {
        public string CheckpointId { get; }
        public string BranchId { get; }
        public string SourceCommitId { get; }
        public BigInteger EvidenceViewRevision { get; }
        public CandidateOriginalGrantRef Grant { get; }
        public int GrantLine { get; }
        public BigInteger UnitStart { get; }
        public BigInteger UnitCount { get; }
        public CandidateInclusionDisposition Disposition { get; }
        public string EndpointOperationId { get; }

        public CandidateExistingInclusionRef(string checkpoint, string branch, string commit, BigInteger revision,
            CandidateOriginalGrantRef grant, int line, BigInteger start, BigInteger count,
            CandidateInclusionDisposition disposition, string endpointOperation)
        {
            CheckpointId = checkpoint;
            BranchId = branch;
            SourceCommitId = commit;
            EvidenceViewRevision = revision;
            Grant = grant;
            GrantLine = line;
            UnitStart = start;
            UnitCount = count;
            Disposition = disposition;
            EndpointOperationId = endpointOperation;
        }
    }

    public sealed class CandidatePermanentSourceLine
    {
        public CandidateOriginalGrantRef Grant { get; }
        public int GrantLine { get; }
        public string ItemId { get; }
        public BigInteger OriginalQuantity { get; }
        public BigInteger AcquisitionOrder { get; }
        public string OriginalOperationId { get; }
        public string OriginalCommitId { get; }
        public string OriginalBranchId { get; }
        public CandidateExistingInclusionRef Inclusion { get; }
        public string RetractionOperationId { get; }

        public CandidatePermanentSourceLine(CandidateOriginalGrantRef grant, int line, string item,
            BigInteger quantity, BigInteger acquired, string operation, string commit, string branch,
            CandidateExistingInclusionRef inclusion, string retractionOperation = null)
        {
            Grant = grant;
            GrantLine = line;
            ItemId = item;
            OriginalQuantity = quantity;
            AcquisitionOrder = acquired;
            OriginalOperationId = operation;
            OriginalCommitId = commit;
            OriginalBranchId = branch;
            Inclusion = inclusion;
            RetractionOperationId = retractionOperation;
        }
    }

    // Exactly one of Source or OutputOperationId identifies the original input.
    public sealed class CandidatePermanentPortion
    {
        public CandidatePermanentSourceLine Source { get; }
        public string OutputOperationId { get; }
        public int OutputLine { get; }
        public string ItemId { get; }
        public BigInteger UnitStart { get; }
        public BigInteger UnitCount { get; }
        public CandidatePermanentEndpoint Endpoint { get; }
        public string EndpointOperationId { get; }

        public CandidatePermanentPortion(CandidatePermanentSourceLine source, string outputOperation,
            int outputLine, string item, BigInteger start, BigInteger count,
            CandidatePermanentEndpoint endpoint = CandidatePermanentEndpoint.Held, string endpointOperation = null)
        {
            Source = source;
            OutputOperationId = outputOperation;
            OutputLine = outputLine;
            ItemId = item;
            UnitStart = start;
            UnitCount = count;
            Endpoint = endpoint;
            EndpointOperationId = endpointOperation;
        }
        internal CandidatePermanentPortion Slice(BigInteger start, BigInteger count)
        {
            return new CandidatePermanentPortion(Source, OutputOperationId, OutputLine, ItemId, start, count, Endpoint, EndpointOperationId);
        }
    }

    public sealed class CandidatePermanentEffect
    {
        public string OperationId { get; }
        public CandidatePermanentQuote Quote { get; }
        public string Outcome { get; }
        public string RelatedOperationId { get; }

        internal CandidatePermanentEffect(string operation, CandidatePermanentQuote quote, string outcome, string related)
        {
            OperationId = operation;
            Quote = quote;
            Outcome = outcome;
            RelatedOperationId = related;
        }
    }

    public sealed class CandidatePermanentInventoryLedger
    {
        public IReadOnlyList<CandidatePermanentSourceLine> Sources { get; }
        public IReadOnlyList<CandidatePermanentEffect> Effects { get; }
        internal CandidatePermanentInventoryLedger(IEnumerable<CandidatePermanentSourceLine> sources,
            IEnumerable<CandidatePermanentEffect> effects)
        {
            Sources = new List<CandidatePermanentSourceLine>(sources).AsReadOnly();
            Effects = new List<CandidatePermanentEffect>(effects).AsReadOnly();
        }
        internal static readonly CandidatePermanentInventoryLedger Empty = new CandidatePermanentInventoryLedger(
            new CandidatePermanentSourceLine[0], new CandidatePermanentEffect[0]);
    }

    public sealed class CandidatePermanentResult
    {
        public string Outcome { get; }
        public string CharacterEffectOperation { get; }
        public string InventoryEffectOperation { get; }
        public string ProgressionEffectOperation { get; }
        public string OriginalLearningOperation { get; }
        internal CandidatePermanentResult(string outcome, string character, string inventory, string progression, string learned)
        {
            Outcome = outcome;
            CharacterEffectOperation = character;
            InventoryEffectOperation = inventory;
            ProgressionEffectOperation = progression;
            OriginalLearningOperation = learned;
        }
    }

    public sealed class CandidatePermanentQuote
    {
        internal CandidatePermanentDraft Draft { get; }
        public CandidatePermanentKind Kind => Draft.Kind;
        public string CharacterId => Draft.CharacterId;
        public string DefinitionId => Draft.DefinitionId;
        public BigInteger? Quantity => Draft.Quantity;
        public bool? Enabled => Draft.Enabled;
        public string StepId => Draft.StepId;
        public string PlayerId { get; }
        public string ClassId { get; }
        public ContentBinding Binding { get; }
        public BigInteger SourceGeneration { get; }
        public ulong SourceDescriptorLength { get; }
        public IReadOnlyList<byte> SourceDescriptorSha256 { get; }
        public BigInteger DefinitionVersion { get; }
        public BigInteger CharacterRevision { get; }
        public BigInteger InventoryRevision { get; }
        public BigInteger PreferenceRevision { get; }
        public BigInteger ProgressionRevision { get; }
        public BigInteger UnitExperience { get; }
        public BigInteger FixedExperience { get; }
        public BigInteger BeforeLevel { get; }
        public BigInteger BeforeExperience { get; }
        public BigInteger FinalLevel { get; }
        public BigInteger FinalExperience { get; }
        public DefinitionBinding TeachingLevel { get; }
        public IReadOnlyList<CandidatePermanentPortion> Inputs { get; }
        public IReadOnlyList<CandidateInventoryQuantity> Costs { get; }
        public IReadOnlyList<CandidateInventoryQuantity> Outputs { get; }
        public string OriginalLearningOperation { get; }

        internal CandidatePermanentQuote(CandidatePermanentDraft draft, string player, string classId, ContentBinding binding,
            BigInteger generation, ulong descriptorLength, IReadOnlyList<byte> descriptorSha256,
            BigInteger version, BigInteger characterRevision, BigInteger inventoryRevision, BigInteger preferenceRevision,
            BigInteger progressionRevision, BigInteger unitExperience, BigInteger xp, BigInteger beforeLevel,
            BigInteger beforeExperience, BigInteger level, BigInteger remainder, DefinitionBinding teaching,
            IReadOnlyList<CandidatePermanentPortion> inputs, IReadOnlyList<CandidateInventoryQuantity> costs,
            IReadOnlyList<CandidateInventoryQuantity> outputs, string originalLearning)
        {
            Draft = new CandidatePermanentDraft
            {
                Kind = draft.Kind,
                CharacterId = draft.CharacterId,
                DefinitionId = draft.DefinitionId,
                Quantity = draft.Quantity,
                Enabled = draft.Enabled,
                PreferenceRevision = draft.PreferenceRevision,
                StepId = draft.StepId,
                SelectedInputs = draft.SelectedInputs == null ? null : new List<CandidatePermanentPortion>(draft.SelectedInputs).AsReadOnly()
            };
            PlayerId = player;
            ClassId = classId;
            Binding = binding;
            SourceGeneration = generation;
            SourceDescriptorLength = descriptorLength;
            SourceDescriptorSha256 = new List<byte>(descriptorSha256).AsReadOnly();
            DefinitionVersion = version;
            CharacterRevision = characterRevision;
            InventoryRevision = inventoryRevision;
            PreferenceRevision = preferenceRevision;
            ProgressionRevision = progressionRevision;
            UnitExperience = unitExperience;
            FixedExperience = xp;
            BeforeLevel = beforeLevel;
            BeforeExperience = beforeExperience;
            FinalLevel = level;
            FinalExperience = remainder;
            TeachingLevel = teaching;
            Inputs = new List<CandidatePermanentPortion>(inputs).AsReadOnly();
            Costs = new List<CandidateInventoryQuantity>(costs).AsReadOnly();
            Outputs = new List<CandidateInventoryQuantity>(outputs).AsReadOnly();
            OriginalLearningOperation = originalLearning;
        }
    }
}
