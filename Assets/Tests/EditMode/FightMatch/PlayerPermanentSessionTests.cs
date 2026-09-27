using System;
using System.Linq;
using System.Numerics;
using FightMatch.Application;
using FightMatch.Platform;
using NUnit.Framework;
using QFramework;
using static FightMatch.Core.Tests.PlayerSessionTestData;
using static FightMatch.Core.Tests.PlayerRosterTestData;

namespace FightMatch.Core.Tests
{
    [TestFixture]
    public sealed class PlayerPermanentSessionTests
    {
        [Test]
        public void CB01_RealFirstReleaseQueryAndPreviewReportMissingDefinitionsWithoutWrites()
        {
            using (var rig = RealRig())
            {
                var head = rig.Head;
                var files = CopyFiles(rig.Storage.Files);
                var query = rig.Session.QueryPermanent(head.Business.Character.CharacterId);
                Assert.AreEqual("NoPublishedDefinition", query.CardAvailability);
                Assert.AreEqual("NoPublishedDefinition", query.SkillAvailability);
                Assert.AreEqual("NoPublishedDefinition", query.RecipeAvailability);
                Assert.IsNotNull(query.Amounts);
                Assert.IsEmpty(query.SourceChoices);
                Assert.IsEmpty(query.OriginalOperations);
                foreach (var kind in new[] { CandidatePermanentKind.UseExperienceCards, CandidatePermanentKind.LearnSkill, CandidatePermanentKind.Craft })
                {
                    var result = rig.Session.PreviewPermanent(new PlayerPermanentDraft {
                        ExpectedCommitId = head.Header.CommitId, CharacterId = head.Business.Character.CharacterId,
                        Kind = kind, DefinitionId = "not-in-first-release", Quantity = kind == CandidatePermanentKind.LearnSkill ? (BigInteger?)null : 5 }, Codec());
                    Assert.AreEqual("NoPublishedDefinition", result.Code);
                }
                var binding = TakeCore(DefinitionBinding.Prepare(rig.Publication.Binding, "level:16", 1, Codec()));
                Assert.AreEqual("NoPublishedDefinition", rig.Session.PrepareTeachingEntry(binding, head.Header.CommitId, Codec()).Code);
                Assert.AreSame(head, rig.Head);
                SameFiles(files, rig.Storage.Files);
                Assert.IsNull(rig.Application.QueryView().View.PendingOperationId);
                Assert.AreEqual(298, rig.Profile.CreateRecord.FrozenNewProfileDefinitionBytes.Count);
            }
        }

