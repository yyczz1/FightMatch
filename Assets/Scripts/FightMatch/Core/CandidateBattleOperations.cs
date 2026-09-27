using System;
using System.Collections.Generic;
using System.Numerics;
using FlowPuzzle.Core;
using static FightMatch.Core.CandidateBattleReportFingerprint;

namespace FightMatch.Core
{
    public static class CandidateBattleOperations
    {
        public static CandidateBattleResult CreateCandidate(CandidateRandomBinding binding, ExactMathBudget budget)
        {
            if (binding == null) throw new ArgumentNullException(nameof(binding));
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            var rejection = CheckBinding(binding, budget);
            if (rejection != null) return rejection;
            return new CandidateBattleResult(new CandidateBattleRun(binding, binding.Start.Baseline,
                binding.Start.Snapshot, binding.Start.Snapshot, Array.Empty<CandidateBattleOperationRecord>(), null), null, true);
        }

        public static CandidateBattleResult EvaluateAttack(CandidateBattleRun run, CandidateAttackRequest request,
            CandidateBattleConditions conditions, BigInteger? occurredAtUnixMilliseconds, RandomSamplingBudget budget)
        {
            if (run == null) throw new ArgumentNullException(nameof(run));
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (conditions == null) throw new ArgumentNullException(nameof(conditions));
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            if (!conditions.PreferenceRevision.HasValue) return Reject("MissingField", "Conditions.PreferenceRevision");
            if (!conditions.ItemUseEnabled.HasValue) return Reject("MissingField", "Conditions.ItemUseEnabled");
            var math = budget.Math;
            math.CheckInteger(conditions.PreferenceRevision.Value);
            if (conditions.PreferenceRevision.Value.Sign <= 0) return Reject("InvalidValue", "Conditions.PreferenceRevision");
            var rejection = BeforeOperation(run, request.OperationId, occurredAtUnixMilliseconds, math);
            if (rejection != null) return rejection;
            var before = run.CurrentSnapshot;
            // Exactly one new 009 -> 010 -> 011 evaluation; no intermediate result escapes.
            var direct = CandidateDirectAttack.Evaluate(run.Binding, before, request, budget);
            if (!direct.IsAccepted) return From(direct);
            var enemy = CandidateEnemyPhase.Evaluate(direct.Frame, math);
            if (!enemy.IsAccepted) return From(enemy);
            var stage = CandidateBattleStage.AfterAttack(before, request, Projection(enemy.Frame), math);
            if (!stage.IsAccepted) return From(stage);
            var values = new CandidateBattleConditionValues(conditions.PreferenceRevision.Value, conditions.ItemUseEnabled.Value);
            var record = AssembleRecord(CandidateBattleOperationKind.Attack, occurredAtUnixMilliseconds.Value,
                before, values, direct.Frame, enemy.Frame, stage.Decision, math);
            return Finish(run, record, math);
        }

        public static CandidateBattleResult EvaluateLink(CandidateBattleRun run, CandidateLinkRequest request,
            BigInteger? occurredAtUnixMilliseconds, ExactMathBudget budget)
        {
            if (run == null) throw new ArgumentNullException(nameof(run));
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            var rejection = BeforeOperation(run, request.OperationId, occurredAtUnixMilliseconds, budget);
            if (rejection != null) return rejection;
            var stage = CandidateBattleStage.CompleteLink(run.CurrentSnapshot, request, budget);
            if (!stage.IsAccepted) return From(stage);
            return Finish(run, AssembleRecord(CandidateBattleOperationKind.Link, occurredAtUnixMilliseconds.Value,
                run.CurrentSnapshot, null, null, null, stage.Decision, budget), budget);
        }

        private static CandidateBattleResult BeforeOperation(CandidateBattleRun run, string operation,
            BigInteger? time, ExactMathBudget math)
        {
            if (!time.HasValue) return Reject("MissingField", "OccurredAtUnixMilliseconds");
            math.CheckInteger(time.Value);
            if (time.Value.Sign < 0) return Reject("InvalidValue", "OccurredAtUnixMilliseconds");
            if (string.IsNullOrWhiteSpace(operation)) return Reject("MissingField", "OperationId");
            var rejection = CheckRun(run, math);
            if (rejection != null) return rejection;
            if (run.FinalReport != null || run.CurrentSnapshot.Phase == BattlePhase.WonPendingSettlement || run.CurrentSnapshot.Phase == BattlePhase.Closed)
                return Reject("InvalidPhase", "Run.CurrentSnapshot.Phase");
            foreach (var record in run.Records)
                if (Same(operation, record.OperationId)) return Reject("OperationConflict", "OperationId");
            return null;
        }

