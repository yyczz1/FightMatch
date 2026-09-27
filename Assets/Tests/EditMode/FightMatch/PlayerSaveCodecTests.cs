using System;
using System.Linq;
using System.Numerics;
using FightMatch.Application;
using FightMatch.Content;
using NUnit.Framework;
using static FightMatch.Core.Tests.PlayerSessionTestData;

namespace FightMatch.Core.Tests
{
    public class PlayerSaveCodecTests
    {
        private SaveEnvelope envelope;
        private CandidateApplicationSnapshot head;
        private PublishedSaveContext closure;
        private PlayerProfileCreateRecord record;
        [OneTimeSetUp] public void CaptureRealPublishedInitialization()
        {
            using (var rig = new Rig()) { envelope = Envelope(rig.Storage, rig.Head.Header.CommitId); head = rig.Head;
                closure = Closure(rig.Publication); record = rig.Profile.CreateRecord; }
        }
        [Test] public void PlayerSixSlicesAreSchemaTwoWithExactBindingsAndActualInitialization()
        {
            Assert.AreEqual(SavePurpose.PlayerSave, envelope.Purpose); Assert.AreEqual(6, envelope.SliceDirectory.Count);
            Assert.IsTrue(envelope.RequiredSliceContracts.All(c => c.SchemaVersion == 2));
            CollectionAssert.AreEqual(new[] { "fm.player.application.v1" }, envelope.RecoveryRequirements.FeatureIds);
            Assert.IsTrue(envelope.RecoveryRequirements.Bindings.All(b => b.Kind == SaveBindingKind.Content || b.Kind == SaveBindingKind.Definition));
            var restored = TakeCore(CandidateApplicationSaveCodec.DecodePublished(envelope, closure, Codec()));
            Assert.AreEqual(record.PlayerId, restored.Business.PlayerId); Assert.AreEqual(BigInteger.One, restored.Business.Character.Level);
            Assert.AreEqual(BigInteger.Zero, restored.Business.Character.Experience); Assert.AreEqual(0, restored.Business.Character.OriginalSlot);
            Assert.IsEmpty(restored.Business.Rewards.BaseRewards); Assert.IsNull(restored.Continuation);
            var first = TakeCore(CandidateBusinessSaveCodec.EncodePublished(restored.Business, restored.Header, closure, Codec()));
            var decoded = TakeCore(CandidateBusinessSaveCodec.DecodePublished(first, closure, Codec()));
            CollectionAssert.AreEqual(Encode(first), Encode(TakeCore(CandidateBusinessSaveCodec.EncodePublished(decoded, restored.Header, closure, Codec()))));
            Assert.Less(first.SliceBytes[0].Count, 1100, "growth authoring definition must not be embedded");
        }
        [Test] public void OldCandidateEntryAndBytesRemainCompatibleAndRejectFormalPayload()
        {
            var candidate = new ApplicationScenario(); var bytes = Encode(candidate.Envelope);
            var restored = TakeCore(CandidateApplicationSaveCodec.Decode(candidate.Envelope, Codec()));
            CollectionAssert.AreEqual(bytes, Encode(TakeCore(CandidateApplicationSaveCodec.Encode(candidate.Candidate, restored.Header, Codec()))));
            Assert.AreEqual("UnsupportedBinding", CandidateApplicationSaveCodec.Decode(envelope, Codec()).RejectionCode);
            Assert.AreEqual("UnsupportedBinding", CandidateApplicationSaveCodec.EncodePublished(candidate.Candidate, restored.Header, closure, Codec()).RejectionCode);
            Assert.AreEqual("UnsupportedBinding", CandidateBusinessSaveCodec.Encode(head.Business, head.Header, Codec()).RejectionCode);
            Assert.IsFalse(Repack(candidate.Envelope, SavePurpose.PlayerSave).IsAccepted, "Candidate bindings cannot be relabelled PlayerSave even at the envelope boundary");
        }
        [TestCase("purpose")] [TestCase("schema")] [TestCase("feature")] [TestCase("body")]
        public void WrongPurposeSchemaCapabilitiesOrFieldsCannotDecode(string defect)
        {
            var changed = Mutate(envelope, defect == "purpose" ? (SavePurpose?)SavePurpose.CandidateValidation : null,
                defect == "schema" ? (uint?)1 : null, defect == "feature", defect == "body");
            var before = Encode(changed); var result = CandidateApplicationSaveCodec.DecodePublished(changed, closure, Codec());
            Assert.IsFalse(result.IsAccepted); Assert.IsNotEmpty(result.FieldPath); CollectionAssert.AreEqual(before, Encode(changed));
        }
        [Test] public void MissingExactPackageRejectsWithoutFallback()
        {
            var fixture = new PublishedFixture(); var different = Closure(fixture.V2);
            var result = CandidateApplicationSaveCodec.DecodePublished(envelope, different, Codec());
            Assert.IsFalse(result.IsAccepted); Assert.AreEqual("UnsupportedBinding", result.RejectionCode);
        }
        [Test] public void CompleteCreationRecordRoundTripsDeepCopiesAndOriginalCanonicalIntent()
        {
            var bytes = TakeCore(PlayerProfileCreateRecordCodec.Write(record, Codec())); var read = TakeCore(PlayerProfileCreateRecordCodec.Read(bytes, Codec()));
            var old = bytes[0]; bytes[0] ^= 1;
            var written = TakeCore(PlayerProfileCreateRecordCodec.Write(read, Codec())); Assert.AreEqual(old, written[0]);
            Assert.AreEqual(1, read.RecordFormatVersion); Assert.AreEqual(2, read.IntentFormatVersion); Assert.IsEmpty(read.GeneratedMaterials);
            Assert.AreEqual(record.RecordSha256, read.RecordSha256); Assert.AreEqual("new-profile:default", read.NewProfileDefinitionId);
            Assert.AreEqual(BigInteger.One, read.NewProfileDefinitionVersion); Assert.AreEqual(298, read.FrozenNewProfileDefinitionBytes.Count);
            Assert.AreEqual("36f2d8935f626c4bdafb53c56ac288f5199e7a6dec29680c0fac02f354685646", read.DefinitionSha256);
            var intent = TakeCore(PlayerProfileCreateRecordCodec.RestoreIntent(read, Codec()));
            CollectionAssert.AreEqual(record.CanonicalInitializeIntentBytes, intent.CanonicalBytes);
            var definition = read.FrozenNewProfileDefinitionBytes.ToArray();
            var frozen = TakeCore(PlayerProfileCreateRecordCodec.Freeze(intent, read.NewProfileDefinitionId, read.NewProfileDefinitionVersion, definition,
                Array.Empty<PlayerProfileCreateMaterial>(), Codec())); definition[0] ^= 1;
            CollectionAssert.AreEqual(read.FrozenNewProfileDefinitionBytes, frozen.FrozenNewProfileDefinitionBytes);
            var proof = TakeCore(PlayerProfileCreateRecordCodec.VerifyInitializationCommit(read, head, Codec()));
            Assert.AreEqual(head.Header.CommitId, proof.OriginalInitializationCommitId); Assert.AreEqual(record.RecordSha256, proof.RecordSha256);
        }
        [TestCase("truncate")] [TestCase("format")] [TestCase("identity")] [TestCase("tail")] [TestCase("length")]
        public void CorruptedRecordIsNeverAcceptedAsAbsent(string defect)
        {
            var bytes = TakeCore(PlayerProfileCreateRecordCodec.Write(record, Codec()));
            if (defect == "truncate") bytes = bytes.Take(bytes.Length / 2).ToArray();
            else if (defect == "format") bytes[8] = 99;
            else if (defect == "identity") bytes[16] ^= 1;
            else if (defect == "tail") bytes[bytes.Length - 1] ^= 1;
            else for (var i = 12; i < 16; i++) bytes[i] = 255;
            var result = PlayerProfileCreateRecordCodec.Read(bytes, Codec());
            Assert.IsFalse(result.IsAccepted); Assert.IsNull(result.Value); Assert.IsNotEmpty(result.FieldPath);
        }
        [Test] public void EntireRecordAndAllocationBudgetsApplyAndMaterialsCannotBeInvented()
        {
            var bytes = TakeCore(PlayerProfileCreateRecordCodec.Write(record, Codec()));
            var small = new SaveCodecBudget(new ExactMathBudget(), maxEnvelopeBytes: (ulong)bytes.Length - 1);
            Assert.AreEqual("Limit", PlayerProfileCreateRecordCodec.Read(bytes, small).RejectionCode);
            Assert.AreEqual("Limit", PlayerProfileCreateRecordCodec.Write(record, small).RejectionCode);
            var original = TakeCore(PlayerProfileCreateRecordCodec.RestoreIntent(record, Codec()));
            var material = new PlayerProfileCreateMaterial("entropy", "premature-h02", "fixture:only", new byte[] { 1 }, Codec());
            Assert.AreEqual("InconsistentCreateIntent", PlayerProfileCreateRecordCodec.Freeze(original, record.NewProfileDefinitionId, 1,
                record.FrozenNewProfileDefinitionBytes.ToArray(), new[] { material }, Codec()).RejectionCode);
            var candidate = new ApplicationScenario().Intents["init"];
            Assert.AreEqual("InconsistentCreateIntent", PlayerProfileCreateRecordCodec.Freeze(candidate, record.NewProfileDefinitionId, 1,
                record.FrozenNewProfileDefinitionBytes.ToArray(), Array.Empty<PlayerProfileCreateMaterial>(), Codec()).RejectionCode);
        }
        [TestCase("definition")] [TestCase("intent")] [TestCase("intent-schema")] [TestCase("materials")]
        public void FrozenDefinitionIntentSchemaAndMaterialFieldsAreIndependentlyChecked(string defect)
        {
            var bytes = TakeCore(PlayerProfileCreateRecordCodec.Write(record, Codec())); var at = 12;
            for (var i = 0; i < 8; i++) at += 4 + BitConverter.ToInt32(bytes, at) * 2;
            at += 4 + BitConverter.ToInt32(bytes, at); // positive definition version, canonical integer token
            var definition = at + 4; at += 4 + BitConverter.ToInt32(bytes, at);
            var schema = at; at += 4; var intent = at + 4; at += 4 + BitConverter.ToInt32(bytes, at);
            for (var i = 0; i < 2; i++) at += 4 + BitConverter.ToInt32(bytes, at) * 2;
            var changed = defect == "definition" ? definition : defect == "intent" ? intent : defect == "intent-schema" ? schema : at;
            bytes[changed] ^= 1; var before = (byte[])bytes.Clone(); var result = PlayerProfileCreateRecordCodec.Read(bytes, Codec());
            Assert.IsFalse(result.IsAccepted); Assert.IsNotEmpty(result.FieldPath); CollectionAssert.AreEqual(before, bytes);
        }
    }
}
