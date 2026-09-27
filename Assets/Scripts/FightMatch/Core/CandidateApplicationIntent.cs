using System;
using System.Collections.Generic;
using System.Numerics;
using System.Security.Cryptography;
using FlowPuzzle.Core;

namespace FightMatch.Core
{
    public enum CandidateApplicationKind
    {
        InitializeProfile = 1,
        EnterAttempt = 2,
        Attack = 3,
        Link = 4,
        Rollback = 5,
        SettleVictory = 6,
        ExitAttempt = 7,
        RestartAttempt = 8,
        AdvanceRecovery = 9,
        MigrateRoster = 10,
        SetFormation = 11,
        EnterFormation = 12,
        PermanentRequest = 13,
        MigratePermanent = 14
    }

    public sealed class CandidateApplicationIntentInput
    {
        internal CandidatePermanentQuote Permanent;
        internal CandidateRosterMigrationInput PermanentMigration;
        public void SetPermanent(CandidatePermanentQuote value) { Permanent = value; }
        public CandidatePermanentQuote GetPermanent() { return Permanent; }
        public void SetPermanentMigration(CandidateRosterMigrationInput value) { PermanentMigration = value; }
        public CandidateRosterMigrationInput GetPermanentMigration() { return PermanentMigration; }
        public string PlayerId { get; set; }
        public string OperationId { get; set; }
        public CandidateApplicationKind? Kind { get; set; }
        public uint? FormatVersion { get; set; }
        public string ExpectedCommitId { get; set; }
        public RuleContext Context { get; set; }
        public CandidateApplicationInitializeInput InitializeProfile { get; set; }
        public CandidateApplicationEnterInput EnterAttempt { get; set; }
        public CandidateApplicationAttackInput Attack { get; set; }
        public CandidateApplicationLinkInput Link { get; set; }
        public CandidateApplicationRollbackInput Rollback { get; set; }
        public CandidateApplicationVictoryInput SettleVictory { get; set; }
        public CandidateApplicationEndInput ExitAttempt { get; set; }
        public CandidateApplicationEndInput RestartAttempt { get; set; }
        public CandidateApplicationRecoveryInput AdvanceRecovery { get; set; }
        public CandidateRosterInitializeInput RosterInitialize { get; set; }
        public CandidateRosterMigrationInput MigrateRoster { get; set; }
        public CandidateFormationInput SetFormation { get; set; }
        public CandidateFormationEntryInput EnterFormation { get; set; }
    }

    public sealed class CandidateApplicationInitializeInput
    {
        public string CharacterId { get; set; }
        public string ClassId { get; set; }
        public BigInteger? InitialLevel { get; set; }
        public BigInteger? InitialExperience { get; set; }
        public int? OriginalSlot { get; set; }
    }

    public sealed class CandidateRosterInitializeInput
    {
        public IReadOnlyList<CandidateApplicationInitializeInput> Characters { get; set; }
        public IReadOnlyList<string> Slots { get; set; }
    }
    public sealed class CandidateRosterMigrationInput
    {
        public BigInteger? SourceGeneration { get; set; }
        public ulong? SourceDescriptorLength { get; set; }
        public IReadOnlyList<byte> SourceDescriptorSha256 { get; set; }
    }
    public sealed class CandidateFormationInput
    {
        public BigInteger? ExpectedFormationRevision { get; set; }
        public IReadOnlyList<string> Slots { get; set; }
    }
    public sealed class CandidateCharacterRevisionInput
    {
        public string CharacterId { get; set; }
        public BigInteger? ExpectedRevision { get; set; }
    }
    public sealed class CandidateFormationEntryInput
    {
        public string LevelId { get; set; }
        public string LevelVersion { get; set; }
        public BigInteger? ExpectedFormationRevision { get; set; }
        public IReadOnlyList<string> Slots { get; set; }
        public IReadOnlyList<CandidateCharacterRevisionInput> SelectedCharacters { get; set; }
        public BigInteger? ExpectedInventoryRevision { get; set; }
        public BigInteger? ExpectedProgressionRevision { get; set; }
        public CandidateTimeSample TimeSample { get; set; }
    }

