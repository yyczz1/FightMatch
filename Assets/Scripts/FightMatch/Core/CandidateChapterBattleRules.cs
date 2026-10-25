using System;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;
using FlowPuzzle.Core;
using static FightMatch.Core.CandidateBattleReportFingerprint;

namespace FightMatch.Core
{
    // The chapter branch owns its passive semantics; legacy records retain the warrior-only interpreter.
    internal static class CandidateChapterBattleRules
    {
        internal const string RuleVersion = "ch1-slice-r1";
        internal static bool IsChapter(RuleContext context) => context != null && context.RuleVersion == RuleVersion;
        internal static bool IsChapter(PreparedRuleContext context) => context != null && context.RuleVersion == RuleVersion;
        internal static bool Supports(PreparedMember member, PreparedRuleContext context) =>
            member.ClassKind == CharacterClassKind.Warrior && member.Crit != null && member.Penetration == null ||
            IsChapter(context) && member.ClassKind == CharacterClassKind.Mage && member.Crit == null && member.Penetration != null;
        internal static bool HasStateContract(PreparedBattleEntry entry) =>
            IsChapter(entry.Context) && entry.RequiredFeatures.Count == 1 &&
            entry.RequiredFeatures[0] == ChapterRuleDefinitions.StateFeatureId;

        internal static bool ValidFeatures(PreparedBattleEntry entry) =>
            entry.RequiredFeatures.Count == 0 || HasStateContract(entry);

        internal static bool SkillsSupported(PreparedMember member, PreparedBattleEntry entry) =>
            HasStateContract(entry) ? member.BattleSkills.Count == member.LearnedSkills.Count :
            member.LearnedSkills.Count == 0 && member.BattleSkills.Count == 0;

        internal static ChapterBattleState InitialState(PreparedBattleEntry entry)
        {
            if (!HasStateContract(entry)) return null;
            var skills = new List<ChapterMemberSkillState>();
            foreach (var member in entry.ReadyParticipants)
                foreach (var skill in member.BattleSkills)
                    skills.Add(new ChapterMemberSkillState(BattleCombatantKey.ForParticipant(entry.AttemptId, member.CharacterId), skill.SkillId, 0));
            return new ChapterBattleState(BigInteger.Zero, skills, Array.Empty<ChapterTauntState>(),
                Array.Empty<ChapterVulnerabilityWindow>());
        }

        internal static bool ValidState(BattleSnapshot snapshot, ExactMathBudget math)
        {
            var state = snapshot.ChapterState;
            if (!HasStateContract(snapshot.Baseline.Entry))
                return state == null && snapshot.Enemies.All(x => x.RemainingWindUses == null);
            if (state == null || state.FaceEnemyPhasesCompleted.Sign < 0 ||
                state.FaceEnemyPhasesCompleted > snapshot.EnemyPhasesCompleted || state.Taunts.Count != 0) return false;
            Check(state, math);
            var index = 0;
            foreach (var member in snapshot.Members)
                foreach (var skill in member.Member.BattleSkills)
                {
                    if (index >= state.MemberSkills.Count) return false;
                    var row = state.MemberSkills[index++];
                    if (row == null || !row.CombatantKey.Equals(member.CombatantKey) || row.SkillId != skill.SkillId ||
                        row.CooldownRemaining < 0 || row.CooldownRemaining > skill.CooldownTurns) return false;
                }
            if (index != state.MemberSkills.Count) return false;
            foreach (var enemy in snapshot.Enemies)
                if (enemy.Enemy.WindUses.HasValue ? !enemy.RemainingWindUses.HasValue || enemy.RemainingWindUses < 0 ||
                    enemy.RemainingWindUses > enemy.Enemy.WindUses : enemy.RemainingWindUses.HasValue) return false;
            var targets = new HashSet<BattleCombatantKey>();
            foreach (var window in state.Vulnerabilities)
            {
                if (window == null || !targets.Add(window.TargetEnemyKey) ||
                    window.EffectiveActionOrdinal != math.Add(snapshot.EffectiveActionsCompleted, BigInteger.One)) return false;
                var enemy = snapshot.Enemies.FirstOrDefault(x => x.CombatantKey.Equals(window.TargetEnemyKey));
                if (enemy == null || !enemy.Enemy.IntentCycle.Any(x => x.NextActionVulnerability != null &&
                    Equal(x.NextActionVulnerability, window.Multiplier, math))) return false;
            }
            return true;
        }

