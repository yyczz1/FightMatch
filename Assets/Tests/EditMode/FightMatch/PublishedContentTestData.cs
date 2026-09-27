using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using FightMatch.Content;
using FightMatch.Platform;
using NUnit.Framework;

namespace FightMatch.Core.Tests
{
    internal static class PublishedContentTestData
    {
        internal const string SourcePath = "Assets/FightMatchContent/demo-r1.source.json";
        internal static ExactMathBudget Math() => new ExactMathBudget(maxPrimitiveSteps: 16000000);
        internal static ContentStoreBudget StoreBudget() => new ContentStoreBudget(Math());
        internal static ContentConsumerCapabilities Caps() => ContentConsumerCapabilities.Current;
        internal static T Take<T>(PublicationResult<T> r) { Assert.IsTrue(r.IsAccepted, r.RejectionCode + " " + r.FieldPath); return r.Value; }
        internal static PublishedSource Source() => Take(PublishedContentCodec.DecodeSource(File.ReadAllBytes(SourcePath), Caps(), Math()));
        internal static byte[] Encode(PublishedSource source) => Take(PublishedContentCodec.EncodeSource(source, Caps(), Math()));
        internal static PublishedSource Copy(PublishedSource source) => Take(PublishedContentCodec.DecodeSource(Encode(source), Caps(), Math()));
        internal static PublishedSource Fixture(int version = 1, bool multi = false)
        {
            var s = Source(); s.PackageId = "fixture:package:v" + version; s.DraftId = "fixture:draft:v" + version;
            s.NewProfile.Id = "fixture:new-profile"; s.SourceNotes = new List<string> { "Isolated publication protocol fixture; never real content approval." };
            s.Levels[0].Level.LevelId = "fixture:alpha"; s.Levels[0].Level.LevelVersion = version.ToString();
            s.Levels[0].Reward.LevelId = "fixture:alpha"; s.Levels[0].Reward.LevelVersion = version.ToString();
            s.Levels[0].Reward.RewardDefinitionId = "fixture:reward:alpha";
            s.Progression.Levels[0].LevelId = "fixture:alpha"; s.Progression.Levels[0].LevelVersion = version.ToString();
            s.NewProfile.OpenLevels = new List<string> { "fixture:alpha" };
            if (multi)
            {
                var extra = Copy(s).Levels[0]; extra.Level.LevelId = "fixture:beta"; extra.Reward.LevelId = "fixture:beta";
                extra.Reward.RewardDefinitionId = "fixture:reward:beta"; s.Levels.Add(extra);
                s.Progression.Levels.Add(new CandidateProgressionLevelInput { LevelId = "fixture:beta", LevelVersion = version.ToString(),
                    UnlockRuleId = "fixture:initial:beta", EntryKind = CandidateProgressionEntryKind.Ordinary,
                    UnlockKind = CandidateProgressionUnlockKind.InitiallyOpen, UnlockAfterLevelId = null, RequiredFeatures = new List<string>() });
                s.NewProfile.OpenLevels.Add("fixture:beta");
            }
            return s;
        }
        internal static PreparedPublication Prepare(PublishedSource s, DemoContentJob job = null)
        {
            var result = PublishedContentCompiler.Prepare(s, job ?? new DemoContentDraft(s.DraftId).BeginJob(), Caps(), Math());
            Assert.IsTrue(result.IsAccepted, result.RejectionCode + " " + result.FieldPath); return result.Prepared;
        }
        internal static ContentReviewEvidence Review(PreparedPublication p)
        {
            Assert.IsTrue(p.Binding.PackageId.StartsWith("fixture:", StringComparison.Ordinal), "A test review must never approve the real source");
            return new ContentReviewEvidence { SchemaVersion = 1, Verdict = "ACCEPT", AuthorTaskId = "fixture:author", AuthorTurnId = "fixture:author-turn",
                ReviewerTaskId = "fixture:independent-reviewer", ReviewerTurnId = "fixture:review-turn", SourcePacketRangeSha256 = new string('1', 64),
                ApprovalBasis = "Isolated fixture only", ApprovedMappingSha256 = new string('2', 64), DraftId = p.DraftId, Revision = p.Revision,
                Binding = ContentBindingRecord.From(p.Binding), DefinitionBindings = p.DefinitionBindings.Select(DefinitionBindingRecord.From).ToList(),
                SourceBytes = p.SourceBytes.Count, SourceSha256 = p.SourceSha256, PayloadBytes = p.PayloadBytes.Count, PayloadSha256 = p.PayloadSha256,
                ValidationBytes = p.Validation.Bytes.Count, ValidationSha256 = p.Validation.Sha256 };
        }
        internal sealed class MemoryStorage : IContentPublicationStorage
        {
            internal readonly Dictionary<string, byte[]> Blobs = new Dictionary<string, byte[]>(StringComparer.Ordinal);
            internal int Writes; internal int FailAt = -1; internal string Fault; internal Action<int> BeforeWrite;
            internal string BadReadKey; internal bool BadReadOnce; internal bool ThrowRead; private readonly object sync = new object();
            public IDisposable AcquireWriter() { Monitor.Enter(sync); return new Unlock(sync); }
            private sealed class Unlock : IDisposable { private readonly object sync; internal Unlock(object sync) { this.sync = sync; } public void Dispose() { Monitor.Exit(sync); } }
            public byte[] Read(string key, int maxBytes)
            {
                ContentPublicationStorage.CheckKey(key); if (ThrowRead) throw new IOException("injected unknown read");
                if (!Blobs.TryGetValue(key, out var bytes)) return null;
                if (bytes.Length > maxBytes) throw new ContentStorageException("RecoveryBlocked", "budget");
                var copy = (byte[])bytes.Clone();
                if (key == BadReadKey) { copy[0] ^= 1; if (BadReadOnce) BadReadKey = null; } return copy;
            }
            public void WriteImmutable(string key, byte[] bytes, int maxBytes)
            {
                ContentPublicationStorage.CheckKey(key); var index = Writes++; BeforeWrite?.Invoke(index);
                if (index == FailAt && Fault == "before") throw new IOException("injected before write");
                if (Blobs.TryGetValue(key, out var old))
                { if (!ContentPublicationStorage.Equal(old, bytes)) throw new ContentStorageException("RecoveryBlocked", "immutable conflict"); return; }
                if (bytes.Length > maxBytes) throw new ContentStorageException("RecoveryBlocked", "budget");
                if (index == FailAt && Fault == "partial") { Blobs.Add(key, bytes.Take(bytes.Length / 2).ToArray()); throw new IOException("partial write retained"); }
                Blobs.Add(key, (byte[])bytes.Clone());
                if (index == FailAt && Fault == "after") throw new IOException("write outcome initially unknown");
                if (index == FailAt && Fault == "bad-read") { BadReadKey = key; BadReadOnce = true; }
            }
        }
    }
}
