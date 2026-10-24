using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FightMatch.Presentation;
using FlowPuzzle.Core;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using static FightMatch.Core.Tests.LocalSaveTestFiles;

namespace FightMatch.Core.Tests
{
    // Injects device samples into the same module used by the scene. The module performs
    // the actual GraphicRaycaster, ownership, pointer-up and end-drag dispatch.
    internal sealed class UguiPointerDriver : IDisposable
    {
        private readonly GameObject root;
        private readonly UguiTestInput input;
        private readonly UnityEngine.EventSystems.BaseInput previousInput;
        private readonly bool ownsRoot;
        private readonly Dictionary<int, Touch> touches = new Dictionary<int, Touch>();
        internal UnityEngine.EventSystems.EventSystem EventSystem { get; }
        internal FightMatchStandaloneInputModule Module { get; }

        internal UguiPointerDriver()
        {
            root = new GameObject("BattleTestEventSystem");
            root.SetActive(false);
            ownsRoot = true;
            EventSystem = root.AddComponent<UnityEngine.EventSystems.EventSystem>();
            EventSystem.sendNavigationEvents = false;
            Module = root.AddComponent<FightMatchStandaloneInputModule>();
            input = root.AddComponent<UguiTestInput>();
            Module.inputOverride = input;
            Module.runInEditMode = true;
            root.SetActive(true);
        }

        internal UguiPointerDriver(UnityEngine.EventSystems.EventSystem eventSystem)
        {
            EventSystem = eventSystem ?? throw new ArgumentNullException(nameof(eventSystem));
            root = eventSystem.gameObject;
            Module = root.GetComponent<FightMatchStandaloneInputModule>();
            Assert.IsNotNull(Module, "The scene input module is required.");
            previousInput = Module.inputOverride;
            input = root.AddComponent<UguiTestInput>();
            Module.inputOverride = input;
        }

        internal List<UnityEngine.EventSystems.RaycastResult> Raycast(Vector2 position)
        {
            Canvas.ForceUpdateCanvases();
            var hits = new List<UnityEngine.EventSystems.RaycastResult>();
            EventSystem.RaycastAll(new UnityEngine.EventSystems.PointerEventData(EventSystem) { position = position }, hits);
            return hits;
        }

        internal void Down(Vector2 position, int id = -1)
        { if (id >= 0) Touch(id, position, TouchPhase.Began); else Mouse(position, true, false, true); }
        internal void Move(Vector2 position, int id = -1)
        { if (id >= 0) Touch(id, position, TouchPhase.Moved); else Mouse(position, false, false, true); }
        internal void Up(Vector2 position, int id = -1)
        { if (id >= 0) Touch(id, position, TouchPhase.Ended); else Mouse(position, false, true, false); }
        internal void Cancel(int id, Vector2 position)
        {
            if (id >= 0) Touch(id, position, TouchPhase.Canceled);
            else
            {
                var target = EventSystem.currentSelectedGameObject;
                if (target != null) EventSystem.SetSelectedGameObject(null);
                foreach (var hit in Raycast(position))
                {
                    var board = hit.gameObject.GetComponentInParent<CandidateBoardElement>();
                    if (board != null) { board.CancelPointer(); break; }
                }
            }
        }
        private void Touch(int id, Vector2 position, TouchPhase phase)
        {
            foreach (var key in new List<int>(touches.Keys))
            { var value = touches[key]; value.phase = TouchPhase.Stationary; touches[key] = value; }
            touches[id] = new Touch { fingerId = id, position = position, phase = phase, type = TouchType.Direct };
            TouchFrame(new List<Touch>(touches.Values).ToArray());
            if (phase == TouchPhase.Ended || phase == TouchPhase.Canceled) touches.Remove(id);
        }
        internal void TouchFrame(params Touch[] samples)
        {
            input.Touches = samples ?? Array.Empty<Touch>(); input.MousePresent = false;
            Canvas.ForceUpdateCanvases(); Module.Process(); input.Touches = Array.Empty<Touch>();
        }
        private void Mouse(Vector2 position, bool down, bool up, bool held)
        {
            input.MousePresent = true; input.Position = position; input.Down = down; input.Up = up; input.Held = held;
            Canvas.ForceUpdateCanvases(); Module.Process(); input.Down = false; input.Up = false;
        }
        internal void Click(UnityEngine.UI.Button button)
        {
            Assert.IsNotNull(button);
            var point = RectTransformUtility.WorldToScreenPoint(null, ((RectTransform)button.transform).TransformPoint(((RectTransform)button.transform).rect.center));
            var hits = Raycast(point);
            Assert.IsNotEmpty(hits, "The button must be reached by a real GraphicRaycaster.");
            Assert.IsTrue(hits[0].gameObject == button.gameObject || hits[0].gameObject.transform.IsChildOf(button.transform),
                "The button must be the foremost interactive hit.");
            Down(point); Up(point);
        }
        internal static Vector2 Point(CandidateBoardElement board, FlowPos cell)
        { Canvas.ForceUpdateCanvases(); return RectTransformUtility.WorldToScreenPoint(null, board.rectTransform.TransformPoint(board.CellCenter(cell))); }
        public void Dispose()
        {
            if (root == null) return;
            if (ownsRoot) UnityEngine.Object.DestroyImmediate(root);
            else { Module.inputOverride = previousInput; UnityEngine.Object.DestroyImmediate(input); }
        }
    }

