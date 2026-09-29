using System;
using System.Linq;
using FightMatch.Application;
using FightMatch.Core;
using UnityEngine.UIElements;

namespace FightMatch.Presentation
{
    public sealed class PlayerBattleView : VisualElement, IDisposable
    {
        public PlayerBattleController Controller { get; private set; }
        public PlayerBattlePageToken Page { get; private set; }
        public CandidateBattlePlaybackView PlaybackView { get; } = new CandidateBattlePlaybackView();
        public PlayerDefaultReferenceView ReferenceView { get; } = new PlayerDefaultReferenceView();
        public CandidateRollbackRange DisplayedRange { get; private set; }
        public BattleSnapshot RollbackBeforeSnapshot => DisplayedRange?.BeforeSnapshot;
        private readonly Label headline = new Label { name = "battle-status" };
        private readonly Label hud = new Label { name = "battle-latest-hud" };
        private readonly VisualElement actions = new VisualElement { name = "battle-actions" };
        private readonly VisualElement history = new VisualElement { name = "battle-history" };
        private readonly VisualElement dialog = new VisualElement { name = "battle-confirmation" };
        private readonly VisualElement recovery = new VisualElement { name = "battle-recovery" };
        private readonly VisualElement receipt = new VisualElement { name = "battle-receipt" };
        private long renderEpoch;
        private bool disposed;
        public PlayerBattleView()
        {
            name = "player-battle-view"; style.flexGrow = 1;
            Add(headline); Add(hud); Add(actions); Add(PlaybackView); Add(history); Add(dialog); Add(recovery); Add(receipt); Add(ReferenceView);
            RegisterCallback<DetachFromPanelEvent>(e => Detach());
        }
        public void Bind(PlayerBattleController controller)
        {
            if (disposed) throw new ObjectDisposedException(nameof(PlayerBattleView));
            if (controller == null) throw new ArgumentNullException(nameof(controller));
            Detach(); Controller = controller; Page = controller.BindPage();
            PlaybackView.Bind(controller.Input, controller.Playback); controller.Changed += Render; Render();
        }
        public void Detach()
        {
            renderEpoch++; ReferenceView.Close();
            if (Controller != null) { Controller.Changed -= Render; Controller.DetachPage(Page); }
            PlaybackView.Detach(); Controller = null; Page = null; DisplayedRange = null;
            actions.Clear(); history.Clear(); dialog.Clear(); recovery.Clear(); receipt.Clear();
        }
        private Button Button(VisualElement parent, string name, string text, Action<PlayerBattleController, PlayerBattlePageToken> action, bool enabled = true)
        {
            var owner = Controller; var page = Page; var epoch = renderEpoch;
            var button = new Button(() =>
            { if (ReferenceEquals(owner, Controller) && epoch == renderEpoch && owner.Owns(page)) action(owner, page); }) { name = name, text = text };
            button.SetEnabled(enabled); parent.Add(button); return button;
        }
        private static void Text(VisualElement parent, string value, string name = null) { parent.Add(new Label(value) { name = name }); }
        private void Render()
        {
            if (Controller == null) return;
            if (!Controller.Owns(Page)) { Detach(); return; }
            renderEpoch++; var view = Controller.View; var context = view.Context; var battle = view.Battle;
            var pendingVictory = battle.BattleSnapshot?.Phase == BattlePhase.WonPendingSettlement;
            headline.text = pendingVictory ? "胜利已保存，基础奖励待结算" : view.Route == PlayerBattleRoute.Result ? "原战斗结果" :
                view.Status == "CreationRequired" ? "建档尚未完成，请继续原创建流程" : "战斗 · " + (view.Status ?? battle.BattleSnapshot?.Phase.ToString());
            hud.text = view.Read.IsAvailable ? "当前资料：" + string.Join(" | ", view.Read.Head.Business.Roster.Characters.Select(c =>
                c.CharacterId + " 等级 " + c.Level + " 经验 " + c.Experience)) + "；当前库存：" +
                string.Join(" | ", view.Read.Head.Business.Inventory.Holdings.Select(h => h.ItemId + " × " + h.T)) : "当前保存状态：" + view.Read.Application.Phase;
            actions.Clear(); history.Clear(); dialog.Clear(); recovery.Clear(); receipt.Clear();
            PlaybackView.style.display = view.Route == PlayerBattleRoute.Result || view.Route == PlayerBattleRoute.HostNavigation ? DisplayStyle.None : DisplayStyle.Flex;
            if (battle.History != null)
            {
                Button(actions, "battle-exit", "退出本局…", (c, p) => c.PreviewEnd(p, CandidateApplicationKind.ExitAttempt, context), view.Read.Lifecycle?.Exit.IsAvailable == true);
                Button(actions, "battle-restart", "按原开局重来…", (c, p) => c.PreviewEnd(p, CandidateApplicationKind.RestartAttempt, context), view.Read.Lifecycle?.Restart.IsAvailable == true);
                Button(actions, "battle-settle", "继续基础奖励结算", (c, p) => c.Settle(p, context), view.CanSettle);
                if (pendingVictory && !view.CanSettle) Text(actions, "待办：" + view.SettlementReason);
                Button(actions, "battle-reference", "开局默认参考", (c, p) =>
                {
                    var entry = c.View.Battle.BattleSnapshot.Baseline.Entry;
                    var reference = c.ReadReference(p, entry.Level.LevelId, entry.Level.LevelVersion, c.View.Context.Binding);
                    if (reference?.IsAvailable == true) ReferenceView.Show(c, p, PlaybackView.InputView.Board, reference.Value);
                    else headline.text = "参考不可用：" + reference?.Code;
                });
                foreach (var entry in Controller.HistoryEntries)
                {
                    var anchor = entry.HistoryAnchorId;
                    Button(history, "history-" + anchor, entry.FaceId + " · " + entry.Record.Kind + " · " + entry.Pair.PairId,
                        (c, p) => c.SelectHistory(p, anchor, context), battle.Rollback.IsAvailable);
                }
            }
            DisplayedRange = Controller.Input.RollbackPreview;
            if (DisplayedRange != null)
            {
                var range = DisplayedRange; var before = range.BeforeSnapshot;
                Text(dialog, "回退将移除 " + range.Entries.Count + " 项：" + string.Join(" → ", range.Entries.Select(x => x.FaceId + "/" + x.Record.Kind)));
                Text(dialog, "恢复前快照：面 " + before.Board.Face.FaceId + "，阶段 " + before.Phase + "，成员 " +
                    string.Join(" | ", before.Members.Select(m => m.Member.CharacterId + " HP " + CandidateBattlePlaybackFrame.Hp(m.Hp))) +
                    "；敌人 " + string.Join(" | ", before.Enemies.Select(e => e.PairKey.PairId + " HP " + CandidateBattlePlaybackFrame.Hp(e.Hp) + " 意图游标 " + e.IntentCursor)) +
                    "；固化路线 " + before.Board.LockedRoutes.Count + "；待补线 " + before.Board.PendingLinks.Count, "rollback-before");
                Button(dialog, "confirm-history", "确认回退这些行动", (c, p) => c.ConfirmHistory(p, range));
                Button(dialog, "cancel-history", "保留当前战局", (c, p) => c.CancelHistory(p, range));
            }
            if (view.Confirmation != null)
            {
                var frozen = view.Confirmation;
                Text(dialog, frozen.Kind == CandidateApplicationKind.ExitAttempt ? "确认退出本局？不会新建一局。" : "确认按原入场阵容、成长与随机初态重来？");
                Button(dialog, "confirm-battle-end", "确认", (c, p) => c.Confirm(p, frozen));
                Button(dialog, "cancel-battle-end", "继续当前局", (c, p) => c.Cancel(p, frozen));
            }
            if (view.Route == PlayerBattleRoute.Recovery)
            {
                Text(recovery, "保存待处理：" + view.Read.Application.Phase + "；原请求保持，奖励以核定结果为准。");
                if (view.OriginalIntent != null && view.Read.Application.PendingOperationId == view.OriginalIntent.OperationId)
                {
                    var original = view.OriginalIntent;
                    Button(recovery, "query-battle-original", "查询原结果", (c, p) => c.Continue(p, PlayerBattleRecoveryAction.Query, original, context));
                    Button(recovery, "retry-battle-original", "重试原保存", (c, p) => c.Continue(p, PlayerBattleRecoveryAction.Retry, original, context), view.CanRetry);
                    Button(recovery, "resolve-battle-original", "确认原保存结果", (c, p) => c.Continue(p, PlayerBattleRecoveryAction.Resolve, original, context), view.CanResolve);
                    Button(recovery, "end-battle-original", "结束未提交请求", (c, p) => c.Continue(p, PlayerBattleRecoveryAction.End, original, context),
                        view.Read.Head?.Continuation == null && original.Kind != CandidateApplicationKind.SettleVictory);
                }
                foreach (var commit in view.Read.Application.ObservedCandidateCommitIds)
                { var candidate = commit; Button(recovery, "resume-" + candidate, "继续观察到的候选 " + candidate, (c, p) => c.ResumeObserved(p, candidate, context)); }
                var inputRequest = Controller.Input.LastRequest;
                if (inputRequest != null && view.Read.Application.PendingOperationId == inputRequest.OperationId)
                {
                    Text(recovery, "棋盘原请求可用上方“重试保存／确认保存结果”继续。");
                    Button(recovery, "query-input-original", "查询棋盘原结果", (c, p) =>
                    { if (ReferenceEquals(inputRequest, c.Input.LastRequest)) c.Input.QueryLastOperation(); });
                    Button(recovery, "end-input-original", "结束未提交的棋盘请求", (c, p) =>
                    { if (ReferenceEquals(inputRequest, c.Input.LastRequest)) c.Input.EndLast(); }, view.Read.Head?.Continuation == null);
                }
            }
            if (view.Route == PlayerBattleRoute.Result && view.Receipt != null)
            {
                var original = view.Receipt;
                Text(receipt, original.Reward == null ? "本局已结束" : "基础奖励已到账", "receipt-title");
                Text(receipt, "关卡 " + original.LevelId + " v" + original.LevelVersion + "；本次结果保留原到账事实。");
                foreach (var member in original.Members)
                    Text(receipt, "原槽 " + member.OriginalSlot + " · " + member.CharacterId + "：基础奖励经验 " + member.Reward?.Amount +
                        "，已记经验 " + member.Experience?.Amount + "；结束 " + member.End?.Kind + "，倒下 " + member.End?.WasDown +
                        "，恢复记录 " + (member.End?.RecoveryId ?? "无"), "receipt-member-" + member.CharacterId);
                Text(receipt, "原材料到账：" + (original.InventoryGrant == null ? "无" :
                    string.Join(" | ", original.InventoryGrant.Items.Select(x => x.ItemId + " × " + x.Quantity))), "receipt-inventory");
                Button(receipt, "result-map", "回地图", (c, p) => c.ReturnTo(p, PlayerNavigationTargetKind.MapAdventure, context));
                Button(receipt, "result-team", "查看当前队伍", (c, p) => c.ReturnTo(p, PlayerNavigationTargetKind.Team, context));
                Button(receipt, "result-bag", "查看当前背包", (c, p) => c.ReturnTo(p, PlayerNavigationTargetKind.Bag, context));
                Button(receipt, "result-replay", "重新入场挑战本关", (c, p) => c.Replay(p, original.LevelId, original.LevelVersion, context));
                if (view.NextLevels.Count == 0) Text(receipt, "本内容段结束，暂无已发布且开放的下一关。", "no-next-level");
                foreach (var level in view.NextLevels)
                { var next = level; Button(receipt, "next-" + next.LevelId, "挑战 " + next.LevelId, (c, p) => c.Replay(p, next.LevelId, next.LevelVersion, context)); }
            }
        }
        public void ClosePresentation() { if (Controller?.Owns(Page) == true) Controller.SkipPlayback(Page); Detach(); }
        public void Dispose() { if (disposed) return; Detach(); PlaybackView.Dispose(); ReferenceView.Dispose(); disposed = true; }
    }
}
