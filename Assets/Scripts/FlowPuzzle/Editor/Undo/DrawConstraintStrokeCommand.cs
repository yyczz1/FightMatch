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
        private bool executed;

        public string DisplayName => isErase ? $"Erase Constraint {colorId}" : $"Draw Constraint {colorId}";

        public DrawConstraintStrokeCommand(FlowLevelDraft draft, int colorId, List<FlowPos> cells, bool erase)
        {
            this.draft = draft; this.colorId = colorId;
            strokeCells = cells != null ? new List<FlowPos>(cells) : new List<FlowPos>();
            isErase = erase;
        }

        public bool Execute()
        {
            // Redo: restore stored after-snapshot directly, don't recalculate
            if (executed && afterSnapshot != null)
            {
                draft.RestoreFrom(afterSnapshot);
                return true;
            }

            beforeSnapshot = draft.Clone();
            bool ok;
            if (isErase)
            {
                var fc = draft.fixedConstraints.FirstOrDefault(c => c.colorId == colorId);
                if (fc?.cells == null || strokeCells.Count == 0) return false;
                // Find earliest chain index among all touched cells
                int minIdx = int.MaxValue;
                foreach (var cell in strokeCells)
                {
                    int idx = fc.cells.FindIndex(c => c.Equals(cell));
                    if (idx >= 0 && idx < minIdx) minIdx = idx;
                }
                if (minIdx == int.MaxValue) return false;
                ok = draft.EraseConstraint(colorId, minIdx).success;
            }
            else
            {
                ok = draft.ApplyConstraint(colorId, strokeCells).success;
            }
            if (!ok) { draft.RestoreFrom(beforeSnapshot); return false; }
            afterSnapshot = draft.Clone();
            executed = true;
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
