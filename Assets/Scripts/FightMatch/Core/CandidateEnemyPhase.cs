using System;
using System.Collections.Generic;
using System.Numerics;
using FlowPuzzle.Core;
using static FightMatch.Core.CandidateEnemyPhaseRejectionCode;

namespace FightMatch.Core
{
    public static class CandidateEnemyPhase
    {
        public static CandidateEnemyPhaseResult Evaluate(CandidateCombatFrame directAttack, ExactMathBudget budget)
        {
            if (directAttack == null) throw new ArgumentNullException(nameof(directAttack));
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            var validation = new Validation(directAttack, budget);
            if (!validation.Check()) return validation.Rejection;
            var phase = budget.Add(directAttack.BeforeSnapshot.EnemyPhasesCompleted, BigInteger.One);
            var members = new List<BattleMemberState>(directAttack.Members);
            var enemies = new List<BattleEnemyState>(directAttack.Enemies);
            var totals = new List<BattleContributionTotals>(directAttack.Contributions);
            var order = new List<int>();
            for (var i = 0; i < enemies.Count; i++) order.Add(i);
            order.Sort((a, b) => enemies[a].StableOrder.CompareTo(enemies[b].StableOrder));
            var facts = new List<CandidateEnemyIntentFact>();
            foreach (var index in order)
            {
                var enemy = enemies[index];
                if (enemy.Hp.Numerator.IsZero) continue;
                var targetIndex = -1;
                for (var i = 0; i < members.Count; i++)
                    if (members[i].Hp.Numerator.Sign > 0 &&
                        (targetIndex < 0 || members[i].OriginalSlot < members[targetIndex].OriginalSlot)) targetIndex = i;
                // N010: even Charge remains unprocessed after the last participant goes down.
                if (targetIndex < 0) break;
                var cycle = enemy.Enemy.IntentCycle;
                // Only the bounded remainder is converted; the original cursor stays a BigInteger.
                var intent = cycle[(int)budget.Remainder(enemy.IntentCursor, cycle.Count)];
                var nextCursor = budget.Add(enemy.IntentCursor, BigInteger.One);
                CandidateEnemyDamageFact damage = null;
                if (intent.Kind == EnemyIntentKind.Strike)
                {
                    var target = members[targetIndex];
                    var hundred = ExactRational.Create(100, 1, budget);
                    var zero = ExactRational.Create(0, 1, budget);
                    var raw = enemy.Enemy.Stats.Attack.Multiply(intent.DamageCoefficient, budget);
                    var mitigated = raw.Multiply(hundred, budget)
                        .Divide(hundred.Add(target.Member.Stats.PhysicalDefense, budget), budget);
                    var rounded = mitigated.Floor(budget);
                    var incoming = ExactRational.Create(rounded, 1, budget);
                    var loss = target.Hp.Compare(incoming, budget) < 0 ? target.Hp : incoming;
                    var hpAfter = target.Hp.Subtract(loss, budget);
                    damage = new CandidateEnemyDamageFact(enemy, target, intent.DamageCoefficient,
                        raw, mitigated, rounded, hpAfter, loss, incoming.Subtract(loss, budget), zero);
                    members[targetIndex] = new BattleMemberState(target.CombatantKey, target.Member, hpAfter);
                    for (var i = 0; i < totals.Count; i++)
                        if (totals[i].CombatantKey.Equals(target.CombatantKey))
                            totals[i] = new BattleContributionTotals(target.CombatantKey,
                                totals[i].EffectiveDamageDealtHp, totals[i].EffectiveDamageTakenHp.Add(loss, budget));
                }
                enemies[index] = new BattleEnemyState(enemy.CombatantKey, enemy.PairKey, enemy.Enemy, enemy.Hp, nextCursor);
                facts.Add(new CandidateEnemyIntentFact(directAttack, phase, facts.Count, enemy, intent.Kind, nextCursor, damage));
            }
            return new CandidateEnemyPhaseResult(new CandidateEnemyPhaseFrame(directAttack, phase, members, enemies, totals, facts));
        }

