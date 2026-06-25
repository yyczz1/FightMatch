using FlowPuzzle.Core;

namespace FlowPuzzle.Generation
{
    public interface IFlowPathGenerationStrategy
    {
        bool TryGeneratePath(
            FlowBoard board,
            int colorId,
            int targetLength,
            int maxAttempt,
            float turnPreference,
            float interactionPreference,
            IFlowRandom random,
            out FlowPathData path);
    }
}
