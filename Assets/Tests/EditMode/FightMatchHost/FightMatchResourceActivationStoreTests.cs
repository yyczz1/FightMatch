using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using NUnit.Framework;

namespace FightMatch.Host.Tests
{
    public sealed class FightMatchResourceActivationStoreTests
    {
        private const string T1 = "p101-1001", T2 = "p102-1002", T3 = "p103-1003";
        private static readonly string H1 = new string('1', 64), H2 = new string('2', 64), H3 = new string('3', 64);
        private static readonly string[] Main = { "active.json", "prepared.json", "restart.json" };
        private static readonly string[] Owned = Main.SelectMany(n => new[] { n, n + ".tmp", n + ".bak" }).ToArray();
        private static byte[] Bytes(string text) => new UTF8Encoding(false, true).GetBytes(text);
        private static byte[] Json(ActivationRecord r) => Bytes("{\"activationId\":\"" + r.ActivationId +
            "\",\"baseActivationId\":\"" + r.BaseActivationId + "\",\"descriptorSha256\":\"" + r.DescriptorSha256 +
            "\",\"phase\":\"" + r.Phase + "\",\"processToken\":\"" + r.ProcessToken +
            "\",\"resourceSetId\":\"" + r.ResourceSetId + "\",\"schemaVersion\":1}");
        private static ActivationSnapshot Ok(ActivationResult result)
        {
            Assert.AreEqual(ActivationOutcome.Confirmed, result.Outcome, result.SafeCode);
            Assert.IsNull(result.SafeCode); Assert.IsNotNull(result.Snapshot); return result.Snapshot;
        }
        private static void Rejected(ActivationResult result, string code = null)
        {
            Assert.AreEqual(ActivationOutcome.Rejected, result.Outcome);
            CollectionAssert.Contains(new[] { "RES_ROOT", "RES_SCHEMA", "RES_STATE", "RES_STORAGE" }, result.SafeCode);
            if (code != null) Assert.AreEqual(code, result.SafeCode);
        }
        private static ActivationSnapshot Ready(FightMatchResourceActivationStore store)
        {
            var first = Ok(store.EnterAtProcessStart("set.a", H1));
            return Ok(store.MarkBusinessReady(first.Active.ActivationId));
        }
        private static void Same(byte[] expected, byte[] actual) => CollectionAssert.AreEqual(expected, actual);
        private static void SameData(Dictionary<string, byte[]> expected, MemoryFiles files)
        {
            CollectionAssert.AreEquivalent(expected.Keys, files.Data.Keys);
            foreach (var pair in expected) Same(pair.Value, files.Data[pair.Key]);
        }

        private sealed class MemoryFiles : IActivationFiles
        {
            internal readonly string Root = Path.Combine(Path.GetPathRoot(Path.GetFullPath(".")), "activation-memory", "product");
            internal readonly Dictionary<string, byte[]> Data = new Dictionary<string, byte[]>(StringComparer.Ordinal);
            internal readonly Dictionary<string, FileAttributes> Nodes = new Dictionary<string, FileAttributes>(StringComparer.Ordinal);
            internal string FailAt, ReadMode;
            internal string DeferredDirectory;
            internal Exception DeferredError;
            internal int DeferredMoves;
            internal bool Fired;
            internal int Promotions, ReadCalls, Flushes, FaultPromotionBase;
            internal string State => Path.Combine(Root, "resource-state", "v1");
            internal string Leaf(string name) => Path.Combine(State, name);
            internal string Sentinel => Path.Combine(Root, "old-resources", "complete.bin");
            internal MemoryFiles()
            {
                AddDirectory(Root); AddDirectory(Path.GetDirectoryName(Sentinel)); Data[Sentinel] = new byte[] { 17, 0, 255 };
            }
            internal FightMatchResourceActivationStore Store(string token = T1) => new FightMatchResourceActivationStore(Root, this, token);
            internal void AddDirectory(string path)
            {
                for (var p = path; p != null; p = Path.GetDirectoryName(p)) Nodes[p] = FileAttributes.Directory;
            }
            internal void Arm(string stage) { FailAt = stage; Fired = false; FaultPromotionBase = Promotions; }
            internal void Hit(string stage)
            {
                if (stage.StartsWith("target-", StringComparison.Ordinal) && Promotions <= FaultPromotionBase) return;
                if (FailAt == stage && !Fired) { Fired = true; throw new IOException("/private/secret?token=never-return"); }
            }
            public FileAttributes Attributes(string path)
            {
                Hit("attributes");
                if (Nodes.TryGetValue(path, out var attributes)) return attributes;
                if (Data.ContainsKey(path)) return FileAttributes.Normal;
                throw new FileNotFoundException("secret missing path");
            }
            public IEnumerable<string> Entries(string directory)
            {
                Hit("enumerate");
                if (directory == DeferredDirectory) return DeferredEntries();
                return Nodes.Keys.Concat(Data.Keys).Distinct(StringComparer.Ordinal)
                    .Where(p => Path.GetDirectoryName(p) == directory).ToArray();
            }
            private IEnumerable<string> DeferredEntries()
            {
                DeferredMoves++;
                if (DeferredError != null) throw DeferredError;
                yield break;
            }
            public void CreateDirectory(string path)
            { Hit("directory-before"); AddDirectory(path); Hit("directory-after"); }
            public Stream CreateNew(string path)
            {
                Hit("create-before"); Assert.IsFalse(Data.ContainsKey(path));
                Data.Add(path, Array.Empty<byte>()); Hit("create-after");
                return new LeafStream(this, path, true);
            }
            public Stream OpenRead(string path)
            {
                var stage = path.EndsWith(".tmp", StringComparison.Ordinal) ? "tmp-open" : Promotions > 0 ? "target-open" : "open";
                Hit(stage + "-before");
                if (FailAt == "target-mismatch" && Promotions > FaultPromotionBase && !Fired && !path.EndsWith(".tmp", StringComparison.Ordinal))
                { Fired = true; return new MemoryStream(Bytes("{}"), false); }
                var result = new LeafStream(this, path, false);
                try { Hit(stage + "-after"); return result; } catch { result.Dispose(); throw; }
            }
            public void Flush(Stream stream) { Hit("flush-before"); Flushes++; stream.Flush(); Hit("flush-after"); }
            public void Move(string source, string target) => Promote(source, target, null);
            public void Replace(string source, string target, string backup) => Promote(source, target, backup);
            private void Promote(string source, string target, string backup)
            {
                Promotions++; Hit("promotion-before");
                if (backup != null) Data[backup] = (byte[])Data[target].Clone();
                Data[target] = (byte[])Data[source].Clone(); Data.Remove(source);
                Hit("promotion-after");
            }
            public void Delete(string path) { Assert.IsTrue(path.EndsWith(".tmp", StringComparison.Ordinal)); Data.Remove(path); }
            internal Dictionary<string, byte[]> Copy() => Data.ToDictionary(p => p.Key, p => (byte[])p.Value.Clone(), StringComparer.Ordinal);
            internal void Restore(Dictionary<string, byte[]> data)
            { Data.Clear(); foreach (var pair in data) Data.Add(pair.Key, (byte[])pair.Value.Clone()); }

