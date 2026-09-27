using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FightMatch.Application;
using FightMatch.Platform;
using FightMatch.Presentation;
using FlowPuzzle.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;
using static FightMatch.Core.Tests.LocalSaveTestFiles;

namespace FightMatch.Core.Tests
{
    internal sealed class PlaybackPanelRig : IDisposable
    {
        private static readonly List<PlaybackPanelRig> open = new List<PlaybackPanelRig>();
        private readonly BoardPanelRig basePanel;
        internal BattleApplicationRig Battle => basePanel.Battle;
        internal CandidateBattlePlaybackView Host { get; }
        internal CandidateBoardInputController Input => Host.InputView.Controller;
        internal CandidateBattlePlaybackController Playback => Host.Controller;
        internal CandidateBoardElement Board => Host.InputView.Board;
        internal Button Peer => basePanel.Peer;
        internal VisualElement Root => basePanel.Window.rootVisualElement;
        internal PlaybackPanelRig(int level = 1)
        {
            basePanel = new BoardPanelRig(level: level);
            basePanel.UI.RemoveFromHierarchy();
            Host = new CandidateBattlePlaybackView(); Root.Add(Host); Host.style.flexGrow = 0; Host.style.width = 600;
            Host.Attach(Battle.System, B()); Resize(); open.Add(this);
        }
        internal void Resize()
        {
            Host.InputView.style.flexGrow = 0; Board.style.flexGrow = 0; Board.style.flexShrink = 0; Board.style.minHeight = 0;
            Board.style.width = Battle.State.Board.Face.Width * 40; Board.style.height = Battle.State.Board.Face.Height * 40;
        }
        internal IEnumerator Ready()
        {
            yield return null; yield return null; yield return null;
            Assert.NotNull(Board.panel); Assert.AreEqual(ContextType.Editor, Board.panel.contextType);
            Board.panel.Pick(Vector2.zero); Assert.Greater(Board.contentRect.width, 0); Assert.Greater(Board.contentRect.height, 0);
            Input.Refresh(); TestContext.Out.WriteLine("P27 actual Editor panel " + Board.contentRect);
        }
        internal IReadOnlyList<FlowPos> Route(int pair = 0) { return Battle.Runtime.Domain.Routes[pair]; }
        internal Vector2 Point(FlowPos p)
        { var rect = Board.contentRect; return Board.LocalToWorld(new Vector2(rect.x + (p.x + .5f) * 40, rect.y + (Battle.State.Board.Face.Height - p.y - .5f) * 40)); }
        internal void Draw(int pair = 0)
        {
            var route = Route(pair);
            using (var e = PointerDownEvent.GetPooled(new PlaybackPointer(Point(route[0]), 1))) { e.target = Board; Board.SendEvent(e); }
            for (var i = 1; i < route.Count - 1; i++)
                using (var e = PointerMoveEvent.GetPooled(new PlaybackPointer(Point(route[i]), 1))) { e.target = Board; Board.SendEvent(e); }
            using (var e = PointerUpEvent.GetPooled(new PlaybackPointer(Point(route[route.Count - 1]), 0))) { e.target = Board; Board.SendEvent(e); }
        }
        internal CandidateBattleCallResult Attack(int pair = 0)
        { Input.SelectMember("W"); Draw(pair); Assert.AreEqual("Completed", Input.LastResult.Code, Input.Status); return Input.LastResult; }
        internal void EnterCriticalSeedWitness()
        {
            var runtime = Battle.Runtime; var begin = Battle.Head.Business.Progression.ActiveAttempt.Begin;
            var exit = runtime.Input(CandidateApplicationKind.ExitAttempt, "exit-p27-crit");
            exit.ExitAttempt = new CandidateApplicationEndInput { AttemptId = begin.AttemptId, ChallengeId = begin.ChallengeId,
                EntryBaselineId = begin.EntryBaselineId, ExpectedSceneRevision = Battle.State.SceneRevision };
            ApplicationRuntimeRig.Is(runtime.Submit(runtime.Freeze(exit), (basis, intent, budget) => {
                runtime.Domain.Use(basis.Business); runtime.Domain.EndExit();
                return CandidateApplicationBuildResult.Success(BusinessSaveScenario.Accept(CandidateBusinessSaveCodec.Prepare(runtime.Domain.Input(), budget)),
                    new CandidateApplicationResultInput { EndReceiptId = "end:" + begin.AttemptId });
            }), "Completed");
            runtime.Domain.EntryInput.AttemptId = "attempt:p27-crit"; runtime.Domain.EntryInput.EntryBaselineId = "baseline:p27-crit";
            ApplicationRuntimeRig.Is(runtime.Submit(runtime.Freeze(runtime.EnterInput("enter-p27-crit")), (basis, intent, budget) => {
                runtime.Domain.Use(basis.Business); runtime.Domain.Begin();
                // A declared bounded source search for branch coverage, not a frequency or unconditional-success claim.
                // Keep the existing candidate C=1/1000 and use actual SC01, PCG and EvaluateAttack; inject no words/results.
                CandidateBattleRun chosen = null;
                for (var seed = 0; seed < 4096; seed++)
                {
                    var bytes = new byte[48]; for (var offset = 0; offset < 48; offset += 16) { bytes[offset] = 42; bytes[offset + 8] = 54; }
                    for (var i = 0; i < 8; i++) bytes[i] = (byte)((ulong)seed >> (i * 8));
                    var binding = CandidateRandomPreparer.Prepare(runtime.Domain.History.CurrentRun.Baseline.Entry,
                        new CandidateSeedMaterial { Bytes = bytes, SourceCapabilityId = "isolated:p27-crit-source-search", MappingId = CandidateRandomPreparer.SupportedMappingId }, new ExactMathBudget());
                    Assert.IsTrue(binding.IsAccepted, binding.FieldPath);
                    var run = CandidateBattleOperations.CreateCandidate(binding.Binding, new ExactMathBudget()).Run;
                    var state = run.CurrentSnapshot;
                    var probe = CandidateBattleOperations.EvaluateAttack(run, new CandidateAttackRequest {
                        PlayerId = state.Baseline.Entry.PlayerId, AttemptId = state.Baseline.Entry.AttemptId, OperationId = "p27-source-probe",
                        ExpectedSceneRevision = state.SceneRevision, Actor = state.Members[0].CombatantKey, Pair = state.Enemies[0].PairKey,
                        Route = runtime.Domain.Routes[0] }, new CandidateBattleConditions { PreferenceRevision = basis.Business.Inventory.PreferenceRevision, ItemUseEnabled = false },
                        0, new RandomSamplingBudget(new ExactMathBudget()));
                    Assert.IsTrue(probe.IsAccepted, probe.RejectionCode);
                    if (!probe.Record.DirectAttack.DamageFacts[0].Crit.Triggered) continue;
                    chosen = run;
                    TestContext.Out.WriteLine("P27 declared SC01 seed cohort [0,4096), first critical source seed=" + seed +
                        "; sequence=54; C=1/1000; f=0; material=" + BitConverter.ToString(bytes) + "; actual words=" +
                        string.Join(",", probe.Record.DirectAttack.DamageFacts[0].Crit.Words));
                    break;
                }
                Assert.NotNull(chosen, "A real critical witness is required within the declared finite cohort");
                var history = CandidateHistoryOperations.CreateCandidate(chosen, new ExactMathBudget()); Assert.IsTrue(history.IsAccepted);
                runtime.Domain.History = history.Next;
                return CandidateApplicationBuildResult.Success(BusinessSaveScenario.Accept(CandidateBusinessSaveCodec.Prepare(runtime.Domain.Input(), budget)),
                    new CandidateApplicationResultInput { ChallengeId = runtime.Domain.EntryInput.ChallengeId,
                        AttemptId = runtime.Domain.EntryInput.AttemptId, EntryBaselineId = runtime.Domain.EntryInput.EntryBaselineId });
            }), "Completed");
            Input.Refresh();
        }
        internal void Finish()
        { var maximum = (Playback.Original?.OrderedFacts.Count ?? 0) + 2; Host.Advance(maximum * CandidateBattlePlaybackController.FactMilliseconds); Assert.IsFalse(Playback.IsPlaying); }
        internal string Label(string name) { return Host.Q<Label>(name).text; }
        internal static void Hp(ExactRational actual, int expected)
        { Assert.AreEqual(new System.Numerics.BigInteger(expected), actual.Numerator); Assert.AreEqual(System.Numerics.BigInteger.One, actual.Denominator); }
        internal static void Same(ExactRational a, ExactRational b)
        { Assert.AreEqual(a.Numerator, b.Numerator); Assert.AreEqual(a.Denominator, b.Denominator); }
        internal static void CloseAll() { foreach (var r in open.ToArray()) r.Dispose(); BoardPanelRig.CloseAll(); }
        public void Dispose()
        { if (!open.Remove(this)) return; try { Host.Dispose(); } finally { basePanel.Dispose(); } }
        private sealed class PlaybackPointer : IPointerEvent
        {
            public int pointerId => 0;
            public string pointerType => UnityEngine.UIElements.PointerType.mouse;
            public bool isPrimary => true;
            public int button => 0;
            public int pressedButtons { get; }
            public Vector3 position { get; }
            public Vector3 localPosition => position;
            public Vector3 deltaPosition => Vector3.zero;
            public float deltaTime => 0;
            public int clickCount => 1;
            public float pressure => 1;
            public float tangentialPressure => 0;
            public float altitudeAngle => 0;
            public float azimuthAngle => 0;
            public float twist => 0;
            public Vector2 radius => Vector2.zero;
            public Vector2 radiusVariance => Vector2.zero;
            public Vector2 tilt => Vector2.zero;
            public PenStatus penStatus => default;
            public EventModifiers modifiers => EventModifiers.None;
            public bool shiftKey => false;
            public bool ctrlKey => false;
            public bool commandKey => false;
            public bool altKey => false;
            public bool actionKey => false;
            internal PlaybackPointer(Vector2 p, int buttons) { position = p; pressedButtons = buttons; }
        }
    }
}
