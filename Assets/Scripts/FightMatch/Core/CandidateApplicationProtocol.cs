using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using static FightMatch.Core.BusinessFields;

namespace FightMatch.Core
{
    public static class CandidateApplicationProtocol
    {
        public static SaveCodecResult<PreparedCandidateApplicationIntent> PrepareIntent(CandidateApplicationIntentInput input, SaveCodecBudget budget)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            return SaveCodecResult<PreparedCandidateApplicationIntent>.Run(() => CandidateApplicationIntentCodec.Prepare(input, budget));
        }

        public static SaveCodecResult<CandidateApplicationCandidate> Propose(CandidateApplicationSnapshot basis,
            CandidateBusinessSnapshot nextBusiness, PreparedCandidateApplicationIntent preparedIntent,
            CandidateApplicationResultInput result, string nextSettlementOperationId, SaveCodecBudget budget)
        {
            if (nextBusiness == null) throw new ArgumentNullException(nameof(nextBusiness));
            if (preparedIntent == null) throw new ArgumentNullException(nameof(preparedIntent));
            if (result == null) throw new ArgumentNullException(nameof(result));
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            return SaveCodecResult<CandidateApplicationCandidate>.Run(() =>
            {
                var intent = CandidateApplicationIntentCodec.Read(preparedIntent.Bytes, budget);
                if (basis != null)
                {
                    var prior = Find(basis, intent, budget);
                    Need(prior == null, "Intent.OperationId", "OperationAlreadyRecorded");
                }
                Need((basis == null) == (intent.Kind == CandidateApplicationKind.InitializeProfile), "Intent.Kind", "UnsupportedBinding");
                Need(intent.PlayerId == nextBusiness.PlayerId && (basis == null || basis.Business.PlayerId == intent.PlayerId), "Intent.PlayerId", "InconsistentBinding");
                Need(intent.ExpectedCommitId == basis?.Header.CommitId, "Intent.ExpectedCommitId", "StaleContext");
                var business = Retain(basis?.Business, nextBusiness, budget);
                var completion = Result(intent, result, business, budget);
                var records = basis == null ? new List<CandidateApplicationRecord>() : new List<CandidateApplicationRecord>(basis.Records);
                SaveCodecFailure.Limit((ulong)records.Count + 1, (ulong)budget.MaxCollectionEntries, "Records", "CollectionEntries");
                records.Add(new CandidateApplicationRecord(intent, completion));
                var references = new CandidateApplicationReferences(business, budget);
                var resolutions = references.Validate(records);
                if (basis == null) Initial(business, intent);
                else Transition(basis, business, intent, result, resolutions[intent.OperationId], budget);
                var continuation = Continue(basis, business, intent, nextSettlementOperationId, budget);
                CheckContinuation(business, records, continuation);
                return new CandidateApplicationCandidate(basis, business, records, continuation);
            });
        }

        public static SaveCodecResult<CandidateApplicationLookup> Lookup(CandidateApplicationSnapshot snapshot,
            PreparedCandidateApplicationIntent preparedIntent, SaveCodecBudget budget)
        {
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
            if (preparedIntent == null) throw new ArgumentNullException(nameof(preparedIntent));
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            return SaveCodecResult<CandidateApplicationLookup>.Run(() =>
            {
                var intent = CandidateApplicationIntentCodec.Read(preparedIntent.Bytes, budget);
                var record = Find(snapshot, intent, budget);
                if (record == null) return new CandidateApplicationLookup();
                var resolved = new CandidateApplicationReferences(snapshot.Business, budget).Validate(snapshot.Records);
                return new CandidateApplicationLookup(record, resolved[record.OperationId]);
            });
        }