        private sealed class Validation
        {
            private readonly CandidateCombatFrame direct;
            private readonly ExactMathBudget math;
            internal CandidateEnemyPhaseResult Rejection { get; private set; }
            internal Validation(CandidateCombatFrame direct, ExactMathBudget math) { this.direct = direct; this.math = math; }

            internal bool Check()
            {
                var binding = direct.Binding; var before = direct.BeforeSnapshot; var action = direct.Action;
                if (binding == null) return Fail(MissingField, "Binding");
                if (binding.Start == null) return Fail(MissingField, "Binding.Start");
                if (before == null) return Fail(MissingField, "Before");
                if (action == null) return Fail(MissingField, "Action");
                if (!Text(action.PlayerId, "Action.PlayerId") || !Text(action.AttemptId, "Action.AttemptId") ||
                    !Text(action.OperationId, "Action.OperationId")) return false;
                if (action.Actor == null) return Fail(MissingField, "Action.Actor");
                if (action.Pair == null) return Fail(MissingField, "Action.Pair");
                if (direct.DamageFacts.Count != 1) return Fail(InconsistentBinding, "DamageFacts");
                var hit = direct.DamageFacts[0];
                if (hit == null) return Fail(MissingField, "DamageFacts[0]");
                if (hit.Crit == null) return Fail(MissingField, "DamageFacts[0].Crit");
                if (!BaselineNumbers(binding.Start.Baseline, "Binding.Start.Baseline") ||
                    !SnapshotNumbers(binding.Start.Snapshot, "Binding.Start.Snapshot")) return false;
                if (!ReferenceEquals(before.Baseline, binding.Start.Baseline) && !BaselineNumbers(before.Baseline, "Before.Baseline")) return false;
                if (!ReferenceEquals(before, binding.Start.Snapshot) && !SnapshotNumbers(before, "Before")) return false;
                if (!Rows(direct.Members, direct.Enemies, direct.Contributions, "DirectAttack") ||
                    !RandomNumbers(direct.Random, "Random") || !HitNumbers(hit)) return false;
                math.CheckInteger(action.ExpectedSceneRevision); math.CheckInteger(action.ActionOrdinal);
                foreach (var cell in action.Route) CellNumbers(cell);
                var domains = new[] { binding.Battle, binding.BaseReward, binding.Bonus };
                var initials = binding.Start.Baseline.RandomInitials;
                var streams = new[] { initials.Battle, initials.BaseReward, initials.Bonus };
                for (var i = 0; i < domains.Length; i++)
                {
                    var domain = domains[i]; var path = "Binding." + (CandidateRandomPurpose)i;
                    if (domain == null || domain.Initial == null) return Fail(MissingField, path);
                    math.CheckInteger(domain.InitState); math.CheckInteger(domain.InitSequence); domain.Initial.Validate(math);
                    if (domain.Purpose != (CandidateRandomPurpose)i || !ReferenceEquals(domain.Initial, streams[i])) return Fail(InconsistentBinding, path);
                }
                if (!Text(binding.SourceCapabilityId, "Binding.SourceCapabilityId")) return false;
                if (!Same(binding.MappingId, CandidateRandomPreparer.SupportedMappingId)) return Fail(UnsupportedBinding, "Binding.MappingId");
                if (!ReferenceEquals(before.Baseline, binding.Start.Baseline) ||
                    !ReferenceEquals(binding.Start.Snapshot.Baseline, binding.Start.Baseline)) return Fail(InconsistentBinding, "Before.Baseline");
                var entry = before.Baseline.Entry;
                if (!Supported(entry)) return false;
                if (!Same(action.PlayerId, entry.PlayerId)) return Fail(InconsistentBinding, "Action.PlayerId");
                if (!Same(action.AttemptId, entry.AttemptId)) return Fail(InconsistentBinding, "Action.AttemptId");
                if (before.Phase != BattlePhase.AwaitAction) return Fail(InvalidPhase, "Before.Phase");
                if (before.SceneRevision.Sign <= 0 || before.EffectiveActionsCompleted.Sign < 0 || before.EnemyPhasesCompleted.Sign < 0)
                    return Fail(InvalidValue, "Before.Counters");
                if (!SameInteger(action.ExpectedSceneRevision, before.SceneRevision)) return Fail(InconsistentBinding, "Action.ExpectedSceneRevision");
                if (!SameInteger(action.ActionOrdinal, math.Add(before.EffectiveActionsCompleted, BigInteger.One))) return Fail(InconsistentBinding, "Action.ActionOrdinal");
                if (before.CurrentFaceIndex < 0 || before.CurrentFaceIndex >= entry.Level.Faces.Count) return Fail(InvalidValue, "Before.CurrentFaceIndex");
                var face = entry.Level.Faces[before.CurrentFaceIndex];
                if (!ReferenceEquals(before.Board.Face, face)) return Fail(InconsistentBinding, "Before.Board.Face");
                if (before.CarryMode != EntryCarryMode.Empty) return Fail(UnsupportedBinding, "Before.CarryMode");
                if (before.Board.PendingLinks.Count != 0) return Fail(InvalidPhase, "Before.Board.PendingLinks");
                var start = binding.Start.Snapshot;
                if (!SameInteger(start.SceneRevision, BigInteger.One) || !start.EffectiveActionsCompleted.IsZero || !start.EnemyPhasesCompleted.IsZero ||
                    start.CurrentFaceIndex != 0 || start.Phase != BattlePhase.AwaitAction ||
                    !ReferenceEquals(start.Board.Face, entry.Level.Faces[0]) || !ReferenceEquals(start.Random.Stream, initials.Battle) ||
                    start.Board.LockedRoutes.Count != 0 || start.Board.PendingLinks.Count != 0)
                    return Fail(InconsistentBinding, "Binding.Start.Snapshot");
                if (!MemberRows(start.Members, entry, "Binding.Start.Members") ||
                    !EnemyRows(start.Enemies, entry, entry.Level.Faces[0], "Binding.Start.Enemies") ||
                    !Totals(start.Contributions, start.Members, "Binding.Start.Contributions") ||
                    !Prd(start.Random, start.Members, "Binding.Start.Random")) return false;
                for (var i = 0; i < start.Members.Count; i++)
                    if (!Equal(start.Members[i].Hp, entry.ReadyParticipants[i].EntryHp) ||
                        !start.Contributions[i].EffectiveDamageDealtHp.Numerator.IsZero || !start.Contributions[i].EffectiveDamageTakenHp.Numerator.IsZero ||
                        !start.Random.PrdStates[i].FailureCount.IsZero) return Fail(InconsistentBinding, "Binding.Start.Snapshot");
                foreach (var row in start.Enemies)
                    if (!Equal(row.Hp, row.Enemy.Stats.MaxHp) || !row.IntentCursor.IsZero) return Fail(InconsistentBinding, "Binding.Start.Enemies");
                if (!MemberRows(before.Members, entry, "Before.Members") || !MemberRows(direct.Members, entry, "Members") ||
                    !EnemyRows(before.Enemies, entry, face, "Before.Enemies") || !EnemyRows(direct.Enemies, entry, face, "Enemies") ||
                    !Totals(before.Contributions, before.Members, "Before.Contributions") || !Totals(direct.Contributions, direct.Members, "Contributions")) return false;
                var actorIndex = -1;
                for (var i = 0; i < direct.Members.Count; i++) if (action.Actor.Equals(direct.Members[i].CombatantKey)) actorIndex = i;
                if (actorIndex < 0 || direct.Members[actorIndex].Hp.Numerator.IsZero) return Fail(InconsistentBinding, "Action.Actor");
                var actor = direct.Members[actorIndex];
                for (var i = 0; i < direct.Members.Count; i++)
                    if (!Equal(direct.Members[i].Hp, before.Members[i].Hp)) return Fail(InconsistentBinding, "Members[" + i + "].Hp");
                BattleEnemyState target = null;
                foreach (var row in before.Enemies) if (row.PairKey.Equals(action.Pair)) target = row;
                if (target == null || target.Hp.Numerator.IsZero) return Fail(InconsistentBinding, "Action.Pair");
                if (!ReferenceEquals(hit.Baseline, before.Baseline) || !Same(hit.PlayerId, action.PlayerId) ||
                    !Same(hit.AttemptId, action.AttemptId) || !Same(hit.OperationId, action.OperationId) || !Same(hit.FaceId, face.FaceId) ||
                    !SameInteger(hit.SceneRevision, before.SceneRevision) || !SameInteger(hit.ActionOrdinal, action.ActionOrdinal) ||
                    !action.Actor.Equals(hit.Actor) || !target.CombatantKey.Equals(hit.Target) || !action.Pair.Equals(hit.Pair))
                    return Fail(InconsistentBinding, "DamageFacts[0].Source");
                if (!Equal(hit.HpBefore, target.Hp) || hit.HpAfter.Compare(hit.HpBefore, math) > 0 ||
                    !Equal(hit.HpBefore.Subtract(hit.HpAfter, math), hit.HpLoss) ||
                    !Equal(ExactRational.Create(hit.RoundedDamage, 1, math), hit.HpLoss.Add(hit.Overflow, math)) ||
                    !hit.BlockPrevented.Numerator.IsZero || !hit.ShieldAbsorbed.Numerator.IsZero)
                    return Fail(InconsistentBinding, "DamageFacts[0].Hp");
                foreach (var row in direct.Enemies)
                {
                    BattleEnemyState original = null;
                    foreach (var old in before.Enemies) if (old.CombatantKey.Equals(row.CombatantKey)) original = old;
                    if (!SameInteger(row.IntentCursor, original.IntentCursor) || !Equal(row.Hp, row.CombatantKey.Equals(target.CombatantKey) ? hit.HpAfter : original.Hp))
                        return Fail(InconsistentBinding, "Enemies");
                }
                for (var i = 0; i < direct.Contributions.Count; i++)
                {
                    var original = before.Contributions[i]; var dealt = i == actorIndex
                        ? original.EffectiveDamageDealtHp.Add(hit.HpLoss, math) : original.EffectiveDamageDealtHp;
                    if (!Equal(direct.Contributions[i].EffectiveDamageDealtHp, dealt) ||
                        !Equal(direct.Contributions[i].EffectiveDamageTakenHp, original.EffectiveDamageTakenHp)) return Fail(InconsistentBinding, "Contributions");
                }
                var crit = hit.Crit;
                if (!actor.CombatantKey.Equals(crit.Actor) || !target.CombatantKey.Equals(crit.Target) ||
                    !ReferenceEquals(crit.Parameters, actor.Member.Crit) || !SameInteger(crit.OpportunityOrdinal, before.EffectiveActionsCompleted) ||
                    !ReferenceEquals(crit.StreamBefore, before.Random.Stream) || !ReferenceEquals(crit.StreamAfter, direct.Random.Stream))
                    return Fail(InconsistentBinding, "DamageFacts[0].Crit");
                if (!Prd(before.Random, before.Members, "Before.Random") || !Prd(direct.Random, direct.Members, "Random")) return false;
                if (!SameInitial(before.Random.Stream, binding.Battle.Initial) ||
                    !SameInteger(crit.FailureCountBefore, before.Random.PrdStates[actorIndex].FailureCount) ||
                    !SameInteger(crit.FailureCountAfter, direct.Random.PrdStates[actorIndex].FailureCount)) return Fail(InconsistentBinding, "Random");
                for (var i = 0; i < before.Members.Count; i++)
                    if (i != actorIndex && !SameInteger(before.Random.PrdStates[i].FailureCount, direct.Random.PrdStates[i].FailureCount))
                        return Fail(InconsistentBinding, "Random");
                return true;
            }

