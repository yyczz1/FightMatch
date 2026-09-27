using System;
using System.Collections.Generic;
using System.Numerics;
using static FightMatch.Core.CandidateBattleReportFingerprint;
using static FightMatch.Core.CandidateHistoryRejectionCode;

namespace FightMatch.Core
{
    public static class CandidateHistoryOperations
    {
        public static CandidateHistoryResult CreateCandidate(CandidateBattleRun initialRun, ExactMathBudget budget)
        {
            Root(initialRun, nameof(initialRun), budget);
            if (initialRun.Binding == null) return Reject(MissingField, "InitialRun.Binding");
            var created = CandidateBattleOperations.CreateCandidate(initialRun.Binding, budget);
            if (!created.IsAccepted)
                return Reject((CandidateHistoryRejectionCode)Enum.Parse(typeof(CandidateHistoryRejectionCode), created.RejectionCode), created.FieldPath);
            var rejection = RunValues(initialRun, initialRun.Binding, budget);
            if (rejection != null) return rejection;
            if (initialRun.Records.Count != 0 || initialRun.FinalReport != null || initialRun.CurrentSnapshot.SceneRevision != BigInteger.One ||
                !Equal(initialRun.CurrentSnapshot, initialRun.InitialSnapshot, budget)) return Reject(IncompleteHistory, "InitialRun");
            return new CandidateHistoryResult(CandidateHistoryOutcome.Created, new CandidateBattleHistory(initialRun,
                Array.Empty<CandidateHistoryEntry>(), Array.Empty<string>(), Array.Empty<CandidateRollbackRecord>()));
        }

        public static CandidateHistoryResult Append(CandidateBattleHistory history, CandidateBattleRun nextRun,
            string historyAnchorId, ExactMathBudget budget)
        {
            Root(history, nameof(history), budget);
            if (nextRun == null) throw new ArgumentNullException(nameof(nextRun));
            var rejection = History(history, budget) ?? Active(history.CurrentRun);
            if (rejection != null) return rejection;
            if (string.IsNullOrWhiteSpace(historyAnchorId)) return Reject(MissingField, "HistoryAnchorId");
            foreach (var entry in history.Archive)
                if (Same(entry.HistoryAnchorId, historyAnchorId)) return Reject(OperationConflict, "HistoryAnchorId");
            rejection = RunValues(nextRun, history.Binding, budget);
            if (rejection != null) return rejection;
            var current = history.CurrentRun;
            if (nextRun.Records.Count != current.Records.Count + 1) return Reject(IncompleteHistory, "NextRun.Records");
            for (var i = 0; i < current.Records.Count; i++)
                if (!ReferenceEquals(current.Records[i], nextRun.Records[i])) return Reject(IncompleteHistory, $"NextRun.Records[{i}]");
            var record = nextRun.Records[nextRun.Records.Count - 1];
            if (HasOperation(history, record.OperationId)) return Reject(OperationConflict, "NextRun.Record.OperationId");
            if (!ReferenceEquals(record.BeforeSnapshot, current.CurrentSnapshot) ||
                !Equal(record.BeforeSnapshot, current.CurrentSnapshot, budget)) return Reject(IncompleteHistory, "NextRun.Record.BeforeSnapshot");
            if (!ReferenceEquals(record.AfterSnapshot, nextRun.CurrentSnapshot)) return Reject(IncompleteHistory, "NextRun.CurrentSnapshot");
            rejection = RecordValues(record, history.Binding, budget) ?? Report(nextRun, budget);
            if (rejection != null) return rejection;
            var added = new CandidateHistoryEntry(historyAnchorId, record);
            var archive = new List<CandidateHistoryEntry>(history.Archive) { added };
            var anchors = new List<string>(history.EffectiveAnchors) { historyAnchorId };
            return new CandidateHistoryResult(CandidateHistoryOutcome.Appended,
                new CandidateBattleHistory(nextRun, archive, anchors, history.RollbackRecords), added);
        }