        private static CandidateApplicationRecord Find(CandidateApplicationSnapshot snapshot, PreparedCandidateApplicationIntent intent, SaveCodecBudget budget)
        {
            SaveEnvelopeCodec.CheckList(snapshot.Records, budget, "Records");
            foreach (var record in snapshot.Records)
            {
                if (record.Intent.PlayerId != intent.PlayerId || record.OperationId != intent.OperationId) continue;
                Need(CandidateApplicationIntentCodec.SameBytes(record.Intent.CanonicalBytes, intent.CanonicalBytes), "Intent", "OperationConflict");
                return record;
            }
            return null;
        }

        private static CandidateBusinessSnapshot Retain(CandidateBusinessSnapshot basis, CandidateBusinessSnapshot next, SaveCodecBudget budget)
        {
            var runs = new List<CandidateBattleRun>();
            var rollbacks = new List<CandidateRollbackRecord>();
            if (basis != null)
            {
                Union(runs, basis.RetainedRuns, budget, "RetainedRuns");
                Union(rollbacks, basis.RetainedRollbacks, budget, "RetainedRollbacks");
            }
            Union(runs, next.RetainedRuns, budget, "RetainedRuns");
            Union(rollbacks, next.RetainedRollbacks, budget, "RetainedRollbacks");
            if (basis?.ActiveHistory != null && basis.ActiveHistory.CurrentRun.Baseline.Entry.AttemptId != next.ActiveHistory?.CurrentRun.Baseline.Entry.AttemptId)
            {
                Union(runs, new[] { basis.ActiveHistory.CurrentRun }, budget, "RetainedRuns");
                Union(rollbacks, basis.ActiveHistory.RollbackRecords, budget, "RetainedRollbacks");
            }
            return Take(CandidateBusinessSaveCodec.Prepare(new CandidateBusinessInput(next.PlayerId, next.Roster, next.Inventory,
                next.Progression, next.Rewards, next.ActiveHistory, runs, rollbacks, next.Format),
                next.Format == CandidateBusinessFormat.CandidateV1 ? SavePurpose.CandidateValidation : SavePurpose.PlayerSave, budget), "Business");
        }

        private static void Union<T>(List<T> target, IReadOnlyList<T> source, SaveCodecBudget budget, string path) where T : class
        {
            SaveEnvelopeCodec.CheckList(source, budget, path);
            foreach (var value in source)
            {
                var exists = false;
                foreach (var old in target) if (ReferenceEquals(old, value)) { exists = true; break; }
                if (exists) continue;
                SaveCodecFailure.Limit((ulong)target.Count + 1, (ulong)budget.MaxCollectionEntries, path, "CollectionEntries");
                target.Add(value);
            }
        }

