#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using FightMatch.Core;
using FightMatch.Platform;
using FightMatch.Presentation;
using NUnit.Framework;
using UnityEngine;

namespace FightMatch.Host.Tests
{
    public sealed class FightMatchHostSaveIsolationTests
    {
        private const string Id = "0123456789abcdef0123456789abcdef";
        private const string Candidate = "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef";
        private string owned, project, real, persistent, activation, ownerLock;
        private string[] Args => new[] { "-fmHostSaveIsolationId", Id, "-fmHostSaveIsolationCandidate", Candidate };

        [Serializable] private sealed class Activation { public Binding hostSaveIsolation; }
        [Serializable] private sealed class Binding
        {
            public int schemaVersion = 1;
            public string activationId, canonicalProjectRoot, persistentRoot, candidateSha256, ownerThread, ownerTurn;
        }
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
                Assert.Ignore("Mac realpath and lease behavior require the separately authorized Mac run.");
            var created = Path.Combine(Path.GetTempPath(), "FightMatch-HISO-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(created);
            var buffer = new byte[4096];
            Assert.AreNotEqual(IntPtr.Zero, RealPath(created, buffer));
            owned = System.Text.Encoding.UTF8.GetString(buffer, 0, Array.IndexOf(buffer, (byte)0));
            project = Path.Combine(owned, "Project");
            real = Path.Combine(owned, "SimulatedAbsentReal", "nested");
            persistent = project + "/TestArtifacts/FightMatch/UGUI-01/native-scene-reopen-001/" + Id + "/host-save/persistent";
            activation = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(persistent)), "activation.json");
            ownerLock = Path.Combine(Path.GetDirectoryName(persistent), "owner.lock");
        }

        [TearDown]
        public void TearDown()
        {
            if (owned != null && Directory.Exists(owned)) Directory.Delete(owned, true);
        }

        private string Resolve(string[] args = null, string projectRoot = null, string realRoot = null,
            RuntimePlatform platform = RuntimePlatform.OSXEditor) =>
            FightMatchPlayerHost.ResolveAcceptancePersistentRoot(args ?? Args, projectRoot ?? project, realRoot ?? real, platform);

        private void Prepare()
        {
            Directory.CreateDirectory(persistent);
            File.WriteAllBytes(ownerLock, Array.Empty<byte>());
            WriteActivation();
        }
        private void WriteActivation(Action<Binding> change = null)
        {
            var binding = new Binding { activationId = Id, canonicalProjectRoot = project,
                persistentRoot = persistent, candidateSha256 = Candidate,
                ownerThread = "01a0e404-d89d-7ab2-bece-3cd1df3fbc52", ownerTurn = "01234567-89ab-cdef-0123-456789abcdef" };
            change?.Invoke(binding);
            File.WriteAllText(activation, JsonUtility.ToJson(new Activation { hostSaveIsolation = binding }));
        }
        private string[] Snapshot() => Directory.GetFiles(owned, "*", SearchOption.AllDirectories)
            .OrderBy(x => x, StringComparer.Ordinal)
            .Select(x => x.Substring(owned.Length) + ":" + Convert.ToBase64String(File.ReadAllBytes(x))).ToArray();

        [Test]
        public void HISO_01_NoMarkerPreservesDefaultPathWithoutExtraIO()
        {
            Assert.IsNull(FightMatchPlayerHost.ResolveAcceptancePersistentRoot(Array.Empty<string>(), null, null, RuntimePlatform.WindowsEditor));
            Assert.IsNull(FightMatchPlayerHost.ResolveAcceptancePersistentRoot(new[] { "-batchmode" }, "not/a/path", "also:invalid", RuntimePlatform.Android));
            Assert.IsNull(FightMatchPlayerHost.ResolveAcceptancePersistentRoot(null, project, real, RuntimePlatform.OSXEditor));
            Assert.IsEmpty(Directory.GetFileSystemEntries(owned));
            Assert.IsFalse(Directory.Exists(project)); Assert.IsFalse(Directory.Exists(real));
            Assert.IsNotNull(typeof(FightMatchPlayerHost).GetProperty(nameof(FightMatchPlayerHost.SystemPersistentDataPath)));
            Assert.IsNotNull(typeof(FightMatchPlayerHost).GetProperty(nameof(FightMatchPlayerHost.CanonicalProductRoot)));
        }

        [Test]
        public void HISO_02_MalformedOrDuplicateMarkerNeverFallsBack()
        {
            var bad = new[] {
                new[] { "-fmHostSaveIsolationId" },
                new[] { "-fmHostSaveIsolationId", Id },
                new[] { "-fmHostSaveIsolationCandidate", Candidate },
                new[] { "-fmHostSaveIsolationId", Id, "-fmHostSaveIsolationCandidate" },
                new[] { "-fmHostSaveIsolationId", Id, "-fmHostSaveIsolationId", Id, "-fmHostSaveIsolationCandidate", Candidate },
                new[] { "-fmHostSaveIsolationId", Id, "-fmHostSaveIsolationCandidate", Candidate, "-fmHostSaveIsolationCandidate", Candidate },
                new[] { "-fmHostSaveIsolationId", "../escape", "-fmHostSaveIsolationCandidate", Candidate },
                new[] { "-fmHostSaveIsolationId", Id.ToUpperInvariant(), "-fmHostSaveIsolationCandidate", Candidate },
                new[] { "-fmHostSaveIsolationId", Id, "-fmHostSaveIsolationCandidate", "invalid" },
                new[] { "-fmHostSaveIsolationId", Id, "-fmHostSaveIsolationCandidate", Candidate.ToUpperInvariant() },
                new[] { "-fmHostSaveIsolationRoot", owned },
                new[] { "-FMHOSTSAVEISOLATIONID", Id, "-fmHostSaveIsolationCandidate", Candidate },
                new[] { "-fmHostSaveIsolationId", "", "-fmHostSaveIsolationCandidate", Candidate }
            };
            foreach (var args in bad) Assert.Throws<IOException>(() => Resolve(args));
            Assert.Throws<PlatformNotSupportedException>(() => Resolve(platform: RuntimePlatform.WindowsEditor));
            Assert.Throws<PlatformNotSupportedException>(() => Resolve(platform: RuntimePlatform.Android));
            Assert.IsEmpty(Directory.GetFileSystemEntries(owned));
            Assert.IsFalse(File.Exists(ownerLock)); Assert.IsFalse(Directory.Exists(real));
        }

        [Test]
        public void HISO_03_LinkedNonCanonicalOrOverlappingRootIsRejected()
        {
            Prepare(); var before = Snapshot();
            Assert.AreEqual(persistent, Resolve());
            Assert.IsFalse(Directory.Exists(real), "Nearest-existing checks must not create the simulated real root.");
            foreach (var realRoot in new[] { persistent, Path.GetDirectoryName(persistent), persistent + "/child", persistent.ToUpperInvariant() })
                Assert.Throws<IOException>(() => Resolve(realRoot: realRoot));
            foreach (var projectRoot in new[] { project + "/.", project + "/", project + "/../Project", project + "//child" })
                Assert.Throws<IOException>(() => Resolve(projectRoot: projectRoot));
            Assert.Throws<IOException>(() => Resolve(realRoot: real + "/.."));
            var alias = Path.Combine(owned, "LinkedProject");
            Assert.AreEqual(0, Link(project, alias));
            try { Assert.Throws<IOException>(() => Resolve(projectRoot: alias)); }
            finally { Assert.AreEqual(0, Unlink(alias)); }
            var parentAlias = Path.Combine(owned, "LinkedAncestor");
            Assert.AreEqual(0, Link(owned, parentAlias));
            try { Assert.Throws<IOException>(() => Resolve(projectRoot: Path.Combine(parentAlias, "Project"))); }
            finally { Assert.AreEqual(0, Unlink(parentAlias)); }
            var held = persistent + ".held";
            Directory.Move(persistent, held);
            try
            {
                foreach (var target in new[] { held, persistent + ".absent" })
                {
                    Assert.AreEqual(0, Link(target, persistent));
                    try { Assert.Throws<IOException>(() => Resolve()); }
                    finally { Assert.AreEqual(0, Unlink(persistent)); }
                }
            }
            finally { Directory.Move(held, persistent); }
            var realAlias = Path.Combine(owned, "LinkedReal");
            Assert.AreEqual(0, Link(project, realAlias));
            try { Assert.Throws<IOException>(() => Resolve(realRoot: realAlias)); }
            finally { Assert.AreEqual(0, Unlink(realAlias)); }
            var productLink = Path.Combine(persistent, "FightMatch");
            Assert.AreEqual(0, Link(project, productLink));
            try { Assert.Throws<IOException>(() => Resolve()); }
            finally { Assert.AreEqual(0, Unlink(productLink)); }
            CollectionAssert.AreEqual(before, Snapshot());
            Assert.IsFalse(Directory.Exists(real)); Assert.IsEmpty(Directory.GetFileSystemEntries(persistent));
        }

        [Test]
        public void HISO_04_ForeignActivationOrConcurrentLeaseIsRejected()
        {
            Prepare();
            var changes = new Action<Binding>[] {
                x => x.schemaVersion = 2, x => x.activationId = new string('a', 32),
                x => x.canonicalProjectRoot = owned, x => x.persistentRoot = real,
                x => x.candidateSha256 = new string('a', 64), x => x.ownerThread = "", x => x.ownerTurn = null
            };
            foreach (var change in changes)
            {
                WriteActivation(change);
                Assert.Throws<IOException>(() => Resolve());
                Assert.IsEmpty(Directory.GetFileSystemEntries(persistent));
            }
            WriteActivation();
            var before = Snapshot();
            using (FightMatchPlayerHost.OpenAcceptanceStorageLease(Resolve()))
                Assert.Throws<IOException>(() => FightMatchPlayerHost.OpenAcceptanceStorageLease(persistent));
            using (FightMatchPlayerHost.OpenAcceptanceStorageLease(persistent)) { }
            File.Move(ownerLock, ownerLock + ".held");
            try
            {
                Assert.Throws<IOException>(() => FightMatchPlayerHost.OpenAcceptanceStorageLease(persistent));
                Assert.IsFalse(File.Exists(ownerLock), "The lease must never create a missing lock.");
                Assert.AreEqual(0, Link(ownerLock + ".held", ownerLock));
                try { Assert.Throws<IOException>(() => FightMatchPlayerHost.OpenAcceptanceStorageLease(persistent)); }
                finally { Assert.AreEqual(0, Unlink(ownerLock)); }
            }
            finally { File.Move(ownerLock + ".held", ownerLock); }
            CollectionAssert.AreEqual(before, Snapshot()); Assert.IsFalse(Directory.Exists(real));
        }

        private sealed class PreferencePathProbe : ILocalePreferenceFiles
        {
            internal string Requested;
            public bool Exists(string path) { Requested = path; return false; }
            public void EnsureDirectory(string path) => throw new InvalidOperationException();
            public Stream CreateNew(string path) => throw new InvalidOperationException();
            public void Write(Stream stream, byte[] bytes) => throw new InvalidOperationException();
            public void Flush(Stream stream) => throw new InvalidOperationException();
            public Stream OpenRead(string path) => throw new InvalidOperationException();
            public byte[] Read(Stream stream) => throw new InvalidOperationException();
            public void Replace(string source, string target, string backup) => throw new InvalidOperationException();
            public void Move(string source, string target) => throw new InvalidOperationException();
            public void Delete(string path) => throw new InvalidOperationException();
        }

        [Test]
        public void HISO_05_PlayerAndPreferencePathsRemainUnderOneEffectiveRoot()
        {
            Prepare(); var before = Snapshot(); var effective = Resolve();
            var product = Path.Combine(effective, "FightMatch");
            var locator = Path.Combine(product, "locator");
            var profiles = Path.Combine(product, "profiles");
            var settings = Path.Combine(product, "settings");
            Assert.AreEqual(persistent + "/FightMatch", product);
            CollectionAssert.AreEqual(new[] { product + "/locator", product + "/profiles", product + "/settings" },
                new[] { locator, profiles, settings });
            var save = new MacEditorSaveStorage(profiles, "test-owned-player", SavePurpose.PlayerSave);
            StringAssert.StartsWith(profiles + "/", save.Profile.DirectoryPath);
            var probe = new PreferencePathProbe();
            var preferences = new FileLocalePreferenceStore(effective, probe);
            Assert.AreEqual(LocalePreferenceLoadDisposition.MissingSystemDefault, preferences.Load(SystemLanguage.English).Disposition);
            Assert.AreEqual(Path.Combine(settings, "locale-preference-v1.json"), probe.Requested);
            Assert.IsFalse(probe.Requested.StartsWith(real + "/", StringComparison.OrdinalIgnoreCase));
            CollectionAssert.AreEqual(before, Snapshot());
            Assert.IsEmpty(Directory.GetFileSystemEntries(persistent)); Assert.IsFalse(Directory.Exists(real));
        }

        [Test]
        public void HISO_06_ReopenRetainsRootAndTeardownReleasesLease()
        {
            Prepare(); var selected = Resolve();
            var marker = Path.Combine(persistent, "test-owned-marker.bin");
            var bytes = new byte[] { 1, 8, 3, 9 }; File.WriteAllBytes(marker, bytes);
            var lease = FightMatchPlayerHost.OpenAcceptanceStorageLease(selected);
            try
            {
                Assert.Throws<IOException>(() => FightMatchPlayerHost.OpenAcceptanceStorageLease(selected));
                WriteActivation(x => x.candidateSha256 = "invalid");
                Assert.Throws<IOException>(() => Resolve());
                WriteActivation();
                Assert.Throws<IOException>(() => FightMatchPlayerHost.OpenAcceptanceStorageLease(selected));
                CollectionAssert.AreEqual(bytes, File.ReadAllBytes(marker));
            }
            finally { lease.Dispose(); }
            var before = Snapshot();
            Assert.AreEqual(selected, Resolve());
            using (FightMatchPlayerHost.OpenAcceptanceStorageLease(selected))
                CollectionAssert.AreEqual(bytes, File.ReadAllBytes(marker));
            CollectionAssert.AreEqual(before, Snapshot());
            File.Move(activation, activation + ".held");
            try { Assert.Throws<IOException>(() => FightMatchPlayerHost.OpenAcceptanceStorageLease(selected)); }
            finally { File.Move(activation + ".held", activation); }
            using (FightMatchPlayerHost.OpenAcceptanceStorageLease(selected)) { }
            CollectionAssert.AreEqual(bytes, File.ReadAllBytes(marker)); Assert.IsFalse(Directory.Exists(real));
            // This local marker proves root/lease continuity only, not a PlayerSave or real Host acceptance.
        }
    }
}
#endif
