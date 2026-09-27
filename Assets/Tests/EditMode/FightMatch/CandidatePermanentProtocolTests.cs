using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using FightMatch.Application;
using FightMatch.Platform;
using NUnit.Framework;
using static FightMatch.Core.Tests.PlayerSessionTestData;

namespace FightMatch.Core.Tests
{
    [TestFixture]
    public sealed class CandidatePermanentProtocolTests
    {
        [Test]
        public void CB12_FrozenInputAndAnyNewHeadInvalidateOldUnconfirmedPreview()
        {
            using (var data = new CandidatePermanentTestData())
            {
                var draft = CandidatePermanentTestData.Card("M", 6);
                var quote = data.Preview(draft);
                draft.CharacterId = "W";
                draft.Quantity = 999;
                Assert.AreEqual("M", quote.CharacterId);
                Assert.AreEqual(new BigInteger(6), quote.Quantity);
                Assert.IsTrue(CandidatePermanentProtocol.ValidatePreview(data.Head, data.Definitions, quote, Codec()).IsAccepted);
                data.Gift();
                Assert.AreEqual("StaleContext", CandidatePermanentProtocol.ValidatePreview(data.Head, data.Definitions, quote, Codec()).RejectionCode);
                Assert.AreEqual(new BigInteger(3), data.Total("card"));
            }
        }

        [TestCase("character")]
        [TestCase("inventory")]
        [TestCase("progression")]
        public void CB12_MissingOneOwnerCannotPublishJointCandidate(string missing)
        {
            using (var data = new CandidatePermanentTestData(warriorLevel: 5, warriorXp: 0))
            {
                data.Gift();
                var before = data.Head;
                var request = data.Request(data.Learn("W"));
                var built = data.Build(request);
                Assert.IsTrue(built.IsAccepted, built.Diagnostic?.FieldPath);
                var after = built.NextBusiness;
                var partial = CandidateBusinessSaveCodec.Prepare(new CandidateBusinessInput(after.PlayerId,
                    missing == "character" ? before.Business.Roster : after.Roster,
                    missing == "inventory" ? before.Business.Inventory : after.Inventory,
                    missing == "progression" ? before.Business.Progression : after.Progression,
                    after.Rewards, after.ActiveHistory, after.RetainedRuns, after.RetainedRollbacks, after.Format), SavePurpose.PlayerSave, Codec());
                if (partial.IsAccepted)
                    Assert.IsFalse(CandidateApplicationProtocol.Propose(before, partial.Value, request.Intent, new CandidateApplicationResultInput(), null, Codec()).IsAccepted);
                Assert.AreSame(before, data.Head);
            }
        }

        [Test]
        public void CB13_OriginalOperationResultWinsAfterResourcesAreGoneAndNewHeadExists()
        {
            using (var data = new CandidatePermanentTestData())
            {
                var request = data.Apply(CandidatePermanentTestData.Card("M", 6));
                var commit = data.Head.Header.CommitId;
                data.Gift();
                data.Commit(data.Entry());
                Assert.AreEqual(BigInteger.Zero, data.Total("card"));
                var lookup = TakeCore(CandidateApplicationProtocol.Lookup(data.Head, request.Intent, Codec()));
                Assert.AreEqual(commit, lookup.OriginalCommitId);
                Assert.AreEqual("Applied", lookup.GetPermanent().Outcome);
                var changed = data.Input(CandidateApplicationKind.PermanentRequest, 4);
                changed.OperationId = request.OperationId;
                changed.SetPermanent(data.Preview(data.Learn("M")));
                Assert.AreEqual("OperationConflict", CandidateApplicationProtocol.Lookup(data.Head,
                    TakeCore(CandidateApplicationProtocol.PrepareIntent(changed, Codec())), Codec()).RejectionCode);
            }
        }

        private static IEnumerable<TestCaseData> Faults()
        {
            foreach (var operation in new[] { "card", "learn", "craft", "equip", "preference", "explanation", "gift" })
                foreach (var fault in new[] { "snapshot-before", "snapshot-promoted", "marker-before", "marker-after" })
                    yield return new TestCaseData(operation, fault);
        }

