using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using FightMatch.Platform;
using NUnit.Framework;
using static FightMatch.Core.Tests.LocalSaveTestFiles;

namespace FightMatch.Core.Tests
{
    public sealed class MacStorageTests
    {
        private const string Id = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        private const string Work = "w-" + Id + ".snapshot.tmp", Final = "c-" + Id + ".snapshot";
        [SetUp] public void ActualMacMono()
        {
            Assert.IsTrue(IsMac, "These are the signed Mac Editor physical probes.");
            Assert.IsNotNull(Type.GetType("Mono.Runtime"), "CoreCLR is not the Unity Mono lease witness.");
        }
        [Test] public void MC01_RawUtf16IdentityAndPurposeRemainDistinct()
        {
            var root = NewCase();
            var a = new MacEditorSaveStorage(root, "\ud800\0 ", SavePurpose.PlayerSave);
            var b = new MacEditorSaveStorage(root, "\ufffd\0 ", SavePurpose.PlayerSave);
            var c = new MacEditorSaveStorage(root, "\ud800\0 ", SavePurpose.CandidateValidation);
            Assert.AreNotEqual(a.Profile.DirectoryPath, b.Profile.DirectoryPath);
            Assert.AreNotEqual(a.Profile.DirectoryPath, c.Profile.DirectoryPath);
            Assert.AreEqual(SaveFaultModel.EditorProcessCrash, a.Profile.SupportedFaultModel);
            Assert.Throws<ArgumentException>(() => new MacEditorSaveStorage(root + "/../escape", "p", SavePurpose.PlayerSave));
            Assert.Throws<ArgumentException>(() => new MacEditorSaveStorage("D:/unapproved", "p", SavePurpose.PlayerSave));
        }
        [Test] public void MC02_UnityLeaseBlocksOtherProcessThenReleaseAllowsReacquisition()
        {
            var root = NewCase(); var storage = new MacEditorSaveStorage(root, "probe", SavePurpose.PlayerSave);
            var path = Path.Combine(storage.Profile.DirectoryPath, "writer.lock");
            using (storage.AcquireWriterLease(true))
            using (var child = Probe.Lock(root, path, false)) { Assert.AreEqual("BUSY", child.Line()); }
            using (var child = Probe.Lock(root, path, false))
            { Assert.AreEqual("ACQUIRED", child.Line()); Assert.AreEqual("RELEASED", child.Line()); }
            using (storage.AcquireWriterLease(false)) { Assert.AreEqual(0, new FileInfo(path).Length); }
        }
        [Test] public void MC02_OtherProcessLeaseIsBusyInUnityAndUnlockDoesNotDeleteLock()
        {
            var root = NewCase(); var storage = new MacEditorSaveStorage(root, "probe", SavePurpose.PlayerSave);
            var path = Path.Combine(storage.Profile.DirectoryPath, "writer.lock");
            using (storage.AcquireWriterLease(true)) { }
            using (var child = Probe.Lock(root, path, true))
            {
                Assert.AreEqual("ACQUIRED", child.Line());
                var busy = LocalSaveStore.Open(storage, "probe", SavePurpose.PlayerSave, SaveOpenMode.Existing, SaveFaultModel.EditorProcessCrash, B());
                Assert.AreEqual("Busy", busy.Code); Assert.IsFalse(busy.IsAccepted);
                child.Release(); Assert.AreEqual("RELEASED", child.Line());
            }
            using (storage.AcquireWriterLease(false)) { Assert.AreEqual(0, new FileInfo(path).Length); }
        }
        [Test] public void MC02_NonContentionLeaseFailureIsStorageFailure()
        {
            var root = NewCase(); var storage = new MacEditorSaveStorage(root, "probe", SavePurpose.PlayerSave);
            using (storage.AcquireWriterLease(true)) { }
            File.WriteAllBytes(Path.Combine(storage.Profile.DirectoryPath, "writer.lock"), new byte[] { 1 });
            var failed = LocalSaveStore.Open(storage, "probe", SavePurpose.PlayerSave, SaveOpenMode.Existing, SaveFaultModel.EditorProcessCrash, B());
            Assert.AreEqual("StorageFailure", failed.Code); Assert.IsNotNull(failed.Diagnostic.ExceptionMessage);
        }
        [Test] public void MC03_PromotionConflictAndOwnedFlushPreserveBytes()
        {
            var root = NewCase(); var a = new MacEditorSaveStorage(root, "probe", SavePurpose.PlayerSave);
            var b = new MacEditorSaveStorage(root, "probe", SavePurpose.PlayerSave);
            using (a.AcquireWriterLease(true))
            {
                using (var stream = a.CreateWork(Work))
                {
                    stream.WriteByte(7); Assert.Throws<ArgumentException>(() => b.FlushFile(stream)); a.FlushFile(stream);
                }
                File.WriteAllBytes(Path.Combine(a.Profile.DirectoryPath, Final), new byte[] { 9 });
                Assert.Throws<IOException>(() => a.PromoteNoReplace(Work, Final));
                CollectionAssert.AreEqual(new byte[] { 7 }, File.ReadAllBytes(Path.Combine(a.Profile.DirectoryPath, Work)));
                CollectionAssert.AreEqual(new byte[] { 9 }, File.ReadAllBytes(Path.Combine(a.Profile.DirectoryPath, Final)));
                Assert.Throws<IOException>(() => a.CreateWork(Work));
                Assert.Throws<ArgumentException>(() => a.FlushFile(new MemoryStream()));
                using (var foreign = File.OpenRead(Path.Combine(a.Profile.DirectoryPath, Final)))
                    Assert.Throws<ArgumentException>(() => a.FlushFile(foreign));
            }
        }
        [Test] public void MC03_ImmutablePublicationUsesExactBytesAndRealExclusiveLease()
        {
            var root = NewCase(); var store = new MacContentPublicationStorage(root);
            var key = new string('a', 64); var bytes = new byte[] { 0, 1, 2, 3 };
            using (store.AcquireWriter())
            {
                using (var child = Probe.Lock(root, Path.Combine(root, "writer.lock"), false)) Assert.AreEqual("BUSY", child.Line());
                store.WriteImmutable(key, bytes, 4); store.WriteImmutable(key, bytes, 4);
                CollectionAssert.AreEqual(bytes, store.Read(key, 4));
                Assert.Throws<ContentStorageException>(() => store.WriteImmutable(key, new byte[] { 3, 2, 1, 0 }, 4));
            }
            Assert.AreEqual(0, new FileInfo(Path.Combine(root, "writer.lock")).Length);
            Assert.IsFalse(File.Exists(Path.Combine(root, key + ".work")));
            CollectionAssert.AreEqual(bytes, File.ReadAllBytes(Path.Combine(root, key + ".blob")));
        }
        [TestCase(false)] [TestCase(true)]
        public void MC04_AncestorAndDanglingLinksRejectBeforeDirectoryCreation(bool dangling)
        {
            var root = NewCase(); var target = Path.Combine(root, "target"); var link = Path.Combine(root, "link");
            if (!dangling) Directory.CreateDirectory(target);
            using (var child = Probe.Link(root, link, target)) Assert.AreEqual("LINKED", child.Line());
            Assert.Throws<NotSupportedException>(() => new MacEditorSaveStorage(link, "probe", SavePurpose.PlayerSave));
            Assert.Throws<NotSupportedException>(() => new MacEditorSaveStorage(link + "/nested", "probe", SavePurpose.PlayerSave));
            Assert.Throws<NotSupportedException>(() => new MacContentPublicationStorage(link));
            Assert.IsFalse(Directory.Exists(Path.Combine(target, "nested")));
        }
        [Test] public void MC04_LeafLinksAndNonMissingIoCannotBeReadAsAbsence()
        {
            var root = NewCase(); var storage = new MacEditorSaveStorage(root, "probe", SavePurpose.PlayerSave);
            using (storage.AcquireWriterLease(true))
            {
                var leaf = Path.Combine(storage.Profile.DirectoryPath, Work);
                using (var child = Probe.Link(root, leaf, Path.Combine(root, "absent"))) Assert.AreEqual("LINKED", child.Line());
                Assert.Throws<NotSupportedException>(() => storage.CreateWork(Work));
                Assert.Throws<NotSupportedException>(() => storage.OpenRead(Work));
                Assert.Throws<NotSupportedException>(() => storage.DeleteUncommitted(Work));
            }
            var file = Path.Combine(root, "not-directory"); File.WriteAllBytes(file, new byte[] { 1 });
            Assert.Throws<IOException>(() => new MacEditorSaveStorage(file + "/child", "probe", SavePurpose.PlayerSave));
            var publication = new MacContentPublicationStorage(root); var key = new string('a', 64);
            Directory.CreateDirectory(Path.Combine(root, key + ".blob"));
            Assert.Catch<Exception>(() => publication.Read(key, 4));
        }

