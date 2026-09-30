using System;
using System.IO;

namespace FightMatch.Platform
{
    public sealed class AndroidContentPublicationStorage : IContentPublicationStorage
    {
        private readonly string root;
        private bool leased;

        public AndroidContentPublicationStorage(string absoluteRoot)
        {
            root = AndroidStoragePaths.Root(absoluteRoot);
        }

        private string FilePath(string key, string suffix)
        {
            ContentPublicationStorage.CheckKey(key);
            var path = Path.Combine(root, key + suffix);
            AndroidStoragePaths.Check(path);
            return path;
        }

        public IDisposable AcquireWriter()
        {
            if (leased) throw new IOException("Writer lease already held.");
            AndroidStoragePaths.Check(root);
            Directory.CreateDirectory(root);
            AndroidStoragePaths.Check(root);
            var native = AndroidStoragePaths.Lock(Path.Combine(root, "writer.lock"));
            leased = true;
            return new AndroidStoragePaths.OwnedLease(native, () => leased = false);
        }

        public byte[] Read(string key, int maxBytes)
        {
            if (maxBytes < 0) throw new ArgumentOutOfRangeException(nameof(maxBytes));
            var path = FilePath(key, ".blob");
            if (!AndroidStoragePaths.Exists(path))
            {
                if (AndroidStoragePaths.Exists(FilePath(key, ".work")))
                    throw new ContentStorageException("Pending", "Unpromoted immutable work");
                return null;
            }
            return ReadFile(path, maxBytes);
        }

        private static byte[] ReadFile(string path, int maxBytes)
        {
            AndroidStoragePaths.Check(path);
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                if (stream.Length > maxBytes)
                    throw new ContentStorageException("RecoveryBlocked", "Record exceeds budget");
                var bytes = new byte[(int)stream.Length];
                var offset = 0;
                while (offset < bytes.Length)
                {
                    var count = stream.Read(bytes, offset, bytes.Length - offset);
                    if (count == 0) throw new ContentStorageException("RecoveryBlocked", "Truncated record");
                    offset += count;
                }
                if (stream.ReadByte() != -1)
                    throw new ContentStorageException("RecoveryBlocked", "Growing record");
                return bytes;
            }
        }

        public void WriteImmutable(string key, byte[] bytes, int maxBytes)
        {
            if (!leased) throw new InvalidOperationException("Writer lease required.");
            if (bytes == null || bytes.Length > maxBytes) throw new ArgumentException("Record budget");
            var path = FilePath(key, ".blob");
            var work = FilePath(key, ".work");
            if (AndroidStoragePaths.Exists(path))
            {
                if (!ContentPublicationStorage.Equal(ReadFile(path, maxBytes), bytes))
                    throw new ContentStorageException("RecoveryBlocked", "Immutable conflict");
                return;
            }
            if (AndroidStoragePaths.Exists(work))
                throw new ContentStorageException("Pending", "Original work retained");
            using (var stream = new FileStream(work, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                AndroidStoragePaths.Check(work);
                stream.Write(bytes, 0, bytes.Length);
                stream.Flush(true);
            }
            AndroidStoragePaths.Check(work);
            AndroidStoragePaths.Check(path);
            // All cooperating publishers hold this native lease; no kernel no-replace guarantee is claimed.
            File.Move(work, path);
            if (!ContentPublicationStorage.Equal(ReadFile(path, maxBytes), bytes))
                throw new ContentStorageException("RecoveryBlocked", "Read-back mismatch");
        }
    }
}
