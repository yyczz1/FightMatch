using System;
using System.Collections.Generic;
using System.Numerics;
using static FightMatch.Core.BusinessFields;

namespace FightMatch.Core
{
    public static class CandidateRosterProtocol
    {
        public static SaveCodecResult<CandidateBusinessSnapshot> Migrate(CandidateApplicationSnapshot basis,
            PreparedCandidateApplicationIntent intent, SaveCodecBudget budget)
        {
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            return SaveCodecResult<CandidateBusinessSnapshot>.Run(() =>
            {
                CheckSource(basis, intent);
                var s = basis.Business; var old = s.Inventory;
                var inventory = new CandidateInventoryState(old.Definition, old.PlayerId, old.Holdings, old.Loadouts,
                    old.ActiveCarry, old.OrdinaryGrants, old.Ends, old.StateRevision, old.PreferenceRevision, true);
                return Take(CandidateBusinessSaveCodec.Prepare(new CandidateBusinessInput(s.PlayerId, s.Roster, inventory,
                    s.Progression, s.Rewards, s.ActiveHistory, s.RetainedRuns, s.RetainedRollbacks,
                    CandidateBusinessFormat.PublishedRosterV3), SavePurpose.PlayerSave, budget), "Migration");
            });
        }
        private static void CheckSource(CandidateApplicationSnapshot basis, PreparedCandidateApplicationIntent intent)
        {
            Need(basis != null && intent?.Kind == CandidateApplicationKind.MigrateRoster &&
                basis.Business.Format == CandidateBusinessFormat.PublishedV2, "Migration.Source", "UnsupportedSchema");
            Need(basis.Business.ActiveHistory == null && basis.Continuation == null, "Migration.Source", "ActiveAttemptConflict");
            var source = intent.Data.MigrateRoster;
            Need(intent.PlayerId == basis.Business.PlayerId && intent.ExpectedCommitId == basis.Header.CommitId &&
                source.SourceGeneration == basis.Header.SaveGeneration && source.SourceDescriptorLength == basis.Descriptor.TotalLength &&
                CandidateApplicationIntentCodec.SameBytes(source.SourceDescriptorSha256, basis.Descriptor.Sha256),
                "Migration.SourceDescriptor", "StaleContext");
        }
        internal static CandidateFormationReceipt Formation(CandidateBusinessSnapshot s, string operation)
        {
            return CandidatePermanentSaveCodec.Find(s.Roster.FormationReceipts, x => x.OperationId == operation, "Formation.Receipt");
        }
        internal static bool Transition(CandidateApplicationSnapshot basis, CandidateBusinessSnapshot after,
            PreparedCandidateApplicationIntent intent, SaveCodecBudget budget)
        {
            if (intent.Kind != CandidateApplicationKind.MigrateRoster && intent.Kind != CandidateApplicationKind.SetFormation) return false;
            var before = basis.Business;
            Need(before.ActiveHistory == null && basis.Continuation == null && after.ActiveHistory == null,
                "Roster.ActiveAttempt", "ActiveAttemptConflict");
            Need(ReferenceEquals(before.Progression, after.Progression) && ReferenceEquals(before.Rewards, after.Rewards),
                "Roster.Owners", "InconsistentBinding");
            SameRows(before.Roster.Characters, after.Roster.Characters);
            SameRows(before.RetainedRuns, after.RetainedRuns); SameRows(before.RetainedRollbacks, after.RetainedRollbacks);
            if (intent.Kind == CandidateApplicationKind.MigrateRoster)
            {
                CheckSource(basis, intent);
                Need(after.Format == CandidateBusinessFormat.PublishedRosterV3 && after.Roster.FormationRevision.IsOne &&
                    after.Roster.FormationReceipts.Count == 0 && CandidateRosterState.SameSlots(before.Roster.Formation, after.Roster.Formation),
                    "Migration.Roster", "InconsistentBinding");
                var a = before.Inventory; var b = after.Inventory;
                Need(b.IsRoster && ReferenceEquals(a.Definition, b.Definition) && a.PlayerId == b.PlayerId &&
                    a.StateRevision == b.StateRevision && a.PreferenceRevision == b.PreferenceRevision &&
                    ReferenceEquals(a.ActiveCarry, b.ActiveCarry), "Migration.Inventory", "InconsistentBinding");
                SameRows(a.Holdings, b.Holdings); SameRows(a.Loadouts, b.Loadouts);
                SameRows(a.OrdinaryGrants, b.OrdinaryGrants); SameRows(a.Ends, b.Ends);
            }
            else
            {
                Need((int)before.Format >= 3 && after.Format == before.Format &&
                    ReferenceEquals(before.Inventory, after.Inventory), "Formation.Owners", "InconsistentBinding");
                var expected = Take(before.Roster.SetFormation(intent.OperationId, intent.Data.SetFormation.Slots,
                    intent.Data.SetFormation.ExpectedFormationRevision.Value, budget), "Formation");
                Need(after.Roster.FormationRevision == expected.FormationRevision &&
                    CandidateRosterState.SameSlots(after.Roster.Formation, expected.Formation) &&
                    after.Roster.FormationReceipts.Count == before.Roster.FormationReceipts.Count + 1, "Formation.Result", "InconsistentBinding");
                for (var i = 0; i < before.Roster.FormationReceipts.Count; i++)
                    Need(ReferenceEquals(before.Roster.FormationReceipts[i], after.Roster.FormationReceipts[i]), "Formation.History", "InconsistentBinding");
            }
            return true;
        }
        private static void SameRows<T>(IReadOnlyList<T> before, IReadOnlyList<T> after) where T : class
        {
            Need(before.Count == after.Count, "Roster.Owners", "InconsistentBinding");
            for (var i = 0; i < before.Count; i++) Need(ReferenceEquals(before[i], after[i]), "Roster.Owners", "InconsistentBinding");
        }
        internal static void Resolve(CandidateBusinessSnapshot s, CandidateApplicationRecord record)
        {
            Need((int)s.Format >= 3 &&
                RuleContextChecks.Same(record.Intent.Context, s.Progression.Definition.Context), "Roster.Context", "InconsistentBinding");
            if (record.Intent.Kind == CandidateApplicationKind.SetFormation)
            {
                var actual = record.Result.Formation; var input = record.Intent.Data.SetFormation;
                Need(ReferenceEquals(actual, Formation(s, record.OperationId)) && actual.BeforeRevision == input.ExpectedFormationRevision &&
                    CandidateRosterState.SameSlots(actual.After, input.Slots), "Formation.Result", "InconsistentBinding");
            }
            else Need(record.Result.Migration != null && record.Result.Migration.SourceCommitId == record.Intent.ExpectedCommitId,
                "Migration.Result", "InconsistentBinding");
        }
        internal static CandidateApplicationRecoveryInput RecoveryInput(CandidateFormationEntryInput input, CandidateApplicationRecoveryReceipt receipt)
        {
            var selected = CandidatePermanentSaveCodec.Find(input.SelectedCharacters, x => x.CharacterId == receipt.CharacterId, "EnterFormation.Recovery.Character");
            return new CandidateApplicationRecoveryInput { CharacterId = receipt.CharacterId, RecoveryId = receipt.RecoveryId,
                ExpectedCharacterRevision = selected.ExpectedRevision, TimeSample = input.TimeSample };
        }
        internal static void EntryRecord(CandidateApplicationRecord record, CandidateProgressionBeginReceipt begin, SaveCodecBudget budget)
        {
            var input = record.Intent.Data.EnterFormation;
            Need(begin.FormationRevision == input.ExpectedFormationRevision && begin.ChallengeId == record.Result.ChallengeId &&
                begin.EntryBaselineId == record.Result.EntryBaselineId && begin.Level.LevelId == input.LevelId &&
                begin.Level.LevelVersion == input.LevelVersion, "EnterFormation.Begin", "InconsistentBinding");
            var recovered = new Dictionary<string, CandidateApplicationRecoveryReceipt>(StringComparer.Ordinal);
            foreach (var receipt in record.Result.RecoveryResults)
            {
                Need(!recovered.ContainsKey(receipt.CharacterId), "EnterFormation.Recoveries", "ReceiptConflict");
                RecoveryInput(input, receipt); recovered.Add(receipt.CharacterId, receipt);
            }
            foreach (var participant in begin.Participants)
            {
                var selected = CandidatePermanentSaveCodec.Find(input.SelectedCharacters, x => x.CharacterId == participant.CharacterId, "EnterFormation.Participant");
                var revision = selected.ExpectedRevision.Value;
                if (recovered.TryGetValue(participant.CharacterId, out var recovery)) revision = recovery.AfterCharacterRevision;
                Need(participant.OriginalSlot >= 0 && participant.OriginalSlot < 3 &&
                    input.Slots[participant.OriginalSlot] == participant.CharacterId && participant.CharacterRevision == revision,
                    "EnterFormation.Participant", "InconsistentBinding");
            }
        }
        internal static void Joint(CandidateBusinessSnapshot before, CandidateBusinessSnapshot after,
            CandidateFormationEntryInput input, IReadOnlyList<CandidateCharacterResult> recoveries, SaveCodecBudget budget)
        {
            Need((int)before.Format >= 3 && before.ActiveHistory == null &&
                before.Roster.FormationRevision == input.ExpectedFormationRevision &&
                CandidateRosterState.SameSlots(before.Roster.Formation, input.Slots) &&
                before.Inventory.StateRevision == input.ExpectedInventoryRevision &&
                before.Progression.StateRevision == input.ExpectedProgressionRevision, "EnterFormation.Expected", "StaleContext");
            var recoveryCount = 0; var readyCount = 0;
            foreach (var selected in input.SelectedCharacters)
            {
                var old = before.Roster.Find(selected.CharacterId); var next = after.Roster.Find(selected.CharacterId);
                Need(old != null && old.StateRevision == selected.ExpectedRevision, "EnterFormation.CharacterRevision", "StaleContext");
                if (old.ActiveRecovery != null)
                {
                    var result = CandidatePermanentSaveCodec.Find(recoveries, x => x.Next.CharacterId == old.CharacterId, "EnterFormation.Recovery");
                    Need(result.RecoveryPeriod.RecoveryId == old.ActiveRecovery.RecoveryId && ReferenceEquals(result.Next, next),
                        "EnterFormation.Recovery", "InconsistentBinding");
                    recoveryCount++;
                }
                else Need(ReferenceEquals(old, next), "EnterFormation.Character", "InconsistentBinding");
                if (next.IsReady) readyCount++;
            }
            Need(recoveryCount == recoveries.Count && readyCount == after.ActiveHistory.CurrentRun.Baseline.Entry.ReadyParticipants.Count,
                "EnterFormation.Coverage", "IncompleteOperationHistory");
        }
        internal static void History(CandidateBusinessSnapshot business, IReadOnlyList<CandidateApplicationRecord> records, SaveCodecBudget budget)
        {
            var initial = records[0].Intent; var version = initial.FormatVersion;
            IReadOnlyList<string> slots;
            if (version >= 3) slots = initial.Data.RosterInitialize.Slots;
            else { var original = new string[3]; original[initial.Data.InitializeProfile.OriginalSlot.Value] = initial.Data.InitializeProfile.CharacterId; slots = original; }
            var revision = BigInteger.One; var formationCount = 0;
            for (var i = 1; i < records.Count; i++)
            {
                var record = records[i]; var kind = record.Intent.Kind;
                if (kind == CandidateApplicationKind.MigrateRoster)
                {
                    Need(version == 2 && record.Result.Migration != null && record.Result.Migration.SourceGeneration == i,
                        "Migration.History", "IncompleteOperationHistory");
                    version = 3;
                }
                else if (kind == CandidateApplicationKind.MigratePermanent)
                {
                    Need(version == 3 && record.Intent.GetPermanentMigration().SourceGeneration == i, "Permanent.Migration.History");
                    version = 4;
                }
                else if (kind == CandidateApplicationKind.PermanentRequest) Need(version == 4, "Permanent.Format");
                else if (kind == CandidateApplicationKind.SetFormation)
                {
                    Need(version >= 3, "Formation.Format", "RosterMigrationRequired");
                    var receipt = record.Result.Formation;
                    Need(receipt.BeforeRevision == revision && CandidateRosterState.SameSlots(receipt.Before, slots),
                        "Formation.History", "InconsistentBinding");
                    slots = receipt.After; revision = receipt.AfterRevision; formationCount++;
                }
                else if (kind == CandidateApplicationKind.EnterFormation)
                    Need(version >= 3 && record.Intent.Data.EnterFormation.ExpectedFormationRevision == revision &&
                        CandidateRosterState.SameSlots(record.Intent.Data.EnterFormation.Slots, slots), "EnterFormation.Formation", "InconsistentBinding");
                else if (kind == CandidateApplicationKind.EnterAttempt)
                    Need(version < 3, "EnterAttempt.Format", "UnsupportedBinding");
            }
            Need((uint)business.Format == version && business.Roster.FormationRevision == revision &&
                CandidateRosterState.SameSlots(business.Roster.Formation, slots) &&
                business.Roster.FormationReceipts.Count == formationCount, "Roster.History", "IncompleteOperationHistory");
        }
    }
}
