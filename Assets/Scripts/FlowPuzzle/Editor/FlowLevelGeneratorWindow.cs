using System;
using System.IO;
using FlowPuzzle.Application;
using FlowPuzzle.Core;
using FlowPuzzle.Difficulty;
using FlowPuzzle.Editor.Persistence;
using FlowPuzzle.Editor.UI;
using FlowPuzzle.Generation;
using FlowPuzzle.Persistence;
using FlowPuzzle.Validation;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace FlowPuzzle.Editor
{
    public sealed class FlowLevelGeneratorWindow : EditorWindow
    {
        // Services
        private FlowLevelGenerationService generationService;
        private FlowLevelAssetRepository repository;
        private FlowLevelJsonExporter jsonExporter;

        // UI
        private FlowParameterPanel paramPanel;
        private FlowResultPanel resultPanel;
        private FlowDiagnosticsPanel diagnosticsPanel;
        private FlowBatchReportPanel batchPanel;
        private FlowBoardView boardView;
        private VisualElement boardContainer;

        // Buttons
        private Button applyPresetBtn, generateOneBtn, generateBatchBtn;
        private Button saveCurrentBtn, exportJsonBtn, validateCurrentBtn, clearPreviewBtn;

        // State
        private FlowGeneratedLevel currentLevel;
        private FlowDifficultyPreset selectedPreset = FlowDifficultyPreset.Custom;

        [MenuItem("Tools/Flow Puzzle/Level Generator")]
        public static void Open() => GetWindow<FlowLevelGeneratorWindow>("Flow Puzzle Generator");

        private void CreateGUI()
        {
            generationService = new FlowLevelGenerationService();
            repository = new FlowLevelAssetRepository(new FlowSolutionValidator(), new FlowDifficultyEvaluator());
            jsonExporter = new FlowLevelJsonExporter();

            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                "Assets/Scripts/FlowPuzzle/Editor/UI/FlowLevelGeneratorWindow.uxml");
            uxml.CloneTree(rootVisualElement);

            var uss = AssetDatabase.LoadAssetAtPath<StyleSheet>(
                "Assets/Scripts/FlowPuzzle/Editor/UI/FlowLevelGeneratorWindow.uss");
            rootVisualElement.styleSheets.Add(uss);

            // Parameter panel
            var paramRoot = rootVisualElement.Q<VisualElement>("param-panel");
            paramPanel = new FlowParameterPanel();
            paramPanel.Build(paramRoot);

            // Board
            boardContainer = rootVisualElement.Q<VisualElement>("board-container");
            boardView = new FlowBoardView();
            boardView.style.flexGrow = 1f;
            boardContainer.Add(boardView);

            // Result
            var resultRoot = rootVisualElement.Q<VisualElement>("result-panel");
            resultPanel = new FlowResultPanel();
            resultPanel.Build(resultRoot);

            // Diagnostics
            var diagRoot = rootVisualElement.Q<VisualElement>("result-panel");
            diagnosticsPanel = new FlowDiagnosticsPanel();
            diagnosticsPanel.Build(diagRoot);

            // Batch
            var batchRoot = rootVisualElement.Q<VisualElement>("batch-panel");
            batchPanel = new FlowBatchReportPanel();
            batchPanel.Build(batchRoot);

            // Action buttons
            var actionBar = new VisualElement();
            actionBar.AddToClassList("action-bar");
            rootVisualElement.Add(actionBar);

            applyPresetBtn = new Button(() => OnApplyPreset()) { text = "Apply Preset", name = "apply-preset" };
            generateOneBtn = new Button(() => OnGenerateOne()) { text = "Generate One", name = "generate-one" };
            generateBatchBtn = new Button(() => OnGenerateBatch()) { text = "Generate Batch", name = "generate-batch" };
            saveCurrentBtn = new Button(() => OnSaveCurrent()) { text = "Save Current", name = "save-current" };
            exportJsonBtn = new Button(() => OnExportJson()) { text = "Export JSON", name = "export-json" };
            validateCurrentBtn = new Button(() => OnValidateCurrent()) { text = "Validate Current", name = "validate-current" };
            clearPreviewBtn = new Button(() => OnClearPreview()) { text = "Clear Preview", name = "clear-preview" };

            actionBar.Add(applyPresetBtn); actionBar.Add(generateOneBtn); actionBar.Add(generateBatchBtn);
            actionBar.Add(saveCurrentBtn); actionBar.Add(exportJsonBtn);
            actionBar.Add(validateCurrentBtn); actionBar.Add(clearPreviewBtn);

            UpdateButtonStates();
        }

        // ── Actions (shell — P5 will complete these) ──

        private void OnApplyPreset()
        {
            var preset = (FlowDifficultyPreset)paramPanel.presetField.value;
            if (preset == FlowDifficultyPreset.Custom) return;
            var config = new FlowGenerationConfig();
            FlowDifficultyPresetLibrary.Apply(preset, config);
            paramPanel.ApplyPresetValues(config);
            selectedPreset = preset;
        }

        private void OnGenerateOne()
        {
            diagnosticsPanel.ShowInfo("Generate One — not wired (P5)");
        }

        private void OnGenerateBatch()
        {
            diagnosticsPanel.ShowInfo("Generate Batch — not wired (P5)");
        }

        private void OnSaveCurrent()
        {
            if (currentLevel == null) { diagnosticsPanel.ShowError("No current result to save."); return; }
            diagnosticsPanel.ShowInfo("Save — not wired (P5)");
        }

        private void OnExportJson()
        {
            if (currentLevel == null) { diagnosticsPanel.ShowError("No current result to export."); return; }
            diagnosticsPanel.ShowInfo("Export — not wired (P5)");
        }

        private void OnValidateCurrent()
        {
            if (currentLevel == null) { diagnosticsPanel.ShowError("No current result to validate."); return; }
            diagnosticsPanel.ShowInfo("Validate — not wired (P5)");
        }

        private void OnClearPreview()
        {
            currentLevel = null;
            boardView.ClearData();
            resultPanel.Clear();
            diagnosticsPanel.Clear();
            batchPanel.Clear();
            UpdateButtonStates();
        }

        private void UpdateButtonStates()
        {
            var hasResult = currentLevel != null;
            saveCurrentBtn.SetEnabled(hasResult);
            exportJsonBtn.SetEnabled(hasResult);
            validateCurrentBtn.SetEnabled(hasResult);
        }
    }
}
