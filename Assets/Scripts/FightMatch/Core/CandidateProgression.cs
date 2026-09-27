using System;
using System.Collections.Generic;
using System.Numerics;
using static FightMatch.Core.CandidateProgressionRejectionCode;
using static FightMatch.Core.ProgressionChecks;

namespace FightMatch.Core
{
    public static class CandidateProgression
    {
        public static CandidateProgressionDefinitionResult PrepareDefinition(CandidateProgressionDefinitionInput input, ExactMathBudget budget)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            var c = new ProgressionChecks(budget);
            if (!c.Context(input.Context)) return new CandidateProgressionDefinitionResult(c);
            if (input.Levels == null) return DefinitionReject(c, MissingField, "Levels");
            if (input.Levels.Count == 0) return DefinitionReject(c, InvalidValue, "Levels");
            var levels = new List<CandidateProgressionLevel>(); var ids = new HashSet<string>(StringComparer.Ordinal);
            var rules = new HashSet<string>(StringComparer.Ordinal); var hasInitial = false;
            for (var i = 0; i < input.Levels.Count; i++)
            {
                var row = input.Levels[i]; var p = $"Levels[{i}]";
                if (row == null) return DefinitionReject(c, MissingField, p);
                if (!c.Text(row.LevelId, p + ".LevelId") || !c.Text(row.LevelVersion, p + ".LevelVersion") ||
                    !c.Text(row.UnlockRuleId, p + ".UnlockRuleId")) return new CandidateProgressionDefinitionResult(c);
                if (!ids.Add(row.LevelId)) return DefinitionReject(c, InvalidValue, p + ".LevelId");
                if (!rules.Add(row.UnlockRuleId)) return DefinitionReject(c, InvalidValue, p + ".UnlockRuleId");
                if (row.EntryKind != CandidateProgressionEntryKind.Ordinary)
                    return DefinitionReject(c, row.EntryKind == CandidateProgressionEntryKind.Unspecified ? MissingField : UnsupportedBinding, p + ".EntryKind");
                if (row.RequiredFeatures == null) return DefinitionReject(c, MissingField, p + ".RequiredFeatures");
                if (row.RequiredFeatures.Count != 0) return DefinitionReject(c, UnsupportedBinding, p + ".RequiredFeatures");
                if (row.UnlockKind == CandidateProgressionUnlockKind.Unspecified) return DefinitionReject(c, MissingField, p + ".UnlockKind");
                if (!row.HasPrerequisite) return DefinitionReject(c, MissingField, p + ".UnlockAfterLevelId");
                if (row.UnlockKind == CandidateProgressionUnlockKind.InitiallyOpen)
                { if (row.UnlockAfterLevelId != null) return DefinitionReject(c, InvalidValue, p + ".UnlockAfterLevelId"); hasInitial = true; }
                else if (row.UnlockKind == CandidateProgressionUnlockKind.AfterWholeLevelClear)
                {
                    if (!c.Text(row.UnlockAfterLevelId, p + ".UnlockAfterLevelId")) return new CandidateProgressionDefinitionResult(c);
                    if (Equal(row.LevelId, row.UnlockAfterLevelId)) return DefinitionReject(c, InvalidValue, p + ".UnlockAfterLevelId");
                }
                else return DefinitionReject(c, UnsupportedBinding, p + ".UnlockKind");
                levels.Add(new CandidateProgressionLevel(row));
            }
            if (!hasInitial) return DefinitionReject(c, InvalidValue, "Levels");
            for (var i = 0; i < levels.Count; i++)
            {
                var visited = new HashSet<string>(StringComparer.Ordinal); var cursor = levels[i];
                while (cursor.UnlockAfterLevelId != null)
                {
                    if (!visited.Add(cursor.LevelId)) return DefinitionReject(c, InvalidValue, $"Levels[{i}].UnlockAfterLevelId");
                    cursor = FindLevel(levels, cursor.UnlockAfterLevelId);
                    if (cursor == null) return DefinitionReject(c, InconsistentBinding, $"Levels[{i}].UnlockAfterLevelId");
                }
            }
            return new CandidateProgressionDefinitionResult(new CandidateProgressionDefinition(input.Context, levels));
        }

