using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FightMatch.Application;
using FightMatch.Host;
using FightMatch.Presentation;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using static FightMatch.Core.Tests.PlayerSessionTestData;

namespace FightMatch.Core.Tests
{
    public sealed class UguiResponsiveLayoutTests
    {
        // Frozen from the approved hierarchy, not from the migrated observations. Paths are relative to SafeAreaRoot.
        private const string ExpectedTextTargets = @"HudLayer/TopBar/Back/Caption|Binding
HudLayer/TopBar/Language/Caption|Binding
HudLayer/TopBar/License/Caption|Binding
HudLayer/TopBar/Title|Binding
PopupLayer/ConfirmationPopup/RootMask/BattleConfirmationPanel/Header/Title|Binding
PopupLayer/ConfirmationPopup/RootMask/NavigationConfirmationPanel/Actions/DangerActions/End/Caption|Binding
PopupLayer/ConfirmationPopup/RootMask/NavigationConfirmationPanel/Actions/DangerActions/End/DisabledReason|Binding
PopupLayer/ConfirmationPopup/RootMask/NavigationConfirmationPanel/Actions/SafeActions/Confirm/Caption|Binding
PopupLayer/ConfirmationPopup/RootMask/NavigationConfirmationPanel/Actions/SafeActions/Confirm/DisabledReason|Binding
PopupLayer/ConfirmationPopup/RootMask/NavigationConfirmationPanel/BodyViewport/Viewport/Content/StateRows/Status|Binding
PopupLayer/ConfirmationPopup/RootMask/NavigationConfirmationPanel/Header/Title|Binding
PopupLayer/LanguagePopup/RootMask/DialogPanel/Actions/Chinese/Caption|Binding
PopupLayer/LanguagePopup/RootMask/DialogPanel/Actions/Close/Caption|Binding
PopupLayer/LanguagePopup/RootMask/DialogPanel/Actions/English/Caption|Binding
PopupLayer/LanguagePopup/RootMask/DialogPanel/BodyViewport/Viewport/Content/Feedback|Binding
PopupLayer/LanguagePopup/RootMask/DialogPanel/Header/Title|Binding
PopupLayer/LicensePopup/RootMask/DialogPanel/Actions/Back/Caption|Binding
PopupLayer/LicensePopup/RootMask/DialogPanel/BodyViewport/Viewport/Content/LicenseBody|License
PopupLayer/LicensePopup/RootMask/DialogPanel/Header/Title|Binding
PopupLayer/QuitPopup/RootMask/DialogPanel/Actions/Confirm/Caption|Binding
PopupLayer/QuitPopup/RootMask/DialogPanel/Actions/Stay/Caption|Binding
PopupLayer/QuitPopup/RootMask/DialogPanel/BodyViewport/Viewport/Content/Body|Binding
PopupLayer/QuitPopup/RootMask/DialogPanel/Header/Title|Binding
ScreenLayer/BattlePage/BattleContent/BattleStatus/Beat|Binding
ScreenLayer/BattlePage/BattleContent/BattleStatus/Phase|Binding
ScreenLayer/BattlePage/BattleContent/BattleStatus/PlaybackStage|Binding
ScreenLayer/BattlePage/BattleContent/BottomHud/HistoryDrawer/Header/Close/Caption|Binding
ScreenLayer/BattlePage/BattleContent/BottomHud/HistoryDrawer/Header/Title|Binding
ScreenLayer/BattlePage/BattleContent/BottomHud/NormalHud/MainRow/CommandStrip/Viewport/Content/FixedBattleActions/HistoryOpen/Caption|Binding
ScreenLayer/BattlePage/BattleContent/BottomHud/NormalHud/MainRow/CommandStrip/Viewport/Content/FixedBattleActions/Resolve/Caption|Binding
ScreenLayer/BattlePage/BattleContent/BottomHud/NormalHud/MainRow/CommandStrip/Viewport/Content/FixedBattleActions/Retry/Caption|Binding
ScreenLayer/BattlePage/BattleContent/BottomHud/NormalHud/MainRow/CommandStrip/Viewport/Content/FixedBattleActions/Skip/Caption|Binding
ScreenLayer/BattlePage/BattleContent/BottomHud/NormalHud/StatusViewport/Viewport/Content/AvailabilityFirstLine|Binding
ScreenLayer/BattlePage/BattleContent/BottomHud/NormalHud/StatusViewport/Viewport/Content/BattleHudFirstLine|Binding
ScreenLayer/BattlePage/BattleContent/BottomHud/NormalHud/StatusViewport/Viewport/Content/PlaybackDiagnostic|Binding
ScreenLayer/BattlePage/BattleContent/BottomHud/NormalHud/StatusViewport/Viewport/Content/Save|Binding
ScreenLayer/BattlePage/BattleContent/BottomHud/ReferenceInfoDrawer/BodyViewport/Viewport/Content/Beat|Binding
ScreenLayer/BattlePage/BattleContent/BottomHud/ReferenceInfoDrawer/BodyViewport/Viewport/Content/Explanation|Binding
ScreenLayer/BattlePage/BattleContent/BottomHud/ReferenceInfoDrawer/Header/Close/Caption|Binding
ScreenLayer/BattlePage/BattleContent/BottomHud/ReferenceInfoDrawer/Header/Title|Binding
ScreenLayer/BattlePage/BattleContent/Stage/AllySlots/ActorSlot0/Hp|Binding
ScreenLayer/BattlePage/BattleContent/Stage/AllySlots/ActorSlot0/Intent|Binding
ScreenLayer/BattlePage/BattleContent/Stage/AllySlots/ActorSlot0/Name|Binding
ScreenLayer/BattlePage/BattleContent/Stage/AllySlots/ActorSlot1/Hp|Binding
ScreenLayer/BattlePage/BattleContent/Stage/AllySlots/ActorSlot1/Intent|Binding
ScreenLayer/BattlePage/BattleContent/Stage/AllySlots/ActorSlot1/Name|Binding
ScreenLayer/BattlePage/BattleContent/Stage/AllySlots/ActorSlot2/Hp|Binding
ScreenLayer/BattlePage/BattleContent/Stage/AllySlots/ActorSlot2/Intent|Binding
ScreenLayer/BattlePage/BattleContent/Stage/AllySlots/ActorSlot2/Name|Binding
ScreenLayer/BattlePage/BattleContent/Stage/EnemySlots/ActorSlot0/Hp|Binding
ScreenLayer/BattlePage/BattleContent/Stage/EnemySlots/ActorSlot0/Intent|Binding
ScreenLayer/BattlePage/BattleContent/Stage/EnemySlots/ActorSlot0/Name|Binding
ScreenLayer/BattlePage/BattleContent/Stage/EnemySlots/ActorSlot1/Hp|Binding
ScreenLayer/BattlePage/BattleContent/Stage/EnemySlots/ActorSlot1/Intent|Binding
ScreenLayer/BattlePage/BattleContent/Stage/EnemySlots/ActorSlot1/Name|Binding
ScreenLayer/BattlePage/BattleContent/Stage/EnemySlots/ActorSlot2/Hp|Binding
ScreenLayer/BattlePage/BattleContent/Stage/EnemySlots/ActorSlot2/Intent|Binding
ScreenLayer/BattlePage/BattleContent/Stage/EnemySlots/ActorSlot2/Name|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/Back/Caption|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/Back/DisabledReason|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/Cancel/Caption|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/Cancel/DisabledReason|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/CraftSection/RecipeAvailability|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/Creation/Caption|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/Creation/DisabledReason|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/Inventory/Caption|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/Inventory/DisabledReason|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/InventorySection/ClearEquipment/Caption|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/InventorySection/ClearEquipment/DisabledReason|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/InventorySection/Craft/Caption|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/InventorySection/Craft/DisabledReason|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/InventorySection/OriginalResult/Caption|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/InventorySection/OriginalResult/DisabledReason|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/InventorySection/Preference/Caption|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/InventorySection/Preference/DisabledReason|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/Map/Caption|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/Map/DisabledReason|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/Migration/Caption|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/Migration/DisabledReason|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/Party/Caption|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/Party/DisabledReason|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/PartySection/FrontSlot/Caption|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/PartySection/FrontSlot/Template/Viewport/Content/Item/ItemLabel|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/PartySection/MiddleSlot/Caption|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/PartySection/MiddleSlot/Template/Viewport/Content/Item/ItemLabel|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/PartySection/PartyPreview/Caption|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/PartySection/PartyPreview/DisabledReason|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/PartySection/RearSlot/Caption|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/PartySection/RearSlot/Template/Viewport/Content/Item/ItemLabel|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/PermanentDetail/Character|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/PermanentDetail/InputError|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/PermanentDetail/PreferenceLabel|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/PermanentDetail/Preview/Caption|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/PermanentDetail/Preview/DisabledReason|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/PermanentDetail/Quantity/TextArea/Value|Input
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/PermanentDetail/QuantityLabel|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/PermanentDetail/SourcesLabel|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/PermanentDetail/Title|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/PreparationSection/Entry/Caption|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/PreparationSection/Entry/DisabledReason|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/PreparationSection/Resume/Caption|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/PreparationSection/Resume/DisabledReason|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/Refresh/Caption|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/Refresh/DisabledReason|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/Settlement/Caption|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/Settlement/DisabledReason|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/Status|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/Title|Binding
ScreenLayer/StartupPage/StartupScroll/Viewport/Content/Continue/Caption|Binding
ScreenLayer/StartupPage/StartupScroll/Viewport/Content/Create/Caption|Binding
ScreenLayer/StartupPage/StartupScroll/Viewport/Content/ProfileTitle|Binding
ScreenLayer/StartupPage/StartupScroll/Viewport/Content/Reload/Caption|Binding
ScreenLayer/StartupPage/StartupScroll/Viewport/Content/StartupStatus|Binding
SystemLayer/BlockingDiagnostic/Diagnostic|Binding
SystemLayer/LayoutDiagnostic/Message|Binding
SystemLayer/LoadingOverlay/Loading|Binding
SystemLayer/RecoveryScreen/RootMask/BattleRecoveryPanel/Header/Title|Binding
SystemLayer/RecoveryScreen/RootMask/NavigationRecoveryPanel/Actions/DangerActions/EndReview/Caption|Binding
SystemLayer/RecoveryScreen/RootMask/NavigationRecoveryPanel/Actions/DangerActions/EndReview/DisabledReason|Binding
SystemLayer/RecoveryScreen/RootMask/NavigationRecoveryPanel/Actions/SafeActions/Refresh/Caption|Binding
SystemLayer/RecoveryScreen/RootMask/NavigationRecoveryPanel/Actions/SafeActions/Refresh/DisabledReason|Binding
SystemLayer/RecoveryScreen/RootMask/NavigationRecoveryPanel/Actions/SafeActions/Resolve/Caption|Binding
SystemLayer/RecoveryScreen/RootMask/NavigationRecoveryPanel/Actions/SafeActions/Resolve/DisabledReason|Binding
SystemLayer/RecoveryScreen/RootMask/NavigationRecoveryPanel/Actions/SafeActions/ResumeObserved/Caption|Binding
SystemLayer/RecoveryScreen/RootMask/NavigationRecoveryPanel/Actions/SafeActions/ResumeObserved/DisabledReason|Binding
SystemLayer/RecoveryScreen/RootMask/NavigationRecoveryPanel/Actions/SafeActions/Retry/Caption|Binding
SystemLayer/RecoveryScreen/RootMask/NavigationRecoveryPanel/Actions/SafeActions/Retry/DisabledReason|Binding
SystemLayer/RecoveryScreen/RootMask/NavigationRecoveryPanel/Actions/SafeActions/Return/Caption|Binding
SystemLayer/RecoveryScreen/RootMask/NavigationRecoveryPanel/Actions/SafeActions/Return/DisabledReason|Binding
SystemLayer/RecoveryScreen/RootMask/NavigationRecoveryPanel/BodyViewport/Viewport/Content/StateRows/Status|Binding
SystemLayer/RecoveryScreen/RootMask/NavigationRecoveryPanel/Header/Title|Binding
TemplatePool/BattleButtonTemplate/Caption|Binding
TemplatePool/MemberButtonTemplate/Caption|Binding
TemplatePool/NavigationButtonTemplate/Caption|Binding
TemplatePool/NavigationButtonTemplate/DisabledReason|Binding
TemplatePool/SourceTemplate/Count/TextArea/Value|Input
TemplatePool/SourceTemplate/CountLabel|Binding
TemplatePool/SourceTemplate/Description|Binding
TemplatePool/SourceTemplate/Start/TextArea/Value|Input
TemplatePool/SourceTemplate/StartLabel|Binding
TemplatePool/TextTemplate|Binding";

