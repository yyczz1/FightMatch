using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FightMatch.AssetAccess;
using Code = FightMatch.AssetAccess.FightMatchAssetDiagnosticCode;
using Stage = FightMatch.AssetAccess.FightMatchAssetDiagnosticStage;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("FightMatch.AssetAccess.Tests")]

namespace FightMatch.YooAssetAdapter
{
    internal interface IAssetDispatcher
    {
        bool IsMain { get; }
        void Post(Action action);
    }

    internal sealed class UnityAssetDispatcher : IAssetDispatcher
    {
        private readonly int thread;
        private readonly SynchronizationContext context;
        internal UnityAssetDispatcher()
        {
            context = SynchronizationContext.Current;
            if (context == null || context.GetType().FullName != "UnityEngine.UnitySynchronizationContext")
                throw new InvalidOperationException("Unity main context required.");
            thread = Thread.CurrentThread.ManagedThreadId;
        }
        public bool IsMain => Thread.CurrentThread.ManagedThreadId == thread;
        public void Post(Action action) => context.Post(_ => action(), null);
    }

    internal sealed class AssetMapping
    {
        internal readonly string Id, Set, Package, Version, Location, Root;
        internal readonly Type Type;
        internal readonly bool Raw;
        internal readonly long? RawBytes;
        internal AssetMapping(string id, string set, string package, string version, string location,
            Type type, string root, bool raw = false, long? rawBytes = null)
        {
            if (!FightMatchAssetId.TryCreate(id, out _) || !ValidSet(set) || string.IsNullOrEmpty(package) ||
                string.IsNullOrEmpty(version) || version == "latest" || string.IsNullOrEmpty(location) ||
                type == null || string.IsNullOrEmpty(root))
                throw new ArgumentException("Invalid immutable mapping.");
            Id = id; Set = set; Package = package; Version = version; Location = location;
            Type = type; Root = root; Raw = raw; RawBytes = rawBytes;
        }
        internal static bool ValidSet(string value) =>
            value != "latest" && FightMatchAssetId.TryCreate(value, out _);
    }

    internal sealed class AssetRequestIdentity
    {
        internal readonly Guid Provider;
        internal readonly long Epoch, Operation;
        internal readonly string Set, Asset;
        internal AssetRequestIdentity(Guid provider, long epoch, long operation, AssetMapping map)
        { Provider = provider; Epoch = epoch; Operation = operation; Set = map.Set; Asset = map.Id; }
    }

    internal sealed class YooAssetAssetProvider : IFightMatchAssetProvider
    {
        private static int globalPending, globalRecords, globalSlots;
        private readonly Guid instance = Guid.NewGuid();
        private readonly Dictionary<string, AssetMapping> mappings;
        private readonly List<Entry> entries = new List<Entry>();
        private readonly List<Reservation> records = new List<Reservation>();
        private readonly IAssetDispatcher dispatcher;
        private readonly YooAssetPackageLifecycle lifecycle;
        private readonly Action<FightMatchAssetDiagnostic> sink;
        private readonly TaskCompletionSource<bool> closed = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        private long epoch = 1, sequence;
        private int posted;
        private bool closing, draining, wakeSlot;
        internal bool GlobalDestroyed => lifecycle.GlobalDestroyed;

        internal YooAssetAssetProvider(IEnumerable<AssetMapping> mappings, Action<FightMatchAssetDiagnostic> sink = null)
            : this(mappings, new UnityAssetDispatcher(), RealYooSdk.Instance, sink) { }

        internal YooAssetAssetProvider(IEnumerable<AssetMapping> mappings, IAssetDispatcher dispatcher,
            IYooSdk sdk, Action<FightMatchAssetDiagnostic> sink = null)
        {
            this.dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            if (!dispatcher.IsMain) throw new InvalidOperationException("Main thread required.");
            this.mappings = new Dictionary<string, AssetMapping>(StringComparer.Ordinal);
            foreach (var map in mappings ?? throw new ArgumentNullException(nameof(mappings))) this.mappings.Add(map.Id, map);
            this.sink = sink;
            lifecycle = new YooAssetPackageLifecycle(sdk, Wake);
        }

        private sealed class Entry
        {
            internal AssetMapping Map;
            internal AssetRequestIdentity Identity;
            internal YooAssetPackageLifecycle.Ticket Ticket;
            internal readonly List<Reservation> Waiters = new List<Reservation>();
            internal object Asset;
            internal int References;
            internal bool Terminal;
        }

        private sealed class Reservation
        {
            internal Entry Entry;
            internal Action<Code?, Stage> Complete;
            internal string LeaseId;
            internal int Released, Reported;
            internal bool Published, Pending = true;
        }

