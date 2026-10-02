using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using FightMatch.Core;
using FightMatch.Platform;
using FightMatch.Presentation;
using TMPro;
using UnityEngine;

namespace FightMatch.Host
{
    public sealed class FightMatchPlayerHost : MonoBehaviour
    {
        [SerializeField] private TMP_FontAsset fontAsset;
        [SerializeField] private UnityEngine.TextAsset fontLicense;
        [SerializeField] private FightMatchHostView runtimeRoot;
        public TMP_FontAsset FontAsset => fontAsset;
        public UnityEngine.TextAsset FontLicense => fontLicense;
        public FightMatchHostView RuntimeRoot => runtimeRoot;
        public FightMatchHostSession Session { get; private set; }
        public string SystemPersistentDataPath { get; private set; }
        public string CanonicalProductRoot { get; private set; }
        private LocalizationService localization;
        private bool destroyed;

        internal string EffectivePersistentDataPath =>
#if UNITY_EDITOR
            acceptancePersistentRoot ??
#endif
            UnityEngine.Application.persistentDataPath;

#if UNITY_EDITOR
        private string acceptancePersistentRoot;
        private string acceptanceActivation;
        private IDisposable acceptanceStorageLease;
        private bool acceptanceSessionReady;
#endif

        private IEnumerator Start()
        {
#if UNITY_EDITOR
            var args = Environment.GetCommandLineArgs();
            if (Array.Exists(args, IsIsolationMarker))
            {
                try
                {
                    acceptancePersistentRoot = ResolveAcceptancePersistentRoot(args,
                        Path.GetDirectoryName(UnityEngine.Application.dataPath),
                        UnityEngine.Application.persistentDataPath, UnityEngine.Application.platform);
                    acceptanceStorageLease = OpenAcceptanceStorageLease(acceptancePersistentRoot);
                    acceptanceActivation = File.ReadAllText(IsolationActivationPath(acceptancePersistentRoot));
                }
                catch (Exception error)
                {
                    ReleaseAcceptanceLease();
                    Debug.LogException(error); ShowFailure("HostSaveIsolationRejected");
                    yield break;
                }
            }
#endif
            var startup = InitializeHost();
            try { while (startup.MoveNext()) yield return startup.Current; }
            finally
            {
                try { (startup as IDisposable)?.Dispose(); }
                finally
                {
#if UNITY_EDITOR
                    if (acceptancePersistentRoot != null && !acceptanceSessionReady) DisposeSessionAndLease();
#endif
                }
            }
        }

        private IEnumerator InitializeHost()
        {
            if (runtimeRoot == null || fontAsset == null || fontLicense == null)
            {
                ShowFailure("MissingHostResources");
                yield break;
            }
            // LOC-TOOL-01 owns the production source. UGUI-01 never turns the draft into a runtime catalog.
            localization = new LocalizationService(null, UnityEngine.Application.systemLanguage);
            runtimeRoot.Bind(null, localization, fontLicense.text);
            if (!localization.IsReady)
            {
                runtimeRoot.ShowFailure("LocalizationNotReady");
                yield break;
            }
            runtimeRoot.SetLoading(true);
            var loader = new FightMatchStreamingAssetsLoader();
            var loading = loader.Load(UnityEngine.Application.streamingAssetsPath);
            try
            {
                while (!destroyed)
                {
                    bool more;
                    try { more = loading.MoveNext(); }
                    catch (Exception error) { Debug.LogException(error); ShowFailure("ContentReadFailed"); yield break; }
                    if (!more) break;
                    yield return loading.Current;
                }
            }
            finally { (loading as IDisposable)?.Dispose(); }
            if (destroyed) yield break;
            if (loader.Catalog == null)
            {
                ShowFailure("ContentValidationFailed");
                yield break;
            }
#if UNITY_EDITOR
            if (acceptancePersistentRoot != null)
            {
                try
                {
                    if (acceptanceStorageLease == null ||
                        ResolveAcceptancePersistentRoot(Environment.GetCommandLineArgs(),
                            Path.GetDirectoryName(UnityEngine.Application.dataPath),
                            UnityEngine.Application.persistentDataPath, UnityEngine.Application.platform) != acceptancePersistentRoot ||
                        File.ReadAllText(IsolationActivationPath(acceptancePersistentRoot)) != acceptanceActivation)
                        throw new IOException("Acceptance storage changed during startup.");
                }
                catch (Exception error)
                {
                    Debug.LogException(error); ShowFailure("HostSaveIsolationRejected");
                    yield break;
                }
            }
#endif
            try
            {
                SystemPersistentDataPath = UnityEngine.Application.persistentDataPath;
                var systemRoot =
#if UNITY_EDITOR
                    acceptancePersistentRoot ??
#endif
                    CanonicalSystemRoot(SystemPersistentDataPath);
                CanonicalProductRoot = Path.Combine(systemRoot, "FightMatch");
                var locator = Path.Combine(CanonicalProductRoot, "locator");
                var profiles = Path.Combine(CanonicalProductRoot, "profiles");
#if UNITY_ANDROID && !UNITY_EDITOR
                Session = new FightMatchHostSession(loader.Catalog, CanonicalProductRoot,
                    new AndroidContentPublicationStorage(locator),
                    id => new AndroidLocalSaveStorage(profiles, id, SavePurpose.PlayerSave));
#else
                Session = new FightMatchHostSession(loader.Catalog, CanonicalProductRoot,
                    new MacContentPublicationStorage(locator),
                    id => new MacEditorSaveStorage(profiles, id, SavePurpose.PlayerSave));
#endif
                Session.QuitRequested += Quit;
                Debug.Log("FightMatch persistentDataPath=" + SystemPersistentDataPath + "; productRoot=" + CanonicalProductRoot);
                BindView();
                Session.ObserveStartup();
#if UNITY_EDITOR
                acceptanceSessionReady = true;
#endif
            }
            catch (Exception error) { Debug.LogException(error); ShowFailure("HostInitializationFailed"); }
        }

