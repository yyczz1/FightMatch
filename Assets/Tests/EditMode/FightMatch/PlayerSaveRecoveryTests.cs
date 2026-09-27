using System;
using System.Linq;
using System.Numerics;
using FightMatch.Application;
using FightMatch.Content;
using FightMatch.Platform;
using NUnit.Framework;
using QFramework;
using static FightMatch.Core.Tests.PlayerSessionTestData;

namespace FightMatch.Core.Tests
{
    public class PlayerSaveRecoveryTests
    {
        [Test] public void C1V01_RecordBeforeCandidateReopensOriginalV1WithoutCurrentLookupOrNewPreparation()
        {
            var fixture = new PublishedFixture(); var caps = Caps(fixture.V1, fixture.V2);
            using (var r = new Rig(false, fixture.Catalog, fixture.V1, caps, source: fixture.Source))
            {
                var original = r.Profile; var recorded = r.Locator.RecordCreateIntent(original.CreateRecord, Budget());
                Assert.IsTrue(recorded.IsAccepted); Assert.AreEqual(0, r.Storage.SnapshotCreates); r.Close(); fixture.Activate(fixture.V2);
                var storage = new NoCurrentStorage(fixture.Storage); var catalog = new PublishedContentCatalog(storage, ContentConsumerCapabilities.Current);
                using (var reopened = new Rig(false, catalog, fixture.V1, caps, r.Storage, r.LocatorStorage, true, fixture.Source))
                {
                    CollectionAssert.AreEqual(original.Intent.CanonicalBytes, reopened.Profile.Intent.CanonicalBytes);
                    Assert.AreEqual(original.PlayerId, reopened.Profile.PlayerId); Assert.AreEqual(original.OperationId, reopened.Profile.OperationId);
                    Assert.AreEqual(original.CreateRecord.RecordSha256, reopened.Profile.CreateRecord.RecordSha256);
                    Assert.IsTrue(reopened.Profile.CreateRecord.ContentBinding.Same(fixture.V1.Binding));
                    Is(reopened.Session.ContinueCreate(reopened.Profile, reopened.Locator.Read(Budget()).Observation, reopened.Storage, reopened.Locator, caps, Budget()));
                    Assert.AreEqual(0, storage.CurrentReads); Assert.AreEqual(1, reopened.Storage.SnapshotCreates);
                    Assert.AreEqual(1, reopened.Head.Records.Count); Assert.IsEmpty(reopened.Head.Business.Rewards.BaseRewards);
                    Assert.IsEmpty(reopened.Profile.CreateRecord.GeneratedMaterials);
                    Witness(reopened, "record-only-resumed-v1");
                }
            }
        }
        [TestCase("snapshot-promoted", "SaveFailed")] [TestCase("marker-before", "CommitUnknown")] [TestCase("marker-after", "CommitUnknown")]
        public void C1V02_OriginalCandidateSurvivesCloseAndContinuesWithoutSecondInitialization(string fault, string code)
        {
            using (var r = new Rig(false))
            {
                r.Storage.Fault = fault; var failed = Is(r.Session.CreateNew(r.Profile, r.Storage, r.Locator, r.Capabilities, Budget()), code);
                var commit = failed.View.PendingCommitId; Assert.IsNotNull(commit); Assert.AreEqual(r.Profile.OperationId, failed.View.PendingOperationId);
                var bytes = (byte[])r.Storage.Files["c-" + commit + ".snapshot"].Clone(); var calls = r.Storage.SnapshotCreates;
                Is(r.Application.End(r.Profile.Intent, Budget()), "CreationPending");
                Is(r.Application.EndObserved(commit, Budget()), "CreationPending");
                Assert.IsFalse(r.Session.PrepareLifecycle(new PlayerLifecycleDraft(), Codec()).IsAccepted); r.Close();
                using (var reopened = new Rig(false, r.Catalog, r.Publication, r.Capabilities, r.Storage, r.LocatorStorage, true))
                {
                    var done = Is(reopened.Session.ContinueCreate(reopened.Profile, reopened.Locator.Read(Budget()).Observation,
                        r.Storage, reopened.Locator, r.Capabilities, Budget()));
                    Assert.AreEqual(commit, done.OriginalCommitId); Assert.AreEqual(commit, reopened.Head.Header.CommitId);
                    Assert.AreEqual(calls, r.Storage.SnapshotCreates); CollectionAssert.AreEqual(bytes, r.Storage.Files["c-" + commit + ".snapshot"]);
                    Assert.AreEqual(1, reopened.Head.Records.Count); Assert.AreEqual(r.Profile.OperationId, reopened.Head.Records[0].OperationId);
                    Assert.IsEmpty(reopened.Head.Business.Rewards.BaseRewards);
                    Assert.AreEqual(LocalPlayerProfileState.Active, reopened.Locator.Read(Budget()).Observation.State);
                    Witness(reopened, "candidate-resumed-" + fault);
                }
            }
        }
        [TestCase("before")] [TestCase("after")] [TestCase("bad-read")]
        public void C1V03_ConfirmationUnknownReopensAndDuplicateConfirmationPreservesLaterHead(string fault)
        {
            using (var r = new Rig(false))
            {
                r.LocatorStorage.FailAt = 1; r.LocatorStorage.Fault = fault;
                var failed = Is(r.Session.CreateNew(r.Profile, r.Storage, r.Locator, r.Capabilities, Budget()), "ConfirmationUnknown");
                Assert.AreEqual(CandidateApplicationPhase.CreationConfirmationRequired, failed.View.Phase);
                Assert.IsTrue(failed.View.IsPublishedHeadVerified); var init = r.Head.Header.CommitId;
                Assert.IsFalse(r.Session.PrepareVictory(null, Codec()).IsAccepted);
                Assert.IsFalse(r.Session.PrepareLifecycle(new PlayerLifecycleDraft(), Codec()).IsAccepted);
                Is(r.Application.End(r.Profile.Intent, Budget()), "CreationPending"); r.Close();
                using (var reopened = new Rig(false, r.Catalog, r.Publication, r.Capabilities, r.Storage, r.LocatorStorage, true))
                {
                    var observation = reopened.Locator.Read(Budget()).Observation;
                    Is(reopened.Session.ContinueCreate(reopened.Profile, observation, r.Storage, reopened.Locator, r.Capabilities, Budget()));
                    Assert.AreEqual(init, reopened.Head.Header.CommitId); Assert.AreEqual(1, r.Storage.SnapshotCreates);
                    reopened.Enter(); var later = reopened.Head; var disk = CopyFiles(r.Storage.Files);
                    var done = Is(reopened.Session.ContinueCreate(reopened.Profile, observation, r.Storage, reopened.Locator, r.Capabilities, Budget()));
                    Assert.AreEqual(init, done.OriginalCommitId); Assert.AreEqual(later.Header.CommitId, reopened.Head.Header.CommitId);
                    Assert.AreEqual(1, reopened.Head.Records.Count(x => x.Intent.Kind == CandidateApplicationKind.InitializeProfile));
                    Assert.IsEmpty(reopened.Head.Business.Rewards.BaseRewards); SameFiles(disk, r.Storage.Files);
                    var proof = TakeCore(PlayerProfileCreateRecordCodec.VerifyInitializationCommit(r.Profile.CreateRecord, reopened.Head, Codec()));
                    Assert.AreEqual(init, proof.OriginalInitializationCommitId);
                    Assert.AreEqual("AlreadyActive", reopened.Locator.ConfirmCommit(observation, proof, Budget()).Code);
                    Witness(reopened, "confirmed-later-head-" + fault);
                }
            }
        }
        [TestCase("before")] [TestCase("after")] [TestCase("partial")] [TestCase("bad-read")]
        public void RecordMustPersistAndReadBackBeforeAnyPlayerSaveWrite(string fault)
        {
            using (var r = new Rig(false))
            {
                r.LocatorStorage.FailAt = 0; r.LocatorStorage.Fault = fault;
                Is(r.Session.CreateNew(r.Profile, r.Storage, r.Locator, r.Capabilities, Budget()), "CreateIntentWriteUnknown");
                Assert.AreEqual(0, r.Storage.SnapshotCreates); Assert.IsFalse(r.Storage.Exists); Assert.IsNull(r.Head);
                var observed = r.Locator.Read(Budget());
                if (fault == "partial") { Assert.IsFalse(observed.IsAccepted); Assert.IsNotNull(observed.Diagnostic); Assert.IsNull(observed.Observation); }
                else
                {
                    Is(r.Session.CreateNew(r.Profile, r.Storage, r.Locator, r.Capabilities, Budget()));
                    Assert.AreEqual(1, r.Storage.SnapshotCreates); Assert.AreEqual(1, r.Head.Records.Count);
                }
            }
        }
        [TestCase("truncate")] [TestCase("version")] [TestCase("identity")] [TestCase("length")] [TestCase("unknown")] [TestCase("budget")]
        public void C1V04_BadUnknownOrOverBudgetLocatorIsNotAbsentAndIsPreserved(string defect)
        {
            using (var r = new Rig(false))
            {
                Assert.AreEqual("Absent", r.Locator.Read(Budget()).Code);
                Assert.IsTrue(r.Locator.RecordCreateIntent(r.Profile.CreateRecord, Budget()).IsAccepted);
                var key = LocalPlayerProfileLocator.CreateRecordKey; var original = r.LocatorStorage.Blobs[key];
                if (defect == "truncate") r.LocatorStorage.Blobs[key] = original.Take(original.Length / 2).ToArray();
                if (defect == "version") original[8] = 99;
                if (defect == "identity") original[16] ^= 1;
                if (defect == "length") for (var i = 12; i < 16; i++) original[i] = 255;
                if (defect == "unknown") r.LocatorStorage.ThrowRead = true;
                var budget = defect == "budget" ? new SaveStoreBudget(new SaveCodecBudget(new ExactMathBudget(), maxEnvelopeBytes: 128)) : Budget();
                var before = CopyFiles(r.LocatorStorage.Blobs); var result = r.Locator.Read(budget);
                Assert.IsFalse(result.IsAccepted); Assert.IsNotNull(result.Diagnostic); Assert.IsNull(result.Observation);
                Assert.AreNotEqual("Absent", result.Code); Assert.IsNotEmpty(result.Diagnostic.FieldPath);
                SameFiles(before, r.LocatorStorage.Blobs); Assert.AreEqual(0, r.Storage.SnapshotCreates);
            }
        }
        [Test] public void C1V04_ChangedFrozenRecipeCannotRecoverAndSameIdsDifferentPayloadConflict()
        {
            using (var r = new Rig(false))
            {
                Assert.IsTrue(r.Locator.RecordCreateIntent(r.Profile.CreateRecord, Budget()).IsAccepted);
                var bytes = r.Profile.CreateRecord.FrozenNewProfileDefinitionBytes.ToArray(); bytes[bytes.Length - 1] ^= 1;
                var changed = TakeCore(PlayerProfileCreateRecordCodec.Freeze(r.Profile.Intent, r.Profile.CreateRecord.NewProfileDefinitionId, 1,
                    bytes, Array.Empty<PlayerProfileCreateMaterial>(), Codec()));
                var before = CopyFiles(r.LocatorStorage.Blobs);
                Assert.AreEqual("CreateIntentConflict", r.Locator.RecordCreateIntent(changed, Budget()).Code); SameFiles(before, r.LocatorStorage.Blobs);
                var alternate = new PublishedContentTestData.MemoryStorage(); var locator = new LocalPlayerProfileLocator(alternate);
                var observed = locator.RecordCreateIntent(changed, Budget()); Assert.IsTrue(observed.IsAccepted);
                Assert.AreEqual("InconsistentCreateIntent", r.Session.RecoverCreateIntent(observed.Observation, r.Catalog, Codec()).Code);
                Assert.AreEqual(0, r.Storage.SnapshotCreates);
            }
        }
        [TestCase(false)] [TestCase(true)] public void MissingOriginalPackageRefusesRecordedAndActiveV1AfterV2Activation(bool active)
        {
            var fixture = new PublishedFixture(); var caps = Caps(fixture.V1, fixture.V2);
            using (var r = new Rig(active, fixture.Catalog, fixture.V1, caps, source: fixture.Source))
            {
                if (!active) Assert.IsTrue(r.Locator.RecordCreateIntent(r.Profile.CreateRecord, Budget()).IsAccepted);
                var observation = r.Locator.Read(Budget()).Observation; r.Close(); fixture.Activate(fixture.V2);
                fixture.Storage.Blobs.Remove(PublishedContentCatalog.BindingKey(fixture.V1.Binding));
                var disk = CopyFiles(r.Storage.Files); var locatorBefore = CopyFiles(r.LocatorStorage.Blobs);
                var arch = FightMatchDemoArchitecture.Interface;
                try
                {
                    var session = arch.GetSystem<PlayerSessionSystem>();
                    Assert.AreEqual("UnsupportedBinding", session.RecoverCreateIntent(observation, fixture.Catalog, Codec()).Code);
                    if (active) Is(session.OpenExisting(observation.ActiveProfile, r.Storage, fixture.Catalog, caps, Budget()), "UnsupportedBinding");
                    SameFiles(disk, r.Storage.Files); SameFiles(locatorBefore, r.LocatorStorage.Blobs);
                }
                finally { arch.Deinit(); }
            }
        }
        [Test] public void C1V05_LocatorIdempotenceOwnerStaleObservationAndWrongProfileProofAreChecked()
        {
            using (var r = new Rig(false))
            {
                var absent = r.Locator.Read(Budget()).Observation;
                var first = r.Locator.RecordCreateIntent(r.Profile.CreateRecord, Budget()); Assert.IsTrue(first.IsAccepted);
                Assert.AreEqual("AlreadyRecorded", r.Locator.RecordCreateIntent(r.Profile.CreateRecord, Budget()).Code);
                Is(r.Session.ContinueCreate(r.Profile, absent, r.Storage, r.Locator, r.Capabilities, Budget()), "StaleProfileObservation");
                var otherLocator = new LocalPlayerProfileLocator(r.LocatorStorage);
                Is(r.Session.ContinueCreate(r.Profile, otherLocator.Read(Budget()).Observation, r.Storage, r.Locator, r.Capabilities, Budget()), "StaleProfileObservation");
                Is(r.Session.ContinueCreate(r.Profile, first.Observation, r.Storage, r.Locator, r.Capabilities, Budget()));
                var proof = TakeCore(PlayerProfileCreateRecordCodec.VerifyInitializationCommit(r.Profile.CreateRecord, r.Head, Codec()));
                Assert.AreEqual("AlreadyActive", r.Locator.ConfirmCommit(absent, proof, Budget()).Code);
                Assert.AreEqual("StaleProfileObservation", otherLocator.ConfirmCommit(first.Observation, proof, Budget()).Code);
                Assert.AreEqual("AlreadyActive", r.Locator.RecordCreateIntent(r.Profile.CreateRecord, Budget()).Code);
                var unrelated = Prepared(r.Session.PrepareNewProfile(r.Catalog, Release, Codec()));
                Assert.IsFalse(PlayerProfileCreateRecordCodec.VerifyInitializationCommit(unrelated.CreateRecord, r.Head, Codec()).IsAccepted);
                var other = new LocalPlayerProfileLocator(new PublishedContentTestData.MemoryStorage());
                var row = other.RecordCreateIntent(unrelated.CreateRecord, Budget());
                Assert.AreEqual("InconsistentCreateIntent", other.ConfirmCommit(row.Observation, proof, Budget()).Code);
                Assert.AreEqual("CreationPending", r.Locator.RecordCreateIntent(unrelated.CreateRecord, Budget()).Code);
            }
        }
        [TestCase(false)] [TestCase(true)] public void C1V05_ActiveMissingOrCorruptHeadNeverReinitializes(bool corrupt)
        {
            using (var r = new Rig())
            {
                var active = r.Locator.Read(Budget()).Observation.ActiveProfile; r.Close();
                if (corrupt) r.Storage.Files["c-" + active.OriginalInitializationCommitId + ".snapshot"] = new byte[] { 1, 2, 3 };
                else { r.Storage.Files.Clear(); r.Storage.Exists = false; }
                var snapshotCreates = r.Storage.SnapshotCreates; var before = CopyFiles(r.Storage.Files); var arch = FightMatchDemoArchitecture.Interface;
                try
                {
                    var session = arch.GetSystem<PlayerSessionSystem>();
                    var result = session.OpenExisting(active, r.Storage, r.Catalog, r.Capabilities, Budget());
                    Assert.IsFalse(result.View.IsPublishedHeadVerified); Assert.AreNotEqual("InitializationReady", result.Code);
                    Assert.AreEqual(snapshotCreates, r.Storage.SnapshotCreates); SameFiles(before, r.Storage.Files);
                }
                finally { arch.Deinit(); }
            }
        }
        [Test] public void C1V05_ASecondValidInitializationCommitCannotReplaceTheOriginalAnchor()
        {
            using (var r = new Rig())
            {
                var first = r.Locator.Read(Budget()).Observation; var original = r.Head.Header.CommitId; r.Close();
                var arch = FightMatchDemoArchitecture.Interface;
                try
                {
                    var other = new MemorySave(r.Profile.PlayerId); var locator = new LocalPlayerProfileLocator(new PublishedContentTestData.MemoryStorage());
                    var created = Is(arch.GetSystem<PlayerSessionSystem>().CreateNew(r.Profile, other, locator, r.Capabilities, Budget()));
                    Assert.AreNotEqual(original, created.OriginalCommitId);
                    var proof = TakeCore(PlayerProfileCreateRecordCodec.VerifyInitializationCommit(r.Profile.CreateRecord, created.View.PublishedSnapshot, Codec()));
                    var before = CopyFiles(r.LocatorStorage.Blobs);
                    Assert.AreEqual("CreateIntentConflict", r.Locator.ConfirmCommit(first, proof, Budget()).Code);
                    SameFiles(before, r.LocatorStorage.Blobs);
                }
                finally { arch.Deinit(); }
            }
        }
        [TestCase("old-root")] [TestCase("capability")] [TestCase("binding")] [TestCase("player")]
        public void ExactRetainedRootsCapabilitiesAndPlayerIdentityAreRequiredWithoutWriting(string defect)
        {
            using (var r = new Rig())
            {
                var init = r.Head.Header.CommitId; r.Enter(); var active = r.Locator.Read(Budget()).Observation.ActiveProfile; r.Close();
                if (defect == "old-root") r.Storage.Files["c-" + init + ".snapshot"][40] ^= 1;
                var c = r.Capabilities;
                if (defect == "capability" || defect == "binding") c = new SaveRecoveryCapabilities(c.ReadableSlices,
                    defect == "binding" ? Array.Empty<SaveBinding>() : c.Bindings, c.RuleVersions, c.NumericContractVersions, c.RandomContractVersions,
                    defect == "capability" ? Array.Empty<string>() : c.FeatureIds);
                var storage = defect == "player" ? new MemorySave(Guid.NewGuid().ToString("N")) : r.Storage;
                var before = CopyFiles(r.Storage.Files); var arch = FightMatchDemoArchitecture.Interface;
                try
                {
                    var result = arch.GetSystem<PlayerSessionSystem>().OpenExisting(active, storage, r.Catalog, c, Budget());
                    Assert.IsFalse(result.View.IsPublishedHeadVerified); Assert.IsFalse(result.IsCommitted);
                    Assert.AreNotEqual("InitializationReady", result.Code); Assert.IsNotNull(result.Diagnostic);
                    Assert.IsNotEmpty(result.Diagnostic.FieldPath); SameFiles(before, r.Storage.Files);
                }
                finally { arch.Deinit(); }
            }
        }
        [TestCase("snapshot-before", "SaveFailed")] [TestCase("marker-before", "CommitUnknown")] [TestCase("marker-after", "CommitUnknown")]
        public void H02RetryUsesOriginalEncodedEntropyAndDoesNotRunNewEntryBuilder(string fault, string code)
        {
            using (var r = new Rig())
            {
                var request = r.EntryRequest(); r.Storage.Fault = fault;
                var failed = Is(r.Lifecycle.Submit(request, Budget()), code); var commit = failed.View.PendingCommitId;
                var bytes = r.Storage.Files.TryGetValue("c-" + commit + ".snapshot", out var saved) ? (byte[])saved.Clone() : null;
                Is(r.Lifecycle.Submit(request, Budget()), code); var done = Is(r.Lifecycle.Retry(request, Budget()));
                Assert.AreEqual(commit, done.OriginalCommitId); Assert.AreEqual(request.OperationId, r.Head.Records.Last().OperationId);
                if (bytes != null) CollectionAssert.AreEqual(bytes, r.Storage.Files["c-" + commit + ".snapshot"]);
                var before = r.Head; var random = before.Business.ActiveHistory.Binding; var disk = CopyFiles(r.Storage.Files);
                Is(r.Lifecycle.Retry(request, Budget())); Is(r.Lifecycle.Resolve(request, Budget()));
                Assert.AreSame(before, r.Head); Assert.AreSame(random, r.Head.Business.ActiveHistory.Binding); SameFiles(disk, r.Storage.Files);
            }
        }
        [TestCase("snapshot-promoted", "SaveFailed")] [TestCase("marker-after", "CommitUnknown")]
        public void SettlingOriginalS17AfterUnknownWriteAndReopenAwardsOnlyOnce(string fault, string code)
        {
            using (var r = new Rig())
            {
                r.Enter(); r.Win(); var request = Prepared(r.Session.PrepareVictory(new LocalPlayerClock().Read(), Codec()));
                var active = r.Locator.Read(Budget()).Observation.ActiveProfile; r.Storage.Fault = fault;
                var pending = Is(r.Lifecycle.Submit(request, Budget()), code); var commit = pending.View.PendingCommitId;
                var bytes = (byte[])r.Storage.Files["c-" + commit + ".snapshot"].Clone(); var writes = r.Storage.SnapshotCreates; r.Close();
                var arch = FightMatchDemoArchitecture.Interface;
                try
                {
                    var session = arch.GetSystem<PlayerSessionSystem>(); var application = arch.GetSystem<CandidateApplicationSystem>();
                    var opened = session.OpenExisting(active, r.Storage, r.Catalog, r.Capabilities, Budget());
                    if (fault == "snapshot-promoted") { Is(opened, "Pending"); Is(application.ResumeObserved(commit, Budget())); }
                    else Is(opened, "Ready");
                    var head = application.QueryView().View.PublishedSnapshot;
                    Assert.AreEqual(commit, head.Header.CommitId); var reward = head.Business.Rewards.BaseRewards.Single();
                    Assert.AreEqual(reward.Experience.Single().Amount, head.Business.Character.Experience);
                    Is(arch.GetSystem<CandidateLifecycleApplicationSystem>().Submit(request, Budget()));
                    Assert.AreEqual(writes, r.Storage.SnapshotCreates); CollectionAssert.AreEqual(bytes, r.Storage.Files["c-" + commit + ".snapshot"]);
                    Assert.AreSame(head, application.QueryView().View.PublishedSnapshot);
                }
                finally { arch.Deinit(); }
            }
        }
    }
}