        internal static PreparedChapterBattleSkill ReadyTaunt(BattleSnapshot before, BattleMemberState actor)
        {
            if (!HasStateContract(before.Baseline.Entry) || before.ChapterState == null || actor.Member.ClassKind != CharacterClassKind.Warrior)
                return null;
            var skill = actor.Member.BattleSkills.SingleOrDefault();
            if (skill == null) return null;
            var state = before.ChapterState.MemberSkills.FirstOrDefault(x => x.CombatantKey.Equals(actor.CombatantKey) && x.SkillId == skill.SkillId);
            return state?.CooldownRemaining == 0 ? skill : null;
        }

        internal static ChapterVulnerabilityWindow Vulnerability(BattleSnapshot before, BattleCombatantKey enemy, BigInteger action) =>
            before.ChapterState?.Vulnerabilities.FirstOrDefault(x => x.TargetEnemyKey.Equals(enemy) && x.EffectiveActionOrdinal == action);

        internal static CandidateCombatFrame FinishDirect(CandidateRandomBinding binding, BattleSnapshot before,
            CandidateAttackSource action, IEnumerable<BattleEnemyState> enemies, BattleRandomSnapshot random,
            IEnumerable<BattleContributionTotals> totals, IEnumerable<BattleDamageFact> damage, ExactMathBudget math)
        {
            var rows = enemies.ToList();
            var casts = new List<ChapterSkillCastFact>();
            var state = before.ChapterState;
            if (state != null)
            {
                var actor = before.Members.Single(x => x.CombatantKey.Equals(action.Actor));
                var target = rows.Single(x => x.PairKey.Equals(action.Pair));
                var skill = ReadyTaunt(before, actor);
                if (skill != null && target.Hp.Numerator.Sign > 0)
                {
                    var phase = math.Add(before.EnemyPhasesCompleted, BigInteger.One);
                    casts.Add(new ChapterSkillCastFact(actor.CombatantKey, target.CombatantKey, skill, 0, skill.CooldownTurns, phase));
                    var skills = state.MemberSkills.Select(x => x.CombatantKey.Equals(actor.CombatantKey) && x.SkillId == skill.SkillId
                        ? new ChapterMemberSkillState(x.CombatantKey, x.SkillId, skill.CooldownTurns) : x);
                    state = new ChapterBattleState(state.FaceEnemyPhasesCompleted, skills,
                        new[] { new ChapterTauntState(target.CombatantKey, actor.CombatantKey, phase) }, state.Vulnerabilities);
                }
            }
            return new CandidateCombatFrame(binding, before, action, rows, random, totals, damage, state, casts);
        }