            private bool Supported(PreparedBattleEntry entry)
            {
                if (entry.CarryMode != EntryCarryMode.Empty || entry.RequiredFeatures.Count != 0) return Fail(UnsupportedBinding, "Baseline.CarryMode");
                if (entry.Members.Count < 1 || entry.Members.Count > (entry.Context is PreparedPublishedRuleContext ? 3 : 1) ||
                    entry.ReadyParticipants.Count != entry.Members.Count) return Fail(UnsupportedBinding, "Baseline.Members");
                var ids = new HashSet<string>(); var classes = new HashSet<string>(); var memberSlots = new HashSet<int>();
                for (var i = 0; i < entry.Members.Count; i++)
                {
                    var member = entry.Members[i]; var p = "Baseline.Members[" + i + "]";
                    if (!ReferenceEquals(member, entry.ReadyParticipants[i]) || member.ClassKind != CharacterClassKind.Warrior ||
                        member.LearnedSkills.Count != 0 || member.StatsOrigin != BaseStatsOrigin.ComputedBaseStats || !member.IsReady)
                        return Fail(UnsupportedBinding, p);
                    if (!ids.Add(member.CharacterId) || !classes.Add(member.ClassId) || !memberSlots.Add(member.OriginalSlot) ||
                        member.OriginalSlot < 0 || member.OriginalSlot > 2) return Fail(InconsistentBinding, p);
                    if (!SameContext(entry.Context, member.StatsContext)) return Fail(InconsistentBinding, p + ".StatsContext");
                }
                foreach (var face in entry.Level.Faces)
                {
                    var orders = new HashSet<int>(); var slots = new HashSet<int>();
                    foreach (var pair in face.Pairs)
                    {
                        var enemy = pair.Enemy; const string path = "Baseline.Enemy.IntentCycle";
                        if (!orders.Add(enemy.StableOrder) || !slots.Add(enemy.OriginalSlot) || enemy.StableOrder < 0 || enemy.OriginalSlot < 0)
                            return Fail(InconsistentBinding, "Baseline.Enemy.Order");
                        if (enemy.Behavior != EnemyBehavior.NormalStrike && enemy.Behavior != EnemyBehavior.ChargeHeavy)
                            return Fail(UnsupportedBinding, "Baseline.Enemy.Behavior");
                        if (enemy.IntentCycle.Count != (enemy.Behavior == EnemyBehavior.NormalStrike ? 1 : 2)) return Fail(UnsupportedBinding, path);
                        for (var i = 0; i < enemy.IntentCycle.Count; i++)
                        {
                            var intent = enemy.IntentCycle[i];
                            var kind = enemy.Behavior == EnemyBehavior.ChargeHeavy && i == 0 ? EnemyIntentKind.Charge : EnemyIntentKind.Strike;
                            if (intent.Kind != kind || intent.Targeting != EnemyTargeting.FirstLiving) return Fail(UnsupportedBinding, path);
                            if (kind == EnemyIntentKind.Charge)
                            { if (intent.DamageKind != null || intent.DamageCoefficient != null) return Fail(UnsupportedBinding, path); }
                            else if (intent.DamageKind != EntryDamageKind.Physical || intent.DamageCoefficient == null || intent.DamageCoefficient.Numerator.Sign <= 0)
                                return Fail(UnsupportedBinding, path);
                        }
                    }
                }
                return true;
            }

