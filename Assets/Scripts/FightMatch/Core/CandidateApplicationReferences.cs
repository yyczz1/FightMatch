using System;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;
using static FightMatch.Core.BusinessFields;

namespace FightMatch.Core
{
    // Indexes only the closed M03-M07 result types needed by this protocol.
    internal sealed class CandidateApplicationReferences
    {
        private readonly CandidateBusinessSnapshot business;
        private readonly SaveCodecBudget budget;
        internal readonly Dictionary<string, CandidateProgressionBeginReceipt> Begins = new Dictionary<string, CandidateProgressionBeginReceipt>(StringComparer.Ordinal);
        internal readonly Dictionary<string, CandidateProgressionEndReceipt> Ends = new Dictionary<string, CandidateProgressionEndReceipt>(StringComparer.Ordinal);
        internal readonly Dictionary<string, CandidateBattleOperationRecord> Actions = new Dictionary<string, CandidateBattleOperationRecord>(StringComparer.Ordinal);
        internal readonly Dictionary<string, CandidateRollbackRecord> Rollbacks = new Dictionary<string, CandidateRollbackRecord>(StringComparer.Ordinal);
        private readonly Dictionary<string, CandidateCarryPlan> carries = new Dictionary<string, CandidateCarryPlan>(StringComparer.Ordinal);
        private readonly Dictionary<(string, string), CandidateCharacterEndReceipt> characterEnds = new Dictionary<(string, string), CandidateCharacterEndReceipt>();
        private readonly Dictionary<(string, string), CandidateBaseExperienceReceipt> experience = new Dictionary<(string, string), CandidateBaseExperienceReceipt>();
        private readonly Dictionary<string, CandidateInventoryEndReceipt> inventoryEnds = new Dictionary<string, CandidateInventoryEndReceipt>(StringComparer.Ordinal);
        private readonly Dictionary<string, CandidateFixedBaseReward> rewards = new Dictionary<string, CandidateFixedBaseReward>(StringComparer.Ordinal);
        private readonly Dictionary<string, CandidateRandomBinding> bindings = new Dictionary<string, CandidateRandomBinding>(StringComparer.Ordinal);
        private readonly Dictionary<(string, BigInteger), CandidateBattleRun> runs = new Dictionary<(string, BigInteger), CandidateBattleRun>();
        private readonly Dictionary<(string, BigInteger), BattleSnapshot> snapshots = new Dictionary<(string, BigInteger), BattleSnapshot>();
        private readonly List<CandidateHistoryEntry> anchors = new List<CandidateHistoryEntry>();

        internal CandidateApplicationReferences(CandidateBusinessSnapshot business, SaveCodecBudget budget)
        {
            this.business = business;
            this.budget = budget;
            foreach (var c in business.Progression.Challenges) foreach (var a in c.Attempts)
            {
                Add(Begins, a.Begin.AttemptId, a.Begin, "M05.Begin");
                if (a.End != null) Add(Ends, a.End.EndReceiptId, a.End, "M05.End");
            }
            foreach (var character in business.Roster.Characters)
            {
                foreach (var e in character.ProcessedEnds) Add(characterEnds, (character.CharacterId, e.EndReceiptId), e, "M03.End");
                foreach (var e in character.BaseRewards) Add(experience, (character.CharacterId, e.SettlementId), e, "M03.Experience");
            }
            foreach (var e in business.Inventory.Ends)
            {
                Add(inventoryEnds, e.EndReceiptId, e, "M04.End");
                Add(carries, e.OriginalCarry.AttemptId, e.OriginalCarry, "M04.Carry");
            }
            if (business.Inventory.ActiveCarry != null)
                Add(carries, business.Inventory.ActiveCarry.AttemptId, business.Inventory.ActiveCarry, "M04.Carry");
            if (business.ActiveHistory != null)
            {
                Run(business.ActiveHistory.CurrentRun);
                anchors.AddRange(business.ActiveHistory.Archive);
                foreach (var r in business.ActiveHistory.RollbackRecords) Rollback(r);
            }
            foreach (var r in business.RetainedRuns) Run(r);
            foreach (var r in business.RetainedRollbacks) Rollback(r);
            foreach (var r in business.Rewards.BaseRewards)
            {
                Add(rewards, r.SettlementId, r, "M07.Reward");
                Binding(r.Report.Binding);
                Snapshot(r.Report.FinalSnapshot);
                foreach (var op in r.Report.Operations) Action(op);
            }
        }

        private void Add<K, T>(Dictionary<K, T> rows, K key, T value, string path) where T : class
        {
            Need(!ReferenceEquals(key, null) && value != null, path, "IncompleteOperationHistory");
            if (rows.TryGetValue(key, out var old))
            {
                Need(ReferenceEquals(old, value), path, "ReceiptConflict");
                return;
            }
            SaveCodecFailure.Limit((ulong)rows.Count + 1, (ulong)budget.MaxCollectionEntries, path, "CollectionEntries");
            rows.Add(key, value);
        }

        private void Binding(CandidateRandomBinding b)
        {
            Add(bindings, b.Start.Baseline.Entry.AttemptId, b, "M06.Binding");
            Snapshot(b.Start.Snapshot);
        }

        private void Snapshot(BattleSnapshot s)
        {
            var key = (s.Baseline.Entry.AttemptId, s.SceneRevision);
            if (snapshots.TryGetValue(key, out var old)) Need(ReferenceEquals(old, s), "M06.Snapshot", "ReceiptConflict");
            else
            {
                SaveCodecFailure.Limit((ulong)snapshots.Count + 1, (ulong)budget.MaxCollectionEntries, "M06.Snapshots", "CollectionEntries");
                snapshots.Add(key, s);
            }
        }

        private void Action(CandidateBattleOperationRecord op)
        {
            Add(Actions, op.OperationId, op, "M06.Operation");
            Snapshot(op.BeforeSnapshot);
            Snapshot(op.AfterSnapshot);
        }

