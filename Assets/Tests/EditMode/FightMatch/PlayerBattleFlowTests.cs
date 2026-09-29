using System.Linq;
using FightMatch.Application;
using NUnit.Framework;
using UnityEngine.UIElements;
using static FightMatch.Core.Tests.PlayerSessionTestData;
using static FightMatch.Core.Tests.PlayerRosterTestData;
using static FightMatch.Core.Tests.BusinessSaveScenario;

namespace FightMatch.Core.Tests
{
    public sealed class PlayerBattleFlowTests
    {
        [Test] public void B01_HostSelectionIsOwnerBoundConsumedOnceAndUsesRealFormationEntry()
        {
            using (var r = new PlayerBattleRig(false)) using (var foreign = new PlayerBattleRig(false))
            {
                Assert.AreSame(r.Session, r.N.Player.GetBattleSession()); var request = r.Selection();
                var head = r.Head; var files = r.N.Files(); var calls = r.ClockReads;
                Assert.AreEqual("StaleHostRequest", r.Host.AcceptHost(foreign.Selection()).Status); r.Unchanged(head, files, calls);
                var done = r.Host.AcceptHost(request); Is(done.Result); Assert.AreEqual(calls + 1, r.ClockReads);
                Assert.AreEqual(head.Records.Count + 1, r.Head.Records.Count); Assert.AreEqual(CandidateApplicationKind.EnterFormation, done.OriginalIntent.Kind);
                head = r.Head; files = r.N.Files(); calls = r.ClockReads;
                Assert.AreEqual("StaleHostRequest", r.Host.AcceptHost(request).Status); r.Unchanged(head, files, calls);
                var resume = r.N.Go(PlayerNavigationTargetKind.ResumeBattleRequested).HostRequest;
                Assert.AreEqual(PlayerBattleRoute.Battle, r.Host.AcceptHost(resume).Route); r.Unchanged(head, files, calls);
            }
        }
        [Test] public void B01_StaleHeadAndRootBackNeverPrepareABattle()
        {
            using (var r = new PlayerBattleRig(false))
            {
                var root = r.N.Go(PlayerNavigationTargetKind.RootBackRequested).HostRequest;
                var head = r.Head; var files = r.N.Files(); var calls = r.ClockReads;
                Assert.AreEqual(PlayerBattleRoute.HostNavigation, r.Host.AcceptHost(root).Route); r.Unchanged(head, files, calls);
                var selected = r.Selection();
                Is(r.N.Life.Submit(Prepared(r.N.Player.PrepareFormation(new PlayerFormationDraft { ExpectedCommitId = head.Header.CommitId,
                    ExpectedFormationRevision = head.Business.Roster.FormationRevision, Slots = new[] { null, "W", null } }, Codec())), PlayerSessionTestData.Budget()));
                head = r.Head; files = r.N.Files();
                Assert.AreEqual("StaleHostRequest", r.Host.AcceptHost(selected).Status); r.Unchanged(head, files, calls);
            }
        }
        [Test] public void B01_DueRecoveryPassesStaticNoReadyHintAndIsCompletedInsideH02()
        {
            using (var r = PlayerBattleRig.Recoverable())
            {
                r.Host.Input.SelectMember("W"); PlayerBattleRig.Draw(r.Host.Input, r.Route(r.State.Enemies[1].PairKey));
                Is(r.Host.Input.LastResult.Application); r.Host.Playback.SkipToFinal();
                Assert.AreEqual(0, r.State.Members[0].Hp.Numerator.Sign); Is(r.End().Result);
                Assert.IsFalse(r.Head.Business.Roster.Find("W").IsReady); r.Elapsed = 1000000;
                r.N.PreparePage(); Assert.AreEqual("NoReadyMember", r.N.View.Read.Lifecycle.Enter.Reason);
                var request = r.N.Go(PlayerNavigationTargetKind.BattleSelectionRequested).HostRequest;
                var count = r.Head.Records.Count; var actual = r.Host.AcceptHost(request); Is(actual.Result);
                Assert.AreEqual(count + 1, r.Head.Records.Count); Assert.AreEqual(CandidateApplicationKind.EnterFormation, actual.OriginalIntent.Kind);
                Assert.IsTrue(r.Head.Business.Roster.Find("W").IsReady); Assert.AreEqual("W", r.State.Members.Single().Member.CharacterId);
                Assert.IsNotEmpty(actual.Result.OriginalLookup.RecoveryResults);
            }
        }
        [Test] public void B02_ActualMultiFaceHistoryListRetainsOriginalRangeAndCommitsItsConfirmationOnce()
        {
            using (var r = PlayerBattleRig.TwoFaces())
            {
                var first = r.State.Board.Face.FaceId;
                for (var i = 0; r.State.Board.Face.FaceId == first; i++) { Assert.Less(i, 32); Is(r.Step().Application); }
                Is(r.Step().Application); var head = r.Head; var files = r.N.Files(); var calls = r.ClockReads;
                var anchor = r.Host.HistoryEntries[0].HistoryAnchorId;
                var range = r.Host.SelectHistory(r.Page.Page, anchor, r.View.Context).Range;
                Assert.Greater(range.Entries.Select(x => x.FaceId).Distinct().Count(), 1); Assert.AreSame(range, r.Page.DisplayedRange);
                Assert.AreSame(range.BeforeSnapshot, r.Page.RollbackBeforeSnapshot); r.Unchanged(head, files, calls);
                var other = r.Host.SelectHistory(r.Page.Page, r.Host.HistoryEntries[1].HistoryAnchorId, r.View.Context).Range;
                Assert.IsNull(r.Host.ConfirmHistory(r.Page.Page, range)); Assert.AreSame(other, r.Host.Input.RollbackPreview);
                r.Host.CancelHistory(r.Page.Page, other); r.Unchanged(head, files, calls);
                range = r.Host.SelectHistory(r.Page.Page, anchor, r.View.Context).Range;
                Is(r.Host.ConfirmHistory(r.Page.Page, range).Application);
                Assert.AreEqual(first, r.State.Board.Face.FaceId); Same(range.BeforeSnapshot.Members, r.State.Members);
                Same(range.BeforeSnapshot.Enemies, r.State.Enemies); Same(range.BeforeSnapshot.Random, r.State.Random);
                CollectionAssert.AreEqual(range.Entries.Select(x => x.OperationId), r.Head.Business.ActiveHistory.RollbackRecords.Last().Range.Entries.Select(x => x.OperationId));
                head = r.Head; files = r.N.Files(); Assert.IsNull(r.Host.ConfirmHistory(r.Page.Page, range)); r.Unchanged(head, files, calls);
                var stale = r.Host.SelectHistory(r.Page.Page, anchor, r.View.Context); Assert.IsFalse(stale.IsAccepted);
            }
        }
        [TestCase(false)] [TestCase(true)]
        public void B03_EndPreviewCancelAndOldConfirmationAreZeroWrite(bool restart)
        {
            using (var r = new PlayerBattleRig())
            {
                var head = r.Head; var files = r.N.Files(); var clocks = r.ClockReads; var current = r.View;
                var first = r.Host.PreviewEnd(r.Page.Page, restart ? CandidateApplicationKind.RestartAttempt : CandidateApplicationKind.ExitAttempt, current.Context).Confirmation;
                Assert.IsNotNull(first); r.Host.Cancel(r.Page.Page, first); r.Unchanged(head, files, clocks);
                var second = r.Host.PreviewEnd(r.Page.Page, CandidateApplicationKind.ExitAttempt, r.View.Context).Confirmation;
                Assert.AreEqual("StaleConfirmation", r.Host.Confirm(r.Page.Page, first).Status); r.Unchanged(head, files, clocks);
                r.Host.Cancel(r.Page.Page, second); r.Unchanged(head, files, clocks);
            }
        }
        [Test] public void B03_RestartPreservesOriginalMembersGrowthAndThreeRandomInitialsWithoutH02()
        {
            using (var r = new PlayerBattleRig())
            {
                var original = r.Head.Business.ActiveHistory.CurrentRun; Is(r.Step().Application);
                var entries = r.Head.Records.Count(x => x.Intent.Kind == CandidateApplicationKind.EnterFormation); var clocks = r.ClockReads;
                Is(r.End(true).Result); var next = r.Head.Business.ActiveHistory.CurrentRun;
                Assert.AreEqual(clocks, r.ClockReads); Assert.AreEqual(entries, r.Head.Records.Count(x => x.Intent.Kind == CandidateApplicationKind.EnterFormation));
                Assert.AreNotEqual(original.Baseline.Entry.AttemptId, next.Baseline.Entry.AttemptId);
                Assert.AreEqual(original.Baseline.Entry.EntryBaselineId, next.Baseline.Entry.EntryBaselineId);
                Assert.AreEqual(original.Baseline.Entry.ChallengeId, next.Baseline.Entry.ChallengeId);
                Same(original.Baseline.Entry.Members, next.Baseline.Entry.Members);
                foreach (var pair in new[] { new[] { original.Binding.Battle, next.Binding.Battle }, new[] { original.Binding.BaseReward, next.Binding.BaseReward }, new[] { original.Binding.Bonus, next.Binding.Bonus } })
                { Assert.AreEqual(pair[0].InitState, pair[1].InitState); Assert.AreEqual(pair[0].InitSequence, pair[1].InitSequence); Same(pair[0].Initial, pair[1].Initial); }
                Assert.IsEmpty(next.Records); Assert.IsEmpty(r.Head.Business.Rewards.BaseRewards);
            }
        }
        [Test] public void B03_ExitConfirmsOneOriginalRequestAndCreatesNoAttempt()
        {
            using (var r = new PlayerBattleRig())
            {
                var count = r.Head.Business.Progression.Challenges.Sum(x => x.Attempts.Count);
                var preview = r.Host.PreviewEnd(r.Page.Page, CandidateApplicationKind.ExitAttempt, r.View.Context);
                var frozen = preview.Confirmation; Is(r.Host.Confirm(r.Page.Page, frozen).Result); var request = r.Session.OriginalRequest;
                Assert.IsNull(r.Head.Business.ActiveHistory); Assert.AreEqual(count, r.Head.Business.Progression.Challenges.Sum(x => x.Attempts.Count));
                var head = r.Head; var files = r.N.Files(); var clocks = r.ClockReads;
                Assert.AreEqual("StaleConfirmation", r.Host.Confirm(r.Page.Page, frozen).Status); r.Unchanged(head, files, clocks);
                Assert.AreSame(request, r.Session.OriginalRequest); Assert.IsNull(r.View.Receipt.Reward);
            }
        }
        [Test] public void B04_FirstPlaybackFinishCallbackStillRejectsSettlementUntilActualTokenRelease()
        {
            using (var r = new PlayerBattleRig())
            {
                r.Win(false); var head = r.Head; var files = r.N.Files(); var clocks = r.ClockReads; var reserved = head.Continuation.ReservedOperationId;
                Assert.IsNotNull(r.N.Battle.QueryView().PresentationToken); Assert.IsFalse(r.View.CanSettle);
                Assert.AreEqual("PresentationPending", r.Settle().Status); r.Unchanged(head, files, clocks);
                var observed = false;
                r.Host.Playback.Changed += () =>
                {
                    if (observed || r.Host.Playback.IsPlaying || r.N.Battle.QueryView().PresentationToken == null) return;
                    observed = true; Assert.AreEqual("PresentationPending", r.Settle().Status); r.Unchanged(head, files, clocks);
                };
                r.Host.Playback.SkipToFinal(); Assert.IsTrue(observed); Assert.IsNull(r.N.Battle.QueryView().PresentationToken);
                r.Unchanged(head, files, clocks); var done = r.Settle(); r.Receipt(done);
                Assert.IsNull(done.Result.OriginalLookup.TerminalRun, "Victory lookup resolves the saved binding and reward, not an exit terminal run");
                Assert.AreEqual(head.Business.ActiveHistory.Binding.Start.Baseline.Entry.EntryBaselineId,
                    done.Result.OriginalLookup.Baseline.Entry.EntryBaselineId);
                Assert.AreEqual(reserved, done.OriginalIntent.OperationId); Assert.IsNull(r.Head.Business.ActiveHistory);
                Assert.IsNull(r.Head.Continuation); Assert.AreEqual(1, r.Head.Business.Rewards.BaseRewards.Count);
                Assert.AreEqual(clocks + 1, r.ClockReads); var settled = r.Head; var saved = r.N.Files();
                var afterClocks = r.ClockReads; r.Settle(); r.Unchanged(settled, saved, afterClocks);
            }
        }
        [Test] public void B05_ThreeMemberReceiptMapsFullOriginalLookupByCharacterAndEntrySlot()
        {
            var isolated = new IsolatedRoster(40); isolated.Commit(isolated.EntryRequest());
            isolated.Attack(); isolated.Attack(); isolated.Attack("B", 1); isolated.Attack("B", 1);
            var request = isolated.Settle(); var lookup = TakeCore(CandidateApplicationProtocol.Lookup(isolated.Head, request.Intent, Codec()));
            var receipt = new PlayerBattleReceipt(lookup); Assert.IsNull(isolated.Head.Business.ActiveHistory);
            CollectionAssert.AreEqual(new[] { "C", "A", "B" }, receipt.Members.Select(x => x.CharacterId));
            CollectionAssert.AreEqual(new[] { 0, 1, 2 }, receipt.Members.Select(x => x.OriginalSlot));
            foreach (var member in receipt.Members)
            {
                Assert.AreSame(lookup.CharacterExperiences.Single(x => x.CharacterId == member.CharacterId), member.Experience);
                Assert.AreSame(lookup.CharacterEnds.Single(x => x.CharacterId == member.CharacterId), member.End);
                Assert.AreSame(lookup.Reward.Experience.Single(x => x.CharacterId == member.CharacterId), member.Reward);
                Assert.AreEqual(member.Reward.Amount, member.Experience.Amount); Assert.AreEqual(receipt.AttemptId, member.End.AttemptId);
            }
            Assert.IsTrue(receipt.Members[0].End.WasDown); Assert.IsTrue(receipt.Members[1].End.WasDown);
            Assert.IsFalse(receipt.Members[2].End.WasDown); Assert.AreSame(lookup.InventoryGrant, receipt.InventoryGrant);
            Assert.AreEqual(receipt.SettlementId, receipt.InventoryGrant.SettlementId);
        }
        [TestCase(PlayerNavigationTargetKind.MapAdventure)] [TestCase(PlayerNavigationTargetKind.Team)] [TestCase(PlayerNavigationTargetKind.Bag)]
        public void B12_ResultDestinationsUseLatestSamePlayerAndRealPublishedEmptyContent(PlayerNavigationTargetKind target)
        {
            using (var r = new PlayerBattleRig())
            {
                r.Win(); var result = r.Settle(); r.Receipt(result);
                Assert.IsEmpty(result.NextLevels); StringAssert.Contains("暂无", r.Page.Q<Label>("no-next-level").text);
                var head = r.Head; var files = r.N.Files(); var clocks = r.ClockReads;
                Assert.AreEqual(PlayerBattleRoute.HostNavigation, r.Host.ReturnTo(r.Page.Page, target, r.View.Context).Route);
                r.Unchanged(head, files, clocks); Assert.AreEqual(head.Business.PlayerId, r.N.View.Context.PlayerId);
                r.Receipt(r.Host.OpenOriginalResult(result.Receipt.OperationId, r.View.Context)); r.Unchanged(head, files, clocks);
                if (target == PlayerNavigationTargetKind.Bag)
                { r.N.Go(PlayerNavigationTargetKind.CraftList); Assert.AreEqual("NoPublishedDefinition", r.N.Player.QueryPermanent("W").RecipeAvailability); }
            }
        }
        [Test] public void B12_ResultReplayCreatesANewH02AndCannotInventNextLevel()
        {
            using (var r = new PlayerBattleRig())
            {
                r.Win(); var done = r.Settle(); var receipt = done.Receipt; var head = r.Head; var files = r.N.Files(); var clocks = r.ClockReads;
                Assert.AreEqual("LevelLocked", r.Host.Replay(r.Page.Page, "invented-next", "1", r.View.Context).Status); r.Unchanged(head, files, clocks);
                var replay = r.Host.Replay(r.Page.Page, receipt.LevelId, receipt.LevelVersion, r.View.Context); Is(replay.Result);
                Assert.AreEqual(CandidateApplicationKind.EnterFormation, replay.OriginalIntent.Kind);
                Assert.AreNotEqual(receipt.AttemptId, r.State.Baseline.Entry.AttemptId); Assert.AreEqual(head.Records.Count + 1, r.Head.Records.Count);
                Assert.AreEqual(1, r.Head.Business.Rewards.BaseRewards.Count);
            }
        }
    }
}
