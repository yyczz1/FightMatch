using FlowPuzzle.Core;

namespace FlowPuzzle.Solving.Tools
{
    public static class FlowSolverToolContracts
    {
        public sealed class SolveRequest { public FlowLevelData level; }
        public sealed class ValidateRequest { public FlowLevelData level; public FlowSolutionData solution; }
        public sealed class EvaluateRequest { public FlowLevelData level; public FlowSolutionData solution; }
    }
}
