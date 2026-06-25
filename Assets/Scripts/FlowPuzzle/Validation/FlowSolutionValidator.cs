using System.Collections.Generic;
using FlowPuzzle.Core;

namespace FlowPuzzle.Validation
{
    public sealed class FlowSolutionValidator
    {
        public FlowValidationResult Validate(
            FlowLevelData level,
            FlowSolutionData solution)
        {
            // 1. NullLevel
            if (level == null)
                return FlowValidationResult.Invalid("NullLevel", "Level is null.");

            // 2. NullSolution
            if (solution == null)
                return FlowValidationResult.Invalid("NullSolution", "Solution is null.");

            // 3. MissingPairs
            if (level.pairs == null)
                return FlowValidationResult.Invalid("MissingPairs", "Level pairs list is null.");

            // 4. MissingPaths
            if (solution.paths == null)
                return FlowValidationResult.Invalid("MissingPaths", "Solution paths list is null.");

            // 5. InvalidDimensions
            if (level.width <= 0 || level.height <= 0)
                return FlowValidationResult.Invalid("InvalidDimensions",
                    $"Invalid dimensions: {level.width}x{level.height}.");

            // 6. DuplicatePairColor
            var pairColorSet = new HashSet<int>();
            foreach (var pair in level.pairs)
            {
                if (!pairColorSet.Add(pair.colorId))
                    return FlowValidationResult.Invalid("DuplicatePairColor",
                        $"Duplicate pair colorId: {pair.colorId}.");
            }

            // 7. EndpointOutOfBounds
            foreach (var pair in level.pairs)
            {
                if (!IsInside(pair.endpointA, level) || !IsInside(pair.endpointB, level))
                    return FlowValidationResult.Invalid("EndpointOutOfBounds",
                        $"Endpoint out of bounds for colorId: {pair.colorId}.");
            }

            // Build pair lookup
            var pairByColor = new Dictionary<int, FlowPairData>();
            foreach (var pair in level.pairs)
                pairByColor[pair.colorId] = pair;

            // 8. DuplicatePathColor
            var pathColorSet = new HashSet<int>();
            foreach (var path in solution.paths)
            {
                if (!pathColorSet.Add(path.colorId))
                    return FlowValidationResult.Invalid("DuplicatePathColor",
                        $"Duplicate path colorId: {path.colorId}.");
            }

            // 9. MissingPath & 10. ExtraPathColor
            foreach (var pair in level.pairs)
            {
                if (!pathColorSet.Contains(pair.colorId))
                    return FlowValidationResult.Invalid("MissingPath",
                        $"No path for pair colorId: {pair.colorId}.");
            }

            foreach (var colorId in pathColorSet)
            {
                if (!pairByColor.ContainsKey(colorId))
                    return FlowValidationResult.Invalid("ExtraPathColor",
                        $"Path colorId {colorId} has no matching pair.");
            }

            // 11. Per-path geometry checks
            var occupiedCells = new Dictionary<FlowPos, int>();

            foreach (var pair in level.pairs)
            {
                var path = FindPath(solution, pair.colorId);

                // PathTooShort
                if (path.cells == null || path.cells.Count < 2)
                    return FlowValidationResult.Invalid("PathTooShort",
                        $"Path for colorId {pair.colorId} has fewer than 2 cells.");

                // EndpointMismatch
                var first = path.cells[0];
                var last = path.cells[path.cells.Count - 1];
                var matchesA = first.Equals(pair.endpointA) && last.Equals(pair.endpointB);
                var matchesB = first.Equals(pair.endpointB) && last.Equals(pair.endpointA);
                if (!matchesA && !matchesB)
                    return FlowValidationResult.Invalid("EndpointMismatch",
                        $"Path for colorId {pair.colorId} does not start/end at its endpoints.");

                // Per-cell checks
                var pathCellSet = new HashSet<FlowPos>();

                for (var i = 0; i < path.cells.Count; i++)
                {
                    var cell = path.cells[i];

                    // CellOutOfBounds
                    if (!IsInside(cell, level))
                        return FlowValidationResult.Invalid("CellOutOfBounds",
                            $"Cell {cell.x},{cell.y} in colorId {pair.colorId} is out of bounds.");

                    // SelfIntersection
                    if (!pathCellSet.Add(cell))
                        return FlowValidationResult.Invalid("SelfIntersection",
                            $"Path for colorId {pair.colorId} self-intersects at {cell.x},{cell.y}.");

                    // InvalidAdjacency (check consecutive)
                    if (i > 0)
                    {
                        if (!FlowPathUtility.AreAdjacent(path.cells[i - 1], cell))
                            return FlowValidationResult.Invalid("InvalidAdjacency",
                                $"Non-adjacent step in colorId {pair.colorId} at {cell.x},{cell.y}.");
                    }
                }
            }

            // ForeignEndpointTraversal & PathOverlap — need per-cell occupancy lookup
            // Build endpoint coordinate set
            var endpointCoords = new HashSet<FlowPos>();
            foreach (var pair in level.pairs)
            {
                endpointCoords.Add(pair.endpointA);
                endpointCoords.Add(pair.endpointB);
            }

            // Walk all paths again to check foreign endpoint and overlap
            var globalOccupied = new Dictionary<FlowPos, int>();

            foreach (var pair in level.pairs)
            {
                var path = FindPath(solution, pair.colorId);

                foreach (var cell in path.cells)
                {
                    // ForeignEndpointTraversal: a non-endpoint cell or wrong endpoint
                    if (endpointCoords.Contains(cell))
                    {
                        var isOwnEndpoint =
                            cell.Equals(pair.endpointA) || cell.Equals(pair.endpointB);
                        if (!isOwnEndpoint)
                            return FlowValidationResult.Invalid("ForeignEndpointTraversal",
                                $"Path for colorId {pair.colorId} crosses foreign endpoint at {cell.x},{cell.y}.");
                    }

                    // PathOverlap: another color already occupies this cell
                    if (globalOccupied.TryGetValue(cell, out var existingColor))
                    {
                        if (existingColor != pair.colorId)
                            return FlowValidationResult.Invalid("PathOverlap",
                                $"Cell {cell.x},{cell.y} shared by colorId {existingColor} and {pair.colorId}.");
                    }
                    else
                    {
                        globalOccupied[cell] = pair.colorId;
                    }
                }
            }

            return FlowValidationResult.Valid();
        }

        private static bool IsInside(FlowPos pos, FlowLevelData level)
        {
            return pos.x >= 0 && pos.x < level.width
                && pos.y >= 0 && pos.y < level.height;
        }

        private static FlowPathData FindPath(FlowSolutionData solution, int colorId)
        {
            for (var i = 0; i < solution.paths.Count; i++)
            {
                if (solution.paths[i].colorId == colorId)
                    return solution.paths[i];
            }
            return null;
        }
    }
}
