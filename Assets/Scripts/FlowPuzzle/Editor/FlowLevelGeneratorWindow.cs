using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FlowPuzzle.Application;
using FlowPuzzle.Core;
using FlowPuzzle.Difficulty;
using FlowPuzzle.Editor.Commands;
using FlowPuzzle.Editor.Draft;
using FlowPuzzle.Editor.Persistence;
using FlowPuzzle.Editor.UI;
using FlowPuzzle.Generation;
using FlowPuzzle.Persistence;
using FlowPuzzle.Solving;
using FlowPuzzle.Validation;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace FlowPuzzle.Editor
{
    public sealed class FlowLevelGeneratorWindow : EditorWindow
    {
        // NOTE: internal visibility for test access
        private FlowLevelGenerationService generationService;
        private FlowLevelCompletionService completionService;
        private FlowLevelCompletionService perfectCompletionService;
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
        private CancellationTokenSource completionCts;
        private bool completionRunning;
        private bool completionCancellationRequested;

        [MenuItem("Tools/Flow Puzzle/Level Generator")]
        public static void Open()
        {
            var window = GetWindow<FlowLevelGeneratorWindow>("Flow Puzzle Generator");
            window.minSize = new Vector2(1000f, 650f);
            if (window.position.width < 1000f || window.position.height < 650f)
                window.position = new Rect(window.position.position, new Vector2(1100f, 720f));
            window.Show();
        }
        [SerializeField] private FlowDraftWindowState draftWindowState = new FlowDraftWindowState();

        internal void CaptureDraftWindowState()
        {
            if (draftPanel == null) return;
            draftWindowState.selectedColorId = (int)draftPanel.selectedColorField.value;
            draftWindowState.selectedTool = (FlowDraftEditTool)draftPanel.toolField.value;
            draftWindowState.isEndpointA = draftPanel.endpointToggle.value;
            draftWindowState.saveAsName = draftPanel.saveAsNameField.value;
            draftWindowState.loadedAssetGuid = loadedAsset != null ? AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(loadedAsset)) : null;
            draftWindowState.draftJson = currentDraft != null ? JsonUtility.ToJson(currentDraft) : null;
        }

        internal void RestoreDraftWindowState()
        {
            commandHistory = new FlowEditorCommandHistory();
            var s = draftWindowState;
            if (draftPanel == null || s == null) return;
            draftPanel.selectedColorField.value = s.selectedColorId;
            draftPanel.toolField.value = s.selectedTool == FlowDraftEditTool.MoveEndpoint
                ? FlowDraftEditTool.PlaceEndpoint
                : s.selectedTool;
            draftPanel.endpointToggle.value = s.isEndpointA;
            draftPanel.saveAsNameField.value = s.saveAsName ?? "";
            if (!string.IsNullOrEmpty(s.loadedAssetGuid))
            {
                var path = AssetDatabase.GUIDToAssetPath(s.loadedAssetGuid);
                if (!string.IsNullOrEmpty(path))
                {
                    loadedAsset = AssetDatabase.LoadAssetAtPath<FlowPuzzle.Persistence.FlowLevelAsset>(path);
                    draftPanel.assetField.value = loadedAsset;
                }
                else { loadedAsset = null; draftPanel.assetField.value = null; }
            }
            else { loadedAsset = null; draftPanel.assetField.value = null; }
            if (!string.IsNullOrEmpty(s.draftJson))
            {
                try { currentDraft = JsonUtility.FromJson<FlowLevelDraft>(s.draftJson); }
                catch { currentDraft = null; }
            }
            else currentDraft = null;
            if (currentDraft != null) { boardView.SetData(currentDraft); }
            else { boardView.ClearData(); }
            draftPanel.UpdateDraftState(currentDraft, commandHistory);
            UpdateButtonStates();
        }

        internal void CreateGUI()
        {
            rootVisualElement.Clear();
            minSize = new Vector2(1000f, 650f);

            generationService = new FlowLevelGenerationService();
            var validator = new FlowSolutionValidator();
            var evaluator = new FlowDifficultyEvaluator();
            completionService = new FlowLevelCompletionService(new LocalExactCompletionProvider(), validator, evaluator);
            perfectCompletionService = new FlowLevelCompletionService(new FlowPerfectCompletionProvider(evaluator), validator, evaluator);
            repository = new FlowLevelAssetRepository(validator, evaluator);
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
            boardContainer.tooltip = "Drop one Flow Level Asset here to load it as an editable Draft.";
            boardContainer.RegisterCallback<DragUpdatedEvent>(OnBoardDragUpdated);
            boardContainer.RegisterCallback<DragPerformEvent>(OnBoardDragPerform);
            boardContainer.RegisterCallback<DragLeaveEvent>(_ => SetBoardDropHighlight(false));

            resultPanel = new FlowResultPanel(); resultPanel.Build(rootVisualElement.Q("result-panel"));
            diagnosticsPanel = new FlowDiagnosticsPanel(); diagnosticsPanel.Build(rootVisualElement.Q("result-panel"));
            diagnosticsPanel.retrySameSeedButton.clicked += () => OnRetry(sameSeed: true);
            diagnosticsPanel.retryNewSeedButton.clicked += () => OnRetry(sameSeed: false);
            diagnosticsPanel.OnApplySuggestionClicked += OnApplySuggestion;
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
            RestoreDraftWindowState();
        }

        private void OnDisable()
        {
            completionCts?.Cancel();
            completionCts?.Dispose();
            completionCts = null;
            completionRunning = false;
            CaptureDraftWindowState();
        }

        private void RefreshDraft(bool captureState = true)
        {
            if (captureState) CaptureDraftWindowState();
            boardView.SetData(currentDraft);
            if (currentDraft == null || !currentDraft.HasCompleteEndpoints
                || currentDraft.currentSolution == null || currentDraft.currentDifficulty == null
                || currentDraft.isSolutionDirty || !currentDraft.isValidated)
            {
                currentLevel = null;
                resultPanel.Clear();
            }
            else
            {
                currentLevel = FlowDraftMapper.ToGeneratedLevel(currentDraft);
                resultPanel.Show(currentLevel);
            }
            draftPanel.UpdateDraftState(currentDraft, commandHistory);
            UpdateButtonStates();
        }

        private FlowGenerationConfig ReadConfig() => paramPanel.ReadConfig();

        // Draft action handlers (internal for testability)
        internal void DoNewDraft()
        {
            currentDraft = new FlowLevelDraft { width = paramPanel.widthField.value, height = paramPanel.heightField.value, colorCount = paramPanel.colorCountField.value, levelId = paramPanel.levelIdField.value, seed = paramPanel.seedField.value };
            for (int i = 0; i < currentDraft.colorCount; i++) currentDraft.pairs.Add(new FlowDraftPairData { colorId = i });
            commandHistory.Clear(); loadedAsset = null;
            RefreshDraft();
        }
        internal void DoLoadDraft()
        {
            var asset = draftPanel.assetField.value as FlowPuzzle.Persistence.FlowLevelAsset;
            if (asset == null) { diagnosticsPanel.ShowError("No asset selected."); return; }
            LoadDraftAsset(asset);
        }

        private void LoadDraftAsset(FlowLevelAsset asset)
        {
            currentDraft = FlowDraftMapper.FromAsset(asset); loadedAsset = asset; commandHistory.Clear();
            draftPanel.assetField.value = asset;
            RefreshDraft();
        }

        internal bool DoLoadDroppedAssets(UnityEngine.Object[] objects)
        {
            if (objects == null || objects.Length != 1 || !(objects[0] is FlowLevelAsset asset))
            {
                diagnosticsPanel.ShowError("Drop exactly one Flow Level Asset onto the board.");
                return false;
            }

            LoadDraftAsset(asset);
            diagnosticsPanel.ShowInfo($"Loaded: {asset.name}");
            return true;
        }

        private void OnBoardDragUpdated(DragUpdatedEvent evt)
        {
            var canLoad = HasSingleDroppedAsset(DragAndDrop.objectReferences);
            DragAndDrop.visualMode = canLoad ? DragAndDropVisualMode.Copy : DragAndDropVisualMode.Rejected;
            SetBoardDropHighlight(canLoad);
            evt.StopPropagation();
        }

        private void OnBoardDragPerform(DragPerformEvent evt)
        {
            var objects = DragAndDrop.objectReferences;
            if (HasSingleDroppedAsset(objects))
                DragAndDrop.AcceptDrag();
            DoLoadDroppedAssets(objects);
            SetBoardDropHighlight(false);
            evt.StopPropagation();
        }

        private static bool HasSingleDroppedAsset(UnityEngine.Object[] objects)
            => objects != null && objects.Length == 1 && objects[0] is FlowLevelAsset;

        private void SetBoardDropHighlight(bool enabled)
        {
            boardContainer?.EnableInClassList("flow-asset-drag-hover", enabled);
        }
        internal void DoAddColor()
        {
            if (currentDraft == null) return;
            var before = currentDraft.Clone(); var r = currentDraft.AddColor();
            if (r.success) { var newColorId = currentDraft.pairs.Max(p => p.colorId); commandHistory.Execute(new FlowSnapshotCommand(currentDraft, before, currentDraft.Clone(), "Add Color")); RefreshDraft(); draftPanel.SelectColor(newColorId); CaptureDraftWindowState(); } else diagnosticsPanel.ShowError(r.errorMessage);
        }
        internal void DoRemoveColor()
        {
            if (currentDraft == null) return;
            int cid = (int)draftPanel.selectedColorField.value;
            var before = currentDraft.Clone(); var r = currentDraft.RemoveColor(cid);
            if (r.success) { commandHistory.Execute(new FlowSnapshotCommand(currentDraft, before, currentDraft.Clone(), $"Remove Color {cid}")); RefreshDraft(); } else diagnosticsPanel.ShowError(r.errorMessage);
        }
        internal void DoEndpointEdit(FlowPos pos)
        {
            if (currentDraft == null) return;
            int cid = (int)draftPanel.selectedColorField.value; bool isA = draftPanel.endpointToggle.value;
            var tool = (FlowDraftEditTool)draftPanel.toolField.value;

            if (tool == FlowDraftEditTool.DrawConstraint)
            {
                var constraint = currentDraft.GetConstraint(cid);
                var clickedIndex = constraint?.cells?.FindIndex(cell => cell.Equals(pos)) ?? -1;
                var colorName = FlowEditorColorPalette.GetDisplayName(cid);
                if (clickedIndex < 0)
                {
                    var pair = currentDraft.GetPair(cid);
                    if ((pair?.endpointA.HasValue == true && pair.endpointA.Value.Equals(pos))
                        || (pair?.endpointB.HasValue == true && pair.endpointB.Value.Equals(pos)))
                    {
                        diagnosticsPanel.ShowInfo($"Drag from this endpoint to draw the {colorName} constraint.");
                        return;
                    }
                    diagnosticsPanel.ShowError($"Cannot rewind the {colorName} constraint: the clicked cell ({pos.x}, {pos.y}) is not part of its current constraint. Drag from endpoint A or B to draw or replace it.");
                    return;
                }
                if (clickedIndex == constraint.cells.Count - 1)
                {
                    diagnosticsPanel.ShowInfo($"The {colorName} constraint already ends at ({pos.x}, {pos.y}).");
                    return;
                }

                var erase = new DrawConstraintStrokeCommand(currentDraft, cid, new List<FlowPos> { pos }, true);
                if (commandHistory.Execute(erase))
                {
                    RefreshDraft();
                    diagnosticsPanel.ShowInfo(clickedIndex == 0
                        ? $"Cleared the {colorName} constraint."
                        : $"Rewound the {colorName} constraint to ({pos.x}, {pos.y}).");
                }
                else diagnosticsPanel.ShowError($"Cannot rewind the {colorName} constraint: {erase.LastErrorMessage}");
                return;
            }

            MoveEndpointCommand cmd = null;
            if (tool == FlowDraftEditTool.PlaceEndpoint || tool == FlowDraftEditTool.MoveEndpoint)
                cmd = MoveEndpointCommand.Place(currentDraft, cid, isA, pos);
            else if (tool == FlowDraftEditTool.RemoveEndpoint) { var p = currentDraft.GetPair(cid); var t = isA ? p?.endpointA : p?.endpointB; if (t.HasValue && t.Value.Equals(pos)) cmd = MoveEndpointCommand.Remove(currentDraft, cid, isA); }
            if (cmd != null) { if (commandHistory.Execute(cmd)) { RefreshDraft(); diagnosticsPanel.Clear(); } else diagnosticsPanel.ShowError(string.IsNullOrEmpty(cmd.LastErrorMessage) ? $"Failed to execute {tool} on Color {cid}." : cmd.LastErrorMessage); }
        }        internal void DoUndo() { if (commandHistory.CanUndo) { commandHistory.Undo(); RefreshDraft(); diagnosticsPanel.Clear(); } }
        internal void DoRedo() { if (commandHistory.CanRedo) { commandHistory.Redo(); RefreshDraft(); diagnosticsPanel.Clear(); } }
        internal void DoConstraintStroke(IReadOnlyList<FlowPos> cells)
        {
            if (currentDraft == null || cells == null || cells.Count == 0) return;
            int cid = (int)draftPanel.selectedColorField.value;
            var tool = (FlowDraftEditTool)draftPanel.toolField.value;
            bool isErase = tool == FlowDraftEditTool.EraseConstraint;
            if (tool != FlowDraftEditTool.DrawConstraint && tool != FlowDraftEditTool.EraseConstraint) return;
            var cmd = new DrawConstraintStrokeCommand(currentDraft, cid, cells.ToList(), isErase);
            if (commandHistory.Execute(cmd)) { RefreshDraft(); diagnosticsPanel.ShowInfo($"Updated Color {cid}'s fixed constraint."); }
            else diagnosticsPanel.ShowError($"{(isErase ? "Erase" : "Draw")} constraint failed for Color {cid}: {cmd.LastErrorMessage}");
        }
        private void WireDraftPanel()
        {
            draftPanel.newDraftBtn.clicked += DoNewDraft;
            draftPanel.loadAssetBtn.clicked += DoLoadDraft;
            draftPanel.addColorBtn.clicked += DoAddColor;
            draftPanel.removeColorBtn.clicked += DoRemoveColor;
            draftPanel.undoBtn.clicked += DoUndo;
            draftPanel.redoBtn.clicked += DoRedo;
            draftPanel.completeBtn.clicked += DoCompleteDraft;
            draftPanel.perfectCompleteBtn.clicked += DoPerfectCompleteDraft;
            draftPanel.stopSolvingBtn.clicked += DoStopSolving;
            boardView.CellSelected += DoEndpointEdit;
            boardView.CellStrokeCompleted += DoConstraintStroke;
            draftPanel.saveBtn.clicked += DoSaveDraft;
            draftPanel.saveAsBtn.clicked += DoSaveDraftAs;
        }

        private bool ConfigValid(FlowGenerationConfig c)
        {
            return c.width > 0 && c.height > 0 && c.colorCount > 0
                && c.minPathLength >= 2 && c.minPathLength <= c.maxPathLength
                && c.minCoverageRatio <= c.maxCoverageRatio
                && c.minCoverageRatio >= 0f && c.maxCoverageRatio <= 1f
                && c.maxPathAttempt > 0 && c.maxLevelAttempt > 0;
        }

        private static string GetDraftSaveBlockReason(FlowLevelDraft draft)
        {
            if (draft == null) return "Cannot save: no Draft exists. Generate a level, create a New Draft, or Load Asset first.";
            var missing = new List<string>();
            if (!draft.HasCompleteEndpoints) missing.Add("one or more colors are missing endpoint A or B");
            if (draft.currentSolution == null) missing.Add("there is no recommended solution; click Complete");
            if (draft.currentDifficulty == null) missing.Add("difficulty has not been evaluated; click Complete");
            if (draft.isSolutionDirty) missing.Add("the solution is out of date after editing; click Complete again");
            if (!draft.isValidated) missing.Add("the current solution has not passed validation; click Complete");
            return missing.Count == 0 ? null : "Cannot save Draft:\n- " + string.Join("\n- ", missing);
        }

        internal void DoSaveDraft()
        {
            var blockReason = GetDraftSaveBlockReason(currentDraft);
            if (blockReason != null) { diagnosticsPanel.ShowError(blockReason); return; }
            try
            {
                var level = FlowDraftMapper.ToGeneratedLevel(currentDraft);
                if (loadedAsset != null) repository.Overwrite(loadedAsset, level);
                else { loadedAsset = repository.SaveNew(level, paramPanel.outputFolderField.value); draftPanel.assetField.value = loadedAsset; }
                diagnosticsPanel.ShowInfo($"Saved: {AssetDatabase.GetAssetPath(loadedAsset)}");
                RefreshDraft();
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException) { diagnosticsPanel.ShowError($"Save failed: {ex.Message}"); }
        }
        internal async void DoCompleteDraft()
        {
            try
            {
                await DoCompleteDraftAsync();
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                completionRunning = false;
                completionCts?.Dispose();
                completionCts = null;
                diagnosticsPanel.ShowError($"Completion failed: {ex.Message}");
                UpdateButtonStates();
            }
        }

        internal async void DoPerfectCompleteDraft()
        {
            try
            {
                await DoPerfectCompleteDraftAsync();
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                ResetCompletionState();
                diagnosticsPanel.ShowError($"Perfect completion failed: {ex.Message}");
                UpdateButtonStates();
            }
        }

        internal async Task<FlowCompletionResult> DoCompleteDraftAsync()
        {
            return await RunCompletionAsync(completionService, false);
        }

        internal async Task<FlowCompletionResult> DoPerfectCompleteDraftAsync()
        {
            return await RunCompletionAsync(perfectCompletionService, true);
        }

        internal void DoStopSolving()
        {
            if (!completionRunning || completionCts == null)
            {
                diagnosticsPanel.ShowInfo("No completion search is running.");
                return;
            }

            completionCancellationRequested = true;
            diagnosticsPanel.ShowInfo("Stopping solver... The current Draft will remain unchanged.");
            UpdateButtonStates();
            completionCts.Cancel();
        }

        private async Task<FlowCompletionResult> RunCompletionAsync(
            FlowLevelCompletionService service,
            bool perfect)
        {
            if (currentDraft == null)
            {
                diagnosticsPanel.ShowError("No draft to complete.");
                return new FlowCompletionResult { status = FlowSolveStatus.InvalidInput };
            }
            if (!currentDraft.HasCompleteEndpoints)
            {
                diagnosticsPanel.ShowError("Draft endpoints are incomplete.");
                return new FlowCompletionResult { status = FlowSolveStatus.InvalidInput };
            }
            if (completionRunning)
            {
                diagnosticsPanel.ShowError("Completion already running.");
                return new FlowCompletionResult { status = FlowSolveStatus.Error };
            }
            if (!perfect && !currentDraft.isSolutionDirty && currentDraft.isValidated
                && currentDraft.currentSolution != null && currentDraft.currentDifficulty != null)
            {
                var completedLevel = FlowDraftMapper.ToGeneratedLevel(currentDraft);
                currentLevel = completedLevel;
                RefreshDraft();
                diagnosticsPanel.ShowInfo("Draft is already complete and validated; the existing recommendation was kept.");
                return new FlowCompletionResult
                {
                    status = FlowSolveStatus.Solved,
                    generatedLevel = completedLevel
                };
            }

            completionRunning = true;
            completionCancellationRequested = false;
            completionCts?.Dispose();
            completionCts = new CancellationTokenSource();
            UpdateButtonStates();
            diagnosticsPanel.ShowInfo(perfect
                ? "Perfect Complete is comparing legal candidates against the current parameters..."
                : "Completing draft with local solver...");

            try
            {
                var completingDraft = currentDraft;
                var completionInput = currentDraft.Clone();
                var request = BuildCompletionRequest(currentDraft);
                var progress = new Progress<FlowCompletionProgress>(ReportCompletionProgress);
                var result = await service.CompleteAsync(request, progress, completionCts.Token);
                if (!ReferenceEquals(currentDraft, completingDraft)
                    || !HasSameCompletionInput(currentDraft, completionInput))
                {
                    const string message = "The Draft changed while completion was running; the stale result was ignored.";
                    diagnosticsPanel.ShowError(message);
                    return new FlowCompletionResult
                    {
                        status = FlowSolveStatus.Cancelled,
                        visitedNodes = result?.visitedNodes ?? 0,
                        elapsedMs = result?.elapsedMs ?? 0,
                        errorCode = "DraftChanged",
                        errorMessage = message
                    };
                }
                if (perfect) ApplyPerfectCompletionResult(result);
                else ApplyCompletionResult(result);
                return result;
            }
            finally
            {
                ResetCompletionState();
                RefreshDraft();
            }
        }

        private void ResetCompletionState()
        {
            completionRunning = false;
            completionCancellationRequested = false;
            completionCts?.Dispose();
            completionCts = null;
        }

        private static bool HasSameCompletionInput(FlowLevelDraft current, FlowLevelDraft original)
        {
            if (current == null || original == null
                || current.levelId != original.levelId
                || current.width != original.width
                || current.height != original.height
                || current.colorCount != original.colorCount
                || current.seed != original.seed
                || current.pairs.Count != original.pairs.Count
                || current.fixedConstraints.Count != original.fixedConstraints.Count)
                return false;

            var currentPairs = current.pairs.OrderBy(pair => pair.colorId).ToList();
            var originalPairs = original.pairs.OrderBy(pair => pair.colorId).ToList();
            for (var i = 0; i < currentPairs.Count; i++)
            {
                if (currentPairs[i].colorId != originalPairs[i].colorId
                    || !Nullable.Equals(currentPairs[i].endpointA, originalPairs[i].endpointA)
                    || !Nullable.Equals(currentPairs[i].endpointB, originalPairs[i].endpointB))
                    return false;
            }

            var currentConstraints = current.fixedConstraints.OrderBy(constraint => constraint.colorId).ToList();
            var originalConstraints = original.fixedConstraints.OrderBy(constraint => constraint.colorId).ToList();
            for (var i = 0; i < currentConstraints.Count; i++)
            {
                if (currentConstraints[i].colorId != originalConstraints[i].colorId)
                    return false;
                var currentCells = currentConstraints[i].cells ?? new List<FlowPos>();
                var originalCells = originalConstraints[i].cells ?? new List<FlowPos>();
                if (!currentCells.SequenceEqual(originalCells))
                    return false;
            }

            return true;
        }

        private FlowCompletionRequest BuildCompletionRequest(FlowLevelDraft draft)
        {
            var level = new FlowLevelData { levelId = draft.levelId, width = draft.width, height = draft.height };
            foreach (var pair in draft.pairs.OrderBy(p => p.colorId))
            {
                if (!pair.endpointA.HasValue || !pair.endpointB.HasValue)
                    continue;

                level.pairs.Add(new FlowPairData
                {
                    colorId = pair.colorId,
                    endpointA = new FlowPos(pair.endpointA.Value.x, pair.endpointA.Value.y),
                    endpointB = new FlowPos(pair.endpointB.Value.x, pair.endpointB.Value.y)
                });
            }

            var fixedPrefixes = new List<FlowPathData>();
            foreach (var constraint in draft.fixedConstraints.OrderBy(c => c.colorId))
            {
                if (constraint.cells == null || constraint.cells.Count == 0)
                    continue;

                var path = new FlowPathData { colorId = constraint.colorId, cells = new List<FlowPos>() };
                foreach (var cell in constraint.cells)
                    path.cells.Add(new FlowPos(cell.x, cell.y));
                fixedPrefixes.Add(path);
            }

            return new FlowCompletionRequest
            {
                levelData = level,
                currentSolution = !draft.isSolutionDirty && draft.isValidated
                    ? CloneSolution(draft.currentSolution)
                    : null,
                fixedPrefixes = fixedPrefixes,
                qualityConfig = ReadConfig(),
                nodeBudget = ReadSolverNodeBudget(),
                timeoutMs = ReadSolverTimeoutMs(),
                progressIntervalNodes = FlowSolveRequest.DefaultProgressIntervalNodes
            };
        }

        private long ReadSolverNodeBudget()
        {
            var value = paramPanel?.solverNodeBudgetField?.value ?? 0;
            return value > 0 ? value : FlowSolveRequest.DefaultNodeBudget;
        }

        private int ReadSolverTimeoutMs()
        {
            var value = paramPanel?.solverTimeoutField?.value ?? 0;
            return value > 0 ? value : FlowSolveRequest.DefaultTimeoutMs;
        }

        private void ApplyCompletionResult(FlowCompletionResult result)
        {
            ApplyCompletionResultInternal(result, false);
        }

        private void ApplyPerfectCompletionResult(FlowCompletionResult result)
        {
            ApplyCompletionResultInternal(result, true);
        }

        private void ApplyCompletionResultInternal(FlowCompletionResult result, bool perfect)
        {
            if (result == null)
            {
                diagnosticsPanel.ShowError("Completion failed: the completion provider returned no result.");
                return;
            }

            if (result.status != FlowSolveStatus.Solved || result.generatedLevel == null)
            {
                if (result.status == FlowSolveStatus.Cancelled)
                {
                    diagnosticsPanel.ShowInfo(
                        $"{(perfect ? "Perfect Complete" : "Complete")} stopped. The Draft was not changed and no candidate was applied.");
                    return;
                }

                var code = string.IsNullOrEmpty(result.errorCode) ? result.status.ToString() : result.errorCode;
                var detail = string.IsNullOrEmpty(result.errorMessage) ? GetCompletionStatusHelp(result.status) : result.errorMessage;
                var prefix = perfect ? "Perfect completion" : "Completion";
                diagnosticsPanel.ShowError($"{prefix} failed [{code}]: {detail}\nVisited nodes: {result.visitedNodes}; elapsed: {result.elapsedMs} ms.");
                return;
            }

            if (currentDraft == null)
            {
                diagnosticsPanel.ShowError("Completion failed: the Draft no longer exists.");
                return;
            }

            var before = currentDraft.Clone();
            var after = before.Clone();
            after.currentSolution = CloneSolution(result.generatedLevel.solutionData);
            after.currentDifficulty = CloneDifficulty(result.generatedLevel.difficultyReport);
            after.coverage = result.generatedLevel.coverageRatio;
            after.isSolutionDirty = false;
            after.isValidated = true;
            commandHistory.Execute(new FlowSnapshotCommand(
                currentDraft,
                before,
                after,
                perfect ? "Perfect Complete Draft" : "Complete Draft"));
            RefreshDraft();
            if (perfect)
            {
                var limitNote = result.searchLimitReached
                    ? " Best candidate found before the solver limit was reached."
                    : " Candidate search completed.";
                diagnosticsPanel.ShowInfo(
                    $"Perfect Complete selected the best of {result.candidateCount} candidate(s). "
                    + $"Coverage {result.generatedLevel.coverageRatio:0.###}, quality score {result.candidateScore:0.###}, "
                    + $"path cells {result.candidateTotalPathCells}, detour {result.candidateTotalDetour}, turns {result.candidateTotalTurns}, "
                    + $"visited {result.visitedNodes} nodes.{limitNote}");
            }
            else
            {
                diagnosticsPanel.ShowInfo($"Completed. Coverage {result.generatedLevel.coverageRatio:0.###}, visited {result.visitedNodes} nodes.");
            }
        }

        private static string GetCompletionStatusHelp(FlowSolveStatus status)
        {
            switch (status)
            {
                case FlowSolveStatus.NoSolution: return "No legal solution exists. Move endpoints or remove/redraw fixed constraints.";
                case FlowSolveStatus.Timeout: return "The solver reached its time or node limit. Increase Solver Timeout/Budget or simplify the Draft.";
                case FlowSolveStatus.Cancelled: return "Completion was cancelled.";
                case FlowSolveStatus.InvalidInput: return "The Draft has invalid dimensions, endpoints, or fixed constraints.";
                default: return "The local solver encountered an internal error.";
            }
        }
        private static FlowSolutionData CloneSolution(FlowSolutionData source)
        {
            if (source == null)
                return null;

            var copy = new FlowSolutionData { levelId = source.levelId };
            if (source.paths != null)
            {
                foreach (var path in source.paths)
                {
                    if (path == null)
                        continue;

                    var pathCopy = new FlowPathData { colorId = path.colorId, cells = new List<FlowPos>() };
                    if (path.cells != null)
                    {
                        foreach (var cell in path.cells)
                            pathCopy.cells.Add(new FlowPos(cell.x, cell.y));
                    }
                    copy.paths.Add(pathCopy);
                }
            }

            return copy;
        }

        private static FlowDifficultyReport CloneDifficulty(FlowDifficultyReport source)
        {
            if (source == null)
                return null;

            return new FlowDifficultyReport
            {
                difficulty = source.difficulty,
                totalScore = source.totalScore,
                boardSizeScore = source.boardSizeScore,
                colorCountScore = source.colorCountScore,
                coverageScore = source.coverageScore,
                turnScore = source.turnScore,
                detourScore = source.detourScore,
                interactionScore = source.interactionScore,
                endpointDistanceScore = source.endpointDistanceScore,
                bottleneckScore = source.bottleneckScore,
                totalTurnCount = source.totalTurnCount,
                totalDetour = source.totalDetour,
                differentColorAdjacentCount = source.differentColorAdjacentCount,
                totalEndpointManhattanDistance = source.totalEndpointManhattanDistance,
                bottleneckCount = source.bottleneckCount
            };
        }
        internal void DoSaveDraftAs()
        {
            var blockReason = GetDraftSaveBlockReason(currentDraft);
            if (blockReason != null) { diagnosticsPanel.ShowError(blockReason); return; }
            var name = draftPanel.saveAsNameField.value;
            if (string.IsNullOrWhiteSpace(name)) name = $"Level_{currentDraft.levelId}";
            try
            {
                var level = FlowDraftMapper.ToGeneratedLevel(currentDraft);
                var asset = loadedAsset != null ? repository.SaveAs(loadedAsset, level, paramPanel.outputFolderField.value, name) : repository.SaveAs(level, paramPanel.outputFolderField.value, name);
                loadedAsset = asset; draftPanel.assetField.value = asset;
                diagnosticsPanel.ShowInfo($"Saved As: {AssetDatabase.GetAssetPath(asset)}");
                RefreshDraft();
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException) { diagnosticsPanel.ShowError($"Save As failed: {ex.Message}"); }
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
                if (result.success) { SetCurrentLevel(result.generatedLevel); diagnosticsPanel.ShowInfo("Generated successfully. This result is already loaded as an editable Draft; adjust it directly or save it now."); }
                else diagnosticsPanel.ShowDiagnostic(result.diagnostic);
            }
            catch (InvalidOperationException ex) { diagnosticsPanel.ShowError($"Error: {ex.Message}"); }
        }
        internal void OnRetry(bool sameSeed)
        {
            var config = ReadConfig();
            if (!sameSeed) config.useRandomSeed = true;
            if (!ConfigValid(config)) { diagnosticsPanel.ShowError("Invalid configuration."); return; }
            try
            {
                var result = generationService.GenerateOne(paramPanel.levelIdField.value, config);
                if (result.success) { SetCurrentLevel(result.generatedLevel); diagnosticsPanel.ShowInfo("Generated successfully. This result is already loaded as an editable Draft; adjust it directly or save it now."); }
                else diagnosticsPanel.ShowDiagnostic(result.diagnostic);
            }
            catch (InvalidOperationException ex) { diagnosticsPanel.ShowError($"Error: {ex.Message}"); }
        }
        internal void OnApplySuggestion(FlowParameterSuggestion suggestion)
        {
            if (suggestion == null || string.IsNullOrEmpty(suggestion.parameterName))
                return;

            // Apply the suggestion to the UI fields based on parameter name.
            // Only safe scalar suggestions reach this handler.
            switch (suggestion.parameterName)
            {
                case "width":
                    paramPanel.widthField.value = ClampIntField(paramPanel.widthField,
                        int.TryParse(suggestion.suggestedValue, out var w) ? w : paramPanel.widthField.value, 1, 99);
                    break;
                case "height":
                    paramPanel.heightField.value = ClampIntField(paramPanel.heightField,
                        int.TryParse(suggestion.suggestedValue, out var h) ? h : paramPanel.heightField.value, 1, 99);
                    break;
                case "colorCount":
                    paramPanel.colorCountField.value = ClampIntField(paramPanel.colorCountField,
                        int.TryParse(suggestion.suggestedValue, out var c) ? c : paramPanel.colorCountField.value, 1, 50);
                    break;
                case "minPathLength":
                    paramPanel.minPathLenField.value = ClampIntField(paramPanel.minPathLenField,
                        int.TryParse(suggestion.suggestedValue, out var minP) ? minP : paramPanel.minPathLenField.value, 1, 999);
                    break;
                case "maxPathLength":
                    paramPanel.maxPathLenField.value = ClampIntField(paramPanel.maxPathLenField,
                        int.TryParse(suggestion.suggestedValue, out var maxP) ? maxP : paramPanel.maxPathLenField.value, 1, 999);
                    break;
                case "maxPathAttempt":
                    paramPanel.pathAttemptField.value = ClampIntField(paramPanel.pathAttemptField,
                        int.TryParse(suggestion.suggestedValue, out var pa) ? pa : paramPanel.pathAttemptField.value, 1, 9999);
                    break;
                case "maxLevelAttempt":
                    paramPanel.levelAttemptField.value = ClampIntField(paramPanel.levelAttemptField,
                        int.TryParse(suggestion.suggestedValue, out var la) ? la : paramPanel.levelAttemptField.value, 1, 9999);
                    break;
                case "minCoverageRatio":
                    paramPanel.minCoverageField.value = ClampFloatField(paramPanel.minCoverageField,
                        float.TryParse(suggestion.suggestedValue, out var minC) ? minC : paramPanel.minCoverageField.value, 0f, 1f);
                    break;
                case "maxCoverageRatio":
                    paramPanel.maxCoverageField.value = ClampFloatField(paramPanel.maxCoverageField,
                        float.TryParse(suggestion.suggestedValue, out var maxC) ? maxC : paramPanel.maxCoverageField.value, 0f, 1f);
                    break;
                case "turnPreference":
                    paramPanel.turnPrefField.value = ClampFloatField(paramPanel.turnPrefField,
                        float.TryParse(suggestion.suggestedValue, out var tp) ? tp : paramPanel.turnPrefField.value, 0f, 1f);
                    break;
                case "interactionPreference":
                    paramPanel.interactionPrefField.value = ClampFloatField(paramPanel.interactionPrefField,
                        float.TryParse(suggestion.suggestedValue, out var ip) ? ip : paramPanel.interactionPrefField.value, 0f, 1f);
                    break;
                case "solverTimeoutMilliseconds":
                    paramPanel.solverTimeoutField.value = ClampIntField(paramPanel.solverTimeoutField,
                        int.TryParse(suggestion.suggestedValue, out var st) ? st : paramPanel.solverTimeoutField.value, 100, 300000);
                    break;
                case "solverNodeBudget":
                    paramPanel.solverNodeBudgetField.value = ClampIntField(paramPanel.solverNodeBudgetField,
                        int.TryParse(suggestion.suggestedValue, out var nb) ? nb : paramPanel.solverNodeBudgetField.value, 100, int.MaxValue);
                    break;
                case "minTargetDifficultyScore":
                    paramPanel.minScoreField.value = ClampFloatField(paramPanel.minScoreField,
                        float.TryParse(suggestion.suggestedValue, out var minD) ? minD : paramPanel.minScoreField.value, 0f, 1000f);
                    break;
                case "maxTargetDifficultyScore":
                    paramPanel.maxScoreField.value = ClampFloatField(paramPanel.maxScoreField,
                        float.TryParse(suggestion.suggestedValue, out var maxD) ? maxD : paramPanel.maxScoreField.value, 0f, 1000f);
                    break;
                case "targetDifficulty":
                    // Map string difficulty name back to enum
                    if (System.Enum.TryParse<FlowDifficultyTier>(suggestion.suggestedValue, out var tier))
                        paramPanel.targetTierField.value = tier;
                    break;
                case "seed":
                    // "Retry" direction â?set useRandomSeed
                    paramPanel.useRandomSeedToggle.value = true;
                    break;
            }

            diagnosticsPanel.ShowInfo($"Applied: {suggestion.parameterName} -> {suggestion.suggestedValue}");
        }

        private static int ClampIntField(IntegerField field, int value, int min, int max)
        {
            return Math.Max(min, Math.Min(max, value));
        }

        private static float ClampFloatField(FloatField field, float value, float min, float max)
        {
            return Math.Max(min, Math.Min(max, value));
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
            if (currentLevel == null) { diagnosticsPanel.ShowError("No current result. Generate or complete a level first."); return; }
            try
            {
                var asset = repository.SaveNew(currentLevel, paramPanel.outputFolderField.value);
                diagnosticsPanel.ShowInfo($"Saved: {AssetDatabase.GetAssetPath(asset)}");
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
            else diagnosticsPanel.ShowDiagnostic(r.diagnostic);
        }

        internal void OnValidateCurrent()
        {
            if (currentLevel == null) { diagnosticsPanel.ShowError("No current result."); return; }
            var v = generationService.Validate(currentLevel);
            if (v.isValid) diagnosticsPanel.ShowInfo("Recommendation is valid.");
            else diagnosticsPanel.ShowError($"Invalid: {v.errorCode} â?{v.errorMessage}");
        }

        internal void OnClearPreview()
        {
            currentLevel = null; currentDraft = null;
            boardView.ClearData(); resultPanel.Clear(); diagnosticsPanel.Clear(); batchPanel.Clear();
            CaptureDraftWindowState(); UpdateButtonStates();
        }

        internal void SetCurrentLevel(FlowGeneratedLevel level)
        {
            currentLevel = level;
            currentDraft = FlowDraftMapper.FromGeneratedLevel(level);
            loadedAsset = null; commandHistory.Clear();
            draftPanel.assetField.value = null;
            resultPanel.Show(level);
            RefreshDraft();
        }

        internal void UpdateButtonStates()
        {
            var has = currentLevel != null;
            saveCurrentBtn?.SetEnabled(has); exportJsonBtn?.SetEnabled(has); validateCurrentBtn?.SetEnabled(has);
            var valid = ConfigValid(ReadConfig());
            generateOneBtn?.SetEnabled(valid); generateBatchBtn?.SetEnabled(valid);
            draftPanel?.completeBtn?.SetEnabled(!completionRunning && currentDraft != null && currentDraft.HasCompleteEndpoints);
            draftPanel?.perfectCompleteBtn?.SetEnabled(!completionRunning && currentDraft != null && currentDraft.HasCompleteEndpoints);
            if (draftPanel?.stopSolvingBtn != null)
            {
                draftPanel.stopSolvingBtn.text = completionCancellationRequested ? "Stopping..." : "Stop Solving";
                draftPanel.stopSolvingBtn.SetEnabled(completionRunning && !completionCancellationRequested);
            }
            var hasDiagnostic = diagnosticsPanel?.currentDiagnostic != null;
            diagnosticsPanel?.retrySameSeedButton?.SetEnabled(hasDiagnostic && (generateOneBtn?.enabledSelf ?? false));
            diagnosticsPanel?.retryNewSeedButton?.SetEnabled(hasDiagnostic);
        }

        private void ReportCompletionProgress(FlowCompletionProgress value)
        {
            if (value == null)
                return;

            var candidates = value.candidateCount > 0 ? $", candidates {value.candidateCount}" : string.Empty;
            diagnosticsPanel.ShowInfo($"Completing {value.phase}: color {value.currentColorId}, visited {value.visitedNodes} nodes{candidates}.");
        }
    }
}
