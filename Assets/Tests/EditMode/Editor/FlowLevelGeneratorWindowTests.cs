using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FlowPuzzle.Core;
using FlowPuzzle.Editor;
using FlowPuzzle.Editor.Commands;
using FlowPuzzle.Editor.Draft;
using FlowPuzzle.Editor.UI;
using FlowPuzzle.Generation;
using FlowPuzzle.Solving;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace FlowPuzzle.Tests.Editor
{
    [TestFixture]
    public class FlowLevelGeneratorWindowTests
    {
        private static readonly string JsonTestDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

        [TearDown]
        public void TearDown()
        {
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

        private static FlowLevelDraft CreateConstraintDraft()
        {
            var draft = new FlowLevelDraft { levelId = 1, width = 5, height = 5, seed = 42 };
            Assert.IsTrue(draft.AddColor().success);
            Assert.IsTrue(draft.PlaceEndpoint(0, true, new FlowPos(0, 0)).success);
            Assert.IsTrue(draft.PlaceEndpoint(0, false, new FlowPos(4, 0)).success);
            Assert.IsTrue(draft.ApplyConstraint(0, new List<FlowPos>
            {
                new FlowPos(0, 0), new FlowPos(1, 0), new FlowPos(2, 0), new FlowPos(3, 0)
            }).success);
            return draft;
        }

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

        [Test]
        public void CreateGUI_SetsReadableMinimumWindowSize()
        {
            var w = CreateWindow();
            Assert.GreaterOrEqual(w.minSize.x, 1000f);
            Assert.GreaterOrEqual(w.minSize.y, 650f);
            UnityEngine.Object.DestroyImmediate(w);
        }

        [Test]
        public void DrawConstraint_ClickMiddleCellRewindsWithOneUndoEntry()
        {
            var w = CreateWindow();
            var draft = CreateConstraintDraft();
            var panel = GF<FlowDraftPanel>(w, "draftPanel");
            panel.toolField.value = FlowDraftEditTool.DrawConstraint;
            panel.selectedColorField.value = 0;
            w.GetType().GetField("currentDraft", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(w, draft);

            w.GetType().GetMethod("DoEndpointEdit", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!
                .Invoke(w, new object[] { new FlowPos(2, 0) });

            Assert.AreEqual(3, draft.fixedConstraints[0].cells.Count);
            Assert.AreEqual(new FlowPos(2, 0), draft.fixedConstraints[0].cells.Last());
            var history = GF<FlowEditorCommandHistory>(w, "commandHistory");
            Assert.IsTrue(history.CanUndo);
            history.Undo();
            Assert.AreEqual(4, draft.fixedConstraints[0].cells.Count);
            Assert.IsFalse(history.CanUndo, "Rewind must create exactly one history entry");
            history.Redo();
            Assert.AreEqual(3, draft.fixedConstraints[0].cells.Count);
            UnityEngine.Object.DestroyImmediate(w);
        }

        [Test]
        public void DrawConstraint_ClickCellOutsideConstraintDoesNotMutateOrAddHistory()
        {
            var w = CreateWindow();
            var draft = CreateConstraintDraft();
            var panel = GF<FlowDraftPanel>(w, "draftPanel");
            panel.toolField.value = FlowDraftEditTool.DrawConstraint;
            panel.selectedColorField.value = 0;
            w.GetType().GetField("currentDraft", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(w, draft);

            w.GetType().GetMethod("DoEndpointEdit", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!
                .Invoke(w, new object[] { new FlowPos(4, 1) });

            Assert.AreEqual(4, draft.fixedConstraints[0].cells.Count);
            Assert.IsFalse(GF<FlowEditorCommandHistory>(w, "commandHistory").CanUndo);
            Assert.That(GF<FlowDiagnosticsPanel>(w, "diagnosticsPanel").helpBox.text, Does.Contain("not part"));
            UnityEngine.Object.DestroyImmediate(w);
        }

        [Test]
        public void SetEndpoint_OnForeignConstraintShowsErrorAndDoesNotCreateDeadEndState()
        {
            var w = CreateWindow();
            var draft = CreateConstraintDraft();
            Assert.IsTrue(draft.AddColor().success);
            var panel = GF<FlowDraftPanel>(w, "draftPanel");
            panel.UpdateDraftState(draft, GF<FlowEditorCommandHistory>(w, "commandHistory"));
            panel.toolField.value = FlowDraftEditTool.PlaceEndpoint;
            panel.selectedColorField.value = 1;
            panel.endpointToggle.value = true;
            w.GetType().GetField("currentDraft", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(w, draft);

            w.GetType().GetMethod("DoEndpointEdit", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!
                .Invoke(w, new object[] { new FlowPos(1, 0) });

            Assert.IsNull(draft.GetPair(1).endpointA);
            Assert.IsFalse(GF<FlowEditorCommandHistory>(w, "commandHistory").CanUndo);
            Assert.That(GF<FlowDiagnosticsPanel>(w, "diagnosticsPanel").helpBox.text, Does.Contain("constraint of color 0"));
            UnityEngine.Object.DestroyImmediate(w);
        }

        [Test]
        public void DrawConstraint_ClickOwnEndpointWithoutConstraintShowsStartHint()
        {
            var w = CreateWindow();
            var draft = CreateConstraintDraft();
            draft.fixedConstraints.Clear();
            var panel = GF<FlowDraftPanel>(w, "draftPanel");
            panel.toolField.value = FlowDraftEditTool.DrawConstraint;
            panel.selectedColorField.value = 0;
            w.GetType().GetField("currentDraft", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(w, draft);

            w.GetType().GetMethod("DoEndpointEdit", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!
                .Invoke(w, new object[] { new FlowPos(0, 0) });

            Assert.IsFalse(GF<FlowEditorCommandHistory>(w, "commandHistory").CanUndo);
            Assert.That(GF<FlowDiagnosticsPanel>(w, "diagnosticsPanel").helpBox.text, Does.Contain("Drag from this endpoint"));
            UnityEngine.Object.DestroyImmediate(w);
        }

        [Test]
        public void DraftColorChoices_UseTheBoardPaletteNames()
        {
            var w = CreateWindow();
            var panel = GF<FlowDraftPanel>(w, "draftPanel");
            var draft = new FlowLevelDraft { levelId = 1, width = 5, height = 5, seed = 42 };
            Assert.IsTrue(draft.AddColor().success);
            Assert.IsTrue(draft.AddColor().success);
            Assert.IsTrue(draft.AddColor().success);

            panel.UpdateDraftState(draft, new FlowEditorCommandHistory());

            Assert.AreEqual("Red", panel.selectedColorField.formatSelectedValueCallback(0));
            Assert.AreEqual("Blue", panel.selectedColorField.formatSelectedValueCallback(1));
            Assert.AreEqual("Green", panel.selectedColorField.formatSelectedValueCallback(2));
            UnityEngine.Object.DestroyImmediate(w);
        }

        [Test]
        public void DraftPanel_UsesOneSetEndpointToolAndPlacesCompleteBeforeSave()
        {
            var root = new VisualElement();
            var panel = new FlowDraftPanel();
            panel.Build(root);

            CollectionAssert.Contains(panel.toolField.choices, FlowDraftEditTool.PlaceEndpoint);
            CollectionAssert.DoesNotContain(panel.toolField.choices, FlowDraftEditTool.MoveEndpoint);
            Assert.AreEqual("Set Endpoint", panel.toolField.formatSelectedValueCallback(FlowDraftEditTool.PlaceEndpoint));

            var children = root.Children().ToList();
            Assert.Less(children.IndexOf(panel.completeBtn), children.IndexOf(panel.saveAsNameField));
            Assert.Less(children.IndexOf(panel.completeBtn), children.IndexOf(panel.saveBtn));
            Assert.Less(children.IndexOf(panel.completeBtn), children.IndexOf(panel.saveAsBtn));
        }

        [Test]
        public void DraftEdit_ClearsStaleResultAndCurrentLevel()
        {
            var w = CreateWindow();
            var draft = CreateConstraintDraft();
            draft.currentSolution = new FlowSolutionData
            {
                levelId = draft.levelId,
                paths = new List<FlowPathData>
                {
                    new FlowPathData
                    {
                        colorId = 0,
                        cells = new List<FlowPos>
                        {
                            new FlowPos(0, 0), new FlowPos(1, 0), new FlowPos(2, 0),
                            new FlowPos(3, 0), new FlowPos(4, 0)
                        }
                    }
                }
            };
            draft.currentDifficulty = new FlowDifficultyReport { difficulty = FlowDifficultyTier.Hard, totalScore = 121.5f };
            draft.coverage = 0.84f;
            draft.isSolutionDirty = false;
            draft.isValidated = true;
            var level = FlowDraftMapper.ToGeneratedLevel(draft);

            w.GetType().GetField("currentDraft", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(w, draft);
            w.GetType().GetField("currentLevel", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(w, level);
            var result = GF<FlowResultPanel>(w, "resultPanel");
            result.Show(level);
            var panel = GF<FlowDraftPanel>(w, "draftPanel");
            panel.toolField.value = FlowDraftEditTool.PlaceEndpoint;
            panel.selectedColorField.value = 0;
            panel.endpointToggle.value = false;

            w.GetType().GetMethod("DoEndpointEdit", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!
                .Invoke(w, new object[] { new FlowPos(4, 1) });

            Assert.IsNull(GF(w, "currentLevel"));
            Assert.AreEqual("Difficulty: —", result.difficultyLabel.text);
            Assert.AreEqual("Coverage: —", result.coverageLabel.text);
            Assert.AreEqual("Score: —", result.scoreLabel.text);
            UnityEngine.Object.DestroyImmediate(w);
        }

        [UnityTest]
        public IEnumerator CompleteDraft_BackgroundProgressReturnsToMainThread()
        {
            var w = CreateWindow();
            var draft = CreateConstraintDraft();
            w.GetType().GetField("currentDraft", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(w, draft);

            var task = (Task<FlowCompletionResult>)w.GetType()
                .GetMethod("DoCompleteDraftAsync", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!
                .Invoke(w, null);
            while (!task.IsCompleted)
                yield return null;

            Assert.IsFalse(task.IsFaulted, task.Exception?.ToString());
            Assert.AreEqual(FlowSolveStatus.Solved, task.Result.status,
                GF<FlowDiagnosticsPanel>(w, "diagnosticsPanel").helpBox.text);
            Assert.IsFalse(GF<FlowDiagnosticsPanel>(w, "diagnosticsPanel").helpBox.text.Contains("ProviderException"));
            Assert.IsNotNull(draft.currentSolution);
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

            CM(w, "OnGenerateOne");
            Assert.IsNotNull(GF(w, "currentLevel"));
            Assert.IsNull(GF(w, "loadedAsset"), "Generate One must not attach a persisted asset");
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
        [Test] public void Panel_UsesPreciseGenerationLabelsAndHelp()
        {
            var p = new FlowParameterPanel(); p.Build(new VisualElement());
            Assert.AreEqual("Color Count", p.colorCountField.label);
            Assert.AreEqual("Min Recommended Path Cells", p.minPathLenField.label);
            Assert.AreEqual("Max Recommended Path Cells", p.maxPathLenField.label);
            Assert.That(p.minPathLenField.tooltip, Does.Contain("including both endpoints"));
            Assert.That(p.maxPathLenField.tooltip, Does.Contain("not a player restriction"));
        }
        [Test] public void Panel_ApplyPreset_FullValues() { var p = new FlowParameterPanel(); p.Build(new VisualElement()); var cfg = new FlowGenerationConfig { width = 6, height = 6, colorCount = 3, minCoverageRatio = 0.3f, maxCoverageRatio = 0.7f, minPathLength = 2, maxPathLength = 7, turnPreference = 0.5f, interactionPreference = -0.3f, maxPathAttempt = 250, maxLevelAttempt = 100, useRandomSeed = false, seed = 99, useTargetDifficulty = true, targetDifficulty = FlowDifficultyTier.Normal, useTargetScoreRange = true, minTargetDifficultyScore = 60f, maxTargetDifficultyScore = 119.999f }; p.ApplyPresetValues(cfg); Assert.AreEqual(6, p.widthField.value); Assert.AreEqual(99, p.seedField.value); Assert.IsTrue(p.targetTierToggle.value); Assert.AreEqual(FlowDifficultyTier.Normal, p.targetTierField.value); Assert.AreEqual(250, p.pathAttemptField.value); Assert.AreEqual(60f, p.minScoreField.value, 0.001f); Assert.AreEqual(119.999f, p.maxScoreField.value, 0.001f); Assert.AreEqual(100, p.levelAttemptField.value); Assert.IsFalse(p.useRandomSeedToggle.value); }

        // ── Board geometry ──

        [Test] public void Geometry_NegPadding_Throws() => Assert.Throws<ArgumentOutOfRangeException>(() => FlowBoardViewGeometry.CalculateBoardRect(new Rect(0,0,100,100), 5, 5, -1f));
        [Test] public void Geometry_WithinContent() { var r = FlowBoardViewGeometry.CalculateBoardRect(new Rect(0,0,100,60), 10, 5, 0f); Assert.IsTrue(r.width <= 100f); }
        [Test] public void Geometry_YInverted() { var br = new Rect(0,0,50,60); var tl = FlowBoardViewGeometry.GetCellRect(br, 5, 3, new FlowPos(0,0)); Assert.AreEqual(40f, tl.y, 0.01f); }
        [Test] public void Geometry_RoundTrip() { var br = new Rect(0,0,100,100); var cell = new FlowPos(2,1); var cr = FlowBoardViewGeometry.GetCellRect(br,5,5,cell); Assert.IsTrue(FlowBoardViewGeometry.TryGetCell(br,5,5,new Vector2(cr.x+cr.width/2f,cr.y+cr.height/2f),out var r)); Assert.AreEqual(cell,r); }
    }
}
