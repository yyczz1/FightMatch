using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Reflection;
using FlowPuzzle.Core;
using NUnit.Framework;
using static FightMatch.Core.CandidateAttackRejectionCode;

namespace FightMatch.Core.Tests
{
    [TestFixture]
    public class CandidateDirectAttackTests
    {
        [TestCase(1, 1)]
        [TestCase(1, 2)]
        [TestCase(3, 1)]
        [TestCase(3, 2)]
        public void SourceCandidates_UseRealFirstOpportunityAndRetainExactDamageSource(int stage, int direction)
        {
            var input = Candidate(stage, direction, out var fixture);
            var binding = Bind(input); var before = binding.Start.Snapshot;
            var request = Request(before, fixture);
            var result = Accept(binding, before, request);
            var fact = result.Fact; var frame = result.Frame;
            Assert.AreSame(before.Baseline, fact.Baseline); Assert.AreSame(binding, frame.Binding);
            Assert.AreEqual(input.PlayerId, fact.PlayerId); Assert.AreEqual(input.AttemptId, fact.AttemptId);
            Assert.AreEqual(before.Board.Face.FaceId, fact.FaceId); Assert.AreEqual(request.OperationId, fact.OperationId);
            Assert.AreEqual(before.SceneRevision, fact.SceneRevision); Assert.AreEqual(BigInteger.One, fact.ActionOrdinal);
            Assert.AreEqual(request.Actor, fact.Actor); Assert.AreEqual(request.Pair, fact.Pair);
            Assert.AreEqual(before.Enemies[0].CombatantKey, fact.Target);
            Assert.AreEqual(0, fact.EffectIndex); Assert.AreEqual(EntryDamageKind.Physical, fact.DamageKind);
            Value(fact.Attack, 20); Value(fact.PhysicalDefense, stage == 1 ? 0 : 20); Value(fact.Multiplier, 1);
            Value(fact.RawDamage, 20); Value(fact.MitigatedDamage, stage == 1 ? 20 : 50, stage == 1 ? 1 : 3);
            Assert.AreEqual(new BigInteger(stage == 1 ? 20 : 16), fact.RoundedDamage);
            Value(fact.HpBefore, stage == 1 ? 15 : 20); Value(fact.HpAfter, stage == 1 ? 0 : 4);
            Value(fact.HpLoss, stage == 1 ? 15 : 16); Value(fact.Overflow, stage == 1 ? 5 : 0);
            Assert.AreEqual(stage == 1, fact.DefeatedTarget); Value(fact.BlockPrevented, 0); Value(fact.ShieldAbsorbed, 0);
            Assert.IsFalse(fact.Crit.Triggered); Value(fact.Crit.Probability, 1, 4);
            Assert.AreEqual(BigInteger.Zero, fact.Crit.OpportunityOrdinal);
            Assert.AreSame(before.Members[0].Member.Crit, fact.Crit.Parameters);
            CollectionAssert.AreEqual(new uint[] { 0xa15c02b7 }, fact.Crit.Words);
            Assert.AreEqual(BigInteger.One, frame.Random.Stream.WordsConsumed);
            Assert.AreEqual(BigInteger.One, frame.Random.PrdStates[0].FailureCount);
            Assert.AreEqual(fact.Actor, fact.Crit.Actor); Assert.AreEqual(fact.Target, fact.Crit.Target);
            Assert.AreSame(before, frame.BeforeSnapshot); Assert.AreSame(before.Members[0], frame.Members[0]);
            Assert.AreSame(before.Enemies[1], frame.Enemies[1]); Assert.AreSame(fact, frame.DamageFacts.Single());
            Value(frame.Members[0].Hp, 100); Value(frame.Contributions[0].EffectiveDamageDealtHp, stage == 1 ? 15 : 16);
            Assert.AreEqual(BigInteger.Zero, frame.Enemies[0].IntentCursor);
            Assert.IsEmpty(before.Board.LockedRoutes); Assert.IsEmpty(before.Board.PendingLinks);
            Assert.AreEqual(BattlePhase.AwaitAction, before.Phase); Assert.AreEqual(BigInteger.One, before.SceneRevision);
            Assert.AreEqual(BigInteger.Zero, before.EffectiveActionsCompleted); Assert.AreEqual(BigInteger.Zero, before.EnemyPhasesCompleted);
            Assert.IsTrue(binding.Context.SourceNotes.Contains(fixture.CoordinateTransform));
            Assert.IsTrue(binding.Context.SourceNotes.Any(n => n.Contains("synthetic C=1/4")));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void CertainCrit_UsesOriginalMultiplierAndZeroWordsInEitherRouteDirection(bool reverse)
        {
            var input = Candidate(3, 1, out var fixture); input.Members[0].Crit.C = R(1);
            var binding = Bind(input); var before = binding.Start.Snapshot; var request = Request(before, fixture);
            if (reverse) request.Route.Reverse();
            var budget = new RandomSamplingBudget(new ExactMathBudget(), 0);
            var result = Accept(binding, before, request, budget);
            Assert.IsTrue(result.Fact.Crit.Triggered); Assert.IsEmpty(result.Fact.Crit.Words);
            Value(result.Fact.Multiplier, 3, 2); Value(result.Fact.RawDamage, 30); Value(result.Fact.MitigatedDamage, 25);
            Assert.AreEqual(new BigInteger(25), result.Fact.RoundedDamage); Value(result.Fact.HpLoss, 20); Value(result.Fact.Overflow, 5);
            Assert.AreEqual(0, budget.WordsUsed); Assert.AreEqual(BigInteger.Zero, result.Frame.Random.PrdStates[0].FailureCount);
            Assert.AreEqual(Describe(before.Random.Stream), Describe(result.Frame.Random.Stream));
        }

        [TestCase(0)]
        [TestCase(1)]
        public void FloorZeroStillProducesARealHitOpportunityWithoutMinimumDamage(int attack)
        {
            var input = Candidate(1, 1, out var fixture);
            input.Members[0].Stats.Attack = R(attack); input.Level.Faces[0].Pairs[0].Enemy.Stats.PhysicalDefense = R(500);
            var binding = Bind(input); var before = binding.Start.Snapshot;
            var result = Accept(binding, before, Request(before, fixture));
            Assert.AreEqual(BigInteger.Zero, result.Fact.RoundedDamage); Value(result.Fact.HpLoss, 0); Value(result.Fact.Overflow, 0);
            Value(result.Fact.HpAfter, 15); Value(result.Frame.Contributions[0].EffectiveDamageDealtHp, 0);
            CollectionAssert.AreEqual(new uint[] { 0xa15c02b7 }, result.Fact.Crit.Words);
            Assert.AreEqual(BigInteger.One, result.Frame.Random.PrdStates[0].FailureCount);
            Assert.IsFalse(result.Fact.DefeatedTarget);
        }

        [Test]
        public void DamageFloorsOnlyAfterAllExactFactors()
        {
            var input = Candidate(3, 1, out var fixture);
            input.Members[0].Stats.Attack = R(5, 2); input.Members[0].Crit.C = R(1);
            var binding = Bind(input); var before = binding.Start.Snapshot;
            var result = Accept(binding, before, Request(before, fixture));
            Value(result.Fact.RawDamage, 15, 4); Value(result.Fact.MitigatedDamage, 25, 8);
            Assert.AreEqual(new BigInteger(3), result.Fact.RoundedDamage); Value(result.Fact.HpAfter, 17);
        }

        [Test]
        public void FractionalCurrentHpCapsActualLossWithoutRoundingTheHp()
        {
            var input = Candidate(1, 1, out var fixture); var binding = Bind(input); var initial = binding.Start.Snapshot;
            var enemies = initial.Enemies.ToList(); enemies[0] = Enemy(enemies[0], R(5, 2), 7);
            var before = State(initial, enemies: enemies);
            var result = Accept(binding, before, Request(before, fixture));
            Assert.AreEqual(new BigInteger(20), result.Fact.RoundedDamage);
            Value(result.Fact.HpBefore, 5, 2); Value(result.Fact.HpLoss, 5, 2); Value(result.Fact.Overflow, 35, 2);
            Value(result.Fact.HpAfter, 0); Value(result.Frame.Contributions[0].EffectiveDamageDealtHp, 5, 2);
            Assert.AreEqual(new BigInteger(7), result.Frame.Enemies[0].IntentCursor);
            Assert.IsTrue(result.Fact.DefeatedTarget);
        }

        [Test]
        public void RealSecondAttackAddsOnlyNewHpLossToExistingHistoryAndRemainsUnfinished()
        {
            var input = Candidate(3, 1, out var fixture); var binding = Bind(input); var initial = binding.Start.Snapshot;
            var first = Accept(binding, initial, Request(initial, fixture));
            // Explicit valid prior completed action: E02 charged; E01 dealt five HP in the enemy phase.
            var members = new[] { new BattleMemberState(initial.Members[0].CombatantKey, initial.Members[0].Member, R(95)) };
            var enemies = first.Frame.Enemies.Select(e => Enemy(e, e.Hp, 1)).ToList();
            var contributions = new[] { new BattleContributionTotals(members[0].CombatantKey, R(16), R(5)) };
            var before = State(initial, members: members, enemies: enemies, random: first.Frame.Random,
                contributions: contributions, revision: 2, actions: 1, phases: 1);
            var result = Accept(binding, before, Request(before, fixture));
            Value(result.Fact.HpBefore, 4); Value(result.Fact.HpLoss, 4); Value(result.Fact.Overflow, 12);
            Value(result.Frame.Contributions[0].EffectiveDamageDealtHp, 20); Value(result.Frame.Contributions[0].EffectiveDamageTakenHp, 5);
            Value(result.Frame.Members[0].Hp, 95); Assert.AreSame(before.Members[0], result.Frame.Members[0]);
            Assert.AreSame(before.Enemies[1], result.Frame.Enemies[1]); Assert.AreSame(before.Enemies[0].Enemy, result.Frame.Enemies[0].Enemy);
            Assert.AreEqual(0, result.Frame.Enemies[0].OriginalSlot); Assert.AreEqual(2, result.Frame.Members[0].OriginalSlot);
            Assert.AreEqual(BigInteger.One, result.Frame.Enemies[0].IntentCursor);
            Assert.AreEqual(new BigInteger(2), result.Fact.ActionOrdinal); Assert.AreEqual(BigInteger.One, result.Fact.Crit.OpportunityOrdinal);
            CollectionAssert.AreEqual(new uint[] { 0x7b47f409 }, result.Fact.Crit.Words);
            Assert.AreEqual(new BigInteger(2), result.Frame.Random.PrdStates[0].FailureCount);
            Assert.AreSame(before, result.Frame.BeforeSnapshot); Assert.IsEmpty(before.Board.LockedRoutes);
            Assert.AreEqual(BattlePhase.AwaitAction, before.Phase); Assert.AreEqual(new BigInteger(2), before.SceneRevision);
            Assert.AreEqual(BigInteger.One, before.EffectiveActionsCompleted); Assert.AreEqual(BigInteger.One, before.EnemyPhasesCompleted);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void LivingOriginalSlotOrderIgnoresPairOrderAndStableOrderAndSkipsDeadFront(bool reversePairs)
        {
            var input = Candidate(1, 1, out var fixture);
            input.Level.Faces[0].Pairs[0].Enemy.StableOrder = 9;
            input.Level.Faces[0].Pairs[1].Enemy.OriginalSlot = 5; input.Level.Faces[0].Pairs[1].Enemy.StableOrder = 0;
            if (reversePairs) input.Level.Faces[0].Pairs.Reverse();
            var binding = Bind(input); var initial = binding.Start.Snapshot; var request = Request(initial, fixture, 1);
            Reject(binding, initial, request, OutOfRange, "Pair");
            var first = Accept(binding, initial, Request(initial, fixture));
            var deadFront = first.Frame.Enemies.Select(e => Enemy(e, e.Hp, e.Hp.Numerator.IsZero ? 0 : 1)).ToList();
            var board = new BattleBoardState(initial.Board.Face,
                new[] { new BattleLockedRoute(Request(initial, fixture).Pair, Request(initial, fixture).Route) }, Array.Empty<BattlePairKey>());
            var members = new[] { new BattleMemberState(initial.Members[0].CombatantKey, initial.Members[0].Member, R(95)) };
            var totals = new[] { new BattleContributionTotals(members[0].CombatantKey, R(15), R(5)) };
            var before = State(initial, enemies: deadFront, board: board, random: first.Frame.Random,
                members: members, contributions: totals, revision: 2, actions: 1, phases: 1);
            var result = Accept(binding, before, Request(before, fixture, 1));
            Assert.AreEqual(5, result.Frame.Enemies.Single(e => e.PairKey.PairId == "B").OriginalSlot);
            Assert.AreEqual(0, result.Frame.Enemies.Single(e => e.PairKey.PairId == "B").StableOrder);
            Assert.IsTrue(result.Frame.Enemies.All(e => e.Hp.Numerator.IsZero));
            Assert.AreSame(board, result.Frame.BeforeSnapshot.Board); Assert.AreEqual(1, board.LockedRoutes.Count);
            Assert.AreEqual(BattlePhase.AwaitAction, result.Frame.BeforeSnapshot.Phase);
            Assert.AreEqual(new BigInteger(2), result.Frame.BeforeSnapshot.SceneRevision);
            Assert.AreEqual(BigInteger.One, result.Frame.BeforeSnapshot.EffectiveActionsCompleted);
        }

        [TestCase("binding")]
        [TestCase("before")]
        [TestCase("request")]
        [TestCase("budget")]
        public void NullRootsThrowArgumentErrorsBeforeOtherWork(string parameter)
        {
            var binding = Bind(Candidate(1, 1, out var fixture)); var before = binding.Start.Snapshot;
            var ex = Assert.Throws<ArgumentNullException>(() => CandidateDirectAttack.Evaluate(
                parameter == "binding" ? null : binding, parameter == "before" ? null : before,
                parameter == "request" ? null : Request(before, fixture), parameter == "budget" ? null : new RandomSamplingBudget(new ExactMathBudget())));
            Assert.AreEqual(parameter, ex.ParamName);
        }

        [TestCase("PlayerId")]
        [TestCase("AttemptId")]
        [TestCase("OperationId")]
        [TestCase("ExpectedSceneRevision")]
        [TestCase("Actor")]
        [TestCase("Pair")]
        [TestCase("Route")]
        public void EveryRequestFieldMustBeExplicitBeforeNumericWork(string field)
        {
            var binding = Bind(Candidate(1, 1, out var fixture)); var before = binding.Start.Snapshot; var request = Request(before, fixture);
            typeof(CandidateAttackRequest).GetProperty(field).SetValue(request, null);
            Reject(binding, before, request, MissingField, field, new RandomSamplingBudget(new ExactMathBudget(64, 0)));
        }

        [TestCase("player", InconsistentBinding, "PlayerId")]
        [TestCase("attempt", InconsistentBinding, "AttemptId")]
        [TestCase("actor-attempt", InconsistentBinding, "Actor")]
        [TestCase("actor-case", InconsistentBinding, "Actor")]
        [TestCase("actor-enemy", InconsistentBinding, "Actor")]
        [TestCase("pair-attempt", InconsistentBinding, "Pair")]
        [TestCase("pair-name", TargetUnavailable, "Pair")]
        [TestCase("pair-face", TargetUnavailable, "Pair")]
        [TestCase("stale", StaleContext, "ExpectedSceneRevision")]
        [TestCase("negative", InvalidValue, "ExpectedSceneRevision")]
        public void IdentitiesUseExactComponentsAndRevision(string fault, CandidateAttackRejectionCode code, string path)
        {
            var binding = Bind(Candidate(1, 1, out var fixture)); var before = binding.Start.Snapshot; var request = Request(before, fixture);
            switch (fault)
            {
                case "player": request.PlayerId += " "; break;
                case "attempt": request.AttemptId = request.AttemptId.ToUpperInvariant(); break;
                case "actor-attempt": request.Actor = BattleCombatantKey.ForParticipant("other", request.Actor.CharacterId); break;
                case "actor-case": request.Actor = BattleCombatantKey.ForParticipant(request.AttemptId, request.Actor.CharacterId.ToUpperInvariant()); break;
                case "actor-enemy": request.Actor = before.Enemies[0].CombatantKey; break;
                case "pair-attempt": request.Pair = BattlePairKey.Create("other", request.Pair.FaceId, request.Pair.PairId); break;
                case "pair-name": request.Pair = BattlePairKey.Create(request.AttemptId, request.Pair.FaceId, "a"); break;
                case "pair-face": request.Pair = BattlePairKey.Create(request.AttemptId, "other", request.Pair.PairId); break;
                case "stale": request.ExpectedSceneRevision++; break;
                case "negative": request.ExpectedSceneRevision = -1; break;
            }
            Reject(binding, before, request, code, path);
        }

        [Test]
        public void IndependentCandidateWithIdenticalDisplayIdsIsNotTheSameBaseline()
        {
            var binding = Bind(Candidate(1, 1, out var fixture)); var other = Bind(Candidate(1, 1, out _));
            Assert.AreEqual(Describe(binding.Start.Baseline), Describe(other.Start.Baseline));
            Reject(binding, other.Start.Snapshot, Request(other.Start.Snapshot, fixture), InconsistentBinding, "Before.Baseline");
        }

        [TestCase(BattlePhase.AwaitLinks)]
        [TestCase(BattlePhase.AwaitRescue)]
        [TestCase(BattlePhase.WonPendingSettlement)]
        [TestCase(BattlePhase.Closed)]
        public void InactivePhasesRejectBeforeSampling(BattlePhase phase)
        {
            var binding = Bind(Candidate(1, 1, out var fixture)); var before = State(binding.Start.Snapshot, phase: phase);
            Reject(binding, before, Request(before, fixture), InvalidPhase, "Before.Phase");
        }

        [TestCase(true)]
        [TestCase(false)]
        public void DeadActorOrTargetCannotCreateAnOpportunity(bool deadActor)
        {
            var binding = Bind(Candidate(1, 1, out var fixture)); var initial = binding.Start.Snapshot;
            var members = initial.Members.ToList(); var enemies = initial.Enemies.ToList();
            if (deadActor) members[0] = new BattleMemberState(members[0].CombatantKey, members[0].Member, R(0));
            else enemies[0] = Enemy(enemies[0], R(0), 0);
            var before = State(initial, members: members, enemies: enemies);
            Reject(binding, before, Request(before, fixture), deadActor ? ActorUnavailable : TargetUnavailable, deadActor ? "Actor" : "Pair");
        }

        [Test]
        public void LaterDefinedFaceIsNotAnAttackTargetUntilItIsCurrent()
        {
            var input = Candidate(1, 1, out var fixture); var later = Candidate(3, 1, out _).Level.Faces[0];
            later.FaceId = "later"; input.Level.Faces.Add(later);
            var binding = Bind(input); var before = binding.Start.Snapshot; var request = Request(before, fixture);
            request.Pair = BattlePairKey.Create(input.AttemptId, "later", "A");
            Reject(binding, before, request, TargetUnavailable, "Pair");
        }

        [TestCase("member-count", InconsistentBinding, "Before.Members")]
        [TestCase("member-key", InconsistentBinding, "Before.Members[0]")]
        [TestCase("member-definition", InconsistentBinding, "Before.Members[0]")]
        [TestCase("member-negative-hp", InvalidValue, "Before.Members[0].Hp")]
        [TestCase("member-excess-hp", InvalidValue, "Before.Members[0].Hp")]
        [TestCase("enemy-count", InconsistentBinding, "Before.Enemies")]
        [TestCase("enemy-key", InconsistentBinding, "Before.Enemies[0]")]
        [TestCase("enemy-pair", InconsistentBinding, "Before.Enemies[0]")]
        [TestCase("enemy-definition", InconsistentBinding, "Before.Enemies[0]")]
        [TestCase("enemy-duplicate", InconsistentBinding, "Before.Enemies[1]")]
        [TestCase("enemy-negative-hp", InvalidValue, "Before.Enemies[0].Hp")]
        [TestCase("enemy-excess-hp", InvalidValue, "Before.Enemies[0].Hp")]
        [TestCase("negative-cursor", InvalidValue, "Before.Enemies[0].IntentCursor")]
        [TestCase("total-count", InconsistentBinding, "Before.Contributions")]
        [TestCase("total-key", InconsistentBinding, "Before.Contributions[0]")]
        [TestCase("negative-dealt", InvalidValue, "Before.Contributions[0]")]
        [TestCase("negative-taken", InvalidValue, "Before.Contributions[0]")]
        [TestCase("negative-actions", InvalidValue, "Before.EffectiveActionsCompleted")]
        [TestCase("negative-phases", InvalidValue, "Before.EnemyPhasesCompleted")]
        [TestCase("zero-revision", InvalidValue, "Before.SceneRevision")]
        [TestCase("invalid-face", InvalidValue, "Before.CurrentFaceIndex")]
        [TestCase("board-definition", InconsistentBinding, "Before.Board.Face")]
        [TestCase("pending-link", InvalidPhase, "Before.Board.PendingLinks")]
        [TestCase("locked-key", InconsistentBinding, "Before.Board.LockedRoutes")]
        public void CompleteCurrentStateMustMatchOriginalDefinitionsAndNumericDomains(string fault,
            CandidateAttackRejectionCode code, string path)
        {
            var binding = Bind(Candidate(1, 1, out var fixture)); var initial = binding.Start.Snapshot;
            var members = initial.Members.ToList(); var enemies = initial.Enemies.ToList(); var totals = initial.Contributions.ToList();
            var board = initial.Board; BigInteger revision = 1, actions = 0, phases = 0; var faceIndex = 0;
            switch (fault)
            {
                case "member-count": members.Clear(); break;
                case "member-key": members[0] = new BattleMemberState(BattleCombatantKey.ForParticipant("other", "other"), members[0].Member, R(100)); break;
                case "member-definition": members[0] = new BattleMemberState(members[0].CombatantKey, Bind(Candidate(1, 1, out _)).Start.Snapshot.Members[0].Member, R(100)); break;
                case "member-negative-hp": members[0] = new BattleMemberState(members[0].CombatantKey, members[0].Member, R(-1)); break;
                case "member-excess-hp": members[0] = new BattleMemberState(members[0].CombatantKey, members[0].Member, R(101)); break;
                case "enemy-count": enemies.RemoveAt(1); break;
                case "enemy-key": enemies[0] = new BattleEnemyState(enemies[1].CombatantKey, enemies[0].PairKey, enemies[0].Enemy, R(15), 0); break;
                case "enemy-pair": enemies[0] = new BattleEnemyState(enemies[0].CombatantKey, enemies[1].PairKey, enemies[0].Enemy, R(15), 0); break;
                case "enemy-definition": enemies[0] = new BattleEnemyState(enemies[0].CombatantKey, enemies[0].PairKey, Bind(Candidate(1, 1, out _)).Start.Snapshot.Enemies[0].Enemy, R(15), 0); break;
                case "enemy-duplicate": enemies[1] = enemies[0]; break;
                case "enemy-negative-hp": enemies[0] = Enemy(enemies[0], R(-1), 0); break;
                case "enemy-excess-hp": enemies[0] = Enemy(enemies[0], R(16), 0); break;
                case "negative-cursor": enemies[0] = Enemy(enemies[0], R(15), -1); break;
                case "total-count": totals.Clear(); break;
                case "total-key": totals[0] = new BattleContributionTotals(enemies[0].CombatantKey, R(0), R(0)); break;
                case "negative-dealt": totals[0] = new BattleContributionTotals(members[0].CombatantKey, R(-1), R(0)); break;
                case "negative-taken": totals[0] = new BattleContributionTotals(members[0].CombatantKey, R(0), R(-1)); break;
                case "negative-actions": actions = -1; break;
                case "negative-phases": phases = -1; break;
                case "zero-revision": revision = 0; break;
                case "invalid-face": faceIndex = 1; break;
                case "board-definition": board = Bind(Candidate(1, 1, out _)).Start.Snapshot.Board; break;
                case "pending-link": board = new BattleBoardState(board.Face, board.LockedRoutes, new[] { enemies[0].PairKey }); break;
                case "locked-key": board = new BattleBoardState(board.Face,
                    new[] { new BattleLockedRoute(BattlePairKey.Create("other", "other", "other"), Request(initial, fixture).Route) }, board.PendingLinks); break;
            }
            var before = State(initial, members: members, enemies: enemies, contributions: totals, board: board,
                revision: revision, actions: actions, phases: phases, faceIndex: faceIndex);
            var request = Request(initial, fixture); request.ExpectedSceneRevision = revision;
            Reject(binding, before, request, code, path);
        }

        [TestCase("out-of-bounds", "CellOutOfBounds", 1)]
        [TestCase("diagonal", "NonAdjacent", 1)]
        [TestCase("self", "SelfIntersection", 2)]
        [TestCase("foreign", "ForeignEndpoint", 3)]
        [TestCase("overlap", "LockedOverlap", 2)]
        [TestCase("locked", "PairAlreadyLocked", -1)]
        [TestCase("empty", "PathTooShort", -1)]
        [TestCase("endpoint", "EndpointMismatch", -1)]
        public void GeometryPropagatesOriginal001ReasonAndCellWithNoSampling(string fault, string reason, int index)
        {
            var binding = Bind(Candidate(1, 1, out var fixture)); var before = binding.Start.Snapshot; var request = Request(before, fixture);
            switch (fault)
            {
                case "out-of-bounds": request.Route = Cells(0, 0, -1, 0, 1, 1); break;
                case "diagonal": request.Route = Cells(0, 0, 1, 1); break;
                case "self": request.Route = Cells(0, 0, 0, 1, 0, 0, 1, 0, 1, 1); break;
                case "foreign": request.Route = Cells(0, 0, 0, 1, 0, 2, 0, 3, 1, 3, 1, 2, 1, 1); break;
                case "overlap":
                    var enemies = before.Enemies.ToList(); enemies[1] = Enemy(enemies[1], R(0), 0);
                    var lockedB = new BattleLockedRoute(enemies[1].PairKey, Cells(0, 3, 0, 2, 1, 2, 2, 2, 2, 3));
                    before = State(before, enemies: enemies, board: new BattleBoardState(before.Board.Face, new[] { lockedB }, Array.Empty<BattlePairKey>()));
                    request.Route = Cells(0, 0, 0, 1, 0, 2, 1, 2, 1, 1); break;
                case "locked": before = State(before, board: new BattleBoardState(before.Board.Face,
                    new[] { new BattleLockedRoute(request.Pair, request.Route) }, Array.Empty<BattlePairKey>())); break;
                case "empty": request.Route.Clear(); break;
                case "endpoint": request.Route[0] = new FlowPos(1, 0); break;
            }
            var result = Reject(binding, before, request, InvalidRoute, "Route");
            Assert.AreEqual(reason, result.RouteReasonCode); Assert.AreEqual(index < 0 ? (int?)null : index, result.RouteCellIndex);
        }

        [Test]
        public void ALocalLegalRouteThatSeparatesTheRemainingPairStillAttacks()
        {
            var input = Candidate(1, 1, out var fixture); var face = input.Level.Faces[0];
            face.Pairs[0].EndpointA = new FlowPos(1, 0); face.Pairs[0].EndpointB = new FlowPos(1, 3);
            face.Pairs[1].EndpointA = new FlowPos(0, 0); face.Pairs[1].EndpointB = new FlowPos(3, 3);
            input.Context.SourceNotes.Add("constructed separating geometry, not a source board or full-replay claim");
            input.Members[0].StatsContext = CopyContext(input.Context);
            var binding = Bind(input); var before = binding.Start.Snapshot; var request = Request(before, fixture);
            request.Route = Cells(1, 0, 1, 1, 1, 2, 1, 3);
            var result = Accept(binding, before, request);
            Assert.IsTrue(result.Fact.DefeatedTarget); Value(result.Fact.HpLoss, 15);
            Assert.IsTrue(result.Frame.Action.Route.All(c => c.x == 1));
            CollectionAssert.AreEqual(new[] { 0, 1, 2, 3 }, result.Frame.Action.Route.Select(c => c.y));
            Assert.Less(face.Pairs[1].EndpointA.x, 1); Assert.Greater(face.Pairs[1].EndpointB.x, 1);
            Assert.IsEmpty(result.Frame.BeforeSnapshot.Board.LockedRoutes);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void CurrentRandomMustRetainTheOriginalStreamAndCritBinding(bool wrongCrit)
        {
            var input = Candidate(1, 1, out var fixture); var binding = Bind(input);
            if (wrongCrit) input.Members[0].Crit.C = R(1, 3);
            var other = Bind(input, wrongCrit ? 42UL : 17UL);
            var before = State(binding.Start.Snapshot, random: other.Start.Snapshot.Random);
            Reject(binding, before, Request(before, fixture), InconsistentBinding,
                wrongCrit ? "Before.Random.PrdStates[0].Crit.C" : "Before.Random.Stream.Initial");
        }

        [TestCase("context")]
        [TestCase("Level.RecommendedLevel")]
        [TestCase("Members[0].Level")]
        [TestCase("Members[0].Stats.MaxHp")]
        [TestCase("Members[0].Stats.Attack")]
        [TestCase("Members[0].Stats.PhysicalDefense")]
        [TestCase("Members[0].Stats.MagicDefense")]
        [TestCase("Members[0].EntryHp")]
        [TestCase("Members[0].Crit.TargetProbability")]
        [TestCase("Members[0].Crit.C")]
        [TestCase("Members[0].Crit.Multiplier")]
        [TestCase("Level.Faces[1].Pairs[0].Enemy.Stats.MaxHp")]
        [TestCase("Level.Faces[1].Pairs[0].Enemy.Stats.Attack")]
        [TestCase("Level.Faces[1].Pairs[0].Enemy.Stats.PhysicalDefense")]
        [TestCase("Level.Faces[1].Pairs[0].Enemy.Stats.MagicDefense")]
        [TestCase("Level.Faces[1].Pairs[0].Enemy.IntentCycle[0].DamageCoefficient")]
        public void NewSmallBudgetRechecksAllRetainedDefinitionNumbersBeforeInvalidIdentity(string field)
        {
            var input = Candidate(1, 1, out var fixture); var large = BigInteger.One << 80;
            var later = Candidate(1, 1, out _).Level.Faces[0]; later.FaceId = "later"; input.Level.Faces.Add(later);
            if (field == "context") { input.Context.DraftRevision = large; input.Members[0].StatsContext = CopyContext(input.Context); }
            else
            {
                object value = field.EndsWith(".Level") || field.EndsWith(".RecommendedLevel") ? (object)large
                    : field.EndsWith(".TargetProbability") || field.EndsWith(".C") ? Fraction(1, large) : R(large);
                Set(input, field, value);
                if (field.EndsWith(".EntryHp")) input.Members[0].Stats.MaxHp = R(large);
            }
            var binding = Bind(input); var before = binding.Start.Snapshot; var request = Request(before, fixture);
            request.PlayerId = "wrong";
            Limit(binding, before, request, new RandomSamplingBudget(new ExactMathBudget(64)), "IntegerBits", 0);
        }

        [TestCase("revision")]
        [TestCase("expected")]
        [TestCase("actions")]
        [TestCase("phases")]
        [TestCase("member-hp")]
        [TestCase("enemy-hp")]
        [TestCase("intent")]
        [TestCase("damage-dealt")]
        [TestCase("damage-taken")]
        [TestCase("random-words")]
        public void NewSmallBudgetRechecksAllRetainedCurrentNumbersBeforeGeometry(string field)
        {
            var binding = Bind(Candidate(1, 1, out var fixture)); var initial = binding.Start.Snapshot; var large = BigInteger.One << 80;
            var members = initial.Members.ToList(); var enemies = initial.Enemies.ToList(); var totals = initial.Contributions.ToList();
            var random = initial.Random; BigInteger revision = 1, actions = 0, phases = 0;
            switch (field)
            {
                case "revision": revision = large; break;
                case "actions": actions = large; break;
                case "phases": phases = large; break;
                case "member-hp": members[0] = new BattleMemberState(members[0].CombatantKey, members[0].Member, Fraction(1, large)); break;
                case "enemy-hp": enemies[0] = Enemy(enemies[0], Fraction(1, large), 0); break;
                case "intent": enemies[0] = Enemy(enemies[0], enemies[0].Hp, large); break;
                case "damage-dealt": totals[0] = new BattleContributionTotals(members[0].CombatantKey, R(large), R(0)); break;
                case "damage-taken": totals[0] = new BattleContributionTotals(members[0].CombatantKey, R(0), Fraction(1, large)); break;
                case "random-words": random = new BattleRandomSnapshot(Pcg32StreamState.Restore(random.Stream.Initial, random.Stream.Current,
                    large, new ExactMathBudget()), random.PrdStates); break;
            }
            var before = State(initial, members: members, enemies: enemies, contributions: totals,
                random: random, revision: revision, actions: actions, phases: phases);
            var request = Request(before, fixture); request.Route.Clear();
            if (field == "expected") request.ExpectedSceneRevision = large;
            Limit(binding, before, request, new RandomSamplingBudget(new ExactMathBudget(64)), "IntegerBits", 0);
        }

        [Test]
        public void SharedMathAndWordLimitsLeaveNoPartialResultAndNewBudgetReplaysExactly()
        {
            var binding = Bind(Candidate(3, 1, out var fixture)); var before = binding.Start.Snapshot; var request = Request(before, fixture);
            var fullBudget = new RandomSamplingBudget(new ExactMathBudget());
            var first = Accept(binding, before, request, fullBudget);
            Limit(binding, before, request, new RandomSamplingBudget(new ExactMathBudget(64, 0)), "PrimitiveSteps", 0);
            Limit(binding, before, request, new RandomSamplingBudget(new ExactMathBudget(), 0), "RandomWords", 0);
            Limit(binding, before, request, new RandomSamplingBudget(new ExactMathBudget(32768,
                (int)fullBudget.Math.PrimitiveStepsUsed - 1)), "PrimitiveSteps", 1);
            var shared = new RandomSamplingBudget(new ExactMathBudget(), 1);
            ExactRandomSampler.Uniform(4, before.Random.Stream, shared);
            Limit(binding, before, request, shared, "RandomWords", 1);
            Assert.AreEqual(Describe(first), Describe(Accept(binding, before, request)));
        }

        [Test]
        public void CertainOpportunityDoesNotResetTheAlreadyUsedCallerWordBudget()
        {
            var input = Candidate(1, 1, out var fixture); input.Members[0].Crit.C = R(1);
            var binding = Bind(input); var before = binding.Start.Snapshot;
            var shared = new RandomSamplingBudget(new ExactMathBudget(), 1);
            ExactRandomSampler.Uniform(4, before.Random.Stream, shared);
            var result = Accept(binding, before, Request(before, fixture), shared);
            Assert.AreEqual(1, shared.WordsUsed); Assert.IsEmpty(result.Fact.Crit.Words);
            Assert.AreEqual(BigInteger.Zero, result.Frame.Random.Stream.WordsConsumed);
        }

        [Test]
        public void RequestAndConstructorListsAreCopiedAndAllReturnedGraphsAreReadOnly()
        {
            var binding = Bind(Candidate(1, 1, out var fixture)); var initial = binding.Start.Snapshot;
            var members = initial.Members.ToList(); var enemies = initial.Enemies.ToList(); var totals = initial.Contributions.ToList();
            var locks = new List<BattleLockedRoute>(); var pending = new List<BattlePairKey>();
            var board = new BattleBoardState(initial.Board.Face, locks, pending);
            var before = State(initial, members: members, enemies: enemies, contributions: totals, board: board);
            var request = Request(before, fixture); request.OperationId = " operation retained verbatim ";
            var result = Accept(binding, before, request); var original = Describe(result); var oldSnapshot = Describe(before);
            var route = request.Route; route[0] = new FlowPos(99, 99); route.Clear(); request.Route = null;
            request.PlayerId = "changed"; request.AttemptId = "changed"; request.OperationId = "changed";
            request.Actor = enemies[0].CombatantKey; request.Pair = enemies[1].PairKey; request.ExpectedSceneRevision = 77;
            members.Clear(); enemies.Clear(); totals.Clear(); pending.Add(initial.Enemies[0].PairKey);
            locks.Add(new BattleLockedRoute(initial.Enemies[0].PairKey, Cells(0, 0, 0, 1, 1, 1)));
            Assert.AreEqual(original, Describe(result)); Assert.AreEqual(oldSnapshot, Describe(before));
            Assert.AreEqual(" operation retained verbatim ", result.Frame.Action.OperationId);
            AssertReadOnlyGraph(result); AssertReadOnlyGraph(before);
            Assert.IsNull(typeof(CandidateCombatFrame).GetProperty("PostSnapshot"));
            Assert.IsNull(typeof(CandidateDirectAttackResult).GetProperty("Completed"));
            Assert.IsEmpty(typeof(BattleSnapshot).GetConstructors(BindingFlags.Public | BindingFlags.Instance));
        }

        private static CandidateDirectAttackResult Accept(CandidateRandomBinding binding, BattleSnapshot before,
            CandidateAttackRequest request, RandomSamplingBudget budget = null)
        {
            var originalBinding = Describe(binding); var original = Describe(before); var originalRequest = Describe(request);
            var result = CandidateDirectAttack.Evaluate(binding, before, request, budget ?? new RandomSamplingBudget(new ExactMathBudget()));
            Assert.IsTrue(result.IsAccepted, result.RejectionCode + " " + result.FieldPath + " " + result.RouteReasonCode);
            Assert.IsNotNull(result.Frame); Assert.IsNotNull(result.Fact); Assert.IsNull(result.RejectionCode);
            Assert.IsNull(result.FieldPath); Assert.IsNull(result.RouteReasonCode); Assert.IsNull(result.RouteCellIndex);
            Assert.AreEqual(originalBinding, Describe(binding)); Assert.AreEqual(original, Describe(before));
            Assert.AreEqual(originalRequest, Describe(request));
            return result;
        }

        private static CandidateDirectAttackResult Reject(CandidateRandomBinding binding, BattleSnapshot before,
            CandidateAttackRequest request, CandidateAttackRejectionCode code, string path, RandomSamplingBudget budget = null)
        {
            var originalBinding = Describe(binding); var original = Describe(before); var originalRequest = Describe(request);
            budget = budget ?? new RandomSamplingBudget(new ExactMathBudget());
            var result = CandidateDirectAttack.Evaluate(binding, before, request, budget);
            Assert.IsFalse(result.IsAccepted); Assert.IsNull(result.Frame); Assert.IsNull(result.Fact);
            Assert.AreEqual(code, result.RejectionCode); Assert.AreEqual(path, result.FieldPath); Assert.AreEqual(0, budget.WordsUsed);
            Assert.AreEqual(originalBinding, Describe(binding)); Assert.AreEqual(original, Describe(before));
            Assert.AreEqual(originalRequest, Describe(request));
            return result;
        }

        private static void Limit(CandidateRandomBinding binding, BattleSnapshot before, CandidateAttackRequest request,
            RandomSamplingBudget budget, string reason, int wordsUsed)
        {
            var originalBinding = Describe(binding); var original = Describe(before); var originalRequest = Describe(request);
            CandidateDirectAttackResult result = null;
            var exception = Assert.Throws<ExactMathLimitException>(() => result = CandidateDirectAttack.Evaluate(binding, before, request, budget));
            Assert.AreEqual(reason, exception.ReasonCode); Assert.IsNull(result); Assert.AreEqual(wordsUsed, budget.WordsUsed);
            Assert.AreEqual(originalBinding, Describe(binding)); Assert.AreEqual(original, Describe(before));
            Assert.AreEqual(originalRequest, Describe(request));
        }

        private static BattleSnapshot State(BattleSnapshot original, IEnumerable<BattleMemberState> members = null,
            IEnumerable<BattleEnemyState> enemies = null, IEnumerable<BattleContributionTotals> contributions = null,
            BattleBoardState board = null, BattleRandomSnapshot random = null, BigInteger? revision = null,
            BigInteger? actions = null, BigInteger? phases = null, int? faceIndex = null, BattlePhase? phase = null)
        {
            return new BattleSnapshot(original.Baseline, revision ?? original.SceneRevision,
                actions ?? original.EffectiveActionsCompleted, phases ?? original.EnemyPhasesCompleted,
                faceIndex ?? original.CurrentFaceIndex, phase ?? original.Phase, board ?? original.Board,
                members ?? original.Members, enemies ?? original.Enemies, random ?? original.Random,
                contributions ?? original.Contributions);
        }

        private static BattleEnemyState Enemy(BattleEnemyState source, ExactRational hp, BigInteger cursor)
        { return new BattleEnemyState(source.CombatantKey, source.PairKey, source.Enemy, hp, cursor); }

        private static CandidateAttackRequest Request(BattleSnapshot before, DemoContentFixture fixture, int color = 0)
        {
            var pair = before.Board.Face.Pairs.Single(p => p.GeometryColorId == color);
            return new CandidateAttackRequest
            {
                PlayerId = before.Baseline.Entry.PlayerId, AttemptId = before.Baseline.Entry.AttemptId,
                OperationId = "candidate:attack", ExpectedSceneRevision = before.SceneRevision,
                Actor = before.Members[0].CombatantKey,
                Pair = BattlePairKey.Create(before.Baseline.Entry.AttemptId, before.Board.Face.FaceId, pair.PairId),
                Route = new List<FlowPos>(fixture.CopySolution().paths.Single(p => p.colorId == color).cells)
            };
        }

        private static CandidateRandomBinding Bind(BattleEntryInput input, ulong seed = 42)
        {
            var entry = new BattleEntryPreparer().PrepareCandidate(input, new ExactMathBudget());
            Assert.IsTrue(entry.IsAccepted, entry.RejectionCode + " " + entry.FieldPath);
            var bytes = new byte[48];
            for (var offset = 0; offset < 48; offset += 16)
                for (var i = 0; i < 8; i++) { bytes[offset + i] = (byte)(seed >> (8 * i)); bytes[offset + 8 + i] = (byte)(54UL >> (8 * i)); }
            var result = CandidateRandomPreparer.Prepare(entry.Entry, new CandidateSeedMaterial
            { Bytes = bytes, SourceCapabilityId = "synthetic-test-provider", MappingId = CandidateRandomPreparer.SupportedMappingId }, new ExactMathBudget());
            Assert.IsTrue(result.IsAccepted, result.RejectionCode + " " + result.FieldPath); return result.Binding;
        }

        private static BattleEntryInput Candidate(int stage, int direction, out DemoContentFixture fixture)
        {
            fixture = DemoContentFixture.CaptureSource(stage, 1, (SourceCoordinateConvention)direction);
            var geometry = fixture.CopyLevel();
            var context = new CandidateContext
            {
                DraftId = "candidate:" + fixture.FixtureKey, DraftRevision = 1,
                ContentFingerprint = "candidate:" + fixture.FixtureKey + ":direction" + direction,
                RuleVersion = "candidate-r1", NumericContractVersion = "exact-candidate", RandomContractVersion = "pcg-candidate",
                SourceNotes = new List<string> { fixture.SourcePath + ":" + fixture.SourceLocator + ":" + fixture.SourceSha256,
                    fixture.CoordinateTransform, "synthetic C=1/4 default; tests explicitly override isolated candidate values; no calibrated 20% or confirmed source orientation" }
            };
            var face = new FaceInput { FaceId = "face:first", Width = geometry.width, Height = geometry.height, Pairs = new List<PairInput>() };
            foreach (var source in fixture.Bindings)
            {
                var pair = geometry.pairs.Single(p => p.colorId == source.ColorId); var heavy = source.EnemyAlias == "E02";
                var intents = new List<EnemyIntentInput>();
                if (heavy) intents.Add(new EnemyIntentInput { Kind = EnemyIntentKind.Charge, Targeting = EnemyTargeting.FirstLiving, DamageKind = null, DamageCoefficient = null });
                intents.Add(new EnemyIntentInput { Kind = EnemyIntentKind.Strike, Targeting = EnemyTargeting.FirstLiving, DamageKind = EntryDamageKind.Physical, DamageCoefficient = R(heavy ? 13 : 3, heavy ? 10 : 5) });
                face.Pairs.Add(new PairInput
                {
                    PairId = source.SourcePair, GeometryColorId = source.ColorId, EndpointA = pair.endpointA, EndpointB = pair.endpointB,
                    Enemy = new EnemyInput { EnemyInstanceKey = source.SourcePair, EnemyDefinitionId = source.EnemyAlias,
                        OriginalSlot = source.OriginalSlot, StableOrder = source.OriginalSlot,
                        Behavior = heavy ? EnemyBehavior.ChargeHeavy : EnemyBehavior.NormalStrike,
                        Stats = Stats(heavy ? 20 : 15, 10, heavy ? 20 : 0, 0), IntentCycle = intents }
                });
            }
            return new BattleEntryInput
            {
                PlayerId = "candidate:player", ChallengeId = "candidate:challenge", AttemptId = "candidate:attempt", EntryBaselineId = "candidate:baseline",
                Context = context, CarryMode = EntryCarryMode.Empty, RequiredFeatures = new List<string>(),
                Level = new LevelInput { LevelId = fixture.FixtureKey, LevelVersion = "candidate-r1", RecommendedLevel = 1, Faces = new List<FaceInput> { face } },
                Members = new List<MemberInput> { new MemberInput { CharacterId = "candidate:warrior", ClassId = "warrior", ClassKind = CharacterClassKind.Warrior,
                    OriginalSlot = 2, Level = 1, IsReady = true, StatsOrigin = BaseStatsOrigin.ComputedBaseStats, StatsContext = CopyContext(context),
                    Stats = Stats(100, 20, 10, 6), EntryHp = R(100), LearnedSkills = new List<string>(), Crit = new WarriorCritInput
                    { PassiveDefinitionId = "warrior:crit", TargetProbability = R(1, 5), C = R(1, 4), Multiplier = R(3, 2) } } }
            };
        }

        private static CandidateContext CopyContext(RuleContext c)
        {
            return new CandidateContext { DraftId = c.DraftId, DraftRevision = c.DraftRevision, ContentFingerprint = c.ContentFingerprint,
                RuleVersion = c.RuleVersion, NumericContractVersion = c.NumericContractVersion, RandomContractVersion = c.RandomContractVersion,
                SourceNotes = new List<string>(c.SourceNotes) };
        }
        private static StatsInput Stats(int hp, int attack, int physical, int magic)
        { return new StatsInput { MaxHp = R(hp), Attack = R(attack), PhysicalDefense = R(physical), MagicDefense = R(magic), Evasion = R(0), AttackRange = 1 }; }
        private static ExactRational R(BigInteger numerator, int denominator = 1) { return Fraction(numerator, denominator); }
        private static ExactRational Fraction(BigInteger numerator, BigInteger denominator)
        { return ExactRational.Create(numerator, denominator, new ExactMathBudget()); }
        private static void Value(ExactRational value, int n, int d = 1)
        { Assert.AreEqual(new BigInteger(n), value.Numerator); Assert.AreEqual(new BigInteger(d), value.Denominator); }
        private static List<FlowPos> Cells(params int[] values)
        {
            var cells = new List<FlowPos>();
            for (var i = 0; i < values.Length; i += 2) cells.Add(new FlowPos(values[i], values[i + 1]));
            return cells;
        }
        private static void Set(object value, string path, object replacement)
        {
            var parts = path.Replace("[", ".").Replace("]", "").Split('.');
            for (var i = 0; i < parts.Length - 1; i++)
                value = value is IList list ? list[int.Parse(parts[i])] : value.GetType().GetProperty(parts[i]).GetValue(value);
            value.GetType().GetProperty(parts.Last()).SetValue(value, replacement);
        }
        private static string Describe(object value)
        {
            if (value == null) return "null";
            if (value is string s) return s.Length + ":" + s;
            if (value is BigInteger n) return n.ToString(CultureInfo.InvariantCulture);
            if (value is ExactRational r) return r.Numerator + "/" + r.Denominator;
            if (value is FlowPos p) return p.x + "," + p.y;
            if (value.GetType().IsValueType) return value.ToString();
            if (value is IEnumerable items) return "[" + string.Join(";", items.Cast<object>().Select(Describe)) + "]";
            return "{" + string.Join(";", value.GetType().GetProperties().OrderBy(p => p.Name, StringComparer.Ordinal)
                .Select(p => p.Name + "=" + Describe(p.GetValue(value)))) + "}";
        }
        private static void AssertReadOnlyGraph(object value)
        {
            if (value == null || value is string || value is ExactRational || value.GetType().IsValueType) return;
            if (value is IList list)
            {
                Assert.IsTrue(list.IsReadOnly); Assert.Throws<NotSupportedException>(() => list.Clear());
                if (list.Count > 0) Assert.Throws<NotSupportedException>(() => list[0] = list[0]);
                foreach (var item in list) AssertReadOnlyGraph(item);
                return;
            }
            Assert.IsEmpty(value.GetType().GetConstructors(BindingFlags.Public | BindingFlags.Instance));
            foreach (var property in value.GetType().GetProperties())
            { Assert.IsFalse(property.CanWrite, property.Name); AssertReadOnlyGraph(property.GetValue(value)); }
        }
    }
}
