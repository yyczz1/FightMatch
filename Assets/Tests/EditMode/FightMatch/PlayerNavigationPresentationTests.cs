using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FightMatch.Application;
using FightMatch.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.EventSystems;
using static FightMatch.Core.Tests.PlayerSessionTestData;
using static FightMatch.Core.Tests.PlayerRosterTestData;
using static FightMatch.Core.Tests.NavigationAssertions;

namespace FightMatch.Core.Tests
{
    public sealed class PlayerNavigationPresentationTests
    {
        [Test] public void UGUI_COPY_D04_ReasonMappingRequiresDomainAndStableAllowlistedCode()
        {
            Assert.AreEqual("fm.entry.binding_unsupported", NavigationBindings.ReasonKey(NavigationReasonDomain.Entry, "UnsupportedBinding"));
            Assert.AreEqual("fm.common.business_attention", NavigationBindings.ReasonKey(NavigationReasonDomain.Permanent, "UnsupportedBinding"));
            Assert.AreEqual("", NavigationBindings.ReasonKey((NavigationReasonDomain)999, "UnsupportedBinding"));
            Assert.AreEqual("", NavigationBindings.ReasonKey(NavigationReasonDomain.Navigation, "unknown-secret-code"));
            Assert.AreEqual("", NavigationBindings.ReasonKey(NavigationReasonDomain.Notification, "NotificationFailure", committed: false));
            Assert.AreEqual("fm.save_result.notification_failed", NavigationBindings.ReasonKey(NavigationReasonDomain.Notification, "NotificationFailure", committed: true));
            Assert.AreEqual("NOTIFICATION_FAILED", NavigationBindings.ErrorCode("NotificationFailure"));
            using (var r = new NavigationRig())
            using (var controller = new PlayerNavigationController(r.Player, Budget()))
            using (var panel = new NavigationPanel(controller))
            {
                var head = r.Head; var files = r.Files(); var token = controller.View.Token;
                var diagnostic = new CandidateApplicationDiagnostic("InvalidValue", "secret.field", stage: "secret.stage",
                    exceptionType: "secret.Exception", exceptionMessage: "secret exception contents");
                panel.View.Render(ReadModel(controller.View, diagnostic: diagnostic, status: diagnostic.Code));
                foreach (var locale in new[] { LocaleId.En, LocaleId.ZhHans })
                {
                    panel.Localization.SetLocale(locale);
                    panel.AssertVisible("fm.common.business_attention", new KeyValuePair<string, string>("errorCode", "INVALID_INPUT"));
                    panel.AssertNoIdentity(diagnostic.FieldPath, diagnostic.Stage, diagnostic.ExceptionType, diagnostic.ExceptionMessage, diagnostic.Code);
                }
                diagnostic = new CandidateApplicationDiagnostic("unknown-secret-code", "secret.field");
                panel.View.Render(ReadModel(controller.View, diagnostic: diagnostic, status: diagnostic.Code));
                Assert.IsTrue(panel.VisibleTexts.Any(x => x.DiagnosticCode != null));
                panel.AssertNoIdentity(diagnostic.Code, diagnostic.FieldPath);
                Assert.AreSame(token, controller.View.Token); Unchanged(r, head, files);
            }
        }
        [Test] public void UGUI_COPY_CD01_LifecycleRoutingShowsOnlyOwningSurfaceAndHidesInternalFacts()
        {
            using (var r = new NavigationRig())
            using (var controller = new PlayerNavigationController(r.Player, Budget()))
            using (var panel = new NavigationPanel(controller))
            {
                var basis = controller.View; var head = r.Head; var files = r.Files();
                foreach (CandidateApplicationPhase phase in Enum.GetValues(typeof(CandidateApplicationPhase)))
                {
                    var recovery = phase == CandidateApplicationPhase.SaveFailed || phase == CandidateApplicationPhase.CommitUnknown ||
                        phase == CandidateApplicationPhase.PendingPreparation || phase == CandidateApplicationPhase.RestoreRequired || phase == CandidateApplicationPhase.RecoveryBlocked;
                    var diagnostic = phase == CandidateApplicationPhase.RestoreRequired ? new CandidateApplicationDiagnostic("RestoreRequired", "Application") :
                        phase == CandidateApplicationPhase.RecoveryBlocked ? new CandidateApplicationDiagnostic("RecoveryBlocked", "Application") : null;
                    var read = ReadState(basis.Read, phase, diagnostic);
                    panel.View.Render(ReadModel(basis, route: recovery ? PlayerNavigationRoute.Recovery : PlayerNavigationRoute.Gate,
                        read: read, diagnostic: diagnostic));
                    Assert.IsNull(panel.Optional<LocalizedTmpText>(NavigationPanel.Row("save.Head")));
                    panel.AssertNoIdentity(head.Header.CommitId, "InitializationReady", "CreationConfirmationRequired", "Disposed", "PendingPreparation", "RecoveryBlocked", "RestoreRequired");
                    if (phase == CandidateApplicationPhase.Disposed)
                        Assert.IsFalse(panel.Find<UnityEngine.UI.Button>(NavigationPanel.Row("navigation.Back")).gameObject.activeInHierarchy);
                }
                foreach (var kind in new[] { PlayerNavigationTargetKind.CreationRequired, PlayerNavigationTargetKind.BattleSelectionRequested,
                    PlayerNavigationTargetKind.ResumeBattleRequested, PlayerNavigationTargetKind.SettlementRequired, PlayerNavigationTargetKind.RootBackRequested })
                {
                    var host = new PlayerNavigationHostRequest(kind, basis.Context, basis.Read.Application);
                    panel.View.Render(ReadModel(basis, host: host, status: kind.ToString()));
                    panel.AssertNoIdentity(kind.ToString());
                    Assert.IsFalse(panel.VisibleTexts.Any(x => x.DiagnosticCode != null));
                }
                using (var data = new CandidatePermanentTestData())
                {
                    var permanent = new PlayerPermanentView(data.Head, "W", data.Definitions.GetPermanentDefinitions(), null);
                    Assert.AreEqual("Defined", permanent.RecipeAvailability);
                    panel.View.Render(ReadModel(basis, route: PlayerNavigationRoute.CraftList, permanent: permanent));
                    Assert.IsFalse(panel.Find<LocalizedTmpText>(NavigationPanel.Row("crafting.Availability")).gameObject.activeInHierarchy);
                    panel.AssertNoIdentity("Defined", "weapons");
                }
                var duplicate = new CandidateApplicationDiagnostic("ResolutionRequired", "Navigation");
                panel.View.Render(ReadModel(basis, route: PlayerNavigationRoute.Recovery,
                    read: ReadState(basis.Read, CandidateApplicationPhase.Ready, duplicate), status: duplicate.Code, diagnostic: duplicate));
                panel.AssertVisible("fm.save_recovery.blocking_notice");
                Assert.AreSame(basis.Token, controller.View.Token); Unchanged(r, head, files);
            }
        }
        [Test] public void UGUI_COPY_B01_EndConfirmationHasOneSaveAttentionTitle()
        {
            using (var r = new NavigationRig())
            using (var controller = new PlayerNavigationController(r.Player, Budget()))
            using (var panel = new NavigationPanel(controller))
            {
                panel.Click(NavigationPanel.Row("navigation.Team")); panel.Click("fm.action.party.confirm");
                r.Storage.Fault = "snapshot-promoted"; panel.Click(NavigationPanel.Row("save.Confirm"));
                var pending = controller.View.Read.Application.PendingOperationId; var head = r.Head; var files = r.Files();
                panel.AssertCaption(NavigationPanel.Row("save.EndReview"), "fm.save_recovery.review_end_button");
                Assert.IsFalse(panel.Find<UnityEngine.UI.Button>(NavigationPanel.Row("save.End")).gameObject.activeInHierarchy);
                panel.Click(NavigationPanel.Row("save.EndReview"));
                Assert.IsTrue(controller.View.Confirmation.IsEndConfirmation);
                panel.AssertVisible("fm.save_recovery.title"); panel.AssertVisible("fm.save_recovery.end_uncommitted_confirm");
                panel.AssertCaption(NavigationPanel.Row("save.End"), "fm.save_recovery.end_uncommitted_button");
                panel.Click(NavigationPanel.Row("navigation.Cancel"));
                Assert.AreEqual(pending, controller.View.Read.Application.PendingOperationId); Unchanged(r, head, files);
                panel.Click(NavigationPanel.Row("save.EndReview")); panel.Click(NavigationPanel.Row("save.End"));
                Assert.IsNull(controller.View.Read.Application.PendingOperationId);
                Assert.IsFalse(r.Head.Records.Any(x => x.OperationId == pending));
            }
        }
        [Test] public void UGUI_COPY_B02_PreferenceSavedRequiresExactCommittedSubtype()
        {
            using (var data = new CandidatePermanentTestData())
            using (var r = new NavigationRig())
            using (var controller = new PlayerNavigationController(r.Player, Budget()))
            using (var panel = new NavigationPanel(controller))
            {
                data.Apply(new CandidatePermanentDraft { Kind = CandidatePermanentKind.Craft, DefinitionId = "weapons", Quantity = 1 });
                var equip = data.Apply(new CandidatePermanentDraft { Kind = CandidatePermanentKind.Equip, CharacterId = "W", DefinitionId = "weapon", Quantity = 2 });
                var other = Saved(data, equip);
                var preference = data.Apply(new CandidatePermanentDraft { Kind = CandidatePermanentKind.SetPreference, CharacterId = "W", DefinitionId = "weapon",
                    Enabled = false, PreferenceRevision = data.Head.Business.Inventory.PreferenceRevision });
                var saved = Saved(data, preference); Assert.IsTrue(NavigationBindings.PreferenceSaved(saved));
                var head = data.Head; var files = CopyFiles(data.Storage.Files);
                panel.View.Render(ReadModel(controller.View, route: PlayerNavigationRoute.CommittedResult, result: saved, status: "Completed"));
                foreach (var locale in new[] { LocaleId.En, LocaleId.ZhHans })
                {
                    panel.Localization.SetLocale(locale);
                    panel.AssertVisible("fm.inventory.preference.saved"); panel.AssertVisible("fm.save_result.title");
                    Assert.IsTrue(FightMatchViewId.Validate(panel.View.transform.root, out var diagnostic), diagnostic);
                    panel.AssertText(NavigationPanel.Row("save.ConfirmedResult"), "fm.save_result.confirmed",
                        new KeyValuePair<string, string>("operationName", panel.Localization.Resolve("fm.operation.set_item_preference", null).Text));
                }
                foreach (var rejected in new[] { other,
                    new CandidateApplicationCallResult("SaveFailed", false, null, saved.OriginalLookup, saved.LookupViewCommitId, saved.View, null, null),
                    new CandidateApplicationCallResult("Completed", true, saved.OriginalCommitId, new CandidateApplicationLookup(), saved.LookupViewCommitId, saved.View, null, null) })
                {
                    Assert.IsFalse(NavigationBindings.PreferenceSaved(rejected));
                    panel.View.Render(ReadModel(controller.View, route: PlayerNavigationRoute.CommittedResult, result: rejected));
                    Assert.AreEqual(0, panel.CountKey("fm.inventory.preference.saved"));
                }
                Assert.AreSame(head, data.Head); SameFiles(files, data.Storage.Files);
            }
        }
        [Test] public void UGUI_COPY_B03_UnsupportedBindingIsEntryOnlyInEntryDomain()
        {
            using (var r = new NavigationRig())
            using (var controller = new PlayerNavigationController(r.Player, Budget()))
            using (var panel = new NavigationPanel(controller))
            {
                var head = r.Head; var files = r.Files();
                controller.NavigationHandler(new PlayerNavigationTarget { Kind = PlayerNavigationTargetKind.Preparation,
                    LevelId = "level:ch01-01", LevelVersion = "unpublished", Binding = controller.View.Read.Binding })();
                Assert.AreEqual("Navigation.Level", controller.View.Diagnostic.FieldPath);
                panel.AssertVisible("fm.entry.binding_unsupported");
                var diagnostic = new CandidateApplicationDiagnostic("UnsupportedBinding", "Navigation.PermanentKind");
                panel.View.Render(ReadModel(controller.View, status: diagnostic.Code, diagnostic: diagnostic));
                Assert.AreEqual(0, panel.CountKey("fm.entry.binding_unsupported"));
                panel.AssertVisible("fm.common.business_attention", new KeyValuePair<string, string>("errorCode", "UNSUPPORTED_CONTENT"));
                Assert.AreEqual("", NavigationBindings.ReasonKey((NavigationReasonDomain)999, "UnsupportedBinding", "Navigation.Level"));
                Unchanged(r, head, files);
            }
        }
        [Test] public void UGUI_COPY_B04_ActiveBattleReasonFollowsActualCommandDomain()
        {
            using (var r = new NavigationRig())
            {
                r.Enter(); var head = r.Head; var files = r.Files();
                using (var controller = new PlayerNavigationController(r.Player, Budget()))
                using (var panel = new NavigationPanel(controller))
                {
                    panel.Click(NavigationPanel.Row("navigation.Team"));
                    Assert.IsFalse(panel.Find<UnityEngine.UI.Button>("fm.action.party.confirm").interactable);
                    panel.AssertVisible("fm.party.active_battle.locked");
                    panel.Click(NavigationPanel.Row("navigation.MapAdventure")); panel.Click("fm.action.map.level01");
                    Assert.IsFalse(panel.Find<UnityEngine.UI.Button>("fm.action.entry.enter").interactable);
                    panel.AssertVisible("fm.entry.active_battle.blocked");
                    panel.Click(NavigationPanel.Row("navigation.Bag")); panel.Click(FightMatchViewId.Member("W"));
                    panel.AssertReason(NavigationPanel.Row("equipment.Clear"), "ActiveAttemptConflict");
                    Assert.AreEqual("fm.common.business_attention", NavigationBindings.ReasonKey(NavigationReasonDomain.Migration, "ActiveAttemptConflict"));
                    Unchanged(r, head, files);
                }
                Committed(r.Migrate()); r.Act(PlayerNavigationAction.Return); r.Go(PlayerNavigationTargetKind.Bag); r.SelectWarrior();
                r.Nav.Navigate(new PlayerNavigationTarget { Kind = PlayerNavigationTargetKind.Detail, PermanentKind = CandidatePermanentKind.Equip }, r.View.Context, Codec());
                var refused = r.Nav.Preview(new PlayerNavigationDraft { Kind = PlayerNavigationDraftKind.Permanent,
                    Permanent = new PlayerPermanentDraft { Kind = CandidatePermanentKind.Equip, CharacterId = "W", Quantity = 0 } }, r.View.Context, Codec());
                Assert.AreEqual("AttemptActive", refused.Status); Assert.AreEqual("Permanent.Equip", refused.Diagnostic.FieldPath);
                head = r.Head; files = r.Files();
                using (var controller = new PlayerNavigationController(r.Player, Budget()))
                using (var panel = new NavigationPanel(controller))
                {
                    Assert.GreaterOrEqual(panel.CountKey("fm.inventory.active_battle.locked"), 1);
                    Assert.AreEqual(0, panel.CountKey("fm.common.business_attention")); Unchanged(r, head, files);
                }
            }
        }
        [Test] public void UGUI_COPY_B05_ReadSideSaveFailedReusesFailureKeyWithStableCode()
        {
            using (var r = new NavigationRig())
            {
                r.Formation(null, "W", null); r.Storage.Fault = "snapshot-promoted"; r.Act(PlayerNavigationAction.Confirm);
                using (var controller = new PlayerNavigationController(r.Player, Budget()))
                using (var panel = new NavigationPanel(controller))
                {
                    var head = r.Head; var files = r.Files(); var basis = controller.View;
                    Assert.AreEqual(CandidateApplicationPhase.SaveFailed, basis.Read.Application.Phase);
                    var readOnly = ReadModel(basis, read: basis.Read, status: "SaveFailed", diagnostic: basis.Read.Diagnostic);
                    Assert.IsNull(readOnly.Result); panel.View.Render(readOnly);
                    panel.AssertVisible("fm.save_recovery.failed", new KeyValuePair<string, string>("errorCode", "SAVE_FAILED"));
                    foreach (var phase in new[] { CandidateApplicationPhase.PendingPreparation, CandidateApplicationPhase.CommitUnknown })
                    {
                        var read = ReadState(basis.Read, phase, pending: basis.Read.Application.PendingOperationId);
                        panel.View.Render(ReadModel(basis, read: read, status: phase == CandidateApplicationPhase.CommitUnknown ? "CommitUnknown" : "ResolutionRequired"));
                        Assert.AreEqual(0, panel.CountKey("fm.save_recovery.failed"));
                    }
                    Unchanged(r, head, files);
                }
            }
        }
        [UnityTest] public IEnumerator CC26_ActualPanelButtonsFollowMapTeamBagAndRealEmptyRecipes()
        {
            using (var r = new NavigationRig())
            using (var controller = new PlayerNavigationController(r.Player, Budget()))
            using (var panel = new NavigationPanel(controller))
            {
                yield return null; yield return null; yield return null;
                Assert.IsNotNull(panel.View.GetComponentInParent<Canvas>());
                var head = r.Head; var files = r.Files();
                panel.AssertCaption("fm.action.map.level01", "fm.name.level.ch01_01");
                panel.Click("fm.action.map.level01");
                Assert.AreEqual(PlayerNavigationRoute.Preparation, controller.View.Route);
                var token = controller.View.Token;
                panel.Localization.SetLocale(LocaleId.ZhHans);
                Assert.AreSame(token, controller.View.Token);
                var warrior = panel.Localization.Resolve("fm.name.character.w", null).Text;
                var empty = panel.Localization.Resolve("fm.party.slot.empty", null).Text;
                panel.AssertText(NavigationPanel.Row("preparation.Level"), "fm.entry.level.label",
                    new KeyValuePair<string, string>("levelName", panel.Localization.Resolve("fm.name.level.ch01_01", null).Text));
                panel.AssertText(NavigationPanel.Row("preparation.Party"), "fm.entry.party.label",
                    new KeyValuePair<string, string>("partySummary", string.Join(" / ", warrior, empty, empty)));
                panel.AssertCaption(FightMatchViewId.Member(head.Business.Character.CharacterId), "fm.party.member.level",
                    new KeyValuePair<string, string>("characterName", warrior), new KeyValuePair<string, string>("level", "1"));
                panel.Click(NavigationPanel.Row("navigation.Team"));
                Assert.AreEqual(3, panel.View.GetComponentsInChildren<TMPro.TMP_Dropdown>().Length);
                panel.Find<TMPro.TMP_Dropdown>(NavigationPanel.Row("party.slot.front")).value = 0;
                panel.Find<TMPro.TMP_Dropdown>(NavigationPanel.Row("party.slot.rear")).value = 1;
                token = controller.View.Token;
                panel.Localization.SetLocale(LocaleId.En);
                Assert.AreSame(token, controller.View.Token);
                Assert.AreEqual(0, panel.Find<TMPro.TMP_Dropdown>(NavigationPanel.Row("party.slot.front")).value);
                Assert.AreEqual(1, panel.Find<TMPro.TMP_Dropdown>(NavigationPanel.Row("party.slot.rear")).value);
                warrior = panel.Localization.Resolve("fm.name.character.w", null).Text;
                empty = panel.Localization.Resolve("fm.party.slot.empty", null).Text;
                Assert.AreEqual(warrior, panel.Find<TMPro.TMP_Dropdown>(NavigationPanel.Row("party.slot.rear")).captionText.text);
                panel.Click("fm.action.party.confirm");
                Assert.AreEqual(PlayerNavigationRoute.Confirmation, controller.View.Route);
                panel.AssertText(NavigationPanel.Row("save.FormationChange"), "fm.party.change_preview.body",
                    new KeyValuePair<string, string>("beforeParty", string.Join(" / ", warrior, empty, empty)),
                    new KeyValuePair<string, string>("afterParty", string.Join(" / ", empty, empty, warrior)));
                token = controller.View.Token;
                panel.Localization.SetLocale(LocaleId.ZhHans);
                Assert.AreSame(token, controller.View.Token);
                warrior = panel.Localization.Resolve("fm.name.character.w", null).Text;
                empty = panel.Localization.Resolve("fm.party.slot.empty", null).Text;
                panel.AssertText(NavigationPanel.Row("save.FormationChange"), "fm.party.change_preview.body",
                    new KeyValuePair<string, string>("beforeParty", string.Join(" / ", warrior, empty, empty)),
                    new KeyValuePair<string, string>("afterParty", string.Join(" / ", empty, empty, warrior)));
                panel.Click(NavigationPanel.Row("navigation.Cancel")); Unchanged(r, head, files);
                panel.Click(NavigationPanel.Row("navigation.Bag"));
                panel.Click(FightMatchViewId.Member(head.Business.Character.CharacterId));
                panel.AssertText(NavigationPanel.Row("inventory.Empty"), "fm.inventory.empty");
                panel.AssertText(NavigationPanel.Row("character.Selected"), "fm.entry.member.selected",
                    new KeyValuePair<string, string>("characterName", warrior));
                panel.Click(NavigationPanel.Row("navigation.CraftList"));
                Assert.AreEqual("NoPublishedDefinition", controller.View.Permanent.RecipeAvailability);
                Assert.AreEqual(panel.Localization.Resolve("fm.crafting.no_recipes", null).Text,
                    panel.Find<LocalizedTmpText>(NavigationPanel.Row("crafting.Availability")).Target.text);
                Assert.IsFalse(panel.View.GetComponentsInChildren<FightMatchViewId>().Any(x => x.Id.StartsWith(NavigationPanel.Row("recipe."), StringComparison.Ordinal)));
                Unchanged(r, head, files);
            }
        }
        [UnityTest] public IEnumerator CC26_ActualFormationButtonsCommitOnceAndOldDetachedButtonCannotSubmit()
        {
            using (var r = new NavigationRig())
            using (var controller = new PlayerNavigationController(r.Player, Budget()))
            using (var panel = new NavigationPanel(controller))
            {
                yield return null; yield return null; yield return null;
                panel.Click(NavigationPanel.Row("navigation.Team"));
                panel.Find<TMPro.TMP_Dropdown>(NavigationPanel.Row("party.slot.front")).value = 0;
                panel.Find<TMPro.TMP_Dropdown>(NavigationPanel.Row("party.slot.middle")).value = 1;
                panel.Click("fm.action.party.confirm");
                var token = controller.View.Token;
                panel.View.Bind(controller, panel.Localization);
                panel.View.Bind(controller, panel.Localization);
                Assert.AreSame(token, controller.View.Token);
                var old = panel.Find<UnityEngine.UI.Button>(NavigationPanel.Row("save.Confirm")); var before = r.Head.Records.Count;
                panel.Click(old); Committed(controller.View);
                var saved = r.Files(); var head = r.Head;
                ExecuteEvents.Execute(old.gameObject, new PointerEventData(panel.EventSystem) {
                    button = PointerEventData.InputButton.Left }, ExecuteEvents.pointerClickHandler);
                Assert.AreEqual(before + 1, r.Head.Records.Count); Unchanged(r, head, saved);
                panel.Click(NavigationPanel.Row("save.Return")); Assert.AreEqual(PlayerNavigationRoute.MapAdventure, controller.View.Route);
            }
        }
        [Test] public void CC16_ViewAndControllerRebuildRetainPendingAndInvalidateOldCallbackEpoch()
        {
            using (var r = new NavigationRig())
            {
                var controller = new PlayerNavigationController(r.Player, Budget()); var view = new NavigationPanel(controller);
                var pending = r.Formation(null, r.Head.Business.Character.CharacterId, null);
                controller.Refresh(); var confirm = controller.ActionHandler(PlayerNavigationAction.Confirm);
                r.Storage.Fault = "snapshot-promoted"; confirm();
                var original = controller.View.Confirmation.OperationId; var files = r.Files();
                var staleRetry = controller.ActionHandler(PlayerNavigationAction.Retry);
                view.Dispose(); controller.Dispose(); staleRetry(); SameFiles(files, r.Storage.Inner.Files);
                using (var rebuilt = new PlayerNavigationController(r.Player, Budget()))
                using (var element = new NavigationPanel(rebuilt))
                {
                    Assert.AreSame(r.Nav, r.Player.GetNavigationSession());
                    Assert.AreEqual(original, rebuilt.View.Confirmation.OperationId);
                    rebuilt.ActionHandler(PlayerNavigationAction.Retry)(); Committed(rebuilt.View);
                    Assert.AreEqual(original, rebuilt.View.Result.OriginalLookup.Record.OperationId);
                }
            }
        }
        [Test] public void CC05_CC29_HostDispatchAndCapturedTargetsAreImmutableAndSingleUse()
        {
            using (var r = new NavigationRig())
            using (var controller = new PlayerNavigationController(r.Player, Budget()))
            {
                r.Formation(null, null, null); Committed(r.Act(PlayerNavigationAction.Confirm)); r.Act(PlayerNavigationAction.Return);
                r.PreparePage(); controller.Refresh(); var count = 0;
                using (var menu = new NavigationPanel(controller))
                {
                    Assert.AreEqual("NoReadyMember", controller.View.Read.Lifecycle.Enter.Reason);
                    Assert.IsTrue(menu.Find<UnityEngine.UI.Button>("fm.action.entry.enter").interactable);
                }
                controller.HostRequested += _ => count++;
                var target = new PlayerNavigationTarget { Kind = PlayerNavigationTargetKind.BattleSelectionRequested };
                var invoke = controller.NavigationHandler(target); target.Kind = PlayerNavigationTargetKind.RootBackRequested;
                var head = r.Head; var files = r.Files(); invoke(); invoke(); controller.Refresh();
                Assert.AreEqual(1, count); Assert.AreEqual(PlayerNavigationTargetKind.BattleSelectionRequested, controller.View.HostRequest.Kind);
                Unchanged(r, head, files);
            }
        }
        [Test] public void CC21_MenuLifetimeDoesNotOwnPlaybackOrItsOriginalToken()
        {
            using (var r = RealRig())
            {
                Is(r.Lifecycle.Submit(EntryRequest(r), Budget()));
                var input = new CandidateBoardInputController(r.Battle, Budget());
                using (var playback = new CandidateBattlePlaybackController(input, r.Battle))
                {
                    var attack = r.Battle.Submit(r.AttackRequest(), Budget()); Is(attack.Application);
                    var token = r.Battle.QueryView().PresentationToken; Assert.IsNotNull(token);
                    var reports = playback.CompletionReports; var starts = playback.Starts;
                    using (var controller = new PlayerNavigationController(r.Session, Budget()))
                    using (var menu = new NavigationPanel(controller))
                    {
                        controller.NavigationHandler(new PlayerNavigationTarget { Kind = PlayerNavigationTargetKind.Bag })();
                        controller.NavigationHandler(new PlayerNavigationTarget { Kind = PlayerNavigationTargetKind.SelectCharacter,
                            CharacterId = controller.View.Read.Roster.Characters[0].CharacterId })();
                        Assert.IsFalse(menu.Find<UnityEngine.UI.Button>(NavigationPanel.Row("equipment.Clear")).interactable);
                        menu.AssertReason(NavigationPanel.Row("equipment.Clear"), "ActiveAttemptConflict");
                        Assert.IsTrue(menu.Find<UnityEngine.UI.Button>(NavigationPanel.Row("navigation.CraftList")).interactable);
                        menu.AssertReason(NavigationPanel.Row("equipment.Preference"), "NoItemEquipped");
                    }
                    Assert.AreSame(token, r.Battle.QueryView().PresentationToken);
                    Assert.AreEqual(reports, playback.CompletionReports); Assert.AreEqual(starts, playback.Starts);
                }
            }
        }
        [Test] public void CC22_SettlementSelectionPreservesReservedOperationAndDoesNotApplyH06()
        {
            using (var r = RealRig())
            {
                Is(r.Lifecycle.Submit(EntryRequest(r), Budget())); r.Win();
                var before = r.Head; var files = CopyFiles(r.Storage.Files);
                using (var controller = new PlayerNavigationController(r.Session, Budget()))
                using (var menu = new NavigationPanel(controller))
                {
                    Assert.AreEqual("SettlementRequired", controller.View.Status);
                    var request = controller.NavigationHandler(new PlayerNavigationTarget { Kind = PlayerNavigationTargetKind.SettlementRequired });
                    var count = 0; controller.HostRequested += _ => count++; request(); request();
                    Assert.AreEqual(1, count); Assert.AreSame(before.Continuation, controller.View.HostRequest.Application.PublishedSnapshot.Continuation);
                    Assert.IsNotNull(menu.Find<UnityEngine.UI.Button>(NavigationPanel.Row("navigation.SettlementRequired")));
                    Assert.AreEqual("SettlementRequired", r.Session.GetNavigationSession().Navigate(new PlayerNavigationTarget {
                        Kind = PlayerNavigationTargetKind.EndConfirmation }, controller.View.Context, Codec()).Status);
                }
                Assert.AreSame(before, r.Head); SameFiles(files, r.Storage.Files);
            }
        }
        [UnityTest] public IEnumerator CC27_QuantityParserRejectsNegativeNoncanonicalAndOversizedWithoutWrites()
        {
            using (var r = new NavigationRig())
            {
                Committed(r.Migrate()); r.Act(PlayerNavigationAction.Return); r.Go(PlayerNavigationTargetKind.Bag); r.SelectWarrior();
                // The first release has no tactical item; this deliberately invalid target must not reach business preview.
                var item = r.Head.Business.Inventory.Definition.Items.First();
                var v = r.View;
                r.Nav.Navigate(new PlayerNavigationTarget { Kind = PlayerNavigationTargetKind.Detail,
                    PermanentKind = CandidatePermanentKind.Equip, DefinitionId = item.ItemId }, v.Context, Codec());
                using (var controller = new PlayerNavigationController(r.Player, Budget()))
                using (var panel = new NavigationPanel(controller))
                {
                    yield return null; yield return null; yield return null;
                    var head = r.Head; var files = r.Files();
                    var inputError = panel.Find<LocalizedTmpText>(NavigationPanel.Row("permanent.InputError"));
                    Assert.IsFalse(inputError.gameObject.activeInHierarchy);
                    foreach (var value in new[] { "-1", "01", "1.5", new string('9', 4097) })
                    {
                        panel.Find<TMPro.TMP_InputField>(NavigationPanel.Row("permanent.Quantity")).text = value;
                        panel.Click(NavigationPanel.Row("permanent.Preview"));
                        Assert.IsTrue(inputError.gameObject.activeInHierarchy);
                        Assert.IsNotEmpty(panel.Find<LocalizedTmpText>(NavigationPanel.Row("permanent.InputError")).Target.text); Unchanged(r, head, files);
                    }
                }
            }
        }
    }
}