        private static CandidateBattleResult Finish(CandidateBattleRun run, CandidateBattleOperationRecord record, ExactMathBudget math)
        {
            var records = new List<CandidateBattleOperationRecord>(run.Records) { record };
            CandidateFinalAttemptReport report = null;
            if (record.AfterSnapshot.Phase == BattlePhase.WonPendingSettlement)
            {
                var rejection = Terminal(record.AfterSnapshot);
                if (rejection != null) return rejection;
                var segments = new List<CandidateContributionSegment>();
                foreach (var row in records) segments.AddRange(row.ContributionSegments);
                rejection = SumContributions(run.Baseline, segments, record.AfterSnapshot, math);
                if (rejection != null) return rejection;
                var wholeHp = ExactRational.Create(0, 1, math);
                foreach (var face in run.Baseline.Entry.Level.Faces)
                    foreach (var pair in face.Pairs) wholeHp = wholeHp.Add(pair.Enemy.Stats.MaxHp, math);
                report = new CandidateFinalAttemptReport(run.Binding, run.Baseline, run.InitialSnapshot, records,
                    record.AfterSnapshot, segments, CandidateBattleOutcome.NormalVictory, record.OccurredAtUnixMilliseconds,
                    record.OperationId, CandidateConsumptionCoverage.EmptyCarryNoUse, wholeHp, null);
                var fingerprint = Compute(report, math);
                report = new CandidateFinalAttemptReport(run.Binding, run.Baseline, run.InitialSnapshot, records,
                    record.AfterSnapshot, segments, report.Outcome, report.EndedAtUnixMilliseconds, report.TerminalOperationId,
                    report.ConsumptionCoverage, wholeHp, fingerprint);
            }
            return new CandidateBattleResult(new CandidateBattleRun(run.Binding, run.Baseline, run.InitialSnapshot,
                record.AfterSnapshot, records, report), record);
        }

        // Also usable by internal stage integration tests. It never imports history or closes a report.
        internal static CandidateBattleOperationRecord AssembleRecord(CandidateBattleOperationKind kind, BigInteger time,
            BattleSnapshot before, CandidateBattleConditionValues conditions, CandidateCombatFrame direct,
            CandidateEnemyPhaseFrame enemy, CandidateStageDecision stage, ExactMathBudget math)
        {
            var after = AssembleSnapshot(before, enemy, stage, math);
            var facts = new List<CandidateBattleOrderedFact>();
            var contributions = new List<CandidateContributionSegment>();
            if (direct != null)
                foreach (var hit in direct.DamageFacts)
                {
                    contributions.Add(new CandidateContributionSegment(hit.OperationId, hit.SceneRevision, hit.FaceId,
                        CandidateBattleFactKind.DirectAttack, hit.EffectIndex, hit.Actor, hit.Target, hit.Actor,
                        CandidateContributionKind.DamageDealtHp, hit.HpLoss, facts.Count));
                    facts.Add(new CandidateBattleOrderedFact(facts.Count, hit));
                }
            if (enemy != null)
                foreach (var intent in enemy.OrderedIntents)
                {
                    if (intent.Damage != null)
                        contributions.Add(new CandidateContributionSegment(intent.OperationId, intent.SceneRevision, intent.FaceId,
                            CandidateBattleFactKind.EnemyIntent, intent.SegmentIndex, intent.Damage.ActorEnemy, intent.Damage.TargetMember,
                            intent.Damage.TargetMember, CandidateContributionKind.DamageTakenHp, intent.Damage.HpLoss, facts.Count));
                    facts.Add(new CandidateBattleOrderedFact(facts.Count, intent));
                }
            foreach (var fact in stage.OrderedFacts) facts.Add(new CandidateBattleOrderedFact(facts.Count, fact));
            return new CandidateBattleOperationRecord(kind, time, before, after, conditions, direct, enemy, stage, facts, contributions);
        }

        private static BattleSnapshot AssembleSnapshot(BattleSnapshot before, CandidateEnemyPhaseFrame enemy,
            CandidateStageDecision stage, ExactMathBudget math)
        {
            var enemies = enemy == null ? before.Enemies : enemy.Enemies;
            if (stage.DidFlipFace)
            {
                var next = new List<BattleEnemyState>();
                foreach (var pair in stage.NextFace.Pairs)
                    next.Add(new BattleEnemyState(BattleCombatantKey.ForEnemy(before.Baseline.Entry.AttemptId,
                        stage.NextFace.FaceId, pair.Enemy.EnemyInstanceKey), BattlePairKey.Create(before.Baseline.Entry.AttemptId,
                        stage.NextFace.FaceId, pair.PairId), pair.Enemy));
                enemies = next.AsReadOnly();
            }
            var increment = enemy == null ? BigInteger.Zero : BigInteger.One;
            return new BattleSnapshot(before.Baseline, math.Add(before.SceneRevision, BigInteger.One),
                math.Add(before.EffectiveActionsCompleted, increment), math.Add(before.EnemyPhasesCompleted, increment),
                stage.NextFaceIndex, stage.NextPhase, stage.Board, enemy == null ? before.Members : enemy.Members,
                enemies, enemy == null ? before.Random : enemy.Random, enemy == null ? before.Contributions : enemy.Contributions);
        }

