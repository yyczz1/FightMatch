using System;
using System.Collections.Generic;
using System.Linq;
using FightMatch.Core;

namespace FightMatch.Application
{
    public sealed partial class PlayerNavigationSession
    {
        private readonly PlayerSessionSystem player;
        private readonly CandidateApplicationSystem application;
        private readonly CandidateLifecycleApplicationSystem lifecycle;
        private readonly List<PlayerNavigationRoute> parents = new List<PlayerNavigationRoute>();
        private PlayerNavigationReadResult read;
        private PlayerNavigationRoute route = PlayerNavigationRoute.Gate, anchor = PlayerNavigationRoute.MapAdventure;
        private string level, levelVersion, character, definition, selectedCommit, selectedOperation;
        private CandidatePermanentKind detailKind;
        private long revision, confirmation;
        private bool executing, endConfirmation;
        private PlayerNavigationDraftKind draftKind;
        private uint fromFormat, toFormat;
        private IReadOnlyList<string> slots;
        private PlayerNavigationContext frozenContext;
        private PreparedPlayerPermanentPreview preview;
        private PreparedCandidateLifecycleRequest request;
        private CandidateApplicationCallResult result;
        private CandidateApplicationDiagnostic diagnostic;
        private string status;
        private PlayerNavigationHostRequest host;

        internal PlayerNavigationSession(PlayerSessionSystem player, CandidateApplicationSystem application,
            CandidateLifecycleApplicationSystem lifecycle)
        { this.player = player; this.application = application; this.lifecycle = lifecycle; }

        public PlayerNavigationView Query(SaveCodecBudget budget)
        {
            if (executing) return View("Busy");
            var next = player.QueryNavigation(budget);
            var changed = read == null || !SameHead(read.Head, next.Head) || read.Code != next.Code ||
                read.Application.Phase != next.Application.Phase || read.Application.PendingOperationId != next.Application.PendingOperationId ||
                !read.Application.ObservedCandidateCommitIds.SequenceEqual(next.Application.ObservedCandidateCommitIds);
            var headChanged = read != null && !SameHead(read.Head, next.Head);
            read = next;
            if (changed) revision++;
            if (headChanged && request == null && frozenContext != null)
            { ClearDraft(); route = PlayerNavigationRoute.Detail; status = "StaleContext"; }
            if (read.IsAvailable && character != null && read.Head.Business.Roster.Find(character) == null)
            { character = null; ClearDraft(); revision++; }
            var gate = GateReason();
            if (gate != null && !endConfirmation)
                route = HasRecovery() ? PlayerNavigationRoute.Recovery : PlayerNavigationRoute.Gate;
            else if (gate == null && (route == PlayerNavigationRoute.Gate || route == PlayerNavigationRoute.Recovery) && request == null)
                ReturnToAnchor();
            if (request != null && result?.IsCommitted == true && gate == null) route = PlayerNavigationRoute.CommittedResult;
            return View();
        }
        private PlayerNavigationContext Context() => new PlayerNavigationContext(this, revision, read, route, anchor,
            level, levelVersion, character, parents);
        private PlayerNavigationView View(string overrideStatus = null)
        {
            var permanent = read?.IsAvailable == true && character != null ? player.QueryPermanent(character) : null;
            var context = Context();
            var confirmationView = frozenContext == null && request == null && !endConfirmation ? null :
                new PlayerNavigationConfirmation(draftKind, preview, slots, fromFormat, toFormat, request?.OperationId, endConfirmation);
            return new PlayerNavigationView(read, context, new PlayerNavigationToken(this, revision, confirmation), permanent,
                detailKind, definition, confirmationView, result, diagnostic, overrideStatus ?? status ?? GateReason(),
                selectedCommit, selectedOperation, host, Enum.GetValues(typeof(PlayerNavigationAction)).Cast<PlayerNavigationAction>()
                    .Select(a => new PlayerNavigationOption(a, ActionReason(a))));
        }
        private PlayerNavigationView Refuse(string code, string field = "Navigation")
        { diagnostic = new CandidateApplicationDiagnostic(code, field); status = code; revision++; return View(); }
        private bool Matches(PlayerNavigationContext context) => context != null && ReferenceEquals(context.Owner, this) &&
            context.Revision == revision && SameHead(context.Head, read?.Head);
        private static bool SameHead(CandidateApplicationSnapshot a, CandidateApplicationSnapshot b)
        {
            if (a == null || b == null) return a == b;
            return a.Header.CommitId == b.Header.CommitId && a.Header.SaveGeneration == b.Header.SaveGeneration &&
                a.Descriptor.PlayerId == b.Descriptor.PlayerId && a.Descriptor.Purpose == b.Descriptor.Purpose &&
                a.Descriptor.TotalLength == b.Descriptor.TotalLength && a.Descriptor.Sha256.SequenceEqual(b.Descriptor.Sha256);
        }
        private void ClearDraft() { preview = null; slots = null; frozenContext = null; endConfirmation = false; }

