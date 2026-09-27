using System;

namespace FlowPuzzle.Core
{
    [Serializable]
    public sealed class FlowParameterSuggestion
    {
        public string parameterName;
        public string currentValue;
        public string suggestedDirection;
        public string suggestedValue;
        public string reason;

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(suggestedValue))
                return string.Format("{0}: {1} -> {2} ({3}) — {4}",
                    parameterName, currentValue, suggestedValue,
                    suggestedDirection, reason);
            return string.Format("{0}: {1} — {2} ({3})",
                parameterName, currentValue, suggestedDirection, reason);
        }
    }
}
