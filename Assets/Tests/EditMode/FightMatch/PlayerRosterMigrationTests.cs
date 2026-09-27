using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using FightMatch.Application;
using FightMatch.Content;
using FightMatch.Platform;
using NUnit.Framework;
using QFramework;
using static FightMatch.Core.Tests.PlayerSessionTestData;
using static FightMatch.Core.Tests.PlayerRosterTestData;

namespace FightMatch.Core.Tests
{
    public class PlayerRosterMigrationTests
    {
        [TestCase("idle")] [TestCase("exit")] [TestCase("restart")] [TestCase("rollback")] [TestCase("victory")]
        public void CA11_OldSchemaTwoFinishesItsOriginalRouteBeforeExplicitReversibleMigration(string route)
        {
            using (var r = RealRig(false))
            {
                var initial = r.Head.Header.CommitId; var recordBytes = TakeCore(PlayerProfileCreateRecordCodec.Write(r.Profile.CreateRecord, Codec()));
                Assert.IsFalse(r.Session.QueryRoster().IsMaterialized);
                Assert.AreEqual("RosterMigrationRequired", r.Session.QueryRoster().UnavailabilityReason);
                var idleFiles = CopyFiles(r.Storage.Files);
                r.Session.QueryRoster(); r.Lifecycle.QueryView(); SameFiles(idleFiles, r.Storage.Files);
                if (route != "idle")
                {
                    r.Enter();
                    Assert.AreEqual("ActiveAttemptConflict", r.Session.PrepareRosterMigration(r.Head.Header.CommitId, Codec()).Code);
                    var attack = r.AttackRequest(); var attacked = r.Battle.Submit(attack, Budget()); Is(attacked.Application);
                    if (attacked.Presentation != null) r.Battle.ReportPresentationCompleted(attacked.Presentation.Token);
                    if (route == "rollback") RollbackLast(r);
                    if (route == "restart") Is(r.Lifecycle.Submit(r.EndRequest(true), Budget()));
                    if (route == "victory")
                    {
                        r.Win(); Assert.IsNotNull(r.Head.Continuation);
                        Assert.IsFalse(r.Session.PrepareRosterMigration(r.Head.Header.CommitId, Codec()).IsAccepted);
                        Is(r.Lifecycle.Submit(Prepared(r.Session.PrepareVictory(new LocalPlayerClock().Read(), Codec())), Budget()));
                    }
                    else Is(r.Lifecycle.Submit(r.EndRequest(false), Budget()));
                }
                var before = r.Head; var disk = CopyFiles(r.Storage.Files);
                Assert.IsTrue(Envelope(r.Storage, before.Header.CommitId).RequiredSliceContracts.All(x => x.SchemaVersion == 2));
                var request = Prepared(r.Session.PrepareRosterMigration(before.Header.CommitId, Codec()));
                CollectionAssert.AreEqual(disk.Keys, r.Storage.Files.Keys); SameFiles(disk, r.Storage.Files);
                var done = Is(r.Lifecycle.Submit(request, Budget())); var after = r.Head;
                Assert.AreEqual(before.Header.SaveGeneration + 1, after.Header.SaveGeneration);
                Assert.AreEqual(before.Records.Count + 1, after.Records.Count);
                Assert.AreEqual(CandidateBusinessFormat.PublishedRosterV3, after.Business.Format);
                Assert.AreEqual(before.Descriptor.TotalLength, done.OriginalLookup.Migration.SourceDescriptorLength);
                CollectionAssert.AreEqual(before.Descriptor.Sha256, done.OriginalLookup.Migration.SourceDescriptorSha256);
                Assert.AreEqual(before.Header.SaveGeneration, done.OriginalLookup.Migration.SourceGeneration);
                AssertProjection(before, after, Closure(r.Publication));
                foreach (var old in before.Records)
                {
                    var lookup = TakeCore(CandidateApplicationProtocol.Lookup(after, old.Intent, Codec()));
                    Assert.AreEqual(old.CommitId, lookup.OriginalCommitId);
                    CollectionAssert.AreEqual(old.Intent.CanonicalBytes, lookup.Record.Intent.CanonicalBytes);
                }
                foreach (var file in disk.Where(x => r.Storage.Files.ContainsKey(x.Key)))
                    CollectionAssert.AreEqual(file.Value, r.Storage.Files[file.Key], "retained physical ancestor must remain exact");
                var proof = TakeCore(PlayerProfileCreateRecordCodec.VerifyInitializationCommit(r.Profile.CreateRecord, after, Codec()));
                Assert.AreEqual(initial, proof.OriginalInitializationCommitId);
                CollectionAssert.AreEqual(recordBytes, TakeCore(PlayerProfileCreateRecordCodec.Write(r.Profile.CreateRecord, Codec())));
                var files = CopyFiles(r.Storage.Files);
                Assert.AreEqual(done.OriginalCommitId, Is(r.Lifecycle.Submit(request, Budget())).OriginalCommitId);
                Assert.AreEqual("UnsupportedSchema", r.Session.PrepareRosterMigration(after.Header.CommitId, Codec()).Code);
                SameFiles(files, r.Storage.Files);
                Is(r.Application.Restore(Budget()), "Ready"); Assert.AreEqual(after.Header.CommitId, r.Head.Header.CommitId);
            }
        }

