using System.Linq;
using System.Numerics;
using NUnit.Framework;
using static FightMatch.Core.Tests.PlayerSessionTestData;

namespace FightMatch.Core.Tests
{
    [TestFixture]
    public sealed class CandidatePermanentProgressionTests
    {
        [Test]
        public void CB09_LevelClassSkillAndExplicitConfirmationAreRequired()
        {
            using (var data = new CandidatePermanentTestData())
            {
                var before = Encode(data.Envelope);
                Assert.AreEqual("LevelRequirement", CandidatePermanentProtocol.Preview(data.Head, data.Definitions, data.Learn("W"), Codec()).RejectionCode);
                var wrong = data.Learn("M");
                wrong.DefinitionId = "taunt";
                Assert.AreEqual("InconsistentBinding", CandidatePermanentProtocol.Preview(data.Head, data.Definitions, wrong, Codec()).RejectionCode);
                var preview = data.Preview(data.Learn("M"));
                Assert.AreEqual(1, preview.Costs.Count);
                Assert.AreEqual(BigInteger.One, data.Total("certificate"));
                CollectionAssert.AreEqual(before, Encode(data.Envelope));
                data.Apply(data.Learn("M"));
                Assert.AreEqual(BigInteger.Zero, data.Total("certificate"));
                Assert.IsNotNull(CandidatePermanentGrowth.FindLearning(data.Head.Business.Roster, "M", "spell"));
                data.Reopen();
                Assert.IsNotNull(CandidatePermanentGrowth.FindLearning(data.Head.Business.Roster, "M", "spell"));
            }
        }

        [Test]
        public void CB09_NewOperationAlreadyLearnedReferencesOriginalWithoutAnyOwnerRevision()
        {
            using (var data = new CandidatePermanentTestData())
            {
                var first = data.Apply(data.Learn("M"));
                var character = data.M.StateRevision;
                var inventory = data.Head.Business.Inventory.StateRevision;
                var progression = data.Head.Business.Progression.StateRevision;
                var second = data.Apply(data.Learn("M"));
                Assert.AreNotEqual(first.OperationId, second.OperationId);
                Assert.AreEqual("AlreadyLearned", data.Head.Records.Last().Result.GetPermanent().Outcome);
                Assert.AreEqual(first.OperationId, data.Head.Records.Last().Result.GetPermanent().OriginalLearningOperation);
                Assert.IsEmpty(second.Intent.GetPermanent().Costs);
                Assert.AreEqual(character, data.M.StateRevision);
                Assert.AreEqual(inventory, data.Head.Business.Inventory.StateRevision);
                Assert.AreEqual(progression, data.Head.Business.Progression.StateRevision);
            }
        }

        [Test]
        public void CB10_FirstCertificateCannotFundMageButSeparateOrdinaryCertificateCan()
        {
            using (var data = new CandidatePermanentTestData(certificates: 0, mageLevel: 6, mageXp: 0))
            {
                data.Gift();
                var before = Encode(data.Envelope);
                Assert.AreEqual("FirstCertificateReserved", CandidatePermanentProtocol.Preview(data.Head, data.Definitions, data.Learn("M"), Codec()).RejectionCode);
                CollectionAssert.AreEqual(before, Encode(data.Envelope));
                Assert.AreEqual(BigInteger.One, data.Total("certificate"));
            }
            using (var data = new CandidatePermanentTestData(mageLevel: 6, mageXp: 0))
            {
                var gift = data.Gift();
                var learned = data.Apply(data.Learn("M"));
                Assert.IsTrue(learned.Intent.GetPermanent().Inputs.All(x => x.OutputOperationId != gift.OperationId));
                Assert.AreEqual(BigInteger.One, data.Total("certificate"));
                Assert.IsNull(CandidatePermanentProgression.Find(data.Head.Business.Progression, CandidatePermanentKind.LearnSkill, "taunt"));
            }
        }

