using System.Numerics;
using System.Linq;
using FightMatch.Core;

namespace FightMatch.Application
{
    public sealed class CandidateBattleAvailability
    {
        public bool IsAvailable => Reason == null;
        public string Reason { get; }
        internal CandidateBattleAvailability(string reason) { Reason = reason; }
    }

    public sealed class CandidateDemoView
    {
        public string Code { get; }
        public CandidateApplicationDiagnostic Diagnostic { get; }
        public CandidateApplicationView ApplicationView { get; }
        public string CommitId => PublishedSnapshot?.Header.CommitId;
        public bool IsPublishedHeadVerified => ApplicationView.IsPublishedHeadVerified;
        public CandidateApplicationPhase Phase => ApplicationView.Phase;
        public CandidateApplicationSnapshot PublishedSnapshot => ApplicationView.PublishedSnapshot;
        public CandidateBattleHistory History => PublishedSnapshot?.Business.ActiveHistory;
        public BattleSnapshot BattleSnapshot => History?.CurrentRun.CurrentSnapshot;
        public CandidateFinalAttemptReport FinalReport => History?.CurrentRun.FinalReport;
        public CandidateInventoryState Inventory => PublishedSnapshot?.Business.Inventory;
        public CandidateApplicationContinuation Continuation => PublishedSnapshot?.Continuation;
        public PreparedRuleContext Context => BattleSnapshot?.Baseline.Entry.Context ?? Inventory?.Definition.Context;
        public ContentBinding ContentBinding => (Context as PreparedPublishedRuleContext)?.Binding;
        public DefinitionBinding DefinitionBinding => BattleSnapshot?.Baseline.Entry.GetDefinitionBinding();
        public SavePurpose Purpose => ApplicationView.Purpose;
        public bool CommitEligible => false;
        public BigInteger? PreferenceRevision => Inventory?.PreferenceRevision;
        public bool? ItemUseEnabled => Inventory?.Loadout.Enabled;
        public bool PreferenceApplicable => ItemUseEnabled.HasValue;
        public bool? RequiredAttackItemUseEnabled => ItemUseEnabled ?? (IsEmptyPreference(Inventory, BattleSnapshot) ? (bool?)false : null);
        public bool? ItemUseEnabledFor(string characterId) => Inventory?.FindLoadout(characterId)?.Enabled;
        public bool PreferenceApplicableFor(string characterId) => ItemUseEnabledFor(characterId).HasValue;
        public bool? RequiredAttackItemUseEnabledFor(string characterId) => ItemUseEnabledFor(characterId) ??
            (IsEmptyPreference(Inventory, BattleSnapshot, characterId) ? (bool?)false : null);
        public CandidateBattleAvailability AttackFor(string characterId)
        {
            var member = BattleSnapshot?.Members.FirstOrDefault(x => x.Member.CharacterId == characterId);
            return new CandidateBattleAvailability(Attack.Reason ?? (member == null ? "InconsistentBinding" :
                member.Hp.Numerator.IsZero ? "ActorDown" : RequiredAttackItemUseEnabledFor(characterId).HasValue ? null : "UnsupportedBinding"));
        }
        public CandidatePresentationToken PresentationToken { get; }
        public CandidateBattleAvailability Attack { get; }
        public CandidateBattleAvailability Link { get; }
        public CandidateBattleAvailability Rollback { get; }
        public CandidateBattleAvailability PreferenceWriting { get; }
        public CandidateBattleAvailability RewardDestination { get; }
        public CandidateBattleAvailability OnlineAndAds { get; }

        internal CandidateDemoView(CandidateApplicationCallResult query, CandidatePresentationToken token, bool busy)
        {
            ApplicationView = query.View;
            Diagnostic = query.Diagnostic;
            PresentationToken = token;
            var guard = query.Code == "WrongThread" || query.Code == "Disposed" ? query.Code : null;
            Code = guard ?? (busy ? "Busy" : Phase == CandidateApplicationPhase.Ready && History == null
                ? "NoActiveBattle" : query.Code);
            var reason = guard ?? (busy ? "Busy" : !IsPublishedHeadVerified || Phase != CandidateApplicationPhase.Ready
                ? Phase.ToString() : History == null ? "NoActiveBattle" : token != null ? "PresentationPending" : null);
            Attack = new CandidateBattleAvailability(reason ?? (BattleSnapshot.Phase != BattlePhase.AwaitAction
                ? "InvalidPhase" : BattleSnapshot.Members.Any(x => RequiredAttackItemUseEnabledFor(x.Member.CharacterId).HasValue) ? null : "UnsupportedBinding"));
            Link = new CandidateBattleAvailability(reason ?? (BattleSnapshot.Phase == BattlePhase.AwaitLinks ? null : "InvalidPhase"));
            Rollback = new CandidateBattleAvailability(reason ?? (BattleSnapshot.Phase == BattlePhase.WonPendingSettlement ||
                BattleSnapshot.Phase == BattlePhase.Closed ? "InvalidPhase" : null));
            PreferenceWriting = new CandidateBattleAvailability("NotImplemented");
            RewardDestination = new CandidateBattleAvailability("NotImplemented");
            OnlineAndAds = new CandidateBattleAvailability("NotImplemented");
        }

        internal static bool IsEmptyPreference(CandidateInventoryState inventory, BattleSnapshot snapshot, string characterId = null)
        {
            if (inventory == null || snapshot == null) return false;
            CandidateInventoryLoadout loadout;
            if (characterId == null) { if (!inventory.TryGetSingle(out loadout)) return false; }
            else loadout = inventory.FindLoadout(characterId);
            return loadout != null && loadout.Enabled == null && loadout.ItemId == null && loadout.L.IsZero &&
                inventory.ActiveCarry?.Mode == EntryCarryMode.Empty && snapshot.CarryMode == EntryCarryMode.Empty &&
                snapshot.Baseline.Entry.ReadyParticipants.Any(x => x.CharacterId == loadout.Actor.CharacterId);
        }
    }
}