        public PlayerNavigationView Navigate(PlayerNavigationTarget target, PlayerNavigationContext context, SaveCodecBudget budget)
        {
            if (executing) return View("Busy");
            Query(budget);
            if (!Matches(context)) return Refuse("StaleNavigationContext");
            if (target == null) return Refuse("MissingField", "Navigation.Target");
            if (target.Kind >= PlayerNavigationTargetKind.CreationRequired) return RequestHost(target.Kind);
            if (target.Kind == PlayerNavigationTargetKind.EndConfirmation)
            {
                var reason = EndReason(); if (reason != null) return Refuse(reason);
                endConfirmation = true; route = PlayerNavigationRoute.Confirmation; revision++; return View();
            }
            if (target.Kind == PlayerNavigationTargetKind.ObservedCandidate)
            {
                if (read.Application.PendingOperationId != null || !read.Application.ObservedCandidateCommitIds.Contains(target.CommitId))
                    return Refuse("StaleContext", "Recovery.Candidate");
                selectedCommit = target.CommitId; route = PlayerNavigationRoute.Recovery; revision++; return View();
            }
            if (target.Kind == PlayerNavigationTargetKind.OriginalOperation)
            {
                if (!read.Application.IsPublishedHeadVerified || read.Head?.Records.Any(x => x.OperationId == target.OperationId) != true)
                    return Refuse("ResolutionRequired", "Navigation.Operation");
                selectedOperation = target.OperationId; revision++; return View();
            }
            if (request != null) return View("ResolutionRequired");
            if (GateReason() != null) return Refuse(GateReason());
            if (target.Kind == PlayerNavigationTargetKind.SelectCharacter)
            {
                if (read.Head.Business.Roster.Find(target.CharacterId) == null) return Refuse("InconsistentBinding", "CharacterId");
                character = target.CharacterId; ClearDraft(); revision++; return View();
            }
            if (target.Kind == PlayerNavigationTargetKind.Preparation)
            {
                if (target.Binding == null || !target.Binding.Same(read.Binding) ||
                    !read.Levels.Any(x => x.LevelId == target.LevelId && x.LevelVersion == target.LevelVersion))
                    return Refuse("UnsupportedBinding", "Navigation.Level");
                level = target.LevelId; levelVersion = target.LevelVersion;
                parents.Clear(); parents.Add(PlayerNavigationRoute.MapAdventure); anchor = route = PlayerNavigationRoute.Preparation;
            }
            else if (target.Kind == PlayerNavigationTargetKind.MapAdventure)
            { parents.Clear(); route = anchor = PlayerNavigationRoute.MapAdventure; level = levelVersion = null; }
            else if (target.Kind == PlayerNavigationTargetKind.Team || target.Kind == PlayerNavigationTargetKind.Bag)
            {
                parents.Clear();
                if (level != null) { parents.Add(PlayerNavigationRoute.MapAdventure); parents.Add(PlayerNavigationRoute.Preparation); }
                route = target.Kind == PlayerNavigationTargetKind.Team ? PlayerNavigationRoute.Team : PlayerNavigationRoute.Bag;
                anchor = level == null ? route : PlayerNavigationRoute.Preparation;
            }
            else if (target.Kind == PlayerNavigationTargetKind.CraftList)
            {
                if (route != PlayerNavigationRoute.Bag) return Refuse("InvalidNavigationTransition");
                parents.Add(route); route = PlayerNavigationRoute.CraftList;
            }
            else if (target.Kind == PlayerNavigationTargetKind.Detail)
            {
                if (route != PlayerNavigationRoute.CraftList && route != PlayerNavigationRoute.Bag && route != PlayerNavigationRoute.Team)
                    return Refuse("InvalidNavigationTransition");
                if (!Supported(target.PermanentKind)) return Refuse("UnsupportedBinding", "Navigation.PermanentKind");
                if (character == null) return Refuse("ActorSelectionRequired");
                if (target.PermanentKind == CandidatePermanentKind.Craft &&
                    player.QueryPermanent(character).Definitions?.Find(CandidatePermanentDefinitionKind.Recipe, target.DefinitionId) == null)
                    return Refuse("NoPublishedDefinition", "Permanent.Definition");
                parents.Add(route); route = PlayerNavigationRoute.Detail; detailKind = target.PermanentKind; definition = target.DefinitionId;
            }
            else return Refuse("UnsupportedNavigationTarget");
            ClearDraft(); diagnostic = null; status = null; host = null; revision++; return View();
        }

