using System;

namespace FlowPuzzle.Validation
{
    [Serializable]
    public sealed class FlowValidationResult
    {
        public bool isValid;
        public string errorCode;
        public string errorMessage;

        public static FlowValidationResult Valid()
        {
            return new FlowValidationResult
            {
                isValid = true,
                errorCode = null,
                errorMessage = null
            };
        }

        public static FlowValidationResult Invalid(string code, string message)
        {
            return new FlowValidationResult
            {
                isValid = false,
                errorCode = code,
                errorMessage = message
            };
        }
    }
}
