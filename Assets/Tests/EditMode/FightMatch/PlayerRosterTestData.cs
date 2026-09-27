using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using FightMatch.Application;
using FightMatch.Content;
using FightMatch.Platform;
using FlowPuzzle.Core;
using NUnit.Framework;
using static FightMatch.Core.Tests.PlayerSessionTestData;

namespace FightMatch.Core.Tests
{
    internal static class PlayerRosterTestData
    {
        internal static ExactRational R(int n, int d = 1) => ExactRational.Create(n, d, Codec().Math);
        internal static CandidateTimeSample Time(int elapsed = 0) => LifecycleRig.Time(100 + elapsed, elapsed, "roster:clock");
        internal static SaveRecoveryCapabilities RosterCaps(ResolvedPublication publication)
        {
            var old = Caps(publication);
            return new SaveRecoveryCapabilities(PlayerSessionSystem.RequiredRecoveryContracts, old.Bindings, old.RuleVersions,
                old.NumericContractVersions, old.RandomContractVersions, PlayerSessionSystem.RequiredRecoveryFeatures);
        }
        internal static Rig RealRig(bool migrated = true)
        {
            var catalog = RealCatalog(); var publication = Resolve(catalog);
            var rig = new Rig(catalog: catalog, publication: publication, capabilities: RosterCaps(publication));
            try { if (migrated) Is(rig.Lifecycle.Submit(Prepared(rig.Session.PrepareRosterMigration(rig.Head.Header.CommitId, Codec())), Budget())); return rig; }
            catch { rig.Dispose(); throw; }
        }
        internal static PlayerFormationDraft Formation(Rig rig, params string[] slots) => new PlayerFormationDraft {
            ExpectedCommitId = rig.Head.Header.CommitId, ExpectedFormationRevision = rig.Head.Business.Roster.FormationRevision, Slots = slots };
        internal static PlayerFormationEntryDraft EntryDraft(CandidateApplicationSnapshot head, PreparedLevel level)
        {
            var roster = head.Business.Roster;
            return new PlayerFormationEntryDraft { ExpectedCommitId = head.Header.CommitId, LevelId = level.LevelId, LevelVersion = level.LevelVersion,
                ExpectedFormationRevision = roster.FormationRevision, ExpectedInventoryRevision = head.Business.Inventory.StateRevision,
                ExpectedProgressionRevision = head.Business.Progression.StateRevision,
                SelectedCharacters = roster.Formation.Where(x => x != null).OrderBy(x => x, StringComparer.Ordinal)
                    .Select(id => new CandidateCharacterRevisionInput { CharacterId = id, ExpectedRevision = roster.Find(id).StateRevision }).ToArray() };
        }
        internal static PreparedCandidateLifecycleRequest EntryRequest(Rig rig, CandidateTimeSample time = null) =>
            Prepared(rig.Session.PrepareFormationEntry(EntryDraft(rig.Head, rig.Publication.Definitions.Levels[0]), time ?? Time(), Codec()));

