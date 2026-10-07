using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using FightMatch.Core;
using FightMatch.Application;
using FightMatch.Platform;
using UnityEngine;
using UnityEngine.Events;

namespace FightMatch.Presentation
{
    public sealed class CandidateBoardInputView : MonoBehaviour
    {
        [SerializeField] private CandidateBoardElement board;
        [SerializeField] private LocalizedTmpText phase, save, availability;
        [SerializeField] private RectTransform[] allyStageSlots, enemyStageSlots, memberSlots;
        [SerializeField] private LocalizedTmpText[] allyNames, allyHp, allyIntent, enemyNames, enemyHp, enemyIntent;
        [SerializeField] private UnityEngine.UI.Button memberTemplate, retry, resolve;
        private readonly Dictionary<string, UnityEngine.UI.Button> buttons = new Dictionary<string, UnityEngine.UI.Button>(StringComparer.Ordinal);
        private readonly Dictionary<string, UnityAction> memberActions = new Dictionary<string, UnityAction>(StringComparer.Ordinal);
        private UnityAction retryAction, resolveAction;
        private LocalizationService localization;
        private BattleLocalizedRows availabilityRows;
        private bool borrowed, ticking, structuralRefresh, nativeBound;
        private double refreshMilliseconds;
        private long bindingEpoch;
        public CandidateBoardInputController Controller { get; private set; }
        public CandidateBoardElement Board => board;
        internal bool DisplayLayoutSupported => SlotProblem() == null;

        private string SlotProblem()
        {
            var state = Controller?.View.BattleSnapshot;
            if (state == null) return null;
            var allies = state.Members.Select(x => x.Member.OriginalSlot).ToArray();
            var enemies = state.Enemies.Select(x => x.Enemy.OriginalSlot).ToArray();
            if (allies.Length == 0 || allies.Any(x => x < 0 || x > 2) || allies.Distinct().Count() != allies.Length ||
                enemies.Any(x => x < 0) || enemies.Distinct().Count() != enemies.Length) return "InvalidOriginalSlot";
            return enemies.Length > 3 || enemies.Any(x => x > 2) ? "UnsupportedEnemySlotLayout" : null;
        }
        private static bool Three<T>(T[] values) where T : UnityEngine.Object =>
            values != null && values.Length == 3 && values.All(x => x != null) && values.Distinct().Count() == 3;
        internal void SetStageSlotPresentation(bool enemy, int originalSlot, BattleTextLine name, BattleTextLine hp, BattleTextLine intent)
        {
            if (localization == null || !DisplayLayoutSupported || originalSlot < 0 || originalSlot > 2) return;
            var names = enemy ? enemyNames : allyNames;
            var health = enemy ? enemyHp : allyHp;
            var intentions = enemy ? enemyIntent : allyIntent;
            BindSlot(names[originalSlot], name); BindSlot(health[originalSlot], hp); BindSlot(intentions[originalSlot], intent);
        }
        private void BindSlot(LocalizedTmpText label, BattleTextLine line)
        {
            if (label == null) return;
            if (line == null) BattleText.Hide(label); else line.Bind(label, localization);
        }
        internal void ClearStageSlotPresentation()
        {
            foreach (var labels in new[] { allyNames, allyHp, allyIntent, enemyNames, enemyHp, enemyIntent })
                if (labels != null) foreach (var label in labels) if (label != null) BattleText.Hide(label);
        }

        internal void Attach(CandidateBattleApplicationSystem system, SaveStoreBudget budget, LocalizationService service)
        {
            Unbind();
            Bind(new CandidateBoardInputController(system, budget), service);
            borrowed = false;
        }