        internal static PlayerPermanentDraft BuildPermanentDraft(PlayerPermanentDraft input,
            PlayerNavigationContext context, SaveCodecBudget budget)
        {
            if (input == null || context == null || budget == null) throw new ArgumentNullException();
            if (input.SelectedInputs != null && input.SelectedInputs.Count > budget.MaxCollectionEntries)
                throw new ArgumentOutOfRangeException(nameof(input.SelectedInputs));
            return new PlayerPermanentDraft { ExpectedCommitId = context.CommitId, Kind = input.Kind,
                CharacterId = input.Kind == CandidatePermanentKind.Craft ? null : input.CharacterId,
                DefinitionId = input.DefinitionId, Quantity = input.Quantity, Enabled = input.Enabled,
                PreferenceRevision = input.PreferenceRevision, StepId = input.StepId,
                SelectedInputs = input.SelectedInputs == null ? null : new List<CandidatePermanentPortion>(input.SelectedInputs).AsReadOnly() };
        }
        private static bool Supported(CandidatePermanentKind kind) => kind == CandidatePermanentKind.Craft ||
            kind == CandidatePermanentKind.Equip || kind == CandidatePermanentKind.SetPreference;

        public PlayerNavigationView Preview(PlayerNavigationDraft draft, PlayerNavigationContext context, SaveCodecBudget budget)
        {
            if (executing) return View("Busy");
            Query(budget);
            if (!Matches(context)) return Refuse("StaleNavigationContext");
            if (request != null || GateReason() != null) return Refuse(GateReason() ?? "ResolutionRequired");
            if (draft == null) return Refuse("MissingField", "Navigation.Draft");
            ClearDraft(); diagnostic = null; status = null;
            if (draft.Kind == PlayerNavigationDraftKind.Formation)
            {
                if (read.Roster.UnavailabilityReason != null) return Refuse(read.Roster.UnavailabilityReason);
                if (draft.Slots == null || draft.Slots.Count != 3) return Refuse("InvalidValue", "Formation.Slots");
                var nonempty = draft.Slots.Where(x => x != null).ToArray();
                if (nonempty.Distinct(StringComparer.Ordinal).Count() != nonempty.Length ||
                    nonempty.Any(x => read.Head.Business.Roster.Find(x) == null)) return Refuse("InconsistentBinding", "Formation.CharacterId");
                slots = new List<string>(draft.Slots).AsReadOnly();
            }
            else if (draft.Kind == PlayerNavigationDraftKind.Permanent)
            {
                var input = draft.Permanent;
                if (input == null || !Supported(input.Kind)) return Refuse("UnsupportedBinding", "Navigation.PermanentKind");
                if (character == null) return Refuse("ActorSelectionRequired");
                if (input.Kind != CandidatePermanentKind.Craft && input.CharacterId != character)
                    return Refuse("StaleContext", "Navigation.TargetCharacter");
                if (input.SelectedInputs != null && input.SelectedInputs.Count > budget.MaxCollectionEntries)
                    return Refuse("Limit", "Permanent.SelectedInputs");
                var actual = BuildPermanentDraft(input, context, budget);
                var prepared = player.PreviewPermanent(actual, budget);
                if (!prepared.IsAccepted) { diagnostic = prepared.Diagnostic; status = prepared.Code; revision++; return View(); }
                preview = prepared.Preview;
            }
            else if (draft.Kind == PlayerNavigationDraftKind.Migration)
            {
                var current = (uint)read.Head.Business.Format;
                if (draft.FromFormat != current || draft.ToFormat != current + 1 || current < 2 || current > 3)
                    return Refuse("UnsupportedSchema", "Navigation.Migration");
                if (current == 2 && read.Head.Business.ActiveHistory != null) return Refuse("ActiveAttemptConflict");
                fromFormat = current; toFormat = draft.ToFormat;
            }
            else return Refuse("UnsupportedNavigationDraft");
            draftKind = draft.Kind; frozenContext = context; confirmation++; revision++;
            route = PlayerNavigationRoute.Confirmation; return View();
        }

