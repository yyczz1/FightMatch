using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FightMatch.YooAssetAdapter;
using NUnit.Framework;
using UnityEngine;
using Code = FightMatch.AssetAccess.FightMatchAssetDiagnosticCode;
using Stage = FightMatch.AssetAccess.FightMatchAssetDiagnosticStage;

namespace FightMatch.AssetAccess.Tests
{
    public sealed class FightMatchAssetProviderTests
    {
        private const string Set = "release-1";
        private static FightMatchAssetId Id(string text = "asset-a")
        {
            Assert.IsTrue(FightMatchAssetId.TryCreate(text, out var id));
            return id;
        }
        private static AssetAcquireBudget Budget(int pending = 32, int callbacks = 1024, int leases = 4096, long raw = 67108864) =>
            new AssetAcquireBudget(raw, pending, callbacks, leases);
        private static AssetMapping Map(string id = "asset-a", string set = Set, bool raw = false, long? bytes = null) =>
            new AssetMapping(id, set, "FightMatchMain", "v1", id, typeof(Texture2D), "/test-owned/builtin", raw, bytes);

        private sealed class Dispatcher : IAssetDispatcher
        {
            private readonly int thread = Thread.CurrentThread.ManagedThreadId;
            private readonly Queue<Action> queue = new Queue<Action>();
            internal bool FailPost;
            internal int Posts, MaximumQueued;
            public bool IsMain => Thread.CurrentThread.ManagedThreadId == thread;
            public void Post(Action action)
            {
                lock (queue)
                {
                    Posts++;
                    if (FailPost) throw new InvalidOperationException("private dispatcher failure");
                    queue.Enqueue(action);
                    MaximumQueued = Math.Max(MaximumQueued, queue.Count);
                }
            }
            internal void Run()
            {
                Assert.IsTrue(IsMain);
                for (var count = 0; count < 128; count++)
                {
                    Action action;
                    lock (queue) { if (queue.Count == 0) return; action = queue.Dequeue(); }
                    action();
                }
                Assert.Fail("Unbounded dispatch.");
            }
        }

        private sealed class Operation : IYooOperation
        {
            private Action<IYooOperation> callbacks;
            private readonly Action terminal;
            internal Action<IYooOperation> Captured;
            internal bool ThrowAsset, ThrowSubscription, ThrowRelease, Duplicate;
            internal int Releases, ReleaseThread;
            public bool Done { get; private set; }
            public bool Success { get; private set; }
            internal UnityEngine.Object Value;
            public UnityEngine.Object Asset => ThrowAsset ? throw new Exception("https://user:token@host/path?sig=secret") : Value;
            internal Operation(bool complete, bool success, UnityEngine.Object value = null, Action terminal = null)
            {
                this.terminal = terminal; Value = value;
                if (complete) Finish(success);
            }
            public event Action<IYooOperation> Completed
            {
                add
                {
                    Captured = value;
                    if (ThrowSubscription) throw new Exception("/Users/example/private");
                    callbacks += value;
                    if (Done) { value(this); if (Duplicate) value(this); }
                }
                remove { callbacks -= value; }
            }
            internal void Finish(bool success = true)
            {
                if (!Done) { Done = true; Success = success; if (success) terminal?.Invoke(); }
                callbacks?.Invoke(this);
                if (Duplicate) callbacks?.Invoke(this);
            }
            internal void Late(IYooOperation sender = null) { Captured?.Invoke(sender ?? this); }
            public void Release()
            {
                Assert.IsTrue(Done);
                Assert.IsNull(callbacks, "Unsubscribe before release.");
                if (ThrowRelease) { ThrowRelease = false; throw new Exception("release exception marker"); }
                Releases++;
                ReleaseThread = Thread.CurrentThread.ManagedThreadId;
            }
        }

