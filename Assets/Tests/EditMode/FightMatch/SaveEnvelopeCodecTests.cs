using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using NUnit.Framework;

namespace FightMatch.Core.Tests
{
    [TestFixture]
    public class SaveEnvelopeCodecTests
    {
        // Independent field concatenation: 60-byte header, 68-byte metadata, empty body.
        // Purpose=0, player=p, generation=1, commit=c, null parent, empty contracts/directory,
        // one index row (1,c,null,no old digest,empty operations), five empty recovery lists.
        private const string GoldenHex =
            "464d5341564530310100000044000000000000000000000000000000" +
            "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855" +
            "00" + "010000007000" + "0100000031" + "010000006300" + "00" +
            "00000000" + "00000000" + "01000000" + "0100000031" + "010000006300" + "00" + "00" +
            "00000000" + "00000000" + "00000000" + "00000000" + "00000000" + "00000000";
        private const string GoldenSha = "3ad60355090274a1067a5480dd1bc8bf89161c97c5951941de012970ae42e0ea";

        [Test]
        public void FixedCompleteGoldenBytesAndShaAreIndependentOfTheProductionWriter()
        {
            var bytes = Hex(GoldenHex);
            Assert.AreEqual(128, bytes.Length);
            Assert.AreEqual(GoldenSha, ToHex(Digest(bytes)));
            var expected = new SnapshotDescriptor(SavePurpose.CandidateValidation, "p", 1, "c", null, 128, Hex(GoldenSha));
            var read = SaveEnvelopeCodec.Read(new MemoryStream(bytes), expected, Budget()); Accepted(read);
            Assert.IsEmpty(read.Value.SliceDirectory); Assert.IsEmpty(read.Value.SliceBytes);
            var summary = SaveEnvelopeCodec.ReadRequirements(new MemoryStream(bytes), expected, Budget()); Accepted(summary);
            Assert.AreEqual("c", summary.Value.CommitIndex.Single().CommitId);
            using (var stream = new MemoryStream())
            {
                var written = SaveEnvelopeCodec.Write(stream, Prepared(Empty()), Budget()); Accepted(written);
                CollectionAssert.AreEqual(bytes, stream.ToArray());
                CollectionAssert.AreEqual(Hex(GoldenSha), written.Value.Sha256);
                Assert.AreEqual(128UL, written.Value.TotalLength);
            }
        }

