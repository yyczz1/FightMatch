using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace FightMatch.AssetAccess.Tests
{
    public sealed class FightMatchResourceRuntimeContractsTests
    {
        private const string Runtime = "0123456789abcdef0123456789abcdef";
        private const string Inspection = "11111111111111111111111111111111";
        private const string Operation = "22222222222222222222222222222222";
        private const string Other = "33333333333333333333333333333333";
        private static readonly string[] InvalidIds =
        {
            null, "", new string('a', 31), new string('a', 33), Runtime.ToUpperInvariant(),
            new string('g', 32), "private-secret", new string('\u4e2d', 32), new string('a', 31) + "\n"
        };
        private static ResourceRuntimeDiagnostic Diagnostic(ResourceRuntimeDiagnosticCode code = ResourceRuntimeDiagnosticCode.Schema,
            bool retryable = false) => new ResourceRuntimeDiagnostic(code, retryable, null);
        private static ResourceInspection Inspect(string runtime = Runtime, string inspection = Inspection, string set = "release-1",
            long epoch = 1, long bytes = 100, int files = 2, ResourceNetworkKind network = ResourceNetworkKind.Wifi,
            bool? requiresNetwork = null) =>
            ResourceInspection.Accepted(runtime, inspection, set, epoch, bytes, files, requiresNetwork ?? (bytes > 0), network);
        private static ResourceDownloadPermit Permit(ResourceInspection inspection)
        {
            Assert.IsTrue(ResourceDownloadPermit.TryCreate(inspection, true, out var permit, out var diagnostic));
            Assert.IsNotNull(permit);
            Assert.IsNull(diagnostic);
            return permit;
        }
        private static void Invalid(Action action)
        {
            var failure = Assert.Catch<ArgumentException>(() => action());
            StringAssert.DoesNotContain("secret", failure.Message);
        }
        private static string TypeName(Type type)
        {
            if (type.IsByRef) return TypeName(type.GetElementType());
            if (!type.IsGenericType) return type.Name;
            return type.Name.Split('`')[0] + "<" + string.Join(",", type.GetGenericArguments().Select(TypeName)) + ">";
        }
        private static string Parameters(MethodBase method) =>
            string.Join(",", method.GetParameters().Select(p => (p.IsOut ? "out " : "") + TypeName(p.ParameterType) + " " + p.Name));
        private static void Surface(Type type, string propertyList, params string[] calls)
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            var properties = type.GetProperties(flags);
            CollectionAssert.AreEquivalent(propertyList.Split(';'), properties.Select(p => p.Name + ":" + TypeName(p.PropertyType)).ToArray());
            foreach (var property in properties)
            {
                Assert.IsNull(property.GetSetMethod(true), property.Name);
                Assert.IsFalse(property.PropertyType.IsArray);
                Assert.IsFalse(property.PropertyType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(property.PropertyType));
            }
            Assert.IsEmpty(type.GetFields(flags));
            var actual = type.GetMethods(flags).Where(m => !m.IsSpecialName)
                .Select(m => (m.IsStatic ? "static " : "") + TypeName(m.ReturnType) + " " + m.Name + "(" + Parameters(m) + ")")
                .Concat(type.GetConstructors().Select(c => ".ctor(" + Parameters(c) + ")")).ToArray();
            CollectionAssert.AreEquivalent(calls, actual);
            Assert.IsTrue(type.IsInterface || type.IsSealed);
        }
        private static void EnumShape(Type type, string names)
        {
            CollectionAssert.AreEqual(names.Split(','), Enum.GetNames(type));
            CollectionAssert.AreEqual(Enumerable.Range(0, names.Split(',').Length).ToArray(),
                Enum.GetValues(type).Cast<object>().Select(Convert.ToInt32).ToArray());
        }

        [Test]
        public void C01_PublicSurfaceIsPureAndFixed()
        {
            Surface(typeof(IFightMatchResourceRuntime), "Assets:IFightMatchAssetProvider",
                "Task<ResourceInspection> InspectAsync(Int64 epoch,CancellationToken cancellationToken)",
                "Task<ResourceTransportResult> PrepareAsync(ResourceDownloadPermit permit,IProgress<ResourceProgress> progress,CancellationToken cancellationToken)");
            CollectionAssert.AreEqual(new[] { typeof(IDisposable) }, typeof(IFightMatchResourceRuntime).GetInterfaces());
            Surface(typeof(ResourceRuntimeDiagnostic), "Code:ResourceRuntimeDiagnosticCode;Retryable:Boolean;AssetDiagnostic:FightMatchAssetDiagnostic;SafeCode:String",
                ".ctor(ResourceRuntimeDiagnosticCode code,Boolean retryable,FightMatchAssetDiagnostic assetDiagnostic)");
            Surface(typeof(ResourceInspection), "IsAccepted:Boolean;RuntimeInstanceId:String;InspectionId:String;ReleaseSetId:String;Epoch:Int64;RemainingBytes:Int64;RemainingFiles:Int32;RequiresNetwork:Boolean;NetworkKind:ResourceNetworkKind;Diagnostic:ResourceRuntimeDiagnostic",
                "static ResourceInspection Accepted(String runtimeInstanceId,String inspectionId,String releaseSetId,Int64 epoch,Int64 remainingBytes,Int32 remainingFiles,Boolean requiresNetwork,ResourceNetworkKind networkKind)",
                "static ResourceInspection Rejected(String runtimeInstanceId,String inspectionId,Int64 epoch,ResourceRuntimeDiagnostic diagnostic)");
            Surface(typeof(ResourceDownloadPermit), "Inspection:ResourceInspection;MobileConfirmed:Boolean",
                "static Boolean TryCreate(ResourceInspection inspection,Boolean mobileConfirmed,out ResourceDownloadPermit permit,out ResourceRuntimeDiagnostic diagnostic)",
                "Boolean Matches(ResourceInspection current)");
            Surface(typeof(ResourceProgress), "Inspection:ResourceInspection;OperationId:String;CompletedBytes:Int64;CompletedFiles:Int32;NetworkKind:ResourceNetworkKind;Stage:ResourceTransportStage;Diagnostic:ResourceRuntimeDiagnostic;TotalBytes:Int64;RemainingBytes:Int64;TotalFiles:Int32;RemainingFiles:Int32;ConsentRequired:Boolean;Retryable:Boolean;SafeCode:String",
                ".ctor(ResourceInspection inspection,String operationId,Int64 completedBytes,Int32 completedFiles,ResourceNetworkKind networkKind,ResourceTransportStage stage,ResourceRuntimeDiagnostic diagnostic)");
            Surface(typeof(ResourceTransportResult), "Status:ResourceTransportStatus;RuntimeInstanceId:String;OperationId:String;RequestEpoch:Int64;Permit:ResourceDownloadPermit;Diagnostic:ResourceRuntimeDiagnostic",
                "static ResourceTransportResult Verified(ResourceDownloadPermit permit,String operationId)",
                "static ResourceTransportResult Cancelled(ResourceDownloadPermit permit,String operationId)",
                "static ResourceTransportResult Rejected(String runtimeInstanceId,String operationId,Int64 requestEpoch,ResourceRuntimeDiagnostic diagnostic)");
            EnumShape(typeof(ResourceNetworkKind), "Unknown,Offline,Wifi,Mobile");
            EnumShape(typeof(ResourceTransportStage), "Inspecting,ReadyFromCache,ConsentRequired,Queued,Downloading,Verifying,StopRequested,Stopped,FailedRetryable,FailedTerminal,TransportVerified");
            EnumShape(typeof(ResourceTransportStatus), "TransportVerified,Cancelled,Rejected");
            EnumShape(typeof(ResourceRuntimeDiagnosticCode), "Trust,Schema,Hash,Receipt,Content,Text,Budget,Storage,Stop,Network,Consent,Stale,Asset");
            foreach (var reference in typeof(IFightMatchResourceRuntime).Assembly.GetReferencedAssemblies())
                Assert.IsFalse(reference.Name.StartsWith("UnityEngine", StringComparison.Ordinal) ||
                    reference.Name.Contains("YooAsset") || reference.Name == "FightMatch.Content");
        }

        [Test]
        public void C02_InspectionIdentitiesAndEpochsAreValidated()
        {
            foreach (var id in InvalidIds)
            {
                Invalid(() => Inspect(runtime: id));
                Invalid(() => Inspect(inspection: id));
                Invalid(() => ResourceInspection.Rejected(id, Inspection, 1, Diagnostic()));
                Invalid(() => ResourceInspection.Rejected(Runtime, id, 1, Diagnostic()));
            }
            foreach (var set in new[] { null, "", "latest", "Release", "a/b", "a:b", "a b", new string('a', 129) })
                Invalid(() => Inspect(set: set));
            foreach (var size in new[] { 1, 127, 128 })
                Assert.AreEqual(new string('a', size), Inspect(set: new string('a', size)).ReleaseSetId);
            foreach (var epoch in new[] { long.MinValue, -1L, 0L })
                Invalid(() => Inspect(epoch: epoch));
            Assert.AreEqual(long.MaxValue, Inspect(epoch: long.MaxValue).Epoch);
            Assert.AreEqual(new string('0', 32), Inspect(runtime: new string('0', 32)).RuntimeInstanceId);
            foreach (var epoch in new[] { long.MinValue, -1L, 0L, 1L, long.MaxValue })
            {
                var failure = ResourceInspection.Rejected(Runtime, Inspection, epoch, Diagnostic());
                Assert.AreEqual(epoch, failure.Epoch);
                Assert.AreEqual(Runtime, failure.RuntimeInstanceId);
                Assert.AreEqual(Inspection, failure.InspectionId);
            }
        }

        [Test]
        public void C03_InspectionCountsAndNetworksAreConsistent()
        {
            foreach (var bytes in new[] { long.MinValue, -1L, 1073741825L, long.MaxValue })
                Invalid(() => Inspect(bytes: bytes));
            foreach (var files in new[] { int.MinValue, -1, 513, int.MaxValue })
                Invalid(() => Inspect(files: files));
            foreach (var bytes in new[] { 1L, 1073741823L, 1073741824L })
                Assert.AreEqual(bytes, Inspect(bytes: bytes, files: 1).RemainingBytes);
            foreach (var files in new[] { 1, 511, 512 })
                Assert.AreEqual(files, Inspect(bytes: 512, files: files).RemainingFiles);
            Invalid(() => Inspect(bytes: 0, files: 1));
            Invalid(() => Inspect(bytes: 1, files: 0));
            Invalid(() => Inspect(bytes: 1, files: 2));
            Invalid(() => Inspect(requiresNetwork: false));
            Invalid(() => Inspect(bytes: 0, files: 0, requiresNetwork: true));
            foreach (ResourceNetworkKind network in Enum.GetValues(typeof(ResourceNetworkKind)))
            {
                Assert.IsTrue(Inspect(network: network).RequiresNetwork);
                var cached = Inspect(bytes: 0, files: 0, network: network);
                Assert.IsFalse(cached.RequiresNetwork);
                Assert.AreEqual(0, cached.RemainingBytes);
                Assert.AreEqual(0, cached.RemainingFiles);
            }
            foreach (var network in new[] { -1, 4, int.MaxValue })
                Invalid(() => Inspect(network: (ResourceNetworkKind)network));
        }

        [Test]
        public void C04_RejectedInspectionCannotBecomePermit()
        {
            Assert.IsFalse(ResourceDownloadPermit.TryCreate(null, true, out var absent, out var schema));
            Assert.IsNull(absent);
            Assert.AreEqual(ResourceRuntimeDiagnosticCode.Schema, schema.Code);
            Assert.AreEqual("RES_SCHEMA", schema.SafeCode);
            var cause = Diagnostic(ResourceRuntimeDiagnosticCode.Storage, true);
            var failure = ResourceInspection.Rejected(Runtime, Inspection, -1, cause);
            Assert.IsFalse(failure.IsAccepted);
            Assert.IsNull(failure.ReleaseSetId);
            Assert.AreEqual(0, failure.RemainingBytes);
            Assert.AreEqual(0, failure.RemainingFiles);
            Assert.IsFalse(failure.RequiresNetwork);
            Assert.AreEqual(ResourceNetworkKind.Unknown, failure.NetworkKind);
            foreach (var confirmation in new[] { false, true })
            {
                Assert.IsFalse(ResourceDownloadPermit.TryCreate(failure, confirmation, out var permit, out var diagnostic));
                Assert.IsNull(permit);
                Assert.AreSame(cause, diagnostic);
            }
            Invalid(() => ResourceInspection.Rejected(Runtime, Inspection, 0, null));
        }

        [Test]
        public void C05_MobileConsentIsBoundToEachInspection()
        {
            var first = Inspect(network: ResourceNetworkKind.Mobile);
            Assert.IsFalse(ResourceDownloadPermit.TryCreate(first, false, out var denied, out var consent));
            Assert.IsNull(denied);
            Assert.AreEqual(ResourceRuntimeDiagnosticCode.Consent, consent.Code);
            Assert.IsFalse(consent.Retryable);
            var allowed = Permit(first);
            Assert.IsTrue(allowed.MobileConfirmed);
            Assert.AreSame(first, allowed.Inspection);
            Assert.IsTrue(allowed.Matches(first));
            Assert.IsTrue(allowed.Matches(first), "Matches is a value comparison, not one-time consumption.");
            var next = Inspect(inspection: Other, network: ResourceNetworkKind.Mobile);
            Assert.IsFalse(allowed.Matches(next));
            Assert.IsFalse(ResourceDownloadPermit.TryCreate(next, false, out var nextPermit, out var nextConsent));
            Assert.IsNull(nextPermit);
            Assert.AreEqual(ResourceRuntimeDiagnosticCode.Consent, nextConsent.Code);
            Assert.IsTrue(Permit(next).MobileConfirmed);
            Assert.IsFalse(Permit(Inspect()).MobileConfirmed);
            Assert.IsFalse(Permit(Inspect(bytes: 0, files: 0, network: ResourceNetworkKind.Mobile)).MobileConfirmed);
        }

        [Test]
        public void C06_UnknownOfflineAndLocalZeroNetworkPermits()
        {
            foreach (var network in new[] { ResourceNetworkKind.Unknown, ResourceNetworkKind.Offline })
            {
                Assert.IsFalse(ResourceDownloadPermit.TryCreate(Inspect(network: network), true, out var permit, out var diagnostic));
                Assert.IsNull(permit);
                Assert.AreEqual(ResourceRuntimeDiagnosticCode.Network, diagnostic.Code);
                Assert.IsTrue(diagnostic.Retryable);
                Assert.AreEqual("RES_NETWORK", diagnostic.SafeCode);
            }
            foreach (ResourceNetworkKind network in Enum.GetValues(typeof(ResourceNetworkKind)))
            foreach (var confirmation in new[] { false, true })
            {
                var cached = Inspect(bytes: 0, files: 0, network: network);
                Assert.IsTrue(ResourceDownloadPermit.TryCreate(cached, confirmation, out var permit, out var diagnostic));
                Assert.IsNotNull(permit);
                Assert.IsNull(diagnostic);
                Assert.IsFalse(permit.MobileConfirmed);
                Assert.IsTrue(permit.Matches(cached));
            }
            Assert.IsTrue(ResourceDownloadPermit.TryCreate(Inspect(), false, out var wifi, out var wifiDiagnostic));
            Assert.IsFalse(wifi.MobileConfirmed);
            Assert.IsNull(wifiDiagnostic);
        }

        [Test]
        public void C07_PermitMatchesEveryBoundField()
        {
            var original = Inspect();
            var permit = Permit(original);
            var equal = Inspect();
            Assert.AreNotSame(original, equal);
            Assert.IsTrue(permit.Matches(equal));
            foreach (var changed in new[]
            {
                Inspect(runtime: Other), Inspect(inspection: Other), Inspect(set: "release-2"), Inspect(epoch: 2),
                Inspect(bytes: 101), Inspect(files: 3), Inspect(network: ResourceNetworkKind.Mobile),
                Inspect(bytes: 0, files: 0)
            }) Assert.IsFalse(permit.Matches(changed));
            // A requiresNetwork-only change cannot form a valid accepted inspection.
            Invalid(() => Inspect(requiresNetwork: false));
            Assert.IsFalse(permit.Matches(null));
            Assert.IsFalse(permit.Matches(ResourceInspection.Rejected(Runtime, Inspection, 1, Diagnostic())));
            var mobile = Inspect(network: ResourceNetworkKind.Mobile);
            Assert.IsTrue(Permit(mobile).Matches(Inspect(network: ResourceNetworkKind.Mobile)));
            Assert.IsFalse(Permit(mobile).Matches(Inspect()));
        }

        [Test]
        public void C08_ProgressCountsStagesAndDiagnosticsAreConsistent()
        {
            var inspection = Inspect(bytes: 10, files: 2);
            foreach (var counts in new[] { new long[] { 0, 0 }, new long[] { 1, 0 }, new long[] { 8, 1 }, new long[] { 9, 1 }, new long[] { 10, 2 } })
            {
                var progress = new ResourceProgress(inspection, Operation, counts[0], (int)counts[1],
                    ResourceNetworkKind.Wifi, ResourceTransportStage.Downloading, null);
                Assert.AreSame(inspection, progress.Inspection);
                Assert.AreEqual(Operation, progress.OperationId);
                Assert.AreEqual(10, progress.TotalBytes);
                Assert.AreEqual(2, progress.TotalFiles);
                Assert.AreEqual(10 - counts[0], progress.RemainingBytes);
                Assert.AreEqual(2 - counts[1], progress.RemainingFiles);
                Assert.IsFalse(progress.Retryable);
                Assert.AreEqual("", progress.SafeCode);
            }
            foreach (var counts in new[]
            {
                new long[] { long.MinValue, 0 }, new long[] { -1, 0 }, new long[] { 11, 2 }, new long[] { long.MaxValue, 0 },
                new long[] { 0, -1 }, new long[] { 0, int.MinValue }, new long[] { 0, int.MaxValue }, new long[] { 0, 3 },
                new long[] { 0, 1 }, new long[] { 9, 0 }, new long[] { 10, 1 }, new long[] { 1, 2 }, new long[] { 2, 2 }
            }) Invalid(() => new ResourceProgress(inspection, Operation, counts[0], (int)counts[1],
                ResourceNetworkKind.Wifi, ResourceTransportStage.Downloading, null));
            foreach (ResourceTransportStage stage in Enum.GetValues(typeof(ResourceTransportStage)))
            {
                var diagnostic = stage == ResourceTransportStage.FailedRetryable ? Diagnostic(ResourceRuntimeDiagnosticCode.Network, true) :
                    stage == ResourceTransportStage.FailedTerminal ? Diagnostic() : null;
                var completed = stage == ResourceTransportStage.TransportVerified;
                var progress = new ResourceProgress(
                    stage == ResourceTransportStage.ReadyFromCache ? Inspect(bytes: 0, files: 0) : inspection,
                    Operation, completed ? 10 : 0, completed ? 2 : 0,
                    ResourceNetworkKind.Mobile, stage, diagnostic);
                Assert.AreEqual(stage == ResourceTransportStage.ConsentRequired, progress.ConsentRequired);
                Assert.AreEqual(diagnostic != null && diagnostic.Retryable, progress.Retryable);
                Assert.AreEqual(diagnostic == null ? "" : diagnostic.SafeCode, progress.SafeCode);
            }
            foreach (ResourceNetworkKind network in Enum.GetValues(typeof(ResourceNetworkKind)))
            {
                var progress = new ResourceProgress(inspection, Operation, 0, 0, network, ResourceTransportStage.StopRequested, null);
                Assert.AreEqual(network, progress.NetworkKind);
                if (network != ResourceNetworkKind.Mobile)
                    Invalid(() => new ResourceProgress(inspection, Operation, 0, 0, network, ResourceTransportStage.ConsentRequired, null));
            }
            foreach (var stage in new[] { ResourceTransportStage.FailedRetryable, ResourceTransportStage.FailedTerminal })
            {
                Invalid(() => new ResourceProgress(inspection, Operation, 0, 0, ResourceNetworkKind.Wifi, stage, null));
                Invalid(() => new ResourceProgress(inspection, Operation, 0, 0, ResourceNetworkKind.Wifi, stage,
                    Diagnostic(ResourceRuntimeDiagnosticCode.Network, stage == ResourceTransportStage.FailedTerminal)));
            }
            Invalid(() => new ResourceProgress(inspection, Operation, 0, 0, ResourceNetworkKind.Wifi, ResourceTransportStage.Downloading, Diagnostic()));
            Invalid(() => new ResourceProgress(inspection, Operation, 9, 1, ResourceNetworkKind.Wifi, ResourceTransportStage.TransportVerified, null));
            Invalid(() => new ResourceProgress(Inspect(bytes: 0, files: 0), Operation, 0, 0, ResourceNetworkKind.Mobile, ResourceTransportStage.ConsentRequired, null));
            foreach (var bad in new[] { -1, int.MaxValue })
            {
                Invalid(() => new ResourceProgress(inspection, Operation, 0, 0, (ResourceNetworkKind)bad, ResourceTransportStage.Inspecting, null));
                Invalid(() => new ResourceProgress(inspection, Operation, 0, 0, ResourceNetworkKind.Wifi, (ResourceTransportStage)bad, null));
            }
            Invalid(() => new ResourceProgress(inspection, Operation, 0, 0, (ResourceNetworkKind)4, ResourceTransportStage.Inspecting, null));
            Invalid(() => new ResourceProgress(inspection, Operation, 0, 0, ResourceNetworkKind.Wifi, (ResourceTransportStage)11, null));
            foreach (var id in InvalidIds)
                Invalid(() => new ResourceProgress(inspection, id, 0, 0, ResourceNetworkKind.Wifi, ResourceTransportStage.Queued, null));
            foreach (var invalid in new[] { null, ResourceInspection.Rejected(Runtime, Inspection, 1, Diagnostic()) })
                Invalid(() => new ResourceProgress(invalid, Operation, 0, 0, ResourceNetworkKind.Wifi, ResourceTransportStage.Inspecting, null));
            var local = new ResourceProgress(Inspect(bytes: 0, files: 0), Operation, 0, 0, ResourceNetworkKind.Offline, ResourceTransportStage.TransportVerified, null);
            Assert.AreEqual(0, local.RemainingBytes);
            Assert.AreEqual(0, local.RemainingFiles);
        }

        [Test]
        public void C09_DiagnosticsAreClosedAndPreserveAssetCause()
        {
            var safeCodes = "RES_TRUST,RES_SCHEMA,RES_HASH,RES_RECEIPT,RES_CONTENT,RES_TEXT,RES_BUDGET,RES_STORAGE,RES_STOP,RES_NETWORK,RES_CONSENT,RES_STALE,RES_ASSET".Split(',');
            Assert.IsTrue(FightMatchAssetId.TryCreate("asset-a", out var id));
            foreach (var retryable in new[] { false, true })
            {
                var cause = new FightMatchAssetDiagnostic(FightMatchAssetDiagnosticCode.SdkFailure,
                    FightMatchAssetDiagnosticStage.Acquire, id, "release-1", retryable, "status=failed");
                var diagnostic = new ResourceRuntimeDiagnostic(ResourceRuntimeDiagnosticCode.Asset, retryable, cause);
                Assert.AreSame(cause, diagnostic.AssetDiagnostic);
                Assert.AreEqual(cause.Code, diagnostic.AssetDiagnostic.Code);
                Assert.AreEqual(cause.Stage, diagnostic.AssetDiagnostic.Stage);
                Assert.AreEqual(retryable, diagnostic.Retryable);
                Assert.AreEqual("RES_ASSET", diagnostic.SafeCode);
                Invalid(() => new ResourceRuntimeDiagnostic(ResourceRuntimeDiagnosticCode.Asset, !retryable, cause));
                foreach (ResourceRuntimeDiagnosticCode code in Enum.GetValues(typeof(ResourceRuntimeDiagnosticCode)))
                {
                    if (code == ResourceRuntimeDiagnosticCode.Asset) continue;
                    var value = Diagnostic(code);
                    Assert.AreEqual(safeCodes[(int)code], value.SafeCode);
                    Assert.IsNull(value.AssetDiagnostic);
                    Invalid(() => new ResourceRuntimeDiagnostic(code, false, cause));
                    if (code == ResourceRuntimeDiagnosticCode.Network || code == ResourceRuntimeDiagnosticCode.Storage || code == ResourceRuntimeDiagnosticCode.Stop)
                        Assert.IsTrue(Diagnostic(code, true).Retryable);
                    else Invalid(() => Diagnostic(code, true));
                }
            }
            Invalid(() => new ResourceRuntimeDiagnostic(ResourceRuntimeDiagnosticCode.Asset, false, null));
            foreach (var code in new[] { -1, 13, int.MaxValue })
                Invalid(() => Diagnostic((ResourceRuntimeDiagnosticCode)code));
        }

        [Test]
        public void C10_TransportResultsRemainImmutableAndTransportOnly()
        {
            var permit = Permit(Inspect(epoch: long.MaxValue));
            var verified = ResourceTransportResult.Verified(permit, Operation);
            var cancelled = ResourceTransportResult.Cancelled(permit, Operation);
            Assert.AreEqual(ResourceTransportStatus.TransportVerified, verified.Status);
            Assert.AreEqual(ResourceTransportStatus.Cancelled, cancelled.Status);
            foreach (var result in new[] { verified, cancelled })
            {
                Assert.AreSame(permit, result.Permit);
                Assert.IsNull(result.Diagnostic);
                Assert.AreEqual(Runtime, result.RuntimeInstanceId);
                Assert.AreEqual(Operation, result.OperationId);
                Assert.AreEqual(long.MaxValue, result.RequestEpoch);
            }
            foreach (var epoch in new[] { long.MinValue, -1L, 0L, 1L, long.MaxValue })
            {
                var diagnostic = Diagnostic(ResourceRuntimeDiagnosticCode.Stale);
                var rejected = ResourceTransportResult.Rejected(Runtime, Operation, epoch, diagnostic);
                Assert.AreEqual(ResourceTransportStatus.Rejected, rejected.Status);
                Assert.IsNull(rejected.Permit);
                Assert.AreSame(diagnostic, rejected.Diagnostic);
                Assert.AreEqual(epoch, rejected.RequestEpoch);
            }
            foreach (var id in InvalidIds)
            {
                Invalid(() => ResourceTransportResult.Verified(permit, id));
                Invalid(() => ResourceTransportResult.Cancelled(permit, id));
                Invalid(() => ResourceTransportResult.Rejected(id, Operation, 1, Diagnostic()));
                Invalid(() => ResourceTransportResult.Rejected(Runtime, id, 1, Diagnostic()));
            }
            Invalid(() => ResourceTransportResult.Verified(null, Operation));
            Invalid(() => ResourceTransportResult.Cancelled(null, Operation));
            Invalid(() => ResourceTransportResult.Rejected(Runtime, Operation, 1, null));
            Assert.IsFalse(Enum.GetNames(typeof(ResourceTransportStatus)).Any(n => n == "Prepared" || n == "BusinessReady"));
            Assert.IsFalse(typeof(ResourceTransportResult).GetProperties().Any(p => p.Name == "Assets" || p.Name == "ReleaseSetId"));
            // Constructed Cancelled values do not execute a downloader or prove that its requests stopped.
        }

        [Test]
        public void C11_ProgressStageRequiresConsistentRemainingWork()
        {
            var cached = new ResourceProgress(Inspect(bytes: 0, files: 0), Operation, 0, 0,
                ResourceNetworkKind.Offline, ResourceTransportStage.ReadyFromCache, null);
            Assert.AreEqual(ResourceTransportStage.ReadyFromCache, cached.Stage);
            Assert.AreEqual(0, cached.RemainingBytes);
            Assert.AreEqual(0, cached.RemainingFiles);
            Assert.IsFalse(cached.ConsentRequired);

            var inspection = Inspect(bytes: 10, files: 2, network: ResourceNetworkKind.Mobile);
            var consent = new ResourceProgress(inspection, Operation, 9, 1,
                ResourceNetworkKind.Mobile, ResourceTransportStage.ConsentRequired, null);
            Assert.AreEqual(ResourceTransportStage.ConsentRequired, consent.Stage);
            Assert.AreEqual(1, consent.RemainingBytes);
            Assert.AreEqual(1, consent.RemainingFiles);
            Assert.IsTrue(consent.ConsentRequired);

            foreach (var completed in new[] { new long[] { 0, 0 }, new long[] { 5, 1 }, new long[] { 10, 2 } })
                Invalid(() => new ResourceProgress(inspection, Operation, completed[0], (int)completed[1],
                    ResourceNetworkKind.Mobile, ResourceTransportStage.ReadyFromCache, null));
            Invalid(() => new ResourceProgress(inspection, Operation, 10, 2,
                ResourceNetworkKind.Mobile, ResourceTransportStage.ConsentRequired, null));
        }
    }
}