            private bool MemberRows(IReadOnlyList<BattleMemberState> rows, PreparedBattleEntry entry, string path)
            {
                if (rows.Count != entry.ReadyParticipants.Count) return Fail(InconsistentBinding, path);
                for (var i = 0; i < rows.Count; i++)
                {
                    if (!ReferenceEquals(rows[i].Member, entry.ReadyParticipants[i]) ||
                        !BattleCombatantKey.ForParticipant(entry.AttemptId, entry.ReadyParticipants[i].CharacterId).Equals(rows[i].CombatantKey))
                        return Fail(InconsistentBinding, path);
                    if (!Hp(rows[i].Hp, rows[i].Member.Stats.MaxHp, path + "[" + i + "].Hp")) return false;
                }
                return true;
            }

            private bool EnemyRows(IReadOnlyList<BattleEnemyState> rows, PreparedBattleEntry entry, PreparedFace face, string path)
            {
                if (rows.Count != face.Pairs.Count) return Fail(InconsistentBinding, path);
                var seen = new HashSet<BattleCombatantKey>();
                for (var i = 0; i < rows.Count; i++)
                {
                    var row = rows[i]; PreparedPair original = null;
                    foreach (var pair in face.Pairs)
                        if (BattlePairKey.Create(entry.AttemptId, face.FaceId, pair.PairId).Equals(row.PairKey)) original = pair;
                    var p = path + $"[{i}]";
                    if (original == null || !ReferenceEquals(row.Enemy, original.Enemy) || !seen.Add(row.CombatantKey) ||
                        !BattleCombatantKey.ForEnemy(entry.AttemptId, face.FaceId, original.Enemy.EnemyInstanceKey).Equals(row.CombatantKey))
                        return Fail(InconsistentBinding, p);
                    if (!Hp(row.Hp, original.Enemy.Stats.MaxHp, p + ".Hp")) return false;
                    if (row.IntentCursor.Sign < 0) return Fail(InvalidValue, p + ".IntentCursor");
                }
                return true;
            }

