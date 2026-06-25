using System;
using System.Collections.Generic;
using FlowPuzzle.Core;

namespace FlowPuzzle.Generation
{
    public sealed class RandomizedDfsPathGenerationStrategy : IFlowPathGenerationStrategy
    {
        public bool TryGeneratePath(
            FlowBoard board,
            int colorId,
            int targetLength,
            int maxAttempt,
            float turnPreference,
            float interactionPreference,
            IFlowRandom random,
            out FlowPathData path)
        {
            if (board == null)
                throw new ArgumentNullException(nameof(board));
            if (random == null)
                throw new ArgumentNullException(nameof(random));
            if (colorId < 0)
                throw new ArgumentOutOfRangeException(nameof(colorId));
            if (targetLength < 2 || maxAttempt <= 0)
            {
                path = null;
                return false;
            }

            var emptyCells = CollectEmptyPositions(board);
            if (emptyCells.Count == 0 || emptyCells.Count < targetLength)
            {
                path = null;
                return false;
            }

            for (var attempt = 0; attempt < maxAttempt; attempt++)
            {
                var startIdx = random.NextInt(0, emptyCells.Count);
                var start = emptyCells[startIdx];

                var tentative = new List<FlowPos>(targetLength) { start };
                var visited = new HashSet<FlowPos> { start };

                if (Dfs(board, tentative, visited, targetLength, turnPreference,
                        interactionPreference, random))
                {
                    // Commit to board
                    var cells = new List<FlowPos>(tentative);
                    for (var i = 0; i < cells.Count; i++)
                        board.Set(cells[i], colorId);

                    path = new FlowPathData
                    {
                        colorId = colorId,
                        cells = cells
                    };
                    return true;
                }
            }

            path = null;
            return false;
        }

        private bool Dfs(
            FlowBoard board,
            List<FlowPos> tentative,
            HashSet<FlowPos> visited,
            int targetLength,
            float turnPreference,
            float interactionPreference,
            IFlowRandom random)
        {
            if (tentative.Count == targetLength)
                return true;

            var current = tentative[tentative.Count - 1];
            var neighbors = board.GetNeighbors(current);

            // Collect candidate neighbors (empty and not visited)
            var candidates = new List<FlowPos>();
            for (var i = 0; i < neighbors.Count; i++)
            {
                var n = neighbors[i];
                if (board.IsEmpty(n) && !visited.Contains(n))
                    candidates.Add(n);
            }

            if (candidates.Count == 0)
                return false;

            // Shuffle then stable-sort by score
            random.Shuffle(candidates);

            var scores = new float[candidates.Count];
            for (var i = 0; i < candidates.Count; i++)
            {
                scores[i] = ScoreCandidate(tentative, candidates[i], turnPreference,
                    interactionPreference, board);
            }

            // Stable sort: higher score = preferred first
            for (var i = 0; i < candidates.Count - 1; i++)
            {
                for (var j = i + 1; j < candidates.Count; j++)
                {
                    if (scores[j] > scores[i])
                    {
                        var tmpC = candidates[i]; candidates[i] = candidates[j]; candidates[j] = tmpC;
                        var tmpS = scores[i]; scores[i] = scores[j]; scores[j] = tmpS;
                    }
                }
            }

            for (var i = 0; i < candidates.Count; i++)
            {
                var next = candidates[i];
                tentative.Add(next);
                visited.Add(next);

                if (Dfs(board, tentative, visited, targetLength, turnPreference,
                        interactionPreference, random))
                    return true;

                tentative.RemoveAt(tentative.Count - 1);
                visited.Remove(next);
            }

            return false;
        }

        private static float ScoreCandidate(
            List<FlowPos> tentative,
            FlowPos candidate,
            float turnPreference,
            float interactionPreference,
            FlowBoard board)
        {
            var score = 0f;

            // Turn score
            if (tentative.Count >= 2)
            {
                var prev = tentative[tentative.Count - 2];
                var cur = tentative[tentative.Count - 1];
                var prevDx = cur.x - prev.x;
                var prevDy = cur.y - prev.y;
                var newDx = candidate.x - cur.x;
                var newDy = candidate.y - cur.y;

                var isTurn = (prevDx != newDx || prevDy != newDy);
                if (isTurn)
                    score += turnPreference;
                else
                    score -= turnPreference;
            }

            // Interaction score: count neighbors already occupied
            var neighbors = board.GetNeighbors(candidate);
            for (var i = 0; i < neighbors.Count; i++)
            {
                if (!board.IsEmpty(neighbors[i]))
                    score += interactionPreference;
            }

            return score;
        }

        private static List<FlowPos> CollectEmptyPositions(FlowBoard board)
        {
            var result = new List<FlowPos>();
            for (var x = 0; x < board.Width; x++)
            for (var y = 0; y < board.Height; y++)
            {
                var pos = new FlowPos(x, y);
                if (board.IsEmpty(pos))
                    result.Add(pos);
            }
            return result;
        }
    }
}
