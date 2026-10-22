using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FightMatch.AssetAccess;

namespace FightMatch.Host
{
    internal sealed class FightMatchBootstrapSnapshot
    {
        internal long Epoch { get; }
        internal ResourceInspection Inspection { get; }
        internal ResourceProgress Progress { get; }
        internal ResourceNetworkKind NetworkKind { get; }
        internal ResourceTransportStage Stage { get; }
        internal ResourceRuntimeDiagnostic Diagnostic { get; }
        internal bool? AdmissionAccepted { get; }
        internal string SafeCode { get; }
        internal FightMatchBootstrapSnapshot(long epoch, ResourceInspection inspection, ResourceProgress progress,
            ResourceNetworkKind network, ResourceTransportStage stage, ResourceRuntimeDiagnostic diagnostic,
            bool? admissionAccepted = null, string admissionCode = null)
        {
            Epoch = epoch; Inspection = inspection; Progress = progress; NetworkKind = network;
            Stage = stage; Diagnostic = diagnostic; AdmissionAccepted = admissionAccepted;
            SafeCode = diagnostic?.SafeCode ?? admissionCode ?? "";
        }
    }

    // The owner serializes calls and runtime callbacks on the Unity main thread. No Unity API is used here.
    // A successful Bind transfers ownership. A rejected Bind leaves ownership with the caller.
    internal sealed class FightMatchBootstrapController : IDisposable
    {
        private sealed class Attempt
        {
            internal readonly IFightMatchResourceRuntime Runtime;
            internal readonly long Epoch;
            internal readonly CancellationTokenSource Cancellation = new CancellationTokenSource();
            internal ResourceInspection Inspection;
            internal ResourceDownloadPermit Permit;
            internal ResourceProgress Progress;
            internal string OperationId;
            internal bool Started, Pending, Preparing, Invalidated, StopProven, Released, CancelRequested;
            internal bool TransportVerified, AdmissionCompleted, CancellationDisposed;
            internal Attempt(IFightMatchResourceRuntime runtime, long epoch) { Runtime = runtime; Epoch = epoch; }
        }

        private sealed class ProgressSink : IProgress<ResourceProgress>
        {
            private readonly FightMatchBootstrapController owner;
            private readonly Attempt attempt;
            internal ProgressSink(FightMatchBootstrapController owner, Attempt attempt)
            { this.owner = owner; this.attempt = attempt; }
            public void Report(ResourceProgress value) => owner.ReceiveProgress(attempt, value);
        }

        private readonly HashSet<string> runtimeIdentities = new HashSet<string>(StringComparer.Ordinal);
        private Attempt current;
        private string targetSet;
        private long epoch;
        private bool focused = true, disposed;
        private ResourceNetworkKind? observedNetwork;
        internal FightMatchBootstrapSnapshot Snapshot { get; private set; }
        internal event Action<string, string, long> TransportVerified;

        internal bool Bind(IFightMatchResourceRuntime runtime)
        {
            if (runtime == null) throw new ArgumentNullException(nameof(runtime));
            if (disposed || epoch == long.MaxValue) return false;
            if (current != null && (ReferenceEquals(current.Runtime, runtime) || current.Pending ||
                !current.Invalidated || !current.StopProven || !Release(current))) return false;
            current = new Attempt(runtime, ++epoch);
            Set(current, ResourceTransportStage.Inspecting);
            return true;
        }

        internal Task StartAsync()
        {
            var attempt = current;
            if (!Live(attempt) || !focused || attempt.Started) return Task.CompletedTask;
            attempt.Started = true;
            attempt.Pending = true;
            return InspectAsync(attempt);
        }

        private async Task InspectAsync(Attempt attempt)
        {
            ResourceInspection inspection;
            try { inspection = await attempt.Runtime.InspectAsync(attempt.Epoch, attempt.Cancellation.Token); }
            catch (Exception) { UnknownTermination(attempt); return; }
            attempt.Pending = false;
            if (attempt.Invalidated)
            {
                // Inspect is local-only; no Prepare has begun. Still wait for its controlled return.
                attempt.StopProven = inspection != null && inspection.Epoch == attempt.Epoch;
                FinishStop(attempt);
                return;
            }
            if (!Live(attempt)) return;
            if (inspection == null || inspection.Epoch != attempt.Epoch)
            { FailBeforeTransport(attempt, Diagnostic(ResourceRuntimeDiagnosticCode.Stale)); return; }
            attempt.Inspection = inspection;
            if (!inspection.IsAccepted)
            { FailBeforeTransport(attempt, inspection.Diagnostic); return; }
            if (!runtimeIdentities.Add(inspection.RuntimeInstanceId) ||
                targetSet != null && targetSet != inspection.ReleaseSetId)
            { FailBeforeTransport(attempt, Diagnostic(ResourceRuntimeDiagnosticCode.Stale)); return; }
            if (targetSet == null) targetSet = inspection.ReleaseSetId;
            if (inspection.RequiresNetwork && observedNetwork.HasValue && observedNetwork.Value != inspection.NetworkKind)
            { Cancel(); return; }
            await PrepareAsync(attempt, false);
        }

