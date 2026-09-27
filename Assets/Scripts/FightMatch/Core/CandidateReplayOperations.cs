using System;
using System.Collections.Generic;
using System.Numerics;
using FlowPuzzle.Core;
using static FightMatch.Core.CandidateBattleReportFingerprint;

namespace FightMatch.Core
{
    public static class CandidateReplayOperations
    {
        public static CandidateReplayResult BuildIsolationInput(CandidateBattleHistory history,
            CandidateIsolationRequest request, ExactMathBudget budget)
        {
            Root(history, nameof(history), budget);
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(request.PlayerId)) return Reject("MissingField", "PlayerId");
            if (string.IsNullOrWhiteSpace(request.AttemptId)) return Reject("MissingField", "AttemptId");
            if (!request.ExpectedSceneRevision.HasValue) return Reject("MissingField", "ExpectedSceneRevision");
            budget.CheckInteger(request.ExpectedSceneRevision.Value);
            if (request.ExpectedSceneRevision.Value.Sign <= 0) return Reject("InvalidValue", "ExpectedSceneRevision");
            if (!request.Purpose.HasValue) return Reject("MissingField", "Purpose");
            if (request.Purpose != CandidateIsolationPurpose.CurrentPlan && request.Purpose != CandidateIsolationPurpose.RecordedOperation)
                return Reject("UnsupportedBinding", "Purpose");
            if (!request.RightsMode.HasValue) return Reject("MissingField", "RightsMode");
            if (request.RightsMode != CandidateIsolationRights.Empty) return Reject("UnsupportedBinding", "RightsMode");
            var current = history.CurrentRun;
            if (current?.Baseline?.Entry == null || current.CurrentSnapshot == null) return Reject("MissingField", "History.CurrentRun");
            var entry = current.Baseline.Entry;
            if (!Same(request.PlayerId, entry.PlayerId)) return Reject("InconsistentBinding", "PlayerId");
            if (!Same(request.AttemptId, entry.AttemptId)) return Reject("InconsistentBinding", "AttemptId");
            if (request.ExpectedSceneRevision != current.CurrentSnapshot.SceneRevision) return Reject("StaleContext", "ExpectedSceneRevision");
            CandidateBattleConditionValues conditions = null;
            CandidateBattleOperationRecord recorded = null; CandidateFinalAttemptReport report = null;
            var run = current;
            if (request.Purpose == CandidateIsolationPurpose.CurrentPlan)
            {
                if (request.RecordedOperationId != null) return Reject("InvalidValue", "RecordedOperationId");
                var bad = Conditions(request.CurrentConditions, budget);
                if (bad != null) return bad;
                conditions = new CandidateBattleConditionValues(request.CurrentConditions.PreferenceRevision.Value, request.CurrentConditions.ItemUseEnabled.Value);
                CandidateHistoryResult checkedHistory;
                if (history.Archive.Count == 0)
                {
                    if (history.RollbackRecords.Count != 0 || history.EffectiveAnchors.Count != 0) return Reject("IncompleteHistory", "History");
                    checkedHistory = CandidateHistoryOperations.CreateCandidate(current, budget);
                }
                else
                {
                    if (history.Archive[0]?.Record?.StageDecision?.Source == null) return Reject("IncompleteHistory", "History.Archive[0]");
                    checkedHistory = CandidateHistoryOperations.FindOperation(history, history.Archive[0].OperationId, budget);
                }
                if (!checkedHistory.IsAccepted) return Reject(checkedHistory.RejectionCode.ToString(), checkedHistory.FieldPath);
            }
            else
            {
                if (request.CurrentConditions != null) return Reject("InvalidValue", "CurrentConditions");
                if (string.IsNullOrWhiteSpace(request.RecordedOperationId)) return Reject("MissingField", "RecordedOperationId");
                var found = CandidateHistoryOperations.FindOperation(history, request.RecordedOperationId, budget);
                if (!found.IsAccepted) return Reject(found.RejectionCode.ToString(), found.FieldPath);
                if (found.Operation.Relation == CandidateHistoryOperationRelation.RollbackRecorded) return Reject("UnsupportedBinding", "RecordedOperationId");
                var original = found.Operation.Entry; var source = current;
                if (original.SupersededBy != null)
                {
                    source = null;
                    foreach (var rollback in history.RollbackRecords)
                        if (Same(rollback.OperationId, original.SupersededBy.OperationId) && rollback.SceneRevision == original.SupersededBy.SceneRevision)
                        {
                            if (source != null) return Reject("IncompleteHistory", "History.RollbackRecords");
                            source = rollback.BeforeRun;
                        }
                    if (source == null) return Reject("IncompleteHistory", "RecordedOperation.SupersededBy");
                }
                var index = -1;
                for (var i = 0; i < source.Records.Count; i++)
                    if (Same(source.Records[i].OperationId, original.OperationId))
                    {
                        if (index >= 0 || !ReferenceEquals(source.Records[i], original.Record)) return Reject("IncompleteHistory", "RecordedOperation.SourcePrefix");
                        index = i;
                    }
                if (index < 0) return Reject("IncompleteHistory", "RecordedOperation.SourcePrefix");
                recorded = original.Record; conditions = recorded.Conditions;
                var prefix = new List<CandidateBattleOperationRecord>();
                for (var i = 0; i < index; i++) prefix.Add(source.Records[i]);
                if (index == source.Records.Count - 1) report = source.FinalReport;
                run = new CandidateBattleRun(source.Binding, source.Baseline, source.InitialSnapshot, recorded.BeforeSnapshot, prefix, null);
            }
            var used = new List<string>();
            foreach (var row in history.Archive) used.Add(row.OperationId);
            foreach (var row in history.RollbackRecords) used.Add(row.OperationId);
            var key = new CandidateReplayContextKey(request.Purpose.Value, entry.PlayerId, entry.AttemptId,
                current.CurrentSnapshot.SceneRevision, run, conditions, request.RightsMode.Value, request.RecordedOperationId, used, report != null);
            var input = new CandidateBattleIsolation(key, recorded, report);
            var rejection = Isolation(input, request.Purpose.Value, budget);
            return rejection ?? new CandidateReplayResult(input);
        }

