using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using static FightMatch.Core.BusinessFields;

namespace FightMatch.Core
{
    internal static class CandidateApplicationIntentCodec
    {
        internal static PreparedCandidateApplicationIntent Prepare(CandidateApplicationIntentInput input, SaveCodecBudget budget)
        {
            Shape(input, budget);
            var measure = new BusinessFields(Stream.Null, false, budget);
            Visit(measure, input);
            var bytes = new byte[(int)measure.Used];
            using (var stream = new MemoryStream(bytes, true)) Visit(new BusinessFields(stream, false, budget), input);
            return Read(bytes, budget);
        }

        internal static PreparedCandidateApplicationIntent Read(byte[] bytes, SaveCodecBudget budget)
        {
            SaveCodecFailure.Limit((ulong)bytes.Length, budget.MaxEnvelopeBytes, "Intent.Bytes", "EnvelopeBytes");
            using (var stream = new MemoryStream(bytes, false))
            {
                var fields = new BusinessFields(stream, true, budget, (ulong)bytes.Length);
                var data = Visit(fields, null);
                fields.End("Intent");
                Shape(data, budget);
                return new PreparedCandidateApplicationIntent(data, bytes);
            }
        }

        internal static PreparedCandidateApplicationIntent ReadFrozenPublishedInitialize(byte[] canonicalBytes, SaveCodecBudget budget)
        {
            Need(canonicalBytes != null, "CreateRecord.Intent", "MissingField");
            SaveCodecFailure.Limit((ulong)canonicalBytes.Length, budget.MaxEnvelopeBytes, "CreateRecord.Intent", "EnvelopeBytes");
            var intent = Read((byte[])canonicalBytes.Clone(), budget);
            Need(intent.Kind == CandidateApplicationKind.InitializeProfile && intent.ExpectedCommitId == null &&
                intent.Context is PreparedPublishedRuleContext, "CreateRecord.Intent", "InconsistentCreateIntent");
            var canonical = Prepare(intent.Data, budget);
            Need(SameBytes(canonical.Bytes, canonicalBytes), "CreateRecord.Intent.Canonical", "InconsistentCreateIntent");
            return intent;
        }

        private static void Shape(CandidateApplicationIntentInput input, SaveCodecBudget budget)
        {
            Need(input != null, "Intent", "MissingField");
            Need(input.Kind.HasValue, "Kind", "MissingField");
            var version = input.FormatVersion ?? (input.Context is PublishedRuleContext ? 2U : 1U);
            Need(version >= 1 && version <= 4 && (version >= 2) == (input.Context is PublishedRuleContext), "FormatVersion", "UnsupportedSchema");
            Need(version == 4 ? input.Kind == CandidateApplicationKind.InitializeProfile || input.Kind == CandidateApplicationKind.PermanentRequest || input.Kind == CandidateApplicationKind.MigratePermanent :
                version == 3 ? input.Kind == CandidateApplicationKind.InitializeProfile || (int)input.Kind.Value >= 10 &&
                (int)input.Kind.Value <= 12 : (int)input.Kind.Value >= 1 && (int)input.Kind.Value <= 9, "Kind", "UnsupportedBinding");
            Need(version >= 3 ? input.InitializeProfile == null : input.RosterInitialize == null &&
                input.MigrateRoster == null && input.SetFormation == null && input.EnterFormation == null, "Payload", "InvalidValue");
            var count = (input.InitializeProfile == null ? 0 : 1) + (input.EnterAttempt == null ? 0 : 1) +
                (input.Attack == null ? 0 : 1) + (input.Link == null ? 0 : 1) + (input.Rollback == null ? 0 : 1) +
                (input.SettleVictory == null ? 0 : 1) + (input.ExitAttempt == null ? 0 : 1) +
                (input.RestartAttempt == null ? 0 : 1) + (input.AdvanceRecovery == null ? 0 : 1) +
                (input.RosterInitialize == null ? 0 : 1) + (input.MigrateRoster == null ? 0 : 1) +
                (input.SetFormation == null ? 0 : 1) + (input.EnterFormation == null ? 0 : 1) +
                (input.Permanent == null ? 0 : 1) + (input.PermanentMigration == null ? 0 : 1);
            Need(version == 4 || input.Permanent == null && input.PermanentMigration == null, "Permanent.Payload", "UnsupportedSchema");
            Need(count == 1, "Payload", count == 0 ? "MissingField" : "InvalidValue");
            Need(input.Context != null, "Context", "MissingField");
            if (input.Context is CandidateContext candidate) SaveEnvelopeCodec.CheckList(candidate.SourceNotes, budget, "Context.SourceNotes");
            var check = new GrowthChecks(budget.Math);
            Need(check.Context(input.Context, "Context"), check.Path ?? "Context", check.Code.ToString());
        }