        private static CandidatePermanentDraft Operation(CandidatePermanentTestData data, string kind)
        {
            if (kind == "card") return CandidatePermanentTestData.Card("M", 6);
            if (kind == "learn") return data.Learn("M");
            if (kind == "craft") return new CandidatePermanentDraft { Kind = CandidatePermanentKind.Craft, DefinitionId = "weapons", Quantity = 1 };
            if (kind == "gift") return new CandidatePermanentDraft { Kind = CandidatePermanentKind.BeginTeachingGift, DefinitionId = "tutorial" };
            if (kind == "explanation")
            {
                data.Gift();
                data.Apply(data.Learn("W"));
                return new CandidatePermanentDraft { Kind = CandidatePermanentKind.ConfirmTeachingExplanation, DefinitionId = "tutorial", StepId = "explain" };
            }
            data.Apply(new CandidatePermanentDraft { Kind = CandidatePermanentKind.Craft, DefinitionId = "weapons", Quantity = 1 });
            var equip = new CandidatePermanentDraft { Kind = CandidatePermanentKind.Equip, CharacterId = "W", DefinitionId = "weapon", Quantity = 6 };
            if (kind == "equip") return equip;
            data.Apply(equip);
            return new CandidatePermanentDraft { Kind = CandidatePermanentKind.SetPreference, CharacterId = "W", DefinitionId = "weapon",
                Enabled = false, PreferenceRevision = data.Head.Business.Inventory.PreferenceRevision };
        }

        [TestCaseSource(nameof(Faults))]
        public void CB14_EachOwnerOperationRetainsFrozenTicketAcrossMemoryWriteFaults(string operation, string fault)
        {
            using (var data = new CandidatePermanentTestData(warriorLevel: 5, warriorXp: 0))
            {
                var request = data.Request(Operation(data, operation));
                var before = data.Head;
                var ticket = data.Prepare(request);
                var bytes = Encode(ticket.Envelope);
                data.Storage.Fault = fault;
                var failure = data.Store.Write(ticket, Budget());
                Assert.IsFalse(failure.IsAccepted);
                Assert.IsTrue(failure.Code == "SaveFailed" || failure.Code == "CommitUnknown", failure.Code);
                Assert.AreSame(before, data.Head);
                var repeated = data.Store.Write(ticket, Budget());
                Assert.IsTrue(repeated.IsAccepted, repeated.Code);
                Assert.AreEqual(ticket.Metadata.CommitId, repeated.Value.Descriptor.CommitId);
                CollectionAssert.AreEqual(bytes, Encode(ticket.Envelope));
                data.Load(repeated.Value.Descriptor);
                Assert.AreEqual(1, data.Head.Records.Count(x => x.OperationId == request.OperationId));
                var files = CopyFiles(data.Storage.Files);
                Assert.IsTrue(data.Store.Write(ticket, Budget()).IsAccepted);
                SameFiles(files, data.Storage.Files);
                data.Reopen();
                Assert.AreEqual(ticket.Metadata.CommitId, data.Head.Header.CommitId);
                Assert.AreEqual(1, data.Head.Records.Count(x => x.OperationId == request.OperationId));
            }
        }