    internal sealed class UguiTestInput : UnityEngine.EventSystems.BaseInput
    {
        internal Touch[] Touches = Array.Empty<Touch>();
        internal bool MousePresent, Down, Up, Held;
        internal Vector2 Position;
        public override bool mousePresent => MousePresent;
        public override Vector2 mousePosition => Position;
        public override Vector2 mouseScrollDelta => Vector2.zero;
        public override bool touchSupported => true;
        public override int touchCount => Touches.Length;
        public override Touch GetTouch(int index) => Touches[index];
        public override bool GetMouseButtonDown(int button) => button == 0 && Down;
        public override bool GetMouseButtonUp(int button) => button == 0 && Up;
        public override bool GetMouseButton(int button) => button == 0 && Held;
        public override float GetAxisRaw(string axisName) => 0;
        public override bool GetButtonDown(string buttonName) => false;
    }

    // The same serialized binder types are assembled in memory; no test-only runtime API or reflection.
    internal sealed class BattleUguiRoot : IDisposable
    {
        private static UguiPointerDriver sharedDriver;
        private static int users;
        private int retained;
        internal readonly GameObject Root;
        internal readonly LocalizationService Localization = new LocalizationService(new UguiTestTextSource(), SystemLanguage.ChineseSimplified);
        internal UguiPointerDriver Driver => sharedDriver;
        internal readonly RectTransform StaleButtons;
        internal BattleUguiRoot()
        {
            if (users++ == 0) sharedDriver = new UguiPointerDriver();
            Root = new GameObject("BattleTestCanvas", typeof(RectTransform), typeof(Canvas), typeof(UnityEngine.UI.GraphicRaycaster));
            Root.SetActive(false);
            Root.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var raycasters = Root.GetComponents<UnityEngine.UI.GraphicRaycaster>(); Assert.AreEqual(1, raycasters.Length);
            raycasters[0].runInEditMode = true;
            StaleButtons = Container(Root.transform, "DetachedControls");
            Root.SetActive(true);
        }
        internal static RectTransform Container(Transform parent, string name)
        {
            var result = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            result.SetParent(parent, false); result.anchorMin = result.anchorMax = result.pivot = Vector2.zero;
            result.sizeDelta = new Vector2(600, 600); return result;
        }
        private static void Set(UnityEngine.Object owner, string property, UnityEngine.Object value)
        {
            var serialized = new SerializedObject(owner); var field = serialized.FindProperty(property);
            Assert.IsNotNull(field, property); field.objectReferenceValue = value; serialized.ApplyModifiedPropertiesWithoutUndo();
        }
        private static void SetArray(UnityEngine.Object owner, string name, UnityEngine.Object[] values)
        {
            var serialized = new SerializedObject(owner); var property = serialized.FindProperty(name);
            Assert.IsNotNull(property, name); property.arraySize = values.Length;
            for (var i = 0; i < values.Length; i++) property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
        private static LocalizedTmpText Text(Transform parent, string name)
        {
            var rect = Container(parent, name); rect.sizeDelta = new Vector2(500, 48);
            var text = rect.gameObject.AddComponent<TMPro.TextMeshProUGUI>();
            text.text = LocalizedTmpText.Placeholder; text.raycastTarget = false;
            text.font = AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>("Assets/UI/FightMatch/Fonts/NotoSansCJKsc-Regular-TMP.asset");
            Assert.IsNotNull(text.font, "The generated and reopened Noto TMP asset is required.");
            var binding = rect.gameObject.AddComponent<LocalizedTmpText>(); Set(binding, "target", text);
            rect.gameObject.AddComponent<FightMatchViewId>().Assign(FightMatchViewId.Row(name)); return binding;
        }
        private static UnityEngine.UI.Button Button(Transform parent, string name, string id = null)
        {
            var rect = Container(parent, name); rect.sizeDelta = new Vector2(240, 48); rect.anchoredPosition = new Vector2(300, 0);
            var graphic = rect.gameObject.AddComponent<UnityEngine.UI.Image>(); graphic.raycastTarget = true;
            var button = rect.gameObject.AddComponent<UnityEngine.UI.Button>(); button.targetGraphic = graphic;
            rect.gameObject.AddComponent<FightMatchViewId>().Assign(id ?? FightMatchViewId.Row(name));
            var label = Text(rect, name + "-label"); UnityEngine.Object.DestroyImmediate(label.GetComponent<FightMatchViewId>());
            label.GetComponent<RectTransform>().sizeDelta = rect.sizeDelta; return button;
        }
        internal CandidateBoardInputView Input(Transform parent = null)
        {
            var root = Container(parent ?? Root.transform, "Input"); var view = root.gameObject.AddComponent<CandidateBoardInputView>();
            foreach (var name in new[] { "phase", "save", "availability" })
            {
                var id = name == "phase" ? "phase-status" : name == "save" ? "save-status" : "input-status";
                Set(view, name, Text(root, id));
            }
            foreach (var family in new[] { "ally", "enemy", "member" })
            {
                var slots = new RectTransform[3]; var names = new LocalizedTmpText[3];
                var hp = new LocalizedTmpText[3]; var intent = new LocalizedTmpText[3];
                for (var slot = 0; slot < 3; slot++)
                {
                    slots[slot] = Container(root, family + "-slot-" + slot);
                    slots[slot].anchoredPosition = new Vector2(300 + slot * 76, 0);
                    slots[slot].sizeDelta = new Vector2(72, 72);
                    if (family == "member") continue;
                    names[slot] = Text(slots[slot], family + "-name-" + slot);
                    hp[slot] = Text(slots[slot], family + "-hp-" + slot);
                    intent[slot] = Text(slots[slot], family + "-intent-" + slot);
                }
                SetArray(view, family == "member" ? "memberSlots" : family + "StageSlots", slots);
                if (family != "member")
                {
                    SetArray(view, family + "Names", names); SetArray(view, family + "Hp", hp); SetArray(view, family + "Intent", intent);
                }
            }
            var template = Button(root, "member-template"); template.gameObject.SetActive(false); Set(view, "memberTemplate", template);
            Set(view, "retry", Button(root, "retry-save", "fm.action.battle.retry"));
            Set(view, "resolve", Button(root, "resolve-save", "fm.action.battle.resolve"));
            var board = Container(root, "Board").gameObject.AddComponent<CandidateBoardElement>();
            board.rectTransform.anchoredPosition = new Vector2(40, 100); board.rectTransform.sizeDelta = new Vector2(160, 160);
            board.gameObject.AddComponent<FightMatchViewId>().Assign("fm.board.candidate"); Set(view, "board", board); return view;
        }
        internal CandidateBattlePlaybackView Playback(Transform parent = null)
        {
            var root = Container(parent ?? Root.transform, "Playback"); var view = root.gameObject.AddComponent<CandidateBattlePlaybackView>();
            Set(view, "inputView", Input(root));
            foreach (var name in new[] { "beat", "stage", "diagnostic" }) Set(view, name, Text(root, "playback-" + name));
            Set(view, "skip", Button(root, "skip-playback", "fm.action.battle.skip")); return view;
        }
        internal PlayerDefaultReferenceView Reference(Transform parent)
        {
            var root = Container(parent, "Reference"); var view = root.gameObject.AddComponent<PlayerDefaultReferenceView>();
            Set(view, "title", Text(root, "reference-title"));
            Set(view, "explanation", Text(root, "reference-explanation")); Set(view, "beat", Text(root, "reference-beat"));
            Set(view, "steps", Container(root, "ReferenceSteps"));
            var template = Button(root, "reference-template"); template.gameObject.SetActive(false); Set(view, "buttonTemplate", template);
            var close = Button(root, "reference-close", FightMatchViewId.Row("reference-close")); Set(view, "closeButton", close);
            root.gameObject.SetActive(false); return view;
        }
        internal FightMatch.Presentation.PlayerBattleView Page()
        {
            var root = Container(Root.transform, "BattlePage"); var view = root.gameObject.AddComponent<FightMatch.Presentation.PlayerBattleView>();
            root.gameObject.AddComponent<FightMatchViewId>().Assign("fm.page.battle");
            var content = Container(root, "BattleContent"); var result = Container(Root.transform, "ResultPage");
            result.gameObject.AddComponent<FightMatchViewId>().Assign("fm.page.result");
            Set(view, "battleContent", content.gameObject); Set(view, "resultRoot", result.gameObject);
            Set(view, "playbackView", Playback(content)); Set(view, "referenceView", Reference(root));
            Set(view, "headline", Text(root, "battle-status")); Set(view, "hud", Text(root, "battle-latest-hud"));
            var normal = Container(content, "NormalHud"); var history = Container(content, "HistoryDrawer");
            Set(view, "normalHudRoot", normal.gameObject); Set(view, "historyDrawerRoot", history.gameObject);
            Set(view, "historyHeader", Text(history, "history-title"));
            Set(view, "historyOpenButton", Button(normal, "HistoryOpen", "fm.action.history.open"));
            Set(view, "historyCloseButton", Button(history, "HistoryClose", FightMatchViewId.Row("history-close")));
            Set(view, "history", Container(history, "HistoryRows")); Set(view, "actions", Container(normal, "DynamicBattleActions"));
            Set(view, "battleNotices", Container(normal, "BattleNotices")); Set(view, "receipt", Container(result, "Receipt"));
            foreach (var family in new[] { "dialog", "recovery" })
            {
                var panel = Container(root, family + "Panel");
                Set(view, family + "Root", panel.gameObject);
                Set(view, family + "Title", Text(panel, family == "dialog" ? "battle-end-title" : "battle-recovery-title"));
                foreach (var section in new[] { "Body", "Choices", "Actions", "DangerActions" })
                    Set(view, family + section, Container(panel, section));
                panel.gameObject.SetActive(false);
            }
            history.gameObject.SetActive(false);
            var button = Button(root, "battle-button-template"); button.gameObject.SetActive(false); Set(view, "buttonTemplate", button);
            var text = Text(root, "battle-text-template"); text.gameObject.SetActive(false); Set(view, "textTemplate", text); return view;
        }
        internal void RemovePage(FightMatch.Presentation.PlayerBattleView page)
        {
            if (page == null) return;
            var serialized = new SerializedObject(page);
            var result = serialized.FindProperty("resultRoot").objectReferenceValue as GameObject;
            page.Dispose(); UnityEngine.Object.DestroyImmediate(page.gameObject);
            if (result != null) UnityEngine.Object.DestroyImmediate(result);
        }
        internal void Keep(UnityEngine.UI.Button button)
        {
            Assert.IsNotNull(button);
            if (button.transform.parent == StaleButtons) return;
            button.GetComponent<FightMatchViewId>().Assign(FightMatchViewId.Row("retained:" + (++retained)));
            button.transform.SetParent(StaleButtons, false);
        }
        internal static T Find<T>(Component view, string id) where T : Component => FightMatchViewId.Find<T>(view.transform.root, id);
        internal static void Submit(UnityEngine.UI.Button button)
        {
            Assert.IsNotNull(button); Assert.IsNotNull(sharedDriver);
            sharedDriver.EventSystem.SetSelectedGameObject(button.gameObject);
            UnityEngine.EventSystems.ExecuteEvents.Execute(button.gameObject,
                new UnityEngine.EventSystems.PointerEventData(sharedDriver.EventSystem) { button = UnityEngine.EventSystems.PointerEventData.InputButton.Left },
                UnityEngine.EventSystems.ExecuteEvents.pointerClickHandler);
        }
        public void Dispose()
        {
            if (Root == null) return;
            UnityEngine.Object.DestroyImmediate(Root);
            if (--users == 0) { sharedDriver.Dispose(); sharedDriver = null; }
        }
    }

