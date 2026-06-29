using FlowPuzzle.Editor.Commands;
using FlowPuzzle.Editor.Draft;

namespace FlowPuzzle.Editor.Commands
{
    public sealed class ResizeBoardCommand : IFlowEditorCommand
    {
        private readonly FlowLevelDraft draft;
        private readonly FlowLevelDraft beforeSnapshot;
        private readonly int newWidth, newHeight;

        public string DisplayName => "Resize Board";

        public ResizeBoardCommand(FlowLevelDraft draft, int newWidth, int newHeight)
        {
            this.draft = draft; this.newWidth = newWidth; this.newHeight = newHeight;
            beforeSnapshot = draft.Clone();
        }

        public bool Execute()
        {
            return draft.Resize(newWidth, newHeight).success;
        }

        public bool Undo()
        {
            draft.RestoreFrom(beforeSnapshot);
            return true;
        }
    }
}