        [TestCase("card")]
        [TestCase("learn")]
        [TestCase("craft")]
        [TestCase("equip")]
        [TestCase("preference")]
        [TestCase("explanation")]
        [TestCase("gift")]
        public void CB14_PrepareBudgetRejectionLeavesOriginalHeadAndNoTicket(string operation)
        {
            using (var data = new CandidatePermanentTestData(warriorLevel: 5, warriorXp: 0))
            {
                var request = data.Request(Operation(data, operation));
                var before = CopyFiles(data.Storage.Files);
                var built = data.Build(request, new SaveCodecBudget(new ExactMathBudget(maxPrimitiveSteps: 0)));
                Assert.IsFalse(built.IsAccepted);
                var valid = data.Build(request);
                Assert.IsTrue(valid.IsAccepted, valid.Diagnostic?.FieldPath);
                var candidate = TakeCore(CandidateApplicationProtocol.Propose(data.Head, valid.NextBusiness,
                    request.Intent, valid.CopyResult(), valid.NextSettlementOperationId, Codec()));
                var tiny = new SaveStoreBudget(new SaveCodecBudget(new ExactMathBudget(), maxEnvelopeBytes: 1));
                var prepared = data.Store.Prepare(data.Head.Descriptor, new[] { request.OperationId }, metadata =>
                    CandidateApplicationSaveCodec.EncodePublished(candidate, new CandidateBusinessSaveHeader(
                        metadata.SaveGeneration, metadata.CommitId, metadata.ParentCommitId, metadata.CommitIndex), data.Closure, tiny.Codec), tiny);
                Assert.IsFalse(prepared.IsAccepted);
                Assert.IsFalse(data.Store.HasPendingTicket);
                SameFiles(before, data.Storage.Files);
            }
        }

        [Test]
        public void CB15_DiscardedPreviewCreatesNoRecoveryTransactionAndCommittedResultsSurviveReconstruction()
        {
            using (var data = new CandidatePermanentTestData())
            {
                var files = CopyFiles(data.Storage.Files);
                data.Preview(CandidatePermanentTestData.Card("W", 5));
                SameFiles(files, data.Storage.Files);
                Assert.IsFalse(data.Store.HasPendingTicket);
                data.Reopen();
                Assert.AreEqual(new BigInteger(4), data.W.Level);
                var request = data.Apply(CandidatePermanentTestData.Card("W", 5));
                data.Reopen();
                Assert.AreEqual("Applied", TakeCore(CandidateApplicationProtocol.Lookup(data.Head, request.Intent, Codec())).GetPermanent().Outcome);
            }
        }

        [Test]
        public void CB16_V4CanonicalRoundTripRejectsMixedSchemasAndUnchangedOldMagic()
        {
            using (var data = new CandidatePermanentTestData())
            {
                data.Apply(CandidatePermanentTestData.Card("M", 6));
                var bytes = Encode(data.Envelope);
                var restored = TakeCore(CandidateApplicationSaveCodec.DecodePublished(data.Envelope, data.Closure, Codec()));
                CollectionAssert.AreEqual(new uint[] { 4, 4, 4, 4, 2, 2 }, data.Envelope.RequiredSliceContracts.Select(x => x.SchemaVersion));
                Assert.AreEqual("FMINT004", System.Text.Encoding.ASCII.GetString(restored.Records.Last().Intent.CanonicalBytes.Take(8).ToArray()));
                Assert.IsFalse(CandidateApplicationSaveCodec.DecodePublished(Mutate(data.Envelope, schema: 3), data.Closure, Codec()).IsAccepted);
                data.Reopen();
                CollectionAssert.AreEqual(bytes, Encode(data.Envelope));
            }
        }

        [TestCase("generation")]
        [TestCase("definition")]
        [TestCase("experience")]
        [TestCase("learning")]
        public void CB12_FrozenQuoteMetadataCannotBeChangedAtConfirmation(string field)
        {
            using (var data = new CandidatePermanentTestData())
            {
                var q = data.Preview(CandidatePermanentTestData.Card("M", 6));
                var changed = CandidatePermanentTestData.Requote(q,
                    generation: field == "generation" ? q.SourceGeneration + 1 : (BigInteger?)null,
                    version: field == "definition" ? q.DefinitionVersion + 1 : (BigInteger?)null,
                    fixedXp: field == "experience" ? q.FixedExperience + 1 : (BigInteger?)null,
                    learned: field == "learning" ? "not-a-learning" : null);
                Assert.IsFalse(CandidatePermanentProtocol.ValidatePreview(data.Head, data.Definitions, changed, Codec()).IsAccepted);
                Assert.AreEqual(new BigInteger(3), data.Total("card"));
            }
        }

