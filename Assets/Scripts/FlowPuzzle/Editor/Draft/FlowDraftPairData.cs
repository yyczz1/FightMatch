using System;
using FlowPuzzle.Core;
using UnityEngine;

namespace FlowPuzzle.Editor.Draft
{
    [Serializable]
    public sealed class FlowDraftPairData : ISerializationCallbackReceiver
    {
        public int colorId;
        public FlowPos? endpointA;
        public FlowPos? endpointB;

        [SerializeField] private int _endpointAx;
        [SerializeField] private int _endpointAy;
        [SerializeField] private bool _hasEndpointA;
        [SerializeField] private int _endpointBx;
        [SerializeField] private int _endpointBy;
        [SerializeField] private bool _hasEndpointB;

        public void OnBeforeSerialize()
        {
            _hasEndpointA = endpointA.HasValue;
            if (endpointA.HasValue) { _endpointAx = endpointA.Value.x; _endpointAy = endpointA.Value.y; }
            _hasEndpointB = endpointB.HasValue;
            if (endpointB.HasValue) { _endpointBx = endpointB.Value.x; _endpointBy = endpointB.Value.y; }
        }

        public void OnAfterDeserialize()
        {
            endpointA = _hasEndpointA ? new FlowPos(_endpointAx, _endpointAy) : null;
            endpointB = _hasEndpointB ? new FlowPos(_endpointBx, _endpointBy) : null;
        }

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