        [TestCase(false, false)]
        [TestCase(true, false)]
        [TestCase(true, true)]
        public void CB16_CB17_ActualV3ToV4PersistenceAndReconstructionPreserveRosterAndF2Anchor(bool active, bool playback)
        {
            using (var rig = RealRig(false))
            {
                Assert.AreEqual(CandidateBusinessFormat.PublishedV2, rig.Session.QueryRoster().Format);
                Assert.IsFalse(rig.Session.QueryRoster().IsMaterialized);
                Is(rig.Lifecycle.Submit(Prepared(rig.Session.PrepareRosterMigration(rig.Head.Header.CommitId, Codec())), Budget()));
                Assert.AreEqual(CandidateBusinessFormat.PublishedRosterV3, rig.Session.QueryRoster().Format);
                Assert.IsTrue(rig.Session.QueryRoster().IsMaterialized);
                if (active) Is(rig.Lifecycle.Submit(EntryRequest(rig), Budget()));
                if (playback) Is(rig.Battle.Submit(rig.AttackRequest(), Budget()).Application);
                var token = rig.Battle.QueryView().PresentationToken;
                if (playback) Assert.IsNotNull(token);
                var before = rig.Head;
                var initialCommit = before.Records[0].CommitId;
                var recordBytes = TakeCore(PlayerProfileCreateRecordCodec.Write(rig.Profile.CreateRecord, Codec()));
                var request = Prepared(rig.Session.PreparePermanentMigration(before.Header.CommitId, Codec()));
                var done = Is(rig.Lifecycle.Submit(request, Budget()));
                Assert.AreSame(token, rig.Battle.QueryView().PresentationToken);
                Assert.AreEqual(CandidateBusinessFormat.PublishedPermanentV4, rig.Session.QueryRoster().Format);
                Assert.IsTrue(rig.Session.QueryRoster().IsMaterialized);
                CollectionAssert.AreEqual(before.Business.Roster.Formation, rig.Session.QueryRoster().Slots);
                CollectionAssert.AreEqual(before.Business.Roster.Characters.Select(x => x.CharacterId), rig.Session.QueryRoster().Characters.Select(x => x.CharacterId));
                Assert.AreEqual(before.Business.Roster.FormationRevision, rig.Session.QueryRoster().FormationRevision);
                Assert.AreEqual(before.Header.SaveGeneration + 1, rig.Head.Header.SaveGeneration);
                Assert.IsTrue(CandidateBusinessSaveCodec.SamePermanentOwners(before.Business.Roster, before.Business.Inventory, rig.Head.Business.Roster, rig.Head.Business.Inventory, Codec()));
                if (active) CollectionAssert.AreEqual(CandidatePermanentTestData.BattleBytes(before.Business), CandidatePermanentTestData.BattleBytes(rig.Head.Business));
                foreach (var original in before.Records)
                    CollectionAssert.AreEqual(original.Intent.CanonicalBytes, rig.Head.Records.Single(x => x.OperationId == original.OperationId).Intent.CanonicalBytes);
                var profile = rig.Locator.Read(Budget()).Observation.ActiveProfile;
                var commit = done.OriginalCommitId;
                rig.Close();
                var architecture = FightMatchDemoArchitecture.Interface;
                try
                {
                    var session = architecture.GetSystem<PlayerSessionSystem>();
                    var app = architecture.GetSystem<CandidateApplicationSystem>();
                    Is(session.OpenExisting(profile, rig.Storage, rig.Catalog, rig.Capabilities, Budget()), "Ready");
                    var restored = app.QueryView().View.PublishedSnapshot;
                    Assert.AreEqual(commit, restored.Header.CommitId);
                    Assert.AreEqual(CandidateBusinessFormat.PublishedPermanentV4, session.QueryRoster().Format);
                    Assert.IsTrue(session.QueryRoster().IsMaterialized);
                    CollectionAssert.AreEqual(before.Business.Roster.Formation, session.QueryRoster().Slots);
                    Assert.AreEqual(before.Business.Roster.FormationRevision, session.QueryRoster().FormationRevision);
                    var proof = TakeCore(PlayerProfileCreateRecordCodec.VerifyInitializationCommit(rig.Profile.CreateRecord, restored, Codec()));
                    Assert.AreEqual(initialCommit, proof.OriginalInitializationCommitId);
                    Assert.AreEqual(BigInteger.One, restored.Records[0].Generation);
                    CollectionAssert.AreEqual(recordBytes, TakeCore(PlayerProfileCreateRecordCodec.Write(rig.Profile.CreateRecord, Codec())));
                }
                finally { architecture.Deinit(); }
            }
        }

