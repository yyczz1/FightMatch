using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using FightMatch.Application;
using FightMatch.Platform;
using FlowPuzzle.Core;
using NUnit.Framework;
using static FightMatch.Core.Tests.PlayerSessionTestData;

namespace FightMatch.Core.Tests
{
    // Pure exact definitions and the original M07/M02/M12 paths; no M10 publication or physical storage.
    internal sealed class CandidatePermanentTestData : IDisposable
    {
        internal readonly ContentBinding Binding = SharedRuleFixture.Binding();
        internal readonly CandidateLifecycleContentInput Content;
        internal readonly PublishedRuleDefinitions Definitions;
        internal readonly PublishedSaveContext Closure;
        internal readonly MemorySave Storage = new MemorySave("permanent:player");
        internal LocalSaveStore Store;
        internal CandidateApplicationSnapshot Head;
        internal SaveEnvelope Envelope;
        internal CandidateApplicationCandidate Candidate;
        internal PreparedCandidateLifecycleRequest Last;
        internal int IdCalls, EntropyCalls;
        private int serial;
        internal static ExactRational R(int n, int d = 1) => ExactRational.Create(n, d, Codec().Math);
        internal CandidateCharacterState W => Head.Business.Roster.Find("W");
        internal CandidateCharacterState M => Head.Business.Roster.Find("M");

