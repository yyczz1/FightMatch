using System;
using System.Collections.Generic;
using System.Linq;
using FightMatch.Core;

namespace FightMatch.Application
{
    public enum PlayerNavigationRoute
    {
        Gate, MapAdventure, Preparation, Team, Bag, CraftList, Detail, Confirmation,
        Recovery, CommittedResult, ReturnRevalidate
    }
    public enum PlayerNavigationTargetKind
    {
        MapAdventure, Preparation, Team, Bag, CraftList, Detail, SelectCharacter,
        EndConfirmation, ObservedCandidate, OriginalOperation, CreationRequired,
        BattleSelectionRequested, ResumeBattleRequested, SettlementRequired, RootBackRequested
    }
    public enum PlayerNavigationAction
    {
        Back, Cancel, Confirm, Return, Retry, Resolve, ResumeObserved, End, Refresh, SelectOriginalOperation
    }
    public enum PlayerNavigationDraftKind { Formation, Permanent, Migration }

    // This slot only retains original immutable inputs/results; it cannot build or save.
    internal sealed class PlayerNavigationOperation
    {
        internal long Epoch { get; private set; }
        internal CandidateApplicationSnapshot Basis { get; private set; }
        internal CandidateApplicationKind Kind { get; private set; }
        internal CandidatePermanentQuote Quote { get; private set; }
        internal PreparedCandidateLifecycleRequest Request { get; private set; }
        internal PreparedCandidateApplicationIntent Intent { get; private set; }
        internal CandidateApplicationCallResult Result { get; private set; }
        internal bool Consumed { get; private set; }
        internal bool HasVerifiedResult => Result?.IsCommitted == true && Result.OriginalLookup?.IsFound == true &&
            Result.View.IsPublishedHeadVerified && Result.LookupViewCommitId == Result.View.PublishedSnapshot?.Header.CommitId;
        internal void Clear()
        { Epoch++; Basis = null; Quote = null; Request = null; Intent = null; Result = null; Consumed = false; }
        internal void Begin(CandidateApplicationSnapshot basis, CandidateApplicationKind kind, CandidatePermanentQuote quote)
        { Clear(); Basis = basis; Kind = kind; Quote = quote; }
        internal bool TryConsume(long epoch)
        {
            if (epoch != Epoch || Consumed || Basis == null) return false;
            Consumed = true; return true;
        }
        internal bool AcceptPrepared(long epoch, PreparedCandidateLifecycleRequest prepared)
        {
            if (epoch != Epoch || !Consumed || Intent != null || prepared == null || Basis == null ||
                prepared.Kind != Kind || prepared.PlayerId != Basis.Business.PlayerId ||
                prepared.Intent.ExpectedCommitId != Basis.Header.CommitId) return false;
            Request = prepared; Intent = prepared.Intent; return true;
        }
        internal void Adopt(PreparedCandidateApplicationIntent original)
        { Clear(); Intent = original; Kind = original.Kind; Consumed = true; }
        internal bool Receive(long epoch, CandidateApplicationCallResult actual)
        {
            if (epoch != Epoch || Intent == null || actual?.View.PlayerId != Intent.PlayerId ||
                actual.OriginalLookup?.IsFound == true && !actual.OriginalLookup.Record.Intent.CanonicalBytes.SequenceEqual(Intent.CanonicalBytes) ||
                actual.IsCommitted && (actual.OriginalLookup?.IsFound != true ||
                    actual.OriginalCommitId != actual.OriginalLookup.OriginalCommitId ||
                    !actual.View.IsPublishedHeadVerified || actual.LookupViewCommitId != actual.View.PublishedSnapshot?.Header.CommitId)) return false;
            Result = actual; return true;
        }
    }

