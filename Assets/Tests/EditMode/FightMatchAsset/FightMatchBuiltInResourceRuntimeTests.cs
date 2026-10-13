using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FightMatch.YooAssetAdapter;
using FightMatch.YooAssetAdapter.Editor;
using NUnit.Framework;

namespace FightMatch.AssetAccess.Tests
{
    public sealed class FightMatchBuiltInResourceRuntimeTests
    {
        [Test]
        public void FactoryRefusesPinSetPlatformProtocolBudgetAndRemote()
        {
            var bytes = FightMatchResourceBuildTests.Boot();
            var pin = FightMatchResourceBuild.Hash(bytes);
            ResourceRuntimeCreationResult Create(string hash, string set = "fm.builtin.r1.macos", string platform = "macOS",
                int protocol = 1, long raw = 16777216, ResourceDeliveryMode mode = ResourceDeliveryMode.BuiltIn) =>
                YooAssetRuntimeFactory.Create(new ResourceBootstrapInput(bytes, hash, set, platform, protocol,
                    new[] { "resource-schema-v1" }, mode, maxRawFileBytes: raw));
            Assert.AreEqual("RES_TRUST", Create(new string('0', 64)).SafeCode);
            Assert.AreEqual("RES_SCHEMA", Create(pin, set: "other-set").SafeCode);
            Assert.AreEqual("RES_SCHEMA", Create(pin, platform: "android").SafeCode);
            Assert.AreEqual("RES_SCHEMA", Create(pin, protocol: 2).SafeCode);
            Assert.AreEqual("RES_BUDGET", Create(pin, raw: 1).SafeCode);
            Assert.AreEqual("RES_UNSUPPORTED", Create(pin, mode: ResourceDeliveryMode.RemoteHttps).SafeCode);
        }

        [Test]
        public void ExactManifestPinRejectsSameLengthMutation()
        {
            var bytes = FightMatchResourceBuildTests.TestManifest;
            var pin = FightMatchResourceBuild.Hash(bytes);
            Assert.IsTrue(RealYooSdk.MatchesManifest(bytes, bytes.Length, pin));
            var wrong = (byte[])bytes.Clone(); wrong[0] ^= 1;
            Assert.IsFalse(RealYooSdk.MatchesManifest(wrong, bytes.Length, pin));
            Assert.IsFalse(RealYooSdk.MatchesManifest(new byte[1], bytes.Length, pin));
        }

        [Test]
        public void InspectDoesNotStartSdkAndAssetsWaitForTransport()
        {
            using (var dispatcher = new Dispatcher())
            {
                var sdk = new FakeSdk(); var runtime = Runtime(sdk, dispatcher);
                Exception bodyFailure = null;
                try
                {
                    var inspection = runtime.InspectAsync(7, CancellationToken.None).Result;
                    Assert.IsFalse(sdk.Initialized); Assert.AreEqual(0, sdk.Reads);
                    Assert.AreEqual(0, inspection.RemainingBytes); Assert.IsFalse(inspection.RequiresNetwork);
                    Assert.IsFalse(runtime.Assets.AcquireAsync<FightMatchRawBytes>(Id(), inspection.ReleaseSetId, Budget(), 7).Result.IsAccepted);
                    Assert.IsTrue(ResourceDownloadPermit.TryCreate(inspection, false, out var permit, out _));
                    var prepare = runtime.PrepareAsync(permit, null, CancellationToken.None);
                    dispatcher.Complete(prepare);
                    Assert.AreEqual(ResourceTransportStatus.TransportVerified, prepare.Result.Status);
                    Assert.AreEqual(8, sdk.Reads); Assert.IsTrue(sdk.PinConfigured);
                    var acquire = runtime.Assets.AcquireAsync<FightMatchRawBytes>(Id(), inspection.ReleaseSetId, Budget(), 7);
                    dispatcher.Complete(acquire);
                    Assert.IsTrue(acquire.Result.IsAccepted);
                    var lease = acquire.Result.Lease;
                    Assert.Greater(lease.Asset.Length, 0);
                    Assert.AreEqual(ResourceTransportStatus.Rejected, runtime.PrepareAsync(permit, null, CancellationToken.None).Result.Status);
                    runtime.Dispose();
                    Assert.IsTrue(lease.IsReleased); Assert.IsNull(lease.Asset);
                    dispatcher.Complete(runtime.Stopped);
                    Assert.IsFalse(sdk.Initialized); Assert.AreEqual(0, sdk.PackageCount);
                }
                catch (Exception error) { bodyFailure = error; throw; }
                finally { Cleanup(runtime, dispatcher, bodyFailure); }
            }
        }