        private sealed class Sdk : IYooSdk, IDisposable
        {
            internal sealed class Package { internal bool Ready, Empty = true; internal string Version; }
            internal readonly Dictionary<string, Package> Packages = new Dictionary<string, Package>();
            internal readonly List<Operation> Operations = new List<Operation>();
            internal readonly List<Operation> Loads = new List<Operation>();
            internal readonly Texture2D Texture = new Texture2D(1, 1);
            internal bool AutoInit = true, AutoManifest = true, AutoLoad = true, AutoDestroy = true;
            internal bool FailInit, FailManifest, FailDestroyOnce, FailDestroyAlways, Location = true, WrongVersion, ThrowLoad;
            internal bool NullAsset, WrongAsset, ThrowAsset, ThrowSubscription, Duplicate;
            internal bool ThrowVersion;
            private GameObject wrong;
            internal int Initializations, PackageInitializations, Creates, Manifests, LoadCalls, Destroys, Removes, GlobalDestroys;
            public bool Initialized { get; private set; }
            public int PackageCount => Packages.Count;
            public void Initialize() { Initializations++; Initialized = true; }
            public void Destroy() { Assert.AreEqual(0, Packages.Count); GlobalDestroys++; Initialized = false; }
            public object Find(string name) => Packages.TryGetValue(name, out var value) ? value : null;
            public object Create(string name) { Creates++; return Packages[name] = new Package(); }
            public bool Ready(object package) => ((Package)package).Ready;
            public bool Busy(object package) => false;
            public bool Empty(object package) => ((Package)package).Empty;
            public string Version(object package)
            {
                if (ThrowVersion) throw new Exception("private version exception marker");
                return WrongVersion ? "wrong" : ((Package)package).Version;
            }
            public IYooOperation Initialize(object package, string root)
            {
                Assert.AreEqual("/test-owned/builtin", root);
                PackageInitializations++;
                return Add(new Operation(AutoInit, !FailInit, terminal: () =>
                { ((Package)package).Ready = true; ((Package)package).Empty = false; }));
            }
            public IYooOperation Manifest(object package, string version)
            {
                Manifests++;
                return Add(new Operation(AutoManifest, !FailManifest, terminal: () => ((Package)package).Version = version));
            }
            public bool HasLocation(object package, string location, Type type) => Location;
            public IYooOperation Load(object package, string location, Type type)
            {
                LoadCalls++;
                if (ThrowLoad) throw new Exception(@"C:\secret SDK exception marker");
                Assert.AreEqual(typeof(Texture2D), type);
                UnityEngine.Object value = NullAsset ? null : Texture;
                if (WrongAsset) value = wrong = new GameObject("test-owned wrong asset");
                var operation = new Operation(AutoLoad, true, value)
                { ThrowAsset = ThrowAsset, ThrowSubscription = ThrowSubscription, Duplicate = Duplicate };
                Loads.Add(operation);
                return Add(operation);
            }
            public IYooOperation Destroy(object package)
            {
                Destroys++;
                var success = !FailDestroyOnce && !FailDestroyAlways;
                FailDestroyOnce = false;
                return Add(new Operation(AutoDestroy, success, terminal: () =>
                { ((Package)package).Ready = false; ((Package)package).Empty = true; }));
            }
            public void Remove(string name) { Assert.IsTrue(Packages[name].Empty); Removes++; Packages.Remove(name); }
            private Operation Add(Operation operation) { Operations.Add(operation); return operation; }
            internal void External(bool ready = true, string version = "v1")
            {
                Initialized = true;
                Packages.Add("FightMatchMain", new Package { Ready = ready, Empty = !ready, Version = version });
            }
            public void Dispose()
            {
                UnityEngine.Object.DestroyImmediate(Texture);
                if (wrong != null) UnityEngine.Object.DestroyImmediate(wrong);
            }
        }

        private sealed class Rig : IDisposable
        {
            internal readonly Sdk Sdk;
            internal readonly Dispatcher Dispatch;
            internal readonly YooAssetAssetProvider Provider;
            internal readonly List<Task<FightMatchAssetAcquireResult<Texture2D>>> Tasks = new List<Task<FightMatchAssetAcquireResult<Texture2D>>>();
            internal readonly List<FightMatchAssetDiagnostic> Diagnostics = new List<FightMatchAssetDiagnostic>();
            private readonly bool owns;
            internal Rig(Sdk sdk = null, Dispatcher dispatcher = null, params AssetMapping[] maps)
            {
                owns = sdk == null; Sdk = sdk ?? new Sdk(); Dispatch = dispatcher ?? new Dispatcher();
                Provider = new YooAssetAssetProvider(maps.Length == 0 ? new[] { Map() } : maps,
                    Dispatch, Sdk, diagnostic => { Diagnostics.Add(diagnostic); throw new Exception("sink failure"); });
            }
            internal Task<FightMatchAssetAcquireResult<Texture2D>> Request(FightMatchAssetId id = null,
                string set = Set, AssetAcquireBudget budget = null, long epoch = 1)
            {
                var task = Provider.AcquireAsync<Texture2D>(id ?? Id(), set, budget ?? Budget(), epoch);
                Tasks.Add(task); return task;
            }
            internal FightMatchAssetAcquireResult<Texture2D> Result(Task<FightMatchAssetAcquireResult<Texture2D>> task)
            {
                Dispatch.Run(); Assert.IsTrue(task.IsCompleted, "Expected terminal project result."); return task.Result;
            }
            internal void CompleteAll()
            {
                for (var i = 0; i < 8; i++)
                {
                    foreach (var op in Sdk.Operations.ToArray()) if (!op.Done) op.Finish();
                    Dispatch.Run();
                }
            }
            public void Dispose()
            {
                Dispatch.FailPost = false;
                CompleteAll();
                foreach (var task in Tasks)
                    if (task.IsCompleted && task.Result.IsAccepted) task.Result.Lease.Dispose();
                Dispatch.Run();
                Provider.CloseAsync();
                CompleteAll();
                if (owns) Sdk.Dispose();
            }
        }

        private static void Rejected<T>(Task<FightMatchAssetAcquireResult<T>> task, Code code, Stage stage = Stage.ValidateRequest) where T : class
        {
            Assert.IsTrue(task.IsCompleted);
            var result = task.Result;
            Assert.IsFalse(result.IsAccepted); Assert.IsNull(result.Lease);
            Assert.AreEqual(code, result.Diagnostic.Code); Assert.AreEqual(stage, result.Diagnostic.Stage);
            foreach (var marker in new[] { "secret", "token", "/Users", "C:\\", "https://", "exception marker" })
                StringAssert.DoesNotContain(marker, result.Diagnostic.SafeDetail);
        }

        private sealed class MemoryLease : IFightMatchAssetLease<object>
        {
            public FightMatchAssetId AssetId => Id();
            public string ReleaseSetId => Set;
            public string LeaseId { get; } = Guid.NewGuid().ToString("N");
            public object Asset => IsReleased ? null : this;
            public bool IsReleased { get; private set; }
            public void Dispose() { IsReleased = true; }
        }
        private sealed class ContractFake : IFightMatchAssetProvider
        {
            public Task<FightMatchAssetAcquireResult<T>> AcquireAsync<T>(FightMatchAssetId id, string set,
                AssetAcquireBudget budget, long epoch) where T : class
            {
                if (typeof(T) == typeof(object))
                    return Task.FromResult(FightMatchAssetAcquireResult<T>.Accepted((IFightMatchAssetLease<T>)(object)new MemoryLease(), epoch, set));
                return Task.FromResult(FightMatchAssetAcquireResult<T>.Rejected(new FightMatchAssetDiagnostic(
                    Code.WrongAssetType, Stage.ValidateRequest, id, set, false, ""), epoch, set));
            }
        }

