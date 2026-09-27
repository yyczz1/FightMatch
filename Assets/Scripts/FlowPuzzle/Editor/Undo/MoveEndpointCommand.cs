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
        private readonly FlowLevelDraft beforeSnapshot;
        private FlowLevelDraft afterSnapshot;
        private bool executed;

        public string LastErrorMessage { get; private set; }

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
            beforeSnapshot = draft.Clone();
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
            if (executed && afterSnapshot != null)
            {
                draft.RestoreFrom(afterSnapshot);
                return true;
            }

            var result = newPosition.HasValue
                ? draft.PlaceEndpoint(colorId, isEndpointA, newPosition.Value)
                : draft.RemoveEndpoint(colorId, isEndpointA);
            LastErrorMessage = result.success ? null : result.errorMessage;
            if (!result.success)
                return false;
            afterSnapshot = draft.Clone();
            executed = true;
            return result.success;
        }

        public bool Undo()
        {
            if (!executed) return false;
            draft.RestoreFrom(beforeSnapshot);
            LastErrorMessage = null;
            return true;
        }
    }
}
