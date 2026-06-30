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

        private bool tempExisted;

        [SetUp] public void SetUp()
        {
            tempExisted = AssetDatabase.IsValidFolder("Assets/Temp");
            if (AssetDatabase.IsValidFolder(TestFolder)) AssetDatabase.DeleteAsset(TestFolder);
        }
        [TearDown] public void TearDown()
        {
            if (AssetDatabase.IsValidFolder(TestFolder)) AssetDatabase.DeleteAsset(TestFolder);
            if (!tempExisted && AssetDatabase.IsValidFolder("Assets/Temp")
                && AssetDatabase.GetSubFolders("Assets/Temp").Length == 0
                && System.IO.Directory.GetFiles("Assets/Temp").Length == 0)
                AssetDatabase.DeleteAsset("Assets/Temp");
        }

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
            // Verify displayed level data via internal snapshot
            var bv = (FlowBoardView)w.GetType().GetField("boardView", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!.GetValue(w);
            var lvlField = bv.GetType().GetField("levelData", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            var displayLevel = (FlowLevelData)lvlField!.GetValue(bv);
            Assert.IsNotNull(displayLevel, "Board should have displayed level data");
            Assert.AreEqual(4, displayLevel.width); Assert.AreEqual(4, displayLevel.height);
            Assert.AreEqual(1, displayLevel.pairs.Count);
            Assert.AreEqual(0, displayLevel.pairs[0].colorId);
            Assert.AreEqual(0, displayLevel.pairs[0].endpointA.x); Assert.AreEqual(0, displayLevel.pairs[0].endpointA.y);
            Assert.AreEqual(3, displayLevel.pairs[0].endpointB.x); Assert.AreEqual(0, displayLevel.pairs[0].endpointB.y);
            // Verify Draft panel state
            Assert.IsFalse(DP(w).undoBtn.enabledSelf, "Undo should be disabled after load+clear");
            Assert.IsFalse(DP(w).redoBtn.enabledSelf, "Redo should be disabled after load+clear");
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

        // ── 8. CreateGUI twice + real button clicks ──
        [Test] public void CreateGUITwice_EachDraftEditActionExecutesOnce()
        {
            var w = MakeWindow();
            var createMethod = typeof(FlowLevelGeneratorWindow).GetMethod("CreateGUI", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!;
            createMethod.Invoke(w, null); // 2nd
            createMethod.Invoke(w, null); // 3rd
            var d = new FlowLevelDraft { width = 5, height = 5, colorCount = 1, levelId = 1, seed = 42 }; d.pairs.Add(new FlowDraftPairData { colorId = 0 });
            curDraftF.SetValue(w, d);

            // Invoke the actual addColorBtn.clickable callback via Clickable.Invoke (Unity internal)
            var clickInvoke = typeof(Clickable).GetMethod("Invoke", BindingFlags.Instance | BindingFlags.NonPublic)!;
            clickInvoke.Invoke(DP(w).addColorBtn.clickable, new object[] { null });
            Assert.AreEqual(2, CD(w).pairs.Count, "After 3 CreateGUI, addColorBtn click must produce exactly 2 pairs (no duplicate callbacks)");
            Assert.IsTrue(CH(w).CanUndo);

            // Invoke undoBtn.clickable
            clickInvoke.Invoke(DP(w).undoBtn.clickable, new object[] { null });
            Assert.AreEqual(1, CD(w).pairs.Count, "Undo button click must restore original color count");
            Assert.IsFalse(CH(w).CanUndo, "After one Undo click, CanUndo must be false");
            Assert.IsTrue(CH(w).CanRedo, "After one Undo click, CanRedo must be true");
            UnityEngine.Object.DestroyImmediate(w);
        }

        // ── RED A2 tests ──
        private static FlowLevelDraft MakeCompleteDraft(int colors = 1, int levelId = 4001)
        {
            var d = new FlowLevelDraft { width = 5, height = 5, colorCount = colors, levelId = levelId, seed = 42 };
            for (int i = 0; i < colors; i++) d.pairs.Add(new FlowDraftPairData { colorId = i });
            d.PlaceEndpoint(0, true, new FlowPos(0, 0)); d.PlaceEndpoint(0, false, new FlowPos(3, 0));
            d.currentSolution = new FlowSolutionData { levelId = levelId };
            d.currentSolution.paths.Add(new FlowPathData { colorId = 0, cells = new List<FlowPos> { new(0,0), new(1,0), new(2,0), new(3,0) } });
            d.currentDifficulty = new FlowDifficultyReport { totalScore = 50f, difficulty = FlowDifficultyTier.Easy };
            d.isSolutionDirty = false; d.isValidated = true;
            return d;
        }
        private static MethodInfo dSaveM, dSaveAsM, setCLevelM, genOneM;

        static void InitA2()
        {
            if (dSaveM != null) return;
            var t = typeof(FlowLevelGeneratorWindow); var b = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            dSaveM = t.GetMethod("DoSaveDraft", b); dSaveAsM = t.GetMethod("DoSaveDraftAs", b);
            setCLevelM = t.GetMethod("SetCurrentLevel", b); genOneM = t.GetMethod("OnGenerateOne", b);
        }

        [Test] public void DraftSaveButtons_RequireCompleteSolutionDifficultyCleanAndValidated()
        {
            var w = MakeWindow();
            var d = MakeCompleteDraft();
            curDraftF.SetValue(w, d);
            DP(w).UpdateDraftState(d, CH(w));
            Assert.IsTrue(DP(w).saveBtn.enabledSelf, "Save enabled when complete");
            d.isSolutionDirty = true;
            DP(w).UpdateDraftState(d, CH(w));
            Assert.IsFalse(DP(w).saveBtn.enabledSelf, "Save disabled when dirty");
            d.isSolutionDirty = false; d.currentDifficulty = null;
            DP(w).UpdateDraftState(d, CH(w));
            Assert.IsFalse(DP(w).saveBtn.enabledSelf, "Save disabled without difficulty");
            UnityEngine.Object.DestroyImmediate(w);
        }

        [Test] public void DraftSaveHandlers_InvalidStatesCreateNoAssetAndShowDiagnostic()
        {
            InitA2();
            var w = MakeWindow(); var d = MakeCompleteDraft();
            d.isSolutionDirty = true; curDraftF.SetValue(w, d);
            loadedAssetF.SetValue(w, null);
            var beforeGuids = AssetDatabase.FindAssets("t:FlowLevelAsset", new[] { TestFolder });
            dSaveM!.Invoke(w, null);
            var afterGuids = AssetDatabase.FindAssets("t:FlowLevelAsset", new[] { TestFolder });
            Assert.AreEqual(beforeGuids.Length, afterGuids.Length, "No asset should be created for invalid draft");
            var diag = w.GetType().GetField("diagnosticsPanel", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!.GetValue(w) as FlowDiagnosticsPanel;
            Assert.IsTrue(diag.helpBox.visible, "Diagnostic should be visible");
            UnityEngine.Object.DestroyImmediate(w);
        }

        [Test] public void SaveDraft_SourceLessCreatesAndTracksAsset()
        {
            InitA2();
            var w = MakeWindow(); var d = MakeCompleteDraft();
            curDraftF.SetValue(w, d); loadedAssetF.SetValue(w, null);
            PP(w).outputFolderField.value = TestFolder;
            dSaveM!.Invoke(w, null);
            var guids = AssetDatabase.FindAssets("t:FlowLevelAsset", new[] { TestFolder });
            Assert.AreEqual(1, guids.Length);
            var asset = AssetDatabase.LoadAssetAtPath<FlowPuzzle.Persistence.FlowLevelAsset>(AssetDatabase.GUIDToAssetPath(guids[0]));
            Assert.AreEqual(4001, asset.levelData.levelId);
            Assert.AreSame(asset, loadedAssetF.GetValue(w), "loadedAsset must track created asset");
            UnityEngine.Object.DestroyImmediate(w);
        }

        [Test] public void SaveDraft_LoadedOverwritesSameAssetWithoutCreatingAnother()
        {
            InitA2();
            var repo = new FlowLevelAssetRepository(new FlowSolutionValidator(), new FlowDifficultyEvaluator());
            var srcDraft = MakeCompleteDraft(levelId: 5001);
            var srcLevel = FlowDraftMapper.ToGeneratedLevel(srcDraft);
            var srcAsset = repo.SaveNew(srcLevel, TestFolder);
            var srcPath = AssetDatabase.GetAssetPath(srcAsset);
            Assert.AreEqual(1, AssetDatabase.FindAssets("t:FlowLevelAsset", new[] { TestFolder }).Length);

            var w = MakeWindow(); var d = MakeCompleteDraft(levelId: 5001);
            d.pairs[0].endpointA = new FlowPos(1, 0); d.currentSolution.paths[0].cells = new List<FlowPos> { new(1,0), new(2,0), new(3,0) };
            curDraftF.SetValue(w, d); loadedAssetF.SetValue(w, srcAsset);
            PP(w).outputFolderField.value = TestFolder;
            dSaveM!.Invoke(w, null);
            var guids = AssetDatabase.FindAssets("t:FlowLevelAsset", new[] { TestFolder });
            Assert.AreEqual(1, guids.Length, "Overwrite must not create new asset");
            var reloaded = AssetDatabase.LoadAssetAtPath<FlowPuzzle.Persistence.FlowLevelAsset>(srcPath);
            Assert.AreEqual(1, reloaded.levelData.pairs[0].endpointA.x, "Asset must contain updated draft data");
            Assert.AreSame(srcAsset, loadedAssetF.GetValue(w), "loadedAsset identity preserved");
            UnityEngine.Object.DestroyImmediate(w);
        }

        [Test] public void SaveAs_SourceLessCreatesNamedAssetAndTracksIt()
        {
            InitA2();
            var w = MakeWindow(); var d = MakeCompleteDraft();
            curDraftF.SetValue(w, d); loadedAssetF.SetValue(w, null);
            DP(w).saveAsNameField.value = "RenamedDraft";
            PP(w).outputFolderField.value = TestFolder;
            dSaveAsM!.Invoke(w, null);
            var guids = AssetDatabase.FindAssets("t:FlowLevelAsset", new[] { TestFolder });
            Assert.AreEqual(1, guids.Length);
            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            Assert.IsTrue(path.EndsWith("RenamedDraft.asset"));
            var asset = AssetDatabase.LoadAssetAtPath<FlowPuzzle.Persistence.FlowLevelAsset>(path);
            Assert.AreSame(asset, loadedAssetF.GetValue(w), "loadedAsset tracks SaveAs result");
            UnityEngine.Object.DestroyImmediate(w);
        }

        [Test] public void SaveAs_LoadedCreatesNewAssetAndPreservesSource()
        {
            InitA2();
            var repo = new FlowLevelAssetRepository(new FlowSolutionValidator(), new FlowDifficultyEvaluator());
            var srcDraft = MakeCompleteDraft(levelId: 6001);
            var srcAsset = repo.SaveNew(FlowDraftMapper.ToGeneratedLevel(srcDraft), TestFolder);
            var srcPath = AssetDatabase.GetAssetPath(srcAsset);
            Assert.AreEqual(1, AssetDatabase.FindAssets("t:FlowLevelAsset", new[] { TestFolder }).Length);

            var w = MakeWindow(); var d = MakeCompleteDraft(levelId: 6001);
            // Modify draft endpoint AND solution to stay consistent
            d.pairs[0].endpointA = new FlowPos(1, 0);
            d.currentSolution.paths[0].cells = new List<FlowPos> { new(1,0), new(2,0), new(3,0) };
            curDraftF.SetValue(w, d); loadedAssetF.SetValue(w, srcAsset);
            DP(w).saveAsNameField.value = "Cloned"; PP(w).outputFolderField.value = TestFolder;
            dSaveAsM!.Invoke(w, null);
            var guids = AssetDatabase.FindAssets("t:FlowLevelAsset", new[] { TestFolder });
            Assert.AreEqual(2, guids.Length, "Save As loaded creates a new second asset");
            var srcReloaded = AssetDatabase.LoadAssetAtPath<FlowPuzzle.Persistence.FlowLevelAsset>(srcPath);
            Assert.AreEqual(0, srcReloaded.levelData.pairs[0].endpointA.x, "Source asset endpointA unchanged");
            Assert.AreNotSame(srcAsset, loadedAssetF.GetValue(w), "loadedAsset must now be the new asset");
            UnityEngine.Object.DestroyImmediate(w);
        }

        [Test] public void GeneratedLevel_BecomesSourceLessEditableDraftAndCanSaveAs()
        {
            InitA2();
            var repo = new FlowLevelAssetRepository(new FlowSolutionValidator(), new FlowDifficultyEvaluator());
            var w = MakeWindow();
            var ld = new FlowLevelData { levelId = 7001, width = 5, height = 5 };
            ld.pairs.Add(new FlowPairData { colorId = 0, endpointA = new(0,0), endpointB = new(4,0) });
            var sd = new FlowSolutionData { levelId = 7001 };
            sd.paths.Add(new FlowPathData { colorId = 0, cells = new List<FlowPos> { new(0,0), new(1,0), new(2,0), new(3,0), new(4,0) } });
            var genLevel = new FlowGeneratedLevel { levelData = ld, solutionData = sd, difficultyReport = new FlowDifficultyReport { totalScore = 50f }, usedSeed = 42, coverageRatio = 0.4f };
            setCLevelM!.Invoke(w, new object[] { genLevel });
            Assert.IsNull(loadedAssetF.GetValue(w), "Generated result must be source-less");
            var cd = CD(w);
            Assert.IsNotNull(cd.currentSolution); Assert.IsNotNull(cd.currentDifficulty);
            Assert.IsFalse(cd.isSolutionDirty); Assert.IsTrue(cd.isValidated);
            Assert.AreEqual(5, cd.width); Assert.AreEqual(5, cd.height);
            Assert.IsTrue(cd.pairs[0].endpointA.HasValue); Assert.IsTrue(cd.pairs[0].endpointB.HasValue);
            Assert.AreEqual(0, cd.pairs[0].endpointA.Value.x); Assert.AreEqual(4, cd.pairs[0].endpointB.Value.x);
            // Deep ownership: mutate Draft — generated source unchanged
            cd.pairs[0].endpointA = new FlowPos(2, 0);
            Assert.AreEqual(0, genLevel.levelData.pairs[0].endpointA.x, "Generated source immutable");
            // Restore valid endpoint before save
            cd.pairs[0].endpointA = new FlowPos(0, 0);
            // Also prove generated source mutation doesn't affect Draft
            genLevel.levelData.pairs[0].endpointA = new FlowPos(9, 9);
            Assert.AreEqual(0, cd.pairs[0].endpointA.Value.x, "Draft unchanged after source mutation");
            // Can save as
            if (!AssetDatabase.IsValidFolder(TestFolder))
            {
                if (!AssetDatabase.IsValidFolder("Assets/Temp"))
                    AssetDatabase.CreateFolder("Assets", "Temp");
                AssetDatabase.CreateFolder("Assets/Temp", "FlowPuzzleDraftEditorTests");
                AssetDatabase.Refresh();
            }
            PP(w).outputFolderField.value = TestFolder;
            DP(w).saveAsNameField.value = "Gen7001"; dSaveAsM!.Invoke(w, null);
            var guids = AssetDatabase.FindAssets("t:FlowLevelAsset", new[] { TestFolder });
            Assert.AreEqual(1, guids.Length);
            UnityEngine.Object.DestroyImmediate(w);
        }

        [Test] public void GenerateOne_AfterLoadedDraftClearsSourceHistoryAndRefreshesDraft()
        {
            InitA2();
            var repo = new FlowLevelAssetRepository(new FlowSolutionValidator(), new FlowDifficultyEvaluator());
            var d = MakeCompleteDraft(levelId: 8001);
            var asset = repo.SaveNew(FlowDraftMapper.ToGeneratedLevel(d), TestFolder);
            var w = MakeWindow();
            curDraftF.SetValue(w, d); loadedAssetF.SetValue(w, asset);
            // Simulate GenerateOne producing a result
            var ld2 = new FlowLevelData { levelId = 9001, width = 5, height = 5 };
            ld2.pairs.Add(new FlowPairData { colorId = 0, endpointA = new(0,0), endpointB = new(4,0) });
            var genLevel = new FlowGeneratedLevel { levelData = ld2, solutionData = new FlowSolutionData { levelId = 9001 }, difficultyReport = new FlowDifficultyReport { totalScore = 50f }, usedSeed = 99 };
            genLevel.solutionData.paths.Add(new FlowPathData { colorId = 0, cells = new List<FlowPos> { new(0,0), new(1,0), new(2,0), new(3,0), new(4,0) } });
            setCLevelM!.Invoke(w, new object[] { genLevel });
            Assert.IsNull(loadedAssetF.GetValue(w), "Generate must clear prior loaded asset");
            Assert.IsFalse(CH(w).CanUndo, "Generate must clear history");
            var cd = CD(w);
            Assert.AreEqual(9001, cd.levelId); Assert.AreEqual(99, cd.seed);
            Assert.IsFalse(cd.isSolutionDirty); Assert.IsTrue(cd.isValidated);
            UnityEngine.Object.DestroyImmediate(w);
        }

        [Test] public void CreateGUITwice_DraftSaveAsCallbackExecutesOnce()
        {
            InitA2();
            var w = MakeWindow();
            var createMethod = typeof(FlowLevelGeneratorWindow).GetMethod("CreateGUI", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!;
            createMethod.Invoke(w, null); createMethod.Invoke(w, null);
            var d = MakeCompleteDraft();
            curDraftF.SetValue(w, d);
            PP(w).outputFolderField.value = TestFolder;
            DP(w).saveAsNameField.value = "DupTest";
            var clickInvoke = typeof(Clickable).GetMethod("Invoke", BindingFlags.Instance | BindingFlags.NonPublic)!;
            // First click — succeeds
            clickInvoke.Invoke(DP(w).saveAsBtn.clickable, new object[] { null });
            var guids1 = AssetDatabase.FindAssets("t:FlowLevelAsset", new[] { TestFolder });
            Assert.AreEqual(1, guids1.Length, "First click creates one asset");
            var diag = w.GetType().GetField("diagnosticsPanel", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!.GetValue(w) as FlowDiagnosticsPanel;
            Assert.IsTrue(diag.helpBox.text.Contains("Saved As"), "First click shows Info");
            // Second click — duplicate path, Error diagnostic
            clickInvoke.Invoke(DP(w).saveAsBtn.clickable, new object[] { null });
            Assert.IsTrue(diag.helpBox.visible, "Second click shows error");
            Assert.IsTrue(diag.helpBox.text.Contains("already exists") || diag.helpBox.text.Contains("Save"), "Should show duplicate error");
            var guids2 = AssetDatabase.FindAssets("t:FlowLevelAsset", new[] { TestFolder });
            Assert.AreEqual(1, guids2.Length, "Only one asset despite duplicate clicks");
            UnityEngine.Object.DestroyImmediate(w);
        }

        // ── B: Draw/Erase window wiring ──

        private static MethodInfo constraintStrokeM;
        static void InitB() { if (constraintStrokeM == null) constraintStrokeM = typeof(FlowLevelGeneratorWindow).GetMethod("DoConstraintStroke", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public); }

        [Test] public void DrawConstraintStroke_WindowActionCreatesConstraintAndUndoEntry()
        {
            InitB();
            var w = MakeWindow(); var d = MakeCompleteDraft();
            curDraftF.SetValue(w, d); DP(w).toolField.value = FlowDraftEditTool.DrawConstraint; DP(w).selectedColorField.value = 0;
            constraintStrokeM!.Invoke(w, new object[] { new List<FlowPos> { new(0,0), new(1,0), new(2,0) } });
            Assert.AreEqual(1, CD(w).fixedConstraints.Count);
            Assert.IsTrue(CH(w).CanUndo); Assert.IsTrue(CD(w).isSolutionDirty); Assert.IsFalse(CD(w).isValidated);
            Assert.IsFalse(DP(w).saveBtn.enabledSelf, "Save disabled after dirty");
            UnityEngine.Object.DestroyImmediate(w);
        }

        [Test] public void EraseConstraintStroke_WindowActionTrimsConstraintAndUndoEntry()
        {
            InitB();
            var w = MakeWindow(); var d = MakeCompleteDraft();
            // Apply valid constraint: anchored at (0,0), does not cross second endpoint (4,0)
            d.ApplyConstraint(0, new List<FlowPos> { new(0, 0), new(1, 0), new(2, 0) });
            d.isSolutionDirty = false; d.isValidated = true;
            curDraftF.SetValue(w, d); DP(w).toolField.value = FlowDraftEditTool.EraseConstraint; DP(w).selectedColorField.value = 0;
            constraintStrokeM!.Invoke(w, new object[] { new List<FlowPos> { new(1, 0), new(2, 0) } });
            Assert.IsTrue(CH(w).CanUndo); Assert.IsTrue(CD(w).isSolutionDirty); Assert.IsFalse(CD(w).isValidated);
            Assert.IsTrue(CD(w).fixedConstraints.Count == 0 || CD(w).fixedConstraints[0].cells.Count < 3, "Constraint trimmed");
            CH(w).Undo(); Assert.AreEqual(3, CD(w).fixedConstraints[0].cells.Count, "Undo restores full constraint");
            CH(w).Redo(); Assert.IsTrue(CD(w).fixedConstraints.Count == 0 || CD(w).fixedConstraints[0].cells.Count < 3, "Redo re-applies trim");
            UnityEngine.Object.DestroyImmediate(w);
        }

        [Test] public void DrawConstraint_InvalidStrokeShowsDiagnosticAndNoHistory()
        {
            InitB();
            var w = MakeWindow(); var d = MakeCompleteDraft();
            curDraftF.SetValue(w, d); DP(w).toolField.value = FlowDraftEditTool.DrawConstraint; DP(w).selectedColorField.value = 99;
            bool hadUndo = CH(w).CanUndo;
            constraintStrokeM!.Invoke(w, new object[] { new List<FlowPos> { new(0, 0), new(1, 0) } });
            Assert.IsFalse(CH(w).CanUndo && !hadUndo, "No new undo entry after invalid stroke");
            var diag = (FlowDiagnosticsPanel)w.GetType().GetField("diagnosticsPanel", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!.GetValue(w);
            Assert.IsTrue(diag.helpBox.visible, "Diagnostic should show error");
            UnityEngine.Object.DestroyImmediate(w);
        }

        [Test] public void CreateGUITwice_BoardStrokeCallbackExecutesOnce()
        {
            var w = MakeWindow();
            typeof(FlowLevelGeneratorWindow).GetMethod("CreateGUI", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!.Invoke(w, null);
            typeof(FlowLevelGeneratorWindow).GetMethod("CreateGUI", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!.Invoke(w, null);
            var d = MakeCompleteDraft();
            curDraftF.SetValue(w, d); DP(w).toolField.value = FlowDraftEditTool.DrawConstraint; DP(w).selectedColorField.value = 0;
            var bv = (FlowBoardView)w.GetType().GetField("boardView", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!.GetValue(w);
            bv.SetData(d); // feed board data so pointer path can detect valid cells
            bv.GetType().GetField("debugContentRect", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!.SetValue(bv, new UnityEngine.Rect(0,0,200,200));
            var DPm = bv.GetType().GetMethod("DoPointerDown", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            var DMm = bv.GetType().GetMethod("DoPointerMove", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            var DUm = bv.GetType().GetMethod("DoPointerUp", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            DPm!.Invoke(bv, new object[] { new UnityEngine.Vector2(23, 177), 1 });
            DMm!.Invoke(bv, new object[] { new UnityEngine.Vector2(62, 177) });
            DUm!.Invoke(bv, new object[] { new UnityEngine.Vector2(62, 177), 1 });
            Assert.AreEqual(1, CD(w).fixedConstraints.Count, "Stroke via CellStrokeCompleted must create constraint");
            Assert.IsTrue(CH(w).CanUndo);
            typeof(FlowLevelGeneratorWindow).GetMethod("DoUndo", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!.Invoke(w, null);
            Assert.AreEqual(0, CD(w).fixedConstraints.Count, "Undo clears constraint");
            Assert.IsFalse(CH(w).CanUndo, "Exactly 1 undo entry — no duplicate callbacks after repeated CreateGUI");
            UnityEngine.Object.DestroyImmediate(w);
        }

        [Test] public void EndpointTools_StillUseSingleCellSelectionNotStroke()
        {
            var w = MakeWindow(); var d = MakeCompleteDraft(2);
            curDraftF.SetValue(w, d); DP(w).toolField.value = FlowDraftEditTool.PlaceEndpoint; DP(w).selectedColorField.value = 1; DP(w).endpointToggle.value = true;
            var bv = (FlowBoardView)w.GetType().GetField("boardView", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!.GetValue(w);
            bv.SetData(d); // feed board data so CellSelected path detects valid cells
            bv.GetType().GetField("debugContentRect", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!.SetValue(bv, new UnityEngine.Rect(0,0,200,200));
            var DPm = bv.GetType().GetMethod("DoPointerDown", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            var DUm = bv.GetType().GetMethod("DoPointerUp", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            DPm!.Invoke(bv, new object[] { new UnityEngine.Vector2(23, 100), 1 }); // cell (0,1)
            DUm!.Invoke(bv, new object[] { new UnityEngine.Vector2(23, 100), 1 });
            Assert.IsNotNull(CD(w).pairs[1].endpointA, "Endpoint for color 1 placed via CellSelected");
            Assert.IsTrue(CH(w).CanUndo, "Endpoint placement creates undo entry");
            // Multi-stroke must NOT create constraint when endpoint tool selected
            DPm.Invoke(bv, new object[] { new UnityEngine.Vector2(23, 177), 1 });
            bv.GetType().GetMethod("DoPointerMove", BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public)!.Invoke(bv, new object[] { new UnityEngine.Vector2(62, 177) });
            DUm.Invoke(bv, new object[] { new UnityEngine.Vector2(62, 177), 1 });
            Assert.AreEqual(0, CD(w).fixedConstraints.Count, "Stroke while PlaceEndpoint tool active must not create constraint");
            UnityEngine.Object.DestroyImmediate(w);
        }

        // ── C: window state restore ──

        [Test] public void WindowState_RestoresDraftControlsAndBoardAfterCreateGUI()
        {
            var w = MakeWindow();
            var d = MakeCompleteDraft();
            d.ApplyConstraint(0, new List<FlowPos> { new(0,0), new(1,0), new(2,0) });
            curDraftF.SetValue(w, d); DP(w).toolField.value = FlowDraftEditTool.DrawConstraint; DP(w).selectedColorField.value = 0; DP(w).endpointToggle.value = false;
            DP(w).saveAsNameField.value = "TestSave";
            typeof(FlowLevelGeneratorWindow).GetMethod("CaptureDraftWindowState", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!.Invoke(w, null);
            // Rebuild
            typeof(FlowLevelGeneratorWindow).GetMethod("CreateGUI", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!.Invoke(w, null);
            var cd2 = CD(w);
            Assert.AreEqual(5, cd2.width); Assert.AreEqual(5, cd2.height); Assert.AreEqual(1, cd2.colorCount);
            Assert.AreEqual(FlowDraftEditTool.DrawConstraint, DP(w).toolField.value);
            Assert.AreEqual(0, DP(w).selectedColorField.value); Assert.IsFalse(DP(w).endpointToggle.value);
            Assert.AreEqual("TestSave", DP(w).saveAsNameField.value);
            Assert.AreEqual(1, cd2.fixedConstraints.Count);
            Assert.IsFalse(CH(w).CanUndo, "History cleared"); Assert.IsFalse(DP(w).undoBtn.enabledSelf, "Undo disabled");
            var bv = (FlowBoardView)w.GetType().GetField("boardView", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!.GetValue(w);
            var gk = typeof(FlowBoardView).GetMethod("GetDebugCellVisualKind", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            var kind = (int)gk!.Invoke(bv, new object[] { new FlowPos(1,0) });
            Assert.AreEqual(1, kind, "Solution cell restored (JSON round-trip preserves solution paths)");
            UnityEngine.Object.DestroyImmediate(w);
        }

        [Test] public void WindowState_RestoresLoadedAssetGuidAndAssetField()
        {
            var repo = new FlowLevelAssetRepository(new FlowSolutionValidator(), new FlowDifficultyEvaluator());
            var d = MakeCompleteDraft(levelId: 9001);
            var asset = repo.SaveNew(FlowDraftMapper.ToGeneratedLevel(d), TestFolder);
            var w = MakeWindow();
            curDraftF.SetValue(w, d); loadedAssetF.SetValue(w, asset);
            DP(w).assetField.value = asset;
            typeof(FlowLevelGeneratorWindow).GetMethod("CaptureDraftWindowState", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!.Invoke(w, null);
            typeof(FlowLevelGeneratorWindow).GetMethod("CreateGUI", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!.Invoke(w, null);
            Assert.AreSame(asset, loadedAssetF.GetValue(w), "loadedAsset restored via GUID");
            Assert.AreSame(asset, DP(w).assetField.value, "assetField restored");
            UnityEngine.Object.DestroyImmediate(w);
        }

        [Test] public void WindowState_InvalidAssetGuidDoesNotThrowOrLoadAsset()
        {
            var w = MakeWindow(); var d = MakeCompleteDraft();
            curDraftF.SetValue(w, d);
            var state = new FlowDraftWindowState { loadedAssetGuid = "nonexistent_guid_12345", draftJson = JsonUtility.ToJson(d) };
            typeof(FlowLevelGeneratorWindow).GetField("draftWindowState", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!.SetValue(w, state);
            Assert.DoesNotThrow(() => typeof(FlowLevelGeneratorWindow).GetMethod("RestoreDraftWindowState", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!.Invoke(w, null));
            Assert.IsNull(loadedAssetF.GetValue(w), "Invalid GUID sets null");
            Assert.IsNotNull(CD(w), "Draft restored despite invalid GUID");
            UnityEngine.Object.DestroyImmediate(w);
        }

        [Test] public void WindowState_ReloadClearsUndoRedoHistory()
        {
            var w = MakeWindow(); var d = MakeCompleteDraft();
            curDraftF.SetValue(w, d); DP(w).toolField.value = FlowDraftEditTool.PlaceEndpoint; DP(w).selectedColorField.value = 0; DP(w).endpointToggle.value = true;
            typeof(FlowLevelGeneratorWindow).GetMethod("DoEndpointEdit", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!.Invoke(w, new object[] { new FlowPos(2, 2) });
            Assert.IsTrue(CH(w).CanUndo);
            typeof(FlowLevelGeneratorWindow).GetMethod("CaptureDraftWindowState", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!.Invoke(w, null);
            typeof(FlowLevelGeneratorWindow).GetMethod("CreateGUI", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!.Invoke(w, null);
            Assert.IsFalse(CH(w).CanUndo, "History cleared after reload"); Assert.IsFalse(CH(w).CanRedo);
            UnityEngine.Object.DestroyImmediate(w);
        }

        [Test] public void WindowState_SaveAsNameSurvivesCreateGUI()
        {
            var w = MakeWindow(); DP(w).saveAsNameField.value = "MyCustomSave";
            typeof(FlowLevelGeneratorWindow).GetMethod("CaptureDraftWindowState", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!.Invoke(w, null);
            typeof(FlowLevelGeneratorWindow).GetMethod("CreateGUI", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!.Invoke(w, null);
            Assert.AreEqual("MyCustomSave", DP(w).saveAsNameField.value);
            UnityEngine.Object.DestroyImmediate(w);
        }

        [Test] public void WindowState_NoDraft_RestoreLeavesDraftNullAndButtonsDisabled()
        {
            var w = MakeWindow();
            // Pre-enable buttons with a complete draft
            var d = MakeCompleteDraft();
            curDraftF.SetValue(w, d);
            DP(w).UpdateDraftState(d, CH(w));
            Assert.IsTrue(DP(w).saveBtn.enabledSelf, "Save enabled before restore");
            // Restore empty state
            var state = new FlowDraftWindowState();
            typeof(FlowLevelGeneratorWindow).GetField("draftWindowState", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!.SetValue(w, state);
            typeof(FlowLevelGeneratorWindow).GetMethod("RestoreDraftWindowState", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!.Invoke(w, null);
            Assert.IsNull(CD(w));
            Assert.IsFalse(DP(w).saveBtn.enabledSelf, "Save disabled after null-draft restore");
            Assert.IsFalse(DP(w).saveAsBtn.enabledSelf, "Save As disabled");
            Assert.IsFalse(DP(w).undoBtn.enabledSelf, "Undo disabled");
            Assert.IsFalse(DP(w).redoBtn.enabledSelf, "Redo disabled");
            UnityEngine.Object.DestroyImmediate(w);
        }

        [Test] public void OnDisable_CapturesCurrentStateAndRestoresAfterCreateGUI()
        {
            var w = MakeWindow();
            DP(w).saveAsNameField.value = "BeforeDisable";
            DP(w).toolField.value = FlowDraftEditTool.EraseConstraint;
            DP(w).selectedColorField.value = 1;
            DP(w).endpointToggle.value = false;
            typeof(FlowLevelGeneratorWindow).GetMethod("OnDisable", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)!.Invoke(w, null);
            typeof(FlowLevelGeneratorWindow).GetMethod("CreateGUI", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!.Invoke(w, null);
            Assert.AreEqual("BeforeDisable", DP(w).saveAsNameField.value, "SaveAs name restored");
            Assert.AreEqual(FlowDraftEditTool.EraseConstraint, DP(w).toolField.value, "Tool restored");
            Assert.AreEqual(1, DP(w).selectedColorField.value, "Color restored");
            Assert.IsFalse(DP(w).endpointToggle.value, "Endpoint toggle restored");
            UnityEngine.Object.DestroyImmediate(w);
        }
    }
}
