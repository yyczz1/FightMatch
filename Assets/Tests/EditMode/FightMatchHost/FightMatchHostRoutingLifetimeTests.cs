using System;
using System.Collections;
using System.Linq;
using FightMatch.Application;
using FightMatch.Core;
using FightMatch.Platform;
using FightMatch.Presentation;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace FightMatch.Host.Tests
{
    public sealed class FightMatchHostRoutingLifetimeTests
    {
        [Test]
        public void CH1_L5_HostKeepsOneCreationIntentAndDoesNotSpendItsLimitTemplate()
        {
            using (var rig = new HostRig(create: false))
            {
                var template = rig.Session.Budget;
                ExactRational.Create(7, 3, template.Codec.Math);
                var spent = template.Codec.Math.PrimitiveStepsUsed;
                Assert.Greater(spent, 0);
                Assert.AreEqual(64000000, template.Codec.Math.MaxPrimitiveSteps);
                rig.Session.ObserveStartup();
                Assert.IsTrue(rig.Session.CanCreate, rig.Session.Status);
                rig.Profile.FailCreateAfterWrite = true;
                rig.Session.CreateProfile();
                Assert.AreEqual("CreateIntentWriteUnknown", rig.Session.Status);
                Assert.AreEqual(LocalPlayerProfileState.CreateIntentRecorded, rig.Session.Observation.State);
                var original = rig.Session.OriginalProfile;
                Assert.IsNotNull(original);
                var canonical = original.Intent.CanonicalBytes.ToArray();
                Assert.AreEqual(spent, template.Codec.Math.PrimitiveStepsUsed);
                rig.Session.ContinueCreation();
                Assert.AreSame(original, rig.Session.OriginalProfile);
                CollectionAssert.AreEqual(canonical, original.Intent.CanonicalBytes);
                Assert.AreEqual(LocalPlayerProfileState.Active, rig.Session.Observation.State, rig.Session.Status);
                Assert.IsTrue(rig.Session.Application.QueryView().View.IsPublishedHeadVerified);
                Assert.AreEqual(1, rig.Head.Records.Count(x => x.OperationId == original.OperationId));
                rig.Enter();
                var head = rig.Head; var files = rig.Files();
                for (var i = 0; i < 6; i++)
                {
                    rig.Session.Navigation.Refresh();
                    rig.Session.Battle.Input.Refresh();
                    var view = rig.Session.Battle.Refresh();
                    Assert.IsTrue(view.Read.IsAvailable, view.Read.Code);
                    Assert.AreEqual(CandidateApplicationPhase.Ready, view.Read.Application.Phase);
                    Assert.AreSame(head, rig.Head);
                }
                Assert.AreSame(template, rig.Session.Budget);
                Assert.AreEqual(spent, template.Codec.Math.PrimitiveStepsUsed);
                rig.SameFiles(files);
            }
        }

        [Test]
        public void H01_DuplicateOwnerCannotTakeOverOrDeinitializeTheActiveArchitecture()
        {
            using (var rig = new HostRig())
            {
                var head = rig.Head;
                Assert.Throws<InvalidOperationException>(() => new FightMatchHostSession(HostRig.Catalog, rig.Root,
                    rig.Profile, id => rig.Storage));
                Assert.AreSame(head, rig.Head);
                Assert.IsFalse(rig.Session.IsDisposed);
                Assert.IsFalse(rig.Session.Battle.IsDisposed);
                var controller = rig.Session.Battle;
                rig.Session.Dispose();
                rig.Session.Dispose();
                Assert.AreEqual(1, rig.Session.DisposeCount);
                Assert.AreEqual(1, controller.DisposeCount);
                rig.Bind();
                rig.Session.ObserveStartup();
                Assert.AreEqual(head.Header.CommitId, rig.Head.Header.CommitId);
            }
        }

        [Test]
        public void H04_ChangedBeforeHostRequestedStillDeliversOneOriginalH02AndRejectsDuplicates()
        {
            using (var rig = new HostRig())
            using (var view = new HostPanel(rig))
            {
                var request = rig.Enter();
                var head = rig.Head;
                var original = rig.Session.Battle.Session.OriginalRequest;
                var files = rig.Files();
                Assert.IsNotNull(original);
                Assert.AreEqual(1, head.Records.Count(x => x.Intent.Kind == CandidateApplicationKind.EnterFormation));
                rig.Session.AcceptHost(request);
                rig.Session.AcceptHost(request);
                Assert.AreSame(original, rig.Session.Battle.Session.OriginalRequest);
                Assert.AreSame(head, rig.Head);
                rig.SameFiles(files);
                view.Dispose();
                rig.Rebuild();
                var restored = rig.Head;
                rig.Session.AcceptHost(request);
                Assert.AreSame(restored, rig.Head);
                Assert.AreEqual(FightMatchHostPage.Battle, rig.Session.Page);
                rig.SameFiles(files);
            }
        }

        [Test]
        public void H04_RootBackMeansApplicationQuitAndCancelPreservesTheSave()
        {
            using (var rig = new HostRig())
            {
                var quit = 0;
                rig.Session.QuitRequested += () => quit++;
                var files = rig.Files();
                rig.Session.Back();
                Assert.AreEqual(FightMatchHostPage.QuitConfirmation, rig.Session.Page);
                Assert.AreEqual(0, quit);
                rig.Session.CancelQuit();
                Assert.AreEqual(FightMatchHostPage.Navigation, rig.Session.Page);
                rig.Session.Back();
                rig.Session.ConfirmQuit();
                Assert.AreEqual(1, quit);
                Assert.IsNull(rig.Head.Business.ActiveHistory);
                rig.SameFiles(files);
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void H04_OriginalEndReceiptCanReturnAndBeExplicitlyOpenedAgainAfterColdRebuild(bool restart)
        {
            using (var rig = new HostRig())
            {
                string operation;
                using (var view = new HostPanel(rig))
                {
                    rig.Enter();
                    var ended = rig.End(view.View, restart);
                    Assert.AreEqual(restart ? PlayerBattleRoute.Battle : PlayerBattleRoute.Result, ended.Route, ended.Status);
                    operation = ended.OriginalIntent.OperationId;
                    if (restart)
                    {
                        Assert.IsNotNull(ended.Receipt);
                        Assert.AreEqual(CandidateApplicationKind.RestartAttempt, ended.OriginalIntent.Kind);
                        ended = rig.End(view.View);
                    }
                    rig.Session.Battle.ReturnTo(view.View.BattleView.Page, PlayerNavigationTargetKind.Bag, ended.Context);
                    Assert.AreEqual(FightMatchHostPage.Navigation, rig.Session.Page);
                }
                var files = rig.Files();
                rig.Rebuild();
                var rebound = new HostPanel(rig);
                try
                {
                    for (var i = 0; i < 2; i++)
                    {
                        rig.Go(PlayerNavigationTargetKind.Bag);
                        rig.Go(PlayerNavigationTargetKind.OriginalOperation, operation);
                        rig.Session.Navigation.ActionHandler(PlayerNavigationAction.SelectOriginalOperation)();
                        Assert.AreEqual(FightMatchHostPage.Battle, rig.Session.Page);
                        var result = rig.Session.Battle.View;
                        Assert.IsNotNull(result.Receipt, result.Status);
                        Assert.AreEqual(operation, result.OriginalIntent.OperationId);
                        rig.Session.Battle.ReturnTo(rebound.View.BattleView.Page, PlayerNavigationTargetKind.MapAdventure, result.Context);
                        Assert.AreEqual(FightMatchHostPage.Navigation, rig.Session.Page);
                        rig.Session.Navigation.Refresh();
                        Assert.AreEqual(FightMatchHostPage.Navigation, rig.Session.Page, "A residual Result is not another user request.");
                        rebound.Dispose();
                        rebound = new HostPanel(rig);
                        Assert.AreEqual(FightMatchHostPage.Navigation, rig.Session.Page, "Rebinding a hidden result does not navigate.");
                    }
                }
                finally { rebound.Dispose(); }
                rig.SameFiles(files);
            }
        }

        [TestCase("save")]
        [TestCase("marker-after")]
        public void H04_ReplacingTheWholeViewKeepsFailedEndRequestAndOriginalResolution(string fault)
        {
            using (var rig = new HostRig())
            {
                var view = new HostPanel(rig);
                try
                {
                    rig.Enter();
                    rig.Storage.Fault = fault;
                    var failed = rig.End(view.View);
                    Assert.AreEqual(fault == "save" ? "SaveFailed" : "CommitUnknown", failed.Result.Code);
                    var request = rig.Session.Battle.Session.OriginalRequest;
                    var intent = failed.OriginalIntent;
                    var files = rig.Files();
                    view.Dispose();
                    view = new HostPanel(rig);
                    Assert.AreSame(request, rig.Session.Battle.Session.OriginalRequest);
                    Assert.AreSame(intent, rig.Session.Battle.View.OriginalIntent);
                    rig.SameFiles(files);
                    var current = rig.Session.Battle.Refresh();
                    var result = rig.Session.Battle.Continue(view.View.BattleView.Page, fault == "save" ?
                        PlayerBattleRecoveryAction.Retry : PlayerBattleRecoveryAction.Resolve, intent, current.Context);
                    Assert.AreEqual(PlayerBattleRoute.Result, result.Route, result.Status);
                    Assert.AreEqual(intent.OperationId, result.Receipt.Lookup.Record.OperationId);
                }
                finally { view.Dispose(); }
            }
        }

        [Test]
        public void H04_ColdObservedMenuSaveUsesOriginalNavigationRecovery()
        {
            using (var rig = new HostRig())
            {
                rig.Go(PlayerNavigationTargetKind.Team);
                var before = rig.Session.Navigation.View;
                rig.Session.Navigation.PreviewHandler(() => new PlayerNavigationDraft {
                    Kind = PlayerNavigationDraftKind.Formation, Slots = before.Read.Roster.Slots.Reverse().ToArray() })();
                rig.Storage.Fault = "snapshot-after";
                rig.Session.Navigation.ActionHandler(PlayerNavigationAction.Confirm)();
                Assert.AreEqual("SaveFailed", rig.Session.Navigation.View.Result.Code);
                var original = rig.Session.Navigation.View.Result;
                var operation = original.View.PendingOperationId;
                rig.Rebuild();
                Assert.AreEqual(FightMatchHostPage.Navigation, rig.Session.Page);
                var commits = rig.Session.Navigation.View.Read.Application.ObservedCandidateCommitIds;
                Assert.AreEqual(1, commits.Count);
                rig.Session.Navigation.NavigationHandler(new PlayerNavigationTarget {
                    Kind = PlayerNavigationTargetKind.ObservedCandidate, CommitId = commits[0] })();
                rig.Session.Navigation.ActionHandler(PlayerNavigationAction.ResumeObserved)();
                rig.Session.Navigation.ActionHandler(PlayerNavigationAction.Retry)();
                Assert.IsTrue(rig.Session.Navigation.View.Result.IsCommitted);
                Assert.AreEqual(operation, rig.Session.Navigation.View.Result.OriginalLookup.Record.OperationId);
                rig.Session.Navigation.ActionHandler(PlayerNavigationAction.Return)();
                rig.Rebuild();
                Assert.AreEqual(FightMatchHostPage.Navigation, rig.Session.Page);
                Assert.IsNull(rig.Session.Navigation.View.Context.SelectedCharacterId);
            }
        }

        [UnityTest]
        public IEnumerator H01_ActualPanelDetachKeepsPresentationTokenAndPauseCompletesItOnlyOnce()
        {
            yield return new EnterPlayMode();
            using (var rig = new HostRig())
            using (var panel = new HostPanel(rig))
            {
                rig.Enter();
                yield return panel.Ready();
                var state = rig.State;
                Assert.IsTrue(rig.Session.Battle.Input.SelectMember(state.Members.First(x => x.Hp.Numerator.Sign > 0).Member.CharacterId));
                panel.Draw(rig.Route(state.Enemies.First(x => x.Hp.Numerator.Sign > 0).PairKey));
                var input = rig.Session.Battle.Input;
                Assert.IsTrue(input.LastResult.Application.IsCommitted, input.Status);
                var request = input.LastRequest;
                var token = input.View.PresentationToken;
                Assert.IsNotNull(token);
                panel.Detach();
                Assert.AreSame(request, input.LastRequest);
                Assert.AreSame(token, input.View.PresentationToken);
                Assert.IsFalse(rig.Session.Battle.IsDisposed);
                var files = rig.Files();
                panel.Rebind();
                Assert.AreSame(request, input.LastRequest);
                Assert.AreSame(token, input.View.PresentationToken);
                rig.Session.PausePresentation();
                Assert.IsNull(input.View.PresentationToken);
                rig.Session.PausePresentation();
                Assert.IsNull(input.View.PresentationToken);
                rig.SameFiles(files);
            }
            yield return new ExitPlayMode();
        }

        [UnityTest]
        public IEnumerator H04_RealBoardVictorySettlesOnceAndTheOriginalReceiptSurvivesWholeHostReconstruction()
        {
            yield return new EnterPlayMode();
            using (var rig = new HostRig())
            {
                string operation, commit;
                using (var panel = new HostPanel(rig))
                {
                    rig.Enter();
                    yield return panel.Ready();
                    for (var step = 0; rig.State.Phase != BattlePhase.WonPendingSettlement; step++)
                    {
                        Assert.Less(step, 64);
                        var state = rig.State;
                        Assert.That(state.Phase, Is.EqualTo(BattlePhase.AwaitAction).Or.EqualTo(BattlePhase.AwaitLinks));
                        if (state.Phase == BattlePhase.AwaitAction)
                            Assert.IsTrue(rig.Session.Battle.Input.SelectMember(state.Members.First(x => x.Hp.Numerator.Sign > 0).Member.CharacterId));
                        var pair = state.Phase == BattlePhase.AwaitLinks ? state.Board.PendingLinks[0] :
                            state.Enemies.First(x => x.Hp.Numerator.Sign > 0).PairKey;
                        panel.Draw(rig.Route(pair));
                        Assert.IsTrue(rig.Session.Battle.Input.LastResult.Application.IsCommitted, rig.Session.Battle.Input.Status);
                        rig.Session.PausePresentation();
                        yield return null;
                    }
                    var battle = rig.Session.Battle;
                    Assert.IsNotNull(rig.Head.Continuation);
                    var result = battle.Settle(panel.View.BattleView.Page, battle.Refresh().Context);
                    Assert.AreEqual(PlayerBattleRoute.Result, result.Route, result.Status);
                    Assert.IsNotNull(result.Receipt.Reward);
                    operation = result.OriginalIntent.OperationId;
                    commit = rig.Head.Header.CommitId;
                    battle.ReturnTo(panel.View.BattleView.Page, PlayerNavigationTargetKind.MapAdventure, result.Context);
                }
                var files = rig.Files();
                rig.Rebuild();
                using (var view = new HostPanel(rig))
                {
                    rig.Go(PlayerNavigationTargetKind.Bag);
                    rig.Go(PlayerNavigationTargetKind.OriginalOperation, operation);
                    rig.Session.Navigation.ActionHandler(PlayerNavigationAction.SelectOriginalOperation)();
                    Assert.AreEqual(FightMatchHostPage.Battle, rig.Session.Page);
                    Assert.IsNotNull(rig.Session.Battle.View.Receipt.Reward);
                    Assert.AreEqual(commit, rig.Head.Header.CommitId);
                }
                rig.SameFiles(files);
            }
            yield return new ExitPlayMode();
        }
        private static UnityEngine.UI.Button NavigationButton(HostPanel panel, string id)
        {
            var button = FightMatchViewId.Find<UnityEngine.UI.Button>(panel.Root.transform, id);
            Assert.IsNotNull(button, "Required real-prefab button: " + id);
            return button;
        }

        [TestCase("save")]
        [TestCase("marker-after")]
        public void H05_AdventureCannotDiscardConfirmationOrTheOriginalRecoveryOwner(string fault)
        {
            using (var rig = new HostRig())
            using (var panel = new HostPanel(rig))
            {
                rig.Enter();
                var adventure = NavigationButton(panel, FightMatchViewId.Row("battle-adventure"));
                var battle = rig.Session.Battle; var page = panel.View.BattleView.Page;
                var preview = battle.PreviewEnd(page, CandidateApplicationKind.RestartAttempt, battle.View.Context);
                Assert.IsNotNull(preview.Confirmation, preview.Status);
                var confirmation = preview.Confirmation; var files = rig.Files();
                adventure.onClick.Invoke();
                Assert.AreEqual(FightMatchHostPage.Battle, rig.Session.Page);
                Assert.AreSame(confirmation, battle.View.Confirmation); Assert.IsTrue(battle.Owns(page)); rig.SameFiles(files);
                rig.Storage.Fault = fault;
                var failed = battle.Confirm(page, confirmation);
                Assert.AreEqual(fault == "save" ? "SaveFailed" : "CommitUnknown", failed.Result.Code);
                var request = battle.Session.OriginalRequest; var intent = failed.OriginalIntent;
                var result = failed.Result; files = rig.Files();
                adventure.onClick.Invoke();
                Assert.AreEqual(FightMatchHostPage.Battle, rig.Session.Page);
                Assert.AreSame(request, battle.Session.OriginalRequest); Assert.AreSame(intent, battle.View.OriginalIntent);
                Assert.AreSame(result, battle.View.Result); Assert.AreEqual(PlayerBattleRoute.Recovery, battle.View.Route);
                Assert.IsTrue(battle.Owns(page)); rig.SameFiles(files);
            }
        }

    }
}