        internal CandidatePermanentTestData(int cards = 3, int certificates = 1, int q = 50, bool farm = true,
            int warriorLevel = 4, int warriorXp = 157, int mageLevel = 5, int mageXp = 92, int warriorHp = 100)
        {
            var context = new PublishedRuleContext(Binding);
            var growths = new[] { Growth(context, "farmer", CharacterClassKind.Warrior),
                Growth(context, "mage", CharacterClassKind.Mage), Growth(context, "warrior", CharacterClassKind.Warrior, warriorHp) };
            var rawItems = new List<CandidateInventoryItemInput>();
            foreach (var id in new[] { "card", "certificate", "ore", "wood" })
                rawItems.Add(new CandidateInventoryItemInput { ItemId = id, Kind = CandidateInventoryItemKind.OrdinaryMaterial });
            rawItems.Add(new CandidateInventoryItemInput { ItemId = "weapon", Kind = CandidateInventoryItemKind.OrdinaryTactical, EquipClassId = "warrior" });
            var inventory = CandidateInventory.PrepareDefinition(new CandidateInventoryDefinitionInput { Context = context, Items = rawItems }, Codec().Math);
            Assert.IsTrue(inventory.IsAccepted, inventory.FieldPath);
            var levels = new List<PreparedLevel>();
            var progressionRows = new List<CandidateProgressionLevelInput>();
            var rewards = new List<CandidateRewardDefinition>();
            foreach (var id in new[] { "test:materials", "test:L1", "test:L16" })
            {
                var raw = SharedRuleFixture.RawEntry();
                raw.Level.LevelId = id;
                var level = new BattleEntryPreparer().PrepareCandidate(raw, Codec().Math);
                Assert.IsTrue(level.IsAccepted, level.FieldPath);
                levels.Add(level.Entry.Level);
                progressionRows.Add(new CandidateProgressionLevelInput { LevelId = id, LevelVersion = "1", UnlockRuleId = "initial:" + id,
                    EntryKind = CandidateProgressionEntryKind.Ordinary, UnlockKind = CandidateProgressionUnlockKind.InitiallyOpen,
                    UnlockAfterLevelId = null, RequiredFeatures = new List<string>() });
                var materials = new List<CandidateRewardMaterialInput>
                {
                    new CandidateRewardMaterialInput { ItemId = "ore", Amount = 120 },
                    new CandidateRewardMaterialInput { ItemId = "wood", Amount = 120 }
                };
                if (cards > 0) materials.Add(new CandidateRewardMaterialInput { ItemId = "card", Amount = cards });
                if (certificates > 0) materials.Add(new CandidateRewardMaterialInput { ItemId = "certificate", Amount = certificates });
                var reward = CandidateBaseRewards.PrepareDefinition(new CandidateRewardDefinitionInput
                {
                    Context = context, RewardDefinitionId = "reward:" + id, Version = "1", LevelId = id, LevelVersion = "1",
                    BaseExperience = 20, DamageWeight = R(1), TakenWeight = R(1, 4), CurveBase = R(1, 2), CurveLog = R(1, 2),
                    ReferenceHpDivisor = R(2), LevelPenaltyBase = R(3, 4), OverlevelGrace = 1,
                    ZeroContributionPolicy = CandidateZeroContributionPolicy.CurveIntercept,
                    DropMode = CandidateRewardDropMode.FixedOrdinaryMaterials, RequiredFeatures = new List<string>(), Materials = id == "test:materials" ? materials : new List<CandidateRewardMaterialInput>()
                }, Codec().Math);
                Assert.IsTrue(reward.IsAccepted, reward.FieldPath);
                rewards.Add(reward.Definition);
            }
            var progression = CandidateProgression.PrepareDefinition(new CandidateProgressionDefinitionInput { Context = context, Levels = progressionRows }, Codec().Math);
            Assert.IsTrue(progression.IsAccepted, progression.FieldPath);
            Content = new CandidateLifecycleContentInput { Growths = growths, Inventory = inventory.Definition, Progression = progression.Definition,
                Levels = levels, Rewards = rewards, CritCoefficients = new[] { new CandidateCritCoefficient(R(1, 5), R(1, 1000)) } };
            var definitions = TakeCore(PublishedRuleDefinitions.Prepare(Binding, growths, Content.Inventory, Content.Progression, levels, rewards, Codec()));
            var permanent = TakeCore(CandidatePermanentDefinitions.Prepare(Binding, DefinitionInputs(q), Codec()));
            Definitions = TakeCore(PublishedRuleDefinitions.PreparePermanent(definitions, permanent, Codec()));
            Content.SetPermanentDefinitions(permanent);
            Closure = TakeCore(PublishedSaveContext.Prepare(new[] { Definitions }, Codec()));
            Store = OpenStore(SaveOpenMode.CreateNew);
            var input = Input(CandidateApplicationKind.InitializeProfile, 4);
            input.RosterInitialize = new CandidateRosterInitializeInput
            {
                Characters = new[] {
                    new CandidateApplicationInitializeInput { CharacterId = "F", ClassId = "farmer", InitialLevel = 1, InitialExperience = 0, OriginalSlot = 0 },
                    new CandidateApplicationInitializeInput { CharacterId = "M", ClassId = "mage", InitialLevel = mageLevel, InitialExperience = mageXp, OriginalSlot = 1 },
                    new CandidateApplicationInitializeInput { CharacterId = "W", ClassId = "warrior", InitialLevel = warriorLevel, InitialExperience = warriorXp, OriginalSlot = 2 } },
                Slots = new[] { "F", null, null }
            };
            Commit(Freeze(input));
            if (farm) Farm();
        }

