using System;
using System.Collections.Generic;
using System.Linq;
using FlowPuzzle.Core;

namespace FlowPuzzle.Generation
{
    [Serializable]
    public sealed class FlowPathLengthAllocationResult
    {
        public bool success;
        public int requestedTargetUsedCellCount;
        public int allocatedUsedCellCount;
        public List<int> pathLengthsByColorId = new List<int>();
        public List<int> generationOrderColorIds = new List<int>();
        public FlowFailureDiagnostic diagnostic;

        public static FlowPathLengthAllocationResult Success(
            int requestedTargetUsedCellCount,
            IList<int> pathLengthsByColorId)
        {
            var result = new FlowPathLengthAllocationResult
            {
                success = true,
                requestedTargetUsedCellCount = requestedTargetUsedCellCount,
                diagnostic = null
            };

            result.pathLengthsByColorId.AddRange(pathLengthsByColorId);
            result.allocatedUsedCellCount = result.pathLengthsByColorId.Sum();

            // generation order: descending length, then ascending colorId
            var order = new List<int>(pathLengthsByColorId.Count);
            for (var i = 0; i < pathLengthsByColorId.Count; i++)
                order.Add(i);

            order.Sort((a, b) =>
            {
                var cmp = pathLengthsByColorId[b].CompareTo(pathLengthsByColorId[a]);
                if (cmp != 0) return cmp;
                return a.CompareTo(b);
            });

            result.generationOrderColorIds = order;
            return result;
        }

        public static FlowPathLengthAllocationResult Failure(
            int requestedTargetUsedCellCount,
            string errorCode,
            string errorMessage)
        {
            return new FlowPathLengthAllocationResult
            {
                success = false,
                requestedTargetUsedCellCount = requestedTargetUsedCellCount,
                diagnostic = new FlowFailureDiagnostic
                {
                    errorCode = errorCode,
                    errorMessage = errorMessage,
                    usedSeed = 0,
                    attemptCount = 0
                }
            };
        }
    }
}
