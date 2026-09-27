using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using FlowPuzzle.Core;
using NUnit.Framework;

namespace FightMatch.Core.Tests
{
    public sealed class CandidateReplayOperationsTests
    {
        [Test]
        public void RecordedOperationRuns012AgainWithOriginalIdentityConditionsAndImmutableGraph()
        {
            var h = Attack(Start(Input(3)), "op:中|😀", "A"); var original = h.Archive[0].Record;
            var input = Recorded(h, original.OperationId); var sourceHash = Describe(h.CurrentRun.CurrentSnapshot);
            var result = Accept(CandidateReplayOperations.ReplayRecorded(input, Sampling()));
            Assert.AreEqual(CandidateReplayOutcome.Matched, result.Outcome); Assert.AreEqual(true, result.Matched);
            Assert.IsNull(result.FirstDivergence); Assert.IsNull(result.PlanEvidence); Assert.IsFalse(result.NormalVictory);
            Assert.AreEqual(1, result.ActualRecords.Count); var actual = result.ActualRecords[0];
            Assert.AreNotSame(original, actual); Assert.AreNotSame(original.DirectAttack, actual.DirectAttack);
            Assert.AreNotSame(original.DirectAttack.DamageFacts[0].Crit, actual.DirectAttack.DamageFacts[0].Crit);
            Assert.AreNotSame(original.EnemyPhase, actual.EnemyPhase); Assert.AreNotSame(original.StageDecision, actual.StageDecision);
            Assert.AreSame(original.BeforeSnapshot, input.Run.CurrentSnapshot); Assert.IsEmpty(input.Run.Records);
            Assert.AreSame(h.Binding, input.Run.Binding); Assert.AreSame(h.CurrentRun.Baseline, input.Run.Baseline);
            Assert.AreSame(h.Binding.Start.Snapshot, input.Run.InitialSnapshot);
            Assert.AreEqual(original.Request.ExpectedSceneRevision, actual.Request.ExpectedSceneRevision);
            Assert.AreEqual(original.OccurredAtUnixMilliseconds, actual.OccurredAtUnixMilliseconds);
            Assert.AreEqual(7, (int)actual.Conditions.PreferenceRevision); Assert.IsTrue(actual.Conditions.ItemUseEnabled);
            Assert.AreEqual(sourceHash, Describe(h.CurrentRun.CurrentSnapshot)); Assert.AreSame(original, h.CurrentRun.Records[0]);
            ReadOnly(input, new HashSet<object>()); ReadOnly(result, new HashSet<object>());
        }

        [TestCase("a")] [TestCase("b")] [TestCase("c")]
        public void TwoRollbackGenerationsReplayTheFirstSupersedingBeforeRun(string operation)
        {
            var h = Start(Source(3, 1, out var routes)); h = Attack(h, "a", "A", 0, routes[0]);
            h = Attack(h, "b", "B", 0, routes[0]); var old = h.CurrentRun;
            h = Rollback(h, "rb1", "B"); h = Attack(h, "c", "C", 0, routes[0]); var newer = h.CurrentRun;
            h = Rollback(h, "rb2", "A"); var input = Recorded(h, operation);
            var entry = h.Archive.Single(e => e.OperationId == operation);
            Assert.AreEqual(operation == "b" ? "rb1" : "rb2", entry.SupersededBy.OperationId);
            Assert.AreSame(entry.Record.BeforeSnapshot, input.Run.CurrentSnapshot);
            Assert.AreEqual(operation == "a" ? 0 : 1, input.Run.Records.Count);
            if (operation == "b") Assert.AreSame(old.Records[0], input.Run.Records[0]);
            if (operation == "c") { Assert.AreSame(newer.Records[0], input.Run.Records[0]); Assert.Greater(input.Run.CurrentSnapshot.SceneRevision, old.CurrentSnapshot.SceneRevision); }
            var result = Accept(CandidateReplayOperations.ReplayRecorded(input, Sampling()));
            Assert.AreEqual(true, result.Matched); Assert.AreEqual(entry.Record.AfterSnapshot.SceneRevision, result.Run.CurrentSnapshot.SceneRevision);
            Assert.IsEmpty(h.CurrentRun.Records); Assert.AreEqual(2, h.RollbackRecords.Count);
        }

        [TestCase(false)] [TestCase(true)]
        public void RecordedCrossFaceAndRescueRestoreTheExactOriginalPrestate(bool rescue)
        {
            var data = Input(3);
            if (rescue) { data.Members[0].EntryHp = R(1, 3); foreach (var p in data.Level.Faces[0].Pairs) p.Enemy.Stats.MaxHp = R(100); }
            else AddSecondFace(data);
            var h = Start(data); h = Attack(h, "a", "A");
            if (!rescue) { h = Attack(h, "b", "B", 1); h = Attack(h, "c", "C", 2); h = Attack(h, "d", "D"); }
            var operation = rescue ? "a" : "d"; var old = h.Archive.Single(e => e.OperationId == operation).Record;
            if (rescue) { Assert.AreEqual(BattlePhase.AwaitRescue, old.AfterSnapshot.Phase); Assert.AreEqual(1, old.EnemyPhase.OrderedIntents.Count); Assert.AreEqual(BigInteger.Zero, old.AfterSnapshot.Enemies[1].IntentCursor); }
            else Assert.AreEqual(1, old.BeforeSnapshot.CurrentFaceIndex);
            h = Rollback(h, "rb", "A"); var result = Accept(CandidateReplayOperations.ReplayRecorded(Recorded(h, operation), Sampling()));
            Assert.AreEqual(true, result.Matched); Assert.IsTrue(CandidateBattleReportFingerprint.Equal(old.AfterSnapshot, result.Run.CurrentSnapshot, Math()));
            Assert.AreEqual(0, h.CurrentRun.CurrentSnapshot.CurrentFaceIndex); Assert.AreEqual(BattlePhase.AwaitAction, h.CurrentRun.CurrentSnapshot.Phase);
        }

        [Test]
        public void RejectionSamplingPreservesAllRawWordsAndSharedWordBudget()
        {
            var data = Input(3); data.Members[0].Crit.C = R(1, 3);
            var h = Attack(Start(data, 4568919932995229530UL, 0), "a", "A"); var input = Recorded(h, "a");
            var actual = Accept(CandidateReplayOperations.ReplayRecorded(input, Sampling())).ActualRecords[0].DirectAttack.DamageFacts[0].Crit;
            Assert.AreEqual(3, actual.Words.Count); Assert.AreEqual(0U, actual.Words[0]); Assert.AreEqual(0U, actual.Words[1]); Assert.AreNotEqual(0U, actual.Words[2]);
            Assert.AreEqual(new BigInteger(3), actual.StreamAfter.WordsConsumed); Value(actual.Parameters.C, 1, 3);
            Value(actual.Probability, 1, 3); Assert.AreEqual(BigInteger.Zero, actual.FailureCountBefore);
            var tight = new RandomSamplingBudget(Math(), 2); CandidateReplayResult partial = null;
            var error = Assert.Throws<ExactMathLimitException>(() => partial = CandidateReplayOperations.ReplayRecorded(input, tight));
            Assert.AreEqual("RandomWords", error.ReasonCode); Assert.AreEqual(2, tight.WordsUsed); Assert.IsNull(partial);
            var retry = Accept(CandidateReplayOperations.ReplayRecorded(input, Sampling())); Assert.AreEqual(true, retry.Matched);
            Assert.AreEqual(BigInteger.Zero, input.Run.CurrentSnapshot.Random.Stream.WordsConsumed);
            var shared = new RandomSamplingBudget(Math(), 3);
            Accept(CandidateReplayOperations.ReplayRecorded(input, shared)); Assert.AreEqual(3, shared.WordsUsed);
            Assert.Throws<ExactMathLimitException>(() => CandidateReplayOperations.ReplayRecorded(input, shared));
            Assert.AreEqual(3, shared.WordsUsed);
        }

