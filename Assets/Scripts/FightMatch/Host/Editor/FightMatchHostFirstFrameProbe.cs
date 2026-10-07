using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
namespace FightMatch.Host.Editor
{
    [InitializeOnLoad]
    public static class FightMatchHostFirstFrameProbe
    {
        const string Owner = "01a11781-abc7-7203-a1de-187852f84168";
        const string Nonce = "hn03-636ff5b0d3304862974c0c4fbdae806a";
        const string Evidence = "/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch/TestArtifacts/FightMatch/HOST-NEXT-001/M03";
        const string ScenePath = "Assets/Scenes/FightMatchCanvasPersistenceProbe.unity";
        const string SourcePath = "Assets/Scripts/FightMatch/Host/Editor/FightMatchHostFirstFrameProbe.cs";
        const string PrefabPath = "Assets/UI/FightMatch/Runtime/FightMatchRuntimeRoot.prefab";
        const string StateKey = "FightMatch.HOST-NEXT-001." + Nonce;
        const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        [Serializable] sealed class Input { public string path, sha256; public long bytes; }
        [Serializable] sealed class Isolation { public int schemaVersion; public string activationId, canonicalProjectRoot, persistentRoot, candidateSha256, ownerThread, ownerTurn; }
        [Serializable] sealed class Activation
        {
            public string ownerTurn, nonce, candidateProjectPath, probeSourceSha256, sceneSha256, sceneMetaSha256, sceneGuid, prefabGuid, originalSceneGuid, executionCandidateSha256;
            public Input[] candidateFiles; public string[] expectedTargets, originalSceneObjectIds, argv; public Isolation hostSaveIsolation;
        }
        [Serializable] sealed class Setup { public string path; public bool loaded, active; }
        [Serializable] sealed class Modification { public bool isNull, propertyNull, valueNull; public string target, property, value, reference; }
        [Serializable] sealed class Sample
        {
            public string stage, utc, diagnostic, effectiveRoot, productRoot, driver, canvasId, hostId, renderMode;
            public int frame, listeners, screenWidth, screenHeight; public long safeVersion;
            public bool layoutGateReleased;
            public bool playing, paused, bound, cancelled, localeLoaded, sessionNull, selectionNull, selectionEnabled, leaseNull, responsiveSubscribed, diagnosticVisible, gate, applyUsable, canvasPositive, supportedSize, bindingsPresent;
            public string[] geometry, safeArea; public bool interactable, blocksRaycasts;
        }
        [Serializable] sealed class Hit { public string target, name, root, module; public int depth, sortingOrder; public float distance; public bool underDiagnostic; }
        [Serializable] sealed class Ray { public string target, obstruction; public float x, y; public Hit[] hits; }
        [Serializable] sealed class PredicateCase { public string name, expected, actual; public bool passed; }
        [Serializable] sealed class Report
        {
            public string ownerTurn = Owner, nonce = Nonce, activationSha256, sourceSha256, assemblySha256, executionCandidateSha256, fullInputCanonicalSha256, probeMetaSha256, probeMetaGuid, sceneGuid, prefabGuid, diskSha256, diskMetaSha256, firstError, phase = "new", status, A = "UNOBSERVED", B = "UNOBSERVED", C = "UNOBSERVED", D = "UNOBSERVED", normalInteraction = "LOC_BLOCKED";
            public List<PredicateCase> geometryPredicateChecks = new List<PredicateCase>();
            public string sceneCleanup, prefabDiskSha256; public string[] originalGameItems, finalGameItems; public int builtinCount, deleteRelativeIndex = -1, deleteTotalIndex = -1, finalSelection = -1, cleanupLogErrors; public bool gameCleanupVerified, sceneCleanupVerified, prefabUnchanged;
            public bool armed, earlyObserver, leaseReleased, persistentEmpty, diskUnchanged, gameReady, newWindow, finished, eventSystemActive, inputModuleCorrect, raycasterActive;
            public int opens, closes, newScenes, restores, saves, reopens, exports, playEntries, playExits, sceneLoads, engineEditReturns, eventAdds, eventRemoves, handlerAdds, handlerRemoves, reloadResumes, queueCalls, gameAdds, gameRemoves, gameRestores, windowCloses, raycastCalls, leaseProbes, firstFrame, lastFrame, naturalCallbacks, validityEvents, gameWindowId, previousSelection, customIndex, initialCustomCount, gameUpdates;
            public double deadline, playDeadline; public string eventSystem, inputModule, stopReason; public Setup[] originalSetup; public string[] sceneObjectIds, targets, viewIds;
            public Modification[] modifications; public List<Sample> samples = new List<Sample>(); public List<string> secondaryErrors = new List<string>(); public List<Ray> rays = new List<Ray>();
        }
        static Activation activation; static Report report; static bool hooked, finishing; static Scene scene; static FightMatchPlayerHost host;
        static object responsive; static EventInfo validity; static Action<bool> observer;
        static FightMatchHostFirstFrameProbe() { Resume(); }
        [InitializeOnEnterPlayMode] static void Entering(EnterPlayModeOptions options) { Resume(); }
        static double Now { get { return DateTime.UtcNow.Ticks / (double)TimeSpan.TicksPerSecond; } }
        static void Require(bool ok, string why) { if (!ok) throw new InvalidOperationException("HOST-NEXT: " + why); }
        static string Hash(string path) { using (var h = SHA256.Create()) return Hex(h.ComputeHash(File.ReadAllBytes(path))); }
        static string Hex(byte[] data) { return BitConverter.ToString(data).Replace("-", "").ToLowerInvariant(); }
        static string Id(UnityEngine.Object value) { return ReferenceEquals(value, null) ? "null" : GlobalObjectId.GetGlobalObjectIdSlow(value).ToString(); }
        static string Num(float v) { return v.ToString("R", CultureInfo.InvariantCulture); }
        static bool Fin(float v) { return !float.IsNaN(v) && !float.IsInfinity(v); }
        static object Read(object obj, string name)
        {
            Require(obj != null, "read " + name + " on null"); var t = obj.GetType(); var f = t.GetField(name, Flags);
            if (f != null) return f.GetValue(obj); var p = t.GetProperty(name, Flags); Require(p != null, "member " + name); return p.GetValue(obj);
        }
        static bool Bool(object obj, string name) { return (bool)Read(obj, name); }
        static string Arg(string name)
        {
            var args = Environment.GetCommandLineArgs(); Require(args.Count(x => x == name) == 1, "one " + name); return args[Array.IndexOf(args, name) + 1];
        }
        static T[] Components<T>() where T : Component { return scene.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<T>(true)).ToArray(); }
        static void Persist() { if (report != null && report.armed) SessionState.SetString(StateKey, JsonUtility.ToJson(report)); }
        static void Resume()
        {
            if (report == null)
            {
                var value = SessionState.GetString(StateKey, ""); if (value.Length == 0) return;
                report = JsonUtility.FromJson<Report>(value); activation = JsonUtility.FromJson<Activation>(File.ReadAllText(Evidence + "/activation.json")); report.reloadResumes++;
            }
            if (!report.armed || hooked) return;
            EditorApplication.update += Tick; EditorApplication.playModeStateChanged += Mode; SceneManager.sceneLoaded += Loaded;
            Canvas.preWillRenderCanvases += PreCanvas; Canvas.willRenderCanvases += WillCanvas; AssemblyReloadEvents.beforeAssemblyReload += BeforeReload;
            hooked = true; report.handlerAdds += 6; Persist();
        }
        static void Detach()
        {
            if (observer != null && responsive != null) { validity.GetRemoveMethod(true).Invoke(responsive, new object[] { observer }); report.eventRemoves++; observer = null; }
            if (!hooked) return;
            EditorApplication.update -= Tick; EditorApplication.playModeStateChanged -= Mode; SceneManager.sceneLoaded -= Loaded;
            Canvas.preWillRenderCanvases -= PreCanvas; Canvas.willRenderCanvases -= WillCanvas; AssemblyReloadEvents.beforeAssemblyReload -= BeforeReload;
            hooked = false; report.handlerRemoves += 6;
        }
        static void BeforeReload() { try { Detach(); Persist(); } catch (Exception ex) { Error(ex); Persist(); } }
        static void Error(Exception ex) { if (string.IsNullOrEmpty(report.firstError)) report.firstError = ex.ToString(); else report.secondaryErrors.Add(ex.ToString()); }
        static string Relative(Transform root, Transform child)
        {
            var parts = new List<string>(); for (var n = child; n != root; n = n.parent) { Require(n != null, "target root"); parts.Insert(0, n.name); } return string.Join("/", parts);
        }
        static void IdentityCheck()
        {
            host = Components<FightMatchPlayerHost>().Single(); var events = Components<EventSystem>().Single(); var root = host.RuntimeRoot.gameObject;
            Require(PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(root) == PrefabPath, "real prefab");
            var canvas = root.GetComponentsInChildren<Canvas>(true).Single(); var source = PrefabUtility.GetCorrespondingObjectFromSource((RectTransform)canvas.transform); var gid = GlobalObjectId.GetGlobalObjectIdSlow(source);
            Require(gid.targetObjectId == 1013562322995895578UL && gid.assetGUID.ToString() == activation.prefabGuid, "source Canvas");
            report.sceneObjectIds = new UnityEngine.Object[] { host.gameObject, host, host.RuntimeRoot.gameObject, host.RuntimeRoot, events.gameObject, events }.Select(Id).ToArray();
            Require(report.sceneObjectIds.SequenceEqual(activation.originalSceneObjectIds.Select(x => x.Replace(activation.originalSceneGuid, activation.sceneGuid))), "six original object IDs");
            var marker = Components<Transform>().Single(x => x.name == "FightMatchPersistenceProbe_N11");
            Require(marker.parent == null && marker.childCount == 0 && marker.GetComponents<Component>().Length == 1 && marker.GetType() == typeof(Transform), "pure marker");
            var safe = root.transform.Find("RuntimeCanvas/SafeAreaRoot"); var binding = AssetDatabase.LoadAssetAtPath<MonoScript>("Assets/Scripts/FightMatch/Presentation/Localization/LocalizedTmpText.cs").GetClass();
            var texts = root.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true);
            report.targets = texts.Select(t => { var input = t.GetComponentInParent<TMPro.TMP_InputField>(true); return Relative(safe, t.transform) + "|" + (input != null && input.textComponent == t ? "Input" : t.GetComponent(binding) != null ? "Binding" : "License"); }).OrderBy(x => x, StringComparer.Ordinal).ToArray();
            Require(report.targets.SequenceEqual(activation.expectedTargets) && texts.Length == 141 && root.GetComponentsInChildren<TMPro.TMP_InputField>(true).Length == 3, "141 text/3 input");
            var view = AssetDatabase.LoadAssetAtPath<MonoScript>("Assets/Scripts/FightMatch/Presentation/FightMatchViewId.cs").GetClass();
            report.viewIds = root.GetComponentsInChildren(view, true).Select(x => new SerializedObject(x).FindProperty("id").stringValue).ToArray();
            Require(report.viewIds.All(x => !string.IsNullOrEmpty(x)) && report.viewIds.Distinct().Count() == report.viewIds.Length && report.viewIds.Count(x => x == "fm.action.history.open") == 1 && report.viewIds.Count(x => x == "fm.popup.reference") == 1, "stable IDs");
            report.modifications = (PrefabUtility.GetPropertyModifications(root) ?? Array.Empty<PropertyModification>()).Select(x => x == null ? new Modification { isNull = true } : new Modification { target = Id(x.target), property = x.propertyPath, value = x.value, reference = Id(x.objectReference), propertyNull = x.propertyPath == null, valueNull = x.value == null }).ToArray();
            Require(File.ReadAllText(ScenePath).Split(new[] { "    - target:" }, StringSplitOptions.None).Length - 1 == 106, "fixed106 disk envelope");
        }
        static Sample Snapshot(string stage, bool gate = false)
        {
            var view = host.RuntimeRoot; responsive = Read(view, "responsiveLayout"); var canvas = (Canvas)Read(responsive, "canvas");
            var rect = (RectTransform)canvas.transform; var safe = (Component)Read(responsive, "safeArea"); var sr = ((RectTransform)safe.transform).rect; var scale = rect.lossyScale; var factor = canvas.scaleFactor;
            var group = (CanvasGroup)Read(responsive, "battleInteraction"); float k = Mathf.Min(sr.width, 508) / 508;
            var values = new List<float> { rect.anchorMax.x, rect.anchorMax.y, rect.localScale.x, rect.localScale.y, rect.localScale.z, scale.x, scale.y, scale.z, rect.rect.x, rect.rect.y, rect.rect.width, rect.rect.height, canvas.pixelRect.x, canvas.pixelRect.y, canvas.pixelRect.width, canvas.pixelRect.height, factor, rect.anchorMin.x, rect.anchorMin.y, rect.pivot.x, rect.pivot.y, rect.anchoredPosition.x, rect.anchoredPosition.y };
            var corners = new Vector3[4]; rect.GetWorldCorners(corners);
            foreach (var p in corners) { values.AddRange(new[] { p.x, p.y, p.z }); var v = RectTransformUtility.WorldToScreenPoint(canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera, p); values.AddRange(new[] { v.x, v.y }); }
            for (int i = 0; i < 16; i++) values.Add(rect.localToWorldMatrix[i]); for (int i = 0; i < 16; i++) values.Add(rect.parent == null ? Matrix4x4.identity[i] : rect.parent.localToWorldMatrix[i]);
            var fields = new[] { "safeArea", "canvas", "screenLayer", "topBar", "battleContent", "stage", "battleStatus", "boardRegion", "bottomHud", "normalHudRoot", "historyDrawerRoot", "referenceDrawerRoot", "normalStatusViewport", "normalMainRow", "startupViewport", "navigationViewport", "resultViewport", "layoutDiagnostic", "layoutDiagnosticText", "battleInteraction" };
            var panels = (Array)Read(responsive, "dialogPanels"); var bindings = fields.All(x => Read(responsive, x) != null) && panels.Length == 7 && panels.Cast<object>().All(x => x != null);
            return new Sample { stage = stage, utc = DateTime.UtcNow.ToString("O"), frame = Time.frameCount, playing = EditorApplication.isPlaying, paused = EditorApplication.isPaused, screenWidth = Screen.width, screenHeight = Screen.height,
                safeVersion = Convert.ToInt64(Read(safe, "Version")), bound = Bool(view, "bound"), cancelled = Bool(host, "startupCancelled"), localeLoaded = Bool(host, "localeLoaded"), sessionNull = host.Session == null,
                selectionNull = Read(view, "selectLocalePreference") == null, selectionEnabled = Bool(host, "localeSelectionEnabled"), listeners = ((ICollection)Read(view, "listeners")).Count, leaseNull = Read(host, "acceptanceStorageLease") == null,
                responsiveSubscribed = Bool(responsive, "subscribed"), diagnostic = (string)Read(view, "DiagnosticCode"), diagnosticVisible = Bool(view, "DiagnosticVisible"),
                effectiveRoot = (string)Read(host, "EffectivePersistentDataPath"), productRoot = host.CanonicalProductRoot, gate = gate, layoutGateReleased = !Bool(responsive, "validityKnown"), bindingsPresent = bindings,
                applyUsable = bindings && Fin(sr.width) && Fin(sr.height) && Fin(factor) && factor > 0 && Fin(scale.x) && Fin(scale.y) && scale.x > 0 && scale.y > 0 && sr.width > 64 && sr.height >= 812 * k && sr.height - 64 - 384 * k >= 48 * k,
                canvasPositive = values.All(Fin) && rect.rect.width > 0 && rect.rect.height > 0 && canvas.pixelRect.width > 0 && canvas.pixelRect.height > 0 && scale.x > 0 && scale.y > 0 && factor > 0,
                supportedSize = Screen.width == 540 && Screen.height == 960, geometry = values.Select(Num).ToArray(), safeArea = new[] { Num(sr.x), Num(sr.y), Num(sr.width), Num(sr.height), Num(Screen.safeArea.x), Num(Screen.safeArea.y), Num(Screen.safeArea.width), Num(Screen.safeArea.height) },
                interactable = group.interactable, blocksRaycasts = group.blocksRaycasts, driver = Id(rect.drivenByObject), canvasId = Id(canvas), hostId = Id(host), renderMode = canvas.renderMode.ToString() };
        }
        static void Loaded(Scene loaded, LoadSceneMode mode)
        {
            if (report == null || !report.armed || !EditorApplication.isPlaying || loaded.path != ScenePath) return;
            try { report.sceneLoads++; ObserveStart(loaded, true); } catch (Exception ex) { Error(ex); Persist(); }
        }
        static void ObserveStart(Scene loaded, bool sceneLoaded)
        {
            if (host != null && observer != null) return;
            scene = loaded; host = Components<FightMatchPlayerHost>().Single(); var sample = Snapshot(sceneLoaded ? "sceneLoaded-before-Start" : "late-EnteredPlayMode");
            report.earlyObserver = sceneLoaded && !sample.localeLoaded && !sample.cancelled && sample.sessionNull && !sample.bound && Read(host, "acceptancePersistentRoot") == null;
            report.samples.Add(sample); report.firstFrame = Time.frameCount; report.playDeadline = Now + 10;
            validity = responsive.GetType().GetEvent("ValidityChanged", Flags); Require(validity != null, "layout event");
            observer = value => { try { report.validityEvents++; if (report.samples.Count < 20) report.samples.Add(Snapshot("synchronous-ValidityChanged", value)); Persist(); } catch (Exception ex) { Error(ex); Persist(); } };
            validity.GetAddMethod(true).Invoke(responsive, new object[] { observer }); report.eventAdds++; Persist();
        }
        static void PreCanvas() { CanvasSample("natural-preWillRender"); }
        static void WillCanvas() { CanvasSample("natural-willRender"); }
        static void CanvasSample(string kind)
        {
            if (report == null || report.phase != "play" || !EditorApplication.isPlaying || host == null) return;
            try { report.naturalCallbacks++; if (report.samples.Count < 18) report.samples.Add(Snapshot(kind)); report.lastFrame = Time.frameCount; Persist(); } catch (Exception ex) { Error(ex); Persist(); }
        }
        static void Mode(PlayModeStateChange mode)
        {
            if (report == null || !report.armed) return;
            try
            {
                if (mode == PlayModeStateChange.EnteredPlayMode) { if (host == null) ObserveStart(SceneManager.GetSceneByPath(ScenePath), false); report.phase = "play"; Persist(); }
                if (mode == PlayModeStateChange.EnteredEditMode) { report.engineEditReturns++; Finish(); }
            }
            catch (Exception ex) { Error(ex); if (EditorApplication.isPlaying) StopPlay("exception"); else Finish(); }
        }
        static object GameGroup()
        {
            var a = typeof(UnityEditor.Editor).Assembly; var t = a.GetType("UnityEditor.GameViewSizes", true);
            var sizes = typeof(ScriptableSingleton<>).MakeGenericType(t).GetProperty("instance", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy).GetValue(null);
            return t.GetMethod("GetGroup").Invoke(sizes, new[] { Enum.Parse(a.GetType("UnityEditor.GameViewSizeGroupType", true), "Standalone") });
        }
        static PropertyInfo Selection { get { return typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GameView", true).GetProperty("selectedSizeIndex", Flags); } }
        static string[] GameItems(object group)
        {
            int count = (int)group.GetType().GetMethod("GetTotalCount").Invoke(group, null); var values = new List<string>();
            for (int i = 0; i < count; i++)
            {
                var item = group.GetType().GetMethod("GetGameViewSize").Invoke(group, new object[] { i });
                values.Add(Read(item, "baseText") + "|" + Read(item, "sizeType") + "|" + Read(item, "width") + "|" + Read(item, "height"));
            }
            return values.ToArray();
        }
        static void SetupGame()
        {
            var a = typeof(UnityEditor.Editor).Assembly; var t = a.GetType("UnityEditor.GameView", true); var existing = Resources.FindObjectsOfTypeAll(t); var w = EditorWindow.GetWindow(t); w.Show();
            report.newWindow = !existing.Contains(w); report.gameWindowId = w.GetInstanceID(); report.previousSelection = (int)Selection.GetValue(w);
            var group = GameGroup(); report.originalGameItems = GameItems(group); report.builtinCount = (int)group.GetType().GetMethod("GetBuiltinCount").Invoke(group, null);
            Require(!report.originalGameItems.Any(x => x.StartsWith(Nonce + "|", StringComparison.Ordinal)), "fresh GameView name");
            report.customIndex = report.initialCustomCount = (int)group.GetType().GetMethod("GetCustomCount").Invoke(group, null);
            var size = Activator.CreateInstance(a.GetType("UnityEditor.GameViewSize", true), Enum.Parse(a.GetType("UnityEditor.GameViewSizeType", true), "FixedResolution"), 540, 960, Nonce);
            group.GetType().GetMethod("AddCustomSize").Invoke(group, new[] { size }); report.gameAdds++;
            Selection.SetValue(w, (int)group.GetType().GetMethod("GetBuiltinCount").Invoke(group, null) + report.customIndex); w.Repaint();
        }
        static void RestoreGame()
        {
            if (report.gameAdds == 0) return;
            var w = (EditorWindow)EditorUtility.InstanceIDToObject(report.gameWindowId); Require(w != null, "owned GameView retained");
            try
            {
                Selection.SetValue(w, report.previousSelection); report.gameRestores++; var group = GameGroup();
                int builtins = (int)group.GetType().GetMethod("GetBuiltinCount").Invoke(group, null);
                var items = GameItems(group); var found = Enumerable.Range(builtins, items.Length - builtins).Where(i => items[i] == Nonce + "|FixedResolution|540|960").ToArray();
                Require(builtins == report.builtinCount && found.Length == 1, "unique current custom GameView item");
                Require(items.Where((x, i) => i != found[0]).SequenceEqual(report.originalGameItems), "other GameView entries retained");
                report.deleteTotalIndex = found[0]; report.deleteRelativeIndex = found[0] - builtins;
                group.GetType().GetMethod("RemoveCustomSize").Invoke(group, new object[] { report.deleteTotalIndex }); report.gameRemoves++; w.Repaint();
                report.finalGameItems = GameItems(group); report.finalSelection = (int)Selection.GetValue(w);
                report.gameCleanupVerified = report.finalGameItems.SequenceEqual(report.originalGameItems) && report.finalSelection == report.previousSelection;
                Require(report.gameCleanupVerified && !report.finalGameItems.Any(x => x.StartsWith(Nonce + "|", StringComparison.Ordinal)), "actual GameView deletion and original selection");
            }
            finally { if (report.newWindow) { w.Close(); report.windowCloses++; } }
        }
        static void Raycast(Component target, string label, EventSystem es)
        {
            var rect = (RectTransform)target.transform; var canvas = target.GetComponentInParent<Canvas>(); var center = rect.TransformPoint(rect.rect.center);
            var point = RectTransformUtility.WorldToScreenPoint(canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera, center); var hits = new List<RaycastResult>();
            report.raycastCalls++; es.RaycastAll(new PointerEventData(es) { position = point }, hits);
            var diagnostic = ((GameObject)Read(host.RuntimeRoot, "blockingDiagnostic")).transform;
            report.rays.Add(new Ray { target = label, x = point.x, y = point.y, obstruction = hits.Count == 0 ? "NO_HIT" : hits[0].gameObject.transform.IsChildOf(diagnostic) ? "DIAGNOSTIC_TOP" : "OTHER_TOP", hits = hits.Select(x => new Hit { target = Id(x.gameObject), name = x.gameObject.name, root = Id(x.gameObject.transform.root.gameObject), module = x.module == null ? "null" : x.module.GetType().FullName, depth = x.depth, sortingOrder = x.sortingOrder, distance = x.distance, underDiagnostic = x.gameObject.transform.IsChildOf(diagnostic) }).ToArray() });
        }
        static string GeometryVerdict(bool earlyObserver, Sample[] samples)
        {
            var first = samples.FirstOrDefault(x => x.stage == "synchronous-ValidityChanged");
            var natural = samples.Where(x => x.stage.StartsWith("natural-")).ToArray();
            if (earlyObserver && first != null && first.applyUsable && !first.gate) return "FAIL";
            if (earlyObserver && first != null && first.gate && first.applyUsable && first.canvasPositive && first.supportedSize &&
                natural.Any(x => x.canvasPositive && x.supportedSize && x.safeVersion > first.safeVersion &&
                    x.cancelled && !x.bound && !x.responsiveSubscribed && !x.gate && x.layoutGateReleased)) return "PASS";
            return "UNOBSERVED";
        }
        static Sample CounterexampleNatural()
        {
            return new Sample { stage = "natural-willRender", canvasPositive = true, supportedSize = true, safeVersion = 1,
                cancelled = true, bound = false, responsiveSubscribed = false, gate = false, layoutGateReleased = true };
        }
        static void CheckGeometryCase(string name, Sample first, Sample natural, Sample terminal, string expected)
        {
            var actual = GeometryVerdict(true, new[] { first, natural, terminal });
            var result = new PredicateCase { name = name, expected = expected, actual = actual, passed = actual == expected };
            report.geometryPredicateChecks.Add(result); Require(result.passed, "independent DTO geometry counterexample " + name);
        }
        static void RunGeometryPredicateChecks()
        {
            var first = new Sample { stage = "synchronous-ValidityChanged", gate = true, applyUsable = true, canvasPositive = true, supportedSize = true, safeVersion = 0 };
            var terminal = CounterexampleNatural(); terminal.stage = "after-natural-frames-cancelled";
            terminal.diagnostic = "LocalizationNotReady"; terminal.diagnosticVisible = true; terminal.sessionNull = true; terminal.selectionNull = true; terminal.leaseNull = true;
            var beforeCancel = CounterexampleNatural(); beforeCancel.cancelled = false; beforeCancel.bound = true; beforeCancel.responsiveSubscribed = true; beforeCancel.layoutGateReleased = false;
            CheckGeometryCase("good-geometry-before-cancellation-with-correct-later-terminal", first, beforeCancel, terminal, "UNOBSERVED");
            CheckGeometryCase("true-post-cancellation-natural", first, CounterexampleNatural(), terminal, "PASS");
            foreach (var missing in new[] { "cancelled", "unbound", "unsubscribed", "released-validity-state", "natural-gate-false" })
            {
                var sample = CounterexampleNatural();
                if (missing == "cancelled") sample.cancelled = false;
                if (missing == "unbound") sample.bound = true;
                if (missing == "unsubscribed") sample.responsiveSubscribed = true;
                if (missing == "released-validity-state") sample.layoutGateReleased = false;
                if (missing == "natural-gate-false") sample.gate = true;
                CheckGeometryCase("missing-" + missing, first, sample, terminal, "UNOBSERVED");
            }
            first.gate = false;
            CheckGeometryCase("original-usable-first-gate-false-remains-fail", first, CounterexampleNatural(), terminal, "FAIL");
        }
        static void ObserveEnd()
        {
            var s = Snapshot("after-natural-frames-cancelled"); report.samples.Add(s); report.lastFrame = Time.frameCount;
            report.A = s.localeLoaded && s.cancelled && s.sessionNull && s.effectiveRoot == activation.hostSaveIsolation.persistentRoot && s.productRoot == activation.hostSaveIsolation.persistentRoot + "/FightMatch" && s.diagnostic == "LocalizationNotReady" ? "PASS" : "FAIL";
            report.B = s.playing && !s.paused && report.lastFrame - report.firstFrame >= 2 && report.naturalCallbacks > 0 ? "PASS" : "UNOBSERVED";
            report.C = GeometryVerdict(report.earlyObserver, report.samples.ToArray());
            var es = Components<EventSystem>().Single(); report.eventSystem = Id(EventSystem.current); report.eventSystemActive = es == EventSystem.current && es.isActiveAndEnabled;
            report.inputModule = es.currentInputModule == null ? "null" : es.currentInputModule.GetType().FullName;
            report.inputModuleCorrect = report.inputModule == "FightMatch.Presentation.FightMatchStandaloneInputModule";
            report.raycasterActive = host.RuntimeRoot.GetComponentsInChildren<GraphicRaycaster>(true).Any(x => x.isActiveAndEnabled);
            Raycast(((GameObject)Read(host.RuntimeRoot, "blockingDiagnostic")).transform, "diagnostic", es); Raycast((Component)Read(host.RuntimeRoot, "languageButton"), "languageButton", es);
            report.leaseProbes++; try { using (new FileStream(Path.GetDirectoryName(activation.hostSaveIsolation.persistentRoot) + "/owner.lock", FileMode.Open, FileAccess.ReadWrite, FileShare.None)) { report.leaseReleased = true; } } catch (IOException) { report.leaseReleased = false; }
            report.persistentEmpty = !Directory.EnumerateFileSystemEntries(activation.hostSaveIsolation.persistentRoot).Any();
            report.D = s.diagnostic == "LocalizationNotReady" && s.diagnosticVisible && s.sessionNull && !s.bound && s.selectionNull && !s.selectionEnabled && s.listeners == 0 && !s.responsiveSubscribed && s.leaseNull && report.leaseReleased && report.persistentEmpty ? "PASS" : "FAIL";
        }
        static void StopPlay(string why)
        {
            if (report.playExits != 0) return; report.stopReason = why;
            try { if (host != null) ObserveEnd(); } catch (Exception ex) { Error(ex); }
            report.phase = "exit"; report.deadline = Now + 15; report.playExits++; Persist(); EditorApplication.ExitPlaymode();
        }
        static void Tick()
        {
            try
            {
                if (report.phase == "size")
                {
                    report.gameUpdates++; report.gameReady = Screen.width == 540 && Screen.height == 960;
                    if (!report.gameReady && report.gameUpdates < 20 && Now < report.deadline) { report.queueCalls++; EditorApplication.QueuePlayerLoopUpdate(); return; }
                    Require(Hash(ScenePath) == activation.sceneSha256 && Hash(ScenePath + ".meta") == activation.sceneMetaSha256, "scene fixed before open");
                    report.opens++; scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single); IdentityCheck();
                    report.phase = "play"; report.deadline = Now + 15; report.playEntries++; Persist(); EditorApplication.EnterPlaymode();
                }
                else if (report.phase == "play")
                {
                    if (EditorApplication.isPlaying && host != null)
                    {
                        if (Time.frameCount - report.firstFrame >= 2 && report.naturalCallbacks > 0) StopPlay("two frames and natural Canvas");
                        else if (Time.frameCount - report.firstFrame >= 10 || Now >= report.playDeadline) StopPlay("bounded observation end");
                        else { report.queueCalls++; EditorApplication.QueuePlayerLoopUpdate(); }
                    }
                    else if (Now >= report.deadline) { Error(new TimeoutException("Play entry/observer deadline")); if (EditorApplication.isPlaying) StopPlay("entry timeout"); else Finish(); }
                }
                else if (report.phase == "exit" && Now >= report.deadline) { Error(new TimeoutException("EditMode return deadline")); Finish(); }
            }
            catch (Exception ex) { Error(ex); if (EditorApplication.isPlaying) StopPlay("observation error"); else Finish(); }
        }
        static void CleanupLog(string message, string stack, LogType kind)
        {
            if (kind != LogType.Error && kind != LogType.Exception && kind != LogType.Assert) return;
            report.cleanupLogErrors++; Error(new InvalidOperationException("Unity cleanup log " + kind + ": " + message + "\n" + stack));
        }
        static void Finish()
        {
            if (finishing) return; finishing = true; report.phase = "cleanup";
            try { Detach(); } catch (Exception ex) { Error(ex); }
            UnityEngine.Application.logMessageReceived += CleanupLog; report.handlerAdds++;
            try
            {
                try { RestoreGame(); } catch (Exception ex) { Error(ex); }
                try
                {
                    Require(!EditorApplication.isPlaying, "EditMode cleanup only"); scene = SceneManager.GetSceneByPath(ScenePath);
                    if (scene.IsValid() && scene.isLoaded)
                    {
                        if (SceneManager.sceneCount == 1) { EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive); report.newScenes++; }
                        Require(EditorSceneManager.CloseScene(scene, true), "owned close"); report.closes++;
                    }
                    if (report.originalSetup == null) report.sceneCleanup = "setup-not-captured";
                    else if (report.originalSetup.Length == 0) { report.sceneCleanup = "empty-original/no-restorable-scene; owned scene closed; temporary empty scene released on editor exit"; report.sceneCleanupVerified = !SceneManager.GetSceneByPath(ScenePath).isLoaded; }
                    else if (report.originalSetup.All(x => !string.IsNullOrEmpty(x.path)))
                    {
                        EditorSceneManager.RestoreSceneManagerSetup(report.originalSetup.Select(x => new SceneSetup { path = x.path, isLoaded = x.loaded, isActive = x.active }).ToArray()); report.restores++;
                        var actual = EditorSceneManager.GetSceneManagerSetup(); report.sceneCleanupVerified = actual.Length == report.originalSetup.Length && actual.Select((x, i) => x.path == report.originalSetup[i].path && x.isLoaded == report.originalSetup[i].loaded && x.isActive == report.originalSetup[i].active).All(x => x);
                        report.sceneCleanup = "nonempty-original-restored"; Require(report.sceneCleanupVerified, "original scene setup restored exactly");
                    }
                    else { report.sceneCleanup = "untitled-original-equivalent-empty-scene"; report.sceneCleanupVerified = SceneManager.sceneCount == 1 && string.IsNullOrEmpty(SceneManager.GetSceneAt(0).path) && !SceneManager.GetSceneAt(0).isDirty; Require(report.sceneCleanupVerified, "untitled original equivalent"); }
                }
                catch (Exception ex) { Error(ex); }
            }
            finally
            {
                try { report.diskUnchanged = Hash(ScenePath) == activation.sceneSha256 && Hash(ScenePath + ".meta") == activation.sceneMetaSha256; Require(report.diskUnchanged, "scene bytes unchanged"); } catch (Exception ex) { Error(ex); }
                try { report.prefabUnchanged = Hash(PrefabPath) == report.prefabDiskSha256; Require(report.prefabUnchanged, "prefab bytes unchanged"); } catch (Exception ex) { Error(ex); }
                UnityEngine.Application.logMessageReceived -= CleanupLog; report.handlerRemoves++;
                report.armed = false; SessionState.EraseString(StateKey); report.finished = true; report.phase = "finished";
            }
            report.status = !string.IsNullOrEmpty(report.firstError) || new[] { report.A, report.B, report.C, report.D }.Contains("FAIL") ? "FAILED" : new[] { report.A, report.B, report.C, report.D }.All(x => x == "PASS") ? "OBSERVATION_COMPLETE" : "PARTIAL";
            try { var data = Encoding.UTF8.GetBytes(JsonUtility.ToJson(report, true) + "\n"); Require(data.Length <= 1048576, "observation budget"); using (var f = new FileStream(Evidence + "/P/observations.json", FileMode.CreateNew, FileAccess.Write)) f.Write(data, 0, data.Length); } catch (Exception ex) { Debug.LogError(ex); }
            Debug.Log("[HOST-NEXT] " + report.status + " A=" + report.A + " B=" + report.B + " C=" + report.C + " D=" + report.D + " first=" + report.firstError);
            EditorApplication.Exit(string.IsNullOrEmpty(report.firstError) ? 0 : 1);
        }
        public static void Execute()
        {
            report = new Report();
            try
            {
                activation = JsonUtility.FromJson<Activation>(File.ReadAllText(Evidence + "/activation.json"));
                Require(UnityEngine.Application.isBatchMode && !EditorApplication.isPlaying && Directory.GetCurrentDirectory() == activation.candidateProjectPath, "batch/cwd");
                Require(activation.ownerTurn == Owner && activation.nonce == Nonce && Arg("-fmProbeInput") == Evidence + "/activation.json" && Arg("-fmProbeNonce") == Nonce, "owner/nonce");
                Require(Arg("-fmHostSaveIsolationId") == activation.hostSaveIsolation.activationId && Arg("-fmHostSaveIsolationCandidate") == activation.executionCandidateSha256 && Environment.GetCommandLineArgs().SequenceEqual(activation.argv), "full argv/isolation");
                Require(PlayerSettings.runInBackground && !EditorSettings.enterPlayModeOptionsEnabled, "fixed Play settings");
                report.originalSetup = EditorSceneManager.GetSceneManagerSetup().Select(x => new Setup { path = x.path, loaded = x.isLoaded, active = x.isActive }).ToArray();
                for (int i = 0; i < SceneManager.sceneCount; i++) Require(!SceneManager.GetSceneAt(i).isDirty, "no dirty scene");
                Require(report.originalSetup.Count(x => string.IsNullOrEmpty(x.path)) <= 1 && (report.originalSetup.All(x => !string.IsNullOrEmpty(x.path)) || report.originalSetup.Length == 1), "restorable setup");
                report.activationSha256 = Hash(Evidence + "/activation.json"); report.sourceSha256 = Hash(SourcePath); report.assemblySha256 = Hash(typeof(FightMatchHostFirstFrameProbe).Assembly.Location); Require(report.sourceSha256 == activation.probeSourceSha256, "source");
                var files = new List<string>(); foreach (var dir in new[] { "Assets", "Packages", "ProjectSettings" }) files.AddRange(Directory.GetFiles(dir, "*", SearchOption.AllDirectories).Select(x => x.Replace('\\', '/')));
                var extra = files.Except(activation.candidateFiles.Select(x => x.path)).ToArray(); Require(extra.All(x => x == SourcePath + ".meta" || x == "ProjectSettings/SceneTemplateSettings.json") && files.Contains(SourcePath + ".meta"), "complete input closure");
                foreach (var input in activation.candidateFiles) Require(new FileInfo(input.path).Length == input.bytes && Hash(input.path) == input.sha256, "input " + input.path);
                var canonical = new StringBuilder(); foreach (var path in files.OrderBy(x => x, StringComparer.Ordinal)) { var bytes = File.ReadAllBytes(path); using (var h = SHA1.Create()) { var prefix = Encoding.UTF8.GetBytes("blob " + bytes.Length + "\0"); canonical.Append(path).Append('\0').Append(Hex(h.ComputeHash(prefix.Concat(bytes).ToArray()))).Append('\0').Append(bytes.Length).Append('\n'); } }
                using (var h = SHA256.Create()) report.fullInputCanonicalSha256 = Hex(h.ComputeHash(Encoding.UTF8.GetBytes(canonical.ToString())));
                report.probeMetaSha256 = Hash(SourcePath + ".meta"); report.probeMetaGuid = AssetDatabase.AssetPathToGUID(SourcePath); report.sceneGuid = AssetDatabase.AssetPathToGUID(ScenePath); report.prefabGuid = AssetDatabase.AssetPathToGUID(PrefabPath);
                Require(report.sceneGuid == activation.sceneGuid && report.prefabGuid == activation.prefabGuid, "asset GUIDs");
                report.prefabDiskSha256 = Hash(PrefabPath); report.diskSha256 = Hash(ScenePath); report.diskMetaSha256 = Hash(ScenePath + ".meta"); report.executionCandidateSha256 = activation.executionCandidateSha256;
                RunGeometryPredicateChecks();
                report.armed = true; report.phase = "size"; report.deadline = Now + 5; Resume(); SetupGame(); Persist();
            }
            catch (Exception ex) { Error(ex); Finish(); }
        }
    }
}
