using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using NUnit.Framework;
using static FightMatch.Core.CandidateInventoryRejectionCode;

namespace FightMatch.Core.Tests
{
    [TestFixture]
    public sealed class CandidateInventoryTests
    {
        [Test]
        public void NewCandidateIsEmpty_AndReadDoesNotChangeRevisions()
        {
            var state = NewState(); Quantities(state, "Axe", 0, 0, 0, 0);
            Assert.AreEqual(BigInteger.One, state.StateRevision); Assert.AreEqual(BigInteger.One, state.PreferenceRevision);
            Assert.IsNull(state.Loadout.ItemId); Assert.IsNull(state.Loadout.Enabled); Assert.IsNull(state.ActiveCarry);
            Assert.IsEmpty(state.OrdinaryGrants); Assert.IsEmpty(state.Ends); Assert.AreEqual(2, state.Actor.OriginalSlot);
            var view = CandidateInventory.Read(state, new ExactMathBudget()); Assert.AreSame(state, view.State);
            Assert.AreEqual(4, view.Items.Count); Assert.AreEqual(BigInteger.One, state.StateRevision);
        }

        [Test]
        public void OrdinaryGrantKeepsFixedSourceAndZeroOrEmptyReceipts()
        {
            var state = NewState(); var first = Grant(state, GrantInput(5)); Quantities(first.Next, "Axe", 5, 0, 0, 5);
            Assert.AreEqual("reward", first.GrantReceipt.SettlementId); Assert.AreEqual("a", first.GrantReceipt.AttemptId);
            Assert.AreEqual("player", first.GrantReceipt.PlayerId); Assert.AreSame(state.Definition.Context, first.GrantReceipt.Context);
            Assert.AreEqual(CandidateOrdinaryGrantSource.OrdinaryBaseReward, first.GrantReceipt.Source);
            Assert.AreEqual(new BigInteger(5), first.GrantedItems[0].Quantity); Quantities(state, "Axe", 0, 0, 0, 0);
            var zero = Grant(first.Next, GrantInput(0, "zero")); Assert.AreEqual(1, zero.GrantReceipt.Items.Count);
            Assert.AreEqual(BigInteger.Zero, zero.GrantReceipt.Items[0].Quantity);
            var input = GrantInput(0, "empty"); input.Items.Clear(); var empty = Grant(zero.Next, input);
            Assert.IsEmpty(empty.GrantReceipt.Items); Assert.AreEqual(3, empty.Next.OrdinaryGrants.Count);
            Assert.AreEqual(new BigInteger(4), empty.Next.StateRevision); Assert.AreEqual(BigInteger.One, empty.Next.PreferenceRevision);
        }

        [Test]
        public void FreshRequestWithSameSettlementReturnsCurrentStateAndCanonicalOriginalVector()
        {
            var input = GrantInput(5); input.Items.Add(Q("Tin", 7)); input.Items.Reverse();
            var first = Grant(NewState(), input); var current = Grant(first.Next, GrantInput(4, "later")).Next;
            var again = GrantInput(5); again.Items.Add(Q("Tin", 7));
            var result = CandidateInventory.GrantOrdinary(current, again, 1, new ExactMathBudget());
            Assert.AreEqual(CandidateInventoryOutcome.AlreadyIncluded, result.Outcome); Assert.AreSame(current, result.Next);
            Assert.AreSame(first.GrantReceipt, result.GrantReceipt); Assert.IsEmpty(result.GrantedItems);
            Assert.AreEqual("Axe", result.GrantReceipt.Items[0].ItemId); Quantities(result.Next, "Axe", 9, 0, 0, 9);
            Assert.IsNull(typeof(CandidateOrdinaryGrant).GetProperty("OperationId"));
        }

        [TestCase("AttemptId", "AttemptId")]
        [TestCase("PlayerId", "PlayerId")]
        [TestCase("Quantity", "Items")]
        [TestCase("Context", "Context.ContentFingerprint")]
        public void SameSettlementChangedFactsConflictBeforeStaleRevision(string change, string field)
        {
            var state = Grant(NewState(), GrantInput(5)).Next; var input = GrantInput(5);
            if (change == "Quantity") input.Items[0].Quantity = 6;
            else if (change == "Context") input.Context.ContentFingerprint = "other";
            else Set(input, change, "other");
            Rejected(CandidateInventory.GrantOrdinary(state, input, 0, new ExactMathBudget()), InconsistentBinding, field);
            Quantities(state, "Axe", 5, 0, 0, 5);
        }

        [Test]
        public void TotalsBeyondDoubleIntegerRangeAndStackProjectionRemainExact()
        {
            var huge = (BigInteger.One << 60) + 117;
            var state = Grant(NewState(), GrantInput(huge)).Next;
            var row = CandidateInventory.Read(state, new ExactMathBudget()).Items.Single(i => i.ItemId == "Axe");
            Assert.AreEqual(huge, row.T); Assert.AreEqual(huge, row.F);
            Assert.AreEqual(huge / 99, row.FullStacks); Assert.AreEqual(huge % 99, row.Remainder);
            var small = Equip(Grant(NewState(), GrantInput(119)).Next, EquipInput(2)).Next;
            row = CandidateInventory.Read(small, new ExactMathBudget()).Items.Single(i => i.ItemId == "Axe");
            Assert.AreEqual(new BigInteger(117), row.F); Assert.AreEqual(BigInteger.One, row.FullStacks); Assert.AreEqual(new BigInteger(18), row.Remainder);
            Assert.AreEqual("player", row.Source.PlayerId); Assert.AreEqual("Axe", row.Source.ItemId);
        }

        [Test]
        public void EquipReleasesOldAllocationAndPreservesItOnInsufficientNewPool()
        {
            var state = Equip(Stock(5), EquipInput(2)).Next; var denied = Equip(state, EquipInput(1, "Spear"), false);
            Rejected(denied, InsufficientFree, "L"); Quantities(state, "Axe", 5, 2, 0, 3);
            var stocked = Grant(state, GrantInput(3, "spear", "Spear")).Next;
            var changed = Equip(stocked, EquipInput(3, "Spear")).Next;
            Quantities(changed, "Axe", 5, 0, 0, 5); Quantities(changed, "Spear", 3, 3, 0, 0);
            Assert.IsTrue(changed.Loadout.Enabled); Assert.AreEqual(stocked.PreferenceRevision + 1, changed.PreferenceRevision);
            Assert.AreEqual(stocked.StateRevision + 1, changed.StateRevision);
        }