        [DllImport("libSystem.B.dylib", EntryPoint = "realpath", SetLastError = true)]
        private static extern IntPtr MacRealPath(string path, byte[] resolved);
        private static string CanonicalSystemRoot(string path)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return AndroidLocalSaveStorage.CanonicalPersistentDataPath(path);
#else
            if (UnityEngine.Application.platform != RuntimePlatform.OSXEditor)
                throw new PlatformNotSupportedException("This host supports Android Player and the Mac Editor.");
            Directory.CreateDirectory(path);
            var buffer = new byte[4096];
            if (MacRealPath(path, buffer) == IntPtr.Zero) throw new IOException("Cannot resolve the system save directory.");
            var count = Array.IndexOf(buffer, (byte)0);
            if (count <= 0) throw new IOException("Invalid system save directory.");
            return System.Text.Encoding.UTF8.GetString(buffer, 0, count);
#endif
        }

        private void BindView()
        {
            runtimeRoot.Unbind();
            runtimeRoot.Bind(Session, localization, fontLicense.text);
        }
        private void OnEnable()
        {
            UnityEngine.Application.lowMemory += LowMemory;
            if (localization != null && runtimeRoot != null) BindView();
        }
        private void OnDisable()
        {
            UnityEngine.Application.lowMemory -= LowMemory;
            runtimeRoot?.PausePresentation(PointerCancellationCause.FocusLost);
            try { runtimeRoot?.Unbind(PointerCancellationCause.FocusLost); }
            finally
            {
#if UNITY_EDITOR
                if (Session == null) ReleaseAcceptanceLease();
#endif
            }
        }
        private void Update()
        {
            if (Session != null && UnityEngine.Input.GetKeyDown(KeyCode.Escape)) Session.Back();
        }
        private void OnApplicationPause(bool paused) { if (paused) runtimeRoot?.PausePresentation(PointerCancellationCause.FocusLost); }
        private void OnApplicationFocus(bool focused) => HandleApplicationFocus(focused);
        internal void HandleApplicationFocus(bool focused) { if (!focused) runtimeRoot?.PausePresentation(PointerCancellationCause.FocusLost); }
        private void LowMemory() { runtimeRoot?.PausePresentation(); }
        private void Quit() { UnityEngine.Application.Quit(); }
        private void ShowFailure(string code)
        {
            Debug.LogError("FightMatch UI: " + code);
            runtimeRoot?.ShowFailure(code);
        }
        private void DisposeSessionAndLease()
        {
            try
            {
                if (Session == null) return;
                var session = Session;
                Session = null;
                session.QuitRequested -= Quit;
                session.Dispose();
            }
            finally
            {
#if UNITY_EDITOR
                ReleaseAcceptanceLease();
#endif
            }
        }

#if UNITY_EDITOR
        private const string IsolationPrefix = "-fmHostSaveIsolation";
        private const string IsolationFolder = "/TestArtifacts/FightMatch/UGUI-01/native-scene-reopen-001/";
        [Serializable] private sealed class IsolationActivation { public IsolationBinding hostSaveIsolation; }
        [Serializable] private sealed class IsolationBinding
        {
            public int schemaVersion;
            public string activationId, canonicalProjectRoot, persistentRoot, candidateSha256, ownerThread, ownerTurn;
        }
        [DllImport("libSystem.B.dylib", EntryPoint = "readlink", SetLastError = true)]
        private static extern IntPtr IsolationReadLink(string path, byte[] buffer, UIntPtr size);

