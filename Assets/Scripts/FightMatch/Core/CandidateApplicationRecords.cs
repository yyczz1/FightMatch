using System.Collections.Generic;
using System.Numerics;

namespace FightMatch.Core
{
    public sealed class CandidateApplicationResultInput
    {
        public string ChallengeId { get; set; }
        public string AttemptId { get; set; }
        public string EntryBaselineId { get; set; }
        public string HistoryAnchorId { get; set; }
        public string EndReceiptId { get; set; }
        public string SettlementId { get; set; }
        public string NewAttemptId { get; set; }
        public CandidateCharacterResult RecoveryResult { get; set; }
        public IReadOnlyList<CandidateCharacterResult> RecoveryResults { get; set; }
    }

    public sealed class CandidateApplicationRecoveryReceipt
    {
        public string RecoveryId => Period.RecoveryId;
        public string CharacterId => Period.EndReceipt.CharacterId;
        public string EndReceiptId => Period.EndReceipt.EndReceiptId;
        public BigInteger AfterCharacterRevision { get; }
        public CandidateGrowthOutcome Outcome { get; }
        public CandidateRecoveryPeriod Period { get; }
        public CandidateTimeAnomaly ResultAnomaly { get; }

        internal CandidateApplicationRecoveryReceipt(BigInteger revision, CandidateGrowthOutcome outcome,
            CandidateRecoveryPeriod period, CandidateTimeAnomaly resultAnomaly)
        {
            AfterCharacterRevision = revision;
            Outcome = outcome;
            Period = period;
            ResultAnomaly = resultAnomaly;
        }
    }

    public sealed class CandidateRosterMigrationReceipt
    {
        public string SourceCommitId { get; }
        public BigInteger SourceGeneration { get; }
        public ulong SourceDescriptorLength { get; }
        public IReadOnlyList<byte> SourceDescriptorSha256 { get; }
        public uint FromVersion => 2;
        public uint ToVersion => 3;
        internal CandidateRosterMigrationReceipt(PreparedCandidateApplicationIntent intent)
        {
            var value = intent.Data.MigrateRoster;
            SourceCommitId = intent.ExpectedCommitId; SourceGeneration = value.SourceGeneration.Value;
            SourceDescriptorLength = value.SourceDescriptorLength.Value;
            SourceDescriptorSha256 = new List<byte>(value.SourceDescriptorSha256).AsReadOnly();
        }
    }

    public sealed class CandidateApplicationResult
    {
        private readonly CandidatePermanentResult permanent;
        public CandidatePermanentResult GetPermanent() { return permanent; }
        public string ChallengeId { get; }
        public string AttemptId { get; }
        public string EntryBaselineId { get; }
        public string HistoryAnchorId { get; }
        public string EndReceiptId { get; }
        public string SettlementId { get; }
        public string NewAttemptId { get; }
        public IReadOnlyList<CandidateApplicationRecoveryReceipt> RecoveryResults { get; }
        public CandidateApplicationRecoveryReceipt Recovery => CandidateRosterState.Single(RecoveryResults);
        public CandidateFormationReceipt Formation { get; }
        public CandidateRosterMigrationReceipt Migration { get; }

        internal CandidateApplicationResult(CandidateApplicationResultInput input,
            CandidateApplicationRecoveryReceipt recovery = null, CandidateFormationReceipt formation = null,
            CandidateRosterMigrationReceipt migration = null, IReadOnlyList<CandidateApplicationRecoveryReceipt> recoveries = null,
            CandidatePermanentResult permanentResult = null)
        {
            ChallengeId = input.ChallengeId;
            AttemptId = input.AttemptId;
            EntryBaselineId = input.EntryBaselineId;
            HistoryAnchorId = input.HistoryAnchorId;
            EndReceiptId = input.EndReceiptId;
            SettlementId = input.SettlementId;
            NewAttemptId = input.NewAttemptId;
            RecoveryResults = recoveries == null && recovery == null ? null :
                new List<CandidateApplicationRecoveryReceipt>(recoveries ?? new[] { recovery }).AsReadOnly();
            Formation = formation; Migration = migration;
            permanent = permanentResult;
        }
    }