        [Test]
        public void CertainCritAndRejectedLinkUseNoSamplingWords()
        {
            var data = Input(3); data.Members[0].Crit.C = R(1); var h = Attack(Start(data), "a", "A");
            var zero = new RandomSamplingBudget(Math(), 0);
            var replay = Accept(CandidateReplayOperations.ReplayRecorded(Recorded(h, "a"), zero));
            var crit = replay.ActualRecords[0].DirectAttack.DamageFacts[0].Crit;
            Assert.IsTrue(crit.Triggered); Value(crit.Probability, 1); Assert.IsEmpty(crit.Words); Assert.AreEqual(0, zero.WordsUsed);
            var initial = Start(Input(3)); var step = Step(initial.CurrentRun.CurrentSnapshot, "link"); step.Kind = CandidateBattleOperationKind.Link; step.Actor = null;
            var result = CandidateReplayOperations.EvaluatePlan(Plan(initial), new[] { step }, zero);
            var direct = CandidateBattleOperations.EvaluateLink(initial.CurrentRun, new CandidateLinkRequest { PlayerId = initial.Binding.GeneratedForPlayerId,
                AttemptId = initial.Binding.GeneratedForAttemptId, OperationId = "link", ExpectedSceneRevision = 1, Pair = step.Pair, Route = step.Route }, step.OccurredAtUnixMilliseconds, Math());
            Reject(result, direct.RejectionCode, 0); Assert.AreEqual(direct.FieldPath, result.FieldPath); Assert.AreEqual(direct.RejectionStage, result.RejectionStage);
            Assert.AreEqual(0, zero.WordsUsed); Assert.IsEmpty(initial.Archive);
        }

        [TestCase(1, 1)] [TestCase(1, 2)] [TestCase(3, 1)] [TestCase(3, 2)]
        public void CompleteRealSourcePlanProducesReportIncludingLastStep(int level, int orientation)
        {
            var h = Start(Source(level, orientation, out var routes)); var input = Plan(h);
            var steps = new List<CandidateReplayStep> { Step(h.CurrentRun.CurrentSnapshot, "a", 0, routes[0]) };
            if (level == 3) steps.Add(Step(h.CurrentRun.CurrentSnapshot, "b", 0, routes[0]));
            steps.Add(Step(h.CurrentRun.CurrentSnapshot, "terminal", 1, routes[1])); steps.Last().OccurredAtUnixMilliseconds = BigInteger.One << 90;
            var result = Accept(CandidateReplayOperations.EvaluatePlan(input, steps, Sampling()));
            Assert.AreEqual(CandidateReplayOutcome.PlanEvaluated, result.Outcome); Assert.IsNull(result.Matched); Assert.IsTrue(result.NormalVictory);
            Assert.AreEqual(BattlePhase.WonPendingSettlement, result.Run.CurrentSnapshot.Phase);
            Assert.AreEqual(steps.Count, result.ActualRecords.Count); Assert.AreEqual(steps.Count, result.FinalReport.Operations.Count);
            Assert.AreSame(result.ActualRecords.Last(), result.FinalReport.Operations.Last()); Assert.AreEqual("terminal", result.FinalReport.TerminalOperationId);
            Assert.AreEqual(BigInteger.One << 90, result.FinalReport.EndedAtUnixMilliseconds);
            Assert.AreEqual(CandidateBattleReportFingerprint.Compute(result.FinalReport, Math()), result.FinalReport.Fingerprint);
            Assert.AreEqual(steps.Count, result.PlanEvidence.Steps.Count); Assert.AreSame(input.ContextKey, result.PlanEvidence.ContextKey);
            for (var i = 0; i < steps.Count; i++) Assert.AreEqual(new BigInteger(i + 1), result.ActualRecords[i].Request.ExpectedSceneRevision);
            Assert.IsEmpty(h.CurrentRun.Records); Assert.IsNull(h.CurrentRun.FinalReport);
            ReadOnly(result, new HashSet<object>());
        }

        [Test]
        public void FullPlanCrossesFacesWithoutChangingOriginalHistory()
        {
            var data = Input(2); AddSecondFace(data); var h = Start(data); var initial = h.CurrentRun.CurrentSnapshot;
            var steps = new[] { Step(initial, "a"), Step(initial, "b", 1), Step(initial, "c"), Step(initial, "d", 1) };
            for (var i = 2; i < 4; i++) steps[i].Pair = BattlePairKey.Create(initial.Baseline.Entry.AttemptId, "face:1", "P" + (i - 2));
            var result = Accept(CandidateReplayOperations.EvaluatePlan(Plan(h), steps, Sampling()));
            Assert.IsTrue(result.NormalVictory); Assert.AreEqual(4, result.FinalReport.Operations.Count);
            Assert.IsTrue(result.ActualRecords[1].StageDecision.DidFlipFace); Assert.AreEqual(1, result.ActualRecords[2].BeforeSnapshot.CurrentFaceIndex);
            Assert.AreSame(initial, h.CurrentRun.CurrentSnapshot); Assert.IsEmpty(initial.Board.LockedRoutes);
        }

        [TestCase("Range")] [TestCase("Route")] [TestCase("Rescue")] [TestCase("TerminalTail")]
        public void PlanFailurePreservesOriginal012RejectionAndNeverReturnsSuccessfulPrefix(string failure)
        {
            var data = Input(2);
            if (failure == "Rescue") { data.Members[0].EntryHp = R(1, 3); foreach (var p in data.Level.Faces[0].Pairs) p.Enemy.Stats.MaxHp = R(100); }
            var h = Start(data); var before = h.CurrentRun.CurrentSnapshot; var first = Step(before, "a"); var second = Step(before, "b", 1);
            if (failure == "Range") first = Step(before, "a", 1);
            var steps = new List<CandidateReplayStep> { first };
            if (failure == "Route") first.Route = new List<FlowPos> { first.Route[0], first.Route[3] };
            if (failure == "Rescue" || failure == "TerminalTail") steps.Add(second);
            if (failure == "TerminalTail") steps.Add(Step(before, "tail"));
            var result = CandidateReplayOperations.EvaluatePlan(Plan(h), steps, Sampling());
            var run = h.CurrentRun; CandidateBattleResult direct = null; var index = -1;
            for (var i = 0; i < steps.Count; i++) { direct = Direct(run, steps[i]); if (!direct.IsAccepted) { index = i; break; } run = direct.NextRun; }
            Assert.GreaterOrEqual(index, 0); Reject(result, direct.RejectionCode, index);
            Assert.AreEqual(direct.RejectionStage, result.RejectionStage); Assert.AreEqual(direct.FieldPath, result.FieldPath);
            Assert.AreEqual(direct.RouteReasonCode, result.RouteReasonCode); Assert.AreEqual(direct.RouteCellIndex, result.RouteCellIndex);
            Assert.IsEmpty(h.CurrentRun.Records); Assert.AreSame(before, h.CurrentRun.CurrentSnapshot);
        }