        [Test]
        public void CancelWaitsForActualPendingRawAndReleasesIt()
        {
            using (var dispatcher = new Dispatcher())
            {
                var sdk = new FakeSdk { HoldRaw = true }; var runtime = Runtime(sdk, dispatcher);
                Exception bodyFailure = null;
                using (var cancellation = new CancellationTokenSource())
                {
                    try
                    {
                        var inspection = runtime.InspectAsync(1, CancellationToken.None).Result;
                        ResourceDownloadPermit.TryCreate(inspection, false, out var permit, out _);
                        var task = runtime.PrepareAsync(permit, null, cancellation.Token);
                        Assert.AreEqual(1, sdk.Reads);
                        cancellation.Cancel();
                        dispatcher.Drain();
                        Assert.IsFalse(task.IsCompleted); Assert.IsFalse(runtime.Stopped.IsCompleted);
                        Assert.AreEqual(0, sdk.Releases);
                        sdk.HoldRaw = false;
                        dispatcher.Complete(task);
                        Assert.AreEqual(ResourceTransportStatus.Cancelled, task.Result.Status);
                        Assert.IsTrue(runtime.Stopped.IsCompleted); Assert.AreEqual(sdk.Reads, sdk.Releases);
                        Assert.IsFalse(sdk.Initialized); Assert.AreEqual(0, sdk.PackageCount);
                    }
                    catch (Exception error) { bodyFailure = error; throw; }
                    finally { sdk.HoldRaw = false; Cleanup(runtime, dispatcher, bodyFailure); }
                }
            }
        }

        [Test]
        public void CorruptRawNeverOpensAssetsAndClosesPackage()
        {
            using (var dispatcher = new Dispatcher())
            {
                var sdk = new FakeSdk { CorruptRaw = true }; var runtime = Runtime(sdk, dispatcher);
                Exception bodyFailure = null;
                try
                {
                    var inspection = runtime.InspectAsync(1, CancellationToken.None).Result;
                    ResourceDownloadPermit.TryCreate(inspection, false, out var permit, out _);
                    var task = runtime.PrepareAsync(permit, null, CancellationToken.None);
                    dispatcher.Complete(task);
                    Assert.AreEqual(ResourceTransportStatus.Rejected, task.Result.Status);
                    Assert.AreEqual(ResourceRuntimeDiagnosticCode.Asset, task.Result.Diagnostic.Code);
                    Assert.IsFalse(runtime.Assets.AcquireAsync<FightMatchRawBytes>(Id(), inspection.ReleaseSetId, Budget(), 1).Result.IsAccepted);
                    Assert.AreEqual(0, sdk.PackageCount); Assert.AreEqual(sdk.Reads, sdk.Releases);
                }
                catch (Exception error) { bodyFailure = error; throw; }
                finally { Cleanup(runtime, dispatcher, bodyFailure); }
            }
        }

        private static void Cleanup(YooAssetResourceRuntime runtime, Dispatcher dispatcher, Exception bodyFailure)
        {
            try { runtime.Dispose(); dispatcher.Complete(runtime.Stopped); }
            catch (Exception cleanupFailure)
            {
                if (bodyFailure == null) throw;
                throw new AggregateException("Test body failed before cleanup also failed.", bodyFailure, cleanupFailure);
            }
        }

