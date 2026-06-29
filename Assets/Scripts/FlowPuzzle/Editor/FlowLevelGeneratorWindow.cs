using System;
using System.Collections.Generic;
using FlowPuzzle.Application;
using FlowPuzzle.Core;
using FlowPuzzle.Difficulty;
using FlowPuzzle.Editor.Commands;
using FlowPuzzle.Editor.Draft;
using FlowPuzzle.Editor.Persistence;
using FlowPuzzle.Editor.UI;
using FlowPuzzle.Generation;
using FlowPuzzle.Persistence;
using FlowPuzzle.Validation;
using UnityEditor;
using UnityEngine.UIElements;

namespace FlowPuzzle.Editor
{
    public sealed class FlowLevelGeneratorWindow : EditorWindow
    {
        // NOTE: internal visibility for test access
        private FlowLevelGenerationService generationService;
        private FlowLevelAssetRepository repository;
        private FlowLevelJsonExporter jsonExporter;

        internal FlowParameterPanel paramPanel;
        internal FlowResultPanel resultPanel;
        internal FlowDiagnosticsPanel diagnosticsPanel;
        internal FlowBatchReportPanel batchPanel;
        internal FlowBoardView boardView;
        internal VisualElement boardContainer;
        internal FlowDraftPanel draftPanel;
        internal FlowLevelDraft currentDraft;
        internal FlowEditorCommandHistory commandHistory = new FlowEditorCommandHistory();
        internal FlowPuzzle.Persistence.FlowLevelAsset loadedAsset;

        internal Button applyPresetBtn, generateOneBtn, generateBatchBtn;
        internal Button saveCurrentBtn, exportJsonBtn, validateCurrentBtn, clearPreviewBtn;

        internal FlowGeneratedLevel currentLevel;
        private List<string> batchSaveErrors;

        [MenuItem("Tools/Flow Puzzle/Level Generator")]
        public static void Open() => GetWindow<FlowLevelGeneratorWindow>("Flow Puzzle Generator");

        internal void CreateGUI()
        {
            rootVisualElement.Clear();

            generationService = new FlowLevelGenerationService();
            var validator = new FlowSolutionValidator();
            repository = new FlowLevelAssetRepository(validator, new FlowDifficultyEvaluator());
            jsonExporter = new FlowLevelJsonExporter();

            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                "Assets/Scripts/FlowPuzzle/Editor/UI/FlowLevelGeneratorWindow.uxml");
            uxml.CloneTree(rootVisualElement);
            var uss = AssetDatabase.LoadAssetAtPath<StyleSheet>(
                "Assets/Scripts/FlowPuzzle/Editor/UI/FlowLevelGeneratorWindow.uss");
            rootVisualElement.styleSheets.Add(uss);

            paramPanel = new FlowParameterPanel();
            paramPanel.Build(rootVisualElement.Q("param-panel"));
            // Live config validation: refresh button states on parameter changes
            var onConfigChanged = (EventCallback<ChangeEvent<int>>)(_ => UpdateButtonStates());
            paramPanel.widthField.RegisterValueChangedCallback(onConfigChanged);
            paramPanel.heightField.RegisterValueChangedCallback(onConfigChanged);
            paramPanel.colorCountField.RegisterValueChangedCallback(onConfigChanged);
            paramPanel.minPathLenField.RegisterValueChangedCallback(onConfigChanged);
            paramPanel.maxPathLenField.RegisterValueChangedCallback(onConfigChanged);
            paramPanel.pathAttemptField.RegisterValueChangedCallback(onConfigChanged);
            paramPanel.levelAttemptField.RegisterValueChangedCallback(onConfigChanged);
            var onFloatChanged = (EventCallback<ChangeEvent<float>>)(_ => UpdateButtonStates());
            paramPanel.minCoverageField.RegisterValueChangedCallback(onFloatChanged);
            paramPanel.maxCoverageField.RegisterValueChangedCallback(onFloatChanged);

            boardContainer = rootVisualElement.Q("board-container");
            boardView = new FlowBoardView { style = { flexGrow = 1f } };
            boardContainer.Add(boardView);

