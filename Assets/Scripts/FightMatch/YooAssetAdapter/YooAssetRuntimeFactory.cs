using System;
using System.Collections.Generic;
using FightMatch.AssetAccess;
using DiagnosticCode = FightMatch.AssetAccess.ResourceRuntimeDiagnosticCode;

namespace FightMatch.YooAssetAdapter
{
    internal enum DeliveryMode { BuiltIn = 0, RemoteHttps = 1 }

    internal sealed class ResourceAdmissionBudget
    {
        internal long MaxBootBytes { get; }
        internal long MaxPhysicalFileBytes { get; }
        internal long MaxTotalPhysicalBytes { get; }
        internal long MaxRawFileBytes { get; }
        internal ResourceAdmissionBudget(long boot, long physical, long total, long raw)
        { MaxBootBytes = boot; MaxPhysicalFileBytes = physical; MaxTotalPhysicalBytes = total; MaxRawFileBytes = raw; }
    }

    // Borrowed for this synchronous call. The caller supplies trusted build/deployment policy.
    internal sealed class ResourceAdmissionInput
    {
        internal byte[] BootBytes { get; }
        internal string ExpectedBootSha256 { get; }
        internal string ExpectedSet { get; }
        internal string ExpectedPlatform { get; }
        internal int ExpectedAppProtocol { get; }
        internal IReadOnlyList<string> SupportedCapabilities { get; }
        internal IReadOnlyList<string> SupportedObjectTypeNames { get; }
        internal DeliveryMode Mode { get; }
        internal IReadOnlyList<string> ApprovedHttpsHosts { get; }
        internal ResourceAdmissionBudget Budget { get; }
        internal ResourceAdmissionInput(byte[] bootBytes, string expectedBootSha256, string expectedSet,
            string expectedPlatform, int expectedAppProtocol, IReadOnlyList<string> supportedCapabilities,
            IReadOnlyList<string> supportedObjectTypeNames, DeliveryMode mode, IReadOnlyList<string> approvedHttpsHosts,
            ResourceAdmissionBudget budget)
        {
            BootBytes = bootBytes; ExpectedBootSha256 = expectedBootSha256; ExpectedSet = expectedSet;
            ExpectedPlatform = expectedPlatform; ExpectedAppProtocol = expectedAppProtocol;
            SupportedCapabilities = supportedCapabilities; SupportedObjectTypeNames = supportedObjectTypeNames;
            Mode = mode; ApprovedHttpsHosts = approvedHttpsHosts; Budget = budget;
        }
    }

    internal sealed class ResourceAdmissionFile
    {
        internal string Name { get; }
        internal string Kind { get; }
        internal long Length { get; }
        internal string Sha256 { get; }
        internal ResourceAdmissionFile(ResourcePlanFile file)
        { Name = file.Name; Kind = file.Kind; Length = file.Length; Sha256 = file.Sha256; }
    }

    internal sealed class ResourceAdmissionMapping
    {
        internal string AssetId { get; }
        internal long ContentLength { get; }
        internal string ContentSha256 { get; }
        internal IReadOnlyList<string> Files { get; }
        internal string Kind { get; }
        internal string Location { get; }
        internal string PackageName { get; }
        internal string Platform { get; }
        internal string ReleaseSetId { get; }
        internal string ScopeId { get; }
        internal string UnityType { get; }
        internal ResourceAdmissionMapping(ResourcePlanMapping entry)
        {
            AssetId = entry.AssetId; ContentLength = entry.ContentLength; ContentSha256 = entry.ContentSha256;
            var files = new string[entry.Files.Count];
            for (var i = 0; i < files.Length; i++) files[i] = entry.Files[i];
            Files = Array.AsReadOnly(files); Kind = entry.Kind; Location = entry.Location;
            PackageName = entry.PackageName; Platform = entry.Platform; ReleaseSetId = entry.ReleaseSetId;
            ScopeId = entry.ScopeId; UnityType = entry.UnityType;
        }
    }

    internal sealed class ResourceAdmissionScopeHash
    {
        internal string ScopeId { get; }
        internal string Sha256 { get; }
        internal ResourceAdmissionScopeHash(ResourcePlanScopeHash scope) { ScopeId = scope.ScopeId; Sha256 = scope.Sha256; }
    }

    internal sealed class ResourceAdmissionPlan
    {
        internal string BootSha256 { get; }
        internal int SchemaVersion { get; }
        internal string ReleaseSetId { get; }
        internal string BusinessReleaseSetId { get; }
        internal string Platform { get; }
        internal string DescriptorSha256 { get; }
        internal string AppBuildIdentity { get; }
        internal int BaseProtocolVersion { get; }
        internal IReadOnlyList<string> RequiredCapabilities { get; }
        internal string PackageName { get; }
        internal string YooAssetPackageVersion { get; }
        internal string YooManifestPackageVersion { get; }
        internal ResourceAdmissionFile Manifest { get; }
        internal IReadOnlyList<ResourceAdmissionFile> PhysicalFiles { get; }
        internal IReadOnlyList<ResourceAdmissionMapping> Mappings { get; }
        internal IReadOnlyList<ResourceAdmissionScopeHash> RequiredScopeHashes { get; }
        internal DeliveryMode Mode { get; }
        internal IReadOnlyList<string> ApprovedHttpsHosts { get; }
        internal ResourceAdmissionBudget Budget { get; }
        internal IReadOnlyList<string> SupportedCapabilities { get; }
        internal IReadOnlyList<string> SupportedObjectTypeNames { get; }

