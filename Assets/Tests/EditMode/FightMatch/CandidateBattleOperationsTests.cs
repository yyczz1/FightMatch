using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Security.Cryptography;
using FlowPuzzle.Core;
using NUnit.Framework;

namespace FightMatch.Core.Tests
{
    [TestFixture]
    public class CandidateBattleOperationsTests
    {
        [TestCase(1, 1)] [TestCase(1, 2)] [TestCase(3, 1)] [TestCase(3, 2)]
        public void PublicSourceLevelsCloseOnlyAfterTheRealFinalOperation(int level, int orientation)
        {
            var input = Source(level, orientation, out var routes); var binding = Bind(input);
            var original = Create(binding); var run = original; var results = new List<CandidateBattleResult>();
            var count = level == 1 ? 2 : 3;
            for (var i = 0; i < count; i++)
            {
                var pair = level == 3 && i < 2 ? 0 : level == 1 ? i : 1;
                var result = Accept(run, Request(run.CurrentSnapshot, "op:" + i, pair, routes[pair]), time: 100 - i);
                results.Add(result); run = result.NextRun;
                Assert.AreSame(result.Record, run.Records.Last()); Assert.AreSame(result.Record.AfterSnapshot, run.CurrentSnapshot);
                Assert.IsFalse(result.Record.DirectAttack.DamageFacts.Single().Crit.Triggered,
                    "Explicit conditional public random vector with synthetic C=1/1000, not production calibration.");
                Assert.AreSame(result.Record.DirectAttack.Random, result.Record.EnemyPhase.Random);
                Assert.AreSame(result.Record.EnemyPhase.Random, run.CurrentSnapshot.Random);
                Assert.AreEqual(i + 1, (int)run.CurrentSnapshot.EffectiveActionsCompleted);
                Assert.AreEqual(i + 1, (int)run.CurrentSnapshot.EnemyPhasesCompleted);
                Assert.AreEqual(i + 2, (int)run.CurrentSnapshot.SceneRevision);
                Assert.IsFalse(run.CommitEligible); AssertFacts(result.Record);
                if (i < count - 1) Assert.IsNull(run.FinalReport);
            }
            var report = run.FinalReport;
            Assert.NotNull(report); Assert.AreEqual(BattlePhase.WonPendingSettlement, run.CurrentSnapshot.Phase);
            Assert.AreEqual(2, run.CurrentSnapshot.Board.LockedRoutes.Count); Assert.IsEmpty(run.CurrentSnapshot.Board.PendingLinks);
            Value(run.CurrentSnapshot.Members[0].Hp, level == 1 ? 95 : 90);
            Value(report.WholeLevelInitialEnemyHp, level == 1 ? 30 : 35);
            Value(run.CurrentSnapshot.Contributions[0].EffectiveDamageDealtHp, level == 1 ? 30 : 35);
            Value(run.CurrentSnapshot.Contributions[0].EffectiveDamageTakenHp, level == 1 ? 5 : 10);
            Assert.AreSame(binding, report.Binding); Assert.AreSame(binding.Start.Baseline, report.Baseline);
            Assert.AreSame(binding.Start.Snapshot, report.InitialSnapshot); Assert.AreSame(run.CurrentSnapshot, report.FinalSnapshot);
            Assert.AreEqual(count, report.Operations.Count); Assert.AreSame(results.Last().Record, report.Operations.Last());
            Assert.AreEqual(new BigInteger(101 - count), report.EndedAtUnixMilliseconds);
            Assert.AreEqual("op:" + (count - 1), report.TerminalOperationId);
            Assert.AreEqual(CandidateBattleOutcome.NormalVictory, report.Outcome); Assert.IsFalse(report.CommitEligible);
            Assert.AreEqual(CandidateConsumptionCoverage.EmptyCarryNoUse, report.ConsumptionCoverage);
            Assert.AreEqual(count, (int)report.FinalSnapshot.Random.Stream.WordsConsumed);
            Assert.AreEqual(count, (int)report.FinalSnapshot.Random.PrdStates[0].FailureCount);
            CollectionAssert.AreEqual(report.Operations.SelectMany(x => x.ContributionSegments).ToArray(), report.Contributions);
            Assert.AreEqual(report.Fingerprint, Fingerprint(report)); Assert.AreEqual(64, report.Fingerprint.Length);
            Assert.AreEqual(0, original.Records.Count); Assert.AreSame(original.InitialSnapshot, original.CurrentSnapshot);
            Value(original.CurrentSnapshot.Members[0].Hp, 100); Assert.IsNull(original.FinalReport);
            Assert.IsTrue(input.Context.SourceNotes.Any(n => n.Contains("conditional")));
            if (level == 3)
            {
                var charge = results[0].Record.EnemyPhase.OrderedIntents[0];
                Assert.AreEqual(EnemyIntentKind.Charge, charge.IntentKind); Assert.IsNull(charge.Damage);
                Assert.AreEqual(2, report.Contributions.Count(x => x.Kind == CandidateContributionKind.DamageTakenHp));
            }
        }

        [TestCase(false)] [TestCase(true)]
        public void CreationRetainsTheOriginalEntryAndAllThreeRandomDomains(bool enabled)
        {
            var input = Input(2); input.Members[0].Level = 19; var binding = Bind(input); var run = Create(binding);
            Assert.AreSame(binding, run.Binding); Assert.AreSame(binding.Start.Baseline, run.Baseline);
            Assert.AreSame(binding.Start.Snapshot, run.InitialSnapshot); Assert.AreSame(run.InitialSnapshot, run.CurrentSnapshot);
            Assert.IsEmpty(run.Records); Assert.AreEqual(BigInteger.One, run.CurrentSnapshot.SceneRevision);
            Assert.AreEqual(BigInteger.Zero, run.CurrentSnapshot.EnemyPhasesCompleted);
            Assert.AreSame(binding.BaseReward.Initial, run.Baseline.RandomInitials.BaseReward);
            Assert.AreSame(binding.Bonus.Initial, run.Baseline.RandomInitials.Bonus);
            var a = Accept(run, Request(run.CurrentSnapshot, "a"), enabled: enabled);
            var b = Accept(a.NextRun, Request(a.NextRun.CurrentSnapshot, "b", 1), enabled: !enabled);
            Assert.AreEqual(enabled, a.Record.Conditions.ItemUseEnabled); Assert.AreEqual(!enabled, b.Record.Conditions.ItemUseEnabled);
            Assert.AreEqual(new BigInteger(19), b.NextRun.FinalReport.Baseline.Entry.ReadyParticipants[0].Level);
            Assert.AreSame(binding.BaseReward.Initial, b.NextRun.FinalReport.Binding.BaseReward.Initial);
            Assert.AreEqual(BigInteger.Zero, binding.BaseReward.Initial.WordsConsumed);
            Assert.AreEqual(BigInteger.Zero, binding.Bonus.Initial.WordsConsumed);
        }

        [TestCase(2)] [TestCase(3)]
        public void FlippingCreatesOnlyTheNextFaceWithoutGivingItAnEnemyPhase(int faces)
        {
            var input = Input(1);
            for (var i = 1; i < faces; i++) { var face = Input(1).Level.Faces[0]; face.FaceId = "face:" + i; input.Level.Faces.Add(face); }
            var run = Create(Bind(input)); var snapshots = new List<BattleSnapshot>();
            for (var i = 0; i < faces; i++)
            {
                var before = run.CurrentSnapshot; var result = Accept(run, Request(before, "face-op:" + i)); run = result.NextRun;
                Assert.IsEmpty(result.Record.EnemyPhase.OrderedIntents); Assert.AreEqual(i + 1, (int)run.CurrentSnapshot.EnemyPhasesCompleted);
                Assert.AreEqual(i + 1, (int)run.CurrentSnapshot.EffectiveActionsCompleted); Value(run.CurrentSnapshot.Members[0].Hp, 100);
                if (i < faces - 1)
                {
                    Assert.AreEqual(i + 1, run.CurrentSnapshot.CurrentFaceIndex); Assert.IsNull(run.FinalReport);
                    Assert.AreEqual(input.Level.Faces[i + 1].FaceId, run.CurrentSnapshot.Board.Face.FaceId);
                    Assert.IsEmpty(run.CurrentSnapshot.Board.LockedRoutes); Assert.AreEqual(BigInteger.Zero, run.CurrentSnapshot.Enemies[0].IntentCursor);
                    Value(run.CurrentSnapshot.Enemies[0].Hp, 15); Assert.IsTrue(result.Record.StageDecision.DidFlipFace);
                    Assert.AreEqual(before.Board.Face.FaceId, result.Record.DirectAttack.DamageFacts[0].FaceId);
                }
                snapshots.Add(run.CurrentSnapshot);
            }
            Value(run.FinalReport.WholeLevelInitialEnemyHp, 15 * faces);
            Assert.AreEqual(faces, run.FinalReport.Operations.Count); Assert.AreEqual(BattlePhase.WonPendingSettlement, run.CurrentSnapshot.Phase);
            Value(snapshots[0].Enemies[0].Hp, 15);
        }