        private void Run(CandidateBattleRun r)
        {
            var key = (r.Baseline.Entry.AttemptId, r.CurrentSnapshot.SceneRevision);
            if (runs.TryGetValue(key, out var old))
            {
                Need(ReferenceEquals(old, r), "M06.Run", "ReceiptConflict");
                return;
            }
            SaveCodecFailure.Limit((ulong)runs.Count + 1, (ulong)budget.MaxCollectionEntries, "M06.Runs", "CollectionEntries");
            runs.Add(key, r);
            Binding(r.Binding);
            Snapshot(r.CurrentSnapshot);
            foreach (var op in r.Records) Action(op);
        }

        private void Rollback(CandidateRollbackRecord r)
        {
            Add(Rollbacks, r.OperationId, r, "M06.Rollback");
            Run(r.BeforeRun);
            Run(r.RestoredRun);
            foreach (var entry in r.Range.Entries)
            {
                Action(entry.Record);
                anchors.Add(entry);
            }
        }

        private static T Get<K, T>(Dictionary<K, T> rows, K key, string path)
        {
            Need(!ReferenceEquals(key, null) && rows.ContainsKey(key), path, "IncompleteOperationHistory");
            return rows[key];
        }

        internal static bool SameContext(PreparedRuleContext a, PreparedRuleContext b)
        { return RuleContextChecks.Same(a, b); }

        internal static bool SameStrings(IReadOnlyList<string> a, IReadOnlyList<string> b)
        {
            if (a.Count != b.Count) return false;
            for (var i = 0; i < a.Count; i++) if (a[i] != b[i]) return false;
            return true;
        }

        private static void Context(PreparedCandidateApplicationIntent intent, PreparedRuleContext context)
        { Need(SameContext(intent.Context, context), "Intent.Context", "InconsistentBinding"); }

        private void Entry(CandidateApplicationResolution result, string attempt, PreparedCandidateApplicationIntent intent)
        {
            result.Begin = Get(Begins, attempt, "Result.Begin");
            result.Carry = Get(carries, attempt, "Result.Carry");
            result.Binding = Get(bindings, attempt, "Result.Binding");
            Context(intent, result.Begin.Context);
            Need(result.Begin.PlayerId == intent.PlayerId && result.Carry.PlayerId == intent.PlayerId &&
                result.Begin.EntryBaselineId == result.Carry.EntryBaselineId, "Result.Entry", "InconsistentBinding");
        }

        internal CandidateApplicationResolution Resolve(CandidateApplicationRecord record)
        {
            var intent = record.Intent;
            var input = intent.Data;
            var saved = record.Result;
            var result = new CandidateApplicationResolution { Relation = CandidateApplicationRelation.Recorded };
            Need(intent.PlayerId == business.PlayerId, "Intent.PlayerId", "InconsistentBinding");
            switch (intent.Kind)
            {
                case CandidateApplicationKind.InitializeProfile:
                    var initial = intent.FormatVersion >= 3 ? input.RosterInitialize.Characters : new[] { input.InitializeProfile };
                    Need(initial.Count == business.Roster.Characters.Count, "InitializeProfile.Characters", "InconsistentBinding");
                    foreach (var init in initial)
                    {
                        var character = business.Roster.Find(init.CharacterId);
                        Need(character != null && init.ClassId == character.ClassId && init.OriginalSlot == character.OriginalSlot,
                            "InitializeProfile", "InconsistentBinding");
                        Context(intent, character.Definition.Context);
                        result.Initializations.Add(new CandidateApplicationInitializationReceipt(init));
                    }
                    Context(intent, business.Inventory.Definition.Context);
                    Context(intent, business.Progression.Definition.Context);
                    break;
                case CandidateApplicationKind.EnterAttempt:
                    Entry(result, saved.AttemptId, intent);
                    var enter = input.EnterAttempt;
                    var b = result.Begin;
                    Need(b.Participants.Count == 1, "EnterAttempt.Participants", "UnsupportedBinding");
                    Need(b.ChallengeId == saved.ChallengeId && b.EntryBaselineId == saved.EntryBaselineId &&
                        b.Level.LevelId == enter.LevelId && b.Level.LevelVersion == enter.LevelVersion &&
                        b.Participant.CharacterId == enter.CharacterId && b.Participant.CharacterRevision == enter.ExpectedCharacterRevision &&
                        b.Participant.OriginalSlot == enter.OriginalSlot, "EnterAttempt", "InconsistentBinding");
                    break;
                case CandidateApplicationKind.EnterFormation:
                    Entry(result, saved.AttemptId, intent);
                    CandidateRosterProtocol.EntryRecord(record, result.Begin, budget);
                    break;
                case CandidateApplicationKind.PermanentRequest:
                    PermanentRecord(record);
                    break;
                case CandidateApplicationKind.MigratePermanent:
                    Need(business.Format == CandidateBusinessFormat.PublishedPermanentV4 &&
                        intent.GetPermanentMigration() != null, "Permanent.Migration", "UnsupportedSchema");
                    break;
                case CandidateApplicationKind.MigrateRoster:
                case CandidateApplicationKind.SetFormation:
                    CandidateRosterProtocol.Resolve(business, record);
                    break;
                case CandidateApplicationKind.Attack:
                case CandidateApplicationKind.Link:
                    var op = Get(Actions, intent.OperationId, "Result.Operation");
                    result.BattleOperation = op;
                    result.Relation = CandidateApplicationRelation.Effective;
                    var attack = input.Attack;
                    var link = input.Link;
                    var isAttack = intent.Kind == CandidateApplicationKind.Attack;
                    var attempt = isAttack ? attack.AttemptId : link.AttemptId;
                    Entry(result, attempt, intent);
                    var source = op.Request;
                    Need(source.PlayerId == intent.PlayerId && source.AttemptId == attempt &&
                        op.Kind == (isAttack ? CandidateBattleOperationKind.Attack : CandidateBattleOperationKind.Link) &&
                        source.ExpectedSceneRevision == (isAttack ? attack.ExpectedSceneRevision : link.ExpectedSceneRevision) &&
                        source.ExpectedSceneRevision == op.BeforeSnapshot.SceneRevision, "Result.Operation.Request", "InconsistentBinding");
                    Need(Equals(source.Pair, isAttack ? attack.Pair : link.Pair), "Result.Operation.Pair", "InconsistentBinding");
                    var route = isAttack ? attack.Route : link.Route;
                    Need(source.Route.Count == route.Count, "Result.Operation.Route", "InconsistentBinding");
                    for (var i = 0; i < route.Count; i++)
                        Need(source.Route[i].Equals(route[i]), "Result.Operation.Route", "InconsistentBinding");
                    if (isAttack) Need(Equals(source.Actor, attack.Actor) && op.Conditions.PreferenceRevision == attack.ExpectedPreferenceRevision &&
                        op.Conditions.ItemUseEnabled == attack.ItemUseEnabled, "Result.Operation.Conditions", "InconsistentBinding");
                    else Need(source.Actor == null, "Result.Operation.Actor", "InconsistentBinding");
                    break;
                case CandidateApplicationKind.Rollback:
                    var rollback = Get(Rollbacks, intent.OperationId, "Result.Rollback");
                    var request = input.Rollback;
                    var range = rollback.Range;
                    Entry(result, request.AttemptId, intent);
                    Need(range.PlayerId == intent.PlayerId && range.AttemptId == request.AttemptId &&
                        range.SceneRevision == request.ExpectedSceneRevision && range.HistoryAnchorId == request.HistoryAnchorId &&
                        range.OperationId == request.TargetOperationId && range.Entries.Count == request.ConfirmedRemovedOperationIds.Count,
                        "Result.Rollback.Range", "InconsistentBinding");
                    for (var i = 0; i < range.Entries.Count; i++)
                        Need(range.Entries[i].OperationId == request.ConfirmedRemovedOperationIds[i], "Result.Rollback.Entries", "InconsistentBinding");
                    result.Rollback = rollback;
                    result.Relation = CandidateApplicationRelation.RollbackRecorded;
                    break;
                case CandidateApplicationKind.SettleVictory:
                case CandidateApplicationKind.ExitAttempt:
                case CandidateApplicationKind.RestartAttempt:
                    Ending(record, result);
                    break;
                case CandidateApplicationKind.AdvanceRecovery:
                    Recovery(record);
                    break;
            }
            return result;
        }

