using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace FightMatch.Host
{
    internal enum ActivationOutcome { Confirmed, Rejected, UnknownCommit }

    internal sealed class ActivationRecord
    {
        internal string ActivationId { get; }
        internal string BaseActivationId { get; }
        internal string DescriptorSha256 { get; }
        internal string Phase { get; }
        internal string ProcessToken { get; }
        internal string ResourceSetId { get; }
        internal int SchemaVersion { get; }
        internal ActivationRecord(string id, string basis, string sha, string phase, string token, string set, int schemaVersion = 1)
        {
            ActivationId = id; BaseActivationId = basis; DescriptorSha256 = sha;
            Phase = phase; ProcessToken = token; ResourceSetId = set; SchemaVersion = schemaVersion;
        }
    }

    internal sealed class ActivationSnapshot
    {
        internal ActivationRecord Active { get; }
        internal ActivationRecord Prepared { get; }
        internal ActivationRecord Restart { get; }
        internal ActivationSnapshot(ActivationRecord active, ActivationRecord prepared, ActivationRecord restart)
        { Active = active; Prepared = prepared; Restart = restart; }
    }

    internal sealed class ActivationResult
    {
        internal ActivationOutcome Outcome { get; }
        internal ActivationSnapshot Snapshot { get; }
        internal string SafeCode { get; }
        internal ActivationResult(ActivationOutcome outcome, ActivationSnapshot snapshot, string code)
        { Outcome = outcome; Snapshot = snapshot; SafeCode = code; }
    }

    internal interface IActivationFiles
    {
        FileAttributes Attributes(string path);
        IEnumerable<string> Entries(string directory);
        void CreateDirectory(string path);
        Stream OpenRead(string path);
        Stream CreateNew(string path);
        void Flush(Stream stream);
        void Move(string source, string target);
        void Replace(string source, string target, string backup);
        void Delete(string path);
    }

    internal sealed class SystemActivationFiles : IActivationFiles
    {
        public FileAttributes Attributes(string path) => File.GetAttributes(path);
        public IEnumerable<string> Entries(string directory) => Directory.EnumerateFileSystemEntries(directory);
        public void CreateDirectory(string path) => Directory.CreateDirectory(path);
        public Stream OpenRead(string path) => new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        public Stream CreateNew(string path) => new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        public void Flush(Stream stream) => ((FileStream)stream).Flush(true);
        public void Move(string source, string target) => File.Move(source, target);
        public void Replace(string source, string target, string backup) => File.Replace(source, target, backup);
        public void Delete(string path) => File.Delete(path);
    }

    internal interface IActivationAndroidPaths
    {
        // Zero denotes a link; EINVAL (22) alone proves an existing path is not a link.
        int ReadLinkError(string path);
        string CanonicalDirectory(string path);
    }

    internal sealed class FightMatchResourceActivationStore
    {
        private const int Limit = 2048;
        private static readonly UTF8Encoding Utf8 = new UTF8Encoding(false, true);
        private static readonly string[] MainNames = { "active.json", "prepared.json", "restart.json" };
        private static readonly HashSet<string> Names = new HashSet<string>(
            MainNames.SelectMany(n => new[] { n, n + ".tmp", n + ".bak" }), StringComparer.Ordinal);
        private const string TokenPattern = @"p[1-9][0-9]{0,9}-[1-9][0-9]{0,18}";
        private const string AndroidTokenPattern = @"a2-b(?:0|[1-9][0-9]{0,9})-p[1-9][0-9]{0,9}-e(?:0|[1-9][0-9]{0,18})";
        private static readonly Regex RecordPattern = new Regex(
            "\\A\\{\"activationId\":\"([0-9a-f]{32})\",\"baseActivationId\":\"(|[0-9a-f]{32})\"," +
            "\"descriptorSha256\":\"([0-9a-f]{64})\",\"phase\":\"(CodeEntered|BusinessReady|Prepared|RequestRestart)\"," +
            "\"processToken\":\"(" + TokenPattern + "|" + AndroidTokenPattern + ")\",\"resourceSetId\":\"([a-z0-9][a-z0-9._-]{0,127})\",\"schemaVersion\":([12])\\}\\z",
            RegexOptions.CultureInvariant);
        private readonly string root, token;
        private readonly IActivationFiles files;
        private readonly int schemaVersion;
        private readonly bool androidStorage;
        private readonly string systemAnchor;
        private readonly IActivationAndroidPaths androidPaths;
        private ActivationSnapshot confirmed;
        private bool unknown;
        private string storageOperation;
        internal string StorageDiagnostic { get; private set; }
        private string Outer => Path.Combine(root, "resource-state");
        private string StateDirectory => Path.Combine(Outer, "v1");

        internal FightMatchResourceActivationStore(string productRoot)
            : this(productRoot, new SystemActivationFiles(), RealToken()) { }
        internal FightMatchResourceActivationStore(string productRoot, IActivationFiles files, string processToken)
        {
            root = productRoot; this.files = files; token = processToken;
            schemaVersion = processToken != null && processToken.StartsWith("a2-", StringComparison.Ordinal) ? 2 : 1;
        }

        internal FightMatchResourceActivationStore(string productRoot, IActivationFiles files, string processToken,
            string systemAnchor, IActivationAndroidPaths androidPaths) : this(productRoot, files, processToken)
        {
            androidStorage = true; this.systemAnchor = systemAnchor; this.androidPaths = androidPaths;
        }

        internal static FightMatchResourceActivationStore CreateAndroid(string productRoot, string systemAnchor)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return new FightMatchResourceActivationStore(productRoot, new SystemActivationFiles(), RealToken(),
                systemAnchor, new NativeAndroidPaths());
#else
            throw new PlatformNotSupportedException("Android Player storage is required.");
#endif
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        private sealed class NativeAndroidPaths : IActivationAndroidPaths
        {
            [DllImport("libc", SetLastError = true)] private static extern IntPtr readlink(string path, byte[] buffer, UIntPtr size);
            [DllImport("libc", SetLastError = true)] private static extern IntPtr realpath(string path, byte[] buffer);
            public int ReadLinkError(string path)
            {
                var length = readlink(path, new byte[1], new UIntPtr(1));
                return length.ToInt64() >= 0 ? 0 : Marshal.GetLastWin32Error();
            }
            public string CanonicalDirectory(string path)
            {
                var bytes = new byte[4096];
                if (realpath(path, bytes) == IntPtr.Zero) throw new IOException("Android anchor inspection failed.");
                var end = Array.IndexOf(bytes, (byte)0);
                if (end <= 0) throw new IOException("Android anchor is unbounded.");
                return Utf8.GetString(bytes, 0, end);
            }
        }
#endif

        internal string EnsureAndroidProductDirectory()
        {
            if (unknown) return "RES_COMMIT_UNKNOWN";
            StorageDiagnostic = null;
            storageOperation = "activation.product.binding";
            try
            {
                AndroidBinding();
                if (!Exists(root))
                {
                    DirectoryGuard(systemAnchor);
                    storageOperation = "activation.product.create";
                    files.CreateDirectory(root);
                }
                Layout();
                return null;
            }
            catch (Fault fault) { return fault.Code; }
            catch (Exception error) { CaptureStorageFailure(error); return "RES_STORAGE"; }
        }

        private void AndroidBinding()
        {
            Require(androidStorage && androidPaths != null && files != null, "RES_ROOT");
            Canonical(systemAnchor, true); Canonical(root, true);
            Require(string.Equals(root, Path.Combine(systemAnchor, "FightMatch"), StringComparison.Ordinal), "RES_ROOT");
        }
        private void AndroidNonLink(string path)
        {
            var error = androidPaths.ReadLinkError(path);
            Require(error != 0, "RES_ROOT");
            if (error != 22) throw new IOException("Android path inspection failed.");
        }

        private static string RealToken()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return AndroidProcessToken.Value;
#else
            try
            {
                using (var process = Process.GetCurrentProcess())
                    return "p" + process.Id.ToString(CultureInfo.InvariantCulture) + "-" +
                        process.StartTime.ToUniversalTime().Ticks.ToString(CultureInfo.InvariantCulture);
            }
            catch (Exception) { return null; }
#endif
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        // First read is made by Host startup on Unity's main thread; cache failures too.
        private static readonly Lazy<string> AndroidProcessToken = new Lazy<string>(ReadAndroidProcessToken);
        private static string ReadAndroidProcessToken()
        {
            try
            {
                using (var player = new UnityEngine.AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var activity = player.GetStatic<UnityEngine.AndroidJavaObject>("currentActivity"))
                using (var resolver = activity.Call<UnityEngine.AndroidJavaObject>("getContentResolver"))
                using (var settings = new UnityEngine.AndroidJavaClass("android.provider.Settings$Global"))
                using (var process = new UnityEngine.AndroidJavaClass("android.os.Process"))
                using (var clock = new UnityEngine.AndroidJavaClass("android.os.SystemClock"))
                {
                    var key = settings.GetStatic<string>("BOOT_COUNT");
                    if (string.IsNullOrEmpty(key)) return null;
                    var before = settings.CallStatic<int>("getInt", resolver, key);
                    var pid = process.CallStatic<int>("myPid");
                    var start = process.CallStatic<long>("getStartElapsedRealtime");
                    var now = clock.CallStatic<long>("elapsedRealtime");
                    var after = settings.CallStatic<int>("getInt", resolver, key);
                    return AndroidTokenFromSample(before, pid, start, now, after);
                }
            }
            catch (Exception) { return null; }
        }
#endif
        internal static string AndroidTokenFromSample(long bootBefore, long pid, long start, long elapsedNow, long bootAfter)
        {
            if (bootBefore < 0 || bootBefore > int.MaxValue || bootBefore != bootAfter ||
                pid <= 0 || pid > int.MaxValue || start < 0 || elapsedNow < 0 || start > elapsedNow) return null;
            return "a2-b" + bootBefore.ToString(CultureInfo.InvariantCulture) + "-p" + pid.ToString(CultureInfo.InvariantCulture) +
                "-e" + start.ToString(CultureInfo.InvariantCulture);
        }
        private static bool TryAndroidToken(string value, out int boot, out int pid, out long start)
        {
            boot = 0; pid = 0; start = 0;
            if (!Matches(value, AndroidTokenPattern)) return false;
            var parts = value.Split('-');
            return int.TryParse(parts[1].Substring(1), NumberStyles.None, CultureInfo.InvariantCulture, out boot) &&
                int.TryParse(parts[2].Substring(1), NumberStyles.None, CultureInfo.InvariantCulture, out pid) &&
                long.TryParse(parts[3].Substring(1), NumberStyles.None, CultureInfo.InvariantCulture, out start);
        }
        private static bool ValidToken(string value, int version) => version == 1
            ? Matches(value, TokenPattern) : version == 2 && TryAndroidToken(value, out _, out _, out _);
        private bool IsSuccessor(string previous)
        {
            if (schemaVersion == 1) return previous != token;
            return TryAndroidToken(previous, out var oldBoot, out _, out var oldStart) &&
                TryAndroidToken(token, out var boot, out _, out var start) &&
                (boot > oldBoot || boot == oldBoot && start > oldStart);
        }

        private sealed class Fault : Exception
        {
            internal readonly string Code;
            internal Fault(string code) { Code = code; }
        }
        private static void Require(bool condition, string code)
        { if (!condition) throw new Fault(code); }
        private static bool Matches(string text, string pattern) =>
            text != null && Regex.IsMatch(text, "\\A(?:" + pattern + ")\\z", RegexOptions.CultureInvariant);
        private static void Target(string set, string sha)
        {
            Require(Matches(set, "[a-z0-9][a-z0-9._-]{0,127}") && set != "latest" &&
                Matches(sha, "[0-9a-f]{64}"), "RES_SCHEMA");
        }
        private static bool SameTarget(ActivationRecord record, string set, string sha) =>
            record != null && record.ResourceSetId == set && record.DescriptorSha256 == sha;
        private static bool Key(ActivationRecord x, ActivationRecord y) =>
            x != null && y != null && x.ActivationId == y.ActivationId && x.BaseActivationId == y.BaseActivationId &&
            SameTarget(x, y.ResourceSetId, y.DescriptorSha256);
        private static bool Next(ActivationRecord p, ActivationRecord a) =>
            p != null && a != null && p.ActivationId != a.ActivationId &&
            p.BaseActivationId == a.ActivationId && p.ResourceSetId != a.ResourceSetId;
        private static bool Pending(ActivationSnapshot s) => s.Restart != null && !Key(s.Restart, s.Active);
        private static ActivationRecord Phase(ActivationRecord r, string phase, string token) =>
            new ActivationRecord(r.ActivationId, r.BaseActivationId, r.DescriptorSha256, phase, token, r.ResourceSetId, r.SchemaVersion);

        internal ActivationResult Read() => Execute(null, null);
        internal ActivationResult EnterAtProcessStart(string exactSet, string descriptorSha256) =>
            Execute("active.json", s =>
            {
                Target(exactSet, descriptorSha256);
                var active = s.Active;
                if (active == null)
                    return new ActivationRecord(Guid.NewGuid().ToString("N"), "", descriptorSha256, "CodeEntered", token, exactSet, schemaVersion);
                if (Pending(s))
                {
                    Require(active.Phase == "BusinessReady" && IsSuccessor(s.Restart.ProcessToken) &&
                        (schemaVersion == 1 || IsSuccessor(active.ProcessToken)) &&
                        SameTarget(s.Restart, exactSet, descriptorSha256), "RES_STATE");
                    return Phase(s.Restart, "CodeEntered", token);
                }
                Require(SameTarget(active, exactSet, descriptorSha256), "RES_STATE");
                if (active.Phase == "CodeEntered" && active.ProcessToken == token) return null;
                Require(IsSuccessor(active.ProcessToken), "RES_STATE");
                return Phase(active, "CodeEntered", token);
            });
        internal ActivationResult MarkBusinessReady(string activationId) =>
            Execute("active.json", s =>
            {
                Require(Matches(activationId, "[0-9a-f]{32}"), "RES_SCHEMA");
                Require(s.Active != null && s.Active.ActivationId == activationId && s.Active.ProcessToken == token, "RES_STATE");
                return s.Active.Phase == "BusinessReady" ? null : Phase(s.Active, "BusinessReady", token);
            });
        internal ActivationResult Prepare(string exactSet, string descriptorSha256) =>
            Execute("prepared.json", s =>
            {
                Target(exactSet, descriptorSha256);
                Require(s.Active != null && s.Active.Phase == "BusinessReady" && s.Active.ProcessToken == token, "RES_STATE");
                if (Next(s.Prepared, s.Active) && SameTarget(s.Prepared, exactSet, descriptorSha256)) return null;
                Require(!Pending(s) && s.Active.ResourceSetId != exactSet, "RES_STATE");
                foreach (var record in new[] { s.Active, s.Prepared, s.Restart })
                    Require(record == null || record.ResourceSetId != exactSet || record.DescriptorSha256 == descriptorSha256, "RES_STATE");
                return new ActivationRecord(Guid.NewGuid().ToString("N"), s.Active.ActivationId,
                    descriptorSha256, "Prepared", token, exactSet, schemaVersion);
            });
        internal ActivationResult RequestRestart(string activationId) =>
            Execute("restart.json", s =>
            {
                Require(Matches(activationId, "[0-9a-f]{32}"), "RES_SCHEMA");
                Require(s.Active != null && s.Active.Phase == "BusinessReady" && s.Active.ProcessToken == token &&
                    Next(s.Prepared, s.Active) && s.Prepared.ActivationId == activationId, "RES_STATE");
                if (Pending(s)) return null;
                return Phase(s.Prepared, "RequestRestart", token);
            });

        private ActivationResult Result(ActivationOutcome outcome, string code = null) =>
            new ActivationResult(outcome, confirmed, code);
        private ActivationResult Execute(string leaf, Func<ActivationSnapshot, ActivationRecord> transition)
        {
            if (unknown) return Result(ActivationOutcome.UnknownCommit, "RES_COMMIT_UNKNOWN");
            StorageDiagnostic = null;
            storageOperation = "activation.load";
            try
            {
                Require(ValidToken(token, schemaVersion), "RES_STATE");
                var state = Load();
                storageOperation = "activation.transition";
                var next = transition == null ? null : transition(state);
                if (next != null) return Commit(state, leaf, next);
                confirmed = state;
                return Result(ActivationOutcome.Confirmed);
            }
            catch (Fault fault) { return Result(ActivationOutcome.Rejected, fault.Code); }
            catch (Exception error)
            {
                CaptureStorageFailure(error);
                return Result(ActivationOutcome.Rejected, "RES_STORAGE");
            }
        }

        private void CaptureStorageFailure(Exception error)
        {
            if (StorageDiagnostic == null)
                StorageDiagnostic = storageOperation + "; error=" + StorageExceptionKind(error);
        }
        internal static string StorageExceptionKind(Exception error) =>
            error is UnauthorizedAccessException ? "UnauthorizedAccessException" :
            error is DirectoryNotFoundException ? "DirectoryNotFoundException" :
            error is FileNotFoundException ? "FileNotFoundException" :
            error is PathTooLongException ? "PathTooLongException" :
            error is IOException ? "IOException" :
            error is System.Security.SecurityException ? "SecurityException" :
            error is NotSupportedException ? "NotSupportedException" :
            error is ArgumentException ? "ArgumentException" :
            error is Fault ? "ValidationFault" : "OtherException";
        private string DirectoryScope(string path) => path == root ? "product" :
            path == Outer ? "outer" : path == StateDirectory ? "state" :
            androidStorage && path == systemAnchor ? "anchor" : "ancestor";

        private static void Canonical(string path, bool product)
        {
            Require(!string.IsNullOrEmpty(path) && Path.IsPathRooted(path) && Path.GetFullPath(path) == path &&
                !path.StartsWith(@"\\?\", StringComparison.Ordinal) && !path.StartsWith(@"\\.\", StringComparison.Ordinal) &&
                path.IndexOf('\0') < 0, "RES_ROOT");
            var volume = Path.GetPathRoot(path);
            Require(!product || path != volume, "RES_ROOT");
            var tail = path.Substring(volume.Length);
            Require(tail.Length == 0 || !tail.Split(Path.DirectorySeparatorChar).Any(x =>
                x == "" || x == "." || x == ".." || x.Contains(":")), "RES_ROOT");
            Require(Path.DirectorySeparatorChar == '\\' || !path.Contains("\\"), "RES_ROOT");
        }
        private void DirectoryGuard(string directory)
        {
            storageOperation = "activation.guard.path";
            Canonical(directory, false);
            if (androidStorage)
            {
                AndroidBinding();
                Require(directory == systemAnchor || directory == root || directory == Outer || directory == StateDirectory, "RES_ROOT");
            }
            var chain = new Stack<string>();
            for (var p = directory; p != null; p = Path.GetDirectoryName(p)) chain.Push(p);
            string parent = null;
            // Android supplies the anchor spelling; attributes and native link checks still cover the entire chain.
            var enumerateParent = !androidStorage;
            while (chain.Count != 0)
            {
                var path = chain.Pop();
                storageOperation = "activation.guard." + DirectoryScope(path) + ".attributes";
                var attributes = files.Attributes(path);
                Require((attributes & FileAttributes.Directory) != 0 && (attributes & FileAttributes.ReparsePoint) == 0, "RES_ROOT");
                if (androidStorage)
                {
                    storageOperation = "activation.guard." + DirectoryScope(path) + ".link";
                    AndroidNonLink(path);
                }
                if (parent != null && enumerateParent)
                {
                    storageOperation = "activation.guard." + DirectoryScope(parent) + ".entries";
                    Require(files.Entries(parent).Any(p => string.Equals(p, path, StringComparison.Ordinal)), "RES_ROOT");
                }
                if (androidStorage && path == systemAnchor) enumerateParent = true;
                parent = path;
            }
            if (androidStorage)
            {
                storageOperation = "activation.anchor.canonical";
                Require(string.Equals(androidPaths.CanonicalDirectory(systemAnchor), systemAnchor, StringComparison.Ordinal), "RES_ROOT");
            }
        }
        private bool Exists(string path)
        {
            var parent = Path.GetDirectoryName(path);
            DirectoryGuard(parent);
            storageOperation = "activation.exists." + DirectoryScope(parent) + ".entries";
            var found = false;
            foreach (var entry in files.Entries(parent))
            {
                if (entry == path)
                {
                    if (!androidStorage) return true;
                    found = true; continue;
                }
                Require(!string.Equals(entry, path, StringComparison.OrdinalIgnoreCase), "RES_ROOT");
            }
            return found;
        }
        private string[] Layout()
        {
            storageOperation = "activation.layout.path";
            Require(files != null, "RES_ROOT");
            Canonical(root, true);
            DirectoryGuard(root);
            if (!Exists(Outer)) return Array.Empty<string>();
            DirectoryGuard(Outer);
            storageOperation = "activation.layout.outer.entries";
            var outerEntries = files.Entries(Outer).Take(2).ToArray();
            Require(outerEntries.All(p => p == StateDirectory), "RES_ROOT");
            if (outerEntries.Length == 0) return Array.Empty<string>();
            DirectoryGuard(StateDirectory);
            storageOperation = "activation.layout.state.entries";
            var entries = files.Entries(StateDirectory).Take(10).ToArray();
            Require(entries.Length <= 9 && entries.Distinct(StringComparer.Ordinal).Count() == entries.Length, "RES_ROOT");
            foreach (var path in entries)
            {
                Require(Path.GetDirectoryName(path) == StateDirectory && Names.Contains(Path.GetFileName(path)), "RES_ROOT");
                storageOperation = "activation.layout.leaf.attributes";
                var attributes = files.Attributes(path);
                Require((attributes & (FileAttributes.Directory | FileAttributes.ReparsePoint)) == 0, "RES_ROOT");
                if (androidStorage)
                {
                    storageOperation = "activation.layout.leaf.link";
                    AndroidNonLink(path);
                }
            }
            return entries;
        }
        private byte[] ReadLeaf(string path)
        {
            Require(Layout().Contains(path), "RES_STATE");
            storageOperation = "activation.read.open";
            using (var stream = files.OpenRead(path))
            {
                Layout();
                storageOperation = "activation.read.length";
                var length = stream.Length;
                Require(length >= 0 && length <= Limit, "RES_SCHEMA");
                var bytes = new byte[(int)length];
                var offset = 0;
                while (offset < bytes.Length)
                {
                    Layout();
                    storageOperation = "activation.read.body";
                    var count = stream.Read(bytes, offset, bytes.Length - offset);
                    Require(count > 0 && count <= bytes.Length - offset, "RES_SCHEMA");
                    offset += count;
                }
                Layout();
                storageOperation = "activation.read.tail";
                Require(stream.ReadByte() == -1 && stream.Length == length, "RES_SCHEMA");
                storageOperation = "activation.read.close";
                return bytes;
            }
        }
        private static ActivationRecord Decode(byte[] bytes, string leaf)
        {
            string text;
            try { text = Utf8.GetString(bytes); }
            catch (DecoderFallbackException) { throw new Fault("RES_SCHEMA"); }
            var m = RecordPattern.Match(text);
            Require(m.Success, "RES_SCHEMA");
            var r = new ActivationRecord(m.Groups[1].Value, m.Groups[2].Value, m.Groups[3].Value,
                m.Groups[4].Value, m.Groups[5].Value, m.Groups[6].Value, m.Groups[7].Value == "1" ? 1 : 2);
            Require(ValidToken(r.ProcessToken, r.ProcessToken.StartsWith("a2-", StringComparison.Ordinal) ? 2 : 1), "RES_SCHEMA");
            Require(ValidToken(r.ProcessToken, r.SchemaVersion), "RES_STATE");
            Target(r.ResourceSetId, r.DescriptorSha256);
            Require(r.BaseActivationId != r.ActivationId, "RES_SCHEMA");
            if (leaf.StartsWith("active.json", StringComparison.Ordinal))
                Require(r.Phase == "CodeEntered" || r.Phase == "BusinessReady", "RES_SCHEMA");
            else
            {
                Require(r.BaseActivationId.Length != 0, "RES_SCHEMA");
                Require(r.Phase == (leaf.StartsWith("prepared.json", StringComparison.Ordinal) ? "Prepared" : "RequestRestart"), "RES_SCHEMA");
            }
            return r;
        }
        private static byte[] Encode(ActivationRecord r, string leaf)
        {
            var text = "{\"activationId\":\"" + r.ActivationId + "\",\"baseActivationId\":\"" + r.BaseActivationId +
                "\",\"descriptorSha256\":\"" + r.DescriptorSha256 + "\",\"phase\":\"" + r.Phase +
                "\",\"processToken\":\"" + r.ProcessToken + "\",\"resourceSetId\":\"" + r.ResourceSetId +
                "\",\"schemaVersion\":" + r.SchemaVersion.ToString(CultureInfo.InvariantCulture) + "}";
            Require(text.Length <= Limit, "RES_SCHEMA");
            var bytes = Utf8.GetBytes(text);
            Require(bytes.Length <= Limit, "RES_SCHEMA");
            Decode(bytes, leaf);
            return bytes;
        }
        private static void Relations(ActivationSnapshot s)
        {
            if (s.Active == null) { Require(s.Prepared == null && s.Restart == null, "RES_STATE"); return; }
            Require(s.Prepared == null || Key(s.Prepared, s.Active) || Next(s.Prepared, s.Active), "RES_STATE");
            Require(s.Restart == null || Key(s.Restart, s.Active) ||
                (s.Active.Phase == "BusinessReady" && Next(s.Prepared, s.Active) && Key(s.Restart, s.Prepared)), "RES_STATE");
        }
        private ActivationSnapshot Load()
        {
            var paths = Layout();
            Require(!paths.Any(p => p.EndsWith(".tmp", StringComparison.Ordinal)), "RES_STATE");
            var activePath = Path.Combine(StateDirectory, MainNames[0]);
            Require(paths.Length == 0 || paths.Contains(activePath), "RES_STATE");
            var records = new Dictionary<string, ActivationRecord>(StringComparer.Ordinal);
            foreach (var path in paths)
            {
                var name = Path.GetFileName(path);
                var bytes = ReadLeaf(path);
                storageOperation = "activation.decode";
                var record = Decode(bytes, name);
                Require(record.SchemaVersion == schemaVersion, "RES_STATE");
                records.Add(name, record);
            }
            records.TryGetValue(MainNames[0], out var active);
            records.TryGetValue(MainNames[1], out var prepared);
            records.TryGetValue(MainNames[2], out var restart);
            var state = new ActivationSnapshot(active, prepared, restart);
            Relations(state);
            return state;
        }
        private void CreateDirectories()
        {
            Layout();
            if (!Exists(Outer)) { DirectoryGuard(root); storageOperation = "activation.create.outer"; files.CreateDirectory(Outer); }
            Layout();
            if (!Exists(StateDirectory)) { DirectoryGuard(Outer); storageOperation = "activation.create.state"; files.CreateDirectory(StateDirectory); }
            Layout();
        }
        private ActivationResult Commit(ActivationSnapshot state, string leaf, ActivationRecord record)
        {
            storageOperation = "activation.encode";
            var bytes = Encode(record, leaf);
            var next = new ActivationSnapshot(leaf == MainNames[0] ? record : state.Active,
                leaf == MainNames[1] ? record : state.Prepared, leaf == MainNames[2] ? record : state.Restart);
            Relations(next);
            var target = Path.Combine(StateDirectory, leaf);
            var temporary = target + ".tmp";
            var created = false;
            var promoted = false;
            try
            {
                CreateDirectories();
                Require(!Layout().Any(p => p.EndsWith(".tmp", StringComparison.Ordinal)), "RES_STATE");
                storageOperation = "activation.commit.create";
                using (var output = files.CreateNew(temporary))
                {
                    created = true;
                    Layout();
                    storageOperation = "activation.commit.write";
                    output.Write(bytes, 0, bytes.Length);
                    Layout();
                    storageOperation = "activation.commit.flush";
                    files.Flush(output);
                    Layout();
                    storageOperation = "activation.commit.close";
                }
                var staged = ReadLeaf(temporary);
                storageOperation = "activation.commit.verify-temporary";
                Require(staged.SequenceEqual(bytes), "RES_STORAGE");
                var entries = Layout();
                var prior = leaf == MainNames[0] ? state.Active : leaf == MainNames[1] ? state.Prepared : state.Restart;
                Require(entries.Contains(target) == (prior != null), "RES_STATE");
                string backup = target + ".bak";
                if (leaf == MainNames[0] && !(record.Phase == "CodeEntered" && state.Active?.Phase == "BusinessReady"))
                    backup = null;
                promoted = true;
                storageOperation = prior == null ? "activation.commit.move" : "activation.commit.replace";
                if (prior == null) files.Move(temporary, target);
                else files.Replace(temporary, target, backup);
                var actual = ReadLeaf(target);
                storageOperation = "activation.commit.verify-target";
                Require(actual.SequenceEqual(bytes) && Key(Decode(actual, leaf), record), "RES_STORAGE");
                Require(!Layout().Any(p => p.EndsWith(".tmp", StringComparison.Ordinal)), "RES_STATE");
                confirmed = next;
                return Result(ActivationOutcome.Confirmed);
            }
            catch (Exception error)
            {
                CaptureStorageFailure(error);
                if (promoted)
                {
                    unknown = true;
                    return Result(ActivationOutcome.UnknownCommit, "RES_COMMIT_UNKNOWN");
                }
                if (created)
                {
                    try { if (Layout().Contains(temporary)) { Layout(); files.Delete(temporary); } }
                    catch (Exception) { } // Preserve uncertain residue; a later read must reject it.
                }
                throw;
            }
        }
    }
}