        private static CandidateApplicationResult Result(PreparedCandidateApplicationIntent intent, CandidateApplicationResultInput input,
            CandidateBusinessSnapshot business, SaveCodecBudget budget)
        {
            var kind = intent.Kind;
            var fields = new[] { input.ChallengeId, input.AttemptId, input.EntryBaselineId, input.HistoryAnchorId,
                input.EndReceiptId, input.SettlementId, input.NewAttemptId };
            var mask = kind == CandidateApplicationKind.EnterAttempt || kind == CandidateApplicationKind.EnterFormation ? 7 : kind == CandidateApplicationKind.Attack || kind == CandidateApplicationKind.Link ? 8 :
                kind == CandidateApplicationKind.SettleVictory ? 48 : kind == CandidateApplicationKind.ExitAttempt ? 16 : kind == CandidateApplicationKind.RestartAttempt ? 80 : 0;
            var f = new BusinessFields(Stream.Null, false, budget);
            for (var i = 0; i < fields.Length; i++)
            {
                var required = (mask & (1 << i)) != 0;
                Need(required == (fields[i] != null), "Result.Fields[" + i + "]", required ? "MissingField" : "InvalidValue");
                if (required) f.Text(fields[i], "Result.Fields[" + i + "]");
            }
            Need((kind == CandidateApplicationKind.AdvanceRecovery) == (input.RecoveryResult != null), "Result.RecoveryResult", "InvalidValue");
            CandidateApplicationRecoveryReceipt recovery = null;
            if (input.RecoveryResult != null)
            {
                var actual = input.RecoveryResult;
                Need(actual.IsAccepted && ReferenceEquals(actual.Next, business.Roster.Find(actual.Next.CharacterId)) && actual.RecoveryPeriod != null &&
                    (int)actual.Outcome >= 1 && (int)actual.Outcome <= 4, "Result.RecoveryResult", "InconsistentBinding");
                recovery = new CandidateApplicationRecoveryReceipt(actual.Next.StateRevision, actual.Outcome, actual.RecoveryPeriod, actual.Anomaly);
            }
            Need((kind == CandidateApplicationKind.EnterFormation) == (input.RecoveryResults != null), "Result.RecoveryResults", "InvalidValue");
            var recoveries = new List<CandidateApplicationRecoveryReceipt>();
            if (input.RecoveryResults != null)
            {
                SaveEnvelopeCodec.CheckList(input.RecoveryResults, budget, "Result.RecoveryResults");
                foreach (var actual in input.RecoveryResults)
                {
                    Need(actual != null && actual.IsAccepted && ReferenceEquals(actual.Next, business.Roster.Find(actual.Next.CharacterId)) &&
                        actual.RecoveryPeriod != null && (int)actual.Outcome >= 1 && (int)actual.Outcome <= 4, "Result.RecoveryResults", "InconsistentBinding");
                    recoveries.Add(new CandidateApplicationRecoveryReceipt(actual.Next.StateRevision, actual.Outcome, actual.RecoveryPeriod, actual.Anomaly));
                }
            }
            var formation = kind == CandidateApplicationKind.SetFormation ? CandidateRosterProtocol.Formation(business, intent.OperationId) : null;
            var migration = kind == CandidateApplicationKind.MigrateRoster ? new CandidateRosterMigrationReceipt(intent) : null;
            return new CandidateApplicationResult(input, recovery, formation, migration, input.RecoveryResults == null ? null : recoveries,
                CandidatePermanentProtocol.Result(business, intent));
        }

        internal static void Initial(CandidateBusinessSnapshot s, PreparedCandidateApplicationIntent intent)
        {
            Need((uint)s.Format == intent.FormatVersion, "InitializeProfile.Format", "InconsistentBinding");
            var recipes = intent.FormatVersion >= 3 ? intent.Data.RosterInitialize.Characters : new[] { intent.Data.InitializeProfile };
            foreach (var recipe in recipes)
            {
                var c = s.Roster.Find(recipe.CharacterId);
                Need(c != null && c.Level == recipe.InitialLevel && c.Experience == recipe.InitialExperience && c.StateRevision.IsOne &&
                    c.BaseRewards.Count == 0 && c.ProcessedEnds.Count == 0 && c.RecoveryPeriods.Count == 0, "InitializeProfile.Character", "InconsistentBinding");
            }
            Need(s.Roster.FormationRevision.IsOne && s.Roster.FormationReceipts.Count == 0 &&
                (intent.FormatVersion < 3 || CandidateRosterState.SameSlots(s.Roster.Formation, intent.Data.RosterInitialize.Slots)),
                "InitializeProfile.Formation", "InconsistentBinding");
            Need(s.Roster.GetPermanentEffects().Count == 0 && s.Inventory.GetPermanentLedger().Sources.Count == 0 &&
                s.Inventory.GetPermanentLedger().Effects.Count == 0 && s.Progression.GetPermanentEffects().Count == 0, "InitializeProfile.Permanent");
            var inventory = s.Inventory;
            Need(inventory.StateRevision.IsOne && inventory.PreferenceRevision.IsOne && inventory.ActiveCarry == null &&
                inventory.OrdinaryGrants.Count == 0 && inventory.Ends.Count == 0, "InitializeProfile.Inventory", "InconsistentBinding");
            foreach (var loadout in inventory.Loadouts) Need(loadout.ItemId == null && loadout.Enabled == null && loadout.L.IsZero,
                "InitializeProfile.Inventory", "InconsistentBinding");
            foreach (var holding in inventory.Holdings) Need(holding.T.IsZero, "InitializeProfile.Holdings", "InconsistentBinding");
            Need(s.Progression.StateRevision.IsOne && s.Progression.Challenges.Count == 0 && s.Progression.FirstClears.Count == 0,
                "InitializeProfile.Progression", "InconsistentBinding");
            foreach (var open in s.Progression.OpenFacts)
                Need(open.Level.UnlockKind == CandidateProgressionUnlockKind.InitiallyOpen, "InitializeProfile.OpenFacts", "InconsistentBinding");
            Need(s.Rewards.StateRevision.IsOne && s.Rewards.BaseRewards.Count == 0 && s.ActiveHistory == null &&
                s.RetainedRuns.Count == 0 && s.RetainedRollbacks.Count == 0, "InitializeProfile.History", "IncompleteOperationHistory");
        }

