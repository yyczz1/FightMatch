using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using FightMatch.Application;
using FightMatch.Content;
using FightMatch.Platform;
using FlowPuzzle.Core;
using NUnit.Framework;
using QFramework;
using static FightMatch.Core.Tests.PlayerSessionTestData;

namespace FightMatch.Core.Tests
{
    [TestFixture]
    public sealed class ChapterMageLifecycleTests
    {
        [Test]
        public void NewChapterProfilePlaysThroughEightAtomicUnlockThenExplicitMageFormationAndColdRecovery()
        {
            var fixture = new ChapterFixture();
            string mageId; BigInteger mageLevel; byte[] stateBeforeClose;
            MemorySave storage; PublishedContentTestData.MemoryStorage locatorStorage;
            using (var rig = new ChapterRig(fixture))
            {
                Assert.AreEqual(CandidateBusinessFormat.PublishedChapterV5, rig.Head.Business.Format);
                Assert.IsTrue(rig.Session.QueryRoster().IsMaterialized);
                Assert.AreEqual(1, rig.Head.Business.Roster.Characters.Count);
                CollectionAssert.AreEqual(new uint[] { 5, 5, 5, 5, 5, 5 }, Envelope(rig.Storage, rig.Head.Header.CommitId).RequiredSliceContracts.Select(x => x.SchemaVersion));
                var warriorId = rig.Head.Business.Roster.Characters.Single().CharacterId;
                var originalFormation = rig.Head.Business.Roster.Formation.ToArray();
                for (var level = 1; level <= 7; level++)
                {
                    rig.Enter(level); rig.Win(); rig.Settle();
                    Assert.AreEqual(1, rig.Head.Business.Roster.Characters.Count);
                }
                rig.Enter(8); rig.Win();
                var pending = rig.Head; var request = Prepared(rig.Session.PrepareVictory(PlayerRosterTestData.Time(), Codec()));
                Assert.AreEqual(5U, request.Intent.FormatVersion);
                rig.Storage.Fault = "marker-after";
                Is(rig.Lifecycle.Submit(request, Budget()), "CommitUnknown");
                Assert.AreSame(pending, rig.Head, "An unknown save must not publish half an unlock.");
                Is(rig.Lifecycle.Resolve(request, Budget()));
                var completed = rig.Head; var business = completed.Business;
                Assert.AreEqual(2, business.Roster.Characters.Count);
                Assert.AreEqual(2, rig.Session.QueryRoster().Characters.Count);
                Assert.IsTrue(rig.Session.QueryRoster().IsMaterialized);
                var mage = business.Roster.Characters.Single(x => x.Definition.ClassKind == CharacterClassKind.Mage);
                mageId = mage.CharacterId; mageLevel = mage.Level;
                Assert.AreEqual(new BigInteger(3), mageLevel); Assert.AreEqual(BigInteger.Zero, mage.Experience);
                Assert.IsTrue(mage.IsReady); Assert.IsEmpty(mage.BaseRewards);
                Assert.IsFalse(business.Roster.GetPermanentEffects().Any(x => x.Quote.CharacterId == mageId));
                CollectionAssert.AreEqual(originalFormation, business.Roster.Formation);
                var reward = business.Rewards.BaseRewards.Single(x => x.Report.Level.LevelId == "fixture:8");
                Assert.AreEqual(1, reward.Experience.Count); Assert.AreEqual(warriorId, reward.Experience[0].CharacterId);
                Assert.AreEqual(2, business.Progression.GetChapterEffects().Count);
                Assert.AreEqual(request.OperationId, business.Progression.GetChapterEffects().Single(x => x.Kind == CandidateChapterMilestoneKind.AcquireClass).OperationId);
                Assert.IsTrue(business.Progression.OpenFacts.Any(x => x.Level.LevelId == "fixture:9"));
                Assert.AreEqual(BigInteger.One, business.Inventory.Holdings.Single(x => x.ItemId == "item:experience-card-50").T);
                var saved = CopyFiles(rig.Storage.Files);
                Is(rig.Lifecycle.Submit(request, Budget())); Is(rig.Lifecycle.Resolve(request, Budget()));
                SameFiles(saved, rig.Storage.Files); Assert.AreSame(completed, rig.Head);
                // A new attempt/operation cannot mint the class or the first-clear card a second time.
                rig.Enter(8); rig.Win(); rig.Settle();
                Assert.AreEqual(2, rig.Head.Business.Roster.Characters.Count);
                Assert.AreEqual(mageId, rig.Head.Business.Roster.Characters.Single(x => x.Definition.ClassKind == CharacterClassKind.Mage).CharacterId);
                Assert.AreEqual(BigInteger.One, rig.Head.Business.Inventory.Holdings.Single(x => x.ItemId == "item:experience-card-50").T);
                var formation = Prepared(rig.Session.PrepareFormation(new PlayerFormationDraft { ExpectedCommitId = rig.Head.Header.CommitId,
                    ExpectedFormationRevision = rig.Head.Business.Roster.FormationRevision, Slots = new[] { warriorId, mageId, null } }, Codec()));
                Is(rig.Lifecycle.Submit(formation, Budget())); rig.Enter(9);
                var entry = rig.Head.Business.ActiveHistory.CurrentRun.Baseline.Entry;
                Assert.AreEqual(2, entry.ReadyParticipants.Count); Assert.AreEqual(1, entry.ReadyParticipants.Single(x => x.CharacterId == mageId).OriginalSlot);
                var attack = rig.AttackRequest(mageId, 1);
                Assert.AreEqual(5U, attack.Intent.FormatVersion);
                var hitCall = rig.Battle.Submit(attack, Budget()); Is(hitCall.Application); rig.FinishPresentation(hitCall);
                var hit = rig.Head.Business.ActiveHistory.CurrentRun.Records.Last().DirectAttack.DamageFacts.Single();
                Assert.AreEqual(EntryDamageKind.Magic, hit.DamageKind); Assert.NotNull(hit.Penetration); Assert.IsNull(hit.Crit);
                stateBeforeClose = CandidateBattleReportFingerprint.Encode(rig.Head.Business.ActiveHistory.CurrentRun.CurrentSnapshot, Codec().Math);
                storage = rig.Storage; locatorStorage = rig.LocatorStorage;
            }
            using (var reopened = new ChapterRig(fixture, storage, locatorStorage))
            {
                CollectionAssert.AreEqual(stateBeforeClose, CandidateBattleReportFingerprint.Encode(reopened.Head.Business.ActiveHistory.CurrentRun.CurrentSnapshot, Codec().Math));
                Assert.AreEqual(mageLevel, reopened.Head.Business.Roster.Find(mageId).Level);
                Assert.AreEqual(BigInteger.Zero, reopened.Head.Business.Roster.Find(mageId).Experience);
                Assert.AreEqual(2, reopened.Head.Business.Progression.GetChapterEffects().Count);
            }
        }