        private void PermanentRecord(CandidateApplicationRecord record)
        {
            Need(business.Format == CandidateBusinessFormat.PublishedPermanentV4, "Permanent.Format", "UnsupportedSchema");
            var q = record.Intent.GetPermanent();
            var actual = record.Result.GetPermanent();
            var expected = CandidatePermanentProtocol.Result(business, record.Intent);
            Need(actual != null && actual.Outcome == expected.Outcome &&
                actual.CharacterEffectOperation == expected.CharacterEffectOperation &&
                actual.InventoryEffectOperation == expected.InventoryEffectOperation &&
                actual.ProgressionEffectOperation == expected.ProgressionEffectOperation &&
                actual.OriginalLearningOperation == expected.OriginalLearningOperation, "Permanent.Result", "ReceiptConflict");
            var learned = q.Kind == CandidatePermanentKind.LearnSkill && q.OriginalLearningOperation != null;
            Need((q.Kind == CandidatePermanentKind.UseExperienceCards || q.Kind == CandidatePermanentKind.LearnSkill && !learned) ==
                (actual.CharacterEffectOperation != null), "Permanent.M03.Coverage", "IncompleteOperationHistory");
            Need(q.Costs.Count == 0 && q.Outputs.Count == 0 || actual.InventoryEffectOperation != null, "Permanent.M04.Coverage", "IncompleteOperationHistory");
            if (learned)
            {
                var prior = CandidatePermanentGrowth.FindLearning(business.Roster, q.CharacterId, q.DefinitionId);
                Need(prior != null && prior.OperationId == q.OriginalLearningOperation && q.Costs.Count == 0 &&
                    q.Inputs.Count == 0 && q.Outputs.Count == 0 && actual.InventoryEffectOperation == null &&
                    (q.TeachingLevel != null || actual.ProgressionEffectOperation == null) &&
                    prior.OperationId != record.OperationId, "Permanent.OriginalLearning", "ReceiptConflict");
            }
            foreach (var effects in new[] { business.Roster.GetPermanentEffects(), business.Inventory.GetPermanentLedger().Effects,
                business.Progression.GetPermanentEffects() })
            {
                var effect = CandidatePermanentProtocol.Find(effects, record.OperationId);
                if (effect == null) continue;
                Need(effect.Outcome == actual.Outcome && CandidatePermanentCodec.SameQuote(q, effect.Quote, budget) &&
                    effect.RelatedOperationId == q.OriginalLearningOperation, "Permanent.OwnerReference", "ReceiptConflict");
            }
            if (q.Kind == CandidatePermanentKind.BeginTeachingGift || q.Kind == CandidatePermanentKind.ConfirmTeachingExplanation ||
                q.Kind == CandidatePermanentKind.LearnSkill && q.TeachingLevel != null)
                Need(CandidatePermanentProgression.Find(business.Progression, q.Kind, q.DefinitionId) != null,
                    "Permanent.M05.Coverage", "IncompleteOperationHistory");
        }