        internal List<CandidatePermanentDefinitionInput> DefinitionInputs(int q = 50)
        {
            var teaching = TakeCore(DefinitionBinding.Prepare(Binding, "test:L16", 1, Codec()));
            return new List<CandidatePermanentDefinitionInput>
            {
                new CandidatePermanentDefinitionInput { Kind = CandidatePermanentDefinitionKind.Card, Id = "card", RecordVersion = 1, UnitExperience = q },
                new CandidatePermanentDefinitionInput { Kind = CandidatePermanentDefinitionKind.Skill, Id = "taunt", RecordVersion = 1,
                    ClassId = "warrior", ItemId = "certificate", BattleCapabilityId = "not-yet-supported:taunt" },
                new CandidatePermanentDefinitionInput { Kind = CandidatePermanentDefinitionKind.Skill, Id = "spell", RecordVersion = 1,
                    ClassId = "mage", ItemId = "certificate", BattleCapabilityId = "not-yet-supported:spell" },
                new CandidatePermanentDefinitionInput { Kind = CandidatePermanentDefinitionKind.Recipe, Id = "weapons", RecordVersion = 1,
                    Inputs = new[] { Q("ore", 2), Q("ore", 1), Q("wood", 1) }, Outputs = new[] { Q("weapon", 6) } },
                new CandidatePermanentDefinitionInput { Kind = CandidatePermanentDefinitionKind.Recipe, Id = "cards", RecordVersion = 1,
                    Inputs = new[] { Q("weapon", 2) }, Outputs = new[] { Q("card", 1) } },
                new CandidatePermanentDefinitionInput { Kind = CandidatePermanentDefinitionKind.Recipe, Id = "mixedcards", RecordVersion = 1,
                    Inputs = new[] { Q("ore", 3), Q("wood", 1) }, Outputs = new[] { Q("card", 2) } },
                new CandidatePermanentDefinitionInput { Kind = CandidatePermanentDefinitionKind.Teaching, Id = "tutorial", RecordVersion = 1,
                    ClassId = "warrior", ItemId = "certificate", SkillId = "taunt", Level = teaching, LearningStepId = "learn", ExplanationStepId = "explain" }
            };
        }

