using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using FightMatch.Core;
using FlowPuzzle.Core;

namespace FightMatch.Application
{
    public sealed partial class CandidateBattleApplicationSystem
    {
        private sealed class BuildObservation
        {
            internal CandidateBattleDomainRejection Rejection;
            internal CandidateApplicationBuildResult Reject(string code, string path,
                CandidateBattleRejectionStage? stage = null, string routeReason = null, int? routeIndex = null,
                string limitReason = null, ulong? required = null, ulong? allowed = null)
            {
                Rejection = new CandidateBattleDomainRejection(code, path, stage, routeReason, routeIndex, limitReason, required, allowed);
                return CandidateApplicationBuildResult.Rejected(code, path, limitReason, required, allowed);
            }
        }

        private CandidateApplicationBuildResult Build(CandidateApplicationSnapshot basis, PreparedCandidateBattleRequest request,
            SaveCodecBudget budget, int maxRandomWords, BuildObservation observation)
        {
            try
            {
                var history = basis?.Business.ActiveHistory;
                if (history == null) return observation.Reject("NoActiveBattle", "ActiveHistory");
                var run = history.CurrentRun;
                var entry = run.Baseline.Entry;
                if (request.Intent.PlayerId != entry.PlayerId) return observation.Reject("InconsistentBinding", "PlayerId");
                if (request.AttemptId != entry.AttemptId) return observation.Reject("StaleContext", "AttemptId");
                if (!SameContext(request.Intent.Context, entry.Context)) return observation.Reject("InconsistentBinding", "Context");
                if (request.SceneRevision != run.CurrentSnapshot.SceneRevision) return observation.Reject("StaleContext", "ExpectedSceneRevision");
                Synchronize(basis);
                if (presentationToken != null) return observation.Reject("Busy", "Presentation");
                if (request.Kind == CandidateApplicationKind.Rollback) return Rollback(basis, request, budget, observation);
                if (request.Kind == CandidateApplicationKind.Attack)
                {
                    var inventory = basis.Business.Inventory;
                    if (request.PreferenceRevision != inventory.PreferenceRevision)
                        return observation.Reject("StaleContext", "Attack.ExpectedPreferenceRevision");
                    var loadout = inventory.FindLoadout(request.Actor.CharacterId);
                    if (loadout == null)
                        return observation.Reject("InconsistentBinding", "Attack.Actor");
                    var enabled = loadout.Enabled;
                    if (!enabled.HasValue && !CandidateDemoView.IsEmptyPreference(inventory, run.CurrentSnapshot, request.Actor.CharacterId))
                        return observation.Reject("UnsupportedBinding", "Inventory.Loadout");
                    if (request.ItemUseEnabled != (enabled ?? false))
                        return observation.Reject("InconsistentBinding", "Attack.ItemUseEnabled");
                }

                var time = new BigInteger(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                CandidateBattleResult evaluated;
                if (request.Kind == CandidateApplicationKind.Attack)
                {
                    evaluated = CandidateBattleOperations.EvaluateAttack(run, new CandidateAttackRequest
                    {
                        PlayerId = request.Intent.PlayerId, AttemptId = request.AttemptId, OperationId = request.OperationId,
                        ExpectedSceneRevision = request.SceneRevision, Actor = request.Actor, Pair = request.Pair,
                        Route = new List<FlowPos>(request.Route)
                    }, new CandidateBattleConditions
                    {
                        PreferenceRevision = request.PreferenceRevision, ItemUseEnabled = request.ItemUseEnabled
                    }, time, new RandomSamplingBudget(budget.Math, maxRandomWords));
                }
                else
                {
                    evaluated = CandidateBattleOperations.EvaluateLink(run, new CandidateLinkRequest
                    {
                        PlayerId = request.Intent.PlayerId, AttemptId = request.AttemptId, OperationId = request.OperationId,
                        ExpectedSceneRevision = request.SceneRevision, Pair = request.Pair, Route = new List<FlowPos>(request.Route)
                    }, time, budget.Math);
                }
                if (!evaluated.IsAccepted) return observation.Reject(evaluated.RejectionCode, evaluated.FieldPath,
                    evaluated.RejectionStage, evaluated.RouteReasonCode, evaluated.RouteCellIndex);
                var anchor = Guid.NewGuid().ToString("N");
                var appended = CandidateHistoryOperations.Append(history, evaluated.NextRun, anchor, budget.Math);
                if (!appended.IsAccepted) return observation.Reject(appended.RejectionCode.ToString(), appended.FieldPath);
                var reserved = evaluated.NextRun.CurrentSnapshot.Phase == BattlePhase.WonPendingSettlement
                    ? Guid.NewGuid().ToString("N") : null;
                var built = ReplaceHistory(basis, appended.Next, new CandidateApplicationResultInput { HistoryAnchorId = anchor },
                    reserved, budget, observation);
                if (built.IsAccepted) pendingPresentationOperation = request.OperationId;
                return built;
            }
            catch (ExactMathLimitException error)
            {
                observation.Reject("Limit", "Budget.Math", limitReason: error.ReasonCode,
                    required: (ulong)error.RequiredAtLeast, allowed: (ulong)error.Allowed);
                throw;
            }
        }

        private static CandidateApplicationBuildResult Rollback(CandidateApplicationSnapshot basis, PreparedCandidateBattleRequest request,
            SaveCodecBudget budget, BuildObservation observation)
        {
            var history = basis.Business.ActiveHistory;
            var read = CandidateHistoryOperations.ReadRange(history, new CandidateHistoryRangeRequest
            {
                PlayerId = request.Intent.PlayerId, AttemptId = request.AttemptId, ExpectedSceneRevision = request.SceneRevision,
                HistoryAnchorId = request.HistoryAnchorId
            }, budget.Math);
            if (!read.IsAccepted) return observation.Reject(read.RejectionCode.ToString(), read.FieldPath);
            var range = read.Range;
            if (range.HistoryAnchorId != request.HistoryAnchorId || range.OperationId != request.TargetOperationId ||
                range.SceneRevision != request.SceneRevision)
                return observation.Reject("InconsistentBinding", "Rollback.TargetOperationId");
            if (range.Entries.Count != request.ConfirmedRemovedOperationIds.Count)
                return observation.Reject("InconsistentBinding", "Rollback.ConfirmedRemovedOperationIds");
            for (var i = 0; i < range.Entries.Count; i++)
                if (range.Entries[i].OperationId != request.ConfirmedRemovedOperationIds[i])
                    return observation.Reject("InconsistentBinding", "Rollback.ConfirmedRemovedOperationIds[" + i + "]");
            var rolled = CandidateHistoryOperations.PrepareRollback(history, new CandidateRollbackRequest
            {
                PlayerId = request.Intent.PlayerId, AttemptId = request.AttemptId, OperationId = request.OperationId,
                ExpectedSceneRevision = request.SceneRevision, HistoryAnchorId = request.HistoryAnchorId
            }, range, budget.Math);
            if (!rolled.IsAccepted) return observation.Reject(rolled.RejectionCode.ToString(), rolled.FieldPath);
            return ReplaceHistory(basis, rolled.Next, new CandidateApplicationResultInput(), null, budget, observation);
        }

        private static CandidateApplicationBuildResult ReplaceHistory(CandidateApplicationSnapshot basis, CandidateBattleHistory next,
            CandidateApplicationResultInput result, string reserved, SaveCodecBudget budget, BuildObservation observation)
        {
            var old = basis.Business;
            var prepared = CandidateBusinessSaveCodec.Prepare(new CandidateBusinessInput(old.PlayerId, old.Roster, old.Inventory,
                old.Progression, old.Rewards, next, old.RetainedRuns, old.RetainedRollbacks, old.Format),
                old.Format == CandidateBusinessFormat.CandidateV1 ? SavePurpose.CandidateValidation : SavePurpose.PlayerSave, budget);
            return prepared.IsAccepted ? CandidateApplicationBuildResult.Success(prepared.Value, result, reserved) :
                observation.Reject(prepared.RejectionCode, prepared.FieldPath, limitReason: prepared.LimitReason,
                    required: prepared.RequiredAtLeast, allowed: prepared.Allowed);
        }

        private static bool SameContext(PreparedRuleContext left, PreparedRuleContext right)
        { return RuleContextChecks.Same(left, right); }
    }
}
