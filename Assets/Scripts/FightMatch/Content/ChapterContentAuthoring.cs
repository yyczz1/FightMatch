using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using FightMatch.Core;
using FightMatch.Platform;
using static FightMatch.Content.ContentChecks;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("FightMatch.Core.Tests")]

namespace FightMatch.Content
{
    // Fixed development release. This entry never supplies independent review authority itself.
    public static class ChapterContentAuthoring
    {
        public const string ReleaseSetId = "release-set:chapter-one-trial";
        public const string EvidenceRoot = "TestArtifacts/FightMatch/CH1-IMPL-001/host-content/r2";
        public const string PrepareRoot = EvidenceRoot + "/prepare";
        public const string ReleaseRoot = "Assets/StreamingAssets/FightMatchChapterOne";
        private const string AuthorRoot = "Config/FightMatch/ChapterOneTrial";
        private const string GeneratedRoot = "Generated/FightMatch/ChapterOneTrial/r2";
        private const string SeedPath = "Assets/FightMatchContent/demo-r1.source.json";
        private const string SeedSha = "fc2a6e8ec7cdba10992b9e0de7433020b75b1a4ef2b18b5570b199b9983a261a";
        private const string AuthorSha = "5faccf6b422ac9348a46a19d7783edb369949dc86b304e71bccad9b83ad29f19";
        private const string GeneratedSha = "b5174456caecddb1c7bb5ede0a2dd998c564b8e94716cfe6c5199e23db125d94";
        private const string PackageId = "fightmatch:chapter-one-trial", DraftId = "draft:chapter-one-trial";
        private const string Scope = "player";
        public const string R3EvidenceRoot = "TestArtifacts/FightMatch/CH1-IMPL-001/host-content/r3";
        public const string R3PrepareRoot = R3EvidenceRoot + "/prepare";
        public const string R3ReleaseRoot = "Assets/StreamingAssets/FightMatchChapterOne-r3";
        private sealed class Profile
        {
            internal readonly int Revision, LevelCount;
            internal readonly string AuthorRoot, GeneratedRoot, AuthorSha, GeneratedSha, EvidenceRoot, ReleaseRoot;
            internal string PrepareRoot => EvidenceRoot + "/prepare";
            internal string ReviewPath => EvidenceRoot + "/approved-review.json";
            internal string PublicationRoot => EvidenceRoot + "/publication";
            internal string Operation => "publish:chapter-one-trial:r" + Revision;
            internal Profile(int revision, int levels, string authorRoot, string generatedRoot, string authorSha,
                string generatedSha, string evidenceRoot, string releaseRoot)
            {
                Revision = revision; LevelCount = levels; AuthorRoot = authorRoot; GeneratedRoot = generatedRoot;
                AuthorSha = authorSha; GeneratedSha = generatedSha; EvidenceRoot = evidenceRoot; ReleaseRoot = releaseRoot;
            }
        }
        private static readonly Profile R2 = new Profile(2, 9, AuthorRoot, GeneratedRoot, AuthorSha,
            GeneratedSha, EvidenceRoot, ReleaseRoot);
        private static readonly Profile R3 = new Profile(3, 16, "Config/FightMatch/ChapterOneTrial-r3",
            "Generated/FightMatch/ChapterOneTrial/r3", "867a9ab630e8d6d6201fe3d5d8b71c8db4cdec016c5b348967242523eebf7462",
            "c7c034446902183a407d3a7c0a763d671d1af84476e9c63890ad8e6644201bb4", R3EvidenceRoot, R3ReleaseRoot);
        private const int Limit = 16 * 1024 * 1024;
        private static readonly string[] Tables = { "Release", "CharacterGrowth", "Passive", "Level", "Face", "PairEnemy",
            "EnemyAction", "DefaultRouteCell", "Item", "Card", "Skill", "Teaching", "Reward", "RewardMaterial",
            "CharacterUnlock", "FirstClearItem", "SourceMetadata" };
        private static readonly string[] ReleaseNames = { "first-release.fmsource.json", "first-release.fmpackage.bytes",
            "first-release.fmvalidation.bytes", "first-release.fmreview.json", "first-release.fmpublish.json", "first-release.fmrelease.json" };
        private static ContentConsumerCapabilities Caps => ContentConsumerCapabilities.Current;
        private static ExactMathBudget Math() => new ExactMathBudget(maxPrimitiveSteps: 16000000);
        private static ContentStoreBudget Budget() => new ContentStoreBudget(Math());
        private static T Take<T>(PublicationResult<T> result)
        { Need(result.IsAccepted, result.RejectionCode, result.FieldPath); return result.Value; }
        private static PreparedPublication Take(PreparedPublicationResult result)
        { Need(result.IsAccepted, result.RejectionCode, result.FieldPath); return result.Prepared; }

