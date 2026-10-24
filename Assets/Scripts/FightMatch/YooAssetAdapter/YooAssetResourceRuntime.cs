using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FightMatch.AssetAccess;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("FightMatch.Host")]

namespace FightMatch.YooAssetAdapter
{
    // One admitted BuiltIn startup. A failed/cancelled startup needs a newly created runtime.
    internal sealed class YooAssetResourceRuntime : IFightMatchResourceRuntime, IFightMatchAssetProvider
    {
        private readonly string runtimeId = Guid.NewGuid().ToString("N");
        private readonly ResourceAdmissionPlan plan;
        private readonly IAssetDispatcher dispatcher;
        private readonly YooAssetAssetProvider provider;
        private readonly HashSet<IDisposable> leases = new HashSet<IDisposable>();
        private ResourceInspection inspection;
        private bool consumed, verified;
        private int disposed;
        private readonly TaskCompletionSource<bool> stopped = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        internal Task Stopped => stopped.Task;
        public IFightMatchAssetProvider Assets => this;

        internal YooAssetResourceRuntime(FightMatchResourceReleaseSet set, ResourceAdmissionPlan plan,
            string root, IAssetDispatcher dispatcher, IYooSdk sdk)
        {
            this.plan = plan; this.dispatcher = dispatcher;
            var mappings = new List<AssetMapping>();
            foreach (var entry in plan.Mappings)
            {
                FightMatchAssetId.TryCreate(entry.AssetId, out var id);
                if (!AssetMapping.TryFromPinnedRaw(set, id, plan.ReleaseSetId, plan.Platform, root,
                    out var map, plan.Manifest.Length, plan.Manifest.Sha256))
                    throw new ArgumentException("Pinned raw mapping is required.");
                mappings.Add(map);
            }
            provider = new YooAssetAssetProvider(mappings, dispatcher, sdk);
        }

        private void MainThread()
        { if (!dispatcher.IsMain) throw new InvalidOperationException("Unity main thread required."); }
        private static ResourceRuntimeDiagnostic Diagnostic(ResourceRuntimeDiagnosticCode code) =>
            new ResourceRuntimeDiagnostic(code, false, null);

        public Task<ResourceInspection> InspectAsync(long epoch, CancellationToken cancellationToken)
        {
            MainThread();
            if (Volatile.Read(ref disposed) != 0 || cancellationToken.IsCancellationRequested || epoch <= 0 ||
                (inspection != null && inspection.Epoch != epoch))
                return Task.FromResult(ResourceInspection.Rejected(runtimeId, Guid.NewGuid().ToString("N"), epoch,
                    Diagnostic(cancellationToken.IsCancellationRequested ? ResourceRuntimeDiagnosticCode.Stop : ResourceRuntimeDiagnosticCode.Stale)));
            if (inspection == null)
            {
                if (epoch > 1) provider.AdvanceEpoch(epoch);
                inspection = ResourceInspection.Accepted(runtimeId, Guid.NewGuid().ToString("N"), plan.ReleaseSetId,
                    epoch, 0, 0, false, ResourceNetworkKind.Offline);
            }
            return Task.FromResult(inspection);
        }

        public async Task<ResourceTransportResult> PrepareAsync(ResourceDownloadPermit permit,
            IProgress<ResourceProgress> progress, CancellationToken cancellationToken)
        {
            MainThread();
            var operation = Guid.NewGuid().ToString("N");
            if (Volatile.Read(ref disposed) != 0 || consumed || permit == null || !permit.Matches(inspection))
                return ResourceTransportResult.Rejected(runtimeId, operation, permit?.Inspection.Epoch ?? 0,
                    Diagnostic(ResourceRuntimeDiagnosticCode.Stale));
            consumed = true;
            ResourceRuntimeDiagnostic failure = null;
            using (cancellationToken.Register(Dispose))
            {
                Report(progress, operation, ResourceTransportStage.Verifying);
                try
                {
                    var budget = new AssetAcquireBudget(plan.Budget.MaxRawFileBytes, 1, 16, 16);
                    foreach (var mapping in plan.Mappings)
                    {
                        if (Volatile.Read(ref disposed) != 0) break;
                        FightMatchAssetId.TryCreate(mapping.AssetId, out var id);
                        var acquired = await provider.AcquireAsync<FightMatchRawBytes>(id, plan.ReleaseSetId, budget, inspection.Epoch);
                        using (acquired.Lease)
                        {
                            if (!acquired.IsAccepted)
                            {
                                failure = new ResourceRuntimeDiagnostic(ResourceRuntimeDiagnosticCode.Asset,
                                    acquired.Diagnostic.Retryable, acquired.Diagnostic);
                                break;
                            }
                            // The provider verifies the SDK manifest, exact raw length/hash and package version.
                            if (acquired.Lease.Asset == null)
                            { failure = Diagnostic(ResourceRuntimeDiagnosticCode.Hash); break; }
                        }
                    }
                }
                catch (Exception) { failure = Diagnostic(ResourceRuntimeDiagnosticCode.Storage); }
                if (failure != null || Volatile.Read(ref disposed) != 0 || cancellationToken.IsCancellationRequested)
                {
                    Dispose();
                    Report(progress, operation, ResourceTransportStage.StopRequested);
                    // Closing waits for actual SDK operations, raw streams, leases and package destruction.
                    try { await Stopped; }
                    catch (Exception)
                    {
                        var stop = new ResourceRuntimeDiagnostic(ResourceRuntimeDiagnosticCode.Stop, true, null);
                        Report(progress, operation, ResourceTransportStage.FailedRetryable, stop);
                        return ResourceTransportResult.Rejected(runtimeId, operation, inspection.Epoch, stop);
                    }
                    if (cancellationToken.IsCancellationRequested || failure == null)
                    {
                        Report(progress, operation, ResourceTransportStage.Stopped);
                        return ResourceTransportResult.Cancelled(permit, operation);
                    }
                    Report(progress, operation, failure.Retryable ? ResourceTransportStage.FailedRetryable :
                        ResourceTransportStage.FailedTerminal, failure);
                    return ResourceTransportResult.Rejected(runtimeId, operation, inspection.Epoch, failure);
                }
                verified = true;
                Report(progress, operation, ResourceTransportStage.TransportVerified);
                return ResourceTransportResult.Verified(permit, operation);
            }
        }

