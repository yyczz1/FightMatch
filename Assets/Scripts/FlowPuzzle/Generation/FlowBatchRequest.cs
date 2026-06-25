using System;
using FlowPuzzle.Core;

namespace FlowPuzzle.Generation
{
    [Serializable]
    public sealed class FlowBatchRequest
    {
        public int startLevelId;
        public int count;
        public int baseSeed;
        public FlowGenerationConfig config;
    }
}
