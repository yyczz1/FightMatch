using System;
using System.IO;

namespace FightMatch.Platform
{
    // Immutable publication storage for cooperating Mac Editor processes, scoped to process crashes.
    public sealed class MacContentPublicationStorage : IContentPublicationStorage
    {
        private readonly string root;
        private bool leased;
        public MacContentPublicationStorage(string absoluteRoot) { root = MacStoragePaths.Root(absoluteRoot); }
        private string FilePath(string key, string suffix)
        { ContentPublicationStorage.CheckKey(key); var path = Path.Combine(root, key + suffix); MacStoragePaths.Check(path); return path; }
        public IDisposable AcquireWriter()
        {
            if (leased) throw new IOException("Writer lease already held.");
            MacStoragePaths.Check(root); Directory.CreateDirectory(root); MacStoragePaths.Check(root);
            var lease = MacStoragePaths.Lock(Path.Combine(root, "writer.lock"));
            leased = true; return new Lease(this, lease);
        }
        private sealed class Lease : IDisposable
        {
            private MacContentPublicationStorage owner;
            private readonly IDisposable lease;
            internal Lease(MacContentPublicationStorage owner, IDisposable lease) { this.owner = owner; this.lease = lease; }
            public void Dispose()
            {
                if (owner == null) return;
                try { lease.Dispose(); } finally { owner.leased = false; owner = null; }
            }
        }
        public byte[] Read(string key, int maxBytes)
        {
            if (maxBytes < 0) throw new ArgumentOutOfRangeException(nameof(maxBytes));
            var path = FilePath(key, ".blob");
            if (!MacStoragePaths.Exists(path))
            {
                if (MacStoragePaths.Exists(FilePath(key, ".work"))) throw new ContentStorageException("Pending", "Unpromoted immutable work");
                return null;
            }
            return ReadFile(path, maxBytes);
        }
        private static byte[] ReadFile(string path, int maxBytes)
        {
            MacStoragePaths.Check(path);
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                if (stream.Length > maxBytes) throw new ContentStorageException("RecoveryBlocked", "Record exceeds budget");
                var bytes = new byte[(int)stream.Length]; var offset = 0;
                while (offset < bytes.Length)
                { var n = stream.Read(bytes, offset, bytes.Length - offset); if (n == 0) throw new ContentStorageException("RecoveryBlocked", "Truncated record"); offset += n; }
                if (stream.ReadByte() != -1) throw new ContentStorageException("RecoveryBlocked", "Growing record"); return bytes;
            }
        }
        public void WriteImmutable(string key, byte[] bytes, int maxBytes)
        {
            if (!leased) throw new InvalidOperationException("Writer lease required");
            if (bytes == null || bytes.Length > maxBytes) throw new ArgumentException("Record budget");
            var path = FilePath(key, ".blob"); var work = FilePath(key, ".work");
            if (MacStoragePaths.Exists(path))
            { if (!ContentPublicationStorage.Equal(ReadFile(path, maxBytes), bytes)) throw new ContentStorageException("RecoveryBlocked", "Immutable conflict"); return; }
            if (MacStoragePaths.Exists(work)) throw new ContentStorageException("Pending", "Original work retained");
            using (var stream = new FileStream(work, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            { MacStoragePaths.Check(work); stream.Write(bytes, 0, bytes.Length); stream.Flush(true); }
            MacStoragePaths.Check(work); MacStoragePaths.Check(path); File.Move(work, path);
            if (!ContentPublicationStorage.Equal(ReadFile(path, maxBytes), bytes)) throw new ContentStorageException("RecoveryBlocked", "Read-back mismatch");
        }
    }
}
