using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FightMatch.Application;
using FightMatch.Content;
using FightMatch.Input;
using FightMatch.Presentation;
using FlowPuzzle.Core;
using NUnit.Framework;
using UnityEngine;
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
        internal readonly BattleUguiRoot Canvas;
        internal int ClockReads, Elapsed;
        internal CandidateApplicationSnapshot Head => N.Head;
        internal BattleSnapshot State => Head.Business.ActiveHistory?.CurrentRun.CurrentSnapshot;
        internal PlayerBattleSession Session => Host.Session;
        internal FightMatch.Application.PlayerBattleView View => Host.Refresh();
        internal PlayerBattleRig(bool enter = true, bool initialize = true, PublishedContentCatalog catalog = null,
            ResolvedPublication publication = null, PublishedSource source = null)
        {
            Source = source ?? RealSource(); N = new NavigationRig(initialize, catalog: catalog, publication: publication);
            Canvas = new BattleUguiRoot();
            BindHost(); if (enter) Enter();
        }
        internal CandidateTimeSample Clock() { ClockReads++; return Time(Elapsed); }
        private void BindHost() { Host = new PlayerBattleController(N.Player, N.Battle, Budget(), Clock); Page = Canvas.Page(); Page.Bind(Host, Canvas.Localization); }
        internal PlayerNavigationHostRequest Selection()
        { N.PreparePage(); return N.Go(PlayerNavigationTargetKind.BattleSelectionRequested).HostRequest; }
        internal void Enter()
        {
            var actual = Host.AcceptHost(Selection());
            Assert.IsNotNull(actual.Result, actual.Status); Is(actual.Result); Assert.AreEqual(PlayerBattleRoute.Battle, actual.Route);
            Assert.AreEqual(CandidateApplicationKind.EnterFormation, actual.OriginalIntent.Kind);
        }
        internal IEnumerator Ready()
        {
            yield return null; yield return null; yield return null;
            UnityEngine.Canvas.ForceUpdateCanvases();
            var board = Page.PlaybackView.InputView.Board; Assert.IsNotNull(board.canvas);
            Assert.AreEqual(1, board.canvas.GetComponents<UnityEngine.UI.GraphicRaycaster>().Length);
            Assert.Greater(board.rectTransform.rect.width, 0); Assert.Greater(board.rectTransform.rect.height, 0);
        }
        internal void RebuildView()
        { Canvas.RemovePage(Page); Page = Canvas.Page(); Page.Bind(Host, Canvas.Localization); }
        internal CandidateApplicationCallResult RebuildObjects()
        {
            Canvas.RemovePage(Page); Page = null; Host.Dispose(); Host = null;
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
        internal void Draw(IReadOnlyList<FlowPos> route)
        {
            var board = Page.PlaybackView.InputView.Board;
            Canvas.Driver.Down(UguiPointerDriver.Point(board, route[0]));
            for (var i = 1; i < route.Count - 1; i++) Canvas.Driver.Move(UguiPointerDriver.Point(board, route[i]));
            Canvas.Driver.Up(UguiPointerDriver.Point(board, route[route.Count - 1]));
        }
        internal CandidateBattleCallResult Step(bool finish = true)
        {
            var state = State; Assert.IsNotNull(state);
            var pair = state.Phase == BattlePhase.AwaitLinks ? state.Board.PendingLinks[0] : state.Enemies.First(x => x.Hp.Numerator.Sign > 0).PairKey;
            if (state.Phase == BattlePhase.AwaitAction) Assert.IsTrue(Host.Input.SelectMember(state.Members.First(x => x.Hp.Numerator.Sign > 0).Member.CharacterId));
            Draw(Route(pair)); var result = Host.Input.LastResult; Assert.IsNotNull(result, Host.Input.Status);
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
        public void Dispose() { Page?.Dispose(); Host?.Dispose(); Canvas?.Dispose(); N.Dispose(); }

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
        private readonly PlayerBattleRig rig;
        internal PlayerBattlePanel(PlayerBattleRig rig) { this.rig = rig; ShowPage(); }
        internal void ShowPage()
        {
            var board = rig.Page.PlaybackView.InputView.Board; var face = rig.State?.Board.Face;
            if (face == null || board == null) return;
            board.rectTransform.sizeDelta = new Vector2(face.Width * 40, face.Height * 40);
        }
        internal IEnumerator Ready() => rig.Ready();
        internal static void Click(UnityEngine.UI.Button button) { BattleUguiRoot.Submit(button); }
        internal void ClickStale(UnityEngine.UI.Button button)
        { Assert.IsNotNull(button); rig.Canvas.Keep(button); button.gameObject.SetActive(true); Click(button); }
        internal UnityEngine.UI.Button Keep(UnityEngine.UI.Button button) { rig.Canvas.Keep(button); return button; }
        internal void Pointer(FlowPos cell, int kind)
        {
            var point = UguiPointerDriver.Point(rig.Page.PlaybackView.InputView.Board, cell);
            if (kind == 0) rig.Canvas.Driver.Down(point);
            else if (kind == 1) rig.Canvas.Driver.Move(point);
            else rig.Canvas.Driver.Up(point);
        }
        public void Dispose() { if (rig.Page != null) rig.Page.gameObject.SetActive(false); }
    }
}