        // Own both provider wakeups and async continuations, as the existing provider tests own their dispatcher.
        private sealed class Dispatcher : SynchronizationContext, IAssetDispatcher, IDisposable
        {
            private readonly SynchronizationContext previous = Current;
            private readonly int thread = Thread.CurrentThread.ManagedThreadId;
            private readonly Queue<Action> queue = new Queue<Action>();
            internal Dispatcher() { SetSynchronizationContext(this); }
            public bool IsMain => Thread.CurrentThread.ManagedThreadId == thread;
            public void Post(Action action) { lock (queue) queue.Enqueue(action); }
            public override void Post(SendOrPostCallback callback, object state) => Post(() => callback(state));
            public override SynchronizationContext CreateCopy() => this;
            internal void Drain()
            {
                Assert.IsTrue(IsMain);
                for (var count = 0; count < 4096; count++)
                {
                    Action action;
                    lock (queue) { if (queue.Count == 0) return; action = queue.Dequeue(); }
                    action();
                }
                Assert.Fail("Bounded fake SDK dispatch did not drain.");
            }
            internal void Complete(Task task)
            {
                Drain();
                Assert.IsTrue(task.IsCompleted, "Fake SDK task is still pending after its queued work drained.");
                task.GetAwaiter().GetResult();
            }
            public void Dispose() { SetSynchronizationContext(previous); }
        }
        private static FightMatchAssetId Id() { FightMatchAssetId.TryCreate("fm.text.full.artifact", out var id); return id; }
        private static AssetAcquireBudget Budget() => new AssetAcquireBudget(16777216, 1, 16, 16);
        private static YooAssetResourceRuntime Runtime(FakeSdk sdk, IAssetDispatcher dispatcher)
        {
            var boot = FightMatchResourceBuildTests.Boot(); var pin = FightMatchResourceBuild.Hash(boot);
            Assert.IsTrue(FightMatchResourceReleaseSet.TryDecodePinned(boot, pin, out var set, out _));
            Assert.IsTrue(YooAssetRuntimeFactory.TryAdmit(new ResourceAdmissionInput(boot, pin, set.ReleaseSetId, "macOS", 1,
                new[] { "resource-schema-v1" }, Array.Empty<string>(), DeliveryMode.BuiltIn, Array.Empty<string>(),
                new ResourceAdmissionBudget(262144, 268435456, 1073741824, 16777216)), out var plan, out _));
            var inputs = FightMatchResourceBuildTests.Inputs(); var entries = FightMatchResourceBuildTests.Plan().entries;
            for (var i = 0; i < entries.Length; i++) sdk.Bytes.Add(entries[i].assetId, inputs[i]);
            return new YooAssetResourceRuntime(set, plan, "/test-only/builtin", dispatcher, sdk);
        }
        private class Operation : IYooOperation
        {
            public virtual bool Done => true;
            public virtual bool Success => true;
            public UnityEngine.Object Asset => null;
            public event Action<IYooOperation> Completed { add { } remove { } }
            public virtual void Release() { }
        }
        private sealed class FakeSdk : IYooSdk, IRawYooSdk, IPinnedManifestYooSdk
        {
            internal readonly Dictionary<string, byte[]> Bytes = new Dictionary<string, byte[]>();
            private bool holdRaw;
            private Action wakeRaw;
            internal bool HoldRaw
            {
                get => holdRaw;
                set { holdRaw = value; if (!value) wakeRaw?.Invoke(); }
            }
            internal bool CorruptRaw, PinConfigured;
            internal int Reads, Releases;
            private object package;
            private bool ready, empty = true;
            private string version;
            public bool Initialized { get; private set; }
            public int PackageCount => package == null ? 0 : 1;
            public void Initialize() { Initialized = true; }
            public void Destroy() { Assert.IsNull(package); Initialized = false; }
            public object Find(string name) => package;
            public object Create(string name) => package = new object();
            public bool Ready(object value) => ready;
            public bool Busy(object value) => false;
            public bool Empty(object value) => empty;
            public string Version(object value) => version;
            public IYooOperation Initialize(object value, string root) => throw new AssertionException("Pinned initialization required.");
            public IYooOperation InitializePinned(object value, string root, long length, string sha)
            {
                Assert.AreEqual(FightMatchResourceBuildTests.TestManifest.Length, length);
                Assert.AreEqual(FightMatchResourceBuild.Hash(FightMatchResourceBuildTests.TestManifest), sha);
                PinConfigured = true; ready = true; empty = false; return new Operation();
            }
            public IYooOperation Manifest(object value, string selected) { version = selected; return new Operation(); }
            public bool HasLocation(object value, string location, Type type) => Bytes.ContainsKey(location);
            public IYooOperation Load(object value, string location, Type type) => throw new AssertionException("Raw operation required.");
            public IYooOperation Destroy(object value) { ready = false; empty = true; return new Operation(); }
            public void Remove(string name) { package = null; }
            public IRawYooOperation BeginRaw(object value, AssetMapping map, long limit, Action wake)
            { Reads++; wakeRaw = wake; return new Raw(this, map, limit); }
            private sealed class Raw : Operation, IRawYooOperation
            {
                private readonly FakeSdk sdk;
                private readonly RawReadState reader;
                private bool stopped;
                internal Raw(FakeSdk sdk, AssetMapping map, long limit)
                {
                    this.sdk = sdk;
                    var bytes = (byte[])sdk.Bytes[map.Location].Clone(); var hash = FightMatchResourceBuild.Hash(bytes);
                    if (sdk.CorruptRaw) bytes[0] ^= 1;
                    reader = new RawReadState(new MemoryStream(bytes, false), map.RawBytes.Value, hash, limit, length => new byte[length]);
                }
                public override bool Done => stopped || reader.Done;
                public override bool Success => Done && !Error.HasValue;
                public object Payload => reader.Payload;
                public FightMatchAssetDiagnosticCode? Error => stopped ? FightMatchAssetDiagnosticCode.StaleEpoch : reader.Error;
                public void Pump(bool stop, Func<bool> continueRaw)
                {
                    if (sdk.HoldRaw) return;
                    if (stop) { stopped = true; reader.Pump(true); }
                    else reader.PumpNext(continueRaw);
                }
                public override void Release() { Assert.IsTrue(Done); reader.Dispose(); sdk.Releases++; }
            }
        }
    }
}
