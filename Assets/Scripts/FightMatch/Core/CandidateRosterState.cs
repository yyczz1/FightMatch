using System;
using System.Collections.Generic;
using System.Numerics;
using static FightMatch.Core.BusinessFields;

namespace FightMatch.Core
{
    public sealed class CandidateFormationReceipt
    {
        public string OperationId { get; }
        public BigInteger BeforeRevision { get; }
        public BigInteger AfterRevision { get; }
        public IReadOnlyList<string> Before { get; }
        public IReadOnlyList<string> After { get; }
        internal CandidateFormationReceipt(string operation, BigInteger beforeRevision, BigInteger afterRevision,
            IReadOnlyList<string> before, IReadOnlyList<string> after)
        {
            OperationId = operation; BeforeRevision = beforeRevision; AfterRevision = afterRevision;
            Before = new List<string>(before).AsReadOnly(); After = new List<string>(after).AsReadOnly();
        }
    }

    public sealed class CandidateRosterEntry
    {
        public CandidateCharacterState Character { get; }
        public CandidateComputedStats Stats { get; }
        public int OriginalSlot { get; }
        internal CandidateRosterEntry(CandidateCharacterState character, int slot, ExactMathBudget budget)
        { Character = character; OriginalSlot = slot; Stats = CandidateCharacterGrowth.ComputeBaseStats(character, budget); }
    }

    public sealed class CandidateRosterEntryPreparation
    {
        public CandidateRosterState Roster { get; }
        public IReadOnlyList<CandidateRosterEntry> Participants { get; }
        public IReadOnlyList<CandidateCharacterResult> Recoveries { get; }
        internal CandidateRosterEntryPreparation(CandidateRosterState roster, IEnumerable<CandidateRosterEntry> participants,
            IEnumerable<CandidateCharacterResult> recoveries)
        {
            Roster = roster; Participants = new List<CandidateRosterEntry>(participants).AsReadOnly();
            Recoveries = new List<CandidateCharacterResult>(recoveries).AsReadOnly();
        }
    }

    // M03 owns both the held instances and current formation. Entry slots are frozen separately.
    public sealed class CandidateRosterState
    {
        private readonly IReadOnlyList<CandidatePermanentEffect> permanentEffects;
        public IReadOnlyList<CandidatePermanentEffect> GetPermanentEffects() { return permanentEffects; }
        public string PlayerId { get; }
        public IReadOnlyList<CandidateCharacterState> Characters { get; }
        public IReadOnlyList<string> Formation { get; }
        public BigInteger FormationRevision { get; }
        public IReadOnlyList<CandidateFormationReceipt> FormationReceipts { get; }
        internal CandidateRosterState(string player, IReadOnlyList<CandidateCharacterState> characters,
            IReadOnlyList<string> formation, BigInteger revision, IReadOnlyList<CandidateFormationReceipt> receipts,
            IEnumerable<CandidatePermanentEffect> effects = null)
        {
            PlayerId = player; Characters = new List<CandidateCharacterState>(characters).AsReadOnly();
            Formation = new List<string>(formation).AsReadOnly(); FormationRevision = revision;
            FormationReceipts = new List<CandidateFormationReceipt>(receipts).AsReadOnly();
            permanentEffects = new List<CandidatePermanentEffect>(effects ?? new CandidatePermanentEffect[0]).AsReadOnly();
        }

        public static SaveCodecResult<CandidateRosterState> Create(string playerId,
            IReadOnlyList<CandidateCharacterState> characters, IReadOnlyList<string> slots, SaveCodecBudget budget)
        {
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            return SaveCodecResult<CandidateRosterState>.Run(() =>
            {
                Need(characters != null, "Roster.Characters", "MissingField");
                SaveCodecFailure.Limit((ulong)characters.Count, (ulong)budget.MaxCollectionEntries, "Roster.Characters", "CollectionEntries");
                var sorted = new List<CandidateCharacterState>(characters);
                Need(sorted.TrueForAll(x => x != null), "Roster.Characters", "MissingField");
                sorted.Sort((a, b) => StringComparer.Ordinal.Compare(a.CharacterId, b.CharacterId));
                var result = new CandidateRosterState(playerId, sorted, CheckSlots(slots, sorted, budget),
                    BigInteger.One, new CandidateFormationReceipt[0]);
                result.Check(budget); return result;
            });
        }

