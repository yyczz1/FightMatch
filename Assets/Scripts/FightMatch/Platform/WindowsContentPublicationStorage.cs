using System;
using System.IO;

namespace FightMatch.Platform
{
    // Windows Editor process-crash scope. No power-loss durability or Android capability is claimed.
    public sealed class WindowsContentPublicationStorage : IContentPublicationStorage
    {
        private readonly string root;
        private bool leased;
        public WindowsContentPublicationStorage(string absoluteRoot)
        {
            if (string.IsNullOrWhiteSpace(absoluteRoot) || absoluteRoot.Length < 3 || absoluteRoot[1] != ':' ||
                !char.IsLetter(absoluteRoot[0]) || (absoluteRoot[2] != '\\' && absoluteRoot[2] != '/')) throw new ArgumentException("Absolute local drive required");
            root = Path.GetFullPath(absoluteRoot);
            if (!string.Equals(root.TrimEnd('\\', '/'), absoluteRoot.Replace('/', '\\').TrimEnd('\\'), StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Canonical root required");
            var drive = new DriveInfo(Path.GetPathRoot(root));
            if (drive.DriveType != DriveType.Fixed && drive.DriveType != DriveType.Removable) throw new ArgumentException("Local drive required");
            CheckPath(root);
        }
        private static void CheckPath(string path)
        {
            var current = Path.GetFullPath(path);
            while (current != null)
            {
                try { if ((File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0) throw new ContentStorageException("RecoveryBlocked", "Reparse path"); }
                catch (FileNotFoundException) { } catch (DirectoryNotFoundException) { }
                current = Path.GetDirectoryName(current);
            }
        }
        private string FilePath(string key, string suffix)
        { ContentPublicationStorage.CheckKey(key); var path = Path.Combine(root, key + suffix); CheckPath(path); return path; }
        public IDisposable AcquireWriter()
        {
            CheckPath(root); Directory.CreateDirectory(root); CheckPath(root);
            var path = Path.Combine(root, "writer.lock"); CheckPath(path);
            var stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            leased = true; return new Lease(this, stream);
        }
        private sealed class Lease : IDisposable
        {
            private WindowsContentPublicationStorage owner; private readonly FileStream stream;
            internal Lease(WindowsContentPublicationStorage owner, FileStream stream) { this.owner = owner; this.stream = stream; }
            public void Dispose() { if (owner == null) return; stream.Dispose(); owner.leased = false; owner = null; }
        }
        public byte[] Read(string key, int maxBytes)
        {
            var path = FilePath(key, ".blob");
            try { return ReadFile(path, maxBytes); }
            catch (FileNotFoundException)
            { if (Exists(FilePath(key, ".work"))) throw new ContentStorageException("Pending", "Unpromoted immutable work"); return null; }
            catch (DirectoryNotFoundException) { CheckPath(root); return null; }
        }
        private static bool Exists(string path)
        { try { File.GetAttributes(path); return true; } catch (FileNotFoundException) { return false; } catch (DirectoryNotFoundException) { return false; } }
        private static byte[] ReadFile(string path, int maxBytes)
        {
            if (maxBytes < 0) throw new ArgumentOutOfRangeException(nameof(maxBytes));
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                if (stream.Length > maxBytes) throw new ContentStorageException("RecoveryBlocked", "Record exceeds budget");
                var bytes = new byte[(int)stream.Length]; var offset = 0;
                while (offset < bytes.Length) { var n = stream.Read(bytes, offset, bytes.Length - offset); if (n == 0) throw new ContentStorageException("RecoveryBlocked", "Truncated record"); offset += n; }
                if (stream.ReadByte() != -1) throw new ContentStorageException("RecoveryBlocked", "Growing record"); return bytes;
            }
        }
        public void WriteImmutable(string key, byte[] bytes, int maxBytes)
        {
            if (!leased) throw new InvalidOperationException("Writer lease required");
            if (bytes == null || bytes.Length > maxBytes) throw new ArgumentException("Record budget");
            var path = FilePath(key, ".blob"); var work = FilePath(key, ".work");
            if (Exists(path))
            { if (!ContentPublicationStorage.Equal(ReadFile(path, maxBytes), bytes)) throw new ContentStorageException("RecoveryBlocked", "Immutable conflict"); return; }
            // Do not truncate, replace, or guess whether an earlier unpromoted write succeeded.
            if (Exists(work)) throw new ContentStorageException("Pending", "Original work retained");
            using (var stream = new FileStream(work, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            { stream.Write(bytes, 0, bytes.Length); stream.Flush(true); }
            CheckPath(work); CheckPath(path); File.Move(work, path);
            if (!ContentPublicationStorage.Equal(ReadFile(path, maxBytes), bytes)) throw new ContentStorageException("RecoveryBlocked", "Read-back mismatch");
        }
    }
}
