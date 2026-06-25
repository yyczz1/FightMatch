using System;

namespace FlowPuzzle.Core
{
    [Serializable]
    public sealed class FlowFailureDiagnostic
    {
        public string errorCode;
        public string errorMessage;
        public int usedSeed;
        public int attemptCount;
    }
}