            private sealed class LeafStream : Stream
            {
                private readonly MemoryFiles owner;
                private readonly string path;
                private readonly bool writing;
                private readonly MemoryStream stream;
                private bool closed;
                internal LeafStream(MemoryFiles owner, string path, bool writing)
                {
                    this.owner = owner; this.path = path; this.writing = writing;
                    stream = writing ? new MemoryStream() : new MemoryStream(owner.Data[path], false);
                }
                public override bool CanRead => !writing;
                public override bool CanSeek => true;
                public override bool CanWrite => writing;
                public override long Length => owner.ReadMode == "length-growth" && stream.Position == stream.Length ? stream.Length + 1 : stream.Length;
                public override long Position { get => stream.Position; set => stream.Position = value; }
                public override int Read(byte[] buffer, int offset, int count)
                {
                    owner.ReadCalls++;
                    var stage = path.EndsWith(".tmp", StringComparison.Ordinal) ? "tmp-read" : "target-read";
                    owner.Hit(stage + "-before");
                    if (owner.ReadMode == "short" && stream.Position != 0) return 0;
                    if (owner.ReadMode == "growth" && stream.Position == stream.Length) { buffer[offset] = 120; return 1; }
                    var read = stream.Read(buffer, offset, owner.ReadMode == "short" ? Math.Max(1, count / 2) : count);
                    owner.Hit(stage + "-after"); return read;
                }
                public override void Write(byte[] buffer, int offset, int count)
                {
                    owner.Hit("write-before"); stream.Write(buffer, offset, count);
                    owner.Data[path] = stream.ToArray(); owner.Hit("write-after");
                }
                public override void Flush() { }
                public override long Seek(long offset, SeekOrigin origin) => stream.Seek(offset, origin);
                public override void SetLength(long value) => stream.SetLength(value);
                protected override void Dispose(bool disposing)
                {
                    if (closed) return;
                    closed = true;
                    try { if (writing) owner.Hit("close-before"); }
                    finally { stream.Dispose(); }
                    if (writing) owner.Hit("close-after");
                    base.Dispose(disposing);
                }
            }
        }

        private sealed class RealArea : IDisposable
        {
            internal readonly string Case, Root;
            internal string State => Path.Combine(Root, "resource-state", "v1");
            internal string External => Path.Combine(Root, "external.bin");
            internal string Leaf(string name) => Path.Combine(State, name);
            internal readonly string[] Sentinels = { "profiles/keep.bin", "settings/keep.bin", "old-resources/complete.bin" };
            internal RealArea(string name)
            {
                var lease = Environment.GetEnvironmentVariable("FIGHTMATCH_ACTIVATION_TEST_ROOT");
                Assert.IsFalse(string.IsNullOrEmpty(lease), "BLOCKED: executor must lease an absolute activation/state-tests directory.");
                Assert.AreEqual(Path.GetFullPath(lease), lease);
                var normalized = lease.Replace('\\', '/');
                var marker = "/TestArtifacts/FightMatch/RES-D-ACTIVATION-001/";
                var at = normalized.LastIndexOf(marker, StringComparison.Ordinal);
                Assert.GreaterOrEqual(at, 0, "BLOCKED: wrong artifact lease.");
                var tail = normalized.Substring(at + marker.Length).Split('/');
                Assert.AreEqual(2, tail.Length); Assert.AreEqual("state-tests", tail[1]);
                Assert.IsTrue(System.Text.RegularExpressions.Regex.IsMatch(tail[0], @"\A[a-zA-Z0-9_-][a-zA-Z0-9._-]{0,63}\z"));
                for (var p = lease; p != null; p = Path.GetDirectoryName(p))
                    Assert.AreEqual(FileAttributes.Directory, File.GetAttributes(p) & (FileAttributes.Directory | FileAttributes.ReparsePoint));
                Case = Path.Combine(lease, name);
                Assert.IsFalse(Directory.EnumerateFileSystemEntries(lease).Contains(Case), "BLOCKED: case root must be absent.");
                Directory.CreateDirectory(Case); Root = Path.Combine(Case, "product"); Directory.CreateDirectory(Root);
                File.WriteAllBytes(External, new byte[] { 17, 0, 255 });
            }
            public void Dispose()
            {
                Same(new byte[] { 17, 0, 255 }, File.ReadAllBytes(External));
                var allowed = new HashSet<string>(Owned.Select(Leaf).Concat(Sentinels.Select(p => Path.Combine(Root, p)))
                    .Concat(new[] { External, Leaf("foreign.json") }), StringComparer.Ordinal);
                var directories = new[] { State, Path.Combine(Root, "resource-state"), Path.Combine(Root, "profiles"),
                    Path.Combine(Root, "settings"), Path.Combine(Root, "old-resources"), Root, Case };
                foreach (var directory in directories)
                {
                    if (!Directory.Exists(directory)) continue;
                    Assert.AreEqual((FileAttributes)0, File.GetAttributes(directory) & FileAttributes.ReparsePoint);
                    foreach (var entry in Directory.GetFileSystemEntries(directory))
                    {
                        Assert.IsTrue(allowed.Contains(entry), "Unexpected evidence retained: " + Path.GetFileName(entry));
                        Assert.IsFalse(Directory.Exists(entry)); File.Delete(entry);
                    }
                    Directory.Delete(directory);
                }
            }
        }

        private const string A1 = "a2-b5-p101-e1000";
        private static byte[] AndroidJson(ActivationRecord record) =>
            Bytes(Encoding.UTF8.GetString(Json(record)).Replace("\"schemaVersion\":1", "\"schemaVersion\":2"));

