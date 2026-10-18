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
            Localization = new LocalizationService(source ?? RealTextSource(), SystemLanguage.English);
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
        private static ILocalizedTextSource RealTextSource()
        {
            var bytes = File.ReadAllBytes(Path.Combine(UnityEngine.Application.dataPath, "../Generated/FightMatch/Localization/fm-text-v1.json"));
            Assert.IsTrue(FightMatchLocalizationCatalogParser.TryParse(bytes,
                "f48264f9ce94cd3bb2ef8d1b6a0be7b241916144b6d5764d11633bb6d6e456bc", 264, out var source, out var code), code);
            return source;
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
        private bool enteredPlayMode;

        [UnityEngine.TestTools.UnityTest]
        public System.Collections.IEnumerator BattleCopyFitsThroughLocalizedActivePlaybackAndPendingVictory()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            yield return new UnityEngine.TestTools.EnterPlayMode(); enteredPlayMode = true;
            var measured = new HashSet<string>(); var failures = new HashSet<string>();
            foreach (var safeHeight in new[] { 920f, 812f })
            foreach (var locale in new[] { LocaleId.En, LocaleId.ZhHans })
            using (var rig = new UguiHostRig(true, bind: false))
            {
                // Isolate the real prefab's minimum/reference geometry from the Editor window size.
                // Device evidence separately covers the overlay Canvas and physical touch alignment.
                var canvas = rig.Root.GetComponentInChildren<Canvas>(true);
                canvas.GetComponent<UnityEngine.UI.CanvasScaler>().enabled = false;
                canvas.renderMode = RenderMode.WorldSpace; canvas.transform.localScale = Vector3.one; canvas.scaleFactor = 1;
                var fitter = rig.Root.GetComponentInChildren<SafeAreaFitter>(true); fitter.enabled = false;
                var safe = (RectTransform)fitter.transform; safe.anchorMin = safe.anchorMax = Vector2.zero;
                safe.sizeDelta = new Vector2(508, safeHeight);
                rig.Localization.SetLocale(locale); rig.Bind(); rig.Enter();
                yield return null; yield return null;
                var input = rig.Session.Battle.Input;
                for (var action = 0; ; action++)
                {
                    MeasureBattleCopy(rig, measured, failures);
                    Assert.AreEqual(508, rig.Board.rectTransform.rect.width, .01f);
                    Assert.AreEqual(rig.Board.rectTransform.rect.width, rig.Board.rectTransform.rect.height, .01f);
                    if (rig.State.Phase == BattlePhase.WonPendingSettlement) break;
                    Assert.Less(action, 64, "Reach pending victory through normal in-memory battle actions.");
                    Assert.AreEqual(BattlePhase.AwaitAction, rig.State.Phase);
                    Assert.IsTrue(input.SelectMember(rig.State.Members.First(x => x.Hp.Numerator.Sign > 0).Member.CharacterId));
                    var route = rig.Route();
                    Func<FlowPos, FightMatch.Input.PointerSample> sample = cell =>
                    {
                        var point = rig.Board.CellCenter(cell);
                        Assert.AreEqual(cell, rig.Board.CellAtLocal(point));
                        return new FightMatch.Input.PointerSample(1, point.x, point.y, cell);
                    };
                    input.Down(sample(route[0]), 6);
                    foreach (var cell in route.Skip(1)) input.Move(sample(cell));
                    input.Up(sample(route.Last()));
                    Assert.IsNotNull(input.LastResult); Assert.IsTrue(input.LastResult.Application.IsCommitted, input.Status);
                    var playback = rig.View.BattleView.PlaybackView;
                    for (var frame = 0; playback.Controller.IsPlaying; frame++)
                    {
                        Assert.Less(frame, 256);
                        MeasureBattleCopy(rig, measured, failures);
                        playback.Advance(CandidateBattlePlaybackController.FactMilliseconds);
                    }
                }
                Assert.IsTrue(rig.Session.Battle.View.CanSettle);
                Assert.AreEqual(0, rig.Head.Business.Rewards.BaseRewards.Count, "Measuring pending copy must not claim rewards.");
            }
            Assert.IsEmpty(failures, string.Join("\n", failures));
            yield return new UnityEngine.TestTools.ExitPlayMode(); enteredPlayMode = false;
        }

        private static void MeasureBattleCopy(UguiHostRig rig, HashSet<string> measured, HashSet<string> failures)
        {
            Canvas.ForceUpdateCanvases();
            var safe = rig.Root.transform.Find("RuntimeCanvas/SafeAreaRoot");
            var battle = safe.Find("ScreenLayer/BattlePage/BattleContent");
            var labels = new List<TMPro.TextMeshProUGUI> { safe.Find("HudLayer/TopBar/Title").GetComponent<TMPro.TextMeshProUGUI>() };
            labels.AddRange(battle.Find("BattleStatus").GetComponentsInChildren<TMPro.TextMeshProUGUI>(false));
            foreach (var group in new[] { "AllySlots", "EnemySlots" })
            foreach (Transform slot in battle.Find("Stage/" + group))
            {
                if (!slot.gameObject.activeInHierarchy) continue;
                Assert.IsFalse(slot.Find("Name").gameObject.activeSelf, "The full name is already included in HP.");
                labels.AddRange(slot.GetComponentsInChildren<TMPro.TextMeshProUGUI>(false));
            }
            foreach (var text in labels.Where(x => x.gameObject.activeInHierarchy))
            {
                StringAssert.DoesNotContain("if you see this", text.text, text.name + " must have a runtime localization binding.");
                text.ForceMeshUpdate(true, true);
                var rect = text.rectTransform.rect;
                var visible = text.textInfo.characterInfo.Take(text.textInfo.characterCount).Where(x => x.isVisible).ToArray();
                Assert.IsNotEmpty(visible, text.name);
                var minX = visible.Min(x => x.bottomLeft.x); var maxX = visible.Max(x => x.topRight.x);
                var minY = visible.Min(x => x.bottomLeft.y); var maxY = visible.Max(x => x.topRight.y);
                var key = text.name + ": " + text.text;
                if (measured.Add(key)) TestContext.WriteLine(key + " | rect=" + rect.size +
                    " preferredHeight=" + text.preferredHeight + " glyphBounds=" + new Vector4(minX, minY, maxX, maxY));
                var lines = visible.GroupBy(x => x.lineNumber).OrderBy(x => x.Key).ToArray();
                for (var i = 1; i < lines.Length; i++)
                    if (lines[i - 1].Min(x => x.bottomLeft.y) < lines[i].Max(x => x.topRight.y))
                        failures.Add(key + " | adjacent line glyphs intersect");
                if (text.preferredHeight > rect.height + .1f || minX < rect.xMin - .5f || maxX > rect.xMax + .5f || minY < rect.yMin - .5f || maxY > rect.yMax + .5f)
                    failures.Add(key + " | rect=" + rect.size + " preferredHeight=" + text.preferredHeight +
                        " glyphBounds=" + new Vector4(minX, minY, maxX, maxY));
                Assert.AreEqual(TMPro.TextOverflowModes.Overflow, text.overflowMode, "Do not mask or truncate missing text.");
                Assert.IsFalse(text.enableAutoSizing); Assert.GreaterOrEqual(text.fontSize, 12);
            }
        }


        [UnityEngine.TestTools.UnityTearDown]
        public System.Collections.IEnumerator ExitControlledPlayModeAfterFailure()
        {
            if (enteredPlayMode && UnityEngine.Application.isPlaying)
            {
                TestContext.Out.WriteLine("FIX20 teardown: exiting controlled PlayMode after test failure.");
                yield return new UnityEngine.TestTools.ExitPlayMode();
            }
            enteredPlayMode = false;
        }

        internal static string BoardDiagnostic(UguiHostRig rig)
        {
            var board = rig.Board; var input = rig.View.BattleView.PlaybackView.InputView;
            var face = input.Controller?.View.BattleSnapshot?.Board.Face;
            var rect = board.rectTransform.rect; var matrix = board.rectTransform.localToWorldMatrix;
            var ancestors = new List<string>();
            for (var current = board.transform; current != null; current = current.parent)
            {
                var m = current.localToWorldMatrix; var r = current as RectTransform;
                ancestors.Add(current.name + "[active=" + current.gameObject.activeInHierarchy +
                    ",rect=" + (r == null ? "none" : r.rect.ToString("R")) +
                    ",m00/m11/m01/m10=" + m.m00.ToString("R") + "/" + m.m11.ToString("R") +
                    "/" + m.m01.ToString("R") + "/" + m.m10.ToString("R") + "]");
            }
            return "FIX20 probe: isPlaying=" + UnityEngine.Application.isPlaying +
                ",pageActive=" + rig.View.BattleView.gameObject.activeInHierarchy +
                ",boardActive=" + board.gameObject.activeInHierarchy +
                ",controllerSame=" + ReferenceEquals(input.Controller, rig.Session.Battle.Input) +
                ",face=" + (face == null ? "null" : face.Width + "x" + face.Height) +
                ",rect=" + rect.ToString("R") + ",m00/m11/m01/m10=" + matrix.m00.ToString("R") +
                "/" + matrix.m11.ToString("R") + "/" + matrix.m01.ToString("R") + "/" + matrix.m10.ToString("R") +
                ",DiagnosticVisible=" + rig.View.DiagnosticVisible + ",cull=" + board.canvasRenderer.cull +
                ",ancestors=" + string.Join(" <- ", ancestors);
        }

        internal static void AssertBoardReady(UguiHostRig rig)
        {
            var diagnostic = BoardDiagnostic(rig); TestContext.Out.WriteLine(diagnostic);
            var input = rig.View.BattleView.PlaybackView.InputView;
            var face = input.Controller?.View.BattleSnapshot?.Board.Face;
            var rect = rig.Board.rectTransform.rect; var matrix = rig.Board.rectTransform.localToWorldMatrix;
            Assert.IsTrue(UnityEngine.Application.isPlaying, diagnostic);
            Assert.IsTrue(rig.View.BattleView.gameObject.activeInHierarchy, diagnostic);
            Assert.IsTrue(rig.Board.gameObject.activeInHierarchy, diagnostic);
            Assert.AreSame(rig.Session.Battle.Input, input.Controller, diagnostic);
            Assert.IsNotNull(face, diagnostic); Assert.Greater(face.Width, 0, diagnostic); Assert.Greater(face.Height, 0, diagnostic);
            Assert.IsTrue(!float.IsNaN(rect.width) && !float.IsInfinity(rect.width) && rect.width > 0 &&
                !float.IsNaN(rect.height) && !float.IsInfinity(rect.height) && rect.height > 0, diagnostic);
            Assert.IsTrue(!float.IsNaN(matrix.m00) && !float.IsInfinity(matrix.m00) && matrix.m00 > 0 &&
                !float.IsNaN(matrix.m11) && !float.IsInfinity(matrix.m11) &&
                Mathf.Abs(matrix.m00 - matrix.m11) <= .0001f * matrix.m00 &&
                Mathf.Abs(matrix.m01) <= .0001f && Mathf.Abs(matrix.m10) <= .0001f, diagnostic);
            Assert.IsFalse(rig.View.DiagnosticVisible, diagnostic);
        }

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
                    "fm.action.language.open", "fm.popup.reference", "fm.popup.confirmation", "fm.popup.recovery", "fm.popup.license", "fm.popup.quit", "fm.popup.blocking" })
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
                // The shipped stage/drawer prefab already has 141 texts; replacing the language UI keeps that total.
                Assert.AreEqual(141, texts.Length); Assert.AreEqual(3, linkedInputs); Assert.AreEqual(138, texts.Length - linkedInputs);
                Assert.AreEqual(0, rig.Root.GetComponentsInChildren<UnityEngine.UIElements.UIDocument>(true).Length);
                rig.Bind();
                Assert.IsFalse(rig.View.DiagnosticVisible, rig.View.DiagnosticCode);
                Assert.IsTrue(rig.Find<FightMatchViewId>("fm.page.startup").gameObject.activeInHierarchy);
            }
        }

        [UnityEngine.TestTools.UnityTest]
        public System.Collections.IEnumerator OwnerEndedCommitsOnceDespiteBothPointerUpAndEndDrag()
        {
            UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene,
                UnityEditor.SceneManagement.NewSceneMode.Single);
            yield return new UnityEngine.TestTools.EnterPlayMode();
            enteredPlayMode = true;
            Assert.IsTrue(UnityEngine.Application.isPlaying);
            using (var rig = new UguiHostRig(true))
            {
                rig.Enter();
                TestContext.Out.WriteLine(BoardDiagnostic(rig));
                yield return rig.Ready();
                AssertBoardReady(rig);
                var before = rig.Storage.SnapshotCreates;
                var observe = rig.Board.gameObject.AddComponent<UguiReleaseObservation>(); observe.Board = rig.Board;
                rig.BeginRoute(); rig.EndRoute();
                Assert.IsTrue(rig.Session.Battle.Input.LastResult.Application.IsCommitted);
                Assert.AreEqual(before + 1, rig.Storage.SnapshotCreates);
                CollectionAssert.Contains(observe.Events, "PointerUp:False"); CollectionAssert.Contains(observe.Events, "EndDrag:False");
                Assert.IsFalse(rig.Board.HasActivePointer);
            }
            yield return new UnityEngine.TestTools.ExitPlayMode();
            enteredPlayMode = false;
        }

        [UnityEngine.TestTools.UnityTest]
        public System.Collections.IEnumerator OwnerCanceledNormalizesBeforeReleaseAndNeverCommits()
        {
            UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene,
                UnityEditor.SceneManagement.NewSceneMode.Single);
            yield return new UnityEngine.TestTools.EnterPlayMode();
            enteredPlayMode = true;
            Assert.IsTrue(UnityEngine.Application.isPlaying);
            using (var rig = new UguiHostRig(true))
            {
                rig.Enter(); var head = rig.Head; var before = rig.Storage.SnapshotCreates;
                var observe = rig.Board.gameObject.AddComponent<UguiReleaseObservation>(); observe.Board = rig.Board;
                TestContext.Out.WriteLine(BoardDiagnostic(rig));
                yield return rig.Ready();
                AssertBoardReady(rig);
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
            yield return new UnityEngine.TestTools.ExitPlayMode();
            enteredPlayMode = false;
        }

        [UnityEngine.TestTools.UnityTest]
        public System.Collections.IEnumerator SecondFingerCanceledLeavesOwnerAndOwnerEndedStillCommitsOnce()
        {
            UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene,
                UnityEditor.SceneManagement.NewSceneMode.Single);
            yield return new UnityEngine.TestTools.EnterPlayMode();
            enteredPlayMode = true;
            Assert.IsTrue(UnityEngine.Application.isPlaying);
            using (var rig = new UguiHostRig(true))
            {
                rig.Enter(); var before = rig.Storage.SnapshotCreates;
                TestContext.Out.WriteLine(BoardDiagnostic(rig));
                yield return rig.Ready();
                AssertBoardReady(rig);
                rig.BeginRoute(1);
                var other = UguiPointerDriver.Point(rig.Board, rig.Route()[0]);
                rig.Driver.Down(other, 2); rig.Driver.Cancel(2, other);
                Assert.IsTrue(rig.Board.HasActivePointer);
                Assert.AreEqual(1, rig.Session.Battle.Input.Gesture.ActivePointerId);
                Assert.AreEqual(before, rig.Storage.SnapshotCreates);
                rig.EndRoute(1); Assert.AreEqual(before + 1, rig.Storage.SnapshotCreates);
            }
            yield return new UnityEngine.TestTools.ExitPlayMode();
            enteredPlayMode = false;
        }

        [UnityEngine.TestTools.UnityTest]
        public System.Collections.IEnumerator FourCornerCentersAndHalfOpenEdgesUseTheActualBoardMapper()
        {
            UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene,
                UnityEditor.SceneManagement.NewSceneMode.Single);
            yield return new UnityEngine.TestTools.EnterPlayMode();
            enteredPlayMode = true;
            Assert.IsTrue(UnityEngine.Application.isPlaying);
            using (var rig = new UguiHostRig(true))
            {
                rig.Enter(); var board = rig.Board; var face = rig.State.Board.Face;
                TestContext.Out.WriteLine(BoardDiagnostic(rig));
                yield return rig.Ready();
                AssertBoardReady(rig);
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
            yield return new UnityEngine.TestTools.ExitPlayMode();
            enteredPlayMode = false;
        }

        [UnityEngine.TestTools.UnityTest]
        public System.Collections.IEnumerator BothResolutionsWithFourInsetsKeepHitAndRenderedCentersIdentical()
        {
            UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene,
                UnityEditor.SceneManagement.NewSceneMode.Single);
            yield return new UnityEngine.TestTools.EnterPlayMode();
            enteredPlayMode = true;
            Assert.IsTrue(UnityEngine.Application.isPlaying);
            using (var rig = new UguiHostRig(true))
            {
                rig.Enter(); var board = rig.Board; var canvas = rig.Root.GetComponentInChildren<Canvas>();
                TestContext.Out.WriteLine(BoardDiagnostic(rig));
                yield return rig.Ready();
                AssertBoardReady(rig);
                canvas.GetComponent<UnityEngine.UI.CanvasScaler>().enabled = false; canvas.renderMode = RenderMode.WorldSpace;
                var canvasRect = (RectTransform)canvas.transform; var safe = rig.Root.GetComponentInChildren<SafeAreaFitter>(); safe.enabled = false;
                foreach (var resolution in new[] { new Vector2(540, 960), new Vector2(1080, 2400) })
                {
                    canvasRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, resolution.x);
                    canvasRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, resolution.y);
                    var inset = new Rect(17, 23, resolution.x - 17 - 29, resolution.y - 23 - 31);
                    safe.Apply(inset, resolution); Canvas.ForceUpdateCanvases();
                    AssertBoardReady(rig);
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
            yield return new UnityEngine.TestTools.ExitPlayMode();
            enteredPlayMode = false;
        }

        // These samples exercise Unity's input module and raycaster, not emulator OS input delivery.
        private static void ClickSelectable(UguiHostRig rig, UnityEngine.UI.Selectable target, int pointer)
        {
            Canvas.ForceUpdateCanvases();
            var rect = (RectTransform)target.transform;
            var point = RectTransformUtility.WorldToScreenPoint(null, rect.TransformPoint(rect.rect.center));
            var hits = rig.Driver.Raycast(point);
            Assert.IsNotEmpty(hits, target.name);
            Assert.IsTrue(hits[0].gameObject == target.gameObject || hits[0].gameObject.transform.IsChildOf(target.transform),
                "The intended control must be the foremost raycast hit: " + target.name);
            rig.Driver.Down(point, pointer); rig.Driver.Up(point, pointer);
        }

        private static void AssertLocaleState(UguiHostRig rig, FightMatchLocaleDropdown dropdown, string caption, Color tint, bool canOpen)
        {
            Assert.IsTrue(dropdown.interactable, "A board pointer must not toggle the language control's persistent availability.");
            Assert.IsFalse(dropdown.IsExpanded);
            Assert.AreNotSame(dropdown.gameObject, rig.Driver.EventSystem.currentSelectedGameObject);
            Assert.AreEqual(caption, ((TMPro.TMP_Text)dropdown.placeholder).text);
            Assert.AreEqual(tint, dropdown.targetGraphic.canvasRenderer.GetColor(), "Check the rendered tint, not only Graphic.color.");
            Assert.IsNotNull(dropdown.CanOpen); Assert.AreEqual(canOpen, dropdown.CanOpen());
        }

        [UnityEngine.TestTools.UnityTest]
        public System.Collections.IEnumerator ActorlessBoardPointerKeepsLocaleStateAndLegalActionCommitsOnce()
        {
            UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene,
                UnityEditor.SceneManagement.NewSceneMode.Single);
            yield return new UnityEngine.TestTools.EnterPlayMode(); enteredPlayMode = true;
            Assert.IsTrue(UnityEngine.Application.isPlaying);
            foreach (var pointer in new[] { -1, 1 })
            {
                using (var rig = new UguiHostRig(true))
                {
                    rig.Enter(); var input = rig.Session.Battle.Input;
                    Assert.IsTrue(input.SelectMember(null)); Assert.IsNull(input.SelectedCharacterId);
                    yield return rig.Ready(); AssertBoardReady(rig);
                    Assert.AreEqual(BattlePhase.AwaitAction, rig.State.Phase);
                    Assert.IsTrue(input.View.Rollback.IsAvailable); Assert.IsTrue(input.InputContext.AllowHistoryTap);
                    Assert.IsTrue(input.InputContext.Pairs.All(x => !x.CanDraw));
                    var dropdown = rig.Find<FightMatchLocaleDropdown>("fm.action.language.open");
                    var label = dropdown.placeholder as TMPro.TMP_Text; Assert.IsNotNull(label);
                    Assert.IsNotNull(dropdown.targetGraphic);
                    yield return new WaitForSecondsRealtime(dropdown.colors.fadeDuration + .05f);
                    var caption = label.text; var tint = dropdown.targetGraphic.canvasRenderer.GetColor();
                    var route = rig.Route(); var start = UguiPointerDriver.Point(rig.Board, route[0]);
                    var end = UguiPointerDriver.Point(rig.Board, route.Last());
                    var head = rig.Head; var before = rig.Storage.SnapshotCreates;
                    AssertLocaleState(rig, dropdown, caption, tint, true);
                    Assert.AreSame(rig.Board.gameObject, rig.Driver.Raycast(start).First().gameObject);

                    rig.Driver.Down(start, pointer);
                    Assert.IsTrue(rig.Board.HasActivePointer); Assert.AreEqual(pointer, input.Gesture.ActivePointerId);
                    Assert.IsFalse(dropdown.CanOpen(), "The pointer gate must apply before Host Update.");
                    yield return null; yield return new WaitForSecondsRealtime(dropdown.colors.fadeDuration + .05f);
                    Assert.IsTrue(rig.Board.HasActivePointer); Assert.AreEqual(pointer, input.Gesture.ActivePointerId);
                    Assert.AreEqual(FightMatch.Input.GestureStage.Pressed, input.Gesture.Stage);
                    AssertLocaleState(rig, dropdown, caption, tint, false);
                    Assert.IsNull(input.LastRequest); Assert.AreSame(head, rig.Head); Assert.AreEqual(before, rig.Storage.SnapshotCreates);
                    rig.Driver.Up(start, pointer);
                    yield return null; yield return null;
                    Assert.IsFalse(rig.Board.HasActivePointer); Assert.IsNull(input.Gesture.ActivePointerId);
                    Assert.IsNull(input.SelectedCharacterId); Assert.IsNull(input.LastRequest);
                    Assert.IsNotNull(input.LastPreview, "Actorless short presses must still reach the existing history lookup.");
                    Assert.IsFalse(input.LastPreview.IsAccepted, "A fresh battle has no attack to roll back.");
                    Assert.AreSame(head, rig.Head); Assert.AreEqual(before, rig.Storage.SnapshotCreates);
                    AssertLocaleState(rig, dropdown, caption, tint, true);

                    var preview = input.LastPreview;
                    Assert.AreSame(rig.Board.gameObject, rig.Driver.Raycast(start).First().gameObject);
                    rig.Driver.Down(start, pointer);
                    for (var i = 1; i < route.Count; i++) rig.Driver.Move(UguiPointerDriver.Point(rig.Board, route[i]), pointer);
                    yield return null; yield return new WaitForSecondsRealtime(dropdown.colors.fadeDuration + .05f);
                    Assert.IsTrue(rig.Board.HasActivePointer); Assert.AreEqual(pointer, input.Gesture.ActivePointerId);
                    Assert.IsTrue(input.Gesture.HasInvalidSample, "An actorless attack drag must not become a legal route.");
                    AssertLocaleState(rig, dropdown, caption, tint, false);
                    rig.Driver.Up(end, pointer);
                    yield return null; yield return null;
                    Assert.IsFalse(rig.Board.HasActivePointer); Assert.IsNull(input.Gesture.ActivePointerId);
                    Assert.IsNull(input.SelectedCharacterId); Assert.IsNull(input.LastRequest); Assert.AreSame(preview, input.LastPreview);
                    Assert.AreSame(head, rig.Head); Assert.AreEqual(before, rig.Storage.SnapshotCreates);
                    AssertLocaleState(rig, dropdown, caption, tint, true);

                    var member = rig.State.Members.First(x => x.Hp.Numerator.Sign > 0).Member.CharacterId;
                    ClickSelectable(rig, rig.Find<UnityEngine.UI.Button>(FightMatchViewId.Member(member)), pointer);
                    Assert.AreEqual(member, input.SelectedCharacterId, "Enter's preselection must not stand in for the member button.");
                    Assert.AreSame(rig.Board.gameObject, rig.Driver.Raycast(start).First().gameObject);
                    rig.Driver.Down(start, pointer);
                    Assert.IsFalse(dropdown.CanOpen());
                    for (var i = 1; i < route.Count; i++) rig.Driver.Move(UguiPointerDriver.Point(rig.Board, route[i]), pointer);
                    yield return null; yield return new WaitForSecondsRealtime(dropdown.colors.fadeDuration + .05f);
                    Assert.IsTrue(rig.Board.HasActivePointer, "Host Update must preserve the legal board gesture.");
                    Assert.AreEqual(pointer, input.Gesture.ActivePointerId); Assert.IsFalse(input.Gesture.HasInvalidSample);
                    CollectionAssert.AreEqual(route, input.Gesture.DraftCells);
                    AssertLocaleState(rig, dropdown, caption, tint, false);
                    Assert.IsNull(input.LastRequest); Assert.AreSame(head, rig.Head); Assert.AreEqual(before, rig.Storage.SnapshotCreates);
                    rig.Driver.Up(end, pointer);
                    Assert.IsNotNull(input.LastResult); Assert.IsTrue(input.LastResult.Application.IsCommitted);
                    Assert.AreNotSame(head, rig.Head); Assert.AreEqual(before + 1, rig.Storage.SnapshotCreates);
                    Assert.IsFalse(rig.Board.HasActivePointer); Assert.IsNull(input.Gesture.ActivePointerId);
                    yield return null; yield return null;
                    AssertLocaleState(rig, dropdown, caption, tint, true);
                    Assert.AreEqual(before + 1, rig.Storage.SnapshotCreates, "PointerUp and EndDrag must not submit twice.");
                }
            }
            yield return new UnityEngine.TestTools.ExitPlayMode(); enteredPlayMode = false;
        }

        private static UnityEngine.UI.Button AssertSettlementVisible(UguiHostRig rig)
        {
            Canvas.ForceUpdateCanvases();
            var button = rig.Find<UnityEngine.UI.Button>("fm.action.victory.settle");
            Assert.IsTrue(button.gameObject.activeInHierarchy); Assert.IsTrue(button.interactable);
            var scroll = button.GetComponentInParent<UnityEngine.UI.ScrollRect>(true);
            Assert.IsNotNull(scroll); Assert.IsNotNull(scroll.viewport);
            Assert.Greater(scroll.viewport.rect.width, 0); Assert.Greater(scroll.viewport.rect.height, 0);
            var rect = (RectTransform)button.transform; var corners = new Vector3[4]; rect.GetWorldCorners(corners);
            foreach (var corner in corners)
            {
                var local = scroll.viewport.InverseTransformPoint(corner); var bounds = scroll.viewport.rect;
                Assert.GreaterOrEqual(local.x, bounds.xMin - .1f); Assert.LessOrEqual(local.x, bounds.xMax + .1f);
                Assert.GreaterOrEqual(local.y, bounds.yMin - .1f); Assert.LessOrEqual(local.y, bounds.yMax + .1f);
            }
            var caption = button.GetComponentInChildren<LocalizedTmpText>(true); Assert.IsNotNull(caption);
            Assert.AreEqual("fm.victory.settle.button", caption.Key);
            Assert.AreEqual(rig.Localization.Resolve(caption.Key, null).Text, caption.Target.text);
            var point = RectTransformUtility.WorldToScreenPoint(null, rect.TransformPoint(rect.rect.center));
            var hits = rig.Driver.Raycast(point); Assert.IsNotEmpty(hits);
            Assert.AreSame(button.gameObject, hits[0].gameObject, "Settlement must be the first real raycast hit.");
            return button;
        }

        [UnityEngine.TestTools.UnityTest]
        public System.Collections.IEnumerator PendingVictoryRevealsSettlementUntilExplicitReleaseCommitsOnce()
        {
            UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene,
                UnityEditor.SceneManagement.NewSceneMode.Single);
            yield return new UnityEngine.TestTools.EnterPlayMode(); enteredPlayMode = true;
            Assert.IsTrue(UnityEngine.Application.isPlaying);
            foreach (var locale in new[] { LocaleId.En, LocaleId.ZhHans })
            {
                using (var rig = new UguiHostRig(true))
                {
                    rig.Localization.SetLocale(locale); rig.Enter(); yield return rig.Ready(); AssertBoardReady(rig);
                    var input = rig.Session.Battle.Input; var source = RealSource();
                    for (var step = 0; rig.State.Phase != BattlePhase.WonPendingSettlement; step++)
                    {
                        Assert.Less(step, 64, "Reach victory through bounded normal board actions.");
                        var state = rig.State;
                        Assert.That(state.Phase, NUnit.Framework.Is.EqualTo(BattlePhase.AwaitAction).Or.EqualTo(BattlePhase.AwaitLinks));
                        if (state.Phase == BattlePhase.AwaitAction)
                        {
                            var member = state.Members.First(x => x.Hp.Numerator.Sign > 0).Member.CharacterId;
                            ClickSelectable(rig, rig.Find<UnityEngine.UI.Button>(FightMatchViewId.Member(member)), -1);
                            Assert.AreEqual(member, input.SelectedCharacterId);
                        }
                        var pair = state.Phase == BattlePhase.AwaitLinks ? state.Board.PendingLinks[0] : state.Enemies.First(x => x.Hp.Numerator.Sign > 0).PairKey;
                        var level = source.Levels.Single(x => x.Level.LevelId == state.Baseline.Entry.Level.LevelId);
                        var face = state.Board.Face;
                        var route = level.SourceRoutes.Single(x => x.FaceId == pair.FaceId && x.PairId == pair.PairId).Cells.Select(p =>
                            new FlowPos(p.x - 1, source.Coordinates == DemoCoordinateCandidate.AssumedBottomLeft ? p.y - 1 : face.Height - p.y)).ToArray();
                        var start = UguiPointerDriver.Point(rig.Board, route[0]); var beforeAction = rig.Storage.SnapshotCreates;
                        Assert.AreSame(rig.Board.gameObject, rig.Driver.Raycast(start).First().gameObject);
                        rig.Driver.Down(start);
                        foreach (var cell in route.Skip(1)) rig.Driver.Move(UguiPointerDriver.Point(rig.Board, cell));
                        rig.Driver.Up(UguiPointerDriver.Point(rig.Board, route.Last()));
                        Assert.IsNotNull(input.LastResult); Assert.IsTrue(input.LastResult.Application.IsCommitted, input.Status);
                        Assert.AreEqual(beforeAction + 1, rig.Storage.SnapshotCreates);
                        if (rig.State.Phase != BattlePhase.WonPendingSettlement) rig.View.BattleView.PlaybackView.SkipToFinal();
                    }
                    Assert.IsNotNull(input.View.PresentationToken);
                    Assert.IsFalse(rig.Session.Battle.View.CanSettle);
                    Assert.IsFalse(rig.Find<UnityEngine.UI.Button>("fm.action.victory.settle").interactable);
                    var pendingHead = rig.Head; var creates = rig.Storage.SnapshotCreates;
                    Assert.AreEqual(0, pendingHead.Business.Rewards.BaseRewards.Count);

                    rig.View.BattleView.PlaybackView.SkipToFinal();
                    // Rebuild rows before LateUpdate consumes the pending reveal.
                    rig.Localization.SetLocale(locale == LocaleId.En ? LocaleId.ZhHans : LocaleId.En);
                    rig.Localization.SetLocale(locale);
                    yield return null; yield return null;
                    Assert.IsNull(input.View.PresentationToken); Assert.IsTrue(rig.Session.Battle.View.CanSettle);
                    Assert.AreEqual(BattlePhase.WonPendingSettlement, rig.State.Phase);
                    var settle = AssertSettlementVisible(rig);
                    var scroll = settle.GetComponentInParent<UnityEngine.UI.ScrollRect>(true);
                    Assert.AreSame(pendingHead, rig.Head); Assert.AreEqual(creates, rig.Storage.SnapshotCreates);
                    Assert.AreEqual(0, rig.Head.Business.Rewards.BaseRewards.Count);

                    // View-only preparation: move away after the product has revealed settlement.
                    scroll.StopMovement(); scroll.horizontalNormalizedPosition = 0; Canvas.ForceUpdateCanvases();
                    rig.Session.Battle.Refresh();
                    rig.Localization.SetLocale(locale == LocaleId.En ? LocaleId.ZhHans : LocaleId.En);
                    rig.Localization.SetLocale(locale);
                    yield return null; yield return null; Canvas.ForceUpdateCanvases();
                    Assert.That(scroll.horizontalNormalizedPosition, NUnit.Framework.Is.EqualTo(0).Within(.001f), "Unchanged eligibility must respect user scrolling.");
                    Assert.AreSame(pendingHead, rig.Head); Assert.AreEqual(creates, rig.Storage.SnapshotCreates);

                    rig.View.Unbind(); rig.Bind(); rig.View.Unbind();
                    yield return null; yield return null; Canvas.ForceUpdateCanvases();
                    Assert.That(scroll.horizontalNormalizedPosition, NUnit.Framework.Is.EqualTo(0).Within(.001f), "Unbind must cancel a pending reveal.");
                    rig.Bind(); yield return null; yield return null;
                    settle = AssertSettlementVisible(rig);
                    Assert.AreSame(pendingHead, rig.Head); Assert.AreEqual(creates, rig.Storage.SnapshotCreates);
                    Assert.AreEqual(0, rig.Head.Business.Rewards.BaseRewards.Count);
                    var rect = (RectTransform)settle.transform;
                    var point = RectTransformUtility.WorldToScreenPoint(null, rect.TransformPoint(rect.rect.center));
                    rig.Driver.Down(point); yield return null;
                    Assert.AreSame(pendingHead, rig.Head); Assert.AreEqual(creates, rig.Storage.SnapshotCreates);
                    Assert.AreEqual(0, rig.Head.Business.Rewards.BaseRewards.Count);
                    rig.Driver.Up(point);
                    Assert.AreEqual(PlayerBattleRoute.Result, rig.Session.Battle.View.Route);
                    var receipt = rig.Session.Battle.View.Receipt; Assert.IsNotNull(receipt); Assert.IsNotNull(receipt.Reward);
                    Assert.AreEqual(pendingHead.Records.Count + 1, rig.Head.Records.Count);
                    Assert.AreEqual(creates + 1, rig.Storage.SnapshotCreates);
                    Assert.AreEqual(1, rig.Head.Business.Rewards.BaseRewards.Count);
                    var record = rig.Head.Records.Single(x => x.OperationId == receipt.OperationId);
                    Assert.AreEqual(CandidateApplicationKind.SettleVictory, record.Intent.Kind);
                    Assert.AreEqual(record.CommitId, receipt.CommitId); Assert.AreEqual(rig.Head.Header.CommitId, receipt.CommitId);
                    var title = rig.Find<LocalizedTmpText>(FightMatchViewId.Row("receipt-title"));
                    Assert.IsTrue(title.gameObject.activeInHierarchy); Assert.AreEqual("fm.result.already_settled", title.Key);
                    var settledHead = rig.Head;
                    rig.Driver.Up(point); rig.Session.Battle.Refresh(); yield return null; yield return null;
                    Assert.AreSame(settledHead, rig.Head); Assert.AreEqual(creates + 1, rig.Storage.SnapshotCreates);
                    Assert.AreEqual(1, rig.Head.Business.Rewards.BaseRewards.Count);
                    Assert.AreEqual(receipt.CommitId, rig.Session.Battle.View.Receipt.CommitId);
                }
            }
            yield return new UnityEngine.TestTools.ExitPlayMode(); enteredPlayMode = false;
        }
    }
}
