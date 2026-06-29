using System.Collections.Generic;
using FlowPuzzle.Core;
using FlowPuzzle.Editor.Commands;
using FlowPuzzle.Editor.Draft;

namespace FlowPuzzle.Editor.Commands
{
    public sealed class DrawConstraintStrokeCommand : IFlowEditorCommand
    {
        private readonly FlowLevelDraft draft;
        private readonly int colorId;
        private readonly List<FlowPos> newCells;
        private readonly FlowDraftConstraintData beforeConstraint;
        private readonly FlowDraftConstraintData afterConstraint;

        public string DisplayName => newCells.Count > 0 ? $"Draw Constraint {colorId}" : $"Erase Constraint {colorId}";

        public DrawConstraintStrokeCommand(FlowLevelDraft draft, int colorId, List<FlowPos> cells)
        {
            this.draft = draft; this.colorId = colorId; newCells = cells ?? new List<FlowPos>();
            beforeConstraint = draft.constraint?.Clone();
            afterConstraint = newCells.Count > 0
                ? new FlowDraftConstraintData { colorId = colorId, cells = new List<FlowPos>(newCells) }
                : null;
        }

        public bool Execute()
        {
            draft.constraint = afterConstraint?.Clone();
            draft.MarkDirty();
            return true;
        }

        public bool Undo()
        {
            draft.constraint = beforeConstraint?.Clone();
            draft.MarkDirty();
            return true;
        }
    }
}
