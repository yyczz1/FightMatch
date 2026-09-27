using System.Collections.Generic;
using System.Numerics;

namespace FightMatch.Core
{
    public sealed class CandidateProgressionParticipant
    {
        public string CharacterId { get; }
        public string ClassId { get; }
        public CharacterClassKind ClassKind { get; }
        public BigInteger CharacterRevision { get; }
        public int OriginalSlot { get; }
        internal CandidateProgressionParticipant(CandidateComputedStats stats, BigInteger revision)
        { CharacterId = stats.CharacterId; ClassId = stats.ClassId; ClassKind = stats.ClassKind; OriginalSlot = stats.OriginalSlot; CharacterRevision = revision; }
        internal CandidateProgressionParticipant(string characterId, string classId, CharacterClassKind classKind,
            BigInteger characterRevision, int originalSlot)
        { CharacterId = characterId; ClassId = classId; ClassKind = classKind; CharacterRevision = characterRevision; OriginalSlot = originalSlot; }
    }
    public sealed class CandidateProgressionBeginReceipt
    {
        public string PlayerId { get; }
        public CandidateProgressionLevel Level { get; }
        public PreparedRuleContext Context { get; }
        public string ChallengeId { get; }
        public string AttemptId { get; }
        public string EntryBaselineId { get; }
        public CandidateProgressionParticipant Participant => CandidateRosterState.Single(Participants);
        internal IReadOnlyList<CandidateProgressionParticipant> Participants { get; }
        internal BigInteger? FormationRevision { get; }
        public IReadOnlyList<CandidateProgressionParticipant> GetParticipants() => Participants;
        public BigInteger? GetFormationRevision() => FormationRevision;
        internal CandidateProgressionBeginReceipt(string player, CandidateProgressionLevel level, PreparedRuleContext context,
            string challenge, string attempt, string baseline, CandidateProgressionParticipant participant)
            : this(player, level, context, challenge, attempt, baseline, new[] { participant }, null) { }
        internal CandidateProgressionBeginReceipt(string player, CandidateProgressionLevel level, PreparedRuleContext context,
            string challenge, string attempt, string baseline, IReadOnlyList<CandidateProgressionParticipant> participants, BigInteger? formationRevision)
        {
            PlayerId = player; Level = level; Context = context; ChallengeId = challenge; AttemptId = attempt; EntryBaselineId = baseline;
            Participants = new List<CandidateProgressionParticipant>(participants).AsReadOnly(); FormationRevision = formationRevision;
        }
    }
    public sealed class CandidateProgressionEndReceipt
    {
        public CandidateProgressionBeginReceipt Begin { get; }
        public string EndReceiptId { get; }
        public CandidateProgressionEndKind Kind { get; }
        public string SettlementId { get; }
        public string FinalReportFingerprint { get; }
        public string NewAttemptId { get; }
        public bool IsFirstClear { get; }
        internal CandidateProgressionEndReceipt(CandidateProgressionBeginReceipt begin, CandidateProgressionEndFacts facts, bool first)
        {
            Begin = begin; EndReceiptId = facts.EndReceiptId; Kind = facts.Kind; SettlementId = facts.SettlementId;
            FinalReportFingerprint = facts.FinalReportFingerprint; NewAttemptId = facts.NewAttemptId; IsFirstClear = first;
        }
    }
    public sealed class CandidateProgressionAttempt
    {
        public CandidateProgressionBeginReceipt Begin { get; }
        public CandidateProgressionEndReceipt End { get; }
        public bool IsEnded => End != null;
        internal CandidateProgressionAttempt(CandidateProgressionBeginReceipt begin, CandidateProgressionEndReceipt end)
        { Begin = begin; End = end; }
    }
    public sealed class CandidateProgressionChallenge
    {
        private readonly DefinitionBinding teachingBinding;
        public DefinitionBinding GetTeachingBinding() { return teachingBinding; }
        public string ChallengeId { get; }
        public CandidateProgressionLevel Level { get; }
        public IReadOnlyList<CandidateProgressionAttempt> Attempts { get; }
        public CandidateProgressionEndReceipt ClosedBy { get; }
        public bool IsClosed => ClosedBy != null;
        internal CandidateProgressionChallenge(string id, CandidateProgressionLevel level,
            IEnumerable<CandidateProgressionAttempt> attempts, CandidateProgressionEndReceipt closedBy, DefinitionBinding binding = null)
        { ChallengeId = id; Level = level; Attempts = new List<CandidateProgressionAttempt>(attempts).AsReadOnly(); ClosedBy = closedBy; teachingBinding = binding; }
    }
    public sealed class CandidateProgressionFirstClear
    {
        public CandidateProgressionEndReceipt End { get; }
        public string PlayerId => End.Begin.PlayerId;
        public string LevelId => End.Begin.Level.LevelId;
        public string LevelVersion => End.Begin.Level.LevelVersion;
        public PreparedRuleContext Context => End.Begin.Context;
        internal CandidateProgressionFirstClear(CandidateProgressionEndReceipt end) { End = end; }
    }
    public sealed class CandidateProgressionOpenFact
    {
        public string PlayerId { get; }
        public CandidateProgressionLevel Level { get; }
        public PreparedRuleContext Context { get; }
        public CandidateProgressionFirstClear SourceClear { get; }
        internal CandidateProgressionOpenFact(string player, CandidateProgressionLevel level,
            PreparedRuleContext context, CandidateProgressionFirstClear source)
        { PlayerId = player; Level = level; Context = context; SourceClear = source; }
    }
    // Isolated candidate relations. There is no persistence, combat state, or content availability claim here.
    public sealed class CandidateProgressionState
    {
        private readonly IReadOnlyList<CandidatePermanentEffect> permanentEffects;
        public IReadOnlyList<CandidatePermanentEffect> GetPermanentEffects() { return permanentEffects; }
        public string PlayerId { get; }
        public CandidateProgressionDefinition Definition { get; }
        public BigInteger StateRevision { get; }
        public IReadOnlyList<CandidateProgressionOpenFact> OpenFacts { get; }
        public IReadOnlyList<CandidateProgressionFirstClear> FirstClears { get; }
        public IReadOnlyList<CandidateProgressionChallenge> Challenges { get; }
        public CandidateProgressionAttempt ActiveAttempt
        {
            get { foreach (var challenge in Challenges) foreach (var attempt in challenge.Attempts) if (!attempt.IsEnded) return attempt; return null; }
        }
        internal CandidateProgressionState(string player, CandidateProgressionDefinition definition, BigInteger revision,
            IEnumerable<CandidateProgressionOpenFact> open, IEnumerable<CandidateProgressionFirstClear> clears,
            IEnumerable<CandidateProgressionChallenge> challenges, IEnumerable<CandidatePermanentEffect> effects = null)
        {
            permanentEffects = new List<CandidatePermanentEffect>(effects ?? new CandidatePermanentEffect[0]).AsReadOnly();
            PlayerId = player; Definition = definition; StateRevision = revision;
            OpenFacts = new List<CandidateProgressionOpenFact>(open).AsReadOnly(); FirstClears = new List<CandidateProgressionFirstClear>(clears).AsReadOnly();
            Challenges = new List<CandidateProgressionChallenge>(challenges).AsReadOnly();
        }
    }
    public sealed class CandidateProgressionLevelView
    {
        public CandidateProgressionLevel Level { get; }
        public CandidateProgressionOpenFact OpenFact { get; }
        public CandidateProgressionFirstClear FirstClear { get; }
        public CandidateProgressionChallenge OpenChallenge { get; }
        public IReadOnlyList<CandidateProgressionChallenge> Challenges { get; }
        internal CandidateProgressionLevelView(CandidateProgressionLevel level, CandidateProgressionOpenFact open,
            CandidateProgressionFirstClear clear, CandidateProgressionChallenge current, IEnumerable<CandidateProgressionChallenge> challenges)
        { Level = level; OpenFact = open; FirstClear = clear; OpenChallenge = current; Challenges = new List<CandidateProgressionChallenge>(challenges).AsReadOnly(); }
    }
    public sealed class CandidateProgressionView
    {
        public CandidateProgressionState State { get; }
        public IReadOnlyList<CandidateProgressionLevelView> Levels { get; }
        internal CandidateProgressionView(CandidateProgressionState state, IEnumerable<CandidateProgressionLevelView> levels)
        { State = state; Levels = new List<CandidateProgressionLevelView>(levels).AsReadOnly(); }
    }
    // Success covers only M05 relations and the supplied M03 basis; M02 must still check M06/H06 and M10/M14.
    public sealed class CandidateProgressionEntryCheck
    {
        public bool IsAccepted => Participants != null && Participants.Count > 0;
        public CandidateProgressionLevel Level { get; }
        public CandidateProgressionOpenFact OpenFact { get; }
        public CandidateProgressionChallenge OpenChallenge { get; }
        public CandidateProgressionParticipant Participant => CandidateRosterState.Single(Participants);
        internal IReadOnlyList<CandidateProgressionParticipant> Participants { get; }
        public IReadOnlyList<CandidateProgressionParticipant> GetParticipants() => Participants;
        public BigInteger StateRevision { get; }
        public CandidateProgressionRejectionCode RejectionCode { get; }
        public string FieldPath { get; }
        internal CandidateProgressionEntryCheck(ProgressionChecks check) { RejectionCode = check.Code; FieldPath = check.Path; }
        internal CandidateProgressionEntryCheck(CandidateProgressionState state, CandidateProgressionLevel level,
            CandidateProgressionOpenFact open, CandidateProgressionChallenge challenge, CandidateProgressionParticipant participant)
            : this(state, level, open, challenge, new[] { participant }) { }
        internal CandidateProgressionEntryCheck(CandidateProgressionState state, CandidateProgressionLevel level,
            CandidateProgressionOpenFact open, CandidateProgressionChallenge challenge, IReadOnlyList<CandidateProgressionParticipant> participants)
        { StateRevision = state.StateRevision; Level = level; OpenFact = open; OpenChallenge = challenge; Participants = new List<CandidateProgressionParticipant>(participants).AsReadOnly(); }
    }
    public sealed class CandidateProgressionResult
    {
        public bool IsAccepted => Next != null;
        public CandidateProgressionState Next { get; }
        public CandidateProgressionOutcome Outcome { get; }
        public CandidateProgressionRejectionCode RejectionCode { get; }
        public string FieldPath { get; }
        public CandidateProgressionBeginReceipt BeginReceipt { get; }
        public CandidateProgressionEndReceipt EndReceipt { get; }
        public CandidateProgressionFirstClear NewFirstClear { get; }
        public IReadOnlyList<CandidateProgressionOpenFact> NewOpenFacts { get; }
        public IReadOnlyList<CandidateProgressionOpenFact> Successors { get; }
        internal CandidateProgressionResult(ProgressionChecks check) { RejectionCode = check.Code; FieldPath = check.Path; }
        internal CandidateProgressionResult(CandidateProgressionState next, CandidateProgressionOutcome outcome,
            CandidateProgressionBeginReceipt begin = null, CandidateProgressionEndReceipt end = null,
            CandidateProgressionFirstClear clear = null, IEnumerable<CandidateProgressionOpenFact> added = null,
            IEnumerable<CandidateProgressionOpenFact> successors = null)
        {
            Next = next; Outcome = outcome; BeginReceipt = begin; EndReceipt = end; NewFirstClear = clear;
            NewOpenFacts = new List<CandidateProgressionOpenFact>(added ?? new CandidateProgressionOpenFact[0]).AsReadOnly();
            Successors = new List<CandidateProgressionOpenFact>(successors ?? new CandidateProgressionOpenFact[0]).AsReadOnly();
        }
    }
}
