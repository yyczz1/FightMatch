using FlowPuzzle.Editor.Draft;

namespace FlowPuzzle.Editor.Commands
{
    public sealed class ResizeBoardCommand : IFlowEditorCommand
    {
        private readonly FlowLevelDraft draft;
        private readonly FlowLevelDraft beforeSnapshot;
        private readonly FlowLevelDraft afterSnapshot;

        public string DisplayName => "Resize Board";

        public ResizeBoardCommand(FlowLevelDraft draft, FlowLevelDraft after)
        {
            this.draft = draft;
            beforeSnapshot = draft.Clone();
            afterSnapshot = after.Clone();
        }

        public bool Execute()
        {
            draft.RestoreFrom(afterSnapshot);
            return true;
        }

        public bool Undo()
        {
            draft.RestoreFrom(beforeSnapshot);
            return true;
        }
    }
}
