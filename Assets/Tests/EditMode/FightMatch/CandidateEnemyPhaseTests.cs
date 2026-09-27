using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using FlowPuzzle.Core;
using NUnit.Framework;
using static FightMatch.Core.CandidateEnemyPhaseRejectionCode;

namespace FightMatch.Core.Tests
{
    [TestFixture]
    public class CandidateEnemyPhaseTests
    {
        [TestCase(1, 1)] [TestCase(1, 2)] [TestCase(3, 1)] [TestCase(3, 2)]
        public void SourceL1AndL3UseReal009AndRetainConditionalSourceFacts(int level, int direction)
        {
            var input = SourceCandidate(level, direction, out var route);
            var binding = Bind(input); var request = Request(binding.Start.Snapshot, route);
            var direct = Direct(binding, binding.Start.Snapshot, request); var result = Accept(direct);
            Assert.IsFalse(direct.DamageFacts[0].Crit.Triggered, "This vector is conditional on the real first opportunity not triggering.");
            Assert.AreEqual(BigInteger.One, result.EnemyPhaseOrdinal);
            Assert.AreSame(direct, result.DirectAttack); Assert.AreSame(direct.Random, result.Random);
            Assert.AreSame(binding.Start.Snapshot, result.DirectAttack.BeforeSnapshot);
            Assert.AreEqual(BigInteger.One, direct.Random.Stream.WordsConsumed);
            Assert.AreEqual(BigInteger.One, direct.Random.PrdStates[0].FailureCount);
            Value(result.Members[0].Hp, 95); Value(result.Contributions[0].EffectiveDamageTakenHp, 5);
            Value(result.Contributions[0].EffectiveDamageDealtHp, level == 1 ? 15 : 16);
            Assert.AreEqual(level == 1 ? 1 : 2, result.OrderedIntents.Count);
            if (level == 3)
            {
                Value(result.Enemies[0].Hp, 4);
                Assert.AreEqual(EnemyIntentKind.Charge, result.OrderedIntents[0].IntentKind);
                Assert.IsNull(result.OrderedIntents[0].Damage);
                Assert.AreEqual(BigInteger.One, result.Enemies[0].IntentCursor);
            }
            else { Value(result.Enemies[0].Hp, 0); Assert.AreEqual(BigInteger.Zero, result.Enemies[0].IntentCursor); }
            var hit = result.OrderedIntents.Last().Damage;
            Value(hit.Attack, 10); Value(hit.DamageCoefficient, 3, 5); Value(hit.PhysicalDefense, 10);
            Value(hit.RawDamage, 6); Value(hit.MitigatedDamage, 60, 11); Assert.AreEqual(new BigInteger(5), hit.RoundedDamage);
            Value(hit.HpBefore, 100); Value(hit.HpAfter, 95); Value(hit.HpLoss, 5); Value(hit.Overflow, 0);
            Value(hit.BlockPrevented, 0); Value(hit.ShieldAbsorbed, 0); Assert.IsFalse(hit.DefeatedTarget);
            Assert.AreEqual(EntryDamageKind.Physical, hit.DamageKind);
            Assert.AreEqual(direct.Members[0].CombatantKey, hit.TargetMember);
            Assert.IsTrue(input.Context.SourceNotes.Any(n => n.Contains("conditional")));
            AssertFacts(result);
        }

        [TestCase(10, 13, 10, 11, 13, 1, 130, 11)]
        [TestCase(1, 13, 10, 1, 13, 10, 13, 11)]
        [TestCase(5, 7, 3, 10, 35, 3, 350, 33)]
        public void HeavyReadsBoundCoefficientAndFloorsOnlyAfterDefense(int attack, int cn, int cd,
            int rounded, int rn, int rd, int mn, int md)
        {
            var input = Input(1); Heavy(input, 0); var enemy = input.Level.Faces[0].Pairs[0].Enemy;
            enemy.EnemyDefinitionId = "synthetic:renamed-heavy"; enemy.Stats.Attack = R(attack);
            enemy.IntentCycle[1].DamageCoefficient = R(cn, cd);
            var binding = Bind(input); var initial = binding.Start.Snapshot;
            var before = State(initial, enemies: new[] { Enemy(initial.Enemies[0], R(100), 7) }, actions: 12, phases: 9, revision: 18);
            var direct = Direct(binding, before); var result = Accept(direct); var fact = result.OrderedIntents.Single();
            Assert.AreEqual(EnemyIntentKind.Strike, fact.IntentKind); Assert.AreEqual(new BigInteger(7), fact.CursorBefore);
            Assert.AreEqual(new BigInteger(8), fact.CursorAfter); Assert.AreEqual(new BigInteger(10), result.EnemyPhaseOrdinal);
            Value(fact.Damage.RawDamage, rn, rd); Value(fact.Damage.MitigatedDamage, mn, md);
            Assert.AreEqual(new BigInteger(rounded), fact.Damage.RoundedDamage); Value(fact.Damage.HpLoss, rounded);
            Value(result.Members[0].Hp, 100 - rounded); AssertFacts(result);
        }

        [TestCase(false)] [TestCase(true)]
        public void StableOrderOverridesOriginalSlotAndCollectionOrderWithIndependentLargeCursors(bool reverse)
        {
            var input = Input(3); Heavy(input, 0); Heavy(input, 2);
            input.Level.Faces[0].Pairs[0].Enemy.StableOrder = 30;
            input.Level.Faces[0].Pairs[1].Enemy.StableOrder = 10;
            input.Level.Faces[0].Pairs[2].Enemy.StableOrder = 20;
            var binding = Bind(input); var start = binding.Start.Snapshot; var huge = BigInteger.One << 90;
            var rows = new[] { Enemy(start.Enemies[0], R(100), huge + 1), Enemy(start.Enemies[1], R(100), 9), Enemy(start.Enemies[2], R(100), huge) };
            var before = State(start, enemies: reverse ? rows.Reverse() : rows, actions: 13, phases: 12, revision: 27);
            var result = Accept(Direct(binding, before));
            CollectionAssert.AreEqual(new[] { 10, 20, 30 }, result.OrderedIntents.Select(f => f.StableOrder));
            CollectionAssert.AreEqual(new[] { EnemyIntentKind.Strike, EnemyIntentKind.Charge, EnemyIntentKind.Strike }, result.OrderedIntents.Select(f => f.IntentKind));
            Assert.AreEqual(huge + 2, result.Enemies.Single(e => e.OriginalSlot == 0).IntentCursor);
            Assert.AreEqual(new BigInteger(10), result.Enemies.Single(e => e.OriginalSlot == 3).IntentCursor);
            Assert.AreEqual(huge + 1, result.Enemies.Single(e => e.OriginalSlot == 6).IntentCursor);
            CollectionAssert.AreEqual(before.Enemies.Select(e => e.CombatantKey), result.Enemies.Select(e => e.CombatantKey));
            Value(result.Members[0].Hp, 84); AssertFacts(result);
        }