            resultPanel = new FlowResultPanel(); resultPanel.Build(rootVisualElement.Q("result-panel"));
            diagnosticsPanel = new FlowDiagnosticsPanel(); diagnosticsPanel.Build(rootVisualElement.Q("result-panel"));
            batchPanel = new FlowBatchReportPanel(); batchPanel.Build(rootVisualElement.Q("batch-panel"));
            draftPanel = new FlowDraftPanel(); draftPanel.Build(rootVisualElement.Q("result-panel"));
            WireDraftPanel();

            var bar = new VisualElement(); bar.AddToClassList("action-bar"); rootVisualElement.Add(bar);
            void AddBtn(string name, Action a) { var b = new Button(a) { text = name, name = name.ToLower().Replace(" ", "-") }; bar.Add(b); }
            AddBtn("Apply Preset", () => { OnApplyPreset(); UpdateButtonStates(); }); applyPresetBtn = (Button)bar[bar.childCount - 1];
            AddBtn("Generate One", () => OnGenerateOne()); generateOneBtn = (Button)bar[bar.childCount - 1];
            AddBtn("Generate Batch", () => OnGenerateBatch()); generateBatchBtn = (Button)bar[bar.childCount - 1];
            AddBtn("Save Current", () => OnSaveCurrent()); saveCurrentBtn = (Button)bar[bar.childCount - 1];
            AddBtn("Export JSON", () => OnExportJson()); exportJsonBtn = (Button)bar[bar.childCount - 1];
            AddBtn("Validate Current", () => OnValidateCurrent()); validateCurrentBtn = (Button)bar[bar.childCount - 1];
            AddBtn("Clear Preview", () => OnClearPreview()); clearPreviewBtn = (Button)bar[bar.childCount - 1];

            UpdateButtonStates();
        }

        private FlowGenerationConfig ReadConfig() => paramPanel.ReadConfig();

        // Draft action handlers (internal for testability)
        internal void DoNewDraft()
        {
            currentDraft = new FlowLevelDraft { width = paramPanel.widthField.value, height = paramPanel.heightField.value, colorCount = paramPanel.colorCountField.value, levelId = paramPanel.levelIdField.value, seed = paramPanel.seedField.value };
            for (int i = 0; i < currentDraft.colorCount; i++) currentDraft.pairs.Add(new FlowDraftPairData { colorId = i });
            commandHistory.Clear(); loadedAsset = null;
            boardView.SetData(currentDraft); draftPanel.UpdateDraftState(currentDraft, commandHistory); UpdateButtonStates();
        }
        internal void DoLoadDraft()
        {
            var asset = draftPanel.assetField.value as FlowPuzzle.Persistence.FlowLevelAsset;
            if (asset == null) { diagnosticsPanel.ShowError("No asset selected."); return; }
            currentDraft = FlowDraftMapper.FromAsset(asset); loadedAsset = asset; commandHistory.Clear();
            boardView.SetData(currentDraft); draftPanel.UpdateDraftState(currentDraft, commandHistory); UpdateButtonStates();
        }
        internal void DoAddColor()
        {
            if (currentDraft == null) return;
            var before = currentDraft.Clone(); var r = currentDraft.AddColor();
            if (r.success) { commandHistory.Execute(new FlowSnapshotCommand(currentDraft, before, currentDraft.Clone(), "Add Color")); boardView.SetData(currentDraft); draftPanel.UpdateDraftState(currentDraft, commandHistory); } else diagnosticsPanel.ShowError(r.errorMessage);
        }
        internal void DoRemoveColor()
        {
            if (currentDraft == null) return;
            int cid = (int)draftPanel.selectedColorField.value;
            var before = currentDraft.Clone(); var r = currentDraft.RemoveColor(cid);
            if (r.success) { commandHistory.Execute(new FlowSnapshotCommand(currentDraft, before, currentDraft.Clone(), $"Remove Color {cid}")); boardView.SetData(currentDraft); draftPanel.UpdateDraftState(currentDraft, commandHistory); } else diagnosticsPanel.ShowError(r.errorMessage);
        }
        internal void DoEndpointEdit(FlowPos pos)
        {
            if (currentDraft == null) return;
            int cid = (int)draftPanel.selectedColorField.value; bool isA = draftPanel.endpointToggle.value;
            var tool = (FlowDraftEditTool)draftPanel.toolField.value;
            IFlowEditorCommand cmd = null;
            if (tool == FlowDraftEditTool.PlaceEndpoint) cmd = MoveEndpointCommand.Place(currentDraft, cid, isA, pos);
            else if (tool == FlowDraftEditTool.MoveEndpoint) cmd = MoveEndpointCommand.Move(currentDraft, cid, isA, pos);
            else if (tool == FlowDraftEditTool.RemoveEndpoint) { var p = currentDraft.GetPair(cid); var t = isA ? p?.endpointA : p?.endpointB; if (t.HasValue && t.Value.Equals(pos)) cmd = MoveEndpointCommand.Remove(currentDraft, cid, isA); }
            if (cmd != null) { if (commandHistory.Execute(cmd)) { boardView.SetData(currentDraft); draftPanel.UpdateDraftState(currentDraft, commandHistory); UpdateButtonStates(); } else diagnosticsPanel.ShowError($"Failed to execute {tool} on color {cid}."); }
        }
        internal void DoUndo() { if (commandHistory.CanUndo) { commandHistory.Undo(); boardView.SetData(currentDraft); draftPanel.UpdateDraftState(currentDraft, commandHistory); UpdateButtonStates(); } }
        internal void DoRedo() { if (commandHistory.CanRedo) { commandHistory.Redo(); boardView.SetData(currentDraft); draftPanel.UpdateDraftState(currentDraft, commandHistory); UpdateButtonStates(); } }