        [Test]
        public void SelectionAtZeroClearAndExplicitPreferenceHaveSeparateRevisions()
        {
            var selected = Equip(Stock(5), EquipInput(0)).Next;
            Assert.AreEqual("Axe", selected.Loadout.ItemId); Assert.IsTrue(selected.Loadout.Enabled);
            var off = Preference(selected, PreferenceInput(false)).Next; Assert.IsFalse(off.Loadout.Enabled);
            var repeated = Preference(off, PreferenceInput(false)); Assert.AreSame(off, repeated.Next);
            Assert.AreEqual(CandidateInventoryOutcome.Unchanged, repeated.Outcome);
            var changed = Equip(off, EquipInput(2)).Next; Assert.IsFalse(changed.Loadout.Enabled);
            Assert.AreEqual(off.PreferenceRevision, changed.PreferenceRevision); Quantities(changed, "Axe", 5, 2, 0, 3);
            Assert.AreSame(changed, Equip(changed, EquipInput(2)).Next);
            var clear = Equip(changed, EquipInput(0, null)).Next; Assert.IsNull(clear.Loadout.ItemId); Assert.IsNull(clear.Loadout.Enabled);
            Assert.AreEqual(changed.PreferenceRevision + 1, clear.PreferenceRevision); Quantities(clear, "Axe", 5, 0, 0, 5);
            Rejected(Preference(clear, PreferenceInput(true), false), InconsistentBinding, "ItemId");
            Rejected(CandidateInventory.SetPreference(changed, PreferenceInput(true), 1, new ExactMathBudget()), StaleContext, "ExpectedPreferenceRevision");
            Rejected(CandidateInventory.Equip(changed, EquipInput(1), 1, new ExactMathBudget()), StaleContext, "ExpectedRevision");
        }

        [TestCase(101, 2, 99, 2)]
        [TestCase(5, 5, 5, 0)]
        [TestCase(0, 0, 0, 0)]
        public void FreezeBuildsBoundedCarryFromLAndFree_WithOriginalPool(int total, int l, int carry, int free)
        {
            var state = Equip(Stock(total), EquipInput(l)).Next; var result = Freeze(state);
            Quantities(result.Next, "Axe", total, 0, carry, free); Assert.AreEqual(EntryCarryMode.NonEmpty, result.CarryPlan.Mode);
            var row = result.CarryPlan.Rows[0]; Assert.AreEqual(new BigInteger(carry), row.C);
            Assert.AreEqual("player", row.Source.PlayerId); Assert.AreEqual("Axe", row.Source.ItemId); Assert.AreEqual(2, row.Actor.OriginalSlot);
            Assert.AreSame(state.Definition.Context, result.CarryPlan.Context); Assert.AreEqual(state.PreferenceRevision, result.Next.PreferenceRevision);
            Assert.AreEqual("a", result.CarryPlan.AttemptId); Assert.AreEqual("baseline", result.CarryPlan.EntryBaselineId);
            var duplicate = CandidateInventory.Freeze(result.Next, FreezeInput(), 0, new ExactMathBudget());
            Assert.AreSame(result.Next, duplicate.Next); Assert.AreSame(result.CarryPlan, duplicate.CarryPlan);
            Assert.AreEqual(CandidateInventoryOutcome.AlreadyIncluded, duplicate.Outcome);
        }

        [Test]
        public void EmptyFreezeHasIdentityBlocksEquipAndSupportsAllEmptyEndKinds()
        {
            foreach (var kind in new[] { CandidateInventoryEndKind.NormalVictory, CandidateInventoryEndKind.NormalExit, CandidateInventoryEndKind.ImmediateRestart })
            {
                var result = Freeze(NewState()); Assert.AreEqual(EntryCarryMode.Empty, result.CarryPlan.Mode);
                Assert.IsEmpty(result.CarryPlan.Rows); Assert.AreEqual(1, result.CarryPlan.ReadyParticipants.Count);
                Rejected(Equip(result.Next, EquipInput(0), false), ActiveAttemptConflict, "ActiveCarry");
                var intent = EndInput(kind: kind); intent.Remaining.Clear(); var ended = End(result.Next, intent);
                if (kind == CandidateInventoryEndKind.ImmediateRestart) Assert.AreEqual("next", ended.Next.ActiveCarry.AttemptId);
                else Assert.IsNull(ended.Next.ActiveCarry);
                Assert.AreEqual(1, ended.Next.Ends.Count);
            }
        }

        [Test]
        public void ActiveGrantOnlyAddsFree_AndPreferenceDoesNotAlterOriginalCarry()
        {
            var frozen = Freeze(Equip(Stock(5), EquipInput(5)).Next); var granted = Grant(frozen.Next, GrantInput(4, "late")).Next;
            Quantities(granted, "Axe", 9, 0, 5, 4); Assert.AreSame(frozen.CarryPlan, granted.ActiveCarry);
            var off = Preference(granted, PreferenceInput(false)).Next;
            Assert.AreSame(frozen.CarryPlan, off.ActiveCarry); Quantities(off, "Axe", 9, 0, 5, 4);
            Assert.IsFalse(off.Loadout.Enabled); Assert.AreEqual(granted.PreferenceRevision + 1, off.PreferenceRevision);
        }

