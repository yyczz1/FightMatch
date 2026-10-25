using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using FightMatch.Core;

namespace FightMatch.Application
{
    public sealed partial class CandidateLifecycleApplicationSystem
    {
        private static CandidateApplicationBuildResult InitializeRoster(PreparedCandidateLifecycleRequest request, SaveCodecBudget codec)
        {
            var characters = new List<CandidateCharacterState>(); var content = request.Content;
            foreach (var recipe in request.Input.RosterInitialize.Characters)
            {
                var definition = content.FindGrowth(recipe.ClassId);
                if (definition == null) return Reject("UnsupportedBinding", "RosterInitialize.ClassId");
                var character = CandidateCharacterGrowth.CreateCandidate(definition, request.PlayerId, recipe.CharacterId,
                    recipe.InitialLevel.Value, recipe.InitialExperience.Value, recipe.OriginalSlot.Value, codec.Math);
                if (!character.IsAccepted) return Reject(character.RejectionCode.ToString(), character.FieldPath);
                characters.Add(character.Next);
            }
            var roster = CandidateRosterState.Create(request.PlayerId, characters, request.Input.RosterInitialize.Slots, codec);
            if (!roster.IsAccepted) return Reject(roster);
            var inventory = CandidateInventory.CreateCandidate(content.Inventory, roster.Value, codec);
            if (!inventory.IsAccepted) return Reject(inventory);
            var progression = CandidateProgression.CreateCandidate(content.Progression, request.PlayerId, codec.Math);
            if (!progression.IsAccepted) return Reject(progression.RejectionCode.ToString(), progression.FieldPath);
            var rewards = CandidateBaseRewards.CreateCandidate(request.PlayerId, codec.Math);
            if (!rewards.IsAccepted) return Reject(rewards.RejectionCode.ToString(), rewards.FieldPath);
            return Complete(new CandidateBusinessInput(request.PlayerId, roster.Value, inventory.Value, progression.Next, rewards.Next,
                null, Array.Empty<CandidateBattleRun>(), Array.Empty<CandidateRollbackRecord>(), (CandidateBusinessFormat)request.Intent.FormatVersion),
                new CandidateApplicationResultInput(), codec);
        }
        private static CandidateApplicationBuildResult Roster(CandidateApplicationSnapshot basis, PreparedCandidateLifecycleRequest request,
            SaveCodecBudget codec, Func<string> nextId, Action<byte[]> entropy)
        {
            var b = basis.Business;
            if (b.ActiveHistory != null || basis.Continuation != null) return Reject("ActiveAttemptConflict", "ActiveHistory");
            if (request.Intent.ExpectedCommitId != basis.Header.CommitId) return Reject("StaleContext", "ExpectedCommitId");
            if (request.Kind == CandidateApplicationKind.MigrateRoster)
            {
                var migrated = CandidateRosterProtocol.Migrate(basis, request.Intent, codec);
                return migrated.IsAccepted ? CandidateApplicationBuildResult.Success(migrated.Value, new CandidateApplicationResultInput()) : Reject(migrated);
            }
            if ((int)b.Format < 3) return Reject("RosterMigrationRequired", "Roster.Format");
            if (request.Kind == CandidateApplicationKind.SetFormation)
            {
                var changed = b.Roster.SetFormation(request.OperationId, request.Input.SetFormation.Slots,
                    request.Input.SetFormation.ExpectedFormationRevision.Value, codec);
                if (!changed.IsAccepted) return Reject(changed);
                return Complete(new CandidateBusinessInput(b.PlayerId, changed.Value, b.Inventory, b.Progression, b.Rewards,
                    null, b.RetainedRuns, b.RetainedRollbacks, b.Format), new CandidateApplicationResultInput(), codec);
            }
            return EnterFormation(basis, request, codec, nextId ?? (() => Guid.NewGuid().ToString("N")), entropy ?? Entropy);
        }
        private static void Entropy(byte[] bytes)
        { using (var source = RandomNumberGenerator.Create()) source.GetBytes(bytes); }
        private static CandidateApplicationBuildResult Reject<T>(SaveCodecResult<T> result)
        { return CandidateApplicationBuildResult.Rejected(result.RejectionCode, result.FieldPath, result.LimitReason, result.RequiredAtLeast, result.Allowed); }