        // Called only after every admission gate; no borrowed array or codec-internal DTO escapes.
        internal ResourceAdmissionPlan(string bootSha, ResourcePlanProjection source, DeliveryMode mode,
            string[] hosts, ResourceAdmissionBudget budget, string[] capabilities, string[] types)
        {
            BootSha256 = bootSha; SchemaVersion = source.SchemaVersion; ReleaseSetId = source.ReleaseSetId;
            BusinessReleaseSetId = source.BusinessReleaseSetId; Platform = source.Platform; DescriptorSha256 = source.DescriptorSha256;
            AppBuildIdentity = source.AppBuildIdentity; BaseProtocolVersion = source.BaseProtocolVersion;
            var required = new string[source.RequiredCapabilities.Count];
            for (var i = 0; i < required.Length; i++) required[i] = source.RequiredCapabilities[i];
            RequiredCapabilities = Array.AsReadOnly(required); PackageName = source.PackageName;
            YooAssetPackageVersion = source.YooAssetPackageVersion; YooManifestPackageVersion = source.YooManifestPackageVersion;
            Manifest = new ResourceAdmissionFile(source.Manifest);
            var files = new ResourceAdmissionFile[source.PhysicalFiles.Count];
            for (var i = 0; i < files.Length; i++) files[i] = new ResourceAdmissionFile(source.PhysicalFiles[i]);
            PhysicalFiles = Array.AsReadOnly(files);
            var mappings = new ResourceAdmissionMapping[source.Mappings.Count];
            for (var i = 0; i < mappings.Length; i++) mappings[i] = new ResourceAdmissionMapping(source.Mappings[i]);
            Mappings = Array.AsReadOnly(mappings);
            var scopes = new ResourceAdmissionScopeHash[source.RequiredScopeHashes.Count];
            for (var i = 0; i < scopes.Length; i++) scopes[i] = new ResourceAdmissionScopeHash(source.RequiredScopeHashes[i]);
            RequiredScopeHashes = Array.AsReadOnly(scopes); Mode = mode;
            ApprovedHttpsHosts = Array.AsReadOnly((string[])hosts.Clone());
            SupportedCapabilities = Array.AsReadOnly((string[])capabilities.Clone());
            SupportedObjectTypeNames = Array.AsReadOnly((string[])types.Clone());
            Budget = new ResourceAdmissionBudget(budget.MaxBootBytes, budget.MaxPhysicalFileBytes,
                budget.MaxTotalPhysicalBytes, budget.MaxRawFileBytes);
        }
    }

    internal static class YooAssetRuntimeFactory
    {
        private sealed class Rejection : Exception
        {
            internal DiagnosticCode Code { get; }
            internal Rejection(DiagnosticCode code) { Code = code; }
        }
        private static void Need(bool valid, DiagnosticCode code = DiagnosticCode.Schema)
        { if (!valid) throw new Rejection(code); }

