using System;
using System.IO;

namespace FightMatch.Platform
{
    // Missing is represented by null only after a successful exact read. Unknown I/O must throw.
    // All keys address immutable blobs; a writer lease covers an entire multi-record publication.
    public interface IContentPublicationStorage
    {
        IDisposable AcquireWriter();
        byte[] Read(string key, int maxBytes);
        void WriteImmutable(string key, byte[] bytes, int maxBytes);
    }
    public sealed class ContentStorageException : IOException
    {
        public string Code { get; }
        public ContentStorageException(string code, string message) : base(message) { Code = code; }
    }
    public static class ContentPublicationStorage
    {
        public static void CheckKey(string key)
        {
            if (key == null || key.Length != 64) throw new ArgumentException("Expected lowercase SHA256 key");
            foreach (var c in key) if (!(c >= '0' && c <= '9' || c >= 'a' && c <= 'f')) throw new ArgumentException("Invalid content key");
        }
        public static bool Equal(byte[] a, byte[] b)
        { if (a == null || b == null || a.Length != b.Length) return false; for (var i = 0; i < a.Length; i++) if (a[i] != b[i]) return false; return true; }
    }
}