        internal void Bind(CandidateBoardInputController controller, LocalizationService service)
        {
            if (controller == null || service == null) throw new ArgumentNullException();
            Unbind(); borrowed = true; Controller = controller; localization = service;
            structuralRefresh = isActiveAndEnabled;
            Subscribe();
            if (structuralRefresh) { BindNative(); Render(); }
        }
        private void Subscribe()
        {
            localization.LocaleChanged -= OnLocaleChanged; localization.LocaleChanged += OnLocaleChanged;
            Controller.Changed -= Render; Controller.Changed += Render;
            ticking = structuralRefresh; refreshMilliseconds = 0;
        }
        private void BindNative()
        {
            if (nativeBound) return;
            var controller = Controller; var service = localization;
            if (board == null || phase == null || save == null || availability == null ||
                !Three(allyStageSlots) || !Three(enemyStageSlots) || !Three(memberSlots) ||
                !Three(allyNames) || !Three(allyHp) || !Three(allyIntent) || !Three(enemyNames) || !Three(enemyHp) || !Three(enemyIntent) ||
                memberTemplate == null || retry == null || resolve == null)
                throw new InvalidOperationException("CandidateBoardInputView serialized bindings are incomplete.");
            memberTemplate.gameObject.SetActive(false);
            board.Bind(controller); board.raycastTarget = true;
            var owner = controller; var epoch = bindingEpoch;
            retryAction = () => { if (ReferenceEquals(owner, Controller) && epoch == bindingEpoch && DisplayLayoutSupported) owner.RetryLast(); };
            resolveAction = () => { if (ReferenceEquals(owner, Controller) && epoch == bindingEpoch && DisplayLayoutSupported) owner.ResolveLast(); };
            retry.onClick.AddListener(retryAction); resolve.onClick.AddListener(resolveAction);
            retry.GetComponentInChildren<LocalizedTmpText>(true).Bind(service, "fm.save_recovery.retry_button");
            resolve.GetComponentInChildren<LocalizedTmpText>(true).Bind(service, "fm.save_recovery.confirm_result_button");
            availabilityRows = new BattleLocalizedRows(availability, "board-availability");
            nativeBound = true;
        }

        private void Update()
        {
            if (!ticking || Controller == null) return;
            refreshMilliseconds += Time.unscaledDeltaTime * 1000d;
            if (refreshMilliseconds < 100) return;
            refreshMilliseconds %= 100; Controller.Refresh();
        }