        [TestCase(CandidateInventoryEndKind.NormalVictory, 0, 0, 3, 3, 0)]
        [TestCase(CandidateInventoryEndKind.NormalVictory, 0, 4, 7, 3, 4)]
        [TestCase(CandidateInventoryEndKind.NormalVictory, 4, 0, 7, 3, 4)]
        [TestCase(CandidateInventoryEndKind.NormalExit, 0, 0, 5, 5, 0)]
        [TestCase(CandidateInventoryEndKind.NormalExit, 4, 0, 9, 5, 4)]
        public void NormalEndConservesCurrentTotalsAndLateRewards(CandidateInventoryEndKind kind, int late, int reward, int total, int l, int free)
        {
            var state = Freeze(Equip(Stock(5), EquipInput(5)).Next).Next;
            if (late != 0) state = Grant(state, GrantInput(late, "late")).Next;
            state = Preference(state, PreferenceInput(false)).Next;
            var intent = EndInput(kind: kind); if (reward != 0) intent.Rewards.Add(Q("Axe", reward));
            var result = End(state, intent); Quantities(result.Next, "Axe", total, l, 0, free);
            Assert.IsNull(result.Next.ActiveCarry); Assert.IsFalse(result.Next.Loadout.Enabled);
            Assert.AreEqual(state.PreferenceRevision, result.Next.PreferenceRevision); Assert.AreEqual(state.StateRevision + 1, result.Next.StateRevision);
            Assert.AreEqual(new BigInteger(5), result.EndReceipt.OriginalCarry.Rows[0].C); Assert.AreEqual(new BigInteger(3), result.EndReceipt.Remaining[0].U);
            var current = Grant(result.Next, GrantInput(2, "later-again")).Next;
            var duplicate = CandidateInventory.End(current, intent, 0, new ExactMathBudget());
            Assert.AreSame(current, duplicate.Next); Assert.AreSame(result.EndReceipt, duplicate.EndReceipt);
            Assert.IsEmpty(duplicate.GrantedItems); Assert.AreEqual(CandidateInventoryOutcome.AlreadyIncluded, duplicate.Outcome);
            Quantities(state, "Axe", 5 + late, 0, 5, late);
        }

        [Test]
        public void RestartTransfersOriginalCAndSourceWithoutToppingUpOrRewindingPreference()
        {
            var frozen = Freeze(Equip(Stock(5), EquipInput(5)).Next);
            var state = Preference(Grant(frozen.Next, GrantInput(100, "late")).Next, PreferenceInput(false)).Next;
            var intent = EndInput(kind: CandidateInventoryEndKind.ImmediateRestart); var result = End(state, intent);
            Quantities(result.Next, "Axe", 105, 0, 5, 100); Assert.AreEqual("next", result.CarryPlan.AttemptId);
            Assert.AreEqual("baseline", result.CarryPlan.EntryBaselineId); Assert.AreSame(frozen.CarryPlan.Rows[0], result.CarryPlan.Rows[0]);
            Assert.AreSame(frozen.CarryPlan.Rows[0].Source, result.CarryPlan.Rows[0].Source); Assert.IsFalse(result.Next.Loadout.Enabled);
            Assert.AreEqual(state.PreferenceRevision, result.Next.PreferenceRevision); Assert.IsEmpty(result.GrantedItems);
            Assert.AreSame(result.Next, CandidateInventory.End(result.Next, intent, 0, new ExactMathBudget()).Next);
            var back = EndInput(kind: CandidateInventoryEndKind.ImmediateRestart); back.AttemptId = "next"; back.EndReceiptId = "end-next"; back.NewAttemptId = "a";
            Rejected(End(result.Next, back, false), InconsistentBinding, "NewAttemptId");
            var oldFreeze = CandidateInventory.Freeze(result.Next, FreezeInput(), 0, new ExactMathBudget());
            Assert.AreSame(result.Next, oldFreeze.Next); Assert.AreSame(frozen.CarryPlan, oldFreeze.CarryPlan);
        }

        [Test]
        public void ZeroCarryRetainsSelectionAndRequiresItsExplicitRemainingRow()
        {
            var state = Freeze(Equip(NewState(), EquipInput(0)).Next).Next;
            var missing = EndInput(u: 0); missing.Remaining.Clear();
            Rejected(End(state, missing, false), InconsistentBinding, "Remaining");
            var ended = End(state, EndInput(u: 0)).Next; Quantities(ended, "Axe", 0, 0, 0, 0);
            Assert.AreEqual("Axe", ended.Loadout.ItemId); Assert.IsTrue(ended.Loadout.Enabled);
            Assert.AreEqual(state.PreferenceRevision, ended.PreferenceRevision);
        }

        [Test]
        public void RestartReceiptRejectsChangedTargetAndPreferenceRetryChecksCurrentRevision()
        {
            var state = Freeze(Equip(Stock(5), EquipInput(2)).Next).Next;
            state = Preference(state, PreferenceInput(false)).Next;
            Rejected(CandidateInventory.SetPreference(state, PreferenceInput(false), state.PreferenceRevision - 1,
                new ExactMathBudget()), StaleContext, "ExpectedPreferenceRevision");
            var intent = EndInput(kind: CandidateInventoryEndKind.ImmediateRestart); state = End(state, intent).Next;
            intent.NewAttemptId = "different";
            Rejected(CandidateInventory.End(state, intent, 0, new ExactMathBudget()), InconsistentBinding, "NewAttemptId");
            Assert.AreEqual("next", state.ActiveCarry.AttemptId); Quantities(state, "Axe", 5, 0, 5, 0);
        }

        [Test]
        public void AlreadyGrantedSettlementIsNotAddedAgainWhenEnding_AndConflictsAreAtomic()
        {
            var active = Freeze(Equip(Stock(5), EquipInput(5)).Next).Next;
            var grant = GrantInput(4, "victory"); var state = Grant(active, grant).Next;
            var intent = EndInput(); intent.Rewards.Add(Q("Axe", 4));
            var result = End(state, intent); Quantities(result.Next, "Axe", 7, 3, 0, 4);
            Assert.IsEmpty(result.GrantedItems); Assert.AreSame(state.OrdinaryGrants[1], result.GrantReceipt);
            Assert.AreEqual(2, result.Next.OrdinaryGrants.Count);
            intent.Rewards[0].Quantity = 5; var before = Describe(state);
            Rejected(End(state, intent, false), InconsistentBinding, "Items"); Assert.AreEqual(before, Describe(state));
            Assert.IsEmpty(state.Ends); Quantities(state, "Axe", 9, 0, 5, 4);
        }

        [Test]
        public void DepletedPoolRetainsGrantReceiptAndAllHistoricalNumbersAreBudgeted()
        {
            var state = Grant(NewState(), GrantInput(256)).Next;
            for (var i = 0; i < 3; i++)
            {
                state = Equip(state, EquipInput(0)).Next;
                var freeze = FreezeInput(); freeze.AttemptId = "consume-" + i; state = Freeze(state, freeze).Next;
                var intent = EndInput(u: 0); intent.AttemptId = freeze.AttemptId; intent.EndReceiptId = "end-" + i; intent.SettlementId = "empty-" + i;
                state = End(state, intent).Next;
            }
            Quantities(state, "Axe", 0, 0, 0, 0); Assert.AreEqual(new BigInteger(256), state.OrdinaryGrants[0].Items[0].Quantity);
            var duplicate = CandidateInventory.GrantOrdinary(state, GrantInput(256), 0, new ExactMathBudget());
            Assert.AreSame(state, duplicate.Next); Assert.IsEmpty(duplicate.GrantedItems);
            AssertAllStateCallsLimit(state, 8);
        }

