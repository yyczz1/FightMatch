using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using FlowPuzzle.Core;
using NUnit.Framework;
using static FightMatch.Core.Tests.GrowthFixture;
using static FightMatch.Core.CandidateGrowthRejectionCode;

namespace FightMatch.Core.Tests
{
    [TestFixture]
    public sealed class CandidateCharacterGrowthTests
    {
        [TestCase(4, 157, 14, 5, 6)]
        [TestCase(4, 157, 50, 5, 42)]
        [TestCase(5, 92, 150, 6, 22)]
        [TestCase(1, 0, 267, 4, 2)]
        [TestCase(4, 157, 0, 4, 157)]
        public void FixedReward_PreservesExactRemainderAndOriginalState(int level, int xp, int amount, int nextLevel, int nextXp)
        {
            var state = State(level, xp); var reward = Reward(amount);
            var result = CandidateCharacterGrowth.ApplyBaseReward(state, reward, state.StateRevision, new ExactMathBudget());
            Assert.IsTrue(result.IsAccepted); Assert.AreEqual(CandidateGrowthOutcome.Applied, result.Outcome);
            Assert.AreEqual(new BigInteger(nextLevel), result.Next.Level); Assert.AreEqual(new BigInteger(nextXp), result.Next.Experience);
            Assert.AreEqual(new BigInteger(2), result.Next.StateRevision); Assert.AreEqual(new BigInteger(amount), result.ExperienceAdded);
            Assert.AreEqual(new BigInteger(level), state.Level); Assert.AreEqual(new BigInteger(xp), state.Experience);
            Assert.IsEmpty(state.BaseRewards); Assert.AreEqual(1, result.Next.BaseRewards.Count);
            Assert.AreEqual(reward.SettlementId, result.RewardReceipt.SettlementId); Assert.AreEqual(reward.AttemptId, result.RewardReceipt.AttemptId);
            Assert.AreSame(state.Definition, result.Next.Definition); Assert.AreSame(state.Definition.Context, result.RewardReceipt.Context);
        }

        [Test]
        public void DuplicateReward_ReturnsCurrentStateAndOriginalReceiptBeforeRevisionCheck()
        {
            var first = Apply(State(4, 157), Reward(14));
            var second = Apply(first.Next, Reward(50, "later"));
            var duplicate = CandidateCharacterGrowth.ApplyBaseReward(second.Next, Reward(14), 1, new ExactMathBudget());
            Assert.AreSame(second.Next, duplicate.Next); Assert.AreSame(first.RewardReceipt, duplicate.RewardReceipt);
            Assert.AreEqual(CandidateGrowthOutcome.AlreadyIncluded, duplicate.Outcome); Assert.AreEqual(BigInteger.Zero, duplicate.ExperienceAdded);
            Assert.AreEqual(new BigInteger(56), duplicate.Next.Experience); Assert.AreEqual(2, duplicate.Next.BaseRewards.Count);
        }

        [TestCase("Amount", "Amount")]
        [TestCase("AttemptId", "AttemptId")]
        [TestCase("CharacterId", "CharacterId")]
        [TestCase("PlayerId", "PlayerId")]
        [TestCase("Context.DraftRevision", "Context.DraftRevision")]
        [TestCase("Context.SourceNotes", "Context.SourceNotes[0]")]
        public void ChangedReceiptFacts_ConflictEvenWhenRevisionIsStale(string field, string path)
        {
            var state = Apply(State(), Reward(14)).Next; var input = Reward(14);
            if (field == "Amount") input.Amount = 15;
            else if (field == "Context.DraftRevision") input.Context.DraftRevision = 2;
            else if (field == "Context.SourceNotes") input.Context.SourceNotes[0] = "changed";
            else Set(input, field, "changed");
            Reject(CandidateCharacterGrowth.ApplyBaseReward(state, input, 0, new ExactMathBudget()), InconsistentBinding, path);
            Assert.AreEqual(1, state.BaseRewards.Count); Assert.AreEqual(new BigInteger(14), state.Experience);
        }

