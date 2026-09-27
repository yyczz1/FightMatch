using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using FightMatch.Core;
using FlowPuzzle.Core;
using FlowPuzzle.Validation;
using static FightMatch.Content.ContentChecks;

namespace FightMatch.Content
{
    public static class DemoContentCompiler
    {
        public static DemoContentPreparationResult PrepareCandidate(DemoContentInput input, DemoContentJob job, ExactMathBudget budget)
        {
            try
            {
                Root(job, "Job"); job.Check(); Root(input, "Input"); Root(budget, "Budget");
                Need(input.Coordinates != DemoCoordinateCandidate.Unspecified, "MissingSourceCoordinateConvention", "Coordinates");
                Need(input.Coordinates == DemoCoordinateCandidate.AssumedBottomLeft || input.Coordinates == DemoCoordinateCandidate.AssumedTopLeft,
                    "UnsupportedBinding", "Coordinates");
                Root(input.Growth, "Growth"); Root(input.Reward, "Reward"); Root(input.Level, "Level");
                Need(input.Growth.Context == null && input.Reward.Context == null, "UnsupportedBinding", "CompilerOwnedContext");
                Root(input.Parameters, "Parameters"); Need(input.Parameters.Count == 31, "MissingField", "Parameters[0..30]");
                Root(input.LearnedSkills, "LearnedSkills"); Need(input.LearnedSkills.Count == 0, "UnsupportedBinding", "LearnedSkills");
                Root(input.SourceNotes, "SourceNotes"); Need(input.SourceNotes.Count > 0, "MissingField", "SourceNotes");
                Root(input.SourceRoutes, "SourceRoutes");
                var sources = Sources(input.Sources);
                var fingerprint = Fingerprint(input, job, sources, budget);
                var context = new CandidateContext { DraftId = job.DraftId, DraftRevision = job.Revision, ContentFingerprint = fingerprint,
                    RuleVersion = input.RuleVersion, NumericContractVersion = input.NumericContractVersion, RandomContractVersion = input.RandomContractVersion,
                    SourceNotes = new List<string>(input.SourceNotes) };
                var growth = CandidateCharacterGrowth.PrepareDefinition(Growth(input.Growth, context), budget);
                Need(growth.IsAccepted, growth.RejectionCode.ToString(), "Growth." + growth.FieldPath);
                var character = CandidateCharacterGrowth.CreateCandidate(growth.Definition, input.PlayerId, input.CharacterId,
                    input.CharacterLevel, input.CharacterExperience, input.OriginalSlot, budget);
                Need(character.IsAccepted, character.RejectionCode.ToString(), "Character." + character.FieldPath);
                var stats = CandidateCharacterGrowth.ComputeBaseStats(character.Next, budget);
                var proofs = new List<DemoParameterEvidence>();
                for (var i = 0; i < input.Parameters.Count; i++)
                {
                    job.Check(); var row = input.Parameters[i]; Root(row, "Parameters[" + i + "]");
                    var target = growth.Definition.CritBase.Add(growth.Definition.CritStep.Multiply(ExactRational.Create(i, 1, budget), budget), budget);
                    if (target.Compare(growth.Definition.CritCap, budget) > 0) target = growth.Definition.CritCap;
                    Root(row.Target, "Parameters[" + i + "].Target");
                    Need(target.Compare(row.Target, budget) == 0, "InconsistentBinding", "Parameters[" + i + "].Target");
                    var proof = DemoContentParameters.EvaluateParameterEvidence(row.Target, row.C, row.Epsilon, job, budget);
                    Need(proof.IsAccepted, proof.RejectionCode, "Parameters[" + i + "]." + proof.FieldPath);
                    Need(proof.Evidence.WithinProposedTolerance, "ToleranceExceeded", "Parameters[" + i + "].Epsilon");
                    proofs.Add(proof.Evidence);
                }
                Need(proofs[30].Target.Compare(growth.Definition.CritCap, budget) == 0, "UnsupportedBinding", "Parameters.CapAt31");
                var index = input.CharacterLevel > 31 ? 30 : (int)input.CharacterLevel - 1;
                Need(stats.TargetProbability.Compare(proofs[index].Target, budget) == 0, "InconsistentBinding", "Character.TargetProbability");
                var member = new MemberInput { CharacterId = stats.CharacterId, ClassId = stats.ClassId, ClassKind = stats.ClassKind,
                    OriginalSlot = stats.OriginalSlot, Level = stats.Level, IsReady = stats.IsReady, StatsOrigin = stats.StatsOrigin,
                    StatsContext = context, Stats = Stats(stats.Stats), EntryHp = stats.EntryHp, LearnedSkills = new List<string>(input.LearnedSkills),
                    Crit = new WarriorCritInput { PassiveDefinitionId = stats.PassiveDefinitionId, TargetProbability = stats.TargetProbability,
                        C = proofs[index].C, Multiplier = stats.CritMultiplier } };
                var prepared = new BattleEntryPreparer().PrepareCandidate(new BattleEntryInput { Context = context, PlayerId = input.PlayerId,
                    ChallengeId = input.ChallengeId, AttemptId = input.AttemptId, EntryBaselineId = input.EntryBaselineId, Level = input.Level,
                    Members = new List<MemberInput> { member }, CarryMode = input.CarryMode, RequiredFeatures = input.RequiredFeatures }, budget);
                Need(prepared.IsAccepted, prepared.RejectionCode.ToString(), "Entry." + prepared.FieldPath);
                var routes = Geometry(prepared.Entry, input.SourceRoutes, input.Coordinates, job);
                var reward = CandidateBaseRewards.PrepareDefinition(Reward(input.Reward, context), budget);
                Need(reward.IsAccepted, reward.RejectionCode.ToString(), "Reward." + reward.FieldPath);
                Need(reward.Definition.LevelId == prepared.Entry.Level.LevelId && reward.Definition.LevelVersion == prepared.Entry.Level.LevelVersion,
                    "InconsistentBinding", "Reward.Level");
                job.Check();
                return new DemoContentPreparationResult(new DemoPreparedContent(job, input.Coordinates, prepared.Entry, character.Next,
                    stats, reward.Definition, sources, routes, proofs));
            }
            catch (ContentFailure ex) { return new DemoContentPreparationResult(ex.Code, ex.Path); }
            catch (ExactMathLimitException) { return new DemoContentPreparationResult("BudgetExceeded", "ExactMathBudget"); }
            catch (EncoderFallbackException) { return new DemoContentPreparationResult("InvalidValue", "Fingerprint.String"); }
        }

