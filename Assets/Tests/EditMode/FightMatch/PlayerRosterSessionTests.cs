using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using FightMatch.Application;
using FightMatch.Content;
using FightMatch.Platform;
using NUnit.Framework;
using QFramework;
using static FightMatch.Core.Tests.PlayerSessionTestData;
using static FightMatch.Core.Tests.PlayerRosterTestData;

namespace FightMatch.Core.Tests
{
    public class PlayerRosterSessionTests
    {
        [Test] public void CA01_RealApprovedWarriorMovesThroughAllSlotsAndEmptyWithoutReplacingItsOriginalIdentity()
        {
            using (var r = RealRig())
            {
                var original = r.Head.Business.Character;
                foreach (var slot in new[] { 0, 1, 2 })
                {
                    var beforeMove = r.Head.Business.Character;
                    var slots = new string[3]; slots[slot] = original.CharacterId;
                    var request = Prepared(r.Session.PrepareFormation(Formation(r, slots), Codec()));
                    slots[slot] = null;
                    Is(r.Lifecycle.Submit(request, Budget()));
                    var view = r.Session.QueryRoster();
                    Assert.IsTrue(view.IsMaterialized); Assert.AreEqual(original.CharacterId, view.Slots[slot]);
                    Assert.AreEqual(1, view.Characters.Count); Assert.AreEqual(original.ClassId, view.Characters[0].ClassId);
                    Assert.AreEqual(original.OriginalSlot, view.Characters[0].OriginalSlot);
                    Assert.AreEqual(beforeMove.StateRevision, view.Characters[0].StateRevision);
                    Is(r.Application.Restore(Budget()), "Ready");
                    Assert.AreEqual(original.CharacterId, r.Session.QueryRoster().Slots[slot]);
                    Is(r.Lifecycle.Submit(EntryRequest(r), Budget()));
                    Assert.AreEqual(slot, r.Head.Business.ActiveHistory.CurrentRun.Baseline.Entry.Members.Single().OriginalSlot);
                    Is(r.Lifecycle.Submit(r.EndRequest(false), Budget()));
                }
                Is(r.Lifecycle.Submit(Prepared(r.Session.PrepareFormation(Formation(r, null, null, null), Codec())), Budget()));
                var head = r.Head; var files = CopyFiles(r.Storage.Files);
                var empty = Is(r.Lifecycle.Submit(EntryRequest(r), Budget()), "BuilderRejected");
                Assert.AreEqual("NoReadyMember", empty.Diagnostic.Code); Assert.AreSame(head, r.Head); SameFiles(files, r.Storage.Files);
                Assert.IsTrue(r.Session.QueryRoster().Slots.All(x => x == null));
                Assert.IsFalse(r.Session.PrepareFormation(Formation(r, original.CharacterId, original.CharacterId, null), Codec()).IsAccepted);
                SameFiles(files, r.Storage.Files);
                var unknown = Prepared(r.Session.PrepareFormation(Formation(r, "unknown", null, null), Codec()));
                var rejected = Is(r.Lifecycle.Submit(unknown, Budget()), "BuilderRejected");
                Assert.AreEqual("InconsistentBinding", rejected.Diagnostic.Code);
                Assert.AreEqual("Formation.CharacterId", rejected.Diagnostic.FieldPath);
                Assert.AreSame(head, r.Head); Assert.IsNull(rejected.View.PendingOperationId);
                SameFiles(files, r.Storage.Files);
            }
        }