        internal static CandidateInventoryQuantityInput Q(string item, int count) => new CandidateInventoryQuantityInput { ItemId = item, Quantity = count };
        internal static GrowthDefinitionInput RawGrowth(RuleContext context, string classId, CharacterClassKind kind, int hp = 100)
        {
            var raw = BusinessSaveScenario.Growth(SharedRuleFixture.Candidate(), hp);
            raw.Context = context;
            raw.ClassId = classId;
            raw.ClassKind = kind;
            raw.XpBase = 60;
            raw.XpLinear = 20;
            raw.XpQuadratic = 5;
            raw.CritStep = R(0);
            raw.CritCap = raw.CritBase;
            if (kind == CharacterClassKind.Mage)
            {
                raw.CritBase = null;
                raw.CritStep = null;
                raw.CritCap = null;
                raw.CritMultiplier = null;
            }
            return raw;
        }
        internal static CandidateGrowthDefinition Growth(RuleContext context, string classId, CharacterClassKind kind, int hp = 100)
        {
            var result = CandidateCharacterGrowth.PreparePermanentDefinition(RawGrowth(context, classId, kind, hp), Codec().Math);
            Assert.IsTrue(result.IsAccepted, result.FieldPath);
            return result.Definition;
        }
        internal LocalSaveStore OpenStore(SaveOpenMode mode)
        {
            var opened = LocalSaveStore.Open(Storage, "permanent:player", SavePurpose.PlayerSave, mode, SaveFaultModel.EditorProcessCrash, Budget());
            Assert.IsTrue(opened.IsAccepted, opened.Code);
            return opened.Value;
        }
        internal CandidateApplicationIntentInput Input(CandidateApplicationKind kind, uint version = 3) =>
            new CandidateApplicationIntentInput { PlayerId = "permanent:player", OperationId = "permanent:op:" + ++serial,
                ExpectedCommitId = Head?.Header.CommitId, Kind = kind, FormatVersion = version, Context = new PublishedRuleContext(Binding) };
        internal PreparedCandidateLifecycleRequest Freeze(CandidateApplicationIntentInput input) =>
            Prepared(CandidateLifecyclePreparation.Roster(input, Content, Codec()));
        internal CandidateApplicationBuildResult Build(PreparedCandidateLifecycleRequest request, SaveCodecBudget budget = null) =>
            CandidateLifecycleApplicationSystem.Build(Head, request, budget ?? Codec(), nextId: () =>
            {
                IdCalls++;
                return "permanent:battle:" + ++serial;
            }, entropy: bytes =>
            {
                EntropyCalls++;
                for (var i = 0; i < bytes.Length; i += 16) { bytes[i] = 42; bytes[i + 8] = 54; }
            });
        internal CandidatePermanentQuote Preview(CandidatePermanentDraft draft) =>
            TakeCore(CandidatePermanentProtocol.Preview(Head, Definitions, draft, Codec()));
        internal PreparedCandidateLifecycleRequest Request(CandidatePermanentDraft draft)
        {
            var quote = Preview(draft);
            var input = Input(CandidateApplicationKind.PermanentRequest, 4);
            input.SetPermanent(quote);
            return Freeze(input);
        }
        internal PreparedCandidateLifecycleRequest Apply(CandidatePermanentDraft draft)
        {
            var request = Request(draft);
            Commit(request);
            return request;
        }
        internal static CandidatePermanentDraft Card(string character, int target) =>
            new CandidatePermanentDraft { Kind = CandidatePermanentKind.UseExperienceCards, CharacterId = character, DefinitionId = "card", Quantity = target };
        internal CandidatePermanentDraft Learn(string character) => new CandidatePermanentDraft { Kind = CandidatePermanentKind.LearnSkill,
            CharacterId = character, DefinitionId = character == "W" ? "taunt" : "spell",
            StepId = character == "W" && Head.Business.Progression.GetPermanentEffects().Any(x => x.Quote.Kind == CandidatePermanentKind.BeginTeachingGift) ? "learn" : null };
        internal PreparedCandidateLifecycleRequest Gift() => Apply(new CandidatePermanentDraft { Kind = CandidatePermanentKind.BeginTeachingGift, DefinitionId = "tutorial" });
        internal PreparedCandidateLifecycleRequest Explain() => Apply(new CandidatePermanentDraft {
            Kind = CandidatePermanentKind.ConfirmTeachingExplanation, DefinitionId = "tutorial", StepId = "explain" });
        internal SaveCommitTicket Prepare(PreparedCandidateLifecycleRequest request)
        {
            var built = Build(request);
            Assert.IsTrue(built.IsAccepted, built.Diagnostic?.Code + " " + built.Diagnostic?.FieldPath);
            return Prepare(request.Intent, built.NextBusiness, built.CopyResult(), built.NextSettlementOperationId);
        }
        private SaveCommitTicket Prepare(PreparedCandidateApplicationIntent intent, CandidateBusinessSnapshot business,
            CandidateApplicationResultInput result, string reserved)
        {
            Candidate = TakeCore(CandidateApplicationProtocol.Propose(Head, business, intent, result, reserved, Codec()));
            Assert.IsFalse(Candidate.CommitEligible);
            var prepared = Store.Prepare(Head?.Descriptor, new[] { intent.OperationId }, metadata =>
                CandidateApplicationSaveCodec.EncodePublished(Candidate, new CandidateBusinessSaveHeader(metadata.SaveGeneration,
                    metadata.CommitId, metadata.ParentCommitId, metadata.CommitIndex), Closure, Codec()), Budget());
            Assert.IsTrue(prepared.IsAccepted, prepared.Code + " " + prepared.FieldPath);
            return prepared.Value;
        }
        internal void Commit(PreparedCandidateLifecycleRequest request)
        {
            Last = request;
            var ticket = Prepare(request);
            var written = Store.Write(ticket, Budget());
            Assert.IsTrue(written.IsAccepted, written.Code);
            Load(written.Value.Descriptor);
        }
        internal void Load(SnapshotDescriptor descriptor)
        {
            using (var stream = Storage.OpenRead("c-" + descriptor.CommitId + ".snapshot"))
                Envelope = TakeCore(SaveEnvelopeCodec.Read(stream, descriptor, Codec()));
            Head = TakeCore(CandidateApplicationSaveCodec.DecodePublished(Envelope, Closure, Codec()));
        }
        internal void Reopen()
        {
            Store.Dispose();
            Store = OpenStore(SaveOpenMode.Existing);
            var inspection = Store.Inspect(Budget());
            Assert.IsTrue(inspection.IsAccepted, inspection.Code);
            Load(inspection.Value.Current.Descriptor);
        }
        internal void Formation(params string[] slots)
        {
            var input = Input(CandidateApplicationKind.SetFormation);
            input.SetFormation = new CandidateFormationInput { ExpectedFormationRevision = Head.Business.Roster.FormationRevision, Slots = slots };
            Commit(Freeze(input));
        }
        internal PreparedCandidateLifecycleRequest Entry(string level = "test:L1")
        {
            var draft = PlayerRosterTestData.EntryDraft(Head, Content.Levels.Single(x => x.LevelId == level));
            var input = Input(CandidateApplicationKind.EnterFormation);
            input.EnterFormation = new CandidateFormationEntryInput { LevelId = draft.LevelId, LevelVersion = draft.LevelVersion,
                ExpectedFormationRevision = draft.ExpectedFormationRevision, Slots = Head.Business.Roster.Formation,
                SelectedCharacters = draft.SelectedCharacters, ExpectedInventoryRevision = draft.ExpectedInventoryRevision,
                ExpectedProgressionRevision = draft.ExpectedProgressionRevision, TimeSample = PlayerRosterTestData.Time() };
            return Freeze(input);
        }
        internal void Farm(string level = "test:materials")
        {
            Commit(Entry(level));
            Attack(0);
            Attack(1);
            Commit(Prepared(CandidateLifecyclePreparation.Victory(Head, Content, PlayerRosterTestData.Time(), Codec(), true)));
        }
        internal void Attack(int pair)
        {
            var b = Head.Business;
            var history = b.ActiveHistory;
            var state = history.CurrentRun.CurrentSnapshot;
            var actor = state.Members[0].CombatantKey;
            var input = Input(CandidateApplicationKind.Attack, 2);
            var enabled = b.Inventory.FindLoadout(actor.CharacterId).Enabled ?? false;
            input.Attack = new CandidateApplicationAttackInput { AttemptId = actor.AttemptId, ExpectedSceneRevision = state.SceneRevision,
                Actor = actor, Pair = state.Enemies[pair].PairKey, Route = new[] { new FlowPos(0, pair * 2), new FlowPos(1, pair * 2),
                    new FlowPos(2, pair * 2), new FlowPos(3, pair * 2) }, ExpectedPreferenceRevision = b.Inventory.PreferenceRevision, ItemUseEnabled = enabled };
            var intent = TakeCore(CandidateApplicationProtocol.PrepareIntent(input, Codec()));
            var result = CandidateBattleOperations.EvaluateAttack(history.CurrentRun, new CandidateAttackRequest {
                PlayerId = b.PlayerId, AttemptId = actor.AttemptId, OperationId = input.OperationId, ExpectedSceneRevision = state.SceneRevision,
                Actor = actor, Pair = input.Attack.Pair, Route = input.Attack.Route.ToList() },
                new CandidateBattleConditions { PreferenceRevision = b.Inventory.PreferenceRevision, ItemUseEnabled = enabled },
                100 + serial, new RandomSamplingBudget(Codec().Math));
            Assert.IsTrue(result.IsAccepted, result.RejectionCode + " " + result.FieldPath);
            var appended = CandidateHistoryOperations.Append(history, result.NextRun, "anchor:" + input.OperationId, Codec().Math);
            Assert.IsTrue(appended.IsAccepted, appended.FieldPath);
            var business = TakeCore(CandidateBusinessSaveCodec.Prepare(new CandidateBusinessInput(b.PlayerId, b.Roster, b.Inventory, b.Progression,
                b.Rewards, appended.Next, b.RetainedRuns, b.RetainedRollbacks, b.Format), SavePurpose.PlayerSave, Codec()));
            var ticket = Prepare(intent, business, new CandidateApplicationResultInput { HistoryAnchorId = "anchor:" + input.OperationId },
                result.NextRun.FinalReport == null ? null : "settlement:" + input.OperationId);
            var written = Store.Write(ticket, Budget());
            Assert.IsTrue(written.IsAccepted, written.Code);
            Load(written.Value.Descriptor);
        }