        [Test] public void CA02_CA07_CA11_LegacyRecoveryMigratesExactlyAndFollowsTheHeldInstanceAcrossSlots()
        {
            var f = new IsolatedRoster(40, legacy: true); f.Commit(f.EntryRequest()); f.Attack("A");
            Assert.IsTrue(f.Head.Business.ActiveHistory.CurrentRun.CurrentSnapshot.Members.Single().Hp.Numerator.IsZero);
            f.End(time: Time());
            var before = f.Head; var period = before.Business.Character.ActiveRecovery; Assert.IsNotNull(period);
            var input = f.Input(CandidateApplicationKind.MigrateRoster);
            input.MigrateRoster = new CandidateRosterMigrationInput { SourceGeneration = before.Header.SaveGeneration,
                SourceDescriptorLength = before.Descriptor.TotalLength, SourceDescriptorSha256 = before.Descriptor.Sha256 };
            f.Commit(f.Freeze(input)); AssertProjection(before, f.Head, f.Closure);
            f.SetFormation(null, null, "A"); var moved = f.Head;
            Assert.AreEqual(period.RecoveryId, moved.Business.Character.ActiveRecovery.RecoveryId);
            Assert.AreEqual(period.StartSample.WallUtcMilliseconds, moved.Business.Character.ActiveRecovery.StartSample.WallUtcMilliseconds);
            var failed = f.Build(f.EntryRequest(Time(180000)));
            Assert.IsFalse(failed.IsAccepted); Assert.AreEqual("NoReadyMember", failed.Diagnostic.Code); Assert.AreSame(moved, f.Head);
            f.SetFormation(null, null, null); var cancelled = f.Head;
            Assert.IsFalse(f.Build(f.EntryRequest(Time(360000))).IsAccepted); Assert.AreSame(cancelled, f.Head);
            f.SetFormation(null, "A", null); var count = f.Head.Records.Count;
            f.Commit(f.EntryRequest(Time(360000)));
            Assert.AreEqual(count + 1, f.Head.Records.Count); Assert.IsNull(f.Head.Business.Character.ActiveRecovery);
            Assert.AreEqual(1, f.Head.Records.Last().Result.RecoveryResults.Count);
            Assert.AreEqual(1, f.Head.Business.ActiveHistory.CurrentRun.Baseline.Entry.Members.Single().OriginalSlot);
            Assert.AreEqual(period.RecoveryId, f.Head.Business.Character.RecoveryPeriods.Single().RecoveryId);
        }