        [TestCase("self")]
        [TestCase("missing")]
        [TestCase("consumed")]
        public void CB16_DanglingCyclicOrTerminalInputCannotRestoreAsHeld(string kind)
        {
            using (var data = new CandidatePermanentTestData())
            {
                data.Apply(new CandidatePermanentDraft { Kind = CandidatePermanentKind.Craft, DefinitionId = "weapons", Quantity = 1 });
                var b = data.Head.Business;
                var effect = b.Inventory.GetPermanentLedger().Effects.Single();
                var input = effect.Quote.Inputs[0];
                var changed = kind == "consumed"
                    ? new CandidatePermanentPortion(input.Source, null, 0, input.ItemId, input.UnitStart, input.UnitCount,
                        CandidatePermanentEndpoint.Consumed, effect.OperationId)
                    : new CandidatePermanentPortion(null, kind == "self" ? effect.OperationId : "absent-effect", 0,
                        input.ItemId, input.UnitStart, input.UnitCount);
                var inputs = effect.Quote.Inputs.ToArray();
                inputs[0] = changed;
                var forged = new CandidatePermanentEffect(effect.OperationId, CandidatePermanentTestData.Requote(effect.Quote, inputs),
                    effect.Outcome, effect.RelatedOperationId);
                var inventory = new CandidateInventoryState(b.Inventory.Definition, b.PlayerId, b.Inventory.Holdings,
                    b.Inventory.Loadouts, b.Inventory.ActiveCarry, b.Inventory.OrdinaryGrants, b.Inventory.Ends,
                    b.Inventory.StateRevision, b.Inventory.PreferenceRevision, true,
                    new CandidatePermanentInventoryLedger(b.Inventory.GetPermanentLedger().Sources, new[] { forged }));
                var restored = CandidateBusinessSaveCodec.Prepare(new CandidateBusinessInput(b.PlayerId, b.Roster,
                    inventory, b.Progression, b.Rewards, b.ActiveHistory, b.RetainedRuns, b.RetainedRollbacks, b.Format),
                    SavePurpose.PlayerSave, Codec());
                if (restored.IsAccepted)
                    Assert.IsFalse(CandidatePermanentInventory.ReadEndpoints(data.WithBusiness(restored.Value), Codec()).IsAccepted);
                Assert.AreSame(b.Inventory, data.Head.Business.Inventory);
            }
        }

        [Test]
        public void CB01_CB12_ReadonlyQueryProjectionExposesOnlyHeldChoicesAndOriginalResults()
        {
            using (var data = new CandidatePermanentTestData())
            {
                var request = data.Apply(CandidatePermanentTestData.Card("W", 5));
                var view = new PlayerPermanentView(data.Head, "W", data.Definitions.GetPermanentDefinitions(), null,
                    TakeCore(CandidatePermanentInventory.ReadEndpoints(data.Head, Codec())),
                    CandidateInventory.Read(data.Head.Business.Inventory, Codec().Math));
                Assert.IsTrue(view.SourceChoices.All(x => x.Endpoint == CandidatePermanentEndpoint.Held));
                Assert.AreEqual(new BigInteger(2), view.SourceChoices.Where(x => x.ItemId == "card").Aggregate(BigInteger.Zero, (v, x) => v + x.UnitCount));
                Assert.AreEqual(new BigInteger(2), view.Amounts.Items.Single(x => x.ItemId == "card").F);
                Assert.AreEqual(request.OperationId, view.OriginalOperations.Single().OperationId);
                var commit = view.HeadCommitId;
                data.Apply(new CandidatePermanentDraft { Kind = CandidatePermanentKind.Craft, DefinitionId = "weapons", Quantity = 1 });
                Assert.AreEqual(commit, view.HeadCommitId);
                Assert.AreEqual(1, view.OriginalOperations.Count);
                Assert.AreNotEqual(data.Head.Header.CommitId, view.HeadCommitId);
            }
        }