        // Read/compile only; callers cannot redirect the seed, revision or authoring inputs.
        public static PreparedPublicationResult Prepare() => Prepare(R2);
        public static PreparedPublicationResult PrepareR3() => Prepare(R3);
        private static PreparedPublicationResult Prepare(Profile profile)
        {
            return new PreparedPublicationResult(PublicationResult<PreparedPublication>.Run(() => {
                CheckInputs(profile);
                var seed = Take(PublishedContentCodec.DecodeSource(Read(SeedPath), Caps, Math()));
                var source = Take(ChapterContentTables.Read(SafePath(profile.GeneratedRoot), SafePath(profile.AuthorRoot), seed, Caps, Math()));
                Need(source.PackageId == PackageId && source.DraftId == DraftId && source.Revision == profile.Revision && source.SchemaVersion == 2 &&
                    source.RuleVersion == ChapterRuleDefinitions.RuleVersion && source.NumericContractVersion == "RC01" &&
                    source.RandomContractVersion == "PC01+SC01" && source.Chapter.ReleaseSetId == ReleaseSetId &&
                    source.Chapter.ParameterStatus == ChapterParameterStatus.Trial, "UnsupportedBinding", "Chapter.Release");
                var draft = new DemoContentDraft(DraftId);
                for (var revision = 1; revision < profile.Revision; revision++) draft.Revise();
                var prepared = Take(PublishedContentCompiler.Prepare(source, draft.BeginJob(), Caps, Math()));
                Need(prepared.Definitions.Levels.Count == profile.LevelCount &&
                    prepared.Definitions.Chapter.HasTauntBossState == (profile.Revision == 3), "UnsupportedBinding", "Chapter.Profile");
                CheckInputs(profile); return prepared;
            }));
        }