        private static CandidateBattleResult CheckRun(CandidateBattleRun run, ExactMathBudget math)
        {
            if (run.Binding == null) return Reject("MissingField", "Run.Binding");
            var rejection = CheckBinding(run.Binding, math);
            if (rejection != null) return rejection;
            if (!ReferenceEquals(run.Baseline, run.Binding.Start.Baseline) || !ReferenceEquals(run.InitialSnapshot, run.Binding.Start.Snapshot))
                return Reject("InconsistentBinding", "Run.InitialSnapshot");
            rejection = SnapshotReferences(run.CurrentSnapshot, run.Baseline, "Run.CurrentSnapshot", math);
            if (rejection != null) return rejection;
            var previous = run.InitialSnapshot; var operations = new HashSet<string>(StringComparer.Ordinal);
            for (var i = 0; i < run.Records.Count; i++)
            {
                var record = run.Records[i]; var path = $"Run.Records[{i}]";
                if (record == null || record.StageDecision?.Source == null || record.BeforeSnapshot == null || record.AfterSnapshot == null)
                    return Reject("IncompleteHistory", path);
                if (previous.Phase == BattlePhase.WonPendingSettlement || previous.Phase == BattlePhase.Closed)
                    return Reject("IncompleteHistory", path + ".BeforeSnapshot.Phase");
                rejection = SnapshotReferences(record.BeforeSnapshot, run.Baseline, path + ".BeforeSnapshot", math)
                    ?? SnapshotReferences(record.AfterSnapshot, run.Baseline, path + ".AfterSnapshot", math);
                if (rejection != null) return rejection;
                if (math.Compare(record.BeforeSnapshot.SceneRevision, previous.SceneRevision) < 0 ||
                    !Equal(previous, record.BeforeSnapshot, math, true)) return Reject("IncompleteHistory", path + ".BeforeSnapshot");
                if (string.IsNullOrWhiteSpace(record.OperationId) || !operations.Add(record.OperationId)) return Reject("OperationConflict", path + ".OperationId");
                rejection = CheckRecord(run.Binding, record, path, math);
                if (rejection != null) return rejection;
                previous = record.AfterSnapshot;
            }
            if (math.Compare(run.CurrentSnapshot.SceneRevision, previous.SceneRevision) < 0 ||
                !Equal(previous, run.CurrentSnapshot, math, true)) return Reject("IncompleteHistory", "Run.CurrentSnapshot");
            if (run.FinalReport != null) Check(run.FinalReport, math);
            if (run.CurrentSnapshot.Phase == BattlePhase.WonPendingSettlement && run.FinalReport == null)
                return Reject("IncompleteHistory", "Run.FinalReport");
            return null;
        }

