using System;
using System.Collections.Generic;
using FightMatch.Platform;
using FightMatch.Presentation;
using UnityEngine;
using UnityEngine.UIElements;
using NavigationView = FightMatch.Presentation.PlayerNavigationView;

namespace FightMatch.Host
{
    public sealed class FightMatchHostView : VisualElement, IDisposable
    {
        private readonly FightMatchHostSession session;
        private readonly VisualElement startup = new VisualElement { name = "host-startup" };
        private readonly VisualElement quit = new VisualElement { name = "host-quit" };
        private readonly ScrollView navigation = new ScrollView { name = "host-navigation" };
        private readonly ScrollView battle = new ScrollView { name = "host-battle" };
        private readonly ScrollView license = new ScrollView { name = "host-license" };
        private readonly NavigationView navigationView;
        public PlayerBattleView BattleView { get; } = new PlayerBattleView();
        private readonly Label status = new Label { name = "host-status" };
        private readonly Button create, resume;
        private bool disposed;
        private static readonly Dictionary<string, string> Captions = new Dictionary<string, string>
        {
            { "nav-MapAdventure", "地图冒险" }, { "nav-Team", "队伍" }, { "nav-Bag", "背包" },
            { "nav-CraftList", "合成" }, { "nav-Back", "返回" }, { "nav-Cancel", "取消" },
            { "nav-query", "刷新当前资料" }, { "nav-battle", "确认入场条件" },
            { "nav-ResumeBattleRequested", "继续当前战斗" }, { "nav-SettlementRequired", "继续基础奖励结算" },
            { "nav-CreationRequired", "继续建档" }, { "team-preview", "检查阵容修改" },
            { "bag-original-result", "查看选中的原结果" }, { "save-Confirm", "确认" },
            { "save-Return", "返回" }, { "save-Retry", "重试原保存" }, { "save-Resolve", "确认原保存结果" },
            { "save-ResumeObserved", "继续选中的原保存" }, { "save-Refresh", "重新读取保存状态" },
            { "save-end-review", "检查未提交请求" }, { "save-End", "确认结束未提交请求" },
            { "save-SelectOriginalOperation", "查看选中的原结果" }, { "equip-empty", "卸下装备" },
            { "preference", "道具使用偏好" }
        };

        public FightMatchHostView(FightMatchHostSession session, string fontLicense)
        {
            this.session = session ?? throw new ArgumentNullException(nameof(session));
            name = "fightmatch-host";
            style.flexGrow = 1;
            var header = new VisualElement { name = "host-header" };
            header.Add(new Label("FightMatch · 冒险"));
            AddButton(header, "host-back", "返回 / 退出应用", session.Back);
            AddButton(header, "host-license-open", "字体许可", () => ShowLicense(true));
            Add(header);
            startup.Add(new Label("本机冒险资料"));
            startup.Add(status);
            create = AddButton(startup, "host-create", "创建本机资料", session.CreateProfile);
            resume = AddButton(startup, "host-continue", "继续原创建", session.ContinueCreation);
            AddButton(startup, "host-observe", "重新读取", session.ObserveStartup);
            startup.Add(new Label("已有资料会保留。保存待处理时，请继续原请求。"));
            Add(startup);
            navigationView = new NavigationView(session.Navigation);
            navigation.Add(navigationView);
            Add(navigation);
            BattleView.Bind(session.Battle);
            battle.Add(BattleView);
            Add(battle);
            quit.Add(new Label("退出应用？当前已保存的进度会保留。未完成的保存将在下次继续。"));
            AddButton(quit, "host-quit-confirm", "退出应用", session.ConfirmQuit);
            AddButton(quit, "host-quit-cancel", "留在游戏", session.CancelQuit);
            Add(quit);
            AddButton(license, "host-license-close", "返回游戏", () => ShowLicense(false));
            license.Add(new Label(fontLicense ?? "字体许可资源缺失"));
            license.style.display = DisplayStyle.None;
            Add(license);
            session.Changed += Render;
            session.Navigation.Changed += PolishNavigation;
            session.Battle.Changed += LayoutBoard;
            RegisterCallback<GeometryChangedEvent>(GeometryChanged);
            RegisterCallback<DetachFromPanelEvent>(Detached);
            Render();
            PolishNavigation();
        }

        private static Button AddButton(VisualElement parent, string name, string text, Action action)
        {
            var button = new Button(action) { name = name, text = text };
            parent.Add(button);
            return button;
        }

        private void ShowLicense(bool show)
        {
            if (disposed) return;
            if (show) session.PausePresentation();
            license.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
            Render();
        }