        [Test]
        public void CB16_CardShareFieldsRoundTripAndRejectChangedBeneficiary()
        {
            using (var data = new CandidatePermanentTestData())
            {
                data.Apply(CandidatePermanentTestData.Card("M", 6));
                var effect = data.Head.Business.Roster.GetPermanentEffects().Single();
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    new CandidatePermanentCodec(new BusinessFields(stream, false, Codec()) { StrictUnicode = true }).Effect(effect, "test", 3);
                    bytes = stream.ToArray();
                }
                using (var stream = new MemoryStream(bytes))
                {
                    var restored = new CandidatePermanentCodec(new BusinessFields(stream, true, Codec(), (ulong)stream.Length) { StrictUnicode = true }).Effect(null, "test", 3);
                    Assert.AreEqual(new BigInteger(150), restored.Quote.FixedExperience);
                    Assert.AreEqual(new BigInteger(3), restored.Quote.Inputs.Single().UnitCount);
                }
                // The final field is the exact card beneficiary class, not an independently trusted label.
                bytes[bytes.Length - 2] ^= 1;
                using (var stream = new MemoryStream(bytes))
                    Assert.IsFalse(SaveCodecResult<bool>.Run(() => {
                        new CandidatePermanentCodec(new BusinessFields(stream, true, Codec(), (ulong)stream.Length) { StrictUnicode = true }).Effect(null, "test", 3);
                        return true;
                    }).IsAccepted);
            }
        }

        [Test]
        public void CB05_CB19_NewRewardCandidateCannotExposeSourceBeforeOriginalCommit()
        {
            using (var data = new CandidatePermanentTestData(farm: false))
            {
                data.Commit(data.Entry("test:materials"));
                data.Attack(0);
                data.Attack(1);
                var request = Prepared(CandidateLifecyclePreparation.Victory(data.Head, data.Content,
                    PlayerRosterTestData.Time(), Codec(), true));
                var files = CopyFiles(data.Storage.Files);
                var ticket = data.Prepare(request);
                var records = data.Candidate.Records;
                var last = records.Last();
                Assert.IsNull(last.Generation);
                Assert.IsNull(last.CommitId);
                Assert.IsEmpty(CandidatePermanentInventory.Held(data.Candidate.Business, records, Codec()));
                SameFiles(files, data.Storage.Files);
                var malformed = records.ToList();
                malformed[malformed.Count - 1] = new CandidateApplicationRecord(last.Intent, last.Result, BigInteger.One);
                Assert.AreEqual("UnsupportedSourceProof", SaveCodecResult<bool>.Run(() => {
                    CandidatePermanentInventory.Held(data.Candidate.Business, malformed, Codec());
                    return true;
                }).RejectionCode);
                var written = data.Store.Write(ticket, Budget());
                Assert.IsTrue(written.IsAccepted, written.Code);
                data.Load(written.Value.Descriptor);
                var held = TakeCore(CandidatePermanentInventory.ReadEndpoints(data.Head, Codec()));
                Assert.AreEqual(new BigInteger(3), held.Where(x => x.ItemId == "card")
                    .Aggregate(BigInteger.Zero, (sum, x) => sum + x.UnitCount));
                Assert.IsTrue(held.All(x => x.Source.OriginalCommitId == data.Head.Header.CommitId));
                Assert.IsTrue(held.All(x => x.Source.AcquisitionOrder == data.Head.Records.Count - 1));
                data.Reopen();
                Assert.AreEqual(data.Head.Header.CommitId,
                    TakeCore(CandidatePermanentInventory.ReadEndpoints(data.Head, Codec())).First().Source.OriginalCommitId);
            }
        }

