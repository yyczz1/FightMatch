namespace FlowPuzzle.Core
{
    public static class FlowDiagnosticCodes
    {
        public const string InvalidDimensions = "InvalidDimensions";
        public const string ImpossibleMinimumOccupancy = "ImpossibleMinimumOccupancy";
        public const string ImpossibleCoverageRange = "ImpossibleCoverageRange";
        public const string PathGenerationFailed = "PathGenerationFailed";
        public const string CoverageOutOfRange = "CoverageOutOfRange";
        public const string ValidationFailed = "ValidationFailed";
        public const string DifficultyOutOfRange = "DifficultyOutOfRange";
        public const string MaxLevelAttemptsReached = "MaxLevelAttemptsReached";
        public const string InvalidFixedConstraint = "InvalidFixedConstraint";
        public const string SolverTimeout = "SolverTimeout";
        public const string SolverCancelled = "SolverCancelled";
        public const string NoSolution = "NoSolution";
        public const string AssetAlreadyExists = "AssetAlreadyExists";
        public const string InvalidOutputFolder = "InvalidOutputFolder";
    }
}
