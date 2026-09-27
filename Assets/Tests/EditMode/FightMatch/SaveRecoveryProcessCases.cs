using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using FightMatch.Core;
using FightMatch.Platform;
using FlowPuzzle.Core;

namespace FightMatch.Tests
{
    // Shared, dependency-free scenarios: the controller references Unity's actual three production DLLs.
    public static class SaveRecoveryProcessCases
    {
        public const string Project = @"D:\Unity\UnityProj\FightMatch";
        public static string NewRun()
        {
            var root = Path.Combine(Project, "TestArtifacts", "FMDemoB13", Guid.NewGuid().ToString("N"));
            CheckRoot(root); Directory.CreateDirectory(root); return root;
        }
        public static void Need(bool value, string reason) { if (!value) throw new InvalidOperationException("B13 assertion: " + reason); }
        public static SaveStoreBudget Budget() { return new SaveStoreBudget(new SaveCodecBudget(new ExactMathBudget())); }
        public static T Core<T>(SaveCodecResult<T> r) { Need(r.IsAccepted, r.RejectionCode + ":" + r.FieldPath); return r.Value; }
        public static T Ok<T>(LocalSaveResult<T> r, string code) { Need(r.IsAccepted && r.Code == code, r.Code + ":" + r.FieldPath + ":" + r.Diagnostic?.ExceptionMessage); return r.Value; }
        public static LocalSaveStore Open(ILocalSaveStorage storage, SaveOpenMode mode = SaveOpenMode.CreateNew)
        { return Ok(LocalSaveStore.Open(storage, storage.Profile.PlayerId, storage.Profile.Purpose, mode, SaveFaultModel.EditorProcessCrash, Budget()), "Opened"); }
        public static string Hash(byte[] bytes) { using (var h = SHA256.Create()) return string.Concat(h.ComputeHash(bytes).Select(x => x.ToString("x2"))); }
        public static string FileHash(string path) { using (var s = File.OpenRead(path)) using (var h = SHA256.Create()) return string.Concat(h.ComputeHash(s).Select(x => x.ToString("x2"))); }
        public static void CheckRoot(string root)
        {
            var full = Path.GetFullPath(root); var parent = Path.Combine(Project, "TestArtifacts", "FMDemoB13"); Guid id;
            Need(Path.GetDirectoryName(full) == parent && Guid.TryParseExact(Path.GetFileName(full), "N", out id), "fresh B13 root");
            CheckAncestors(full);
        }
        public static string Safe(string root, string path)
        {
            CheckRoot(root); var full = Path.GetFullPath(path);
            Need(full.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase), "path outside experiment: " + full);
            CheckAncestors(full); return full;
        }
        private static void CheckAncestors(string full)
        {
            for (var p = full; !string.IsNullOrEmpty(p); p = Path.GetDirectoryName(p))
            {
                try { Need((File.GetAttributes(p) & FileAttributes.ReparsePoint) == 0, "reparse: " + p); }
                catch (FileNotFoundException) { } catch (DirectoryNotFoundException) { }
            }
        }
        public static void WriteRecord(string root, string path, IDictionary<string, string> fields)
        {
            var full = Safe(root, path); var temp = Safe(root, full + ".writing");
            using (var s = new FileStream(temp, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            using (var w = new StreamWriter(s, new UTF8Encoding(false)))
            {
                foreach (var pair in fields.OrderBy(x => x.Key, StringComparer.Ordinal))
                    w.WriteLine(pair.Key + "\t" + Convert.ToBase64String(Encoding.UTF8.GetBytes(pair.Value ?? "")));
                w.Flush(); s.Flush(true);
            }
            File.Move(temp, full);
        }
        public static Dictionary<string, string> ReadRecord(string root, string path)
        {
            return File.ReadAllLines(Safe(root, path)).Select(x => x.Split('\t')).ToDictionary(x => x[0],
                x => Encoding.UTF8.GetString(Convert.FromBase64String(x[1])), StringComparer.Ordinal);
        }
        public static Dictionary<string, string> Identity(string root, string nonce, string caseRoot, string mode, string number)
        {
            using (var p = Process.GetCurrentProcess()) return new Dictionary<string, string> {
                { "pid", p.Id.ToString(CultureInfo.InvariantCulture) }, { "startTicks", p.StartTime.ToUniversalTime().Ticks.ToString(CultureInfo.InvariantCulture) },
                { "exe", Path.GetFullPath(p.MainModule.FileName) }, { "root", root }, { "nonce", nonce }, { "caseRoot", caseRoot },
                { "mode", mode }, { "case", number }, { "utc", DateTime.UtcNow.ToString("o") },
                { "os", Environment.OSVersion.ToString() }, { "runtime", System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription },
                { "filesystem", new DriveInfo(Path.GetPathRoot(root)).DriveFormat } };
        }
        public static void Run(string[] args)
        {
            Func<string, string> value = key => { var n = Array.IndexOf(args, key); Need(n >= 0 && n + 1 < args.Length, key); return args[n + 1]; };
            var root = Path.GetFullPath(value("--b13-root")); CheckRoot(root);
            var nonce = value("--b13-nonce"); Need(Path.GetFileName(root) == nonce, "nonce/root");
            var caseRoot = Safe(root, value("--b13-case-root")); var mode = value("--b13-mode"); var number = value("--b13-case");
            Need(Enumerable.Range(1, 10).Any(x => number == "P" + x.ToString("00")), "case");
            Need(mode == "writer" || mode == "reader", "mode");
            WriteRecord(root, Path.Combine(caseRoot, mode + "-identity.tsv"), Identity(root, nonce, caseRoot, mode, number));
            var real = new WindowsEditorSaveStorage(Safe(root, Path.Combine(caseRoot, "data")), "b13:" + nonce + ":" + Path.GetFileName(caseRoot), SavePurpose.CandidateValidation);
            if (mode == "writer") Writer(root, nonce, caseRoot, number, real); else Reader(root, caseRoot, number, real);
        }
        private static void Barrier(string root, string nonce, string caseRoot, string number, string phase)
        {
            var identity = Identity(root, nonce, caseRoot, "writer", number); identity.Add("phase", phase);
            identity.Add("files", Inventory(root, Path.Combine(caseRoot, "data")));
            WriteRecord(root, Path.Combine(caseRoot, "barrier.tsv"), identity);
            // This is a bounded waiting writer, not a simulated crash or an exception injection.
            var deadline = DateTime.UtcNow.AddMinutes(8);
            while (DateTime.UtcNow < deadline) Thread.Sleep(250);
            throw new TimeoutException("Controller did not terminate the identified writer.");
        }
        public static string Inventory(string root, string directory)
        {
            Safe(root, directory); var rows = new List<string>();
            if (!Directory.Exists(directory)) return "";
            foreach (var path in Directory.EnumerateFileSystemEntries(directory).OrderBy(x => x, StringComparer.Ordinal))
            {
                Safe(root, path);
                if (Directory.Exists(path)) rows.Add(Inventory(root, path));
                else
                {
                    var name = path.Substring(root.Length + 1);
                    if (Path.GetFileName(path) == "writer.lock") rows.Add(name + "\tlease\tbytes-excluded");
                    else
                    {
                        // Active work streams have FileShare.None; record an explicit inaccessible reason at a barrier.
                        try { rows.Add(name + "\t" + new FileInfo(path).Length + "\t" + FileHash(path)); }
                        catch (IOException e) { rows.Add(name + "\tunreadable\t" + e.GetType().FullName + ":" + e.Message); }
                    }
                }
            }
            return string.Join("\n", rows);
        }
        public static SaveCommitTicket PrepareBusiness(LocalSaveStore store, SnapshotDescriptor head, string operation, CandidateBusinessSnapshot snapshot)
        {
            var b = Budget(); return Ok(store.Prepare(head, new[] { operation }, m => CandidateBusinessSaveCodec.Encode(snapshot,
                new CandidateBusinessSaveHeader(m.SaveGeneration, m.CommitId, m.ParentCommitId, m.CommitIndex), b.Codec), b), "Prepared");
        }
        private static string Slices(SaveEnvelope e) { return string.Join(",", e.SliceBytes.Select(x => Hash(x.ToArray()))); }
        private static void Writer(string root, string nonce, string caseRoot, string number, WindowsEditorSaveStorage real)
        {
            Need(!Directory.Exists(real.Profile.DirectoryPath), "writer starts with no save");
            var wrapper = new RecoveryProcessStorage(real, number, phase => Barrier(root, nonce, caseRoot, number, phase));
            using (var store = Open(wrapper))
            {
                var scenario = RecoveryBusinessScenario.New(real.Profile.PlayerId); scenario.Begin();
                var baseline = PrepareBusiness(store, null, "save:1", scenario.Prepare());
                var first = Ok(store.Write(baseline, Budget()), "Committed");
                scenario.Attack("fixed-attack", 0); scenario.Rollback("fixed-rollback", "fixed-attack");
                var candidate = PrepareBusiness(store, first.CurrentHead, "save:2", scenario.Prepare());
                var expected = new Dictionary<string, string> { { "oldCommit", first.Descriptor.CommitId }, { "candidate", candidate.Metadata.CommitId },
                    { "oldSlices", Slices(baseline.Envelope) }, { "newSlices", Slices(candidate.Envelope) }, { "oldGeneration", "1" }, { "newGeneration", "2" } };
                if (number == "P09")
                {
                    var head = Ok(store.Write(candidate, Budget()), "Committed").CurrentHead;
                    for (var i = 3; i <= 4; i++) head = Ok(store.Write(PrepareBusiness(store, head, "save:" + i, scenario.Prepare()), Budget()), "Committed").CurrentHead;
                    expected["candidate"] = head.CommitId; expected["newGeneration"] = "4"; expected["previous"] = head.ParentCommitId;
                    WriteRecord(root, Path.Combine(caseRoot, "expected.tsv"), expected);
                    var view = Ok(store.ReadRecovery(Budget()), "RecoveryObserved"); wrapper.Armed = true;
                    Ok(store.Cleanup(view, Budget()), "Cleaned"); throw new InvalidOperationException("P09 barrier not reached");
                }
                WriteRecord(root, Path.Combine(caseRoot, "expected.tsv"), expected); wrapper.Armed = true;
                var committed = Ok(store.Write(candidate, Budget()), "Committed");
                WriteRecord(root, Path.Combine(caseRoot, "receipt.tsv"), new Dictionary<string, string> {
                    { "code", "Committed" }, { "commit", committed.Descriptor.CommitId }, { "generation", committed.CurrentHead.SaveGeneration.ToString() },
                    { "operation", "save:2" }, { "snapshotSha", string.Concat(committed.Descriptor.Sha256.Select(x => x.ToString("x2"))) } });
                if (number == "P08") Barrier(root, nonce, caseRoot, number, "Write.Committed.ReceiptPublished");
                Need(number == "P10", "expected writer barrier");
                WriteRecord(root, Path.Combine(caseRoot, "normal-exit.tsv"), new Dictionary<string, string> { { "code", "Committed" }, { "files", Inventory(root, real.Profile.DirectoryPath) } });
            }
        }
        private static void Reader(string root, string caseRoot, string number, WindowsEditorSaveStorage real)
        {
            var expected = ReadRecord(root, Path.Combine(caseRoot, "expected.tsv"));
            var before = Inventory(root, real.Profile.DirectoryPath);
            var committed = number == "P07" || number == "P08" || number == "P09" || number == "P10";
            using (var store = Open(real, SaveOpenMode.Existing))
            {
                var view = Ok(store.ReadRecovery(Budget()), "RecoveryObserved");
                Need(view.Current != null && view.Current.Descriptor.CommitId == expected[committed ? "candidate" : "oldCommit"], "exact head");
                Need(view.Status == (committed ? SaveHeadStatus.Ready : SaveHeadStatus.Pending), "classification");
                Need(view.EvidenceComplete, "all crash files are bounded readable evidence");
                Need(view.RequirementsComplete == (number != "P01" && number != "P04"), "complete/partial roots");
                Need(view.HasUnresolvedCandidate == !committed, "orphan stays unresolved");
                var original = store.Lookup(expected["candidate"], number == "P09" ? "save:4" : "save:2", Budget());
                Need(original.Code == (committed ? "Committed" : "ConfirmedNotCommitted"), "original key lookup");
                SaveEnvelope loaded;
                if (committed) loaded = Ok(store.Load(Budget()), "Loaded");
                else
                {
                    Need(store.Load(Budget()).Code == "CommitUnknown", "pending closes Load");
                    Need(!store.Cleanup(view, Budget()).IsAccepted, "pending closes Cleanup");
                    using (var stream = real.OpenRead("c-" + view.Current.Descriptor.CommitId + ".snapshot"))
                        loaded = Core(SaveEnvelopeCodec.Read(stream, view.Current.Descriptor, Budget().Codec));
                    var calls = 0; var caps = Capabilities(view);
                    var verified = store.WithVerifiedRecovery(view, caps, _ => calls++, Budget());
                    Need(verified.IsAccepted == view.RequirementsComplete && calls == (view.RequirementsComplete ? 1 : 0), "pending capability authorization");
                    Need(view.RetainedRoots.Any(x => x.Kind == SaveRecoveryRootKind.Pending) == (number != "P01"), "complete candidate root");
                }
                Need(Slices(loaded) == expected[committed ? "newSlices" : "oldSlices"], "five original business byte arrays");
                var business = Core(CandidateBusinessSaveCodec.Decode(loaded, Budget().Codec)); FixedBusiness(business, committed);
                Need(before == Inventory(root, real.Profile.DirectoryPath), "read/lookup/capability check preserves all files");
                var removed = "";
                if (number == "P09")
                {
                    Need(view.Current.Descriptor.ParentCommitId == expected["previous"], "immediate parent");
                    Need(view.RedundancyDegraded, "interrupted old pair diagnosed");
                    var clean = Ok(store.Cleanup(view, Budget()), "Cleaned"); removed = string.Join(",", clean.RemovedNames);
                    Need(clean.RemovedNames.Count == 3, "resume exactly three remaining old files");
                    Need(store.Cleanup(view, Budget()).Code == "StaleContext", "old evidence cannot be reused");
                    var fresh = Ok(store.ReadRecovery(Budget()), "RecoveryObserved");
                    Need(Ok(store.Cleanup(fresh, Budget()), "Cleaned").RemovedNames.Count == 0, "fresh cleanup is idempotent");
                    Need(fresh.RetainedRoots.Count == 1 && fresh.RetainedRoots[0].Descriptor.CommitId == expected["previous"], "current plus adjacent previous");
                    Need(Ok(store.Lookup(expected["oldCommit"], "save:1", Budget()), "Committed").CurrentHead.CommitId == expected["candidate"], "old index survives");
                    FixedBusiness(Core(CandidateBusinessSaveCodec.Decode(Ok(store.Load(Budget()), "Loaded"), Budget().Codec)), true);
                }
                if (number == "P08" || number == "P10")
                { var receipt = ReadRecord(root, Path.Combine(caseRoot, "receipt.tsv")); Need(original.Value.Descriptor.CommitId == receipt["commit"], "every acknowledged commit remains"); }
                WriteRecord(root, Path.Combine(caseRoot, "reader-result.tsv"), new Dictionary<string, string> {
                    { "result", "Passed" }, { "head", view.Current.Descriptor.CommitId }, { "generation", view.Current.Descriptor.SaveGeneration.ToString() },
                    { "status", view.Status.ToString() }, { "lookup", original.Code }, { "requirementsComplete", view.RequirementsComplete.ToString() },
                    { "evidenceComplete", view.EvidenceComplete.ToString() }, { "roots", string.Join("\n", view.RetainedRoots.Select(x => x.Kind + "\t" + x.SourceName + "\t" + x.Descriptor.CommitId)) },
                    { "observations", string.Join("\n", view.Files.Select(x => x.Name + "\t" + x.Kind + "\t" + x.Disposition + "\t" + x.Length + "\t" + x.Detail)) },
                    { "fiveSliceSha", Slices(loaded) }, { "archive", business.ActiveHistory.Archive.Count.ToString() }, { "rollbacks", business.ActiveHistory.RollbackRecords.Count.ToString() },
                    { "randomState", business.ActiveHistory.CurrentRun.CurrentSnapshot.Random.Stream.Current.State.ToString() }, { "removed", removed },
                    { "before", before }, { "after", Inventory(root, real.Profile.DirectoryPath) } });
            }
        }
        public static SaveRecoveryCapabilities Capabilities(SaveRecoveryView view)
        {
            var roots = (view.Current == null ? view.RetainedRoots : new[] { view.Current }.Concat(view.RetainedRoots)).ToArray();
            var slices = roots.SelectMany(x => x.RequiredSliceContracts).GroupBy(x => x.SliceId + "\t" + x.OwnerId + "\t" + x.SchemaVersion).Select(x => x.First()).ToArray();
            var bindings = new List<SaveBinding>();
            foreach (var b in roots.SelectMany(x => x.Requirements.Bindings))
                if (!bindings.Any(x => BindingKey(x) == BindingKey(b))) bindings.Add(b);
            return new SaveRecoveryCapabilities(slices, bindings, roots.SelectMany(x => x.Requirements.RuleVersions).Distinct(StringComparer.Ordinal).ToArray(),
                roots.SelectMany(x => x.Requirements.NumericContractVersions).Distinct(StringComparer.Ordinal).ToArray(),
                roots.SelectMany(x => x.Requirements.RandomContractVersions).Distinct(StringComparer.Ordinal).ToArray(),
                roots.SelectMany(x => x.Requirements.FeatureIds).Distinct(StringComparer.Ordinal).ToArray());
        }
        private static string BindingKey(SaveBinding b)
        { return string.Join("|", new[] { b.Kind.ToString(), b.PackageId, b.DraftId, b.DraftRevision?.ToString(), b.ContentFingerprint,
            b.RuleVersion, b.NumericContractVersion, b.RandomContractVersion, b.LevelId, b.LevelVersion, b.SourceNotes == null ? null : string.Join("\n", b.SourceNotes) }); }
        public static void FixedBusiness(CandidateBusinessSnapshot b, bool restored)
        {
            Need(b.Character.CharacterId == "W" && b.Character.Level == 1 && b.Character.Experience.IsZero, "fixed permanent character");
            Need(b.Rewards.BaseRewards.Count == 0 && b.Character.BaseRewards.Count == 0, "no invented reward");
            Need(b.Inventory.ActiveCarry != null && b.Progression.ActiveAttempt != null, "owners retain the same active attempt");
            var h = b.ActiveHistory; Need(h != null && h.Archive.Count == (restored ? 1 : 0) && h.RollbackRecords.Count == (restored ? 1 : 0), "original history and rollback");
            if (restored) Need(h.Archive[0].OperationId == "fixed-attack" && h.RollbackRecords[0].OperationId == "fixed-rollback", "original operation identities");
            var random = h.CurrentRun.CurrentSnapshot.Random.Stream;
            Need(h.Binding.Battle.InitState == 42 && h.Binding.Battle.InitSequence == 54 && random.WordsConsumed.IsZero &&
                random.Current.State == h.Binding.Battle.Initial.Current.State && random.Current.Increment == 109, "original random, no replay/resampling");
            Need(h.CurrentRun.CurrentSnapshot.Members[0].Hp.Numerator == 100, "restored original HP");
        }
    }

    internal sealed class RecoveryProcessStorage : ILocalSaveStorage
    {
        private readonly WindowsEditorSaveStorage real;
        private readonly string number;
        private readonly Action<string> barrier;
        internal bool Armed;
        internal RecoveryProcessStorage(WindowsEditorSaveStorage real, string number, Action<string> barrier)
        { this.real = real; this.number = number; this.barrier = barrier; }
        public SaveStorageProfile Profile => real.Profile;
        public IDisposable AcquireWriterLease(bool createDirectory) { return real.AcquireWriterLease(createDirectory); }
        public IEnumerable<string> EnumerateNames() { return real.EnumerateNames(); }
        public Stream OpenRead(string name) { return real.OpenRead(name); }
        public Stream CreateWork(string name) { return new BarrierStream(real.CreateWork(name), name, this); }
        public void FlushFile(Stream stream)
        {
            var b = (BarrierStream)stream; real.FlushFile(b.Inner);
            if (Armed && ((number == "P02" && b.Name.EndsWith(".snapshot.tmp")) || (number == "P05" && b.Name.EndsWith(".commit.tmp")))) barrier(number + ".FlushCompleted");
        }
        public void PromoteNoReplace(string work, string final)
        {
            var marker = final.EndsWith(".commit", StringComparison.Ordinal);
            if (Armed && marker && number == "P06") barrier("Marker.Promote.Before");
            real.PromoteNoReplace(work, final);
            if (Armed && !marker && number == "P03") barrier("Snapshot.Promote.After");
            if (Armed && marker && number == "P07") barrier("Marker.Promote.After.WriteNotReturned");
        }
        public void DeleteUncommitted(string name) { real.DeleteUncommitted(name); }
        public void DeleteIndexedOld(string name)
        { real.DeleteIndexedOld(name); if (Armed && number == "P09" && name.EndsWith(".commit")) barrier("Cleanup.OldMarkerDeleted.BeforeSnapshot"); }
        private sealed class BarrierStream : Stream
        {
            internal readonly Stream Inner; internal readonly string Name; private readonly RecoveryProcessStorage owner; private int writes;
            internal BarrierStream(Stream inner, string name, RecoveryProcessStorage owner) { Inner = inner; Name = name; this.owner = owner; }
            public override bool CanRead => Inner.CanRead; public override bool CanWrite => Inner.CanWrite; public override bool CanSeek => Inner.CanSeek;
            public override long Length => Inner.Length; public override long Position { get => Inner.Position; set => Inner.Position = value; }
            public override int Read(byte[] b, int o, int c) { return Inner.Read(b, o, c); }
            public override long Seek(long o, SeekOrigin origin) { return Inner.Seek(o, origin); }
            public override void SetLength(long n) { Inner.SetLength(n); }
            public override void Flush() { Inner.Flush(); }
            public override void Write(byte[] b, int o, int c)
            {
                // Snapshot writes are header, metadata, then business slices; P01 interrupts the first business slice.
                if (owner.Armed && ((writes == 2 && owner.number == "P01" && Name.EndsWith(".snapshot.tmp")) ||
                    (writes == 0 && owner.number == "P04" && Name.EndsWith(".commit.tmp"))))
                { Inner.Write(b, o, Math.Max(1, c / 2)); Inner.Flush(); owner.barrier(owner.number + ".PartialWrite"); }
                Inner.Write(b, o, c); writes++;
            }
            protected override void Dispose(bool disposing) { if (disposing) Inner.Dispose(); base.Dispose(disposing); }
        }
    }

    internal sealed class RecoveryBusinessScenario
    {
        internal CandidateContext Context;
        internal CandidateCharacterState Character;
        internal CandidateInventoryState Inventory;
        internal CandidateProgressionState Progression;
        internal CandidateRewardState Rewards;
        internal CandidateBattleHistory History;
        internal BattleEntryInput EntryInput;
        internal List<List<FlowPos>> Routes;
        internal List<CandidateBattleRun> RetainedRuns = new List<CandidateBattleRun>();
        internal List<CandidateRollbackRecord> RetainedRollbacks = new List<CandidateRollbackRecord>();
        internal int Level;
        internal static ExactMathBudget Math() { return new ExactMathBudget(); }
        internal static SaveCodecBudget Budget() { return new SaveCodecBudget(Math()); }
        internal static ExactRational R(BigInteger n, int d = 1) { return ExactRational.Create(n, d, Math()); }
        internal static RecoveryBusinessScenario New(string player)
        {
            const int level = 3, faces = 1, entryLevel = 1, hp = 100;
            var routes = new List<List<FlowPos>> {
                new List<FlowPos> { new FlowPos(0, 3), new FlowPos(0, 2), new FlowPos(1, 2) },
                new List<FlowPos> { new FlowPos(0, 0), new FlowPos(1, 0), new FlowPos(2, 0) } };
            var geometry = new FlowLevelData { levelId = 3, width = 4, height = 4 };
            for (var i = 0; i < routes.Count; i++) geometry.pairs.Add(new FlowPairData { colorId = i, endpointA = routes[i][0], endpointB = routes[i][2] });
            var s = new RecoveryBusinessScenario { Level = level, Context = new CandidateContext { DraftId = "isolated:B13", DraftRevision = 1,
                ContentFingerprint = "candidate-only", RuleVersion = "candidate-r1", NumericContractVersion = "exact-r1", RandomContractVersion = "pcg-r1",
                SourceNotes = new List<string> { "constructed:B13:4x4", "ZeroBasedBottomLeft", "explicit conditional C=1/1000", "explicit conditional C=1/1000" } } };
            var growth = CandidateCharacterGrowth.PrepareDefinition(Growth(s.Context, hp), Math()); SaveRecoveryProcessCases.Need(growth.IsAccepted, growth.FieldPath);
            var character = CandidateCharacterGrowth.CreateCandidate(growth.Definition, player, "W", entryLevel, entryLevel == 4 ? 157 : 0, 2, Math());
            SaveRecoveryProcessCases.Need(character.IsAccepted, character.FieldPath); s.Character = character.Next;
            var inventory = CandidateInventory.PrepareDefinition(new CandidateInventoryDefinitionInput { Context = s.Context, Items = new List<CandidateInventoryItemInput> {
                new CandidateInventoryItemInput { ItemId = "candidate:tin", Kind = CandidateInventoryItemKind.OrdinaryMaterial },
                new CandidateInventoryItemInput { ItemId = "candidate:wood", Kind = CandidateInventoryItemKind.OrdinaryMaterial } } }, Math());
            SaveRecoveryProcessCases.Need(inventory.IsAccepted, inventory.FieldPath);
            s.Inventory = CandidateInventory.CreateCandidate(inventory.Definition, s.Character.PlayerId, new List<CandidateInventoryActorInput> { Actor(s.Character) }, Math()).Next;
            var progression = CandidateProgression.PrepareDefinition(new CandidateProgressionDefinitionInput { Context = s.Context, Levels = new List<CandidateProgressionLevelInput> {
                new CandidateProgressionLevelInput { LevelId = "B13-L3", LevelVersion = "candidate-r1", UnlockRuleId = "initial", EntryKind = CandidateProgressionEntryKind.Ordinary,
                    UnlockKind = CandidateProgressionUnlockKind.InitiallyOpen, UnlockAfterLevelId = null, RequiredFeatures = new List<string>() },
                new CandidateProgressionLevelInput { LevelId = "next-level", LevelVersion = "candidate-r1", UnlockRuleId = "after", EntryKind = CandidateProgressionEntryKind.Ordinary,
                    UnlockKind = CandidateProgressionUnlockKind.AfterWholeLevelClear, UnlockAfterLevelId = "B13-L3", RequiredFeatures = new List<string>() } } }, Math());
            SaveRecoveryProcessCases.Need(progression.IsAccepted, progression.FieldPath); s.Progression = CandidateProgression.CreateCandidate(progression.Definition, s.Character.PlayerId, Math()).Next;
            s.Rewards = CandidateBaseRewards.CreateCandidate(s.Character.PlayerId, Math()).Next;
            var stats = CandidateCharacterGrowth.ComputeBaseStats(s.Character, Math());
            var face = new FaceInput { FaceId = "face0", Width = geometry.width, Height = geometry.height, Pairs = new List<PairInput>() };
            foreach (var pair in geometry.pairs.OrderBy(x => x.colorId))
            {
                var heavy = level == 3 && pair.colorId == 0;
                face.Pairs.Add(new PairInput { PairId = "pair" + pair.colorId, GeometryColorId = pair.colorId, EndpointA = pair.endpointA, EndpointB = pair.endpointB,
                    Enemy = new EnemyInput { EnemyInstanceKey = "enemy" + pair.colorId, EnemyDefinitionId = heavy ? "E02" : "E01", OriginalSlot = pair.colorId, StableOrder = pair.colorId,
                        Behavior = heavy ? EnemyBehavior.ChargeHeavy : EnemyBehavior.NormalStrike, Stats = Stats(heavy ? 20 : 15, 10, heavy ? 20 : 0, 0),
                        IntentCycle = heavy ? new List<EnemyIntentInput> { new EnemyIntentInput { Kind = EnemyIntentKind.Charge, Targeting = EnemyTargeting.FirstLiving,
                            DamageKind = null, DamageCoefficient = null }, Strike(13, 10) } : new List<EnemyIntentInput> { Strike(3, 5) } } });
            }
            s.EntryInput = new BattleEntryInput { PlayerId = s.Character.PlayerId, ChallengeId = "challenge", AttemptId = "attempt", EntryBaselineId = "baseline",
                Context = s.Context, CarryMode = EntryCarryMode.Empty, RequiredFeatures = new List<string>(),
                Level = new LevelInput { LevelId = "B13-L3", LevelVersion = "candidate-r1", RecommendedLevel = 1, Faces = new List<FaceInput> { face } },
                Members = new List<MemberInput> { new MemberInput { CharacterId = s.Character.CharacterId, ClassId = stats.ClassId, ClassKind = stats.ClassKind,
                    OriginalSlot = stats.OriginalSlot, Level = stats.Level, IsReady = stats.IsReady, StatsOrigin = stats.StatsOrigin, StatsContext = s.Context,
                    Stats = CopyStats(stats.Stats), EntryHp = stats.EntryHp, LearnedSkills = new List<string>(), Crit = new WarriorCritInput {
                        PassiveDefinitionId = stats.PassiveDefinitionId, TargetProbability = stats.TargetProbability, C = R(1, 1000), Multiplier = stats.CritMultiplier } } } };
            for (var i = 1; i < faces; i++) s.EntryInput.Level.Faces.Add(new FaceInput { FaceId = "face" + i, Width = face.Width, Height = face.Height, Pairs = face.Pairs });
            s.Routes = routes; return s;
        }
        internal static GrowthDefinitionInput Growth(CandidateContext context, int hp)
        { return new GrowthDefinitionInput { Context = context, ClassId = "warrior", ClassKind = CharacterClassKind.Warrior, PassiveDefinitionId = "warrior:crit", BaseStats = Stats(hp, 20, 10, 6),
            GrowthHp = R(2, 25), GrowthAttack = R(3, 50), GrowthDefense = R(3, 2), CritBase = R(1, 5), CritStep = R(1, 200), CritCap = R(7, 20), CritMultiplier = R(3, 2),
            XpBase = 60, XpLinear = 20, XpQuadratic = 5, RecoveryDurationMilliseconds = R(180000) }; }
        internal void Begin()
        {
            var input = EntryInput; var b = CandidateProgression.BeginAttempt(Progression, new CandidateProgressionBeginIntent { PlayerId = input.PlayerId,
                LevelId = input.Level.LevelId, LevelVersion = input.Level.LevelVersion, Context = Context, CharacterId = Character.CharacterId,
                ExpectedCharacterRevision = Character.StateRevision, ExpectedOriginalSlot = Character.OriginalSlot, ChallengeId = input.ChallengeId,
                AttemptId = input.AttemptId, EntryBaselineId = input.EntryBaselineId }, Character, Progression.StateRevision, Math());
            SaveRecoveryProcessCases.Need(b.IsAccepted, b.FieldPath); Progression = b.Next;
            var frozen = CandidateInventory.Freeze(Inventory, new CandidateInventoryFreezeIntent { PlayerId = input.PlayerId, AttemptId = input.AttemptId, EntryBaselineId = input.EntryBaselineId,
                Context = Context, ReadyParticipants = new List<CandidateInventoryActorInput> { Actor(Character) } }, Inventory.StateRevision, Math());
            SaveRecoveryProcessCases.Need(frozen.IsAccepted, frozen.FieldPath); Inventory = frozen.Next;
            var entry = new BattleEntryPreparer().PrepareCandidate(input, Math()); SaveRecoveryProcessCases.Need(entry.IsAccepted, entry.FieldPath);
            var bytes = new byte[48]; for (var offset = 0; offset < 48; offset += 16) { bytes[offset] = 42; bytes[offset + 8] = 54; }
            var bound = CandidateRandomPreparer.Prepare(entry.Entry, new CandidateSeedMaterial { Bytes = bytes, SourceCapabilityId = "isolated:B13", MappingId = CandidateRandomPreparer.SupportedMappingId }, Math());
            SaveRecoveryProcessCases.Need(bound.IsAccepted, bound.FieldPath); var run = CandidateBattleOperations.CreateCandidate(bound.Binding, Math()); SaveRecoveryProcessCases.Need(run.IsAccepted, run.FieldPath);
            var history = CandidateHistoryOperations.CreateCandidate(run.Run, Math()); SaveRecoveryProcessCases.Need(history.IsAccepted, history.FieldPath); History = history.Next;
        }
        internal void Attack(string operation, int pair, BigInteger? time = null)
        {
            var run = History.CurrentRun; var request = AttackRequest(run.CurrentSnapshot, operation, pair, Routes[pair]);
            var result = CandidateBattleOperations.EvaluateAttack(run, request, new CandidateBattleConditions { PreferenceRevision = Inventory.PreferenceRevision, ItemUseEnabled = true },
                time ?? 123, new RandomSamplingBudget(Math())); SaveRecoveryProcessCases.Need(result.IsAccepted, result.RejectionCode + " " + result.FieldPath);
            var appended = CandidateHistoryOperations.Append(History, result.NextRun, "anchor:" + operation, Math()); SaveRecoveryProcessCases.Need(appended.IsAccepted, appended.RejectionCode + " " + appended.FieldPath); History = appended.Next;
        }
        internal void Rollback(string operation, string target)
        {
            var h = History; var range = CandidateHistoryOperations.ReadRange(h, new CandidateHistoryRangeRequest { PlayerId = Character.PlayerId,
                AttemptId = h.Binding.GeneratedForAttemptId, ExpectedSceneRevision = h.CurrentRun.CurrentSnapshot.SceneRevision, HistoryAnchorId = "anchor:" + target }, Math());
            SaveRecoveryProcessCases.Need(range.IsAccepted, range.FieldPath); var result = CandidateHistoryOperations.PrepareRollback(h, new CandidateRollbackRequest {
                PlayerId = Character.PlayerId, AttemptId = h.Binding.GeneratedForAttemptId, ExpectedSceneRevision = h.CurrentRun.CurrentSnapshot.SceneRevision,
                OperationId = operation, HistoryAnchorId = "anchor:" + target }, range.Range, Math()); SaveRecoveryProcessCases.Need(result.IsAccepted, result.FieldPath); History = result.Next;
        }
        internal CandidateBusinessSnapshot Prepare()
        { return SaveRecoveryProcessCases.Core(CandidateBusinessSaveCodec.Prepare(new CandidateBusinessInput(Character.PlayerId, Character, Inventory,
            Progression, Rewards, History, RetainedRuns, RetainedRollbacks), Budget())); }
        internal static CandidateAttackRequest AttackRequest(BattleSnapshot state, string operation, int pair, List<FlowPos> route)
        { return new CandidateAttackRequest { PlayerId = state.Baseline.Entry.PlayerId, AttemptId = state.Baseline.Entry.AttemptId, OperationId = operation,
            ExpectedSceneRevision = state.SceneRevision, Actor = state.Members[0].CombatantKey, Pair = state.Enemies[pair].PairKey, Route = route }; }        private static CandidateInventoryActorInput Actor(CandidateCharacterState c) { return new CandidateInventoryActorInput { CharacterId = c.CharacterId, ClassId = c.ClassId, ClassKind = c.Definition.ClassKind, OriginalSlot = c.OriginalSlot }; }
        private static StatsInput Stats(int hp, int attack, int defense, int magic) { return new StatsInput { MaxHp = R(hp), Attack = R(attack), PhysicalDefense = R(defense), MagicDefense = R(magic), Evasion = R(0), AttackRange = 3 }; }
        private static StatsInput CopyStats(PreparedStats s) { return new StatsInput { MaxHp = s.MaxHp, Attack = s.Attack, PhysicalDefense = s.PhysicalDefense, MagicDefense = s.MagicDefense, Evasion = s.Evasion, AttackRange = s.AttackRange }; }
        private static EnemyIntentInput Strike(int n, int d) { return new EnemyIntentInput { Kind = EnemyIntentKind.Strike, Targeting = EnemyTargeting.FirstLiving, DamageKind = EntryDamageKind.Physical, DamageCoefficient = R(n, d) }; }
    }
}
