using System.Collections.Generic;
using System.Numerics;
using FightMatch.Core;
using FightMatch.Content;

namespace FightMatch.Application
{
    public sealed class PlayerFormationDraft
    {
        public string ExpectedCommitId { get; set; }
        public BigInteger? ExpectedFormationRevision { get; set; }
        public IReadOnlyList<string> Slots { get; set; }
    }
    public sealed class PlayerFormationEntryDraft
    {
        public string ExpectedCommitId { get; set; }
        public string LevelId { get; set; }
        public string LevelVersion { get; set; }
        public BigInteger? ExpectedFormationRevision { get; set; }
        public IReadOnlyList<CandidateCharacterRevisionInput> SelectedCharacters { get; set; }
        public BigInteger? ExpectedInventoryRevision { get; set; }
        public BigInteger? ExpectedProgressionRevision { get; set; }
    }
    public sealed class PlayerRosterView
    {
        public CandidateBusinessFormat? Format { get; }
        public bool IsMaterialized => Format == CandidateBusinessFormat.PublishedRosterV3 || Format == CandidateBusinessFormat.PublishedPermanentV4;
        public IReadOnlyList<CandidateCharacterState> Characters { get; }
        public IReadOnlyList<string> Slots { get; }
        public BigInteger? FormationRevision { get; }
        public string UnavailabilityReason { get; }
        internal PlayerRosterView(CandidateBusinessSnapshot business, string reason)
        {
            Format = business?.Format; Characters = business?.Roster.Characters ?? new List<CandidateCharacterState>().AsReadOnly();
            Slots = business?.Roster.Formation ?? new List<string> { null, null, null }.AsReadOnly();
            FormationRevision = business?.Roster.FormationRevision; UnavailabilityReason = reason;
        }
    }

    public sealed class PlayerLifecycleDraft
    {
        public string ExpectedCommitId { get; set; }
        public CandidateApplicationKind? Kind { get; set; }
        public CandidateApplicationEnterInput EnterAttempt { get; set; }
        public CandidateApplicationEndInput ExitAttempt { get; set; }
        public CandidateApplicationEndInput RestartAttempt { get; set; }
        public CandidateApplicationRecoveryInput AdvanceRecovery { get; set; }
        public CandidateTimeSample EndTimeSample { get; set; }
    }
    public sealed class PreparedPlayerProfile
    {
        public PlayerProfileCreateRecord CreateRecord { get; }
        public string PlayerId => CreateRecord.PlayerId;
        public string OperationId => CreateRecord.OperationId;
        public PreparedCandidateApplicationIntent Intent => Request.Intent;
        internal PublishedContentCatalog Catalog { get; }
        internal PreparedCandidateLifecycleRequest Request { get; }
        internal PreparedPlayerProfile(PlayerProfileCreateRecord record, PublishedContentCatalog catalog, PreparedCandidateLifecycleRequest request)
        { CreateRecord = record; Catalog = catalog; Request = request; }
    }
    public sealed class PreparedPlayerProfileResult
    {
        public bool IsAccepted => Request != null;
        public PreparedPlayerProfile Request { get; }
        public CandidateApplicationDiagnostic Diagnostic { get; }
        public string Code => IsAccepted ? "Prepared" : Diagnostic.Code;
        internal PreparedPlayerProfileResult(PreparedPlayerProfile request, CandidateApplicationDiagnostic diagnostic = null)
        { Request = request; Diagnostic = diagnostic; }
    }
}
