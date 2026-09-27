using System.Collections.Generic;
using static FightMatch.Core.BusinessFields;

namespace FightMatch.Core
{
    internal sealed class CandidateRosterSaveCodec
    {
        private readonly BusinessFields f;
        internal CandidateRosterSaveCodec(BusinessFields fields) { f = fields; }
        internal CandidateRosterState Roster(CandidateRosterState value, string p)
        {
            f.Required(value, p);
            var player = f.Text(value?.PlayerId, p + ".PlayerId");
            var permanent = new CandidatePermanentSaveCodec(f);
            var characters = f.List(value?.Characters, permanent.Character, p + ".Characters");
            var slots = Slots(f, value?.Formation, p + ".Formation");
            var revision = f.Integer(value?.FormationRevision ?? 0, p + ".FormationRevision", 1);
            var receipts = f.List(value?.FormationReceipts, (row, path) =>
            {
                f.Required(row, path);
                var operation = f.Text(row?.OperationId, path + ".OperationId");
                var before = f.Integer(row?.BeforeRevision ?? 0, path + ".BeforeRevision", 1);
                var after = f.Integer(row?.AfterRevision ?? 0, path + ".AfterRevision", 1);
                var original = Slots(f, row?.Before, path + ".Before");
                var next = Slots(f, row?.After, path + ".After");
                return f.Reading ? new CandidateFormationReceipt(operation, before, after, original, next) : row;
            }, p + ".FormationReceipts");
            var effects = f.SchemaVersion == 4 ? new CandidatePermanentCodec(f).Effects(value?.GetPermanentEffects(), p + ".Permanent", 3) : null;
            var result = f.Reading ? new CandidateRosterState(player, characters, slots, revision, receipts, effects) : value;
            result.Check(f.Budget); return result;
        }
        internal static IReadOnlyList<string> Slots(BusinessFields f, IReadOnlyList<string> value, string path)
        {
            var slots = f.List(value, (x, p) => f.Text(x, p, true), path);
            Need(slots.Count == 3, path); return slots.AsReadOnly();
        }
    }
}
