using System;
using System.Collections.Generic;

namespace FlowPuzzle.Core
{
    public static class FlowPathUtility
    {
        public static bool AreAdjacent(FlowPos a, FlowPos b)
        {
            return GetManhattanDistance(a, b) == 1;
        }

        public static int GetManhattanDistance(FlowPos a, FlowPos b)
        {
            return Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y);
        }

        public static int GetMoveCount(IList<FlowPos> cells)
        {
            if (cells == null || cells.Count <= 1)
                return 0;
            return cells.Count - 1;
        }

        public static int CountTurns(IList<FlowPos> cells)
        {
            if (cells == null || cells.Count < 3)
                return 0;

            var turns = 0;
            var prevDx = cells[1].x - cells[0].x;
            var prevDy = cells[1].y - cells[0].y;

            for (var i = 2; i < cells.Count; i++)
            {
                var dx = cells[i].x - cells[i - 1].x;
                var dy = cells[i].y - cells[i - 1].y;

                if (dx != prevDx || dy != prevDy)
                {
                    turns++;
                }

                prevDx = dx;
                prevDy = dy;
            }

            return turns;
        }

        public static bool HasDuplicateCells(IList<FlowPos> cells)
        {
            if (cells == null || cells.Count <= 1)
                return false;

            var seen = new HashSet<FlowPos>();
            for (var i = 0; i < cells.Count; i++)
            {
                if (!seen.Add(cells[i]))
                    return true;
            }

            return false;
        }

        public static int GetDetour(IList<FlowPos> cells)
        {
            if (cells == null || cells.Count <= 1)
                return 0;

            var moves = GetMoveCount(cells);
            var straight = GetManhattanDistance(cells[0], cells[cells.Count - 1]);
            var detour = moves - straight;

            return detour > 0 ? detour : 0;
        }
    }
}