        [Test]
        public void CB11_GiftLearningAndExplanationHaveIndependentOnceFacts()
        {
            using (var data = new CandidatePermanentTestData(warriorLevel: 5, warriorXp: 0))
            {
                var first = data.Gift();
                var revision = data.Head.Business.Progression.StateRevision;
                var challenge = data.Head.Business.Progression.Challenges.Single(x => x.Level.LevelId == "test:L16");
                Assert.IsEmpty(challenge.Attempts);
                Assert.IsTrue(challenge.GetTeachingBinding().Same(first.Intent.GetPermanent().TeachingLevel));
                data.Gift();
                Assert.AreEqual(revision, data.Head.Business.Progression.StateRevision);
                Assert.AreEqual(new BigInteger(2), data.Total("certificate"));
                var learned = data.Apply(data.Learn("W"));
                Assert.AreEqual(first.OperationId, learned.Intent.GetPermanent().Inputs.Single().OutputOperationId);
                Assert.IsNotNull(CandidatePermanentProgression.Find(data.Head.Business.Progression, CandidatePermanentKind.LearnSkill, "taunt"));
                Assert.IsNull(CandidatePermanentProgression.Find(data.Head.Business.Progression, CandidatePermanentKind.ConfirmTeachingExplanation, "tutorial"));
                data.Explain();
                var amount = data.Total("certificate");
                revision = data.Head.Business.Progression.StateRevision;
                data.Explain();
                Assert.AreEqual(amount, data.Total("certificate"));
                Assert.AreEqual(revision, data.Head.Business.Progression.StateRevision);
                data.Reopen();
                Assert.AreEqual(BigInteger.One, data.Total("certificate"));
            }
        }

        [Test]
        public void CB11_InvalidStepAndUnknownTeachingDoNotProduceGiftOrLearning()
        {
            using (var data = new CandidatePermanentTestData(warriorLevel: 5, warriorXp: 0))
            {
                var before = data.Head;
                Assert.AreEqual("NoPublishedDefinition", CandidatePermanentProtocol.Preview(data.Head, data.Definitions,
                    new CandidatePermanentDraft { Kind = CandidatePermanentKind.BeginTeachingGift, DefinitionId = "missing" }, Codec()).RejectionCode);
                Assert.AreEqual("LearningUnavailable", CandidatePermanentProtocol.Preview(data.Head, data.Definitions,
                    new CandidatePermanentDraft { Kind = CandidatePermanentKind.ConfirmTeachingExplanation, DefinitionId = "tutorial", StepId = "explain" }, Codec()).RejectionCode);
                Assert.AreSame(before, data.Head);
                data.Gift();
                var invalid = data.Learn("W");
                invalid.StepId = "wrong";
                var request = CandidatePermanentProtocol.Preview(data.Head, data.Definitions, invalid, Codec());
                Assert.IsFalse(request.IsAccepted);
            }
        }

        [Test]
        public void CB10_CB16_RestoreHistoryCannotOmitTeachingBindingAfterOriginalGift()
        {
            using (var data = new CandidatePermanentTestData(warriorLevel: 5, warriorXp: 0))
            {
                data.Gift();
                var quote = data.Preview(data.Learn("W"));
                var altered = CandidatePermanentTestData.Requote(quote, omitTeaching: true);
                var prior = data.Head.Records.Select(x => (x.OperationId, x.Intent.GetPermanent())).ToArray();
                var original = SaveCodecResult<bool>.Run(() => {
                    CandidatePermanentProgression.CheckHistory(quote, prior, data.Definitions.GetPermanentDefinitions());
                    return true;
                });
                Assert.IsTrue(original.IsAccepted);
                var rejected = SaveCodecResult<bool>.Run(() => {
                    CandidatePermanentProgression.CheckHistory(altered, prior, data.Definitions.GetPermanentDefinitions());
                    return true;
                });
                Assert.AreEqual("TeachingRequired", rejected.RejectionCode);
                Assert.AreEqual(new BigInteger(2), data.Total("certificate"));
            }
        }

        [Test]
        public void C1_R1_WarriorExplicitOrdinaryCannotBypassFirstTeachingCertificate()
        {
            using (var data = new CandidatePermanentTestData(warriorLevel: 5, warriorXp: 0))
            {
                data.Gift();
                var draft = data.Learn("W");
                var canonical = data.Preview(draft);
                draft.SelectedInputs = new[] { CandidatePermanentInventory.Held(data.Head.Business, data.Head.Records, Codec())
                    .Single(x => x.ItemId == "certificate" && x.Source != null).Slice(0, 1) };
                var forged = CandidatePermanentTestData.Requote(canonical, draft.SelectedInputs);
                var before = Encode(data.Envelope);
                Assert.IsFalse(CandidatePermanentProtocol.Preview(data.Head, data.Definitions, draft, Codec()).IsAccepted, "Preview first certificate");
                Assert.IsFalse(CandidatePermanentProtocol.ValidatePreview(data.Head, data.Definitions, forged, Codec()).IsAccepted, "Frozen first certificate");
                Assert.IsFalse(CandidatePermanentProtocol.Build(data.Head, data.Definitions, forged, "c1:ordinary-first", Codec()).IsAccepted, "Build first certificate");
                CollectionAssert.AreEqual(before, Encode(data.Envelope));
            }
        }

