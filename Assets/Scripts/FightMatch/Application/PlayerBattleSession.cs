using System;
using System.Collections.Generic;
using System.Linq;
using FightMatch.Core;
using FightMatch.Platform;

namespace FightMatch.Application
{
    public sealed partial class PlayerSessionSystem
    {
        private PlayerBattleSession battleSession;
        public PlayerBattleSession GetBattleSession()
        { CheckGuard(); return battleSession ?? (battleSession = new PlayerBattleSession(this, application, lifecycle)); }
        internal CandidateApplicationCallResult ReadBattleResumedIntent(string commit, string operation,
            out PreparedCandidateApplicationIntent intent) => runtime.QueryResumedIntent(this, commit, operation, out intent);
        internal PlayerDefaultReferenceResult ReadBattleReference(string level, string version, ContentBinding binding)
        {
            try
            {
                var head = Ready(); var publication = For(head.Business.Progression.Definition.Context);
                Need(binding != null && binding.Same(publication.Binding), "UnsupportedBinding", "Reference.Binding");
                var replay = publication.GetDefaultReferences().SingleOrDefault(x =>
                    x.Candidate.Entry.Level.LevelId == level && x.Candidate.Entry.Level.LevelVersion == version);
                Need(replay?.IsAccepted == true, "NoPublishedReference", "Reference.Level");
                var entry = replay.Candidate.Entry; var conditions = new List<string>(); var differences = new List<string>();
                conditions.Add("发布：" + publication.Binding.PackageId + " / " + level + " v" + version);
                foreach (var m in entry.Members)
                    conditions.Add("原槽 " + m.OriginalSlot + "：" + m.CharacterId + " / " + m.ClassId + "，等级 " + m.Level +
                        "，技能 " + (m.LearnedSkills.Count == 0 ? "无" : string.Join(", ", m.LearnedSkills)));
                conditions.Add("携带：" + entry.CarryMode + "；道具关闭；偏好修订 " + replay.ExecutedSteps[0].Conditions.PreferenceRevision);
                conditions.Add("隔离种子：" + string.Join("", replay.SeedBytes.Select(b => b.ToString("x2"))) + "；仅该开局已验证");
                var current = head.Business.ActiveHistory?.CurrentRun.CurrentSnapshot;
                var members = current?.Baseline.Entry.Members;
                var formation = head.Business.Roster.Formation;
                var referenceSlots = entry.Members.OrderBy(m => m.OriginalSlot).Select(m => m.OriginalSlot + ":" + m.CharacterId).ToArray();
                var currentSlots = formation.Select((id, slot) => new { id, slot }).Where(x => x.id != null).Select(x => x.slot + ":" + x.id).ToArray();
                if (!referenceSlots.SequenceEqual(currentSlots)) differences.Add("阵容：当前 " + string.Join(", ", currentSlots) + "；参考 " + string.Join(", ", referenceSlots));
                foreach (var m in entry.Members)
                {
                    var actual = members?.SingleOrDefault(x => x.CharacterId == m.CharacterId);
                    var character = head.Business.Roster.Find(m.CharacterId);
                    var actualLevel = actual?.Level ?? character?.Level;
                    if (actualLevel != m.Level) differences.Add(m.CharacterId + " 等级：当前 " + actualLevel + "；参考 " + m.Level);
                    if (actual != null && !actual.LearnedSkills.SequenceEqual(m.LearnedSkills))
                        differences.Add(m.CharacterId + " 技能：当前 " + string.Join(", ", actual.LearnedSkills) + "；参考 " + string.Join(", ", m.LearnedSkills));
                }
                if (current?.Baseline.Entry.CarryMode != entry.CarryMode || head.Business.Inventory.ActiveCarry?.Rows.Any(x => x.C > 0) == true)
                    differences.Add("携带：当前 " + current?.Baseline.Entry.CarryMode + "；参考 " + entry.CarryMode);
                if (head.Business.Inventory.Loadouts.Any(x => x.Enabled == true) || head.Business.Inventory.PreferenceRevision != replay.ExecutedSteps[0].Conditions.PreferenceRevision)
                    differences.Add("道具偏好：当前修订 " + head.Business.Inventory.PreferenceRevision + "，启用 " + head.Business.Inventory.Loadouts.Count(x => x.Enabled == true) + " 项；参考关闭");
                differences.Add("随机：当前玩家独立随机流不使用参考隔离种子，不保证同样结果");
                if (current == null || current.SceneRevision != 0) differences.Add("局面：当前修订 " + current?.SceneRevision + "；参考从开局开始");
                return new PlayerDefaultReferenceResult(new PlayerDefaultReference(publication.Binding, entry, replay.ExecutedSteps,
                    replay.SeedBytes, conditions, differences));
            }
            catch (Refusal error) { return new PlayerDefaultReferenceResult(null, error.Diagnostic.Code); }
        }
    }