        private static CandidateBattleResult CheckRecord(CandidateRandomBinding binding, CandidateBattleOperationRecord record,
            string path, ExactMathBudget math)
        {
            var before = record.BeforeSnapshot; var source = record.Request;
            if (!ReferenceEquals(record.StageDecision.BeforeSnapshot, before) || record.OccurredAtUnixMilliseconds.Sign < 0 ||
                record.ConsumptionCoverage != CandidateConsumptionCoverage.EmptyCarryNoUse) return Reject("IncompleteHistory", path);
            if (record.StageDecision.NextPhase < BattlePhase.AwaitAction || record.StageDecision.NextPhase > BattlePhase.Closed)
                return Reject("UnsupportedBinding", path + ".StageDecision.NextPhase");
            if (record.Kind != CandidateBattleOperationKind.Attack && record.Kind != CandidateBattleOperationKind.Link)
                return Reject("UnsupportedBinding", path + ".Kind");
            CandidateStageResult checkedStage;
            if (record.Kind == CandidateBattleOperationKind.Attack)
            {
                if (record.Conditions == null || record.Conditions.PreferenceRevision.Sign <= 0 || record.DirectAttack == null || record.EnemyPhase == null)
                    return Reject("IncompleteHistory", path + ".Attack");
                var direct = record.DirectAttack;
                if (!ReferenceEquals(direct.Binding, binding) || !ReferenceEquals(direct.BeforeSnapshot, before) ||
                    !ReferenceEquals(record.EnemyPhase.DirectAttack, direct) || source.Kind != CandidateStageOperation.AfterAttack || direct.Action == null)
                    return Reject("InconsistentBinding", path + ".DirectAttack");
                var fragmentState = new BattleSnapshot(before.Baseline, before.SceneRevision, before.EffectiveActionsCompleted,
                    before.EnemyPhasesCompleted, before.CurrentFaceIndex, before.Phase, before.Board, record.EnemyPhase.Members,
                    record.EnemyPhase.Enemies, record.EnemyPhase.Random, record.EnemyPhase.Contributions);
                var fragmentRejection = SnapshotReferences(fragmentState, before.Baseline, path + ".EnemyPhase", math);
                if (fragmentRejection != null) return fragmentRejection;
                foreach (var intent in record.EnemyPhase.OrderedIntents)
                {
                    if (intent == null) return Reject("IncompleteHistory", path + ".EnemyPhase.OrderedIntents");
                    if (!ReferenceEquals(intent.Baseline, before.Baseline)) return Reject("InconsistentBinding", path + ".EnemyPhase.Baseline");
                    if (intent.IntentKind != EnemyIntentKind.Charge && intent.IntentKind != EnemyIntentKind.Strike)
                        return Reject("UnsupportedBinding", path + ".EnemyPhase.IntentKind");
                }
                var action = direct.Action;
                var expectedSource = new CandidateStageSource(CandidateStageOperation.AfterAttack, action.PlayerId, action.AttemptId,
                    action.OperationId, action.ExpectedSceneRevision, action.Actor, action.Pair, action.Route);
                if (!Equal(source, expectedSource, math)) return Reject("IncompleteHistory", path + ".Request");
                // Inspect retained history without rerunning 009 or drawing any old random opportunity.
                // The deterministic segment owners verify their own results; 012 does not duplicate 010/011 rules.
                var checkedEnemy = CandidateEnemyPhase.Evaluate(direct, math);
                if (!checkedEnemy.IsAccepted) return From(checkedEnemy);
                if (!Equal(checkedEnemy.Frame, record.EnemyPhase, math)) return Reject("IncompleteHistory", path + ".EnemyPhase");
                var rejection = DirectArithmetic(direct, math);
                if (rejection != null) return rejection;
                checkedStage = CandidateBattleStage.AfterAttack(before, Attack(source), Projection(record.EnemyPhase), math);
            }
            else
            {
                if (record.Conditions != null || record.DirectAttack != null || record.EnemyPhase != null || source.Actor != null ||
                    source.Kind != CandidateStageOperation.CompleteLink) return Reject("IncompleteHistory", path + ".Link");
                checkedStage = CandidateBattleStage.CompleteLink(before, Link(source), math);
            }
            foreach (var fact in record.StageDecision.OrderedFacts)
                if (fact == null || fact.Kind < CandidateStageFactKind.TemporaryRouteRemoved || fact.Kind > CandidateStageFactKind.PhaseSelected)
                    return Reject("UnsupportedBinding", path + ".StageDecision.OrderedFacts");
            foreach (var segment in record.ContributionSegments)
                if (segment == null || segment.HpLoss == null ||
                    (segment.RuleSegment != CandidateBattleFactKind.DirectAttack && segment.RuleSegment != CandidateBattleFactKind.EnemyIntent) ||
                    (segment.Kind != CandidateContributionKind.DamageDealtHp && segment.Kind != CandidateContributionKind.DamageTakenHp))
                    return Reject("UnsupportedBinding", path + ".ContributionSegments");
            if (!checkedStage.IsAccepted) return From(checkedStage);
            if (!ReferenceEquals(checkedStage.Decision.Board.Face, record.StageDecision.Board?.Face) ||
                !ReferenceEquals(checkedStage.Decision.NextFace, record.StageDecision.NextFace) ||
                !Equal(checkedStage.Decision, record.StageDecision, math)) return Reject("IncompleteHistory", path + ".StageDecision");
            Check(record, math);
            var expected = AssembleRecord(record.Kind, record.OccurredAtUnixMilliseconds, before, record.Conditions,
                record.DirectAttack, record.EnemyPhase, record.StageDecision, math);
            if (!Equal(expected.AfterSnapshot, record.AfterSnapshot, math)) return Reject("IncompleteHistory", path + ".AfterSnapshot");
            if (!Equal(expected.ContributionSegments, record.ContributionSegments, math)) return Reject("IncompleteHistory", path + ".ContributionSegments");
            if (record.OrderedFacts.Count != expected.OrderedFacts.Count) return Reject("IncompleteHistory", path + ".OrderedFacts");
            for (var i = 0; i < expected.OrderedFacts.Count; i++)
            {
                var a = expected.OrderedFacts[i]; var b = record.OrderedFacts[i];
                if (b == null || b.Index != i || b.Kind != a.Kind || !ReferenceEquals(a.DirectAttack, b.DirectAttack) ||
                    !ReferenceEquals(a.EnemyIntent, b.EnemyIntent) || !ReferenceEquals(a.Stage, b.Stage))
                    return Reject("IncompleteHistory", path + ".OrderedFacts");
                math.CheckInteger(b.Index);
            }
            return null;
        }

