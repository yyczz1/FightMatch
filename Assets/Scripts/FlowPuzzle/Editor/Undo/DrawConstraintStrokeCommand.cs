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
        private readonly List<FlowPos> strokeCells;
        private FlowDraftConstraintData beforeSnapshot;
        private bool isErase;

        public string DisplayName => isErase ? $"Erase Constraint {colorId}" : $"Draw Constraint {colorId}";

        public DrawConstraintStrokeCommand(FlowLevelDraft draft, int colorId, List<FlowPos> cells, bool erase)
        {
            this.draft = draft; this.colorId = colorId; strokeCells = cells ?? new List<FlowPos>();
            isErase = erase;
        }

        public bool Execute()
        {
            beforeSnapshot = draft.fixedConstraints.FirstOrDefault(c => c.colorId == colorId)?.Clone();
            if (isErase)
            {
                var fc = draft.fixedConstraints.FirstOrDefault(c => c.colorId == colorId);
                if (fc?.cells == null || strokeCells.Count == 0) return false;
                int idx = fc.cells.FindIndex(cell => cell.Equals(strokeCells[0]));
                if (idx < 0) return false;
                var r = draft.EraseConstraint(colorId, idx);
                return r.success;
            }
            else
            {
                var r = draft.ApplyConstraint(colorId, strokeCells);
                return r.success;
            }
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