        private static bool IsIsolationMarker(string value) =>
            value != null && value.StartsWith(IsolationPrefix, StringComparison.OrdinalIgnoreCase);
        private static bool IsolationHex(string value, int length) =>
            value != null && System.Text.RegularExpressions.Regex.IsMatch(value, "^[a-f0-9]{" + length + "}$");
        private static void IsolationCanonical(string path)
        {
            if (string.IsNullOrEmpty(path) || !path.StartsWith("/", StringComparison.Ordinal) ||
                path.Contains("\\") || path.Contains(":") || path.Contains("\0") || Path.GetFullPath(path) != path ||
                (path != "/" && Array.Exists(path.Substring(1).Split('/'), x => x == "" || x == "." || x == "..")))
                throw new IOException("Acceptance path is not canonical.");
        }
        private static bool IsolationContains(string a, string b) =>
            string.Equals(a, b, StringComparison.OrdinalIgnoreCase) ||
            b.StartsWith(a.TrimEnd('/') + "/", StringComparison.OrdinalIgnoreCase);
        private static void IsolationDisjoint(string a, string b)
        {
            if (IsolationContains(a, b) || IsolationContains(b, a))
                throw new IOException("Acceptance storage overlaps the system root.");
        }
        private static bool IsolationExistingNonLink(string path)
        {
            if (IsolationReadLink(path, new byte[1], new UIntPtr(1)).ToInt64() >= 0)
                throw new IOException("Acceptance storage contains a symbolic link.");
            var error = Marshal.GetLastWin32Error();
            if (error != 22 && error != 2) throw new IOException("Acceptance path inspection failed.");
            return error == 22;
        }
        private static void IsolationRealPath(string path)
        {
            var buffer = new byte[4096];
            if (MacRealPath(path, buffer) == IntPtr.Zero) throw new IOException("Acceptance realpath failed.");
            var count = Array.IndexOf(buffer, (byte)0);
            if (count <= 0 || System.Text.Encoding.UTF8.GetString(buffer, 0, count) != path)
                throw new IOException("Acceptance path has a different canonical name.");
        }
        private static void IsolationDirectory(string path, bool allowMissing)
        {
            IsolationCanonical(path);
            string nearest = null;
            for (var current = path; !string.IsNullOrEmpty(current); current = Path.GetDirectoryName(current))
            {
                if (!IsolationExistingNonLink(current)) continue;
                var attributes = File.GetAttributes(current);
                if ((attributes & FileAttributes.ReparsePoint) != 0 || (attributes & FileAttributes.Directory) == 0)
                    throw new IOException("Acceptance ancestor is not a plain directory.");
                if (nearest == null) nearest = current;
            }
            if (nearest == null || (!allowMissing && nearest != path))
                throw new IOException("Acceptance directory must already exist.");
            IsolationRealPath(nearest); // Missing real roots are checked through their nearest existing ancestor, never created.
        }
        private static void IsolationFile(string path)
        {
            IsolationCanonical(path);
            IsolationDirectory(Path.GetDirectoryName(path), false);
            if (!IsolationExistingNonLink(path) ||
                (File.GetAttributes(path) & (FileAttributes.Directory | FileAttributes.ReparsePoint)) != 0)
                throw new IOException("Acceptance file must already exist without links.");
            IsolationRealPath(path);
        }
        private static string IsolationActivationPath(string root) =>
            Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(root)), "activation.json");
        private static IsolationBinding IsolationRecord(string root)
        {
            IsolationCanonical(root);
            var marker = root.LastIndexOf(IsolationFolder, StringComparison.Ordinal);
            if (marker <= 0 || !root.EndsWith("/host-save/persistent", StringComparison.Ordinal))
                throw new IOException("Acceptance root is outside the fixed topology.");
            var project = root.Substring(0, marker);
            var id = root.Substring(marker + IsolationFolder.Length).Split('/')[0];
            if (!IsolationHex(id, 32) || root != project + IsolationFolder + id + "/host-save/persistent")
                throw new IOException("Acceptance activation path is invalid.");
            IsolationDirectory(project, false); IsolationDirectory(root, false);
            IsolationDirectory(Path.Combine(root, "FightMatch"), true);
            var activation = IsolationActivationPath(root);
            IsolationFile(activation); IsolationFile(Path.Combine(Path.GetDirectoryName(root), "owner.lock"));
            var record = JsonUtility.FromJson<IsolationActivation>(File.ReadAllText(activation))?.hostSaveIsolation;
            if (record == null || record.schemaVersion != 1 || record.activationId != id ||
                record.canonicalProjectRoot != project || record.persistentRoot != root ||
                !IsolationHex(record.candidateSha256, 64) || string.IsNullOrWhiteSpace(record.ownerThread) ||
                string.IsNullOrWhiteSpace(record.ownerTurn))
                throw new IOException("Acceptance activation identity does not match.");
            return record;
        }
        internal static string ResolveAcceptancePersistentRoot(string[] args, string projectRoot,
            string realPersistentRoot, RuntimePlatform platform)
        {
            if (args == null || !Array.Exists(args, IsIsolationMarker)) return null;
            if (platform != RuntimePlatform.OSXEditor) throw new PlatformNotSupportedException("Acceptance storage requires OSXEditor.");
            string id = null, candidate = null;
            for (var i = 0; i < args.Length; i++)
            {
                if (!IsIsolationMarker(args[i])) continue;
                var flag = args[i];
                if (++i >= args.Length) throw new IOException("Acceptance marker is missing its value.");
                if (flag == IsolationPrefix + "Id" && id == null) id = args[i];
                else if (flag == IsolationPrefix + "Candidate" && candidate == null) candidate = args[i];
                else throw new IOException("Unknown or duplicate acceptance marker.");
            }
            if (!IsolationHex(id, 32) || !IsolationHex(candidate, 64)) throw new IOException("Invalid acceptance markers.");
            IsolationCanonical(projectRoot); IsolationCanonical(realPersistentRoot);
            var root = projectRoot + IsolationFolder + id + "/host-save/persistent";
            foreach (var isolated in new[] { root, Path.Combine(root, "FightMatch") })
                foreach (var real in new[] { realPersistentRoot, Path.Combine(realPersistentRoot, "FightMatch") })
                    IsolationDisjoint(isolated, real);
            IsolationDirectory(realPersistentRoot, true);
            var record = IsolationRecord(root);
            if (record.candidateSha256 != candidate) throw new IOException("Acceptance candidate differs from activation.");
            return root;
        }
        internal static IDisposable OpenAcceptanceStorageLease(string persistentRoot)
        {
            if (UnityEngine.Application.platform != RuntimePlatform.OSXEditor)
                throw new PlatformNotSupportedException("Acceptance storage requires OSXEditor.");
            var record = JsonUtility.ToJson(IsolationRecord(persistentRoot));
            var lease = new FileStream(Path.Combine(Path.GetDirectoryName(persistentRoot), "owner.lock"),
                FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            try
            {
                if (JsonUtility.ToJson(IsolationRecord(persistentRoot)) != record)
                    throw new IOException("Acceptance activation changed while opening the lease.");
                return lease;
            }
            catch { lease.Dispose(); throw; }
        }
        private void ReleaseAcceptanceLease()
        {
            var lease = acceptanceStorageLease;
            acceptanceStorageLease = null;
            lease?.Dispose();
        }
#endif

        private void OnDestroy()
        {
            destroyed = true;
            try
            {
                StopAllCoroutines();
                UnityEngine.Application.lowMemory -= LowMemory;
                runtimeRoot?.Unbind();
            }
            finally { DisposeSessionAndLease(); }
        }
    }
}