        [Test] public void CA09_PrepareFreezesRevisionAndSlotsAndOriginalOperationWinsBeforeCurrentHeadAdmission()
        {
            using (var r = RealRig())
            {
                var id = r.Head.Business.Character.CharacterId;
                var draft = Formation(r, null, id, null);
                var prepared = Prepared(r.Session.PrepareFormation(draft, Codec()));
                var competing = Prepared(r.Session.PrepareFormation(Formation(r, null, null, id), Codec()));
                var originalBytes = prepared.Intent.CanonicalBytes.ToArray();
                draft.ExpectedCommitId = "changed"; draft.ExpectedFormationRevision++;
                Is(r.Lifecycle.Submit(prepared, Budget()));
                var commit = r.Head.Header.CommitId; var head = r.Head; var files = CopyFiles(r.Storage.Files);
                Is(r.Lifecycle.Submit(competing, Budget()), "StaleContext");
                Assert.AreEqual("StaleContext", r.Session.PrepareFormation(draft, Codec()).Code);
                Assert.AreEqual(commit, Is(r.Lifecycle.Submit(prepared, Budget())).OriginalCommitId);
                Assert.AreSame(head, r.Head); CollectionAssert.AreEqual(originalBytes, prepared.Intent.CanonicalBytes);
                var changed = TakeCore(CandidateApplicationProtocol.PrepareIntent(new CandidateApplicationIntentInput {
                    PlayerId = r.Profile.PlayerId, OperationId = prepared.OperationId, ExpectedCommitId = prepared.Intent.ExpectedCommitId,
                    Kind = CandidateApplicationKind.SetFormation, FormatVersion = 3, Context = new PublishedRuleContext(r.Publication.Binding),
                    SetFormation = new CandidateFormationInput { ExpectedFormationRevision = head.Business.Roster.FormationRevision - 1,
                        Slots = new[] { null, null, id } } }, Codec()));
                Is(r.Application.QueryOperation(changed, Budget()), "OperationConflict"); SameFiles(files, r.Storage.Files);
                var noChange = Prepared(r.Session.PrepareFormation(Formation(r, null, id, null), Codec()));
                Is(r.Lifecycle.Submit(noChange, Budget()));
                Assert.AreEqual(head.Business.Roster.FormationRevision, r.Head.Business.Roster.FormationRevision);
                Assert.AreEqual(r.Head.Records.Last().Result.Formation.BeforeRevision, r.Head.Records.Last().Result.Formation.AfterRevision);
                var stale = EntryDraft(r.Head, r.Publication.Definitions.Levels[0]); stale.ExpectedInventoryRevision++;
                var before = r.Head;
                Is(r.Lifecycle.Submit(Prepared(r.Session.PrepareFormationEntry(stale, Time(), Codec())), Budget()), "BuilderRejected");
                Assert.AreSame(before, r.Head);
                Is(r.Lifecycle.Submit(EntryRequest(r), Budget()));
                Assert.AreEqual("ActiveAttemptConflict", r.Session.PrepareFormation(Formation(r, id, null, null), Codec()).Code);
                Assert.AreEqual("ActiveAttemptConflict", r.Session.QueryRoster().UnavailabilityReason);
                Assert.AreEqual(commit, Is(r.Lifecycle.Submit(prepared, Budget())).OriginalCommitId);
                r.Win();
                Assert.AreEqual("ActiveAttemptConflict", r.Session.PrepareRosterMigration(r.Head.Header.CommitId, Codec()).Code);
                Assert.IsFalse(r.Session.PrepareFormation(Formation(r, id, null, null), Codec()).IsAccepted);
                Is(r.Lifecycle.Submit(Prepared(r.Session.PrepareVictory(new LocalPlayerClock().Read(), Codec())), Budget()));
                Assert.AreEqual(1, r.Head.Business.Rewards.BaseRewards.Count);
            }
        }

        [TestCase("snapshot-before", "SaveFailed")]
        [TestCase("snapshot-promoted", "SaveFailed")]
        [TestCase("marker-before", "CommitUnknown")]
        [TestCase("marker-after", "CommitUnknown")]
        public void CA08_H02WriteFailuresRetainOriginalIntentCandidateAndEntropy(string fault, string code)
        {
            using (var r = RealRig())
            {
                var time = Time(); var request = EntryRequest(r, time); var bytes = request.Intent.CanonicalBytes.ToArray();
                var before = r.Head; r.Storage.Fault = fault;
                var failed = Is(r.Lifecycle.Submit(request, Budget()), code);
                Assert.AreSame(before, r.Head); Assert.IsFalse(failed.View.IsPublishedHeadVerified);
                Assert.AreEqual(request.OperationId, failed.View.PendingOperationId);
                Assert.IsFalse(r.Session.PrepareFormation(Formation(r, null, null, null), Codec()).IsAccepted);
                var commit = failed.View.PendingCommitId;
                var candidate = CandidateBytes(r.Storage, commit);
                var calls = r.Storage.SnapshotCreates; var disk = CopyFiles(r.Storage.Files);
                time.WallUtcMilliseconds = 999999; time.MonotonicElapsedMilliseconds = R(999999);
                Is(r.Lifecycle.Submit(request, Budget()), code);
                SameFiles(disk, r.Storage.Files); Assert.AreEqual(calls, r.Storage.SnapshotCreates);
                var done = Is(r.Lifecycle.Retry(request, Budget()));
                Assert.AreEqual(commit, done.OriginalCommitId);
                if (candidate != null) CollectionAssert.AreEqual(candidate, r.Storage.Files["c-" + commit + ".snapshot"]);
                CollectionAssert.AreEqual(bytes, request.Intent.CanonicalBytes);
                Assert.AreEqual(before.Records.Count + 1, r.Head.Records.Count);
                var frozen = r.Head; var saved = CopyFiles(r.Storage.Files);
                Is(r.Lifecycle.Resolve(request, Budget())); Assert.AreSame(frozen, r.Head); SameFiles(saved, r.Storage.Files);
                Assert.AreEqual("local:System.Security.Cryptography.RandomNumberGenerator", r.Head.Business.ActiveHistory.Binding.SourceCapabilityId);
            }
        }

