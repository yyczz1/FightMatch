using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FightMatch.Application;
using FightMatch.Core;
using UnityEngine;
using UnityEngine.Events;

namespace FightMatch.Presentation
{
    public sealed class PlayerBattleView : MonoBehaviour, IDisposable
    {
        [SerializeField] private GameObject battleContent, resultRoot;
        [SerializeField] private CandidateBattlePlaybackView playbackView;
        [SerializeField] private PlayerDefaultReferenceView referenceView;
        [SerializeField] private LocalizedTmpText headline, hud;
        [SerializeField] private RectTransform actions, history, receipt, battleNotices;
        [SerializeField] private GameObject normalHudRoot, historyDrawerRoot, dialogRoot, recoveryRoot;
        [SerializeField] private LocalizedTmpText historyHeader, dialogTitle, recoveryTitle;
        [SerializeField] private UnityEngine.UI.Button historyOpenButton, historyCloseButton;
        [SerializeField] private RectTransform dialogBody, dialogChoices, dialogActions, dialogDangerActions;
        [SerializeField] private RectTransform recoveryBody, recoveryChoices, recoveryActions, recoveryDangerActions;
        private enum BottomHudMode { Normal, History, Reference }
        private BottomHudMode bottomHudMode;
        private long referenceEpoch;
        [SerializeField] private UnityEngine.UI.Button buttonTemplate;
        [SerializeField] private LocalizedTmpText textTemplate;
        private readonly List<GameObject> rows = new List<GameObject>();
        private readonly List<KeyValuePair<UnityEngine.UI.Button, UnityAction>> listeners = new List<KeyValuePair<UnityEngine.UI.Button, UnityAction>>();
        private LocalizationService localization;
        private BattleLocalizedRows hudRows;
        private long renderEpoch;
        private bool disposed;
        internal bool IsDisposed => disposed;
        public PlayerBattleController Controller { get; private set; }
        public PlayerBattlePageToken Page { get; private set; }
        public CandidateBattlePlaybackView PlaybackView => playbackView;
        public PlayerDefaultReferenceView ReferenceView => referenceView;
        public CandidateRollbackRange DisplayedRange { get; private set; }
        public BattleSnapshot RollbackBeforeSnapshot => DisplayedRange?.BeforeSnapshot;

