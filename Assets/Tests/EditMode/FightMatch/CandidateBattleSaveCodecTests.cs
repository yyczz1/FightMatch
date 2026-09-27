using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using NUnit.Framework;
using static FightMatch.Core.Tests.BusinessSaveScenario;

namespace FightMatch.Core.Tests
{
    [TestFixture]
    public class CandidateBattleSaveCodecTests
    {
        [Test]
        public void TwoRollbackGenerationsKeepOriginalRangesReferencesAndRecordedReplay()
        {
            var s = Branches(); var original = s.History; var restored = RoundTrip(Prepare(s)); var h = restored.ActiveHistory;
            Assert.AreSame(h.Binding.Battle.Initial, h.CurrentRun.Baseline.RandomInitials.Battle);
            Assert.AreSame(h.Binding.BaseReward.Initial, h.CurrentRun.Baseline.RandomInitials.BaseReward);
            Assert.AreSame(h.Binding.Bonus.Initial, h.CurrentRun.Baseline.RandomInitials.Bonus);
            Assert.AreSame(h.Binding.Battle.Initial, h.CurrentRun.InitialSnapshot.Random.Stream);
            Assert.AreEqual(4, h.Archive.Count); Assert.AreEqual(2, h.RollbackRecords.Count); Assert.AreEqual(1, h.CurrentRun.Records.Count);
            CollectionAssert.AreEqual(new[] { "a", "b", "c", "d" }, h.Archive.Select(x => x.OperationId));
            CollectionAssert.AreEqual(new[] { "rb1", "rb1", "rb2", null }, h.Archive.Select(x => x.SupersededBy?.OperationId));
            CollectionAssert.AreEqual(new[] { "anchor:d" }, h.EffectiveAnchors);
            Assert.AreSame(h.Archive[0].Record, h.RollbackRecords[0].BeforeRun.Records[0]);
            Assert.AreSame(h.Archive[1].Record, h.RollbackRecords[0].Range.Entries[1].Record);
            Assert.AreSame(h.Archive[2].Record, h.RollbackRecords[1].BeforeRun.Records[0]);
            Assert.AreSame(h.Archive[3].Record, h.CurrentRun.Records[0]);
            foreach (var rollback in h.RollbackRecords)
            {
                Assert.IsTrue(rollback.Range.Entries.All(x => x.SupersededBy == null));
                Assert.AreSame(rollback.BeforeRun.Binding, h.Binding); Assert.AreSame(rollback.RestoredRun.InitialSnapshot, h.CurrentRun.InitialSnapshot);
                Assert.AreSame(rollback.Range.BeforeSnapshot, rollback.Range.Entries[0].Record.BeforeSnapshot);
            }
            for (var i = 0; i < h.Archive.Count; i++)
            {
                Same(original.Archive[i].Record, h.Archive[i].Record); var operation = h.Archive[i].OperationId;
                var record = h.Archive[i].Record; var hit = record.DirectAttack.DamageFacts[0];
                Assert.AreSame(record.BeforeSnapshot.Random.Stream, hit.Crit.StreamBefore);
                Assert.AreSame(record.DirectAttack.Random.Stream, hit.Crit.StreamAfter);
                Assert.AreSame(record.AfterSnapshot.Random, record.DirectAttack.Random);
                var old = Replay(original, operation); var loaded = Replay(h, operation);
                Assert.AreEqual(true, old.Matched); Assert.AreEqual(true, loaded.Matched); Same(old.ActualRecords, loaded.ActualRecords);
            }
            var lookup = CandidateHistoryOperations.FindOperation(h, "rb1", Math()); Assert.IsTrue(lookup.IsAccepted);
            Assert.AreEqual(CandidateHistoryOperationRelation.RollbackRecorded, lookup.Operation.Relation);
            EqualBodies(Encode(Prepare(s)), Encode(restored));
        }

