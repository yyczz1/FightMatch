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
        public string LastErrorMessage { get; private set; }

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
                if (fc?.cells == null || strokeCells.Count == 0)
                {
                    LastErrorMessage = $"Color {colorId} has no constraint to erase.";
                    return false;
                }
                // Find earliest chain index among all touched cells
                int minIdx = int.MaxValue;
                foreach (var cell in strokeCells)
                {
                    int idx = fc.cells.FindIndex(c => c.Equals(cell));
                    if (idx >= 0 && idx < minIdx) minIdx = idx;
                }
                if (minIdx == int.MaxValue)
                {
                    LastErrorMessage = "The erase stroke did not touch the selected color's constraint.";
                    return false;
                }
                var result = draft.EraseConstraint(colorId, minIdx);
                ok = result.success;
                LastErrorMessage = result.errorMessage;
            }
            else
            {
                var result = draft.ApplyConstraint(colorId, strokeCells);
                ok = result.success;
                LastErrorMessage = result.errorMessage;
            }
            if (!ok) { draft.RestoreFrom(beforeSnapshot); return false; }
            LastErrorMessage = null;
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
