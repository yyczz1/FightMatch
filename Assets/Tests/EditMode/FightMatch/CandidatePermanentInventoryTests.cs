using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using NUnit.Framework;
using static FightMatch.Core.Tests.PlayerSessionTestData;

namespace FightMatch.Core.Tests
{
    [TestFixture]
    public sealed class CandidatePermanentInventoryTests
    {
        internal static CandidatePermanentPortion ExistingCard(int count = 3, string grantId = "grant:G", string purpose = "experience-card")
        {
            var grant = new CandidateOriginalGrantRef(CandidatePermanentSourceKind.ExistingAdvertisementHeld,
                "p", SharedRuleFixture.Binding(), null, null, grantId, purpose, "fact:original", "use:original");
            var evidence = new CandidateExistingInclusionRef("checkpoint:original", "branch:original", "commit:source",
                7, grant, 0, 0, count, CandidateInclusionDisposition.SelectedHolding, null);
            var source = new CandidatePermanentSourceLine(grant, 0, "card", count, 12, "operation:original",
                "commit:original", "branch:original", evidence);
            return new CandidatePermanentPortion(source, null, 0, "card", 0, count);
        }

        [Test]
        public void CB05_ExistingHeldOwnerKernelRetainsOriginalUnitPositionsAndRejectsReuse()
        {
            // Existing-owner value fixture only: this neither creates InclusionEvidence nor admits a real grant.
            var original = ExistingCard();
            CandidatePermanentInventory.CheckSource(original.Source, "p", Codec());
            var held = new List<CandidatePermanentPortion> { original };
            var spent = original.Slice(0, 2);
            CandidatePermanentInventory.Consume(held, spent, Codec());
            Assert.AreEqual(1, held.Count);
            Assert.AreEqual(new BigInteger(2), held[0].UnitStart);
            Assert.AreEqual(BigInteger.One, held[0].UnitCount);
            Assert.AreSame(original.Source, held[0].Source);
            Assert.Throws<SaveCodecFailure>(() => CandidatePermanentInventory.Consume(held, spent, Codec()));
            CandidatePermanentInventory.Consume(held, original.Slice(2, 1), Codec());
            Assert.IsEmpty(held);
            Assert.AreEqual(new BigInteger(3), original.Source.OriginalQuantity);
            Assert.Throws<SaveCodecFailure>(() => CandidatePermanentInventory.Consume(held, original.Slice(0, 1), Codec()));
        }

        [TestCase("foreign-grant", "experience-card")]
        [TestCase("grant:G", "wrong-purpose")]
        public void CB05_ChangedGrantOrPurposeCannotConsumeCurrentOwnerRange(string grant, string purpose)
        {
            var held = new List<CandidatePermanentPortion> { ExistingCard() };
            Assert.Throws<SaveCodecFailure>(() => CandidatePermanentInventory.Consume(held, ExistingCard(3, grant, purpose).Slice(0, 1), Codec()));
            Assert.AreEqual(new BigInteger(3), held[0].UnitCount);
        }

        [Test]
        public void CB05_AdvertisementPriorityAndStableAcquisitionPrecedeOrdinary()
        {
            var advertisement = ExistingCard(1);
            var ordinaryGrant = new CandidateOriginalGrantRef(CandidatePermanentSourceKind.OrdinaryBaseReward, "p",
                advertisement.Source.Grant.Binding, "attempt:N", "settlement:N", null, null, null, null);
            var source = new CandidatePermanentSourceLine(ordinaryGrant, 0, "card", 2, 1, "op:N", "commit:N", null, null);
            var ordinary = new CandidatePermanentPortion(source, null, 0, "card", 0, 2);
            var portions = new List<CandidatePermanentPortion> { ordinary, advertisement };
            portions.Sort(CandidatePermanentInventory.Compare);
            Assert.AreSame(advertisement, portions[0]);
            var held = new List<CandidatePermanentPortion>(portions);
            CandidatePermanentInventory.Consume(held, advertisement, Codec());
            Assert.AreSame(ordinary, held.Single());
            CandidatePermanentInventory.Consume(held, ordinary.Slice(0, 1), Codec());
            Assert.AreEqual(BigInteger.One, held.Single().UnitStart);
            Assert.AreEqual(new BigInteger(100), (advertisement.UnitCount + 1) * 50);
        }

