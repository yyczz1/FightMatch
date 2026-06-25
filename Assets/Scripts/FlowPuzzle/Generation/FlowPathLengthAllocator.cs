using System;
using System.Collections.Generic;
using System.Linq;
using FlowPuzzle.Core;

namespace FlowPuzzle.Generation
{
    public sealed class FlowPathLengthAllocator
    {
        public FlowPathLengthAllocationResult Allocate(
            FlowGenerationConfig config,
            int targetUsedCellCount,
            IFlowRandom random)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
            if (random == null)
                throw new ArgumentNullException(nameof(random));

            // InvalidDimensions
            if (config.width <= 0 || config.height <= 0 || config.colorCount <= 0)
                return FlowPathLengthAllocationResult.Failure(targetUsedCellCount,
                    "InvalidDimensions",
                    $"Invalid dimensions: {config.width}x{config.height} or colorCount={config.colorCount}.");

            // InvalidPathLengthRange
            if (config.minPathLength < 2 || config.minPathLength > config.maxPathLength)
                return FlowPathLengthAllocationResult.Failure(targetUsedCellCount,
                    "InvalidPathLengthRange",
                    $"Path length range [{config.minPathLength}, {config.maxPathLength}] is invalid.");

            var boardCapacity = config.width * config.height;
            var feasibleMin = config.colorCount * config.minPathLength;
            var feasibleMax = Math.Min(boardCapacity, config.colorCount * config.maxPathLength);

            // ImpossibleMinimumOccupancy
            if (feasibleMin > boardCapacity)
                return FlowPathLengthAllocationResult.Failure(targetUsedCellCount,
                    "ImpossibleMinimumOccupancy",
                    $"Minimum occupancy {feasibleMin} exceeds board capacity {boardCapacity}.");

            // Clamp target
            var clampedTarget = Math.Max(feasibleMin, Math.Min(targetUsedCellCount, feasibleMax));

            // Initialize every color to minimum
            var lengths = new int[config.colorCount];
            for (var i = 0; i < config.colorCount; i++)
                lengths[i] = config.minPathLength;

            var remaining = clampedTarget - feasibleMin;

            // Shuffle color IDs for cyclic distribution
            var colorIds = new List<int>(config.colorCount);
            for (var i = 0; i < config.colorCount; i++)
                colorIds.Add(i);
            random.Shuffle(colorIds);

            // Distribute remaining cells
            var idx = 0;
            while (remaining > 0 && idx < colorIds.Count * config.maxPathLength)
            {
                var cid = colorIds[idx % colorIds.Count];
                if (lengths[cid] < config.maxPathLength)
                {
                    lengths[cid]++;
                    remaining--;
                }
                idx++;
            }

            return FlowPathLengthAllocationResult.Success(targetUsedCellCount, lengths);
        }
    }
}