        [Test]
        public void NewReward_RequiresCurrentRevision_AndIdsUseOrdinalWithoutTrimming()
        {
            var state = Apply(State(), Reward(0)).Next;
            Reject(CandidateCharacterGrowth.ApplyBaseReward(state, Reward(1, "new"), 1, new ExactMathBudget()), StaleContext, "ExpectedRevision");
            var changed = Reward(0); changed.SettlementId += " ";
            var result = Apply(state, changed);
            Assert.AreEqual(2, result.Next.BaseRewards.Count);
            changed.PlayerId = "PLAYER"; Reject(Apply(result.Next, changed, false), InconsistentBinding, "PlayerId");
        }

        [TestCase("PlayerId")]
        [TestCase("CharacterId")]
        [TestCase("AttemptId")]
        [TestCase("SettlementId")]
        [TestCase("Context")]
        [TestCase("Amount")]
        public void RewardRequiresEveryField(string path)
        {
            var input = Reward(0); Set(input, path, null);
            Reject(Apply(State(), input, false), MissingField, path);
        }

        [Test]
        public void NegativeRewardIsRejected()
        { Reject(Apply(State(), Reward(-1), false), InvalidValue, "Amount"); }

        [TestCase(1, 100, 20, 1, 10, 1, 6, 1, 1, 5)]
        [TestCase(4, 124, 118, 5, 29, 2, 21, 2, 43, 200)]
        [TestCase(20, 252, 214, 5, 77, 2, 69, 2, 59, 200)]
        [TestCase(31, 340, 56, 1, 55, 1, 51, 1, 7, 20)]
        [TestCase(40, 412, 334, 5, 137, 2, 129, 2, 7, 20)]
        public void StatsAreExactAndDoNotChangeRevision(int level, int hp, int an, int ad, int pn, int pd, int mn, int md, int cn, int cd)
        {
            var state = State(level); var value = CandidateCharacterGrowth.ComputeBaseStats(state, new ExactMathBudget());
            Value(value.Stats.MaxHp, hp); Value(value.Stats.Attack, an, ad);
            Value(value.Stats.PhysicalDefense, pn, pd); Value(value.Stats.MagicDefense, mn, md);
            Value(value.TargetProbability, cn, cd); Value(value.CritMultiplier, 3, 2); Value(value.Stats.Evasion, 0);
            Assert.AreEqual(1, value.Stats.AttackRange); Assert.AreEqual(2, value.OriginalSlot);
            Assert.AreEqual(BaseStatsOrigin.ComputedBaseStats, value.StatsOrigin); Assert.IsTrue(value.IsReady);
            Assert.AreSame(value.Stats.MaxHp, value.EntryHp); Assert.AreSame(state.Definition.Context, value.Context);
            Assert.AreEqual(BigInteger.One, state.StateRevision); Assert.IsNull(value.GetType().GetProperty("C"));
            Assert.IsNull(value.GetType().GetProperty("FailureCount")); Assert.AreEqual("crit", value.PassiveDefinitionId);
        }

        [Test]
        public void ComputedStats_HandOffWithExplicitSyntheticC_AndOldBaselineStaysFixed()
        {
            var state = State(4, 157); var stats = CandidateCharacterGrowth.ComputeBaseStats(state, new ExactMathBudget());
            var entry = new BattleEntryPreparer().PrepareCandidate(Entry(stats), new ExactMathBudget());
            Assert.IsTrue(entry.IsAccepted, entry.FieldPath);
            var start = BattleStartAssembler.CreateCandidate(entry.Entry, new CandidateRandomInitials
            {
                Battle = Pcg32StreamState.Initialize(42, 54), BaseReward = Pcg32StreamState.Initialize(0, 0),
                Bonus = Pcg32StreamState.Initialize(1, 1), PrdStates = new List<CandidatePrdInitial>
                { new CandidatePrdInitial { CharacterId = "warrior", PassiveDefinitionId = "crit", FailureCount = BigInteger.Zero } }
            }, new ExactMathBudget());
            Assert.IsTrue(start.IsAccepted, start.FieldPath);
            var newer = CandidateCharacterGrowth.ComputeBaseStats(Apply(state, Reward(14)).Next, new ExactMathBudget());
            Value(newer.Stats.MaxHp, 132); Value(entry.Entry.Members[0].Stats.MaxHp, 124);
            Value(start.Start.Baseline.Entry.Members[0].Stats.Attack, 118, 5);
            Assert.AreEqual(2, start.Start.Baseline.Entry.Members[0].OriginalSlot);
            Assert.AreEqual(new BigInteger(4), start.Start.Baseline.Entry.Members[0].Level);
        }

