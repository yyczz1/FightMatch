using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using FightMatch.Content;
using FlowPuzzle.Core;
using NUnit.Framework;

namespace FightMatch.Core.Tests
{
    internal static class DemoContentTestData
    {
        internal const string EvidenceRoot = "TestArtifacts/FMDemoB17/62aa18a2eb9443d195296216f0184526/stage025p1/review-inputs";
        internal const string Boards = "docs/game-design/balance/results/boards.json";
        internal const string Config = "docs/game-design/balance/config.json";
        internal static readonly BigInteger Grid = BigInteger.Pow(10, 12);
        private static List<PrdRow> candidates;
        internal static ExactMathBudget Math() { return new ExactMathBudget(maxPrimitiveSteps: 16000000); }
        internal static ExactRational R(BigInteger n, BigInteger? d = null) { return ExactRational.Create(n, d ?? BigInteger.One, Math()); }
        internal static ExactEvaluationScope Scope() { return new ExactEvaluationScope(new ExactEvaluationBudget(Math())); }
        internal static DemoContentJob Job(string id = "demo-p1") { return new DemoContentDraft(id).BeginJob(); }
        internal static void Same(ExactRational a, ExactRational b) { Assert.AreEqual(b.Numerator, a.Numerator); Assert.AreEqual(b.Denominator, a.Denominator); }
        internal static string Sha(string path)
        { using (var hash = SHA256.Create()) return BitConverter.ToString(hash.ComputeHash(File.ReadAllBytes(path))).Replace("-", "").ToLowerInvariant(); }