        private PlayerNavigationView Confirm(PlayerNavigationToken token, FightMatch.Platform.SaveStoreBudget budget)
        {
            if (request != null) return Receive(token, lifecycle.QueryOperation(request, budget));
            if (frozenContext == null || !SameHead(frozenContext.Head, read.Head)) return Refuse("StaleContext");
            executing = true;
            try
            {
                CandidateLifecyclePrepareResult prepared;
                if (draftKind == PlayerNavigationDraftKind.Permanent) prepared = player.ConfirmPermanent(preview, budget.Codec);
                else if (draftKind == PlayerNavigationDraftKind.Formation)
                    prepared = player.PrepareFormation(new PlayerFormationDraft { ExpectedCommitId = frozenContext.CommitId,
                        ExpectedFormationRevision = frozenContext.Head.Business.Roster.FormationRevision, Slots = slots }, budget.Codec);
                else prepared = fromFormat == 2 ? player.PrepareRosterMigration(frozenContext.CommitId, budget.Codec) :
                    player.PreparePermanentMigration(frozenContext.CommitId, budget.Codec);
                if (!prepared.IsAccepted)
                { ClearDraft(); route = PlayerNavigationRoute.Detail; diagnostic = prepared.Diagnostic; status = prepared.Code; revision++; return View(); }
                if (!AcceptPrepared(token, prepared.Request)) return Refuse("StaleNavigationContext");
                return Receive(token, lifecycle.Submit(request, budget));
            }
            finally { executing = false; }
        }
        internal bool AcceptPrepared(PlayerNavigationToken token, PreparedCandidateLifecycleRequest prepared)
        {
            if (!OriginalToken(token) || prepared == null || request != null || frozenContext == null ||
                prepared.PlayerId != frozenContext.PlayerId || prepared.Intent.ExpectedCommitId != frozenContext.CommitId) return false;
            var kind = draftKind == PlayerNavigationDraftKind.Permanent ? CandidateApplicationKind.PermanentRequest :
                draftKind == PlayerNavigationDraftKind.Formation ? CandidateApplicationKind.SetFormation :
                fromFormat == 2 ? CandidateApplicationKind.MigrateRoster : CandidateApplicationKind.MigratePermanent;
            if (prepared.Kind != kind) return false;
            request = prepared; result = null; route = PlayerNavigationRoute.Recovery; revision++; return true;
        }
        private PlayerNavigationView RequestHost(PlayerNavigationTargetKind kind)
        {
            var app = read.Application; var reason = GateReason();
            if (kind == PlayerNavigationTargetKind.CreationRequired)
            { if (app.Phase != CandidateApplicationPhase.Unconfigured && app.Phase != CandidateApplicationPhase.InitializationReady &&
                app.Phase != CandidateApplicationPhase.CreationConfirmationRequired) return Refuse("InvalidNavigationTransition"); }
            else if (kind == PlayerNavigationTargetKind.SettlementRequired)
            { if (read.Head?.Continuation == null) return Refuse("InvalidPhase"); }
            else
            {
                if (reason != null) return Refuse(reason);
                if (kind == PlayerNavigationTargetKind.BattleSelectionRequested && (level == null || read.Lifecycle.Enter.Reason != null))
                    return Refuse(read.Lifecycle.Enter.Reason ?? "LevelSelectionRequired");
                if (kind == PlayerNavigationTargetKind.ResumeBattleRequested && read.Head.Business.ActiveHistory == null) return Refuse("NoActiveBattle");
            }
            revision++; host = new PlayerNavigationHostRequest(kind, Context(), app); status = kind.ToString(); return View();
        }
    }
}
