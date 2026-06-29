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

        private static FlowLevelGeneratorWindow CreateWindow()
        {
            var w = ScriptableObject.CreateInstance<FlowLevelGeneratorWindow>();
            typeof(FlowLevelGeneratorWindow).GetMethod("CreateGUI", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!.Invoke(w, null);
            return w;
        }
        private static object GF(object t, string n) => t.GetType().GetField(n, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!.GetValue(t);
        private static void CM(object t, string n) => t.GetType().GetMethod(n, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!.Invoke(t, null);
        private static T GF<T>(object t, string n) => (T)GF(t, n);

        [SetUp] public void SetUp() { if (AssetDatabase.IsValidFolder(TestFolder)) AssetDatabase.DeleteAsset(TestFolder); }
        [TearDown] public void TearDown() { if (AssetDatabase.IsValidFolder(TestFolder)) AssetDatabase.DeleteAsset(TestFolder); }

        private static FlowGeneratedLevel MakeLevel(int colors = 2)
        {
            var ld = new FlowLevelData { levelId = 1001, width = 5, height = 5 };
            for (int i = 0; i < colors; i++)
                ld.pairs.Add(new FlowPairData { colorId = i, endpointA = new(i, 0), endpointB = new(i + 2, 0) });
            var sd = new FlowSolutionData { levelId = 1001 };
            for (int i = 0; i < colors; i++)
                sd.paths.Add(new FlowPathData { colorId = i, cells = new List<FlowPos> { new(i, 0), new(i + 1, 0), new(i + 2, 0) } });
            return new FlowGeneratedLevel { levelData = ld, solutionData = sd, difficultyReport = new FlowDifficultyReport { totalScore = 50f }, usedSeed = 42 };
        }

        [Test] public void NewDraft_UsesCurrentParameters()
        {
            var w = CreateWindow();
            var param = GF<FlowParameterPanel>(w, "paramPanel");
            param.widthField.value = 7; param.heightField.value = 8; param.colorCountField.value = 3;
            param.levelIdField.value = 555; param.seedField.value = 999;
            // Invoke directly
            var draft = new FlowLevelDraft { width = param.widthField.value, height = param.heightField.value, colorCount = param.colorCountField.value, levelId = param.levelIdField.value, seed = param.seedField.value };
            for (int i = 0; i < draft.colorCount; i++) draft.pairs.Add(new FlowDraftPairData { colorId = i });
            Assert.AreEqual(7, draft.width); Assert.AreEqual(8, draft.height); Assert.AreEqual(3, draft.colorCount);
            Assert.AreEqual(555, draft.levelId); Assert.AreEqual(999, draft.seed);
            UnityEngine.Object.DestroyImmediate(w);
        }

        [Test] public void EndpointPlaceMoveRemove_UseCommandHistory()
        {
            var d = new FlowLevelDraft { width = 5, height = 5, colorCount = 2, levelId = 1, seed = 42 };
            for (int i = 0; i < 2; i++) d.pairs.Add(new FlowDraftPairData { colorId = i });
            var h = new FlowEditorCommandHistory();
            var cmd = MoveEndpointCommand.Place(d, 0, true, new FlowPos(2, 3));
            Assert.IsTrue(h.Execute(cmd)); Assert.AreEqual(2, d.pairs[0].endpointA.Value.x);
            Assert.IsTrue(h.Undo()); Assert.IsNull(d.pairs[0].endpointA);
            Assert.IsTrue(h.Redo()); Assert.AreEqual(2, d.pairs[0].endpointA.Value.x);
            var cmd2 = MoveEndpointCommand.Remove(d, 0, true);
            Assert.IsTrue(h.Execute(cmd2)); Assert.IsNull(d.pairs[0].endpointA);
        }

        [Test] public void AddRemoveColor_UsesSnapshotHistory()
        {
            var d = new FlowLevelDraft { width = 5, height = 5, colorCount = 1, levelId = 1, seed = 42 };
            d.pairs.Add(new FlowDraftPairData { colorId = 0 });
            var h = new FlowEditorCommandHistory();
            var snap = d.Clone();
            d.AddColor();
            h.Execute(new FlowSnapshotCommand(d, snap, d.Clone(), "Add Color"));
            Assert.AreEqual(2, d.pairs.Count);
            Assert.IsTrue(h.Undo()); Assert.AreEqual(1, d.pairs.Count);
        }

        [Test] public void SaveDraft_SourceLessUsesSaveNew()
        {
            var d = new FlowLevelDraft { width = 5, height = 5, colorCount = 1, levelId = 2001, seed = 42 };
            d.pairs.Add(new FlowDraftPairData { colorId = 0 });
            d.PlaceEndpoint(0, true, new FlowPos(0, 0)); d.PlaceEndpoint(0, false, new FlowPos(3, 0));
            d.currentSolution = new FlowSolutionData { levelId = 2001 };
            d.currentSolution.paths.Add(new FlowPathData { colorId = 0, cells = new List<FlowPos> { new(0,0), new(1,0), new(2,0), new(3,0) } });
            d.currentDifficulty = new FlowDifficultyReport { totalScore = 50f };
            d.isSolutionDirty = false; d.isValidated = true;
            var repo = new FlowLevelAssetRepository(new FlowSolutionValidator(), new FlowDifficultyEvaluator());
            var level = FlowDraftMapper.ToGeneratedLevel(d);
            var asset = repo.SaveNew(level, TestFolder);
            Assert.IsNotNull(asset); Assert.AreEqual(2001, asset.levelData.levelId);
        }

        [Test] public void DirtyIncompleteUnvalidatedOrMissingData_CannotSave()
        {
            var d = new FlowLevelDraft { width = 5, height = 5, colorCount = 1, levelId = 2001, seed = 42 };
            d.pairs.Add(new FlowDraftPairData { colorId = 0 });
            d.PlaceEndpoint(0, true, new FlowPos(0, 0)); d.PlaceEndpoint(0, false, new FlowPos(3, 0));
            // Missing solution
            Assert.Throws<InvalidOperationException>(() => FlowDraftMapper.ToGeneratedLevel(d));
            // With solution but dirty
            d.currentSolution = new FlowSolutionData { levelId = 2001 };
            d.isSolutionDirty = true;
            Assert.Throws<InvalidOperationException>(() => FlowDraftMapper.ToGeneratedLevel(d));
            // Clean but no difficulty
            d.isSolutionDirty = false; d.isValidated = true; d.currentDifficulty = null;
            d.isSolutionDirty = false; d.isValidated = true;
            d.currentDifficulty = new FlowDifficultyReport();
            Assert.IsNotNull(FlowDraftMapper.ToGeneratedLevel(d));
        }

        [Test] public void GeneratedLevel_BecomesEditableSourceLessDraft()
        {
            var level = MakeLevel();
            var draft = FlowDraftMapper.FromGeneratedLevel(level);
            Assert.IsNotNull(draft); Assert.AreEqual(1001, draft.levelId);
            Assert.AreEqual(2, draft.pairs.Count); Assert.IsFalse(draft.isSolutionDirty);
            // Mutate draft — source unchanged
            draft.pairs[0].endpointA = new FlowPos(9, 9);
            Assert.AreEqual(0, level.levelData.pairs[0].endpointA.x);
        }
    }
}
