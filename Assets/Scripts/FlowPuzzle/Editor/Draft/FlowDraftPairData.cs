using System;
using FlowPuzzle.Core;

namespace FlowPuzzle.Editor.Draft
{
    [Serializable]
    public sealed class FlowDraftPairData
    {
        public int colorId;
        public FlowPos? endpointA;
        public FlowPos? endpointB;

        public FlowDraftPairData Clone()
        {
            return new FlowDraftPairData
            {
                colorId = colorId,
                endpointA = endpointA.HasValue ? new FlowPos(endpointA.Value.x, endpointA.Value.y) : null,
                endpointB = endpointB.HasValue ? new FlowPos(endpointB.Value.x, endpointB.Value.y) : null
            };
        }
    }
}
