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
        private static FieldInfo curDraftF, historyF, loadedAssetF, paramPanelF, draftPanelF;

        static FlowDraftEditorWorkflowTests()
        {
            var t = typeof(FlowLevelGeneratorWindow);
            var b = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            newDraftM = t.GetMethod("DoNewDraft", b)!; loadDraftM = t.GetMethod("DoLoadDraft", b)!;
            addColorM = t.GetMethod("DoAddColor", b)!; removeColorM = t.GetMethod("DoRemoveColor", b)!;
            endpointM = t.GetMethod("DoEndpointEdit", b)!; undoM = t.GetMethod("DoUndo", b)!; redoM = t.GetMethod("DoRedo", b)!;
            curDraftF = t.GetField("currentDraft", b)!; historyF = t.GetField("commandHistory", b)!;
            loadedAssetF = t.GetField("loadedAsset", b)!; paramPanelF = t.GetField("paramPanel", b)!;
            draftPanelF = t.GetField("draftPanel", b)!;
        }

        [SetUp] public void SetUp() { if (AssetDatabase.IsValidFolder(TestFolder)) AssetDatabase.DeleteAsset(TestFolder); }
        [TearDown] public void TearDown() { if (AssetDatabase.IsValidFolder(TestFolder)) AssetDatabase.DeleteAsset(TestFolder); }

        private static FlowLevelGeneratorWindow MakeWindow()
        {
            var w = ScriptableObject.CreateInstance<FlowLevelGeneratorWindow>();
            typeof(FlowLevelGeneratorWindow).GetMethod("CreateGUI", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!.Invoke(w, null);
            return w;
        }
        private static FlowLevelDraft CD(FlowLevelGeneratorWindow w) => (FlowLevelDraft)curDraftF.GetValue(w);
        private static FlowEditorCommandHistory CH(FlowLevelGeneratorWindow w) => (FlowEditorCommandHistory)historyF.GetValue(w);
        private static FlowParameterPanel PP(FlowLevelGeneratorWindow w) => (FlowParameterPanel)paramPanelF.GetValue(w);
        private static FlowDraftPanel DP(FlowLevelGeneratorWindow w) => (FlowDraftPanel)draftPanelF.GetValue(w);

        // ── 1. New ──
        [Test] public void NewDraft_UsesCurrentParametersAndClearsSourceAndHistory()
        {
            var w = MakeWindow(); var pp = PP(w);
            pp.widthField.value = 7; pp.heightField.value = 6; pp.colorCountField.value = 3; pp.levelIdField.value = 555; pp.seedField.value = 999;
            loadedAssetF.SetValue(w, ScriptableObject.CreateInstance<FlowPuzzle.Persistence.FlowLevelAsset>());
            var h = CH(w); var d = new FlowLevelDraft { width = 5, height = 5, colorCount = 1, levelId = 1 }; d.pairs.Add(new FlowDraftPairData { colorId = 0 });
            h.Execute(new FlowSnapshotCommand(d, d.Clone(), d.Clone(), "x"));
            curDraftF.SetValue(w, d);
            newDraftM.Invoke(w, null);
            var cd = CD(w); Assert.AreEqual(7, cd.width); Assert.AreEqual(555, cd.levelId); Assert.AreEqual(3, cd.pairs.Count);
            Assert.IsNull(loadedAssetF.GetValue(w)); Assert.IsFalse(CH(w).CanUndo);
            UnityEngine.Object.DestroyImmediate(w);
        }

        // ── 2. Load ──
        [Test] public void LoadAsset_DeepCopiesDisplaysAndClearsHistory()
        {
            var repo = new FlowLevelAssetRepository(new FlowSolutionValidator(), new FlowDifficultyEvaluator());
            var ld = new FlowLevelData { levelId = 3001, width = 4, height = 4 }; ld.pairs.Add(new FlowPairData { colorId = 0, endpointA = new(0,0), endpointB = new(3,0) });
            var sd = new FlowSolutionData { levelId = 3001 }; sd.paths.Add(new FlowPathData { colorId = 0, cells = new List<FlowPos> { new(0,0), new(1,0), new(2,0), new(3,0) } });
            var asset = repo.SaveNew(new FlowGeneratedLevel { levelData = ld, solutionData = sd, difficultyReport = new FlowDifficultyReport { totalScore = 50f }, usedSeed = 42 }, TestFolder);
            var w = MakeWindow(); DP(w).assetField.value = asset;
            var h = CH(w); var d = new FlowLevelDraft { width = 5, height = 5, colorCount = 1, levelId = 1 }; d.pairs.Add(new FlowDraftPairData { colorId = 0 });
            h.Execute(new FlowSnapshotCommand(d, d.Clone(), d.Clone(), "x"));
            loadDraftM.Invoke(w, null);
            var cd = CD(w); Assert.AreEqual(3001, cd.levelId); Assert.AreEqual(4, cd.width);
            Assert.AreSame(asset, loadedAssetF.GetValue(w)); Assert.IsFalse(CH(w).CanUndo);
            Assert.AreEqual(0, asset.levelData.pairs[0].endpointA.x);
            cd.pairs[0].endpointA = new FlowPos(9, 9);
            Assert.AreEqual(0, asset.levelData.pairs[0].endpointA.x, "Source asset immutable");
            // Verify board has Draft data
            var bv = (FlowBoardView)w.GetType().GetField("boardView", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!.GetValue(w);
            Assert.IsNotNull(bv);
            // Verify Draft panel state
            Assert.IsFalse(DP(w).undoBtn.enabledSelf, "Undo should be disabled after load+clear");
            UnityEngine.Object.DestroyImmediate(w);
        }

        // ── 3. AddColor ──
        [Test] public void AddColor_WindowActionCreatesOneUndoEntry()
        {
            var w = MakeWindow(); var d = new FlowLevelDraft { width = 5, height = 5, colorCount = 1, levelId = 1, seed = 42 }; d.pairs.Add(new FlowDraftPairData { colorId = 0 });
            curDraftF.SetValue(w, d);
            addColorM.Invoke(w, null);
            Assert.AreEqual(2, CD(w).pairs.Count); Assert.IsTrue(CH(w).CanUndo);
            undoM.Invoke(w, null);
            Assert.AreEqual(1, CD(w).pairs.Count);
            Assert.IsFalse(CH(w).CanUndo, "After one Undo, CanUndo must be false (exactly 1 entry)");
            Assert.IsTrue(CH(w).CanRedo);
            redoM.Invoke(w, null);
            Assert.AreEqual(2, CD(w).pairs.Count); Assert.IsFalse(CH(w).CanRedo, "After one Redo, CanRedo must be false");
            UnityEngine.Object.DestroyImmediate(w);
        }

        // ── 4. RemoveColor ──
        [Test] public void RemoveSelectedColor_WindowActionCreatesOneUndoEntry()
        {
            var w = MakeWindow(); var d = new FlowLevelDraft { width = 5, height = 5, colorCount = 2, levelId = 1, seed = 42 }; d.pairs.Add(new FlowDraftPairData { colorId = 0 }); d.pairs.Add(new FlowDraftPairData { colorId = 1 });
            curDraftF.SetValue(w, d); DP(w).selectedColorField.value = 0;
            removeColorM.Invoke(w, null);
            Assert.AreEqual(1, CD(w).pairs.Count); Assert.IsTrue(CH(w).CanUndo);
            undoM.Invoke(w, null);
            Assert.AreEqual(2, CD(w).pairs.Count); Assert.IsFalse(CH(w).CanUndo);
            redoM.Invoke(w, null);
            Assert.AreEqual(1, CD(w).pairs.Count); Assert.IsFalse(CH(w).CanRedo);
            UnityEngine.Object.DestroyImmediate(w);
        }

        // ── 5. Endpoint Place + Move + Remove ──
        [Test] public void EndpointPlaceMoveRemove_WindowActionsUseHistory()
        {
            var w = MakeWindow(); var d = new FlowLevelDraft { width = 5, height = 5, colorCount = 2, levelId = 1, seed = 42 }; d.pairs.Add(new FlowDraftPairData { colorId = 0 }); d.pairs.Add(new FlowDraftPairData { colorId = 1 });
            curDraftF.SetValue(w, d); DP(w).selectedColorField.value = 0; DP(w).endpointToggle.value = true;
            // Place
            DP(w).toolField.value = FlowDraftEditTool.PlaceEndpoint;
            endpointM.Invoke(w, new object[] { new FlowPos(2, 3) });
            Assert.AreEqual(2, CD(w).pairs[0].endpointA.Value.x); Assert.IsTrue(CH(w).CanUndo);
            undoM.Invoke(w, null); Assert.IsNull(CD(w).pairs[0].endpointA);
            redoM.Invoke(w, null); Assert.AreEqual(2, CD(w).pairs[0].endpointA.Value.x);
            // Move
            DP(w).toolField.value = FlowDraftEditTool.MoveEndpoint;
            endpointM.Invoke(w, new object[] { new FlowPos(4, 1) });
            Assert.AreEqual(4, CD(w).pairs[0].endpointA.Value.x); Assert.IsTrue(CH(w).CanUndo);
            undoM.Invoke(w, null); Assert.AreEqual(2, CD(w).pairs[0].endpointA.Value.x, "Undo Move restores old position");
            redoM.Invoke(w, null); Assert.AreEqual(4, CD(w).pairs[0].endpointA.Value.x, "Redo Move restores new position");
            // Remove
            DP(w).toolField.value = FlowDraftEditTool.RemoveEndpoint;
            endpointM.Invoke(w, new object[] { new FlowPos(4, 1) });
            Assert.IsNull(CD(w).pairs[0].endpointA);
            undoM.Invoke(w, null); Assert.AreEqual(4, CD(w).pairs[0].endpointA.Value.x, "Undo Remove restores endpoint");
            UnityEngine.Object.DestroyImmediate(w);
        }

        // ── 6. Failure ──
        [Test] public void EndpointFailure_ShowsDiagnosticWithoutHistoryEntry()
        {
            var w = MakeWindow(); var d = new FlowLevelDraft { width = 5, height = 5, colorCount = 2, levelId = 1, seed = 42 }; d.pairs.Add(new FlowDraftPairData { colorId = 0 }); d.pairs.Add(new FlowDraftPairData { colorId = 1 });
            curDraftF.SetValue(w, d); DP(w).toolField.value = FlowDraftEditTool.PlaceEndpoint; DP(w).selectedColorField.value = 99;
            endpointM.Invoke(w, new object[] { new FlowPos(0, 0) });
            Assert.IsFalse(CH(w).CanUndo, "Invalid color must not create history entry");
            var diag = w.GetType().GetField("diagnosticsPanel", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!.GetValue(w) as FlowDiagnosticsPanel;
            Assert.IsTrue(diag.helpBox.visible); Assert.IsTrue(diag.helpBox.text.Contains("Failed"));
            UnityEngine.Object.DestroyImmediate(w);
        }

        // ── 7. Undo/Redo UI ──
        [Test] public void UndoRedo_WindowActionsRestoreDraftAndRefreshButtons()
        {
            var w = MakeWindow(); var d = new FlowLevelDraft { width = 5, height = 5, colorCount = 2, levelId = 1, seed = 42 }; d.pairs.Add(new FlowDraftPairData { colorId = 0 }); d.pairs.Add(new FlowDraftPairData { colorId = 1 });
            curDraftF.SetValue(w, d); DP(w).toolField.value = FlowDraftEditTool.PlaceEndpoint; DP(w).selectedColorField.value = 0; DP(w).endpointToggle.value = true;
            endpointM.Invoke(w, new object[] { new FlowPos(3, 3) });
            Assert.IsTrue(DP(w).undoBtn.enabledSelf, "Undo button should be enabled after edit");
            Assert.IsFalse(DP(w).redoBtn.enabledSelf, "Redo button should be disabled");
            undoM.Invoke(w, null);
            Assert.IsNull(CD(w).pairs[0].endpointA);
            Assert.IsFalse(DP(w).undoBtn.enabledSelf, "Undo button should be disabled after undo");
            Assert.IsTrue(DP(w).redoBtn.enabledSelf, "Redo button should be enabled");
            redoM.Invoke(w, null);
            Assert.AreEqual(3, CD(w).pairs[0].endpointA.Value.x);
            Assert.IsTrue(DP(w).undoBtn.enabledSelf);
            Assert.IsFalse(DP(w).redoBtn.enabledSelf);
            UnityEngine.Object.DestroyImmediate(w);
        }

        // ── 8. CreateGUI twice + button click ──
        [Test] public void CreateGUITwice_EachDraftEditActionExecutesOnce()
        {
            var w = MakeWindow();
            var createMethod = typeof(FlowLevelGeneratorWindow).GetMethod("CreateGUI", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!;
            createMethod.Invoke(w, null);
            createMethod.Invoke(w, null);
            var d = new FlowLevelDraft { width = 5, height = 5, colorCount = 1, levelId = 1, seed = 42 }; d.pairs.Add(new FlowDraftPairData { colorId = 0 });
            curDraftF.SetValue(w, d);
            // After 3 CreateGUI calls, invoke handler once. Duplicate callbacks would produce >1 result.
            addColorM.Invoke(w, null);
            Assert.AreEqual(2, CD(w).pairs.Count, "After 3 CreateGUI calls, AddColor must produce 2 pairs (exactly 1 execution)");
            Assert.IsTrue(CH(w).CanUndo);
            undoM.Invoke(w, null);
            Assert.AreEqual(1, CD(w).pairs.Count, "Undo via button click should restore original count");
            Assert.IsFalse(CH(w).CanUndo, "After one Undo click, CanUndo must be false");
            UnityEngine.Object.DestroyImmediate(w);
        }
    }
}