            private bool Totals(IReadOnlyList<BattleContributionTotals> rows, IReadOnlyList<BattleMemberState> members, string path)
            {
                if (rows.Count != members.Count) return Fail(InconsistentBinding, path);
                for (var i = 0; i < rows.Count; i++)
                {
                    if (!members[i].CombatantKey.Equals(rows[i].CombatantKey)) return Fail(InconsistentBinding, path);
                    if (rows[i].EffectiveDamageDealtHp.Numerator.Sign < 0 || rows[i].EffectiveDamageTakenHp.Numerator.Sign < 0) return Fail(InvalidValue, path);
                }
                return true;
            }

            private bool Prd(BattleRandomSnapshot random, IReadOnlyList<BattleMemberState> members, string path)
            {
                if (random.PrdStates.Count != members.Count) return Fail(InconsistentBinding, path + ".PrdStates");
                for (var i = 0; i < members.Count; i++)
                {
                    var member = members[i]; var prd = random.PrdStates[i];
                    if (!member.CombatantKey.Equals(prd.CombatantKey) || !ReferenceEquals(member.Member.Crit, prd.Crit))
                        return Fail(InconsistentBinding, path + ".PrdStates");
                    var c = member.Member.Crit.C;
                    var limit = math.DivRem(c.Denominator, c.Numerator, out var remainder);
                    if (!remainder.IsZero) limit = math.Add(limit, BigInteger.One);
                    if (prd.FailureCount.Sign < 0 || math.Compare(prd.FailureCount, limit) >= 0)
                        return Fail(InvalidValue, path + ".PrdStates[" + i + "].FailureCount");
                }
                return true;
            }

