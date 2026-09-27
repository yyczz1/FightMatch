using FightMatch.Core;
using FightMatch.Platform;
using QFramework;

namespace FightMatch.Application
{
    public sealed class FightMatchDemoArchitecture : Architecture<FightMatchDemoArchitecture>
    {
        private CandidateApplicationRuntime runtime;
        private bool deinitialized;
        internal CandidateApplicationSystem ApplicationSystem { get; private set; }

        protected override void Init()
        {
            runtime = new CandidateApplicationRuntime(e => SendEvent(e));
            RegisterModel(new CandidateApplicationModel(runtime));
            ApplicationSystem = new CandidateApplicationSystem(runtime);
            RegisterSystem(ApplicationSystem);
            var battle = new CandidateBattleApplicationSystem(ApplicationSystem);
            RegisterSystem(battle);
            var lifecycle = new CandidateLifecycleApplicationSystem(ApplicationSystem, battle);
            RegisterSystem(lifecycle);
            RegisterSystem(new PlayerSessionSystem(runtime, ApplicationSystem, lifecycle));
        }

        protected override void OnDeinit()
        {
            if (deinitialized) throw new System.InvalidOperationException("AlreadyDeinitialized");
            // QFramework calls this before clearing its models and systems.
            runtime.Close();
            deinitialized = true;
        }
    }

    public sealed class CandidateApplicationModel : AbstractModel
    {
        private readonly CandidateApplicationRuntime runtime;
        internal CandidateApplicationModel(CandidateApplicationRuntime runtime) { this.runtime = runtime; }
        public CandidateApplicationView View => runtime.View;
        protected override void OnInit() { }
    }

    public sealed class CandidateApplicationSystem : AbstractSystem
    {
        private readonly CandidateApplicationRuntime runtime;
        internal CandidateApplicationSystem(CandidateApplicationRuntime runtime) { this.runtime = runtime; }
        protected override void OnInit() { }

        public CandidateApplicationCallResult Open(ILocalSaveStorage storage, string playerId,
            SaveOpenMode mode, SaveRecoveryCapabilities capabilities, SaveStoreBudget budget)
        { return runtime.Open(storage, playerId, mode, capabilities, budget); }

        public CandidateApplicationCallResult Restore(SaveStoreBudget budget) { return runtime.Restore(budget); }

        public CandidateApplicationCallResult Submit(PreparedCandidateApplicationIntent intent,
            CandidateApplicationBuilder builder, SaveStoreBudget budget)
        { return runtime.Submit(intent, builder, budget); }

        internal CandidateApplicationCallResult SubmitTrusted(PreparedCandidateApplicationIntent intent,
            CandidateApplicationBuilder builder, SaveStoreBudget budget)
        { return runtime.Submit(intent, builder, budget, true); }

        public CandidateApplicationCallResult Resolve(PreparedCandidateApplicationIntent intent, SaveStoreBudget budget)
        { return runtime.Resolve(intent, budget); }

        public CandidateApplicationCallResult Retry(PreparedCandidateApplicationIntent intent, SaveStoreBudget budget)
        { return runtime.Retry(intent, budget); }

        public CandidateApplicationCallResult End(PreparedCandidateApplicationIntent intent, SaveStoreBudget budget)
        { return runtime.End(intent, budget); }

        public CandidateApplicationCallResult ResumeObserved(string commitId, SaveStoreBudget budget)
        { return runtime.ResumeObserved(commitId, budget); }

        public CandidateApplicationCallResult EndObserved(string commitId, SaveStoreBudget budget)
        { return runtime.EndObserved(commitId, budget); }

        public CandidateApplicationCallResult QueryView() { return runtime.QueryView(); }

        public CandidateApplicationCallResult QueryOperation(PreparedCandidateApplicationIntent intent, SaveStoreBudget budget)
        { return runtime.QueryOperation(intent, budget); }
    }
}
