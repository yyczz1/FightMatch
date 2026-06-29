using System.Collections.Generic;
using FlowPuzzle.Core;

namespace FlowPuzzle.Solving
{
    public sealed class FlowSolveRequest
    {
        public FlowLevelData levelData;
        public List<FlowPathData> fixedPrefixes;

        public FlowSolveRequest Clone()
        {
            var c = new FlowSolveRequest();
            if (levelData != null)
            {
                c.levelData = new FlowLevelData { levelId = levelData.levelId, width = levelData.width, height = levelData.height };
                foreach (var p in levelData.pairs)
                    c.levelData.pairs.Add(new FlowPairData { colorId = p.colorId, endpointA = p.endpointA, endpointB = p.endpointB });
            }
            if (fixedPrefixes != null)
            {
                c.fixedPrefixes = new List<FlowPathData>();
                foreach (var fp in fixedPrefixes)
                {
                    var path = new FlowPathData { colorId = fp.colorId };
                    path.cells = new List<FlowPos>(fp.cells.Count);
                    foreach (var cell in fp.cells) path.cells.Add(new FlowPos(cell.x, cell.y));
                    c.fixedPrefixes.Add(path);
                }
            }
            return c;
        }
    }
}
