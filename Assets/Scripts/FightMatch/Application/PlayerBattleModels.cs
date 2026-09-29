using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using FightMatch.Core;

namespace FightMatch.Application
{
    public enum PlayerBattleRoute { Gate, Battle, Confirmation, Recovery, Result, HostNavigation }
    public enum PlayerBattleRecoveryAction { Query, Retry, Resolve, End }

    public sealed class PlayerBattleContext
    {
        internal PlayerBattleSession Owner { get; }
        public long Revision { get; }
        public CandidateApplicationSnapshot Head { get; }
        public string PlayerId => Head?.Business.PlayerId;
        public string CommitId => Head?.Header.CommitId;
        public BattleSnapshot Snapshot => Head?.Business.ActiveHistory?.CurrentRun.CurrentSnapshot;
        public string AttemptId => Snapshot?.Baseline.Entry.AttemptId;
        public string ChallengeId => Snapshot?.Baseline.Entry.ChallengeId;
        public string EntryBaselineId => Snapshot?.Baseline.Entry.EntryBaselineId;
        public BigInteger? SceneRevision => Snapshot?.SceneRevision;
        public ContentBinding Binding => (Head?.Business.Progression.Definition.Context as PreparedPublishedRuleContext)?.Binding;
        internal PlayerBattleContext(PlayerBattleSession owner, long revision, CandidateApplicationSnapshot head)
        { Owner = owner; Revision = revision; Head = head; }
    }

    public sealed class PlayerBattleConfirmation
    {
        public CandidateApplicationKind Kind { get; }
        public PlayerBattleContext Context { get; }
        internal PlayerBattleConfirmation(CandidateApplicationKind kind, PlayerBattleContext context)
        { Kind = kind; Context = context; }
    }

    public sealed class PlayerBattleMemberReceipt
    {
        public string CharacterId { get; }
        public int OriginalSlot { get; }
        public CandidateRewardExperience Reward { get; }
        public CandidateBaseExperienceReceipt Experience { get; }
        public CandidateCharacterEndReceipt End { get; }
        internal PlayerBattleMemberReceipt(CandidateProgressionParticipant participant, CandidateApplicationLookup lookup)
        {
            CharacterId = participant.CharacterId; OriginalSlot = participant.OriginalSlot;
            Reward = lookup.Reward?.Experience.SingleOrDefault(x => x.CharacterId == CharacterId && x.OriginalSlot == OriginalSlot);
            Experience = lookup.CharacterExperiences.SingleOrDefault(x => x.CharacterId == CharacterId);
            End = lookup.CharacterEnds.SingleOrDefault(x => x.CharacterId == CharacterId);
        }
    }

    // The original lookup is the receipt. The latest HUD is deliberately read separately.
    public sealed class PlayerBattleReceipt
    {
        public string OperationId => Lookup.Record.OperationId;
        public string CommitId => Lookup.OriginalCommitId;
        public string AttemptId => Lookup.End.Begin.AttemptId;
        public string SettlementId => Lookup.End.SettlementId;
        public string LevelId => Lookup.End.Begin.Level.LevelId;
        public string LevelVersion => Lookup.End.Begin.Level.LevelVersion;
        public CandidateApplicationLookup Lookup { get; }
        public CandidateFixedBaseReward Reward => Lookup.Reward;
        public CandidateOrdinaryGrantReceipt InventoryGrant => Lookup.InventoryGrant;
        public CandidateProgressionEndReceipt End => Lookup.End;
        public IReadOnlyList<PlayerBattleMemberReceipt> Members { get; }
        internal PlayerBattleReceipt(CandidateApplicationLookup lookup)
        {
            Lookup = lookup;
            Members = lookup.End.Begin.GetParticipants().OrderBy(x => x.OriginalSlot)
                .Select(x => new PlayerBattleMemberReceipt(x, lookup)).ToList().AsReadOnly();
        }
    }