        private void Report(IProgress<ResourceProgress> progress, string operation, ResourceTransportStage stage,
            ResourceRuntimeDiagnostic diagnostic = null)
        {
            // A UI observer cannot prevent transport cleanup.
            try { progress?.Report(new ResourceProgress(inspection, operation, 0, 0, ResourceNetworkKind.Offline, stage, diagnostic)); }
            catch (Exception) { }
        }

        public async Task<FightMatchAssetAcquireResult<T>> AcquireAsync<T>(FightMatchAssetId id, string set,
            AssetAcquireBudget budget, long epoch) where T : class
        {
            MainThread();
            if (!verified || Volatile.Read(ref disposed) != 0 || inspection == null || epoch != inspection.Epoch || set != plan.ReleaseSetId)
                return Refuse<T>(id, set, epoch);
            var acquired = await provider.AcquireAsync<T>(id, set, budget, epoch);
            if (!acquired.IsAccepted) return acquired;
            if (Volatile.Read(ref disposed) != 0)
            { acquired.Lease.Dispose(); return Refuse<T>(id, set, epoch); }
            var lease = new RuntimeLease<T>(this, acquired.Lease);
            leases.Add(lease);
            return FightMatchAssetAcquireResult<T>.Accepted(lease, epoch, set);
        }

        private FightMatchAssetAcquireResult<T> Refuse<T>(FightMatchAssetId id, string set, long epoch) where T : class
        {
            var validSet = AssetMapping.ValidSet(set) ? set : null;
            var code = id == null ? FightMatchAssetDiagnosticCode.InvalidAssetId :
                validSet == null ? FightMatchAssetDiagnosticCode.WrongReleaseSet :
                epoch <= 0 || inspection != null && epoch != inspection.Epoch ? FightMatchAssetDiagnosticCode.StaleEpoch :
                set != plan.ReleaseSetId ? FightMatchAssetDiagnosticCode.WrongReleaseSet : FightMatchAssetDiagnosticCode.PackageUnavailable;
            if (id == null) validSet = null;
            var diagnostic = new FightMatchAssetDiagnostic(code, FightMatchAssetDiagnosticStage.ValidateRequest,
                id, validSet, false, "reason=runtime-not-ready");
            return FightMatchAssetAcquireResult<T>.Rejected(diagnostic, epoch, validSet);
        }

        public void Dispose()
        {
            Interlocked.Exchange(ref disposed, 1);
            // Invalidation is one-way, but unfinished cleanup may need another explicit CloseAsync.
            if (stopped.Task.Status == TaskStatus.RanToCompletion) return;
            if (dispatcher.IsMain) Close();
            else dispatcher.Post(Close);
        }
        private async void Close()
        {
            if (stopped.Task.Status == TaskStatus.RanToCompletion) return;
            try
            {
                verified = false;
                foreach (var lease in leases.ToArray()) lease.Dispose();
                await provider.CloseAsync();
                stopped.TrySetResult(true);
            }
            catch (Exception error) { stopped.TrySetException(error); }
        }
        private sealed class RuntimeLease<T> : IFightMatchAssetLease<T> where T : class
        {
            private readonly YooAssetResourceRuntime owner;
            private readonly IFightMatchAssetLease<T> inner;
            internal RuntimeLease(YooAssetResourceRuntime owner, IFightMatchAssetLease<T> inner)
            { this.owner = owner; this.inner = inner; }
            public FightMatchAssetId AssetId => inner.AssetId;
            public string ReleaseSetId => inner.ReleaseSetId;
            public string LeaseId => inner.LeaseId;
            public bool IsReleased => inner.IsReleased;
            public T Asset => Volatile.Read(ref owner.disposed) == 0 ? inner.Asset : null;
            public void Dispose()
            {
                inner.Dispose();
                if (owner.dispatcher.IsMain) owner.leases.Remove(this);
                else owner.dispatcher.Post(() => owner.leases.Remove(this));
            }
        }
    }
}