        public static void Run() => RunArguments(Environment.GetCommandLineArgs());
        public static void RunR3() => RunArguments(Environment.GetCommandLineArgs(), R3);
        internal static void RunArguments(string[] args) => RunArguments(args, R2);
        internal static void RunR3Arguments(string[] args) => RunArguments(args, R3);
        private static void RunArguments(string[] args, Profile profile)
        {
            string mode = null;
            for (var i = 0; i < args.Length; i++) if (args[i].StartsWith("-fm", StringComparison.Ordinal))
            {
                Need(args[i] == "-fmChapterMode" && mode == null && i + 1 < args.Length, "InvalidValue", "Arguments");
                mode = args[++i];
            }
            Need(mode == "prepare" || mode == "publish" || mode == "activate-export" || mode == "verify-release",
                "UnsupportedBinding", "Mode");
            if (mode == "verify-release") { VerifyRelease(profile); return; }
            var prepared = Take(Prepare(profile));
            if (mode == "prepare")
            {
                var files = PreparationFiles(prepared, profile);
                foreach (var name in files.Keys) Need(!File.Exists(SafePath(profile.PrepareRoot + "/" + name)), "OutputExists", name);
                foreach (var file in files) WriteSameOrNew(profile.PrepareRoot + "/" + file.Key, file.Value);
                Console.WriteLine("PREPARED " + prepared.PayloadSha256 + "; independent review required"); return;
            }
            Same(Read(profile.PrepareRoot + "/source-copy.json"), prepared.SourceBytes.ToArray(), "Prepared.Source");
            Same(Read(profile.PrepareRoot + "/prepared.fmpackage.bytes"), prepared.PayloadBytes.ToArray(), "Prepared.Payload");
            Same(Read(profile.PrepareRoot + "/prepared.fmvalidation.bytes"), prepared.Validation.Bytes.ToArray(), "Prepared.Validation");
            var storage = Storage(profile); var catalog = new PublishedContentCatalog(storage, Caps);
            if (mode == "publish")
            {
                var before = catalog.GetCurrentBinding(Scope, ReleaseSetId, Caps);
                Need(!before.IsAccepted && before.RejectionCode == "UnsupportedBinding" && before.FieldPath == "ReleaseSet",
                    "RecoveryBlocked", "Publish.AlreadyActive");
                var outcome = Take(PublishedContentAuthoring.Publish(catalog, prepared, SafePath(profile.ReviewPath), prepared.Job, profile.Operation, Budget()));
                var after = catalog.GetCurrentBinding(Scope, ReleaseSetId, Caps);
                Need(!after.IsAccepted && after.RejectionCode == "UnsupportedBinding" && after.FieldPath == "ReleaseSet",
                    "RecoveryBlocked", "Publish.Activation");
                CheckResolved(outcome.Publication, profile);
                WriteSameOrNew(profile.PublicationRoot + "/publication-result.json", Json(new { outcome.Status, outcome.OperationId,
                    outcome.Publication.Binding, ParameterStatus = prepared.ParameterStatus, ReleaseActivated = false }));
                Console.WriteLine("PUBLISHED " + profile.Operation + "; release is not active"); return;
            }
            var resolved = Take(catalog.ResolveExact(prepared.Binding, Caps)); CheckResolved(resolved, profile);
            var record = PublishedContentCodec.Decode<PublicationRecord>(resolved.ReceiptBytes.ToArray(), 65536, Caps, Math());
            var exported = new[] { storage.Read(PublishedContentCatalog.Key("source", record.SourceSha256), Limit),
                storage.Read(PublishedContentCatalog.Key("payload", record.PayloadSha256), Limit),
                storage.Read(PublishedContentCatalog.Key("validation", record.ValidationSha256), Limit),
                storage.Read(PublishedContentCatalog.Key("review", record.ReviewSha256), Limit), resolved.ReceiptBytes.ToArray(),
                Take(PublishedContentCodec.EncodeReleaseSet(new ContentReleaseSet { SchemaVersion = 1, Scope = Scope,
                    ReleaseSetId = ReleaseSetId, Binding = ContentBindingRecord.From(resolved.Binding),
                    PublicationReceiptSha256 = PublishedContentCodec.Sha256(resolved.ReceiptBytes.ToArray()) }, Budget())) };
            // Admit the exact six files before activating or exporting any of them.
            var cold = Take(FirstReleaseContentStorage.Create(exported[0], exported[1], exported[2], exported[3], exported[4], exported[5], Caps, Budget()));
            CheckResolved(Take(new PublishedContentCatalog(cold, Caps).ResolveExact(resolved.Binding, Caps)), profile);
            for (var i = 0; i < ReleaseNames.Length; i++)
                if (File.Exists(SafePath(profile.ReleaseRoot + "/" + ReleaseNames[i]))) Same(Read(profile.ReleaseRoot + "/" + ReleaseNames[i]), exported[i], "Export.Existing");
            using (storage.AcquireWriter())
            {
                var key = PublishedContentCatalog.ReleaseSetKey(Scope, ReleaseSetId);
                storage.WriteImmutable(key, exported[5], Limit); Same(storage.Read(key, Limit), exported[5], "Release.ReadBack");
            }
            Need(prepared.Binding.Same(Take(catalog.GetCurrentBinding(Scope, ReleaseSetId, Caps))), "RecoveryBlocked", "Release.Current");
            for (var i = 0; i < ReleaseNames.Length; i++) WriteSameOrNew(profile.ReleaseRoot + "/" + ReleaseNames[i], exported[i]);
            WriteSameOrNew(profile.PublicationRoot + "/export-manifest.json", Json(new { Status = "ActivatedAndExported", Scope,
                ReleaseSetId, resolved.Binding, Files = ReleaseNames.Select((name, i) => new { Name = name, Bytes = exported[i].Length,
                    Sha256 = PublishedContentCodec.Sha256(exported[i]) }).ToArray() }));
            Console.WriteLine("ACTIVATED_EXPORTED " + ReleaseSetId);
        }

