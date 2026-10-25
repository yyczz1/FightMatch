using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FightMatch.Content;
using NUnit.Framework;
using static FightMatch.Core.Tests.PlayerSessionTestData;

namespace FightMatch.Core.Tests
{
    public sealed class ChapterSaveCompatibilityTests
    {
        private PublishedRuleDefinitions chapter;
        private PublishedSaveContext closure;

        [OneTimeSetUp]
        public void PrepareChapterDefinitions()
        {
            chapter = ChapterContentTestData.Prepare().Definitions;
            closure = TakeCore(PublishedSaveContext.Prepare(new[] { chapter }, Codec()));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void LegacyActiveVersionsKeepTheirOriginalBindingAndWireLayout(bool legacy)
        {
            var fixture = new PlayerRosterTestData.IsolatedRoster(legacy: legacy);
            fixture.Commit(fixture.EntryRequest());
            var original = Bytes(fixture.Envelope);
            var restored = TakeCore(CandidateApplicationSaveCodec.DecodePublished(fixture.Envelope, fixture.Closure, Codec()));
            Assert.That(restored.Business.Format, NUnit.Framework.Is.EqualTo(legacy ? CandidateBusinessFormat.PublishedV2 : CandidateBusinessFormat.PublishedRosterV3));
            Assert.That(restored.Business.ActiveHistory.CurrentRun.Baseline.Entry.Context.RuleVersion, NUnit.Framework.Is.EqualTo(fixture.Context.RuleVersion));
            var five = TakeCore(CandidateBusinessSaveCodec.EncodePublished(restored.Business, restored.Header, fixture.Closure, Codec()));
            var again = TakeCore(CandidateBusinessSaveCodec.DecodePublished(five, fixture.Closure, Codec()));
            CollectionAssert.AreEqual(Bytes(five), Bytes(TakeCore(CandidateBusinessSaveCodec.EncodePublished(again, restored.Header, fixture.Closure, Codec()))));
            var missingOriginal = CandidateApplicationSaveCodec.DecodePublished(fixture.Envelope, closure, Codec());
            Assert.That(missingOriginal.IsAccepted, NUnit.Framework.Is.False);
            Assert.That(missingOriginal.RejectionCode, NUnit.Framework.Is.EqualTo("UnsupportedBinding"));
            CollectionAssert.AreEqual(original, Bytes(fixture.Envelope));
        }

        [Test]
        public void PermanentV4RetainsRewardsAndFormatWithoutAutomaticMigration()
        {
            using (var fixture = new CandidatePermanentTestData())
            {
                var original = Bytes(fixture.Envelope);
                var restored = TakeCore(CandidateApplicationSaveCodec.DecodePublished(fixture.Envelope, fixture.Closure, Codec()));
                Assert.That(restored.Business.Format, NUnit.Framework.Is.EqualTo(CandidateBusinessFormat.PublishedPermanentV4));
                Assert.That(restored.Business.Rewards.BaseRewards.Count, NUnit.Framework.Is.EqualTo(fixture.Head.Business.Rewards.BaseRewards.Count));
                Assert.That(restored.Business.Progression.GetChapterEffects(), NUnit.Framework.Is.Empty);
                CollectionAssert.AreEqual(new uint[] { 4, 4, 4, 4, 2, 2 }, fixture.Envelope.RequiredSliceContracts.Select(x => x.SchemaVersion));
                CollectionAssert.AreEqual(original, Bytes(fixture.Envelope));
            }
        }

        [Test]
        public void ChapterV5UsesOneVersionAcrossAllOwnersAndRestoresMageDefinition()
        {
            var created = CreateChapter();
            CollectionAssert.AreEqual(new uint[] { 5, 5, 5, 5, 5, 5 }, created.envelope.RequiredSliceContracts.Select(x => x.SchemaVersion));
            var restored = TakeCore(CandidateApplicationSaveCodec.DecodePublished(created.envelope, closure, Codec()));
            Assert.That(restored.Business.Format, NUnit.Framework.Is.EqualTo(CandidateBusinessFormat.PublishedChapterV5));
            var mage = restored.Business.Roster.Find("M");
            Assert.That(mage.Definition.ClassKind, NUnit.Framework.Is.EqualTo(CharacterClassKind.Mage));
            Assert.That(mage.Definition.PenetrationBase, NUnit.Framework.Is.Not.Null);
            Assert.That(mage.Definition.CritBase, NUnit.Framework.Is.Null);
            Assert.That(restored.Business.Progression.GetChapterEffects(), NUnit.Framework.Is.Empty);
            CollectionAssert.AreEqual(created.intent.CanonicalBytes, restored.Records[0].Intent.CanonicalBytes);
            var five = TakeCore(CandidateBusinessSaveCodec.EncodePublished(restored.Business, restored.Header, closure, Codec()));
            var again = TakeCore(CandidateBusinessSaveCodec.DecodePublished(five, closure, Codec()));
            CollectionAssert.AreEqual(Bytes(five), Bytes(TakeCore(CandidateBusinessSaveCodec.EncodePublished(again, restored.Header, closure, Codec()))));
        }

        [TestCase("mixed")]
        [TestCase("unknown-version")]
        [TestCase("missing-capability")]
        [TestCase("unknown-capability")]
        public void ChapterV5RejectsMixedContractsAndCapabilityDrift(string defect)
        {
            var created = CreateChapter(); var envelope = created.envelope;
            var original = Bytes(envelope);
            var slices = envelope.SliceDirectory.Select((row, i) => new SaveSliceInput {
                Contract = new RequiredSliceContract(row.Contract.SliceId, row.Contract.OwnerId,
                    defect == "mixed" && i == 4 ? 2U : defect == "unknown-version" && i == 0 ? 99U : row.Contract.SchemaVersion),
                Bytes = envelope.SliceBytes[i].ToArray(), Requirements = row.Requirements }).ToArray();
            if (defect.EndsWith("capability", StringComparison.Ordinal))
            {
                var old = slices[0].Requirements;
                var features = defect == "missing-capability" ? old.FeatureIds.Where(x => x != "CH1.BusinessV5").ToArray()
                    : old.FeatureIds.Concat(new[] { "CH1.Unknown" }).ToArray();
                slices[0].Requirements = new SaveRequirements(old.Bindings, old.RuleVersions, old.NumericContractVersions, old.RandomContractVersions, features);
            }
            var altered = TakeCore(SaveEnvelopeCodec.Prepare(new SaveEnvelopeInput { Purpose = envelope.Purpose,
                PlayerId = envelope.PlayerId, SaveGeneration = envelope.SaveGeneration, CommitId = envelope.CommitId,
                ParentCommitId = envelope.ParentCommitId, CommitIndex = envelope.CommitIndex,
                RequiredSliceContracts = slices.Select(x => x.Contract).ToArray(), Slices = slices }, Codec()));
            Assert.That(CandidateApplicationSaveCodec.DecodePublished(altered, closure, Codec()).IsAccepted, NUnit.Framework.Is.False);
            CollectionAssert.AreEqual(original, Bytes(envelope));
        }

        [Test]
        public void ChapterCreateRecordFreezesV5IntentAndDoesNotEnableV4CreateRecord()
        {
            var created = CreateChapter();
            var record = TakeCore(PlayerProfileCreateRecordCodec.Freeze(created.intent, "new:chapter", 1, new byte[] { 1, 2, 3 },
                Array.Empty<PlayerProfileCreateMaterial>(), Codec()));
            Assert.That(record.RecordFormatVersion, NUnit.Framework.Is.EqualTo(4));
            Assert.That(record.IntentFormatVersion, NUnit.Framework.Is.EqualTo(5));
            var bytes = TakeCore(PlayerProfileCreateRecordCodec.Write(record, Codec()));
            var decoded = TakeCore(PlayerProfileCreateRecordCodec.Read(bytes, Codec()));
            CollectionAssert.AreEqual(created.intent.CanonicalBytes, TakeCore(PlayerProfileCreateRecordCodec.RestoreIntent(decoded, Codec())).CanonicalBytes);
            var input = InitialInput(); input.FormatVersion = 4;
            var v4 = TakeCore(CandidateApplicationProtocol.PrepareIntent(input, Codec()));
            Assert.That(PlayerProfileCreateRecordCodec.Freeze(v4, "new:chapter", 1, new byte[] { 1 },
                Array.Empty<PlayerProfileCreateMaterial>(), Codec()).RejectionCode, NUnit.Framework.Is.EqualTo("UnsupportedCreateRecord"));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void ExplicitMigrationChecksOriginalDescriptorAndRequiresVerifiedMapping(bool stale)
        {
            using (var fixture = new CandidatePermanentTestData(farm: false))
            {
                var before = fixture.Head; var sourceBytes = Bytes(fixture.Envelope);
                var input = new CandidateApplicationIntentInput { PlayerId = before.Business.PlayerId, OperationId = "chapter:migrate",
                    ExpectedCommitId = before.Header.CommitId, Kind = CandidateApplicationKind.MigrateChapter, FormatVersion = 5,
                    Context = new PublishedRuleContext(chapter.Binding) };
                input.SetChapterMigration(new CandidateRosterMigrationInput { SourceGeneration = before.Header.SaveGeneration,
                    SourceDescriptorLength = before.Descriptor.TotalLength + (stale ? 1UL : 0UL), SourceDescriptorSha256 = before.Descriptor.Sha256 });
                var intent = TakeCore(CandidateApplicationProtocol.PrepareIntent(input, Codec()));
                var result = CandidateApplicationProtocol.Propose(before, before.Business, intent, new CandidateApplicationResultInput(), null, Codec());
                Assert.That(result.IsAccepted, NUnit.Framework.Is.False);
                Assert.That(result.RejectionCode, NUnit.Framework.Is.EqualTo(stale ? "StaleContext" : "MigrationMappingRequired"));
                CollectionAssert.AreEqual(sourceBytes, Bytes(fixture.Envelope));
                Assert.That(before.Business.Format, NUnit.Framework.Is.EqualTo(CandidateBusinessFormat.PublishedPermanentV4));
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void ExplicitMigrationRejectsActiveAndPendingSettlementBeforeAnyChange(bool pendingSettlement)
        {
            using (var fixture = new CandidatePermanentTestData(farm: false))
            {
                fixture.Commit(fixture.Entry());
                if (pendingSettlement) { fixture.Attack(0); fixture.Attack(1); }
                var before = fixture.Head; var original = Bytes(fixture.Envelope);
                var input = new CandidateApplicationIntentInput { PlayerId = before.Business.PlayerId, OperationId = "chapter:migrate-active",
                    ExpectedCommitId = before.Header.CommitId, Kind = CandidateApplicationKind.MigrateChapter, FormatVersion = 5,
                    Context = new PublishedRuleContext(chapter.Binding) };
                input.SetChapterMigration(new CandidateRosterMigrationInput { SourceGeneration = before.Header.SaveGeneration,
                    SourceDescriptorLength = before.Descriptor.TotalLength, SourceDescriptorSha256 = before.Descriptor.Sha256 });
                var intent = TakeCore(CandidateApplicationProtocol.PrepareIntent(input, Codec()));
                var result = CandidateApplicationProtocol.Propose(before, before.Business, intent, new CandidateApplicationResultInput(), null, Codec());
                Assert.That(result.RejectionCode, NUnit.Framework.Is.EqualTo("ActiveAttemptConflict"));
                CollectionAssert.AreEqual(original, Bytes(fixture.Envelope));
            }
        }

        private CandidateApplicationIntentInput InitialInput()
        {
            return new CandidateApplicationIntentInput { PlayerId = "chapter:save", OperationId = "chapter:create",
                Kind = CandidateApplicationKind.InitializeProfile, FormatVersion = 5, Context = new PublishedRuleContext(chapter.Binding),
                RosterInitialize = new CandidateRosterInitializeInput {
                    Characters = new[] { "M", "W" }.Select(id => new CandidateApplicationInitializeInput { CharacterId = id,
                        ClassId = chapter.Growths.Single(x => x.ClassKind == (id == "M" ? CharacterClassKind.Mage : CharacterClassKind.Warrior)).ClassId,
                        InitialLevel = 1, InitialExperience = 0, OriginalSlot = 0 }).ToArray(), Slots = new[] { "W", "M", null } } };
        }
        private (SaveEnvelope envelope, PreparedCandidateApplicationIntent intent) CreateChapter()
        {
            var input = InitialInput(); var budget = Codec();
            var intent = TakeCore(CandidateApplicationProtocol.PrepareIntent(input, budget));
            var characters = input.RosterInitialize.Characters.Select(x => CandidateCharacterGrowth.CreateCandidate(chapter.FindGrowth(x.ClassId),
                input.PlayerId, x.CharacterId, x.InitialLevel.Value, x.InitialExperience.Value, x.OriginalSlot.Value, budget.Math).Next).ToArray();
            var roster = TakeCore(CandidateRosterState.Create(input.PlayerId, characters, input.RosterInitialize.Slots, budget));
            var inventory = TakeCore(CandidateInventory.CreateCandidate(chapter.Inventory, roster, budget));
            var progression = CandidateProgression.CreateCandidate(chapter.Progression, input.PlayerId, budget.Math);
            Assert.That(progression.IsAccepted, NUnit.Framework.Is.True, progression.FieldPath);
            var rewards = CandidateBaseRewards.CreateCandidate(input.PlayerId, budget.Math);
            Assert.That(rewards.IsAccepted, NUnit.Framework.Is.True, rewards.FieldPath);
            var business = TakeCore(CandidateBusinessSaveCodec.Prepare(new CandidateBusinessInput(input.PlayerId, roster, inventory,
                progression.Next, rewards.Next, null, Array.Empty<CandidateBattleRun>(), Array.Empty<CandidateRollbackRecord>(),
                CandidateBusinessFormat.PublishedChapterV5), SavePurpose.PlayerSave, budget));
            var proposed = TakeCore(CandidateApplicationProtocol.Propose(null, business, intent, new CandidateApplicationResultInput(), null, budget));
            return (TakeCore(CandidateApplicationSaveCodec.EncodePublished(proposed, ApplicationScenario.Header(null, intent.OperationId), closure, budget)), intent);
        }
        private static byte[] Bytes(SaveEnvelope envelope)
        {
            using (var stream = new MemoryStream())
            { TakeCore(SaveEnvelopeCodec.Write(stream, envelope, Codec())); return stream.ToArray(); }
        }
    }
}
