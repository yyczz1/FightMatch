using System.Collections.Generic;
using System.Numerics;

namespace FightMatch.Core
{
    public sealed class CandidateInventoryActor
    {
        public string CharacterId { get; }
        public string ClassId { get; }
        public CharacterClassKind ClassKind { get; }
        public int OriginalSlot { get; }
        internal CandidateInventoryActor(CandidateInventoryActorInput input)
        { CharacterId = input.CharacterId; ClassId = input.ClassId; ClassKind = input.ClassKind; OriginalSlot = input.OriginalSlot.Value; }
    }
    public sealed class CandidateInventoryQuantity
    {
        public string ItemId { get; }
        public BigInteger Quantity { get; }
        internal CandidateInventoryQuantity(string item, BigInteger quantity) { ItemId = item; Quantity = quantity; }
    }
    public sealed class CandidateInventoryHolding
    {
        public string ItemId { get; }
        public BigInteger T { get; }
        internal CandidateInventoryHolding(string item, BigInteger total) { ItemId = item; T = total; }
    }
    public sealed class CandidateInventoryLoadout
    {
        public CandidateInventoryActor Actor { get; }
        public string ItemId { get; }
        public BigInteger L { get; }
        public bool? Enabled { get; }
        internal CandidateInventoryLoadout(CandidateInventoryActor actor, string item, BigInteger allocated, bool? enabled)
        { Actor = actor; ItemId = item; L = allocated; Enabled = enabled; }
    }
    public sealed class CandidateOrdinaryPool
    {
        public string PlayerId { get; }
        public string ItemId { get; }
        internal CandidateOrdinaryPool(string player, string item) { PlayerId = player; ItemId = item; }
    }
    public sealed class CandidateCarryRow
    {
        public CandidateInventoryActor Actor { get; }
        public string ItemId { get; }
        public BigInteger C { get; }
        public CandidateOrdinaryPool Source { get; }
        internal CandidateCarryRow(CandidateInventoryActor actor, string item, BigInteger count, CandidateOrdinaryPool source)
        { Actor = actor; ItemId = item; C = count; Source = source; }
    }
    public sealed class CandidateCarryPlan
    {
        public string PlayerId { get; }
        public string AttemptId { get; }
        public string EntryBaselineId { get; }
        public PreparedRuleContext Context { get; }
        public IReadOnlyList<CandidateInventoryActor> ReadyParticipants { get; }
        public IReadOnlyList<CandidateCarryRow> Rows { get; }
        public EntryCarryMode Mode => Rows.Count == 0 ? EntryCarryMode.Empty : EntryCarryMode.NonEmpty;
        internal CandidateCarryPlan(string player, string attempt, string baseline, PreparedRuleContext context,
            IEnumerable<CandidateInventoryActor> actors, IEnumerable<CandidateCarryRow> rows)
        {
            PlayerId = player; AttemptId = attempt; EntryBaselineId = baseline; Context = context;
            ReadyParticipants = new List<CandidateInventoryActor>(actors).AsReadOnly(); Rows = new List<CandidateCarryRow>(rows).AsReadOnly();
        }
    }
    public sealed class CandidateOrdinaryGrantReceipt
    {
        public string PlayerId { get; }
        public string AttemptId { get; }
        public string SettlementId { get; }
        public PreparedRuleContext Context { get; }
        public CandidateOrdinaryGrantSource Source => CandidateOrdinaryGrantSource.OrdinaryBaseReward;
        public IReadOnlyList<CandidateInventoryQuantity> Items { get; }
        internal CandidateOrdinaryGrantReceipt(string player, string attempt, string settlement,
            PreparedRuleContext context, IEnumerable<CandidateInventoryQuantity> items)
        { PlayerId = player; AttemptId = attempt; SettlementId = settlement; Context = context; Items = new List<CandidateInventoryQuantity>(items).AsReadOnly(); }
    }
    public sealed class CandidateInventoryRemaining
    {
        public string CharacterId { get; }
        public string ItemId { get; }
        public BigInteger U { get; }
        internal CandidateInventoryRemaining(string character, string item, BigInteger remaining)
        { CharacterId = character; ItemId = item; U = remaining; }
    }
    public sealed class CandidateInventoryEndReceipt
    {
        public CandidateCarryPlan OriginalCarry { get; }
        public string EndReceiptId { get; }
        public CandidateInventoryEndKind Kind { get; }
        public string SettlementId { get; }
        public string NewAttemptId { get; }
        public IReadOnlyList<CandidateInventoryRemaining> Remaining { get; }
        public IReadOnlyList<CandidateInventoryQuantity> Rewards { get; }
        public CandidateOrdinaryGrantReceipt RewardReceipt { get; }
        internal CandidateInventoryEndReceipt(CandidateCarryPlan carry, CandidateInventoryEndIntent input,
            IEnumerable<CandidateInventoryRemaining> remaining, IEnumerable<CandidateInventoryQuantity> rewards, CandidateOrdinaryGrantReceipt grant)
        {
            OriginalCarry = carry; EndReceiptId = input.EndReceiptId; Kind = input.Kind; SettlementId = input.SettlementId;
            NewAttemptId = input.NewAttemptId; Remaining = new List<CandidateInventoryRemaining>(remaining).AsReadOnly();
            Rewards = new List<CandidateInventoryQuantity>(rewards).AsReadOnly(); RewardReceipt = grant;
        }
    }
    // A new isolated inventory candidate is not a loader or a repair of a saved player.
    public sealed class CandidateInventoryState
    {
        private readonly CandidateInventoryActor legacyActor;
        private readonly CandidatePermanentInventoryLedger permanent;
        public CandidatePermanentInventoryLedger GetPermanentLedger() { return permanent; }
        public CandidateInventoryDefinition Definition { get; }
        public string PlayerId { get; }
        public CandidateInventoryActor Actor => IsRoster ? Loadout?.Actor : legacyActor;
        public bool IsRoster { get; }
        public IReadOnlyList<CandidateInventoryHolding> Holdings { get; }
        public CandidateInventoryLoadout Loadout => CandidateRosterState.Single(Loadouts);
        public IReadOnlyList<CandidateInventoryLoadout> Loadouts { get; }
        public CandidateInventoryLoadout FindLoadout(string characterId)
        { foreach (var loadout in Loadouts) if (loadout.Actor.CharacterId == characterId) return loadout; return null; }
        public bool TryGetSingle(out CandidateInventoryLoadout loadout)
        { loadout = Loadouts.Count == 1 ? Loadouts[0] : null; return loadout != null; }
        public CandidateCarryPlan ActiveCarry { get; }
        public IReadOnlyList<CandidateOrdinaryGrantReceipt> OrdinaryGrants { get; }
        public IReadOnlyList<CandidateInventoryEndReceipt> Ends { get; }
        public BigInteger StateRevision { get; }
        public BigInteger PreferenceRevision { get; }
        internal CandidateInventoryState(CandidateInventoryDefinition definition, string player, CandidateInventoryActor actor,
            IEnumerable<CandidateInventoryHolding> holdings, CandidateInventoryLoadout loadout, CandidateCarryPlan carry,
            IEnumerable<CandidateOrdinaryGrantReceipt> grants, IEnumerable<CandidateInventoryEndReceipt> ends,
            BigInteger revision, BigInteger preferenceRevision, CandidatePermanentInventoryLedger ledger = null)
        {
            permanent = ledger ?? CandidatePermanentInventoryLedger.Empty;
            Definition = definition; PlayerId = player; Holdings = new List<CandidateInventoryHolding>(holdings).AsReadOnly();
            legacyActor = actor;
            Loadouts = new List<CandidateInventoryLoadout> { loadout }.AsReadOnly();
            ActiveCarry = carry; OrdinaryGrants = new List<CandidateOrdinaryGrantReceipt>(grants).AsReadOnly();
            Ends = new List<CandidateInventoryEndReceipt>(ends).AsReadOnly(); StateRevision = revision; PreferenceRevision = preferenceRevision;
        }
        internal CandidateInventoryState(CandidateInventoryDefinition definition, string player,
            IEnumerable<CandidateInventoryHolding> holdings, IEnumerable<CandidateInventoryLoadout> loadouts, CandidateCarryPlan carry,
            IEnumerable<CandidateOrdinaryGrantReceipt> grants, IEnumerable<CandidateInventoryEndReceipt> ends,
            BigInteger revision, BigInteger preferenceRevision, bool isRoster, CandidatePermanentInventoryLedger ledger = null)
        {
            permanent = ledger ?? CandidatePermanentInventoryLedger.Empty;
            Definition = definition; PlayerId = player; Holdings = new List<CandidateInventoryHolding>(holdings).AsReadOnly();
            Loadouts = new List<CandidateInventoryLoadout>(loadouts).AsReadOnly(); IsRoster = isRoster;
            if (!isRoster) legacyActor = CandidateRosterState.Single(Loadouts)?.Actor;
            ActiveCarry = carry; OrdinaryGrants = new List<CandidateOrdinaryGrantReceipt>(grants).AsReadOnly();
            Ends = new List<CandidateInventoryEndReceipt>(ends).AsReadOnly(); StateRevision = revision; PreferenceRevision = preferenceRevision;
        }
    }
    public sealed class CandidateInventoryItemView
    {
        public string ItemId { get; }
        public BigInteger T { get; }
        public BigInteger L { get; }
        public BigInteger R { get; }
        public BigInteger F { get; }
        public BigInteger FullStacks { get; }
        public BigInteger Remainder { get; }
        public CandidateOrdinaryPool Source { get; }
        internal CandidateInventoryItemView(string player, string item, BigInteger total, BigInteger allocated,
            BigInteger reserved, BigInteger free, BigInteger full, BigInteger remainder)
        { ItemId = item; T = total; L = allocated; R = reserved; F = free; FullStacks = full; Remainder = remainder; Source = new CandidateOrdinaryPool(player, item); }
    }
    public sealed class CandidateInventoryView
    {
        public CandidateInventoryState State { get; }
        public IReadOnlyList<CandidateInventoryItemView> Items { get; }
        internal CandidateInventoryView(CandidateInventoryState state, IEnumerable<CandidateInventoryItemView> items)
        { State = state; Items = new List<CandidateInventoryItemView>(items).AsReadOnly(); }
    }
    public sealed class CandidateInventoryResult
    {
        public bool IsAccepted => Next != null;
        public CandidateInventoryState Next { get; }
        public CandidateInventoryOutcome Outcome { get; }
        public CandidateInventoryRejectionCode RejectionCode { get; }
        public string FieldPath { get; }
        public CandidateOrdinaryGrantReceipt GrantReceipt { get; }
        public CandidateInventoryEndReceipt EndReceipt { get; }
        public CandidateCarryPlan CarryPlan { get; }
        public IReadOnlyList<CandidateInventoryQuantity> GrantedItems { get; }
        internal CandidateInventoryResult(InventoryChecks check) { RejectionCode = check.Code; FieldPath = check.Path; }
        internal CandidateInventoryResult(CandidateInventoryState next, CandidateInventoryOutcome outcome,
            CandidateOrdinaryGrantReceipt grant = null, CandidateInventoryEndReceipt end = null,
            CandidateCarryPlan carry = null, IEnumerable<CandidateInventoryQuantity> added = null)
        {
            Next = next; Outcome = outcome; GrantReceipt = grant; EndReceipt = end; CarryPlan = carry;
            GrantedItems = new List<CandidateInventoryQuantity>(added ?? new CandidateInventoryQuantity[0]).AsReadOnly();
        }
    }
}
