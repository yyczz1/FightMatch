using FlowPuzzle.Core;
using FlowPuzzle.Editor.Commands;
using FlowPuzzle.Editor.Draft;

namespace FlowPuzzle.Editor.Commands
{
    public sealed class MoveEndpointCommand : IFlowEditorCommand
    {
        private readonly FlowLevelDraft draft;
        private readonly int colorId;
        private readonly bool isEndpointA;
        private readonly FlowPos? newPosition;
        private readonly FlowPos? oldPosition;

        public string DisplayName => oldPosition == null
            ? $"Place Endpoint {colorId}"
            : newPosition == null
                ? $"Remove Endpoint {colorId}"
                : $"Move Endpoint {colorId}";

        private MoveEndpointCommand(FlowLevelDraft draft, int colorId, bool isEndpointA,
            FlowPos? from, FlowPos? to)
        {
            this.draft = draft; this.colorId = colorId; this.isEndpointA = isEndpointA;
            oldPosition = from; newPosition = to;
        }

        public static MoveEndpointCommand Place(FlowLevelDraft draft, int colorId, bool isEndpointA, FlowPos position)
        {
            var old = isEndpointA ? draft.GetPair(colorId)?.endpointA : draft.GetPair(colorId)?.endpointB;
            return new MoveEndpointCommand(draft, colorId, isEndpointA, old, position);
        }

        public static MoveEndpointCommand Move(FlowLevelDraft draft, int colorId, bool isEndpointA, FlowPos newPosition)
        {
            var old = isEndpointA ? draft.GetPair(colorId)?.endpointA : draft.GetPair(colorId)?.endpointB;
            return new MoveEndpointCommand(draft, colorId, isEndpointA, old, newPosition);
        }

        public static MoveEndpointCommand Remove(FlowLevelDraft draft, int colorId, bool isEndpointA)
        {
            var old = isEndpointA ? draft.GetPair(colorId)?.endpointA : draft.GetPair(colorId)?.endpointB;
            return new MoveEndpointCommand(draft, colorId, isEndpointA, old, null);
        }

        public bool Execute()
        {
            var result = newPosition.HasValue
                ? draft.PlaceEndpoint(colorId, isEndpointA, newPosition.Value)
                : draft.RemoveEndpoint(colorId, isEndpointA);
            return result.success;
        }

        public bool Undo()
        {
            var result = oldPosition.HasValue
                ? draft.PlaceEndpoint(colorId, isEndpointA, oldPosition.Value)
                : draft.RemoveEndpoint(colorId, isEndpointA);
            return result.success;
        }
    }
}
