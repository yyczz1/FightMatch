using System.Collections.Generic;
using System.Numerics;

namespace FightMatch.Core
{
    public sealed class CandidateBattleHistory
    {
        public CandidateRandomBinding Binding => CurrentRun.Binding;
        public CandidateBattleRun CurrentRun { get; }
        public IReadOnlyList<CandidateHistoryEntry> Archive { get; }
        public IReadOnlyList<string> EffectiveAnchors { get; }
        public IReadOnlyList<CandidateRollbackRecord> RollbackRecords { get; }
        public bool CommitEligible => false;

        internal CandidateBattleHistory(CandidateBattleRun run, IEnumerable<CandidateHistoryEntry> archive,
            IEnumerable<string> anchors, IEnumerable<CandidateRollbackRecord> rollbacks)
        {
            CurrentRun = run; Archive = new List<CandidateHistoryEntry>(archive).AsReadOnly();
            EffectiveAnchors = new List<string>(anchors).AsReadOnly();
            RollbackRecords = new List<CandidateRollbackRecord>(rollbacks).AsReadOnly();
        }
    }

    public sealed class CandidateHistorySupersededBy
    {
        public string OperationId { get; }
        public BigInteger SceneRevision { get; }
        internal CandidateHistorySupersededBy(string operation, BigInteger revision)
        { OperationId = operation; SceneRevision = revision; }
    }

    public sealed class CandidateHistoryEntry
    {
        public string HistoryAnchorId { get; }
        public CandidateBattleOperationRecord Record { get; }
        public string OperationId => Record.OperationId;
        public string FaceId => Record.BeforeSnapshot.Board.Face.FaceId;
        public BattlePairKey Pair => Record.Request.Pair;
        public CandidateHistorySupersededBy SupersededBy { get; }
        public bool CommitEligible => false;
        internal CandidateHistoryEntry(string anchor, CandidateBattleOperationRecord record,
            CandidateHistorySupersededBy superseded = null)
        { HistoryAnchorId = anchor; Record = record; SupersededBy = superseded; }
    }

    public sealed class CandidateRollbackRange
    {
        public string PlayerId { get; }
        public string AttemptId { get; }
        public CandidateRandomBinding Binding { get; }
        public BigInteger SceneRevision { get; }
        public string HistoryAnchorId { get; }
        public string OperationId { get; }
        public BattleSnapshot BeforeSnapshot { get; }
        public IReadOnlyList<CandidateHistoryEntry> Entries { get; }
        public bool CommitEligible => false;
        internal CandidateRollbackRange(CandidateRandomBinding binding, BigInteger revision,
            string anchor, string operation, BattleSnapshot before, IEnumerable<CandidateHistoryEntry> entries)
        {
            Binding = binding; PlayerId = binding.Start.Baseline.Entry.PlayerId;
            AttemptId = binding.Start.Baseline.Entry.AttemptId; SceneRevision = revision;
            HistoryAnchorId = anchor; OperationId = operation; BeforeSnapshot = before;
            Entries = new List<CandidateHistoryEntry>(entries).AsReadOnly();
        }
    }

    public sealed class CandidateRollbackRecord
    {
        public string OperationId { get; }
        public CandidateBattleRun BeforeRun { get; }
        public CandidateRollbackRange Range { get; }
        public CandidateBattleRun RestoredRun { get; }
        public BigInteger SceneRevision => RestoredRun.CurrentSnapshot.SceneRevision;
        public bool CommitEligible => false;
        internal CandidateRollbackRecord(string operation, CandidateBattleRun before,
            CandidateRollbackRange range, CandidateBattleRun restored)
        { OperationId = operation; BeforeRun = before; Range = range; RestoredRun = restored; }
    }

    public enum CandidateHistoryOperationRelation { Effective, Superseded, RollbackRecorded }

    public sealed class CandidateHistoryOperation
    {
        public CandidateHistoryOperationRelation Relation { get; }
        public CandidateHistoryEntry Entry { get; }
        public CandidateRollbackRecord RollbackRecord { get; }
        public bool CommitEligible => false;
        internal CandidateHistoryOperation(CandidateHistoryEntry entry)
        { Entry = entry; Relation = entry.SupersededBy == null ? CandidateHistoryOperationRelation.Effective : CandidateHistoryOperationRelation.Superseded; }
        internal CandidateHistoryOperation(CandidateRollbackRecord rollback)
        { RollbackRecord = rollback; Relation = CandidateHistoryOperationRelation.RollbackRecorded; }
    }
}