        [Test]
        public void CB16_SourceAndExistingInclusionRoundTripStrictlyWithoutReissuingGrant()
        {
            var original = ExistingCard();
            using (var stream = new MemoryStream())
            {
                var writer = new CandidatePermanentCodec(new BusinessFields(stream, false, Codec()) { StrictUnicode = true });
                writer.Portion(original, "source");
                var bytes = stream.ToArray();
                stream.Position = 0;
                var fields = new BusinessFields(stream, true, Codec(), (ulong)stream.Length) { StrictUnicode = true };
                var restored = new CandidatePermanentCodec(fields).Portion(null, "source");
                fields.End("source");
                Assert.IsTrue(CandidatePermanentCodec.SameRoot(original, restored, Codec()));
                Assert.AreEqual("fact:original", restored.Source.Grant.FactId);
                Assert.AreEqual("use:original", restored.Source.Grant.UseId);
                Assert.AreEqual("checkpoint:original", restored.Source.Inclusion.CheckpointId);
                bytes[0] = 9;
                using (var bad = new MemoryStream(bytes))
                    Assert.Throws<SaveCodecFailure>(() => new CandidatePermanentCodec(new BusinessFields(bad, true, Codec(), (ulong)bad.Length)).Portion(null, "source"));
            }
        }

        [TestCase(1, 6, 117, 119)]
        [TestCase(20, 120, 60, 100)]
        public void CB06_MergedRecipeAndBatchesConserveTotalsAboveDisplayStack(int batches, int weapons, int ore, int wood)
        {
            using (var data = new CandidatePermanentTestData())
            {
                var request = data.Apply(new CandidatePermanentDraft { Kind = CandidatePermanentKind.Craft, DefinitionId = "weapons", Quantity = batches });
                Assert.AreEqual(new BigInteger(weapons), data.Total("weapon"));
                Assert.AreEqual(new BigInteger(ore), data.Total("ore"));
                Assert.AreEqual(new BigInteger(wood), data.Total("wood"));
                Assert.AreEqual(2, request.Intent.GetPermanent().Costs.Count);
                Assert.AreEqual(2, request.Intent.GetPermanent().Inputs.Count);
                data.Reopen();
                Assert.AreEqual(new BigInteger(weapons), data.Total("weapon"));
                var view = CandidateInventory.Read(data.Head.Business.Inventory, Codec().Math).Items.Single(x => x.ItemId == "weapon");
                Assert.AreEqual(new BigInteger(weapons / 99), view.FullStacks);
                Assert.AreEqual(new BigInteger(weapons % 99), view.Remainder);
            }
        }

        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(41)]
        public void CB06_InvalidOrInsufficientRecipeDoesNotWrite(int batches)
        {
            using (var data = new CandidatePermanentTestData())
            {
                var files = CopyFiles(data.Storage.Files);
                Assert.IsFalse(CandidatePermanentProtocol.Preview(data.Head, data.Definitions,
                    new CandidatePermanentDraft { Kind = CandidatePermanentKind.Craft, DefinitionId = "weapons", Quantity = batches }, Codec()).IsAccepted);
                SameFiles(files, data.Storage.Files);
            }
        }