        [Test]
        public void RescueIsAnAcceptedPartialPlanOutcomeWithoutClaimingVictoryOrNoSolution()
        {
            var data = Input(3); data.Members[0].EntryHp = R(1, 3); foreach (var p in data.Level.Faces[0].Pairs) p.Enemy.Stats.MaxHp = R(100);
            var h = Start(data); var result = Accept(CandidateReplayOperations.EvaluatePlan(Plan(h), new[] { Step(h.CurrentRun.CurrentSnapshot, "a") }, Sampling()));
            Assert.AreEqual(BattlePhase.AwaitRescue, result.Run.CurrentSnapshot.Phase); Assert.IsFalse(result.NormalVictory); Assert.IsNull(result.FinalReport);
            Assert.AreEqual(CandidateReplayOutcome.PlanEvaluated, result.Outcome); Assert.AreEqual(1, result.PlanEvidence.Steps.Count);
        }

        [TestCase("Effective")] [TestCase("Superseded")] [TestCase("Rollback")] [TestCase("WithinPlan")]
        public void PlanRejectsEveryKindOfPreviouslyUsedOperationId(string source)
        {
            var h = Attack(Start(Input(3)), "used", "A");
            if (source == "Superseded" || source == "Rollback") h = Rollback(h, "rb", "A");
            var state = h.CurrentRun.CurrentSnapshot; var step = Step(state, source == "Rollback" ? "rb" : "used", source == "Effective" ? 1 : 0);
            var steps = new List<CandidateReplayStep> { step };
            if (source == "WithinPlan") { step.OperationId = "new"; step.Pair = Pair(state, 1); step.Route = Request(state, "new", 1).Route; steps.Add(Step(state, "new", 2)); }
            Reject(CandidateReplayOperations.EvaluatePlan(Plan(h), steps, Sampling()), "OperationConflict", source == "WithinPlan" ? 1 : 0);
        }

        [TestCase("Revision")] [TestCase("Enabled")] [TestCase("Definitions")] [TestCase("Random")] [TestCase("SourceRevision")]
        public void ContextKeyBindsCompleteValuesWithoutRelyingOnBaselineIdOrHash(string difference)
        {
            var data = Input(3); var h = Start(data); var left = Plan(h); CandidateBattleIsolation right;
            if (difference == "Revision") right = Plan(h, 8, true);
            else if (difference == "Enabled") right = Plan(h, 7, false);
            else if (difference == "Definitions") { data.Level.Faces[0].Pairs[0].Enemy.Stats.MaxHp = R(16); right = Plan(Start(data)); }
            else if (difference == "Random") right = Plan(Start(data, 43));
            else right = Plan(Rollback(Attack(h, "a", "A"), "rb", "A"));
            Assert.AreEqual(left.Run.Baseline.Entry.EntryBaselineId, right.Run.Baseline.Entry.EntryBaselineId);
            Assert.IsFalse(left.ContextKey.HasSameValue(right.ContextKey, Math()));
            Assert.IsTrue(left.ContextKey.HasSameValue(Plan(Start(Input(3))).ContextKey, Math()));
            Assert.IsFalse(left.ContextKey.HasSameValue(null, Math()));
        }

        [Test]
        public void ConditionsRequestsRoutesAndOrderedStepsAreFrozenAndBindDistinctEvidence()
        {
            var h = Attack(Start(Input(3)), "old", "A"); var request = IsolationRequest(h);
            request.CurrentConditions = new CandidateBattleConditions { PreferenceRevision = 31, ItemUseEnabled = false };
            var input = Accept(CandidateReplayOperations.BuildIsolationInput(h, request, Math())).Input;
            request.CurrentConditions.PreferenceRevision = 99; request.CurrentConditions.ItemUseEnabled = true; request.PlayerId = "changed";
            var old = Accept(CandidateReplayOperations.ReplayRecorded(Recorded(h, "old"), Sampling()));
            Assert.AreEqual(7, (int)old.ActualRecords[0].Conditions.PreferenceRevision); Assert.IsTrue(old.ActualRecords[0].Conditions.ItemUseEnabled);
            var step = Step(h.CurrentRun.CurrentSnapshot, "new", 1); var steps = new List<CandidateReplayStep> { step };
            var result = Accept(CandidateReplayOperations.EvaluatePlan(input, steps, Sampling()));
            Assert.AreEqual(31, (int)result.ActualRecords[0].Conditions.PreferenceRevision); Assert.IsFalse(result.ActualRecords[0].Conditions.ItemUseEnabled);
            var same = Accept(CandidateReplayOperations.EvaluatePlan(input, steps, Sampling())); Assert.IsTrue(result.PlanEvidence.HasSameValue(same.PlanEvidence, Math()));
            step.OccurredAtUnixMilliseconds = 999;
            var differentTime = Accept(CandidateReplayOperations.EvaluatePlan(input, steps, Sampling())); Assert.IsFalse(result.PlanEvidence.HasSameValue(differentTime.PlanEvidence, Math()));
            step.OperationId = "another";
            var differentId = Accept(CandidateReplayOperations.EvaluatePlan(input, steps, Sampling())); Assert.IsFalse(differentTime.PlanEvidence.HasSameValue(differentId.PlanEvidence, Math()));
            step.Route.Clear(); steps.Clear(); Assert.AreEqual(4, result.ActualRecords[0].Request.Route.Count); Assert.AreEqual("new", result.PlanEvidence.Steps[0].OperationId);
            Assert.AreEqual(1, h.Archive.Count); Assert.AreEqual(7, (int)h.Archive[0].Record.Conditions.PreferenceRevision);
        }