        [TestCase("Missing")]
        [TestCase("Null")]
        [TestCase("Duplicate")]
        [TestCase("Extra")]
        [TestCase("Negative")]
        [TestCase("AboveC")]
        [TestCase("NoQuantity")]
        [TestCase("Character")]
        [TestCase("Item")]
        public void EndRequiresExactRemainingCoverageWithoutPartialChanges(string fault)
        {
            var state = Freeze(Equip(Stock(5), EquipInput(5)).Next).Next; var intent = EndInput();
            if (fault == "Missing") intent.Remaining.Clear();
            if (fault == "Null") intent.Remaining = null;
            if (fault == "Duplicate") intent.Remaining.Add(U(3));
            if (fault == "Extra") intent.Remaining.Add(new CandidateInventoryRemainingInput { CharacterId = "other", ItemId = "Axe", U = 0 });
            if (fault == "Negative") intent.Remaining[0].U = -1;
            if (fault == "AboveC") intent.Remaining[0].U = 6;
            if (fault == "NoQuantity") intent.Remaining[0].U = null;
            if (fault == "Character") intent.Remaining[0].CharacterId = "other";
            if (fault == "Item") intent.Remaining[0].ItemId = "Tin";
            var before = Describe(state); var result = End(state, intent, false); Assert.IsFalse(result.IsAccepted); Assert.IsNull(result.Next);
            Assert.IsTrue(result.FieldPath.StartsWith("Remaining", StringComparison.Ordinal)); Assert.AreEqual(before, Describe(state));
        }

        [TestCase("EndReceiptId")]
        [TestCase("EntryBaselineId")]
        [TestCase("Remaining")]
        [TestCase("Rewards")]
        [TestCase("Context")]
        [TestCase("Kind")]
        [TestCase("SettlementId")]
        public void CompletedEndCannotBeRewritten(string fault)
        {
            var active = Freeze(Equip(Stock(5), EquipInput(5)).Next).Next; var original = EndInput(); var state = End(active, original).Next;
            var changed = EndInput();
            if (fault == "Remaining") changed.Remaining[0].U = 2;
            else if (fault == "Rewards") changed.Rewards.Add(Q("Tin", 1));
            else if (fault == "Context") changed.Context.SourceNotes[0] = "other";
            else if (fault == "Kind") { changed.Kind = CandidateInventoryEndKind.NormalExit; changed.SettlementId = null; }
            else Set(changed, fault, "other");
            Assert.IsFalse(End(state, changed, false).IsAccepted); Assert.AreEqual(1, state.Ends.Count); Quantities(state, "Axe", 3, 3, 0, 0);
        }

        [Test]
        public void EndReceiptIdentityAndNewAttemptTargetAreExact()
        {
            var state = End(Freeze(Equip(Stock(5), EquipInput(5)).Next).Next, EndInput()).Next;
            var next = FreezeInput(); next.AttemptId = "b"; state = Freeze(state, next).Next;
            var reused = EndInput(u: 3); reused.AttemptId = "b";
            Rejected(End(state, reused, false), InconsistentBinding, "AttemptId");
            var restart = EndInput(u: 3, kind: CandidateInventoryEndKind.ImmediateRestart); restart.AttemptId = "b"; restart.EndReceiptId = "end-b"; restart.NewAttemptId = "b";
            Rejected(End(state, restart, false), InconsistentBinding, "NewAttemptId");
        }

        [TestCase("OtherAttempt", ActiveAttemptConflict)]
        [TestCase("Baseline", InconsistentBinding)]
        [TestCase("Character", InconsistentBinding)]
        [TestCase("Slot", InconsistentBinding)]
        [TestCase("Context", InconsistentBinding)]
        [TestCase("NoReady", InvalidValue)]
        public void FreezeRejectsConflictingIdentityAndParticipantProjection(string fault, CandidateInventoryRejectionCode code)
        {
            var state = Freeze(NewState()).Next; var input = FreezeInput();
            if (fault == "OtherAttempt") input.AttemptId = "b";
            if (fault == "Baseline") input.EntryBaselineId = "other";
            if (fault == "Character") input.ReadyParticipants[0].CharacterId = "other";
            if (fault == "Slot") input.ReadyParticipants[0].OriginalSlot = 1;
            if (fault == "Context") input.Context.DraftRevision = 2;
            if (fault == "NoReady") input.ReadyParticipants.Clear();
            var result = Freeze(state, input, false); Assert.IsFalse(result.IsAccepted); Assert.IsNull(result.Next); Assert.AreEqual(code, result.RejectionCode);
        }

        [Test]
        public void IsolatedNonEmptyCarryDoesNotPassExistingBattleEntryGate()
        {
            var carry = Freeze(Equip(Stock(5), EquipInput(5)).Next).CarryPlan;
            var result = new BattleEntryPreparer().PrepareCandidate(new BattleEntryInput { PlayerId = "player", ChallengeId = "isolated",
                AttemptId = carry.AttemptId, EntryBaselineId = carry.EntryBaselineId, Context = Context(), CarryMode = carry.Mode,
                RequiredFeatures = new List<string>() }, new ExactMathBudget());
            Assert.IsFalse(result.IsAccepted); Assert.IsNull(result.Entry);
            Assert.AreEqual(BattleEntryRejectionCode.UnsupportedBinding, result.RejectionCode); Assert.AreEqual("CarryMode", result.FieldPath);
        }