        [Test]
        public void CB07_MultipleMaterialSourcesRequireExplicitFrozenSelection()
        {
            using (var data = new CandidatePermanentTestData())
            {
                data.Farm();
                var draft = new CandidatePermanentDraft { Kind = CandidatePermanentKind.Craft, DefinitionId = "weapons", Quantity = 1 };
                Assert.AreEqual("CostSelectionRequired", CandidatePermanentProtocol.Preview(data.Head, data.Definitions, draft, Codec()).RejectionCode);
                var held = CandidatePermanentInventory.Held(data.Head.Business, data.Head.Records, Codec());
                draft.SelectedInputs = new[] { held.First(x => x.ItemId == "ore").Slice(0, 3), held.First(x => x.ItemId == "wood").Slice(0, 1) };
                var quote = data.Preview(draft);
                data.Apply(draft);
                Assert.AreEqual(new BigInteger(6), data.Total("weapon"));
                Assert.AreEqual(2, quote.Inputs.Count);
            }
        }

        [Test]
        public void CB08_ExplicitPreferenceSurvivesQuantityChangesAndZeroSelection()
        {
            using (var data = new CandidatePermanentTestData())
            {
                data.Apply(new CandidatePermanentDraft { Kind = CandidatePermanentKind.Craft, DefinitionId = "weapons", Quantity = 20 });
                data.Apply(new CandidatePermanentDraft { Kind = CandidatePermanentKind.Equip, CharacterId = "W", DefinitionId = "weapon", Quantity = 99 });
                Assert.IsTrue(data.Head.Business.Inventory.FindLoadout("W").Enabled);
                var off = new CandidatePermanentDraft { Kind = CandidatePermanentKind.SetPreference, CharacterId = "W", DefinitionId = "weapon",
                    Enabled = false, PreferenceRevision = data.Head.Business.Inventory.PreferenceRevision };
                data.Apply(off);
                var revision = data.Head.Business.Inventory.PreferenceRevision;
                data.Apply(new CandidatePermanentDraft { Kind = CandidatePermanentKind.Equip, CharacterId = "W", DefinitionId = "weapon", Quantity = 0 });
                var loadout = data.Head.Business.Inventory.FindLoadout("W");
                Assert.AreEqual("weapon", loadout.ItemId);
                Assert.IsFalse(loadout.Enabled);
                Assert.AreEqual(revision, data.Head.Business.Inventory.PreferenceRevision);
                off.PreferenceRevision = revision;
                var same = data.Apply(off);
                Assert.AreEqual("Unchanged", data.Head.Records.Last().Result.GetPermanent().Outcome);
                Assert.AreEqual(revision, data.Head.Business.Inventory.PreferenceRevision);
                Assert.IsFalse(CandidatePermanentProtocol.Preview(data.Head, data.Definitions,
                    new CandidatePermanentDraft { Kind = CandidatePermanentKind.Equip, CharacterId = "M", DefinitionId = "weapon", Quantity = 1 }, Codec()).IsAccepted);
                Assert.IsNotNull(same);
            }
        }

