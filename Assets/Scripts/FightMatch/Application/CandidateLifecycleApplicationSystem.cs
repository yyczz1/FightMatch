using System;
using FightMatch.Core;
using FightMatch.Platform;
using QFramework;

namespace FightMatch.Application
{
    public sealed partial class CandidateLifecycleApplicationSystem : AbstractSystem
    {
        private readonly CandidateApplicationSystem application;
        private readonly CandidateBattleApplicationSystem battle;
        private bool executing;
        internal CandidateLifecycleApplicationSystem(CandidateApplicationSystem application, CandidateBattleApplicationSystem battle)
        { this.application = application; this.battle = battle; }
        protected override void OnInit() { }

        private CandidateApplicationDiagnostic PreparationGuard()
        {
            var q = application.QueryView();
            if (q.Code == "WrongThread" || q.Code == "Disposed") return q.Diagnostic;
            return executing || battle.QueryView().Code == "Busy" ? new CandidateApplicationDiagnostic("Busy", "Application") : null;
        }

        public CandidateLifecyclePrepareResult PrepareNewProfile(CandidateLifecycleContentInput content,
            CandidateApplicationInitializeInput recipe, SaveCodecBudget budget)
        {
            var guard = PreparationGuard();
            return guard != null ? new CandidateLifecyclePrepareResult(null, guard) : CandidateLifecyclePreparation.NewProfile(content, recipe, budget);
        }

        public CandidateLifecyclePrepareResult Prepare(CandidateLifecycleDraft draft, SaveCodecBudget budget)
        {
            var guard = PreparationGuard();
            return guard != null ? new CandidateLifecyclePrepareResult(null, guard) : CandidateLifecyclePreparation.Prepare(draft, budget);
        }

        public CandidateLifecyclePrepareResult PrepareVictory(CandidateLifecycleContentInput content, CandidateTimeSample endTime,
            SaveCodecBudget budget)
        {
            var guard = PreparationGuard();
            if (guard != null) return new CandidateLifecyclePrepareResult(null, guard);
            var q = application.QueryView();
            if (!q.View.IsPublishedHeadVerified || q.View.Phase != CandidateApplicationPhase.Ready)
                return new CandidateLifecyclePrepareResult(null, new CandidateApplicationDiagnostic("ResolutionRequired", "Application.View"));
            return CandidateLifecyclePreparation.Victory(q.View.PublishedSnapshot, content, endTime, budget);
        }

        public CandidateApplicationCallResult OpenNewProfile(PreparedCandidateLifecycleRequest request, ILocalSaveStorage storage,
            SaveRecoveryCapabilities capabilities, SaveStoreBudget budget)
        {
            var q = application.QueryView();
            if (q.Code == "WrongThread" || q.Code == "Disposed") return q;
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (request.Kind != CandidateApplicationKind.InitializeProfile) return Refuse(q, "UnsupportedBinding", "Kind");
            // Once opened, only the original runtime decides whether this same prepared operation may continue.
            if (q.View.Phase != CandidateApplicationPhase.Unconfigured) return Submit(request, budget);
            return Invoke(() =>
            {
                var opened = application.Open(storage, request.PlayerId, SaveOpenMode.CreateNew, capabilities, budget);
                return opened.View.Phase == CandidateApplicationPhase.InitializationReady
                    ? application.Submit(request.Intent, (basis, intent, codec) => Initialize(request, codec), budget) : opened;
            });
        }

        public CandidateApplicationCallResult Submit(PreparedCandidateLifecycleRequest request, SaveStoreBudget budget,
            int maxLiveIntegerBits = 1048576, int maxLogTerms = 4096)
        {
            var q = application.QueryView();
            if (q.Code == "WrongThread" || q.Code == "Disposed") return q;
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            if (maxLiveIntegerBits < 0 || maxLiveIntegerBits > 1048576) throw new ArgumentOutOfRangeException(nameof(maxLiveIntegerBits));
            if (maxLogTerms < 0 || maxLogTerms > 4096) throw new ArgumentOutOfRangeException(nameof(maxLogTerms));
            // The runtime performs original-result and pending lookup before invoking this new-operation builder.
            return Invoke(() => application.SubmitTrusted(request.Intent, (basis, intent, codec) =>
            {
                if (request.Kind != CandidateApplicationKind.InitializeProfile && request.Kind != CandidateApplicationKind.EnterAttempt &&
                    request.Kind != CandidateApplicationKind.AdvanceRecovery && request.Kind != CandidateApplicationKind.SettleVictory &&
                    request.Kind != CandidateApplicationKind.PermanentRequest && request.Kind != CandidateApplicationKind.MigratePermanent &&
                    battle.QueryView().PresentationToken != null) return Reject("Busy", "Presentation");
                return Build(basis, request, codec, maxLiveIntegerBits, maxLogTerms);
            }, budget));
        }