        private static void Same(bool condition, string path) { Need(condition, "Transition." + path, "InconsistentBinding"); }

        private static void Prefix<T>(IReadOnlyList<T> before, IReadOnlyList<T> after, int added, string path) where T : class
        {
            Same(added >= 0 && after.Count == before.Count + added, path);
            for (var i = 0; i < before.Count; i++) Same(ReferenceEquals(before[i], after[i]), path);
        }

        private static void Transition(CandidateApplicationSnapshot basis, CandidateBusinessSnapshot after,
            PreparedCandidateApplicationIntent intent, CandidateApplicationResultInput actual, CandidateApplicationResolution result, SaveCodecBudget budget)
        {
            var before = basis.Business;
            if (CandidatePermanentProtocol.Transition(basis, after, intent, budget)) return;
            if (CandidateRosterProtocol.Transition(basis, after, intent, budget)) return;
            var kind = intent.Kind;
            var action = kind == CandidateApplicationKind.Attack || kind == CandidateApplicationKind.Link || kind == CandidateApplicationKind.Rollback;
            var enter = kind == CandidateApplicationKind.EnterAttempt || kind == CandidateApplicationKind.EnterFormation;
            var recover = kind == CandidateApplicationKind.AdvanceRecovery;
            var win = kind == CandidateApplicationKind.SettleVictory;
            var end = kind == CandidateApplicationKind.ExitAttempt || kind == CandidateApplicationKind.RestartAttempt || win;
            Same(before.Format == after.Format && ReferenceEquals(before.Inventory.Definition, after.Inventory.Definition) &&
                ReferenceEquals(before.Progression.Definition, after.Progression.Definition), "Definitions");
            Same(before.Roster.Characters.Count == after.Roster.Characters.Count && before.Roster.FormationRevision == after.Roster.FormationRevision &&
                CandidateRosterState.SameSlots(before.Roster.Formation, after.Roster.Formation), "Roster");
            Prefix(before.Roster.FormationReceipts, after.Roster.FormationReceipts, 0, "FormationReceipts");
            Same(before.Inventory.Loadouts.Count == after.Inventory.Loadouts.Count &&
                before.Inventory.PreferenceRevision == after.Inventory.PreferenceRevision, "Preference");
            for (var i = 0; i < before.Inventory.Loadouts.Count; i++)
            {
                var oldLoadout = before.Inventory.Loadouts[i]; var loadout = after.Inventory.Loadouts[i];
                Same(oldLoadout.Actor.CharacterId == loadout.Actor.CharacterId && oldLoadout.ItemId == loadout.ItemId &&
                    oldLoadout.Enabled == loadout.Enabled && oldLoadout.L == loadout.L, "Preference");
            }
            if (action || enter) Same(ReferenceEquals(before.Rewards, after.Rewards), "Owners");
            if (action || recover) Same(ReferenceEquals(before.Inventory, after.Inventory) && ReferenceEquals(before.Progression, after.Progression), "Owners");
            if (recover) Same(ReferenceEquals(before.Rewards, after.Rewards) && ReferenceEquals(before.ActiveHistory, after.ActiveHistory), "Owners");
            if (action && kind != CandidateApplicationKind.Rollback)
            {
                Same(ReferenceEquals(before.ActiveHistory?.CurrentRun.CurrentSnapshot, result.BattleOperation.BeforeSnapshot), "BeforeSnapshot");
                var found = false;
                foreach (var entry in after.ActiveHistory.Archive) if (entry.OperationId == intent.OperationId)
                    found = ReferenceEquals(entry.Record, result.BattleOperation) && entry.HistoryAnchorId == actual.HistoryAnchorId;
                Same(found, "HistoryAnchorId");
                if (kind == CandidateApplicationKind.Attack)
                    Same(intent.Data.Attack.ExpectedPreferenceRevision == before.Inventory.PreferenceRevision, "PreferenceRevision");
            }
            if (kind == CandidateApplicationKind.Rollback)
                Same(ReferenceEquals(before.ActiveHistory?.CurrentRun, result.Rollback.BeforeRun), "Rollback.BeforeRun");
            foreach (var old in before.Roster.Characters)
            {
                var next = after.Roster.Find(old.CharacterId);
                Same(next != null && ReferenceEquals(old.Definition, next.Definition), "Definitions");
                CandidateCharacterEndReceipt ending = null;
                foreach (var receipt in result.CharacterEnds) if (receipt.CharacterId == old.CharacterId) ending = receipt;
                var recovered = recover && intent.Data.AdvanceRecovery.CharacterId == old.CharacterId;
                if (kind == CandidateApplicationKind.EnterFormation)
                    foreach (var receipt in actual.RecoveryResults) if (receipt.Next.CharacterId == old.CharacterId) recovered = true;
                Prefix(old.BaseRewards, next.BaseRewards, win && ending != null ? 1 : 0, "Character.Rewards");
                Prefix(old.ProcessedEnds, next.ProcessedEnds, ending == null ? 0 : 1, "Character.Ends");
                if (ending != null)
                {
                    Same(next.StateRevision == budget.Math.Add(old.StateRevision, win ? 2 : 1), "Character.Revision");
                    if (!win) Same(old.Level == next.Level && old.Experience == next.Experience, "End.Owners");
                    Prefix(old.RecoveryPeriods, next.RecoveryPeriods, ending.RecoveryId == null ? 0 : 1, "RecoveryPeriods");
                }
                else if (!recovered) Same(ReferenceEquals(old, next), "Owners");
            }
            Prefix(before.Inventory.OrdinaryGrants, after.Inventory.OrdinaryGrants, win ? 1 : 0, "Inventory.Grants");
            Prefix(before.Inventory.Ends, after.Inventory.Ends, end ? 1 : 0, "Inventory.Ends");
            Prefix(before.Rewards.BaseRewards, after.Rewards.BaseRewards, win ? 1 : 0, "Rewards");
            ProgressionPrefix(before.Progression, after.Progression);
            if (enter)
            {
                Same(before.ActiveHistory == null && after.ActiveHistory != null && after.ActiveHistory.CurrentRun.Records.Count == 0 &&
                    after.ActiveHistory.Archive.Count == 0 && after.ActiveHistory.RollbackRecords.Count == 0, "Entry.History");
                if (kind == CandidateApplicationKind.EnterAttempt)
                {
                    Same((int)before.Format < 3, "Entry.Format");
                    Same(before.Character.StateRevision == intent.Data.EnterAttempt.ExpectedCharacterRevision && before.Character.IsReady, "Entry.Character");
                }
                else
                {
                    CandidateRosterProtocol.Joint(before, after, intent.Data.EnterFormation, actual.RecoveryResults, budget);
                    foreach (var recovery in actual.RecoveryResults)
                        RecoveryTransition(before.Roster.Find(recovery.Next.CharacterId), after.Roster.Find(recovery.Next.CharacterId),
                            CandidateRosterProtocol.RecoveryInput(intent.Data.EnterFormation,
                                new CandidateApplicationRecoveryReceipt(recovery.Next.StateRevision, recovery.Outcome, recovery.RecoveryPeriod, recovery.Anomaly)), recovery, budget);
                }
            }
            if (end || enter)
            {
                Same(after.Inventory.StateRevision == budget.Math.Add(before.Inventory.StateRevision, 1) &&
                    after.Progression.StateRevision == budget.Math.Add(before.Progression.StateRevision, 1), "OwnerRevisions");
            }
            if (end)
            {
                if (win) Same(after.Rewards.StateRevision == budget.Math.Add(before.Rewards.StateRevision, 1), "Rewards.Revision");
                else Same(ReferenceEquals(before.Rewards, after.Rewards), "End.Owners");
            }
            if (recover) RecoveryTransition(before.Roster.Find(intent.Data.AdvanceRecovery.CharacterId),
                after.Roster.Find(intent.Data.AdvanceRecovery.CharacterId), intent.Data.AdvanceRecovery, actual.RecoveryResult, budget);
        }