        private void Ending(CandidateApplicationRecord record, CandidateApplicationResolution result)
        {
            var intent = record.Intent;
            var win = intent.Kind == CandidateApplicationKind.SettleVictory;
            var restart = intent.Kind == CandidateApplicationKind.RestartAttempt;
            var endInput = restart ? intent.Data.RestartAttempt : intent.Data.ExitAttempt;
            var victory = intent.Data.SettleVictory;
            var attempt = win ? victory.AttemptId : endInput.AttemptId;
            Entry(result, attempt, intent);
            var end = Get(Ends, record.Result.EndReceiptId, "Result.End");
            Need(ReferenceEquals(end.Begin, result.Begin) && end.Kind == (win ? CandidateProgressionEndKind.NormalVictory :
                restart ? CandidateProgressionEndKind.ImmediateRestart : CandidateProgressionEndKind.NormalExit), "Result.End.Kind", "InconsistentBinding");
            Need(end.Begin.ChallengeId == (win ? victory.ChallengeId : endInput.ChallengeId) &&
                end.Begin.EntryBaselineId == (win ? victory.EntryBaselineId : endInput.EntryBaselineId), "Result.End.Begin", "InconsistentBinding");
            result.End = end;
            foreach (var participant in result.Begin.Participants)
                result.CharacterEnds.Add(Get(characterEnds, (participant.CharacterId, end.EndReceiptId), "Result.CharacterEnd"));
            result.InventoryEnd = Get(inventoryEnds, end.EndReceiptId, "Result.InventoryEnd");
            if (win)
            {
                var reward = Get(rewards, record.Result.SettlementId, "Result.Reward");
                Need(ReferenceEquals(reward.Ending, end) && reward.FinalReportFingerprint == victory.FinalReportFingerprint &&
                    reward.Report.TerminalOperationId == victory.TerminalOperationId && reward.Definition.RewardDefinitionId == victory.RewardDefinitionId &&
                    reward.Definition.Version == victory.RewardDefinitionVersion, "Result.Reward", "InconsistentBinding");
                Context(intent, reward.Definition.Context);
                result.Reward = reward;
                foreach (var participant in result.Begin.Participants)
                    result.CharacterExperiences.Add(Get(experience, (participant.CharacterId, reward.SettlementId), "Result.CharacterExperience"));
            }
            else
            {
                Need(runs.TryGetValue((attempt, endInput.ExpectedSceneRevision.Value), out var terminal), "Result.TerminalRun", "IncompleteOperationHistory");
                result.TerminalRun = terminal;
                Need(terminal.FinalReport == null, "Result.TerminalRun", "InconsistentBinding");
                foreach (var characterEnd in result.CharacterEnds)
                {
                    var member = CandidatePermanentSaveCodec.Find(terminal.CurrentSnapshot.Members,
                        x => x.Member.CharacterId == characterEnd.CharacterId, "Result.TerminalRun.Member");
                    Need(characterEnd.WasDown == member.Hp.Numerator.IsZero, "Result.TerminalRun", "InconsistentBinding");
                }
            }
            if (restart)
            {
                Need(end.NewAttemptId == record.Result.NewAttemptId, "Result.NewAttemptId", "InconsistentBinding");
                result.NewBegin = Get(Begins, end.NewAttemptId, "Result.NewBegin");
                result.NewCarry = Get(carries, end.NewAttemptId, "Result.NewCarry");
                result.NewBinding = Get(bindings, end.NewAttemptId, "Result.NewBinding");
            }
        }

        internal static CandidateRecoveryPeriod Period(CandidateBusinessSnapshot business, string id, string characterId)
        {
            var character = business.Roster.Find(characterId);
            Need(character != null, "Recovery.CharacterId", "InconsistentBinding");
            CandidateRecoveryPeriod found = null;
            foreach (var period in character.RecoveryPeriods) if (period.RecoveryId == id)
            {
                Need(found == null, "Recovery.Period", "ReceiptConflict");
                found = period;
            }
            Need(found != null, "Recovery.Period", "IncompleteOperationHistory");
            return found;
        }

        private void Recovery(CandidateApplicationRecord record)
        {
            var input = record.Intent.Data.AdvanceRecovery;
            var receipt = record.Result.Recovery;
            Need(receipt != null, "Result.Recovery", "MissingField");
            Recovery(record.Intent, input, receipt);
        }
        private void Recovery(PreparedCandidateApplicationIntent intent, CandidateApplicationRecoveryInput input, CandidateApplicationRecoveryReceipt receipt)
        {
            var current = Period(business, input.RecoveryId, input.CharacterId);
            var historical = receipt.Period;
            Need(receipt.CharacterId == input.CharacterId && historical.RecoveryId == input.RecoveryId &&
                ReferenceEquals(historical.EndReceipt, current.EndReceipt) && ReferenceEquals(historical.Definition, current.Definition),
                "Result.Recovery.Identity", "InconsistentBinding");
            Context(intent, historical.Definition.Context);
            Need(receipt.AfterCharacterRevision > 0 && receipt.AfterCharacterRevision <= business.Roster.Find(input.CharacterId).StateRevision &&
                historical.Elapsed.Numerator.Sign >= 0 && historical.Elapsed.Compare(current.Elapsed, budget.Math) <= 0 &&
                current.Elapsed.Compare(current.Duration, budget.Math) <= 0, "Result.Recovery.History", "InconsistentBinding");
            Need((int)receipt.Outcome >= 1 && (int)receipt.Outcome <= 4 && (int)historical.Anomaly >= 0 && (int)historical.Anomaly <= 3 &&
                (int)receipt.ResultAnomaly >= 0 && (int)receipt.ResultAnomaly <= 3, "Result.Recovery.Outcome", "InvalidValue");
            if (receipt.Outcome == CandidateGrowthOutcome.Applied)
            {
                Need(receipt.AfterCharacterRevision == budget.Math.Add(input.ExpectedCharacterRevision.Value, 1) &&
                    CandidateRecoveryClock.SameTime(historical.LastAcceptedSample,
                        CandidateApplicationIntentCodec.PrepareTime(input.TimeSample, budget, "Intent.TimeSample"), new GrowthChecks(budget.Math)) &&
                    receipt.ResultAnomaly == historical.Anomaly, "Result.Recovery.Applied", "InconsistentBinding");
            }
            else if (receipt.Outcome == CandidateGrowthOutcome.AlreadyIncluded)
                Need(historical.IsCompleted && receipt.ResultAnomaly == CandidateTimeAnomaly.None, "Result.Recovery.AlreadyIncluded", "InconsistentBinding");
            else
            {
                Need(!historical.IsCompleted && receipt.AfterCharacterRevision == input.ExpectedCharacterRevision, "Result.Recovery.Revision", "InconsistentBinding");
                if (receipt.Outcome == CandidateGrowthOutcome.Unchanged)
                    Need(receipt.ResultAnomaly == historical.Anomaly && CandidateRecoveryClock.SameTime(historical.LastAcceptedSample,
                        CandidateApplicationIntentCodec.PrepareTime(input.TimeSample, budget, "Intent.TimeSample"), new GrowthChecks(budget.Math)),
                        "Result.Recovery.Unchanged", "InconsistentBinding");
                else
                {
                    var time = CandidateApplicationIntentCodec.PrepareTime(input.TimeSample, budget, "Intent.TimeSample");
                    var last = historical.LastAcceptedSample;
                    var sameDomain = last.MonotonicScopeId != null && string.Equals(last.MonotonicScopeId, time.MonotonicScopeId, StringComparison.Ordinal);
                    var comparison = sameDomain ? time.MonotonicElapsedMilliseconds.Compare(last.MonotonicElapsedMilliseconds, budget.Math)
                        : budget.Math.Compare(time.WallUtcMilliseconds, last.WallUtcMilliseconds);
                    var anomaly = historical.Anomaly | time.Anomaly | CandidateTimeAnomaly.ClockBackward |
                        (sameDomain ? CandidateTimeAnomaly.None : CandidateTimeAnomaly.DomainChanged);
                    Need(comparison < 0 && receipt.ResultAnomaly == anomaly, "Result.Recovery.Ignored", "InconsistentBinding");
                }
            }
        }

