using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FightMatch.Content;
using FightMatch.Platform;
using NUnit.Framework;
using static FightMatch.Core.Tests.PublishedContentTestData;

namespace FightMatch.Core.Tests
{
    public sealed class PublishedContentCatalogReuseTests
    {
        private const string Scope = "fixture:reuse-scope", Release = "fixture:reuse-release";
        private Package[] packages;
        private sealed class Package
        {
            internal PreparedPublication Prepared;
            internal ContentReviewEvidence Review;
            internal string Operation;
            internal string[] Keys;
            internal Dictionary<string, byte[]> Records;
        }
        [OneTimeSetUp] public void BuildTwoRealPublications()
        {
            packages = Enumerable.Range(1, 2).Select(version => {
                var source = Fixture(version); var job = new DemoContentDraft(source.DraftId).BeginJob();
                var prepared = Prepare(source, job); var review = Review(prepared); var operation = "fixture:reuse:" + version;
                var memory = new MemoryStorage();
                Take(new PublishedContentCatalog(memory, Caps()).Publish(prepared, prepared.Validation, review, job, operation, StoreBudget()));
                return new Package { Prepared = prepared, Review = review, Operation = operation, Records = memory.Blobs,
                    Keys = new[] { PublishedContentCatalog.BindingKey(prepared.Binding), PublishedContentCatalog.Key("operation", operation),
                        PublishedContentCatalog.Key("receipt", operation), PublishedContentCatalog.Key("payload", prepared.PayloadSha256),
                        PublishedContentCatalog.Key("source", prepared.SourceSha256), PublishedContentCatalog.Key("validation", prepared.Validation.Sha256),
                        PublishedContentCatalog.Key("review", PublishedContentCodec.Sha256(Take(PublishedContentCodec.EncodeReview(review, StoreBudget())))) } };
            }).ToArray();
        }
        private ObservedStorage Store(int index = 0, bool both = false, bool borrowed = false)
        {
            var store = new ObservedStorage { Borrowed = borrowed };
            foreach (var package in both ? packages : new[] { packages[index] })
                foreach (var pair in package.Records) store.Blobs.Add(pair.Key, (byte[])pair.Value.Clone());
            return store;
        }
        private PublishedContentCatalog Warm(ObservedStorage store, int index = 0)
        {
            var catalog = new PublishedContentCatalog(store, Caps());
            SamePublication(packages[index], Take(catalog.ResolveExact(packages[index].Prepared.Binding, Caps())));
            store.ClearReads(); return catalog;
        }
        private static void SamePublication(Package expected, ResolvedPublication actual)
        {
            Assert.IsTrue(actual.Binding.Same(expected.Prepared.Binding));
            CollectionAssert.AreEqual(expected.Records[expected.Keys[0]], actual.ReceiptBytes);
            CollectionAssert.AreEqual(expected.Prepared.NewProfile.CanonicalBytes, actual.NewProfile.CanonicalBytes);
            CollectionAssert.AreEqual(expected.Prepared.Replays[0].SeedBytes, actual.GetDefaultReferences()[0].SeedBytes);
        }
        private static void SameResult<T>(PublicationResult<T> expected, PublicationResult<T> actual)
        {
            Assert.AreEqual(expected.IsAccepted, actual.IsAccepted);
            Assert.AreEqual(expected.RejectionCode, actual.RejectionCode); Assert.AreEqual(expected.FieldPath, actual.FieldPath);
        }
        [TestCase(0, "missing")] [TestCase(0, "corrupt")] [TestCase(0, "io")]
        [TestCase(1, "missing")] [TestCase(1, "corrupt")] [TestCase(1, "io")]
        [TestCase(2, "missing")] [TestCase(2, "corrupt")] [TestCase(2, "io")]
        [TestCase(3, "missing")] [TestCase(3, "corrupt")] [TestCase(3, "io")]
        [TestCase(4, "missing")] [TestCase(4, "corrupt")] [TestCase(4, "io")]
        [TestCase(5, "missing")] [TestCase(5, "corrupt")] [TestCase(5, "io")]
        [TestCase(6, "missing")] [TestCase(6, "corrupt")] [TestCase(6, "io")]
        public void EveryRecordFailureAfterWarmReadKeepsColdRejectionAndReadOrder(int record, string fault)
        {
            var store = Store(); var warm = Warm(store); var p = packages[0]; store.Faults[p.Keys[record]] = fault;
            var actual = warm.ResolveExact(p.Prepared.Binding, Caps()); var actualReads = store.Trace();
            store.ClearReads(); var cold = new PublishedContentCatalog(store, Caps()).ResolveExact(p.Prepared.Binding, Caps());
            Assert.IsFalse(actual.IsAccepted); SameResult(cold, actual);
            CollectionAssert.AreEqual(p.Keys.Take(record + 1), actualReads); CollectionAssert.AreEqual(actualReads, store.Trace());
            Assert.AreEqual(0, store.Writes);
        }
        [Test] public void OperationMismatchShortCircuitsAReceiptReadThatWouldThrow()
        {
            var store = Store(); var warm = Warm(store); var p = packages[0];
            store.Faults[p.Keys[1]] = "corrupt"; store.Faults[p.Keys[2]] = "io";
            var actual = warm.ResolveExact(p.Prepared.Binding, Caps());
            Assert.AreEqual("RecoveryBlocked", actual.RejectionCode); Assert.AreEqual("Receipt.Identity", actual.FieldPath);
            CollectionAssert.AreEqual(p.Keys.Take(2), store.Trace()); store.ClearReads();
            SameResult(new PublishedContentCatalog(store, Caps()).ResolveExact(p.Prepared.Binding, Caps()), actual);
            CollectionAssert.AreEqual(p.Keys.Take(2), store.Trace()); Assert.AreEqual(0, store.Writes);
        }
        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)] [TestCase(5)]
        public void ChangedConsumerValuesKeepTheCompleteColdBehavior(int change)
        {
            var store = Store(); var warm = Warm(store); var caps = Caps();
            var consumer = change == 5 ? null : new ContentConsumerCapabilities(
                change == 0 ? caps.Capabilities.Take(caps.Capabilities.Count - 1) : change == 4 ? caps.Capabilities.Reverse() : caps.Capabilities,
                change == 1 ? 1 : caps.MaxSourceBytes, change == 2 ? 1 : caps.MaxCollectionEntries, change == 3 ? 1 : caps.MaxStringCodeUnits);
            var actualBudget = Budget(); var actual = Invoke(warm, true, packages[0], consumer, actualBudget); var reads = store.Trace();
            store.ClearReads(); var coldBudget = Budget(); var cold = Invoke(warm, false, packages[0], consumer, coldBudget);
            SameAttempt(cold, actual); CollectionAssert.AreEqual(reads, store.Trace());
            Assert.AreEqual(coldBudget.Math.PrimitiveStepsUsed, actualBudget.Math.PrimitiveStepsUsed);
            if (actual.Value != null) SamePublication(packages[0], actual.Value);
            Assert.AreEqual(0, store.Writes);
        }
        [TestCase(128)] [TestCase(32767)] [TestCase(32769)] [TestCase(65536)]
        public void DifferentIntegerBitLimitsDoNotBorrowPriorAdmission(int bits)
        { CompareBudgetConditions(bits, 16 * 1024 * 1024); }
        [TestCase(1)] [TestCase(65536)] [TestCase(16777217)]
        public void DifferentRecordLimitsKeepColdBudgetAndReadBehavior(int bytes)
        { CompareBudgetConditions(32768, bytes); }
        private void CompareBudgetConditions(int bits, int bytes)
        {
            var store = Store(); var warm = Warm(store);
            var actualBudget = Budget(bits: bits, bytes: bytes); var actual = Invoke(warm, true, packages[0], Caps(), actualBudget); var reads = store.Trace();
            store.ClearReads(); var coldBudget = Budget(bits: bits, bytes: bytes); var cold = Invoke(warm, false, packages[0], Caps(), coldBudget);
            SameAttempt(cold, actual); CollectionAssert.AreEqual(reads, store.Trace());
            Assert.AreEqual(coldBudget.Math.PrimitiveStepsUsed, actualBudget.Math.PrimitiveStepsUsed);
            Assert.AreEqual(0, store.Writes);
        }
        [TestCase(-1, 0)] [TestCase(0, 0)] [TestCase(1, 0)]
        [TestCase(-1, 37)] [TestCase(0, 37)] [TestCase(1, 37)]
        public void RemainingBudgetBoundaryUsesTheSameReadBatchAndCallerBudget(int delta, int spent)
        {
            var store = Store(); var catalog = new PublishedContentCatalog(store, Caps()); var p = packages[0]; var measured = Budget(); long prefix = -1;
            store.OnRead = key => { if (key == p.Keys[6]) prefix = measured.Math.PrimitiveStepsUsed; };
            var full = Invoke(catalog, false, p, Caps(), measured); store.OnRead = null;
            Assert.IsNull(full.Error); SamePublication(p, full.Value); Assert.Greater(prefix, 0); Assert.Greater(measured.Math.PrimitiveStepsUsed, prefix);
            Take(catalog.ResolveExact(p.Prepared.Binding, Caps())); store.ClearReads();
            var limit = checked((int)measured.Math.PrimitiveStepsUsed + delta + spent);
            var actualBudget = Budget(limit, spent: spent); var actual = Invoke(catalog, true, p, Caps(), actualBudget); var reads = store.Trace();
            store.ClearReads(); var coldBudget = Budget(limit, spent: spent); var cold = Invoke(catalog, false, p, Caps(), coldBudget);
            SameAttempt(cold, actual); CollectionAssert.AreEqual(p.Keys, reads); CollectionAssert.AreEqual(reads, store.Trace());
            if (delta < 0) { Assert.IsNotNull(actual.Error); Assert.AreEqual(coldBudget.Math.PrimitiveStepsUsed, actualBudget.Math.PrimitiveStepsUsed); }
            else { Assert.IsNull(actual.Error); SamePublication(p, actual.Value); Assert.LessOrEqual(actualBudget.Math.PrimitiveStepsUsed, coldBudget.Math.PrimitiveStepsUsed); }
            Console.WriteLine("CATALOG-REUSE-BUDGET\t" + delta + "\t" + spent + "\t" + prefix + "\t" +
                measured.Math.PrimitiveStepsUsed + "\t" + coldBudget.Math.PrimitiveStepsUsed + "\t" + actualBudget.Math.PrimitiveStepsUsed);
            Assert.AreEqual(0, store.Writes);
        }
        [TestCase("receipt")] [TestCase("scope")] [TestCase("release")]
        public void WarmCurrentBindingStillValidatesTheReleaseRecord(string change)
        {
            var store = Store(); var warm = Warm(store); var p = packages[0]; var key = PublishedContentCatalog.ReleaseSetKey(Scope, Release);
            store.Blobs[key] = ReleaseBytes(p); Assert.IsTrue(Take(warm.GetCurrentBinding(Scope, Release, Caps())).Same(p.Prepared.Binding));
            store.Blobs[key] = ReleaseBytes(p, change);
            var actual = warm.GetCurrentBinding(Scope, Release, Caps());
            SameResult(new PublishedContentCatalog(store, Caps()).GetCurrentBinding(Scope, Release, Caps()), actual);
            Assert.IsFalse(actual.IsAccepted); Assert.AreEqual(0, store.Writes);
        }
        [TestCase("level")] [TestCase("version")] [TestCase("binding")] [TestCase("null")]
        public void WrongDefinitionOrBindingNeverFallsBackToTheWarmPublication(string change)
        {
            var store = Store(); var warm = Warm(store); var cold = new PublishedContentCatalog(store, Caps()); var p = packages[0];
            if (change == "level" || change == "version")
            {
                var other = DefinitionBinding.Prepare(p.Prepared.Binding, change == "level" ? "fixture:missing" : "fixture:alpha",
                    change == "version" ? 999 : 1, new SaveCodecBudget(Math()));
                Assert.IsTrue(other.IsAccepted); var actual = warm.ResolveExact(other.Value, Caps());
                SameResult(cold.ResolveExact(other.Value, Caps()), actual); Assert.IsFalse(actual.IsAccepted);
            }
            else
            {
                var binding = change == "null" ? null : packages[1].Prepared.Binding;
                var actual = warm.ResolveExact(binding, Caps()); SameResult(cold.ResolveExact(binding, Caps()), actual); Assert.IsFalse(actual.IsAccepted);
            }
            Assert.AreEqual(0, store.Writes);
        }
        [Test] public void BorrowedArraysChangedLaterCannotRewriteAnAdmittedEntry()
        {
            var store = Store(borrowed: true); var p = packages[0]; var warm = Warm(store);
            var admitted = Take(warm.ResolveExact(p.Prepared.Binding, Caps())); var receipt = admitted.ReceiptBytes.ToArray();
            var review = store.Blobs[p.Keys[6]]; var oldHash = PublishedContentCodec.Sha256(review);
            ReplaceInPlace(review, "ACCEPT", "REJECT"); var newHash = PublishedContentCodec.Sha256(review);
            foreach (var key in p.Keys.Take(3)) ReplaceInPlace(store.Blobs[key], oldHash, newHash);
            store.Blobs.Add(PublishedContentCatalog.Key("review", newHash), review); store.Blobs.Remove(p.Keys[6]);
            var actual = warm.ResolveExact(p.Prepared.Binding, Caps());
            SameResult(new PublishedContentCatalog(store, Caps()).ResolveExact(p.Prepared.Binding, Caps()), actual);
            Assert.AreEqual("ReviewRequired", actual.RejectionCode); Assert.AreEqual("Review.Verdict", actual.FieldPath);
            CollectionAssert.AreEqual(receipt, admitted.ReceiptBytes); SamePublication(p, admitted); Assert.AreEqual(0, store.Writes);
        }
        [Test] public void ConcurrentPackageReplacementKeepsEachCallBoundToItsOwnContent()
        {
            var store = Store(both: true); var catalog = Warm(store);
            using (var barrier = new Barrier(2))
            {
                store.OnRead = key => { if (packages.Any(p => p.Keys[6] == key) && !barrier.SignalAndWait(TimeSpan.FromSeconds(15)))
                    throw new IOException("Concurrent fixture rendezvous timed out"); };
                for (var round = 0; round < 3; round++)
                {
                    var first = Task.Run(() => catalog.ResolveExact(packages[0].Prepared.Binding, Caps()));
                    var second = Task.Run(() => catalog.ResolveExact(packages[1].Prepared.Binding, Caps()));
                    Task.WaitAll(first, second); SamePublication(packages[0], Take(first.Result)); SamePublication(packages[1], Take(second.Result));
                }
                store.OnRead = null;
            }
            Assert.AreEqual(0, store.Writes);
        }
        [Test] public void SeparateCatalogsDoNotShareTheirSuccessfulEntry()
        {
            var store = Store(); Warm(store); var other = new PublishedContentCatalog(store, Caps());
            var readBudget = Budget(); var actual = Invoke(other, true, packages[0], Caps(), readBudget);
            var fullBudget = Budget(); var full = Invoke(other, false, packages[0], Caps(), fullBudget);
            SameAttempt(full, actual); SamePublication(packages[0], actual.Value);
            Assert.AreEqual(fullBudget.Math.PrimitiveStepsUsed, readBudget.Math.PrimitiveStepsUsed);
        }
        [TestCase(0)] [TestCase(1)] [TestCase(16000000)]
        public void PublishStillUsesItsFullCallerBudgetAfterAReadOnlyWarmup(int steps)
        {
            var store = Store(); var warm = Warm(store); var cold = new PublishedContentCatalog(store, Caps()); var p = packages[0];
            var warmBudget = Budget(steps); var actual = warm.Publish(p.Prepared, p.Prepared.Validation, p.Review, null, p.Operation, warmBudget); var reads = store.Trace();
            store.ClearReads(); var coldBudget = Budget(steps); var expected = cold.Publish(p.Prepared, p.Prepared.Validation, p.Review, null, p.Operation, coldBudget);
            SameResult(expected, actual); CollectionAssert.AreEqual(reads, store.Trace());
            Assert.AreEqual(coldBudget.Math.PrimitiveStepsUsed, warmBudget.Math.PrimitiveStepsUsed);
            if (actual.IsAccepted) SamePublication(p, actual.Value.Publication); Assert.AreEqual(0, store.Writes);
        }
        [Test] public void PublishDoesNotPopulateAReadOnlyEntry()
        {
            var store = Store(); var catalog = new PublishedContentCatalog(store, Caps()); var p = packages[0];
            Take(catalog.Publish(p.Prepared, p.Prepared.Validation, p.Review, null, p.Operation, StoreBudget()));
            var readBudget = Budget(); var actual = Invoke(catalog, true, p, Caps(), readBudget);
            var fullBudget = Budget(); var full = Invoke(catalog, false, p, Caps(), fullBudget);
            SameAttempt(full, actual); SamePublication(p, actual.Value); Assert.AreEqual(fullBudget.Math.PrimitiveStepsUsed, readBudget.Math.PrimitiveStepsUsed);
            Assert.AreEqual(0, store.Writes);
        }
        private static byte[] ReleaseBytes(Package p, string change = null)
        {
            return Take(PublishedContentCodec.EncodeReleaseSet(new ContentReleaseSet { SchemaVersion = 1,
                Scope = change == "scope" ? "fixture:other-scope" : Scope, ReleaseSetId = change == "release" ? "fixture:other-release" : Release,
                Binding = ContentBindingRecord.From(p.Prepared.Binding),
                PublicationReceiptSha256 = change == "receipt" ? new string('f', 64) : PublishedContentCodec.Sha256(p.Records[p.Keys[0]]) }, StoreBudget()));
        }
        private static void ReplaceInPlace(byte[] bytes, string old, string replacement)
        {
            var text = Encoding.UTF8.GetString(bytes); Assert.IsTrue(text.Contains(old)); var changed = Encoding.UTF8.GetBytes(text.Replace(old, replacement));
            Assert.AreEqual(bytes.Length, changed.Length); Buffer.BlockCopy(changed, 0, bytes, 0, bytes.Length);
        }
        private static ContentStoreBudget Budget(int steps = 16000000, int bits = 32768, int bytes = 16 * 1024 * 1024, int spent = 0)
        {
            var math = new ExactMathBudget(bits, steps);
            for (var i = 0; i < spent; i++) math.Compare(BigInteger.Zero, BigInteger.Zero);
            return new ContentStoreBudget(math, bytes);
        }
        private sealed class Attempt { internal ResolvedPublication Value; internal string Error; }
        private static Attempt Invoke(PublishedContentCatalog catalog, bool readOnly, Package p, ContentConsumerCapabilities caps, ContentStoreBudget budget)
        {
            var method = typeof(PublishedContentCatalog).GetMethod(readOnly ? "ResolveReadOnly" : "Resolve", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(method); var args = new object[] { p.Prepared.Binding, caps, budget };
            if (method.GetParameters().Length == 4) args = args.Concat(new object[] { false }).ToArray();
            try { return new Attempt { Value = (ResolvedPublication)method.Invoke(catalog, args) }; }
            catch (TargetInvocationException e) { return new Attempt { Error = e.InnerException.GetType().FullName + ": " + e.InnerException.Message }; }
        }
        private static void SameAttempt(Attempt expected, Attempt actual)
        {
            Assert.AreEqual(expected.Error, actual.Error); Assert.AreEqual(expected.Value == null, actual.Value == null);
            if (actual.Value != null) { Assert.IsTrue(actual.Value.Binding.Same(expected.Value.Binding)); CollectionAssert.AreEqual(expected.Value.ReceiptBytes, actual.Value.ReceiptBytes); }
        }
        private sealed class ObservedStorage : IContentPublicationStorage
        {
            internal readonly Dictionary<string, byte[]> Blobs = new Dictionary<string, byte[]>(StringComparer.Ordinal);
            internal readonly Dictionary<string, string> Faults = new Dictionary<string, string>(StringComparer.Ordinal);
            private readonly List<string> reads = new List<string>(); private readonly object sync = new object();
            internal bool Borrowed; internal int Writes; internal Action<string> OnRead;
            internal string[] Trace() { lock (sync) return reads.ToArray(); }
            internal void ClearReads() { lock (sync) reads.Clear(); }
            public byte[] Read(string key, int maxBytes)
            {
                ContentPublicationStorage.CheckKey(key); lock (sync) reads.Add(key);
                Faults.TryGetValue(key, out var fault); if (fault == "io") throw new IOException("Injected read failure");
                if (fault == "missing" || !Blobs.TryGetValue(key, out var bytes)) return null;
                if (bytes.Length > maxBytes) throw new ContentStorageException("RecoveryBlocked", "read budget");
                OnRead?.Invoke(key); var result = Borrowed && fault == null ? bytes : (byte[])bytes.Clone();
                if (fault == "corrupt") result[0] ^= 1; return result;
            }
            public IDisposable AcquireWriter() { Monitor.Enter(sync); return new Unlock(sync); }
            public void WriteImmutable(string key, byte[] bytes, int maxBytes)
            {
                Writes++; if (bytes.Length > maxBytes) throw new ContentStorageException("RecoveryBlocked", "write budget");
                if (Blobs.TryGetValue(key, out var old))
                { if (!ContentPublicationStorage.Equal(old, bytes)) throw new ContentStorageException("RecoveryBlocked", "immutable conflict"); return; }
                Blobs.Add(key, (byte[])bytes.Clone());
            }
            private sealed class Unlock : IDisposable
            { private readonly object sync; internal Unlock(object sync) { this.sync = sync; } public void Dispose() { Monitor.Exit(sync); } }
        }
    }
}