        private static List<DemoContentSource> Sources(List<DemoContentSourceInput> input)
        {
            Root(input, "Sources"); Need(input.Count > 0, "MissingField", "Sources");
            var output = new List<DemoContentSource>(); var identities = new HashSet<string>(StringComparer.Ordinal);
            foreach (var row in input)
            {
                Root(row, "Sources[]"); Text(row.Path, "Sources.Path"); Text(row.Locator, "Sources.Locator");
                Need(identities.Add(row.Path.Length + ":" + row.Path + row.Locator), "InvalidValue", "Sources.DuplicateLocator");
                var sha = row.Bytes == null ? row.Sha256?.ToLowerInvariant() : Hash(row.Bytes);
                Text(sha, "Sources.Sha256"); Need(sha.Length == 64 && sha.All(c => c >= '0' && c <= '9' || c >= 'a' && c <= 'f'), "InvalidValue", "Sources.Sha256");
                if (row.Bytes != null && row.Sha256 != null)
                    Need(string.Equals(sha, row.Sha256, StringComparison.OrdinalIgnoreCase), "InconsistentBinding", "Sources.Sha256");
                output.Add(new DemoContentSource(row.Path, row.Locator, sha));
            }
            return output.OrderBy(s => s.Path, StringComparer.Ordinal).ThenBy(s => s.Locator, StringComparer.Ordinal).ToList();
        }

