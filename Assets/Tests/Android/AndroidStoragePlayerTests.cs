using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using FightMatch.Core;
using FightMatch.Host;
using FightMatch.Platform;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace FightMatch.Android.Tests
{
    [UnityPlatform(RuntimePlatform.Android)]
    public sealed class AndroidStoragePlayerTests
    {
        [DllImport("libc", SetLastError = true)] private static extern int symlink(string target, string path);
        [UnityTest]
        public IEnumerator A02_FixedPhaseExecutesAgainstTheRealAndroidFilesystem()
        {
            AndroidQaRun.Initialize();
            var phase = Array.IndexOf(AndroidQaRun.Phases, AndroidQaRun.Current.phase);
            if (phase == 0) OrdinaryStorage();
            else if (phase <= 3) yield return CrossProcessLease(phase);
            else yield return ProfileBoundaryRecovery(phase);
        }

        private static AndroidLocalSaveStorage Store(string leaf, string id = "qa-029-player", SavePurpose purpose = SavePurpose.PlayerSave)
        {
            return new AndroidLocalSaveStorage(Path.Combine(AndroidQaRun.StorageRoot, leaf), id, purpose);
        }
        private static void OrdinaryStorage()
        {
            var store = Store("ordinary");
            Assert.AreEqual(SaveFaultModel.EditorProcessCrash, store.Profile.SupportedFaultModel);
            Assert.AreEqual(SavePurpose.PlayerSave, store.Profile.Purpose);
            Assert.Throws<DirectoryNotFoundException>(() => store.AcquireWriterLease(false));
            var commit = Guid.NewGuid().ToString("N");
            var work = "w-" + commit + ".snapshot.tmp";
            var final = "c-" + commit + ".snapshot";
            var markerWork = "w-" + commit + ".commit.tmp";
            var marker = "c-" + commit + ".commit";
            Assert.Throws<InvalidOperationException>(() => store.CreateWork(work));
            using (store.AcquireWriterLease(true))
            {
                var other = Store("ordinary");
                var busy = Assert.Throws<IOException>(() => other.AcquireWriterLease(false));
                Assert.AreEqual(unchecked((int)0x80070020), busy.HResult);
                using (var stream = store.CreateWork(work))
                {
                    stream.WriteByte(41);
                    store.FlushFile(stream);
                    Assert.Throws<IOException>(() => store.CreateWork(work));
                    using (var foreign = new MemoryStream()) Assert.Throws<ArgumentException>(() => store.FlushFile(foreign));
                }
                Assert.Throws<ArgumentException>(() => store.PromoteNoReplace(work, marker));
                store.PromoteNoReplace(work, final);
                using (var stream = store.CreateWork(work)) { stream.WriteByte(99); store.FlushFile(stream); }
                Assert.Throws<IOException>(() => store.PromoteNoReplace(work, final));
                using (var stream = store.OpenRead(final)) Assert.AreEqual(41, stream.ReadByte());
                using (var stream = store.CreateWork(markerWork)) { stream.WriteByte(1); store.FlushFile(stream); }
                store.PromoteNoReplace(markerWork, marker);
                Assert.Throws<IOException>(() => store.DeleteUncommitted(final));
                Assert.Throws<ArgumentException>(() => store.OpenRead("../escape"));
                store.DeleteUncommitted(work);
            }
            using (Store("ordinary").AcquireWriterLease(false)) { }
            var candidate = Store("ordinary", purpose: SavePurpose.CandidateValidation);
            Assert.AreNotEqual(store.Profile.DirectoryPath, candidate.Profile.DirectoryPath);
            using (candidate.AcquireWriterLease(true)) Assert.IsFalse(candidate.EnumerateNames().Contains(final));
            Assert.AreNotEqual(Store("identity", "p\ud800").Profile.DirectoryPath, Store("identity", "p\ufffd").Profile.DirectoryPath);
            Assert.AreNotEqual(Store("identity", "A").Profile.DirectoryPath, Store("identity", "a").Profile.DirectoryPath);
            Assert.Throws<ArgumentException>(() => new AndroidLocalSaveStorage(AndroidQaRun.StorageRoot + "/../escape", "x", SavePurpose.PlayerSave));
            var bad = Store("bad-lock");
            Directory.CreateDirectory(bad.Profile.DirectoryPath);
            File.WriteAllText(Path.Combine(bad.Profile.DirectoryPath, "writer.lock"), "unexpected bytes");
            var invalid = Assert.Throws<IOException>(() => bad.AcquireWriterLease(false));
            Assert.AreNotEqual(unchecked((int)0x80070020), invalid.HResult);
            CheckLinks();
        }
        private static void CheckLinks()
        {
            var target = Path.Combine(AndroidQaRun.StorageRoot, "link-target");
            Directory.CreateDirectory(target);
            var link = Path.Combine(AndroidQaRun.StorageRoot, "link");
            var outcome = symlink(target, link);
            var error = Marshal.GetLastWin32Error();
            if (outcome == 0)
            {
                Assert.Throws<NotSupportedException>(() => new AndroidContentPublicationStorage(link));
                var dangling = Path.Combine(AndroidQaRun.StorageRoot, "dangling");
                Assert.AreEqual(0, symlink(Path.Combine(AndroidQaRun.StorageRoot, "never-created"), dangling));
                Assert.Throws<NotSupportedException>(() => new AndroidLocalSaveStorage(dangling, "x", SavePurpose.PlayerSave));
            }
            else Assert.That(error, Is.EqualTo(1).Or.EqualTo(13).Or.EqualTo(95), "Unexpected symlink probe failure");
            AndroidQaRun.Write("links.json", new LinkResult { supported = outcome == 0, errno = outcome == 0 ? 0 : error,
                coverage = outcome == 0 ? "Existing and dangling links rejected" : "Filesystem prohibits link creation; Android link fixture not executed" });
        }
        private static IEnumerator CrossProcessLease(int phase)
        {
            var store = Store("cross-process");
            IDisposable lease = store.AcquireWriterLease(true);
            if (phase != 1) { lease.Dispose(); lease = null; }
            try
            {
                AndroidQaRun.Write("lock-ready.json", new AndroidQaRun.Witness {
                    phase = AndroidQaRun.Current.phase, runId = AndroidQaRun.Current.runId, pid = AndroidQaRun.Pid,
                    uid = AndroidQaRun.Uid, lockPath = Path.Combine(store.Profile.DirectoryPath, "writer.lock"), point = phase == 1 ? "player-held" : "ready-for-shell" });
                yield return AndroidQaRun.WaitFor("shell.json");
                var shell = AndroidQaRun.Read<AndroidQaRun.Witness>("shell.json");
                Assert.AreEqual(AndroidQaRun.Current.runId, shell.runId);
                Assert.AreEqual(AndroidQaRun.Current.phase, shell.phase);
                Assert.AreEqual(AndroidQaRun.Uid, shell.uid);
                Assert.Greater(shell.pid, 0);
                Assert.AreNotEqual(AndroidQaRun.Pid, shell.pid);
                if (phase == 1)
                {
                    Assert.AreEqual(1, shell.exitCode, "The other PID's nonblocking flock must report conflict.");
                    lease.Dispose();
                    lease = null;
                }
                else
                {
                    Assert.AreEqual(0, shell.exitCode, "The other PID acquired the same lock.");
                    Assert.IsFalse(AndroidQaRun.ProcessAbsent(shell.pid));
                    var busy = Assert.Throws<IOException>(() => store.AcquireWriterLease(false));
                    Assert.AreEqual(unchecked((int)0x80070020), busy.HResult);
                    AndroidQaRun.Write("busy-observed.json", new AndroidQaRun.Witness {
                        phase = AndroidQaRun.Current.phase, runId = AndroidQaRun.Current.runId, pid = AndroidQaRun.Pid, otherPid = shell.pid });
                    yield return AndroidQaRun.WaitFor("released.json");
                    var released = AndroidQaRun.Read<AndroidQaRun.Witness>("released.json");
                    Assert.AreEqual(shell.pid, released.pid);
                    Assert.AreEqual(AndroidQaRun.Current.runId, released.runId);
                    Assert.AreEqual(phase == 3 ? "terminated" : "normal-release", released.point);
                    if (phase == 3) Assert.IsTrue(AndroidQaRun.ProcessAbsent(shell.pid), "The witnessed holder PID must be gone.");
                }
                using (store.AcquireWriterLease(false))
                    AndroidQaRun.Write("reacquired.json", new AndroidQaRun.Witness {
                        phase = AndroidQaRun.Current.phase, runId = AndroidQaRun.Current.runId, pid = AndroidQaRun.Pid,
                        uid = AndroidQaRun.Uid, lockPath = Path.Combine(store.Profile.DirectoryPath, "writer.lock"), point = "reacquired" });
            }
            finally { lease?.Dispose(); }
        }
        private static IEnumerator ProfileBoundaryRecovery(int phase)
        {
            yield return AndroidContentPlayerTests.Load();
            var root = Path.Combine(AndroidQaRun.StorageRoot, "product");
            var content = new ProfileBoundary(new AndroidContentPublicationStorage(Path.Combine(root, "locator")));
            SaveBoundary storage = null;
            FightMatchHostSession host = null;
            var triggered = false;
            Action<string, string> boundary = (point, commit) =>
            {
                if (AndroidQaRun.Current.step != "prepare" || triggered) return;
                triggered = true;
                AndroidQaRun.Boundary(point, host, commit);
                throw new IOException("FM029 approved boundary interruption: " + point);
            };
            content.BeforeActive = () => { if (phase == 7) boundary("initialized-before-active", storage?.LastCommit); };
            content.AfterCreate = () => { if (phase == 6) boundary("locator-created-before-initialize", null); };
            try
            {
                host = new FightMatchHostSession(AndroidContentPlayerTests.Catalog, root, content, id =>
                {
                    storage = storage ?? new SaveBoundary(new AndroidLocalSaveStorage(Path.Combine(root, "profiles"), id, SavePurpose.PlayerSave));
                    storage.AfterPromotion = (final, commit) =>
                    {
                        if (phase == 4 && final.EndsWith(".snapshot", StringComparison.Ordinal)) boundary("snapshot-promoted-before-marker", commit);
                        if (phase == 5 && final.EndsWith(".commit", StringComparison.Ordinal)) boundary("marker-published-response-lost", commit);
                    };
                    return storage;
                });
                host.ObserveStartup();
                if (AndroidQaRun.Current.step == "prepare")
                {
                    Assert.IsTrue(host.CanCreate, host.Status);
                    host.CreateProfile();
                    Assert.IsTrue(triggered, "The real I/O boundary was not reached.");
                    Assert.AreEqual(LocalPlayerProfileState.CreateIntentRecorded, host.Observation.State);
                    if (phase == 6) Assert.IsNull(host.Application.QueryView().View.PublishedSnapshot);
                    yield return AndroidQaRun.AwaitExternalTermination();
                }
                else
                {
                    var original = AndroidQaRun.Read<AndroidQaRun.Witness>("boundary.json");
                    Assert.AreEqual(AndroidQaRun.Current.phase, original.phase);
                    Assert.AreEqual(AndroidQaRun.Current.runId, original.runId);
                    Assert.AreNotEqual(AndroidQaRun.Pid, original.pid);
                    Assert.IsTrue(AndroidQaRun.ProcessAbsent(original.pid));
                    Assert.IsFalse(host.CanCreate);
                    host.ContinueCreation();
                    Assert.AreEqual(LocalPlayerProfileState.Active, host.Observation.State, host.Status);
                    Assert.AreEqual(original.playerId, host.Observation.ActiveProfile.PlayerId);
                    Assert.AreEqual(original.createHash, host.Observation.CreateRecord.RecordSha256);
                    Assert.AreEqual(original.operationId, host.OriginalProfile.OperationId);
                    if (original.commitId != null) Assert.AreEqual(original.commitId, host.Observation.ActiveProfile.OriginalInitializationCommitId);
                    var view = host.Application.QueryView().View;
                    Assert.IsTrue(view.IsPublishedHeadVerified);
                    Assert.AreEqual(SavePurpose.PlayerSave, view.PublishedSnapshot.Descriptor.Purpose);
                    Assert.AreEqual(1, view.PublishedSnapshot.Records.Count);
                    AndroidQaRun.Write("recovered.json", new AndroidQaRun.Witness { phase = original.phase, runId = original.runId,
                        point = "verified-original-recovered", pid = AndroidQaRun.Pid, otherPid = original.pid, uid = AndroidQaRun.Uid,
                        playerId = original.playerId, operationId = original.operationId, createHash = original.createHash,
                        commitId = view.PublishedSnapshot.Header.CommitId });
                }
            }
            finally { host?.Dispose(); }
        }
        private sealed class ProfileBoundary : IContentPublicationStorage
        {
            private readonly IContentPublicationStorage inner;
            internal Action BeforeActive, AfterCreate;
            internal ProfileBoundary(IContentPublicationStorage inner) { this.inner = inner; }
            public IDisposable AcquireWriter() => inner.AcquireWriter();
            public byte[] Read(string key, int limit) => inner.Read(key, limit);
            public void WriteImmutable(string key, byte[] bytes, int limit)
            {
                if (key == LocalPlayerProfileLocator.ActiveRecordKey) BeforeActive?.Invoke();
                inner.WriteImmutable(key, bytes, limit);
                if (key == LocalPlayerProfileLocator.CreateRecordKey) AfterCreate?.Invoke();
            }
        }
        private sealed class SaveBoundary : ILocalSaveStorage
        {
            private readonly ILocalSaveStorage inner;
            internal Action<string, string> AfterPromotion;
            internal string LastCommit;
            internal SaveBoundary(ILocalSaveStorage inner) { this.inner = inner; }
            public SaveStorageProfile Profile => inner.Profile;
            public IDisposable AcquireWriterLease(bool create) => inner.AcquireWriterLease(create);
            public IEnumerable<string> EnumerateNames() => inner.EnumerateNames();
            public Stream OpenRead(string name) => inner.OpenRead(name);
            public Stream CreateWork(string name) => inner.CreateWork(name);
            public void FlushFile(Stream stream) => inner.FlushFile(stream);
            public void PromoteNoReplace(string work, string final)
            {
                inner.PromoteNoReplace(work, final);
                LastCommit = final.Substring(2, 32);
                AfterPromotion?.Invoke(final, LastCommit);
            }
            public void DeleteUncommitted(string name) => inner.DeleteUncommitted(name);
            public void DeleteIndexedOld(string name) => inner.DeleteIndexedOld(name);
        }
        [Serializable] private sealed class LinkResult { public bool supported; public int errno; public string coverage; }
    }
}
