using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FightMatch.AssetAccess;
using FightMatch.Presentation;
using NUnit.Framework;

namespace FightMatch.Host.Tests
{
    public sealed class FightMatchResourceBridgeTests
    {
        [Test]
        public void FormalTextAndSixBusinessFilesProduceDataReadyAndReleaseEveryLease()
        {
            using (var runtime = new FixtureRuntime())
            {
                var result = Completed(FightMatchResourceBridge.OpenAsync(runtime, 1, CancellationToken.None));
                Assert.IsTrue(result.IsDataReady, result.DiagnosticCode);
                Assert.AreEqual("fm.bridge.test", result.ReleaseSetId);
                Assert.AreEqual(8, runtime.Leases.Count);
                Assert.IsTrue(runtime.Leases.All(x => x.IsReleased));
                Assert.IsNotNull(result.Catalog); Assert.IsNotNull(result.TextSource);
            }
        }
        [TestCase("fm.text.full.manifest", "RES_TEXT")]
        [TestCase("fm.text.full.artifact", "RES_TEXT")]
        [TestCase("fm.content.package", "RES_CONTENT")]
        public void WrongTextOrBusinessBytesNeverPublishPartialData(string id, string code)
        {
            using (var runtime = new FixtureRuntime())
            {
                runtime.Data[id][0] ^= 1;
                var result = Completed(FightMatchResourceBridge.OpenAsync(runtime, 1, CancellationToken.None));
                Assert.IsFalse(result.IsDataReady); Assert.AreEqual(code, result.DiagnosticCode);
                Assert.IsNull(result.Catalog); Assert.IsNull(result.TextSource);
                Assert.IsTrue(runtime.Leases.All(x => x.IsReleased));
            }
        }
        [Test]
        public void CancellationAfterAcquireReleasesLateLeaseAndPublishesNothing()
        {
            using (var cancellation = new CancellationTokenSource())
            using (var runtime = new FixtureRuntime { AfterAcquire = () => cancellation.Cancel() })
            {
                var result = Completed(FightMatchResourceBridge.OpenAsync(runtime, 1, cancellation.Token));
                Assert.AreEqual("RES_STOP", result.DiagnosticCode); Assert.IsFalse(result.IsDataReady);
                Assert.IsTrue(runtime.Disposed); Assert.AreEqual(1, runtime.Leases.Count);
                Assert.IsTrue(runtime.Leases[0].IsReleased);
            }
        }
        private static FightMatchResourceBridgeResult Completed(Task<FightMatchResourceBridgeResult> task)
        {
            // This fixture returns Task.FromResult at every await boundary; never block the Editor on a pending task.
            Assert.IsTrue(task.IsCompleted, "The synchronous bridge fixture left work pending.");
            return task.GetAwaiter().GetResult();
        }
        private sealed class FixtureRuntime : IFightMatchResourceRuntime, IFightMatchAssetProvider
        {
            internal readonly Dictionary<string, byte[]> Data = new Dictionary<string, byte[]>();
            internal readonly List<RawLease> Leases = new List<RawLease>();
            internal Action AfterAcquire;
            internal bool Disposed;
            public IFightMatchAssetProvider Assets => this;
            internal FixtureRuntime()
            {
                var ids = new[] { "source", "package", "validation", "review", "publication", "release-set" };
                for (var i = 0; i < 6; i++) Data.Add("fm.content." + ids[i], File.ReadAllBytes(
                    Path.Combine(UnityEngine.Application.streamingAssetsPath, "FightMatch", FightMatchStreamingAssetsLoader.FileNames[i])));
                var root = Path.Combine(Path.GetDirectoryName(UnityEngine.Application.dataPath), "Generated/FightMatch/Localization");
                Data.Add("fm.text.full.artifact", File.ReadAllBytes(Path.Combine(root, "fm-text-v1.json")));
                Data.Add("fm.text.full.manifest", File.ReadAllBytes(Path.Combine(root, "fm-text-v1.manifest.json")));
            }
            public Task<ResourceInspection> InspectAsync(long epoch, CancellationToken cancellationToken) =>
                Task.FromResult(ResourceInspection.Accepted(new string('1', 32), new string('2', 32), "fm.bridge.test", epoch,
                    0, 0, false, ResourceNetworkKind.Offline));
            public Task<ResourceTransportResult> PrepareAsync(ResourceDownloadPermit permit, IProgress<ResourceProgress> progress,
                CancellationToken cancellationToken) => throw new NotSupportedException("Bridge fixture is DataReady-only testing.");
            public Task<FightMatchAssetAcquireResult<T>> AcquireAsync<T>(FightMatchAssetId id, string set, AssetAcquireBudget budget, long epoch) where T : class
            {
                Assert.IsFalse(Disposed); Assert.AreEqual(typeof(FightMatchRawBytes), typeof(T));
                var lease = new RawLease(id, set, Data[id.Value]); Leases.Add(lease);
                var result = FightMatchAssetAcquireResult<T>.Accepted((IFightMatchAssetLease<T>)(object)lease, epoch, set);
                AfterAcquire?.Invoke();
                return Task.FromResult(result);
            }
            public void Dispose() { Disposed = true; foreach (var lease in Leases) lease.Dispose(); }
        }
        private sealed class RawLease : IFightMatchAssetLease<FightMatchRawBytes>
        {
            private readonly object buffer;
            public FightMatchAssetId AssetId { get; }
            public string ReleaseSetId { get; }
            public string LeaseId { get; } = Guid.NewGuid().ToString("N");
            public bool IsReleased { get; private set; }
            private readonly FightMatchRawBytes bytes;
            public FightMatchRawBytes Asset => IsReleased ? null : bytes;
            internal RawLease(FightMatchAssetId id, string set, byte[] data)
            {
                AssetId = id; ReleaseSetId = set;
                // Reflection stays in tests; production keeps raw constructors and buffers internal to AssetAccess.
                var type = typeof(FightMatchRawBytes).Assembly.GetType("FightMatch.AssetAccess.RawBuffer", true);
                buffer = Activator.CreateInstance(type, BindingFlags.Instance | BindingFlags.NonPublic, null,
                    new object[] { (byte[])data.Clone() }, null);
                bytes = (FightMatchRawBytes)Activator.CreateInstance(typeof(FightMatchRawBytes), BindingFlags.Instance | BindingFlags.NonPublic,
                    null, new object[] { buffer, (Func<bool>)(() => IsReleased) }, null);
            }
            public void Dispose()
            {
                if (IsReleased) return;
                IsReleased = true;
                buffer.GetType().GetMethod("Release", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(buffer, null);
            }
        }
    }
}