        internal static bool ValidateDirectFrame(CandidateCombatFrame frame, ExactMathBudget math)
        {
            if (frame?.BeforeSnapshot == null) return false;
            if (!HasStateContract(frame.BeforeSnapshot.Baseline.Entry))
                return frame.ChapterState == null && frame.SkillCasts.Count == 0 && ValidateNormalFrame(frame, math);
            var before = frame.BeforeSnapshot;
            var action = frame.Action;
            if (action == null || frame.Binding?.Start == null || !ReferenceEquals(frame.Binding.Start.Baseline, before.Baseline) ||
                action.PlayerId != before.Baseline.Entry.PlayerId || action.AttemptId != before.Baseline.Entry.AttemptId ||
                string.IsNullOrWhiteSpace(action.OperationId) || action.ExpectedSceneRevision != before.SceneRevision ||
                action.ActionOrdinal != math.Add(before.EffectiveActionsCompleted, BigInteger.One) || !ValidState(before, math)) return false;
            var request = new CandidateAttackRequest { PlayerId = action.PlayerId, AttemptId = action.AttemptId,
                OperationId = action.OperationId, ExpectedSceneRevision = action.ExpectedSceneRevision,
                Actor = action.Actor, Pair = action.Pair, Route = action.Route.ToList() };
            if (CandidateDirectAttack.CheckState(before, request, math, out var actor, out var target) != null) return false;
            var face = before.Board.Face;
            var geometry = new FlowLevelData { width = face.Width, height = face.Height };
            foreach (var pair in face.Pairs)
                geometry.pairs.Add(new FlowPairData { colorId = pair.GeometryColorId, endpointA = pair.EndpointA, endpointB = pair.EndpointB });
            var pairDefinition = face.Pairs.Single(x => x.PairId == action.Pair.PairId);
            var locked = before.Board.LockedRoutes.Select(x => new FlowPathData {
                colorId = face.Pairs.Single(p => p.PairId == x.PairKey.PairId).GeometryColorId, cells = x.Route.ToList() }).ToList();
            if (!new BattleRouteValidator().Validate(geometry, locked, Array.Empty<FlowPos>(), pairDefinition.GeometryColorId, request.Route).IsValid) return false;
            var ordinal = 1 + before.Enemies.Count(x => x.Hp.Numerator.Sign > 0 && x.OriginalSlot < target.OriginalSlot);
            var canNormal = ordinal <= actor.Member.Stats.AttackRange;
            if (frame.DamageFacts.Count != (canNormal ? 1 : 0)) return false;
            var enemies = before.Enemies.ToList();
            var totals = before.Contributions.ToList();
            if (canNormal)
            {
                if (!ValidateNormalFrame(frame, math)) return false;
                var hit = frame.DamageFacts[0];
                if (!ReferenceEquals(hit.Baseline, before.Baseline) || hit.PlayerId != action.PlayerId || hit.AttemptId != action.AttemptId ||
                    hit.OperationId != action.OperationId || hit.SceneRevision != before.SceneRevision || hit.ActionOrdinal != action.ActionOrdinal ||
                    !hit.Actor.Equals(action.Actor) || !hit.Pair.Equals(action.Pair) || !hit.Target.Equals(target.CombatantKey)) return false;
                var i = enemies.FindIndex(x => x.CombatantKey.Equals(target.CombatantKey));
                enemies[i] = new BattleEnemyState(target.CombatantKey, target.PairKey, target.Enemy, hit.HpAfter,
                    target.IntentCursor, target.RemainingWindUses);
                i = totals.FindIndex(x => x.CombatantKey.Equals(actor.CombatantKey));
                totals[i] = new BattleContributionTotals(actor.CombatantKey,
                    totals[i].EffectiveDamageDealtHp.Add(hit.HpLoss, math), totals[i].EffectiveDamageTakenHp);
            }
            else if (ReadyTaunt(before, actor) == null || !ValidPrd(before.Baseline.Entry, before.Random, math) ||
                !Equal(before.Random.Stream.Initial.State, frame.Binding.Battle.Initial.Initial.State, math) ||
                !Equal(before.Random.Stream.Initial.Increment, frame.Binding.Battle.Initial.Initial.Increment, math) ||
                !Equal(before.Random, frame.Random, math)) return false;
            var expected = FinishDirect(frame.Binding, before, action, enemies, frame.Random, totals, frame.DamageFacts, math);
            return Equal(before.Members, frame.Members, math) && Equal(expected.Enemies, frame.Enemies, math) &&
                Equal(expected.Contributions, frame.Contributions, math) && Equal(expected.ChapterState, frame.ChapterState, math) &&
                Equal(expected.SkillCasts, frame.SkillCasts, math);
        }

        internal static string PassiveId(PreparedMember member) => member.Crit != null ? member.Crit.PassiveDefinitionId : member.Penetration.PassiveDefinitionId;
        internal static ExactRational Coefficient(PreparedMember member) => member.Crit != null ? member.Crit.C : member.Penetration.C;
        internal static BattlePrdState PrdState(BattleCombatantKey key, PreparedMember member, BigInteger failures) =>
            member.Crit != null ? new BattlePrdState(key, member.Crit, failures) : new BattlePrdState(key, member.Penetration, failures);
        internal static bool SamePassive(BattlePrdState row, PreparedMember member) =>
            ReferenceEquals(row.Crit, member.Crit) && ReferenceEquals(row.Penetration, member.Penetration);

        internal static void CheckPassiveNumbers(PreparedWarriorCrit crit, PreparedArcanePenetration penetration, ExactMathBudget math)
        {
            if (crit != null)
            { Check(crit.TargetProbability, math); Check(crit.C, math); Check(crit.Multiplier, math); }
            if (penetration != null)
            { Check(penetration.TargetProbability, math); Check(penetration.C, math); Check(penetration.IgnoreMagicDefenseFraction, math); }
        }