        private sealed class Probe : IDisposable
        {
            private readonly Process process;
            private bool released;
            private const string LockScript =
                "import sys,os,fcntl,select\n" +
                "r,p,mode=sys.argv[1:]\nassert os.path.commonpath([r,p])==r and mode in ('try','hold')\n" +
                "with open(p,'a+b') as f:\n" +
                " try: fcntl.flock(f,fcntl.LOCK_EX|fcntl.LOCK_NB)\n" +
                " except BlockingIOError: print('BUSY',flush=True);sys.exit(0)\n" +
                " print('ACQUIRED',flush=True)\n" +
                " if mode=='hold':\n  ready=select.select([sys.stdin],[],[],15)[0]\n  if ready: sys.stdin.readline()\n" +
                " fcntl.flock(f,fcntl.LOCK_UN)\n print('RELEASED',flush=True)\n";
            private const string LinkScript =
                "import sys,os\nr,p,t=sys.argv[1:]\nassert os.path.commonpath([r,p,t])==r\nos.symlink(t,p)\nprint('LINKED',flush=True)\n";
            private Probe(string root, string path, string argument, bool link)
            {
                Safe(root); Safe(path);
                Assert.AreEqual(MacIoRoot, Path.GetDirectoryName(root));
                Assert.IsTrue(path.StartsWith(root + "/", StringComparison.Ordinal));
                if (link) { Safe(argument); Assert.IsTrue(argument.StartsWith(root + "/", StringComparison.Ordinal)); }
                var script = Convert.ToBase64String(Encoding.UTF8.GetBytes(link ? LinkScript : LockScript));
                var code = "import base64;exec(base64.b64decode('" + script + "'))";
                process = Process.Start(new ProcessStartInfo {
                    FileName = "/opt/homebrew/bin/python3", Arguments = "-u -c " + Quote(code) + " " + Quote(root) + " " + Quote(path) + " " + Quote(argument),
                    UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true, RedirectStandardInput = true,
                    CreateNoWindow = true, WorkingDirectory = root });
                TestContext.Out.WriteLine("Mac Mono probe child PID=" + process.Id + " mode=" + (link ? "symlink" : argument) + " path=" + path);
            }
            internal static Probe Lock(string root, string path, bool hold) => new Probe(root, path, hold ? "hold" : "try", false);
            internal static Probe Link(string root, string path, string target) => new Probe(root, path, target, true);
            private static string Quote(string value) => "\"" + value.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
            internal string Line()
            {
                var pending = process.StandardOutput.ReadLineAsync();
                Assert.IsTrue(pending.Wait(16000), "bounded child stdout handshake");
                var line = pending.Result; TestContext.Out.WriteLine("Mac probe PID=" + process.Id + " result=" + line); return line;
            }
            internal void Release()
            { if (released) return; released = true; process.StandardInput.Close(); }
            public void Dispose()
            {
                Release();
                Assert.IsTrue(process.WaitForExit(17000), "child must exit through its 15-second protocol; never killed");
                var error = process.StandardError.ReadToEnd();
                TestContext.Out.WriteLine("Mac probe PID=" + process.Id + " actualExit=" + process.ExitCode + " stderr=" + error);
                Assert.AreEqual(0, process.ExitCode, error); process.Dispose();
            }
        }
    }
}