        [TestCaseSource(nameof(InvalidGrantCases))]
        public void GrantRejectsMissingInvalidOrUnsupportedInput(string path, object value, CandidateInventoryRejectionCode code, string field)
        {
            var input = GrantInput(1); Set(input, path, value); Rejected(Grant(NewState(), input, false), code, field);
        }
        private static IEnumerable<TestCaseData> InvalidGrantCases()
        {
            foreach (var path in new[] { "PlayerId", "AttemptId", "SettlementId", "Context", "Items" }) yield return new TestCaseData(path, null, MissingField, path);
            yield return new TestCaseData("Items[0]", null, MissingField, "Items[0]");
            yield return new TestCaseData("Items[0].Quantity", null, MissingField, "Items[0].Quantity");
            yield return new TestCaseData("Items[0].Quantity", new BigInteger(-1), InvalidValue, "Items[0].Quantity");
            yield return new TestCaseData("Items[0].ItemId", "axe", UnsupportedBinding, "Items[0].ItemId");
            yield return new TestCaseData("Items", new List<CandidateInventoryQuantityInput> { Q("Axe", 1), Q("Axe", 0) }, InvalidValue, "Items[1].ItemId");
            yield return new TestCaseData("Source", CandidateOrdinaryGrantSource.Unspecified, MissingField, "Source");
            foreach (var source in new[] { CandidateOrdinaryGrantSource.Advertisement, CandidateOrdinaryGrantSource.Import, CandidateOrdinaryGrantSource.Mixed })
                yield return new TestCaseData("Source", source, UnsupportedBinding, "Source");
        }

        [TestCaseSource(nameof(InvalidDefinitionCases))]
        public void DefinitionRejectsMissingAndIncompatibleFields(string path, object value, CandidateInventoryRejectionCode code)
        {
            var input = DefinitionInput(); Set(input, path, value); var result = CandidateInventory.PrepareDefinition(input, new ExactMathBudget());
            Assert.IsFalse(result.IsAccepted); Assert.IsNull(result.Definition); Assert.AreEqual(code, result.RejectionCode); Assert.AreEqual(path, result.FieldPath);
        }
        private static IEnumerable<TestCaseData> InvalidDefinitionCases()
        {
            foreach (var path in new[] { "Context", "Context.DraftId", "Context.ContentFingerprint", "Context.RuleVersion", "Context.NumericContractVersion",
                "Context.RandomContractVersion", "Context.SourceNotes", "Items", "Items[0]", "Items[0].ItemId", "Items[0].EquipClassId" })
                yield return new TestCaseData(path, null, MissingField);
            yield return new TestCaseData("Context.DraftRevision", BigInteger.Zero, InvalidValue);
            yield return new TestCaseData("Context.SourceNotes", new List<string>(), InvalidValue);
            yield return new TestCaseData("Context.SourceNotes[0]", " ", MissingField);
            yield return new TestCaseData("Items[0].Kind", CandidateInventoryItemKind.Unspecified, MissingField);
            yield return new TestCaseData("Items[0].Kind", (CandidateInventoryItemKind)99, UnsupportedBinding);
            yield return new TestCaseData("Items[1].ItemId", "Axe", InvalidValue);
            yield return new TestCaseData("Items[3].EquipClassId", "W", InvalidValue);
        }

        [TestCase("CharacterId", null, MissingField)]
        [TestCase("ClassId", null, MissingField)]
        [TestCase("ClassKind", CharacterClassKind.Unspecified, MissingField)]
        [TestCase("ClassKind", (CharacterClassKind)9, UnsupportedBinding)]
        [TestCase("OriginalSlot", null, MissingField)]
        [TestCase("OriginalSlot", -1, InvalidValue)]
        [TestCase("OriginalSlot", 3, InvalidValue)]
        public void ActorProjectionMustBeExplicitSingleWarrior(string field, object value, CandidateInventoryRejectionCode code)
        {
            var actor = Actor(); Set(actor, field, value);
            Rejected(CandidateInventory.CreateCandidate(Definition(), "player", new List<CandidateInventoryActorInput> { actor }, new ExactMathBudget()), code, "Actors[0]." + field);
        }

        [Test]
        public void EmptyDefinitionAndExactOneActorAreSupportedBoundaries()
        {
            var input = DefinitionInput(); input.Items.Clear(); var state = NewState(Definition(input));
            Assert.IsEmpty(CandidateInventory.Read(state, new ExactMathBudget()).Items); Assert.AreEqual(EntryCarryMode.Empty, Freeze(state).CarryPlan.Mode);
            Rejected(CandidateInventory.CreateCandidate(Definition(), "player", new List<CandidateInventoryActorInput>(), new ExactMathBudget()), InvalidValue, "Actors");
            Rejected(CandidateInventory.CreateCandidate(Definition(), "player", new List<CandidateInventoryActorInput> { Actor(), Actor() }, new ExactMathBudget()), UnsupportedBinding, "Actors");
            var actor = Actor(); actor.OriginalSlot = 0; Assert.AreEqual(0, CandidateInventory.CreateCandidate(Definition(), "player",
                new List<CandidateInventoryActorInput> { actor }, new ExactMathBudget()).Next.Actor.OriginalSlot);
        }

        [TestCase("ItemId", "Tin", UnsupportedBinding, "ItemId")]
        [TestCase("ItemId", "MageItem", InconsistentBinding, "ItemId")]
        [TestCase("ItemId", "unknown", UnsupportedBinding, "ItemId")]
        [TestCase("L", null, MissingField, "L")]
        [TestCase("CharacterId", null, MissingField, "CharacterId")]
        public void EquipRejectsUnsupportedAndMissingFields(string path, object value, CandidateInventoryRejectionCode code, string field)
        { var input = EquipInput(0); Set(input, path, value); Rejected(Equip(Stock(5), input, false), code, field); }

        [Test]
        public void EquipLimitsAndOptionalClearAreExplicit()
        {
            var state = Stock(200); Rejected(Equip(state, EquipInput(-1), false), InvalidValue, "L");
            Rejected(Equip(state, EquipInput(100), false), InvalidValue, "L"); Rejected(Equip(state, EquipInput(1, null), false), InvalidValue, "L");
            Rejected(Equip(state, new CandidateEquipIntent { CharacterId = "warrior", L = 0 }, false), MissingField, "ItemId");
            var selected = Equip(state, EquipInput(99)).Next; Quantities(selected, "Axe", 200, 99, 0, 101);
            var preference = PreferenceInput(false); preference.Enabled = null; Rejected(Preference(selected, preference, false), MissingField, "Enabled");
            preference.Enabled = true; preference.ItemId = "Spear"; Rejected(Preference(selected, preference, false), InconsistentBinding, "ItemId");
        }

