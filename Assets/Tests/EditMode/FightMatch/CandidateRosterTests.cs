using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using FightMatch.Application;
using NUnit.Framework;
using static FightMatch.Core.Tests.PlayerSessionTestData;
using static FightMatch.Core.Tests.PlayerRosterTestData;

namespace FightMatch.Core.Tests
{
    public class CandidateRosterTests
    {
        [TestCase(false)] [TestCase(true)]
        public void CA16_ProgressionReadMethodsPreserveLegacyPublicPropertyShape(bool legacy)
        {
            CollectionAssert.AreEquivalent(new[] { "PlayerId", "Level", "Context", "ChallengeId", "AttemptId", "EntryBaselineId", "Participant" },
                typeof(CandidateProgressionBeginReceipt).GetProperties().Select(x => x.Name));
            CollectionAssert.AreEquivalent(new[] { "IsAccepted", "Level", "OpenFact", "OpenChallenge", "Participant", "StateRevision", "RejectionCode", "FieldPath" },
                typeof(CandidateProgressionEntryCheck).GetProperties().Select(x => x.Name));
            Assert.IsEmpty(typeof(CandidateProgressionBeginReceipt).GetFields());
            Assert.IsEmpty(typeof(CandidateProgressionEntryCheck).GetFields());
            var f = new IsolatedRoster(legacy: legacy); var b = f.Head.Business; var level = f.Content.Levels[0];
            Assert.IsNull(f.Head.Records[0].Result.RecoveryResults, "initialization has no recovery result after decode");
            var check = legacy ? CandidateProgression.CheckEntry(b.Progression, new CandidateProgressionLevelRequest {
                PlayerId = b.PlayerId, LevelId = level.LevelId, LevelVersion = level.LevelVersion, Context = f.Context,
                CharacterId = b.Character.CharacterId, ExpectedCharacterRevision = b.Character.StateRevision,
                ExpectedOriginalSlot = b.Character.OriginalSlot }, b.Character, Codec().Math) :
                CandidateProgression.CheckEntry(b.Progression, new CandidateProgressionRosterIntent {
                    PlayerId = b.PlayerId, LevelId = level.LevelId, LevelVersion = level.LevelVersion, Context = f.Context,
                    FormationRevision = b.Roster.FormationRevision, Participants = TakeCore(b.Roster.PrepareEntry(Time(), Codec())).Participants }, Codec().Math);
            Assert.IsTrue(check.IsAccepted, check.FieldPath);
            Assert.AreEqual(legacy ? 1 : 3, check.GetParticipants().Count);
            Assert.AreSame(check.GetParticipants(), check.GetParticipants());
            Assert.IsTrue(((IList<CandidateProgressionParticipant>)check.GetParticipants()).IsReadOnly);
            f.Commit(f.EntryRequest()); var begin = f.Head.Business.Progression.ActiveAttempt.Begin;
            Assert.AreSame(begin.GetParticipants(), begin.GetParticipants());
            Assert.IsTrue(((IList<CandidateProgressionParticipant>)begin.GetParticipants()).IsReadOnly);
            CollectionAssert.AreEqual(check.GetParticipants().Select(x => x.CharacterId), begin.GetParticipants().Select(x => x.CharacterId));
            if (legacy)
            {
                Assert.IsNull(begin.GetFormationRevision());
                Assert.IsNull(f.Head.Records.Last().Result.RecoveryResults, "legacy entry has no recovery result after decode");
                Assert.AreSame(begin.GetParticipants()[0], begin.Participant);
                Assert.AreSame(check.GetParticipants()[0], check.Participant);
            }
            else
            {
                Assert.AreEqual(b.Roster.FormationRevision, begin.GetFormationRevision());
                Assert.IsNotNull(f.Head.Records.Last().Result.RecoveryResults);
                Assert.IsEmpty(f.Head.Records.Last().Result.RecoveryResults, "H02 keeps its explicit empty collection after decode");
                Assert.Throws<InvalidOperationException>(() => { var value = begin.Participant; });
                Assert.Throws<InvalidOperationException>(() => { var value = check.Participant; });
            }
        }
        [Test] public void CA03_CA15_PublicDefinitionsAndSharedBuilderProduceCanonicalFourInstanceGenesis()
        {
            var f = new IsolatedRoster(); var b = f.Head.Business;
            CollectionAssert.AreEqual(new[] { "A", "B", "C", "D" }, b.Roster.Characters.Select(x => x.CharacterId));
            CollectionAssert.AreEqual(new[] { "C", "A", "B" }, b.Roster.Formation);
            Assert.AreEqual(4, b.Inventory.Loadouts.Count); Assert.IsTrue(b.Inventory.IsRoster);
            Assert.IsTrue(b.Roster.Characters.All(x => x.Definition.ClassKind == CharacterClassKind.Warrior && x.OriginalSlot == 0));
            Assert.AreEqual(0, f.IdCalls); Assert.AreEqual(0, f.EntropyCalls);
            Assert.IsFalse(f.Head.CommitEligible); Assert.IsFalse(f.Candidate.CommitEligible);
            Assert.Throws<InvalidOperationException>(() => { var value = b.Character; });
            Assert.Throws<InvalidOperationException>(() => { var value = b.Inventory.Loadout; });
            Assert.Throws<InvalidOperationException>(() => { var value = b.Inventory.Actor; });
            Assert.IsFalse(b.Roster.TryGetSingle(out _)); Assert.IsFalse(b.Inventory.TryGetSingle(out _));
            Assert.IsTrue(((IList<CandidateCharacterState>)b.Roster.Characters).IsReadOnly);
            Assert.IsTrue(((IList<string>)b.Roster.Formation).IsReadOnly);
            var five = TakeCore(CandidateBusinessSaveCodec.EncodePublished(b, f.Head.Header, f.Closure, Codec()));
            var restored = TakeCore(CandidateBusinessSaveCodec.DecodePublished(five, f.Closure, Codec()));
            CollectionAssert.AreEqual(Encode(five), Encode(TakeCore(CandidateBusinessSaveCodec.EncodePublished(restored, f.Head.Header, f.Closure, Codec()))));
            Assert.AreEqual(4, TakeCore(CandidateApplicationProtocol.Lookup(f.Head, f.Initialization.Intent, Codec())).Initializations.Count);
        }
        [Test] public void CA01_CA09_SetExactSlotsAndUnchangedReceiptsPreserveInitialFactsAndOperationIdentity()
        {
            var f = new IsolatedRoster(); var before = f.Head.Business.Roster;
            var slots = new[] { "B", null, "A" }; var input = f.Input(CandidateApplicationKind.SetFormation);
            input.SetFormation = new CandidateFormationInput { ExpectedFormationRevision = before.FormationRevision, Slots = slots };
            var request = f.Freeze(input); slots[0] = "D"; input.SetFormation.ExpectedFormationRevision = 99;
            CollectionAssert.AreEqual(new[] { "B", null, "A" }, request.FormationPreview);
            Assert.IsTrue(((IList<string>)request.FormationPreview).IsReadOnly); f.Commit(request);
            var lookup = TakeCore(CandidateApplicationProtocol.Lookup(f.Head, request.Intent, Codec()));
            Assert.AreEqual(new BigInteger(2), lookup.Formation.AfterRevision);
            Assert.IsTrue(f.Head.Business.Roster.Characters.All(x => x.OriginalSlot == 0 && x.StateRevision.IsOne));
            var unchanged = f.SetFormation("B", null, "A");
            var receipt = TakeCore(CandidateApplicationProtocol.Lookup(f.Head, unchanged.Intent, Codec())).Formation;
            Assert.AreEqual(receipt.BeforeRevision, receipt.AfterRevision); Assert.AreEqual(new BigInteger(2), f.Head.Business.Roster.FormationRevision);
            Assert.AreEqual(2, f.Head.Business.Roster.FormationReceipts.Count);
            f.SetFormation(null, null, null);
            Assert.AreEqual("NoReadyMember", f.Build(f.EntryRequest()).Diagnostic.Code);
            Assert.AreEqual(0, f.IdCalls); Assert.AreEqual(0, f.EntropyCalls);
            CollectionAssert.AreEqual(new[] { "C", "A", "B" }, before.Formation);
        }
        [TestCase("duplicate-slot")] [TestCase("unknown")] [TestCase("short")] [TestCase("duplicate-character")] [TestCase("duplicate-class")]
        public void CA01_CA14_RosterRejectsWrongIdentitiesAndDoesNotMutateInputs(string defect)
        {
            var f = new IsolatedRoster(); var roster = f.Head.Business.Roster;
            var characters = roster.Characters.ToList(); IReadOnlyList<string> slots = roster.Formation;
            if (defect == "duplicate-slot") slots = new[] { "A", "A", null };
            if (defect == "unknown") slots = new[] { "missing", null, null };
            if (defect == "short") slots = new[] { "A", null };
            if (defect == "duplicate-character") characters.Add(characters[0]);
            if (defect == "duplicate-class") characters.Add(CandidateCharacterGrowth.CreateCandidate(characters[0].Definition,
                roster.PlayerId, "another", 1, 0, 1, Codec().Math).Next);
            var old = Encode(f.Envelope);
            Assert.IsFalse(CandidateRosterState.Create(roster.PlayerId, characters, slots, Codec()).IsAccepted);
            CollectionAssert.AreEqual(old, Encode(f.Envelope));
        }
        [Test] public void CA14_CollectionAndWireBudgetsApplyBeforeCopiesAndUnknownVersionsCannotEnter()
        {
            var f = new IsolatedRoster(); var r = f.Head.Business.Roster;
            var small = new SaveCodecBudget(Codec().Math, maxCollectionEntries: 2);
            Assert.AreEqual("Limit", CandidateRosterState.Create(r.PlayerId, r.Characters, r.Formation, small).RejectionCode);
            var input = f.Input(CandidateApplicationKind.SetFormation);
            input.SetFormation = new CandidateFormationInput { ExpectedFormationRevision = 1, Slots = new[] { "A", null, null } };
            input.FormatVersion = 99; Assert.IsFalse(CandidateApplicationProtocol.PrepareIntent(input, Codec()).IsAccepted);
            input.FormatVersion = 2; Assert.IsFalse(CandidateApplicationProtocol.PrepareIntent(input, Codec()).IsAccepted);
            input.FormatVersion = 3; input.SetFormation.Slots = new[] { new string((char)0xd800, 1), null, null };
            Assert.IsFalse(CandidateApplicationProtocol.PrepareIntent(input, Codec()).IsAccepted);
            Assert.IsFalse(CandidateApplicationSaveCodec.DecodePublished(f.Envelope, f.Closure,
                new SaveCodecBudget(Codec().Math, maxEnvelopeBytes: 128)).IsAccepted);
        }
        [TestCase("schema99")] [TestCase("mixed")] [TestCase("all3")] [TestCase("feature")]
        [TestCase("missing-feature")] [TestCase("player")] [TestCase("foreign-class")]
        public void CA14_SixSliceCanonicalIdentityRejectsMixedContractsUnknownFeaturesAndForeignOwners(string defect)
        {
            var f = new IsolatedRoster(); var original = Encode(f.Envelope);
            var slices = f.Envelope.SliceDirectory.Select((row, index) => new SaveSliceInput {
                Contract = new RequiredSliceContract(row.Contract.SliceId, row.Contract.OwnerId,
                    defect == "schema99" && index == 0 ? 99U : defect == "mixed" && index == 2 ? 2U :
                    defect == "all3" ? 3U : row.Contract.SchemaVersion),
                Bytes = f.Envelope.SliceBytes[index].ToArray(), Requirements = row.Requirements }).ToArray();
            if (defect == "feature" || defect == "missing-feature")
            {
                var old = slices[0].Requirements;
                var features = defect == "feature" ? old.FeatureIds.Concat(new[] { "fm.unsupported.v99" }).ToArray()
                    : old.FeatureIds.Where(x => x != "fm.player.roster.v1").ToArray();
                slices[0].Requirements = new SaveRequirements(old.Bindings, old.RuleVersions,
                    old.NumericContractVersions, old.RandomContractVersions, features);
            }
            if (defect == "foreign-class")
            {
                var bytes = slices[1].Bytes; var token = System.Text.Encoding.Unicode.GetBytes("warrior:A"); var offset = -1;
                for (var i = 0; i <= bytes.Length - token.Length; i++)
                    if (bytes.Skip(i).Take(token.Length).SequenceEqual(token)) { offset = i; break; }
                Assert.GreaterOrEqual(offset, 0); bytes[offset + token.Length - 2] = (byte)'Z';
            }
            var changed = TakeCore(SaveEnvelopeCodec.Prepare(new SaveEnvelopeInput {
                Purpose = f.Envelope.Purpose, PlayerId = defect == "player" ? "foreign:player" : f.Envelope.PlayerId,
                SaveGeneration = f.Envelope.SaveGeneration, CommitId = f.Envelope.CommitId, ParentCommitId = f.Envelope.ParentCommitId,
                CommitIndex = f.Envelope.CommitIndex, RequiredSliceContracts = slices.Select(x => x.Contract).ToArray(), Slices = slices }, Codec()));
            Assert.IsFalse(CandidateApplicationSaveCodec.DecodePublished(changed, f.Closure, Codec()).IsAccepted);
            CollectionAssert.AreEqual(original, Encode(f.Envelope));
        }

