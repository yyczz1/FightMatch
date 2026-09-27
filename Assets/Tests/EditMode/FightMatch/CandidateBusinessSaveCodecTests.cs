using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using FlowPuzzle.Core;
using NUnit.Framework;

namespace FightMatch.Core.Tests
{
    [TestFixture]
    public class CandidateBusinessSaveCodecTests
    {
        [Test]
        public void FreshPublicOwnersRoundTripThroughTheActualEnvelope()
        {
            var s = BusinessSaveScenario.New(); var input = s.Input(); var snapshot = BusinessSaveScenario.Accept(CandidateBusinessSaveCodec.Prepare(input, BusinessSaveScenario.Budget()));
            Assert.IsFalse(snapshot.CommitEligible); Assert.IsNull(snapshot.ActiveHistory);
            var encoded = BusinessSaveScenario.Encode(snapshot); var restored = BusinessSaveScenario.RoundTrip(snapshot);
            CollectionAssert.AreEqual(new[] { "fm.m03.character", "fm.m04.inventory", "fm.m05.progression", "fm.m06.battle", "fm.m07.rewards" }, encoded.RequiredSliceContracts.Select(x => x.SliceId));
            CollectionAssert.AreEqual(new[] { "M03", "M04", "M05", "M06", "M07" }, encoded.RequiredSliceContracts.Select(x => x.OwnerId));
            Assert.IsTrue(encoded.RequiredSliceContracts.All(x => x.SchemaVersion == 1));
            Assert.AreEqual(SavePurpose.CandidateValidation, encoded.Purpose); Assert.AreEqual(input.PlayerId, restored.PlayerId);
            Assert.AreEqual(input.Character.Experience, restored.Character.Experience); Assert.AreEqual(input.Character.Level, restored.Character.Level);
            Assert.AreEqual(input.Progression.OpenFacts.Count, restored.Progression.OpenFacts.Count); Assert.IsEmpty(restored.Inventory.OrdinaryGrants);
            Assert.IsEmpty(restored.Rewards.BaseRewards); Assert.IsEmpty(restored.RetainedRuns); Assert.IsFalse(restored.CommitEligible);
            Assert.AreEqual(SaveBindingKind.CandidateContent, encoded.SliceDirectory[0].Requirements.Bindings[0].Kind);
            Assert.AreEqual(SaveBindingKind.CandidateDefinition, encoded.SliceDirectory[2].Requirements.Bindings[0].Kind);
            BusinessSaveScenario.EqualBodies(encoded, BusinessSaveScenario.Encode(restored));
        }

        [Test]
        public void EmptyBattleAndRewardSlicesMatchIndependentFixedBytes()
        {
            var envelope = BusinessSaveScenario.Encode(BusinessSaveScenario.Prepare(BusinessSaveScenario.New()));
            // Literal UTF-16LE "player:015b"; six u32 table counts, explicit null history, two empty root lists.
            const string player = "0b00000070006c0061007900650072003a003000310035006200";
            var battle = BusinessSaveScenario.Hex("464d42495a3030310100000006" + player + new string('0', 66));
            var rewards = BusinessSaveScenario.Hex("464d42495a3030310100000007" + player + player + "010000003100000000");
            Assert.AreEqual(72, battle.Length); CollectionAssert.AreEqual(battle, envelope.Bodies[3]);
            CollectionAssert.AreEqual(rewards, envelope.Bodies[4]); Assert.IsEmpty(envelope.SliceDirectory[3].Requirements.Bindings);
            var bodies = envelope.Bodies.Select(x => (byte[])x.Clone()).ToArray(); bodies[3] = battle; bodies[4] = rewards;
            var restored = BusinessSaveScenario.Accept(CandidateBusinessSaveCodec.Decode(BusinessSaveScenario.Repack(envelope, bodies), BusinessSaveScenario.Budget()));
            Assert.IsNull(restored.ActiveHistory); Assert.IsEmpty(restored.Rewards.BaseRewards);
        }

        [TestCase(1, 1, 26)] [TestCase(1, 4, 14)] [TestCase(3, 1, 31)]
        public void NormalEndingRetainsTheOriginalScoreAndEveryReceiptAndRetriesAddZero(int level, int entryLevel, int amount)
        {
            var s = BusinessSaveScenario.New(level, entryLevel: entryLevel); s.Begin(); s.Win(); s.EndVictory();
            var original = s.Rewards.BaseRewards[0]; Assert.AreEqual(new BigInteger(amount), original.Experience[0].Amount);
            var restored = BusinessSaveScenario.RoundTrip(BusinessSaveScenario.Prepare(s)); var reward = restored.Rewards.BaseRewards[0];
            Assert.IsNull(restored.ActiveHistory); Assert.AreSame(restored.Progression.Challenges[0].Attempts[0].End, reward.Ending);
            Assert.AreSame(reward.Ending, restored.Progression.FirstClears[0].End);
            Assert.AreSame(restored.Progression.FirstClears[0], restored.Progression.OpenFacts.Last().SourceClear);
            Assert.AreSame(reward.Report.Binding.Start.Baseline, reward.Report.Baseline); Assert.AreSame(reward.Report.Binding.Start.Snapshot, reward.Report.InitialSnapshot);
            Assert.AreSame(restored.Inventory.OrdinaryGrants[0], restored.Inventory.Ends[0].RewardReceipt);
            Assert.AreSame(reward.Report.Binding.BaseReward, reward.Random.Domain); Assert.AreSame(reward.Random.Before, reward.Random.After); Assert.IsEmpty(reward.Random.Words);
            BusinessSaveScenario.Same(original.Report, reward.Report); BusinessSaveScenario.Same(original.Experience[0].Score.LowerBound, reward.Experience[0].Score.LowerBound);
            BusinessSaveScenario.Same(original.Experience[0].Score.UpperBound, reward.Experience[0].Score.UpperBound); Assert.AreEqual(original.Experience[0].Score.TermsUsed, reward.Experience[0].Score.TermsUsed);
            Assert.AreEqual(new BigInteger(entryLevel), reward.Experience[0].EntryLevel);
            var xp = CandidateCharacterGrowth.ApplyBaseReward(restored.Character, BusinessSaveScenario.Experience(reward), 1, BusinessSaveScenario.Math());
            Assert.IsTrue(xp.IsAccepted, xp.FieldPath); Assert.AreSame(restored.Character, xp.Next); Assert.AreEqual(BigInteger.Zero, xp.ExperienceAdded);
            var inventory = CandidateInventory.End(restored.Inventory, BusinessSaveScenario.InventoryEnd(restored.Inventory.Ends[0]), 1, BusinessSaveScenario.Math());
            Assert.IsTrue(inventory.IsAccepted, inventory.FieldPath); Assert.AreSame(restored.Inventory, inventory.Next); Assert.IsEmpty(inventory.GrantedItems);
            var character = CandidateCharacterEnd.Propose(restored.Character, BusinessSaveScenario.CharacterEnd(restored.Character.ProcessedEnds[0]), 1, BusinessSaveScenario.Math());
            Assert.IsTrue(character.IsAccepted, character.FieldPath); Assert.AreSame(restored.Character, character.Next);
            var progression = CandidateProgression.EndAttempt(restored.Progression, BusinessSaveScenario.ProgressionEnd(reward.Ending), 1, BusinessSaveScenario.Math());
            Assert.IsTrue(progression.IsAccepted, progression.FieldPath); Assert.AreSame(restored.Progression, progression.Next);
            using (var scope = new ExactEvaluationScope(new ExactEvaluationBudget(BusinessSaveScenario.Math(), 0, 0)))
            {
                var fixedAgain = CandidateBaseRewards.FixNormal(restored.Rewards, reward.Report, reward.Definition, reward.Ending, BusinessSaveScenario.RewardRequest(reward, 1), scope);
                Assert.IsTrue(fixedAgain.IsAccepted, fixedAgain.FieldPath); Assert.AreSame(restored.Rewards, fixedAgain.Next); Assert.AreSame(reward, fixedAgain.BaseReward);
                Assert.AreEqual(0, scope.Budget.LogTermsUsed);
            }
        }