        internal static CandidateApplicationBuildResult Build(CandidateApplicationSnapshot basis, PreparedCandidateLifecycleRequest request,
            SaveCodecBudget codec, int maxLiveIntegerBits = 1048576, int maxLogTerms = 4096,
            Func<string> nextId = null, Action<byte[]> entropy = null)
        {
            if (request.Kind == CandidateApplicationKind.InitializeProfile)
                return request.Intent.FormatVersion >= 3 ? InitializeRoster(request, codec) : Initialize(request, codec);
            if (basis == null) return Reject("ResolutionRequired", "Application.View");
            foreach (var character in basis.Business.Roster.Characters)
                if (!CandidateLifecyclePreparation.SameContext(request.Content.Context, character.Definition.Context, codec.Math))
                    return Reject("InconsistentBinding", "Content.Context");
            if (request.Kind == CandidateApplicationKind.EnterAttempt) return Enter(basis, request, codec);
            if (request.Kind == CandidateApplicationKind.AdvanceRecovery) return Recover(basis, request, codec);
            if (request.Kind == CandidateApplicationKind.PermanentRequest || request.Kind == CandidateApplicationKind.MigratePermanent)
                return Permanent(basis, request, codec);
            if ((int)request.Kind >= 10) return Roster(basis, request, codec, nextId, entropy);
            return Finish(basis, request, codec, maxLiveIntegerBits, maxLogTerms);
        }

        public CandidateApplicationCallResult Resolve(PreparedCandidateLifecycleRequest request, SaveStoreBudget budget)
        { return Continue(request, budget, application.Resolve); }
        public CandidateApplicationCallResult Retry(PreparedCandidateLifecycleRequest request, SaveStoreBudget budget)
        { return Continue(request, budget, application.Retry); }
        public CandidateApplicationCallResult End(PreparedCandidateLifecycleRequest request, SaveStoreBudget budget)
        { return Continue(request, budget, application.End); }
        private CandidateApplicationCallResult Continue(PreparedCandidateLifecycleRequest request, SaveStoreBudget budget,
            Func<PreparedCandidateApplicationIntent, SaveStoreBudget, CandidateApplicationCallResult> action)
        {
            var q = application.QueryView();
            if (q.Code == "WrongThread" || q.Code == "Disposed") return q;
            if (request == null) throw new ArgumentNullException(nameof(request));
            return Invoke(() => action(request.Intent, budget));
        }
        private CandidateApplicationCallResult Invoke(Func<CandidateApplicationCallResult> action)
        {
            if (executing) return action(); // Preserve the original runtime's reentrancy diagnostic.
            executing = true;
            try { return action(); }
            finally { executing = false; }
        }
        public CandidateApplicationCallResult QueryOperation(PreparedCandidateLifecycleRequest request, SaveStoreBudget budget)
        {
            var q = application.QueryView();
            if (q.Code == "WrongThread" || q.Code == "Disposed") return q;
            if (request == null) throw new ArgumentNullException(nameof(request));
            return application.QueryOperation(request.Intent, budget);
        }
        public CandidateLifecycleView QueryView() { return new CandidateLifecycleView(battle.QueryView(), executing); }
        private static CandidateApplicationCallResult Refuse(CandidateApplicationCallResult q, string code, string path)
        { return new CandidateApplicationCallResult(code, false, null, null, null, q.View, new CandidateApplicationDiagnostic(code, path), null); }
        private static CandidateApplicationBuildResult Reject(string code, string path)
        { return CandidateApplicationBuildResult.Rejected(code, path); }
        private static CandidateApplicationBuildResult Complete(CandidateBusinessInput input, CandidateApplicationResultInput result, SaveCodecBudget budget)
        {
            var prepared = CandidateBusinessSaveCodec.Prepare(input, input.Format == CandidateBusinessFormat.CandidateV1 ? SavePurpose.CandidateValidation : SavePurpose.PlayerSave, budget);
            return prepared.IsAccepted ? CandidateApplicationBuildResult.Success(prepared.Value, result) :
                CandidateApplicationBuildResult.Rejected(prepared.RejectionCode, prepared.FieldPath, prepared.LimitReason, prepared.RequiredAtLeast, prepared.Allowed);
        }
    }
}
