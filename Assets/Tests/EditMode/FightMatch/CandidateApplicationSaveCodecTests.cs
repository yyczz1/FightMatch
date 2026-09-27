using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using NUnit.Framework;
using static FightMatch.Core.Tests.BusinessSaveScenario;

namespace FightMatch.Core.Tests
{
    [TestFixture]
    public class CandidateApplicationSaveCodecTests
    {
        [Test]
        public void SixSlicesKeepIndependentFiveBodiesAndFullDescriptorAndExactRequirements()
        {
            var s = new ApplicationScenario();
            s.Enter();
            var full = s.Envelope;
            var five = Accept(CandidateBusinessSaveCodec.Encode(s.Head.Business, s.Head.Header, Budget()));
            CollectionAssert.AreEqual(new[] { "M02", "M03", "M04", "M05", "M06", "M07" }, full.RequiredSliceContracts.Select(x => x.OwnerId));
            for (var i = 0; i < 5; i++)
            {
                CollectionAssert.AreEqual(five.Bodies[i], full.Bodies[i + 1]);
                Assert.IsTrue(BusinessRequirements.Same(five.SliceDirectory[i].Requirements, full.SliceDirectory[i + 1].Requirements));
            }
            var fullDescriptor = Accept(SaveEnvelopeCodec.Write(Stream.Null, full, Budget()));
            var projectionDescriptor = Accept(SaveEnvelopeCodec.Write(Stream.Null, five, Budget()));
            Assert.Greater(fullDescriptor.TotalLength, projectionDescriptor.TotalLength);
            CollectionAssert.AreEqual(fullDescriptor.Sha256, s.Head.Descriptor.Sha256);
            Assert.IsFalse(projectionDescriptor.Sha256.SequenceEqual(s.Head.Descriptor.Sha256));
            var requirements = full.SliceDirectory[0].Requirements;
            CollectionAssert.AreEqual(new[] { "fm.candidate.application.v1" }, requirements.FeatureIds);
            CollectionAssert.AreEqual(new[] { SaveBindingKind.CandidateContent, SaveBindingKind.CandidateDefinition }, requirements.Bindings.Select(x => x.Kind));
            Assert.AreEqual(s.Lookup("enter").Begin.Level.LevelId, requirements.Bindings[1].LevelId);
            CollectionAssert.AreEqual(s.Intents["init"].Context.SourceNotes, requirements.Bindings[0].SourceNotes);
            Rejected(CandidateApplicationSaveCodec.Decode(five, Budget()), "MissingApplicationRecords");
            Rejected(CandidateBusinessSaveCodec.Decode(full, Budget()), "UnsupportedSchema");
        }

        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)] [TestCase(5)]
        public void EverySliceRejectsUnknownSchemaAndTruncationAndTrailingBytes(int slice)
        {
            var s = new ApplicationScenario();
            foreach (var mutation in new[] { 0, 1, 2, 3 })
            {
                var bodies = s.Envelope.Bodies.Select(x => (byte[])x.Clone()).ToArray();
                if (mutation == 0) bodies[slice][8] = 2;
                if (mutation == 1) bodies[slice][12] = 99;
                if (mutation == 2) bodies[slice] = bodies[slice].Take(bodies[slice].Length - 1).ToArray();
                if (mutation == 3) bodies[slice] = bodies[slice].Concat(new byte[] { 0 }).ToArray();
                Rejected(CandidateApplicationSaveCodec.Decode(Repack(s.Envelope, bodies), Budget()));
            }
        }

        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)] [TestCase(5)] [TestCase(6)]
        public void DirectoryOrderOwnerPurposeAndRequirementsCannotLie(int mutation)
        {
            var s = new ApplicationScenario();
            var envelope = s.Envelope;
            var slices = Slices(envelope);
            if (mutation == 0) slices.Reverse();
            if (mutation == 1) slices[0].Contract = new RequiredSliceContract("fm.m02.application", "M09", 1);
            if (mutation == 2) slices[0].Contract = new RequiredSliceContract("fm.m02.application", "M02", 2);
            if (mutation == 3) slices.Add(new SaveSliceInput { Contract = new RequiredSliceContract("extra", "M09", 1), Bytes = new byte[0], Requirements = EmptyRequirements() });
            if (mutation == 4) slices[0].Requirements = EmptyRequirements();
            if (mutation == 5)
            {
                var old = slices[0].Requirements;
                slices[0].Requirements = new SaveRequirements(old.Bindings, old.RuleVersions, old.NumericContractVersions, old.RandomContractVersions, new[] { "wrong" });
            }
            if (mutation != 6) envelope = Repack(envelope, slices);
            else envelope = new SaveEnvelope(SavePurpose.PlayerSave, envelope.PlayerId, envelope.SaveGeneration, envelope.CommitId, envelope.ParentCommitId,
                envelope.RequiredSliceContracts.ToArray(), envelope.SliceDirectory.ToArray(), envelope.CommitIndex.ToArray(), envelope.RecoveryRequirements,
                envelope.BodyLength, envelope.BodySha256.ToArray(), envelope.Bodies);
            Rejected(CandidateApplicationSaveCodec.Decode(envelope, Budget()), mutation == 6 ? "UnsupportedBinding" : mutation <= 3 ? "UnsupportedSchema" : "InconsistentBinding");
        }

        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)] [TestCase(5)]
        public void MalformedApplicationCountsIntentBytesAndNumericTokensAreRejected(int mutation)
        {
            var s = new ApplicationScenario();
            var bytes = s.Envelope.Bodies[0].ToArray();
            var count = 17 + s.Envelope.PlayerId.Length * 2;
            var intentLengthOffset = count + 4;
            var length = BitConverter.ToInt32(bytes, intentLengthOffset);
            var generation = intentLengthOffset + 4 + length;
            if (mutation == 0) Put(bytes, count, uint.MaxValue);
            if (mutation == 1) Put(bytes, intentLengthOffset, uint.MaxValue);
            if (mutation == 2) bytes[intentLengthOffset + 4] = 0;
            if (mutation == 3) bytes[generation + 4] = (byte)'0';
            if (mutation == 4) Put(bytes, generation, uint.MaxValue);
            if (mutation == 5) bytes[generation + 4] = (byte)'+';
            var bodies = s.Envelope.Bodies.ToArray();
            bodies[0] = bytes;
            Rejected(CandidateApplicationSaveCodec.Decode(Repack(s.Envelope, bodies), Budget()));
        }

        [TestCase(false)] [TestCase(true)]
        public void MissingApplicationRecordOrFalsePersistedAnchorCannotBeRepairedByIndex(bool anchor)
        {
            var s = new ApplicationScenario(3);
            s.Enter();
            s.Attack("a", 0);
            var bytes = s.Envelope.Bodies[0].ToArray();
            var start = LastRecord(bytes, s.Envelope.PlayerId, s.Head.Records, out var result);
            if (anchor) bytes[result + 4] ^= 1;
            else
            {
                var count = 17 + s.Envelope.PlayerId.Length * 2;
                Put(bytes, count, 2);
                bytes = bytes.Take(start).Concat(new byte[4]).ToArray();
            }
            var bodies = s.Envelope.Bodies.ToArray();
            bodies[0] = bytes;
            Rejected(CandidateApplicationSaveCodec.Decode(Repack(s.Envelope, bodies), Budget()));
        }

        [TestCase(false)] [TestCase(true)]
        public void ClosedAttemptStillRequiresTerminalRunAndRemovedBranches(bool removeRollback)
        {
            var s = new ApplicationScenario(3);
            s.Enter();
            s.Attack("a", 0);
            s.Rollback("r", "a");
            s.Exit();
            var old = s.Head.Business;
            var business = Accept(CandidateBusinessSaveCodec.Prepare(new CandidateBusinessInput(old.PlayerId, old.Character, old.Inventory, old.Progression,
                old.Rewards, old.ActiveHistory, removeRollback ? old.RetainedRuns : new CandidateBattleRun[0],
                removeRollback ? new CandidateRollbackRecord[0] : old.RetainedRollbacks), Budget()));
            // Removing the terminal root alone is redundant when the identical restored run
            // is already retained by the rollback. Remove both to test genuinely absent history.
            if (!removeRollback) business = Accept(CandidateBusinessSaveCodec.Prepare(new CandidateBusinessInput(old.PlayerId, old.Character, old.Inventory,
                old.Progression, old.Rewards, null, new CandidateBattleRun[0], new CandidateRollbackRecord[0]), Budget()));
            Rejected(CandidateApplicationSaveCodec.Decode(WithBusiness(s.Envelope, business, s.Head.Header), Budget()), "IncompleteOperationHistory");
        }

        [TestCase(0)] [TestCase(1)] [TestCase(2)]
        public void PersistedContinuationMustBePresentUniqueAndMatchReport(int mutation)
        {
            var s = new ApplicationScenario();
            s.Enter();
            s.Win();
            var bytes = s.Envelope.Bodies[0].ToArray();
            LastRecord(bytes, s.Envelope.PlayerId, s.Head.Records, out var result);
            var route = result + 4 + BitConverter.ToInt32(bytes, result) * 2;
            if (mutation == 0) { bytes = bytes.Take(route + 4).ToArray(); Put(bytes, route, 0); }
            if (mutation == 1) Put(bytes, route, 2);
            if (mutation == 2) bytes[route + 8] ^= 1;
            var bodies = s.Envelope.Bodies.ToArray();
            bodies[0] = bytes;
            Rejected(CandidateApplicationSaveCodec.Decode(Repack(s.Envelope, bodies), Budget()), "InvalidContinuation");
            Assert.IsNotNull(s.Head.Continuation);
        }

        [Test]
        public void BudgetMeasuresAggregateSixSlicesIncludingMetadataBeforeAllocatingM02()
        {
            var s = new ApplicationScenario();
            var full = s.Head.Descriptor.TotalLength;
            var tooSmall = new SaveCodecBudget(Math(), maxEnvelopeBytes: full - 1);
            var result = CandidateApplicationSaveCodec.Encode(s.Candidate, s.Head.Header, tooSmall);
            Rejected(result, "Limit");
            Assert.AreEqual("EnvelopeBytes", result.LimitReason);
            Assert.AreEqual(full, result.RequiredAtLeast);
            Assert.AreEqual(full - 1, result.Allowed);
            Assert.AreEqual("Envelope.TotalLength", result.FieldPath);
            Accept(CandidateApplicationSaveCodec.Encode(s.Candidate, s.Head.Header, new SaveCodecBudget(Math(), maxEnvelopeBytes: full)));
            Rejected(CandidateApplicationSaveCodec.Decode(s.Envelope, new SaveCodecBudget(Math(), maxEnvelopeBytes: full - 1)), "Limit");
        }

        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)]
        public void CollectionStringNumericAndMetadataLimitsPreserveOriginalDiagnostics(int kind)
        {
            var s = new ApplicationScenario();
            var budget = kind == 0 ? new SaveCodecBudget(Math(), maxCollectionEntries: 1) : kind == 1 ? new SaveCodecBudget(Math(), maxStringCodeUnits: 1) :
                kind == 2 ? new SaveCodecBudget(Math(), maxNumericTokenBytes: 0) : new SaveCodecBudget(Math(), maxMetadataBytes: 1);
            var result = CandidateApplicationSaveCodec.Decode(s.Envelope, budget);
            Rejected(result, "Limit");
            Assert.IsNotNull(result.RequiredAtLeast);
            Assert.IsNotNull(result.Allowed);
            Assert.Greater(result.RequiredAtLeast, result.Allowed);
            var intent = ApplicationScenario.Simple(CandidateApplicationKind.Attack);
            Assert.Greater(Accept(CandidateApplicationProtocol.PrepareIntent(intent, new SaveCodecBudget(Math(), maxCollectionEntries: 2))).CanonicalBytes.Count, 2);
        }

        [Test]
        public void CallerMathBudgetIsSharedAcrossNestedDecodeAndNotResetOnLookup()
        {
            var s = new ApplicationScenario();
            var measure = Budget();
            Accept(CandidateApplicationSaveCodec.Decode(s.Envelope, measure));
            var used = measure.Math.PrimitiveStepsUsed;
            Assert.Greater(used, 1);
            var budget = new SaveCodecBudget(new ExactMathBudget(maxPrimitiveSteps: checked((int)used - 1)));
            var result = CandidateApplicationSaveCodec.Decode(s.Envelope, budget);
            Rejected(result, "Limit");
            Assert.AreEqual("Budget.Math", result.FieldPath);
            Assert.AreEqual((ulong)used - 1, result.Allowed);
            var empty = new SaveCodecBudget(new ExactMathBudget(maxPrimitiveSteps: 0));
            Rejected(CandidateApplicationProtocol.Lookup(s.Head, s.Intents["init"], empty), "Limit");
        }

        [Test]
        public void ARealButUnrecordedRecoveryAdvanceCannotAppearInTheCurrentOwner()
        {
            var s = new ApplicationScenario(3, hp: 1);
            s.Enter();
            s.Attack("down", 0);
            s.Exit(down: true);
            s.Recover("first", ApplicationScenario.Clock(101, R(1)));
            var before = s.Head.Business;
            var second = CandidateRecoveryClock.Advance(before.Character, "recovery", ApplicationScenario.Clock(102, R(2)), before.Character.StateRevision, Math());
            Assert.IsTrue(second.IsAccepted);
            var business = Accept(CandidateBusinessSaveCodec.Prepare(new CandidateBusinessInput(before.PlayerId, second.Next, before.Inventory,
                before.Progression, before.Rewards, before.ActiveHistory, before.RetainedRuns, before.RetainedRollbacks), Budget()));
            Rejected(CandidateApplicationSaveCodec.Decode(WithBusiness(s.Envelope, business, s.Head.Header), Budget()), "IncompleteOperationHistory");
        }

        [Test]
        public void ValidEnvelopeCannotPairAnIgnoredReceiptWithANonRegressingIntent()
        {
            var s = new ApplicationScenario(3, hp: 1);
            s.Enter();
            s.Attack("down", 0);
            s.Exit(down: true);
            var input = s.Intent(CandidateApplicationKind.AdvanceRecovery, "backward");
            input.AdvanceRecovery = new CandidateApplicationRecoveryInput { CharacterId = s.Domain.Character.CharacterId, RecoveryId = "recovery",
                ExpectedCharacterRevision = s.Domain.Character.StateRevision, TimeSample = ApplicationScenario.Clock(101) };
            var actual = s.Recover("backward", ApplicationScenario.Clock(99));
            Assert.AreEqual(CandidateGrowthOutcome.IgnoredTimeRegression, actual.Outcome);
            var original = s.Intents["backward"].CanonicalBytes.ToArray();
            var replacement = Accept(CandidateApplicationProtocol.PrepareIntent(input, Budget())).CanonicalBytes.ToArray();
            var body = s.Envelope.Bodies[0];
            var matches = Enumerable.Range(4, body.Length - original.Length - 3)
                .Where(i => body.Skip(i).Take(original.Length).SequenceEqual(original)).ToArray();
            Assert.AreEqual(1, matches.Length);
            var offset = matches[0] - 4;
            Assert.AreEqual(original.Length, BitConverter.ToInt32(body, offset));
            var bodies = s.Envelope.Bodies.ToArray();
            bodies[0] = body.Take(offset).Concat(BitConverter.GetBytes(replacement.Length)).Concat(replacement)
                .Concat(body.Skip(offset + 4 + original.Length)).ToArray();
            var envelope = Repack(s.Envelope, bodies);
            for (var i = 1; i < 6; i++) CollectionAssert.AreEqual(s.Envelope.Bodies[i], envelope.Bodies[i]);
            using (var stream = new MemoryStream())
            {
                var descriptor = Accept(SaveEnvelopeCodec.Write(stream, envelope, Budget()));
                stream.Position = 0;
                var loaded = Accept(SaveEnvelopeCodec.Read(stream, descriptor, Budget()));
                Rejected(CandidateApplicationSaveCodec.Decode(loaded, Budget()), "InconsistentBinding");
            }
        }

        [Test]
        public void HistoricalIgnoredReceiptUsesItsOriginalPeriodAfterDomainChangeAndCompletion()
        {
            var s = new ApplicationScenario(3, hp: 1);
            s.Enter();
            s.Attack("down", 0);
            s.Exit(down: true);
            s.Recover("prime", ApplicationScenario.Clock(-500, R(2, 3)));
            s.Recover("old-ignored", ApplicationScenario.Clock(900, R(1, 3)));
            var commit = s.Head.Header.CommitId;
            var generation = s.Head.Header.SaveGeneration;
            var revision = s.Domain.Character.StateRevision;
            var changed = ApplicationScenario.Clock(-400, R(2), "clock-other");
            Assert.AreEqual(CandidateGrowthOutcome.Applied, s.Recover("change-domain", changed).Outcome);
            Assert.AreEqual(CandidateGrowthOutcome.Unchanged, s.Recover("same", changed).Outcome);
            var laterIgnored = s.Recover("new-ignored", ApplicationScenario.Clock(-399, R(1), "clock-other"));
            Assert.AreEqual(CandidateGrowthOutcome.IgnoredTimeRegression, laterIgnored.Outcome);
            Assert.AreEqual(CandidateTimeAnomaly.DomainChanged, laterIgnored.RecoveryPeriod.Anomaly);
            Assert.AreEqual(CandidateTimeAnomaly.ClockBackward | CandidateTimeAnomaly.DomainChanged, laterIgnored.Anomaly);
            s.Recover("complete", ApplicationScenario.Clock(180100));
            var included = s.Recover("included", ApplicationScenario.Clock(-999, R(0), "unrelated"), BigInteger.One << 80);
            Assert.AreEqual(CandidateGrowthOutcome.AlreadyIncluded, included.Outcome);
            var loaded = Accept(CandidateApplicationSaveCodec.Decode(s.Envelope, Budget()));
            var old = Accept(CandidateApplicationProtocol.Lookup(loaded, s.Intents["old-ignored"], Budget()));
            Assert.AreEqual(commit, old.OriginalCommitId);
            Assert.AreEqual(generation, old.OriginalGeneration);
            Assert.AreEqual(revision, old.Recovery.AfterCharacterRevision);
            Assert.AreEqual(CandidateGrowthOutcome.IgnoredTimeRegression, old.Recovery.Outcome);
            Assert.AreEqual(new BigInteger(-500), old.Recovery.Period.LastAcceptedSample.WallUtcMilliseconds);
            Assert.AreEqual("clock-original", old.Recovery.Period.LastAcceptedSample.MonotonicScopeId);
            Same(R(2, 3), old.Recovery.Period.Elapsed);
            Same(R(2, 3), old.Recovery.Period.LastAcceptedSample.MonotonicElapsedMilliseconds);
            Assert.AreEqual(CandidateTimeAnomaly.None, old.Recovery.Period.Anomaly);
            Assert.AreEqual(CandidateTimeAnomaly.ClockBackward, old.Recovery.ResultAnomaly);
            Assert.IsFalse(old.Recovery.Period.IsCompleted);
            Assert.IsTrue(loaded.Business.Character.RecoveryPeriods[0].IsCompleted);
            Assert.AreEqual(CandidateTimeAnomaly.None, s.Lookup("included").Recovery.ResultAnomaly);
            var five = Accept(CandidateBusinessSaveCodec.Encode(loaded.Business, loaded.Header, Budget()));
            for (var i = 0; i < 5; i++) CollectionAssert.AreEqual(five.Bodies[i], s.Envelope.Bodies[i + 1]);
            var measure = Budget();
            Accept(CandidateApplicationSaveCodec.Decode(s.Envelope, measure));
            Rejected(CandidateApplicationSaveCodec.Decode(s.Envelope,
                new SaveCodecBudget(new ExactMathBudget(maxPrimitiveSteps: checked((int)measure.Math.PrimitiveStepsUsed - 1)))), "Limit");
        }

        private static void Put(byte[] bytes, int offset, uint value)
        { Array.Copy(BitConverter.GetBytes(value), 0, bytes, offset, 4); }

        private static int LastRecord(byte[] bytes, string player, IReadOnlyList<CandidateApplicationRecord> records, out int resultOffset)
        {
            using (var reader = new BinaryReader(new MemoryStream(bytes)))
            {
                reader.BaseStream.Position = 21 + player.Length * 2;
                var start = 0;
                resultOffset = 0;
                for (var i = 0; i < records.Count; i++)
                {
                    start = (int)reader.BaseStream.Position;
                    Skip(reader, 1); // Raw canonical intent bytes.
                    Skip(reader, 1); // Generation ASCII token.
                    Skip(reader, 2); // CommitId UTF16.
                    resultOffset = (int)reader.BaseStream.Position;
                    var kind = records[i].Intent.Kind;
                    var fields = kind == CandidateApplicationKind.EnterAttempt ? 3 : kind == CandidateApplicationKind.SettleVictory || kind == CandidateApplicationKind.RestartAttempt ? 2 :
                        kind == CandidateApplicationKind.Attack || kind == CandidateApplicationKind.Link || kind == CandidateApplicationKind.ExitAttempt ? 1 : 0;
                    Assert.AreNotEqual(CandidateApplicationKind.AdvanceRecovery, kind, "This corruption probe is limited to non-recovery records.");
                    for (var j = 0; j < fields; j++) Skip(reader, 2);
                }
                return start;
            }
        }

        private static void Skip(BinaryReader reader, int width)
        { var length = reader.ReadUInt32(); reader.BaseStream.Position += length * width; }

        private static SaveEnvelope WithBusiness(SaveEnvelope original, CandidateBusinessSnapshot business, CandidateBusinessSaveHeader header)
        {
            var five = Accept(CandidateBusinessSaveCodec.Encode(business, header, Budget()));
            var slices = Slices(five);
            slices.Insert(0, Slices(original)[0]);
            return Repack(original, slices);
        }
    }
}