        public static CandidateHistoryResult Locate(CandidateBattleHistory history, CandidateHistoryLocator locator, ExactMathBudget budget)
        {
            Root(history, nameof(history), budget);
            if (locator == null) throw new ArgumentNullException(nameof(locator));
            var rejection = Context(history, locator.PlayerId, locator.AttemptId, locator.ExpectedSceneRevision, budget);
            if (rejection != null) return rejection;
            if (!locator.Kind.HasValue) return Reject(MissingField, "Locator.Kind");
            if (locator.Kind != CandidateHistoryLocatorKind.EndpointLatestAttack && locator.Kind != CandidateHistoryLocatorKind.LockedRouteKillingAttack)
                return Reject(UnsupportedBinding, "Locator.Kind");
            if (locator.Pair == null) return Reject(MissingField, "Locator.Pair");
            var state = history.CurrentRun.CurrentSnapshot; var validPair = false;
            foreach (var pair in state.Board.Face.Pairs)
                if (locator.Pair.Equals(BattlePairKey.Create(locator.AttemptId, state.Board.Face.FaceId, pair.PairId))) validPair = true;
            if (!validPair) return Reject(InconsistentBinding, "Locator.Pair");
            BattleLockedRoute locked = null;
            foreach (var route in state.Board.LockedRoutes) if (locator.Pair.Equals(route.PairKey)) locked = route;
            if (locator.Kind == CandidateHistoryLocatorKind.LockedRouteKillingAttack && locked == null) return Reject(NotFound, "Locator.Pair");
            for (var i = history.CurrentRun.Records.Count - 1; i >= 0; i--)
            {
                var record = history.CurrentRun.Records[i];
                if (record.Kind != CandidateBattleOperationKind.Attack || !locator.Pair.Equals(record.Request.Pair)) continue;
                if (locator.Kind == CandidateHistoryLocatorKind.LockedRouteKillingAttack)
                {
                    var killed = false; var fixedHere = false;
                    foreach (var hit in record.DirectAttack.DamageFacts)
                        if (hit.Pair.Equals(locator.Pair) && hit.DefeatedTarget && hit.HpBefore.Numerator.Sign > 0) killed = true;
                    foreach (var fact in record.StageDecision.OrderedFacts)
                        if (fact.Kind == CandidateStageFactKind.RouteLocked && locator.Pair.Equals(fact.Pair)) fixedHere = true;
                    if (!killed || !fixedHere || !Equal(locked.Route, record.Request.Route, budget)) continue;
                }
                return new CandidateHistoryResult(CandidateHistoryOutcome.RangeFound, range: Range(history, i));
            }
            return Reject(NotFound, "Locator.Pair");
        }

        public static CandidateHistoryResult ReadRange(CandidateBattleHistory history, CandidateHistoryRangeRequest request, ExactMathBudget budget)
        {
            Root(history, nameof(history), budget);
            if (request == null) throw new ArgumentNullException(nameof(request));
            var rejection = Context(history, request.PlayerId, request.AttemptId, request.ExpectedSceneRevision, budget);
            if (rejection != null) return rejection;
            return Read(history, request.HistoryAnchorId);
        }

        public static CandidateHistoryResult PrepareRollback(CandidateBattleHistory history, CandidateRollbackRequest request,
            CandidateRollbackRange confirmedRange, ExactMathBudget budget)
        {
            Root(history, nameof(history), budget);
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (confirmedRange == null) throw new ArgumentNullException(nameof(confirmedRange));
            var rejection = Context(history, request.PlayerId, request.AttemptId, request.ExpectedSceneRevision, budget);
            if (rejection != null) return rejection;
            if (string.IsNullOrWhiteSpace(request.OperationId)) return Reject(MissingField, "OperationId");
            if (HasOperation(history, request.OperationId)) return Reject(OperationConflict, "OperationId");
            var read = Read(history, request.HistoryAnchorId);
            if (!read.IsAccepted) return read;
            RangeValues(confirmedRange, budget);
            if (confirmedRange.SceneRevision != history.CurrentRun.CurrentSnapshot.SceneRevision) return Reject(StaleContext, "ConfirmedRange.SceneRevision");
            if (!RangeEqual(read.Range, confirmedRange, budget)) return Reject(InconsistentBinding, "ConfirmedRange");
            var range = read.Range; var before = range.BeforeSnapshot;
            var revision = budget.Add(history.CurrentRun.CurrentSnapshot.SceneRevision, BigInteger.One);
            var restored = new BattleSnapshot(before.Baseline, revision, before.EffectiveActionsCompleted, before.EnemyPhasesCompleted,
                before.CurrentFaceIndex, before.Phase, before.Board, before.Members, before.Enemies, before.Random, before.Contributions);
            Check(restored, budget);
            var prefixCount = history.EffectiveAnchors.Count - range.Entries.Count;
            var prefix = new List<CandidateBattleOperationRecord>(); var anchors = new List<string>();
            for (var i = 0; i < prefixCount; i++) { prefix.Add(history.CurrentRun.Records[i]); anchors.Add(history.EffectiveAnchors[i]); }
            var run = new CandidateBattleRun(history.Binding, history.CurrentRun.Baseline, history.CurrentRun.InitialSnapshot, restored, prefix, null);
            var rollback = new CandidateRollbackRecord(request.OperationId, history.CurrentRun, range, run);
            var superseded = new CandidateHistorySupersededBy(request.OperationId, revision);
            var removed = new HashSet<string>(StringComparer.Ordinal);
            foreach (var entry in range.Entries) removed.Add(entry.HistoryAnchorId);
            var archive = new List<CandidateHistoryEntry>();
            foreach (var entry in history.Archive)
                archive.Add(removed.Contains(entry.HistoryAnchorId) ? new CandidateHistoryEntry(entry.HistoryAnchorId, entry.Record, superseded) : entry);
            var rollbacks = new List<CandidateRollbackRecord>(history.RollbackRecords) { rollback };
            return new CandidateHistoryResult(CandidateHistoryOutcome.RollbackPrepared,
                new CandidateBattleHistory(run, archive, anchors, rollbacks), range: range, rollback: rollback);
        }