        [Test]
        public void LoadedAndOriginalBranchesHaveIdenticalNextActionRandomPrdAndFinalReport()
        {
            var original = Branches(); var loaded = New(3); loaded.Use(RoundTrip(Prepare(original)));
            var time = (BigInteger.One << 100) + 123;
            original.Attack("next", 0, time); loaded.Attack("next", 0, time);
            Same(original.History.CurrentRun.CurrentSnapshot, loaded.History.CurrentRun.CurrentSnapshot);
            Same(original.History.CurrentRun.Records, loaded.History.CurrentRun.Records);
            Same(original.History.CurrentRun.CurrentSnapshot.Random, loaded.History.CurrentRun.CurrentSnapshot.Random);
            original.Attack("terminal", 1, time + 1); loaded.Attack("terminal", 1, time + 1);
            Same(original.History.CurrentRun.FinalReport, loaded.History.CurrentRun.FinalReport);
            Assert.AreEqual(time + 1, loaded.History.CurrentRun.FinalReport.EndedAtUnixMilliseconds);
            Assert.AreEqual(original.History.CurrentRun.FinalReport.Fingerprint, loaded.History.CurrentRun.FinalReport.Fingerprint);
        }

        [Test]
        public void ExplicitOldRunAndRollbackRootsSurviveRemovalOfTheActiveHistory()
        {
            var s = Branches(); var h = s.History;
            s.RetainedRuns.Add(h.CurrentRun); s.RetainedRuns.Add(h.RollbackRecords[0].BeforeRun);
            s.RetainedRollbacks.AddRange(h.RollbackRecords); s.EndExit();
            var restored = RoundTrip(Prepare(s)); Assert.IsNull(restored.ActiveHistory);
            Assert.IsNull(restored.Progression.ActiveAttempt); Assert.IsNull(restored.Inventory.ActiveCarry);
            Assert.AreEqual(2, restored.RetainedRuns.Count); Assert.AreEqual(2, restored.RetainedRollbacks.Count);
            Assert.AreSame(restored.RetainedRuns[1], restored.RetainedRollbacks[0].BeforeRun);
            Assert.AreSame(restored.RetainedRuns[0].Binding, restored.RetainedRuns[1].Binding);
            Same(h.RollbackRecords[0].BeforeRun.Records, restored.RetainedRuns[1].Records);
            Same(h.RollbackRecords[1].RestoredRun.CurrentSnapshot, restored.RetainedRollbacks[1].RestoredRun.CurrentSnapshot);
            Assert.AreEqual(SaveBindingKind.CandidateDefinition, Encode(restored).SliceDirectory[3].Requirements.Bindings[0].Kind);
        }

        [Test]
        public void CrossFaceBoundaryRestoresCompleteOldFaceRecordsAndContinues()
        {
            var s = New(faces: 2); s.Begin(); s.Attack("face0-a", 0); s.Attack("face0-b", 1);
            Assert.AreEqual(1, s.History.CurrentRun.CurrentSnapshot.CurrentFaceIndex);
            var loaded = New(faces: 2); loaded.Use(RoundTrip(Prepare(s)));
            Assert.AreSame(loaded.History.Binding.Start.Baseline.Entry.Level.Faces[1], loaded.History.CurrentRun.CurrentSnapshot.Board.Face);
            Assert.AreEqual("face0", loaded.History.Archive[1].Record.BeforeSnapshot.Board.Face.FaceId);
            Assert.IsTrue(loaded.History.Archive[1].Record.StageDecision.DidFlipFace); Assert.AreEqual(true, Replay(loaded.History, "face0-b").Matched);
            s.Win(); loaded.Win(); Same(s.History.CurrentRun.FinalReport, loaded.History.CurrentRun.FinalReport);
        }

        [Test]
        public void RescueStateRestoresWithoutHealingAndPublicRollbackCanRestoreThePreActionState()
        {
            var s = New(3, hp: 1); s.Begin(); s.Attack("down", 0); var restored = RoundTrip(Prepare(s));
            Assert.AreEqual(BattlePhase.AwaitRescue, restored.ActiveHistory.CurrentRun.CurrentSnapshot.Phase);
            Same(R(0), restored.ActiveHistory.CurrentRun.CurrentSnapshot.Members[0].Hp);
            Assert.IsTrue(restored.Character.IsReady); Assert.IsEmpty(restored.Character.RecoveryPeriods);
            s.Use(restored); s.Rollback("undo-down", "down"); Assert.AreEqual(BattlePhase.AwaitAction, s.History.CurrentRun.CurrentSnapshot.Phase);
            Same(R(1), s.History.CurrentRun.CurrentSnapshot.Members[0].Hp);
            Assert.AreEqual(true, Replay(RoundTrip(Prepare(s)).ActiveHistory, "down").Matched);
        }

