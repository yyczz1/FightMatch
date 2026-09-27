using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FightMatch.Core;
using FightMatch.Platform;
using static FightMatch.Content.ContentChecks;

namespace FightMatch.Content
{
    public static class PublishedContentAuthoring
    {
        public static PreparedPublicationResult Prepare(string sourcePath, DemoContentJob job, ContentConsumerCapabilities capabilities, ExactMathBudget budget)
        {
            var decoded = PublishedContentCodec.DecodeSource(PublishedContentCodec.ReadBounded(sourcePath, capabilities.MaxSourceBytes), capabilities, budget);
            if (!decoded.IsAccepted) return new PreparedPublicationResult(new PublicationResult<PreparedPublication>(decoded.RejectionCode, decoded.FieldPath));
            return PublishedContentCompiler.Prepare(decoded.Value, job, capabilities, budget);
        }
        public static PublicationResult<ContentPublicationOutcome> Publish(PublishedContentCatalog catalog, PreparedPublication prepared,
            string reviewPath, DemoContentJob job, string operationId, ContentStoreBudget budget)
        {
            var review = PublishedContentCodec.DecodeReview(PublishedContentCodec.ReadBounded(reviewPath, Math.Min(65536, budget.MaxRecordBytes)), budget);
            return review.IsAccepted ? catalog.Publish(prepared, prepared.Validation, review.Value, job, operationId, budget)
                : new PublicationResult<ContentPublicationOutcome>(review.RejectionCode, review.FieldPath);
        }
        // Unity executeMethod entry. A thrown error is intentionally observable as a failed batch process.
        public static void Run() => RunArguments(Environment.GetCommandLineArgs());
        private static void RunArguments(string[] args)
        {
            var values = new Dictionary<string, string>(StringComparer.Ordinal);
            for (var i = 0; i < args.Length; i++) if (args[i].StartsWith("-fm", StringComparison.Ordinal))
            {
                var key = args[i]; Need(new[] { "-fmMode", "-fmSource", "-fmEvidenceRoot", "-fmReview", "-fmStoreRoot", "-fmOperationId", "-fmScope", "-fmReleaseSetId", "-fmReleaseRoot" }.Contains(key) &&
                    !values.ContainsKey(key) && i + 1 < args.Length, "InvalidValue", "Arguments"); values.Add(key, args[++i]);
            }
            Need(values.ContainsKey("-fmMode"), "MissingField", "Mode");
            if (values["-fmMode"] != "prepare") { RunPublication(values); return; }
            Need(values.Count == 3 && values.ContainsKey("-fmSource") && values.ContainsKey("-fmEvidenceRoot"), "UnsupportedBinding", "Mode");
            var sourcePath = CanonicalPath(values["-fmSource"]); var root = CanonicalPath(values["-fmEvidenceRoot"]);
            var caps = ContentConsumerCapabilities.Current; var math = new ExactMathBudget(maxPrimitiveSteps: 16000000);
            var sourceBytes = PublishedContentCodec.ReadBounded(sourcePath, caps.MaxSourceBytes);
            var decoded = PublishedContentCodec.DecodeSource(sourceBytes, caps, math); Need(decoded.IsAccepted, decoded.RejectionCode, decoded.FieldPath);
            var job = new DemoContentDraft(decoded.Value.DraftId).BeginJob();
            var result = PublishedContentCompiler.Prepare(decoded.Value, job, caps, math); Need(result.IsAccepted, result.RejectionCode, result.FieldPath);
            var p = result.Prepared;
            var evidenceCaps = new ContentConsumerCapabilities(caps.Capabilities, maxCollectionEntries: 65536);
            Func<object, byte[]> json = x => PublishedContentCodec.Encode(x, 16 * 1024 * 1024, evidenceCaps, math);
            var files = new Dictionary<string, byte[]>(StringComparer.Ordinal) {
                ["source-copy.json"] = sourceBytes,
                ["prepared.fmpackage.bytes"] = p.PayloadBytes.ToArray(),
                ["prepared.fmvalidation.bytes"] = p.Validation.Bytes.ToArray(),
                ["prepared-descriptor.json"] = json(new { Status = "PreparedPendingIndependentReview", p.DraftId, p.Revision,
                    p.Binding, p.DefinitionBindings, p.SourceSha256, SourceBytes = p.SourceBytes.Count, p.PayloadSha256, PayloadBytes = p.PayloadBytes.Count,
                    ValidationSha256 = p.Validation.Sha256, ValidationBytes = p.Validation.Bytes.Count, p.NewProfile }),
                ["field-map.json"] = json(new { DecodedSource = decoded.Value, ResolvedDefinitions = p.Definitions, FrozenNewProfile = p.NewProfile,
                    Meaning = "Decoded input and actual shared-kernel projection; approved original-field mapping is bound externally by the author report." }),
                ["geometry.json"] = json(p.Replays.Select(r => new { r.Candidate.Coordinates, r.Candidate.Routes, Level = r.Candidate.Entry.GetDefinitionBinding(),
                    Completed = r.Run.CurrentSnapshot.Board.LockedRoutes.Count }).ToList()),
                ["replay.json"] = PublishedContentCompiler.EvidenceBytes(p.Replays, math),
                ["review-template.json"] = json(new ContentReviewEvidence { SchemaVersion = 1, Verdict = "UNREVIEWED", Binding = ContentBindingRecord.From(p.Binding),
                    DraftId = p.DraftId, Revision = p.Revision, DefinitionBindings = p.DefinitionBindings.Select(DefinitionBindingRecord.From).ToList(),
                    SourceBytes = p.SourceBytes.Count, SourceSha256 = p.SourceSha256, PayloadBytes = p.PayloadBytes.Count, PayloadSha256 = p.PayloadSha256,
                    ValidationBytes = p.Validation.Bytes.Count, ValidationSha256 = p.Validation.Sha256 }),
                ["result.json"] = json(new { Status = "PreparedPendingIndependentReview", Mode = "prepare", SourcePath = sourcePath,
                    Binding = p.Binding, GeometryValidatedLevels = p.Replays.Count, ReplayValidatedLevels = p.Replays.Count,
                    ExactParameterRowsPerLevel = p.Replays.Select(r => r.Candidate.Parameters.Count).ToArray(), PublicationInvoked = false })
            };
            foreach (var name in files.Keys) { var path = CanonicalPath(Path.Combine(root, name)); Need(!File.Exists(path), "OutputExists", path); }
            Directory.CreateDirectory(root); CanonicalPath(root);
            foreach (var file in files)
            {
                using (var stream = new FileStream(CanonicalPath(Path.Combine(root, file.Key)), FileMode.CreateNew, FileAccess.Write, FileShare.None))
                { stream.Write(file.Value, 0, file.Value.Length); stream.Flush(true); }
            }
            Console.WriteLine("PREPARED " + p.Binding.ContentFingerprint + "; independent review required");
        }
        private const string ProjectRoot = "D:/Unity/UnityProj/FightMatch";
        private const string PublicationRoot = ProjectRoot + "/TestArtifacts/FMDemo025P2/p2b-publish";
        private const string ReleaseRoot = ProjectRoot + "/Assets/StreamingAssets/FightMatch";
        private const string ApprovedRoot = ProjectRoot + "/TestArtifacts/FMDemo025P2/p2b-prepare-c2/authoring";
        private const string SourcePath = ProjectRoot + "/Assets/FightMatchContent/demo-r1.source.json";
        private const string ReviewPath = ProjectRoot + "/docs/system-design/2026-09-17/demo-025-p2b-content-review.json";
        private const string Operation = "publish:fightmatch-demo-r1:1", Scope = "player", ReleaseId = "release-set:fightmatch-demo-r1";
        private const string SourceHash = "fc2a6e8ec7cdba10992b9e0de7433020b75b1a4ef2b18b5570b199b9983a261a";
        private const string PayloadHash = "b2247d3f951626edfdf25753520f3421dc20e8d8cab7731c9ab3ba6ece1a5129";
        private const string ValidationHash = "512b2f9b5027674dee1eed127e811a7b7c31322562f2dd81604d1faf1c79b38d";
        private const string ReviewHash = "16b282218b0652910535a2c6d70c736da1023d74c327e0492a399b5b8fd9fa75";
        private const int RecordLimit = 16 * 1024 * 1024;
        private static readonly string[] ReleaseNames = { "first-release.fmsource.json", "first-release.fmpackage.bytes",
            "first-release.fmvalidation.bytes", "first-release.fmreview.json", "first-release.fmpublish.json", "first-release.fmrelease.json" };
        private static ExactMathBudget NewMath() => new ExactMathBudget(maxPrimitiveSteps: 16000000);
        private static ContentStoreBudget StoreBudget() => new ContentStoreBudget(NewMath());
        private static T Take<T>(PublicationResult<T> result) { Need(result.IsAccepted, result.RejectionCode, result.FieldPath); return result.Value; }
        private static ContentBinding FixedBinding()
        {
            var r = ContentBinding.Prepare("package:fightmatch-demo-r1", PayloadHash, "demo-r1", "RC01", "PC01+SC01", new SaveCodecBudget(NewMath()));
            Need(r.IsAccepted, r.RejectionCode, r.FieldPath); return r.Value;
        }
        private static void Same(byte[] a, byte[] b, string field) { Need(ContentPublicationStorage.Equal(a, b), "RecoveryBlocked", field); }
        private static byte[] ReadFile(string path) => PublishedContentCodec.ReadBounded(CanonicalPath(path), RecordLimit);
        private static void CheckHash(byte[] bytes, int count, string hash, string field)
        { Need(bytes.Length == count && PublishedContentCodec.Sha256(bytes) == hash, "RecoveryBlocked", field); }
        private static string[] StoreKeys() => new[] { PublishedContentCatalog.Key("source", SourceHash), PublishedContentCatalog.Key("payload", PayloadHash),
            PublishedContentCatalog.Key("validation", ValidationHash), PublishedContentCatalog.Key("review", ReviewHash),
            PublishedContentCatalog.Key("operation", Operation), PublishedContentCatalog.Key("receipt", Operation),
            PublishedContentCatalog.BindingKey(FixedBinding()), PublishedContentCatalog.ReleaseSetKey(Scope, ReleaseId) };
        private static void RunPublication(Dictionary<string, string> values)
        {
            var mode = values["-fmMode"]; Need(new[] { "publish", "activate-export", "verify-release" }.Contains(mode), "UnsupportedBinding", "Mode");
            var expected = new Dictionary<string, string>(StringComparer.Ordinal) { ["-fmMode"] = mode, ["-fmEvidenceRoot"] = PublicationRoot };
            if (mode != "verify-release") { expected.Add("-fmStoreRoot", PublicationRoot + "/publication-store"); expected.Add("-fmOperationId", Operation); }
            if (mode == "publish") { expected.Add("-fmSource", SourcePath); expected.Add("-fmReview", ReviewPath); }
            else { expected.Add("-fmScope", Scope); expected.Add("-fmReleaseSetId", ReleaseId); expected.Add("-fmReleaseRoot", ReleaseRoot); }
            Need(values.Count == expected.Count && expected.Keys.All(values.ContainsKey), "InvalidValue", "Arguments");
            foreach (var item in expected)
            {
                var path = item.Key.EndsWith("Root", StringComparison.Ordinal) || item.Key == "-fmSource" || item.Key == "-fmReview";
                Need(path ? string.Equals(CanonicalPath(values[item.Key]), CanonicalPath(item.Value), StringComparison.OrdinalIgnoreCase) :
                    values[item.Key] == item.Value, "UnsupportedBinding", item.Key);
            }
            var caps = ContentConsumerCapabilities.Current;
            if (mode == "verify-release")
            {
                // This process reads content only from the six exported files, never from the publication store or authoring source.
                var files = ReleaseNames.Select(n => ReadFile(ReleaseRoot + "/" + n)).ToArray();
                var memory = Take(FirstReleaseContentStorage.Create(files[0], files[1], files[2], files[3], files[4], files[5], caps, StoreBudget()));
                var cold = new PublishedContentCatalog(memory, caps); Need(FixedBinding().Same(Take(cold.GetCurrentBinding(Scope, ReleaseId, caps))), "RecoveryBlocked", "Cold.Binding");
                var resolved = Confirm(cold);
                Evidence("cold-load-result.json", new { Status = "ColdLoaded", Scope, ReleaseSetId = ReleaseId, resolved.Binding,
                    resolved.NewProfile, ContentInputs = ReleaseNames.Select((n, i) => FileIdentity(ReleaseRoot + "/" + n, files[i])).ToArray(),
                    PublicationStoreRead = false, AuthoringSourceRead = false, OnlySixFiles = true });
                Console.WriteLine("COLD_LOADED " + resolved.Binding.ContentFingerprint); return;
            }
            var store = new WindowsContentPublicationStorage(CanonicalPath(PublicationRoot + "/publication-store"));
            CheckStorePaths(); var catalog = new PublishedContentCatalog(store, caps);
            if (mode == "publish")
            {
                var bytes = ReadFile(SourcePath); CheckHash(bytes, 20443, SourceHash, "Approved.Source");
                var review = ReadFile(ReviewPath); CheckHash(review, 4766, ReviewHash, "Approved.Review");
                var source = Take(PublishedContentCodec.DecodeSource(bytes, caps, NewMath()));
                Need(source.DraftId == "draft:fightmatch-demo-r1" && source.Revision == 1, "UnsupportedBinding", "Draft");
                var job = new DemoContentDraft(source.DraftId).BeginJob(); var built = PublishedContentCompiler.Prepare(source, job, caps, NewMath());
                Need(built.IsAccepted, built.RejectionCode, built.FieldPath); var prepared = built.Prepared;
                Need(FixedBinding().Same(prepared.Binding) && prepared.DefinitionBindings.Count == 1 &&
                    prepared.DefinitionBindings[0].LevelId == "level:ch01-01" && prepared.DefinitionBindings[0].LevelVersion == 1, "UnsupportedBinding", "Prepared.Binding");
                Same(prepared.SourceBytes.ToArray(), bytes, "Prepared.Source");
                Same(bytes, ReadFile(ApprovedRoot + "/source-copy.json"), "Approved.SourceCopy");
                Same(prepared.PayloadBytes.ToArray(), ReadFile(ApprovedRoot + "/prepared.fmpackage.bytes"), "Approved.Payload");
                Same(prepared.Validation.Bytes.ToArray(), ReadFile(ApprovedRoot + "/prepared.fmvalidation.bytes"), "Approved.Validation");
                var before = catalog.GetCurrentBinding(Scope, ReleaseId, caps);
                Need(!before.IsAccepted && before.RejectionCode == "UnsupportedBinding" && before.FieldPath == "ReleaseSet", "RecoveryBlocked", "Publish.AlreadyActive");
                if (!File.Exists(PublicationRoot + "/store-before.json")) Evidence("store-before.json", StoreSnapshot(store));
                var outcome = Take(Publish(catalog, prepared, ReviewPath, job, Operation, StoreBudget()));
                Need(outcome.Status == "Completed" && outcome.OperationId == Operation, "RecoveryBlocked", "Publish.Status");
                var resolved = Confirm(catalog); var files = PublishedFiles(store, resolved);
                var after = catalog.GetCurrentBinding(Scope, ReleaseId, caps);
                Need(!after.IsAccepted && after.RejectionCode == "UnsupportedBinding" && after.FieldPath == "ReleaseSet", "RecoveryBlocked", "Publish.Activation");
                Evidence("publication-result.json", new { outcome.Status, outcome.OperationId, resolved.Binding, BeforeCurrent = before.RejectionCode,
                    AfterCurrent = after.RejectionCode, Records = StoreSnapshot(store), PreparedSourceCompared = true, PreparedPayloadCompared = true,
                    PreparedValidationCompared = true, ReviewSha256 = PublishedContentCodec.Sha256(files[3]) });
                Console.WriteLine("PUBLISHED " + Operation + " Completed; exact records retained; release absent"); return;
            }
            var publication = Confirm(catalog); var exported = PublishedFiles(store, publication);
            var previous = catalog.GetCurrentBinding(Scope, ReleaseId, caps);
            Need(previous.IsAccepted ? FixedBinding().Same(previous.Value) : previous.RejectionCode == "UnsupportedBinding" && previous.FieldPath == "ReleaseSet",
                "RecoveryBlocked", "Release.Before");
            var release = Take(PublishedContentCodec.EncodeReleaseSet(new ContentReleaseSet { SchemaVersion = 1, Scope = Scope, ReleaseSetId = ReleaseId,
                Binding = ContentBindingRecord.From(publication.Binding), PublicationReceiptSha256 = PublishedContentCodec.Sha256(exported[4]) }, StoreBudget()));
            var key = PublishedContentCatalog.ReleaseSetKey(Scope, ReleaseId);
            using (store.AcquireWriter()) { store.WriteImmutable(key, release, RecordLimit); Same(release, store.Read(key, RecordLimit), "Release.ReadBack"); }
            Need(FixedBinding().Same(Take(catalog.GetCurrentBinding(Scope, ReleaseId, caps))), "RecoveryBlocked", "Release.Current");
            exported[5] = release; Need(exported.Sum(b => (long)b.Length) <= 32 * 1024 * 1024, "BudgetExceeded", "Export.Total");
            for (var i = 0; i < ReleaseNames.Length; i++)
            { var path = CanonicalPath(ReleaseRoot + "/" + ReleaseNames[i]); if (File.Exists(path)) Same(ReadFile(path), exported[i], "Export.Existing"); }
            for (var i = 0; i < ReleaseNames.Length; i++) WriteSameOrNew(ReleaseRoot + "/" + ReleaseNames[i], exported[i]);
            Evidence("release-result.json", new { Status = "Activated", Scope, ReleaseSetId = ReleaseId, publication.Binding,
                BeforeCurrentAccepted = previous.IsAccepted, BeforeCurrentRejection = previous.RejectionCode, AfterCurrentAccepted = true,
                PublicationReceiptSha256 = PublishedContentCodec.Sha256(exported[4]), Release = FileIdentity(key, release) });
            Evidence("export-manifest.json", new { Status = "Exported", Files = ReleaseNames.Select((n, i) => FileIdentity(ReleaseRoot + "/" + n, exported[i])).ToArray(),
                OperationReceiptBindingBytesEqual = true, OriginalSourceRetained = true, TotalBytes = exported.Sum(b => (long)b.Length) });
            Evidence("store-after.json", StoreSnapshot(store)); Console.WriteLine("ACTIVATED_EXPORTED " + ReleaseId);
        }
        private static ResolvedPublication Confirm(PublishedContentCatalog catalog)
        {
            var binding = FixedBinding(); var caps = ContentConsumerCapabilities.Current; var r = Take(catalog.ResolveExact(binding, caps));
            var level = DefinitionBinding.Prepare(binding, "level:ch01-01", 1, new SaveCodecBudget(NewMath())); Need(level.IsAccepted, level.RejectionCode, level.FieldPath);
            var resolved = Take(catalog.ResolveExact(level.Value, caps)); Need(resolved.LevelId == "level:ch01-01" && resolved.LevelVersion == "1", "RecoveryBlocked", "Level");
            CheckHash(r.NewProfile.CanonicalBytes.ToArray(), 298, "36f2d8935f626c4bdafb53c56ac288f5199e7a6dec29680c0fac02f354685646", "NewProfile");
            Need(binding.Same(r.Binding), "RecoveryBlocked", "Resolved.Binding"); return r;
        }
        private static byte[][] PublishedFiles(IContentPublicationStorage store, ResolvedPublication publication)
        {
            var keys = StoreKeys(); var files = new byte[6][];
            for (var i = 0; i < 5; i++) { files[i] = store.Read(keys[i], RecordLimit); Root(files[i], "Published.File"); }
            Same(files[4], publication.ReceiptBytes.ToArray(), "Receipt.Resolved");
            Same(files[4], store.Read(keys[5], RecordLimit), "Receipt.Persisted");
            Same(files[4], store.Read(keys[6], RecordLimit), "Binding.Persisted");
            CheckHash(files[0], 20443, SourceHash, "Published.Source"); CheckHash(files[1], 11486, PayloadHash, "Published.Payload");
            CheckHash(files[2], 664, ValidationHash, "Published.Validation"); CheckHash(files[3], 4766, ReviewHash, "Published.Review"); return files;
        }
        private static object StoreSnapshot(IContentPublicationStorage store)
        {
            return StoreKeys().Select(key => { var bytes = store.Read(key, RecordLimit);
                return new { Key = key, Present = bytes != null, Bytes = bytes == null ? 0 : bytes.Length, Sha256 = bytes == null ? null : PublishedContentCodec.Sha256(bytes) }; }).ToArray();
        }
        private static object FileIdentity(string path, byte[] bytes) => new { Path = path, Bytes = bytes.Length, Sha256 = PublishedContentCodec.Sha256(bytes) };
        private static void CheckStorePaths()
        {
            var root = CanonicalPath(PublicationRoot + "/publication-store"); if (!Directory.Exists(root)) return;
            var names = new HashSet<string>(StoreKeys().Select(k => k + ".blob"), StringComparer.Ordinal) { "writer.lock" };
            foreach (var entry in Directory.EnumerateFileSystemEntries(root))
            { Need(names.Contains(Path.GetFileName(entry)) && File.Exists(CanonicalPath(entry)), "RecoveryBlocked", "Store.UnknownOrUnpromoted"); }
            var writer = root + "/writer.lock"; if (File.Exists(writer)) Need(new FileInfo(writer).Length == 0, "RecoveryBlocked", "Store.Lock");
        }
        private static void Evidence(string name, object value)
        {
            var bytes = PublishedContentCodec.Encode(new { StageId = "025-P2B-PUBLISH", AuthorTaskId = "01a0c403-bfa1-7e90-b503-c0fcd61f23c1",
                AuthorTurnId = "01a0d43a-d905-7780-a1aa-f1c979d9c751", SourcePacketRangeSha256 = "39a2c272b5c55f51e71bb681e9340f5a819f5937d747b7816f914d2c4779d7b5",
                Data = value }, RecordLimit, ContentConsumerCapabilities.Current, NewMath());
            WriteSameOrNew(PublicationRoot + "/" + name, bytes);
        }
        private static void WriteSameOrNew(string path, byte[] bytes)
        {
            path = CanonicalPath(path); Need(bytes.Length <= RecordLimit, "BudgetExceeded", "Output");
            if (File.Exists(path)) { Same(ReadFile(path), bytes, "Output.Existing"); return; }
            Directory.CreateDirectory(Path.GetDirectoryName(path)); CanonicalPath(path);
            using (var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            { stream.Write(bytes, 0, bytes.Length); stream.Flush(true); }
            Same(ReadFile(path), bytes, "Output.ReadBack");
        }

        private static string CanonicalPath(string path)
        {
            Need(Path.IsPathRooted(path), "InvalidValue", "AbsolutePath"); var full = Path.GetFullPath(path);
            Need(string.Equals(full.Replace('\\', '/'), path.Replace('\\', '/'), StringComparison.OrdinalIgnoreCase), "InvalidValue", "CanonicalPath");
            for (var at = full; at != null; at = Path.GetDirectoryName(at))
            {
                try { Need((File.GetAttributes(at) & FileAttributes.ReparsePoint) == 0, "InvalidValue", "ReparsePath"); }
                catch (FileNotFoundException) { } catch (DirectoryNotFoundException) { }
            }
            return full;
        }
    }
}