        private static void ProgressionPrefix(CandidateProgressionState before, CandidateProgressionState after)
        {
            Same(after.Challenges.Count >= before.Challenges.Count, "Challenges");
            for (var i = 0; i < before.Challenges.Count; i++)
            {
                var old = before.Challenges[i];
                var next = after.Challenges[i];
                Same(old.ChallengeId == next.ChallengeId && SameLevel(old.Level, next.Level) && next.Attempts.Count >= old.Attempts.Count &&
                    (old.ClosedBy == null || ReferenceEquals(old.ClosedBy, next.ClosedBy)), "Challenges");
                for (var j = 0; j < old.Attempts.Count; j++)
                    Same(ReferenceEquals(old.Attempts[j].Begin, next.Attempts[j].Begin) &&
                        (old.Attempts[j].End == null || ReferenceEquals(old.Attempts[j].End, next.Attempts[j].End)), "Attempts");
            }
            Prefix(before.FirstClears, after.FirstClears, after.FirstClears.Count - before.FirstClears.Count, "FirstClears");
            Prefix(before.OpenFacts, after.OpenFacts, after.OpenFacts.Count - before.OpenFacts.Count, "OpenFacts");
        }

        private static bool SameLevel(CandidateProgressionLevel a, CandidateProgressionLevel b)
        {
            return a.LevelId == b.LevelId && a.LevelVersion == b.LevelVersion && a.UnlockRuleId == b.UnlockRuleId && a.EntryKind == b.EntryKind &&
                a.UnlockKind == b.UnlockKind && a.UnlockAfterLevelId == b.UnlockAfterLevelId && CandidateApplicationReferences.SameStrings(a.RequiredFeatures, b.RequiredFeatures);
        }