        [Test]
        public void HistoricalParticipantAndConditionRevisionsDoNotBecomeCurrentRevisions()
        {
            var s = New(3); s.Begin(); s.Attack("old", 0); var original = s.History.Archive[0].Record;
            var huge = (BigInteger.One << 90) + 7; var c = s.Character; var p = s.Progression;
            s.Character = new CandidateCharacterState(c.Definition, c.PlayerId, c.CharacterId, c.Level, c.Experience, c.OriginalSlot, huge,
                c.BaseRewards, c.ProcessedEnds, c.RecoveryPeriods);
            s.Inventory = InventoryCopy(s.Inventory, preference: huge);
            s.Progression = new CandidateProgressionState(p.PlayerId, p.Definition, huge, p.OpenFacts, p.FirstClears, p.Challenges);
            s.Rewards = new CandidateRewardState(s.Rewards.PlayerId, huge, s.Rewards.BaseRewards);
            var loaded = RoundTrip(Prepare(s)); Assert.AreEqual(huge, loaded.Character.StateRevision);
            Assert.AreEqual(huge, loaded.Progression.StateRevision); Assert.AreEqual(huge, loaded.Rewards.StateRevision);
            Assert.AreEqual(BigInteger.One, loaded.Progression.ActiveAttempt.Begin.Participant.CharacterRevision);
            Assert.AreEqual(BigInteger.One, loaded.ActiveHistory.Archive[0].Record.Conditions.PreferenceRevision);
            var replay = Replay(loaded.ActiveHistory, "old"); Same(original, replay.ActualRecords[0]);
            Assert.AreEqual(BigInteger.One, replay.ActualRecords[0].Conditions.PreferenceRevision);
            s.Use(loaded); s.Attack("new", 0); Assert.AreEqual(huge, s.History.Archive.Last().Record.Conditions.PreferenceRevision);
            EqualBodies(Encode(Prepare(s)), Encode(RoundTrip(Prepare(s))));
        }

        [Test]
        public void OwnerTextKeepsUtf16CodeUnitsAndDuplicateSourceNotes()
        {
            var s = New(); s.Context.SourceNotes.Add("unicode:\ud800:\udfff:尾");
            var definition = CandidateCharacterGrowth.PrepareDefinition(Growth(s.Context, 100), Math()); Assert.IsTrue(definition.IsAccepted);
            var c = s.Character; s.Character = new CandidateCharacterState(definition.Definition, c.PlayerId, c.CharacterId, c.Level, c.Experience, c.OriginalSlot,
                c.StateRevision, c.BaseRewards, c.ProcessedEnds, c.RecoveryPeriods);
            var original = Encode(Prepare(s)); var restored = RoundTrip(Prepare(s));
            CollectionAssert.AreEqual(s.Context.SourceNotes, restored.Character.Definition.Context.SourceNotes);
            CollectionAssert.AreEqual(s.Context.SourceNotes, original.SliceDirectory[0].Requirements.Bindings[0].SourceNotes);
            EqualBodies(original, Encode(restored));
        }

        [Test]
        public void HugeSceneRevisionAndMaterialCountsPreserveEveryIntegerBit()
        {
            var s = New(3); s.Begin(); var original = s.History.CurrentRun; var state = original.CurrentSnapshot; var huge = (BigInteger.One << 97) + 13;
            var high = new BattleSnapshot(state.Baseline, huge, state.EffectiveActionsCompleted, state.EnemyPhasesCompleted, state.CurrentFaceIndex,
                state.Phase, state.Board, state.Members, state.Enemies, state.Random, state.Contributions);
            var retained = new CandidateBattleRun(original.Binding, original.Baseline, original.InitialSnapshot, high, original.Records, null);
            var next = CandidateBattleOperations.EvaluateAttack(retained, AttackRequest(high, "large-revision", 0, s.Routes[0]),
                new CandidateBattleConditions { PreferenceRevision = 1, ItemUseEnabled = true }, huge + 1, new RandomSamplingBudget(Math()));
            Assert.IsTrue(next.IsAccepted, next.RejectionCode + " " + next.FieldPath); s.RetainedRuns.Add(next.NextRun); s.EndExit();
            var loaded = RoundTrip(Prepare(s)); Assert.AreEqual(huge + 1, loaded.RetainedRuns[0].CurrentSnapshot.SceneRevision);
            Same(next.Record, loaded.RetainedRuns[0].Records[0]);
            var victory = New(); victory.Begin(); victory.Win(); victory.EndVictory(huge);
            var complete = RoundTrip(Prepare(victory));
            Assert.AreEqual(huge, complete.Inventory.Holdings.Single(x => x.ItemId == "candidate:tin").T);
            Assert.AreEqual(huge, complete.Rewards.BaseRewards[0].Materials.Single(x => x.ItemId == "candidate:tin").Amount);
            Assert.AreEqual(huge, complete.Inventory.OrdinaryGrants[0].Items.Single(x => x.ItemId == "candidate:tin").Quantity);
        }

