using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FightMatch.Application;
using FightMatch.Content;
using FightMatch.Core;
using FightMatch.Host;
using FightMatch.Presentation;
using FlowPuzzle.Core;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using static FightMatch.Core.Tests.PlayerSessionTestData;

namespace FightMatch.Core.Tests
{
    // Actual Host/HostView/Prefab with bounded in-memory persistence; no mirrored routing or diagnostic gate.
    internal sealed class UguiHostRig : IDisposable
    {
        internal readonly GameObject HostRoot;
        internal readonly FightMatchPlayerHost Host;
        internal readonly FightMatchHostSession Session;
        internal readonly UguiPointerDriver Driver;
        internal readonly LocalizationService Localization;
        internal readonly FightMatchHostView View;
        internal readonly string License;
        internal MemorySave Storage;
        private readonly EventSystem[] previousEvents;
        internal GameObject Root => View.gameObject;
        internal CandidateBoardElement Board => View.BattleView.PlaybackView.InputView.Board;
        internal CandidateApplicationSnapshot Head => Session.Application.QueryView().View.PublishedSnapshot;
        internal BattleSnapshot State => Head?.Business.ActiveHistory?.CurrentRun.CurrentSnapshot;
        internal UguiHostRig(bool create = false, ILocalizedTextSource source = null, bool bind = true)
        {
            var path = Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, "../TestArtifacts/FightMatch/UGUI-01/host-memory", Guid.NewGuid().ToString("N")));
            Assert.IsFalse(Directory.Exists(path));
            Session = new FightMatchHostSession(RealCatalog(), path, new NavigationProfileStorage(), id => Storage ?? (Storage = new MemorySave(id)));
            Session.ObserveStartup();
            if (create) { Assert.IsTrue(Session.CanCreate); Session.CreateProfile(); Assert.IsNotNull(Head); }
            Localization = new LocalizationService(source ?? new UguiTestTextSource(), SystemLanguage.English);
            previousEvents = UnityEngine.Object.FindObjectsOfType<EventSystem>().Where(x => x.enabled).ToArray();
            foreach (var previous in previousEvents) previous.enabled = false;
            Driver = new UguiPointerDriver();
            HostRoot = new GameObject("UguiHostFixture"); HostRoot.SetActive(false);
            Host = HostRoot.AddComponent<FightMatchPlayerHost>(); Host.enabled = false;
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/UI/FightMatch/Runtime/FightMatchRuntimeRoot.prefab");
            Assert.IsNotNull(prefab, "Generate and reopen the approved prefab before this target run.");
            var root = UnityEngine.Object.Instantiate(prefab, HostRoot.transform); root.SetActive(false);
            var raycasters = root.GetComponentsInChildren<UnityEngine.UI.GraphicRaycaster>(true); Assert.AreEqual(1, raycasters.Length);
            raycasters[0].runInEditMode = true;
            View = root.GetComponent<FightMatchHostView>(); Assert.IsNotNull(View);
            var font = AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>("Assets/UI/FightMatch/Fonts/NotoSansCJKsc-Regular-TMP.asset");
            var license = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/UI/FightMatch/Fonts/OFL.txt");
            Assert.IsNotNull(font); Assert.IsNotNull(license); License = license.text;
            SetReference(Host, "runtimeRoot", View); SetReference(Host, "fontAsset", font); SetReference(Host, "fontLicense", license);
            HostRoot.SetActive(true);
            if (bind) Bind();
        }
        internal static void SetReference(UnityEngine.Object target, string field, UnityEngine.Object value)
        {
            var serialized = new SerializedObject(target); var property = serialized.FindProperty(field);
            Assert.IsNotNull(property, field); property.objectReferenceValue = value; serialized.ApplyModifiedPropertiesWithoutUndo();
        }
        internal void Bind()
        {
            Root.SetActive(false); View.Bind(Session, Localization, License); Root.SetActive(true); Canvas.ForceUpdateCanvases();
        }
        internal System.Collections.IEnumerator Ready() => Ready(Board.rectTransform, false);
        internal System.Collections.IEnumerator Ready(RectTransform target) => Ready(target, true);
        private static System.Collections.IEnumerator Ready(RectTransform target, bool requireActive)
        {
            yield return null; yield return null; yield return null;
            Canvas.ForceUpdateCanvases();
            if (requireActive) Assert.IsTrue(target.gameObject.activeInHierarchy);
            var canvas = target.GetComponentInParent<Canvas>(true);
            Assert.IsNotNull(canvas);
            Assert.AreEqual(1, canvas.GetComponents<UnityEngine.UI.GraphicRaycaster>().Length);
            Assert.Greater(target.rect.width, 0); Assert.Greater(target.rect.height, 0);
        }
        internal T Find<T>(string id) where T : Component
        { var result = FightMatchViewId.Find<T>(Root.transform, id); Assert.IsNotNull(result, id); return result; }
        internal void Enter()
        {
            var navigation = Session.Navigation;
            navigation.Refresh(); var level = navigation.View.Read.Levels[0];
            navigation.NavigationHandler(new PlayerNavigationTarget { Kind = PlayerNavigationTargetKind.Preparation,
                LevelId = level.LevelId, LevelVersion = level.LevelVersion, Binding = navigation.View.Read.Binding })();
            navigation.NavigationHandler(new PlayerNavigationTarget { Kind = PlayerNavigationTargetKind.BattleSelectionRequested })();
            Assert.AreEqual(FightMatchHostPage.Battle, Session.Page);
            Canvas.ForceUpdateCanvases();
            Assert.IsFalse(View.DiagnosticVisible, View.DiagnosticCode);
            Assert.IsTrue(Session.Battle.Input.SelectMember(State.Members.First(x => x.Hp.Numerator.Sign > 0).Member.CharacterId));
        }
        internal IReadOnlyList<FlowPos> Route()
        {
            var pair = State.Enemies.First(x => x.Hp.Numerator.Sign > 0).PairKey;
            var source = RealSource(); var level = source.Levels.Single(x => x.Level.LevelId == State.Baseline.Entry.Level.LevelId);
            var face = State.Baseline.Entry.Level.Faces.Single(x => x.FaceId == pair.FaceId);
            return level.SourceRoutes.Single(x => x.FaceId == pair.FaceId && x.PairId == pair.PairId).Cells.Select(p =>
                new FlowPos(p.x - 1, source.Coordinates == DemoCoordinateCandidate.AssumedBottomLeft ? p.y - 1 : face.Height - p.y)).ToArray();
        }
        internal void BeginRoute(int pointer = 1)
        {
            var route = Route(); var start = UguiPointerDriver.Point(Board, route[0]);
            var hits = Driver.Raycast(start);
            Assert.IsNotEmpty(hits); Assert.AreEqual(Board.gameObject, hits[0].gameObject);
            Driver.Down(start, pointer);
            for (var i = 1; i < route.Count; i++) Driver.Move(UguiPointerDriver.Point(Board, route[i]), pointer);
            Assert.IsTrue(Board.HasActivePointer);
        }
        internal void EndRoute(int pointer = 1) { Driver.Up(UguiPointerDriver.Point(Board, Route().Last()), pointer); }
        public void Dispose()
        {
            View?.Unbind();
            if (HostRoot != null) UnityEngine.Object.DestroyImmediate(HostRoot);
            Driver?.Dispose();
            foreach (var previous in previousEvents) if (previous != null) previous.enabled = true;
            Session?.Dispose();
        }
    }

    internal sealed class UguiReleaseObservation : MonoBehaviour, IPointerUpHandler, IEndDragHandler
    {
        internal readonly List<string> Events = new List<string>();
        internal CandidateBoardElement Board;
        public void OnPointerUp(PointerEventData data) { Events.Add("PointerUp:" + Board.HasActivePointer); }
        public void OnEndDrag(PointerEventData data) { Events.Add("EndDrag:" + Board.HasActivePointer); }
    }

    public sealed class UguiSceneCompositionTests
    {
        [Test]
        public void SavedPrefabAssemblesOneRealHostCanvasAndProjectEventSystem()
        {
            using (var rig = new UguiHostRig(bind: false))
            {
                Assert.AreEqual(1, rig.HostRoot.GetComponentsInChildren<FightMatchPlayerHost>(true).Length);
                Assert.AreEqual(1, rig.Root.GetComponentsInChildren<Canvas>(true).Length);
                Assert.AreEqual(0, rig.Root.GetComponentsInChildren<EventSystem>(true).Length);
                Assert.AreEqual(1, rig.Driver.EventSystem.GetComponents<BaseInputModule>().Length);
                Assert.IsInstanceOf<FightMatchStandaloneInputModule>(rig.Driver.Module);
                Assert.AreSame(rig.View, rig.Host.RuntimeRoot);
                Assert.IsTrue(FightMatchViewId.Validate(rig.Root.transform, out var diagnostic), diagnostic);
                foreach (var id in new[] { "fm.page.startup", "fm.page.navigation", "fm.page.battle", "fm.page.result",
                    "fm.section.map", "fm.section.party", "fm.section.inventory", "fm.section.preparation", "fm.board.candidate",
                    "fm.popup.language", "fm.popup.reference", "fm.popup.confirmation", "fm.popup.recovery", "fm.popup.license", "fm.popup.quit", "fm.popup.blocking" })
                    Assert.IsNotNull(rig.Find<FightMatchViewId>(id));
                var texts = rig.Root.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true); var linkedInputs = 0;
                foreach (var text in texts)
                {
                    var input = text.GetComponentInParent<TMPro.TMP_InputField>(true);
                    if (input != null && ReferenceEquals(input.textComponent, text))
                    {
                        linkedInputs++; Assert.AreEqual(LocalizedTmpText.Placeholder, input.text);
                        Assert.That(text.text, NUnit.Framework.Is.EqualTo(LocalizedTmpText.Placeholder).Or.EqualTo(LocalizedTmpText.Placeholder + "\u200B"));
                    }
                    else Assert.AreEqual(LocalizedTmpText.Placeholder, text.text);
                }
                Assert.AreEqual(119, texts.Length); Assert.AreEqual(3, linkedInputs); Assert.AreEqual(116, texts.Length - linkedInputs);
                Assert.AreEqual(0, rig.Root.GetComponentsInChildren<UnityEngine.UIElements.UIDocument>(true).Length);
                rig.Bind();
                Assert.IsFalse(rig.View.DiagnosticVisible, rig.View.DiagnosticCode);
                Assert.IsTrue(rig.Find<FightMatchViewId>("fm.page.startup").gameObject.activeInHierarchy);
            }
        }

        [UnityEngine.TestTools.UnityTest]
        public System.Collections.IEnumerator OwnerEndedCommitsOnceDespiteBothPointerUpAndEndDrag()
        {
            using (var rig = new UguiHostRig(true))
            {
                rig.Enter();
                yield return rig.Ready();
                var before = rig.Storage.SnapshotCreates;
                var observe = rig.Board.gameObject.AddComponent<UguiReleaseObservation>(); observe.Board = rig.Board;
                rig.BeginRoute(); rig.EndRoute();
                Assert.IsTrue(rig.Session.Battle.Input.LastResult.Application.IsCommitted);
                Assert.AreEqual(before + 1, rig.Storage.SnapshotCreates);
                CollectionAssert.Contains(observe.Events, "PointerUp:False"); CollectionAssert.Contains(observe.Events, "EndDrag:False");
                Assert.IsFalse(rig.Board.HasActivePointer);
            }
        }

        [UnityEngine.TestTools.UnityTest]
        public System.Collections.IEnumerator OwnerCanceledNormalizesBeforeReleaseAndNeverCommits()
        {
            using (var rig = new UguiHostRig(true))
            {
                rig.Enter(); var head = rig.Head; var before = rig.Storage.SnapshotCreates;
                var observe = rig.Board.gameObject.AddComponent<UguiReleaseObservation>(); observe.Board = rig.Board;
                yield return rig.Ready();
                rig.BeginRoute();
                Action changed = () => { if (!rig.Board.HasActivePointer) observe.Events.Add("OwnerCanceled"); };
                rig.Session.Battle.Input.Changed += changed;
                try { rig.Driver.Cancel(1, UguiPointerDriver.Point(rig.Board, rig.Route().Last())); }
                finally { rig.Session.Battle.Input.Changed -= changed; }
                Assert.AreSame(head, rig.Head); Assert.AreEqual(before, rig.Storage.SnapshotCreates);
                Assert.IsFalse(rig.Board.HasActivePointer);
                var canceled = observe.Events.IndexOf("OwnerCanceled"); var up = observe.Events.IndexOf("PointerUp:False");
                Assert.GreaterOrEqual(canceled, 0); Assert.Greater(up, canceled);
                CollectionAssert.Contains(observe.Events, "EndDrag:False");
                rig.EndRoute(); Assert.AreEqual(before, rig.Storage.SnapshotCreates);
                TestContext.Out.WriteLine(string.Join(" -> ", observe.Events));
            }
        }

        [UnityEngine.TestTools.UnityTest]
        public System.Collections.IEnumerator SecondFingerCanceledLeavesOwnerAndOwnerEndedStillCommitsOnce()
        {
            using (var rig = new UguiHostRig(true))
            {
                rig.Enter(); var before = rig.Storage.SnapshotCreates;
                yield return rig.Ready();
                rig.BeginRoute(1);
                var other = UguiPointerDriver.Point(rig.Board, rig.Route()[0]);
                rig.Driver.Down(other, 2); rig.Driver.Cancel(2, other);
                Assert.IsTrue(rig.Board.HasActivePointer);
                Assert.AreEqual(1, rig.Session.Battle.Input.Gesture.ActivePointerId);
                Assert.AreEqual(before, rig.Storage.SnapshotCreates);
                rig.EndRoute(1); Assert.AreEqual(before + 1, rig.Storage.SnapshotCreates);
            }
        }

        [Test]
        public void FourCornerCentersAndHalfOpenEdgesUseTheActualBoardMapper()
        {
            using (var rig = new UguiHostRig(true))
            {
                rig.Enter(); var board = rig.Board; var face = rig.State.Board.Face;
                foreach (var cell in new[] { new FlowPos(0, 0), new FlowPos(face.Width - 1, 0),
                    new FlowPos(0, face.Height - 1), new FlowPos(face.Width - 1, face.Height - 1) })
                    Assert.AreEqual(cell, board.CellAtLocal(board.CellCenter(cell)));
                var rect = board.rectTransform.rect; var size = Mathf.Min(rect.width / face.Width, rect.height / face.Height);
                Assert.AreEqual(new FlowPos(0, 0), board.CellAtLocal(new Vector2(rect.xMin, rect.yMin)));
                Assert.IsNull(board.CellAtLocal(new Vector2(rect.xMin - .01f, rect.yMin)));
                Assert.IsNull(board.CellAtLocal(new Vector2(rect.xMin, rect.yMin - .01f)));
                Assert.IsNull(board.CellAtLocal(new Vector2(rect.xMin + size * face.Width, rect.yMin)));
                Assert.IsNull(board.CellAtLocal(new Vector2(rect.xMin, rect.yMin + size * face.Height)));
                Assert.Greater(board.CellCenter(new FlowPos(0, 1)).y, board.CellCenter(new FlowPos(0, 0)).y);
            }
        }

        [Test]
        public void BothResolutionsWithFourInsetsKeepHitAndRenderedCentersIdentical()
        {
            using (var rig = new UguiHostRig(true))
            {
                rig.Enter(); var board = rig.Board; var canvas = rig.Root.GetComponentInChildren<Canvas>();
                canvas.GetComponent<UnityEngine.UI.CanvasScaler>().enabled = false; canvas.renderMode = RenderMode.WorldSpace;
                var canvasRect = (RectTransform)canvas.transform; var safe = rig.Root.GetComponentInChildren<SafeAreaFitter>(); safe.enabled = false;
                foreach (var resolution in new[] { new Vector2(540, 960), new Vector2(1080, 2400) })
                {
                    canvasRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, resolution.x);
                    canvasRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, resolution.y);
                    var inset = new Rect(17, 23, resolution.x - 17 - 29, resolution.y - 23 - 31);
                    safe.Apply(inset, resolution); Canvas.ForceUpdateCanvases();
                    Assert.AreEqual(new Vector2(inset.xMin / resolution.x, inset.yMin / resolution.y), ((RectTransform)safe.transform).anchorMin);
                    Assert.AreEqual(new Vector2(inset.xMax / resolution.x, inset.yMax / resolution.y), ((RectTransform)safe.transform).anchorMax);
                    board.SetVerticesDirty(); board.Rebuild(UnityEngine.UI.CanvasUpdate.PreRender);
                    var mesh = board.canvasRenderer.GetMesh();
                    Assert.IsNotNull(mesh); Assert.Greater(mesh.vertexCount, 0);
                    foreach (var pair in rig.State.Board.Face.Pairs)
                        foreach (var endpoint in new[] { pair.EndpointA, pair.EndpointB })
                        {
                            var center = board.CellCenter(endpoint);
                            Assert.AreEqual(endpoint, board.CellAtLocal(center));
                            Assert.IsTrue(mesh.vertices.Any(v => Vector2.Distance(v, center) < .001f), "The rendered endpoint must use CellCenter.");
                        }
                }
            }
        }
    }
}
