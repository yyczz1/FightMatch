using System;
using System.Collections.Generic;

namespace FightMatch.Core
{
    public sealed class RandomSample<T>
    {
        public T Value { get; }
        public Pcg32StreamState NextState { get; }
        public IReadOnlyList<uint> Words { get; }

        internal RandomSample(T value, Pcg32StreamState nextState, List<uint> words)
        {
            Value = value;
            NextState = nextState;
            Words = Array.AsReadOnly(words.ToArray());
        }
    }
}
