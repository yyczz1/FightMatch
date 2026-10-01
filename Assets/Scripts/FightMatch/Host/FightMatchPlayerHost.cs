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

        private IEnumerator Start()
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
            try
            {
                SystemPersistentDataPath = UnityEngine.Application.persistentDataPath;
                var systemRoot = CanonicalSystemRoot(SystemPersistentDataPath);
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
            runtimeRoot?.Unbind(PointerCancellationCause.FocusLost);
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
        private void OnDestroy()
        {
            destroyed = true;
            StopAllCoroutines();
            UnityEngine.Application.lowMemory -= LowMemory;
            runtimeRoot?.Unbind();
            if (Session == null) return;
            Session.QuitRequested -= Quit;
            Session.Dispose();
        }
    }
}
