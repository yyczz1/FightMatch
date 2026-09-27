using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using FlowPuzzle.Core;
using NUnit.Framework;
using static FightMatch.Core.CandidateHistoryRejectionCode;

namespace FightMatch.Core.Tests
{
    [TestFixture]
    public class CandidateHistoryOperationsTests
    {
        [TestCase(1, 1)] [TestCase(1, 2)] [TestCase(3, 1)] [TestCase(3, 2)]
        public void RealPublicSourceOperationsAppendAndRetainTheTerminalStep(int level, int orientation)
        {
            var input = Source(level, orientation, out var routes); var initial = Start(input); var history = initial;
            var count = level == 1 ? 2 : 3;
            for (var i = 0; i < count; i++)
            {
                var pair = level == 1 ? i : i < 2 ? 0 : 1;
                var before = history; history = Attack(history, "op:" + i, "anchor:" + i, pair, routes[pair]);
                Assert.AreSame(before.CurrentRun.CurrentSnapshot, history.Archive[i].Record.BeforeSnapshot);
                Assert.AreSame(history.CurrentRun.Records[i], history.Archive[i].Record);
                Assert.AreEqual("anchor:" + i, history.EffectiveAnchors[i]);
                Assert.IsFalse(history.CurrentRun.Records[i].DirectAttack.DamageFacts[0].Crit.Triggered);
                Assert.IsEmpty(history.RollbackRecords); Assert.IsNull(history.Archive[i].SupersededBy);
                Assert.AreEqual(i + 2, (int)history.CurrentRun.CurrentSnapshot.SceneRevision);
                var query = Accept(CandidateHistoryOperations.FindOperation(history, "op:" + i, Math()));
                Assert.AreSame(history.Archive[i], query.Operation.Entry); Assert.IsNull(query.Next);
                Assert.AreEqual(CandidateHistoryOperationRelation.Effective, query.Operation.Relation);
            }
            Assert.IsEmpty(initial.Archive); Assert.IsEmpty(initial.CurrentRun.Records);
            var report = history.CurrentRun.FinalReport; Assert.NotNull(report); Assert.AreEqual(count, report.Operations.Count);
            Assert.AreSame(history.Archive.Last().Record, report.Operations.Last());
            Value(history.CurrentRun.CurrentSnapshot.Members[0].Hp, level == 1 ? 95 : 90);
            Assert.AreEqual(BattlePhase.WonPendingSettlement, history.CurrentRun.CurrentSnapshot.Phase);
            Assert.IsFalse(history.CommitEligible); Assert.IsFalse(report.CommitEligible);
            Assert.AreSame(initial.Binding.BaseReward.Initial, history.Binding.BaseReward.Initial);
            Assert.AreSame(initial.Binding.Bonus.Initial, history.Binding.Bonus.Initial);
        }

        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)] [TestCase(5)]
        public void CreationCannotImportMiddleStateOrAnotherOriginalGraph(int mutation)
        {
            var history = Start(Input(2)); var initial = history.CurrentRun;
            var next = Evaluate(initial, "a"); var run = initial;
            if (mutation == 0) run = next;
            if (mutation == 1) run = Clone(initial, current: next.CurrentSnapshot);
            if (mutation == 2) run = Clone(initial, current: State(initial.CurrentSnapshot, revision: 2));
            if (mutation == 3) run = new CandidateBattleRun(initial.Binding, Start(Input(2)).CurrentRun.Baseline,
                initial.InitialSnapshot, initial.CurrentSnapshot, initial.Records, null);
            if (mutation == 4) run = new CandidateBattleRun(initial.Binding, initial.Baseline,
                State(initial.InitialSnapshot), initial.CurrentSnapshot, initial.Records, null);
            if (mutation == 5) run = new CandidateBattleRun(null, initial.Baseline, initial.InitialSnapshot, initial.CurrentSnapshot, initial.Records, null);
            Reject(CandidateHistoryOperations.CreateCandidate(run, Math()), mutation == 5 ? MissingField : mutation >= 3 ? InconsistentBinding : IncompleteHistory);
            Assert.IsEmpty(initial.Records); Assert.AreEqual(BigInteger.One, initial.CurrentSnapshot.SceneRevision);
        }

        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)] [TestCase(5)] [TestCase(6)] [TestCase(7)]
        public void AppendRejectsMissingSkippedOrChangedPrefixesAndForeignBindings(int mutation)
        {
            var initial = Start(Input(3)); var history = Attack(initial, "a", "A");
            var next = Evaluate(history.CurrentRun, "b", 1); var first = history.CurrentRun.Records[0];
            if (mutation == 0) next = Evaluate(next, "c", 2);
            if (mutation == 1) next = Clone(next, records: new[] { next.Records.Last() });
            if (mutation == 2) next = Clone(next, records: Array.Empty<CandidateBattleOperationRecord>());
            if (mutation == 3) next = Clone(next, records: new[] { Record(first, conditions: new CandidateBattleConditionValues(99, false)), next.Records.Last() });
            if (mutation == 4) next = Clone(next, records: new[] { Record(first, time: 77), next.Records.Last() });
            if (mutation == 5) next = new CandidateBattleRun(Start(Input(3)).Binding, next.Baseline, next.InitialSnapshot, next.CurrentSnapshot, next.Records, null);
            if (mutation == 6) next = Clone(next, current: State(next.CurrentSnapshot, revision: 8));
            if (mutation == 7) next = Clone(next, records: new[] { Record(first), next.Records.Last() });
            Reject(CandidateHistoryOperations.Append(history, next, "B", Math()), mutation == 5 ? InconsistentBinding : IncompleteHistory);
            Assert.AreEqual(1, history.Archive.Count); Assert.AreSame(first, history.CurrentRun.Records[0]);
        }

        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)] [TestCase(5)] [TestCase(6)] [TestCase(7)]
        public void AppendRequiresTheCompleteOriginalFragmentsAndAfterState(int mutation)
        {
            var history = Start(Input(3)); var next = Evaluate(history.CurrentRun, "a"); var original = next.Records[0];
            var before = original.BeforeSnapshot; var after = original.AfterSnapshot; var direct = original.DirectAttack;
            var enemy = original.EnemyPhase; var stage = original.StageDecision; var facts = original.OrderedFacts;
            var segments = original.ContributionSegments;
            if (mutation == 0) before = State(before);
            if (mutation == 1) direct = null;
            if (mutation == 2) enemy = null;
            if (mutation == 3) stage = null;
            if (mutation == 4) after = State(after, members: new[] { new BattleMemberState(after.Members[0].CombatantKey, after.Members[0].Member, R(99)) });
            if (mutation == 5) facts = original.OrderedFacts.Skip(1).ToArray();
            if (mutation == 6) segments = Array.Empty<CandidateContributionSegment>();
            if (mutation == 7)
            {
                var foreign = Start(Input(3)).CurrentRun;
                direct = new CandidateCombatFrame(foreign.Binding, before, direct.Action, direct.Enemies, direct.Random, direct.Contributions, direct.DamageFacts[0]);
            }
            var changed = new CandidateBattleOperationRecord(original.Kind, 0, before, after, original.Conditions, direct, enemy, stage, facts, segments);
            var forged = Clone(next, current: after, records: new[] { changed });
            var result = CandidateHistoryOperations.Append(history, forged, "A", Math());
            Reject(result, mutation == 7 ? InconsistentBinding : IncompleteHistory);
            Assert.IsEmpty(history.Archive);
        }

        [TestCase(1)] [TestCase(2)]
        public void L3EndpointChoosesTheLatestActualAttackAndLockedRouteChoosesTheKillingAttack(int orientation)
        {
            var history = Start(Source(3, orientation, out var routes));
            Reject(Locate(history, 0, false), NotFound); Reject(Locate(history, 0, true), NotFound);
            history = Attack(history, "first-hit", "A", 0, routes[0]);
            var first = Accept(Locate(history, 0, false)).Range;
            Assert.AreEqual("A", first.HistoryAnchorId); Assert.AreEqual("first-hit", first.OperationId);
            Assert.AreEqual(1, first.Entries.Count); Assert.AreSame(history.CurrentRun.InitialSnapshot, first.BeforeSnapshot);
            Assert.IsFalse(history.Archive[0].Record.DirectAttack.DamageFacts[0].DefeatedTarget);
            Reject(Locate(history, 0, true), NotFound); Reject(Locate(history, 1, false), NotFound);
            history = Attack(history, "kill", "B", 0, routes[0]);
            foreach (var locked in new[] { false, true })
            {
                var range = Accept(Locate(history, 0, locked)).Range;
                Assert.AreEqual("B", range.HistoryAnchorId); Assert.AreEqual("kill", range.OperationId);
                Assert.AreEqual(1, range.Entries.Count); Assert.AreEqual("face:0", range.Entries[0].FaceId);
                Assert.AreEqual(Pair(history, 0), range.Entries[0].Pair);
            }
            var full = Range(history, "A"); CollectionAssert.AreEqual(new[] { "A", "B" }, full.Entries.Select(e => e.HistoryAnchorId));
            CollectionAssert.AreEqual(new[] { "first-hit", "kill" }, full.Entries.Select(e => e.OperationId));
            Assert.AreSame(history.Binding, full.Binding); Assert.AreEqual(new BigInteger(3), full.SceneRevision);
            Assert.AreEqual(2, history.Archive.Count); Assert.IsEmpty(history.RollbackRecords);
        }

        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)] [TestCase(5)]
        public void LocatorUsesExplicitKindAndCurrentStructuralPairIdentity(int mutation)
        {
            var history = Attack(Start(Input(3)), "a", "A"); var locator = Locator(history, 0, false);
            if (mutation == 0) locator.Kind = null;
            if (mutation == 1) locator.Kind = (CandidateHistoryLocatorKind)99;
            if (mutation == 2) locator.Pair = null;
            if (mutation == 3) locator.Pair = BattlePairKey.Create("another", "face:0", "P0");
            if (mutation == 4) locator.Pair = BattlePairKey.Create("attempt", "another-face", "P0");
            if (mutation == 5) locator.Pair = BattlePairKey.Create("attempt", "face:0", "p0");
            Reject(CandidateHistoryOperations.Locate(history, locator, Math()), mutation == 0 || mutation == 2 ? MissingField : mutation == 1 ? UnsupportedBinding : InconsistentBinding);
        }

        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)] [TestCase(5)] [TestCase(6)] [TestCase(7)]
        public void ConfirmationMustMatchTheOriginalRevisionTargetAndEverySuccessor(int mutation)
        {
            var h = Start(Source(3, 1, out var routes)); h = Attack(h, "a", "A", 0, routes[0]); h = Attack(h, "b", "B", 0, routes[0]);
            var range = Range(h, "A"); var request = RollbackRequest(h, "rollback", "A"); var confirmed = range;
            if (mutation == 0) request.ExpectedSceneRevision--;
            if (mutation == 1) confirmed = CopyRange(range, revision: range.SceneRevision - 1);
            if (mutation == 2) request.HistoryAnchorId = "B";
            if (mutation == 3) confirmed = CopyRange(range, entries: range.Entries.Take(1));
            if (mutation == 4) confirmed = CopyRange(range, entries: range.Entries.Reverse());
            if (mutation == 5) confirmed = CopyRange(range, binding: Start(Source(3, 1, out _)).Binding);
            if (mutation == 6) confirmed = CopyRange(range, before: State(range.BeforeSnapshot));
            if (mutation == 7) confirmed = CopyRange(range, operation: "b");
            Reject(CandidateHistoryOperations.PrepareRollback(h, request, confirmed, Math()), mutation <= 1 ? StaleContext : InconsistentBinding);
            Assert.AreEqual(new BigInteger(3), h.CurrentRun.CurrentSnapshot.SceneRevision); Assert.IsEmpty(h.RollbackRecords);
            Assert.IsTrue(h.Archive.All(e => e.SupersededBy == null)); Assert.AreEqual(2, h.EffectiveAnchors.Count);
        }

        [TestCase(1)] [TestCase(2)]
        public void RollingBackBothL3StepsRestoresEveryBusinessValueAndTheOriginalRandomState(int orientation)
        {
            var initial = Start(Source(3, orientation, out var routes));
            var h = Attack(initial, "a", "A", 0, routes[0]); h = Attack(h, "b", "B", 0, routes[0]);
            var changed = h.CurrentRun.CurrentSnapshot;
            Value(changed.Members[0].Hp, 90); Assert.AreEqual(2, (int)changed.Random.PrdStates[0].FailureCount);
            Assert.AreEqual(2, (int)changed.Random.Stream.WordsConsumed); Assert.AreEqual(1, changed.Board.LockedRoutes.Count);
            var result = Rollback(h, "rb", "A"); var restored = result.Next.CurrentRun.CurrentSnapshot;
            AssertBusinessEqual(initial.CurrentRun.CurrentSnapshot, restored); Assert.AreEqual(new BigInteger(4), restored.SceneRevision);
            Assert.AreSame(initial.CurrentRun.CurrentSnapshot.Random, restored.Random); Assert.AreSame(initial.Binding, result.Next.Binding);
            Assert.AreSame(initial.CurrentRun.InitialSnapshot, result.Next.CurrentRun.InitialSnapshot);
            Assert.IsEmpty(result.Next.CurrentRun.Records); Assert.IsEmpty(result.Next.EffectiveAnchors);
            Assert.AreEqual(2, result.Next.Archive.Count); Assert.AreEqual(1, result.Next.RollbackRecords.Count);
            foreach (var entry in result.Next.Archive) { Assert.AreEqual("rb", entry.SupersededBy.OperationId); Assert.AreEqual(new BigInteger(4), entry.SupersededBy.SceneRevision); }
            Assert.AreSame(h.CurrentRun, result.RollbackRecord.BeforeRun); Assert.AreSame(result.Next.CurrentRun, result.RollbackRecord.RestoredRun);
            Assert.IsTrue(h.Archive.All(e => e.SupersededBy == null)); Assert.AreSame(changed, h.CurrentRun.CurrentSnapshot);
            var replay1 = Attack(result.Next, "new-a", "new-A", 0, routes[0]);
            var replay2 = Attack(replay1, "new-b", "new-B", 0, routes[0]);
            AssertBusinessEqual(changed, replay2.CurrentRun.CurrentSnapshot);
            Assert.AreEqual(new BigInteger(6), replay2.CurrentRun.CurrentSnapshot.SceneRevision);
            Assert.AreEqual("new-a", replay2.CurrentRun.Records[0].OperationId);
            Assert.AreEqual(new BigInteger(4), replay2.CurrentRun.Records[0].BeforeSnapshot.SceneRevision);
            Assert.AreEqual(new BigInteger(1), h.CurrentRun.Records[0].BeforeSnapshot.SceneRevision);
            CollectionAssert.AreEqual(h.Archive[0].Record.DirectAttack.DamageFacts[0].Crit.Words,
                replay1.CurrentRun.Records[0].DirectAttack.DamageFacts[0].Crit.Words);
        }

        [TestCase(0)] [TestCase(1)]
        public void CrossFaceRangesRestoreTheOldFaceAndPreserveRevisionGaps(int target)
        {
            var input = Input(2); var face2 = Input(2).Level.Faces[0]; face2.FaceId = "face:1"; input.Level.Faces.Add(face2);
            var initial = Start(input); var a = Attack(initial, "a", "A"); var b = Attack(a, "b", "B", 1);
            Assert.AreEqual(1, b.CurrentRun.CurrentSnapshot.CurrentFaceIndex);
            Reject(Locate(b, 0, false), NotFound);
            var locator = Locator(b, 0, false); locator.Pair = Pair(a, 0);
            Reject(CandidateHistoryOperations.Locate(b, locator, Math()), InconsistentBinding);
            var c = Attack(b, "c", "C");
            Assert.AreEqual("C", Accept(Locate(c, 0, false)).Range.HistoryAnchorId);
            Assert.AreEqual("C", Accept(Locate(c, 0, true)).Range.HistoryAnchorId);
            var range = Range(c, target == 0 ? "A" : "B"); Assert.AreEqual(3 - target, range.Entries.Count);
            Assert.AreEqual("face:0", range.Entries[0].FaceId); Assert.AreEqual("face:1", range.Entries.Last().FaceId);
            var rolled = Rollback(c, "rb", target == 0 ? "A" : "B").Next;
            var expected = target == 0 ? initial.CurrentRun.CurrentSnapshot : a.CurrentRun.CurrentSnapshot;
            AssertBusinessEqual(expected, rolled.CurrentRun.CurrentSnapshot); Assert.AreEqual(new BigInteger(5), rolled.CurrentRun.CurrentSnapshot.SceneRevision);
            Assert.AreSame(initial.Binding.Start.Baseline, rolled.CurrentRun.Baseline);
            Assert.AreSame(initial.CurrentRun.InitialSnapshot.Board.Face, rolled.CurrentRun.CurrentSnapshot.Board.Face);
            Assert.AreEqual(target, rolled.CurrentRun.Records.Count);
            var resumed = Attack(rolled, "resume", "R", target);
            Assert.AreEqual(new BigInteger(5), resumed.CurrentRun.Records.Last().BeforeSnapshot.SceneRevision);
            if (target == 1) { Assert.AreSame(a.CurrentRun.Records[0], resumed.CurrentRun.Records[0]); Assert.AreEqual(new BigInteger(2), resumed.CurrentRun.Records[0].AfterSnapshot.SceneRevision); }
        }

        [TestCase(false)] [TestCase(true)]
        public void RescueCanUndoTheLethalStepWithoutAdvancingUnexecutedIntentCursors(bool heavy)
        {
            var input = Input(3); input.Members[0].EntryHp = R(1, 3);
            foreach (var pair in input.Level.Faces[0].Pairs) pair.Enemy.Stats.MaxHp = R(100);
            if (heavy) Heavy(input.Level.Faces[0].Pairs[1].Enemy);
            var initial = Start(input); var h = Attack(initial, "lethal", "A"); var state = h.CurrentRun.CurrentSnapshot;
            Assert.AreEqual(BattlePhase.AwaitRescue, state.Phase); Assert.IsNull(h.CurrentRun.FinalReport);
            Assert.AreEqual(1, h.CurrentRun.Records[0].EnemyPhase.OrderedIntents.Count);
            Assert.AreEqual(BigInteger.One, state.Enemies[0].IntentCursor);
            Assert.AreEqual(BigInteger.Zero, state.Enemies[1].IntentCursor); Assert.AreEqual(BigInteger.Zero, state.Enemies[2].IntentCursor);
            Assert.AreEqual("A", Accept(Locate(h, 0, false)).Range.HistoryAnchorId);
            var restored = Rollback(h, "rescue-rollback", "A").Next;
            AssertBusinessEqual(initial.CurrentRun.CurrentSnapshot, restored.CurrentRun.CurrentSnapshot);
            Assert.AreEqual(new BigInteger(3), restored.CurrentRun.CurrentSnapshot.SceneRevision); Value(restored.CurrentRun.CurrentSnapshot.Members[0].Hp, 1, 3);
            var repeated = Attack(restored, "new-lethal", "B"); AssertBusinessEqual(state, repeated.CurrentRun.CurrentSnapshot);
        }

        [Test]
        public void BranchesKeepEachFirstSupersedingRollbackAndReportsContainOnlyTheNewEffectivePath()
        {
            var h = Start(Source(3, 1, out var routes)); h = Attack(h, "a", "A", 0, routes[0]); h = Attack(h, "b", "B", 0, routes[0]);
            var one = Rollback(h, "rb-one", "B").Next;
            Assert.AreEqual(CandidateHistoryOperationRelation.Superseded, Find(one, "b").Relation);
            Assert.AreEqual("rb-one", Find(one, "b").Entry.SupersededBy.OperationId);
            var next = Attack(one, "c", "C", 0, routes[0]); var two = Rollback(next, "rb-two", "A").Next;
            Assert.AreEqual("rb-one", Find(two, "b").Entry.SupersededBy.OperationId);
            Assert.AreEqual(new BigInteger(4), Find(two, "b").Entry.SupersededBy.SceneRevision);
            foreach (var id in new[] { "a", "c" }) { Assert.AreEqual("rb-two", Find(two, id).Entry.SupersededBy.OperationId); Assert.AreEqual(new BigInteger(6), Find(two, id).Entry.SupersededBy.SceneRevision); }
            foreach (var id in new[] { "rb-one", "rb-two" }) Assert.AreEqual(CandidateHistoryOperationRelation.RollbackRecorded, Find(two, id).Relation);
            Assert.IsEmpty(two.CurrentRun.Records); Assert.AreEqual(3, two.Archive.Count); Assert.AreEqual(2, two.RollbackRecords.Count);
            var d = Attack(two, "d", "D", 0, routes[0]); d = Attack(d, "e", "E", 0, routes[0]); d = Attack(d, "f", "F", 1, routes[1]);
            var report = d.CurrentRun.FinalReport; Assert.NotNull(report);
            CollectionAssert.AreEqual(new[] { "d", "e", "f" }, report.Operations.Select(r => r.OperationId));
            Assert.IsTrue(report.Contributions.All(c => c.OperationId == "d" || c.OperationId == "e" || c.OperationId == "f"));
            Value(report.FinalSnapshot.Contributions[0].EffectiveDamageDealtHp, 35); Value(report.FinalSnapshot.Contributions[0].EffectiveDamageTakenHp, 10);
            Assert.AreEqual(6, d.Archive.Count); Assert.AreEqual(2, d.RollbackRecords.Count);
            foreach (var id in new[] { "a", "b", "c", "d", "e", "f", "rb-one", "rb-two" }) Assert.NotNull(Find(d, id));
            Assert.AreEqual("rb-one", Find(d, "b").Entry.SupersededBy.OperationId);
        }

        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)]
        public void AcceptedAndSupersededAnchorsAndOperationIdsAreNeverReusable(int reuse)
        {
            var h = Attack(Start(Input(3)), "old", "OLD");
            if (reuse == 0)
            { Reject(CandidateHistoryOperations.Append(h, Evaluate(h.CurrentRun, "fresh", 1), "OLD", Math()), OperationConflict); return; }
            if (reuse == 1)
            { Reject(CandidateHistoryOperations.PrepareRollback(h, RollbackRequest(h, "old", "OLD"), Range(h, "OLD"), Math()), OperationConflict); return; }
            var restored = Rollback(h, "rollback", "OLD").Next;
            var operation = reuse == 2 ? "old" : reuse == 3 ? "rollback" : "fresh";
            var next = Evaluate(restored.CurrentRun, operation);
            Reject(CandidateHistoryOperations.Append(restored, next, reuse == 4 ? "OLD" : "FRESH", Math()), OperationConflict);
            Assert.IsEmpty(restored.CurrentRun.Records); Assert.AreEqual(1, restored.Archive.Count);
        }

        [Test]
        public void RollbackIdsRemainOccupiedAfterMoreActionsAndQueriesDoNotActAsRetryCaches()
        {
            var h = Attack(Start(Input(3)), "a", "A"); var preview = Range(h, "A"); var request = RollbackRequest(h, "rollback", "A");
            var restored = Accept(CandidateHistoryOperations.PrepareRollback(h, request, preview, Math())).Next;
            Reject(CandidateHistoryOperations.PrepareRollback(restored, request, preview, Math()), StaleContext);
            Reject(CandidateHistoryOperations.ReadRange(restored, RangeRequest(restored, "A"), Math()), NotFound);
            var h2 = Attack(restored, "b", "B");
            Reject(CandidateHistoryOperations.PrepareRollback(h2, RollbackRequest(h2, "rollback", "B"), Range(h2, "B"), Math()), OperationConflict);
            Reject(CandidateHistoryOperations.FindOperation(h2, "unknown", Math()), NotFound);
            Assert.AreEqual(CandidateHistoryOperationRelation.RollbackRecorded, Find(h2, "rollback").Relation);
        }

        [TestCase("en-US")] [TestCase("tr-TR")] [TestCase("zh-CN")]
        public void IdentitiesRemainOrdinalAndDoNotMergeSeparatorOrCaseVariants(string culture)
        {
            var old = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo(culture);
                var h = Attack(Start(Input(3)), "I:中|😀", "A|中😀"); h = Rollback(h, "rb:I", "A|中😀").Next;
                h = Attack(h, "i:中|😀", "a|中😀");
                Assert.AreEqual(CandidateHistoryOperationRelation.Superseded, Find(h, "I:中|😀").Relation);
                Assert.AreEqual(CandidateHistoryOperationRelation.Effective, Find(h, "i:中|😀").Relation);
                Reject(CandidateHistoryOperations.FindOperation(h, "ı:中|😀", Math()), NotFound);
            }
            finally { CultureInfo.CurrentCulture = old; }
        }

        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)]
        public void VictoryCannotBeUndoneOrReceiveMoreHistory(int method)
        {
            var h = Attack(Start(Input(2)), "a", "A"); var preview = Range(h, "A"); h = Attack(h, "b", "B", 1);
            CandidateHistoryResult result;
            if (method == 0) result = CandidateHistoryOperations.ReadRange(h, RangeRequest(h, "A"), Math());
            else if (method == 1) result = Locate(h, 0, false);
            else if (method == 2) result = CandidateHistoryOperations.PrepareRollback(h, RollbackRequest(h, "rb", "A"), preview, Math());
            else result = CandidateHistoryOperations.Append(h, h.CurrentRun, "C", Math());
            Reject(result, InvalidPhase); Assert.AreEqual(2, h.Archive.Count); Assert.IsEmpty(h.RollbackRecords);
            Assert.AreEqual(CandidateHistoryOperationRelation.Effective, Find(h, "b").Relation);
        }

        [Test]
        public void CurrentExplicitPreferenceCanChangeWithoutRewritingOldConditionsOrPermanentInput()
        {
            var input = Input(3); var h = Start(input); var request = Request(h.CurrentRun.CurrentSnapshot, "a");
            var conditions = new CandidateBattleConditions { PreferenceRevision = 7, ItemUseEnabled = false };
            var battle = CandidateBattleOperations.EvaluateAttack(h.CurrentRun, request, conditions, 0, Sampling()); Assert.IsTrue(battle.IsAccepted);
            var old = Accept(CandidateHistoryOperations.Append(h, battle.NextRun, "A", Math())).Next;
            var query = RangeRequest(old, "A"); var preview = Accept(CandidateHistoryOperations.ReadRange(old, query, Math())).Range;
            var rollbackRequest = RollbackRequest(old, "rb", "A");
            var restored = Accept(CandidateHistoryOperations.PrepareRollback(old, rollbackRequest, preview, Math())).Next;
            request.Route.Clear(); request.OperationId = "changed"; conditions.PreferenceRevision = 88; conditions.ItemUseEnabled = true;
            query.HistoryAnchorId = "changed"; rollbackRequest.OperationId = "changed";
            input.Members[0].Stats.Attack = R(900); input.Members[0].Level = 99;
            var current = Attack(restored, "b", "B", conditions: conditions);
            Assert.AreEqual(new BigInteger(7), old.CurrentRun.Records[0].Conditions.PreferenceRevision); Assert.IsFalse(old.CurrentRun.Records[0].Conditions.ItemUseEnabled);
            Assert.AreEqual(new BigInteger(88), current.CurrentRun.Records[0].Conditions.PreferenceRevision); Assert.IsTrue(current.CurrentRun.Records[0].Conditions.ItemUseEnabled);
            Assert.AreEqual("a", preview.OperationId); Assert.AreEqual("rb", restored.RollbackRecords[0].OperationId);
            Assert.AreSame(old.Archive[0].Record, current.Archive[0].Record); Value(current.CurrentRun.Baseline.Entry.Members[0].Stats.Attack, 20);
            Assert.AreEqual(BigInteger.One, current.CurrentRun.Baseline.Entry.Members[0].Level);
            ReadOnly(current, new HashSet<object>()); ReadOnly(preview, new HashSet<object>()); ReadOnly(Find(current, "a"), new HashSet<object>());
            Assert.IsFalse(current.CommitEligible); Assert.IsFalse(preview.CommitEligible); Assert.IsFalse(restored.RollbackRecords[0].CommitEligible);
        }

        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)] [TestCase(5)] [TestCase(6)]
        public void ContextFieldsAreExplicitAndZeroIsNotTreatedAsMissing(int mutation)
        {
            var h = Attack(Start(Input(3)), "a", "A"); var request = RangeRequest(h, "A");
            if (mutation == 0) request.PlayerId = null;
            if (mutation == 1) request.AttemptId = " ";
            if (mutation == 2) request.ExpectedSceneRevision = null;
            if (mutation == 3) request.ExpectedSceneRevision = 0;
            if (mutation == 4) request.PlayerId = "foreign";
            if (mutation == 5) request.AttemptId = "foreign";
            if (mutation == 6) request.HistoryAnchorId = " ";
            Reject(CandidateHistoryOperations.ReadRange(h, request, Math()), mutation == 3 ? InvalidValue : mutation == 4 || mutation == 5 ? InconsistentBinding : MissingField);
            Assert.AreEqual(BigInteger.Zero, h.CurrentRun.Records[0].OccurredAtUnixMilliseconds);
        }

        [TestCase(0)] [TestCase(1)] [TestCase(2)]
        public void UnknownHistoryRecordCapabilitiesProduceStructuredRejections(int mutation)
        {
            var h = Start(Input(3)); var next = Evaluate(h.CurrentRun, "a"); var original = next.Records[0];
            var kind = mutation == 0 ? (CandidateBattleOperationKind)99 : original.Kind;
            var coverage = mutation == 1 ? (CandidateConsumptionCoverage)99 : original.ConsumptionCoverage;
            var stage = original.StageDecision;
            if (mutation == 2) stage = new CandidateStageDecision(stage.BeforeSnapshot, stage.Source, stage.FinalHp, stage.Board,
                stage.NextFaceIndex, (BattlePhase)99, stage.NextFace, stage.OrderedFacts);
            var record = new CandidateBattleOperationRecord(kind, 0, original.BeforeSnapshot, original.AfterSnapshot, original.Conditions,
                original.DirectAttack, original.EnemyPhase, stage, original.OrderedFacts, original.ContributionSegments, coverage);
            Reject(CandidateHistoryOperations.Append(h, Clone(next, records: new[] { record }), "A", Math()), UnsupportedBinding);
        }

        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)]
        public void IncompleteOrMisattributedAuditArchivesCannotBeReadAsValidHistory(int mutation)
        {
            var h = Attack(Start(Input(3)), "a", "A"); h = Rollback(h, "rb", "A").Next;
            var archive = h.Archive; var rollbacks = h.RollbackRecords; var anchors = h.EffectiveAnchors;
            if (mutation == 0) archive = Array.Empty<CandidateHistoryEntry>();
            if (mutation == 1) rollbacks = Array.Empty<CandidateRollbackRecord>();
            if (mutation == 2) archive = new[] { new CandidateHistoryEntry("A", archive[0].Record, new CandidateHistorySupersededBy("wrong", 3)) };
            if (mutation == 3) anchors = new[] { "A" };
            var bad = new CandidateBattleHistory(h.CurrentRun, archive, anchors, rollbacks);
            Reject(CandidateHistoryOperations.FindOperation(bad, "a", Math()), IncompleteHistory);
        }

        [TestCase(0)] [TestCase(1)]
        public void SupersededLargeOriginalNumbersAreRecheckedWithTheCallersNewBudget(int field)
        {
            var h = Start(Input(3)); var huge = BigInteger.One << 100;
            var conditions = new CandidateBattleConditions { PreferenceRevision = field == 0 ? huge : 7, ItemUseEnabled = true };
            var run = Evaluate(h.CurrentRun, "a", conditions: conditions, time: field == 1 ? huge : BigInteger.Zero);
            h = Accept(CandidateHistoryOperations.Append(h, run, "A", Math())).Next; h = Rollback(h, "rb", "A").Next;
            h = Attack(h, "b", "B"); Assert.AreEqual(new BigInteger(7), h.CurrentRun.Records[0].Conditions.PreferenceRevision);
            var saved = h; CandidateHistoryResult output = null;
            var limit = Assert.Throws<ExactMathLimitException>(() => output = CandidateHistoryOperations.FindOperation(saved, "b", new ExactMathBudget(64)));
            Assert.AreEqual("IntegerBits", limit.ReasonCode); Assert.IsNull(output);
            Assert.AreSame(run.Records[0], h.Archive[0].Record); Assert.AreEqual("rb", h.Archive[0].SupersededBy.OperationId);
            Assert.AreEqual(CandidateHistoryOperationRelation.Effective, Find(h, "b").Relation);
        }

        [Test]
        public void AHighCurrentRevisionIsNumericallyCheckedBeforeAnyHistoryResultEscapes()
        {
            var h = Start(Input(3)); var large = State(h.CurrentRun.CurrentSnapshot, revision: BigInteger.One << 100);
            // Deliberately invalid internal audit probe, not a public import or a claim of 2^100 executed operations.
            var probe = new CandidateBattleHistory(Clone(h.CurrentRun, current: large), h.Archive, h.EffectiveAnchors, h.RollbackRecords);
            CandidateHistoryResult output = null;
            Assert.Throws<ExactMathLimitException>(() => output = CandidateHistoryOperations.FindOperation(probe, "a", new ExactMathBudget(64)));
            Assert.IsNull(output); Reject(CandidateHistoryOperations.FindOperation(probe, "a", Math()), IncompleteHistory);
        }

        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)] [TestCase(5)]
        public void SharedBudgetLimitsNeverPublishPartialCandidatesAndFreshRetriesAgree(int method)
        {
            var initial = Start(Input(3)); var h = Attack(initial, "a", "A"); var next = Evaluate(h.CurrentRun, "b", 1);
            var preview = Range(h, "A");
            Func<ExactMathBudget, CandidateHistoryResult> call = math => method == 0 ? CandidateHistoryOperations.CreateCandidate(initial.CurrentRun, math)
                : method == 1 ? CandidateHistoryOperations.Append(h, next, "B", math)
                : method == 2 ? CandidateHistoryOperations.Locate(h, Locator(h, 0, false), math)
                : method == 3 ? CandidateHistoryOperations.ReadRange(h, RangeRequest(h, "A"), math)
                : method == 4 ? CandidateHistoryOperations.PrepareRollback(h, RollbackRequest(h, "rb", "A"), preview, math)
                : CandidateHistoryOperations.FindOperation(h, "a", math);
            var measured = Math(); var success = Accept(call(measured)); CandidateHistoryResult failed = null;
            Assert.Throws<ExactMathLimitException>(() => failed = call(new ExactMathBudget(32768, (int)measured.PrimitiveStepsUsed - 1)));
            Assert.IsNull(failed); Assert.IsNull(h.Archive[0].SupersededBy); Assert.IsEmpty(h.RollbackRecords);
            var retry = Accept(call(Math())); Assert.AreEqual(success.Outcome, retry.Outcome);
            if (success.Next != null) AssertBusinessEqual(success.Next.CurrentRun.CurrentSnapshot, retry.Next.CurrentRun.CurrentSnapshot);
            Assert.AreEqual(BigInteger.One, h.CurrentRun.CurrentSnapshot.Random.Stream.WordsConsumed);
        }

        [Test]
        public void PreviewCannotSilentlyGrowAfterAnotherRealOperation()
        {
            var h = Start(Source(3, 1, out var routes)); h = Attack(h, "a", "A", 0, routes[0]);
            var preview = Range(h, "A"); var staleRequest = RollbackRequest(h, "rb", "A");
            var next = Attack(h, "b", "B", 0, routes[0]);
            Reject(CandidateHistoryOperations.PrepareRollback(next, staleRequest, preview, Math()), StaleContext);
            Reject(CandidateHistoryOperations.PrepareRollback(next, RollbackRequest(next, "rb", "A"), preview, Math()), StaleContext);
            Assert.AreEqual(1, preview.Entries.Count); Assert.AreEqual(2, Range(next, "A").Entries.Count);
            Assert.IsTrue(next.Archive.All(e => e.SupersededBy == null));
        }

        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)]
        public void MissingOperationAndAnchorInputsDoNotAllocateHistory(int method)
        {
            var h = Attack(Start(Input(3)), "a", "A"); CandidateHistoryResult result;
            if (method == 0) result = CandidateHistoryOperations.Append(h, Evaluate(h.CurrentRun, "b", 1), null, Math());
            else if (method == 1) result = CandidateHistoryOperations.FindOperation(h, null, Math());
            else if (method == 2) result = CandidateHistoryOperations.PrepareRollback(h, RollbackRequest(h, null, "A"), Range(h, "A"), Math());
            else result = CandidateHistoryOperations.PrepareRollback(h, RollbackRequest(h, "rb", null), Range(h, "A"), Math());
            Reject(result, MissingField); Assert.IsEmpty(h.RollbackRecords); Assert.AreEqual(1, h.Archive.Count);
        }

        [Test]
        public void SyntheticLinkWithoutItsOriginalDeathHistoryCannotEnterThePublicHistory()
        {
            var h = Start(Input(2)); var initial = h.CurrentRun.CurrentSnapshot;
            var dead = initial.Enemies.Select(e => new BattleEnemyState(e.CombatantKey, e.PairKey, e.Enemy, R(0), 0)).ToArray();
            var board = new BattleBoardState(initial.Board.Face, Array.Empty<BattleLockedRoute>(), dead.Select(e => e.PairKey));
            var before = new BattleSnapshot(initial.Baseline, 1, 0, 0, 0, BattlePhase.AwaitLinks, board, initial.Members, dead, initial.Random, initial.Contributions);
            var request = Request(before, "link");
            var stage = CandidateBattleStage.CompleteLink(before, new CandidateLinkRequest { PlayerId = request.PlayerId,
                AttemptId = request.AttemptId, OperationId = request.OperationId, ExpectedSceneRevision = 1, Pair = request.Pair, Route = request.Route }, Math());
            Assert.IsTrue(stage.IsAccepted);
            var record = CandidateBattleOperations.AssembleRecord(CandidateBattleOperationKind.Link, 0, before, null, null, null, stage.Decision, Math());
            Assert.IsNull(record.Conditions); Assert.IsNull(record.DirectAttack); Assert.IsNull(record.EnemyPhase); Assert.IsEmpty(record.ContributionSegments);
            var fake = Clone(h.CurrentRun, current: record.AfterSnapshot, records: new[] { record });
            Reject(CandidateHistoryOperations.Append(h, fake, "LINK", Math()), IncompleteHistory);
            Reject(CandidateHistoryOperations.CreateCandidate(Clone(h.CurrentRun, current: before), Math()), IncompleteHistory);
            Reject(Locate(h, 0, false), NotFound); Assert.IsEmpty(h.Archive);
        }

        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)] [TestCase(5)] [TestCase(6)] [TestCase(7)]
        [TestCase(8)] [TestCase(9)] [TestCase(10)] [TestCase(11)] [TestCase(12)] [TestCase(13)] [TestCase(14)] [TestCase(15)]
        public void NullRootsThrowWithoutChangingTheOriginalAggregate(int method)
        {
            var h = Attack(Start(Input(3)), "a", "A"); var range = Range(h, "A");
            Assert.Throws<ArgumentNullException>(() =>
            {
                switch (method)
                {
                    case 0: CandidateHistoryOperations.CreateCandidate(null, Math()); break;
                    case 1: CandidateHistoryOperations.CreateCandidate(h.CurrentRun, null); break;
                    case 2: CandidateHistoryOperations.Append(null, h.CurrentRun, "B", Math()); break;
                    case 3: CandidateHistoryOperations.Append(h, null, "B", Math()); break;
                    case 4: CandidateHistoryOperations.Append(h, h.CurrentRun, "B", null); break;
                    case 5: CandidateHistoryOperations.Locate(null, Locator(h, 0, false), Math()); break;
                    case 6: CandidateHistoryOperations.Locate(h, null, Math()); break;
                    case 7: CandidateHistoryOperations.Locate(h, Locator(h, 0, false), null); break;
                    case 8: CandidateHistoryOperations.ReadRange(null, RangeRequest(h, "A"), Math()); break;
                    case 9: CandidateHistoryOperations.ReadRange(h, null, Math()); break;
                    case 10: CandidateHistoryOperations.ReadRange(h, RangeRequest(h, "A"), null); break;
                    case 11: CandidateHistoryOperations.PrepareRollback(null, RollbackRequest(h, "rb", "A"), range, Math()); break;
                    case 12: CandidateHistoryOperations.PrepareRollback(h, null, range, Math()); break;
                    case 13: CandidateHistoryOperations.PrepareRollback(h, RollbackRequest(h, "rb", "A"), null, Math()); break;
                    case 14: CandidateHistoryOperations.PrepareRollback(h, RollbackRequest(h, "rb", "A"), range, null); break;
                    case 15: CandidateHistoryOperations.FindOperation(null, "a", Math()); break;
                }
            });
            Assert.Throws<ArgumentNullException>(() => CandidateHistoryOperations.FindOperation(h, "a", null));
            Assert.AreEqual(1, h.Archive.Count); Assert.IsEmpty(h.RollbackRecords);
        }

        private static CandidateBattleHistory Start(BattleEntryInput input)
        {
            var prepared = new BattleEntryPreparer().PrepareCandidate(input, Math()); Assert.IsTrue(prepared.IsAccepted, prepared.RejectionCode + " " + prepared.FieldPath);
            var bytes = new byte[48]; for (var i = 0; i < 48; i += 16) { bytes[i] = 42; bytes[i + 8] = 54; }
            var binding = CandidateRandomPreparer.Prepare(prepared.Entry, new CandidateSeedMaterial {
                SourceCapabilityId = "synthetic-013-test", MappingId = CandidateRandomPreparer.SupportedMappingId, Bytes = bytes }, Math());
            Assert.IsTrue(binding.IsAccepted, binding.RejectionCode + " " + binding.FieldPath);
            var run = CandidateBattleOperations.CreateCandidate(binding.Binding, Math()); Assert.IsTrue(run.IsAccepted, run.RejectionCode + " " + run.FieldPath);
            return Accept(CandidateHistoryOperations.CreateCandidate(run.Run, Math())).Next;
        }
        private static CandidateBattleRun Evaluate(CandidateBattleRun run, string operation, int pair = 0, List<FlowPos> route = null,
            CandidateBattleConditions conditions = null, BigInteger? time = null)
        {
            var result = CandidateBattleOperations.EvaluateAttack(run, Request(run.CurrentSnapshot, operation, pair, route),
                conditions ?? new CandidateBattleConditions { PreferenceRevision = 7, ItemUseEnabled = true }, time ?? BigInteger.Zero, Sampling());
            Assert.IsTrue(result.IsAccepted, result.RejectionStage + " " + result.RejectionCode + " " + result.FieldPath); return result.NextRun;
        }
        private static CandidateBattleHistory Attack(CandidateBattleHistory history, string operation, string anchor, int pair = 0,
            List<FlowPos> route = null, CandidateBattleConditions conditions = null)
        { return Accept(CandidateHistoryOperations.Append(history, Evaluate(history.CurrentRun, operation, pair, route, conditions), anchor, Math())).Next; }
        private static CandidateHistoryResult Rollback(CandidateBattleHistory h, string operation, string anchor)
        { return Accept(CandidateHistoryOperations.PrepareRollback(h, RollbackRequest(h, operation, anchor), Range(h, anchor), Math())); }
        private static CandidateRollbackRange Range(CandidateBattleHistory h, string anchor)
        { return Accept(CandidateHistoryOperations.ReadRange(h, RangeRequest(h, anchor), Math())).Range; }
        private static CandidateHistoryOperation Find(CandidateBattleHistory h, string operation)
        { return Accept(CandidateHistoryOperations.FindOperation(h, operation, Math())).Operation; }
        private static CandidateHistoryResult Locate(CandidateBattleHistory h, int pair, bool locked)
        { return CandidateHistoryOperations.Locate(h, Locator(h, pair, locked), Math()); }
        private static CandidateHistoryLocator Locator(CandidateBattleHistory h, int pair, bool locked)
        { return new CandidateHistoryLocator { PlayerId = h.CurrentRun.Baseline.Entry.PlayerId, AttemptId = h.CurrentRun.Baseline.Entry.AttemptId,
            ExpectedSceneRevision = h.CurrentRun.CurrentSnapshot.SceneRevision, Pair = Pair(h, pair), Kind = locked ? CandidateHistoryLocatorKind.LockedRouteKillingAttack : CandidateHistoryLocatorKind.EndpointLatestAttack }; }
        private static CandidateHistoryRangeRequest RangeRequest(CandidateBattleHistory h, string anchor)
        { return new CandidateHistoryRangeRequest { PlayerId = h.CurrentRun.Baseline.Entry.PlayerId, AttemptId = h.CurrentRun.Baseline.Entry.AttemptId,
            ExpectedSceneRevision = h.CurrentRun.CurrentSnapshot.SceneRevision, HistoryAnchorId = anchor }; }
        private static CandidateRollbackRequest RollbackRequest(CandidateBattleHistory h, string operation, string anchor)
        { return new CandidateRollbackRequest { PlayerId = h.CurrentRun.Baseline.Entry.PlayerId, AttemptId = h.CurrentRun.Baseline.Entry.AttemptId,
            ExpectedSceneRevision = h.CurrentRun.CurrentSnapshot.SceneRevision, OperationId = operation, HistoryAnchorId = anchor }; }
        private static BattlePairKey Pair(CandidateBattleHistory h, int index)
        { var state = h.CurrentRun.CurrentSnapshot; return BattlePairKey.Create(state.Baseline.Entry.AttemptId, state.Board.Face.FaceId, state.Board.Face.Pairs[index].PairId); }
        private static CandidateAttackRequest Request(BattleSnapshot state, string operation, int index = 0, List<FlowPos> route = null)
        { var pair = state.Board.Face.Pairs[index]; return new CandidateAttackRequest { PlayerId = state.Baseline.Entry.PlayerId,
            AttemptId = state.Baseline.Entry.AttemptId, OperationId = operation, ExpectedSceneRevision = state.SceneRevision,
            Actor = state.Members[0].CombatantKey, Pair = BattlePairKey.Create(state.Baseline.Entry.AttemptId, state.Board.Face.FaceId, pair.PairId),
            Route = route ?? new List<FlowPos> { pair.EndpointA, new FlowPos(1, pair.EndpointA.y), new FlowPos(2, pair.EndpointA.y), pair.EndpointB } }; }
        private static CandidateHistoryResult Accept(CandidateHistoryResult result)
        { Assert.IsTrue(result.IsAccepted, result.RejectionCode + " " + result.FieldPath); Assert.IsNull(result.RejectionCode); Assert.IsFalse(result.CommitEligible); return result; }
        private static void Reject(CandidateHistoryResult result, CandidateHistoryRejectionCode code)
        { Assert.IsFalse(result.IsAccepted); Assert.AreEqual(code, result.RejectionCode); Assert.IsNotEmpty(result.FieldPath); Assert.IsNull(result.Outcome);
            Assert.IsNull(result.Next); Assert.IsNull(result.Entry); Assert.IsNull(result.Range); Assert.IsNull(result.RollbackRecord); Assert.IsNull(result.Operation); }
        private static ExactMathBudget Math() { return new ExactMathBudget(); }
        private static RandomSamplingBudget Sampling() { return new RandomSamplingBudget(Math()); }
        private static ExactRational R(BigInteger n, int d = 1) { return ExactRational.Create(n, d, Math()); }
        private static void Value(ExactRational r, int n, int d = 1) { Assert.AreEqual(new BigInteger(n), r.Numerator); Assert.AreEqual(new BigInteger(d), r.Denominator); }
        private static void AssertBusinessEqual(BattleSnapshot expected, BattleSnapshot actual)
        { Assert.AreSame(expected.Baseline, actual.Baseline); Assert.IsTrue(CandidateBattleReportFingerprint.Equal(expected, actual, Math(), true)); }
        private static CandidateBattleRun Clone(CandidateBattleRun run, BattleSnapshot current = null, IEnumerable<CandidateBattleOperationRecord> records = null)
        { return new CandidateBattleRun(run.Binding, run.Baseline, run.InitialSnapshot, current ?? run.CurrentSnapshot, records ?? run.Records, run.FinalReport); }
        private static BattleSnapshot State(BattleSnapshot s, BigInteger? revision = null, IEnumerable<BattleMemberState> members = null)
        { return new BattleSnapshot(s.Baseline, revision ?? s.SceneRevision, s.EffectiveActionsCompleted, s.EnemyPhasesCompleted, s.CurrentFaceIndex,
            s.Phase, s.Board, members ?? s.Members, s.Enemies, s.Random, s.Contributions); }
        private static CandidateBattleOperationRecord Record(CandidateBattleOperationRecord r, CandidateBattleConditionValues conditions = null, BigInteger? time = null)
        { return new CandidateBattleOperationRecord(r.Kind, time ?? r.OccurredAtUnixMilliseconds, r.BeforeSnapshot, r.AfterSnapshot,
            conditions ?? r.Conditions, r.DirectAttack, r.EnemyPhase, r.StageDecision, r.OrderedFacts, r.ContributionSegments); }
        private static CandidateRollbackRange CopyRange(CandidateRollbackRange r, CandidateRandomBinding binding = null, BigInteger? revision = null,
            string operation = null, BattleSnapshot before = null, IEnumerable<CandidateHistoryEntry> entries = null)
        { return new CandidateRollbackRange(binding ?? r.Binding, revision ?? r.SceneRevision, r.HistoryAnchorId, operation ?? r.OperationId,
            before ?? r.BeforeSnapshot, entries ?? r.Entries); }
        private static void ReadOnly(object value, HashSet<object> seen)
        {
            if (value == null || value is string || value.GetType().IsValueType || !seen.Add(value)) return;
            if (value is IList list) { Assert.IsTrue(list.IsReadOnly); Assert.Throws<NotSupportedException>(() => list.Clear()); }
            if (value is IEnumerable rows) { foreach (var row in rows) ReadOnly(row, seen); return; }
            foreach (var property in value.GetType().GetProperties()) { Assert.IsNull(property.GetSetMethod()); ReadOnly(property.GetValue(value), seen); }
        }
        private static BattleEntryInput Input(int count)
        {
            var context = new CandidateContext { DraftId = "synthetic:013", DraftRevision = 1, ContentFingerprint = "test-only",
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
                    OriginalSlot = 2, Level = 1, IsReady = true, StatsOrigin = BaseStatsOrigin.ComputedBaseStats, StatsContext = CopyContext(context),
                    Stats = Stats(100, 20, 10, 6), EntryHp = R(100), LearnedSkills = new List<string>(),
                    Crit = new WarriorCritInput { PassiveDefinitionId = "warrior:crit", TargetProbability = R(1, 5), C = R(1, 1000), Multiplier = R(3, 2) } } } };
        }
        private static BattleEntryInput Source(int level, int orientation, out List<List<FlowPos>> routes)
        {
            var fixture = DemoContentFixture.CaptureSource(level, 1, (SourceCoordinateConvention)orientation);
            var geometry = fixture.CopyLevel(); var input = Input(2); var face = input.Level.Faces[0];
            input.Level.LevelId = fixture.FixtureKey; face.Width = geometry.width; face.Height = geometry.height;
            input.Context.SourceNotes.Add(fixture.SourcePath + ":" + fixture.SourceLocator + ":" + fixture.SourceSha256);
            input.Context.SourceNotes.Add(fixture.CoordinateTransform); input.Members[0].StatsContext = CopyContext(input.Context);
            foreach (var pair in geometry.pairs) { face.Pairs[pair.colorId].EndpointA = pair.endpointA; face.Pairs[pair.colorId].EndpointB = pair.endpointB; }
            if (level == 3) { Heavy(face.Pairs[0].Enemy); face.Pairs[0].Enemy.Stats.MaxHp = R(20); face.Pairs[0].Enemy.Stats.PhysicalDefense = R(20); }
            routes = fixture.CopySolution().paths.OrderBy(p => p.colorId).Select(p => new List<FlowPos>(p.cells)).ToList(); return input;
        }
        private static void Heavy(EnemyInput enemy)
        { enemy.EnemyDefinitionId = "E02"; enemy.Behavior = EnemyBehavior.ChargeHeavy; enemy.IntentCycle = new List<EnemyIntentInput> {
            new EnemyIntentInput { Kind = EnemyIntentKind.Charge, Targeting = EnemyTargeting.FirstLiving, DamageKind = null, DamageCoefficient = null }, Strike(13, 10) }; }
        private static EnemyIntentInput Strike(int n, int d)
        { return new EnemyIntentInput { Kind = EnemyIntentKind.Strike, Targeting = EnemyTargeting.FirstLiving, DamageKind = EntryDamageKind.Physical, DamageCoefficient = R(n, d) }; }
        private static CandidateContext CopyContext(RuleContext c)
        { return new CandidateContext { DraftId = c.DraftId, DraftRevision = c.DraftRevision, ContentFingerprint = c.ContentFingerprint,
            RuleVersion = c.RuleVersion, NumericContractVersion = c.NumericContractVersion, RandomContractVersion = c.RandomContractVersion, SourceNotes = new List<string>(c.SourceNotes) }; }
        private static StatsInput Stats(int hp, int attack, int defense, int magic)
        { return new StatsInput { MaxHp = R(hp), Attack = R(attack), PhysicalDefense = R(defense), MagicDefense = R(magic), Evasion = R(0), AttackRange = 1 }; }
    }
}