        [TestCase("PlayerMissing", "MissingField")] [TestCase("AttemptMissing", "MissingField")]
        [TestCase("RevisionMissing", "MissingField")] [TestCase("PurposeMissing", "MissingField")]
        [TestCase("RightsMissing", "MissingField")] [TestCase("ConditionsMissing", "MissingField")]
        [TestCase("PreferenceMissing", "MissingField")] [TestCase("EnabledMissing", "MissingField")]
        [TestCase("RevisionZero", "InvalidValue")] [TestCase("PreferenceZero", "InvalidValue")]
        [TestCase("PlayerWrong", "InconsistentBinding")] [TestCase("AttemptWrong", "InconsistentBinding")]
        [TestCase("Stale", "StaleContext")] [TestCase("PurposeUnknown", "UnsupportedBinding")]
        [TestCase("RightsUnknown", "UnsupportedBinding")] [TestCase("CurrentWithTarget", "InvalidValue")]
        [TestCase("RecordedWithConditions", "InvalidValue")] [TestCase("RecordedMissing", "MissingField")]
        [TestCase("RecordedUnknown", "NotFound")] [TestCase("RollbackTarget", "UnsupportedBinding")]
        public void IsolationRequestsRejectMissingConflictingOrUnsupportedFields(string change, string code)
        {
            var h = Rollback(Attack(Start(Input(3)), "old", "A"), "rb", "A"); var r = IsolationRequest(h);
            switch (change)
            {
                case "PlayerMissing": r.PlayerId = null; break; case "AttemptMissing": r.AttemptId = " "; break;
                case "RevisionMissing": r.ExpectedSceneRevision = null; break; case "PurposeMissing": r.Purpose = null; break;
                case "RightsMissing": r.RightsMode = null; break; case "ConditionsMissing": r.CurrentConditions = null; break;
                case "PreferenceMissing": r.CurrentConditions.PreferenceRevision = null; break; case "EnabledMissing": r.CurrentConditions.ItemUseEnabled = null; break;
                case "RevisionZero": r.ExpectedSceneRevision = 0; break; case "PreferenceZero": r.CurrentConditions.PreferenceRevision = 0; break;
                case "PlayerWrong": r.PlayerId = "PLAYER"; break; case "AttemptWrong": r.AttemptId = "Attempt"; break;
                case "Stale": r.ExpectedSceneRevision = 1; break; case "PurposeUnknown": r.Purpose = (CandidateIsolationPurpose)99; break;
                case "RightsUnknown": r.RightsMode = (CandidateIsolationRights)99; break; case "CurrentWithTarget": r.RecordedOperationId = "old"; break;
                case "RecordedWithConditions": r.Purpose = CandidateIsolationPurpose.RecordedOperation; r.RecordedOperationId = "old"; break;
                default: r.Purpose = CandidateIsolationPurpose.RecordedOperation; r.CurrentConditions = null;
                    r.RecordedOperationId = change == "RecordedMissing" ? null : change == "RollbackTarget" ? "rb" : "missing"; break;
            }
            Reject(CandidateReplayOperations.BuildIsolationInput(h, r, Math()), code); Assert.AreEqual(1, h.RollbackRecords.Count);
        }

        [TestCase("Step", "MissingField")] [TestCase("Kind", "MissingField")] [TestCase("KindUnknown", "UnsupportedBinding")]
        [TestCase("Operation", "MissingField")] [TestCase("Time", "MissingField")] [TestCase("NegativeTime", "InvalidValue")]
        [TestCase("Actor", "MissingField")] [TestCase("Pair", "MissingField")] [TestCase("Route", "MissingField")]
        [TestCase("LinkActor", "InvalidValue")]
        public void InvalidPlanStepReturnsItsExactIndexWithoutLeakingPrefix(string change, string code)
        {
            var h = Start(Input(3)); var state = h.CurrentRun.CurrentSnapshot; var broken = Step(state, "b", 1);
            switch (change)
            {
                case "Step": broken = null; break; case "Kind": broken.Kind = null; break;
                case "KindUnknown": broken.Kind = (CandidateBattleOperationKind)99; break; case "Operation": broken.OperationId = " "; break;
                case "Time": broken.OccurredAtUnixMilliseconds = null; break; case "NegativeTime": broken.OccurredAtUnixMilliseconds = -1; break;
                case "Actor": broken.Actor = null; break; case "Pair": broken.Pair = null; break; case "Route": broken.Route = null; break;
                case "LinkActor": broken.Kind = CandidateBattleOperationKind.Link; break;
            }
            var result = CandidateReplayOperations.EvaluatePlan(Plan(h), new[] { Step(state, "a"), broken }, Sampling());
            Reject(result, code, 1); StringAssert.StartsWith("Steps[1]", result.FieldPath); Assert.IsEmpty(h.CurrentRun.Records);
        }

        [Test]
        public void EmptyAndWrongPurposePlansAreExplicitRejections()
        {
            var h = Attack(Start(Input(3)), "a", "A");
            Reject(CandidateReplayOperations.EvaluatePlan(Plan(h), Array.Empty<CandidateReplayStep>(), Sampling()), "InvalidValue");
            Reject(CandidateReplayOperations.ReplayRecorded(Plan(h), Sampling()), "UnsupportedBinding");
            Reject(CandidateReplayOperations.EvaluatePlan(Recorded(h, "a"), new[] { Step(h.CurrentRun.CurrentSnapshot, "b", 1) }, Sampling()), "UnsupportedBinding");
        }

        [TestCase("NoRelation")] [TestCase("WrongRollback")]
        public void MissingOriginalRelationCannotBeGuessedFromCurrentSnapshotOrArchiveOrder(string change)
        {
            var h = Rollback(Attack(Start(Input(3)), "a", "A"), "rb", "A");
            var entry = h.Archive[0];
            var changed = new CandidateHistoryEntry(entry.HistoryAnchorId, entry.Record,
                change == "NoRelation" ? null : new CandidateHistorySupersededBy("missing", entry.SupersededBy.SceneRevision));
            var forged = new CandidateBattleHistory(h.CurrentRun, new[] { changed }, h.EffectiveAnchors, h.RollbackRecords);
            var request = IsolationRequest(forged, "a");
            Reject(CandidateReplayOperations.BuildIsolationInput(forged, request, Math()), "IncompleteHistory");
        }

        [TestCase("Word", "Record.DirectAttack.DamageFacts[0].Crit.Words[0]")]
        [TestCase("WordCount", "Record.DirectAttack.DamageFacts[0].Crit.Words.Count")]
        [TestCase("C", "Record.DirectAttack.DamageFacts[0].Crit.Parameters.C.Numerator")]
        [TestCase("Probability", "Record.DirectAttack.DamageFacts[0].Crit.Probability.Numerator")]
        [TestCase("Failure", "Record.DirectAttack.DamageFacts[0].Crit.FailureCountBefore")]
        [TestCase("Stream", "Record.DirectAttack.DamageFacts[0].Crit.StreamAfter.WordsConsumed")]
        [TestCase("Damage", "Record.DirectAttack.DamageFacts[0].RoundedDamage")]
        [TestCase("Enemy", "Record.EnemyPhase.OrderedIntents[0].CursorAfter")]
        [TestCase("EnemyCount", "Record.EnemyPhase.OrderedIntents.Count")]
        [TestCase("Stage", "Record.StageDecision.NextPhase")]
        [TestCase("Board", "Record.StageDecision.Board.LockedRoutes.Count")]
        [TestCase("Ordered", "Record.OrderedFacts[0].Index")]
        [TestCase("Contribution", "Record.ContributionSegments[0].HpLoss.Numerator")]
        [TestCase("ContributionCount", "Record.ContributionSegments.Count")]
        [TestCase("Revision", "Record.AfterSnapshot.SceneRevision")]
        [TestCase("ActionCount", "Record.AfterSnapshot.EffectiveActionsCompleted")]
        [TestCase("EnemyPhaseCount", "Record.AfterSnapshot.EnemyPhasesCompleted")]
        [TestCase("Phase", "Record.AfterSnapshot.Phase")]
        [TestCase("AfterHp", "Record.AfterSnapshot.Members[0].Hp.Numerator")]
        [TestCase("AfterCursor", "Record.AfterSnapshot.Enemies[1].IntentCursor")]
        [TestCase("AfterBoard", "Record.AfterSnapshot.Board.LockedRoutes.Count")]
        [TestCase("AfterRandom", "Record.AfterSnapshot.Random.Stream.WordsConsumed")]
        public void CompleteButChangedRecordedEvidenceReportsFirstBusinessField(string change, string path)
        {
            var h = Attack(Start(Input(3)), "a", "A"); var input = Recorded(h, "a");
            var record = Alter(input.RecordedOperation, change);
            var changed = new CandidateBattleIsolation(input.ContextKey, record);
            var sampling = Sampling(); var result = Accept(CandidateReplayOperations.ReplayRecorded(changed, sampling));
            Assert.AreEqual(CandidateReplayOutcome.Diverged, result.Outcome); Assert.AreEqual(false, result.Matched);
            Assert.Greater(sampling.WordsUsed, 0); Assert.AreNotSame(record, result.ActualRecords[0]);
            Assert.AreEqual(path, result.FirstDivergence.FieldPath); Assert.AreEqual(0, result.FirstDivergence.StepIndex);
            Assert.AreEqual("a", result.FirstDivergence.OperationId); Assert.AreNotEqual(result.FirstDivergence.Expected, result.FirstDivergence.Actual);
            Assert.IsNull(result.RejectionCode); Assert.IsNull(result.PlanEvidence); Assert.IsFalse(result.CommitEligible);
            Assert.IsTrue(CandidateBattleReportFingerprint.Equal(h.Archive[0].Record, result.ActualRecords[0], Math()));
        }