        internal CandidateApplicationSnapshot WithBusiness(CandidateBusinessSnapshot business)
        {
            return new CandidateApplicationSnapshot(business, Head.Records.ToList(), Head.Continuation, Head.Header, Head.Descriptor);
        }

        internal CandidateApplicationSnapshot ExistingOwnerFixture(string item, int count)
        {
            // Reference values exercise only the existing-held owner kernel, without M10/M12 grant admission.
            var original = new CandidateOriginalGrantRef(CandidatePermanentSourceKind.ExistingAdvertisementHeld,
                Head.Business.PlayerId, Binding, null, null, "existing:grant", "permanent:" + item, "existing:fact", "existing:use");
            var inclusion = new CandidateExistingInclusionRef("existing:checkpoint", "existing:branch", "existing:source-commit",
                7, original, 0, 0, count, CandidateInclusionDisposition.SelectedHolding, null);
            var source = new CandidatePermanentSourceLine(original, 0, item, count, 12, "existing:operation",
                "existing:original-commit", "existing:branch", inclusion);
            var old = Head.Business.Inventory;
            var totals = old.Holdings.Select(x => new CandidateInventoryHolding(x.ItemId, x.T + (x.ItemId == item ? count : 0))).ToArray();
            var ledger = new CandidatePermanentInventoryLedger(new[] { source }, old.GetPermanentLedger().Effects);
            var inventory = new CandidateInventoryState(old.Definition, old.PlayerId, totals, old.Loadouts, old.ActiveCarry,
                old.OrdinaryGrants, old.Ends, old.StateRevision, old.PreferenceRevision, true, ledger);
            var business = CandidatePermanentProtocol.Copy(Head.Business, Head.Business.Roster, inventory,
                Head.Business.Progression, Head.Business.Format, Codec());
            return WithBusiness(business);
        }

