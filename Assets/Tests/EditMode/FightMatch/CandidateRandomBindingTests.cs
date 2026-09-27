using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Reflection;
using FlowPuzzle.Core;
using NUnit.Framework;
using static FightMatch.Core.BattleEntryRejectionCode;

namespace FightMatch.Core.Tests
{
    [TestFixture]
    public sealed class CandidateRandomBindingTests
    {
        [Test]
        public void Sc01_ZeroBlocksAndFullWidthLittleEndianMapping()
        {
            var entry = PrepareEntry(Candidate());
            var material = Material(0, 0);
            var binding = Bind(entry, material);
            foreach (var domain in Domains(binding))
            {
                Assert.AreEqual(0UL, domain.InitState);
                Assert.AreEqual(0UL, domain.InitSequence);
                Assert.AreEqual(6364136223846793006UL, domain.Initial.Initial.State);
                Assert.AreEqual(1UL, domain.Initial.Initial.Increment);
                Assert.AreEqual(BigInteger.Zero, domain.Initial.WordsConsumed);
                Assert.AreSame(domain.Initial.Initial, domain.Initial.Current);
            }
            Assert.AreNotSame(binding.Battle.Initial, binding.BaseReward.Initial);
            for (var i = 0; i < 16; i++) material.Bytes[i] = (byte)i;
            var little = Bind(entry, material).Battle;
            Assert.AreEqual(0x0706050403020100UL, little.InitState);
            Assert.AreEqual(0x0f0e0d0c0b0a0908UL, little.InitSequence);
            Assert.AreEqual(0x1e1c1a1816141211UL, little.Initial.Initial.Increment);
            var high = Material(0, 1UL << 63);
            var mapped = Bind(entry, high, new ExactMathBudget(63));
            Assert.AreEqual(Describe(binding.Start), Describe(mapped.Start));
            Assert.AreEqual(0UL, mapped.Battle.InitSequence);
            Assert.AreEqual(128, high.Bytes[15]);
            Assert.Throws<ArgumentOutOfRangeException>(() => Pcg32StreamState.Initialize(0, 1UL << 63));
            material.Bytes = Enumerable.Repeat((byte)255, 48).ToArray();
            foreach (var domain in Domains(Bind(entry, material)))
            {
                Assert.AreEqual(ulong.MaxValue, domain.InitState);
                Assert.AreEqual(ulong.MaxValue >> 1, domain.InitSequence);
                Assert.AreEqual(ulong.MaxValue, domain.Initial.Initial.Increment);
                Assert.AreEqual(BigInteger.Zero, domain.Initial.WordsConsumed);
            }
        }

        [TestCase(1, 1)]
        [TestCase(1, 2)]
        [TestCase(3, 1)]
        [TestCase(3, 2)]
        public void SourceCandidates_Keep007BStateAndOriginalGenerationRelations(int stage, int direction)
        {
            var entry = PrepareEntry(Candidate(stage, direction));
            var material = Material();
            WriteBlock(material.Bytes, 16, 17, 19);
            WriteBlock(material.Bytes, 32, 23, 29);
            var beforeEntry = Describe(entry);
            var beforeMaterial = Describe(material);
            var binding = Bind(entry, material);
            var domains = Domains(binding);
            CollectionAssert.AreEqual(new[] { CandidateRandomPurpose.Battle, CandidateRandomPurpose.BaseReward, CandidateRandomPurpose.Bonus }, domains.Select(d => d.Purpose));
            CollectionAssert.AreEqual(new ulong[] { 42, 17, 23 }, domains.Select(d => d.InitState));
            CollectionAssert.AreEqual(new ulong[] { 54, 19, 29 }, domains.Select(d => d.InitSequence));
            Assert.AreSame(entry.Context, binding.Context);
            Assert.AreEqual(entry.PlayerId, binding.GeneratedForPlayerId);
            Assert.AreEqual(entry.ChallengeId, binding.GeneratedForChallengeId);
            Assert.AreEqual(entry.AttemptId, binding.GeneratedForAttemptId);
            Assert.AreEqual(entry.EntryBaselineId, binding.GeneratedForEntryBaselineId);
            Assert.AreEqual(material.SourceCapabilityId, binding.SourceCapabilityId);
            Assert.AreEqual(CandidateRandomPreparer.SupportedMappingId, binding.MappingId);
            Assert.AreSame(binding.Battle.Initial, binding.Start.Baseline.RandomInitials.Battle);
            Assert.AreSame(binding.BaseReward.Initial, binding.Start.Baseline.RandomInitials.BaseReward);
            Assert.AreSame(binding.Bonus.Initial, binding.Start.Baseline.RandomInitials.Bonus);
            Assert.AreEqual(0xa15c02b7U, Pcg32Core.Next32(binding.Battle.Initial.Current, out _));
            var expected = BattleStartAssembler.CreateCandidate(entry, new CandidateRandomInitials
            {
                Battle = domains[0].Initial, BaseReward = domains[1].Initial, Bonus = domains[2].Initial,
                PrdStates = new List<CandidatePrdInitial> { new CandidatePrdInitial
                { CharacterId = entry.ReadyParticipants[0].CharacterId, PassiveDefinitionId = entry.ReadyParticipants[0].Crit.PassiveDefinitionId, FailureCount = 0 } }
            }, new ExactMathBudget());
            Assert.IsTrue(expected.IsAccepted);
            Assert.AreEqual(Describe(expected.Start), Describe(binding.Start));
            Assert.AreSame(entry, binding.Start.Baseline.Entry);
            Assert.AreSame(binding.Start.Baseline, binding.Start.Snapshot.Baseline);
            Assert.IsEmpty(binding.Start.Snapshot.Board.LockedRoutes);
            Assert.IsEmpty(binding.Start.Snapshot.Board.PendingLinks);
            Assert.AreEqual(BigInteger.One, binding.Start.Snapshot.SceneRevision);
            Assert.AreEqual(BigInteger.Zero, binding.Start.Snapshot.EffectiveActionsCompleted);
            Assert.AreEqual(BattlePhase.AwaitAction, binding.Start.Snapshot.Phase);
            Value(binding.Start.Snapshot.Members[0].Hp, 97, 2);
            Assert.AreEqual(beforeEntry, Describe(entry));
            Assert.AreEqual(beforeMaterial, Describe(material));
        }