        [TestCase(1, 157, 1)]
        [TestCase(2, 85, 2)]
        public void CB05_OneOwnerAllocationUsesAdvertisementBeforeOrdinaryWithoutGrantAdmission(int ordinary, int xp, int units)
        {
            using (var data = new CandidatePermanentTestData(cards: ordinary, warriorXp: xp))
            {
                var basis = data.ExistingOwnerFixture("card", 1);
                var quote = TakeCore(CandidatePermanentProtocol.Preview(basis, data.Definitions, CandidatePermanentTestData.Card("W", 5), Codec()));
                Assert.AreEqual(CandidatePermanentSourceKind.ExistingAdvertisementHeld, quote.Inputs[0].Source.Grant.Kind);
                Assert.AreEqual(units, (int)quote.Inputs.Aggregate(BigInteger.Zero, (n, x) => n + x.UnitCount));
                Assert.AreEqual(new BigInteger(units * 50), quote.FixedExperience);
                Assert.AreEqual(units, quote.Inputs.Count);
                var next = TakeCore(CandidatePermanentProtocol.Build(basis, data.Definitions, quote, "fixture:spend", Codec()));
                var endpoints = TakeCore(CandidatePermanentInventory.ReadEndpoints(data.WithBusiness(next), Codec()));
                Assert.AreEqual(units, (int)endpoints.Where(x => x.Endpoint == CandidatePermanentEndpoint.Consumed)
                    .Aggregate(BigInteger.Zero, (n, x) => n + x.UnitCount));
                Assert.IsFalse(endpoints.Any(x => x.Source?.Grant.Kind == CandidatePermanentSourceKind.ExistingAdvertisementHeld &&
                    x.Endpoint == CandidatePermanentEndpoint.Held));
                var intent = data.Input(CandidateApplicationKind.PermanentRequest, 4);
                intent.OperationId = "fixture:spend";
                intent.SetPermanent(quote);
                var candidate = CandidateApplicationProtocol.Propose(basis, next,
                    TakeCore(CandidateApplicationProtocol.PrepareIntent(intent, Codec())), new CandidateApplicationResultInput(), null, Codec());
                Assert.IsFalse(candidate.IsAccepted, "Existing-held reference values must not authorize M02/M12 writes.");
            }
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(6)]
        public void CB05_MissingOrChangedOriginalIdentityCannotEstablishHolding(int field)
        {
            var old = ExistingCard().Source;
            var changed = new CandidatePermanentSourceLine(old.Grant, field == 0 ? 1 : old.GrantLine,
                old.ItemId, field == 1 ? 0 : old.OriginalQuantity, field == 2 ? -1 : old.AcquisitionOrder,
                field == 3 ? null : old.OriginalOperationId, field == 4 ? null : old.OriginalCommitId,
                field == 5 ? "changed-branch" : old.OriginalBranchId, field == 6 ? null : old.Inclusion);
            Assert.Throws<SaveCodecFailure>(() => CandidatePermanentInventory.CheckSource(changed, "p", Codec()));
            Assert.Throws<SaveCodecFailure>(() => CandidatePermanentInventory.CheckSource(old, "foreign-player", Codec()));
        }

        [TestCase(-1, 1)]
        [TestCase(0, 4)]
        [TestCase(3, 1)]
        [TestCase(1, 0)]
        public void CB05_InvalidUnitRangeDoesNotAlterHeldUnits(int start, int count)
        {
            var original = ExistingCard();
            var held = new List<CandidatePermanentPortion> { original };
            Assert.Throws<SaveCodecFailure>(() => CandidatePermanentInventory.Consume(held, original.Slice(start, count), Codec()));
            Assert.AreSame(original, held.Single());
        }

        [Test]
        public void CB06_RealOrdinaryConversionRetainsPredecessorsAndOnlyActualRemainingOutput()
        {
            using (var data = new CandidatePermanentTestData(cards: 0))
            {
                var first = data.Apply(new CandidatePermanentDraft { Kind = CandidatePermanentKind.Craft, DefinitionId = "weapons", Quantity = 1 });
                var second = data.Apply(new CandidatePermanentDraft { Kind = CandidatePermanentKind.Craft, DefinitionId = "cards", Quantity = 1 });
                Assert.AreEqual(first.OperationId, second.Intent.GetPermanent().Inputs.Single().OutputOperationId);
                var card = data.Apply(CandidatePermanentTestData.Card("W", 5));
                Assert.AreEqual(second.OperationId, card.Intent.GetPermanent().Inputs.Single().OutputOperationId);
                Assert.AreEqual(new BigInteger(4), data.Total("weapon"));
                Assert.AreEqual(BigInteger.Zero, data.Total("card"));
                data.Reopen();
                var endpoints = TakeCore(CandidatePermanentInventory.ReadEndpoints(data.Head, Codec()));
                var weapon = endpoints.Where(x => x.OutputOperationId == first.OperationId).ToArray();
                Assert.AreEqual(new BigInteger(2), weapon.Single(x => x.Endpoint == CandidatePermanentEndpoint.Transformed).UnitCount);
                Assert.AreEqual(new BigInteger(2), weapon.Single(x => x.Endpoint == CandidatePermanentEndpoint.Held).UnitStart);
                Assert.AreEqual(new BigInteger(4), weapon.Single(x => x.Endpoint == CandidatePermanentEndpoint.Held).UnitCount);
                Assert.AreEqual(card.OperationId, endpoints.Single(x => x.OutputOperationId == second.OperationId).EndpointOperationId);
                Assert.AreEqual(3, data.Head.Business.Inventory.GetPermanentLedger().Effects.Count);
            }
        }