        private static CandidateBattleResult DirectArithmetic(CandidateCombatFrame frame, ExactMathBudget math)
        {
            var hit = frame.DamageFacts[0]; var crit = hit.Crit; BattleMemberState actor = null;
            foreach (var member in frame.Members) if (member.CombatantKey.Equals(hit.Actor)) actor = member;
            if (actor == null) return Reject("IncompleteHistory", "DirectAttack.Actor");
            var before = frame.BeforeSnapshot; BattleEnemyState target = null; var ordinal = 1;
            foreach (var row in before.Enemies) if (row.CombatantKey.Equals(hit.Target)) target = row;
            foreach (var row in before.Enemies) if (row.Hp.Numerator.Sign > 0 && row.OriginalSlot < target.OriginalSlot) ordinal++;
            if (ordinal > actor.Member.Stats.AttackRange) return Reject("IncompleteHistory", "DirectAttack.Range");
            var one = ExactRational.Create(1, 1, math); var hundred = ExactRational.Create(100, 1, math);
            var multiplier = crit.Triggered ? actor.Member.Crit.Multiplier : one;
            var raw = actor.Member.Stats.Attack.Multiply(multiplier, math);
            var mitigated = raw.Multiply(hundred, math).Divide(hundred.Add(target.Enemy.Stats.PhysicalDefense, math), math);
            var rounded = mitigated.Floor(math); var incoming = ExactRational.Create(rounded, 1, math);
            var loss = incoming.Compare(target.Hp, math) < 0 ? incoming : target.Hp;
            if (!Equal(hit.Attack, actor.Member.Stats.Attack, math) || !Equal(hit.PhysicalDefense, target.Enemy.Stats.PhysicalDefense, math) ||
                !Equal(hit.Multiplier, multiplier, math) || !Equal(hit.RawDamage, raw, math) || !Equal(hit.MitigatedDamage, mitigated, math) ||
                math.Compare(hit.RoundedDamage, rounded) != 0 || !Equal(hit.HpLoss, loss, math)) return Reject("IncompleteHistory", "DirectAttack.Damage");
            var failures = math.Add(crit.FailureCountBefore, BigInteger.One);
            var p = actor.Member.Crit.C.Multiply(ExactRational.Create(failures, 1, math), math);
            if (p.Compare(one, math) > 0) p = one;
            if (!Equal(crit.Probability, p, math) || math.Compare(crit.FailureCountAfter, crit.Triggered ? BigInteger.Zero : failures) != 0 ||
                math.Compare(crit.StreamAfter.WordsConsumed, math.Add(crit.StreamBefore.WordsConsumed, crit.Words.Count)) != 0 ||
                (p.Compare(one, math) == 0 ? !crit.Triggered || crit.Words.Count != 0 : crit.Words.Count == 0))
                return Reject("IncompleteHistory", "DirectAttack.Crit");
            return null;
        }

