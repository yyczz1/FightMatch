using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using FightMatch.Core;

namespace FightMatch.Application
{
    internal static partial class CandidateLifecyclePreparation
    {
        private sealed class Refusal : Exception
        {
            internal readonly CandidateApplicationDiagnostic Diagnostic;
            internal Refusal(CandidateApplicationDiagnostic diagnostic) { Diagnostic = diagnostic; }
        }
        internal static CandidateLifecyclePrepareResult NewProfile(CandidateLifecycleContentInput content,
            CandidateApplicationInitializeInput recipe, SaveCodecBudget budget, bool published = false, bool roster = false)
        {
            return Run(budget, () =>
            {
                var frozen = Content(content, budget, published);
                Need(recipe != null, "MissingField", "Recipe");
                Text(recipe.CharacterId, "Recipe.CharacterId", budget); Text(recipe.ClassId, "Recipe.ClassId", budget);
                Number(recipe.InitialLevel, budget); Number(recipe.InitialExperience, budget); Number(recipe.OriginalSlot, budget);
                Need(recipe.ClassId == frozen.Growth.ClassId, "InconsistentBinding", "Recipe.ClassId");
                var copy = new CandidateApplicationInitializeInput { CharacterId = recipe.CharacterId, ClassId = recipe.ClassId,
                    InitialLevel = recipe.InitialLevel, InitialExperience = recipe.InitialExperience, OriginalSlot = recipe.OriginalSlot };
                return Freeze(new CandidateApplicationIntentInput { PlayerId = Guid.NewGuid().ToString("N"),
                    OperationId = Guid.NewGuid().ToString("N"), Kind = CandidateApplicationKind.InitializeProfile,
                    Context = Context(frozen.Context), FormatVersion = roster ? 3U : (uint?)null,
                    InitializeProfile = roster ? null : copy, RosterInitialize = roster ? RosterRecipe(copy) : null }, frozen, null, budget);
            });
        }
        internal static CandidateLifecyclePrepareResult Prepare(CandidateLifecycleDraft draft, SaveCodecBudget budget, bool published = false)
        {
            return Run(budget, () =>
            {
                Need(draft != null, "MissingField", "Draft");
                Need(draft.Kind == CandidateApplicationKind.EnterAttempt || draft.Kind == CandidateApplicationKind.ExitAttempt ||
                    draft.Kind == CandidateApplicationKind.RestartAttempt || draft.Kind == CandidateApplicationKind.AdvanceRecovery,
                    "UnsupportedBinding", "Kind");
                var content = Content(draft.Content, budget, published);
                var count = (draft.EnterAttempt == null ? 0 : 1) + (draft.ExitAttempt == null ? 0 : 1) +
                    (draft.RestartAttempt == null ? 0 : 1) + (draft.AdvanceRecovery == null ? 0 : 1);
                Need(count == 1, "InvalidValue", "Payload");
                Text(draft.PlayerId, "PlayerId", budget); Text(draft.ExpectedCommitId, "ExpectedCommitId", budget);
                var input = new CandidateApplicationIntentInput { PlayerId = draft.PlayerId, ExpectedCommitId = draft.ExpectedCommitId,
                    Kind = draft.Kind, Context = Context(content.Context) };
                if (draft.EnterAttempt != null)
                {
                    Need(draft.Kind == CandidateApplicationKind.EnterAttempt, "InvalidValue", "Kind");
                    var e = draft.EnterAttempt;
                    Text(e.LevelId, "Enter.LevelId", budget); Text(e.LevelVersion, "Enter.LevelVersion", budget);
                    Text(e.CharacterId, "Enter.CharacterId", budget); Number(e.ExpectedCharacterRevision, budget); Number(e.OriginalSlot, budget);
                    input.EnterAttempt = new CandidateApplicationEnterInput { LevelId = e.LevelId, LevelVersion = e.LevelVersion,
                        CharacterId = e.CharacterId, ExpectedCharacterRevision = e.ExpectedCharacterRevision, OriginalSlot = e.OriginalSlot };
                }
                if (draft.ExitAttempt != null)
                { Need(draft.Kind == CandidateApplicationKind.ExitAttempt, "InvalidValue", "Kind"); input.ExitAttempt = End(draft.ExitAttempt, budget); }
                if (draft.RestartAttempt != null)
                { Need(draft.Kind == CandidateApplicationKind.RestartAttempt, "InvalidValue", "Kind"); input.RestartAttempt = End(draft.RestartAttempt, budget); }
                if (draft.AdvanceRecovery != null)
                {
                    Need(draft.Kind == CandidateApplicationKind.AdvanceRecovery, "InvalidValue", "Kind");
                    var r = draft.AdvanceRecovery;
                    Text(r.CharacterId, "Recovery.CharacterId", budget); Text(r.RecoveryId, "Recovery.RecoveryId", budget);
                    Number(r.ExpectedCharacterRevision, budget);
                    Need(r.TimeSample != null, "MissingField", "Recovery.TimeSample");
                    input.AdvanceRecovery = new CandidateApplicationRecoveryInput { CharacterId = r.CharacterId,
                        RecoveryId = r.RecoveryId, ExpectedCharacterRevision = r.ExpectedCharacterRevision, TimeSample = Time(r.TimeSample, budget) };
                }
                var time = Time(draft.EndTimeSample, budget);
                Need(time == null || draft.Kind == CandidateApplicationKind.ExitAttempt, "InvalidValue", "EndTimeSample");
                input.OperationId = Guid.NewGuid().ToString("N");
                return Freeze(input, content, time, budget);
            });
        }
        internal static CandidateLifecyclePrepareResult Victory(CandidateApplicationSnapshot snapshot,
            CandidateLifecycleContentInput input, CandidateTimeSample time, SaveCodecBudget budget, bool published = false)
        {
            return Run(budget, () =>
            {
                var content = Content(input, budget, published);
                var run = snapshot?.Business.ActiveHistory?.CurrentRun;
                Need(run?.CurrentSnapshot.Phase == BattlePhase.WonPendingSettlement && run.FinalReport != null &&
                    snapshot.Continuation != null, "InvalidPhase", "SettleVictory");
                var report = run.FinalReport;
                var definition = content.Rewards.FirstOrDefault(x => x.LevelId == report.Level.LevelId && x.LevelVersion == report.Level.LevelVersion);
                Need(definition != null, "UnsupportedBinding", "RewardDefinition");
                return Freeze(new CandidateApplicationIntentInput { PlayerId = snapshot.Business.PlayerId,
                    ExpectedCommitId = snapshot.Header.CommitId, Kind = CandidateApplicationKind.SettleVictory,
                    OperationId = snapshot.Continuation.ReservedOperationId, Context = Context(content.Context),
                    SettleVictory = new CandidateApplicationVictoryInput { AttemptId = report.AttemptId, ChallengeId = report.ChallengeId,
                        EntryBaselineId = report.EntryBaselineId, FinalReportFingerprint = report.Fingerprint,
                        TerminalOperationId = snapshot.Continuation.ClosingOperationId,
                        RewardDefinitionId = definition.RewardDefinitionId, RewardDefinitionVersion = definition.Version } },
                    content, Time(time, budget), budget);
            });
        }
        private static CandidateLifecyclePrepareResult Run(SaveCodecBudget budget, Func<CandidateLifecyclePrepareResult> action)
        {
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            try { return action(); }
            catch (Refusal e) { return new CandidateLifecyclePrepareResult(null, e.Diagnostic); }
            catch (ExactMathLimitException e) { return new CandidateLifecyclePrepareResult(null, CandidateApplicationDiagnostic.From("Limit", "Prepare", e)); }
        }
        internal static CandidateLifecyclePrepareResult RestoreProfile(PreparedCandidateApplicationIntent original,
            CandidateLifecycleContentInput content, CandidateApplicationInitializeInput recipe, SaveCodecBudget budget)
        {
            return Run(budget, () =>
            {
                var frozen = Content(content, budget, true);
                var result = Freeze(new CandidateApplicationIntentInput { PlayerId = original.PlayerId, OperationId = original.OperationId,
                    Kind = CandidateApplicationKind.InitializeProfile, FormatVersion = original.FormatVersion, Context = Context(frozen.Context),
                    InitializeProfile = original.FormatVersion == 3 ? null : recipe,
                    RosterInitialize = original.FormatVersion == 3 ? RosterRecipe(recipe) : null }, frozen, null, budget);
                if (result.IsAccepted) Need(result.Request.Intent.CanonicalBytes.SequenceEqual(original.CanonicalBytes), "InconsistentCreateIntent", "CreateRecord.Intent");
                return result;
            });
        }
        private static CandidateRosterInitializeInput RosterRecipe(CandidateApplicationInitializeInput recipe)
        {
            Need(recipe.OriginalSlot.HasValue && recipe.OriginalSlot >= 0 && recipe.OriginalSlot <= 2, "InvalidValue", "Recipe.OriginalSlot");
            var slots = new string[3]; slots[recipe.OriginalSlot.Value] = recipe.CharacterId;
            return new CandidateRosterInitializeInput { Characters = Array.AsReadOnly(new[] { recipe }), Slots = Array.AsReadOnly(slots) };
        }
        internal static CandidateLifecyclePrepareResult Roster(CandidateApplicationIntentInput input,
            CandidateLifecycleContentInput content, SaveCodecBudget budget)
        {
            return Run(budget, () =>
            {
                var frozen = Content(content, budget, true);
                var prepared = CandidateApplicationProtocol.PrepareIntent(input, budget);
                if (!prepared.IsAccepted) return new CandidateLifecyclePrepareResult(null, CandidateApplicationDiagnostic.From(prepared, "Prepare"));
                Need((prepared.Value.FormatVersion == 3 || prepared.Value.FormatVersion == 4) && SameContext(prepared.Value.Context, frozen.Context, budget.Math),
                    "InconsistentBinding", "Roster.Context");
                var copy = new CandidateApplicationIntentInput { PlayerId = input.PlayerId, OperationId = input.OperationId,
                    ExpectedCommitId = input.ExpectedCommitId, Kind = input.Kind, FormatVersion = prepared.Value.FormatVersion, Context = Context(frozen.Context) };
                copy.SetPermanent(prepared.Value.GetPermanent());
                var permanentSource = prepared.Value.GetPermanentMigration();
                if (permanentSource != null) copy.SetPermanentMigration(new CandidateRosterMigrationInput {
                    SourceGeneration = permanentSource.SourceGeneration, SourceDescriptorLength = permanentSource.SourceDescriptorLength,
                    SourceDescriptorSha256 = Array.AsReadOnly(permanentSource.SourceDescriptorSha256.ToArray()) });
                if (input.RosterInitialize != null) copy.RosterInitialize = new CandidateRosterInitializeInput {
                    Characters = input.RosterInitialize.Characters.Select(x => new CandidateApplicationInitializeInput {
                        CharacterId = x.CharacterId, ClassId = x.ClassId, InitialLevel = x.InitialLevel,
                        InitialExperience = x.InitialExperience, OriginalSlot = x.OriginalSlot }).ToList().AsReadOnly(),
                    Slots = Array.AsReadOnly(input.RosterInitialize.Slots.ToArray()) };
                if (input.SetFormation != null) copy.SetFormation = new CandidateFormationInput {
                    ExpectedFormationRevision = input.SetFormation.ExpectedFormationRevision,
                    Slots = Array.AsReadOnly(input.SetFormation.Slots.ToArray()) };
                if (input.MigrateRoster != null) copy.MigrateRoster = new CandidateRosterMigrationInput {
                    SourceGeneration = input.MigrateRoster.SourceGeneration, SourceDescriptorLength = input.MigrateRoster.SourceDescriptorLength,
                    SourceDescriptorSha256 = Array.AsReadOnly(input.MigrateRoster.SourceDescriptorSha256.ToArray()) };
                if (input.EnterFormation != null)
                {
                    var entry = input.EnterFormation;
                    copy.EnterFormation = new CandidateFormationEntryInput { LevelId = entry.LevelId, LevelVersion = entry.LevelVersion,
                        ExpectedFormationRevision = entry.ExpectedFormationRevision, Slots = Array.AsReadOnly(entry.Slots.ToArray()),
                        SelectedCharacters = entry.SelectedCharacters.Select(x => new CandidateCharacterRevisionInput {
                            CharacterId = x.CharacterId, ExpectedRevision = x.ExpectedRevision }).ToList().AsReadOnly(),
                        ExpectedInventoryRevision = entry.ExpectedInventoryRevision, ExpectedProgressionRevision = entry.ExpectedProgressionRevision,
                        TimeSample = Time(entry.TimeSample, budget) };
                }
                return new CandidateLifecyclePrepareResult(new PreparedCandidateLifecycleRequest(prepared.Value, copy, frozen, null));
            });
        }
        private static CandidateLifecyclePrepareResult Freeze(CandidateApplicationIntentInput input, CandidateLifecycleContent content,
            CandidateTimeSample time, SaveCodecBudget budget)
        {
            var result = CandidateApplicationProtocol.PrepareIntent(input, budget);
            return result.IsAccepted ? new CandidateLifecyclePrepareResult(new PreparedCandidateLifecycleRequest(result.Value, input, content, time)) :
                new CandidateLifecyclePrepareResult(null, CandidateApplicationDiagnostic.From(result, "Prepare"));
        }
        private static CandidateApplicationEndInput End(CandidateApplicationEndInput input, SaveCodecBudget budget)
        {
            Text(input.AttemptId, "End.AttemptId", budget); Text(input.ChallengeId, "End.ChallengeId", budget);
            Text(input.EntryBaselineId, "End.EntryBaselineId", budget); Number(input.ExpectedSceneRevision, budget);
            return new CandidateApplicationEndInput { AttemptId = input.AttemptId, ChallengeId = input.ChallengeId,
                EntryBaselineId = input.EntryBaselineId, ExpectedSceneRevision = input.ExpectedSceneRevision };
        }
        private static CandidateTimeSample Time(CandidateTimeSample t, SaveCodecBudget budget)
        {
            if (t == null) return null;
            Need(t.WallUtcMilliseconds.HasValue && t.ObservedAtUtcMilliseconds.HasValue && t.Anomaly.HasValue,
                "MissingField", "TimeSample");
            Number(t.WallUtcMilliseconds, budget); Number(t.ObservedAtUtcMilliseconds, budget);
            Need(t.HasMonotonic, "MissingField", "TimeSample.MonotonicElapsedMilliseconds");
            Need(t.HasScope, "MissingField", "TimeSample.MonotonicScopeId");
            Text(t.Source, "TimeSample.Source", budget); Text(t.MonotonicScopeId, "TimeSample.MonotonicScopeId", budget);
            Need(!string.IsNullOrWhiteSpace(t.Source), "MissingField", "TimeSample.Source");
            Need(t.Trust == CandidateTimeTrust.DeviceUntrusted, "UnsupportedBinding", "TimeSample.Trust");
            Need((t.MonotonicElapsedMilliseconds == null) == (t.MonotonicScopeId == null), "InconsistentBinding", "TimeSample.MonotonicScopeId");
            Need((t.Anomaly.Value & ~(CandidateTimeAnomaly.ClockBackward | CandidateTimeAnomaly.DomainChanged)) == 0, "InvalidValue", "TimeSample.Anomaly");
            if (t.MonotonicElapsedMilliseconds != null)
            {
                Rational(t.MonotonicElapsedMilliseconds, budget);
                Need(t.MonotonicElapsedMilliseconds.Numerator.Sign >= 0 && !string.IsNullOrWhiteSpace(t.MonotonicScopeId), "InvalidValue", "TimeSample.Monotonic");
            }
            return new CandidateTimeSample { WallUtcMilliseconds = t.WallUtcMilliseconds, ObservedAtUtcMilliseconds = t.ObservedAtUtcMilliseconds,
                MonotonicElapsedMilliseconds = t.MonotonicElapsedMilliseconds, MonotonicScopeId = t.MonotonicScopeId,
                Source = t.Source, Trust = t.Trust, Anomaly = t.Anomaly };
        }
        internal static RuleContext Context(PreparedRuleContext c)
        { return RuleContextChecks.Copy(c); }
        internal static bool SameContext(PreparedRuleContext a, PreparedRuleContext b, ExactMathBudget math)
        { return RuleContextChecks.Same(a, b); }
        private static CandidateLifecycleContent Content(CandidateLifecycleContentInput c, SaveCodecBudget b, bool published)
        {
            Need(c != null && (c.Growth != null || c.Growths != null) && c.Inventory != null && c.Progression != null, "MissingField", "Content");
            var growths = c.Growths ?? new[] { c.Growth };
            Count(growths, "Content.Growths", b);
            Need(growths.Count > 0 && (c.Growths == null || c.Growth == null), "InvalidValue", "Content.Growths");
            // Check all caller collection counts before traversing or copying any of them.
            Count(c.Levels, "Content.Levels", b); Count(c.Rewards, "Content.Rewards", b); Count(c.CritCoefficients, "Content.CritCoefficients", b);
            Count(c.Inventory.Items, "Content.Inventory.Items", b); Count(c.Progression.Levels, "Content.Progression.Levels", b);
            CheckContext(c.Inventory.Context, b, published); CheckContext(c.Progression.Context, b, published);
            var classIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var growth in growths)
            {
                Need(growth != null && classIds.Add(growth.ClassId), "InvalidValue", "Content.Growths");
                CheckContext(growth.Context, b, published);
                Need(SameContext(growth.Context, c.Inventory.Context, b.Math) && SameContext(growth.Context, c.Progression.Context, b.Math),
                    "InconsistentBinding", "Content.Context");
                Text(growth.ClassId, "Growth.ClassId", b); Text(growth.PassiveDefinitionId, "Growth.PassiveDefinitionId", b); Stats(growth.BaseStats, b);
                foreach (var n in new[] { growth.GrowthHp, growth.GrowthAttack, growth.GrowthDefense, growth.RecoveryDurationMilliseconds }) Rational(n, b);
                if (growth.ClassKind == CharacterClassKind.Warrior)
                    foreach (var n in new[] { growth.CritBase, growth.CritStep, growth.CritCap, growth.CritMultiplier }) Rational(n, b);
                else Need(published && growth.ClassKind == CharacterClassKind.Mage && growth.CritBase == null &&
                    growth.CritStep == null && growth.CritCap == null && growth.CritMultiplier == null, "UnsupportedBinding", "Growth.ClassKind");
                Number(growth.XpBase, b); Number(growth.XpLinear, b); Number(growth.XpQuadratic, b);
            }
            foreach (var item in c.Inventory.Items) { Text(item.ItemId, "Inventory.ItemId", b); Text(item.EquipClassId, "Inventory.EquipClassId", b); }
            var levels = new HashSet<string>(StringComparer.Ordinal);
            foreach (var level in c.Levels)
            {
                Need(level != null, "MissingField", "Content.Levels");
                Need(levels.Add(level.LevelId), "InconsistentBinding", "Content.Levels.LevelId");
                CheckLevel(level, b);
                Need(c.Progression.Levels.Any(x => x.LevelId == level.LevelId && x.LevelVersion == level.LevelVersion), "InconsistentBinding", "Content.Levels");
            }
            Need(c.Progression.Levels.Count == c.Levels.Count, "InconsistentBinding", "Content.Levels");
            foreach (var p in c.Progression.Levels)
            {
                Text(p.LevelId, "Progression.LevelId", b); Text(p.LevelVersion, "Progression.LevelVersion", b);
                Text(p.UnlockRuleId, "Progression.UnlockRuleId", b); Text(p.UnlockAfterLevelId, "Progression.UnlockAfterLevelId", b);
                Count(p.RequiredFeatures, "Progression.RequiredFeatures", b);
            }
            var rewardIds = new HashSet<string>(StringComparer.Ordinal); var rewardLevels = new HashSet<string>(StringComparer.Ordinal);
            foreach (var r in c.Rewards)
            {
                Need(r != null, "MissingField", "Content.Rewards"); CheckContext(r.Context, b, published);
                Need(SameContext(c.Inventory.Context, r.Context, b.Math) && rewardIds.Add(r.RewardDefinitionId) && rewardLevels.Add(r.LevelId) &&
                    c.Levels.Any(x => x.LevelId == r.LevelId && x.LevelVersion == r.LevelVersion), "InconsistentBinding", "Content.Rewards");
                Text(r.RewardDefinitionId, "Reward.Id", b); Text(r.Version, "Reward.Version", b); Text(r.LevelId, "Reward.LevelId", b); Text(r.LevelVersion, "Reward.LevelVersion", b);
                Count(r.Materials, "Reward.Materials", b); Count(r.RequiredFeatures, "Reward.RequiredFeatures", b);
                Number(r.BaseExperience, b); Number(r.OverlevelGrace, b);
                foreach (var n in new[] { r.DamageWeight, r.TakenWeight, r.CurveBase, r.CurveLog, r.ReferenceHpDivisor, r.LevelPenaltyBase }) Rational(n, b);
                foreach (var m in r.Materials)
                {
                    Text(m.ItemId, "Reward.Material.ItemId", b); Number(m.Amount, b);
                    Need(c.Inventory.Items.Any(x => x.ItemId == m.ItemId && x.Kind == CandidateInventoryItemKind.OrdinaryMaterial), "InconsistentBinding", "Reward.Material.ItemId");
                }
            }
            Need(rewardLevels.SetEquals(levels), "InconsistentBinding", "Content.Rewards");
            var probabilities = new HashSet<string>(StringComparer.Ordinal);
            foreach (var row in c.CritCoefficients)
            {
                Need(row != null, "MissingField", "CritCoefficients"); Rational(row.TargetProbability, b); Rational(row.C, b);
                Need(row.TargetProbability.Numerator.Sign > 0 && row.TargetProbability.Numerator <= row.TargetProbability.Denominator &&
                    row.C.Numerator.Sign > 0 && row.C.Numerator <= row.C.Denominator, "InvalidValue", "CritCoefficients");
                Need(probabilities.Add(row.TargetProbability.Numerator + "/" + row.TargetProbability.Denominator), "InconsistentBinding", "CritCoefficients.TargetProbability");
            }
            var frozen = new CandidateLifecycleContent(c);
            if (c.GetPermanentDefinitions() != null)
            {
                var permanent = PermanentDefinitions(frozen, b);
                if (!permanent.IsAccepted) throw new Refusal(CandidateApplicationDiagnostic.From(permanent, "Content"));
            }
            return frozen;
        }
        private static void CheckLevel(PreparedLevel l, SaveCodecBudget b)
        {
            Text(l.LevelId, "Level.Id", b); Text(l.LevelVersion, "Level.Version", b); Number(l.RecommendedLevel, b); Count(l.Faces, "Level.Faces", b);
            foreach (var f in l.Faces)
            {
                Text(f.FaceId, "Face.Id", b); Number(f.Width, b); Number(f.Height, b); Count(f.Pairs, "Face.Pairs", b);
                foreach (var p in f.Pairs)
                {
                    Text(p.PairId, "Pair.Id", b); Number(p.GeometryColorId, b);
                    Number(p.EndpointA.x, b); Number(p.EndpointA.y, b); Number(p.EndpointB.x, b); Number(p.EndpointB.y, b);
                    var e = p.Enemy; Text(e.EnemyInstanceKey, "Enemy.Key", b); Text(e.EnemyDefinitionId, "Enemy.Id", b);
                    Number(e.OriginalSlot, b); Number(e.StableOrder, b); Stats(e.Stats, b); Count(e.IntentCycle, "Enemy.Intents", b);
                    foreach (var intent in e.IntentCycle) if (intent.DamageCoefficient != null) Rational(intent.DamageCoefficient, b);
                }
            }
        }
        private static void Stats(PreparedStats s, SaveCodecBudget b)
        { foreach (var n in new[] { s.MaxHp, s.Attack, s.PhysicalDefense, s.MagicDefense, s.Evasion }) Rational(n, b); Number(s.AttackRange, b); }
        private static void CheckContext(PreparedRuleContext c, SaveCodecBudget b, bool published)
        {
            Need(published ? c is PreparedPublishedRuleContext : c is PreparedCandidateContext, "UnsupportedBinding", "Context");
            var checkedContext = RuleContextChecks.CheckBudget(c, b);
            if (!checkedContext.IsAccepted) throw new Refusal(CandidateApplicationDiagnostic.From(checkedContext, "Prepare"));
        }
        private static void Need(bool condition, string code, string path)
        { if (!condition) throw new Refusal(new CandidateApplicationDiagnostic(code, path, stage: "Prepare")); }
        private static void Count<T>(IReadOnlyList<T> rows, string path, SaveCodecBudget budget)
        { Need(rows != null, "MissingField", path); Limit(rows.Count, budget.MaxCollectionEntries, path, "CollectionEntries"); }
        private static void Text(string value, string path, SaveCodecBudget budget)
        { if (value != null) Limit(value.Length, budget.MaxStringCodeUnits, path, "StringCodeUnits"); }
        private static void Limit(int required, int allowed, string path, string reason)
        { if (required > allowed) throw new Refusal(new CandidateApplicationDiagnostic("Limit", path, reason, (ulong)required, (ulong)allowed, "Prepare")); }
        private static void Number(BigInteger? number, SaveCodecBudget budget)
        {
            if (!number.HasValue) return;
            var encoded = ExactSaveValueCodec.EncodeInteger(number.Value, budget);
            if (!encoded.IsAccepted) throw new Refusal(CandidateApplicationDiagnostic.From(encoded, "Prepare"));
        }
        private static void Rational(ExactRational value, SaveCodecBudget budget)
        { Need(value != null, "MissingField", "Number"); Number(value.Numerator, budget); Number(value.Denominator, budget); }
    }
}
