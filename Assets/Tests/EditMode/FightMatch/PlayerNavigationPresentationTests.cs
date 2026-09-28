using System;
using System.Collections;
using System.Linq;
using FightMatch.Application;
using FightMatch.Presentation;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using NavigationElement = FightMatch.Presentation.PlayerNavigationView;
using static FightMatch.Core.Tests.PlayerSessionTestData;
using static FightMatch.Core.Tests.PlayerRosterTestData;
using static FightMatch.Core.Tests.NavigationAssertions;

namespace FightMatch.Core.Tests
{
    public sealed class PlayerNavigationPresentationTests
    {
        private static void Click(Button button)
        {
            Assert.IsNotNull(button); Assert.IsTrue(button.enabledInHierarchy, button.name);
            using (var evt = NavigationSubmitEvent.GetPooled()) { evt.target = button; button.SendEvent(evt); }
        }
        [UnityTest] public IEnumerator CC26_ActualPanelButtonsFollowMapTeamBagAndRealEmptyRecipes()
        {
            using (var r = new NavigationRig())
            using (var controller = new PlayerNavigationController(r.Player, Budget()))
            using (var panel = new NavigationPanel(controller))
            {
                yield return null; yield return null; yield return null;
                Assert.IsNotNull(panel.View.panel);
                var head = r.Head; var files = r.Files();
                Click(panel.View.Query<Button>().ToList().Single(x => x.name.StartsWith("level-")));
                Assert.AreEqual(PlayerNavigationRoute.Preparation, controller.View.Route);
                Click(panel.View.Q<Button>("nav-Team"));
                Assert.AreEqual(3, panel.View.Query<DropdownField>().ToList().Count);
                panel.View.Q<DropdownField>("team-slot-0").value = "";
                panel.View.Q<DropdownField>("team-slot-2").value = head.Business.Character.CharacterId;
                Click(panel.View.Q<Button>("team-preview"));
                Assert.AreEqual(PlayerNavigationRoute.Confirmation, controller.View.Route);
                Click(panel.View.Q<Button>("nav-Cancel")); Unchanged(r, head, files);
                Click(panel.View.Q<Button>("nav-Bag"));
                Click(panel.View.Query<Button>().ToList().Single(x => x.name.StartsWith("character-")));
                Click(panel.View.Q<Button>("nav-CraftList"));
                Assert.AreEqual("NoPublishedDefinition", panel.View.Q<Label>("recipe-availability").text);
                Assert.IsFalse(panel.View.Query<Button>().ToList().Any(x => x.name.StartsWith("recipe-")));
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
                Click(panel.View.Q<Button>("nav-Team"));
                panel.View.Q<DropdownField>("team-slot-0").value = "";
                panel.View.Q<DropdownField>("team-slot-1").value = r.Head.Business.Character.CharacterId;
                Click(panel.View.Q<Button>("team-preview"));
                var old = panel.View.Q<Button>("save-Confirm"); var before = r.Head.Records.Count;
                Click(old); Committed(controller.View);
                var saved = r.Files(); var head = r.Head;
                using (var evt = NavigationSubmitEvent.GetPooled()) { evt.target = old; old.SendEvent(evt); }
                Assert.AreEqual(before + 1, r.Head.Records.Count); Unchanged(r, head, saved);
                Click(panel.View.Q<Button>("save-Return")); Assert.AreEqual(PlayerNavigationRoute.MapAdventure, controller.View.Route);
            }
        }
        [Test] public void CC16_ViewAndControllerRebuildRetainPendingAndInvalidateOldCallbackEpoch()
        {
            using (var r = new NavigationRig())
            {
                var controller = new PlayerNavigationController(r.Player, Budget()); var view = new NavigationElement(controller);
                var pending = r.Formation(null, r.Head.Business.Character.CharacterId, null);
                controller.Refresh(); var confirm = controller.ActionHandler(PlayerNavigationAction.Confirm);
                r.Storage.Fault = "snapshot-promoted"; confirm();
                var original = controller.View.Confirmation.OperationId; var files = r.Files();
                var staleRetry = controller.ActionHandler(PlayerNavigationAction.Retry);
                view.Dispose(); controller.Dispose(); staleRetry(); SameFiles(files, r.Storage.Inner.Files);
                using (var rebuilt = new PlayerNavigationController(r.Player, Budget()))
                using (var element = new NavigationElement(rebuilt))
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
                using (var menu = new NavigationElement(controller))
                {
                    Assert.AreEqual("NoReadyMember", controller.View.Read.Lifecycle.Enter.Reason);
                    Assert.IsTrue(menu.Q<Button>("nav-battle").enabledSelf);
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
                    using (var menu = new NavigationElement(controller))
                    {
                        controller.NavigationHandler(new PlayerNavigationTarget { Kind = PlayerNavigationTargetKind.Bag })();
                        controller.NavigationHandler(new PlayerNavigationTarget { Kind = PlayerNavigationTargetKind.SelectCharacter,
                            CharacterId = controller.View.Read.Roster.Characters[0].CharacterId })();
                        Assert.IsFalse(menu.Q<Button>("equip-empty").enabledSelf);
                        Assert.AreEqual("ActiveAttemptConflict", menu.Q<Button>("equip-empty").tooltip);
                        Assert.IsTrue(menu.Q<Button>("nav-CraftList").enabledSelf);
                        Assert.AreEqual("NoItemEquipped", menu.Q<Button>("preference").tooltip);
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
                using (var menu = new NavigationElement(controller))
                {
                    Assert.AreEqual("SettlementRequired", controller.View.Status);
                    var request = controller.NavigationHandler(new PlayerNavigationTarget { Kind = PlayerNavigationTargetKind.SettlementRequired });
                    var count = 0; controller.HostRequested += _ => count++; request(); request();
                    Assert.AreEqual(1, count); Assert.AreSame(before.Continuation, controller.View.HostRequest.Application.PublishedSnapshot.Continuation);
                    Assert.IsNotNull(menu.Q<Button>("nav-SettlementRequired"));
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
                    foreach (var value in new[] { "-1", "01", "1.5", new string('9', 4097) })
                    {
                        panel.View.Q<TextField>("permanent-quantity").value = value;
                        Click(panel.View.Q<Button>("permanent-preview"));
                        Assert.IsNotEmpty(panel.View.Q<Label>("permanent-input-error").text); Unchanged(r, head, files);
                    }
                }
            }
        }
    }
    internal sealed class NavigationPanel : IDisposable
    {
        private readonly CandidateBoardTestWindow window;
        internal readonly NavigationElement View;
        internal NavigationPanel(PlayerNavigationController controller)
        {
            window = ScriptableObject.CreateInstance<CandidateBoardTestWindow>();
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
            {
                LogAssert.Expect(LogType.Error, "No graphic device is available to initialize the view.");
                LogAssert.Expect(LogType.Error, "No graphic device is available to show the window.");
                LogAssert.Expect(LogType.Error, "No graphic device is available to initialize the view.");
            }
            window.Show(); View = new NavigationElement(controller); window.rootVisualElement.Add(View);
        }
        public void Dispose()
        {
            View.Dispose();
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
                LogAssert.Expect(LogType.Error, "No graphic device is available to initialize the view.");
            window.Close();
        }
    }
}