        public static CandidateReplayResult ReplayRecorded(CandidateBattleIsolation input, RandomSamplingBudget budget)
        {
            Root(input, nameof(input), budget);
            var bad = Isolation(input, CandidateIsolationPurpose.RecordedOperation, budget.Math);
            if (bad != null) return bad;
            var original = input.RecordedOperation; var request = original.Request;
            var step = new FrozenStep(original.Kind, original.OperationId, original.OccurredAtUnixMilliseconds,
                request.Actor, request.Pair, request.Route);
            var actual = Execute(input.Run, step, input.Conditions, budget);
            if (!actual.IsAccepted) return From(actual, 0);
            var difference = CandidateReplayComparison.Compare(original, actual.Record, input.RecordedReport, actual.NextRun.FinalReport, budget.Math);
            return new CandidateReplayResult(difference == null ? CandidateReplayOutcome.Matched : CandidateReplayOutcome.Diverged,
                actual.NextRun, new[] { actual.Record }, difference);
        }

        public static CandidateReplayResult EvaluatePlan(CandidateBattleIsolation input, IReadOnlyList<CandidateReplayStep> steps,
            RandomSamplingBudget budget)
        {
            Root(input, nameof(input), budget);
            if (steps == null) throw new ArgumentNullException(nameof(steps));
            var bad = Isolation(input, CandidateIsolationPurpose.CurrentPlan, budget.Math);
            if (bad != null) return bad;
            if (steps.Count == 0) return Reject("InvalidValue", "Steps");
            // Freeze the whole ordered plan before executing its first operation.
            var frozen = new List<FrozenStep>();
            foreach (var step in steps) frozen.Add(step == null ? null : new FrozenStep(step.Kind, step.OperationId,
                step.OccurredAtUnixMilliseconds, step.Actor, step.Pair, step.Route));
            var used = new HashSet<string>(input.ContextKey.UsedOperationIds, StringComparer.Ordinal);
            var run = input.Run; var records = new List<CandidateBattleOperationRecord>();
            for (var i = 0; i < frozen.Count; i++)
            {
                var step = frozen[i]; var path = $"Steps[{i}]";
                if (step == null) return Reject("MissingField", path, i);
                if (!step.Kind.HasValue) return Reject("MissingField", path + ".Kind", i);
                if (step.Kind != CandidateBattleOperationKind.Attack && step.Kind != CandidateBattleOperationKind.Link)
                    return Reject("UnsupportedBinding", path + ".Kind", i);
                if (string.IsNullOrWhiteSpace(step.Operation)) return Reject("MissingField", path + ".OperationId", i);
                if (!used.Add(step.Operation)) return Reject("OperationConflict", path + ".OperationId", i);
                if (!step.Time.HasValue) return Reject("MissingField", path + ".OccurredAtUnixMilliseconds", i);
                budget.Math.CheckInteger(step.Time.Value);
                if (step.Time.Value.Sign < 0) return Reject("InvalidValue", path + ".OccurredAtUnixMilliseconds", i);
                if (step.Pair == null) return Reject("MissingField", path + ".Pair", i);
                if (step.Route == null) return Reject("MissingField", path + ".Route", i);
                if (step.Kind == CandidateBattleOperationKind.Attack && step.Actor == null) return Reject("MissingField", path + ".Actor", i);
                if (step.Kind == CandidateBattleOperationKind.Link && step.Actor != null) return Reject("InvalidValue", path + ".Actor", i);
                var result = Execute(run, step, input.Conditions, budget);
                if (!result.IsAccepted) return From(result, i);
                records.Add(result.Record); run = result.NextRun;
            }
            return new CandidateReplayResult(CandidateReplayOutcome.PlanEvaluated, run, records,
                evidence: new CandidateReplayPlanEvidence(input.ContextKey, records));
        }

