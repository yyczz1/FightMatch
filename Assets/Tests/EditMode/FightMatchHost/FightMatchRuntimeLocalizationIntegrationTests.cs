using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FightMatch.AssetAccess;
using FightMatch.Platform;
using NUnit.Framework;
using UnityEngine;

namespace FightMatch.Host.Tests
{
    public sealed class FightMatchRuntimeLocalizationIntegrationTests
    {
        [TestCase(false)]
        [TestCase(true)]
        public void MissingOrWrongBootPinCreatesNoSession(bool present)
        {
            var go = new GameObject("resource-startup-failure-test"); go.SetActive(false);
            var host = go.AddComponent<FightMatchPlayerHost>();
            var boot = new TextAsset("{}");
            try
            {
                Set(host, "resourceEpoch", 1L); Set(host, "resourceCancellation", new CancellationTokenSource());
                Configure(host, "macResourceBoot", present ? boot : null, "fm.builtin.r1.macos");
                Configure(host, "windowsResourceBoot", present ? boot : null, "fm.builtin.r1.windows");
                Configure(host, "androidResourceBoot", present ? boot : null, "fm.builtin.r1.android");
                var task = (Task<FightMatchResourceBridgeResult>)Call(host, "OpenResourcesAsync", 1L, CancellationToken.None);
                Assert.IsTrue(task.IsCompleted); Assert.IsFalse(task.Result.IsDataReady);
                Assert.AreEqual(present ? "RES_TRUST" : "RES_SCHEMA", task.Result.DiagnosticCode);
                Assert.IsNull(host.Session); Assert.IsNull(Get(host, "resourceActivation"));
            }
            finally { UnityEngine.Object.DestroyImmediate(go); UnityEngine.Object.DestroyImmediate(boot); }
        }
        [TestCase(RuntimePlatform.OSXEditor, "macResourceBoot", "macOS", "fm.builtin.r1.macos")]
        [TestCase(RuntimePlatform.WindowsEditor, "windowsResourceBoot", "windows", "fm.builtin.r1.windows")]
        [TestCase(RuntimePlatform.Android, "androidResourceBoot", "android", "fm.builtin.r1.android")]
        public void HostSelectsOnlyTheActualPlatformBoot(RuntimePlatform platform, string field, string expectedPlatform, string set)
        {
            var go = new GameObject("platform-boot-selection-test"); go.SetActive(false);
            var host = go.AddComponent<FightMatchPlayerHost>();
            var mac = new TextAsset("mac"); var windows = new TextAsset("windows"); var android = new TextAsset("android");
            try
            {
                Configure(host, "macResourceBoot", mac, "fm.builtin.r1.macos", new string('a', 64));
                Configure(host, "windowsResourceBoot", windows, "fm.builtin.r1.windows", new string('b', 64));
                Configure(host, "androidResourceBoot", android, "fm.builtin.r1.android", new string('c', 64));
                var selected = platform == RuntimePlatform.OSXEditor ? mac : platform == RuntimePlatform.WindowsEditor ? windows : android;
                var input = (ResourceBootstrapInput)Call(host, "CreateResourceInput", platform);
                CollectionAssert.AreEqual(selected.bytes, input.BootBytes);
                Assert.AreEqual(expectedPlatform, input.ExpectedPlatform); Assert.AreEqual(set, input.ExpectedSet);
                var pin = platform == RuntimePlatform.OSXEditor ? 'a' : platform == RuntimePlatform.WindowsEditor ? 'b' : 'c';
                Assert.AreEqual(new string(pin, 64), input.ExpectedBootSha256); Assert.AreEqual(1, input.ExpectedAppProtocol);
                Assert.IsNull(Call(host, "CreateResourceInput", RuntimePlatform.LinuxEditor));
                Assert.IsNull(Call(host, "CreateResourceInput", RuntimePlatform.OSXPlayer));
                Assert.IsNull(Call(host, "CreateResourceInput", RuntimePlatform.WindowsPlayer));
                Configure(host, field, null, set);
                Assert.IsNull(Call(host, "CreateResourceInput", platform), "Missing boot must not select another platform.");
                Configure(host, field, selected, set);
                Get(host, field).GetType().GetField("pinnedSha256").SetValue(Get(host, field), "");
                Assert.IsNull(Call(host, "CreateResourceInput", platform), "Missing pin must not be derived from boot bytes.");
                Configure(host, field, selected, "");
                Assert.IsNull(Call(host, "CreateResourceInput", platform));
                Set(host, field, null);
                Assert.IsNull(Call(host, "CreateResourceInput", platform), "Missing configuration must not select another platform.");
                Assert.IsNull(host.Session); Assert.IsNull(Get(host, "resourceActivation"));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go); UnityEngine.Object.DestroyImmediate(mac);
                UnityEngine.Object.DestroyImmediate(windows); UnityEngine.Object.DestroyImmediate(android);
            }
        }
        [Test]
        public void ResourceEpochIsIndependentOfLocaleAndCancelInvalidatesIt()
        {
            var go = new GameObject("resource-epoch-test"); go.SetActive(false);
            var host = go.AddComponent<FightMatchPlayerHost>();
            try
            {
                var cancellation = new CancellationTokenSource();
                Set(host, "resourceEpoch", 3L); Set(host, "resourceCancellation", cancellation);
                Set(host, "localeEpoch", 999L);
                Assert.IsTrue((bool)Call(host, "CurrentResourceStartup", 3L));
                Call(host, "CancelStartup");
                Assert.IsTrue(cancellation.IsCancellationRequested);
                Assert.IsFalse((bool)Call(host, "CurrentResourceStartup", 3L));
                Assert.IsNull(host.Session);
            }
            finally { UnityEngine.Object.DestroyImmediate(go); }
        }
        [Test]
        public void WindowsHostPathsUseExistingGuardsWithoutMacIsolation()
        {
            if (UnityEngine.Application.platform != RuntimePlatform.WindowsEditor)
                Assert.Ignore("Requires the native Windows Editor; do not execute Windows storage on another platform.");
            var root = Path.Combine(Path.GetTempPath(), "fightmatch-windows-host-" + Guid.NewGuid().ToString("N"));
            var go = new GameObject("windows-host-path-test"); go.SetActive(false);
            var host = go.AddComponent<FightMatchPlayerHost>();
            FightMatchHostSession session = null;
            try
            {
                var canonical = typeof(FightMatchPlayerHost).GetMethod("CanonicalSystemRoot", BindingFlags.Static | BindingFlags.NonPublic);
                Assert.AreEqual(Path.GetFullPath(root), canonical.Invoke(null, new object[] { root }));
                foreach (var invalid in new[] { "relative-root", @"\\server\share\root", Path.Combine(root, "..", "other") })
                {
                    var error = Assert.Throws<TargetInvocationException>(() => canonical.Invoke(null, new object[] { invalid }));
                    Assert.IsInstanceOf<ArgumentException>(error.InnerException);
                }
                Set(host, "preferenceRoot", root);
                typeof(FightMatchPlayerHost).GetProperty(nameof(FightMatchPlayerHost.CanonicalProductRoot))
                    .SetValue(host, Path.Combine(root, "FightMatch"));
                Call(host, "VerifyLocaleStorage");
                Assert.IsFalse(Directory.Exists(root), "Path validation must not create storage or acquire a writer.");
                Set(host, "acceptancePersistentRoot", root);
                var isolation = Assert.Throws<TargetInvocationException>(() => Call(host, "VerifyLocaleStorage"));
                Assert.IsInstanceOf<PlatformNotSupportedException>(isolation.InnerException);
                Set(host, "acceptancePersistentRoot", null);
                var product = host.CanonicalProductRoot;
                Directory.CreateDirectory(product);
                session = new FightMatchHostSession(HostRig.Catalog, product,
                    new WindowsContentPublicationStorage(Path.Combine(product, "locator")),
                    id => new WindowsEditorSaveStorage(Path.Combine(product, "profiles"), id, FightMatch.Core.SavePurpose.PlayerSave));
                session.ObserveStartup(); Assert.IsTrue(session.CanCreate, session.Status);
                var activation = new FightMatchResourceActivationStore(product);
                Assert.AreEqual(ActivationOutcome.Confirmed, activation.EnterAtProcessStart("fm.integration.windows", new string('a', 64)).Outcome);
                session.ObserveStartup(); Assert.IsTrue(session.CanCreate, session.Status);
                var unknown = Path.Combine(product, "unknown.json"); File.WriteAllText(unknown, "{}");
                session.ObserveStartup(); Assert.IsFalse(session.CanCreate);
                File.Delete(unknown);
                session.ObserveStartup(); Assert.IsTrue(session.CanCreate, session.Status);
            }
            finally
            {
                session?.Dispose(); UnityEngine.Object.DestroyImmediate(go);
                if (Directory.Exists(root)) Directory.Delete(root, true);
            }
        }
        [Test]
        public void CodeEnteredStateAllowsFirstCreationButUnknownOrCorruptStateDoesNot()
        {
            if (UnityEngine.Application.platform != RuntimePlatform.OSXEditor)
                Assert.Ignore("Existing Host save-root implementation requires the Mac Editor.");
            var root = Path.Combine(Path.GetTempPath(), "fightmatch-resource-test-" + Guid.NewGuid().ToString("N"));
            // macOS /tmp can be a symlink; use the same canonical path the platform storage returns.
            Directory.CreateDirectory(root);
            FightMatchHostSession session = null;
            try
            {
                var canonical = (string)typeof(FightMatchPlayerHost).GetMethod("CanonicalSystemRoot", BindingFlags.Static | BindingFlags.NonPublic)
                    .Invoke(null, new object[] { root });
                var activation = new FightMatchResourceActivationStore(canonical);
                Assert.AreEqual(ActivationOutcome.Confirmed, activation.EnterAtProcessStart("fm.integration.test", new string('a', 64)).Outcome);
                session = new FightMatchHostSession(HostRig.Catalog, canonical,
                    new MacContentPublicationStorage(Path.Combine(canonical, "locator")),
                    id => new MacEditorSaveStorage(Path.Combine(canonical, "profiles"), id, FightMatch.Core.SavePurpose.PlayerSave));
                session.ObserveStartup(); Assert.IsTrue(session.CanCreate, session.Status);
                var state = Path.Combine(canonical, "resource-state", "v1");
                var unknown = Path.Combine(state, "unknown.json"); File.WriteAllText(unknown, "{}");
                session.ObserveStartup(); Assert.IsFalse(session.CanCreate); File.Delete(unknown);
                File.WriteAllText(Path.Combine(state, "active.json.tmp"), "{}");
                session.ObserveStartup(); Assert.IsFalse(session.CanCreate); File.Delete(Path.Combine(state, "active.json.tmp"));
                File.WriteAllText(Path.Combine(state, "active.json"), "{}");
                session.ObserveStartup(); Assert.IsFalse(session.CanCreate);
            }
            finally { session?.Dispose(); Directory.Delete(root, true); }
        }
        private static void Configure(FightMatchPlayerHost host, string field, TextAsset boot, string set, string pin = null)
        {
            var configuration = Get(host, field);
            configuration.GetType().GetField("boot").SetValue(configuration, boot);
            configuration.GetType().GetField("pinnedSha256").SetValue(configuration, pin ?? new string('0', 64));
            configuration.GetType().GetField("resourceSetId").SetValue(configuration, set);
        }
        private static object Call(FightMatchPlayerHost host, string method, params object[] arguments) =>
            typeof(FightMatchPlayerHost).GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(host, arguments);
        private static void Set(FightMatchPlayerHost host, string field, object value) =>
            typeof(FightMatchPlayerHost).GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(host, value);
        private static object Get(FightMatchPlayerHost host, string field) =>
            typeof(FightMatchPlayerHost).GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(host);
    }
}