        internal static void AssertExactTextTargets(GameObject root)
        {
            var safe = root.transform.Find("RuntimeCanvas/SafeAreaRoot"); Assert.IsNotNull(safe);
            var actual = root.GetComponentsInChildren<TextMeshProUGUI>(true).Select(text => {
                var input = text.GetComponentInParent<TMP_InputField>(true);
                var kind = input != null && input.textComponent == text ? "Input" : text.GetComponent<LocalizedTmpText>() != null ? "Binding" : "License";
                return Relative(safe, text.transform) + "|" + kind;
            }).OrderBy(x => x, StringComparer.Ordinal).ToArray();
            CollectionAssert.AreEqual(ExpectedTextTargets.Split('\n').OrderBy(x => x, StringComparer.Ordinal).ToArray(), actual);
            Assert.AreEqual(141, actual.Length); Assert.AreEqual(3, actual.Count(x => x.EndsWith("|Input", StringComparison.Ordinal)));
            Assert.AreEqual(137, actual.Count(x => x.EndsWith("|Binding", StringComparison.Ordinal)));
            Assert.AreEqual(1, actual.Count(x => x.EndsWith("|License", StringComparison.Ordinal)));
        }
        private const string BattlePath = "ScreenLayer/BattlePage/BattleContent";
        private const string NormalPath = BattlePath + "/BottomHud/NormalHud";
        private const string CommandPath = NormalPath + "/MainRow/CommandStrip";
        private const string ConfirmationPath = "PopupLayer/ConfirmationPopup";
        private const string RecoveryPath = "SystemLayer/RecoveryScreen";
        private bool enteredPlayMode;