        [TestCase(false)] [TestCase(true)]
        public void DirectlyKilledAndPreviouslyDeadEnemiesNeitherActNorAdvanceAndEmptyPhaseStillHasOrdinal(bool allDead)
        {
            var input = Input(2); input.Members[0].Stats.Attack = R(100);
            var binding = Bind(input); var start = binding.Start.Snapshot;
            var rows = new[] { Enemy(start.Enemies[0], R(2), 7), Enemy(start.Enemies[1], R(allDead ? 0 : 100), 12) };
            var locked = allDead ? new[] { new BattleLockedRoute(rows[1].PairKey, Route(start.Board.Face.Pairs[1])) } : Array.Empty<BattleLockedRoute>();
            var before = State(start, enemies: rows, board: new BattleBoardState(start.Board.Face, locked, Array.Empty<BattlePairKey>()), phases: 17);
            var direct = Direct(binding, before); var result = Accept(direct);
            Assert.AreEqual(new BigInteger(18), result.EnemyPhaseOrdinal);
            Assert.AreEqual(new BigInteger(7), result.Enemies[0].IntentCursor);
            Assert.AreEqual(new BigInteger(allDead ? 12 : 13), result.Enemies[1].IntentCursor);
            Assert.AreEqual(allDead ? 0 : 1, result.OrderedIntents.Count);
            Value(result.Members[0].Hp, allDead ? 100 : 95);
            Assert.AreSame(direct.Random, result.Random); AssertFacts(result);
        }

        [Test]
        public void CurrentFaceUsesItsOwnEnemyInstanceEvenWhenOtherFacesReuseDisplayIdentities()
        {
            var input = Input(1); var laterInput = Input(1); Heavy(laterInput, 0);
            var later = laterInput.Level.Faces[0]; later.FaceId = "face:later";
            input.Level.Faces[0].Pairs[0].Enemy.Stats.Attack = R(99); input.Level.Faces.Add(later);
            var binding = Bind(input); var start = binding.Start.Snapshot; var face = start.Baseline.Entry.Level.Faces[1];
            var enemy = face.Pairs[0].Enemy;
            var before = State(start, faceIndex: 1, board: new BattleBoardState(face), members: new[] { Member(start, R(90)) },
                enemies: new[] { new BattleEnemyState(BattleCombatantKey.ForEnemy(input.AttemptId, face.FaceId, enemy.EnemyInstanceKey),
                    BattlePairKey.Create(input.AttemptId, face.FaceId, face.Pairs[0].PairId), enemy, R(100), 9) }, revision: 12, actions: 5, phases: 5);
            var result = Accept(Direct(binding, before)); var fact = result.OrderedIntents.Single();
            Assert.AreEqual("face:later", fact.FaceId); Assert.AreEqual(before.Enemies[0].CombatantKey, fact.EnemyKey);
            Assert.AreNotEqual(start.Enemies[0].CombatantKey, fact.EnemyKey);
            Value(fact.Damage.Attack, 10); Value(result.Members[0].Hp, 79); Assert.AreEqual(new BigInteger(10), result.Enemies[0].IntentCursor);
            AssertFacts(result);
        }

        [Test]
        public void ADeadEnemyCannotReappearInTheDirectAttackProjection()
        {
            var binding = Bind(Input(2)); var start = binding.Start.Snapshot;
            var before = State(start, enemies: new[] { start.Enemies[0], Enemy(start.Enemies[1], R(0), 3) },
                board: new BattleBoardState(start.Board.Face, new[] { new BattleLockedRoute(start.Enemies[1].PairKey, Route(start.Board.Face.Pairs[1])) }, Array.Empty<BattlePairKey>()));
            var direct = Direct(binding, before);
            Reject(Frame(direct, enemies: new[] { direct.Enemies[0], Enemy(direct.Enemies[1], R(1), 3) }), InconsistentBinding, "Enemies");
        }

        [TestCase(0)] [TestCase(1)] [TestCase(2)]
        public void N010LastMemberDeathStopsEveryLaterIntentAndRetainsTheLethalFact(int laterCursor)
        {
            var input = Input(3); Heavy(input, 1);
            var binding = Bind(input); var start = binding.Start.Snapshot;
            var before = State(start, members: new[] { Member(start, R(1, 3)) },
                enemies: new[] { Enemy(start.Enemies[0], R(100), 8), Enemy(start.Enemies[1], R(100), laterCursor), Enemy(start.Enemies[2], R(100), 5) },
                contributions: new[] { new BattleContributionTotals(start.Members[0].CombatantKey, R(7, 2), R(4, 3)) }, revision: 9, actions: 4, phases: 3);
            var direct = Direct(binding, before); var result = Accept(direct); var fact = result.OrderedIntents.Single();
            Assert.IsTrue(fact.Damage.DefeatedTarget); Value(fact.Damage.HpLoss, 1, 3); Value(fact.Damage.Overflow, 14, 3);
            Value(result.Members[0].Hp, 0); Value(result.Contributions[0].EffectiveDamageTakenHp, 5, 3);
            Value(result.Contributions[0].EffectiveDamageDealtHp, 7, 2);
            CollectionAssert.AreEqual(new BigInteger[] { 9, laterCursor, 5 }, result.Enemies.Select(e => e.IntentCursor));
            Assert.AreEqual(new BigInteger(4), result.EnemyPhaseOrdinal); AssertFacts(result);
            // A separate synthetic legal input represents a later recovered member, not a revive operation.
            var nextBefore = State(start, members: new[] { Member(start, R(100)) }, enemies: result.Enemies,
                random: result.Random, contributions: result.Contributions, revision: 11, actions: 5, phases: 4);
            var nextRequest = Request(nextBefore); nextRequest.OperationId = "synthetic:later-action-after-recovery";
            var later = Accept(Direct(binding, nextBefore, nextRequest));
            Assert.AreEqual(laterCursor % 2 == 0 ? EnemyIntentKind.Charge : EnemyIntentKind.Strike, later.OrderedIntents[1].IntentKind);
            Assert.AreEqual(new BigInteger(laterCursor), later.OrderedIntents[1].CursorBefore);
            Assert.AreEqual(new BigInteger(laterCursor + 1), later.OrderedIntents[1].CursorAfter);
            Assert.AreEqual(nextRequest.OperationId, later.OrderedIntents[0].OperationId);
            Assert.AreEqual(3, later.OrderedIntents.Count); AssertFacts(later);
        }