        private static void RollbackLast(Rig r)
        {
            var history = r.Head.Business.ActiveHistory; var last = r.Head.Records.Last();
            var range = CandidateHistoryOperations.ReadRange(history, new CandidateHistoryRangeRequest {
                PlayerId = r.Profile.PlayerId, AttemptId = history.CurrentRun.Baseline.Entry.AttemptId,
                ExpectedSceneRevision = history.CurrentRun.CurrentSnapshot.SceneRevision, HistoryAnchorId = last.Result.HistoryAnchorId }, Codec().Math);
            Assert.IsTrue(range.IsAccepted, range.FieldPath);
            var result = r.Battle.Prepare(new CandidateBattleDraft { PlayerId = r.Profile.PlayerId,
                ExpectedCommitId = r.Head.Header.CommitId, Context = new PublishedRuleContext(r.Publication.Binding),
                Kind = CandidateApplicationKind.Rollback, Rollback = new CandidateApplicationRollbackInput {
                    AttemptId = range.Range.AttemptId, ExpectedSceneRevision = range.Range.SceneRevision,
                    HistoryAnchorId = range.Range.HistoryAnchorId, TargetOperationId = range.Range.OperationId,
                    ConfirmedRemovedOperationIds = range.Range.Entries.Select(x => x.OperationId).ToArray() } }, Codec());
            Assert.IsTrue(result.IsAccepted, result.Code);
            var call = r.Battle.Submit(result.Request, Budget()); Is(call.Application);
            if (call.Presentation != null) r.Battle.ReportPresentationCompleted(call.Presentation.Token);
            Assert.AreEqual(CandidateApplicationKind.Rollback, r.Head.Records.Last().Intent.Kind);
        }

        internal static void AssertProjection(CandidateApplicationSnapshot before, CandidateApplicationSnapshot after, PublishedSaveContext closure)
        {
            var b = after.Business; var i = b.Inventory;
            var legacyInventory = new CandidateInventoryState(i.Definition, i.PlayerId, i.Actor, i.Holdings, i.Loadout,
                i.ActiveCarry, i.OrdinaryGrants, i.Ends, i.StateRevision, i.PreferenceRevision);
            var projection = TakeCore(CandidateBusinessSaveCodec.Prepare(new CandidateBusinessInput(b.PlayerId, b.Character,
                legacyInventory, b.Progression, b.Rewards, b.ActiveHistory, b.RetainedRuns, b.RetainedRollbacks), SavePurpose.PlayerSave, Codec()));
            var original = TakeCore(CandidateBusinessSaveCodec.EncodePublished(before.Business, before.Header, closure, Codec()));
            var projected = TakeCore(CandidateBusinessSaveCodec.EncodePublished(projection, before.Header, closure, Codec()));
            CollectionAssert.AreEqual(Encode(original), Encode(projected), "all five legacy owners must round-trip without a domain revision or receipt change");
        }

        [TestCase("snapshot-before")] [TestCase("snapshot-promoted")] [TestCase("marker-before")] [TestCase("marker-after")]
        public void CA12_MigrationRetryAndObjectRebuildPreserveTheExactTechnicalOperation(string fault)
        {
            using (var r = RealRig(false))
            {
                var before = r.Head; var request = Prepared(r.Session.PrepareRosterMigration(before.Header.CommitId, Codec()));
                var intent = request.Intent.CanonicalBytes.ToArray(); r.Storage.Fault = fault;
                var failed = r.Lifecycle.Submit(request, Budget()); Assert.IsFalse(failed.IsCommitted);
                Assert.AreSame(before, r.Head); Assert.AreEqual(request.OperationId, failed.View.PendingOperationId);
                Assert.IsFalse(r.Session.PrepareRosterMigration(before.Header.CommitId, Codec()).IsAccepted);
                var commit = failed.View.PendingCommitId; var bytes = PlayerRosterSessionTests.CandidateBytes(r.Storage, commit);
                if (bytes == null)
                {
                    Is(r.Lifecycle.Retry(request, Budget())); bytes = r.Storage.Files["c-" + commit + ".snapshot"];
                }
                var active = r.Locator.Read(Budget()).Observation.ActiveProfile; r.Close();
                var arch = FightMatchDemoArchitecture.Interface;
                try
                {
                    var session = arch.GetSystem<PlayerSessionSystem>(); var app = arch.GetSystem<CandidateApplicationSystem>();
                    var lifecycle = arch.GetSystem<CandidateLifecycleApplicationSystem>();
                    var opened = session.OpenExisting(active, r.Storage, r.Catalog, r.Capabilities, Budget());
                    if (opened.View.ObservedCandidateCommitIds.Count != 0)
                    {
                        Assert.IsFalse(session.PrepareRosterMigration(before.Header.CommitId, Codec()).IsAccepted);
                        Is(app.ResumeObserved(commit, Budget()));
                    }
                    else Is(opened, "Ready");
                    var done = Is(lifecycle.Submit(request, Budget()));
                    Assert.AreEqual(commit, done.OriginalCommitId); CollectionAssert.AreEqual(intent, request.Intent.CanonicalBytes);
                    CollectionAssert.AreEqual(bytes, r.Storage.Files["c-" + commit + ".snapshot"]);
                    var after = app.QueryView().View.PublishedSnapshot;
                    Assert.AreEqual(1, after.Records.Count(x => x.Intent.Kind == CandidateApplicationKind.MigrateRoster));
                    AssertProjection(before, after, Closure(r.Publication));
                    var saved = CopyFiles(r.Storage.Files);
                    Is(lifecycle.Resolve(request, Budget())); SameFiles(saved, r.Storage.Files);
                }
                finally { arch.Deinit(); }
            }
        }

