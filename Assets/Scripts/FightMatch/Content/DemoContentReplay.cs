using System;
using System.Collections.Generic;
using FightMatch.Core;
using static FightMatch.Content.ContentChecks;

namespace FightMatch.Content
{
    public sealed class DemoContentReplayResult
    {
        public bool IsAccepted => Reward != null;
        public bool CommitEligible => false;
        public DemoPreparedContent Candidate { get; }
        public CandidateRandomBinding Binding { get; }
        public IReadOnlyList<byte> SeedBytes { get; }
        public CandidateBattleRun Run { get; }
        // Original immutable records include the exact executed requests, facts and all snapshots.
        public IReadOnlyList<CandidateBattleOperationRecord> ExecutedSteps => Run?.Records;
        public IReadOnlyList<CandidateReplayResult> RecordedReplays { get; }
        public CandidateFinalAttemptReport Report => Run?.FinalReport;
        public CandidateFixedBaseReward Reward { get; }
        public string RejectionCode { get; }
        public string FieldPath { get; }
        public int? FirstFailureStep { get; }
        internal DemoContentReplayResult(DemoPreparedContent candidate, CandidateRandomBinding binding, byte[] seed,
            CandidateBattleRun run, List<CandidateReplayResult> replays, CandidateFixedBaseReward reward,
            string code = null, string path = null, int? failedStep = null)
        { Candidate = candidate; Binding = binding; SeedBytes = seed == null ? null : new List<byte>(seed).AsReadOnly();
            Run = run; RecordedReplays = replays.AsReadOnly(); Reward = reward; RejectionCode = code; FieldPath = path; FirstFailureStep = failedStep; }
    }

    public static class DemoContentReplay
    {
        // Selection is a making-side policy over an immutable actual state; it cannot inject outcomes or streams.
        public static DemoContentReplayResult ReplayCandidate(DemoPreparedContent candidate, CandidateSeedMaterial seed,
            CandidateBattleConditions conditions, Func<BattleSnapshot, CandidateReplayStep> selectNext, int maxSteps,
            DemoContentJob job, ExactEvaluationScope scope, int maxRandomWords = 4096)
        {
            CandidateRandomBinding binding = null; CandidateBattleRun run = null; byte[] seedBytes = null;
            var replays = new List<CandidateReplayResult>(); var stepIndex = 0;
            try
            {
                Root(job, "Job"); job.Check(); Root(candidate, "Candidate"); candidate.Job.Check();
                Need(ReferenceEquals(job, candidate.Job), "StaleContext", "Candidate.Job");
                Root(seed, "Seed"); Root(seed.Bytes, "Seed.Bytes"); Root(conditions, "Conditions");
                Root(selectNext, "SelectNext"); Root(scope, "Scope");
                Need(maxSteps > 0 && maxRandomWords >= 0, "InvalidValue", "Budget");
                Need(conditions.PreferenceRevision.HasValue && conditions.ItemUseEnabled.HasValue, "MissingField", "Conditions");
                Need(conditions.ItemUseEnabled == false, "UnsupportedBinding", "Conditions.ItemUseEnabled");
                var frozenConditions = new CandidateBattleConditions { PreferenceRevision = conditions.PreferenceRevision, ItemUseEnabled = false };
                var math = scope.Budget.Math; var sampling = new RandomSamplingBudget(math, maxRandomWords);
                seedBytes = (byte[])seed.Bytes.Clone();
                var bound = CandidateRandomPreparer.Prepare(candidate.Entry, new CandidateSeedMaterial { Bytes = seedBytes,
                    SourceCapabilityId = seed.SourceCapabilityId, MappingId = seed.MappingId }, math);
                Need(bound.IsAccepted, bound.RejectionCode.ToString(), "Seed." + bound.FieldPath); binding = bound.Binding;
                var created = CandidateBattleOperations.CreateCandidate(binding, math);
                Need(created.IsAccepted, created.RejectionCode, created.FieldPath); run = created.Run;
                var initial = CandidateHistoryOperations.CreateCandidate(run, math);
                Need(initial.IsAccepted, initial.RejectionCode.ToString(), initial.FieldPath); var history = initial.Next;
                while (run.FinalReport == null)
                {
                    job.Check(); Need(stepIndex < maxSteps, "BudgetExceeded", "MaxSteps");
                    var step = selectNext(run.CurrentSnapshot); job.Check(); Root(step, "Steps[" + stepIndex + "]");
                    var isolation = CandidateReplayOperations.BuildIsolationInput(history, Isolation(candidate, history,
                        CandidateIsolationPurpose.CurrentPlan, frozenConditions), math);
                    Need(isolation.IsAccepted, isolation.RejectionCode, isolation.FieldPath);
                    var result = CandidateReplayOperations.EvaluatePlan(isolation.Input, new[] { step }, sampling);
                    Need(result.IsAccepted, result.RejectionCode, result.FieldPath);
                    var appended = CandidateHistoryOperations.Append(history, result.Run, "content-anchor:" + stepIndex, math);
                    Need(appended.IsAccepted, appended.RejectionCode.ToString(), appended.FieldPath);
                    history = appended.Next; run = result.Run;
                    var replayInput = CandidateReplayOperations.BuildIsolationInput(history, Isolation(candidate, history,
                        CandidateIsolationPurpose.RecordedOperation, null, result.ActualRecords[0].OperationId), math);
                    Need(replayInput.IsAccepted, replayInput.RejectionCode, replayInput.FieldPath);
                    var replay = CandidateReplayOperations.ReplayRecorded(replayInput.Input, sampling);
                    Need(replay.IsAccepted && replay.Matched == true, replay.RejectionCode ?? "ReplayDiverged",
                        replay.FirstDivergence?.FieldPath ?? replay.FieldPath);
                    replays.Add(replay); stepIndex++;
                }
                Need(run.CurrentSnapshot.Phase == BattlePhase.WonPendingSettlement && run.FinalReport.Outcome == CandidateBattleOutcome.NormalVictory,
                    "IncompleteReport", "FinalReport");
                var reward = Reward(candidate, run.FinalReport, scope);
                job.Check();
                return new DemoContentReplayResult(candidate, binding, seedBytes, run, replays, reward);
            }
            catch (ContentFailure ex)
            { return new DemoContentReplayResult(candidate, binding, seedBytes, run, replays, null, ex.Code, ex.Path, stepIndex); }
            catch (ExactMathLimitException)
            { return new DemoContentReplayResult(candidate, binding, seedBytes, run, replays, null, "BudgetExceeded", "EvaluationBudget", stepIndex); }
        }