            private bool BaselineNumbers(BattleEntryBaseline baseline, string path)
            {
                if (baseline == null || baseline.Entry == null) return Fail(MissingField, path);
                var entry = baseline.Entry;
                RuleContextChecks.CheckBudget(entry, math); math.CheckInteger(entry.Level.RecommendedLevel);
                foreach (var member in entry.Members) if (!MemberNumbers(member, path + ".Member")) return false;
                foreach (var face in entry.Level.Faces) if (!FaceNumbers(face, path + ".Face")) return false;
                baseline.RandomInitials.Battle.Validate(math); baseline.RandomInitials.BaseReward.Validate(math); baseline.RandomInitials.Bonus.Validate(math);
                return PrdNumbers(baseline.PrdInitialStates, path + ".PrdInitialStates");
            }

            private bool SnapshotNumbers(BattleSnapshot state, string path)
            {
                if (state == null || state.Board == null || state.Board.Face == null) return Fail(MissingField, path);
                math.CheckInteger(state.SceneRevision); math.CheckInteger(state.EffectiveActionsCompleted);
                math.CheckInteger(state.EnemyPhasesCompleted); math.CheckInteger(state.CurrentFaceIndex);
                if (!FaceNumbers(state.Board.Face, path + ".Board.Face")) return false;
                foreach (var route in state.Board.LockedRoutes) foreach (var cell in route.Route) CellNumbers(cell);
                return Rows(state.Members, state.Enemies, state.Contributions, path) && RandomNumbers(state.Random, path + ".Random");
            }

