#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using FightMatch.Core;
using FightMatch.Platform;
using FightMatch.Presentation;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace FightMatch.Host.Tests
{
    public sealed class FightMatchHostLocaleWiringTests
    {
        private const string Prefab = "Assets/UI/FightMatch/Runtime/FightMatchRuntimeRoot.prefab";
        private const string License = "Assets/UI/FightMatch/Fonts/OFL.txt";
        private const string PreferenceName = "locale-preference-v1.json";
        private string owned, persistent;
        private GameObject owner;
        private FightMatchPlayerHost host;
        private FightMatchHostView view;
        private TextAsset license;

        [DllImport("libSystem.B.dylib", EntryPoint = "realpath", SetLastError = true)]
        private static extern IntPtr RealPath(string path, byte[] buffer);
        [DllImport("libSystem.B.dylib", EntryPoint = "symlink", SetLastError = true)]
        private static extern int Link(string target, string path);
        [DllImport("libSystem.B.dylib", EntryPoint = "unlink", SetLastError = true)]
        private static extern int Unlink(string path);

        [SetUp]
        public void SetUp()
        {
            if (UnityEngine.Application.platform != RuntimePlatform.OSXEditor)
                Assert.Ignore("This Host's root/lease checks require the separately authorized Mac run.");
            var path = Path.Combine(Path.GetTempPath(), "FightMatch-HCP-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            var buffer = new byte[4096];
            Assert.AreNotEqual(IntPtr.Zero, RealPath(path, buffer));
            owned = System.Text.Encoding.UTF8.GetString(buffer, 0, Array.IndexOf(buffer, (byte)0));
            persistent = Path.Combine(owned, "persistent");
            Directory.CreateDirectory(persistent);
            CreateHost(persistent);
        }

        [TearDown]
        public void TearDown()
        {
            try { DestroyHost(); }
            finally { if (owned != null && Directory.Exists(owned)) Directory.Delete(owned, true); }
        }

        private static FieldInfo Field(object target, string name) =>
            target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
        private static object Get(object target, string name) => Field(target, name).GetValue(target);
        private static void Set(object target, string name, object value) => Field(target, name).SetValue(target, value);
        private static object Call(object target, string name, params object[] args) =>
            target.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(target, args);
        private void ConfigureRoot(string root)
        {
            Set(host, "preferenceRoot", root);
            Set(host, "<CanonicalProductRoot>k__BackingField", Path.Combine(root, "FightMatch"));
            Set(host, "<SystemPersistentDataPath>k__BackingField", Path.Combine(owned, "simulated-real"));
        }
        private void CreateHost(string root)
        {
            DestroyHost();
            owner = new GameObject("HCP component owner");
            owner.SetActive(false); // Never let the default Start touch a real user root.
            host = owner.AddComponent<FightMatchPlayerHost>();
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
            Assert.IsNotNull(prefab);
            view = UnityEngine.Object.Instantiate(prefab, owner.transform, false).GetComponent<FightMatchHostView>();
            Assert.IsNotNull(view);
            license = AssetDatabase.LoadAssetAtPath<TextAsset>(License);
            Assert.IsNotNull(license);
            Set(host, "runtimeRoot", view);
            Set(host, "fontAsset", view.GetComponentsInChildren<TextMeshProUGUI>(true).First().font);
            Set(host, "fontLicense", license);
            ConfigureRoot(root);
        }
        private void DestroyHost()
        {
            try { if (host != null) Call(host, "OnDestroy"); }
            finally
            {
                if (owner != null) UnityEngine.Object.DestroyImmediate(owner);
                host = null; view = null; owner = null;
            }
        }
        private static LocalizationService Service(SystemLanguage language = SystemLanguage.English) =>
            new LocalizationService(null, language);
        private Func<LocaleId, LocalePreferenceSaveResult> Selection =>
            (Func<LocaleId, LocalePreferenceSaveResult>)Get(view, "selectLocalePreference");
        private int ListenerCount => ((IList)Get(view, "listeners")).Count;
        private LocalizedTmpText Feedback => (LocalizedTmpText)Get(view, "languageFeedback");
        private void Bind() => Call(host, "BindView");
        private void ChooseInComponent(LocaleId locale)
        {
            // Component feedback only; null LOC still prevents real button dispatch.
            ((GameObject)Get(view, "languagePopup")).SetActive(true);
            Call(view, "SelectLocale", locale);
        }
        private FileStream AttachLease(string root)
        {
            var lease = new FileStream(Path.Combine(root, "component-owner.lock"),
                FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None);
            Set(host, "acceptanceStorageLease", lease);
            return lease;
        }
        private static void AssertLocked(string path)
        {
            Assert.Throws<IOException>(() =>
            {
                using (new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None)) { }
            });
        }
        private static void AssertReleased(string path)
        {
            using (new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None)) { }
        }

        private sealed class PreferenceStub : ILocalePreferenceStore
        {
            internal LocalePreferenceLoadResult Loaded = new LocalePreferenceLoadResult(
                LocaleId.En, LocalePreferenceLoadDisposition.MissingSystemDefault, null);
            internal LocalePreferenceSaveResult Result = LocalePreferenceSaveResult.Saved();
            internal int Loads, Saves;
            internal SystemLanguage System;
            internal LocaleId Desired;
            internal Action Loading, Saving;
            public LocalePreferenceLoadResult Load(SystemLanguage language)
            {
                Loads++; System = language; Loading?.Invoke(); return Loaded;
            }
            public LocalePreferenceSaveResult Save(LocaleId locale)
            {
                Saves++; Desired = locale; Saving?.Invoke(); return Result;
            }
        }

        [Test]
        public void HCP_01_LoadBeforeBind()
        {
            var dispositions = new[] { LocalePreferenceLoadDisposition.Remembered,
                LocalePreferenceLoadDisposition.MissingSystemDefault, LocalePreferenceLoadDisposition.ReadFailedSystemDefault,
                LocalePreferenceLoadDisposition.InvalidSystemDefault };
            foreach (var disposition in dispositions)
            {
                CreateHost(persistent);
                var language = disposition == LocalePreferenceLoadDisposition.ReadFailedSystemDefault ?
                    SystemLanguage.English : SystemLanguage.ChineseSimplified;
                var expected = disposition == LocalePreferenceLoadDisposition.Remembered ? LocaleId.En :
                    LocalePolicy.FromSystemLanguage(language);
                var code = disposition == LocalePreferenceLoadDisposition.ReadFailedSystemDefault ? "PreferenceReadFailed" :
                    disposition == LocalePreferenceLoadDisposition.InvalidSystemDefault ? "PreferenceInvalidDocument" : null;
                var service = Service(language);
                var store = new PreferenceStub { Loaded = new LocalePreferenceLoadResult(expected, disposition, code) };
                store.Loading = () => Assert.IsFalse((bool)Get(view, "bound"));
                service.LocaleChanged += _ => Assert.IsFalse((bool)Get(view, "bound"), "Load must precede first Bind.");
                if (code != null) LogAssert.Expect(LogType.Warning, "FightMatch locale preference: " + code);
                host.InitializeLocalePreference(service, store, language);
                Assert.AreEqual(expected, service.CurrentLocale);
                Assert.AreEqual(language, store.System);
                Assert.AreEqual(1, store.Loads); Assert.AreEqual(0, store.Saves);
                Bind();
                Assert.AreSame(service, Get(view, "localization"));
                Assert.AreEqual(expected, ((LocalizationService)Get(view, "localization")).CurrentLocale);
                Assert.IsFalse(service.IsReady);
                Assert.AreEqual("LocalizationNotReady", view.DiagnosticCode);
                host.InitializeLocalePreference(service, store, language);
                Assert.AreEqual(1, store.Loads); Assert.AreEqual(0, store.Saves);
            }
        }

        [Test]
        public void HCP_02_SelectAndRetry()
        {
            var service = Service(); var store = new PreferenceStub();
            host.InitializeLocalePreference(service, store, SystemLanguage.English);
            var order = new List<string>();
            int changes = 0;
            service.LocaleChanged += _ => { changes++; order.Add("Set"); };
            store.Saving = () =>
            {
                Assert.AreEqual(store.Desired, service.CurrentLocale); order.Add("Save");
            };
            var results = new[] { LocalePreferenceSaveResult.Saved(),
                LocalePreferenceSaveResult.Failed("PreferenceCommitFailed"),
                LocalePreferenceSaveResult.Failed("PreferenceTargetReadFailed", true) };
            for (int i = 0; i < results.Length; i++)
            {
                store.Result = results[i]; order.Clear();
                var locale = i % 2 == 0 ? LocaleId.ZhHans : LocaleId.En;
                Assert.AreSame(results[i], host.SelectLocalePreference(locale));
                CollectionAssert.AreEqual(new[] { "Set", "Save" }, order);
                Assert.AreEqual(locale, service.CurrentLocale, "Failed/Unknown must not roll back the selected language.");
                var count = changes; order.Clear();
                Assert.AreSame(results[i], host.SelectLocalePreference(locale));
                CollectionAssert.AreEqual(new[] { "Save" }, order);
                Assert.AreEqual(count, changes);
                Assert.AreEqual((i + 1) * 2, store.Saves);
                Assert.IsNull(host.Session);
            }
            Assert.IsFalse(Directory.Exists(Path.Combine(persistent, "FightMatch", "profiles")));
        }

        [Test]
        public void HCP_03_Feedback()
        {
            var service = Service(); var store = new PreferenceStub();
            host.InitializeLocalePreference(service, store, SystemLanguage.English); Bind();
            foreach (var result in new[] { LocalePreferenceSaveResult.Saved(),
                LocalePreferenceSaveResult.Failed("PreferenceCommitFailed"),
                LocalePreferenceSaveResult.Failed("PreferenceTargetReadFailed", true) })
            {
                store.Result = result; ChooseInComponent(LocaleId.ZhHans);
                Assert.AreEqual(result.LocalizationKey, Feedback.Key);
                var args = (KeyValuePair<string, string>[])Get(Feedback, "arguments");
                if (result.ErrorCode == null) Assert.IsEmpty(args);
                else CollectionAssert.AreEqual(new[] { new KeyValuePair<string, string>("errorCode", result.ErrorCode) }, args);
                Assert.IsTrue(Feedback.gameObject.activeSelf);
                Assert.AreEqual(LocalizedTmpText.Placeholder, Feedback.Target.text);
                Assert.IsNull(host.Session);
            }
            int before = store.Saves;
            store.Result = null;
            Feedback.gameObject.SetActive(false); ChooseInComponent(LocaleId.En);
            Assert.IsFalse(Feedback.gameObject.activeSelf, "Null selection result must not publish Saved.");
            Assert.AreEqual(before + 1, store.Saves);
            store.Result = LocalePreferenceSaveResult.Saved();
            store.Saving = () => Bind();
            ChooseInComponent(LocaleId.ZhHans);
            Assert.IsFalse(Feedback.gameObject.activeSelf, "A result from the old View generation must be ignored.");
            store.Saving = null; before = store.Saves;
            view.Bind(null, service, license.text);
            ChooseInComponent(LocaleId.En);
            Assert.AreEqual("fm.language.changed", Feedback.Key);
            Assert.AreEqual(before, store.Saves, "The three-argument Bind remains memory-only.");
        }

        [Test]
        public void HCP_04_Rebind()
        {
            var service = Service(); var store = new PreferenceStub();
            host.InitializeLocalePreference(service, store, SystemLanguage.English); Bind();
            var old = Selection; int listeners = ListenerCount;
            Assert.Greater(listeners, 0);
            Bind();
            Assert.IsNull(old(LocaleId.ZhHans));
            Assert.AreEqual(0, store.Saves);
            Assert.AreEqual(listeners, ListenerCount);
            var buttons = ((IList)Get(view, "listeners")).Cast<KeyValuePair<UnityEngine.UI.Button, UnityEngine.Events.UnityAction>>()
                .Select(x => x.Key).ToArray();
            Assert.AreEqual(buttons.Length, buttons.Distinct().Count(), "Each button must have one owned listener.");
            Assert.AreEqual(1, store.Loads);
            var current = Selection;
            service.LocaleChanged += _ => Call(host, "OnDisable");
            Assert.IsNull(current(LocaleId.ZhHans), "LocaleChanged cancellation must be checked before Save.");
            Assert.AreEqual(LocaleId.ZhHans, service.CurrentLocale);
            Assert.AreEqual(0, store.Saves); Assert.AreEqual(0, ListenerCount);
            Assert.IsNull(Selection);
            Call(host, "OnEnable");
            Assert.AreEqual(1, store.Loads); Assert.AreEqual(0, ListenerCount);
            Assert.IsNull(current(LocaleId.En));
        }

        [Test]
        public void HCP_05_CancelStartup()
        {
            var service = Service(); var store = new PreferenceStub();
            var lease = AttachLease(owned); string lockPath = lease.Name;
            host.InitializeLocalePreference(service, store, SystemLanguage.English); Bind();
            var epoch = (long)Get(host, "localeEpoch"); var old = Selection;
            var continuation = (IEnumerator)Call(host, "InitializeHost");
            bool observed = false;
            service.DiagnosticsChanged += () =>
            {
                observed = true;
                Assert.IsNull(Get(host, "localePreference"));
                Assert.IsFalse((bool)Get(view, "bound"));
                Assert.IsTrue(lease.CanWrite, "Revoke and Unbind must precede lease release.");
                Assert.IsNull(old(LocaleId.ZhHans));
            };
            Call(host, "OnDisable");
            Assert.IsTrue(observed);
            Assert.IsFalse((bool)Call(host, "CheckLocalePreference", epoch), "The startup resume guard must reject the old epoch.");
            Assert.IsFalse(continuation.MoveNext());
            (continuation as IDisposable)?.Dispose();
            Assert.IsFalse(lease.CanWrite); AssertReleased(lockPath);
            Call(host, "OnEnable");
            host.InitializeLocalePreference(service, store, SystemLanguage.English);
            Assert.AreEqual(1, store.Loads); Assert.AreEqual(0, store.Saves);
            Assert.IsNull(host.Session); Assert.AreEqual(0, ListenerCount);
            Assert.AreEqual("HostInitializationFailed", view.DiagnosticCode);
            Assert.IsFalse(Directory.Exists(Path.Combine(persistent, "FightMatch")));
        }

        [Test]
        public void HCP_06_DisposeOrder()
        {
            // Main-engineering clarification also permits the existing real HostRig here, without fault injection.
            using (var rig = new HostRig(false))
            {
                ConfigureRoot(Path.GetDirectoryName(rig.Root));
                var service = Service(); var store = new PreferenceStub();
                var lease = AttachLease(Path.GetDirectoryName(rig.Root)); string lockPath = lease.Name;
                Set(host, "<Session>k__BackingField", rig.Session);
                host.InitializeLocalePreference(service, store, SystemLanguage.English); Bind();
                var session = rig.Session; var old = Selection; var files = rig.Files();
                Call(host, "OnDisable");
                Assert.AreSame(session, host.Session); Assert.IsFalse(session.IsDisposed);
                Assert.IsTrue(lease.CanWrite); AssertLocked(lockPath);
                Assert.AreSame(store, Get(host, "localePreference"));
                Assert.IsNull(old(LocaleId.ZhHans)); Assert.IsNull(host.SelectLocalePreference(LocaleId.ZhHans));
                Assert.AreEqual(0, ListenerCount); Assert.AreEqual(0, store.Saves);
                Call(host, "OnEnable");
                Assert.AreEqual(1, store.Loads); Assert.Greater(ListenerCount, 0);
                Assert.AreEqual(LocaleId.En, service.CurrentLocale);
                bool unboundBeforeDispose = false;
                service.DiagnosticsChanged += () =>
                {
                    unboundBeforeDispose = true;
                    Assert.IsNull(Get(host, "localePreference")); Assert.IsFalse((bool)Get(view, "bound"));
                    Assert.IsFalse(session.IsDisposed); Assert.IsTrue(lease.CanWrite);
                    AssertLocked(lockPath);
                };
                Call(host, "OnDestroy");
                Assert.IsTrue(unboundBeforeDispose);
                Assert.IsNull(host.Session); Assert.IsTrue(session.IsDisposed); Assert.AreEqual(1, session.DisposeCount);
                Assert.IsFalse(lease.CanWrite); AssertReleased(lockPath);
                Call(host, "OnDestroy");
                Assert.AreEqual(1, session.DisposeCount); Assert.AreEqual(0, store.Saves);
                Assert.AreEqual(0, ListenerCount); rig.SameFiles(files);
            }
        }

        [Test]
        public void HCP_07_SettingsFirst()
        {
            using (var rig = new HostRig(false))
            {
                var root = Path.GetDirectoryName(rig.Root);
                ConfigureRoot(root);
                var preferences = new FileLocalePreferenceStore(root, new LocalePreferenceFiles());
                var service = Service();
                host.InitializeLocalePreference(service, preferences, SystemLanguage.English);
                Assert.AreEqual(LocalePreferenceSaveDisposition.Saved, host.SelectLocalePreference(LocaleId.ZhHans).Disposition);
                var target = Path.Combine(rig.Root, "settings", PreferenceName);
                var preferenceBytes = File.ReadAllBytes(target);
                Assert.AreEqual(0, rig.FactoryCalls); Assert.IsNull(rig.Session.OriginalProfile);
                rig.Create();
                var player = rig.Session.Observation.ActiveProfile.PlayerId;
                var commit = rig.Head.Header.CommitId; var files = rig.Files();
                rig.Rebuild();
                Assert.IsNull(rig.Session.OriginalProfile);
                Assert.AreEqual(player, rig.Session.Observation.ActiveProfile.PlayerId);
                Assert.AreEqual(commit, rig.Head.Header.CommitId);
                Assert.AreEqual(LocalPlayerProfileState.Active, rig.Session.Observation.State);
                rig.SameFiles(files);
                CreateHost(root); service = Service();
                host.InitializeLocalePreference(service, new FileLocalePreferenceStore(root, new LocalePreferenceFiles()),
                    SystemLanguage.English);
                Bind();
                Assert.AreEqual(LocaleId.ZhHans, service.CurrentLocale);
                CollectionAssert.AreEqual(preferenceBytes, File.ReadAllBytes(target));
                rig.SameFiles(files);
            }
        }

        [Serializable] private sealed class Activation { public Binding hostSaveIsolation; }
        [Serializable] private sealed class Binding
        {
            public int schemaVersion = 1;
            public string activationId, canonicalProjectRoot, persistentRoot, candidateSha256, ownerThread, ownerTurn;
        }

        [Test]
        public void HCP_08_NullSourceAndIsolation()
        {
            var service = Service(); var store = new PreferenceStub();
            host.InitializeLocalePreference(service, store, SystemLanguage.English); Bind();
            ((GameObject)Get(view, "languagePopup")).SetActive(true);
            foreach (var name in new[] { "languageEnglishButton", "languageChineseButton", "createButton", "continueButton" })
                ((UnityEngine.UI.Button)Get(view, name)).onClick.Invoke();
            Assert.AreEqual("LocalizationNotReady", view.DiagnosticCode); Assert.IsFalse(service.IsReady);
            Assert.IsNull(host.Session); Assert.AreEqual(0, store.Saves);
            Assert.IsFalse(Directory.Exists(Path.Combine(persistent, "FightMatch")));
            var project = Path.Combine(owned, "Project"); var id = new string('a', 32);
            persistent = project + "/TestArtifacts/FightMatch/UGUI-01/native-scene-reopen-001/" + id + "/host-save/persistent";
            Directory.CreateDirectory(persistent);
            var activation = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(persistent)), "activation.json");
            var lockPath = Path.Combine(Path.GetDirectoryName(persistent), "owner.lock");
            File.WriteAllBytes(lockPath, Array.Empty<byte>());
            File.WriteAllText(activation, JsonUtility.ToJson(new Activation { hostSaveIsolation = new Binding {
                activationId = id, canonicalProjectRoot = project, persistentRoot = persistent,
                candidateSha256 = new string('b', 64), ownerThread = "HCP-owned", ownerTurn = "HCP-owned-turn" } }));
            CreateHost(persistent);
            Set(host, "acceptancePersistentRoot", persistent);
            Set(host, "acceptanceActivation", File.ReadAllText(activation));
            var lease = (FileStream)FightMatchPlayerHost.OpenAcceptanceStorageLease(persistent);
            Set(host, "acceptanceStorageLease", lease);
            Call(host, "VerifyLocaleStorage");
            var settings = Path.Combine(persistent, "FightMatch", "settings");
            Directory.CreateDirectory(Path.GetDirectoryName(settings));
            var outside = Path.Combine(owned, "outside");
            Directory.CreateDirectory(outside);
            var sentinel = Path.Combine(outside, "sentinel"); File.WriteAllBytes(sentinel, new byte[] { 7, 19, 3 });
            foreach (var leaf in new[] { false, true })
            foreach (var dangling in new[] { false, true })
            {
                if (leaf) Directory.CreateDirectory(settings);
                var link = leaf ? Path.Combine(settings, PreferenceName) : settings;
                var destination = dangling ? Path.Combine(outside, "absent") : leaf ? sentinel : outside;
                Assert.AreEqual(0, Link(destination, link));
                try
                {
                    var error = Assert.Throws<TargetInvocationException>(() => Call(host, "VerifyLocaleStorage"));
                    Assert.IsInstanceOf<IOException>(error.InnerException);
                }
                finally { Assert.AreEqual(0, Unlink(link)); }
            }
            var target = Path.Combine(settings, PreferenceName);
            Assert.AreEqual(0, Link(sentinel, target));
            try
            {
                store = new PreferenceStub();
                LogAssert.Expect(LogType.Exception, new System.Text.RegularExpressions.Regex("IOException: Acceptance storage contains a symbolic link"));
                LogAssert.Expect(LogType.Error, "FightMatch UI: HostSaveIsolationRejected");
                host.InitializeLocalePreference(Service(), store, SystemLanguage.English);
                Assert.AreEqual(0, store.Loads); Assert.AreEqual(0, store.Saves);
                Assert.AreEqual("HostSaveIsolationRejected", view.DiagnosticCode);
                Assert.IsNull(host.Session); Assert.IsFalse(lease.CanWrite); AssertReleased(lockPath);
            }
            finally { Assert.AreEqual(0, Unlink(target)); }
            CollectionAssert.AreEqual(new byte[] { 7, 19, 3 }, File.ReadAllBytes(sentinel));
            CollectionAssert.AreEquivalent(new[] { sentinel }, Directory.GetFileSystemEntries(outside));
            Assert.IsFalse(Directory.Exists(Path.Combine(owned, "simulated-real")));
            Assert.IsFalse(Directory.Exists(Path.Combine(persistent, "FightMatch", "profiles")));
        }
    }
}
#endif