    public sealed class CandidateApplicationRecord
    {
        public PreparedCandidateApplicationIntent Intent { get; }
        public BigInteger? Generation { get; }
        public string CommitId { get; }
        public CandidateApplicationResult Result { get; }
        public string OperationId => Intent.OperationId;

        internal CandidateApplicationRecord(PreparedCandidateApplicationIntent intent,
            CandidateApplicationResult result, BigInteger? generation = null, string commitId = null)
        {
            Intent = intent;
            Result = result;
            Generation = generation;
            CommitId = commitId;
        }
    }

    public enum CandidateApplicationContinuationStage { AwaitBaseSettlement }

    public sealed class CandidateApplicationContinuation
    {
        public CandidateApplicationContinuationStage Stage => CandidateApplicationContinuationStage.AwaitBaseSettlement;
        public string ClosingOperationId { get; }
        public string AttemptId { get; }
        public string FinalReportFingerprint { get; }
        public string ReservedOperationId { get; }

        internal CandidateApplicationContinuation(string closing, string attempt, string fingerprint, string reserved)
        {
            ClosingOperationId = closing;
            AttemptId = attempt;
            FinalReportFingerprint = fingerprint;
            ReservedOperationId = reserved;
        }
    }

    public sealed class CandidateApplicationCandidate
    {
        public CandidateBusinessSnapshot Business { get; }
        public IReadOnlyList<CandidateApplicationRecord> Records { get; }
        public CandidateApplicationContinuation Continuation { get; }
        public bool CommitEligible => false;
        internal CandidateApplicationSnapshot Basis { get; }

        internal CandidateApplicationCandidate(CandidateApplicationSnapshot basis, CandidateBusinessSnapshot business,
            List<CandidateApplicationRecord> records, CandidateApplicationContinuation continuation)
        {
            Basis = basis;
            Business = business;
            Records = records.AsReadOnly();
            Continuation = continuation;
        }
    }

    public sealed class CandidateApplicationSnapshot
    {
        public CandidateBusinessSnapshot Business { get; }
        public IReadOnlyList<CandidateApplicationRecord> Records { get; }
        public CandidateApplicationContinuation Continuation { get; }
        public CandidateBusinessSaveHeader Header { get; }
        public SnapshotDescriptor Descriptor { get; }
        public bool CommitEligible => false;

        internal CandidateApplicationSnapshot(CandidateBusinessSnapshot business, List<CandidateApplicationRecord> records,
            CandidateApplicationContinuation continuation, CandidateBusinessSaveHeader header, SnapshotDescriptor descriptor)
        {
            Business = business;
            Records = records.AsReadOnly();
            Continuation = continuation;
            Header = header;
            Descriptor = descriptor;
        }
    }

    public sealed class CandidateApplicationInitializationReceipt
    {
        public bool Created => true;
        public string CharacterId { get; }
        public string ClassId { get; }
        public BigInteger InitialLevel { get; }
        public BigInteger InitialExperience { get; }
        public int OriginalSlot { get; }

        internal CandidateApplicationInitializationReceipt(CandidateApplicationInitializeInput input)
        {
            CharacterId = input.CharacterId;
            ClassId = input.ClassId;
            InitialLevel = input.InitialLevel.Value;
            InitialExperience = input.InitialExperience.Value;
            OriginalSlot = input.OriginalSlot.Value;
        }
    }

    public enum CandidateApplicationRelation { Recorded, Effective, Superseded, RollbackRecorded }

