using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using FightMatch.Core;

namespace FightMatch.Platform
{
    internal sealed class SaveCommitMarker
    {
        internal readonly SnapshotDescriptor Descriptor;
        internal readonly ulong Length;
        internal readonly byte[] Sha256;
        internal SaveCommitMarker(SnapshotDescriptor descriptor, ulong length, byte[] sha256)
        { Descriptor = descriptor; Length = length; Sha256 = sha256; }
    }

    internal static class SaveCommitMarkerCodec
    {
        private static readonly byte[] Magic = Encoding.ASCII.GetBytes("FMCMT001");
        internal static byte[] Encode(SnapshotDescriptor descriptor, SaveStoreBudget budget)
        {
            CheckIdentity(descriptor, budget);
            using (var bytes = new MemoryStream())
            using (var writer = new BinaryWriter(bytes, Encoding.ASCII, true))
            {
                writer.Write(Magic); writer.Write(1U); writer.Write((byte)descriptor.Purpose); writer.Write((byte)SaveFaultModel.EditorProcessCrash);
                Text(writer, descriptor.PlayerId, budget, "Marker.PlayerId");
                var token = LocalSaveFailure.Core(ExactSaveValueCodec.EncodeInteger(descriptor.SaveGeneration, budget.Codec));
                Space(bytes, (ulong)token.Length + 4, budget);
                writer.Write((uint)token.Length); writer.Write(Encoding.ASCII.GetBytes(token));
                Text(writer, descriptor.CommitId, budget, "Marker.CommitId");
                Space(bytes, 1, budget); writer.Write((byte)(descriptor.ParentCommitId == null ? 0 : 1));
                if (descriptor.ParentCommitId != null) Text(writer, descriptor.ParentCommitId, budget, "Marker.ParentCommitId");
                Text(writer, SaveFileNames.Snapshot(descriptor.CommitId), budget, "Marker.SnapshotName");
                Space(bytes, 40, budget); writer.Write(descriptor.TotalLength); writer.Write(descriptor.Sha256.ToArray());
                using (var hash = SHA256.Create()) writer.Write(hash.ComputeHash(bytes.GetBuffer(), 0, (int)bytes.Length));
                return bytes.ToArray();
            }
        }
        internal static SaveCommitMarker Read(Stream stream, string name, SaveStoreBudget budget)
        {
            var bytes = ReadBounded(stream, budget); var contentLength = bytes.Length - 32;
            LocalSaveFailure.Need(contentLength >= 14, "Malformed", "Marker.Length");
            using (var hash = SHA256.Create())
                LocalSaveFailure.Need(hash.ComputeHash(bytes, 0, contentLength).SequenceEqual(bytes.Skip(contentLength)), "DigestMismatch", "Marker.Sha256");
            try
            {
                using (var content = new MemoryStream(bytes, 0, contentLength, false))
                using (var reader = new BinaryReader(content, Encoding.ASCII, true))
                {
                    LocalSaveFailure.Need(reader.ReadBytes(8).SequenceEqual(Magic), "Malformed", "Marker.Magic");
                    LocalSaveFailure.Need(reader.ReadUInt32() == 1, "UnsupportedMarkerVersion", "Marker.Version");
                    var purpose = (SavePurpose)reader.ReadByte();
                    LocalSaveFailure.Need(reader.ReadByte() == 1, "StorageCapabilityUnavailable", "Marker.FaultModel");
                    var player = Text(reader, budget, "Marker.PlayerId");
                    var size = reader.ReadUInt32();
                    LocalSaveFailure.Limit(size, (ulong)budget.Codec.MaxNumericTokenBytes, "Marker.Generation", "NumericTokenBytes");
                    LocalSaveFailure.Need(size <= content.Length - content.Position, "Malformed", "Marker.Generation");
                    var token = reader.ReadBytes(checked((int)size));
                    LocalSaveFailure.Need(token.All(x => x <= 127), "Malformed", "Marker.Generation");
                    var generation = LocalSaveFailure.Core(ExactSaveValueCodec.DecodeInteger(Encoding.ASCII.GetString(token), budget.Codec)).Value;
                    var commit = Text(reader, budget, "Marker.CommitId");
                    var exists = reader.ReadByte(); LocalSaveFailure.Need(exists <= 1, "Malformed", "Marker.ParentCommitId");
                    var parent = exists == 0 ? null : Text(reader, budget, "Marker.ParentCommitId");
                    var snapshot = Text(reader, budget, "Marker.SnapshotName");
                    var length = reader.ReadUInt64(); var digest = reader.ReadBytes(32);
                    LocalSaveFailure.Need(digest.Length == 32 && content.Position == content.Length, "Malformed", "Marker.End");
                    var descriptor = new SnapshotDescriptor(purpose, player, generation, commit, parent, length, Array.AsReadOnly(digest));
                    CheckIdentity(descriptor, budget);
                    LocalSaveFailure.Need(snapshot == SaveFileNames.Snapshot(commit), "InconsistentBinding", "Marker.SnapshotName");
                    LocalSaveFailure.Need(name == SaveFileNames.Marker(commit) || name == SaveFileNames.MarkerWork(commit), "InconsistentBinding", "Marker.FileName");
                    using (var hash = SHA256.Create()) return new SaveCommitMarker(descriptor, (ulong)bytes.Length, hash.ComputeHash(bytes));
                }
            }
            catch (EndOfStreamException error) { throw new LocalSaveFailure("Malformed", "Marker.End", cause: error); }
        }
        private static void CheckIdentity(SnapshotDescriptor descriptor, SaveStoreBudget budget)
        {
            LocalSaveFailure.Need(descriptor.Purpose == SavePurpose.CandidateValidation || descriptor.Purpose == SavePurpose.PlayerSave, "InvalidValue", "Marker.Purpose");
            LocalSaveFailure.Need(!string.IsNullOrEmpty(descriptor.PlayerId), "MissingField", "Marker.PlayerId");
            LocalSaveFailure.Core(ExactSaveValueCodec.EncodeInteger(descriptor.SaveGeneration, budget.Codec));
            LocalSaveFailure.Need(descriptor.SaveGeneration.Sign > 0, "InvalidValue", "Marker.Generation");
            LocalSaveFailure.Need(SaveFileNames.Commit(descriptor.CommitId), "InvalidValue", "Marker.CommitId");
            LocalSaveFailure.Need(descriptor.ParentCommitId == null || SaveFileNames.Commit(descriptor.ParentCommitId), "InvalidValue", "Marker.ParentCommitId");
            LocalSaveFailure.Need(descriptor.TotalLength > 0 && descriptor.Sha256 != null && descriptor.Sha256.Count == 32, "InvalidValue", "Marker.Snapshot");
            LocalSaveFailure.Limit(descriptor.TotalLength, budget.Codec.MaxEnvelopeBytes, "Marker.Snapshot.Length", "EnvelopeBytes");
        }
        private static byte[] ReadBounded(Stream stream, SaveStoreBudget budget)
        {
            using (var buffer = new MemoryStream())
            {
                var chunk = new byte[8192];
                while (true)
                {
                    var used = (ulong)buffer.Length; var allowed = Math.Min(budget.MaxMarkerBytes, int.MaxValue);
                    LocalSaveFailure.Limit(used, allowed, "Marker.Length", "MarkerBytes");
                    var remaining = allowed - used;
                    var count = remaining >= (ulong)chunk.Length ? chunk.Length : (int)remaining + 1;
                    var read = stream.Read(chunk, 0, count); if (read == 0) return buffer.ToArray();
                    LocalSaveFailure.Limit(used + (ulong)read, allowed, "Marker.Length", "MarkerBytes");
                    buffer.Write(chunk, 0, read);
                }
            }
        }
        private static void Space(Stream stream, ulong count, SaveStoreBudget budget)
        { LocalSaveFailure.Limit((ulong)stream.Position + count + 32, Math.Min(budget.MaxMarkerBytes, int.MaxValue), "Marker.Length", "MarkerBytes"); }
        private static void Text(BinaryWriter writer, string value, SaveStoreBudget budget, string path)
        {
            LocalSaveFailure.Limit((ulong)value.Length, (ulong)budget.Codec.MaxStringCodeUnits, path, "StringCodeUnits");
            Space(writer.BaseStream, 4UL + (ulong)value.Length * 2, budget); writer.Write((uint)value.Length);
            foreach (var c in value) writer.Write((ushort)c);
        }
        private static string Text(BinaryReader reader, SaveStoreBudget budget, string path)
        {
            var count = reader.ReadUInt32(); LocalSaveFailure.Limit(count, (ulong)budget.Codec.MaxStringCodeUnits, path, "StringCodeUnits");
            LocalSaveFailure.Need((ulong)count * 2 <= (ulong)(reader.BaseStream.Length - reader.BaseStream.Position), "Malformed", path);
            var chars = new char[checked((int)count)]; for (var i = 0; i < chars.Length; i++) chars[i] = (char)reader.ReadUInt16();
            return new string(chars);
        }
    }
}