        public static CandidateHistoryResult FindOperation(CandidateBattleHistory history, string operationId, ExactMathBudget budget)
        {
            Root(history, nameof(history), budget);
            var rejection = History(history, budget);
            if (rejection != null) return rejection;
            if (string.IsNullOrWhiteSpace(operationId)) return Reject(MissingField, "OperationId");
            foreach (var entry in history.Archive)
                if (Same(entry.OperationId, operationId)) return new CandidateHistoryResult(CandidateHistoryOutcome.OperationFound,
                    operation: new CandidateHistoryOperation(entry));
            foreach (var rollback in history.RollbackRecords)
                if (Same(rollback.OperationId, operationId)) return new CandidateHistoryResult(CandidateHistoryOutcome.OperationFound,
                    operation: new CandidateHistoryOperation(rollback));
            return Reject(NotFound, "OperationId");
        }

        private static CandidateHistoryResult Context(CandidateBattleHistory history, string player, string attempt,
            BigInteger? revision, ExactMathBudget math)
        {
            if (string.IsNullOrWhiteSpace(player)) return Reject(MissingField, "PlayerId");
            if (string.IsNullOrWhiteSpace(attempt)) return Reject(MissingField, "AttemptId");
            if (!revision.HasValue) return Reject(MissingField, "ExpectedSceneRevision");
            math.CheckInteger(revision.Value);
            if (revision.Value.Sign <= 0) return Reject(InvalidValue, "ExpectedSceneRevision");
            var rejection = History(history, math);
            if (rejection != null) return rejection;
            var entry = history.CurrentRun.Baseline.Entry;
            if (!Same(player, entry.PlayerId)) return Reject(InconsistentBinding, "PlayerId");
            if (!Same(attempt, entry.AttemptId)) return Reject(InconsistentBinding, "AttemptId");
            if (revision.Value != history.CurrentRun.CurrentSnapshot.SceneRevision) return Reject(StaleContext, "ExpectedSceneRevision");
            return Active(history.CurrentRun);
        }

        private static CandidateHistoryResult Read(CandidateBattleHistory history, string anchor)
        {
            if (string.IsNullOrWhiteSpace(anchor)) return Reject(MissingField, "HistoryAnchorId");
            for (var i = 0; i < history.EffectiveAnchors.Count; i++)
                if (Same(anchor, history.EffectiveAnchors[i])) return new CandidateHistoryResult(CandidateHistoryOutcome.RangeFound, range: Range(history, i));
            return Reject(NotFound, "HistoryAnchorId");
        }

        private static CandidateRollbackRange Range(CandidateBattleHistory history, int index)
        {
            var entries = new List<CandidateHistoryEntry>();
            for (var i = index; i < history.EffectiveAnchors.Count; i++)
                foreach (var entry in history.Archive) if (Same(entry.HistoryAnchorId, history.EffectiveAnchors[i])) { entries.Add(entry); break; }
            var first = entries[0];
            return new CandidateRollbackRange(history.Binding, history.CurrentRun.CurrentSnapshot.SceneRevision,
                first.HistoryAnchorId, first.OperationId, first.Record.BeforeSnapshot, entries);
        }