        [TestCase(0)] [TestCase(1)]
        public void ZeroDamageStrikeRemainsDistinctFromChargeAndDoesNotChangeContribution(int zeroMode)
        {
            var input = Input(2); Heavy(input, 0);
            input.Level.Faces[0].Pairs[1].Enemy.Stats.Attack = zeroMode == 0 ? R(0) : R(1);
            var binding = Bind(input); var before = State(binding.Start.Snapshot,
                contributions: new[] { new BattleContributionTotals(binding.Start.Snapshot.Members[0].CombatantKey, R(11), R(9, 4)) });
            var result = Accept(Direct(binding, before));
            Assert.IsNull(result.OrderedIntents[0].Damage); Assert.IsNotNull(result.OrderedIntents[1].Damage);
            var strike = result.OrderedIntents[1].Damage;
            Value(strike.RawDamage, zeroMode == 0 ? 0 : 3, zeroMode == 0 ? 1 : 5);
            Assert.AreEqual(BigInteger.Zero, strike.RoundedDamage); Value(strike.HpLoss, 0);
            Assert.IsFalse(strike.DefeatedTarget); Value(result.Members[0].Hp, 100);
            Value(result.Contributions[0].EffectiveDamageTakenHp, 9, 4); Value(result.Contributions[0].EffectiveDamageDealtHp, 11);
            AssertFacts(result);
        }