    public sealed class CandidateApplicationEnterInput
    {
        public string LevelId { get; set; }
        public string LevelVersion { get; set; }
        public string CharacterId { get; set; }
        public BigInteger? ExpectedCharacterRevision { get; set; }
        public int? OriginalSlot { get; set; }
    }

    public sealed class CandidateApplicationAttackInput
    {
        public string AttemptId { get; set; }
        public BigInteger? ExpectedSceneRevision { get; set; }
        public BattleCombatantKey Actor { get; set; }
        public BattlePairKey Pair { get; set; }
        public IReadOnlyList<FlowPos> Route { get; set; }
        public BigInteger? ExpectedPreferenceRevision { get; set; }
        public bool? ItemUseEnabled { get; set; }
    }

    public sealed class CandidateApplicationLinkInput
    {
        public string AttemptId { get; set; }
        public BigInteger? ExpectedSceneRevision { get; set; }
        public BattlePairKey Pair { get; set; }
        public IReadOnlyList<FlowPos> Route { get; set; }
    }

    public sealed class CandidateApplicationRollbackInput
    {
        public string AttemptId { get; set; }
        public BigInteger? ExpectedSceneRevision { get; set; }
        public string HistoryAnchorId { get; set; }
        public string TargetOperationId { get; set; }
        public IReadOnlyList<string> ConfirmedRemovedOperationIds { get; set; }
    }

    public sealed class CandidateApplicationVictoryInput
    {
        public string AttemptId { get; set; }
        public string ChallengeId { get; set; }
        public string EntryBaselineId { get; set; }
        public string FinalReportFingerprint { get; set; }
        public string TerminalOperationId { get; set; }
        public string RewardDefinitionId { get; set; }
        public string RewardDefinitionVersion { get; set; }
    }

    public sealed class CandidateApplicationEndInput
    {
        public string AttemptId { get; set; }
        public string ChallengeId { get; set; }
        public string EntryBaselineId { get; set; }
        public BigInteger? ExpectedSceneRevision { get; set; }
    }

    public sealed class CandidateApplicationRecoveryInput
    {
        public string CharacterId { get; set; }
        public string RecoveryId { get; set; }
        public BigInteger? ExpectedCharacterRevision { get; set; }
        public CandidateTimeSample TimeSample { get; set; }
    }

    // Payload DTOs are private implementation data after canonical decoding, never returned to callers.
    public sealed class PreparedCandidateApplicationIntent
    {
        public CandidatePermanentQuote GetPermanent() { return Data.Permanent; }
        public CandidateRosterMigrationInput GetPermanentMigration()
        {
            var source = Data.PermanentMigration;
            return source == null ? null : new CandidateRosterMigrationInput
            {
                SourceGeneration = source.SourceGeneration,
                SourceDescriptorLength = source.SourceDescriptorLength,
                SourceDescriptorSha256 = new List<byte>(source.SourceDescriptorSha256).AsReadOnly()
            };
        }
        internal CandidateApplicationIntentInput Data { get; }
        internal byte[] Bytes { get; }
        public string PlayerId => Data.PlayerId;
        public string OperationId => Data.OperationId;
        public CandidateApplicationKind Kind => Data.Kind.Value;
        public uint FormatVersion => Data.FormatVersion.Value;
        public string ExpectedCommitId => Data.ExpectedCommitId;
        public PreparedRuleContext Context { get; }
        public IReadOnlyList<byte> CanonicalBytes { get; }
        public IReadOnlyList<byte> Sha256 { get; }

        internal PreparedCandidateApplicationIntent(CandidateApplicationIntentInput frozen, byte[] bytes)
        {
            Data = frozen;
            Bytes = bytes;
            Context = RuleContextChecks.Freeze(frozen.Context);
            CanonicalBytes = Array.AsReadOnly(bytes);
            using (var sha = SHA256.Create()) Sha256 = Array.AsReadOnly(sha.ComputeHash(bytes));
        }
    }
}