        [TestCase("Bytes", null, MissingField)]
        [TestCase("SourceCapabilityId", null, MissingField)]
        [TestCase("SourceCapabilityId", "", MissingField)]
        [TestCase("SourceCapabilityId", " \t", MissingField)]
        [TestCase("MappingId", null, MissingField)]
        [TestCase("MappingId", "", MissingField)]
        [TestCase("MappingId", "unknown", UnsupportedBinding)]
        [TestCase("MappingId", "SC01-PCG32-LE128-V1", UnsupportedBinding)]
        [TestCase("MappingId", " sc01-pcg32-le128-v1", UnsupportedBinding)]
        public void MaterialMetadata_IsExplicitAndOrdinal(string field, string value, BattleEntryRejectionCode code)
        {
            var entry = PrepareEntry(Candidate());
            var material = Material();
            Set(material, field, value);
            RejectMaterial(entry, material, code, "Material." + field);
        }

        [TestCase(0)]
        [TestCase(47)]
        [TestCase(49)]
        public void MaterialLength_MustBeExactly48(int length)
        {
            var entry = PrepareEntry(Candidate());
            var material = Material();
            material.Bytes = new byte[length];
            RejectMaterial(entry, material, InvalidValue, "Material.Bytes");
        }

        [Test]
        public void PreparationOrder_IsRootsThenMetadataLengthMappingAndNumericWork()
        {
            var entry = PrepareEntry(Candidate());
            var material = new CandidateSeedMaterial();
            Assert.AreEqual("entry", Assert.Throws<ArgumentNullException>(() => CandidateRandomPreparer.Prepare(null, null, null)).ParamName);
            Assert.AreEqual("material", Assert.Throws<ArgumentNullException>(() => CandidateRandomPreparer.Prepare(entry, null, null)).ParamName);
            Assert.AreEqual("budget", Assert.Throws<ArgumentNullException>(() => CandidateRandomPreparer.Prepare(entry, material, null)).ParamName);
            RejectMaterial(entry, material, MissingField, "Material.SourceCapabilityId");
            material.SourceCapabilityId = " synthetic capability ";
            RejectMaterial(entry, material, MissingField, "Material.MappingId");
            material.MappingId = "unknown";
            RejectMaterial(entry, material, MissingField, "Material.Bytes");
            material.Bytes = new byte[47];
            RejectMaterial(entry, material, InvalidValue, "Material.Bytes");
            material.Bytes = new byte[48];
            RejectMaterial(entry, material, UnsupportedBinding, "Material.MappingId");
            material.MappingId = CandidateRandomPreparer.SupportedMappingId;
            Assert.AreEqual(" synthetic capability ", Bind(entry, material).SourceCapabilityId);
            CandidateRandomPreparationResult result = null;
            Assert.Throws<ExactMathLimitException>(() => result = CandidateRandomPreparer.Prepare(entry, material, new ExactMathBudget(256, 0)));
            Assert.IsNull(result);
        }