        internal static bool TryAdmit(ResourceAdmissionInput input, out ResourceAdmissionPlan plan,
            out ResourceRuntimeDiagnostic diagnostic)
        {
            plan = null; diagnostic = null;
            try
            {
                Need(input != null && input.Budget != null);
                var budget = input.Budget;
                Need(budget.MaxBootBytes > 0 && budget.MaxBootBytes <= 262144 &&
                    budget.MaxPhysicalFileBytes > 0 && budget.MaxPhysicalFileBytes <= 268435456 &&
                    budget.MaxTotalPhysicalBytes > 0 && budget.MaxTotalPhysicalBytes <= 1073741824 &&
                    budget.MaxRawFileBytes > 0 && budget.MaxRawFileBytes <= 16777216, DiagnosticCode.Budget);
                Need(!string.IsNullOrEmpty(input.ExpectedSet) && input.ExpectedSet.Length <= 128 &&
                    !string.IsNullOrEmpty(input.ExpectedPlatform) && input.ExpectedPlatform.Length <= 7 && input.ExpectedAppProtocol > 0);
                Need(input.BootBytes != null && input.BootBytes.Length > 0);
                Need(input.BootBytes.LongLength <= budget.MaxBootBytes, DiagnosticCode.Budget);
                var capabilities = Policy(input.SupportedCapabilities, 32, false);
                var types = Policy(input.SupportedObjectTypeNames, 256, true);
                var hosts = Snapshot(input.ApprovedHttpsHosts, 16);
                if (!FightMatchResourceReleaseSet.TryDecodePinned(input.BootBytes, input.ExpectedBootSha256, out var set, out var code))
                    throw new Rejection(code == "RES_TRUST" ? DiagnosticCode.Trust : code == "RES_HASH" ? DiagnosticCode.Hash :
                        code == "RES_BUDGET" ? DiagnosticCode.Budget : DiagnosticCode.Schema);
                Need(set.ReleaseSetId == input.ExpectedSet && set.Platform == input.ExpectedPlatform);
                var projection = set.PlanProjection;
                Need(projection.BaseProtocolVersion == input.ExpectedAppProtocol);
                foreach (var capability in projection.RequiredCapabilities) Need(Array.IndexOf(capabilities, capability) >= 0);
                foreach (var mapping in projection.Mappings)
                    if (mapping.Kind == "object") Need(Array.IndexOf(types, mapping.UnityType) >= 0);
                Need(input.Mode == DeliveryMode.BuiltIn || input.Mode == DeliveryMode.RemoteHttps);
                Need(input.Mode == DeliveryMode.BuiltIn ? hosts.Length == 0 : hosts.Length > 0);
                var hostSet = new HashSet<string>(StringComparer.Ordinal);
                foreach (var host in hosts) Need(CanonicalHost(host) && hostSet.Add(host));
                long total = 0;
                var names = new HashSet<string>(StringComparer.Ordinal);
                foreach (var file in projection.PhysicalFiles)
                {
                    Need(file.Length <= budget.MaxPhysicalFileBytes, DiagnosticCode.Budget);
                    if (file.Kind == "raw") Need(file.Length <= budget.MaxRawFileBytes, DiagnosticCode.Budget);
                    if (names.Add(file.Name)) total = checked(total + file.Length);
                    Need(total <= budget.MaxTotalPhysicalBytes, DiagnosticCode.Budget);
                }
                plan = new ResourceAdmissionPlan(input.ExpectedBootSha256, projection, input.Mode, hosts, budget, capabilities, types);
                return true;
            }
            catch (Rejection failure) { diagnostic = new ResourceRuntimeDiagnostic(failure.Code, false, null); }
            catch (OverflowException) { diagnostic = new ResourceRuntimeDiagnostic(DiagnosticCode.Budget, false, null); }
            catch (UriFormatException) { diagnostic = new ResourceRuntimeDiagnostic(DiagnosticCode.Schema, false, null); }
            catch (ArgumentException) { diagnostic = new ResourceRuntimeDiagnostic(DiagnosticCode.Schema, false, null); }
            catch (InvalidOperationException) { diagnostic = new ResourceRuntimeDiagnostic(DiagnosticCode.Schema, false, null); }
            catch (IndexOutOfRangeException) { diagnostic = new ResourceRuntimeDiagnostic(DiagnosticCode.Schema, false, null); }
            return false;
        }

        private static string[] Snapshot(IReadOnlyList<string> source, int maximum)
        {
            Need(source != null);
            var count = source.Count;
            Need(count >= 0); Need(count <= maximum, DiagnosticCode.Budget);
            var copy = new string[count];
            for (var i = 0; i < count; i++) copy[i] = source[i];
            Need(source.Count == count);
            return copy;
        }
        private static string[] Policy(IReadOnlyList<string> source, int maximum, bool type)
        {
            var copy = Snapshot(source, maximum);
            var distinct = new HashSet<string>(StringComparer.Ordinal);
            foreach (var name in copy) Need(PolicyName(name, type) && distinct.Add(name));
            return copy;
        }
        private static bool PolicyName(string name, bool type)
        {
            if (name == null || name.Length < (type ? 3 : 1) || name.Length > (type ? 128 : 256)) return false;
            var first = true; var dots = 0;
            foreach (var c in name)
            {
                var letter = c >= 'a' && c <= 'z' || c >= 'A' && c <= 'Z';
                var digit = c >= '0' && c <= '9';
                if (type)
                {
                    if (c == '.') { if (first) return false; first = true; dots++; continue; }
                    if (!letter && c != '_' && (first || !digit)) return false;
                }
                else if (!letter && !digit && (first || c != '.' && c != '_' && c != ':' && c != '+' && c != '-')) return false;
                first = false;
            }
            return !first && (!type || dots > 0);
        }
        private static bool CanonicalHost(string host)
        {
            if (host == null || host.Length == 0 || host.Length > 253) return false;
            if (!Uri.TryCreate("https://" + host + "/", UriKind.Absolute, out var uri)) return false;
            return uri.Scheme == Uri.UriSchemeHttps && uri.UserInfo.Length == 0 && uri.Port == 443 &&
                uri.AbsolutePath == "/" && uri.Query.Length == 0 && uri.Fragment.Length == 0 &&
                string.Equals(host, uri.IdnHost, StringComparison.Ordinal);
        }
    }
}
