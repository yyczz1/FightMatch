using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FightMatch.Application;
using FightMatch.Content;
using FightMatch.Core;
using FightMatch.Platform;
using FlowPuzzle.Core;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace FightMatch.Host.Tests
{
    internal sealed class HostRig : IDisposable
    {
        private static PublishedContentCatalog catalog;
        internal static string Project => Path.GetDirectoryName(UnityEngine.Application.dataPath);
        internal static PublishedContentCatalog Catalog
        {
            get
            {
                if (catalog != null) return catalog;
                var files = FightMatchStreamingAssetsLoader.FileNames.Select(name => File.ReadAllBytes(
                    Path.Combine(UnityEngine.Application.streamingAssetsPath, "FightMatch", name))).ToArray();
                var result = FirstReleaseContentStorage.Create(files[0], files[1], files[2], files[3], files[4], files[5],
                    ContentConsumerCapabilities.Current, new ContentStoreBudget(new ExactMathBudget(maxPrimitiveSteps: 64000000)));
                Assert.IsTrue(result.IsAccepted, result.RejectionCode);
                return catalog = new PublishedContentCatalog(result.Value, ContentConsumerCapabilities.Current);
            }
        }
        internal readonly string Root;
        internal readonly ProfileFault Profile;
        internal SaveFault Storage;
        internal FightMatchHostSession Session;
        internal int FactoryCalls;
        internal Action BeforeStorage;
        internal CandidateApplicationSnapshot Head => Session.Application.QueryView().View.PublishedSnapshot;
        internal BattleSnapshot State => Head?.Business.ActiveHistory?.CurrentRun.CurrentSnapshot;
        internal HostRig(bool create = true)
        {
            var parent = Path.Combine(Project, "TestArtifacts/FMDemo029/mac-r1/host-io");
            Directory.CreateDirectory(parent);
            Assert.Less(Directory.GetDirectories(parent).Length, 256);
            Root = Path.Combine(parent, Guid.NewGuid().ToString("N"), "FightMatch");
            Directory.CreateDirectory(Root);
            Profile = new ProfileFault(new MacContentPublicationStorage(Path.Combine(Root, "locator")));
            Bind();
            if (create) Create();
        }
        internal void Bind()
        {
            Session = new FightMatchHostSession(Catalog, Root, Profile, id =>
            {
                FactoryCalls++;
                BeforeStorage?.Invoke();
                if (Storage == null) Storage = new SaveFault(new MacEditorSaveStorage(Path.Combine(Root, "profiles"), id, SavePurpose.PlayerSave));
                Assert.AreEqual(id, Storage.Profile.PlayerId);
                return Storage;
            });
        }
        internal void Create()
        {
            Session.ObserveStartup();
            Assert.IsTrue(Session.CanCreate, Session.Status);
            Session.CreateProfile();
            Assert.AreEqual(LocalPlayerProfileState.Active, Session.Observation.State, Session.Status);
            Assert.IsTrue(Session.Application.QueryView().View.IsPublishedHeadVerified);
        }
        internal void Rebuild()
        {
            Session.Dispose();
            Session = null;
            Storage = null;
            Bind();
            Session.ObserveStartup();
        }
        internal void Go(PlayerNavigationTargetKind kind, string operation = null)
        {
            Session.Navigation.NavigationHandler(new PlayerNavigationTarget { Kind = kind, OperationId = operation })();
        }
        internal PlayerNavigationHostRequest Enter()
        {
            var nav = Session.Navigation;
            nav.Refresh();
            var level = nav.View.Read.Levels[0];
            nav.NavigationHandler(new PlayerNavigationTarget { Kind = PlayerNavigationTargetKind.Preparation,
                LevelId = level.LevelId, LevelVersion = level.LevelVersion, Binding = nav.View.Read.Binding })();
            PlayerNavigationHostRequest request = null;
            Action<PlayerNavigationHostRequest> capture = value => request = value;
            nav.HostRequested += capture;
            Go(PlayerNavigationTargetKind.BattleSelectionRequested);
            nav.HostRequested -= capture;
            Assert.IsNotNull(request);
            Assert.IsNotNull(State, Session.Status);
            Assert.AreEqual(FightMatchHostPage.Battle, Session.Page);
            return request;
        }
        internal FightMatch.Application.PlayerBattleView End(FightMatchHostView view, bool restart = false)
        {
            var battle = Session.Battle;
            var preview = battle.PreviewEnd(view.BattleView.Page, restart ? CandidateApplicationKind.RestartAttempt :
                CandidateApplicationKind.ExitAttempt, battle.Refresh().Context);
            Assert.IsNotNull(preview.Confirmation, preview.Status);
            return battle.Confirm(view.BattleView.Page, preview.Confirmation);
        }
        internal Dictionary<string, byte[]> Files() => Directory.GetFiles(Root, "*", SearchOption.AllDirectories)
            .ToDictionary(x => x.Substring(Root.Length), File.ReadAllBytes);
        internal void SameFiles(Dictionary<string, byte[]> expected)
        {
            var actual = Files();
            CollectionAssert.AreEquivalent(expected.Keys, actual.Keys);
            foreach (var row in expected) CollectionAssert.AreEqual(row.Value, actual[row.Key], row.Key);
        }
        internal IReadOnlyList<FlowPos> Route(BattlePairKey pair)
        {
            var decoded = PublishedContentCodec.DecodeSource(File.ReadAllBytes(Path.Combine(UnityEngine.Application.streamingAssetsPath,
                "FightMatch/first-release.fmsource.json")), ContentConsumerCapabilities.Current, Session.Budget.Codec.Math);
            Assert.IsTrue(decoded.IsAccepted, decoded.RejectionCode);
            var source = decoded.Value;
            var level = source.Levels.Single(x => x.Level.LevelId == State.Baseline.Entry.Level.LevelId);
            var face = State.Baseline.Entry.Level.Faces.Single(x => x.FaceId == pair.FaceId);
            return level.SourceRoutes.Single(x => x.FaceId == pair.FaceId && x.PairId == pair.PairId).Cells.Select(c =>
                new FlowPos(c.x - 1, source.Coordinates == DemoCoordinateCandidate.AssumedBottomLeft ? c.y - 1 : face.Height - c.y)).ToArray();
        }
        public void Dispose() { Session?.Dispose(); }
    }

    internal sealed class ProfileFault : IContentPublicationStorage
    {
        private readonly IContentPublicationStorage inner;
        internal bool ReadFailure, FailCreateAfterWrite, FailActive;
        internal Action Writing;
        internal ProfileFault(IContentPublicationStorage inner) { this.inner = inner; }
        public IDisposable AcquireWriter() => inner.AcquireWriter();
        public byte[] Read(string key, int limit)
        {
            if (ReadFailure) throw new IOException("029 isolated locator read failure");
            return inner.Read(key, limit);
        }
        public void WriteImmutable(string key, byte[] bytes, int limit)
        {
            Writing?.Invoke();
            if (FailActive && key == LocalPlayerProfileLocator.ActiveRecordKey) throw new IOException("029 before Active");
            inner.WriteImmutable(key, bytes, limit);
            if (FailCreateAfterWrite && key == LocalPlayerProfileLocator.CreateRecordKey)
            {
                FailCreateAfterWrite = false;
                throw new IOException("029 after CreateIntent");
            }
        }
    }

    internal sealed class SaveFault : ILocalSaveStorage
    {
        private readonly ILocalSaveStorage inner;
        internal string Fault;
        internal SaveFault(ILocalSaveStorage inner) { this.inner = inner; }
        public SaveStorageProfile Profile => inner.Profile;
        public IDisposable AcquireWriterLease(bool create) => inner.AcquireWriterLease(create);
        public IEnumerable<string> EnumerateNames() => inner.EnumerateNames();
        public Stream OpenRead(string name) => inner.OpenRead(name);
        public Stream CreateWork(string name)
        {
            if (Fault == "save" && name.EndsWith(".snapshot.tmp", StringComparison.Ordinal))
            {
                Fault = null;
                throw new IOException("029 before snapshot");
            }
            return inner.CreateWork(name);
        }
        public void FlushFile(Stream stream) => inner.FlushFile(stream);
        public void PromoteNoReplace(string work, string final)
        {
            inner.PromoteNoReplace(work, final);
            if (Fault == "snapshot-after" && final.EndsWith(".snapshot", StringComparison.Ordinal))
            {
                Fault = null;
                throw new IOException("029 snapshot promoted before marker");
            }
            if (Fault == "marker-after" && final.EndsWith(".commit", StringComparison.Ordinal))
            {
                Fault = null;
                throw new IOException("029 marker published, response lost");
            }
        }
        public void DeleteUncommitted(string name) => inner.DeleteUncommitted(name);
        public void DeleteIndexedOld(string name) => inner.DeleteIndexedOld(name);
    }

    internal sealed class HostTestWindow : EditorWindow { }
    internal sealed class HostPanel : IDisposable
    {
        internal readonly HostTestWindow Window;
        internal FightMatchHostView View;
        private readonly HostRig rig;
        private static bool NoGraphics => SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null;
        internal HostPanel(HostRig rig)
        {
            this.rig = rig;
            Window = ScriptableObject.CreateInstance<HostTestWindow>();
            Window.position = new Rect(40, 40, 900, 1200);
            if (NoGraphics)
            {
                LogAssert.Expect(LogType.Error, "No graphic device is available to initialize the view.");
                LogAssert.Expect(LogType.Error, "No graphic device is available to show the window.");
                LogAssert.Expect(LogType.Error, "No graphic device is available to initialize the view.");
            }
            Window.Show();
            Window.rootVisualElement.style.width = 900;
            Window.rootVisualElement.style.height = 1200;
            Rebind();
        }
        internal void Rebind()
        {
            View?.Dispose();
            Window.rootVisualElement.Clear();
            View = new FightMatchHostView(rig.Session, "test license");
            Window.rootVisualElement.Add(View);
        }
        internal IEnumerator Ready()
        {
            yield return null;
            yield return null;
            yield return null;
            Assert.IsNotNull(View.panel);
            View.panel.Pick(Vector2.zero);
            var board = View.BattleView.PlaybackView.InputView.Board;
            Assert.Greater(board.contentRect.width, 0);
            Assert.Greater(board.contentRect.height, 0);
        }
        internal void Draw(IReadOnlyList<FlowPos> route)
        {
            var board = View.BattleView.PlaybackView.InputView.Board;
            var face = rig.State.Board.Face;
            var cell = Math.Min(board.contentRect.width / face.Width, board.contentRect.height / face.Height);
            var left = board.contentRect.x + (board.contentRect.width - face.Width * cell) / 2;
            var top = board.contentRect.y + (board.contentRect.height - face.Height * cell) / 2;
            for (var i = 0; i < route.Count; i++)
            {
                var pos = route[i];
                var point = board.LocalToWorld(new Vector2(left + (pos.x + .5f) * cell, top + (face.Height - pos.y - .5f) * cell));
                var pointer = new HostPointer(point, i == route.Count - 1 ? 0 : 1);
                if (i == 0) using (var evt = PointerDownEvent.GetPooled(pointer)) { evt.target = board; board.SendEvent(evt); }
                else if (i == route.Count - 1) using (var evt = PointerUpEvent.GetPooled(pointer)) { evt.target = board; board.SendEvent(evt); }
                else using (var evt = PointerMoveEvent.GetPooled(pointer)) { evt.target = board; board.SendEvent(evt); }
            }
        }
        public void Dispose()
        {
            View.Dispose();
            if (NoGraphics) LogAssert.Expect(LogType.Error, "No graphic device is available to initialize the view.");
            Window.Close();
        }
        private sealed class HostPointer : IPointerEvent
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
            internal HostPointer(Vector2 position, int buttons) { this.position = position; pressedButtons = buttons; }
        }
    }
}
