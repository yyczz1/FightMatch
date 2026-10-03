using System;
using System.Threading;
using System.Threading.Tasks;

namespace FightMatch.AssetAccess
{
    // Real implementations must revalidate/consume permits and observe actual stop before cancellation.
    public interface IFightMatchResourceRuntime : IDisposable
    {
        Task<ResourceInspection> InspectAsync(long epoch, CancellationToken cancellationToken);
        Task<ResourceTransportResult> PrepareAsync(ResourceDownloadPermit permit,
            IProgress<ResourceProgress> progress, CancellationToken cancellationToken);
        IFightMatchAssetProvider Assets { get; }
    }

    public enum ResourceNetworkKind
    {
        Unknown = 0,
        Offline = 1,
        Wifi = 2,
        Mobile = 3
    }

    public enum ResourceTransportStage
    {
        Inspecting = 0,
        ReadyFromCache = 1,
        ConsentRequired = 2,
        Queued = 3,
        Downloading = 4,
        Verifying = 5,
        StopRequested = 6,
        Stopped = 7,
        FailedRetryable = 8,
        FailedTerminal = 9,
        TransportVerified = 10
    }

    public enum ResourceTransportStatus
    {
        TransportVerified = 0,
        Cancelled = 1,
        Rejected = 2
    }

    public enum ResourceRuntimeDiagnosticCode
    {
        Trust = 0,
        Schema = 1,
        Hash = 2,
        Receipt = 3,
        Content = 4,
        Text = 5,
        Budget = 6,
        Storage = 7,
        Stop = 8,
        Network = 9,
        Consent = 10,
        Stale = 11,
        Asset = 12
    }

    public sealed class ResourceRuntimeDiagnostic
    {
        private static readonly string[] Codes =
        {
            "RES_TRUST", "RES_SCHEMA", "RES_HASH", "RES_RECEIPT", "RES_CONTENT", "RES_TEXT",
            "RES_BUDGET", "RES_STORAGE", "RES_STOP", "RES_NETWORK", "RES_CONSENT", "RES_STALE", "RES_ASSET"
        };
        public ResourceRuntimeDiagnostic(ResourceRuntimeDiagnosticCode code, bool retryable,
            FightMatchAssetDiagnostic assetDiagnostic)
        {
            if (!Enum.IsDefined(typeof(ResourceRuntimeDiagnosticCode), code))
                throw new ArgumentOutOfRangeException(nameof(code));
            if (code == ResourceRuntimeDiagnosticCode.Asset && assetDiagnostic == null)
                throw new ArgumentNullException(nameof(assetDiagnostic));
            if (code != ResourceRuntimeDiagnosticCode.Asset && assetDiagnostic != null)
                throw new ArgumentException("Only asset diagnostics carry an asset cause.", nameof(assetDiagnostic));
            if (code == ResourceRuntimeDiagnosticCode.Asset && retryable != assetDiagnostic.Retryable)
                throw new ArgumentException("Retryability must match the asset cause.", nameof(retryable));
            if (retryable && code != ResourceRuntimeDiagnosticCode.Network &&
                code != ResourceRuntimeDiagnosticCode.Storage && code != ResourceRuntimeDiagnosticCode.Stop &&
                code != ResourceRuntimeDiagnosticCode.Asset)
                throw new ArgumentException("This diagnostic cannot be retried.", nameof(retryable));
            Code = code;
            Retryable = retryable;
            AssetDiagnostic = assetDiagnostic;
            SafeCode = Codes[(int)code];
        }

        public ResourceRuntimeDiagnosticCode Code { get; }
        public bool Retryable { get; }
        public FightMatchAssetDiagnostic AssetDiagnostic { get; }
        public string SafeCode { get; }
    }

    public sealed class ResourceInspection
    {
        public bool IsAccepted => Diagnostic == null;
        public string RuntimeInstanceId { get; }
        public string InspectionId { get; }
        public string ReleaseSetId { get; }
        public long Epoch { get; }
        public long RemainingBytes { get; }
        public int RemainingFiles { get; }
        public bool RequiresNetwork { get; }
        public ResourceNetworkKind NetworkKind { get; }
        public ResourceRuntimeDiagnostic Diagnostic { get; }

