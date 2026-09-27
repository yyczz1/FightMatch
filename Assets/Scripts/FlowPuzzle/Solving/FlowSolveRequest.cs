using System.Collections.Generic;
using FlowPuzzle.Core;

namespace FlowPuzzle.Solving
{
    public sealed class FlowSolveRequest
    {
        public const long DefaultNodeBudget = 10_000_000L;
        public const int DefaultTimeoutMs = 10000;
        public const int DefaultProgressIntervalNodes = 1000;

        public FlowLevelData levelData;
        public List<FlowPathData> fixedPrefixes;
        public long nodeBudget = DefaultNodeBudget;
        public int timeoutMs = DefaultTimeoutMs;
        public int progressIntervalNodes = DefaultProgressIntervalNodes;

        public FlowSolveRequest Clone()
        {
            var c = new FlowSolveRequest
            {
                nodeBudget = nodeBudget,
                timeoutMs = timeoutMs,
                progressIntervalNodes = progressIntervalNodes
            };
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
                    if (fp.cells != null)
                    {
                        path.cells = new List<FlowPos>(fp.cells.Count);
                        foreach (var cell in fp.cells) path.cells.Add(new FlowPos(cell.x, cell.y));
                    }
                    else
                    {
                        path.cells = null;
                    }
                    c.fixedPrefixes.Add(path);
                }
            }
            return c;
        }
    }
}