        public static CandidateProgressionResult CreateCandidate(CandidateProgressionDefinition definition, string playerId, ExactMathBudget budget)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            var c = new ProgressionChecks(budget); RuleContextChecks.CheckBudget(definition.Context, budget); budget.CheckInteger(BigInteger.One);
            if (!c.Text(playerId, "PlayerId")) return new CandidateProgressionResult(c);
            var open = new List<CandidateProgressionOpenFact>();
            foreach (var level in definition.Levels) if (level.UnlockKind == CandidateProgressionUnlockKind.InitiallyOpen)
                open.Add(new CandidateProgressionOpenFact(playerId, level, definition.Context, null));
            var state = new CandidateProgressionState(playerId, definition, BigInteger.One, open,
                new CandidateProgressionFirstClear[0], new CandidateProgressionChallenge[0]);
            return new CandidateProgressionResult(state, CandidateProgressionOutcome.Changed, added: open);
        }

        public static CandidateProgressionView Read(CandidateProgressionState state, ExactMathBudget budget)
        {
            Begin(state, budget); var levels = new List<CandidateProgressionLevelView>();
            foreach (var level in state.Definition.Levels)
            {
                var challenges = new List<CandidateProgressionChallenge>();
                foreach (var challenge in state.Challenges) if (Equal(challenge.Level.LevelId, level.LevelId)) challenges.Add(challenge);
                levels.Add(new CandidateProgressionLevelView(level, Open(state, level.LevelId), Clear(state, level.LevelId),
                    CurrentChallenge(state, level.LevelId), challenges));
            }
            return new CandidateProgressionView(state, levels);
        }

        public static CandidateProgressionEntryCheck CheckEntry(CandidateProgressionState state, CandidateProgressionLevelRequest request,
            CandidateCharacterState character, ExactMathBudget budget)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (character == null) throw new ArgumentNullException(nameof(character));
            var c = Begin(state, budget); var stats = CandidateCharacterGrowth.ComputeBaseStats(character, budget);
            if (!c.Request(state, request, out var level) || !Entry(state, request, character, stats, c)) return new CandidateProgressionEntryCheck(c);
            return new CandidateProgressionEntryCheck(state, level, Open(state, level.LevelId), CurrentChallenge(state, level.LevelId),
                new CandidateProgressionParticipant(stats, character.StateRevision));
        }

        public static CandidateProgressionResult BeginAttempt(CandidateProgressionState state, CandidateProgressionBeginIntent intent,
            CandidateCharacterState character, BigInteger expectedRevision, ExactMathBudget budget)
        {
            if (intent == null) throw new ArgumentNullException(nameof(intent));
            if (character == null) throw new ArgumentNullException(nameof(character));
            var c = Begin(state, budget); var stats = CandidateCharacterGrowth.ComputeBaseStats(character, budget);
            if (!c.Request(state, intent, out var level) || !c.Text(intent.ChallengeId, "ChallengeId") ||
                !c.Text(intent.AttemptId, "AttemptId") || !c.Text(intent.EntryBaselineId, "EntryBaselineId")) return new CandidateProgressionResult(c);
            var prior = FindAttempt(state, intent.AttemptId);
            if (prior != null)
            {
                var original = prior.Begin;
                if (!c.Same(original.Level.LevelId, intent.LevelId, "LevelId") || !c.Same(original.ChallengeId, intent.ChallengeId, "ChallengeId") ||
                    !c.Same(original.EntryBaselineId, intent.EntryBaselineId, "EntryBaselineId") ||
                    !c.Same(original.Participant.CharacterId, intent.CharacterId, "CharacterId")) return new CandidateProgressionResult(c);
                if (budget.Compare(original.Participant.CharacterRevision, intent.ExpectedCharacterRevision.Value) != 0) return Reject(c, InconsistentBinding, "ExpectedCharacterRevision");
                if (original.Participant.OriginalSlot != intent.ExpectedOriginalSlot.Value) return Reject(c, InconsistentBinding, "ExpectedOriginalSlot");
                return new CandidateProgressionResult(state, CandidateProgressionOutcome.AlreadyIncluded, begin: original);
            }
            if (!c.Revision(state.StateRevision, expectedRevision) || !Entry(state, intent, character, stats, c)) return new CandidateProgressionResult(c);
            var current = CurrentChallenge(state, level.LevelId);
            if (current != null && !Equal(current.ChallengeId, intent.ChallengeId)) return Reject(c, ChallengeConflict, "ChallengeId");
            if (current == null) foreach (var challenge in state.Challenges)
                if (Equal(challenge.ChallengeId, intent.ChallengeId)) return Reject(c, ChallengeConflict, "ChallengeId");
            var receipt = new CandidateProgressionBeginReceipt(state.PlayerId, level, state.Definition.Context, intent.ChallengeId,
                intent.AttemptId, intent.EntryBaselineId, new CandidateProgressionParticipant(stats, character.StateRevision));
            var attempts = current == null ? new List<CandidateProgressionAttempt>() : new List<CandidateProgressionAttempt>(current.Attempts);
            attempts.Add(new CandidateProgressionAttempt(receipt, null));
            var replacement = new CandidateProgressionChallenge(intent.ChallengeId, level, attempts, null, current?.GetTeachingBinding());
            return new CandidateProgressionResult(Change(state, Replace(state, current, replacement), state.OpenFacts, state.FirstClears, budget),
                CandidateProgressionOutcome.Changed, begin: receipt);
        }

        public static CandidateProgressionResult EndAttempt(CandidateProgressionState state, CandidateProgressionEndFacts facts,
            BigInteger expectedRevision, ExactMathBudget budget)
        {
            if (facts == null) throw new ArgumentNullException(nameof(facts));
            var c = Begin(state, budget);
            if (!c.Binding(state, facts.PlayerId, facts.LevelId, facts.LevelVersion, facts.Context, out var level) ||
                !c.Text(facts.ChallengeId, "ChallengeId") || !c.Text(facts.AttemptId, "AttemptId") ||
                !c.Text(facts.EntryBaselineId, "EntryBaselineId") || !c.Text(facts.EndReceiptId, "EndReceiptId")) return new CandidateProgressionResult(c);
            if (facts.Kind == CandidateProgressionEndKind.Unspecified) return Reject(c, MissingField, "Kind");
            if (facts.Kind != CandidateProgressionEndKind.NormalVictory && facts.Kind != CandidateProgressionEndKind.NormalExit &&
                facts.Kind != CandidateProgressionEndKind.ImmediateRestart) return Reject(c, UnsupportedBinding, "Kind");
            if (!facts.HasSettlement) return Reject(c, MissingField, "SettlementId");
            if (!facts.HasFingerprint) return Reject(c, MissingField, "FinalReportFingerprint");
            if (!facts.HasNewAttempt) return Reject(c, MissingField, "NewAttemptId");
            if (facts.Kind == CandidateProgressionEndKind.NormalVictory)
            { if (!c.Text(facts.SettlementId, "SettlementId") || !c.Text(facts.FinalReportFingerprint, "FinalReportFingerprint")) return new CandidateProgressionResult(c); }
            else
            {
                if (facts.SettlementId != null) return Reject(c, InvalidValue, "SettlementId");
                if (facts.FinalReportFingerprint != null) return Reject(c, InvalidValue, "FinalReportFingerprint");
            }
            if (facts.Kind == CandidateProgressionEndKind.ImmediateRestart)
            {
                if (!c.Text(facts.NewAttemptId, "NewAttemptId")) return new CandidateProgressionResult(c);
                if (Equal(facts.NewAttemptId, facts.AttemptId)) return Reject(c, InconsistentBinding, "NewAttemptId");
            }
            else if (facts.NewAttemptId != null) return Reject(c, InvalidValue, "NewAttemptId");
            var original = FindAttempt(state, facts.AttemptId);
            if (original == null) return Reject(c, InconsistentBinding, "AttemptId");
            if (!c.Same(original.Begin.Level.LevelId, facts.LevelId, "LevelId") || !c.Same(original.Begin.ChallengeId, facts.ChallengeId, "ChallengeId") ||
                !c.Same(original.Begin.EntryBaselineId, facts.EntryBaselineId, "EntryBaselineId")) return new CandidateProgressionResult(c);
            if (original.End != null)
            {
                var end = original.End;
                if (end.Kind != facts.Kind) return Reject(c, AttemptAlreadyClosed, "Kind");
                if (!c.Same(end.EndReceiptId, facts.EndReceiptId, "EndReceiptId") || !c.Same(end.SettlementId, facts.SettlementId, "SettlementId") ||
                    !c.Same(end.FinalReportFingerprint, facts.FinalReportFingerprint, "FinalReportFingerprint") ||
                    !c.Same(end.NewAttemptId, facts.NewAttemptId, "NewAttemptId")) return new CandidateProgressionResult(c);
                return new CandidateProgressionResult(state, CandidateProgressionOutcome.AlreadyIncluded, end: end,
                    successors: Successors(state, level.LevelId));
            }
            if (!c.Revision(state.StateRevision, expectedRevision)) return new CandidateProgressionResult(c);
            var current = CurrentChallenge(state, level.LevelId);
            if (current == null || !Equal(current.ChallengeId, facts.ChallengeId)) return Reject(c, ChallengeConflict, "ChallengeId");
            if (state.ActiveAttempt != original) return Reject(c, ActiveAttemptConflict, "AttemptId");
            foreach (var challenge in state.Challenges) foreach (var attempt in challenge.Attempts) if (attempt.End != null)
            {
                if (Equal(attempt.End.EndReceiptId, facts.EndReceiptId)) return Reject(c, InconsistentBinding, "EndReceiptId");
                if (facts.SettlementId != null && Equal(attempt.End.SettlementId, facts.SettlementId)) return Reject(c, InconsistentBinding, "SettlementId");
            }
            if (facts.Kind == CandidateProgressionEndKind.ImmediateRestart && FindAttempt(state, facts.NewAttemptId) != null)
                return Reject(c, InconsistentBinding, "NewAttemptId");
            var victory = facts.Kind == CandidateProgressionEndKind.NormalVictory;
            var sourceClear = Clear(state, level.LevelId); var isFirst = victory && sourceClear == null;
            var receipt = new CandidateProgressionEndReceipt(original.Begin, facts, isFirst);
            var attempts = new List<CandidateProgressionAttempt>();
            foreach (var attempt in current.Attempts) attempts.Add(attempt == original ? new CandidateProgressionAttempt(attempt.Begin, receipt) : attempt);
            CandidateProgressionBeginReceipt restart = null;
            if (facts.Kind == CandidateProgressionEndKind.ImmediateRestart)
            {
                restart = new CandidateProgressionBeginReceipt(state.PlayerId, level, original.Begin.Context, current.ChallengeId,
                    facts.NewAttemptId, original.Begin.EntryBaselineId, original.Begin.Participants, original.Begin.FormationRevision);
                attempts.Add(new CandidateProgressionAttempt(restart, null));
            }
            var replacement = new CandidateProgressionChallenge(current.ChallengeId, level, attempts, victory ? receipt : null, current.GetTeachingBinding());
            var clears = new List<CandidateProgressionFirstClear>(state.FirstClears); CandidateProgressionFirstClear addedClear = null;
            if (isFirst) { addedClear = new CandidateProgressionFirstClear(receipt); sourceClear = addedClear; clears.Add(addedClear); }
            var open = new List<CandidateProgressionOpenFact>(state.OpenFacts); var added = new List<CandidateProgressionOpenFact>();
            if (victory) foreach (var candidate in state.Definition.Levels)
                if (Equal(candidate.UnlockAfterLevelId, level.LevelId) && Open(state, candidate.LevelId) == null)
                { var fact = new CandidateProgressionOpenFact(state.PlayerId, candidate, state.Definition.Context, sourceClear); open.Add(fact); added.Add(fact); }
            var next = Change(state, Replace(state, current, replacement), open, clears, budget);
            return new CandidateProgressionResult(next, CandidateProgressionOutcome.Changed, restart, receipt, addedClear, added, Successors(next, level.LevelId));
        }

        public static CandidateProgressionEntryCheck CheckEntry(CandidateProgressionState state,
            CandidateProgressionRosterIntent request, ExactMathBudget budget)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            var c = Begin(state, budget);
            if (!c.Binding(state, request.PlayerId, request.LevelId, request.LevelVersion, request.Context, out var level) ||
                !RosterParticipants(state, request, c, out var participants)) return new CandidateProgressionEntryCheck(c);
            if (Open(state, request.LevelId) == null) c.Fail(Locked, "LevelId");
            else if (state.ActiveAttempt != null) c.Fail(ActiveAttemptConflict, "AttemptId");
            else return new CandidateProgressionEntryCheck(state, level, Open(state, level.LevelId), CurrentChallenge(state, level.LevelId), participants);
            return new CandidateProgressionEntryCheck(c);
        }

        public static CandidateProgressionResult BeginAttempt(CandidateProgressionState state,
            CandidateProgressionRosterIntent intent, BigInteger expectedRevision, ExactMathBudget budget)
        {
            if (intent == null) throw new ArgumentNullException(nameof(intent));
            var c = Begin(state, budget);
            if (!c.Binding(state, intent.PlayerId, intent.LevelId, intent.LevelVersion, intent.Context, out var level) ||
                !RosterParticipants(state, intent, c, out var participants) || !c.Text(intent.ChallengeId, "ChallengeId") ||
                !c.Text(intent.AttemptId, "AttemptId") || !c.Text(intent.EntryBaselineId, "EntryBaselineId")) return new CandidateProgressionResult(c);
            var prior = FindAttempt(state, intent.AttemptId);
            if (prior != null)
            {
                var original = prior.Begin;
                if (original.FormationRevision != intent.FormationRevision || original.Level.LevelId != intent.LevelId ||
                    original.ChallengeId != intent.ChallengeId || original.EntryBaselineId != intent.EntryBaselineId ||
                    original.Participants.Count != participants.Count) return Reject(c, InconsistentBinding, "Begin");
                for (var i = 0; i < participants.Count; i++)
                    if (original.Participants[i].CharacterId != participants[i].CharacterId || original.Participants[i].ClassId != participants[i].ClassId ||
                        original.Participants[i].CharacterRevision != participants[i].CharacterRevision || original.Participants[i].OriginalSlot != participants[i].OriginalSlot)
                        return Reject(c, InconsistentBinding, "Participants");
                return new CandidateProgressionResult(state, CandidateProgressionOutcome.AlreadyIncluded, begin: original);
            }
            if (!c.Revision(state.StateRevision, expectedRevision)) return new CandidateProgressionResult(c);
            var entry = CheckEntry(state, intent, budget);
            if (!entry.IsAccepted) return Reject(c, entry.RejectionCode, entry.FieldPath);
            var current = CurrentChallenge(state, level.LevelId);
            if (current != null && !Equal(current.ChallengeId, intent.ChallengeId)) return Reject(c, ChallengeConflict, "ChallengeId");
            if (current == null) foreach (var challenge in state.Challenges)
                if (Equal(challenge.ChallengeId, intent.ChallengeId)) return Reject(c, ChallengeConflict, "ChallengeId");
            var receipt = new CandidateProgressionBeginReceipt(state.PlayerId, level, state.Definition.Context, intent.ChallengeId,
                intent.AttemptId, intent.EntryBaselineId, participants, intent.FormationRevision);
            var attempts = current == null ? new List<CandidateProgressionAttempt>() : new List<CandidateProgressionAttempt>(current.Attempts);
            attempts.Add(new CandidateProgressionAttempt(receipt, null));
            var replacement = new CandidateProgressionChallenge(intent.ChallengeId, level, attempts, null, current?.GetTeachingBinding());
            return new CandidateProgressionResult(Change(state, Replace(state, current, replacement), state.OpenFacts, state.FirstClears, budget),
                CandidateProgressionOutcome.Changed, begin: receipt);
        }

        private static bool RosterParticipants(CandidateProgressionState state, CandidateProgressionRosterIntent request,
            ProgressionChecks c, out List<CandidateProgressionParticipant> result)
        {
            result = new List<CandidateProgressionParticipant>();
            if (request.Participants == null) return c.Fail(MissingField, "Participants");
            if (request.Participants.Count < 1 || request.Participants.Count > 3) return c.Fail(NoReadyMember, "Participants");
            if (!c.Number(request.FormationRevision, "FormationRevision", true)) return false;
            var ids = new HashSet<string>(); var classes = new HashSet<string>(); var slots = new HashSet<int>();
            foreach (var entry in request.Participants)
            {
                if (entry == null) return c.Fail(MissingField, "Participants");
                var character = entry.Character; var stats = entry.Stats;
                if (!c.Same(state.PlayerId, character.PlayerId, "Participant.PlayerId") ||
                    !c.ContextFields(state.Definition.Context, stats.Context, "Participant.Context")) return false;
                if (!character.IsReady) return c.Fail(NoReadyMember, "Participant.IsReady");
                if (stats.ClassKind != CharacterClassKind.Warrior) return c.Fail(UnsupportedBinding, "Participant.ClassKind");
                if (!ids.Add(character.CharacterId) || !classes.Add(character.ClassId) || !slots.Add(entry.OriginalSlot) ||
                    entry.OriginalSlot < 0 || entry.OriginalSlot > 2) return c.Fail(InconsistentBinding, "Participants");
                c.Math.CheckInteger(character.StateRevision);
                result.Add(new CandidateProgressionParticipant(character.CharacterId, character.ClassId, stats.ClassKind, character.StateRevision, entry.OriginalSlot));
            }
            result.Sort((a, b) => a.OriginalSlot.CompareTo(b.OriginalSlot)); return true;
        }

        private static ProgressionChecks Begin(CandidateProgressionState state, ExactMathBudget budget)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            var c = new ProgressionChecks(budget); c.CheckState(state); return c;
        }
        private static CandidateProgressionDefinitionResult DefinitionReject(ProgressionChecks c, CandidateProgressionRejectionCode code, string path)
        { c.Fail(code, path); return new CandidateProgressionDefinitionResult(c); }
        private static CandidateProgressionResult Reject(ProgressionChecks c, CandidateProgressionRejectionCode code, string path)
        { c.Fail(code, path); return new CandidateProgressionResult(c); }
        private static bool Entry(CandidateProgressionState state, CandidateProgressionLevelRequest request,
            CandidateCharacterState character, CandidateComputedStats stats, ProgressionChecks c)
        {
            if (Open(state, request.LevelId) == null) return c.Fail(Locked, "LevelId");
            if (state.ActiveAttempt != null) return c.Fail(ActiveAttemptConflict, "AttemptId");
            if (!c.Same(state.PlayerId, stats.PlayerId, "Character.PlayerId") || !c.Same(request.CharacterId, stats.CharacterId, "CharacterId")) return false;
            if (stats.ClassKind != CharacterClassKind.Warrior) return c.Fail(UnsupportedBinding, "Character.ClassKind");
            var context = stats.Context;
            if (!c.ContextFields(state.Definition.Context, context, "Character.Context") ||
                !c.Revision(character.StateRevision, request.ExpectedCharacterRevision.Value, "ExpectedCharacterRevision")) return false;
            c.Math.CheckInteger(stats.OriginalSlot);
            if (stats.OriginalSlot != request.ExpectedOriginalSlot.Value) return c.Fail(InconsistentBinding, "ExpectedOriginalSlot");
            return stats.IsReady || c.Fail(NoReadyMember, "Character.IsReady");
        }
        private static CandidateProgressionAttempt FindAttempt(CandidateProgressionState state, string id)
        { foreach (var challenge in state.Challenges) foreach (var attempt in challenge.Attempts) if (Equal(attempt.Begin.AttemptId, id)) return attempt; return null; }
        private static CandidateProgressionChallenge CurrentChallenge(CandidateProgressionState state, string level)
        { foreach (var challenge in state.Challenges) if (!challenge.IsClosed && Equal(challenge.Level.LevelId, level)) return challenge; return null; }
        private static CandidateProgressionOpenFact Open(CandidateProgressionState state, string level)
        { foreach (var fact in state.OpenFacts) if (Equal(fact.Level.LevelId, level)) return fact; return null; }
        private static CandidateProgressionFirstClear Clear(CandidateProgressionState state, string level)
        { foreach (var fact in state.FirstClears) if (Equal(fact.LevelId, level)) return fact; return null; }
        private static List<CandidateProgressionChallenge> Replace(CandidateProgressionState state,
            CandidateProgressionChallenge current, CandidateProgressionChallenge replacement)
        {
            var result = new List<CandidateProgressionChallenge>();
            foreach (var challenge in state.Challenges) result.Add(challenge == current ? replacement : challenge);
            if (current == null) result.Add(replacement); return result;
        }
        private static CandidateProgressionState Change(CandidateProgressionState state, IEnumerable<CandidateProgressionChallenge> challenges,
            IEnumerable<CandidateProgressionOpenFact> open, IEnumerable<CandidateProgressionFirstClear> clears, ExactMathBudget budget)
        { return new CandidateProgressionState(state.PlayerId, state.Definition, budget.Add(state.StateRevision, BigInteger.One), open, clears, challenges, state.GetPermanentEffects()); }
        private static List<CandidateProgressionOpenFact> Successors(CandidateProgressionState state, string level)
        {
            var result = new List<CandidateProgressionOpenFact>();
            foreach (var candidate in state.Definition.Levels) if (Equal(candidate.UnlockAfterLevelId, level))
            { var open = Open(state, candidate.LevelId); if (open != null) result.Add(open); }
            return result;
        }
    }
}