        [Test]
        public void AS09_AndroidSamplesAndCanonicalTokenBounds()
        {
            Assert.AreEqual("a2-b0-p1-e0", FightMatchResourceActivationStore.AndroidTokenFromSample(0, 1, 0, 0, 0));
            var maximum = "a2-b2147483647-p2147483647-e9223372036854775807";
            Assert.AreEqual(maximum, FightMatchResourceActivationStore.AndroidTokenFromSample(int.MaxValue,
                int.MaxValue, long.MaxValue, long.MaxValue, int.MaxValue));
            foreach (var sample in new[] {
                new long[] { -1, 1, 0, 0, -1 }, new long[] { 2147483648L, 1, 0, 0, 2147483648L },
                new long[] { 5, 0, 1, 2, 5 }, new long[] { 5, -1, 1, 2, 5 },
                new long[] { 5, 2147483648L, 1, 2, 5 }, new long[] { 5, 1, -1, 2, 5 },
                new long[] { 5, 1, 0, -1, 5 }, new long[] { 5, 1, 3, 2, 5 },
                new long[] { 5, 1, 1, 2, 6 }, new long[] { 5, 1, 1, 2, 4 } })
            {
                var token = FightMatchResourceActivationStore.AndroidTokenFromSample(sample[0], sample[1], sample[2], sample[3], sample[4]);
                Assert.IsNull(token); var f = new MemoryFiles(); var old = f.Copy();
                var nodes = f.Nodes.ToArray(); Rejected(f.Store(token).EnterAtProcessStart("set.a", H1), "RES_STATE");
                SameData(old, f); CollectionAssert.AreEquivalent(nodes, f.Nodes.ToArray()); Assert.AreEqual(0, f.Promotions);
            }
            foreach (var token in new[] { null, "", "a2-b05-p1-e0", "a2-b5-p01-e0", "a2-b5-p1-e00",
                "a2-b-1-p1-e0", "a2-b2147483648-p1-e0", "a2-b5-p0-e0", "a2-b5-p2147483648-e0",
                "a2-b5-p1-e9223372036854775808", "a2-b5-p1-e-1", A1 + " ", "a2-b5-p1-e+1" })
            {
                var f = new MemoryFiles(); var old = f.Copy();
                Rejected(f.Store(token).Read(), "RES_STATE"); SameData(old, f); Assert.AreEqual(0, f.ReadCalls);
            }
            foreach (var token in new[] { "a2-b0-p1-e0", maximum })
            {
                var f = new MemoryFiles(); var active = Ok(f.Store(token).EnterAtProcessStart("set.a", H1)).Active;
                Assert.AreEqual(2, active.SchemaVersion); Assert.AreEqual(token, active.ProcessToken);
                Same(AndroidJson(active), f.Data[f.Leaf("active.json")]); Ok(f.Store(token).Read());
            }
        }

        [Test]
        public void AS10_AndroidOrdinaryEntryRequiresStrictSuccessor()
        {
            foreach (var ready in new[] { false, true })
            {
                var f = new MemoryFiles(); var first = Ok(f.Store(A1).EnterAtProcessStart("set.a", H1)).Active;
                if (ready) Ok(f.Store(A1).MarkBusinessReady(first.ActivationId));
                var original = f.Copy(); var promotions = f.Promotions;
                var same = f.Store(A1).EnterAtProcessStart("set.a", H1);
                if (ready) Rejected(same, "RES_STATE"); else Ok(same);
                SameData(original, f); Assert.AreEqual(promotions, f.Promotions);
                foreach (var token in new[] { "a2-b5-p102-e1000", "a2-b5-p101-e999", "a2-b5-p102-e999", "a2-b4-p103-e9999" })
                {
                    Rejected(f.Store(token).EnterAtProcessStart("set.a", H1), "RES_STATE");
                    Rejected(f.Store(token).MarkBusinessReady(first.ActivationId), "RES_STATE");
                    SameData(original, f); Assert.AreEqual(promotions, f.Promotions);
                }
                foreach (var token in new[] { "a2-b5-p101-e1001", "a2-b5-p102-e1001", "a2-b6-p101-e0" })
                {
                    f.Restore(original); var successor = f.Store(token);
                    var active = Ok(successor.EnterAtProcessStart("set.a", H1)).Active;
                    Assert.AreEqual(first.ActivationId, active.ActivationId); Assert.AreEqual(token, active.ProcessToken);
                    Assert.AreEqual(2, active.SchemaVersion);
                    var entered = f.Copy(); Ok(f.Store(token).EnterAtProcessStart("set.a", H1)); SameData(entered, f);
                    Ok(successor.MarkBusinessReady(active.ActivationId));
                    var after = f.Copy(); Rejected(f.Store(A1).Prepare("set.b", H2), "RES_STATE"); SameData(after, f);
                }
            }
        }

        [Test]
        public void AS11_AndroidPendingRestartRequiresStrictSuccessor()
        {
            var f = new MemoryFiles(); var owner = f.Store(A1); var ready = Ready(owner);
            var prepared = Ok(owner.Prepare("set.b", H2)).Prepared; Ok(owner.RequestRestart(prepared.ActivationId));
            f.Data[f.Leaf("active.json.bak")] = AndroidJson(ready.Active);
            var pending = f.Copy(); var promotions = f.Promotions;
            foreach (var token in new[] { A1, "a2-b5-p102-e1000", "a2-b5-p101-e999", "a2-b4-p103-e9999" })
            {
                Rejected(f.Store(token).EnterAtProcessStart("set.b", H2), "RES_STATE");
                if (token != A1)
                {
                    Rejected(f.Store(token).Prepare("set.c", H3), "RES_STATE");
                    Rejected(f.Store(token).RequestRestart(prepared.ActivationId), "RES_STATE");
                }
                SameData(pending, f); Assert.AreEqual(promotions, f.Promotions);
            }
            foreach (var token in new[] { "a2-b5-p101-e1001", "a2-b5-p102-e1001", "a2-b6-p101-e0" })
            {
                f.Restore(pending); var successor = f.Store(token);
                Rejected(successor.EnterAtProcessStart("set.b", H3), "RES_STATE"); SameData(pending, f);
                var entered = Ok(successor.EnterAtProcessStart("set.b", H2)).Active;
                Assert.AreEqual(prepared.ActivationId, entered.ActivationId); Assert.AreEqual(token, entered.ProcessToken);
                Same(AndroidJson(ready.Active), f.Data[f.Leaf("active.json.bak")]);
                Same(pending[f.Leaf("prepared.json")], f.Data[f.Leaf("prepared.json")]);
                Same(pending[f.Leaf("restart.json")], f.Data[f.Leaf("restart.json")]);
                Ok(successor.MarkBusinessReady(entered.ActivationId)); Rejected(successor.RequestRestart(entered.ActivationId), "RES_STATE");
                Same(pending[f.Sentinel], f.Data[f.Sentinel]);
            }
            f.Restore(pending);
            f.Data[f.Leaf("active.json")] = Bytes(Encoding.UTF8.GetString(f.Data[f.Leaf("active.json")]).Replace(A1, "a2-b5-p101-e2000"));
            var contradictory = f.Copy();
            Rejected(f.Store("a2-b5-p102-e1500").EnterAtProcessStart("set.b", H2), "RES_STATE"); SameData(contradictory, f);
        }