        [TestCase("W")]
        [TestCase("M")]
        public void C1_R1_CanonicalWarriorGiftAndIndependentMageOrdinarySelectionRemainLegal(string character)
        {
            using (var data = new CandidatePermanentTestData(warriorLevel: 5, warriorXp: 0))
            {
                var gift = data.Gift();
                var draft = data.Learn(character);
                var canonical = data.Preview(draft);
                draft.SelectedInputs = canonical.Inputs;
                var explicitQuote = data.Preview(draft);
                Assert.IsTrue(CandidatePermanentCodec.SameQuote(canonical, explicitQuote, Codec()));
                var selected = explicitQuote.Inputs.Single();
                if (character == "W") Assert.AreEqual(gift.OperationId, selected.OutputOperationId);
                else Assert.IsNotNull(selected.Source);
                data.Apply(draft);
                data.Reopen();
                Assert.AreEqual(BigInteger.One, data.Total("certificate"));
                Assert.IsNotNull(CandidatePermanentGrowth.FindLearning(data.Head.Business.Roster, character, draft.DefinitionId));
            }
        }

        [Test]
        public void C1_R4_AlreadyLearnedRestoresOriginalReferenceWithoutCostsOrOwnerEffects()
        {
            using (var data = new CandidatePermanentTestData(certificates: 2))
            {
                var original = data.Apply(data.Learn("M"));
                var before = data.Head.Business;
                var repeat = data.Apply(data.Learn("M"));
                var quote = repeat.Intent.GetPermanent();
                var result = data.Head.Records.Last().Result.GetPermanent();
                Assert.AreEqual("AlreadyLearned", result.Outcome);
                Assert.AreEqual(original.OperationId, result.OriginalLearningOperation);
                Assert.IsEmpty(quote.Inputs);
                Assert.IsEmpty(quote.Costs);
                Assert.IsEmpty(quote.Outputs);
                Assert.IsNull(result.CharacterEffectOperation);
                Assert.IsNull(result.InventoryEffectOperation);
                Assert.IsNull(result.ProgressionEffectOperation);
                data.Reopen();
                Assert.AreEqual(original.OperationId, data.Head.Records.Last().Result.GetPermanent().OriginalLearningOperation);
                Assert.AreEqual(before.Roster.Find("M").StateRevision, data.M.StateRevision);
                Assert.AreEqual(before.Inventory.StateRevision, data.Head.Business.Inventory.StateRevision);
                Assert.AreEqual(before.Progression.StateRevision, data.Head.Business.Progression.StateRevision);
                Assert.AreEqual(before.Roster.GetPermanentEffects().Count, data.Head.Business.Roster.GetPermanentEffects().Count);
                Assert.AreEqual(before.Inventory.GetPermanentLedger().Effects.Count, data.Head.Business.Inventory.GetPermanentLedger().Effects.Count);
                Assert.AreEqual(before.Progression.GetPermanentEffects().Count, data.Head.Business.Progression.GetPermanentEffects().Count);
                Assert.AreEqual(BigInteger.One, data.Total("certificate"));
            }
        }

        [Test]
        public void C1_R1_R4_OriginalLearningCanCompleteTeachingAndReleaseCertificatePurpose()
        {
            using (var data = new CandidatePermanentTestData(certificates: 2, warriorLevel: 5, warriorXp: 0))
            {
                var original = data.Apply(data.Learn("W"));
                var gift = data.Gift();
                var inventory = data.Head.Business.Inventory.StateRevision;
                var character = data.W.StateRevision;
                var reference = data.Apply(data.Learn("W"));
                Assert.AreEqual(original.OperationId, reference.Intent.GetPermanent().OriginalLearningOperation);
                Assert.IsEmpty(reference.Intent.GetPermanent().Costs);
                Assert.AreEqual(inventory, data.Head.Business.Inventory.StateRevision);
                Assert.AreEqual(character, data.W.StateRevision);
                data.Explain();
                var mage = data.Learn("M");
                mage.SelectedInputs = new[] { CandidatePermanentInventory.Held(data.Head.Business, data.Head.Records, Codec())
                    .Single(x => x.ItemId == "certificate" && x.Source != null).Slice(1, 1) };
                data.Apply(mage);
                data.Reopen();
                Assert.AreEqual(BigInteger.One, data.Total("certificate"));
                Assert.IsTrue(CandidatePermanentInventory.Held(data.Head.Business, data.Head.Records, Codec())
                    .Any(x => x.OutputOperationId == gift.OperationId));
                Assert.AreEqual(original.OperationId, CandidatePermanentProgression.Find(data.Head.Business.Progression,
                    CandidatePermanentKind.LearnSkill, "taunt").RelatedOperationId);
            }
        }
    }
}
