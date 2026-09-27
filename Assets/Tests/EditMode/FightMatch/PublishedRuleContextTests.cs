using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using NUnit.Framework;
using static FightMatch.Core.Tests.SharedRuleFixture;

namespace FightMatch.Core.Tests
{
    public class PublishedRuleContextTests
    {
        [Test]
        public void CandidateAndPublishedCopiesPreserveTheirOwnIdentityAndFreezeMutableInputs()
        {
            var raw = Candidate();
            var copy = (CandidateContext)RuleContextChecks.Copy(raw);
            var prepared = Take(RuleContextChecks.Prepare(raw, Budget()));
            raw.SourceNotes[0] = "changed"; raw.DraftId = "changed";
            Assert.AreEqual("s", prepared.SourceNotes[0]); Assert.AreEqual("d", prepared.DraftId);
            Assert.AreEqual("s", copy.SourceNotes[0]); Assert.AreEqual("d", copy.DraftId);
            var restored = (CandidateContext)RuleContextChecks.Copy(prepared);
            restored.SourceNotes.Clear();
            Assert.AreEqual(1, prepared.SourceNotes.Count);
            var formal = Take(RuleContextChecks.Prepare(new PublishedRuleContext(Binding()), Budget()));
            var formalCopy = RuleContextChecks.Copy(formal);
            Assert.IsInstanceOf<PublishedRuleContext>(formalCopy);
            Assert.IsTrue(RuleContextChecks.Same(formal, formalCopy));
            Assert.IsFalse(RuleContextChecks.Same(formal, prepared));
            Assert.Throws<NotSupportedException>(() => { var unused = formal.DraftId; });
            Assert.Throws<NotSupportedException>(() => formalCopy.ContentFingerprint = "other");
            Assert.IsTrue(RuleContextChecks.CheckBudget(formal, Budget()).IsAccepted);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void EachContentFieldParticipatesInIdentityAndPublishedEntryAdmission(int field)
        {
            var a = Binding(); var b = Binding(field);
            Assert.IsFalse(a.Same(b));
            Assert.IsFalse(RuleContextChecks.Same(new PublishedRuleContext(a), new PublishedRuleContext(b)));
            var original = Take(DefinitionBinding.Prepare(a, "test:L1", 1, Budget()));
            Assert.IsFalse(original.Same(Take(DefinitionBinding.Prepare(b, "test:L1", 1, Budget()))));
            var input = RawEntry();
            var resolved = new BattleEntryPreparer().PrepareCandidate(input, Math()).Entry;
            input.Context = new PublishedRuleContext(b); input.Members[0].StatsContext = input.Context;
            var result = new BattleEntryPreparer().PreparePublished(input, original, resolved.Level, Math());
            Assert.IsFalse(result.IsAccepted); Assert.AreEqual(BattleEntryRejectionCode.InconsistentBinding, result.RejectionCode);
            Assert.AreEqual("Binding.Content", result.FieldPath);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        public void EveryPublishedIdentityFieldRejectsMissingOrBrokenUnicode(int index)
        {
            // Build ill-formed UTF-16 at runtime; metadata attribute strings replace lone surrogates.
            var invalid = new[] { null, "", " ", new string((char)0xD800, 1), new string((char)0xDC00, 1), "x" + (char)0xD800 + "y" }[index];
            for (var field = 0; field < 5; field++)
            {
                var values = new[] { "p", "c", "r", "n", "q" }; values[field] = invalid;
                Assert.IsFalse(ContentBinding.Prepare(values[0], values[1], values[2], values[3], values[4], Budget()).IsAccepted);
            }
            Assert.IsFalse(DefinitionBinding.Prepare(Binding(), invalid, 1, Budget()).IsAccepted);
        }

        [Test]
        public void LegalReplacementCharacterAndSurrogatePairRoundTripWithoutLoss()
        {
            const string value = "中\uFFFD\U0001F600";
            var binding = Take(ContentBinding.Prepare(value, value, value, value, value, Budget()));
            var input = SharedRuleContinuityTests.SimpleIntent(new PublishedRuleContext(binding));
            var prepared = Take(CandidateApplicationProtocol.PrepareIntent(input, Budget()));
            var decoded = CandidateApplicationIntentCodec.Read(new List<byte>(prepared.CanonicalBytes).ToArray(), Budget());
            var context = (PreparedPublishedRuleContext)decoded.Context;
            Assert.AreEqual(value, context.Binding.PackageId); Assert.IsTrue(binding.Same(context.Binding));
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void DefinitionVersionMustBePositive(int version)
        { Assert.AreEqual("InvalidValue", DefinitionBinding.Prepare(Binding(), "test:L1", version, Budget()).RejectionCode); }

        [TestCase("0")]
        [TestCase("-1")]
        [TestCase("01")]
        [TestCase("+1")]
        [TestCase("candidate-r1")]
        [TestCase("1.0")]
        public void PublishedEnterIntentRejectsNonPositiveOrNonCanonicalLevelVersion(string version)
        {
            var input = Enter(version);
            Assert.IsFalse(CandidateApplicationProtocol.PrepareIntent(input, Budget()).IsAccepted);
            input.Context = Candidate();
            Assert.IsTrue(CandidateApplicationProtocol.PrepareIntent(input, Budget()).IsAccepted, "Candidate v1 accepts opaque versions.");
        }

        [Test]
        public void PublishedEnterIntentCarriesExactLevelIdentityAndVersion()
        {
            var a = Take(CandidateApplicationProtocol.PrepareIntent(Enter("1"), Budget()));
            var b = Take(CandidateApplicationProtocol.PrepareIntent(Enter("2"), Budget()));
            CollectionAssert.AreNotEqual(a.CanonicalBytes, b.CanonicalBytes);
            var input = Enter("1"); input.EnterAttempt.LevelId = "test:L2";
            var c = Take(CandidateApplicationProtocol.PrepareIntent(input, Budget()));
            CollectionAssert.AreNotEqual(a.CanonicalBytes, c.CanonicalBytes);
            Assert.AreEqual("1", CandidateApplicationIntentCodec.Read(new List<byte>(a.CanonicalBytes).ToArray(), Budget()).Data.EnterAttempt.LevelVersion);
        }

        [Test]
        public void PublishedIntentRejectsBrokenUnicodeInPayloadAndEncodedBytesWithoutChangingCandidateV1()
        {
            var input = SharedRuleContinuityTests.SimpleIntent(new PublishedRuleContext(Binding()));
            var valid = Take(CandidateApplicationProtocol.PrepareIntent(input, Budget()));
            var bytes = new List<byte>(valid.CanonicalBytes).ToArray(); bytes[16] = 0; bytes[17] = 0xD8;
            Assert.Throws<SaveCodecFailure>(() => CandidateApplicationIntentCodec.Read(bytes, Budget()));
            input.PlayerId = new string((char)0xD800, 1);
            Assert.AreEqual("InvalidValue", CandidateApplicationProtocol.PrepareIntent(input, Budget()).RejectionCode);
            input.Context = Candidate();
            Assert.IsTrue(CandidateApplicationProtocol.PrepareIntent(input, Budget()).IsAccepted);
            input.Context = new PublishedRuleContext(Binding()); input.PlayerId = "p";
            input.InitializeProfile.CharacterId = new string((char)0xDC00, 1);
            Assert.AreEqual("InvalidValue", CandidateApplicationProtocol.PrepareIntent(input, Budget()).RejectionCode);
        }

        private static CandidateApplicationIntentInput Enter(string version)
        {
            return new CandidateApplicationIntentInput { PlayerId = "p", OperationId = "enter", ExpectedCommitId = "commit",
                Kind = CandidateApplicationKind.EnterAttempt, Context = new PublishedRuleContext(Binding()),
                EnterAttempt = new CandidateApplicationEnterInput { LevelId = "test:L1", LevelVersion = version,
                    CharacterId = "W", ExpectedCharacterRevision = 1, OriginalSlot = 2 } };
        }

        [Test]
        public void DefinitionVersionAndLevelIdentityAreExactAndBudgeted()
        {
            var a = Take(DefinitionBinding.Prepare(Binding(), "test:L1", 1, Budget()));
            Assert.AreEqual("1", a.CanonicalLevelVersion);
            Assert.IsFalse(a.Same(Take(DefinitionBinding.Prepare(Binding(), "test:L1", 2, Budget()))));
            Assert.IsFalse(a.Same(Take(DefinitionBinding.Prepare(Binding(), "test:L2", 1, Budget()))));
            var tinyMath = new SaveCodecBudget(new ExactMathBudget(maxIntegerBits: 8));
            Assert.AreEqual("Limit", DefinitionBinding.Prepare(Binding(), "test:L1", new BigInteger(256), tinyMath).RejectionCode);
            var strings = new SaveCodecBudget(Math(), maxStringCodeUnits: 1);
            Assert.IsTrue(ContentBinding.Prepare("p", "c", "r", "n", "q", strings).IsAccepted);
            Assert.AreEqual("Limit", ContentBinding.Prepare("pp", "c", "r", "n", "q", strings).RejectionCode);
            var candidate = Candidate();
            Assert.AreEqual("Limit", RuleContextChecks.Prepare(candidate, new SaveCodecBudget(Math(), maxCollectionEntries: 0)).RejectionCode);
            candidate.DraftRevision = 256;
            Assert.AreEqual("Limit", RuleContextChecks.Prepare(candidate, tinyMath).RejectionCode);
        }

        [Test]
        public void InvalidContextBranchesReturnStructuredRejectionsAtBusinessEntrypoints()
        {
            foreach (var invalid in new RuleContext[] { null, new PublishedRuleContext(null), new UnknownContext() })
            {
                Assert.IsFalse(RuleContextChecks.Prepare(invalid, Budget()).IsAccepted);
                var input = RawEntry(); input.Context = invalid;
                Assert.IsFalse(new BattleEntryPreparer().PrepareCandidate(input, Math()).IsAccepted);
                var growth = BusinessSaveScenario.Growth(Candidate(), 100); growth.Context = invalid;
                Assert.IsFalse(CandidateCharacterGrowth.PrepareDefinition(growth, Math()).IsAccepted);
            }
            Assert.IsFalse(RuleContextChecks.Same((RuleContext)null, null));
            Assert.IsFalse(RuleContextChecks.Same(new UnknownContext(), new UnknownContext()));
            Assert.IsFalse(RuleContextChecks.CheckBudget((PreparedRuleContext)null, Budget()).IsAccepted);
        }

        [Test]
        public void EntryAdmissionRejectsWrongDomainLevelPayloadAndMismatchedMemberContext()
        {
            var preparer = new BattleEntryPreparer(); var raw = RawEntry();
            var resolved = preparer.PrepareCandidate(raw, Math()).Entry.Level;
            var binding = Take(DefinitionBinding.Prepare(Binding(), "test:L1", 1, Budget()));
            Assert.IsFalse(preparer.PreparePublished(raw, binding, resolved, Math()).IsAccepted);
            raw.Context = new PublishedRuleContext(Binding()); raw.Members[0].StatsContext = raw.Context;
            Assert.IsFalse(preparer.PrepareCandidate(raw, Math()).IsAccepted);
            Assert.IsFalse(preparer.PreparePublished(raw, null, resolved, Math()).IsAccepted);
            Assert.IsFalse(preparer.PreparePublished(raw, binding, null, Math()).IsAccepted);
            Assert.IsTrue(preparer.PreparePublished(raw, binding, resolved, Math()).IsAccepted);
            raw.Members[0].StatsContext = Candidate();
            Assert.AreEqual(BattleEntryRejectionCode.InconsistentBinding, preparer.PreparePublished(raw, binding, resolved, Math()).RejectionCode);
            raw.Members[0].StatsContext = raw.Context;
            raw.Level.Faces[0].Pairs[0].Enemy.Stats.MaxHp = R(16);
            Assert.AreEqual(BattleEntryRejectionCode.InconsistentBinding, preparer.PreparePublished(raw, binding, resolved, Math()).RejectionCode);
            raw.Level.Faces[0].Pairs[0].Enemy.Stats.MaxHp = R(15);
            foreach (var wrong in new[] { Take(DefinitionBinding.Prepare(Binding(), "test:L1", 2, Budget())),
                Take(DefinitionBinding.Prepare(Binding(), "test:L2", 1, Budget())) })
                Assert.AreEqual(BattleEntryRejectionCode.InconsistentBinding, preparer.PreparePublished(raw, wrong, resolved, Math()).RejectionCode);
        }

        [Test]
        public void ResolvedDefinitionClosureIsImmutableExactAndDoesNotFallbackToAnotherPackage()
        {
            var s = new SharedRuleFixture(true);
            var levels = new List<PreparedLevel> { s.Entry.Level };
            var rewards = new List<CandidateRewardDefinition> { s.RewardDefinition };
            var definitions = Take(PublishedRuleDefinitions.Prepare(s.Content, s.Character.Definition, s.Inventory.Definition,
                s.Progression.Definition, levels, rewards, Budget()));
            var entries = new List<PublishedRuleDefinitions> { definitions };
            var context = Take(PublishedSaveContext.Prepare(entries, Budget()));
            levels.Clear(); rewards.Clear(); entries.Clear();
            Assert.AreSame(definitions, context.FindExact(Binding()));
            Assert.AreSame(s.Entry.Level, context.FindExact(s.Definition));
            Assert.AreEqual(1, definitions.Rewards.Count);
            for (var field = 0; field < 5; field++) Assert.IsNull(context.FindExact(Binding(field)));
            Assert.IsNull(context.FindExact(Take(DefinitionBinding.Prepare(Binding(), "test:L1", 2, Budget()))));
            Assert.IsNull(context.FindExact((ContentBinding)null));
            Assert.IsNull(context.FindExact((DefinitionBinding)null));
            Assert.IsFalse(PublishedSaveContext.Prepare(new[] { definitions, definitions }, Budget()).IsAccepted);
            Assert.IsFalse(PublishedSaveContext.Prepare(new PublishedRuleDefinitions[] { null }, Budget()).IsAccepted);
            Assert.IsFalse(PublishedSaveContext.Prepare(Array.Empty<PublishedRuleDefinitions>(), Budget()).IsAccepted);
            Assert.AreEqual("Limit", PublishedSaveContext.Prepare(new[] { definitions }, new SaveCodecBudget(Math(), maxCollectionEntries: 0)).RejectionCode);
        }

        [Test]
        public void DefinitionClosureRejectsCandidateMixedBindingsMissingAndDuplicateLevels()
        {
            var s = new SharedRuleFixture(true); var candidate = new SharedRuleFixture(false);
            var levels = new[] { s.Entry.Level }; var rewards = new[] { s.RewardDefinition };
            Assert.IsFalse(PublishedRuleDefinitions.Prepare(s.Content, candidate.Character.Definition, s.Inventory.Definition,
                s.Progression.Definition, levels, rewards, Budget()).IsAccepted);
            Assert.IsFalse(PublishedRuleDefinitions.Prepare(Binding(0), s.Character.Definition, s.Inventory.Definition,
                s.Progression.Definition, levels, rewards, Budget()).IsAccepted);
            Assert.IsFalse(PublishedRuleDefinitions.Prepare(s.Content, null, s.Inventory.Definition,
                s.Progression.Definition, levels, rewards, Budget()).IsAccepted);
            foreach (var invalid in new IReadOnlyList<PreparedLevel>[] { null, Array.Empty<PreparedLevel>(),
                new PreparedLevel[] { null }, new[] { s.Entry.Level, s.Entry.Level } })
                Assert.IsFalse(PublishedRuleDefinitions.Prepare(s.Content, s.Character.Definition, s.Inventory.Definition,
                    s.Progression.Definition, invalid, rewards, Budget()).IsAccepted);
            Assert.IsFalse(PublishedRuleDefinitions.Prepare(s.Content, s.Character.Definition, s.Inventory.Definition,
                s.Progression.Definition, levels, new[] { s.RewardDefinition, s.RewardDefinition }, Budget()).IsAccepted);
            Assert.IsFalse(PublishedRuleDefinitions.Prepare(s.Content, s.Character.Definition, s.Inventory.Definition,
                s.Progression.Definition, levels, new[] { candidate.RewardDefinition }, Budget()).IsAccepted);
        }

        [Test]
        public void FormalRequirementsDeduplicateAndRoundTripThroughTheExistingEnvelopeContract()
        {
            var s = new SharedRuleFixture(true);
            // Metadata projection probe only; no formal business admission, publication or player save.
            var snapshot = new CandidateBusinessSnapshot(new CandidateBusinessInput("p", s.Character, s.Inventory, s.Progression,
                CandidateBaseRewards.CreateCandidate("p", Math()).Next, s.History, Array.Empty<CandidateBattleRun>(), Array.Empty<CandidateRollbackRecord>()));
            for (var owner = 0; owner < 3; owner++)
            {
                var requirements = BusinessRequirements.For(snapshot, null, owner, Budget());
                Assert.AreEqual(1, requirements.Bindings.Count, "Repeated contexts and levels must deduplicate.");
                var binding = requirements.Bindings[0];
                Assert.AreEqual(owner == 2 ? SaveBindingKind.Definition : SaveBindingKind.Content, binding.Kind);
                Assert.AreEqual(s.Content.PackageId, binding.PackageId);
                Assert.IsNull(binding.DraftId); Assert.IsNull(binding.DraftRevision); Assert.IsNull(binding.SourceNotes);
                if (owner == 2) { Assert.AreEqual("test:L1", binding.LevelId); Assert.AreEqual("1", binding.LevelVersion); }
                var contract = new RequiredSliceContract("test.requirements", "test", 1);
                var envelope = Take(SaveEnvelopeCodec.Prepare(new SaveEnvelopeInput { Purpose = SavePurpose.CandidateValidation,
                    PlayerId = "p", SaveGeneration = 1, CommitId = "commit", ParentCommitId = null,
                    RequiredSliceContracts = new[] { contract },
                    Slices = new[] { new SaveSliceInput { Contract = contract, Bytes = new byte[] { 1 }, Requirements = requirements } },
                    CommitIndex = new[] { new SaveCommitIndexEntry(1, "commit", null, null, null, Array.Empty<string>()) } }, Budget()));
                using (var stream = new MemoryStream())
                {
                    var descriptor = Take(SaveEnvelopeCodec.Write(stream, envelope, Budget())); stream.Position = 0;
                    var read = Take(SaveEnvelopeCodec.Read(stream, descriptor, Budget()));
                    Assert.IsTrue(BusinessRequirements.Same(requirements, read.SliceDirectory[0].Requirements));
                }
                Assert.IsFalse(snapshot.CommitEligible);
            }
        }

        [TestCase(null)]
        [TestCase("0")]
        [TestCase("-1")]
        [TestCase("01")]
        [TestCase("candidate-r1")]
        public void FormalRequirementProjectionRejectsInvalidDefinitionVersions(string version)
        {
            var context = Take(RuleContextChecks.Prepare(new PublishedRuleContext(Binding()), Budget()));
            Assert.Throws<SaveCodecFailure>(() => RuleContextChecks.SaveBinding(context, Budget(), "test:L1", version));
        }

        private sealed class UnknownContext : RuleContext
        {
            public override string ContentFingerprint { get; set; }
            public override string RuleVersion { get; set; }
            public override string NumericContractVersion { get; set; }
            public override string RandomContractVersion { get; set; }
        }
    }
}