        [Test]
        public void PublishedPcgVector_ProducesTwoFailuresThenSuccessWithCompleteFacts()
        {
            var binding = Bind(PrepareEntry(Candidate()), Material());
            var original = Describe(binding);
            var before = binding.Start.Snapshot.Random;
            var budget = new RandomSamplingBudget(new ExactMathBudget(), 3);
            var words = new[] { 0xa15c02b7U, 0x7b47f409U, 0xba1d3330U };
            for (var i = 0; i < 3; i++)
            {
                var frozen = Describe(before);
                var result = Evaluate(binding, before, i, budget);
                var fact = result.Fact;
                Assert.AreSame(Actor(binding), fact.Actor);
                Assert.AreSame(Target(binding), fact.Target);
                Assert.AreEqual(new BigInteger(i), fact.OpportunityOrdinal);
                Assert.AreSame(binding.Start.Baseline.Entry.ReadyParticipants[0].Crit, fact.Parameters);
                Assert.AreEqual(new BigInteger(i), fact.FailureCountBefore);
                Assert.AreEqual(new BigInteger(i == 2 ? 0 : i + 1), fact.FailureCountAfter);
                Assert.AreEqual(i == 2, fact.Triggered);
                Value(fact.Probability, i == 1 ? 1 : i + 1, i == 1 ? 2 : 4);
                CollectionAssert.AreEqual(new[] { words[i] }, fact.Words);
                Assert.AreSame(before.Stream, fact.StreamBefore);
                Assert.AreSame(result.Next.Stream, fact.StreamAfter);
                Assert.AreEqual(new BigInteger(i + 1), result.Next.Stream.WordsConsumed);
                Assert.AreEqual(fact.FailureCountAfter, result.Next.PrdStates[0].FailureCount);
                Assert.AreEqual(frozen, Describe(before));
                AssertReadOnlyGraph(result);
                before = result.Next;
            }
            Assert.AreEqual(3, budget.WordsUsed);
            Assert.AreEqual(original, Describe(binding));
            Assert.AreEqual(BigInteger.Zero, binding.BaseReward.Initial.WordsConsumed);
            Assert.AreEqual(BigInteger.Zero, binding.Bonus.Initial.WordsConsumed);
        }

        [Test]
        public void CertainCrit_ResetsWithoutTakingAWordOrResettingCallerBudget()
        {
            var input = Candidate(); input.Members[0].Crit.C = R(1);
            var binding = Bind(PrepareEntry(input), Material());
            var before = binding.Start.Snapshot.Random;
            var zeroWords = new RandomSamplingBudget(new ExactMathBudget(), 0);
            var certain = Evaluate(binding, before, BigInteger.One << 80, zeroWords);
            Assert.IsTrue(certain.Fact.Triggered);
            Value(certain.Fact.Probability, 1);
            Assert.AreEqual(BigInteger.Zero, certain.Next.PrdStates[0].FailureCount);
            Assert.AreEqual(BigInteger.One << 80, certain.Fact.OpportunityOrdinal);
            Assert.IsEmpty(certain.Fact.Words);
            Assert.AreEqual(0, zeroWords.WordsUsed);
            Assert.AreSame(before.Stream, certain.Next.Stream);
            var used = new RandomSamplingBudget(new ExactMathBudget(), 1);
            ExactRandomSampler.Uniform(BigInteger.One << 32, before.Stream, used);
            Evaluate(binding, before, 0, used);
            Assert.AreEqual(1, used.WordsUsed);
        }

        [Test]
        public void FactIncludesRejectedRawWords_AndExhaustionKeepsOriginalState()
        {
            var input = Candidate(); input.Members[0].Crit.C = R(1, 3);
            // This SC01 seed initializes the accepted PCG core to (state=0, increment=1).
            var binding = Bind(PrepareEntry(input), Material(4568919932995229530UL, 0));
            var before = binding.Start.Snapshot.Random;
            Assert.AreEqual(0UL, before.Stream.Current.State);
            var original = Describe(before);
            var result = Evaluate(binding, before, 0);
            Assert.AreEqual(3, result.Fact.Words.Count);
            Assert.AreEqual(0U, result.Fact.Words[0]);
            Assert.AreEqual(0U, result.Fact.Words[1]);
            Assert.AreNotEqual(0U, result.Fact.Words[2]);
            Assert.AreEqual(new BigInteger(3), result.Next.Stream.WordsConsumed);
            CandidateCritResult partial = null;
            var tight = new RandomSamplingBudget(new ExactMathBudget(), 2);
            var error = Assert.Throws<ExactMathLimitException>(() => partial = CandidateWarriorCritEvaluator.EvaluateOpportunity(
                binding, before, Actor(binding), Target(binding), 0, tight));
            Assert.AreEqual("RandomWords", error.ReasonCode);
            Assert.AreEqual(2, tight.WordsUsed);
            Assert.IsNull(partial);
            Assert.AreEqual(original, Describe(before));
            Assert.AreEqual(Describe(result), Describe(Evaluate(binding, before, 0)));
        }

