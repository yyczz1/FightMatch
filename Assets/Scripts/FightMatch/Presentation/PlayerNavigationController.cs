using System;
using FightMatch.Application;
using FightMatch.Platform;
using NavigationView = FightMatch.Application.PlayerNavigationView;

namespace FightMatch.Presentation
{
    // Owns callbacks only. The PlayerSession retains the single navigation operation across views.
    public sealed class PlayerNavigationController : IDisposable
    {
        private readonly PlayerNavigationSession session;
        private readonly SaveStoreBudget budget;
        private readonly Func<SaveStoreBudget> createBudget;
        private long epoch;
        private bool closed;
        private PlayerNavigationHostRequest delivered;
        public NavigationView View { get; private set; }
        public event Action Changed;
        public event Action<PlayerNavigationHostRequest> HostRequested;

        public PlayerNavigationController(PlayerSessionSystem player, SaveStoreBudget budget)
            : this(player, budget, () => budget) { }

        internal PlayerNavigationController(PlayerSessionSystem player, SaveStoreBudget budget, Func<SaveStoreBudget> createBudget)
        {
            if (player == null) throw new ArgumentNullException(nameof(player));
            this.budget = budget ?? throw new ArgumentNullException(nameof(budget));
            this.createBudget = createBudget ?? throw new ArgumentNullException(nameof(createBudget));
            session = player.GetNavigationSession();
            View = session.Query(createBudget().Codec);
        }
        public void Refresh()
        {
            if (!closed) Apply(session.Query(createBudget().Codec), false);
        }
        private void Apply(NavigationView next, bool dispatch)
        {
            View = next; epoch++;
            var request = dispatch && next.HostRequest != null && !ReferenceEquals(delivered, next.HostRequest)
                ? next.HostRequest : null;
            if (request != null) delivered = request;
            Changed?.Invoke();
            if (request != null && !closed) HostRequested?.Invoke(request);
        }
        public Action NavigationHandler(PlayerNavigationTarget target)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            var frozen = new PlayerNavigationTarget { Kind = target.Kind, LevelId = target.LevelId,
                LevelVersion = target.LevelVersion, Binding = target.Binding, CharacterId = target.CharacterId,
                DefinitionId = target.DefinitionId, PermanentKind = target.PermanentKind,
                CommitId = target.CommitId, OperationId = target.OperationId };
            var context = View.Context; var binding = epoch;
            return () => {
                if (closed || binding != epoch) return;
                var evaluation = createBudget();
                Apply(frozen.Kind == PlayerNavigationTargetKind.Preparation ?
                    session.EnterPreparation(frozen, context, evaluation) : session.Navigate(frozen, context, evaluation.Codec), true);
            };
        }
        public Action PreviewHandler(Func<PlayerNavigationDraft> draft)
        {
            if (draft == null) throw new ArgumentNullException(nameof(draft));
            var context = View.Context; var binding = epoch;
            return () => {
                if (closed || binding != epoch) return;
                var value = draft();
                if (value != null) Apply(session.Preview(value, context, createBudget().Codec), false);
            };
        }
        public Action ActionHandler(PlayerNavigationAction action)
        {
            var token = View.Token; var binding = epoch;
            return () => { if (!closed && binding == epoch) Apply(session.Act(action, token, createBudget()), true); };
        }
        public int IntegerBitLimit => budget.Codec.Math.MaxIntegerBits;
        public int NumericTokenLimit => budget.Codec.MaxNumericTokenBytes;
        public void Dispose()
        {
            if (closed) return;
            closed = true; epoch++; Changed = null; HostRequested = null;
        }
    }

}
