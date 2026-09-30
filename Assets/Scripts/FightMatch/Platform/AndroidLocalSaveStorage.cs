using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using FightMatch.Core;

namespace FightMatch.Platform
{
    // All cooperating writers hold the same native lease. The fault model is process crash, not power loss.
    public sealed class AndroidLocalSaveStorage : ILocalSaveStorage
    {
        private readonly ConditionalWeakTable<FileStream, object> created = new ConditionalWeakTable<FileStream, object>();
        private bool leased;
        public SaveStorageProfile Profile { get; }

        public static string CanonicalPersistentDataPath(string systemPath)
        {
            return AndroidStoragePaths.SystemRoot(systemPath);
        }

        public AndroidLocalSaveStorage(string root, string playerId, SavePurpose purpose)
        {
            if (playerId == null) throw new ArgumentNullException(nameof(playerId));
            if (playerId.Length == 0 || (purpose != SavePurpose.PlayerSave && purpose != SavePurpose.CandidateValidation))
                throw new ArgumentException("Explicit player and purpose are required.");
            var full = AndroidStoragePaths.Root(root);
            string key;
            using (var hash = SHA256.Create())
            {
                var units = new byte[4096];
                var used = 0;
                foreach (var unit in playerId)
                {
                    units[used++] = (byte)unit;
                    units[used++] = (byte)(unit >> 8);
                    if (used != units.Length) continue;
                    hash.TransformBlock(units, 0, used, units, 0);
                    used = 0;
                }
                hash.TransformFinalBlock(units, 0, used);
                key = BitConverter.ToString(hash.Hash).Replace("-", "").ToLowerInvariant();
            }
            Profile = new SaveStorageProfile(playerId, purpose, Path.Combine(full, "p-" + key,
                purpose == SavePurpose.PlayerSave ? "player" : "candidate"), SaveFaultModel.EditorProcessCrash);
            AndroidStoragePaths.Check(Profile.DirectoryPath);
        }

        public IDisposable AcquireWriterLease(bool createDirectory)
        {
            if (leased) throw new IOException("Writer lease already held.");
            AndroidStoragePaths.Check(Profile.DirectoryPath);
            if (createDirectory) Directory.CreateDirectory(Profile.DirectoryPath);
            else File.GetAttributes(Profile.DirectoryPath);
            AndroidStoragePaths.Check(Profile.DirectoryPath);
            var native = AndroidStoragePaths.Lock(FilePath("writer.lock"));
            leased = true;
            return new AndroidStoragePaths.OwnedLease(native, () => leased = false);
        }

        public IEnumerable<string> EnumerateNames()
        {
            AndroidStoragePaths.Check(Profile.DirectoryPath);
            foreach (var path in Directory.EnumerateFileSystemEntries(Profile.DirectoryPath))
            {
                AndroidStoragePaths.Check(path);
                yield return Path.GetFileName(path);
            }
        }