        [Test]
        public void CB06_MixedOwnerOutputPreservesWholeInputsAndCannotBecomeAdvertisementCards()
        {
            using (var data = new CandidatePermanentTestData(cards: 0))
            {
                var basis = data.ExistingOwnerFixture("ore", 1);
                var held = CandidatePermanentInventory.Held(basis.Business, basis.Records, Codec());
                var advertisement = held.Single(x => x.Source?.Grant.Kind == CandidatePermanentSourceKind.ExistingAdvertisementHeld);
                var ordinary = held.Single(x => x.ItemId == "ore" && x.Source.Grant.Kind == CandidatePermanentSourceKind.OrdinaryBaseReward);
                var wood = held.Single(x => x.ItemId == "wood");
                var draft = new CandidatePermanentDraft { Kind = CandidatePermanentKind.Craft, DefinitionId = "mixedcards", Quantity = 1,
                    SelectedInputs = new[] { advertisement, ordinary.Slice(0, 2), wood.Slice(0, 1) } };
                var quote = TakeCore(CandidatePermanentProtocol.Preview(basis, data.Definitions, draft, Codec()));
                var next = TakeCore(CandidatePermanentProtocol.Build(basis, data.Definitions, quote, "fixture:mixed", Codec()));
                Assert.AreEqual(3, next.Inventory.GetPermanentLedger().Effects.Single().Quote.Inputs.Count);
                var continued = data.WithBusiness(next);
                Assert.AreEqual("UnsupportedSourceProof", CandidatePermanentProtocol.Preview(continued, data.Definitions,
                    CandidatePermanentTestData.Card("W", 5), Codec()).RejectionCode);
                var mixed = SaveCodecResult<bool>.Run(() => {
                    CandidatePermanentInventory.CheckOrdinaryOutput(continued.Business.Inventory,
                        continued.Business.Inventory.GetPermanentLedger().Effects.Last().OperationId, new HashSet<string>(), Codec());
                    return true;
                });
                Assert.AreEqual("UnsupportedSourceProof", mixed.RejectionCode);
                var endpoints = TakeCore(CandidatePermanentInventory.ReadEndpoints(continued, Codec()));
                Assert.AreEqual(CandidatePermanentEndpoint.Transformed,
                    endpoints.Single(x => x.Source?.Grant.Kind == CandidatePermanentSourceKind.ExistingAdvertisementHeld).Endpoint);
                Assert.Throws<SaveCodecFailure>(() => CandidatePermanentInventory.Consume(endpoints.ToList(), advertisement, Codec()));
            }
        }

        [Test]
        public void CB12_ExplicitAdjacentSlicesFreezeToOneCanonicalRangeAndOverlapsFail()
        {
            using (var data = new CandidatePermanentTestData())
            {
                var input = data.Preview(CandidatePermanentTestData.Card("M", 6)).Inputs.Single();
                var selected = new[] { input.Slice(0, 1), input.Slice(1, 2) };
                var draft = CandidatePermanentTestData.Card("M", 6);
                draft.SelectedInputs = selected;
                var quote = data.Preview(draft);
                Assert.AreEqual(1, quote.Inputs.Count);
                Assert.AreEqual(new BigInteger(3), quote.Inputs[0].UnitCount);
                selected[1] = input.Slice(0, 2);
                Assert.IsTrue(CandidatePermanentProtocol.ValidatePreview(data.Head, data.Definitions, quote, Codec()).IsAccepted);
                Assert.IsFalse(CandidatePermanentProtocol.Preview(data.Head, data.Definitions, draft, Codec()).IsAccepted);
                var held = CandidatePermanentInventory.Held(data.Head.Business, data.Head.Records, Codec());
                CandidatePermanentInventory.Consume(held, input.Slice(0, 2), Codec());
                Assert.Throws<SaveCodecFailure>(() => CandidatePermanentInventory.Consume(held, input.Slice(1, 2), Codec()));
            }
        }