        private static CandidateApplicationIntentInput Visit(BusinessFields f, CandidateApplicationIntentInput input)
        {
            foreach (var b in new byte[] { 70, 77, 73, 78, 84, 48, 48 })
                Need(f.U(b, 1, "Intent.Magic") == b, "Intent.Magic", "Malformed");
            var version = input?.FormatVersion ?? (input?.Context is PublishedRuleContext ? 2U : 1U);
            var tag = f.U(48UL + version, 1, "Intent.Magic");
            Need(tag >= 49 && tag <= 52, "Intent.Magic", "Malformed");
            var published = tag >= 50;
            f.StrictUnicode = published;
            var schema = tag - 48;
            Need(f.U(schema, 4, "Intent.Schema") == schema, "Intent.Schema", "UnsupportedSchema");
            var result = new CandidateApplicationIntentInput
            {
                PlayerId = f.Text(input?.PlayerId, "PlayerId"),
                OperationId = f.Text(input?.OperationId, "OperationId"),
                Kind = (CandidateApplicationKind)f.Enum((int)(input?.Kind ?? 0), 1, schema == 4 ? 14 : schema == 3 ? 12 : 9, "Kind"),
                FormatVersion = (uint)schema,
                ExpectedCommitId = f.Text(input?.ExpectedCommitId, "ExpectedCommitId", true),
                Context = published ? f.PublishedContext(f.Reading ? null : ((PublishedRuleContext)input.Context).Binding, "Context")
                    : f.Context(f.Reading ? null : RuleContextChecks.Freeze(input.Context), "Context")
            };
            switch (result.Kind.Value)
            {
                case CandidateApplicationKind.InitializeProfile:
                    if (schema >= 3)
                    {
                        var init = input?.RosterInitialize;
                        f.Required(init, "RosterInitialize");
                        var characters = f.List(init?.Characters, (x, p) => Initialize(f, x, p), "RosterInitialize.Characters", 20);
                        Need(characters.Count > 0, "RosterInitialize.Characters");
                        var classes = new HashSet<string>(StringComparer.Ordinal); string previous = null;
                        foreach (var character in characters)
                        {
                            Need((previous == null || StringComparer.Ordinal.Compare(previous, character.CharacterId) < 0) &&
                                classes.Add(character.ClassId), "RosterInitialize.Characters", "InvalidValue");
                            previous = character.CharacterId;
                        }
                        var slots = CandidateRosterSaveCodec.Slots(f, init?.Slots, "RosterInitialize.Slots");
                        foreach (var id in slots) if (id != null) Need(characters.Exists(x => x.CharacterId == id), "RosterInitialize.Slots", "InconsistentBinding");
                        CheckSlots(slots);
                        result.RosterInitialize = new CandidateRosterInitializeInput { Characters = characters.AsReadOnly(), Slots = slots };
                    }
                    else result.InitializeProfile = Initialize(f, input?.InitializeProfile, "InitializeProfile");
                    break;
                case CandidateApplicationKind.PermanentRequest:
                    result.Permanent = new CandidatePermanentCodec(f).Quote(input?.Permanent, "Permanent");
                    Need(result.Permanent.PlayerId == result.PlayerId &&
                        result.Permanent.Binding.Same(((PublishedRuleContext)result.Context).Binding), "Permanent.Binding", "InconsistentBinding");
                    break;
                case CandidateApplicationKind.MigratePermanent:
                    var permanentSource = input?.PermanentMigration;
                    f.Required(permanentSource, "PermanentMigration");
                    result.PermanentMigration = new CandidateRosterMigrationInput
                    {
                        SourceGeneration = Integer(f, permanentSource?.SourceGeneration, "PermanentMigration.Generation"),
                        SourceDescriptorLength = f.U(permanentSource?.SourceDescriptorLength ?? 0, 8, "PermanentMigration.Length"),
                        SourceDescriptorSha256 = new CandidatePermanentCodec(f).Digest(permanentSource?.SourceDescriptorSha256, "PermanentMigration.Sha256")
                    };
                    Need(result.PermanentMigration.SourceDescriptorLength > 0, "PermanentMigration.Length");
                    Need(f.U(3, 4, "PermanentMigration.From") == 3 && f.U(4, 4, "PermanentMigration.To") == 4,
                        "PermanentMigration.Version", "UnsupportedSchema");
                    break;
                case CandidateApplicationKind.MigrateRoster:
                    var migration = input?.MigrateRoster;
                    f.Required(migration, "MigrateRoster");
                    var generation = Integer(f, migration?.SourceGeneration, "MigrateRoster.SourceGeneration");
                    f.Required(migration?.SourceDescriptorLength, "MigrateRoster.SourceDescriptorLength");
                    var length = f.U(migration?.SourceDescriptorLength ?? 0, 8, "MigrateRoster.SourceDescriptorLength");
                    Need(length > 0, "MigrateRoster.SourceDescriptorLength");
                    if (!f.Reading) Need(migration.SourceDescriptorSha256 != null && migration.SourceDescriptorSha256.Count == 32,
                        "MigrateRoster.SourceDescriptorSha256", "InvalidValue");
                    var digest = new byte[32];
                    for (var i = 0; i < digest.Length; i++) digest[i] = (byte)f.U(f.Reading ? 0UL : migration.SourceDescriptorSha256[i], 1, "MigrateRoster.SourceDescriptorSha256");
                    Need(f.U(2, 4, "MigrateRoster.From") == 2 && f.U(3, 4, "MigrateRoster.To") == 3, "MigrateRoster.Version", "UnsupportedSchema");
                    result.MigrateRoster = new CandidateRosterMigrationInput { SourceGeneration = generation, SourceDescriptorLength = length,
                        SourceDescriptorSha256 = Array.AsReadOnly(digest) };
                    break;
                case CandidateApplicationKind.SetFormation:
                    var formation = input?.SetFormation;
                    f.Required(formation, "SetFormation");
                    result.SetFormation = new CandidateFormationInput {
                        ExpectedFormationRevision = Integer(f, formation?.ExpectedFormationRevision, "SetFormation.ExpectedFormationRevision"),
                        Slots = CandidateRosterSaveCodec.Slots(f, formation?.Slots, "SetFormation.Slots") };
                    CheckSlots(result.SetFormation.Slots);
                    break;
                case CandidateApplicationKind.EnterFormation:
                    var entry = input?.EnterFormation;
                    f.Required(entry, "EnterFormation");
                    var ef = new CandidateFormationEntryInput {
                        LevelId = f.Text(entry?.LevelId, "EnterFormation.LevelId"),
                        LevelVersion = f.Text(entry?.LevelVersion, "EnterFormation.LevelVersion"),
                        ExpectedFormationRevision = Integer(f, entry?.ExpectedFormationRevision, "EnterFormation.ExpectedFormationRevision"),
                        Slots = CandidateRosterSaveCodec.Slots(f, entry?.Slots, "EnterFormation.Slots"),
                        SelectedCharacters = f.List(entry?.SelectedCharacters, (x, p) => {
                            f.Required(x, p); return new CandidateCharacterRevisionInput {
                                CharacterId = f.Text(x?.CharacterId, p + ".CharacterId"),
                                ExpectedRevision = Integer(f, x?.ExpectedRevision, p + ".ExpectedRevision") };
                        }, "EnterFormation.SelectedCharacters", 8).AsReadOnly(),
                        ExpectedInventoryRevision = Integer(f, entry?.ExpectedInventoryRevision, "EnterFormation.ExpectedInventoryRevision"),
                        ExpectedProgressionRevision = Integer(f, entry?.ExpectedProgressionRevision, "EnterFormation.ExpectedProgressionRevision"),
                        TimeSample = Time(f, entry?.TimeSample, "EnterFormation.TimeSample") };
                    var selected = CheckSlots(ef.Slots);
                    Need(selected.Count == ef.SelectedCharacters.Count, "EnterFormation.SelectedCharacters", "InconsistentBinding");
                    string previousId = null;
                    foreach (var character in ef.SelectedCharacters)
                    {
                        Need(selected.Contains(character.CharacterId) && (previousId == null ||
                            StringComparer.Ordinal.Compare(previousId, character.CharacterId) < 0), "EnterFormation.SelectedCharacters", "InvalidValue");
                        previousId = character.CharacterId;
                    }
                    var levelVersion = Take(ExactSaveValueCodec.DecodeInteger(ef.LevelVersion, f.Budget), "EnterFormation.LevelVersion");
                    Need(levelVersion.HasValue && levelVersion.Value.Sign > 0, "EnterFormation.LevelVersion");
                    Take(DefinitionBinding.Prepare(((PublishedRuleContext)result.Context).Binding, ef.LevelId, levelVersion.Value, f.Budget), "EnterFormation.Binding");
                    result.EnterFormation = ef;
                    break;
                case CandidateApplicationKind.EnterAttempt:
                    var enter = input?.EnterAttempt;
                    f.Required(enter, "EnterAttempt");
                    result.EnterAttempt = new CandidateApplicationEnterInput
                    {
                        LevelId = f.Text(enter?.LevelId, "EnterAttempt.LevelId"),
                        LevelVersion = f.Text(enter?.LevelVersion, "EnterAttempt.LevelVersion"),
                        CharacterId = f.Text(enter?.CharacterId, "EnterAttempt.CharacterId"),
                        ExpectedCharacterRevision = Integer(f, enter?.ExpectedCharacterRevision, "EnterAttempt.ExpectedCharacterRevision"),
                        OriginalSlot = Slot(f, enter?.OriginalSlot, "EnterAttempt.OriginalSlot")
                    };
                    break;
                case CandidateApplicationKind.Attack:
                    var attack = input?.Attack;
                    f.Required(attack, "Attack");
                    var a = new CandidateApplicationAttackInput
                    {
                        AttemptId = f.Text(attack?.AttemptId, "Attack.AttemptId"),
                        ExpectedSceneRevision = Integer(f, attack?.ExpectedSceneRevision, "Attack.ExpectedSceneRevision"),
                        Actor = f.Combatant(attack?.Actor, "Attack.Actor"),
                        Pair = f.PairKey(attack?.Pair, "Attack.Pair"),
                        Route = f.Route(attack?.Route, "Attack.Route"),
                        ExpectedPreferenceRevision = Integer(f, attack?.ExpectedPreferenceRevision, "Attack.ExpectedPreferenceRevision")
                    };
                    f.Required(attack?.ItemUseEnabled, "Attack.ItemUseEnabled");
                    a.ItemUseEnabled = f.Flag(attack?.ItemUseEnabled ?? false, "Attack.ItemUseEnabled");
                    Need(a.Actor.Kind == BattleCombatantKind.Participant && a.Actor.AttemptId == a.AttemptId,
                        "Attack.Actor", "InconsistentBinding");
                    Need(a.Pair.AttemptId == a.AttemptId, "Attack.Pair", "InconsistentBinding");
                    Need(a.Route.Count >= 2, "Attack.Route");
                    result.Attack = a;
                    break;
                case CandidateApplicationKind.Link:
                    var link = input?.Link;
                    f.Required(link, "Link");
                    var l = new CandidateApplicationLinkInput
                    {
                        AttemptId = f.Text(link?.AttemptId, "Link.AttemptId"),
                        ExpectedSceneRevision = Integer(f, link?.ExpectedSceneRevision, "Link.ExpectedSceneRevision"),
                        Pair = f.PairKey(link?.Pair, "Link.Pair"),
                        Route = f.Route(link?.Route, "Link.Route")
                    };
                    Need(l.Pair.AttemptId == l.AttemptId, "Link.Pair", "InconsistentBinding");
                    Need(l.Route.Count >= 2, "Link.Route");
                    result.Link = l;
                    break;
                case CandidateApplicationKind.Rollback:
                    var rollback = input?.Rollback;
                    f.Required(rollback, "Rollback");
                    var r = new CandidateApplicationRollbackInput
                    {
                        AttemptId = f.Text(rollback?.AttemptId, "Rollback.AttemptId"),
                        ExpectedSceneRevision = Integer(f, rollback?.ExpectedSceneRevision, "Rollback.ExpectedSceneRevision"),
                        HistoryAnchorId = f.Text(rollback?.HistoryAnchorId, "Rollback.HistoryAnchorId"),
                        TargetOperationId = f.Text(rollback?.TargetOperationId, "Rollback.TargetOperationId"),
                        ConfirmedRemovedOperationIds = f.Strings(rollback?.ConfirmedRemovedOperationIds, "Rollback.ConfirmedRemovedOperationIds")
                    };
                    Need(r.ConfirmedRemovedOperationIds.Count > 0, "Rollback.ConfirmedRemovedOperationIds");
                    var ids = new HashSet<string>(StringComparer.Ordinal);
                    foreach (var id in r.ConfirmedRemovedOperationIds)
                        Need(ids.Add(id), "Rollback.ConfirmedRemovedOperationIds", "InvalidValue");
                    result.Rollback = r;
                    break;
                case CandidateApplicationKind.SettleVictory:
                    var victory = input?.SettleVictory;
                    f.Required(victory, "SettleVictory");
                    result.SettleVictory = new CandidateApplicationVictoryInput
                    {
                        AttemptId = f.Text(victory?.AttemptId, "SettleVictory.AttemptId"),
                        ChallengeId = f.Text(victory?.ChallengeId, "SettleVictory.ChallengeId"),
                        EntryBaselineId = f.Text(victory?.EntryBaselineId, "SettleVictory.EntryBaselineId"),
                        FinalReportFingerprint = f.Text(victory?.FinalReportFingerprint, "SettleVictory.FinalReportFingerprint"),
                        TerminalOperationId = f.Text(victory?.TerminalOperationId, "SettleVictory.TerminalOperationId"),
                        RewardDefinitionId = f.Text(victory?.RewardDefinitionId, "SettleVictory.RewardDefinitionId"),
                        RewardDefinitionVersion = f.Text(victory?.RewardDefinitionVersion, "SettleVictory.RewardDefinitionVersion")
                    };
                    break;
                case CandidateApplicationKind.ExitAttempt:
                    result.ExitAttempt = End(f, input?.ExitAttempt, "ExitAttempt");
                    break;
                case CandidateApplicationKind.RestartAttempt:
                    result.RestartAttempt = End(f, input?.RestartAttempt, "RestartAttempt");
                    break;
                case CandidateApplicationKind.AdvanceRecovery:
                    var recovery = input?.AdvanceRecovery;
                    f.Required(recovery, "AdvanceRecovery");
                    result.AdvanceRecovery = new CandidateApplicationRecoveryInput
                    {
                        CharacterId = f.Text(recovery?.CharacterId, "AdvanceRecovery.CharacterId"),
                        RecoveryId = f.Text(recovery?.RecoveryId, "AdvanceRecovery.RecoveryId"),
                        ExpectedCharacterRevision = Integer(f, recovery?.ExpectedCharacterRevision, "AdvanceRecovery.ExpectedCharacterRevision"),
                        TimeSample = Time(f, recovery?.TimeSample, "AdvanceRecovery.TimeSample")
                    };
                    break;
            }
            if (published && result.EnterAttempt != null)
            {
                var enter = result.EnterAttempt;
                var levelVersion = Take(ExactSaveValueCodec.DecodeInteger(enter.LevelVersion, f.Budget), "EnterAttempt.LevelVersion");
                Need(levelVersion.HasValue && levelVersion.Value.Sign > 0, "EnterAttempt.LevelVersion");
                Take(DefinitionBinding.Prepare(((PublishedRuleContext)result.Context).Binding, enter.LevelId, levelVersion.Value, f.Budget), "EnterAttempt.Binding");
            }
            return result;
        }