        [TestCase("snapshot-promoted")] [TestCase("marker-before")] [TestCase("marker-after")]
        public void CA08_H02ObjectRebuildResumesOnlyOriginalDiskCandidate(string fault)
        {
            using (var r = RealRig())
            {
                var request = EntryRequest(r); var previous = r.Head; r.Storage.Fault = fault;
                var failed = r.Lifecycle.Submit(request, Budget()); Assert.IsFalse(failed.IsCommitted);
                var commit = failed.View.PendingCommitId; var bytes = CandidateBytes(r.Storage, commit); Assert.IsNotNull(bytes);
                var writes = r.Storage.SnapshotCreates; var active = r.Locator.Read(Budget()).Observation.ActiveProfile; r.Close();
                var arch = FightMatchDemoArchitecture.Interface;
                try
                {
                    var session = arch.GetSystem<PlayerSessionSystem>(); var app = arch.GetSystem<CandidateApplicationSystem>();
                    var lifecycle = arch.GetSystem<CandidateLifecycleApplicationSystem>();
                    var opened = session.OpenExisting(active, r.Storage, r.Catalog, r.Capabilities, Budget());
                    if (opened.View.ObservedCandidateCommitIds.Count != 0)
                    {
                        CollectionAssert.AreEqual(new[] { commit }, opened.View.ObservedCandidateCommitIds);
                        Assert.IsFalse(session.PrepareRosterMigration(previous.Header.CommitId, Codec()).IsAccepted);
                        Is(app.ResumeObserved(commit, Budget()));
                    }
                    else Is(opened, "Ready");
                    Assert.AreEqual(commit, Is(lifecycle.Submit(request, Budget())).OriginalCommitId);
                    Assert.AreEqual(writes, r.Storage.SnapshotCreates);
                    CollectionAssert.AreEqual(bytes, r.Storage.Files["c-" + commit + ".snapshot"]);
                    Assert.AreEqual(previous.Records.Count + 1, app.QueryView().View.PublishedSnapshot.Records.Count);
                }
                finally { arch.Deinit(); }
            }
        }