        internal static Dictionary<string, byte[]> PreparationFiles(PreparedPublication prepared) => PreparationFiles(prepared, R2);
        internal static Dictionary<string, byte[]> PreparationFilesR3(PreparedPublication prepared) => PreparationFiles(prepared, R3);
        private static Dictionary<string, byte[]> PreparationFiles(PreparedPublication prepared, Profile profile)
        {
            Need(prepared.Revision == profile.Revision, "InconsistentBinding", "Chapter.Profile");
            var source = Take(PublishedContentCodec.DecodeSource(prepared.PayloadBytes.ToArray(), Caps, Math()));
            var built = PublishedContentCompiler.Build(source, prepared.Binding, prepared.Job, Caps, Math(), false);
            var evidence = PublishedContentCompiler.EvidenceBytes(built, Math());
            var validation = PublishedContentCodec.Decode<ValidationRecord>(prepared.Validation.Bytes.ToArray(), 65536, Caps, Math());
            Need(validation.EvidenceSha256 == PublishedContentCodec.Sha256(evidence), "RecoveryBlocked", "Validation.Evidence");
            return new Dictionary<string, byte[]>(StringComparer.Ordinal) {
                ["source-copy.json"] = prepared.SourceBytes.ToArray(),
                ["prepared.fmpackage.bytes"] = prepared.PayloadBytes.ToArray(),
                ["prepared.fmvalidation.bytes"] = prepared.Validation.Bytes.ToArray(),
                ["prepared-descriptor.json"] = Json(new { Status = "PreparedPendingIndependentReview", prepared.DraftId, prepared.Revision,
                    prepared.Binding, prepared.DefinitionBindings, prepared.SourceSha256, SourceBytes = prepared.SourceBytes.Count,
                    prepared.PayloadSha256, PayloadBytes = prepared.PayloadBytes.Count, ValidationSha256 = prepared.Validation.Sha256,
                    ValidationBytes = prepared.Validation.Bytes.Count, prepared.ParameterStatus, prepared.ReleaseSetId }),
                // The legacy Growth accessor requires one definition; this chapter has two explicit growths.
                ["field-map.json"] = Json(new { DecodedSource = source, ResolvedDefinitions = new {
                    prepared.Definitions.Binding, prepared.Definitions.Growths, prepared.Definitions.Inventory,
                    prepared.Definitions.Progression, prepared.Definitions.Levels, prepared.Definitions.Rewards,
                    prepared.Definitions.Chapter }, prepared.NewProfile }),
                ["chapter-evidence.json"] = evidence,
                ["review-template.json"] = Take(PublishedContentCodec.EncodeReview(new ContentReviewEvidence { SchemaVersion = 1,
                    Verdict = "UNREVIEWED", DraftId = prepared.DraftId, Revision = prepared.Revision, Binding = ContentBindingRecord.From(prepared.Binding),
                    DefinitionBindings = prepared.DefinitionBindings.Select(DefinitionBindingRecord.From).ToList(), SourceBytes = prepared.SourceBytes.Count,
                    SourceSha256 = prepared.SourceSha256, PayloadBytes = prepared.PayloadBytes.Count, PayloadSha256 = prepared.PayloadSha256,
                    ValidationBytes = prepared.Validation.Bytes.Count, ValidationSha256 = prepared.Validation.Sha256 }, Budget())),
                ["result.json"] = Json(new { Status = "PreparedPendingIndependentReview", prepared.Binding,
                    GeometryValidatedLevels = built.LevelBindings.Count, ExactParameterRows = built.Parameters.Count,
                    BattleReplayPerformed = false, PublicationInvoked = false, AuthorCsvSetSha256 = profile.AuthorSha, GeneratedJsonSetSha256 = profile.GeneratedSha })
            };
        }