        private static CandidateIsolationRequest Isolation(DemoPreparedContent c, CandidateBattleHistory h, CandidateIsolationPurpose purpose,
            CandidateBattleConditions conditions, string operation = null)
        { return new CandidateIsolationRequest { PlayerId = c.Entry.PlayerId, AttemptId = c.Entry.AttemptId,
            ExpectedSceneRevision = h.CurrentRun.CurrentSnapshot.SceneRevision, Purpose = purpose, RightsMode = CandidateIsolationRights.Empty,
            CurrentConditions = conditions, RecordedOperationId = operation }; }

        private static CandidateFixedBaseReward Reward(DemoPreparedContent c, CandidateFinalAttemptReport report, ExactEvaluationScope scope)
        {
            var e = c.Entry; var context = DemoContentCompiler.Context(e.Context); var math = scope.Budget.Math;
            // An isolated one-level domain solely to obtain the real candidate end receipt. No L1 -> L3 or player unlock claim.
            var definition = CandidateProgression.PrepareDefinition(new CandidateProgressionDefinitionInput { Context = context,
                Levels = new List<CandidateProgressionLevelInput> { new CandidateProgressionLevelInput { LevelId = e.Level.LevelId,
                    LevelVersion = e.Level.LevelVersion, UnlockRuleId = "candidate-isolated-only", EntryKind = CandidateProgressionEntryKind.Ordinary,
                    UnlockKind = CandidateProgressionUnlockKind.InitiallyOpen, UnlockAfterLevelId = null, RequiredFeatures = new List<string>() } } }, math);
            Need(definition.IsAccepted, definition.RejectionCode.ToString(), definition.FieldPath);
            var initial = CandidateProgression.CreateCandidate(definition.Definition, e.PlayerId, math);
            Need(initial.IsAccepted, initial.RejectionCode.ToString(), initial.FieldPath);
            var begin = CandidateProgression.BeginAttempt(initial.Next, new CandidateProgressionBeginIntent { PlayerId = e.PlayerId,
                LevelId = e.Level.LevelId, LevelVersion = e.Level.LevelVersion, Context = context, CharacterId = c.Character.CharacterId,
                ExpectedCharacterRevision = c.Character.StateRevision, ExpectedOriginalSlot = c.Character.OriginalSlot,
                ChallengeId = e.ChallengeId, AttemptId = e.AttemptId, EntryBaselineId = e.EntryBaselineId }, c.Character, initial.Next.StateRevision, math);
            Need(begin.IsAccepted, begin.RejectionCode.ToString(), begin.FieldPath);
            var settlement = "candidate-settlement:" + e.AttemptId;
            var ended = CandidateProgression.EndAttempt(begin.Next, new CandidateProgressionEndFacts { PlayerId = e.PlayerId,
                LevelId = e.Level.LevelId, LevelVersion = e.Level.LevelVersion, Context = context, ChallengeId = e.ChallengeId,
                AttemptId = e.AttemptId, EntryBaselineId = e.EntryBaselineId, EndReceiptId = "candidate-end:" + e.AttemptId,
                Kind = CandidateProgressionEndKind.NormalVictory, SettlementId = settlement, FinalReportFingerprint = report.Fingerprint,
                NewAttemptId = null }, begin.Next.StateRevision, math);
            Need(ended.IsAccepted, ended.RejectionCode.ToString(), ended.FieldPath);
            var empty = CandidateBaseRewards.CreateCandidate(e.PlayerId, math);
            Need(empty.IsAccepted, empty.RejectionCode.ToString(), empty.FieldPath);
            var fixedReward = CandidateBaseRewards.FixNormal(empty.Next, report, c.Reward, ended.EndReceipt,
                new CandidateRewardFixRequest { PlayerId = e.PlayerId, AttemptId = e.AttemptId, ChallengeId = e.ChallengeId,
                    EntryBaselineId = e.EntryBaselineId, SettlementId = settlement, FinalReportFingerprint = report.Fingerprint,
                    ExpectedStateRevision = empty.Next.StateRevision }, scope);
            Need(fixedReward.IsAccepted, fixedReward.RejectionCode.ToString(), fixedReward.FieldPath);
            return fixedReward.BaseReward;
        }
    }
}