        [TestCase(CandidatePermanentKind.UseExperienceCards)]
        [TestCase(CandidatePermanentKind.LearnSkill)]
        [TestCase(CandidatePermanentKind.Craft)]
        [TestCase(CandidatePermanentKind.SetPreference)]
        [TestCase(CandidatePermanentKind.BeginTeachingGift)]
        [TestCase(CandidatePermanentKind.ConfirmTeachingExplanation)]
        public void C1_R2_NonEquipDefinitionRemainsRequired(CandidatePermanentKind kind)
        {
            using (var data = new CandidatePermanentTestData())
            {
                var quote = data.Preview(data.Learn("M"));
                quote.Draft.Kind = kind;
                quote.Draft.DefinitionId = null;
                var encoded = SaveCodecResult<bool>.Run(() => {
                    new CandidatePermanentCodec(new BusinessFields(Stream.Null, false, Codec())).Quote(quote, "test");
                    return true;
                });
                Assert.AreEqual("MissingField", encoded.RejectionCode);
                Assert.AreEqual("test.Definition", encoded.FieldPath);
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void C1_R3_M03RejectsReversedCharacterOrOperationOrder(bool sameCharacter)
        {
            using (var data = new CandidatePermanentTestData(cards: 6))
            {
                C1Apply(data, CandidatePermanentTestData.Card("M", 6), "c1:z");
                C1Apply(data, sameCharacter ? data.Learn("M") : CandidatePermanentTestData.Card("W", 5), "c1:a");
                var b = data.Head.Business;
                var canonical = b.Roster.GetPermanentEffects();
                Assert.AreEqual(2, canonical.Count);
                var reverse = canonical.Reverse().ToArray();
                var body = data.Envelope.SliceBytes[1].ToArray();
                var malformed = C1ReplaceSuffix(body, C1RawEffects(canonical), C1RawEffects(reverse));
                var envelope = C1Repack(data.Envelope, new Dictionary<int, byte[]> { [1] = malformed });
                var decoded = CandidateApplicationSaveCodec.DecodePublished(envelope, data.Closure, Codec());
                var roster = new CandidateRosterState(b.PlayerId, b.Roster.Characters, b.Roster.Formation,
                    b.Roster.FormationRevision, b.Roster.FormationReceipts, reverse);
                var changed = new CandidateBusinessSnapshot(new CandidateBusinessInput(b.PlayerId, roster, b.Inventory,
                    b.Progression, b.Rewards, b.ActiveHistory, b.RetainedRuns, b.RetainedRollbacks, b.Format));
                var encoded = SaveCodecResult<byte[]>.Run(() => C1OwnerBody(data, changed, 3));
                Assert.IsFalse(decoded.IsAccepted, "M03 reader order");
                Assert.IsFalse(encoded.IsAccepted, "M03 writer order");
                var bytes = Encode(data.Envelope);
                data.Reopen();
                CollectionAssert.AreEqual(bytes, Encode(data.Envelope));
                CollectionAssert.AreEqual(canonical.Select(x => x.OperationId),
                    data.Head.Business.Roster.GetPermanentEffects().Select(x => x.OperationId));
            }
        }

        [Test]
        public void C1_R3_InventoryAndTeachingHistoryKeepChronologicalOrder()
        {
            using (var data = new CandidatePermanentTestData(warriorLevel: 5, warriorXp: 0))
            {
                C1Apply(data, new CandidatePermanentDraft { Kind = CandidatePermanentKind.BeginTeachingGift, DefinitionId = "tutorial" }, "c1:z");
                C1Apply(data, data.Learn("W"), "c1:a");
                C1Apply(data, new CandidatePermanentDraft { Kind = CandidatePermanentKind.ConfirmTeachingExplanation,
                    DefinitionId = "tutorial", StepId = "explain" }, "c1:m");
                var bytes = Encode(data.Envelope);
                data.Reopen();
                CollectionAssert.AreEqual(bytes, Encode(data.Envelope));
                CollectionAssert.AreEqual(new[] { "c1:z", "c1:a" },
                    data.Head.Business.Inventory.GetPermanentLedger().Effects.Select(x => x.OperationId));
                CollectionAssert.AreEqual(new[] { "c1:z", "c1:a", "c1:m" },
                    data.Head.Business.Progression.GetPermanentEffects().Select(x => x.OperationId));
            }
        }

        [Test]
        public void C1_R4_IndependentCostsAndConsistentReferencesCannotRestoreDuplicateLearning()
        {
            using (var data = new CandidatePermanentTestData(certificates: 2))
            {
                data.Apply(data.Learn("M"));
                var basis = data.Head;
                var original = basis.Business.Roster.GetPermanentEffects().Single();
                var input = CandidatePermanentInventory.Held(basis.Business, basis.Records, Codec()).Single(x => x.ItemId == "certificate");
                data.Apply(data.Learn("M"));
                var files = CopyFiles(data.Storage.Files);
                var validRecord = data.Head.Records.Last();
                var q = validRecord.Intent.GetPermanent();
                var draft = data.Learn("M");
                draft.SelectedInputs = new[] { input.Slice(input.UnitStart, 1) };
                var forged = new CandidatePermanentQuote(draft, q.PlayerId, q.ClassId, q.Binding, q.SourceGeneration,
                    q.SourceDescriptorLength, q.SourceDescriptorSha256, q.DefinitionVersion, q.CharacterRevision,
                    q.InventoryRevision, q.PreferenceRevision, q.ProgressionRevision, q.UnitExperience, q.FixedExperience,
                    q.BeforeLevel, q.BeforeExperience, q.FinalLevel, q.FinalExperience, null, draft.SelectedInputs,
                    new[] { new CandidateInventoryQuantity("certificate", 1) }, q.Outputs, null);
                var intentInput = data.Input(CandidateApplicationKind.PermanentRequest, 4);
                intentInput.OperationId = validRecord.OperationId;
                intentInput.ExpectedCommitId = basis.Header.CommitId;
                intentInput.SetPermanent(forged);
                var intent = TakeCore(CandidateApplicationProtocol.PrepareIntent(intentInput, Codec()));
                var effect = new CandidatePermanentEffect(intent.OperationId, forged, "Applied", null);
                var b = basis.Business;
                var roster = CandidatePermanentGrowth.ApplyFrozen(b.Roster, effect, Codec());
                var inventory = CandidatePermanentInventory.Apply(basis, effect, Codec());
                var business = new CandidateBusinessSnapshot(new CandidateBusinessInput(b.PlayerId, roster, inventory,
                    b.Progression, b.Rewards, b.ActiveHistory, b.RetainedRuns, b.RetainedRollbacks, b.Format));
                var result = new CandidateApplicationResult(new CandidateApplicationResultInput(),
                    permanentResult: CandidatePermanentProtocol.Result(business, intent));
                var record = new CandidateApplicationRecord(intent, result, validRecord.Generation, validRecord.CommitId);
                var records = data.Head.Records.ToList();
                records[records.Count - 1] = record;
                Assert.AreNotEqual(original.OperationId, effect.OperationId);
                Assert.LessOrEqual(original.Quote.Inputs.Single().UnitStart + original.Quote.Inputs.Single().UnitCount, input.UnitStart);
                Assert.AreEqual(original.Quote.CharacterRevision + 1, forged.CharacterRevision);
                Assert.AreEqual(BigInteger.Zero, inventory.Holdings.Single(x => x.ItemId == "certificate").T);
                Assert.IsTrue(CandidatePermanentProtocol.Transition(basis, business, intent, Codec()));
                Assert.IsTrue(SaveCodecResult<bool>.Run(() => {
                    new CandidateApplicationReferences(business, Codec()).Validate(records); return true;
                }).IsAccepted, "The forged M02/M03/M04 references and revision history are otherwise consistent.");
                CollectionAssert.AreEqual(data.Envelope.SliceBytes[1], C1OwnerBody(data, data.Head.Business, 3));
                CollectionAssert.AreEqual(data.Envelope.SliceBytes[2], C1OwnerBody(data, data.Head.Business, 4));
                var envelope = C1Repack(data.Envelope, new Dictionary<int, byte[]> {
                    [0] = C1ReplaceSuffix(data.Envelope.SliceBytes[0].ToArray(), C1RecordTail(validRecord), C1RecordTail(record)),
                    [1] = C1OwnerBody(data, business, 3), [2] = C1OwnerBody(data, business, 4) });
                var restored = CandidateApplicationSaveCodec.DecodePublished(envelope, data.Closure, Codec());
                var prepared = CandidateBusinessSaveCodec.Prepare(new CandidateBusinessInput(b.PlayerId, roster, inventory,
                    b.Progression, b.Rewards, b.ActiveHistory, b.RetainedRuns, b.RetainedRollbacks, b.Format), SavePurpose.PlayerSave, Codec());
                Assert.IsFalse(restored.IsAccepted, "Full M02-M07 restore must reject a duplicate character/skill Applied fact.");
                Assert.IsFalse(prepared.IsAccepted, "M03 business restore must reject the same duplicate.");
                SameFiles(files, data.Storage.Files);
                data.Reopen();
                Assert.AreEqual("AlreadyLearned", data.Head.Records.Last().Result.GetPermanent().Outcome);
                Assert.AreEqual(original.OperationId, data.Head.Records.Last().Result.GetPermanent().OriginalLearningOperation);
            }
        }

        private static void C1Apply(CandidatePermanentTestData data, CandidatePermanentDraft draft, string operation)
        {
            var input = data.Input(CandidateApplicationKind.PermanentRequest, 4);
            input.OperationId = operation;
            input.SetPermanent(data.Preview(draft));
            data.Commit(data.Freeze(input));
        }

        private static byte[] C1RawEffects(IReadOnlyList<CandidatePermanentEffect> effects)
        {
            using (var stream = new MemoryStream())
            {
                var f = new BusinessFields(stream, false, Codec()) { StrictUnicode = true, SchemaVersion = 4 };
                f.U((ulong)effects.Count, 4, "test.Count");
                foreach (var effect in effects) new CandidatePermanentCodec(f).Effect(effect, "test", 3);
                return stream.ToArray();
            }
        }

        private static byte[] C1ReplaceSuffix(byte[] body, byte[] before, byte[] after)
        {
            Assert.GreaterOrEqual(body.Length, before.Length);
            CollectionAssert.AreEqual(before, body.Skip(body.Length - before.Length).ToArray());
            return body.Take(body.Length - before.Length).Concat(after).ToArray();
        }

        private static byte[] C1OwnerBody(CandidatePermanentTestData data, CandidateBusinessSnapshot business, int owner)
        {
            using (var stream = new MemoryStream())
            {
                var f = new BusinessFields(stream, false, Codec()) {
                    StrictUnicode = true, SchemaVersion = 4, Resolved = data.Closure, Roster = business.Roster };
                f.Header(owner, business.PlayerId, "M0" + owner);
                if (owner == 3) new CandidateRosterSaveCodec(f).Roster(business.Roster, "M03");
                else new CandidatePermanentSaveCodec(f).Inventory(business.Inventory, "M04");
                return stream.ToArray();
            }
        }

        private static byte[] C1RecordTail(CandidateApplicationRecord record)
        {
            using (var stream = new MemoryStream())
            {
                var f = new BusinessFields(stream, false, Codec()) { StrictUnicode = true, SchemaVersion = 4 };
                var bytes = record.Intent.CanonicalBytes;
                f.U((ulong)bytes.Count, 4, "Intent.Length");
                foreach (var value in bytes) f.U(value, 1, "Intent");
                f.Integer(record.Generation.Value, "Generation", 1);
                f.Text(record.CommitId, "CommitId");
                new CandidatePermanentCodec(f).Result(record.Result.GetPermanent(), "Permanent");
                f.U(0, 4, "Continuation.Count");
                return stream.ToArray();
            }
        }

        private static SaveEnvelope C1Repack(SaveEnvelope original, IDictionary<int, byte[]> changes)
        {
            var slices = original.SliceDirectory.Select((s, i) => new SaveSliceInput { Contract = s.Contract,
                Requirements = s.Requirements, Bytes = changes.ContainsKey(i) ? changes[i] : original.SliceBytes[i].ToArray() }).ToArray();
            return TakeCore(SaveEnvelopeCodec.Prepare(new SaveEnvelopeInput { PlayerId = original.PlayerId, Purpose = original.Purpose,
                SaveGeneration = original.SaveGeneration, CommitId = original.CommitId, ParentCommitId = original.ParentCommitId,
                CommitIndex = original.CommitIndex, RequiredSliceContracts = original.RequiredSliceContracts.ToArray(), Slices = slices }, Codec()));
        }
    }
}
