using System;

namespace FlowPuzzle.Editor.Draft
{
    [Serializable]
    public sealed class FlowDraftMutationResult
    {
        public bool success;
        public string errorCode;
        public string errorMessage;

        public static FlowDraftMutationResult Ok()
            => new FlowDraftMutationResult { success = true };

        public static FlowDraftMutationResult Fail(string code, string message)
            => new FlowDraftMutationResult { success = false, errorCode = code, errorMessage = message };
    }
}