        [Test]
        public void AS12_AndroidRejectsLegacyMixedAndMismatchedSchemasWithoutWrites()
        {
            var f = new MemoryFiles(); var legacy = f.Store(); Ready(legacy);
            var oldPrepared = Ok(legacy.Prepare("set.b", H2)).Prepared; Ok(legacy.RequestRestart(oldPrepared.ActivationId));
            foreach (var leaf in Main) f.Data[f.Leaf(leaf + ".bak")] = (byte[])f.Data[f.Leaf(leaf)].Clone();
            var old = f.Copy(); var promotions = f.Promotions; var nodes = f.Nodes.ToArray();
            var android = f.Store(A1);
            foreach (var result in new[] { android.Read(), android.EnterAtProcessStart("set.a", H1),
                android.EnterAtProcessStart("set.b", H2), android.MarkBusinessReady(oldPrepared.ActivationId),
                android.Prepare("set.c", H3), android.RequestRestart(oldPrepared.ActivationId) }) Rejected(result, "RES_STATE");
            SameData(old, f); Assert.AreEqual(promotions, f.Promotions); CollectionAssert.AreEquivalent(nodes, f.Nodes.ToArray());

            var v = new MemoryFiles(); var store = v.Store(A1); Ready(store);
            var prepared = Ok(store.Prepare("set.b", H2)).Prepared; Ok(store.RequestRestart(prepared.ActivationId));
            foreach (var leaf in Main) v.Data[v.Leaf(leaf + ".bak")] = (byte[])v.Data[v.Leaf(leaf)].Clone();
            var baseline = v.Copy(); promotions = v.Promotions;
            Rejected(v.Store(T2).Read(), "RES_STATE"); Rejected(v.Store(T2).EnterAtProcessStart("set.b", H2), "RES_STATE"); SameData(baseline, v);
            foreach (var name in Main.SelectMany(x => new[] { x, x + ".bak" }))
            {
                v.Restore(baseline); v.Data[v.Leaf(name)] = old[f.Leaf(name)]; var mixed = v.Copy();
                Rejected(v.Store("a2-b6-p102-e0").Read(), "RES_STATE");
                Rejected(v.Store("a2-b6-p102-e0").EnterAtProcessStart("set.b", H2), "RES_STATE");
                SameData(mixed, v); Assert.AreEqual(promotions, v.Promotions);
            }
            var json = Encoding.UTF8.GetString(baseline[v.Leaf("active.json")]);
            foreach (var change in new[] { new[] { "\"schemaVersion\":2", "\"schemaVersion\":1" },
                new[] { A1, T1 }, new[] { "\"schemaVersion\":2", "\"schemaVersion\":3" },
                new[] { A1, "a2-b05-p101-e1000" }, new[] { A1, "a2-b5-p101-e9223372036854775808" } })
            {
                v.Restore(baseline); v.Data[v.Leaf("active.json")] = Bytes(json.Replace(change[0], change[1]));
                var bad = v.Copy(); Rejected(v.Store(A1).Read());
                Rejected(v.Store("a2-b6-p102-e0").EnterAtProcessStart("set.b", H2)); SameData(bad, v);
                Assert.AreEqual(promotions, v.Promotions);
            }
        }

        [Test]
        public void AS01_InitialBuiltInAndOrdinaryReentry()
        {
            var f = new MemoryFiles(); var store = f.Store();
            var empty = Ok(store.Read()); Assert.IsNull(empty.Active); Assert.IsNull(empty.Prepared); Assert.IsNull(empty.Restart);
            var first = Ok(store.EnterAtProcessStart("set.a", H1)); var id = first.Active.ActivationId;
            Assert.AreEqual("CodeEntered", first.Active.Phase); Assert.AreEqual("", first.Active.BaseActivationId);
            Assert.AreEqual(T1, first.Active.ProcessToken); Assert.IsNull(first.Prepared); Assert.IsNull(first.Restart);
            Same(Json(first.Active), f.Data[f.Leaf("active.json")]);
            var original = f.Copy();
            Rejected(store.Prepare("set.b", H2)); Rejected(store.EnterAtProcessStart("set.b", H2));
            Rejected(store.EnterAtProcessStart("set.a", H2)); Rejected(store.MarkBusinessReady(new string('f', 32)));
            SameData(original, f); Ok(store.EnterAtProcessStart("set.a", H1)); SameData(original, f);
            var ready = Ok(store.MarkBusinessReady(id)); Assert.AreEqual("BusinessReady", ready.Active.Phase);
            original = f.Copy(); Ok(store.MarkBusinessReady(id)); Rejected(store.EnterAtProcessStart("set.a", H1)); SameData(original, f);
            var next = f.Store(T2); Rejected(next.MarkBusinessReady(id));
            var entered = Ok(next.EnterAtProcessStart("set.a", H1));
            Assert.AreEqual(id, entered.Active.ActivationId); Assert.AreEqual("", entered.Active.BaseActivationId);
            Same(Json(ready.Active), f.Data[f.Leaf("active.json.bak")]);
            var backup = (byte[])f.Data[f.Leaf("active.json.bak")].Clone();
            Ok(next.EnterAtProcessStart("set.a", H1)); Ok(next.MarkBusinessReady(id)); Same(backup, f.Data[f.Leaf("active.json.bak")]);
            Rejected(next.Prepare("latest", H2), "RES_SCHEMA"); Rejected(next.Prepare("set.a", H2), "RES_STATE");
            Rejected(f.Store("p0-1").Read(), "RES_STATE");
        }

