using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using FightMatch.Core;

namespace FightMatch.Application
{
    public sealed class PlayerPermanentDraft
    {
        public string ExpectedCommitId { get; set; }
        public CandidatePermanentKind Kind { get; set; }
        public string CharacterId { get; set; }
        public string DefinitionId { get; set; }
        public BigInteger? Quantity { get; set; }
        public bool? Enabled { get; set; }
        public BigInteger? PreferenceRevision { get; set; }
        public string StepId { get; set; }
        public IReadOnlyList<CandidatePermanentPortion> SelectedInputs { get; set; }

        internal CandidatePermanentDraft Copy()
        {
            return new CandidatePermanentDraft
            {
                Kind = Kind,
                CharacterId = CharacterId,
                DefinitionId = DefinitionId,
                Quantity = Quantity,
                Enabled = Enabled,
                PreferenceRevision = PreferenceRevision,
                StepId = StepId,
                SelectedInputs = SelectedInputs == null ? null : new List<CandidatePermanentPortion>(SelectedInputs).AsReadOnly()
            };
        }
    }

    public sealed class PlayerPermanentView
    {
        public string HeadCommitId { get; }
        public CandidateCharacterState Character { get; }
        public CandidateInventoryState Inventory { get; }
        public CandidateProgressionState Progression { get; }
        public CandidatePermanentDefinitions Definitions { get; }
        public CandidateInventoryView Amounts { get; }
        public IReadOnlyList<CandidatePermanentPortion> SourceChoices { get; }
        public IReadOnlyList<CandidateApplicationRecord> OriginalOperations { get; }
        public IReadOnlyList<CandidatePermanentEffect> CharacterEffects { get; }
        public string UnavailabilityReason { get; }
        public string CardAvailability => Availability(CandidatePermanentDefinitionKind.Card);
        public string SkillAvailability => Availability(CandidatePermanentDefinitionKind.Skill);
        public string RecipeAvailability => Availability(CandidatePermanentDefinitionKind.Recipe);

        internal PlayerPermanentView(CandidateApplicationSnapshot snapshot, string character,
            CandidatePermanentDefinitions definitions, string reason,
            IReadOnlyList<CandidatePermanentPortion> endpoints = null, CandidateInventoryView amounts = null)
        {
            HeadCommitId = snapshot?.Header.CommitId;
            Character = snapshot?.Business.Roster.Find(character);
            Inventory = snapshot?.Business.Inventory;
            Progression = snapshot?.Business.Progression;
            Definitions = definitions;
            Amounts = amounts;
            SourceChoices = endpoints == null ? Array.Empty<CandidatePermanentPortion>() :
                endpoints.Where(x => x.Endpoint == CandidatePermanentEndpoint.Held).ToList().AsReadOnly();
            OriginalOperations = snapshot == null ? Array.Empty<CandidateApplicationRecord>() :
                snapshot.Records.Where(x => x.Intent.Kind == CandidateApplicationKind.PermanentRequest).ToList().AsReadOnly();
            CharacterEffects = snapshot?.Business.Roster.GetPermanentEffects() ?? Array.Empty<CandidatePermanentEffect>();
            UnavailabilityReason = reason;
        }

        private string Availability(CandidatePermanentDefinitionKind kind)
        {
            if (UnavailabilityReason != null) return UnavailabilityReason;
            if (Definitions != null) foreach (var definition in Definitions.Records)
                if (definition.Kind == kind) return "Defined";
            return "NoPublishedDefinition";
        }
    }

    public sealed class PreparedPlayerPermanentPreview
    {
        internal PlayerSessionSystem Owner { get; }
        public string HeadCommitId { get; }
        public CandidatePermanentQuote Quote { get; }
        internal PreparedPlayerPermanentPreview(PlayerSessionSystem owner, string head, CandidatePermanentQuote quote)
        {
            Owner = owner;
            HeadCommitId = head;
            Quote = quote;
        }
    }

    public sealed class PlayerPermanentPreviewResult
    {
        public PreparedPlayerPermanentPreview Preview { get; }
        public CandidateApplicationDiagnostic Diagnostic { get; }
        public bool IsAccepted => Preview != null;
        public string Code => IsAccepted ? "Previewed" : Diagnostic.Code;
        internal PlayerPermanentPreviewResult(PreparedPlayerPermanentPreview preview, CandidateApplicationDiagnostic diagnostic = null)
        {
            Preview = preview;
            Diagnostic = diagnostic;
        }
    }
}
