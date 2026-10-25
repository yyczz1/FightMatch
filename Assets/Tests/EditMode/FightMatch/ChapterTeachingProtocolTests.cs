using System;
using System.Linq;
using FightMatch.Application;
using NUnit.Framework;
using Data = FightMatch.Core.Tests.PlayerSessionTestData;

namespace FightMatch.Core.Tests
{
    public sealed class ChapterTeachingProtocolTests
    {
        [Test]
        public void GiftReservationAppliesToItsExactHeldOutputAndOrdinaryCertificatesRemainUsable()
        {
            using (var fixture = new CandidatePermanentTestData(warriorLevel: 5, warriorXp: 0))
            {
                var gift = fixture.Gift();
                var mage = fixture.Preview(fixture.Learn("M"));
                Assert.AreEqual(1, mage.Inputs.Count);
                Assert.NotNull(mage.Inputs[0].Source, "An ordinary certificate is not the reserved entry gift.");
                Assert.IsNull(mage.Inputs[0].OutputOperationId);
                var warrior = fixture.Preview(fixture.Learn("W"));
                Assert.AreEqual(gift.OperationId, warrior.Inputs.Single().OutputOperationId);
                Assert.AreEqual(CandidatePermanentEndpoint.Held, warrior.Inputs.Single().Endpoint);
                var forgedChoice = fixture.Learn("M");
                forgedChoice.SelectedInputs = warrior.Inputs;
                Assert.IsFalse(CandidatePermanentProtocol.Preview(fixture.Head, fixture.Definitions, forgedChoice, Data.Codec()).IsAccepted);
            }
        }

        [Test]
        public void AlreadyLearnedUsesOriginalLearningAndDoesNotSpendTheEntryGift()
        {
            using (var fixture = new CandidatePermanentTestData(warriorLevel: 5, warriorXp: 0))
            {
                var original = fixture.Apply(fixture.Learn("W"));
                var gift = fixture.Gift();
                var quote = fixture.Preview(fixture.Learn("W"));
                Assert.AreEqual(original.OperationId, quote.OriginalLearningOperation);
                Assert.IsEmpty(quote.Inputs); Assert.IsEmpty(quote.Costs);
                fixture.Apply(fixture.Learn("W"));
                Assert.AreEqual(1, fixture.Head.Business.Roster.GetPermanentEffects().Count(x => x.Quote.Kind == CandidatePermanentKind.LearnSkill));
                Assert.IsNull(fixture.Head.Business.Progression.GetPermanentEffects().FirstOrDefault(x => x.Quote.Kind == CandidatePermanentKind.ConfirmTeachingExplanation));
                fixture.Explain();
                var held = Data.TakeCore(CandidatePermanentInventory.ReadEndpoints(fixture.Head, Data.Codec()));
                Assert.IsTrue(held.Any(x => x.OutputOperationId == gift.OperationId && x.Endpoint == CandidatePermanentEndpoint.Held));
            }
        }

        [Test]
        public void CardQuantityMeansTargetLevelAndQuoteCarriesActualCostAndGrowth()
        {
            using (var fixture = new CandidatePermanentTestData())
            {
                var quote = fixture.Preview(CandidatePermanentTestData.Card("W", 5));
                Assert.AreEqual(5, (int)quote.Quantity.Value);
                Assert.AreEqual(4, (int)quote.BeforeLevel);
                Assert.AreEqual(157, (int)quote.BeforeExperience);
                Assert.AreEqual(quote.Costs.Single().Quantity * quote.UnitExperience, quote.FixedExperience);
                Assert.GreaterOrEqual(quote.FinalLevel, quote.Quantity.Value);
                Assert.AreEqual("card", quote.Costs.Single().ItemId);
                Assert.AreEqual(4, (int)fixture.W.Level, "Preview does not apply growth.");
            }
        }

