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
        private readonly bool isErase;
        private FlowLevelDraft beforeSnapshot;
        private FlowLevelDraft afterSnapshot;

        public string DisplayName => isErase ? $"Erase Constraint {colorId}" : $"Draw Constraint {colorId}";

        public DrawConstraintStrokeCommand(FlowLevelDraft draft, int colorId, List<FlowPos> cells, bool erase)
        {
            this.draft = draft; this.colorId = colorId;
            strokeCells = cells != null ? new List<FlowPos>(cells) : new List<FlowPos>();
            isErase = erase;
        }

        public bool Execute()
        {
            beforeSnapshot = draft.Clone();
            bool ok;
            if (isErase)
            {
                var fc = draft.fixedConstraints.FirstOrDefault(c => c.colorId == colorId);
                if (fc?.cells == null || strokeCells.Count == 0) return false;
                int idx = fc.cells.FindIndex(cell => cell.Equals(strokeCells[0]));
                if (idx < 0) return false;
                ok = draft.EraseConstraint(colorId, idx).success;
            }
            else
            {
                ok = draft.ApplyConstraint(colorId, strokeCells).success;
            }
            if (!ok) { draft.RestoreFrom(beforeSnapshot); return false; }
            afterSnapshot = draft.Clone();
            return true;
        }

        public bool Undo()
        {
            if (beforeSnapshot == null) return false;
            draft.RestoreFrom(beforeSnapshot);
            return true;
        }
    }
}