        private static CandidateBattleResult CheckBinding(CandidateRandomBinding binding, ExactMathBudget math)
        {
            if (binding.Start?.Baseline?.Entry == null || binding.Start.Snapshot == null) return Reject("MissingField", "Binding.Start");
            if (string.IsNullOrWhiteSpace(binding.SourceCapabilityId)) return Reject("MissingField", "Binding.SourceCapabilityId");
            if (!Same(binding.MappingId, CandidateRandomPreparer.SupportedMappingId)) return Reject("UnsupportedBinding", "Binding.MappingId");
            var baseline = binding.Start.Baseline; var entry = baseline.Entry;
            // Reuse preparation's support/identity/parameter rules without replacing the retained entry.
            var preparer = new BattleEntryPreparer();
            var prepared = entry.Context is PreparedPublishedRuleContext
                ? preparer.PreparePublished(Input(entry), entry.GetDefinitionBinding(), entry.Level, math)
                : preparer.PrepareCandidate(Input(entry), math);
            if (!prepared.IsAccepted) return Reject(prepared.RejectionCode == BattleEntryRejectionCode.UnsupportedBinding
                ? "UnsupportedBinding" : "InconsistentBinding", "Binding.Start.Baseline.Entry." + prepared.FieldPath);
            if (entry.Members.Count < 1 || entry.Members.Count > (entry.Context is PreparedPublishedRuleContext ? 3 : 1) ||
                entry.Members.Count != entry.ReadyParticipants.Count) return Reject("UnsupportedBinding", "Binding.Start.Baseline.Entry.Members");
            for (var i = 0; i < entry.Members.Count; i++) if (!ReferenceEquals(entry.Members[i], entry.ReadyParticipants[i]))
                return Reject("UnsupportedBinding", "Binding.Start.Baseline.Entry.Members");
            if (baseline.RandomInitials == null) return Reject("MissingField", "Binding.Start.Baseline.RandomInitials");
            var domains = new[] { binding.Battle, binding.BaseReward, binding.Bonus };
            var streams = new[] { baseline.RandomInitials.Battle, baseline.RandomInitials.BaseReward, baseline.RandomInitials.Bonus };
            for (var i = 0; i < domains.Length; i++)
            {
                var domain = domains[i];
                if (domain?.Initial == null) return Reject("MissingField", "Binding.RandomDomains");
                if (domain.Purpose != (CandidateRandomPurpose)i || domain.InitSequence > 0x7fffffffffffffffUL || !ReferenceEquals(domain.Initial, streams[i]))
                    return Reject("InconsistentBinding", "Binding.RandomDomains");
                Check(domain, math);
                if (!Equal(domain.Initial, Pcg32StreamState.Initialize(domain.InitState, domain.InitSequence), math))
                    return Reject("InconsistentBinding", "Binding.RandomDomains.Initial");
            }
            if (baseline.PrdInitialStates.Count != entry.Members.Count) return Reject("InconsistentBinding", "Binding.Start.Baseline.PrdInitialStates");
            for (var i = 0; i < entry.Members.Count; i++)
            {
                var key = BattleCombatantKey.ForParticipant(entry.AttemptId, entry.Members[i].CharacterId); var prd = baseline.PrdInitialStates[i];
                if (prd == null || !key.Equals(prd.CombatantKey) || !ReferenceEquals(entry.Members[i].Crit, prd.Crit) ||
                    !prd.FailureCount.IsZero) return Reject("InconsistentBinding", "Binding.Start.Baseline.PrdInitialStates");
            }
            Check(baseline, math);
            var rejection = SnapshotReferences(binding.Start.Snapshot, baseline, "Binding.Start.Snapshot", math);
            if (rejection != null) return rejection;
            var initial = new BattleSnapshot(baseline, ExactRational.Create(0, 1, math));
            if (!Equal(initial, binding.Start.Snapshot, math)) return Reject("InconsistentBinding", "Binding.Start.Snapshot");
            return null;
        }

        private static CandidateBattleResult SnapshotReferences(BattleSnapshot state, BattleEntryBaseline baseline, string path, ExactMathBudget math)
        {
            if (state == null || state.Board?.Face == null || state.Random?.Stream == null) return Reject("MissingField", path);
            if (!ReferenceEquals(state.Baseline, baseline)) return Reject("InconsistentBinding", path + ".Baseline");
            var entry = baseline.Entry;
            if (state.CurrentFaceIndex < 0 || state.CurrentFaceIndex >= entry.Level.Faces.Count) return Reject("InvalidValue", path + ".CurrentFaceIndex");
            var face = entry.Level.Faces[state.CurrentFaceIndex];
            if (!ReferenceEquals(state.Board.Face, face)) return Reject("InconsistentBinding", path + ".Board.Face");
            if (state.CarryMode != EntryCarryMode.Empty) return Reject("UnsupportedBinding", path + ".CarryMode");
            if (state.Phase < BattlePhase.AwaitAction || state.Phase > BattlePhase.Closed) return Reject("InvalidPhase", path + ".Phase");
            if (state.SceneRevision.Sign <= 0 || state.EffectiveActionsCompleted.Sign < 0 || state.EnemyPhasesCompleted.Sign < 0)
                return Reject("InvalidValue", path + ".Counters");
            if (state.Members.Count != entry.ReadyParticipants.Count) return Reject("InconsistentBinding", path + ".Members");
            for (var i = 0; i < state.Members.Count; i++)
                if (state.Members[i]?.Hp == null || !ReferenceEquals(state.Members[i].Member, entry.ReadyParticipants[i]) ||
                    !BattleCombatantKey.ForParticipant(entry.AttemptId, entry.ReadyParticipants[i].CharacterId).Equals(state.Members[i].CombatantKey))
                    return Reject("InconsistentBinding", path + ".Members");
            if (state.Enemies.Count != face.Pairs.Count) return Reject("InconsistentBinding", path + ".Enemies");
            var seen = new HashSet<BattleCombatantKey>();
            foreach (var enemy in state.Enemies)
            {
                if (enemy?.Hp == null || enemy.CombatantKey == null || enemy.PairKey == null) return Reject("MissingField", path + ".Enemies");
                PreparedPair original = null;
                foreach (var pair in face.Pairs)
                    if (BattlePairKey.Create(entry.AttemptId, face.FaceId, pair.PairId).Equals(enemy.PairKey)) original = pair;
                if (original == null || !ReferenceEquals(original.Enemy, enemy.Enemy) || !seen.Add(enemy.CombatantKey) ||
                    !BattleCombatantKey.ForEnemy(entry.AttemptId, face.FaceId, original.Enemy.EnemyInstanceKey).Equals(enemy.CombatantKey))
                    return Reject("InconsistentBinding", path + ".Enemies");
            }
            if (state.Contributions.Count != state.Members.Count || state.Random.PrdStates.Count != state.Members.Count)
                return Reject("InconsistentBinding", path + ".Contributions");
            for (var i = 0; i < state.Members.Count; i++)
            {
                var total = state.Contributions[i]; var prd = state.Random.PrdStates[i]; var member = state.Members[i];
                if (total == null || !member.CombatantKey.Equals(total.CombatantKey) || total.EffectiveDamageDealtHp == null || total.EffectiveDamageTakenHp == null)
                    return Reject("InconsistentBinding", path + ".Contributions");
                if (prd == null || !member.CombatantKey.Equals(prd.CombatantKey) || !ReferenceEquals(member.Member.Crit, prd.Crit))
                    return Reject("InconsistentBinding", path + ".Random");
            }
            foreach (var route in state.Board.LockedRoutes)
                if (route?.PairKey == null) return Reject("MissingField", path + ".Board.LockedRoutes");
            foreach (var key in state.Board.PendingLinks) if (key == null) return Reject("MissingField", path + ".Board.PendingLinks");
            Check(state, math);
            return null;
        }