        private static List<DemoContentRoute> Geometry(PreparedBattleEntry entry, List<DemoContentRouteInput> input,
            DemoCoordinateCandidate mapping, DemoContentJob job)
        {
            var result = new List<DemoContentRoute>(); var used = new HashSet<DemoContentRouteInput>();
            foreach (var face in entry.Level.Faces)
            {
                job.Check(); var level = new FlowLevelData { levelId = result.Count + 1, width = face.Width, height = face.Height };
                var solution = new FlowSolutionData { levelId = level.levelId };
                foreach (var pair in face.Pairs)
                {
                    var matches = input.Where(r => r != null && r.FaceId == face.FaceId && r.PairId == pair.PairId).ToList();
                    Need(matches.Count == 1, matches.Count == 0 ? "MissingField" : "InvalidValue", "SourceRoutes." + face.FaceId + "." + pair.PairId);
                    var source = matches[0]; Root(source.Cells, "SourceRoutes.Cells"); used.Add(source);
                    var cells = new List<FlowPos>();
                    foreach (var cell in source.Cells)
                    {
                        Need(cell.x >= 1 && cell.x <= face.Width && cell.y >= 1 && cell.y <= face.Height, "InvalidValue", "SourceRoutes.OneBasedCell");
                        cells.Add(new FlowPos(cell.x - 1, mapping == DemoCoordinateCandidate.AssumedBottomLeft ? cell.y - 1 : face.Height - cell.y));
                    }
                    level.pairs.Add(new FlowPairData { colorId = pair.GeometryColorId, endpointA = pair.EndpointA, endpointB = pair.EndpointB });
                    solution.paths.Add(new FlowPathData { colorId = pair.GeometryColorId, cells = cells });
                    result.Add(new DemoContentRoute(source, cells));
                }
                var validation = new FlowSolutionValidator().Validate(level, solution);
                Need(validation.isValid, "InvalidGeometry", face.FaceId + ":" + validation.errorCode);
            }
            Need(used.Count == input.Count, "InconsistentBinding", "SourceRoutes.Unbound");
            return result;
        }

        internal static RuleContext Context(PreparedRuleContext context)
        { return RuleContextChecks.Copy(context); }
        private static StatsInput Stats(PreparedStats s)
        { return new StatsInput { MaxHp = s.MaxHp, Attack = s.Attack, PhysicalDefense = s.PhysicalDefense, MagicDefense = s.MagicDefense,
            Evasion = s.Evasion, AttackRange = s.AttackRange }; }
        private static GrowthDefinitionInput Growth(GrowthDefinitionInput g, RuleContext context)
        { return new GrowthDefinitionInput { Context = context, ClassId = g.ClassId, ClassKind = g.ClassKind, PassiveDefinitionId = g.PassiveDefinitionId,
            BaseStats = g.BaseStats, GrowthHp = g.GrowthHp, GrowthAttack = g.GrowthAttack, GrowthDefense = g.GrowthDefense,
            CritBase = g.CritBase, CritStep = g.CritStep, CritCap = g.CritCap, CritMultiplier = g.CritMultiplier,
            XpBase = g.XpBase, XpLinear = g.XpLinear, XpQuadratic = g.XpQuadratic, RecoveryDurationMilliseconds = g.RecoveryDurationMilliseconds }; }
        private static CandidateRewardDefinitionInput Reward(CandidateRewardDefinitionInput r, RuleContext context)
        { return new CandidateRewardDefinitionInput { Context = context, RewardDefinitionId = r.RewardDefinitionId, Version = r.Version,
            LevelId = r.LevelId, LevelVersion = r.LevelVersion, BaseExperience = r.BaseExperience, DamageWeight = r.DamageWeight,
            TakenWeight = r.TakenWeight, CurveBase = r.CurveBase, CurveLog = r.CurveLog, ReferenceHpDivisor = r.ReferenceHpDivisor,
            LevelPenaltyBase = r.LevelPenaltyBase, OverlevelGrace = r.OverlevelGrace, ZeroContributionPolicy = r.ZeroContributionPolicy,
            DropMode = r.DropMode, RequiredFeatures = r.RequiredFeatures, Materials = r.Materials }; }

