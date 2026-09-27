using System;
using System.Collections.Generic;
using System.Linq;
using FightMatch.Core;
using FightMatch.Content;

namespace FightMatch.Application
{
    public sealed partial class PlayerSessionSystem
    {
        public static IReadOnlyList<RequiredSliceContract> RequiredRecoveryContracts { get; } =
            Array.AsReadOnly(new[] { "application", "character", "inventory", "progression", "battle", "rewards" }
                .Select((name, i) => new RequiredSliceContract("fm.m0" + (i + 2) + "." + name, "M0" + (i + 2), 2))
                .Concat(new[] { "application", "character", "inventory", "progression" }
                    .Select((name, i) => new RequiredSliceContract("fm.m0" + (i + 2) + "." + name, "M0" + (i + 2), 3)))
                .Concat(new[] { "application", "character", "inventory", "progression" }
                    .Select((name, i) => new RequiredSliceContract("fm.m0" + (i + 2) + "." + name, "M0" + (i + 2), 4))).ToArray());
        public static IReadOnlyList<string> RequiredRecoveryFeatures { get; } =
            Array.AsReadOnly(new[] { "fm.player.application.v1", "fm.player.roster.v1", "fm.player.permanent.v1" });

        public PreparedPlayerProfileResult PrepareNewRosterProfile(PublishedContentCatalog catalog, string releaseSetId, SaveCodecBudget budget)
        { return PrepareProfile(catalog, releaseSetId, budget, true); }

        public PlayerRosterView QueryRoster()
        {
            var q = runtime.PlayerAdmission() ?? application.QueryView();
            var reason = Guard(q)?.Code;
            var business = q.View.PublishedSnapshot?.Business;
            if (reason == null) reason = profile == null || !q.View.IsPublishedHeadVerified || q.View.Phase != CandidateApplicationPhase.Ready ||
                q.View.PendingOperationId != null || q.View.ObservedCandidateCommitIds.Count != 0 ? "ResolutionRequired" :
                business.ActiveHistory != null || q.View.PublishedSnapshot.Continuation != null ? "ActiveAttemptConflict" :
                (int)business.Format >= 3 ? null : "RosterMigrationRequired";
            return new PlayerRosterView(business, reason);
        }
        private CandidateApplicationSnapshot RosterHead(string expected, bool migration)
        {
            var snapshot = Ready();
            Need(runtime.View.PendingOperationId == null && runtime.View.ObservedCandidateCommitIds.Count == 0, "ResolutionRequired", "Application.Pending");
            Need(snapshot.Business.ActiveHistory == null && snapshot.Continuation == null, "ActiveAttemptConflict", "ActiveHistory");
            Need(expected == snapshot.Header.CommitId, "StaleContext", "ExpectedCommitId");
            Need(migration ? snapshot.Business.Format == CandidateBusinessFormat.PublishedV2 : (int)snapshot.Business.Format >= 3,
                migration ? "UnsupportedSchema" : "RosterMigrationRequired", "Roster.Format");
            return snapshot;
        }
        private CandidateApplicationIntentInput RosterIntent(CandidateApplicationSnapshot snapshot, CandidateApplicationKind kind)
        {
            return new CandidateApplicationIntentInput { PlayerId = snapshot.Business.PlayerId, ExpectedCommitId = snapshot.Header.CommitId,
                OperationId = Guid.NewGuid().ToString("N"), Kind = kind, FormatVersion = 3,
                Context = CandidateLifecyclePreparation.Context(snapshot.Business.Progression.Definition.Context) };
        }
        private CandidateLifecyclePrepareResult PrepareRoster(CandidateApplicationSnapshot snapshot, CandidateApplicationIntentInput input, SaveCodecBudget budget)
        {
            return CandidateLifecyclePreparation.Roster(input, Content(For(snapshot.Business.Progression.Definition.Context), budget), budget);
        }
        public CandidateLifecyclePrepareResult PrepareFormation(PlayerFormationDraft draft, SaveCodecBudget budget)
        {
            return LifecycleResult(() =>
            {
                Need(draft != null, "MissingField", "Draft");
                var head = RosterHead(draft.ExpectedCommitId, false); var input = RosterIntent(head, CandidateApplicationKind.SetFormation);
                Need(draft.ExpectedFormationRevision == head.Business.Roster.FormationRevision, "StaleContext", "ExpectedFormationRevision");
                input.SetFormation = new CandidateFormationInput { ExpectedFormationRevision = draft.ExpectedFormationRevision, Slots = draft.Slots };
                return PrepareRoster(head, input, budget);
            });
        }
        public CandidateLifecyclePrepareResult PrepareFormationEntry(PlayerFormationEntryDraft draft, CandidateTimeSample time, SaveCodecBudget budget)
        {
            return LifecycleResult(() =>
            {
                Need(draft != null, "MissingField", "Draft");
                var head = RosterHead(draft.ExpectedCommitId, false);
                if (head.Business.Format == CandidateBusinessFormat.PublishedPermanentV4)
                    Take(CandidatePermanentGrowth.CheckBattleCapability(Take(head.Business.Roster.PrepareEntry(time, budget)).Roster, budget));
                var input = RosterIntent(head, CandidateApplicationKind.EnterFormation);
                input.EnterFormation = new CandidateFormationEntryInput { LevelId = draft.LevelId, LevelVersion = draft.LevelVersion,
                    ExpectedFormationRevision = draft.ExpectedFormationRevision, Slots = head.Business.Roster.Formation,
                    SelectedCharacters = draft.SelectedCharacters, ExpectedInventoryRevision = draft.ExpectedInventoryRevision,
                    ExpectedProgressionRevision = draft.ExpectedProgressionRevision, TimeSample = time };
                return PrepareRoster(head, input, budget);
            });
        }
        public CandidateLifecyclePrepareResult PrepareRosterMigration(string expectedCommitId, SaveCodecBudget budget)
        {
            return LifecycleResult(() =>
            {
                var head = RosterHead(expectedCommitId, true); var input = RosterIntent(head, CandidateApplicationKind.MigrateRoster);
                input.MigrateRoster = new CandidateRosterMigrationInput { SourceGeneration = head.Header.SaveGeneration,
                    SourceDescriptorLength = head.Descriptor.TotalLength, SourceDescriptorSha256 = head.Descriptor.Sha256 };
                return PrepareRoster(head, input, budget);
            });
        }
    }
}
