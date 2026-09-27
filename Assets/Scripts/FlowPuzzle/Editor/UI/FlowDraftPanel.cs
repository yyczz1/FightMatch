using System.Collections.Generic;
using System.Linq;
using FlowPuzzle.Editor.Draft;
using FlowPuzzle.Editor.Commands;
using FlowPuzzle.Persistence;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace FlowPuzzle.Editor.UI
{
    public sealed class FlowDraftPanel
    {
        public Button newDraftBtn, loadAssetBtn, addColorBtn, removeColorBtn;
        public Button undoBtn, redoBtn, saveBtn, saveAsBtn, completeBtn, perfectCompleteBtn, stopSolvingBtn;
        public ObjectField assetField;
        public PopupField<int> selectedColorField;
        public PopupField<bool> endpointToggle;
        public PopupField<FlowDraftEditTool> toolField;
        public TextField saveAsNameField;

        public void Build(VisualElement root)
        {
            root.Add(new HelpBox(
                "Draft workflow: choose a color, choose endpoint A or B, place both endpoints, optionally draw a fixed constraint from an endpoint, then click Complete.",
                HelpBoxMessageType.Info));
            newDraftBtn = new Button { text = "New Draft", name = "new-draft" }; root.Add(newDraftBtn);
            assetField = new ObjectField("Asset") { objectType = typeof(FlowLevelAsset), allowSceneObjects = false }; root.Add(assetField);
            loadAssetBtn = new Button { text = "Load Asset", name = "load-asset" }; root.Add(loadAssetBtn);
            selectedColorField = new PopupField<int>("Color", new List<int> { 0 }, 0, FormatColor, FormatColor) { name = "selected-color" }; root.Add(selectedColorField);
            endpointToggle = new PopupField<bool>("Endpoint", new List<bool> { true, false }, 0, FormatEndpoint, FormatEndpoint)
            {
                tooltip = "A and B are symmetric endpoints, not start and end. A is drawn as a diamond; B as a square."
            };
            root.Add(endpointToggle);
            toolField = new PopupField<FlowDraftEditTool>("Tool", new List<FlowDraftEditTool>
            {
                FlowDraftEditTool.Select,
                FlowDraftEditTool.PlaceEndpoint,
                FlowDraftEditTool.RemoveEndpoint,
                FlowDraftEditTool.DrawConstraint
            }, 0, FormatTool, FormatTool);
            root.Add(toolField);
            addColorBtn = new Button { text = "Add Color", name = "add-color" }; root.Add(addColorBtn);
            removeColorBtn = new Button { text = "Remove Color", name = "remove-color" }; root.Add(removeColorBtn);
            undoBtn = new Button { text = "Undo", name = "undo" }; root.Add(undoBtn);
            redoBtn = new Button { text = "Redo", name = "redo" }; root.Add(redoBtn);
            saveAsNameField = new TextField("Save As Name")
            {
                value = "",
                tooltip = "Optional. Leave blank to use Level_<Level ID>. Save As keeps an explicitly entered name."
            };
            completeBtn = new Button { text = "Complete", name = "complete" };
            completeBtn.SetEnabled(false);
            completeBtn.tooltip = "Fast completion: keep the first legal solution found for the endpoints and optional fixed constraints.";
            root.Add(completeBtn);
            perfectCompleteBtn = new Button { text = "Perfect Complete", name = "perfect-complete" };
            perfectCompleteBtn.SetEnabled(false);
            perfectCompleteBtn.tooltip = "Quality completion: compare legal candidates against the current parameters and keep the best one found within the Solver Timeout and Solver Budget.";
            root.Add(perfectCompleteBtn);
            stopSolvingBtn = new Button { text = "Stop Solving", name = "stop-solving" };
            stopSolvingBtn.SetEnabled(false);
            stopSolvingBtn.tooltip = "Cancel the active completion. The current Draft remains unchanged and no partial candidate is applied.";
            root.Add(stopSolvingBtn);
            root.Add(saveAsNameField);
            saveBtn = new Button { text = "Save", name = "save" }; root.Add(saveBtn);
            saveAsBtn = new Button { text = "Save As", name = "save-as" }; root.Add(saveAsBtn);
        }

        public void UpdateDraftState(FlowLevelDraft draft, FlowEditorCommandHistory history)
        {
            UpdateColorChoices(draft, selectedColorField != null ? selectedColorField.value : 0);
            bool canSave = false;
            if (draft != null)
                canSave = draft.HasCompleteEndpoints && draft.currentSolution != null
                    && draft.currentDifficulty != null && !draft.isSolutionDirty && draft.isValidated;
            var hasDraft = draft != null;
            saveBtn?.SetEnabled(hasDraft); saveAsBtn?.SetEnabled(hasDraft);
            if (saveBtn != null) saveBtn.tooltip = canSave ? "Save the current validated draft." : "Click to see which save requirements are missing.";
            if (saveAsBtn != null) saveAsBtn.tooltip = canSave ? "Save a copy of the current validated draft." : "Click to see which save requirements are missing.";
            completeBtn?.SetEnabled(draft != null && draft.HasCompleteEndpoints);
            perfectCompleteBtn?.SetEnabled(draft != null && draft.HasCompleteEndpoints);
            stopSolvingBtn?.SetEnabled(false);
            undoBtn?.SetEnabled(history?.CanUndo ?? false);
            redoBtn?.SetEnabled(history?.CanRedo ?? false);
        }

        public void SelectColor(int colorId)
        {
            if (selectedColorField == null) return;
            if (selectedColorField.choices.Contains(colorId)) selectedColorField.value = colorId;
        }

        private void UpdateColorChoices(FlowLevelDraft draft, int preferredColorId)
        {
            if (selectedColorField == null) return;
            var choices = draft?.pairs?.Select(p => p.colorId).OrderBy(id => id).ToList() ?? new List<int>();
            selectedColorField.choices = choices;
            selectedColorField.SetEnabled(choices.Count > 0);
            if (choices.Count == 0) return;
            selectedColorField.value = choices.Contains(preferredColorId) ? preferredColorId : choices[0];
        }

        private static string FormatColor(int colorId) => FlowEditorColorPalette.GetDisplayName(colorId);
        private static string FormatEndpoint(bool isA) => isA ? "A (diamond)" : "B (square)";
        private static string FormatTool(FlowDraftEditTool tool)
        {
            switch (tool)
            {
                case FlowDraftEditTool.PlaceEndpoint:
                case FlowDraftEditTool.MoveEndpoint:
                    return "Set Endpoint";
                case FlowDraftEditTool.RemoveEndpoint: return "Remove Endpoint";
                case FlowDraftEditTool.DrawConstraint: return "Draw / Replace Constraint";
                default: return "Select";
            }
        }
    }
}