        internal static bool ValidPrd(PreparedBattleEntry entry, BattleRandomSnapshot random, ExactMathBudget math)
        {
            if (random == null || random.Stream == null || random.PrdStates.Count != entry.ReadyParticipants.Count) return false;
            random.Stream.Validate(math);
            for (var i = 0; i < random.PrdStates.Count; i++)
            {
                var row = random.PrdStates[i]; var member = entry.ReadyParticipants[i];
                if (row == null || !Supports(member, entry.Context) || !SamePassive(row, member) ||
                    !BattleCombatantKey.ForParticipant(entry.AttemptId, member.CharacterId).Equals(row.CombatantKey)) return false;
                CheckPassiveNumbers(row.Crit, row.Penetration, math);
                math.CheckInteger(row.FailureCount);
                var c = Coefficient(member);
                var limit = math.DivRem(c.Denominator, c.Numerator, out var remainder);
                if (!remainder.IsZero) limit = math.Add(limit, BigInteger.One);
                if (row.FailureCount.Sign < 0 || math.Compare(row.FailureCount, limit) >= 0) return false;
            }
            return true;
        }

        // Called only after route, range and living actor/target checks establish a real normal hit.
        internal static ChapterPassiveResolution ResolvePassive(CandidateRandomBinding binding, BattleSnapshot before,
            BattleMemberState actor, BattleEnemyState target, RandomSamplingBudget budget)
        {
            var entry = before.Baseline.Entry;
            if (!IsChapter(entry.Context) || !ValidPrd(entry, before.Random, budget.Math) ||
                !Equal(before.Random.Stream.Initial.State, binding.Battle.Initial.Initial.State, budget.Math) ||
                !Equal(before.Random.Stream.Initial.Increment, binding.Battle.Initial.Initial.Increment, budget.Math)) return null;
            BattlePrdState previous = null;
            foreach (var row in before.Random.PrdStates) if (row.CombatantKey.Equals(actor.CombatantKey)) previous = row;
            var sample = ExactRandomSampler.PrdOpportunity(Coefficient(actor.Member), previous.FailureCount, before.Random.Stream, budget);
            var rows = new List<BattlePrdState>();
            foreach (var row in before.Random.PrdStates)
                rows.Add(row.CombatantKey.Equals(actor.CombatantKey) ? PrdState(row.CombatantKey, actor.Member, sample.Value.Failures) : row);
            var next = new BattleRandomSnapshot(sample.NextState, rows);
            if (actor.Member.ClassKind == CharacterClassKind.Mage)
                return new ChapterPassiveResolution(next, null, new CandidatePenetrationFact(actor.CombatantKey, target.CombatantKey,
                    before.EffectiveActionsCompleted, actor.Member.Penetration, previous, before.Random.Stream, sample));
            return new ChapterPassiveResolution(next, new CandidateCritFact(actor.CombatantKey, target.CombatantKey,
                before.EffectiveActionsCompleted, actor.Member.Crit, previous, before.Random.Stream, sample), null);
        }