        [TestCase("snapshot-promoted")] [TestCase("marker-after")]
        public void CA11_OldUnknownH02IsResumedAsSchemaTwoBeforeExitAndMigration(string fault)
        {
            using (var r = RealRig(false))
            {
                var request = r.EntryRequest(); r.Storage.Fault = fault;
                var failed = r.Lifecycle.Submit(request, Budget()); var commit = failed.View.PendingCommitId;
                var bytes = PlayerRosterSessionTests.CandidateBytes(r.Storage, commit);
                Assert.IsFalse(r.Session.PrepareRosterMigration(r.Head.Header.CommitId, Codec()).IsAccepted);
                Is(r.Lifecycle.Retry(request, Budget()));
                CollectionAssert.AreEqual(bytes, r.Storage.Files["c-" + commit + ".snapshot"]);
                Assert.IsTrue(Envelope(r.Storage, commit).RequiredSliceContracts.All(x => x.SchemaVersion == 2));
                Is(r.Lifecycle.Submit(r.EndRequest(false), Budget()));
                Is(r.Lifecycle.Submit(Prepared(r.Session.PrepareRosterMigration(r.Head.Header.CommitId, Codec())), Budget()));
                Assert.AreEqual(commit, Is(r.Lifecycle.Submit(request, Budget())).OriginalCommitId);
            }
        }

