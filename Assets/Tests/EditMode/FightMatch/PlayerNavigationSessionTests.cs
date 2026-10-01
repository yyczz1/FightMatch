using System;
using System.Linq;
using System.Threading.Tasks;
using FightMatch.Application;
using FightMatch.Content;
using FightMatch.Platform;
using NUnit.Framework;
using static FightMatch.Core.Tests.PlayerSessionTestData;
using static FightMatch.Core.Tests.NavigationAssertions;

namespace FightMatch.Core.Tests
{
    public sealed class PlayerNavigationSessionTests
    {
        [TestCase(false)] [TestCase(true)]
        public void CC02_RecordOnlyAndCommittedUnconfirmedRemainCreationGated(bool committed)
        {
            using (var r = new NavigationRig(initialize: false))
            {
                if (committed) r.ProfileStorage.FailConfirmation = true; else r.Storage.FailLease = true;
                r.Player.CreateNew(r.Profile, r.Storage, r.Locator, r.Capabilities, Budget());
                Assert.AreEqual(LocalPlayerProfileState.CreateIntentRecorded, r.Locator.Read(Budget()).Observation.State);
                Assert.AreEqual(committed, r.Head != null); var files = r.Files();
                var v = r.View; Assert.AreEqual("CreationPending", v.Status);
                Assert.AreEqual("CreationPending", v.ReasonFor(PlayerNavigationAction.End));
                Assert.AreEqual("CreationPending", r.Go(PlayerNavigationTargetKind.EndConfirmation).Status);
                SameFiles(files, r.Storage.Inner.Files);
                Is(r.Player.ContinueCreate(r.Profile, r.Locator.Read(Budget()).Observation, r.Storage, r.Locator, r.Capabilities, Budget()));
                Assert.IsTrue(r.View.Read.IsAvailable); Assert.AreEqual(1, r.Head.Records.Count);
            }
        }
        [Test] public void CC01_CC03_FirstReleaseNavigationIsOneOwnerAndPureRead()
        {
            using (var r = new NavigationRig())
            {
                var head = r.Head; var files = r.Files(); var published = r.Publications;
                Assert.AreSame(r.Nav, r.Player.GetNavigationSession());
                var v = r.View;
                Assert.IsTrue(v.Read.IsAvailable); Assert.AreSame(head, v.Read.Head);
                CollectionAssert.AreEqual(new[] { "level:ch01-01" }, v.Read.Levels.Select(x => x.LevelId));
                Assert.AreEqual(298, r.Profile.CreateRecord.FrozenNewProfileDefinitionBytes.Count);
                Assert.AreEqual(1, v.Read.Roster.Characters.Count); Assert.AreEqual(3, v.Read.Roster.Slots.Count);
                r.PreparePage(); r.Go(PlayerNavigationTargetKind.Team); r.Go(PlayerNavigationTargetKind.Bag);
                r.Act(PlayerNavigationAction.Back); r.Act(PlayerNavigationAction.Cancel);
                Unchanged(r, head, files); Assert.AreEqual(published, r.Publications);
            }
        }
        [Test] public void CC03_CurrentDownloadDoesNotReplaceOpenedExactBinding()
        {
            var content = new PublishedFixture();
            using (var r = new NavigationRig(catalog: content.Catalog, publication: content.V1))
            {
                var head = r.Head; var files = r.Files(); content.Activate(content.V2);
                Assert.IsTrue(r.View.Read.Binding.Same(content.V1.Binding));
                Assert.IsFalse(r.View.Read.Binding.Same(content.V2.Binding)); Unchanged(r, head, files);
            }
        }
        [Test] public void CC04_CC05_SelectionChecksExactBindingAndHostRequestIsSingleUse()
        {
            using (var r = new NavigationRig())
            {
                var v = r.View; var head = r.Head; var files = r.Files();
                var invalid = r.Nav.Navigate(new PlayerNavigationTarget { Kind = PlayerNavigationTargetKind.Preparation,
                    LevelId = v.Read.Levels[0].LevelId, LevelVersion = "2", Binding = v.Read.Binding }, v.Context, Codec());
                Assert.AreEqual("UnsupportedBinding", invalid.Status);
                var prepared = r.PreparePage();
                Assert.IsNull(prepared.HostRequest); Assert.AreSame(head, prepared.Read.Head);
                var target = new PlayerNavigationTarget { Kind = PlayerNavigationTargetKind.BattleSelectionRequested };
                var requested = r.Nav.Navigate(target, prepared.Context, Codec());
                Assert.AreEqual(target.Kind, requested.HostRequest.Kind); Assert.AreSame(head, requested.HostRequest.Context.Head);
                var duplicate = r.Nav.Navigate(target, prepared.Context, Codec());
                Assert.AreEqual("StaleNavigationContext", duplicate.Status); Unchanged(r, head, files);
            }
        }
        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(-1)]
        public void CC06_ExplicitSlotsAndEmptyOnlyCommitAfterConfirm(int slot)
        {
            using (var r = new NavigationRig())
            {
                r.Go(PlayerNavigationTargetKind.Team); var head = r.Head; var files = r.Files();
                var slots = new string[3]; if (slot >= 0) slots[slot] = head.Business.Character.CharacterId;
                var preview = r.Formation(slots); var expected = slots.ToArray(); slots[0] = "mutated";
                Unchanged(r, head, files);
                CollectionAssert.AreEqual(expected, preview.Confirmation.Slots);
                Committed(r.Nav.Act(PlayerNavigationAction.Confirm, preview.Token, Budget()));
                CollectionAssert.AreEqual(expected, r.Player.QueryRoster().Slots);
                var saved = r.Files(); var committed = r.Head;
                r.Act(PlayerNavigationAction.Return); Unchanged(r, committed, saved);
            }
        }
        [Test] public void CC05_NoReadyMemberIsAnEntryCheckHintAndDoesNotPerformOrPreventHostSelection()
        {
            using (var r = new NavigationRig())
            {
                r.Formation(null, null, null); Committed(r.Act(PlayerNavigationAction.Confirm)); r.Act(PlayerNavigationAction.Return);
                var v = r.PreparePage(); var head = r.Head; var files = r.Files(); var publications = r.Publications;
                Assert.AreEqual("NoReadyMember", v.Read.Lifecycle.Enter.Reason);
                v = r.Nav.Navigate(new PlayerNavigationTarget { Kind = PlayerNavigationTargetKind.BattleSelectionRequested }, v.Context, Codec());
                Assert.AreEqual(PlayerNavigationTargetKind.BattleSelectionRequested, v.HostRequest.Kind);
                Assert.AreEqual(head.Header.CommitId, v.HostRequest.Context.CommitId);
                Assert.IsNull(head.Business.ActiveHistory); Unchanged(r, head, files); Assert.AreEqual(publications, r.Publications);
            }
        }
        [Test] public void CC06_DuplicateUnknownAndWrongSlotCountAreRejectedWithoutWrites()
        {
            using (var r = new NavigationRig())
            {
                var head = r.Head; var files = r.Files(); var id = head.Business.Character.CharacterId;
                Assert.AreEqual("InconsistentBinding", r.Formation(id, id, null).Status);
                Assert.AreEqual("InconsistentBinding", r.Formation("unknown", null, null).Status);
                Assert.AreEqual("InvalidValue", r.Formation(id).Status); Unchanged(r, head, files);
            }
        }
        [Test] public void CC07_BagNeedsNoInventedCharacterAndDoesNotChangeLoadouts()
        {
            using (var r = new NavigationRig())
            {
                var head = r.Head; var files = r.Files(); var v = r.Go(PlayerNavigationTargetKind.Bag);
                Assert.IsNull(v.Context.SelectedCharacterId); Assert.IsNotNull(v.Read.Inventory);
                var rejected = r.Nav.Preview(new PlayerNavigationDraft { Kind = PlayerNavigationDraftKind.Permanent,
                    Permanent = new PlayerPermanentDraft { Kind = CandidatePermanentKind.Equip, Quantity = 0 } }, v.Context, Codec());
                Assert.AreEqual("ActorSelectionRequired", rejected.Status); Unchanged(r, head, files);
            }
        }
        [Test] public void CC12_HeadChangeInvalidatesUnconfirmedDraftButNotOriginalPreparedOperation()
        {
            using (var r = new NavigationRig())
            {
                var confirmation = r.Formation(null, r.Head.Business.Character.CharacterId, null);
                Is(r.Life.Submit(Prepared(r.Player.PrepareFormation(new PlayerFormationDraft {
                    ExpectedCommitId = r.Head.Header.CommitId, ExpectedFormationRevision = r.Head.Business.Roster.FormationRevision,
                    Slots = new[] { null, null, r.Head.Business.Character.CharacterId } }, Codec())), Budget()));
                var head = r.Head; var files = r.Files();
                Assert.AreEqual("StaleNavigationToken", r.Nav.Act(PlayerNavigationAction.Confirm, confirmation.Token, Budget()).Status);
                Unchanged(r, head, files); Assert.IsNull(r.View.Confirmation);
                var owned = r.Formation(null, head.Business.Character.CharacterId, null);
                var done = r.Nav.Act(PlayerNavigationAction.Confirm, owned.Token, Budget()); Committed(done);
                Is(r.Life.Submit(Prepared(r.Player.PrepareFormation(new PlayerFormationDraft {
                    ExpectedCommitId = r.Head.Header.CommitId, ExpectedFormationRevision = r.Head.Business.Roster.FormationRevision,
                    Slots = new[] { r.Head.Business.Character.CharacterId, null, null } }, Codec())), Budget()));
                head = r.Head; files = r.Files();
                var repeated = r.Nav.Act(PlayerNavigationAction.Confirm, owned.Token, Budget()); Committed(repeated);
                Assert.AreEqual(done.Result.OriginalCommitId, repeated.Result.OriginalCommitId);
                Assert.AreEqual(done.Result.OriginalLookup.Record.OperationId, repeated.Result.OriginalLookup.Record.OperationId);
                Assert.AreEqual(head.Header.CommitId, repeated.Result.LookupViewCommitId);
                Assert.AreNotEqual(repeated.Result.OriginalCommitId, repeated.Result.LookupViewCommitId); Unchanged(r, head, files);
            }
        }
        [TestCase(true)] [TestCase(false)]
        public void CC23_ExplicitAdjacentMigrationPreservesCreationAnchorAndRejectsSkipping(bool legacy)
        {
            using (var r = new NavigationRig(legacy: legacy))
            {
                var first = r.Head.Records[0].CommitId; var head = r.Head; var files = r.Files();
                var v = r.View;
                Assert.AreEqual("UnsupportedSchema", r.Nav.Preview(new PlayerNavigationDraft {
                    Kind = PlayerNavigationDraftKind.Migration, FromFormat = 2, ToFormat = 4 }, v.Context, Codec()).Status);
                Unchanged(r, head, files);
                if (legacy) { Committed(r.Migrate()); r.Act(PlayerNavigationAction.Return); }
                Committed(r.Migrate()); Assert.AreEqual(CandidateBusinessFormat.PublishedPermanentV4, r.Head.Business.Format);
                Assert.IsTrue(r.Player.QueryRoster().IsMaterialized);
                Assert.AreEqual(first, r.Head.Records[0].CommitId);
                Assert.AreEqual(first, TakeCore(PlayerProfileCreateRecordCodec.VerifyInitializationCommit(r.Profile.CreateRecord, r.Head, Codec())).OriginalInitializationCommitId);
            }
            using (var r = new NavigationRig(legacy: legacy))
            using (var controller = new FightMatch.Presentation.PlayerNavigationController(r.Player, Budget()))
            using (var panel = new NavigationPanel(controller))
            {
                var anchor = r.Head.Records[0].CommitId;
                var count = legacy ? 2 : 1;
                for (var i = 0; i < count; i++)
                {
                    var head = r.Head; var files = r.Files();
                    panel.AssertCaption(NavigationPanel.Row("navigation.Migration"), "fm.profile.data_upgrade.title");
                    panel.Click(NavigationPanel.Row("navigation.Migration"));
                    panel.AssertHeading("fm.profile.data_upgrade.title");
                    panel.AssertText(NavigationPanel.Row("save.Migration"), "fm.profile.data_upgrade.body");
                    panel.AssertNoIdentity("V2", "V3", "V4", "PublishedRosterV3", "PublishedPermanentV4");
                    panel.Click(NavigationPanel.Row("navigation.Cancel")); Unchanged(r, head, files);
                    panel.Click(NavigationPanel.Row("navigation.Migration")); panel.Click(NavigationPanel.Row("save.Confirm"));
                    Committed(controller.View);
                    Assert.AreEqual(anchor, r.Head.Records[0].CommitId);
                    panel.Click(NavigationPanel.Row("save.Return"));
                }
                Assert.AreEqual(CandidateBusinessFormat.PublishedPermanentV4, r.Head.Business.Format);
                Assert.IsFalse(panel.Find<UnityEngine.UI.Button>(NavigationPanel.Row("navigation.Migration")).gameObject.activeInHierarchy);
            }
        }
        [Test] public void CC20_CC23_ActiveBattleGatesFormationWhileExplicitV3MigrationPreservesBattle()
        {
            using (var r = new NavigationRig())
            {
                r.Enter(); var battle = CandidatePermanentTestData.BattleBytes(r.Head.Business); var head = r.Head; var files = r.Files();
                Assert.AreEqual("ActiveAttemptConflict", r.Formation(null, null, null).Status);
                Assert.IsNull(r.Go(PlayerNavigationTargetKind.BattleSelectionRequested).HostRequest); Unchanged(r, head, files);
                Committed(r.Migrate()); CollectionAssert.AreEqual(battle, CandidatePermanentTestData.BattleBytes(r.Head.Business));
                r.Act(PlayerNavigationAction.Return); r.Go(PlayerNavigationTargetKind.Bag); r.SelectWarrior();
                head = r.Head; files = r.Files(); var view = r.View;
                view = r.Nav.Preview(new PlayerNavigationDraft { Kind = PlayerNavigationDraftKind.Permanent,
                    Permanent = new PlayerPermanentDraft { Kind = CandidatePermanentKind.Equip,
                        CharacterId = view.Context.SelectedCharacterId, Quantity = 0 } }, view.Context, Codec());
                Assert.AreEqual("AttemptActive", view.Status); Unchanged(r, head, files);
                CollectionAssert.AreEqual(battle, CandidatePermanentTestData.BattleBytes(r.Head.Business));
                r.Nav.Navigate(new PlayerNavigationTarget { Kind = PlayerNavigationTargetKind.Detail,
                    PermanentKind = CandidatePermanentKind.Equip }, r.View.Context, Codec());
                using (var controller = new FightMatch.Presentation.PlayerNavigationController(r.Player, Budget()))
                using (var menu = new NavigationPanel(controller))
                {
                    Assert.IsFalse(menu.Find<UnityEngine.UI.Button>(NavigationPanel.Row("permanent.Preview")).interactable);
                    menu.AssertReason(NavigationPanel.Row("permanent.Preview"), "ActiveAttemptConflict");
                    Unchanged(r, head, files);
                }
            }
        }
        [TestCase(false, PlayerNavigationAction.Back)] [TestCase(true, PlayerNavigationAction.Back)]
        [TestCase(false, PlayerNavigationAction.Cancel)] [TestCase(true, PlayerNavigationAction.Cancel)]
        [TestCase(false, PlayerNavigationAction.Return)] [TestCase(true, PlayerNavigationAction.Return)]
        public void CC06_CC10_CC13_EndedEquipmentRestoresOuterFormationContext(bool preparation, PlayerNavigationAction exit)
        {
            using (var r = new NavigationRig())
            {
                Committed(r.Migrate()); r.Act(PlayerNavigationAction.Return);
                if (preparation) r.PreparePage();
                r.Go(PlayerNavigationTargetKind.Team); r.SelectWarrior(); var team = r.View.Context;
                var v = r.Nav.Navigate(new PlayerNavigationTarget { Kind = PlayerNavigationTargetKind.Detail,
                    PermanentKind = CandidatePermanentKind.Equip }, team, Codec());
                if (exit != PlayerNavigationAction.Back)
                    v = r.Nav.Preview(new PlayerNavigationDraft { Kind = PlayerNavigationDraftKind.Permanent,
                        Permanent = new PlayerPermanentDraft { Kind = CandidatePermanentKind.Equip,
                            CharacterId = team.SelectedCharacterId, Quantity = 0 } }, v.Context, Codec());
                if (exit == PlayerNavigationAction.Return) { v = r.Nav.Act(PlayerNavigationAction.Confirm, v.Token, Budget()); Committed(v); }
                var oldToken = v.Token; var head = r.Head; var files = r.Files();
                v = r.Nav.Act(exit, oldToken, Budget()); Assert.AreEqual(PlayerNavigationRoute.Team, v.Route);
                var outer = preparation ? PlayerNavigationRoute.Preparation : PlayerNavigationRoute.MapAdventure;
                Assert.AreEqual(outer, v.Context.ReturnAnchor); CollectionAssert.AreEqual(team.Parents, v.Context.Parents);
                Assert.AreEqual(team.SelectedCharacterId, v.Context.SelectedCharacterId);
                Assert.AreEqual(team.LevelId, v.Context.LevelId); Assert.AreEqual(team.LevelVersion, v.Context.LevelVersion);
                CollectionAssert.AreEqual(preparation ? new[] { PlayerNavigationRoute.MapAdventure, PlayerNavigationRoute.Preparation } :
                    new[] { PlayerNavigationRoute.MapAdventure }, v.Context.Parents);
                Assert.AreEqual("StaleNavigationToken", r.Nav.Act(PlayerNavigationAction.Back, oldToken, Budget()).Status);
                Assert.AreEqual(PlayerNavigationRoute.Team, r.View.Route); Unchanged(r, head, files);
                var formation = r.Formation(null, head.Business.Character.CharacterId, null);
                v = r.Nav.Act(PlayerNavigationAction.Cancel, formation.Token, Budget()); Assert.AreEqual(outer, v.Route);
                Assert.AreEqual(outer, v.Context.ReturnAnchor); Assert.AreEqual(team.SelectedCharacterId, v.Context.SelectedCharacterId);
                Assert.AreEqual(team.LevelId, v.Context.LevelId); Assert.AreEqual(team.LevelVersion, v.Context.LevelVersion);
                CollectionAssert.AreEqual(preparation ? new[] { PlayerNavigationRoute.MapAdventure } : Array.Empty<PlayerNavigationRoute>(), v.Context.Parents);
                Unchanged(r, head, files);
            }
        }
        [TestCase(false)] [TestCase(true)]
        public void CC10_CC23_LeavingUncommittedEquipmentRestoresOuterMigrationContext(bool preparation)
        {
            using (var r = new NavigationRig())
            {
                foreach (var exit in new[] { PlayerNavigationAction.Back, PlayerNavigationAction.Cancel })
                {
                    if (preparation) r.PreparePage();
                    r.Go(PlayerNavigationTargetKind.Team); r.SelectWarrior(); var team = r.View.Context;
                    r.Nav.Navigate(new PlayerNavigationTarget { Kind = PlayerNavigationTargetKind.Detail,
                        PermanentKind = CandidatePermanentKind.Equip }, team, Codec());
                    var v = r.Act(exit); Assert.AreEqual(PlayerNavigationRoute.Team, v.Route);
                    var outer = preparation ? PlayerNavigationRoute.Preparation : PlayerNavigationRoute.MapAdventure;
                    Assert.AreEqual(outer, v.Context.ReturnAnchor); var head = r.Head; var files = r.Files();
                    v = r.Nav.Preview(new PlayerNavigationDraft { Kind = PlayerNavigationDraftKind.Migration,
                        FromFormat = 3, ToFormat = 4 }, v.Context, Codec());
                    v = r.Nav.Act(PlayerNavigationAction.Cancel, v.Token, Budget()); Assert.AreEqual(outer, v.Route);
                    Assert.AreEqual(outer, v.Context.ReturnAnchor); Assert.AreEqual(team.LevelId, v.Context.LevelId);
                    Assert.AreEqual(team.LevelVersion, v.Context.LevelVersion);
                    CollectionAssert.AreEqual(preparation ? new[] { PlayerNavigationRoute.MapAdventure } : Array.Empty<PlayerNavigationRoute>(), v.Context.Parents);
                    Unchanged(r, head, files);
                }
            }
        }
        [Test] public void CC10_CC14_CC16_CC17_PreparedMigrationKeepsPreparationAnchorThroughRecoveryAndRebind()
        {
            using (var r = new NavigationRig())
            {
                var preparation = r.PreparePage().Context; r.Go(PlayerNavigationTargetKind.Team);
                var v = r.Nav.Preview(new PlayerNavigationDraft { Kind = PlayerNavigationDraftKind.Migration,
                    FromFormat = 3, ToFormat = 4 }, r.View.Context, Codec());
                r.Storage.Fault = "snapshot-promoted";
                var failed = r.Nav.Act(PlayerNavigationAction.Confirm, v.Token, Budget());
                Assert.AreEqual("SaveFailed", failed.Result.Code); var commit = failed.Read.Application.PendingCommitId;
                var operation = failed.Confirmation.OperationId; var head = r.Head; var files = r.Files();
                Assert.AreEqual(PlayerNavigationRoute.Recovery, r.Act(PlayerNavigationAction.Back).Route);
                Assert.AreEqual(PlayerNavigationRoute.Recovery, r.Act(PlayerNavigationAction.Cancel).Route);
                Assert.AreEqual(operation, r.View.Confirmation.OperationId); Unchanged(r, head, files);
                using (var controller = new FightMatch.Presentation.PlayerNavigationController(r.Player, Budget()))
                using (var menu = new NavigationPanel(controller))
                {
                    Assert.AreSame(r.Nav, r.Player.GetNavigationSession());
                    Assert.AreEqual(operation, controller.View.Confirmation.OperationId);
                    controller.ActionHandler(PlayerNavigationAction.Retry)(); Committed(controller.View);
                    Assert.AreEqual(operation, controller.View.Result.OriginalLookup.Record.OperationId);
                    Assert.AreEqual(commit, controller.View.Result.OriginalCommitId);
                }
                var repeated = r.Nav.Act(PlayerNavigationAction.Confirm, v.Token, Budget()); Committed(repeated);
                Assert.AreEqual(commit, repeated.Result.OriginalCommitId); Assert.AreEqual(head.Records.Count + 1, r.Head.Records.Count);
                head = r.Head; files = r.Files(); v = r.Act(PlayerNavigationAction.Return);
                Assert.AreEqual(PlayerNavigationRoute.Preparation, v.Route); Assert.AreEqual(preparation.ReturnAnchor, v.Context.ReturnAnchor);
                CollectionAssert.AreEqual(preparation.Parents, v.Context.Parents);
                Assert.AreEqual(preparation.LevelId, v.Context.LevelId); Assert.AreEqual(preparation.LevelVersion, v.Context.LevelVersion);
                Unchanged(r, head, files); var reopened = r.Rebuild();
                Assert.IsTrue(r.View.Read.IsAvailable, reopened.Code); Assert.AreEqual(PlayerNavigationRoute.MapAdventure, r.View.Route);
                Assert.AreEqual(PlayerNavigationRoute.MapAdventure, r.View.Context.ReturnAnchor);
                Assert.IsEmpty(r.View.Context.Parents); Assert.IsNull(r.View.Context.LevelId);
                Assert.AreEqual(head.Header.CommitId, r.Head.Header.CommitId); SameFiles(files, r.Storage.Inner.Files);
            }
        }
        [Test] public void CC24_WrongThreadDisposedAndBusyDoNotMutateOwnedContext()
        {
            using (var r = new NavigationRig())
            {
                var v = r.View; var head = r.Head; var files = r.Files();
                Assert.AreEqual("WrongThread", Task.Run(() => r.Nav.Query(Codec()).Read.Code).GetAwaiter().GetResult());
                Unchanged(r, head, files);
                PlayerNavigationView nested = null;
                r.OnPublish = _ => nested = r.Nav.Query(Codec());
                var preview = r.Formation(null, head.Business.Character.CharacterId, null);
                Committed(r.Nav.Act(PlayerNavigationAction.Confirm, preview.Token, Budget()));
                Assert.AreEqual("Busy", nested.Status);
                r.Runtime.Close(); Assert.AreEqual("Disposed", r.Nav.Query(Codec()).Read.Code);
            }
        }
        [Test] public void CC24_QueryBudgetRefusalDoesNotWriteOrReplaceOriginalHead()
        {
            using (var r = new NavigationRig())
            {
                var head = r.Head; var files = r.Files();
                var v = r.Nav.Query(new SaveCodecBudget(new ExactMathBudget(maxPrimitiveSteps: 0)));
                Assert.AreEqual("Limit", v.Read.Code); Unchanged(r, head, files);
            }
        }
        [TestCase("snapshot-before")] [TestCase("marker-before")] [TestCase("marker-after")]
        public void CC02_CreatePendingCannotBeEndedOrUsedForNavigationWrites(string fault)
        {
            using (var r = new NavigationRig(initialize: false))
            {
                r.Storage.Fault = fault;
                var create = r.Player.CreateNew(r.Profile, r.Storage, r.Locator, r.Capabilities, Budget());
                Assert.IsFalse(create.IsCommitted); var files = r.Files();
                Assert.AreEqual("CreationPending", r.View.Status);
                Assert.AreEqual("CreationPending", r.Go(PlayerNavigationTargetKind.EndConfirmation).Status);
                Assert.IsFalse(r.Formation(null, null, null).ReasonFor(PlayerNavigationAction.Confirm) == null);
                SameFiles(files, r.Storage.Inner.Files);
                Is(r.Player.ContinueCreate(r.Profile, r.Locator.Read(Budget()).Observation, r.Storage, r.Locator, r.Capabilities, Budget()));
                Assert.IsTrue(r.View.Read.IsAvailable);
            }
        }
    }
}
