using System;
using System.Collections.Generic;
using System.Numerics;
using FlowPuzzle.Core;
using static FightMatch.Core.BusinessFields;

namespace FightMatch.Core
{
    internal sealed class CandidateBusinessRestoreChecks
    {
        private readonly CandidateBusinessSnapshot s;
        private readonly ExactMathBudget math;
        private bool IsRoster => (int)s.Format >= 3;
        private readonly Dictionary<string, CandidateProgressionAttempt> attempts = new Dictionary<string, CandidateProgressionAttempt>(StringComparer.Ordinal);
        private readonly Dictionary<string, CandidateFixedBaseReward> rewards = new Dictionary<string, CandidateFixedBaseReward>(StringComparer.Ordinal);
        private CandidateBusinessRestoreChecks(CandidateBusinessSnapshot snapshot, SaveCodecBudget budget) { s = snapshot; math = budget.Math; }
        internal static void Shape(CandidateBusinessSnapshot x, SaveCodecBudget budget)
        {
            SaveEnvelopeCodec.CheckList(x.RetainedRuns, budget, "RetainedRuns"); SaveEnvelopeCodec.CheckList(x.RetainedRollbacks, budget, "RetainedRollbacks");
            SaveEnvelopeCodec.CheckList(x.Rewards.BaseRewards, budget, "M07.BaseRewards");
            var runs = new List<CandidateBattleRun>(x.RetainedRuns); var rollbacks = new List<CandidateRollbackRecord>(x.RetainedRollbacks);
            if (x.ActiveHistory != null)
            {
                SaveEnvelopeCodec.CheckList(x.ActiveHistory.Archive, budget, "M06.Archive"); SaveEnvelopeCodec.CheckList(x.ActiveHistory.EffectiveAnchors, budget, "M06.EffectiveAnchors");
                SaveEnvelopeCodec.CheckList(x.ActiveHistory.RollbackRecords, budget, "M06.RollbackRecords"); runs.Add(x.ActiveHistory.CurrentRun); rollbacks.AddRange(x.ActiveHistory.RollbackRecords);
            }
            foreach (var r in rollbacks)
            { Need(r?.Range != null, "M06.Rollback", "MissingField"); SaveEnvelopeCodec.CheckList(r.Range.Entries, budget, "M06.Range.Entries"); runs.Add(r.BeforeRun); runs.Add(r.RestoredRun); }
            foreach (var r in runs) { Need(r != null, "M06.Run", "MissingField"); SaveEnvelopeCodec.CheckList(r.Records, budget, "M06.Run.Records"); }
            foreach (var r in x.Rewards.BaseRewards) { Need(r?.Report != null, "M07.Report", "MissingField"); SaveEnvelopeCodec.CheckList(r.Report.Operations, budget, "M07.Report.Operations"); }
        }
        internal static void Check(CandidateBusinessSnapshot snapshot, CandidateBattleSaveCodec battle, SaveCodecBudget budget)
        {
            var c = new CandidateBusinessRestoreChecks(snapshot, budget);
            if (c.IsRoster) snapshot.Roster.Check(budget);
            else Need(snapshot.Roster.Characters.Count == 1, "M03.Characters", "UnsupportedBinding");
            foreach (var character in snapshot.Roster.Characters)
            { c.Player(character.PlayerId, "M03.PlayerId"); new GrowthChecks(budget.Math).CheckState(character); }
            c.Player(snapshot.Inventory.PlayerId, "M04.PlayerId");
            c.Player(snapshot.Progression.PlayerId, "M05.PlayerId"); c.Player(snapshot.Rewards.PlayerId, "M07.PlayerId");
            new InventoryChecks(budget.Math).CheckState(snapshot.Inventory); new ProgressionChecks(budget.Math).CheckState(snapshot.Progression);
            c.Progression(); foreach (var character in snapshot.Roster.Characters) c.Character(character); c.Inventory();
            foreach (var baseline in battle.Baselines.Rows) c.Entry(baseline);
            foreach (var binding in battle.Bindings.Rows) c.Binding(binding);
            foreach (var state in battle.Snapshots.Rows) c.Snapshot(state, "M06.Snapshot");
            foreach (var record in battle.Records.Rows) c.Record(record);
            foreach (var report in battle.Reports.Rows)
            { var checks = new RewardChecks(budget.Math); Need(new RewardReportChecks(checks).Report(report), "M06." + checks.Path, "IncompleteHistory"); }
            foreach (var run in battle.Runs.Rows) c.Run(run);
            if (snapshot.ActiveHistory != null)
            {
                var h = snapshot.ActiveHistory;
                var result = CandidateHistoryOperations.FindOperation(h, h.Archive.Count > 0 ? h.Archive[0].OperationId : "save-validation", budget.Math);
                Need(result.IsAccepted || result.RejectionCode == CandidateHistoryRejectionCode.NotFound, "M06." + result.FieldPath, "IncompleteHistory");
                foreach (var r in h.RollbackRecords) c.Rollback(r);
            }
            foreach (var r in snapshot.RetainedRollbacks) c.Rollback(r);
            c.Active(); c.Rewards(); c.Receipts();
            c.Permanent(budget);
        }
        private void Progression()
        {
            var state = s.Progression; var challenges = new HashSet<string>(StringComparer.Ordinal); var openChallenges = new HashSet<string>(StringComparer.Ordinal);
            var endIds = new HashSet<string>(StringComparer.Ordinal); var settlements = new HashSet<string>(StringComparer.Ordinal);
            var first = new Dictionary<string, CandidateProgressionEndReceipt>(StringComparer.Ordinal); var activeCount = 0;
            foreach (var challenge in state.Challenges)
            {
                Need(challenges.Add(challenge.ChallengeId) && (challenge.Attempts.Count > 0 ||
                    s.Format == CandidateBusinessFormat.PublishedPermanentV4 && challenge.GetTeachingBinding() != null), "M05.Challenges", "ReceiptConflict");
                if (!challenge.IsClosed) Need(openChallenges.Add(challenge.Level.LevelId), "M05.Challenges.Level", "ReceiptConflict");
                CandidateProgressionEndReceipt victory = null;
                for (var i = 0; i < challenge.Attempts.Count; i++)
                {
                    var attempt = challenge.Attempts[i]; var b = attempt.Begin; Player(b.PlayerId, "M05.Begin.PlayerId");
                    Need(!attempts.ContainsKey(b.AttemptId) && b.ChallengeId == challenge.ChallengeId && SameLevel(b.Level, challenge.Level), "M05.Begin", "ReceiptConflict");
                    attempts.Add(b.AttemptId, attempt);
                    Need(b.Participants.Count >= 1 && b.Participants.Count <= (IsRoster ? 3 : 1), "M05.Begin.Participants", "UnsupportedBinding");
                    var ids = new HashSet<string>(); var slots = new HashSet<int>();
                    foreach (var a in b.Participants)
                    {
                        var character = s.Roster.Find(a.CharacterId);
                        Need(character != null && ids.Add(a.CharacterId) && slots.Add(a.OriginalSlot) && a.OriginalSlot >= 0 && a.OriginalSlot <= 2 &&
                            a.ClassId == character.ClassId && a.ClassKind == character.Definition.ClassKind &&
                            (IsRoster || a.OriginalSlot == character.OriginalSlot) && a.CharacterRevision > 0 && a.CharacterRevision <= character.StateRevision,
                            "M05.Begin.Participant", "ReceiptConflict");
                    }
                    Need(b.FormationRevision == null || IsRoster && b.FormationRevision > 0 && b.FormationRevision <= s.Roster.FormationRevision,
                        "M05.Begin.FormationRevision", "ReceiptConflict");
                    if (attempt.End == null) { activeCount++; Need(i == challenge.Attempts.Count - 1 && !challenge.IsClosed, "M05.Attempt", "IncompleteHistory"); continue; }
                    var end = attempt.End;
                    Need(ReferenceEquals(end.Begin, b) && endIds.Add(end.EndReceiptId), "M05.End", "ReceiptConflict");
                    var win = end.Kind == CandidateProgressionEndKind.NormalVictory; var restart = end.Kind == CandidateProgressionEndKind.ImmediateRestart;
                    Need(win == (end.SettlementId != null) && win == (end.FinalReportFingerprint != null) && restart == (end.NewAttemptId != null), "M05.End.Kind", "ReceiptConflict");
                    if (win)
                    {
                        Need(victory == null && settlements.Add(end.SettlementId) && i == challenge.Attempts.Count - 1, "M05.End.SettlementId", "ReceiptConflict"); victory = end;
                        var isFirst = !first.ContainsKey(b.Level.LevelId); Need(end.IsFirstClear == isFirst, "M05.End.IsFirstClear", "ReceiptConflict"); if (isFirst) first.Add(b.Level.LevelId, end);
                    }
                    else Need(!end.IsFirstClear, "M05.End.IsFirstClear", "ReceiptConflict");
                    if (restart)
                    {
                        Need(i + 1 < challenge.Attempts.Count, "M05.End.NewAttemptId", "IncompleteHistory"); var next = challenge.Attempts[i + 1].Begin;
                        Need(next.AttemptId == end.NewAttemptId && next.EntryBaselineId == b.EntryBaselineId && SameParticipants(next.Participants, b.Participants) && next.FormationRevision == b.FormationRevision, "M05.End.NewAttemptId", "ReceiptConflict");
                        Eq(next.Context, b.Context, "M05.Restart.Context");
                    }
                }
                Need(ReferenceEquals(challenge.ClosedBy, victory), "M05.Challenge.ClosedBy", "ReceiptConflict");
            }
            Need(activeCount <= 1, "M05.ActiveAttempt", "IncompleteHistory");
            Need(state.FirstClears.Count == first.Count, "M05.FirstClears", "ReceiptConflict"); var clearIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var clear in state.FirstClears) Need(clearIds.Add(clear.LevelId) && first.TryGetValue(clear.LevelId, out var end) && ReferenceEquals(end, clear.End), "M05.FirstClears", "ReceiptConflict");
            var opens = new HashSet<string>(StringComparer.Ordinal);
            foreach (var open in state.OpenFacts)
            {
                Player(open.PlayerId, "M05.OpenFacts.PlayerId"); Need(opens.Add(open.Level.LevelId), "M05.OpenFacts", "ReceiptConflict");
                if (open.Level.UnlockKind == CandidateProgressionUnlockKind.InitiallyOpen) Need(open.SourceClear == null, "M05.OpenFacts.SourceClear", "ReceiptConflict");
                else Need(open.SourceClear != null && ContainsReference(state.FirstClears, open.SourceClear) && open.SourceClear.LevelId == open.Level.UnlockAfterLevelId, "M05.OpenFacts.SourceClear", "ReceiptConflict");
            }
            foreach (var level in state.Definition.Levels)
                if (level.UnlockKind == CandidateProgressionUnlockKind.InitiallyOpen || first.ContainsKey(level.UnlockAfterLevelId ?? "")) Need(opens.Contains(level.LevelId), "M05.OpenFacts", "ReceiptConflict");
        }
        private void Character(CandidateCharacterState x)
        {
            var d = x.Definition; var offset = math.Subtract(x.Level, 1);
            var need = math.Add(d.XpBase, math.Multiply(offset, math.Add(d.XpLinear, math.Multiply(d.XpQuadratic, offset))));
            Need(x.Level > 0 && x.Experience >= 0 && x.Experience < need && x.OriginalSlot >= 0 && x.OriginalSlot <= 2, "M03.LevelExperience");
            var settlements = new HashSet<string>(StringComparer.Ordinal); var rewardAttempts = new HashSet<string>(StringComparer.Ordinal);
            foreach (var r in x.BaseRewards)
            { Player(r.PlayerId, "M03.BaseRewards.PlayerId"); Need(r.CharacterId == x.CharacterId && settlements.Add(r.SettlementId) && rewardAttempts.Add(r.AttemptId), "M03.BaseRewards", "ReceiptConflict"); }
            var ends = new HashSet<string>(StringComparer.Ordinal); var endAttempts = new HashSet<string>(StringComparer.Ordinal);
            foreach (var end in x.ProcessedEnds)
            {
                Player(end.PlayerId, "M03.ProcessedEnds.PlayerId"); Need(end.CharacterId == x.CharacterId && ends.Add(end.EndReceiptId) && endAttempts.Add(end.AttemptId), "M03.ProcessedEnds", "ReceiptConflict");
                Need(end.WasParticipant || !end.WasDown, "M03.ProcessedEnds.WasDown", "ReceiptConflict");
                var recover = end.Kind != CandidateCharacterEndKind.ImmediateRestart && end.WasParticipant && end.WasDown;
                Need(recover == (end.RecoveryId != null) && recover == (end.TimeSample != null), "M03.ProcessedEnds.RecoveryId", "ReceiptConflict");
            }
            var recoveryIds = new HashSet<string>(StringComparer.Ordinal); var recoveryEnds = new HashSet<string>(StringComparer.Ordinal); var active = 0;
            foreach (var period in x.RecoveryPeriods)
            {
                Need(ContainsReference(x.ProcessedEnds, period.EndReceipt) && period.EndReceipt.RecoveryId != null && recoveryIds.Add(period.RecoveryId) && recoveryEnds.Add(period.EndReceipt.EndReceiptId), "M03.RecoveryPeriods", "ReceiptConflict");
                Eq(period.Definition.Context, period.EndReceipt.Context, "M03.RecoveryPeriods.Definition.Context");
                Need(period.Definition.ClassId == x.ClassId && period.Elapsed.Numerator.Sign >= 0 && period.Elapsed.Compare(period.Duration, math) <= 0 &&
                    period.IsCompleted == (period.Elapsed.Compare(period.Duration, math) == 0), "M03.RecoveryPeriods.Elapsed"); if (!period.IsCompleted) active++;
            }
            Need(active <= 1, "M03.RecoveryPeriods", "ReceiptConflict");
            foreach (var end in x.ProcessedEnds) Need((end.RecoveryId != null) == recoveryEnds.Contains(end.EndReceiptId), "M03.ProcessedEnds.RecoveryId", "ReceiptConflict");
        }
        private void Inventory()
        {
            var x = s.Inventory;
            Need(x.IsRoster == IsRoster && x.Loadouts.Count == s.Roster.Characters.Count, "M04.Loadouts", "InconsistentBinding");
            for (var i = 0; i < x.Loadouts.Count; i++)
            {
                var a = x.Loadouts[i].Actor; var character = s.Roster.Characters[i];
                Need(a.CharacterId == character.CharacterId && a.ClassId == character.ClassId && a.ClassKind == character.Definition.ClassKind &&
                    a.OriginalSlot == character.OriginalSlot && (IsRoster || SameActor(x.Actor, a)), "M04.Actor", "InconsistentBinding");
            }
            var holdings = new Dictionary<string, CandidateInventoryHolding>(StringComparer.Ordinal); var totals = new Dictionary<string, BigInteger>(StringComparer.Ordinal);
            Need(x.Holdings.Count == x.Definition.Items.Count, "M04.Holdings", "InconsistentBinding");
            foreach (var h in x.Holdings)
            { Need(!holdings.ContainsKey(h.ItemId) && InventoryChecks.FindItem(x.Definition, h.ItemId) != null && h.T.Sign >= 0, "M04.Holdings"); holdings.Add(h.ItemId, h); totals.Add(h.ItemId, 0); }
            foreach (var loadout in x.Loadouts)
            {
                if (loadout.ItemId == null) Need(loadout.L.IsZero && !loadout.Enabled.HasValue, "M04.Loadout");
                else
                {
                    var item = InventoryChecks.FindItem(x.Definition, loadout.ItemId);
                    Need(item != null && item.Kind == CandidateInventoryItemKind.OrdinaryTactical && item.EquipClassId == loadout.Actor.ClassId &&
                        loadout.Enabled.HasValue && loadout.L >= 0 && loadout.L <= 99, "M04.Loadout");
                }
            }
            var grants = new HashSet<string>(StringComparer.Ordinal); var grantAttempts = new HashSet<string>(StringComparer.Ordinal);
            foreach (var grant in x.OrdinaryGrants)
            {
                Player(grant.PlayerId, "M04.OrdinaryGrants.PlayerId"); Need(grants.Add(grant.SettlementId) && grantAttempts.Add(grant.AttemptId), "M04.OrdinaryGrants", "ReceiptConflict");
                Vector(grant.Items, "M04.OrdinaryGrants.Items"); foreach (var row in grant.Items) totals[row.ItemId] = math.Add(totals[row.ItemId], row.Quantity);
            }
            var ends = new HashSet<string>(StringComparer.Ordinal); var carried = new HashSet<string>(StringComparer.Ordinal);
            foreach (var end in x.Ends)
            {
                Carry(end.OriginalCarry, "M04.Ends.OriginalCarry"); Need(ends.Add(end.EndReceiptId) && carried.Add(end.OriginalCarry.AttemptId), "M04.Ends", "ReceiptConflict");
                Vector(end.Rewards, "M04.Ends.Rewards"); Need(end.Remaining.Count == end.OriginalCarry.Rows.Count, "M04.Ends.Remaining", "ReceiptConflict");
                for (var i = 0; i < end.Remaining.Count; i++)
                {
                    var u = end.Remaining[i]; var row = end.OriginalCarry.Rows[i]; Need(u.CharacterId == row.Actor.CharacterId && u.ItemId == row.ItemId && u.U >= 0 && u.U <= row.C, "M04.Ends.Remaining", "ReceiptConflict");
                    if (end.Kind == CandidateInventoryEndKind.NormalVictory) totals[u.ItemId] = math.Subtract(totals[u.ItemId], math.Subtract(row.C, u.U));
                }
                var win = end.Kind == CandidateInventoryEndKind.NormalVictory;
                Need(win == (end.SettlementId != null) && win == (end.RewardReceipt != null) && (end.Kind == CandidateInventoryEndKind.ImmediateRestart) == (end.NewAttemptId != null), "M04.Ends.Kind", "ReceiptConflict");
                if (win)
                { Need(ContainsReference(x.OrdinaryGrants, end.RewardReceipt) && end.RewardReceipt.SettlementId == end.SettlementId && end.RewardReceipt.AttemptId == end.OriginalCarry.AttemptId, "M04.Ends.RewardReceipt", "ReceiptConflict"); SameVector(end.Rewards, end.RewardReceipt.Items, "M04.Ends.Rewards"); }
                else Need(end.Rewards.Count == 0, "M04.Ends.Rewards", "ReceiptConflict");
            }
            if (x.ActiveCarry != null)
            {
                Carry(x.ActiveCarry, "M04.ActiveCarry"); Need(carried.Add(x.ActiveCarry.AttemptId), "M04.ActiveCarry", "ReceiptConflict");
                foreach (var actor in x.ActiveCarry.ReadyParticipants) Need(x.FindLoadout(actor.CharacterId).L.IsZero, "M04.ActiveCarry.Loadout", "ReceiptConflict");
            }
            if (s.Format == CandidateBusinessFormat.PublishedPermanentV4)
            {
                foreach (var source in x.GetPermanentLedger().Sources)
                {
                    Need(totals.ContainsKey(source.ItemId) && source.Inclusion != null, "M04.Permanent.Source");
                    totals[source.ItemId] = math.Add(totals[source.ItemId], source.Inclusion.UnitCount);
                }
                foreach (var effect in x.GetPermanentLedger().Effects)
                {
                    Vector(effect.Quote.Costs, "M04.Permanent.Costs");
                    Vector(effect.Quote.Outputs, "M04.Permanent.Outputs");
                    foreach (var row in effect.Quote.Costs) totals[row.ItemId] = math.Subtract(totals[row.ItemId], row.Quantity);
                    foreach (var row in effect.Quote.Outputs) totals[row.ItemId] = math.Add(totals[row.ItemId], row.Quantity);
                }
            }
            foreach (var h in x.Holdings)
            {
                Need(h.T == totals[h.ItemId], "M04.Holdings.T", "ReceiptConflict"); var reserved = BigInteger.Zero;
                if (x.ActiveCarry != null) foreach (var row in x.ActiveCarry.Rows) if (row.ItemId == h.ItemId) reserved = math.Add(reserved, row.C);
                var allocated = BigInteger.Zero;
                foreach (var loadout in x.Loadouts) if (loadout.ItemId == h.ItemId) allocated = math.Add(allocated, loadout.L);
                Need(math.Subtract(math.Subtract(h.T, allocated), reserved).Sign >= 0, "M04.Holdings.Free");
            }
        }
        private void Permanent(SaveCodecBudget budget)
        {
            var growth = s.Roster.GetPermanentEffects();
            var inventory = s.Inventory.GetPermanentLedger();
            var progression = s.Progression.GetPermanentEffects();
            if (s.Format != CandidateBusinessFormat.PublishedPermanentV4)
            {
                Need(growth.Count == 0 && inventory.Sources.Count == 0 && inventory.Effects.Count == 0 && progression.Count == 0,
                    "Permanent.Legacy", "UnsupportedSchema");
                foreach (var challenge in s.Progression.Challenges) Need(challenge.GetTeachingBinding() == null, "Permanent.Legacy.Challenge");
                return;
            }
            SaveEnvelopeCodec.CheckList(inventory.Sources, budget, "Permanent.Sources");
            var held = new List<CandidatePermanentPortion>();
            foreach (var source in inventory.Sources)
            {
                CandidatePermanentInventory.CheckSource(source, s.PlayerId, budget);
                Need(source.Grant.Kind == CandidatePermanentSourceKind.ExistingAdvertisementHeld, "Permanent.HeldSource");
                var portion = new CandidatePermanentPortion(source, null, 0, source.ItemId, source.Inclusion.UnitStart, source.Inclusion.UnitCount);
                foreach (var prior in held)
                    Need(prior.Source.GrantLine != source.GrantLine ||
                        !CandidatePermanentCodec.SameGrant(prior.Source.Grant, source.Grant, budget), "Permanent.Sources.Duplicate");
                held.Add(portion);
            }
            var owners = new[] { growth, inventory.Effects, progression };
            var learnedSkills = new HashSet<(string, string)>();
            for (var owner = 0; owner < owners.Length; owner++)
            {
                SaveEnvelopeCodec.CheckList(owners[owner], budget, "Permanent.Effects");
                var seen = new HashSet<string>();
                foreach (var effect in owners[owner])
                {
                    Need(effect != null && seen.Add(effect.OperationId) && effect.Quote.PlayerId == s.PlayerId, "Permanent.Effect.Identity");
                    CandidatePermanentProtocol.Shape(effect.Quote, budget);
                    var q = effect.Quote;
                    if (owner == 0)
                    {
                        Need(effect.Outcome == "Applied" && (q.Kind == CandidatePermanentKind.UseExperienceCards || q.Kind == CandidatePermanentKind.LearnSkill), "M03.Permanent.Kind");
                        if (q.Kind == CandidatePermanentKind.LearnSkill)
                            Need(q.OriginalLearningOperation == null && learnedSkills.Add((q.CharacterId, q.DefinitionId)), "M03.Permanent.LearningUnique");
                        var character = s.Roster.Find(q.CharacterId);
                        Need(character != null && character.ClassId == q.ClassId, "M03.Permanent.Character");
                        var before = new CandidateCharacterState(character.Definition, s.PlayerId, character.CharacterId,
                            q.BeforeLevel, q.BeforeExperience, character.OriginalSlot, q.CharacterRevision,
                            character.BaseRewards, character.ProcessedEnds, character.RecoveryPeriods);
                        if (q.Kind == CandidatePermanentKind.UseExperienceCards)
                        {
                            var requirement = CandidatePermanentGrowth.CalculateCards(before, q.UnitExperience, q.Quantity.Value, budget);
                            Need(requirement.FixedExperience == q.FixedExperience && requirement.Cards == q.Costs[0].Quantity,
                                "M03.Permanent.MinimumCards");
                        }
                        CandidateCharacterGrowth.Accumulate(before, q.FixedExperience, math, out var level, out var experience);
                        Need(level == q.FinalLevel && experience == q.FinalExperience, "M03.Permanent.Experience");
                        var cost = CandidatePermanentProtocol.Find(inventory.Effects, effect.OperationId);
                        Need(cost != null && CandidatePermanentCodec.SameQuote(q, cost.Quote, budget), "M03.Permanent.Cost");
                    }
                    if (owner == 1) Need(effect.Outcome == "Applied", "M04.Permanent.Outcome");
                    if (owner == 2) Need(q.Kind == CandidatePermanentKind.BeginTeachingGift ||
                        q.Kind == CandidatePermanentKind.LearnSkill || q.Kind == CandidatePermanentKind.ConfirmTeachingExplanation, "M05.Permanent.Kind");
                }
            }
            foreach (var challenge in s.Progression.Challenges)
            {
                if (challenge.Attempts.Count > 0) continue;
                var gift = CandidatePermanentProtocol.Find(progression, challenge.ChallengeId);
                Need(gift != null && gift.Quote.Kind == CandidatePermanentKind.BeginTeachingGift &&
                    gift.Quote.TeachingLevel.Same(challenge.GetTeachingBinding()) &&
                    challenge.Level.LevelId == gift.Quote.TeachingLevel.LevelId &&
                    challenge.Level.LevelVersion == gift.Quote.TeachingLevel.CanonicalLevelVersion, "M05.Permanent.EmptyChallenge");
            }
        }