        [TestCase("prepare")] [TestCase("complete")]
        public void CA08_PrepareAndCommittedReadbackFailuresKeepOriginalOperationUntilVerified(string stage)
        {
            using (var r = RealRig())
            {
                var before = r.Head; var active = r.Locator.Read(Budget()).Observation.ActiveProfile; r.Close();
                var arch = FightMatchDemoArchitecture.Interface;
                try
                {
                    var session = arch.GetSystem<PlayerSessionSystem>(); var app = arch.GetSystem<CandidateApplicationSystem>();
                    var lifecycle = arch.GetSystem<CandidateLifecycleApplicationSystem>(); var storage = new FaultStorage(r.Storage);
                    Is(session.OpenExisting(active, storage, r.Catalog, r.Capabilities, Budget()), "Ready");
                    var request = Prepared(session.PrepareFormationEntry(EntryDraft(before, r.Publication.Definitions.Levels[0]), Time(), Codec()));
                    storage.Arm = stage;
                    var failed = lifecycle.Submit(request, Budget());
                    Assert.IsTrue(storage.Used); Assert.AreEqual(request.OperationId, failed.View.PendingOperationId);
                    if (stage == "complete") Assert.AreEqual("CommittedRestoreRequired", failed.Code);
                    else Assert.AreEqual(CandidateApplicationPhase.PendingPreparation, failed.View.Phase);
                    Assert.AreEqual(before.Header.CommitId, failed.View.PublishedSnapshot.Header.CommitId);
                    Assert.IsFalse(failed.View.IsPublishedHeadVerified);
                    Assert.IsFalse(session.PrepareFormation(new PlayerFormationDraft {
                        ExpectedCommitId = before.Header.CommitId, ExpectedFormationRevision = before.Business.Roster.FormationRevision,
                        Slots = before.Business.Roster.Formation }, Codec()).IsAccepted);
                    var original = CandidateBytes(r.Storage, failed.View.PendingCommitId);
                    storage.BlockReads = false;
                    var done = Is(lifecycle.Retry(request, Budget()));
                    Assert.AreEqual(request.OperationId, done.OriginalLookup.Record.OperationId);
                    if (original != null) CollectionAssert.AreEqual(original, r.Storage.Files["c-" + done.OriginalCommitId + ".snapshot"]);
                    Assert.AreEqual(before.Records.Count + 1, app.QueryView().View.PublishedSnapshot.Records.Count);
                }
                finally { arch.Deinit(); }
            }
        }

        [Test] public void CA18_PublicBuilderStillCannotWriteRosterPlayerSave()
        {
            using (var r = RealRig())
            {
                var request = EntryRequest(r); var files = CopyFiles(r.Storage.Files); var calls = 0;
                Is(r.Application.Submit(request.Intent, (s, i, b) => { calls++; return null; }, Budget()), "UnsupportedBinding");
                Assert.AreEqual(0, calls); SameFiles(files, r.Storage.Files);
                Assert.AreEqual(14, PlayerSessionSystem.RequiredRecoveryContracts.Count);
                CollectionAssert.AreEquivalent(new[] { "fm.player.application.v1", "fm.player.roster.v1", "fm.player.permanent.v1" }, PlayerSessionSystem.RequiredRecoveryFeatures);
            }
        }

        internal static byte[] CandidateBytes(MemorySave storage, string commit)
        {
            if (commit == null) return null;
            foreach (var name in new[] { "c-" + commit + ".snapshot", "w-" + commit + ".snapshot.tmp" })
                if (storage.Files.TryGetValue(name, out var bytes)) return (byte[])bytes.Clone();
            return null;
        }

        // Isolated memory faults only; the protocol and object reconstruction still use the production facade.
        internal sealed class FaultStorage : ILocalSaveStorage
        {
            private readonly MemorySave inner;
            internal string Arm;
            internal bool Used, BlockReads;
            private bool markerPublished;
            private int inspectionsAfterMarker;
            internal FaultStorage(MemorySave inner) { this.inner = inner; }
            public SaveStorageProfile Profile => inner.Profile;
            public IDisposable AcquireWriterLease(bool createDirectory) => inner.AcquireWriterLease(createDirectory);
            public IEnumerable<string> EnumerateNames()
            {
                if (Arm == "prepare") { Arm = null; Used = true; throw new IOException("isolated prepare inspection failure"); }
                // Let M12 confirm its marker; fail the next inspection in Application Complete.
                if (Arm == "complete" && markerPublished && ++inspectionsAfterMarker == 2)
                { Arm = null; Used = true; BlockReads = true; }
                return inner.EnumerateNames();
            }
            public Stream OpenRead(string name)
            {
                if (BlockReads) throw new IOException("isolated committed readback failure");
                return inner.OpenRead(name);
            }
            public Stream CreateWork(string name) => inner.CreateWork(name);
            public void FlushFile(Stream stream) => inner.FlushFile(stream);
            public void PromoteNoReplace(string workName, string finalName)
            {
                inner.PromoteNoReplace(workName, finalName);
                if (Arm == "complete" && finalName.EndsWith(".commit")) markerPublished = true;
            }
            public void DeleteUncommitted(string name) => inner.DeleteUncommitted(name);
            public void DeleteIndexedOld(string name) => inner.DeleteIndexedOld(name);
        }
    }
}
