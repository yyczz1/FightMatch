using FightMatch.Core;

namespace FightMatch.Application
{
    internal static partial class CandidateLifecyclePreparation
    {
        internal static SaveCodecResult<PublishedRuleDefinitions> PermanentDefinitions(CandidateLifecycleContent content, SaveCodecBudget budget)
        {
            var context = content.Context as PreparedPublishedRuleContext;
            var basis = PublishedRuleDefinitions.Prepare(context?.Binding, content.Growths, content.Inventory,
                content.Progression, content.Levels, content.Rewards, budget);
            if (!basis.IsAccepted || content.GetPermanentDefinitions() == null) return basis;
            return PublishedRuleDefinitions.PreparePermanent(basis.Value, content.GetPermanentDefinitions(), budget);
        }
    }

    public sealed partial class CandidateLifecycleApplicationSystem
    {
        // Both the trusted PlayerSession route and isolated Core evidence use this same owner build.
        private static CandidateApplicationBuildResult Permanent(CandidateApplicationSnapshot basis,
            PreparedCandidateLifecycleRequest request, SaveCodecBudget budget)
        {
            if (request.Intent.ExpectedCommitId != basis.Header.CommitId) return Reject("StaleContext", "ExpectedCommitId");
            if (basis.Continuation != null) return Reject("ResolutionRequired", "Continuation");
            SaveCodecResult<CandidateBusinessSnapshot> next;
            if (request.Kind == CandidateApplicationKind.MigratePermanent)
                next = CandidatePermanentProtocol.Migrate(basis, request.Intent.GetPermanentMigration(), budget);
            else
            {
                var definitions = CandidateLifecyclePreparation.PermanentDefinitions(request.Content, budget);
                if (!definitions.IsAccepted) return Reject(definitions);
                next = CandidatePermanentProtocol.Build(basis, definitions.Value, request.Intent.GetPermanent(), request.OperationId, budget);
            }
            return next.IsAccepted ? CandidateApplicationBuildResult.Success(next.Value, new CandidateApplicationResultInput()) : Reject(next);
        }
    }
}
