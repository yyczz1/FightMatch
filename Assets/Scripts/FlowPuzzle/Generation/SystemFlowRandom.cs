using System;
using System.Collections.Generic;

namespace FlowPuzzle.Generation
{
    public sealed class SystemFlowRandom : IFlowRandom
    {
        private readonly Random random;

        public SystemFlowRandom(int seed)
        {
            random = new Random(seed);
        }

        public int NextInt(int minInclusive, int maxExclusive)
        {
            if (minInclusive >= maxExclusive)
                throw new ArgumentOutOfRangeException(nameof(minInclusive),
                    $"minInclusive ({minInclusive}) must be less than maxExclusive ({maxExclusive}).");

            return random.Next(minInclusive, maxExclusive);
        }

        public float NextFloat(float minInclusive, float maxInclusive)
        {
            if (minInclusive >= maxInclusive)
                throw new ArgumentOutOfRangeException(nameof(minInclusive),
                    $"minInclusive ({minInclusive}) must not be greater than maxInclusive ({maxInclusive}).");

            var t = (float)random.NextDouble();
            return minInclusive + t * (maxInclusive - minInclusive);
        }

        public void Shuffle<T>(IList<T> items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            for (var i = items.Count - 1; i > 0; i--)
            {
                var j = random.Next(0, i + 1);
                var tmp = items[i];
                items[i] = items[j];
                items[j] = tmp;
            }
        }
    }
}