    public sealed class PlayerBattleView
    {
        public PlayerBattleContext Context { get; }
        public PlayerBattleRoute Route { get; }
        public PlayerNavigationReadResult Read { get; }
        public CandidateDemoView Battle { get; }
        public PlayerBattleConfirmation Confirmation { get; }
        public PreparedCandidateApplicationIntent OriginalIntent { get; }
        public CandidateApplicationCallResult Result { get; }
        public PlayerBattleReceipt Receipt { get; }
        public string Status { get; }
        public string SettlementReason { get; }
        public bool CanSettle => SettlementReason == null;
        public bool CanRetry => OriginalIntent != null && Battle.ApplicationView.PendingOperationId == OriginalIntent.OperationId &&
            (Battle.Phase == CandidateApplicationPhase.SaveFailed || Battle.Phase == CandidateApplicationPhase.PendingPreparation);
        public bool CanResolve => OriginalIntent != null && Battle.ApplicationView.PendingOperationId == OriginalIntent.OperationId &&
            Battle.Phase == CandidateApplicationPhase.CommitUnknown;
        public IReadOnlyList<PreparedLevel> NextLevels { get; }
        internal PlayerBattleView(PlayerBattleContext context, PlayerBattleRoute route, PlayerNavigationReadResult read,
            CandidateDemoView battle, PlayerBattleConfirmation confirmation, PreparedCandidateApplicationIntent intent,
            CandidateApplicationCallResult result, PlayerBattleReceipt receipt, string status, string settlementReason)
        {
            Context = context; Route = route; Read = read; Battle = battle; Confirmation = confirmation;
            OriginalIntent = intent; Result = result; Receipt = receipt; Status = status; SettlementReason = settlementReason;
            NextLevels = (read?.IsAvailable == true && receipt != null ? read.Levels.Where(l => l.LevelId != receipt.LevelId &&
                read.Head.Business.Progression.OpenFacts.Any(f => f.Level.LevelId == l.LevelId && f.Level.LevelVersion == l.LevelVersion)) :
                Enumerable.Empty<PreparedLevel>()).ToList().AsReadOnly();
        }
    }

    public sealed class PlayerDefaultReference
    {
        public ContentBinding Binding { get; }
        public string LevelId => Entry.Level.LevelId;
        public string LevelVersion => Entry.Level.LevelVersion;
        public PreparedBattleEntry Entry { get; }
        public IReadOnlyList<CandidateBattleOperationRecord> Steps { get; }
        public IReadOnlyList<byte> SeedBytes { get; }
        public IReadOnlyList<string> Conditions { get; }
        public IReadOnlyList<string> CurrentDifferences { get; }
        public string CurrentAnalysis => "当前局面分析未接入";
        public string Title => "已验证的开局默认参考";
        internal PlayerDefaultReference(ContentBinding binding, PreparedBattleEntry entry,
            IReadOnlyList<CandidateBattleOperationRecord> steps, IReadOnlyList<byte> seed,
            IEnumerable<string> conditions, IEnumerable<string> differences)
        {
            Binding = binding; Entry = entry; Steps = new List<CandidateBattleOperationRecord>(steps).AsReadOnly();
            SeedBytes = new List<byte>(seed).AsReadOnly(); Conditions = conditions.ToList().AsReadOnly();
            CurrentDifferences = differences.ToList().AsReadOnly();
        }
        // Display-only original facts; no commit or presentation token can be acknowledged from this value.
        public CandidateBattlePresentation ReadStep(int index)
        { return new CandidateBattlePresentation(null, Steps[index], null); }
    }

    public sealed class PlayerDefaultReferenceResult
    {
        public PlayerDefaultReference Value { get; }
        public string Code { get; }
        public bool IsAvailable => Value != null;
        internal PlayerDefaultReferenceResult(PlayerDefaultReference value, string code = null)
        { Value = value; Code = code ?? "Ready"; }
    }
}