        private static HashSet<string> CheckSlots(IReadOnlyList<string> slots)
        {
            var selected = new HashSet<string>(StringComparer.Ordinal);
            foreach (var id in slots) if (id != null) Need(selected.Add(id), "Slots", "DuplicateCharacter");
            return selected;
        }
        private static CandidateApplicationInitializeInput Initialize(BusinessFields f, CandidateApplicationInitializeInput init, string p)
        {
            f.Required(init, p);
            return new CandidateApplicationInitializeInput {
                CharacterId = f.Text(init?.CharacterId, p + ".CharacterId"),
                ClassId = f.Text(init?.ClassId, p + ".ClassId"),
                InitialLevel = Integer(f, init?.InitialLevel, p + ".InitialLevel"),
                InitialExperience = Integer(f, init?.InitialExperience, p + ".InitialExperience", 0),
                OriginalSlot = Slot(f, init?.OriginalSlot, p + ".OriginalSlot") };
        }
        private static CandidateApplicationEndInput End(BusinessFields f, CandidateApplicationEndInput input, string p)
        {
            f.Required(input, p);
            return new CandidateApplicationEndInput
            {
                AttemptId = f.Text(input?.AttemptId, p + ".AttemptId"),
                ChallengeId = f.Text(input?.ChallengeId, p + ".ChallengeId"),
                EntryBaselineId = f.Text(input?.EntryBaselineId, p + ".EntryBaselineId"),
                ExpectedSceneRevision = Integer(f, input?.ExpectedSceneRevision, p + ".ExpectedSceneRevision")
            };
        }