        internal sealed class PrdRow
        {
            public int Level { get; set; }
            public BigInteger LowerGridNumerator { get; set; }
            public BigInteger UpperGridNumerator { get; set; }
            public ExactRational LowerRate { get; set; }
            public ExactRational UpperRate { get; set; }
            public DemoPrdParameterInput Proposed { get; set; }
        }
        internal static List<PrdRow> Candidates()
        {
            if (candidates != null) return candidates;
            var rows = new List<PrdRow>();
            for (var i = 0; i < 31; i++)
            {
                var p = R(40 + i, 200); BigInteger low = 1, high = Grid;
                while (high - low > 1)
                {
                    var middle = (low + high) / 2;
                    if (ModelRate(R(middle, Grid)).Compare(p, Math()) <= 0) low = middle; else high = middle;
                }
                var lower = ModelRate(R(low, Grid)); var upper = ModelRate(R(high, Grid));
                var lowerError = p.Subtract(lower, Math()); var upperError = upper.Subtract(p, Math());
                var selected = lowerError.Compare(upperError, Math()) <= 0 ? low : high;
                rows.Add(new PrdRow { Level = i + 1, LowerGridNumerator = low, UpperGridNumerator = high,
                    LowerRate = lower, UpperRate = upper, Proposed = new DemoPrdParameterInput { Target = p, C = R(selected, Grid), Epsilon = R(1, 1000000000) } });
            }
            candidates = rows; return rows;
        }
        // Test-side integer oracle: survival products over a common denominator, without the production proof code.
        internal static ExactRational ModelRate(ExactRational c)
        {
            var n = (int)((c.Denominator + c.Numerator - 1) / c.Numerator);
            BigInteger survival = 1, power = 1, sumNumerator = 1;
            for (var k = 1; k < n; k++)
            { survival *= c.Denominator - k * c.Numerator; power *= c.Denominator; sumNumerator = sumNumerator * c.Denominator + survival; }
            return R(power, sumNumerator);
        }
        internal static DemoPrdParameterInput Copy(DemoPrdParameterInput p)
        { return new DemoPrdParameterInput { Target = p.Target, C = p.C, Epsilon = p.Epsilon }; }
        internal static DemoContentInput MakeInput(int stage = 1, DemoCoordinateCandidate coordinates = DemoCoordinateCandidate.AssumedBottomLeft)
        {
            Assert.Contains(stage, new[] { 1, 3 });
            Assert.AreEqual("4e815411914bf86fe9e1a046ee80b86c24d048bd8455a1d4bb55a088255aa7f3", Sha(Config));
            Assert.AreEqual("671467caf72c52eb83a03b396ad82f5557a47b2f9cbd3f9da6d0ec560a7d2aad", Sha(Boards));
            var face = new FaceInput { FaceId = "F1", Width = 4, Height = 4, Pairs = new List<PairInput>() };
            var original = stage == 1
                ? new[] { new[] { new FlowPos(1, 1), new FlowPos(1, 2), new FlowPos(2, 2) }, new[] { new FlowPos(1, 4), new FlowPos(2, 4), new FlowPos(3, 4) } }
                : new[] { new[] { new FlowPos(1, 4), new FlowPos(1, 3), new FlowPos(2, 3) }, new[] { new FlowPos(1, 1), new FlowPos(2, 1), new FlowPos(3, 1) } };
            var routes = new List<DemoContentRouteInput>();
            for (var p = 0; p < 2; p++)
            {
                var key = p == 0 ? "A" : "B"; var heavy = stage == 3 && p == 0;
                var cycle = new List<EnemyIntentInput>();
                if (heavy) cycle.Add(new EnemyIntentInput { Kind = EnemyIntentKind.Charge, Targeting = EnemyTargeting.FirstLiving,
                    DamageKind = null, DamageCoefficient = null });
                cycle.Add(new EnemyIntentInput { Kind = EnemyIntentKind.Strike, Targeting = EnemyTargeting.FirstLiving,
                    DamageKind = EntryDamageKind.Physical, DamageCoefficient = heavy ? R(13, 10) : R(3, 5) });
                face.Pairs.Add(new PairInput { PairId = key, GeometryColorId = p, EndpointA = Map(original[p][0], coordinates),
                    EndpointB = Map(original[p][2], coordinates), Enemy = new EnemyInput { EnemyInstanceKey = key, EnemyDefinitionId = heavy ? "candidate:E02" : "candidate:E01",
                        OriginalSlot = p, StableOrder = p, Behavior = heavy ? EnemyBehavior.ChargeHeavy : EnemyBehavior.NormalStrike,
                        Stats = Stats(heavy ? 20 : 15, 10, heavy ? 20 : 0, 0), IntentCycle = cycle } });
                routes.Add(new DemoContentRouteInput { FaceId = "F1", PairId = key, Cells = new List<FlowPos>(original[p]) });
            }
            var levelId = "candidate:L" + stage;
            return new DemoContentInput { RuleVersion = "candidate-demo-r1", NumericContractVersion = "RC01", RandomContractVersion = "PC01+SC01",
                Coordinates = coordinates, Sources = new List<DemoContentSourceInput> {
                    new DemoContentSourceInput { Path = Boards, Locator = "[stage=" + stage + ",phase=1].size,paths.A,paths.B", Sha256 = Sha(Boards) },
                    new DemoContentSourceInput { Path = Config, Locator = "roles[id=W].{hp,attack,pdef,mdef,range,p0,p_per_level,p_cap,effect}; progression.{hp_growth,attack_growth,defense_growth,xp_level_base,xp_level_linear,xp_level_quadratic,base_offset,base_per_stage,overlevel_grace,overlevel_factor,score_floor,score_log_scale,contribution_divisor}", Bytes = File.ReadAllBytes(Config) },
                    new DemoContentSourceInput { Path = "docs/game-design/balance/chapter_model.py", Locator = "LEVELS[0,2] lines13,15; enemy lines49-65; run_battle lines86-122; base reward lines190-223", Sha256 = Sha("docs/game-design/balance/chapter_model.py") },
                    new DemoContentSourceInput { Path = "docs/game-design/balance/results/stages.csv", Locator = "stage=" + stage + ": recommended_level,base_xp,tin_per_win,wood_per_win,card_first_clear,proof_first_entry,proof_first_entry_quantity,proof_first_clear,proof_first_clear_quantity,unlock", Sha256 = Sha("docs/game-design/balance/results/stages.csv") },
                    new DemoContentSourceInput { Path = "docs/system-design/2026-09-16/details/content-validation.md", Locator = "§2 closure table; §3 origin ambiguity; §4 legal replay; §5 CV02–04", Sha256 = Sha("docs/system-design/2026-09-16/details/content-validation.md") } },
                SourceNotes = new List<string> { "Candidate only; coordinate origin, identities, C and epsilon require review.", "Isolated W; no official unlock, save or publication." },
                Growth = new GrowthDefinitionInput { ClassId = "candidate:warrior", ClassKind = CharacterClassKind.Warrior, PassiveDefinitionId = "candidate:warrior-crit",
                    BaseStats = Stats(100, 20, 10, 6), GrowthHp = R(2, 25), GrowthAttack = R(3, 50), GrowthDefense = R(3, 2),
                    CritBase = R(1, 5), CritStep = R(1, 200), CritCap = R(7, 20), CritMultiplier = R(3, 2),
                    XpBase = 60, XpLinear = 20, XpQuadratic = 5, RecoveryDurationMilliseconds = R(180000) },
                PlayerId = "isolated:P1", CharacterId = "W", CharacterLevel = 1, CharacterExperience = 0, OriginalSlot = 0,
                ChallengeId = "isolated:challenge:" + stage, AttemptId = "isolated:attempt:" + stage, EntryBaselineId = "isolated:entry:" + stage,
                Level = new LevelInput { LevelId = levelId, LevelVersion = "candidate-r1", RecommendedLevel = 1, Faces = new List<FaceInput> { face } },
                SourceRoutes = routes, CarryMode = EntryCarryMode.Empty, RequiredFeatures = new List<string>(), LearnedSkills = new List<string>(),
                Parameters = Candidates().Select(p => Copy(p.Proposed)).ToList(),
                Reward = new CandidateRewardDefinitionInput { RewardDefinitionId = "candidate:reward:L" + stage, Version = "candidate-r1", LevelId = levelId,
                    LevelVersion = "candidate-r1", BaseExperience = stage == 1 ? 20 : 24, DamageWeight = R(1), TakenWeight = R(1, 4),
                    CurveBase = R(1, 2), CurveLog = R(1, 2), ReferenceHpDivisor = R(2), LevelPenaltyBase = R(3, 4), OverlevelGrace = 1,
                    ZeroContributionPolicy = CandidateZeroContributionPolicy.CurveIntercept, DropMode = CandidateRewardDropMode.FixedOrdinaryMaterials,
                    RequiredFeatures = new List<string>(), Materials = new List<CandidateRewardMaterialInput> {
                        new CandidateRewardMaterialInput { ItemId = "candidate:tin", Amount = 2 }, new CandidateRewardMaterialInput { ItemId = "candidate:wood", Amount = 0 } } } };
        }
        private static FlowPos Map(FlowPos p, DemoCoordinateCandidate c)
        { return new FlowPos(p.x - 1, c == DemoCoordinateCandidate.AssumedTopLeft ? 4 - p.y : p.y - 1); }
        private static StatsInput Stats(int hp, int attack, int pd, int md)
        { return new StatsInput { MaxHp = R(hp), Attack = R(attack), PhysicalDefense = R(pd), MagicDefense = R(md), AttackRange = 1, Evasion = R(0) }; }
        internal static DemoPreparedContent Prepare(int stage = 1, DemoCoordinateCandidate coordinates = DemoCoordinateCandidate.AssumedBottomLeft)
        { var result = DemoContentCompiler.PrepareCandidate(MakeInput(stage, coordinates), Job(), Math());
            Assert.IsTrue(result.IsAccepted, result.RejectionCode + " " + result.FieldPath); return result.Candidate; }
        internal static CandidateSeedMaterial Seed()
        {
            // Declared before the first run, not selected by searching results. All three independent domains are nonempty.
            return new CandidateSeedMaterial { SourceCapabilityId = "isolated-declared-bytes-00-through-2f", MappingId = CandidateRandomPreparer.SupportedMappingId,
                Bytes = Enumerable.Range(0, 48).Select(i => (byte)i).ToArray() };
        }
        internal static CandidateReplayStep Select(DemoPreparedContent c, BattleSnapshot s)
        {
            // Explicit reference policy: WA; if A lives WA again; once A is dead WB. Never submit a dead target.
            var target = s.Enemies.First(e => e.Hp.Numerator.Sign > 0); var route = c.Routes.Single(r => r.FaceId == target.PairKey.FaceId && r.PairId == target.PairKey.PairId);
            return new CandidateReplayStep { Kind = CandidateBattleOperationKind.Attack, OperationId = "reference:" + s.EffectiveActionsCompleted,
                OccurredAtUnixMilliseconds = 1000 + s.EffectiveActionsCompleted, Actor = s.Members[0].CombatantKey, Pair = target.PairKey,
                Route = new List<FlowPos>(route.Cells) };
        }
        internal static DemoContentReplayResult Replay(DemoPreparedContent c)
        { using (var scope = Scope()) return DemoContentReplay.ReplayCandidate(c, Seed(), new CandidateBattleConditions { PreferenceRevision = 1, ItemUseEnabled = false },
            s => Select(c, s), 3, c.Job, scope); }
        internal static void WriteEvidence(string file, object data)
        {
            var text = Json(data) + "\n"; var path = Path.Combine(EvidenceRoot, file);
            // Later packet regressions may recompute but must not rewrite a frozen P1 artifact.
            if (File.Exists("docs/system-design/2026-09-17/demo-025-p1-scope.json"))
                Assert.AreEqual(text, File.ReadAllText(path), "frozen P1 evidence changed: " + file);
            else { Directory.CreateDirectory(EvidenceRoot); File.WriteAllText(path, text, new UTF8Encoding(false)); }
        }
        internal static string Json(object value)
        {
            if (value == null) return "null";
            if (value is string || value is Enum || value is BigInteger || value is ulong) return Quote(Convert.ToString(value, CultureInfo.InvariantCulture));
            if (value is bool flag) return flag ? "true" : "false";
            if (value is ExactRational r) return "{\"numerator\":" + Quote(r.Numerator.ToString(CultureInfo.InvariantCulture)) + ",\"denominator\":" + Quote(r.Denominator.ToString(CultureInfo.InvariantCulture)) + "}";
            if (value is byte || value is int || value is uint || value is long) return Convert.ToString(value, CultureInfo.InvariantCulture);
            if (value is IEnumerable list) return "[" + string.Join(",", list.Cast<object>().Select(Json)) + "]";
            var fields = value.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.GetIndexParameters().Length == 0)
                .OrderBy(p => p.Name, StringComparer.Ordinal).Select(p => Quote(p.Name) + ":" + Json(p.GetValue(value))).ToList();
            fields.AddRange(value.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance).OrderBy(p => p.Name, StringComparer.Ordinal)
                .Select(p => Quote(p.Name) + ":" + Json(p.GetValue(value))));
            return "{" + string.Join(",", fields) + "}";
        }
        private static string Quote(string value)
        {
            var b = new StringBuilder("\"");
            foreach (var c in value) { if (c == '\\' || c == '"') b.Append('\\').Append(c); else if (c < 32) b.Append("\\u").Append(((int)c).ToString("x4")); else b.Append(c); }
            return b.Append('"').ToString();
        }
    }
}
