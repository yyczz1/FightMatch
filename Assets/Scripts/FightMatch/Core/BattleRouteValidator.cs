using System;
using System.Collections.Generic;
using FlowPuzzle.Core;

namespace FightMatch.Core
{
    public sealed class BattleRouteValidator
    {
        public RouteValidationResult Validate(
            FlowLevelData level,
            IReadOnlyList<FlowPathData> lockedPaths,
            IReadOnlyCollection<FlowPos> blockedCells,
            int pairColorId,
            IReadOnlyList<FlowPos> cells)
        {
            if (level == null)
                throw new ArgumentNullException(nameof(level));
            if (lockedPaths == null)
                throw new ArgumentNullException(nameof(lockedPaths));
            if (blockedCells == null)
                throw new ArgumentNullException(nameof(blockedCells));
            if (cells == null)
                throw new ArgumentNullException(nameof(cells));

            FlowPairData selectedPair = null;
            var foreignEndpoints = new HashSet<FlowPos>();
            foreach (var pair in level.pairs)
            {
                if (pair.colorId == pairColorId)
                {
                    selectedPair = pair;
                }
                else
                {
                    foreignEndpoints.Add(pair.endpointA);
                    foreignEndpoints.Add(pair.endpointB);
                }
            }

            if (selectedPair == null)
                return new RouteValidationResult(false, "PairNotFound");

            var lockedCells = new HashSet<FlowPos>();
            foreach (var path in lockedPaths)
            {
                if (path.colorId == pairColorId)
                    return new RouteValidationResult(false, "PairAlreadyLocked");

                foreach (var cell in path.cells)
                    lockedCells.Add(cell);
            }

            if (cells.Count < 2)
                return new RouteValidationResult(false, "PathTooShort");

            var first = cells[0];
            var last = cells[cells.Count - 1];
            var forward = first == selectedPair.endpointA && last == selectedPair.endpointB;
            var reverse = first == selectedPair.endpointB && last == selectedPair.endpointA;
            if (!forward && !reverse)
                return new RouteValidationResult(false, "EndpointMismatch");

            var blocked = new HashSet<FlowPos>(blockedCells);
            var visited = new HashSet<FlowPos>();
            for (var i = 0; i < cells.Count; i++)
            {
                var cell = cells[i];
                if (cell.x < 0 || cell.x >= level.width || cell.y < 0 || cell.y >= level.height)
                    return new RouteValidationResult(false, "CellOutOfBounds", i);
                if (i > 0 && !FlowPathUtility.AreAdjacent(cells[i - 1], cell))
                    return new RouteValidationResult(false, "NonAdjacent", i);
                if (!visited.Add(cell))
                    return new RouteValidationResult(false, "SelfIntersection", i);
                if (foreignEndpoints.Contains(cell))
                    return new RouteValidationResult(false, "ForeignEndpoint", i);
                if (lockedCells.Contains(cell))
                    return new RouteValidationResult(false, "LockedOverlap", i);
                if (blocked.Contains(cell))
                    return new RouteValidationResult(false, "BlockedCell", i);
            }

            return new RouteValidationResult(true, string.Empty);
        }
    }
}