        [UnityTearDown]
        public IEnumerator ExitControlledPlayModeAfterFailure()
        {
            if (enteredPlayMode && UnityEngine.Application.isPlaying) yield return new ExitPlayMode();
            enteredPlayMode = false;
        }
        // GameView fixed sizes drive the actual Overlay display. CanvasScaler is never disabled in this fixture.
        private sealed class OverlayResolution : IDisposable
        {
            private readonly EditorWindow window;
            private readonly object group;
            private readonly PropertyInfo selection;
            private readonly int previous, customIndex;
            internal OverlayResolution(int width, int height)
            {
                var assembly = typeof(Editor).Assembly;
                var gameView = assembly.GetType("UnityEditor.GameView", true);
                var sizesType = assembly.GetType("UnityEditor.GameViewSizes", true);
                var singleton = typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
                var sizes = singleton.GetProperty("instance", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy).GetValue(null);
                var groupType = assembly.GetType("UnityEditor.GameViewSizeGroupType", true);
                group = sizesType.GetMethod("GetGroup").Invoke(sizes, new[] { Enum.Parse(groupType, "Standalone") });
                window = EditorWindow.GetWindow(gameView); window.Show();
                selection = gameView.GetProperty("selectedSizeIndex", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                Assert.IsNotNull(selection); previous = (int)selection.GetValue(window);
                var sizeType = assembly.GetType("UnityEditor.GameViewSize", true);
                var kind = assembly.GetType("UnityEditor.GameViewSizeType", true);
                var size = Activator.CreateInstance(sizeType, Enum.Parse(kind, "FixedResolution"), width, height, "LAYOUT controlled " + width + "x" + height);
                customIndex = (int)group.GetType().GetMethod("GetCustomCount").Invoke(group, null);
                group.GetType().GetMethod("AddCustomSize").Invoke(group, new[] { size });
                var builtins = (int)group.GetType().GetMethod("GetBuiltinCount").Invoke(group, null);
                selection.SetValue(window, builtins + customIndex); window.Repaint();
            }
            internal IEnumerator Ready(int width, int height)
            {
                for (var frame = 0; frame < 20 && (Screen.width != width || Screen.height != height); frame++) yield return null;
                Assert.AreEqual(width, Screen.width, "Actual GameView width"); Assert.AreEqual(height, Screen.height, "Actual GameView height");
            }
            public void Dispose()
            {
                if (window == null) return;
                selection.SetValue(window, previous);
                group.GetType().GetMethod("RemoveCustomSize").Invoke(group, new object[] { customIndex }); window.Repaint();
            }
        }
        private static void EmptyScene() => EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        private static RectTransform R(UguiHostRig rig, string path)
        {
            var result = rig.Root.transform.Find("RuntimeCanvas/SafeAreaRoot/" + path) as RectTransform;
            Assert.IsNotNull(result, path); return result;
        }
        private static T Reference<T>(UnityEngine.Object owner, string field) where T : UnityEngine.Object
        {
            var property = new SerializedObject(owner).FindProperty(field); Assert.IsNotNull(property, field);
            var result = property.objectReferenceValue as T; Assert.IsNotNull(result, field); return result;
        }
        private static T[] References<T>(UnityEngine.Object owner, string field) where T : UnityEngine.Object
        {
            var array = new SerializedObject(owner).FindProperty(field); Assert.IsNotNull(array);
            return Enumerable.Range(0, array.arraySize).Select(i => (T)array.GetArrayElementAtIndex(i).objectReferenceValue).ToArray();
        }
        private static SafeAreaFitter Safe(UguiHostRig rig) => rig.Root.GetComponentInChildren<SafeAreaFitter>(true);
        private static void ApplySafe(UguiHostRig rig, Rect physical, int width, int height)
        {
            Assert.AreEqual(width, Screen.width); Assert.AreEqual(height, Screen.height);
            var fitter = Safe(rig); fitter.enabled = false; fitter.Apply(physical, new Vector2(width, height)); Canvas.ForceUpdateCanvases();
        }
        private static UguiHostRig OverlayRig(Rect physical, int width, int height)
        {
            var rig = new UguiHostRig(true, bind: false);
            ApplySafe(rig, physical, width, height); rig.Bind();
            var firstFrames = 0;
            Canvas.WillRenderCanvases firstFrame = () => {
                if (!rig.Board.gameObject.activeInHierarchy) return;
                firstFrames++; Assert.Greater(rig.Board.rectTransform.rect.width, 0, "First rendered board width");
                Assert.Greater(rig.Board.rectTransform.rect.height, 0, "First rendered board height");
            };
            Canvas.willRenderCanvases += firstFrame;
            try { rig.Enter(); Canvas.ForceUpdateCanvases(); }
            finally { Canvas.willRenderCanvases -= firstFrame; }
            Assert.Greater(firstFrames, 0, "Observed first actual Overlay render");
            var canvas = rig.Root.GetComponentInChildren<Canvas>(true); var scaler = canvas.GetComponent<CanvasScaler>();
            Assert.AreEqual(RenderMode.ScreenSpaceOverlay, canvas.renderMode); Assert.IsTrue(scaler.enabled);
            Assert.AreEqual(CanvasScaler.ScaleMode.ScaleWithScreenSize, scaler.uiScaleMode);
            Assert.AreEqual(new Vector2(540, 960), scaler.referenceResolution); Assert.AreEqual(.5f, scaler.matchWidthOrHeight);
            return rig;
        }
        private static Rect CanvasRect(UguiHostRig rig, RectTransform rect)
        {
            var canvas = rig.Root.GetComponentInChildren<Canvas>(true); var corners = new Vector3[4]; rect.GetWorldCorners(corners);
            var points = corners.Select(x => (Vector2)canvas.transform.InverseTransformPoint(x)).ToArray();
            return Rect.MinMaxRect(points.Min(x => x.x), points.Min(x => x.y), points.Max(x => x.x), points.Max(x => x.y));
        }
        private static Vector2 PixelCenter(RectTransform rect) => RectTransformUtility.WorldToScreenPoint(null, rect.TransformPoint(rect.rect.center));
        private static void Near(float expected, float actual, string name) => Assert.AreEqual(expected, actual, .08f, name);
        private static void Geometry(UguiHostRig rig, float width, float height, float stageHeight, float hudHeight)
        {
            var safe = (RectTransform)Safe(rig).transform; var board = Mathf.Min(width, 508); var k = board / 508;
            Near(width, safe.rect.width, "Safe width"); Near(height, safe.rect.height, "Safe height");
            var regions = new[] { R(rig, "HudLayer/TopBar"), R(rig, BattlePath + "/Stage"), R(rig, BattlePath + "/BattleStatus"),
                R(rig, BattlePath + "/BoardRegion"), R(rig, BattlePath + "/BottomHud") };
            var heights = new[] { 56 * k, stageHeight, 48 * k, board, hudHeight };
            for (var i = 0; i < regions.Length; i++)
            {
                var rect = CanvasRect(rig, regions[i]); Near(board, rect.width, regions[i].name + " width"); Near(heights[i], rect.height, regions[i].name + " height");
                if (i != 0) Near(CanvasRect(rig, regions[i - 1]).yMin, rect.yMax, "Contiguous regions");
                TestContext.Out.WriteLine(regions[i].name + " local=" + regions[i].rect + " Canvas=" + rect + " scale=" + regions[i].localScale);
            }
            var actualBoard = CanvasRect(rig, rig.Board.rectTransform); Assert.AreEqual(CanvasRect(rig, regions[3]), actualBoard);
            Assert.AreEqual(Vector3.one, regions[3].localScale); Assert.AreEqual(Vector3.one, rig.Board.rectTransform.localScale);
            Assert.AreEqual(1, rig.Board.GetComponents<Graphic>().Length);
            Near(hudHeight - 80 * k, CanvasRect(rig, R(rig, NormalPath + "/StatusViewport")).height, "Status height");
            Near(72 * k, CanvasRect(rig, R(rig, NormalPath + "/MainRow")).height, "Main row height");
            foreach (var page in new[] { "StartupPage/StartupScroll", "NavigationPage/NavigationScroll", "ResultPage/ResultScroll" })
                Near(CanvasRect(rig, regions[0]).yMin, CanvasRect(rig, R(rig, "ScreenLayer/" + page)).yMax, "One TopBar deduction");
            var canvas = rig.Root.GetComponentInChildren<Canvas>(true);
            TestContext.Out.WriteLine("physical=" + Screen.width + "x" + Screen.height + " Safe version=" + Safe(rig).Version +
                " rect=" + safe.rect + " factor=" + canvas.scaleFactor + " lossy=" + canvas.transform.lossyScale + " k=" + k);
            Assert.IsFalse(R(rig, "SystemLayer/LayoutDiagnostic").gameObject.activeSelf);
            Assert.IsFalse(rig.View.DiagnosticVisible, rig.View.DiagnosticCode);
        }
        private static void VisibleCopy(UguiHostRig rig)
        {
            foreach (var text in rig.Root.GetComponentsInChildren<TextMeshProUGUI>(false))
            {
                Assert.AreNotEqual(LocalizedTmpText.Placeholder, text.text, Relative(rig.Root.transform, text.transform));
                foreach (var character in text.text.Where(x => !char.IsWhiteSpace(x)))
                    Assert.IsTrue(text.font.HasCharacter(character, true, true), "Missing glyph " + character);
            }
            Assert.IsTrue(FightMatchViewId.Validate(rig.Root.transform, out var error), error);
        }
        private static void Commands(UguiHostRig rig)
        {
            var scroll = R(rig, CommandPath).GetComponent<ScrollRect>();
            var ids = new[] { "fm.action.battle.retry", "fm.action.battle.resolve", "fm.action.battle.skip", "fm.action.history.open",
                FightMatchViewId.Row("battle-exit"), FightMatchViewId.Row("battle-restart"), "fm.action.victory.settle", "fm.action.reference.open" };
            var keys = new[] { "fm.save_recovery.retry_button", "fm.save_recovery.confirm_result_button", "fm.battle.playback.skip_button", "fm.history.title",
                "fm.operation.exit_attempt", "fm.operation.restart_attempt", "fm.victory.settle.button", "fm.reference.play_button" };
            var widths = new[] { 184, 224, 200, 144, 152, 256, 248, 152 };
            foreach (var locale in new[] { LocaleId.En, LocaleId.ZhHans })
            {
                rig.Localization.SetLocale(locale); Canvas.ForceUpdateCanvases();
                Near(252, scroll.viewport.rect.width, "Command viewport"); Near(1624, scroll.content.rect.width, "Command content");
                Near(1372, scroll.content.rect.width - scroll.viewport.rect.width, "Scroll range");
                for (var i = 0; i < ids.Length; i++)
                {
                    var button = rig.Find<Button>(ids[i]); var rect = (RectTransform)button.transform;
                    var caption = button.GetComponentInChildren<LocalizedTmpText>(true); var text = caption.Target;
                    Near(widths[i], rect.rect.width, ids[i]); Near(48, rect.rect.height, "Hot zone");
                    Assert.AreEqual(keys[i], caption.Key); Assert.AreEqual(rig.Localization.Resolve(keys[i], null).Text, text.text);
                    Assert.AreEqual(18, text.fontSize); Assert.IsFalse(text.enableAutoSizing); Assert.IsFalse(text.enableWordWrapping);
                    Near(widths[i] - 24, text.rectTransform.rect.width, "Caption horizontal padding");
                    Near(32, text.rectTransform.rect.height, "Caption vertical padding");
                    text.ForceMeshUpdate(true); Assert.LessOrEqual(text.preferredWidth, text.rectTransform.rect.width + .1f, text.text);
                    Assert.LessOrEqual(text.preferredHeight, text.rectTransform.rect.height + .1f, text.text);
                }
                scroll.horizontalNormalizedPosition = 0; Canvas.ForceUpdateCanvases();
                var first = CanvasRect(rig, (RectTransform)rig.Find<Button>(ids[0]).transform); var viewport = CanvasRect(rig, scroll.viewport);
                Assert.GreaterOrEqual(first.xMin, viewport.xMin - .1f); Assert.LessOrEqual(first.xMax, viewport.xMax + .1f);
                scroll.horizontalNormalizedPosition = 1; Canvas.ForceUpdateCanvases();
                var last = CanvasRect(rig, (RectTransform)rig.Find<Button>(ids.Last()).transform);
                Assert.GreaterOrEqual(last.xMin, viewport.xMin - .1f); Assert.LessOrEqual(last.xMax, viewport.xMax + .1f);
            }
            scroll.horizontalNormalizedPosition = 0; Canvas.ForceUpdateCanvases(); VisibleCopy(rig);
        }
        private static void VictoryCaption(UguiHostRig rig)
        {
            for (var step = 0; rig.State.Phase != BattlePhase.WonPendingSettlement; step++)
            {
                Assert.Less(step, 64, "Bounded real gameplay to the second Skip binding");
                var state = rig.State;
                Assert.That(state.Phase, NUnit.Framework.Is.EqualTo(BattlePhase.AwaitAction).Or.EqualTo(BattlePhase.AwaitLinks));
                if (state.Phase == BattlePhase.AwaitAction)
                    Assert.IsTrue(rig.Session.Battle.Input.SelectMember(state.Members.First(x => x.Hp.Numerator.Sign > 0).Member.CharacterId));
                var pair = state.Phase == BattlePhase.AwaitLinks ? state.Board.PendingLinks[0] : state.Enemies.First(x => x.Hp.Numerator.Sign > 0).PairKey;
                var source = RealSource(); var level = source.Levels.Single(x => x.Level.LevelId == state.Baseline.Entry.Level.LevelId);
                var face = state.Board.Face;
                var route = level.SourceRoutes.Single(x => x.FaceId == pair.FaceId && x.PairId == pair.PairId).Cells.Select(p =>
                    new FlowPuzzle.Core.FlowPos(p.x - 1, source.Coordinates == FightMatch.Content.DemoCoordinateCandidate.AssumedBottomLeft ? p.y - 1 : face.Height - p.y)).ToArray();
                rig.Driver.Down(UguiPointerDriver.Point(rig.Board, route[0]));
                foreach (var cell in route.Skip(1).Take(route.Length - 2)) rig.Driver.Move(UguiPointerDriver.Point(rig.Board, cell));
                rig.Driver.Up(UguiPointerDriver.Point(rig.Board, route.Last()));
                Assert.IsTrue(rig.Session.Battle.Input.LastResult.Application.IsCommitted, rig.Session.Battle.Input.Status);
                if (rig.State.Phase != BattlePhase.WonPendingSettlement) rig.View.BattleView.PlaybackView.SkipToFinal();
            }
            Assert.IsNotNull(rig.Session.Battle.Input.View.PresentationToken);
            foreach (var locale in new[] { LocaleId.En, LocaleId.ZhHans })
            {
                rig.Localization.SetLocale(locale); Canvas.ForceUpdateCanvases();
                var caption = rig.Find<Button>("fm.action.battle.skip").GetComponentInChildren<LocalizedTmpText>(true);
                Assert.AreEqual("fm.victory.playback.skip_button", caption.Key);
                Assert.AreEqual(rig.Localization.Resolve(caption.Key, null).Text, caption.Target.text);
                caption.Target.ForceMeshUpdate(true);
                Assert.LessOrEqual(caption.Target.preferredWidth, caption.Target.rectTransform.rect.width + .1f);
                Assert.LessOrEqual(caption.Target.preferredHeight, caption.Target.rectTransform.rect.height + .1f);
            }
            rig.View.BattleView.PlaybackView.SkipToFinal(); VisibleCopy(rig);
        }
        private static Array LayoutLeaseFiles(Type rowType, params string[] paths)
        {
            var rows = Array.CreateInstance(rowType, paths.Length);
            for (var i = 0; i < paths.Length; i++)
            {
                var row = Activator.CreateInstance(rowType, true);
                rowType.GetField("path").SetValue(row, paths[i]); rows.SetValue(row, i);
            }
            return rows;
        }
        private static void RejectLayoutInput(MethodInfo check, params object[] arguments)
        {
            var error = Assert.Throws<TargetInvocationException>(() => check.Invoke(null, arguments));
            Assert.IsInstanceOf<InvalidOperationException>(error.InnerException);
        }
        private static void AssertCompleteLayoutLease()
        {
            // Independent contract paths: do not derive this expectation from the production array.
            var paths = new[] {
                "Assets/Scripts/FightMatch/Host/Editor/FightMatchAndroidBuild.cs",
                "Assets/Scripts/FightMatch/Host/FightMatchHostView.cs",
                "Assets/Scripts/FightMatch/Presentation/PlayerBattleView.cs",
                "Assets/Scripts/FightMatch/Presentation/PlayerDefaultReferenceView.cs",
                "Assets/Scripts/FightMatch/Presentation/PlayerNavigationRecoveryView.cs",
                "Assets/Scripts/FightMatch/Presentation/CandidateBoardInputView.cs",
                "Assets/Scripts/FightMatch/Presentation/CandidateBattlePlaybackView.cs",
                "Assets/Scripts/FightMatch/Presentation/FightMatchResponsiveLayout.cs",
                "Assets/Tests/EditMode/FightMatch/UguiResponsiveLayoutTests.cs",
                "Assets/Tests/EditMode/FightMatch/CandidateBoardInputTestData.cs",
                "Assets/Tests/EditMode/FightMatch/PlayerNavigationTestFixture.cs",
                "Assets/Tests/EditMode/FightMatch/PlayerBattlePresentationTests.cs",
                "Assets/Tests/EditMode/FightMatch/CandidateBattlePlaybackPanelTests.cs",
                "Assets/Tests/EditMode/FightMatch/UguiSceneCompositionTests.cs",
                "Assets/Tests/EditMode/FightMatch/LocalizedTextBindingTests.cs",
            };
            var editor = AppDomain.CurrentDomain.GetAssemblies().Select(x => x.GetType("FightMatch.Host.Editor.FightMatchAndroidBuild"))
                .Single(x => x != null);
            var rowType = editor.GetNestedType("LayoutFile", BindingFlags.NonPublic); Assert.IsNotNull(rowType);
            var scope = editor.GetMethod("LayoutCheckSourceSet", BindingFlags.NonPublic | BindingFlags.Static); Assert.IsNotNull(scope);
            var files = editor.GetMethod("LayoutCheckFiles", BindingFlags.NonPublic | BindingFlags.Static); Assert.IsNotNull(files);
            Assert.DoesNotThrow(() => scope.Invoke(null, new object[] { LayoutLeaseFiles(rowType, paths) }));
            Assert.DoesNotThrow(() => scope.Invoke(null, new object[] { LayoutLeaseFiles(rowType, paths.Reverse().ToArray()) }));
            for (var missing = 0; missing < paths.Length; missing++)
                RejectLayoutInput(scope, LayoutLeaseFiles(rowType, paths.Where((_, i) => i != missing).ToArray()));
            RejectLayoutInput(scope, LayoutLeaseFiles(rowType, paths.Take(9).ToArray()));
            RejectLayoutInput(scope, LayoutLeaseFiles(rowType, paths.Concat(new[] { "Assets/unreviewed.cs" }).ToArray()));
            RejectLayoutInput(scope, LayoutLeaseFiles(rowType, paths.Concat(new[] { paths[0] }).ToArray()));
            foreach (var replacement in new[] { paths[0], "Assets/unreviewed.cs", "", null, paths[14].ToUpperInvariant() })
            {
                var changed = (string[])paths.Clone(); changed[14] = replacement;
                RejectLayoutInput(scope, LayoutLeaseFiles(rowType, changed));
            }
            var nullEntry = LayoutLeaseFiles(rowType, paths); nullEntry.SetValue(null, 14);
            RejectLayoutInput(scope, nullEntry); RejectLayoutInput(scope, LayoutLeaseFiles(rowType));
            RejectLayoutInput(scope, new object[] { null });
            var root = System.IO.Path.GetFullPath(System.IO.Path.Combine(UnityEngine.Application.dataPath, ".."));
            foreach (var path in paths.Skip(9))
            {
                var bytes = System.IO.File.ReadAllBytes(System.IO.Path.Combine(root, path)); string sha;
                using (var algorithm = System.Security.Cryptography.SHA256.Create())
                    sha = BitConverter.ToString(algorithm.ComputeHash(bytes)).Replace("-", "").ToLowerInvariant();
                var rows = LayoutLeaseFiles(rowType, path); var row = rows.GetValue(0);
                rowType.GetField("bytes").SetValue(row, (long)bytes.Length); rowType.GetField("sha256").SetValue(row, sha);
                Assert.DoesNotThrow(() => files.Invoke(null, new object[] { root, rows }), path);
                var wrongSha = (sha[0] == '0' ? "1" : "0") + sha.Substring(1);
                rowType.GetField("sha256").SetValue(row, wrongSha); RejectLayoutInput(files, root, rows);
                rowType.GetField("sha256").SetValue(row, sha); rowType.GetField("bytes").SetValue(row, (long)bytes.Length + 1);
                RejectLayoutInput(files, root, rows);
            }
        }
        [Test]
        public void LAYOUT_01_ReferenceRecoveryAndModalHierarchyMatchesAdjudication()
        {
            AssertCompleteLayoutLease();
            using (var rig = new UguiHostRig(bind: false))
            {
                AssertExactTextTargets(rig.Root);
                Assert.IsNull(rig.Root.transform.Find("RuntimeCanvas/SafeAreaRoot/" + BattlePath + "/BattleScroll"));
                Assert.AreEqual(R(rig, BattlePath + "/BottomHud/ReferenceInfoDrawer"), rig.View.BattleView.ReferenceView.transform);
                Assert.AreEqual("SystemLayer", R(rig, RecoveryPath).parent.name);
                var layout = rig.Root.GetComponentInChildren<FightMatchResponsiveLayout>(true);
                var panels = References<RectTransform>(layout, "dialogPanels"); Assert.AreEqual(7, panels.Length);
                foreach (var panel in panels)
                {
                    Assert.AreEqual("RootMask", panel.parent.name);
                    Assert.IsNotNull(panel.Find("Header/Title")); Assert.IsNotNull(panel.Find("Actions"));
                    Assert.IsNotNull(panel.Find("BodyViewport/Viewport/Content"));
                }
                Assert.AreEqual(R(rig, ConfirmationPath + "/RootMask/BattleConfirmationPanel/Header/Title"),
                    Reference<LocalizedTmpText>(rig.View.BattleView, "dialogTitle").transform);
                Assert.AreEqual(R(rig, RecoveryPath + "/RootMask/BattleRecoveryPanel/Header/Title"),
                    Reference<LocalizedTmpText>(rig.View.BattleView, "recoveryTitle").transform);
                Assert.IsTrue(FightMatchViewId.Validate(rig.Root.transform, out var code), code);
                Assert.AreEqual(1, rig.Root.GetComponentsInChildren<FightMatchViewId>(true).Count(x => x.Id == "fm.action.history.open"));
            }
        }
        [UnityTest]
        public IEnumerator LAYOUT_02_540SafeAreaUsesAcceptedFiveRegionGeometry()
        {
            EmptyScene(); yield return new EnterPlayMode(); enteredPlayMode = true;
            using (var size = new OverlayResolution(540, 960))
            {
                yield return size.Ready(540, 960);
                using (var rig = OverlayRig(new Rect(16, 16, 508, 920), 540, 960))
                {
                    Geometry(rig, 508, 920, 166, 142); yield return null;
                    Geometry(rig, 508, 920, 166, 142); Commands(rig);
                    foreach (var cell in new[] { new FlowPuzzle.Core.FlowPos(0, 0), new FlowPuzzle.Core.FlowPos(rig.State.Board.Face.Width - 1, rig.State.Board.Face.Height - 1) })
                        Assert.AreEqual(cell, rig.Board.CellAtLocal(rig.Board.CellCenter(cell)));
                    VictoryCaption(rig);
                }
            }
            yield return new ExitPlayMode(); enteredPlayMode = false;
        }
        [UnityTest]
        public IEnumerator LAYOUT_03_LongScreenConvertsPhysicalSafeAreaToAcceptedCanvasTable()
        {
            EmptyScene(); yield return new EnterPlayMode(); enteredPlayMode = true;
            using (var size = new OverlayResolution(1080, 2400))
            {
                yield return size.Ready(1080, 2400);
                using (var rig = OverlayRig(new Rect(32, 120, 1016, 2160), 1080, 2400))
                {
                    var factor = Mathf.Sqrt(5); var k = 2 / factor;
                    Near(factor, rig.Root.GetComponentInChildren<Canvas>().scaleFactor, "Canvas scaleFactor");
                    Geometry(rig, 1016 / factor, 2160 / factor, 166 * k, 142 * k); Commands(rig);
                    var retry = (RectTransform)rig.Find<Button>("fm.action.battle.retry").transform;
                    Near(48 * k, CanvasRect(rig, retry).height, "Canvas hot zone");
                    var corners = new Vector3[4]; retry.GetWorldCorners(corners); Near(96, corners[1].y - corners[0].y, "96 physical pixel hot zone");
                    yield return null; Geometry(rig, 1016 / factor, 2160 / factor, 166 * k, 142 * k); VictoryCaption(rig);
                }
            }
            yield return new ExitPlayMode(); enteredPlayMode = false;
        }
        // This case isolates the compression formula; the eight Overlay cases retain the real scaler.
        [Test]
        public void LAYOUT_04_ShortHeightCompressionStopsAtStageAndBottomHudMinima()
        {
            using (var rig = new UguiHostRig(true))
            {
                var canvas = rig.Root.GetComponentInChildren<Canvas>(true); canvas.GetComponent<CanvasScaler>().enabled = false;
                canvas.renderMode = RenderMode.WorldSpace; canvas.transform.localScale = Vector3.one; canvas.scaleFactor = 1;
                var safe = (RectTransform)Safe(rig).transform; Safe(rig).enabled = false; safe.anchorMin = safe.anchorMax = Vector2.zero;
                rig.Enter();
                foreach (var entry in new[] { new Vector3(920, 166, 142), new Vector3(880, 126, 142), new Vector3(850, 96, 142), new Vector3(830, 96, 122), new Vector3(812, 96, 104) })
                { safe.sizeDelta = new Vector2(508, entry.x); Canvas.ForceUpdateCanvases(); Geometry(rig, 508, entry.x, entry.y, entry.z); }
            }
        }
        private static float[] LayoutGeometry(FightMatchResponsiveLayout layout)
        {
            var roots = new[] { "screenLayer", "topBar", "battleContent", "stage", "battleStatus", "boardRegion", "bottomHud",
                "normalHudRoot", "historyDrawerRoot", "referenceDrawerRoot", "normalStatusViewport", "normalMainRow",
                "startupViewport", "navigationViewport", "resultViewport" }.Select(x => Reference<RectTransform>(layout, x));
            return roots.Concat(References<RectTransform>(layout, "dialogPanels")).SelectMany(x => new[] {
                x.anchorMin.x, x.anchorMin.y, x.anchorMax.x, x.anchorMax.y, x.pivot.x, x.pivot.y,
                x.anchoredPosition.x, x.anchoredPosition.y, x.sizeDelta.x, x.sizeDelta.y,
                x.localScale.x, x.localScale.y, x.localScale.z }).ToArray();
        }
        private static void LayoutSubscriptions(FightMatchResponsiveLayout layout, SafeAreaFitter safe, int expected)
        {
            var changed = typeof(SafeAreaFitter).GetField("Changed", BindingFlags.Instance | BindingFlags.NonPublic);
            var render = typeof(Canvas).GetField("willRenderCanvases", BindingFlags.Static | BindingFlags.NonPublic);
            Assert.IsNotNull(changed); Assert.IsNotNull(render);
            foreach (var callbacks in new[] { (Delegate)changed.GetValue(safe), (Delegate)render.GetValue(null) })
                Assert.AreEqual(expected, callbacks?.GetInvocationList().Count(x => ReferenceEquals(x.Target, layout)) ?? 0);
        }
        private static void ReenabledInvalidLayoutCancelsDisabledGesture(UguiHostRig rig, FightMatchResponsiveLayout layout,
            CanvasGroup otherGroup, Button otherButton, GameObject otherDiagnostic)
        {
            var group = R(rig, BattlePath).GetComponent<CanvasGroup>();
            var diagnostic = R(rig, "SystemLayer/LayoutDiagnostic").gameObject;
            var head = rig.Head; var calls = rig.Storage.SnapshotCreates; var files = CopyFiles(rig.Storage.Files);
            foreach (var wasInvalid in new[] { false, true })
            {
                ApplySafe(rig, new Rect(16, 16, 508, 812), 540, 960);
                var geometry = LayoutGeometry(layout); var boardRect = rig.Board.rectTransform.rect;
                if (wasInvalid) ApplySafe(rig, new Rect(16, 16, 508, 811), 540, 960);
                var notifications = new List<bool>(); Action<bool> observe = notifications.Add;
                layout.ValidityChanged += observe;
                try
                {
                    if (!wasInvalid) rig.BeginRoute(21);
                    layout.enabled = false;
                    CollectionAssert.IsEmpty(notifications); LayoutSubscriptions(layout, Safe(rig), 0);
                    Assert.IsFalse(diagnostic.activeSelf); Assert.IsTrue(group.interactable); Assert.IsTrue(group.blocksRaycasts);
                    Assert.AreSame(rig.Session.Battle, rig.View.BattleView.Controller);
                    if (!wasInvalid)
                    {
                        Assert.IsTrue(rig.Board.HasActivePointer, "Disable alone must preserve the existing gesture.");
                        rig.Driver.Cancel(21, UguiPointerDriver.Point(rig.Board, rig.Route().Last()));
                        Assert.IsFalse(rig.Board.HasActivePointer);
                    }
                    var pointer = 22; rig.BeginRoute(pointer);
                    if (!wasInvalid)
                    {
                        ApplySafe(rig, new Rect(16, 16, 508, 811), 540, 960);
                        // The board independently cancels on safe-area geometry changes. Start again while layout is still disabled.
                        Assert.IsFalse(rig.Board.HasActivePointer); rig.EndRoute(pointer);
                        pointer = 23; rig.BeginRoute(pointer);
                    }
                    Assert.IsTrue(rig.Board.HasActivePointer); CollectionAssert.IsEmpty(notifications);
                    var endpoint = UguiPointerDriver.Point(rig.Board, rig.Route().Last());
                    layout.enabled = true;
                    Assert.IsFalse(rig.Board.HasActivePointer, "First invalid sample must notify the real Host immediately.");
                    Assert.IsNull(rig.Session.Battle.Input.Gesture.ActivePointerId);
                    CollectionAssert.AreEqual(new[] { false }, notifications); LayoutSubscriptions(layout, Safe(rig), 1);
                    Assert.IsTrue(diagnostic.activeSelf); Assert.IsFalse(group.interactable); Assert.IsFalse(group.blocksRaycasts);
                    for (var refresh = 0; refresh < 3; refresh++) Canvas.ForceUpdateCanvases();
                    CollectionAssert.AreEqual(new[] { false }, notifications);
                    rig.Driver.Move(endpoint, pointer); rig.EndRoute(pointer);
                    Assert.IsFalse(rig.Board.HasActivePointer); Assert.IsNull(rig.Session.Battle.Input.LastRequest);
                    ApplySafe(rig, new Rect(16, 16, 508, 810), 540, 960);
                    CollectionAssert.AreEqual(new[] { false }, notifications);
                    CollectionAssert.AreEqual(geometry, LayoutGeometry(layout)); Assert.AreEqual(boardRect, rig.Board.rectTransform.rect);
                    Assert.AreSame(head, rig.Head); Assert.AreEqual(calls, rig.Storage.SnapshotCreates); SameFiles(files, rig.Storage.Files);
                    Assert.IsFalse(otherGroup.interactable); Assert.IsFalse(otherGroup.blocksRaycasts);
                    Assert.IsFalse(otherButton.interactable); Assert.IsTrue(otherDiagnostic.activeSelf);
                    ApplySafe(rig, new Rect(16, 16, 508, 812), 540, 960); Geometry(rig, 508, 812, 96, 104);
                    CollectionAssert.AreEqual(new[] { false, true }, notifications);
                    Assert.IsFalse(diagnostic.activeSelf); Assert.IsTrue(group.interactable); Assert.IsTrue(group.blocksRaycasts);
                }
                finally { layout.ValidityChanged -= observe; }
            }
        }
        private static void DisabledLayoutBindDefersSampling(UguiHostRig rig, FightMatchResponsiveLayout layout,
            CanvasGroup otherGroup, Button otherButton, GameObject otherDiagnostic)
        {
            var group = R(rig, BattlePath).GetComponent<CanvasGroup>();
            var diagnostic = R(rig, "SystemLayer/LayoutDiagnostic").gameObject;
            var label = Reference<LocalizedTmpText>(layout, "layoutDiagnosticText");
            var geometry = LayoutGeometry(layout); var boardRect = rig.Board.rectTransform.rect;
            var head = rig.Head; var calls = rig.Storage.SnapshotCreates; var files = CopyFiles(rig.Storage.Files);
            var notifications = new List<bool>(); Action<bool> observe = notifications.Add;
            var service = new LocalizationService(new UguiTestTextSource(), SystemLanguage.English);
            Action released = () => {
                CollectionAssert.IsEmpty(notifications); LayoutSubscriptions(layout, Safe(rig), 0);
                Assert.IsFalse(diagnostic.activeSelf); Assert.IsTrue(group.interactable); Assert.IsTrue(group.blocksRaycasts);
                CollectionAssert.AreEqual(geometry, LayoutGeometry(layout)); Assert.AreEqual(boardRect, rig.Board.rectTransform.rect);
                Assert.IsFalse(otherGroup.interactable); Assert.IsFalse(otherGroup.blocksRaycasts);
                Assert.IsFalse(otherButton.interactable); Assert.IsTrue(otherDiagnostic.activeSelf);
                Assert.AreSame(head, rig.Head); Assert.AreEqual(calls, rig.Storage.SnapshotCreates); SameFiles(files, rig.Storage.Files);
                Assert.IsNull(rig.Session.Battle.Input.LastRequest);
            };
            layout.ValidityChanged += observe;
            try
            {
                layout.enabled = false; ApplySafe(rig, new Rect(16, 16, 508, 811), 540, 960);
                rig.BeginRoute(31); var endpoint = UguiPointerDriver.Point(rig.Board, rig.Route().Last());
                foreach (var locale in new[] { LocaleId.ZhHans, LocaleId.En })
                {
                    layout.Bind(service); service.SetLocale(locale); Canvas.ForceUpdateCanvases(); released();
                    Assert.IsTrue(rig.Board.HasActivePointer, "Direct layout Bind must not cancel while disabled.");
                    Assert.AreSame(rig.Session.Battle, rig.View.BattleView.Controller);
                    Assert.AreEqual(service.Resolve(label.Key,
                        new[] { new KeyValuePair<string, string>("errorCode", "LayoutInvalid") }).Text, label.Target.text);
                }
                layout.enabled = true;
                Assert.IsFalse(rig.Board.HasActivePointer); Assert.IsNull(rig.Session.Battle.Input.Gesture.ActivePointerId);
                CollectionAssert.AreEqual(new[] { false }, notifications); LayoutSubscriptions(layout, Safe(rig), 1);
                Assert.IsTrue(diagnostic.activeSelf); Assert.IsFalse(group.interactable); Assert.IsFalse(group.blocksRaycasts);
                rig.Driver.Move(endpoint, 31); rig.EndRoute(31);
                for (var refresh = 0; refresh < 3; refresh++) Canvas.ForceUpdateCanvases();
                CollectionAssert.AreEqual(new[] { false }, notifications); Assert.IsNull(rig.Session.Battle.Input.LastRequest);
                ApplySafe(rig, new Rect(16, 16, 508, 812), 540, 960); Geometry(rig, 508, 812, 96, 104);
                CollectionAssert.AreEqual(new[] { false, true }, notifications); notifications.Clear();

                layout.enabled = false; ApplySafe(rig, new Rect(16, 16, 508, 811), 540, 960); rig.BeginRoute(32);
                rig.Bind(); // Host Unbind legitimately cancels this pointer; this is separate from direct layout Bind above.
                Assert.IsFalse(rig.Board.HasActivePointer); rig.EndRoute(32); released();
                Assert.IsFalse(layout.enabled); Assert.AreSame(rig.Session.Battle, rig.View.BattleView.Controller);
                rig.Bind(); Canvas.ForceUpdateCanvases(); released();
                layout.enabled = true; CollectionAssert.AreEqual(new[] { false }, notifications);
                LayoutSubscriptions(layout, Safe(rig), 1); Assert.IsTrue(diagnostic.activeSelf);
                Assert.IsFalse(group.interactable); Assert.IsFalse(group.blocksRaycasts);
                ApplySafe(rig, new Rect(16, 16, 508, 812), 540, 960); Geometry(rig, 508, 812, 96, 104);
                CollectionAssert.AreEqual(new[] { false, true }, notifications); notifications.Clear();

                layout.enabled = false; ApplySafe(rig, new Rect(16, 16, 508, 811), 540, 960);
                rig.Root.SetActive(false); layout.enabled = true;
                Assert.IsTrue(layout.enabled); Assert.IsFalse(layout.isActiveAndEnabled);
                for (var repeat = 0; repeat < 2; repeat++)
                { layout.Bind(service); Canvas.ForceUpdateCanvases(); released(); }
                Assert.AreNotEqual(LocalizedTmpText.Placeholder, label.Target.text);
                rig.Bind(); // Activation supplies the first real sample after binding the inactive hierarchy.
                CollectionAssert.AreEqual(new[] { false }, notifications); LayoutSubscriptions(layout, Safe(rig), 1);
                Assert.IsTrue(diagnostic.activeSelf); Assert.IsFalse(group.interactable); Assert.IsFalse(group.blocksRaycasts);
                for (var refresh = 0; refresh < 3; refresh++) Canvas.ForceUpdateCanvases();
                CollectionAssert.AreEqual(new[] { false }, notifications);
                ApplySafe(rig, new Rect(16, 16, 508, 812), 540, 960); Geometry(rig, 508, 812, 96, 104);
                CollectionAssert.AreEqual(new[] { false, true }, notifications);
                Assert.IsFalse(diagnostic.activeSelf); Assert.IsTrue(group.interactable); Assert.IsTrue(group.blocksRaycasts);
                Assert.IsFalse(otherGroup.interactable); Assert.IsFalse(otherGroup.blocksRaycasts);
                Assert.IsFalse(otherButton.interactable); Assert.IsTrue(otherDiagnostic.activeSelf);
                Assert.AreSame(head, rig.Head); Assert.AreEqual(calls, rig.Storage.SnapshotCreates); SameFiles(files, rig.Storage.Files);
                Assert.IsNull(rig.Session.Battle.Input.LastRequest);
            }
            finally { layout.ValidityChanged -= observe; }
        }
        [UnityTest]
        public IEnumerator LAYOUT_05_TooShortHeightFailsClosedWithoutBoardOrHotZoneShrink()
        {
            EmptyScene(); yield return new EnterPlayMode(); enteredPlayMode = true;
            using (var size = new OverlayResolution(540, 960))
            {
                yield return size.Ready(540, 960);
                using (var rig = OverlayRig(new Rect(16, 16, 508, 920), 540, 960))
                {
                    var rect = rig.Board.rectTransform.rect; var head = rig.Head; var calls = rig.Storage.SnapshotCreates;
                    var files = CopyFiles(rig.Storage.Files); rig.BeginRoute();
                    var layout = rig.Root.GetComponentInChildren<FightMatchResponsiveLayout>(true);
                    var geometry = LayoutGeometry(layout); var safe = Safe(rig);
                    var label = Reference<LocalizedTmpText>(layout, "layoutDiagnosticText");
                    var diagnostic = R(rig, "SystemLayer/LayoutDiagnostic").gameObject;
                    var independent = new GameObject("IndependentLayoutLock", typeof(CanvasGroup), typeof(Button));
                    independent.transform.SetParent(rig.Root.transform, false); // Owned by the existing rig's disposal.
                    var otherGroup = independent.GetComponent<CanvasGroup>(); otherGroup.interactable = otherGroup.blocksRaycasts = false;
                    var otherButton = independent.GetComponent<Button>(); otherButton.interactable = false;
                    var otherDiagnostic = new GameObject("IndependentDiagnostic"); otherDiagnostic.transform.SetParent(independent.transform, false);
                    LayoutSubscriptions(layout, safe, 1);
                    ApplySafe(rig, new Rect(16, 16, 508, 811), 540, 960);
                    Assert.IsTrue(R(rig, "SystemLayer/LayoutDiagnostic").gameObject.activeInHierarchy);
                    var group = R(rig, BattlePath).GetComponent<CanvasGroup>(); Assert.IsFalse(group.interactable); Assert.IsFalse(group.blocksRaycasts);
                    Assert.IsFalse(rig.Board.HasActivePointer); Assert.AreEqual(rect, rig.Board.rectTransform.rect);
                    var diagnosticCopy = label.Target.text; Assert.AreNotEqual(LocalizedTmpText.Placeholder, diagnosticCopy);
                    layout.enabled = false;
                    Assert.IsFalse(diagnostic.activeSelf); Assert.IsTrue(group.interactable); Assert.IsTrue(group.blocksRaycasts);
                    Assert.AreSame(rig.Session.Battle, rig.View.BattleView.Controller); Assert.IsTrue(rig.View.enabled);
                    Assert.AreEqual(diagnosticCopy, label.Target.text); CollectionAssert.AreEqual(geometry, LayoutGeometry(layout));
                    LayoutSubscriptions(layout, safe, 0);
                    foreach (var height in new[] { 810, 920, 809 })
                    {
                        ApplySafe(rig, new Rect(16, 16, 508, height), 540, 960); Canvas.ForceUpdateCanvases();
                        Assert.IsFalse(diagnostic.activeSelf); Assert.IsTrue(group.interactable); Assert.IsTrue(group.blocksRaycasts);
                        CollectionAssert.AreEqual(geometry, LayoutGeometry(layout)); Assert.AreEqual(rect, rig.Board.rectTransform.rect);
                    }
                    var locale = rig.Localization.CurrentLocale;
                    rig.Localization.SetLocale(locale == LocaleId.En ? LocaleId.ZhHans : LocaleId.En);
                    Assert.AreEqual(rig.Localization.Resolve(label.Key,
                        new[] { new KeyValuePair<string, string>("errorCode", "LayoutInvalid") }).Text, label.Target.text);
                    rig.Localization.SetLocale(locale);
                    Assert.IsFalse(otherGroup.interactable); Assert.IsFalse(otherGroup.blocksRaycasts);
                    Assert.IsFalse(otherButton.interactable); Assert.IsTrue(otherDiagnostic.activeSelf);
                    layout.enabled = true; LayoutSubscriptions(layout, safe, 1);
                    Assert.IsTrue(diagnostic.activeSelf); Assert.IsFalse(group.interactable); Assert.IsFalse(group.blocksRaycasts);
                    CollectionAssert.AreEqual(geometry, LayoutGeometry(layout));
                    rig.EndRoute(); yield return null; Assert.AreSame(head, rig.Head); Assert.AreEqual(calls, rig.Storage.SnapshotCreates);
                    SameFiles(files, rig.Storage.Files); Assert.IsNull(rig.Session.Battle.Input.LastRequest);
                    ApplySafe(rig, new Rect(16, 16, 508, 812), 540, 960); Geometry(rig, 508, 812, 96, 104);
                    Assert.IsTrue(group.interactable); Assert.IsTrue(group.blocksRaycasts);
                    ReenabledInvalidLayoutCancelsDisabledGesture(rig, layout, otherGroup, otherButton, otherDiagnostic);
                    DisabledLayoutBindDefersSampling(rig, layout, otherGroup, otherButton, otherDiagnostic);
                    for (var repeat = 0; repeat < 2; repeat++)
                    {
                        layout.enabled = false; layout.enabled = false; LayoutSubscriptions(layout, safe, 0);
                        Canvas.ForceUpdateCanvases(); Assert.IsFalse(diagnostic.activeSelf);
                        Assert.IsTrue(group.interactable); Assert.IsTrue(group.blocksRaycasts);
                        layout.enabled = true; LayoutSubscriptions(layout, safe, 1); Geometry(rig, 508, 812, 96, 104);
                    }
                    layout.enabled = false; layout.Unbind(); layout.Unbind(); LayoutSubscriptions(layout, safe, 0);
                    ApplySafe(rig, new Rect(16, 16, 508, 811), 540, 960); Canvas.ForceUpdateCanvases();
                    Assert.IsFalse(diagnostic.activeSelf); Assert.IsTrue(group.interactable); Assert.IsTrue(group.blocksRaycasts);
                    Assert.IsFalse(otherGroup.interactable); Assert.IsFalse(otherGroup.blocksRaycasts);
                    Assert.IsFalse(otherButton.interactable); Assert.IsTrue(otherDiagnostic.activeSelf);
                    UnityEngine.Object.DestroyImmediate(layout); LayoutSubscriptions(layout, safe, 0);
                    Assert.AreSame(head, rig.Head); Assert.AreEqual(calls, rig.Storage.SnapshotCreates);
                    SameFiles(files, rig.Storage.Files); Assert.IsNull(rig.Session.Battle.Input.LastRequest);
                }
            }
            yield return new ExitPlayMode(); enteredPlayMode = false;
        }
        [Test]
        public void LAYOUT_06_LayoutOwnershipHasNoSameAxisControllerConflict()
        {
            using (var rig = new UguiHostRig(bind: false))
            {
                var layout = rig.Root.GetComponentInChildren<FightMatchResponsiveLayout>(true);
                foreach (var name in new[] { "screenLayer", "topBar", "battleContent", "stage", "battleStatus", "boardRegion", "bottomHud",
                    "normalHudRoot", "historyDrawerRoot", "referenceDrawerRoot", "normalStatusViewport", "normalMainRow", "startupViewport", "navigationViewport", "resultViewport" })
                {
                    var rect = Reference<RectTransform>(layout, name);
                    Assert.IsNull(rect.GetComponent<ContentSizeFitter>(), name); Assert.IsNull(rect.GetComponent<AspectRatioFitter>(), name);
                    Assert.IsNull(rect.parent.GetComponent<LayoutGroup>(), "Parent must not drive responsive root " + name);
                }
                foreach (var scroll in rig.Root.GetComponentsInChildren<ScrollRect>(true))
                {
                    Assert.AreEqual(scroll.transform, scroll.viewport.parent); Assert.AreEqual(scroll.viewport, scroll.content.parent);
                    Assert.IsNotNull(scroll.viewport.GetComponent<Image>()); Assert.IsNotNull(scroll.viewport.GetComponent<Mask>());
                    Assert.AreEqual(ScrollRect.MovementType.Clamped, scroll.movementType); Assert.IsFalse(scroll.inertia); Assert.AreEqual(0, scroll.elasticity);
                }
                foreach (var panel in References<RectTransform>(layout, "dialogPanels").Where(x => x.name != "DialogPanel"))
                {
                    var content = panel.Find("BodyViewport/Viewport/Content");
                    Assert.IsFalse(content.GetComponent<VerticalLayoutGroup>().childControlHeight);
                    Assert.IsFalse(content.GetComponent<VerticalLayoutGroup>().childForceExpandHeight);
                    Assert.IsNotNull(content.Find("StateRows").GetComponent<ContentSizeFitter>());
                    Assert.IsNotNull(content.Find("ChoiceRows").GetComponent<ContentSizeFitter>());
                }
            }
        }
        [UnityTest]
        public IEnumerator LAYOUT_07_RuntimeResizeAndSafeAreaVersionApplyOnceWithoutZeroFrame()
        {
            EmptyScene(); yield return new EnterPlayMode(); enteredPlayMode = true;
            using (var size = new OverlayResolution(540, 960))
            {
                yield return size.Ready(540, 960);
                using (var rig = OverlayRig(new Rect(16, 16, 508, 920), 540, 960))
                {
                    var version = Safe(rig).Version; var head = rig.Head; var files = CopyFiles(rig.Storage.Files);
                    var frames = 0; Canvas.WillRenderCanvases observe = () => {
                        frames++; Assert.Greater(rig.Board.rectTransform.rect.width, 0); Assert.Greater(rig.Board.rectTransform.rect.height, 0);
                    };
                    Canvas.willRenderCanvases += observe;
                    try
                    {
                        ApplySafe(rig, new Rect(16, 16, 508, 920), 540, 960); Assert.AreEqual(version, Safe(rig).Version);
                        for (var n = 0; n < 4; n++) Canvas.ForceUpdateCanvases();
                        ApplySafe(rig, new Rect(16, 16, 508, 830), 540, 960); Assert.AreEqual(version + 1, Safe(rig).Version);
                        Geometry(rig, 508, 830, 96, 122); yield return null;
                        using (var resized = new OverlayResolution(1080, 2400))
                        {
                            yield return resized.Ready(1080, 2400); ApplySafe(rig, new Rect(32, 120, 1016, 2160), 1080, 2400);
                            var factor = Mathf.Sqrt(5); Geometry(rig, 1016 / factor, 2160 / factor, 332 / factor, 284 / factor);
                            var current = R(rig, NormalPath + "/MainRow").localScale;
                            for (var n = 0; n < 4; n++) Canvas.ForceUpdateCanvases(); Assert.AreEqual(current, R(rig, NormalPath + "/MainRow").localScale);
                        }
                        Assert.Greater(frames, 0); Assert.AreSame(head, rig.Head); SameFiles(files, rig.Storage.Files);
                    }
                    finally { Canvas.willRenderCanvases -= observe; }
                    rig.View.Unbind(); rig.View.Unbind(); rig.Bind(); Assert.IsFalse(rig.View.DiagnosticVisible, rig.View.DiagnosticCode);
                }
            }
            yield return new ExitPlayMode(); enteredPlayMode = false;
        }
        private static void LongBody(UguiHostRig rig, RectTransform panel)
        {
            var scroll = panel.Find("BodyViewport").GetComponent<ScrollRect>();
            var content = scroll.content.Find("StateRows") ?? scroll.content;
            var template = rig.Root.GetComponentsInChildren<LocalizedTmpText>(true).Single(x => x.name == "TextTemplate");
            var rows = new List<LocalizedTmpText>();
            try
            {
                for (var i = 0; i < 24; i++)
                {
                    var row = UnityEngine.Object.Instantiate(template, content, false);
                    row.GetComponent<FightMatchViewId>().Assign(FightMatchViewId.Row("layout.fixture.row." + i));
                    row.Bind(rig.Localization, "fm.save_recovery.blocking_notice"); row.gameObject.SetActive(true); rows.Add(row);
                }
                foreach (var locale in new[] { LocaleId.En, LocaleId.ZhHans })
                {
                    rig.Localization.SetLocale(locale); Canvas.ForceUpdateCanvases();
                    var header = CanvasRect(rig, (RectTransform)panel.Find("Header")); var actions = CanvasRect(rig, (RectTransform)panel.Find("Actions"));
                    Assert.Greater(scroll.content.rect.height, scroll.viewport.rect.height);
                    foreach (var end in new[] { 1f, 0f })
                    {
                        scroll.verticalNormalizedPosition = end; Canvas.ForceUpdateCanvases();
                        Assert.AreEqual(header, CanvasRect(rig, (RectTransform)panel.Find("Header")));
                        Assert.AreEqual(actions, CanvasRect(rig, (RectTransform)panel.Find("Actions")));
                        var row = CanvasRect(rig, (RectTransform)(end == 1 ? rows[0] : rows.Last()).transform); var viewport = CanvasRect(rig, scroll.viewport);
                        Assert.IsTrue(viewport.Overlaps(row), "First/last body row must be reachable");
                    }
                    Assert.GreaterOrEqual(CanvasRect(rig, scroll.viewport).height, 48 * Mathf.Min(((RectTransform)Safe(rig).transform).rect.width, 508) / 508 - .1f);
                }
            }
            finally { foreach (var row in rows) { row.Unbind(); UnityEngine.Object.DestroyImmediate(row.gameObject); } }
        }
        [UnityTest]
        public IEnumerator LAYOUT_08_ModalAndRecoveryScrollRectsKeepHeaderAndActionsFixed()
        {
            EmptyScene(); yield return new EnterPlayMode(); enteredPlayMode = true;
            using (var size = new OverlayResolution(540, 960))
            {
                yield return size.Ready(540, 960);
                using (var rig = OverlayRig(new Rect(16, 16, 508, 920), 540, 960))
                {
                    var view = rig.Session.Battle.View;
                    rig.Session.Battle.PreviewEnd(rig.View.BattleView.Page, CandidateApplicationKind.ExitAttempt, view.Context);
                    rig.View.SynchronizeOverlayRoots(); Canvas.ForceUpdateCanvases();
                    var panel = R(rig, ConfirmationPath + "/RootMask/BattleConfirmationPanel"); Assert.IsTrue(panel.gameObject.activeInHierarchy);
                    LongBody(rig, panel); VisibleCopy(rig);
                    rig.Driver.Click(rig.Find<Button>(FightMatchViewId.Row("cancel-battle-end")));
                    rig.Storage.Fault = "snapshot-before"; rig.BeginRoute(); rig.EndRoute(); rig.View.SynchronizeOverlayRoots();
                    panel = R(rig, RecoveryPath + "/RootMask/BattleRecoveryPanel"); Assert.IsTrue(panel.gameObject.activeInHierarchy);
                    var request = rig.Session.Battle.Input.LastRequest; LongBody(rig, panel); Assert.AreSame(request, rig.Session.Battle.Input.LastRequest);
                    VisibleCopy(rig); yield return null;
                }
            }
            yield return new ExitPlayMode(); enteredPlayMode = false;
        }
        [Test]
        public void LAYOUT_09_FixedSlotsUseOriginalSlotWithoutCompaction()
        {
            foreach (var unsupported in new[] { false, true })
            using (var battle = new BattleApplicationRig(ready: false))
            using (var canvas = new BattleUguiRoot())
            {
                var input = battle.Runtime.Domain.EntryInput;
                input.Members[0].OriginalSlot = 2;
                var character = battle.Runtime.Domain.Character;
                battle.Runtime.Domain.Character = new CandidateCharacterState(character.Definition, character.PlayerId, character.CharacterId,
                    character.Level, character.Experience, 2, character.StateRevision, character.BaseRewards, character.ProcessedEnds, character.RecoveryPeriods);
                input.Level.Faces[0].Pairs[0].Enemy.OriginalSlot = unsupported ? 7 : 2;
                input.Level.Faces[0].Pairs[1].Enemy.OriginalSlot = 0;
                ApplicationRuntimeRig.Is(battle.Runtime.Open(), "InitializationReady");
                ApplicationRuntimeRig.Is(battle.Runtime.Initialize(), "Completed"); ApplicationRuntimeRig.Is(battle.Runtime.Enter(), "Completed");
                var view = canvas.Playback(); view.Attach(battle.System, LocalSaveTestFiles.B(), canvas.Localization);
                var slots = References<RectTransform>(view.InputView, "memberSlots"); var fixedIds = slots.Select(x => x.GetInstanceID()).ToArray();
                Assert.AreEqual(3, slots.Length); Assert.AreEqual(0, slots[0].GetComponentsInChildren<Button>().Length);
                Assert.AreEqual(0, slots[1].GetComponentsInChildren<Button>().Length); Assert.AreEqual(1, slots[2].GetComponentsInChildren<Button>().Length);
                Assert.AreEqual(!unsupported, view.InputView.DisplayLayoutSupported); Assert.AreEqual(!unsupported, view.InputView.Board.raycastTarget);
                Assert.AreEqual(!unsupported, slots[2].GetComponentInChildren<Button>().interactable);
                if (!unsupported)
                {
                    var health = References<LocalizedTmpText>(view.InputView, "allyHp");
                    Assert.IsFalse(health[0].gameObject.activeSelf); Assert.IsFalse(health[1].gameObject.activeSelf);
                    Assert.IsTrue(health[2].gameObject.activeSelf); StringAssert.Contains("100", health[2].Target.text);
                }
                var head = battle.Head; var calls = battle.Runtime.Storage.Base.Calls;
                foreach (var locale in new[] { LocaleId.En, LocaleId.ZhHans }) { canvas.Localization.SetLocale(locale); view.InputView.Controller.Refresh(); }
                CollectionAssert.AreEqual(fixedIds, slots.Select(x => x.GetInstanceID())); Assert.AreSame(head, battle.Head);
                Assert.AreEqual(calls, battle.Runtime.Storage.Base.Calls);
                Assert.AreEqual(unsupported ? 7 : 2, battle.State.Enemies[0].Enemy.OriginalSlot, "Domain sparse slot must not be rewritten");
                view.Unbind(); Assert.AreEqual(3, slots.Length); Assert.IsTrue(slots.All(x => x != null));
            }
            using (var battle = new BattleApplicationRig(hp: 1, level: 3))
            using (var canvas = new BattleUguiRoot())
            {
                var view = canvas.Playback(); view.Attach(battle.System, LocalSaveTestFiles.B(), canvas.Localization);
                var slots = References<RectTransform>(view.InputView, "memberSlots");
                var slot = battle.State.Members[0].Member.OriginalSlot; var original = slots[slot].GetComponentInChildren<Button>();
                ApplicationRuntimeRig.Is(battle.Runtime.Attack("layout-down", 0), "Completed"); view.InputView.Controller.Refresh();
                Assert.IsTrue(battle.State.Members[0].Hp.Numerator.IsZero);
                Assert.AreSame(original, slots[slot].GetComponentInChildren<Button>()); Assert.IsFalse(original.interactable);
                Assert.AreEqual(3, slots.Length); Assert.IsTrue(slots.All(x => x != null)); view.Unbind();
            }
        }
        [UnityTest]
        public IEnumerator LAYOUT_10_DrawersAreMutuallyExclusiveAndNeverCoverBoard()
        {
            EmptyScene(); yield return new EnterPlayMode(); enteredPlayMode = true;
            using (var size = new OverlayResolution(540, 960))
            {
                yield return size.Ready(540, 960);
                using (var rig = OverlayRig(new Rect(16, 16, 508, 920), 540, 960))
                {
                    var command = R(rig, CommandPath).GetComponent<ScrollRect>(); command.horizontalNormalizedPosition = 1; Canvas.ForceUpdateCanvases();
                    var head = rig.Head; var files = CopyFiles(rig.Storage.Files); var board = CanvasRect(rig, rig.Board.rectTransform);
                    rig.Driver.Click(rig.Find<Button>("fm.action.reference.open")); Canvas.ForceUpdateCanvases();
                    Assert.IsTrue(rig.View.BattleView.ReferenceView.IsOpen); Assert.IsFalse(R(rig, NormalPath).gameObject.activeSelf);
                    Assert.IsFalse(R(rig, BattlePath + "/BottomHud/HistoryDrawer").gameObject.activeSelf);
                    var drawer = R(rig, BattlePath + "/BottomHud/ReferenceInfoDrawer"); Assert.IsFalse(board.Overlaps(CanvasRect(rig, drawer)));
                    Near(94, drawer.Find("BodyViewport").GetComponent<RectTransform>().rect.height, "Reference body at preferred height");
                    ApplySafe(rig, new Rect(16, 16, 508, 812), 540, 960); Canvas.ForceUpdateCanvases();
                    Near(56, drawer.Find("BodyViewport").GetComponent<RectTransform>().rect.height, "Reference body at minimum height");
                    ApplySafe(rig, new Rect(16, 16, 508, 920), 540, 960); rig.BeginRoute();
                    Assert.IsFalse(rig.View.BattleView.ReferenceView.IsOpen); Assert.IsTrue(R(rig, NormalPath).gameObject.activeSelf);
                    Assert.IsTrue(rig.Board.HasActivePointer); Assert.AreSame(head, rig.Head); SameFiles(files, rig.Storage.Files);
                    rig.EndRoute(); rig.View.BattleView.PlaybackView.SkipToFinal();
                    command.horizontalNormalizedPosition = .42f; Canvas.ForceUpdateCanvases();
                    // Reveal History by its real content position, independent of the current normalized scroll offset.
                    var history = rig.Find<Button>("fm.action.history.open"); Reveal(command, (RectTransform)history.transform);
                    rig.Driver.Click(history); Canvas.ForceUpdateCanvases();
                    Assert.IsTrue(R(rig, BattlePath + "/BottomHud/HistoryDrawer").gameObject.activeSelf);
                    Assert.IsFalse(R(rig, NormalPath).gameObject.activeSelf); Assert.IsFalse(drawer.gameObject.activeSelf);
                    var saved = rig.Head; var savedFiles = CopyFiles(rig.Storage.Files); rig.Localization.SetLocale(LocaleId.En);
                    rig.Driver.Click(rig.Find<Button>(FightMatchViewId.Row("history-close")));
                    Assert.IsTrue(R(rig, NormalPath).gameObject.activeSelf); Assert.AreSame(saved, rig.Head); SameFiles(savedFiles, rig.Storage.Files);
                    yield return null; VisibleCopy(rig);
                }
            }
            yield return new ExitPlayMode(); enteredPlayMode = false;
        }
        private static void Reveal(ScrollRect scroll, RectTransform child)
        {
            Canvas.ForceUpdateCanvases();
            var center = scroll.content.InverseTransformPoint(child.TransformPoint(child.rect.center)).x - scroll.content.rect.xMin;
            scroll.horizontalNormalizedPosition = Mathf.Clamp01((center - scroll.viewport.rect.width / 2) / (scroll.content.rect.width - scroll.viewport.rect.width));
            Canvas.ForceUpdateCanvases();
        }
        [UnityTest]
        public IEnumerator LAYOUT_11_SystemRecoveryPrecedesEveryOrdinaryPopup()
        {
            EmptyScene(); yield return new EnterPlayMode(); enteredPlayMode = true;
            using (var size = new OverlayResolution(540, 960))
            {
                yield return size.Ready(540, 960);
                using (var rig = OverlayRig(new Rect(16, 16, 508, 920), 540, 960))
                {
                    rig.Storage.Fault = "snapshot-before"; rig.BeginRoute(); rig.EndRoute();
                    foreach (var name in new[] { "LanguagePopup", "LicensePopup", "QuitPopup", "ConfirmationPopup" }) R(rig, "PopupLayer/" + name).gameObject.SetActive(true);
                    rig.View.SynchronizeOverlayRoots(); Canvas.ForceUpdateCanvases();
                    Assert.IsTrue(R(rig, RecoveryPath).gameObject.activeInHierarchy);
                    foreach (var name in new[] { "LanguagePopup", "LicensePopup", "QuitPopup", "ConfirmationPopup" })
                        Assert.IsFalse(R(rig, "PopupLayer/" + name).gameObject.activeSelf, name);
                    var head = rig.Head; var calls = rig.Storage.SnapshotCreates;
                    var hits = rig.Driver.Raycast(PixelCenter(rig.Board.rectTransform)); Assert.IsNotEmpty(hits);
                    Assert.IsTrue(hits[0].gameObject.transform.IsChildOf(R(rig, RecoveryPath)));
                    rig.Driver.Down(PixelCenter(rig.Board.rectTransform)); rig.Driver.Up(PixelCenter(rig.Board.rectTransform));
                    Assert.AreSame(head, rig.Head); Assert.AreEqual(calls, rig.Storage.SnapshotCreates);
                    var navigation = R(rig, RecoveryPath + "/RootMask/NavigationRecoveryPanel"); navigation.gameObject.SetActive(true);
                    rig.View.SynchronizeOverlayRoots(); Assert.IsFalse(R(rig, RecoveryPath).gameObject.activeSelf);
                    Assert.AreEqual("OverlappingPanelOwners", rig.View.DiagnosticCode); Assert.IsTrue(rig.View.DiagnosticVisible);
                    navigation.gameObject.SetActive(false); rig.View.SynchronizeOverlayRoots(); Assert.IsNull(rig.View.DiagnosticCode);
                    Assert.IsTrue(R(rig, RecoveryPath).gameObject.activeSelf);
                    rig.View.Unbind(); Assert.IsFalse(R(rig, RecoveryPath).gameObject.activeSelf); Assert.IsFalse(R(rig, ConfirmationPath).gameObject.activeSelf);
                    rig.Bind(); yield return null; rig.View.SynchronizeOverlayRoots(); Assert.IsTrue(R(rig, RecoveryPath).gameObject.activeSelf);
                }
            }
            yield return new ExitPlayMode(); enteredPlayMode = false;
        }
        [UnityTest]
        public IEnumerator LAYOUT_12_RaycastTargetsExistOnlyOnInteractiveSurfaces()
        {
            EmptyScene(); yield return new EnterPlayMode(); enteredPlayMode = true;
            using (var size = new OverlayResolution(540, 960))
            {
                yield return size.Ready(540, 960);
                using (var rig = OverlayRig(new Rect(16, 16, 508, 920), 540, 960))
                {
                    foreach (var graphic in rig.Root.GetComponentsInChildren<Graphic>(true))
                    {
                        if (graphic is TMP_Text || graphic.name.StartsWith("TempArt_", StringComparison.Ordinal)) Assert.IsFalse(graphic.raycastTarget, graphic.name);
                        if (!graphic.raycastTarget) continue;
                        var selectable = graphic.GetComponentInParent<Selectable>(true);
                        var allowed = graphic == rig.Board || selectable != null && selectable.targetGraphic == graphic ||
                            graphic.GetComponent<Mask>() != null || graphic.name == "RootMask" ||
                            new[] { "LoadingOverlay", "LayoutDiagnostic", "BlockingDiagnostic" }.Contains(graphic.name);
                        Assert.IsTrue(allowed, Relative(rig.Root.transform, graphic.transform));
                    }
                    var scroll = R(rig, CommandPath).GetComponent<ScrollRect>(); scroll.horizontalNormalizedPosition = 0; Canvas.ForceUpdateCanvases();
                    var head = rig.Head; var files = CopyFiles(rig.Storage.Files); var start = PixelCenter(scroll.viewport);
                    rig.Driver.Down(start); rig.Driver.Move(start + new Vector2(-120, 0)); rig.Driver.Up(start + new Vector2(-120, 0));
                    Assert.Greater(scroll.horizontalNormalizedPosition, 0); Assert.IsFalse(rig.Board.HasActivePointer);
                    Assert.IsNull(rig.Session.Battle.Input.LastRequest); Assert.AreSame(head, rig.Head); SameFiles(files, rig.Storage.Files);
                    var hits = rig.Driver.Raycast(UguiPointerDriver.Point(rig.Board, rig.Route()[0]));
                    Assert.IsNotEmpty(hits); Assert.AreEqual(rig.Board.gameObject, hits[0].gameObject); yield return null; VisibleCopy(rig);
                }
            }
            yield return new ExitPlayMode(); enteredPlayMode = false;
        }
        private static string Relative(Transform root, Transform child)
        {
            var parts = new List<string>();
            for (var current = child; current != root; current = current.parent)
            { Assert.IsNotNull(current, "Target outside SafeAreaRoot"); parts.Insert(0, current.name); }
            return string.Join("/", parts);
        }
    }
}