        [TestCase(0)]
        [TestCase(1)]
        public void CB16_EffectReferenceRejectsWrongOwnerOrEffectRow(int field)
        {
            using (var stream = new MemoryStream())
            {
                new CandidatePermanentSaveCodec(new BusinessFields(stream, false, Codec())).PermanentEffectReference("original:op", 4, "ref");
                var bytes = stream.ToArray();
                bytes[field == 0 ? 0 : bytes.Length - 4] = 9;
                using (var changed = new MemoryStream(bytes))
                    Assert.Throws<SaveCodecFailure>(() => new CandidatePermanentSaveCodec(new BusinessFields(changed, true, Codec(),
                        (ulong)changed.Length)).PermanentEffectReference(null, 4, "ref"));
            }
        }

        [Test]
        public void CB16_ExistingRetractionReferenceRoundTripsButCannotReplaceMissingOriginalIndex()
        {
            var old = ExistingCard().Source;
            var source = new CandidatePermanentSourceLine(old.Grant, old.GrantLine, old.ItemId, old.OriginalQuantity,
                old.AcquisitionOrder, old.OriginalOperationId, old.OriginalCommitId, old.OriginalBranchId, old.Inclusion, "existing:retraction");
            using (var stream = new MemoryStream())
            {
                new CandidatePermanentCodec(new BusinessFields(stream, false, Codec())).Source(source, "source");
                stream.Position = 0;
                var restored = new CandidatePermanentCodec(new BusinessFields(stream, true, Codec(), (ulong)stream.Length)).Source(null, "source");
                Assert.AreEqual("existing:retraction", restored.RetractionOperationId);
                Assert.Throws<SaveCodecFailure>(() => CandidatePermanentInventory.CheckSource(restored, "p", Codec()));
            }
        }

        [Test]
        public void CB07_CraftingCannotBorrowAnotherCharactersLoadoutOrActiveCarry()
        {
            using (var data = new CandidatePermanentTestData())
            {
                data.Apply(new CandidatePermanentDraft { Kind = CandidatePermanentKind.Craft, DefinitionId = "weapons", Quantity = 1 });
                var equip = new CandidatePermanentDraft { Kind = CandidatePermanentKind.Equip, CharacterId = "W", DefinitionId = "weapon", Quantity = 6 };
                data.Apply(equip);
                var craft = new CandidatePermanentDraft { Kind = CandidatePermanentKind.Craft, DefinitionId = "cards", Quantity = 1 };
                Assert.AreEqual("InsufficientResources", CandidatePermanentProtocol.Preview(data.Head, data.Definitions, craft, Codec()).RejectionCode);
                equip.Quantity = 4;
                data.Apply(equip);
                data.Apply(craft);
                Assert.AreEqual(new BigInteger(4), data.Total("weapon"));
                Assert.AreEqual(new BigInteger(4), data.Head.Business.Inventory.FindLoadout("W").L);
                data.Commit(data.Entry());
                Assert.AreEqual("AttemptActive", CandidatePermanentProtocol.Preview(data.Head, data.Definitions, equip, Codec()).RejectionCode);
                Assert.AreEqual("InsufficientResources", CandidatePermanentProtocol.Preview(data.Head, data.Definitions, craft, Codec()).RejectionCode);
                Assert.AreEqual(new BigInteger(4), data.Head.Business.Inventory.FindLoadout("W").L);
            }
        }

