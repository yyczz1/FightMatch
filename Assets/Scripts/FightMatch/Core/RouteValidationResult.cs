namespace FightMatch.Core
{
    public sealed class RouteValidationResult
    {
        public bool IsValid { get; }
        public string ReasonCode { get; }
        public int? CellIndex { get; }

        internal RouteValidationResult(bool isValid, string reasonCode, int? cellIndex = null)
        {
            IsValid = isValid;
            ReasonCode = reasonCode;
            CellIndex = cellIndex;
        }
    }
}