        private void Render()
        {
            if (disposed) return;
            var covered = license.style.display.value == DisplayStyle.Flex;
            startup.style.display = !covered && session.Page == FightMatchHostPage.Startup ? DisplayStyle.Flex : DisplayStyle.None;
            navigation.style.display = !covered && session.Page == FightMatchHostPage.Navigation ? DisplayStyle.Flex : DisplayStyle.None;
            battle.style.display = !covered && session.Page == FightMatchHostPage.Battle ? DisplayStyle.Flex : DisplayStyle.None;
            quit.style.display = !covered && session.Page == FightMatchHostPage.QuitConfirmation ? DisplayStyle.Flex : DisplayStyle.None;
            status.text = session.CanCreate ? "尚无冒险资料，可以创建。" : session.Status == "ContinueCreation" ?
                "上次创建尚未完成，请继续原创建。" : session.Status == "UnclaimedData" ?
                "发现已有资料，暂不能新建。请保留数据并检查保存状态。" : "保存状态：" + session.Status;
            if (session.StorageError != null) status.text += "\n读取失败：" + session.StorageError;
            create.SetEnabled(session.CanCreate);
            resume.SetEnabled(session.OriginalProfile != null ||
                session.Observation?.State == LocalPlayerProfileState.CreateIntentRecorded);
            LayoutBoard();
        }

        private void PolishNavigation()
        {
            if (disposed) return;
            navigationView.Query<Button>().ForEach(button =>
            {
                if (button.name == "nav-Back" && session.AtNavigationRoot) button.style.display = DisplayStyle.None;
                if (Captions.TryGetValue(button.name ?? "", out var caption)) button.text = caption;
                else if (button.name?.StartsWith("level-", StringComparison.Ordinal) == true)
                    button.text = button.text.Replace(" Locked", " · 未开放").Replace(" Open", " · 可挑战");
                else if (button.name == "nav-migration")
                {
                    var format = (uint)session.Navigation.View.Read.Head.Business.Format;
                    button.text = "检查存档格式升级 " + format + " → " + (format + 1);
                }
                else if (button.text.StartsWith("Select original ", StringComparison.Ordinal)) button.text = button.text.Replace("Select original ", "选择原操作 ");
                else if (button.text.StartsWith("Select candidate ", StringComparison.Ordinal)) button.text = button.text.Replace("Select candidate ", "选择原保存候选 ");
                else if (button.text.StartsWith("Equip ", StringComparison.Ordinal)) button.text = button.text.Replace("Equip ", "装备 ");
            });
            for (var i = 0; i < 3; i++)
            {
                var slot = navigationView.Q<DropdownField>("team-slot-" + i);
                if (slot != null) slot.label = "队伍槽 " + (i + 1);
            }
            var route = navigationView.Q<Label>("navigation-route");
            if (route != null) route.text = PlayerText(session.Navigation.View.Route.ToString());
            navigationView.Query<Label>().ForEach(label => label.text = PlayerText(label.text));
            var availability = navigationView.Q<Label>("recipe-availability");
            if (availability?.text == "NoPublishedRecipes") availability.text = "当前内容暂无已发布配方。";
        }

        private static string PlayerText(string value)
        {
            switch (value)
            {
                case "MapAdventure": return "地图冒险";
                case "Preparation": return "入场准备";
                case "Team": return "当前队伍";
                case "Bag": return "当前背包";
                case "CraftList": return "合成";
                case "Detail": return "角色与道具";
                case "Confirmation": return "确认操作";
                case "Recovery": return "保存恢复";
                case "CommittedResult": return "原操作结果";
                case "Gate": return "资料状态";
                case "Ready": return "当前资料已读取";
                case "EntryCheckRequired": return "选择角色后检查入场条件。";
                case "ActorSelectionRequired": return "请先选择角色。";
                case "NoPublishedRecipes": return "当前内容暂无已发布配方。";
                case "NoPublishedDefinition": return "当前没有已发布关卡。";
                case "ActiveAttemptConflict": return "请先处理当前战斗。";
                case "ResolutionRequired": return "保存待处理，请继续原请求。";
                case "SaveFailed": return "保存未完成，可重试原保存。";
                case "CommitUnknown": return "正在核定原保存结果，请勿新建资料。";
                case "NoItemEquipped": return "尚未装备道具。";
            }
            return (value ?? "").Replace("Selected: ", "已选角色：").Replace("None", "未选择")
                .Replace("Recovery periods: ", "恢复时段：").Replace("; revision ", "；修订 ")
                .Replace("Requires: ", "开放条件：").Replace("; cleared: ", "；已通关：")
                .Replace("Before: ", "修改前：").Replace("After: ", "修改后：").Replace("Empty", "空槽")
                .Replace(": total ", "：总数 ").Replace(", equipped ", "，已装备 ")
                .Replace(", reserved ", "，预留 ").Replace(", free ", "，可用 ").Replace("Battle carry ", "本局携带 ");
        }

        private void GeometryChanged(GeometryChangedEvent evt) { LayoutBoard(); }
        private void LayoutBoard()
        {
            var board = BattleView.PlaybackView.InputView.Board;
            if (board == null || !float.IsFinite(resolvedStyle.width) || resolvedStyle.width <= 0) return;
            board.style.height = Mathf.Clamp(resolvedStyle.width - 32, 240, 720);
            board.style.flexGrow = 0;
            board.style.flexShrink = 0;
        }

        private void Detached(DetachFromPanelEvent evt)
        {
            if (evt.target == this) Dispose();
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            session.Changed -= Render;
            session.Navigation.Changed -= PolishNavigation;
            session.Battle.Changed -= LayoutBoard;
            UnregisterCallback<GeometryChangedEvent>(GeometryChanged);
            UnregisterCallback<DetachFromPanelEvent>(Detached);
            navigationView.Dispose();
            BattleView.Dispose();
        }
    }
}