        public void Close() => Close(PointerCancellationCause.Cancelled);
        internal void Close(PointerCancellationCause cause)
        {
            if (borrowed) { Unbind(cause); return; }
            var owner = Controller;
            Unbind(cause);
            // Standalone callers may still inspect the closed controller; it owns no view callbacks.
            Controller = owner;
            if (board != null) board.raycastTarget = false;
            if (retry != null) retry.interactable = false;
            if (resolve != null) resolve.interactable = false;
        }
        public void Detach() { Unbind(); }
        public void Unbind() => Unbind(PointerCancellationCause.Cancelled);
        internal void Unbind(PointerCancellationCause cause)
        {
            var owner = Controller; var ownsInput = !borrowed;
            var oldRetry = retryAction; var oldResolve = resolveAction;
            var oldButtons = buttons.Select(x => new KeyValuePair<UnityEngine.UI.Button, UnityAction>(x.Value, memberActions[x.Key])).ToArray();
            var oldAvailability = availabilityRows;
            DetachManaged();
            if (this != null) { oldAvailability?.Clear(); ClearStageSlotPresentation(); }
            if (board != null) board.Unbind(cause);
            if (ownsInput) { owner?.CancelGesture(cause); owner?.CancelRollback(); }
            if (retry != null && oldRetry != null) retry.onClick.RemoveListener(oldRetry);
            if (resolve != null && oldResolve != null) resolve.onClick.RemoveListener(oldResolve);
            if (retry != null) foreach (var text in retry.GetComponentsInChildren<LocalizedTmpText>(true)) text.Unbind();
            if (resolve != null) foreach (var text in resolve.GetComponentsInChildren<LocalizedTmpText>(true)) text.Unbind();
            foreach (var listener in oldButtons)
            {
                var button = listener.Key; if (button == null) continue;
                button.onClick.RemoveListener(listener.Value);
                foreach (var text in button.GetComponentsInChildren<LocalizedTmpText>(true)) text.Unbind();
                if (memberSlots == null || !memberSlots.Any(slot => slot != null && button.transform.parent == slot)) continue;
                button.gameObject.SetActive(false); button.transform.SetParent(null, false);
                if (UnityEngine.Application.isPlaying) Destroy(button.gameObject); else DestroyImmediate(button.gameObject);
            }
            foreach (var text in new[] { phase, save, availability }) if (text != null) text.Unbind();
            owner?.ClearVisibleFeedback();
        }
        internal void SuspendCallbacks()
        {
            ticking = false; refreshMilliseconds = 0;
            if (localization != null) localization.LocaleChanged -= OnLocaleChanged;
            if (Controller != null) Controller.Changed -= Render;
        }
        internal void DetachManaged()
        {
            SuspendCallbacks(); bindingEpoch++;
            Controller = null; localization = null; nativeBound = false;
            retryAction = null; resolveAction = null; memberActions.Clear(); buttons.Clear();
            availabilityRows = null;
        }
        private void RemoveMember(string id)
        {
            var button = buttons[id]; button.onClick.RemoveListener(memberActions[id]);
            foreach (var text in button.GetComponentsInChildren<LocalizedTmpText>(true)) text.Unbind();
            buttons.Remove(id); memberActions.Remove(id);
            if (memberSlots == null || !memberSlots.Any(slot => slot != null && button.transform.parent == slot)) return;
            button.gameObject.SetActive(false); button.transform.SetParent(null, false);
            if (UnityEngine.Application.isPlaying) Destroy(button.gameObject); else DestroyImmediate(button.gameObject);
        }
        private void Render()
        {
            if (!structuralRefresh || !nativeBound || !ticking || Controller == null || localization == null) return;
            var view = Controller.View; var state = view.BattleSnapshot;
            if (state == null) BattleText.Hide(phase);
            else BattleText.Phase(localization, state.Phase).Bind(phase, localization);
            var problem = SlotProblem();
            board.raycastTarget = problem == null;
            if (problem != null) ClearStageSlotPresentation();
            var saveLine = BattleText.Save(view.Phase, view.Code);
            if (saveLine == null) BattleText.Hide(save); else saveLine.Bind(save, localization);
            var currentMembers = state?.Members;
            foreach (var id in buttons.Keys.ToArray())
                if (currentMembers == null || !currentMembers.Any(x => x.Member.CharacterId == id)) RemoveMember(id);
            if (currentMembers != null) foreach (var member in currentMembers)
            {
                var id = member.Member.CharacterId;
                var slot = member.Member.OriginalSlot;
                if (slot < 0 || slot >= memberSlots.Length) continue;
                if (buttons.TryGetValue(id, out var previous) && previous.transform.parent != memberSlots[slot]) RemoveMember(id);
                if (!buttons.TryGetValue(id, out var button))
                {
                    button = Instantiate(memberTemplate, memberSlots[slot], false); button.gameObject.SetActive(false);
                    var rect = (RectTransform)button.transform;
                    rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero;
                    button.GetComponent<FightMatchViewId>().Assign(FightMatchViewId.Member(id));
                    var owner = Controller; var epoch = bindingEpoch;
                    UnityAction action = () => { if (ReferenceEquals(owner, Controller) && epoch == bindingEpoch && DisplayLayoutSupported) owner.SelectMember(id); };
                    button.onClick.AddListener(action); buttons.Add(id, button); memberActions.Add(id, action);
                }
                BattleText.MemberHp(localization, id, member.Hp, member.Member.Stats.MaxHp)
                    .Bind(button.GetComponentInChildren<LocalizedTmpText>(true), localization);
                button.interactable = problem == null && member.Hp.Numerator.Sign > 0;
                button.gameObject.SetActive(true);
            }
            var feedback = new List<BattleTextLine>();
            if (problem != null)
            {
                feedback.Add(problem == "UnsupportedEnemySlotLayout" ? new BattleTextLine("fm.entry.binding_unsupported") :
                    new BattleTextLine("fm.diagnostic.missing_binding", BattleText.Arg("errorCode", problem)));
                availabilityRows.Bind(localization, feedback);
                retry.interactable = resolve.interactable = false;
                return;
            }
            if (Controller.SelectedCharacterId != null) feedback.Add(new BattleTextLine("fm.battle.member.selected",
                BattleText.Arg("characterName", BattleText.Character(localization, Controller.SelectedCharacterId))));
            if (view.PresentationToken != null) feedback.Add(new BattleTextLine("fm.battle.gesture.playback_locked"));
            else if (view.Phase == CandidateApplicationPhase.PendingPreparation || view.Phase == CandidateApplicationPhase.SaveFailed ||
                view.Phase == CandidateApplicationPhase.CommitUnknown) feedback.Add(new BattleTextLine("fm.save_recovery.blocking_notice"));
            else if (state == null) { }
            else if (state?.Phase == BattlePhase.WonPendingSettlement) feedback.Add(new BattleTextLine("fm.victory.pending.title"));
            else if (view.Link.IsAvailable) feedback.Add(new BattleTextLine("fm.battle.gesture.instruction"));
            else if (state.Phase == BattlePhase.AwaitAction && Controller.SelectedCharacterId == null) feedback.Add(new BattleTextLine("fm.battle.member.select_prompt"));
            else if (view.AttackFor(Controller.SelectedCharacterId).IsAvailable) feedback.Add(new BattleTextLine("fm.battle.gesture.instruction"));
            else
            {
                feedback.Add(BattleText.Reason(BattleTextDomain.Input, view.AttackFor(Controller.SelectedCharacterId).Reason));
            }
            if (Controller.VisibleFeedback.HasValue) feedback.Add(BattleText.Feedback(Controller.VisibleFeedback.Value)
                .For("board-visible-feedback:" + Controller.VisibleFeedbackRevision.ToString(CultureInfo.InvariantCulture)));
            availabilityRows.Bind(localization, feedback);
            retry.interactable = Controller.CanRetry; resolve.interactable = Controller.CanResolve;
        }
        private void OnLocaleChanged(LocaleId locale) { Render(); }
        private void OnEnable()
        {
            structuralRefresh = true;
            if (Controller != null && localization != null) { Subscribe(); BindNative(); Render(); }
        }
        private void OnDisable()
        {
            structuralRefresh = false; SuspendCallbacks();
            if (board != null) board.CancelPointer(PointerCancellationCause.FocusLost);
        }
        private void OnDestroy() { structuralRefresh = false; Unbind(); }
    }