    public sealed class CandidateApplicationLookup
    {
        public CandidatePermanentResult GetPermanent() { return Record?.Result.GetPermanent(); }
        public bool IsFound => Record != null;
        public CandidateApplicationRecord Record { get; }
        public string OriginalCommitId => Record?.CommitId;
        public BigInteger? OriginalGeneration => Record?.Generation;
        public CandidateApplicationRelation Relation { get; }
        public CandidateHistorySupersededBy SupersededBy { get; }
        public IReadOnlyList<CandidateApplicationInitializationReceipt> Initializations { get; } = new CandidateApplicationInitializationReceipt[0];
        public CandidateApplicationInitializationReceipt Initialization => CandidateRosterState.Single(Initializations);
        public CandidateBattleOperationRecord BattleOperation { get; }
        public CandidateRollbackRecord Rollback { get; }
        public CandidateProgressionBeginReceipt Begin { get; }
        public CandidateCarryPlan Carry { get; }
        public CandidateRandomBinding Binding { get; }
        public BattleEntryBaseline Baseline => Binding?.Start.Baseline;
        public BattleSnapshot InitialSnapshot => Binding?.Start.Snapshot;
        public CandidateBattleRun TerminalRun { get; }
        public CandidateProgressionEndReceipt End { get; }
        public IReadOnlyList<CandidateCharacterEndReceipt> CharacterEnds { get; } = new CandidateCharacterEndReceipt[0];
        public IReadOnlyList<CandidateBaseExperienceReceipt> CharacterExperiences { get; } = new CandidateBaseExperienceReceipt[0];
        public CandidateCharacterEndReceipt CharacterEnd => CandidateRosterState.Single(CharacterEnds);
        public CandidateBaseExperienceReceipt CharacterExperience => CandidateRosterState.Single(CharacterExperiences);
        public bool TryGetSingleCharacterEnd(out CandidateCharacterEndReceipt value)
        { value = CharacterEnds.Count == 1 ? CharacterEnds[0] : null; return value != null; }
        public bool TryGetSingleCharacterExperience(out CandidateBaseExperienceReceipt value)
        { value = CharacterExperiences.Count == 1 ? CharacterExperiences[0] : null; return value != null; }
        public CandidateFormationReceipt Formation => Record?.Result.Formation;
        public CandidateRosterMigrationReceipt Migration => Record?.Result.Migration;
        public IReadOnlyList<CandidateApplicationRecoveryReceipt> RecoveryResults => Record?.Result.RecoveryResults;
        public CandidateInventoryEndReceipt InventoryEnd { get; }
        public CandidateOrdinaryGrantReceipt InventoryGrant => InventoryEnd?.RewardReceipt;
        public CandidateFixedBaseReward Reward { get; }
        public CandidateProgressionBeginReceipt NewBegin { get; }
        public CandidateCarryPlan NewCarry { get; }
        public CandidateRandomBinding NewBinding { get; }
        public CandidateApplicationRecoveryReceipt Recovery => Record?.Result.Recovery;

        internal CandidateApplicationLookup(CandidateApplicationRecord record = null,
            CandidateApplicationResolution result = null)
        {
            Record = record;
            if (result == null) return;
            Relation = result.Relation;
            SupersededBy = result.SupersededBy;
            Initializations = result.Initializations.AsReadOnly();
            BattleOperation = result.BattleOperation;
            Rollback = result.Rollback;
            Begin = result.Begin;
            Carry = result.Carry;
            Binding = result.Binding;
            TerminalRun = result.TerminalRun;
            End = result.End;
            CharacterEnds = result.CharacterEnds.AsReadOnly();
            CharacterExperiences = result.CharacterExperiences.AsReadOnly();
            InventoryEnd = result.InventoryEnd;
            Reward = result.Reward;
            NewBegin = result.NewBegin;
            NewCarry = result.NewCarry;
            NewBinding = result.NewBinding;
        }
    }

    internal sealed class CandidateApplicationResolution
    {
        internal CandidateApplicationRelation Relation;
        internal CandidateHistorySupersededBy SupersededBy;
        internal readonly List<CandidateApplicationInitializationReceipt> Initializations = new List<CandidateApplicationInitializationReceipt>();
        internal CandidateBattleOperationRecord BattleOperation;
        internal CandidateRollbackRecord Rollback;
        internal CandidateProgressionBeginReceipt Begin, NewBegin;
        internal CandidateCarryPlan Carry, NewCarry;
        internal CandidateRandomBinding Binding, NewBinding;
        internal CandidateBattleRun TerminalRun;
        internal CandidateProgressionEndReceipt End;
        internal readonly List<CandidateCharacterEndReceipt> CharacterEnds = new List<CandidateCharacterEndReceipt>();
        internal readonly List<CandidateBaseExperienceReceipt> CharacterExperiences = new List<CandidateBaseExperienceReceipt>();
        internal CandidateInventoryEndReceipt InventoryEnd;
        internal CandidateFixedBaseReward Reward;
    }
}
