using System;
using System.Linq;
using FightMatch.Core;

namespace FightMatch.Application
{
    public sealed partial class PlayerSessionSystem
    {
        public PlayerPermanentView QueryPermanent(string characterId)
        {
            var query = runtime.PlayerAdmission() ?? application.QueryView();
            var snapshot = query.View.PublishedSnapshot;
            try
            {
                var head = PermanentHead(null);
                Need(head.Business.Roster.Find(characterId) != null, "InconsistentBinding", "CharacterId");
                var budget = new SaveCodecBudget(new ExactMathBudget());
                var sources = Take(CandidatePermanentInventory.ReadEndpoints(head, budget));
                return new PlayerPermanentView(head, characterId,
                    For(head.Business.Progression.Definition.Context).Definitions.GetPermanentDefinitions(), null,
                    sources, CandidateInventory.Read(head.Business.Inventory, budget.Math));
            }
            catch (Refusal error)
            {
                return new PlayerPermanentView(snapshot, characterId, null, error.Diagnostic.Code);
            }
            catch (ExactMathLimitException)
            {
                return new PlayerPermanentView(snapshot, characterId, null, "Limit");
            }
        }

        private CandidateApplicationSnapshot PermanentHead(string expected)
        {
            var head = Ready();
            Need(runtime.View.PendingOperationId == null && runtime.View.ObservedCandidateCommitIds.Count == 0,
                "ResolutionRequired", "Permanent.Pending");
            Need(head.Continuation == null, "ResolutionRequired", "Permanent.BaseSettlement");
            Need(expected == null || expected == head.Header.CommitId, "StaleContext", "ExpectedCommitId");
            return head;
        }

        public PlayerPermanentPreviewResult PreviewPermanent(PlayerPermanentDraft draft, SaveCodecBudget budget)
        {
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            try
            {
                Need(draft != null, "MissingField", "Draft");
                Need(draft.Kind != CandidatePermanentKind.BeginTeachingGift, "UnsupportedBinding", "Permanent.TeachingEntry");
                var head = PermanentHead(draft.ExpectedCommitId);
                Need(draft.ExpectedCommitId != null, "MissingField", "ExpectedCommitId");
                var definitions = For(head.Business.Progression.Definition.Context).Definitions;
                CheckDefinition(definitions, draft.Copy());
                var quote = Take(CandidatePermanentProtocol.Preview(head, definitions, draft.Copy(), budget));
                return new PlayerPermanentPreviewResult(new PreparedPlayerPermanentPreview(this, head.Header.CommitId, quote));
            }
            catch (Refusal error) { return new PlayerPermanentPreviewResult(null, error.Diagnostic); }
            catch (ExactMathLimitException error)
            { return new PlayerPermanentPreviewResult(null, CandidateApplicationDiagnostic.From("Limit", "Permanent.Preview", error)); }
        }

        private static void CheckDefinition(PublishedRuleDefinitions definitions, CandidatePermanentDraft draft)
        {
            if (draft.Kind == CandidatePermanentKind.Equip || draft.Kind == CandidatePermanentKind.SetPreference) return;
            var kind = draft.Kind == CandidatePermanentKind.UseExperienceCards ? CandidatePermanentDefinitionKind.Card :
                draft.Kind == CandidatePermanentKind.LearnSkill ? CandidatePermanentDefinitionKind.Skill :
                draft.Kind == CandidatePermanentKind.Craft ? CandidatePermanentDefinitionKind.Recipe : CandidatePermanentDefinitionKind.Teaching;
            Need(definitions.GetPermanentDefinitions()?.Find(kind, draft.DefinitionId) != null, "NoPublishedDefinition", "Permanent.Definition");
        }

        public CandidateLifecyclePrepareResult ConfirmPermanent(PreparedPlayerPermanentPreview preview, SaveCodecBudget budget)
        {
            return LifecycleResult(() =>
            {
                Need(preview != null && ReferenceEquals(preview.Owner, this), "InconsistentBinding", "Permanent.PreviewOwner");
                var head = PermanentHead(preview.HeadCommitId);
                var publication = For(head.Business.Progression.Definition.Context);
                Take(CandidatePermanentProtocol.ValidatePreview(head, publication.Definitions, preview.Quote, budget));
                var input = new CandidateApplicationIntentInput
                {
                    PlayerId = head.Business.PlayerId,
                    OperationId = Guid.NewGuid().ToString("N"),
                    ExpectedCommitId = head.Header.CommitId,
                    Kind = CandidateApplicationKind.PermanentRequest,
                    FormatVersion = 4,
                    Context = CandidateLifecyclePreparation.Context(head.Business.Progression.Definition.Context)
                };
                input.SetPermanent(preview.Quote);
                return CandidateLifecyclePreparation.Roster(input, Content(publication, budget), budget);
            });
        }

        public CandidateLifecyclePrepareResult PreparePermanentMigration(string expectedCommitId, SaveCodecBudget budget)
        {
            return LifecycleResult(() =>
            {
                Need(expectedCommitId != null, "MissingField", "ExpectedCommitId");
                var head = PermanentHead(expectedCommitId);
                var source = new CandidateRosterMigrationInput
                {
                    SourceGeneration = head.Header.SaveGeneration,
                    SourceDescriptorLength = head.Descriptor.TotalLength,
                    SourceDescriptorSha256 = head.Descriptor.Sha256
                };
                Take(CandidatePermanentProtocol.Migrate(head, source, budget));
                var input = new CandidateApplicationIntentInput
                {
                    PlayerId = head.Business.PlayerId,
                    OperationId = Guid.NewGuid().ToString("N"),
                    ExpectedCommitId = head.Header.CommitId,
                    Kind = CandidateApplicationKind.MigratePermanent,
                    FormatVersion = 4,
                    Context = CandidateLifecyclePreparation.Context(head.Business.Progression.Definition.Context)
                };
                input.SetPermanentMigration(source);
                return CandidateLifecyclePreparation.Roster(input, Content(For(head.Business.Progression.Definition.Context), budget), budget);
            });
        }

        public CandidateLifecyclePrepareResult PrepareTeachingEntry(DefinitionBinding exactLevelBinding,
            string expectedCommitId, SaveCodecBudget budget)
        {
            return LifecycleResult(() =>
            {
                Need(exactLevelBinding != null && expectedCommitId != null, "MissingField", "Permanent.TeachingEntry");
                var head = PermanentHead(expectedCommitId);
                var definitions = For(head.Business.Progression.Definition.Context).Definitions;
                var teaching = definitions.GetPermanentDefinitions()?.Records.FirstOrDefault(x =>
                    x.Kind == CandidatePermanentDefinitionKind.Teaching && x.Level.Same(exactLevelBinding));
                Need(teaching != null, "NoPublishedDefinition", "Permanent.Teaching");
                var draft = new CandidatePermanentDraft { Kind = CandidatePermanentKind.BeginTeachingGift, DefinitionId = teaching.Id };
                var quote = Take(CandidatePermanentProtocol.Preview(head, definitions, draft, budget));
                return ConfirmPermanent(new PreparedPlayerPermanentPreview(this, head.Header.CommitId, quote), budget);
            });
        }
    }
}
