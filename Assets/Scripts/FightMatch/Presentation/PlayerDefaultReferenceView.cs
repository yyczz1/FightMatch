using System;
using System.Linq;
using FightMatch.Application;
using FightMatch.Core;
using FightMatch.Input;
using UnityEngine;
using UnityEngine.UIElements;

namespace FightMatch.Presentation
{
    // A second display layer. It has neither a battle submitter nor a presentation-token reporter.
    public sealed class PlayerDefaultReferenceView : VisualElement, IDisposable
    {
        private PlayerBattleController owner;
        private PlayerBattlePageToken page;
        private CandidateBoardElement board;
        private PlayerBattleContext context;
        private CandidateBattlePresentation step;
        private readonly Label explanation = new Label { name = "reference-explanation" };
        private readonly Label beat = new Label { name = "reference-beat" };
        private readonly VisualElement steps = new VisualElement();
        private IVisualElementScheduledItem scheduleItem;
        private Button closeButton;
        private int nextFact;
        private double elapsed, lastTick;
        private long playEpoch;
        public PlayerDefaultReference Source { get; private set; }
        public CandidateBattlePlaybackFrame Frame { get; private set; }
        public bool IsOpen { get; private set; }
        public string OverlayReason { get; private set; }
        public long Generation { get; private set; }
        public PlayerDefaultReferenceView()
        {
            name = "player-default-reference"; style.display = DisplayStyle.None;
            style.position = Position.Absolute; style.right = 8; style.top = 8; style.width = 320;
            style.backgroundColor = new Color(.12f, .14f, .18f, .98f);
            Add(explanation); Add(beat); Add(steps);
            closeButton = new Button { name = "close-reference", text = "关闭参考" }; Add(closeButton);
            RegisterCallback<DetachFromPanelEvent>(e => Close());
        }
        public void Show(PlayerBattleController controller, PlayerBattlePageToken token, CandidateBoardElement target, PlayerDefaultReference reference)
        {
            Close();
            if (controller == null || !controller.Owns(token) || target == null || reference == null) return;
            owner = controller; page = token; board = target; context = controller.View.Context; Source = reference;
            IsOpen = true; style.display = DisplayStyle.Flex; owner.Input.Changed += OnInput; owner.Playback.Changed += OnInput;
            explanation.text = reference.Title + "\n" + string.Join("\n", reference.Conditions) + "\n当前差异：\n" +
                string.Join("\n", reference.CurrentDifferences) + "\n" + reference.CurrentAnalysis;
            steps.Clear(); var generation = Generation;
            closeButton.RemoveFromHierarchy();
            closeButton = new Button(() =>
            { if (generation == Generation && IsOpen && ReferenceEquals(controller, owner) && controller.Owns(token)) Close(); })
            { name = "close-reference", text = "关闭参考" }; Add(closeButton);
            for (var i = 0; i < reference.Steps.Count; i++)
            {
                var index = i; var record = reference.Steps[i];
                steps.Add(new Button(() => { if (generation == Generation && IsOpen) PlayStep(index); })
                { name = "reference-step-" + i, text = (i + 1) + ". " + record.BeforeSnapshot.Board.Face.FaceId + " / " + record.Kind });
            }
            OverlayReason = ContextReason(); beat.text = OverlayReason ?? "选择原验证步骤，仅播放参考覆盖";
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
            if (OverlayReason != null) { beat.text = OverlayReason; ClearOverlay(); return false; }
            ClearOverlay(); step = Source.ReadStep(index); nextFact = 0; elapsed = 0;
            Frame = CandidateBattlePlaybackFrame.From(step.BeforeSnapshot, "原验证步骤", false, Source.Steps[index].Request.Route);
            if (!board.SetReferenceOverride(Frame)) { OverlayReason = "FaceGeometryMismatch"; ClearOverlay(); return false; }
            beat.text = "参考：" + Frame.Beat;
            var generation = Generation; var epoch = playEpoch; var controller = owner; lastTick = Time.realtimeSinceStartupAsDouble;
            scheduleItem = schedule.Execute(() =>
            {
                if (!IsOpen || generation != Generation || epoch != playEpoch || !ReferenceEquals(controller, owner)) return;
                var now = Time.realtimeSinceStartupAsDouble; Advance(Math.Max(0, (now - lastTick) * 1000)); lastTick = now;
            }).Every(16);
            return true;
        }
        public void Advance(double milliseconds)
        {
            if (double.IsNaN(milliseconds) || double.IsInfinity(milliseconds) || milliseconds < 0) throw new ArgumentOutOfRangeException(nameof(milliseconds));
            if (!IsOpen || step == null) return;
            var reason = ContextReason(); if (reason != null) { OverlayReason = reason; ClearOverlay(); beat.text = reason; return; }
            elapsed += milliseconds;
            while (step != null && elapsed >= CandidateBattlePlaybackController.FactMilliseconds)
            {
                elapsed -= CandidateBattlePlaybackController.FactMilliseconds;
                if (nextFact == step.OrderedFacts.Count) { scheduleItem?.Pause(); return; }
                Frame = Frame.Apply(step, step.OrderedFacts[nextFact++]);
                if (!SameGeometry(owner.Input.View.BattleSnapshot.Board.Face, Frame.Face) || !board.SetReferenceOverride(Frame))
                { OverlayReason = "FaceGeometryMismatch"; ClearOverlay(); beat.text = "该步骤切换至另一面；当前棋盘不叠加"; return; }
                beat.text = "参考：" + Frame.Beat;
            }
        }
        private void OnInput()
        {
            if (!IsOpen || owner == null) return;
            // Do not cancel, release capture, or detach the real board on the first drag.
            if (owner.Input.Gesture.Stage == GestureStage.Dragging) { Close(); return; }
            var reason = ContextReason();
            if (reason == "StaleReference" || reason == "ResolutionRequired") { Close(); return; }
            if (reason != null) { OverlayReason = reason; ClearOverlay(); beat.text = reason; }
        }
        private void ClearOverlay() { playEpoch++; scheduleItem?.Pause(); scheduleItem = null; step = null; Frame = null; board?.ClearReferenceOverride(); }
        public void Close()
        {
            Generation++; ClearOverlay();
            if (owner != null) { owner.Input.Changed -= OnInput; owner.Playback.Changed -= OnInput; }
            IsOpen = false; style.display = DisplayStyle.None; owner = null; page = null; board = null; context = null; Source = null;
        }
        public void Dispose() { Close(); }
    }
}
