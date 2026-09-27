using System;
using System.Collections.Generic;
using System.Linq;
using FightMatch.Core;
using FightMatch.Content;
using FightMatch.Platform;
using QFramework;

namespace FightMatch.Application
{
    public sealed partial class PlayerSessionSystem : AbstractSystem
    {
        private readonly CandidateApplicationRuntime runtime;
        private readonly CandidateApplicationSystem application;
        private readonly CandidateLifecycleApplicationSystem lifecycle;
        private PublishedContentCatalog catalog;
        private LocalPlayerProfileRef profile;
        private List<ResolvedPublication> publications = new List<ResolvedPublication>();
        internal PlayerProfileCreateRecord CreateRecord { get; private set; }
        internal bool CreationPending => CreateRecord != null && profile == null;
        internal PublishedSaveContext Resolved { get; private set; }
        internal PlayerSessionSystem(CandidateApplicationRuntime runtime, CandidateApplicationSystem application, CandidateLifecycleApplicationSystem lifecycle)
        { this.runtime = runtime; this.application = application; this.lifecycle = lifecycle; }
        protected override void OnInit() { }

        public CandidateApplicationCallResult OpenExisting(LocalPlayerProfileRef profile, ILocalSaveStorage storage,
            PublishedContentCatalog catalog, SaveRecoveryCapabilities capabilities, SaveStoreBudget budget)
        {
            var q = runtime.PlayerAdmission() ?? application.QueryView(); if (Guard(q) != null) return q;
            if (q.View.Phase != CandidateApplicationPhase.Unconfigured) return Refuse("AlreadyOpen", "Application.Store");
            if (profile == null || catalog == null) return Refuse("MissingField", "Profile");
            this.catalog = catalog; CreateRecord = profile.CreateRecord; this.profile = profile;
            return runtime.Open(storage, profile.PlayerId, SaveOpenMode.Existing, capabilities, budget, this);
        }
        public PreparedPlayerProfileResult PrepareNewProfile(PublishedContentCatalog catalog, string releaseSetId, SaveCodecBudget budget)
        { return PrepareProfile(catalog, releaseSetId, budget, false); }
        private PreparedPlayerProfileResult PrepareProfile(PublishedContentCatalog catalog, string releaseSetId, SaveCodecBudget budget, bool roster)
        {
            return PrepareResult(budget, () =>
            {
                CheckGuard(); Need(catalog != null, "MissingField", "Catalog");
                var binding = Take(catalog.GetCurrentBinding("player", releaseSetId, ContentConsumerCapabilities.Current));
                var publication = Take(catalog.ResolveExact(binding, ContentConsumerCapabilities.Current));
                var prepared = CandidateLifecyclePreparation.NewProfile(Content(publication, budget), Recipe(publication.NewProfile), budget, true, roster);
                Need(prepared.IsAccepted, prepared.Code, prepared.Diagnostic?.FieldPath);
                var record = Take(PlayerProfileCreateRecordCodec.Freeze(prepared.Request.Intent, publication.NewProfile.Id,
                    publication.NewProfile.RecordVersion, publication.NewProfile.CanonicalBytes.ToArray(), Array.Empty<PlayerProfileCreateMaterial>(), budget));
                return new PreparedPlayerProfile(record, catalog, prepared.Request);
            });
        }
        public PreparedPlayerProfileResult RecoverCreateIntent(LocalPlayerProfileObservation observed, PublishedContentCatalog catalog, SaveCodecBudget budget)
        {
            return PrepareResult(budget, () =>
            {
                CheckGuard(); Need(observed?.CreateRecord != null && catalog != null, "MissingField", "CreateRecord");
                var record = observed.CreateRecord; var original = Take(PlayerProfileCreateRecordCodec.RestoreIntent(record, budget));
                var publication = ResolveRecord(record, catalog);
                var restored = CandidateLifecyclePreparation.RestoreProfile(original, Content(publication, budget), Recipe(publication.NewProfile), budget);
                Need(restored.IsAccepted, restored.Code, restored.Diagnostic?.FieldPath);
                return new PreparedPlayerProfile(record, catalog, restored.Request);
            });
        }
        public CandidateApplicationCallResult CreateNew(PreparedPlayerProfile request, ILocalSaveStorage storage,
            LocalPlayerProfileLocator locator, SaveRecoveryCapabilities capabilities, SaveStoreBudget budget)
        {
            var q = runtime.PlayerAdmission() ?? application.QueryView(); if (Guard(q) != null) return q;
            if (request == null || locator == null) return Refuse("MissingField", "CreateRecord");
            var recorded = locator.RecordCreateIntent(request.CreateRecord, budget);
            if (!recorded.IsAccepted) return Refuse(recorded.Code, recorded.Diagnostic?.FieldPath, Diagnostic(recorded));
            return ContinueCreate(request, recorded.Observation, storage, locator, capabilities, budget);
        }
        public CandidateApplicationCallResult ContinueCreate(PreparedPlayerProfile request, LocalPlayerProfileObservation expected,
            ILocalSaveStorage storage, LocalPlayerProfileLocator locator, SaveRecoveryCapabilities capabilities, SaveStoreBudget budget)
        {
            var q = runtime.PlayerAdmission() ?? application.QueryView(); if (Guard(q) != null) return q;
            if (request == null || expected == null || locator == null) return Refuse("MissingField", "CreateRecord");
            if (!ReferenceEquals(expected.Owner, locator)) return Refuse("StaleProfileObservation", "Profile.Owner");
            var observed = locator.Read(budget);
            if (!observed.IsAccepted) return Refuse(observed.Code, observed.Diagnostic?.FieldPath, Diagnostic(observed));
            var current = observed.Observation;
            if (current.CreateRecord?.RecordSha256 != request.CreateRecord.RecordSha256) return Refuse("InconsistentCreateIntent", "CreateRecord");
            if (current.ActiveProfile == null && current.PhysicalIdentity != expected.PhysicalIdentity) return Refuse("StaleProfileObservation", "Profile.PhysicalIdentity");
            if (q.View.Phase == CandidateApplicationPhase.Unconfigured)
            {
                catalog = request.Catalog; CreateRecord = request.CreateRecord; profile = current.ActiveProfile;
                q = runtime.Open(storage, request.PlayerId, profile == null ? SaveOpenMode.CreateNew : SaveOpenMode.Existing, capabilities, budget, this);
            }
            else if (CreateRecord?.RecordSha256 != request.CreateRecord.RecordSha256 || q.View.Purpose != SavePurpose.PlayerSave)
                return Refuse("InconsistentCreateIntent", "Application.Profile");
            if (q.View.PendingOperationId != null)
            {
                if (q.View.PendingOperationId != request.OperationId) return Refuse("InconsistentCreateIntent", "Candidate.OperationId");
                q = application.Retry(request.Intent, budget);
            }
            else if (q.View.ObservedCandidateCommitIds.Count != 0)
            {
                if (q.View.ObservedCandidateCommitIds.Count != 1) return Refuse("RecoveryBlocked", "Recovery.Candidates");
                q = application.ResumeObserved(q.View.ObservedCandidateCommitIds[0], budget);
            }
            else if (q.View.Phase == CandidateApplicationPhase.InitializationReady && CreationPending)
                q = lifecycle.Submit(request.Request, budget);
            if (q.View.IsPublishedHeadVerified && q.View.PublishedSnapshot != null)
                return runtime.ConfirmProfile(locator, current, budget);
            return q;
        }
        public CandidateLifecyclePrepareResult PrepareLifecycle(PlayerLifecycleDraft draft, SaveCodecBudget budget)
        {
            return LifecycleResult(() =>
            {
                var snapshot = Ready(); Need(draft != null, "MissingField", "Draft");
                var publication = For(snapshot.Business.Progression.Definition.Context);
                return CandidateLifecyclePreparation.Prepare(new CandidateLifecycleDraft { PlayerId = snapshot.Business.PlayerId,
                    ExpectedCommitId = draft.ExpectedCommitId, Kind = draft.Kind, Content = Content(publication, budget), EnterAttempt = draft.EnterAttempt,
                    ExitAttempt = draft.ExitAttempt, RestartAttempt = draft.RestartAttempt, AdvanceRecovery = draft.AdvanceRecovery, EndTimeSample = draft.EndTimeSample }, budget, true);
            });
        }
        public CandidateLifecyclePrepareResult PrepareVictory(CandidateTimeSample time, SaveCodecBudget budget)
        {
            return LifecycleResult(() =>
            {
                var snapshot = Ready(); var context = snapshot.Business.ActiveHistory?.CurrentRun.Baseline.Entry.Context;
                Need(context != null, "NoActiveBattle", "ActiveHistory");
                return CandidateLifecyclePreparation.Victory(snapshot, Content(For(context), budget), time, budget, true);
            });
        }
        internal bool IsOriginalCreation(PreparedCandidateApplicationIntent intent)
        { return CreateRecord != null && intent.CanonicalBytes.SequenceEqual(CreateRecord.CanonicalInitializeIntentBytes); }
        internal void AcceptProfile(LocalPlayerProfileRef active) { profile = active; }
        internal CandidateApplicationDiagnostic CheckSnapshot(CandidateApplicationSnapshot snapshot, SaveCodecBudget budget)
        {
            var proof = PlayerProfileCreateRecordCodec.VerifyInitializationCommit(CreateRecord, snapshot, budget);
            if (!proof.IsAccepted) return CandidateApplicationDiagnostic.From(proof, "Initialization");
            return profile == null || profile.OriginalInitializationCommitId == proof.Value.OriginalInitializationCommitId ? null
                : new CandidateApplicationDiagnostic("InconsistentCreateIntent", "Initialization.OriginalCommitId");
        }
        internal CandidateApplicationDiagnostic ResolveRoots(SaveRecoveryView view, SaveCodecBudget budget)
        {
            try
            {
                Need(view.EvidenceComplete && view.RequirementsComplete, "RecoveryBlocked", "Recovery.Complete");
                Need(!view.RedundancyDegraded, "RecoveryBlocked", "Recovery.RetainedRoots");
                var resolved = new List<ResolvedPublication>(); resolved.Add(ResolveRecord(CreateRecord, catalog));
                var roots = view.Current == null ? view.RetainedRoots : new[] { view.Current }.Concat(view.RetainedRoots);
                foreach (var root in roots)
                {
                    Need(root.Descriptor.Purpose == SavePurpose.PlayerSave && root.Descriptor.PlayerId == CreateRecord.PlayerId, "InconsistentBinding", "Recovery.Root");
                    foreach (var b in root.Requirements.Bindings)
                    {
                        Need(b.Kind == SaveBindingKind.Content || b.Kind == SaveBindingKind.Definition, "UnsupportedBinding", "Recovery.Binding.Kind");
                        var content = Take(ContentBinding.Prepare(b.PackageId, b.ContentFingerprint, b.RuleVersion, b.NumericContractVersion, b.RandomContractVersion, budget));
                        var publication = resolved.FirstOrDefault(x => x.Binding.Same(content));
                        if (publication == null)
                        {
                            Need(resolved.Count < budget.MaxCollectionEntries, "Limit", "Recovery.Definitions");
                            publication = Take(catalog.ResolveExact(content, ContentConsumerCapabilities.Current)); resolved.Add(publication);
                        }
                        if (b.Kind == SaveBindingKind.Definition)
                        {
                            var version = Take(ExactSaveValueCodec.DecodeInteger(b.LevelVersion, budget));
                            var exact = Take(DefinitionBinding.Prepare(content, b.LevelId, version ?? 0, budget));
                            Need(publication.Definitions.Levels.Any(l => l.LevelId == exact.LevelId && l.LevelVersion == exact.CanonicalLevelVersion), "UnsupportedBinding", "Recovery.Definition");
                        }
                    }
                }
                Resolved = Take(PublishedSaveContext.Prepare(resolved.Select(x => x.Definitions).ToArray(), budget));
                publications = resolved; return null;
            }
            catch (Refusal e) { return e.Diagnostic; }
            catch (ExactMathLimitException e) { return CandidateApplicationDiagnostic.From("Limit", "Recovery", e); }
        }
        private ResolvedPublication For(PreparedRuleContext context)
        {
            var p = context as PreparedPublishedRuleContext;
            var value = p == null ? null : publications.FirstOrDefault(x => x.Binding.Same(p.Binding));
            Need(value != null, "UnsupportedBinding", "Content"); return value;
        }
        private static ResolvedPublication ResolveRecord(PlayerProfileCreateRecord record, PublishedContentCatalog catalog)
        {
            Need(record != null && catalog != null, "MissingField", "CreateRecord");
            var value = Take(catalog.ResolveExact(record.ContentBinding, ContentConsumerCapabilities.Current));
            Need(record.NewProfileDefinitionId == value.NewProfile.Id && record.NewProfileDefinitionVersion == value.NewProfile.RecordVersion &&
                record.FrozenNewProfileDefinitionBytes.SequenceEqual(value.NewProfile.CanonicalBytes), "InconsistentCreateIntent", "CreateRecord.Definition");
            return value;
        }
        private static CandidateApplicationInitializeInput Recipe(NewProfileDefinition profile)
        { return new CandidateApplicationInitializeInput { CharacterId = profile.CharacterId, ClassId = profile.ClassId,
            InitialLevel = profile.Level, InitialExperience = profile.Experience, OriginalSlot = profile.OriginalSlot }; }
        private static CandidateLifecycleContentInput Content(ResolvedPublication publication, SaveCodecBudget budget)
        {
            Need(publication.Parameters.Count <= budget.MaxCollectionEntries, "Limit", "Content.Parameters");
            var content = new CandidateLifecycleContentInput { Growths = publication.Definitions.Growths, Inventory = publication.Definitions.Inventory,
                Progression = publication.Definitions.Progression, Levels = publication.Definitions.Levels, Rewards = publication.Definitions.Rewards,
                CritCoefficients = publication.Parameters.Select(p => new CandidateCritCoefficient(p.Target, p.C)).ToArray() };
            content.SetPermanentDefinitions(publication.Definitions.GetPermanentDefinitions());
            return content;
        }
        private CandidateApplicationSnapshot Ready()
        {
            CheckGuard(); var view = runtime.View;
            Need(profile != null && view.IsPublishedHeadVerified && view.Phase == CandidateApplicationPhase.Ready && view.Purpose == SavePurpose.PlayerSave,
                "ResolutionRequired", "Application.View"); return view.PublishedSnapshot;
        }
        private static CandidateApplicationDiagnostic Guard(CandidateApplicationCallResult q)
        { return q.Code == "WrongThread" || q.Code == "Disposed" || q.Code == "Busy" ? q.Diagnostic : null; }
        private void CheckGuard() { var q = runtime.PlayerAdmission() ?? application.QueryView(); var guard = Guard(q); if (guard != null) throw new Refusal(guard); }
        private CandidateApplicationCallResult Refuse(string code, string path, CandidateApplicationDiagnostic diagnostic = null)
        { return new CandidateApplicationCallResult(code, false, null, null, null, runtime.View, diagnostic ?? new CandidateApplicationDiagnostic(code, path), null); }
        internal static CandidateApplicationDiagnostic Diagnostic(LocalPlayerProfileResult result)
        { return new CandidateApplicationDiagnostic(result.Code, result.Diagnostic?.FieldPath, result.LimitReason, result.RequiredAtLeast, result.Allowed, stage: result.Diagnostic?.Stage,
            exceptionType: result.Diagnostic?.ExceptionType, exceptionMessage: result.Diagnostic?.ExceptionMessage); }
        private static PreparedPlayerProfileResult PrepareResult(SaveCodecBudget budget, Func<PreparedPlayerProfile> action)
        {
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            try { return new PreparedPlayerProfileResult(action()); }
            catch (Refusal e) { return new PreparedPlayerProfileResult(null, e.Diagnostic); }
            catch (ExactMathLimitException e) { return new PreparedPlayerProfileResult(null, CandidateApplicationDiagnostic.From("Limit", "Prepare", e)); }
        }
        private static CandidateLifecyclePrepareResult LifecycleResult(Func<CandidateLifecyclePrepareResult> action)
        {
            try { return action(); }
            catch (Refusal e) { return new CandidateLifecyclePrepareResult(null, e.Diagnostic); }
            catch (ExactMathLimitException e) { return new CandidateLifecyclePrepareResult(null, CandidateApplicationDiagnostic.From("Limit", "Prepare", e)); }
        }
        private sealed class Refusal : Exception
        { internal readonly CandidateApplicationDiagnostic Diagnostic; internal Refusal(CandidateApplicationDiagnostic diagnostic) { Diagnostic = diagnostic; } }
        private static void Need(bool condition, string code, string path)
        { if (!condition) throw new Refusal(new CandidateApplicationDiagnostic(code, path)); }
        private static T Take<T>(SaveCodecResult<T> value)
        { if (!value.IsAccepted) throw new Refusal(CandidateApplicationDiagnostic.From(value, "PlayerSession")); return value.Value; }
        private static T Take<T>(PublicationResult<T> value)
        { Need(value.IsAccepted, value.RejectionCode, value.FieldPath); return value.Value; }
    }
}