        // Fixed schema, length-prefixed UTF-8 fields; no culture, JSON property-order, float, or caller hash dependence.
        private static string Fingerprint(DemoContentInput i, DemoContentJob job, List<DemoContentSource> sources, ExactMathBudget budget)
        {
            using (var c = new Canonical(budget))
            {
                c.S("demo-content-candidate-v1"); c.S(job.DraftId); c.N(job.Revision);
                c.S(i.RuleVersion); c.S(i.NumericContractVersion); c.S(i.RandomContractVersion); c.N((int)i.Coordinates);
                c.List(sources, s => { c.S(s.Path); c.S(s.Locator); c.S(s.Sha256); }); c.List(i.SourceNotes, c.S);
                c.S(i.PlayerId); c.S(i.CharacterId); c.N(i.CharacterLevel); c.N(i.CharacterExperience); c.N(i.OriginalSlot);
                c.S(i.ChallengeId); c.S(i.AttemptId); c.S(i.EntryBaselineId); c.N((int)i.CarryMode);
                c.List(i.RequiredFeatures, c.S); c.List(i.LearnedSkills, c.S);
                var g = i.Growth; c.S(g.ClassId); c.N((int)g.ClassKind); c.S(g.PassiveDefinitionId); c.Stats(g.BaseStats);
                c.R(g.GrowthHp); c.R(g.GrowthAttack); c.R(g.GrowthDefense); c.R(g.CritBase); c.R(g.CritStep); c.R(g.CritCap); c.R(g.CritMultiplier);
                c.N(g.XpBase); c.N(g.XpLinear); c.N(g.XpQuadratic); c.R(g.RecoveryDurationMilliseconds);
                c.S(i.Level.LevelId); c.S(i.Level.LevelVersion); c.N(i.Level.RecommendedLevel);
                c.List(i.Level.Faces, f =>
                {
                    Root(f, "Level.Faces[]"); c.S(f.FaceId); c.N(f.Width); c.N(f.Height);
                    c.List(f.Pairs, p =>
                    {
                        Root(p, "Level.Pairs[]"); c.S(p.PairId); c.N(p.GeometryColorId); c.Cell(p.EndpointA); c.Cell(p.EndpointB);
                        var e = p.Enemy; Root(e, "Level.Enemy"); c.S(e.EnemyInstanceKey); c.S(e.EnemyDefinitionId);
                        c.N(e.OriginalSlot); c.N(e.StableOrder); c.N((int)e.Behavior); c.Stats(e.Stats);
                        c.List(e.IntentCycle, t => { Root(t, "Level.IntentCycle[]"); c.N((int)t.Kind); c.N((int)t.Targeting);
                            c.N(t.DamageKind.HasValue ? (BigInteger?)(int)t.DamageKind.Value : null); c.R(t.DamageCoefficient); });
                    });
                });
                c.List(i.SourceRoutes, r => { Root(r, "SourceRoutes[]"); c.S(r.FaceId); c.S(r.PairId); c.List(r.Cells, c.Cell); });
                c.List(i.Parameters, p => { Root(p, "Parameters[]"); c.R(p.Target); c.R(p.C); c.R(p.Epsilon); });
                var r = i.Reward; c.S(r.RewardDefinitionId); c.S(r.Version); c.S(r.LevelId); c.S(r.LevelVersion); c.N(r.BaseExperience);
                c.R(r.DamageWeight); c.R(r.TakenWeight); c.R(r.CurveBase); c.R(r.CurveLog); c.R(r.ReferenceHpDivisor); c.R(r.LevelPenaltyBase);
                c.N(r.OverlevelGrace); c.N((int)r.ZeroContributionPolicy); c.N((int)r.DropMode); c.List(r.RequiredFeatures, c.S);
                c.List(r.Materials, m => { Root(m, "Reward.Materials[]"); c.S(m.ItemId); c.N(m.Amount); });
                return Hash(c.Bytes());
            }
        }
        private static string Hash(byte[] bytes)
        { using (var sha = SHA256.Create()) return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", "").ToLowerInvariant(); }
        private sealed class Canonical : IDisposable
        {
            private static readonly UTF8Encoding utf8 = new UTF8Encoding(false, true);
            private readonly MemoryStream stream = new MemoryStream();
            private readonly ExactMathBudget budget;
            internal Canonical(ExactMathBudget budget) { this.budget = budget; }
            internal void S(string value)
            {
                var bytes = value == null ? null : utf8.GetBytes(value);
                var prefix = Encoding.ASCII.GetBytes((bytes == null ? "-1" : bytes.Length.ToString(CultureInfo.InvariantCulture)) + ":");
                stream.Write(prefix, 0, prefix.Length); if (bytes != null) stream.Write(bytes, 0, bytes.Length);
            }
            internal void N(BigInteger? value)
            { if (value.HasValue) ExactRational.Create(value.Value, 1, budget); S(value?.ToString(CultureInfo.InvariantCulture)); }
            internal void R(ExactRational value) { N(value?.Numerator); N(value?.Denominator); }
            internal void Cell(FlowPos p) { N(p.x); N(p.y); }
            internal void Stats(StatsInput s)
            { Root(s, "Stats"); R(s.MaxHp); R(s.Attack); R(s.PhysicalDefense); R(s.MagicDefense); R(s.Evasion); N(s.AttackRange); }
            internal void List<T>(ICollection<T> values, Action<T> write)
            { Root(values, "Collection"); N(values.Count); foreach (var value in values) write(value); }
            internal byte[] Bytes() { return stream.ToArray(); }
            public void Dispose() { stream.Dispose(); }
        }
    }
}
