using System;
using System.Collections.Generic;
using System.Linq;
using FightMatch.Application;
using FightMatch.Core;
using FightMatch.Input;
using UnityEngine;
using UnityEngine.Events;

namespace FightMatch.Presentation
{
    // A second display layer. It has neither a battle submitter nor a presentation-token reporter.
    public sealed class PlayerDefaultReferenceView : MonoBehaviour, IDisposable
    {
        [SerializeField] private LocalizedTmpText title, explanation, beat;
        [SerializeField] private RectTransform steps;
        [SerializeField] private UnityEngine.UI.Button buttonTemplate, closeButton;
        private readonly List<KeyValuePair<UnityEngine.UI.Button, UnityAction>> listeners = new List<KeyValuePair<UnityEngine.UI.Button, UnityAction>>();
        private PlayerBattleController owner;
        private PlayerBattlePageToken page;
        private CandidateBoardElement board;
        private PlayerBattleContext context;
        private CandidateBattlePresentation step;
        private LocalizationService localization;
        private BattleLocalizedRows explanationRows;
        private readonly List<UnityEngine.UI.Button> stepButtons = new List<UnityEngine.UI.Button>();
        private int nextFact;
        private int selectedStep = -1;
        private bool stepCompleted;
        private double elapsed, lastTick;
        private long playEpoch;
        private bool ticking;
        private Action closed;
        public PlayerDefaultReference Source { get; private set; }
        public CandidateBattlePlaybackFrame Frame { get; private set; }
        public bool IsOpen { get; private set; }
        public string OverlayReason { get; private set; }
        public long Generation { get; private set; }