        [TestCase(false)] [TestCase(true)]
        public void N010StopsLaterChargeAndStrikeAndPublishesRescueWithTheLethalFact(bool heavy)
        {
            var input = Input(3); input.Members[0].EntryHp = R(1, 3);
            foreach (var p in input.Level.Faces[0].Pairs) p.Enemy.Stats.MaxHp = R(100);
            if (heavy) Heavy(input.Level.Faces[0].Pairs[1].Enemy);
            var run = Create(Bind(input)); var result = Accept(run, Request(run.CurrentSnapshot, "lethal"));
            var next = result.NextRun; Assert.AreEqual(BattlePhase.AwaitRescue, next.CurrentSnapshot.Phase); Assert.IsNull(next.FinalReport);
            Assert.AreEqual(1, result.Record.EnemyPhase.OrderedIntents.Count);
            var fact = result.Record.EnemyPhase.OrderedIntents[0]; Assert.IsTrue(fact.Damage.DefeatedTarget);
            Value(fact.Damage.HpLoss, 1, 3); Value(fact.Damage.Overflow, 14, 3);
            Assert.AreEqual(BigInteger.One, next.CurrentSnapshot.Enemies[0].IntentCursor);
            Assert.AreEqual(BigInteger.Zero, next.CurrentSnapshot.Enemies[1].IntentCursor);
            Assert.AreEqual(BigInteger.Zero, next.CurrentSnapshot.Enemies[2].IntentCursor);
            Assert.AreEqual(BigInteger.One, next.CurrentSnapshot.EnemyPhasesCompleted);
            Assert.IsTrue(result.Record.OrderedFacts.Any(x => ReferenceEquals(x.EnemyIntent, fact)));
            var rejected = CandidateBattleOperations.EvaluateAttack(next, Request(next.CurrentSnapshot, "after"), Conditions(), 0, Sampling());
            Reject(rejected, "InvalidPhase", CandidateBattleRejectionStage.DirectAttack);
        }

        [TestCase(false, false)] [TestCase(false, true)] [TestCase(true, false)] [TestCase(true, true)]
        public void ExplicitSyntheticLinkAssemblyHasNoCombatAndCannotBecomeAnImportedPublicRun(bool down, bool nextFace)
        {
            var input = Input(2);
            if (nextFace) { var f = Input(1).Level.Faces[0]; f.FaceId = "next"; input.Level.Faces.Add(f); }
            var run = Create(Bind(input)); var first = run.InitialSnapshot;
            // Deliberately synthetic deaths, with no claim that this single-Warrior domain can cause AOE.
            var dead = first.Enemies.Select(e => new BattleEnemyState(e.CombatantKey, e.PairKey, e.Enemy, R(0), 8)).ToArray();
            var members = down ? new[] { new BattleMemberState(first.Members[0].CombatantKey, first.Members[0].Member, R(0)) } : first.Members;
            var pending = new BattleBoardState(first.Board.Face, Array.Empty<BattleLockedRoute>(), dead.Select(e => e.PairKey));
            var state = State(first, members: members, enemies: dead, board: pending, phase: BattlePhase.AwaitLinks, actions: 7, phases: 7);
            var request = LinkRequest(state, "link:1", 0); var decision = CandidateBattleStage.CompleteLink(state, request, Math());
            Assert.IsTrue(decision.IsAccepted);
            var record = CandidateBattleOperations.AssembleRecord(CandidateBattleOperationKind.Link, 0, state, null, null, null, decision.Decision, Math());
            Assert.IsNull(record.DirectAttack); Assert.IsNull(record.EnemyPhase); Assert.IsNull(record.Conditions);
            Assert.IsEmpty(record.ContributionSegments); Assert.AreSame(state.Random, record.AfterSnapshot.Random);
            Assert.AreEqual(state.SceneRevision + 1, record.AfterSnapshot.SceneRevision);
            Assert.AreEqual(state.EffectiveActionsCompleted, record.AfterSnapshot.EffectiveActionsCompleted);
            Assert.AreEqual(state.EnemyPhasesCompleted, record.AfterSnapshot.EnemyPhasesCompleted);
            var request2 = LinkRequest(record.AfterSnapshot, "link:2", 1);
            var last = CandidateBattleStage.CompleteLink(record.AfterSnapshot, request2, Math()); Assert.IsTrue(last.IsAccepted);
            var terminal = CandidateBattleOperations.AssembleRecord(CandidateBattleOperationKind.Link, 1, record.AfterSnapshot,
                null, null, null, last.Decision, Math());
            Assert.AreEqual("link:2", terminal.OperationId); Assert.AreEqual(BigInteger.One, terminal.OccurredAtUnixMilliseconds);
            Assert.AreEqual(nextFace ? down ? BattlePhase.AwaitRescue : BattlePhase.AwaitAction : BattlePhase.WonPendingSettlement, terminal.AfterSnapshot.Phase);
            Assert.AreSame(state.Random, terminal.AfterSnapshot.Random); Assert.AreEqual(new BigInteger(7), terminal.AfterSnapshot.EnemyPhasesCompleted);
            if (nextFace) { Value(terminal.AfterSnapshot.Enemies[0].Hp, 15); Assert.AreEqual(BigInteger.Zero, terminal.AfterSnapshot.Enemies[0].IntentCursor); }
            var imported = CloneRun(run, current: state);
            Reject(CandidateBattleOperations.EvaluateLink(imported, request, 0, Math()), "IncompleteHistory");
            var fakeHistory = CloneRun(run, current: terminal.AfterSnapshot, records: new[] { record, terminal });
            Reject(CandidateBattleOperations.EvaluateLink(fakeHistory, request2, 2, Math()), "IncompleteHistory");
            Assert.IsNull(imported.FinalReport); Assert.IsNull(fakeHistory.FinalReport);
        }

