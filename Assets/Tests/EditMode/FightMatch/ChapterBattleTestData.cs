using System.Collections.Generic;
using System.Numerics;
using FlowPuzzle.Core;
using NUnit.Framework;

namespace FightMatch.Core.Tests
{
    internal static class ChapterBattleTestData
    {
        internal static ExactMathBudget Math() => new ExactMathBudget(maxPrimitiveSteps: 16000000);
        internal static ExactRational R(BigInteger numerator, BigInteger? denominator = null) => ExactRational.Create(numerator, denominator ?? BigInteger.One, Math());
        internal static BattleEntryInput Input(int pairs = 1, bool warrior = false, string rule = "ch1-slice-r1")
        {
            var content = ContentBinding.Prepare("fixture:chapter-battle", "fixture:chapter-battle-fingerprint", rule,
                "exact-test", "pcg-test", new SaveCodecBudget(Math()));
            Assert.IsTrue(content.IsAccepted, content.FieldPath);
            var context = new PublishedRuleContext(content.Value);
            var input = new BattleEntryInput
            {
                PlayerId = "player", ChallengeId = "challenge", AttemptId = "attempt", EntryBaselineId = "entry",
                Context = context, CarryMode = EntryCarryMode.Empty, RequiredFeatures = new List<string>(),
                Level = new LevelInput { LevelId = "fixture:9", LevelVersion = "1", RecommendedLevel = 3,
                    Faces = new List<FaceInput> { new FaceInput { FaceId = "face", Width = 3, Height = pairs * 2 - 1, Pairs = new List<PairInput>() } } },
                Members = new List<MemberInput>()
            };
            if (warrior)
                input.Members.Add(new MemberInput { CharacterId = "warrior", ClassId = "warrior", ClassKind = CharacterClassKind.Warrior,
                    OriginalSlot = 0, Level = 3, IsReady = true, StatsOrigin = BaseStatsOrigin.ComputedBaseStats,
                    StatsContext = context, Stats = Stats(100, R(22), 1), EntryHp = R(100), LearnedSkills = new List<string>(),
                    Crit = new WarriorCritInput { PassiveDefinitionId = "fixture:crit", TargetProbability = R(1, 5), C = R(1, 10), Multiplier = R(3, 2) } });
            input.Members.Add(new MemberInput { CharacterId = "mage", ClassId = "mage", ClassKind = CharacterClassKind.Mage,
                OriginalSlot = 1, Level = 3, IsReady = true, StatsOrigin = BaseStatsOrigin.ComputedBaseStats,
                StatsContext = context, Stats = Stats(80, R(616, 25), 2), EntryHp = R(80), LearnedSkills = new List<string>(),
                Penetration = new ArcanePenetrationInput { PassiveDefinitionId = "fixture:penetration", TargetProbability = R(21, 100),
                    C = R(1, 10), IgnoreMagicDefenseFraction = R(1, 2) } });
            for (var i = 0; i < pairs; i++)
            {
                var stats = Stats(20, R(4), 3); stats.PhysicalDefense = R(200); stats.MagicDefense = R(5);
                input.Level.Faces[0].Pairs.Add(new PairInput { PairId = "pair:" + i, GeometryColorId = i,
                    EndpointA = new FlowPos(0, i * 2), EndpointB = new FlowPos(2, i * 2),
                    Enemy = new EnemyInput { EnemyInstanceKey = "enemy:" + i, EnemyDefinitionId = "fixture:normal",
                        OriginalSlot = i * 3, StableOrder = i, Behavior = EnemyBehavior.NormalStrike, Stats = stats,
                        IntentCycle = new List<EnemyIntentInput> { new EnemyIntentInput { Kind = EnemyIntentKind.Strike,
                            Targeting = EnemyTargeting.FirstLiving, DamageKind = EntryDamageKind.Physical, DamageCoefficient = R(1) } } } });
            }
            return input;
        }
        internal static StatsInput Stats(int hp, ExactRational attack, int range) => new StatsInput
        { MaxHp = R(hp), Attack = attack, PhysicalDefense = R(0), MagicDefense = R(0), Evasion = R(0), AttackRange = range };
        internal static BattleEntryPreparationResult Prepare(BattleEntryInput input)
        {
            var binding = DefinitionBinding.Prepare(((PublishedRuleContext)input.Context).Binding, input.Level.LevelId, 1, new SaveCodecBudget(Math()));
            Assert.IsTrue(binding.IsAccepted, binding.FieldPath);
            return new BattleEntryPreparer().PreparePublished(input, binding.Value, new PreparedLevel(input.Level), Math());
        }
        internal static CandidateRandomBinding Bind(BattleEntryInput input = null)
        {
            var prepared = Prepare(input ?? Input());
            Assert.IsTrue(prepared.IsAccepted, prepared.RejectionCode + " " + prepared.FieldPath);
            // Reference PCG seed42/sequence54; synthetic C is explicit and is not a calibration claim.
            var bytes = new byte[48]; for (var i = 0; i < 48; i += 16) { bytes[i] = 42; bytes[i + 8] = 54; }
            var random = CandidateRandomPreparer.Prepare(prepared.Entry, new CandidateSeedMaterial
            { Bytes = bytes, SourceCapabilityId = "fixture:chapter-battle", MappingId = CandidateRandomPreparer.SupportedMappingId }, Math());
            Assert.IsTrue(random.IsAccepted, random.RejectionCode + " " + random.FieldPath);
            return random.Binding;
        }
        internal static CandidateAttackRequest Attack(BattleSnapshot state, int pair = 0, string actor = "mage", string operation = "attack:1")
        {
            var definition = state.Board.Face.Pairs[pair];
            return new CandidateAttackRequest { PlayerId = state.Baseline.Entry.PlayerId, AttemptId = state.Baseline.Entry.AttemptId,
                OperationId = operation, ExpectedSceneRevision = state.SceneRevision,
                Actor = BattleCombatantKey.ForParticipant(state.Baseline.Entry.AttemptId, actor),
                Pair = BattlePairKey.Create(state.Baseline.Entry.AttemptId, state.Board.Face.FaceId, definition.PairId),
                Route = new List<FlowPos> { definition.EndpointA, new FlowPos(1, definition.EndpointA.y), definition.EndpointB } };
        }
        internal static CandidateBattleRun Run(CandidateRandomBinding binding)
        {
            var result = CandidateBattleOperations.CreateCandidate(binding, Math());
            Assert.IsTrue(result.IsAccepted, result.RejectionCode + " " + result.FieldPath); return result.Run;
        }
        internal static CandidateBattleRun Act(CandidateBattleRun run, int pair, string operation, string actor = "mage")
        {
            var result = CandidateBattleOperations.EvaluateAttack(run, Attack(run.CurrentSnapshot, pair, actor, operation),
                new CandidateBattleConditions { PreferenceRevision = 1, ItemUseEnabled = false }, 100, new RandomSamplingBudget(Math()));
            Assert.IsTrue(result.IsAccepted, result.RejectionCode + " " + result.FieldPath); return result.NextRun;
        }
        internal static void Value(ExactRational value, BigInteger n, BigInteger? d = null)
        { Assert.AreEqual(n, value.Numerator); Assert.AreEqual(d ?? BigInteger.One, value.Denominator); }
    }
}
