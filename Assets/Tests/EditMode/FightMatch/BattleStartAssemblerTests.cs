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
    public sealed class BattleStartAssemblerTests
    {
        [TestCase(1, 1)]
        [TestCase(1, 2)]
        [TestCase(3, 1)]
        [TestCase(3, 2)]
        public void SourceCandidates_CreateCompleteUntouchedInitialState(int stage, int direction)
        {
            var fixture = DemoContentFixture.CaptureSource(stage, 1, (SourceCoordinateConvention)direction);
            var geometry = fixture.CopyLevel();
            var entry = Prepare(Candidate(stage, direction));
            var initials = Initials(entry);
            var beforeEntry = Describe(entry);
            var beforeInitials = Describe(initials);
            var start = Accept(entry, initials);
            var snapshot = start.Snapshot;
            Assert.AreSame(entry, start.Baseline.Entry);
            Assert.AreSame(start.Baseline, snapshot.Baseline);
            Assert.AreSame(entry.Context, snapshot.Baseline.Entry.Context);
            Assert.AreEqual(BigInteger.One, snapshot.SceneRevision);
            Assert.AreEqual(BigInteger.Zero, snapshot.EffectiveActionsCompleted);
            Assert.AreEqual(BigInteger.Zero, snapshot.EnemyPhasesCompleted);
            Assert.AreEqual(0, snapshot.CurrentFaceIndex);
            Assert.AreEqual(BattlePhase.AwaitAction, snapshot.Phase);
            Assert.AreEqual(EntryCarryMode.Empty, snapshot.CarryMode);
            Assert.AreSame(entry.Level.Faces[0], snapshot.Board.Face);
            Assert.IsEmpty(snapshot.Board.LockedRoutes);
            Assert.IsEmpty(snapshot.Board.PendingLinks);
            Assert.AreEqual(geometry.width, snapshot.Board.Face.Width);
            Assert.AreEqual(geometry.height, snapshot.Board.Face.Height);
            Assert.AreEqual(fixture.Bindings.Count, snapshot.Enemies.Count);
            for (var i = 0; i < snapshot.Enemies.Count; i++)
            {
                var binding = fixture.Bindings[i];
                var pair = snapshot.Board.Face.Pairs[i];
                var sourcePair = geometry.pairs.Single(p => p.colorId == binding.ColorId);
                var enemy = snapshot.Enemies[i];
                var heavy = binding.EnemyAlias == "E02";
                Assert.AreEqual(binding.SourcePair, pair.PairId);
                Assert.AreEqual(binding.ColorId, pair.GeometryColorId);
                Assert.AreEqual(sourcePair.endpointA, pair.EndpointA);
                Assert.AreEqual(sourcePair.endpointB, pair.EndpointB);
                Assert.AreSame(pair.Enemy, enemy.Enemy);
                Assert.AreEqual(binding.EnemyAlias, enemy.Enemy.EnemyDefinitionId);
                Assert.AreEqual(binding.OriginalSlot, enemy.OriginalSlot);
                Assert.AreEqual(pair.Enemy.StableOrder, enemy.StableOrder);
                Assert.AreSame(pair.Enemy.Stats.MaxHp, enemy.Hp);
                Value(enemy.Hp, heavy ? 20 : 15);
                Assert.AreEqual(BigInteger.Zero, enemy.IntentCursor);
                Assert.AreEqual(heavy ? EnemyIntentKind.Charge : EnemyIntentKind.Strike, enemy.Enemy.IntentCycle[0].Kind);
                if (heavy)
                {
                    Assert.IsNull(enemy.Enemy.IntentCycle[0].DamageCoefficient);
                    Value(enemy.Enemy.IntentCycle[1].DamageCoefficient, 13, 10);
                }
                else Value(enemy.Enemy.IntentCycle[0].DamageCoefficient, 3, 5);
                Assert.AreEqual(BattleCombatantKey.ForEnemy(entry.AttemptId, snapshot.Board.Face.FaceId,
                    pair.Enemy.EnemyInstanceKey), enemy.CombatantKey);
                Assert.AreEqual(BattlePairKey.Create(entry.AttemptId, snapshot.Board.Face.FaceId, pair.PairId), enemy.PairKey);
                Assert.IsNull(enemy.CombatantKey.CharacterId);
            }
            Assert.AreEqual(1, snapshot.Members.Count);
            var member = snapshot.Members[0];
            Assert.AreSame(entry.ReadyParticipants[0], member.Member);
            Assert.AreSame(member.Member.EntryHp, member.Hp);
            Value(member.Hp, 97, 2);
            Value(member.Member.Stats.MaxHp, 100);
            Assert.AreEqual(member.Member.OriginalSlot, member.OriginalSlot);
            Assert.AreEqual(BattleCombatantKey.ForParticipant(entry.AttemptId, member.Member.CharacterId), member.CombatantKey);
            Assert.IsNull(member.CombatantKey.FaceId);
            Assert.IsNull(member.CombatantKey.EnemyInstanceKey);
            Assert.AreSame(initials.Battle, start.Baseline.RandomInitials.Battle);
            Assert.AreSame(initials.BaseReward, start.Baseline.RandomInitials.BaseReward);
            Assert.AreSame(initials.Bonus, start.Baseline.RandomInitials.Bonus);
            Assert.AreSame(initials.Battle, snapshot.Random.Stream);
            Assert.AreEqual(1, snapshot.Random.PrdStates.Count);
            Assert.AreEqual(1, start.Baseline.PrdInitialStates.Count);
            var prd = snapshot.Random.PrdStates[0];
            Assert.AreSame(start.Baseline.PrdInitialStates[0], prd);
            Assert.AreEqual(member.CombatantKey, prd.CombatantKey);
            Assert.AreSame(member.Member.Crit, prd.Crit);
            Assert.AreEqual(BigInteger.Zero, prd.FailureCount);
            Value(prd.Crit.TargetProbability, 1, 5);
            Value(prd.Crit.C, 1, 4);
            Value(prd.Crit.Multiplier, 3, 2);
            Assert.AreEqual(1, snapshot.Contributions.Count);
            Assert.AreEqual(member.CombatantKey, snapshot.Contributions[0].CombatantKey);
            Value(snapshot.Contributions[0].EffectiveDamageDealtHp, 0);
            Value(snapshot.Contributions[0].EffectiveDamageTakenHp, 0);
            StringAssert.Contains(fixture.SourceSha256, entry.Context.SourceNotes[0]);
            StringAssert.Contains(fixture.CoordinateTransform, entry.Context.SourceNotes[1]);
            StringAssert.Contains("synthetic C=1/4", entry.Context.SourceNotes[2]);
            Assert.AreEqual(beforeEntry, Describe(entry));
            Assert.AreEqual(beforeInitials, Describe(initials));
        }

        [Test]
        public void SlotsPairOrderAndAllFaces_ArePreservedWithoutInitializingLaterEnemies()
        {
            var input = TwoFaces();
            input.Members[0].OriginalSlot = 2;
            input.Level.Faces[0].Pairs.Reverse();
            input.Level.Faces[0].Pairs[0].Enemy.OriginalSlot = 7;
            input.Level.Faces[0].Pairs[0].Enemy.StableOrder = 17;
            input.Level.Faces[0].Pairs[1].Enemy.OriginalSlot = 2;
            input.Level.Faces[0].Pairs[1].Enemy.StableOrder = 3;
            input.Level.Faces[1].Pairs[0].Enemy.Stats.MaxHp = R(34);
            var entry = Prepare(input);
            var start = Accept(entry, Initials(entry));
            Assert.AreEqual(2, start.Snapshot.Members.Single().OriginalSlot);
            CollectionAssert.AreEqual(new[] { "B", "A" }, start.Snapshot.Enemies.Select(e => e.PairKey.PairId));
            CollectionAssert.AreEqual(new[] { 7, 2 }, start.Snapshot.Enemies.Select(e => e.OriginalSlot));
            CollectionAssert.AreEqual(new[] { 17, 3 }, start.Snapshot.Enemies.Select(e => e.StableOrder));
            Assert.AreEqual(2, start.Baseline.Entry.Level.Faces.Count);
            Assert.AreSame(entry.Level.Faces[1], start.Baseline.Entry.Level.Faces[1]);
            Value(start.Baseline.Entry.Level.Faces[1].Pairs[0].Enemy.Stats.MaxHp, 34);
            var firstA = start.Snapshot.Enemies[1];
            var laterFace = entry.Level.Faces[1];
            var laterA = laterFace.Pairs[0];
            Assert.AreEqual(firstA.PairKey.PairId, laterA.PairId);
            Assert.AreEqual(firstA.Enemy.EnemyInstanceKey, laterA.Enemy.EnemyInstanceKey);
            Assert.AreNotEqual(firstA.PairKey, BattlePairKey.Create(entry.AttemptId, laterFace.FaceId, laterA.PairId));
            Assert.AreNotEqual(firstA.CombatantKey, BattleCombatantKey.ForEnemy(entry.AttemptId,
                laterFace.FaceId, laterA.Enemy.EnemyInstanceKey));
            input.AttemptId += ":next";
            var nextEntry = Prepare(input);
            var next = Accept(nextEntry, Initials(nextEntry));
            Assert.AreNotEqual(start.Snapshot.Members[0].CombatantKey, next.Snapshot.Members[0].CombatantKey);
            Assert.AreNotEqual(firstA.CombatantKey, next.Snapshot.Enemies[1].CombatantKey);
            Assert.AreNotEqual(firstA.PairKey, next.Snapshot.Enemies[1].PairKey);
        }

        [Test]
        public void Keys_UseOrdinalComponentsAndKindRatherThanDelimitedStrings()
        {
            var participant = BattleCombatantKey.ForParticipant("a|b", "c");
            var same = BattleCombatantKey.ForParticipant("a|b", "c");
            Assert.IsTrue(participant.Equals(same));
            Assert.IsTrue(participant.Equals((object)same));
            Assert.AreEqual(participant.GetHashCode(), same.GetHashCode());
            Assert.AreNotEqual(participant, BattleCombatantKey.ForParticipant("a", "b|c"));
            Assert.AreNotEqual(participant, BattleCombatantKey.ForParticipant("A|b", "c"));
            Assert.AreNotEqual(participant, BattleCombatantKey.ForParticipant("a|b", "C"));
            Assert.AreNotEqual(participant, BattleCombatantKey.ForParticipant("a|b", " c"));
            var enemy = BattleCombatantKey.ForEnemy("a", "b|c", "d");
            var sameEnemy = BattleCombatantKey.ForEnemy("a", "b|c", "d");
            Assert.IsTrue(enemy.Equals(sameEnemy));
            Assert.AreEqual(enemy.GetHashCode(), sameEnemy.GetHashCode());
            Assert.AreNotEqual(enemy, BattleCombatantKey.ForEnemy("a|b", "c", "d"));
            Assert.AreNotEqual(enemy, BattleCombatantKey.ForEnemy("a", "b", "c|d"));
            Assert.AreNotEqual(enemy, BattleCombatantKey.ForEnemy("a", "B|c", "d"));
            Assert.AreNotEqual(enemy, BattleCombatantKey.ForEnemy("a", "b|c", "D"));
            Assert.AreNotEqual(enemy, BattleCombatantKey.ForParticipant("a", "d"));
            var pair = BattlePairKey.Create("a", "b|c", "d");
            var samePair = BattlePairKey.Create("a", "b|c", "d");
            Assert.IsTrue(pair.Equals(samePair));
            Assert.IsTrue(pair.Equals((object)samePair));
            Assert.AreEqual(pair.GetHashCode(), samePair.GetHashCode());
            Assert.AreNotEqual(pair, BattlePairKey.Create("a|b", "c", "d"));
            Assert.AreNotEqual(pair, BattlePairKey.Create("a", "b", "c|d"));
            Assert.AreNotEqual(pair, BattlePairKey.Create("A", "b|c", "d"));
            Assert.AreNotEqual(pair, BattlePairKey.Create("a", "B|c", "d"));
            Assert.AreNotEqual(pair, BattlePairKey.Create("a", "b|c", "D"));
            Assert.IsFalse(pair.Equals(null));
            Assert.IsFalse(enemy.Equals(null));
            Assert.IsFalse(participant.Equals("a|b|c"));
            Assert.AreEqual(2, new HashSet<BattleCombatantKey> { participant, same, enemy, sameEnemy }.Count);
            Assert.AreEqual(1, new HashSet<BattlePairKey> { pair, samePair }.Count);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(6)]
        [TestCase(7)]
        public void KeyFactories_RejectEachMissingComponent(int component)
        {
            foreach (var missing in new[] { null, "", " \t" })
            {
                var values = new[] { "attempt", "face-or-character", "instance-or-pair" };
                values[component < 2 ? component : component < 5 ? component - 2 : component - 5] = missing;
                if (component < 2) Assert.Throws<ArgumentException>(() => BattleCombatantKey.ForParticipant(values[0], values[1]));
                else if (component < 5) Assert.Throws<ArgumentException>(() => BattleCombatantKey.ForEnemy(values[0], values[1], values[2]));
                else Assert.Throws<ArgumentException>(() => BattlePairKey.Create(values[0], values[1], values[2]));
            }
        }

        [Test]
        public void NullRoots_ThrowBeforeAnyBudgetWork()
        {
            var entry = Prepare(Candidate());
            var initials = Initials(entry);
            var budget = new ExactMathBudget(1, 0);
            Assert.AreEqual("entry", Assert.Throws<ArgumentNullException>(() => BattleStartAssembler.CreateCandidate(null, null, null)).ParamName);
            Assert.AreEqual("randomInitials", Assert.Throws<ArgumentNullException>(() => BattleStartAssembler.CreateCandidate(entry, null, null)).ParamName);
            Assert.AreEqual("budget", Assert.Throws<ArgumentNullException>(() => BattleStartAssembler.CreateCandidate(entry, initials, null)).ParamName);
            Assert.Throws<ArgumentNullException>(() => BattleStartAssembler.CreateCandidate(null, initials, budget));
            Assert.AreEqual(0, budget.PrimitiveStepsUsed);
        }

        [TestCase("Battle")]
        [TestCase("BaseReward")]
        [TestCase("Bonus")]
        public void EachRandomDomain_IsMandatoryAndMustBeUnconsumed(string domain)
        {
            var entry = Prepare(Candidate());
            var initials = Initials(entry);
            Set(initials, domain, null);
            Reject(entry, initials, MissingField, "RandomInitials." + domain);
            Set(initials, domain, Stream(0, 1, BigInteger.One));
            Reject(entry, initials, InvalidValue, "RandomInitials." + domain);
            var initial = Pcg32Core.Restore(0, 1);
            Pcg32Core.Next32(initial, out var current);
            Set(initials, domain, Pcg32StreamState.Restore(initial, current, BigInteger.One, new ExactMathBudget()));
            Reject(entry, initials, InvalidValue, "RandomInitials." + domain);
        }

        [TestCase("PrdStates")]
        [TestCase("PrdStates[0]")]
        [TestCase("PrdStates[0].CharacterId")]
        [TestCase("PrdStates[0].PassiveDefinitionId")]
        [TestCase("PrdStates[0].FailureCount")]
        public void MissingPrdFields_AreNotDefaulted(string path)
        {
            var entry = Prepare(Candidate());
            var initials = Initials(entry);
            Set(initials, path, null);
            Reject(entry, initials, MissingField, "RandomInitials." + path);
        }

        [TestCase("CharacterId")]
        [TestCase("PassiveDefinitionId")]
        public void EmptyPrdIdentities_AreMissing(string field)
        {
            var entry = Prepare(Candidate());
            foreach (var value in new[] { "", " \t" })
            {
                var initials = Initials(entry);
                Set(initials, "PrdStates[0]." + field, value);
                Reject(entry, initials, MissingField, "RandomInitials.PrdStates[0]." + field);
            }
        }

        [Test]
        public void PrdRows_MustExactlyMatchReadyParticipantAndPassive()
        {
            var entry = Prepare(Candidate());
            var initials = Initials(entry);
            initials.PrdStates.Clear();
            Reject(entry, initials, MissingField, "RandomInitials.PrdStates");
            var valid = Initials(entry).PrdStates[0];
            initials.PrdStates.Add(new CandidatePrdInitial { CharacterId = "extra", PassiveDefinitionId = valid.PassiveDefinitionId, FailureCount = 0 });
            Reject(entry, initials, InconsistentBinding, "RandomInitials.PrdStates[0].CharacterId");
            initials.PrdStates.Insert(0, valid);
            Reject(entry, initials, InconsistentBinding, "RandomInitials.PrdStates[1].CharacterId");
            initials.PrdStates[1].CharacterId = valid.CharacterId;
            initials.PrdStates[1].PassiveDefinitionId = valid.PassiveDefinitionId.ToUpperInvariant();
            Reject(entry, initials, InconsistentBinding, "RandomInitials.PrdStates[1].PassiveDefinitionId");
            initials.PrdStates[1].PassiveDefinitionId = valid.PassiveDefinitionId;
            Reject(entry, initials, DuplicateIdentity, "RandomInitials.PrdStates[1]");
            valid.FailureCount = -1;
            Reject(entry, initials, InvalidValue, "RandomInitials.PrdStates[0].FailureCount");
        }

        [TestCase("CharacterId")]
        [TestCase("PassiveDefinitionId")]
        public void PrdBindings_AreOrdinalWithoutTrimming(string field)
        {
            var entry = Prepare(Candidate());
            var initials = Initials(entry);
            var path = "PrdStates[0]." + field;
            var value = (string)Get(initials, path);
            Set(initials, path, value.ToUpperInvariant());
            Reject(entry, initials, InconsistentBinding, "RandomInitials." + path);
            Set(initials, path, " " + value);
            Reject(entry, initials, InconsistentBinding, "RandomInitials." + path);
        }

        [TestCase(-1)]
        [TestCase(1)]
        public void NonzeroPrdFailureCount_IsRejectedRatherThanReset(int count)
        {
            var entry = Prepare(Candidate());
            var initials = Initials(entry);
            initials.PrdStates[0].FailureCount = count;
            Reject(entry, initials, InvalidValue, "RandomInitials.PrdStates[0].FailureCount");
        }

        [TestCase(false)]
        [TestCase(true)]
        public void EqualDomainsAndZeroCoreState_AreLegal(bool shareStream)
        {
            var entry = Prepare(Candidate());
            var initials = Initials(entry);
            initials.Battle = Stream(0, 1);
            initials.BaseReward = shareStream ? initials.Battle : Stream(0, 1);
            initials.Bonus = shareStream ? initials.Battle : Stream(0, 1);
            var before = Describe(initials);
            var start = Accept(entry, initials);
            Assert.AreEqual(0UL, start.Snapshot.Random.Stream.Current.State);
            Assert.AreEqual(BigInteger.Zero, start.Snapshot.Random.Stream.WordsConsumed);
            Assert.AreEqual(BigInteger.Zero, start.Snapshot.Random.PrdStates[0].FailureCount);
            Assert.AreSame(initials.BaseReward, start.Baseline.RandomInitials.BaseReward);
            Assert.AreSame(initials.Bonus, start.Baseline.RandomInitials.Bonus);
            Assert.AreEqual(before, Describe(initials));
        }

        [Test]
        public void FailureOrder_IsPreparedBudgetThenDomainsThenPrd()
        {
            var entry = Prepare(Candidate());
            var initials = new CandidateRandomInitials();
            Assert.Throws<ExactMathLimitException>(() => BattleStartAssembler.CreateCandidate(entry, initials, new ExactMathBudget(256, 0)));
            Reject(entry, initials, MissingField, "RandomInitials.Battle");
            initials.Battle = Stream(0, 1);
            Reject(entry, initials, MissingField, "RandomInitials.BaseReward");
            initials.BaseReward = Stream(0, 1, BigInteger.One);
            Reject(entry, initials, InvalidValue, "RandomInitials.BaseReward");
            initials.BaseReward = Stream(0, 1);
            Reject(entry, initials, MissingField, "RandomInitials.Bonus");
            initials.Bonus = Stream(0, 1);
            Reject(entry, initials, MissingField, "RandomInitials.PrdStates");
        }

        [TestCase("Members[0].Level")]
        [TestCase("Level.RecommendedLevel")]
        [TestCase("Context.DraftRevision")]
        public void PreparedIntegers_AreRecheckedAgainstThisCallsBudget(string path)
        {
            var input = Candidate();
            var huge = (BigInteger.One << 80) + 1;
            Set(input, path, huge);
            input.Members[0].StatsContext = CopyContext(input.Context);
            var entry = Prepare(input);
            var initials = Initials(entry);
            AssertIntegerLimit(entry, initials);
            var start = Accept(entry, initials);
            Assert.AreEqual(huge, Get(start.Baseline.Entry, path));
        }

        private static IEnumerable<TestCaseData> RetainedRationals()
        {
            var paths = new List<string>();
            var owners = new List<string> { "Members[0]" };
            for (var face = 0; face < 2; face++)
                for (var pair = 0; pair < 2; pair++)
                {
                    var prefix = $"Level.Faces[{face}].Pairs[{pair}].Enemy";
                    owners.Add(prefix);
                    paths.Add(prefix + $".IntentCycle[{(face == 0 && pair == 0 ? 1 : 0)}].DamageCoefficient");
                }
            foreach (var owner in owners)
                foreach (var field in new[] { "MaxHp", "Attack", "PhysicalDefense", "MagicDefense" })
                    paths.Add(owner + ".Stats." + field);
            paths.AddRange(new[] { "Members[0].EntryHp", "Members[0].Crit.TargetProbability", "Members[0].Crit.C", "Members[0].Crit.Multiplier" });
            foreach (var path in paths)
                foreach (var denominator in new[] { false, true })
                    yield return new TestCaseData(path, denominator);
        }

        [TestCaseSource(nameof(RetainedRationals))]
        public void EveryRetainedRational_RechecksLargeNumeratorAndDenominator(string path, bool denominator)
        {
            var input = TwoFaces();
            var huge = (BigInteger.One << 80) + 1;
            var value = ExactRational.Create(denominator ? BigInteger.One : huge,
                denominator ? huge : huge + 1, new ExactMathBudget());
            Set(input, path, value);
            if (path == "Members[0].Stats.MaxHp")
                input.Members[0].EntryHp = ExactRational.Create(1, huge + 1, new ExactMathBudget());
            var entry = Prepare(input);
            var initials = Initials(entry);
            AssertIntegerLimit(entry, initials);
            Assert.AreSame(value, Get(Accept(entry, initials).Baseline.Entry, path));
        }

        [TestCase("Battle", "State")]
        [TestCase("Battle", "Increment")]
        [TestCase("Battle", "CurrentState")]
        [TestCase("Battle", "WordsConsumed")]
        [TestCase("BaseReward", "State")]
        [TestCase("BaseReward", "Increment")]
        [TestCase("BaseReward", "CurrentState")]
        [TestCase("BaseReward", "WordsConsumed")]
        [TestCase("Bonus", "State")]
        [TestCase("Bonus", "Increment")]
        [TestCase("Bonus", "CurrentState")]
        [TestCase("Bonus", "WordsConsumed")]
        public void EachStream_RechecksItsMathematicalValuesBeforeInitiality(string domain, string field)
        {
            var entry = Prepare(Candidate());
            var initials = Initials(entry);
            var state = field == "State" ? 1UL << 32 : 0;
            var increment = field == "Increment" ? (1UL << 32) | 1 : 1;
            var initial = Pcg32Core.Restore(state, increment);
            var current = field == "CurrentState" ? Pcg32Core.Restore(1UL << 32, increment) : initial;
            var words = field == "WordsConsumed" ? BigInteger.One << 80 : field == "CurrentState" ? BigInteger.One : BigInteger.Zero;
            Set(initials, domain, Pcg32StreamState.Restore(initial, current, words, new ExactMathBudget()));
            AssertIntegerLimit(entry, initials);
        }

        [TestCase(-1)]
        [TestCase(1)]
        public void PrdFailureCount_UsesTheSameIntegerBudget(int sign)
        {
            var entry = Prepare(Candidate());
            var initials = Initials(entry);
            initials.PrdStates[0].FailureCount = sign * (BigInteger.One << 80);
            AssertIntegerLimit(entry, initials);
            Reject(entry, initials, InvalidValue, "RandomInitials.PrdStates[0].FailureCount");
        }

        [Test]
        public void SharedStepBudget_CoversWholeCallAndFailureHasNoPartialOutput()
        {
            var entry = Prepare(TwoFaces());
            var initials = Initials(entry);
            var beforeEntry = Describe(entry);
            var beforeInitials = Describe(initials);
            var measuredBudget = new ExactMathBudget(256);
            var expected = Accept(entry, initials, measuredBudget);
            var steps = checked((int)measuredBudget.PrimitiveStepsUsed);
            Assert.Greater(steps, 2);
            var exactBudget = new ExactMathBudget(256, steps);
            Assert.AreEqual(Describe(expected), Describe(Accept(entry, initials, exactBudget)));
            foreach (var preconsume in new[] { false, true })
            {
                var tight = new ExactMathBudget(256, preconsume ? steps + 1 : steps - 1);
                if (preconsume) ExactRational.Create(0, 1, tight);
                BattleStartResult output = null;
                var error = Assert.Throws<ExactMathLimitException>(() => output = BattleStartAssembler.CreateCandidate(entry, initials, tight));
                Assert.AreEqual("PrimitiveSteps", error.ReasonCode);
                Assert.IsNull(output);
                Assert.AreEqual(Describe(expected), Describe(Accept(entry, initials)));
            }
            Assert.AreEqual(beforeEntry, Describe(entry));
            Assert.AreEqual(beforeInitials, Describe(initials));
        }

        [Test]
        public void ComputedW4AndEntryHp_AreNotRecomputedHealedOrScaled()
        {
            var input = Candidate(3);
            var member = input.Members[0];
            member.Level = 4;
            member.Stats.MaxHp = R(124);
            member.Stats.Attack = R(118, 5);
            member.Stats.PhysicalDefense = R(29, 2);
            member.Stats.MagicDefense = R(21, 2);
            member.EntryHp = R(247, 2);
            member.Crit.TargetProbability = R(43, 200);
            input.Level.RecommendedLevel = (BigInteger.One << 80) + 1;
            var entry = Prepare(input);
            var start = Accept(entry, Initials(entry));
            var state = start.Snapshot.Members[0];
            Assert.AreEqual(new BigInteger(4), state.Member.Level);
            Value(state.Member.Stats.MaxHp, 124);
            Value(state.Member.Stats.Attack, 118, 5);
            Value(state.Member.Stats.PhysicalDefense, 29, 2);
            Value(state.Member.Stats.MagicDefense, 21, 2);
            Value(state.Hp, 247, 2);
            Assert.AreSame(entry.Members[0].EntryHp, state.Hp);
            Value(start.Snapshot.Random.PrdStates[0].Crit.TargetProbability, 43, 200);
            Value(start.Snapshot.Random.PrdStates[0].Crit.C, 1, 4);
            Value(start.Snapshot.Enemies[0].Hp, 20);
            Value(start.Snapshot.Enemies[1].Hp, 15);
        }

        [Test]
        public void RepeatedAssemblyAndMutableInputChanges_CannotAlterReturnedCandidateValues()
        {
            var entry = Prepare(TwoFaces());
            var beforeEntry = Describe(entry);
            var initials = Initials(entry);
            var first = Accept(entry, initials);
            var second = Accept(entry, initials);
            var before = Describe(first);
            Assert.AreEqual(before, Describe(second));
            Assert.AreNotSame(first.Baseline, second.Baseline);
            Assert.AreNotSame(first.Snapshot, second.Snapshot);
            Assert.AreSame(entry, first.Baseline.Entry);
            Assert.AreNotSame(initials, first.Baseline.RandomInitials);
            Assert.AreNotSame(initials.PrdStates, first.Baseline.PrdInitialStates);
            initials.Battle = Stream(55, 7);
            initials.BaseReward = Stream(66, 9);
            initials.Bonus = Stream(77, 11);
            initials.PrdStates[0].CharacterId = "changed";
            initials.PrdStates[0].PassiveDefinitionId = "changed";
            initials.PrdStates[0].FailureCount = 23;
            initials.PrdStates.Clear();
            initials.PrdStates = null;
            Assert.AreEqual(before, Describe(first));
            Assert.AreEqual(before, Describe(second));
            Assert.AreEqual(beforeEntry, Describe(entry));
            AssertReadOnlyGraph(first);
            AssertReadOnlyGraph(second);
        }

        [Test]
        public void RandomOwnershipAndConstructionSurface_StayWithinCandidateStartScope()
        {
            CollectionAssert.AreEquivalent(new[] { "Stream", "PrdStates" }, typeof(BattleRandomSnapshot).GetProperties().Select(p => p.Name));
            CollectionAssert.AreEquivalent(new[] { "Battle", "BaseReward", "Bonus" }, typeof(BattleRandomInitials).GetProperties().Select(p => p.Name));
            CollectionAssert.AreEquivalent(new[] { "CombatantKey", "EffectiveDamageDealtHp", "EffectiveDamageTakenHp" },
                typeof(BattleContributionTotals).GetProperties().Select(p => p.Name));
            CollectionAssert.AreEquivalent(new[] { "AwaitAction", "AwaitLinks", "AwaitRescue", "WonPendingSettlement", "Closed" }, Enum.GetNames(typeof(BattlePhase)));
            CollectionAssert.AreEqual(new[] { "CreateCandidate" }, typeof(BattleStartAssembler)
                .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).Select(m => m.Name));
            foreach (var type in new[] { typeof(BattleLockedRoute), typeof(BattleSnapshot), typeof(BattleEntryBaseline), typeof(CandidateBattleStart) })
            {
                Assert.IsEmpty(type.GetConstructors(BindingFlags.Public | BindingFlags.Instance));
                Assert.IsTrue(type.GetProperties().All(p => !p.CanWrite));
                Assert.IsEmpty(type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                    .Where(m => !m.IsSpecialName));
            }
        }

        private static BattleEntryInput Candidate(int stage = 1, int direction = 1)
        {
            var fixture = DemoContentFixture.CaptureSource(stage, 1, (SourceCoordinateConvention)direction);
            var geometry = fixture.CopyLevel();
            var context = new CandidateContext
            {
                DraftId = "isolated:" + fixture.FixtureKey, DraftRevision = fixture.Revision,
                ContentFingerprint = "candidate:" + fixture.FixtureKey + ":direction" + direction + ":initial-state",
                RuleVersion = "candidate-r1", NumericContractVersion = "exact-candidate", RandomContractVersion = "pcg-candidate",
                SourceNotes = new List<string>
                {
                    fixture.SourcePath + " SHA256=" + fixture.SourceSha256 + " " + fixture.SourceLocator,
                    fixture.CoordinateTransform,
                    "synthetic C=1/4; coordinate approval and calibration remain external",
                    "Explicit synthetic PCG mathematical initials; no seed source or production binding"
                }
            };
            var face = new FaceInput { FaceId = "face:first", Width = geometry.width, Height = geometry.height, Pairs = new List<PairInput>() };
            foreach (var binding in fixture.Bindings)
            {
                var pair = geometry.pairs.Single(p => p.colorId == binding.ColorId);
                var heavy = binding.EnemyAlias == "E02";
                var cycle = new List<EnemyIntentInput>();
                if (heavy) cycle.Add(new EnemyIntentInput
                { Kind = EnemyIntentKind.Charge, Targeting = EnemyTargeting.FirstLiving, DamageKind = null, DamageCoefficient = null });
                cycle.Add(new EnemyIntentInput
                { Kind = EnemyIntentKind.Strike, Targeting = EnemyTargeting.FirstLiving, DamageKind = EntryDamageKind.Physical, DamageCoefficient = R(heavy ? 13 : 3, heavy ? 10 : 5) });
                face.Pairs.Add(new PairInput
                {
                    PairId = binding.SourcePair, GeometryColorId = binding.ColorId, EndpointA = pair.endpointA, EndpointB = pair.endpointB,
                    Enemy = new EnemyInput
                    {
                        EnemyInstanceKey = binding.SourcePair, EnemyDefinitionId = binding.EnemyAlias,
                        OriginalSlot = binding.OriginalSlot, StableOrder = binding.OriginalSlot,
                        Behavior = heavy ? EnemyBehavior.ChargeHeavy : EnemyBehavior.NormalStrike,
                        Stats = Stats(heavy ? 20 : 15, 10, heavy ? 20 : 0, 0), IntentCycle = cycle
                    }
                });
            }
            return new BattleEntryInput
            {
                PlayerId = "isolated:player", ChallengeId = "isolated:challenge", AttemptId = "isolated:attempt",
                EntryBaselineId = "isolated:baseline", Context = context, CarryMode = EntryCarryMode.Empty,
                RequiredFeatures = new List<string>(),
                Level = new LevelInput { LevelId = "candidate:L" + stage, LevelVersion = "candidate-r1", RecommendedLevel = 1, Faces = new List<FaceInput> { face } },
                Members = new List<MemberInput> { new MemberInput
                {
                    CharacterId = "isolated:warrior", ClassId = "class:warrior", ClassKind = CharacterClassKind.Warrior,
                    OriginalSlot = 0, Level = 1, IsReady = true, StatsOrigin = BaseStatsOrigin.ComputedBaseStats,
                    StatsContext = CopyContext(context), Stats = Stats(100, 20, 10, 6), EntryHp = R(97, 2),
                    LearnedSkills = new List<string>(), Crit = new WarriorCritInput
                    { PassiveDefinitionId = "candidate:warrior-crit", TargetProbability = R(1, 5), C = R(1, 4), Multiplier = R(3, 2) }
                } }
            };
        }

        private static BattleEntryInput TwoFaces()
        {
            var input = Candidate(3);
            var second = Candidate(1);
            second.Level.Faces[0].FaceId = "face:second";
            input.Level.Faces.Add(second.Level.Faces[0]);
            input.Context.SourceNotes.AddRange(second.Context.SourceNotes);
            input.Members[0].StatsContext = CopyContext(input.Context);
            return input;
        }

        private static CandidateContext CopyContext(RuleContext c)
        {
            return new CandidateContext
            {
                DraftId = c.DraftId, DraftRevision = c.DraftRevision, ContentFingerprint = c.ContentFingerprint,
                RuleVersion = c.RuleVersion, NumericContractVersion = c.NumericContractVersion,
                RandomContractVersion = c.RandomContractVersion, SourceNotes = new List<string>(c.SourceNotes)
            };
        }

        private static StatsInput Stats(int hp, int attack, int physical, int magic)
        {
            return new StatsInput { MaxHp = R(hp), Attack = R(attack), PhysicalDefense = R(physical), MagicDefense = R(magic), Evasion = R(0), AttackRange = 1 };
        }

        private static CandidateRandomInitials Initials(PreparedBattleEntry entry)
        {
            return new CandidateRandomInitials
            {
                Battle = Stream(0, 1), BaseReward = Stream(5, 3), Bonus = Stream(9, 5),
                PrdStates = entry.ReadyParticipants.Select(m => new CandidatePrdInitial
                { CharacterId = m.CharacterId, PassiveDefinitionId = m.Crit.PassiveDefinitionId, FailureCount = BigInteger.Zero }).ToList()
            };
        }

        private static Pcg32StreamState Stream(ulong state, ulong increment, BigInteger words = default)
        {
            return Pcg32StreamState.Restore(Pcg32Core.Restore(state, increment), Pcg32Core.Restore(state, increment), words, new ExactMathBudget());
        }

        private static PreparedBattleEntry Prepare(BattleEntryInput input)
        {
            var result = new BattleEntryPreparer().PrepareCandidate(input, new ExactMathBudget());
            Assert.IsTrue(result.IsAccepted, result.RejectionCode + " " + result.FieldPath);
            return result.Entry;
        }

        private static CandidateBattleStart Accept(PreparedBattleEntry entry, CandidateRandomInitials initials, ExactMathBudget budget = null)
        {
            var result = BattleStartAssembler.CreateCandidate(entry, initials, budget ?? new ExactMathBudget());
            Assert.IsTrue(result.IsAccepted, result.RejectionCode + " " + result.FieldPath);
            Assert.IsNull(result.RejectionCode);
            Assert.IsNull(result.FieldPath);
            return result.Start;
        }

        private static void Reject(PreparedBattleEntry entry, CandidateRandomInitials initials, BattleEntryRejectionCode code, string path)
        {
            var beforeEntry = Describe(entry);
            var beforeInitials = Describe(initials);
            var result = BattleStartAssembler.CreateCandidate(entry, initials, new ExactMathBudget());
            Assert.IsFalse(result.IsAccepted);
            Assert.IsNull(result.Start);
            Assert.AreEqual(code, result.RejectionCode);
            Assert.AreEqual(path, result.FieldPath);
            Assert.AreEqual(beforeEntry, Describe(entry));
            Assert.AreEqual(beforeInitials, Describe(initials));
        }

        private static void AssertIntegerLimit(PreparedBattleEntry entry, CandidateRandomInitials initials)
        {
            var beforeEntry = Describe(entry);
            var beforeInitials = Describe(initials);
            BattleStartResult output = null;
            var error = Assert.Throws<ExactMathLimitException>(() => output = BattleStartAssembler.CreateCandidate(entry, initials, new ExactMathBudget(16)));
            Assert.AreEqual("IntegerBits", error.ReasonCode);
            Assert.IsNull(output);
            Assert.AreEqual(beforeEntry, Describe(entry));
            Assert.AreEqual(beforeInitials, Describe(initials));
        }

        private static ExactRational R(int n, int d = 1) { return ExactRational.Create(n, d, new ExactMathBudget()); }
        private static void Value(ExactRational value, int n, int d = 1) { Assert.AreEqual(new BigInteger(n), value.Numerator); Assert.AreEqual(new BigInteger(d), value.Denominator); }
        private static string[] Parts(string path) { return path.Replace("[", ".").Replace("]", "").Split('.'); }
        private static object Get(object root, string path)
        {
            foreach (var part in Parts(path)) root = root is IList list ? list[int.Parse(part)] : root.GetType().GetProperty(part).GetValue(root);
            return root;
        }

        // Only edits mutable public inputs, never fabricates immutable mathematical states.
        private static void Set(object root, string path, object value)
        {
            var parts = Parts(path);
            for (var i = 0; i < parts.Length - 1; i++) root = Get(root, parts[i]);
            if (root is IList list) list[int.Parse(parts.Last())] = value;
            else root.GetType().GetProperty(parts.Last()).SetValue(root, value);
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
                Assert.IsTrue(list.IsReadOnly);
                Assert.Throws<NotSupportedException>(() => list.Clear());
                if (list.Count > 0) Assert.Throws<NotSupportedException>(() => list[0] = list[0]);
                foreach (var item in list) AssertReadOnlyGraph(item);
                return;
            }
            Assert.IsEmpty(value.GetType().GetConstructors(BindingFlags.Public | BindingFlags.Instance));
            foreach (var property in value.GetType().GetProperties())
            {
                Assert.IsFalse(property.CanWrite, property.Name);
                AssertReadOnlyGraph(property.GetValue(value));
            }
        }
    }
}