        // Checks retained evidence using its recorded words. It never generates a new opportunity or advances live random state.
        private static bool ValidateNormalFrame(CandidateCombatFrame frame, ExactMathBudget math)
        {
            if (frame?.BeforeSnapshot == null || frame.DamageFacts.Count != 1) return false;
            var before = frame.BeforeSnapshot; var entry = before.Baseline.Entry; var hit = frame.DamageFacts[0];
            if (!IsChapter(entry.Context) || hit == null || !ValidPrd(entry, before.Random, math) || !ValidPrd(entry, frame.Random, math)) return false;
            BattleMemberState actor = null; BattleEnemyState target = null;
            foreach (var member in before.Members) if (member.CombatantKey.Equals(hit.Actor)) actor = member;
            foreach (var enemy in before.Enemies) if (enemy.CombatantKey.Equals(hit.Target)) target = enemy;
            if (actor == null || target == null || actor.Hp.Numerator.Sign <= 0 || target.Hp.Numerator.Sign <= 0) return false;
            var ordinal = 1;
            foreach (var enemy in before.Enemies)
                if (enemy.Hp.Numerator.Sign > 0 && enemy.OriginalSlot < target.OriginalSlot) ordinal++;
            if (ordinal > actor.Member.Stats.AttackRange) return false;
            var mage = actor.Member.ClassKind == CharacterClassKind.Mage;
            if (mage ? hit.Crit != null || hit.Penetration == null : hit.Crit == null || hit.Penetration != null) return false;
            var evidence = new PassiveEvidence(hit);
            if (!evidence.Actor.Equals(hit.Actor) || !evidence.Target.Equals(hit.Target) || evidence.Ordinal != before.EffectiveActionsCompleted ||
                !(mage ? ReferenceEquals(hit.Penetration.Parameters, actor.Member.Penetration) : ReferenceEquals(hit.Crit.Parameters, actor.Member.Crit)) ||
                !Equal(evidence.Before, before.Random.Stream, math) || !Equal(evidence.After, frame.Random.Stream, math)) return false;
            var one = ExactRational.Create(1, 1, math); var hundred = ExactRational.Create(100, 1, math);
            var multiplier = !mage && evidence.Triggered ? actor.Member.Crit.Multiplier : one;
            var defense = mage ? target.Enemy.Stats.MagicDefense : target.Enemy.Stats.PhysicalDefense;
            if (mage && evidence.Triggered) defense = defense.Multiply(one.Subtract(actor.Member.Penetration.IgnoreMagicDefenseFraction, math), math);
            var vulnerability = Vulnerability(before, target.CombatantKey, frame.Action.ActionOrdinal);
            if (!Equal(vulnerability, hit.Vulnerability, math)) return false;
            var raw = actor.Member.Stats.Attack.Multiply(multiplier, math);
            if (vulnerability != null) raw = raw.Multiply(vulnerability.Multiplier, math);
            var mitigated = raw.Multiply(hundred, math).Divide(hundred.Add(defense, math), math);
            var rounded = mitigated.Floor(math); var incoming = ExactRational.Create(rounded, 1, math);
            var loss = incoming.Compare(target.Hp, math) < 0 ? incoming : target.Hp;
            if (hit.DamageKind != (mage ? EntryDamageKind.Magic : EntryDamageKind.Physical) ||
                !Equal(hit.Attack, actor.Member.Stats.Attack, math) || !Equal(hit.PhysicalDefense, target.Enemy.Stats.PhysicalDefense, math) ||
                !Equal(hit.MagicDefense, target.Enemy.Stats.MagicDefense, math) ||
                !Equal(hit.EffectiveMagicDefense, mage ? defense : null, math) || !Equal(hit.Multiplier, multiplier, math) ||
                !Equal(hit.RawDamage, raw, math) || !Equal(hit.MitigatedDamage, mitigated, math) || hit.RoundedDamage != rounded ||
                !Equal(hit.HpBefore, target.Hp, math) || !Equal(hit.HpLoss, loss, math) ||
                !Equal(hit.HpAfter, target.Hp.Subtract(loss, math), math) || !Equal(hit.Overflow, incoming.Subtract(loss, math), math) ||
                !hit.BlockPrevented.Numerator.IsZero || !hit.ShieldAbsorbed.Numerator.IsZero) return false;
            var failures = math.Add(evidence.FailuresBefore, BigInteger.One);
            var probability = Coefficient(actor.Member).Multiply(ExactRational.Create(failures, 1, math), math);
            if (probability.Compare(one, math) > 0) probability = one;
            if (!Equal(evidence.Probability, probability, math) || evidence.FailuresAfter != (evidence.Triggered ? BigInteger.Zero : failures)) return false;
            for (var i = 0; i < before.Random.PrdStates.Count; i++)
            {
                var old = before.Random.PrdStates[i]; var next = frame.Random.PrdStates[i];
                if (old.CombatantKey.Equals(hit.Actor))
                { if (old.FailureCount != evidence.FailuresBefore || next.FailureCount != evidence.FailuresAfter) return false; }
                else if (old.FailureCount != next.FailureCount) return false;
            }
            return RecordedSample(evidence, math);
        }