        [TestCaseSource(nameof(MissingDefinitionFields))]
        public void DefinitionMissingField_IsNeverImplicitZero(string path)
        {
            var input = DefinitionInput(); Set(input, path, null);
            var result = CandidateCharacterGrowth.PrepareDefinition(input, new ExactMathBudget());
            Assert.IsFalse(result.IsAccepted); Assert.IsNull(result.Definition);
            Assert.AreEqual(MissingField, result.RejectionCode); Assert.AreEqual(path, result.FieldPath);
        }
        private static IEnumerable<string> MissingDefinitionFields()
        {
            return new[] { "Context", "Context.DraftId", "Context.ContentFingerprint", "Context.RuleVersion", "Context.NumericContractVersion",
                "Context.RandomContractVersion", "Context.SourceNotes", "ClassId", "PassiveDefinitionId", "BaseStats", "XpBase", "XpLinear", "XpQuadratic" }
                .Concat(DefinitionRationals());
        }

        [TestCaseSource(nameof(InvalidDefinitionFields))]
        public void InvalidDefinition_IsRejectedAtExactField(string path, object value, CandidateGrowthRejectionCode code)
        {
            var input = DefinitionInput(); Set(input, path, value);
            var result = CandidateCharacterGrowth.PrepareDefinition(input, new ExactMathBudget());
            Assert.IsFalse(result.IsAccepted); Assert.IsNull(result.Definition);
            Assert.AreEqual(code, result.RejectionCode); Assert.AreEqual(path, result.FieldPath);
        }
        private static IEnumerable<TestCaseData> InvalidDefinitionFields()
        {
            foreach (var path in DefinitionRationals()) yield return new TestCaseData(path, R(-1), InvalidValue).SetName("Definition_Negative_" + path);
            foreach (var path in new[] { "XpBase", "XpLinear", "XpQuadratic" }) yield return new TestCaseData(path, new BigInteger(-1), InvalidValue);
            yield return new TestCaseData("XpBase", BigInteger.Zero, InvalidValue);
            yield return new TestCaseData("Context.DraftRevision", BigInteger.Zero, InvalidValue);
            yield return new TestCaseData("Context.SourceNotes", new List<string>(), InvalidValue);
            yield return new TestCaseData("ClassId", " ", MissingField);
            yield return new TestCaseData("ClassKind", CharacterClassKind.Unspecified, MissingField);
            yield return new TestCaseData("ClassKind", (CharacterClassKind)99, UnsupportedBinding);
            yield return new TestCaseData("BaseStats.AttackRange", 0, InvalidValue);
            yield return new TestCaseData("BaseStats.MaxHp", R(0), InvalidValue);
            yield return new TestCaseData("BaseStats.Evasion", R(1, 2), UnsupportedBinding);
            yield return new TestCaseData("CritBase", R(0), InvalidValue);
            yield return new TestCaseData("CritBase", R(2), InvalidValue);
            yield return new TestCaseData("CritCap", R(2), InvalidValue);
            yield return new TestCaseData("CritCap", R(1, 10), InvalidValue);
            yield return new TestCaseData("CritMultiplier", R(0), InvalidValue);
            yield return new TestCaseData("RecoveryDurationMilliseconds", R(0), InvalidValue);
        }