        private static void VerifyRelease(Profile profile)
        {
            var files = ReleaseNames.Select(name => Read(profile.ReleaseRoot + "/" + name)).ToArray();
            var storage = Take(FirstReleaseContentStorage.Create(files[0], files[1], files[2], files[3], files[4], files[5], Caps, Budget()));
            var catalog = new PublishedContentCatalog(storage, Caps);
            var binding = Take(catalog.GetCurrentBinding(Scope, ReleaseSetId, Caps));
            var resolved = Take(catalog.ResolveExact(binding, Caps)); CheckResolved(resolved, profile);
            WriteSameOrNew(profile.PublicationRoot + "/cold-load-result.json", Json(new { Status = "ColdLoaded", binding,
                resolved.ReleaseSetId, resolved.ParameterStatus, OnlySixFiles = true, AuthoringSourceRead = false, PublicationStoreRead = false }));
            Console.WriteLine("COLD_LOADED " + binding.ContentFingerprint);
        }
        private static void CheckResolved(ResolvedPublication publication, Profile profile)
        {
            var record = PublishedContentCodec.Decode<PublicationRecord>(publication.ReceiptBytes.ToArray(), 65536, Caps, Math());
            Need(record.DraftId == DraftId && record.Revision == profile.Revision && record.OperationId == profile.Operation &&
                publication.Binding.PackageId == PackageId && publication.Binding.RuleVersion == ChapterRuleDefinitions.RuleVersion &&
                publication.Binding.NumericContractVersion == "RC01" && publication.Binding.RandomContractVersion == "PC01+SC01" &&
                publication.ReleaseSetId == ReleaseSetId && publication.ParameterStatus == ChapterParameterStatus.Trial &&
                publication.Definitions.Chapter != null && publication.Definitions.Levels.Count == profile.LevelCount &&
                publication.Definitions.Chapter.HasTauntBossState == (profile.Revision == 3) &&
                publication.NewProfile.CharacterId == "W", "UnsupportedBinding", "Chapter.Publication");
        }
        private static IContentPublicationStorage Storage(Profile profile)
        {
            var root = SafePath(profile.PublicationRoot + "/publication-store");
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) return new MacContentPublicationStorage(root);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return new WindowsContentPublicationStorage(root);
            throw new ContentFailure("UnsupportedCapability", "Authoring.Platform");
        }
        private static void CheckInputs(Profile profile)
        {
            Need(PublishedContentCodec.Sha256(Read(SeedPath)) == SeedSha, "InconsistentBinding", "Chapter.SeedSha256");
            Need(CollectionSha(profile.AuthorRoot, ".csv") == profile.AuthorSha, "InconsistentBinding", "Chapter.AuthorCsvSetSha256");
            Need(CollectionSha(profile.GeneratedRoot, ".json") == profile.GeneratedSha, "InconsistentBinding", "Chapter.GeneratedJsonSetSha256");
        }
        private static string CollectionSha(string directory, string extension)
        {
            var names = Tables.Select(name => name + extension).OrderBy(name => name, StringComparer.Ordinal).ToArray();
            Need(Directory.GetFiles(SafePath(directory), "*" + extension).Select(Path.GetFileName).OrderBy(name => name, StringComparer.Ordinal)
                .SequenceEqual(names), "InconsistentBinding", "Chapter.InputFiles");
            return PublishedContentCodec.Sha256(Encoding.UTF8.GetBytes(string.Concat(names.Select(name => name + ":" +
                PublishedContentCodec.Sha256(Read(directory + "/" + name)) + "\n"))));
        }
        private static byte[] Json(object value) => PublishedContentCodec.Encode(value, Limit,
            new ContentConsumerCapabilities(Caps.Capabilities, maxCollectionEntries: 65536), Math());
        private static byte[] Read(string relative) => PublishedContentCodec.ReadBounded(SafePath(relative), Limit);
        private static void Same(byte[] a, byte[] b, string field)
        { Need(ContentPublicationStorage.Equal(a, b), "RecoveryBlocked", field); }
        private static string SafePath(string relative)
        {
            Need(!Path.IsPathRooted(relative), "InvalidValue", "RelativePath");
            var root = Path.GetFullPath(Directory.GetCurrentDirectory()); var full = Path.GetFullPath(Path.Combine(root, relative));
            Need(full.StartsWith(root.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar, StringComparison.Ordinal), "InvalidValue", "Path");
            for (var current = full; current != null; current = Path.GetDirectoryName(current))
            {
                try { Need((File.GetAttributes(current) & FileAttributes.ReparsePoint) == 0, "InvalidValue", "ReparsePath"); }
                catch (FileNotFoundException) { } catch (DirectoryNotFoundException) { }
            }
            return full;
        }
        private static void WriteSameOrNew(string relative, byte[] bytes)
        {
            var path = SafePath(relative); Need(bytes != null && bytes.Length <= Limit, "BudgetExceeded", "Output");
            if (File.Exists(path)) { Same(Read(relative), bytes, "Output.Existing"); return; }
            Directory.CreateDirectory(Path.GetDirectoryName(path)); SafePath(relative);
            using (var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            { stream.Write(bytes, 0, bytes.Length); stream.Flush(true); }
            Same(Read(relative), bytes, "Output.ReadBack");
        }
    }
}
