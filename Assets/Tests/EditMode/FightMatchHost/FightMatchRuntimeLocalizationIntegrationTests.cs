using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
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
                Set(host, "resourceBoot", present ? boot : null); Set(host, "resourceBootSha256", new string('0', 64));
                Set(host, "resourceSetId", "fm.builtin.r1.macos"); Set(host, "resourcePlatform", "macOS");
                var task = (Task<FightMatchResourceBridgeResult>)Call(host, "OpenResourcesAsync", 1L, CancellationToken.None);
                Assert.IsTrue(task.IsCompleted); Assert.IsFalse(task.Result.IsDataReady);
                Assert.AreEqual(present ? "RES_TRUST" : "RES_SCHEMA", task.Result.DiagnosticCode);
                Assert.IsNull(host.Session); Assert.IsNull(Get(host, "resourceActivation"));
            }
            finally { UnityEngine.Object.DestroyImmediate(go); UnityEngine.Object.DestroyImmediate(boot); }
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
        private static object Call(FightMatchPlayerHost host, string method, params object[] arguments) =>
            typeof(FightMatchPlayerHost).GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(host, arguments);
        private static void Set(FightMatchPlayerHost host, string field, object value) =>
            typeof(FightMatchPlayerHost).GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(host, value);
        private static object Get(FightMatchPlayerHost host, string field) =>
            typeof(FightMatchPlayerHost).GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(host);
    }
}
