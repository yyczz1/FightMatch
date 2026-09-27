using System;
using System.Collections.Generic;
using System.Linq;
using FightMatch.Application;
using FightMatch.Core;
using FightMatch.Platform;
using UnityEngine.UIElements;

namespace FightMatch.Presentation
{
    public sealed class CandidateBoardInputView : VisualElement
    {
        private readonly Label phase = new Label { name = "phase-status" };
        private readonly Label save = new Label { name = "save-status" };
        private readonly Label enemies = new Label { name = "enemy-status" };
        private readonly Label availability = new Label { name = "input-status" };
        private readonly VisualElement members = new VisualElement { name = "members" };
        private readonly Dictionary<string, Button> buttons = new Dictionary<string, Button>(StringComparer.Ordinal);
        private readonly IVisualElementScheduledItem refresh;
        private readonly Button retry, resolve;
        public CandidateBoardInputController Controller { get; private set; }
        public CandidateBoardElement Board { get; private set; }

        public CandidateBoardInputView()
        {
            name = "candidate-input-view"; style.flexGrow = 1;
            members.style.flexDirection = FlexDirection.Row;
            retry = new Button(() => Controller?.RetryLast()) { text = "重试保存", name = "retry-save" };
            resolve = new Button(() => Controller?.ResolveLast()) { text = "确认保存结果", name = "resolve-save" };
            Add(phase); Add(save); Add(members); Add(enemies); Add(availability); Add(retry); Add(resolve);
            refresh = schedule.Execute(() => Controller?.Refresh()).Every(100);
            RegisterCallback<AttachToPanelEvent>(e => { refresh.Resume(); Controller?.Refresh(); });
            RegisterCallback<DetachFromPanelEvent>(e => Close());
        }

        public void Attach(CandidateBattleApplicationSystem system, SaveStoreBudget budget)
        {
            Close();
            if (Controller != null) Controller.Changed -= Render;
            Board?.RemoveFromHierarchy();
            Controller = new CandidateBoardInputController(system, budget);
            Board = new CandidateBoardElement(Controller); Add(Board);
            Controller.Changed += Render; Render();
            if (panel != null) refresh.Resume();
        }

        public void Close()
        { refresh.Pause(); Controller?.CancelGesture(); Controller?.CancelRollback(); }

        private static string Hp(ExactRational value)
        { return value.Denominator.IsOne ? value.Numerator.ToString() : value.Numerator + "/" + value.Denominator; }

        private void Render()
        {
            var view = Controller.View; var state = view.BattleSnapshot;
            phase.text = "阶段：" + (state == null ? view.Code : state.Phase.ToString());
            save.text = "保存：" + view.Phase + (view.IsPublishedHeadVerified ? "" : "（当前头未核定）") +
                (view.ApplicationView.PendingOperationId == null ? "" : "；有未完成保存请求");
            var currentMembers = state?.Members;
            foreach (var id in buttons.Keys.ToArray())
                if (currentMembers == null || !currentMembers.Any(x => x.Member.CharacterId == id)) { buttons[id].RemoveFromHierarchy(); buttons.Remove(id); }
            if (currentMembers != null) foreach (var member in currentMembers)
            {
                var id = member.Member.CharacterId;
                if (!buttons.TryGetValue(id, out var button))
                { button = new Button(() => Controller.SelectMember(id)) { name = "member-" + id }; buttons.Add(id, button); members.Add(button); }
                button.text = (Controller.SelectedCharacterId == id ? "● " : "") + id + " HP " + Hp(member.Hp);
                button.SetEnabled(member.Hp.Numerator.Sign > 0);
            }
            enemies.text = state == null ? "" : string.Join(" | ", state.Enemies.Select(x => x.PairKey.PairId + " HP " + Hp(x.Hp)));
            availability.text = "攻击：" + (view.Attack.Reason ?? "可用") + "；补线：" + (view.Link.Reason ?? "可用") +
                "；回退：" + (view.Rollback.Reason ?? "可用") + (Controller.Status == null ? "" : "；" + Controller.Status);
            retry.SetEnabled(Controller.CanRetry); resolve.SetEnabled(Controller.CanResolve);
        }
    }
}