        private ResourceInspection(string runtimeInstanceId, string inspectionId, string releaseSetId,
            long epoch, long remainingBytes, int remainingFiles, bool requiresNetwork,
            ResourceNetworkKind networkKind, ResourceRuntimeDiagnostic diagnostic)
        {
            RuntimeInstanceId = runtimeInstanceId;
            InspectionId = inspectionId;
            ReleaseSetId = releaseSetId;
            Epoch = epoch;
            RemainingBytes = remainingBytes;
            RemainingFiles = remainingFiles;
            RequiresNetwork = requiresNetwork;
            NetworkKind = networkKind;
            Diagnostic = diagnostic;
        }

        private static void Identities(string runtimeInstanceId, string inspectionId)
        {
            if (!AssetContractValues.IsHex(runtimeInstanceId, 32))
                throw new ArgumentException("A valid runtime identity is required.", nameof(runtimeInstanceId));
            if (!AssetContractValues.IsHex(inspectionId, 32))
                throw new ArgumentException("A valid inspection identity is required.", nameof(inspectionId));
        }

        public static ResourceInspection Accepted(string runtimeInstanceId, string inspectionId,
            string releaseSetId, long epoch, long remainingBytes, int remainingFiles,
            bool requiresNetwork, ResourceNetworkKind networkKind)
        {
            Identities(runtimeInstanceId, inspectionId);
            if (!AssetContractValues.IsReleaseSet(releaseSetId))
                throw new ArgumentException("A valid release set is required.", nameof(releaseSetId));
            if (epoch <= 0) throw new ArgumentOutOfRangeException(nameof(epoch));
            if (remainingBytes < 0 || remainingBytes > 1073741824)
                throw new ArgumentOutOfRangeException(nameof(remainingBytes));
            if (remainingFiles < 0 || remainingFiles > 512)
                throw new ArgumentOutOfRangeException(nameof(remainingFiles));
            if ((remainingBytes == 0) != (remainingFiles == 0) || remainingFiles > remainingBytes)
                throw new ArgumentException("Remaining byte and file counts must agree.", nameof(remainingFiles));
            if (requiresNetwork != (remainingBytes > 0))
                throw new ArgumentException("Network requirement must match remaining work.", nameof(requiresNetwork));
            if (!Enum.IsDefined(typeof(ResourceNetworkKind), networkKind))
                throw new ArgumentOutOfRangeException(nameof(networkKind));
            return new ResourceInspection(runtimeInstanceId, inspectionId, releaseSetId, epoch,
                remainingBytes, remainingFiles, requiresNetwork, networkKind, null);
        }

        public static ResourceInspection Rejected(string runtimeInstanceId, string inspectionId,
            long epoch, ResourceRuntimeDiagnostic diagnostic)
        {
            Identities(runtimeInstanceId, inspectionId);
            if (diagnostic == null) throw new ArgumentNullException(nameof(diagnostic));
            return new ResourceInspection(runtimeInstanceId, inspectionId, null, epoch, 0, 0,
                false, ResourceNetworkKind.Unknown, diagnostic);
        }
    }

    public sealed class ResourceDownloadPermit
    {
        public ResourceInspection Inspection { get; }
        public bool MobileConfirmed { get; }

        private ResourceDownloadPermit(ResourceInspection inspection, bool mobileConfirmed)
        {
            Inspection = inspection;
            MobileConfirmed = mobileConfirmed;
        }

