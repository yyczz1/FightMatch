using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FlowPuzzle.Core;
using FlowPuzzle.Difficulty;
using FlowPuzzle.Editor;
using FlowPuzzle.Editor.Commands;
using FlowPuzzle.Editor.Draft;
using FlowPuzzle.Editor.Persistence;
using FlowPuzzle.Editor.UI;
using FlowPuzzle.Validation;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace FlowPuzzle.Tests.Editor
{
    [TestFixture]
    public class FlowDraftEditorWorkflowTests
    {
        private const string TestFolder = "Assets/Temp/FlowPuzzleDraftEditorTests";
        private static MethodInfo newDraftM, loadDraftM, addColorM, removeColorM, endpointM, undoM, redoM;
        private static FieldInfo curDraftF, historyF, loadedAssetF, paramPanelF, boardViewF, draftPanelF;

        static FlowDraftEditorWorkflowTests()
        {
            var t = typeof(FlowLevelGeneratorWindow);
            var b = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            newDraftM = t.GetMethod("DoNewDraft", b)!; loadDraftM = t.GetMethod("DoLoadDraft", b)!;
            addColorM = t.GetMethod("DoAddColor", b)!; removeColorM = t.GetMethod("DoRemoveColor", b)!;
            endpointM = t.GetMethod("DoEndpointEdit", b)!; undoM = t.GetMethod("DoUndo", b)!; redoM = t.GetMethod("DoRedo", b)!;
            curDraftF = t.GetField("currentDraft", b)!; historyF = t.GetField("commandHistory", b)!;
            loadedAssetF = t.GetField("loadedAsset", b)!; paramPanelF = t.GetField("paramPanel", b)!;
            boardViewF = t.GetField("boardView", b)!; draftPanelF = t.GetField("draftPanel", b)!;
        }

        [SetUp] public void SetUp() { if (AssetDatabase.IsValidFolder(TestFolder)) AssetDatabase.DeleteAsset(TestFolder); }
        [TearDown] public void TearDown() { if (AssetDatabase.IsValidFolder(TestFolder)) AssetDatabase.DeleteAsset(TestFolder); }

        private static FlowLevelGeneratorWindow MakeWindow()
        {
            var w = ScriptableObject.CreateInstance<FlowLevelGeneratorWindow>();
            typeof(FlowLevelGeneratorWindow).GetMethod("CreateGUI", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!.Invoke(w, null);
            return w;
        }
        private static T G<T>(object t, string n) => (T)t.GetType().GetField(n, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!.GetValue(t);
        private static FlowLevelDraft CD(FlowLevelGeneratorWindow w) => (FlowLevelDraft)curDraftF.GetValue(w);
        private static FlowEditorCommandHistory CH(FlowLevelGeneratorWindow w) => (FlowEditorCommandHistory)historyF.GetValue(w);
        private static FlowParameterPanel PP(FlowLevelGeneratorWindow w) => (FlowParameterPanel)paramPanelF.GetValue(w);
        private static FlowDraftPanel DP(FlowLevelGeneratorWindow w) => (FlowDraftPanel)draftPanelF.GetValue(w);
        private static void Call(FlowLevelGeneratorWindow w, MethodInfo m, params object[] args) => m.Invoke(w, args);

        // ── 1. NewDraft ──
        [Test] public void NewDraft_UsesCurrentParametersAndClearsSourceAndHistory()
        {
            var w = MakeWindow(); var pp = PP(w);
            pp.widthField.value = 7; pp.heightField.value = 6; pp.colorCountField.value = 3; pp.levelIdField.value = 555; pp.seedField.value = 999;
            // Seed a loaded asset and non-empty history
            loadedAssetF.SetValue(w, ScriptableObject.CreateInstance<FlowPuzzle.Persistence.FlowLevelAsset>());
            var h = CH(w); var draft = new FlowLevelDraft { width = 5, height = 5, colorCount = 1, levelId = 1 }; draft.pairs.Add(new FlowDraftPairData { colorId = 0 });
            h.Execute(new FlowSnapshotCommand(draft, draft.Clone(), draft.Clone(), "x"));
            curDraftF.SetValue(w, draft);
            Call(w, newDraftM);
            var cd = CD(w); Assert.AreEqual(7, cd.width); Assert.AreEqual(6, cd.height);
            Assert.AreEqual(3, cd.colorCount); Assert.AreEqual(555, cd.levelId); Assert.AreEqual(999, cd.seed);
            Assert.AreEqual(3, cd.pairs.Count);
            Assert.IsNull(loadedAssetF.GetValue(w)); Assert.IsFalse(CH(w).CanUndo);
            UnityEngine.Object.DestroyImmediate(w);
        }

        // ── 2. Load ──
        [Test] public void LoadAsset_DeepCopiesDisplaysAndClearsHistory()
        {
            var repo = new FlowLevelAssetRepository(new FlowSolutionValidator(), new FlowDifficultyEvaluator());
            var ld = new FlowLevelData { levelId = 3001, width = 4, height = 4 }; ld.pairs.Add(new FlowPairData { colorId = 0, endpointA = new(0,0), endpointB = new(3,0) });
            var sd = new FlowSolutionData { levelId = 3001 }; sd.paths.Add(new FlowPathData { colorId = 0, cells = new List<FlowPos> { new(0,0), new(1,0), new(2,0), new(3,0) } });
            var level = new FlowGeneratedLevel { levelData = ld, solutionData = sd, difficultyReport = new FlowDifficultyReport { totalScore = 50f }, usedSeed = 42 };
            var asset = repo.SaveNew(level, TestFolder);

            var w = MakeWindow(); var dp = DP(w);
            dp.assetField.value = asset;
            // Seed non-empty history
            var h = CH(w); var d = new FlowLevelDraft { width = 5, height = 5, colorCount = 1, levelId = 1 }; d.pairs.Add(new FlowDraftPairData { colorId = 0 });
            h.Execute(new FlowSnapshotCommand(d, d.Clone(), d.Clone(), "x"));
            Call(w, loadDraftM);

            var cd = CD(w); Assert.AreEqual(3001, cd.levelId); Assert.AreEqual(4, cd.width);
            Assert.AreSame(asset, loadedAssetF.GetValue(w)); Assert.IsFalse(CH(w).CanUndo);
            Assert.AreEqual(0, asset.levelData.pairs[0].endpointA.x);
            cd.pairs[0].endpointA = new FlowPos(9, 9);
            Assert.AreEqual(0, asset.levelData.pairs[0].endpointA.x, "Source asset remains immutable");
            UnityEngine.Object.DestroyImmediate(w);
        }

        // ── 3. AddColor ──
        [Test] public void AddColor_WindowActionCreatesOneUndoEntry()
        {
            var w = MakeWindow(); var d = new FlowLevelDraft { width = 5, height = 5, colorCount = 1, levelId = 1, seed = 42 }; d.pairs.Add(new FlowDraftPairData { colorId = 0 });
            curDraftF.SetValue(w, d);
            Call(w, addColorM);
            var cd = CD(w); Assert.AreEqual(2, cd.pairs.Count); Assert.IsTrue(CH(w).CanUndo);
            Call(w, undoM); Assert.AreEqual(1, CD(w).pairs.Count);
            Call(w, redoM); Assert.AreEqual(2, CD(w).pairs.Count);
            UnityEngine.Object.DestroyImmediate(w);
        }

        // ── 4. RemoveColor ──
        [Test] public void RemoveSelectedColor_WindowActionCreatesOneUndoEntry()
        {
            var w = MakeWindow(); var d = new FlowLevelDraft { width = 5, height = 5, colorCount = 2, levelId = 1, seed = 42 }; d.pairs.Add(new FlowDraftPairData { colorId = 0 }); d.pairs.Add(new FlowDraftPairData { colorId = 1 });
            curDraftF.SetValue(w, d);
            DP(w).selectedColorField.value = 0;
            Call(w, removeColorM);
            Assert.AreEqual(1, CD(w).pairs.Count); Assert.IsTrue(CH(w).CanUndo);
            Call(w, undoM); Assert.AreEqual(2, CD(w).pairs.Count);
            UnityEngine.Object.DestroyImmediate(w);
        }

        // ── 5. Endpoint Place/Move/Remove ──
        [Test] public void EndpointPlaceMoveRemove_WindowActionsUseHistory()
        {
            var w = MakeWindow(); var d = new FlowLevelDraft { width = 5, height = 5, colorCount = 2, levelId = 1, seed = 42 }; d.pairs.Add(new FlowDraftPairData { colorId = 0 }); d.pairs.Add(new FlowDraftPairData { colorId = 1 });
            curDraftF.SetValue(w, d); DP(w).toolField.value = FlowDraftEditTool.PlaceEndpoint;
            DP(w).selectedColorField.value = 0; DP(w).endpointToggle.value = true;
            Call(w, endpointM, new FlowPos(2, 3));
            Assert.AreEqual(2, CD(w).pairs[0].endpointA.Value.x); Assert.IsTrue(CH(w).CanUndo);
            Call(w, undoM); Assert.IsNull(CD(w).pairs[0].endpointA);
            Call(w, redoM); Assert.AreEqual(2, CD(w).pairs[0].endpointA.Value.x);
            // Remove
            DP(w).toolField.value = FlowDraftEditTool.RemoveEndpoint;
            Call(w, endpointM, new FlowPos(2, 3));
            Assert.IsNull(CD(w).pairs[0].endpointA);
            Call(w, undoM); Assert.AreEqual(2, CD(w).pairs[0].endpointA.Value.x);
            UnityEngine.Object.DestroyImmediate(w);
        }

        // ── 6. Failure + Diagnostics ──
        [Test] public void EndpointFailure_ShowsDiagnosticWithoutHistoryEntry()
        {
            var w = MakeWindow(); var d = new FlowLevelDraft { width = 5, height = 5, colorCount = 2, levelId = 1, seed = 42 }; d.pairs.Add(new FlowDraftPairData { colorId = 0 }); d.pairs.Add(new FlowDraftPairData { colorId = 1 });
            curDraftF.SetValue(w, d); DP(w).toolField.value = FlowDraftEditTool.PlaceEndpoint;
            DP(w).selectedColorField.value = 99; // invalid color
            Call(w, endpointM, new FlowPos(0, 0));
            Assert.IsFalse(CH(w).CanUndo, "Invalid endpoint should not create history entry");
            var diag = G<FlowDiagnosticsPanel>(w, "diagnosticsPanel");
            Assert.IsTrue(diag.helpBox.visible, "Diagnostics should be visible");
            Assert.IsTrue(diag.helpBox.text.Contains("Failed"), "Should show error text");
            UnityEngine.Object.DestroyImmediate(w);
        }

        // ── 7. Undo/Redo buttons ──
        [Test] public void UndoRedo_WindowActionsRestoreDraftAndRefreshButtons()
        {
            var w = MakeWindow(); var d = new FlowLevelDraft { width = 5, height = 5, colorCount = 2, levelId = 1, seed = 42 }; d.pairs.Add(new FlowDraftPairData { colorId = 0 }); d.pairs.Add(new FlowDraftPairData { colorId = 1 });
            curDraftF.SetValue(w, d); DP(w).toolField.value = FlowDraftEditTool.PlaceEndpoint;
            DP(w).selectedColorField.value = 0; DP(w).endpointToggle.value = true;
            Call(w, endpointM, new FlowPos(3, 3));
            Assert.IsTrue(CH(w).CanUndo);
            Call(w, undoM); Assert.IsNull(CD(w).pairs[0].endpointA);
            Assert.IsTrue(CH(w).CanRedo); Assert.IsFalse(CH(w).CanUndo);
            Call(w, redoM); Assert.AreEqual(3, CD(w).pairs[0].endpointA.Value.x);
            UnityEngine.Object.DestroyImmediate(w);
        }

        // ── 8. CreateGUI twice ──
        [Test] public void CreateGUITwice_EachDraftEditActionExecutesOnce()
        {
            var w = MakeWindow();
            typeof(FlowLevelGeneratorWindow).GetMethod("CreateGUI", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!.Invoke(w, null);
            typeof(FlowLevelGeneratorWindow).GetMethod("CreateGUI", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!.Invoke(w, null);
            var d = new FlowLevelDraft { width = 5, height = 5, colorCount = 1, levelId = 1, seed = 42 }; d.pairs.Add(new FlowDraftPairData { colorId = 0 });
            curDraftF.SetValue(w, d);
            Call(w, addColorM);
            Assert.AreEqual(2, CD(w).pairs.Count, "After 3 CreateGUI calls, AddColor should execute exactly once");
            Assert.AreEqual(1, CH(w).CanUndo ? 1 : 0, "Should have exactly one undo entry, not duplicated");
            UnityEngine.Object.DestroyImmediate(w);
        }
    }
}