            private bool Rows(IReadOnlyList<BattleMemberState> members, IReadOnlyList<BattleEnemyState> enemies,
                IReadOnlyList<BattleContributionTotals> totals, string path)
            {
                for (var i = 0; i < members.Count; i++)
                {
                    var row = members[i]; var p = path + $".Members[{i}]";
                    if (row == null) return Fail(MissingField, p);
                    if (!Number(row.Hp, p + ".Hp") || !MemberNumbers(row.Member, p + ".Member")) return false;
                    math.CheckInteger(row.OriginalSlot);
                }
                for (var i = 0; i < enemies.Count; i++)
                {
                    var row = enemies[i]; var p = path + $".Enemies[{i}]";
                    if (row == null) return Fail(MissingField, p);
                    if (!Number(row.Hp, p + ".Hp") || !EnemyNumbers(row.Enemy, p + ".Enemy")) return false;
                    math.CheckInteger(row.IntentCursor); math.CheckInteger(row.OriginalSlot); math.CheckInteger(row.StableOrder);
                }
                for (var i = 0; i < totals.Count; i++)
                {
                    var row = totals[i]; var p = path + $".Contributions[{i}]";
                    if (row == null) return Fail(MissingField, p);
                    if (!Number(row.EffectiveDamageDealtHp, p + ".Dealt") || !Number(row.EffectiveDamageTakenHp, p + ".Taken")) return false;
                }
                return true;
            }

