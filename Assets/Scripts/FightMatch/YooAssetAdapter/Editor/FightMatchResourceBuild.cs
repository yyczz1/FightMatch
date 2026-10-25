using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using FightMatch.AssetAccess;
using UnityEditor;
using UnityEngine;
using YooAsset;
using YooAsset.Editor;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("FightMatch.AssetAccess.Tests")]

namespace FightMatch.YooAssetAdapter.Editor
{
    public static class FightMatchResourceBuild
    {
        internal const string PlanPath = "Assets/FightMatch/ResourceBuild/FightMatchResourcePlan.json";
        internal const string ChapterPlanPath = "Assets/FightMatch/ResourceBuild/FightMatchChapterOneResourcePlan.json";
        private const string OutputRoot = "TestArtifacts/FightMatch/RUNTIME-INTEGRATION-001/build";
        private const string ChapterOutputRoot = "TestArtifacts/FightMatch/CH1-IMPL-001/host-resources/r2";
        private const string Package = "FightMatchMain";
        private const string ChapterPackage = "FightMatchChapterOne";
        private static readonly string[] Names = { "first-release.fmsource.json", "first-release.fmpackage.bytes",
            "first-release.fmvalidation.bytes", "first-release.fmreview.json", "first-release.fmpublish.json",
            "first-release.fmrelease.json", "fm-text-v1.json", "fm-text-v1.manifest.json" };
        private static readonly string[] Ids = { "fm.content.source", "fm.content.package", "fm.content.validation",
            "fm.content.review", "fm.content.publication", "fm.content.release-set", "fm.text.full.artifact", "fm.text.full.manifest" };
        [Serializable] internal sealed class Plan
        {
            public int schemaVersion, appProtocol;
            public string packageName, packageVersion, resourceSetPrefix, appBuildIdentity, authorizationEvidenceId,
                operationId, textArtifactSha256, textManifestSha256;
            public string[] requiredCapabilities;
            public Entry[] entries;
        }
        [Serializable] internal sealed class Entry { public string assetId, source, importPath; }
        [Serializable] private sealed class Binding
        { public string ContentFingerprint, NumericContractVersion, PackageId, RandomContractVersion, RuleVersion; }
        [Serializable] private sealed class Receipt { public Binding Binding; }
        [Serializable] private sealed class Release { public string ReleaseSetId; }

        [MenuItem("FightMatch/Resources/Build BuiltIn Mac")]
        public static void BuildMac() => Build(BuildTarget.StandaloneOSX);
        [MenuItem("FightMatch/Resources/Build BuiltIn Windows")]
        public static void BuildWindows() => Build(BuildTarget.StandaloneWindows64);
        [MenuItem("FightMatch/Resources/Build BuiltIn Android")]
        public static void BuildAndroid() => Build(BuildTarget.Android);
        [MenuItem("FightMatch/Resources/Build ChapterOne Mac")]
        public static void BuildChapterMac() => Build(BuildTarget.StandaloneOSX, true);
        [MenuItem("FightMatch/Resources/Build ChapterOne Windows")]
        public static void BuildChapterWindows() => Build(BuildTarget.StandaloneWindows64, true);
        [MenuItem("FightMatch/Resources/Build ChapterOne Android")]
        public static void BuildChapterAndroid() => Build(BuildTarget.Android, true);

