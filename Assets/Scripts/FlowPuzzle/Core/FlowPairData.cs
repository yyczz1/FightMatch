using System;

namespace FlowPuzzle.Core
{
    [Serializable]
    public sealed class FlowPairData
    {
        public int colorId;
        public FlowPos endpointA;
        public FlowPos endpointB;
    }
}
