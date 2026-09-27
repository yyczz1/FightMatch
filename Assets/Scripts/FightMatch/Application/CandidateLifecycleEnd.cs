using System;
using System.Collections.Generic;
using System.Linq;
using FightMatch.Core;

namespace FightMatch.Application
{
    public sealed partial class CandidateLifecycleApplicationSystem
    {
        private static CandidateApplicationBuildResult Finish(CandidateApplicationSnapshot basis, PreparedCandidateLifecycleRequest request,
            SaveCodecBudget codec, int maxLiveIntegerBits, int maxLogTerms)
        {
            var business = basis.Business; var run = business.ActiveHistory?.CurrentRun; var math = codec.Math;
            if (run == null) return Reject("NoActiveBattle", "ActiveHistory");
            var state = run.CurrentSnapshot; var entry = run.Baseline.Entry;
            var win = request.Kind == CandidateApplicationKind.SettleVictory;
            var restart = request.Kind == CandidateApplicationKind.RestartAttempt;
            var context = CandidateLifecyclePreparation.Context(entry.Context);
            if (state.Phase == BattlePhase.Closed || (state.Phase == BattlePhase.WonPendingSettlement) != win)
                return Reject("InvalidPhase", "Battle.Phase");
            var endInput = restart ? request.Input.RestartAttempt : request.Input.ExitAttempt;
            var victory = request.Input.SettleVictory;
            if (win)
            {
                if (run.FinalReport == null || basis.Continuation?.ReservedOperationId != request.OperationId ||
                    victory.AttemptId != entry.AttemptId || victory.ChallengeId != entry.ChallengeId || victory.EntryBaselineId != entry.EntryBaselineId ||
                    victory.FinalReportFingerprint != run.FinalReport.Fingerprint || victory.TerminalOperationId != basis.Continuation.ClosingOperationId)
                    return Reject("InconsistentBinding", "SettleVictory");
            }
            else if (endInput.AttemptId != entry.AttemptId || endInput.ChallengeId != entry.ChallengeId ||
                endInput.EntryBaselineId != entry.EntryBaselineId || endInput.ExpectedSceneRevision != state.SceneRevision)
                return Reject("StaleContext", "EndAttempt");
            if (restart && run.Binding.MappingId != CandidateRandomPreparer.SupportedMappingId)
                return Reject("UnsupportedBinding", "Random.MappingId");
            if (state.Members.Any(x => business.Roster.Find(x.Member.CharacterId) == null)) return Reject("UnsupportedBinding", "Participants");
            if (state.Members.Any(x => x.Hp.Numerator.IsZero) && !restart && request.EndTime == null) return Reject("MissingField", "EndTimeSample");
            var nextAttempt = restart ? Guid.NewGuid().ToString("N") : null;
            var endId = Guid.NewGuid().ToString("N"); var settlement = win ? Guid.NewGuid().ToString("N") : null;
            var ended = CandidateProgression.EndAttempt(business.Progression, new CandidateProgressionEndFacts
            { PlayerId = request.PlayerId, LevelId = entry.Level.LevelId, LevelVersion = entry.Level.LevelVersion, Context = context,
                ChallengeId = entry.ChallengeId, AttemptId = entry.AttemptId, EntryBaselineId = entry.EntryBaselineId, EndReceiptId = endId,
                Kind = win ? CandidateProgressionEndKind.NormalVictory : restart ? CandidateProgressionEndKind.ImmediateRestart : CandidateProgressionEndKind.NormalExit,
                SettlementId = settlement, FinalReportFingerprint = win ? run.FinalReport.Fingerprint : null, NewAttemptId = nextAttempt },
                business.Progression.StateRevision, math);
            if (!ended.IsAccepted) return Reject(ended.RejectionCode.ToString(), ended.FieldPath);
            var roster = business.Roster; var rewards = business.Rewards;
            IReadOnlyList<CandidateRewardMaterial> materials = Array.Empty<CandidateRewardMaterial>();
            if (win)
            {
                var definition = request.Content.Rewards.FirstOrDefault(x => x.RewardDefinitionId == victory.RewardDefinitionId && x.Version == victory.RewardDefinitionVersion);
                if (definition == null) return Reject("UnsupportedBinding", "RewardDefinition");
                CandidateRewardResult fixedReward;
                using (var scope = new ExactEvaluationScope(new ExactEvaluationBudget(math, maxLiveIntegerBits, maxLogTerms)))
                    fixedReward = CandidateBaseRewards.FixNormal(rewards, run.FinalReport, definition, ended.EndReceipt,
                        new CandidateRewardFixRequest { PlayerId = request.PlayerId, AttemptId = entry.AttemptId, ChallengeId = entry.ChallengeId,
                            EntryBaselineId = entry.EntryBaselineId, SettlementId = settlement, FinalReportFingerprint = run.FinalReport.Fingerprint,
                            ExpectedStateRevision = rewards.StateRevision }, scope);
                if (!fixedReward.IsAccepted) return Reject(fixedReward.RejectionCode.ToString(), fixedReward.FieldPath);
                rewards = fixedReward.Next; materials = fixedReward.BaseReward.Materials;
                foreach (var xp in fixedReward.BaseReward.Experience)
                {
                    var character = roster.Find(xp.CharacterId);
                    var grown = CandidateCharacterGrowth.ApplyBaseReward(character, new CandidateBaseExperience
                    { PlayerId = request.PlayerId, CharacterId = xp.CharacterId, AttemptId = entry.AttemptId, SettlementId = settlement,
                        Context = context, Amount = xp.Amount }, character.StateRevision, math);
                    if (!grown.IsAccepted) return Reject(grown.RejectionCode.ToString(), grown.FieldPath);
                    roster = roster.Replace(grown.Next);
                }
            }
            foreach (var participant in state.Members)
            {
                var character = roster.Find(participant.Member.CharacterId); var down = participant.Hp.Numerator.IsZero;
                var recovery = down && !restart;
                var characterEnd = CandidateCharacterEnd.Propose(character, new CandidateCharacterEndFacts
                { PlayerId = request.PlayerId, CharacterId = character.CharacterId, AttemptId = entry.AttemptId, EntryBaselineId = entry.EntryBaselineId,
                    EndReceiptId = endId, Context = context, Kind = win ? CandidateCharacterEndKind.NormalVictory : restart ?
                        CandidateCharacterEndKind.ImmediateRestart : CandidateCharacterEndKind.NormalExit,
                    WasParticipant = true, WasDown = down, RecoveryId = recovery ? Guid.NewGuid().ToString("N") : null,
                    TimeSample = recovery ? request.EndTime : null }, character.StateRevision, math);
                if (!characterEnd.IsAccepted) return Reject(characterEnd.RejectionCode.ToString(), characterEnd.FieldPath);
                roster = roster.Replace(characterEnd.Next);
            }
            var inventoryEnd = CandidateInventory.End(business.Inventory, new CandidateInventoryEndIntent
            { PlayerId = request.PlayerId, AttemptId = entry.AttemptId, EntryBaselineId = entry.EntryBaselineId, EndReceiptId = endId,
                Context = context, Kind = win ? CandidateInventoryEndKind.NormalVictory : restart ? CandidateInventoryEndKind.ImmediateRestart : CandidateInventoryEndKind.NormalExit,
                SettlementId = settlement, NewAttemptId = nextAttempt, Remaining = new List<CandidateInventoryRemainingInput>(),
                Rewards = materials.Select(x => new CandidateInventoryQuantityInput { ItemId = x.ItemId, Quantity = x.Amount }).ToList() }, business.Inventory.StateRevision, math);
            if (!inventoryEnd.IsAccepted) return Reject(inventoryEnd.RejectionCode.ToString(), inventoryEnd.FieldPath);
            CandidateBattleHistory history = null;
            if (restart)
            {
                var input = new BattleEntryInput { PlayerId = entry.PlayerId, ChallengeId = entry.ChallengeId, AttemptId = nextAttempt,
                    EntryBaselineId = entry.EntryBaselineId, Context = context, Level = CopyLevel(entry.Level), CarryMode = entry.CarryMode,
                    RequiredFeatures = new List<string>(entry.RequiredFeatures), Members = entry.Members.Select(m => new MemberInput
                    { CharacterId = m.CharacterId, ClassId = m.ClassId, ClassKind = m.ClassKind, OriginalSlot = m.OriginalSlot,
                        Level = m.Level, IsReady = m.IsReady, StatsOrigin = m.StatsOrigin, StatsContext = CandidateLifecyclePreparation.Context(m.StatsContext),
                        Stats = CopyStats(m.Stats), EntryHp = m.EntryHp, LearnedSkills = new List<string>(m.LearnedSkills),
                        Crit = new WarriorCritInput { PassiveDefinitionId = m.Crit.PassiveDefinitionId, TargetProbability = m.Crit.TargetProbability,
                            C = m.Crit.C, Multiplier = m.Crit.Multiplier } }).ToList() };
                var prepared = PrepareEntry(input, entry.Level, codec);
                if (!prepared.IsAccepted) return Reject(prepared.RejectionCode.ToString(), prepared.FieldPath);
                var bytes = new byte[48]; var domains = new[] { run.Binding.Battle, run.Binding.BaseReward, run.Binding.Bonus };
                for (var i = 0; i < domains.Length; i++)
                { WriteLittleEndian(bytes, i * 16, domains[i].InitState); WriteLittleEndian(bytes, i * 16 + 8, domains[i].InitSequence); }
                history = CreateHistory(prepared.Entry, new CandidateSeedMaterial { Bytes = bytes, SourceCapabilityId = run.Binding.SourceCapabilityId,
                    MappingId = run.Binding.MappingId }, codec, out var rejected);
                if (rejected != null) return rejected;
            }
            return Complete(new CandidateBusinessInput(request.PlayerId, roster, inventoryEnd.Next, ended.Next, rewards,
                history, business.RetainedRuns, business.RetainedRollbacks, business.Format), new CandidateApplicationResultInput
                { EndReceiptId = endId, SettlementId = settlement, NewAttemptId = nextAttempt }, codec);
        }
        private static void WriteLittleEndian(byte[] bytes, int offset, ulong value)
        { for (var i = 0; i < 8; i++) bytes[offset + i] = (byte)(value >> (8 * i)); }
        private static CandidateApplicationBuildResult Recover(CandidateApplicationSnapshot basis, PreparedCandidateLifecycleRequest request, SaveCodecBudget codec)
        {
            var b = basis.Business; var input = request.Input.AdvanceRecovery;
            var character = b.Roster.Find(input.CharacterId);
            if (character == null) return Reject("InconsistentBinding", "CharacterId");
            var result = CandidateRecoveryClock.Advance(character, input.RecoveryId, input.TimeSample, input.ExpectedCharacterRevision.Value, codec.Math);
            if (!result.IsAccepted) return Reject(result.RejectionCode.ToString(), result.FieldPath);
            return Complete(new CandidateBusinessInput(request.PlayerId, b.Roster.Replace(result.Next), b.Inventory, b.Progression, b.Rewards,
                b.ActiveHistory, b.RetainedRuns, b.RetainedRollbacks, b.Format), new CandidateApplicationResultInput { RecoveryResult = result }, codec);
        }
    }
}