        private sealed class Lease<T> : IFightMatchAssetLease<T> where T : class
        {
            private readonly YooAssetAssetProvider owner;
            private readonly Reservation record;
            internal Lease(YooAssetAssetProvider owner, Reservation record) { this.owner = owner; this.record = record; }
            public FightMatchAssetId AssetId { get; internal set; }
            public string ReleaseSetId => record.Entry.Map.Set;
            public string LeaseId => record.LeaseId;
            public bool IsReleased => Volatile.Read(ref record.Released) != 0;
            public T Asset
            {
                get
                {
                    if (IsReleased)
                    {
                        if (Interlocked.Exchange(ref record.Reported, 1) == 0)
                            owner.Report(new FightMatchAssetDiagnostic(Code.ReleasedLease, Stage.Release, AssetId,
                                ReleaseSetId, false, "reason=released-lease"));
                        return null;
                    }
                    return owner.dispatcher.IsMain ? record.Entry.Asset as T : null;
                }
            }
            public void Dispose()
            {
                if (Interlocked.Exchange(ref record.Released, 1) == 0) owner.Wake();
            }
        }

        private void Report(FightMatchAssetDiagnostic diagnostic)
        {
            try { sink?.Invoke(diagnostic); } catch (Exception) { }
        }

        private static FightMatchAssetAcquireResult<T> Reject<T>(FightMatchAssetId id, string set,
            long requestEpoch, Code code, Stage stage) where T : class =>
            FightMatchAssetAcquireResult<T>.Rejected(new FightMatchAssetDiagnostic(code, stage, id, set,
                code == Code.SdkFailure || code == Code.PackageUnavailable || code == Code.ManifestUnavailable,
                code == Code.WrongReleaseSet && set == null ? "reason=invalid-release-set" : "status=failed"),
                requestEpoch, set);

        public Task<FightMatchAssetAcquireResult<T>> AcquireAsync<T>(FightMatchAssetId assetId,
            string releaseSetId, AssetAcquireBudget budget, long requestEpoch) where T : class
        {
            Code? error = null;
            if (assetId == null) { error = Code.InvalidAssetId; releaseSetId = null; }
            else if (!AssetMapping.ValidSet(releaseSetId)) { error = Code.WrongReleaseSet; releaseSetId = null; }
            else if (budget == null) error = Code.BudgetExceeded;
            else if (requestEpoch <= 0) error = Code.StaleEpoch;
            else if (!mappings.TryGetValue(assetId.Value, out var mapping)) error = Code.UnknownAsset;
            else if (mapping.Set != releaseSetId) error = Code.WrongReleaseSet;
            else if (!typeof(UnityEngine.Object).IsAssignableFrom(typeof(T)) || mapping.Type != typeof(T)) error = Code.WrongAssetType;
            else if (mapping.Raw && (!mapping.RawBytes.HasValue || mapping.RawBytes < 0 || mapping.RawBytes > budget.MaxRawBytes))
                error = Code.BudgetExceeded;
            else if (mapping.Raw) error = Code.WrongAssetType; // Raw DTO/reading belongs to D.
            if (error.HasValue) return Task.FromResult(Reject<T>(assetId, releaseSetId, requestEpoch, error.Value, Stage.ValidateRequest));
            if (!dispatcher.IsMain) return Task.FromResult(Reject<T>(assetId, releaseSetId, requestEpoch, Code.SdkFailure, Stage.ValidateRequest));
            Drain(true);
            if (closing || requestEpoch != epoch)
                return Task.FromResult(Reject<T>(assetId, releaseSetId, requestEpoch, Code.StaleEpoch, Stage.ValidateRequest));
            var extraSlot = !wakeSlot || records.Count != 0 ? 1 : 0;
            if (globalPending >= budget.MaxConcurrentAcquisitions || globalRecords >= budget.MaxRetainedLeases ||
                globalSlots + extraSlot > budget.MaxQueuedCallbacks)
                return Task.FromResult(Reject<T>(assetId, releaseSetId, requestEpoch, Code.BudgetExceeded, Stage.ValidateRequest));
            var map = mappings[assetId.Value];
            var entry = entries.FirstOrDefault(x => x.Identity.Epoch == epoch && ReferenceEquals(x.Map, map) &&
                (!x.Terminal || x.Asset != null));
            if (entry == null)
            {
                entry = new Entry { Map = map, Identity = new AssetRequestIdentity(instance, epoch, ++sequence, map) };
                entries.Add(entry);
            }
            var completion = new TaskCompletionSource<FightMatchAssetAcquireResult<T>>(TaskCreationOptions.RunContinuationsAsynchronously);
            var record = new Reservation { Entry = entry };
            records.Add(record);
            entry.Waiters.Add(record);
            globalPending++; globalRecords++; globalSlots += extraSlot; wakeSlot = true;
            record.Complete = (code, stage) =>
            {
                if (!record.Pending) return;
                record.Pending = false;
                globalPending--;
                if (code.HasValue)
                {
                    completion.TrySetResult(Reject<T>(assetId, releaseSetId, requestEpoch, code.Value, stage));
                    Interlocked.Exchange(ref record.Released, 1);
                }
                else
                {
                    record.LeaseId = Guid.NewGuid().ToString("N");
                    if (records.Any(x => !ReferenceEquals(x, record) && x.LeaseId == record.LeaseId))
                        throw new InvalidOperationException("Lease identity collision.");
                    var lease = new Lease<T>(this, record) { AssetId = assetId };
                    var result = FightMatchAssetAcquireResult<T>.Accepted(lease, requestEpoch, releaseSetId);
                    record.Published = true;
                    entry.References++;
                    completion.TrySetResult(result);
                }
            };
            if (entry.Ticket == null && !entry.Terminal)
            {
                try { entry.Ticket = lifecycle.Begin(map, entry.Identity); }
                catch (YooAssetPackageLifecycle.Failure failure) { Finish(entry, failure.Code, failure.Stage); }
                catch (Exception) { Finish(entry, Code.SdkFailure, Stage.InitializePackage); }
            }
            Drain();
            return completion.Task;
        }