        [Test]
        public void AS02_UpdateRequiresNextProcess()
        {
            var f = new MemoryFiles(); var a = f.Store(); var initial = Ready(a);
            var p = Ok(a.Prepare("set.b", H2)).Prepared; Assert.AreEqual(initial.Active.ActivationId, p.BaseActivationId);
            Ok(f.Store().Read()); var preparedBytes = (byte[])f.Data[f.Leaf("prepared.json")].Clone();
            Assert.AreEqual(p.ActivationId, Ok(f.Store().Prepare("set.b", H2)).Prepared.ActivationId); Same(preparedBytes, f.Data[f.Leaf("prepared.json")]);
            Rejected(a.RequestRestart(initial.Active.ActivationId)); Ok(a.RequestRestart(p.ActivationId));
            Ok(f.Store().Read()); var requestedBytes = (byte[])f.Data[f.Leaf("restart.json")].Clone();
            Ok(f.Store().RequestRestart(p.ActivationId)); Same(requestedBytes, f.Data[f.Leaf("restart.json")]);
            Rejected(f.Store().EnterAtProcessStart("set.b", H2)); Rejected(a.Prepare("set.c", H3));
            var b = f.Store(T2); Rejected(b.EnterAtProcessStart("set.b", H3));
            var enteredB = Ok(b.EnterAtProcessStart("set.b", H2)); Assert.AreEqual(p.ActivationId, enteredB.Active.ActivationId);
            Ok(f.Store(T2).Read()); Rejected(b.Prepare("set.c", H3)); Ok(b.MarkBusinessReady(p.ActivationId));
            Rejected(b.RequestRestart(p.ActivationId), "RES_STATE");
            Ok(f.Store(T2).Read()); var nextP = Ok(b.Prepare("set.c", H3)).Prepared;
            var nextBytes = (byte[])f.Data[f.Leaf("prepared.json")].Clone();
            Assert.AreEqual(p.ActivationId, nextP.BaseActivationId); Assert.AreEqual(p.ActivationId, Ok(f.Store(T2).Read()).Restart.ActivationId);
            var resumed = f.Store(T3); var sameB = Ok(resumed.EnterAtProcessStart("set.b", H2)); Ok(f.Store(T3).Read());
            Assert.AreEqual(p.ActivationId, sameB.Active.ActivationId); Assert.AreEqual(p.BaseActivationId, sameB.Active.BaseActivationId);
            Rejected(resumed.RequestRestart(nextP.ActivationId)); Ok(resumed.MarkBusinessReady(p.ActivationId));
            Assert.AreEqual(nextP.ActivationId, Ok(resumed.Prepare("set.c", H3)).Prepared.ActivationId);
            Same(nextBytes, f.Data[f.Leaf("prepared.json")]);
            Ok(resumed.RequestRestart(nextP.ActivationId)); requestedBytes = (byte[])f.Data[f.Leaf("restart.json")].Clone();
            Ok(resumed.RequestRestart(nextP.ActivationId)); Same(requestedBytes, f.Data[f.Leaf("restart.json")]);
            Rejected(f.Store(T3).EnterAtProcessStart("set.c", H3));
            var c = f.Store("p104-1004"); var enteredC = Ok(c.EnterAtProcessStart("set.c", H3));
            Assert.AreEqual(nextP.ActivationId, enteredC.Active.ActivationId); Ok(f.Store("p104-1004").Read());
            Ok(c.MarkBusinessReady(nextP.ActivationId)); Ok(f.Store("p104-1004").Read());
            var consumed = f.Copy(); var r = Encoding.UTF8.GetString(consumed[f.Leaf("restart.json")]);
            var wrong = new string(nextP.ActivationId[0] == 'a' ? 'b' : 'a', 32);
            foreach (var change in new[] {
                new[] { nextP.ActivationId, wrong }, new[] { p.ActivationId, initial.Active.ActivationId },
                new[] { "set.c", "set.z" }, new[] { H3, H1 },
                new[] { "\"baseActivationId\":\"" + p.ActivationId + "\"", "\"baseActivationId\":\"\"" },
                new[] { "\"baseActivationId\":\"" + p.ActivationId + "\"", "\"baseActivationId\":\"" + nextP.ActivationId + "\"" },
                new[] { "RequestRestart", "Prepared" }, new[] { T3, "p0-1" }, new[] { "\"schemaVersion\":1", "\"schemaVersion\":2" } })
            {
                f.Restore(consumed); f.Data[f.Leaf("restart.json")] = Bytes(r.Replace(change[0], change[1]));
                var damaged = f.Copy(); Rejected(f.Store(T3).Read()); SameData(damaged, f);
            }
            f.Restore(consumed); f.Data[f.Leaf("restart.json")] = Bytes(r.Replace(T3, T1));
            Ok(f.Store(T3).Read()); // Valid different token does not change Consumed; this is not a real restart.
            f.Restore(consumed); f.Data[f.Leaf("active.json")] = Bytes(Encoding.UTF8.GetString(f.Data[f.Leaf("active.json")]).Replace("BusinessReady", "CodeEntered"));
            Ok(f.Store(T3).Read()); Rejected(f.Store("p104-1004").Prepare("set.d", new string('4', 64)), "RES_STATE");
            f.Restore(consumed); var d = Ok(c.Prepare("set.d", new string('4', 64))).Prepared; Ok(c.RequestRestart(d.ActivationId));
            var pending = f.Copy();
            foreach (var mode in new[] { "missing", "no-active", "base", "stale-restart" })
            {
                f.Restore(pending);
                if (mode == "missing") f.Data.Remove(f.Leaf("prepared.json"));
                else if (mode == "no-active") f.Data.Remove(f.Leaf("active.json"));
                else if (mode == "base") f.Data[f.Leaf("prepared.json")] = Bytes(Encoding.UTF8.GetString(f.Data[f.Leaf("prepared.json")]).Replace(nextP.ActivationId, initial.Active.ActivationId));
                else f.Data[f.Leaf("restart.json")] = Json(new ActivationRecord(p.ActivationId, initial.Active.ActivationId, H2, "RequestRestart", T1, "set.b"));
                var damaged = f.Copy(); Rejected(f.Store(T3).Read()); SameData(damaged, f);
            }
        }

        [Test]
        public void AS03_StrictBoundedRecords()
        {
            var f = new MemoryFiles(); var good = Ready(f.Store()).Active;
            var baseline = f.Copy(); var json = Encoding.UTF8.GetString(Json(good));
            var bad = new List<byte[]> {
                Bytes(json + " "), Bytes("\uFEFF" + json), new byte[] { 255 },
                Bytes(json.Replace("\"schemaVersion\":1", "\"schemaVersion\":1,\"schemaVersion\":1")),
                Bytes(json.Replace("\"schemaVersion\":1", "\"extra\":0,\"schemaVersion\":1")),
                Bytes(json.Replace("\"processToken\":\"" + T1 + "\",", "")),
                Bytes(json.Replace("BusinessReady", "Prepared")), Bytes(json.Replace("\"schemaVersion\":1", "\"schemaVersion\":1.0")),
                Bytes(json.Replace(H1, new string('g', 64))), Bytes(json.Replace(good.ActivationId, "BAD")),
                Bytes(json.Replace("set.a", "set\\u002ea")), Bytes(json.Replace(T1, "p01-1")),
                Bytes(json.PadRight(2048)), Bytes(json.PadRight(2049)) };
            foreach (var bytes in bad)
            {
                f.Restore(baseline); f.Data[f.Leaf("active.json")] = bytes; f.ReadCalls = 0;
                var damaged = f.Copy(); Rejected(f.Store().Read(), "RES_SCHEMA"); SameData(damaged, f);
                if (bytes.Length == 2049) Assert.AreEqual(0, f.ReadCalls);
                if (bytes.Length == 2048) Assert.Greater(f.ReadCalls, 0);
            }
            foreach (var mode in new[] { "short", "growth", "length-growth" })
            {
                f.Restore(baseline); f.ReadMode = mode; var unchanged = f.Copy();
                Rejected(f.Store().Read(), "RES_SCHEMA"); SameData(unchanged, f);
            }
            f.ReadMode = null; f.Restore(baseline); f.Data[f.Leaf("active.json.tmp")] = Json(good);
            var residue = f.Copy(); Rejected(f.Store().Read(), "RES_STATE"); SameData(residue, f);
            f.Data.Remove(f.Leaf("active.json.tmp")); f.Data[f.Leaf("active.json.bak")] = Json(good); f.Data.Remove(f.Leaf("active.json"));
            residue = f.Copy(); Rejected(f.Store().EnterAtProcessStart("set.a", H1), "RES_STATE"); SameData(residue, f);
        }