        [TestCase(1, "record")] [TestCase(2, "record")]
        [TestCase(1, "snapshot-promoted")] [TestCase(2, "snapshot-promoted")]
        [TestCase(1, "marker-before")] [TestCase(2, "marker-before")]
        [TestCase(1, "marker-after")] [TestCase(2, "marker-after")]
        [TestCase(1, "confirmation-before")] [TestCase(2, "confirmation-before")]
        [TestCase(1, "confirmation-after")] [TestCase(2, "confirmation-after")]
        [TestCase(1, "confirmation-bad-read")] [TestCase(2, "confirmation-bad-read")]
        public void CA13_BothF2FormatsRebuildFromTheirOriginalRecordBeforeAnyMigration(int format, string point)
        {
            var catalog = RealCatalog(); var publication = Resolve(catalog); var caps = RosterCaps(publication);
            using (var r = new Rig(false, catalog, publication, caps))
            {
                var profile = format == 1 ? r.Profile : Prepared(r.Session.PrepareNewRosterProfile(catalog, Release, Codec()));
                var storage = new MemorySave(profile.PlayerId);
                var record = profile.CreateRecord; var recordBytes = TakeCore(PlayerProfileCreateRecordCodec.Write(record, Codec()));
                Assert.AreEqual(format, record.RecordFormatVersion); Assert.AreEqual(format + 1, record.IntentFormatVersion);
                Assert.AreEqual("FMPROF0" + format, Encoding.ASCII.GetString(recordBytes.Take(8).ToArray()));
                Assert.AreEqual("FMINT00" + (format + 1), Encoding.ASCII.GetString(profile.Intent.CanonicalBytes.Take(8).ToArray()));
                Assert.AreEqual(298, record.FrozenNewProfileDefinitionBytes.Count);
                Assert.AreEqual("36f2d8935f626c4bdafb53c56ac288f5199e7a6dec29680c0fac02f354685646", record.DefinitionSha256);
                Assert.AreEqual("new-profile:default", record.NewProfileDefinitionId);
                if (point == "record") Assert.IsTrue(r.Locator.RecordCreateIntent(record, Budget()).IsAccepted);
                else
                {
                    if (point.StartsWith("confirmation-"))
                    {
                        r.LocatorStorage.FailAt = 1; r.LocatorStorage.Fault = point.Substring("confirmation-".Length);
                    }
                    else storage.Fault = point;
                    var failed = r.Session.CreateNew(profile, storage, r.Locator, caps, Budget());
                    Assert.AreNotEqual("Completed", failed.Code);
                    Assert.IsFalse(r.Session.PrepareRosterMigration(r.Head?.Header.CommitId, Codec()).IsAccepted);
                }
                var initialSnapshots = storage.SnapshotCreates; r.Close();
                using (var reopened = new Rig(false, catalog, publication, caps, storage, r.LocatorStorage, true))
                {
                    CollectionAssert.AreEqual(recordBytes, TakeCore(PlayerProfileCreateRecordCodec.Write(reopened.Profile.CreateRecord, Codec())));
                    CollectionAssert.AreEqual(profile.Intent.CanonicalBytes, reopened.Profile.Intent.CanonicalBytes);
                    var done = Is(reopened.Session.ContinueCreate(reopened.Profile, reopened.Locator.Read(Budget()).Observation,
                        storage, reopened.Locator, caps, Budget()));
                    Assert.AreEqual(initialSnapshots == 0 ? 1 : initialSnapshots, storage.SnapshotCreates);
                    Assert.AreEqual(1, reopened.Head.Records.Count); Assert.AreEqual(BigInteger.One, reopened.Head.Header.SaveGeneration);
                    Assert.AreEqual(format == 1 ? CandidateBusinessFormat.PublishedV2 : CandidateBusinessFormat.PublishedRosterV3, reopened.Head.Business.Format);
                    Assert.AreEqual(profile.PlayerId, reopened.Head.Business.PlayerId);
                    Assert.AreEqual(publication.NewProfile.CharacterId, reopened.Head.Business.Character.CharacterId);
                    Assert.AreEqual(BigInteger.One, reopened.Head.Business.Character.Level); Assert.AreEqual(BigInteger.Zero, reopened.Head.Business.Character.Experience);
                    Assert.AreEqual(0, reopened.Head.Business.Character.OriginalSlot);
                    Assert.AreEqual(0, CandidateCharacterGrowth.ComputeBaseStats(reopened.Head.Business.Character, Codec().Math).EntryHp.Compare(R(100), Codec().Math));
                    Assert.IsNull(reopened.Head.Business.Character.ActiveRecovery); Assert.IsNull(reopened.Head.Business.ActiveHistory);
                    Assert.IsTrue(reopened.Head.Business.Inventory.Holdings.All(x => x.T.IsZero));
                    var initial = done.OriginalCommitId;
                    if (format == 1) Is(reopened.Lifecycle.Submit(Prepared(reopened.Session.PrepareRosterMigration(reopened.Head.Header.CommitId, Codec())), Budget()));
                    var id = reopened.Head.Business.Character.CharacterId;
                    CollectionAssert.AreEqual(new[] { id, null, null }, reopened.Session.QueryRoster().Slots);
                    Is(reopened.Lifecycle.Submit(Prepared(reopened.Session.PrepareFormation(Formation(reopened, null, null, id), Codec())), Budget()));
                    var proof = TakeCore(PlayerProfileCreateRecordCodec.VerifyInitializationCommit(record, reopened.Head, Codec()));
                    Assert.AreEqual(initial, proof.OriginalInitializationCommitId);
                    Assert.AreEqual(LocalPlayerProfileState.Active, reopened.Locator.Read(Budget()).Observation.State);
                    var disk = CopyFiles(storage.Files);
                    Assert.AreEqual(initial, Is(reopened.Session.ContinueCreate(reopened.Profile, reopened.Locator.Read(Budget()).Observation,
                        storage, reopened.Locator, caps, Budget())).OriginalCommitId);
                    SameFiles(disk, storage.Files);
                }
            }
        }