        [Test]
        public void FixedScalarBytesPreserveOrdinalCodeUnitsAndNullableStringMarkers()
        {
            using (var stream = new MemoryStream())
            {
                var f = new SaveFields(stream, false, Budget());
                f.Integer(0, "i"); f.Text("A中", "s"); f.Text("\ud800", "s");
                f.OptionalText(null, "s"); f.OptionalText("", "s");
                Assert.AreEqual("0100000030" + "0200000041002d4e" + "0100000000d8" + "00" + "0100000000", ToHex(stream.ToArray()));
            }
            var token = ExactSaveValueCodec.EncodeRational(ExactRational.Create(-1, 2, new ExactMathBudget()), Budget()); Accepted(token);
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream, Encoding.ASCII, true))
            {
                writer.Write((uint)token.Value.Length); writer.Write(Encoding.ASCII.GetBytes(token.Value));
                Assert.AreEqual("040000002d312f32", ToHex(stream.ToArray()));
            }
            var input = Empty(); input.PlayerId = " A中\ud800\udfff\0 ";
            var file = Written(input);
            var read = SaveEnvelopeCodec.Read(new MemoryStream(file.Bytes), file.Expected, Budget()); Accepted(read);
            Assert.AreEqual(input.PlayerId, read.Value.PlayerId);
            Assert.AreNotEqual("é", "e\u0301");
        }

        [TestCase("0")]
        [TestCase("1")]
        [TestCase("-1")]
        [TestCase("9007199254740993")]
        [TestCase("-9007199254740993")]
        [TestCase("1234567890123456789012345678901234567890")]
        public void CanonicalIntegersRoundTripExactly(string token)
        {
            var decoded = ExactSaveValueCodec.DecodeInteger(token, Budget()); Accepted(decoded);
            Assert.AreEqual(BigInteger.Parse(token, CultureInfo.InvariantCulture), decoded.Value);
            var encoded = ExactSaveValueCodec.EncodeInteger(decoded.Value.Value, Budget()); Accepted(encoded);
            Assert.AreEqual(token, encoded.Value);
        }

        [TestCase("0/1", 0, 1)]
        [TestCase("1/3", 1, 3)]
        [TestCase("-1/2", -1, 2)]
        [TestCase("2/1", 2, 1)]
        [TestCase("-17/23", -17, 23)]
        public void CanonicalRationalsRoundTripExactly(string token, int numerator, int denominator)
        {
            var value = ExactSaveValueCodec.DecodeRational(token, Budget()); Accepted(value);
            Assert.AreEqual(new BigInteger(numerator), value.Value.Numerator);
            Assert.AreEqual(new BigInteger(denominator), value.Value.Denominator);
            var encoded = ExactSaveValueCodec.EncodeRational(value.Value, Budget()); Accepted(encoded);
            Assert.AreEqual(token, encoded.Value);
        }

        [TestCase("")]
        [TestCase("+")]
        [TestCase("-")]
        [TestCase("+1")]
        [TestCase("01")]
        [TestCase("00")]
        [TestCase("-0")]
        [TestCase("-01")]
        [TestCase("1.0")]
        [TestCase("1e3")]
        [TestCase(" 1")]
        [TestCase("1 ")]
        [TestCase("1\n")]
        [TestCase("1/1")]
        [TestCase("1,000")]
        [TestCase("NaN")]
        [TestCase("Infinity")]
        [TestCase("−1")]
        [TestCase("１")]
        [TestCase("١")]
        [TestCase("1\0")]
        public void EveryNoncanonicalIntegerIsRejectedWithoutAValue(string token)
        { Rejected(ExactSaveValueCodec.DecodeInteger(token, Budget()), "Malformed"); }

        [TestCase("")]
        [TestCase("1")]
        [TestCase("1/")]
        [TestCase("/1")]
        [TestCase("1/2/3")]
        [TestCase("+1/2")]
        [TestCase("1/+2")]
        [TestCase("01/2")]
        [TestCase("1/02")]
        [TestCase("1/-2")]
        [TestCase("1/0")]
        [TestCase("0/0")]
        [TestCase("0/2")]
        [TestCase("-0/1")]
        [TestCase("2/4")]
        [TestCase("-2/4")]
        [TestCase("3/3")]
        [TestCase("1.0/2")]
        [TestCase("1e2/3")]
        [TestCase("1 /2")]
        [TestCase("1/ 2")]
        [TestCase("1/2\n")]
        [TestCase("NaN/1")]
        [TestCase("Infinity/1")]
        [TestCase("１/2")]
        public void EveryNoncanonicalRationalIsRejectedWithoutNormalization(string token)
        { Rejected(ExactSaveValueCodec.DecodeRational(token, Budget()), "Malformed"); }

        [Test]
        public void ScalarLimitsPrecedeParsingAndPreserveTheSharedMathLimitDetails()
        {
            var shortToken = new SaveCodecBudget(new ExactMathBudget(), maxNumericTokenBytes: 3);
            var limit = ExactSaveValueCodec.DecodeInteger("bad!", shortToken); Rejected(limit, "Limit");
            Assert.AreEqual("NumericTokenBytes", limit.LimitReason); Assert.AreEqual(4UL, limit.RequiredAtLeast);
            Rejected(ExactSaveValueCodec.EncodeInteger(1000, shortToken), "Limit");
            Rejected(ExactSaveValueCodec.EncodeRational(ExactRational.Create(-1, 2, new ExactMathBudget()), shortToken), "Limit");
            var bits = ExactSaveValueCodec.DecodeInteger("256", new SaveCodecBudget(new ExactMathBudget(8)));
            Rejected(bits, "Limit"); Assert.AreEqual("IntegerBits", bits.LimitReason); Assert.AreEqual(8UL, bits.Allowed);
            Accepted(ExactSaveValueCodec.DecodeInteger("255", new SaveCodecBudget(new ExactMathBudget(8))));
            var measure = Budget(); Accepted(ExactSaveValueCodec.DecodeRational("123/257", measure));
            var shared = new SaveCodecBudget(new ExactMathBudget(32768, (int)measure.Math.PrimitiveStepsUsed));
            Accepted(ExactSaveValueCodec.DecodeRational("123/257", shared));
            var exhausted = ExactSaveValueCodec.DecodeRational("123/257", shared); Rejected(exhausted, "Limit");
            Assert.AreEqual("PrimitiveSteps", exhausted.LimitReason);
            Accepted(ExactSaveValueCodec.DecodeRational("123/257", Budget()));
        }

        [Test]
        public void FrozenInputHasNoWritableOutputAndStartupSummaryHasNoBodyOrBusinessObjects()
        {
            var input = Populated();
            var source = input.Slices[0].Bytes;
            var operations = (List<string>)input.CommitIndex[0].OperationIds;
            var features = (List<string>)input.Slices[0].Requirements.FeatureIds;
            var notes = new List<string> { "source" };
            var bindings = new List<SaveBinding> { Binding(SaveBindingKind.CandidateDefinition, notes: notes) };
            input.Slices[0].Requirements = Requirements(bindings, features);
            var frozen = Prepared(input); var first = Written(frozen);
            source[0] ^= 255; operations.Add("later"); features.Add("later"); notes.Add("later"); bindings.Clear();
            ((List<RequiredSliceContract>)input.RequiredSliceContracts).Clear();
            ((List<SaveSliceInput>)input.Slices).Clear(); input.PlayerId = "other";
            var second = Written(frozen); CollectionAssert.AreEqual(first.Bytes, second.Bytes);
            Assert.Throws<NotSupportedException>(() => ((IList<byte>)frozen.SliceBytes[0])[0] = 0);
            Assert.Throws<NotSupportedException>(() => ((IList<string>)frozen.CommitIndex[0].OperationIds).Add("x"));
            Assert.Throws<NotSupportedException>(() => ((IList<string>)frozen.RecoveryRequirements.FeatureIds).Clear());
            Assert.Throws<NotSupportedException>(() => ((IList<SaveBinding>)frozen.RecoveryRequirements.Bindings).Clear());
            Assert.Throws<NotSupportedException>(() => ((IList<string>)frozen.RecoveryRequirements.Bindings[0].SourceNotes).Clear());
            var summary = SaveEnvelopeCodec.ReadRequirements(new MemoryStream(first.Bytes), first.Expected, Budget()); Accepted(summary);
            Assert.IsNull(typeof(SaveRecoverySummary).GetProperty("SliceBytes"));
            Assert.IsNull(typeof(SaveRecoverySummary).GetProperty("Envelope"));
            Assert.AreEqual(2, summary.Value.SliceDirectory.Count);
            Assert.AreEqual("source", summary.Value.RecoveryRequirements.Bindings[0].SourceNotes.Single());
            CollectionAssert.AreEqual(first.Expected.Sha256, summary.Value.Descriptor.Sha256);
        }

        [TestCase(SaveBindingKind.Content)]
        [TestCase(SaveBindingKind.Definition)]
        [TestCase(SaveBindingKind.CandidateContent)]
        [TestCase(SaveBindingKind.CandidateDefinition)]
        public void BindingKindsRemainDistinctAndPlayerPurposeRejectsCandidateReferences(SaveBindingKind kind)
        {
            var input = Populated();
            input.Slices[0].Requirements = Requirements(new[] { Binding(kind) });
            var candidate = Written(input);
            var summary = SaveEnvelopeCodec.ReadRequirements(new MemoryStream(candidate.Bytes), candidate.Expected, Budget()); Accepted(summary);
            Assert.AreEqual(kind, summary.Value.RecoveryRequirements.Bindings.Single().Kind);
            input.Purpose = SavePurpose.PlayerSave;
            if ((byte)kind < 3)
            {
                var player = Written(input); Accepted(SaveEnvelopeCodec.Read(new MemoryStream(player.Bytes), player.Expected, Budget()));
                Assert.IsNull(typeof(SnapshotDescriptor).GetProperty("Published"));
                Assert.IsNull(typeof(SnapshotDescriptor).GetProperty("Completed"));
            }
            else
            {
                Rejected(SaveEnvelopeCodec.Prepare(input, Budget()), "InvalidValue");
                AssertBadBoth(Independent(input), "InvalidValue");
            }
        }

        [Test]
        public void RequirementsUnionPreservesFirstOccurrenceAndCompleteComponentIdentity()
        {
            var a = Binding(SaveBindingKind.Content, package: "a|b", fingerprint: "c");
            var b = Binding(SaveBindingKind.Content, package: "a", fingerprint: "b|c");
            var c = Binding(SaveBindingKind.Content, rule: "r2");
            var input = Populated();
            input.Slices[0].Requirements = Requirements(new[] { a, b }, new[] { "f2", "f1" });
            input.Slices[1].Requirements = new SaveRequirements(new[] { b, c }, new[] { "r", "r2" }, new[] { "n" }, new[] { "q" }, new[] { "f1", "f3" });
            var frozen = Prepared(input);
            CollectionAssert.AreEqual(new[] { "f2", "f1", "f3" }, frozen.RecoveryRequirements.FeatureIds);
            Assert.AreEqual(3, frozen.RecoveryRequirements.Bindings.Count);
            CollectionAssert.AreEqual(new[] { "r", "r2" }, frozen.RecoveryRequirements.RuleVersions);
            var file = Written(frozen);
            var read = SaveEnvelopeCodec.ReadRequirements(new MemoryStream(file.Bytes), file.Expected, Budget()); Accepted(read);
            Assert.AreEqual("a|b", read.Value.RecoveryRequirements.Bindings[0].PackageId);
            Assert.AreEqual("a", read.Value.RecoveryRequirements.Bindings[1].PackageId);
            Assert.AreEqual("r2", read.Value.RecoveryRequirements.Bindings[2].RuleVersion);
        }

        [Test]
        public void CandidateRevisionAndOrderedSourceNotesRetainExactIdentity()
        {
            var input = Populated(); var notes = new[] { "same", "same" };
            input.Slices[0].Requirements = Requirements(new[] { Binding(SaveBindingKind.CandidateContent, notes: notes) });
            var file = Written(input);
            var summary = SaveEnvelopeCodec.ReadRequirements(new MemoryStream(file.Bytes), file.Expected, Budget()); Accepted(summary);
            var binding = summary.Value.RecoveryRequirements.Bindings.Single();
            Assert.AreEqual(BigInteger.Parse("9007199254740993", CultureInfo.InvariantCulture), binding.DraftRevision);
            CollectionAssert.AreEqual(notes, binding.SourceNotes);
            var small = new SaveCodecBudget(new ExactMathBudget(53));
            Rejected(SaveEnvelopeCodec.ReadRequirements(new MemoryStream(file.Bytes), file.Expected, small), "Limit");
        }

        [TestCase("unknownPurpose", "InvalidValue")]
        [TestCase("zeroGeneration", "InvalidValue")]
        [TestCase("negativeGeneration", "InvalidValue")]
        [TestCase("emptyPlayer", "InvalidValue")]
        [TestCase("emptyCommit", "InvalidValue")]
        [TestCase("emptySlice", "InvalidValue")]
        [TestCase("zeroSchema", "InvalidValue")]
        [TestCase("emptyOwner", "InvalidValue")]
        [TestCase("unknownBinding", "InvalidValue")]
        [TestCase("zeroRevision", "InvalidValue")]
        [TestCase("emptyFingerprint", "InvalidValue")]
        public void InvalidIdentityFieldsAreNotTrimmedDefaultedOrReinterpreted(string change, string code)
        {
            var input = Populated();
            switch (change)
            {
                case "unknownPurpose": input.Purpose = (SavePurpose)255; break;
                case "zeroGeneration": input.SaveGeneration = 0; break;
                case "negativeGeneration": input.SaveGeneration = -1; break;
                case "emptyPlayer": input.PlayerId = ""; break;
                case "emptyCommit": input.CommitId = ""; break;
                case "emptySlice": input.Slices[0].Contract = new RequiredSliceContract("", "m1", 7); break;
                case "zeroSchema": input.Slices[0].Contract = new RequiredSliceContract("a", "m1", 0); break;
                case "emptyOwner": input.Slices[0].Contract = new RequiredSliceContract("a", "", 7); break;
                default:
                    var b = change == "unknownBinding" ? Binding((SaveBindingKind)255) : change == "emptyFingerprint"
                        ? Binding(SaveBindingKind.Content, fingerprint: "")
                        : new SaveBinding(SaveBindingKind.CandidateContent, null, "draft", 0, "fp", "r", "n", "q", new[] { "note" }, null, null);
                    input.Slices[0].Requirements = Requirements(new[] { b }); break;
            }
            Rejected(SaveEnvelopeCodec.Prepare(input, Budget()), code);
            AssertBadBoth(Independent(input), code);
        }

        [TestCase("Purpose", "MissingField")]
        [TestCase("PlayerId", "MissingField")]
        [TestCase("Generation", "MissingField")]
        [TestCase("CommitId", "MissingField")]
        [TestCase("Contracts", "MissingField")]
        [TestCase("Slices", "MissingField")]
        [TestCase("Bytes", "MissingField")]
        [TestCase("Requirements", "MissingField")]
        [TestCase("Index", "MissingField")]
        [TestCase("Operations", "MissingField")]
        [TestCase("Bindings", "MissingField")]
        [TestCase("Rules", "MissingField")]
        [TestCase("Numeric", "MissingField")]
        [TestCase("Random", "MissingField")]
        [TestCase("Features", "MissingField")]
        public void MissingDeclarationsNeverBecomeAnImplicitEmptyList(string field, string code)
        {
            var input = Populated(); var req = input.Slices[0].Requirements;
            switch (field)
            {
                case "Purpose": input.Purpose = null; break;
                case "PlayerId": input.PlayerId = null; break;
                case "Generation": input.SaveGeneration = null; break;
                case "CommitId": input.CommitId = null; break;
                case "Contracts": input.RequiredSliceContracts = null; break;
                case "Slices": input.Slices = null; break;
                case "Bytes": input.Slices[0].Bytes = null; break;
                case "Requirements": input.Slices[0].Requirements = null; break;
                case "Index": input.CommitIndex = null; break;
                case "Operations": ReplaceIndex(input, 0, operations: null, replaceOperations: true); break;
                default:
                    input.Slices[0].Requirements = new SaveRequirements(field == "Bindings" ? null : req.Bindings,
                        field == "Rules" ? null : req.RuleVersions, field == "Numeric" ? null : req.NumericContractVersions,
                        field == "Random" ? null : req.RandomContractVersions, field == "Features" ? null : req.FeatureIds); break;
            }
            Rejected(SaveEnvelopeCodec.Prepare(input, Budget()), code);
        }

        [TestCase("missing")]
        [TestCase("duplicate")]
        [TestCase("owner")]
        [TestCase("schema")]
        [TestCase("order")]
        public void DirectoryMustExactlyCoverTheRequiredContractsInOrder(string change)
        {
            var input = Populated(); var rows = (List<RequiredSliceContract>)input.RequiredSliceContracts;
            switch (change)
            {
                case "missing": rows.RemoveAt(1); break;
                case "duplicate": rows[1] = rows[0]; input.Slices[1].Contract = rows[0]; break;
                case "owner": rows[0] = new RequiredSliceContract("a", "other", 7); break;
                case "schema": rows[0] = new RequiredSliceContract("a", "m1", 8); break;
                case "order": rows.Reverse(); break;
            }
            Rejected(SaveEnvelopeCodec.Prepare(input, Budget()), "DirectoryMismatch");
            AssertBadBoth(Independent(input), "DirectoryMismatch");
        }

        [TestCase("gap")]
        [TestCase("overlap")]
        [TestCase("first")]
        [TestCase("end")]
        [TestCase("digest")]
        public void IndependentlySealedInvalidDirectoryOffsetsAndDigestsAreRejected(string change)
        {
            var file = Independent(Populated());
            if (change == "digest") file.Bytes[file.Offsets["d0.sha"]] ^= 1;
            else if (change == "first") Put(file.Bytes, file.Offsets["d0.offset"], 1, 8);
            else if (change == "end") Put(file.Bytes, file.Offsets["d1.length"], 2, 8);
            else Put(file.Bytes, file.Offsets["d1.offset"], change == "gap" ? 4UL : 2UL, 8);
            Reseal(file);
            AssertBadBoth(file, change == "digest" ? "DigestMismatch" : "DirectoryMismatch");
        }

        [TestCase("missing")]
        [TestCase("extra")]
        [TestCase("duplicate")]
        [TestCase("reordered")]
        public void StoredRecoveryUnionCannotUnderstateOverstateDuplicateOrReorderRequirements(string change)
        {
            var feature = change == "missing" ? new[] { "x" } : change == "extra" ? new[] { "x", "y", "z" } :
                change == "duplicate" ? new[] { "x", "y", "x" } : new[] { "y", "x" };
            AssertBadBoth(Independent(Populated(), EmptyRequirements(feature)), "RequirementsMismatch");
        }

        [TestCase("bindingDuplicate")]
        [TestCase("featureDuplicate")]
        [TestCase("rules")]
        [TestCase("numeric")]
        [TestCase("random")]
        public void SliceRequirementsMustCoverEveryBindingCapabilityWithoutDuplicates(string change)
        {
            var input = Populated(); var b = Binding(SaveBindingKind.Content);
            input.Slices[0].Requirements = new SaveRequirements(change == "bindingDuplicate" ? new[] { b, b } : new[] { b },
                change == "rules" ? Array.Empty<string>() : new[] { "r" },
                change == "numeric" ? Array.Empty<string>() : new[] { "n" },
                change == "random" ? Array.Empty<string>() : new[] { "q" },
                change == "featureDuplicate" ? new[] { "x", "x" } : new[] { "x" });
            Rejected(SaveEnvelopeCodec.Prepare(input, Budget()), "RequirementsMismatch");
            AssertBadBoth(Independent(input), "RequirementsMismatch");
        }

        [Test]
        public void ThreeGenerationsAndUnknownBusinessSchemaReadWithoutAnAncestorFile()
        {
            var file = Independent(Populated());
            var result = SaveEnvelopeCodec.ReadRequirements(new MemoryStream(file.Bytes), file.Expected, Budget()); Accepted(result);
            Assert.AreEqual(3, result.Value.CommitIndex.Count);
            Assert.AreEqual("c1", result.Value.CommitIndex[1].ParentCommitId);
            Assert.AreEqual("c2", result.Value.Descriptor.ParentCommitId);
            Assert.AreEqual(uint.MaxValue, result.Value.SliceDirectory[1].Contract.SchemaVersion);
            Assert.AreEqual(10UL, result.Value.CommitIndex[0].SnapshotLength);
            Assert.IsNull(result.Value.CommitIndex[2].SnapshotSha256);
            Assert.IsEmpty(result.Value.CommitIndex[1].OperationIds);
            // Zero-length slices are retained, including their SHA256 of the empty byte string.
            var input = Populated(); input.Slices[0].Bytes = Array.Empty<byte>();
            var zero = Written(input); var read = SaveEnvelopeCodec.Read(new MemoryStream(zero.Bytes), zero.Expected, Budget()); Accepted(read);
            Assert.IsEmpty(read.Value.SliceBytes[0]); Assert.AreEqual(0UL, read.Value.SliceDirectory[1].BodyOffset);
            CollectionAssert.AreEqual(Digest(Array.Empty<byte>()), read.Value.SliceDirectory[0].Sha256);
        }

        [TestCase("gap")]
        [TestCase("branch")]
        [TestCase("commitDuplicate")]
        [TestCase("operationDuplicate")]
        [TestCase("withinRowDuplicate")]
        [TestCase("firstParent")]
        [TestCase("selfDigest")]
        [TestCase("missingOldDigest")]
        [TestCase("zeroOldLength")]
        [TestCase("headCommit")]
        [TestCase("headParent")]
        [TestCase("headGeneration")]
        public void SelfSufficientIndexRejectsBrokenChainsAndCurrentSelfReference(string change)
        {
            var input = Populated();
            switch (change)
            {
                case "gap": ReplaceIndex(input, 1, generation: 3); break;
                case "branch": ReplaceIndex(input, 2, parent: "c1", replaceParent: true); break;
                case "commitDuplicate": ReplaceIndex(input, 1, commit: "c1"); break;
                case "operationDuplicate": ReplaceIndex(input, 2, operations: new[] { "op1" }, replaceOperations: true); break;
                case "withinRowDuplicate": ReplaceIndex(input, 0, operations: new[] { "op1", "op1" }, replaceOperations: true); break;
                case "firstParent": ReplaceIndex(input, 0, parent: "", replaceParent: true); break;
                case "selfDigest": ReplaceIndex(input, 2, length: 10, digest: new byte[32], replaceDigest: true); break;
                case "missingOldDigest": ReplaceIndex(input, 0, replaceDigest: true); break;
                case "zeroOldLength": ReplaceIndex(input, 0, length: 0, digest: new byte[32], replaceDigest: true); break;
                case "headCommit": input.CommitId = "wrong"; break;
                case "headParent": input.ParentCommitId = "wrong"; break;
                case "headGeneration": input.SaveGeneration = 4; break;
            }
            Rejected(SaveEnvelopeCodec.Prepare(input, Budget()), "IndexMismatch");
            AssertBadBoth(Independent(input), "IndexMismatch");
        }

        [TestCase("magic", "Malformed")]
        [TestCase("version", "UnsupportedEnvelopeVersion")]
        [TestCase("metadata", "DigestMismatch")]
        [TestCase("body", "DigestMismatch")]
        [TestCase("bodyHash", "DigestMismatch")]
        [TestCase("trailing", "LengthMismatch")]
        [TestCase("length", "LengthMismatch")]
        [TestCase("present", "Malformed")]
        [TestCase("numericLexical", "Malformed")]
        [TestCase("nonAsciiToken", "Malformed")]
        public void CorruptionIsDetectedAtHeaderMetadataBodyAndSnapshotBoundaries(string change, string code)
        {
            var file = Independent(Populated());
            switch (change)
            {
                case "magic": file.Bytes[0] ^= 1; break;
                case "version": file.Bytes[8] = 2; break;
                case "metadata": file.Bytes[file.Offsets["i0.op"] + 4] = (byte)'z'; break;
                case "body": file.Bytes[file.Bytes.Length - 1] ^= 1; break;
                case "bodyHash": file.Bytes[28] ^= 1; break;
                case "trailing": file.Bytes = file.Bytes.Concat(new byte[] { 0 }).ToArray(); break;
                case "length": Put(file.Bytes, 20, 5, 8); break;
                case "present": file.Bytes[file.Offsets["parent"]] = 2; Reseal(file); break;
                case "numericLexical": file.Bytes[file.Offsets["generation"] + 4] = (byte)'+'; Reseal(file); break;
                case "nonAsciiToken": file.Bytes[file.Offsets["generation"] + 4] = 200; Reseal(file); break;
            }
            AssertBadBoth(file, code);
        }

        [Test]
        public void EveryTruncationOfTheIndependentSnapshotReturnsNoPartialValue()
        {
            var file = Independent(Populated());
            for (var length = 0; length < file.Bytes.Length; length++)
            {
                var prefix = file.Bytes.Take(length).ToArray();
                Rejected(SaveEnvelopeCodec.Read(new MemoryStream(prefix), file.Expected, Budget()), "LengthMismatch");
                Rejected(SaveEnvelopeCodec.ReadRequirements(new MemoryStream(prefix), file.Expected, Budget()), "LengthMismatch");
            }
        }

        [TestCase("purpose", "InvalidValue")]
        [TestCase("player", "InvalidValue")]
        [TestCase("generation", "InvalidValue")]
        [TestCase("commit", "InvalidValue")]
        [TestCase("parent", "InvalidValue")]
        [TestCase("length", "LengthMismatch")]
        [TestCase("sha", "DigestMismatch")]
        [TestCase("missingSha", "MissingField")]
        public void ExpectedIdentityLengthAndFullDigestAreRequiredEvidence(string change, string code)
        {
            var file = Independent(Populated()); var d = file.Expected;
            file.Expected = new SnapshotDescriptor(change == "purpose" ? SavePurpose.PlayerSave : d.Purpose,
                change == "player" ? "P" : d.PlayerId, change == "generation" ? 2 : d.SaveGeneration,
                change == "commit" ? "C3" : d.CommitId, change == "parent" ? null : d.ParentCommitId,
                change == "length" ? d.TotalLength + 1 : d.TotalLength,
                change == "missingSha" ? null : change == "sha" ? new byte[32] : d.Sha256);
            AssertBadBoth(file, code);
        }

        [TestCase("metadata")]
        [TestCase("envelope")]
        [TestCase("collection")]
        [TestCase("string")]
        [TestCase("numeric")]
        [TestCase("math")]
        public void LimitsAreCheckedAgainByEveryEntryPointBeforeReturningAValue(string kind)
        {
            var input = Populated(); var frozen = Prepared(input); var file = Written(frozen);
            Func<SaveCodecBudget> small = () => new SaveCodecBudget(kind == "math" ? new ExactMathBudget(1) : new ExactMathBudget(),
                maxEnvelopeBytes: kind == "envelope" ? 64UL : 64UL * 1024 * 1024,
                maxMetadataBytes: kind == "metadata" ? 10UL : 4UL * 1024 * 1024,
                maxCollectionEntries: kind == "collection" ? 1 : 65536,
                maxStringCodeUnits: kind == "string" ? 1 : 65536,
                maxNumericTokenBytes: kind == "numeric" ? 0 : 4096);
            Rejected(SaveEnvelopeCodec.Prepare(input, small()), "Limit");
            using (var stream = new MemoryStream())
            { Rejected(SaveEnvelopeCodec.Write(stream, frozen, small()), "Limit"); Assert.AreEqual(0, stream.Length); }
            Rejected(SaveEnvelopeCodec.Read(new MemoryStream(file.Bytes), file.Expected, small()), "Limit");
            Rejected(SaveEnvelopeCodec.ReadRequirements(new MemoryStream(file.Bytes), file.Expected, small()), "Limit");
            CollectionAssert.AreEqual(file.Bytes, Written(frozen).Bytes);
        }

        [TestCase("metadata")]
        [TestCase("body")]
        [TestCase("string")]
        [TestCase("collection")]
        [TestCase("numeric")]
        public void HostileDeclaredLengthsAreRejectedBeforeAllocationOrEntryTraversal(string kind)
        {
            var file = Independent(Populated());
            switch (kind)
            {
                case "metadata": Put(file.Bytes, 12, ulong.MaxValue, 8); break;
                case "body": Put(file.Bytes, 20, ulong.MaxValue - 4096, 8); break;
                case "string": Put(file.Bytes, 61, uint.MaxValue, 4); break;
                case "collection": Put(file.Bytes, file.Offsets["contracts"], uint.MaxValue, 4); break;
                case "numeric": Put(file.Bytes, file.Offsets["generation"], uint.MaxValue, 4); break;
            }
            Reseal(file);
            AssertBadBoth(file, "Limit");
        }

        [Test]
        public void OverflowingHeaderTotalsAreLengthFailuresAndMetadataCannotHideExtraBytes()
        {
            var file = Independent(Populated()); Put(file.Bytes, 20, ulong.MaxValue, 8); Reseal(file);
            AssertBadBoth(file, "LengthMismatch");
            var empty = Independent(Empty());
            empty.Bytes = empty.Bytes.Concat(new byte[] { 0 }).ToArray(); Put(empty.Bytes, 12, 69, 8); Reseal(empty);
            AssertBadBoth(empty, "LengthMismatch");
        }

        [Test]
        public void NonSeekShortReadStreamsStartAtCurrentPositionAndRemainOwnedByTheCaller()
        {
            var file = Independent(Populated());
            foreach (var summaryOnly in new[] { false, true })
            {
                var source = new ProbeStream(file.Bytes) { MaxRead = 1 };
                if (summaryOnly) Accepted(SaveEnvelopeCodec.ReadRequirements(source, file.Expected, Budget()));
                else Accepted(SaveEnvelopeCodec.Read(source, file.Expected, Budget()));
                Assert.AreEqual(file.Bytes.Length, source.BytesRead); Assert.IsFalse(source.Closed); Assert.AreEqual(0, source.FlushCalls);
            }
            var output = new ProbeStream(Array.Empty<byte>());
            output.Write(new byte[] { 8, 9 }, 0, 2);
            var descriptor = SaveEnvelopeCodec.Write(output, Prepared(Populated()), Budget()); Accepted(descriptor);
            Assert.AreEqual(file.Bytes.Length, (int)descriptor.Value.TotalLength);
            CollectionAssert.AreEqual(file.Bytes, output.Output.Skip(2).ToArray());
            Assert.IsFalse(output.Closed); Assert.AreEqual(0, output.FlushCalls);
            using (var positioned = new MemoryStream(new byte[] { 8, 9 }.Concat(file.Bytes).ToArray()))
            { positioned.Position = 2; Accepted(SaveEnvelopeCodec.Read(positioned, file.Expected, Budget())); }
        }

        [TestCase("header")]
        [TestCase("metadata")]
        [TestCase("body")]
        [TestCase("eof")]
        public void EveryReadPhasePropagatesTheOriginalIoException(string phase)
        {
            var file = Independent(Populated());
            var at = phase == "header" ? 3 : phase == "metadata" ? 65 : phase == "body" ? file.Bytes.Length - 2 : file.Bytes.Length;
            foreach (var summaryOnly in new[] { false, true })
            {
                var source = new ProbeStream(file.Bytes) { FailReadAt = at, MaxRead = 2 };
                var error = Assert.Throws<IOException>(() =>
                {
                    if (summaryOnly) SaveEnvelopeCodec.ReadRequirements(source, file.Expected, Budget());
                    else SaveEnvelopeCodec.Read(source, file.Expected, Budget());
                });
                Assert.AreSame(source.Error, error); Assert.IsFalse(source.Closed);
            }
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void EveryWritePhasePropagatesIoWithoutClaimingRollbackOrClosingTheStream(int call)
        {
            var target = new ProbeStream(Array.Empty<byte>()) { FailWriteCall = call };
            var error = Assert.Throws<IOException>(() => SaveEnvelopeCodec.Write(target, Prepared(Populated()), Budget()));
            Assert.AreSame(target.Error, error); Assert.IsFalse(target.Closed); Assert.AreEqual(0, target.FlushCalls);
            if (call > 1) Assert.Greater(target.Output.Length, 0, "Earlier temporary bytes remain written.");
        }

        [Test]
        public void UnreadableUnwritableAndNullRootsHaveExplicitFailureSemantics()
        {
            var file = Independent(Empty()); var frozen = Prepared(Empty());
            Rejected(SaveEnvelopeCodec.Read(new ProbeStream(file.Bytes) { Readable = false }, file.Expected, Budget()), "InvalidValue");
            Rejected(SaveEnvelopeCodec.Write(new ProbeStream(file.Bytes) { Writable = false }, frozen, Budget()), "InvalidValue");
            Assert.Throws<ArgumentNullException>(() => SaveEnvelopeCodec.Prepare(null, Budget()));
            Assert.Throws<ArgumentNullException>(() => SaveEnvelopeCodec.Prepare(Empty(), null));
            Assert.Throws<ArgumentNullException>(() => SaveEnvelopeCodec.Write(null, frozen, Budget()));
            Assert.Throws<ArgumentNullException>(() => SaveEnvelopeCodec.Write(new MemoryStream(), null, Budget()));
            Assert.Throws<ArgumentNullException>(() => SaveEnvelopeCodec.Write(new MemoryStream(), frozen, null));
            Assert.Throws<ArgumentNullException>(() => SaveEnvelopeCodec.Read(null, file.Expected, Budget()));
            Assert.Throws<ArgumentNullException>(() => SaveEnvelopeCodec.Read(new MemoryStream(), null, Budget()));
            Assert.Throws<ArgumentNullException>(() => SaveEnvelopeCodec.ReadRequirements(new MemoryStream(), file.Expected, null));
            Assert.Throws<ArgumentNullException>(() => ExactSaveValueCodec.DecodeInteger(null, Budget()));
            Assert.Throws<ArgumentNullException>(() => ExactSaveValueCodec.DecodeRational(null, Budget()));
            Assert.Throws<ArgumentNullException>(() => ExactSaveValueCodec.EncodeRational(null, Budget()));
            Assert.Throws<ArgumentNullException>(() => ExactSaveValueCodec.EncodeInteger(1, null));
            Assert.Throws<ArgumentNullException>(() => new SaveCodecBudget(null));
        }

        [TestCase(1, 0)]
        [TestCase(10, 1024)]
        [TestCase(100, 4096)]
        [TestCase(1000, 1)]
        public void IncreasingDirectoriesAndBodiesRespectExactByteBoundariesWithoutAGameplayCap(int count, int bytesPerSlice)
        {
            var input = Empty(); var contracts = new List<RequiredSliceContract>(); var slices = new List<SaveSliceInput>();
            for (var i = 0; i < count; i++)
            {
                var contract = new RequiredSliceContract("slice" + i, "owner", 1); contracts.Add(contract);
                slices.Add(new SaveSliceInput { Contract = contract, Bytes = new byte[bytesPerSlice], Requirements = EmptyRequirements() });
            }
            input.RequiredSliceContracts = contracts; input.Slices = slices;
            var file = Written(input); var metadata = Get(file.Bytes, 12, 8);
            Assert.AreEqual(60UL + metadata + (ulong)(count * bytesPerSlice), file.Expected.TotalLength);
            var exact = new SaveCodecBudget(new ExactMathBudget(), file.Expected.TotalLength, metadata, count);
            var summary = SaveEnvelopeCodec.ReadRequirements(new MemoryStream(file.Bytes), file.Expected, exact); Accepted(summary);
            Assert.AreEqual(count, summary.Value.SliceDirectory.Count);
            Rejected(SaveEnvelopeCodec.ReadRequirements(new MemoryStream(file.Bytes), file.Expected,
                new SaveCodecBudget(new ExactMathBudget(), file.Expected.TotalLength - 1)), "Limit");
            TestContext.WriteLine("slices={0}; bodyBytes={1}; metadataBytes={2}; totalBytes={3}", count, count * bytesPerSlice, metadata, file.Bytes.Length);
        }

        [Test]
        public void StartupStreamingAllocationsDoNotGrowByASecondBodyArray()
        {
            var small = Populated(); var large = Populated();
            large.Slices[0].Bytes = new byte[2 * 1024 * 1024];
            var smallFile = Written(small); var largeFile = Written(large);
            var smallBuffers = ObserveSummaryBuffers(smallFile); var largeBuffers = ObserveSummaryBuffers(largeFile);
            Assert.AreEqual(1, smallBuffers.BodyReadBuffers.Count);
            Assert.AreEqual(1, largeBuffers.BodyReadBuffers.Count, "Every body chunk and slice must reuse the same scratch array.");
            Assert.AreEqual(8192, smallBuffers.BodyReadBuffers.Single().Length);
            Assert.AreEqual(8192, largeBuffers.BodyReadBuffers.Single().Length);
            Assert.Greater(largeBuffers.BodyReadCalls, 1, "The large body must be traversed in multiple bounded reads.");
            Assert.LessOrEqual(largeBuffers.MaxBodyReadCount, 8192);
            Assert.AreNotSame(largeFile.Bytes, largeBuffers.BodyReadBuffers.Single());
            var retained = ObservedSource(largeFile);
            var read = SaveEnvelopeCodec.Read(retained, largeFile.Expected, Budget()); Accepted(read);
            Assert.AreEqual(2, retained.BodyReadBuffers.Count, "One array per retained slice, with no aggregate body buffer.");
            for (var i = 0; i < read.Value.Bodies.Length; i++)
            {
                Assert.IsTrue(retained.BodyReadBuffers.Contains(read.Value.Bodies[i]), "Retain the exact array filled by Stream.Read; do not copy it again.");
                Assert.AreNotSame(largeFile.Bytes, read.Value.Bodies[i]);
                Assert.AreEqual(large.Slices[i].Bytes.Length, read.Value.Bodies[i].Length);
                Assert.AreEqual(large.Slices[i].Bytes.Length, read.Value.SliceBytes[i].Count);
                CollectionAssert.AreEqual(Digest(read.Value.Bodies[i]), read.Value.SliceDirectory[i].Sha256);
            }
            Assert.AreEqual(2 * 1024 * 1024 + 1, retained.BodyReadBuffers.Sum(bytes => bytes.Length));
            TestContext.WriteLine("summaryBodyBuffers={0}; summaryBufferBytes={1}; summaryBodyReads={2}; retainedBodyArrays={3}; retainedBodyBytes={4}",
                largeBuffers.BodyReadBuffers.Count, largeBuffers.BodyReadBuffers.Sum(bytes => bytes.Length),
                largeBuffers.BodyReadCalls, retained.BodyReadBuffers.Count, retained.BodyReadBuffers.Sum(bytes => bytes.Length));
        }

        private static ProbeStream ObserveSummaryBuffers(FileImage file)
        {
            // Unity's current-thread allocation counter returned zero even for a retained 2 MiB array.
            // Observe actual array identities/lengths at the stream seam; this does not claim total heap peak.
            var source = ObservedSource(file);
            Accepted(SaveEnvelopeCodec.ReadRequirements(source, file.Expected, Budget()));
            return source;
        }
        private static ProbeStream ObservedSource(FileImage file)
        { return new ProbeStream(file.Bytes) { BodyStartsAt = 60 + (int)Get(file.Bytes, 12, 8) }; }
        private static SaveCodecBudget Budget() { return new SaveCodecBudget(new ExactMathBudget()); }
        private static SaveEnvelope Prepared(SaveEnvelopeInput input)
        { var result = SaveEnvelopeCodec.Prepare(input, Budget()); Accepted(result); return result.Value; }
        private static void Accepted<T>(SaveCodecResult<T> result)
        { Assert.IsTrue(result.IsAccepted, result.RejectionCode + " at " + result.FieldPath + " / " + result.LimitReason); Assert.IsNull(result.RejectionCode); }
        private static void Rejected<T>(SaveCodecResult<T> result, string code)
        { Assert.IsFalse(result.IsAccepted); Assert.AreEqual(code, result.RejectionCode, result.FieldPath); Assert.IsNotEmpty(result.FieldPath); Assert.IsNull(result.Value); }
        private static void AssertBadBoth(FileImage file, string code)
        {
            Rejected(SaveEnvelopeCodec.Read(new MemoryStream(file.Bytes), file.Expected, Budget()), code);
            Rejected(SaveEnvelopeCodec.ReadRequirements(new MemoryStream(file.Bytes), file.Expected, Budget()), code);
        }
        private static SaveRequirements EmptyRequirements(IReadOnlyList<string> features = null)
        { return new SaveRequirements(Array.Empty<SaveBinding>(), Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>(), features ?? Array.Empty<string>()); }
        private static SaveRequirements Requirements(IReadOnlyList<SaveBinding> bindings, IReadOnlyList<string> features = null)
        { return new SaveRequirements(bindings, new[] { "r" }, new[] { "n" }, new[] { "q" }, features ?? Array.Empty<string>()); }
        private static SaveBinding Binding(SaveBindingKind kind, string package = "pkg", string fingerprint = "fp", string rule = "r", IReadOnlyList<string> notes = null)
        {
            var candidate = (byte)kind >= 3; var level = kind == SaveBindingKind.Definition || kind == SaveBindingKind.CandidateDefinition;
            return new SaveBinding(kind, candidate ? null : package, candidate ? "draft" : null,
                candidate ? BigInteger.Parse("9007199254740993", CultureInfo.InvariantCulture) : (BigInteger?)null,
                fingerprint, rule, "n", "q", candidate ? notes ?? new[] { "source" } : null,
                level ? "level" : null, level ? "version" : null);
        }
        private static SaveEnvelopeInput Empty()
        {
            return new SaveEnvelopeInput { Purpose = SavePurpose.CandidateValidation, PlayerId = "p", SaveGeneration = 1,
                CommitId = "c", ParentCommitId = null, RequiredSliceContracts = new List<RequiredSliceContract>(),
                Slices = new List<SaveSliceInput>(), CommitIndex = new List<SaveCommitIndexEntry>
                { new SaveCommitIndexEntry(1, "c", null, null, null, new List<string>()) } };
        }
        private static SaveEnvelopeInput Populated()
        {
            var input = Empty(); input.SaveGeneration = 3; input.CommitId = "c3"; input.ParentCommitId = "c2";
            input.CommitIndex = new List<SaveCommitIndexEntry>
            {
                new SaveCommitIndexEntry(1, "c1", null, 10, new byte[32], new List<string> { "op1" }),
                new SaveCommitIndexEntry(2, "c2", "c1", 11, Enumerable.Repeat((byte)2, 32).ToArray(), new List<string>()),
                new SaveCommitIndexEntry(3, "c3", "c2", null, null, new List<string> { "op3" })
            };
            var a = new RequiredSliceContract("a", "m1", 7); var b = new RequiredSliceContract("b", "m2", uint.MaxValue);
            input.RequiredSliceContracts = new List<RequiredSliceContract> { a, b };
            input.Slices = new List<SaveSliceInput>
            {
                new SaveSliceInput { Contract = a, Bytes = new byte[] { 1, 2, 3 }, Requirements = EmptyRequirements(new List<string> { "x" }) },
                new SaveSliceInput { Contract = b, Bytes = new byte[] { 4 }, Requirements = EmptyRequirements(new List<string> { "y", "x" }) }
            };
            return input;
        }
        private static void ReplaceIndex(SaveEnvelopeInput input, int i, BigInteger? generation = null, string commit = null,
            string parent = null, bool replaceParent = false, ulong? length = null, byte[] digest = null,
            bool replaceDigest = false, IReadOnlyList<string> operations = null, bool replaceOperations = false)
        {
            var rows = (List<SaveCommitIndexEntry>)input.CommitIndex; var row = rows[i];
            rows[i] = new SaveCommitIndexEntry(generation ?? row.Generation, commit ?? row.CommitId,
                replaceParent ? parent : row.ParentCommitId, replaceDigest ? length : row.SnapshotLength,
                replaceDigest ? digest : row.SnapshotSha256, replaceOperations ? operations : row.OperationIds);
        }
        private sealed class FileImage
        {
            internal byte[] Bytes;
            internal SnapshotDescriptor Expected;
            internal Dictionary<string, int> Offsets = new Dictionary<string, int>();
        }
        private static FileImage Written(SaveEnvelopeInput input) { return Written(Prepared(input)); }
        private static FileImage Written(SaveEnvelope envelope)
        {
            using (var stream = new MemoryStream())
            {
                var result = SaveEnvelopeCodec.Write(stream, envelope, Budget()); Accepted(result);
                return new FileImage { Bytes = stream.ToArray(), Expected = result.Value };
            }
        }

        // A small test-only wire builder intentionally permits invalid metadata. It does not call the codec,
        // normalize values, calculate a requirements union, or use the production field walker.
        private static FileImage Independent(SaveEnvelopeInput input, SaveRequirements recovery = null)
        {
            var file = new FileImage(); var body = input.Slices.SelectMany(s => s.Bytes).ToArray();
            using (var metadata = new MemoryStream())
            using (var w = new BinaryWriter(metadata, Encoding.ASCII, true))
            {
                Action<string> mark = name => file.Offsets[name] = 60 + (int)metadata.Position;
                w.Write((byte)input.Purpose.Value); Text(w, input.PlayerId); mark("generation"); Token(w, input.SaveGeneration.Value);
                Text(w, input.CommitId); mark("parent"); Optional(w, input.ParentCommitId);
                mark("contracts"); w.Write((uint)input.RequiredSliceContracts.Count);
                foreach (var c in input.RequiredSliceContracts) Contract(w, c);
                w.Write((uint)input.Slices.Count); var offset = 0UL;
                for (var i = 0; i < input.Slices.Count; i++)
                {
                    var s = input.Slices[i]; Contract(w, s.Contract); mark("d" + i + ".offset"); w.Write(offset);
                    mark("d" + i + ".length"); w.Write((ulong)s.Bytes.Length); mark("d" + i + ".sha"); w.Write(Digest(s.Bytes));
                    Req(w, s.Requirements); offset += (ulong)s.Bytes.Length;
                }
                w.Write((uint)input.CommitIndex.Count);
                for (var i = 0; i < input.CommitIndex.Count; i++)
                {
                    var row = input.CommitIndex[i]; Token(w, row.Generation); Text(w, row.CommitId); Optional(w, row.ParentCommitId);
                    w.Write((byte)(row.SnapshotLength.HasValue ? 1 : 0));
                    if (row.SnapshotLength.HasValue) { w.Write(row.SnapshotLength.Value); w.Write(row.SnapshotSha256.ToArray()); }
                    w.Write((uint)row.OperationIds.Count);
                    for (var j = 0; j < row.OperationIds.Count; j++) { mark("i" + i + ".op"); Text(w, row.OperationIds[j]); }
                }
                Req(w, recovery ?? EmptyRequirements(input.Slices.Count == 0 ? Array.Empty<string>() : new[] { "x", "y" }));
                using (var full = new MemoryStream())
                using (var h = new BinaryWriter(full, Encoding.ASCII, true))
                {
                    h.Write(Encoding.ASCII.GetBytes("FMSAVE01")); h.Write(1U); h.Write((ulong)metadata.Length);
                    h.Write((ulong)body.Length); h.Write(Digest(body)); h.Write(metadata.ToArray()); h.Write(body);
                    file.Bytes = full.ToArray();
                }
            }
            file.Expected = new SnapshotDescriptor(input.Purpose.Value, input.PlayerId, input.SaveGeneration.Value,
                input.CommitId, input.ParentCommitId, (ulong)file.Bytes.Length, Digest(file.Bytes));
            return file;
        }
        private static void Text(BinaryWriter w, string s)
        { w.Write((uint)s.Length); foreach (var c in s) w.Write((ushort)c); }
        private static void Optional(BinaryWriter w, string s) { w.Write((byte)(s == null ? 0 : 1)); if (s != null) Text(w, s); }
        private static void Token(BinaryWriter w, BigInteger n)
        { var token = Encoding.ASCII.GetBytes(n.ToString(CultureInfo.InvariantCulture)); w.Write((uint)token.Length); w.Write(token); }
        private static void Contract(BinaryWriter w, RequiredSliceContract c) { Text(w, c.SliceId); Text(w, c.OwnerId); w.Write(c.SchemaVersion); }
        private static void Strings(BinaryWriter w, IReadOnlyList<string> strings)
        { w.Write((uint)strings.Count); foreach (var s in strings) Text(w, s); }
        private static void Req(BinaryWriter w, SaveRequirements r)
        {
            w.Write((uint)r.Bindings.Count);
            foreach (var b in r.Bindings)
            {
                w.Write((byte)b.Kind); var candidate = (byte)b.Kind >= 3;
                Text(w, candidate ? b.DraftId : b.PackageId); if (candidate) Token(w, b.DraftRevision.Value);
                Text(w, b.ContentFingerprint); Text(w, b.RuleVersion); Text(w, b.NumericContractVersion); Text(w, b.RandomContractVersion);
                if (candidate) Strings(w, b.SourceNotes);
                if (b.Kind == SaveBindingKind.Definition || b.Kind == SaveBindingKind.CandidateDefinition) { Text(w, b.LevelId); Text(w, b.LevelVersion); }
            }
            Strings(w, r.RuleVersions); Strings(w, r.NumericContractVersions); Strings(w, r.RandomContractVersions); Strings(w, r.FeatureIds);
        }
        private static byte[] Digest(byte[] bytes) { using (var sha = SHA256.Create()) return sha.ComputeHash(bytes); }
        private static byte[] Hex(string text)
        { var bytes = new byte[text.Length / 2]; for (var i = 0; i < bytes.Length; i++) bytes[i] = Convert.ToByte(text.Substring(i * 2, 2), 16); return bytes; }
        private static string ToHex(byte[] bytes) { return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant(); }
        private static void Put(byte[] bytes, int position, ulong value, int length)
        { for (var i = 0; i < length; i++) bytes[position + i] = (byte)(value >> (8 * i)); }
        private static ulong Get(byte[] bytes, int position, int length)
        { var result = 0UL; for (var i = 0; i < length; i++) result |= (ulong)bytes[position + i] << (8 * i); return result; }
        private static void Reseal(FileImage file)
        {
            var d = file.Expected;
            file.Expected = new SnapshotDescriptor(d.Purpose, d.PlayerId, d.SaveGeneration, d.CommitId,
                d.ParentCommitId, (ulong)file.Bytes.Length, Digest(file.Bytes));
        }

        private sealed class ProbeStream : Stream
        {
            private readonly byte[] input;
            private readonly MemoryStream output = new MemoryStream();
            private int writes;
            internal readonly IOException Error = new IOException("injected I/O failure");
            internal int BytesRead;
            internal int MaxRead = int.MaxValue;
            internal int FailReadAt = -1;
            internal int FailWriteCall = -1;
            internal bool Readable = true;
            internal bool Writable = true;
            internal bool Closed;
            internal int FlushCalls;
            internal int BodyStartsAt = int.MaxValue;
            internal readonly HashSet<byte[]> BodyReadBuffers = new HashSet<byte[]>();
            internal int BodyReadCalls;
            internal int MaxBodyReadCount;
            internal byte[] Output => output.ToArray();
            internal ProbeStream(byte[] input) { this.input = input; }
            public override bool CanRead => Readable;
            public override bool CanSeek => false;
            public override bool CanWrite => Writable;
            public override long Length => throw new NotSupportedException();
            public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
            public override long Seek(long offset, SeekOrigin origin) { throw new NotSupportedException(); }
            public override void SetLength(long value) { throw new NotSupportedException(); }
            public override void Flush() { FlushCalls++; }
            public override int Read(byte[] buffer, int offset, int count)
            {
                if (FailReadAt >= 0 && BytesRead >= FailReadAt) throw Error;
                var size = Math.Min(Math.Min(count, MaxRead), input.Length - BytesRead);
                if (FailReadAt >= 0) size = Math.Min(size, FailReadAt - BytesRead);
                if (BytesRead >= BodyStartsAt && size > 0)
                { BodyReadBuffers.Add(buffer); BodyReadCalls++; MaxBodyReadCount = Math.Max(MaxBodyReadCount, count); }
                Array.Copy(input, BytesRead, buffer, offset, size); BytesRead += size; return size;
            }
            public override void Write(byte[] buffer, int offset, int count)
            { writes++; if (writes == FailWriteCall) throw Error; output.Write(buffer, offset, count); }
            protected override void Dispose(bool disposing) { Closed = true; base.Dispose(disposing); }
        }
    }
}