        [Test]
        public void RandomEvidenceWinsOverDamageAndLaterSnapshotDifferences()
        {
            var h = Attack(Start(Input(3)), "a", "A"); var input = Recorded(h, "a");
            var record = Alter(Alter(Alter(input.RecordedOperation, "AfterHp"), "Damage"), "Word");
            var result = Accept(CandidateReplayOperations.ReplayRecorded(new CandidateBattleIsolation(input.ContextKey, record), Sampling()));
            Assert.AreEqual("Record.DirectAttack.DamageFacts[0].Crit.Words[0]", result.FirstDivergence.FieldPath);
        }

        [TestCase("EndedAt", "FinalReport.EndedAtUnixMilliseconds")]
        [TestCase("Terminal", "FinalReport.TerminalOperationId")]
        [TestCase("Fingerprint", "FinalReport.Fingerprint")]
        [TestCase("Operations", "FinalReport.Operations.Count")]
        [TestCase("Contribution", "FinalReport.Contributions.Count")]
        [TestCase("WholeHp", "FinalReport.WholeLevelInitialEnemyHp.Numerator")]
        public void TerminalReplayChecksCompleteReportAndFingerprintAfterLastOperation(string change, string path)
        {
            var h = Start(Input(2)); h = Attack(Attack(h, "a", "A"), "end", "END", 1);
            var input = Recorded(h, "end"); var matched = Accept(CandidateReplayOperations.ReplayRecorded(input, Sampling()));
            Assert.AreEqual(true, matched.Matched); Assert.IsTrue(matched.NormalVictory); Assert.AreEqual(2, matched.FinalReport.Operations.Count);
            var r = input.RecordedReport;
            var altered = new CandidateFinalAttemptReport(r.Binding, r.Baseline, r.InitialSnapshot,
                change == "Operations" ? r.Operations.Take(1) : r.Operations, r.FinalSnapshot,
                change == "Contribution" ? r.Contributions.Skip(1) : r.Contributions, r.Outcome,
                change == "EndedAt" ? r.EndedAtUnixMilliseconds + 1 : r.EndedAtUnixMilliseconds,
                change == "Terminal" ? "END" : r.TerminalOperationId, r.ConsumptionCoverage,
                change == "WholeHp" ? R(99) : r.WholeLevelInitialEnemyHp, change == "Fingerprint" ? "changed" : r.Fingerprint);
            var result = Accept(CandidateReplayOperations.ReplayRecorded(new CandidateBattleIsolation(input.ContextKey, input.RecordedOperation, altered), Sampling()));
            Assert.AreEqual(false, result.Matched); Assert.AreEqual(path, result.FirstDivergence.FieldPath);
            Assert.AreNotSame(r, result.FinalReport); Assert.AreEqual(r.Fingerprint, result.FinalReport.Fingerprint);
        }

        [TestCase("Baseline")] [TestCase("Initial")] [TestCase("AfterBaseline")] [TestCase("DirectBinding")]
        [TestCase("MissingRecord")] [TestCase("MissingCrit")] [TestCase("MissingReport")]
        public void IncompleteOrForeignGraphIsRejectedBeforeTakingAnyRandomWord(string change)
        {
            var h = Attack(Start(Input(2)), "a", "A");
            if (change == "MissingReport") h = Attack(h, "end", "END", 1);
            var input = Recorded(h, change == "MissingReport" ? "end" : "a"); var run = input.Run; var record = input.RecordedOperation;
            var foreign = Start(Input(2)).CurrentRun;
            if (change == "Baseline") run = new CandidateBattleRun(run.Binding, foreign.Baseline, run.InitialSnapshot, run.CurrentSnapshot, run.Records, null);
            if (change == "Initial") run = new CandidateBattleRun(run.Binding, run.Baseline, foreign.InitialSnapshot, run.CurrentSnapshot, run.Records, null);
            if (change == "AfterBaseline") record = Copy(record, after: State(record.AfterSnapshot, baseline: foreign.Baseline));
            if (change == "DirectBinding")
            {
                var d = record.DirectAttack; var direct = new CandidateCombatFrame(foreign.Binding, d.BeforeSnapshot, d.Action, d.Enemies, d.Random, d.Contributions, d.DamageFacts[0]);
                record = Copy(record, direct: direct, enemy: EnemyFrame(record.EnemyPhase, direct));
            }
            if (change == "MissingCrit")
            {
                var d = record.DirectAttack; var fact = Damage(d, null); var direct = new CandidateCombatFrame(d.Binding, d.BeforeSnapshot, d.Action, d.Enemies, d.Random, d.Contributions, fact);
                record = Copy(record, direct: direct, enemy: EnemyFrame(record.EnemyPhase, direct));
            }
            var key = input.ContextKey;
            key = new CandidateReplayContextKey(key.Purpose, key.PlayerId, key.AttemptId, key.SourceSceneRevision, run,
                key.Conditions, key.RightsMode, key.RecordedOperationId, key.UsedOperationIds, key.RecordedHasFinalReport);
            var bad = new CandidateBattleIsolation(key, change == "MissingRecord" ? null : record,
                change == "MissingReport" ? null : input.RecordedReport); var sampling = Sampling();
            var result = CandidateReplayOperations.ReplayRecorded(bad, sampling);
            Reject(result, change.StartsWith("Missing", StringComparison.Ordinal) ? "IncompleteHistory" : "InconsistentBinding");
            Assert.AreEqual(0, sampling.WordsUsed); Assert.AreEqual(true, Accept(CandidateReplayOperations.ReplayRecorded(input, Sampling())).Matched);
        }

