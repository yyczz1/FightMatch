using System;
using System.Collections.Generic;
using FlowPuzzle.Core;

namespace FlowPuzzle.Editor.Draft
{
    [Serializable]
    public sealed class FlowDraftConstraintData
    {
        public int? colorId;
        public List<FlowPos> cells;

        public FlowDraftConstraintData Clone()
        {
            if (colorId == null) return new FlowDraftConstraintData();
            var c = new FlowDraftConstraintData { colorId = colorId, cells = new List<FlowPos>() };
            if (cells != null) foreach (var cell in cells) c.cells.Add(new FlowPos(cell.x, cell.y));
            return c;
        }
    }
}
