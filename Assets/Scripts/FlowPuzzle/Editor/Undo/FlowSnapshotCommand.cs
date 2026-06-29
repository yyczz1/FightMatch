using FlowPuzzle.Editor.Draft;

namespace FlowPuzzle.Editor.Commands
{
    public sealed class FlowSnapshotCommand : IFlowEditorCommand
    {
        private readonly FlowLevelDraft draft;
        private readonly FlowLevelDraft beforeSnapshot;
        private readonly FlowLevelDraft afterSnapshot;
        private readonly string displayName;

        public string DisplayName => displayName;

        public FlowSnapshotCommand(FlowLevelDraft draft, FlowLevelDraft before, FlowLevelDraft after, string name)
        {
            this.draft = draft;
            beforeSnapshot = before.Clone();
            afterSnapshot = after.Clone();
            displayName = name;
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