        private static CandidateBattleResult Terminal(BattleSnapshot state)
        {
            if (state.CurrentFaceIndex != state.Baseline.Entry.Level.Faces.Count - 1 || state.Board.PendingLinks.Count != 0 ||
                state.Board.LockedRoutes.Count != state.Board.Face.Pairs.Count) return Reject("IncompleteHistory", "FinalSnapshot.Board");
            foreach (var enemy in state.Enemies) if (!enemy.Hp.Numerator.IsZero) return Reject("IncompleteHistory", "FinalSnapshot.Enemies");
            return null;
        }

        private static CandidateBattleResult SumContributions(BattleEntryBaseline baseline, IEnumerable<CandidateContributionSegment> segments,
            BattleSnapshot final, ExactMathBudget math)
        {
            var dealt = new Dictionary<BattleCombatantKey, ExactRational>(); var taken = new Dictionary<BattleCombatantKey, ExactRational>();
            foreach (var member in baseline.Entry.ReadyParticipants)
            {
                var key = BattleCombatantKey.ForParticipant(baseline.Entry.AttemptId, member.CharacterId);
                dealt.Add(key, ExactRational.Create(0, 1, math)); taken.Add(key, ExactRational.Create(0, 1, math));
            }
            foreach (var segment in segments)
            {
                var key = segment.Beneficiary;
                if (key == null || !dealt.ContainsKey(key) || segment.HpLoss.Numerator.Sign < 0) return Reject("IncompleteHistory", "Contributions.Beneficiary");
                if (segment.Kind == CandidateContributionKind.DamageDealtHp) dealt[key] = dealt[key].Add(segment.HpLoss, math);
                else if (segment.Kind == CandidateContributionKind.DamageTakenHp) taken[key] = taken[key].Add(segment.HpLoss, math);
                else return Reject("UnsupportedBinding", "Contributions.Kind");
            }
            var seen = new HashSet<BattleCombatantKey>();
            foreach (var total in final.Contributions)
                if (!seen.Add(total.CombatantKey) || !dealt.ContainsKey(total.CombatantKey) ||
                    !Equal(dealt[total.CombatantKey], total.EffectiveDamageDealtHp, math) || !Equal(taken[total.CombatantKey], total.EffectiveDamageTakenHp, math))
                    return Reject("IncompleteHistory", "FinalSnapshot.Contributions");
            if (seen.Count != dealt.Count) return Reject("IncompleteHistory", "FinalSnapshot.Contributions");
            return null;
        }