    internal static class BattleTestLookup
    {
        internal static T Find<T>(this Component view, string id) where T : Component => FightMatchViewId.Find<T>(view.transform.root, id);
    }

    internal static class PointerType
    {
        internal const string mouse = "mouse", touch = "touch";
    }

    internal sealed class BoardPanelRig : IDisposable
    {
        private static readonly List<BoardPanelRig> open = new List<BoardPanelRig>();
        internal readonly BattleApplicationRig Battle;
        internal readonly BattleUguiRoot Canvas;
        internal readonly CandidateBoardInputView UI;
        internal CandidateBoardInputController Controller => UI.Controller;
        internal CandidateBoardElement Board => UI.Board;
        internal float CellSize = 40;
        internal BoardPanelRig(int level = 1, int hp = 100)
        {
            Battle = new BattleApplicationRig(hp: hp, level: level);
            try
            {
                Canvas = new BattleUguiRoot(); UI = Canvas.Input(); UI.Attach(Battle.System, B(), Canvas.Localization);
                Resize(CellSize); open.Add(this);
            }
            catch { Canvas?.Dispose(); Battle.Dispose(); throw; }
        }
        internal void Resize(float points)
        {
            CellSize = points; var face = Battle.State.Board.Face;
            Board.rectTransform.sizeDelta = new Vector2(face.Width * points, face.Height * points);
        }
        internal IEnumerator Ready()
        {
            yield return null; yield return null; yield return null;
            UnityEngine.Canvas.ForceUpdateCanvases();
            Assert.IsNotNull(Board.canvas); Assert.IsNotNull(Board.canvas.GetComponent<UnityEngine.UI.GraphicRaycaster>());
            Assert.Greater(Board.rectTransform.rect.width, 0); Assert.Greater(Board.rectTransform.rect.height, 0);
            TestContext.Out.WriteLine("uGUI board layout: " + Board.rectTransform.rect); Controller.Refresh();
        }
        internal Vector2 Point(FlowPos cell) => UguiPointerDriver.Point(Board, cell);
        internal Vector2 Local(float x, float y) => RectTransformUtility.WorldToScreenPoint(null, Board.rectTransform.TransformPoint(new Vector2(x, y)));
        internal IReadOnlyList<FlowPos> Route(int pair = 0) => Battle.Runtime.Domain.Routes[pair];
        private static int Pointer(int id, string type) => type == PointerType.touch ? id : -1;
        internal bool HasCapture(int id) => Controller?.Gesture.ActivePointerId == id;
        internal void Down(Vector2 p, int id = -1, string type = null) => Canvas.Driver.Down(p, Pointer(id, type));
        internal void Move(Vector2 p, int id = -1, string type = null) => Canvas.Driver.Move(p, Pointer(id, type));
        internal void Up(Vector2 p, int id = -1, string type = null) => Canvas.Driver.Up(p, Pointer(id, type));
        internal void Cancel(int id = -1, string type = null) => Canvas.Driver.Cancel(Pointer(id, type), Point(Route()[0]));
        internal void Draw(int pair = 0, int id = -1, string type = null)
        {
            var route = Route(pair); Down(Point(route[0]), id, type);
            for (var i = 1; i < route.Count - 1; i++) Move(Point(route[i]), id, type);
            Up(Point(route[route.Count - 1]), id, type);
        }
        internal void Tap(FlowPos cell) { var p = Point(cell); Down(p); Up(p); }
        internal static void CloseAll() { foreach (var rig in open.ToArray()) rig.Dispose(); }
        public void Dispose()
        {
            if (!open.Remove(this)) return;
            try { UI?.Unbind(); Canvas?.Dispose(); } finally { Battle.Dispose(); }
        }
    }

