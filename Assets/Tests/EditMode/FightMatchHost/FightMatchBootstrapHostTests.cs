using System;
using System.Collections;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FightMatch.AssetAccess;
using FightMatch.Content;
using FightMatch.Platform;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace FightMatch.Host.Tests
{
    public sealed class FightMatchBootstrapHostTests
    {
        [UnityTest]
        public IEnumerator BH01_BridgeWaitsForVerifiedTransportAndUsesInspectionEpoch()
        {
            var runtime = new FixtureRuntime(); var controller = new FightMatchBootstrapController();
            var bridge = new TaskCompletionSource<FightMatchResourceBridgeResult>();
            var bridgeCalls = 0; long hostEpoch = 71;
            try
            {
                var running = FightMatchPlayerHost.OpenBootstrapAsync(controller, runtime, runtime.Stopped, runtime.Set,
                    () => hostEpoch == 71, CancellationToken.None, (bound, epoch, token) =>
                    {
                        bridgeCalls++; Assert.AreSame(runtime, bound);
                        Assert.AreEqual(runtime.Inspection.Epoch, epoch); Assert.AreNotEqual(hostEpoch, epoch);
                        Assert.AreEqual(runtime.Identity, controller.Snapshot.Inspection.RuntimeInstanceId);
                        Assert.IsNull(controller.Snapshot.AdmissionAccepted);
                        return bridge.Task;
                    });
                Assert.AreEqual(0, bridgeCalls); Assert.IsFalse(running.IsCompleted);
                runtime.Verified(); yield return Until(() => bridgeCalls == 1);
                Assert.IsFalse(running.IsCompleted); Assert.IsNull(controller.Snapshot.AdmissionAccepted);
                var ready = DataReady(runtime.Set); bridge.SetResult(ready); yield return Settle(running);
                Assert.AreSame(ready, running.Result); Assert.AreEqual(true, controller.Snapshot.AdmissionAccepted);
                Assert.AreEqual(ResourceTransportStage.TransportVerified, controller.Snapshot.Stage);
                Assert.AreEqual(1, bridgeCalls);
            }
            finally { FightMatchPlayerHost.DisposeBootstrap(controller, runtime); }
            Assert.AreEqual(1, runtime.ReleaseEffects);
        }

        [UnityTest]
        public IEnumerator BH02_WrongTransportIdentityOrTargetSetNeverEntersBridge()
        {
            foreach (var wrongIdentity in new[] { true, false })
            {
                var runtime = new FixtureRuntime(); var controller = new FightMatchBootstrapController(); var calls = 0;
                try
                {
                    var running = FightMatchPlayerHost.OpenBootstrapAsync(controller, runtime, runtime.Stopped,
                        wrongIdentity ? runtime.Set : "fm.other.set", () => true, CancellationToken.None,
                        (bound, epoch, token) => { calls++; return Task.FromResult(DataReady(runtime.Set)); });
                    if (wrongIdentity)
                        runtime.Preparation.SetResult(ResourceTransportResult.Rejected(Guid.NewGuid().ToString("N"), runtime.Operation,
                            runtime.Inspection.Epoch, new ResourceRuntimeDiagnostic(ResourceRuntimeDiagnosticCode.Network, true, null)));
                    else runtime.Verified();
                    yield return Settle(running);
                    Assert.AreEqual(0, calls); Assert.IsFalse(running.Result.IsDataReady);
                    Assert.AreEqual(wrongIdentity ? "RES_STOP" : "RES_STALE", running.Result.DiagnosticCode);
                    Assert.IsNull(controller.Snapshot.AdmissionAccepted);
                }
                finally { FightMatchPlayerHost.DisposeBootstrap(controller, runtime); }
            }
        }

        [UnityTest]
        public IEnumerator BH03_ExternalCancellationReachesRuntimeAndRejectsLateTransport()
        {
            var runtime = new FixtureRuntime(); var controller = new FightMatchBootstrapController(); var calls = 0;
            using (var cancellation = new CancellationTokenSource())
            try
            {
                var running = FightMatchPlayerHost.OpenBootstrapAsync(controller, runtime, runtime.Stopped, runtime.Set,
                    () => true, cancellation.Token,
                    (bound, epoch, token) => { calls++; return Task.FromResult(DataReady(runtime.Set)); });
                cancellation.Cancel();
                Assert.IsTrue(runtime.Token.IsCancellationRequested); Assert.IsFalse(running.IsCompleted);
                Assert.AreEqual(ResourceTransportStage.StopRequested, controller.Snapshot.Stage);
                runtime.Verified(); yield return Settle(running);
                Assert.AreEqual(0, calls); Assert.AreEqual("RES_STOP", running.Result.DiagnosticCode);
                Assert.IsFalse(running.Result.IsDataReady); Assert.IsNull(controller.Snapshot.AdmissionAccepted);
                Assert.AreEqual(ResourceTransportStage.StopRequested, controller.Snapshot.Stage);
            }
            finally { FightMatchPlayerHost.DisposeBootstrap(controller, runtime); }
        }

        [UnityTest]
        public IEnumerator BH04_CancellationOrOldHostRequestDiscardsLateBridgeDataReady()
        {
            foreach (var cancelToken in new[] { true, false })
            {
                var runtime = new FixtureRuntime(); var controller = new FightMatchBootstrapController();
                var bridge = new TaskCompletionSource<FightMatchResourceBridgeResult>();
                var calls = 0; long hostEpoch = 31; CancellationToken bridgeToken = default;
                using (var cancellation = new CancellationTokenSource())
                try
                {
                    var running = FightMatchPlayerHost.OpenBootstrapAsync(controller, runtime, runtime.Stopped, runtime.Set,
                        () => hostEpoch == 31, cancellation.Token, (bound, epoch, token) =>
                        { calls++; bridgeToken = token; return bridge.Task; });
                    runtime.Verified(); yield return Until(() => calls == 1);
                    if (cancelToken) cancellation.Cancel(); else hostEpoch++;
                    Assert.AreEqual(cancelToken, bridgeToken.IsCancellationRequested);
                    bridge.SetResult(DataReady(runtime.Set)); yield return Settle(running);
                    Assert.IsFalse(running.Result.IsDataReady); Assert.AreEqual("RES_STOP", running.Result.DiagnosticCode);
                    Assert.IsNull(controller.Snapshot.AdmissionAccepted);
                }
                finally { FightMatchPlayerHost.DisposeBootstrap(controller, runtime); }
            }
        }

        [UnityTest]
        public IEnumerator BH05_TransportFailureAndMissingConsentKeepSafeDiagnostics()
        {
            foreach (var mobile in new[] { false, true })
            {
                var runtime = new FixtureRuntime { Mobile = mobile }; var controller = new FightMatchBootstrapController();
                var calls = 0;
                try
                {
                    var running = FightMatchPlayerHost.OpenBootstrapAsync(controller, runtime, runtime.Stopped, runtime.Set,
                        () => true, CancellationToken.None,
                        (bound, epoch, token) => { calls++; return Task.FromResult(DataReady(runtime.Set)); });
                    if (!mobile) runtime.Preparation.SetResult(ResourceTransportResult.Rejected(runtime.Identity, runtime.Operation,
                        runtime.Inspection.Epoch, new ResourceRuntimeDiagnostic(ResourceRuntimeDiagnosticCode.Hash, false, null)));
                    yield return Settle(running);
                    Assert.AreEqual(0, calls); Assert.AreEqual(mobile ? 0 : 1, runtime.PrepareCalls);
                    Assert.IsFalse(running.Result.IsDataReady);
                    Assert.AreEqual(mobile ? "RES_CONSENT" : "RES_HASH", running.Result.DiagnosticCode);
                    Assert.IsNull(controller.Snapshot.AdmissionAccepted);
                }
                finally { FightMatchPlayerHost.DisposeBootstrap(controller, runtime); }
            }
        }

        [UnityTest]
        public IEnumerator BH06_BridgeFailureAndWrongSetRejectAdmissionAndUnsubscribe()
        {
            foreach (var wrongSet in new[] { false, true })
            {
                var runtime = new FixtureRuntime(); var controller = new FightMatchBootstrapController(); var currentCalls = 0;
                var next = new FixtureRuntime();
                try
                {
                    var running = FightMatchPlayerHost.OpenBootstrapAsync(controller, runtime, runtime.Stopped, runtime.Set,
                        () => { currentCalls++; return true; }, CancellationToken.None,
                        (bound, epoch, token) => Task.FromResult(wrongSet ? DataReady("fm.other.set") :
                            new FightMatchResourceBridgeResult(null, null, null, "RES_TEXT")));
                    runtime.Verified(); yield return Settle(running);
                    Assert.IsFalse(running.Result.IsDataReady);
                    Assert.AreEqual(wrongSet ? "RES_STALE" : "RES_TEXT", running.Result.DiagnosticCode);
                    Assert.AreEqual(false, controller.Snapshot.AdmissionAccepted);
                    Assert.AreEqual(running.Result.DiagnosticCode, controller.Snapshot.SafeCode);
                    var callsAfterCompletion = currentCalls;
                    controller.Cancel(); Assert.IsTrue(controller.Bind(next, next.Stopped));
                    var nextRun = controller.StartAsync(); next.Verified(); yield return Settle(nextRun);
                    Assert.AreEqual(callsAfterCompletion, currentCalls, "The completed Host observer remained subscribed.");
                    Assert.IsNull(controller.Snapshot.AdmissionAccepted);
                }
                finally
                {
                    FightMatchPlayerHost.DisposeBootstrap(controller, next);
                    runtime.Dispose();
                }
            }
        }

        [UnityTest]
        public IEnumerator BH07_FinalReleaseFailureCannotReturnDataReadyEvenIfRetrySucceeds()
        {
            foreach (var failures in new[] { 1, 2 })
            {
                var runtime = new FixtureRuntime { DisposeFailures = failures }; var controller = new FightMatchBootstrapController();
                try
                {
                    var running = OpenAndRelease(controller, runtime);
                    runtime.Verified(); yield return Until(() => running.IsCompleted);
                    Assert.IsTrue(running.IsFaulted); Assert.IsFalse(running.IsCanceled);
                    Assert.IsInstanceOf<IOException>(running.Exception.InnerException);
                    Assert.AreEqual(2, runtime.DisposeCalls);
                    Assert.AreEqual(failures == 1 ? 1 : 0, runtime.ReleaseEffects);
                    Assert.AreEqual(ResourceTransportStage.StopRequested, controller.Snapshot.Stage);
                }
                finally { FightMatchPlayerHost.DisposeBootstrap(controller, runtime); }
                Assert.AreEqual(1, runtime.ReleaseEffects);
            }
        }

        [UnityTest]
        public IEnumerator BH08_OldAttemptCleanupDoesNotReleaseNextRuntime()
        {
            var oldRuntime = new FixtureRuntime(); var oldController = new FightMatchBootstrapController();
            var nextRuntime = new FixtureRuntime(); var nextController = new FightMatchBootstrapController();
            var oldBridge = new TaskCompletionSource<FightMatchResourceBridgeResult>(); var oldCurrent = true; var oldCalls = 0;
            try
            {
                var oldRun = FightMatchPlayerHost.OpenBootstrapAsync(oldController, oldRuntime, oldRuntime.Stopped, oldRuntime.Set,
                    () => oldCurrent, CancellationToken.None, (bound, epoch, token) => { oldCalls++; return oldBridge.Task; });
                oldRuntime.Verified(); yield return Until(() => oldCalls == 1); oldCurrent = false;
                var nextRun = FightMatchPlayerHost.OpenBootstrapAsync(nextController, nextRuntime, nextRuntime.Stopped, nextRuntime.Set,
                    () => true, CancellationToken.None, (bound, epoch, token) => Task.FromResult(DataReady(nextRuntime.Set)));
                oldBridge.SetResult(DataReady(oldRuntime.Set)); yield return Settle(oldRun);
                FightMatchPlayerHost.DisposeBootstrap(oldController, oldRuntime);
                Assert.AreEqual("RES_STOP", oldRun.Result.DiagnosticCode); Assert.AreEqual(0, nextRuntime.DisposeCalls);
                Assert.IsFalse(nextRuntime.Token.IsCancellationRequested);
                nextRuntime.Verified(); yield return Settle(nextRun); Assert.IsTrue(nextRun.Result.IsDataReady);
            }
            finally
            {
                FightMatchPlayerHost.DisposeBootstrap(oldController, oldRuntime);
                FightMatchPlayerHost.DisposeBootstrap(nextController, nextRuntime);
            }
        }

        [UnityTest]
        public IEnumerator BH09_FinalCloseWaitsThroughLateFailureAndExplicitRetryAndRejectsStaleSuccess()
        {
            foreach (var invalidation in new[] { "none", "cancel", "epoch" })
            using (var cancellation = new CancellationTokenSource())
            {
                var runtime = new FixtureRuntime { HoldClose = true }; var controller = new FightMatchBootstrapController();
                var next = new FixtureRuntime(); var current = true;
                try
                {
                    var running = OpenAndRelease(controller, runtime, () => current, cancellation.Token);
                    runtime.Verified(); yield return Until(() => runtime.ClosePending);
                    Assert.IsFalse(running.IsCompleted); Assert.AreEqual(0, runtime.ReleaseEffects);
                    Assert.AreEqual(ResourceTransportStage.StopRequested, controller.Snapshot.Stage);
                    Assert.IsFalse(controller.Bind(next, next.Stopped));
                    var requests = runtime.DisposeCalls;
                    runtime.FailClose(); yield return null;
                    Assert.AreEqual(requests, runtime.DisposeCalls, "Late failure must not cause an automatic retry.");
                    Assert.IsFalse(running.IsCompleted); Assert.IsFalse(runtime.Stopped.IsCompleted);
                    if (invalidation == "cancel") cancellation.Cancel();
                    if (invalidation == "epoch") current = false;
                    FightMatchPlayerHost.DisposeBootstrap(controller, runtime);
                    Assert.AreEqual(2, runtime.CloseAttempts); Assert.IsFalse(running.IsCompleted);
                    Assert.AreEqual(0, next.DisposeCalls); Assert.AreEqual(0, runtime.ReleaseEffects);
                    runtime.CompleteClose(); yield return Settle(running);
                    Assert.AreEqual(invalidation == "none", running.Result.IsDataReady);
                    if (invalidation != "none") Assert.AreEqual("RES_STOP", running.Result.DiagnosticCode);
                    Assert.AreEqual(1, runtime.ReleaseEffects);
                }
                finally
                {
                    runtime.HoldClose = false; FightMatchPlayerHost.DisposeBootstrap(controller, runtime);
                    next.Dispose();
                }
            }
        }

        private static async Task<FightMatchResourceBridgeResult> OpenAndRelease(FightMatchBootstrapController controller,
            FixtureRuntime runtime, Func<bool> isCurrent = null, CancellationToken cancellationToken = default)
        {
            var current = isCurrent ?? (() => true);
            var result = await FightMatchPlayerHost.OpenBootstrapAsync(controller, runtime, runtime.Stopped, runtime.Set, current,
                cancellationToken, (bound, epoch, token) => Task.FromResult(DataReady(runtime.Set)));
            return await FightMatchPlayerHost.FinishBootstrapAsync(controller, runtime, runtime.Stopped, result, current, cancellationToken);
        }

        private static IEnumerator Until(Func<bool> complete)
        {
            for (var frame = 0; frame < 8 && !complete(); frame++) yield return null;
            Assert.IsTrue(complete(), "The controlled completion did not reach the Host orchestration.");
        }
        private static IEnumerator Settle(Task task)
        {
            yield return Until(() => task.IsCompleted);
            Assert.IsFalse(task.IsFaulted, task.Exception?.ToString()); Assert.IsFalse(task.IsCanceled);
        }
        private static FightMatchResourceBridgeResult DataReady(string set) => new FightMatchResourceBridgeResult(
            new PublishedContentCatalog(new UnusedContentStorage(), ContentConsumerCapabilities.Current),
            new HostMemoryTextSource(Array.Empty<HostMemoryTextSource.Entry>()), set, null);

        // A typed DataReady sentinel only: these tests do not parse content or create a Session.
        private sealed class UnusedContentStorage : IContentPublicationStorage
        {
            public IDisposable AcquireWriter() => throw new InvalidOperationException("No business storage access expected.");
            public byte[] Read(string key, int maxBytes) => throw new InvalidOperationException("No business storage access expected.");
            public void WriteImmutable(string key, byte[] bytes, int maxBytes) => throw new InvalidOperationException("No business storage access expected.");
        }
        private sealed class FixtureRuntime : IFightMatchResourceRuntime
        {
            internal readonly string Identity = Guid.NewGuid().ToString("N"), Set = "fm.bootstrap.host";
            internal readonly string Operation = Guid.NewGuid().ToString("N");
            internal ResourceInspection Inspection;
            internal ResourceDownloadPermit Permit;
            internal CancellationToken Token;
            internal bool Mobile, HoldClose, ClosePending;
            internal int CloseAttempts;
            internal readonly TaskCompletionSource<bool> CloseCompletion = new TaskCompletionSource<bool>();
            internal Task Stopped => CloseCompletion.Task;
            internal int PrepareCalls, DisposeCalls, ReleaseEffects, DisposeFailures;
            internal readonly TaskCompletionSource<ResourceTransportResult> Preparation = new TaskCompletionSource<ResourceTransportResult>();
            public IFightMatchAssetProvider Assets => throw new InvalidOperationException("The Bridge is controlled by this fixture.");
            public Task<ResourceInspection> InspectAsync(long epoch, CancellationToken cancellationToken)
            {
                Inspection = ResourceInspection.Accepted(Identity, Guid.NewGuid().ToString("N"), Set, epoch,
                    Mobile ? 1 : 0, Mobile ? 1 : 0, Mobile, Mobile ? ResourceNetworkKind.Mobile : ResourceNetworkKind.Offline);
                return Task.FromResult(Inspection);
            }
            public Task<ResourceTransportResult> PrepareAsync(ResourceDownloadPermit permit, IProgress<ResourceProgress> progress,
                CancellationToken cancellationToken)
            { PrepareCalls++; Permit = permit; Token = cancellationToken; return Preparation.Task; }
            internal void Verified() => Preparation.SetResult(ResourceTransportResult.Verified(Permit, Operation));
            public void Dispose()
            {
                DisposeCalls++;
                if (DisposeCalls <= DisposeFailures) throw new IOException("Controlled release failure.");
                if (!HoldClose) CompleteClose();
                else if (!Stopped.IsCompleted && !ClosePending) { ClosePending = true; CloseAttempts++; }
            }
            internal void FailClose() { Assert.IsTrue(ClosePending); ClosePending = false; }
            internal void CompleteClose()
            {
                if (ReleaseEffects == 0) ReleaseEffects++;
                ClosePending = false; CloseCompletion.TrySetResult(true);
            }
        }
    }
}
