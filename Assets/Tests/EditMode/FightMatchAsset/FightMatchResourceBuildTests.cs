using System;
using System.IO;
using System.Linq;
using System.Text;
using FightMatch.YooAssetAdapter;
using FightMatch.YooAssetAdapter.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace FightMatch.AssetAccess.Tests
{
    public sealed class FightMatchResourceBuildTests
    {
        // Encoder fixture only: this byte string is deliberately not an SDK manifest or runtime evidence.
        internal static readonly byte[] TestManifest = Encoding.UTF8.GetBytes("synthetic-manifest-for-encoder-tests");
        internal static FightMatchResourceBuild.Plan Plan() => FightMatchResourceBuild.ReadPlan(File.ReadAllBytes(FightMatchResourceBuild.PlanPath));
        internal static byte[][] Inputs() => Plan().entries.Select(e => File.ReadAllBytes(e.source)).ToArray();
        internal static byte[] Boot(string platform = "macOS")
        {
            var bytes = File.ReadAllBytes(FightMatchResourceBuild.PlanPath);
            var plan = FightMatchResourceBuild.ReadPlan(bytes);
            return FightMatchResourceBuild.CreateBoot(plan, bytes, platform, Inputs(),
                Enumerable.Range(0, 8).Select(i => "raw-" + i + ".bytes").ToArray(), "test-manifest.bytes", TestManifest);
        }
        [Test]
        public void BuildMapIsDeterministicAndKeepsEightExactSources()
        {
            var plan = Plan(); var a = Boot(); var b = Boot();
            CollectionAssert.AreEqual(a, b);
            Assert.AreEqual(8, plan.entries.Select(e => e.assetId).Distinct().Count());
            Assert.AreEqual(2, plan.entries.Count(e => e.source.StartsWith("Generated/", StringComparison.Ordinal)));
            Assert.IsTrue(FightMatchResourceReleaseSet.TryDecodePinned(a, FightMatchResourceBuild.Hash(a), out var decoded, out var diagnostic), diagnostic);
            Assert.AreEqual("fm.builtin.r1.macos", decoded.ReleaseSetId);
            Assert.AreEqual("release-set:fightmatch-demo-r1", decoded.BusinessReleaseSetId);
            CollectionAssert.AreEqual(a, decoded.EncodeCanonical());
        }
        [Test]
        public void BuildPlanRefusesAlternateTextAuthorityAndUnknownTarget()
        {
            var plan = File.ReadAllText(FightMatchResourceBuild.PlanPath).Replace("Generated/FightMatch/Localization/fm-text-v1.json", "Assets/Other/text.json");
            Assert.Throws<InvalidDataException>(() => FightMatchResourceBuild.ReadPlan(Encoding.UTF8.GetBytes(plan)));
            Assert.Throws<ArgumentException>(() => FightMatchResourceBuild.Platform(BuildTarget.iOS));
        }
        [TestCase(BuildTarget.StandaloneOSX, "macOS", "fm.builtin.r1.macos")]
        [TestCase(BuildTarget.StandaloneWindows64, "windows", "fm.builtin.r1.windows")]
        [TestCase(BuildTarget.Android, "android", "fm.builtin.r1.android")]
        public void PlatformBootAndPlayerInventoryUseTheSameIsolatedRoot(BuildTarget target, string platform, string set)
        {
            Assert.AreEqual(platform, FightMatchResourceBuild.Platform(target));
            var bytes = Boot(platform);
            Assert.IsTrue(FightMatchResourceReleaseSet.TryDecodePinned(bytes, FightMatchResourceBuild.Hash(bytes), out var decoded, out var diagnostic), diagnostic);
            Assert.AreEqual(platform, decoded.Platform); Assert.AreEqual(set, decoded.ReleaseSetId);
            var expected = "Assets/StreamingAssets/FightMatchResources/" + platform + "/FightMatchMain";
            Assert.AreEqual(expected, FightMatchResourceBuild.PlayerRoot(target));
            Assert.AreEqual(expected, YooAssetRuntimeFactory.BuiltInRoot("Assets/StreamingAssets", platform, "FightMatchMain"));
            Assert.AreEqual("jar:file:///app.apk!/assets/FightMatchResources/" + platform + "/FightMatchMain",
                YooAssetRuntimeFactory.BuiltInRoot("jar:file:///app.apk!/assets", platform, "FightMatchMain"));
        }
        [Test]
        public void FactoryRejectsOtherPlatformBeforeCreatingRuntime()
        {
            var platform = UnityEngine.Application.platform == RuntimePlatform.WindowsEditor ? "macOS" : "windows";
            var bytes = Boot(platform);
            var result = YooAssetRuntimeFactory.Create(new ResourceBootstrapInput(bytes, FightMatchResourceBuild.Hash(bytes),
                "fm.builtin.r1." + platform.ToLowerInvariant(), platform, 1, new[] { "resource-schema-v1" }));
            Assert.IsFalse(result.IsAccepted); Assert.AreEqual("RES_SCHEMA", result.SafeCode); Assert.IsNull(result.Runtime);
            Assert.Throws<ArgumentException>(() => YooAssetRuntimeFactory.BuiltInRoot("Assets/StreamingAssets", "../macOS", "FightMatchMain"));
        }
    }
}
