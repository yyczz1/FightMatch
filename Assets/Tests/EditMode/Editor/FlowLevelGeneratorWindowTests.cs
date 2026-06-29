using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using FlowPuzzle.Core;
using FlowPuzzle.Editor;
using FlowPuzzle.Editor.UI;
using FlowPuzzle.Generation;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace FlowPuzzle.Tests.Editor
{
    [TestFixture]
    public class FlowLevelGeneratorWindowTests
    {
        private const string TestFolder = "Assets/Temp/FlowPuzzleEditorTests";
        private static readonly string JsonTestDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        private bool tempExistedBefore;

        [SetUp]
        public void SetUp()
        {
            tempExistedBefore = AssetDatabase.IsValidFolder("Assets/Temp");
            if (AssetDatabase.IsValidFolder(TestFolder)) AssetDatabase.DeleteAsset(TestFolder);
        }

        [TearDown]
        public void TearDown()
        {
            if (AssetDatabase.IsValidFolder(TestFolder)) AssetDatabase.DeleteAsset(TestFolder);
            if (!tempExistedBefore && AssetDatabase.IsValidFolder("Assets/Temp") &&
                AssetDatabase.GetSubFolders("Assets/Temp").Length == 0)
                AssetDatabase.DeleteAsset("Assets/Temp");
            if (Directory.Exists(JsonTestDir)) Directory.Delete(JsonTestDir, true);
        }

        private static FlowLevelGeneratorWindow CreateWindow()
        {
            var w = ScriptableObject.CreateInstance<FlowLevelGeneratorWindow>();
            typeof(FlowLevelGeneratorWindow).GetMethod("CreateGUI",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!.Invoke(w, null);
            return w;
        }

        private static object GF(object t, string n) => t.GetType().GetField(n,
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!.GetValue(t);
        private static void CM(object t, string n) => t.GetType().GetMethod(n,
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!.Invoke(t, null);
        private static T GF<T>(object t, string n) => (T)GF(t, n);

        // ── Lifecycle ──

        [Test]
        public void CreateGUI_RequiredControlsExistExactlyOnce()
        {
            var w = CreateWindow();
            Assert.AreEqual(1, w.rootVisualElement.Query<Button>("generate-one").ToList().Count);
            Assert.AreEqual(1, w.rootVisualElement.Query<Button>("generate-batch").ToList().Count);
            Assert.AreEqual(1, w.rootVisualElement.Query<Button>("save-current").ToList().Count);
            Assert.AreEqual(1, w.rootVisualElement.Query<Button>("export-json").ToList().Count);
            Assert.AreEqual(1, w.rootVisualElement.Query<Button>("clear-preview").ToList().Count);
            UnityEngine.Object.DestroyImmediate(w);
        }

        [Test]
        public void CreateGUI_Twice_DoesNotDuplicateButtonsOrBoardView()
        {
            var w = CreateWindow();
            CM(w, "CreateGUI"); CM(w, "CreateGUI");
            Assert.AreEqual(1, w.rootVisualElement.Query<Button>("generate-one").ToList().Count);
            var bc = (VisualElement)GF(w, "boardContainer");
            Assert.AreEqual(1, bc.childCount, "Board views should not accumulate");
            UnityEngine.Object.DestroyImmediate(w);
        }

        // ── Button states ──

        [Test]
        public void InvalidConfig_DisablesGenerateButtons()
        {
            var w = CreateWindow();
            var param = GF<FlowParameterPanel>(w, "paramPanel");
            // Set invalid value, then trigger the window's own state refresh
            param.widthField.value = 0;
            CM(w, "UpdateButtonStates");
            Assert.IsFalse(w.rootVisualElement.Q<Button>("generate-one").enabledSelf);
            Assert.IsFalse(w.rootVisualElement.Q<Button>("generate-batch").enabledSelf);
            UnityEngine.Object.DestroyImmediate(w);
        }

        [Test]
        public void InvalidConfigThenValidConfig_ReenablesGenerateButtons()
        {
            var w = CreateWindow();
            var param = GF<FlowParameterPanel>(w, "paramPanel");
            param.widthField.value = 0; CM(w, "UpdateButtonStates");
            Assert.IsFalse(w.rootVisualElement.Q<Button>("generate-one").enabledSelf);
            param.widthField.value = 5; CM(w, "UpdateButtonStates");
            Assert.IsTrue(w.rootVisualElement.Q<Button>("generate-one").enabledSelf);
            UnityEngine.Object.DestroyImmediate(w);
        }

        // ── Generate One ──

        [Test]
        public void GenerateOne_CreatesPreviewAndNoAsset()
        {
            var w = CreateWindow();
            var param = GF<FlowParameterPanel>(w, "paramPanel");
            param.widthField.value = 5; param.heightField.value = 5; param.colorCountField.value = 2; param.seedField.value = 42;

            var beforeGuids = new HashSet<string>(AssetDatabase.FindAssets("t:FlowLevelAsset"));
            CM(w, "OnGenerateOne");
            Assert.IsNotNull(GF(w, "currentLevel"));
            var afterGuids = new HashSet<string>(AssetDatabase.FindAssets("t:FlowLevelAsset"));
            Assert.IsTrue(beforeGuids.SetEquals(afterGuids), "Generate One must not create any assets");
            UnityEngine.Object.DestroyImmediate(w);
        }

        // ── Save Current ──

        [Test]
        public void SaveCurrent_CreatesCompleteAsset()
        {
            var w = CreateWindow();
            var param = GF<FlowParameterPanel>(w, "paramPanel");
            param.levelIdField.value = 1001; param.widthField.value = 5; param.heightField.value = 5;
            param.colorCountField.value = 2; param.seedField.value = 42;
            param.outputFolderField.value = TestFolder;

            CM(w, "OnGenerateOne"); CM(w, "OnSaveCurrent");

            var guids = AssetDatabase.FindAssets("t:FlowLevelAsset", new[] { TestFolder });
            Assert.AreEqual(1, guids.Length);
            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            var asset = AssetDatabase.LoadAssetAtPath<FlowPuzzle.Persistence.FlowLevelAsset>(path);
            Assert.AreEqual(1001, asset.levelData.levelId);
            Assert.AreEqual(42, asset.generationSeed);
            Assert.IsTrue(asset.solutionData.paths.Count >= 1);
            Assert.IsNotNull(asset.difficultyReport);
            UnityEngine.Object.DestroyImmediate(w);
        }

        // ── Export JSON ──

        [Test]
        public void ExportJson_CreatesLevelAndSolutionJson()
        {
            var w = CreateWindow();
            var param = GF<FlowParameterPanel>(w, "paramPanel");
            param.widthField.value = 5; param.heightField.value = 5; param.colorCountField.value = 2; param.seedField.value = 42;
            param.outputFolderField.value = JsonTestDir;

            CM(w, "OnGenerateOne"); CM(w, "OnExportJson");

            Assert.IsTrue(File.Exists(Path.Combine(JsonTestDir, "level_1.json")));
            Assert.IsTrue(File.Exists(Path.Combine(JsonTestDir, "solution_1.json")));
            Assert.IsFalse(File.ReadAllText(Path.Combine(JsonTestDir, "level_1.json")).Contains("\"paths\""));
            Assert.IsTrue(File.ReadAllText(Path.Combine(JsonTestDir, "solution_1.json")).Contains("\"paths\""));
            UnityEngine.Object.DestroyImmediate(w);
        }

        // ── Clear ──

        [Test]
        public void ClearPreview_ClearsCurrentResultButPreservesConfig()
        {
            var w = CreateWindow();
            var param = GF<FlowParameterPanel>(w, "paramPanel");
            param.widthField.value = 7; param.heightField.value = 6; param.colorCountField.value = 3; param.seedField.value = 99;

            CM(w, "OnGenerateOne"); Assert.IsNotNull(GF(w, "currentLevel"));
            CM(w, "OnClearPreview");
            Assert.IsNull(GF(w, "currentLevel"));
            Assert.AreEqual(7, param.widthField.value);
            Assert.AreEqual(99, param.seedField.value);
            UnityEngine.Object.DestroyImmediate(w);
        }

        // ── Batch success ──

        [Test]
        public void GenerateBatch_SavesSuccessfulAssets()
        {
            var w = CreateWindow();
            var param = GF<FlowParameterPanel>(w, "paramPanel");
            param.widthField.value = 5; param.heightField.value = 5; param.colorCountField.value = 2; param.seedField.value = 42;
            param.batchCountField.value = 2; param.outputFolderField.value = TestFolder;

            CM(w, "OnGenerateBatch");

            var batchPanel = GF<FlowBatchReportPanel>(w, "batchPanel");
            var items = (IList)batchPanel.listView.itemsSource;
            Assert.IsNotNull(items, "ListView itemsSource should not be null");
            Assert.AreEqual(2, items.Count);

            var guids = AssetDatabase.FindAssets("t:FlowLevelAsset", new[] { TestFolder });
            int saved = 0;
            for (int i = 0; i < items.Count; i++)
            {
                var item = (FlowBatchItemResult)items[i];
                if (item.success)
                {
                    saved++;
                    var ig = AssetDatabase.FindAssets($"t:FlowLevelAsset Level_{item.levelId}", new[] { TestFolder });
                    Assert.IsTrue(ig.Length >= 1, $"Asset should exist for successful level {item.levelId}");
                }
            }
            Assert.AreEqual(guids.Length, saved,
                $"Saved asset count ({guids.Length}) should equal successful items ({saved})");
            UnityEngine.Object.DestroyImmediate(w);
        }

        // ── Batch failure ──

        [Test]
        public void GenerateBatch_SaveFailureMarksItemFailedAndContinues()
        {
            var w = CreateWindow();
            var param = GF<FlowParameterPanel>(w, "paramPanel");
            param.widthField.value = 5; param.heightField.value = 5; param.colorCountField.value = 2; param.seedField.value = 42;
            param.batchCountField.value = 2; param.outputFolderField.value = "AssetsOutside";

            CM(w, "OnGenerateBatch");

            var batchPanel = GF<FlowBatchReportPanel>(w, "batchPanel");
            var items = (IList)batchPanel.listView.itemsSource;
            Assert.IsNotNull(items, "ListView itemsSource should not be null");
            Assert.AreEqual(2, items.Count, "Should process all 2 items — continuation proof");

            // Both items must have been processed and marked failed
            for (int i = 0; i < items.Count; i++)
            {
                var item = (FlowBatchItemResult)items[i];
                Assert.IsFalse(item.success, $"Item {i} should be marked failed due to invalid folder");
                Assert.IsTrue(item.message.Contains("Save failed"),
                    $"Item {i} message should mention save failure: '{item.message}'");
            }

            var diag = GF<FlowDiagnosticsPanel>(w, "diagnosticsPanel");
            Assert.IsTrue(diag.helpBox.visible, "Diagnostics should be visible");
            Assert.IsTrue(diag.helpBox.text.Contains("save failure") || diag.helpBox.text.Contains("Batch"));
            UnityEngine.Object.DestroyImmediate(w);
        }

        // ── Parameter panel ──

        [Test] public void Panel_ReadConfig_Defaults() { var p = new FlowParameterPanel(); p.Build(new VisualElement()); var c = p.ReadConfig(); Assert.AreEqual(5, c.width); Assert.AreEqual(42, c.seed); }
        [Test] public void Panel_ApplyPreset_FullValues() { var p = new FlowParameterPanel(); p.Build(new VisualElement()); var cfg = new FlowGenerationConfig { width = 6, height = 6, colorCount = 3, minCoverageRatio = 0.3f, maxCoverageRatio = 0.7f, minPathLength = 2, maxPathLength = 7, turnPreference = 0.5f, interactionPreference = -0.3f, maxPathAttempt = 250, maxLevelAttempt = 100, useRandomSeed = false, seed = 99, useTargetDifficulty = true, targetDifficulty = FlowDifficultyTier.Normal, useTargetScoreRange = true, minTargetDifficultyScore = 60f, maxTargetDifficultyScore = 119.999f }; p.ApplyPresetValues(cfg); Assert.AreEqual(6, p.widthField.value); Assert.AreEqual(99, p.seedField.value); Assert.IsTrue(p.targetTierToggle.value); Assert.AreEqual(FlowDifficultyTier.Normal, p.targetTierField.value); Assert.AreEqual(250, p.pathAttemptField.value); Assert.AreEqual(60f, p.minScoreField.value, 0.001f); Assert.AreEqual(119.999f, p.maxScoreField.value, 0.001f); Assert.AreEqual(100, p.levelAttemptField.value); Assert.IsFalse(p.useRandomSeedToggle.value); }

        // ── Board geometry ──

        [Test] public void Geometry_NegPadding_Throws() => Assert.Throws<ArgumentOutOfRangeException>(() => FlowBoardViewGeometry.CalculateBoardRect(new Rect(0,0,100,100), 5, 5, -1f));
        [Test] public void Geometry_WithinContent() { var r = FlowBoardViewGeometry.CalculateBoardRect(new Rect(0,0,100,60), 10, 5, 0f); Assert.IsTrue(r.width <= 100f); }
        [Test] public void Geometry_YInverted() { var br = new Rect(0,0,50,60); var tl = FlowBoardViewGeometry.GetCellRect(br, 5, 3, new FlowPos(0,0)); Assert.AreEqual(40f, tl.y, 0.01f); }
        [Test] public void Geometry_RoundTrip() { var br = new Rect(0,0,100,100); var cell = new FlowPos(2,1); var cr = FlowBoardViewGeometry.GetCellRect(br,5,5,cell); Assert.IsTrue(FlowBoardViewGeometry.TryGetCell(br,5,5,new Vector2(cr.x+cr.width/2f,cr.y+cr.height/2f),out var r)); Assert.AreEqual(cell,r); }
    }
}