        private static CandidateApplicationBuildResult EnterFormation(CandidateApplicationSnapshot basis, PreparedCandidateLifecycleRequest request,
            SaveCodecBudget codec, Func<string> nextId, Action<byte[]> entropy)
        {
            var business = basis.Business; var input = request.Input.EnterFormation; var math = codec.Math;
            if (business.Roster.FormationRevision != input.ExpectedFormationRevision ||
                !business.Roster.Formation.SequenceEqual(input.Slots) || business.Inventory.StateRevision != input.ExpectedInventoryRevision ||
                business.Progression.StateRevision != input.ExpectedProgressionRevision) return Reject("StaleContext", "EnterFormation.Revisions");
            foreach (var selected in input.SelectedCharacters)
                if (business.Roster.Find(selected.CharacterId)?.StateRevision != selected.ExpectedRevision)
                    return Reject("StaleContext", "EnterFormation.CharacterRevision");
            var entry = business.Roster.PrepareEntry(input.TimeSample, codec);
            if (!entry.IsAccepted) return Reject(entry);
            PublishedRuleDefinitions definitions = null;
            if ((int)business.Format >= 4)
            {
                var resolved = CandidateLifecyclePreparation.PermanentDefinitions(request.Content, codec);
                if (!resolved.IsAccepted) return Reject(resolved);
                definitions = resolved.Value;
                var supported = CandidatePermanentGrowth.CheckBattleCapability(entry.Value.Roster, definitions, codec);
                if (!supported.IsAccepted) return Reject(supported);
            }
            var context = CandidateLifecyclePreparation.Context(request.Intent.Context);
            var intent = new CandidateProgressionRosterIntent { PlayerId = business.PlayerId, LevelId = input.LevelId,
                LevelVersion = input.LevelVersion, Context = context, Participants = entry.Value.Participants,
                FormationRevision = input.ExpectedFormationRevision };
            var checkedEntry = CandidateProgression.CheckEntry(business.Progression, intent, math);
            if (!checkedEntry.IsAccepted) return Reject(checkedEntry.RejectionCode.ToString(), checkedEntry.FieldPath);
            var level = request.Content.Levels.FirstOrDefault(x => x.LevelId == input.LevelId && x.LevelVersion == input.LevelVersion);
            if (level == null) return Reject("UnsupportedBinding", "Level");
            if (request.Content.Chapter?.HasTauntBossState == true)
            {
                if (business.Format != CandidateBusinessFormat.PublishedChapterV5 || definitions == null)
                    return Reject("UnsupportedBinding", "EnterFormation.ChapterState");
                var levelBinding = DefinitionBinding.Prepare(definitions.Binding, level.LevelId,
                    BigInteger.Parse(level.LevelVersion, System.Globalization.CultureInfo.InvariantCulture), codec);
                if (!levelBinding.IsAccepted) return Reject(levelBinding);
                var admission = CandidatePermanentProgression.CheckTeachingAdmission(business, levelBinding.Value,
                    definitions, codec, request.Content.SourceDefinitions);
                if (!admission.IsAccepted) return Reject(admission);
            }
            var actors = new List<CandidateInventoryActorInput>(); var members = new List<MemberInput>();
            foreach (var participant in entry.Value.Participants)
            {
                var stats = participant.Stats;
                var loadout = business.Inventory.FindLoadout(stats.CharacterId);
                if (loadout == null) return Reject("InconsistentBinding", "Inventory.Loadout");
                // Any selected tactical item produces a carry row, even when its count is zero.
                if (loadout.ItemId != null) return Reject("UnsupportedBinding", "CarryMode");
                var character = entry.Value.Roster.Find(stats.CharacterId);
                var mage = stats.ClassKind == CharacterClassKind.Mage;
                if (mage && (business.Format != CandidateBusinessFormat.PublishedChapterV5 || request.Content.Chapter == null))
                    return Reject("UnsupportedBinding", "Entry.Mage");
                var probability = mage ? CandidateCharacterGrowth.ComputePenetrationProbability(character, math) : stats.TargetProbability;
                var coefficient = request.Content.CritCoefficients.FirstOrDefault(x => x.TargetProbability.Compare(probability, math) == 0);
                if (coefficient == null) return Reject("UnsupportedBinding", "CritCoefficients.TargetProbability");
                actors.Add(new CandidateInventoryActorInput { CharacterId = stats.CharacterId, ClassId = stats.ClassId,
                    ClassKind = stats.ClassKind, OriginalSlot = participant.OriginalSlot });
                var learned = entry.Value.Roster.GetPermanentEffects().Where(x => x.Quote.Kind == CandidatePermanentKind.LearnSkill &&
                    x.Quote.CharacterId == stats.CharacterId).Select(x => x.Quote.DefinitionId).Distinct().OrderBy(x => x, StringComparer.Ordinal).ToList();
                members.Add(new MemberInput { CharacterId = stats.CharacterId, ClassId = stats.ClassId, ClassKind = stats.ClassKind,
                    OriginalSlot = participant.OriginalSlot, Level = stats.Level, IsReady = stats.IsReady, StatsOrigin = stats.StatsOrigin,
                    StatsContext = CandidateLifecyclePreparation.Context(stats.Context), Stats = CopyStats(stats.Stats),
                    EntryHp = stats.EntryHp, LearnedSkills = learned,
                    BattleSkills = learned.Select(id => request.Content.Chapter.FindSkill(id)).Select(skill =>
                        new ChapterBattleSkillInput { SkillId = skill.SkillId, RecordVersion = skill.RecordVersion,
                            BattleCapabilityId = skill.BattleCapabilityId, CooldownTurns = skill.CooldownTurns }).ToList(),
                    Crit = mage ? null : new WarriorCritInput { PassiveDefinitionId = stats.PassiveDefinitionId,
                        TargetProbability = probability, C = coefficient.C, Multiplier = stats.CritMultiplier },
                    Penetration = !mage ? null : new ArcanePenetrationInput { PassiveDefinitionId = stats.PassiveDefinitionId,
                        TargetProbability = probability, C = coefficient.C, IgnoreMagicDefenseFraction = character.Definition.IgnoreFraction } });
            }
            var seedBudget = ExactSaveValueCodec.EncodeInteger(ulong.MaxValue, codec);
            if (!seedBudget.IsAccepted) return Reject(seedBudget);
            // This value is a disposable capability preview; no battle identity or entropy has been allocated.
            var battleInput = new BattleEntryInput { PlayerId = business.PlayerId, ChallengeId = request.OperationId,
                AttemptId = request.OperationId, EntryBaselineId = request.OperationId, Context = context,
                Level = CopyLevel(level), CarryMode = EntryCarryMode.Empty,
                RequiredFeatures = request.Content.Chapter?.HasTauntBossState == true
                    ? new List<string> { ChapterRuleDefinitions.StateFeatureId } : new List<string>(), Members = members };
            var preview = PrepareEntry(battleInput, level, codec);
            if (!preview.IsAccepted) return Reject(preview.RejectionCode.ToString(), preview.FieldPath);
            var inventoryPreview = CandidateInventory.Freeze(business.Inventory, new CandidateInventoryFreezeIntent {
                PlayerId = business.PlayerId, Context = context, AttemptId = request.OperationId,
                EntryBaselineId = request.OperationId, ReadyParticipants = actors }, entry.Value.Roster, business.Inventory.StateRevision, math);
            if (!inventoryPreview.IsAccepted) return Reject(inventoryPreview.RejectionCode.ToString(), inventoryPreview.FieldPath);
            if (inventoryPreview.Next.ActiveCarry.Mode != EntryCarryMode.Empty) return Reject("UnsupportedBinding", "CarryMode");
            intent.AttemptId = nextId(); intent.EntryBaselineId = nextId();
            intent.ChallengeId = checkedEntry.OpenChallenge?.ChallengeId ?? nextId();
            var begun = CandidateProgression.BeginAttempt(business.Progression, intent, business.Progression.StateRevision, math);
            if (!begun.IsAccepted) return Reject(begun.RejectionCode.ToString(), begun.FieldPath);
            var carry = CandidateInventory.Freeze(business.Inventory, new CandidateInventoryFreezeIntent {
                PlayerId = business.PlayerId, Context = context, AttemptId = intent.AttemptId,
                EntryBaselineId = intent.EntryBaselineId, ReadyParticipants = actors }, entry.Value.Roster, business.Inventory.StateRevision, math);
            if (!carry.IsAccepted) return Reject(carry.RejectionCode.ToString(), carry.FieldPath);
            if (carry.Next.ActiveCarry.Mode != EntryCarryMode.Empty) return Reject("UnsupportedBinding", "CarryMode");
            battleInput.ChallengeId = intent.ChallengeId; battleInput.AttemptId = intent.AttemptId; battleInput.EntryBaselineId = intent.EntryBaselineId;
            var prepared = PrepareEntry(battleInput, level, codec);
            if (!prepared.IsAccepted) return Reject(prepared.RejectionCode.ToString(), prepared.FieldPath);
            var bytes = new byte[48]; entropy(bytes);
            var history = CreateHistory(prepared.Entry, new CandidateSeedMaterial { Bytes = bytes,
                MappingId = CandidateRandomPreparer.SupportedMappingId, SourceCapabilityId = "local:System.Security.Cryptography.RandomNumberGenerator" }, codec, out var rejected);
            if (rejected != null) return rejected;
            return Complete(new CandidateBusinessInput(business.PlayerId, entry.Value.Roster, carry.Next, begun.Next, business.Rewards,
                history, business.RetainedRuns, business.RetainedRollbacks, business.Format), new CandidateApplicationResultInput {
                    ChallengeId = intent.ChallengeId, AttemptId = intent.AttemptId, EntryBaselineId = intent.EntryBaselineId,
                    RecoveryResults = entry.Value.Recoveries }, codec);
        }
    }
}
