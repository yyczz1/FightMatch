using System;
using System.Threading;
using System.Threading.Tasks;

namespace FlowPuzzle.Solving
{
    public interface IFlowLevelCompletionProvider
    {
        string DisplayName { get; }
        Task<FlowCompletionResult> CompleteAsync(FlowCompletionRequest request, IProgress<FlowCompletionProgress> progress, CancellationToken ct);
    }
}