        private void Carry(CandidateCarryPlan carry, string p)
        {
            Player(carry.PlayerId, p + ".PlayerId");
            Need(carry.ReadyParticipants.Count >= 1 && carry.ReadyParticipants.Count <= (IsRoster ? 3 : 1) &&
                carry.Rows.Count <= carry.ReadyParticipants.Count, p + ".ReadyParticipants", "InconsistentBinding");
            var ids = new HashSet<string>(); var slots = new HashSet<int>(); var rows = new HashSet<string>();
            foreach (var actor in carry.ReadyParticipants)
            {
                var owned = s.Inventory.FindLoadout(actor.CharacterId)?.Actor;
                Need(owned != null && ids.Add(actor.CharacterId) && slots.Add(actor.OriginalSlot) && actor.OriginalSlot >= 0 && actor.OriginalSlot <= 2 &&
                    actor.ClassId == owned.ClassId && actor.ClassKind == owned.ClassKind && (IsRoster || SameActor(actor, owned)), p + ".ReadyParticipants", "InconsistentBinding");
            }
            foreach (var row in carry.Rows)
            {
                var item = InventoryChecks.FindItem(s.Inventory.Definition, row.ItemId);
                var actor = CandidatePermanentSaveCodec.Find(carry.ReadyParticipants, a => a.CharacterId == row.Actor.CharacterId, p + ".Rows.Actor");
                Need(rows.Add(row.Actor.CharacterId) && SameActor(row.Actor, actor) && row.C >= 0 && row.C <= 99 &&
                    row.Source.PlayerId == s.PlayerId && row.Source.ItemId == row.ItemId && item != null &&
                    item.Kind == CandidateInventoryItemKind.OrdinaryTactical && item.EquipClassId == row.Actor.ClassId, p + ".Rows", "InconsistentBinding");
            }
        }
        private void Vector(IReadOnlyList<CandidateInventoryQuantity> rows, string p)
        { var ids = new HashSet<string>(StringComparer.Ordinal); foreach (var r in rows) Need(ids.Add(r.ItemId) && r.Quantity >= 0 && InventoryChecks.FindItem(s.Inventory.Definition, r.ItemId) != null, p); }
        private void SameVector(IReadOnlyList<CandidateInventoryQuantity> x, IReadOnlyList<CandidateInventoryQuantity> y, string p)
        { Need(x.Count == y.Count, p, "ReceiptConflict"); for (var i = 0; i < x.Count; i++) Need(x[i].ItemId == y[i].ItemId && x[i].Quantity == y[i].Quantity, p, "ReceiptConflict"); }
        private void Active()
        {
            var attempt = s.Progression.ActiveAttempt; var h = s.ActiveHistory; var carry = s.Inventory.ActiveCarry;
            Need((h != null) == (attempt != null) && (h != null) == (carry != null), "ActiveHistory", "IncompleteHistory");
            if (h == null) return;
            var entry = h.CurrentRun.Baseline.Entry;
            Need(entry.AttemptId == attempt.Begin.AttemptId && entry.EntryBaselineId == carry.EntryBaselineId && entry.AttemptId == carry.AttemptId && carry.Mode == EntryCarryMode.Empty, "ActiveHistory.Attempt", "InconsistentBinding");
            Eq(entry.Context, carry.Context, "ActiveHistory.Carry.Context");
            foreach (var participant in entry.ReadyParticipants) Need(s.Roster.Find(participant.CharacterId).IsReady, "ActiveHistory.Character.IsReady", "InconsistentBinding");
        }
        private void Rewards()
        {
            var settlements = new HashSet<string>(StringComparer.Ordinal);
            foreach (var r in s.Rewards.BaseRewards)
            {
                Player(r.PlayerId, "M07.PlayerId"); Need(!rewards.ContainsKey(r.AttemptId) && settlements.Add(r.SettlementId), "M07.BaseRewards", "ReceiptConflict"); rewards.Add(r.AttemptId, r);
                Need(attempts.TryGetValue(r.AttemptId, out var attempt) && ReferenceEquals(attempt.End, r.Ending) && r.Ending.Kind == CandidateProgressionEndKind.NormalVictory &&
                    r.Ending.FinalReportFingerprint == r.Report.Fingerprint && r.Definition.LevelId == r.Report.Level.LevelId && r.Definition.LevelVersion == r.Report.Level.LevelVersion, "M07.Ending", "ReceiptConflict");
                Eq(r.Definition.Context, r.Report.Baseline.Entry.Context, "M07.Definition.Context"); Need(r.Experience.Count == r.Report.Baseline.Entry.ReadyParticipants.Count, "M07.Experience", "ReceiptConflict");
                var ids = new HashSet<BattleCombatantKey>();
                foreach (var e in r.Experience)
                {
                    Need(ids.Add(e.CombatantKey), "M07.Experience", "ReceiptConflict"); var member = CandidateBattleSaveCodec.MemberDefinition(r.Report.Baseline.Entry, e.CombatantKey, "M07.Experience");
                    var totals = CandidatePermanentSaveCodec.Find(r.Report.FinalSnapshot.Contributions, t => t.CombatantKey.Equals(e.CombatantKey), "M07.Experience.Totals");
                    Need(e.CharacterId == member.CharacterId && e.OriginalSlot == member.OriginalSlot && e.EntryLevel == member.Level, "M07.Experience.Participant", "ReceiptConflict");
                    Eq(e.Dealt, totals.EffectiveDamageDealtHp, "M07.Experience.Dealt"); Eq(e.Taken, totals.EffectiveDamageTakenHp, "M07.Experience.Taken");
                    Eq(e.Contribution, e.Dealt.Multiply(r.Definition.DamageWeight, math).Add(e.Taken.Multiply(r.Definition.TakenWeight, math), math), "M07.Experience.Contribution");
                    Eq(e.Reference, r.Report.WholeLevelInitialEnemyHp.Divide(r.Definition.ReferenceHpDivisor, math), "M07.Experience.Reference");
                    var exponent = math.Subtract(math.Subtract(e.EntryLevel, r.Report.Level.RecommendedLevel), r.Definition.OverlevelGrace); var factor = r.Definition.LevelPenaltyBase; var multiplier = One();
                    while (exponent.Sign > 0) { exponent = math.DivRem(exponent, 2, out var rest); if (!rest.IsZero) multiplier = multiplier.Multiply(factor, math); if (!exponent.IsZero) factor = factor.Multiply(factor, math); }
                    Eq(e.LevelMultiplier, multiplier, "M07.Experience.LevelMultiplier"); var score = e.Score;
                    Need(score.Amount >= 0 && score.TermsUsed >= 0 && score.LowerBound.Numerator.Sign >= 0 && score.LowerBound.Compare(score.UpperBound, math) <= 0 &&
                        score.LowerBound.Floor(math) == score.Amount && score.UpperBound.Floor(math) == score.Amount, "M07.Experience.Score", "ReceiptConflict");
                }
            }
        }
        private void Receipts()
        {
            var endCount = 0; var characterEnds = new Dictionary<string, int>(); var characterRewards = new Dictionary<string, int>();
            foreach (var character in s.Roster.Characters) { characterEnds.Add(character.CharacterId, 0); characterRewards.Add(character.CharacterId, 0); }
            foreach (var attempt in attempts.Values)
            {
                var b = attempt.Begin;
                var carry = attempt.End == null ? s.Inventory.ActiveCarry
                    : CandidatePermanentSaveCodec.Find(s.Inventory.Ends, x => x.OriginalCarry.AttemptId == b.AttemptId, "M04.Ends").OriginalCarry;
                Need(carry != null && carry.ReadyParticipants.Count == b.Participants.Count, "M04.Carry.Participants", "ReceiptConflict");
                foreach (var participant in b.Participants)
                {
                    var actor = CandidatePermanentSaveCodec.Find(carry.ReadyParticipants, x => x.CharacterId == participant.CharacterId, "M04.Carry.Participant");
                    Need(actor.OriginalSlot == participant.OriginalSlot && actor.ClassId == participant.ClassId, "M04.Carry.Participant", "ReceiptConflict");
                }
                if (attempt.End == null) continue;
                endCount++; var end = attempt.End;
                var inventory = CandidatePermanentSaveCodec.Find(s.Inventory.Ends, x => x.OriginalCarry.AttemptId == b.AttemptId, "M04.Ends");
                Need(inventory.EndReceiptId == end.EndReceiptId && inventory.OriginalCarry.EntryBaselineId == b.EntryBaselineId && (int)inventory.Kind == (int)end.Kind &&
                    inventory.SettlementId == end.SettlementId && inventory.NewAttemptId == end.NewAttemptId, "EndReceipts", "ReceiptConflict");
                Eq(inventory.OriginalCarry.Context, b.Context, "M04.Ends.Context");
                CandidateFixedBaseReward reward = null;
                if (end.Kind == CandidateProgressionEndKind.NormalVictory)
                    Need(rewards.TryGetValue(b.AttemptId, out reward) && reward.Experience.Count == b.Participants.Count, "M07.BaseRewards", "ReceiptConflict");
                else Need(!rewards.ContainsKey(b.AttemptId), "M07.BaseRewards", "ReceiptConflict");
                foreach (var participant in b.Participants)
                {
                    var owned = s.Roster.Find(participant.CharacterId); characterEnds[owned.CharacterId]++;
                    var character = CandidatePermanentSaveCodec.Find(owned.ProcessedEnds, x => x.AttemptId == b.AttemptId, "M03.ProcessedEnds");
                    Need(character.EndReceiptId == end.EndReceiptId && character.EntryBaselineId == b.EntryBaselineId &&
                        (int)character.Kind == (int)end.Kind && character.WasParticipant, "EndReceipts", "ReceiptConflict");
                    Eq(character.Context, b.Context, "M03.ProcessedEnds.Context");
                    if (reward == null) continue;
                    characterRewards[owned.CharacterId]++;
                    var experience = CandidatePermanentSaveCodec.Find(owned.BaseRewards, x => x.AttemptId == b.AttemptId, "M03.BaseRewards");
                    var amount = CandidatePermanentSaveCodec.Find(reward.Experience, x => x.CharacterId == owned.CharacterId, "M07.Experience");
                    var member = CandidatePermanentSaveCodec.Find(reward.Report.FinalSnapshot.Members, x => x.Member.CharacterId == owned.CharacterId, "M07.FinalMember");
                    Need(experience.SettlementId == end.SettlementId && experience.Amount == amount.Amount &&
                        character.WasDown == member.Hp.Numerator.IsZero, "M03.BaseRewards.Amount", "ReceiptConflict");
                    Eq(experience.Context, reward.Definition.Context, "M03.BaseRewards.Context");
                }
                if (reward == null) continue;
                Need(inventory.Rewards.Count == reward.Materials.Count && inventory.OriginalCarry.Rows.Count == 0, "M04.Ends.Rewards", "ReceiptConflict");
                foreach (var material in reward.Materials)
                { var row = CandidatePermanentSaveCodec.Find(inventory.Rewards, x => x.ItemId == material.ItemId, "M04.Ends.Rewards"); Need(row.Quantity == material.Amount, "M04.Ends.Rewards.Amount", "ReceiptConflict"); }
                Eq(inventory.RewardReceipt.Context, reward.Definition.Context, "M04.Ends.RewardReceipt.Context");
            }
            foreach (var character in s.Roster.Characters)
                Need(character.ProcessedEnds.Count == characterEnds[character.CharacterId] && character.BaseRewards.Count == characterRewards[character.CharacterId], "Receipts.Coverage", "ReceiptConflict");
            Need(s.Inventory.Ends.Count == endCount && s.Inventory.OrdinaryGrants.Count == rewards.Count, "Receipts.Coverage", "ReceiptConflict");
        }
        private void Entry(BattleEntryBaseline baseline)
        {
            var e = baseline.Entry; Player(e.PlayerId, "M06.Baseline.PlayerId");
            Need(attempts.TryGetValue(e.AttemptId, out var attempt), "M06.Baseline.AttemptId", "InconsistentBinding"); var b = attempt.Begin;
            Need(e.ChallengeId == b.ChallengeId && e.EntryBaselineId == b.EntryBaselineId && e.Level.LevelId == b.Level.LevelId && e.Level.LevelVersion == b.Level.LevelVersion,
                "M06.Baseline.Identity", "InconsistentBinding"); Eq(e.Context, b.Context, "M06.Baseline.Context");
            Need(e.Members.Count >= 1 && e.Members.Count <= (IsRoster ? 3 : 1) && e.Members.Count == e.ReadyParticipants.Count &&
                e.Members.Count == b.Participants.Count && baseline.PrdInitialStates.Count == e.Members.Count, "M06.Baseline.Members", "UnsupportedBinding");
            for (var i = 0; i < e.ReadyParticipants.Count; i++)
            {
                var m = e.ReadyParticipants[i]; var a = b.Participants[i];
                Need(ReferenceEquals(e.Members[i], m) && m.CharacterId == a.CharacterId && m.ClassId == a.ClassId && m.ClassKind == a.ClassKind &&
                    m.OriginalSlot == a.OriginalSlot, "M06.Baseline.Participant", "InconsistentBinding");
                var prd = baseline.PrdInitialStates[i];
                Need(prd.CombatantKey.Equals(BattleCombatantKey.ForParticipant(e.AttemptId, m.CharacterId)) && ReferenceEquals(prd.Crit, m.Crit) &&
                    prd.FailureCount.IsZero, "M06.Baseline.PrdInitialStates", "InconsistentBinding");
            }
        }
        private static bool SameParticipants(IReadOnlyList<CandidateProgressionParticipant> a, IReadOnlyList<CandidateProgressionParticipant> b)
        {
            if (a.Count != b.Count) return false;
            for (var i = 0; i < a.Count; i++) if (!SameParticipant(a[i], b[i])) return false;
            return true;
        }
        private void Binding(CandidateRandomBinding b)
        {
            Need(b.MappingId == CandidateRandomPreparer.SupportedMappingId, "M06.Binding.MappingId", "UnsupportedBinding");
            var domains = new[] { b.Battle, b.BaseReward, b.Bonus }; var initials = b.Start.Baseline.RandomInitials;
            var values = new[] { initials.Battle, initials.BaseReward, initials.Bonus };
            for (var i = 0; i < domains.Length; i++)
            {
                var d = domains[i]; Need((int)d.Purpose == i && d.InitSequence <= 0x7fffffffffffffffUL, "M06.Binding.Domain", "InconsistentBinding");
                // Check the persisted seed mapping algebraically; no generator is initialized or sampled.
                var increment = unchecked((d.InitSequence << 1) | 1UL);
                var state = unchecked((increment + d.InitState) * 6364136223846793005UL + increment);
                Need(d.Initial.WordsConsumed.IsZero && d.Initial.Initial.State == state && d.Initial.Current.State == state &&
                    d.Initial.Initial.Increment == increment && d.Initial.Current.Increment == increment, "M06.Binding.Domain.Initial", "InconsistentBinding");
                Eq(d.Initial, values[i], "M06.Binding.Baseline.RandomInitials");
            }
            var initial = b.Start.Snapshot; var baseline = b.Start.Baseline;
            Need(ReferenceEquals(initial.Baseline, baseline) && initial.SceneRevision.IsOne && initial.CurrentFaceIndex == 0 && initial.Phase == BattlePhase.AwaitAction &&
                initial.EffectiveActionsCompleted.IsZero && initial.EnemyPhasesCompleted.IsZero && initial.Board.LockedRoutes.Count == 0 && initial.Board.PendingLinks.Count == 0,
                "M06.Binding.InitialSnapshot", "InconsistentBinding");
            Eq(initial.Random.Stream, b.Battle.Initial, "M06.Binding.InitialSnapshot.Random"); Eq(initial.Random.PrdStates, baseline.PrdInitialStates, "M06.Binding.InitialSnapshot.Prd");
            Need(ReferenceEquals(initial.Random.Stream, b.Battle.Initial), "M06.Binding.InitialSnapshot.Random", "InconsistentBinding");
            foreach (var member in initial.Members) Eq(member.Hp, member.Member.EntryHp, "M06.Binding.InitialSnapshot.Hp");
            foreach (var enemy in initial.Enemies) { Eq(enemy.Hp, enemy.Enemy.Stats.MaxHp, "M06.Binding.InitialSnapshot.EnemyHp"); Need(enemy.IntentCursor.IsZero, "M06.Binding.InitialSnapshot.IntentCursor"); }
            foreach (var total in initial.Contributions) Need(total.EffectiveDamageDealtHp.Numerator.IsZero && total.EffectiveDamageTakenHp.Numerator.IsZero, "M06.Binding.InitialSnapshot.Contributions");
        }
        private void Snapshot(BattleSnapshot x, string p)
        {
            var entry = x.Baseline.Entry; var face = entry.Level.Faces[x.CurrentFaceIndex];
            Need(x.SceneRevision > 0 && x.EffectiveActionsCompleted >= 0 && x.EnemyPhasesCompleted >= 0 && x.EffectiveActionsCompleted == x.EnemyPhasesCompleted &&
                x.Members.Count == entry.ReadyParticipants.Count && x.Enemies.Count == face.Pairs.Count && x.Contributions.Count == x.Members.Count && x.Random.PrdStates.Count == x.Members.Count,
                p + ".Coverage", "IncompleteHistory");
            for (var i = 0; i < x.Members.Count; i++)
            {
                var m = x.Members[i]; var key = BattleCombatantKey.ForParticipant(entry.AttemptId, entry.ReadyParticipants[i].CharacterId);
                Need(ReferenceEquals(m.Member, entry.ReadyParticipants[i]) && m.CombatantKey.Equals(key) && x.Contributions[i].CombatantKey.Equals(key) && x.Random.PrdStates[i].CombatantKey.Equals(key) &&
                    ReferenceEquals(x.Random.PrdStates[i].Crit, m.Member.Crit), p + ".Members", "InconsistentBinding");
                Hp(m.Hp, m.Member.Stats.MaxHp, p + ".Members.Hp"); Nonnegative(x.Contributions[i].EffectiveDamageDealtHp, p + ".Contributions.Dealt"); Nonnegative(x.Contributions[i].EffectiveDamageTakenHp, p + ".Contributions.Taken");
            }
            for (var i = 0; i < x.Enemies.Count; i++)
            { var enemy = x.Enemies[i]; Need(ReferenceEquals(enemy.Enemy, face.Pairs[i].Enemy) && enemy.PairKey.PairId == face.Pairs[i].PairId, p + ".Enemies", "InconsistentBinding"); Hp(enemy.Hp, enemy.Enemy.Stats.MaxHp, p + ".Enemies.Hp"); }
            var marked = new HashSet<BattlePairKey>(); var locked = new List<FlowPathData>();
            foreach (var route in x.Board.LockedRoutes)
            {
                Need(marked.Add(route.PairKey), p + ".Board.LockedRoutes", "IncompleteHistory"); Route(x, route.PairKey, route.Route, locked, p + ".Board.LockedRoutes");
                locked.Add(new FlowPathData { colorId = CandidateBattleSaveCodec.PairDefinition(x.Baseline, route.PairKey, p).GeometryColorId, cells = new List<FlowPos>(route.Route) });
            }
            foreach (var pending in x.Board.PendingLinks)
            { Need(pending.FaceId == face.FaceId && marked.Add(pending), p + ".Board.PendingLinks", "IncompleteHistory"); CandidateBattleSaveCodec.PairDefinition(x.Baseline, pending, p); }
            var alive = false;
            foreach (var enemy in x.Enemies) { Need(marked.Contains(enemy.PairKey) == enemy.Hp.Numerator.IsZero, p + ".Board.DeathCoverage", "IncompleteHistory"); if (enemy.Hp.Numerator.Sign > 0) alive = true; }
            var ready = false; foreach (var member in x.Members) if (member.Hp.Numerator.Sign > 0) ready = true;
            var phase = x.Board.PendingLinks.Count > 0 ? BattlePhase.AwaitLinks : alive ? (ready ? BattlePhase.AwaitAction : BattlePhase.AwaitRescue) : BattlePhase.WonPendingSettlement;
            Need(x.Phase == phase && (alive || x.Board.PendingLinks.Count > 0 || x.CurrentFaceIndex == entry.Level.Faces.Count - 1), p + ".Phase", "IncompleteHistory");
            var stream = x.Random.Stream; var original = x.Baseline.RandomInitials.Battle;
            Need(stream.Initial.State == original.Initial.State && stream.Initial.Increment == original.Initial.Increment && stream.Current.Increment == original.Initial.Increment, p + ".Random.Initial", "InconsistentBinding");
        }
        private void Run(CandidateBattleRun run)
        {
            Need(ReferenceEquals(run.Binding.Start.Baseline, run.Baseline) && ReferenceEquals(run.Binding.Start.Snapshot, run.InitialSnapshot), "M06.Run.Binding", "InconsistentBinding");
            Need(ReferenceEquals(run.CurrentSnapshot.Baseline, run.Baseline), "M06.Run.CurrentSnapshot.Baseline", "InconsistentBinding");
            var previous = run.InitialSnapshot; var operations = new HashSet<string>(StringComparer.Ordinal);
            foreach (var r in run.Records)
            {
                Need(ReferenceEquals(r.BeforeSnapshot.Baseline, run.Baseline) && operations.Add(r.OperationId) && r.BeforeSnapshot.SceneRevision >= previous.SceneRevision &&
                    previous.Phase != BattlePhase.WonPendingSettlement, "M06.Run.Records", "IncompleteHistory");
                Eq(previous, r.BeforeSnapshot, "M06.Run.Records.BeforeSnapshot", true); previous = r.AfterSnapshot;
            }
            Need(run.CurrentSnapshot.SceneRevision >= previous.SceneRevision, "M06.Run.CurrentSnapshot.SceneRevision", "IncompleteHistory"); Eq(previous, run.CurrentSnapshot, "M06.Run.CurrentSnapshot", true);
            Need((run.CurrentSnapshot.Phase == BattlePhase.WonPendingSettlement) == (run.FinalReport != null), "M06.Run.FinalReport", "IncompleteHistory");
            if (run.FinalReport == null) return; var report = run.FinalReport;
            Need(ReferenceEquals(report.Binding, run.Binding) && ReferenceEquals(report.Baseline, run.Baseline) && ReferenceEquals(report.InitialSnapshot, run.InitialSnapshot) &&
                ReferenceEquals(report.FinalSnapshot, run.CurrentSnapshot) && report.Operations.Count == run.Records.Count, "M06.Run.FinalReport", "IncompleteHistory");
            for (var i = 0; i < run.Records.Count; i++) Need(ReferenceEquals(run.Records[i], report.Operations[i]), "M06.Run.FinalReport.Operations", "IncompleteHistory");
        }
        private void Rollback(CandidateRollbackRecord r)
        {
            var before = r.BeforeRun; var restored = r.RestoredRun; var range = r.Range;
            Need(ReferenceEquals(before.Binding, restored.Binding) && ReferenceEquals(range.Binding, before.Binding) && range.SceneRevision == before.CurrentSnapshot.SceneRevision &&
                restored.CurrentSnapshot.SceneRevision == math.Add(before.CurrentSnapshot.SceneRevision, 1) && restored.FinalReport == null && range.Entries.Count > 0,
                "M06.Rollback", "IncompleteHistory");
            var index = before.Records.Count - range.Entries.Count; Need(index >= 0 && restored.Records.Count == index, "M06.Rollback.Range", "IncompleteHistory");
            Need(range.HistoryAnchorId == range.Entries[0].HistoryAnchorId && range.OperationId == range.Entries[0].OperationId &&
                ReferenceEquals(range.BeforeSnapshot, range.Entries[0].Record.BeforeSnapshot), "M06.Rollback.Range.Target", "IncompleteHistory");
            var anchors = new HashSet<string>(StringComparer.Ordinal);
            for (var i = 0; i < before.Records.Count; i++)
            {
                Need(before.Records[i].OperationId != r.OperationId, "M06.Rollback.OperationId", "IncompleteHistory");
                if (i < index) Need(ReferenceEquals(before.Records[i], restored.Records[i]), "M06.Rollback.Prefix", "IncompleteHistory");
                else { var e = range.Entries[i - index]; Need(ReferenceEquals(e.Record, before.Records[i]) && e.SupersededBy == null && anchors.Add(e.HistoryAnchorId), "M06.Rollback.Range.Entries", "IncompleteHistory"); }
            }
            Eq(range.BeforeSnapshot, restored.CurrentSnapshot, "M06.Rollback.RestoredRun", true);
        }
        private void Record(CandidateBattleOperationRecord r)
        {
            var p = "M06.Record[" + r.OperationId + "]"; var before = r.BeforeSnapshot; var after = r.AfterSnapshot; var stage = r.StageDecision; var source = r.Request;
            Need(ReferenceEquals(stage.BeforeSnapshot, before) && ReferenceEquals(before.Baseline, after.Baseline) && source.PlayerId == s.PlayerId && source.AttemptId == before.Baseline.Entry.AttemptId &&
                source.ExpectedSceneRevision == before.SceneRevision && after.SceneRevision == math.Add(before.SceneRevision, 1), p + ".Source", "IncompleteHistory");
            var locked = new List<FlowPathData>(); foreach (var route in before.Board.LockedRoutes) locked.Add(new FlowPathData {
                colorId = CandidateBattleSaveCodec.PairDefinition(before.Baseline, route.PairKey, p).GeometryColorId, cells = new List<FlowPos>(route.Route) });
            Route(before, source.Pair, source.Route, locked, p + ".Route");
            IReadOnlyList<BattleMemberState> members = before.Members; IReadOnlyList<BattleEnemyState> enemies = before.Enemies;
            IReadOnlyList<BattleContributionTotals> totals = before.Contributions; var random = before.Random; var increment = BigInteger.Zero;
            var expectedSegments = new List<CandidateContributionSegment>(); var ordered = 0;
            if (r.Kind == CandidateBattleOperationKind.Attack)
            {
                var d = r.DirectAttack; var e = r.EnemyPhase;
                Need(d != null && e != null && r.Conditions != null && r.Conditions.PreferenceRevision > 0 &&
                    r.Conditions.PreferenceRevision <= s.Inventory.PreferenceRevision && ReferenceEquals(d.BeforeSnapshot, before) && ReferenceEquals(e.DirectAttack, d) &&
                    ReferenceEquals(d.Binding.Start.Baseline, before.Baseline) && source.Kind == CandidateStageOperation.AfterAttack && before.Phase == BattlePhase.AwaitAction && d.DamageFacts.Count == 1,
                    p + ".Attack", "IncompleteHistory");
                var action = d.Action; Eq(source, new CandidateStageSource(CandidateStageOperation.AfterAttack, action.PlayerId, action.AttemptId, action.OperationId,
                    action.ExpectedSceneRevision, action.Actor, action.Pair, action.Route), p + ".Action");
                Need(action.ActionOrdinal == math.Add(before.EffectiveActionsCompleted, 1) && e.EnemyPhaseOrdinal == math.Add(before.EnemyPhasesCompleted, 1), p + ".Ordinals", "IncompleteHistory");
                var hit = d.DamageFacts[0]; var actor = CandidatePermanentSaveCodec.Find(before.Members, x => x.CombatantKey.Equals(hit.Actor), p + ".Actor"); var target = CandidatePermanentSaveCodec.Find(before.Enemies, x => x.CombatantKey.Equals(hit.Target), p + ".Target");
                Need(actor.CombatantKey.Equals(hit.Actor) && source.Actor.Equals(hit.Actor) && source.Pair.Equals(target.PairKey) && target.Hp.Numerator.Sign > 0 && actor.Hp.Numerator.Sign > 0,
                    p + ".DirectAttack.Identity", "IncompleteHistory");
                Eq(hit.Attack, actor.Member.Stats.Attack, p + ".DirectAttack.Attack"); Eq(hit.PhysicalDefense, target.Enemy.Stats.PhysicalDefense, p + ".DirectAttack.Defense");
                Eq(hit.HpBefore, target.Hp, p + ".DirectAttack.HpBefore"); Damage(hit.HpBefore, hit.HpAfter, hit.HpLoss, hit.Overflow, hit.RoundedDamage, hit.BlockPrevented, hit.ShieldAbsorbed, p + ".DirectAttack");
                Crit(hit.Crit, actor, hit, before, d.Random, p + ".Crit"); Eq(hit.Multiplier, hit.Crit.Triggered ? actor.Member.Crit.Multiplier : One(), p + ".Multiplier");
                DamageNumbers(hit.Attack, hit.Multiplier, hit.PhysicalDefense, hit.RawDamage, hit.MitigatedDamage, hit.RoundedDamage, p + ".DirectAttack");
                var distance = 1; foreach (var enemy in before.Enemies) if (enemy.Hp.Numerator.Sign > 0 && enemy.OriginalSlot < target.OriginalSlot) distance++;
                Need(distance <= actor.Member.Stats.AttackRange, p + ".AttackRange", "IncompleteHistory");
                Ordered(r, ordered, CandidateBattleFactKind.DirectAttack, hit, p);
                expectedSegments.Add(new CandidateContributionSegment(r.OperationId, before.SceneRevision, before.Board.Face.FaceId, CandidateBattleFactKind.DirectAttack,
                    0, hit.Actor, hit.Target, hit.Actor, CandidateContributionKind.DamageDealtHp, hit.HpLoss, ordered++));
                var directEnemies = new List<BattleEnemyState>(); foreach (var enemy in before.Enemies) directEnemies.Add(enemy.CombatantKey.Equals(target.CombatantKey)
                    ? new BattleEnemyState(enemy.CombatantKey, enemy.PairKey, enemy.Enemy, hit.HpAfter, enemy.IntentCursor) : enemy);
                Eq(d.Enemies, directEnemies, p + ".DirectAttack.Enemies"); Eq(d.Members, before.Members, p + ".DirectAttack.Members");
                var directTotals = new List<BattleContributionTotals>();
                foreach (var total in before.Contributions) directTotals.Add(total.CombatantKey.Equals(actor.CombatantKey)
                    ? new BattleContributionTotals(total.CombatantKey, total.EffectiveDamageDealtHp.Add(hit.HpLoss, math), total.EffectiveDamageTakenHp) : total);
                Eq(d.Contributions, directTotals, p + ".DirectAttack.Contributions");
                var alive = new List<BattleEnemyState>(); foreach (var enemy in d.Enemies) if (enemy.Hp.Numerator.Sign > 0) alive.Add(enemy);
                alive.Sort((a, b) => a.StableOrder.CompareTo(b.StableOrder));
                var hp = new Dictionary<BattleCombatantKey, ExactRational>(); var taken = new Dictionary<BattleCombatantKey, ExactRational>();
                foreach (var member in d.Members) hp.Add(member.CombatantKey, member.Hp);
                foreach (var total in directTotals) taken.Add(total.CombatantKey, total.EffectiveDamageTakenHp);
                var cursors = new Dictionary<BattleCombatantKey, BigInteger>();
                for (var i = 0; i < e.OrderedIntents.Count; i++)
                {
                    var front = Front(d.Members, hp);
                    Need(i < alive.Count && front != null, p + ".EnemyPhase.Coverage", "IncompleteHistory"); var intent = e.OrderedIntents[i]; var enemy = alive[i];
                    var definition = enemy.Enemy.IntentCycle[(int)math.Remainder(enemy.IntentCursor, enemy.Enemy.IntentCycle.Count)];
                    Need(intent.EnemyKey.Equals(enemy.CombatantKey) && intent.SegmentIndex == i && intent.IntentKind == definition.Kind && intent.CursorBefore == enemy.IntentCursor &&
                        intent.CursorAfter == math.Add(enemy.IntentCursor, 1), p + ".EnemyPhase.Intent", "IncompleteHistory");
                    Ordered(r, ordered, CandidateBattleFactKind.EnemyIntent, intent, p); cursors.Add(enemy.CombatantKey, intent.CursorAfter);
                    if (definition.Kind == EnemyIntentKind.Strike)
                    {
                        var h = intent.Damage; Need(h != null && h.ActorEnemy.Equals(enemy.CombatantKey) && h.TargetMember.Equals(front.CombatantKey), p + ".EnemyPhase.Damage", "IncompleteHistory");
                        Eq(h.HpBefore, hp[front.CombatantKey], p + ".EnemyPhase.HpBefore"); Eq(h.DamageCoefficient, definition.DamageCoefficient, p + ".EnemyPhase.Coefficient");
                        Eq(h.Attack, enemy.Enemy.Stats.Attack, p + ".EnemyPhase.Attack"); Eq(h.PhysicalDefense, front.Member.Stats.PhysicalDefense, p + ".EnemyPhase.Defense");
                        DamageNumbers(h.Attack, h.DamageCoefficient, h.PhysicalDefense, h.RawDamage, h.MitigatedDamage, h.RoundedDamage, p + ".EnemyPhase.Damage");
                        Damage(h.HpBefore, h.HpAfter, h.HpLoss, h.Overflow, h.RoundedDamage, h.BlockPrevented, h.ShieldAbsorbed, p + ".EnemyPhase.Damage"); hp[front.CombatantKey] = h.HpAfter; taken[front.CombatantKey] = taken[front.CombatantKey].Add(h.HpLoss, math);
                        expectedSegments.Add(new CandidateContributionSegment(r.OperationId, before.SceneRevision, before.Board.Face.FaceId, CandidateBattleFactKind.EnemyIntent,
                            i, h.ActorEnemy, h.TargetMember, h.TargetMember, CandidateContributionKind.DamageTakenHp, h.HpLoss, ordered));
                    }
                    else Need(intent.Damage == null, p + ".EnemyPhase.Charge", "IncompleteHistory"); ordered++;
                }
                Need(Front(d.Members, hp) == null || e.OrderedIntents.Count == alive.Count, p + ".EnemyPhase.Coverage", "IncompleteHistory");
                var finalEnemies = new List<BattleEnemyState>(); foreach (var enemy in d.Enemies) finalEnemies.Add(new BattleEnemyState(enemy.CombatantKey, enemy.PairKey, enemy.Enemy, enemy.Hp,
                    cursors.TryGetValue(enemy.CombatantKey, out var cursor) ? cursor : enemy.IntentCursor));
                var finalMembers = new List<BattleMemberState>(); var finalTotals = new List<BattleContributionTotals>();
                foreach (var member in d.Members) finalMembers.Add(new BattleMemberState(member.CombatantKey, member.Member, hp[member.CombatantKey]));
                foreach (var total in directTotals) finalTotals.Add(new BattleContributionTotals(total.CombatantKey, total.EffectiveDamageDealtHp, taken[total.CombatantKey]));
                Eq(e.Enemies, finalEnemies, p + ".EnemyPhase.Enemies"); Eq(e.Members, finalMembers, p + ".EnemyPhase.Members");
                Eq(e.Contributions, finalTotals, p + ".EnemyPhase.Contributions"); Eq(e.Random, d.Random, p + ".EnemyPhase.Random");
                members = e.Members; enemies = e.Enemies; totals = e.Contributions; random = e.Random; increment = 1;
            }
            else Need(r.DirectAttack == null && r.EnemyPhase == null && r.Conditions == null && source.Actor == null && source.Kind == CandidateStageOperation.CompleteLink &&
                before.Phase == BattlePhase.AwaitLinks && Contains(before.Board.PendingLinks, source.Pair), p + ".Link", "IncompleteHistory");
            Eq(r.ContributionSegments, expectedSegments, p + ".ContributionSegments");
            Stage(r, members, enemies, ref ordered, p); Need(ordered == r.OrderedFacts.Count, p + ".OrderedFacts", "IncompleteHistory");
            Need(after.EffectiveActionsCompleted == math.Add(before.EffectiveActionsCompleted, increment) && after.EnemyPhasesCompleted == math.Add(before.EnemyPhasesCompleted, increment), p + ".Counters", "IncompleteHistory");
            Eq(after.Members, members, p + ".After.Members"); Eq(after.Contributions, totals, p + ".After.Contributions"); Eq(after.Random, random, p + ".After.Random");
            if (stage.DidFlipFace)
            {
                var next = new List<BattleEnemyState>(); foreach (var pair in stage.NextFace.Pairs) next.Add(new BattleEnemyState(
                    BattleCombatantKey.ForEnemy(source.AttemptId, stage.NextFace.FaceId, pair.Enemy.EnemyInstanceKey), BattlePairKey.Create(source.AttemptId, stage.NextFace.FaceId, pair.PairId), pair.Enemy));
                enemies = next;
            }
            Eq(after.Enemies, enemies, p + ".After.Enemies");
        }
        private void Crit(CandidateCritFact c, BattleMemberState actor, BattleDamageFact hit, BattleSnapshot before, BattleRandomSnapshot after, string p)
        {
            var original = CandidatePermanentSaveCodec.Find(before.Random.PrdStates, x => x.CombatantKey.Equals(c.Actor), p + ".Before");
            var current = CandidatePermanentSaveCodec.Find(after.PrdStates, x => x.CombatantKey.Equals(c.Actor), p + ".After");
            Need(c.Actor.Equals(hit.Actor) && c.Target.Equals(hit.Target) && ReferenceEquals(c.Parameters, actor.Member.Crit) && c.OpportunityOrdinal == before.EffectiveActionsCompleted &&
                c.FailureCountBefore == original.FailureCount && after.PrdStates.Count == before.Random.PrdStates.Count &&
                c.FailureCountAfter == current.FailureCount && ReferenceEquals(current.Crit, original.Crit), p + ".Source", "IncompleteHistory");
            foreach (var old in before.Random.PrdStates) if (!old.CombatantKey.Equals(c.Actor))
            {
                var other = CandidatePermanentSaveCodec.Find(after.PrdStates, x => x.CombatantKey.Equals(old.CombatantKey), p + ".Other");
                Need(ReferenceEquals(old.Crit, other.Crit) && old.FailureCount == other.FailureCount, p + ".Other", "IncompleteHistory");
            }
            Eq(c.StreamBefore, before.Random.Stream, p + ".StreamBefore"); Eq(c.StreamAfter, after.Stream, p + ".StreamAfter");
            var q = c.Parameters.C.Multiply(ExactRational.Create(math.Add(c.FailureCountBefore, 1), 1, math), math); if (q.Compare(One(), math) > 0) q = One();
            Eq(q, c.Probability, p + ".Probability"); Need(c.FailureCountAfter == (c.Triggered ? BigInteger.Zero : math.Add(c.FailureCountBefore, 1)) &&
                math.Subtract(c.StreamAfter.WordsConsumed, c.StreamBefore.WordsConsumed) == c.Words.Count && (q.Compare(One(), math) != 0 || c.Triggered), p + ".Outcome", "IncompleteHistory");
        }
        private static BattleMemberState Front(IReadOnlyList<BattleMemberState> members, Dictionary<BattleCombatantKey, ExactRational> hp)
        {
            BattleMemberState front = null;
            foreach (var member in members) if (hp[member.CombatantKey].Numerator.Sign > 0 && (front == null || member.OriginalSlot < front.OriginalSlot)) front = member;
            return front;
        }
        private void Stage(CandidateBattleOperationRecord r, IReadOnlyList<BattleMemberState> members, IReadOnlyList<BattleEnemyState> enemies, ref int ordered, string p)
        {
            var stage = r.StageDecision; var source = r.Request; var before = r.BeforeSnapshot; var after = r.AfterSnapshot;
            Need(stage.FinalHp.MemberHp.Count == members.Count && stage.FinalHp.EnemyHp.Count == enemies.Count, p + ".FinalHp", "IncompleteHistory");
            for (var i = 0; i < members.Count; i++) { Need(stage.FinalHp.MemberHp[i].CombatantKey.Equals(members[i].CombatantKey), p + ".FinalHp.Members"); Eq(stage.FinalHp.MemberHp[i].Hp, members[i].Hp, p + ".FinalHp.Members.Hp"); }
            for (var i = 0; i < enemies.Count; i++) { Need(stage.FinalHp.EnemyHp[i].CombatantKey.Equals(enemies[i].CombatantKey), p + ".FinalHp.Enemies"); Eq(stage.FinalHp.EnemyHp[i].Hp, enemies[i].Hp, p + ".FinalHp.Enemies.Hp"); }
            var locked = new List<BattleLockedRoute>(before.Board.LockedRoutes); var pending = new List<BattlePairKey>(before.Board.PendingLinks); var routeFacts = 0; var phaseFacts = 0; var faceFacts = 0;
            for (var i = 0; i < stage.OrderedFacts.Count; i++)
            {
                var fact = stage.OrderedFacts[i]; Need(fact.SegmentIndex == i && fact.OperationId == r.OperationId && fact.SceneRevision == before.SceneRevision && fact.FaceId == before.Board.Face.FaceId, p + ".StageFacts", "IncompleteHistory");
                Ordered(r, ordered++, CandidateBattleFactKind.Stage, fact, p);
                switch (fact.Kind)
                {
                    case CandidateStageFactKind.RouteLocked:
                        Need(source.Pair.Equals(fact.Pair) && fact.NextFaceId == null && fact.Phase == null && (r.Kind == CandidateBattleOperationKind.Link || r.DirectAttack.DamageFacts[0].DefeatedTarget), p + ".RouteLocked", "IncompleteHistory");
                        locked.Add(new BattleLockedRoute(fact.Pair, source.Route)); routeFacts++; break;
                    case CandidateStageFactKind.TemporaryRouteRemoved:
                        Need(r.Kind == CandidateBattleOperationKind.Attack && !r.DirectAttack.DamageFacts[0].DefeatedTarget && source.Pair.Equals(fact.Pair) && fact.NextFaceId == null && fact.Phase == null, p + ".TemporaryRouteRemoved", "IncompleteHistory"); routeFacts++; break;
                    case CandidateStageFactKind.PendingLinkAdded:
                        Need(fact.Pair != null && !Contains(pending, fact.Pair) && fact.NextFaceId == null && fact.Phase == null, p + ".PendingLinkAdded", "IncompleteHistory"); pending.Add(fact.Pair); break;
                    case CandidateStageFactKind.PendingLinkRemoved:
                        Need(r.Kind == CandidateBattleOperationKind.Link && source.Pair.Equals(fact.Pair) && pending.Remove(fact.Pair) && fact.NextFaceId == null && fact.Phase == null, p + ".PendingLinkRemoved", "IncompleteHistory"); break;
                    case CandidateStageFactKind.FaceChanged:
                        Need(stage.NextFace != null && fact.NextFaceId == stage.NextFace.FaceId && fact.Pair == null && fact.Phase == null, p + ".FaceChanged", "IncompleteHistory"); faceFacts++; break;
                    case CandidateStageFactKind.PhaseSelected:
                        Need(i == stage.OrderedFacts.Count - 1 && fact.Phase == stage.NextPhase && fact.Pair == null && fact.NextFaceId == null, p + ".PhaseSelected", "IncompleteHistory"); phaseFacts++; break;
                }
            }
            Need(routeFacts == 1 && phaseFacts == 1 && faceFacts == (stage.DidFlipFace ? 1 : 0), p + ".StageFacts.Coverage", "IncompleteHistory");
            foreach (var enemy in enemies)
            { var marked = Contains(pending, enemy.PairKey); foreach (var route in locked) if (route.PairKey.Equals(enemy.PairKey)) marked = true; Need(marked == enemy.Hp.Numerator.IsZero, p + ".Stage.DeathCoverage", "IncompleteHistory"); }
            var board = new BattleBoardState(before.Board.Face, locked, pending);
            if (stage.DidFlipFace)
            {
                Need(pending.Count == 0 && locked.Count == before.Enemies.Count && stage.NextFaceIndex == before.CurrentFaceIndex + 1 && stage.NextFaceIndex < before.Baseline.Entry.Level.Faces.Count &&
                    ReferenceEquals(stage.NextFace, before.Baseline.Entry.Level.Faces[stage.NextFaceIndex]), p + ".NextFace", "IncompleteHistory"); board = new BattleBoardState(stage.NextFace);
            }
            else Need(stage.NextFaceIndex == before.CurrentFaceIndex, p + ".NextFaceIndex", "IncompleteHistory");
            Eq(stage.Board, board, p + ".Stage.Board"); Eq(after.Board, board, p + ".After.Board");
            Need(after.CurrentFaceIndex == stage.NextFaceIndex && after.Phase == stage.NextPhase, p + ".After.Stage", "IncompleteHistory");
        }
        private void Route(BattleSnapshot snapshot, BattlePairKey key, IReadOnlyList<FlowPos> route, IReadOnlyList<FlowPathData> locked, string p)
        {
            var face = snapshot.Board.Face; var level = new FlowLevelData { width = face.Width, height = face.Height, pairs = new List<FlowPairData>() };
            foreach (var pair in face.Pairs) level.pairs.Add(new FlowPairData { colorId = pair.GeometryColorId, endpointA = pair.EndpointA, endpointB = pair.EndpointB });
            Need(key.FaceId == face.FaceId, p + ".FaceId", "InconsistentBinding"); var original = CandidateBattleSaveCodec.PairDefinition(snapshot.Baseline, key, p);
            var result = new BattleRouteValidator().Validate(level, locked, Array.Empty<FlowPos>(), original.GeometryColorId, route); Need(result.IsValid, p + "." + result.ReasonCode, "IncompleteHistory");
        }
        private void Damage(ExactRational before, ExactRational after, ExactRational loss, ExactRational overflow, BigInteger rounded, ExactRational block, ExactRational shield, string p)
        {
            Need(before.Numerator.Sign > 0 && rounded >= 0 && block.Numerator.IsZero && shield.Numerator.IsZero, p, "IncompleteHistory");
            Nonnegative(after, p + ".HpAfter"); Nonnegative(loss, p + ".HpLoss"); Nonnegative(overflow, p + ".Overflow");
            Eq(before.Subtract(loss, math), after, p + ".HpConservation"); Eq(loss.Add(overflow, math), ExactRational.Create(rounded, 1, math), p + ".DamageConservation");
            Need(overflow.Numerator.IsZero || after.Numerator.IsZero, p + ".Overflow", "IncompleteHistory");
        }
        private void DamageNumbers(ExactRational attack, ExactRational multiplier, ExactRational defense, ExactRational raw, ExactRational mitigated, BigInteger rounded, string p)
        {
            var hundred = ExactRational.Create(100, 1, math); Eq(raw, attack.Multiply(multiplier, math), p + ".Raw");
            Eq(mitigated, raw.Multiply(hundred, math).Divide(hundred.Add(defense, math), math), p + ".Mitigated");
            Need(rounded == mitigated.Floor(math), p + ".Rounded", "IncompleteHistory");
        }
        private void Ordered(CandidateBattleOperationRecord r, int index, CandidateBattleFactKind kind, object payload, string p)
        {
            Need(index < r.OrderedFacts.Count, p + ".OrderedFacts", "IncompleteHistory"); var fact = r.OrderedFacts[index];
            Need(fact.Index == index && fact.Kind == kind && ReferenceEquals(fact.DirectAttack, kind == CandidateBattleFactKind.DirectAttack ? payload : null) &&
                ReferenceEquals(fact.EnemyIntent, kind == CandidateBattleFactKind.EnemyIntent ? payload : null) && ReferenceEquals(fact.Stage, kind == CandidateBattleFactKind.Stage ? payload : null), p + ".OrderedFacts", "IncompleteHistory");
        }
        private void Hp(ExactRational value, ExactRational max, string p) { Need(value.Numerator.Sign >= 0 && value.Compare(max, math) <= 0, p); }
        private static void Nonnegative(ExactRational value, string p) { Need(value.Numerator.Sign >= 0, p); }
        private static bool Contains<T>(IReadOnlyList<T> rows, T value) { foreach (var row in rows) if (Equals(row, value)) return true; return false; }
        private void Player(string value, string path) { Need(value == s.PlayerId, path, "InconsistentBinding"); }
        private void Eq(object a, object b, string path, bool ignoreRevision = false)
        { Need(CandidateBattleReportFingerprint.Equal(a, b, math, ignoreRevision), path, "InconsistentBinding"); }
        private ExactRational Zero() { return ExactRational.Create(0, 1, math); }
        private ExactRational One() { return ExactRational.Create(1, 1, math); }
        private static bool ContainsReference<T>(IReadOnlyList<T> rows, T value) where T : class
        { foreach (var row in rows) if (ReferenceEquals(row, value)) return true; return false; }
        private static bool SameActor(CandidateInventoryActor a, CandidateInventoryActor b)
        { return a != null && b != null && a.CharacterId == b.CharacterId && a.ClassId == b.ClassId && a.ClassKind == b.ClassKind && a.OriginalSlot == b.OriginalSlot; }
        private static bool SameParticipant(CandidateProgressionParticipant a, CandidateProgressionParticipant b)
        { return a.CharacterId == b.CharacterId && a.ClassId == b.ClassId && a.ClassKind == b.ClassKind && a.OriginalSlot == b.OriginalSlot && a.CharacterRevision == b.CharacterRevision; }
        private static bool SameLevel(CandidateProgressionLevel a, CandidateProgressionLevel b)
        { return a.LevelId == b.LevelId && a.LevelVersion == b.LevelVersion && a.UnlockRuleId == b.UnlockRuleId && a.EntryKind == b.EntryKind && a.UnlockKind == b.UnlockKind && a.UnlockAfterLevelId == b.UnlockAfterLevelId; }
    }
}