        [Test]
        public void CB05_ConvertedOrdinaryCardsFollowOriginalM02AcquisitionOrder()
        {
            using (var data = new CandidatePermanentTestData())
            {
                data.Apply(new CandidatePermanentDraft { Kind = CandidatePermanentKind.Craft, DefinitionId = "weapons", Quantity = 1 });
                var conversion = data.Apply(new CandidatePermanentDraft { Kind = CandidatePermanentKind.Craft, DefinitionId = "cards", Quantity = 1 });
                var quote = data.Preview(CandidatePermanentTestData.Card("M", 6));
                Assert.AreEqual(1, quote.Inputs.Count);
                Assert.IsNotNull(quote.Inputs[0].Source);
                Assert.AreEqual(new BigInteger(3), quote.Inputs[0].UnitCount);
                data.Apply(CandidatePermanentTestData.Card("M", 6));
                var remaining = TakeCore(CandidatePermanentInventory.ReadEndpoints(data.Head, Codec()))
                    .Single(x => x.ItemId == "card" && x.Endpoint == CandidatePermanentEndpoint.Held);
                Assert.AreEqual(conversion.OperationId, remaining.OutputOperationId);
                Assert.AreEqual(BigInteger.One, remaining.UnitCount);
            }
        }

        [Test]
        public void CB07_AllRemainingMaterialSourcesHaveOneAllocationWithoutAnExtraChoice()
        {
            using (var data = new CandidatePermanentTestData())
            {
                var weapons = new CandidatePermanentDraft { Kind = CandidatePermanentKind.Craft, DefinitionId = "weapons", Quantity = 1 };
                data.Apply(weapons);
                data.Apply(weapons);
                var cards = new CandidatePermanentDraft { Kind = CandidatePermanentKind.Craft, DefinitionId = "cards", Quantity = 6 };
                var quote = data.Preview(cards);
                Assert.AreEqual(2, quote.Inputs.Count);
                data.Apply(cards);
                Assert.AreEqual(BigInteger.Zero, data.Total("weapon"));
                Assert.AreEqual(new BigInteger(9), data.Total("card"));
            }
        }

        [TestCase(1, 157, 1)]
        [TestCase(2, 85, 2)]
        public void C1_R1_ExplicitOrdinaryCardsCannotBypassHeldAdvertisement(int ordinary, int xp, int units)
        {
            using (var data = new CandidatePermanentTestData(cards: ordinary, warriorXp: xp))
            {
                var basis = data.ExistingOwnerFixture("card", 1);
                var before = Encode(data.Envelope);
                var draft = CandidatePermanentTestData.Card("W", 5);
                var canonical = TakeCore(CandidatePermanentProtocol.Preview(basis, data.Definitions, draft, Codec()));
                draft.SelectedInputs = new[] { CandidatePermanentInventory.Held(basis.Business, basis.Records, Codec())
                    .Single(x => x.ItemId == "card" && x.Source.Grant.Kind == CandidatePermanentSourceKind.OrdinaryBaseReward).Slice(0, units) };
                var forged = CandidatePermanentTestData.Requote(canonical, draft.SelectedInputs);
                Assert.IsFalse(CandidatePermanentProtocol.Preview(basis, data.Definitions, draft, Codec()).IsAccepted, "Preview priority");
                Assert.IsFalse(CandidatePermanentProtocol.ValidatePreview(basis, data.Definitions, forged, Codec()).IsAccepted, "Frozen priority");
                Assert.IsFalse(CandidatePermanentProtocol.Build(basis, data.Definitions, forged, "c1:bypass", Codec()).IsAccepted, "Build priority");
                CollectionAssert.AreEqual(before, Encode(data.Envelope));
            }
        }

