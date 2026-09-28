using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using FightMatch.Core;

namespace FightMatch.Platform
{
    // Cooperating Editor processes only; no power-loss or Android capability is claimed.
    public sealed class MacEditorSaveStorage : ILocalSaveStorage
    {
        private readonly System.Runtime.CompilerServices.ConditionalWeakTable<FileStream, object> created =
            new System.Runtime.CompilerServices.ConditionalWeakTable<FileStream, object>();
        public SaveStorageProfile Profile { get; }
        public MacEditorSaveStorage(string root, string playerId, SavePurpose purpose)
        {
            if (playerId == null) throw new ArgumentNullException(nameof(playerId));
            if (playerId.Length == 0 || (purpose != SavePurpose.CandidateValidation && purpose != SavePurpose.PlayerSave))
                throw new ArgumentException("Explicit player and purpose are required.");
            var full = MacStoragePaths.Root(root);
            string key;
            using (var hash = SHA256.Create())
            {
                var units = new byte[4096]; var used = 0;
                foreach (var unit in playerId)
                {
                    units[used++] = (byte)unit; units[used++] = (byte)(unit >> 8);
                    if (used == units.Length) { hash.TransformBlock(units, 0, used, units, 0); used = 0; }
                }
                hash.TransformFinalBlock(units, 0, used); var text = new StringBuilder(64);
                foreach (var b in hash.Hash) text.Append(b.ToString("x2")); key = text.ToString();
            }
            Profile = new SaveStorageProfile(playerId, purpose, Path.Combine(full, "p-" + key,
                purpose == SavePurpose.CandidateValidation ? "candidate" : "player"), SaveFaultModel.EditorProcessCrash);
            MacStoragePaths.Check(Profile.DirectoryPath);
        }
        public IDisposable AcquireWriterLease(bool createDirectory)
        {
            MacStoragePaths.Check(Profile.DirectoryPath);
            if (createDirectory) Directory.CreateDirectory(Profile.DirectoryPath);
            else File.GetAttributes(Profile.DirectoryPath);
            MacStoragePaths.Check(Profile.DirectoryPath);
            return MacStoragePaths.Lock(FilePath("writer.lock"));
        }
        public IEnumerable<string> EnumerateNames()
        {
            MacStoragePaths.Check(Profile.DirectoryPath);
            foreach (var path in Directory.EnumerateFileSystemEntries(Profile.DirectoryPath))
            { MacStoragePaths.Check(path); yield return Path.GetFileName(path); }
        }
        public Stream OpenRead(string name) => new FileStream(FilePath(name), FileMode.Open, FileAccess.Read, FileShare.Read);
        public Stream CreateWork(string name)
        {
            var kind = SaveFileNames.Kind(name, out var commit);
            if (kind != SaveFileKind.SnapshotWork && kind != SaveFileKind.MarkerWork)
                throw new ArgumentException("Only protocol work files can be created.");
            var path = FilePath(name);
            var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
            try { MacStoragePaths.Check(path); created.Add(stream, new object()); return stream; }
            catch { stream.Dispose(); throw; }
        }
        public void FlushFile(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            if (!(stream is FileStream file) || !created.TryGetValue(file, out var owner))
                throw new ArgumentException("Only a stream created by this storage can be flushed.");
            file.Flush(true);
        }
        public void PromoteNoReplace(string workName, string finalName)
        {
            var from = SaveFileNames.Kind(workName, out var a); var to = SaveFileNames.Kind(finalName, out var b);
            if (a == null || a != b || !((from == SaveFileKind.SnapshotWork && to == SaveFileKind.Snapshot) ||
                (from == SaveFileKind.MarkerWork && to == SaveFileKind.Marker)))
                throw new ArgumentException("Promotion must preserve protocol identity and type.");
            File.Move(FilePath(workName), FilePath(finalName));
        }
        public void DeleteUncommitted(string name)
        {
            var kind = SaveFileNames.Kind(name, out var commit);
            if (kind != SaveFileKind.SnapshotWork && kind != SaveFileKind.MarkerWork && kind != SaveFileKind.Snapshot)
                throw new ArgumentException("Only uncommitted candidate files can be deleted.");
            if (kind == SaveFileKind.Snapshot && MacStoragePaths.Exists(FilePath(SaveFileNames.Marker(commit))))
                throw new IOException("A final marker prevents candidate deletion.");
            File.Delete(FilePath(name));
        }
        public void DeleteIndexedOld(string name)
        {
            var kind = SaveFileNames.Kind(name, out var commit);
            if (kind != SaveFileKind.Marker && kind != SaveFileKind.Snapshot)
                throw new ArgumentException("Only final indexed files can be deleted.", nameof(name));
            File.Delete(FilePath(name));
        }
        private string FilePath(string name)
        {
            if (SaveFileNames.Kind(name, out var commit) == SaveFileKind.Unknown)
                throw new ArgumentException("Invalid protocol file name.", nameof(name));
            var path = Path.Combine(Profile.DirectoryPath, name); MacStoragePaths.Check(path); return path;
        }
    }