        private static CandidateBattleResult Execute(CandidateBattleRun run, FrozenStep step,
            CandidateBattleConditionValues conditions, RandomSamplingBudget budget)
        {
            var before = run.CurrentSnapshot; var entry = run.Baseline.Entry;
            if (step.Kind == CandidateBattleOperationKind.Link)
                return CandidateBattleOperations.EvaluateLink(run, new CandidateLinkRequest { PlayerId = entry.PlayerId,
                    AttemptId = entry.AttemptId, OperationId = step.Operation, ExpectedSceneRevision = before.SceneRevision,
                    Pair = step.Pair, Route = new List<FlowPos>(step.Route) }, step.Time, budget.Math);
            return CandidateBattleOperations.EvaluateAttack(run, new CandidateAttackRequest { PlayerId = entry.PlayerId,
                AttemptId = entry.AttemptId, OperationId = step.Operation, ExpectedSceneRevision = before.SceneRevision,
                Actor = step.Actor, Pair = step.Pair, Route = new List<FlowPos>(step.Route) },
                new CandidateBattleConditions { PreferenceRevision = conditions.PreferenceRevision, ItemUseEnabled = conditions.ItemUseEnabled }, step.Time, budget);
        }

        private static CandidateReplayResult Isolation(CandidateBattleIsolation input, CandidateIsolationPurpose purpose, ExactMathBudget math)
        {
            var key = input.ContextKey;
            if (key == null || key.StartingRun == null) return Reject("MissingField", "Input.ContextKey.StartingRun");
            if (key.Purpose != purpose) return Reject("UnsupportedBinding", "Input.ContextKey.Purpose");
            if (key.RightsMode != CandidateIsolationRights.Empty) return Reject("UnsupportedBinding", "Input.ContextKey.RightsMode");
            math.CheckInteger(key.SourceSceneRevision);
            var run = input.Run;
            if (run.Binding == null) return Reject("MissingField", "Input.Run.Binding");
            var binding = CandidateBattleOperations.CreateCandidate(run.Binding, math);
            if (!binding.IsAccepted) return From(binding, null);
            if (!ReferenceEquals(run.Baseline, run.Binding.Start.Baseline) || !ReferenceEquals(run.InitialSnapshot, run.Binding.Start.Snapshot))
                return Reject("InconsistentBinding", "Input.Run.Baseline");
            if (!Same(key.PlayerId, run.Baseline.Entry.PlayerId) || !Same(key.AttemptId, run.Baseline.Entry.AttemptId))
                return Reject("InconsistentBinding", "Input.ContextKey");
            var bad = SnapshotGraph(run.CurrentSnapshot, run.Baseline, "Input.Run.CurrentSnapshot");
            if (bad != null) return bad;
            if (key.SourceSceneRevision < run.CurrentSnapshot.SceneRevision) return Reject("StaleContext", "Input.ContextKey.SourceSceneRevision");
            foreach (var row in run.Records)
                if (row?.StageDecision?.Source == null || row.BeforeSnapshot == null || row.AfterSnapshot == null)
                    return Reject("IncompleteHistory", "Input.Run.Records");
            Check(run.Baseline, math); Check(run.InitialSnapshot, math); Check(run.CurrentSnapshot, math);
            Check(run.Records, math); Check(run.FinalReport, math); Check(key.UsedOperationIds, math);
            if (purpose == CandidateIsolationPurpose.CurrentPlan)
            {
                if (input.RecordedOperation != null || input.RecordedReport != null || key.RecordedOperationId != null)
                    return Reject("InvalidValue", "Input.RecordedOperation");
                if (input.Conditions == null) return Reject("MissingField", "Input.Conditions");
                Check(input.Conditions, math);
                if (input.Conditions.PreferenceRevision.Sign <= 0) return Reject("InvalidValue", "Input.Conditions.PreferenceRevision");
                return null;
            }
            var record = input.RecordedOperation;
            if (key.RecordedHasFinalReport != (input.RecordedReport != null)) return Reject("IncompleteHistory", "Input.RecordedReport");
            if (record?.StageDecision?.Source == null || record.BeforeSnapshot == null || record.AfterSnapshot == null)
                return Reject("IncompleteHistory", "Input.RecordedOperation");
            if (!Same(record.OperationId, key.RecordedOperationId) || !Contains(key.UsedOperationIds, record.OperationId))
                return Reject("IncompleteHistory", "Input.RecordedOperation.OperationId");
            if (run.FinalReport != null || !ReferenceEquals(run.CurrentSnapshot, record.BeforeSnapshot) ||
                !ReferenceEquals(record.StageDecision.BeforeSnapshot, record.BeforeSnapshot)) return Reject("InconsistentBinding", "Input.RecordedOperation.BeforeSnapshot");
            var request = record.Request;
            if (!Same(request.PlayerId, key.PlayerId) || !Same(request.AttemptId, key.AttemptId) ||
                request.ExpectedSceneRevision != run.CurrentSnapshot.SceneRevision) return Reject("InconsistentBinding", "Input.RecordedOperation.Request");
            if (request.Pair == null) return Reject("MissingField", "Input.RecordedOperation.Request.Pair");
            if (record.Kind != CandidateBattleOperationKind.Attack && record.Kind != CandidateBattleOperationKind.Link)
                return Reject("UnsupportedBinding", "Input.RecordedOperation.Kind");
            if (record.OccurredAtUnixMilliseconds.Sign < 0) return Reject("InvalidValue", "Input.RecordedOperation.OccurredAtUnixMilliseconds");
            bad = SnapshotGraph(record.AfterSnapshot, run.Baseline, "Input.RecordedOperation.AfterSnapshot");
            if (bad != null) return bad;
            if (record.StageDecision.FinalHp == null || record.StageDecision.Board?.Face == null)
                return Reject("IncompleteHistory", "Input.RecordedOperation.StageDecision");
            bad = BoardGraph(record.StageDecision.Board, run.Baseline, "Input.RecordedOperation.StageDecision.Board");
            if (bad != null) return bad;
            foreach (var hp in record.StageDecision.FinalHp.MemberHp)
                if (hp?.CombatantKey == null || hp.Hp == null) return Reject("IncompleteHistory", "Input.RecordedOperation.StageDecision.FinalHp.MemberHp");
            foreach (var hp in record.StageDecision.FinalHp.EnemyHp)
                if (hp?.CombatantKey == null || hp.Hp == null) return Reject("IncompleteHistory", "Input.RecordedOperation.StageDecision.FinalHp.EnemyHp");
            if (record.Kind == CandidateBattleOperationKind.Attack)
            {
                if (request.Kind != CandidateStageOperation.AfterAttack || request.Actor == null || record.Conditions == null ||
                    record.DirectAttack == null || record.EnemyPhase == null) return Reject("IncompleteHistory", "Input.RecordedOperation.Attack");
                if (!Equal(input.Conditions, record.Conditions, math)) return Reject("InconsistentBinding", "Input.Conditions");
                var direct = record.DirectAttack;
                if (!ReferenceEquals(direct.Binding, run.Binding) || !ReferenceEquals(direct.BeforeSnapshot, record.BeforeSnapshot) ||
                    !ReferenceEquals(record.EnemyPhase.DirectAttack, direct)) return Reject("InconsistentBinding", "Input.RecordedOperation.DirectAttack");
                if (direct.Action == null || direct.Random?.Stream == null) return Reject("IncompleteHistory", "Input.RecordedOperation.DirectAttack");
                bad = Combatants(direct.Members, direct.Enemies, direct.Random, direct.Contributions, run.Baseline, "Input.RecordedOperation.DirectAttack") ??
                    Combatants(record.EnemyPhase.Members, record.EnemyPhase.Enemies, record.EnemyPhase.Random,
                        record.EnemyPhase.Contributions, run.Baseline, "Input.RecordedOperation.EnemyPhase");
                if (bad != null) return bad;
                foreach (var hit in direct.DamageFacts)
                    if (hit?.Crit?.Parameters == null || hit.Crit.StreamBefore?.Initial == null || hit.Crit.StreamBefore.Current == null ||
                        hit.Crit.StreamAfter?.Initial == null || hit.Crit.StreamAfter.Current == null || hit.Crit.Probability == null ||
                        hit.Attack == null || hit.PhysicalDefense == null || hit.Multiplier == null || hit.RawDamage == null || hit.MitigatedDamage == null ||
                        hit.BlockPrevented == null || hit.ShieldAbsorbed == null || hit.HpBefore == null || hit.HpAfter == null || hit.HpLoss == null || hit.Overflow == null)
                        return Reject("IncompleteHistory", "Input.RecordedOperation.DirectAttack.DamageFacts");
                    else if (!ReferenceEquals(hit.Baseline, run.Baseline)) return Reject("InconsistentBinding", "Input.RecordedOperation.DirectAttack.DamageFacts.Baseline");
                foreach (var intent in record.EnemyPhase.OrderedIntents)
                    if (intent == null) return Reject("IncompleteHistory", "Input.RecordedOperation.EnemyPhase.OrderedIntents");
                    else if (!ReferenceEquals(intent.Baseline, run.Baseline)) return Reject("InconsistentBinding", "Input.RecordedOperation.EnemyPhase.OrderedIntents.Baseline");
            }
            else if (request.Kind != CandidateStageOperation.CompleteLink || request.Actor != null || record.Conditions != null ||
                input.Conditions != null || record.DirectAttack != null || record.EnemyPhase != null)
                return Reject("IncompleteHistory", "Input.RecordedOperation.Link");
            foreach (var fact in record.OrderedFacts) if (fact == null) return Reject("IncompleteHistory", "Input.RecordedOperation.OrderedFacts");
            foreach (var fact in record.StageDecision.OrderedFacts) if (fact == null) return Reject("IncompleteHistory", "Input.RecordedOperation.StageDecision.OrderedFacts");
            foreach (var segment in record.ContributionSegments)
                if (segment?.HpLoss == null) return Reject("IncompleteHistory", "Input.RecordedOperation.ContributionSegments");
            if (input.RecordedReport != null && (!ReferenceEquals(input.RecordedReport.Binding, run.Binding) ||
                !ReferenceEquals(input.RecordedReport.Baseline, run.Baseline) || !ReferenceEquals(input.RecordedReport.InitialSnapshot, run.InitialSnapshot)))
                return Reject("InconsistentBinding", "Input.RecordedReport.Binding");
            if (input.RecordedReport != null)
            {
                if (input.RecordedReport.Fingerprint == null || input.RecordedReport.WholeLevelInitialEnemyHp == null)
                    return Reject("IncompleteHistory", "Input.RecordedReport");
                bad = SnapshotGraph(input.RecordedReport.FinalSnapshot, run.Baseline, "Input.RecordedReport.FinalSnapshot");
                if (bad != null) return bad;
                foreach (var row in input.RecordedReport.Operations)
                    if (row?.StageDecision?.Source == null || row.BeforeSnapshot == null || row.AfterSnapshot == null)
                        return Reject("IncompleteHistory", "Input.RecordedReport.Operations");
            }
            Check(record, math); Check(record.StageDecision, math); Check(record.EnemyPhase, math); Check(input.RecordedReport, math);
            return null;
        }

