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
        // Opaque metadata fixtures only: these six bytestrings are not an admitted publication or a real review.
        private static byte[][] ChapterEncoderInputs(FightMatchResourceBuild.Plan plan)
        {
            var input = Enumerable.Range(0, 8).Select(i => Encoding.UTF8.GetBytes("chapter-encoder-fixture-" + i)).ToArray();
            input[3] = Encoding.UTF8.GetBytes("UNREVIEWED encoder fixture");
            input[4] = Encoding.UTF8.GetBytes("{\"Binding\":{\"ContentFingerprint\":\"" + FightMatchResourceBuild.Hash(input[1]) +
                "\",\"PackageId\":\"fightmatch:chapter-one-trial\",\"RuleVersion\":\"ch1-slice-r1\",\"NumericContractVersion\":\"RC01\",\"RandomContractVersion\":\"PC01+SC01\"}}");
            input[5] = Encoding.UTF8.GetBytes("{\"ReleaseSetId\":\"release-set:chapter-one-trial\"}");
            input[6] = File.ReadAllBytes(plan.entries[6].source); input[7] = File.ReadAllBytes(plan.entries[7].source);
            return input;
        }
        [Test]
        public void ChapterPlanKeepsTwoIsolatedFixedMappingsAndActualTextPins()
        {
            var bytes = File.ReadAllBytes(FightMatchResourceBuild.ChapterPlanPath);
            var plan = FightMatchResourceBuild.ReadPlan(bytes, true);
            Assert.AreEqual("FightMatchChapterOne", plan.packageName); Assert.AreEqual(8, plan.entries.Length);
            Assert.AreEqual(plan.textArtifactSha256, FightMatchResourceBuild.Hash(File.ReadAllBytes(plan.entries[6].source)));
            Assert.AreEqual(plan.textManifestSha256, FightMatchResourceBuild.Hash(File.ReadAllBytes(plan.entries[7].source)));
            Assert.IsFalse(plan.entries.Select(e => e.importPath).Intersect(Plan().entries.Select(e => e.importPath)).Any());
            Assert.Throws<InvalidDataException>(() => FightMatchResourceBuild.ReadPlan(bytes));
            Assert.Throws<InvalidDataException>(() => FightMatchResourceBuild.ReadPlan(File.ReadAllBytes(FightMatchResourceBuild.PlanPath), true));
            foreach (var changed in new[] { Encoding.UTF8.GetString(bytes).Replace("FightMatchChapterOne", "ArbitraryPackage"),
                Encoding.UTF8.GetString(bytes).Replace("Assets/StreamingAssets/FightMatchChapterOne/", "Assets/StreamingAssets/FightMatch/"),
                Encoding.UTF8.GetString(bytes).Replace("Generated/FightMatch/Localization/ChapterOneTrial-r2/", "Generated/FightMatch/Localization/") })
                Assert.Throws<InvalidDataException>(() => FightMatchResourceBuild.ReadPlan(Encoding.UTF8.GetBytes(changed), true));
        }
        [TestCase(BuildTarget.StandaloneOSX, "macOS")]
        [TestCase(BuildTarget.StandaloneWindows64, "windows")]
        [TestCase(BuildTarget.Android, "android")]
        public void ChapterEncoderAndRuntimeUseTheSameSeparatePackageRoot(BuildTarget target, string platform)
        {
            var bytes = File.ReadAllBytes(FightMatchResourceBuild.ChapterPlanPath);
            var plan = FightMatchResourceBuild.ReadPlan(bytes, true);
            var boot = FightMatchResourceBuild.CreateBoot(plan, bytes, platform, ChapterEncoderInputs(plan),
                Enumerable.Range(0, 8).Select(i => "chapter-fixture-" + i + ".bytes").ToArray(), "test-manifest.bytes", TestManifest);
            Assert.IsTrue(FightMatchResourceReleaseSet.TryDecodePinned(boot, FightMatchResourceBuild.Hash(boot), out var decoded, out var code), code);
            Assert.AreEqual("fm.ch1.trial.r2." + platform.ToLowerInvariant(), decoded.ReleaseSetId);
            Assert.AreEqual("release-set:chapter-one-trial", decoded.BusinessReleaseSetId);
            var root = "Assets/StreamingAssets/FightMatchResources/" + platform + "/FightMatchChapterOne";
            Assert.AreEqual(root, FightMatchResourceBuild.PlayerRoot(target, true));
            Assert.AreEqual(root, YooAssetRuntimeFactory.BuiltInRoot("Assets/StreamingAssets", platform, "FightMatchChapterOne"));
            Assert.AreNotEqual(root, FightMatchResourceBuild.PlayerRoot(target));
        }
        [TestCase(4)]
        [TestCase(5)]
        public void ChapterEncoderRejectsLegacyBusinessBinding(int index)
        {
            var bytes = File.ReadAllBytes(FightMatchResourceBuild.ChapterPlanPath);
            var plan = FightMatchResourceBuild.ReadPlan(bytes, true); var input = ChapterEncoderInputs(plan);
            input[index] = index == 4 ? Encoding.UTF8.GetBytes(Encoding.UTF8.GetString(input[4]).Replace("ch1-slice-r1", "demo-r1")) :
                Encoding.UTF8.GetBytes("{\"ReleaseSetId\":\"release-set:fightmatch-demo-r1\"}");
            Assert.Throws<InvalidDataException>(() => FightMatchResourceBuild.CreateBoot(plan, bytes, "macOS", input,
                Enumerable.Range(0, 8).Select(i => "chapter-fixture-" + i + ".bytes").ToArray(), "test-manifest.bytes", TestManifest));
        }

        [Test]
        public void R3PlanUsesOnlyItsNewPathsAndExact310TextPins()
        {
            var bytes = File.ReadAllBytes(FightMatchResourceBuild.ChapterR3PlanPath);
            var plan = FightMatchResourceBuild.ReadChapterR3Plan(bytes);
            var r2 = FightMatchResourceBuild.ReadPlan(File.ReadAllBytes(FightMatchResourceBuild.ChapterPlanPath), true);
            Assert.AreEqual("FightMatchChapterOneR3", plan.packageName);
            Assert.AreEqual("chapter-one-trial-r3", plan.packageVersion);
            Assert.AreEqual(8, plan.entries.Length);
            Assert.AreEqual(plan.textArtifactSha256, FightMatchResourceBuild.Hash(File.ReadAllBytes(plan.entries[6].source)));
            Assert.AreEqual(plan.textManifestSha256, FightMatchResourceBuild.Hash(File.ReadAllBytes(plan.entries[7].source)));
            Assert.IsFalse(plan.entries.Select(e => e.importPath).Intersect(r2.entries.Select(e => e.importPath)).Any());
            Assert.IsFalse(plan.entries.Select(e => e.importPath).Intersect(Plan().entries.Select(e => e.importPath)).Any());
            Assert.Throws<InvalidDataException>(() => FightMatchResourceBuild.ReadPlan(bytes, true));
            Assert.Throws<InvalidDataException>(() => FightMatchResourceBuild.ReadChapterR3Plan(File.ReadAllBytes(FightMatchResourceBuild.ChapterPlanPath)));
            foreach (var changed in new[] { Encoding.UTF8.GetString(bytes).Replace("FightMatchChapterOneR3", "FightMatchChapterOne"),
                Encoding.UTF8.GetString(bytes).Replace("FightMatchChapterOne-r3/", "FightMatchChapterOne/"),
                Encoding.UTF8.GetString(bytes).Replace("ChapterOneTrial-r3/", "ChapterOneTrial-r2/") })
                Assert.Throws<InvalidDataException>(() => FightMatchResourceBuild.ReadChapterR3Plan(Encoding.UTF8.GetBytes(changed)));
        }

        [TestCase(BuildTarget.StandaloneOSX, "macOS")]
        [TestCase(BuildTarget.StandaloneWindows64, "windows")]
        [TestCase(BuildTarget.Android, "android")]
        public void R3EncoderKeepsAllEightMappingsInItsOwnRuntimePackage(BuildTarget target, string platform)
        {
            var bytes = File.ReadAllBytes(FightMatchResourceBuild.ChapterR3PlanPath);
            var plan = FightMatchResourceBuild.ReadChapterR3Plan(bytes);
            var boot = FightMatchResourceBuild.CreateBoot(plan, bytes, platform, ChapterEncoderInputs(plan),
                Enumerable.Range(0, 8).Select(i => "chapter-r3-fixture-" + i + ".bytes").ToArray(), "test-manifest.bytes", TestManifest);
            Assert.IsTrue(FightMatchResourceReleaseSet.TryDecodePinned(boot, FightMatchResourceBuild.Hash(boot), out var decoded, out var code), code);
            Assert.AreEqual("fm.ch1.trial.r3." + platform.ToLowerInvariant(), decoded.ReleaseSetId);
            Assert.AreEqual("release-set:chapter-one-trial", decoded.BusinessReleaseSetId);
            var root = FightMatchResourceBuild.PlayerRootR3(target);
            Assert.AreEqual("Assets/StreamingAssets/FightMatchResources/" + platform + "/FightMatchChapterOneR3", root);
            Assert.AreEqual(root, YooAssetRuntimeFactory.BuiltInRoot("Assets/StreamingAssets", platform, "FightMatchChapterOneR3"));
            Assert.AreNotEqual(root, FightMatchResourceBuild.PlayerRoot(target, true));
            Assert.AreNotEqual(root, FightMatchResourceBuild.PlayerRoot(target));
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