    internal sealed class BattleTextLine
    {
        internal readonly string Key, Identity;
        internal readonly KeyValuePair<string, string>[] Arguments;
        internal BattleTextLine(string key, params KeyValuePair<string, string>[] arguments)
            : this(key, key, arguments) { }
        private BattleTextLine(string key, string identity, KeyValuePair<string, string>[] arguments)
        { Key = key; Identity = identity; Arguments = arguments; }
        internal BattleTextLine For(string identity) => new BattleTextLine(Key, identity, Arguments);
        internal string Resolve(LocalizationService service)
        { var result = service.Resolve(Key, Arguments); return result.IsSuccess ? result.Text : null; }
        internal void Bind(LocalizedTmpText target, LocalizationService service)
        { target.Bind(service, Key, Arguments); target.gameObject.SetActive(true); }
    }

    // Each displayed row owns a localization binding. Extra rows are cloned only while the source is inactive.
    internal sealed class BattleLocalizedRows
    {
        private readonly LocalizedTmpText first;
        private readonly string prefix;
        private readonly Dictionary<string, LocalizedTmpText> extra = new Dictionary<string, LocalizedTmpText>(StringComparer.Ordinal);
        internal BattleLocalizedRows(LocalizedTmpText first, string prefix) { this.first = first; this.prefix = prefix; }
        internal void Bind(LocalizationService service, IReadOnlyList<BattleTextLine> lines)
        {
            var identities = new HashSet<string>(StringComparer.Ordinal);
            if (lines.Any(line => string.IsNullOrEmpty(line.Identity) || !identities.Add(line.Identity)))
            {
                Clear(); BattleText.Unknown().Bind(first, service); return;
            }
            var needed = new HashSet<string>(lines.Skip(1).Select(line => line.Identity), StringComparer.Ordinal);
            foreach (var id in extra.Keys.ToArray()) if (!needed.Contains(id)) Remove(id);
            if (lines.Count == 0) { BattleText.Hide(first); return; }
            for (var i = 1; i < lines.Count; i++)
            {
                var line = lines[i];
                if (!extra.TryGetValue(line.Identity, out var row))
                {
                    first.gameObject.SetActive(false);
                    row = UnityEngine.Object.Instantiate(first, first.transform.parent, false);
                    row.GetComponent<FightMatchViewId>().Assign(FightMatchViewId.Row(prefix + ":" + line.Identity));
                    extra.Add(line.Identity, row);
                }
                row.transform.SetSiblingIndex(first.transform.GetSiblingIndex() + i);
                line.Bind(row, service);
            }
            lines[0].Bind(first, service);
        }
        private void Remove(string identity)
        {
            var row = extra[identity]; extra.Remove(identity);
            if (row == null) return;
            row.Unbind(); row.gameObject.SetActive(false); row.transform.SetParent(null, false);
            if (UnityEngine.Application.isPlaying) UnityEngine.Object.Destroy(row.gameObject); else UnityEngine.Object.DestroyImmediate(row.gameObject);
        }
        internal void Clear() { foreach (var id in extra.Keys.ToArray()) Remove(id); if (first != null) first.Unbind(); }
    }

    internal enum BattleTextDomain { Input, Save, Settlement, Reference, Playback, Notification }