        private static void RecoveryTransition(CandidateCharacterState before, CandidateCharacterState after,
            CandidateApplicationRecoveryInput input, CandidateCharacterResult actual, SaveCodecBudget budget)
        {
            var prior = CandidatePermanentSaveCodec.Find(before.RecoveryPeriods, x => x.RecoveryId == input.RecoveryId, "Recovery.Before");
            var next = CandidatePermanentSaveCodec.Find(after.RecoveryPeriods, x => x.RecoveryId == input.RecoveryId, "Recovery.After");
            Same(ReferenceEquals(actual.RecoveryPeriod, next), "Recovery.Result");
            Same(after.Level == before.Level && after.Experience == before.Experience &&
                after.RecoveryPeriods.Count == before.RecoveryPeriods.Count, "Recovery.Character");
            for (var i = 0; i < before.RecoveryPeriods.Count; i++)
                if (before.RecoveryPeriods[i].RecoveryId != input.RecoveryId)
                    Same(ReferenceEquals(before.RecoveryPeriods[i], after.RecoveryPeriods[i]), "Recovery.OtherPeriod");
            if (actual.Outcome == CandidateGrowthOutcome.Applied)
                Same(!prior.IsCompleted && input.ExpectedCharacterRevision == before.StateRevision &&
                    after.StateRevision == budget.Math.Add(before.StateRevision, 1) && !ReferenceEquals(prior, next) &&
                    ReferenceEquals(prior.EndReceipt, next.EndReceipt) && ReferenceEquals(prior.Definition, next.Definition) &&
                    next.Elapsed.Compare(prior.Elapsed, budget.Math) >= 0, "Recovery.Applied");
            else
            {
                Same(ReferenceEquals(before, after) && ReferenceEquals(prior, next), "Recovery.UnchangedState");
                if (actual.Outcome == CandidateGrowthOutcome.AlreadyIncluded) Same(prior.IsCompleted, "Recovery.Completed");
                else Same(!prior.IsCompleted && input.ExpectedCharacterRevision == before.StateRevision, "Recovery.ExpectedRevision");
            }
        }