        [TestCase("snapshot-before")]
        [TestCase("snapshot-promoted")]
        [TestCase("marker-before")]
        [TestCase("marker-after")]
        public void CB15_RealRuntimeResumesOriginalMigrationAndBlocksNewWritesDuringUnknown(string fault)
        {
            using (var rig = RealRig())
            {
                var before = rig.Head;
                var request = Prepared(rig.Session.PreparePermanentMigration(before.Header.CommitId, Codec()));
                var canonical = request.Intent.CanonicalBytes.ToArray();
                rig.Storage.Fault = fault;
                var failure = rig.Lifecycle.Submit(request, Budget());
                Assert.IsFalse(failure.IsCommitted);
                Assert.AreSame(before, rig.Head);
                Assert.AreEqual(request.OperationId, failure.View.PendingOperationId);
                Assert.IsFalse(rig.Session.PreparePermanentMigration(before.Header.CommitId, Codec()).IsAccepted);
                var commit = failure.View.PendingCommitId;
                var bytes = PlayerRosterSessionTests.CandidateBytes(rig.Storage, commit);
                if (bytes == null)
                {
                    Is(rig.Lifecycle.Retry(request, Budget()));
                    bytes = rig.Storage.Files["c-" + commit + ".snapshot"];
                }
                var profile = rig.Locator.Read(Budget()).Observation.ActiveProfile;
                rig.Close();
                var architecture = FightMatchDemoArchitecture.Interface;
                try
                {
                    var session = architecture.GetSystem<PlayerSessionSystem>();
                    var app = architecture.GetSystem<CandidateApplicationSystem>();
                    var lifecycle = architecture.GetSystem<CandidateLifecycleApplicationSystem>();
                    var opened = session.OpenExisting(profile, rig.Storage, rig.Catalog, rig.Capabilities, Budget());
                    if (opened.View.ObservedCandidateCommitIds.Count != 0) Is(app.ResumeObserved(commit, Budget()));
                    else Is(opened, "Ready");
                    Assert.AreEqual(commit, Is(lifecycle.Submit(request, Budget())).OriginalCommitId);
                    Assert.AreEqual(1, app.QueryView().View.PublishedSnapshot.Records.Count(x => x.Intent.Kind == CandidateApplicationKind.MigratePermanent));
                    CollectionAssert.AreEqual(canonical, request.Intent.CanonicalBytes);
                    CollectionAssert.AreEqual(bytes, rig.Storage.Files["c-" + commit + ".snapshot"]);
                    var files = CopyFiles(rig.Storage.Files);
                    Is(lifecycle.Resolve(request, Budget()));
                    lifecycle.End(request, Budget());
                    SameFiles(files, rig.Storage.Files);
                }
                finally { architecture.Deinit(); }
            }
        }

        [TestCase("M")]
        [TestCase("W")]
        public void CB18_UnsupportedNewCombatCapabilityRejectsBeforeBattleIdentityOrEntropy(string id)
        {
            using (var data = new CandidatePermanentTestData(warriorLevel: 5, warriorXp: 0))
            {
                if (id == "W") data.Apply(data.Learn("W"));
                data.Formation(id, null, null);
                var ids = data.IdCalls;
                var entropy = data.EntropyCalls;
                var head = data.Head;
                var result = data.Build(data.Entry());
                Assert.IsFalse(result.IsAccepted);
                Assert.AreEqual("UnsupportedBattleCapability", result.Diagnostic.Code);
                Assert.AreEqual(ids, data.IdCalls);
                Assert.AreEqual(entropy, data.EntropyCalls);
                Assert.AreSame(head, data.Head);
            }
        }

        [Test]
        public void CB19_ArbitraryBuilderCannotSubmitIntoRealPlayerSession()
        {
            using (var rig = RealRig())
            {
                var request = Prepared(rig.Session.PreparePermanentMigration(rig.Head.Header.CommitId, Codec()));
                var calls = 0;
                var before = rig.Head;
                var result = rig.Application.Submit(request.Intent, (basis, intent, budget) => {
                    calls++;
                    return CandidateApplicationBuildResult.Success(basis.Business, new CandidateApplicationResultInput());
                }, Budget());
                Assert.IsFalse(result.IsCommitted);
                Assert.AreEqual(0, calls);
                Assert.AreSame(before, rig.Head);
                Assert.AreEqual(14, PlayerSessionSystem.RequiredRecoveryContracts.Count);
                CollectionAssert.Contains(PlayerSessionSystem.RequiredRecoveryFeatures, "fm.player.permanent.v1");
            }
        }