        private static CandidateReplayResult SnapshotGraph(BattleSnapshot state, BattleEntryBaseline baseline, string path)
        {
            if (state?.Board?.Face == null || state.Random?.Stream == null) return Reject("MissingField", path);
            if (!ReferenceEquals(state.Baseline, baseline)) return Reject("InconsistentBinding", path + ".Baseline");
            return BoardGraph(state.Board, baseline, path + ".Board") ??
                Combatants(state.Members, state.Enemies, state.Random, state.Contributions, baseline, path);
        }
        private static CandidateReplayResult BoardGraph(BattleBoardState board, BattleEntryBaseline baseline, string path)
        {
            var faceFound = false;
            foreach (var face in baseline.Entry.Level.Faces) if (ReferenceEquals(face, board.Face)) faceFound = true;
            if (!faceFound) return Reject("InconsistentBinding", path + ".Face");
            foreach (var route in board.LockedRoutes) if (route?.PairKey == null) return Reject("MissingField", path + ".LockedRoutes");
            foreach (var pair in board.PendingLinks) if (pair == null) return Reject("MissingField", path + ".PendingLinks");
            return null;
        }
        private static CandidateReplayResult Combatants(IReadOnlyList<BattleMemberState> members, IReadOnlyList<BattleEnemyState> enemies,
            BattleRandomSnapshot random, IReadOnlyList<BattleContributionTotals> totals, BattleEntryBaseline baseline, string path)
        {
            if (random?.Stream?.Initial == null || random.Stream.Current == null) return Reject("MissingField", path + ".Random");
            foreach (var member in members)
            {
                if (member?.Hp == null || member.CombatantKey == null) return Reject("MissingField", path + ".Members");
                var found = false; foreach (var definition in baseline.Entry.ReadyParticipants) if (ReferenceEquals(definition, member.Member)) found = true;
                if (!found) return Reject("InconsistentBinding", path + ".Members.Member");
            }
            foreach (var enemy in enemies)
            {
                if (enemy?.Hp == null || enemy.CombatantKey == null || enemy.PairKey == null) return Reject("MissingField", path + ".Enemies");
                var found = false;
                foreach (var face in baseline.Entry.Level.Faces) foreach (var pair in face.Pairs) if (ReferenceEquals(pair.Enemy, enemy.Enemy)) found = true;
                if (!found) return Reject("InconsistentBinding", path + ".Enemies.Enemy");
            }
            foreach (var prd in random.PrdStates)
            {
                if (prd?.CombatantKey == null) return Reject("MissingField", path + ".Random.PrdStates");
                var found = false; foreach (var member in baseline.Entry.ReadyParticipants) if (ReferenceEquals(member.Crit, prd.Crit)) found = true;
                if (!found) return Reject("InconsistentBinding", path + ".Random.PrdStates.Crit");
            }
            foreach (var total in totals)
                if (total?.CombatantKey == null || total.EffectiveDamageDealtHp == null || total.EffectiveDamageTakenHp == null)
                    return Reject("MissingField", path + ".Contributions");
            return null;
        }
        private static CandidateReplayResult Conditions(CandidateBattleConditions conditions, ExactMathBudget math)
        {
            if (conditions == null) return Reject("MissingField", "CurrentConditions");
            if (!conditions.PreferenceRevision.HasValue) return Reject("MissingField", "CurrentConditions.PreferenceRevision");
            if (!conditions.ItemUseEnabled.HasValue) return Reject("MissingField", "CurrentConditions.ItemUseEnabled");
            math.CheckInteger(conditions.PreferenceRevision.Value);
            return conditions.PreferenceRevision.Value.Sign > 0 ? null : Reject("InvalidValue", "CurrentConditions.PreferenceRevision");
        }
        private static bool Same(string a, string b) { return StringComparer.Ordinal.Equals(a, b); }
        private static bool Contains(IReadOnlyList<string> ids, string id)
        { foreach (var existing in ids) if (Same(existing, id)) return true; return false; }
        private static void Root(object input, string name, object budget)
        { if (input == null) throw new ArgumentNullException(name); if (budget == null) throw new ArgumentNullException(nameof(budget)); }
        private static CandidateReplayResult Reject(string code, string path, int? step = null)
        { return new CandidateReplayResult(code, path, step); }
        private static CandidateReplayResult From(CandidateBattleResult result, int? step)
        { return new CandidateReplayResult(result.RejectionCode, result.FieldPath, step, result.RejectionStage, result.RouteReasonCode, result.RouteCellIndex); }
        private sealed class FrozenStep
        {
            internal readonly CandidateBattleOperationKind? Kind;
            internal readonly string Operation;
            internal readonly BigInteger? Time;
            internal readonly BattleCombatantKey Actor;
            internal readonly BattlePairKey Pair;
            internal readonly IReadOnlyList<FlowPos> Route;
            internal FrozenStep(CandidateBattleOperationKind? kind, string operation, BigInteger? time,
                BattleCombatantKey actor, BattlePairKey pair, IEnumerable<FlowPos> route)
            { Kind = kind; Operation = operation; Time = time; Actor = actor; Pair = pair; Route = route == null ? null : new List<FlowPos>(route).AsReadOnly(); }
        }
    }
}