        [Test]
        public void AS04_PrePromotionFailuresPreserveActive()
        {
            foreach (var fault in new[] { "attributes", "enumerate", "directory-before", "directory-after", "create-before", "create-after",
                "write-before", "write-after", "flush-before", "flush-after", "close-before", "close-after",
                "tmp-open-before", "tmp-open-after", "tmp-read-before", "tmp-read-after" })
            {
                var f = new MemoryFiles(); var store = f.Store(); var initial = fault.StartsWith("directory", StringComparison.Ordinal);
                if (!initial) Ready(store);
                var snapshot = Ok(store.Read()); var old = f.Copy(); var promotions = f.Promotions; f.Arm(fault);
                var result = initial ? store.EnterAtProcessStart("set.a", H1) : store.Prepare("set.b", H2);
                Rejected(result); Assert.IsTrue(f.Fired, fault); Assert.AreSame(snapshot, result.Snapshot);
                Assert.AreEqual(promotions, f.Promotions); Same(old[f.Sentinel], f.Data[f.Sentinel]);
                foreach (var leaf in Main)
                {
                    Assert.AreEqual(old.ContainsKey(f.Leaf(leaf)), f.Data.ContainsKey(f.Leaf(leaf)));
                    if (old.ContainsKey(f.Leaf(leaf))) Same(old[f.Leaf(leaf)], f.Data[f.Leaf(leaf)]);
                }
            }
        }

        [Test]
        public void AS13_StorageFailureReportsTheOriginalOperationBeforeCleanup()
        {
            var cases = new[] {
                new[] { "attributes", "activation.guard.ancestor.attributes" },
                new[] { "enumerate", "activation.guard.ancestor.entries" },
                new[] { "directory-before", "activation.create.outer" },
                new[] { "create-before", "activation.commit.create" },
                new[] { "write-before", "activation.commit.write" },
                new[] { "flush-before", "activation.commit.flush" },
                new[] { "close-before", "activation.commit.close" },
                new[] { "tmp-open-before", "activation.read.open" },
                new[] { "tmp-read-before", "activation.read.body" } };
            foreach (var item in cases)
            {
                var f = new MemoryFiles(); var store = f.Store();
                if (item[0] != "directory-before") Ready(store);
                var snapshot = Ok(store.Read()); var old = f.Copy(); var promotions = f.Promotions;
                f.Arm(item[0]);
                var result = item[0] == "directory-before" ? store.EnterAtProcessStart("set.a", H1) : store.Prepare("set.b", H2);
                Rejected(result, "RES_STORAGE"); Assert.IsTrue(f.Fired, item[0]);
                Assert.AreEqual(item[1] + "; error=IOException", store.StorageDiagnostic);
                Assert.AreSame(snapshot, result.Snapshot); Assert.AreEqual(promotions, f.Promotions);
                SameData(old, f);
                f.FailAt = null; Ok(store.Read()); Assert.IsNull(store.StorageDiagnostic);
            }
        }

        [Test]
        public void AS14_DeferredEnumerationFailureReportsScopeWithoutLeakingExceptionData()
        {
            const string secret = "/private/profile-secret?token=never-return\nforged-log";
            var errors = new Exception[] { new UnauthorizedAccessException(secret), new IOException(secret),
                new DirectoryNotFoundException(secret), new InvalidOperationException(secret) };
            var kinds = new[] { "UnauthorizedAccessException", "IOException", "DirectoryNotFoundException", "OtherException" };
            for (var i = 0; i < errors.Length; i++)
            foreach (var scope in new[] { "ancestor", "product", "outer", "state" })
            {
                var f = new MemoryFiles(); var store = f.Store("a2-b5-p101-e1000");
                if (scope == "outer" || scope == "state") Ready(store);
                f.DeferredDirectory = scope == "ancestor" ? Path.GetPathRoot(f.Root) : scope == "product" ? f.Root :
                    scope == "outer" ? Path.GetDirectoryName(f.State) : f.State;
                f.DeferredError = errors[i]; var old = f.Copy(); var nodes = f.Nodes.ToArray(); var promotions = f.Promotions;
                Rejected(store.EnterAtProcessStart("set.a", H1), "RES_STORAGE");
                var stage = scope == "ancestor" ? "guard.ancestor" : scope == "product" ? "exists.product" : "layout." + scope;
                Assert.AreEqual("activation." + stage + ".entries; error=" + kinds[i], store.StorageDiagnostic);
                Assert.AreEqual(1, f.DeferredMoves); Assert.AreEqual(promotions, f.Promotions);
                SameData(old, f); CollectionAssert.AreEquivalent(nodes, f.Nodes.ToArray());
                f.DeferredDirectory = null; Ok(store.Read()); Assert.IsNull(store.StorageDiagnostic);
            }
        }

        [Test]
        public void AS15_UnknownCommitKeepsItsDiagnosticAndDoesNotRetry()
        {
            var f = new MemoryFiles(); var store = f.Store(); var snapshot = Ready(store); f.Arm("promotion-before");
            var result = store.Prepare("set.b", H2);
            Assert.AreEqual(ActivationOutcome.UnknownCommit, result.Outcome);
            Assert.AreEqual("RES_COMMIT_UNKNOWN", result.SafeCode); Assert.AreSame(snapshot, result.Snapshot);
            const string diagnostic = "activation.commit.move; error=IOException";
            Assert.AreEqual(diagnostic, store.StorageDiagnostic);
            var uncertain = f.Copy(); var promotions = f.Promotions;
            Assert.AreEqual(ActivationOutcome.UnknownCommit, store.Read().Outcome);
            Assert.AreEqual(ActivationOutcome.UnknownCommit, store.Prepare("set.b", H2).Outcome);
            Assert.AreEqual(diagnostic, store.StorageDiagnostic); Assert.AreEqual(promotions, f.Promotions);
            SameData(uncertain, f);
        }

