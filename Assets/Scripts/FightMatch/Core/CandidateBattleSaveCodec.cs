using System;
using System.Collections.Generic;
using System.Numerics;
using static FightMatch.Core.BusinessFields;

namespace FightMatch.Core
{
    internal sealed class CandidateBattleSaveCodec
    {
        internal readonly BusinessTable<BattleEntryBaseline> Baselines = new BusinessTable<BattleEntryBaseline>();
        internal readonly BusinessTable<BattleSnapshot> Snapshots = new BusinessTable<BattleSnapshot>();
        internal readonly BusinessTable<CandidateRandomBinding> Bindings = new BusinessTable<CandidateRandomBinding>();
        internal readonly BusinessTable<CandidateBattleOperationRecord> Records = new BusinessTable<CandidateBattleOperationRecord>();
        internal readonly BusinessTable<CandidateFinalAttemptReport> Reports = new BusinessTable<CandidateFinalAttemptReport>();
        internal readonly BusinessTable<CandidateBattleRun> Runs = new BusinessTable<CandidateBattleRun>();
        private readonly List<CandidateRandomBinding> recordBindings = new List<CandidateRandomBinding>();
        private readonly SaveCodecBudget budget;
        private BusinessFields f;
        internal CandidateBattleSaveCodec(SaveCodecBudget budget) { this.budget = budget; }
        internal void Collect(CandidateBusinessSnapshot snapshot)
        {
            var h = snapshot.ActiveHistory;
            if (h != null)
            {
                RunRoot(h.CurrentRun); foreach (var e in h.Archive) { Need(e != null, "M06.Archive", "MissingField"); RecordRoot(e.Record, h.Binding); }
                foreach (var r in h.RollbackRecords) RollbackRoot(r);
            }
            foreach (var r in snapshot.RetainedRuns) RunRoot(r);
            foreach (var r in snapshot.RetainedRollbacks) RollbackRoot(r);
            foreach (var reward in snapshot.Rewards.BaseRewards) { Need(reward != null, "M07.BaseRewards", "MissingField"); ReportRoot(reward.Report); }
        }
        private void BaselineRoot(BattleEntryBaseline x) { Baselines.Add(x, budget, "M06.Baselines"); }
        private void SnapshotRoot(BattleSnapshot x) { if (Snapshots.Add(x, budget, "M06.Snapshots")) BaselineRoot(x.Baseline); }
        private void BindingRoot(CandidateRandomBinding x)
        { if (!Bindings.Add(x, budget, "M06.Bindings")) return; Need(x.Start != null, "M06.Binding.Start", "MissingField"); BaselineRoot(x.Start.Baseline); SnapshotRoot(x.Start.Snapshot); }
        private void RecordRoot(CandidateBattleOperationRecord x, CandidateRandomBinding binding)
        {
            if (!Records.Add(x, budget, "M06.Records"))
            { Need(ReferenceEquals(recordBindings[Records.Index(x)], binding), "M06.Record.Binding", "InconsistentBinding"); return; }
            recordBindings.Add(binding); BindingRoot(binding); SnapshotRoot(x.BeforeSnapshot); SnapshotRoot(x.AfterSnapshot);
        }
        private void ReportRoot(CandidateFinalAttemptReport x)
        {
            if (!Reports.Add(x, budget, "M06.Reports")) return; BindingRoot(x.Binding); BaselineRoot(x.Baseline); SnapshotRoot(x.InitialSnapshot);
            foreach (var r in x.Operations) RecordRoot(r, x.Binding); SnapshotRoot(x.FinalSnapshot);
        }
        private void RunRoot(CandidateBattleRun x)
        {
            if (!Runs.Add(x, budget, "M06.Runs")) return; BindingRoot(x.Binding); BaselineRoot(x.Baseline);
            SnapshotRoot(x.InitialSnapshot); SnapshotRoot(x.CurrentSnapshot); foreach (var r in x.Records) RecordRoot(r, x.Binding);
            if (x.FinalReport != null) ReportRoot(x.FinalReport);
        }
        private void RollbackRoot(CandidateRollbackRecord x)
        {
            Need(x?.Range != null, "M06.Rollback", "MissingField"); RunRoot(x.BeforeRun); BindingRoot(x.Range.Binding);
            SnapshotRoot(x.Range.BeforeSnapshot); foreach (var e in x.Range.Entries) { Need(e != null, "M06.Range.Entries", "MissingField"); RecordRoot(e.Record, x.Range.Binding); }
            RunRoot(x.RestoredRun);
        }
        internal void Body(BusinessFields fields, CandidateBusinessSnapshot source, out CandidateBattleHistory history,
            out List<CandidateBattleRun> retainedRuns, out List<CandidateRollbackRecord> retainedRollbacks)
        {
            f = fields;
            Table(Baselines, Baseline, "M06.Baselines"); Table(Snapshots, Snapshot, "M06.Snapshots"); Table(Bindings, Binding, "M06.Bindings");
            Table(Records, Record, "M06.Records"); Table(Reports, Report, "M06.Reports"); Table(Runs, Run, "M06.Runs");
            history = f.Optional(source?.ActiveHistory, History, "M06.ActiveHistory");
            retainedRuns = f.List(source?.RetainedRuns, (x, p) => Runs.Ref(f, x, p), "M06.RetainedRuns", 4);
            retainedRollbacks = f.List(source?.RetainedRollbacks, Rollback, "M06.RetainedRollbacks");
        }
        private void Table<T>(BusinessTable<T> table, Func<T, string, T> body, string path) where T : class
        {
            var rows = f.List(f.Reading ? null : table.Rows, (x, p) =>
            { var value = body(x, p); if (f.Reading) table.Add(value, budget, p); return value; }, path);
            Need(rows.Count == table.Rows.Count, path, "Malformed");
        }
        private BattleEntryBaseline Baseline(BattleEntryBaseline x, string p)
        {
            f.Required(x, p); var entry = f.Entry(x?.Entry, p + ".Entry");
            f.Required(x?.RandomInitials, p + ".RandomInitials");
            var initials = new CandidateRandomInitials { Battle = f.RandomStream(x?.RandomInitials.Battle, p + ".RandomInitials.Battle"),
                BaseReward = f.RandomStream(x?.RandomInitials.BaseReward, p + ".RandomInitials.BaseReward"), Bonus = f.RandomStream(x?.RandomInitials.Bonus, p + ".RandomInitials.Bonus") };
            var prd = f.List(x?.PrdInitialStates, (v, vp) => Prd(v, entry, vp), p + ".PrdInitialStates");
            Need(initials.Battle.WordsConsumed.IsZero && initials.BaseReward.WordsConsumed.IsZero && initials.Bonus.WordsConsumed.IsZero, p + ".RandomInitials");
            return f.Reading ? new BattleEntryBaseline(entry, initials, prd) : x;
        }
        private BattlePrdState Prd(BattlePrdState x, PreparedBattleEntry entry, string p)
        {
            f.Required(x, p); var key = f.Combatant(x?.CombatantKey, p + ".CombatantKey"); var member = MemberDefinition(entry, key, p);
            var crit = new PreparedWarriorCrit(f.Crit(x?.Crit, p + ".Crit")); Equal(crit, member.Crit, p + ".Crit");
            if (!f.Reading) Need(ReferenceEquals(x.Crit, member.Crit), p + ".Crit", "InconsistentBinding");
            var failures = f.Integer(x?.FailureCount ?? 0, p + ".FailureCount"); return f.Reading ? new BattlePrdState(key, member.Crit, failures) : x;
        }
        private BattleSnapshot Snapshot(BattleSnapshot x, string p)
        {
            f.Required(x, p); var baseline = Baselines.Ref(f, x?.Baseline, p + ".Baseline");
            var revision = f.Integer(x?.SceneRevision ?? 0, p + ".SceneRevision", 1); var actions = f.Integer(x?.EffectiveActionsCompleted ?? 0, p + ".EffectiveActionsCompleted");
            var phases = f.Integer(x?.EnemyPhasesCompleted ?? 0, p + ".EnemyPhasesCompleted"); var face = f.I(x?.CurrentFaceIndex ?? 0, p + ".CurrentFaceIndex");
            var phase = (BattlePhase)f.Enum((int)(x?.Phase ?? 0), 0, 4, p + ".Phase"); f.Enum((int)(x?.CarryMode ?? 0), 1, 1, p + ".CarryMode");
            var board = Board(x?.Board, baseline, p + ".Board"); var members = Members(x?.Members, baseline, p + ".Members");
            var enemies = Enemies(x?.Enemies, baseline, p + ".Enemies"); var random = Random(x?.Random, baseline, p + ".Random");
            var contributions = f.List(x?.Contributions, Totals, p + ".Contributions");
            Need(face >= 0 && face < baseline.Entry.Level.Faces.Count && ReferenceEquals(board.Face, baseline.Entry.Level.Faces[face]), p + ".CurrentFaceIndex", "InconsistentBinding");
            return f.Reading ? new BattleSnapshot(baseline, revision, actions, phases, face, phase, board, members, enemies, random, contributions) : x;
        }
        private CandidateRandomBinding Binding(CandidateRandomBinding x, string p)
        {
            f.Required(x, p); var baseline = Baselines.Ref(f, x?.Start.Baseline, p + ".Baseline"); var initial = Snapshots.Ref(f, x?.Start.Snapshot, p + ".InitialSnapshot");
            var capability = f.Text(x?.SourceCapabilityId, p + ".SourceCapabilityId"); var mapping = f.Text(x?.MappingId, p + ".MappingId");
            var battle = f.Domain(x?.Battle, p + ".Battle"); var reward = f.Domain(x?.BaseReward, p + ".BaseReward"); var bonus = f.Domain(x?.Bonus, p + ".Bonus");
            battle = BoundDomain(battle, baseline.RandomInitials.Battle, p + ".Battle");
            reward = BoundDomain(reward, baseline.RandomInitials.BaseReward, p + ".BaseReward");
            bonus = BoundDomain(bonus, baseline.RandomInitials.Bonus, p + ".Bonus");
            Need(ReferenceEquals(initial.Baseline, baseline), p + ".InitialSnapshot", "InconsistentBinding");
            return f.Reading ? new CandidateRandomBinding(new CandidateBattleStart(baseline, initial), capability, mapping, battle, reward, bonus) : x;
        }
        private CandidateRandomDomain BoundDomain(CandidateRandomDomain domain, Pcg32StreamState original, string p)
        {
            Equal(domain.Initial, original, p + ".Initial");
            if (!f.Reading) Need(ReferenceEquals(domain.Initial, original), p + ".Initial", "InconsistentBinding");
            return f.Reading ? new CandidateRandomDomain(domain.Purpose, domain.InitState, domain.InitSequence, original) : domain;
        }
        private BattleBoardState Board(BattleBoardState x, BattleEntryBaseline baseline, string p)
        {
            f.Required(x, p); var id = f.Text(x?.Face.FaceId, p + ".Face"); var face = FaceDefinition(baseline, id, p + ".Face");
            if (!f.Reading) Need(ReferenceEquals(x.Face, face), p + ".Face", "InconsistentBinding");
            var locked = f.List(x?.LockedRoutes, (route, rp) => { f.Required(route, rp); var pair = f.PairKey(route?.PairKey, rp + ".PairKey");
                var cells = f.Route(route?.Route, rp + ".Route"); return f.Reading ? new BattleLockedRoute(pair, cells) : route; }, p + ".LockedRoutes");
            var pending = f.List(x?.PendingLinks, f.PairKey, p + ".PendingLinks");
            return f.Reading ? new BattleBoardState(face, locked, pending) : x;
        }
        private List<BattleMemberState> Members(IReadOnlyList<BattleMemberState> x, BattleEntryBaseline baseline, string p)
        {
            return f.List(x, (row, path) => { f.Required(row, path); var key = f.Combatant(row?.CombatantKey, path + ".CombatantKey");
                var id = f.Text(row?.Member.CharacterId, path + ".Member"); var member = MemberDefinition(baseline.Entry, key, path + ".Member");
                Need(id == member.CharacterId && (f.Reading || ReferenceEquals(row.Member, member)), path + ".Member", "InconsistentBinding");
                var hp = f.Rational(row?.Hp, path + ".Hp"); return f.Reading ? new BattleMemberState(key, member, hp) : row; }, p);
        }
        private List<BattleEnemyState> Enemies(IReadOnlyList<BattleEnemyState> x, BattleEntryBaseline baseline, string p)
        {
            return f.List(x, (row, path) => { f.Required(row, path); var key = f.Combatant(row?.CombatantKey, path + ".CombatantKey");
                var pair = f.PairKey(row?.PairKey, path + ".PairKey"); var id = f.Text(row?.Enemy.EnemyInstanceKey, path + ".Enemy");
                var enemy = PairDefinition(baseline, pair, path).Enemy;
                Need(key.Equals(BattleCombatantKey.ForEnemy(baseline.Entry.AttemptId, pair.FaceId, id)) && id == enemy.EnemyInstanceKey &&
                    (f.Reading || ReferenceEquals(row.Enemy, enemy)), path + ".Enemy", "InconsistentBinding");
                var hp = f.Rational(row?.Hp, path + ".Hp"); var cursor = f.Integer(row?.IntentCursor ?? 0, path + ".IntentCursor");
                return f.Reading ? new BattleEnemyState(key, pair, enemy, hp, cursor) : row; }, p);
        }
        private BattleRandomSnapshot Random(BattleRandomSnapshot x, BattleEntryBaseline baseline, string p)
        {
            f.Required(x, p); var stream = f.RandomStream(x?.Stream, p + ".Stream");
            if (stream.WordsConsumed.IsZero)
            { Equal(stream, baseline.RandomInitials.Battle, p + ".Stream"); if (f.Reading) stream = baseline.RandomInitials.Battle; }
            var prd = f.List(x?.PrdStates, (v, vp) => Prd(v, baseline.Entry, vp), p + ".PrdStates");
            return f.Reading ? new BattleRandomSnapshot(stream, prd) : x;
        }
        private BattleContributionTotals Totals(BattleContributionTotals x, string p)
        {
            f.Required(x, p); var key = f.Combatant(x?.CombatantKey, p + ".CombatantKey");
            var dealt = f.Rational(x?.EffectiveDamageDealtHp, p + ".Dealt"); var taken = f.Rational(x?.EffectiveDamageTakenHp, p + ".Taken");
            return f.Reading ? new BattleContributionTotals(key, dealt, taken) : x;
        }
        private CandidateBattleOperationRecord Record(CandidateBattleOperationRecord x, string p)
        {
            f.Required(x, p); var binding = Bindings.Ref(f, f.Reading ? null : recordBindings[Records.Index(x)], p + ".Binding");
            var kind = (CandidateBattleOperationKind)f.Enum((int)(x?.Kind ?? 0), 0, 1, p + ".Kind");
            var time = f.Integer(x?.OccurredAtUnixMilliseconds ?? 0, p + ".OccurredAtUnixMilliseconds");
            var before = Snapshots.Ref(f, x?.BeforeSnapshot, p + ".BeforeSnapshot"); var after = Snapshots.Ref(f, x?.AfterSnapshot, p + ".AfterSnapshot");
            var conditions = f.Optional(x?.Conditions, (v, vp) => { f.Required(v, vp); var revision = f.Integer(v?.PreferenceRevision ?? 0, vp + ".PreferenceRevision", 1);
                var enabled = f.Flag(v?.ItemUseEnabled ?? false, vp + ".ItemUseEnabled"); return f.Reading ? new CandidateBattleConditionValues(revision, enabled) : v; }, p + ".Conditions");
            var direct = f.Optional(x?.DirectAttack, (v, vp) => Direct(v, binding, before, after, vp), p + ".DirectAttack");
            var enemy = f.Optional(x?.EnemyPhase, (v, vp) => EnemyPhase(v, direct, vp), p + ".EnemyPhase");
            var stage = Stage(x?.StageDecision, before, p + ".StageDecision");
            var ordered = f.List(x?.OrderedFacts, (v, vp) => Ordered(v, direct, enemy, stage, vp), p + ".OrderedFacts");
            var contributions = f.List(x?.ContributionSegments, Contribution, p + ".ContributionSegments");
            var coverage = (CandidateConsumptionCoverage)f.Enum((int)(x?.ConsumptionCoverage ?? 0), 0, 0, p + ".ConsumptionCoverage");
            Need(ReferenceEquals(before.Baseline, binding.Start.Baseline) && ReferenceEquals(after.Baseline, before.Baseline), p + ".Binding", "InconsistentBinding");
            if (f.Reading) recordBindings.Add(binding);
            return f.Reading ? new CandidateBattleOperationRecord(kind, time, before, after, conditions, direct, enemy, stage, ordered, contributions, coverage) : x;
        }
        private CandidateCombatFrame Direct(CandidateCombatFrame x, CandidateRandomBinding binding, BattleSnapshot before, BattleSnapshot after, string p)
        {
            f.Required(x, p);
            if (!f.Reading) Need(ReferenceEquals(x.Binding, binding) && ReferenceEquals(x.BeforeSnapshot, before), p, "InconsistentBinding");
            var a = x?.Action; f.Required(a, p + ".Action");
            var request = new CandidateAttackRequest { PlayerId = f.Text(a?.PlayerId, p + ".Action.PlayerId"), AttemptId = f.Text(a?.AttemptId, p + ".Action.AttemptId"),
                OperationId = f.Text(a?.OperationId, p + ".Action.OperationId"), ExpectedSceneRevision = f.Integer(a?.ExpectedSceneRevision ?? 0, p + ".Action.ExpectedSceneRevision", 1) };
            var ordinal = f.Integer(a?.ActionOrdinal ?? 0, p + ".Action.ActionOrdinal", 1);
            request.Actor = f.Combatant(a?.Actor, p + ".Action.Actor"); request.Pair = f.PairKey(a?.Pair, p + ".Action.Pair"); request.Route = f.Route(a?.Route, p + ".Action.Route");
            var action = f.Reading ? new CandidateAttackSource(request, ordinal) : a;
            var members = Members(x?.Members, before.Baseline, p + ".Members"); Equal(members, before.Members, p + ".Members");
            var enemies = Enemies(x?.Enemies, before.Baseline, p + ".Enemies"); var random = Random(x?.Random, before.Baseline, p + ".Random");
            Equal(random, after.Random, p + ".Random"); if (f.Reading) random = after.Random;
            else Need(ReferenceEquals(random, after.Random), p + ".Random", "InconsistentBinding");
            var totals = f.List(x?.Contributions, Totals, p + ".Contributions");
            var facts = f.List(x?.DamageFacts, (v, vp) => Damage(v, before, action, random, vp), p + ".DamageFacts");
            Need(facts.Count == 1, p + ".DamageFacts", "UnsupportedBinding");
            return f.Reading ? new CandidateCombatFrame(binding, before, action, enemies, random, totals, facts[0]) : x;
        }
        private CandidateCritFact CritFact(CandidateCritFact x, BattleSnapshot before, BattleRandomSnapshot after, string p)
        {
            f.Required(x, p); var actor = f.Combatant(x?.Actor, p + ".Actor"); var target = f.Combatant(x?.Target, p + ".Target");
            var ordinal = f.Integer(x?.OpportunityOrdinal ?? 0, p + ".OpportunityOrdinal");
            var parameters = new PreparedWarriorCrit(f.Crit(x?.Parameters, p + ".Parameters")); var member = MemberDefinition(before.Baseline.Entry, actor, p);
            Equal(parameters, member.Crit, p + ".Parameters");
            var q = f.Rational(x?.Probability, p + ".Probability"); var old = f.Integer(x?.FailureCountBefore ?? 0, p + ".FailureCountBefore");
            var next = f.Integer(x?.FailureCountAfter ?? 0, p + ".FailureCountAfter"); var triggered = f.Flag(x?.Triggered ?? false, p + ".Triggered");
            var streamBefore = f.RandomStream(x?.StreamBefore, p + ".StreamBefore"); var streamAfter = f.RandomStream(x?.StreamAfter, p + ".StreamAfter");
            Equal(streamBefore, before.Random.Stream, p + ".StreamBefore"); Equal(streamAfter, after.Stream, p + ".StreamAfter");
            if (!f.Reading) Need(ReferenceEquals(streamBefore, before.Random.Stream) && ReferenceEquals(streamAfter, after.Stream), p + ".Streams", "InconsistentBinding");
            else { streamBefore = before.Random.Stream; streamAfter = after.Stream; }
            var words = f.List(x?.Words, (v, vp) => (uint)f.U(v, 4, vp), p + ".Words", 4);
            return f.Reading ? new CandidateCritFact(actor, target, ordinal, member.Crit, new BattlePrdState(actor, member.Crit, old), streamBefore,
                new RandomSample<PrdOutcome>(new PrdOutcome(triggered, next, q), streamAfter, words)) : x;
        }
        private BattleDamageFact Damage(BattleDamageFact x, BattleSnapshot before, CandidateAttackSource action, BattleRandomSnapshot random, string p)
        {
            f.Required(x, p); var baseline = Baselines.Ref(f, x?.Baseline, p + ".Baseline"); Need(ReferenceEquals(baseline, before.Baseline), p + ".Baseline", "InconsistentBinding");
            Identity(x?.PlayerId, action.PlayerId, p + ".PlayerId"); Identity(x?.AttemptId, action.AttemptId, p + ".AttemptId");
            var face = f.Text(x?.FaceId, p + ".FaceId"); Identity(x?.OperationId, action.OperationId, p + ".OperationId");
            Number(x?.SceneRevision ?? 0, before.SceneRevision, p + ".SceneRevision"); Number(x?.ActionOrdinal ?? 0, action.ActionOrdinal, p + ".ActionOrdinal");
            var actor = f.Combatant(x?.Actor, p + ".Actor"); var targetKey = f.Combatant(x?.Target, p + ".Target"); var pair = f.PairKey(x?.Pair, p + ".Pair");
            var target = CandidatePermanentSaveCodec.Find(before.Enemies, e => e.CombatantKey.Equals(targetKey), p + ".Target");
            Need(actor.Equals(action.Actor) && pair.Equals(action.Pair) && pair.Equals(target.PairKey) && face == target.PairKey.FaceId, p + ".Identity", "InconsistentBinding");
            var attack = f.Rational(x?.Attack, p + ".Attack"); Value(x?.PhysicalDefense, target.Enemy.Stats.PhysicalDefense, p + ".PhysicalDefense");
            var multiplier = f.Rational(x?.Multiplier, p + ".Multiplier"); var raw = f.Rational(x?.RawDamage, p + ".RawDamage");
            var mitigated = f.Rational(x?.MitigatedDamage, p + ".MitigatedDamage"); var rounded = f.Integer(x?.RoundedDamage ?? 0, p + ".RoundedDamage");
            var zero = Zero(); Value(x?.BlockPrevented, zero, p + ".BlockPrevented"); Value(x?.ShieldAbsorbed, zero, p + ".ShieldAbsorbed");
            Value(x?.HpBefore, target.Hp, p + ".HpBefore"); var hp = f.Rational(x?.HpAfter, p + ".HpAfter");
            var loss = f.Rational(x?.HpLoss, p + ".HpLoss"); var overflow = f.Rational(x?.Overflow, p + ".Overflow"); var crit = CritFact(x?.Crit, before, random, p + ".Crit");
            return f.Reading ? new BattleDamageFact(before, action, target, attack, multiplier, raw, mitigated, rounded, hp, loss, overflow, zero, crit) : x;
        }
        private CandidateEnemyPhaseFrame EnemyPhase(CandidateEnemyPhaseFrame x, CandidateCombatFrame direct, string p)
        {
            f.Required(x, p); Need(direct != null && (f.Reading || ReferenceEquals(x.DirectAttack, direct)), p + ".DirectAttack", "InconsistentBinding");
            // The direct association is local to this record, and is explicitly identified by its original operation.
            Identity(x?.DirectAttack.Action.OperationId, direct.Action.OperationId, p + ".DirectAttack");
            var members = Members(x?.Members, direct.BeforeSnapshot.Baseline, p + ".Members"); var enemies = Enemies(x?.Enemies, direct.BeforeSnapshot.Baseline, p + ".Enemies");
            var totals = f.List(x?.Contributions, Totals, p + ".Contributions"); var ordinal = f.Integer(x?.EnemyPhaseOrdinal ?? 0, p + ".EnemyPhaseOrdinal", 1);
            var facts = f.List(x?.OrderedIntents, (v, vp) => EnemyFact(v, direct, ordinal, vp), p + ".OrderedIntents");
            return f.Reading ? new CandidateEnemyPhaseFrame(direct, ordinal, members, enemies, totals, facts) : x;
        }
        private CandidateEnemyIntentFact EnemyFact(CandidateEnemyIntentFact x, CandidateCombatFrame direct, BigInteger phase, string p)
        {
            f.Required(x, p); var baseline = Baselines.Ref(f, x?.Baseline, p + ".Baseline"); Need(ReferenceEquals(baseline, direct.BeforeSnapshot.Baseline), p + ".Baseline", "InconsistentBinding");
            Identity(x?.PlayerId, direct.Action.PlayerId, p + ".PlayerId"); Identity(x?.AttemptId, direct.Action.AttemptId, p + ".AttemptId");
            Identity(x?.OperationId, direct.Action.OperationId, p + ".OperationId"); var face = f.Text(x?.FaceId, p + ".FaceId");
            Number(x?.SceneRevision ?? 0, direct.BeforeSnapshot.SceneRevision, p + ".SceneRevision"); Number(x?.ActionOrdinal ?? 0, direct.Action.ActionOrdinal, p + ".ActionOrdinal");
            Number(x?.EnemyPhaseOrdinal ?? 0, phase, p + ".EnemyPhaseOrdinal"); var index = f.I(x?.SegmentIndex ?? 0, p + ".SegmentIndex");
            var key = f.Combatant(x?.EnemyKey, p + ".EnemyKey"); var pair = f.PairKey(x?.Pair, p + ".Pair");
            var enemy = CandidatePermanentSaveCodec.Find(direct.Enemies, e => e.CombatantKey.Equals(key), p + ".EnemyKey");
            Need(pair.Equals(enemy.PairKey) && face == pair.FaceId, p + ".Pair", "InconsistentBinding");
            Need(f.I(x?.StableOrder ?? 0, p + ".StableOrder") == enemy.StableOrder, p + ".StableOrder", "InconsistentBinding");
            var kind = (EnemyIntentKind)f.Enum((int)(x?.IntentKind ?? 0), 1, 2, p + ".IntentKind");
            Number(x?.CursorBefore ?? 0, enemy.IntentCursor, p + ".CursorBefore"); var cursor = f.Integer(x?.CursorAfter ?? 0, p + ".CursorAfter");
            var damage = f.Optional(x?.Damage, (v, vp) => EnemyDamage(v, direct, enemy, vp), p + ".Damage");
            return f.Reading ? new CandidateEnemyIntentFact(direct, phase, index, enemy, kind, cursor, damage) : x;
        }
        private CandidateEnemyDamageFact EnemyDamage(CandidateEnemyDamageFact x, CandidateCombatFrame direct, BattleEnemyState enemy, string p)
        {
            f.Required(x, p); var actor = f.Combatant(x?.ActorEnemy, p + ".ActorEnemy"); var key = f.Combatant(x?.TargetMember, p + ".TargetMember");
            Need(actor.Equals(enemy.CombatantKey), p + ".ActorEnemy", "InconsistentBinding"); var member = MemberDefinition(direct.BeforeSnapshot.Baseline.Entry, key, p + ".TargetMember");
            Value(x?.Attack, enemy.Enemy.Stats.Attack, p + ".Attack"); var coefficient = f.Rational(x?.DamageCoefficient, p + ".DamageCoefficient");
            Value(x?.PhysicalDefense, member.Stats.PhysicalDefense, p + ".PhysicalDefense"); var raw = f.Rational(x?.RawDamage, p + ".RawDamage");
            var mitigated = f.Rational(x?.MitigatedDamage, p + ".MitigatedDamage"); var rounded = f.Integer(x?.RoundedDamage ?? 0, p + ".RoundedDamage");
            var zero = Zero(); Value(x?.BlockPrevented, zero, p + ".BlockPrevented"); Value(x?.ShieldAbsorbed, zero, p + ".ShieldAbsorbed");
            var hpBefore = f.Rational(x?.HpBefore, p + ".HpBefore"); var hp = f.Rational(x?.HpAfter, p + ".HpAfter");
            var loss = f.Rational(x?.HpLoss, p + ".HpLoss"); var overflow = f.Rational(x?.Overflow, p + ".Overflow");
            return f.Reading ? new CandidateEnemyDamageFact(enemy, new BattleMemberState(key, member, hpBefore), coefficient, raw, mitigated, rounded, hp, loss, overflow, zero) : x;
        }
        private CandidateStageDecision Stage(CandidateStageDecision x, BattleSnapshot before, string p)
        {
            f.Required(x, p); if (!f.Reading) Need(ReferenceEquals(x.BeforeSnapshot, before), p + ".BeforeSnapshot", "InconsistentBinding");
            var s = x?.Source; f.Required(s, p + ".Source");
            var kind = (CandidateStageOperation)f.Enum((int)(s?.Kind ?? 0), 0, 1, p + ".Source.Kind");
            var source = new CandidateStageSource(kind, f.Text(s?.PlayerId, p + ".Source.PlayerId"), f.Text(s?.AttemptId, p + ".Source.AttemptId"),
                f.Text(s?.OperationId, p + ".Source.OperationId"), f.Integer(s?.ExpectedSceneRevision ?? 0, p + ".Source.ExpectedSceneRevision", 1),
                f.Optional(s?.Actor, f.Combatant, p + ".Source.Actor"), f.PairKey(s?.Pair, p + ".Source.Pair"), f.Route(s?.Route, p + ".Source.Route"));
            if (!f.Reading) source = s;
            f.Required(x?.FinalHp, p + ".FinalHp");
            var members = f.List(x?.FinalHp.MemberHp, Hp, p + ".FinalHp.MemberHp"); var enemies = f.List(x?.FinalHp.EnemyHp, Hp, p + ".FinalHp.EnemyHp");
            var projection = new CandidateFinalHpProjection { MemberHp = new List<CandidateHpInput>(), EnemyHp = new List<CandidateHpInput>() };
            foreach (var v in members) projection.MemberHp.Add(new CandidateHpInput { CombatantKey = v.CombatantKey, Hp = v.Hp });
            foreach (var v in enemies) projection.EnemyHp.Add(new CandidateHpInput { CombatantKey = v.CombatantKey, Hp = v.Hp });
            var board = Board(x?.Board, before.Baseline, p + ".Board"); var nextIndex = f.I(x?.NextFaceIndex ?? 0, p + ".NextFaceIndex");
            var nextPhase = (BattlePhase)f.Enum((int)(x?.NextPhase ?? 0), 0, 4, p + ".NextPhase");
            var nextId = f.Text(x?.NextFace?.FaceId, p + ".NextFace", true); var nextFace = nextId == null ? null : FaceDefinition(before.Baseline, nextId, p + ".NextFace");
            if (!f.Reading) Need(ReferenceEquals(nextFace, x.NextFace), p + ".NextFace", "InconsistentBinding");
            var facts = f.List(x?.OrderedFacts, (v, vp) => StageFact(v, before, source, vp), p + ".OrderedFacts");
            return f.Reading ? new CandidateStageDecision(before, source, new CandidateFinalHpValues(projection), board, nextIndex, nextPhase, nextFace, facts) : x;
        }
        private CandidateHpValue Hp(CandidateHpValue x, string p)
        { f.Required(x, p); var key = f.Combatant(x?.CombatantKey, p + ".CombatantKey"); var hp = f.Rational(x?.Hp, p + ".Hp"); return f.Reading ? new CandidateHpValue(key, hp) : x; }
        private CandidateStageFact StageFact(CandidateStageFact x, BattleSnapshot before, CandidateStageSource source, string p)
        {
            f.Required(x, p); var kind = (CandidateStageFactKind)f.Enum((int)(x?.Kind ?? 0), 0, 5, p + ".Kind");
            Identity(x?.OperationId, source.OperationId, p + ".OperationId"); Number(x?.SceneRevision ?? 0, before.SceneRevision, p + ".SceneRevision");
            Identity(x?.FaceId, before.Board.Face.FaceId, p + ".FaceId"); var pair = f.Optional(x?.Pair, f.PairKey, p + ".Pair");
            var index = f.I(x?.SegmentIndex ?? 0, p + ".SegmentIndex"); var next = f.Text(x?.NextFaceId, p + ".NextFaceId", true);
            BattlePhase? phase = f.Flag(x?.Phase != null, p + ".Phase") ? (BattlePhase?)f.Enum((int)(x?.Phase ?? 0), 0, 4, p + ".Phase.Value") : null;
            return f.Reading ? new CandidateStageFact(kind, before, source, pair, index, next, phase) : x;
        }
        private CandidateBattleOrderedFact Ordered(CandidateBattleOrderedFact x, CandidateCombatFrame direct, CandidateEnemyPhaseFrame enemy, CandidateStageDecision stage, string p)
        {
            f.Required(x, p); var index = f.I(x?.Index ?? 0, p + ".Index"); var kind = (CandidateBattleFactKind)f.Enum((int)(x?.Kind ?? 0), 0, 2, p + ".Kind");
            if (kind == CandidateBattleFactKind.DirectAttack)
            { Need(direct != null && (f.Reading || x.EnemyIntent == null && x.Stage == null), p); return new CandidateBattleOrderedFact(index, LocalRef(direct.DamageFacts, x?.DirectAttack, p)); }
            if (kind == CandidateBattleFactKind.EnemyIntent)
            { Need(enemy != null && (f.Reading || x.DirectAttack == null && x.Stage == null), p); return new CandidateBattleOrderedFact(index, LocalRef(enemy.OrderedIntents, x?.EnemyIntent, p)); }
            Need(f.Reading || x.DirectAttack == null && x.EnemyIntent == null, p); return new CandidateBattleOrderedFact(index, LocalRef(stage.OrderedFacts, x?.Stage, p));
        }
        private T LocalRef<T>(IReadOnlyList<T> values, T value, string p) where T : class
        {
            var index = -1; if (!f.Reading) for (var i = 0; i < values.Count; i++) if (ReferenceEquals(values[i], value)) { index = i; break; }
            Need(f.Reading || index >= 0, p, "InconsistentBinding"); var id = f.U(unchecked((uint)index), 4, p + ".FactRef");
            Need(id < (ulong)values.Count, p + ".FactRef", "Malformed"); return values[(int)id];
        }
        private CandidateContributionSegment Contribution(CandidateContributionSegment x, string p)
        {
            f.Required(x, p); var operation = f.Text(x?.OperationId, p + ".OperationId"); var revision = f.Integer(x?.SceneRevision ?? 0, p + ".SceneRevision", 1);
            var face = f.Text(x?.FaceId, p + ".FaceId"); var segment = (CandidateBattleFactKind)f.Enum((int)(x?.RuleSegment ?? 0), 0, 1, p + ".RuleSegment");
            var index = f.I(x?.SegmentIndex ?? 0, p + ".SegmentIndex"); var actor = f.Combatant(x?.Actor, p + ".Actor");
            var target = f.Combatant(x?.Target, p + ".Target"); var beneficiary = f.Combatant(x?.Beneficiary, p + ".Beneficiary");
            var kind = (CandidateContributionKind)f.Enum((int)(x?.Kind ?? 0), 0, 1, p + ".Kind"); var loss = f.Rational(x?.HpLoss, p + ".HpLoss"); var fact = f.I(x?.FactIndex ?? 0, p + ".FactIndex");
            return f.Reading ? new CandidateContributionSegment(operation, revision, face, segment, index, actor, target, beneficiary, kind, loss, fact) : x;
        }
        private CandidateFinalAttemptReport Report(CandidateFinalAttemptReport x, string p)
        {
            f.Required(x, p); var binding = Bindings.Ref(f, x?.Binding, p + ".Binding"); var baseline = Baselines.Ref(f, x?.Baseline, p + ".Baseline");
            var initial = Snapshots.Ref(f, x?.InitialSnapshot, p + ".InitialSnapshot"); var records = f.List(x?.Operations, (v, vp) => Records.Ref(f, v, vp), p + ".Operations", 4);
            var final = Snapshots.Ref(f, x?.FinalSnapshot, p + ".FinalSnapshot"); var contributions = f.List(x?.Contributions, Contribution, p + ".Contributions");
            var outcome = (CandidateBattleOutcome)f.Enum((int)(x?.Outcome ?? 0), 0, 0, p + ".Outcome"); var ended = f.Integer(x?.EndedAtUnixMilliseconds ?? 0, p + ".EndedAtUnixMilliseconds");
            var terminal = f.Text(x?.TerminalOperationId, p + ".TerminalOperationId"); var coverage = (CandidateConsumptionCoverage)f.Enum((int)(x?.ConsumptionCoverage ?? 0), 0, 0, p + ".ConsumptionCoverage");
            var hp = f.Rational(x?.WholeLevelInitialEnemyHp, p + ".WholeLevelInitialEnemyHp"); var fingerprint = f.Text(x?.Fingerprint, p + ".Fingerprint");
            return f.Reading ? new CandidateFinalAttemptReport(binding, baseline, initial, records, final, contributions, outcome, ended, terminal, coverage, hp, fingerprint) : x;
        }
        private CandidateBattleRun Run(CandidateBattleRun x, string p)
        {
            f.Required(x, p); var binding = Bindings.Ref(f, x?.Binding, p + ".Binding"); var baseline = Baselines.Ref(f, x?.Baseline, p + ".Baseline");
            var initial = Snapshots.Ref(f, x?.InitialSnapshot, p + ".InitialSnapshot"); var current = Snapshots.Ref(f, x?.CurrentSnapshot, p + ".CurrentSnapshot");
            var records = f.List(x?.Records, (v, vp) => Records.Ref(f, v, vp), p + ".Records", 4); var report = Reports.Ref(f, x?.FinalReport, p + ".FinalReport", true);
            return f.Reading ? new CandidateBattleRun(binding, baseline, initial, current, records, report) : x;
        }
        private CandidateHistoryEntry HistoryEntry(CandidateHistoryEntry x, string p)
        {
            f.Required(x, p); var anchor = f.Text(x?.HistoryAnchorId, p + ".HistoryAnchorId"); var record = Records.Ref(f, x?.Record, p + ".Record");
            var superseded = f.Optional(x?.SupersededBy, (v, vp) => { f.Required(v, vp); var operation = f.Text(v?.OperationId, vp + ".OperationId");
                var revision = f.Integer(v?.SceneRevision ?? 0, vp + ".SceneRevision", 1); return f.Reading ? new CandidateHistorySupersededBy(operation, revision) : v; }, p + ".SupersededBy");
            return f.Reading ? new CandidateHistoryEntry(anchor, record, superseded) : x;
        }
        private CandidateBattleHistory History(CandidateBattleHistory x, string p)
        {
            f.Required(x, p); var run = Runs.Ref(f, x?.CurrentRun, p + ".CurrentRun"); var archive = f.List(x?.Archive, HistoryEntry, p + ".Archive");
            var anchors = f.Strings(x?.EffectiveAnchors, p + ".EffectiveAnchors"); var rollbacks = f.List(x?.RollbackRecords, Rollback, p + ".RollbackRecords");
            return f.Reading ? new CandidateBattleHistory(run, archive, anchors, rollbacks) : x;
        }
        private CandidateRollbackRecord Rollback(CandidateRollbackRecord x, string p)
        {
            f.Required(x, p); var operation = f.Text(x?.OperationId, p + ".OperationId"); var before = Runs.Ref(f, x?.BeforeRun, p + ".BeforeRun");
            var r = x?.Range; f.Required(r, p + ".Range"); var player = f.Text(r?.PlayerId, p + ".Range.PlayerId"); var attempt = f.Text(r?.AttemptId, p + ".Range.AttemptId");
            var binding = Bindings.Ref(f, r?.Binding, p + ".Range.Binding"); var revision = f.Integer(r?.SceneRevision ?? 0, p + ".Range.SceneRevision", 1);
            var anchor = f.Text(r?.HistoryAnchorId, p + ".Range.HistoryAnchorId"); var target = f.Text(r?.OperationId, p + ".Range.OperationId");
            var snapshot = Snapshots.Ref(f, r?.BeforeSnapshot, p + ".Range.BeforeSnapshot"); var entries = f.List(r?.Entries, HistoryEntry, p + ".Range.Entries");
            Need(player == binding.GeneratedForPlayerId && attempt == binding.GeneratedForAttemptId, p + ".Range.Binding", "InconsistentBinding");
            var restored = Runs.Ref(f, x?.RestoredRun, p + ".RestoredRun");
            return f.Reading ? new CandidateRollbackRecord(operation, before, new CandidateRollbackRange(binding, revision, anchor, target, snapshot, entries), restored) : x;
        }
        private void Identity(string value, string expected, string p) { Need(f.Text(value, p) == expected, p, "InconsistentBinding"); }
        private void Number(BigInteger value, BigInteger expected, string p) { Need(f.Integer(value, p) == expected, p, "InconsistentBinding"); }
        private void Value(ExactRational value, ExactRational expected, string p) { Equal(f.Rational(value, p), expected, p); }
        private void Equal(object value, object expected, string p) { Need(CandidateBattleReportFingerprint.Equal(value, expected, budget.Math), p, "InconsistentBinding"); }
        private ExactRational Zero() { return ExactRational.Create(0, 1, budget.Math); }
        internal static PreparedMember MemberDefinition(PreparedBattleEntry entry, BattleCombatantKey key, string p)
        {
            Need(key != null && key.AttemptId == entry.AttemptId && key.Kind == BattleCombatantKind.Participant, p, "InconsistentBinding");
            return CandidatePermanentSaveCodec.Find(entry.ReadyParticipants, m => m.CharacterId == key.CharacterId, p);
        }
        internal static PreparedFace FaceDefinition(BattleEntryBaseline baseline, string id, string p)
        { return CandidatePermanentSaveCodec.Find(baseline.Entry.Level.Faces, face => face.FaceId == id, p); }
        internal static PreparedPair PairDefinition(BattleEntryBaseline baseline, BattlePairKey key, string p)
        {
            Need(key != null && key.AttemptId == baseline.Entry.AttemptId, p, "InconsistentBinding");
            return CandidatePermanentSaveCodec.Find(FaceDefinition(baseline, key.FaceId, p).Pairs, pair => pair.PairId == key.PairId, p);
        }
    }
}
