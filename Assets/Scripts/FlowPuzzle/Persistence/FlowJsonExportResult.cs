using System;
using FlowPuzzle.Core;

namespace FlowPuzzle.Persistence
{
    [Serializable]
    public sealed class FlowJsonExportResult
    {
        public bool success;
        public string levelFilePath = string.Empty;
        public string solutionFilePath = string.Empty;
        public FlowFailureDiagnostic diagnostic;

        public static FlowJsonExportResult Success(string levelPath, string solutionPath)
        {
            return new FlowJsonExportResult
            {
                success = true,
                levelFilePath = levelPath ?? string.Empty,
                solutionFilePath = solutionPath ?? string.Empty,
                diagnostic = null
            };
        }

        public static FlowJsonExportResult Failure(string errorCode, string errorMessage)
        {
            return new FlowJsonExportResult
            {
                success = false,
                diagnostic = new FlowFailureDiagnostic
                {
                    errorCode = errorCode,
                    errorMessage = errorMessage
                }
            };
        }
    }
}