        public CandidateCharacterState Find(string characterId)
        { foreach (var character in Characters) if (character.CharacterId == characterId) return character; return null; }
        public bool TryGetSingle(out CandidateCharacterState character)
        { character = Characters.Count == 1 ? Characters[0] : null; return character != null; }
        internal static T Single<T>(IReadOnlyList<T> values) where T : class
        {
            if (values == null || values.Count == 0) return null;
            if (values.Count != 1) throw new InvalidOperationException("AmbiguousCharacter");
            return values[0];
        }
        internal static CandidateRosterState Legacy(CandidateCharacterState character)
        {
            if (character == null) return null;
            var slots = new string[3];
            if (character.OriginalSlot >= 0 && character.OriginalSlot < slots.Length) slots[character.OriginalSlot] = character.CharacterId;
            return new CandidateRosterState(character.PlayerId, new[] { character }, slots, BigInteger.One,
                new CandidateFormationReceipt[0]);
        }

        public SaveCodecResult<CandidateRosterState> SetFormation(string operationId, IReadOnlyList<string> slots,
            BigInteger expectedRevision, SaveCodecBudget budget)
        {
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            return SaveCodecResult<CandidateRosterState>.Run(() =>
            {
                Check(budget); RuleContextChecks.Text(operationId, "OperationId", budget, true);
                var nextSlots = CheckSlots(slots, Characters, budget);
                foreach (var prior in FormationReceipts) if (prior.OperationId == operationId)
                { Need(SameSlots(prior.After, nextSlots), "Formation.OperationId", "OperationConflict"); return this; }
                budget.Math.CheckInteger(expectedRevision);
                Need(expectedRevision == FormationRevision, "FormationRevision", "StaleContext");
                var revision = SameSlots(Formation, nextSlots) ? FormationRevision : budget.Math.Add(FormationRevision, BigInteger.One);
                SaveCodecFailure.Limit((ulong)FormationReceipts.Count + 1, (ulong)budget.MaxCollectionEntries, "FormationReceipts", "CollectionEntries");
                var receipts = new List<CandidateFormationReceipt>(FormationReceipts)
                { new CandidateFormationReceipt(operationId, FormationRevision, revision, Formation, nextSlots) };
                return new CandidateRosterState(PlayerId, Characters, nextSlots, revision, receipts, permanentEffects);
            });
        }

        public SaveCodecResult<CandidateRosterEntryPreparation> PrepareEntry(CandidateTimeSample sample, SaveCodecBudget budget)
        {
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            return SaveCodecResult<CandidateRosterEntryPreparation>.Run(() =>
            {
                Check(budget); Need(sample != null, "TimeSample", "MissingField");
                var timeCheck = new GrowthChecks(budget.Math);
                Need(CandidateRecoveryClock.PrepareTime(sample, timeCheck, out _), timeCheck.Path ?? "TimeSample", timeCheck.Code.ToString());
                var roster = this; var recoveries = new List<CandidateCharacterResult>();
                var participants = new List<CandidateRosterEntry>();
                for (var slot = 0; slot < 3; slot++)
                {
                    if (Formation[slot] == null) continue;
                    var character = Find(Formation[slot]);
                    if (character.ActiveRecovery != null)
                    {
                        var recovered = CandidateRecoveryClock.Advance(character, character.ActiveRecovery.RecoveryId,
                            sample, character.StateRevision, budget.Math);
                        Need(recovered.IsAccepted, recovered.FieldPath ?? "Recovery", recovered.RejectionCode.ToString());
                        recoveries.Add(recovered); character = recovered.Next; roster = roster.Replace(character);
                    }
                    if (character.IsReady) participants.Add(new CandidateRosterEntry(character, slot, budget.Math));
                }
                Need(participants.Count > 0, "Formation", "NoReadyMember");
                return new CandidateRosterEntryPreparation(roster, participants, recoveries);
            });
        }

