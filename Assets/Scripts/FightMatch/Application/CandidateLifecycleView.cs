using System.Collections.Generic;
using System.Linq;
using FightMatch.Core;

namespace FightMatch.Application
{
    public sealed class CandidateLifecycleView
    {
        public CandidateDemoView Battle { get; }
        public CandidateApplicationView Application => Battle.ApplicationView;
        public SavePurpose Purpose => Application.Purpose;
        public ContentBinding ContentBinding => (Progression?.Definition.Context as PreparedPublishedRuleContext)?.Binding;
        // These owners always come from the same published snapshot. Verification is explicit.
        public CandidateCharacterState Character => Application.PublishedSnapshot?.Business.Character;
        public CandidateRosterState Roster => Application.PublishedSnapshot?.Business.Roster;
        public IReadOnlyList<CandidateCharacterState> Characters => Roster?.Characters;
        public IReadOnlyList<string> Formation => Roster?.Formation;
        public CandidateBusinessFormat? Format => Application.PublishedSnapshot?.Business.Format;
        public bool TryGetSingle(out CandidateCharacterState character)
        { character = null; return Roster != null && Roster.TryGetSingle(out character); }
        public CandidateProgressionState Progression => Application.PublishedSnapshot?.Business.Progression;
        public CandidateRewardState Rewards => Application.PublishedSnapshot?.Business.Rewards;
        public IReadOnlyList<CandidateCharacterEndReceipt> Ends => Character?.ProcessedEnds;
        public CandidateRecoveryPeriod Recovery => Character?.ActiveRecovery;
        public bool HasEndedAttemptWithoutActiveHistory => Battle.History == null && Characters != null && Characters.Any(x => x.ProcessedEnds.Count > 0);
        public CandidateBattleAvailability Enter { get; }
        public CandidateBattleAvailability Exit { get; }
        public CandidateBattleAvailability Restart { get; }
        public CandidateBattleAvailability SettleVictory { get; }
        public CandidateBattleAvailability AdvanceRecovery { get; }
        internal CandidateLifecycleView(CandidateDemoView battle, bool busy)
        {
            Battle = battle;
            var guard = battle.Code == "WrongThread" || battle.Code == "Disposed" ? battle.Code :
                busy || battle.Code == "Busy" ? "Busy" : !battle.IsPublishedHeadVerified || battle.Phase != CandidateApplicationPhase.Ready
                ? "ResolutionRequired" : null;
            var active = battle.History != null;
            var victory = battle.BattleSnapshot?.Phase == BattlePhase.WonPendingSettlement;
            Enter = new CandidateBattleAvailability(guard ?? (active ? "ActiveAttemptConflict" : Roster.Formation.Any(id => id != null && Roster.Find(id).IsReady) ? null : "NoReadyMember"));
            var endReason = guard ?? (!active ? "NoActiveBattle" : victory || battle.BattleSnapshot.Phase == BattlePhase.Closed
                ? "InvalidPhase" : battle.PresentationToken != null ? "PresentationPending" : null);
            Exit = new CandidateBattleAvailability(endReason); Restart = new CandidateBattleAvailability(endReason);
            SettleVictory = new CandidateBattleAvailability(guard ?? (victory && battle.Continuation != null ? null : "InvalidPhase"));
            AdvanceRecovery = new CandidateBattleAvailability(guard ?? (Characters.Any(x => x.ActiveRecovery != null) ? null : "NoActiveRecovery"));
        }
    }
}