        [TestCase(1)] [TestCase(2)]
        public void CA13_F2MixedMagicVersionAndIntentAreRejected(int version)
        {
            using (var r = new Rig(false))
            {
                var profile = version == 1 ? r.Profile : Prepared(r.Session.PrepareNewRosterProfile(r.Catalog, Release, Codec()));
                var original = TakeCore(PlayerProfileCreateRecordCodec.Write(profile.CreateRecord, Codec()));
                var mixedMagic = (byte[])original.Clone(); mixedMagic[7] = (byte)(version == 1 ? '2' : '1');
                Assert.IsFalse(PlayerProfileCreateRecordCodec.Read(mixedMagic, Codec()).IsAccepted);
                var mixedSchema = (byte[])original.Clone(); mixedSchema[8] = (byte)(version == 1 ? 2 : 1);
                Assert.IsFalse(PlayerProfileCreateRecordCodec.Read(mixedSchema, Codec()).IsAccepted);
                var a = profile.CreateRecord;
                var mixed = new PlayerProfileCreateRecord(a.PlayerId, a.OperationId, a.ContentBinding, a.NewProfileDefinitionId,
                    a.NewProfileDefinitionVersion, a.FrozenNewProfileDefinitionBytes.ToArray(), a.CanonicalInitializeIntentBytes.ToArray(),
                    a.IntentSha256, a.DefinitionSha256, a.RecordSha256, version == 1 ? 2 : 1);
                Assert.IsFalse(PlayerProfileCreateRecordCodec.Write(mixed, Codec()).IsAccepted);
                CollectionAssert.AreEqual(original, TakeCore(PlayerProfileCreateRecordCodec.Write(a, Codec())));
                Assert.AreEqual(0, r.Storage.SnapshotCreates);
            }
        }

        [TestCase("old-root")] [TestCase("old-contract")] [TestCase("roster-contract")]
        [TestCase("feature")] [TestCase("binding")] [TestCase("player")] [TestCase("package")]
        public void CA14_V3RecoveryRequiresEveryRetainedRootAndExactCapabilityWithoutWriting(string defect)
        {
            using (var r = RealRig())
            {
                var original = r.Head.Records[0].CommitId; var active = r.Locator.Read(Budget()).Observation.ActiveProfile; r.Close();
                if (defect == "old-root") r.Storage.Files["c-" + original + ".snapshot"][40] ^= 1;
                var c = r.Capabilities;
                var contracts = c.ReadableSlices.Where(x => defect != "old-contract" || x.SchemaVersion != 2)
                    .Where(x => defect != "roster-contract" || x.SchemaVersion != 3).ToArray();
                c = new SaveRecoveryCapabilities(contracts, defect == "binding" ? Array.Empty<SaveBinding>() : c.Bindings,
                    c.RuleVersions, c.NumericContractVersions, c.RandomContractVersions,
                    defect == "feature" ? new[] { "fm.player.application.v1" } : c.FeatureIds);
                var storage = defect == "player" ? new MemorySave(Guid.NewGuid().ToString("N")) : r.Storage;
                var catalog = defect == "package" ? new PublishedContentCatalog(new PublishedContentTestData.MemoryStorage(),
                    ContentConsumerCapabilities.Current) : r.Catalog;
                var files = CopyFiles(r.Storage.Files); var locatorFiles = CopyFiles(r.LocatorStorage.Blobs);
                var arch = FightMatchDemoArchitecture.Interface;
                try
                {
                    var result = arch.GetSystem<PlayerSessionSystem>().OpenExisting(active, storage, catalog, c, Budget());
                    Assert.IsFalse(result.View.IsPublishedHeadVerified); Assert.IsFalse(result.IsCommitted);
                    Assert.AreNotEqual("InitializationReady", result.Code); Assert.IsNotNull(result.Diagnostic);
                    SameFiles(files, r.Storage.Files); SameFiles(locatorFiles, r.LocatorStorage.Blobs);
                }
                finally { arch.Deinit(); }
            }
        }
    }
}