        [Test]
        public void AS05_UnknownCommitNeverAdvances()
        {
            foreach (var fault in new[] { "promotion-before", "promotion-after", "target-open-before", "target-open-after",
                "target-read-before", "target-read-after", "target-mismatch" })
            {
                var f = new MemoryFiles(); var oldStore = f.Store(); var ready = Ready(oldStore);
                var p = Ok(oldStore.Prepare("set.b", H2)).Prepared; Ok(oldStore.RequestRestart(p.ActivationId));
                f.Data[f.Leaf("active.json.bak")] = Json(ready.Active);
                var store = f.Store(T2); var confirmed = Ok(store.Read()); var oldActive = Json(ready.Active);
                var count = f.Promotions; f.Arm(fault);
                var result = store.EnterAtProcessStart("set.b", H2);
                Assert.AreEqual(ActivationOutcome.UnknownCommit, result.Outcome); Assert.AreEqual("RES_COMMIT_UNKNOWN", result.SafeCode);
                Assert.IsTrue(f.Fired, fault); Assert.AreSame(confirmed, result.Snapshot); Assert.AreEqual(count + 1, f.Promotions);
                Same(oldActive, f.Data[f.Leaf("active.json.bak")]); Same(new byte[] { 17, 0, 255 }, f.Data[f.Sentinel]);
                var unknownBytes = f.Copy();
                Assert.AreEqual(ActivationOutcome.UnknownCommit, store.MarkBusinessReady(p.ActivationId).Outcome);
                Assert.AreEqual(ActivationOutcome.UnknownCommit, store.Prepare("set.c", H3).Outcome);
                Assert.AreSame(confirmed, store.Read().Snapshot); SameData(unknownBytes, f);
                Assert.AreEqual(count + 1, f.Promotions); f.FailAt = null;
                if (fault == "promotion-before")
                {
                    Assert.IsTrue(f.Data.ContainsKey(f.Leaf("active.json.tmp"))); Same(oldActive, f.Data[f.Leaf("active.json")]);
                    Rejected(f.Store(T3).Read(), "RES_STATE"); // Preserved uncertain tmp forbids guessing or recovery.
                }
                else
                {
                    Assert.IsFalse(f.Data.ContainsKey(f.Leaf("active.json.tmp")));
                    var actual = Ok(f.Store(T3).Read()); Assert.AreEqual(p.ActivationId, actual.Active.ActivationId);
                    Assert.AreEqual("CodeEntered", actual.Active.Phase); Same(Json(actual.Active), f.Data[f.Leaf("active.json")]);
                }
            }
        }

        [Test]
        public void AS06_RealFilesMoveReplaceAndReopen()
        {
            using (var area = new RealArea("AS06"))
            {
                var store = new FightMatchResourceActivationStore(area.Root);
                var first = Ok(store.EnterAtProcessStart("set.a", H1));
                using (var process = System.Diagnostics.Process.GetCurrentProcess())
                    Assert.AreEqual("p" + process.Id.ToString(System.Globalization.CultureInfo.InvariantCulture) + "-" +
                        process.StartTime.ToUniversalTime().Ticks.ToString(System.Globalization.CultureInfo.InvariantCulture), first.Active.ProcessToken);
                Same(Json(first.Active), File.ReadAllBytes(area.Leaf("active.json")));
                var ready = Ok(store.MarkBusinessReady(first.Active.ActivationId));
                var p = Ok(store.Prepare("set.b", H2)).Prepared; Ok(store.RequestRestart(p.ActivationId));
                var sameProcess = new FightMatchResourceActivationStore(area.Root);
                Ok(sameProcess.Read()); Rejected(sameProcess.EnterAtProcessStart("set.b", H2), "RES_STATE");
                var simulatedNext = new FightMatchResourceActivationStore(area.Root, new SystemActivationFiles(), T1);
                var entered = Ok(simulatedNext.EnterAtProcessStart("set.b", H2));
                Assert.AreEqual(p.ActivationId, entered.Active.ActivationId); Same(Json(ready.Active), File.ReadAllBytes(area.Leaf("active.json.bak")));
                var backup = File.ReadAllBytes(area.Leaf("active.json.bak")); Ok(simulatedNext.MarkBusinessReady(p.ActivationId));
                Same(backup, File.ReadAllBytes(area.Leaf("active.json.bak")));
                var reopened = Ok(new FightMatchResourceActivationStore(area.Root).Read());
                Assert.AreEqual("BusinessReady", reopened.Active.Phase);
                Same(Json(reopened.Active), File.ReadAllBytes(area.Leaf("active.json")));
                Same(Json(reopened.Prepared), File.ReadAllBytes(area.Leaf("prepared.json")));
                Same(Json(reopened.Restart), File.ReadAllBytes(area.Leaf("restart.json")));
                var names = Directory.GetFileSystemEntries(area.State).Select(Path.GetFileName).ToArray();
                Assert.LessOrEqual(names.Length, 9); Assert.IsTrue(names.All(Owned.Contains));
                Assert.IsFalse(names.Any(n => n.EndsWith(".tmp", StringComparison.Ordinal)));
                TestContext.Progress.WriteLine("Real System.IO Move/Replace/Flush and reopen exercised; injected next token is not a real process restart.");
            }
        }

#if UNITY_EDITOR_OSX
        [DllImport("libSystem.B.dylib", EntryPoint = "symlink", SetLastError = true)]
        private static extern int CreateMacSymlink(string target, string path);
#endif
        private static void CreateAS07Link(RealArea area, string target)
        {
            Assert.IsNotNull(area, "BLOCKED: missing AS07 fixture lease.");
            Assert.AreEqual("AS07", Path.GetFileName(area.Case), "BLOCKED: wrong case lease.");
            Assert.AreEqual(Path.Combine(area.Case, "product"), area.Root);
            var link = area.Leaf("active.json"); var absent = Path.Combine(area.Root, "absent-target");
            Assert.IsTrue(target == area.External || target == absent, "BLOCKED: link target outside AS07 product.");
            Assert.AreEqual(Path.Combine(area.Root, "resource-state", "v1", "active.json"), link);
            for (var p = area.State; p != null; p = Path.GetDirectoryName(p))
                Assert.AreEqual(FileAttributes.Directory, File.GetAttributes(p) & (FileAttributes.Directory | FileAttributes.ReparsePoint));
            Assert.AreEqual((FileAttributes)0, File.GetAttributes(area.External) & (FileAttributes.Directory | FileAttributes.ReparsePoint));
            Assert.IsFalse(Directory.EnumerateFileSystemEntries(area.Root).Contains(absent, StringComparer.OrdinalIgnoreCase), "BLOCKED: dangling target must remain absent.");
            Assert.IsFalse(Directory.EnumerateFileSystemEntries(area.State).Contains(link, StringComparer.OrdinalIgnoreCase), "BLOCKED: link must be absent.");
#if UNITY_EDITOR_OSX
            try { Assert.AreEqual(0, CreateMacSymlink(target, link), "BLOCKED: native symlink returned nonzero; no mocked pass."); }
            catch (DllNotFoundException) { Assert.Fail("BLOCKED: libSystem.B.dylib unavailable for AS07."); }
            catch (EntryPointNotFoundException) { Assert.Fail("BLOCKED: symlink symbol unavailable for AS07."); }
#else
            var createLink = typeof(File).GetMethod("CreateSymbolicLink", new[] { typeof(string), typeof(string) });
            Assert.IsNotNull(createLink, "BLOCKED: runtime lacks a supported real symlink fixture API; memory cases do not replace this evidence.");
            try { createLink.Invoke(null, new object[] { area.Leaf("active.json"), target }); }
            catch (Exception) { Assert.Fail("BLOCKED: real symlink creation unavailable; no mocked pass."); }
#endif
        }