        [TestCase(CandidateInventoryEndKind.NormalVictory)] [TestCase(CandidateInventoryEndKind.NormalExit)]
        [TestCase(CandidateInventoryEndKind.ImmediateRestart)]
        public void CA04_GlobalTotalsAndIndependentLoadoutsConserveEveryFrozenRow(CandidateInventoryEndKind kind)
        {
            var f = new IsolatedRoster(tactical: true); var roster = TakeCore(f.Head.Business.Roster.SetFormation("inventory:slots",
                new[] { null, "A", "B" }, 1, Codec()));
            var inventory = f.Head.Business.Inventory;
            inventory = Inv(CandidateInventory.GrantOrdinary(inventory, new CandidateOrdinaryGrant { PlayerId = roster.PlayerId,
                Context = f.Context, AttemptId = "inventory:source", SettlementId = "inventory:grant",
                Source = CandidateOrdinaryGrantSource.OrdinaryBaseReward,
                Items = new[] { "A", "B", "C", "D" }.Select(x => new CandidateInventoryQuantityInput { ItemId = "item:" + x, Quantity = 120 }).ToList()
            }, inventory.StateRevision, Codec().Math));
            var amount = 7;
            foreach (var id in new[] { "A", "B", "C", "D" }) inventory = Inv(CandidateInventory.Equip(inventory,
                new CandidateEquipIntent { CharacterId = id, ItemId = "item:" + id, L = amount++ }, inventory.StateRevision, Codec().Math));
            inventory = Inv(CandidateInventory.SetPreference(inventory, new CandidateInventoryPreferenceIntent {
                CharacterId = "A", ItemId = "item:A", Enabled = false }, inventory.PreferenceRevision, Codec().Math));
            Assert.IsFalse(inventory.FindLoadout("A").Enabled.Value);
            Assert.AreNotEqual(inventory.FindLoadout("A").Enabled, inventory.FindLoadout("B").Enabled);
            var otherC = inventory.FindLoadout("C"); var otherD = inventory.FindLoadout("D");
            var prepared = TakeCore(roster.PrepareEntry(Time(), Codec()));
            var actors = prepared.Participants.Select(x => new CandidateInventoryActorInput { CharacterId = x.Character.CharacterId,
                ClassId = x.Character.ClassId, ClassKind = CharacterClassKind.Warrior, OriginalSlot = x.OriginalSlot }).ToList();
            var frozen = CandidateInventory.Freeze(inventory, new CandidateInventoryFreezeIntent { PlayerId = roster.PlayerId,
                Context = f.Context, AttemptId = "inventory:attempt", EntryBaselineId = "inventory:baseline", ReadyParticipants = actors },
                roster, inventory.StateRevision, Codec().Math);
            inventory = Inv(frozen); Assert.AreEqual(2, frozen.CarryPlan.Rows.Count);
            foreach (var id in new[] { "A", "B" })
            {
                var row = CandidateInventory.Read(inventory, Codec().Math).Items.Single(x => x.ItemId == "item:" + id);
                Assert.AreEqual(new BigInteger(120), row.T); Assert.AreEqual(BigInteger.Zero, row.L);
                Assert.AreEqual(new BigInteger(99), row.R); Assert.AreEqual(new BigInteger(21), row.F);
                Assert.AreEqual(row.T, row.L + row.R + row.F);
            }
            Assert.AreSame(otherC, inventory.FindLoadout("C")); Assert.AreSame(otherD, inventory.FindLoadout("D"));
            var end = new CandidateInventoryEndIntent { PlayerId = roster.PlayerId, Context = f.Context,
                AttemptId = "inventory:attempt", EntryBaselineId = "inventory:baseline", EndReceiptId = "inventory:end", Kind = kind,
                SettlementId = kind == CandidateInventoryEndKind.NormalVictory ? "inventory:reward" : null,
                NewAttemptId = kind == CandidateInventoryEndKind.ImmediateRestart ? "inventory:again" : null,
                Remaining = new List<CandidateInventoryRemainingInput> {
                    new CandidateInventoryRemainingInput { CharacterId = "A", ItemId = "item:A", U = 4 },
                    new CandidateInventoryRemainingInput { CharacterId = "B", ItemId = "item:B", U = 6 } },
                Rewards = kind == CandidateInventoryEndKind.NormalVictory ? new List<CandidateInventoryQuantityInput> {
                    new CandidateInventoryQuantityInput { ItemId = "tin", Quantity = 2 } } : new List<CandidateInventoryQuantityInput>() };
            var ended = CandidateInventory.End(inventory, end, inventory.StateRevision, Codec().Math); var next = Inv(ended);
            Assert.AreSame(otherC, next.FindLoadout("C")); Assert.AreSame(otherD, next.FindLoadout("D"));
            if (kind == CandidateInventoryEndKind.NormalVictory)
            {
                Assert.AreEqual(new BigInteger(25), next.Holdings.Single(x => x.ItemId == "item:A").T);
                Assert.AreEqual(new BigInteger(4), next.FindLoadout("A").L); Assert.AreEqual(new BigInteger(6), next.FindLoadout("B").L);
                Assert.AreEqual(new BigInteger(2), next.Holdings.Single(x => x.ItemId == "tin").T);
            }
            else if (kind == CandidateInventoryEndKind.NormalExit) Assert.AreEqual(new BigInteger(99), next.FindLoadout("A").L);
            else
            {
                CollectionAssert.AreEqual(new[] { 1, 2 }, next.ActiveCarry.ReadyParticipants.Select(x => x.OriginalSlot));
                CollectionAssert.AreEqual(frozen.CarryPlan.Rows.Select(x => x.C), next.ActiveCarry.Rows.Select(x => x.C));
            }
            Assert.AreSame(next, Inv(CandidateInventory.End(next, end, inventory.StateRevision, Codec().Math)));
        }
        [Test] public void CA02_CA07_RecoveryMovesAndSelectionKeepFactsWithoutAutoAddingUnselectedMembers()
        {
            var f = new IsolatedRoster(40); f.Commit(f.EntryRequest()); f.Attack(); f.Attack(); f.End();
            var old = f.Head.Business.Roster; Assert.IsNotNull(old.Find("C").ActiveRecovery); Assert.IsNotNull(old.Find("A").ActiveRecovery);
            f.SetFormation(null, "C", "A");
            var moved = f.Head.Business.Roster;
            Assert.AreEqual(old.Find("C").ActiveRecovery.RecoveryId, moved.Find("C").ActiveRecovery.RecoveryId);
            Assert.AreEqual(0, moved.Find("C").ActiveRecovery.Elapsed.Compare(old.Find("C").ActiveRecovery.Elapsed, Codec().Math));
            var none = f.Build(f.EntryRequest(Time(100))); Assert.IsFalse(none.IsAccepted); Assert.AreEqual("NoReadyMember", none.Diagnostic.Code);
            Assert.AreEqual(0, f.Head.Business.Roster.Find("C").ActiveRecovery.Elapsed.Numerator.Sign);
            f.Commit(f.EntryRequest(Time(180000)));
            var after = f.Head.Business; CollectionAssert.AreEqual(new[] { "C" }, after.ActiveHistory.CurrentRun.Baseline.Entry.Members.Select(x => x.CharacterId));
            CollectionAssert.AreEqual(new[] { null, "C", "A" }, after.Roster.Formation);
            Assert.IsNotNull(after.Roster.Find("A").ActiveRecovery); Assert.IsTrue(after.Roster.Find("C").IsReady);
            Assert.AreEqual(2, f.Head.Records.Last().Result.RecoveryResults.Count);
            Assert.AreEqual(BigInteger.One, after.Roster.Find("D").StateRevision);
        }
        private static CandidateInventoryState Inv(CandidateInventoryResult result)
        { Assert.IsTrue(result.IsAccepted, result.RejectionCode + " " + result.FieldPath); return result.Next; }
    }
}