        private static BigInteger Integer(BusinessFields f, BigInteger? value, string path, int minimum = 1)
        {
            f.Required(value, path);
            return f.Integer(value ?? 0, path, minimum);
        }

        private static int Slot(BusinessFields f, int? value, string path)
        {
            f.Required(value, path);
            var slot = f.I(value ?? 0, path);
            Need(slot >= 0 && slot <= 2, path);
            return slot;
        }

        internal static CandidateTimeSample Time(BusinessFields f, CandidateTimeSample input, string p)
        {
            f.Required(input, p);
            if (!f.Reading) PrepareTime(input, f.Budget, p);
            var wall = f.SignedInteger(input?.WallUtcMilliseconds ?? 0, p + ".WallUtcMilliseconds");
            var observed = f.SignedInteger(input?.ObservedAtUtcMilliseconds ?? 0, p + ".ObservedAtUtcMilliseconds");
            var monotonic = f.Rational(input?.MonotonicElapsedMilliseconds, p + ".MonotonicElapsedMilliseconds", true);
            var scope = f.Text(input?.MonotonicScopeId, p + ".MonotonicScopeId", true);
            var sample = new CandidateTimeSample
            {
                WallUtcMilliseconds = wall,
                ObservedAtUtcMilliseconds = observed,
                MonotonicElapsedMilliseconds = monotonic,
                MonotonicScopeId = scope,
                Source = f.Text(input?.Source, p + ".Source"),
                Trust = (CandidateTimeTrust)f.Enum((int)(input?.Trust ?? 0), 1, 1, p + ".Trust"),
                Anomaly = (CandidateTimeAnomaly)f.Enum((int)(input?.Anomaly ?? 0), 0, 3, p + ".Anomaly")
            };
            PrepareTime(sample, f.Budget, p);
            return sample;
        }

        internal static PreparedCandidateTimeSample PrepareTime(CandidateTimeSample input, SaveCodecBudget budget, string path)
        {
            var check = new GrowthChecks(budget.Math);
            Need(CandidateRecoveryClock.PrepareTime(input, check, out var sample),
                path + "." + check.Path, check.Code.ToString());
            return sample;
        }

        internal static CandidateTimeSample TimeInput(PreparedCandidateTimeSample value)
        {
            return new CandidateTimeSample
            {
                WallUtcMilliseconds = value.WallUtcMilliseconds,
                ObservedAtUtcMilliseconds = value.ObservedAtUtcMilliseconds,
                MonotonicElapsedMilliseconds = value.MonotonicElapsedMilliseconds,
                MonotonicScopeId = value.MonotonicScopeId,
                Source = value.Source,
                Trust = value.Trust,
                Anomaly = value.Anomaly
            };
        }

        internal static bool SameBytes(IReadOnlyList<byte> a, IReadOnlyList<byte> b)
        {
            if (a.Count != b.Count) return false;
            for (var i = 0; i < a.Count; i++) if (a[i] != b[i]) return false;
            return true;
        }
    }
}
