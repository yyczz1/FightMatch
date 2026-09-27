using System;
using System.Collections.Generic;

namespace FlowPuzzle.Core
{
    [Serializable]
    public sealed class FlowFailureDiagnostic
    {
        public string errorCode;
        public string errorMessage;
        public int usedSeed;
        public int attemptCount;

        /// <summary>
        /// Zero or more concrete parameter suggestions derived from the error code and configuration.
        /// May be empty for diagnostics that do not have actionable parameter changes.
        /// </summary>
        public List<FlowParameterSuggestion> suggestions = new List<FlowParameterSuggestion>();
    }
}