        [TestCase("ActorAttempt")]
        [TestCase("ActorCharacter")]
        [TestCase("ActorCase")]
        [TestCase("ActorSpace")]
        [TestCase("ActorKind")]
        [TestCase("TargetAttempt")]
        [TestCase("TargetFace")]
        [TestCase("TargetEnemy")]
        [TestCase("TargetKind")]
        [TestCase("Ordinal")]
        public void OpportunityIdentity_IsBoundToOriginalActorTargetAndOrdinal(string difference)
        {
            var binding = Bind(PrepareEntry(Candidate()), Material());
            var actor = Actor(binding); var target = Target(binding); BigInteger ordinal = 0;
            switch (difference)
            {
                case "ActorAttempt": actor = BattleCombatantKey.ForParticipant("other", actor.CharacterId); break;
                case "ActorCharacter": actor = BattleCombatantKey.ForParticipant(actor.AttemptId, "other"); break;
                case "ActorCase": actor = BattleCombatantKey.ForParticipant(actor.AttemptId, actor.CharacterId.ToUpperInvariant()); break;
                case "ActorSpace": actor = BattleCombatantKey.ForParticipant(actor.AttemptId, " " + actor.CharacterId); break;
                case "ActorKind": actor = target; break;
                case "TargetAttempt": target = BattleCombatantKey.ForEnemy("other", target.FaceId, target.EnemyInstanceKey); break;
                case "TargetFace": target = BattleCombatantKey.ForEnemy(target.AttemptId, "other", target.EnemyInstanceKey); break;
                case "TargetEnemy": target = BattleCombatantKey.ForEnemy(target.AttemptId, target.FaceId, "other"); break;
                case "TargetKind": target = actor; break;
                case "Ordinal": ordinal = -1; break;
            }
            RejectCrit(binding, binding.Start.Snapshot.Random, actor, target, ordinal,
                difference == "Ordinal" ? InvalidValue : InconsistentBinding,
                difference == "Ordinal" ? "OpportunityOrdinal" : difference.StartsWith("Actor") ? "Actor" : "Target");
        }

        [TestCase("InitialState", "Before.Stream.Initial")]
        [TestCase("InitialIncrement", "Before.Stream.Initial")]
        [TestCase("Attempt", "Before.PrdStates[0].CombatantKey")]
        [TestCase("Character", "Before.PrdStates[0].CombatantKey")]
        [TestCase("PassiveDefinitionId", "Before.PrdStates[0].Crit.PassiveDefinitionId")]
        [TestCase("TargetProbability", "Before.PrdStates[0].Crit.TargetProbability")]
        [TestCase("C", "Before.PrdStates[0].Crit.C")]
        [TestCase("Multiplier", "Before.PrdStates[0].Crit.Multiplier")]
        public void OtherValidCandidates_CannotSupplyMismatchedCurrentState(string difference, string path)
        {
            var binding = Bind(PrepareEntry(Candidate()), Material());
            var other = Candidate(); var material = Material();
            switch (difference)
            {
                case "InitialState": WriteBlock(material.Bytes, 0, 43, 54); break;
                case "InitialIncrement": WriteBlock(material.Bytes, 0, 42, 55); break;
                case "Attempt": other.AttemptId += ":other"; break;
                case "Character": other.Members[0].CharacterId += ":other"; break;
                case "PassiveDefinitionId": other.Members[0].Crit.PassiveDefinitionId += ":other"; break;
                case "TargetProbability": other.Members[0].Crit.TargetProbability = R(1, 3); break;
                case "C": other.Members[0].Crit.C = R(1, 3); break;
                case "Multiplier": other.Members[0].Crit.Multiplier = R(5, 2); break;
            }
            var before = Bind(PrepareEntry(other), material).Start.Snapshot.Random;
            RejectCrit(binding, before, Actor(binding), Target(binding), 0, InconsistentBinding, path);
        }

        [Test]
        public void EqualParametersFromAnotherCandidate_AreComparedByExactValue()
        {
            var binding = Bind(PrepareEntry(Candidate()), Material());
            var other = Candidate(); other.Members[0].Crit.C = R(2, 8);
            var before = Bind(PrepareEntry(other), Material()).Start.Snapshot.Random;
            Assert.AreNotSame(binding.Start.Snapshot.Random.PrdStates[0].Crit, before.PrdStates[0].Crit);
            var result = Evaluate(binding, before, 0);
            Assert.AreSame(binding.Start.Snapshot.Random.PrdStates[0].Crit, result.Fact.Parameters);
            Assert.AreSame(result.Fact.Parameters, result.Next.PrdStates[0].Crit);
        }

        [Test]
        public void DefinedLaterFaceTarget_IsKnownButThisCalculatorDoesNotProveItsAvailability()
        {
            var input = Candidate(3);
            var later = Candidate().Level.Faces[0]; later.FaceId = "face:later";
            input.Level.Faces.Add(later);
            var binding = Bind(PrepareEntry(input), Material());
            var target = BattleCombatantKey.ForEnemy(input.AttemptId, later.FaceId, later.Pairs[0].Enemy.EnemyInstanceKey);
            var result = CandidateWarriorCritEvaluator.EvaluateOpportunity(binding, binding.Start.Snapshot.Random,
                Actor(binding), target, 0, new RandomSamplingBudget(new ExactMathBudget()));
            Assert.IsTrue(result.IsAccepted);
            Assert.AreEqual(target, result.Fact.Target);
            Assert.AreEqual(0, binding.Start.Snapshot.CurrentFaceIndex);
            Assert.AreEqual(BigInteger.One, binding.Start.Snapshot.SceneRevision);
        }