        internal void AdvanceEpoch(long value)
        {
            if (!dispatcher.IsMain || value <= epoch) throw new InvalidOperationException("Monotonic main-thread epoch required.");
            epoch = value;
            Drain();
        }

        internal Task CloseAsync()
        {
            if (!dispatcher.IsMain) throw new InvalidOperationException("Main thread required.");
            closing = true;
            Drain(true);
            return closed.Task;
        }

        private void Wake()
        {
            if (Interlocked.CompareExchange(ref posted, 1, 0) != 0) return;
            try { dispatcher.Post(() => { Interlocked.Exchange(ref posted, 0); Drain(); }); }
            catch (Exception) { Interlocked.Exchange(ref posted, 0); }
        }

        private void Finish(Entry entry, Code? code, Stage stage)
        {
            entry.Terminal = true;
            foreach (var record in entry.Waiters.ToArray())
            {
                try { record.Complete(code, stage); }
                catch (Exception)
                {
                    // Factories and callbacks never escape the project boundary.
                    record.Pending = true;
                    globalPending++;
                    record.Complete(Code.SdkFailure, Stage.ValidateResult);
                }
            }
            entry.Waiters.Clear();
        }

        private void Drain(bool retryCleanup = false)
        {
            if (!dispatcher.IsMain || draining || closed.Task.IsCompleted) return;
            draining = true;
            try
            {
                foreach (var entry in entries.ToArray())
                {
                    if (!entry.Terminal && entry.Ticket != null)
                    {
                        try
                        {
                            if (closing || entry.Identity.Epoch != epoch) entry.Ticket.Invalidate();
                            if (!entry.Ticket.Poll()) continue;
                            var identity = entry.Ticket.Identity;
                            var stale = closing || identity.Provider != instance || identity.Epoch != epoch ||
                                !ReferenceEquals(identity, entry.Identity) || identity.Operation != entry.Identity.Operation ||
                                identity.Set != entry.Map.Set || identity.Asset != entry.Map.Id;
                            var code = stale ? Code.StaleEpoch : entry.Ticket.Error;
                            if (!code.HasValue)
                            {
                                var asset = entry.Ticket.Asset;
                                if (asset == null) code = Code.SdkFailure;
                                else if (!entry.Map.Type.IsInstanceOfType(asset)) code = Code.WrongAssetType;
                                else entry.Asset = asset;
                            }
                            Finish(entry, code, stale || !entry.Ticket.Error.HasValue ? Stage.ValidateResult : entry.Ticket.Stage);
                        }
                        catch (Exception)
                        {
                            if (entry.Ticket.Fail()) Finish(entry, Code.SdkFailure, entry.Ticket.Stage);
                        }
                    }
                    else if (entry.Terminal && entry.Waiters.Count != 0)
                        Finish(entry, closing ? Code.StaleEpoch : (Code?)null, Stage.ValidateResult);
                }
                foreach (var record in records.ToArray())
                {
                    if (record.Pending || Volatile.Read(ref record.Released) == 0) continue;
                    if (record.Published) record.Entry.References--;
                    if (records.Count > 1) globalSlots--;
                    records.Remove(record);
                    globalRecords--;
                }
                foreach (var entry in entries.ToArray())
                {
                    if (!entry.Terminal || entry.References != 0 || entry.Waiters.Count != 0) continue;
                    try
                    {
                        entry.Ticket?.Release();
                        entry.Asset = null;
                        entries.Remove(entry);
                    }
                    catch (Exception) { } // Retain ticket/counts; retry on the next main-thread entry.
                }
                if (closing && records.Count == 0 && entries.Count == 0 && lifecycle.TryClose(retryCleanup))
                {
                    if (wakeSlot) { globalSlots--; wakeSlot = false; }
                    closed.TrySetResult(true);
                }
            }
            catch (Exception) { } // Cleanup remains pending; a later main-thread entry retries.
            finally { draining = false; }
        }
    }
}