        [TestCase(0, 0, 2, "Level")]
        [TestCase(1, -1, 2, "Experience")]
        [TestCase(1, 60, 2, "Experience")]
        [TestCase(4, 165, 2, "Experience")]
        [TestCase(1, 0, -1, "OriginalSlot")]
        [TestCase(1, 0, 3, "OriginalSlot")]
        public void InvalidInitialCharacterHasNoState(int level, int xp, int slot, string path)
        {
            Reject(CandidateCharacterGrowth.CreateCandidate(Definition(), "player", "warrior", level, xp, slot,
                new ExactMathBudget()), InvalidValue, path);
        }

        [TestCaseSource(nameof(LargeDefinitionFields))]
        public void EveryRetainedDefinitionNumber_IsRecheckedWithSmallerBudget(string path)
        {
            var input = DefinitionInput(); var huge = BigInteger.One << 160;
            if (path == "Context.DraftRevision" || path.StartsWith("Xp", StringComparison.Ordinal)) Set(input, path, huge);
            else
            {
                if (path == "CritCap") input.CritBase = R(1, huge);
                Set(input, path, R(1, huge));
            }
            var definition = Definition(input);
            Assert.Throws<ExactMathLimitException>(() => CandidateCharacterGrowth.PrepareDefinition(input, new ExactMathBudget(64)));
            Assert.Throws<ExactMathLimitException>(() => CandidateCharacterGrowth.CreateCandidate(definition, "player", "warrior", 1, 0, 2, new ExactMathBudget(64)));
            CheckEveryStateEntryRejectsSmallBudget(State(definition: definition), 64);
        }
        private static IEnumerable<string> LargeDefinitionFields()
        { return DefinitionRationals().Where(p => p != "BaseStats.Evasion").Concat(new[] { "XpBase", "XpLinear", "XpQuadratic", "Context.DraftRevision" }); }

        [Test]
        public void BeyondDoubleIntegerRange_LevelExperienceAndComputedStatsRemainExact()
        {
            var huge = (BigInteger.One << 60) + 3; var input = DefinitionInput();
            input.XpBase = huge + 10; input.XpLinear = 0; input.XpQuadratic = 0;
            var state = State(huge, huge - 1, Definition(input));
            var next = Apply(Apply(state, Reward(2)).Next, Reward(20, "next")).Next;
            Assert.AreEqual(huge + 1, next.Level); Assert.AreEqual(new BigInteger(11), next.Experience);
            var stats = CandidateCharacterGrowth.ComputeBaseStats(state, new ExactMathBudget());
            Value(stats.Stats.MaxHp, 8 * huge + 92); Value(stats.Stats.Attack, 6 * huge + 94, 5);
            Assert.Throws<ExactMathLimitException>(() => CandidateCharacterGrowth.ComputeBaseStats(state, new ExactMathBudget(53)));
        }

        [Test]
        public void HistoricalRewardAmountIsRecheckedEvenAfterItsExperienceHasBeenSpentOnLevels()
        {
            BigInteger count = 2000;
            var amount = 60 * count + 20 * count * (count - 1) / 2 + 5 * count * (count - 1) * (2 * count - 1) / 6;
            var state = Apply(State(), Reward(amount)).Next;
            Assert.AreEqual(count + 1, state.Level); Assert.AreEqual(BigInteger.Zero, state.Experience);
            CheckEveryStateEntryRejectsSmallBudget(state, 32);
        }

        [Test]
        public void SharedStepBudgetFailsWithoutPartialReward_AndFreshRetryMatches()
        {
            var state = State(); var input = Reward(10000); var budget = new ExactMathBudget();
            var first = CandidateCharacterGrowth.ApplyBaseReward(state, input, 1, budget);
            Assert.IsTrue(first.IsAccepted);
            var tooSmall = new ExactMathBudget(32768, (int)budget.PrimitiveStepsUsed - 1);
            var error = Assert.Throws<ExactMathLimitException>(() => CandidateCharacterGrowth.ApplyBaseReward(state, input, 1, tooSmall));
            Assert.AreEqual("PrimitiveSteps", error.ReasonCode); Assert.IsEmpty(state.BaseRewards); Assert.AreEqual(BigInteger.One, state.StateRevision);
            Assert.AreEqual(Describe(first), Describe(Apply(state, input)));
            Assert.Throws<ExactMathLimitException>(() => ApplyWithBudget(State(), Reward(BigInteger.One << 100), new ExactMathBudget(32768, 2000)));
        }