        private static bool RecordedSample(PassiveEvidence evidence, ExactMathBudget math)
        {
            var before = evidence.Before; var after = evidence.After;
            if (!Equal(before.Initial.State, after.Initial.State, math) || !Equal(before.Initial.Increment, after.Initial.Increment, math) ||
                after.WordsConsumed != math.Add(before.WordsConsumed, evidence.Words.Count)) return false;
            var current = before.Current;
            foreach (var word in evidence.Words)
            {
                math.ChargePcgWord(current.State);
                if (Pcg32Core.Next32(current, out var next) != word) return false;
                current = next;
            }
            if (current.State != after.Current.State || current.Increment != after.Current.Increment) return false;
            if (evidence.Probability.Numerator == evidence.Probability.Denominator) return evidence.Triggered && evidence.Words.Count == 0;
            if (evidence.Words.Count == 0) return false;
            try
            {
                var sampled = ExactRandomSampler.UniformRecorded(evidence.Probability.Denominator, evidence.Words,
                    new RandomSamplingBudget(math, evidence.Words.Count), out var consumed);
                return consumed == evidence.Words.Count && evidence.Triggered == (sampled.CompareTo(evidence.Probability.Numerator) < 0);
            }
            catch (InvalidOperationException) { return false; }
        }
    }

    internal sealed class ChapterPassiveResolution
    {
        internal BattleRandomSnapshot Next { get; }
        internal CandidateCritFact Crit { get; }
        internal CandidatePenetrationFact Penetration { get; }
        internal ChapterPassiveResolution(BattleRandomSnapshot next, CandidateCritFact crit, CandidatePenetrationFact penetration)
        { Next = next; Crit = crit; Penetration = penetration; }
    }

    internal sealed class PassiveEvidence
    {
        internal BattleCombatantKey Actor { get; }
        internal BattleCombatantKey Target { get; }
        internal BigInteger Ordinal { get; }
        internal BigInteger FailuresBefore { get; }
        internal BigInteger FailuresAfter { get; }
        internal bool Triggered { get; }
        internal ExactRational Probability { get; }
        internal Pcg32StreamState Before { get; }
        internal Pcg32StreamState After { get; }
        internal IReadOnlyList<uint> Words { get; }
        internal PassiveEvidence(BattleDamageFact hit)
        {
            var p = hit.Penetration; var c = hit.Crit;
            Actor = p != null ? p.Actor : c.Actor; Target = p != null ? p.Target : c.Target;
            Ordinal = p != null ? p.OpportunityOrdinal : c.OpportunityOrdinal;
            FailuresBefore = p != null ? p.FailureCountBefore : c.FailureCountBefore;
            FailuresAfter = p != null ? p.FailureCountAfter : c.FailureCountAfter;
            Triggered = p != null ? p.Triggered : c.Triggered; Probability = p != null ? p.Probability : c.Probability;
            Before = p != null ? p.StreamBefore : c.StreamBefore; After = p != null ? p.StreamAfter : c.StreamAfter;
            Words = p != null ? p.Words : c.Words;
        }
    }

    public sealed class CandidatePenetrationFact
    {
        public BattleCombatantKey Actor { get; }
        public BattleCombatantKey Target { get; }
        public BigInteger OpportunityOrdinal { get; }
        public PreparedArcanePenetration Parameters { get; }
        public ExactRational Probability { get; }
        public BigInteger FailureCountBefore { get; }
        public BigInteger FailureCountAfter { get; }
        public bool Triggered { get; }
        public Pcg32StreamState StreamBefore { get; }
        public Pcg32StreamState StreamAfter { get; }
        public IReadOnlyList<uint> Words { get; }
        internal CandidatePenetrationFact(BattleCombatantKey actor, BattleCombatantKey target, BigInteger ordinal,
            PreparedArcanePenetration parameters, BattlePrdState before, Pcg32StreamState stream, RandomSample<PrdOutcome> sample)
        {
            Actor = actor; Target = target; OpportunityOrdinal = ordinal; Parameters = parameters;
            Probability = sample.Value.Probability; FailureCountBefore = before.FailureCount;
            FailureCountAfter = sample.Value.Failures; Triggered = sample.Value.Triggered;
            StreamBefore = stream; StreamAfter = sample.NextState; Words = new List<uint>(sample.Words).AsReadOnly();
        }
    }
}