        [Test]
        public void CandidateBusinessDecodeRejectsPlayerSaveEvenWithTheSameFiveBodies()
        {
            var e = Encode(Prepare(New()));
            var wrong = new SaveEnvelope(SavePurpose.PlayerSave, e.PlayerId, e.SaveGeneration, e.CommitId, e.ParentCommitId,
                e.RequiredSliceContracts.ToArray(), e.SliceDirectory.ToArray(), e.CommitIndex.ToArray(), e.RecoveryRequirements,
                e.BodyLength, e.BodySha256.ToArray(), e.Bodies);
            Rejected(CandidateBusinessSaveCodec.Decode(wrong, Budget()), "UnsupportedBinding");
        }

        [Test]
        public void PreparedRootCollectionsAreCopiedAndAllPublishedCollectionsAreReadOnly()
        {
            var s = Branches(); s.RetainedRuns.Add(s.History.CurrentRun); s.RetainedRuns.Add(s.History.CurrentRun);
            var snapshot = Prepare(s); s.RetainedRuns.Clear(); s.RetainedRollbacks.Add(s.History.RollbackRecords[0]);
            Assert.AreEqual(2, snapshot.RetainedRuns.Count); Assert.IsEmpty(snapshot.RetainedRollbacks);
            Assert.Throws<NotSupportedException>(() => ((IList<CandidateBattleRun>)snapshot.RetainedRuns).Clear());
            var restored = RoundTrip(snapshot); Assert.AreSame(restored.ActiveHistory.CurrentRun, restored.RetainedRuns[0]);
            Assert.AreSame(restored.RetainedRuns[0], restored.RetainedRuns[1]);
            Assert.Throws<NotSupportedException>(() => ((IList<CandidateHistoryEntry>)restored.ActiveHistory.Archive).Clear());
            Assert.Throws<NotSupportedException>(() => ((IList<CandidateBattleOperationRecord>)restored.RetainedRuns[0].Records).Clear());
            var tables = new CandidateBattleSaveCodec(Budget()); tables.Collect(restored);
            Assert.AreEqual(1, tables.Baselines.Rows.Count); Assert.AreEqual(1, tables.Bindings.Rows.Count); Assert.AreEqual(4, tables.Records.Rows.Count);
        }

        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)] [TestCase(5)] [TestCase(6)]
        public void MissingAndForgedBranchAuditCannotBePrepared(int mutation)
        {
            var s = Branches(); var h = s.History; var archive = h.Archive.ToList(); var rollbacks = h.RollbackRecords.ToList();
            var anchors = h.EffectiveAnchors.ToList(); var run = h.CurrentRun;
            if (mutation == 0) archive.RemoveAt(0);
            if (mutation == 1) rollbacks.RemoveAt(0);
            if (mutation == 2) archive[0] = new CandidateHistoryEntry(archive[0].HistoryAnchorId, archive[0].Record, archive[2].SupersededBy);
            if (mutation == 3) archive[0] = new CandidateHistoryEntry(archive[0].HistoryAnchorId, archive[0].Record);
            if (mutation == 4) anchors[0] = "anchor:a";
            if (mutation == 5) run = new CandidateBattleRun(run.Binding, run.Baseline, run.InitialSnapshot, run.CurrentSnapshot, new CandidateBattleOperationRecord[0], null);
            if (mutation == 6)
            {
                var r = rollbacks[0]; var range = r.Range;
                rollbacks[0] = new CandidateRollbackRecord(r.OperationId, r.BeforeRun, new CandidateRollbackRange(range.Binding, range.SceneRevision,
                    range.HistoryAnchorId, range.OperationId, range.BeforeSnapshot, range.Entries.Reverse()), r.RestoredRun);
            }
            s.History = new CandidateBattleHistory(run, archive, anchors, rollbacks); Rejected(CandidateBusinessSaveCodec.Prepare(s.Input(), Budget()));
        }

        [TestCase("conditions")] [TestCase("contribution")] [TestCase("ordered")] [TestCase("after")] [TestCase("stage")] [TestCase("raw")] [TestCase("mitigated")]
        public void IncompleteOrInconsistentOperationFragmentsAreRejected(string mutation)
        {
            var s = New(3); s.Begin(); s.Attack("first", 0); var run = s.History.CurrentRun; var r = run.Records[0];
            var direct = r.DirectAttack; var enemy = r.EnemyPhase; var stage = r.StageDecision; var facts = r.OrderedFacts.ToList();
            if (mutation == "ordered") facts.RemoveAt(0);
            if (mutation == "stage") stage = new CandidateStageDecision(stage.BeforeSnapshot, stage.Source, stage.FinalHp, stage.Board,
                stage.NextFaceIndex, BattlePhase.AwaitRescue, stage.NextFace, stage.OrderedFacts);
            if (mutation == "raw" || mutation == "mitigated")
            {
                var hit = direct.DamageFacts[0]; var target = direct.BeforeSnapshot.Enemies.Single(x => x.CombatantKey.Equals(hit.Target));
                var changed = new BattleDamageFact(direct.BeforeSnapshot, direct.Action, target, hit.Attack, hit.Multiplier,
                    mutation == "raw" ? R(1) : hit.RawDamage, mutation == "mitigated" ? R(1) : hit.MitigatedDamage, hit.RoundedDamage,
                    hit.HpAfter, hit.HpLoss, hit.Overflow, hit.BlockPrevented, hit.Crit);
                direct = new CandidateCombatFrame(direct.Binding, direct.BeforeSnapshot, direct.Action, direct.Enemies, direct.Random, direct.Contributions, changed);
                enemy = new CandidateEnemyPhaseFrame(direct, enemy.EnemyPhaseOrdinal, enemy.Members, enemy.Enemies, enemy.Contributions, enemy.OrderedIntents);
                facts[0] = new CandidateBattleOrderedFact(0, changed);
            }
            var altered = new CandidateBattleOperationRecord(r.Kind, r.OccurredAtUnixMilliseconds, r.BeforeSnapshot,
                mutation == "after" ? r.BeforeSnapshot : r.AfterSnapshot, mutation == "conditions" ? null : r.Conditions, direct, enemy, stage,
                facts, mutation == "contribution" ? new CandidateContributionSegment[0] : r.ContributionSegments, r.ConsumptionCoverage);
            s.History = new CandidateBattleHistory(new CandidateBattleRun(run.Binding, run.Baseline, run.InitialSnapshot, altered.AfterSnapshot, new[] { altered }, null),
                new[] { new CandidateHistoryEntry("anchor:first", altered) }, new[] { "anchor:first" }, new CandidateRollbackRecord[0]);
            Rejected(CandidateBusinessSaveCodec.Prepare(s.Input(), Budget()));
        }

        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)]
        public void WpsRequiresTheExactCompleteOriginalFinalReport(int mutation)
        {
            var s = New(); s.Begin(); s.Win(); var run = s.History.CurrentRun; var r = run.FinalReport;
            var report = mutation == 0 ? null : new CandidateFinalAttemptReport(r.Binding, r.Baseline, r.InitialSnapshot,
                mutation == 1 ? r.Operations.Take(1) : r.Operations, r.FinalSnapshot, mutation == 2 ? new CandidateContributionSegment[0] : r.Contributions,
                r.Outcome, r.EndedAtUnixMilliseconds, mutation == 3 ? "absent" : r.TerminalOperationId, r.ConsumptionCoverage,
                r.WholeLevelInitialEnemyHp, mutation == 4 ? new string('0', 64) : r.Fingerprint);
            s.History = new CandidateBattleHistory(new CandidateBattleRun(run.Binding, run.Baseline, run.InitialSnapshot, run.CurrentSnapshot, run.Records, report),
                s.History.Archive, s.History.EffectiveAnchors, s.History.RollbackRecords);
            Rejected(CandidateBusinessSaveCodec.Prepare(s.Input(), Budget()));
        }

        [TestCase("reference")] [TestCase("nullable")] [TestCase("table-count")] [TestCase("missing-row")]
        public void IndependentlyMutatedReferencesCountsAndNullableTagsRejectAtomically(string mutation)
        {
            var s = New(); s.Begin(); var envelope = Encode(Prepare(s)); var bodies = envelope.Bodies.Select(x => (byte[])x.Clone()).ToArray();
            var body = bodies[3]; var historyFlag = body.Length - 25;
            Assert.AreEqual(1, body[historyFlag]); // Empty active history tail: flag, RunRef, three counts, two root counts.
            if (mutation == "reference") PutU32(body, historyFlag + 1, uint.MaxValue);
            if (mutation == "nullable") body[historyFlag] = 2;
            if (mutation == "table-count") PutU32(body, 13 + 4 + s.Character.PlayerId.Length * 2, uint.MaxValue);
            if (mutation == "missing-row") PutU32(body, 13 + 4 + s.Character.PlayerId.Length * 2, 0);
            Rejected(CandidateBusinessSaveCodec.Decode(Repack(envelope, bodies), Budget()));
        }

        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)]
        public void PayloadCorruptionStillFailsAfterTheEnvelopeHashesAreRebuilt(int mutation)
        {
            var s = New(); s.Begin(); s.Win(); s.EndVictory(); var envelope = Encode(Prepare(s)); var bodies = envelope.Bodies.Select(x => (byte[])x.Clone()).ToArray();
            if (mutation == 0) Replace(bodies[3], Encoding.Unicode.GetBytes(s.Rewards.BaseRewards[0].FinalReportFingerprint), 0, (byte)'z');
            if (mutation == 1) Replace(bodies[3], Encoding.Unicode.GetBytes("sc01-pcg32-le128-v1"), 0, (byte)'x');
            if (mutation == 2) Replace(bodies[3], Encoding.Unicode.GetBytes("candidate-r1"), 0, (byte)'x');
            if (mutation == 3) Replace(bodies[4], Encoding.ASCII.GetBytes("1/2"), 2, (byte)'0');
            if (mutation == 4) Replace(bodies[0], Encoding.ASCII.GetBytes("180000/1"), 0, (byte)'0');
            Rejected(CandidateBusinessSaveCodec.Decode(Repack(envelope, bodies), Budget()));
        }

        [TestCase(false)] [TestCase(true)]
        public void ExactMathFailureNearTheEndHasNoPartialResultAndCanRetry(bool decode)
        {
            var s = Branches(); var snapshot = Prepare(s); var envelope = Encode(snapshot); var measured = Math();
            if (decode) Accept(CandidateBusinessSaveCodec.Decode(envelope, new SaveCodecBudget(measured)));
            else Accept(CandidateBusinessSaveCodec.Prepare(s.Input(), new SaveCodecBudget(measured)));
            Assert.Greater(measured.PrimitiveStepsUsed, 10); var tight = new SaveCodecBudget(new ExactMathBudget(maxPrimitiveSteps: (int)measured.PrimitiveStepsUsed - 1));
            Rejected(decode ? CandidateBusinessSaveCodec.Decode(envelope, tight) : CandidateBusinessSaveCodec.Prepare(s.Input(), tight), "Limit");
            EqualBodies(envelope, Encode(RoundTrip(snapshot)));
        }

        [Test]
        public void BodyBudgetAppliesToAllFiveSlicesTogether()
        {
            var snapshot = Prepare(New()); var envelope = Encode(snapshot); var sum = envelope.Bodies.Sum(x => x.Length);
            var budget = new SaveCodecBudget(Math(), maxEnvelopeBytes: (ulong)(sum - 1));
            Assert.IsTrue(envelope.Bodies.All(x => x.Length < sum - 1));
            Rejected(CandidateBusinessSaveCodec.Decode(envelope, budget), "Limit");
            Rejected(CandidateBusinessSaveCodec.Encode(snapshot, Header(), new SaveCodecBudget(Math(), maxEnvelopeBytes: (ulong)(sum - 1))), "Limit");
            EqualBodies(envelope, Encode(RoundTrip(snapshot)));
        }

        [Test]
        public void SameValueDistinctRetainedBaselinesRoundTripAfterNormalExit()
        {
            var s = RetainedTwinRuns(); var restored = RoundTrip(Prepare(s));
            Assert.IsNull(restored.ActiveHistory); Assert.IsNull(restored.Progression.ActiveAttempt); Assert.IsNull(restored.Inventory.ActiveCarry);
            Assert.AreEqual(2, restored.RetainedRuns.Count); Assert.IsEmpty(restored.RetainedRollbacks);
            var a = restored.RetainedRuns[0]; var b = restored.RetainedRuns[1];
            Assert.AreNotSame(a.Baseline, b.Baseline); Assert.AreNotSame(a.Binding, b.Binding); Assert.AreNotSame(a.InitialSnapshot, b.InitialSnapshot);
            Same(a.Baseline, b.Baseline); Same(a.InitialSnapshot, b.InitialSnapshot);
            for (var i = 0; i < restored.RetainedRuns.Count; i++)
            {
                var run = restored.RetainedRuns[i];
                Assert.AreSame(run.Baseline, run.Binding.Start.Baseline); Assert.AreSame(run.Baseline, run.CurrentSnapshot.Baseline);
                Assert.AreSame(run.InitialSnapshot, run.CurrentSnapshot); Same(s.RetainedRuns[i].CurrentSnapshot, run.CurrentSnapshot);
            }
        }

        [Test]
        public void PrepareRejectsRetainedCurrentSnapshotFromAnotherEqualBaseline()
        {
            var s = RetainedTwinRuns(); var a = s.RetainedRuns[0]; var b = s.RetainedRuns[1];
            var mixed = new CandidateBattleRun(a.Binding, a.Baseline, a.InitialSnapshot, b.CurrentSnapshot, a.Records, a.FinalReport);
            s.RetainedRuns[0] = mixed;
            var result = CandidateBusinessSaveCodec.Prepare(s.Input(), Budget());
            Assert.AreSame(mixed, s.RetainedRuns[0]); Assert.AreSame(b, s.RetainedRuns[1]);
            Assert.AreSame(a.Baseline, mixed.Baseline); Assert.AreSame(a.InitialSnapshot, mixed.InitialSnapshot);
            Assert.AreSame(b.CurrentSnapshot, mixed.CurrentSnapshot); Assert.AreNotSame(mixed.Baseline, mixed.CurrentSnapshot.Baseline);
            if (result.IsAccepted) Assert.AreSame(mixed, result.Value.RetainedRuns[0]);
            Assert.IsFalse(result.IsAccepted, "F1: Prepare accepted a retained Run whose CurrentSnapshot belongs to another equal Baseline.");
            Assert.IsNull(result.Value); Assert.AreEqual("InconsistentBinding", result.RejectionCode);
            Assert.AreEqual("M06.Run.CurrentSnapshot.Baseline", result.FieldPath);
        }

        [Test]
        public void DecodeRejectsValidSnapshotReferenceToAnotherEqualBaseline()
        {
            var s = RetainedTwinRuns(); var envelope = Encode(Prepare(s)); var bodies = envelope.Bodies.Select(x => (byte[])x.Clone()).ToArray();
            var body = bodies[3];
            // Two empty Runs, no active History, two retained Run refs, no retained rollback refs.
            var tail = Hex("02000000" + new string('0', 40) + "ffffffff" +
                "0100000001000000010000000100000000000000ffffffff" + "00" + "02000000000000000100000000000000");
            Assert.AreEqual(69, tail.Length); CollectionAssert.AreEqual(tail, body.Skip(body.Length - tail.Length));
            var currentSnapshotRef = body.Length - tail.Length + 4 + 12;
            Assert.AreEqual(0, body[currentSnapshotRef]); PutU32(body, currentSnapshotRef, 1);
            Assert.AreEqual(1, body[currentSnapshotRef]);
            CollectionAssert.AreEqual(new[] { currentSnapshotRef }, Enumerable.Range(0, body.Length).Where(i => body[i] != envelope.Bodies[3][i]));
            for (var i = 0; i < bodies.Length; i++) if (i != 3) CollectionAssert.AreEqual(envelope.Bodies[i], bodies[i]);
            var repacked = Repack(envelope, bodies); SaveEnvelope checkedEnvelope;
            using (var stream = new System.IO.MemoryStream())
            {
                var descriptor = Accept(SaveEnvelopeCodec.Write(stream, repacked, Budget())); stream.Position = 0;
                checkedEnvelope = Accept(SaveEnvelopeCodec.Read(stream, descriptor, Budget()));
            }
            var result = CandidateBusinessSaveCodec.Decode(checkedEnvelope, Budget());
            if (result.IsAccepted)
            {
                var a = result.Value.RetainedRuns[0]; var b = result.Value.RetainedRuns[1];
                Assert.AreSame(b.InitialSnapshot, a.CurrentSnapshot); Assert.AreNotSame(a.Baseline, a.CurrentSnapshot.Baseline);
                Same(a.Baseline, b.Baseline);
            }
            Assert.IsFalse(result.IsAccepted, "F1: Decode accepted a valid table reference to another equal Baseline's Snapshot.");
            Assert.IsNull(result.Value); Assert.AreEqual("InconsistentBinding", result.RejectionCode);
            Assert.AreEqual("M06.Run.CurrentSnapshot.Baseline", result.FieldPath);
        }

        private static BusinessSaveScenario RetainedTwinRuns()
        {
            var s = New(3); s.Begin(); var a = s.History.CurrentRun;
            var bytes = new byte[48]; for (var offset = 0; offset < 48; offset += 16) { bytes[offset] = 42; bytes[offset + 8] = 54; }
            var bound = CandidateRandomPreparer.Prepare(a.Baseline.Entry, new CandidateSeedMaterial { Bytes = bytes,
                SourceCapabilityId = a.Binding.SourceCapabilityId, MappingId = a.Binding.MappingId }, Math());
            Assert.IsTrue(bound.IsAccepted, bound.RejectionCode + " " + bound.FieldPath);
            var created = CandidateBattleOperations.CreateCandidate(bound.Binding, Math()); Assert.IsTrue(created.IsAccepted, created.FieldPath);
            var b = created.Run;
            Assert.AreNotSame(a.Baseline, b.Baseline); Assert.AreNotSame(a.Binding, b.Binding); Assert.AreNotSame(a.InitialSnapshot, b.InitialSnapshot);
            Same(a.Baseline, b.Baseline); Same(a.InitialSnapshot, b.InitialSnapshot);
            s.RetainedRuns.Add(a); s.RetainedRuns.Add(b); s.EndExit();
            Assert.IsNull(s.History); Assert.IsNull(s.Progression.ActiveAttempt); Assert.IsNull(s.Inventory.ActiveCarry);
            return s;
        }

        private static BusinessSaveScenario Branches()
        {
            var s = New(3); s.Begin(); s.Attack("a", 0); s.Attack("b", 0); s.Rollback("rb1", "a");
            s.Attack("c", 0); s.Rollback("rb2", "c"); s.Attack("d", 0); return s;
        }
        private static CandidateReplayResult Replay(CandidateBattleHistory history, string operation)
        {
            var isolated = CandidateReplayOperations.BuildIsolationInput(history, new CandidateIsolationRequest { PlayerId = history.Binding.GeneratedForPlayerId,
                AttemptId = history.Binding.GeneratedForAttemptId, ExpectedSceneRevision = history.CurrentRun.CurrentSnapshot.SceneRevision,
                Purpose = CandidateIsolationPurpose.RecordedOperation, RightsMode = CandidateIsolationRights.Empty, RecordedOperationId = operation }, Math());
            Assert.IsTrue(isolated.IsAccepted, isolated.RejectionCode + " " + isolated.FieldPath);
            var replay = CandidateReplayOperations.ReplayRecorded(isolated.Input, new RandomSamplingBudget(Math()));
            Assert.IsTrue(replay.IsAccepted, replay.RejectionCode + " " + replay.FieldPath); Assert.AreEqual(true, replay.Matched); return replay;
        }
        private static void PutU32(byte[] bytes, int index, uint value)
        { for (var i = 0; i < 4; i++) bytes[index + i] = (byte)(value >> (8 * i)); }
        private static void Replace(byte[] body, byte[] needle, int offset, byte value)
        {
            for (var i = 0; i <= body.Length - needle.Length; i++)
            {
                var equal = true; for (var j = 0; j < needle.Length; j++) if (body[i + j] != needle[j]) { equal = false; break; }
                if (!equal) continue; body[i + offset] = value; return;
            }
            Assert.Fail("Independent corruption target was absent.");
        }
    }
}