        [Test]
        public void OpportunityNullRoots_AreArgumentErrors()
        {
            var b = Bind(PrepareEntry(Candidate()), Material());
            var s = b.Start.Snapshot.Random; var a = Actor(b); var t = Target(b);
            var budget = new RandomSamplingBudget(new ExactMathBudget());
            Assert.Throws<ArgumentNullException>(() => CandidateWarriorCritEvaluator.EvaluateOpportunity(null, s, a, t, 0, budget));
            Assert.Throws<ArgumentNullException>(() => CandidateWarriorCritEvaluator.EvaluateOpportunity(b, null, a, t, 0, budget));
            Assert.Throws<ArgumentNullException>(() => CandidateWarriorCritEvaluator.EvaluateOpportunity(b, s, null, t, 0, budget));
            Assert.Throws<ArgumentNullException>(() => CandidateWarriorCritEvaluator.EvaluateOpportunity(b, s, a, null, 0, budget));
            Assert.Throws<ArgumentNullException>(() => CandidateWarriorCritEvaluator.EvaluateOpportunity(b, s, a, t, 0, null));
            Assert.AreEqual(0, budget.Math.PrimitiveStepsUsed);
            Assert.AreEqual(0, budget.WordsUsed);
        }

        [TestCase("Members[0].Level")]
        [TestCase("Level.RecommendedLevel")]
        [TestCase("Context.DraftRevision")]
        [TestCase("Members[0].Stats.Attack")]
        [TestCase("Members[0].EntryHp")]
        [TestCase("Members[0].Crit.C")]
        [TestCase("Members[0].Crit.TargetProbability")]
        [TestCase("Members[0].Crit.Multiplier")]
        [TestCase("Level.Faces[1].Pairs[0].Enemy.Stats.MaxHp")]
        [TestCase("Level.Faces[1].Pairs[0].Enemy.IntentCycle[0].DamageCoefficient")]
        public void BothEntrypoints_RecheckLargeRetainedPreparedNumbers(string path)
        {
            var input = Candidate(3); var later = Candidate().Level.Faces[0]; later.FaceId = "later";
            input.Level.Faces.Add(later);
            var huge = (BigInteger.One << 80) + 1;
            if (Get(input, path) is BigInteger) Set(input, path, huge);
            else Set(input, path, ExactRational.Create(1, huge, new ExactMathBudget()));
            input.Members[0].StatsContext = CopyContext(input.Context);
            var entry = PrepareEntry(input); var material = Material();
            CandidateRandomPreparationResult prepared = null;
            Assert.AreEqual("IntegerBits", Assert.Throws<ExactMathLimitException>(() => prepared =
                CandidateRandomPreparer.Prepare(entry, material, new ExactMathBudget(64))).ReasonCode);
            Assert.IsNull(prepared);
            var binding = Bind(entry, material);
            CandidateCritResult result = null;
            Assert.AreEqual("IntegerBits", Assert.Throws<ExactMathLimitException>(() => result = CandidateWarriorCritEvaluator.EvaluateOpportunity(
                binding, binding.Start.Snapshot.Random, Actor(binding), Target(binding), 0, new RandomSamplingBudget(new ExactMathBudget(64)))).ReasonCode);
            Assert.IsNull(result);
        }

        [TestCase(0)]
        [TestCase(16)]
        [TestCase(32)]
        public void EachDomain_RetainedSeedAndStreamValuesUseThisCallsBudget(int offset)
        {
            var entry = PrepareEntry(Candidate()); var material = Material(0, 0);
            WriteBlock(material.Bytes, offset, ulong.MaxValue, 54);
            Assert.Throws<ExactMathLimitException>(() => CandidateRandomPreparer.Prepare(entry, material, new ExactMathBudget(63)));
            var binding = Bind(entry, material);
            Assert.Throws<ExactMathLimitException>(() => CandidateWarriorCritEvaluator.EvaluateOpportunity(binding,
                binding.Start.Snapshot.Random, Actor(binding), Target(binding), 0, new RandomSamplingBudget(new ExactMathBudget(63))));
        }

        [Test]
        public void BeforePrdValuesAndOrdinal_AreRecheckedBeforeSampling()
        {
            var binding = Bind(PrepareEntry(Candidate()), Material());
            var other = Candidate(); other.Members[0].Crit.C = ExactRational.Create(1, BigInteger.One << 80, new ExactMathBudget());
            var before = Bind(PrepareEntry(other), Material()).Start.Snapshot.Random;
            var budget = new RandomSamplingBudget(new ExactMathBudget(64));
            Assert.Throws<ExactMathLimitException>(() => CandidateWarriorCritEvaluator.EvaluateOpportunity(binding, before, Actor(binding), Target(binding), 0, budget));
            Assert.AreEqual(0, budget.WordsUsed);
            Assert.Throws<ExactMathLimitException>(() => CandidateWarriorCritEvaluator.EvaluateOpportunity(binding,
                binding.Start.Snapshot.Random, Actor(binding), Target(binding), BigInteger.One << 80, new RandomSamplingBudget(new ExactMathBudget(64))));
        }