            private bool MemberNumbers(PreparedMember member, string path)
            {
                math.CheckInteger(member.Level); math.CheckInteger(member.OriginalSlot); RuleContextChecks.CheckBudget(member.StatsContext, math);
                return Stats(member.Stats, path + ".Stats") && Number(member.EntryHp, path + ".EntryHp") && CritNumbers(member.Crit, path + ".Crit");
            }
            private bool FaceNumbers(PreparedFace face, string path)
            {
                math.CheckInteger(face.Width); math.CheckInteger(face.Height);
                foreach (var pair in face.Pairs)
                { math.CheckInteger(pair.GeometryColorId); CellNumbers(pair.EndpointA); CellNumbers(pair.EndpointB); if (!EnemyNumbers(pair.Enemy, path + ".Enemy")) return false; }
                return true;
            }
            private bool EnemyNumbers(PreparedEnemy enemy, string path)
            {
                math.CheckInteger(enemy.OriginalSlot); math.CheckInteger(enemy.StableOrder);
                if (!Stats(enemy.Stats, path + ".Stats")) return false;
                foreach (var intent in enemy.IntentCycle)
                    if (intent.DamageCoefficient != null && !Number(intent.DamageCoefficient, path + ".Coefficient")) return false;
                return true;
            }
            private bool Stats(PreparedStats stats, string path)
            {
                math.CheckInteger(stats.AttackRange);
                if (!Number(stats.MaxHp, path + ".MaxHp") || !Number(stats.Attack, path + ".Attack") ||
                    !Number(stats.PhysicalDefense, path + ".PhysicalDefense") || !Number(stats.MagicDefense, path + ".MagicDefense") ||
                    !Number(stats.Evasion, path + ".Evasion")) return false;
                if (!stats.Evasion.Numerator.IsZero) return Fail(UnsupportedBinding, path + ".Evasion");
                return stats.MaxHp.Numerator.Sign > 0 && stats.Attack.Numerator.Sign >= 0 && stats.PhysicalDefense.Numerator.Sign >= 0 &&
                    stats.MagicDefense.Numerator.Sign >= 0 && stats.AttackRange > 0 || Fail(InvalidValue, path);
            }
            private bool RandomNumbers(BattleRandomSnapshot random, string path)
            {
                if (random == null || random.Stream == null) return Fail(MissingField, path);
                random.Stream.Validate(math); return PrdNumbers(random.PrdStates, path + ".PrdStates");
            }
            private bool PrdNumbers(IReadOnlyList<BattlePrdState> rows, string path)
            {
                foreach (var row in rows)
                {
                    if (row == null || row.Crit == null) return Fail(MissingField, path);
                    math.CheckInteger(row.FailureCount); if (!CritNumbers(row.Crit, path + ".Crit")) return false;
                }
                return true;
            }
            private bool CritNumbers(PreparedWarriorCrit crit, string path)
            {
                if (!Number(crit.TargetProbability, path + ".TargetProbability") || !Number(crit.C, path + ".C") ||
                    !Number(crit.Multiplier, path + ".Multiplier")) return false;
                return crit.C.Numerator.Sign > 0 && math.Compare(crit.C.Numerator, crit.C.Denominator) <= 0 && crit.TargetProbability.Numerator.Sign > 0 &&
                    math.Compare(crit.TargetProbability.Numerator, crit.TargetProbability.Denominator) <= 0 && crit.Multiplier.Numerator.Sign > 0 || Fail(InvalidValue, path);
            }
            private bool HitNumbers(BattleDamageFact hit)
            {
                math.CheckInteger(hit.SceneRevision); math.CheckInteger(hit.ActionOrdinal); math.CheckInteger(hit.EffectIndex); math.CheckInteger(hit.RoundedDamage);
                foreach (var value in new[] { hit.Attack, hit.PhysicalDefense, hit.Multiplier, hit.RawDamage, hit.MitigatedDamage,
                    hit.BlockPrevented, hit.ShieldAbsorbed, hit.HpBefore, hit.HpAfter, hit.HpLoss, hit.Overflow })
                { if (!Number(value, "DamageFacts[0].Numbers")) return false; if (value.Numerator.Sign < 0) return Fail(InvalidValue, "DamageFacts[0].Numbers"); }
                var crit = hit.Crit;
                math.CheckInteger(crit.OpportunityOrdinal); math.CheckInteger(crit.FailureCountBefore); math.CheckInteger(crit.FailureCountAfter);
                crit.StreamBefore.Validate(math); crit.StreamAfter.Validate(math);
                foreach (var word in crit.Words) math.CheckInteger(word);
                return CritNumbers(crit.Parameters, "DamageFacts[0].Crit.Parameters") && Number(crit.Probability, "DamageFacts[0].Crit.Probability") &&
                    (hit.RoundedDamage.Sign >= 0 || Fail(InvalidValue, "DamageFacts[0].RoundedDamage"));
            }
            private bool Number(ExactRational value, string path)
            { if (value == null) return Fail(MissingField, path); math.CheckInteger(value.Numerator); math.CheckInteger(value.Denominator); return true; }
            private void CellNumbers(FlowPos cell) { math.CheckInteger(cell.x); math.CheckInteger(cell.y); }
            private bool Hp(ExactRational hp, ExactRational max, string path)
            { return hp.Numerator.Sign >= 0 && hp.Compare(max, math) <= 0 || Fail(InvalidValue, path); }
            private bool Equal(ExactRational a, ExactRational b)
            { return math.Compare(a.Numerator, b.Numerator) == 0 && math.Compare(a.Denominator, b.Denominator) == 0; }
            private static bool Same(string a, string b) { return string.Equals(a, b, StringComparison.Ordinal); }
            private bool SameInteger(BigInteger a, BigInteger b) { return math.Compare(a, b) == 0; }
            private bool SameInitial(Pcg32StreamState a, Pcg32StreamState b)
            { return SameInteger(a.Initial.State, b.Initial.State) && SameInteger(a.Initial.Increment, b.Initial.Increment); }
            private bool SameContext(PreparedRuleContext a, PreparedRuleContext b)
            { return RuleContextChecks.Same(a, b); }
            private bool Text(string value, string path) { return !string.IsNullOrWhiteSpace(value) || Fail(MissingField, path); }
            private bool Fail(CandidateEnemyPhaseRejectionCode code, string path)
            { Rejection = new CandidateEnemyPhaseResult(code, path); return false; }
        }
    }
}
