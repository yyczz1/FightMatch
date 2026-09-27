using System;
using System.Linq;
using FightMatch.Application;
using FightMatch.Core;
using FightMatch.Platform;
using UnityEngine;
using UnityEngine.UIElements;

namespace FightMatch.Presentation
{
    public sealed class CandidateBattlePlaybackView : VisualElement, IDisposable
    {
        public CandidateBoardInputView InputView { get; } = new CandidateBoardInputView();
        public CandidateBattlePlaybackController Controller { get; private set; }
        private readonly Label hp = new Label { name = "playback-hp" };
        private readonly Label intent = new Label { name = "playback-intent" };
        private readonly Label beat = new Label { name = "playback-beat" };
        private readonly Label stage = new Label { name = "playback-stage" };
        private readonly Label diagnostic = new Label { name = "playback-diagnostic" };
        private IVisualElementScheduledItem scheduled;
        private long epoch, scheduledGeneration = -1;
        private double lastTick;
        private bool closed = true, disposed, memorySubscribed;

        public CandidateBattlePlaybackView()
        {
            name = "candidate-playback-view"; style.flexGrow = 1;
            Add(hp); Add(intent); Add(beat); Add(stage); Add(diagnostic); Add(InputView);
            Add(new Button(SkipToFinal) { name = "skip-playback", text = "跳到当前战局" });
            RegisterCallback<DetachFromPanelEvent>(e => Close());
            RegisterCallback<BlurEvent>(e => { if (e.relatedTarget is VisualElement target && Contains(target)) return; SkipToFinal(); }, TrickleDown.TrickleDown);
        }
        public void Attach(CandidateBattleApplicationSystem system, SaveStoreBudget budget)
        {
            if (disposed) throw new ObjectDisposedException(nameof(CandidateBattlePlaybackView));
            Close(); InputView.Attach(system, budget); InputView.SetEnabled(true); closed = false;
            Controller = new CandidateBattlePlaybackController(InputView.Controller, system); Controller.Changed += Render;
            UnityEngine.Application.lowMemory += OnLowMemory; memorySubscribed = true;
            // Explicit new-page takeover is the only unconditional 019 RebuildLatest use in this host.
            InputView.Controller.RebuildLatest(); Render();
        }
        public void Advance(double deltaMilliseconds)
        { lastTick = Time.realtimeSinceStartupAsDouble; Controller?.Advance(deltaMilliseconds); }
        public void SkipToFinal() { if (!closed) { StopSchedule(); Controller?.SkipToFinal(); } }
        public void NotifyLowMemory() { OnLowMemory(); }
        private void OnLowMemory() { SkipToFinal(); }
        public void ReportSchedulingFailure(Exception error)
        { if (!closed) { StopSchedule(); Controller.ReportSchedulingFailure(error); } }
        private void StopSchedule() { scheduled?.Pause(); scheduled = null; scheduledGeneration = -1; epoch++; }
        private void Render()
        {
            if (Controller == null) return;
            var frame = Controller.Frame; var latest = Controller.LatestView;
            if (Controller.IsPlaying) InputView.Board.SetPlaybackOverride(frame); else InputView.Board.ClearPlaybackOverride();
            hp.text = string.Join(" | ", frame.Actors.Select(a => a.Label + " HP " + CandidateBattlePlaybackFrame.Hp(a.Hp) + "/" + CandidateBattlePlaybackFrame.Hp(a.MaxHp)));
            intent.text = string.Join(" | ", frame.Actors.Where(a => a.NextIntent != null).Select(a => a.Label + " 意图：" + a.NextIntent));
            beat.text = (Controller.IsPlaying ? "节拍 " + frame.OriginalFactIndex + "：" : "") + frame.Beat;
            stage.text = "阶段：" + frame.Phase + "；" + frame.StageFeedback;
            diagnostic.text = Controller.Diagnostic ?? (latest.IsPublishedHeadVerified && latest.Phase == CandidateApplicationPhase.Ready ? "" : "保存状态：" + latest.Phase + "；" + latest.Attack.Reason);
            InputView.Q<Label>("enemy-status").style.display = DisplayStyle.None;
            InputView.Q<Label>("phase-status").style.display = DisplayStyle.None;
            foreach (var actor in frame.Actors.Where(a => a.Key.Kind == BattleCombatantKind.Participant))
            {
                var button = InputView.Q<Button>("member-" + actor.Key.CharacterId);
                if (button != null) button.text = (InputView.Controller.SelectedCharacterId == actor.Key.CharacterId ? "● " : "") + actor.Label + " HP " + CandidateBattlePlaybackFrame.Hp(actor.Hp);
            }
            if (closed || !Controller.IsPlaying) { if (scheduled != null) StopSchedule(); return; }
            if (scheduled != null && scheduledGeneration == Controller.Generation) return;
            StopSchedule(); scheduledGeneration = Controller.Generation; var generation = scheduledGeneration; var lifetime = epoch; var owner = Controller;
            lastTick = Time.realtimeSinceStartupAsDouble;
            scheduled = schedule.Execute(() =>
            {
                if (closed || lifetime != epoch || !ReferenceEquals(owner, Controller) || generation != owner.Generation) return;
                try { var now = Time.realtimeSinceStartupAsDouble; var delta = Math.Max(0, (now - lastTick) * 1000); lastTick = now; owner.Advance(delta); }
                catch (Exception error) { ReportSchedulingFailure(error); }
            }).Every(16);
        }
        public void Close()
        {
            closed = true; StopSchedule();
            if (memorySubscribed) { UnityEngine.Application.lowMemory -= OnLowMemory; memorySubscribed = false; }
            InputView.Board?.ClearPlaybackOverride();
            if (Controller != null) { Controller.Changed -= Render; Controller.Dispose(); }
            InputView.Board?.ClearPlaybackOverride(); InputView.Close(); InputView.SetEnabled(false); Render();
        }
        public void Dispose() { if (disposed) return; Close(); disposed = true; }
    }
}
