using System.Collections.Generic;

namespace FlowPuzzle.Generation
{
    public interface IFlowRandom
    {
        int NextInt(int minInclusive, int maxExclusive);
        float NextFloat(float minInclusive, float maxInclusive);
        void Shuffle<T>(IList<T> items);
    }
}