        [Test]
        public void WpsIsASeparateRecoverableSubmissionBeforeAnyEndOrReward()
        {
            var s = BusinessSaveScenario.New(); s.Begin(); s.Win(); var restored = BusinessSaveScenario.RoundTrip(BusinessSaveScenario.Prepare(s));
            Assert.AreEqual(BattlePhase.WonPendingSettlement, restored.ActiveHistory.CurrentRun.CurrentSnapshot.Phase);
            Assert.IsNotNull(restored.ActiveHistory.CurrentRun.FinalReport); Assert.IsNotNull(restored.Progression.ActiveAttempt); Assert.IsNotNull(restored.Inventory.ActiveCarry);
            Assert.IsEmpty(restored.Rewards.BaseRewards); Assert.IsEmpty(restored.Character.ProcessedEnds); Assert.IsEmpty(restored.Inventory.Ends);
            s.Use(restored); s.EndVictory(); var completed = BusinessSaveScenario.RoundTrip(BusinessSaveScenario.Prepare(s));
            Assert.IsNull(completed.ActiveHistory); Assert.AreEqual(1, completed.Rewards.BaseRewards.Count);
        }

        [Test]
        public void RecoveryKeepsOldDefinitionAndExactClockDomainWithoutAdvancingTime()
        {
            var s = BusinessSaveScenario.New(3, hp: 1); s.Begin(); s.Attack("down", 0);
            Assert.AreEqual(BattlePhase.AwaitRescue, s.History.CurrentRun.CurrentSnapshot.Phase); s.EndExit(true);
            var originalPeriod = s.Character.RecoveryPeriods[0]; var big = (BigInteger.One << 70) + 7;
            var advance = CandidateRecoveryClock.Advance(s.Character, originalPeriod.RecoveryId, new CandidateTimeSample { WallUtcMilliseconds = big,
                ObservedAtUtcMilliseconds = big + 1, MonotonicElapsedMilliseconds = BusinessSaveScenario.R(1, 3), MonotonicScopeId = "clock-original",
                Source = "test-clock", Trust = CandidateTimeTrust.DeviceUntrusted, Anomaly = CandidateTimeAnomaly.DomainChanged }, s.Character.StateRevision, BusinessSaveScenario.Math());
            Assert.IsTrue(advance.IsAccepted, advance.FieldPath); s.Character = advance.Next;
            var input = BusinessSaveScenario.Growth(s.Context, 1); input.Context = BusinessFields.ContextInput(s.Character.Definition.Context); input.Context.DraftRevision = big; input.Context.ContentFingerprint = "new-content";
            var newer = CandidateCharacterGrowth.PrepareDefinition(input, BusinessSaveScenario.Math()); Assert.IsTrue(newer.IsAccepted);
            var c = s.Character; s.Character = new CandidateCharacterState(newer.Definition, c.PlayerId, c.CharacterId, c.Level, c.Experience, c.OriginalSlot, big, c.BaseRewards, c.ProcessedEnds, c.RecoveryPeriods);
            var encoded = BusinessSaveScenario.Encode(BusinessSaveScenario.Prepare(s)); var restored = BusinessSaveScenario.RoundTrip(BusinessSaveScenario.Prepare(s));
            var period = restored.Character.RecoveryPeriods[0]; Assert.AreSame(restored.Character.ProcessedEnds[0], period.EndReceipt); Assert.AreNotSame(restored.Character.Definition, period.Definition);
            BusinessSaveScenario.Same(BusinessSaveScenario.R(1, 3), period.Elapsed); Assert.IsFalse(period.IsCompleted); Assert.AreEqual(big, restored.Character.StateRevision);
            Assert.AreEqual(big, period.LastAcceptedSample.WallUtcMilliseconds); Assert.AreEqual("clock-original", period.LastAcceptedSample.MonotonicScopeId);
            Assert.AreEqual(CandidateTimeAnomaly.DomainChanged, period.Anomaly); Assert.AreEqual(new BigInteger(1), period.Definition.Context.DraftRevision);
            CollectionAssert.AreEqual(new[] { big, BigInteger.One }, encoded.SliceDirectory[0].Requirements.Bindings.Select(x => x.DraftRevision.Value));
            var missingOld = BusinessSaveScenario.Slices(encoded); var requirements = missingOld[0].Requirements;
            missingOld[0].Requirements = new SaveRequirements(requirements.Bindings.Take(1).ToArray(), requirements.RuleVersions,
                requirements.NumericContractVersions, requirements.RandomContractVersions, requirements.FeatureIds);
            BusinessSaveScenario.Rejected(CandidateBusinessSaveCodec.Decode(BusinessSaveScenario.Repack(encoded, missingOld), BusinessSaveScenario.Budget()), "InconsistentBinding");
            var same = CandidateRecoveryClock.Advance(restored.Character, period.RecoveryId, BusinessSaveScenario.Time(period.LastAcceptedSample), restored.Character.StateRevision, BusinessSaveScenario.Math());
            Assert.IsTrue(same.IsAccepted); Assert.AreSame(restored.Character, same.Next);
        }

        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)] [TestCase(5)] [TestCase(6)] [TestCase(7)]
        public void MissingOwnerOrExplicitRootListsNeverBecomeAnEmptySlice(int field)
        {
            var s = BusinessSaveScenario.New(); var input = new CandidateBusinessInput(field == 0 ? null : s.Character.PlayerId,
                field == 1 ? null : s.Character, field == 2 ? null : s.Inventory, field == 3 ? null : s.Progression, field == 4 ? null : s.Rewards,
                null, field == 5 ? null : new CandidateBattleRun[0], field == 6 ? null : new CandidateRollbackRecord[0]);
            if (field == 7) { s.Begin(); input = new CandidateBusinessInput(s.Character.PlayerId, s.Character, s.Inventory, s.Progression, s.Rewards, null, new CandidateBattleRun[0], new CandidateRollbackRecord[0]); }
            BusinessSaveScenario.Rejected(CandidateBusinessSaveCodec.Prepare(input, BusinessSaveScenario.Budget()));
        }

        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)] [TestCase(5)] [TestCase(6)] [TestCase(7)]
        [TestCase(8)] [TestCase(9)] [TestCase(10)] [TestCase(11)] [TestCase(12)] [TestCase(13)] [TestCase(14)] [TestCase(15)]
        public void PartialOrConflictingNormalSettlementIsRejected(int mutation)
        {
            var s = BusinessSaveScenario.New(); s.Begin(); s.Win(); s.EndVictory(); var c = s.Character; var i = s.Inventory; var p = s.Progression; var r = s.Rewards;
            if (mutation == 0) s.Character = BusinessSaveScenario.CharacterCopy(c, rewards: new CandidateBaseExperienceReceipt[0]);
            if (mutation == 1) s.Character = BusinessSaveScenario.CharacterCopy(c, ends: new CandidateCharacterEndReceipt[0]);
            if (mutation == 2) s.Inventory = BusinessSaveScenario.InventoryCopy(i, grants: new CandidateOrdinaryGrantReceipt[0]);
            if (mutation == 3) s.Inventory = BusinessSaveScenario.InventoryCopy(i, ends: new CandidateInventoryEndReceipt[0]);
            if (mutation == 4) s.Rewards = new CandidateRewardState(r.PlayerId, r.StateRevision, new CandidateFixedBaseReward[0]);
            if (mutation == 5) s.Character = BusinessSaveScenario.CharacterCopy(c, rewards: new[] { new CandidateBaseExperienceReceipt(new CandidateBaseExperience {
                PlayerId = c.PlayerId, CharacterId = c.CharacterId, AttemptId = c.BaseRewards[0].AttemptId, SettlementId = c.BaseRewards[0].SettlementId, Amount = c.BaseRewards[0].Amount + 1 }, c.BaseRewards[0].Context) });
            if (mutation == 6) s.Inventory = BusinessSaveScenario.InventoryCopy(i, holdings: i.Holdings.Select(x => new CandidateInventoryHolding(x.ItemId, x.T + 1)));
            if (mutation == 7) s.Progression = new CandidateProgressionState(p.PlayerId, p.Definition, p.StateRevision, p.OpenFacts, new CandidateProgressionFirstClear[0], p.Challenges);
            if (mutation == 8) s.Progression = new CandidateProgressionState(p.PlayerId, p.Definition, p.StateRevision, p.OpenFacts.Take(1), p.FirstClears, p.Challenges);
            if (mutation == 9) s.Rewards = new CandidateRewardState("another-player", r.StateRevision, r.BaseRewards);
            if (mutation == 10) s.Character = BusinessSaveScenario.CharacterCopy(c, rewards: c.BaseRewards.Concat(c.BaseRewards));
            if (mutation == 11) s.Inventory = BusinessSaveScenario.InventoryCopy(i, ends: i.Ends.Concat(i.Ends));
            if (mutation == 12) s.Rewards = new CandidateRewardState(r.PlayerId, r.StateRevision, r.BaseRewards.Concat(r.BaseRewards));
            if (mutation == 13 || mutation == 14)
            {
                var a = p.Challenges[0].Attempts[0]; var b = a.Begin; var old = b.Participant;
                var participant = new CandidateProgressionParticipant(old.CharacterId, old.ClassId, old.ClassKind, mutation == 13 ? 0 : c.StateRevision + 1, old.OriginalSlot);
                var begin = new CandidateProgressionBeginReceipt(b.PlayerId, b.Level, b.Context, b.ChallengeId, b.AttemptId, b.EntryBaselineId, participant);
                var end = new CandidateProgressionEndReceipt(begin, BusinessSaveScenario.ProgressionEnd(a.End), a.End.IsFirstClear);
                s.Progression = new CandidateProgressionState(p.PlayerId, p.Definition, p.StateRevision, p.OpenFacts, p.FirstClears,
                    new[] { new CandidateProgressionChallenge(b.ChallengeId, b.Level, new[] { new CandidateProgressionAttempt(begin, end) }, end) });
            }
            if (mutation == 15)
            {
                var old = r.BaseRewards[0]; var e = old.Experience[0]; var member = old.Report.Baseline.Entry.ReadyParticipants[0];
                var wrong = new CandidateRewardExperience(member, old.Report.FinalSnapshot.Contributions[0], e.Contribution, e.Reference, e.LevelMultiplier,
                    new ExactScoreResult(e.Amount + 1, e.Score.LowerBound, e.Score.UpperBound, e.Score.TermsUsed));
                s.Rewards = new CandidateRewardState(r.PlayerId, r.StateRevision, new[] { new CandidateFixedBaseReward(old.Report, old.Definition, old.Ending, new[] { wrong }) });
            }
            BusinessSaveScenario.Rejected(CandidateBusinessSaveCodec.Prepare(s.Input(), BusinessSaveScenario.Budget()));
        }

        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)]
        public void EverySliceRejectsWrongHeaderTruncationAndTrailingByte(int slice)
        {
            var envelope = BusinessSaveScenario.Encode(BusinessSaveScenario.Prepare(BusinessSaveScenario.New()));
            foreach (var mutation in new[] { 0, 1, 2, 3, 4 })
            {
                var bodies = envelope.Bodies.Select(x => (byte[])x.Clone()).ToArray();
                if (mutation == 0) bodies[slice][0] ^= 1;
                if (mutation == 1) bodies[slice][8] = 2;
                if (mutation == 2) bodies[slice][12] = 99;
                if (mutation == 3) bodies[slice] = bodies[slice].Take(bodies[slice].Length - 1).ToArray();
                if (mutation == 4) bodies[slice] = bodies[slice].Concat(new byte[] { 1 }).ToArray();
                BusinessSaveScenario.Rejected(CandidateBusinessSaveCodec.Decode(BusinessSaveScenario.Repack(envelope, bodies), BusinessSaveScenario.Budget()));
            }
        }

        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)] [TestCase(5)] [TestCase(6)]
        public void DirectoryAndRequirementsMustDescribeExactlyTheFiveActualOwners(int mutation)
        {
            var s = BusinessSaveScenario.New(); s.Begin(); s.Win(); s.EndVictory(); var envelope = BusinessSaveScenario.Encode(BusinessSaveScenario.Prepare(s));
            var slices = BusinessSaveScenario.Slices(envelope);
            if (mutation == 0) slices.RemoveAt(4);
            if (mutation == 1) slices.Add(new SaveSliceInput { Contract = new RequiredSliceContract("unknown", "M99", 1), Bytes = new byte[0], Requirements = BusinessSaveScenario.EmptyRequirements() });
            if (mutation == 2) slices[0].Contract = new RequiredSliceContract("fm.m03.character", "M03", 2);
            if (mutation == 3) slices[3].Requirements = BusinessSaveScenario.EmptyRequirements();
            if (mutation >= 4)
            {
                var old = slices[4].Requirements.Bindings[0]; var wrong = new SaveBinding(old.Kind, old.PackageId, old.DraftId, old.DraftRevision, old.ContentFingerprint,
                    mutation == 4 ? "wrong-rule" : old.RuleVersion, mutation == 5 ? "wrong-numeric" : old.NumericContractVersion,
                    mutation == 6 ? "wrong-random" : old.RandomContractVersion, old.SourceNotes, old.LevelId, old.LevelVersion);
                slices[4].Requirements = new SaveRequirements(new[] { wrong }, new[] { wrong.RuleVersion }, new[] { wrong.NumericContractVersion }, new[] { wrong.RandomContractVersion }, new string[0]);
            }
            var changed = BusinessSaveScenario.Repack(envelope, slices);
            BusinessSaveScenario.Rejected(CandidateBusinessSaveCodec.Decode(changed, BusinessSaveScenario.Budget()));
        }

        [TestCase("bytes")] [TestCase("collection")] [TestCase("string")] [TestCase("number")] [TestCase("math")]
        public void ResourceFailureHasNoValueAndALargerFreshBudgetCanRetryTheSameSource(string kind)
        {
            var s = BusinessSaveScenario.New(3); s.Begin(); s.Attack("first", 0); var snapshot = BusinessSaveScenario.Prepare(s); var envelope = BusinessSaveScenario.Encode(snapshot);
            var limit = kind == "bytes" ? new SaveCodecBudget(BusinessSaveScenario.Math(), maxEnvelopeBytes: 64) :
                kind == "collection" ? new SaveCodecBudget(BusinessSaveScenario.Math(), maxCollectionEntries: 0) :
                kind == "string" ? new SaveCodecBudget(BusinessSaveScenario.Math(), maxStringCodeUnits: 1) :
                kind == "number" ? new SaveCodecBudget(BusinessSaveScenario.Math(), maxNumericTokenBytes: 1) : new SaveCodecBudget(new ExactMathBudget(4));
            BusinessSaveScenario.Rejected(CandidateBusinessSaveCodec.Prepare(s.Input(), limit), "Limit");
            limit = kind == "bytes" ? new SaveCodecBudget(BusinessSaveScenario.Math(), maxEnvelopeBytes: 64) :
                kind == "collection" ? new SaveCodecBudget(BusinessSaveScenario.Math(), maxCollectionEntries: 0) :
                kind == "string" ? new SaveCodecBudget(BusinessSaveScenario.Math(), maxStringCodeUnits: 1) :
                kind == "number" ? new SaveCodecBudget(BusinessSaveScenario.Math(), maxNumericTokenBytes: 1) : new SaveCodecBudget(new ExactMathBudget(4));
            BusinessSaveScenario.Rejected(CandidateBusinessSaveCodec.Decode(envelope, limit), "Limit");
            BusinessSaveScenario.EqualBodies(envelope, BusinessSaveScenario.Encode(BusinessSaveScenario.RoundTrip(snapshot)));
        }

        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)] [TestCase(5)] [TestCase(6)]
        public void RootNullsKeepTheProgrammingErrorContract(int arg)
        {
            var s = BusinessSaveScenario.New(); var snapshot = BusinessSaveScenario.Prepare(s); var envelope = BusinessSaveScenario.Encode(snapshot);
            Assert.Throws<ArgumentNullException>(() => {
                if (arg < 2) CandidateBusinessSaveCodec.Prepare(arg == 0 ? null : s.Input(), arg == 1 ? null : BusinessSaveScenario.Budget());
                else if (arg < 5) CandidateBusinessSaveCodec.Encode(arg == 2 ? null : snapshot, arg == 3 ? null : BusinessSaveScenario.Header(), arg == 4 ? null : BusinessSaveScenario.Budget());
                else CandidateBusinessSaveCodec.Decode(arg == 5 ? null : envelope, arg == 6 ? null : BusinessSaveScenario.Budget());
            });
        }
    }

    // Shared test data uses the accepted public owners for each business transition.
    internal sealed class BusinessSaveScenario
    {
        internal CandidateContext Context;
        internal CandidateCharacterState Character;
        internal CandidateInventoryState Inventory;
        internal CandidateProgressionState Progression;
        internal CandidateRewardState Rewards;
        internal CandidateBattleHistory History;
        internal BattleEntryInput EntryInput;
        internal List<List<FlowPos>> Routes;
        internal List<CandidateBattleRun> RetainedRuns = new List<CandidateBattleRun>();
        internal List<CandidateRollbackRecord> RetainedRollbacks = new List<CandidateRollbackRecord>();
        internal int Level;
        internal static ExactMathBudget Math() { return new ExactMathBudget(); }
        internal static SaveCodecBudget Budget() { return new SaveCodecBudget(Math()); }
        internal static ExactRational R(BigInteger n, int d = 1) { return ExactRational.Create(n, d, Math()); }
        internal static T Accept<T>(SaveCodecResult<T> result) { Assert.IsTrue(result.IsAccepted, result.RejectionCode + " " + result.FieldPath + " " + result.LimitReason); return result.Value; }
        internal static void Rejected<T>(SaveCodecResult<T> result, string code = null)
        { Assert.IsFalse(result.IsAccepted); Assert.IsNull(result.Value); Assert.IsNotEmpty(result.RejectionCode); Assert.IsNotEmpty(result.FieldPath); if (code != null) Assert.AreEqual(code, result.RejectionCode); }
        internal static void Same(object x, object y) { Assert.IsTrue(CandidateBattleReportFingerprint.Equal(x, y, Math()), "Exact independent FMBR01 comparison"); }
        internal static BusinessSaveScenario New(int level = 1, int faces = 1, int entryLevel = 1, int hp = 100)
        {
            var fixture = DemoContentFixture.CaptureSource(level, 1, SourceCoordinateConvention.OneBasedBottomLeft);
            var s = new BusinessSaveScenario { Level = level, Context = new CandidateContext { DraftId = "isolated:015b", DraftRevision = 1,
                ContentFingerprint = "candidate-only", RuleVersion = "candidate-r1", NumericContractVersion = "exact-r1", RandomContractVersion = "pcg-r1",
                SourceNotes = new List<string> { fixture.SourcePath + ":" + fixture.SourceSha256, fixture.CoordinateTransform, "explicit conditional C=1/1000", "explicit conditional C=1/1000" } } };
            var growth = CandidateCharacterGrowth.PrepareDefinition(Growth(s.Context, hp), Math()); Assert.IsTrue(growth.IsAccepted, growth.FieldPath);
            var character = CandidateCharacterGrowth.CreateCandidate(growth.Definition, "player:015b", "W", entryLevel, entryLevel == 4 ? 157 : 0, 2, Math());
            Assert.IsTrue(character.IsAccepted, character.FieldPath); s.Character = character.Next;
            var inventory = CandidateInventory.PrepareDefinition(new CandidateInventoryDefinitionInput { Context = s.Context, Items = new List<CandidateInventoryItemInput> {
                new CandidateInventoryItemInput { ItemId = "candidate:tin", Kind = CandidateInventoryItemKind.OrdinaryMaterial },
                new CandidateInventoryItemInput { ItemId = "candidate:wood", Kind = CandidateInventoryItemKind.OrdinaryMaterial } } }, Math());
            Assert.IsTrue(inventory.IsAccepted, inventory.FieldPath);
            s.Inventory = CandidateInventory.CreateCandidate(inventory.Definition, s.Character.PlayerId, new List<CandidateInventoryActorInput> { Actor(s.Character) }, Math()).Next;
            var progression = CandidateProgression.PrepareDefinition(new CandidateProgressionDefinitionInput { Context = s.Context, Levels = new List<CandidateProgressionLevelInput> {
                new CandidateProgressionLevelInput { LevelId = fixture.FixtureKey, LevelVersion = "candidate-r1", UnlockRuleId = "initial", EntryKind = CandidateProgressionEntryKind.Ordinary,
                    UnlockKind = CandidateProgressionUnlockKind.InitiallyOpen, UnlockAfterLevelId = null, RequiredFeatures = new List<string>() },
                new CandidateProgressionLevelInput { LevelId = "next-level", LevelVersion = "candidate-r1", UnlockRuleId = "after", EntryKind = CandidateProgressionEntryKind.Ordinary,
                    UnlockKind = CandidateProgressionUnlockKind.AfterWholeLevelClear, UnlockAfterLevelId = fixture.FixtureKey, RequiredFeatures = new List<string>() } } }, Math());
            Assert.IsTrue(progression.IsAccepted, progression.FieldPath); s.Progression = CandidateProgression.CreateCandidate(progression.Definition, s.Character.PlayerId, Math()).Next;
            s.Rewards = CandidateBaseRewards.CreateCandidate(s.Character.PlayerId, Math()).Next;
            var geometry = fixture.CopyLevel(); var stats = CandidateCharacterGrowth.ComputeBaseStats(s.Character, Math());
            var face = new FaceInput { FaceId = "face0", Width = geometry.width, Height = geometry.height, Pairs = new List<PairInput>() };
            foreach (var pair in geometry.pairs.OrderBy(x => x.colorId))
            {
                var heavy = level == 3 && pair.colorId == 0;
                face.Pairs.Add(new PairInput { PairId = "pair" + pair.colorId, GeometryColorId = pair.colorId, EndpointA = pair.endpointA, EndpointB = pair.endpointB,
                    Enemy = new EnemyInput { EnemyInstanceKey = "enemy" + pair.colorId, EnemyDefinitionId = heavy ? "E02" : "E01", OriginalSlot = pair.colorId, StableOrder = pair.colorId,
                        Behavior = heavy ? EnemyBehavior.ChargeHeavy : EnemyBehavior.NormalStrike, Stats = Stats(heavy ? 20 : 15, 10, heavy ? 20 : 0, 0),
                        IntentCycle = heavy ? new List<EnemyIntentInput> { new EnemyIntentInput { Kind = EnemyIntentKind.Charge, Targeting = EnemyTargeting.FirstLiving,
                            DamageKind = null, DamageCoefficient = null }, Strike(13, 10) } : new List<EnemyIntentInput> { Strike(3, 5) } } });
            }
            s.EntryInput = new BattleEntryInput { PlayerId = s.Character.PlayerId, ChallengeId = "challenge", AttemptId = "attempt", EntryBaselineId = "baseline",
                Context = s.Context, CarryMode = EntryCarryMode.Empty, RequiredFeatures = new List<string>(),
                Level = new LevelInput { LevelId = fixture.FixtureKey, LevelVersion = "candidate-r1", RecommendedLevel = 1, Faces = new List<FaceInput> { face } },
                Members = new List<MemberInput> { new MemberInput { CharacterId = s.Character.CharacterId, ClassId = stats.ClassId, ClassKind = stats.ClassKind,
                    OriginalSlot = stats.OriginalSlot, Level = stats.Level, IsReady = stats.IsReady, StatsOrigin = stats.StatsOrigin, StatsContext = s.Context,
                    Stats = CopyStats(stats.Stats), EntryHp = stats.EntryHp, LearnedSkills = new List<string>(), Crit = new WarriorCritInput {
                        PassiveDefinitionId = stats.PassiveDefinitionId, TargetProbability = stats.TargetProbability, C = R(1, 1000), Multiplier = stats.CritMultiplier } } } };
            for (var i = 1; i < faces; i++) s.EntryInput.Level.Faces.Add(new FaceInput { FaceId = "face" + i, Width = face.Width, Height = face.Height, Pairs = face.Pairs });
            s.Routes = fixture.CopySolution().paths.OrderBy(x => x.colorId).Select(x => new List<FlowPos>(x.cells)).ToList(); return s;
        }
        internal static GrowthDefinitionInput Growth(CandidateContext context, int hp)
        { return new GrowthDefinitionInput { Context = context, ClassId = "warrior", ClassKind = CharacterClassKind.Warrior, PassiveDefinitionId = "warrior:crit", BaseStats = Stats(hp, 20, 10, 6),
            GrowthHp = R(2, 25), GrowthAttack = R(3, 50), GrowthDefense = R(3, 2), CritBase = R(1, 5), CritStep = R(1, 200), CritCap = R(7, 20), CritMultiplier = R(3, 2),
            XpBase = 60, XpLinear = 20, XpQuadratic = 5, RecoveryDurationMilliseconds = R(180000) }; }
        internal void Begin()
        {
            var input = EntryInput; var b = CandidateProgression.BeginAttempt(Progression, new CandidateProgressionBeginIntent { PlayerId = input.PlayerId,
                LevelId = input.Level.LevelId, LevelVersion = input.Level.LevelVersion, Context = Context, CharacterId = Character.CharacterId,
                ExpectedCharacterRevision = Character.StateRevision, ExpectedOriginalSlot = Character.OriginalSlot, ChallengeId = input.ChallengeId,
                AttemptId = input.AttemptId, EntryBaselineId = input.EntryBaselineId }, Character, Progression.StateRevision, Math());
            Assert.IsTrue(b.IsAccepted, b.FieldPath); Progression = b.Next;
            var frozen = CandidateInventory.Freeze(Inventory, new CandidateInventoryFreezeIntent { PlayerId = input.PlayerId, AttemptId = input.AttemptId, EntryBaselineId = input.EntryBaselineId,
                Context = Context, ReadyParticipants = new List<CandidateInventoryActorInput> { Actor(Character) } }, Inventory.StateRevision, Math());
            Assert.IsTrue(frozen.IsAccepted, frozen.FieldPath); Inventory = frozen.Next;
            var entry = new BattleEntryPreparer().PrepareCandidate(input, Math()); Assert.IsTrue(entry.IsAccepted, entry.FieldPath);
            var bytes = new byte[48]; for (var offset = 0; offset < 48; offset += 16) { bytes[offset] = 42; bytes[offset + 8] = 54; }
            var bound = CandidateRandomPreparer.Prepare(entry.Entry, new CandidateSeedMaterial { Bytes = bytes, SourceCapabilityId = "isolated:015b", MappingId = CandidateRandomPreparer.SupportedMappingId }, Math());
            Assert.IsTrue(bound.IsAccepted, bound.FieldPath); var run = CandidateBattleOperations.CreateCandidate(bound.Binding, Math()); Assert.IsTrue(run.IsAccepted, run.FieldPath);
            var history = CandidateHistoryOperations.CreateCandidate(run.Run, Math()); Assert.IsTrue(history.IsAccepted, history.FieldPath); History = history.Next;
        }
        internal void Attack(string operation, int pair, BigInteger? time = null)
        {
            var run = History.CurrentRun; var request = AttackRequest(run.CurrentSnapshot, operation, pair, Routes[pair]);
            var result = CandidateBattleOperations.EvaluateAttack(run, request, new CandidateBattleConditions { PreferenceRevision = Inventory.PreferenceRevision, ItemUseEnabled = true },
                time ?? 123, new RandomSamplingBudget(Math())); Assert.IsTrue(result.IsAccepted, result.RejectionCode + " " + result.FieldPath);
            var appended = CandidateHistoryOperations.Append(History, result.NextRun, "anchor:" + operation, Math()); Assert.IsTrue(appended.IsAccepted, appended.RejectionCode + " " + appended.FieldPath); History = appended.Next;
        }
        internal void Rollback(string operation, string target)
        {
            var h = History; var range = CandidateHistoryOperations.ReadRange(h, new CandidateHistoryRangeRequest { PlayerId = Character.PlayerId,
                AttemptId = h.Binding.GeneratedForAttemptId, ExpectedSceneRevision = h.CurrentRun.CurrentSnapshot.SceneRevision, HistoryAnchorId = "anchor:" + target }, Math());
            Assert.IsTrue(range.IsAccepted, range.FieldPath); var result = CandidateHistoryOperations.PrepareRollback(h, new CandidateRollbackRequest {
                PlayerId = Character.PlayerId, AttemptId = h.Binding.GeneratedForAttemptId, ExpectedSceneRevision = h.CurrentRun.CurrentSnapshot.SceneRevision,
                OperationId = operation, HistoryAnchorId = "anchor:" + target }, range.Range, Math()); Assert.IsTrue(result.IsAccepted, result.FieldPath); History = result.Next;
        }
        internal void Win()
        {
            var n = 0;
            while (History.CurrentRun.FinalReport == null)
            { Assert.Less(n, 20); var state = History.CurrentRun.CurrentSnapshot; var index = state.Enemies.ToList().FindIndex(e => e.Hp.Numerator.Sign > 0); Attack("win:" + n++, index); }
        }
        internal void EndVictory(BigInteger? materialCount = null)
        {
            var report = History.CurrentRun.FinalReport; Assert.IsNotNull(report); var b = Progression.ActiveAttempt.Begin;
            var facts = EndFacts(b, CandidateProgressionEndKind.NormalVictory, report.Fingerprint); var p = CandidateProgression.EndAttempt(Progression, facts, Progression.StateRevision, Math());
            Assert.IsTrue(p.IsAccepted, p.FieldPath); Progression = p.Next;
            var definition = CandidateBaseRewards.PrepareDefinition(new CandidateRewardDefinitionInput { Context = Context, RewardDefinitionId = "reward", Version = "candidate-r1",
                LevelId = report.Level.LevelId, LevelVersion = report.Level.LevelVersion, BaseExperience = Level == 3 ? 24 : 20, DamageWeight = R(1), TakenWeight = R(1, 4),
                CurveBase = R(1, 2), CurveLog = R(1, 2), ReferenceHpDivisor = R(2), LevelPenaltyBase = R(3, 4), OverlevelGrace = 1,
                ZeroContributionPolicy = CandidateZeroContributionPolicy.CurveIntercept, DropMode = CandidateRewardDropMode.FixedOrdinaryMaterials, RequiredFeatures = new List<string>(),
                Materials = new List<CandidateRewardMaterialInput> { new CandidateRewardMaterialInput { ItemId = "candidate:tin", Amount = materialCount ?? 2 },
                    new CandidateRewardMaterialInput { ItemId = "candidate:wood", Amount = 0 } } }, Math()); Assert.IsTrue(definition.IsAccepted, definition.FieldPath);
            CandidateRewardResult fixedResult;
            using (var scope = new ExactEvaluationScope(new ExactEvaluationBudget(Math(), 1048576, 4096)))
                fixedResult = CandidateBaseRewards.FixNormal(Rewards, report, definition.Definition, p.EndReceipt, new CandidateRewardFixRequest { PlayerId = report.PlayerId,
                    AttemptId = report.AttemptId, ChallengeId = report.ChallengeId, EntryBaselineId = report.EntryBaselineId, SettlementId = p.EndReceipt.SettlementId,
                    FinalReportFingerprint = report.Fingerprint, ExpectedStateRevision = Rewards.StateRevision }, scope);
            Assert.IsTrue(fixedResult.IsAccepted, fixedResult.FieldPath); Rewards = fixedResult.Next;
            var growth = CandidateCharacterGrowth.ApplyBaseReward(Character, Experience(fixedResult.BaseReward), Character.StateRevision, Math());
            Assert.IsTrue(growth.IsAccepted, growth.FieldPath); Character = growth.Next; CloseOwners(p.EndReceipt, false, fixedResult.BaseReward.Materials); History = null;
        }
        internal void EndExit(bool down = false)
        {
            var b = Progression.ActiveAttempt.Begin; var result = CandidateProgression.EndAttempt(Progression, EndFacts(b, CandidateProgressionEndKind.NormalExit, null), Progression.StateRevision, Math());
            Assert.IsTrue(result.IsAccepted, result.FieldPath); Progression = result.Next; CloseOwners(result.EndReceipt, down, new CandidateRewardMaterial[0]); History = null;
        }
        private void CloseOwners(CandidateProgressionEndReceipt end, bool down, IReadOnlyList<CandidateRewardMaterial> materials)
        {
            var c = CandidateCharacterEnd.Propose(Character, new CandidateCharacterEndFacts { PlayerId = Character.PlayerId, CharacterId = Character.CharacterId,
                AttemptId = end.Begin.AttemptId, EntryBaselineId = end.Begin.EntryBaselineId, EndReceiptId = end.EndReceiptId, Context = Context,
                Kind = (CandidateCharacterEndKind)end.Kind, WasParticipant = true, WasDown = down, RecoveryId = down ? "recovery" : null,
                TimeSample = down ? new CandidateTimeSample { WallUtcMilliseconds = 100, ObservedAtUtcMilliseconds = 101, MonotonicElapsedMilliseconds = R(0),
                    MonotonicScopeId = "clock-original", Source = "test-clock", Trust = CandidateTimeTrust.DeviceUntrusted, Anomaly = CandidateTimeAnomaly.None } : null },
                Character.StateRevision, Math()); Assert.IsTrue(c.IsAccepted, c.FieldPath); Character = c.Next;
            var i = CandidateInventory.End(Inventory, new CandidateInventoryEndIntent { PlayerId = Character.PlayerId, AttemptId = end.Begin.AttemptId,
                EntryBaselineId = end.Begin.EntryBaselineId, EndReceiptId = end.EndReceiptId, Context = Context, Kind = (CandidateInventoryEndKind)end.Kind,
                SettlementId = end.SettlementId, NewAttemptId = null, Remaining = new List<CandidateInventoryRemainingInput>(),
                Rewards = materials.Select(x => new CandidateInventoryQuantityInput { ItemId = x.ItemId, Quantity = x.Amount }).ToList() }, Inventory.StateRevision, Math());
            Assert.IsTrue(i.IsAccepted, i.FieldPath); Inventory = i.Next;
        }
        internal CandidateBusinessInput Input() { return new CandidateBusinessInput(Character.PlayerId, Character, Inventory, Progression, Rewards, History, RetainedRuns, RetainedRollbacks); }
        internal void Use(CandidateBusinessSnapshot x) { Character = x.Character; Inventory = x.Inventory; Progression = x.Progression; Rewards = x.Rewards; History = x.ActiveHistory; RetainedRuns = x.RetainedRuns.ToList(); RetainedRollbacks = x.RetainedRollbacks.ToList(); }
        internal static CandidateBusinessSnapshot Prepare(BusinessSaveScenario s) { return Accept(CandidateBusinessSaveCodec.Prepare(s.Input(), Budget())); }
        internal static CandidateBusinessSaveHeader Header() { return new CandidateBusinessSaveHeader(1, "commit:015b", null, new[] { new SaveCommitIndexEntry(1, "commit:015b", null, null, null, new string[0]) }); }
        internal static SaveEnvelope Encode(CandidateBusinessSnapshot x) { return Accept(CandidateBusinessSaveCodec.Encode(x, Header(), Budget())); }
        internal static CandidateBusinessSnapshot RoundTrip(CandidateBusinessSnapshot x)
        {
            var envelope = Encode(x); using (var stream = new MemoryStream())
            { var descriptor = Accept(SaveEnvelopeCodec.Write(stream, envelope, Budget())); stream.Position = 0;
                var decoded = Accept(SaveEnvelopeCodec.Read(stream, descriptor, Budget())); return Accept(CandidateBusinessSaveCodec.Decode(decoded, Budget())); }
        }
        internal static List<SaveSliceInput> Slices(SaveEnvelope e)
        { return e.SliceDirectory.Select((x, i) => new SaveSliceInput { Contract = x.Contract, Requirements = x.Requirements, Bytes = (byte[])e.Bodies[i].Clone() }).ToList(); }
        internal static SaveEnvelope Repack(SaveEnvelope e, byte[][] bodies)
        { var slices = Slices(e); for (var i = 0; i < slices.Count; i++) slices[i].Bytes = bodies[i]; return Repack(e, slices); }
        internal static SaveEnvelope Repack(SaveEnvelope e, List<SaveSliceInput> slices)
        { return Accept(SaveEnvelopeCodec.Prepare(new SaveEnvelopeInput { Purpose = e.Purpose, PlayerId = e.PlayerId, SaveGeneration = e.SaveGeneration,
            CommitId = e.CommitId, ParentCommitId = e.ParentCommitId, CommitIndex = e.CommitIndex, RequiredSliceContracts = slices.Select(x => x.Contract).ToArray(), Slices = slices }, Budget())); }
        internal static SaveRequirements EmptyRequirements() { return new SaveRequirements(new SaveBinding[0], new string[0], new string[0], new string[0], new string[0]); }
        internal static void EqualBodies(SaveEnvelope x, SaveEnvelope y) { for (var i = 0; i < 5; i++) CollectionAssert.AreEqual(x.Bodies[i], y.Bodies[i], "slice " + i); }
        internal static byte[] Hex(string value) { return Enumerable.Range(0, value.Length / 2).Select(i => Convert.ToByte(value.Substring(i * 2, 2), 16)).ToArray(); }
        internal static CandidateAttackRequest AttackRequest(BattleSnapshot state, string operation, int pair, List<FlowPos> route)
        { return new CandidateAttackRequest { PlayerId = state.Baseline.Entry.PlayerId, AttemptId = state.Baseline.Entry.AttemptId, OperationId = operation,
            ExpectedSceneRevision = state.SceneRevision, Actor = state.Members[0].CombatantKey, Pair = state.Enemies[pair].PairKey, Route = route }; }
        private static CandidateProgressionEndFacts EndFacts(CandidateProgressionBeginReceipt b, CandidateProgressionEndKind kind, string fingerprint)
        { return new CandidateProgressionEndFacts { PlayerId = b.PlayerId, LevelId = b.Level.LevelId, LevelVersion = b.Level.LevelVersion, Context = BusinessFields.ContextInput(b.Context),
            ChallengeId = b.ChallengeId, AttemptId = b.AttemptId, EntryBaselineId = b.EntryBaselineId, EndReceiptId = "end:" + b.AttemptId, Kind = kind,
            SettlementId = kind == CandidateProgressionEndKind.NormalVictory ? "settlement:" + b.AttemptId : null, FinalReportFingerprint = fingerprint, NewAttemptId = null }; }
        internal static CandidateProgressionEndFacts ProgressionEnd(CandidateProgressionEndReceipt e)
        { var facts = EndFacts(e.Begin, e.Kind, e.FinalReportFingerprint); facts.EndReceiptId = e.EndReceiptId; facts.SettlementId = e.SettlementId; facts.NewAttemptId = e.NewAttemptId; return facts; }
        internal static CandidateBaseExperience Experience(CandidateFixedBaseReward r)
        { return new CandidateBaseExperience { PlayerId = r.PlayerId, CharacterId = r.Experience[0].CharacterId, AttemptId = r.AttemptId, SettlementId = r.SettlementId,
            Context = BusinessFields.ContextInput(r.Definition.Context), Amount = r.Experience[0].Amount }; }
        internal static CandidateRewardFixRequest RewardRequest(CandidateFixedBaseReward r, BigInteger revision)
        { return new CandidateRewardFixRequest { PlayerId = r.PlayerId, AttemptId = r.AttemptId, ChallengeId = r.ChallengeId, EntryBaselineId = r.EntryBaselineId,
            SettlementId = r.SettlementId, FinalReportFingerprint = r.FinalReportFingerprint, ExpectedStateRevision = revision }; }
        internal static CandidateCharacterEndFacts CharacterEnd(CandidateCharacterEndReceipt e)
        { return new CandidateCharacterEndFacts { PlayerId = e.PlayerId, CharacterId = e.CharacterId, AttemptId = e.AttemptId, EntryBaselineId = e.EntryBaselineId,
            EndReceiptId = e.EndReceiptId, Context = BusinessFields.ContextInput(e.Context), Kind = e.Kind, WasParticipant = e.WasParticipant, WasDown = e.WasDown,
            RecoveryId = e.RecoveryId, TimeSample = e.TimeSample == null ? null : Time(e.TimeSample) }; }
        internal static CandidateTimeSample Time(PreparedCandidateTimeSample t)
        { return new CandidateTimeSample { WallUtcMilliseconds = t.WallUtcMilliseconds, ObservedAtUtcMilliseconds = t.ObservedAtUtcMilliseconds,
            MonotonicElapsedMilliseconds = t.MonotonicElapsedMilliseconds, MonotonicScopeId = t.MonotonicScopeId, Source = t.Source, Trust = t.Trust, Anomaly = t.Anomaly }; }
        internal static CandidateInventoryEndIntent InventoryEnd(CandidateInventoryEndReceipt e)
        { return new CandidateInventoryEndIntent { PlayerId = e.OriginalCarry.PlayerId, AttemptId = e.OriginalCarry.AttemptId, EntryBaselineId = e.OriginalCarry.EntryBaselineId,
            EndReceiptId = e.EndReceiptId, Context = BusinessFields.ContextInput(e.OriginalCarry.Context), Kind = e.Kind, SettlementId = e.SettlementId, NewAttemptId = e.NewAttemptId,
            Remaining = e.Remaining.Select(x => new CandidateInventoryRemainingInput { CharacterId = x.CharacterId, ItemId = x.ItemId, U = x.U }).ToList(),
            Rewards = e.Rewards.Select(x => new CandidateInventoryQuantityInput { ItemId = x.ItemId, Quantity = x.Quantity }).ToList() }; }
        internal static CandidateCharacterState CharacterCopy(CandidateCharacterState c, IEnumerable<CandidateBaseExperienceReceipt> rewards = null, IEnumerable<CandidateCharacterEndReceipt> ends = null)
        { return new CandidateCharacterState(c.Definition, c.PlayerId, c.CharacterId, c.Level, c.Experience, c.OriginalSlot, c.StateRevision, rewards ?? c.BaseRewards, ends ?? c.ProcessedEnds, c.RecoveryPeriods); }
        internal static CandidateInventoryState InventoryCopy(CandidateInventoryState i, IEnumerable<CandidateInventoryHolding> holdings = null,
            IEnumerable<CandidateOrdinaryGrantReceipt> grants = null, IEnumerable<CandidateInventoryEndReceipt> ends = null, BigInteger? preference = null)
        { return new CandidateInventoryState(i.Definition, i.PlayerId, i.Actor, holdings ?? i.Holdings, i.Loadout, i.ActiveCarry, grants ?? i.OrdinaryGrants, ends ?? i.Ends, i.StateRevision, preference ?? i.PreferenceRevision); }
        private static CandidateInventoryActorInput Actor(CandidateCharacterState c) { return new CandidateInventoryActorInput { CharacterId = c.CharacterId, ClassId = c.ClassId, ClassKind = c.Definition.ClassKind, OriginalSlot = c.OriginalSlot }; }
        private static StatsInput Stats(int hp, int attack, int defense, int magic) { return new StatsInput { MaxHp = R(hp), Attack = R(attack), PhysicalDefense = R(defense), MagicDefense = R(magic), Evasion = R(0), AttackRange = 3 }; }
        private static StatsInput CopyStats(PreparedStats s) { return new StatsInput { MaxHp = s.MaxHp, Attack = s.Attack, PhysicalDefense = s.PhysicalDefense, MagicDefense = s.MagicDefense, Evasion = s.Evasion, AttackRange = s.AttackRange }; }
        private static EnemyIntentInput Strike(int n, int d) { return new EnemyIntentInput { Kind = EnemyIntentKind.Strike, Targeting = EnemyTargeting.FirstLiving, DamageKind = EntryDamageKind.Physical, DamageCoefficient = R(n, d) }; }
    }
}
