using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using FightMatch.Core;

namespace FightMatch.Application
{
    public sealed partial class CandidateLifecycleApplicationSystem
    {
        private static CandidateApplicationBuildResult Initialize(PreparedCandidateLifecycleRequest request, SaveCodecBudget codec)
        {
            var recipe = request.Input.InitializeProfile; var content = request.Content; var math = codec.Math;
            var character = CandidateCharacterGrowth.CreateCandidate(content.Growth, request.PlayerId, recipe.CharacterId,
                recipe.InitialLevel.Value, recipe.InitialExperience.Value, recipe.OriginalSlot.Value, math);
            if (!character.IsAccepted) return Reject(character.RejectionCode.ToString(), character.FieldPath);
            var inventory = CandidateInventory.CreateCandidate(content.Inventory, request.PlayerId,
                new List<CandidateInventoryActorInput> { Actor(character.Next) }, math);
            if (!inventory.IsAccepted) return Reject(inventory.RejectionCode.ToString(), inventory.FieldPath);
            var progression = CandidateProgression.CreateCandidate(content.Progression, request.PlayerId, math);
            if (!progression.IsAccepted) return Reject(progression.RejectionCode.ToString(), progression.FieldPath);
            var rewards = CandidateBaseRewards.CreateCandidate(request.PlayerId, math);
            if (!rewards.IsAccepted) return Reject(rewards.RejectionCode.ToString(), rewards.FieldPath);
            return Complete(new CandidateBusinessInput(request.PlayerId, character.Next, inventory.Next, progression.Next, rewards.Next,
                null, Array.Empty<CandidateBattleRun>(), Array.Empty<CandidateRollbackRecord>()), new CandidateApplicationResultInput(), codec);
        }

        private static CandidateApplicationBuildResult Enter(CandidateApplicationSnapshot basis, PreparedCandidateLifecycleRequest request, SaveCodecBudget codec)
        {
            var business = basis.Business; var e = request.Input.EnterAttempt; var math = codec.Math;
            var context = CandidateLifecyclePreparation.Context(request.Intent.Context);
            if ((int)business.Format >= 3) return Reject("UnsupportedBinding", "EnterAttempt.Format");
            if (business.ActiveHistory != null) return Reject("ActiveAttemptConflict", "ActiveHistory");
            var check = CandidateProgression.CheckEntry(business.Progression, new CandidateProgressionLevelRequest
            { PlayerId = request.PlayerId, LevelId = e.LevelId, LevelVersion = e.LevelVersion, CharacterId = e.CharacterId,
                Context = context, ExpectedCharacterRevision = e.ExpectedCharacterRevision, ExpectedOriginalSlot = e.OriginalSlot }, business.Character, math);
            if (!check.IsAccepted) return Reject(check.RejectionCode.ToString(), check.FieldPath);
            var stats = CandidateCharacterGrowth.ComputeBaseStats(business.Character, math);
            var coefficient = request.Content.CritCoefficients.FirstOrDefault(x => x.TargetProbability.Compare(stats.TargetProbability, math) == 0);
            if (coefficient == null) return Reject("UnsupportedBinding", "CritCoefficients.TargetProbability");
            var level = request.Content.Levels.FirstOrDefault(x => x.LevelId == e.LevelId && x.LevelVersion == e.LevelVersion);
            if (level == null) return Reject("UnsupportedBinding", "Level");
            var seedBudget = ExactSaveValueCodec.EncodeInteger(ulong.MaxValue, codec);
            if (!seedBudget.IsAccepted) return CandidateApplicationBuildResult.Rejected(seedBudget.RejectionCode, seedBudget.FieldPath,
                seedBudget.LimitReason, seedBudget.RequiredAtLeast, seedBudget.Allowed);
            var attempt = Guid.NewGuid().ToString("N"); var baseline = Guid.NewGuid().ToString("N");
            var challenge = check.OpenChallenge?.ChallengeId ?? Guid.NewGuid().ToString("N");
            var started = CandidateProgression.BeginAttempt(business.Progression, new CandidateProgressionBeginIntent
            { PlayerId = request.PlayerId, LevelId = e.LevelId, LevelVersion = e.LevelVersion, CharacterId = e.CharacterId,
                Context = context, ExpectedCharacterRevision = e.ExpectedCharacterRevision, ExpectedOriginalSlot = e.OriginalSlot,
                AttemptId = attempt, EntryBaselineId = baseline, ChallengeId = challenge }, business.Character, business.Progression.StateRevision, math);
            if (!started.IsAccepted) return Reject(started.RejectionCode.ToString(), started.FieldPath);
            var frozen = CandidateInventory.Freeze(business.Inventory, new CandidateInventoryFreezeIntent
            { PlayerId = request.PlayerId, Context = context, AttemptId = attempt, EntryBaselineId = baseline,
                ReadyParticipants = new List<CandidateInventoryActorInput> { Actor(business.Character) } }, business.Inventory.StateRevision, math);
            if (!frozen.IsAccepted) return Reject(frozen.RejectionCode.ToString(), frozen.FieldPath);
            if (frozen.Next.ActiveCarry.Mode != EntryCarryMode.Empty) return Reject("UnsupportedBinding", "CarryMode");
            var input = new BattleEntryInput { PlayerId = request.PlayerId, ChallengeId = challenge, AttemptId = attempt,
                EntryBaselineId = baseline, Context = context, Level = CopyLevel(level), CarryMode = EntryCarryMode.Empty,
                RequiredFeatures = new List<string>(), Members = new List<MemberInput> { new MemberInput
                { CharacterId = stats.CharacterId, ClassId = stats.ClassId, ClassKind = stats.ClassKind, OriginalSlot = stats.OriginalSlot,
                    Level = stats.Level, IsReady = stats.IsReady, StatsOrigin = stats.StatsOrigin,
                    StatsContext = CandidateLifecyclePreparation.Context(stats.Context), Stats = CopyStats(stats.Stats), EntryHp = stats.EntryHp,
                    LearnedSkills = new List<string>(), Crit = new WarriorCritInput { PassiveDefinitionId = stats.PassiveDefinitionId,
                        TargetProbability = stats.TargetProbability, C = coefficient.C, Multiplier = stats.CritMultiplier } } } };
            var prepared = PrepareEntry(input, level, codec);
            if (!prepared.IsAccepted) return Reject(prepared.RejectionCode.ToString(), prepared.FieldPath);
            var bytes = new byte[48];
            using (var entropy = RandomNumberGenerator.Create()) entropy.GetBytes(bytes);
            var history = CreateHistory(prepared.Entry, new CandidateSeedMaterial { Bytes = bytes,
                MappingId = CandidateRandomPreparer.SupportedMappingId, SourceCapabilityId = "local:System.Security.Cryptography.RandomNumberGenerator" }, codec, out var rejected);
            if (rejected != null) return rejected;
            return Complete(new CandidateBusinessInput(request.PlayerId, business.Character, frozen.Next, started.Next, business.Rewards,
                history, business.RetainedRuns, business.RetainedRollbacks), new CandidateApplicationResultInput
                { ChallengeId = challenge, AttemptId = attempt, EntryBaselineId = baseline }, codec);
        }
        private static BattleEntryPreparationResult PrepareEntry(BattleEntryInput input, PreparedLevel level, SaveCodecBudget codec)
        {
            var preparer = new BattleEntryPreparer();
            if (!(input.Context is PublishedRuleContext context)) return preparer.PrepareCandidate(input, codec.Math);
            var version = ExactSaveValueCodec.DecodeInteger(level.LevelVersion, codec);
            var binding = DefinitionBinding.Prepare(context.Binding, level.LevelId, version.Value ?? 0, codec);
            return preparer.PreparePublished(input, binding.Value, level, codec.Math);
        }

