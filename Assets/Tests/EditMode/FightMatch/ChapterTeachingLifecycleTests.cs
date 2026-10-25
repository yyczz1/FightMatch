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
    public sealed class ChapterTeachingLifecycleTests
    {
        [Test]
        public void NormalUnlockThenGiftLearningAndExplanationCompleteBeforeAnyTeachingAttempt()
        {
            var fixture = new ChapterTeachingFixture();
            MemorySave storage;
            PublishedContentTestData.MemoryStorage locator;
            string learningOperation;
            using (var rig = new ChapterTeachingRig(fixture))
            {
                for (var level = 1; level <= 15; level++)
                {
                    rig.Enter(level);
                    rig.Win();
                    rig.Settle();
                }
                Assert.AreEqual(2, rig.Head.Business.Roster.Characters.Count);
                var before = rig.Head;
                var files = CopyFiles(rig.Storage.Files);
                Assert.AreEqual("TeachingGiftRequired", rig.PrepareEntry(16).Code);
                Assert.AreSame(before, rig.Head);
                SameFiles(files, rig.Storage.Files);
                var gift = Prepared(rig.Session.PrepareTeachingEntry(rig.LevelBinding, rig.Head.Header.CommitId, Codec()));
                rig.Storage.Fault = "marker-after";
                Is(rig.Lifecycle.Submit(gift, Budget()), "CommitUnknown");
                Assert.AreSame(before, rig.Head);
                Is(rig.Lifecycle.Resolve(gift, Budget()));
                Assert.IsNull(rig.Head.Business.ActiveHistory);
                Assert.AreEqual(BigInteger.One, rig.CertificateCount);
                Assert.AreEqual("TeachingLearningRequired", rig.PrepareEntry(16).Code);
                var settledFiles = CopyFiles(rig.Storage.Files);
                Is(rig.Lifecycle.Submit(gift, Budget()));
                SameFiles(settledFiles, rig.Storage.Files);
                var learned = rig.Learn();
                learningOperation = learned.OperationId;
                Assert.AreEqual(BigInteger.Zero, rig.CertificateCount);
                Assert.AreEqual("TeachingExplanationRequired", rig.PrepareEntry(16).Code);
                Assert.IsNull(rig.Head.Business.ActiveHistory);
                storage = rig.Storage;
                locator = rig.LocatorStorage;
            }
            using (var rig = new ChapterTeachingRig(fixture, storage, locator))
            {
                Assert.AreEqual(BigInteger.Zero, rig.CertificateCount);
                Assert.AreEqual("TeachingExplanationRequired", rig.PrepareEntry(16).Code);
                rig.Gift();
                Assert.AreEqual(BigInteger.Zero, rig.CertificateCount, "Returning to teaching does not mint another certificate.");
                rig.Explain();
                var teaching = TakeCore(CandidatePermanentProgression.ReadTeaching(rig.Head.Business, rig.LevelBinding,
                    fixture.Publication.Definitions, Codec()));
                Assert.IsTrue(teaching.IsComplete);
                Assert.AreEqual(learningOperation, teaching.Learning.OperationId);
                rig.Enter(16);
                var entry = rig.Head.Business.ActiveHistory.CurrentRun.Baseline.Entry;
                CollectionAssert.AreEqual(new[] { ChapterRuleDefinitions.StateFeatureId }, entry.RequiredFeatures);
                var warrior = entry.ReadyParticipants.Single(x => x.ClassKind == CharacterClassKind.Warrior);
                Assert.AreEqual("skill:warrior-taunt", warrior.BattleSkills.Single().SkillId);
                Assert.AreEqual("CH1.MageTauntBoss", warrior.BattleSkills.Single().BattleCapabilityId);
                Assert.AreEqual(2, warrior.BattleSkills.Single().CooldownTurns);
                Assert.AreEqual(0, rig.Head.Business.ActiveHistory.CurrentRun.CurrentSnapshot.ChapterState.MemberSkills.Single().CooldownRemaining);
                var permanentCount = rig.Head.Business.Roster.GetPermanentEffects().Count;
                rig.Act();
                Assert.AreEqual(2, rig.Head.Business.ActiveHistory.CurrentRun.CurrentSnapshot.ChapterState.MemberSkills.Single().CooldownRemaining);
                rig.RollbackLast();
                Assert.AreEqual(0, rig.Head.Business.ActiveHistory.CurrentRun.CurrentSnapshot.ChapterState.MemberSkills.Single().CooldownRemaining);
                Assert.AreEqual(permanentCount, rig.Head.Business.Roster.GetPermanentEffects().Count);
                Assert.IsTrue(TakeCore(CandidatePermanentProgression.ReadTeaching(rig.Head.Business, rig.LevelBinding,
                    fixture.Publication.Definitions, Codec())).IsComplete);
                rig.Act();
                var oldAttempt = rig.Head.Business.ActiveHistory.CurrentRun.Baseline.Entry.AttemptId;
                rig.Restart();
                var restarted = rig.Head.Business.ActiveHistory.CurrentRun;
                Assert.AreNotEqual(oldAttempt, restarted.Baseline.Entry.AttemptId);
                Assert.AreEqual(0, restarted.CurrentSnapshot.ChapterState.MemberSkills.Single().CooldownRemaining);
                Assert.AreEqual(0, restarted.CurrentSnapshot.CurrentFaceIndex);
                Assert.AreEqual(permanentCount, rig.Head.Business.Roster.GetPermanentEffects().Count);
                Assert.AreEqual(BigInteger.Zero, rig.CertificateCount);
            }
        }

        [Test]
        public void UnderlevelWarriorCannotLearnOrCreateTeachingAttempt()
        {
            using (var rig = new ChapterTeachingRig(new ChapterTeachingFixture(true, 4)))
            {
                rig.Gift();
                var before = rig.Head;
                var preview = rig.Session.PreviewPermanent(new PlayerPermanentDraft { ExpectedCommitId = before.Header.CommitId,
                    CharacterId = rig.WarriorId, Kind = CandidatePermanentKind.LearnSkill, DefinitionId = "skill:warrior-taunt",
                    StepId = "step:l16-learn" }, Codec());
                Assert.IsFalse(preview.IsAccepted);
                Assert.AreEqual("TeachingLearningRequired", rig.PrepareEntry(16).Code);
                Assert.AreEqual(BigInteger.One, rig.CertificateCount);
                Assert.AreSame(before, rig.Head);
                Assert.IsNull(before.Business.ActiveHistory);
            }
        }
    }

    // Synthetic exact publication for protocol tests. Its easy early levels and initial level are not authoring balance evidence.
    internal sealed class ChapterTeachingFixture
    {
        internal readonly PublishedSource Source;
        internal readonly PublishedContentCatalog Catalog;
        internal readonly ResolvedPublication Publication;
        internal static string LevelId(int number) => "level:ch01-" + number.ToString("00");
        internal ChapterTeachingFixture(bool initiallyOpenBoss = false, int initialWarriorLevel = 5, bool actualAuthoring = false)
        {
            if (actualAuthoring)
            {
                var read = ChapterContentTables.Read("Generated/FightMatch/ChapterOneTrial/r3", "Config/FightMatch/ChapterOneTrial-r3",
                    PublishedContentTestData.Source(), ContentConsumerCapabilities.Current, ChapterContentTestData.Math());
                Assert.IsTrue(read.IsAccepted, read.RejectionCode + " " + read.FieldPath);
                Source = read.Value;
                Source.PackageId = "fixture:actual-r3-lifecycle";
                Source.DraftId = "fixture:actual-r3-lifecycle-draft";
                Source.Chapter.ReleaseSetId = Release;
                var authorMemory = new PublishedContentTestData.MemoryStorage();
                Catalog = new PublishedContentCatalog(authorMemory, ContentConsumerCapabilities.Current);
                var draft = new DemoContentDraft(Source.DraftId);
                for (var revision = BigInteger.One; revision < Source.Revision; revision++) draft.Revise();
                var authorJob = draft.BeginJob();
                var authorPrepared = ChapterContentTestData.Prepare(Source, authorJob);
                Publication = PlayerSessionTestData.Content(Catalog.Publish(authorPrepared, authorPrepared.Validation,
                    PublishedContentTestData.Review(authorPrepared), authorJob, "fixture:actual-r3-publication", PublishedContentTestData.StoreBudget())).Publication;
                authorMemory.Blobs[PublishedContentCatalog.ReleaseSetKey("player", Release)] = PlayerSessionTestData.Content(PublishedContentCodec.EncodeReleaseSet(
                    new ContentReleaseSet { SchemaVersion = 1, Scope = "player", ReleaseSetId = Release, Binding = ContentBindingRecord.From(Publication.Binding),
                        PublicationReceiptSha256 = PublishedContentCodec.Sha256(Publication.ReceiptBytes.ToArray()) }, PublishedContentTestData.StoreBudget()));
                return;
            }
            Source = ChapterContentTestData.Source();
            var template = PublishedContentTestData.Copy(Source);
            Source.PackageId = "fixture:taunt-boss";
            Source.DraftId = "fixture:taunt-boss-draft";
            Source.RequiredCapabilities = ChapterRuleDefinitions.StateCapabilities.ToList();
            Source.Chapter.ReleaseSetId = Release;
            foreach (var growth in Source.Growths)
            {
                growth.BaseStats.MaxHp = Q(1000);
                growth.BaseStats.Attack = Q(20);
                growth.GrowthHp = Q(0);
                growth.GrowthAttack = Q(0);
            }
            Source.NewProfile.Level = initialWarriorLevel;
            Source.NewProfile.Experience = 0;
            Source.NewProfile.Hp = Q(1000);
            Source.Inventory.Items.Add(new CandidateInventoryItemInput { ItemId = "item:profession-certificate", Kind = CandidateInventoryItemKind.OrdinaryMaterial });
            Source.Chapter.Skills.Add(new ChapterSkillInput { SkillId = "skill:warrior-taunt", RecordVersion = 1,
                ClassId = Source.NewProfile.ClassId, CostItemId = "item:profession-certificate", BattleCapabilityId = "CH1.MageTauntBoss",
                MinimumLevel = 5, CostCount = 1, CooldownTurns = 2 });
            Source.Chapter.Teachings.Add(new ChapterTeachingInput { TutorialId = "tutorial:l16-taunt", RecordVersion = 1,
                LevelId = LevelId(16), LevelVersion = 1, ClassId = Source.NewProfile.ClassId, SkillId = "skill:warrior-taunt",
                GiftItemId = "item:profession-certificate", LearningStepId = "step:l16-learn", ExplanationStepId = "step:l16-explain",
                GiftOnceKey = "Level16EntryGift", GiftCount = 1, ExplanationTextKey = "fm.ch1.tutorial.taunt.explanation" });
            Source.Chapter.CharacterUnlocks[0].AfterLevelId = LevelId(8);
            Source.Chapter.FirstClearItems[0].LevelId = LevelId(8);
            Source.Chapter.FirstClearItems.Add(new ChapterFirstClearItemInput { LevelId = LevelId(16), ItemId = "item:profession-certificate", Amount = 1 });
            Source.Levels.Clear();
            Source.Progression.Levels.Clear();
            for (var level = 1; level <= 16; level++)
            {
                var row = PublishedContentTestData.Copy(template).Levels[0];
                row.Level = level == 16 ? BossLevel() : ChapterBattleTestData.StateInput(1, false).Level;
                row.Level.LevelId = LevelId(level);
                row.Level.RecommendedLevel = level >= 8 ? 5 : 1;
                if (level != 16)
                {
                    row.Level.Faces[0].Pairs[0].Enemy.Stats.MaxHp = Q(1);
                    row.Level.Faces[0].Pairs[0].Enemy.Stats.Attack = Q(0);
                }
                row.Reward.LevelId = row.Level.LevelId;
                row.Reward.RewardDefinitionId = "fixture:reward:" + level;
                row.SourceRoutes = new List<DemoContentRouteInput>();
                foreach (var face in row.Level.Faces)
                    foreach (var pair in face.Pairs)
                    {
                        pair.Enemy.OriginalSlot = pair.Enemy.StableOrder;
                        row.SourceRoutes.Add(new DemoContentRouteInput { FaceId = face.FaceId, PairId = pair.PairId,
                            Cells = new List<FlowPos> { AuthorCell(pair.EndpointA, face),
                                AuthorCell(new FlowPos(1, pair.EndpointA.y), face), AuthorCell(pair.EndpointB, face) } });
                    }
                Source.Levels.Add(row);
                var initial = level == 1 || initiallyOpenBoss && level == 16;
                Source.Progression.Levels.Add(new CandidateProgressionLevelInput { LevelId = row.Level.LevelId, LevelVersion = "1",
                    UnlockRuleId = "fixture:unlock:" + level, EntryKind = CandidateProgressionEntryKind.Ordinary,
                    UnlockKind = initial ? CandidateProgressionUnlockKind.InitiallyOpen : CandidateProgressionUnlockKind.AfterWholeLevelClear,
                    UnlockAfterLevelId = initial ? null : LevelId(level - 1), RequiredFeatures = new List<string>() });
            }
            Source.NewProfile.OpenLevels = initiallyOpenBoss ? new List<string> { LevelId(1), LevelId(16) } : new List<string> { LevelId(1) };
            var memory = new PublishedContentTestData.MemoryStorage();
            Catalog = new PublishedContentCatalog(memory, ContentConsumerCapabilities.Current);
            var job = new DemoContentDraft(Source.DraftId).BeginJob();
            var prepared = ChapterContentTestData.Prepare(Source, job);
            Publication = PlayerSessionTestData.Content(Catalog.Publish(prepared, prepared.Validation, PublishedContentTestData.Review(prepared), job,
                "fixture:taunt-boss-publication", PublishedContentTestData.StoreBudget())).Publication;
            memory.Blobs[PublishedContentCatalog.ReleaseSetKey("player", Release)] = PlayerSessionTestData.Content(PublishedContentCodec.EncodeReleaseSet(
                new ContentReleaseSet { SchemaVersion = 1, Scope = "player", ReleaseSetId = Release,
                    Binding = ContentBindingRecord.From(Publication.Binding), PublicationReceiptSha256 = PublishedContentCodec.Sha256(Publication.ReceiptBytes.ToArray()) },
                PublishedContentTestData.StoreBudget()));
        }
        private static ExactRational Q(int n, int d = 1) => ChapterBattleTestData.R(n, d);
        private static FlowPos AuthorCell(FlowPos cell, FaceInput face) => new FlowPos(cell.x + 1, face.Height - cell.y);
        private static LevelInput BossLevel()
        {
            var input = ChapterBossBattleTests.WindInput();
            foreach (var pair in input.Level.Faces[0].Pairs)
                pair.Enemy.Stats.MaxHp = Q(pair.Enemy.StableOrder == 0 ? 120 : 20);
            ChapterBattleTestData.AddStateFace(input);
            var second = input.Level.Faces[1].Pairs[0].Enemy;
            second.EnemyInstanceKey = "boss:second";
            second.Stats.MaxHp = Q(40);
            var heavy = ChapterBattleTestData.Strike(attack: 12);
            heavy.DamageCoefficient = Q(2);
            heavy.NextActionVulnerability = Q(3, 2);
            second.IntentCycle = new List<EnemyIntentInput> { new EnemyIntentInput { Kind = EnemyIntentKind.Charge }, heavy,
                new EnemyIntentInput { Kind = EnemyIntentKind.Rest } };
            return input.Level;
        }
    }

    internal sealed class ChapterTeachingRig : IDisposable
    {
        private readonly IArchitecture architecture;
        internal readonly ChapterTeachingFixture Fixture;
        internal readonly CandidateApplicationSystem Application;
        internal readonly CandidateLifecycleApplicationSystem Lifecycle;
        internal readonly CandidateBattleApplicationSystem Battle;
        internal readonly PlayerSessionSystem Session;
        internal readonly MemorySave Storage;
        internal readonly PublishedContentTestData.MemoryStorage LocatorStorage;
        internal CandidateApplicationSnapshot Head => Application.QueryView().View.PublishedSnapshot;
        internal string WarriorId => Head.Business.Roster.Characters.Single(x => x.Definition.ClassKind == CharacterClassKind.Warrior).CharacterId;
        internal string MageId => Head.Business.Roster.Characters.Single(x => x.Definition.ClassKind == CharacterClassKind.Mage).CharacterId;
        internal DefinitionBinding LevelBinding => TakeCore(DefinitionBinding.Prepare(Fixture.Publication.Binding, ChapterTeachingFixture.LevelId(16), 1, Codec()));
        internal BigInteger CertificateCount => Head.Business.Inventory.Holdings.Where(x => x.ItemId == "item:profession-certificate").Aggregate(BigInteger.Zero, (sum, x) => sum + x.T);
        internal ChapterTeachingRig(ChapterTeachingFixture fixture, MemorySave storage = null, PublishedContentTestData.MemoryStorage locatorStorage = null)
        {
            Fixture = fixture;
            architecture = FightMatchDemoArchitecture.Interface;
            Application = architecture.GetSystem<CandidateApplicationSystem>();
            Lifecycle = architecture.GetSystem<CandidateLifecycleApplicationSystem>();
            Battle = architecture.GetSystem<CandidateBattleApplicationSystem>();
            Session = architecture.GetSystem<PlayerSessionSystem>();
            LocatorStorage = locatorStorage ?? new PublishedContentTestData.MemoryStorage();
            var locator = new LocalPlayerProfileLocator(LocatorStorage);
            try
            {
                var caps = PlayerRosterTestData.RosterCaps(fixture.Publication);
                if (storage == null)
                {
                    var profile = Prepared(Session.PrepareNewProfile(fixture.Catalog, Release, Codec()));
                    Storage = new MemorySave(profile.PlayerId);
                    Is(Session.CreateNew(profile, Storage, locator, caps, Budget()));
                }
                else
                {
                    Storage = storage;
                    var observed = locator.Read(Budget());
                    Assert.IsTrue(observed.IsAccepted, observed.Code);
                    Is(Session.OpenExisting(observed.Observation.ActiveProfile, Storage, fixture.Catalog, caps, Budget()), "Ready");
                }
            }
            catch { architecture.Deinit(); throw; }
        }
        internal CandidateLifecyclePrepareResult PrepareEntry(int level) => Session.PrepareFormationEntry(PlayerRosterTestData.EntryDraft(Head,
            Fixture.Publication.Definitions.Levels.Single(x => x.LevelId == ChapterTeachingFixture.LevelId(level))), PlayerRosterTestData.Time(), Codec());
        internal void Enter(int level) => Is(Lifecycle.Submit(Prepared(PrepareEntry(level)), Budget()));
        internal PreparedCandidateLifecycleRequest Gift()
        {
            var request = Prepared(Session.PrepareTeachingEntry(LevelBinding, Head.Header.CommitId, Codec()));
            Is(Lifecycle.Submit(request, Budget()));
            return request;
        }
        private PreparedCandidateLifecycleRequest Permanent(CandidatePermanentKind kind, string definition, string step, string character = null)
        {
            var preview = Session.PreviewPermanent(new PlayerPermanentDraft { ExpectedCommitId = Head.Header.CommitId,
                CharacterId = character, Kind = kind, DefinitionId = definition, StepId = step }, Codec());
            Assert.IsTrue(preview.IsAccepted, preview.Code + " " + preview.Diagnostic?.FieldPath);
            var request = Prepared(Session.ConfirmPermanent(preview.Preview, Codec()));
            Is(Lifecycle.Submit(request, Budget()));
            return request;
        }
        internal PreparedCandidateLifecycleRequest Learn() => Permanent(CandidatePermanentKind.LearnSkill, "skill:warrior-taunt", "step:l16-learn", WarriorId);
        internal PreparedCandidateLifecycleRequest Explain() => Permanent(CandidatePermanentKind.ConfirmTeachingExplanation, "tutorial:l16-taunt", "step:l16-explain");
        internal void CompleteTeaching() { Gift(); Learn(); Explain(); }
        internal void AddMageToFormation() => Is(Lifecycle.Submit(Prepared(Session.PrepareFormation(new PlayerFormationDraft {
            ExpectedCommitId = Head.Header.CommitId, ExpectedFormationRevision = Head.Business.Roster.FormationRevision,
            Slots = new[] { WarriorId, MageId, null } }, Codec())), Budget()));
        internal PreparedCandidateBattleRequest AttackRequest(string actor = null, int? pair = null)
        {
            var state = Head.Business.ActiveHistory.CurrentRun.CurrentSnapshot;
            var target = pair.HasValue ? state.Enemies[pair.Value] : state.Enemies.First(x => x.Hp.Numerator.Sign > 0);
            var row = Fixture.Source.Levels.Single(x => x.Level.LevelId == state.Baseline.Entry.Level.LevelId);
            var route = row.SourceRoutes.Single(x => x.FaceId == target.PairKey.FaceId && x.PairId == target.PairKey.PairId).Cells
                .Select(x => new FlowPos(x.x - 1, state.Board.Face.Height - x.y)).ToArray();
            var prepared = Battle.Prepare(new CandidateBattleDraft { PlayerId = Head.Business.PlayerId,
                ExpectedCommitId = Head.Header.CommitId, Context = new PublishedRuleContext(Fixture.Publication.Binding), Kind = CandidateApplicationKind.Attack,
                Attack = new CandidateApplicationAttackInput { AttemptId = state.Baseline.Entry.AttemptId, ExpectedSceneRevision = state.SceneRevision,
                    Actor = state.Members.Single(x => x.Member.CharacterId == (actor ?? WarriorId)).CombatantKey, Pair = target.PairKey,
                    Route = route,
                    ExpectedPreferenceRevision = Head.Business.Inventory.PreferenceRevision, ItemUseEnabled = false } }, Codec());
            Assert.IsTrue(prepared.IsAccepted, prepared.Code + " " + prepared.Diagnostic?.FieldPath);
            return prepared.Request;
        }
        internal void FinishPresentation(CandidateBattleCallResult call)
        { if (call.Presentation != null) Battle.ReportPresentationCompleted(call.Presentation.Token); }
        internal void Act(string actor = null, int? pair = null)
        {
            var call = Battle.Submit(AttackRequest(actor, pair), Budget());
            Is(call.Application);
            FinishPresentation(call);
        }
        internal void Win(bool useParty = false)
        {
            var actions = 0;
            while (Head.Business.ActiveHistory.CurrentRun.CurrentSnapshot.Phase != BattlePhase.WonPendingSettlement)
            {
                Assert.Less(actions++, 64);
                var state = Head.Business.ActiveHistory.CurrentRun.CurrentSnapshot;
                Assert.AreEqual(BattlePhase.AwaitAction, state.Phase,
                    state.Baseline.Entry.Level.LevelId + " face " + state.CurrentFaceIndex + " action " + state.EffectiveActionsCompleted + " " +
                    string.Join(", ", state.Members.Select(x => x.Member.ClassId + " Lv" + x.Member.Level + " HP " + x.Hp.Numerator + "/" + x.Hp.Denominator)));
                var target = state.Enemies.First(x => x.Hp.Numerator.Sign > 0);
                var actor = state.Members.FirstOrDefault(x => x.Member.ClassKind == CharacterClassKind.Warrior && x.Hp.Numerator.Sign > 0);
                if (useParty && target.Enemy.Stats.PhysicalDefense.Compare(target.Enemy.Stats.MagicDefense, Codec().Math) > 0)
                    actor = state.Members.FirstOrDefault(x => x.Member.ClassKind == CharacterClassKind.Mage && x.Hp.Numerator.Sign > 0) ?? actor;
                actor = actor ?? state.Members.First(x => x.Hp.Numerator.Sign > 0);
                Act(actor.Member.CharacterId);
            }
        }
        internal void RollbackLast()
        {
            var history = Head.Business.ActiveHistory;
            var last = history.Archive.Last();
            var preview = Battle.PreviewHistoryRollback(last.HistoryAnchorId, Head.Header.CommitId, Codec());
            Assert.IsNotNull(preview.Range);
            var request = Battle.Prepare(new CandidateBattleDraft { PlayerId = Head.Business.PlayerId,
                ExpectedCommitId = Head.Header.CommitId, Context = new PublishedRuleContext(Fixture.Publication.Binding),
                Kind = CandidateApplicationKind.Rollback, Rollback = new CandidateApplicationRollbackInput {
                    AttemptId = history.CurrentRun.Baseline.Entry.AttemptId, ExpectedSceneRevision = history.CurrentRun.CurrentSnapshot.SceneRevision,
                    HistoryAnchorId = last.HistoryAnchorId, TargetOperationId = last.OperationId,
                    ConfirmedRemovedOperationIds = preview.Range.Entries.Select(x => x.OperationId).ToArray() } }, Codec());
            Assert.IsTrue(request.IsAccepted, request.Code);
            var call = Battle.Submit(request.Request, Budget());
            Is(call.Application);
            FinishPresentation(call);
        }
        internal void Restart()
        {
            var state = Head.Business.ActiveHistory.CurrentRun.CurrentSnapshot;
            var entry = state.Baseline.Entry;
            var request = Prepared(Session.PrepareLifecycle(new PlayerLifecycleDraft { ExpectedCommitId = Head.Header.CommitId,
                Kind = CandidateApplicationKind.RestartAttempt, RestartAttempt = new CandidateApplicationEndInput {
                    AttemptId = entry.AttemptId, ChallengeId = entry.ChallengeId, EntryBaselineId = entry.EntryBaselineId,
                    ExpectedSceneRevision = state.SceneRevision }, EndTimeSample = PlayerRosterTestData.Time() }, Codec()));
            Is(Lifecycle.Submit(request, Budget()));
        }
        internal void TryUseCardsToNextLevel(string itemId = "item:xp-card-50")
        {
            var warrior = Head.Business.Roster.Find(WarriorId);
            var preview = Session.PreviewPermanent(new PlayerPermanentDraft { ExpectedCommitId = Head.Header.CommitId,
                Kind = CandidatePermanentKind.UseExperienceCards, CharacterId = WarriorId, DefinitionId = itemId, Quantity = warrior.Level + 1 }, Codec());
            if (preview.IsAccepted) Is(Lifecycle.Submit(Prepared(Session.ConfirmPermanent(preview.Preview, Codec())), Budget()));
        }
        internal void Settle() => Is(Lifecycle.Submit(Prepared(Session.PrepareVictory(PlayerRosterTestData.Time(), Codec())), Budget()));
        public void Dispose() => architecture.Deinit();
    }
}