        internal Dictionary<string, CandidateApplicationResolution> Validate(IReadOnlyList<CandidateApplicationRecord> records)
        {
            SaveEnvelopeCodec.CheckList(records, budget, "Records");
            Need(records.Count > 0, "Records", "MissingApplicationRecords");
            var resolved = new Dictionary<string, CandidateApplicationResolution>(StringComparer.Ordinal);
            var beginIds = new HashSet<string>(StringComparer.Ordinal);
            var endIds = new HashSet<string>(StringComparer.Ordinal);
            var actionIds = new HashSet<string>(StringComparer.Ordinal);
            var rollbackIds = new HashSet<string>(StringComparer.Ordinal);
            var rewardIds = new HashSet<string>(StringComparer.Ordinal);
            var anchorIds = new HashSet<(string, string)>();
            var byId = new Dictionary<string, CandidateApplicationRecord>(StringComparer.Ordinal);
            BattleSnapshot current = null;
            for (var i = 0; i < records.Count; i++)
            {
                var row = records[i];
                Need((i == 0) == (row.Intent.Kind == CandidateApplicationKind.InitializeProfile), "Records.InitializeProfile", "IncompleteOperationHistory");
                Need(!resolved.ContainsKey(row.OperationId), "Records.OperationId", "OperationConflict");
                Need(row.Intent.ExpectedCommitId == (i == 0 ? null : records[i - 1].CommitId), "Records.ExpectedCommitId", "StaleContext");
                var value = Resolve(row);
                var kind = row.Intent.Kind;
                if (kind == CandidateApplicationKind.EnterAttempt || kind == CandidateApplicationKind.EnterFormation)
                {
                    Need(current == null && beginIds.Add(value.Begin.AttemptId), "Records.EnterAttempt", "IncompleteOperationHistory");
                    current = value.Binding.Start.Snapshot;
                }
                if (value.BattleOperation != null)
                {
                    var action = value.BattleOperation;
                    Need(ReferenceEquals(current, action.BeforeSnapshot) && actionIds.Add(row.OperationId), "Records.Operation.Before", "IncompleteOperationHistory");
                    Need(anchorIds.Add((action.Request.AttemptId, row.Result.HistoryAnchorId)), "Records.HistoryAnchorId", "ReceiptConflict");
                    current = action.AfterSnapshot;
                }
                if (value.Rollback != null)
                {
                    var rollback = value.Rollback;
                    Need(ReferenceEquals(current, rollback.BeforeRun.CurrentSnapshot) && rollbackIds.Add(row.OperationId), "Records.Rollback.Before", "IncompleteOperationHistory");
                    foreach (var removed in rollback.Range.Entries)
                    {
                        Need(resolved.TryGetValue(removed.OperationId, out var earlier) && earlier.BattleOperation != null &&
                            ReferenceEquals(earlier.BattleOperation, removed.Record), "Records.Rollback.Entries", "IncompleteOperationHistory");
                        if (earlier.SupersededBy == null)
                        {
                            earlier.Relation = CandidateApplicationRelation.Superseded;
                            earlier.SupersededBy = new CandidateHistorySupersededBy(row.OperationId, rollback.SceneRevision);
                        }
                    }
                    current = rollback.RestoredRun.CurrentSnapshot;
                }
                if (value.End != null)
                {
                    Need(current != null && current.Baseline.Entry.AttemptId == value.Begin.AttemptId && endIds.Add(value.End.EndReceiptId),
                        "Records.End", "IncompleteOperationHistory");
                    Need(ReferenceEquals(current, value.Reward == null ? value.TerminalRun.CurrentSnapshot : value.Reward.Report.FinalSnapshot),
                        "Records.End.Snapshot", "IncompleteOperationHistory");
                    if (value.Reward != null) Need(rewardIds.Add(value.Reward.SettlementId), "Records.Reward", "ReceiptConflict");
                    current = null;
                    if (value.NewBegin != null)
                    {
                        Need(beginIds.Add(value.NewBegin.AttemptId), "Records.NewBegin", "ReceiptConflict");
                        current = Get(bindings, value.NewBegin.AttemptId, "Records.NewBinding").Start.Snapshot;
                    }
                }
                if (kind == CandidateApplicationKind.MigrateRoster || kind == CandidateApplicationKind.SetFormation)
                    Need(current == null, "Records.Roster.ActiveAttempt", "ActiveAttemptConflict");
                if (kind == CandidateApplicationKind.EnterFormation)
                    foreach (var recovery in row.Result.RecoveryResults)
                        Recovery(row.Intent, CandidateRosterProtocol.RecoveryInput(row.Intent.Data.EnterFormation, recovery), recovery);
                resolved.Add(row.OperationId, value);
                byId.Add(row.OperationId, row);
            }
            Need(beginIds.Count == Begins.Count && endIds.Count == Ends.Count && actionIds.Count == Actions.Count &&
                rollbackIds.Count == Rollbacks.Count && rewardIds.Count == rewards.Count, "Records.Coverage", "IncompleteOperationHistory");
            Need(ReferenceEquals(current, business.ActiveHistory?.CurrentRun.CurrentSnapshot), "Records.CurrentSnapshot", "IncompleteOperationHistory");
            foreach (var entry in anchors)
                Need(byId.TryGetValue(entry.OperationId, out var row) && row.Result.HistoryAnchorId == entry.HistoryAnchorId,
                    "Records.Anchor", "InconsistentBinding");
            if (business.ActiveHistory != null) foreach (var entry in business.ActiveHistory.Archive)
            {
                var actual = resolved[entry.OperationId].SupersededBy;
                Need((actual == null) == (entry.SupersededBy == null) && (actual == null ||
                    actual.OperationId == entry.SupersededBy.OperationId && actual.SceneRevision == entry.SupersededBy.SceneRevision),
                    "Records.SupersededBy", "InconsistentBinding");
            }
            if (business.Format == CandidateBusinessFormat.PublishedPermanentV4)
            {
                foreach (var effects in new[] { business.Roster.GetPermanentEffects(), business.Inventory.GetPermanentLedger().Effects, business.Progression.GetPermanentEffects() })
                    foreach (var effect in effects) Need(byId.TryGetValue(effect.OperationId, out var owner) &&
                        owner.Intent.Kind == CandidateApplicationKind.PermanentRequest, "Permanent.Records.Coverage", "IncompleteOperationHistory");
                // This version has no authenticated advertisement ingress or owner index resolver.
                Need(business.Inventory.GetPermanentLedger().Sources.Count == 0, "Permanent.HeldSourceIndex", "UnsupportedSourceProof");
                CandidatePermanentInventory.Held(business, records, budget);
            }
            CandidateRosterProtocol.History(business, records, budget);
            OwnerHistory(records, resolved);
            return resolved;
        }