        public Stream OpenRead(string name)
        {
            return new FileStream(FilePath(name), FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        public Stream CreateWork(string name)
        {
            RequireLease();
            var kind = SaveFileNames.Kind(name, out var commit);
            if (kind != SaveFileKind.SnapshotWork && kind != SaveFileKind.MarkerWork)
                throw new ArgumentException("Only protocol work files can be created.");
            var path = FilePath(name);
            var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
            try
            {
                AndroidStoragePaths.Check(path);
                created.Add(stream, new object());
                return stream;
            }
            catch
            {
                stream.Dispose();
                throw;
            }
        }

        public void FlushFile(Stream stream)
        {
            RequireLease();
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            if (!(stream is FileStream file) || !created.TryGetValue(file, out var owner))
                throw new ArgumentException("Only a stream created by this storage can be flushed.");
            file.Flush(true);
        }

        public void PromoteNoReplace(string workName, string finalName)
        {
            RequireLease();
            var from = SaveFileNames.Kind(workName, out var source);
            var to = SaveFileNames.Kind(finalName, out var target);
            if (source == null || source != target ||
                !((from == SaveFileKind.SnapshotWork && to == SaveFileKind.Snapshot) ||
                  (from == SaveFileKind.MarkerWork && to == SaveFileKind.Marker)))
                throw new ArgumentException("Promotion must preserve protocol identity and type.");
            // IL2CPP File.Move is no-replace only within this cooperating-writer lease model.
            File.Move(FilePath(workName), FilePath(finalName));
        }

        public void DeleteUncommitted(string name)
        {
            RequireLease();
            var kind = SaveFileNames.Kind(name, out var commit);
            if (kind != SaveFileKind.SnapshotWork && kind != SaveFileKind.MarkerWork && kind != SaveFileKind.Snapshot)
                throw new ArgumentException("Only uncommitted candidate files can be deleted.");
            if (kind == SaveFileKind.Snapshot && AndroidStoragePaths.Exists(FilePath(SaveFileNames.Marker(commit))))
                throw new IOException("A final marker prevents candidate deletion.");
            File.Delete(FilePath(name));
        }

        public void DeleteIndexedOld(string name)
        {
            RequireLease();
            var kind = SaveFileNames.Kind(name, out var commit);
            if (kind != SaveFileKind.Snapshot && kind != SaveFileKind.Marker)
                throw new ArgumentException("Only final indexed files can be deleted.");
            File.Delete(FilePath(name));
        }

        private void RequireLease()
        {
            if (!leased) throw new InvalidOperationException("Writer lease required.");
        }

        private string FilePath(string name)
        {
            if (SaveFileNames.Kind(name, out var commit) == SaveFileKind.Unknown)
                throw new ArgumentException("Invalid protocol file name.", nameof(name));
            var path = Path.Combine(Profile.DirectoryPath, name);
            AndroidStoragePaths.Check(path);
            return path;
        }
    }

    internal static class AndroidStoragePaths
    {
        [DllImport("libc", SetLastError = true)] private static extern int open(string path, int flags, int mode);
        [DllImport("libc", SetLastError = true)] private static extern int flock(int fd, int operation);
        [DllImport("libc", SetLastError = true)] private static extern int close(int fd);
        [DllImport("libc", SetLastError = true)] private static extern long lseek(int fd, long offset, int origin);
        [DllImport("libc", SetLastError = true)] private static extern IntPtr realpath(string path, byte[] buffer);
        [DllImport("libc", SetLastError = true)] private static extern IntPtr readlink(string path, byte[] buffer, UIntPtr size);

        private static void RequireAndroid()
        {
#if !UNITY_ANDROID || UNITY_EDITOR
            throw new PlatformNotSupportedException("Android Player storage is required.");
#endif
        }

        internal static string SystemRoot(string path)
        {
            RequireAndroid();
            Absolute(path);
            var bytes = new byte[4096];
            if (realpath(path, bytes) == IntPtr.Zero) throw Failure("System root", Marshal.GetLastWin32Error());
            var end = Array.IndexOf(bytes, (byte)0);
            if (end <= 0) throw new IOException("Unbounded system root.");
            var canonical = Encoding.UTF8.GetString(bytes, 0, end);
            if ((File.GetAttributes(canonical) & FileAttributes.Directory) == 0)
                throw new IOException("System root is not a directory.");
            return canonical;
        }

        private static void Absolute(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (!path.StartsWith("/", StringComparison.Ordinal) || path.Contains("\\") || path.Contains(":") || path.Contains("\0"))
                throw new ArgumentException("An absolute Android path is required.");
            if (Path.GetFullPath(path) != path || path.Split('/').Skip(1).Any(x => x == "" || x == "." || x == ".."))
                throw new ArgumentException("A canonical absolute path is required.");
        }

        internal static string Root(string path)
        {
            RequireAndroid();
            Absolute(path);
            Check(path);
            return path;
        }

        internal static void Check(string path)
        {
            for (var current = path; !string.IsNullOrEmpty(current); current = Path.GetDirectoryName(current))
            {
                if (readlink(current, new byte[1], new UIntPtr(1)).ToInt64() >= 0)
                    throw new NotSupportedException("Product paths cannot contain symbolic links.");
                var error = Marshal.GetLastWin32Error();
                if (error != 22 && error != 2) throw Failure("Path inspection", error);
            }
        }

        internal static bool Exists(string path)
        {
            Check(path);
            try { File.GetAttributes(path); return true; }
            catch (FileNotFoundException) { return false; }
            catch (DirectoryNotFoundException) { Check(path); return false; }
        }

        internal static IDisposable Lock(string path)
        {
            Check(path);
            // ARM64 NDK r23b: RDWR=2, CREAT=64, CLOEXEC=524288, NOFOLLOW=32768; mode 0600.
            var fd = open(path, 2 | 64 | 524288 | 32768, 384);
            if (fd < 0) throw Failure("Writer open", Marshal.GetLastWin32Error());
            try
            {
                Check(path);
                var length = lseek(fd, 0, 2);
                if (length < 0) throw Failure("Writer inspection", Marshal.GetLastWin32Error());
                if (length != 0) throw new IOException("Writer lock must remain empty.");
                if (flock(fd, 2 | 4) != 0)
                {
                    var error = Marshal.GetLastWin32Error();
                    if (error == 11)
                        throw new IOException("A cooperating writer holds the lease.", unchecked((int)0x80070020));
                    throw Failure("Writer lease", error);
                }
                return new NativeLease(fd);
            }
            catch
            {
                close(fd);
                throw;
            }
        }

        private static IOException Failure(string operation, int error)
        {
            return new IOException(operation + " failed (Bionic errno " + error + ").", new Win32Exception(error));
        }

        private sealed class NativeLease : IDisposable
        {
            private int descriptor;
            internal NativeLease(int descriptor) { this.descriptor = descriptor; }
            public void Dispose()
            {
                var fd = descriptor;
                if (fd < 0) return;
                descriptor = -1;
                var error = flock(fd, 8) == 0 ? 0 : Marshal.GetLastWin32Error();
                var closed = close(fd);
                if (error != 0) throw Failure("Writer unlock", error);
                if (closed != 0) throw Failure("Writer close", Marshal.GetLastWin32Error());
            }
        }

        internal sealed class OwnedLease : IDisposable
        {
            private IDisposable native;
            private readonly Action released;
            internal OwnedLease(IDisposable native, Action released)
            {
                this.native = native;
                this.released = released;
            }
            public void Dispose()
            {
                var owned = native;
                if (owned == null) return;
                native = null;
                try { owned.Dispose(); }
                finally { released(); }
            }
        }
    }
}
