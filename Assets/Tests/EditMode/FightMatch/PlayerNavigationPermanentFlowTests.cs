using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using FightMatch.Application;
using FightMatch.Platform;
using NUnit.Framework;
using static FightMatch.Core.Tests.PlayerSessionTestData;
using static FightMatch.Core.Tests.NavigationAssertions;

namespace FightMatch.Core.Tests
{
    public sealed class PlayerNavigationPermanentFlowTests
    {
        [Test] public void CC07_CC10_CC27_RealEmptyEquipmentFlowHasBoundedParentsAndBackConsumesOneLayer()
        {
            using (var r = new NavigationRig())
            {
                Committed(r.Migrate()); r.Act(PlayerNavigationAction.Return); r.PreparePage();
                r.Go(PlayerNavigationTargetKind.Bag); r.SelectWarrior(); r.Go(PlayerNavigationTargetKind.CraftList);
                var v = r.View;
                v = r.Nav.Navigate(new PlayerNavigationTarget { Kind = PlayerNavigationTargetKind.Detail,
                    PermanentKind = CandidatePermanentKind.Equip }, v.Context, Codec());
                Assert.AreEqual(4, v.Context.Parents.Count); var head = r.Head; var files = r.Files();
                v = r.Nav.Preview(new PlayerNavigationDraft { Kind = PlayerNavigationDraftKind.Permanent,
                    Permanent = new PlayerPermanentDraft { Kind = CandidatePermanentKind.Equip, CharacterId = v.Context.SelectedCharacterId,
                        DefinitionId = null, Quantity = 0 } }, v.Context, Codec());
                Assert.AreEqual(PlayerNavigationRoute.Confirmation, v.Route, v.Status);
                Assert.IsNull(v.Confirmation.Quote.DefinitionId); Assert.AreEqual(BigInteger.Zero, v.Confirmation.Quote.Quantity);
                foreach (var expected in new[] { PlayerNavigationRoute.Detail, PlayerNavigationRoute.CraftList,
                    PlayerNavigationRoute.Bag, PlayerNavigationRoute.Preparation, PlayerNavigationRoute.MapAdventure })
                {
                    var token = v.Token; v = r.Nav.Act(PlayerNavigationAction.Back, token, Budget());
                    Assert.AreEqual(expected, v.Route); Assert.LessOrEqual(v.Context.Parents.Count + 1, 5);
                    r.Nav.Act(PlayerNavigationAction.Back, token, Budget()); Assert.AreEqual(expected, r.View.Route); v = r.View;
                }
                Assert.IsNull(v.Context.LevelId); Unchanged(r, head, files);
            }
        }
        private static PlayerNavigationContext Context(CandidatePermanentTestData data)
        {
            var app = new CandidateApplicationView(data.Head.Business.PlayerId, CandidateApplicationPhase.Ready,
                data.Head, true, null, null, Array.Empty<string>(), null, SavePurpose.PlayerSave);
            var read = new PlayerNavigationReadResult(app, data.Binding, data.Definitions.Levels, null, null, null, null, null);
            return new PlayerNavigationContext(null, 1, read, PlayerNavigationRoute.Detail, PlayerNavigationRoute.Preparation,
                "test:L1", "1", "W", new[] { PlayerNavigationRoute.MapAdventure, PlayerNavigationRoute.Preparation });
        }
        internal static PlayerPermanentDraft Craft(CandidatePermanentTestData data, IReadOnlyList<CandidatePermanentPortion> inputs = null)
        {
            return PlayerNavigationSession.BuildPermanentDraft(new PlayerPermanentDraft {
                Kind = CandidatePermanentKind.Craft, CharacterId = "W", DefinitionId = "weapons", Quantity = 1,
                SelectedInputs = inputs }, Context(data), Codec());
        }
        [Test] public void CC08_RealSessionReportsMissingRecipeAndNeverReceivesIsolatedDefinitions()
        {
            using (var r = new NavigationRig())
            {
                Committed(r.Migrate()); r.Act(PlayerNavigationAction.Return); r.Go(PlayerNavigationTargetKind.Bag); r.SelectWarrior();
                var head = r.Head; var files = r.Files(); var v = r.Go(PlayerNavigationTargetKind.CraftList);
                Assert.AreEqual("NoPublishedDefinition", v.Permanent.RecipeAvailability);
                var draft = new PlayerNavigationDraft { Kind = PlayerNavigationDraftKind.Permanent, Permanent = new PlayerPermanentDraft {
                    Kind = CandidatePermanentKind.Craft, CharacterId = v.Context.SelectedCharacterId, DefinitionId = "weapons", Quantity = 1 } };
                Assert.AreEqual("NoPublishedDefinition", r.Nav.Preview(draft, v.Context, Codec()).Status);
                Assert.AreEqual("PreviewRequired", r.View.ReasonFor(PlayerNavigationAction.Confirm)); Unchanged(r, head, files);
            }
        }
        [Test] public void CC09_CC11_ActualProductionDraftCopyPrecedesIsolatedKernelAndSingleConfirmationSlot()
        {
            using (var data = new CandidatePermanentTestData())
            {
                var before = data.Head; var actual = Craft(data);
                Assert.IsNull(actual.CharacterId); Assert.AreEqual(before.Header.CommitId, actual.ExpectedCommitId);
                Assert.AreEqual("weapons", actual.DefinitionId); Assert.AreEqual(BigInteger.One, actual.Quantity);
                var quote = TakeCore(CandidatePermanentProtocol.Preview(before, data.Definitions, actual.Copy(), Codec()));
                Assert.IsNull(quote.CharacterId); Assert.IsNull(quote.ClassId);
                Assert.AreEqual(new BigInteger(3), quote.Costs.Single(x => x.ItemId == "ore").Quantity);
                Assert.AreEqual(new BigInteger(1), quote.Costs.Single(x => x.ItemId == "wood").Quantity);
                Assert.AreEqual(new BigInteger(6), quote.Outputs.Single(x => x.ItemId == "weapon").Quantity);
                Assert.IsTrue(quote.Inputs.All(x => x.Endpoint == CandidatePermanentEndpoint.Held));
                var input = data.Input(CandidateApplicationKind.PermanentRequest, 4); input.SetPermanent(quote);
                var request = data.Freeze(input); var slot = new PlayerNavigationOperation();
                slot.Begin(before, CandidateApplicationKind.PermanentRequest, quote); var epoch = slot.Epoch;
                Assert.IsTrue(slot.TryConsume(epoch)); Assert.IsFalse(slot.TryConsume(epoch));
                Assert.IsTrue(slot.AcceptPrepared(epoch, request)); Assert.AreSame(request, slot.Request);
                Assert.AreSame(request.Intent, slot.Intent); Assert.IsFalse(slot.AcceptPrepared(epoch, request));
                var canonical = request.Intent.CanonicalBytes.ToArray(); data.Commit(request);
                var lookup = TakeCore(CandidateApplicationProtocol.Lookup(data.Head, request.Intent, Codec()));
                var app = new CandidateApplicationView(data.Head.Business.PlayerId, CandidateApplicationPhase.Ready,
                    data.Head, true, null, null, Array.Empty<string>(), null, SavePurpose.PlayerSave);
                var result = new CandidateApplicationCallResult("Completed", true, lookup.OriginalCommitId, lookup,
                    data.Head.Header.CommitId, app, null, null);
                Assert.IsTrue(slot.Receive(epoch, result)); Assert.IsTrue(slot.HasVerifiedResult);
                CollectionAssert.AreEqual(canonical, slot.Intent.CanonicalBytes);
                Assert.AreEqual(new BigInteger(6), data.Total("weapon"));
                Assert.IsFalse(slot.Receive(epoch - 1, result));
                Assert.IsFalse(slot.Receive(epoch, new CandidateApplicationCallResult("Completed", true, "wrong", lookup,
                    data.Head.Header.CommitId, app, null, null)));
                Assert.IsTrue(slot.Receive(epoch, new CandidateApplicationCallResult("NotCommitted", false, null,
                    new CandidateApplicationLookup(), data.Head.Header.CommitId, app, null, null)));
                Assert.IsFalse(slot.HasVerifiedResult);
                slot.Clear(); Assert.IsFalse(slot.Receive(epoch, result));
            }
        }
        [Test] public void CC09_MultipleSourcesRequireAnExplicitSelectionThroughTheProductionDraft()
        {
            using (var data = new CandidatePermanentTestData())
            {
                data.Farm(); var head = data.Head; var files = CopyFiles(data.Storage.Files);
                var draft = Craft(data);
                Assert.AreEqual("CostSelectionRequired", CandidatePermanentProtocol.Preview(head, data.Definitions, draft.Copy(), Codec()).RejectionCode);
                var held = TakeCore(CandidatePermanentInventory.ReadEndpoints(head, Codec()));
                var ore = held.First(x => x.ItemId == "ore"); var wood = held.First(x => x.ItemId == "wood");
                draft = Craft(data, new[] { ore.Slice(ore.UnitStart, 3), wood.Slice(wood.UnitStart, 1) });
                var quote = TakeCore(CandidatePermanentProtocol.Preview(head, data.Definitions, draft.Copy(), Codec()));
                Assert.IsNull(draft.CharacterId); Assert.IsNull(quote.CharacterId); Assert.IsNull(quote.ClassId);
                Assert.AreEqual(2, quote.Inputs.Count); Assert.AreEqual(new BigInteger(6), quote.Outputs.Single().Quantity);
                Assert.AreSame(head, data.Head); SameFiles(files, data.Storage.Files);
            }
        }
        [Test] public void CC09_CC12_ExplicitHeldPortionsAreFrozenWithoutReorderingOrReplacingSources()
        {
            using (var data = new CandidatePermanentTestData())
            {
                var endpoints = TakeCore(CandidatePermanentInventory.ReadEndpoints(data.Head, Codec()));
                var ore = endpoints.First(x => x.ItemId == "ore"); var wood = endpoints.First(x => x.ItemId == "wood");
                var selected = new List<CandidatePermanentPortion> { ore.Slice(ore.UnitStart, 3), wood.Slice(wood.UnitStart, 1) };
                var actual = Craft(data, selected); selected.Clear();
                Assert.AreEqual(2, actual.SelectedInputs.Count);
                var quote = TakeCore(CandidatePermanentProtocol.Preview(data.Head, data.Definitions, actual.Copy(), Codec()));
                Assert.IsNull(quote.CharacterId); Assert.IsNull(quote.ClassId);
                Assert.AreEqual(ore.Source.OriginalOperationId, quote.Inputs.Single(x => x.ItemId == "ore").Source.OriginalOperationId);
                Assert.AreEqual(ore.UnitStart, quote.Inputs.Single(x => x.ItemId == "ore").UnitStart);
                data.Gift();
                Assert.AreEqual("StaleContext", CandidatePermanentProtocol.ValidatePreview(data.Head, data.Definitions, quote, Codec()).RejectionCode);
            }
        }
        [TestCase(CandidatePermanentKind.Equip)] [TestCase(CandidatePermanentKind.SetPreference)]
        public void CC07_NonCraftDraftRetainsExplicitTarget(CandidatePermanentKind kind)
        {
            using (var data = new CandidatePermanentTestData())
            {
                var actual = PlayerNavigationSession.BuildPermanentDraft(new PlayerPermanentDraft {
                    Kind = kind, CharacterId = "M", DefinitionId = "weapon", Quantity = kind == CandidatePermanentKind.Equip ? (BigInteger?)0 : null,
                    Enabled = kind == CandidatePermanentKind.SetPreference ? (bool?)false : null,
                    PreferenceRevision = kind == CandidatePermanentKind.SetPreference ? (BigInteger?)data.Head.Business.Inventory.PreferenceRevision : null
                }, Context(data), Codec());
                Assert.AreEqual("M", actual.CharacterId); Assert.AreEqual("weapon", actual.DefinitionId);
                Assert.AreEqual(kind, actual.Copy().Kind);
            }
        }
        [TestCase(PlayerNavigationTargetKind.Bag)] [TestCase(PlayerNavigationTargetKind.Team)] [TestCase(PlayerNavigationTargetKind.Preparation)]
        public void CC10_CC13_CancelAndCommittedReturnPreserveTheActualAnchor(PlayerNavigationTargetKind origin)
        {
            using (var r = new NavigationRig())
            {
                if (origin == PlayerNavigationTargetKind.Preparation) r.PreparePage(); else r.Go(origin);
                var anchor = r.View.Context.ReturnAnchor; var head = r.Head; var files = r.Files();
                if (origin == PlayerNavigationTargetKind.Team) Assert.AreEqual(PlayerNavigationRoute.MapAdventure, anchor);
                var preview = r.Formation(null, head.Business.Character.CharacterId, null);
                var cancel = r.Nav.Act(PlayerNavigationAction.Cancel, preview.Token, Budget());
                Assert.AreEqual(anchor, cancel.Route); Unchanged(r, head, files);
                Assert.AreEqual("StaleNavigationToken", r.Nav.Act(PlayerNavigationAction.Confirm, preview.Token, Budget()).Status);
                preview = r.Formation(null, null, head.Business.Character.CharacterId);
                var result = r.Nav.Act(PlayerNavigationAction.Confirm, preview.Token, Budget()); Committed(result);
                var returned = r.Nav.Act(PlayerNavigationAction.Return, result.Token, Budget());
                Assert.AreEqual(anchor, returned.Route); Assert.AreEqual(r.Head.Header.CommitId, returned.Context.CommitId);
                var current = r.Head; var saved = r.Files();
                r.Nav.Act(PlayerNavigationAction.Return, result.Token, Budget()); Unchanged(r, current, saved);
            }
        }
        [Test] public void CC10_CC13_TeamEquipmentReturnRestoresPreparationParentsAfterCancelAndCommit()
        {
            using (var r = new NavigationRig())
            {
                Committed(r.Migrate()); r.Act(PlayerNavigationAction.Return);
                foreach (var commit in new[] { false, true })
                {
                    var level = r.PreparePage().Context.LevelId; r.Go(PlayerNavigationTargetKind.Team); r.SelectWarrior();
                    var v = r.View;
                    v = r.Nav.Navigate(new PlayerNavigationTarget { Kind = PlayerNavigationTargetKind.Detail,
                        PermanentKind = CandidatePermanentKind.Equip }, v.Context, Codec());
                    var head = r.Head; var files = r.Files();
                    v = r.Nav.Preview(new PlayerNavigationDraft { Kind = PlayerNavigationDraftKind.Permanent,
                        Permanent = new PlayerPermanentDraft { Kind = CandidatePermanentKind.Equip,
                            CharacterId = v.Context.SelectedCharacterId, Quantity = 0 } }, v.Context, Codec());
                    Assert.AreEqual(PlayerNavigationRoute.Confirmation, v.Route, v.Status);
                    if (commit)
                    {
                        var done = r.Nav.Act(PlayerNavigationAction.Confirm, v.Token, Budget()); Committed(done);
                        Assert.AreEqual(head.Records.Count + 1, r.Head.Records.Count);
                        v = r.Nav.Act(PlayerNavigationAction.Return, done.Token, Budget());
                        Assert.AreEqual(done.Result.OriginalCommitId, v.Result.OriginalCommitId);
                        head = r.Head; files = r.Files();
                    }
                    else v = r.Nav.Act(PlayerNavigationAction.Cancel, v.Token, Budget());
                    Assert.AreEqual(PlayerNavigationRoute.Team, v.Route); Assert.AreEqual(level, v.Context.LevelId);
                    Assert.AreEqual(PlayerNavigationRoute.Preparation, v.Context.ReturnAnchor);
                    CollectionAssert.AreEqual(new[] { PlayerNavigationRoute.MapAdventure, PlayerNavigationRoute.Preparation }, v.Context.Parents);
                    Assert.AreEqual(PlayerNavigationRoute.Preparation, r.Act(PlayerNavigationAction.Back).Route);
                    Assert.AreEqual(PlayerNavigationRoute.MapAdventure, r.Act(PlayerNavigationAction.Back).Route);
                    Assert.IsNull(r.View.Context.LevelId); Unchanged(r, head, files);
                }
            }
        }
        [Test] public void CC11_CC12_ForeignAndReplacedConfirmationTokensCannotWrite()
        {
            using (var r = new NavigationRig())
            {
                var head = r.Head; var files = r.Files(); var warrior = head.Business.Character.CharacterId;
                var first = r.Formation(null, warrior, null);
                var foreign = new PlayerNavigationToken(null, first.Token.Revision, first.Token.Confirmation);
                Assert.AreEqual("StaleNavigationToken", r.Nav.Act(PlayerNavigationAction.Confirm, foreign, Budget()).Status);
                var second = r.Formation(null, null, warrior);
                Assert.AreEqual("StaleNavigationToken", r.Nav.Act(PlayerNavigationAction.Confirm, first.Token, Budget()).Status);
                r.SelectWarrior();
                Assert.AreEqual("StaleNavigationToken", r.Nav.Act(PlayerNavigationAction.Confirm, second.Token, Budget()).Status);
                var third = r.Formation(null, warrior, null); r.Go(PlayerNavigationTargetKind.Bag);
                Assert.AreEqual("StaleNavigationToken", r.Nav.Act(PlayerNavigationAction.Confirm, third.Token, Budget()).Status);
                Unchanged(r, head, files);
                var current = r.Formation(null, null, warrior);
                Committed(r.Nav.Act(PlayerNavigationAction.Confirm, current.Token, Budget()));
                Assert.AreEqual(head.Records.Count + 1, r.Head.Records.Count);
                CollectionAssert.AreEqual(new[] { null, null, warrior }, r.Player.QueryRoster().Slots);
            }
        }
        private static PlayerNavigationView MigrationPreview(NavigationRig r)
        {
            var v = r.View;
            return r.Nav.Preview(new PlayerNavigationDraft { Kind = PlayerNavigationDraftKind.Migration,
                FromFormat = 3, ToFormat = 4 }, v.Context, Codec());
        }
        private static void SamePage(PlayerNavigationContext expected, PlayerNavigationView actual)
        {
            Assert.AreEqual(expected.Route, actual.Route); Assert.AreEqual(expected.ReturnAnchor, actual.Context.ReturnAnchor);
            CollectionAssert.AreEqual(expected.Parents, actual.Context.Parents);
            Assert.AreEqual(expected.LevelId, actual.Context.LevelId); Assert.AreEqual(expected.LevelVersion, actual.Context.LevelVersion);
        }
        [Test] public void CC10_CC13_CC23_BackPastTeamRestoresPreparationForMigrationCancelAndReturn()
        {
            using (var r = new NavigationRig())
            {
                var preparation = r.PreparePage().Context; r.Go(PlayerNavigationTargetKind.Team); r.SelectWarrior();
                r.Nav.Navigate(new PlayerNavigationTarget { Kind = PlayerNavigationTargetKind.Detail,
                    PermanentKind = CandidatePermanentKind.Equip }, r.View.Context, Codec());
                Assert.AreEqual(PlayerNavigationRoute.Team, r.Act(PlayerNavigationAction.Back).Route);
                SamePage(preparation, r.Act(PlayerNavigationAction.Back)); var head = r.Head; var files = r.Files();
                SamePage(preparation, r.Act(PlayerNavigationAction.Cancel)); Unchanged(r, head, files);
                var preview = MigrationPreview(r); SamePage(preparation, r.Nav.Act(PlayerNavigationAction.Back, preview.Token, Budget()));
                preview = MigrationPreview(r);
                SamePage(preparation, r.Nav.Act(PlayerNavigationAction.Cancel, preview.Token, Budget())); Unchanged(r, head, files);
                var done = r.Migrate(); Committed(done); SamePage(preparation, r.Act(PlayerNavigationAction.Return));
                Is(r.Life.Submit(Prepared(r.Player.PrepareFormation(new PlayerFormationDraft {
                    ExpectedCommitId = r.Head.Header.CommitId, ExpectedFormationRevision = r.Head.Business.Roster.FormationRevision,
                    Slots = new[] { null, r.Head.Business.Character.CharacterId, null } }, Codec())), Budget()));
                var committed = r.Head; var saved = r.Files(); var selected = r.Go(PlayerNavigationTargetKind.OriginalOperation,
                    operation: done.Result.OriginalLookup.Record.OperationId);
                var original = r.Nav.Act(PlayerNavigationAction.SelectOriginalOperation, selected.Token, Budget()); Committed(original);
                Assert.AreEqual(done.Result.OriginalCommitId, original.Result.OriginalCommitId);
                Assert.AreEqual(committed.Header.CommitId, original.Result.LookupViewCommitId);
                Assert.AreNotEqual(original.Result.OriginalCommitId, original.Result.LookupViewCommitId); Unchanged(r, committed, saved);
                SamePage(preparation, r.Nav.Act(PlayerNavigationAction.Return, original.Token, Budget()));
                Assert.AreEqual("StaleNavigationToken", r.Nav.Act(PlayerNavigationAction.Return, original.Token, Budget()).Status);
                SamePage(preparation, r.View);
                Assert.AreEqual(PlayerNavigationRoute.MapAdventure, r.Act(PlayerNavigationAction.Back).Route);
                Unchanged(r, committed, saved);
            }
        }
        [TestCase(PlayerNavigationRoute.MapAdventure)] [TestCase(PlayerNavigationRoute.Preparation)]
        [TestCase(PlayerNavigationRoute.Team)] [TestCase(PlayerNavigationRoute.Bag)]
        [TestCase(PlayerNavigationRoute.CraftList)] [TestCase(PlayerNavigationRoute.Detail)]
        public void CC10_CC12_CC23_MigrationBackReplacementAndInvalidationPreserveActualPage(PlayerNavigationRoute page)
        {
            using (var r = new NavigationRig())
            {
                if (page != PlayerNavigationRoute.MapAdventure) r.PreparePage();
                if (page == PlayerNavigationRoute.Team || page == PlayerNavigationRoute.Detail) r.Go(PlayerNavigationTargetKind.Team);
                if (page == PlayerNavigationRoute.Bag || page == PlayerNavigationRoute.CraftList) r.Go(PlayerNavigationTargetKind.Bag);
                if (page == PlayerNavigationRoute.CraftList) r.Go(PlayerNavigationTargetKind.CraftList);
                if (page == PlayerNavigationRoute.Detail)
                {
                    r.SelectWarrior(); r.Nav.Navigate(new PlayerNavigationTarget { Kind = PlayerNavigationTargetKind.Detail,
                        PermanentKind = CandidatePermanentKind.Equip }, r.View.Context, Codec());
                }
                var expected = r.View.Context; var head = r.Head; var files = r.Files();
                MigrationPreview(r); var replacement = MigrationPreview(r);
                SamePage(expected, r.Nav.Act(PlayerNavigationAction.Back, replacement.Token, Budget())); Unchanged(r, head, files);
                replacement = MigrationPreview(r);
                var rejected = r.Nav.Preview(new PlayerNavigationDraft { Kind = PlayerNavigationDraftKind.Migration,
                    FromFormat = 3, ToFormat = 5 }, replacement.Context, Codec());
                Assert.AreEqual("UnsupportedSchema", rejected.Status); SamePage(expected, rejected); Assert.IsNull(rejected.Confirmation);
                Assert.AreEqual("StaleNavigationToken", r.Nav.Act(PlayerNavigationAction.Confirm, replacement.Token, Budget()).Status);
                replacement = MigrationPreview(r); rejected = r.Nav.Preview(null, replacement.Context, Codec());
                Assert.AreEqual("MissingField", rejected.Status); SamePage(expected, rejected); Assert.IsNull(rejected.Confirmation);
                Assert.AreEqual("StaleNavigationToken", r.Nav.Act(PlayerNavigationAction.Confirm, replacement.Token, Budget()).Status);
                replacement = MigrationPreview(r);
                rejected = r.Nav.Act(PlayerNavigationAction.Confirm, replacement.Token,
                    new SaveStoreBudget(new SaveCodecBudget(Codec().Math, maxStringCodeUnits: 0)));
                Assert.AreEqual("Limit", rejected.Status); SamePage(expected, rejected); Assert.IsNull(rejected.Confirmation);
                Assert.AreEqual("StaleNavigationToken", r.Nav.Act(PlayerNavigationAction.Confirm, replacement.Token, Budget()).Status);
                Unchanged(r, head, files);
                var roleQuote = MigrationPreview(r); r.SelectWarrior(); SamePage(expected, r.View);
                Assert.IsNull(r.View.Confirmation);
                Assert.AreEqual("StaleNavigationToken", r.Nav.Act(PlayerNavigationAction.Confirm, roleQuote.Token, Budget()).Status);
                Unchanged(r, head, files);
                var stale = MigrationPreview(r);
                Is(r.Life.Submit(Prepared(r.Player.PrepareFormation(new PlayerFormationDraft {
                    ExpectedCommitId = head.Header.CommitId, ExpectedFormationRevision = head.Business.Roster.FormationRevision,
                    Slots = new[] { null, head.Business.Character.CharacterId, null } }, Codec())), Budget()));
                head = r.Head; files = r.Files(); SamePage(expected, r.View); Assert.IsNull(r.View.Confirmation);
                Assert.AreEqual(head.Header.CommitId, r.View.Context.CommitId);
                Assert.AreEqual("StaleNavigationToken", r.Nav.Act(PlayerNavigationAction.Confirm, stale.Token, Budget()).Status);
                Unchanged(r, head, files);
            }
        }
        [Test] public void CC11_ReentrantAndDuplicateConfirmKeepThePreparedOriginalRequest()
        {
            using (var r = new NavigationRig())
            {
                var old = r.Head; var preview = r.Formation(null, old.Business.Character.CharacterId, null);
                PlayerNavigationView nested = null;
                r.Storage.DuringWrite = () => nested = r.Nav.Act(PlayerNavigationAction.Confirm, preview.Token, Budget());
                var done = r.Nav.Act(PlayerNavigationAction.Confirm, preview.Token, Budget()); Committed(done);
                Assert.AreEqual("Busy", nested.Status); var saved = r.Files(); var head = r.Head;
                var duplicate = r.Nav.Act(PlayerNavigationAction.Confirm, preview.Token, Budget()); Committed(duplicate);
                Assert.AreEqual(done.Result.OriginalCommitId, duplicate.Result.OriginalCommitId);
                Assert.AreEqual(old.Records.Count + 1, head.Records.Count); Unchanged(r, head, saved);
            }
        }
        [TestCase(-1)] [TestCase(0)]
        public void CC27_InvalidCraftQuantityHasNoBusinessEffect(int quantity)
        {
            using (var data = new CandidatePermanentTestData())
            {
                var actual = Craft(data); actual.Quantity = quantity;
                var head = data.Head; var files = CopyFiles(data.Storage.Files);
                Assert.IsFalse(CandidatePermanentProtocol.Preview(head, data.Definitions, actual.Copy(), Codec()).IsAccepted);
                Assert.AreSame(head, data.Head); SameFiles(files, data.Storage.Files);
            }
        }
        [Test] public void CC27_QuantityAndSourceBudgetRefuseWithoutTruncating()
        {
            using (var data = new CandidatePermanentTestData())
            {
                var actual = Craft(data); actual.Quantity = BigInteger.One << 100;
                Assert.IsFalse(CandidatePermanentProtocol.Preview(data.Head, data.Definitions, actual.Copy(),
                    new SaveCodecBudget(new ExactMathBudget(maxIntegerBits: 64, maxPrimitiveSteps: 64000000))).IsAccepted);
                var portions = TakeCore(CandidatePermanentInventory.ReadEndpoints(data.Head, Codec()));
                Assert.Throws<ArgumentOutOfRangeException>(() => PlayerNavigationSession.BuildPermanentDraft(new PlayerPermanentDraft {
                    Kind = CandidatePermanentKind.Craft, SelectedInputs = portions }, Context(data),
                    new SaveCodecBudget(new ExactMathBudget(), maxCollectionEntries: 0)));
            }
        }
    }
}