        internal void Bind(PlayerBattleController controller, LocalizationService service)
        {
            if (disposed) throw new ObjectDisposedException(nameof(PlayerBattleView));
            if (controller == null || service == null) throw new ArgumentNullException();
            Unbind();
            if (battleContent == null || resultRoot == null || playbackView == null || referenceView == null || headline == null || hud == null || actions == null || history == null ||
                dialogRoot == null || recoveryRoot == null || receipt == null || buttonTemplate == null || textTemplate == null ||
                normalHudRoot == null || historyDrawerRoot == null || historyHeader == null || historyOpenButton == null || historyCloseButton == null ||
                dialogTitle == null || recoveryTitle == null || battleNotices == null || dialogBody == null || dialogChoices == null || dialogActions == null || dialogDangerActions == null ||
                recoveryBody == null || recoveryChoices == null || recoveryActions == null || recoveryDangerActions == null)
                throw new InvalidOperationException("PlayerBattleView serialized bindings are incomplete.");
            buttonTemplate.gameObject.SetActive(false); textTemplate.gameObject.SetActive(false);
            Controller = controller; localization = service; Page = controller.BindPage();
            playbackView.Bind(controller.Input, controller.Playback, service); referenceView.Bind(service);
            hudRows = new BattleLocalizedRows(hud, "battle-hud"); localization.LocaleChanged += OnLocaleChanged;
            controller.Changed += Render; Render();
        }
        public void Detach() { Unbind(); }
        public void Unbind() => Unbind(PointerCancellationCause.Cancelled);
        internal void Unbind(PointerCancellationCause cause)
        {
            DetachSelfManaged();
            if (!ReferenceEquals(referenceView, null))
            {
                if (referenceView != null) referenceView.Unbind();
                else referenceView.DetachManaged();
            }
            if (!ReferenceEquals(playbackView, null))
            {
                if (playbackView != null) playbackView.Unbind(cause);
                else playbackView.DetachManaged();
            }
            if (this != null) hudRows?.Clear(); hudRows = null;
            ClearRows();
            if (dialogRoot != null) dialogRoot.SetActive(false);
            if (recoveryRoot != null) recoveryRoot.SetActive(false);
            if (historyHeader != null) historyHeader.Unbind();
            if (dialogTitle != null) dialogTitle.Unbind();
            if (recoveryTitle != null) recoveryTitle.Unbind();
            ResetBottomHud();
            if (headline != null) BattleText.Hide(headline);
            if (hud != null) hud.Unbind();
            foreach (var button in new[] { historyOpenButton, historyCloseButton })
                if (button != null) foreach (var text in button.GetComponentsInChildren<LocalizedTmpText>(true)) text.Unbind();
        }
        private void DetachSelfManaged()
        {
            renderEpoch++;
            if (localization != null) localization.LocaleChanged -= OnLocaleChanged;
            var owner = Controller; var page = Page;
            if (owner != null) owner.Changed -= Render;
            Controller = null; Page = null; DisplayedRange = null; localization = null;
            if (!ReferenceEquals(referenceView, null)) referenceView.SuspendCallbacks();
            if (!ReferenceEquals(playbackView, null)) playbackView.SuspendCallbacks();
            owner?.DetachPage(page);
        }
        internal void DetachManaged()
        {
            DetachSelfManaged();
            if (!ReferenceEquals(referenceView, null)) referenceView.DetachManaged();
            if (!ReferenceEquals(playbackView, null)) playbackView.DetachManaged();
            listeners.Clear(); rows.Clear(); hudRows = null;
        }
        private bool OwnContainer(Transform parent)
        { return parent == actions || parent == history || parent == battleNotices || parent == receipt ||
            parent == dialogBody || parent == dialogChoices || parent == dialogActions || parent == dialogDangerActions ||
            parent == recoveryBody || parent == recoveryChoices || parent == recoveryActions || parent == recoveryDangerActions; }
        private void ResetBottomHud()
        {
            bottomHudMode = BottomHudMode.Normal;
            if (historyDrawerRoot != null) historyDrawerRoot.SetActive(false);
            if (normalHudRoot != null) normalHudRoot.SetActive(true);
        }
        private void SetBottomHud(BottomHudMode mode)
        {
            if (mode != BottomHudMode.Reference && referenceView != null && referenceView.IsOpen) referenceView.Close();
            bottomHudMode = mode;
            normalHudRoot.SetActive(mode == BottomHudMode.Normal);
            historyDrawerRoot.SetActive(mode == BottomHudMode.History);
        }
        private void FixedButton(UnityEngine.UI.Button button, string key, Action action, bool enabled = true)
        {
            var owner = Controller; var page = Page; var epoch = renderEpoch;
            button.GetComponentInChildren<LocalizedTmpText>(true).Bind(localization, key);
            UnityAction callback = () => {
                if (this != null && button != null && button.isActiveAndEnabled && button.interactable &&
                    ReferenceEquals(owner, Controller) && epoch == renderEpoch && owner.Owns(page)) action();
            };
            button.onClick.AddListener(callback); listeners.Add(new KeyValuePair<UnityEngine.UI.Button, UnityAction>(button, callback));
            button.interactable = enabled;
        }
        private void ShowReference(PlayerBattleController owner, PlayerBattlePageToken page, PlayerDefaultReference reference)
        {
            referenceView.Bind(localization, () => {
                if (this != null && ReferenceEquals(owner, Controller) && ReferenceEquals(page, Page) &&
                    referenceEpoch == renderEpoch && owner.Owns(page) && bottomHudMode == BottomHudMode.Reference)
                    SetBottomHud(BottomHudMode.Normal);
            });
            referenceEpoch = renderEpoch;
            referenceView.Show(owner, page, playbackView.InputView.Board, reference);
            if (referenceView.IsOpen) SetBottomHud(BottomHudMode.Reference);
        }
        private void ClearRows()
        {
            foreach (var listener in listeners) if (listener.Key != null) listener.Key.onClick.RemoveListener(listener.Value);
            listeners.Clear();
            foreach (var row in rows)
            {
                if (row == null) continue;
                foreach (var text in row.GetComponentsInChildren<LocalizedTmpText>(true)) text.Unbind();
                if (!OwnContainer(row.transform.parent)) continue;
                row.SetActive(false); row.transform.SetParent(null, false);
                if (UnityEngine.Application.isPlaying) Destroy(row); else DestroyImmediate(row);
            }
            rows.Clear();
        }
        private UnityEngine.UI.Button Button(RectTransform parent, string id, string key,
            Action<PlayerBattleController, PlayerBattlePageToken> action, bool enabled = true, params KeyValuePair<string, string>[] arguments)
        {
            var owner = Controller; var page = Page; var epoch = renderEpoch;
            var button = Instantiate(buttonTemplate, parent, false); button.gameObject.SetActive(false);
            button.GetComponent<FightMatchViewId>().Assign(id);
            if (parent == actions)
            {
                var width = id == Row("battle-exit") ? 152 : id == Row("battle-restart") ? 256 :
                    id == "fm.action.victory.settle" ? 248 : 152;
                var cell = button.GetComponent<UnityEngine.UI.LayoutElement>();
                if (cell == null) cell = button.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();
                cell.minWidth = cell.preferredWidth = width; cell.minHeight = cell.preferredHeight = 48;
            }
            button.GetComponentInChildren<LocalizedTmpText>(true).Bind(localization, key, arguments);
            UnityAction callback = () =>
            { if (this != null && button != null && button.isActiveAndEnabled && button.interactable && ReferenceEquals(owner, Controller) && epoch == renderEpoch && owner.Owns(page)) action(owner, page); };
            button.onClick.AddListener(callback); listeners.Add(new KeyValuePair<UnityEngine.UI.Button, UnityAction>(button, callback));
            rows.Add(button.gameObject); button.interactable = enabled; button.gameObject.SetActive(true); return button;
        }
        private void Text(RectTransform parent, string id, string key, params KeyValuePair<string, string>[] arguments)
        {
            var text = Instantiate(textTemplate, parent, false); text.gameObject.SetActive(false);
            text.GetComponent<FightMatchViewId>().Assign(id); text.Bind(localization, key, arguments);
            rows.Add(text.gameObject); text.gameObject.SetActive(true);
        }
        private void Text(RectTransform parent, string id, BattleTextLine line) { Text(parent, id, line.Key, line.Arguments); }
        private void OnLocaleChanged(LocaleId locale) { Render(); }
        private static string Row(string id) => FightMatchViewId.Row(id);
        private void Render()
        {
            if (Controller == null) return;
            if (!Controller.Owns(Page)) { Unbind(); return; }
            renderEpoch++; var view = Controller.View; var context = view.Context; var battle = view.Battle;
            var pendingVictory = battle.BattleSnapshot?.Phase == BattlePhase.WonPendingSettlement;
            if (pendingVictory) new BattleTextLine("fm.victory.pending.title").Bind(headline, localization);
            else if (view.Route == PlayerBattleRoute.Result) ResultTitle(view.Receipt, false).Bind(headline, localization);
            else if (view.Route == PlayerBattleRoute.Recovery) BattleText.Hide(headline);
            else if (battle.BattleSnapshot != null) BattleText.Phase(localization, battle.BattleSnapshot.Phase).Bind(headline, localization);
            else BattleText.Hide(headline);
            RenderHud(view); ClearRows();
            dialogRoot.SetActive(false); recoveryRoot.SetActive(false);
            BattleText.Hide(dialogTitle); BattleText.Hide(recoveryTitle);
            var supported = playbackView.InputView.DisplayLayoutSupported;
            if (view.Route != PlayerBattleRoute.Battle && view.Route != PlayerBattleRoute.Confirmation || !supported)
            { if (referenceView.IsOpen) referenceView.Close(); SetBottomHud(BottomHudMode.Normal); }
            else if (referenceView.IsOpen) referenceEpoch = renderEpoch;
            historyHeader.Bind(localization, "fm.history.title");
            FixedButton(historyOpenButton, "fm.history.title", () => SetBottomHud(BottomHudMode.History),
                supported && Controller.HistoryEntries.Count != 0);
            FixedButton(historyCloseButton, "fm.common.action.close", () => SetBottomHud(BottomHudMode.Normal));
            historyOpenButton.gameObject.SetActive(battle.History != null);
            battleContent.SetActive(view.Route != PlayerBattleRoute.Result && view.Route != PlayerBattleRoute.HostNavigation);
            resultRoot.SetActive(view.Route == PlayerBattleRoute.Result);
            if (battle.History != null)
            {
                Button(actions, Row("battle-exit"), "fm.operation.exit_attempt",
                    (c, p) => c.PreviewEnd(p, CandidateApplicationKind.ExitAttempt, context), view.Read.Lifecycle?.Exit.IsAvailable == true);
                Button(actions, Row("battle-restart"), "fm.operation.restart_attempt",
                    (c, p) => c.PreviewEnd(p, CandidateApplicationKind.RestartAttempt, context), supported && view.Read.Lifecycle?.Restart.IsAvailable == true);
                Button(actions, "fm.action.victory.settle", "fm.victory.settle.button", (c, p) => c.Settle(p, context), supported && view.CanSettle);
                if (pendingVictory && !view.CanSettle)
                {
                    if (battle.PresentationToken != null) Text(battleNotices, Row("settlement-pending"), "fm.victory.playback_pending");
                    else
                    {
                        var reason = BattleText.Reason(BattleTextDomain.Settlement, view.SettlementReason).Resolve(localization);
                        Text(battleNotices, Row("settlement-pending"), "fm.victory.settle.unavailable", BattleText.Arg("reason", reason));
                    }
                }
                Button(actions, "fm.action.reference.open", "fm.reference.play_button", (c, p) =>
                {
                    var entry = c.View.Battle.BattleSnapshot.Baseline.Entry;
                    var reference = c.ReadReference(p, entry.Level.LevelId, entry.Level.LevelVersion, c.View.Context.Binding);
                    if (reference?.IsAvailable == true) ShowReference(c, p, reference.Value);
                    else BattleText.Reason(BattleTextDomain.Reference, reference?.Code ?? "StaleReference").Bind(headline, localization);
                }, supported);
                var actionNumber = 0;
                if (Controller.HistoryEntries.Count != 0)
                {
                    if (!battle.Rollback.IsAvailable && (battle.Phase == CandidateApplicationPhase.PendingPreparation ||
                        battle.Phase == CandidateApplicationPhase.SaveFailed || battle.Phase == CandidateApplicationPhase.CommitUnknown))
                        Text(history, Row("history-blocked-by-save"), "fm.history.undo.blocked_by_save");
                }
                foreach (var entry in Controller.HistoryEntries)
                {
                    var anchor = entry.HistoryAnchorId;
                    var label = BattleText.History(localization, entry.Record, ++actionNumber);
                    Button(history, Row("history:" + anchor), label.Key, (c, p) => c.SelectHistory(p, anchor, context), supported && battle.Rollback.IsAvailable, label.Arguments);
                }
            }
            DisplayedRange = Controller.Input.RollbackPreview;
            if (DisplayedRange != null)
            {
                var range = DisplayedRange;
                new BattleTextLine("fm.history.title").Bind(dialogTitle, localization);
                Text(dialogBody, Row("rollback-range"), "fm.history.undo_preview",
                    new KeyValuePair<string, string>("actionCount", range.Entries.Count.ToString(CultureInfo.InvariantCulture)));
                Text(dialogBody, Row("rollback-before"), "fm.history.snapshot.title");
                var before = range.BeforeSnapshot;
                foreach (var member in before.Members) Text(dialogBody, Row("rollback-member:" + member.Member.CharacterId),
                    BattleText.MemberHp(localization, member.Member.CharacterId, member.Hp, member.Member.Stats.MaxHp));
                foreach (var enemy in before.Enemies) Text(dialogBody, Row("rollback-enemy:" + enemy.PairKey.PairId),
                    BattleText.EnemyHp(localization, enemy.Enemy, enemy.Hp, enemy.Enemy.Stats.MaxHp));
                Text(dialogBody, Row("rollback-phase"), BattleText.Phase(localization, before.Phase));
                Text(dialogBody, Row("rollback-routes"), "fm.history.snapshot.routes", BattleText.Arg("lockedRouteCount", BattleText.Number(before.Board.LockedRoutes.Count)));
                if (before.Board.PendingLinks.Count != 0) Text(dialogBody, Row("rollback-pending-links"), "fm.history.snapshot.pending_links",
                    BattleText.Arg("pendingLinkCount", BattleText.Number(before.Board.PendingLinks.Count)));
                var removedNumber = 0;
                foreach (var removed in range.Entries) Text(dialogBody, Row("rollback-removed:" + removed.HistoryAnchorId),
                    BattleText.History(localization, removed.Record, ++removedNumber));
                Button(dialogDangerActions, Row("confirm-history"), "fm.history.undo.confirm_button", (c, p) => c.ConfirmHistory(p, range));
                Button(dialogActions, Row("cancel-history"), "fm.history.undo.keep_button", (c, p) => c.CancelHistory(p, range));
            }
            if (view.Confirmation != null)
            {
                var frozen = view.Confirmation; var exiting = frozen.Kind == CandidateApplicationKind.ExitAttempt;
                new BattleTextLine(exiting ? "fm.battle.exit.preview_title" : "fm.battle.restart.preview_title").Bind(dialogTitle, localization);
                Text(dialogBody, Row("battle-end-preview"), exiting ? "fm.battle.exit.preview_body" : "fm.battle.restart.preview_body");
                Button(dialogDangerActions, Row("confirm-battle-end"), exiting ? "fm.battle.exit.confirm_button" : "fm.battle.restart.confirm_button", (c, p) => c.Confirm(p, frozen));
                Button(dialogActions, Row("cancel-battle-end"), "fm.common.action.cancel", (c, p) => c.Cancel(p, frozen));
            }
            if (view.Route == PlayerBattleRoute.Recovery)
            {
                new BattleTextLine("fm.save_recovery.title").Bind(recoveryTitle, localization);
                Text(recoveryBody, Row("battle-recovery-state"), BattleText.Save(view.Read.Application.Phase, battle.Code) ??
                    new BattleTextLine("fm.save_recovery.blocking_notice"));
                var pendingKind = view.OriginalIntent != null && view.Read.Application.PendingOperationId == view.OriginalIntent.OperationId ?
                    (CandidateApplicationKind?)view.OriginalIntent.Kind : Controller.Input.LastRequest != null &&
                    view.Read.Application.PendingOperationId == Controller.Input.LastRequest.OperationId ? Controller.Input.LastRequest.Kind : (CandidateApplicationKind?)null;
                if (pendingKind.HasValue) Text(recoveryBody, Row("battle-recovery-operation"), "fm.save_recovery.operation_summary",
                    BattleText.Arg("operationName", BattleText.Operation(localization, pendingKind.Value)));
                if (view.OriginalIntent != null && view.Read.Application.PendingOperationId == view.OriginalIntent.OperationId)
                {
                    var original = view.OriginalIntent;
                    Button(recoveryActions, Row("query-battle-original"), "fm.save_recovery.lookup_button", (c, p) => c.Continue(p, PlayerBattleRecoveryAction.Query, original, context));
                    Button(recoveryActions, Row("retry-battle-original"), "fm.save_recovery.retry_button", (c, p) => c.Continue(p, PlayerBattleRecoveryAction.Retry, original, context), view.CanRetry);
                    Button(recoveryActions, Row("resolve-battle-original"), "fm.save_recovery.confirm_result_button", (c, p) => c.Continue(p, PlayerBattleRecoveryAction.Resolve, original, context), view.CanResolve);
                    Button(recoveryDangerActions, Row("end-battle-original"), "fm.save_recovery.end_uncommitted_button", (c, p) => c.Continue(p, PlayerBattleRecoveryAction.End, original, context),
                        view.Read.Head?.Continuation == null && original.Kind != CandidateApplicationKind.SettleVictory);
                }
                var candidateNumber = 0;
                foreach (var commit in view.Read.Application.ObservedCandidateCommitIds.OrderBy(x => x, StringComparer.Ordinal))
                {
                    var candidate = commit;
                    Button(recoveryChoices, Row("resume:" + candidate), "fm.save_recovery.candidate_row", (c, p) => c.ResumeObserved(p, candidate, context), true,
                        BattleText.Arg("candidateNumber", BattleText.Number(++candidateNumber)));
                }
                var inputRequest = Controller.Input.LastRequest;
                if (inputRequest != null && view.Read.Application.PendingOperationId == inputRequest.OperationId)
                {
                    Button(recoveryActions, Row("query-input-original"), "fm.save_recovery.lookup_button", (c, p) =>
                    { if (ReferenceEquals(inputRequest, c.Input.LastRequest)) c.Input.QueryLastOperation(); });
                    Button(recoveryDangerActions, Row("end-input-original"), "fm.save_recovery.end_uncommitted_button", (c, p) =>
                    { if (ReferenceEquals(inputRequest, c.Input.LastRequest)) c.Input.EndLast(); }, view.Read.Head?.Continuation == null);
                }
            }
            var notification = BattleText.Notification(Controller.Input.LastResult?.Application) ?? BattleText.Notification(view.Result);
            if (notification != null) Text(view.Route == PlayerBattleRoute.Recovery ? recoveryBody : battleNotices, Row("battle-notification"), notification);
            if (view.Route == PlayerBattleRoute.Result && view.Receipt != null)
            {
                var original = view.Receipt;
                Text(receipt, Row("receipt-title"), ResultTitle(original, true));
                Text(receipt, Row("receipt-level"), "fm.result.level", BattleText.Arg("levelName", BattleText.Level(localization, original.LevelId)));
                if (original.Reward == null) Text(receipt, Row("receipt-no-reward"), "fm.result.no_reward.body",
                    BattleText.Arg("levelName", BattleText.Level(localization, original.LevelId)));
                foreach (var member in original.Members)
                {
                    if (member.Experience != null) Text(receipt, Row("receipt-member:" + member.CharacterId), "fm.result.member_xp",
                        BattleText.Arg("characterName", BattleText.Character(localization, member.CharacterId)), BattleText.Arg("amount", BattleText.Number(member.Experience.Amount)));
                    else Text(receipt, Row("receipt-member:" + member.CharacterId), member.CharacterId == "W" ? "fm.name.character.w" : string.Empty);
                    Text(receipt, Row("receipt-slot:" + member.CharacterId), "fm.result.member.original_slot",
                        BattleText.Arg("characterName", BattleText.Character(localization, member.CharacterId)),
                        BattleText.Arg("slotNumber", BattleText.Number(member.OriginalSlot + 1)));
                    if (member.End?.WasDown == true)
                    {
                        var period = context.Head?.Business.Roster.Find(member.CharacterId)?.RecoveryPeriods.SingleOrDefault(x => x.RecoveryId == member.End.RecoveryId);
                        // The receipt reports the recovery opened by this original end, never a later HUD countdown.
                        var status = period == null ? null : new BattleTextLine("fm.result.recovery_started",
                            BattleText.Arg("duration", BattleText.Duration(period.Duration))).Resolve(localization);
                        Text(receipt, Row("receipt-fallen:" + member.CharacterId), "fm.result.member_fallen",
                            BattleText.Arg("characterName", BattleText.Character(localization, member.CharacterId)), BattleText.Arg("recoveryStatus", status));
                    }
                }
                if (original.Reward != null)
                {
                    Text(receipt, Row("receipt-inventory"), "fm.result.base_reward_section");
                    foreach (var item in original.InventoryGrant.Items) Text(receipt, Row("receipt-item:" + item.ItemId), "fm.result.item_reward",
                        BattleText.Arg("itemName", BattleText.Item(localization, item.ItemId)), BattleText.Arg("amount", BattleText.Number(item.Quantity)));
                    // Only the fixed L1 publication supplies this accepted product fact. New publications need their reward configuration contract.
                    if (original.LevelId == "level:ch01-01" && context.Binding?.ContentFingerprint ==
                        "b2247d3f951626edfdf25753520f3421dc20e8d8cab7731c9ab3ba6ece1a5129")
                        Text(receipt, Row("receipt-no-first-clear"), "fm.result.no_first_clear_bonus");
                }
                Button(receipt, "fm.action.result.map", "fm.result.map_button", (c, p) => c.ReturnTo(p, PlayerNavigationTargetKind.MapAdventure, context));
                Button(receipt, "fm.action.result.party", "fm.result.party_button", (c, p) => c.ReturnTo(p, PlayerNavigationTargetKind.Team, context));
                Button(receipt, "fm.action.result.inventory", "fm.result.inventory_button", (c, p) => c.ReturnTo(p, PlayerNavigationTargetKind.Bag, context));
                Button(receipt, "fm.action.result.replay", "fm.result.replay_button", (c, p) => c.Replay(p, original.LevelId, original.LevelVersion, context));
                if (view.NextLevels.Count == 0) Text(receipt, Row("no-next-level"), "fm.map.no_next_level");
                foreach (var level in view.NextLevels)
                {
                    var next = level;
                    // WAITING_FOR_COPY_KEYS: next-level action label/name.
                    Button(receipt, Row("next:" + next.LevelId), string.Empty, (c, p) => c.Replay(p, next.LevelId, next.LevelVersion, context));
                }
            }
            dialogRoot.SetActive(DisplayedRange != null || view.Confirmation != null);
            recoveryRoot.SetActive(view.Route == PlayerBattleRoute.Recovery);
        }
        private BattleTextLine ResultTitle(PlayerBattleReceipt original, bool receiptTitle)
        {
            if (original == null) return BattleText.Unknown();
            if (original.Reward != null) return new BattleTextLine(receiptTitle ? "fm.result.already_settled" : "fm.result.title");
            var kind = original.Lookup.Record.Intent.Kind;
            return new BattleTextLine("fm.result.no_reward.title", BattleText.Arg("operationName",
                kind == CandidateApplicationKind.ExitAttempt || kind == CandidateApplicationKind.RestartAttempt ? BattleText.Operation(localization, kind) : null));
        }
        private void RenderHud(FightMatch.Application.PlayerBattleView view)
        {
            var lines = new List<BattleTextLine>(); var state = view.Battle.BattleSnapshot;
            if (state != null) lines.Add(new BattleTextLine("fm.battle.hud.phase", BattleText.Arg("currentPhase", BattleText.Number(state.CurrentFaceIndex + 1)),
                BattleText.Arg("totalPhases", BattleText.Number(state.Baseline.Entry.Level.Faces.Count))));
            var head = view.Context.Head;
            if (head != null)
            {
                foreach (var member in head.Business.Roster.Characters)
                {
                    lines.Add(new BattleTextLine("fm.party.member.level", BattleText.Arg("characterName", BattleText.Character(localization, member.CharacterId)),
                        BattleText.Arg("level", BattleText.Number(member.Level))).For("member:" + member.CharacterId + ":level"));
                    var offset = member.Level - System.Numerics.BigInteger.One; var definition = member.Definition;
                    var required = definition.XpBase + offset * (definition.XpLinear + definition.XpQuadratic * offset);
                    lines.Add(new BattleTextLine("fm.party.member.experience", BattleText.Arg("currentXp", BattleText.Number(member.Experience)),
                        BattleText.Arg("requiredXp", BattleText.Number(required))).For("member:" + member.CharacterId + ":xp"));
                    var period = member.ActiveRecovery;
                    if (!member.IsReady || period != null) lines.Add(new BattleTextLine("fm.party.member.recovering",
                        BattleText.Arg("remainingTime", Remaining(period))).For("member:" + member.CharacterId + ":recovery"));
                }
                foreach (var item in head.Business.Inventory.Holdings) lines.Add(new BattleTextLine("fm.inventory.item.row",
                    BattleText.Arg("itemName", BattleText.Item(localization, item.ItemId)), BattleText.Arg("amount", BattleText.Number(item.T))).For("item:" + item.ItemId));
            }
            hudRows.Bind(localization, lines);
        }
        private static string Remaining(CandidateRecoveryPeriod period)
        {
            if (period?.Duration == null || period.Elapsed == null || period.Elapsed.Numerator.Sign < 0) return null;
            try { return BattleText.Duration(period.Duration.Subtract(period.Elapsed, new ExactMathBudget())); }
            catch (ExactMathLimitException) { return null; }
            catch (ArithmeticException) { return null; }
        }
        public void ClosePresentation() { if (Controller?.Owns(Page) == true) Controller.SkipPlayback(Page); Unbind(); }
        private void OnDisable() { Unbind(PointerCancellationCause.FocusLost); }
        private void OnDestroy() { Dispose(); }
        public void Dispose() { if (disposed) return; Unbind(); disposed = true; }
    }
}