        private static CandidateApplicationContinuation Continue(CandidateApplicationSnapshot basis, CandidateBusinessSnapshot business,
            PreparedCandidateApplicationIntent intent, string reserved, SaveCodecBudget budget)
        {
            var old = basis?.Continuation;
            if (old != null)
            {
                var v = intent.Data.SettleVictory;
                Need(intent.Kind == CandidateApplicationKind.SettleVictory && intent.OperationId == old.ReservedOperationId &&
                    v.AttemptId == old.AttemptId && v.FinalReportFingerprint == old.FinalReportFingerprint && v.TerminalOperationId == old.ClosingOperationId,
                    "Continuation", "InvalidContinuation");
                Need(reserved == null && business.ActiveHistory == null, "Continuation", "InvalidContinuation");
                return null;
            }
            var report = business.ActiveHistory?.CurrentRun.FinalReport;
            var closes = (intent.Kind == CandidateApplicationKind.Attack || intent.Kind == CandidateApplicationKind.Link) && report != null;
            Need(closes == (reserved != null), "NextSettlementOperationId", "InvalidContinuation");
            if (!closes) return null;
            new BusinessFields(Stream.Null, false, budget).Text(reserved, "NextSettlementOperationId");
            Need(reserved != intent.OperationId, "NextSettlementOperationId", "InvalidContinuation");
            if (basis != null) foreach (var row in basis.Records)
                Need(reserved != row.OperationId, "NextSettlementOperationId", "InvalidContinuation");
            return new CandidateApplicationContinuation(intent.OperationId, report.AttemptId, report.Fingerprint, reserved);
        }

        internal static void CheckContinuation(CandidateBusinessSnapshot business, IReadOnlyList<CandidateApplicationRecord> records,
            CandidateApplicationContinuation continuation)
        {
            var run = business.ActiveHistory?.CurrentRun;
            var pending = run != null && run.CurrentSnapshot.Phase == BattlePhase.WonPendingSettlement;
            Need(pending == (continuation != null), "Continuation", "InvalidContinuation");
            if (!pending) return;
            var report = run.FinalReport;
            var last = records[records.Count - 1];
            Need(report != null && (last.Intent.Kind == CandidateApplicationKind.Attack || last.Intent.Kind == CandidateApplicationKind.Link) &&
                continuation.ClosingOperationId == last.OperationId && report.TerminalOperationId == last.OperationId &&
                continuation.AttemptId == report.AttemptId && continuation.FinalReportFingerprint == report.Fingerprint &&
                !string.IsNullOrWhiteSpace(continuation.ReservedOperationId), "Continuation.Report", "InvalidContinuation");
            foreach (var row in records) Need(row.OperationId != continuation.ReservedOperationId, "Continuation.ReservedOperationId", "InvalidContinuation");
        }
    }
}