        [TestCase("PlayerId")]
        [TestCase("AttemptId")]
        [TestCase("EntryBaselineId")]
        [TestCase("EndReceiptId")]
        [TestCase("Context")]
        [TestCase("SettlementId")]
        [TestCase("Rewards")]
        [TestCase("Remaining")]
        public void EndRequiresExplicitFields(string field)
        {
            var state = Freeze(Equip(Stock(5), EquipInput(5)).Next).Next; var input = EndInput(); Set(input, field, null);
            Rejected(End(state, input, false), MissingField, field);
        }

        [Test]
        public void EndModeOptionalFieldsAndStaleContextAreChecked()
        {
            var state = Freeze(Equip(Stock(5), EquipInput(5)).Next).Next;
            Rejected(CandidateInventory.End(state, EndInput(), 1, new ExactMathBudget()), StaleContext, "ExpectedRevision");
            var input = EndInput(); input.Kind = CandidateInventoryEndKind.Unspecified; Rejected(End(state, input, false), MissingField, "Kind");
            input.Kind = (CandidateInventoryEndKind)99; Rejected(End(state, input, false), UnsupportedBinding, "Kind");
            input = EndInput(); input.NewAttemptId = "new"; Rejected(End(state, input, false), InvalidValue, "NewAttemptId");
            input = EndInput(kind: CandidateInventoryEndKind.NormalExit); input.SettlementId = "bad"; Rejected(End(state, input, false), InvalidValue, "SettlementId");
            input.SettlementId = null; input.Rewards.Add(Q("Axe", 0)); Rejected(End(state, input, false), InvalidValue, "Rewards");
            input = EndInput(kind: CandidateInventoryEndKind.ImmediateRestart); input.NewAttemptId = null; Rejected(End(state, input, false), MissingField, "NewAttemptId");
            input = new CandidateInventoryEndIntent { PlayerId = "player", AttemptId = "a", EntryBaselineId = "baseline", EndReceiptId = "e",
                Context = Context(), Kind = CandidateInventoryEndKind.NormalExit, Remaining = new List<CandidateInventoryRemainingInput> { U(0) }, Rewards = new List<CandidateInventoryQuantityInput>() };
            Rejected(End(state, input, false), MissingField, "SettlementId"); input.SettlementId = null;
            Rejected(End(state, input, false), MissingField, "NewAttemptId"); input.NewAttemptId = null; Assert.IsTrue(End(state, input).IsAccepted);
        }

        [Test]
        public void OrdinalIdentitiesAndNewRevisionChecksDoNotNormalizeRequests()
        {
            var state = Stock(5); var input = GrantInput(5); input.SettlementId = "reward "; Assert.AreEqual(2, Grant(state, input).Next.OrdinaryGrants.Count);
            input.PlayerId = "Player"; Rejected(Grant(state, input, false), InconsistentBinding, "PlayerId");
            Rejected(CandidateInventory.GrantOrdinary(state, GrantInput(1, "new"), 1, new ExactMathBudget()), StaleContext, "ExpectedRevision");
            Rejected(CandidateInventory.Freeze(state, FreezeInput(), 1, new ExactMathBudget()), StaleContext, "ExpectedRevision");
        }

        [Test]
        public void EveryPublicEntryRechecksRetainedLargeContextAndQuantities()
        {
            var input = DefinitionInput(); input.Context.DraftRevision = BigInteger.One << 160;
            var definition = Definition(input);
            Assert.Throws<ExactMathLimitException>(() => CandidateInventory.PrepareDefinition(input, new ExactMathBudget(64)));
            Assert.Throws<ExactMathLimitException>(() => CandidateInventory.CreateCandidate(definition, "player", new List<CandidateInventoryActorInput> { Actor() }, new ExactMathBudget(64)));
            AssertAllStateCallsLimit(NewState(definition), 64);
            AssertAllStateCallsLimit(Grant(NewState(), GrantInput(BigInteger.One << 160)).Next, 64);
        }

        [Test]
        public void IntegerOverflowAndLateStepFailureLeaveNoPartialCandidate()
        {
            var state = Grant(NewState(), GrantInput(250, item: "Tin")).Next;
            var input = GrantInput(1, "overflow"); input.Items.Add(Q("Tin", 10)); var old = Describe(state);
            Assert.Throws<ExactMathLimitException>(() => CandidateInventory.GrantOrdinary(state, input, state.StateRevision, new ExactMathBudget(8)));
            Assert.AreEqual(old, Describe(state)); var success = Grant(state, input); Quantities(success.Next, "Tin", 260, 0, 0, 260);
            var active = Freeze(Equip(Stock(5), EquipInput(5)).Next).Next; var end = EndInput(); end.Rewards.Add(Q("Tin", 4));
            var budget = new ExactMathBudget(); var result = CandidateInventory.End(active, end, active.StateRevision, budget);
            Assert.Throws<ExactMathLimitException>(() => CandidateInventory.End(active, end, active.StateRevision,
                new ExactMathBudget(32768, (int)budget.PrimitiveStepsUsed - 1)));
            Assert.AreEqual(Describe(result), Describe(End(active, end))); Quantities(active, "Axe", 5, 0, 5, 0); Assert.IsEmpty(active.Ends);
        }

        [TestCase("Read")]
        [TestCase("Grant")]
        [TestCase("Equip")]
        [TestCase("Preference")]
        [TestCase("Freeze")]
        public void OneSharedStepBudgetCoversEachOperation(string operation)
        {
            var state = Equip(Stock(5), EquipInput(2)).Next; var budget = new ExactMathBudget();
            var first = Invoke(operation, state, budget);
            Assert.Throws<ExactMathLimitException>(() => Invoke(operation, state, new ExactMathBudget(32768, (int)budget.PrimitiveStepsUsed - 1)));
            Assert.AreEqual(Describe(first), Describe(Invoke(operation, state, new ExactMathBudget())));
        }

