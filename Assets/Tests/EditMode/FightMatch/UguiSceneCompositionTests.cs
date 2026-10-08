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
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
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
        private bool enteredPlayMode;

        [UnityEngine.TestTools.UnityTest]
        public System.Collections.IEnumerator SavedDemoScenePreservesUsableCanvasWithoutStaleOverrides()
        {
            const string scenePath = "Assets/Scenes/FightMatchDemo.unity";
            const string prefabPath = "Assets/UI/FightMatch/Runtime/FightMatchRuntimeRoot.prefab";
            Assert.IsFalse(UnityEngine.Application.isPlaying);
            Assert.IsFalse(SceneManager.GetSceneByPath(scenePath).IsValid(), "Do not close or reuse an already open Demo Scene.");
            var originalActive = SceneManager.GetActiveScene();
            var sceneBytes = File.ReadAllBytes(scenePath); var prefabBytes = File.ReadAllBytes(prefabPath);
            var opened = default(Scene);
            EditorApplication.CallbackFunction update = null;
            try
            {
                var diskKeys = (string[])SceneCanvasCheck("LayoutRepairDiskKeys").Invoke(null, new object[] { sceneBytes, true });
                opened = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
                var roots = opened.GetRootGameObjects();
                var hosts = roots.SelectMany(x => x.GetComponentsInChildren<FightMatchPlayerHost>(true)).ToArray();
                var canvases = roots.SelectMany(x => x.GetComponentsInChildren<Canvas>(true)).ToArray();
                Assert.AreEqual(1, hosts.Length); Assert.AreEqual(1, canvases.Length);
                var instance = hosts[0].RuntimeRoot; Assert.IsNotNull(instance);
                Assert.AreEqual(prefabPath, PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(instance));
                Assert.AreSame(canvases[0].transform, instance.transform.Find("RuntimeCanvas"));
                var rect = (RectTransform)canvases[0].transform;
                var source = PrefabUtility.GetCorrespondingObjectFromSource(rect); Assert.IsNotNull(source);
                Assert.AreEqual(prefabPath, AssetDatabase.GetAssetPath(source));
                Assert.IsTrue(AssetDatabase.TryGetGUIDAndLocalFileIdentifier(source, out string guid, out long fileId));
                Assert.AreEqual("c36df3cfc25424c8cb3ec6cae6be1237", guid); Assert.AreEqual(1013562322995895578L, fileId);
                Assert.AreEqual(Vector3.one, source.localScale);
                Assert.AreEqual(Vector2.zero, source.anchorMin); Assert.AreEqual(Vector2.one, source.anchorMax);
                var canvas = canvases[0]; Canvas.ForceUpdateCanvases();
                var deadline = EditorApplication.timeSinceStartup + 5; var ticks = 0; var pending = true; var ready = false;
                Exception failure = null; SavedCanvasSample sample = null;
                update = () => {
                    if (!pending) return;
                    try
                    {
                        if (EditorApplication.timeSinceStartup >= deadline) throw new TimeoutException("Two Editor updates exceeded five seconds.");
                        if (++ticks == 1) return;
                        pending = false; EditorApplication.update -= update;
                        sample = CaptureSavedCanvasSample(canvas);
                        if (EditorApplication.timeSinceStartup >= deadline) throw new TimeoutException("Canvas capture exceeded five seconds.");
                    }
                    catch (Exception error) { pending = false; failure = error; EditorApplication.update -= update; }
                    finally { if (!pending) ready = true; }
                };
                EditorApplication.update += update; EditorApplication.QueuePlayerLoopUpdate();
                while (!ready && EditorApplication.timeSinceStartup < deadline) yield return null;
                Assert.IsTrue(ready && EditorApplication.timeSinceStartup < deadline, "Two subsequent Editor updates must finish within five seconds.");
                Assert.IsNull(failure, failure?.ToString()); Assert.AreEqual(2, ticks); Assert.IsNotNull(sample);
                var pixels = sample.pixels; var area = sample.area;
                Assert.AreEqual(RenderMode.ScreenSpaceOverlay, sample.mode);
                Assert.IsTrue(sample.enabled && sample.active && sample.isRootCanvas);
                Assert.AreSame(canvas, sample.rootCanvas); Assert.AreSame(canvas, sample.driver);
                Assert.AreSame(instance.transform, sample.parent); Assert.AreSame(source, sample.source);
                foreach (var value in new[] { sample.localScale.x, sample.localScale.y, sample.localScale.z,
                    sample.worldScale.x, sample.worldScale.y, sample.worldScale.z, area.width, area.height,
                    pixels.width, pixels.height, sample.scaleFactor })
                    Assert.IsTrue(!float.IsNaN(value) && !float.IsInfinity(value) && value > 0, "Canvas world scale must be finite and positive.");
                foreach (var value in new[] { area.xMin, area.yMin, area.xMax, area.yMax, pixels.xMin, pixels.yMin, pixels.xMax, pixels.yMax })
                    Assert.IsFalse(float.IsNaN(value) || float.IsInfinity(value), "Canvas bounds must be finite.");
                var expected = new[] { new Vector2(pixels.xMin, pixels.yMin), new Vector2(pixels.xMin, pixels.yMax),
                    new Vector2(pixels.xMax, pixels.yMax), new Vector2(pixels.xMax, pixels.yMin) };
                for (var i = 0; i < 4; i++)
                {
                    var point = sample.screenCorners[i];
                    Assert.IsFalse(float.IsNaN(point.x) || float.IsInfinity(point.x) || float.IsNaN(point.y) || float.IsInfinity(point.y));
                    Assert.That(point.x, NUnit.Framework.Is.EqualTo(expected[i].x).Within(.5f), "Canvas screen x corner " + i);
                    Assert.That(point.y, NUnit.Framework.Is.EqualTo(expected[i].y).Within(.5f), "Canvas screen y corner " + i);
                }
                SceneCanvasCheck("LayoutVerifyRepairOverrideEnvelope").Invoke(null, new object[] { diskKeys, sample.overrideKeys });
                Assert.IsFalse(sample.dirty, "The scene check must remain read-only.");
            }
            finally
            {
                if (update != null) EditorApplication.update -= update;
                try
                {
                    if (opened.IsValid()) Assert.IsTrue(EditorSceneManager.CloseScene(opened, true), "Close only the Scene opened by this test.");
                }
                finally
                {
                    if (originalActive.IsValid() && originalActive.isLoaded) SceneManager.SetActiveScene(originalActive);
                    CollectionAssert.AreEqual(sceneBytes, File.ReadAllBytes(scenePath), "Scene bytes changed during read-only inspection.");
                    CollectionAssert.AreEqual(prefabBytes, File.ReadAllBytes(prefabPath), "Prefab bytes changed during read-only inspection.");
                }
            }
        }

        private sealed class SavedCanvasSample
        {
            public Vector3 localScale, worldScale;
            public Rect area, pixels;
            public float scaleFactor;
            public RenderMode mode;
            public bool enabled, active, isRootCanvas, dirty;
            public Canvas rootCanvas;
            public UnityEngine.Object driver;
            public Transform parent;
            public RectTransform source;
            public Vector2[] screenCorners;
            public string[] overrideKeys;
        }
        private static SavedCanvasSample CaptureSavedCanvasSample(Canvas canvas)
        {
            var rect = (RectTransform)canvas.transform; var source = PrefabUtility.GetCorrespondingObjectFromSource(rect);
            var corners = new Vector3[4]; rect.GetWorldCorners(corners);
            return new SavedCanvasSample {
                localScale = rect.localScale, worldScale = rect.lossyScale, area = rect.rect, pixels = canvas.pixelRect,
                scaleFactor = canvas.scaleFactor, mode = canvas.renderMode, enabled = canvas.enabled, active = canvas.gameObject.activeInHierarchy,
                isRootCanvas = canvas.isRootCanvas, rootCanvas = canvas.rootCanvas, driver = rect.drivenByObject, parent = rect.parent,
                source = source, dirty = canvas.gameObject.scene.isDirty,
                screenCorners = corners.Select(x => RectTransformUtility.WorldToScreenPoint(null, x)).ToArray(),
                overrideKeys = (string[])SceneCanvasCheck("LayoutOverrideKeys").Invoke(null, new object[] {
                    PrefabUtility.GetPropertyModifications(rect.parent.gameObject) ?? Array.Empty<PropertyModification>() })
            };
        }

        private static string RepairKey(long id, string property, string value, string reference = "null")
        {
            return (string)SceneCanvasCheck("LayoutRepairOverrideKey").Invoke(null, new object[] { id, property, value, reference });
        }
        private static string[][] RepairOriginalRows()
        {
            var rows = new List<string[]> {
                new[] { "686838913121989351", "m_Name", "FightMatchRuntimeRoot" },
                new[] { "686838913121989351", "m_IsActive", "1" },
                new[] { "1013562322995895578", "m_Pivot.x", "0" },
                new[] { "1013562322995895578", "m_Pivot.y", "0" }
            };
            foreach (var id in new[] { "2223056809955240070", "2452066831200993119", "2783145704458770550",
                "3538173933649760411", "4530181991579543270" })
                rows.Add(new[] { id, "m_isOrthographic", "1" });
            foreach (var property in new[] { "m_LocalPosition.x", "m_LocalPosition.y", "m_LocalPosition.z", "m_LocalRotation.w",
                "m_LocalRotation.x", "m_LocalRotation.y", "m_LocalRotation.z", "m_LocalEulerAnglesHint.x", "m_LocalEulerAnglesHint.y", "m_LocalEulerAnglesHint.z" })
                rows.Add(new[] { "4535975771220848728", property, property == "m_LocalRotation.w" ? "1" : "0" });
            foreach (var id in new[] { "5265726911075511648", "7259531018279518139", "7481568776225615129" })
                rows.Add(new[] { id, "m_isOrthographic", "1" });
            return rows.ToArray();
        }
        private static string[] RepairOriginalKeys()
        {
            return RepairOriginalRows().Select(x => RepairKey(long.Parse(x[0], System.Globalization.CultureInfo.InvariantCulture), x[1], x[2])).ToArray();
        }
        private static string[] RepairObservedKeys()
        {
            var keys = new List<string>();
            foreach (var id in new long[] { 20060541972559401L, 525732035437362821L, 2083101828246034960L, 3603424187799618609L,
                3613386703531820566L, 5408553659951591733L, 6280369087512753137L, 6783473654260464751L,
                8076177029604565045L, 8217930521597258139L, 9020564264068189081L, 9181528876988089927L })
                foreach (var property in new[] { "m_AnchorMax.x", "m_AnchorMax.y", "m_AnchorMin.x", "m_AnchorMin.y",
                    "m_SizeDelta.x", "m_SizeDelta.y", "m_AnchoredPosition.x", "m_AnchoredPosition.y" })
                    keys.Add(RepairKey(id, property, "0"));
            keys.Add(RepairKey(6960656654093734010L, "m_SizeDelta.y", "0"));
            foreach (var property in new[] { "m_AnchorMax.x", "m_AnchorMax.y", "m_LocalScale.x", "m_LocalScale.y", "m_LocalScale.z" })
                keys.Add(RepairKey(1013562322995895578L, property, "0"));
            keys.Add(RepairKey(9054230707113910417L, "m_AdditionalShaderChannelsFlag", "25"));
            foreach (var id in new long[] { 2223056809955240070L, 2452066831200993119L, 2783145704458770550L, 3538173933649760411L,
                4530181991579543270L, 5257332161253344251L, 5265726911075511648L, 7259531018279518139L, 7481568776225615129L })
                keys.Add(RepairKey(id, "m_TextStyleHashCode", "-1183493901"));
            return keys.ToArray();
        }
        private static string[][] RepairFiveRows()
        {
            return new[] { "m_AnchorMax.x", "m_AnchorMax.y", "m_LocalScale.x", "m_LocalScale.y", "m_LocalScale.z" }
                .Select(x => new[] { "1013562322995895578", x, "0" }).ToArray();
        }
        private static byte[] RepairDiskFixture(IEnumerable<string[]> rows, long instance = 1017091371L)
        {
            var text = "%YAML 1.1\n--- !u!1001 &" + instance.ToString(System.Globalization.CultureInfo.InvariantCulture) +
                "\nPrefabInstance:\n  m_ObjectHideFlags: 0\n  serializedVersion: 2\n  m_Modification:\n" +
                "    serializedVersion: 3\n    m_TransformParent: {fileID: 222360855}\n    m_Modifications:\n";
            foreach (var row in rows)
                text += "    - target: {fileID: " + row[0] + ", guid: c36df3cfc25424c8cb3ec6cae6be1237, type: 3}\n" +
                    "      propertyPath: " + row[1] + "\n      value: " + row[2] + "\n      objectReference: {fileID: 0}\n";
            text += "    m_RemovedComponents: []\n    m_RemovedGameObjects: []\n    m_AddedGameObjects: []\n    m_AddedComponents: []\n" +
                "  m_SourcePrefab: {fileID: 100100000, guid: c36df3cfc25424c8cb3ec6cae6be1237, type: 3}\n" +
                "--- !u!4 &1017091373 stripped\nTransform:\n  m_PrefabInstance: {fileID: 1017091371}\n";
            return new System.Text.UTF8Encoding(false, true).GetBytes(text);
        }
        private static string[][] RepairBeforeRows()
        {
            var rows = RepairOriginalRows();
            return rows.Take(4).Concat(RepairFiveRows()).Concat(rows.Skip(4)).ToArray();
        }
        private static void RepairReject(string method, params object[] arguments)
        {
            var failure = Assert.Throws<System.Reflection.TargetInvocationException>(() => SceneCanvasCheck(method).Invoke(null, arguments));
            Assert.IsInstanceOf<InvalidOperationException>(failure.InnerException, "The production guard must reject the damaged input.");
        }

        [Test]
        public void RepairOverrideEnvelopeAcceptsOnlyObservedInsertionsAndFrozenOriginals()
        {
            var original = RepairOriginalKeys(); var extra = RepairObservedKeys();
            Assert.AreEqual(22, original.Length); Assert.AreEqual(112, extra.Length); Assert.AreEqual(112, extra.Distinct().Count());
            var actual = extra.Take(56).Concat(original.Take(11)).Concat(extra.Skip(56)).Concat(original.Skip(11)).ToArray();
            Assert.DoesNotThrow(() => SceneCanvasCheck("LayoutVerifyRepairOverrideEnvelope").Invoke(null, new object[] { original, actual }));
            var source = new[] { new PropertyModification { propertyPath = "m_Name", value = "before" } };
            var copy = (PropertyModification[])SceneCanvasCheck("LayoutCopyModifications").Invoke(null, new object[] { source });
            Assert.AreNotSame(source[0], copy[0]); source[0].value = "after"; Assert.AreEqual("before", copy[0].value);
        }
        [Test]
        public void RepairOverrideEnvelopeRejectsUnknownTargetOrProperty()
        {
            var original = RepairOriginalKeys();
            foreach (var extra in new[] { RepairKey(999L, "m_AnchorMax.x", "0"), RepairKey(20060541972559401L, "m_Pivot.x", "0"),
                RepairKey(20060541972559401L, "m_AnchorMax.x", "0").Replace("c36df3cfc25424c8cb3ec6cae6be1237", "00000000000000000000000000000000") })
                RepairReject("LayoutVerifyRepairOverrideEnvelope", original, original.Concat(new[] { extra }).ToArray());
        }
        [Test]
        public void RepairOverrideEnvelopeRejectsWrongValue()
        {
            var original = RepairOriginalKeys();
            foreach (var value in new[] { "1", "", null })
                RepairReject("LayoutVerifyRepairOverrideEnvelope", original,
                    original.Concat(new[] { RepairKey(20060541972559401L, "m_AnchorMax.x", value) }).ToArray());
        }
        [Test]
        public void RepairOverrideEnvelopeRejectsObjectReference()
        {
            var original = RepairOriginalKeys();
            RepairReject("LayoutVerifyRepairOverrideEnvelope", original, original.Concat(new[] {
                RepairKey(20060541972559401L, "m_AnchorMax.x", "0", "GlobalObjectId_V1-1-c36df3cfc25424c8cb3ec6cae6be1237-686838913121989351-0")
            }).ToArray());
        }
        [Test]
        public void RepairOverrideEnvelopeRejectsDuplicates()
        {
            var original = RepairOriginalKeys(); var extra = RepairObservedKeys()[0];
            RepairReject("LayoutVerifyRepairOverrideEnvelope", original, original.Concat(new[] { extra, extra }).ToArray());
            RepairReject("LayoutVerifyRepairOverrideEnvelope", original, original.Concat(new[] { original[0] }).ToArray());
        }
        [Test]
        public void RepairOverrideEnvelopeRejectsMissingOriginal()
        {
            var original = RepairOriginalKeys();
            RepairReject("LayoutVerifyRepairOverrideEnvelope", original, original.Skip(1).Concat(RepairObservedKeys()).ToArray());
        }
        [Test]
        public void RepairOverrideEnvelopeRejectsChangedOriginal()
        {
            var original = RepairOriginalKeys(); var actual = original.ToArray();
            actual[0] = RepairKey(686838913121989351L, "m_Name", "ChangedRoot");
            RepairReject("LayoutVerifyRepairOverrideEnvelope", original, actual);
        }
        [Test]
        public void RepairOverrideEnvelopeRejectsReorderedOriginals()
        {
            var original = RepairOriginalKeys(); var actual = original.ToArray();
            actual[0] = original[1]; actual[1] = original[0];
            RepairReject("LayoutVerifyRepairOverrideEnvelope", original, actual);
        }
        [Test]
        public void RepairDiskDeltaAcceptsOnlyFiveBlockRemoval()
        {
            var before = RepairDiskFixture(RepairBeforeRows()); var after = RepairDiskFixture(RepairOriginalRows());
            Assert.DoesNotThrow(() => SceneCanvasCheck("LayoutVerifyRepairDiskDelta").Invoke(null, new object[] { before, after }));
            CollectionAssert.AreEqual(RepairOriginalKeys(), (string[])SceneCanvasCheck("LayoutRepairDiskKeys").Invoke(null, new object[] { after, true }));
        }
        [Test]
        public void RepairDiskDeltaRejectsResidualCanvasBlock()
        {
            var before = RepairDiskFixture(RepairBeforeRows()); var original = RepairOriginalRows();
            foreach (var residual in RepairFiveRows())
            {
                var after = RepairDiskFixture(original.Take(4).Concat(new[] { residual }).Concat(original.Skip(4)));
                RepairReject("LayoutVerifyRepairDiskDelta", before, after);
                RepairReject("LayoutRepairDiskKeys", after, true);
                RepairReject("LayoutRepairDiskKeys", RepairDiskFixture(original.Take(21).Concat(new[] { residual })), true);
            }
        }
        [Test]
        public void RepairDiskDeltaRejectsUnrelatedBytes()
        {
            var after = RepairDiskFixture(RepairOriginalRows());
            after[0] = (byte)'#';
            RepairReject("LayoutVerifyRepairDiskDelta", RepairDiskFixture(RepairBeforeRows()), after);
        }
        [Test]
        public void RepairDiskDeltaRejectsAdditionalRemoval()
        {
            RepairReject("LayoutVerifyRepairDiskDelta", RepairDiskFixture(RepairBeforeRows()), RepairDiskFixture(RepairOriginalRows().Skip(1)));
        }
        [Test]
        public void RepairDiskDeltaRejectsWrongTargetOrInstance()
        {
            var rows = RepairBeforeRows(); rows[4] = new[] { "1013562322995895579", "m_AnchorMax.x", "0" };
            RepairReject("LayoutVerifyRepairDiskDelta", RepairDiskFixture(rows), RepairDiskFixture(RepairOriginalRows()));
            RepairReject("LayoutVerifyRepairDiskDelta", RepairDiskFixture(RepairBeforeRows(), 1017091372L), RepairDiskFixture(RepairOriginalRows(), 1017091372L));
        }

        [Test]
        public void SceneCanvasGeometryRejectsZeroNonFiniteAndIncompleteScreenCoverage()
        {
            var method = SceneCanvasCheck("LayoutUsableSceneCanvasGeometry");
            Func<object[]> sample = () => new object[] { new Vector3(.8f, .8f, 1), new Vector3(.8f, .8f, 1),
                new Rect(-450, -800, 900, 1600), new Rect(8, 12, 720, 1280), .8f,
                new[] { new Vector2(8, 12), new Vector2(8, 1292), new Vector2(728, 1292), new Vector2(728, 12) } };
            Func<object[], bool> usable = values => (bool)method.Invoke(null, values);
            Assert.IsTrue(usable(sample()));
            foreach (var bad in new[] { 0f, -1f, float.NaN, float.PositiveInfinity, float.NegativeInfinity })
            {
                for (var group = 0; group < 2; group++)
                    for (var axis = 0; axis < 3; axis++)
                    {
                        var values = sample(); var scale = (Vector3)values[group]; scale[axis] = bad; values[group] = scale;
                        Assert.IsFalse(usable(values), "scale group/axis " + group + "/" + axis + " = " + bad);
                    }
                for (var group = 2; group < 4; group++)
                    for (var axis = 0; axis < 2; axis++)
                    {
                        var values = sample(); var rect = (Rect)values[group];
                        if (axis == 0) rect.width = bad; else rect.height = bad; values[group] = rect;
                        Assert.IsFalse(usable(values), "rect group/axis " + group + "/" + axis + " = " + bad);
                    }
                var factor = sample(); factor[4] = bad; Assert.IsFalse(usable(factor), "scaleFactor = " + bad);
                for (var corner = 0; corner < 4; corner++)
                    for (var axis = 0; axis < 2; axis++)
                    {
                        var values = sample(); var points = (Vector2[])values[5]; var point = points[corner];
                        point[axis] = bad; points[corner] = point; Assert.IsFalse(usable(values), "invalid corner coordinate");
                    }
            }
            foreach (var bad in new[] { float.NaN, float.PositiveInfinity, float.NegativeInfinity })
                for (var group = 2; group < 4; group++)
                    for (var axis = 0; axis < 2; axis++)
                    {
                        var values = sample(); var rect = (Rect)values[group];
                        if (axis == 0) rect.x = bad; else rect.y = bad; values[group] = rect;
                        Assert.IsFalse(usable(values), "non-finite rect origin");
                    }
            for (var corner = 0; corner < 4; corner++)
                for (var axis = 0; axis < 2; axis++)
                    foreach (var delta in new[] { -.5f, .5f, -1f, 1f })
                    {
                        var values = sample(); var points = (Vector2[])values[5]; var point = points[corner];
                        point[axis] += delta; points[corner] = point;
                        Assert.AreEqual(Mathf.Abs(delta) <= .5f, usable(values), "pixel coverage tolerance");
                    }
            foreach (var corners in new[] { null, new Vector2[3], new Vector2[5] })
            { var values = sample(); values[5] = corners; Assert.IsFalse(usable(values), "exactly four mapped corners required"); }
        }

        [Test]
        public void SceneCanvasOverrideClassifierRejectsAllFiveStaleFields()
        {
            const string prefabPath = "Assets/UI/FightMatch/Runtime/FightMatchRuntimeRoot.prefab";
            var before = File.ReadAllBytes(prefabPath);
            try
            {
                var root = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath); Assert.IsNotNull(root);
                var source = root.transform.Find("RuntimeCanvas") as RectTransform; Assert.IsNotNull(source);
                Assert.IsTrue(AssetDatabase.TryGetGUIDAndLocalFileIdentifier(source, out string guid, out long fileId));
                Assert.AreEqual("c36df3cfc25424c8cb3ec6cae6be1237", guid); Assert.AreEqual(1013562322995895578L, fileId);
                Assert.AreEqual(Vector3.one, source.localScale); Assert.AreEqual(Vector2.one, source.anchorMax);
                var method = SceneCanvasCheck("LayoutStaleCanvasOverride"); var properties = new SerializedObject(source);
                Func<PropertyModification, bool> stale = change => (bool)method.Invoke(null, new object[] { change, source });
                foreach (var path in new[] { "m_LocalScale.x", "m_LocalScale.y", "m_LocalScale.z", "m_AnchorMax.x", "m_AnchorMax.y" })
                {
                    var modification = new PropertyModification { target = source, propertyPath = path };
                    foreach (var bad in new[] { "0", "-1", "NaN", "Infinity", "-Infinity", "invalid", "", null })
                    { modification.value = bad; Assert.IsTrue(stale(modification), path + " = " + bad); }
                    modification.value = properties.FindProperty(path).floatValue.ToString("R", System.Globalization.CultureInfo.InvariantCulture);
                    Assert.IsFalse(stale(modification), "Matching source value must survive: " + path);
                    modification.target = root.transform; modification.value = "0";
                    Assert.IsFalse(stale(modification), "Other target must survive: " + path);
                }
                foreach (var path in new[] { "m_Pivot.x", "m_Pivot.y", "m_AnchorMin.x", "m_AnchoredPosition.x", "m_SizeDelta.y" })
                    Assert.IsFalse(stale(new PropertyModification { target = source, propertyPath = path, value = "invalid" }), path);
                Assert.IsFalse(stale(null));
            }
            finally { CollectionAssert.AreEqual(before, File.ReadAllBytes(prefabPath), "Classifier test must not change the prefab."); }
        }

        private static System.Reflection.MethodInfo SceneCanvasCheck(string name)
        {
            var script = AssetDatabase.LoadAssetAtPath<MonoScript>("Assets/Scripts/FightMatch/Host/Editor/FightMatchAndroidBuild.cs");
            Assert.IsNotNull(script); var type = script.GetClass(); Assert.IsNotNull(type);
            var method = type.GetMethod(name, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            Assert.IsNotNull(method, name); return method;
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
                UguiResponsiveLayoutTests.AssertExactTextTargets(rig.Root);
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
    }
}