        [Test]
        public void CB16_FMPROF02InitialAnchorSurvivesV4MigrationAndObjectReconstruction()
        {
            using (var rig = new Rig(false))
            {
                var profile = Prepared(rig.Session.PrepareNewRosterProfile(rig.Catalog, Release, Codec()));
                var storage = new MemorySave(profile.PlayerId);
                var caps = RosterCaps(rig.Publication);
                Is(rig.Session.CreateNew(profile, storage, rig.Locator, caps, Budget()));
                Assert.AreEqual(CandidateBusinessFormat.PublishedRosterV3, rig.Session.QueryRoster().Format);
                var original = TakeCore(PlayerProfileCreateRecordCodec.Write(profile.CreateRecord, Codec()));
                Assert.AreEqual(2, profile.CreateRecord.RecordFormatVersion);
                var initial = rig.Head.Records[0].CommitId;
                Is(rig.Lifecycle.Submit(Prepared(rig.Session.PreparePermanentMigration(rig.Head.Header.CommitId, Codec())), Budget()));
                var final = rig.Head.Header.CommitId;
                var active = rig.Locator.Read(Budget()).Observation.ActiveProfile;
                rig.Close();
                var architecture = FightMatchDemoArchitecture.Interface;
                try
                {
                    var session = architecture.GetSystem<PlayerSessionSystem>();
                    var app = architecture.GetSystem<CandidateApplicationSystem>();
                    Is(session.OpenExisting(active, storage, rig.Catalog, caps, Budget()), "Ready");
                    var restored = app.QueryView().View.PublishedSnapshot;
                    Assert.AreEqual(final, restored.Header.CommitId);
                    Assert.IsTrue(session.QueryRoster().IsMaterialized);
                    Assert.AreEqual(CandidateBusinessFormat.PublishedPermanentV4, session.QueryRoster().Format);
                    Assert.AreEqual(initial, TakeCore(PlayerProfileCreateRecordCodec.VerifyInitializationCommit(
                        profile.CreateRecord, restored, Codec())).OriginalInitializationCommitId);
                    Assert.AreEqual(BigInteger.One, restored.Records[0].Generation);
                    CollectionAssert.AreEqual(original, TakeCore(PlayerProfileCreateRecordCodec.Write(profile.CreateRecord, Codec())));
                }
                finally { architecture.Deinit(); }
            }
        }

        [Test]
        public void CB16_UnknownV3EntryResolvesInOriginalFormatBeforeExplicitMigration()
        {
            using (var rig = RealRig())
            {
                var request = EntryRequest(rig);
                var intent = request.Intent.CanonicalBytes.ToArray();
                rig.Storage.Fault = "marker-before";
                var failed = rig.Lifecycle.Submit(request, Budget());
                Assert.IsFalse(failed.IsCommitted);
                var commit = failed.View.PendingCommitId;
                var bytes = PlayerRosterSessionTests.CandidateBytes(rig.Storage, commit);
                Assert.IsNotNull(bytes);
                Assert.IsFalse(rig.Session.PreparePermanentMigration(rig.Head.Header.CommitId, Codec()).IsAccepted);
                var active = rig.Locator.Read(Budget()).Observation.ActiveProfile;
                rig.Close();
                var architecture = FightMatchDemoArchitecture.Interface;
                try
                {
                    var session = architecture.GetSystem<PlayerSessionSystem>();
                    var app = architecture.GetSystem<CandidateApplicationSystem>();
                    var lifecycle = architecture.GetSystem<CandidateLifecycleApplicationSystem>();
                    var opened = session.OpenExisting(active, rig.Storage, rig.Catalog, rig.Capabilities, Budget());
                    Assert.Contains(commit, opened.View.ObservedCandidateCommitIds.ToArray());
                    Is(app.ResumeObserved(commit, Budget()));
                    Assert.AreEqual(commit, Is(lifecycle.Submit(request, Budget())).OriginalCommitId);
                    Assert.AreEqual(CandidateBusinessFormat.PublishedRosterV3, session.QueryRoster().Format);
                    CollectionAssert.AreEqual(intent, request.Intent.CanonicalBytes);
                    CollectionAssert.AreEqual(bytes, rig.Storage.Files["c-" + commit + ".snapshot"]);
                    Is(lifecycle.Submit(Prepared(session.PreparePermanentMigration(commit, Codec())), Budget()));
                    Assert.AreEqual(CandidateBusinessFormat.PublishedPermanentV4, session.QueryRoster().Format);
                    Assert.AreEqual(commit, Is(lifecycle.Submit(request, Budget())).OriginalCommitId);
                }
                finally { architecture.Deinit(); }
            }
        }
    }
}
