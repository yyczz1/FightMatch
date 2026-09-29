using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FightMatch.Application;
using FightMatch.Content;
using FightMatch.Input;
using FightMatch.Presentation;
using FlowPuzzle.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using static FightMatch.Core.Tests.PlayerSessionTestData;
using static FightMatch.Core.Tests.PlayerRosterTestData;
using BattlePage = FightMatch.Presentation.PlayerBattleView;

namespace FightMatch.Core.Tests
{
    // Real PlayerSave/session/content path. Storage faults are bounded in memory; no physical Player cold-start claim.
    internal sealed class PlayerBattleRig : IDisposable
    {
        internal readonly NavigationRig N;
        internal readonly PublishedSource Source;
        internal PlayerBattleController Host;
        internal BattlePage Page;
        internal int ClockReads, Elapsed;
        internal CandidateApplicationSnapshot Head => N.Head;
        internal BattleSnapshot State => Head.Business.ActiveHistory?.CurrentRun.CurrentSnapshot;
        internal PlayerBattleSession Session => Host.Session;
        internal FightMatch.Application.PlayerBattleView View => Host.Refresh();
        internal PlayerBattleRig(bool enter = true, bool initialize = true, PublishedContentCatalog catalog = null,
            ResolvedPublication publication = null, PublishedSource source = null)
        {
            Source = source ?? RealSource(); N = new NavigationRig(initialize, catalog: catalog, publication: publication);
            BindHost(); if (enter) Enter();
        }
        internal CandidateTimeSample Clock() { ClockReads++; return Time(Elapsed); }
        private void BindHost() { Host = new PlayerBattleController(N.Player, N.Battle, Budget(), Clock); Page = new BattlePage(); Page.Bind(Host); }
        internal PlayerNavigationHostRequest Selection()
        { N.PreparePage(); return N.Go(PlayerNavigationTargetKind.BattleSelectionRequested).HostRequest; }
        internal void Enter()
        {
            var actual = Host.AcceptHost(Selection());
            Assert.IsNotNull(actual.Result, actual.Status); Is(actual.Result); Assert.AreEqual(PlayerBattleRoute.Battle, actual.Route);
            Assert.AreEqual(CandidateApplicationKind.EnterFormation, actual.OriginalIntent.Kind);
        }
        internal void RebuildView()
        { Page.Dispose(); Page = null; Page = new BattlePage(); Page.Bind(Host); }
        internal CandidateApplicationCallResult RebuildObjects()
        {
            Page.Dispose(); Page = null; Host.Dispose(); Host = null;
            var opened = N.Rebuild(); BindHost(); return opened;
        }
        internal List<FlowPos> Route(BattlePairKey pair)
        {
            var entry = State.Baseline.Entry;
            var source = Source.Levels.Single(x => x.Level.LevelId == entry.Level.LevelId && x.Level.LevelVersion == entry.Level.LevelVersion);
            var face = entry.Level.Faces.Single(x => x.FaceId == pair.FaceId);
            return source.SourceRoutes.Single(x => x.FaceId == pair.FaceId && x.PairId == pair.PairId).Cells
                .Select(c => new FlowPos(c.x - 1, Source.Coordinates == DemoCoordinateCandidate.AssumedBottomLeft ? c.y - 1 : face.Height - c.y)).ToList();
        }
        internal static PointerSample Sample(FlowPos cell, int pointer = 0) => new PointerSample(pointer, cell.x * 40, cell.y * 40, cell);
        internal static void Draw(CandidateBoardInputController input, IReadOnlyList<FlowPos> route)
        {
            Gesture(input, "Down", Sample(route[0]), 6d);
            for (var i = 1; i < route.Count - 1; i++) Gesture(input, "Move", Sample(route[i]));
            Gesture(input, "Up", Sample(route[route.Count - 1]));
        }
        // Invoke the same internal gesture entry points as CandidateBoardElement; never inspect or inject private state.
        private static void Gesture(CandidateBoardInputController input, string method, params object[] arguments)
        { typeof(CandidateBoardInputController).GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(input, arguments); }
        internal CandidateBattleCallResult Step(bool finish = true)
        {
            var state = State; Assert.IsNotNull(state);
            var pair = state.Phase == BattlePhase.AwaitLinks ? state.Board.PendingLinks[0] : state.Enemies.First(x => x.Hp.Numerator.Sign > 0).PairKey;
            if (state.Phase == BattlePhase.AwaitAction) Assert.IsTrue(Host.Input.SelectMember(state.Members.First(x => x.Hp.Numerator.Sign > 0).Member.CharacterId));
            Draw(Host.Input, Route(pair)); var result = Host.Input.LastResult; Assert.IsNotNull(result, Host.Input.Status);
            if (finish && result.Application.IsCommitted) Host.Playback.SkipToFinal();
            return result;
        }
        internal void Win(bool finishLast = true)
        {
            for (var i = 0; State.Phase != BattlePhase.WonPendingSettlement; i++)
            {
                Assert.Less(i, 64, "Bounded real attacks/links must reach the saved victory");
                Assert.That(State.Phase, NUnit.Framework.Is.EqualTo(BattlePhase.AwaitAction).Or.EqualTo(BattlePhase.AwaitLinks));
                PlayerSessionTestData.Is(Step(false).Application);
                if (finishLast || State.Phase != BattlePhase.WonPendingSettlement) Host.Playback.SkipToFinal();
            }
            Assert.IsNotNull(Head.Continuation); Host.Refresh();
        }
        internal FightMatch.Application.PlayerBattleView Settle()
        { var current = View; return Host.Settle(Page.Page, current.Context); }
        internal FightMatch.Application.PlayerBattleView End(bool restart = false)
        {
            var current = View; var preview = Host.PreviewEnd(Page.Page,
                restart ? CandidateApplicationKind.RestartAttempt : CandidateApplicationKind.ExitAttempt, current.Context);
            Assert.IsNotNull(preview.Confirmation, preview.Status); return Host.Confirm(Page.Page, preview.Confirmation);
        }
        internal FightMatch.Application.PlayerBattleView Continue(PlayerBattleRecoveryAction action)
        { var current = View; return Host.Continue(Page.Page, action, current.OriginalIntent, current.Context); }
        internal void Unchanged(CandidateApplicationSnapshot head, Dictionary<string, byte[]> files, int clocks)
        { Assert.AreSame(head, Head); SameFiles(files, N.Storage.Inner.Files); Assert.AreEqual(clocks, ClockReads); }
        internal void Receipt(FightMatch.Application.PlayerBattleView view)
        {
            Assert.AreEqual(PlayerBattleRoute.Result, view.Route, view.Status); Assert.IsNotNull(view.Receipt);
            Assert.IsTrue(view.Result.IsCommitted); Assert.IsTrue(view.Result.OriginalLookup.IsFound);
            Assert.AreEqual(Head.Header.CommitId, view.Result.LookupViewCommitId);
            Assert.AreSame(view.Result.OriginalLookup, view.Receipt.Lookup);
            Assert.AreEqual(view.Receipt.AttemptId, view.Receipt.End.Begin.AttemptId);
        }
        public void Dispose() { Page?.Dispose(); Host?.Dispose(); N.Dispose(); }