        [Test]
        public void MutableInputsAreDeeplyIsolatedAndAllOutputsAreReadOnly()
        {
            var input = DefinitionInput(); var definition = Definition(input); var actor = Actor(); var actors = new List<CandidateInventoryActorInput> { actor };
            var state = CandidateInventory.CreateCandidate(definition, "player", actors, new ExactMathBudget()).Next;
            var grant = GrantInput(5); state = Equip(Grant(state, grant).Next, EquipInput(2)).Next;
            var freeze = FreezeInput(); var frozen = Freeze(state, freeze); var pref = PreferenceInput(false); var off = Preference(frozen.Next, pref);
            var end = EndInput(); end.Rewards.Add(Q("Tin", 4)); var result = End(off.Next, end); var text = Describe(result); var frozenText = Describe(frozen);
            input.Context.SourceNotes.Clear(); input.Items[0].ItemId = "changed"; input.Items.Clear(); actor.OriginalSlot = 0; actors.Clear();
            grant.Context.DraftId = "changed"; grant.Items[0].Quantity = 999; freeze.ReadyParticipants[0].CharacterId = "changed"; freeze.ReadyParticipants.Clear();
            pref.Enabled = true; end.Remaining[0].U = 0; end.Rewards[0].Quantity = 999; end.Context.SourceNotes.Clear();
            Assert.AreEqual(text, Describe(result)); Assert.AreEqual(frozenText, Describe(frozen)); Immutable(result); Immutable(CandidateInventory.Read(result.Next, new ExactMathBudget()));
            Assert.Throws<NotSupportedException>(() => ((IList<CandidateInventoryHolding>)result.Next.Holdings).Clear());
            Assert.Throws<NotSupportedException>(() => ((IList<CandidateCarryRow>)frozen.CarryPlan.Rows).Clear());
            Assert.Throws<NotSupportedException>(() => ((IList<CandidateInventoryEndReceipt>)result.Next.Ends).Clear());
            Assert.AreEqual(2, result.Next.Actor.OriginalSlot);
        }

        [Test]
        public void RootNullsThrowAndDoNotUseInternalOrFriendConstruction()
        {
            var state = NewState(); var budget = new ExactMathBudget();
            Assert.Throws<ArgumentNullException>(() => CandidateInventory.PrepareDefinition(null, budget));
            Assert.Throws<ArgumentNullException>(() => CandidateInventory.PrepareDefinition(DefinitionInput(), null));
            Assert.Throws<ArgumentNullException>(() => CandidateInventory.CreateCandidate(null, "p", new List<CandidateInventoryActorInput>(), budget));
            Assert.Throws<ArgumentNullException>(() => CandidateInventory.CreateCandidate(Definition(), "p", null, budget));
            Assert.Throws<ArgumentNullException>(() => CandidateInventory.CreateCandidate(Definition(), "p", new List<CandidateInventoryActorInput>(), null));
            foreach (var op in new[] { "Read", "Grant", "Equip", "Preference", "Freeze", "End" })
            { Assert.Throws<ArgumentNullException>(() => Invoke(op, null, budget)); Assert.Throws<ArgumentNullException>(() => Invoke(op, state, null)); }
            Assert.Throws<ArgumentNullException>(() => CandidateInventory.GrantOrdinary(state, null, 1, budget));
            Assert.Throws<ArgumentNullException>(() => CandidateInventory.Equip(state, null, 1, budget));
            Assert.Throws<ArgumentNullException>(() => CandidateInventory.SetPreference(state, null, 1, budget));
            Assert.Throws<ArgumentNullException>(() => CandidateInventory.Freeze(state, null, 1, budget));
            Assert.Throws<ArgumentNullException>(() => CandidateInventory.End(state, null, 1, budget));
        }