        [Test]
        public void AS07_RootAndResidueGuards()
        {
            var f = new MemoryFiles(); Ready(f.Store()); var baseline = f.Copy();
            foreach (var root in new[] { ".", "relative", Path.GetPathRoot(f.Root), f.Root + Path.DirectorySeparatorChar,
                Path.Combine(f.Root, "..", "product"), Path.Combine(f.Root, ".") })
            {
                Rejected(new FightMatchResourceActivationStore(root, f, T1).Read()); SameData(baseline, f);
            }
            foreach (var link in new[] { f.Root, Path.GetDirectoryName(f.Root), f.Leaf("active.json") })
            {
                var had = f.Nodes.TryGetValue(link, out var attributes);
                f.Nodes[link] = FileAttributes.ReparsePoint | (link == f.Leaf("active.json") ? (FileAttributes)0 : FileAttributes.Directory);
                Rejected(f.Store().Read(), "RES_ROOT"); Rejected(f.Store().Prepare("set.b", H2)); SameData(baseline, f);
                if (had) f.Nodes[link] = attributes; else f.Nodes.Remove(link);
            }
            foreach (var name in new[] { "foreign.json", "active.json" })
            {
                var path = f.Leaf(name); f.Nodes[path] = FileAttributes.Directory;
                Rejected(f.Store().Read(), "RES_ROOT"); Rejected(f.Store().Prepare("set.b", H2)); SameData(baseline, f); f.Nodes.Remove(path);
            }
            f.Arm("enumerate"); Rejected(f.Store().Read(), "RES_STORAGE"); SameData(baseline, f);
            using (var area = new RealArea("AS07"))
            {
                Directory.CreateDirectory(area.State); File.WriteAllBytes(area.Leaf("foreign.json"), new byte[] { 7 });
                var store = new FightMatchResourceActivationStore(area.Root);
                Rejected(store.Read(), "RES_ROOT"); Rejected(store.EnterAtProcessStart("set.a", H1), "RES_ROOT");
                Same(new byte[] { 7 }, File.ReadAllBytes(area.Leaf("foreign.json"))); File.Delete(area.Leaf("foreign.json"));
                Ok(store.Read()); Rejected(new FightMatchResourceActivationStore(Path.Combine(area.Case, "PRODUCT")).Read());
                Directory.CreateDirectory(area.Leaf("active.json")); Rejected(store.Read(), "RES_ROOT"); Directory.Delete(area.Leaf("active.json"));
                foreach (var target in new[] { area.External, Path.Combine(area.Root, "absent-target") })
                {
                    CreateAS07Link(area, target);
                    try
                    {
                        var code = target == area.External ? "RES_ROOT" : null;
                        Rejected(store.Read(), code); Rejected(store.EnterAtProcessStart("set.a", H1), code);
                        Same(new byte[] { 17, 0, 255 }, File.ReadAllBytes(area.External));
                    }
                    finally { File.Delete(area.Leaf("active.json")); }
                }
            }
        }

        [Test]
        public void AS08_PlayerAndOldSetRemainUntouched()
        {
            using (var area = new RealArea("AS08"))
            {
                var sentinels = area.Sentinels.ToDictionary(p => Path.Combine(area.Root, p), p => Bytes("complete:" + p));
                foreach (var pair in sentinels) { Directory.CreateDirectory(Path.GetDirectoryName(pair.Key)); File.WriteAllBytes(pair.Key, pair.Value); }
                var store = new FightMatchResourceActivationStore(area.Root, new SystemActivationFiles(), T1);
                var entered = Ok(store.EnterAtProcessStart("set.a", H1));
                foreach (var result in new[] { store.EnterAtProcessStart("set.b", H2), store.Prepare("set.b", H2), store.MarkBusinessReady(new string('f', 32)) })
                {
                    Rejected(result); Assert.IsFalse(result.SafeCode.Contains(area.Root));
                    Assert.AreEqual(entered.Active.ActivationId, result.Snapshot.Active.ActivationId);
                }
                var again = Ok(new FightMatchResourceActivationStore(area.Root, new SystemActivationFiles(), T2).EnterAtProcessStart("set.a", H1));
                Assert.AreEqual(entered.Active.ActivationId, again.Active.ActivationId);
                File.WriteAllBytes(area.Leaf("active.json.bak"), Json(entered.Active)); File.WriteAllBytes(area.Leaf("active.json"), Bytes("{"));
                Rejected(new FightMatchResourceActivationStore(area.Root).Read(), "RES_SCHEMA");
                Same(Bytes("{"), File.ReadAllBytes(area.Leaf("active.json"))); Same(Json(entered.Active), File.ReadAllBytes(area.Leaf("active.json.bak")));
                foreach (var pair in sentinels) Same(pair.Value, File.ReadAllBytes(pair.Key));
                Assert.IsTrue(Directory.GetFileSystemEntries(area.State).Select(Path.GetFileName).All(Owned.Contains));
                Assert.IsFalse(typeof(FightMatchResourceActivationStore).IsPublic);
                Assert.IsFalse(typeof(ActivationResult).IsPublic); Assert.IsFalse(typeof(ActivationSnapshot).IsPublic);
            }
        }
    }
}