        // Pure resolved-definition closure only. This does not publish a package, forge review evidence, or create a formal session.
        internal sealed class IsolatedRoster
        {
            internal readonly PublishedRuleContext Context;
            internal readonly CandidateLifecycleContentInput Content;
            internal readonly PublishedSaveContext Closure;
            internal CandidateApplicationSnapshot Head;
            internal SaveEnvelope Envelope;
            internal CandidateApplicationCandidate Candidate;
            internal PreparedCandidateLifecycleRequest Initialization;
            internal int IdCalls, EntropyCalls;
            private int serial;
            internal IsolatedRoster(int enemyHp = 15, bool tactical = false, bool legacy = false)
            {
                var original = new SharedRuleFixture(true); Context = new PublishedRuleContext(original.Content);
                var growths = new List<CandidateGrowthDefinition>();
                foreach (var id in new[] { "A", "B", "C", "D" })
                {
                    var raw = BusinessSaveScenario.Growth(SharedRuleFixture.Candidate(), id == "C" ? 3 : id == "A" ? 7 : 100);
                    raw.Context = Context; raw.ClassId = "warrior:" + id;
                    raw.RecoveryDurationMilliseconds = R(id == "A" ? 360000 : 180000);
                    var growth = CandidateCharacterGrowth.PrepareDefinition(raw, Codec().Math);
                    Assert.IsTrue(growth.IsAccepted, growth.FieldPath); growths.Add(growth.Definition);
                }
                var rawEntry = SharedRuleFixture.RawEntry();
                foreach (var pair in rawEntry.Level.Faces[0].Pairs) pair.Enemy.Stats.MaxHp = R(enemyHp);
                var level = new BattleEntryPreparer().PrepareCandidate(rawEntry, Codec().Math);
                Assert.IsTrue(level.IsAccepted, level.FieldPath);
                var items = new List<CandidateInventoryItemInput> { new CandidateInventoryItemInput { ItemId = "tin", Kind = CandidateInventoryItemKind.OrdinaryMaterial } };
                if (tactical) foreach (var id in new[] { "A", "B", "C", "D" })
                    items.Add(new CandidateInventoryItemInput { ItemId = "item:" + id, Kind = CandidateInventoryItemKind.OrdinaryTactical, EquipClassId = "warrior:" + id });
                var inventory = CandidateInventory.PrepareDefinition(new CandidateInventoryDefinitionInput { Context = Context, Items = items }, Codec().Math);
                Assert.IsTrue(inventory.IsAccepted, inventory.FieldPath);
                if (legacy) growths = new List<CandidateGrowthDefinition> { growths[0] };
                Content = new CandidateLifecycleContentInput { Growths = growths, Inventory = inventory.Definition,
                    Progression = original.Progression.Definition, Levels = new[] { level.Entry.Level },
                    Rewards = new[] { original.RewardDefinition }, CritCoefficients = new[] { new CandidateCritCoefficient(R(1, 5), R(1, 1000)),
                        new CandidateCritCoefficient(R(41, 200), R(2, 1000)) } };
                var definitions = TakeCore(PublishedRuleDefinitions.Prepare(original.Content, growths, Content.Inventory, Content.Progression,
                    Content.Levels, Content.Rewards, Codec()));
                Closure = TakeCore(PublishedSaveContext.Prepare(new[] { definitions }, Codec()));
                if (legacy)
                {
                    Initialization = Prepared(CandidateLifecyclePreparation.NewProfile(Content, new CandidateApplicationInitializeInput {
                        CharacterId = "A", ClassId = "warrior:A", InitialLevel = 1, InitialExperience = 0, OriginalSlot = 0 }, Codec(), true));
                    Commit(Initialization); return;
                }
                var input = Input(CandidateApplicationKind.InitializeProfile);
                input.RosterInitialize = new CandidateRosterInitializeInput {
                    Characters = new[] { "A", "B", "C", "D" }.Select(id => new CandidateApplicationInitializeInput {
                        CharacterId = id, ClassId = "warrior:" + id, InitialLevel = 1, InitialExperience = 0, OriginalSlot = 0 }).ToArray(),
                    Slots = new[] { "C", "A", "B" } };
                Initialization = Freeze(input); Commit(Initialization);
            }
            internal CandidateApplicationIntentInput Input(CandidateApplicationKind kind) => new CandidateApplicationIntentInput {
                PlayerId = Head?.Business.PlayerId ?? "roster:isolated", OperationId = "roster:op:" + ++serial, ExpectedCommitId = Head?.Header.CommitId,
                Kind = kind, FormatVersion = 3, Context = Context };
            internal PreparedCandidateLifecycleRequest Freeze(CandidateApplicationIntentInput input) =>
                Prepared(CandidateLifecyclePreparation.Roster(input, Content, Codec()));
            internal PreparedCandidateLifecycleRequest EntryRequest(CandidateTimeSample time = null)
            {
                if (Head.Business.Format == CandidateBusinessFormat.PublishedV2)
                {
                    var c = Head.Business.Character; var l = Content.Levels[0];
                    return Prepared(CandidateLifecyclePreparation.Prepare(new CandidateLifecycleDraft {
                        PlayerId = Head.Business.PlayerId, ExpectedCommitId = Head.Header.CommitId, Content = Content,
                        Kind = CandidateApplicationKind.EnterAttempt, EnterAttempt = new CandidateApplicationEnterInput {
                            CharacterId = c.CharacterId, OriginalSlot = c.OriginalSlot, ExpectedCharacterRevision = c.StateRevision,
                            LevelId = l.LevelId, LevelVersion = l.LevelVersion } }, Codec(), true));
                }
                var draft = EntryDraft(Head, Content.Levels[0]); var input = Input(CandidateApplicationKind.EnterFormation);
                input.EnterFormation = new CandidateFormationEntryInput { LevelId = draft.LevelId, LevelVersion = draft.LevelVersion,
                    ExpectedFormationRevision = draft.ExpectedFormationRevision, Slots = Head.Business.Roster.Formation,
                    SelectedCharacters = draft.SelectedCharacters, ExpectedInventoryRevision = draft.ExpectedInventoryRevision,
                    ExpectedProgressionRevision = draft.ExpectedProgressionRevision, TimeSample = time ?? Time() };
                return Freeze(input);
            }
            internal CandidateApplicationBuildResult Build(PreparedCandidateLifecycleRequest request, SaveCodecBudget codec = null)
            {
                return CandidateLifecycleApplicationSystem.Build(Head, request, codec ?? Codec(), nextId: () => {
                    IdCalls++; return "roster:id:" + ++serial;
                }, entropy: bytes => {
                    EntropyCalls++; Assert.AreEqual(48, bytes.Length);
                    for (var i = 0; i < bytes.Length; i += 16) { bytes[i] = 42; bytes[i + 8] = 54; }
                });
            }
            internal void Commit(PreparedCandidateLifecycleRequest request)
            {
                var built = Build(request); Assert.IsTrue(built.IsAccepted, built.Diagnostic?.Code + " " + built.Diagnostic?.FieldPath);
                Assert.IsFalse(built.CommitEligible);
                Commit(request.Intent, built.NextBusiness, built.CopyResult(), built.NextSettlementOperationId);
            }
            internal void Commit(PreparedCandidateApplicationIntent intent, CandidateBusinessSnapshot business,
                CandidateApplicationResultInput result, string reserved = null)
            {
                Candidate = TakeCore(CandidateApplicationProtocol.Propose(Head, business, intent, result, reserved, Codec()));
                Assert.IsFalse(Candidate.CommitEligible);
                Envelope = TakeCore(CandidateApplicationSaveCodec.EncodePublished(Candidate, ApplicationScenario.Header(Head, intent.OperationId), Closure, Codec()));
                Head = TakeCore(CandidateApplicationSaveCodec.DecodePublished(Envelope, Closure, Codec()));
                CollectionAssert.AreEqual(Head.Business.Format == CandidateBusinessFormat.PublishedRosterV3
                    ? new uint[] { 3, 3, 3, 3, 2, 2 } : new uint[] { 2, 2, 2, 2, 2, 2 }, Envelope.RequiredSliceContracts.Select(x => x.SchemaVersion));
            }
            internal PreparedCandidateLifecycleRequest SetFormation(params string[] slots)
            {
                var input = Input(CandidateApplicationKind.SetFormation);
                input.SetFormation = new CandidateFormationInput { ExpectedFormationRevision = Head.Business.Roster.FormationRevision, Slots = slots };
                var request = Freeze(input); Commit(request); return request;
            }
            internal CandidateBattleOperationRecord Attack(string character = "B", int pair = 0)
            {
                var b = Head.Business; var history = b.ActiveHistory; var state = history.CurrentRun.CurrentSnapshot;
                var operation = "roster:attack:" + ++serial;
                var attack = new CandidateApplicationAttackInput { AttemptId = state.Baseline.Entry.AttemptId,
                    ExpectedSceneRevision = state.SceneRevision, Actor = state.Members.Single(x => x.Member.CharacterId == character).CombatantKey,
                    Pair = state.Enemies[pair].PairKey, Route = new[] { new FlowPos(0, pair * 2), new FlowPos(1, pair * 2),
                        new FlowPos(2, pair * 2), new FlowPos(3, pair * 2) }, ExpectedPreferenceRevision = b.Inventory.PreferenceRevision, ItemUseEnabled = false };
                var intent = TakeCore(CandidateApplicationProtocol.PrepareIntent(new CandidateApplicationIntentInput { PlayerId = b.PlayerId,
                    OperationId = operation, Kind = CandidateApplicationKind.Attack, Context = Context, ExpectedCommitId = Head.Header.CommitId, Attack = attack }, Codec()));
                var result = CandidateBattleOperations.EvaluateAttack(history.CurrentRun, new CandidateAttackRequest { PlayerId = b.PlayerId,
                    AttemptId = attack.AttemptId, OperationId = operation, ExpectedSceneRevision = state.SceneRevision, Actor = attack.Actor,
                    Pair = attack.Pair, Route = attack.Route.ToList() }, new CandidateBattleConditions {
                        PreferenceRevision = b.Inventory.PreferenceRevision, ItemUseEnabled = false }, 100 + serial, new RandomSamplingBudget(Codec().Math));
                Assert.IsTrue(result.IsAccepted, result.RejectionCode + " " + result.FieldPath);
                var appended = CandidateHistoryOperations.Append(history, result.NextRun, "anchor:" + operation, Codec().Math);
                Assert.IsTrue(appended.IsAccepted, appended.FieldPath);
                var business = TakeCore(CandidateBusinessSaveCodec.Prepare(new CandidateBusinessInput(b.PlayerId, b.Roster, b.Inventory, b.Progression,
                    b.Rewards, appended.Next, b.RetainedRuns, b.RetainedRollbacks, b.Format), SavePurpose.PlayerSave, Codec()));
                Commit(intent, business, new CandidateApplicationResultInput { HistoryAnchorId = "anchor:" + operation },
                    result.NextRun.FinalReport == null ? null : "roster:settle:" + serial);
                return Head.Business.ActiveHistory.CurrentRun.Records.Last();
            }
            internal PreparedCandidateLifecycleRequest End(bool restart = false, CandidateTimeSample time = null)
            {
                var state = Head.Business.ActiveHistory.CurrentRun.CurrentSnapshot; var entry = state.Baseline.Entry;
                var input = new CandidateApplicationEndInput { AttemptId = entry.AttemptId, ChallengeId = entry.ChallengeId,
                    EntryBaselineId = entry.EntryBaselineId, ExpectedSceneRevision = state.SceneRevision };
                var prepared = CandidateLifecyclePreparation.Prepare(new CandidateLifecycleDraft { PlayerId = Head.Business.PlayerId,
                    ExpectedCommitId = Head.Header.CommitId, Kind = restart ? CandidateApplicationKind.RestartAttempt : CandidateApplicationKind.ExitAttempt,
                    Content = Content, RestartAttempt = restart ? input : null, ExitAttempt = restart ? null : input,
                    EndTimeSample = restart ? null : time ?? Time() }, Codec(), true);
                var request = Prepared(prepared); Commit(request); return request;
            }
            internal PreparedCandidateLifecycleRequest Settle()
            {
                var request = Prepared(CandidateLifecyclePreparation.Victory(Head, Content, Time(), Codec(), true)); Commit(request); return request;
            }
        }
    }
}
