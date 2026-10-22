using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using FightMatch.AssetAccess;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace FightMatch.Host.Tests
{
    public sealed class FightMatchBootstrapControllerTests
    {
        [UnityTest]
        public IEnumerator BC01_WifiPreparesOnceAndSeparatesTransportFromAdmission()
        {
            var runtime = new FixtureRuntime();
            using (var controller = new FightMatchBootstrapController())
            {
                Assert.IsTrue(controller.Bind(runtime));
                var notifications = 0;
                controller.TransportVerified += (instance, set, epoch) =>
                {
                    notifications++;
                    Assert.AreEqual(runtime.Identity, instance); Assert.AreEqual(runtime.Set, set);
                    Assert.AreEqual(runtime.Inspection.Epoch, epoch);
                };
                var running = controller.StartAsync();
                Assert.AreEqual(1, runtime.InspectCalls); Assert.AreEqual(1, runtime.PrepareCalls);
                Assert.IsFalse(runtime.Permit.MobileConfirmed);
                yield return Settle(controller.StartAsync());
                yield return Settle(controller.ConfirmMobileAsync(controller.Snapshot.Epoch));
                Assert.AreEqual(1, runtime.PrepareCalls); Assert.IsFalse(running.IsCompleted);
                runtime.Report(ResourceTransportStage.Downloading, 20, 0);
                Assert.AreEqual(20, controller.Snapshot.Progress.CompletedBytes);
                runtime.Report(ResourceTransportStage.TransportVerified, 100, 2);
                Assert.AreEqual(0, notifications);
                Assert.AreEqual(ResourceTransportStage.Verifying, controller.Snapshot.Stage);
                runtime.Verified(); yield return Settle(running);
                Assert.AreEqual(1, notifications); Assert.IsNull(controller.Snapshot.AdmissionAccepted);
                Assert.AreEqual(ResourceTransportStage.TransportVerified, controller.Snapshot.Stage);
                controller.CompleteAdmission(controller.Snapshot.Epoch, true, null);
                Assert.AreEqual(true, controller.Snapshot.AdmissionAccepted);
                controller.CompleteAdmission(controller.Snapshot.Epoch, false, "RES_CONTENT");
                Assert.AreEqual(true, controller.Snapshot.AdmissionAccepted);
                Assert.AreEqual(ResourceTransportStage.TransportVerified, controller.Snapshot.Stage);
            }
            Assert.AreEqual(1, runtime.DisposeCalls);
        }

        [UnityTest]
        public IEnumerator BC02_ZeroRemainingWorkPreparesOnEveryNetworkWithoutMobileConsent()
        {
            foreach (ResourceNetworkKind network in Enum.GetValues(typeof(ResourceNetworkKind)))
            {
                var runtime = new FixtureRuntime { Network = network, Bytes = 0, Files = 0 };
                using (var controller = new FightMatchBootstrapController())
                {
                    Assert.IsTrue(controller.Bind(runtime));
                    var running = controller.StartAsync();
                    Assert.AreEqual(1, runtime.PrepareCalls); Assert.IsFalse(runtime.Permit.MobileConfirmed);
                    runtime.Verified(); yield return Settle(running);
                    Assert.AreEqual(ResourceTransportStage.TransportVerified, controller.Snapshot.Stage);
                }
            }
        }

        [UnityTest]
        public IEnumerator BC03_MobileRequiresCurrentEpochConfirmationAndCannotReenter()
        {
            var runtime = new FixtureRuntime { Network = ResourceNetworkKind.Mobile };
            using (var controller = new FightMatchBootstrapController())
            {
                Assert.IsTrue(controller.Bind(runtime)); yield return Settle(controller.StartAsync());
                Assert.AreEqual(ResourceTransportStage.ConsentRequired, controller.Snapshot.Stage);
                Assert.AreEqual(100, controller.Snapshot.Inspection.RemainingBytes);
                Assert.AreEqual(0, runtime.PrepareCalls);
                var epoch = controller.Snapshot.Epoch;
                yield return Settle(controller.ConfirmMobileAsync(epoch + 1));
                Assert.AreEqual(0, runtime.PrepareCalls);
                var running = controller.ConfirmMobileAsync(epoch);
                Assert.IsTrue(runtime.Permit.MobileConfirmed); Assert.AreEqual(1, runtime.PrepareCalls);
                yield return Settle(controller.ConfirmMobileAsync(epoch));
                Assert.AreEqual(1, runtime.PrepareCalls);
                runtime.Verified(); yield return Settle(running);
            }
        }

        [UnityTest]
        public IEnumerator BC04_UnknownAndOfflineMissingWorkNeverPrepare()
        {
            foreach (var network in new[] { ResourceNetworkKind.Unknown, ResourceNetworkKind.Offline })
            {
                var runtime = new FixtureRuntime { Network = network };
                using (var controller = new FightMatchBootstrapController())
                {
                    Assert.IsTrue(controller.Bind(runtime)); yield return Settle(controller.StartAsync());
                    Assert.AreEqual(0, runtime.PrepareCalls); Assert.AreEqual(1, runtime.DisposeCalls);
                    Assert.AreEqual(ResourceTransportStage.FailedRetryable, controller.Snapshot.Stage);
                    Assert.AreEqual("RES_NETWORK", controller.Snapshot.SafeCode);
                }
            }
        }

        [UnityTest]
        public IEnumerator BC05_CancelWaitsForMatchingTerminationBeforeReplacement()
        {
            var runtime = new FixtureRuntime(); var replacement = new FixtureRuntime();
            using (var controller = new FightMatchBootstrapController())
            {
                Assert.IsTrue(controller.Bind(runtime)); var running = controller.StartAsync();
                controller.Cancel(); controller.Cancel();
                Assert.IsTrue(runtime.Token.IsCancellationRequested); Assert.AreEqual(1, runtime.DisposeCalls);
                Assert.AreEqual(ResourceTransportStage.StopRequested, controller.Snapshot.Stage);
                Assert.IsFalse(running.IsCompleted); Assert.IsFalse(controller.Bind(replacement));
                Assert.AreEqual(0, replacement.InspectCalls); Assert.AreEqual(0, replacement.DisposeCalls);
                runtime.Report(ResourceTransportStage.Stopped, 0, 0);
                Assert.AreEqual(ResourceTransportStage.StopRequested, controller.Snapshot.Stage);
                runtime.Cancelled(); yield return Settle(running);
                Assert.AreEqual(ResourceTransportStage.Stopped, controller.Snapshot.Stage);
                Assert.IsFalse(controller.Bind(runtime)); Assert.IsTrue(controller.Bind(replacement));
            }
            Assert.AreEqual(1, replacement.DisposeCalls);
        }

        [UnityTest]
        public IEnumerator BC06_FocusLossAndWifiToMobileInvalidateProgressAndCancel()
        {
            foreach (var focusLoss in new[] { true, false })
            {
                var runtime = new FixtureRuntime();
                using (var controller = new FightMatchBootstrapController())
                {
                    Assert.IsTrue(controller.Bind(runtime)); var running = controller.StartAsync();
                    runtime.Report(ResourceTransportStage.Downloading, 20, 0);
                    if (focusLoss) controller.NotifyFocus(false);
                    else controller.NotifyNetwork(ResourceNetworkKind.Mobile);
                    Assert.IsTrue(runtime.Token.IsCancellationRequested);
                    var stoppedIntent = controller.Snapshot;
                    runtime.Report(ResourceTransportStage.Downloading, 40, 1);
                    controller.CompleteAdmission(stoppedIntent.Epoch, true, null);
                    Assert.AreSame(stoppedIntent, controller.Snapshot);
                    Assert.AreEqual(ResourceTransportStage.StopRequested, controller.Snapshot.Stage);
                    runtime.Cancelled(); yield return Settle(running);
                    controller.NotifyFocus(true);
                    Assert.AreEqual(ResourceTransportStage.Stopped, controller.Snapshot.Stage);
                    Assert.AreEqual(1, runtime.PrepareCalls);
                }
            }
        }

        [UnityTest]
        public IEnumerator BC07_RetryUsesFreshRuntimeEpochInspectionAndMobileConsent()
        {
            var first = new FixtureRuntime { Network = ResourceNetworkKind.Mobile };
            var next = new FixtureRuntime { Network = ResourceNetworkKind.Mobile };
            using (var controller = new FightMatchBootstrapController())
            {
                var notifications = 0;
                controller.TransportVerified += (instance, set, epoch) => { notifications++; Assert.AreEqual(next.Identity, instance); };
                Assert.IsTrue(controller.Bind(first)); yield return Settle(controller.StartAsync());
                var oldEpoch = controller.Snapshot.Epoch;
                var firstRun = controller.ConfirmMobileAsync(oldEpoch);
                controller.Cancel(); first.Cancelled(); yield return Settle(firstRun);
                Assert.IsTrue(controller.Bind(next)); yield return Settle(controller.StartAsync());
                var newEpoch = controller.Snapshot.Epoch;
                Assert.Greater(newEpoch, oldEpoch);
                Assert.AreNotEqual(first.Inspection.InspectionId, next.Inspection.InspectionId);
                Assert.AreEqual(first.Set, next.Inspection.ReleaseSetId);
                Assert.AreEqual(ResourceTransportStage.ConsentRequired, controller.Snapshot.Stage);
                var awaitingConsent = controller.Snapshot;
                first.Report(ResourceTransportStage.TransportVerified, 100, 2);
                controller.CompleteAdmission(oldEpoch, true, null);
                yield return Settle(controller.ConfirmMobileAsync(oldEpoch));
                Assert.AreSame(awaitingConsent, controller.Snapshot); Assert.AreEqual(0, next.PrepareCalls);
                var nextRun = controller.ConfirmMobileAsync(newEpoch);
                Assert.IsTrue(next.Permit.MobileConfirmed);
                next.Verified(); yield return Settle(nextRun);
                Assert.AreEqual(1, notifications); Assert.AreEqual(1, next.PrepareCalls);
            }
        }

        [UnityTest]
        public IEnumerator BC08_RetryRejectsChangedTargetAndReusedRuntimeIdentity()
        {
            var first = new FixtureRuntime();
            using (var controller = new FightMatchBootstrapController())
            {
                Assert.IsTrue(controller.Bind(first)); var running = controller.StartAsync();
                controller.Cancel(); first.Cancelled(); yield return Settle(running);
                var wrongSet = new FixtureRuntime { Set = "fm.bootstrap.other" };
                Assert.IsTrue(controller.Bind(wrongSet)); yield return Settle(controller.StartAsync());
                Assert.AreEqual(0, wrongSet.PrepareCalls); Assert.AreEqual("RES_STALE", controller.Snapshot.SafeCode);
                Assert.AreEqual(1, wrongSet.DisposeCalls);
                var reusedIdentity = new FixtureRuntime { Identity = first.Identity };
                Assert.IsTrue(controller.Bind(reusedIdentity)); yield return Settle(controller.StartAsync());
                Assert.AreEqual(0, reusedIdentity.PrepareCalls); Assert.AreEqual("RES_STALE", controller.Snapshot.SafeCode);
            }
        }

        [UnityTest]
        public IEnumerator BC09_CancellationExceptionCannotProveStopped()
        {
            var runtime = new FixtureRuntime(); var replacement = new FixtureRuntime();
            using (var controller = new FightMatchBootstrapController())
            {
                Assert.IsTrue(controller.Bind(runtime)); var running = controller.StartAsync();
                controller.Cancel(); runtime.Preparation.SetException(new OperationCanceledException());
                yield return Settle(running);
                Assert.AreEqual(ResourceTransportStage.StopRequested, controller.Snapshot.Stage);
                Assert.AreEqual("RES_STOP", controller.Snapshot.SafeCode);
                Assert.IsFalse(controller.Bind(replacement));
                controller.Dispose(); controller.Dispose();
                Assert.AreEqual(ResourceTransportStage.StopRequested, controller.Snapshot.Stage);
                Assert.AreEqual("RES_STOP", controller.Snapshot.SafeCode);
                Assert.AreEqual(1, runtime.DisposeCalls);
            }
            replacement.Dispose();
        }

        [UnityTest]
        public IEnumerator BC10_LateSuccessAfterCancelNeitherSignalsNorProvesStopped()
        {
            var runtime = new FixtureRuntime(); var replacement = new FixtureRuntime();
            using (var controller = new FightMatchBootstrapController())
            {
                var notifications = 0; controller.TransportVerified += (instance, set, epoch) => notifications++;
                Assert.IsTrue(controller.Bind(runtime)); var running = controller.StartAsync();
                controller.Cancel(); runtime.Verified(); yield return Settle(running);
                Assert.AreEqual(0, notifications); Assert.IsFalse(controller.Bind(replacement));
                Assert.AreEqual(ResourceTransportStage.StopRequested, controller.Snapshot.Stage);
            }
            replacement.Dispose();
        }

        [UnityTest]
        public IEnumerator BC11_WrongOperationResultAndProgressCannotAuthorizeAdmission()
        {
            var runtime = new FixtureRuntime();
            using (var controller = new FightMatchBootstrapController())
            {
                Assert.IsTrue(controller.Bind(runtime)); var running = controller.StartAsync();
                runtime.Report(ResourceTransportStage.Downloading, 20, 0);
                var progress = controller.Snapshot;
                var otherOperation = Guid.NewGuid().ToString("N");
                runtime.Observer.Report(new ResourceProgress(runtime.Inspection, otherOperation, 40, 1,
                    ResourceNetworkKind.Wifi, ResourceTransportStage.Downloading, null));
                Assert.AreSame(progress, controller.Snapshot);
                runtime.Preparation.SetResult(ResourceTransportResult.Verified(runtime.Permit, otherOperation));
                yield return Settle(running);
                Assert.IsTrue(runtime.Token.IsCancellationRequested);
                Assert.AreEqual(ResourceTransportStage.StopRequested, controller.Snapshot.Stage);
                controller.CompleteAdmission(controller.Snapshot.Epoch, true, null);
                Assert.IsNull(controller.Snapshot.AdmissionAccepted);
            }
        }

        [UnityTest]
        public IEnumerator BC12_StopProgressDoesNotCancelOrProveTermination()
        {
            var runtime = new FixtureRuntime(); var replacement = new FixtureRuntime();
            using (var controller = new FightMatchBootstrapController())
            {
                Assert.IsTrue(controller.Bind(runtime)); var running = controller.StartAsync();
                foreach (var stage in new[] { ResourceTransportStage.StopRequested, ResourceTransportStage.Stopped })
                {
                    runtime.Report(stage, 0, 0);
                    Assert.IsFalse(runtime.Token.IsCancellationRequested); Assert.IsFalse(running.IsCompleted);
                    Assert.AreEqual(ResourceTransportStage.StopRequested, controller.Snapshot.Stage);
                    Assert.IsFalse(controller.Bind(replacement));
                }
                runtime.Cancelled(); yield return Settle(running);
                Assert.AreEqual(ResourceTransportStage.Stopped, controller.Snapshot.Stage);
                Assert.IsTrue(controller.Bind(replacement));
            }
        }

        [UnityTest]
        public IEnumerator BC13_InspectionCancellationAndNetworkChangeNeverStartStaleWork()
        {
            foreach (var cancel in new[] { true, false })
            {
                var runtime = new FixtureRuntime { HoldInspection = true };
                using (var controller = new FightMatchBootstrapController())
                {
                    Assert.IsTrue(controller.Bind(runtime)); var running = controller.StartAsync();
                    if (cancel) controller.Cancel(); else controller.NotifyNetwork(ResourceNetworkKind.Mobile);
                    Assert.IsFalse(running.IsCompleted); Assert.AreEqual(0, runtime.PrepareCalls);
                    runtime.InspectionCompletion.SetResult(runtime.Inspection); yield return Settle(running);
                    Assert.AreEqual(0, runtime.PrepareCalls);
                    Assert.AreEqual(ResourceTransportStage.Stopped, controller.Snapshot.Stage);
                    Assert.AreEqual(1, runtime.DisposeCalls);
                }
            }
        }

        [Test]
        public void BC14_DisposeRetainsFailedReleaseAndIsIdempotentAfterSuccess()
        {
            var runtime = new FixtureRuntime { FailFirstDispose = true };
            var controller = new FightMatchBootstrapController();
            Assert.IsTrue(controller.Bind(runtime));
            controller.Dispose();
            Assert.AreEqual(1, runtime.DisposeCalls);
            Assert.AreEqual(ResourceTransportStage.StopRequested, controller.Snapshot.Stage);
            Assert.AreEqual("RES_STOP", controller.Snapshot.SafeCode);
            controller.Dispose(); controller.Dispose();
            Assert.AreEqual(2, runtime.DisposeCalls);
            Assert.AreEqual(ResourceTransportStage.Stopped, controller.Snapshot.Stage);
            Assert.IsFalse(controller.Bind(runtime));
        }

        [UnityTest]
        public IEnumerator BC15_AdmissionRejectsUnsafeCodeAndOldEpochCannotOverwriteNewAttempt()
        {
            var first = new FixtureRuntime(); var next = new FixtureRuntime { Network = ResourceNetworkKind.Mobile };
            using (var controller = new FightMatchBootstrapController())
            {
                Assert.IsTrue(controller.Bind(first)); var running = controller.StartAsync();
                first.Verified(); yield return Settle(running);
                var oldEpoch = controller.Snapshot.Epoch;
                Assert.Throws<ArgumentException>(() => controller.CompleteAdmission(oldEpoch, false, "/private/secret"));
                Assert.IsNull(controller.Snapshot.AdmissionAccepted);
                controller.CompleteAdmission(oldEpoch, false, "RES_CONTENT");
                Assert.AreEqual(false, controller.Snapshot.AdmissionAccepted);
                Assert.AreEqual("RES_CONTENT", controller.Snapshot.SafeCode);
                Assert.AreEqual(ResourceTransportStage.TransportVerified, controller.Snapshot.Stage);
                controller.Cancel(); Assert.IsTrue(controller.Bind(next));
                yield return Settle(controller.StartAsync());
                var newState = controller.Snapshot;
                controller.CompleteAdmission(oldEpoch, true, null);
                first.Report(ResourceTransportStage.TransportVerified, 100, 2);
                Assert.AreSame(newState, controller.Snapshot); Assert.AreEqual(0, next.PrepareCalls);
            }
        }

        [UnityTest]
        public IEnumerator BC16_ControlledFailureCanRetryButUnconfirmedStopCannot()
        {
            var first = new FixtureRuntime(); var next = new FixtureRuntime();
            using (var controller = new FightMatchBootstrapController())
            {
                Assert.IsTrue(controller.Bind(first)); var running = controller.StartAsync();
                first.Rejected(ResourceRuntimeDiagnosticCode.Network); yield return Settle(running);
                Assert.AreEqual(ResourceTransportStage.FailedRetryable, controller.Snapshot.Stage);
                Assert.AreEqual(1, first.DisposeCalls); Assert.IsTrue(controller.Bind(next));
                var retry = controller.StartAsync();
                next.Rejected(ResourceRuntimeDiagnosticCode.Stop); yield return Settle(retry);
                Assert.AreEqual(ResourceTransportStage.StopRequested, controller.Snapshot.Stage);
                Assert.IsTrue(next.Token.IsCancellationRequested); Assert.IsFalse(controller.Bind(first));
            }
        }

        [UnityTest]
        public IEnumerator BC17_RuntimeFailureCleanupPreservesOriginalRejectionUntilControlledTermination()
        {
            FightMatchAssetId.TryCreate("fm.content.source", out var assetId);
            var asset = new FightMatchAssetDiagnostic(FightMatchAssetDiagnosticCode.SdkFailure,
                FightMatchAssetDiagnosticStage.Acquire, assetId, "fm.bootstrap.test", true, null);
            var failures = new[]
            {
                new ResourceRuntimeDiagnostic(ResourceRuntimeDiagnosticCode.Asset, true, asset),
                new ResourceRuntimeDiagnostic(ResourceRuntimeDiagnosticCode.Hash, false, null),
                new ResourceRuntimeDiagnostic(ResourceRuntimeDiagnosticCode.Storage, true, null)
            };
            foreach (var failure in failures)
            {
                var runtime = new FixtureRuntime { Network = ResourceNetworkKind.Offline, Bytes = 0, Files = 0 };
                var replacement = new FixtureRuntime();
                using (var controller = new FightMatchBootstrapController())
                {
                    Assert.IsTrue(controller.Bind(runtime)); var running = controller.StartAsync();
                    // YooAssetResourceRuntime closes itself before reporting failure-cleanup intent.
                    runtime.Dispose(); runtime.Report(ResourceTransportStage.StopRequested, 0, 0);
                    Assert.IsFalse(runtime.Token.IsCancellationRequested, "Runtime cleanup is not owner cancellation.");
                    Assert.AreEqual(ResourceTransportStage.StopRequested, controller.Snapshot.Stage);
                    Assert.IsFalse(running.IsCompleted); Assert.IsFalse(controller.Bind(replacement));
                    runtime.CompleteFailureCleanup(failure); yield return Settle(running);
                    Assert.IsFalse(runtime.Token.IsCancellationRequested);
                    Assert.AreSame(failure, controller.Snapshot.Diagnostic);
                    Assert.AreSame(failure.AssetDiagnostic, controller.Snapshot.Diagnostic.AssetDiagnostic);
                    Assert.AreEqual(failure.Retryable ? ResourceTransportStage.FailedRetryable :
                        ResourceTransportStage.FailedTerminal, controller.Snapshot.Stage);
                    Assert.IsTrue(controller.Bind(replacement));
                }
            }
        }

        [UnityTest]
        public IEnumerator BC18_OwnerCancellationDuringRuntimeCleanupStillWaitsForCancelledResult()
        {
            var runtime = new FixtureRuntime { Network = ResourceNetworkKind.Offline, Bytes = 0, Files = 0 };
            var replacement = new FixtureRuntime();
            using (var controller = new FightMatchBootstrapController())
            {
                Assert.IsTrue(controller.Bind(runtime)); var running = controller.StartAsync();
                runtime.Dispose(); runtime.Report(ResourceTransportStage.StopRequested, 0, 0);
                Assert.IsFalse(runtime.Token.IsCancellationRequested);
                controller.Cancel();
                Assert.IsTrue(runtime.Token.IsCancellationRequested); Assert.IsFalse(running.IsCompleted);
                Assert.AreEqual(ResourceTransportStage.StopRequested, controller.Snapshot.Stage);
                Assert.IsFalse(controller.Bind(replacement));
                runtime.CompleteFailureCleanup(new ResourceRuntimeDiagnostic(ResourceRuntimeDiagnosticCode.Hash, false, null));
                yield return Settle(running);
                Assert.AreEqual(ResourceTransportStage.Stopped, controller.Snapshot.Stage);
                Assert.IsNull(controller.Snapshot.Diagnostic); Assert.IsNull(controller.Snapshot.AdmissionAccepted);
                Assert.IsTrue(controller.Bind(replacement));
            }
        }

        private static IEnumerator Settle(Task task)
        {
            for (var frame = 0; frame < 8 && !task.IsCompleted; frame++) yield return null;
            Assert.IsTrue(task.IsCompleted, "The controlled completion did not reach the controller.");
            Assert.IsFalse(task.IsFaulted, task.Exception?.ToString());
            Assert.IsFalse(task.IsCanceled);
        }

        private sealed class FixtureRuntime : IFightMatchResourceRuntime
        {
            internal string Identity = Guid.NewGuid().ToString("N"), Set = "fm.bootstrap.test";
            internal readonly string Operation = Guid.NewGuid().ToString("N");
            internal ResourceNetworkKind Network = ResourceNetworkKind.Wifi;
            internal long Bytes = 100;
            internal int Files = 2, InspectCalls, PrepareCalls, DisposeCalls;
            internal bool HoldInspection, FailFirstDispose;
            internal ResourceInspection Inspection;
            internal ResourceDownloadPermit Permit;
            internal IProgress<ResourceProgress> Observer;
            internal CancellationToken Token;
            internal readonly TaskCompletionSource<ResourceInspection> InspectionCompletion = new TaskCompletionSource<ResourceInspection>();
            internal readonly TaskCompletionSource<ResourceTransportResult> Preparation = new TaskCompletionSource<ResourceTransportResult>();
            public IFightMatchAssetProvider Assets => throw new InvalidOperationException("Bootstrap policy must not acquire assets.");
            public Task<ResourceInspection> InspectAsync(long epoch, CancellationToken cancellationToken)
            {
                InspectCalls++;
                Inspection = ResourceInspection.Accepted(Identity, Guid.NewGuid().ToString("N"), Set, epoch,
                    Bytes, Files, Bytes > 0, Network);
                return HoldInspection ? InspectionCompletion.Task : Task.FromResult(Inspection);
            }
            public Task<ResourceTransportResult> PrepareAsync(ResourceDownloadPermit permit, IProgress<ResourceProgress> progress,
                CancellationToken cancellationToken)
            {
                PrepareCalls++; Permit = permit; Observer = progress; Token = cancellationToken;
                return Preparation.Task;
            }
            internal void Verified() => Preparation.SetResult(ResourceTransportResult.Verified(Permit, Operation));
            internal void Cancelled() => Preparation.SetResult(ResourceTransportResult.Cancelled(Permit, Operation));
            internal void Rejected(ResourceRuntimeDiagnosticCode code) => Preparation.SetResult(ResourceTransportResult.Rejected(
                Identity, Operation, Inspection.Epoch, new ResourceRuntimeDiagnostic(code, true, null)));
            internal void CompleteFailureCleanup(ResourceRuntimeDiagnostic failure)
            {
                // Match the real runtime's post-cleanup branch: cancellation wins only when requested by the owner.
                if (Token.IsCancellationRequested)
                { Report(ResourceTransportStage.Stopped, 0, 0); Cancelled(); }
                else
                {
                    Observer.Report(new ResourceProgress(Inspection, Operation, 0, 0, Network,
                        failure.Retryable ? ResourceTransportStage.FailedRetryable : ResourceTransportStage.FailedTerminal, failure));
                    Preparation.SetResult(ResourceTransportResult.Rejected(Identity, Operation, Inspection.Epoch, failure));
                }
            }
            internal void Report(ResourceTransportStage stage, long bytes, int files) => Observer.Report(
                new ResourceProgress(Inspection, Operation, bytes, files, Network, stage, null));
            public void Dispose()
            {
                DisposeCalls++;
                if (FailFirstDispose && DisposeCalls == 1) throw new InvalidOperationException("Fixture release failure.");
            }
        }
    }
}
