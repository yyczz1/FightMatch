using System;
using System.Collections.Generic;
using System.Linq;
using FightMatch.AssetAccess;
using YooAsset;

namespace FightMatch.YooAssetAdapter
{
    internal sealed class YooAssetRemoteServices : IRemoteService
    {
        private readonly string primary, fallback, prefix;
        private readonly bool valid;
        private readonly Action<FightMatchAssetDiagnosticCode, FightMatchAssetDiagnosticStage> diagnostic;
        internal YooAssetRemoteServices(string primary, string fallback, IEnumerable<string> allowedHosts,
            string platform, string release, string package, string version, IEnumerable<string> platforms,
            IEnumerable<string> releases, IEnumerable<string> packages, IEnumerable<string> versions,
            Action<FightMatchAssetDiagnosticCode, FightMatchAssetDiagnosticStage> diagnostic = null)
        {
            this.diagnostic = diagnostic;
            var hosts = new HashSet<string>(allowedHosts ?? Array.Empty<string>(), StringComparer.Ordinal);
            valid = Base(primary, hosts) && (fallback == null || (Base(fallback, hosts) && fallback != primary)) &&
                Known(platform, platforms) && Known(release, releases) && Known(package, packages) &&
                Known(version, versions) && version != "latest" && AssetMapping.ValidSet(release);
            if (!valid) return;
            this.primary = primary;
            this.fallback = fallback;
            prefix = "fightmatch/" + platform + "/" + release + "/yoo/" + package + "/" + version + "/";
        }

        private static bool Known(string value, IEnumerable<string> allowed) =>
            FileName(value) && value.Length <= 128 && value.IndexOf('/') < 0 &&
            char.IsLetterOrDigit(value[0]) && allowed != null && allowed.Contains(value, StringComparer.Ordinal);

        private static bool Base(string value, HashSet<string> allowed)
        {
            if (value == null || value.Length > 2048 || !Uri.TryCreate(value, UriKind.Absolute, out var uri)) return false;
            return uri.Scheme == "https" && uri.IsDefaultPort && uri.UserInfo.Length == 0 &&
                uri.Query.Length == 0 && uri.Fragment.Length == 0 && uri.AbsolutePath == "/" &&
                allowed.Contains(uri.IdnHost) && value == "https://" + uri.IdnHost + "/";
        }

        private static bool FileName(string value)
        {
            if (string.IsNullOrEmpty(value) || value.Length > 256 || value[0] == '/' || value[value.Length - 1] == '/') return false;
            foreach (var part in value.Split('/'))
            {
                if (part.Length == 0 || part == "." || part == "..") return false;
                foreach (var c in part)
                    if (!((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') ||
                        (c >= '0' && c <= '9') || c == '.' || c == '_' || c == '-')) return false;
            }
            return true;
        }

        public IReadOnlyList<string> GetRemoteUrls(string fileName)
        {
            if (!valid || !FileName(fileName))
            {
                try { diagnostic?.Invoke(FightMatchAssetDiagnosticCode.InvalidRemoteUrl, FightMatchAssetDiagnosticStage.BuildRemoteUrl); }
                catch (Exception) { }
                return Array.Empty<string>();
            }
            var first = primary + prefix + fileName;
            return Array.AsReadOnly(fallback == null ? new[] { first } : new[] { first, fallback + prefix + fileName });
        }
    }
}
