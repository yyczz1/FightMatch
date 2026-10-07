using System;
using System.Threading.Tasks;

namespace FightMatch.AssetAccess
{
    public sealed class FightMatchAssetId : IEquatable<FightMatchAssetId>
    {
        public string Value { get; }
        private FightMatchAssetId(string value) { Value = value; }

        public static bool TryCreate(string value, out FightMatchAssetId assetId)
        {
            assetId = null;
            if (!AssetContractValues.IsIdentifier(value)) return false;
            assetId = new FightMatchAssetId(value);
            return true;
        }
        public bool Equals(FightMatchAssetId other) =>
            other != null && string.Equals(Value, other.Value, StringComparison.Ordinal);
        public override bool Equals(object obj) => obj is FightMatchAssetId other && Equals(other);
        public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);
        public override string ToString() => Value;
    }

    public sealed class AssetAcquireBudget
    {
        public long MaxRawBytes { get; }
        public int MaxConcurrentAcquisitions { get; }
        public int MaxQueuedCallbacks { get; }
        public int MaxRetainedLeases { get; }

        public AssetAcquireBudget(long maxRawBytes, int maxConcurrentAcquisitions,
            int maxQueuedCallbacks, int maxRetainedLeases)
        {
            if (maxRawBytes < 1 || maxRawBytes > 67108864)
                throw new ArgumentOutOfRangeException(nameof(maxRawBytes));
            if (maxConcurrentAcquisitions < 1 || maxConcurrentAcquisitions > 32)
                throw new ArgumentOutOfRangeException(nameof(maxConcurrentAcquisitions));
            if (maxQueuedCallbacks < 1 || maxQueuedCallbacks > 1024)
                throw new ArgumentOutOfRangeException(nameof(maxQueuedCallbacks));
            if (maxRetainedLeases < 1 || maxRetainedLeases > 4096)
                throw new ArgumentOutOfRangeException(nameof(maxRetainedLeases));
            MaxRawBytes = maxRawBytes;
            MaxConcurrentAcquisitions = maxConcurrentAcquisitions;
            MaxQueuedCallbacks = maxQueuedCallbacks;
            MaxRetainedLeases = maxRetainedLeases;
        }
    }

    public enum FightMatchAssetDiagnosticCode
    {
        InvalidAssetId,
        UnknownAsset,
        WrongAssetType,
        WrongReleaseSet,
        BudgetExceeded,
        PackageUnavailable,
        ManifestUnavailable,
        LocationUnavailable,
        SdkFailure,
        StaleEpoch,
        InvalidRemoteUrl,
        ReleasedLease
    }

    public enum FightMatchAssetDiagnosticStage
    {
        ValidateRequest,
        InitializePackage,
        SelectManifest,
        ResolveLocation,
        Acquire,
        ValidateResult,
        Release,
        BuildRemoteUrl
    }

    public sealed class FightMatchAssetDiagnostic
    {
        public FightMatchAssetDiagnosticCode Code { get; }
        public FightMatchAssetDiagnosticStage Stage { get; }
        public FightMatchAssetId AssetId { get; }
        public string ReleaseSetId { get; }
        public bool Retryable { get; }
        public string SafeDetail { get; }

        public FightMatchAssetDiagnostic(FightMatchAssetDiagnosticCode code, FightMatchAssetDiagnosticStage stage,
            FightMatchAssetId assetId, string releaseSetId, bool retryable, string safeDetail)
        {
            if (!Enum.IsDefined(typeof(FightMatchAssetDiagnosticCode), code))
                throw new ArgumentOutOfRangeException(nameof(code));
            if (!Enum.IsDefined(typeof(FightMatchAssetDiagnosticStage), stage))
                throw new ArgumentOutOfRangeException(nameof(stage));
            if ((code == FightMatchAssetDiagnosticCode.InvalidAssetId) != (assetId == null))
                throw new ArgumentException("Asset identity does not match the diagnostic code.", nameof(assetId));
            var set = AssetContractValues.IsReleaseSet(releaseSetId) ? releaseSetId : null;
            if (code == FightMatchAssetDiagnosticCode.InvalidAssetId && set != null)
                throw new ArgumentException("Asset validation precedes release-set validation.", nameof(releaseSetId));
            if (set == null && code != FightMatchAssetDiagnosticCode.InvalidAssetId &&
                code != FightMatchAssetDiagnosticCode.WrongReleaseSet)
                throw new ArgumentException("This diagnostic requires a validated release set.", nameof(releaseSetId));
            Code = code;
            Stage = stage;
            AssetId = assetId;
            ReleaseSetId = set;
            Retryable = retryable;
            SafeDetail = code == FightMatchAssetDiagnosticCode.WrongReleaseSet && set == null
                ? "reason=invalid-release-set" : AssetContractValues.CanonicalDetail(safeDetail);
        }
    }

    public interface IFightMatchAssetLease<out T> : IDisposable where T : class
    {
        FightMatchAssetId AssetId { get; }
        string ReleaseSetId { get; }
        string LeaseId { get; }
        T Asset { get; }
        bool IsReleased { get; }
    }

    public sealed class FightMatchAssetAcquireResult<T> where T : class
    {
        public bool IsAccepted => Lease != null;
        public IFightMatchAssetLease<T> Lease { get; }
        public FightMatchAssetDiagnostic Diagnostic { get; }
        public long RequestEpoch { get; }
        public string ReleaseSetId { get; }

        private FightMatchAssetAcquireResult(IFightMatchAssetLease<T> lease,
            FightMatchAssetDiagnostic diagnostic, long requestEpoch, string releaseSetId)
        {
            Lease = lease;
            Diagnostic = diagnostic;
            RequestEpoch = requestEpoch;
            ReleaseSetId = releaseSetId;
        }

        public static FightMatchAssetAcquireResult<T> Accepted(IFightMatchAssetLease<T> lease,
            long requestEpoch, string releaseSetId)
        {
            if (lease == null) throw new ArgumentNullException(nameof(lease));
            if (requestEpoch <= 0) throw new ArgumentOutOfRangeException(nameof(requestEpoch));
            if (!AssetContractValues.IsReleaseSet(releaseSetId))
                throw new ArgumentException("A validated release set is required.", nameof(releaseSetId));
            if (lease.IsReleased || lease.AssetId == null || lease.Asset == null ||
                !AssetContractValues.IsHex(lease.LeaseId, 32) ||
                !AssetContractValues.IsReleaseSet(lease.ReleaseSetId) ||
                !string.Equals(lease.ReleaseSetId, releaseSetId, StringComparison.Ordinal))
                throw new ArgumentException("A matching live lease is required.", nameof(lease));
            return new FightMatchAssetAcquireResult<T>(lease, null, requestEpoch, releaseSetId);
        }

        public static FightMatchAssetAcquireResult<T> Rejected(FightMatchAssetDiagnostic diagnostic,
            long requestEpoch, string releaseSetId)
        {
            if (diagnostic == null) throw new ArgumentNullException(nameof(diagnostic));
            var set = AssetContractValues.IsReleaseSet(releaseSetId) ? releaseSetId : null;
            if (!string.Equals(set, diagnostic.ReleaseSetId, StringComparison.Ordinal) ||
                (set == null && diagnostic.Code != FightMatchAssetDiagnosticCode.InvalidAssetId &&
                    diagnostic.Code != FightMatchAssetDiagnosticCode.WrongReleaseSet))
                throw new ArgumentException("Release set does not match the diagnostic.", nameof(releaseSetId));
            // Earlier validation failures must remain reportable even when a later epoch is invalid.
            var earlierFailure = diagnostic.Code == FightMatchAssetDiagnosticCode.InvalidAssetId ||
                (diagnostic.Code == FightMatchAssetDiagnosticCode.WrongReleaseSet && set == null) ||
                diagnostic.Code == FightMatchAssetDiagnosticCode.BudgetExceeded;
            if (requestEpoch <= 0 && (diagnostic.Stage != FightMatchAssetDiagnosticStage.ValidateRequest ||
                (!earlierFailure && diagnostic.Code != FightMatchAssetDiagnosticCode.StaleEpoch)))
                throw new ArgumentOutOfRangeException(nameof(requestEpoch));
            return new FightMatchAssetAcquireResult<T>(null, diagnostic, requestEpoch, set);
        }
    }

    public interface IFightMatchAssetProvider
    {
        Task<FightMatchAssetAcquireResult<T>> AcquireAsync<T>(FightMatchAssetId assetId,
            string releaseSetId, AssetAcquireBudget budget, long requestEpoch) where T : class;
    }

    internal static class AssetContractValues
    {
        internal static bool IsIdentifier(string value)
        {
            if (string.IsNullOrEmpty(value) || value.Length > 128 || !IsLowerOrDigit(value[0])) return false;
            for (var i = 1; i < value.Length; i++)
                if (!IsLowerOrDigit(value[i]) && value[i] != '.' && value[i] != '_' && value[i] != '-') return false;
            return true;
        }
        private static bool IsLowerOrDigit(char value) =>
            (value >= 'a' && value <= 'z') || (value >= '0' && value <= '9');
        internal static bool IsReleaseSet(string value) =>
            IsIdentifier(value) && !string.Equals(value, "latest", StringComparison.Ordinal);
        internal static bool IsHex(string value, int length)
        {
            if (value == null || value.Length != length) return false;
            foreach (var c in value)
                if (!((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f'))) return false;
            return true;
        }
        internal static string CanonicalDetail(string value)
        {
            if (value == null) return "";
            if (value.Length > 239) return "reason=redacted";
            if (value.Length == 0) return value;
            string previous = null;
            foreach (var field in value.Split(';'))
            {
                var separator = field.IndexOf('=');
                if (separator <= 0) return "reason=redacted";
                var key = field.Substring(0, separator);
                var token = field.Substring(separator + 1);
                if (previous != null && string.CompareOrdinal(previous, key) >= 0) return "reason=redacted";
                if (!DetailValue(key, token)) return "reason=redacted";
                previous = key;
            }
            return value;
        }
        private static bool DetailValue(string key, string value)
        {
            switch (key)
            {
                case "status":
                    return value == "failed" || value == "pending" || value == "cancelled" || value == "timeout";
                case "reason":
                    switch (value)
                    {
                        case "invalid-asset-id": case "unknown-asset": case "wrong-asset-type":
                        case "invalid-release-set": case "wrong-release-set": case "budget-exceeded":
                        case "package-unavailable": case "manifest-unavailable": case "location-unavailable":
                        case "sdk-failure": case "stale-epoch": case "invalid-remote-url":
                        case "released-lease": case "redacted": return true;
                        default: return false;
                    }
                case "host":
                    return value == "primary" || value == "fallback" || value == "none";
                case "http":
                    return value.Length == 3 && value[0] >= '1' && value[0] <= '5' && DecimalDigits(value);
                case "count":
                    return value.Length >= 1 && value.Length <= 19 && DecimalDigits(value) &&
                        (value.Length == 1 || value[0] != '0');
                case "manifest": case "sha256":
                    return IsHex(value, 64);
                default: return false;
            }
        }
        private static bool DecimalDigits(string value)
        {
            foreach (var c in value) if (c < '0' || c > '9') return false;
            return true;
        }
    }
}