        internal Task ConfirmMobileAsync(long expectedEpoch)
        {
            var attempt = current;
            if (!Live(attempt) || !focused || attempt.Epoch != expectedEpoch || attempt.Pending ||
                attempt.Preparing || Snapshot.Stage != ResourceTransportStage.ConsentRequired)
                return Task.CompletedTask;
            return PrepareAsync(attempt, true);
        }

        private async Task PrepareAsync(Attempt attempt, bool mobileConfirmed)
        {
            if (!Live(attempt) || attempt.Preparing) return;
            if (!ResourceDownloadPermit.TryCreate(attempt.Inspection, mobileConfirmed, out var permit, out var diagnostic))
            {
                if (diagnostic.Code == ResourceRuntimeDiagnosticCode.Consent)
                    Set(attempt, ResourceTransportStage.ConsentRequired);
                else FailBeforeTransport(attempt, diagnostic);
                return;
            }
            attempt.Permit = permit;
            attempt.Preparing = true;
            attempt.Pending = true;
            Set(attempt, ResourceTransportStage.Queued);
            ResourceTransportResult result;
            try
            {
                result = await attempt.Runtime.PrepareAsync(permit, new ProgressSink(this, attempt), attempt.Cancellation.Token);
            }
            catch (Exception) { UnknownTermination(attempt); return; }
            attempt.Pending = false;
            var matches = Matches(attempt, result);
            if (attempt.Invalidated)
            {
                // A late success, error, token exception or progress report does not acknowledge this cancellation.
                attempt.StopProven = matches && result.Status == ResourceTransportStatus.Cancelled;
                FinishStop(attempt);
                return;
            }
            if (!Live(attempt)) return;
            if (!matches) { UnknownTermination(attempt); return; }
            attempt.OperationId = result.OperationId;
            if (result.Status == ResourceTransportStatus.Cancelled)
            {
                attempt.Invalidated = true; attempt.StopProven = true;
                FinishStop(attempt);
            }
            else if (result.Status == ResourceTransportStatus.Rejected)
            {
                if (result.Diagnostic.Code == ResourceRuntimeDiagnosticCode.Stop)
                { UnknownTermination(attempt); return; }
                attempt.Invalidated = true; attempt.StopProven = true;
                if (Release(attempt)) Set(attempt, result.Diagnostic.Retryable ? ResourceTransportStage.FailedRetryable :
                    ResourceTransportStage.FailedTerminal, result.Diagnostic);
            }
            else
            {
                attempt.StopProven = true;
                attempt.TransportVerified = true;
                attempt.Progress = new ResourceProgress(attempt.Inspection, result.OperationId,
                    attempt.Inspection.RemainingBytes, attempt.Inspection.RemainingFiles,
                    attempt.Inspection.NetworkKind, ResourceTransportStage.TransportVerified, null);
                Set(attempt, ResourceTransportStage.TransportVerified);
                TransportVerified?.Invoke(result.RuntimeInstanceId, attempt.Inspection.ReleaseSetId, attempt.Epoch);
            }
        }

        private static bool Matches(Attempt attempt, ResourceTransportResult result) =>
            result != null && result.RuntimeInstanceId == attempt.Inspection.RuntimeInstanceId &&
            result.RequestEpoch == attempt.Epoch &&
            (attempt.OperationId == null || attempt.OperationId == result.OperationId) &&
            (result.Status == ResourceTransportStatus.Rejected || result.Permit != null && result.Permit.Matches(attempt.Inspection));

        private void ReceiveProgress(Attempt attempt, ResourceProgress progress)
        {
            if (!Live(attempt) || !attempt.Pending || !attempt.Preparing || progress == null ||
                !attempt.Permit.Matches(progress.Inspection) ||
                attempt.OperationId != null && attempt.OperationId != progress.OperationId) return;
            if (attempt.Inspection.RequiresNetwork && progress.NetworkKind != attempt.Inspection.NetworkKind)
            { NotifyNetwork(progress.NetworkKind); return; }
            attempt.OperationId = progress.OperationId;
            attempt.Progress = progress;
            if (progress.Stage == ResourceTransportStage.StopRequested || progress.Stage == ResourceTransportStage.Stopped)
            { Cancel(); return; }
            // Only the matching operation result may publish TransportVerified.
            Set(attempt, progress.Stage == ResourceTransportStage.TransportVerified ? ResourceTransportStage.Verifying :
                progress.Stage, progress.Diagnostic);
        }

        internal void CompleteAdmission(long expectedEpoch, bool accepted, string safeCode)
        {
            var attempt = current;
            if (!Live(attempt) || attempt.Epoch != expectedEpoch || !attempt.TransportVerified || attempt.AdmissionCompleted) return;
            if (accepted ? !string.IsNullOrEmpty(safeCode) : !AdmissionCode(safeCode))
                throw new ArgumentException("Invalid admission diagnostic.", nameof(safeCode));
            attempt.AdmissionCompleted = true;
            // Admission is a separate fact; this controller never declares BusinessReady.
            Snapshot = new FightMatchBootstrapSnapshot(attempt.Epoch, attempt.Inspection, attempt.Progress,
                observedNetwork ?? attempt.Inspection.NetworkKind, ResourceTransportStage.TransportVerified,
                null, accepted, safeCode);
        }

