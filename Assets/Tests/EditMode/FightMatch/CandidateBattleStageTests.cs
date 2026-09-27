using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Reflection;
using FlowPuzzle.Core;
using NUnit.Framework;
using static FightMatch.Core.CandidateStageRejectionCode;
using static FightMatch.Core.CandidateStageFactKind;

namespace FightMatch.Core.Tests
{
    [TestFixture]
    public class CandidateBattleStageTests
    {
        [TestCase(false, false, false)]
        [TestCase(false, false, true)]
        [TestCase(false, true, false)]
        [TestCase(false, true, true)]
        [TestCase(true, false, false)]
        [TestCase(true, false, true)]
        [TestCase(true, true, false)]
        [TestCase(true, true, true)]
        public void AttackLineUsesFinalHpAndPreservesExistingHistory(bool killed, bool allDown, bool reverse)
        {
            var binding = Bind(Input()); var initial = binding.Start.Snapshot;
            var opportunity = CandidateWarriorCritEvaluator.EvaluateOpportunity(binding, initial.Random,
                initial.Members[0].CombatantKey, initial.Enemies[0].CombatantKey, 0, new RandomSamplingBudget(new ExactMathBudget()));
            Assert.IsTrue(opportunity.IsAccepted);
            var before = State(initial, new[] { R(20), R(0), R(20) }, R(80), new[] { 1 }, random: opportunity.Next,
                revision: 7, actions: 3, phases: 2, totals: new[] { new BattleContributionTotals(initial.Members[0].CombatantKey, R(7, 3), R(20)) });
            var attack = Attack(before); if (reverse) attack.Route.Reverse();
            var hp = Projection(before); hp.MemberHp[0].Hp = R(allDown ? 0 : 50); hp.EnemyHp[0].Hp = R(killed ? 0 : 5);
            var decision = AcceptAttack(before, attack, hp);
            Assert.AreSame(before, decision.BeforeSnapshot); Assert.AreSame(before.Baseline, decision.Baseline);
            Assert.AreSame(before.Board.Face, decision.Board.Face); Assert.AreSame(before.Board.LockedRoutes[0], decision.Board.LockedRoutes[0]);
            Assert.AreEqual(killed ? 2 : 1, decision.Board.LockedRoutes.Count); Assert.IsEmpty(decision.Board.PendingLinks);
            if (killed) CollectionAssert.AreEqual(attack.Route, decision.Board.LockedRoutes[1].Route);
            Assert.AreEqual(allDown ? BattlePhase.AwaitRescue : BattlePhase.AwaitAction, decision.NextPhase);
            Assert.IsFalse(decision.DidFlipFace); Assert.IsNull(decision.NextFace); Assert.AreEqual(0, decision.NextFaceIndex);
            Facts(decision, killed ? RouteLocked : TemporaryRouteRemoved, PhaseSelected);
            Assert.AreEqual(attack.Pair, decision.OrderedFacts[0].Pair); Assert.IsNull(decision.OrderedFacts[1].Pair);
            Assert.AreEqual(new BigInteger(7), before.SceneRevision); Assert.AreEqual(new BigInteger(3), before.EffectiveActionsCompleted);
            Assert.AreEqual(new BigInteger(2), before.EnemyPhasesCompleted); Assert.AreEqual(new BigInteger(5), before.Enemies[2].IntentCursor);
            Value(before.Members[0].Hp, 80); Value(before.Contributions[0].EffectiveDamageDealtHp, 7, 3);
            Value(before.Contributions[0].EffectiveDamageTakenHp, 20); Assert.AreEqual(BigInteger.One, before.Random.Stream.WordsConsumed);
            Assert.AreEqual(BigInteger.One, before.Random.PrdStates[0].FailureCount);
            Assert.AreEqual(CandidateStageOperation.AfterAttack, decision.Source.Kind); Assert.AreEqual(attack.Actor, decision.Source.Actor);
        }

        [Test]
        public void Real009AttackFeedsStageWithoutRepeatingItsDamageOrRandomWork()
        {
            var binding = Bind(Input()); var before = binding.Start.Snapshot; var attack = Attack(before);
            var result = CandidateDirectAttack.Evaluate(binding, before, attack, new RandomSamplingBudget(new ExactMathBudget()));
            Assert.IsTrue(result.IsAccepted); Value(result.Fact.HpLoss, 20); Assert.IsTrue(result.Fact.DefeatedTarget);
            var hp = new CandidateFinalHpProjection
            {
                MemberHp = result.Frame.Members.Select(m => Hp(m.CombatantKey, m.Hp)).ToList(),
                EnemyHp = result.Frame.Enemies.Select(e => Hp(e.CombatantKey, e.Hp)).ToList()
            };
            var decision = AcceptAttack(before, attack, hp);
            Value(decision.FinalHp.EnemyHp[0].Hp, 0); Assert.AreEqual(attack.Pair, decision.Board.LockedRoutes.Single().PairKey);
            Assert.AreEqual(BigInteger.One, result.Frame.Random.Stream.WordsConsumed);
            Assert.AreEqual(BigInteger.Zero, decision.BeforeSnapshot.Random.Stream.WordsConsumed);
            Assert.IsNull(typeof(CandidateStageDecision).GetProperty("Random"));
            Assert.IsNull(typeof(CandidateStageDecision).GetProperty("PostSnapshot"));
            Assert.IsNull(typeof(CandidateStageDecision).GetProperty("FinalAttemptReport"));
            Assert.IsNull(typeof(CandidateStageDecision).GetProperty("Completed"));
        }

