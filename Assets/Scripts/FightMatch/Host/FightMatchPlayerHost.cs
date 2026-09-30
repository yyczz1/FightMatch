using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using FightMatch.Core;
using FightMatch.Platform;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;

namespace FightMatch.Host
{
    [RequireComponent(typeof(UIDocument))]
    public sealed class FightMatchPlayerHost : MonoBehaviour
    {
        [SerializeField] private FontAsset fontAsset;
        [SerializeField] private UnityEngine.TextAsset fontLicense;
        public FontAsset FontAsset => fontAsset;
        public UnityEngine.TextAsset FontLicense => fontLicense;
        public FightMatchHostSession Session { get; private set; }
        public string SystemPersistentDataPath { get; private set; }
        public string CanonicalProductRoot { get; private set; }
        private UIDocument document;
        private FightMatchHostView view;
        private Rect lastSafeArea;
        private Vector2Int lastScreen;
        private bool destroyed;

        private IEnumerator Start()
        {
            document = GetComponent<UIDocument>();
            if (document.panelSettings == null || fontAsset == null || fontLicense == null)
            {
                ShowFailure("游戏界面资源缺失，无法启动。");
                yield break;
            }
            document.rootVisualElement.style.unityFontDefinition = FontDefinition.FromSDFFont(fontAsset);
            document.rootVisualElement.Add(new Label("正在读取冒险内容…"));
            var loader = new FightMatchStreamingAssetsLoader();
            var loading = loader.Load(UnityEngine.Application.streamingAssetsPath);
            try
            {
                while (!destroyed)
                {
                    bool more;
                    try { more = loading.MoveNext(); }
                    catch (Exception error) { ShowFailure("内容读取失败：" + error.Message); yield break; }
                    if (!more) break;
                    yield return loading.Current;
                }
            }
            finally { (loading as IDisposable)?.Dispose(); }
            if (destroyed) yield break;
            if (loader.Catalog == null)
            {
                ShowFailure("内容未通过检查：" + loader.Error);
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
            catch (Exception error) { ShowFailure("游戏启动受阻：" + error.Message); }
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
            view?.Dispose();
            document.rootVisualElement.Clear();
            view = new FightMatchHostView(Session, fontLicense.text);
            view.style.unityFontDefinition = FontDefinition.FromSDFFont(fontAsset);
            document.rootVisualElement.Add(view);
            ApplySafeArea();
        }

        private void OnEnable()
        {
            UnityEngine.Application.lowMemory += LowMemory;
            if (Session != null && !Session.IsDisposed) BindView();
        }
        private void OnDisable()
        {
            UnityEngine.Application.lowMemory -= LowMemory;
            Session?.PausePresentation();
            view?.Dispose();
        }
        private void Update()
        {
            if (Session == null) return;
            if (UnityEngine.Input.GetKeyDown(KeyCode.Escape)) Session.Back();
            if (lastSafeArea != Screen.safeArea || lastScreen != new Vector2Int(Screen.width, Screen.height)) ApplySafeArea();
        }
        private void ApplySafeArea()
        {
            if (view == null || Screen.width <= 0 || Screen.height <= 0) return;
            lastSafeArea = Screen.safeArea;
            lastScreen = new Vector2Int(Screen.width, Screen.height);
            view.style.position = Position.Absolute;
            view.style.left = Length.Percent(100 * lastSafeArea.xMin / Screen.width);
            view.style.right = Length.Percent(100 * (Screen.width - lastSafeArea.xMax) / Screen.width);
            view.style.top = Length.Percent(100 * (Screen.height - lastSafeArea.yMax) / Screen.height);
            view.style.bottom = Length.Percent(100 * lastSafeArea.yMin / Screen.height);
        }
        private void OnApplicationPause(bool paused) { if (paused) Session?.PausePresentation(); }
        private void OnApplicationFocus(bool focused) { if (!focused) Session?.PausePresentation(); }
        private void LowMemory() { Session?.PausePresentation(); }
        private void Quit() { UnityEngine.Application.Quit(); }
        private void ShowFailure(string message)
        {
            Debug.LogError(message);
            if (document == null) return;
            view?.Dispose();
            document.rootVisualElement.Clear();
            document.rootVisualElement.Add(new Label(message));
        }
        private void OnDestroy()
        {
            destroyed = true;
            StopAllCoroutines();
            UnityEngine.Application.lowMemory -= LowMemory;
            view?.Dispose();
            if (Session == null) return;
            Session.QuitRequested -= Quit;
            Session.Dispose();
        }
    }
}