        [Test]
        public void B00_CallersUseOnlyProjectInterfaceAndResults()
        {
            IFightMatchAssetProvider provider = new ContractFake();
            var result = provider.AcquireAsync<object>(Id(), Set, Budget(), 1).Result;
            Assert.IsTrue(result.IsAccepted); Assert.IsNull(result.Diagnostic);
            Assert.AreEqual(Set, result.ReleaseSetId); Assert.AreEqual(1, result.RequestEpoch);
            result.Lease.Dispose(); Assert.IsTrue(result.Lease.IsReleased);
            Rejected(provider.AcquireAsync<string>(Id(), Set, Budget(), 1), Code.WrongAssetType);
        }

        [Test]
        public void B02_B04_ValidationOrderRejectsInvalidUnknownTypesAndSetsWithoutSdk()
        {
            using (var rig = new Rig())
            {
                var p = rig.Provider;
                Rejected(p.AcquireAsync<Texture2D>(null, "bad set", null, 0), Code.InvalidAssetId);
                foreach (var raw in new[] { "https://user:token@host/path?sig=secret", "/Users/example/private", @"C:\secret", "a\nb" })
                {
                    Assert.IsFalse(FightMatchAssetId.TryCreate(raw, out var id));
                    var task = p.AcquireAsync<Texture2D>(id, Set, Budget(), 1);
                    Rejected(task, Code.InvalidAssetId); Assert.IsNull(task.Result.Diagnostic.AssetId);
                    var set = p.AcquireAsync<Texture2D>(Id(), raw, Budget(), 1);
                    Rejected(set, Code.WrongReleaseSet); Assert.IsNull(set.Result.ReleaseSetId);
                }
                foreach (var set in new[] { null, "", " ", "latest", "other-set" })
                    Rejected(p.AcquireAsync<Texture2D>(Id(), set, Budget(), 1), Code.WrongReleaseSet);
                Rejected(p.AcquireAsync<Texture2D>(Id(), Set, null, 0), Code.BudgetExceeded);
                Rejected(p.AcquireAsync<Texture2D>(Id(), Set, Budget(), 0), Code.StaleEpoch);
                Rejected(p.AcquireAsync<Texture2D>(Id("unknown"), Set, Budget(), 1), Code.UnknownAsset);
                Rejected(p.AcquireAsync<object>(Id(), Set, Budget(), 1), Code.WrongAssetType);
                Rejected(p.AcquireAsync<GameObject>(Id(), Set, Budget(), 1), Code.WrongAssetType);
                Assert.AreEqual(0, rig.Sdk.Initializations); Assert.AreEqual(0, rig.Sdk.LoadCalls);
            }
        }

        [Test]
        public void B05_FailuresHaveDistinctStagesAndNoSdkText()
        {
            foreach (var kind in new[] { "init", "manifest", "version", "location", "load", "null", "type", "asset", "subscribe" })
            using (var rig = new Rig())
            {
                var sdk = rig.Sdk;
                sdk.FailInit = kind == "init"; sdk.FailManifest = kind == "manifest";
                sdk.WrongVersion = kind == "version"; sdk.Location = kind != "location";
                sdk.ThrowLoad = kind == "load"; sdk.NullAsset = kind == "null";
                sdk.WrongAsset = kind == "type"; sdk.ThrowAsset = kind == "asset"; sdk.ThrowSubscription = kind == "subscribe";
                var task = rig.Request(); rig.Dispatch.Run();
                var code = kind == "init" ? Code.PackageUnavailable : kind == "manifest" || kind == "version" ?
                    Code.ManifestUnavailable : kind == "location" ? Code.LocationUnavailable : kind == "type" ? Code.WrongAssetType : Code.SdkFailure;
                var stage = kind == "init" ? Stage.InitializePackage : kind == "manifest" || kind == "version" ?
                    Stage.SelectManifest : kind == "location" ? Stage.ResolveLocation : kind == "null" || kind == "type" ?
                    Stage.ValidateResult : Stage.Acquire;
                Rejected(task, code, stage);
                Assert.IsTrue(sdk.Loads.All(op => op.Releases == 1));
            }
        }

        [Test]
        public void B06_B09_SharedPendingAndLiveHandlesHaveIndependentIdempotentLeases()
        {
            using (var rig = new Rig())
            {
                rig.Sdk.AutoLoad = false;
                var first = rig.Request(); var second = rig.Request();
                Assert.AreEqual(1, rig.Sdk.LoadCalls);
                rig.Sdk.Loads[0].Finish();
                var a = rig.Result(first).Lease; var b = rig.Result(second).Lease;
                Assert.AreNotEqual(a.LeaseId, b.LeaseId); Assert.AreEqual(32, a.LeaseId.Length);
                Assert.AreSame(a.Asset, b.Asset);
                var third = rig.Result(rig.Request()).Lease;
                Assert.AreEqual(1, rig.Sdk.LoadCalls);
                a.Dispose(); a.Dispose(); rig.Dispatch.Run();
                Assert.IsNull(a.Asset); Assert.IsNull(a.Asset);
                Assert.AreEqual(1, rig.Diagnostics.Count); Assert.AreEqual(Code.ReleasedLease, rig.Diagnostics[0].Code);
                Assert.AreSame(rig.Sdk.Texture, b.Asset); Assert.AreEqual(0, rig.Sdk.Loads[0].Releases);
                b.Dispose(); rig.Dispatch.Run(); Assert.AreEqual(0, rig.Sdk.Loads[0].Releases);
                third.Dispose(); rig.Dispatch.Run(); Assert.AreEqual(1, rig.Sdk.Loads[0].Releases);
            }
        }