    // One host-owned business session. Views borrow it; hiding a page does not end a request.
    public sealed class PlayerBattleSession
    {
        private readonly PlayerSessionSystem player;
        private readonly CandidateApplicationSystem application;
        private readonly CandidateLifecycleApplicationSystem lifecycle;
        private PlayerNavigationReadResult read;
        private CandidateDemoView battle;
        private PlayerBattleRoute route = PlayerBattleRoute.Gate;
        private PlayerBattleConfirmation confirmation;
        private PreparedCandidateApplicationIntent intent;
        private CandidateApplicationCallResult result;
        private PlayerBattleReceipt receipt;
        private long revision, consumedHostRevision = -1;
        private bool executing;
        private string status;
        public PreparedCandidateLifecycleRequest OriginalRequest { get; private set; }
        internal PlayerBattleSession(PlayerSessionSystem player, CandidateApplicationSystem application, CandidateLifecycleApplicationSystem lifecycle)
        { this.player = player; this.application = application; this.lifecycle = lifecycle; }
        private static bool OwnerRefusal(string code) => code == "WrongThread" || code == "Disposed" || code == "Busy";
        private static bool EndKind(CandidateApplicationKind kind) => kind == CandidateApplicationKind.ExitAttempt ||
            kind == CandidateApplicationKind.RestartAttempt || kind == CandidateApplicationKind.SettleVictory;
        private static bool BattleKind(CandidateApplicationKind kind) => EndKind(kind) || kind == CandidateApplicationKind.EnterAttempt ||
            kind == CandidateApplicationKind.EnterFormation || kind == CandidateApplicationKind.Attack ||
            kind == CandidateApplicationKind.Link || kind == CandidateApplicationKind.Rollback;
        private PlayerBattleContext Context() => new PlayerBattleContext(this, revision, read?.Head);
        private string SettlementReason() => executing ? "Busy" : player.CreationPending ? "CreationPending" :
            read?.IsAvailable != true ? read?.Code ?? "ResolutionRequired" : battle.PresentationToken != null ? "PresentationPending" :
            read.Head.Continuation == null || battle.BattleSnapshot?.Phase != BattlePhase.WonPendingSettlement ? "InvalidPhase" : null;
        private PlayerBattleView View(string code = null) => new PlayerBattleView(Context(), route, read, battle, confirmation,
            intent, result, receipt, code ?? status, OwnerRefusal(code) ? code : SettlementReason());
        public PlayerBattleView Query(SaveCodecBudget budget)
        {
            if (executing) return View("Busy");
            var next = player.QueryNavigation(budget);
            if (OwnerRefusal(next.Code)) return new PlayerBattleView(Context(), route, next, lifecycle.QueryView().Battle,
                confirmation, intent, result, receipt, next.Code, next.Code);
            var nextBattle = lifecycle.QueryView().Battle;
            var changed = read == null || !ReferenceEquals(read.Head, next.Head) || read.Code != next.Code ||
                read.Application.Phase != next.Application.Phase || read.Application.PendingOperationId != next.Application.PendingOperationId ||
                !read.Application.ObservedCandidateCommitIds.SequenceEqual(next.Application.ObservedCandidateCommitIds);
            if (changed) { revision++; if (confirmation != null) { confirmation = null; status = "StaleContext"; } }
            read = next; battle = nextBattle;
            if (player.CreationPending) { route = PlayerBattleRoute.HostNavigation; status = "CreationRequired"; }
            else if (!read.IsAvailable || read.Application.PendingOperationId != null || read.Application.ObservedCandidateCommitIds.Count != 0)
                route = PlayerBattleRoute.Recovery;
            else if (confirmation != null) route = PlayerBattleRoute.Confirmation;
            else if (receipt != null && route == PlayerBattleRoute.Result) { }
            else if (battle.History != null) route = PlayerBattleRoute.Battle;
            else if (route != PlayerBattleRoute.HostNavigation) route = PlayerBattleRoute.Gate;
            return View();
        }
        private bool Matches(PlayerBattleContext context) => context != null && ReferenceEquals(context.Owner, this) &&
            context.Revision == revision && ReferenceEquals(context.Head, read?.Head);
        private string Check(PlayerBattleContext context, SaveCodecBudget budget)
        {
            var current = Query(budget);
            return OwnerRefusal(current.Status) ? current.Status : !Matches(context) ? "StaleContext" :
                player.CreationPending ? "CreationRequired" : null;
        }
        private PlayerBattleView Refuse(string code) => View(code);
        private void ClearOperation() { intent = null; OriginalRequest = null; result = null; receipt = null; confirmation = null; }
        public PlayerBattleView AcceptHost(PlayerNavigationHostRequest host, Func<CandidateTimeSample> clock, SaveStoreBudget budget)
        {
            var current = Query(budget.Codec); if (OwnerRefusal(current.Status)) return current;
            var navigation = player.GetNavigationSession(); var nav = navigation.Query(budget.Codec);
            if (host == null || !ReferenceEquals(host, nav.HostRequest) || !ReferenceEquals(host.Context.Owner, navigation) ||
                host.Context.Revision != nav.Context.Revision || host.Context.Revision <= consumedHostRevision ||
                !ReferenceEquals(host.Context.Head, read.Head) || host.Application.PlayerId != read.Application.PlayerId ||
                (read.Binding != null && host.Context.Binding?.Same(read.Binding) != true)) return Refuse("StaleHostRequest");
            if (host.Kind == PlayerNavigationTargetKind.CreationRequired || host.Kind == PlayerNavigationTargetKind.RootBackRequested)
            { consumedHostRevision = host.Context.Revision; route = PlayerBattleRoute.HostNavigation; status = host.Kind.ToString(); return View(); }
            if (player.CreationPending || !read.IsAvailable || read.Application.PendingOperationId != null ||
                read.Application.ObservedCandidateCommitIds.Count != 0) return Refuse("ResolutionRequired");
            if (host.Kind == PlayerNavigationTargetKind.ResumeBattleRequested || host.Kind == PlayerNavigationTargetKind.SettlementRequired)
            {
                if (battle.History == null || host.Kind == PlayerNavigationTargetKind.SettlementRequired && read.Head.Continuation == null)
                    return Refuse("InvalidPhase");
                consumedHostRevision = host.Context.Revision; ClearOperation(); route = PlayerBattleRoute.Battle; status = null; revision++; return View();
            }
            if (host.Kind != PlayerNavigationTargetKind.BattleSelectionRequested) return Refuse("UnsupportedBattleHostRequest");
            if (battle.History != null || read.Head.Continuation != null) return Refuse("ActiveAttemptConflict");
            if (!read.Levels.Any(x => x.LevelId == host.Context.LevelId && x.LevelVersion == host.Context.LevelVersion)) return Refuse("UnsupportedBinding");
            consumedHostRevision = host.Context.Revision; ClearOperation(); executing = true;
            try
            {
                var business = read.Head.Business;
                var draft = new PlayerFormationEntryDraft { ExpectedCommitId = read.Head.Header.CommitId,
                    LevelId = host.Context.LevelId, LevelVersion = host.Context.LevelVersion,
                    ExpectedFormationRevision = business.Roster.FormationRevision, ExpectedInventoryRevision = business.Inventory.StateRevision,
                    ExpectedProgressionRevision = business.Progression.StateRevision, SelectedCharacters = business.Roster.Formation.Where(x => x != null)
                        .OrderBy(x => x, StringComparer.Ordinal).Select(id => new CandidateCharacterRevisionInput { CharacterId = id,
                            ExpectedRevision = business.Roster.Find(id).StateRevision }).ToArray() };
                Submit(player.PrepareFormationEntry(draft, clock(), budget.Codec), budget);
            }
            finally { executing = false; }
            return Query(budget.Codec);
        }
        private void Submit(CandidateLifecyclePrepareResult prepared, SaveStoreBudget budget)
        {
            confirmation = null; revision++;
            if (!prepared.IsAccepted) { status = prepared.Code; return; }
            OriginalRequest = prepared.Request; intent = prepared.Request.Intent;
            Receive(lifecycle.Submit(OriginalRequest, budget));
        }
        private bool Verified(CandidateApplicationCallResult actual, PreparedCandidateApplicationIntent original)
        {
            var lookup = actual?.OriginalLookup; var record = lookup?.Record; var q = application.QueryView();
            return original != null && BattleKind(original.Kind) && actual.IsCommitted && lookup?.IsFound == true &&
                q.View.IsPublishedHeadVerified && actual.View.IsPublishedHeadVerified &&
                actual.LookupViewCommitId == q.View.PublishedSnapshot?.Header.CommitId &&
                ReferenceEquals(actual.View.PublishedSnapshot, q.View.PublishedSnapshot) && original.PlayerId == q.View.PlayerId &&
                actual.OriginalCommitId == record.CommitId && record.OperationId == original.OperationId &&
                record.Intent.Kind == original.Kind && record.Intent.CanonicalBytes.SequenceEqual(original.CanonicalBytes);
        }
        private bool ReceiptMatches(CandidateApplicationLookup lookup)
        {
            var end = lookup.End; var entry = lookup.Baseline?.Entry;
            return end != null && entry != null && end.Begin.PlayerId == intent.PlayerId && end.Begin.AttemptId == entry.AttemptId &&
                end.Begin.ChallengeId == entry.ChallengeId && end.Begin.EntryBaselineId == entry.EntryBaselineId &&
                lookup.Record.Result.EndReceiptId == end.EndReceiptId && lookup.Record.Result.SettlementId == end.SettlementId &&
                (OriginalRequest == null || OriginalRequest.Input.ExitAttempt == null && OriginalRequest.Input.RestartAttempt == null ||
                    (OriginalRequest.Input.ExitAttempt ?? OriginalRequest.Input.RestartAttempt).AttemptId == entry.AttemptId);
        }
        private void Receive(CandidateApplicationCallResult actual)
        {
            result = actual; status = actual.NotificationFailure?.Code ?? actual.Code; revision++;
            if (!Verified(actual, intent)) { route = PlayerBattleRoute.Recovery; return; }
            if (EndKind(intent.Kind))
            {
                if (!ReceiptMatches(actual.OriginalLookup)) { status = "InconsistentBinding"; route = PlayerBattleRoute.Recovery; return; }
                receipt = new PlayerBattleReceipt(actual.OriginalLookup);
                route = intent.Kind == CandidateApplicationKind.RestartAttempt ? PlayerBattleRoute.Battle : PlayerBattleRoute.Result;
            }
            else { receipt = null; route = PlayerBattleRoute.Battle; }
        }
        public PlayerBattleView PreviewEnd(CandidateApplicationKind kind, PlayerBattleContext context, SaveCodecBudget budget)
        {
            var reason = Check(context, budget); if (reason != null) return Refuse(reason);
            if (kind != CandidateApplicationKind.ExitAttempt && kind != CandidateApplicationKind.RestartAttempt) return Refuse("UnsupportedBinding");
            var life = lifecycle.QueryView(); reason = (kind == CandidateApplicationKind.ExitAttempt ? life.Exit : life.Restart).Reason;
            if (reason != null) return Refuse(reason);
            ClearOperation(); revision++; confirmation = new PlayerBattleConfirmation(kind, Context());
            route = PlayerBattleRoute.Confirmation; status = null; return View();
        }
        public PlayerBattleView Cancel(PlayerBattleConfirmation expected, SaveCodecBudget budget)
        {
            var current = Query(budget); if (OwnerRefusal(current.Status)) return current;
            if (expected == null || !ReferenceEquals(expected, confirmation) || !Matches(expected.Context)) return Refuse("StaleConfirmation");
            confirmation = null; revision++; route = PlayerBattleRoute.Battle; return View();
        }
        public PlayerBattleView Confirm(PlayerBattleConfirmation expected, Func<CandidateTimeSample> clock, SaveStoreBudget budget)
        {
            var current = Query(budget.Codec); if (OwnerRefusal(current.Status)) return current;
            if (expected == null || !ReferenceEquals(expected, confirmation) || !Matches(expected.Context)) return Refuse("StaleConfirmation");
            var life = lifecycle.QueryView(); var reason = (expected.Kind == CandidateApplicationKind.ExitAttempt ? life.Exit : life.Restart).Reason;
            if (reason != null) return Refuse(reason);
            var c = expected.Context; var end = new CandidateApplicationEndInput { AttemptId = c.AttemptId, ChallengeId = c.ChallengeId,
                EntryBaselineId = c.EntryBaselineId, ExpectedSceneRevision = c.SceneRevision };
            confirmation = null; executing = true;
            try { Submit(player.PrepareLifecycle(new PlayerLifecycleDraft { ExpectedCommitId = c.CommitId, Kind = expected.Kind,
                ExitAttempt = expected.Kind == CandidateApplicationKind.ExitAttempt ? end : null,
                RestartAttempt = expected.Kind == CandidateApplicationKind.RestartAttempt ? end : null,
                EndTimeSample = expected.Kind == CandidateApplicationKind.ExitAttempt ? clock() : null }, budget.Codec), budget); }
            finally { executing = false; }
            return Query(budget.Codec);
        }
        public PlayerBattleView Settle(PlayerBattleContext context, Func<CandidateTimeSample> clock, SaveStoreBudget budget)
        {
            var reason = Check(context, budget.Codec) ?? SettlementReason(); if (reason != null) return Refuse(reason);
            ClearOperation(); executing = true;
            try { Submit(player.PrepareVictory(clock(), budget.Codec), budget); }
            finally { executing = false; }
            return Query(budget.Codec);
        }
        public PlayerBattleView Continue(PlayerBattleRecoveryAction action, PreparedCandidateApplicationIntent original,
            PlayerBattleContext context, SaveStoreBudget budget)
        {
            var reason = Check(context, budget.Codec); if (reason != null) return Refuse(reason);
            if (!Enum.IsDefined(typeof(PlayerBattleRecoveryAction), action) || original == null || !ReferenceEquals(original, intent) ||
                !BattleKind(original.Kind) || original.PlayerId != read.Application.PlayerId) return Refuse("OriginalOwnerRequired");
            if (action == PlayerBattleRecoveryAction.End && (read.Head?.Continuation != null || original.Kind == CandidateApplicationKind.SettleVictory))
                return Refuse("SettlementRequired");
            executing = true;
            try
            {
                var actual = application.QueryOperation(original, budget);
                if (actual.OriginalLookup?.IsFound != true && action != PlayerBattleRecoveryAction.Query)
                    actual = action == PlayerBattleRecoveryAction.Retry ? application.Retry(original, budget) :
                        action == PlayerBattleRecoveryAction.Resolve ? application.Resolve(original, budget) : application.End(original, budget);
                if (actual.Code == "Ended" && !actual.IsCommitted) { ClearOperation(); status = actual.Code; route = PlayerBattleRoute.Battle; revision++; }
                else Receive(actual);
            }
            finally { executing = false; }
            return Query(budget.Codec);
        }
        public PlayerBattleView ResumeObserved(string commit, PlayerBattleContext context, SaveStoreBudget budget)
        {
            var reason = Check(context, budget.Codec); if (reason != null) return Refuse(reason);
            if (read.Application.PendingOperationId != null || !read.Application.ObservedCandidateCommitIds.Contains(commit)) return Refuse("StaleCandidate");
            executing = true;
            try
            {
                var actual = application.ResumeObserved(commit, budget);
                if (actual.View.PendingCommitId == commit && actual.View.PendingOperationId != null)
                {
                    var captured = player.ReadBattleResumedIntent(commit, actual.View.PendingOperationId, out var recovered);
                    if (recovered != null && BattleKind(recovered.Kind)) { ClearOperation(); intent = recovered; Receive(actual); }
                    else status = recovered == null ? captured.Code : "OriginalOwnerRequired";
                }
                else if (actual.IsCommitted && actual.View.IsPublishedHeadVerified)
                {
                    var saved = actual.View.PublishedSnapshot.Records.SingleOrDefault(x => x.CommitId == commit);
                    if (saved != null && BattleKind(saved.Intent.Kind)) { ClearOperation(); intent = saved.Intent; Receive(application.QueryOperation(intent, budget)); }
                    else status = "OriginalOwnerRequired";
                }
                else { result = actual; status = actual.Code; }
                revision++;
            }
            finally { executing = false; }
            return Query(budget.Codec);
        }
        public PlayerBattleView OpenOriginalResult(string operation, PlayerBattleContext context, SaveStoreBudget budget)
        {
            var reason = Check(context, budget.Codec); if (reason != null) return Refuse(reason);
            if (read.Head?.Continuation != null) return Refuse("SettlementRequired");
            if (read.Application.PendingOperationId != null || read.Application.ObservedCandidateCommitIds.Count != 0) return Refuse("ResolutionRequired");
            if (!read.Application.IsPublishedHeadVerified) return Refuse("ResolutionRequired");
            var record = read.Head.Records.SingleOrDefault(x => x.OperationId == operation);
            if (record == null || !EndKind(record.Intent.Kind) || record.Intent.PlayerId != context.PlayerId) return Refuse("OriginalResultUnavailable");
            var actual = application.QueryOperation(record.Intent, budget);
            if (!Verified(actual, record.Intent)) return Refuse("OriginalResultUnavailable");
            ClearOperation(); intent = record.Intent; Receive(actual);
            if (receipt != null) route = PlayerBattleRoute.Result;
            return Query(budget.Codec);
        }
        public PlayerBattleView ReturnTo(PlayerNavigationTargetKind target, PlayerBattleContext context, SaveCodecBudget budget)
        {
            var reason = Check(context, budget); if (reason != null) return Refuse(reason);
            if (read.Head?.Continuation != null) return Refuse("SettlementRequired");
            if (receipt == null || route != PlayerBattleRoute.Result || !read.IsAvailable) return Refuse("ResultRequired");
            if (target != PlayerNavigationTargetKind.MapAdventure && target != PlayerNavigationTargetKind.Team && target != PlayerNavigationTargetKind.Bag)
                return Refuse("UnsupportedNavigationTarget");
            var navigation = player.GetNavigationSession(); var nav = navigation.Query(budget);
            nav = navigation.Navigate(new PlayerNavigationTarget { Kind = target }, nav.Context, budget);
            var expected = target == PlayerNavigationTargetKind.MapAdventure ? PlayerNavigationRoute.MapAdventure :
                target == PlayerNavigationTargetKind.Team ? PlayerNavigationRoute.Team : PlayerNavigationRoute.Bag;
            if (nav.Route != expected) return Refuse(nav.Status ?? "ResolutionRequired");
            route = PlayerBattleRoute.HostNavigation; status = target.ToString(); revision++; return View();
        }
        public PlayerBattleView Replay(string level, string version, PlayerBattleContext context, Func<CandidateTimeSample> clock, SaveStoreBudget budget)
        {
            var reason = Check(context, budget.Codec); if (reason != null) return Refuse(reason);
            if (receipt == null || !read.IsAvailable || read.Head.Business.ActiveHistory != null || read.Head.Continuation != null) return Refuse("ResultRequired");
            if (!read.Levels.Any(x => x.LevelId == level && x.LevelVersion == version) ||
                !read.Head.Business.Progression.OpenFacts.Any(x => x.Level.LevelId == level && x.Level.LevelVersion == version)) return Refuse("LevelLocked");
            var navigation = player.GetNavigationSession(); var nav = navigation.Query(budget.Codec);
            nav = navigation.Navigate(new PlayerNavigationTarget { Kind = PlayerNavigationTargetKind.Preparation,
                Binding = read.Binding, LevelId = level, LevelVersion = version }, nav.Context, budget.Codec);
            nav = navigation.Navigate(new PlayerNavigationTarget { Kind = PlayerNavigationTargetKind.BattleSelectionRequested }, nav.Context, budget.Codec);
            return AcceptHost(nav.HostRequest, clock, budget);
        }
        public PlayerDefaultReferenceResult ReadDefaultReference(string level, string version, ContentBinding binding,
            PlayerBattleContext context, SaveCodecBudget budget)
        {
            var reason = Check(context, budget);
            return reason != null ? new PlayerDefaultReferenceResult(null, reason) : player.ReadBattleReference(level, version, binding);
        }
    }
}