    internal static class MacStoragePaths
    {
        [DllImport("libSystem.B.dylib", SetLastError = true)]
        private static extern IntPtr readlink(string path, byte[] buffer, UIntPtr size);
        [DllImport("libSystem.B.dylib", SetLastError = true)]
        private static extern int flock(int descriptor, int operation);
        internal static string Root(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) throw new PlatformNotSupportedException("Mac local storage is required.");
            if (!path.StartsWith("/", StringComparison.Ordinal) || path.Contains("\\") || path.Contains(":") || path.Contains("\0"))
                throw new ArgumentException("A canonical Mac absolute path is required.");
            var full = Path.GetFullPath(path);
            if (!string.Equals(full, path, StringComparison.Ordinal) || path.Split('/').Skip(1).Any(x => x == "" || x == "." || x == ".."))
                throw new ArgumentException("A canonical Mac absolute path is required.");
            Check(full);
            var drive = DriveInfo.GetDrives().Where(x => full == x.Name.TrimEnd('/') || full.StartsWith(x.Name.TrimEnd('/') + "/", StringComparison.Ordinal))
                .OrderByDescending(x => x.Name.Length).FirstOrDefault();
            if (drive == null || (drive.DriveType != DriveType.Fixed && drive.DriveType != DriveType.Removable))
                throw new NotSupportedException("The path is not on a verified local drive.");
            return full;
        }
        internal static void Check(string path)
        {
            for (var current = path; !string.IsNullOrEmpty(current); current = Path.GetDirectoryName(current))
            {
                var result = readlink(current, new byte[1], new UIntPtr(1)).ToInt64();
                if (result >= 0) throw new NotSupportedException("Symbolic links are outside this storage capability.");
                var error = Marshal.GetLastWin32Error();
                // EINVAL is a real non-link; only ENOENT means absent. ENOTDIR/EACCES and other I/O are failures.
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
            var stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
            try
            {
                Check(path);
                if (stream.Length != 0) throw new IOException("Writer lock must remain empty.");
                if (flock(stream.SafeFileHandle.DangerousGetHandle().ToInt32(), 2 | 4) != 0)
                {
                    var error = Marshal.GetLastWin32Error();
                    if (error == 35) throw new IOException("A cooperating writer already holds the lease.", unchecked((int)0x80070020)); // Darwin EWOULDBLOCK only.
                    throw Failure("Writer lease", error);
                }
                return new Lease(stream);
            }
            catch { stream.Dispose(); throw; }
        }
        private static IOException Failure(string operation, int error) =>
            new IOException(operation + " failed (Darwin errno " + error + ").", new Win32Exception(error));
        private sealed class Lease : IDisposable
        {
            private FileStream stream;
            internal Lease(FileStream stream) { this.stream = stream; }
            public void Dispose()
            {
                var owned = stream; if (owned == null) return; stream = null;
                try
                {
                    if (flock(owned.SafeFileHandle.DangerousGetHandle().ToInt32(), 8) != 0)
                        throw Failure("Writer lease release", Marshal.GetLastWin32Error());
                }
                finally { owned.Dispose(); }
            }
        }
    }
}