        [Test]
        public void B10_B11_EpochAndForeignLateDuplicateCallbacksCannotPublishOrReleaseNewHandles()
        {
            using (var rig = new Rig())
            {
                rig.Sdk.AutoLoad = false;
                var old = rig.Request(); var oldHandle = rig.Sdk.Loads[0];
                rig.Provider.AdvanceEpoch(2);
                var current = rig.Request(epoch: 2); var currentHandle = rig.Sdk.Loads[1];
                oldHandle.Late(currentHandle); rig.Dispatch.Run();
                Assert.IsFalse(old.IsCompleted); Assert.IsFalse(current.IsCompleted);
                currentHandle.Duplicate = true; currentHandle.Finish();
                var lease = rig.Result(current).Lease;
                oldHandle.Finish(); rig.Dispatch.Run();
                Rejected(old, Code.StaleEpoch, Stage.ValidateResult);
                Assert.AreEqual(1, oldHandle.Releases); Assert.AreEqual(0, currentHandle.Releases);
                oldHandle.Late(); currentHandle.Late(); rig.Dispatch.Run();
                Assert.AreEqual(1, oldHandle.Releases); Assert.AreSame(rig.Sdk.Texture, lease.Asset);
                Rejected(rig.Request(epoch: 1), Code.StaleEpoch);
            }
        }

        [Test]
        public void B12_RequestAndModuleLimitsReserveBeforeStartingSdk()
        {
            foreach (var limit in new[] { "pending", "callbacks", "leases" })
            using (var rig = new Rig())
            {
                rig.Sdk.AutoLoad = false;
                var budget = Budget(pending: limit == "pending" ? 1 : 32,
                    callbacks: limit == "callbacks" ? 1 : 1024, leases: limit == "leases" ? 1 : 4096);
                rig.Request(budget: budget);
                Rejected(rig.Request(budget: budget), Code.BudgetExceeded);
                Assert.AreEqual(1, rig.Sdk.LoadCalls);
            }
            using (var first = new Rig())
            using (var second = new Rig())
            {
                first.Sdk.AutoLoad = false; first.Request();
                Rejected(second.Request(budget: Budget(pending: 1)), Code.BudgetExceeded);
                Assert.AreEqual(0, second.Sdk.Initializations);
            }
            foreach (var bytes in new long?[] { null, -1, 2 })
            using (var rig = new Rig(null, null, Map(raw: true, bytes: bytes)))
            {
                Rejected(rig.Request(budget: Budget(raw: 1)), Code.BudgetExceeded);
                Assert.AreEqual(0, rig.Sdk.Initializations);
            }
        }

        [Test]
        public void AnyThreadDisposeUsesOneReservedWakeAndNeverReleasesOffThread()
        {
            using (var rig = new Rig())
            {
                var first = rig.Result(rig.Request(budget: Budget(callbacks: 2))).Lease;
                var second = rig.Result(rig.Request(budget: Budget(callbacks: 2))).Lease;
                Rejected(rig.Request(budget: Budget(callbacks: 2)), Code.BudgetExceeded);
                var main = Thread.CurrentThread.ManagedThreadId;
                var posts = rig.Dispatch.Posts;
                var worker = new Thread(() => { first.Dispose(); second.Dispose(); first.Dispose(); });
                worker.Start(); worker.Join();
                Assert.IsTrue(first.IsReleased); Assert.AreEqual(0, rig.Sdk.Loads[0].Releases);
                Assert.AreEqual(posts + 1, rig.Dispatch.Posts);
                rig.Dispatch.Run();
                Assert.AreEqual(1, rig.Sdk.Loads[0].Releases); Assert.AreEqual(main, rig.Sdk.Loads[0].ReleaseThread);
                Assert.LessOrEqual(rig.Dispatch.MaximumQueued, 1);
            }
        }

        [Test]
        public void FailedPostAndFailedReleaseRetainCleanupForNextMainEntry()
        {
            using (var rig = new Rig())
            {
                var lease = rig.Result(rig.Request()).Lease;
                rig.Dispatch.FailPost = true;
                var worker = new Thread(lease.Dispose); worker.Start(); worker.Join();
                Assert.IsTrue(lease.IsReleased); Assert.AreEqual(0, rig.Sdk.Loads[0].Releases);
                rig.Sdk.Loads[0].ThrowRelease = true;
                var close = rig.Provider.CloseAsync();
                Assert.IsFalse(close.IsCompleted);
                rig.Dispatch.FailPost = false;
                Assert.AreSame(close, rig.Provider.CloseAsync()); rig.Dispatch.Run();
                Assert.IsTrue(close.IsCompleted); Assert.AreEqual(1, rig.Sdk.Loads[0].Releases);
            }
        }