        private void WireDraftPanel()
        {
            draftPanel.newDraftBtn.clicked += DoNewDraft;
            draftPanel.loadAssetBtn.clicked += DoLoadDraft;
            draftPanel.addColorBtn.clicked += DoAddColor;
            draftPanel.removeColorBtn.clicked += DoRemoveColor;
            draftPanel.undoBtn.clicked += DoUndo;
            draftPanel.redoBtn.clicked += DoRedo;
            boardView.CellSelected += DoEndpointEdit;
            draftPanel.saveBtn.clicked += () =>
            {
                if (currentDraft == null || !currentDraft.HasCompleteEndpoints || currentDraft.isSolutionDirty || !currentDraft.isValidated) { diagnosticsPanel.ShowError("Draft not ready for save."); return; }
                try { var level = FlowDraftMapper.ToGeneratedLevel(currentDraft); if (loadedAsset != null) repository.Overwrite(loadedAsset, level); else repository.SaveNew(level, paramPanel.outputFolderField.value); diagnosticsPanel.ShowInfo("Saved."); } catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException) { diagnosticsPanel.ShowError(ex.Message); }
            };
            draftPanel.saveAsBtn.clicked += () =>
            {
                if (currentDraft == null || !currentDraft.HasCompleteEndpoints || currentDraft.isSolutionDirty || !currentDraft.isValidated) { diagnosticsPanel.ShowError("Draft not ready for Save As."); return; }
                var name = draftPanel.saveAsNameField.value; if (string.IsNullOrWhiteSpace(name)) name = $"Level_{currentDraft.levelId}";
                try { var level = FlowDraftMapper.ToGeneratedLevel(currentDraft); var asset = repository.SaveAs(level, paramPanel.outputFolderField.value, name); loadedAsset = asset; diagnosticsPanel.ShowInfo($"Saved As {name}."); } catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException) { diagnosticsPanel.ShowError(ex.Message); }
            };
        }

        private bool ConfigValid(FlowGenerationConfig c)
        {
            return c.width > 0 && c.height > 0 && c.colorCount > 0
                && c.minPathLength >= 2 && c.minPathLength <= c.maxPathLength
                && c.minCoverageRatio <= c.maxCoverageRatio
                && c.minCoverageRatio >= 0f && c.maxCoverageRatio <= 1f
                && c.maxPathAttempt > 0 && c.maxLevelAttempt > 0;
        }

        private void OnApplyPreset()
        {
            var preset = (FlowDifficultyPreset)paramPanel.presetField.value;
            if (preset == FlowDifficultyPreset.Custom) return;
            var config = new FlowGenerationConfig();
            FlowDifficultyPresetLibrary.Apply(preset, config);
            paramPanel.ApplyPresetValues(config);
        }

        internal void OnGenerateOne()
        {
            var config = ReadConfig();
            if (!ConfigValid(config)) { diagnosticsPanel.ShowError("Invalid configuration."); return; }
            try
            {
                var result = generationService.GenerateOne(paramPanel.levelIdField.value, config);
                if (result.success) { SetCurrentLevel(result.generatedLevel); diagnosticsPanel.Clear(); }
                else diagnosticsPanel.ShowError($"Generation failed: {result.diagnostic?.errorCode}");
            }
            catch (InvalidOperationException ex) { diagnosticsPanel.ShowError($"Error: {ex.Message}"); }
        }

        internal void OnGenerateBatch()
        {
            var config = ReadConfig();
            if (!ConfigValid(config)) { diagnosticsPanel.ShowError("Invalid configuration."); return; }
            batchSaveErrors = new List<string>();
            try
            {
                var req = new FlowBatchRequest
                {
                    startLevelId = paramPanel.levelIdField.value, count = paramPanel.batchCountField.value,
                    baseSeed = paramPanel.seedField.value, config = config
                };
                var report = generationService.GenerateBatch(req);
                foreach (var item in report.items)
                {
                    if (item.success && item.generationResult?.success == true)
                    {
                        try { repository.SaveNew(item.generationResult.generatedLevel, paramPanel.outputFolderField.value); }
                        catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException)
                        {
                            batchSaveErrors.Add($"Level {item.levelId}: {ex.Message}");
                            item.success = false;
                            item.message = $"Save failed: {ex.Message}";
                        }
                    }
                }
                batchPanel.Show(report);
                if (batchSaveErrors.Count > 0)
                    diagnosticsPanel.ShowError($"Batch: {report.successfulCount}/{report.requestedCount} generated, {batchSaveErrors.Count} save failures.");
                else diagnosticsPanel.ShowInfo($"Batch: {report.successfulCount}/{report.requestedCount} succeeded.");
            }
            catch (InvalidOperationException ex) { diagnosticsPanel.ShowError($"Batch error: {ex.Message}"); }
        }

        internal void OnSaveCurrent()
        {
            if (currentLevel == null) { diagnosticsPanel.ShowError("No current result."); return; }
            try
            {
                repository.SaveNew(currentLevel, paramPanel.outputFolderField.value);
                diagnosticsPanel.ShowInfo($"Saved to {paramPanel.outputFolderField.value}");
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException)
            { diagnosticsPanel.ShowError($"Save failed: {ex.Message}"); }
        }

        internal void OnExportJson()
        {
            if (currentLevel == null) { diagnosticsPanel.ShowError("No current result."); return; }
            var folder = paramPanel.outputFolderField.value;
            if (string.IsNullOrWhiteSpace(folder)) folder = "FlowPuzzleExport";
            var r = jsonExporter.Export(currentLevel, folder);
            if (r.success) diagnosticsPanel.ShowInfo($"Exported: {r.levelFilePath}\n{r.solutionFilePath}");
            else diagnosticsPanel.ShowError($"Export failed: {r.diagnostic?.errorCode}");
        }

        internal void OnValidateCurrent()
        {
            if (currentLevel == null) { diagnosticsPanel.ShowError("No current result."); return; }
            var v = generationService.Validate(currentLevel);
            if (v.isValid) diagnosticsPanel.ShowInfo("Recommendation is valid.");
            else diagnosticsPanel.ShowError($"Invalid: {v.errorCode} — {v.errorMessage}");
        }

        internal void OnClearPreview()
        {
            currentLevel = null;
            boardView.ClearData(); resultPanel.Clear(); diagnosticsPanel.Clear(); batchPanel.Clear();
            UpdateButtonStates();
        }

        internal void SetCurrentLevel(FlowGeneratedLevel level)
        {
            currentLevel = level;
            currentDraft = FlowDraftMapper.FromGeneratedLevel(level);
            boardView.SetData(currentDraft);
            resultPanel.Show(level);
            UpdateButtonStates();
        }

        internal void UpdateButtonStates()
        {
            var has = currentLevel != null;
            saveCurrentBtn?.SetEnabled(has); exportJsonBtn?.SetEnabled(has); validateCurrentBtn?.SetEnabled(has);
            var valid = ConfigValid(ReadConfig());
            generateOneBtn?.SetEnabled(valid); generateBatchBtn?.SetEnabled(valid);
        }
    }
}