        public static bool TryCreate(ResourceInspection inspection, bool mobileConfirmed,
            out ResourceDownloadPermit permit, out ResourceRuntimeDiagnostic diagnostic)
        {
            permit = null;
            diagnostic = null;
            if (inspection == null)
                diagnostic = new ResourceRuntimeDiagnostic(ResourceRuntimeDiagnosticCode.Schema, false, null);
            else if (!inspection.IsAccepted) diagnostic = inspection.Diagnostic;
            else if (inspection.RequiresNetwork &&
                (inspection.NetworkKind == ResourceNetworkKind.Unknown || inspection.NetworkKind == ResourceNetworkKind.Offline))
                diagnostic = new ResourceRuntimeDiagnostic(ResourceRuntimeDiagnosticCode.Network, true, null);
            else if (inspection.RequiresNetwork && inspection.NetworkKind == ResourceNetworkKind.Mobile && !mobileConfirmed)
                diagnostic = new ResourceRuntimeDiagnostic(ResourceRuntimeDiagnosticCode.Consent, false, null);
            if (diagnostic != null) return false;
            permit = new ResourceDownloadPermit(inspection,
                inspection.RequiresNetwork && inspection.NetworkKind == ResourceNetworkKind.Mobile && mobileConfirmed);
            return true;
        }

        // Value binding only: runtime ownership, one-time consumption and real user consent are separate.
        public bool Matches(ResourceInspection current)
        {
            return current != null && current.IsAccepted &&
                string.Equals(Inspection.RuntimeInstanceId, current.RuntimeInstanceId, StringComparison.Ordinal) &&
                string.Equals(Inspection.InspectionId, current.InspectionId, StringComparison.Ordinal) &&
                string.Equals(Inspection.ReleaseSetId, current.ReleaseSetId, StringComparison.Ordinal) &&
                Inspection.Epoch == current.Epoch && Inspection.RemainingBytes == current.RemainingBytes &&
                Inspection.RemainingFiles == current.RemainingFiles && Inspection.RequiresNetwork == current.RequiresNetwork &&
                Inspection.NetworkKind == current.NetworkKind &&
                (!current.RequiresNetwork || current.NetworkKind != ResourceNetworkKind.Mobile || MobileConfirmed);
        }
    }

    public sealed class ResourceProgress
    {
        public ResourceProgress(ResourceInspection inspection, string operationId, long completedBytes,
            int completedFiles, ResourceNetworkKind networkKind, ResourceTransportStage stage,
            ResourceRuntimeDiagnostic diagnostic)
        {
            if (inspection == null) throw new ArgumentNullException(nameof(inspection));
            if (!inspection.IsAccepted)
                throw new ArgumentException("An accepted inspection is required.", nameof(inspection));
            if (!AssetContractValues.IsHex(operationId, 32))
                throw new ArgumentException("A valid operation identity is required.", nameof(operationId));
            if (!Enum.IsDefined(typeof(ResourceNetworkKind), networkKind))
                throw new ArgumentOutOfRangeException(nameof(networkKind));
            if (!Enum.IsDefined(typeof(ResourceTransportStage), stage))
                throw new ArgumentOutOfRangeException(nameof(stage));
            if (completedBytes < 0 || completedBytes > inspection.RemainingBytes)
                throw new ArgumentOutOfRangeException(nameof(completedBytes));
            if (completedFiles < 0 || completedFiles > inspection.RemainingFiles)
                throw new ArgumentOutOfRangeException(nameof(completedFiles));
            var remainingBytes = checked(inspection.RemainingBytes - completedBytes);
            var remainingFiles = checked(inspection.RemainingFiles - completedFiles);
            if (completedFiles > completedBytes || remainingFiles > remainingBytes ||
                (remainingBytes == 0) != (remainingFiles == 0))
                throw new ArgumentException("Progress byte and file counts must agree.", nameof(completedFiles));
            if (stage == ResourceTransportStage.ConsentRequired &&
                (!inspection.RequiresNetwork || networkKind != ResourceNetworkKind.Mobile))
                throw new ArgumentException("Mobile network work is required for consent.", nameof(stage));
            var failed = stage == ResourceTransportStage.FailedRetryable || stage == ResourceTransportStage.FailedTerminal;
            if (failed != (diagnostic != null) ||
                (failed && diagnostic.Retryable != (stage == ResourceTransportStage.FailedRetryable)))
                throw new ArgumentException("Stage and diagnostic must agree.", nameof(diagnostic));
            if (stage == ResourceTransportStage.TransportVerified && remainingBytes != 0)
                throw new ArgumentException("Transport verification requires complete progress.", nameof(stage));
            Inspection = inspection;
            OperationId = operationId;
            CompletedBytes = completedBytes;
            CompletedFiles = completedFiles;
            NetworkKind = networkKind;
            Stage = stage;
            Diagnostic = diagnostic;
            RemainingBytes = remainingBytes;
            RemainingFiles = remainingFiles;
        }