        [Test]
        public void SharedBudgets_RejectWithoutOutputsAndRetryWithIdenticalValues()
        {
            var entry = PrepareEntry(Candidate()); var material = Material();
            var math = new ExactMathBudget(); var binding = Bind(entry, material, math);
            CandidateRandomPreparationResult partialBinding = null;
            Assert.Throws<ExactMathLimitException>(() => partialBinding = CandidateRandomPreparer.Prepare(entry, material,
                new ExactMathBudget(256, checked((int)math.PrimitiveStepsUsed - 1))));
            Assert.IsNull(partialBinding);
            Assert.AreEqual(Describe(binding), Describe(Bind(entry, material)));
            var before = binding.Start.Snapshot.Random;
            var measured = new RandomSamplingBudget(new ExactMathBudget());
            var expected = Evaluate(binding, before, 0, measured);
            var tight = new RandomSamplingBudget(new ExactMathBudget(256, checked((int)measured.Math.PrimitiveStepsUsed + 1)));
            ExactRational.Create(0, 1, tight.Math);
            CandidateCritResult partial = null;
            Assert.AreEqual("PrimitiveSteps", Assert.Throws<ExactMathLimitException>(() => partial = CandidateWarriorCritEvaluator.EvaluateOpportunity(
                binding, before, Actor(binding), Target(binding), 0, tight)).ReasonCode);
            Assert.IsNull(partial);
            var words = new RandomSamplingBudget(new ExactMathBudget(), 1);
            var first = Evaluate(binding, before, 0, words);
            var frozen = Describe(first.Next);
            Assert.AreEqual("RandomWords", Assert.Throws<ExactMathLimitException>(() => CandidateWarriorCritEvaluator.EvaluateOpportunity(
                binding, first.Next, Actor(binding), Target(binding), 1, words)).ReasonCode);
            Assert.AreEqual(1, words.WordsUsed);
            Assert.AreEqual(frozen, Describe(first.Next));
            Assert.AreEqual(Describe(expected), Describe(Evaluate(binding, before, 0)));
        }

        [Test]
        public void MaterialMutationAndNewAttempt_DoNotChangeOriginalBindingOrSaltTheSeed()
        {
            var input = Candidate(); var entry = PrepareEntry(input); var material = Material();
            var first = Bind(entry, material); var original = Describe(first);
            Assert.AreEqual(original, Describe(Bind(entry, material)));
            var bytes = material.Bytes;
            Array.Clear(bytes, 0, bytes.Length);
            material.Bytes = new byte[1]; material.SourceCapabilityId = "changed"; material.MappingId = "changed";
            Assert.AreEqual(original, Describe(first));
            input.AttemptId = "next-attempt";
            var next = Bind(PrepareEntry(input), Material());
            Assert.AreEqual(Describe(first.Battle), Describe(next.Battle));
            Assert.AreNotEqual(first.GeneratedForAttemptId, next.GeneratedForAttemptId);
            Assert.AreEqual("candidate:attempt", first.GeneratedForAttemptId);
            AssertReadOnlyGraph(first);
            Assert.IsNull(typeof(CandidateRandomBinding).GetProperty("Bytes"));
            CollectionAssert.AreEquivalent(new[] { "Stream", "PrdStates" }, typeof(BattleRandomSnapshot).GetProperties().Select(p => p.Name));
            CollectionAssert.AreEquivalent(new[] { "IsAccepted", "Next", "Fact", "RejectionCode", "FieldPath" }, typeof(CandidateCritResult).GetProperties().Select(p => p.Name));
        }

        private static CandidateRandomDomain[] Domains(CandidateRandomBinding binding) { return new[] { binding.Battle, binding.BaseReward, binding.Bonus }; }
        private static BattleCombatantKey Actor(CandidateRandomBinding b) { return b.Start.Snapshot.Members[0].CombatantKey; }
        private static BattleCombatantKey Target(CandidateRandomBinding b) { return b.Start.Snapshot.Enemies[0].CombatantKey; }
        private static CandidateCritResult Evaluate(CandidateRandomBinding b, BattleRandomSnapshot before, BigInteger ordinal, RandomSamplingBudget budget = null)
        {
            var result = CandidateWarriorCritEvaluator.EvaluateOpportunity(b, before, Actor(b), Target(b), ordinal, budget ?? new RandomSamplingBudget(new ExactMathBudget()));
            Assert.IsTrue(result.IsAccepted, result.RejectionCode + " " + result.FieldPath);
            Assert.IsNull(result.RejectionCode); Assert.IsNull(result.FieldPath);
            return result;
        }

