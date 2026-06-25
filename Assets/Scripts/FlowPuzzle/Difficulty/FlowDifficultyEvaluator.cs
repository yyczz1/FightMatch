using System;
using System.Collections.Generic;
using FlowPuzzle.Core;

namespace FlowPuzzle.Difficulty
{
    public sealed class FlowDifficultyEvaluator
    {
        public FlowDifficultyReport Evaluate(
            FlowLevelData level,
            FlowSolutionData solution)
        {
            if (level == null)
                throw new ArgumentNullException(nameof(level));
            if (solution == null)
                throw new ArgumentNullException(nameof(solution));
            if (level.width <= 0 || level.height <= 0)
                throw new ArgumentOutOfRangeException(nameof(level),
                    $"Invalid dimensions: {level.width}x{level.height}.");
            if (level.pairs == null)
                throw new ArgumentException("Level pairs list is null.", nameof(level));
            if (solution.paths == null)
                throw new ArgumentException("Solution paths list is null.", nameof(solution));

            var boardCellCount = level.width * level.height;
            var colorCount = level.pairs.Count;

            // Build board from paths
            var board = new FlowBoard(level.width, level.height);
            var pathByColor = new Dictionary<int, FlowPathData>();

            foreach (var path in solution.paths)
            {
                if (path.cells == null)
                    throw new ArgumentException(
                        $"Path cells for colorId {path.colorId} is null.", nameof(solution));

                pathByColor[path.colorId] = path;

                for (var i = 0; i < path.cells.Count; i++)
                    board.Set(path.cells[i], path.colorId);
            }

            var usedCellCount = board.OccupiedCellCount;
            var coverageRatio = (float)usedCellCount / boardCellCount;

            // Turn and detour
            var totalTurnCount = 0;
            var totalDetour = 0;

            foreach (var path in solution.paths)
            {
                totalTurnCount += FlowPathUtility.CountTurns(path.cells);
                totalDetour += FlowPathUtility.GetDetour(path.cells);
            }

            // Endpoint distances
            var totalEndpointManhattanDistance = 0;
            foreach (var pair in level.pairs)
                totalEndpointManhattanDistance +=
                    FlowPathUtility.GetManhattanDistance(pair.endpointA, pair.endpointB);

            // Interaction: different-color adjacency (right and up only, once per pair)
            var differentColorAdjacentCount = 0;
            var endpointSet = new HashSet<FlowPos>();
            foreach (var pair in level.pairs)
            {
                endpointSet.Add(pair.endpointA);
                endpointSet.Add(pair.endpointB);
            }

            for (var x = 0; x < level.width; x++)
            {
                for (var y = 0; y < level.height; y++)
                {
                    var pos = new FlowPos(x, y);
                    if (board.IsEmpty(pos))
                        continue;

                    var color = board.Get(pos);

                    // check right neighbor
                    var right = new FlowPos(x + 1, y);
                    if (board.IsInside(right) && !board.IsEmpty(right))
                    {
                        var rightColor = board.Get(right);
                        if (rightColor != color)
                            differentColorAdjacentCount++;
                    }

                    // check up neighbor
                    var up = new FlowPos(x, y + 1);
                    if (board.IsInside(up) && !board.IsEmpty(up))
                    {
                        var upColor = board.Get(up);
                        if (upColor != color)
                            differentColorAdjacentCount++;
                    }
                }
            }

            // Bottleneck: inspect every board cell, excluding endpoints
            var bottleneckCount = 0;

            for (var x = 0; x < level.width; x++)
            {
                for (var y = 0; y < level.height; y++)
                {
                    var pos = new FlowPos(x, y);
                    if (endpointSet.Contains(pos))
                        continue;

                    var traversableNeighbors = 0;
                    var candidateIsEmpty = board.IsEmpty(pos);
                    var candidateColor = candidateIsEmpty ? -1 : board.Get(pos);

                    // right
                    CheckBottleneckNeighbor(board, x + 1, y, candidateIsEmpty, candidateColor,
                        ref traversableNeighbors);
                    // left
                    CheckBottleneckNeighbor(board, x - 1, y, candidateIsEmpty, candidateColor,
                        ref traversableNeighbors);
                    // up
                    CheckBottleneckNeighbor(board, x, y + 1, candidateIsEmpty, candidateColor,
                        ref traversableNeighbors);
                    // down
                    CheckBottleneckNeighbor(board, x, y - 1, candidateIsEmpty, candidateColor,
                        ref traversableNeighbors);

                    if (traversableNeighbors <= 2)
                        bottleneckCount++;
                }
            }

            // Scores
            var boardSizeScore = boardCellCount * 0.25f;
            var colorCountScore = colorCount * 6f;
            var coverageScore = coverageRatio * 45f;
            var turnScore = totalTurnCount * 2.5f;
            var detourScore = totalDetour * 2f;
            var interactionScore = differentColorAdjacentCount * 1.5f;
            var endpointDistanceScore = totalEndpointManhattanDistance * 1.2f;
            var bottleneckScore = bottleneckCount * 1.5f;

            var totalScore = boardSizeScore
                             + colorCountScore
                             + coverageScore
                             + turnScore
                             + detourScore
                             + interactionScore
                             + endpointDistanceScore
                             + bottleneckScore;

            FlowDifficultyTier tier;
            if (totalScore < 60f)
                tier = FlowDifficultyTier.Easy;
            else if (totalScore < 120f)
                tier = FlowDifficultyTier.Normal;
            else if (totalScore < 200f)
                tier = FlowDifficultyTier.Hard;
            else
                tier = FlowDifficultyTier.Expert;

            return new FlowDifficultyReport
            {
                difficulty = tier,
                totalScore = totalScore,
                boardSizeScore = boardSizeScore,
                colorCountScore = colorCountScore,
                coverageScore = coverageScore,
                turnScore = turnScore,
                detourScore = detourScore,
                interactionScore = interactionScore,
                endpointDistanceScore = endpointDistanceScore,
                bottleneckScore = bottleneckScore,
                totalTurnCount = totalTurnCount,
                totalDetour = totalDetour,
                differentColorAdjacentCount = differentColorAdjacentCount,
                totalEndpointManhattanDistance = totalEndpointManhattanDistance,
                bottleneckCount = bottleneckCount
            };
        }

        private static void CheckBottleneckNeighbor(
            FlowBoard board, int x, int y,
            bool candidateIsEmpty, int candidateColor,
            ref int traversableNeighbors)
        {
            var pos = new FlowPos(x, y);
            if (!board.IsInside(pos))
                return;

            if (candidateIsEmpty)
            {
                // traversable = empty neighbor
                if (board.IsEmpty(pos))
                    traversableNeighbors++;
            }
            else
            {
                // traversable = empty neighbor OR same-color neighbor
                if (board.IsEmpty(pos) || board.Get(pos) == candidateColor)
                    traversableNeighbors++;
            }
        }
    }
}