        public ResourceInspection Inspection { get; }
        public string OperationId { get; }
        public long CompletedBytes { get; }
        public int CompletedFiles { get; }
        public ResourceNetworkKind NetworkKind { get; }
        public ResourceTransportStage Stage { get; }
        public ResourceRuntimeDiagnostic Diagnostic { get; }
        public long TotalBytes => Inspection.RemainingBytes;
        public long RemainingBytes { get; }
        public int TotalFiles => Inspection.RemainingFiles;
        public int RemainingFiles { get; }
        public bool ConsentRequired => Stage == ResourceTransportStage.ConsentRequired &&
            Inspection.RequiresNetwork && NetworkKind == ResourceNetworkKind.Mobile;
        public bool Retryable => Diagnostic != null && Diagnostic.Retryable;
        public string SafeCode => Diagnostic == null ? "" : Diagnostic.SafeCode;
    }

    public sealed class ResourceTransportResult
    {
        public ResourceTransportStatus Status { get; }
        public string RuntimeInstanceId { get; }
        public string OperationId { get; }
        public long RequestEpoch { get; }
        public ResourceDownloadPermit Permit { get; }
        public ResourceRuntimeDiagnostic Diagnostic { get; }

        private ResourceTransportResult(ResourceTransportStatus status, string runtimeInstanceId,
            string operationId, long requestEpoch, ResourceDownloadPermit permit, ResourceRuntimeDiagnostic diagnostic)
        {
            Status = status;
            RuntimeInstanceId = runtimeInstanceId;
            OperationId = operationId;
            RequestEpoch = requestEpoch;
            Permit = permit;
            Diagnostic = diagnostic;
        }

        private static ResourceTransportResult Complete(ResourceDownloadPermit permit,
            string operationId, ResourceTransportStatus status)
        {
            if (permit == null) throw new ArgumentNullException(nameof(permit));
            if (!permit.Matches(permit.Inspection))
                throw new ArgumentException("A valid permit is required.", nameof(permit));
            if (!AssetContractValues.IsHex(operationId, 32))
                throw new ArgumentException("A valid operation identity is required.", nameof(operationId));
            return new ResourceTransportResult(status, permit.Inspection.RuntimeInstanceId,
                operationId, permit.Inspection.Epoch, permit, null);
        }

        public static ResourceTransportResult Verified(ResourceDownloadPermit permit, string operationId)
        {
            return Complete(permit, operationId, ResourceTransportStatus.TransportVerified);
        }

        public static ResourceTransportResult Cancelled(ResourceDownloadPermit permit, string operationId)
        {
            return Complete(permit, operationId, ResourceTransportStatus.Cancelled);
        }

        public static ResourceTransportResult Rejected(string runtimeInstanceId, string operationId,
            long requestEpoch, ResourceRuntimeDiagnostic diagnostic)
        {
            if (!AssetContractValues.IsHex(runtimeInstanceId, 32))
                throw new ArgumentException("A valid runtime identity is required.", nameof(runtimeInstanceId));
            if (!AssetContractValues.IsHex(operationId, 32))
                throw new ArgumentException("A valid operation identity is required.", nameof(operationId));
            if (diagnostic == null) throw new ArgumentNullException(nameof(diagnostic));
            return new ResourceTransportResult(ResourceTransportStatus.Rejected,
                runtimeInstanceId, operationId, requestEpoch, null, diagnostic);
        }
    }
}