        [Test]
        public void ComputationAlsoUsesOneBudgetAcrossAllFields()
        {
            var state = State(31); var budget = new ExactMathBudget();
            var first = CandidateCharacterGrowth.ComputeBaseStats(state, budget);
            Assert.Throws<ExactMathLimitException>(() => CandidateCharacterGrowth.ComputeBaseStats(state,
                new ExactMathBudget(32768, (int)budget.PrimitiveStepsUsed - 1)));
            Assert.AreEqual(Describe(first), Describe(CandidateCharacterGrowth.ComputeBaseStats(state, new ExactMathBudget())));
        }

        [Test]
        public void MutableInputsAreCopied_AndAllOutputGraphsAreReadOnly()
        {
            var input = DefinitionInput(); var definition = Definition(input); var state = State(definition: definition);
            var reward = Reward(0); var applied = Apply(state, reward); var before = Describe(applied);
            input.Context.SourceNotes[0] = "changed"; input.Context.DraftId = "changed"; input.BaseStats.MaxHp = R(1);
            input.GrowthHp = R(999); input.XpBase = 1; reward.Context.SourceNotes.Clear(); reward.Amount = 999;
            Assert.AreEqual(before, Describe(applied)); Immutable(applied); Immutable(CandidateCharacterGrowth.ComputeBaseStats(state, new ExactMathBudget()));
            Assert.Throws<NotSupportedException>(() => ((IList<CandidateBaseExperienceReceipt>)applied.Next.BaseRewards).Clear());
            Assert.Throws<NotSupportedException>(() => ((IList<string>)definition.Context.SourceNotes)[0] = "changed");
        }

        [Test]
        public void RootNullsThrowArgumentNullException()
        {
            var state = State(); var budget = new ExactMathBudget();
            Assert.Throws<ArgumentNullException>(() => CandidateCharacterGrowth.PrepareDefinition(null, budget));
            Assert.Throws<ArgumentNullException>(() => CandidateCharacterGrowth.PrepareDefinition(DefinitionInput(), null));
            Assert.Throws<ArgumentNullException>(() => CandidateCharacterGrowth.CreateCandidate(null, "p", "c", 1, 0, 0, budget));
            Assert.Throws<ArgumentNullException>(() => CandidateCharacterGrowth.CreateCandidate(state.Definition, "p", "c", 1, 0, 0, null));
            Assert.Throws<ArgumentNullException>(() => CandidateCharacterGrowth.ApplyBaseReward(null, Reward(0), 1, budget));
            Assert.Throws<ArgumentNullException>(() => CandidateCharacterGrowth.ApplyBaseReward(state, null, 1, budget));
            Assert.Throws<ArgumentNullException>(() => CandidateCharacterGrowth.ApplyBaseReward(state, Reward(0), 1, null));
            Assert.Throws<ArgumentNullException>(() => CandidateCharacterGrowth.ComputeBaseStats(null, budget));
            Assert.Throws<ArgumentNullException>(() => CandidateCharacterGrowth.ComputeBaseStats(state, null));
        }
    }

