using System;
using System.Collections.Generic;
using System.Numerics;
using FlowPuzzle.Core;
using static FightMatch.Core.CandidateBattleReportFingerprint;
using static FightMatch.Core.CandidateRewardRejectionCode;
using static FightMatch.Core.RewardChecks;

namespace FightMatch.Core
{
    public static class CandidateBaseRewards
    {
        public static CandidateRewardDefinitionResult PrepareDefinition(CandidateRewardDefinitionInput input, ExactMathBudget budget)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            var c = new RewardChecks(budget);
            if (!c.Context(input.Context) || !c.Text(input.RewardDefinitionId, "RewardDefinitionId") || !c.Text(input.Version, "Version") ||
                !c.Text(input.LevelId, "LevelId") || !c.Text(input.LevelVersion, "LevelVersion") ||
                !c.Number(input.BaseExperience, "BaseExperience") || !c.Number(input.OverlevelGrace, "OverlevelGrace") ||
                !c.Rational(input.DamageWeight, "DamageWeight") || !c.Rational(input.TakenWeight, "TakenWeight") ||
                !c.Rational(input.CurveBase, "CurveBase") || !c.Rational(input.CurveLog, "CurveLog") ||
                !c.Rational(input.ReferenceHpDivisor, "ReferenceHpDivisor", true) || !c.Rational(input.LevelPenaltyBase, "LevelPenaltyBase") ||
                !c.Need(budget.Compare(input.LevelPenaltyBase.Numerator, input.LevelPenaltyBase.Denominator) <= 0, InvalidValue, "LevelPenaltyBase") ||
                !c.Need(input.ZeroContributionPolicy == CandidateZeroContributionPolicy.CurveIntercept,
                    input.ZeroContributionPolicy == CandidateZeroContributionPolicy.Unspecified ? MissingField : UnsupportedBinding, "ZeroContributionPolicy") ||
                !c.Need(input.DropMode == CandidateRewardDropMode.FixedOrdinaryMaterials,
                    input.DropMode == CandidateRewardDropMode.Unspecified ? MissingField : UnsupportedBinding, "DropMode") ||
                !c.Need(input.RequiredFeatures != null, MissingField, "RequiredFeatures") ||
                !c.Need(input.RequiredFeatures.Count == 0, UnsupportedBinding, "RequiredFeatures") ||
                !c.Need(input.Materials != null, MissingField, "Materials")) return new CandidateRewardDefinitionResult(c);
            var ids = new HashSet<string>(StringComparer.Ordinal);
            for (var i = 0; i < input.Materials.Count; i++)
            {
                var row = input.Materials[i]; var path = $"Materials[{i}]";
                if (!c.Need(row != null, MissingField, path) || !c.Text(row.ItemId, path + ".ItemId") ||
                    !c.Number(row.Amount, path + ".Amount") || !c.Need(ids.Add(row.ItemId), InvalidValue, path + ".ItemId"))
                    return new CandidateRewardDefinitionResult(c);
            }
            return new CandidateRewardDefinitionResult(new CandidateRewardDefinition(input));
        }

        public static CandidateRewardResult CreateCandidate(string playerId, ExactMathBudget budget)
        {
            if (playerId == null) throw new ArgumentNullException(nameof(playerId));
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            var c = new RewardChecks(budget);
            if (!c.Text(playerId, "PlayerId") || !c.Number(BigInteger.One, "StateRevision", true)) return new CandidateRewardResult(c);
            return new CandidateRewardResult(new CandidateRewardState(playerId, BigInteger.One,
                Array.Empty<CandidateFixedBaseReward>()), CandidateRewardOutcome.None);
        }

