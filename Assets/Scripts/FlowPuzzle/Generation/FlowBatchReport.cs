using System;
using System.Collections.Generic;
using FlowPuzzle.Core;

namespace FlowPuzzle.Generation
{
    [Serializable]
    public sealed class FlowBatchItemResult
    {
        public int levelId;
        public int usedSeed;
        public bool success;
        public string message = string.Empty;
        public FlowGenerationResult generationResult;
    }

    [Serializable]
    public sealed class FlowBatchReport
    {
        public int requestedCount;
        public int successfulCount;
        public int failedCount;
        public List<FlowBatchItemResult> items = new List<FlowBatchItemResult>();
    }
}
