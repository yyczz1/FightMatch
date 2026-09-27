using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using FightMatch.Core;

namespace FightMatch.Platform
{
    public sealed class WindowsEditorSaveStorage : ILocalSaveStorage
    {
        private readonly System.Runtime.CompilerServices.ConditionalWeakTable<FileStream, object> created =
            new System.Runtime.CompilerServices.ConditionalWeakTable<FileStream, object>();
        public SaveStorageProfile Profile { get; }
        public WindowsEditorSaveStorage(string root, string playerId, SavePurpose purpose)
        {
            if (root == null) throw new ArgumentNullException(nameof(root));
            if (playerId == null) throw new ArgumentNullException(nameof(playerId));
            if (Environment.OSVersion.Platform != PlatformID.Win32NT) throw new PlatformNotSupportedException("Windows local storage is required.");
            if (playerId.Length == 0 || (purpose != SavePurpose.CandidateValidation && purpose != SavePurpose.PlayerSave))
                throw new ArgumentException("Explicit player and purpose are required.");
            if (root.Length < 3 || !char.IsLetter(root[0]) || root[1] != ':' || (root[2] != '\\' && root[2] != '/'))
                throw new ArgumentException("A local absolute drive path is required.", nameof(root));
            var full = Path.GetFullPath(root);
            var drive = new DriveInfo(Path.GetPathRoot(full));
            if (drive.DriveType != DriveType.Fixed && drive.DriveType != DriveType.Removable)
                throw new NotSupportedException("The path is not on a verified local drive.");
            CheckAncestors(full);
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
            CheckAncestors(Profile.DirectoryPath);
        }
        public IDisposable AcquireWriterLease(bool createDirectory)
        {
            CheckAncestors(Profile.DirectoryPath);
            if (createDirectory) Directory.CreateDirectory(Profile.DirectoryPath);
            else File.GetAttributes(Profile.DirectoryPath);
            return new FileStream(FilePath("writer.lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
        }
        public IEnumerable<string> EnumerateNames()
        {
            CheckAncestors(Profile.DirectoryPath);
            foreach (var path in Directory.EnumerateFileSystemEntries(Profile.DirectoryPath)) yield return Path.GetFileName(path);
        }
        public Stream OpenRead(string name)
        { return new FileStream(FilePath(name), FileMode.Open, FileAccess.Read, FileShare.Read); }
        public Stream CreateWork(string name)
        {
            string commit; var kind = SaveFileNames.Kind(name, out commit);
            if (kind != SaveFileKind.SnapshotWork && kind != SaveFileKind.MarkerWork) throw new ArgumentException("Only protocol work files can be created.");
            var stream = new FileStream(FilePath(name), FileMode.CreateNew, FileAccess.Write, FileShare.None);
            created.Add(stream, new object()); return stream;
        }
        public void FlushFile(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            var file = stream as FileStream;
            if (file == null || !created.TryGetValue(file, out var owner))
                throw new ArgumentException("Only a stream created by this storage can be flushed.");
            file.Flush(true);
        }
        public void PromoteNoReplace(string workName, string finalName)
        {
            string a, b; var from = SaveFileNames.Kind(workName, out a); var to = SaveFileNames.Kind(finalName, out b);
            if (a == null || a != b || !((from == SaveFileKind.SnapshotWork && to == SaveFileKind.Snapshot) ||
                (from == SaveFileKind.MarkerWork && to == SaveFileKind.Marker))) throw new ArgumentException("Promotion must preserve protocol identity and type.");
            File.Move(FilePath(workName), FilePath(finalName));
        }
        public void DeleteUncommitted(string name)
        {
            string commit; var kind = SaveFileNames.Kind(name, out commit);
            if (kind != SaveFileKind.SnapshotWork && kind != SaveFileKind.MarkerWork && kind != SaveFileKind.Snapshot)
                throw new ArgumentException("Only uncommitted candidate files can be deleted.");
            if (kind == SaveFileKind.Snapshot)
            {
                var marked = true;
                try { File.GetAttributes(FilePath(SaveFileNames.Marker(commit))); }
                catch (FileNotFoundException) { marked = false; }
                catch (DirectoryNotFoundException) { marked = false; }
                if (marked) throw new IOException("A final marker prevents candidate deletion.");
            }
            File.Delete(FilePath(name));
        }
        public void DeleteIndexedOld(string name)
        {
            string commit; var kind = SaveFileNames.Kind(name, out commit);
            if (kind != SaveFileKind.Marker && kind != SaveFileKind.Snapshot)
                throw new ArgumentException("Only final indexed files can be deleted.", nameof(name));
            File.Delete(FilePath(name));
        }
        private string FilePath(string name)
        {
            string commit;
            if (SaveFileNames.Kind(name, out commit) == SaveFileKind.Unknown) throw new ArgumentException("Invalid protocol file name.", nameof(name));
            var full = Path.GetFullPath(Path.Combine(Profile.DirectoryPath, name)); CheckAncestors(full); return full;
        }
        internal static void CheckAncestors(string path)
        {
            for (var current = path; !string.IsNullOrEmpty(current); current = Path.GetDirectoryName(current))
            {
                try
                {
                    if ((File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0)
                        throw new NotSupportedException("Reparse paths are outside this local storage capability.");
                }
                catch (FileNotFoundException) { }
                catch (DirectoryNotFoundException) { }
            }
        }
    }

    internal static class SaveFileNames
    {
        internal static bool Commit(string id)
        {
            if (id == null || id.Length != 32) return false;
            foreach (var c in id) if (!(c >= '0' && c <= '9') && !(c >= 'a' && c <= 'f')) return false;
            return true;
        }
        internal static string Snapshot(string id) { return "c-" + id + ".snapshot"; }
        internal static string Marker(string id) { return "c-" + id + ".commit"; }
        internal static string SnapshotWork(string id) { return "w-" + id + ".snapshot.tmp"; }
        internal static string MarkerWork(string id) { return "w-" + id + ".commit.tmp"; }
        internal static SaveFileKind Kind(string name, out string commit)
        {
            commit = null;
            if (name == "writer.lock") return SaveFileKind.WriterLock;
            if (name == null || name.Length < 34) return SaveFileKind.Unknown;
            var id = name.Substring(2, 32); if (!Commit(id)) return SaveFileKind.Unknown;
            SaveFileKind kind;
            if (name == Snapshot(id)) kind = SaveFileKind.Snapshot;
            else if (name == Marker(id)) kind = SaveFileKind.Marker;
            else if (name == SnapshotWork(id)) kind = SaveFileKind.SnapshotWork;
            else if (name == MarkerWork(id)) kind = SaveFileKind.MarkerWork;
            else return SaveFileKind.Unknown;
            commit = id; return kind;
        }
    }
}