        [TestCase(0)] [TestCase(1)] [TestCase(2)]
        public void ActualHpLossIsUnweightedAnd009ContributionIsNotCountedTwice(int mode)
        {
            var input = Input(2); input.Members[0].Stats.Attack = R(2);
            var binding = Bind(input); var start = binding.Start.Snapshot;
            var before = State(start, members: new[] { Member(start, mode == 0 ? R(7, 2) : mode == 1 ? R(6) : R(100)) },
                contributions: new[] { new BattleContributionTotals(start.Members[0].CombatantKey, R(5, 3), R(7, 3)) });
            var direct = Direct(binding, before); var result = Accept(direct);
            Value(direct.Contributions[0].EffectiveDamageDealtHp, 11, 3);
            Value(result.Contributions[0].EffectiveDamageDealtHp, 11, 3);
            Value(result.Contributions[0].EffectiveDamageTakenHp, mode == 0 ? 35 : mode == 1 ? 25 : 37, mode == 0 ? 6 : 3);
            Assert.AreEqual(mode == 0 ? 1 : 2, result.OrderedIntents.Count);
            Value(result.OrderedIntents.Last().Damage.HpLoss, mode == 0 ? 7 : mode == 1 ? 1 : 5, mode == 0 ? 2 : 1);
            AssertFacts(result);
        }

        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)]
        public void RepeatedEvaluationIsIdenticalAndAllOutputsRetainAnImmutable009Source(int mode)
        {
            var input = Input(mode == 2 ? 1 : 3);
            if (mode == 1) for (var i = 0; i < 3; i++) Heavy(input, i);
            if (mode == 2) input.Members[0].Stats.Attack = R(100);
            var binding = Bind(input); var before = binding.Start.Snapshot;
            if (mode == 3) before = State(before, members: new[] { Member(before, R(1)) });
            var request = Request(before); var direct = Direct(binding, before, request); var result = Accept(direct);
            var original = Describe(result);
            request.PlayerId = "changed"; request.OperationId = "changed"; request.Route.Clear();
            input.Members.Clear(); input.Level.Faces.Clear(); input.Context.SourceNotes.Clear();
            Assert.AreEqual(original, Describe(result)); Assert.AreEqual(original, Describe(Accept(direct)));
            AssertReadOnlyGraph(result);
            foreach (var property in new[] { "PostSnapshot", "FinalAttemptReport", "Completed", "SceneRevision", "EffectiveActionsCompleted", "Board", "Phase" })
                Assert.IsNull(typeof(CandidateEnemyPhaseFrame).GetProperty(property), property);
            Assert.AreEqual(typeof(CandidateCombatFrame), typeof(CandidateEnemyPhase).GetMethod("Evaluate").GetParameters()[0].ParameterType);
        }

        [TestCase(true)] [TestCase(false)]
        public void NullRootsThrowNamedArgument(bool frame)
        {
            var direct = Direct(Bind(Input(1)));
            var error = Assert.Throws<ArgumentNullException>(() => CandidateEnemyPhase.Evaluate(frame ? null : direct, frame ? new ExactMathBudget() : null));
            Assert.AreEqual(frame ? "directAttack" : "budget", error.ParamName);
        }

        [TestCase("binding", "Binding")] [TestCase("action", "Action")]
        [TestCase("fact", "DamageFacts[0]")] [TestCase("crit", "DamageFacts[0].Crit")]
        [TestCase("player", "Action.PlayerId")] [TestCase("attempt", "Action.AttemptId")]
        [TestCase("operation", "Action.OperationId")] [TestCase("actor", "Action.Actor")]
        [TestCase("pair", "Action.Pair")]
        public void MissingDirectSourceIsRejectedWithoutNumericWork(string change, string path)
        {
            var direct = Direct(Bind(Input(2))); var request = Request(direct.BeforeSnapshot);
            CandidateCombatFrame bad;
            if (change == "binding") bad = new CandidateCombatFrame(null, direct.BeforeSnapshot, direct.Action, direct.Enemies, direct.Random, direct.Contributions, direct.DamageFacts[0]);
            else if (change == "action") bad = new CandidateCombatFrame(direct.Binding, direct.BeforeSnapshot, null, direct.Enemies, direct.Random, direct.Contributions, direct.DamageFacts[0]);
            else if (change == "fact") bad = new CandidateCombatFrame(direct.Binding, direct.BeforeSnapshot, direct.Action, direct.Enemies, direct.Random, direct.Contributions, null);
            else if (change == "crit") bad = Frame(direct, hit: Hit(direct, critNull: true));
            else
            {
                if (change == "player") request.PlayerId = null;
                if (change == "attempt") request.AttemptId = "";
                if (change == "operation") request.OperationId = "  ";
                if (change == "actor") request.Actor = null;
                if (change == "pair") request.Pair = null;
                bad = Frame(direct, action: new CandidateAttackSource(request, direct.Action.ActionOrdinal));
            }
            var math = new ExactMathBudget(maxPrimitiveSteps: 0); Reject(bad, MissingField, path, math); Assert.AreEqual(0, math.PrimitiveStepsUsed);
        }

        [TestCase(BattlePhase.AwaitLinks)] [TestCase(BattlePhase.AwaitRescue)]
        [TestCase(BattlePhase.WonPendingSettlement)] [TestCase(BattlePhase.Closed)] [TestCase((BattlePhase)99)]
        public void EnemyPhaseRequiresTheOriginalAwaitActionSource(BattlePhase phase)
        {
            var direct = Direct(Bind(Input(2)));
            Reject(Frame(direct, before: State(direct.BeforeSnapshot, phase: phase)), InvalidPhase, "Before.Phase");
        }

        [TestCase("binding")] [TestCase("revision")] [TestCase("ordinal")] [TestCase("player")]
        [TestCase("attempt")] [TestCase("actor")] [TestCase("pair")] [TestCase("operation")]
        [TestCase("face")] [TestCase("pending")] [TestCase("start")]
        [TestCase("domain")] [TestCase("initial")] [TestCase("mapping")]
        public void SourceBindingsAreExactAndCannotBeSilentlySubstituted(string change)
        {
            var direct = Direct(Bind(Input(2))); var before = direct.BeforeSnapshot; var binding = direct.Binding;
            var request = Request(before); var action = direct.Action; var path = ""; var code = InconsistentBinding;
            switch (change)
            {
                case "binding": binding = Bind(Input(2)); path = "Before.Baseline"; break;
                case "revision": request.ExpectedSceneRevision = 2; action = new CandidateAttackSource(request, 1); path = "Action.ExpectedSceneRevision"; break;
                case "ordinal": action = new CandidateAttackSource(request, 0); path = "Action.ActionOrdinal"; break;
                case "player": request.PlayerId = "Synthetic:player"; action = new CandidateAttackSource(request, 1); path = "Action.PlayerId"; break;
                case "attempt": request.AttemptId += " "; action = new CandidateAttackSource(request, 1); path = "Action.AttemptId"; break;
                case "actor": request.Actor = before.Enemies[0].CombatantKey; action = new CandidateAttackSource(request, 1); path = "Action.Actor"; break;
                case "pair": request.Pair = BattlePairKey.Create(request.AttemptId, "other-face", "P0"); action = new CandidateAttackSource(request, 1); path = "Action.Pair"; break;
                case "operation": request.OperationId += " "; action = new CandidateAttackSource(request, 1); path = "DamageFacts[0].Source"; break;
                case "face": before = State(before, board: new BattleBoardState(Bind(Input(2)).Start.Snapshot.Board.Face)); path = "Before.Board.Face"; break;
                case "pending": before = State(before, board: new BattleBoardState(before.Board.Face, before.Board.LockedRoutes, new[] { before.Enemies[1].PairKey })); code = InvalidPhase; path = "Before.Board.PendingLinks"; break;
                case "start": binding = Binding(binding, start: new CandidateBattleStart(before.Baseline, State(before, actions: 1))); path = "Binding.Start.Snapshot"; break;
                case "domain": binding = Binding(binding, battle: new CandidateRandomDomain(CandidateRandomPurpose.Bonus, binding.Battle.InitState, binding.Battle.InitSequence, binding.Battle.Initial)); path = "Binding.Battle"; break;
                case "initial": binding = Binding(binding, battle: Bind(Input(2), 43).Battle); path = "Binding.Battle"; break;
                case "mapping": binding = Binding(binding, mapping: "UNKNOWN"); code = UnsupportedBinding; path = "Binding.MappingId"; break;
            }
            Reject(Frame(direct, before: before, binding: binding, action: action), code, path);
        }

        [TestCase("memberMissing")] [TestCase("memberDuplicate")] [TestCase("memberKey")]
        [TestCase("memberDefinition")] [TestCase("memberNegative")] [TestCase("memberAboveMax")]
        [TestCase("enemyMissing")] [TestCase("enemyDuplicate")] [TestCase("enemyKey")]
        [TestCase("enemyPair")] [TestCase("enemyDefinition")] [TestCase("enemyNegative")]
        [TestCase("enemyAboveMax")] [TestCase("enemyCursorNegative")] [TestCase("enemyCursorChanged")]
        [TestCase("enemyHpChanged")] [TestCase("totalsMissing")] [TestCase("totalsDuplicate")]
        [TestCase("totalsKey")] [TestCase("totalsNegative")] [TestCase("dealtChanged")] [TestCase("takenChanged")]
        public void CompleteHpCursorAndContributionCoverageCannotBeForged(string change)
        {
            var direct = Direct(Bind(Input(2))); var before = direct.BeforeSnapshot;
            var members = before.Members.ToList(); var enemies = direct.Enemies.ToList(); var totals = direct.Contributions.ToList();
            var code = InconsistentBinding; var path = "";
            switch (change)
            {
                case "memberMissing": members.Clear(); path = "Before.Members"; break;
                case "memberDuplicate": members.Add(members[0]); path = "Before.Members"; break;
                case "memberKey": members[0] = new BattleMemberState(enemies[0].CombatantKey, members[0].Member, R(100)); path = "Before.Members"; break;
                case "memberDefinition": members[0] = new BattleMemberState(members[0].CombatantKey, Bind(Input(2)).Start.Snapshot.Members[0].Member, R(100)); path = "Before.Members"; break;
                case "memberNegative": members[0] = Member(before, R(-1)); code = InvalidValue; path = "Before.Members[0].Hp"; break;
                case "memberAboveMax": members[0] = Member(before, R(101)); code = InvalidValue; path = "Before.Members[0].Hp"; break;
                case "enemyMissing": enemies.Clear(); path = "Enemies"; break;
                case "enemyDuplicate": enemies[1] = enemies[0]; path = "Enemies[1]"; break;
                case "enemyKey": enemies[0] = new BattleEnemyState(BattleCombatantKey.ForEnemy("other-attempt", "face:0", "E0"), enemies[0].PairKey, enemies[0].Enemy, R(100), 0); path = "Enemies[0]"; break;
                case "enemyPair": enemies[0] = new BattleEnemyState(enemies[0].CombatantKey, enemies[1].PairKey, enemies[0].Enemy, R(100), 0); path = "Enemies[0]"; break;
                case "enemyDefinition": enemies[0] = new BattleEnemyState(enemies[0].CombatantKey, enemies[0].PairKey, enemies[1].Enemy, R(100), 0); path = "Enemies[0]"; break;
                case "enemyNegative": enemies[0] = Enemy(enemies[0], R(-1), 0); code = InvalidValue; path = "Enemies[0].Hp"; break;
                case "enemyAboveMax": enemies[0] = Enemy(enemies[0], R(101), 0); code = InvalidValue; path = "Enemies[0].Hp"; break;
                case "enemyCursorNegative": enemies[0] = Enemy(enemies[0], R(100), -1); code = InvalidValue; path = "Enemies[0].IntentCursor"; break;
                case "enemyCursorChanged": enemies[0] = Enemy(enemies[0], R(100), 1); path = "Enemies"; break;
                case "enemyHpChanged": enemies[1] = Enemy(enemies[1], R(99), 0); path = "Enemies"; break;
                case "totalsMissing": totals.Clear(); path = "Contributions"; break;
                case "totalsDuplicate": totals.Add(totals[0]); path = "Contributions"; break;
                case "totalsKey": totals[0] = new BattleContributionTotals(enemies[0].CombatantKey, R(0)); path = "Contributions"; break;
                case "totalsNegative": totals[0] = new BattleContributionTotals(totals[0].CombatantKey, R(-1), R(0)); code = InvalidValue; path = "Contributions"; break;
                case "dealtChanged": totals[0] = new BattleContributionTotals(totals[0].CombatantKey, R(1), R(0)); path = "Contributions"; break;
                case "takenChanged": totals[0] = new BattleContributionTotals(totals[0].CombatantKey, R(0), R(1)); path = "Contributions"; break;
            }
            Reject(Frame(direct, before: State(before, members: members), enemies: enemies, totals: totals), code, path);
        }

        [TestCase("revision")] [TestCase("actions")] [TestCase("phases")] [TestCase("faceLow")] [TestCase("faceHigh")]
        public void InvalidOriginalCountersAndFaceNeverProduceAFragment(string change)
        {
            var direct = Direct(Bind(Input(2))); var before = direct.BeforeSnapshot;
            if (change == "revision") before = State(before, revision: 0);
            if (change == "actions") before = State(before, actions: -1);
            if (change == "phases") before = State(before, phases: -1);
            if (change == "faceLow") before = State(before, faceIndex: -1);
            if (change == "faceHigh") before = State(before, faceIndex: 1);
            Reject(Frame(direct, before: before), InvalidValue, change.StartsWith("face", StringComparison.Ordinal) ? "Before.CurrentFaceIndex" : "Before.Counters");
        }

        [TestCase("memberHp", "Before.Members[0].Hp")] [TestCase("memberRow", "Before.Members[0]")]
        [TestCase("enemyHp", "DirectAttack.Enemies[0].Hp")] [TestCase("enemyRow", "DirectAttack.Enemies[0]")]
        [TestCase("total", "DirectAttack.Contributions[0]")] [TestCase("dealt", "DirectAttack.Contributions[0].Dealt")]
        [TestCase("taken", "DirectAttack.Contributions[0].Taken")]
        public void MissingHpRowsAndAmountsAreNotDefaultedToZero(string change, string path)
        {
            var direct = Direct(Bind(Input(2))); var before = direct.BeforeSnapshot;
            var members = before.Members.ToList(); var enemies = direct.Enemies.ToList(); var totals = direct.Contributions.ToList();
            if (change == "memberHp") members[0] = Member(before, null);
            if (change == "memberRow") members[0] = null;
            if (change == "enemyHp") enemies[0] = Enemy(enemies[0], null, 0);
            if (change == "enemyRow") enemies[0] = null;
            if (change == "total") totals[0] = null;
            if (change == "dealt") totals[0] = new BattleContributionTotals(totals[0].CombatantKey, null, R(0));
            if (change == "taken") totals[0] = new BattleContributionTotals(totals[0].CombatantKey, R(0), null);
            Reject(Frame(direct, before: State(before, members: members), enemies: enemies, totals: totals), MissingField, path);
        }

        [TestCase("randomStream")] [TestCase("prdMissing")] [TestCase("prdDuplicate")]
        [TestCase("prdKey")] [TestCase("prdNegative")] [TestCase("prdChanged")]
        public void Existing009RandomEvidenceMustRemainBoundAndIsNeverResampled(string change)
        {
            var direct = Direct(Bind(Input(2))); var random = direct.Random; var rows = random.PrdStates.ToList();
            var code = InconsistentBinding; var path = "Random.PrdStates";
            if (change == "randomStream") { random = new BattleRandomSnapshot(direct.BeforeSnapshot.Random.Stream, rows); path = "DamageFacts[0].Crit"; }
            else
            {
                if (change == "prdMissing") rows.Clear();
                if (change == "prdDuplicate") rows.Add(rows[0]);
                if (change == "prdKey") rows[0] = new BattlePrdState(direct.Enemies[0].CombatantKey, rows[0].Crit, rows[0].FailureCount);
                if (change == "prdNegative") { rows[0] = new BattlePrdState(rows[0].CombatantKey, rows[0].Crit, -1); code = InvalidValue; path = "Random.PrdStates[0].FailureCount"; }
                if (change == "prdChanged") { rows[0] = new BattlePrdState(rows[0].CombatantKey, rows[0].Crit, 2); path = "Random"; }
                random = new BattleRandomSnapshot(random.Stream, rows);
            }
            Reject(Frame(direct, random: random), code, path);
        }

        [TestCase("features")] [TestCase("carry")] [TestCase("class")] [TestCase("skills")]
        [TestCase("behavior")] [TestCase("cycle")] [TestCase("targeting")] [TestCase("magic")]
        [TestCase("memberEvasion")] [TestCase("enemyEvasion")] [TestCase("chargeDamage")]
        [TestCase("coefficient")] [TestCase("memberContext")]
        public void InternalNegativeProbesCannotExtendTheAcceptedSupportedDomain(string change)
        {
            var valid = Direct(Bind(Input(2))); var input = Input(2); var member = input.Members[0]; var enemy = input.Level.Faces[0].Pairs[0].Enemy;
            var path = ""; var code = UnsupportedBinding;
            switch (change)
            {
                case "features": input.RequiredFeatures.Add("shield"); path = "Baseline.CarryMode"; break;
                case "carry": input.CarryMode = EntryCarryMode.NonEmpty; path = "Baseline.CarryMode"; break;
                case "class": member.ClassKind = (CharacterClassKind)99; path = "Baseline.Members[0]"; break;
                case "skills": member.LearnedSkills.Add("taunt"); path = "Baseline.Members[0]"; break;
                case "behavior": enemy.Behavior = (EnemyBehavior)99; path = "Baseline.Enemy.Behavior"; break;
                case "cycle": enemy.IntentCycle.Clear(); path = "Baseline.Enemy.IntentCycle"; break;
                case "targeting": enemy.IntentCycle[0].Targeting = (EnemyTargeting)99; path = "Baseline.Enemy.IntentCycle"; break;
                case "magic": enemy.IntentCycle[0].DamageKind = (EntryDamageKind)99; path = "Baseline.Enemy.IntentCycle"; break;
                case "memberEvasion": member.Stats.Evasion = R(1, 10); path = "Binding.Start.Baseline.Member.Stats.Evasion"; break;
                case "enemyEvasion": enemy.Stats.Evasion = R(1, 10); path = "Binding.Start.Baseline.Face.Enemy.Stats.Evasion"; break;
                case "chargeDamage": Heavy(input, 0); enemy.IntentCycle[0].DamageCoefficient = R(1); path = "Baseline.Enemy.IntentCycle"; break;
                case "coefficient": enemy.IntentCycle[0].DamageCoefficient = R(0); path = "Baseline.Enemy.IntentCycle"; break;
                case "memberContext": member.StatsContext.RuleVersion += " "; code = InconsistentBinding; path = "Baseline.Members[0].StatsContext"; break;
            }
            // Internal constructors are used only to probe rejected corrupt data; all success paths use public 009.
            var binding = BindPrepared(new PreparedBattleEntry(input), 42); var before = binding.Start.Snapshot;
            var bad = new CandidateCombatFrame(binding, before, new CandidateAttackSource(Request(before), 1), before.Enemies,
                before.Random, before.Contributions, valid.DamageFacts[0]);
            Reject(bad, code, path);
        }

        [TestCase("revision")] [TestCase("actions")] [TestCase("phases")] [TestCase("cursor")]
        [TestCase("hpDenominator")] [TestCase("dealt")] [TestCase("taken")] [TestCase("words")]
        public void SmallBudgetsRecheckEveryRetainedCurrentLargeValue(string change)
        {
            var binding = Bind(Input(2)); var start = binding.Start.Snapshot; var before = start; var huge = BigInteger.One << 80;
            if (change == "revision") before = State(start, revision: huge);
            if (change == "actions") before = State(start, actions: huge);
            if (change == "phases") before = State(start, phases: huge);
            if (change == "cursor") before = State(start, enemies: new[] { Enemy(start.Enemies[0], R(100), huge), start.Enemies[1] });
            if (change == "hpDenominator") before = State(start, members: new[] { Member(start, R(1, huge + 1)) });
            if (change == "dealt" || change == "taken") before = State(start, contributions: new[] { new BattleContributionTotals(start.Members[0].CombatantKey, change == "dealt" ? R(huge) : R(0), change == "taken" ? R(huge) : R(0)) });
            if (change == "words") before = State(start, random: new BattleRandomSnapshot(Pcg32StreamState.Restore(start.Random.Stream.Initial, start.Random.Stream.Current, huge, new ExactMathBudget()), start.Random.PrdStates));
            var direct = Direct(binding, before); Limit(direct, new ExactMathBudget(64), "IntegerBits");
            Assert.AreEqual(Describe(Accept(direct)), Describe(Accept(direct)));
        }

        [TestCase("context")] [TestCase("recommended")] [TestCase("memberLevel")] [TestCase("maxHp")]
        [TestCase("entryHp")] [TestCase("attack")] [TestCase("physical")] [TestCase("magic")]
        [TestCase("critC")] [TestCase("multiplier")] [TestCase("laterEnemy")] [TestCase("coefficient")]
        public void SmallBudgetsRecheckTheFullRetainedBaselineIncludingUnusedFaces(string change)
        {
            var input = Input(1); var member = input.Members[0]; var huge = BigInteger.One << 80;
            if (change == "context") { input.Context.DraftRevision = huge; member.StatsContext.DraftRevision = huge; }
            if (change == "recommended") input.Level.RecommendedLevel = huge;
            if (change == "memberLevel") member.Level = huge;
            if (change == "maxHp" || change == "entryHp") member.Stats.MaxHp = R(huge);
            if (change == "entryHp") member.EntryHp = R(huge);
            if (change == "attack") member.Stats.Attack = R(huge);
            if (change == "physical") member.Stats.PhysicalDefense = R(huge);
            if (change == "magic") member.Stats.MagicDefense = R(huge);
            if (change == "critC") member.Crit.C = R(1, huge + 1);
            if (change == "multiplier") member.Crit.Multiplier = R(huge);
            if (change == "coefficient") input.Level.Faces[0].Pairs[0].Enemy.IntentCycle[0].DamageCoefficient = R(1, huge + 1);
            if (change == "laterEnemy")
            { var face = Input(1).Level.Faces[0]; face.FaceId = "face:later"; face.Pairs[0].Enemy.Stats.Attack = R(huge); input.Level.Faces.Add(face); }
            var direct = Direct(Bind(input)); Limit(direct, new ExactMathBudget(64), "IntegerBits");
            Accept(direct);
        }

        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)]
        public void OneBudgetCoversValidationAndLateExecutionWithoutPublishingPartialResults(int mode)
        {
            var input = Input(mode == 2 ? 1 : 3);
            if (mode == 1) for (var i = 0; i < 3; i++) Heavy(input, i);
            if (mode == 2) input.Members[0].Stats.Attack = R(100);
            var binding = Bind(input); var before = binding.Start.Snapshot;
            if (mode == 3) before = State(before, members: new[] { Member(before, R(1)) });
            var direct = Direct(binding, before); var full = new ExactMathBudget(); var expected = Describe(Accept(direct, full));
            Limit(direct, new ExactMathBudget(maxPrimitiveSteps: 0), "PrimitiveSteps");
            Limit(direct, new ExactMathBudget(maxPrimitiveSteps: (int)full.PrimitiveStepsUsed - 1), "PrimitiveSteps");
            var shared = new ExactMathBudget(maxPrimitiveSteps: (int)full.PrimitiveStepsUsed);
            ExactRational.Create(1, 2, shared); Limit(direct, shared, "PrimitiveSteps");
            Assert.AreEqual(expected, Describe(Accept(direct)));
        }

        [TestCase("phase")] [TestCase("laterCursor")] [TestCase("damage")]
        public void ArithmeticOverflowCannotPublishEarlierLocalEnemyResults(string change)
        {
            var input = Input(2); var max = (BigInteger.One << 64) - 1;
            if (change == "damage") { input.Level.Faces[0].Pairs[1].Enemy.Stats.Attack = R(BigInteger.One << 63); input.Level.Faces[0].Pairs[1].Enemy.IntentCycle[0].DamageCoefficient = R(2); }
            var binding = Bind(input); var before = binding.Start.Snapshot;
            if (change == "phase") before = State(before, phases: max);
            if (change == "laterCursor") before = State(before, enemies: new[] { before.Enemies[0], Enemy(before.Enemies[1], R(100), max) });
            var direct = Direct(binding, before); Limit(direct, new ExactMathBudget(64), "IntegerBits"); Accept(direct);
        }

        private static CandidateEnemyPhaseFrame Accept(CandidateCombatFrame direct, ExactMathBudget math = null)
        {
            var original = Describe(direct); var result = CandidateEnemyPhase.Evaluate(direct, math ?? new ExactMathBudget());
            Assert.IsTrue(result.IsAccepted, result.RejectionCode + " " + result.FieldPath);
            Assert.IsNull(result.RejectionCode); Assert.IsNull(result.FieldPath); Assert.IsNotNull(result.Frame);
            Assert.AreEqual(original, Describe(direct)); Assert.AreSame(direct.Random, result.Frame.Random);
            return result.Frame;
        }
        private static void Reject(CandidateCombatFrame direct, CandidateEnemyPhaseRejectionCode code, string path, ExactMathBudget math = null)
        {
            var original = Describe(direct); var result = CandidateEnemyPhase.Evaluate(direct, math ?? new ExactMathBudget());
            Assert.IsFalse(result.IsAccepted); Assert.IsNull(result.Frame); Assert.AreEqual(code, result.RejectionCode); Assert.AreEqual(path, result.FieldPath);
            Assert.AreEqual(original, Describe(direct));
        }
        private static void Limit(CandidateCombatFrame direct, ExactMathBudget math, string reason)
        {
            var original = Describe(direct); CandidateEnemyPhaseResult result = null;
            var error = Assert.Throws<ExactMathLimitException>(() => result = CandidateEnemyPhase.Evaluate(direct, math));
            Assert.AreEqual(reason, error.ReasonCode); Assert.IsNull(result); Assert.AreEqual(original, Describe(direct));
        }
        private static void AssertFacts(CandidateEnemyPhaseFrame result)
        {
            var direct = result.DirectAttack; var before = direct.BeforeSnapshot;
            for (var i = 0; i < result.OrderedIntents.Count; i++)
            {
                var fact = result.OrderedIntents[i]; var enemy = direct.Enemies.Single(e => e.CombatantKey.Equals(fact.EnemyKey));
                Assert.AreSame(before.Baseline, fact.Baseline); Assert.AreEqual(direct.Action.PlayerId, fact.PlayerId);
                Assert.AreEqual(direct.Action.AttemptId, fact.AttemptId); Assert.AreEqual(direct.Action.OperationId, fact.OperationId);
                Assert.AreEqual(before.Board.Face.FaceId, fact.FaceId); Assert.AreEqual(before.SceneRevision, fact.SceneRevision);
                Assert.AreEqual(direct.Action.ActionOrdinal, fact.ActionOrdinal); Assert.AreEqual(result.EnemyPhaseOrdinal, fact.EnemyPhaseOrdinal);
                Assert.AreEqual(i, fact.SegmentIndex); Assert.AreEqual(enemy.PairKey, fact.Pair); Assert.AreEqual(enemy.StableOrder, fact.StableOrder);
                Assert.AreEqual(enemy.IntentCursor, fact.CursorBefore); Assert.AreEqual(enemy.IntentCursor + 1, fact.CursorAfter);
                if (fact.Damage != null) Assert.AreEqual(fact.EnemyKey, fact.Damage.ActorEnemy);
            }
            Assert.AreSame(direct.Random, result.Random); Assert.AreSame(direct.DamageFacts[0], result.DirectAttack.DamageFacts[0]);
        }
        private static CandidateCombatFrame Direct(CandidateRandomBinding binding, BattleSnapshot before = null, CandidateAttackRequest request = null)
        {
            before = before ?? binding.Start.Snapshot; request = request ?? Request(before);
            var result = CandidateDirectAttack.Evaluate(binding, before, request, new RandomSamplingBudget(new ExactMathBudget()));
            Assert.IsTrue(result.IsAccepted, result.RejectionCode + " " + result.FieldPath + " " + result.RouteReasonCode); return result.Frame;
        }
        private static CandidateAttackRequest Request(BattleSnapshot before, List<FlowPos> route = null)
        {
            var pair = before.Board.Face.Pairs.OrderBy(p => p.Enemy.OriginalSlot).First(p => before.Enemies.Single(e => e.PairKey.PairId == p.PairId).Hp.Numerator.Sign > 0);
            return new CandidateAttackRequest { PlayerId = before.Baseline.Entry.PlayerId, AttemptId = before.Baseline.Entry.AttemptId,
                OperationId = "synthetic:attack", ExpectedSceneRevision = before.SceneRevision, Actor = before.Members[0].CombatantKey,
                Pair = BattlePairKey.Create(before.Baseline.Entry.AttemptId, before.Board.Face.FaceId, pair.PairId), Route = route ?? Route(pair) };
        }
        private static List<FlowPos> Route(PreparedPair pair)
        { return new List<FlowPos> { pair.EndpointA, new FlowPos(1, pair.EndpointA.y), new FlowPos(2, pair.EndpointA.y), pair.EndpointB }; }
        private static CandidateRandomBinding Bind(BattleEntryInput input, ulong seed = 42)
        {
            var result = new BattleEntryPreparer().PrepareCandidate(input, new ExactMathBudget());
            Assert.IsTrue(result.IsAccepted, result.RejectionCode + " " + result.FieldPath); return BindPrepared(result.Entry, seed);
        }
        private static CandidateRandomBinding BindPrepared(PreparedBattleEntry entry, ulong seed)
        {
            var bytes = new byte[48];
            for (var offset = 0; offset < 48; offset += 16)
                for (var i = 0; i < 8; i++) { bytes[offset + i] = (byte)(seed >> (8 * i)); bytes[offset + 8 + i] = (byte)(54UL >> (8 * i)); }
            var result = CandidateRandomPreparer.Prepare(entry, new CandidateSeedMaterial { Bytes = bytes,
                SourceCapabilityId = "synthetic-enemy-phase-test", MappingId = CandidateRandomPreparer.SupportedMappingId }, new ExactMathBudget());
            Assert.IsTrue(result.IsAccepted, result.RejectionCode + " " + result.FieldPath); return result.Binding;
        }
        private static CandidateRandomBinding Binding(CandidateRandomBinding source, CandidateBattleStart start = null, CandidateRandomDomain battle = null, string mapping = null)
        { return new CandidateRandomBinding(start ?? source.Start, source.SourceCapabilityId, mapping ?? source.MappingId, battle ?? source.Battle, source.BaseReward, source.Bonus); }
        private static BattleSnapshot State(BattleSnapshot source, IEnumerable<BattleMemberState> members = null,
            IEnumerable<BattleEnemyState> enemies = null, IEnumerable<BattleContributionTotals> contributions = null,
            BattleBoardState board = null, BattleRandomSnapshot random = null, BigInteger? revision = null,
            BigInteger? actions = null, BigInteger? phases = null, int? faceIndex = null, BattlePhase? phase = null)
        { return new BattleSnapshot(source.Baseline, revision ?? source.SceneRevision, actions ?? source.EffectiveActionsCompleted,
            phases ?? source.EnemyPhasesCompleted, faceIndex ?? source.CurrentFaceIndex, phase ?? source.Phase, board ?? source.Board,
            members ?? source.Members, enemies ?? source.Enemies, random ?? source.Random, contributions ?? source.Contributions); }
        private static BattleMemberState Member(BattleSnapshot source, ExactRational hp)
        { return new BattleMemberState(source.Members[0].CombatantKey, source.Members[0].Member, hp); }
        private static BattleEnemyState Enemy(BattleEnemyState source, ExactRational hp, BigInteger cursor)
        { return new BattleEnemyState(source.CombatantKey, source.PairKey, source.Enemy, hp, cursor); }
        private static CandidateCombatFrame Frame(CandidateCombatFrame source, BattleSnapshot before = null, CandidateRandomBinding binding = null,
            CandidateAttackSource action = null, IEnumerable<BattleEnemyState> enemies = null, IEnumerable<BattleContributionTotals> totals = null,
            BattleRandomSnapshot random = null, BattleDamageFact hit = null)
        { return new CandidateCombatFrame(binding ?? source.Binding, before ?? source.BeforeSnapshot, action ?? source.Action,
            enemies ?? source.Enemies, random ?? source.Random, totals ?? source.Contributions, hit ?? source.DamageFacts[0]); }
        private static BattleDamageFact Hit(CandidateCombatFrame source, bool critNull)
        {
            var h = source.DamageFacts[0]; var target = source.BeforeSnapshot.Enemies.Single(e => e.CombatantKey.Equals(h.Target));
            return new BattleDamageFact(source.BeforeSnapshot, source.Action, target, h.Attack, h.Multiplier, h.RawDamage,
                h.MitigatedDamage, h.RoundedDamage, h.HpAfter, h.HpLoss, h.Overflow, h.BlockPrevented, critNull ? null : h.Crit);
        }
        private static BattleEntryInput Input(int count)
        {
            var context = new CandidateContext { DraftId = "synthetic:enemy-phase", DraftRevision = 1, ContentFingerprint = "synthetic:010",
                RuleVersion = "candidate-r1", NumericContractVersion = "exact-r1", RandomContractVersion = "pcg-r1",
                SourceNotes = new List<string> { "isolated enemy-phase vector; synthetic C=1/4 and explicit state overrides; no production or full-history proof" } };
            var face = new FaceInput { FaceId = "face:0", Width = 4, Height = count * 2 - 1, Pairs = new List<PairInput>() };
            for (var i = 0; i < count; i++) face.Pairs.Add(new PairInput { PairId = "P" + i, GeometryColorId = i,
                EndpointA = new FlowPos(0, i * 2), EndpointB = new FlowPos(3, i * 2),
                Enemy = new EnemyInput { EnemyInstanceKey = "E" + i, EnemyDefinitionId = "E01", OriginalSlot = i * 3, StableOrder = i,
                    Behavior = EnemyBehavior.NormalStrike, Stats = Stats(100, 10, 0, 0), IntentCycle = new List<EnemyIntentInput> { Strike(3, 5) } } });
            return new BattleEntryInput { PlayerId = "synthetic:player", ChallengeId = "synthetic:challenge", AttemptId = "synthetic:attempt",
                EntryBaselineId = "synthetic:baseline", Context = context, CarryMode = EntryCarryMode.Empty, RequiredFeatures = new List<string>(),
                Level = new LevelInput { LevelId = "synthetic:level", LevelVersion = "candidate-r1", RecommendedLevel = 1, Faces = new List<FaceInput> { face } },
                Members = new List<MemberInput> { new MemberInput { CharacterId = "synthetic:warrior", ClassId = "warrior", ClassKind = CharacterClassKind.Warrior,
                    OriginalSlot = 2, Level = 1, IsReady = true, StatsOrigin = BaseStatsOrigin.ComputedBaseStats, StatsContext = Context(context),
                    Stats = Stats(100, 0, 10, 6), EntryHp = R(100), LearnedSkills = new List<string>(),
                    Crit = new WarriorCritInput { PassiveDefinitionId = "warrior:crit", TargetProbability = R(1, 5), C = R(1, 4), Multiplier = R(3, 2) } } } };
        }
        private static void Heavy(BattleEntryInput input, int index)
        {
            var enemy = input.Level.Faces[0].Pairs[index].Enemy; enemy.Behavior = EnemyBehavior.ChargeHeavy; enemy.EnemyDefinitionId = "E02";
            enemy.IntentCycle = new List<EnemyIntentInput> { new EnemyIntentInput { Kind = EnemyIntentKind.Charge, Targeting = EnemyTargeting.FirstLiving,
                DamageKind = null, DamageCoefficient = null }, Strike(13, 10) };
        }
        private static EnemyIntentInput Strike(int numerator, int denominator)
        { return new EnemyIntentInput { Kind = EnemyIntentKind.Strike, Targeting = EnemyTargeting.FirstLiving, DamageKind = EntryDamageKind.Physical, DamageCoefficient = R(numerator, denominator) }; }
        private static BattleEntryInput SourceCandidate(int stage, int direction, out List<FlowPos> route)
        {
            var fixture = DemoContentFixture.CaptureSource(stage, 1, (SourceCoordinateConvention)direction);
            var geometry = fixture.CopyLevel(); var input = Input(2); var face = input.Level.Faces[0];
            input.Level.LevelId = fixture.FixtureKey; face.Width = geometry.width; face.Height = geometry.height;
            input.Members[0].Stats.Attack = R(20);
            input.Context.SourceNotes.Add(fixture.SourcePath + ":" + fixture.SourceLocator + ":" + fixture.SourceSha256);
            input.Context.SourceNotes.Add(fixture.CoordinateTransform + "; conditional no-crit sample produced by public 009, not calibrated production PRD");
            input.Members[0].StatsContext = Context(input.Context);
            foreach (var source in fixture.Bindings)
            {
                var pair = face.Pairs[source.ColorId]; var g = geometry.pairs.Single(p => p.colorId == source.ColorId);
                pair.PairId = source.SourcePair; pair.EndpointA = g.endpointA; pair.EndpointB = g.endpointB;
                pair.Enemy.EnemyInstanceKey = source.SourcePair; pair.Enemy.OriginalSlot = source.OriginalSlot;
                if (source.EnemyAlias == "E02") Heavy(input, source.ColorId);
                pair.Enemy.Stats = Stats(source.EnemyAlias == "E02" ? 20 : 15, 10, source.EnemyAlias == "E02" ? 20 : 0, 0);
            }
            route = new List<FlowPos>(fixture.CopySolution().paths.Single(p => p.colorId == 0).cells); return input;
        }
        private static CandidateContext Context(RuleContext c)
        { return new CandidateContext { DraftId = c.DraftId, DraftRevision = c.DraftRevision, ContentFingerprint = c.ContentFingerprint,
            RuleVersion = c.RuleVersion, NumericContractVersion = c.NumericContractVersion, RandomContractVersion = c.RandomContractVersion, SourceNotes = new List<string>(c.SourceNotes) }; }
        private static StatsInput Stats(int hp, int attack, int physical, int magic)
        { return new StatsInput { MaxHp = R(hp), Attack = R(attack), PhysicalDefense = R(physical), MagicDefense = R(magic), Evasion = R(0), AttackRange = 1 }; }
        private static ExactRational R(BigInteger numerator) { return R(numerator, BigInteger.One); }
        private static ExactRational R(BigInteger numerator, BigInteger denominator) { return ExactRational.Create(numerator, denominator, new ExactMathBudget()); }
        private static void Value(ExactRational actual, BigInteger numerator, int denominator = 1)
        { Assert.AreEqual(numerator, actual.Numerator); Assert.AreEqual(new BigInteger(denominator), actual.Denominator); }
        private static string Describe(object value)
        {
            if (value == null) return "null";
            if (value is string text) return text.Length + ":" + text;
            if (value is ExactRational r) return r.Numerator + "/" + r.Denominator;
            if (value is FlowPos cell) return cell.x + "," + cell.y;
            if (value.GetType().IsValueType) return value.ToString();
            if (value is IEnumerable items) return "[" + string.Join(";", items.Cast<object>().Select(Describe)) + "]";
            return "{" + string.Join(";", value.GetType().GetProperties().OrderBy(p => p.Name, StringComparer.Ordinal).Select(p => p.Name + "=" + Describe(p.GetValue(value)))) + "}";
        }
        private static void AssertReadOnlyGraph(object value)
        {
            if (value == null || value is string || value is ExactRational || value.GetType().IsValueType) return;
            if (value is IList list)
            {
                Assert.IsTrue(list.IsReadOnly); Assert.Throws<NotSupportedException>(() => list.Clear());
                if (list.Count > 0) Assert.Throws<NotSupportedException>(() => list[0] = list[0]);
                foreach (var row in list) AssertReadOnlyGraph(row); return;
            }
            Assert.IsEmpty(value.GetType().GetConstructors(BindingFlags.Public | BindingFlags.Instance));
            foreach (var property in value.GetType().GetProperties()) { Assert.IsFalse(property.CanWrite, property.Name); AssertReadOnlyGraph(property.GetValue(value)); }
        }
    }
}
