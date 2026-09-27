using FightMatch.Core;
using FightMatch.Platform;
using QFramework;

namespace FightMatch.Application
{
    public sealed class OpenCandidateApplicationCommand : AbstractCommand<CandidateApplicationCallResult>
    {
        private readonly ILocalSaveStorage storage;
        private readonly string playerId;
        private readonly SaveOpenMode mode;
        private readonly SaveRecoveryCapabilities capabilities;
        private readonly SaveStoreBudget budget;

        public OpenCandidateApplicationCommand(ILocalSaveStorage storage, string playerId, SaveOpenMode mode,
            SaveRecoveryCapabilities capabilities, SaveStoreBudget budget)
        {
            this.storage = storage;
            this.playerId = playerId;
            this.mode = mode;
            this.capabilities = capabilities;
            this.budget = budget;
        }

        protected override CandidateApplicationCallResult OnExecute()
        {
            return ((FightMatchDemoArchitecture)((IBelongToArchitecture)this).GetArchitecture()).ApplicationSystem
                .Open(storage, playerId, mode, capabilities, budget);
        }
    }

    public sealed class RestoreCandidateApplicationCommand : AbstractCommand<CandidateApplicationCallResult>
    {
        private readonly SaveStoreBudget budget;
        public RestoreCandidateApplicationCommand(SaveStoreBudget budget) { this.budget = budget; }
        protected override CandidateApplicationCallResult OnExecute()
        { return ((FightMatchDemoArchitecture)((IBelongToArchitecture)this).GetArchitecture()).ApplicationSystem.Restore(budget); }
    }

    public sealed class SubmitCandidateApplicationCommand : AbstractCommand<CandidateApplicationCallResult>
    {
        private readonly PreparedCandidateApplicationIntent intent;
        private readonly CandidateApplicationBuilder builder;
        private readonly SaveStoreBudget budget;
        public SubmitCandidateApplicationCommand(PreparedCandidateApplicationIntent intent,
            CandidateApplicationBuilder builder, SaveStoreBudget budget)
        {
            this.intent = intent;
            this.builder = builder;
            this.budget = budget;
        }

        protected override CandidateApplicationCallResult OnExecute()
        { return ((FightMatchDemoArchitecture)((IBelongToArchitecture)this).GetArchitecture()).ApplicationSystem.Submit(intent, builder, budget); }
    }

    public sealed class ResolveCandidateApplicationCommand : AbstractCommand<CandidateApplicationCallResult>
    {
        private readonly PreparedCandidateApplicationIntent intent;
        private readonly SaveStoreBudget budget;
        public ResolveCandidateApplicationCommand(PreparedCandidateApplicationIntent intent, SaveStoreBudget budget)
        { this.intent = intent; this.budget = budget; }
        protected override CandidateApplicationCallResult OnExecute()
        { return ((FightMatchDemoArchitecture)((IBelongToArchitecture)this).GetArchitecture()).ApplicationSystem.Resolve(intent, budget); }
    }

    public sealed class RetryCandidateApplicationCommand : AbstractCommand<CandidateApplicationCallResult>
    {
        private readonly PreparedCandidateApplicationIntent intent;
        private readonly SaveStoreBudget budget;
        public RetryCandidateApplicationCommand(PreparedCandidateApplicationIntent intent, SaveStoreBudget budget)
        { this.intent = intent; this.budget = budget; }
        protected override CandidateApplicationCallResult OnExecute()
        { return ((FightMatchDemoArchitecture)((IBelongToArchitecture)this).GetArchitecture()).ApplicationSystem.Retry(intent, budget); }
    }

    public sealed class EndCandidateApplicationCommand : AbstractCommand<CandidateApplicationCallResult>
    {
        private readonly PreparedCandidateApplicationIntent intent;
        private readonly SaveStoreBudget budget;
        public EndCandidateApplicationCommand(PreparedCandidateApplicationIntent intent, SaveStoreBudget budget)
        { this.intent = intent; this.budget = budget; }
        protected override CandidateApplicationCallResult OnExecute()
        { return ((FightMatchDemoArchitecture)((IBelongToArchitecture)this).GetArchitecture()).ApplicationSystem.End(intent, budget); }
    }

    public sealed class ResumeObservedCandidateCommand : AbstractCommand<CandidateApplicationCallResult>
    {
        private readonly string commitId;
        private readonly SaveStoreBudget budget;
        public ResumeObservedCandidateCommand(string commitId, SaveStoreBudget budget)
        { this.commitId = commitId; this.budget = budget; }
        protected override CandidateApplicationCallResult OnExecute()
        { return ((FightMatchDemoArchitecture)((IBelongToArchitecture)this).GetArchitecture()).ApplicationSystem.ResumeObserved(commitId, budget); }
    }

    public sealed class EndObservedCandidateCommand : AbstractCommand<CandidateApplicationCallResult>
    {
        private readonly string commitId;
        private readonly SaveStoreBudget budget;
        public EndObservedCandidateCommand(string commitId, SaveStoreBudget budget)
        { this.commitId = commitId; this.budget = budget; }
        protected override CandidateApplicationCallResult OnExecute()
        { return ((FightMatchDemoArchitecture)((IBelongToArchitecture)this).GetArchitecture()).ApplicationSystem.EndObserved(commitId, budget); }
    }

    public sealed class CandidateApplicationViewQuery : AbstractQuery<CandidateApplicationCallResult>
    {
        protected override CandidateApplicationCallResult OnDo()
        { return ((FightMatchDemoArchitecture)GetArchitecture()).ApplicationSystem.QueryView(); }
    }

    public sealed class CandidateApplicationOperationQuery : AbstractQuery<CandidateApplicationCallResult>
    {
        private readonly PreparedCandidateApplicationIntent intent;
        private readonly SaveStoreBudget budget;
        public CandidateApplicationOperationQuery(PreparedCandidateApplicationIntent intent, SaveStoreBudget budget)
        { this.intent = intent; this.budget = budget; }
        protected override CandidateApplicationCallResult OnDo()
        { return ((FightMatchDemoArchitecture)GetArchitecture()).ApplicationSystem.QueryOperation(intent, budget); }
    }
}