        internal void Bind(LocalizationService service, Action closed = null)
        {
            Unbind(); localization = service ?? throw new ArgumentNullException(nameof(service));
            if (title == null || explanation == null || beat == null || steps == null || buttonTemplate == null || closeButton == null)
                throw new InvalidOperationException("PlayerDefaultReferenceView serialized bindings are incomplete.");
            this.closed = closed;
            buttonTemplate.gameObject.SetActive(false); closeButton.gameObject.SetActive(true);
            explanationRows = new BattleLocalizedRows(explanation, "reference-explanation"); localization.LocaleChanged += OnLocaleChanged;
        }
        public void Show(PlayerBattleController controller, PlayerBattlePageToken token, CandidateBoardElement target, PlayerDefaultReference reference)
        {
            Close();
            if (controller == null || !controller.Owns(token) || target == null || reference == null) return;
            if (localization == null) throw new InvalidOperationException("Reference localization has not been bound.");
            owner = controller; page = token; board = target; context = controller.View.Context; Source = reference;
            IsOpen = true; owner.Input.Changed += OnInput; owner.Playback.Changed += OnInput;
            var generation = Generation;
            var close = closeButton;
            close.GetComponentInChildren<LocalizedTmpText>(true).Bind(localization, "fm.reference.close_button");
            UnityAction closeAction = () =>
            { if (this != null && close != null && close.isActiveAndEnabled && close.interactable && generation == Generation && IsOpen && ReferenceEquals(controller, owner) && controller.Owns(token)) Close(); };
            close.onClick.AddListener(closeAction); listeners.Add(new KeyValuePair<UnityEngine.UI.Button, UnityAction>(close, closeAction));
            close.gameObject.SetActive(true);
            for (var i = 0; i < reference.Steps.Count; i++)
            {
                var index = i; var record = reference.Steps[i];
                var button = Instantiate(buttonTemplate, steps, false); button.gameObject.SetActive(false);
                button.GetComponent<FightMatchViewId>().Assign(FightMatchViewId.Row("reference-step:" + record.OperationId));
                stepButtons.Add(button);
                UnityAction action = () => { if (this != null && button != null && button.isActiveAndEnabled && button.interactable && generation == Generation && IsOpen) PlayStep(index); };
                button.onClick.AddListener(action); listeners.Add(new KeyValuePair<UnityEngine.UI.Button, UnityAction>(button, action));
                button.gameObject.SetActive(true);
            }
            OverlayReason = ContextReason(); RenderCopy(); gameObject.SetActive(true);
        }
        private void OnLocaleChanged(LocaleId locale) { if (IsOpen) RenderCopy(); }
        private void RenderCopy()
        {
            title.Bind(localization, "fm.reference.title");
            var lines = new List<BattleTextLine>();
            var conditions = new List<BattleTextLine> { new BattleTextLine("fm.reference.condition.level",
                BattleText.Arg("levelName", BattleText.Level(localization, Source.LevelId))) };
            foreach (var member in Source.Entry.Members.OrderBy(x => x.OriginalSlot))
                conditions.Add(new BattleTextLine("fm.reference.condition.member",
                    BattleText.Arg("characterName", BattleText.Character(localization, member.CharacterId)), BattleText.Arg("level", BattleText.Number(member.Level)),
                    BattleText.Arg("slotNumber", BattleText.Number(member.OriginalSlot + 1)),
                    BattleText.Arg("skillSummary", member.LearnedSkills.Count == 0 ? BattleText.Resolve(localization, "fm.reference.value.none") : null)));
            if (Source.Entry.CarryMode == EntryCarryMode.Empty && Source.Steps.All(x => !x.Conditions.ItemUseEnabled))
                conditions.Add(new BattleTextLine("fm.reference.condition.carry_empty"));
            else conditions.Add(BattleText.Unknown()); // WAITING_FOR_COPY_KEYS: nonempty reference carry/skills/items.
            conditions.Add(new BattleTextLine("fm.reference.condition.random_verified"));
            var conditionText = conditions.Select(x => x.Resolve(localization)).ToArray();
            lines.Add(new BattleTextLine("fm.reference.conditions", BattleText.Arg("conditions",
                conditionText.Any(x => x == null) ? null : string.Join("\n", conditionText))));
            lines.Add(new BattleTextLine("fm.reference.scope_notice"));
            lines.Add(new BattleTextLine("fm.reference.differences.title"));
            var current = context.Snapshot; var head = context.Head;
            var referenceMembers = Source.Entry.Members.OrderBy(x => x.OriginalSlot).ToArray();
            var formation = head.Business.Roster.Formation;
            var sameParty = referenceMembers.Length == formation.Count(x => x != null) &&
                referenceMembers.All(x => x.OriginalSlot < formation.Count && formation[x.OriginalSlot] == x.CharacterId);
            if (!sameParty)
            {
                var currentParty = formation.Select((id, slot) => new { id, slot }).Where(x => x.id != null)
                    .Select(x => PartyMember(x.id, x.slot)).ToArray();
                var referenceParty = referenceMembers.Select(x => PartyMember(x.CharacterId, x.OriginalSlot)).ToArray();
                if (currentParty.Length == 0) lines.Add(new BattleTextLine("fm.reference.difference.party_empty"));
                else lines.Add(new BattleTextLine("fm.reference.difference.party",
                    BattleText.Arg("currentParty", currentParty.Any(x => x == null) ? null : string.Join(", ", currentParty)),
                    BattleText.Arg("referenceParty", referenceParty.Any(x => x == null) ? null : string.Join(", ", referenceParty))));
            }
            foreach (var member in referenceMembers)
            {
                var actual = current?.Baseline.Entry.Members.SingleOrDefault(x => x.CharacterId == member.CharacterId);
                var actualLevel = actual?.Level ?? head.Business.Roster.Find(member.CharacterId)?.Level;
                if (!actualLevel.HasValue) lines.Add(new BattleTextLine("fm.reference.difference.member_absent",
                    BattleText.Arg("characterName", BattleText.Character(localization, member.CharacterId)),
                    BattleText.Arg("referenceLevel", BattleText.Number(member.Level))).For("reference-member-absent:" + member.CharacterId));
                else if (actualLevel != member.Level) lines.Add(new BattleTextLine("fm.reference.difference.level",
                    BattleText.Arg("characterName", BattleText.Character(localization, member.CharacterId)),
                    BattleText.Arg("currentLevel", actualLevel.HasValue ? BattleText.Number(actualLevel.Value) : null),
                    BattleText.Arg("referenceLevel", BattleText.Number(member.Level))).For("reference-level:" + member.CharacterId));
                if (actual != null && !actual.LearnedSkills.SequenceEqual(member.LearnedSkills)) lines.Add(BattleText.Unknown());
            }
            if (current?.Baseline.Entry.CarryMode != Source.Entry.CarryMode || head.Business.Inventory.ActiveCarry?.Rows.Any(x => x.C > 0) == true ||
                head.Business.Inventory.Loadouts.Any(x => x.Enabled == true) ||
                head.Business.Inventory.PreferenceRevision != Source.Steps[0].Conditions.PreferenceRevision)
                lines.Add(BattleText.Unknown()); // WAITING_FOR_COPY_KEYS: non-L1 skill/carry/item-preference differences.
            lines.Add(new BattleTextLine("fm.reference.difference.random"));
            if (current == null || current.EffectiveActionsCompleted != 0) lines.Add(new BattleTextLine("fm.reference.difference.state"));
            explanationRows.Bind(localization, lines);
            for (var i = 0; i < stepButtons.Count; i++)
            {
                var record = Source.Steps[i];
                var line = stepCompleted && selectedStep == i ? new BattleTextLine("fm.reference.replay_button") :
                    record.Kind == CandidateBattleOperationKind.Attack ? new BattleTextLine("fm.reference.step.row",
                    BattleText.Arg("stepNumber", BattleText.Number(i + 1)),
                    BattleText.Arg("characterName", BattleText.Character(localization, record.BeforeSnapshot, record.Request.Actor)),
                    BattleText.Arg("enemyName", BattleText.Enemy(localization, record.BeforeSnapshot, record.Request.Pair))) :
                    record.Kind == CandidateBattleOperationKind.Link ? new BattleTextLine("fm.reference.step.link_row",
                        BattleText.Arg("stepNumber", BattleText.Number(i + 1)),
                        BattleText.Arg("enemyName", BattleText.Enemy(localization, record.BeforeSnapshot, record.Request.Pair))) : BattleText.Unknown();
                line.Bind(stepButtons[i].GetComponentInChildren<LocalizedTmpText>(true), localization);
            }
            BindBeat();
        }
        private string PartyMember(string character, int slot)
        {
            var name = BattleText.Character(localization, character);
            return name == null ? null : BattleText.Number(slot + 1) + ": " + name;
        }
        private string ContextReason()
        {
            if (!IsOpen || owner == null || !owner.Owns(page)) return "StaleReference";
            var view = owner.Input.View; var snapshot = view.BattleSnapshot;
            if (!view.IsPublishedHeadVerified || view.Phase != CandidateApplicationPhase.Ready) return "ResolutionRequired";
            if (view.PresentationToken != null || owner.Playback.IsPlaying || board.PlaybackOverride != null) return "PresentationPending";
            if (view.CommitId != context.CommitId || snapshot?.Baseline.Entry.AttemptId != context.AttemptId || snapshot?.SceneRevision != context.SceneRevision)
                return "StaleReference";
            var binding = snapshot?.Baseline.Entry.GetDefinitionBinding();
            if (binding == null || !binding.Content.Same(Source.Binding)) return "UnsupportedBinding";
            if (binding.LevelId != Source.LevelId || binding.CanonicalLevelVersion != Source.LevelVersion) return "LevelMismatch";
            return null;
        }
        private static bool SameGeometry(PreparedFace a, PreparedFace b)
        {
            if (a == null || b == null || a.FaceId != b.FaceId || a.Width != b.Width || a.Height != b.Height || a.Pairs.Count != b.Pairs.Count) return false;
            return a.Pairs.All(x => b.Pairs.Any(y => x.PairId == y.PairId && x.GeometryColorId == y.GeometryColorId &&
                x.EndpointA.Equals(y.EndpointA) && x.EndpointB.Equals(y.EndpointB)));
        }
        public bool PlayStep(int index)
        {
            OverlayReason = ContextReason();
            if (OverlayReason == null && (index < 0 || index >= Source.Steps.Count)) OverlayReason = "StepUnavailable";
            if (OverlayReason == null && !SameGeometry(owner.Input.View.BattleSnapshot.Board.Face, Source.Steps[index].BeforeSnapshot.Board.Face))
                OverlayReason = "FaceGeometryMismatch";
            if (OverlayReason != null) { ClearOverlay(); if (IsOpen) RenderCopy(); else BindBeat(); return false; }
            ClearOverlay(); step = Source.ReadStep(index); selectedStep = index; nextFact = 0; elapsed = 0;
            Frame = CandidateBattlePlaybackFrame.From(step.BeforeSnapshot, string.Empty, false, Source.Steps[index].Request.Route);
            if (!board.SetReferenceOverride(Frame)) { OverlayReason = "FaceGeometryMismatch"; ClearOverlay(); RenderCopy(); return false; }
            RenderCopy(); ticking = true; lastTick = Time.realtimeSinceStartupAsDouble;
            return true;
        }
        private void Update()
        {
            if (!IsOpen || !ticking || step == null) return;
            var generation = Generation; var epoch = playEpoch; var controller = owner;
            var now = Time.realtimeSinceStartupAsDouble; var delta = Math.Max(0, (now - lastTick) * 1000);
            if (delta < 16) return;
            lastTick = now;
            if (generation == Generation && epoch == playEpoch && ReferenceEquals(controller, owner)) Advance(delta);
        }
        public void Advance(double milliseconds)
        {
            if (double.IsNaN(milliseconds) || double.IsInfinity(milliseconds) || milliseconds < 0) throw new ArgumentOutOfRangeException(nameof(milliseconds));
            if (!IsOpen || step == null) return;
            lastTick = Time.realtimeSinceStartupAsDouble;
            var reason = ContextReason(); if (reason != null) { OverlayReason = reason; ClearOverlay(); RenderCopy(); return; }
            elapsed += milliseconds;
            while (step != null && elapsed >= CandidateBattlePlaybackController.FactMilliseconds)
            {
                elapsed -= CandidateBattlePlaybackController.FactMilliseconds;
                if (nextFact == step.OrderedFacts.Count) { ticking = false; stepCompleted = true; RenderCopy(); return; }
                Frame = Frame.Apply(step, step.OrderedFacts[nextFact++]);
                if (!SameGeometry(owner.Input.View.BattleSnapshot.Board.Face, Frame.Face) || !board.SetReferenceOverride(Frame))
                { OverlayReason = "FaceGeometryMismatch"; ClearOverlay(); RenderCopy(); return; }
                BindBeat();
            }
        }
        private void BindBeat()
        {
            if (localization == null) return;
            if (OverlayReason != null)
                BattleText.Reason(BattleTextDomain.Reference, OverlayReason).Bind(beat, localization);
            else if (Frame?.OriginalFact == null) beat.Bind(localization, "fm.reference.step.select_prompt");
            else BattleText.Beat(localization, Frame, step).Bind(beat, localization);
        }
        private void OnInput()
        {
            if (!IsOpen || owner == null) return;
            // Do not cancel, release the pointer, or detach the real board on the first drag.
            if (owner.Input.Gesture.Stage == GestureStage.Dragging)
            {
                var input = owner.Input;
                Close(); // Unsubscribe before Changed is emitted again; retain the same active pointer.
                input.PublishReferenceClosedFeedback(); return;
            }
            var reason = ContextReason();
            if (reason == "StaleReference" || reason == "ResolutionRequired") { Close(); return; }
            if (reason != null) { OverlayReason = reason; ClearOverlay(); RenderCopy(); }
        }
        private void ClearOverlay()
        { playEpoch++; ticking = false; step = null; Frame = null; selectedStep = -1; stepCompleted = false; if (board != null) board.ClearReferenceOverride(); }
        public void Close()
        {
            var notify = IsOpen ? closed : null;
            var oldBoard = board; var oldListeners = listeners.ToArray();
            DetachReferenceManaged();
            if (oldBoard != null) oldBoard.ClearReferenceOverride();
            foreach (var listener in oldListeners)
            {
                var button = listener.Key; if (button == null) continue;
                button.onClick.RemoveListener(listener.Value);
                foreach (var text in button.GetComponentsInChildren<LocalizedTmpText>(true)) text.Unbind();
                if (button == closeButton || button.transform.parent != steps) continue;
                button.gameObject.SetActive(false); button.transform.SetParent(null, false);
                if (UnityEngine.Application.isPlaying) Destroy(button.gameObject); else DestroyImmediate(button.gameObject);
            }
            if (this != null) explanationRows?.Clear();
            if (title != null) title.Unbind();
            if (explanation != null) explanation.Unbind();
            if (beat != null) beat.Unbind();
            if (this != null) gameObject.SetActive(false);
            notify?.Invoke();
        }
        internal void SuspendCallbacks()
        {
            ticking = false; playEpoch++;
            if (owner != null) { owner.Input.Changed -= OnInput; owner.Playback.Changed -= OnInput; }
        }
        private void DetachReferenceManaged()
        {
            Generation++; SuspendCallbacks();
            IsOpen = false; owner = null; page = null; board = null; context = null; Source = null;
            step = null; Frame = null; selectedStep = -1; stepCompleted = false;
            listeners.Clear(); stepButtons.Clear();
        }
        internal void DetachManaged()
        {
            if (localization != null) localization.LocaleChanged -= OnLocaleChanged;
            DetachReferenceManaged(); localization = null; explanationRows = null; closed = null;
        }
        public void Unbind()
        {
            closed = null;
            if (localization != null) localization.LocaleChanged -= OnLocaleChanged;
            Close(); explanationRows = null; localization = null;
        }
        private void OnDisable() { if (IsOpen) Close(); }
        private void OnDestroy() { Unbind(); }
        public void Dispose() { Unbind(); }
    }
}