        public CandidateRosterState Replace(CandidateCharacterState character)
        {
            Need(character != null && character.PlayerId == PlayerId && Find(character.CharacterId)?.ClassId == character.ClassId,
                "Roster.Character", "InconsistentBinding");
            var values = new List<CandidateCharacterState>();
            foreach (var old in Characters) values.Add(old.CharacterId == character.CharacterId ? character : old);
            return new CandidateRosterState(PlayerId, values, Formation, FormationRevision, FormationReceipts, permanentEffects);
        }
        internal void Check(SaveCodecBudget budget)
        {
            RuleContextChecks.Text(PlayerId, "Roster.PlayerId", budget, true);
            SaveCodecFailure.Limit((ulong)Characters.Count, (ulong)budget.MaxCollectionEntries, "Roster.Characters", "CollectionEntries");
            Need(Characters.Count > 0, "Roster.Characters");
            var classes = new HashSet<string>(StringComparer.Ordinal); string previous = null;
            foreach (var character in Characters)
            {
                Need(character != null && character.PlayerId == PlayerId, "Roster.Character", "InconsistentBinding");
                RuleContextChecks.Text(character.CharacterId, "Roster.CharacterId", budget, true);
                RuleContextChecks.Text(character.ClassId, "Roster.ClassId", budget, true);
                Need(previous == null || StringComparer.Ordinal.Compare(previous, character.CharacterId) < 0, "Roster.CharacterId");
                Need(classes.Add(character.ClassId), "Roster.ClassId"); previous = character.CharacterId;
                new GrowthChecks(budget.Math).CheckState(character);
            }
            CheckSlots(Formation, Characters, budget); budget.Math.CheckInteger(FormationRevision);
            Need(FormationRevision >= 1, "FormationRevision");
            SaveCodecFailure.Limit((ulong)FormationReceipts.Count, (ulong)budget.MaxCollectionEntries, "FormationReceipts", "CollectionEntries");
            var operations = new HashSet<string>(StringComparer.Ordinal); BigInteger revision = 1;
            IReadOnlyList<string> last = null;
            foreach (var receipt in FormationReceipts)
            {
                RuleContextChecks.Text(receipt.OperationId, "FormationReceipt.OperationId", budget, true);
                Need(operations.Add(receipt.OperationId), "FormationReceipt.OperationId");
                CheckSlots(receipt.Before, Characters, budget); CheckSlots(receipt.After, Characters, budget);
                var next = SameSlots(receipt.Before, receipt.After) ? revision : budget.Math.Add(revision, BigInteger.One);
                Need(receipt.BeforeRevision == revision && receipt.AfterRevision == next &&
                    (last == null || SameSlots(last, receipt.Before)), "FormationReceipt.Revision", "InconsistentBinding");
                revision = next; last = receipt.After;
            }
            Need(revision == FormationRevision && (last == null || SameSlots(last, Formation)), "FormationReceipts", "InconsistentBinding");
        }
        internal static bool SameSlots(IReadOnlyList<string> a, IReadOnlyList<string> b)
        {
            if (a == null || b == null || a.Count != 3 || b.Count != 3) return false;
            for (var i = 0; i < 3; i++) if (a[i] != b[i]) return false;
            return true;
        }
        private static string[] CheckSlots(IReadOnlyList<string> slots, IReadOnlyList<CandidateCharacterState> characters, SaveCodecBudget budget)
        {
            Need(slots != null && slots.Count == 3, "Formation");
            SaveCodecFailure.Limit(3, (ulong)budget.MaxCollectionEntries, "Formation", "CollectionEntries");
            var result = new string[3]; var seen = new HashSet<string>(StringComparer.Ordinal);
            for (var i = 0; i < 3; i++)
            {
                result[i] = slots[i]; if (slots[i] == null) continue;
                RuleContextChecks.Text(slots[i], "Formation.CharacterId", budget, true);
                Need(seen.Add(slots[i]), "Formation.CharacterId");
                var found = false;
                foreach (var character in characters) if (character.CharacterId == slots[i]) found = true;
                Need(found, "Formation.CharacterId", "InconsistentBinding");
            }
            return result;
        }
    }
}
