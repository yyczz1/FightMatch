using System;

namespace FightMatch.Core
{
    public sealed class RandomSamplingBudget
    {
        public ExactMathBudget Math { get; }
        public int MaxWords { get; }
        public int WordsUsed { get; private set; }

        public RandomSamplingBudget(ExactMathBudget math, int maxWords = 4096)
        {
            if (math == null)
                throw new ArgumentNullException(nameof(math));
            if (maxWords < 0)
                throw new ArgumentOutOfRangeException(nameof(maxWords));
            Math = math;
            MaxWords = maxWords;
        }

        internal void CheckNextWord()
        {
            if (WordsUsed >= MaxWords)
                throw new ExactMathLimitException("RandomWords", (long)WordsUsed + 1, MaxWords);
        }

        internal void RecordWord()
        {
            WordsUsed++;
        }
    }
}