        [TestCase(0)] [TestCase(1)] [TestCase(2)]
        public void RestoredHigherRevisionsKeepTheEffectivePrefixAndOriginalRecordRevisions(int prefix)
        {
            var input = Input(3); var run = Create(Bind(input));
            for (var i = 0; i < prefix; i++) run = Accept(run, Request(run.CurrentSnapshot, "old:" + i, i)).NextRun;
            var saved = run.Records.ToArray(); var current = State(run.CurrentSnapshot, revision: BigInteger.One << 80);
            var restored = CloneRun(run, current: current); var firstNew = Accept(restored, Request(current, "new:" + prefix, prefix));
            Assert.AreEqual((BigInteger.One << 80) + 1, firstNew.NextRun.CurrentSnapshot.SceneRevision);
            Assert.AreEqual(prefix + 1, firstNew.NextRun.Records.Count);
            for (var i = 0; i < saved.Length; i++) { Assert.AreSame(saved[i], firstNew.NextRun.Records[i]); Assert.AreEqual(i + 1, (int)saved[i].BeforeSnapshot.SceneRevision); }
            var end = firstNew.NextRun;
            for (var i = prefix + 1; i < 3; i++) end = Accept(end, Request(end.CurrentSnapshot, "new:" + i, i)).NextRun;
            Assert.NotNull(end.FinalReport); Assert.AreEqual(3, end.FinalReport.Operations.Count);
            Assert.AreEqual(new BigInteger(3), end.CurrentSnapshot.EffectiveActionsCompleted);
            Assert.AreEqual(BigInteger.One, end.FinalReport.InitialSnapshot.SceneRevision);
        }

        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)] [TestCase(5)] [TestCase(6)] [TestCase(7)]
        public void CreationRejectsMiddleOrUnsupportedBindings(int mutation)
        {
            var input = Input(2); var binding = Bind(input); var start = binding.Start; var state = start.Snapshot;
            switch (mutation)
            {
                case 0: start = new CandidateBattleStart(start.Baseline, State(state, revision: 2)); break;
                case 1: start = new CandidateBattleStart(start.Baseline, State(state, actions: 1)); break;
                case 2: start = new CandidateBattleStart(start.Baseline, State(state, members: new[] { new BattleMemberState(state.Members[0].CombatantKey, state.Members[0].Member, R(99)) })); break;
                case 3: start = new CandidateBattleStart(start.Baseline, State(state, enemies: state.Enemies.Select(e => new BattleEnemyState(e.CombatantKey, e.PairKey, e.Enemy, e.Hp, 1)))); break;
                case 4: input.RequiredFeatures.Add("unknown-ability"); binding = BindUnchecked(input); start = binding.Start; break;
                case 5: input.Members[0].LearnedSkills.Add("skill"); binding = BindUnchecked(input); start = binding.Start; break;
                case 6: input.CarryMode = EntryCarryMode.NonEmpty; binding = BindUnchecked(input); start = binding.Start; break;
                case 7: input.Level.Faces[0].Pairs[0].Enemy.Behavior = (EnemyBehavior)99; binding = BindUnchecked(input); start = binding.Start; break;
            }
            binding = new CandidateRandomBinding(start, binding.SourceCapabilityId, binding.MappingId, binding.Battle, binding.BaseReward, binding.Bonus);
            Reject(CandidateBattleOperations.CreateCandidate(binding, Math()), mutation >= 4 ? "UnsupportedBinding" : "InconsistentBinding");
        }

        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)] [TestCase(5)]
        public void RandomBindingDomainsAndInitialPrdCannotBeSubstituted(int mutation)
        {
            var b = Bind(Input(1)); var domain = b.Battle; var start = b.Start;
            if (mutation == 0) domain = new CandidateRandomDomain(CandidateRandomPurpose.Bonus, domain.InitState, domain.InitSequence, domain.Initial);
            if (mutation == 1) domain = new CandidateRandomDomain(domain.Purpose, domain.InitState + 1, domain.InitSequence, domain.Initial);
            if (mutation == 2) domain = new CandidateRandomDomain(domain.Purpose, domain.InitState, ulong.MaxValue, domain.Initial);
            if (mutation == 3) domain = new CandidateRandomDomain(domain.Purpose, domain.InitState, domain.InitSequence, Pcg32StreamState.Initialize(1, 2));
            if (mutation == 4) start = new CandidateBattleStart(start.Baseline, State(start.Snapshot, random: new BattleRandomSnapshot(domain.Initial,
                new[] { new BattlePrdState(start.Snapshot.Members[0].CombatantKey, start.Snapshot.Members[0].Member.Crit, 1) })));
            var bad = new CandidateRandomBinding(start, b.SourceCapabilityId, mutation == 5 ? "unknown" : b.MappingId, domain, b.BaseReward, b.Bonus);
            Reject(CandidateBattleOperations.CreateCandidate(bad, Math()), mutation == 5 ? "UnsupportedBinding" : "InconsistentBinding");
        }

        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)] [TestCase(5)] [TestCase(6)] [TestCase(7)] [TestCase(8)]
        public void NullRootsHaveNamedArgumentErrors(int which)
        {
            var run = Create(Bind(Input(1))); var request = Request(run.CurrentSnapshot, "op"); var link = LinkRequest(run.CurrentSnapshot, "link", 0);
            var names = new[] { "binding", "budget", "run", "request", "conditions", "budget", "run", "request", "budget" };
            TestDelegate call = () => CandidateBattleOperations.CreateCandidate(null, Math());
            if (which == 1) call = () => CandidateBattleOperations.CreateCandidate(run.Binding, null);
            if (which >= 2 && which <= 5) call = () => CandidateBattleOperations.EvaluateAttack(which == 2 ? null : run,
                which == 3 ? null : request, which == 4 ? null : Conditions(), 0, which == 5 ? null : Sampling());
            if (which >= 6) call = () => CandidateBattleOperations.EvaluateLink(which == 6 ? null : run, which == 7 ? null : link, 0, which == 8 ? null : Math());
            Assert.AreEqual(names[which], Assert.Throws<ArgumentNullException>(call).ParamName);
        }

        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)] [TestCase(5)]
        public void MissingAndInvalidConditionsAndTimeHaveNoCandidate(int mutation)
        {
            var run = Create(Bind(Input(1))); var conditions = Conditions(); BigInteger? time = 0;
            if (mutation == 0) conditions.PreferenceRevision = null;
            if (mutation == 1) conditions.ItemUseEnabled = null;
            if (mutation == 2) conditions.PreferenceRevision = 0;
            if (mutation == 3) conditions.PreferenceRevision = -1;
            if (mutation == 4) time = null;
            if (mutation == 5) time = -1;
            var budget = Sampling(); var before = Describe(run);
            Reject(CandidateBattleOperations.EvaluateAttack(run, Request(run.CurrentSnapshot, "x"), conditions, time, budget),
                mutation == 0 || mutation == 1 || mutation == 4 ? "MissingField" : "InvalidValue");
            Assert.AreEqual(0, budget.WordsUsed); Assert.AreEqual(before, Describe(run));
        }

        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)] [TestCase(5)] [TestCase(6)]
        public void Original009RejectionsAndGeometryDetailsPassThrough(int mutation)
        {
            var run = Create(Bind(Input(2))); var request = Request(run.CurrentSnapshot, "x");
            if (mutation == 0) request.ExpectedSceneRevision = 0;
            if (mutation == 1) request.PlayerId = "wrong";
            if (mutation == 2) request.Actor = BattleCombatantKey.ForParticipant(request.AttemptId, "wrong");
            if (mutation == 3) request.Route[1] = new FlowPos(-1, 0);
            if (mutation == 4) request.Route.Clear();
            if (mutation == 5) request = Request(run.CurrentSnapshot, "x", 1);
            if (mutation == 6) request.Pair = null;
            var expected = CandidateDirectAttack.Evaluate(run.Binding, run.CurrentSnapshot, request, Sampling()); Assert.IsFalse(expected.IsAccepted);
            var budget = Sampling(); var actual = CandidateBattleOperations.EvaluateAttack(run, request, Conditions(), 0, budget);
            Reject(actual, expected.RejectionCode.ToString(), CandidateBattleRejectionStage.DirectAttack);
            Assert.AreEqual(expected.FieldPath, actual.FieldPath); Assert.AreEqual(expected.RouteReasonCode, actual.RouteReasonCode);
            Assert.AreEqual(expected.RouteCellIndex, actual.RouteCellIndex); Assert.AreEqual(0, budget.WordsUsed);
        }

        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)] [TestCase(5)] [TestCase(6)] [TestCase(7)] [TestCase(8)] [TestCase(9)]
        public void IncompleteOrAlteredStateChainsCannotBeContinuedToAReport(int mutation)
        {
            var original = Create(Bind(Input(3))); var a = Accept(original, Request(original.CurrentSnapshot, "a"));
            var b = Accept(a.NextRun, Request(a.NextRun.CurrentSnapshot, "b", 1)); var run = b.NextRun;
            var state = run.CurrentSnapshot; var records = run.Records.ToArray(); var baseline = run.Baseline; var binding = run.Binding;
            switch (mutation)
            {
                case 0: records = Array.Empty<CandidateBattleOperationRecord>(); break;
                case 1: records = new[] { records[1] }; break;
                case 2: records = new[] { records[0] }; break;
                case 3: state = State(state, revision: 1); break;
                case 4: state = State(state, actions: 99); break;
                case 5: state = State(state, phases: 99); break;
                case 6: state = State(state, members: new[] { new BattleMemberState(state.Members[0].CombatantKey, state.Members[0].Member, R(1)) }); break;
                case 7: state = State(state, contributions: new[] { new BattleContributionTotals(state.Members[0].CombatantKey, R(31), R(15)) }); break;
                case 8: binding = Bind(Input(3)); break;
                case 9: state = State(state, phase: BattlePhase.WonPendingSettlement); break;
            }
            var broken = new CandidateBattleRun(binding, baseline, run.InitialSnapshot, state, records, null);
            var outcome = CandidateBattleOperations.EvaluateAttack(broken, Request(state, "last", 2), Conditions(), 0, Sampling());
            Reject(outcome, mutation == 8 ? "InconsistentBinding" : "IncompleteHistory"); Assert.IsNull(run.FinalReport);
        }

        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)] [TestCase(5)] [TestCase(6)] [TestCase(7)] [TestCase(8)] [TestCase(9)] [TestCase(10)] [TestCase(11)] [TestCase(12)] [TestCase(13)] [TestCase(14)]
        public void MissingFragmentsAndForgedContributionOrFactRowsCannotSeal(int mutation)
        {
            var initial = Create(Bind(Input(2))); var result = Accept(initial, Request(initial.CurrentSnapshot, "a")); var r = result.Record;
            var segments = r.ContributionSegments.ToList(); var facts = r.OrderedFacts.ToList(); var direct = r.DirectAttack;
            var enemy = r.EnemyPhase; var stage = r.StageDecision; var conditions = r.Conditions; var after = r.AfterSnapshot;
            if (mutation == 0) direct = null;
            if (mutation == 1) enemy = null;
            if (mutation == 2) segments.Add(segments[0]);
            if (mutation == 3) segments.RemoveAt(0);
            if (mutation == 4) segments[0] = Segment(segments[0], beneficiary: BattleCombatantKey.ForParticipant(initial.Baseline.Entry.AttemptId, "other"));
            if (mutation == 5) segments[0] = Segment(segments[0], loss: R(16));
            if (mutation == 6) segments[0] = Segment(segments[0], face: "other");
            if (mutation == 7) segments[0] = Segment(segments[0], factIndex: 1);
            if (mutation == 8) facts.RemoveAt(facts.Count - 1);
            if (mutation == 9) conditions = null;
            if (mutation == 10) after = State(after, revision: 99);
            if (mutation == 11) stage = new CandidateStageDecision(stage.BeforeSnapshot,
                new CandidateStageSource(stage.Source.Kind, stage.Source.PlayerId, stage.Source.AttemptId, "other", stage.Source.ExpectedSceneRevision,
                    stage.Source.Actor, stage.Source.Pair, stage.Source.Route), stage.FinalHp, stage.Board, stage.NextFaceIndex, stage.NextPhase, stage.NextFace, stage.OrderedFacts);
            if (mutation == 12) after = State(after, members: new[] { new BattleMemberState(after.Members[0].CombatantKey, after.Members[0].Member, R(94)) });
            if (mutation == 13)
            {
                var h = direct.DamageFacts[0];
                var hit = new BattleDamageFact(r.BeforeSnapshot, direct.Action, r.BeforeSnapshot.Enemies[0], h.Attack, h.Multiplier,
                    R(21), h.MitigatedDamage, h.RoundedDamage, h.HpAfter, h.HpLoss, h.Overflow, h.BlockPrevented, h.Crit);
                direct = new CandidateCombatFrame(direct.Binding, direct.BeforeSnapshot, direct.Action, direct.Enemies, direct.Random, direct.Contributions, hit);
                enemy = new CandidateEnemyPhaseFrame(direct, enemy.EnemyPhaseOrdinal, enemy.Members, enemy.Enemies, enemy.Contributions, enemy.OrderedIntents);
            }
            var bad = new CandidateBattleOperationRecord(r.Kind, r.OccurredAtUnixMilliseconds, r.BeforeSnapshot, after, conditions,
                direct, enemy, stage, facts, segments, mutation == 14 ? (CandidateConsumptionCoverage)99 : r.ConsumptionCoverage);
            var broken = CloneRun(result.NextRun, records: new[] { bad }, current: after);
            var outcome = CandidateBattleOperations.EvaluateAttack(broken, Request(after, "last", 1), Conditions(), 0, Sampling());
            Reject(outcome, "IncompleteHistory"); Assert.IsNull(outcome.NextRun); Assert.IsNull(result.NextRun.FinalReport);
        }

        [TestCase(false)] [TestCase(true)]
        public void RepeatedOperationIdsAndTerminalRunsNeverActAgain(bool terminal)
        {
            var start = Create(Bind(Input(2))); var first = Accept(start, Request(start.CurrentSnapshot, "same")); var run = first.NextRun;
            if (terminal) run = Accept(run, Request(run.CurrentSnapshot, "last", 1)).NextRun;
            var budget = Sampling(); var state = Describe(run);
            Reject(CandidateBattleOperations.EvaluateAttack(run, Request(run.CurrentSnapshot, terminal ? "new" : "same", 1), Conditions(), 0, budget),
                terminal ? "InvalidPhase" : "OperationConflict");
            Assert.AreEqual(0, budget.WordsUsed); Assert.AreEqual(state, Describe(run));
            if (terminal) Reject(CandidateBattleOperations.EvaluateLink(run, LinkRequest(run.CurrentSnapshot, "new-link", 1), 0, Math()), "InvalidPhase");
        }

        [Test]
        public void PublicLinkKeeps011InvalidPhaseCodeAndNeverInventsConditions()
        {
            var run = Create(Bind(Input(1))); var request = LinkRequest(run.CurrentSnapshot, "link", 0);
            var expected = CandidateBattleStage.CompleteLink(run.CurrentSnapshot, request, Math());
            var result = CandidateBattleOperations.EvaluateLink(run, request, 0, Math());
            Reject(result, expected.RejectionCode.ToString(), CandidateBattleRejectionStage.Stage);
            Assert.AreEqual(expected.FieldPath, result.FieldPath);
            Reject(CandidateBattleOperations.EvaluateLink(run, request, null, Math()), "MissingField");
        }

        [TestCase(false)] [TestCase(true)]
        public void NumericallyIdenticalForeignDefinitionsAreNotOriginalFragmentReferences(bool board)
        {
            var input = Input(2); var start = Create(Bind(input)); var result = Accept(start, Request(start.CurrentSnapshot, "a"));
            var r = result.Record; var enemy = r.EnemyPhase; var stage = r.StageDecision;
            if (board)
                stage = new CandidateStageDecision(stage.BeforeSnapshot, stage.Source, stage.FinalHp,
                    new BattleBoardState(new PreparedFace(input.Level.Faces[0]), stage.Board.LockedRoutes, stage.Board.PendingLinks),
                    stage.NextFaceIndex, stage.NextPhase, stage.NextFace, stage.OrderedFacts);
            else
                enemy = new CandidateEnemyPhaseFrame(r.DirectAttack, enemy.EnemyPhaseOrdinal,
                    new[] { new BattleMemberState(enemy.Members[0].CombatantKey, new PreparedMember(input.Members[0]), enemy.Members[0].Hp) },
                    enemy.Enemies, enemy.Contributions, enemy.OrderedIntents);
            var bad = new CandidateBattleOperationRecord(r.Kind, r.OccurredAtUnixMilliseconds, r.BeforeSnapshot, r.AfterSnapshot,
                r.Conditions, r.DirectAttack, enemy, stage, r.OrderedFacts, r.ContributionSegments);
            var broken = CloneRun(result.NextRun, records: new[] { bad });
            Reject(CandidateBattleOperations.EvaluateAttack(broken, Request(broken.CurrentSnapshot, "last", 1), Conditions(), 0, Sampling()),
                board ? "IncompleteHistory" : "InconsistentBinding");
        }

        [Test]
        public void CreationBudgetAndTerminalEncodingHaveNoPartialResult()
        {
            var binding = Bind(Input(1)); CandidateBattleResult result = null;
            Assert.Throws<ExactMathLimitException>(() => result = CandidateBattleOperations.CreateCandidate(binding, new ExactMathBudget(32768, 0)));
            Assert.IsNull(result); Assert.AreSame(binding.Start.Snapshot, Create(binding).InitialSnapshot);
            var report = Win(Input(1)); var math = Math(); var expected = Fingerprint(report, math);
            Assert.Throws<ExactMathLimitException>(() => Fingerprint(report, new ExactMathBudget(32768, checked((int)math.PrimitiveStepsUsed - 1))));
            Assert.AreEqual(expected, Fingerprint(report));
        }

        [Test]
        public void Fmbr01HasTheExactIndependent51ByteVector()
        {
            byte[] bytes;
            using (var output = new MemoryStream()) { new Fmbr01Writer(output, Math()).Record("x", R(-1, 2), "z", "中😀"); bytes = output.ToArray(); }
            Assert.AreEqual(51, bytes.Length);
            Assert.AreEqual("464d425230310a0602000000040100000078000302020000002d3102010000003204010000007a0004030000002d4e3dd800de", CandidateBattleReportFingerprint.Hex(bytes));
            using (var sha = SHA256.Create()) Assert.AreEqual("c3918a41722ceee31527635fd94a3b52faff87f80bd40235f136b0b8bd13465c", CandidateBattleReportFingerprint.Hex(sha.ComputeHash(bytes)));
        }

        [TestCase("en-US")] [TestCase("tr-TR")] [TestCase("ar-SA")] [TestCase("zh-CN")]
        public void FingerprintIsCultureIndependentAndPreservesOriginalUtf16(string culture)
        {
            var input = Input(1); input.PlayerId = "a|中😀\ud800"; input.Context.SourceNotes.Add("e\u0301 / é / |:;");
            input.Members[0].StatsContext = Context(input.Context); var report = Win(input);
            var oldCulture = CultureInfo.CurrentCulture; var oldUi = CultureInfo.CurrentUICulture;
            try { CultureInfo.CurrentCulture = new CultureInfo(culture); CultureInfo.CurrentUICulture = new CultureInfo(culture); Assert.AreEqual(report.Fingerprint, Fingerprint(report)); }
            finally { CultureInfo.CurrentCulture = oldCulture; CultureInfo.CurrentUICulture = oldUi; }
            Assert.AreEqual(input.PlayerId, report.PlayerId);
            Assert.AreNotEqual(Bytes(null), Bytes("")); Assert.AreNotEqual(Bytes("é"), Bytes("e\u0301"));
            Assert.AreNotEqual(Bytes("a|b"), Bytes(new[] { "a", "b" }));
            Assert.AreEqual("464d425230310a040100000000d8", Bytes("\ud800"));
        }

        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)] [TestCase(5)] [TestCase(6)] [TestCase(7)]
        public void EachReportSourceGroupIsIncludedAndOnlyFingerprintIsExcluded(int mutation)
        {
            var report = Win(Input(1)); var original = Fingerprint(report); var binding = report.Binding; var baseline = report.Baseline;
            var operations = report.Operations.ToArray(); var contributions = report.Contributions.ToArray(); var time = report.EndedAtUnixMilliseconds;
            if (mutation == 0) binding = new CandidateRandomBinding(binding.Start, "changed-source", binding.MappingId, binding.Battle, binding.BaseReward, binding.Bonus);
            if (mutation == 1) { var input = Input(1); input.Level.Faces[0].Pairs[0].Enemy.Stats.MagicDefense = R(17); baseline = Bind(input).Start.Baseline; }
            if (mutation == 2) time++;
            if (mutation == 3) { var input = Input(1); var run = Create(Bind(input)); var request = Request(run.CurrentSnapshot, "op:0"); request.Route.Reverse(); operations[0] = Accept(run, request).Record; }
            if (mutation == 4) contributions[0] = Segment(contributions[0], loss: R(14));
            if (mutation == 5) operations[0] = CloneRecord(operations[0], time: 999);
            if (mutation == 6)
            {
                var r = operations[0]; var h = r.DirectAttack.DamageFacts[0];
                var hit = new BattleDamageFact(r.BeforeSnapshot, r.DirectAttack.Action, r.BeforeSnapshot.Enemies[0], h.Attack, h.Multiplier,
                    R(21), h.MitigatedDamage, h.RoundedDamage, h.HpAfter, h.HpLoss, h.Overflow, h.BlockPrevented, h.Crit);
                var frame = new CandidateCombatFrame(r.DirectAttack.Binding, r.BeforeSnapshot, r.DirectAttack.Action, r.DirectAttack.Enemies,
                    r.DirectAttack.Random, r.DirectAttack.Contributions, hit);
                operations[0] = CloneRecord(r, direct: frame);
            }
            var changed = new CandidateFinalAttemptReport(binding, baseline, report.InitialSnapshot, operations, report.FinalSnapshot,
                contributions, report.Outcome, time, report.TerminalOperationId, report.ConsumptionCoverage, report.WholeLevelInitialEnemyHp, "arbitrary-self-field");
            // Encoding-only probes are deliberately not claimed to be valid closed battle reports.
            if (mutation == 7) Assert.AreEqual(original, Fingerprint(changed)); else Assert.AreNotEqual(original, Fingerprint(changed));
        }

        [Test]
        public void FixedSchemaIncludesEveryFrozenOriginalFactFieldInDeclarationOrder()
        {
            var report = Win(Input(2)); var tree = Decode(CandidateBattleReportFingerprint.Encode(report, Math()));
            CollectionAssert.AreEqual(new[] { "Binding", "Baseline", "InitialSnapshot", "Operations", "FinalSnapshot", "Contributions", "Outcome",
                "EndedAtUnixMilliseconds", "TerminalOperationId", "ConsumptionCoverage", "WholeLevelInitialEnemyHp", "CommitEligible" }, Keys(tree));
            var baseline = Field(tree, "Baseline"); var entry = Field(baseline, "Entry");
            CollectionAssert.AreEqual(DataNames(typeof(PreparedBattleEntry)), Keys(entry));
            CollectionAssert.AreEqual(DataNames(typeof(PreparedCandidateContext)), Keys(Field(entry, "Context")));
            CollectionAssert.AreEqual(DataNames(typeof(PreparedLevel)), Keys(Field(entry, "Level")));
            var member = First(Field(entry, "Members")); CollectionAssert.AreEqual(DataNames(typeof(PreparedMember)), Keys(member));
            CollectionAssert.AreEqual(DataNames(typeof(PreparedStats)), Keys(Field(member, "Stats")));
            CollectionAssert.AreEqual(DataNames(typeof(PreparedWarriorCrit)), Keys(Field(member, "Crit")));
            var face = First(Field(Field(entry, "Level"), "Faces")); CollectionAssert.AreEqual(DataNames(typeof(PreparedFace)), Keys(face));
            var pair = First(Field(face, "Pairs")); CollectionAssert.AreEqual(DataNames(typeof(PreparedPair)), Keys(pair));
            var enemy = Field(pair, "Enemy"); CollectionAssert.AreEqual(DataNames(typeof(PreparedEnemy)), Keys(enemy));
            CollectionAssert.AreEqual(DataNames(typeof(PreparedEnemyIntent)), Keys(First(Field(enemy, "IntentCycle"))));
            var operation = First(Field(tree, "Operations")); var direct = First(Field(operation, "DirectFacts"));
            CollectionAssert.AreEqual(DataNames(typeof(BattleDamageFact)), Keys(direct));
            var crit = Field(direct, "Crit"); CollectionAssert.AreEqual(DataNames(typeof(CandidateCritFact)), Keys(crit));
            var intent = First(Field(operation, "EnemyFacts")); CollectionAssert.AreEqual(DataNames(typeof(CandidateEnemyIntentFact)), Keys(intent));
            CollectionAssert.AreEqual(DataNames(typeof(CandidateEnemyDamageFact)), Keys(Field(intent, "Damage")));
            CollectionAssert.AreEqual(DataNames(typeof(CandidateStageFact)), Keys(First(Field(operation, "StageFacts"))));
            CollectionAssert.AreEqual(DataNames(typeof(CandidateContributionSegment)), Keys(First(Field(tree, "Contributions"))));
            Assert.AreEqual(report.Baseline.Entry.EntryBaselineId, Field(direct, "Baseline"));
            Assert.IsInstanceOf<byte[]>(Field(crit, "Words")); Assert.AreEqual(4, ((byte[])Field(crit, "Words")).Length);
            Assert.AreEqual(16, ((byte[])Field(Field(crit, "StreamBefore"), "InitialCore")).Length);
        }

        [Test]
        public void UnknownEnumsAreRejectedInsteadOfBeingSerializedAsNumbers()
        {
            var report = Win(Input(1)); var bad = new CandidateFinalAttemptReport(report.Binding, report.Baseline, report.InitialSnapshot,
                report.Operations, report.FinalSnapshot, report.Contributions, (CandidateBattleOutcome)99, 0, report.TerminalOperationId,
                report.ConsumptionCoverage, report.WholeLevelInitialEnemyHp, null);
            Assert.Throws<ArgumentException>(() => Fingerprint(bad));
            Assert.Throws<ArgumentNullException>(() => CandidateBattleReportFingerprint.Compute(null, Math()));
            Assert.Throws<ArgumentNullException>(() => CandidateBattleReportFingerprint.Compute(report, null));
        }

        [Test]
        public void RequestsConditionsAndAllPublicOutputCollectionsAreIsolated()
        {
            var run = Create(Bind(Input(1))); var request = Request(run.CurrentSnapshot, "op"); var conditions = Conditions();
            var result = CandidateBattleOperations.EvaluateAttack(run, request, conditions, 0, Sampling()); Assert.IsTrue(result.IsAccepted);
            var text = Describe(result.NextRun); request.Route.Clear(); request.OperationId = "changed"; request.Actor = null;
            conditions.PreferenceRevision = 300; conditions.ItemUseEnabled = !conditions.ItemUseEnabled;
            Assert.AreEqual(text, Describe(result.NextRun)); AssertReadOnly(result.NextRun, new HashSet<object>());
            Assert.IsNull(run.FinalReport); Assert.IsEmpty(run.Records);
        }

        [TestCase(false)] [TestCase(true)]
        public void LateSharedMathAndSamplingLimitsPublishNoPartialRunOrReport(bool closing)
        {
            var start = Create(Bind(Input(2))); var run = closing ? Accept(start, Request(start.CurrentSnapshot, "a")).NextRun : start;
            var request = Request(run.CurrentSnapshot, "try", closing ? 1 : 0); var before = Describe(run);
            var math = Math(); var success = CandidateBattleOperations.EvaluateAttack(run, request, Conditions(), 0, new RandomSamplingBudget(math));
            Assert.IsTrue(success.IsAccepted); var steps = checked((int)math.PrimitiveStepsUsed); Assert.Greater(steps, 1);
            foreach (var limit in new[] { 0, steps - 1 })
            {
                CandidateBattleResult failed = null;
                Assert.Throws<ExactMathLimitException>(() => failed = CandidateBattleOperations.EvaluateAttack(run, request, Conditions(), 0,
                    new RandomSamplingBudget(new ExactMathBudget(32768, limit))));
                Assert.IsNull(failed); Assert.AreEqual(before, Describe(run));
            }
            var shared = new ExactMathBudget(32768, steps); shared.CheckInteger(0); CandidateBattleResult sharedFailure = null;
            Assert.Throws<ExactMathLimitException>(() => sharedFailure = CandidateBattleOperations.EvaluateAttack(run, request, Conditions(), 0, new RandomSamplingBudget(shared)));
            Assert.IsNull(sharedFailure);
            var noWords = new RandomSamplingBudget(Math(), 0); CandidateBattleResult sampleFailure = null;
            Assert.Throws<ExactMathLimitException>(() => sampleFailure = CandidateBattleOperations.EvaluateAttack(run, request, Conditions(), 0, noWords));
            Assert.IsNull(sampleFailure); Assert.AreEqual(0, noWords.WordsUsed); Assert.AreEqual(before, Describe(run));
            var oneWord = new RandomSamplingBudget(Math(), 1);
            var retry = CandidateBattleOperations.EvaluateAttack(run, request, Conditions(), 0, oneWord);
            Assert.IsTrue(retry.IsAccepted); Assert.AreEqual(1, oneWord.WordsUsed, "Checking retained history must not resample old opportunities.");
            Assert.AreEqual(Describe(success.NextRun), Describe(retry.NextRun));
            if (closing)
            {
                Assert.NotNull(retry.NextRun.FinalReport); var encoding = Math(); Fingerprint(retry.NextRun.FinalReport, encoding);
                Assert.Throws<ExactMathLimitException>(() => Fingerprint(retry.NextRun.FinalReport, new ExactMathBudget(32768, checked((int)encoding.PrimitiveStepsUsed - 1))));
            }
        }

        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)] [TestCase(5)]
        public void TheSameSmallBudgetRechecksLargeRetainedInputsAndHistory(int mutation)
        {
            var input = Input(2); var huge = BigInteger.One << 100;
            if (mutation == 0) input.Members[0].Level = huge;
            if (mutation == 1) input.Level.RecommendedLevel = huge;
            if (mutation == 2) input.Level.Faces[0].Pairs[1].Enemy.Stats.MagicDefense = R(huge);
            var run = Create(Bind(input));
            if (mutation >= 3)
            {
                var first = CandidateBattleOperations.EvaluateAttack(run, Request(run.CurrentSnapshot, "a"),
                    new CandidateBattleConditions { PreferenceRevision = mutation == 4 ? huge : 1, ItemUseEnabled = true }, mutation == 3 ? huge : 0, Sampling());
                Assert.IsTrue(first.IsAccepted); run = first.NextRun;
                if (mutation == 5) run = CloneRun(run, current: State(run.CurrentSnapshot, revision: huge));
            }
            var before = Describe(run); var current = run;
            Assert.Throws<ExactMathLimitException>(() => CandidateBattleOperations.EvaluateAttack(current,
                Request(current.CurrentSnapshot, "next", mutation >= 3 ? 1 : 0), Conditions(), 0, new RandomSamplingBudget(new ExactMathBudget(64))));
            Assert.AreEqual(before, Describe(run));
            Assert.IsTrue(CandidateBattleOperations.EvaluateAttack(run, Request(run.CurrentSnapshot, "next", mutation >= 3 ? 1 : 0), Conditions(), 0, Sampling()).IsAccepted);
        }

        [Test]
        public void ZeroHpLossStillHasAContributionSegmentAndCertainCritUsesNoWords()
        {
            var input = Input(1); input.Members[0].Stats.Attack = R(0); input.Members[0].Crit.C = R(1);
            var run = Create(Bind(input)); var budget = new RandomSamplingBudget(Math(), 0);
            var result = CandidateBattleOperations.EvaluateAttack(run, Request(run.CurrentSnapshot, "zero"), Conditions(), 0, budget);
            Assert.IsTrue(result.IsAccepted); Assert.AreEqual(2, result.Record.ContributionSegments.Count);
            Value(result.Record.ContributionSegments[0].HpLoss, 0); Assert.IsTrue(result.Record.DirectAttack.DamageFacts[0].Crit.Triggered);
            Assert.IsEmpty(result.Record.DirectAttack.DamageFacts[0].Crit.Words); Assert.AreEqual(0, budget.WordsUsed);
            Assert.IsNull(result.NextRun.FinalReport); AssertFacts(result.Record);
        }

        private static CandidateBattleRun Create(CandidateRandomBinding binding)
        { var r = CandidateBattleOperations.CreateCandidate(binding, Math()); Assert.IsTrue(r.IsAccepted, r.RejectionCode + " " + r.FieldPath); Assert.IsNull(r.NextRun); return r.Run; }
        private static CandidateBattleResult Accept(CandidateBattleRun run, CandidateAttackRequest request, bool enabled = true, int time = 0)
        { var r = CandidateBattleOperations.EvaluateAttack(run, request, new CandidateBattleConditions { PreferenceRevision = 7, ItemUseEnabled = enabled }, time, Sampling());
            Assert.IsTrue(r.IsAccepted, r.RejectionStage + " " + r.RejectionCode + " " + r.FieldPath); Assert.IsNull(r.Run); return r; }
        private static void Reject(CandidateBattleResult result, string code, CandidateBattleRejectionStage stage = CandidateBattleRejectionStage.Run)
        { Assert.IsFalse(result.IsAccepted); Assert.IsNull(result.Run); Assert.IsNull(result.NextRun); Assert.IsNull(result.Record);
            Assert.AreEqual(code, result.RejectionCode); Assert.AreEqual(stage, result.RejectionStage); Assert.IsNotEmpty(result.FieldPath); }
        private static CandidateBattleConditions Conditions() { return new CandidateBattleConditions { PreferenceRevision = 7, ItemUseEnabled = true }; }
        private static ExactMathBudget Math() { return new ExactMathBudget(); }
        private static RandomSamplingBudget Sampling() { return new RandomSamplingBudget(Math()); }
        private static ExactRational R(BigInteger n) { return R(n, 1); }
        private static ExactRational R(BigInteger n, BigInteger d) { return ExactRational.Create(n, d, Math()); }
        private static void Value(ExactRational value, int n, int d = 1) { Assert.AreEqual(new BigInteger(n), value.Numerator); Assert.AreEqual(new BigInteger(d), value.Denominator); }
        private static string Fingerprint(CandidateFinalAttemptReport report, ExactMathBudget math = null) { return CandidateBattleReportFingerprint.Compute(report, math ?? Math()); }
        private static string Bytes(object value) { return CandidateBattleReportFingerprint.Hex(CandidateBattleReportFingerprint.Encode(value, Math())); }
        private static CandidateFinalAttemptReport Win(BattleEntryInput input)
        {
            var run = Create(Bind(input));
            for (var i = 0; i < input.Level.Faces[0].Pairs.Count; i++) run = Accept(run, Request(run.CurrentSnapshot, "op:" + i, i)).NextRun;
            Assert.NotNull(run.FinalReport); return run.FinalReport;
        }
        private static CandidateAttackRequest Request(BattleSnapshot state, string operation, int index = 0, List<FlowPos> route = null)
        {
            var pair = state.Board.Face.Pairs[index];
            return new CandidateAttackRequest { PlayerId = state.Baseline.Entry.PlayerId, AttemptId = state.Baseline.Entry.AttemptId,
                OperationId = operation, ExpectedSceneRevision = state.SceneRevision, Actor = state.Members[0].CombatantKey,
                Pair = BattlePairKey.Create(state.Baseline.Entry.AttemptId, state.Board.Face.FaceId, pair.PairId), Route = route ?? Route(pair) };
        }
        private static CandidateLinkRequest LinkRequest(BattleSnapshot state, string operation, int index)
        { var r = Request(state, operation, index); return new CandidateLinkRequest { PlayerId = r.PlayerId, AttemptId = r.AttemptId,
            OperationId = operation, ExpectedSceneRevision = state.SceneRevision, Pair = r.Pair, Route = r.Route }; }
        private static List<FlowPos> Route(PreparedPair pair)
        { return new List<FlowPos> { pair.EndpointA, new FlowPos(1, pair.EndpointA.y), new FlowPos(2, pair.EndpointA.y), pair.EndpointB }; }
        private static CandidateRandomBinding Bind(BattleEntryInput input)
        {
            var entry = new BattleEntryPreparer().PrepareCandidate(input, Math()); Assert.IsTrue(entry.IsAccepted, entry.RejectionCode + " " + entry.FieldPath);
            return BindPrepared(entry.Entry);
        }
        private static CandidateRandomBinding BindUnchecked(BattleEntryInput input) { return BindPrepared(new PreparedBattleEntry(input)); }
        private static CandidateRandomBinding BindPrepared(PreparedBattleEntry entry)
        {
            var bytes = new byte[48]; for (var offset = 0; offset < 48; offset += 16) { bytes[offset] = 42; bytes[offset + 8] = 54; }
            var bound = CandidateRandomPreparer.Prepare(entry, new CandidateSeedMaterial { SourceCapabilityId = "synthetic-012-test",
                MappingId = CandidateRandomPreparer.SupportedMappingId, Bytes = bytes }, Math());
            Assert.IsTrue(bound.IsAccepted, bound.RejectionCode + " " + bound.FieldPath); return bound.Binding;
        }
        private static CandidateBattleRun CloneRun(CandidateBattleRun run, BattleSnapshot current = null, IEnumerable<CandidateBattleOperationRecord> records = null)
        { return new CandidateBattleRun(run.Binding, run.Baseline, run.InitialSnapshot, current ?? run.CurrentSnapshot, records ?? run.Records, null); }
        private static BattleSnapshot State(BattleSnapshot s, BigInteger? revision = null, BigInteger? actions = null, BigInteger? phases = null,
            IEnumerable<BattleMemberState> members = null, IEnumerable<BattleEnemyState> enemies = null, IEnumerable<BattleContributionTotals> contributions = null,
            BattleBoardState board = null, BattlePhase? phase = null, BattleRandomSnapshot random = null)
        { return new BattleSnapshot(s.Baseline, revision ?? s.SceneRevision, actions ?? s.EffectiveActionsCompleted, phases ?? s.EnemyPhasesCompleted,
            s.CurrentFaceIndex, phase ?? s.Phase, board ?? s.Board, members ?? s.Members, enemies ?? s.Enemies, random ?? s.Random, contributions ?? s.Contributions); }
        private static CandidateContributionSegment Segment(CandidateContributionSegment s, BattleCombatantKey beneficiary = null,
            ExactRational loss = null, string face = null, int? factIndex = null)
        { return new CandidateContributionSegment(s.OperationId, s.SceneRevision, face ?? s.FaceId, s.RuleSegment, s.SegmentIndex, s.Actor, s.Target,
            beneficiary ?? s.Beneficiary, s.Kind, loss ?? s.HpLoss, factIndex ?? s.FactIndex); }
        private static CandidateBattleOperationRecord CloneRecord(CandidateBattleOperationRecord r, BigInteger? time = null, CandidateCombatFrame direct = null)
        { return new CandidateBattleOperationRecord(r.Kind, time ?? r.OccurredAtUnixMilliseconds, r.BeforeSnapshot, r.AfterSnapshot,
            r.Conditions, direct ?? r.DirectAttack, r.EnemyPhase, r.StageDecision, r.OrderedFacts, r.ContributionSegments); }
        private static void AssertFacts(CandidateBattleOperationRecord record)
        {
            var kinds = new List<CandidateBattleFactKind>();
            if (record.DirectAttack != null) kinds.AddRange(record.DirectAttack.DamageFacts.Select(_ => CandidateBattleFactKind.DirectAttack));
            if (record.EnemyPhase != null) kinds.AddRange(record.EnemyPhase.OrderedIntents.Select(_ => CandidateBattleFactKind.EnemyIntent));
            kinds.AddRange(record.StageDecision.OrderedFacts.Select(_ => CandidateBattleFactKind.Stage));
            CollectionAssert.AreEqual(kinds, record.OrderedFacts.Select(x => x.Kind));
            for (var i = 0; i < record.OrderedFacts.Count; i++) Assert.AreEqual(i, record.OrderedFacts[i].Index);
            foreach (var segment in record.ContributionSegments)
            {
                Assert.AreEqual(record.OperationId, segment.OperationId); Assert.AreEqual(record.BeforeSnapshot.SceneRevision, segment.SceneRevision);
                var fact = record.OrderedFacts[segment.FactIndex]; Assert.AreEqual(segment.RuleSegment, fact.Kind);
                Assert.AreEqual(record.BeforeSnapshot.Members[0].CombatantKey, segment.Beneficiary);
                Assert.AreSame(segment.Kind == CandidateContributionKind.DamageDealtHp ? fact.DirectAttack.HpLoss : fact.EnemyIntent.Damage.HpLoss, segment.HpLoss);
            }
        }
        private static BattleEntryInput Input(int count)
        {
            var context = new CandidateContext { DraftId = "synthetic:012", DraftRevision = 1, ContentFingerprint = "test-only",
                RuleVersion = "candidate-r1", NumericContractVersion = "exact-r1", RandomContractVersion = "pcg-r1",
                SourceNotes = new List<string> { "explicit synthetic C=1/1000; conditional public-rule test, not production calibration" } };
            var face = new FaceInput { FaceId = "face:0", Width = 4, Height = count * 2 - 1, Pairs = new List<PairInput>() };
            for (var i = 0; i < count; i++) face.Pairs.Add(new PairInput { PairId = "P" + i, GeometryColorId = i,
                EndpointA = new FlowPos(0, i * 2), EndpointB = new FlowPos(3, i * 2), Enemy = new EnemyInput {
                    EnemyInstanceKey = "E" + i, EnemyDefinitionId = "E01", OriginalSlot = i, StableOrder = i,
                    Behavior = EnemyBehavior.NormalStrike, Stats = Stats(15, 10, 0, 0), IntentCycle = new List<EnemyIntentInput> { Strike(3, 5) } } });
            return new BattleEntryInput { PlayerId = "player:中|😀", ChallengeId = "challenge", AttemptId = "attempt", EntryBaselineId = "baseline",
                Context = context, CarryMode = EntryCarryMode.Empty, RequiredFeatures = new List<string>(),
                Level = new LevelInput { LevelId = "synthetic:level", LevelVersion = "candidate-r1", RecommendedLevel = 1, Faces = new List<FaceInput> { face } },
                Members = new List<MemberInput> { new MemberInput { CharacterId = "W1", ClassId = "warrior", ClassKind = CharacterClassKind.Warrior,
                    OriginalSlot = 2, Level = 1, IsReady = true, StatsOrigin = BaseStatsOrigin.ComputedBaseStats, StatsContext = Context(context),
                    Stats = Stats(100, 20, 10, 6), EntryHp = R(100), LearnedSkills = new List<string>(),
                    Crit = new WarriorCritInput { PassiveDefinitionId = "warrior:crit", TargetProbability = R(1, 5), C = R(1, 1000), Multiplier = R(3, 2) } } } };
        }
        private static BattleEntryInput Source(int level, int orientation, out List<List<FlowPos>> routes)
        {
            var fixture = DemoContentFixture.CaptureSource(level, 1, (SourceCoordinateConvention)orientation);
            var geometry = fixture.CopyLevel(); var input = Input(2); var face = input.Level.Faces[0];
            input.Level.LevelId = fixture.FixtureKey; face.Width = geometry.width; face.Height = geometry.height;
            input.Context.SourceNotes.Add(fixture.SourcePath + ":" + fixture.SourceLocator + ":" + fixture.SourceSha256);
            input.Context.SourceNotes.Add(fixture.CoordinateTransform); input.Members[0].StatsContext = Context(input.Context);
            foreach (var pair in geometry.pairs)
            { var p = face.Pairs[pair.colorId]; p.EndpointA = pair.endpointA; p.EndpointB = pair.endpointB; }
            if (level == 3) { Heavy(face.Pairs[0].Enemy); face.Pairs[0].Enemy.Stats.MaxHp = R(20); face.Pairs[0].Enemy.Stats.PhysicalDefense = R(20); }
            routes = fixture.CopySolution().paths.OrderBy(p => p.colorId).Select(p => new List<FlowPos>(p.cells)).ToList(); return input;
        }
        private static void Heavy(EnemyInput enemy)
        { enemy.EnemyDefinitionId = "E02"; enemy.Behavior = EnemyBehavior.ChargeHeavy; enemy.IntentCycle = new List<EnemyIntentInput> {
            new EnemyIntentInput { Kind = EnemyIntentKind.Charge, Targeting = EnemyTargeting.FirstLiving, DamageKind = null, DamageCoefficient = null }, Strike(13, 10) }; }
        private static EnemyIntentInput Strike(int n, int d)
        { return new EnemyIntentInput { Kind = EnemyIntentKind.Strike, Targeting = EnemyTargeting.FirstLiving, DamageKind = EntryDamageKind.Physical, DamageCoefficient = R(n, d) }; }
        private static CandidateContext Context(RuleContext c)
        { return new CandidateContext { DraftId = c.DraftId, DraftRevision = c.DraftRevision, ContentFingerprint = c.ContentFingerprint,
            RuleVersion = c.RuleVersion, NumericContractVersion = c.NumericContractVersion, RandomContractVersion = c.RandomContractVersion, SourceNotes = new List<string>(c.SourceNotes) }; }
        private static StatsInput Stats(int hp, int attack, int defense, int magic)
        { return new StatsInput { MaxHp = R(hp), Attack = R(attack), PhysicalDefense = R(defense), MagicDefense = R(magic), Evasion = R(0), AttackRange = 1 }; }
        private static string Describe(object value)
        {
            if (value == null) return "null";
            if (value is string s) return s.Length + ":" + s;
            if (value is ExactRational r) return r.Numerator + "/" + r.Denominator;
            if (value is FlowPos pos) return pos.x + "," + pos.y;
            if (value.GetType().IsValueType) return value.ToString();
            if (value is IEnumerable values) return "[" + string.Join(";", values.Cast<object>().Select(Describe)) + "]";
            return "{" + string.Join(";", value.GetType().GetProperties().OrderBy(p => p.Name, StringComparer.Ordinal).Select(p => p.Name + "=" + Describe(p.GetValue(value)))) + "}";
        }
        private static void AssertReadOnly(object value, HashSet<object> seen)
        {
            if (value == null || value is string || value.GetType().IsValueType || !seen.Add(value)) return;
            if (value is IList list) { Assert.IsTrue(list.IsReadOnly); Assert.Throws<NotSupportedException>(() => list.Clear()); }
            if (value is IEnumerable rows) { foreach (var row in rows) AssertReadOnly(row, seen); return; }
            foreach (var p in value.GetType().GetProperties()) { Assert.IsNull(p.GetSetMethod()); AssertReadOnly(p.GetValue(value), seen); }
        }
        // Independent format reader: schema assertions use the frozen declarations, not the production encoder.
        private static object Decode(byte[] bytes)
        { using (var input = new MemoryStream(bytes)) using (var reader = new BinaryReader(input)) {
            CollectionAssert.AreEqual(new byte[] { 70, 77, 66, 82, 48, 49, 10 }, reader.ReadBytes(7)); var value = Read(reader);
            Assert.AreEqual(input.Length, input.Position); return value; } }
        private static object Read(BinaryReader r)
        {
            var tag = r.ReadByte(); if (tag == 0) return null; if (tag == 1) return r.ReadByte() != 0;
            if (tag == 3) return new[] { Read(r), Read(r) };
            var length = checked((int)r.ReadUInt32());
            if (tag == 2) return System.Text.Encoding.ASCII.GetString(r.ReadBytes(length));
            if (tag == 4) { var chars = new char[length]; for (var i = 0; i < length; i++) chars[i] = (char)r.ReadUInt16(); return new string(chars); }
            if (tag == 7) return r.ReadBytes(length);
            if (tag == 5) { var values = new List<object>(); for (var i = 0; i < length; i++) values.Add(Read(r)); return values; }
            Assert.AreEqual(6, tag); var fields = new List<KeyValuePair<string, object>>();
            for (var i = 0; i < length; i++) fields.Add(new KeyValuePair<string, object>((string)Read(r), Read(r))); return fields;
        }
        private static string[] Keys(object record) { return ((List<KeyValuePair<string, object>>)record).Select(x => x.Key).ToArray(); }
        private static object Field(object record, string key) { return ((List<KeyValuePair<string, object>>)record).Single(x => x.Key == key).Value; }
        private static object First(object list) { return ((List<object>)list)[0]; }
        private static string[] DataNames(Type type) { return type.GetProperties(BindingFlags.Public | BindingFlags.Instance).OrderBy(p => p.MetadataToken).Select(p => p.Name).ToArray(); }
    }
}