        internal static CandidatePermanentQuote Requote(CandidatePermanentQuote q,
            IReadOnlyList<CandidatePermanentPortion> inputs = null, string learned = null, BigInteger? fixedXp = null,
            BigInteger? version = null, BigInteger? generation = null, bool omitTeaching = false)
        {
            var draft = new CandidatePermanentDraft
            {
                Kind = q.Kind, CharacterId = q.CharacterId, DefinitionId = q.DefinitionId,
                Quantity = q.Quantity, Enabled = q.Enabled, StepId = omitTeaching ? null : q.StepId,
                PreferenceRevision = q.Draft.PreferenceRevision, SelectedInputs = inputs ?? q.Inputs
            };
            return new CandidatePermanentQuote(draft, q.PlayerId, q.ClassId, q.Binding,
                generation ?? q.SourceGeneration, q.SourceDescriptorLength, q.SourceDescriptorSha256, version ?? q.DefinitionVersion,
                q.CharacterRevision, q.InventoryRevision, q.PreferenceRevision, q.ProgressionRevision, q.UnitExperience,
                fixedXp ?? q.FixedExperience, q.BeforeLevel, q.BeforeExperience, q.FinalLevel, q.FinalExperience,
                omitTeaching ? null : q.TeachingLevel, inputs ?? q.Inputs, q.Costs, q.Outputs, learned ?? q.OriginalLearningOperation);
        }