        // This is a synthetic nine-level protocol fixture. It does not approve the actual authoring tables or balance.
        private sealed class ChapterFixture
        {
            internal readonly PublishedSource Source;
            internal readonly PublishedContentCatalog Catalog;
            internal readonly ResolvedPublication Publication;
            internal ChapterFixture()
            {
                Source = ChapterContentTestData.Source();
                Source.Chapter.ReleaseSetId = Release;
                var template = PublishedContentTestData.Copy(Source);
                Source.Levels.Clear(); Source.Progression.Levels.Clear();
                for (var i = 1; i <= 9; i++)
                {
                    var row = PublishedContentTestData.Copy(template).Levels[0];
                    row.Level.LevelId = "fixture:" + i; row.Reward.LevelId = row.Level.LevelId;
                    row.Reward.RewardDefinitionId = "fixture:reward:" + i;
                    row.Level.RecommendedLevel = i >= 8 ? 3 : 1;
                    Source.Levels.Add(row);
                    Source.Progression.Levels.Add(new CandidateProgressionLevelInput { LevelId = row.Level.LevelId, LevelVersion = "1",
                        UnlockRuleId = "fixture:unlock:" + i, EntryKind = CandidateProgressionEntryKind.Ordinary,
                        UnlockKind = i == 1 ? CandidateProgressionUnlockKind.InitiallyOpen : CandidateProgressionUnlockKind.AfterWholeLevelClear,
                        UnlockAfterLevelId = i == 1 ? null : "fixture:" + (i - 1), RequiredFeatures = new List<string>() });
                }
                Source.NewProfile.OpenLevels = new List<string> { "fixture:1" };
                Source.Chapter.CharacterUnlocks[0].AfterLevelId = "fixture:8";
                Source.Chapter.FirstClearItems[0].LevelId = "fixture:8";
                var contentStorage = new PublishedContentTestData.MemoryStorage();
                Catalog = new PublishedContentCatalog(contentStorage, ContentConsumerCapabilities.Current);
                var job = new DemoContentDraft(Source.DraftId).BeginJob();
                var prepared = ChapterContentTestData.Prepare(Source, job);
                Publication = PlayerSessionTestData.Content(Catalog.Publish(prepared, prepared.Validation, PublishedContentTestData.Review(prepared), job,
                    "fixture:chapter-lifecycle-publication", PublishedContentTestData.StoreBudget())).Publication;
                contentStorage.Blobs[PublishedContentCatalog.ReleaseSetKey("player", Release)] = PlayerSessionTestData.Content(PublishedContentCodec.EncodeReleaseSet(new ContentReleaseSet {
                    SchemaVersion = 1, Scope = "player", ReleaseSetId = Release, Binding = ContentBindingRecord.From(Publication.Binding),
                    PublicationReceiptSha256 = PublishedContentCodec.Sha256(Publication.ReceiptBytes.ToArray()) }, PublishedContentTestData.StoreBudget()));
            }
        }

