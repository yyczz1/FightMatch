using System;
using System.Linq;
using FightMatch.Core;

namespace FightMatch.Application
{
    public sealed partial class PlayerSessionSystem
    {
        private PlayerNavigationSession navigation;
        public PlayerNavigationSession GetNavigationSession()
        {
            CheckGuard();
            return navigation ?? (navigation = new PlayerNavigationSession(this, application, lifecycle));
        }

        public PlayerNavigationReadResult QueryNavigation(SaveCodecBudget budget)
        {
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            var q = runtime.PlayerAdmission() ?? application.QueryView();
            try
            {
                var guard = Guard(q); if (guard != null) throw new Refusal(guard);
                var head = Ready();
                var publication = For(head.Business.Progression.Definition.Context);
                Need(publication.Definitions.Levels.Count <= budget.MaxCollectionEntries &&
                    head.Business.Roster.Characters.Count <= budget.MaxCollectionEntries &&
                    head.Business.Inventory.Holdings.Count <= budget.MaxCollectionEntries,
                    "Limit", "Navigation.Collections");
                var roster = QueryRoster();
                var inventory = CandidateInventory.Read(head.Business.Inventory, budget.Math);
                var progression = CandidateProgression.Read(head.Business.Progression, budget.Math);
                var life = lifecycle.QueryView();
                var after = runtime.PlayerAdmission() ?? application.QueryView();
                Need(Guard(after) == null && ReferenceEquals(head, after.View.PublishedSnapshot) &&
                    ReferenceEquals(head, life.Application.PublishedSnapshot), "StaleContext", "Navigation.Head");
                return new PlayerNavigationReadResult(after.View, publication.Binding, publication.Definitions.Levels,
                    roster, inventory, progression, life, null);
            }
            catch (Refusal error)
            { return new PlayerNavigationReadResult(q.View, null, null, null, null, null, null, error.Diagnostic); }
            catch (ExactMathLimitException error)
            { return new PlayerNavigationReadResult(q.View, null, null, null, null, null, null,
                CandidateApplicationDiagnostic.From("Limit", "Navigation.Query", error)); }
        }
    }
}
