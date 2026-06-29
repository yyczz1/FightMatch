using FlowPuzzle.Editor.Draft;
using FlowPuzzle.Editor.Commands;
using UnityEngine.UIElements;

namespace FlowPuzzle.Editor.UI
{
    public sealed class FlowDraftPanel
    {
        public Button newDraftBtn, loadAssetBtn, addColorBtn, removeColorBtn;
        public Button undoBtn, redoBtn, saveBtn, saveAsBtn, completeBtn;
        public IntegerField selectedColorField;
        public Toggle endpointToggle;
        public EnumField toolField;
        public TextField saveAsNameField;

        public void Build(VisualElement root)
        {
            newDraftBtn = new Button { text = "New Draft", name = "new-draft" }; root.Add(newDraftBtn);
            loadAssetBtn = new Button { text = "Load Asset", name = "load-asset" }; root.Add(loadAssetBtn);
            selectedColorField = new IntegerField("Selected Color") { value = 0 }; root.Add(selectedColorField);
            endpointToggle = new Toggle("Endpoint A (else B)") { value = true }; root.Add(endpointToggle);
            toolField = new EnumField("Tool", FlowDraftEditTool.Select); root.Add(toolField);
            addColorBtn = new Button { text = "Add Color", name = "add-color" }; root.Add(addColorBtn);
            removeColorBtn = new Button { text = "Remove Color", name = "remove-color" }; root.Add(removeColorBtn);
            undoBtn = new Button { text = "Undo", name = "undo" }; root.Add(undoBtn);
            redoBtn = new Button { text = "Redo", name = "redo" }; root.Add(redoBtn);
            saveAsNameField = new TextField("Save As Name") { value = "" }; root.Add(saveAsNameField);
            saveBtn = new Button { text = "Save", name = "save" }; root.Add(saveBtn);
            saveAsBtn = new Button { text = "Save As", name = "save-as" }; root.Add(saveAsBtn);
            completeBtn = new Button { text = "Complete", name = "complete" };
            completeBtn.SetEnabled(false);
            completeBtn.tooltip = "Available after local solver is installed.";
            root.Add(completeBtn);
        }

        public void UpdateDraftState(FlowLevelDraft draft, FlowEditorCommandHistory history)
        {
            bool canSave = false;
            if (draft != null)
                canSave = draft.HasCompleteEndpoints && draft.currentSolution != null
                    && !draft.isSolutionDirty && draft.isValidated;
            saveBtn?.SetEnabled(canSave); saveAsBtn?.SetEnabled(canSave);
            undoBtn?.SetEnabled(history?.CanUndo ?? false);
            redoBtn?.SetEnabled(history?.CanRedo ?? false);
        }
    }
}