        [TestCase("Build")] [TestCase("Replay")] [TestCase("Plan")]
        public void CallerMathLimitPropagatesWithoutAnySuccessObjectAndFreshBudgetRetries(string method)
        {
            var h = Attack(Start(Input(3)), "a", "A"); var recorded = Recorded(h, "a"); var plan = Plan(h);
            var before = Describe(h.CurrentRun.CurrentSnapshot); CandidateReplayResult result = null;
            var tight = new ExactMathBudget(maxPrimitiveSteps: 0);
            Assert.Throws<ExactMathLimitException>(() =>
            {
                if (method == "Build") result = CandidateReplayOperations.BuildIsolationInput(h, IsolationRequest(h, "a"), tight);
                else if (method == "Replay") result = CandidateReplayOperations.ReplayRecorded(recorded, new RandomSamplingBudget(tight));
                else result = CandidateReplayOperations.EvaluatePlan(plan, new[] { Step(h.CurrentRun.CurrentSnapshot, "b", 1) }, new RandomSamplingBudget(tight));
            });
            Assert.IsNull(result); Assert.AreEqual(before, Describe(h.CurrentRun.CurrentSnapshot));
            Assert.AreEqual(true, Accept(CandidateReplayOperations.ReplayRecorded(recorded, Sampling())).Matched);
            Accept(CandidateReplayOperations.EvaluatePlan(plan, new[] { Step(h.CurrentRun.CurrentSnapshot, "b", 1) }, Sampling()));
        }