        private static CandidateRandomBinding Bind(PreparedBattleEntry entry, CandidateSeedMaterial material, ExactMathBudget budget = null)
        {
            var result = CandidateRandomPreparer.Prepare(entry, material, budget ?? new ExactMathBudget());
            Assert.IsTrue(result.IsAccepted, result.RejectionCode + " " + result.FieldPath);
            Assert.IsNull(result.RejectionCode); Assert.IsNull(result.FieldPath);
            return result.Binding;
        }

        private static void RejectMaterial(PreparedBattleEntry entry, CandidateSeedMaterial material, BattleEntryRejectionCode code, string path)
        {
            var oldEntry = Describe(entry); var oldMaterial = Describe(material);
            var result = CandidateRandomPreparer.Prepare(entry, material, new ExactMathBudget());
            Assert.IsFalse(result.IsAccepted); Assert.IsNull(result.Binding);
            Assert.AreEqual(code, result.RejectionCode); Assert.AreEqual(path, result.FieldPath);
            Assert.AreEqual(oldEntry, Describe(entry)); Assert.AreEqual(oldMaterial, Describe(material));
        }

        private static void RejectCrit(CandidateRandomBinding binding, BattleRandomSnapshot before, BattleCombatantKey actor,
            BattleCombatantKey target, BigInteger ordinal, BattleEntryRejectionCode code, string path)
        {
            var original = Describe(binding); var oldState = Describe(before);
            var budget = new RandomSamplingBudget(new ExactMathBudget());
            var result = CandidateWarriorCritEvaluator.EvaluateOpportunity(binding, before, actor, target, ordinal, budget);
            Assert.IsFalse(result.IsAccepted); Assert.IsNull(result.Next); Assert.IsNull(result.Fact);
            Assert.AreEqual(code, result.RejectionCode); Assert.AreEqual(path, result.FieldPath);
            Assert.AreEqual(0, budget.WordsUsed);
            Assert.AreEqual(original, Describe(binding)); Assert.AreEqual(oldState, Describe(before));
        }

        private static CandidateSeedMaterial Material(ulong a = 42, ulong t = 54)
        {
            var material = new CandidateSeedMaterial { Bytes = new byte[48], SourceCapabilityId = "synthetic-test-provider", MappingId = CandidateRandomPreparer.SupportedMappingId };
            for (var offset = 0; offset < 48; offset += 16) WriteBlock(material.Bytes, offset, a, t);
            return material;
        }

        private static void WriteBlock(byte[] bytes, int offset, ulong a, ulong t)
        {
            for (var i = 0; i < 8; i++) { bytes[offset + i] = (byte)(a >> (8 * i)); bytes[offset + 8 + i] = (byte)(t >> (8 * i)); }
        }

        private static BattleEntryInput Candidate(int stage = 1, int direction = 1)
        {
            var fixture = DemoContentFixture.CaptureSource(stage, 1, (SourceCoordinateConvention)direction);
            var geometry = fixture.CopyLevel();
            var context = new CandidateContext
            {
                DraftId = "candidate:" + fixture.FixtureKey, DraftRevision = 1,
                ContentFingerprint = "candidate:" + fixture.FixtureKey + ":direction" + direction,
                RuleVersion = "candidate-r1", NumericContractVersion = "exact-candidate", RandomContractVersion = "pcg-candidate",
                SourceNotes = new List<string> { fixture.SourcePath + ":" + fixture.SourceLocator + ":" + fixture.SourceSha256,
                    fixture.CoordinateTransform, "synthetic C=1/4; no calibrated 20% or approved source orientation; supplied SC01 test material" }
            };
            var face = new FaceInput { FaceId = "face:first", Width = geometry.width, Height = geometry.height, Pairs = new List<PairInput>() };
            foreach (var b in fixture.Bindings)
            {
                var pair = geometry.pairs.Single(p => p.colorId == b.ColorId); var heavy = b.EnemyAlias == "E02";
                var intents = new List<EnemyIntentInput>();
                if (heavy) intents.Add(new EnemyIntentInput { Kind = EnemyIntentKind.Charge, Targeting = EnemyTargeting.FirstLiving, DamageKind = null, DamageCoefficient = null });
                intents.Add(new EnemyIntentInput { Kind = EnemyIntentKind.Strike, Targeting = EnemyTargeting.FirstLiving, DamageKind = EntryDamageKind.Physical, DamageCoefficient = R(heavy ? 13 : 3, heavy ? 10 : 5) });
                face.Pairs.Add(new PairInput
                {
                    PairId = b.SourcePair, GeometryColorId = b.ColorId, EndpointA = pair.endpointA, EndpointB = pair.endpointB,
                    Enemy = new EnemyInput { EnemyInstanceKey = b.SourcePair, EnemyDefinitionId = b.EnemyAlias, OriginalSlot = b.OriginalSlot,
                        StableOrder = b.OriginalSlot, Behavior = heavy ? EnemyBehavior.ChargeHeavy : EnemyBehavior.NormalStrike,
                        Stats = Stats(heavy ? 20 : 15, 10, heavy ? 20 : 0, 0), IntentCycle = intents }
                });
            }
            return new BattleEntryInput
            {
                PlayerId = "candidate:player", ChallengeId = "candidate:challenge", AttemptId = "candidate:attempt", EntryBaselineId = "candidate:baseline",
                Context = context, CarryMode = EntryCarryMode.Empty, RequiredFeatures = new List<string>(),
                Level = new LevelInput { LevelId = fixture.FixtureKey, LevelVersion = "candidate-r1", RecommendedLevel = 1, Faces = new List<FaceInput> { face } },
                Members = new List<MemberInput> { new MemberInput { CharacterId = "candidate:warrior", ClassId = "warrior", ClassKind = CharacterClassKind.Warrior,
                    OriginalSlot = 2, Level = 1, IsReady = true, StatsOrigin = BaseStatsOrigin.ComputedBaseStats, StatsContext = CopyContext(context),
                    Stats = Stats(100, 20, 10, 6), EntryHp = R(97, 2), LearnedSkills = new List<string>(), Crit = new WarriorCritInput
                    { PassiveDefinitionId = "warrior:crit", TargetProbability = R(1, 5), C = R(1, 4), Multiplier = R(3, 2) } } }
            };
        }