        private bool SamePeriod(CandidateRecoveryPeriod a, CandidateRecoveryPeriod b)
        {
            return ReferenceEquals(a.EndReceipt, b.EndReceipt) && ReferenceEquals(a.Definition, b.Definition) && a.Anomaly == b.Anomaly &&
                a.Elapsed.Compare(b.Elapsed, budget.Math) == 0 && CandidateRecoveryClock.SameTime(a.LastAcceptedSample, b.LastAcceptedSample, new GrowthChecks(budget.Math));
        }

        // Count only declared state transitions and compare stored recovery values. This
        // does not execute growth, recompute a clock delta, or advance a recovery period.
        private void OwnerHistory(IReadOnlyList<CandidateApplicationRecord> records, Dictionary<string, CandidateApplicationResolution> resolved)
        {
            var characterRevisions = new Dictionary<string, BigInteger>(StringComparer.Ordinal);
            foreach (var character in business.Roster.Characters) characterRevisions.Add(character.CharacterId, BigInteger.One);
            var inventoryRevision = BigInteger.One;
            var progressionRevision = BigInteger.One;
            var rewardRevision = BigInteger.One;
            var preferenceRevision = BigInteger.One;
            var permanent = business.Format == CandidateBusinessFormat.PublishedPermanentV4;
            var loadouts = business.Inventory.Loadouts.ToDictionary(x => x.Actor.CharacterId,
                x => new CandidateInventoryLoadout(x.Actor, null, BigInteger.Zero, null));
            var initial = records[0].Intent;
            var recipes = initial.FormatVersion >= 3 ? initial.Data.RosterInitialize.Characters : new[] { initial.Data.InitializeProfile };
            var levels = recipes.ToDictionary(x => x.CharacterId, x => (x.InitialLevel.Value, x.InitialExperience.Value));
            var periods = new Dictionary<(string, string), CandidateRecoveryPeriod>();
            var priorOperations = new HashSet<string>(StringComparer.Ordinal);
            var permanentFacts = new HashSet<(CandidatePermanentKind, string, string)>();
            foreach (var row in records)
            {
                var result = resolved[row.OperationId];
                if (row.Intent.Kind == CandidateApplicationKind.PermanentRequest)
                {
                    var q = row.Intent.GetPermanent();
                    var receipt = row.Result.GetPermanent();
                    foreach (var input in q.Inputs)
                        Need(priorOperations.Contains(input.Source?.OriginalOperationId ?? input.OutputOperationId),
                            "Permanent.History.Input", "IncompleteOperationHistory");
                    if (q.OriginalLearningOperation != null)
                    {
                        Need(priorOperations.Contains(q.OriginalLearningOperation), "Permanent.History.Learning", "IncompleteOperationHistory");
                        Need(business.Roster.GetPermanentEffects().Any(x => x.OperationId == q.OriginalLearningOperation &&
                            x.Quote.Kind == CandidatePermanentKind.LearnSkill && (q.Kind != CandidatePermanentKind.LearnSkill ||
                            x.Quote.CharacterId == q.CharacterId && x.Quote.DefinitionId == q.DefinitionId)),
                            "Permanent.History.LearningIdentity", "IncompleteOperationHistory");
                    }
                    if (q.Kind == CandidatePermanentKind.ConfirmTeachingExplanation)
                        Need(business.Progression.GetPermanentEffects().Any(x => x.Quote.Kind == CandidatePermanentKind.LearnSkill &&
                            x.Quote.TeachingLevel.Same(q.TeachingLevel) && priorOperations.Contains(x.OperationId) &&
                            (x.RelatedOperationId ?? x.OperationId) == q.OriginalLearningOperation),
                            "Permanent.History.LearningStep", "IncompleteOperationHistory");
                    if (q.TeachingLevel != null)
                    {
                        var key = (q.Kind, q.DefinitionId, q.CharacterId);
                        var had = permanentFacts.Contains(key);
                        Need((receipt.ProgressionEffectOperation != null) != had, "Permanent.History.Teaching");
                        if (!had)
                        {
                            permanentFacts.Add(key);
                            if (q.Kind == CandidatePermanentKind.BeginTeachingGift)
                                Need(q.Outputs.Count == 1 && receipt.InventoryEffectOperation != null, "Permanent.History.Gift");
                            else
                                Need(business.Progression.GetPermanentEffects().Any(x =>
                                    x.Quote.Kind == CandidatePermanentKind.BeginTeachingGift && x.Quote.TeachingLevel.Same(q.TeachingLevel) &&
                                    priorOperations.Contains(x.OperationId)), "Permanent.History.Gift", "IncompleteOperationHistory");
                        }
                    }
                    Need(q.InventoryRevision == inventoryRevision && q.PreferenceRevision == preferenceRevision &&
                        q.ProgressionRevision == progressionRevision, "Permanent.History.Owners", "StaleContext");
                    if (q.CharacterId != null)
                        Need(q.CharacterRevision == characterRevisions[q.CharacterId] && q.BeforeLevel == levels[q.CharacterId].Item1 &&
                            q.BeforeExperience == levels[q.CharacterId].Item2, "Permanent.History.Character", "StaleContext");
                    if (receipt.CharacterEffectOperation != null)
                    {
                        characterRevisions[q.CharacterId] = budget.Math.Add(characterRevisions[q.CharacterId], 1);
                        levels[q.CharacterId] = (q.FinalLevel, q.FinalExperience);
                    }
                    if (receipt.InventoryEffectOperation != null) inventoryRevision = budget.Math.Add(inventoryRevision, 1);
                    if (receipt.ProgressionEffectOperation != null) progressionRevision = budget.Math.Add(progressionRevision, 1);
                    if (q.Kind == CandidatePermanentKind.Equip || q.Kind == CandidatePermanentKind.SetPreference)
                    {
                        var old = loadouts[q.CharacterId];
                        var changed = q.Kind == CandidatePermanentKind.Equip
                            ? old.ItemId != q.DefinitionId || old.L != q.Quantity : old.Enabled != q.Enabled;
                        Need(changed == (receipt.Outcome == "Applied"), "Permanent.History.Loadout");
                        if (changed && (q.Kind == CandidatePermanentKind.SetPreference || old.ItemId != q.DefinitionId))
                            preferenceRevision = budget.Math.Add(preferenceRevision, 1);
                        loadouts[q.CharacterId] = q.Kind == CandidatePermanentKind.Equip
                            ? new CandidateInventoryLoadout(old.Actor, q.DefinitionId, q.Quantity.Value,
                                q.DefinitionId == null ? (bool?)null : old.ItemId == q.DefinitionId ? old.Enabled : true)
                            : new CandidateInventoryLoadout(old.Actor, old.ItemId, old.L, q.Enabled);
                    }
                }
                if (row.Intent.Kind == CandidateApplicationKind.EnterFormation)
                {
                    var entry = row.Intent.Data.EnterFormation;
                    Need(entry.ExpectedInventoryRevision == inventoryRevision && entry.ExpectedProgressionRevision == progressionRevision,
                        "Records.EnterFormation.OwnerRevisions", "InconsistentBinding");
                    var expectedRecoveries = 0;
                    foreach (var character in entry.SelectedCharacters)
                    {
                        Need(characterRevisions.TryGetValue(character.CharacterId, out var revision) && character.ExpectedRevision == revision,
                            "Records.EnterFormation.CharacterRevision", "InconsistentBinding");
                        foreach (var pair in periods) if (pair.Key.Item1 == character.CharacterId && !pair.Value.IsCompleted)
                        {
                            Need(row.Result.RecoveryResults.Any(x => x.CharacterId == character.CharacterId && x.RecoveryId == pair.Key.Item2),
                                "Records.EnterFormation.RecoveryCoverage", "IncompleteOperationHistory");
                            expectedRecoveries++;
                        }
                    }
                    Need(expectedRecoveries == row.Result.RecoveryResults.Count, "Records.EnterFormation.RecoveryCoverage", "IncompleteOperationHistory");
                }
                foreach (var receipt in row.Result.RecoveryResults ?? Array.Empty<CandidateApplicationRecoveryReceipt>())
                {
                    var input = row.Intent.Kind == CandidateApplicationKind.EnterFormation
                        ? CandidateRosterProtocol.RecoveryInput(row.Intent.Data.EnterFormation, receipt) : row.Intent.Data.AdvanceRecovery;
                    var key = (input.CharacterId, input.RecoveryId); var characterRevision = characterRevisions[input.CharacterId];
                    Need(periods.TryGetValue(key, out var prior), "Records.Recovery.Before", "IncompleteOperationHistory");
                    if (receipt.Outcome == CandidateGrowthOutcome.Applied)
                    {
                        Need(!prior.IsCompleted && input.ExpectedCharacterRevision == characterRevision &&
                            receipt.Period.Elapsed.Compare(prior.Elapsed, budget.Math) >= 0, "Records.Recovery.Applied", "InconsistentBinding");
                        characterRevision = budget.Math.Add(characterRevision, 1); periods[key] = receipt.Period;
                    }
                    else
                    {
                        Need(SamePeriod(prior, receipt.Period), "Records.Recovery.UnchangedPeriod", "InconsistentBinding");
                        Need(receipt.Outcome == CandidateGrowthOutcome.AlreadyIncluded ? prior.IsCompleted :
                            !prior.IsCompleted && input.ExpectedCharacterRevision == characterRevision, "Records.Recovery.BeforeRevision", "InconsistentBinding");
                    }
                    Need(receipt.AfterCharacterRevision == characterRevision, "Records.Recovery.AfterRevision", "InconsistentBinding");
                    characterRevisions[input.CharacterId] = characterRevision;
                }
                if (row.Intent.Kind == CandidateApplicationKind.EnterAttempt || row.Intent.Kind == CandidateApplicationKind.EnterFormation)
                {
                    foreach (var participant in result.Begin.Participants)
                        Need(participant.CharacterRevision == characterRevisions[participant.CharacterId] &&
                            !periods.Any(x => x.Key.Item1 == participant.CharacterId && !x.Value.IsCompleted),
                            "Records.EnterAttempt.CharacterRevision", "InconsistentBinding");
                    if (row.Intent.Kind == CandidateApplicationKind.EnterFormation)
                        foreach (var selected in row.Intent.Data.EnterFormation.SelectedCharacters)
                            Need(result.Begin.Participants.Any(x => x.CharacterId == selected.CharacterId) ==
                                !periods.Any(x => x.Key.Item1 == selected.CharacterId && !x.Value.IsCompleted),
                                "Records.EnterFormation.ReadyCoverage", "IncompleteOperationHistory");
                    inventoryRevision = budget.Math.Add(inventoryRevision, 1);
                    progressionRevision = budget.Math.Add(progressionRevision, 1);
                }
                if (row.Intent.Kind == CandidateApplicationKind.Attack)
                    Need(row.Intent.Data.Attack.ExpectedPreferenceRevision == (permanent ? preferenceRevision : business.Inventory.PreferenceRevision) &&
                        (!permanent || row.Intent.Data.Attack.ItemUseEnabled == (loadouts[row.Intent.Data.Attack.Actor.CharacterId].Enabled ?? false)),
                        "Records.Attack.PreferenceRevision", "InconsistentBinding");
                if (result.End != null)
                {
                    foreach (var participant in result.Begin.Participants)
                        characterRevisions[participant.CharacterId] = budget.Math.Add(characterRevisions[participant.CharacterId], result.Reward == null ? 1 : 2);
                    inventoryRevision = budget.Math.Add(inventoryRevision, 1);
                    progressionRevision = budget.Math.Add(progressionRevision, 1);
                    if (result.Reward != null) rewardRevision = budget.Math.Add(rewardRevision, 1);
                    if (permanent) foreach (var experience in result.CharacterExperiences)
                    {
                        var state = business.Roster.Find(experience.CharacterId);
                        var prior = levels[state.CharacterId];
                        var before = new CandidateCharacterState(state.Definition, state.PlayerId, state.CharacterId, prior.Item1, prior.Item2,
                            state.OriginalSlot, state.StateRevision, state.BaseRewards, state.ProcessedEnds, state.RecoveryPeriods);
                        CandidateCharacterGrowth.Accumulate(before, experience.Amount, budget.Math, out var level, out var remainder);
                        levels[state.CharacterId] = (level, remainder);
                    }
                    foreach (var end in result.CharacterEnds) if (end.RecoveryId != null)
                    {
                        var key = (end.CharacterId, end.RecoveryId); var current = Period(business, end.RecoveryId, end.CharacterId);
                        Need(!periods.ContainsKey(key), "Records.RecoveryId", "ReceiptConflict");
                        periods.Add(key, new CandidateRecoveryPeriod(end, current.Definition, end.TimeSample,
                            ExactRational.Create(0, 1, budget.Math), end.TimeSample.Anomaly, budget.Math));
                    }
                }
                priorOperations.Add(row.OperationId);
            }
            Need(inventoryRevision == business.Inventory.StateRevision && progressionRevision == business.Progression.StateRevision &&
                rewardRevision == business.Rewards.StateRevision, "Records.OwnerRevisions", "IncompleteOperationHistory");
            foreach (var loadout in business.Inventory.Loadouts)
                Need(permanent ? preferenceRevision == business.Inventory.PreferenceRevision &&
                    loadout.ItemId == loadouts[loadout.Actor.CharacterId].ItemId && loadout.L == loadouts[loadout.Actor.CharacterId].L && loadout.Enabled == loadouts[loadout.Actor.CharacterId].Enabled :
                    business.Inventory.PreferenceRevision.IsOne && loadout.ItemId == null && loadout.Enabled == null && loadout.L.IsZero,
                    "Records.Preference", "IncompleteOperationHistory");
            var periodCount = 0;
            foreach (var character in business.Roster.Characters)
            {
                Need(characterRevisions[character.CharacterId] == character.StateRevision, "Records.OwnerRevisions", "IncompleteOperationHistory");
                if (permanent) Need(character.Level == levels[character.CharacterId].Item1 && character.Experience == levels[character.CharacterId].Item2, "Permanent.History.Experience");
                periodCount += character.RecoveryPeriods.Count;
                foreach (var period in character.RecoveryPeriods)
                    Need(periods.TryGetValue((character.CharacterId, period.RecoveryId), out var last) && SamePeriod(last, period),
                        "Records.Recovery.CurrentPeriod", "IncompleteOperationHistory");
            }
            Need(periods.Count == periodCount, "Records.Recovery.Coverage", "IncompleteOperationHistory");
        }
    }
}