        [Test]
        public void ChapterGiftIsOnceAndLearningAndExplanationAreIndependentAdmissionFacts()
        {
            var fixture = new ChapterTeachingFixture(initiallyOpenBoss: true);
            using (var rig = new ChapterTeachingRig(fixture))
            {
                var definitions = fixture.Publication.Definitions;
                var before = rig.Head;
                var missing = CandidatePermanentProgression.CheckTeachingAdmission(before.Business, rig.LevelBinding, definitions, Data.Codec());
                Assert.AreEqual("TeachingGiftRequired", missing.RejectionCode);
                Assert.IsNull(before.Business.ActiveHistory);
                rig.Gift();
                var gift = rig.Head.Business.Progression.GetPermanentEffects().Single(x => x.Quote.Kind == CandidatePermanentKind.BeginTeachingGift);
                rig.Gift();
                Assert.AreEqual(1, rig.Head.Business.Progression.GetPermanentEffects().Count(x => x.Quote.Kind == CandidatePermanentKind.BeginTeachingGift));
                Assert.AreEqual("TeachingLearningRequired", CandidatePermanentProgression.CheckTeachingAdmission(rig.Head.Business,
                    rig.LevelBinding, definitions, Data.Codec()).RejectionCode);
                rig.Learn();
                Assert.AreEqual("TeachingExplanationRequired", CandidatePermanentProgression.CheckTeachingAdmission(rig.Head.Business,
                    rig.LevelBinding, definitions, Data.Codec()).RejectionCode);
                rig.Explain();
                Assert.IsTrue(CandidatePermanentProgression.CheckTeachingAdmission(rig.Head.Business, rig.LevelBinding, definitions, Data.Codec()).IsAccepted);
                Assert.IsNull(rig.Head.Business.ActiveHistory);
                Assert.IsEmpty(rig.Head.Business.Progression.Challenges.Single(x => x.ChallengeId == gift.OperationId).Attempts);
            }
        }

        [Test]
        public void StableGiftKeyResolvesOriginalBindingAndRejectsMissingOriginalDefinition()
        {
            var fixture = new ChapterTeachingFixture(initiallyOpenBoss: true);
            using (var rig = new ChapterTeachingRig(fixture))
            {
                rig.Gift();
                var oldDefinitions = fixture.Publication.Definitions;
                var old = oldDefinitions.GetPermanentDefinitions().Find(CandidatePermanentDefinitionKind.Teaching, "tutorial:l16-taunt");
                var binding = Data.TakeCore(ContentBinding.Prepare(oldDefinitions.Binding.PackageId, new string('f', 64),
                    oldDefinitions.Binding.RuleVersion, oldDefinitions.Binding.NumericContractVersion, oldDefinitions.Binding.RandomContractVersion, Data.Codec()));
                var level = Data.TakeCore(DefinitionBinding.Prepare(binding, old.Level.LevelId, 1, Data.Codec()));
                var next = Data.TakeCore(CandidatePermanentDefinitions.Prepare(binding, new[] { new CandidatePermanentDefinitionInput {
                    Kind = CandidatePermanentDefinitionKind.Teaching, Id = old.Id, RecordVersion = old.RecordVersion + 1,
                    ClassId = old.ClassId, ItemId = old.ItemId, SkillId = old.SkillId, Level = level,
                    LearningStepId = old.LearningStepId, ExplanationStepId = old.ExplanationStepId, GiftOnceKey = old.GiftOnceKey } }, Data.Codec()));
                var teaching = next.Find(CandidatePermanentDefinitionKind.Teaching, old.Id);
                var closure = Data.TakeCore(PublishedSaveContext.Prepare(new[] { oldDefinitions }, Data.Codec()));
                var found = CandidatePermanentProgression.FindTeaching(rig.Head.Business.Progression,
                    CandidatePermanentKind.BeginTeachingGift, teaching, next, closure);
                Assert.NotNull(found); Assert.IsTrue(found.Quote.Binding.Same(oldDefinitions.Binding));
                var missing = SaveCodecResult<bool>.Run(() => {
                    CandidatePermanentProgression.FindTeaching(rig.Head.Business.Progression,
                        CandidatePermanentKind.BeginTeachingGift, teaching, next, null); return true; });
                Assert.AreEqual("UnsupportedSourceProof", missing.RejectionCode);
                Assert.IsTrue(rig.Head.Business.Progression.Definition.Context.ContentFingerprint == oldDefinitions.Binding.ContentFingerprint,
                    "Resolving a stable fact does not migrate the player binding.");
            }
        }
    }
}