        private sealed class ChapterRig : IDisposable
        {
            private readonly IArchitecture architecture;
            private readonly ChapterFixture fixture;
            internal readonly CandidateApplicationSystem Application;
            internal readonly CandidateLifecycleApplicationSystem Lifecycle;
            internal readonly CandidateBattleApplicationSystem Battle;
            internal readonly PlayerSessionSystem Session;
            internal readonly MemorySave Storage;
            internal readonly PublishedContentTestData.MemoryStorage LocatorStorage;
            internal CandidateApplicationSnapshot Head => Application.QueryView().View.PublishedSnapshot;
            internal ChapterRig(ChapterFixture fixture, MemorySave storage = null, PublishedContentTestData.MemoryStorage locatorStorage = null)
            {
                this.fixture = fixture; architecture = FightMatchDemoArchitecture.Interface;
                Application = architecture.GetSystem<CandidateApplicationSystem>(); Lifecycle = architecture.GetSystem<CandidateLifecycleApplicationSystem>();
                Battle = architecture.GetSystem<CandidateBattleApplicationSystem>(); Session = architecture.GetSystem<PlayerSessionSystem>();
                LocatorStorage = locatorStorage ?? new PublishedContentTestData.MemoryStorage(); var locator = new LocalPlayerProfileLocator(LocatorStorage);
                try
                {
                    var caps = PlayerRosterTestData.RosterCaps(fixture.Publication);
                    if (storage == null)
                    {
                        var profile = Prepared(Session.PrepareNewProfile(fixture.Catalog, Release, Codec()));
                        Storage = new MemorySave(profile.PlayerId); Is(Session.CreateNew(profile, Storage, locator, caps, Budget()));
                    }
                    else
                    {
                        Storage = storage;
                        var observation = locator.Read(Budget()); Assert.IsTrue(observation.IsAccepted, observation.Code);
                        Is(Session.OpenExisting(observation.Observation.ActiveProfile, Storage, fixture.Catalog, caps, Budget()), "Ready");
                    }
                }
                catch { architecture.Deinit(); throw; }
            }
            internal void Enter(int level)
            {
                var definition = fixture.Publication.Definitions.Levels.Single(x => x.LevelId == "fixture:" + level);
                var request = Prepared(Session.PrepareFormationEntry(PlayerRosterTestData.EntryDraft(Head, definition), PlayerRosterTestData.Time(), Codec()));
                Assert.AreEqual(5U, request.Intent.FormatVersion); Is(Lifecycle.Submit(request, Budget()));
            }
            internal PreparedCandidateBattleRequest AttackRequest(string actor = null, int? pair = null)
            {
                var state = Head.Business.ActiveHistory.CurrentRun.CurrentSnapshot;
                var target = pair.HasValue ? state.Enemies[pair.Value] : state.Enemies.First(x => x.Hp.Numerator.Sign > 0);
                var source = fixture.Source.Levels.Single(x => x.Level.LevelId == state.Baseline.Entry.Level.LevelId);
                var route = source.SourceRoutes.Single(x => x.FaceId == target.PairKey.FaceId && x.PairId == target.PairKey.PairId).Cells
                    .Select(c => new FlowPos(c.x - 1, state.Board.Face.Height - c.y)).ToArray();
                var prepared = Battle.Prepare(new CandidateBattleDraft { PlayerId = Head.Business.PlayerId, ExpectedCommitId = Head.Header.CommitId,
                    Context = new PublishedRuleContext(fixture.Publication.Binding), Kind = CandidateApplicationKind.Attack,
                    Attack = new CandidateApplicationAttackInput { AttemptId = state.Baseline.Entry.AttemptId, ExpectedSceneRevision = state.SceneRevision,
                        Actor = actor == null ? state.Members[0].CombatantKey : state.Members.Single(x => x.Member.CharacterId == actor).CombatantKey,
                        Pair = target.PairKey, Route = route, ExpectedPreferenceRevision = Head.Business.Inventory.PreferenceRevision, ItemUseEnabled = false } }, Codec());
                Assert.IsTrue(prepared.IsAccepted, prepared.Code + " " + prepared.Diagnostic?.FieldPath); return prepared.Request;
            }
            internal void FinishPresentation(CandidateBattleCallResult call)
            { if (call.Presentation != null) Battle.ReportPresentationCompleted(call.Presentation.Token); }
            internal void Win()
            {
                var actions = 0;
                while (Head.Business.ActiveHistory.CurrentRun.CurrentSnapshot.Phase != BattlePhase.WonPendingSettlement)
                {
                    Assert.Less(actions++, 32); Assert.AreEqual(BattlePhase.AwaitAction, Head.Business.ActiveHistory.CurrentRun.CurrentSnapshot.Phase);
                    var result = Battle.Submit(AttackRequest(), Budget()); Is(result.Application); FinishPresentation(result);
                }
            }
            internal void Settle() => Is(Lifecycle.Submit(Prepared(Session.PrepareVictory(PlayerRosterTestData.Time(), Codec())), Budget()));
            public void Dispose() => architecture.Deinit();
        }
    }
}