        private static bool AdmissionCode(string code)
        {
            switch (code)
            {
                case "RES_TRUST": case "RES_SCHEMA": case "RES_HASH": case "RES_RECEIPT":
                case "RES_CONTENT": case "RES_TEXT": case "RES_BUDGET": case "RES_STORAGE":
                case "RES_STOP": case "RES_NETWORK": case "RES_CONSENT": case "RES_STALE": case "RES_ASSET": return true;
                default: return false;
            }
        }

        internal void NotifyNetwork(ResourceNetworkKind network)
        {
            if (!Enum.IsDefined(typeof(ResourceNetworkKind), network)) throw new ArgumentOutOfRangeException(nameof(network));
            if (disposed) return;
            observedNetwork = network;
            if (Live(current) && current.Inspection != null && current.Inspection.RequiresNetwork &&
                !current.TransportVerified && network != current.Inspection.NetworkKind) Cancel();
        }

        internal void NotifyFocus(bool hasFocus)
        {
            if (disposed) return;
            focused = hasFocus;
            if (!hasFocus) Cancel();
        }

        internal void Cancel()
        {
            if (disposed || current == null) return;
            RequestStop(current);
        }

        private void RequestStop(Attempt attempt)
        {
            attempt.Invalidated = true;
            Set(attempt, ResourceTransportStage.StopRequested, !attempt.Pending && attempt.Started &&
                !attempt.StopProven ? Diagnostic(ResourceRuntimeDiagnosticCode.Stop) : null);
            if (!attempt.Pending && (!attempt.Started || !attempt.Preparing && attempt.Inspection != null)) attempt.StopProven = true;
            if (!attempt.CancelRequested && !attempt.CancellationDisposed)
            {
                attempt.CancelRequested = true;
                try { attempt.Cancellation.Cancel(); }
                catch (Exception) { Set(attempt, ResourceTransportStage.StopRequested, Diagnostic(ResourceRuntimeDiagnosticCode.Stop)); }
            }
            if (Release(attempt) && !attempt.Pending && attempt.StopProven)
                Set(attempt, ResourceTransportStage.Stopped);
        }

        private void FinishStop(Attempt attempt)
        {
            if (Release(attempt)) Set(attempt, attempt.StopProven ? ResourceTransportStage.Stopped :
                ResourceTransportStage.StopRequested, attempt.StopProven ? null : Diagnostic(ResourceRuntimeDiagnosticCode.Stop));
        }

        private void UnknownTermination(Attempt attempt)
        {
            attempt.Pending = false;
            attempt.Invalidated = true;
            attempt.StopProven = false;
            if (!attempt.CancelRequested && !attempt.CancellationDisposed)
            {
                attempt.CancelRequested = true;
                try { attempt.Cancellation.Cancel(); }
                catch (Exception) { } // The failure remains visible and cannot authorize another runtime.
            }
            Release(attempt);
            Set(attempt, ResourceTransportStage.StopRequested, Diagnostic(ResourceRuntimeDiagnosticCode.Stop));
        }

        private void FailBeforeTransport(Attempt attempt, ResourceRuntimeDiagnostic diagnostic)
        {
            attempt.Invalidated = true; attempt.StopProven = true;
            if (Release(attempt)) Set(attempt, diagnostic.Retryable ? ResourceTransportStage.FailedRetryable :
                ResourceTransportStage.FailedTerminal, diagnostic);
        }

        private bool Release(Attempt attempt)
        {
            if (!attempt.Released)
            {
                try { attempt.Runtime.Dispose(); attempt.Released = true; }
                catch (Exception)
                {
                    // Keep ownership and allow a later Dispose to retry the failed release.
                    Set(attempt, ResourceTransportStage.StopRequested, Diagnostic(ResourceRuntimeDiagnosticCode.Stop));
                    return false;
                }
            }
            if (!attempt.Pending && !attempt.CancellationDisposed)
            { attempt.Cancellation.Dispose(); attempt.CancellationDisposed = true; }
            return true;
        }

        private bool Live(Attempt attempt) => !disposed && attempt != null && ReferenceEquals(current, attempt) && !attempt.Invalidated;
        private static ResourceRuntimeDiagnostic Diagnostic(ResourceRuntimeDiagnosticCode code) =>
            new ResourceRuntimeDiagnostic(code, code == ResourceRuntimeDiagnosticCode.Stop, null);
        private void Set(Attempt attempt, ResourceTransportStage stage, ResourceRuntimeDiagnostic diagnostic = null)
        {
            if (!ReferenceEquals(current, attempt)) return;
            Snapshot = new FightMatchBootstrapSnapshot(attempt.Epoch, attempt.Inspection, attempt.Progress,
                observedNetwork ?? attempt.Inspection?.NetworkKind ?? ResourceNetworkKind.Unknown, stage, diagnostic);
        }

        public void Dispose()
        {
            disposed = true;
            if (current != null) RequestStop(current);
        }
    }
}
