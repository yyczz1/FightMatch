using System;
using System.Collections.Generic;
using System.Linq;
using FightMatch.Core;
using FightMatch.Application;
using FightMatch.Platform;
using UnityEngine;
using UnityEngine.Events;

namespace FightMatch.Presentation
{
    public sealed class CandidateBattlePlaybackView : MonoBehaviour, IDisposable
    {
        [SerializeField] private CandidateBoardInputView inputView;
        [SerializeField] private LocalizedTmpText beat, stage, diagnostic;
        [SerializeField] private UnityEngine.UI.Button skip;
        public CandidateBoardInputView InputView => inputView;
        public CandidateBattlePlaybackController Controller { get; private set; }
        private UnityAction skipAction;
        private LocalizationService localization;
        private long epoch, scheduledGeneration = -1, bindingEpoch;
        private double lastTick;
        private bool closed = true, disposed, memorySubscribed, borrowed, ticking, structuralRefresh, nativeBound, callbacksActive;

        internal void Attach(CandidateBattleApplicationSystem system, SaveStoreBudget budget, LocalizationService service)
        {
            if (disposed) throw new ObjectDisposedException(nameof(CandidateBattlePlaybackView));
            Unbind(); borrowed = false; localization = service ?? throw new ArgumentNullException(nameof(service));
            CheckBindings(); inputView.Attach(system, budget, service); closed = false;
            Controller = new CandidateBattlePlaybackController(inputView.Controller, system);
            structuralRefresh = isActiveAndEnabled;
            BindCallbacks();
            // Explicit new-page takeover is the only unconditional 019 RebuildLatest use in this host.
            inputView.Controller.RebuildLatest(); Render();
        }
        internal void Bind(CandidateBoardInputController input, CandidateBattlePlaybackController playback, LocalizationService service)
        {
            if (disposed) throw new ObjectDisposedException(nameof(CandidateBattlePlaybackView));
            if (input == null || playback == null || service == null) throw new ArgumentNullException();
            Unbind(); CheckBindings(); borrowed = true; localization = service;
            inputView.Bind(input, service); closed = false; Controller = playback;
            structuralRefresh = isActiveAndEnabled;
            BindCallbacks(); Render();
        }
        private void CheckBindings()
        {
            if (inputView == null || beat == null || stage == null || diagnostic == null || skip == null)
                throw new InvalidOperationException("CandidateBattlePlaybackView serialized bindings are incomplete.");
        }
        private void BindCallbacks()
        {
            Subscribe();
            if (structuralRefresh) BindNative();
        }
        private void Subscribe()
        {
            callbacksActive = true;
            localization.LocaleChanged -= OnLocaleChanged; localization.LocaleChanged += OnLocaleChanged;
            Controller.Changed -= Render; Controller.Changed += Render;
            if (!memorySubscribed) { UnityEngine.Application.lowMemory += OnLowMemory; memorySubscribed = true; }
        }
        private void BindNative()
        {
            if (nativeBound) return;
            var owner = Controller; var binding = ++bindingEpoch;
            skipAction = () => { if (ReferenceEquals(owner, Controller) && binding == bindingEpoch &&
                inputView != null && inputView.DisplayLayoutSupported) SkipToFinal(); };
            skip.onClick.AddListener(skipAction);
            skip.GetComponentInChildren<LocalizedTmpText>(true).Bind(localization, "fm.battle.playback.skip_button");
            nativeBound = true;
        }
        public void Detach() { Unbind(); }
        public void Unbind() => Unbind(PointerCancellationCause.Cancelled);
        internal void Unbind(PointerCancellationCause cause)
        {
            SuspendCallbacks();
            if (!borrowed && Controller != null && !closed) Close(cause);
            var oldSkip = skipAction;
            DetachSelfManaged();
            if (!ReferenceEquals(inputView, null))
            {
                if (inputView != null)
                {
                    if (inputView.Board != null) inputView.Board.ClearPlaybackOverride();
                    inputView.ClearStageSlotPresentation();
                    inputView.Unbind(cause);
                }
                else inputView.DetachManaged();
            }
            if (skip != null && oldSkip != null) skip.onClick.RemoveListener(oldSkip);
            foreach (var text in new[] { beat, stage, diagnostic }) if (text != null) text.Unbind();
            if (skip != null) foreach (var text in skip.GetComponentsInChildren<LocalizedTmpText>(true)) text.Unbind();
        }
        internal void SuspendCallbacks()
        {
            callbacksActive = false; StopSchedule();
            if (memorySubscribed) { UnityEngine.Application.lowMemory -= OnLowMemory; memorySubscribed = false; }
            if (localization != null) localization.LocaleChanged -= OnLocaleChanged;
            if (Controller != null) Controller.Changed -= Render;
            if (!ReferenceEquals(inputView, null)) inputView.SuspendCallbacks();
        }
        private void DetachSelfManaged()
        {
            closed = true; bindingEpoch++; SuspendCallbacks();
            Controller = null; localization = null; skipAction = null;
            nativeBound = false;
        }
        internal void DetachManaged()
        {
            DetachSelfManaged();
            if (!ReferenceEquals(inputView, null)) inputView.DetachManaged();
        }
        public void Advance(double deltaMilliseconds)
        { lastTick = Time.realtimeSinceStartupAsDouble; Controller?.Advance(deltaMilliseconds); }
        public void SkipToFinal() { if (!closed) { StopSchedule(); Controller?.SkipToFinal(); } }
        public void NotifyLowMemory() { OnLowMemory(); }
        private void OnLowMemory() { SkipToFinal(); }
        public void ReportSchedulingFailure(Exception error)
        { if (!closed) { StopSchedule(); Controller?.ReportSchedulingFailure(error); } }
        private void StopSchedule() { ticking = false; scheduledGeneration = -1; epoch++; }
        private void Update()
        {
            if (closed || !ticking || Controller == null) return;
            var owner = Controller; var generation = scheduledGeneration; var lifetime = epoch;
            if (generation != owner.Generation) { Render(); return; }
            var now = Time.realtimeSinceStartupAsDouble; var delta = Math.Max(0, (now - lastTick) * 1000);
            if (delta < 16) return;
            lastTick = now;
            try { if (lifetime == epoch && ReferenceEquals(owner, Controller)) owner.Advance(delta); }
            catch (Exception error) { ReportSchedulingFailure(error); }
        }
        private void Render()
        {
            if (!structuralRefresh || closed || !callbacksActive || !nativeBound || Controller == null || localization == null) return;
            var frame = Controller.Frame; var latest = Controller.LatestView;
            if (Controller.IsPlaying) inputView.Board.SetPlaybackOverride(frame); else inputView.Board.ClearPlaybackOverride();
            var original = Controller.Original;
            var state = original == null ? latest.BattleSnapshot : frame?.Face?.FaceId == original.BeforeSnapshot.Board.Face.FaceId ?
                original.BeforeSnapshot : original.AfterSnapshot;
            inputView.ClearStageSlotPresentation();
            if (frame != null) foreach (var actor in frame.Actors)
            {
                var member = state?.Members.FirstOrDefault(x => x.CombatantKey.Equals(actor.Key));
                var enemy = state?.Enemies.FirstOrDefault(x => x.CombatantKey.Equals(actor.Key));
                if (member != null)
                    inputView.SetStageSlotPresentation(false, member.Member.OriginalSlot,
                        new BattleTextLine(member.Member.CharacterId == "W" ? "fm.name.character.w" : string.Empty),
                        BattleText.MemberHp(localization, member.Member.CharacterId, actor.Hp, actor.MaxHp), null);
                else if (enemy != null)
                {
                    string intentKey = string.Empty;
                    if (actor.Hp.Numerator.IsZero) intentKey = "fm.battle.intent.defeated";
                    else if (enemy != null && actor.IntentCursor.HasValue)
                    {
                        switch (enemy.Enemy.IntentCycle[(int)(actor.IntentCursor.Value % enemy.Enemy.IntentCycle.Count)].Kind)
                        {
                            case EnemyIntentKind.Strike: intentKey = "fm.battle.intent.strike"; break;
                            case EnemyIntentKind.Charge: intentKey = "fm.battle.intent.charge"; break;
                        }
                    }
                    inputView.SetStageSlotPresentation(true, enemy.Enemy.OriginalSlot, BattleText.EnemyName(localization, enemy.Enemy),
                        BattleText.EnemyHp(localization, enemy.Enemy, actor.Hp, actor.MaxHp),
                        new BattleTextLine("fm.battle.hud.enemy_intent", BattleText.Arg("intentName", BattleText.Resolve(localization, intentKey))));
                }
            }
            if (state == null || frame == null || frame.Face == null)
            {
                BattleText.Hide(beat); BattleText.Hide(stage);
            }
            else
            {
                BattleText.Beat(localization, frame, original).Bind(beat, localization);
                var stageFact = original?.OrderedFacts.LastOrDefault(x => x.Index <= frame.OriginalFactIndex && x.Kind == CandidateBattleFactKind.Stage)?.Stage;
                (stageFact == null ? BattleText.Phase(localization, frame.Phase) : BattleText.Stage(localization, stageFact, original.BeforeSnapshot)).Bind(stage, localization);
            }
            // Diagnostic has exactly one producer: ReportSchedulingFailure. Its exception stays on the controller.
            var problem = Controller.Diagnostic != null ? BattleText.Reason(BattleTextDomain.Playback, "PlaybackInterrupted") :
                BattleText.Save(latest.Phase, latest.Code);
            if (problem == null) BattleText.Hide(diagnostic); else problem.Bind(diagnostic, localization);
            skip.GetComponentInChildren<LocalizedTmpText>(true).Bind(localization,
                Controller.IsPlaying && latest.BattleSnapshot?.Phase == BattlePhase.WonPendingSettlement ?
                "fm.victory.playback.skip_button" : "fm.battle.playback.skip_button");
            skip.interactable = !closed && Controller.IsPlaying && inputView.DisplayLayoutSupported;
            if (closed || !Controller.IsPlaying) { StopSchedule(); return; }
            if (ticking && scheduledGeneration == Controller.Generation) return;
            StopSchedule(); scheduledGeneration = Controller.Generation; ticking = true;
            lastTick = Time.realtimeSinceStartupAsDouble;
        }
        public void Close() => Close(PointerCancellationCause.Cancelled);
        internal void Close(PointerCancellationCause cause)
        {
            closed = true; SuspendCallbacks();
            if (borrowed)
            {
                if (inputView != null)
                {
                    if (inputView.Board != null) inputView.Board.CancelPointer(cause);
                    inputView.Controller?.CancelGesture(cause);
                }
                Controller?.SkipToFinal(); Unbind(cause); return;
            }
            closed = true; StopSchedule();
            if (memorySubscribed) { UnityEngine.Application.lowMemory -= OnLowMemory; memorySubscribed = false; }
            if (inputView != null && inputView.Board != null) inputView.Board.ClearPlaybackOverride();
            // Dispose/Finish also cancel input; preserve their ordering, but consume the originating cause first.
            if (inputView != null)
            {
                if (inputView.Board != null) inputView.Board.CancelPointer(cause);
                inputView.Controller?.CancelGesture(cause);
            }
            if (Controller != null) { Controller.Dispose(); Controller.Changed -= Render; }
            if (inputView != null)
            {
                if (inputView.Board != null) inputView.Board.ClearPlaybackOverride();
                inputView.Close(cause);
            }
            Render();
        }
        private void OnLocaleChanged(LocaleId locale) { Render(); }
        private void OnDisable()
        {
            structuralRefresh = false; SuspendCallbacks();
            if (!borrowed) Close(PointerCancellationCause.FocusLost);
            else
            {
                StopSchedule();
                if (inputView != null && inputView.Board != null) inputView.Board.CancelPointer(PointerCancellationCause.FocusLost);
                if (inputView != null && inputView.Board != null) inputView.Board.ClearPlaybackOverride();
            }
        }
        private void OnEnable()
        {
            structuralRefresh = true;
            if (!closed && Controller != null && localization != null) { Subscribe(); BindNative(); Render(); }
        }
        private void OnApplicationPause(bool paused)
        { if (paused) { if (inputView != null && inputView.Board != null) inputView.Board.CancelPointer(PointerCancellationCause.FocusLost); SkipToFinal(); } }
        private void OnApplicationFocus(bool focused)
        { if (!focused) { if (inputView != null && inputView.Board != null) inputView.Board.CancelPointer(PointerCancellationCause.FocusLost); SkipToFinal(); } }
        private void OnDestroy() { structuralRefresh = false; Dispose(); }
        public void Dispose() { if (disposed) return; Unbind(); disposed = true; }
    }
}