        [Test]
        public void C1_R1_ExplicitNewerOrdinarySourceCannotBypassEarlierHeldCard()
        {
            using (var data = new CandidatePermanentTestData())
            {
                data.Farm();
                var draft = CandidatePermanentTestData.Card("W", 5);
                var canonical = data.Preview(draft);
                var newer = CandidatePermanentInventory.Held(data.Head.Business, data.Head.Records, Codec())
                    .Where(x => x.ItemId == "card").OrderByDescending(x => x.Source.AcquisitionOrder).First();
                draft.SelectedInputs = new[] { newer.Slice(0, 1) };
                var forged = CandidatePermanentTestData.Requote(canonical, draft.SelectedInputs);
                Assert.IsFalse(CandidatePermanentProtocol.Preview(data.Head, data.Definitions, draft, Codec()).IsAccepted);
                Assert.IsFalse(CandidatePermanentProtocol.Build(data.Head, data.Definitions, forged, "c1:newer", Codec()).IsAccepted);
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void C1_R1_CanonicalExplicitCardsAndAdjacentSplitsMatchDefault(bool split)
        {
            using (var data = new CandidatePermanentTestData())
            {
                var basis = data.ExistingOwnerFixture("card", 2);
                var draft = CandidatePermanentTestData.Card("M", 6);
                var canonical = TakeCore(CandidatePermanentProtocol.Preview(basis, data.Definitions, draft, Codec()));
                var advertisement = canonical.Inputs.Single(x => x.Source.Grant.Kind == CandidatePermanentSourceKind.ExistingAdvertisementHeld);
                draft.SelectedInputs = split ? new[] { advertisement.Slice(1, 1), canonical.Inputs.Last(), advertisement.Slice(0, 1) } : canonical.Inputs;
                var explicitQuote = TakeCore(CandidatePermanentProtocol.Preview(basis, data.Definitions, draft, Codec()));
                Assert.IsTrue(CandidatePermanentCodec.SameQuote(canonical, explicitQuote, Codec()));
                Assert.IsTrue(CandidatePermanentProtocol.ValidatePreview(basis, data.Definitions, explicitQuote, Codec()).IsAccepted);
                var next = TakeCore(CandidatePermanentProtocol.Build(basis, data.Definitions, explicitQuote, "c1:canonical", Codec()));
                Assert.AreEqual(new BigInteger(6), next.Roster.Find("M").Level);
                Assert.AreEqual(new BigInteger(22), next.Roster.Find("M").Experience);
                Assert.AreEqual(2, explicitQuote.Inputs.Count);
            }
        }

        [Test]
        public void C1_R2_ExplicitEmptyEquipPersistsNullStateAndRepeatsUnchanged()
        {
            using (var data = new CandidatePermanentTestData())
            {
                data.Apply(new CandidatePermanentDraft { Kind = CandidatePermanentKind.Craft, DefinitionId = "weapons", Quantity = 1 });
                data.Apply(new CandidatePermanentDraft { Kind = CandidatePermanentKind.Equip, CharacterId = "W", DefinitionId = "weapon", Quantity = 2 });
                var empty = new CandidatePermanentDraft { Kind = CandidatePermanentKind.Equip, CharacterId = "W", DefinitionId = null, Quantity = 0 };
                data.Apply(empty);
                data.Reopen();
                var loadout = data.Head.Business.Inventory.FindLoadout("W");
                Assert.IsNull(loadout.ItemId);
                Assert.AreEqual(BigInteger.Zero, loadout.L);
                Assert.IsNull(loadout.Enabled);
                var revision = data.Head.Business.Inventory.StateRevision;
                var preference = data.Head.Business.Inventory.PreferenceRevision;
                var effects = data.Head.Business.Inventory.GetPermanentLedger().Effects.Count;
                data.Apply(empty);
                Assert.AreEqual("Unchanged", data.Head.Records.Last().Result.GetPermanent().Outcome);
                Assert.AreEqual(revision, data.Head.Business.Inventory.StateRevision);
                Assert.AreEqual(preference, data.Head.Business.Inventory.PreferenceRevision);
                Assert.AreEqual(effects, data.Head.Business.Inventory.GetPermanentLedger().Effects.Count);
                data.Reopen();
                Assert.IsNull(data.Head.Business.Inventory.FindLoadout("W").Enabled);
                Assert.IsNull(data.Head.Business.Inventory.FindLoadout("W").ItemId);
            }
        }
    }
}