        [Test]
        public void BackgroundAcquirePerformsPureValidationThenRejectsWithoutSdk()
        {
            using (var rig = new Rig())
            {
                Task<FightMatchAssetAcquireResult<Texture2D>> invalid = null, valid = null;
                var worker = new Thread(() =>
                {
                    invalid = rig.Provider.AcquireAsync<Texture2D>(null, Set, Budget(), 1);
                    valid = rig.Provider.AcquireAsync<Texture2D>(Id(), Set, Budget(), 1);
                });
                worker.Start(); worker.Join();
                Rejected(invalid, Code.InvalidAssetId); Rejected(valid, Code.SdkFailure);
                Assert.AreEqual(0, rig.Sdk.Initializations);
            }
        }

        [Test]
        public void CloseWaitsForRealInitManifestOrLoadTerminalsAndDoesNotPublish()
        {
            foreach (var stage in new[] { "init", "manifest", "load" })
            using (var rig = new Rig())
            {
                rig.Sdk.AutoInit = stage != "init"; rig.Sdk.AutoManifest = stage != "manifest"; rig.Sdk.AutoLoad = stage != "load";
                var acquire = rig.Request(); var close = rig.Provider.CloseAsync();
                Assert.IsFalse(close.IsCompleted); Assert.IsFalse(acquire.IsCompleted);
                Assert.AreEqual(0, rig.Sdk.Destroys);
                rig.CompleteAll();
                Rejected(acquire, Code.StaleEpoch, Stage.ValidateResult);
                Assert.IsTrue(close.IsCompleted); Assert.AreEqual(1, rig.Sdk.Destroys);
                Assert.AreEqual(stage == "load" ? 1 : 0, rig.Sdk.LoadCalls);
                Assert.IsTrue(rig.Provider.GlobalDestroyed);
            }
        }

        [Test]
        public void ClosePreservesLiveLeaseAndRetriesFailedPackageDestruction()
        {
            using (var rig = new Rig())
            {
                var lease = rig.Result(rig.Request()).Lease;
                var close = rig.Provider.CloseAsync();
                Assert.IsFalse(close.IsCompleted); Assert.AreSame(rig.Sdk.Texture, lease.Asset);
                Assert.AreEqual(0, rig.Sdk.Destroys);
                rig.Sdk.FailDestroyOnce = true;
                lease.Dispose(); rig.Dispatch.Run();
                rig.Provider.CloseAsync(); rig.Dispatch.Run();
                Assert.IsTrue(close.IsCompleted);
                Assert.AreEqual(2, rig.Sdk.Destroys); Assert.AreEqual(1, rig.Sdk.Removes);
                Assert.AreEqual(1, rig.Sdk.GlobalDestroys);
            }
        }

        [Test]
        public void BorrowedPackageAndGlobalAreNeverReinitializedSwitchedOrDestroyed()
        {
            foreach (var kind in new[] { "exact", "version", "incomplete" })
            using (var sdk = new Sdk())
            {
                sdk.External(kind != "incomplete", kind == "version" ? "other" : "v1");
                using (var rig = new Rig(sdk))
                {
                    var result = rig.Result(rig.Request());
                    if (kind == "exact") Assert.IsTrue(result.IsAccepted);
                    else Assert.AreEqual(kind == "version" ? Code.ManifestUnavailable : Code.PackageUnavailable, result.Diagnostic.Code);
                }
                Assert.AreEqual(0, sdk.Initializations); Assert.AreEqual(0, sdk.Manifests);
                Assert.AreEqual(0, sdk.PackageInitializations);
                Assert.AreEqual(0, sdk.Destroys); Assert.AreEqual(0, sdk.Removes); Assert.AreEqual(0, sdk.GlobalDestroys);
            }
        }

        [Test]
        public void CreatorShutdownCannotDestroyPackageUsedByAnotherProvider()
        {
            using (var sdk = new Sdk())
            {
                var dispatcher = new Dispatcher();
                using (var creator = new Rig(sdk, dispatcher))
                using (var borrower = new Rig(sdk, dispatcher))
                {
                    var a = creator.Result(creator.Request()).Lease;
                    var b = borrower.Result(borrower.Request()).Lease;
                    a.Dispose(); dispatcher.Run();
                    var closing = creator.Provider.CloseAsync();
                    Assert.AreEqual(0, sdk.Destroys); Assert.AreSame(sdk.Texture, b.Asset);
                    b.Dispose(); dispatcher.Run();
                    borrower.Provider.CloseAsync(); dispatcher.Run();
                    creator.Provider.CloseAsync(); dispatcher.Run();
                    Assert.IsTrue(closing.IsCompleted); Assert.AreEqual(1, sdk.Destroys);
                    Assert.AreEqual(1, sdk.Creates); Assert.AreEqual(1, sdk.Initializations);
                }
            }
        }

        [Test]
        public void FailedDestructionDoesNotCreateAnAutomaticRetryLoop()
        {
            using (var rig = new Rig())
            {
                rig.Result(rig.Request()).Lease.Dispose();
                rig.Dispatch.Run();
                rig.Sdk.FailDestroyAlways = true;
                var close = rig.Provider.CloseAsync();
                rig.Dispatch.Run();
                Assert.IsFalse(close.IsCompleted); Assert.AreEqual(1, rig.Sdk.Destroys);
                rig.Provider.CloseAsync(); rig.Dispatch.Run();
                Assert.IsFalse(close.IsCompleted); Assert.AreEqual(2, rig.Sdk.Destroys);
                rig.Sdk.FailDestroyAlways = false;
                rig.Provider.CloseAsync(); rig.Dispatch.Run();
                Assert.IsTrue(close.IsCompleted); Assert.AreEqual(3, rig.Sdk.Destroys);
            }
        }