    public sealed class PlayerNavigationTarget
    {
        public PlayerNavigationTargetKind Kind { get; set; }
        public string LevelId { get; set; }
        public string LevelVersion { get; set; }
        public ContentBinding Binding { get; set; }
        public string CharacterId { get; set; }
        public string DefinitionId { get; set; }
        public CandidatePermanentKind PermanentKind { get; set; }
        public string CommitId { get; set; }
        public string OperationId { get; set; }
    }
    public sealed class PlayerNavigationDraft
    {
        public PlayerNavigationDraftKind Kind { get; set; }
        public IReadOnlyList<string> Slots { get; set; }
        public PlayerPermanentDraft Permanent { get; set; }
        public uint FromFormat { get; set; }
        public uint ToFormat { get; set; }
    }
    public sealed class PlayerNavigationReadResult
    {
        public CandidateApplicationView Application { get; }
        public CandidateApplicationSnapshot Head => Application.PublishedSnapshot;
        public ContentBinding Binding { get; }
        public IReadOnlyList<PreparedLevel> Levels { get; }
        public PlayerRosterView Roster { get; }
        public CandidateInventoryView Inventory { get; }
        public CandidateProgressionView Progression { get; }
        public CandidateLifecycleView Lifecycle { get; }
        public CandidateApplicationDiagnostic Diagnostic { get; }
        public string Code => Diagnostic?.Code ?? "Ready";
        public bool IsAvailable => Diagnostic == null;
        internal PlayerNavigationReadResult(CandidateApplicationView application, ContentBinding binding,
            IEnumerable<PreparedLevel> levels, PlayerRosterView roster, CandidateInventoryView inventory,
            CandidateProgressionView progression, CandidateLifecycleView lifecycle, CandidateApplicationDiagnostic diagnostic)
        {
            Application = application; Binding = binding;
            Levels = new List<PreparedLevel>(levels ?? Array.Empty<PreparedLevel>()).AsReadOnly();
            Roster = roster; Inventory = inventory; Progression = progression; Lifecycle = lifecycle; Diagnostic = diagnostic;
        }
    }
    public sealed class PlayerNavigationContext
    {
        internal PlayerNavigationSession Owner { get; }
        public long Revision { get; }
        public CandidateApplicationSnapshot Head { get; }
        public string PlayerId => Head?.Business.PlayerId;
        public string CommitId => Head?.Header.CommitId;
        public ContentBinding Binding { get; }
        public PlayerNavigationRoute Route { get; }
        public PlayerNavigationRoute ReturnAnchor { get; }
        public string LevelId { get; }
        public string LevelVersion { get; }
        public string SelectedCharacterId { get; }
        public IReadOnlyList<PlayerNavigationRoute> Parents { get; }
        internal PlayerNavigationContext(PlayerNavigationSession owner, long revision, PlayerNavigationReadResult read,
            PlayerNavigationRoute route, PlayerNavigationRoute anchor, string level, string version, string character,
            IEnumerable<PlayerNavigationRoute> parents)
        {
            Owner = owner; Revision = revision; Head = read?.Head; Binding = read?.Binding;
            Route = route; ReturnAnchor = anchor; LevelId = level; LevelVersion = version; SelectedCharacterId = character;
            Parents = new List<PlayerNavigationRoute>(parents).AsReadOnly();
        }
    }
    public sealed class PlayerNavigationToken
    {
        internal PlayerNavigationSession Owner { get; }
        internal long Confirmation { get; }
        public long Revision { get; }
        internal PlayerNavigationToken(PlayerNavigationSession owner, long revision, long confirmation)
        { Owner = owner; Revision = revision; Confirmation = confirmation; }
    }
    public sealed class PlayerNavigationConfirmation
    {
        public PlayerNavigationDraftKind Kind { get; }
        public CandidatePermanentQuote Quote { get; }
        public IReadOnlyList<string> Slots { get; }
        public uint FromFormat { get; }
        public uint ToFormat { get; }
        public string OperationId { get; }
        public bool IsEndConfirmation { get; }
        public CandidateApplicationKind? OriginalKind { get; }
        public CandidatePermanentKind? OriginalPermanentKind { get; }
        public bool OriginalClearsEquipment { get; }
        internal PlayerNavigationConfirmation(PlayerNavigationDraftKind kind, PreparedPlayerPermanentPreview preview,
            IEnumerable<string> slots, uint from, uint to, string operation, bool end,
            CandidateApplicationKind? originalKind, CandidatePermanentKind? originalPermanentKind, bool originalClearsEquipment)
        {
            Kind = kind; Quote = preview?.Quote; Slots = new List<string>(slots ?? Array.Empty<string>()).AsReadOnly();
            FromFormat = from; ToFormat = to; OperationId = operation; IsEndConfirmation = end;
            OriginalKind = originalKind; OriginalPermanentKind = originalPermanentKind; OriginalClearsEquipment = originalClearsEquipment;
        }
    }
    public sealed class PlayerNavigationHostRequest
    {
        public PlayerNavigationTargetKind Kind { get; }
        public PlayerNavigationContext Context { get; }
        public CandidateApplicationView Application { get; }
        internal PlayerNavigationHostRequest(PlayerNavigationTargetKind kind, PlayerNavigationContext context,
            CandidateApplicationView application)
        { Kind = kind; Context = context; Application = application; }
    }
    public sealed class PlayerNavigationOption
    {
        public PlayerNavigationAction Action { get; }
        public string Reason { get; }
        public bool IsAvailable => Reason == null;
        internal PlayerNavigationOption(PlayerNavigationAction action, string reason) { Action = action; Reason = reason; }
    }
    public sealed class PlayerNavigationView
    {
        public PlayerNavigationReadResult Read { get; }
        public PlayerNavigationContext Context { get; }
        public PlayerNavigationToken Token { get; }
        public PlayerNavigationRoute Route => Context.Route;
        public PlayerPermanentView Permanent { get; }
        public CandidatePermanentKind DetailKind { get; }
        public string DefinitionId { get; }
        public PlayerNavigationConfirmation Confirmation { get; }
        public CandidateApplicationCallResult Result { get; }
        public CandidateApplicationDiagnostic Diagnostic { get; }
        public string Status { get; }
        public string SelectedCommitId { get; }
        public string SelectedOperationId { get; }
        public PlayerNavigationHostRequest HostRequest { get; }
        public IReadOnlyList<PlayerNavigationOption> Actions { get; }
        public string ReasonFor(PlayerNavigationAction action) => Actions.FirstOrDefault(x => x.Action == action)?.Reason ??
            (Actions.Any(x => x.Action == action) ? null : "UnsupportedNavigationAction");
        internal PlayerNavigationView(PlayerNavigationReadResult read, PlayerNavigationContext context, PlayerNavigationToken token,
            PlayerPermanentView permanent, CandidatePermanentKind detailKind, string definition, PlayerNavigationConfirmation confirmation,
            CandidateApplicationCallResult result, CandidateApplicationDiagnostic diagnostic, string status,
            string commit, string operation, PlayerNavigationHostRequest host, IEnumerable<PlayerNavigationOption> actions)
        {
            Read = read; Context = context; Token = token; Permanent = permanent; DetailKind = detailKind; DefinitionId = definition;
            Confirmation = confirmation; Result = result; Diagnostic = diagnostic; Status = status;
            SelectedCommitId = commit; SelectedOperationId = operation; HostRequest = host;
            Actions = new List<PlayerNavigationOption>(actions).AsReadOnly();
        }
    }
}