    internal static class BattleText
    {
        internal static KeyValuePair<string, string> Arg(string name, string value) => new KeyValuePair<string, string>(name, value);
        internal static string Number(int value) => value.ToString(CultureInfo.InvariantCulture);
        internal static string Number(System.Numerics.BigInteger value) => value.ToString(CultureInfo.InvariantCulture);
        internal static string Hp(ExactRational value) => value.Denominator.IsOne ? Number(value.Numerator) : Number(value.Numerator) + "/" + Number(value.Denominator);
        internal static string Duration(ExactRational milliseconds)
        {
            if (milliseconds == null || milliseconds.Numerator.Sign < 0 || milliseconds.Denominator.Sign <= 0) return null;
            try
            {
                var budget = new ExactMathBudget();
                var seconds = milliseconds.Divide(ExactRational.Create(1000, 1, budget), budget).Ceil(budget);
                return Number(seconds / 60) + ":" + (seconds % 60).ToString("D2", CultureInfo.InvariantCulture);
            }
            catch (ExactMathLimitException) { return null; }
            catch (ArithmeticException) { return null; }
        }
        internal static BattleTextLine Unknown() => new BattleTextLine(string.Empty);
        internal static void Hide(LocalizedTmpText text) { text.Unbind(); text.gameObject.SetActive(false); }
        internal static string Resolve(LocalizationService service, string key) => new BattleTextLine(key).Resolve(service);
        // COPY FIX-01 maps published IDs explicitly. Unpublished/synthetic IDs remain binding diagnostics.
        internal static string Character(LocalizationService service, string id) => Resolve(service, id == "W" ? "fm.name.character.w" : string.Empty);
        internal static string Item(LocalizationService service, string id) => Resolve(service,
            id == "item:tin" ? "fm.name.item.tin" : id == "item:wood" ? "fm.name.item.wood" : string.Empty);
        internal static string Level(LocalizationService service, string id) => Resolve(service,
            id == "level:ch01-01" ? "fm.name.level.ch01_01" : string.Empty);
        internal static BattleTextLine EnemyName(LocalizationService service, PreparedEnemy enemy) => new BattleTextLine(
            "fm.battle.enemy.numbered_name", Arg("enemyName", Resolve(service, enemy?.EnemyDefinitionId == "enemy:clockwork-infantry" ?
                "fm.name.enemy.clockwork_infantry" : string.Empty)), Arg("enemyNumber", enemy == null ? null : Number(enemy.OriginalSlot + 1))).For(enemy == null ? null : EnemyIdentity(enemy) + ":name");
        internal static string EnemyIdentity(PreparedEnemy enemy) => enemy == null ? null : "enemy:" + enemy.EnemyDefinitionId + ":" + Number(enemy.OriginalSlot);
        internal static string Enemy(LocalizationService service, PreparedEnemy enemy) => EnemyName(service, enemy).Resolve(service);
        internal static string Character(LocalizationService service, BattleSnapshot snapshot, BattleCombatantKey key) =>
            Character(service, snapshot?.Members.FirstOrDefault(x => x.CombatantKey.Equals(key))?.Member.CharacterId);
        internal static string Enemy(LocalizationService service, BattleSnapshot snapshot, BattlePairKey pair) =>
            Enemy(service, snapshot?.Enemies.FirstOrDefault(x => x.PairKey.Equals(pair))?.Enemy);
        internal static string PhaseName(LocalizationService service, BattlePhase? phase)
        {
            string key;
            switch (phase)
            {
                case BattlePhase.AwaitAction: key = "fm.battle.phase.await_action"; break;
                case BattlePhase.AwaitLinks: key = "fm.battle.phase.await_links"; break;
                case BattlePhase.AwaitRescue: key = "fm.battle.phase.await_rescue"; break;
                case BattlePhase.WonPendingSettlement: key = "fm.battle.phase.won_pending_settlement"; break;
                case BattlePhase.Closed: key = "fm.battle.phase.closed"; break;
                default: key = string.Empty; break;
            }
            return Resolve(service, key);
        }
        internal static BattleTextLine Phase(LocalizationService service, BattlePhase? phase) =>
            new BattleTextLine("fm.battle.phase.label", Arg("phaseName", PhaseName(service, phase)));
        internal static string Operation(LocalizationService service, CandidateApplicationKind kind)
        {
            string key;
            switch (kind)
            {
                case CandidateApplicationKind.InitializeProfile: key = "fm.operation.initialize_profile"; break;
                case CandidateApplicationKind.EnterAttempt: key = "fm.operation.enter_attempt"; break;
                case CandidateApplicationKind.EnterFormation: key = "fm.operation.enter_formation"; break;
                case CandidateApplicationKind.Attack: key = "fm.operation.attack"; break;
                case CandidateApplicationKind.Link: key = "fm.operation.link"; break;
                case CandidateApplicationKind.Rollback: key = "fm.operation.rollback"; break;
                case CandidateApplicationKind.SettleVictory: key = "fm.operation.settle_victory"; break;
                case CandidateApplicationKind.ExitAttempt: key = "fm.operation.exit_attempt"; break;
                case CandidateApplicationKind.RestartAttempt: key = "fm.operation.restart_attempt"; break;
                case CandidateApplicationKind.SetFormation: key = "fm.operation.set_formation"; break;
                case CandidateApplicationKind.AdvanceRecovery: key = "fm.operation.advance_recovery"; break;
                case CandidateApplicationKind.MigrateRoster:
                case CandidateApplicationKind.MigratePermanent: key = "fm.profile.data_upgrade.title"; break;
                default: key = string.Empty; break;
            }
            return Resolve(service, key);
        }
        internal static BattleTextLine MemberHp(LocalizationService service, string id, ExactRational hp, ExactRational max) =>
            new BattleTextLine("fm.battle.hud.member_hp", Arg("characterName", Character(service, id)), Arg("currentHp", Hp(hp)), Arg("maxHp", Hp(max))).For("member:" + id + ":hp");
        internal static BattleTextLine EnemyHp(LocalizationService service, PreparedEnemy enemy, ExactRational hp, ExactRational max) =>
            new BattleTextLine("fm.battle.hud.enemy_hp", Arg("enemyName", Enemy(service, enemy)), Arg("currentHp", Hp(hp)), Arg("maxHp", Hp(max))).For(enemy == null ? null : EnemyIdentity(enemy) + ":hp");
        internal static BattleTextLine Save(CandidateApplicationPhase phase, string code)
        {
            if (phase == CandidateApplicationPhase.Ready || phase == CandidateApplicationPhase.Unconfigured ||
                phase == CandidateApplicationPhase.InitializationReady || phase == CandidateApplicationPhase.CreationConfirmationRequired ||
                phase == CandidateApplicationPhase.Disposed) return null;
            if (phase == CandidateApplicationPhase.SaveFailed) return Reason(BattleTextDomain.Save, "SaveFailed");
            if (phase == CandidateApplicationPhase.CommitUnknown) return new BattleTextLine("fm.save_recovery.unknown");
            if (phase == CandidateApplicationPhase.PendingPreparation) return new BattleTextLine("fm.save_recovery.blocking_notice");
            return Reason(BattleTextDomain.Save, code);
        }
        internal static BattleTextLine Feedback(CandidateBoardVisibleFeedbackKind kind)
        {
            switch (kind)
            {
                case CandidateBoardVisibleFeedbackKind.InvalidStart: return new BattleTextLine("fm.battle.gesture.invalid_start");
                case CandidateBoardVisibleFeedbackKind.NotAdjacent: return new BattleTextLine("fm.battle.gesture.not_adjacent");
                case CandidateBoardVisibleFeedbackKind.Crossed: return new BattleTextLine("fm.battle.gesture.crossed");
                case CandidateBoardVisibleFeedbackKind.WrongEndpoint: return new BattleTextLine("fm.battle.gesture.wrong_endpoint");
                case CandidateBoardVisibleFeedbackKind.Cancelled: return new BattleTextLine("fm.battle.gesture.cancelled");
                case CandidateBoardVisibleFeedbackKind.FocusLost: return new BattleTextLine("fm.battle.gesture.focus_lost");
                case CandidateBoardVisibleFeedbackKind.SecondPointerIgnored: return new BattleTextLine("fm.battle.gesture.second_pointer_ignored");
                case CandidateBoardVisibleFeedbackKind.ActionAccepted: return new BattleTextLine("fm.battle.action.accepted");
                case CandidateBoardVisibleFeedbackKind.ReferenceClosedForInput: return new BattleTextLine("fm.reference.closed_for_input");
                default: return Unknown();
            }
        }
        // Every diagnostic parameter is a code literal approved for this actual producer/domain, never an exception or arbitrary status.
        internal static BattleTextLine Reason(BattleTextDomain domain, string code)
        {
            if (domain == BattleTextDomain.Playback)
                return code == "PlaybackInterrupted" ? new BattleTextLine("fm.battle.playback.interrupted", Arg("errorCode", "PlaybackInterrupted")) : Unknown();
            if (domain == BattleTextDomain.Notification)
                return code == "NotificationFailure" ? new BattleTextLine("fm.save_result.notification_failed", Arg("errorCode", "NotificationFailure")) : Unknown();
            if (domain == BattleTextDomain.Reference)
            {
                string stable;
                switch (code)
                {
                    case "StaleReference": stable = "StaleReference"; break;
                    case "ResolutionRequired": stable = "ResolutionRequired"; break;
                    case "PresentationPending": stable = "PresentationPending"; break;
                    case "UnsupportedBinding": stable = "UnsupportedBinding"; break;
                    case "LevelMismatch": stable = "LevelMismatch"; break;
                    case "StepUnavailable": stable = "StepUnavailable"; break;
                    case "FaceGeometryMismatch": stable = "FaceGeometryMismatch"; break;
                    case "NoPublishedReference": stable = "NoPublishedReference"; break;
                    default: return Unknown();
                }
                return new BattleTextLine("fm.reference.unavailable", Arg("errorCode", stable));
            }
            if (domain != BattleTextDomain.Save && domain != BattleTextDomain.Input && domain != BattleTextDomain.Settlement) return Unknown();
            switch (code)
            {
                case "SaveFailed": return new BattleTextLine("fm.save_recovery.failed", Arg("errorCode", "SaveFailed"));
                case "CommitUnknown": return new BattleTextLine("fm.save_recovery.unknown");
                case "CreationPending": return new BattleTextLine("fm.profile.creation_pending.body");
                case "SettlementRequired": return new BattleTextLine("fm.victory.pending.title");
                case "PendingPreparation": case "ResolutionRequired": return new BattleTextLine("fm.save_recovery.blocking_notice");
                case "OperationSelectionRequired": return new BattleTextLine("fm.save_recovery.select_operation");
                case "CandidateSelectionRequired": return new BattleTextLine("fm.save_recovery.select_candidate");
                case "OriginalOwnerRequired": return new BattleTextLine("fm.save_recovery.return_to_original_flow");
                case "RetryRequired": return new BattleTextLine("fm.save_recovery.retry_before_resolve");
                case "PreviewRequired": case "EndConfirmationRequired": return new BattleTextLine("fm.common.state.review_required");
                case "ResultRequired": return new BattleTextLine("fm.save_recovery.result_required");
                case "StaleContext": case "StaleNavigationContext": case "StaleNavigationToken": case "StaleConfirmation": case "StaleHostRequest":
                    return new BattleTextLine("fm.common.state.changed_review_again");
            }
            string attention = null;
            if (domain == BattleTextDomain.Save)
            {
                switch (code)
                {
                    case "RecoveryBlocked": attention = "RecoveryBlocked"; break;
                    case "RestoreRequired": attention = "RestoreRequired"; break;
                    case "Limit": attention = "SaveLimit"; break;
                }
            }
            else
            {
                switch (code)
                {
                    case "Busy": attention = "BattleBusy"; break;
                    case "InvalidPhase": attention = "BattlePhaseUnavailable"; break;
                    case "NoActiveBattle": attention = "BattleUnavailable"; break;
                    case "ActorDown": case "ActorUnavailable": attention = "BattleActorUnavailable"; break;
                    case "TargetUnavailable": attention = "BattleTargetUnavailable"; break;
                    case "InconsistentBinding": case "UnsupportedBinding": attention = "BattleBindingUnavailable"; break;
                    case "NotPendingLink": attention = "BattleLinkUnavailable"; break;
                    case "Limit": attention = "BattleLimit"; break;
                }
            }
            return attention == null ? Unknown() : new BattleTextLine("fm.common.business_attention", Arg("errorCode", attention));
        }
        internal static BattleTextLine Notification(CandidateApplicationCallResult result)
        {
            return result?.IsCommitted == true && result.OriginalLookup?.IsFound == true &&
                result.OriginalCommitId == result.OriginalLookup.OriginalCommitId && result.NotificationFailure != null ?
                Reason(BattleTextDomain.Notification, result.NotificationFailure.Code) : null;
        }
        internal static BattleTextLine Stage(LocalizationService service, CandidateStageFact fact, BattleSnapshot snapshot)
        {
            if (fact?.Kind == CandidateStageFactKind.RouteLocked)
                return new BattleTextLine("fm.battle.stage.route_locked", Arg("enemyName", Enemy(service, snapshot, fact.Pair)));
            if (fact?.Kind == CandidateStageFactKind.PhaseSelected)
                return new BattleTextLine("fm.battle.stage.phase_selected", Arg("phaseName", PhaseName(service, fact.Phase)));
            if (fact?.Kind == CandidateStageFactKind.TemporaryRouteRemoved)
                return new BattleTextLine("fm.battle.stage.temporary_route_removed", Arg("enemyName", Enemy(service, snapshot, fact.Pair)));
            if (fact?.Kind == CandidateStageFactKind.PendingLinkAdded)
                return new BattleTextLine("fm.battle.stage.pending_link_added", Arg("enemyName", Enemy(service, snapshot, fact.Pair)));
            if (fact?.Kind == CandidateStageFactKind.PendingLinkRemoved)
                return new BattleTextLine("fm.battle.stage.pending_link_removed", Arg("enemyName", Enemy(service, snapshot, fact.Pair)));
            if (fact?.Kind == CandidateStageFactKind.FaceChanged && snapshot != null)
            {
                var faces = snapshot.Baseline.Entry.Level.Faces;
                var next = faces.Select((face, index) => new { face, index }).SingleOrDefault(x => x.face.FaceId == fact.NextFaceId);
                return new BattleTextLine("fm.battle.stage.face_changed", Arg("currentPhase", next == null ? null : Number(next.index + 1)),
                    Arg("totalPhases", Number(faces.Count)));
            }
            return Unknown();
        }
        internal static BattleTextLine Beat(LocalizationService service, CandidateBattlePlaybackFrame frame, CandidateBattlePresentation original)
        {
            if (frame?.OriginalFact == null) return new BattleTextLine(original == null ?
                "fm.battle.playback.current_state" : "fm.battle.playback.action_submitted");
            var fact = frame.OriginalFact; BattleTextLine line;
            if (fact.Kind == CandidateBattleFactKind.DirectAttack)
            {
                var hit = fact.DirectAttack;
                line = new BattleTextLine(hit.Crit.Triggered ? "fm.battle.playback.critical_hit" : "fm.battle.playback.direct_hit",
                    Arg("characterName", Character(service, original.BeforeSnapshot, hit.Actor)),
                    Arg("enemyName", Enemy(service, original.BeforeSnapshot, hit.Pair)), Arg("hpBefore", Hp(hit.HpBefore)), Arg("hpAfter", Hp(hit.HpAfter)));
            }
            else if (fact.Kind == CandidateBattleFactKind.EnemyIntent && fact.EnemyIntent.IntentKind == EnemyIntentKind.Strike && fact.EnemyIntent.Damage != null)
            {
                var hit = fact.EnemyIntent;
                line = new BattleTextLine("fm.battle.playback.enemy_strike",
                    Arg("characterName", Character(service, original.BeforeSnapshot, hit.Damage.TargetMember)),
                    Arg("enemyName", Enemy(service, original.BeforeSnapshot, hit.Pair)), Arg("hpBefore", Hp(hit.Damage.HpBefore)), Arg("hpAfter", Hp(hit.Damage.HpAfter)));
            }
            else if (fact.Kind == CandidateBattleFactKind.Stage) line = Stage(service, fact.Stage, original.BeforeSnapshot);
            else if (fact.Kind == CandidateBattleFactKind.EnemyIntent && fact.EnemyIntent.IntentKind == EnemyIntentKind.Charge)
                line = new BattleTextLine("fm.battle.playback.enemy_charge", Arg("enemyName", Enemy(service, original.BeforeSnapshot, fact.EnemyIntent.Pair)));
            else line = Unknown();
            return new BattleTextLine("fm.battle.playback.beat_row", Arg("beatNumber", Number(fact.Index + 1)), Arg("beat", line.Resolve(service)));
        }
        internal static BattleTextLine History(LocalizationService service, CandidateBattleOperationRecord record, int number)
        {
            if (record.Kind == CandidateBattleOperationKind.Link) return new BattleTextLine("fm.history.link_row",
                Arg("actionNumber", Number(number)), Arg("enemyName", Enemy(service, record.BeforeSnapshot, record.Request.Pair)),
                Arg("operationName", Operation(service, CandidateApplicationKind.Link)));
            if (record.Kind != CandidateBattleOperationKind.Attack) return Unknown();
            return new BattleTextLine("fm.history.action_row", Arg("actionNumber", Number(number)),
                Arg("characterName", Character(service, record.BeforeSnapshot, record.Request.Actor)),
                Arg("enemyName", Enemy(service, record.BeforeSnapshot, record.Request.Pair)),
                Arg("operationName", Operation(service, CandidateApplicationKind.Attack)));
        }
    }

}