    internal static class BattleCopyAssert
    {
        internal static LocalizedTmpText Localized(Component root, string row, LocalizationService service, string key,
            params KeyValuePair<string, string>[] arguments)
        {
            var binding = root.Find<LocalizedTmpText>(FightMatchViewId.Row(row)); Assert.IsNotNull(binding, row);
            Assert.AreEqual(key, binding.Key, row); Assert.IsNull(binding.DiagnosticCode, row);
            var expected = service.Resolve(key, arguments); Assert.IsTrue(expected.IsSuccess, expected.DiagnosticCode);
            Assert.AreEqual(expected.Text, binding.Target.text, row); return binding;
        }
        internal static void Diagnostic(Component root, string row)
        {
            var binding = root.Find<LocalizedTmpText>(FightMatchViewId.Row(row)); Assert.IsNotNull(binding, row);
            Assert.IsNotNull(binding.DiagnosticCode, row); Assert.AreEqual(LocalizedTmpText.Placeholder, binding.Target.text, row);
        }
        internal static string VisibleText(Component root) => string.Join("\n", root.GetComponentsInChildren<LocalizedTmpText>(true)
            .Where(x => x.gameObject.activeInHierarchy).Select(x => x.Target.text));
        internal static void Feedback(Component root, CandidateBoardInputController controller, LocalizationService service, string key)
        {
            var lines = root.GetComponentsInChildren<LocalizedTmpText>(true).Where(x => x.gameObject.activeInHierarchy &&
                (x.Key == "fm.reference.closed_for_input" || x.Key == "fm.battle.action.accepted" ||
                x.Key == "fm.battle.gesture.invalid_start" || x.Key == "fm.battle.gesture.not_adjacent" ||
                x.Key == "fm.battle.gesture.crossed" || x.Key == "fm.battle.gesture.wrong_endpoint" ||
                x.Key == "fm.battle.gesture.cancelled" || x.Key == "fm.battle.gesture.focus_lost" ||
                x.Key == "fm.battle.gesture.second_pointer_ignored")).ToArray();
            Assert.AreEqual(key == null ? 0 : 1, lines.Length, "Gesture/action/reference share exactly one retained row.");
            if (key == null) return;
            Assert.AreEqual(key, lines[0].Key); Assert.IsNull(lines[0].DiagnosticCode);
            Assert.AreEqual(service.Resolve(key, Array.Empty<KeyValuePair<string, string>>()).Text, lines[0].Target.text);
            Assert.AreEqual(FightMatchViewId.Row("board-availability:board-visible-feedback:" +
                controller.VisibleFeedbackRevision.ToString(System.Globalization.CultureInfo.InvariantCulture)), lines[0].GetComponent<FightMatchViewId>().Id);
        }
    }