        [TestCase(false)] [TestCase(true)]
        public void MathLimitAtFinalComparisonOrPlanStepCannotReturnPartialProof(bool plan)
        {
            var h = Attack(Start(Input(3)), "a", "A"); var input = plan ? Plan(h) : Recorded(h, "a");
            var steps = new[] { Step(h.CurrentRun.CurrentSnapshot, "b", 1), Step(h.CurrentRun.CurrentSnapshot, "c", 2) };
            var measured = Sampling(); var success = plan ? CandidateReplayOperations.EvaluatePlan(input, steps, measured) : CandidateReplayOperations.ReplayRecorded(input, measured);
            Accept(success); Assert.Greater(measured.Math.PrimitiveStepsUsed, 1);
            var tight = new RandomSamplingBudget(new ExactMathBudget(maxPrimitiveSteps: checked((int)measured.Math.PrimitiveStepsUsed - 1)));
            CandidateReplayResult partial = null;
            Assert.Throws<ExactMathLimitException>(() => partial = plan ? CandidateReplayOperations.EvaluatePlan(input, steps, tight) : CandidateReplayOperations.ReplayRecorded(input, tight));
            Assert.IsNull(partial); Assert.Greater(tight.WordsUsed, 0);
            var retry = Accept(plan ? CandidateReplayOperations.EvaluatePlan(input, steps, Sampling()) : CandidateReplayOperations.ReplayRecorded(input, Sampling()));
            Assert.AreEqual(success.Outcome, retry.Outcome); Assert.IsTrue(CandidateBattleReportFingerprint.Equal(success.Run.CurrentSnapshot, retry.Run.CurrentSnapshot, Math()));
            Assert.AreEqual(1, h.CurrentRun.Records.Count);
        }

        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)] [TestCase(5)] [TestCase(6)] [TestCase(7)]
        public void NullRootsThrowArgumentNull(int method)
        {
            var h = Attack(Start(Input(3)), "a", "A"); var input = Recorded(h, "a");
            Assert.Throws<ArgumentNullException>(() =>
            {
                switch (method)
                {
                    case 0: CandidateReplayOperations.BuildIsolationInput(null, IsolationRequest(h), Math()); break;
                    case 1: CandidateReplayOperations.BuildIsolationInput(h, null, Math()); break;
                    case 2: CandidateReplayOperations.BuildIsolationInput(h, IsolationRequest(h), null); break;
                    case 3: CandidateReplayOperations.ReplayRecorded(null, Sampling()); break;
                    case 4: CandidateReplayOperations.ReplayRecorded(input, null); break;
                    case 5: CandidateReplayOperations.EvaluatePlan(null, Array.Empty<CandidateReplayStep>(), Sampling()); break;
                    case 6: CandidateReplayOperations.EvaluatePlan(input, null, Sampling()); break;
                    case 7: CandidateReplayOperations.EvaluatePlan(input, Array.Empty<CandidateReplayStep>(), null); break;
                }
            });
            Assert.Throws<ArgumentNullException>(() => input.ContextKey.HasSameValue(input.ContextKey, null)); Assert.AreEqual(1, h.Archive.Count);
        }

        private static CandidateBattleOperationRecord Alter(CandidateBattleOperationRecord r, string change)
        {
            var d = r.DirectAttack; var c = d.DamageFacts[0].Crit;
            if (change == "Word" || change == "WordCount" || change == "C" || change == "Probability" || change == "Failure" || change == "Stream" || change == "Damage")
            {
                var words = c.Words.ToList(); if (change == "Word") words[0] ^= 1; if (change == "WordCount") words.Add(0);
                var parameters = change == "C" ? new PreparedWarriorCrit(new WarriorCritInput { PassiveDefinitionId = c.Parameters.PassiveDefinitionId,
                    TargetProbability = c.Parameters.TargetProbability, C = R(3, 1000), Multiplier = c.Parameters.Multiplier }) : c.Parameters;
                var after = change == "Stream" ? new Pcg32StreamState(c.StreamAfter.Initial, c.StreamAfter.Current, c.StreamAfter.WordsConsumed + 1) : c.StreamAfter;
                var crit = new CandidateCritFact(c.Actor, c.Target, c.OpportunityOrdinal, parameters,
                    new BattlePrdState(c.Actor, c.Parameters, change == "Failure" ? c.FailureCountBefore + 1 : c.FailureCountBefore), c.StreamBefore,
                    new RandomSample<PrdOutcome>(new PrdOutcome(c.Triggered, c.FailureCountAfter,
                        change == "Probability" ? R(3, 1000) : c.Probability), after, words));
                var direct = new CandidateCombatFrame(d.Binding, d.BeforeSnapshot, d.Action, d.Enemies, d.Random, d.Contributions,
                    Damage(d, crit, change == "Damage" ? d.DamageFacts[0].RoundedDamage + 1 : (BigInteger?)null));
                return Copy(r, direct: direct, enemy: EnemyFrame(r.EnemyPhase, direct));
            }
            if (change == "Enemy" || change == "EnemyCount")
            {
                var frame = r.EnemyPhase; var intents = frame.OrderedIntents.ToList();
                if (change == "EnemyCount") intents.RemoveAt(0);
                else { var f = intents[0]; var enemy = d.Enemies.Single(e => e.CombatantKey.Equals(f.EnemyKey));
                    intents[0] = new CandidateEnemyIntentFact(d, f.EnemyPhaseOrdinal, f.SegmentIndex, enemy, f.IntentKind, f.CursorAfter + 1, f.Damage); }
                return Copy(r, enemy: new CandidateEnemyPhaseFrame(d, frame.EnemyPhaseOrdinal, frame.Members, frame.Enemies, frame.Contributions, intents));
            }
            if (change == "Stage" || change == "Board")
            {
                var s = r.StageDecision;
                return Copy(r, stage: new CandidateStageDecision(s.BeforeSnapshot, s.Source, s.FinalHp,
                    change == "Board" ? new BattleBoardState(s.Board.Face) : s.Board, s.NextFaceIndex,
                    change == "Stage" ? BattlePhase.AwaitRescue : s.NextPhase, s.NextFace, s.OrderedFacts));
            }
            if (change == "Ordered") { var facts = r.OrderedFacts.ToList(); facts[0] = new CandidateBattleOrderedFact(99, facts[0].DirectAttack); return Copy(r, ordered: facts); }
            if (change == "Contribution" || change == "ContributionCount")
            {
                var segments = r.ContributionSegments.ToList(); var s = segments[0];
                if (change == "ContributionCount") segments.RemoveAt(0);
                else segments[0] = new CandidateContributionSegment(s.OperationId, s.SceneRevision, s.FaceId, s.RuleSegment, s.SegmentIndex,
                    s.Actor, s.Target, s.Beneficiary, s.Kind, R(1), s.FactIndex);
                return Copy(r, segments: segments);
            }
            var state = r.AfterSnapshot; var members = state.Members.ToList(); var enemies = state.Enemies.ToList();
            if (change == "AfterHp") members[0] = new BattleMemberState(members[0].CombatantKey, members[0].Member, R(1));
            if (change == "AfterCursor") { var e = enemies[1]; enemies[1] = new BattleEnemyState(e.CombatantKey, e.PairKey, e.Enemy, e.Hp, e.IntentCursor + 1); }
            var random = state.Random;
            if (change == "AfterRandom") random = new BattleRandomSnapshot(new Pcg32StreamState(random.Stream.Initial, random.Stream.Current, random.Stream.WordsConsumed + 1), random.PrdStates);
            return Copy(r, after: new BattleSnapshot(state.Baseline, change == "Revision" ? state.SceneRevision + 1 : state.SceneRevision,
                change == "ActionCount" ? state.EffectiveActionsCompleted + 1 : state.EffectiveActionsCompleted,
                change == "EnemyPhaseCount" ? state.EnemyPhasesCompleted + 1 : state.EnemyPhasesCompleted, state.CurrentFaceIndex,
                change == "Phase" ? BattlePhase.AwaitRescue : state.Phase, change == "AfterBoard" ? new BattleBoardState(state.Board.Face) : state.Board,
                members, enemies, random, state.Contributions));
        }
        private static BattleDamageFact Damage(CandidateCombatFrame d, CandidateCritFact crit, BigInteger? rounded = null)
        { var f = d.DamageFacts[0]; var target = d.BeforeSnapshot.Enemies.Single(e => e.CombatantKey.Equals(f.Target));
            return new BattleDamageFact(d.BeforeSnapshot, d.Action, target, f.Attack, f.Multiplier, f.RawDamage, f.MitigatedDamage,
                rounded ?? f.RoundedDamage, f.HpAfter, f.HpLoss, f.Overflow, f.BlockPrevented, crit); }
        private static CandidateEnemyPhaseFrame EnemyFrame(CandidateEnemyPhaseFrame frame, CandidateCombatFrame direct)
        { return new CandidateEnemyPhaseFrame(direct, frame.EnemyPhaseOrdinal, frame.Members, frame.Enemies, frame.Contributions, frame.OrderedIntents); }
        private static CandidateBattleOperationRecord Copy(CandidateBattleOperationRecord r, BattleSnapshot after = null,
            CandidateCombatFrame direct = null, CandidateEnemyPhaseFrame enemy = null, CandidateStageDecision stage = null,
            IEnumerable<CandidateContributionSegment> segments = null, IEnumerable<CandidateBattleOrderedFact> ordered = null)
        { return new CandidateBattleOperationRecord(r.Kind, r.OccurredAtUnixMilliseconds, r.BeforeSnapshot, after ?? r.AfterSnapshot,
            r.Conditions, direct ?? r.DirectAttack, enemy ?? r.EnemyPhase, stage ?? r.StageDecision, ordered ?? r.OrderedFacts, segments ?? r.ContributionSegments, r.ConsumptionCoverage); }
        private static BattleSnapshot State(BattleSnapshot s, BattleEntryBaseline baseline)
        { return new BattleSnapshot(baseline, s.SceneRevision, s.EffectiveActionsCompleted, s.EnemyPhasesCompleted, s.CurrentFaceIndex,
            s.Phase, s.Board, s.Members, s.Enemies, s.Random, s.Contributions); }

        private static CandidateIsolationRequest IsolationRequest(CandidateBattleHistory h, string operation = null)
        { return new CandidateIsolationRequest { PlayerId = h.Binding.GeneratedForPlayerId, AttemptId = h.Binding.GeneratedForAttemptId,
            ExpectedSceneRevision = h.CurrentRun.CurrentSnapshot.SceneRevision, RightsMode = CandidateIsolationRights.Empty,
            Purpose = operation == null ? CandidateIsolationPurpose.CurrentPlan : CandidateIsolationPurpose.RecordedOperation,
            CurrentConditions = operation == null ? new CandidateBattleConditions { PreferenceRevision = 7, ItemUseEnabled = true } : null,
            RecordedOperationId = operation }; }
        private static CandidateBattleIsolation Recorded(CandidateBattleHistory h, string operation)
        { return Accept(CandidateReplayOperations.BuildIsolationInput(h, IsolationRequest(h, operation), Math())).Input; }
        private static CandidateBattleIsolation Plan(CandidateBattleHistory h, int revision = 7, bool enabled = true)
        { var r = IsolationRequest(h); r.CurrentConditions.PreferenceRevision = revision; r.CurrentConditions.ItemUseEnabled = enabled;
            return Accept(CandidateReplayOperations.BuildIsolationInput(h, r, Math())).Input; }
        private static CandidateReplayStep Step(BattleSnapshot state, string operation, int index = 0, List<FlowPos> route = null)
        { var r = Request(state, operation, index, route); return new CandidateReplayStep { Kind = CandidateBattleOperationKind.Attack,
            OperationId = operation, OccurredAtUnixMilliseconds = 123, Actor = r.Actor, Pair = r.Pair, Route = r.Route }; }
        private static CandidateBattleResult Direct(CandidateBattleRun run, CandidateReplayStep step)
        { return CandidateBattleOperations.EvaluateAttack(run, new CandidateAttackRequest { PlayerId = run.Binding.GeneratedForPlayerId,
            AttemptId = run.Binding.GeneratedForAttemptId, OperationId = step.OperationId, ExpectedSceneRevision = run.CurrentSnapshot.SceneRevision,
            Actor = step.Actor, Pair = step.Pair, Route = step.Route }, new CandidateBattleConditions { PreferenceRevision = 7, ItemUseEnabled = true }, step.OccurredAtUnixMilliseconds, Sampling()); }
        private static CandidateBattleHistory Start(BattleEntryInput input, ulong seed = 42, ulong sequence = 54)
        {
            var prepared = new BattleEntryPreparer().PrepareCandidate(input, Math()); Assert.IsTrue(prepared.IsAccepted, prepared.RejectionCode + " " + prepared.FieldPath);
            var bytes = new byte[48];
            for (var offset = 0; offset < 48; offset += 16) for (var i = 0; i < 8; i++) { bytes[offset + i] = (byte)(seed >> (8 * i)); bytes[offset + 8 + i] = (byte)(sequence >> (8 * i)); }
            var binding = CandidateRandomPreparer.Prepare(prepared.Entry, new CandidateSeedMaterial {
                SourceCapabilityId = "synthetic-014-test", MappingId = CandidateRandomPreparer.SupportedMappingId, Bytes = bytes }, Math());
            Assert.IsTrue(binding.IsAccepted, binding.RejectionCode + " " + binding.FieldPath);
            var run = CandidateBattleOperations.CreateCandidate(binding.Binding, Math()); Assert.IsTrue(run.IsAccepted, run.RejectionCode + " " + run.FieldPath);
            var history = CandidateHistoryOperations.CreateCandidate(run.Run, Math()); Assert.IsTrue(history.IsAccepted, history.RejectionCode + " " + history.FieldPath); return history.Next;
        }
        private static CandidateBattleHistory Attack(CandidateBattleHistory h, string operation, string anchor, int index = 0, List<FlowPos> route = null)
        { var evaluated = Direct(h.CurrentRun, Step(h.CurrentRun.CurrentSnapshot, operation, index, route)); Assert.IsTrue(evaluated.IsAccepted, evaluated.RejectionCode + " " + evaluated.FieldPath);
            var appended = CandidateHistoryOperations.Append(h, evaluated.NextRun, anchor, Math()); Assert.IsTrue(appended.IsAccepted, appended.RejectionCode + " " + appended.FieldPath); return appended.Next; }
        private static CandidateBattleHistory Rollback(CandidateBattleHistory h, string operation, string anchor)
        {
            var range = CandidateHistoryOperations.ReadRange(h, new CandidateHistoryRangeRequest { PlayerId = h.Binding.GeneratedForPlayerId,
                AttemptId = h.Binding.GeneratedForAttemptId, ExpectedSceneRevision = h.CurrentRun.CurrentSnapshot.SceneRevision, HistoryAnchorId = anchor }, Math());
            Assert.IsTrue(range.IsAccepted, range.RejectionCode + " " + range.FieldPath);
            var result = CandidateHistoryOperations.PrepareRollback(h, new CandidateRollbackRequest { PlayerId = h.Binding.GeneratedForPlayerId,
                AttemptId = h.Binding.GeneratedForAttemptId, ExpectedSceneRevision = h.CurrentRun.CurrentSnapshot.SceneRevision, OperationId = operation, HistoryAnchorId = anchor }, range.Range, Math());
            Assert.IsTrue(result.IsAccepted, result.RejectionCode + " " + result.FieldPath); return result.Next;
        }
        private static BattlePairKey Pair(BattleSnapshot state, int index)
        { return BattlePairKey.Create(state.Baseline.Entry.AttemptId, state.Board.Face.FaceId, state.Board.Face.Pairs[index].PairId); }
        private static CandidateAttackRequest Request(BattleSnapshot state, string operation, int index = 0, List<FlowPos> route = null)
        { var pair = state.Board.Face.Pairs[index]; return new CandidateAttackRequest { PlayerId = state.Baseline.Entry.PlayerId, AttemptId = state.Baseline.Entry.AttemptId,
            OperationId = operation, ExpectedSceneRevision = state.SceneRevision, Actor = state.Members[0].CombatantKey, Pair = Pair(state, index),
            Route = route ?? new List<FlowPos> { pair.EndpointA, new FlowPos(1, pair.EndpointA.y), new FlowPos(2, pair.EndpointA.y), pair.EndpointB } }; }
        private static CandidateReplayResult Accept(CandidateReplayResult result)
        { Assert.IsTrue(result.IsAccepted, result.RejectionCode + " " + result.FieldPath); Assert.IsNull(result.RejectionCode); Assert.IsNull(result.FieldPath); Assert.IsFalse(result.CommitEligible); return result; }
        private static void Reject(CandidateReplayResult result, string code, int? index = null)
        { Assert.IsFalse(result.IsAccepted); Assert.AreEqual(code, result.RejectionCode); Assert.AreEqual(index, result.StepIndex); Assert.IsNotEmpty(result.FieldPath);
            Assert.IsNull(result.Input); Assert.IsNull(result.Run); Assert.IsNull(result.ActualRecords); Assert.IsNull(result.FinalReport); Assert.IsNull(result.PlanEvidence);
            Assert.IsNull(result.Outcome); Assert.IsNull(result.Matched); Assert.IsNull(result.FirstDivergence); Assert.IsFalse(result.NormalVictory); Assert.IsFalse(result.CommitEligible); }
        private static string Describe(object value) { return Convert.ToBase64String(CandidateBattleReportFingerprint.Encode(value, Math())); }
        private static ExactMathBudget Math() { return new ExactMathBudget(); }
        private static RandomSamplingBudget Sampling() { return new RandomSamplingBudget(Math()); }
        private static ExactRational R(BigInteger n, int d = 1) { return ExactRational.Create(n, d, Math()); }
        private static void Value(ExactRational r, int n, int d = 1) { Assert.AreEqual(new BigInteger(n), r.Numerator); Assert.AreEqual(new BigInteger(d), r.Denominator); }
        private static void ReadOnly(object value, HashSet<object> seen)
        {
            if (value == null || value is string || value.GetType().IsValueType || !seen.Add(value)) return;
            if (value is IList list) { Assert.IsTrue(list.IsReadOnly); Assert.Throws<NotSupportedException>(() => list.Clear()); }
            if (value is IEnumerable rows) { foreach (var row in rows) ReadOnly(row, seen); return; }
            foreach (var property in value.GetType().GetProperties()) { Assert.IsNull(property.GetSetMethod()); ReadOnly(property.GetValue(value), seen); }
        }
        private static BattleEntryInput Input(int count)
        {
            var context = new CandidateContext { DraftId = "synthetic:014", DraftRevision = 1, ContentFingerprint = "test-only",
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
            if (level == 3) { var enemy = face.Pairs[0].Enemy; enemy.EnemyDefinitionId = "E02"; enemy.Behavior = EnemyBehavior.ChargeHeavy;
                enemy.IntentCycle = new List<EnemyIntentInput> { new EnemyIntentInput { Kind = EnemyIntentKind.Charge, Targeting = EnemyTargeting.FirstLiving,
                    DamageKind = null, DamageCoefficient = null }, Strike(13, 10) };
                enemy.Stats.MaxHp = R(20); enemy.Stats.PhysicalDefense = R(20); }
            routes = fixture.CopySolution().paths.OrderBy(p => p.colorId).Select(p => new List<FlowPos>(p.cells)).ToList(); return input;
        }
        private static void AddSecondFace(BattleEntryInput input)
        { var face = Input(input.Level.Faces[0].Pairs.Count).Level.Faces[0]; face.FaceId = "face:1"; input.Level.Faces.Add(face); }
        private static EnemyIntentInput Strike(int n, int d)
        { return new EnemyIntentInput { Kind = EnemyIntentKind.Strike, Targeting = EnemyTargeting.FirstLiving, DamageKind = EntryDamageKind.Physical, DamageCoefficient = R(n, d) }; }
        private static CandidateContext CopyContext(RuleContext c)
        { return new CandidateContext { DraftId = c.DraftId, DraftRevision = c.DraftRevision, ContentFingerprint = c.ContentFingerprint,
            RuleVersion = c.RuleVersion, NumericContractVersion = c.NumericContractVersion, RandomContractVersion = c.RandomContractVersion, SourceNotes = new List<string>(c.SourceNotes) }; }
        private static StatsInput Stats(int hp, int attack, int defense, int magic)
        { return new StatsInput { MaxHp = R(hp), Attack = R(attack), PhysicalDefense = R(defense), MagicDefense = R(magic), Evasion = R(0), AttackRange = 1 }; }
    }
}