    internal static class GrowthFixture
    {
        internal static ExactRational R(BigInteger n, BigInteger? d = null) { return ExactRational.Create(n, d ?? BigInteger.One, new ExactMathBudget()); }
        internal static void Value(ExactRational value, BigInteger n, BigInteger? d = null)
        { var expected = R(n, d); Assert.AreEqual(expected.Numerator, value.Numerator); Assert.AreEqual(expected.Denominator, value.Denominator); }
        internal static CandidateContext Context()
        {
            return new CandidateContext { DraftId = "growth-candidate", DraftRevision = 1, ContentFingerprint = "synthetic-growth-v1",
                RuleVersion = "candidate-rule", NumericContractVersion = "exact", RandomContractVersion = "pcg32",
                SourceNotes = new List<string> { "Constructed candidate; no player save or reviewed PRD C." } };
        }
        internal static GrowthDefinitionInput DefinitionInput()
        {
            return new GrowthDefinitionInput { Context = Context(), ClassId = "W", ClassKind = CharacterClassKind.Warrior,
                PassiveDefinitionId = "crit", BaseStats = Stats(100, 20, 10, 6), GrowthHp = R(2, 25), GrowthAttack = R(3, 50),
                GrowthDefense = R(3, 2), CritBase = R(1, 5), CritStep = R(1, 200), CritCap = R(7, 20), CritMultiplier = R(3, 2),
                XpBase = 60, XpLinear = 20, XpQuadratic = 5, RecoveryDurationMilliseconds = R(180000) };
        }
        internal static CandidateGrowthDefinition Definition(GrowthDefinitionInput input = null)
        {
            var result = CandidateCharacterGrowth.PrepareDefinition(input ?? DefinitionInput(), new ExactMathBudget());
            Assert.IsTrue(result.IsAccepted, result.FieldPath); return result.Definition;
        }
        internal static CandidateCharacterState State(BigInteger? level = null, BigInteger? xp = null, CandidateGrowthDefinition definition = null)
        {
            var result = CandidateCharacterGrowth.CreateCandidate(definition ?? Definition(), "player", "warrior", level ?? BigInteger.One,
                xp ?? BigInteger.Zero, 2, new ExactMathBudget()); Assert.IsTrue(result.IsAccepted, result.FieldPath); return result.Next;
        }
        internal static CandidateBaseExperience Reward(BigInteger amount, string id = "base")
        { return new CandidateBaseExperience { PlayerId = "player", CharacterId = "warrior", AttemptId = "attempt-" + id, SettlementId = id, Amount = amount, Context = Context() }; }
        internal static CandidateCharacterResult Apply(CandidateCharacterState state, CandidateBaseExperience reward, bool assertAccepted = true)
        {
            var result = ApplyWithBudget(state, reward, new ExactMathBudget());
            if (assertAccepted) Assert.IsTrue(result.IsAccepted, result.FieldPath); return result;
        }
        internal static CandidateCharacterResult ApplyWithBudget(CandidateCharacterState state, CandidateBaseExperience reward, ExactMathBudget budget)
        { return CandidateCharacterGrowth.ApplyBaseReward(state, reward, state.StateRevision, budget); }
        internal static void Reject(CandidateCharacterResult result, CandidateGrowthRejectionCode code, string path)
        {
            Assert.IsFalse(result.IsAccepted); Assert.IsNull(result.Next); Assert.IsNull(result.RewardReceipt); Assert.IsNull(result.EndReceipt);
            Assert.IsNull(result.RecoveryPeriod); Assert.AreEqual(code, result.RejectionCode); Assert.AreEqual(path, result.FieldPath);
        }
        internal static void Set(object input, string path, object value)
        {
            var parts = path.Split('.'); for (var i = 0; i < parts.Length - 1; i++) input = input.GetType().GetProperty(parts[i]).GetValue(input);
            input.GetType().GetProperty(parts[parts.Length - 1]).SetValue(input, value);
        }
        internal static IEnumerable<string> DefinitionRationals()
        { return new[] { "BaseStats.MaxHp", "BaseStats.Attack", "BaseStats.PhysicalDefense", "BaseStats.MagicDefense", "BaseStats.Evasion",
            "GrowthHp", "GrowthAttack", "GrowthDefense", "CritBase", "CritStep", "CritCap", "CritMultiplier", "RecoveryDurationMilliseconds" }; }
        internal static string Describe(object value)
        {
            if (value == null) return "null";
            if (value is string || value is BigInteger || value.GetType().IsPrimitive || value.GetType().IsEnum) return value.ToString();
            if (value is IEnumerable items) return "[" + string.Join(",", items.Cast<object>().Select(Describe)) + "]";
            return "{" + string.Join(",", value.GetType().GetProperties().Select(p => p.Name + ":" + Describe(p.GetValue(value)))) + "}";
        }
        internal static void Immutable(object value)
        {
            if (value == null || value is string || value is BigInteger || value.GetType().IsPrimitive || value.GetType().IsEnum) return;
            if (value is IList list) { Assert.IsTrue(list.IsReadOnly); foreach (var item in list) Immutable(item); return; }
            Assert.IsEmpty(value.GetType().GetConstructors());
            foreach (var p in value.GetType().GetProperties()) { Assert.IsNull(p.SetMethod); Immutable(p.GetValue(value)); }
        }
        internal static void CheckEveryStateEntryRejectsSmallBudget(CandidateCharacterState state, int bits)
        {
            Assert.Throws<ExactMathLimitException>(() => CandidateCharacterGrowth.ComputeBaseStats(state, new ExactMathBudget(bits)));
            Assert.Throws<ExactMathLimitException>(() => ApplyWithBudget(state, Reward(0, "new"), new ExactMathBudget(bits)));
            Assert.Throws<ExactMathLimitException>(() => CandidateCharacterEnd.Propose(state, CandidateRecoveryTests.End(), state.StateRevision, new ExactMathBudget(bits)));
            Assert.Throws<ExactMathLimitException>(() => CandidateRecoveryClock.Advance(state, "unknown", CandidateRecoveryTests.Time(0), state.StateRevision, new ExactMathBudget(bits)));
        }
        internal static StatsInput Stats(int hp, int attack, int physical, int magic)
        { return new StatsInput { MaxHp = R(hp), Attack = R(attack), PhysicalDefense = R(physical), MagicDefense = R(magic), Evasion = R(0), AttackRange = 1 }; }
        internal static BattleEntryInput Entry(CandidateComputedStats value)
        {
            return new BattleEntryInput
            {
                PlayerId = value.PlayerId, ChallengeId = "challenge", AttemptId = "attempt", EntryBaselineId = "baseline", Context = Context(),
                CarryMode = EntryCarryMode.Empty, RequiredFeatures = new List<string>(),
                Level = new LevelInput { LevelId = "constructed", LevelVersion = "1", RecommendedLevel = 1, Faces = new List<FaceInput>
                { new FaceInput { FaceId = "face", Width = 2, Height = 1, Pairs = new List<PairInput>
                { new PairInput { PairId = "pair", GeometryColorId = 0, EndpointA = new FlowPos(0, 0), EndpointB = new FlowPos(1, 0),
                    Enemy = new EnemyInput { EnemyInstanceKey = "enemy", EnemyDefinitionId = "E01", OriginalSlot = 0, StableOrder = 0,
                        Behavior = EnemyBehavior.NormalStrike, Stats = Stats(10, 1, 0, 0), IntentCycle = new List<EnemyIntentInput>
                        { new EnemyIntentInput { Kind = EnemyIntentKind.Strike, Targeting = EnemyTargeting.FirstLiving,
                            DamageKind = EntryDamageKind.Physical, DamageCoefficient = R(1) } } } } } } } },
                Members = new List<MemberInput> { new MemberInput { CharacterId = value.CharacterId, ClassId = value.ClassId,
                    ClassKind = value.ClassKind, OriginalSlot = value.OriginalSlot, Level = value.Level, IsReady = value.IsReady,
                    StatsOrigin = value.StatsOrigin, StatsContext = Context(), EntryHp = value.EntryHp, LearnedSkills = new List<string>(),
                    Stats = new StatsInput { MaxHp = value.Stats.MaxHp, Attack = value.Stats.Attack, PhysicalDefense = value.Stats.PhysicalDefense,
                        MagicDefense = value.Stats.MagicDefense, AttackRange = value.Stats.AttackRange, Evasion = value.Stats.Evasion },
                    // C is synthetic for this handoff only, not derived from the target probability.
                    Crit = new WarriorCritInput { PassiveDefinitionId = value.PassiveDefinitionId, TargetProbability = value.TargetProbability,
                        C = R(1, 4), Multiplier = value.CritMultiplier } } }
            };
        }
    }
}
