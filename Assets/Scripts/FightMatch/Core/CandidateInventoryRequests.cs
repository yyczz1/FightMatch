using System.Collections.Generic;
using System.Numerics;

namespace FightMatch.Core
{
    // Explicit mutable candidate inputs; do not mutate them during a call.
    public sealed class CandidateInventoryActorInput
    {
        public string CharacterId { get; set; }
        public string ClassId { get; set; }
        public CharacterClassKind ClassKind { get; set; }
        public int? OriginalSlot { get; set; }
    }
    public sealed class CandidateInventoryQuantityInput
    {
        public string ItemId { get; set; }
        public BigInteger? Quantity { get; set; }
    }
    public enum CandidateOrdinaryGrantSource { Unspecified, OrdinaryBaseReward, Advertisement, Import, Mixed }
    public sealed class CandidateOrdinaryGrant
    {
        public string PlayerId { get; set; }
        public string AttemptId { get; set; }
        public string SettlementId { get; set; }
        public RuleContext Context { get; set; }
        public CandidateOrdinaryGrantSource Source { get; set; }
        public List<CandidateInventoryQuantityInput> Items { get; set; }
    }
    public sealed class CandidateEquipIntent
    {
        private string itemId;
        public string CharacterId { get; set; }
        public string ItemId { get => itemId; set { itemId = value; HasItem = true; } }
        public BigInteger? L { get; set; }
        internal bool HasItem { get; private set; }
    }
    public sealed class CandidateInventoryPreferenceIntent
    {
        public string CharacterId { get; set; }
        public string ItemId { get; set; }
        public bool? Enabled { get; set; }
    }
    public sealed class CandidateInventoryFreezeIntent
    {
        public string PlayerId { get; set; }
        public string AttemptId { get; set; }
        public string EntryBaselineId { get; set; }
        public RuleContext Context { get; set; }
        public List<CandidateInventoryActorInput> ReadyParticipants { get; set; }
    }
    public enum CandidateInventoryEndKind { Unspecified, NormalVictory, NormalExit, ImmediateRestart }
    public sealed class CandidateInventoryRemainingInput
    {
        public string CharacterId { get; set; }
        public string ItemId { get; set; }
        public BigInteger? U { get; set; }
    }
    public sealed class CandidateInventoryEndIntent
    {
        private string settlementId;
        private string newAttemptId;
        public string PlayerId { get; set; }
        public string AttemptId { get; set; }
        public string EntryBaselineId { get; set; }
        public string EndReceiptId { get; set; }
        public RuleContext Context { get; set; }
        public CandidateInventoryEndKind Kind { get; set; }
        public string SettlementId { get => settlementId; set { settlementId = value; HasSettlement = true; } }
        public string NewAttemptId { get => newAttemptId; set { newAttemptId = value; HasNewAttempt = true; } }
        public List<CandidateInventoryRemainingInput> Remaining { get; set; }
        public List<CandidateInventoryQuantityInput> Rewards { get; set; }
        internal bool HasSettlement { get; private set; }
        internal bool HasNewAttempt { get; private set; }
    }
}