        private static CandidateContext CopyContext(RuleContext c)
        {
            return new CandidateContext { DraftId = c.DraftId, DraftRevision = c.DraftRevision, ContentFingerprint = c.ContentFingerprint,
                RuleVersion = c.RuleVersion, NumericContractVersion = c.NumericContractVersion, RandomContractVersion = c.RandomContractVersion,
                SourceNotes = new List<string>(c.SourceNotes) };
        }
        private static StatsInput Stats(int hp, int attack, int physical, int magic)
        { return new StatsInput { MaxHp = R(hp), Attack = R(attack), PhysicalDefense = R(physical), MagicDefense = R(magic), Evasion = R(0), AttackRange = 1 }; }
        private static PreparedBattleEntry PrepareEntry(BattleEntryInput input)
        {
            var result = new BattleEntryPreparer().PrepareCandidate(input, new ExactMathBudget());
            Assert.IsTrue(result.IsAccepted, result.RejectionCode + " " + result.FieldPath); return result.Entry;
        }
        private static ExactRational R(int n, int d = 1) { return ExactRational.Create(n, d, new ExactMathBudget()); }
        private static void Value(ExactRational value, int n, int d = 1) { Assert.AreEqual(new BigInteger(n), value.Numerator); Assert.AreEqual(new BigInteger(d), value.Denominator); }
        private static object Get(object root, string path)
        {
            foreach (var part in path.Replace("[", ".").Replace("]", "").Split('.'))
                root = root is IList list ? list[int.Parse(part)] : root.GetType().GetProperty(part).GetValue(root);
            return root;
        }
        private static void Set(object root, string path, object value)
        {
            var last = path.LastIndexOf('.');
            if (last >= 0) { root = Get(root, path.Substring(0, last)); path = path.Substring(last + 1); }
            root.GetType().GetProperty(path).SetValue(root, value);
        }
        private static string Describe(object value)
        {
            if (value == null) return "null";
            if (value is string s) return s.Length + ":" + s;
            if (value is BigInteger integer) return integer.ToString(CultureInfo.InvariantCulture);
            if (value is ExactRational r) return r.Numerator + "/" + r.Denominator;
            if (value is FlowPos p) return p.x + "," + p.y;
            if (value.GetType().IsValueType) return value.ToString();
            if (value is IEnumerable items) return "[" + string.Join(";", items.Cast<object>().Select(Describe)) + "]";
            return "{" + string.Join(";", value.GetType().GetProperties().OrderBy(p => p.Name, StringComparer.Ordinal)
                .Select(p => p.Name + "=" + Describe(p.GetValue(value)))) + "}";
        }
        private static void AssertReadOnlyGraph(object value)
        {
            if (value == null || value is string || value is ExactRational || value.GetType().IsValueType) return;
            if (value is IList list)
            {
                Assert.IsTrue(list.IsReadOnly); Assert.Throws<NotSupportedException>(() => list.Clear());
                if (list.Count > 0) Assert.Throws<NotSupportedException>(() => list[0] = list[0]);
                foreach (var item in list) AssertReadOnlyGraph(item);
                return;
            }
            Assert.IsEmpty(value.GetType().GetConstructors(BindingFlags.Public | BindingFlags.Instance));
            foreach (var property in value.GetType().GetProperties())
            { Assert.IsFalse(property.CanWrite, property.Name); AssertReadOnlyGraph(property.GetValue(value)); }
        }
    }
}
