using System;
using System.IO;
using FightMatch.Application;
using FightMatch.Platform;
using NUnit.Framework;
using QFramework;
using static FightMatch.Core.Tests.BusinessSaveScenario;
using static FightMatch.Core.Tests.LocalSaveTestFiles;
using static FightMatch.Core.Tests.LifecycleRig;

namespace FightMatch.Core.Tests
{
    public sealed class CandidateLifecycleEntryTests
    {
        [Test]
        public void ExplicitProfileEntryAndRestoreUseRealGrowthAndEmptyCarry()
        {
            using (var r = new LifecycleRig())
            {
                Assert.IsNull(r.Head.Business.ActiveHistory); Assert.IsFalse(r.Head.CommitEligible);
                var c = r.Head.Business.Character; var computed = CandidateCharacterGrowth.ComputeBaseStats(c, Math());
                r.Enter(); var entry = r.State.Baseline.Entry;
                Same(computed.Stats, entry.ReadyParticipants[0].Stats); Same(computed.EntryHp, r.State.Members[0].Hp);
                Assert.AreEqual(c.StateRevision, r.Head.Business.Progression.ActiveAttempt.Begin.Participant.CharacterRevision);
                Assert.AreEqual(c.OriginalSlot, r.State.Members[0].OriginalSlot);
                Assert.AreEqual(EntryCarryMode.Empty, r.Head.Business.Inventory.ActiveCarry.Mode);
                Assert.AreEqual("local:System.Security.Cryptography.RandomNumberGenerator", r.Head.Business.ActiveHistory.Binding.SourceCapabilityId);
                var original = r.Head.Business.ActiveHistory.Binding; r.Restore();
                Assert.AreEqual(original.Battle.InitState, r.Head.Business.ActiveHistory.Binding.Battle.InitState);
                Assert.AreEqual(original.BaseReward.InitSequence, r.Head.Business.ActiveHistory.Binding.BaseReward.InitSequence);
                Assert.AreEqual(original.Bonus.InitState, r.Head.Business.ActiveHistory.Binding.Bonus.InitState);
                Assert.AreEqual(entry.AttemptId, r.State.Baseline.Entry.AttemptId);
            }
        }
        [Test]
        public void SeparateExplicitProfilesRemainIsolatedInOneRoot()
        {
            string root, player, directory; byte[] snapshot;
            using (var first = new LifecycleRig())
            { root = first.Root; player = first.Profile.PlayerId; directory = first.Storage.Profile.DirectoryPath; snapshot = File.ReadAllBytes(Path.Combine(directory, SnapshotName(first.Head.Header.CommitId))); }
            using (var second = new LifecycleRig(root: root))
            {
                Assert.AreNotEqual(player, second.Profile.PlayerId); Assert.AreNotEqual(directory, second.Storage.Profile.DirectoryPath);
                var files = Directory.GetFiles(directory, "*.snapshot"); Assert.AreEqual(1, files.Length); CollectionAssert.AreEqual(snapshot, File.ReadAllBytes(files[0]));
                second.Enter(); Assert.AreEqual(second.Profile.PlayerId, second.State.Baseline.Entry.PlayerId);
            }
        }
        [TestCase("NoSave")] [TestCase("Lease")]
        public void ExistingOpenFailuresNeverCreateAProfile(string condition)
        {
            using (var r = new LifecycleRig(false))
            {
                if (condition == "NoSave")
                {
                    Is(r.Application.Open(r.Storage, r.Profile.PlayerId, SaveOpenMode.Existing, r.Caps, B()), "NoSave");
                    Assert.IsNull(r.Head); Assert.AreEqual(0, Directory.GetFiles(r.Root, "*.snapshot", SearchOption.AllDirectories).Length);
                }
                else using (var lease = r.Storage.AcquireWriterLease(true))
                {
                    var result = r.Application.Open(r.Storage, r.Profile.PlayerId, SaveOpenMode.Existing, r.Caps, B());
                    Assert.IsFalse(result.IsCommitted); Assert.IsNull(r.Head); Assert.AreNotEqual("InitializationReady", result.Code);
                    Assert.AreEqual(0, Directory.GetFiles(r.Root, "*.snapshot", SearchOption.AllDirectories).Length);
                }
            }
        }
        [Test]
        public void CorruptExistingHeadCannotBeOverwrittenByProfileConvenienceCall()
        {
            using (var r = new LifecycleRig())
            {
                var path = Path.Combine(r.Storage.Profile.DirectoryPath, SnapshotName(r.Head.Header.CommitId));
                File.WriteAllBytes(path, new byte[] { 1, 2, 3 }); var before = r.Disk();
                var restore = r.Application.Restore(B()); Assert.IsFalse(restore.View.IsPublishedHeadVerified);
                var result = r.System.OpenNewProfile(r.Profile, r.Storage, r.Caps, B());
                Is(result, "Completed"); Assert.IsTrue(result.IsCommitted); Assert.IsFalse(result.View.IsPublishedHeadVerified);
                Assert.AreEqual(r.Profile.OperationId, result.OriginalLookup.Record.OperationId); r.SameDisk(before);
            }
        }
        [TestCase("locked", "Locked")] [TestCase("revision", "StaleContext")]
        [TestCase("head", "StaleContext")] [TestCase("coefficient", "UnsupportedBinding")]
        public void InvalidEntryDoesNotPublishOrWrite(string change, string code)
        {
            using (var r = new LifecycleRig())
            {
                var d = r.EntryDraft(change == "locked" ? "next-level" : null);
                if (change == "revision") d.EnterAttempt.ExpectedCharacterRevision++;
                if (change == "head") d.ExpectedCommitId = "old-head";
                if (change == "coefficient") r.Content.CritCoefficients = Array.Empty<CandidateCritCoefficient>();
                var prepared = r.Freeze(d); var before = r.Head; var disk = r.Disk();
                var result = r.System.Submit(prepared, B());
                if (change == "head") Is(result, code); else BuilderRefusal(result, code);
                Assert.AreSame(before, r.Head); r.SameDisk(disk);
            }
        }
        [Test]
        public void EntryDtoMutationDoesNotChangePreparedIntent()
        {
            using (var r = new LifecycleRig())
            {
                var d = r.EntryDraft(); var request = r.Freeze(d); d.EnterAttempt.CharacterId = "other"; d.EnterAttempt.ExpectedCharacterRevision = 999;
                Is(r.System.Submit(request, B()), "Completed"); Assert.AreEqual("W", r.State.Members[0].Member.CharacterId);
                var before = r.Head; Is(r.System.Submit(request, B()), "Completed"); Assert.AreSame(before, r.Head);
                r.Restore(); Is(r.System.QueryOperation(request, B()), "Completed");
            }
        }
        [Test]
        public void DownCharacterCannotEnterAfterExit()
        {
            using (var r = new LifecycleRig(level: 3, hp: 1))
            { r.DownAndExit(); var before = r.Head; BuilderRefusal(r.System.Submit(r.Freeze(r.EntryDraft()), B()), "NoReadyMember"); Assert.AreSame(before, r.Head); }
        }
        [Test]
        public void InitializeSaveFailureRetainsPlayerOperationAndOriginalCandidate()
        {
            using (var r = new LifecycleRig(false))
            {
                r.Storage.Base.Arm("Snapshot.Flush.after", null);
                var fail = Is(r.System.OpenNewProfile(r.Profile, r.Storage, r.Caps, B()), "SaveFailed");
                Assert.IsNull(r.Head); Assert.AreEqual(r.Profile.OperationId, fail.View.PendingOperationId);
                var commit = fail.View.PendingCommitId;
                Is(r.System.OpenNewProfile(r.Profile, r.Storage, r.Caps, B()), "SaveFailed");
                Assert.AreEqual(commit, Is(r.System.Retry(r.Profile, B()), "Completed").OriginalCommitId);
                Assert.AreEqual(r.Profile.PlayerId, r.Head.Business.PlayerId); r.Restore();
            }
        }
    }
}