    internal sealed class BattleFeedbackObserver : IDisposable
    {
        private readonly CandidateBoardInputController input;
        private long revision;
        internal readonly List<CandidateBoardVisibleFeedbackKind> Published = new List<CandidateBoardVisibleFeedbackKind>();
        internal BattleFeedbackObserver(CandidateBoardInputController input)
        { this.input = input; revision = input.VisibleFeedbackRevision; input.Changed += Observe; }
        private void Observe()
        {
            if (revision == input.VisibleFeedbackRevision) return;
            revision = input.VisibleFeedbackRevision;
            if (input.VisibleFeedback.HasValue) Published.Add(input.VisibleFeedback.Value);
        }
        public void Dispose() { input.Changed -= Observe; }
    }

    internal static class BattleCancellationCases
    {
        // Called by both original focus cases. These actual MonoBehaviour disables deliberately order the first active owner.
        internal static IEnumerator AssertDisableAndExplicitCancellationOrdersBody()
        {
            foreach (var mode in new[] { "parent-first", "child-first", "host-view", "battle-view", "input-close",
                "playback-close", "input-unbind", "battle-unbind", "low-memory" })
            using (var r = new UguiHostRig(true))
            {
                r.Enter(); yield return r.Ready();
                r.Host.enabled = true; // No yield after enabling: Start cannot run or open a real player store.
                var input = r.Session.Battle.Input; var board = r.Board; var page = r.View.BattleView;
                var head = r.Head; var calls = r.Storage.SnapshotCreates; var files = PlayerSessionTestData.CopyFiles(r.Storage.Files);
                r.BeginRoute(); var cells = input.Gesture.DraftCells.ToArray(); var pointer = input.Gesture.ActivePointerId;
                var revision = input.VisibleFeedbackRevision;
                using (var observed = new BattleFeedbackObserver(input))
                {
                    r.Host.HandleApplicationFocus(true);
                    Assert.AreEqual(pointer, input.Gesture.ActivePointerId, mode); CollectionAssert.AreEqual(cells, input.Gesture.DraftCells, mode);
                    Assert.AreEqual(revision, input.VisibleFeedbackRevision, mode); Assert.IsEmpty(observed.Published, mode);
                    if (mode == "parent-first") { r.Host.enabled = false; board.gameObject.SetActive(false); }
                    else if (mode == "child-first") { board.gameObject.SetActive(false); r.Host.enabled = false; }
                    else if (mode == "host-view") { r.View.enabled = false; r.Host.enabled = false; }
                    else if (mode == "battle-view") { page.enabled = false; r.Host.enabled = false; }
                    else if (mode == "input-close") page.PlaybackView.InputView.Close();
                    else if (mode == "playback-close") page.PlaybackView.Close();
                    else if (mode == "input-unbind") page.PlaybackView.InputView.Unbind();
                    else if (mode == "battle-unbind") page.Unbind();
                    else page.PlaybackView.NotifyLowMemory();
                    var expected = mode == "parent-first" || mode == "child-first" || mode == "host-view" || mode == "battle-view" ?
                        CandidateBoardVisibleFeedbackKind.FocusLost : CandidateBoardVisibleFeedbackKind.Cancelled;
                    CollectionAssert.AreEqual(new[] { expected }, observed.Published, mode);
                    Assert.IsNull(input.Gesture.ActivePointerId, mode); Assert.IsFalse(board.HasActivePointer, mode);
                    Assert.IsNull(input.LastRequest, mode); Assert.AreSame(head, r.Head, mode); Assert.AreEqual(calls, r.Storage.SnapshotCreates, mode);
                    PlayerSessionTestData.SameFiles(files, r.Storage.Files);
                    if (mode == "low-memory") Assert.AreEqual(expected, input.VisibleFeedback, mode);
                    else Assert.IsNull(input.VisibleFeedback, mode + " teardown clears only after the sole publish");
                    page.Unbind(); r.Host.enabled = false; r.Root.SetActive(false);
                    CollectionAssert.AreEqual(new[] { expected }, observed.Published, mode + " repeated layers do not republish");
                    Assert.IsNull(input.VisibleFeedback, mode);
                }
            }
            foreach (var mode in new[] { "close", "disable", "unbind", "low-memory" })
            using (var r = new PlaybackPanelRig())
            {
                yield return r.Ready();
                var input = r.Input; input.SelectMember("W"); var head = r.Battle.Head; var calls = r.Battle.Runtime.Storage.Base.Calls;
                var route = r.Route(); var board = r.Board;
                r.Canvas.Driver.Down(UguiPointerDriver.Point(board, route[0]), 1);
                r.Canvas.Driver.Move(UguiPointerDriver.Point(board, route[1]), 1);
                Assert.IsTrue(board.HasActivePointer);
                using (var observed = new BattleFeedbackObserver(input))
                {
                    if (mode == "close") r.Host.Close();
                    else if (mode == "disable") r.Host.enabled = false;
                    else if (mode == "unbind") r.Host.Unbind();
                    else r.Host.NotifyLowMemory();
                    CollectionAssert.AreEqual(new[] { mode == "disable" ? CandidateBoardVisibleFeedbackKind.FocusLost :
                        CandidateBoardVisibleFeedbackKind.Cancelled }, observed.Published, "non-borrowed " + mode);
                    Assert.IsFalse(board.HasActivePointer); Assert.IsNull(input.LastRequest); Assert.AreSame(head, r.Battle.Head);
                    Assert.AreEqual(calls, r.Battle.Runtime.Storage.Base.Calls);
                    if (mode != "low-memory") Assert.IsNull(input.VisibleFeedback);
                }
            }
        }
    }
}