        internal PreparedCandidateLifecycleRequest End(bool restart)
        {
            var state = Head.Business.ActiveHistory.CurrentRun.CurrentSnapshot;
            var entry = state.Baseline.Entry;
            var input = new CandidateApplicationEndInput { AttemptId = entry.AttemptId, ChallengeId = entry.ChallengeId,
                EntryBaselineId = entry.EntryBaselineId, ExpectedSceneRevision = state.SceneRevision };
            return Prepared(CandidateLifecyclePreparation.Prepare(new CandidateLifecycleDraft {
                PlayerId = Head.Business.PlayerId, ExpectedCommitId = Head.Header.CommitId, Content = Content,
                Kind = restart ? CandidateApplicationKind.RestartAttempt : CandidateApplicationKind.ExitAttempt,
                RestartAttempt = restart ? input : null, ExitAttempt = restart ? null : input,
                EndTimeSample = restart ? null : PlayerRosterTestData.Time() }, Codec(), true));
        }

        internal void RollbackLastAttack()
        {
            var old = Head.Business;
            var history = old.ActiveHistory;
            var last = Head.Records.Last(x => x.Intent.Kind == CandidateApplicationKind.Attack);
            var read = CandidateHistoryOperations.ReadRange(history, new CandidateHistoryRangeRequest {
                PlayerId = old.PlayerId, AttemptId = history.CurrentRun.Baseline.Entry.AttemptId,
                ExpectedSceneRevision = history.CurrentRun.CurrentSnapshot.SceneRevision,
                HistoryAnchorId = last.Result.HistoryAnchorId }, Codec().Math);
            Assert.IsTrue(read.IsAccepted, read.FieldPath);
            var input = Input(CandidateApplicationKind.Rollback, 2);
            input.Rollback = new CandidateApplicationRollbackInput {
                AttemptId = read.Range.AttemptId, ExpectedSceneRevision = read.Range.SceneRevision,
                HistoryAnchorId = read.Range.HistoryAnchorId, TargetOperationId = read.Range.OperationId,
                ConfirmedRemovedOperationIds = read.Range.Entries.Select(x => x.OperationId).ToArray() };
            var intent = TakeCore(CandidateApplicationProtocol.PrepareIntent(input, Codec()));
            var rolled = CandidateHistoryOperations.PrepareRollback(history, new CandidateRollbackRequest {
                PlayerId = old.PlayerId, AttemptId = read.Range.AttemptId, OperationId = input.OperationId,
                ExpectedSceneRevision = read.Range.SceneRevision, HistoryAnchorId = read.Range.HistoryAnchorId }, read.Range, Codec().Math);
            Assert.IsTrue(rolled.IsAccepted, rolled.FieldPath);
            var business = TakeCore(CandidateBusinessSaveCodec.Prepare(new CandidateBusinessInput(old.PlayerId,
                old.Roster, old.Inventory, old.Progression, old.Rewards, rolled.Next, old.RetainedRuns,
                old.RetainedRollbacks, old.Format), SavePurpose.PlayerSave, Codec()));
            var ticket = Prepare(intent, business, new CandidateApplicationResultInput(), null);
            var written = Store.Write(ticket, Budget());
            Assert.IsTrue(written.IsAccepted, written.Code);
            Load(written.Value.Descriptor);
        }

        internal static byte[] CharacterBytes(CandidateCharacterState character)
        {
            using (var stream = new MemoryStream())
            {
                var fields = new BusinessFields(stream, false, Codec()) { SchemaVersion = 4, ValidatePublished = true, StrictUnicode = true };
                new CandidatePermanentSaveCodec(fields).Character(character, "M03.Character");
                return stream.ToArray();
            }
        }

        internal static byte[] BattleBytes(CandidateBusinessSnapshot business)
        {
            var budget = Codec();
            var battle = new CandidateBattleSaveCodec(budget);
            battle.Collect(business);
            using (var stream = new MemoryStream())
            {
                var fields = new BusinessFields(stream, false, budget) { SchemaVersion = 2, ValidatePublished = true, StrictUnicode = true };
                battle.Body(fields, business, out _, out _, out _);
                return stream.ToArray();
            }
        }

        internal BigInteger Total(string item) => Head.Business.Inventory.Holdings.Single(x => x.ItemId == item).T;
        public void Dispose() { Store?.Dispose(); }
    }
}
