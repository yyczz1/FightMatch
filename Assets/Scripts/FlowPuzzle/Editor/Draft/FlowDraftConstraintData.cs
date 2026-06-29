using System;
using System.Collections.Generic;
using FlowPuzzle.Core;

namespace FlowPuzzle.Editor.Draft
{
    [Serializable]
    public sealed class FlowDraftConstraintData
    {
        /// <summary>Null means no constraint for this color.</summary>
        public int? colorId;
        public List<FlowPos> cells;

        public FlowDraftConstraintData Clone()
        {
            if (colorId == null || cells == null) return new FlowDraftConstraintData();
            var copy = new FlowDraftConstraintData { colorId = colorId };
            copy.cells = new List<FlowPos>(cells.Count);
            foreach (var c in cells) copy.cells.Add(new FlowPos(c.x, c.y));
            return copy;
        }
    }
}