        // Inspect the audit sequence, never execute a battle, draw a word, or import a replacement snapshot.
        private static CandidateHistoryResult History(CandidateBattleHistory history, ExactMathBudget math)
        {
            if (history.CurrentRun?.Binding?.Start?.Baseline == null) return Reject(MissingField, "History.CurrentRun");
            var binding = history.Binding;
            var rejection = RunValues(history.CurrentRun, binding, math);
            if (rejection != null) return rejection;
            var operations = new HashSet<string>(StringComparer.Ordinal); var anchors = new HashSet<string>(StringComparer.Ordinal);
            foreach (var entry in history.Archive)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.HistoryAnchorId)) return Reject(IncompleteHistory, "History.Archive");
                rejection = RecordValues(entry.Record, binding, math);
                if (rejection != null) return rejection;
                if (!anchors.Add(entry.HistoryAnchorId) || !operations.Add(entry.OperationId)) return Reject(OperationConflict, "History.Archive");
                if (entry.SupersededBy != null) math.CheckInteger(entry.SupersededBy.SceneRevision);
            }
            foreach (var rollback in history.RollbackRecords)
            {
                if (rollback?.BeforeRun == null || rollback.RestoredRun == null || rollback.Range == null || string.IsNullOrWhiteSpace(rollback.OperationId))
                    return Reject(IncompleteHistory, "History.RollbackRecords");
                rejection = RunValues(rollback.BeforeRun, binding, math) ?? RunValues(rollback.RestoredRun, binding, math);
                if (rejection != null) return rejection;
                RangeValues(rollback.Range, math);
                if (!operations.Add(rollback.OperationId)) return Reject(OperationConflict, "History.RollbackRecords.OperationId");
            }
            var effective = new List<CandidateHistoryEntry>();
            var superseded = new Dictionary<string, CandidateRollbackRecord>(StringComparer.Ordinal);
            var current = binding.Start.Snapshot; var a = 0; var r = 0;
            while (a < history.Archive.Count || r < history.RollbackRecords.Count)
            {
                var attackNext = a < history.Archive.Count && (r == history.RollbackRecords.Count ||
                    history.Archive[a].Record.BeforeSnapshot.SceneRevision < history.RollbackRecords[r].BeforeRun.CurrentSnapshot.SceneRevision);
                if (current.Phase == BattlePhase.WonPendingSettlement || current.Phase == BattlePhase.Closed) return Reject(IncompleteHistory, "History.AfterTerminal");
                if (attackNext)
                {
                    var entry = history.Archive[a++];
                    if (!Equal(current, entry.Record.BeforeSnapshot, math)) return Reject(IncompleteHistory, "History.Archive.BeforeSnapshot");
                    effective.Add(entry); current = entry.Record.AfterSnapshot;
                }
                else
                {
                    var rollback = history.RollbackRecords[r++];
                    if (!Equal(current, rollback.BeforeRun.CurrentSnapshot, math) || !Prefix(rollback.BeforeRun.Records, effective, effective.Count))
                        return Reject(IncompleteHistory, "History.RollbackRecords.BeforeRun");
                    var index = effective.FindIndex(e => Same(e.HistoryAnchorId, rollback.Range.HistoryAnchorId));
                    if (index < 0) return Reject(IncompleteHistory, "History.RollbackRecords.Range");
                    var expected = new CandidateRollbackRange(binding, current.SceneRevision, effective[index].HistoryAnchorId,
                        effective[index].OperationId, effective[index].Record.BeforeSnapshot, effective.GetRange(index, effective.Count - index));
                    if (!RangeEqual(expected, rollback.Range, math)) return Reject(IncompleteHistory, "History.RollbackRecords.Range");
                    var restored = rollback.RestoredRun;
                    if (restored.FinalReport != null || restored.CurrentSnapshot.SceneRevision != math.Add(current.SceneRevision, BigInteger.One) ||
                        !Equal(expected.BeforeSnapshot, restored.CurrentSnapshot, math, true) || !Prefix(restored.Records, effective, index))
                        return Reject(IncompleteHistory, "History.RollbackRecords.RestoredRun");
                    for (var i = index; i < effective.Count; i++) superseded.Add(effective[i].HistoryAnchorId, rollback);
                    effective.RemoveRange(index, effective.Count - index); current = restored.CurrentSnapshot;
                }
            }
            if (!Equal(current, history.CurrentRun.CurrentSnapshot, math) || !Prefix(history.CurrentRun.Records, effective, effective.Count) ||
                effective.Count != history.EffectiveAnchors.Count) return Reject(IncompleteHistory, "History.CurrentRun");
            for (var i = 0; i < effective.Count; i++)
                if (!Same(effective[i].HistoryAnchorId, history.EffectiveAnchors[i])) return Reject(IncompleteHistory, "History.EffectiveAnchors");
            foreach (var entry in history.Archive)
            {
                if (!superseded.TryGetValue(entry.HistoryAnchorId, out var rollback))
                { if (entry.SupersededBy != null) return Reject(IncompleteHistory, "History.Archive.SupersededBy"); }
                else if (entry.SupersededBy == null || !Same(entry.SupersededBy.OperationId, rollback.OperationId) ||
                    entry.SupersededBy.SceneRevision != rollback.SceneRevision) return Reject(IncompleteHistory, "History.Archive.SupersededBy");
            }
            return Report(history.CurrentRun, math);
        }

        private static CandidateHistoryResult RunValues(CandidateBattleRun run, CandidateRandomBinding binding, ExactMathBudget math)
        {
            if (run == null || binding?.Start?.Baseline?.Entry == null || binding.Start.Snapshot == null) return Reject(MissingField, "Run");
            if (!ReferenceEquals(run.Binding, binding) || !ReferenceEquals(run.Baseline, binding.Start.Baseline) ||
                !ReferenceEquals(run.InitialSnapshot, binding.Start.Snapshot)) return Reject(InconsistentBinding, "Run.Binding");
            Check(binding, math); Check(run.Baseline, math); Check(run.InitialSnapshot, math);
            var rejection = Snapshot(run.CurrentSnapshot, run.Baseline, math);
            if (rejection != null) return rejection;
            foreach (var record in run.Records)
            {
                rejection = Shape(record);
                if (rejection != null) return rejection;
                Check(record, math);
            }
            if (run.FinalReport != null)
            {
                if (run.FinalReport.Outcome != CandidateBattleOutcome.NormalVictory ||
                    run.FinalReport.ConsumptionCoverage != CandidateConsumptionCoverage.EmptyCarryNoUse)
                    return Reject(UnsupportedBinding, "Run.FinalReport");
                Check(run.FinalReport, math);
            }
            return null;
        }

        private static CandidateHistoryResult Snapshot(BattleSnapshot state, BattleEntryBaseline baseline, ExactMathBudget math)
        {
            if (state?.Board?.Face == null || state.Random?.Stream == null) return Reject(MissingField, "Snapshot");
            if (!ReferenceEquals(state.Baseline, baseline)) return Reject(InconsistentBinding, "Snapshot.Baseline");
            if (state.CurrentFaceIndex < 0 || state.CurrentFaceIndex >= baseline.Entry.Level.Faces.Count) return Reject(InvalidValue, "Snapshot.CurrentFaceIndex");
            var face = baseline.Entry.Level.Faces[state.CurrentFaceIndex];
            if (!ReferenceEquals(state.Board.Face, face)) return Reject(InconsistentBinding, "Snapshot.Board.Face");
            if (state.Phase < BattlePhase.AwaitAction || state.Phase > BattlePhase.Closed) return Reject(InvalidPhase, "Snapshot.Phase");
            if (state.CarryMode != EntryCarryMode.Empty) return Reject(UnsupportedBinding, "Snapshot.CarryMode");
            if (state.Members.Count != baseline.Entry.ReadyParticipants.Count || state.Enemies.Count != face.Pairs.Count)
                return Reject(InconsistentBinding, "Snapshot.Combatants");
            for (var i = 0; i < state.Members.Count; i++)
                if (state.Members[i]?.Hp == null || !ReferenceEquals(state.Members[i].Member, baseline.Entry.ReadyParticipants[i]))
                    return Reject(InconsistentBinding, "Snapshot.Members");
            for (var i = 0; i < state.Enemies.Count; i++)
                if (state.Enemies[i]?.Hp == null || !ReferenceEquals(state.Enemies[i].Enemy, face.Pairs[i].Enemy))
                    return Reject(InconsistentBinding, "Snapshot.Enemies");
            if (state.Random.PrdStates.Count != state.Members.Count) return Reject(InconsistentBinding, "Snapshot.Random.PrdStates");
            for (var i = 0; i < state.Members.Count; i++)
            {
                var prd = state.Random.PrdStates[i]; var member = state.Members[i];
                if (prd == null || !prd.CombatantKey.Equals(member.CombatantKey) || !ReferenceEquals(prd.Crit, member.Member.Crit))
                    return Reject(InconsistentBinding, "Snapshot.Random.PrdStates");
            }
            Check(state, math); return null;
        }

        private static CandidateHistoryResult RecordValues(CandidateBattleOperationRecord record, CandidateRandomBinding binding, ExactMathBudget math)
        {
            var rejection = Shape(record);
            if (rejection != null) return rejection;
            var before = record.BeforeSnapshot; var stage = record.StageDecision; var source = stage.Source;
            rejection = Snapshot(before, binding.Start.Baseline, math) ?? Snapshot(record.AfterSnapshot, binding.Start.Baseline, math);
            if (rejection != null) return rejection;
            if (record.Kind != CandidateBattleOperationKind.Attack && record.Kind != CandidateBattleOperationKind.Link)
                return Reject(UnsupportedBinding, "Record.Kind");
            if (record.ConsumptionCoverage != CandidateConsumptionCoverage.EmptyCarryNoUse) return Reject(UnsupportedBinding, "Record.ConsumptionCoverage");
            if (record.OccurredAtUnixMilliseconds.Sign < 0 || string.IsNullOrWhiteSpace(record.OperationId)) return Reject(InvalidValue, "Record");
            if (!ReferenceEquals(stage.BeforeSnapshot, before) || !Same(source.PlayerId, binding.Start.Baseline.Entry.PlayerId) ||
                !Same(source.AttemptId, binding.Start.Baseline.Entry.AttemptId) || source.ExpectedSceneRevision != before.SceneRevision)
                return Reject(InconsistentBinding, "Record.Request");
            if (record.Kind == CandidateBattleOperationKind.Attack)
            {
                var direct = record.DirectAttack; var enemy = record.EnemyPhase;
                if (record.Conditions == null || record.Conditions.PreferenceRevision.Sign <= 0 || direct?.Action == null || enemy == null)
                    return Reject(IncompleteHistory, "Record.Attack");
                if (!ReferenceEquals(direct.Binding, binding) || !ReferenceEquals(direct.BeforeSnapshot, before) ||
                    !ReferenceEquals(enemy.DirectAttack, direct) || source.Kind != CandidateStageOperation.AfterAttack)
                    return Reject(InconsistentBinding, "Record.Attack.Source");
                var action = direct.Action;
                var expected = new CandidateStageSource(CandidateStageOperation.AfterAttack, action.PlayerId, action.AttemptId,
                    action.OperationId, action.ExpectedSceneRevision, action.Actor, action.Pair, action.Route);
                if (!Equal(expected, source, math)) return Reject(IncompleteHistory, "Record.Request");
                math.CheckInteger(action.ActionOrdinal); Check(direct.Enemies, math); Check(direct.Members, math);
                Check(direct.Random, math); Check(direct.Contributions, math); Check(enemy, math);
                foreach (var hit in direct.DamageFacts)
                    if (hit == null || !ReferenceEquals(hit.Baseline, before.Baseline)) return Reject(InconsistentBinding, "Record.DirectFacts");
                foreach (var intent in enemy.OrderedIntents)
                    if (intent == null || !ReferenceEquals(intent.Baseline, before.Baseline)) return Reject(InconsistentBinding, "Record.EnemyFacts");
                if (stage.FinalHp == null || stage.FinalHp.MemberHp.Count != enemy.Members.Count || stage.FinalHp.EnemyHp.Count != enemy.Enemies.Count)
                    return Reject(IncompleteHistory, "Record.FinalHp");
                for (var i = 0; i < enemy.Members.Count; i++)
                    if (!enemy.Members[i].CombatantKey.Equals(stage.FinalHp.MemberHp[i].CombatantKey) ||
                        !Equal(enemy.Members[i].Hp, stage.FinalHp.MemberHp[i].Hp, math)) return Reject(IncompleteHistory, "Record.FinalHp.MemberHp");
                for (var i = 0; i < enemy.Enemies.Count; i++)
                    if (!enemy.Enemies[i].CombatantKey.Equals(stage.FinalHp.EnemyHp[i].CombatantKey) ||
                        !Equal(enemy.Enemies[i].Hp, stage.FinalHp.EnemyHp[i].Hp, math)) return Reject(IncompleteHistory, "Record.FinalHp.EnemyHp");
            }
            else if (record.Conditions != null || record.DirectAttack != null || record.EnemyPhase != null ||
                source.Actor != null || source.Kind != CandidateStageOperation.CompleteLink) return Reject(IncompleteHistory, "Record.Link");
            Check(record, math); Check(stage, math);
            // Reassemble immutable fragment outputs only; this helper never evaluates an enemy or samples randomness.
            var assembled = CandidateBattleOperations.AssembleRecord(record.Kind, record.OccurredAtUnixMilliseconds, before,
                record.Conditions, record.DirectAttack, record.EnemyPhase, stage, math);
            if (!Equal(record.AfterSnapshot, assembled.AfterSnapshot, math) || !Equal(record.ContributionSegments, assembled.ContributionSegments, math))
                return Reject(IncompleteHistory, "Record.AfterSnapshot");
            if (record.OrderedFacts.Count != assembled.OrderedFacts.Count) return Reject(IncompleteHistory, "Record.OrderedFacts");
            for (var i = 0; i < record.OrderedFacts.Count; i++)
            {
                var actual = record.OrderedFacts[i]; var expected = assembled.OrderedFacts[i];
                if (actual == null || actual.Index != i || actual.Kind != expected.Kind || !ReferenceEquals(actual.DirectAttack, expected.DirectAttack) ||
                    !ReferenceEquals(actual.EnemyIntent, expected.EnemyIntent) || !ReferenceEquals(actual.Stage, expected.Stage))
                    return Reject(IncompleteHistory, "Record.OrderedFacts");
                math.CheckInteger(actual.Index);
            }
            return null;
        }

        private static CandidateHistoryResult Shape(CandidateBattleOperationRecord record)
        {
            if (record?.StageDecision?.Source == null || record.BeforeSnapshot == null || record.AfterSnapshot == null)
                return Reject(IncompleteHistory, "Record");
            if ((record.Kind != CandidateBattleOperationKind.Attack && record.Kind != CandidateBattleOperationKind.Link) ||
                record.ConsumptionCoverage != CandidateConsumptionCoverage.EmptyCarryNoUse) return Reject(UnsupportedBinding, "Record.KindOrCoverage");
            var stage = record.StageDecision;
            if ((stage.Source.Kind != CandidateStageOperation.AfterAttack && stage.Source.Kind != CandidateStageOperation.CompleteLink) ||
                stage.NextPhase < BattlePhase.AwaitAction || stage.NextPhase > BattlePhase.Closed) return Reject(UnsupportedBinding, "Record.StageDecision");
            foreach (var fact in stage.OrderedFacts)
                if (fact == null || fact.Kind < CandidateStageFactKind.TemporaryRouteRemoved || fact.Kind > CandidateStageFactKind.PhaseSelected ||
                    (fact.Phase.HasValue && (fact.Phase < BattlePhase.AwaitAction || fact.Phase > BattlePhase.Closed)))
                    return Reject(UnsupportedBinding, "Record.StageFacts");
            foreach (var segment in record.ContributionSegments)
                if (segment?.HpLoss == null || (segment.Kind != CandidateContributionKind.DamageDealtHp && segment.Kind != CandidateContributionKind.DamageTakenHp) ||
                    (segment.RuleSegment != CandidateBattleFactKind.DirectAttack && segment.RuleSegment != CandidateBattleFactKind.EnemyIntent))
                    return Reject(UnsupportedBinding, "Record.ContributionSegments");
            if (record.DirectAttack != null)
                foreach (var hit in record.DirectAttack.DamageFacts)
                    if (hit?.Crit?.StreamBefore == null || hit.Crit.StreamAfter == null || hit.DamageKind != EntryDamageKind.Physical)
                        return Reject(IncompleteHistory, "Record.DirectFacts");
            if (record.EnemyPhase != null)
                foreach (var fact in record.EnemyPhase.OrderedIntents)
                    if (fact == null || (fact.IntentKind != EnemyIntentKind.Charge && fact.IntentKind != EnemyIntentKind.Strike) ||
                        (fact.Damage != null && fact.Damage.DamageKind != EntryDamageKind.Physical)) return Reject(UnsupportedBinding, "Record.EnemyFacts");
            return null;
        }

        private static CandidateHistoryResult Report(CandidateBattleRun run, ExactMathBudget math)
        {
            var report = run.FinalReport;
            if (run.CurrentSnapshot.Phase != BattlePhase.WonPendingSettlement)
                return report == null ? null : Reject(IncompleteHistory, "Run.FinalReport");
            if (report == null || run.Records.Count == 0 || !ReferenceEquals(report.Binding, run.Binding) ||
                !ReferenceEquals(report.Baseline, run.Baseline) || !ReferenceEquals(report.InitialSnapshot, run.InitialSnapshot) ||
                !ReferenceEquals(report.FinalSnapshot, run.CurrentSnapshot) || report.Operations.Count != run.Records.Count)
                return Reject(IncompleteHistory, "Run.FinalReport");
            var contributions = new List<CandidateContributionSegment>();
            for (var i = 0; i < run.Records.Count; i++)
            { if (!ReferenceEquals(run.Records[i], report.Operations[i])) return Reject(IncompleteHistory, "Run.FinalReport.Operations"); contributions.AddRange(run.Records[i].ContributionSegments); }
            var last = run.Records[run.Records.Count - 1];
            if (!Same(last.OperationId, report.TerminalOperationId) || last.OccurredAtUnixMilliseconds != report.EndedAtUnixMilliseconds ||
                report.Outcome != CandidateBattleOutcome.NormalVictory || report.ConsumptionCoverage != CandidateConsumptionCoverage.EmptyCarryNoUse ||
                !Equal(contributions, report.Contributions, math) || !Same(Compute(report, math), report.Fingerprint)) return Reject(IncompleteHistory, "Run.FinalReport");
            return null;
        }

        private static void RangeValues(CandidateRollbackRange range, ExactMathBudget math)
        {
            math.CheckInteger(range.SceneRevision); Check(range.Binding, math); Check(range.BeforeSnapshot, math);
            foreach (var entry in range.Entries)
                if (entry != null) { Check(entry.Record, math); if (entry.SupersededBy != null) math.CheckInteger(entry.SupersededBy.SceneRevision); }
        }
        private static bool RangeEqual(CandidateRollbackRange a, CandidateRollbackRange b, ExactMathBudget math)
        {
            if (!ReferenceEquals(a.Binding, b.Binding) || !Same(a.PlayerId, b.PlayerId) || !Same(a.AttemptId, b.AttemptId) ||
                a.SceneRevision != b.SceneRevision || !Same(a.HistoryAnchorId, b.HistoryAnchorId) || !Same(a.OperationId, b.OperationId) ||
                !ReferenceEquals(a.BeforeSnapshot, b.BeforeSnapshot) || !Equal(a.BeforeSnapshot, b.BeforeSnapshot, math) || a.Entries.Count != b.Entries.Count) return false;
            for (var i = 0; i < a.Entries.Count; i++)
                if (b.Entries[i] == null || !Same(a.Entries[i].HistoryAnchorId, b.Entries[i].HistoryAnchorId) ||
                    !ReferenceEquals(a.Entries[i].Record, b.Entries[i].Record) || b.Entries[i].SupersededBy != null) return false;
            return true;
        }
        private static bool Prefix(IReadOnlyList<CandidateBattleOperationRecord> records, List<CandidateHistoryEntry> entries, int count)
        { if (records.Count != count) return false; for (var i = 0; i < count; i++) if (!ReferenceEquals(records[i], entries[i].Record)) return false; return true; }
        private static bool HasOperation(CandidateBattleHistory history, string operation)
        {
            foreach (var entry in history.Archive) if (Same(entry.OperationId, operation)) return true;
            foreach (var rollback in history.RollbackRecords) if (Same(rollback.OperationId, operation)) return true;
            return false;
        }
        private static CandidateHistoryResult Active(CandidateBattleRun run)
        { return run.FinalReport != null || run.CurrentSnapshot.Phase == BattlePhase.WonPendingSettlement || run.CurrentSnapshot.Phase == BattlePhase.Closed
            ? Reject(InvalidPhase, "CurrentRun.CurrentSnapshot.Phase") : null; }
        private static bool Same(string a, string b) { return string.Equals(a, b, StringComparison.Ordinal); }
        private static CandidateHistoryResult Reject(CandidateHistoryRejectionCode code, string path) { return new CandidateHistoryResult(code, path); }
        private static void Root(object value, string name, ExactMathBudget budget)
        { if (value == null) throw new ArgumentNullException(name); if (budget == null) throw new ArgumentNullException(nameof(budget)); }
    }
}