        private static CandidateContext Context() { return new CandidateContext { DraftId = "inventory-candidate", DraftRevision = 1,
            ContentFingerprint = "isolated-items-v1", RuleVersion = "candidate", NumericContractVersion = "exact", RandomContractVersion = "pcg32",
            SourceNotes = new List<string> { "Isolated Tin/Axe ordinary pool; no published content or advertisement source proof." } }; }
        private static CandidateInventoryDefinitionInput DefinitionInput() { return new CandidateInventoryDefinitionInput { Context = Context(), Items = new List<CandidateInventoryItemInput>
            { new CandidateInventoryItemInput { ItemId = "Axe", Kind = CandidateInventoryItemKind.OrdinaryTactical, EquipClassId = "W" },
              new CandidateInventoryItemInput { ItemId = "Spear", Kind = CandidateInventoryItemKind.OrdinaryTactical, EquipClassId = "W" },
              new CandidateInventoryItemInput { ItemId = "MageItem", Kind = CandidateInventoryItemKind.OrdinaryTactical, EquipClassId = "M" },
              new CandidateInventoryItemInput { ItemId = "Tin", Kind = CandidateInventoryItemKind.OrdinaryMaterial, EquipClassId = null } } }; }
        private static CandidateInventoryDefinition Definition(CandidateInventoryDefinitionInput input = null)
        { var r = CandidateInventory.PrepareDefinition(input ?? DefinitionInput(), new ExactMathBudget()); Assert.IsTrue(r.IsAccepted, r.FieldPath); return r.Definition; }
        private static CandidateInventoryActorInput Actor() { return new CandidateInventoryActorInput { CharacterId = "warrior", ClassId = "W", ClassKind = CharacterClassKind.Warrior, OriginalSlot = 2 }; }
        private static CandidateInventoryState NewState(CandidateInventoryDefinition definition = null)
        { return Accepted(CandidateInventory.CreateCandidate(definition ?? Definition(), "player", new List<CandidateInventoryActorInput> { Actor() }, new ExactMathBudget())).Next; }
        private static CandidateInventoryQuantityInput Q(string item, BigInteger quantity) { return new CandidateInventoryQuantityInput { ItemId = item, Quantity = quantity }; }
        private static CandidateOrdinaryGrant GrantInput(BigInteger quantity, string id = "reward", string item = "Axe")
        { return new CandidateOrdinaryGrant { PlayerId = "player", AttemptId = "a", SettlementId = id, Context = Context(), Source = CandidateOrdinaryGrantSource.OrdinaryBaseReward, Items = new List<CandidateInventoryQuantityInput> { Q(item, quantity) } }; }
        private static CandidateEquipIntent EquipInput(BigInteger l, string item = "Axe") { return new CandidateEquipIntent { CharacterId = "warrior", ItemId = item, L = l }; }
        private static CandidateInventoryPreferenceIntent PreferenceInput(bool enabled) { return new CandidateInventoryPreferenceIntent { CharacterId = "warrior", ItemId = "Axe", Enabled = enabled }; }
        private static CandidateInventoryFreezeIntent FreezeInput() { return new CandidateInventoryFreezeIntent { PlayerId = "player", AttemptId = "a", EntryBaselineId = "baseline", Context = Context(), ReadyParticipants = new List<CandidateInventoryActorInput> { Actor() } }; }
        private static CandidateInventoryRemainingInput U(BigInteger u) { return new CandidateInventoryRemainingInput { CharacterId = "warrior", ItemId = "Axe", U = u }; }
        private static CandidateInventoryEndIntent EndInput(int u = 3, CandidateInventoryEndKind kind = CandidateInventoryEndKind.NormalVictory)
        { return new CandidateInventoryEndIntent { PlayerId = "player", AttemptId = "a", EntryBaselineId = "baseline", EndReceiptId = "end-a", Context = Context(), Kind = kind,
            SettlementId = kind == CandidateInventoryEndKind.NormalVictory ? "victory" : null, NewAttemptId = kind == CandidateInventoryEndKind.ImmediateRestart ? "next" : null,
            Remaining = new List<CandidateInventoryRemainingInput> { U(u) }, Rewards = new List<CandidateInventoryQuantityInput>() }; }
        private static CandidateInventoryState Stock(BigInteger amount) { return Grant(NewState(), GrantInput(amount)).Next; }
        private static CandidateInventoryResult Accepted(CandidateInventoryResult result, bool accepted = true) { if (accepted) Assert.IsTrue(result.IsAccepted, result.FieldPath); return result; }
        private static CandidateInventoryResult Grant(CandidateInventoryState state, CandidateOrdinaryGrant input, bool accepted = true)
        { return Accepted(CandidateInventory.GrantOrdinary(state, input, state.StateRevision, new ExactMathBudget()), accepted); }
        private static CandidateInventoryResult Equip(CandidateInventoryState state, CandidateEquipIntent input, bool accepted = true)
        { return Accepted(CandidateInventory.Equip(state, input, state.StateRevision, new ExactMathBudget()), accepted); }
        private static CandidateInventoryResult Preference(CandidateInventoryState state, CandidateInventoryPreferenceIntent input, bool accepted = true)
        { return Accepted(CandidateInventory.SetPreference(state, input, state.PreferenceRevision, new ExactMathBudget()), accepted); }
        private static CandidateInventoryResult Freeze(CandidateInventoryState state, CandidateInventoryFreezeIntent input = null, bool accepted = true)
        { return Accepted(CandidateInventory.Freeze(state, input ?? FreezeInput(), state.StateRevision, new ExactMathBudget()), accepted); }
        private static CandidateInventoryResult End(CandidateInventoryState state, CandidateInventoryEndIntent input, bool accepted = true)
        { return Accepted(CandidateInventory.End(state, input, state.StateRevision, new ExactMathBudget()), accepted); }
        private static void Rejected(CandidateInventoryResult result, CandidateInventoryRejectionCode code, string field)
        { Assert.IsFalse(result.IsAccepted); Assert.IsNull(result.Next); Assert.IsNull(result.GrantReceipt); Assert.IsNull(result.EndReceipt); Assert.IsNull(result.CarryPlan); Assert.AreEqual(code, result.RejectionCode); Assert.AreEqual(field, result.FieldPath); }
        private static void Quantities(CandidateInventoryState state, string item, BigInteger t, BigInteger l, BigInteger r, BigInteger f)
        { var row = CandidateInventory.Read(state, new ExactMathBudget()).Items.Single(i => i.ItemId == item); Assert.AreEqual(t, row.T); Assert.AreEqual(l, row.L); Assert.AreEqual(r, row.R); Assert.AreEqual(f, row.F); Assert.AreEqual(t, row.L + row.R + row.F); }
        private static object Invoke(string op, CandidateInventoryState state, ExactMathBudget budget)
        {
            var revision = state == null ? BigInteger.One : state.StateRevision;
            switch (op)
            {
                case "Read": return CandidateInventory.Read(state, budget);
                case "Grant": return CandidateInventory.GrantOrdinary(state, GrantInput(1, "new"), revision, budget);
                case "Equip": return CandidateInventory.Equip(state, EquipInput(1), revision, budget);
                case "Preference": return CandidateInventory.SetPreference(state, PreferenceInput(false), state == null ? BigInteger.One : state.PreferenceRevision, budget);
                case "Freeze": return CandidateInventory.Freeze(state, FreezeInput(), revision, budget);
                default: return CandidateInventory.End(state, EndInput(), revision, budget);
            }
        }
        private static void AssertAllStateCallsLimit(CandidateInventoryState state, int bits)
        { foreach (var op in new[] { "Read", "Grant", "Equip", "Preference", "Freeze", "End" }) Assert.Throws<ExactMathLimitException>(() => Invoke(op, state, new ExactMathBudget(bits)), op); }
        private static void Set(object input, string path, object value)
        {
            var parts = path.Replace("[", ".").Replace("]", "").Split('.');
            for (var i = 0; i < parts.Length - 1; i++) input = input is IList list ? list[int.Parse(parts[i])] : input.GetType().GetProperty(parts[i]).GetValue(input);
            if (input is IList values) values[int.Parse(parts[parts.Length - 1])] = value;
            else input.GetType().GetProperty(parts[parts.Length - 1]).SetValue(input, value);
        }
        private static string Describe(object value)
        {
            if (value == null) return "null";
            if (value is string || value is BigInteger || value.GetType().IsPrimitive || value.GetType().IsEnum) return value.ToString();
            if (value is IEnumerable items) return "[" + string.Join(",", items.Cast<object>().Select(Describe)) + "]";
            return "{" + string.Join(",", value.GetType().GetProperties().Select(p => p.Name + ":" + Describe(p.GetValue(value)))) + "}";
        }
        private static void Immutable(object value)
        {
            if (value == null || value is string || value is BigInteger || value.GetType().IsPrimitive || value.GetType().IsEnum) return;
            if (value is IList list) { Assert.IsTrue(list.IsReadOnly); foreach (var item in list) Immutable(item); return; }
            Assert.IsEmpty(value.GetType().GetConstructors()); foreach (var p in value.GetType().GetProperties()) { Assert.IsNull(p.SetMethod); Immutable(p.GetValue(value)); }
        }
    }
}
