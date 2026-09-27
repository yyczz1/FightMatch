namespace FightMatch.Core
{
    public enum BattleEntryRejectionCode
    {
        MissingField, InvalidValue, DuplicateIdentity,
        InconsistentBinding, UnsupportedBinding, NoReadyMember
    }

    public sealed class BattleEntryPreparationResult
    {
        public bool IsAccepted => Entry != null;
        public PreparedBattleEntry Entry { get; }
        public BattleEntryRejectionCode? RejectionCode { get; }
        public string FieldPath { get; }

        internal BattleEntryPreparationResult(PreparedBattleEntry entry)
        {
            Entry = entry;
        }

        internal BattleEntryPreparationResult(BattleEntryRejectionCode code, string fieldPath)
        {
            RejectionCode = code;
            FieldPath = fieldPath;
        }
    }
}
