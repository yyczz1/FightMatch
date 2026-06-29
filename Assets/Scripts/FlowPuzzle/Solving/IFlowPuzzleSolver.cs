using System;
using System.Threading;

namespace FlowPuzzle.Solving
{
    public interface IFlowPuzzleSolver
    {
        FlowSolveResult Solve(
            FlowSolveRequest request,
            IProgress<FlowSolveProgress> progress,
            CancellationToken cancellationToken);
    }
}