        [Test]
        public void SynchronousDuplicateAndPriorProviderCallbacksRemainExactOnce()
        {
            using (var sdk = new Sdk())
            {
                Operation old;
                using (var prior = new Rig(sdk))
                {
                    sdk.Duplicate = true;
                    var lease = prior.Result(prior.Request()).Lease;
                    old = sdk.Loads[0];
                    lease.Dispose(); prior.Dispatch.Run();
                    Assert.IsTrue(prior.Provider.CloseAsync().IsCompleted);
                    using (var next = new Rig(sdk))
                    {
                        var current = next.Result(next.Request()).Lease;
                        old.Late(); old.Late(); prior.Dispatch.Run(); next.Dispatch.Run();
                        Assert.AreEqual(1, old.Releases);
                        Assert.AreEqual(0, sdk.Loads[1].Releases);
                        Assert.AreSame(sdk.Texture, current.Asset);
                    }
                }
            }
        }

        [Test]
        public void OwnedPackageUnderBorrowedGlobalPreservesGlobalAndOtherPackages()
        {
            using (var sdk = new Sdk())
            {
                sdk.Initialize();
                sdk.Packages.Add("other", new Sdk.Package());
                using (var rig = new Rig(sdk)) Assert.IsTrue(rig.Result(rig.Request()).IsAccepted);
                Assert.AreEqual(1, sdk.Destroys); Assert.AreEqual(1, sdk.Removes);
                Assert.AreEqual(0, sdk.GlobalDestroys); Assert.IsTrue(sdk.Packages.ContainsKey("other"));
            }
        }

        [Test]
        public void B15_B16_AssemblyBoundaryHasNoBusinessReferencesAndContractsNeedNoSdk()
        {
            var contracts = typeof(IFightMatchAssetProvider).Assembly.GetReferencedAssemblies().Select(a => a.Name).ToArray();
            CollectionAssert.DoesNotContain(contracts, "YooAsset");
            Assert.IsFalse(contracts.Any(name => name.StartsWith("UnityEngine", StringComparison.Ordinal)));
            var adapter = typeof(YooAssetAssetProvider).Assembly.GetReferencedAssemblies().Select(a => a.Name).ToArray();
            CollectionAssert.Contains(adapter, "YooAsset");
            foreach (var forbidden in new[] { "FightMatch.Core", "FightMatch.Application", "FightMatch.Content",
                "FightMatch.Platform", "FightMatch.Presentation", "FightMatch.Host" })
                CollectionAssert.DoesNotContain(adapter, forbidden);
        }

        [Test]
        public void ProductionConstructorRejectsThreadPoolContextsBeforeSdkAccess()
        {
            Exception failure = null;
            var worker = new Thread(() =>
            {
                SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
                try { new YooAssetAssetProvider(new[] { Map() }); } catch (Exception error) { failure = error; }
            });
            worker.Start(); worker.Join();
            Assert.IsInstanceOf<InvalidOperationException>(failure);
        }

        private static YooAssetRemoteServices Remote(string primary = "https://primary.invalid/", string fallback = null,
            string platform = "android", string set = Set, string package = "FightMatchMain", string version = "v1") =>
            new YooAssetRemoteServices(primary, fallback, new[] { "primary.invalid", "fallback.invalid" },
                platform, set, package, version, new[] { "android" }, new[] { Set }, new[] { "FightMatchMain" }, new[] { "v1" });

        [Test]
        public void B13_B14_RemoteUrlsUseExactImmutableIdentityAndRejectInjection()
        {
            var expected = "https://primary.invalid/fightmatch/android/release-1/yoo/FightMatchMain/v1/hash.bundle";
            CollectionAssert.AreEqual(new[] { expected }, Remote().GetRemoteUrls("hash.bundle"));
            CollectionAssert.AreEqual(new[] { expected, expected.Replace("primary.invalid", "fallback.invalid") },
                Remote(fallback: "https://fallback.invalid/").GetRemoteUrls("hash.bundle"));
            foreach (var file in new[] { null, "", "/file", "../file", "a/../b", "a//b", "a?sig=secret", "a#b", "a\nb", @"C:\secret", "%2e%2e/file", "https://host/a", "a/" })
                Assert.IsEmpty(Remote().GetRemoteUrls(file));
            foreach (var remote in new[] { Remote("http://primary.invalid/"), Remote("https://unknown.invalid/"),
                Remote("https://user:token@primary.invalid/"), Remote(fallback: "https://primary.invalid/"),
                Remote(platform: "ios"), Remote(set: "latest"), Remote(package: "unknown"), Remote(version: "latest"),
                Remote("https://primary.invalid/path/"), Remote("https://primary.invalid/?sig=secret") })
                Assert.IsEmpty(remote.GetRemoteUrls("hash.bundle"));
        }

