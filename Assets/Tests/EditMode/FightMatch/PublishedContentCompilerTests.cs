using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using FightMatch.Content;
using FlowPuzzle.Core;
using NUnit.Framework;
using static FightMatch.Core.Tests.PublishedContentTestData;

namespace FightMatch.Core.Tests
{
    public class PublishedContentCompilerTests
    {
        private PreparedPublication actual;
        [OneTimeSetUp] public void PrepareApprovedSource() { actual = Prepare(Source()); }
        [Test] public void ApprovedRealMappingsAndEveryFrozenParameterAreExact()
        {
            var s = Source(); Assert.AreEqual("package:fightmatch-demo-r1", s.PackageId); Assert.AreEqual("draft:fightmatch-demo-r1", s.DraftId);
            Assert.AreEqual(BigInteger.One, s.Revision); Assert.AreEqual("demo-r1", s.RuleVersion); Assert.AreEqual("RC01", s.NumericContractVersion);
            Assert.AreEqual("PC01+SC01", s.RandomContractVersion); Assert.AreEqual(DemoCoordinateCandidate.AssumedBottomLeft, s.Coordinates);
            Assert.AreEqual(1, s.Levels.Count); var l = s.Levels[0]; Assert.AreEqual("level:ch01-01", l.Level.LevelId); Assert.AreEqual("1", l.Level.LevelVersion);
            Assert.AreEqual("class:warrior", s.Growth.ClassId); Assert.AreEqual("passive:warrior-crit", s.Growth.PassiveDefinitionId);
            Assert.AreEqual("reward:ch01-01", l.Reward.RewardDefinitionId); Assert.AreEqual(new BigInteger(20), l.Reward.BaseExperience);
            CollectionAssert.AreEqual(new[] { "item:tin", "item:wood" }, l.Reward.Materials.Select(m => m.ItemId));
            CollectionAssert.AreEqual(new BigInteger?[] { 2, 0 }, l.Reward.Materials.Select(m => m.Amount));
            var expected = new[] { "1114080859/20000000000", "58364884869/1000000000000", "30540415857/500000000000", "63851473113/1000000000000",
                "33338201811/500000000000", "17388806239/250000000000", "36243771699/500000000000", "75472966607/1000000000000", "9813890083/125000000000",
                "20400402513/250000000000", "21186022963/250000000000", "43969078281/500000000000", "91183460913/1000000000000", "94479714473/1000000000000",
                "19565276097/200000000000", "253058481/2500000000", "837361819/8000000000", "21633259923/200000000000", "55855879121/500000000000",
                "23061331847/200000000000", "4757967709/40000000000", "122639525691/1000000000000", "126379316121/1000000000000", "26033503199/200000000000",
                "26800172907/200000000000", "137879953521/1000000000000", "141805195687/1000000000000", "145785689139/1000000000000", "149810087949/1000000000000",
                "76937749041/500000000000", "78991549063/500000000000" };
            CollectionAssert.AreEqual(expected, s.Parameters.Select(p => p.C.Numerator + "/" + p.C.Denominator));
            for (var i = 0; i < 31; i++)
            {
                var row = actual.Replays[0].Candidate.Parameters[i]; Assert.IsTrue(row.WithinProposedTolerance);
                Assert.AreEqual(0, row.Target.Compare(ExactRational.Create(40 + i, 200, Math()), Math()));
                Assert.AreEqual(BigInteger.One, row.Epsilon.Numerator); Assert.AreEqual(new BigInteger(1000000000), row.Epsilon.Denominator);
            }
            var pairs = actual.Definitions.Levels[0].Faces[0].Pairs;
            CollectionAssert.AreEqual(new[] { "A", "B" }, pairs.Select(p => p.PairId)); CollectionAssert.AreEqual(new[] { 0, 1 }, pairs.Select(p => p.GeometryColorId));
            Assert.IsTrue(pairs.All(p => p.Enemy.EnemyDefinitionId == "enemy:clockwork-infantry"));
            CollectionAssert.AreEqual(new[] { 0, 1 }, pairs.Select(p => p.Enemy.OriginalSlot)); CollectionAssert.AreEqual(new[] { 0, 1 }, pairs.Select(p => p.Enemy.StableOrder));
        }
        [Test] public void NewProfileFreezesCompleteRecipeWithoutPlayerEntropyOrRewardResult()
        {
            var s = Source(); var n = s.NewProfile; Assert.AreEqual("new-profile:default", n.Id); Assert.AreEqual(BigInteger.One, n.RecordVersion);
            Assert.AreEqual("W", n.CharacterId); Assert.AreEqual(BigInteger.One, n.Level); Assert.AreEqual(BigInteger.Zero, n.Experience);
            Assert.AreEqual(new BigInteger(100), n.Hp.Numerator); Assert.AreEqual(0, n.OriginalSlot); CollectionAssert.AreEqual(new[] { 1, 2 }, n.EmptySlots);
            Assert.IsEmpty(n.Inventory); Assert.IsEmpty(n.Carry); Assert.IsEmpty(n.LearnedActiveSkills); Assert.IsNull(n.Recovery);
            CollectionAssert.AreEqual(new[] { "level:ch01-01" }, n.OpenLevels); Assert.IsNull(s.Progression.Levels[0].UnlockAfterLevelId);
            var bytes = actual.NewProfile.CanonicalBytes.ToArray(); var text = Encoding.UTF8.GetString(bytes);
            Assert.IsFalse(text.Contains("PlayerId") || text.Contains("OperationId") || text.Contains("Seed") || text.Contains("isolated:P1"));
            Assert.AreEqual(PublishedContentCodec.Sha256(bytes), actual.NewProfile.Sha256);
            Assert.Throws<NotSupportedException>(() => ((IList<byte>)actual.NewProfile.CanonicalBytes)[0] = 0);
        }
        [Test] public void FreshGeometryReplayAndRealRewardUseSharedFormalRuleContext()
        {
            var r = actual.Replays.Single(); Assert.IsInstanceOf<PreparedPublishedRuleContext>(r.Binding.Context);
            Assert.IsTrue(actual.Binding.Same(((PreparedPublishedRuleContext)r.Binding.Context).Binding));
            Assert.AreEqual(BattlePhase.WonPendingSettlement, r.Run.CurrentSnapshot.Phase);
            Assert.AreEqual(CandidateBattleOutcome.NormalVictory, r.Report.Outcome); Assert.AreSame(r.Report, r.Reward.Report);
            Assert.AreEqual(2, r.ExecutedSteps.Count); Assert.IsTrue(r.RecordedReplays.All(x => x.Matched == true));
            Assert.AreEqual(2, r.Run.CurrentSnapshot.Board.LockedRoutes.Count); Assert.AreEqual(6, r.Candidate.Routes.Sum(x => x.Cells.Count));
            CollectionAssert.AreEqual(new[] { new FlowPos(0, 0), new FlowPos(0, 1), new FlowPos(1, 1) }, r.Candidate.Routes[0].Cells);
            Assert.AreNotEqual("5db393d0441880fc387ce7db0421bdf855b3b38b20edbfe818a83144bcbb5a12", actual.Binding.ContentFingerprint);
            Assert.AreEqual(PublishedContentCodec.Sha256(actual.PayloadBytes.ToArray()), actual.Binding.ContentFingerprint);
            CollectionAssert.AreEqual(File.ReadAllBytes(SourcePath), actual.SourceBytes);
        }
        [Test] public void CanonicalBytesIgnoreJsonPropertyOrderAndNormalizeSetOrder()
        {
            var s = Source(); s.RequiredCapabilities.Reverse(); s.Sources.Reverse(); s.NewProfile.EmptySlots.Reverse();
            s.Inventory.Items.Reverse(); var again = Prepare(s); CollectionAssert.AreEqual(actual.PayloadBytes, again.PayloadBytes);
            var decoded = Take(PublishedContentCodec.DecodeSource(actual.PayloadBytes.ToArray(), Caps(), Math()));
            CollectionAssert.AreEqual(actual.PayloadBytes, Prepare(decoded).PayloadBytes);
        }
        [Test] public void MutableInputsCannotChangePreparedPayloadOrDefinitions()
        {
            var s = Fixture(); var p = Prepare(s); var before = p.PayloadBytes.ToArray(); s.Levels[0].Level.Faces[0].Pairs[0].Enemy.Stats.MaxHp = ExactRational.Create(900, 1, Math());
            s.Parameters.Clear(); s.NewProfile.OpenLevels.Clear(); CollectionAssert.AreEqual(before, p.PayloadBytes);
            Assert.AreEqual(new BigInteger(15), p.Definitions.Levels[0].Faces[0].Pairs[0].Enemy.Stats.MaxHp.Numerator);
            Assert.Throws<NotSupportedException>(() => ((IList<byte>)p.PayloadBytes).Clear());
        }
        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)] [TestCase(5)] [TestCase(6)] [TestCase(7)] [TestCase(8)] [TestCase(9)] [TestCase(10)] [TestCase(11)]
        public void RejectsMissingUnsupportedOrMismatchedDefinition(int kind)
        {
            var s = Fixture(); if (kind == 0) s.SchemaVersion = 2; if (kind == 1) s.Parameters.RemoveAt(0);
            if (kind == 2) s.RequiredCapabilities.RemoveAt(0); if (kind == 3) s.RequiredCapabilities[0] = "unknown:feature";
            if (kind == 4) s.NewProfile.ClassId = "wrong"; if (kind == 5) s.Levels[0].Reward.LevelId = "wrong";
            if (kind == 6) s.Progression.Levels[0].LevelVersion = "2"; if (kind == 7) s.NewProfile.OpenLevels[0] = "missing";
            if (kind == 8) s.Levels[0].SourceRoutes[0].Cells[1] = new FlowPos(4, 4);
            if (kind == 9) s.Parameters[0].C = ExactRational.Create(1, 2, Math());
            if (kind == 10) s.Levels[0].Reward.Materials[0].ItemId = "unbound:item"; if (kind == 11) s.Levels[0].Level.LevelVersion = "01";
            var r = PublishedContentCompiler.Prepare(s, new DemoContentDraft(s.DraftId).BeginJob(), Caps(), Math());
            Assert.IsFalse(r.IsAccepted); Assert.IsNotEmpty(r.RejectionCode);
        }
        [TestCase("{\"SchemaVersion\":1,\"SchemaVersion\":1}")]
        [TestCase("{}")]
        [TestCase("{\"SchemaVersion\":1.0}")]
        [TestCase("{\"SchemaVersion\":1}garbage")]
        public void StrictDecoderRejectsInvalidShape(string text) { Assert.IsFalse(PublishedContentCodec.DecodeSource(Encoding.UTF8.GetBytes(text), Caps(), Math()).IsAccepted); }
        [TestCase(false)] [TestCase(true)] public void InvalidUnicodeIsRejectedBeforeEncodingOrAfterEscape(bool escaped)
        {
            var s = Fixture(); if (!escaped) { s.SourceNotes.Add("\ud800"); Assert.IsFalse(PublishedContentCodec.EncodeSource(s, Caps(), Math()).IsAccepted); }
            else { var json = Encoding.UTF8.GetString(Encode(s)).Replace("fixture:draft:v1", "\\ud800"); Assert.IsFalse(PublishedContentCodec.DecodeSource(Encoding.UTF8.GetBytes(json), Caps(), Math()).IsAccepted); }
        }
        [Test] public void ValidUnicodeSurrogatePairRemainsDistinctAndRoundTrips()
        { var s = Fixture(); s.SourceNotes.Add("中文🧩"); var p = Prepare(s); CollectionAssert.AreEqual(p.PayloadBytes, Prepare(Copy(s)).PayloadBytes); }
        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)] public void RejectsBudgetBeforeReturningLargeWork(int kind)
        {
            var caps = kind == 0 ? new ContentConsumerCapabilities(Caps().Capabilities, maxSourceBytes: 8) : kind == 1 ?
                new ContentConsumerCapabilities(Caps().Capabilities, maxCollectionEntries: 1) : kind == 2 ? new ContentConsumerCapabilities(Caps().Capabilities, maxStringCodeUnits: 1) : Caps();
            var s = Fixture(); var r = PublishedContentCompiler.Prepare(s, new DemoContentDraft(s.DraftId).BeginJob(), caps,
                kind == 3 ? new ExactMathBudget(maxPrimitiveSteps: 0) : Math()); Assert.IsFalse(r.IsAccepted); Assert.AreEqual("BudgetExceeded", r.RejectionCode);
        }
        [TestCase(false)] [TestCase(true)] public void StaleOrCancelledPrepareNeverReturnsCandidate(bool cancel)
        {
            var s = Fixture(); var draft = new DemoContentDraft(s.DraftId); using (var token = new CancellationTokenSource())
            { var job = draft.BeginJob(token.Token); if (cancel) token.Cancel(); else draft.Revise();
                var r = PublishedContentCompiler.Prepare(s, job, Caps(), Math()); Assert.AreEqual(cancel ? "Cancelled" : "StaleContext", r.RejectionCode); }
        }
        [TestCase("null")] [TestCase(" \r\n\t null \t\r\n ")]
        public void SourceNullRootIsRejectedWithoutThrowing(string text)
        {
            PublicationResult<PublishedSource> result = null;
            Assert.DoesNotThrow(() => result = PublishedContentCodec.DecodeSource(Encoding.UTF8.GetBytes(text), Caps(), Math()));
            Assert.IsFalse(result.IsAccepted); Assert.AreEqual("InvalidSchema", result.RejectionCode); Assert.AreEqual("Root", result.FieldPath);
        }
        [TestCase("null")] [TestCase(" \r\n\t null \t\r\n ")]
        public void ReviewNullRootIsRejectedWithoutThrowing(string text)
        {
            PublicationResult<ContentReviewEvidence> result = null;
            Assert.DoesNotThrow(() => result = PublishedContentCodec.DecodeReview(Encoding.UTF8.GetBytes(text), StoreBudget()));
            Assert.IsFalse(result.IsAccepted); Assert.AreEqual("InvalidSchema", result.RejectionCode); Assert.AreEqual("Root", result.FieldPath);
        }
        [Test] public void ApprovedNestedNullsRemainValidAndKeepCanonicalPayload()
        {
            var decoded = Take(PublishedContentCodec.DecodeSource(actual.PayloadBytes.ToArray(), Caps(), Math()));
            Assert.IsNull(decoded.Growth.Context); Assert.IsNull(decoded.Inventory.Context);
            Assert.IsNull(decoded.Progression.Context); Assert.IsNull(decoded.Levels[0].Reward.Context);
            Assert.IsNull(decoded.NewProfile.Recovery);
            Assert.IsNull(decoded.Progression.Levels[0].UnlockAfterLevelId);
            CollectionAssert.AreEqual(actual.PayloadBytes, Encode(decoded));
        }
        [Test] public void EncodingKeepsCanonicalMetadataOrderAndReadsFreshValuesOnEveryCall()
        {
            var original = Take(PublishedContentCodec.DecodeSource(actual.PayloadBytes.ToArray(), Caps(), Math()));
            var source = Take(PublishedContentCodec.DecodeSource(actual.PayloadBytes.ToArray(), Caps(), Math()));
            var math = Math(); var bytes = Take(PublishedContentCodec.EncodeSource(source, Caps(), math));
            Assert.AreEqual(1283, math.PrimitiveStepsUsed); Assert.AreEqual(11486, bytes.Length);
            Assert.AreEqual("b2247d3f951626edfdf25753520f3421dc20e8d8cab7731c9ab3ba6ece1a5129", PublishedContentCodec.Sha256(bytes));
            var json = Encoding.UTF8.GetString(bytes); var prefix = "\"Coordinates\":\"AssumedBottomLeft\",";
            Assert.IsTrue(json.StartsWith("{" + prefix, StringComparison.Ordinal));
            var reordered = "{" + json.Substring(1 + prefix.Length, json.Length - prefix.Length - 2) + "," + prefix.TrimEnd(',') + "}";
            CollectionAssert.AreEqual(bytes, Encode(Take(PublishedContentCodec.DecodeSource(Encoding.UTF8.GetBytes(reordered), Caps(), Math()))));
            source.DraftId = "中文🧩\"\\\n\u0000"; source.SourceNotes = new List<string> { null, "", "\b\f\n\r\t/\"\\" };
            var notes = source.SourceNotes.ToArray(); var changed = Encode(source);
            Assert.AreEqual("d6bb25169f4bf8eca37f70b713231aa5aa0ddd5a61ebc5b237abe5468bfe53c8", PublishedContentCodec.Sha256(changed));
            var decoded = Take(PublishedContentCodec.DecodeSource(changed, Caps(), Math()));
            Assert.AreEqual(source.DraftId, decoded.DraftId); CollectionAssert.AreEqual(notes, decoded.SourceNotes);
            CollectionAssert.AreEqual(notes, source.SourceNotes); CollectionAssert.AreEqual(changed, Encode(decoded));
            source.DraftId = "second"; source.SourceNotes[0] = "new value";
            Assert.AreEqual("037ccc2185ef34e4b6a998be95a8d847dc394dbe35d935bbe9a58f16eb78e9f9", PublishedContentCodec.Sha256(Encode(source)));
            CollectionAssert.AreEqual(bytes, Encode(original));
        }
        [TestCase("bytes", 0, "ProjectionBytes", 0)] [TestCase("bytes", 1, "ProjectionBytes", 0)]
        [TestCase("bytes", 11485, "EncodedBytes", 1283)] [TestCase("bytes", 11486, null, 1283)] [TestCase("bytes", 11487, null, 1283)]
        [TestCase("collection", 1, "Object", 0)] [TestCase("collection", 2, "Collection", 326)]
        [TestCase("collection", 30, "Collection", 348)] [TestCase("collection", 31, null, 1283)]
        [TestCase("string", 8192, null, 1283)] [TestCase("string", 8193, "String", 0)]
        [TestCase("steps", 0, "Math", 0)] [TestCase("steps", 1, "Math", 1)] [TestCase("steps", 2, "Math", 2)]
        [TestCase("steps", 1282, "Math", 1282)] [TestCase("steps", 1283, null, 1283)] [TestCase("steps", 1284, null, 1283)]
        public void EncodingPreservesMeasuredBudgetBoundariesAndCharges(string kind, int maximum, string field, int used)
        {
            var source = Take(PublishedContentCodec.DecodeSource(actual.PayloadBytes.ToArray(), Caps(), Math()));
            if (kind == "string") source.DraftId = new string('x', maximum);
            var before = Take(PublishedContentCodec.EncodeSource(source,
                new ContentConsumerCapabilities(Caps().Capabilities, maxStringCodeUnits: 8193), Math()));
            var caps = new ContentConsumerCapabilities(Caps().Capabilities,
                maxSourceBytes: kind == "bytes" ? maximum : 262144, maxCollectionEntries: kind == "collection" ? maximum : 512);
            var math = new ExactMathBudget(maxPrimitiveSteps: kind == "steps" ? maximum : 16000000);
            var result = PublishedContentCodec.EncodeSource(source, caps, math);
            Assert.AreEqual(field == null, result.IsAccepted); Assert.AreEqual(field, result.FieldPath);
            Assert.AreEqual(field == null ? null : "BudgetExceeded", result.RejectionCode); Assert.AreEqual(used, math.PrimitiveStepsUsed);
            if (result.IsAccepted) CollectionAssert.AreEqual(before, result.Value);
            CollectionAssert.AreEqual(before, Take(PublishedContentCodec.EncodeSource(source,
                new ContentConsumerCapabilities(Caps().Capabilities, maxStringCodeUnits: 8193), Math())));
        }
        [TestCase(0, "InvalidUnicode", "String")] [TestCase(1, "InvalidUnicode", "String")]
        [TestCase(2, "BudgetExceeded", "ProjectionBytes")] [TestCase(3, "BudgetExceeded", "String")]
        public void EncodingPreservesUnicodeFailurePriority(int kind, string code, string field)
        {
            var source = Source(); source.DraftId = "\ud800"; var notes = source.SourceNotes.ToArray();
            var caps = new ContentConsumerCapabilities(Caps().Capabilities, maxSourceBytes: kind == 2 ? 1 : 262144,
                maxStringCodeUnits: kind == 3 ? 0 : 8192);
            var math = new ExactMathBudget(maxPrimitiveSteps: kind == 1 ? 0 : 16000000);
            PublicationResult<byte[]> result = null;
            Assert.DoesNotThrow(() => result = PublishedContentCodec.EncodeSource(source, caps, math));
            Assert.AreEqual(code, result.RejectionCode); Assert.AreEqual(field, result.FieldPath); Assert.AreEqual(0, math.PrimitiveStepsUsed);
            Assert.AreEqual("\ud800", source.DraftId); CollectionAssert.AreEqual(notes, source.SourceNotes);
        }
        [TestCase(-256, false, 1)] [TestCase(-255, true, 6)] [TestCase(0, true, 2)] [TestCase(255, true, 5)] [TestCase(256, false, 1)]
        public void EncodingPreservesIntegerBitBoundaries(int revision, bool accepted, int used)
        {
            var source = new PublishedSource { SchemaVersion = 1, Revision = revision }; var math = new ExactMathBudget(8, 16);
            var result = PublishedContentCodec.EncodeSource(source, Caps(), math);
            Assert.AreEqual(accepted, result.IsAccepted); Assert.AreEqual(used, math.PrimitiveStepsUsed);
            Assert.AreEqual(accepted ? null : "BudgetExceeded", result.RejectionCode); Assert.AreEqual(accepted ? null : "Math", result.FieldPath);
            Assert.AreEqual(new BigInteger(revision), source.Revision);
        }
        [TestCase(0, "ProjectionBytes")] [TestCase(1, "EncodedBytes")] [TestCase(2, "EncodedBytes")]
        [TestCase(3, "EncodedBytes")] [TestCase(4, null)] [TestCase(5, null)]
        public void EncodingNullPreservesProjectionAndOutputByteBoundaries(int maximum, string field)
        {
            var math = Math(); var result = PublishedContentCodec.EncodeSource(null,
                new ContentConsumerCapabilities(Caps().Capabilities, maxSourceBytes: maximum), math);
            Assert.AreEqual(field == null, result.IsAccepted); Assert.AreEqual(field, result.FieldPath); Assert.AreEqual(0, math.PrimitiveStepsUsed);
            Assert.AreEqual(field == null ? null : "BudgetExceeded", result.RejectionCode);
            if (result.IsAccepted) CollectionAssert.AreEqual(Encoding.UTF8.GetBytes("null"), result.Value);
        }
        [TestCase(4096, true)] [TestCase(4097, false)] public void EncodingPreservesDecimalTokenLimit(int digits, bool accepted)
        {
            var source = new PublishedSource { SchemaVersion = 1, Revision = BigInteger.Pow(10, digits - 1) }; var math = Math();
            var result = PublishedContentCodec.EncodeSource(source, Caps(), math);
            Assert.AreEqual(accepted, result.IsAccepted); Assert.AreEqual(accepted ? null : "Integer", result.FieldPath);
            Assert.AreEqual(accepted ? null : "BudgetExceeded", result.RejectionCode); Assert.AreEqual(5, math.PrimitiveStepsUsed);
            Assert.AreEqual(BigInteger.Pow(10, digits - 1), source.Revision);
        }
        [TestCase(1, false, 2)] [TestCase(2, true, 18)] public void EncodingPreservesRationalObjectLimit(int maximum, bool accepted, int used)
        {
            var source = new PublishedSource { SchemaVersion = 1, NewProfile = new NewProfileDefinitionInput { Hp = ExactRational.Create(1, 2, Math()) } };
            var math = Math(); var result = PublishedContentCodec.EncodeSource(source,
                new ContentConsumerCapabilities(Caps().Capabilities, maxCollectionEntries: maximum), math);
            Assert.AreEqual(accepted, result.IsAccepted); Assert.AreEqual(accepted ? null : "Object", result.FieldPath);
            Assert.AreEqual(accepted ? null : "BudgetExceeded", result.RejectionCode); Assert.AreEqual(used, math.PrimitiveStepsUsed);
            Assert.AreEqual(BigInteger.One, source.NewProfile.Hp.Numerator); Assert.AreEqual(new BigInteger(2), source.NewProfile.Hp.Denominator);
        }
        [Test] public void Above31UsesFinalExactCoefficientAndChangedRevisionHasNewFingerprint()
        {
            var s = Fixture(); s.NewProfile.Level = 32; s.NewProfile.Hp = ExactRational.Create(348, 1, Math());
            var p = Prepare(s); Assert.AreEqual(0, p.Replays[0].Candidate.Entry.Members[0].Crit.C.Compare(s.Parameters[30].C, Math()));
            var draft = new DemoContentDraft(s.DraftId); draft.Revise(); s.Revision = 2; var newer = Prepare(s, draft.BeginJob());
            Assert.AreNotEqual(p.Binding.ContentFingerprint, newer.Binding.ContentFingerprint);
        }
    }
}