        public static CandidateRewardResult FindByAttempt(CandidateRewardState state, string attemptId, ExactMathBudget budget)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (attemptId == null) throw new ArgumentNullException(nameof(attemptId));
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            var c = new RewardChecks(budget);
            if (!c.Text(attemptId, "AttemptId") || !StateNumbers(state, c)) return new CandidateRewardResult(c);
            foreach (var reward in state.BaseRewards)
                if (Same(reward.AttemptId, attemptId)) return new CandidateRewardResult(state, CandidateRewardOutcome.AlreadyIncluded, reward);
            return new CandidateRewardResult(state, CandidateRewardOutcome.NotFound);
        }

        public static CandidateRewardResult FixNormal(CandidateRewardState state, CandidateFinalAttemptReport report,
            CandidateRewardDefinition definition, CandidateProgressionEndReceipt ending, CandidateRewardFixRequest request,
            ExactEvaluationScope scope)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (report == null) throw new ArgumentNullException(nameof(report));
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            if (ending == null) throw new ArgumentNullException(nameof(ending));
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (scope == null) throw new ArgumentNullException(nameof(scope));
            var c = new RewardChecks(scope.Budget.Math); var math = c.Math;
            if (!StateNumbers(state, c) || !DefinitionNumbers(definition, c) || !EndNumbers(ending, c) ||
                !c.Text(request.PlayerId, "PlayerId") || !c.Text(request.AttemptId, "AttemptId") ||
                !c.Text(request.ChallengeId, "ChallengeId") || !c.Text(request.EntryBaselineId, "EntryBaselineId") ||
                !c.Text(request.SettlementId, "SettlementId") || !c.Text(request.FinalReportFingerprint, "FinalReportFingerprint") ||
                !c.Number(request.ExpectedStateRevision, "ExpectedStateRevision", true) ||
                !c.Need(Same(state.PlayerId, request.PlayerId), InconsistentBinding, "PlayerId")) return new CandidateRewardResult(c);
            // Business identity precedes the expected revision and scoring. Rechecks never re-score an old result.
            foreach (var old in state.BaseRewards)
            {
                if (!Same(old.AttemptId, request.AttemptId) && !Same(old.SettlementId, request.SettlementId)) continue;
                if (!c.Need(RequestMatches(old.Report, old.Ending, request) && Same(old.Report.Fingerprint, report.Fingerprint) &&
                    SameDefinition(old.Definition, definition, math) && SameEnd(old.Ending, ending, math), RewardConflict, "BaseRewards"))
                    return new CandidateRewardResult(c);
                var checker = new RewardReportChecks(c);
                if (!checker.Report(report) || !c.Need(Equal(old.Report, report, math), RewardConflict, "Report"))
                { c.Need(false, RewardConflict, "Report"); return new CandidateRewardResult(c); }
                return new CandidateRewardResult(state, CandidateRewardOutcome.AlreadyIncluded, old);
            }
            if (!c.Need(state.StateRevision == request.ExpectedStateRevision.Value, StaleContext, "ExpectedStateRevision")) return new CandidateRewardResult(c);
            var check = new RewardReportChecks(c);
            if (!check.Report(report) || !c.Need(RequestMatches(report, ending, request), InconsistentBinding, "Request") ||
                !c.Need(Same(definition.LevelId, report.Level.LevelId) && Same(definition.LevelVersion, report.Level.LevelVersion) &&
                    Equal(definition.Context, report.Baseline.Entry.Context, math), InconsistentBinding, "Definition") ||
                !MatchesEnd(report, ending, c)) return new CandidateRewardResult(c);
            var revision = math.Add(state.StateRevision, BigInteger.One);
            var entry = report.Baseline.Entry;
            var experience = new List<CandidateRewardExperience>();
            foreach (var member in entry.ReadyParticipants)
            {
                var key = BattleCombatantKey.ForParticipant(entry.AttemptId, member.CharacterId);
                BattleContributionTotals totals = null;
                foreach (var row in report.FinalSnapshot.Contributions) if (row.CombatantKey.Equals(key)) totals = row;
                var contribution = totals.EffectiveDamageDealtHp.Multiply(definition.DamageWeight, math)
                    .Add(totals.EffectiveDamageTakenHp.Multiply(definition.TakenWeight, math), math);
                var reference = report.WholeLevelInitialEnemyHp.Divide(definition.ReferenceHpDivisor, math);
                var exponent = math.Subtract(math.Subtract(member.Level, entry.Level.RecommendedLevel), definition.OverlevelGrace);
                var multiplier = ExactRational.Create(1, 1, math); var factor = definition.LevelPenaltyBase;
                while (exponent.Sign > 0)
                {
                    exponent = math.DivRem(exponent, 2, out var remainder);
                    if (!remainder.IsZero) multiplier = multiplier.Multiply(factor, math);
                    if (!exponent.IsZero) factor = factor.Multiply(factor, math);
                }
                var score = ExactScoreCalculator.Evaluate(definition.BaseExperience, multiplier, definition.CurveBase,
                    definition.CurveLog, contribution, reference, scope);
                experience.Add(new CandidateRewardExperience(member, totals, contribution, reference, multiplier, score));
            }
            var reward = new CandidateFixedBaseReward(report, definition, ending, experience);
            var next = new List<CandidateFixedBaseReward>(state.BaseRewards) { reward };
            return new CandidateRewardResult(new CandidateRewardState(state.PlayerId, revision, next), CandidateRewardOutcome.Fixed, reward);
        }

        private static bool RequestMatches(CandidateFinalAttemptReport report, CandidateProgressionEndReceipt end, CandidateRewardFixRequest request)
        {
            return Same(report.PlayerId, request.PlayerId) && Same(report.AttemptId, request.AttemptId) &&
                Same(report.ChallengeId, request.ChallengeId) && Same(report.EntryBaselineId, request.EntryBaselineId) &&
                Same(end.SettlementId, request.SettlementId) && Same(report.Fingerprint, request.FinalReportFingerprint);
        }
        private static bool MatchesEnd(CandidateFinalAttemptReport report, CandidateProgressionEndReceipt end, RewardChecks c)
        {
            var b = end.Begin; var entry = report.Baseline.Entry;
            if (!c.Need(b.Participants.Count == entry.ReadyParticipants.Count, InconsistentBinding, "Ending.Begin.Participants")) return false;
            for (var i = 0; i < b.Participants.Count; i++)
            {
                var p = b.Participants[i]; var m = entry.ReadyParticipants[i];
                if (!c.Need(Same(p.CharacterId, m.CharacterId) && Same(p.ClassId, m.ClassId) &&
                    p.ClassKind == m.ClassKind && p.OriginalSlot == m.OriginalSlot, InconsistentBinding, "Ending.Begin")) return false;
            }
            return c.Need(end.Kind == CandidateProgressionEndKind.NormalVictory && end.NewAttemptId == null, UnsupportedBinding, "Ending.Kind") &&
                c.Text(end.EndReceiptId, "Ending.EndReceiptId") && c.Text(end.SettlementId, "Ending.SettlementId") &&
                c.Need(Same(b.PlayerId, entry.PlayerId) && Same(b.AttemptId, entry.AttemptId) && Same(b.ChallengeId, entry.ChallengeId) &&
                    Same(b.EntryBaselineId, entry.EntryBaselineId) && Same(end.FinalReportFingerprint, report.Fingerprint) &&
                    Same(b.Level.LevelId, entry.Level.LevelId) && Same(b.Level.LevelVersion, entry.Level.LevelVersion) &&
                    Equal(b.Context, entry.Context, c.Math), InconsistentBinding, "Ending.Begin");
        }
        private static bool SameEnd(CandidateProgressionEndReceipt a, CandidateProgressionEndReceipt b, ExactMathBudget math)
        {
            var x = a.Begin; var y = b.Begin;
            if (x.Participants.Count != y.Participants.Count || x.FormationRevision != y.FormationRevision) return false;
            for (var i = 0; i < x.Participants.Count; i++)
            {
                var p = x.Participants[i]; var q = y.Participants[i];
                if (!Same(p.CharacterId, q.CharacterId) || !Same(p.ClassId, q.ClassId) || p.ClassKind != q.ClassKind ||
                    p.CharacterRevision != q.CharacterRevision || p.OriginalSlot != q.OriginalSlot) return false;
            }
            return Same(a.EndReceiptId, b.EndReceiptId) && a.Kind == b.Kind && Same(a.SettlementId, b.SettlementId) &&
                Same(a.FinalReportFingerprint, b.FinalReportFingerprint) && Same(a.NewAttemptId, b.NewAttemptId) && a.IsFirstClear == b.IsFirstClear &&
                Same(x.PlayerId, y.PlayerId) && Same(x.AttemptId, y.AttemptId) && Same(x.ChallengeId, y.ChallengeId) &&
                Same(x.EntryBaselineId, y.EntryBaselineId) && Equal(x.Context, y.Context, math) &&
                Same(x.Level.LevelId, y.Level.LevelId) && Same(x.Level.LevelVersion, y.Level.LevelVersion) &&
                Same(x.Level.UnlockRuleId, y.Level.UnlockRuleId) && x.Level.EntryKind == y.Level.EntryKind && x.Level.UnlockKind == y.Level.UnlockKind &&
                Same(x.Level.UnlockAfterLevelId, y.Level.UnlockAfterLevelId) && Equal(x.Level.RequiredFeatures, y.Level.RequiredFeatures, math);
        }
        private static bool SameDefinition(CandidateRewardDefinition a, CandidateRewardDefinition b, ExactMathBudget math)
        {
            if (!Same(a.RewardDefinitionId, b.RewardDefinitionId) || !Same(a.Version, b.Version) || !Same(a.LevelId, b.LevelId) ||
                !Same(a.LevelVersion, b.LevelVersion) || !Equal(a.Context, b.Context, math) || a.BaseExperience != b.BaseExperience ||
                a.OverlevelGrace != b.OverlevelGrace || a.ZeroContributionPolicy != b.ZeroContributionPolicy || a.DropMode != b.DropMode ||
                !Equal(a.RequiredFeatures, b.RequiredFeatures, math) || a.Materials.Count != b.Materials.Count) return false;
            var x = new[] { a.DamageWeight, a.TakenWeight, a.CurveBase, a.CurveLog, a.ReferenceHpDivisor, a.LevelPenaltyBase };
            var y = new[] { b.DamageWeight, b.TakenWeight, b.CurveBase, b.CurveLog, b.ReferenceHpDivisor, b.LevelPenaltyBase };
            if (!Equal(x, y, math)) return false;
            for (var i = 0; i < a.Materials.Count; i++) if (!Same(a.Materials[i].ItemId, b.Materials[i].ItemId) || a.Materials[i].Amount != b.Materials[i].Amount) return false;
            return true;
        }
        private static bool DefinitionNumbers(CandidateRewardDefinition d, RewardChecks c)
        {
            Check(d.Context, c.Math);
            if (!c.Need(d.ZeroContributionPolicy == CandidateZeroContributionPolicy.CurveIntercept &&
                d.DropMode == CandidateRewardDropMode.FixedOrdinaryMaterials && d.RequiredFeatures.Count == 0, UnsupportedBinding, "Definition.Coverage")) return false;
            if (!c.Number(d.BaseExperience, "Definition.BaseExperience") || !c.Number(d.OverlevelGrace, "Definition.OverlevelGrace") ||
                !c.Rational(d.DamageWeight, "Definition.DamageWeight") || !c.Rational(d.TakenWeight, "Definition.TakenWeight") ||
                !c.Rational(d.CurveBase, "Definition.CurveBase") || !c.Rational(d.CurveLog, "Definition.CurveLog") ||
                !c.Rational(d.ReferenceHpDivisor, "Definition.ReferenceHpDivisor", true) || !c.Rational(d.LevelPenaltyBase, "Definition.LevelPenaltyBase")) return false;
            foreach (var row in d.Materials) if (!c.Number(row.Amount, "Definition.Materials.Amount")) return false;
            return true;
        }
        private static bool EndNumbers(CandidateProgressionEndReceipt end, RewardChecks c)
        {
            if (!c.Need(end.Begin?.Context != null && end.Begin.Level != null && end.Begin.Participants.Count > 0, MissingField, "Ending.Begin")) return false;
            Check(end.Begin.Context, c.Math);
            foreach (var participant in end.Begin.Participants)
                if (!c.Need(participant != null, MissingField, "Ending.Participant") ||
                    !c.Number(participant.CharacterRevision, "Ending.Participant.CharacterRevision", true) ||
                    !c.Number(participant.OriginalSlot, "Ending.Participant.OriginalSlot")) return false;
            return true;
        }
        private static bool StateNumbers(CandidateRewardState state, RewardChecks c)
        {
            if (!c.Number(state.StateRevision, "StateRevision", true)) return false;
            foreach (var reward in state.BaseRewards)
            {
                if (!DefinitionNumbers(reward.Definition, c) || !EndNumbers(reward.Ending, c)) return false;
                Check(reward.Report, c.Math);
                foreach (var record in reward.Report.Operations)
                { Check(record.StageDecision, c.Math); if (record.EnemyPhase != null) Check(record.EnemyPhase, c.Math);
                    foreach (var fact in record.OrderedFacts) c.Math.CheckInteger(fact.Index); }
                foreach (var row in reward.Experience)
                {
                    c.Math.CheckInteger(row.OriginalSlot); c.Math.CheckInteger(row.EntryLevel); c.Math.CheckInteger(row.Score.TermsUsed);
                    c.Math.CheckInteger(row.Amount);
                    Check(new[] { row.Dealt, row.Taken, row.Contribution, row.Reference, row.LevelMultiplier, row.Score.LowerBound, row.Score.UpperBound }, c.Math);
                }
            }
            return true;
        }
    }

    // Inspect retained coverage and arithmetic. No battle stage is evaluated and no random word is drawn here.
    internal sealed class RewardReportChecks
    {
        private readonly RewardChecks c;
        private readonly ExactMathBudget math;
        internal RewardReportChecks(RewardChecks checks) { c = checks; math = checks.Math; }
        private bool Need(bool value, string path) { return c.Need(value, IncompleteReport, path); }
        private bool Eq(object a, object b, string path) { return Need(Equal(a, b, math), path); }
        private bool Known(object value, string path)
        { try { Check(value, math); return true; } catch (ArgumentException) { return c.Need(false, UnsupportedBinding, path); } }

        internal bool Report(CandidateFinalAttemptReport report)
        {
            if (!c.Need(report.Binding?.Start?.Baseline?.Entry != null && report.Baseline?.Entry != null &&
                report.InitialSnapshot != null && report.FinalSnapshot != null, MissingField, "Report.Binding")) return false;
            var binding = report.Binding; var baseline = report.Baseline; var entry = baseline.Entry;
            if (!c.Need(ReferenceEquals(baseline, binding.Start.Baseline) && ReferenceEquals(report.InitialSnapshot, binding.Start.Snapshot),
                InconsistentBinding, "Report.InitialSnapshot")) return false;
            if (!c.Need(report.Outcome == CandidateBattleOutcome.NormalVictory && entry.CarryMode == EntryCarryMode.Empty &&
                entry.RequiredFeatures.Count == 0 && entry.Members.Count >= 1 &&
                entry.Members.Count <= (entry.Context is PreparedPublishedRuleContext ? 3 : 1) &&
                entry.ReadyParticipants.Count == entry.Members.Count &&
                report.ConsumptionCoverage == CandidateConsumptionCoverage.EmptyCarryNoUse, UnsupportedBinding, "Report.Coverage")) return false;
            if (!Need(entry.Level.Faces.Count > 0 && entry.Level.RecommendedLevel.Sign > 0, "Report.Baseline.Entry")) return false;
            var characters = new HashSet<string>(StringComparer.Ordinal); var classes = new HashSet<string>(StringComparer.Ordinal);
            var lastSlot = -1;
            for (var i = 0; i < entry.Members.Count; i++)
            {
                var member = entry.ReadyParticipants[i];
                if (!Need(member != null && ReferenceEquals(entry.Members[i], member) && member.Level.Sign > 0 &&
                    characters.Add(member.CharacterId) && classes.Add(member.ClassId) && member.OriginalSlot > lastSlot &&
                    member.OriginalSlot <= 2, "Report.Baseline.Entry")) return false;
                if (!c.Need(member.ClassKind == CharacterClassKind.Warrior && member.LearnedSkills.Count == 0 && member.IsReady &&
                    member.StatsOrigin == BaseStatsOrigin.ComputedBaseStats && member.Stats.Evasion.Numerator.IsZero,
                    UnsupportedBinding, "Report.Baseline.Participant") ||
                    !Eq(member.StatsContext, entry.Context, "Report.Baseline.Participant.Context")) return false;
                lastSlot = member.OriginalSlot;
            }
            var domains = new[] { binding.Battle, binding.BaseReward, binding.Bonus };
            if (!c.Text(binding.SourceCapabilityId, "Report.Binding.SourceCapabilityId") ||
                !c.Need(Same(binding.MappingId, CandidateRandomPreparer.SupportedMappingId), UnsupportedBinding, "Report.Binding.MappingId") ||
                !Need(baseline.RandomInitials != null, "Report.Baseline.RandomInitials")) return false;
            var streams = new[] { baseline.RandomInitials.Battle, baseline.RandomInitials.BaseReward, baseline.RandomInitials.Bonus };
            for (var i = 0; i < domains.Length; i++)
                if (!Need(domains[i]?.Initial != null && domains[i].Purpose == (CandidateRandomPurpose)i &&
                    ReferenceEquals(domains[i].Initial, streams[i]) && streams[i].WordsConsumed.IsZero, "Report.Binding.RandomDomains")) return false;
            var wholeHp = ExactRational.Create(0, 1, math);
            foreach (var face in entry.Level.Faces)
            {
                if (!Need(face.Pairs.Count > 0, "Report.Level.Faces.Pairs")) return false;
                foreach (var pair in face.Pairs)
                {
                    var enemy = pair.Enemy;
                    if (!c.Need(enemy.Behavior == EnemyBehavior.NormalStrike || enemy.Behavior == EnemyBehavior.ChargeHeavy,
                        UnsupportedBinding, "Report.Level.Enemy.Behavior")) return false;
                    if (!c.Need(enemy.Stats.Evasion.Numerator.IsZero, UnsupportedBinding, "Report.Level.Enemy.Evasion")) return false;
                    if (!c.Rational(enemy.Stats.MaxHp, "Report.Level.Enemy.MaxHp", true) || !Need(enemy.IntentCycle.Count > 0, "Report.Level.Enemy.IntentCycle")) return false;
                    foreach (var intent in enemy.IntentCycle)
                        if (!c.Need(intent.Targeting == EnemyTargeting.FirstLiving &&
                            (intent.Kind == EnemyIntentKind.Charge ? intent.DamageKind == null && intent.DamageCoefficient == null :
                            intent.Kind == EnemyIntentKind.Strike && intent.DamageKind == EntryDamageKind.Physical && intent.DamageCoefficient != null),
                            UnsupportedBinding, "Report.Level.Enemy.IntentCycle")) return false;
                    wholeHp = wholeHp.Add(enemy.Stats.MaxHp, math);
                }
            }
            if (!Known(baseline, "Report.Baseline") || !Snapshot(report.InitialSnapshot, baseline, "Report.InitialSnapshot") ||
                !Eq(report.InitialSnapshot, new BattleSnapshot(baseline, ExactRational.Create(0, 1, math)), "Report.InitialSnapshot") ||
                !Need(report.Operations.Count > 0, "Report.Operations") || !Eq(wholeHp, report.WholeLevelInitialEnemyHp, "Report.WholeLevelInitialEnemyHp")) return false;
            var previous = report.InitialSnapshot; var ids = new HashSet<string>(StringComparer.Ordinal);
            var flat = new List<CandidateContributionSegment>();
            for (var i = 0; i < report.Operations.Count; i++)
            {
                var record = report.Operations[i]; var path = $"Report.Operations[{i}]";
                if (!Need(record?.StageDecision?.Source != null && record.BeforeSnapshot != null && record.AfterSnapshot != null, path) ||
                    !Need(previous.Phase != BattlePhase.WonPendingSettlement && previous.Phase != BattlePhase.Closed, path + ".BeforeSnapshot.Phase") ||
                    !Snapshot(record.BeforeSnapshot, baseline, path + ".BeforeSnapshot") || !Snapshot(record.AfterSnapshot, baseline, path + ".AfterSnapshot") ||
                    !Need(record.BeforeSnapshot.SceneRevision >= previous.SceneRevision, path + ".BeforeSnapshot.SceneRevision") ||
                    !Eq(previous, record.BeforeSnapshot, path + ".BeforeSnapshot", true) ||
                    !Need(!string.IsNullOrWhiteSpace(record.OperationId) && ids.Add(record.OperationId), path + ".OperationId") || !Record(binding, record, path)) return false;
                flat.AddRange(record.ContributionSegments); previous = record.AfterSnapshot;
            }
            var last = report.Operations[report.Operations.Count - 1];
            if (!Snapshot(report.FinalSnapshot, baseline, "Report.FinalSnapshot") || !Eq(previous, report.FinalSnapshot, "Report.FinalSnapshot") ||
                !Need(report.FinalSnapshot.Phase == BattlePhase.WonPendingSettlement &&
                    report.FinalSnapshot.CurrentFaceIndex == entry.Level.Faces.Count - 1 && CompleteBoard(report.FinalSnapshot.Board, report.FinalSnapshot.Enemies), "Report.FinalSnapshot.Terminal") ||
                !Need(report.EndedAtUnixMilliseconds == last.OccurredAtUnixMilliseconds && Same(report.TerminalOperationId, last.OperationId), "Report.TerminalOperation") ||
                !Known(report.Contributions, "Report.Contributions") || !Eq(flat, report.Contributions, "Report.Contributions") || !Known(report, "Report")) return false;
            return Need(Same(report.Fingerprint, Compute(report, math)), "Report.Fingerprint");
        }

        private bool Record(CandidateRandomBinding binding, CandidateBattleOperationRecord r, string path)
        {
            var before = r.BeforeSnapshot; var after = r.AfterSnapshot; var stage = r.StageDecision; var source = r.Request;
            var entry = before.Baseline.Entry;
            if (!Need(ReferenceEquals(stage.BeforeSnapshot, before) && r.OccurredAtUnixMilliseconds.Sign >= 0 &&
                Same(source.PlayerId, entry.PlayerId) && Same(source.AttemptId, entry.AttemptId) &&
                source.ExpectedSceneRevision == before.SceneRevision && source.Pair != null &&
                Pair(before.Board.Face, entry.AttemptId, source.Pair) != null, path + ".Source") ||
                !c.Need(r.ConsumptionCoverage == CandidateConsumptionCoverage.EmptyCarryNoUse, UnsupportedBinding, path + ".ConsumptionCoverage") ||
                !c.Need(r.Kind == CandidateBattleOperationKind.Attack || r.Kind == CandidateBattleOperationKind.Link, UnsupportedBinding, path + ".Kind")) return false;
            var expectedSegments = new List<CandidateContributionSegment>(); var factIndex = 0;
            IReadOnlyList<BattleMemberState> members = before.Members;
            IReadOnlyList<BattleEnemyState> enemies = before.Enemies;
            IReadOnlyList<BattleContributionTotals> totals = before.Contributions;
            var random = before.Random; var increment = BigInteger.Zero;
            if (r.Kind == CandidateBattleOperationKind.Attack)
            {
                var direct = r.DirectAttack; var phase = r.EnemyPhase;
                if (!Need(before.Phase == BattlePhase.AwaitAction &&
                    r.Conditions != null && r.Conditions.PreferenceRevision.Sign > 0 && direct?.Action != null && phase != null &&
                    ReferenceEquals(direct.Binding, binding) && ReferenceEquals(direct.BeforeSnapshot, before) &&
                    ReferenceEquals(phase.DirectAttack, direct) && source.Kind == CandidateStageOperation.AfterAttack && direct.DamageFacts.Count == 1,
                    path + ".Attack")) return false;
                var action = direct.Action; var hit = direct.DamageFacts[0];
                if (!Snapshot(new BattleSnapshot(before.Baseline, before.SceneRevision, before.EffectiveActionsCompleted,
                    before.EnemyPhasesCompleted, before.CurrentFaceIndex, before.Phase, before.Board, direct.Members,
                    direct.Enemies, direct.Random, direct.Contributions), before.Baseline, path + ".DirectAttack") ||
                    !Snapshot(new BattleSnapshot(before.Baseline, before.SceneRevision, before.EffectiveActionsCompleted,
                    before.EnemyPhasesCompleted, before.CurrentFaceIndex, before.Phase, before.Board, phase.Members,
                    phase.Enemies, phase.Random, phase.Contributions), before.Baseline, path + ".EnemyPhase")) return false;
                if (!Need(hit?.Crit != null, path + ".DirectAttack.DamageFacts") ||
                    !Eq(source, new CandidateStageSource(CandidateStageOperation.AfterAttack, action.PlayerId, action.AttemptId,
                        action.OperationId, action.ExpectedSceneRevision, action.Actor, action.Pair, action.Route), path + ".Request") ||
                    !Need(action.ActionOrdinal == math.Add(before.EffectiveActionsCompleted, 1), path + ".ActionOrdinal")) return false;
                var target = Enemy(before.Enemies, hit.Target); BattleMemberState actor = null;
                foreach (var member in before.Members) if (member.CombatantKey.Equals(hit.Actor)) actor = member;
                if (!Need(target != null && target.Hp.Numerator.Sign > 0 && actor != null && actor.Hp.Numerator.Sign > 0 &&
                    actor.CombatantKey.Equals(source.Actor) && source.Pair.Equals(target.PairKey) && hit.Pair.Equals(target.PairKey) &&
                    ReferenceEquals(hit.Baseline, before.Baseline) && Same(hit.PlayerId, entry.PlayerId) && Same(hit.AttemptId, entry.AttemptId) &&
                    Same(hit.FaceId, before.Board.Face.FaceId) && Same(hit.OperationId, r.OperationId) && hit.SceneRevision == before.SceneRevision &&
                    hit.ActionOrdinal == action.ActionOrdinal, path + ".DirectAttack.Source") ||
                    !Eq(hit.HpBefore, target.Hp, path + ".DirectAttack.HpBefore") ||
                    !Damage(hit.HpBefore, hit.HpAfter, hit.HpLoss, hit.Overflow, hit.RoundedDamage, hit.BlockPrevented, hit.ShieldAbsorbed, path + ".DirectAttack.Damage") ||
                    !Ordered(r, factIndex, CandidateBattleFactKind.DirectAttack, hit, path)) return false;
                expectedSegments.Add(new CandidateContributionSegment(r.OperationId, before.SceneRevision, before.Board.Face.FaceId,
                    CandidateBattleFactKind.DirectAttack, hit.EffectIndex, hit.Actor, hit.Target, hit.Actor,
                    CandidateContributionKind.DamageDealtHp, hit.HpLoss, factIndex++));
                var directEnemies = new List<BattleEnemyState>();
                foreach (var enemy in before.Enemies) directEnemies.Add(enemy.CombatantKey.Equals(target.CombatantKey)
                    ? new BattleEnemyState(enemy.CombatantKey, enemy.PairKey, enemy.Enemy, hit.HpAfter, enemy.IntentCursor) : enemy);
                var directTotals = new List<BattleContributionTotals>();
                foreach (var row in before.Contributions) directTotals.Add(new BattleContributionTotals(row.CombatantKey,
                    row.CombatantKey.Equals(hit.Actor) ? row.EffectiveDamageDealtHp.Add(hit.HpLoss, math) : row.EffectiveDamageDealtHp,
                    row.EffectiveDamageTakenHp));
                if (!Eq(directEnemies, direct.Enemies, path + ".DirectAttack.Enemies") || !Eq(before.Members, direct.Members, path + ".DirectAttack.Members") ||
                    !Eq(directTotals, direct.Contributions, path + ".DirectAttack.Contributions") ||
                    !Need(direct.Random?.Stream != null && direct.Random.PrdStates.Count == before.Random.PrdStates.Count, path + ".DirectAttack.Random") ||
                    !Need(hit.Crit.Actor.Equals(hit.Actor) && hit.Crit.Target.Equals(hit.Target) &&
                        ReferenceEquals(hit.Crit.Parameters, actor.Member.Crit) && hit.Crit.OpportunityOrdinal == before.EffectiveActionsCompleted,
                        path + ".DirectAttack.Crit.Source") ||
                    !Eq(hit.Crit.StreamBefore, before.Random.Stream, path + ".DirectAttack.Crit.Before") ||
                    !Eq(hit.Crit.StreamAfter, direct.Random.Stream, path + ".DirectAttack.Crit.After")) return false;
                for (var i = 0; i < before.Random.PrdStates.Count; i++)
                {
                    var old = before.Random.PrdStates[i]; var next = direct.Random.PrdStates[i];
                    if (!Need(old.CombatantKey.Equals(next.CombatantKey) && ReferenceEquals(old.Crit, next.Crit) &&
                        (old.CombatantKey.Equals(hit.Actor) ? hit.Crit.FailureCountBefore == old.FailureCount &&
                        hit.Crit.FailureCountAfter == next.FailureCount : old.FailureCount == next.FailureCount),
                        path + ".DirectAttack.Crit.Prd")) return false;
                }
                var hp = new Dictionary<BattleCombatantKey, ExactRational>();
                var taken = new Dictionary<BattleCombatantKey, ExactRational>();
                foreach (var member in direct.Members) hp.Add(member.CombatantKey, member.Hp);
                foreach (var row in directTotals) taken.Add(row.CombatantKey, row.EffectiveDamageTakenHp);
                var active = new List<BattleEnemyState>();
                foreach (var enemy in directEnemies) if (enemy.Hp.Numerator.Sign > 0) active.Add(enemy);
                active.Sort((a, b) => a.StableOrder.CompareTo(b.StableOrder));
                if (!Need(phase.EnemyPhaseOrdinal == math.Add(before.EnemyPhasesCompleted, 1), path + ".EnemyPhase.Ordinal")) return false;
                var updated = new Dictionary<BattleCombatantKey, BigInteger>();
                for (var i = 0; i < phase.OrderedIntents.Count; i++)
                {
                    var intent = phase.OrderedIntents[i]; var front = Front(direct.Members, hp);
                    if (!Need(intent != null && i < active.Count && front != null, path + ".EnemyPhase.OrderedIntents")) return false;
                    var enemy = active[i]; var definition = enemy.Enemy.IntentCycle[(int)math.Remainder(enemy.IntentCursor, enemy.Enemy.IntentCycle.Count)];
                    if (!Need(ReferenceEquals(intent.Baseline, before.Baseline) && Same(intent.PlayerId, entry.PlayerId) &&
                        Same(intent.AttemptId, entry.AttemptId) && Same(intent.OperationId, r.OperationId) && Same(intent.FaceId, before.Board.Face.FaceId) &&
                        intent.SceneRevision == before.SceneRevision && intent.ActionOrdinal == action.ActionOrdinal && intent.EnemyPhaseOrdinal == phase.EnemyPhaseOrdinal &&
                        intent.SegmentIndex == i && enemy.CombatantKey.Equals(intent.EnemyKey) && enemy.PairKey.Equals(intent.Pair) &&
                        intent.StableOrder == enemy.StableOrder && intent.CursorBefore == enemy.IntentCursor &&
                        intent.CursorAfter == math.Add(enemy.IntentCursor, 1) && intent.IntentKind == definition.Kind, path + ".EnemyPhase.Source") ||
                        !Ordered(r, factIndex, CandidateBattleFactKind.EnemyIntent, intent, path)) return false;
                    updated.Add(enemy.CombatantKey, intent.CursorAfter);
                    if (intent.IntentKind == EnemyIntentKind.Strike)
                    {
                        var hitEnemy = intent.Damage;
                        if (!Need(hitEnemy != null && enemy.CombatantKey.Equals(hitEnemy.ActorEnemy) && front.CombatantKey.Equals(hitEnemy.TargetMember), path + ".EnemyPhase.Damage.Source") ||
                            !Eq(hitEnemy.HpBefore, hp[front.CombatantKey], path + ".EnemyPhase.Damage.HpBefore") ||
                            !Eq(hitEnemy.DamageCoefficient, definition.DamageCoefficient, path + ".EnemyPhase.Damage.Coefficient") ||
                            !Damage(hitEnemy.HpBefore, hitEnemy.HpAfter, hitEnemy.HpLoss, hitEnemy.Overflow, hitEnemy.RoundedDamage,
                                hitEnemy.BlockPrevented, hitEnemy.ShieldAbsorbed, path + ".EnemyPhase.Damage")) return false;
                        hp[front.CombatantKey] = hitEnemy.HpAfter;
                        taken[front.CombatantKey] = taken[front.CombatantKey].Add(hitEnemy.HpLoss, math);
                        expectedSegments.Add(new CandidateContributionSegment(r.OperationId, before.SceneRevision, before.Board.Face.FaceId,
                            CandidateBattleFactKind.EnemyIntent, intent.SegmentIndex, hitEnemy.ActorEnemy, hitEnemy.TargetMember,
                            front.CombatantKey, CandidateContributionKind.DamageTakenHp, hitEnemy.HpLoss, factIndex));
                    }
                    else if (!c.Need(intent.IntentKind == EnemyIntentKind.Charge && intent.Damage == null, UnsupportedBinding, path + ".EnemyPhase.IntentKind")) return false;
                    factIndex++;
                }
                if (!Need(Front(direct.Members, hp) == null || phase.OrderedIntents.Count == active.Count, path + ".EnemyPhase.Coverage")) return false;
                var finalEnemies = new List<BattleEnemyState>();
                foreach (var enemy in directEnemies) finalEnemies.Add(new BattleEnemyState(enemy.CombatantKey, enemy.PairKey, enemy.Enemy,
                    enemy.Hp, updated.TryGetValue(enemy.CombatantKey, out var cursor) ? cursor : enemy.IntentCursor));
                var finalMembers = new List<BattleMemberState>(); var finalTotals = new List<BattleContributionTotals>();
                foreach (var member in direct.Members) finalMembers.Add(new BattleMemberState(member.CombatantKey, member.Member, hp[member.CombatantKey]));
                foreach (var row in directTotals) finalTotals.Add(new BattleContributionTotals(row.CombatantKey, row.EffectiveDamageDealtHp, taken[row.CombatantKey]));
                if (!Eq(finalMembers, phase.Members, path + ".EnemyPhase.Members") ||
                    !Eq(finalEnemies, phase.Enemies, path + ".EnemyPhase.Enemies") ||
                    !Eq(finalTotals, phase.Contributions, path + ".EnemyPhase.Contributions") ||
                    !Eq(direct.Random, phase.Random, path + ".EnemyPhase.Random") ||
                    !Known(phase, path + ".EnemyPhase")) return false;
                members = phase.Members; enemies = phase.Enemies; totals = phase.Contributions; random = phase.Random; increment = BigInteger.One;
            }
            else if (!Need(before.Phase == BattlePhase.AwaitLinks && r.Conditions == null && r.DirectAttack == null && r.EnemyPhase == null &&
                source.Kind == CandidateStageOperation.CompleteLink && source.Actor == null, path + ".Link")) return false;
            if (!Known(r.ContributionSegments, path + ".ContributionSegments") || !Eq(expectedSegments, r.ContributionSegments, path + ".ContributionSegments") ||
                !Stage(r, members, enemies, ref factIndex, path) || !Need(factIndex == r.OrderedFacts.Count, path + ".OrderedFacts.Count") ||
                !Need(after.SceneRevision == math.Add(before.SceneRevision, 1) &&
                    after.EffectiveActionsCompleted == math.Add(before.EffectiveActionsCompleted, increment) &&
                    after.EnemyPhasesCompleted == math.Add(before.EnemyPhasesCompleted, increment), path + ".AfterSnapshot.Counters") ||
                !Eq(members, after.Members, path + ".AfterSnapshot.Members") || !Eq(totals, after.Contributions, path + ".AfterSnapshot.Contributions") ||
                !Eq(random, after.Random, path + ".AfterSnapshot.Random")) return false;
            if (stage.DidFlipFace)
            {
                var nextEnemies = new List<BattleEnemyState>();
                foreach (var pair in stage.NextFace.Pairs) nextEnemies.Add(new BattleEnemyState(
                    BattleCombatantKey.ForEnemy(entry.AttemptId, stage.NextFace.FaceId, pair.Enemy.EnemyInstanceKey),
                    BattlePairKey.Create(entry.AttemptId, stage.NextFace.FaceId, pair.PairId), pair.Enemy));
                enemies = nextEnemies;
            }
            return Eq(enemies, after.Enemies, path + ".AfterSnapshot.Enemies") && Known(r, path);
        }
        private bool Damage(ExactRational before, ExactRational after, ExactRational loss, ExactRational overflow,
            BigInteger rounded, ExactRational block, ExactRational shield, string path)
        {
            return c.Rational(before, path + ".Before", true) && c.Rational(after, path + ".After") &&
                c.Rational(loss, path + ".Loss") && c.Rational(overflow, path + ".Overflow") &&
                c.Rational(block, path + ".Block") && c.Rational(shield, path + ".Shield") && c.Number(rounded, path + ".Rounded") &&
                c.Need(block.Numerator.IsZero && shield.Numerator.IsZero, UnsupportedBinding, path + ".Coverage") &&
                Eq(before.Subtract(loss, math), after, path + ".HpConservation") &&
                Eq(loss.Add(overflow, math), ExactRational.Create(rounded, 1, math), path + ".DamageConservation") &&
                Need(overflow.Numerator.IsZero || after.Numerator.IsZero, path + ".Overflow");
        }
        private bool Ordered(CandidateBattleOperationRecord r, int index, CandidateBattleFactKind kind, object payload, string path)
        {
            if (!Need(index < r.OrderedFacts.Count && r.OrderedFacts[index] != null, path + ".OrderedFacts")) return false;
            var fact = r.OrderedFacts[index]; math.CheckInteger(fact.Index);
            return Need(fact.Index == index && fact.Kind == kind &&
                ReferenceEquals(fact.DirectAttack, kind == CandidateBattleFactKind.DirectAttack ? payload : null) &&
                ReferenceEquals(fact.EnemyIntent, kind == CandidateBattleFactKind.EnemyIntent ? payload : null) &&
                ReferenceEquals(fact.Stage, kind == CandidateBattleFactKind.Stage ? payload : null), path + ".OrderedFacts.Source");
        }
        private bool Stage(CandidateBattleOperationRecord r, IReadOnlyList<BattleMemberState> members,
            IReadOnlyList<BattleEnemyState> enemies, ref int index, string path)
        {
            var stage = r.StageDecision; var before = r.BeforeSnapshot; var source = r.Request; var after = r.AfterSnapshot;
            if (!Need(stage.FinalHp != null && stage.Board?.Face != null, path + ".StageDecision")) return false;
            var hpMembers = new List<CandidateHpInput>(); var hpEnemies = new List<CandidateHpInput>();
            foreach (var member in members) hpMembers.Add(new CandidateHpInput { CombatantKey = member.CombatantKey, Hp = member.Hp });
            foreach (var enemy in enemies) hpEnemies.Add(new CandidateHpInput { CombatantKey = enemy.CombatantKey, Hp = enemy.Hp });
            if (!Eq(new CandidateFinalHpValues(new CandidateFinalHpProjection { MemberHp = hpMembers, EnemyHp = hpEnemies }),
                stage.FinalHp, path + ".StageDecision.FinalHp")) return false;
            var locked = new List<BattleLockedRoute>(before.Board.LockedRoutes); var pending = new List<BattlePairKey>(before.Board.PendingLinks);
            var phaseFacts = 0; var faceFacts = 0; var routeFacts = 0;
            for (var i = 0; i < stage.OrderedFacts.Count; i++)
            {
                var fact = stage.OrderedFacts[i];
                if (!Need(fact != null && fact.SegmentIndex == i && Same(fact.OperationId, r.OperationId) &&
                    fact.SceneRevision == before.SceneRevision && Same(fact.FaceId, before.Board.Face.FaceId), path + ".StageDecision.Facts") ||
                    !Ordered(r, index++, CandidateBattleFactKind.Stage, fact, path)) return false;
                switch (fact.Kind)
                {
                    case CandidateStageFactKind.RouteLocked:
                        if (!Need(source.Pair.Equals(fact.Pair) && fact.NextFaceId == null && fact.Phase == null, path + ".StageDecision.RouteLocked")) return false;
                        routeFacts++; locked.Add(new BattleLockedRoute(fact.Pair, source.Route)); break;
                    case CandidateStageFactKind.TemporaryRouteRemoved:
                        if (!Need(r.Kind == CandidateBattleOperationKind.Attack && !r.DirectAttack.DamageFacts[0].DefeatedTarget &&
                            source.Pair.Equals(fact.Pair) && fact.NextFaceId == null && fact.Phase == null, path + ".StageDecision.TemporaryRoute")) return false;
                        routeFacts++; break;
                    case CandidateStageFactKind.PendingLinkAdded:
                        if (!Need(fact.Pair != null && fact.NextFaceId == null && fact.Phase == null && !pending.Contains(fact.Pair), path + ".StageDecision.PendingAdded")) return false;
                        pending.Add(fact.Pair); break;
                    case CandidateStageFactKind.PendingLinkRemoved:
                        if (!Need(r.Kind == CandidateBattleOperationKind.Link && source.Pair.Equals(fact.Pair) && pending.Remove(fact.Pair) &&
                            fact.NextFaceId == null && fact.Phase == null, path + ".StageDecision.PendingRemoved")) return false;
                        break;
                    case CandidateStageFactKind.FaceChanged:
                        if (!Need(stage.DidFlipFace && Same(fact.NextFaceId, stage.NextFace.FaceId) && fact.Pair == null && fact.Phase == null,
                            path + ".StageDecision.FaceChanged")) return false;
                        faceFacts++; break;
                    case CandidateStageFactKind.PhaseSelected:
                        if (!Need(i == stage.OrderedFacts.Count - 1 && fact.Phase == stage.NextPhase && fact.Pair == null && fact.NextFaceId == null,
                            path + ".StageDecision.PhaseSelected")) return false;
                        phaseFacts++; break;
                    default: return c.Need(false, UnsupportedBinding, path + ".StageDecision.FactKind");
                }
            }
            var oldBoard = new BattleBoardState(before.Board.Face, locked, pending);
            if (!Need(phaseFacts == 1 && routeFacts == 1 && faceFacts == (stage.DidFlipFace ? 1 : 0), path + ".StageDecision.Coverage") ||
                !Board(oldBoard, before.Baseline.Entry.AttemptId, path + ".StageDecision.OldBoard")) return false;
            foreach (var enemy in enemies)
            {
                var marked = pending.Contains(enemy.PairKey); foreach (var route in locked) if (route.PairKey.Equals(enemy.PairKey)) marked = true;
                if (!Need(marked == enemy.Hp.Numerator.IsZero, path + ".StageDecision.DeathCoverage")) return false;
            }
            var board = oldBoard;
            if (stage.DidFlipFace)
            {
                if (!Need(CompleteBoard(oldBoard, enemies) && stage.NextFaceIndex == before.CurrentFaceIndex + 1 &&
                    stage.NextFaceIndex < before.Baseline.Entry.Level.Faces.Count &&
                    ReferenceEquals(stage.NextFace, before.Baseline.Entry.Level.Faces[stage.NextFaceIndex]), path + ".StageDecision.NextFace")) return false;
                board = new BattleBoardState(stage.NextFace);
            }
            else if (!Need(stage.NextFaceIndex == before.CurrentFaceIndex, path + ".StageDecision.NextFaceIndex")) return false;
            return Need(after.CurrentFaceIndex == stage.NextFaceIndex && after.Phase == stage.NextPhase &&
                ReferenceEquals(stage.Board.Face, board.Face), path + ".AfterSnapshot.Stage") &&
                Eq(board, stage.Board, path + ".StageDecision.Board") && Eq(stage.Board, after.Board, path + ".AfterSnapshot.Board") && Known(stage, path + ".StageDecision");
        }

        private bool Eq(object a, object b, string path, bool ignoreRevision)
        { return Need(Equal(a, b, math, ignoreRevision), path); }
        private bool Snapshot(BattleSnapshot s, BattleEntryBaseline baseline, string path)
        {
            if (!Need(s?.Board?.Face != null && s.Random?.Stream != null && ReferenceEquals(s.Baseline, baseline), path)) return false;
            var entry = baseline.Entry;
            if (!Need(s.CurrentFaceIndex >= 0 && s.CurrentFaceIndex < entry.Level.Faces.Count &&
                ReferenceEquals(s.Board.Face, entry.Level.Faces[s.CurrentFaceIndex]), path + ".Face")) return false;
            if (!Need(s.SceneRevision.Sign > 0 && s.EffectiveActionsCompleted.Sign >= 0 && s.EnemyPhasesCompleted.Sign >= 0 &&
                s.Members.Count == entry.ReadyParticipants.Count && s.Contributions.Count == s.Members.Count &&
                s.Random.PrdStates.Count == s.Members.Count, path + ".Members")) return false;
            for (var i = 0; i < s.Members.Count; i++)
            {
                var member = entry.ReadyParticipants[i]; var key = BattleCombatantKey.ForParticipant(entry.AttemptId, member.CharacterId);
                var row = s.Members[i]; var total = s.Contributions[i]; var prd = s.Random.PrdStates[i];
                if (!Need(row != null && key.Equals(row.CombatantKey) && ReferenceEquals(row.Member, member) &&
                    total != null && key.Equals(total.CombatantKey), path + ".Members") ||
                    !c.Rational(row.Hp, path + ".Members.Hp") || !Need(row.Hp.Compare(member.Stats.MaxHp, math) <= 0, path + ".Members.Hp") ||
                    !c.Rational(total.EffectiveDamageDealtHp, path + ".Contributions.Dealt") ||
                    !c.Rational(total.EffectiveDamageTakenHp, path + ".Contributions.Taken") ||
                    !Need(prd != null && key.Equals(prd.CombatantKey) && ReferenceEquals(prd.Crit, member.Crit), path + ".Random")) return false;
            }
            if (!Need(s.Enemies.Count == s.Board.Face.Pairs.Count, path + ".Enemies")) return false;
            var seen = new HashSet<BattlePairKey>();
            foreach (var enemy in s.Enemies)
            {
                if (!Need(enemy?.PairKey != null && enemy.CombatantKey != null && seen.Add(enemy.PairKey), path + ".Enemies")) return false;
                var pair = Pair(s.Board.Face, entry.AttemptId, enemy.PairKey);
                if (!Need(pair != null && ReferenceEquals(pair.Enemy, enemy.Enemy) &&
                    BattleCombatantKey.ForEnemy(entry.AttemptId, s.Board.Face.FaceId, pair.Enemy.EnemyInstanceKey).Equals(enemy.CombatantKey), path + ".Enemies.Source") ||
                    !c.Rational(enemy.Hp, path + ".Enemies.Hp") || !Need(enemy.Hp.Compare(enemy.Enemy.Stats.MaxHp, math) <= 0 && enemy.IntentCursor.Sign >= 0, path + ".Enemies.Hp")) return false;
            }
            return Board(s.Board, entry.AttemptId, path + ".Board") && Known(s, path);
        }

        private bool Board(BattleBoardState board, string attempt, string path)
        {
            var seen = new HashSet<BattlePairKey>(); var paths = new List<FlowPathData>();
            var level = new FlowLevelData { width = board.Face.Width, height = board.Face.Height, pairs = new List<FlowPairData>() };
            foreach (var pair in board.Face.Pairs) level.pairs.Add(new FlowPairData { colorId = pair.GeometryColorId, endpointA = pair.EndpointA, endpointB = pair.EndpointB });
            foreach (var route in board.LockedRoutes)
            {
                if (!Need(route?.PairKey != null && seen.Add(route.PairKey), path + ".LockedRoutes")) return false;
                var pair = Pair(board.Face, attempt, route.PairKey);
                if (!Need(pair != null && new BattleRouteValidator().Validate(level, paths, Array.Empty<FlowPos>(), pair.GeometryColorId, route.Route).IsValid, path + ".LockedRoutes.Route")) return false;
                paths.Add(new FlowPathData { colorId = pair.GeometryColorId, cells = new List<FlowPos>(route.Route) });
            }
            foreach (var key in board.PendingLinks)
                if (!Need(key != null && seen.Add(key) && Pair(board.Face, attempt, key) != null, path + ".PendingLinks")) return false;
            return true;
        }
        private static PreparedPair Pair(PreparedFace face, string attempt, BattlePairKey key)
        {
            foreach (var pair in face.Pairs) if (BattlePairKey.Create(attempt, face.FaceId, pair.PairId).Equals(key)) return pair;
            return null;
        }
        private static BattleEnemyState Enemy(IReadOnlyList<BattleEnemyState> rows, BattleCombatantKey key)
        { foreach (var row in rows) if (row.CombatantKey.Equals(key)) return row; return null; }
        private static BattleMemberState Front(IReadOnlyList<BattleMemberState> members, Dictionary<BattleCombatantKey, ExactRational> hp)
        {
            BattleMemberState front = null;
            foreach (var member in members)
                if (hp[member.CombatantKey].Numerator.Sign > 0 && (front == null || member.OriginalSlot < front.OriginalSlot)) front = member;
            return front;
        }
        private static bool CompleteBoard(BattleBoardState board, IReadOnlyList<BattleEnemyState> enemies)
        {
            if (board.PendingLinks.Count != 0 || board.LockedRoutes.Count != board.Face.Pairs.Count) return false;
            foreach (var enemy in enemies) if (!enemy.Hp.Numerator.IsZero) return false;
            return true;
        }
    }
}
