using System.Collections.Generic;
using System.Numerics;

namespace FightMatch.Core
{
    public enum CandidateBusinessFormat { CandidateV1 = 1, PublishedV2 = 2, PublishedRosterV3 = 3, PublishedPermanentV4 = 4 }
    // All five owners are required, including an explicitly absent active history.
    public sealed class CandidateBusinessInput
    {
        public string PlayerId { get; }
        public CandidateCharacterState Character => CandidateRosterState.Single(Roster?.Characters);
        public CandidateRosterState Roster { get; }
        public CandidateBusinessFormat Format { get; }
        public CandidateInventoryState Inventory { get; }
        public CandidateProgressionState Progression { get; }
        public CandidateRewardState Rewards { get; }
        public CandidateBattleHistory ActiveHistory { get; }
        public IReadOnlyList<CandidateBattleRun> RetainedRuns { get; }
        public IReadOnlyList<CandidateRollbackRecord> RetainedRollbacks { get; }
        public CandidateBusinessInput(string playerId, CandidateCharacterState character,
            CandidateInventoryState inventory, CandidateProgressionState progression, CandidateRewardState rewards,
            CandidateBattleHistory activeHistory, IReadOnlyList<CandidateBattleRun> retainedRuns,
            IReadOnlyList<CandidateRollbackRecord> retainedRollbacks)
        {
            PlayerId = playerId; Roster = CandidateRosterState.Legacy(character); Inventory = inventory; Progression = progression;
            Format = character?.Definition.Context is PreparedPublishedRuleContext ? CandidateBusinessFormat.PublishedV2 : CandidateBusinessFormat.CandidateV1;
            Rewards = rewards; ActiveHistory = activeHistory; RetainedRuns = retainedRuns; RetainedRollbacks = retainedRollbacks;
        }
        public CandidateBusinessInput(string playerId, CandidateRosterState roster,
            CandidateInventoryState inventory, CandidateProgressionState progression, CandidateRewardState rewards,
            CandidateBattleHistory activeHistory, IReadOnlyList<CandidateBattleRun> retainedRuns,
            IReadOnlyList<CandidateRollbackRecord> retainedRollbacks, CandidateBusinessFormat format)
        {
            PlayerId = playerId; Roster = roster; Format = format; Inventory = inventory; Progression = progression;
            Rewards = rewards; ActiveHistory = activeHistory; RetainedRuns = retainedRuns; RetainedRollbacks = retainedRollbacks;
        }
    }

    public sealed class CandidateBusinessSnapshot
    {
        public string PlayerId { get; }
        public CandidateCharacterState Character => CandidateRosterState.Single(Roster?.Characters);
        public CandidateRosterState Roster { get; }
        public CandidateBusinessFormat Format { get; }
        public CandidateInventoryState Inventory { get; }
        public CandidateProgressionState Progression { get; }
        public CandidateRewardState Rewards { get; }
        public CandidateBattleHistory ActiveHistory { get; }
        public IReadOnlyList<CandidateBattleRun> RetainedRuns { get; }
        public IReadOnlyList<CandidateRollbackRecord> RetainedRollbacks { get; }
        public bool CommitEligible => false;
        internal CandidateBusinessSnapshot(CandidateBusinessInput input)
        {
            PlayerId = input.PlayerId; Roster = input.Roster; Format = input.Format; Inventory = input.Inventory;
            Progression = input.Progression; Rewards = input.Rewards; ActiveHistory = input.ActiveHistory;
            RetainedRuns = new List<CandidateBattleRun>(input.RetainedRuns).AsReadOnly();
            RetainedRollbacks = new List<CandidateRollbackRecord>(input.RetainedRollbacks).AsReadOnly();
        }
    }

    public sealed class CandidateBusinessSaveHeader
    {
        public BigInteger SaveGeneration { get; }
        public string CommitId { get; }
        public string ParentCommitId { get; }
        public IReadOnlyList<SaveCommitIndexEntry> CommitIndex { get; }
        public CandidateBusinessSaveHeader(BigInteger generation, string commitId, string parentCommitId,
            IReadOnlyList<SaveCommitIndexEntry> commitIndex)
        { SaveGeneration = generation; CommitId = commitId; ParentCommitId = parentCommitId; CommitIndex = commitIndex; }
    }
}