        private static CandidateBattleHistory CreateHistory(PreparedBattleEntry entry, CandidateSeedMaterial material,
            SaveCodecBudget codec, out CandidateApplicationBuildResult rejected)
        {
            rejected = null;
            var binding = CandidateRandomPreparer.Prepare(entry, material, codec.Math);
            if (!binding.IsAccepted) { rejected = Reject(binding.RejectionCode.ToString(), binding.FieldPath); return null; }
            var run = CandidateBattleOperations.CreateCandidate(binding.Binding, codec.Math);
            if (!run.IsAccepted) { rejected = Reject(run.RejectionCode.ToString(), run.FieldPath); return null; }
            var history = CandidateHistoryOperations.CreateCandidate(run.Run, codec.Math);
            if (!history.IsAccepted) { rejected = Reject(history.RejectionCode.ToString(), history.FieldPath); return null; }
            return history.Next;
        }
        private static CandidateInventoryActorInput Actor(CandidateCharacterState c)
        { return new CandidateInventoryActorInput { CharacterId = c.CharacterId, ClassId = c.ClassId, ClassKind = c.Definition.ClassKind, OriginalSlot = c.OriginalSlot }; }
        private static StatsInput CopyStats(PreparedStats s)
        { return new StatsInput { MaxHp = s.MaxHp, Attack = s.Attack, PhysicalDefense = s.PhysicalDefense, MagicDefense = s.MagicDefense, Evasion = s.Evasion, AttackRange = s.AttackRange }; }
        private static LevelInput CopyLevel(PreparedLevel level)
        {
            return new LevelInput { LevelId = level.LevelId, LevelVersion = level.LevelVersion, RecommendedLevel = level.RecommendedLevel,
                Faces = level.Faces.Select(f => new FaceInput { FaceId = f.FaceId, Width = f.Width, Height = f.Height,
                    Pairs = f.Pairs.Select(p => new PairInput { PairId = p.PairId, GeometryColorId = p.GeometryColorId,
                        EndpointA = p.EndpointA, EndpointB = p.EndpointB, Enemy = new EnemyInput { EnemyInstanceKey = p.Enemy.EnemyInstanceKey,
                            EnemyDefinitionId = p.Enemy.EnemyDefinitionId, OriginalSlot = p.Enemy.OriginalSlot, StableOrder = p.Enemy.StableOrder,
                            Behavior = p.Enemy.Behavior, Stats = CopyStats(p.Enemy.Stats), IntentCycle = p.Enemy.IntentCycle.Select(i => new EnemyIntentInput
                            { Kind = i.Kind, Targeting = i.Targeting, DamageKind = i.DamageKind, DamageCoefficient = i.DamageCoefficient }).ToList() } }).ToList() }).ToList() };
        }
    }
}
