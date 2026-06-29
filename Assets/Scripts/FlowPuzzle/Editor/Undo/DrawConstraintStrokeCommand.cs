using System.Collections.Generic;
using System.Linq;
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
        private FlowDraftConstraintData beforeSnapshot;

        public string DisplayName => newCells.Count > 0 ? $"Draw Constraint {colorId}" : $"Erase Constraint {colorId}";

        public DrawConstraintStrokeCommand(FlowLevelDraft draft, int colorId, List<FlowPos> cells)
        {
            this.draft = draft; this.colorId = colorId; newCells = cells ?? new List<FlowPos>();
        }

        public bool Execute()
        {
            beforeSnapshot = draft.fixedConstraints
                .FirstOrDefault(c => c.colorId == colorId)?.Clone();

            // Remove existing constraint for this color
            draft.fixedConstraints.RemoveAll(c => c.colorId == colorId);

            if (newCells.Count > 0)
            {
                var fc = new FlowDraftConstraintData { colorId = colorId, cells = new List<FlowPos>(newCells) };
                draft.fixedConstraints.Add(fc);
            }

            draft.MarkDirty();
            return true;
        }

        public bool Undo()
        {
            draft.fixedConstraints.RemoveAll(c => c.colorId == colorId);
            if (beforeSnapshot != null && beforeSnapshot.cells != null && beforeSnapshot.cells.Count > 0)
                draft.fixedConstraints.Add(beforeSnapshot.Clone());
            draft.MarkDirty();
            return true;
        }
    }
}