        [Test]
        public void StageProjectionIsExplicitInputAndDoesNotRecheckRangeOrClaimCombatProof()
        {
            var before = Start(); var attack = Attack(before, 2); var hp = Projection(before);
            Assert.AreEqual(1, before.Members[0].Member.Stats.AttackRange);
            hp.EnemyHp[2].Hp = R(1, 3);
            var decision = AcceptAttack(before, attack, hp);
            Value(decision.FinalHp.EnemyHp[2].Hp, 1, 3); Facts(decision, TemporaryRouteRemoved, PhaseSelected);
            Assert.AreEqual(BigInteger.Zero, before.Random.Stream.WordsConsumed);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void OtherDeathsUseDefinitionOrderAndPlayerCanLinkTheLaterPendingFirst(bool allDown)
        {
            var input = Input(pairs: 4); var pairs = input.Level.Faces[0].Pairs;
            var swap = pairs[1]; pairs[1] = pairs[2]; pairs[2] = swap;
            var before = Bind(input).Start.Snapshot; var hp = Projection(before);
            // Explicit stage inputs for other deaths; this does not claim an AOE or DOT evaluator.
            hp.EnemyHp[1].Hp = R(0); hp.EnemyHp[2].Hp = R(0); hp.MemberHp[0].Hp = R(allDown ? 0 : 60);
            hp.EnemyHp.Reverse();
            var decision = AcceptAttack(before, Attack(before), hp);
            CollectionAssert.AreEqual(new[] { "pair:2", "pair:1" }, decision.Board.PendingLinks.Select(k => k.PairId));
            Facts(decision, TemporaryRouteRemoved, PendingLinkAdded, PendingLinkAdded, PhaseSelected);
            Assert.AreEqual("pair:2", decision.OrderedFacts[1].Pair.PairId); Assert.AreEqual("pair:1", decision.OrderedFacts[2].Pair.PairId);
            Assert.AreEqual(BattlePhase.AwaitLinks, decision.NextPhase);
            var linksBefore = InstallStageInput(decision);
            var first = AcceptLink(linksBefore, Link(linksBefore, 2));
            CollectionAssert.AreEqual(new[] { "pair:2" }, first.Board.PendingLinks.Select(k => k.PairId));
            Assert.AreEqual(BattlePhase.AwaitLinks, first.NextPhase); Facts(first, RouteLocked, PendingLinkRemoved, PhaseSelected);
            var lastBefore = InstallStageInput(first); var last = AcceptLink(lastBefore, Link(lastBefore, 1));
            Assert.IsEmpty(last.Board.PendingLinks); Assert.AreEqual(allDown ? BattlePhase.AwaitRescue : BattlePhase.AwaitAction, last.NextPhase);
            Assert.AreEqual(2, last.Board.LockedRoutes.Count); Assert.IsNull(last.Source.Actor);
            Assert.AreEqual(CandidateStageOperation.CompleteLink, last.Source.Kind);
        }

        [TestCase(false, 1, false)]
        [TestCase(false, 1, true)]
        [TestCase(false, 2, false)]
        [TestCase(false, 2, true)]
        [TestCase(false, 3, true)]
        [TestCase(true, 1, false)]
        [TestCase(true, 1, true)]
        [TestCase(true, 2, false)]
        [TestCase(true, 2, true)]
        [TestCase(true, 3, true)]
        public void ClearingTheLastLineFlipsExactlyOneFaceOrWinsEvenWhenAllDown(bool link, int faces, bool allDown)
        {
            var initial = Start(Input(faces));
            var before = State(initial, new[] { R(link ? 0 : 20), R(0), R(0) }, R(link && allDown ? 0 : 80),
                new[] { 1, 2 }, link ? new[] { 0 } : null, phase: link ? BattlePhase.AwaitLinks : BattlePhase.AwaitAction);
            CandidateStageDecision decision;
            if (link) decision = AcceptLink(before, Link(before));
            else
            {
                var hp = Projection(before); hp.EnemyHp[0].Hp = R(0); hp.MemberHp[0].Hp = R(allDown ? 0 : 80);
                decision = AcceptAttack(before, Attack(before), hp);
            }
            Assert.AreEqual(faces > 1, decision.DidFlipFace); Assert.AreEqual(faces > 1 ? 1 : 0, decision.NextFaceIndex);
            Assert.AreEqual(faces == 1 ? BattlePhase.WonPendingSettlement : allDown ? BattlePhase.AwaitRescue : BattlePhase.AwaitAction, decision.NextPhase);
            Assert.IsEmpty(decision.Board.PendingLinks); Assert.AreSame(before, decision.BeforeSnapshot);
            Assert.IsTrue(decision.FinalHp.EnemyHp.All(r => r.Hp.Numerator.IsZero));
            Assert.IsTrue(decision.FinalHp.EnemyHp.All(r => r.CombatantKey.FaceId == before.Board.Face.FaceId));
            Value(decision.FinalHp.MemberHp[0].Hp, allDown ? 0 : 80);
            var kinds = new List<CandidateStageFactKind> { RouteLocked }; if (link) kinds.Add(PendingLinkRemoved);
            if (faces > 1)
            {
                kinds.Add(FaceChanged); Assert.AreSame(before.Baseline.Entry.Level.Faces[1], decision.NextFace);
                Assert.AreSame(decision.NextFace, decision.Board.Face); Assert.IsEmpty(decision.Board.LockedRoutes);
                Value(decision.NextFace.Pairs[0].Enemy.Stats.MaxHp, 30);
                Assert.AreEqual(decision.NextFace.FaceId, decision.OrderedFacts.Last(f => f.Kind == FaceChanged).NextFaceId);
            }
            else { Assert.IsNull(decision.NextFace); Assert.AreEqual(3, decision.Board.LockedRoutes.Count); }
            kinds.Add(PhaseSelected); Facts(decision, kinds.ToArray());
            Assert.IsTrue(decision.OrderedFacts.All(f => f.FaceId == before.Board.Face.FaceId));
        }

        [Test]
        public void FinalFaceVictoryDoesNotRequireAnActionableMemberOrFullBoard()
        {
            var initial = Start(Input(2)); var face = initial.Baseline.Entry.Level.Faces[1];
            var enemies = face.Pairs.Select(p => new BattleEnemyState(
                BattleCombatantKey.ForEnemy(initial.Baseline.Entry.AttemptId, face.FaceId, p.Enemy.EnemyInstanceKey),
                BattlePairKey.Create(initial.Baseline.Entry.AttemptId, face.FaceId, p.PairId), p.Enemy, R(0), 4)).ToList();
            var before = new BattleSnapshot(initial.Baseline, 9, 4, 4, 1, BattlePhase.AwaitLinks,
                new BattleBoardState(face, enemies.Take(2).Select(e => new BattleLockedRoute(e.PairKey, Route(face, e.PairKey.PairId))), new[] { enemies[2].PairKey }),
                new[] { new BattleMemberState(initial.Members[0].CombatantKey, initial.Members[0].Member, R(0)) }, enemies, initial.Random, initial.Contributions);
            var result = AcceptLink(before, Link(before, 2));
            Assert.AreEqual(BattlePhase.WonPendingSettlement, result.NextPhase); Assert.AreEqual(1, result.NextFaceIndex);
            Assert.AreEqual(12, result.Board.LockedRoutes.Sum(r => r.Route.Count)); Assert.AreEqual(20, face.Width * face.Height);
            Value(result.FinalHp.MemberHp[0].Hp, 0);
        }

        [TestCase("before")]
        [TestCase("attack")]
        [TestCase("finalHp")]
        [TestCase("budget")]
        public void AttackNullRootsThrowTheNamedArgument(string field)
        {
            var before = Start();
            var error = Assert.Throws<ArgumentNullException>(() => CandidateBattleStage.AfterAttack(field == "before" ? null : before,
                field == "attack" ? null : Attack(before), field == "finalHp" ? null : Projection(before), field == "budget" ? null : new ExactMathBudget()));
            Assert.AreEqual(field, error.ParamName);
        }

        [TestCase("before")]
        [TestCase("link")]
        [TestCase("budget")]
        public void LinkNullRootsThrowTheNamedArgument(string field)
        {
            var before = LinkState();
            var error = Assert.Throws<ArgumentNullException>(() => CandidateBattleStage.CompleteLink(field == "before" ? null : before,
                field == "link" ? null : Link(before), field == "budget" ? null : new ExactMathBudget()));
            Assert.AreEqual(field, error.ParamName);
        }

        [TestCase("PlayerId")]
        [TestCase("AttemptId")]
        [TestCase("OperationId")]
        [TestCase("ExpectedSceneRevision")]
        [TestCase("Pair")]
        [TestCase("Route")]
        public void BothRequestsRejectEachMissingFieldBeforeNumericWork(string field)
        {
            var before = Start(); var attack = Attack(before); Set(attack, field, null);
            RejectAttack(before, attack, Projection(before), MissingField, field, new ExactMathBudget(maxPrimitiveSteps: 0));
            before = LinkState(); var link = Link(before); Set(link, field, null);
            RejectLink(before, link, MissingField, field, new ExactMathBudget(maxPrimitiveSteps: 0));
        }

        [Test]
        public void OnlyAttackRequiresActorAndBlankTextIsMissing()
        {
            var before = Start(); var attack = Attack(before); attack.Actor = null;
            RejectAttack(before, attack, Projection(before), MissingField, "Actor");
            attack = Attack(before); attack.OperationId = " \t";
            RejectAttack(before, attack, Projection(before), MissingField, "OperationId");
            var links = LinkState(); Assert.IsNull(typeof(CandidateLinkRequest).GetProperty("Actor"));
            var link = Link(links); link.OperationId = " operation retained ";
            Assert.AreEqual(" operation retained ", AcceptLink(links, link).Source.OperationId);
        }

        [TestCase("MemberHp")]
        [TestCase("EnemyHp")]
        [TestCase("MemberHp.0")]
        [TestCase("EnemyHp.0")]
        [TestCase("MemberHp.0.CombatantKey")]
        [TestCase("EnemyHp.0.CombatantKey")]
        [TestCase("MemberHp.0.Hp")]
        [TestCase("EnemyHp.0.Hp")]
        public void ProjectionRequiresEveryListRowKeyAndHp(string field)
        {
            var before = Start(); var hp = Projection(before); Set(hp, field, null);
            var path = "FinalHp." + field.Replace(".0", "[0]");
            RejectAttack(before, Attack(before), hp, MissingField, path, new ExactMathBudget(maxPrimitiveSteps: 0));
        }

        [TestCase("player-case", InconsistentBinding, "PlayerId")]
        [TestCase("player-space", InconsistentBinding, "PlayerId")]
        [TestCase("attempt-case", InconsistentBinding, "AttemptId")]
        [TestCase("stale", StaleContext, "ExpectedSceneRevision")]
        [TestCase("negative", InvalidValue, "ExpectedSceneRevision")]
        public void IdentitiesAndExpectedRevisionAreExactForBothEntrypoints(string change, CandidateStageRejectionCode code, string path)
        {
            foreach (var linkMode in new[] { false, true })
            {
                var before = linkMode ? LinkState() : Start(); object request = linkMode ? (object)Link(before) : Attack(before);
                if (change == "player-case") Set(request, "PlayerId", "PLAYER");
                if (change == "player-space") Set(request, "PlayerId", "player ");
                if (change == "attempt-case") Set(request, "AttemptId", "ATTEMPT");
                if (change == "stale") Set(request, "ExpectedSceneRevision", new BigInteger(5));
                if (change == "negative") Set(request, "ExpectedSceneRevision", new BigInteger(-1));
                if (linkMode) RejectLink(before, (CandidateLinkRequest)request, code, path);
                else RejectAttack(before, (CandidateAttackRequest)request, Projection(before), code, path);
            }
        }

        [TestCase("actor-kind")]
        [TestCase("actor-case")]
        [TestCase("actor-attempt")]
        [TestCase("dead-actor")]
        [TestCase("dead-target")]
        [TestCase("pair-face")]
        [TestCase("pair-case")]
        [TestCase("pair-attempt")]
        public void ActorAndTargetMustBeAliveInTheExactCurrentContext(string change)
        {
            var before = Start(Input(2));
            if (change == "dead-actor") before = State(before, memberHp: R(0));
            if (change == "dead-target") before = State(before, new[] { R(0), R(20), R(20) }, locked: new[] { 0 });
            var attack = Attack(before); var code = TargetUnavailable; var path = "Pair";
            if (change.StartsWith("actor-")) { code = InconsistentBinding; path = "Actor"; }
            if (change == "actor-kind") attack.Actor = before.Enemies[0].CombatantKey;
            if (change == "actor-case") attack.Actor = BattleCombatantKey.ForParticipant("attempt", "WARRIOR");
            if (change == "actor-attempt") attack.Actor = BattleCombatantKey.ForParticipant("other", "warrior");
            if (change == "dead-actor") { code = ActorUnavailable; path = "Actor"; }
            if (change == "pair-face") attack.Pair = BattlePairKey.Create("attempt", "face:1", "pair:0");
            if (change == "pair-case") attack.Pair = BattlePairKey.Create("attempt", "face:0", "PAIR:0");
            if (change == "pair-attempt") attack.Pair = BattlePairKey.Create("other", "face:0", "pair:0");
            RejectAttack(before, attack, Projection(before), code, path);
        }

        [TestCase("locked")]
        [TestCase("alive")]
        [TestCase("face")]
        [TestCase("attempt")]
        [TestCase("unknown")]
        public void FreeLinkOnlyAcceptsAnExactPendingPair(string change)
        {
            var before = State(Start(Input(2)), new[] { R(0), R(0), R(20) }, R(0), new[] { 1 }, new[] { 0 }, phase: BattlePhase.AwaitLinks);
            var link = Link(before);
            if (change == "locked") link.Pair = before.Enemies[1].PairKey;
            if (change == "alive") link.Pair = before.Enemies[2].PairKey;
            if (change == "face") link.Pair = BattlePairKey.Create("attempt", "face:1", "pair:0");
            if (change == "attempt") link.Pair = BattlePairKey.Create("other", "face:0", "pair:0");
            if (change == "unknown") link.Pair = BattlePairKey.Create("attempt", "face:0", "missing");
            RejectLink(before, link, NotPendingLink, "Pair");
        }

        [TestCase(BattlePhase.AwaitLinks)]
        [TestCase(BattlePhase.AwaitRescue)]
        [TestCase(BattlePhase.WonPendingSettlement)]
        [TestCase(BattlePhase.Closed)]
        [TestCase((BattlePhase)99)]
        public void AttackDoesNotRunOutsideAwaitAction(BattlePhase phase)
        {
            var before = Copy(Start(), phase: phase);
            RejectAttack(before, Attack(before), Projection(before), InvalidPhase, "Before.Phase");
        }

        [TestCase(BattlePhase.AwaitAction)]
        [TestCase(BattlePhase.AwaitRescue)]
        [TestCase(BattlePhase.WonPendingSettlement)]
        [TestCase(BattlePhase.Closed)]
        [TestCase((BattlePhase)99)]
        public void LinkDoesNotRunOutsideAwaitLinks(BattlePhase phase)
        {
            var before = Copy(LinkState(), phase: phase);
            RejectLink(before, Link(before), InvalidPhase, "Before.Phase");
        }

        [TestCase("missing-member", InconsistentBinding, "MemberHp")]
        [TestCase("extra-member", InconsistentBinding, "MemberHp")]
        [TestCase("wrong-member", InconsistentBinding, "MemberHp[0].CombatantKey")]
        [TestCase("member-kind", InconsistentBinding, "MemberHp[0].CombatantKey")]
        [TestCase("missing-enemy", InconsistentBinding, "EnemyHp")]
        [TestCase("extra-enemy", InconsistentBinding, "EnemyHp")]
        [TestCase("duplicate-enemy", InconsistentBinding, "EnemyHp[1].CombatantKey")]
        [TestCase("wrong-face", InconsistentBinding, "EnemyHp[0].CombatantKey")]
        [TestCase("negative-member", InvalidValue, "MemberHp[0].Hp")]
        [TestCase("negative-enemy", InvalidValue, "EnemyHp[0].Hp")]
        [TestCase("above-max", InvalidValue, "EnemyHp[0].Hp")]
        [TestCase("heal-member", InvalidValue, "MemberHp[0].Hp")]
        [TestCase("heal-enemy", InvalidValue, "EnemyHp[0].Hp")]
        [TestCase("revive", InvalidValue, "EnemyHp[1].Hp")]
        public void ProjectionMustCoverExactlyTheOriginalKeysWithoutHealing(string change, CandidateStageRejectionCode code, string path)
        {
            var before = State(Start(Input(2)), new[] { R(10), R(0), R(20) }, R(80), new[] { 1 }); var hp = Projection(before);
            if (change == "missing-member") hp.MemberHp.Clear();
            if (change == "extra-member") hp.MemberHp.Add(hp.MemberHp[0]);
            if (change == "wrong-member") hp.MemberHp[0].CombatantKey = BattleCombatantKey.ForParticipant("other", "warrior");
            if (change == "member-kind") hp.MemberHp[0].CombatantKey = hp.EnemyHp[0].CombatantKey;
            if (change == "missing-enemy") hp.EnemyHp.RemoveAt(2);
            if (change == "extra-enemy") hp.EnemyHp.Add(hp.EnemyHp[0]);
            if (change == "duplicate-enemy") hp.EnemyHp[1].CombatantKey = hp.EnemyHp[0].CombatantKey;
            if (change == "wrong-face") hp.EnemyHp[0].CombatantKey = BattleCombatantKey.ForEnemy("attempt", "face:1", "enemy:0");
            if (change == "negative-member") hp.MemberHp[0].Hp = R(-1);
            if (change == "negative-enemy") hp.EnemyHp[0].Hp = R(-1);
            if (change == "above-max") hp.EnemyHp[0].Hp = R(21);
            if (change == "heal-member") hp.MemberHp[0].Hp = R(81);
            if (change == "heal-enemy") hp.EnemyHp[0].Hp = R(11);
            if (change == "revive") hp.EnemyHp[1].Hp = R(1);
            RejectAttack(before, Attack(before), hp, code, "FinalHp." + path);
        }

        [TestCase("member-missing", InconsistentBinding, "Members")]
        [TestCase("member-duplicate", InconsistentBinding, "Members")]
        [TestCase("member-key", InconsistentBinding, "Members[0]")]
        [TestCase("member-definition", InconsistentBinding, "Members[0]")]
        [TestCase("member-negative", InvalidValue, "Members[0].Hp")]
        [TestCase("member-max", InvalidValue, "Members[0].Hp")]
        [TestCase("enemy-missing", InconsistentBinding, "Enemies")]
        [TestCase("enemy-duplicate", InconsistentBinding, "Enemies[1]")]
        [TestCase("enemy-key", InconsistentBinding, "Enemies[0]")]
        [TestCase("enemy-pair", InconsistentBinding, "Enemies[0]")]
        [TestCase("enemy-definition", InconsistentBinding, "Enemies[0]")]
        [TestCase("enemy-negative", InvalidValue, "Enemies[0].Hp")]
        [TestCase("enemy-max", InvalidValue, "Enemies[0].Hp")]
        [TestCase("cursor", InvalidValue, "Enemies[0].IntentCursor")]
        [TestCase("totals-missing", InconsistentBinding, "Contributions")]
        [TestCase("totals-key", InconsistentBinding, "Contributions")]
        [TestCase("totals-negative", InvalidValue, "Contributions")]
        [TestCase("revision", InvalidValue, "SceneRevision")]
        [TestCase("actions", InvalidValue, "EffectiveActionsCompleted")]
        [TestCase("phases", InvalidValue, "EnemyPhasesCompleted")]
        [TestCase("face-index", InvalidValue, "CurrentFaceIndex")]
        [TestCase("board-face", InconsistentBinding, "Board.Face")]
        public void BothEntrypointsRejectInvalidOriginalStateCoverage(string change, CandidateStageRejectionCode code, string path)
        {
            foreach (var linkMode in new[] { false, true })
            {
                var before = linkMode ? LinkState() : Start(); var other = Start();
                var members = before.Members.ToList(); var enemies = before.Enemies.ToList(); var totals = before.Contributions.ToList();
                var m = members[0]; var e = enemies[0];
                if (change == "member-missing") members.Clear();
                if (change == "member-duplicate") members.Add(m);
                if (change == "member-key") members[0] = new BattleMemberState(BattleCombatantKey.ForParticipant("other", "warrior"), m.Member, m.Hp);
                if (change == "member-definition") members[0] = new BattleMemberState(m.CombatantKey, other.Members[0].Member, m.Hp);
                if (change == "member-negative" || change == "member-max") members[0] = new BattleMemberState(m.CombatantKey, m.Member, R(change == "member-max" ? 101 : -1));
                if (change == "enemy-missing") enemies.RemoveAt(2);
                if (change == "enemy-duplicate") enemies[1] = e;
                if (change == "enemy-key") enemies[0] = new BattleEnemyState(BattleCombatantKey.ForEnemy("other", "face:0", "enemy:0"), e.PairKey, e.Enemy, e.Hp, 0);
                if (change == "enemy-pair") enemies[0] = new BattleEnemyState(e.CombatantKey, enemies[1].PairKey, e.Enemy, e.Hp, 0);
                if (change == "enemy-definition") enemies[0] = new BattleEnemyState(e.CombatantKey, e.PairKey, other.Enemies[0].Enemy, e.Hp, 0);
                if (change == "enemy-negative" || change == "enemy-max") enemies[0] = new BattleEnemyState(e.CombatantKey, e.PairKey, e.Enemy, R(change == "enemy-max" ? 21 : -1), 0);
                if (change == "cursor") enemies[0] = new BattleEnemyState(e.CombatantKey, e.PairKey, e.Enemy, e.Hp, -1);
                if (change == "totals-missing") totals.Clear();
                if (change == "totals-key") totals[0] = new BattleContributionTotals(e.CombatantKey, R(0), R(0));
                if (change == "totals-negative") totals[0] = new BattleContributionTotals(m.CombatantKey, R(-1), R(0));
                var request = Attack(before); var link = Link(before); var hp = Projection(before);
                before = Copy(before, members, enemies, totals, change == "board-face" ? other.Board : null,
                    revision: change == "revision" ? 0 : (BigInteger?)null, actions: change == "actions" ? -1 : (BigInteger?)null,
                    phases: change == "phases" ? -1 : (BigInteger?)null, faceIndex: change == "face-index" ? 1 : (int?)null);
                if (change == "revision") { request.ExpectedSceneRevision = 0; link.ExpectedSceneRevision = 0; }
                if (linkMode) RejectLink(before, link, code, "Before." + path);
                else RejectAttack(before, request, hp, code, "Before." + path);
            }
        }

        [TestCase("duplicate-lock", "Board.LockedRoutes[1]")]
        [TestCase("live-lock", "Board.LockedRoutes[0]")]
        [TestCase("foreign-lock", "Board.LockedRoutes[0]")]
        [TestCase("missing-dead", "Board")]
        [TestCase("conflict", "Board.PendingLinks")]
        [TestCase("duplicate-pending", "Board.PendingLinks")]
        [TestCase("live-pending", "Board.PendingLinks")]
        [TestCase("foreign-pending", "Board.PendingLinks")]
        public void OriginalBoardRequiresEveryDeadPairExactlyOnce(string change, string path)
        {
            var before = State(Start(), new[] { R(0), R(0), R(20) }, R(0), new[] { 1 }, new[] { 0 }, phase: BattlePhase.AwaitLinks);
            var locked = before.Board.LockedRoutes.ToList(); var pending = before.Board.PendingLinks.ToList();
            if (change == "duplicate-lock") locked.Add(locked[0]);
            if (change == "live-lock") locked[0] = new BattleLockedRoute(before.Enemies[2].PairKey, Route(before.Board.Face, "pair:2"));
            if (change == "foreign-lock") locked[0] = new BattleLockedRoute(BattlePairKey.Create("attempt", "face:other", "pair:1"), locked[0].Route);
            if (change == "missing-dead") locked.Clear();
            if (change == "conflict") pending.Add(locked[0].PairKey);
            if (change == "duplicate-pending") pending.Add(pending[0]);
            if (change == "live-pending") pending.Add(before.Enemies[2].PairKey);
            if (change == "foreign-pending") pending.Add(BattlePairKey.Create("attempt", "face:other", "pair:0"));
            before = Copy(before, board: new BattleBoardState(before.Board.Face, locked, pending));
            RejectLink(before, Link(before), InconsistentBinding, "Before." + path);
        }

        [Test]
        public void PhaseAndPendingGateCannotBeContradicted()
        {
            var links = Copy(Start(), phase: BattlePhase.AwaitLinks);
            RejectLink(links, Link(links), InvalidPhase, "Before.Board.PendingLinks");
            var attack = Copy(LinkState(), phase: BattlePhase.AwaitAction);
            RejectAttack(attack, Attack(attack, 1), Projection(attack), InvalidPhase, "Before.Board.PendingLinks");
        }

        [TestCase("bounds", "CellOutOfBounds", 1)]
        [TestCase("diagonal", "NonAdjacent", 1)]
        [TestCase("self", "SelfIntersection", 2)]
        [TestCase("foreign", "ForeignEndpoint", 2)]
        [TestCase("overlap", "LockedOverlap", 3)]
        [TestCase("short", "PathTooShort", -1)]
        [TestCase("endpoint", "EndpointMismatch", -1)]
        public void AttackAndFreeLinkPropagateExact001Reasons(string routeCase, string reason, int cellIndex)
        {
            foreach (var linkMode in new[] { false, true })
            {
                var before = State(Start(), new[] { R(linkMode ? 0 : 20), R(0), R(20) }, locked: new[] { 1 },
                    pending: linkMode ? new[] { 0 } : null, phase: linkMode ? BattlePhase.AwaitLinks : BattlePhase.AwaitAction);
                var route = routeCase == "bounds" ? Cells(0,0,-1,0,3,0) : routeCase == "diagonal" ? Cells(0,0,1,1,3,0) :
                    routeCase == "self" ? Cells(0,0,1,0,0,0,1,0,2,0,3,0) : routeCase == "foreign" ? Cells(0,0,0,1,0,2,3,0) :
                    routeCase == "overlap" ? Cells(0,0,1,0,1,1,1,2,3,0) : routeCase == "short" ? Cells() : Cells(0,0,1,0);
                CandidateStageResult result;
                if (linkMode) { var link = Link(before); link.Route = route; result = RejectLink(before, link, InvalidRoute, "Route"); }
                else { var attack = Attack(before); attack.Route = route; result = RejectAttack(before, attack, Projection(before), InvalidRoute, "Route"); }
                Assert.AreEqual(reason, result.RouteReasonCode); Assert.AreEqual(cellIndex < 0 ? (int?)null : cellIndex, result.RouteCellIndex);
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void OriginalFixedRoutesMustBeLegalAndMutuallyDisjoint(bool overlap)
        {
            var before = State(Start(), new[] { R(20), R(0), R(0) }, locked: new[] { 1, 2 });
            var locked = before.Board.LockedRoutes.ToList();
            if (overlap) locked[1] = new BattleLockedRoute(locked[1].PairKey, Cells(0,4,1,4,1,3,1,2,2,2,2,3,2,4,3,4));
            else locked[0] = new BattleLockedRoute(locked[0].PairKey, Cells(0,2,3,2));
            before = Copy(before, board: new BattleBoardState(before.Board.Face, locked, Array.Empty<BattlePairKey>()));
            var result = RejectAttack(before, Attack(before), Projection(before), InvalidRoute,
                overlap ? "Before.Board.LockedRoutes[1].Route" : "Before.Board.LockedRoutes[0].Route");
            Assert.AreEqual(overlap ? "LockedOverlap" : "NonAdjacent", result.RouteReasonCode);
            Assert.AreEqual(overlap ? 3 : 1, result.RouteCellIndex);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void ReverseFreeLinkAndUnsolvableFutureAreStillLocallyLegal(bool linkMode)
        {
            var input = Input(pairs: 2); var face = input.Level.Faces[0]; face.Height = 4;
            face.Pairs[0].EndpointA = new FlowPos(1, 0); face.Pairs[0].EndpointB = new FlowPos(1, 3);
            face.Pairs[1].EndpointA = new FlowPos(0, 1); face.Pairs[1].EndpointB = new FlowPos(3, 1);
            var initial = Start(input);
            var before = State(initial, new[] { R(linkMode ? 0 : 20), R(linkMode ? 0 : 20) }, R(linkMode ? 0 : 100),
                pending: linkMode ? new[] { 0, 1 } : null, phase: linkMode ? BattlePhase.AwaitLinks : BattlePhase.AwaitAction);
            var route = Cells(1,3,1,2,1,1,1,0); CandidateStageDecision decision;
            if (linkMode) { var link = Link(before); link.Route = route; decision = AcceptLink(before, link); }
            else { var attack = Attack(before); attack.Route = route; var hp = Projection(before); hp.EnemyHp.ForEach(r => r.Hp = R(0)); decision = AcceptAttack(before, attack, hp); }
            CollectionAssert.AreEqual(route, decision.Board.LockedRoutes.Single().Route);
            Assert.AreEqual("pair:1", decision.Board.PendingLinks.Single().PairId); Assert.AreEqual(BattlePhase.AwaitLinks, decision.NextPhase);
            Assert.AreEqual(4, decision.Board.LockedRoutes[0].Route.Count);
        }

        [TestCase("Context.DraftRevision")]
        [TestCase("Level.RecommendedLevel")]
        [TestCase("Members.0.Level")]
        [TestCase("Members.0.Stats.MaxHp")]
        [TestCase("Members.0.Stats.Attack")]
        [TestCase("Members.0.Stats.PhysicalDefense")]
        [TestCase("Members.0.Stats.MagicDefense")]
        [TestCase("Members.0.EntryHp")]
        [TestCase("Members.0.Crit.C")]
        [TestCase("Members.0.Crit.Multiplier")]
        [TestCase("Level.Faces.1.Pairs.0.Enemy.Stats.MaxHp")]
        [TestCase("Level.Faces.1.Pairs.0.Enemy.Stats.Attack")]
        [TestCase("Level.Faces.1.Pairs.0.Enemy.IntentCycle.0.DamageCoefficient")]
        public void SmallBudgetRechecksRetainedBaselineIncludingUnusedNextFace(string path)
        {
            var input = Input(2); var big = BigInteger.One << 80;
            object number = path.EndsWith("Level") || path.EndsWith("DraftRevision") ? (object)big : path.EndsWith("Crit.C") ? R(1, big + 1) : R(big);
            Set(input, path, number);
            if (path == "Context.DraftRevision") input.Members[0].StatsContext.DraftRevision = big;
            if (path == "Members.0.EntryHp") input.Members[0].Stats.MaxHp = R(big);
            var initial = Start(input);
            foreach (var linkMode in new[] { false, true })
            {
                var before = linkMode ? State(initial, new[] { R(0), R(20), R(20) }, pending: new[] { 0 }, phase: BattlePhase.AwaitLinks) : initial;
                Limit(before, linkMode, new ExactMathBudget(64), "IntegerBits");
            }
        }

        [TestCase("revision")]
        [TestCase("expected")]
        [TestCase("actions")]
        [TestCase("phases")]
        [TestCase("cursor")]
        [TestCase("dealt")]
        [TestCase("taken")]
        [TestCase("member-hp")]
        [TestCase("enemy-hp")]
        [TestCase("words")]
        [TestCase("prd")]
        public void SmallBudgetRechecksEveryRetainedCurrentNumber(string field)
        {
            foreach (var linkMode in new[] { false, true })
            {
                var before = linkMode ? LinkState() : Start(); var big = BigInteger.One << 80;
                var members = before.Members.ToList(); var enemies = before.Enemies.ToList(); var totals = before.Contributions.ToList();
                var random = before.Random;
                if (field == "member-hp") members[0] = new BattleMemberState(members[0].CombatantKey, members[0].Member, R(1, big + 1));
                if (field == "enemy-hp" || field == "cursor") enemies[1] = new BattleEnemyState(enemies[1].CombatantKey, enemies[1].PairKey, enemies[1].Enemy,
                    field == "enemy-hp" ? R(1, big + 1) : enemies[1].Hp, field == "cursor" ? big : BigInteger.Zero);
                if (field == "dealt" || field == "taken") totals[0] = new BattleContributionTotals(members[0].CombatantKey, R(field == "dealt" ? big : 0), R(field == "taken" ? big : 0));
                if (field == "words") random = new BattleRandomSnapshot(Pcg32StreamState.Restore(random.Stream.Initial, random.Stream.Current, big, new ExactMathBudget()), random.PrdStates);
                if (field == "prd") random = new BattleRandomSnapshot(random.Stream, new[] { new BattlePrdState(members[0].CombatantKey, members[0].Member.Crit, big) });
                before = Copy(before, members, enemies, totals, random: random, revision: field == "revision" ? big : (BigInteger?)null,
                    actions: field == "actions" ? big : (BigInteger?)null, phases: field == "phases" ? big : (BigInteger?)null);
                Limit(before, linkMode, new ExactMathBudget(64), "IntegerBits", field == "expected" ? big : (BigInteger?)null);
            }
        }

        [TestCase(false, false)]
        [TestCase(false, true)]
        [TestCase(true, false)]
        [TestCase(true, true)]
        public void FinalHpNumeratorAndDenominatorUseTheSameNewBudget(bool member, bool denominator)
        {
            var before = Start(); var hp = Projection(before); var big = BigInteger.One << 80;
            (member ? hp.MemberHp : hp.EnemyHp)[0].Hp = denominator ? R(1, big + 1) : R(big);
            var original = Describe(hp); CandidateStageResult result = null;
            Assert.Throws<ExactMathLimitException>(() => result = CandidateBattleStage.AfterAttack(before, Attack(before), hp, new ExactMathBudget(64)));
            Assert.IsNull(result); Assert.AreEqual(original, Describe(hp));
            if (denominator)
            {
                var decision = AcceptAttack(before, Attack(before), hp); Assert.AreEqual(original, Describe(hp));
                Assert.AreEqual(Describe(hp.MemberHp), Describe(decision.FinalHp.MemberHp));
                Assert.AreEqual(Describe(hp.EnemyHp), Describe(decision.FinalHp.EnemyHp));
            }
            else RejectAttack(before, Attack(before), hp, InvalidValue, member ? "FinalHp.MemberHp[0].Hp" : "FinalHp.EnemyHp[0].Hp");
        }

        [TestCase(false)]
        [TestCase(true)]
        public void SharedAndLateMathLimitsProduceNoPartialDecisionAndFreshBudgetReplays(bool linkMode)
        {
            var before = linkMode ? LinkState() : Start(); var budget = new ExactMathBudget();
            var reference = linkMode ? AcceptLink(before, Link(before), budget) : AcceptAttack(before, Attack(before), Projection(before), budget);
            Assert.Greater(budget.PrimitiveStepsUsed, 1);
            Limit(before, linkMode, new ExactMathBudget(maxPrimitiveSteps: 0), "PrimitiveSteps");
            Limit(before, linkMode, new ExactMathBudget(maxPrimitiveSteps: (int)budget.PrimitiveStepsUsed - 1), "PrimitiveSteps");
            var shared = new ExactMathBudget(maxPrimitiveSteps: (int)budget.PrimitiveStepsUsed);
            ExactRational.Create(1, 1, shared); Limit(before, linkMode, shared, "PrimitiveSteps");
            var replay = linkMode ? AcceptLink(before, Link(before)) : AcceptAttack(before, Attack(before), Projection(before));
            Assert.AreEqual(Describe(reference), Describe(replay));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void EveryRequestAndHpListIsDetachedAndAllReturnedCollectionsAreReadOnly(bool linkMode)
        {
            var before = linkMode ? LinkState() : Start(); var attack = Attack(before); var link = Link(before); var hp = Projection(before);
            var decision = linkMode ? AcceptLink(before, link) : AcceptAttack(before, attack, hp);
            var original = Describe(decision);
            attack.Route.Clear(); attack.PlayerId = "mutated"; attack.Pair = before.Enemies[1].PairKey;
            link.Route.Reverse(); link.OperationId = "mutated"; link.Route.Clear();
            hp.MemberHp[0].Hp = R(0); hp.EnemyHp[0].CombatantKey = before.Members[0].CombatantKey;
            hp.EnemyHp.Clear(); hp.MemberHp.Clear();
            Assert.AreEqual(original, Describe(decision)); AssertReadOnly(decision);
        }

        private static CandidateStageDecision AcceptAttack(BattleSnapshot before, CandidateAttackRequest request,
            CandidateFinalHpProjection hp, ExactMathBudget budget = null)
        {
            var old = Describe(before); var input = Describe(request); var values = Describe(hp);
            var result = CandidateBattleStage.AfterAttack(before, request, hp, budget ?? new ExactMathBudget());
            Assert.IsTrue(result.IsAccepted, result.RejectionCode + " " + result.FieldPath);
            Assert.IsNull(result.RejectionCode); Assert.AreEqual(old, Describe(before)); Assert.AreEqual(input, Describe(request)); Assert.AreEqual(values, Describe(hp));
            return result.Decision;
        }
        private static CandidateStageDecision AcceptLink(BattleSnapshot before, CandidateLinkRequest request, ExactMathBudget budget = null)
        {
            var old = Describe(before); var input = Describe(request);
            var result = CandidateBattleStage.CompleteLink(before, request, budget ?? new ExactMathBudget());
            Assert.IsTrue(result.IsAccepted, result.RejectionCode + " " + result.FieldPath);
            Assert.AreEqual(old, Describe(before)); Assert.AreEqual(input, Describe(request)); return result.Decision;
        }
        private static CandidateStageResult RejectAttack(BattleSnapshot before, CandidateAttackRequest request,
            CandidateFinalHpProjection hp, CandidateStageRejectionCode code, string path, ExactMathBudget budget = null)
        {
            var old = Describe(before); var input = Describe(request); var values = Describe(hp);
            var result = CandidateBattleStage.AfterAttack(before, request, hp, budget ?? new ExactMathBudget());
            Rejected(result, code, path); Assert.AreEqual(old, Describe(before)); Assert.AreEqual(input, Describe(request)); Assert.AreEqual(values, Describe(hp)); return result;
        }
        private static CandidateStageResult RejectLink(BattleSnapshot before, CandidateLinkRequest request,
            CandidateStageRejectionCode code, string path, ExactMathBudget budget = null)
        {
            var old = Describe(before); var input = Describe(request);
            var result = CandidateBattleStage.CompleteLink(before, request, budget ?? new ExactMathBudget());
            Rejected(result, code, path); Assert.AreEqual(old, Describe(before)); Assert.AreEqual(input, Describe(request)); return result;
        }
        private static void Rejected(CandidateStageResult result, CandidateStageRejectionCode code, string path)
        { Assert.IsFalse(result.IsAccepted); Assert.IsNull(result.Decision); Assert.AreEqual(code, result.RejectionCode); Assert.AreEqual(path, result.FieldPath); }
        private static void Limit(BattleSnapshot before, bool linkMode, ExactMathBudget budget, string reason, BigInteger? expected = null)
        {
            var old = Describe(before); var attack = Attack(before); var link = Link(before); var hp = Projection(before);
            if (expected.HasValue) { attack.ExpectedSceneRevision = expected; link.ExpectedSceneRevision = expected; }
            var input = Describe(new object[] { attack, link, hp }); CandidateStageResult result = null;
            var error = Assert.Throws<ExactMathLimitException>(() => result = linkMode ? CandidateBattleStage.CompleteLink(before, link, budget)
                : CandidateBattleStage.AfterAttack(before, attack, hp, budget));
            Assert.AreEqual(reason, error.ReasonCode); Assert.IsNull(result); Assert.AreEqual(old, Describe(before));
            Assert.AreEqual(input, Describe(new object[] { attack, link, hp }));
        }
        private static void Facts(CandidateStageDecision decision, params CandidateStageFactKind[] expected)
        {
            CollectionAssert.AreEqual(expected, decision.OrderedFacts.Select(f => f.Kind));
            for (var i = 0; i < expected.Length; i++)
            {
                var fact = decision.OrderedFacts[i]; Assert.AreEqual(i, fact.SegmentIndex);
                Assert.AreEqual(decision.Source.OperationId, fact.OperationId); Assert.AreEqual(decision.BeforeSnapshot.SceneRevision, fact.SceneRevision);
                Assert.AreEqual(decision.BeforeSnapshot.Board.Face.FaceId, fact.FaceId);
            }
            Assert.AreEqual(decision.NextPhase, decision.OrderedFacts.Last().Phase);
        }
        private static CandidateFinalHpProjection Projection(BattleSnapshot before)
        { return new CandidateFinalHpProjection { MemberHp = before.Members.Select(m => Hp(m.CombatantKey, m.Hp)).ToList(), EnemyHp = before.Enemies.Select(e => Hp(e.CombatantKey, e.Hp)).ToList() }; }
        private static CandidateHpInput Hp(BattleCombatantKey key, ExactRational hp) { return new CandidateHpInput { CombatantKey = key, Hp = hp }; }
        private static CandidateAttackRequest Attack(BattleSnapshot before, int pair = 0)
        {
            return new CandidateAttackRequest { PlayerId = "player", AttemptId = "attempt", OperationId = " stage-operation ", ExpectedSceneRevision = before.SceneRevision,
                Actor = before.Members[0].CombatantKey, Pair = Key(before, pair), Route = Route(before.Board.Face, before.Board.Face.Pairs[pair].PairId) };
        }
        private static CandidateLinkRequest Link(BattleSnapshot before, int pair = 0)
        {
            return new CandidateLinkRequest { PlayerId = "player", AttemptId = "attempt", OperationId = "free-link", ExpectedSceneRevision = before.SceneRevision,
                Pair = Key(before, pair), Route = Route(before.Board.Face, before.Board.Face.Pairs[pair].PairId) };
        }
        private static BattlePairKey Key(BattleSnapshot before, int pair) { return BattlePairKey.Create("attempt", before.Board.Face.FaceId, before.Board.Face.Pairs[pair].PairId); }
        private static List<FlowPos> Route(PreparedFace face, string pairId)
        {
            var pair = face.Pairs.Single(p => p.PairId == pairId); var cells = new List<FlowPos>();
            if (pair.EndpointA.x == pair.EndpointB.x)
                for (var y = pair.EndpointA.y; y <= pair.EndpointB.y; y++) cells.Add(new FlowPos(pair.EndpointA.x, y));
            else for (var x = pair.EndpointA.x; x <= pair.EndpointB.x; x++) cells.Add(new FlowPos(x, pair.EndpointA.y));
            return cells;
        }
        private static BattleSnapshot InstallStageInput(CandidateStageDecision decision)
        {
            // Explicit subsequent stage input, not a production action commit or an implementation of 012.
            Assert.IsFalse(decision.DidFlipFace); var before = decision.BeforeSnapshot;
            return Copy(before, before.Members.Select(m => new BattleMemberState(m.CombatantKey, m.Member, decision.FinalHp.MemberHp.Single(h => h.CombatantKey.Equals(m.CombatantKey)).Hp)),
                before.Enemies.Select(e => new BattleEnemyState(e.CombatantKey, e.PairKey, e.Enemy, decision.FinalHp.EnemyHp.Single(h => h.CombatantKey.Equals(e.CombatantKey)).Hp, e.IntentCursor)),
                board: decision.Board, phase: decision.NextPhase);
        }
        private static BattleSnapshot LinkState() { return State(Start(), new[] { R(0), R(20), R(20) }, R(0), pending: new[] { 0 }, phase: BattlePhase.AwaitLinks); }
        private static BattleSnapshot State(BattleSnapshot before, ExactRational[] enemyHp = null, ExactRational memberHp = null,
            int[] locked = null, int[] pending = null, BattlePhase phase = BattlePhase.AwaitAction, BattleRandomSnapshot random = null,
            BigInteger? revision = null, BigInteger? actions = null, BigInteger? phases = null, IEnumerable<BattleContributionTotals> totals = null)
        {
            return Copy(before, new[] { new BattleMemberState(before.Members[0].CombatantKey, before.Members[0].Member, memberHp ?? before.Members[0].Hp) },
                before.Enemies.Select((e, i) => new BattleEnemyState(e.CombatantKey, e.PairKey, e.Enemy, enemyHp == null ? e.Hp : enemyHp[i], 5)), totals,
                new BattleBoardState(before.Board.Face, (locked ?? Array.Empty<int>()).Select(i => new BattleLockedRoute(Key(before, i), Route(before.Board.Face, before.Board.Face.Pairs[i].PairId))),
                    (pending ?? Array.Empty<int>()).Select(i => Key(before, i))), random, revision, actions, phases, phase: phase);
        }
        private static BattleSnapshot Copy(BattleSnapshot before, IEnumerable<BattleMemberState> members = null,
            IEnumerable<BattleEnemyState> enemies = null, IEnumerable<BattleContributionTotals> totals = null, BattleBoardState board = null,
            BattleRandomSnapshot random = null, BigInteger? revision = null, BigInteger? actions = null, BigInteger? phases = null,
            int? faceIndex = null, BattlePhase? phase = null)
        {
            return new BattleSnapshot(before.Baseline, revision ?? before.SceneRevision, actions ?? before.EffectiveActionsCompleted,
                phases ?? before.EnemyPhasesCompleted, faceIndex ?? before.CurrentFaceIndex, phase ?? before.Phase, board ?? before.Board,
                members ?? before.Members, enemies ?? before.Enemies, random ?? before.Random, totals ?? before.Contributions);
        }
        private static BattleSnapshot Start(BattleEntryInput input = null) { return Bind(input ?? Input()).Start.Snapshot; }
        private static CandidateRandomBinding Bind(BattleEntryInput input)
        {
            var entry = new BattleEntryPreparer().PrepareCandidate(input, new ExactMathBudget()); Assert.IsTrue(entry.IsAccepted, entry.RejectionCode + " " + entry.FieldPath);
            var bytes = new byte[48]; for (var i = 0; i < 48; i += 16) { bytes[i] = 42; bytes[i + 8] = 54; }
            var result = CandidateRandomPreparer.Prepare(entry.Entry, new CandidateSeedMaterial
            { Bytes = bytes, SourceCapabilityId = "synthetic-stage-tests", MappingId = CandidateRandomPreparer.SupportedMappingId }, new ExactMathBudget());
            Assert.IsTrue(result.IsAccepted, result.RejectionCode + " " + result.FieldPath); return result.Binding;
        }
        private static BattleEntryInput Input(int faces = 1, int pairs = 3)
        {
            var context = new CandidateContext { DraftId = "stage-test", DraftRevision = 1, ContentFingerprint = "stage-synthetic",
                RuleVersion = "stage-r1", NumericContractVersion = "exact-test", RandomContractVersion = "pcg-test",
                SourceNotes = new List<string> { "Synthetic stage projection fixtures; not AOE/DOT/whole-action proof; C=1/4 is not calibrated." } };
            var result = new BattleEntryInput
            {
                PlayerId = "player", ChallengeId = "challenge", AttemptId = "attempt", EntryBaselineId = "baseline", Context = context,
                CarryMode = EntryCarryMode.Empty, RequiredFeatures = new List<string>(),
                Level = new LevelInput { LevelId = "stage-level", LevelVersion = "v1", RecommendedLevel = 1, Faces = new List<FaceInput>() },
                Members = new List<MemberInput> { new MemberInput { CharacterId = "warrior", ClassId = "warrior", ClassKind = CharacterClassKind.Warrior,
                    OriginalSlot = 2, Level = 1, IsReady = true, StatsOrigin = BaseStatsOrigin.ComputedBaseStats,
                    StatsContext = new CandidateContext { DraftId = context.DraftId, DraftRevision = context.DraftRevision, ContentFingerprint = context.ContentFingerprint,
                        RuleVersion = context.RuleVersion, NumericContractVersion = context.NumericContractVersion, RandomContractVersion = context.RandomContractVersion, SourceNotes = new List<string>(context.SourceNotes) },
                    Stats = Stats(100), EntryHp = R(100), LearnedSkills = new List<string>(), Crit = new WarriorCritInput
                    { PassiveDefinitionId = "crit", TargetProbability = R(1, 5), C = R(1, 4), Multiplier = R(3, 2) } } }
            };
            for (var f = 0; f < faces; f++)
            {
                var face = new FaceInput { FaceId = "face:" + f, Width = 4, Height = pairs * 2 - 1, Pairs = new List<PairInput>() };
                for (var p = 0; p < pairs; p++) face.Pairs.Add(new PairInput
                {
                    PairId = "pair:" + p, GeometryColorId = p, EndpointA = new FlowPos(0, p * 2), EndpointB = new FlowPos(3, p * 2),
                    Enemy = new EnemyInput { EnemyInstanceKey = "enemy:" + p, EnemyDefinitionId = "normal", OriginalSlot = p * 3, StableOrder = pairs - p,
                        Behavior = EnemyBehavior.NormalStrike, Stats = Stats(20 + f * 10), IntentCycle = new List<EnemyIntentInput>
                        { new EnemyIntentInput { Kind = EnemyIntentKind.Strike, Targeting = EnemyTargeting.FirstLiving, DamageKind = EntryDamageKind.Physical, DamageCoefficient = R(1) } } }
                });
                result.Level.Faces.Add(face);
            }
            return result;
        }
        private static StatsInput Stats(int maxHp) { return new StatsInput { MaxHp = R(maxHp), Attack = R(20), PhysicalDefense = R(0), MagicDefense = R(0), Evasion = R(0), AttackRange = 1 }; }
        private static ExactRational R(BigInteger n) { return R(n, BigInteger.One); }
        private static ExactRational R(BigInteger n, BigInteger d) { return ExactRational.Create(n, d, new ExactMathBudget()); }
        private static void Value(ExactRational value, int n, int d = 1) { Assert.AreEqual(new BigInteger(n), value.Numerator); Assert.AreEqual(new BigInteger(d), value.Denominator); }
        private static List<FlowPos> Cells(params int[] values) { var cells = new List<FlowPos>(); for (var i = 0; i < values.Length; i += 2) cells.Add(new FlowPos(values[i], values[i + 1])); return cells; }
        private static void Set(object value, string path, object replacement)
        {
            var parts = path.Split('.');
            for (var i = 0; i < parts.Length - 1; i++) value = value is IList list ? list[int.Parse(parts[i])] : value.GetType().GetProperty(parts[i]).GetValue(value);
            if (value is IList finalList) finalList[int.Parse(parts.Last())] = replacement; else value.GetType().GetProperty(parts.Last()).SetValue(value, replacement);
        }
        private static string Describe(object value)
        {
            if (value == null) return "null"; if (value is string s) return s.Length + ":" + s;
            if (value is BigInteger n) return n.ToString(CultureInfo.InvariantCulture);
            if (value is ExactRational r) return r.Numerator + "/" + r.Denominator; if (value is FlowPos p) return p.x + "," + p.y;
            if (value.GetType().IsValueType) return value.ToString();
            if (value is IEnumerable items) return "[" + string.Join(";", items.Cast<object>().Select(Describe)) + "]";
            return "{" + string.Join(";", value.GetType().GetProperties().OrderBy(p => p.Name, StringComparer.Ordinal).Select(p => p.Name + "=" + Describe(p.GetValue(value)))) + "}";
        }
        private static void AssertReadOnly(object value)
        {
            if (value == null || value is string || value is ExactRational || value.GetType().IsValueType) return;
            if (value is IList list) { Assert.IsTrue(list.IsReadOnly); Assert.Throws<NotSupportedException>(() => list.Clear()); foreach (var item in list) AssertReadOnly(item); return; }
            Assert.IsEmpty(value.GetType().GetConstructors(BindingFlags.Public | BindingFlags.Instance));
            foreach (var property in value.GetType().GetProperties()) { Assert.IsFalse(property.CanWrite, property.Name); AssertReadOnly(property.GetValue(value)); }
        }
    }
}