        [Test]
        public void FailedInitializationCanBeRetriedByLaterAcquisition()
        {
            using (var sdk = new Sdk())
            using (var first = new Rig(sdk))
            using (var observer = new Rig(sdk))
            {
                sdk.AutoInit = false;
                var failed = first.Request();
                var oldObserver = observer.Request();
                Assert.AreEqual(1, sdk.PackageInitializations);
                sdk.Operations[0].Finish(false);
                first.Dispatch.Run();
                Rejected(failed, Code.PackageUnavailable, Stage.InitializePackage);
                Assert.IsFalse(oldObserver.IsCompleted);
                first.Dispatch.Run();
                Assert.AreEqual(1, sdk.PackageInitializations);
                Assert.AreEqual(0, sdk.Manifests);
                Assert.AreEqual(0, sdk.LoadCalls);
                sdk.AutoInit = true;
                var retry = first.Result(first.Request());
                Assert.IsTrue(retry.IsAccepted);
                Assert.AreEqual(2, sdk.PackageInitializations);
                Assert.AreEqual(1, sdk.Manifests);
                Assert.AreEqual(1, sdk.LoadCalls);
                observer.Dispatch.Run();
                Rejected(oldObserver, Code.PackageUnavailable, Stage.InitializePackage);
                Rejected(failed, Code.PackageUnavailable, Stage.InitializePackage);
                Assert.AreEqual(2, sdk.PackageInitializations);
                retry.Lease.Dispose();
                first.Dispatch.Run();
                observer.Dispatch.Run();
                var firstClose = first.Provider.CloseAsync();
                var observerClose = observer.Provider.CloseAsync();
                first.Dispatch.Run();
                observer.Dispatch.Run();
                Assert.IsTrue(firstClose.IsCompleted);
                Assert.IsTrue(observerClose.IsCompleted);
            }
        }

        [Test]
        public void FailedManifestCanBeRetriedWithoutReinitializingPackage()
        {
            using (var sdk = new Sdk())
            using (var first = new Rig(sdk))
            using (var observer = new Rig(sdk))
            {
                sdk.AutoManifest = false;
                var failed = first.Request();
                var oldObserver = observer.Request();
                Assert.AreEqual(1, sdk.PackageInitializations);
                Assert.AreEqual(1, sdk.Manifests);
                sdk.Operations[1].Finish(false);
                first.Dispatch.Run();
                Rejected(failed, Code.ManifestUnavailable, Stage.SelectManifest);
                Assert.IsFalse(oldObserver.IsCompleted);
                first.Dispatch.Run();
                Assert.AreEqual(1, sdk.PackageInitializations);
                Assert.AreEqual(1, sdk.Manifests);
                Assert.AreEqual(0, sdk.LoadCalls);
                sdk.AutoManifest = true;
                var retry = first.Result(first.Request());
                Assert.IsTrue(retry.IsAccepted);
                Assert.AreEqual(1, sdk.PackageInitializations);
                Assert.AreEqual(2, sdk.Manifests);
                Assert.AreEqual(1, sdk.LoadCalls);
                observer.Dispatch.Run();
                Rejected(oldObserver, Code.ManifestUnavailable, Stage.SelectManifest);
                Rejected(failed, Code.ManifestUnavailable, Stage.SelectManifest);
                Assert.AreEqual(1, sdk.PackageInitializations);
                Assert.AreEqual(2, sdk.Manifests);
                retry.Lease.Dispose();
                first.Dispatch.Run();
                observer.Dispatch.Run();
                var firstClose = first.Provider.CloseAsync();
                var observerClose = observer.Provider.CloseAsync();
                first.Dispatch.Run();
                observer.Dispatch.Run();
                Assert.IsTrue(firstClose.IsCompleted);
                Assert.IsTrue(observerClose.IsCompleted);
            }
        }

        [Test]
        public void InvalidReleaseSetsEmitExactCanonicalReasonBeforeSdkWork()
        {
            using (var rig = new Rig())
            {
                foreach (var set in new[] { null, "", " ", "\t", "bad set", "latest", "../set",
                    "https://user:token@host/path?sig=secret", "/Users/example/private", @"C:\secret" })
                {
                    var task = rig.Provider.AcquireAsync<Texture2D>(Id(), set, Budget(), 1);
                    Rejected(task, Code.WrongReleaseSet);
                    Assert.IsNull(task.Result.ReleaseSetId);
                    Assert.IsNull(task.Result.Diagnostic.ReleaseSetId);
                    Assert.AreEqual("reason=invalid-release-set", task.Result.Diagnostic.SafeDetail);
                    Assert.AreEqual(0, rig.Sdk.Initializations);
                    Assert.AreEqual(0, rig.Sdk.PackageInitializations);
                    Assert.AreEqual(0, rig.Sdk.Manifests);
                    Assert.AreEqual(0, rig.Sdk.LoadCalls);
                }
                var invalidId = rig.Provider.AcquireAsync<Texture2D>(null, "bad set", null, 0);
                Rejected(invalidId, Code.InvalidAssetId);
                Assert.IsNull(invalidId.Result.Diagnostic.AssetId);
                Assert.IsNull(invalidId.Result.ReleaseSetId);
                var mismatch = rig.Provider.AcquireAsync<Texture2D>(Id(), "another-set", Budget(), 1);
                Rejected(mismatch, Code.WrongReleaseSet);
                Assert.AreEqual("another-set", mismatch.Result.ReleaseSetId);
                Assert.AreEqual("status=failed", mismatch.Result.Diagnostic.SafeDetail);
                Assert.AreEqual(0, rig.Sdk.Initializations);
                Assert.AreEqual(0, rig.Sdk.LoadCalls);
            }
        }

        [Test]
        public void PostLoadVersionMismatchRetriesManifestOnLaterAcquisition()
        {
            AssertPostLoadVersionRetry(false);
        }

        [Test]
        public void PostLoadVersionExceptionRetriesManifestOnLaterAcquisition()
        {
            AssertPostLoadVersionRetry(true);
        }