        internal static PlayerBattleRig TwoFaces()
        {
            var source = PublishedContentTestData.Fixture(); var second = PublishedContentTestData.Copy(source).Levels[0];
            second.Level.Faces[0].FaceId = "fixture:second-face";
            source.Levels[0].Level.Faces.Add(second.Level.Faces[0]);
            foreach (var route in second.SourceRoutes) { route.FaceId = "fixture:second-face"; source.Levels[0].SourceRoutes.Add(route); }
            return Isolated(source);
        }
        internal static PlayerBattleRig Recoverable()
        {
            var source = PublishedContentTestData.Fixture(); var pairs = source.Levels[0].Level.Faces[0].Pairs;
            source.Growth.BaseStats.AttackRange = 2;
            Assert.AreEqual(2, pairs.Count);
            pairs[0].Enemy.Stats.MaxHp = R(1); pairs[0].Enemy.Stats.Attack = R(1000);
            pairs[1].Enemy.Stats.MaxHp = R(1); pairs[1].Enemy.Stats.Attack = R(0);
            return Isolated(source);
        }
        internal static PlayerBattleRig DurableReference()
        {
            var source = PublishedContentTestData.Fixture();
            foreach (var pair in source.Levels[0].Level.Faces[0].Pairs) { pair.Enemy.Stats.MaxHp = R(40); pair.Enemy.Stats.Attack = R(0); }
            return Isolated(source);
        }
        private static PlayerBattleRig Isolated(PublishedSource source)
        {
            var storage = new PublishedContentTestData.MemoryStorage(); var catalog = new PublishedContentCatalog(storage, ContentConsumerCapabilities.Current);
            var job = new DemoContentDraft(source.DraftId).BeginJob(); var prepared = PublishedContentTestData.Prepare(source, job);
            var publication = PlayerSessionTestData.Content(catalog.Publish(prepared, prepared.Validation, PublishedContentTestData.Review(prepared), job,
                "fixture:028-isolated", PublishedContentTestData.StoreBudget())).Publication;
            storage.Blobs[PublishedContentCatalog.ReleaseSetKey("player", Release)] = PlayerSessionTestData.Content(PublishedContentCodec.EncodeReleaseSet(new ContentReleaseSet {
                SchemaVersion = 1, Scope = "player", ReleaseSetId = Release, Binding = ContentBindingRecord.From(publication.Binding),
                PublicationReceiptSha256 = PublishedContentCodec.Sha256(publication.ReceiptBytes.ToArray()) }, PublishedContentTestData.StoreBudget()));
            return new PlayerBattleRig(catalog: catalog, publication: Resolve(catalog), source: source);
        }
    }
    internal sealed class PlayerBattlePanel : IDisposable
    {
        internal readonly CandidateBoardTestWindow Window;
        private readonly PlayerBattleRig rig;
        internal PlayerBattlePanel(PlayerBattleRig rig)
        {
            this.rig = rig; Window = ScriptableObject.CreateInstance<CandidateBoardTestWindow>();
            Window.position = new Rect(50, 50, 900, 1000);
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
            {
                LogAssert.Expect(LogType.Error, "No graphic device is available to initialize the view.");
                LogAssert.Expect(LogType.Error, "No graphic device is available to show the window.");
                LogAssert.Expect(LogType.Error, "No graphic device is available to initialize the view.");
            }
            Window.Show(); Window.rootVisualElement.style.width = 900; Window.rootVisualElement.style.height = 1000; ShowPage();
        }
        internal void ShowPage()
        {
            Window.rootVisualElement.Clear(); Window.rootVisualElement.Add(rig.Page);
            var board = rig.Page.PlaybackView.InputView.Board; var face = rig.State?.Board.Face;
            if (face == null || board == null) return;
            board.style.flexGrow = 0; board.style.flexShrink = 0; board.style.minHeight = 0;
            board.style.width = face.Width * 40; board.style.height = face.Height * 40;
        }
        internal IEnumerator Ready()
        {
            yield return null; yield return null; yield return null;
            Assert.IsNotNull(rig.Page.panel); rig.Page.panel.Pick(Vector2.zero);
            var board = rig.Page.PlaybackView.InputView.Board; Assert.Greater(board.contentRect.width, 0);
        }
        internal static void Click(Button button)
        { Assert.IsNotNull(button); using (var e = NavigationSubmitEvent.GetPooled()) { e.target = button; button.SendEvent(e); } }
        internal void ClickStale(Button button)
        { Window.rootVisualElement.Add(button); Click(button); button.RemoveFromHierarchy(); }
        internal void Pointer(FlowPos cell, int kind)
        {
            var board = rig.Page.PlaybackView.InputView.Board; var rect = board.contentRect;
            var point = board.LocalToWorld(new Vector2(rect.x + (cell.x + .5f) * 40, rect.y + (rig.State.Board.Face.Height - cell.y - .5f) * 40));
            var sample = new BattlePointer(point, kind == 2 ? 0 : 1);
            if (kind == 0) using (var e = PointerDownEvent.GetPooled(sample)) { e.target = board; board.SendEvent(e); }
            else if (kind == 1) using (var e = PointerMoveEvent.GetPooled(sample)) { e.target = board; board.SendEvent(e); }
            else using (var e = PointerUpEvent.GetPooled(sample)) { e.target = board; board.SendEvent(e); }
        }
        public void Dispose()
        {
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
                LogAssert.Expect(LogType.Error, "No graphic device is available to initialize the view.");
            Window.Close();
        }
        private sealed class BattlePointer : IPointerEvent
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
            internal BattlePointer(Vector2 point, int buttons) { position = point; pressedButtons = buttons; }
        }
    }
}