        private static CandidateFinalHpProjection Projection(CandidateEnemyPhaseFrame frame)
        {
            var projection = new CandidateFinalHpProjection { MemberHp = new List<CandidateHpInput>(), EnemyHp = new List<CandidateHpInput>() };
            foreach (var row in frame.Members) projection.MemberHp.Add(new CandidateHpInput { CombatantKey = row.CombatantKey, Hp = row.Hp });
            foreach (var row in frame.Enemies) projection.EnemyHp.Add(new CandidateHpInput { CombatantKey = row.CombatantKey, Hp = row.Hp });
            return projection;
        }
        private static CandidateAttackRequest Attack(CandidateStageSource s)
        { return new CandidateAttackRequest { PlayerId = s.PlayerId, AttemptId = s.AttemptId, OperationId = s.OperationId,
            ExpectedSceneRevision = s.ExpectedSceneRevision, Actor = s.Actor, Pair = s.Pair, Route = new List<FlowPos>(s.Route) }; }
        private static CandidateLinkRequest Link(CandidateStageSource s)
        { return new CandidateLinkRequest { PlayerId = s.PlayerId, AttemptId = s.AttemptId, OperationId = s.OperationId,
            ExpectedSceneRevision = s.ExpectedSceneRevision, Pair = s.Pair, Route = new List<FlowPos>(s.Route) }; }
        private static bool Same(string a, string b) { return string.Equals(a, b, StringComparison.Ordinal); }
        private static CandidateBattleResult Reject(string code, string path) { return new CandidateBattleResult(code, path); }
        private static CandidateBattleResult From(CandidateDirectAttackResult r)
        { return new CandidateBattleResult(r.RejectionCode.Value.ToString(), r.FieldPath, CandidateBattleRejectionStage.DirectAttack, r.RouteReasonCode, r.RouteCellIndex); }
        private static CandidateBattleResult From(CandidateEnemyPhaseResult r)
        { return new CandidateBattleResult(r.RejectionCode.Value.ToString(), r.FieldPath, CandidateBattleRejectionStage.EnemyPhase); }
        private static CandidateBattleResult From(CandidateStageResult r)
        { return new CandidateBattleResult(r.RejectionCode.Value.ToString(), r.FieldPath, CandidateBattleRejectionStage.Stage, r.RouteReasonCode, r.RouteCellIndex); }

        private static BattleEntryInput Input(PreparedBattleEntry entry)
        {
            var level = new LevelInput { LevelId = entry.Level.LevelId, LevelVersion = entry.Level.LevelVersion,
                RecommendedLevel = entry.Level.RecommendedLevel, Faces = new List<FaceInput>() };
            foreach (var face in entry.Level.Faces)
            {
                var f = new FaceInput { FaceId = face.FaceId, Width = face.Width, Height = face.Height, Pairs = new List<PairInput>() };
                foreach (var pair in face.Pairs)
                {
                    var e = pair.Enemy; var intents = new List<EnemyIntentInput>();
                    foreach (var intent in e.IntentCycle) intents.Add(new EnemyIntentInput { Kind = intent.Kind, Targeting = intent.Targeting,
                        DamageKind = intent.DamageKind, DamageCoefficient = intent.DamageCoefficient });
                    f.Pairs.Add(new PairInput { PairId = pair.PairId, GeometryColorId = pair.GeometryColorId,
                        EndpointA = pair.EndpointA, EndpointB = pair.EndpointB, Enemy = new EnemyInput { EnemyInstanceKey = e.EnemyInstanceKey,
                            EnemyDefinitionId = e.EnemyDefinitionId, OriginalSlot = e.OriginalSlot, StableOrder = e.StableOrder,
                            Behavior = e.Behavior, Stats = Stats(e.Stats), IntentCycle = intents } });
                }
                level.Faces.Add(f);
            }
            var members = new List<MemberInput>();
            foreach (var m in entry.Members) members.Add(new MemberInput { CharacterId = m.CharacterId, ClassId = m.ClassId,
                ClassKind = m.ClassKind, OriginalSlot = m.OriginalSlot, Level = m.Level, IsReady = m.IsReady, StatsOrigin = m.StatsOrigin,
                StatsContext = Context(m.StatsContext), Stats = Stats(m.Stats), EntryHp = m.EntryHp, LearnedSkills = new List<string>(m.LearnedSkills),
                Crit = new WarriorCritInput { PassiveDefinitionId = m.Crit.PassiveDefinitionId, TargetProbability = m.Crit.TargetProbability,
                    C = m.Crit.C, Multiplier = m.Crit.Multiplier } });
            return new BattleEntryInput { PlayerId = entry.PlayerId, ChallengeId = entry.ChallengeId, AttemptId = entry.AttemptId,
                EntryBaselineId = entry.EntryBaselineId, Context = Context(entry.Context), Level = level, Members = members,
                CarryMode = entry.CarryMode, RequiredFeatures = new List<string>(entry.RequiredFeatures) };
        }
        private static RuleContext Context(PreparedRuleContext c)
        { return RuleContextChecks.Copy(c); }
        private static StatsInput Stats(PreparedStats s)
        { return new StatsInput { MaxHp = s.MaxHp, Attack = s.Attack, PhysicalDefense = s.PhysicalDefense,
            MagicDefense = s.MagicDefense, Evasion = s.Evasion, AttackRange = s.AttackRange }; }
    }
}