        // This is a normal Editor build: it never changes a scene, Player setting, collector setting or Player staging.
        public static void Build(BuildTarget target) => Build(target, false);
        private static void Build(BuildTarget target, bool chapter)
        {
            var platform = Platform(target);
            var planBytes = ReadBounded(chapter ? ChapterPlanPath : PlanPath, 32768);
            var plan = ReadPlan(planBytes, chapter);
            var input = plan.entries.Select(e => ReadBounded(e.source, 16777216)).ToArray();
            if (input.Sum(x => (long)x.Length) > 33554432 || Hash(input[6]) != plan.textArtifactSha256 ||
                Hash(input[7]) != plan.textManifestSha256) throw new InvalidDataException("Formal input identity differs.");
            for (var i = 6; i < 8; i++)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(plan.entries[i].importPath));
                File.WriteAllBytes(plan.entries[i].importPath, input[i]);
                AssetDatabase.ImportAsset(plan.entries[i].importPath, ImportAssetOptions.ForceSynchronousImport);
            }
            var output = Path.GetFullPath((chapter ? ChapterOutputRoot : OutputRoot) + "/" + platform);
            var map = new FixedRawMap(plan.entries, plan.packageName);
            var parameters = new RawFileBuildParameters
            {
                BuildOutputRoot = output + "/packages", BundledFileRoot = output + "/builtin",
                BuildPipeline = nameof(RawFileBuildPipeline), BuildBundleType = (int)EBundleType.RawBundle,
                BuildTarget = target, PackageName = plan.packageName, PackageVersion = plan.packageVersion,
                PackageNote = plan.appBuildIdentity, FileNameStyle = EFileNameStyle.HashName,
                BundledCopyOption = EBundledCopyOption.ClearAndCopyAll, VerifyBuildingResult = true
            };
            // Only collection is project-owned; all build, manifest, copy and catalog tasks are SDK tasks.
            var result = new BundleBuilder().Run(parameters, new List<IBuildTask>
            {
                new TaskPrepare_RFBP(), map, new TaskBuilding_RFBP(), new TaskEncryption_RFBP(),
                new TaskUpdateBundleInfo_RFBP(), new TaskCreateManifest_RFBP(), new TaskCreateReport_RFBP(),
                new TaskCreatePackage_RFBP(), new TaskCopyBundledFiles_RFBP(), new TaskCreateCatalog_RFBP()
            }, true);
            if (!result.Success) throw new InvalidOperationException("FightMatch raw package build failed: " + result.FailedTask);
            var physicalNames = new string[8];
            for (var i = 0; i < physicalNames.Length; i++)
            {
                var file = map.Map.GetBundleInfo(plan.entries[i].assetId + ".rawfile").PackageDestFilePath;
                var built = ReadBounded(file, 16777216);
                if (!built.SequenceEqual(input[i]) || !ReadBounded(plan.entries[i].source, 16777216).SequenceEqual(input[i]))
                    throw new InvalidDataException("Input changed during build.");
                physicalNames[i] = Path.GetFileName(file);
            }
            var manifestName = YooAssetConfiguration.GetManifestBinaryFileName(plan.packageName, plan.packageVersion);
            var manifest = ReadBounded(Path.Combine(result.OutputPackageDirectory, manifestName), 268435456);
            var boot = CreateBoot(plan, planBytes, platform, input, physicalNames, manifestName, manifest);
            File.WriteAllBytes(output + "/fm-resource-boot-v1.json", boot);
            var builtin = output + "/builtin/" + plan.packageName;
            File.WriteAllBytes(builtin + "/fm-text-source.receipt.json", input[7]);
            var inventory = Directory.GetFiles(builtin, "*", SearchOption.AllDirectories).OrderBy(x => x, StringComparer.Ordinal)
                .Select(path => Obj("source", path, "playerPath", PlayerRoot(target, chapter) + "/" +
                    path.Substring(builtin.Length + 1).Replace('\\', '/'), "length", new FileInfo(path).Length,
                    "sha256", Hash(ReadBounded(path, 268435456)))).ToArray();
            File.WriteAllBytes(output + "/inventory.json", Encode(Obj("schemaVersion", 1, "platform", platform,
                "resourceSetId", SetId(plan, platform), "appProtocol", plan.appProtocol,
                "bootPath", output + "/fm-resource-boot-v1.json", "bootSha256", Hash(boot),
                "pinAuthority", "Candidate only. Independently approve and embed this pin in Player assembly.",
                "playerFiles", inventory)));
            Debug.Log("FightMatch BuiltIn output: " + output + "; independent boot pin candidate=" + Hash(boot));
        }

        internal static Plan ReadPlan(byte[] bytes, bool chapter = false)
        {
            var plan = JsonUtility.FromJson<Plan>(new UTF8Encoding(false, true).GetString(bytes));
            if (plan == null || plan.schemaVersion != 1 || plan.packageName != (chapter ? ChapterPackage : Package) || plan.appProtocol != 1 ||
                plan.entries == null || plan.entries.Length != 8 || plan.requiredCapabilities == null ||
                !plan.requiredCapabilities.SequenceEqual(new[] { "resource-schema-v1" }) ||
                plan.textArtifactSha256 != (chapter ? "038a9fed918495f04bb259ab48bf157d55df0e1a81a37e649dd7469c1953177c" :
                    "f48264f9ce94cd3bb2ef8d1b6a0be7b241916144b6d5764d11633bb6d6e456bc") ||
                plan.textManifestSha256 != (chapter ? "4072b5f149e609b8fdb810ebc9c1fb46d5bda6374bf77a12d872188ce091a8b5" :
                    "18437127bc2e2d7371f97da0990de933f0193fe202539593ef19338d47123667"))
                throw new InvalidDataException("Unsupported resource build plan.");
            if (chapter && (plan.packageVersion != "chapter-one-trial-r2" || plan.resourceSetPrefix != "fm.ch1.trial.r2" ||
                plan.appBuildIdentity != "fightmatch-ch1-trial-r2" || plan.authorizationEvidenceId != "CH1-IMPL-001" ||
                plan.operationId != "build:fightmatch:chapter-one-trial-r2"))
                throw new InvalidDataException("Unexpected chapter build identity.");
            for (var i = 0; i < 8; i++)
            {
                var expected = i < 6 ? (chapter ? "Assets/StreamingAssets/FightMatchChapterOne/" : "Assets/StreamingAssets/FightMatch/") :
                    (chapter ? "Generated/FightMatch/Localization/ChapterOneTrial-r2/" : "Generated/FightMatch/Localization/");
                var imported = i < 6 ? expected : (chapter ? "Assets/FightMatch/ResourceBuild/Inputs/ChapterOneTrial-r2/" : "Assets/FightMatch/ResourceBuild/Inputs/");
                var entry = plan.entries[i];
                if (entry == null || entry.assetId != Ids[i] || entry.source != expected + Names[i] || entry.importPath != imported + Names[i])
                    throw new InvalidDataException("Unexpected raw input mapping.");
            }
            if (!FightMatchAssetId.TryCreate(plan.packageVersion, out _) || plan.packageVersion == "latest" ||
                !FightMatchAssetId.TryCreate(plan.resourceSetPrefix, out _) || plan.resourceSetPrefix == "latest")
                throw new InvalidDataException("Exact build identities required.");
            return plan;
        }
        internal static string Platform(BuildTarget target) => target == BuildTarget.Android ? "android" :
            target == BuildTarget.StandaloneOSX ? "macOS" : target == BuildTarget.StandaloneWindows64 ? "windows" :
            throw new ArgumentException("Unsupported BuiltIn platform.", nameof(target));
        internal static string PlayerRoot(BuildTarget target, bool chapter = false) =>
            "Assets/StreamingAssets/FightMatchResources/" + Platform(target) + "/" + (chapter ? ChapterPackage : Package);
        private static string SetId(Plan plan, string platform) => plan.resourceSetPrefix + "." + platform.ToLowerInvariant();

        private sealed class FixedRawMap : IBuildTask, IAssetIgnoreRule
        {
            private readonly Entry[] entries;
            private readonly string packageName;
            internal BuildMapContext Map { get; private set; }
            internal FixedRawMap(Entry[] entries, string packageName) { this.entries = entries; this.packageName = packageName; }
            public bool IsIgnoreAsset(EditorAssetInfo assetInfo) => false;
            public void Run(BuildContext context)
            {
                var command = new CollectCommand(packageName, this) { EnableAddressable = true };
                command.SetFlag(ECollectFlags.IgnoreGetDependencies, true);
                Map = new BuildMapContext { Command = command, AssetFileCount = entries.Length };
                foreach (var entry in entries.OrderBy(e => e.assetId, StringComparer.Ordinal))
                {
                    var asset = new BuildAssetInfo(ECollectorType.MainAssetCollector, entry.assetId + ".rawfile",
                        entry.assetId, new EditorAssetInfo(entry.importPath));
                    asset.SetDependAssetInfos(new List<BuildAssetInfo>());
                    Map.PackAsset(asset);
                }
                context.SetContextObject(Map);
            }
        }

        internal static byte[] CreateBoot(Plan plan, byte[] planBytes, string platform, byte[][] input,
            string[] physicalNames, string manifestName, byte[] manifest)
        {
            var set = SetId(plan, platform);
            var physical = new List<SortedDictionary<string, object>>();
            var mappings = new List<SortedDictionary<string, object>>();
            var business = new List<SortedDictionary<string, object>>();
            for (var i = 0; i < 8; i++)
            {
                physical.Add(FileRecord(physicalNames[i], input[i], "raw"));
                if (i < 6) business.Add(FileRecord(Names[i], input[i]));
                mappings.Add(Obj("assetId", Ids[i], "contentLength", input[i].LongLength,
                    "contentSha256", Hash(input[i]), "files", new[] { physicalNames[i] }, "kind", "raw",
                    "location", Ids[i], "packageName", plan.packageName, "platform", platform, "releaseSetId", set,
                    "scopeId", i < 6 ? "fm.content.first-release" : "fm.text.full", "unityType", ""));
            }
            mappings = mappings.OrderBy(m => (string)m["assetId"], StringComparer.Ordinal).ToList();
            physical.Add(FileRecord(manifestName, manifest, "manifest"));
            const string receiptName = "fm-text-source.receipt.json";
            physical.Add(FileRecord(receiptName, input[7], "receipt"));
            physical = physical.OrderBy(f => (string)f["name"], StringComparer.Ordinal).ToList();
            var scopes = new List<SortedDictionary<string, object>>();
            foreach (var scope in new[] { "fm.content.first-release", "fm.text.full" })
            {
                var assets = mappings.Where(m => (string)m["scopeId"] == scope).ToArray();
                var names = new HashSet<string>(assets.SelectMany(m => (string[])m["files"]), StringComparer.Ordinal);
                var files = physical.Where(f => names.Contains((string)f["name"])).ToArray();
                scopes.Add(Obj("scopeId", scope, "sha256", Hash(Encode(Obj("assets", assets, "files", files, "scopeId", scope)))));
            }
            var mapping = Obj("entries", mappings, "schemaVersion", 1);
            var receipt = Obj("authorizationEvidenceId", plan.authorizationEvidenceId, "businessInputsSha256", Hash(Encode(business)),
                "kind", "resource-inputs-receipt-v1", "operationId", plan.operationId, "schemaVersion", 1,
                "sourcePlanSha256", Hash(planBytes), "textSourceReceiptSha256", Hash(input[7]));
            var binding = JsonUtility.FromJson<Receipt>(Encoding.UTF8.GetString(input[4])).Binding;
            var businessSet = JsonUtility.FromJson<Release>(Encoding.UTF8.GetString(input[5])).ReleaseSetId;
            var chapter = plan.packageName == ChapterPackage;
            if (plan.packageName != Package && !chapter || businessSet != (chapter ? "release-set:chapter-one-trial" : "release-set:fightmatch-demo-r1"))
                throw new InvalidDataException("Unexpected business release.");
            if (chapter && (binding == null || binding.PackageId != "fightmatch:chapter-one-trial" || binding.RuleVersion != "ch1-slice-r1" ||
                binding.NumericContractVersion != "RC01" || binding.RandomContractVersion != "PC01+SC01"))
                throw new InvalidDataException("Unexpected chapter content binding.");
            var descriptor = Obj("businessContent", Obj("binding", Obj("ContentFingerprint", binding.ContentFingerprint,
                "NumericContractVersion", binding.NumericContractVersion, "PackageId", binding.PackageId,
                "RandomContractVersion", binding.RandomContractVersion, "RuleVersion", binding.RuleVersion),
                "businessReleaseSetId", businessSet, "contentReleaseSetSha256", Hash(input[5]), "files", business,
                "publicationReceiptSha256", Hash(input[4])),
                "codeCompatibility", Obj("appBuildIdentity", plan.appBuildIdentity, "baseProtocolVersion", plan.appProtocol,
                    "requiredCapabilities", plan.requiredCapabilities),
                "publication", Obj("authorizationEvidenceId", plan.authorizationEvidenceId, "operationId", plan.operationId,
                    "receipt", receipt, "receiptSha256", Hash(Encode(receipt))), "releaseSetId", set,
                "resources", Obj("manifestLength", manifest.LongLength, "manifestName", manifestName,
                    "manifestSha256", Hash(manifest), "mappingDescriptorSha256", Hash(Encode(mapping)), "packageName", plan.packageName,
                    "platform", platform, "requiredScopeHashes", scopes, "yooAssetPackageVersion", "3.0.6",
                    "yooManifestPackageVersion", plan.packageVersion), "schemaVersion", 1,
                "text", Obj("artifactLength", input[6].LongLength, "artifactName", Names[6], "artifactSha256", Hash(input[6]),
                    "manifestLength", input[7].LongLength, "manifestName", Names[7], "manifestSha256", Hash(input[7]),
                    "schemaVersion", 1, "sourceReceipt", FileRecord(receiptName, input[7])));
            var boot = Encode(Obj("descriptor", descriptor, "descriptorSha256", Hash(Encode(descriptor)),
                "mapping", mapping, "physicalFiles", physical, "schemaVersion", 1));
            if (!FightMatchResourceReleaseSet.TryDecodePinned(boot, Hash(boot), out _, out var code))
                throw new InvalidDataException("Generated resource boot rejected: " + code);
            return boot;
        }

        private static SortedDictionary<string, object> FileRecord(string name, byte[] bytes, string kind = null)
        {
            var record = Obj("length", bytes.LongLength, "name", name, "sha256", Hash(bytes));
            if (kind != null) record.Add("kind", kind);
            return record;
        }
        private static SortedDictionary<string, object> Obj(params object[] pairs)
        {
            var result = new SortedDictionary<string, object>(StringComparer.Ordinal);
            for (var i = 0; i < pairs.Length; i += 2) result.Add((string)pairs[i], pairs[i + 1]);
            return result;
        }
        private static byte[] Encode(object value)
        {
            var text = new StringBuilder(); Write(text, value);
            return new UTF8Encoding(false, true).GetBytes(text.ToString());
        }
        private static void Write(StringBuilder text, object value)
        {
            if (value is string str)
            {
                text.Append('"');
                foreach (var c in str)
                {
                    if (c == '"' || c == '\\') text.Append('\\').Append(c);
                    else if (c < 32) text.Append("\\u").Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
                    else text.Append(c);
                }
                text.Append('"');
            }
            else if (value is SortedDictionary<string, object> map)
            {
                text.Append('{'); var comma = false;
                foreach (var pair in map)
                { if (comma) text.Append(','); comma = true; Write(text, pair.Key); text.Append(':'); Write(text, pair.Value); }
                text.Append('}');
            }
            else if (value is IEnumerable list)
            {
                text.Append('['); var comma = false;
                foreach (var item in list) { if (comma) text.Append(','); comma = true; Write(text, item); }
                text.Append(']');
            }
            else if (value is long || value is int) text.Append(Convert.ToString(value, CultureInfo.InvariantCulture));
            else throw new InvalidDataException("Unsupported canonical resource value.");
        }
        internal static string Hash(byte[] bytes)
        { using (var sha = SHA256.Create()) return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", "").ToLowerInvariant(); }
        private static byte[] ReadBounded(string path, int maximum)
        {
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                if (stream.Length < 1 || stream.Length > maximum) throw new InvalidDataException("Resource input exceeds budget.");
                var bytes = new byte[checked((int)stream.Length)];
                var offset = 0;
                while (offset < bytes.Length)
                { var read = stream.Read(bytes, offset, bytes.Length - offset); if (read == 0) throw new EndOfStreamException(); offset += read; }
                if (stream.ReadByte() != -1) throw new InvalidDataException("Resource input changed length.");
                return bytes;
            }
        }
    }
}