        private static void AssertPostLoadVersionRetry(bool throws)
        {
            using (var rig = new Rig())
            {
                var sdk = rig.Sdk;
                sdk.AutoManifest = false;
                sdk.WrongVersion = !throws;
                sdk.ThrowVersion = throws;
                var failed = rig.Request();
                Assert.IsFalse(failed.IsCompleted);
                sdk.Operations[1].Finish();
                Assert.IsTrue(sdk.Operations[1].Success);
                rig.Dispatch.Run();
                Rejected(failed, Code.ManifestUnavailable, Stage.SelectManifest);
                Assert.AreEqual(1, sdk.PackageInitializations);
                Assert.AreEqual(1, sdk.Manifests);
                Assert.AreEqual(0, sdk.LoadCalls);
                sdk.WrongVersion = false;
                sdk.ThrowVersion = false;
                rig.Dispatch.Run();
                Assert.AreEqual(1, sdk.Manifests);
                var retry = rig.Request();
                rig.Dispatch.Run();
                Assert.IsFalse(retry.IsCompleted);
                Assert.AreEqual(1, sdk.PackageInitializations);
                Assert.AreEqual(2, sdk.Manifests);
                Assert.AreEqual(0, sdk.LoadCalls);
                sdk.Operations[2].Finish();
                var accepted = rig.Result(retry);
                Assert.IsTrue(accepted.IsAccepted);
                Rejected(failed, Code.ManifestUnavailable, Stage.SelectManifest);
                Assert.AreEqual(1, sdk.PackageInitializations);
                Assert.AreEqual(2, sdk.Manifests);
                Assert.AreEqual(1, sdk.LoadCalls);
                var shared = rig.Result(rig.Request());
                Assert.IsTrue(shared.IsAccepted);
                Assert.AreEqual(2, sdk.Manifests);
                Assert.AreEqual(1, sdk.LoadCalls);
                accepted.Lease.Dispose();
                shared.Lease.Dispose();
            }
        }

        [Test]
        public void PostLoadValidationFailureRemainsStableAcrossLaterAttempts()
        {
            foreach (var throws in new[] { false, true })
            using (var sdk = new Sdk())
            using (var first = new Rig(sdk))
            using (var early = new Rig(sdk))
            using (var late = new Rig(sdk))
            {
                sdk.AutoManifest = false;
                sdk.WrongVersion = !throws;
                sdk.ThrowVersion = throws;
                var failed = first.Request();
                var earlyObserver = early.Request();
                var lateObserver = late.Request();
                Assert.AreEqual(1, sdk.Manifests);
                sdk.Operations[1].Finish();
                first.Dispatch.Run();
                Rejected(failed, Code.ManifestUnavailable, Stage.SelectManifest);
                Assert.IsFalse(earlyObserver.IsCompleted);
                Assert.IsFalse(lateObserver.IsCompleted);
                sdk.WrongVersion = false;
                sdk.ThrowVersion = false;
                early.Dispatch.Run();
                Rejected(earlyObserver, Code.ManifestUnavailable, Stage.SelectManifest);
                Assert.AreEqual(1, sdk.Manifests);
                Assert.AreEqual(0, sdk.LoadCalls);
                var retry = first.Request();
                first.Dispatch.Run();
                Assert.IsFalse(retry.IsCompleted);
                Assert.IsFalse(lateObserver.IsCompleted);
                Assert.AreEqual(1, sdk.PackageInitializations);
                Assert.AreEqual(2, sdk.Manifests);
                Assert.AreEqual(0, sdk.LoadCalls);
                sdk.Operations[2].Finish();
                var accepted = first.Result(retry);
                Assert.IsTrue(accepted.IsAccepted);
                late.Dispatch.Run();
                Rejected(lateObserver, Code.ManifestUnavailable, Stage.SelectManifest);
                Rejected(earlyObserver, Code.ManifestUnavailable, Stage.SelectManifest);
                Rejected(failed, Code.ManifestUnavailable, Stage.SelectManifest);
                Assert.AreEqual(1, sdk.PackageInitializations);
                Assert.AreEqual(2, sdk.Manifests);
                Assert.AreEqual(1, sdk.LoadCalls);
                accepted.Lease.Dispose();
                first.Dispatch.Run();
                early.Dispatch.Run();
                late.Dispatch.Run();
                var firstClose = first.Provider.CloseAsync();
                var earlyClose = early.Provider.CloseAsync();
                var lateClose = late.Provider.CloseAsync();
                first.Dispatch.Run();
                early.Dispatch.Run();
                late.Dispatch.Run();
                Assert.IsTrue(firstClose.IsCompleted);
                Assert.IsTrue(earlyClose.IsCompleted);
                Assert.IsTrue(lateClose.IsCompleted);
            }
        }

        [Test]
        public void B17_B18_AdapterHonorsIdentifierEpochAndBudgetBoundariesWithoutLeakingInputs()
        {
            foreach (var length in new[] { 1, 127, 128 })
            using (var rig = new Rig(null, null, Map(new string('a', length), new string('b', length))))
            {
                rig.Provider.AdvanceEpoch(long.MaxValue);
                var result = rig.Result(rig.Request(Id(new string('a', length)), new string('b', length),
                    new AssetAcquireBudget(1, 1, 1, 1), long.MaxValue));
                Assert.IsTrue(result.IsAccepted); Assert.AreEqual(long.MaxValue, result.RequestEpoch);
                Assert.That(result.Lease.LeaseId, Does.Match("^[0-9a-f]{32}$"));
            }
            using (var rig = new Rig())
            {
                foreach (var epoch in new[] { long.MinValue, -1L, 0L })
                    Rejected(rig.Request(epoch: epoch), Code.StaleEpoch);
                foreach (var set in new[] { "", new string('a', 129), "a\0b" })
                    Rejected(rig.Request(set: set), Code.WrongReleaseSet);
                Assert.AreEqual(0, rig.Sdk.Initializations);
            }
        }
    }
}
