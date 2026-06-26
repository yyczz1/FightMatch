using System;
using System.Collections.Generic;
using FlowPuzzle.Application;
using FlowPuzzle.Core;
using FlowPuzzle.Difficulty;
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
        private FlowLevelGenerationService generationService;
        private FlowLevelAssetRepository repository;
        private FlowLevelJsonExporter jsonExporter;

        private FlowParameterPanel paramPanel;
        private FlowResultPanel resultPanel;
        private FlowDiagnosticsPanel diagnosticsPanel;
        private FlowBatchReportPanel batchPanel;
        private FlowBoardView boardView;
        private VisualElement boardContainer;

        private Button applyPresetBtn, generateOneBtn, generateBatchBtn;
        private Button saveCurrentBtn, exportJsonBtn, validateCurrentBtn, clearPreviewBtn;

        private FlowGeneratedLevel currentLevel;
        private List<string> batchSaveErrors;

        [MenuItem("Tools/Flow Puzzle/Level Generator")]
        public static void Open() => GetWindow<FlowLevelGeneratorWindow>("Flow Puzzle Generator");

        private void CreateGUI()
        {
            // Clear old tree to avoid duplication on rebuild
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

            boardContainer = rootVisualElement.Q("board-container");
            boardView = new FlowBoardView { style = { flexGrow = 1f } };
            boardContainer.Add(boardView);

            resultPanel = new FlowResultPanel(); resultPanel.Build(rootVisualElement.Q("result-panel"));
            diagnosticsPanel = new FlowDiagnosticsPanel(); diagnosticsPanel.Build(rootVisualElement.Q("result-panel"));
            batchPanel = new FlowBatchReportPanel(); batchPanel.Build(rootVisualElement.Q("batch-panel"));

            var bar = new VisualElement(); bar.AddToClassList("action-bar"); rootVisualElement.Add(bar);

            void AddBtn(string name, Action a) { var b = new Button(a) { text = name, name = name.ToLower().Replace(" ", "-") }; bar.Add(b); }
            AddBtn("Apply Preset", () => OnApplyPreset()); applyPresetBtn = (Button)bar[bar.childCount - 1];
            AddBtn("Generate One", () => OnGenerateOne()); generateOneBtn = (Button)bar[bar.childCount - 1];
            AddBtn("Generate Batch", () => OnGenerateBatch()); generateBatchBtn = (Button)bar[bar.childCount - 1];
            AddBtn("Save Current", () => OnSaveCurrent()); saveCurrentBtn = (Button)bar[bar.childCount - 1];
            AddBtn("Export JSON", () => OnExportJson()); exportJsonBtn = (Button)bar[bar.childCount - 1];
            AddBtn("Validate Current", () => OnValidateCurrent()); validateCurrentBtn = (Button)bar[bar.childCount - 1];
            AddBtn("Clear Preview", () => OnClearPreview()); clearPreviewBtn = (Button)bar[bar.childCount - 1];

            UpdateButtonStates();
        }

        private FlowGenerationConfig ReadConfig() => paramPanel.ReadConfig();

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

        private void OnGenerateOne()
        {
            var config = ReadConfig();
            if (!ConfigValid(config)) { diagnosticsPanel.ShowError("Invalid configuration — check dimensions, path lengths, and coverage range."); return; }
            try
            {
                var result = generationService.GenerateOne(paramPanel.levelIdField.value, config);
                if (result.success) { SetCurrentLevel(result.generatedLevel); diagnosticsPanel.Clear(); }
                else diagnosticsPanel.ShowError($"Generation failed: {result.diagnostic?.errorCode}");
            }
            catch (InvalidOperationException ex) { diagnosticsPanel.ShowError($"Error: {ex.Message}"); }
        }

        private void OnGenerateBatch()
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
                        catch (InvalidOperationException ex) { batchSaveErrors.Add($"Level {item.levelId} save failed: {ex.Message}"); item.message += " [SAVE FAILED]"; }
                    }
                }
                batchPanel.Show(report);
                if (batchSaveErrors.Count > 0)
                    diagnosticsPanel.ShowError($"Batch: {report.successfulCount}/{report.requestedCount} succeeded. {batchSaveErrors.Count} save failures.");
                else diagnosticsPanel.ShowInfo($"Batch: {report.successfulCount}/{report.requestedCount} succeeded.");
            }
            catch (InvalidOperationException ex) { diagnosticsPanel.ShowError($"Batch error: {ex.Message}"); }
        }

        private void OnSaveCurrent()
        {
            if (currentLevel == null) { diagnosticsPanel.ShowError("No current result."); return; }
            try
            {
                repository.SaveNew(currentLevel, paramPanel.outputFolderField.value);
                diagnosticsPanel.ShowInfo($"Saved to {paramPanel.outputFolderField.value}");
            }
            catch (InvalidOperationException ex) { diagnosticsPanel.ShowError($"Save failed: {ex.Message}"); }
        }

        private void OnExportJson()
        {
            if (currentLevel == null) { diagnosticsPanel.ShowError("No current result."); return; }
            var folder = paramPanel.outputFolderField.value;
            if (string.IsNullOrWhiteSpace(folder)) folder = "FlowPuzzleExport";
            var r = jsonExporter.Export(currentLevel, folder);
            if (r.success) diagnosticsPanel.ShowInfo($"Exported: {r.levelFilePath}\n{r.solutionFilePath}");
            else diagnosticsPanel.ShowError($"Export failed: {r.diagnostic?.errorCode}");
        }

        private void OnValidateCurrent()
        {
            if (currentLevel == null) { diagnosticsPanel.ShowError("No current result."); return; }
            var v = generationService.Validate(currentLevel);
            if (v.isValid) diagnosticsPanel.ShowInfo("Recommendation is valid.");
            else diagnosticsPanel.ShowError($"Invalid: {v.errorCode} — {v.errorMessage}");
        }

        private void OnClearPreview()
        {
            currentLevel = null;
            boardView.ClearData(); resultPanel.Clear(); diagnosticsPanel.Clear(); batchPanel.Clear();
            UpdateButtonStates();
        }

        private void SetCurrentLevel(FlowGeneratedLevel level)
        {
            currentLevel = level;
            boardView.SetData(level.levelData, level.solutionData);
            resultPanel.Show(level);
            UpdateButtonStates();
        }

        private void UpdateButtonStates()
        {
            var has = currentLevel != null;
            saveCurrentBtn?.SetEnabled(has); exportJsonBtn?.SetEnabled(has); validateCurrentBtn?.SetEnabled(has);
            var valid = ConfigValid(ReadConfig());
            generateOneBtn?.SetEnabled(valid); generateBatchBtn?.SetEnabled(valid);
        }
    }
}
