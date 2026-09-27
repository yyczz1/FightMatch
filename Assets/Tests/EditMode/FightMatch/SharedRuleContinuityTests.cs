using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using FlowPuzzle.Core;
using NUnit.Framework;
using static FightMatch.Core.Tests.SharedRuleFixture;

namespace FightMatch.Core.Tests
{
    public class SharedRuleContinuityTests
    {
        [Test]
        public void CandidateAndPublishedSourcesProduceTheSameCompleteBattleFactsAndRewards()
        {
            var candidate = new SharedRuleFixture(false);
            var published = new SharedRuleFixture(true);
            Assert.IsFalse(RuleContextChecks.Same(candidate.Entry.Context, published.Entry.Context));
            Assert.IsNull(candidate.Entry.GetDefinitionBinding());
            Assert.IsTrue(published.Definition.Same(published.Entry.GetDefinitionBinding()));
            Same(candidate.Entry.Level, published.Entry.Level);
            Same(candidate.Entry.Members[0].Stats, published.Entry.Members[0].Stats);
            Same(candidate.Entry.Members[0].Crit, published.Entry.Members[0].Crit);
            for (var i = 0; i < 2; i++)
            {
                var a = candidate.Attack(i); var b = published.Attack(i);
                // Includes all HP, random states/words, damage, enemy, stage and contribution facts.
                Same(a.Record, b.Record);
                Assert.AreEqual(a.Record.DirectAttack.DamageFacts[0].HpLoss.Numerator, b.Record.DirectAttack.DamageFacts[0].HpLoss.Numerator);
                Assert.AreEqual(a.NextRun.CurrentSnapshot.SceneRevision, b.NextRun.CurrentSnapshot.SceneRevision);
                Assert.IsFalse(a.Record.DirectAttack.DamageFacts[0].Crit.Triggered);
                Assert.IsFalse(b.Record.DirectAttack.DamageFacts[0].Crit.Triggered);
            }
            var ar = candidate.Run.FinalReport; var br = published.Run.FinalReport;
            Assert.NotNull(ar); Assert.NotNull(br); Assert.AreNotEqual(ar.Fingerprint, br.Fingerprint);
            Assert.AreEqual(CandidateBattleOutcome.NormalVictory, br.Outcome);
            Same(ar.Contributions, br.Contributions); Same(ar.WholeLevelInitialEnemyHp, br.WholeLevelInitialEnemyHp);
            Assert.AreEqual(ar.EndedAtUnixMilliseconds, br.EndedAtUnixMilliseconds);
            Assert.AreEqual(ar.TerminalOperationId, br.TerminalOperationId);
            Assert.AreEqual(ar.ConsumptionCoverage, br.ConsumptionCoverage);
            Assert.AreEqual(new BigInteger(95), br.FinalSnapshot.Members[0].Hp.Numerator);
            Assert.Greater(br.FinalSnapshot.Random.Stream.WordsConsumed, BigInteger.Zero);
            Assert.AreEqual(ar.FinalSnapshot.Random.Stream.WordsConsumed, br.FinalSnapshot.Random.Stream.WordsConsumed);
            var ca = candidate.Finish(); var pb = published.Finish();
            Assert.AreEqual(new BigInteger(26), pb.Experience[0].Amount);
            Assert.AreEqual(ca.Experience[0].Amount, pb.Experience[0].Amount);
            Same(ca.Experience[0].Contribution, pb.Experience[0].Contribution);
            Same(ca.Experience[0].Score.LowerBound, pb.Experience[0].Score.LowerBound);
            Same(ca.Experience[0].Score.UpperBound, pb.Experience[0].Score.UpperBound);
            Assert.AreEqual(ca.Materials[0].Amount, pb.Materials[0].Amount);
            Assert.AreEqual(candidate.Character.Level, published.Character.Level);
            Assert.AreEqual(candidate.Character.Experience, published.Character.Experience);
            Assert.AreEqual(new BigInteger(26), published.Character.Experience);
            Assert.AreEqual(candidate.Inventory.Holdings[0].T, published.Inventory.Holdings[0].T);
            Assert.AreEqual(new BigInteger(2), published.Inventory.Holdings[0].T);
            Assert.IsNull(published.Inventory.ActiveCarry); Assert.IsNull(published.Progression.ActiveAttempt);
            Assert.AreEqual(candidate.Progression.StateRevision, published.Progression.StateRevision);
            Assert.AreEqual(1, published.Progression.FirstClears.Count);
            Assert.AreEqual(BigInteger.Zero, pb.Random.WordsConsumed);
            Assert.IsFalse(pb.CommitEligible); Assert.IsFalse(br.CommitEligible);
        }

        [Test]
        public void CandidateFullReportStillMatchesTheFrozenPreP2ASchema()
        {
            var s = new SharedRuleFixture(false); s.Attack(0); s.Attack(1);
            byte[] expected;
            using (var output = new MemoryStream())
            {
                new LegacyCandidateReportWriter(output, Math()).Value(s.Run.FinalReport);
                expected = output.ToArray();
            }
            CollectionAssert.AreEqual(expected, CandidateBattleReportFingerprint.Encode(s.Run.FinalReport, Math()));
            using (var sha = SHA256.Create())
                Assert.AreEqual(Hex(sha.ComputeHash(expected)), s.Run.FinalReport.Fingerprint);
        }

        [Test]
        public void CandidateIntentAndReportPrimitiveKeepTheAcceptedLiteralGoldens()
        {
            // Accepted CandidateApplicationIntentTests.cs:18-23, before this packet.
            const string expected = "464d494e543030310100000001000000700001000000690001000000000100000064000100000031" +
                "010000006300010000007200010000006e00010000007100010000000100000073000100000078000100000077000100000031010000003000000000";
            var intent = CandidateApplicationProtocol.PrepareIntent(SimpleIntent(Candidate()), Budget());
            Assert.IsTrue(intent.IsAccepted, intent.FieldPath);
            Assert.AreEqual(expected, Hex(intent.Value.CanonicalBytes));
            Assert.AreEqual("dc9d0a7e42a08310d68fd546199dd0f484f818da5deee3f4d5568e799a95383d", Hex(intent.Value.Sha256));
            // Accepted CandidateBattleOperationsTests.cs:383-385; independent hand-calculated record.
            using (var output = new MemoryStream())
            {
                new Fmbr01Writer(output, Math()).Record("x", R(-1, 2), "z", "中😀");
                Assert.AreEqual("464d425230310a0602000000040100000078000302020000002d3102010000003204010000007a0004030000002d4e3dd800de", Hex(output.ToArray()));
                using (var sha = SHA256.Create())
                    Assert.AreEqual("c3918a41722ceee31527635fd94a3b52faff87f80bd40235f136b0b8bd13465c", Hex(sha.ComputeHash(output.ToArray())));
            }
        }

        [Test]
        public void PublishedIntentUsesItsOwnTagAndExactCompleteBinding()
        {
            var candidate = Take(CandidateApplicationProtocol.PrepareIntent(SimpleIntent(Candidate()), Budget()));
            var original = Take(CandidateApplicationProtocol.PrepareIntent(SimpleIntent(new PublishedRuleContext(Binding())), Budget()));
            Assert.AreEqual("FMINT001", Encoding.ASCII.GetString(candidate.CanonicalBytes.Take(8).ToArray()));
            Assert.AreEqual("FMINT002", Encoding.ASCII.GetString(original.CanonicalBytes.Take(8).ToArray()));
            Assert.AreEqual(2, original.CanonicalBytes[8]);
            Assert.IsFalse(original.CanonicalBytes.SequenceEqual(candidate.CanonicalBytes));
            var decoded = CandidateApplicationIntentCodec.Read(original.CanonicalBytes.ToArray(), Budget());
            Assert.IsTrue(RuleContextChecks.Same(original.Context, decoded.Context));
            for (var field = 0; field < 5; field++)
            {
                var changed = Take(CandidateApplicationProtocol.PrepareIntent(SimpleIntent(new PublishedRuleContext(Binding(field))), Budget()));
                Assert.IsFalse(original.Sha256.SequenceEqual(changed.Sha256), "field " + field);
            }
            var mixed = original.CanonicalBytes.ToArray(); mixed[7] = (byte)'1';
            Assert.Throws<SaveCodecFailure>(() => CandidateApplicationIntentCodec.Read(mixed, Budget()));
        }

        [Test]
        public void PublishedIsolationReplaysThroughTheExistingCoreAndNeverBecomesCommitEligible()
        {
            var s = new SharedRuleFixture(true);
            var first = s.Attack(0);
            var isolation = CandidateReplayOperations.BuildIsolationInput(s.History, new CandidateIsolationRequest {
                PlayerId = "p", AttemptId = "a", ExpectedSceneRevision = s.Run.CurrentSnapshot.SceneRevision,
                Purpose = CandidateIsolationPurpose.RecordedOperation, RightsMode = CandidateIsolationRights.Empty,
                RecordedOperationId = first.Record.OperationId }, Math());
            Assert.IsTrue(isolation.IsAccepted, isolation.FieldPath); Assert.IsFalse(isolation.Input.CommitEligible);
            var replay = CandidateReplayOperations.ReplayRecorded(isolation.Input, new RandomSamplingBudget(Math()));
            Assert.IsTrue(replay.IsAccepted, replay.FieldPath); Assert.AreEqual(CandidateReplayOutcome.Matched, replay.Outcome);
            Assert.IsFalse(replay.CommitEligible); Assert.IsFalse(isolation.Input.ContextKey.CommitEligible);
            Assert.IsInstanceOf<PreparedPublishedRuleContext>(isolation.Input.ContextKey.Context);
            Same(first.Record, replay.ActualRecords[0]);
        }

        [Test]
        public void LegacyBusinessAndApplicationSaveRejectFormalContextsAsValues()
        {
            var s = new SharedRuleFixture(true);
            var input = new CandidateBusinessInput("p", s.Character, s.Inventory, s.Progression,
                CandidateBaseRewards.CreateCandidate("p", Math()).Next, null, Array.Empty<CandidateBattleRun>(), Array.Empty<CandidateRollbackRecord>());
            var prepared = CandidateBusinessSaveCodec.Prepare(input, Budget());
            Assert.IsFalse(prepared.IsAccepted); Assert.AreEqual("UnsupportedBinding", prepared.RejectionCode);
            // A negative malformed-admission probe; this is not a successfully prepared business snapshot.
            var snapshot = new CandidateBusinessSnapshot(input);
            var header = new CandidateBusinessSaveHeader(1, "commit", null, new[] { new SaveCommitIndexEntry(1, "commit", null, null, null, new[] { "i" }) });
            var encoded = CandidateBusinessSaveCodec.Encode(snapshot, header, Budget());
            Assert.IsFalse(encoded.IsAccepted); Assert.AreEqual("UnsupportedBinding", encoded.RejectionCode);
            var legacy = new SharedRuleFixture(false);
            var inventory = CandidateInventory.CreateCandidate(legacy.Inventory.Definition, "p", new List<CandidateInventoryActorInput> {
                new CandidateInventoryActorInput { CharacterId = "W", ClassId = "warrior", ClassKind = CharacterClassKind.Warrior, OriginalSlot = 2 } }, Math());
            Assert.IsTrue(inventory.IsAccepted, inventory.FieldPath);
            var business = Take(CandidateBusinessSaveCodec.Prepare(new CandidateBusinessInput("p", legacy.Character, inventory.Next,
                CandidateProgression.CreateCandidate(legacy.Progression.Definition, "p", Math()).Next,
                CandidateBaseRewards.CreateCandidate("p", Math()).Next, null, Array.Empty<CandidateBattleRun>(), Array.Empty<CandidateRollbackRecord>()), Budget()));
            var initialize = SimpleIntent(Candidate()); initialize.InitializeProfile.CharacterId = "W";
            initialize.InitializeProfile.ClassId = "warrior"; initialize.InitializeProfile.OriginalSlot = 2;
            var candidate = Take(CandidateApplicationProtocol.Propose(null, business, Take(CandidateApplicationProtocol.PrepareIntent(initialize, Budget())),
                new CandidateApplicationResultInput(), null, Budget()));
            var old = Take(CandidateApplicationSaveCodec.Encode(candidate, header, Budget()));
            var forged = Take(SaveEnvelopeCodec.Prepare(new SaveEnvelopeInput { Purpose = SavePurpose.PlayerSave, PlayerId = old.PlayerId,
                SaveGeneration = old.SaveGeneration, CommitId = old.CommitId, ParentCommitId = old.ParentCommitId, CommitIndex = old.CommitIndex,
                RequiredSliceContracts = old.RequiredSliceContracts, Slices = old.SliceDirectory.Select((row, i) => new SaveSliceInput {
                    Contract = row.Contract, Bytes = old.SliceBytes[i].ToArray(),
                    Requirements = new SaveRequirements(Array.Empty<SaveBinding>(), Array.Empty<string>(), Array.Empty<string>(),
                        Array.Empty<string>(), Array.Empty<string>()) }).ToArray() }, Budget()));
            Assert.AreEqual("UnsupportedBinding", CandidateApplicationSaveCodec.Decode(forged, Budget()).RejectionCode);
            Assert.AreEqual("UnsupportedBinding", CandidateBusinessSaveCodec.Decode(forged, Budget()).RejectionCode);
        }

        internal static CandidateApplicationIntentInput SimpleIntent(RuleContext context)
        {
            return new CandidateApplicationIntentInput { PlayerId = "p", OperationId = "i", Kind = CandidateApplicationKind.InitializeProfile,
                Context = context, InitializeProfile = new CandidateApplicationInitializeInput {
                    CharacterId = "x", ClassId = "w", InitialLevel = 1, InitialExperience = 0, OriginalSlot = 0 } };
        }
        internal static string Hex(IEnumerable<byte> bytes) { return string.Concat(bytes.Select(x => x.ToString("x2", CultureInfo.InvariantCulture))); }
    }

    // Synthetic in-memory facts. They are not approved/published game content.
    internal sealed class SharedRuleFixture
    {
        internal readonly RuleContext Context;
        internal readonly ContentBinding Content;
        internal readonly DefinitionBinding Definition;
        internal readonly BattleEntryInput Input;
        internal readonly PreparedBattleEntry Entry;
        internal readonly CandidateRewardDefinition RewardDefinition;
        internal CandidateCharacterState Character;
        internal CandidateInventoryState Inventory;
        internal CandidateProgressionState Progression;
        internal CandidateBattleRun Run;
        internal CandidateBattleHistory History;
        internal static ExactMathBudget Math() { return new ExactMathBudget(); }
        internal static SaveCodecBudget Budget() { return new SaveCodecBudget(Math()); }
        internal static ExactRational R(int n, int d = 1) { return ExactRational.Create(n, d, Math()); }
        internal static T Take<T>(SaveCodecResult<T> result) { Assert.IsTrue(result.IsAccepted, result.RejectionCode + " " + result.FieldPath); return result.Value; }
        internal static void Same(object a, object b) { Assert.IsTrue(CandidateBattleReportFingerprint.Equal(a, b, Math())); }
        internal static CandidateContext Candidate() { return new CandidateContext { DraftId = "d", DraftRevision = 1,
            ContentFingerprint = "c", RuleVersion = "r", NumericContractVersion = "n", RandomContractVersion = "q", SourceNotes = new List<string> { "s" } }; }
        internal static ContentBinding Binding(int changedField = -1)
        {
            var fields = new[] { "package:test", "c", "r", "n", "q" };
            if (changedField >= 0) fields[changedField] += ":other";
            return Take(ContentBinding.Prepare(fields[0], fields[1], fields[2], fields[3], fields[4], Budget()));
        }

        internal SharedRuleFixture(bool published)
        {
            Content = Binding(); Definition = Take(DefinitionBinding.Prepare(Content, "test:L1", 1, Budget()));
            Context = published ? (RuleContext)new PublishedRuleContext(Content) : Candidate();
            var growthInput = BusinessSaveScenario.Growth(Candidate(), 100); growthInput.Context = Context;
            var growth = CandidateCharacterGrowth.PrepareDefinition(growthInput, Math());
            Assert.IsTrue(growth.IsAccepted, growth.FieldPath);
            var character = CandidateCharacterGrowth.CreateCandidate(growth.Definition, "p", "W", 1, 0, 2, Math());
            Assert.IsTrue(character.IsAccepted, character.FieldPath); Character = character.Next;
            var inventory = CandidateInventory.PrepareDefinition(new CandidateInventoryDefinitionInput { Context = Context,
                Items = new List<CandidateInventoryItemInput> { new CandidateInventoryItemInput { ItemId = "tin", Kind = CandidateInventoryItemKind.OrdinaryMaterial } } }, Math());
            Assert.IsTrue(inventory.IsAccepted, inventory.FieldPath);
            Inventory = CandidateInventory.CreateCandidate(inventory.Definition, "p", new List<CandidateInventoryActorInput> { Actor() }, Math()).Next;
            var progression = CandidateProgression.PrepareDefinition(new CandidateProgressionDefinitionInput { Context = Context,
                Levels = new List<CandidateProgressionLevelInput> { new CandidateProgressionLevelInput { LevelId = "test:L1", LevelVersion = "1",
                    UnlockRuleId = "initial", EntryKind = CandidateProgressionEntryKind.Ordinary, UnlockKind = CandidateProgressionUnlockKind.InitiallyOpen,
                    UnlockAfterLevelId = null, RequiredFeatures = new List<string>() } } }, Math());
            Assert.IsTrue(progression.IsAccepted, progression.FieldPath);
            Progression = CandidateProgression.CreateCandidate(progression.Definition, "p", Math()).Next;
            Input = RawEntry();
            var resolved = new BattleEntryPreparer().PrepareCandidate(Input, Math());
            Assert.IsTrue(resolved.IsAccepted, resolved.FieldPath);
            Input.Context = RuleContextChecks.Copy(Context); Input.Members[0].StatsContext = RuleContextChecks.Copy(Context);
            var entry = published ? new BattleEntryPreparer().PreparePublished(Input, Definition, resolved.Entry.Level, Math())
                : new BattleEntryPreparer().PrepareCandidate(Input, Math());
            Assert.IsTrue(entry.IsAccepted, entry.FieldPath); Entry = entry.Entry;
            var seed = new byte[48]; for (var i = 0; i < 48; i += 16) { seed[i] = 42; seed[i + 8] = 54; }
            var random = CandidateRandomPreparer.Prepare(Entry, new CandidateSeedMaterial { Bytes = seed,
                SourceCapabilityId = "test:frozen48", MappingId = CandidateRandomPreparer.SupportedMappingId }, Math());
            Assert.IsTrue(random.IsAccepted, random.FieldPath);
            var run = CandidateBattleOperations.CreateCandidate(random.Binding, Math());
            Assert.IsTrue(run.IsAccepted, run.FieldPath); Run = run.Run;
            var history = CandidateHistoryOperations.CreateCandidate(Run, Math()); Assert.IsTrue(history.IsAccepted, history.FieldPath); History = history.Next;
            var reward = CandidateBaseRewards.PrepareDefinition(new CandidateRewardDefinitionInput { Context = Context, RewardDefinitionId = "reward:L1", Version = "1",
                LevelId = "test:L1", LevelVersion = "1", BaseExperience = 20, DamageWeight = R(1), TakenWeight = R(1, 4),
                CurveBase = R(1, 2), CurveLog = R(1, 2), ReferenceHpDivisor = R(2), LevelPenaltyBase = R(3, 4), OverlevelGrace = 1,
                ZeroContributionPolicy = CandidateZeroContributionPolicy.CurveIntercept, DropMode = CandidateRewardDropMode.FixedOrdinaryMaterials,
                RequiredFeatures = new List<string>(), Materials = new List<CandidateRewardMaterialInput> { new CandidateRewardMaterialInput { ItemId = "tin", Amount = 2 } } }, Math());
            Assert.IsTrue(reward.IsAccepted, reward.FieldPath); RewardDefinition = reward.Definition;
            var begin = CandidateProgression.BeginAttempt(Progression, new CandidateProgressionBeginIntent {
                PlayerId = "p", LevelId = "test:L1", LevelVersion = "1", Context = Context, CharacterId = "W",
                ExpectedCharacterRevision = Character.StateRevision, ExpectedOriginalSlot = 2, ChallengeId = "challenge", AttemptId = "a", EntryBaselineId = "baseline" },
                Character, Progression.StateRevision, Math());
            Assert.IsTrue(begin.IsAccepted, begin.FieldPath); Progression = begin.Next;
            var carry = CandidateInventory.Freeze(Inventory, new CandidateInventoryFreezeIntent { PlayerId = "p", AttemptId = "a",
                EntryBaselineId = "baseline", Context = Context, ReadyParticipants = new List<CandidateInventoryActorInput> { Actor() } }, Inventory.StateRevision, Math());
            Assert.IsTrue(carry.IsAccepted, carry.FieldPath); Inventory = carry.Next;
        }

        private static CandidateInventoryActorInput Actor() { return new CandidateInventoryActorInput { CharacterId = "W", ClassId = "warrior",
            ClassKind = CharacterClassKind.Warrior, OriginalSlot = 2 }; }
        internal static BattleEntryInput RawEntry()
        {
            var c = Candidate();
            var pairs = new List<PairInput>();
            for (var i = 0; i < 2; i++) pairs.Add(new PairInput { PairId = "pair" + i, GeometryColorId = i,
                EndpointA = new FlowPos(0, i * 2), EndpointB = new FlowPos(3, i * 2), Enemy = new EnemyInput {
                    EnemyInstanceKey = "enemy" + i, EnemyDefinitionId = "E01", OriginalSlot = i, StableOrder = i, Behavior = EnemyBehavior.NormalStrike,
                    Stats = Stats(15, 10, 0, 0), IntentCycle = new List<EnemyIntentInput> { new EnemyIntentInput {
                        Kind = EnemyIntentKind.Strike, Targeting = EnemyTargeting.FirstLiving, DamageKind = EntryDamageKind.Physical, DamageCoefficient = R(3, 5) } } } });
            return new BattleEntryInput { PlayerId = "p", ChallengeId = "challenge", AttemptId = "a", EntryBaselineId = "baseline", Context = c,
                CarryMode = EntryCarryMode.Empty, RequiredFeatures = new List<string>(),
                Level = new LevelInput { LevelId = "test:L1", LevelVersion = "1", RecommendedLevel = 1,
                    Faces = new List<FaceInput> { new FaceInput { FaceId = "face0", Width = 4, Height = 3, Pairs = pairs } } },
                Members = new List<MemberInput> { new MemberInput { CharacterId = "W", ClassId = "warrior", ClassKind = CharacterClassKind.Warrior,
                    OriginalSlot = 2, Level = 1, IsReady = true, StatsOrigin = BaseStatsOrigin.ComputedBaseStats, StatsContext = RuleContextChecks.Copy(c),
                    Stats = Stats(100, 20, 10, 6), EntryHp = R(100), LearnedSkills = new List<string>(), Crit = new WarriorCritInput {
                        PassiveDefinitionId = "warrior:crit", TargetProbability = R(1, 5), C = R(1, 1000), Multiplier = R(3, 2) } } } };
        }
        private static StatsInput Stats(int hp, int attack, int defense, int magic)
        { return new StatsInput { MaxHp = R(hp), Attack = R(attack), PhysicalDefense = R(defense), MagicDefense = R(magic), Evasion = R(0), AttackRange = 1 }; }
        internal CandidateBattleResult Attack(int pair)
        {
            var s = Run.CurrentSnapshot;
            var result = CandidateBattleOperations.EvaluateAttack(Run, new CandidateAttackRequest { PlayerId = "p", AttemptId = "a",
                OperationId = "op" + pair, ExpectedSceneRevision = s.SceneRevision, Actor = s.Members[0].CombatantKey,
                Pair = BattlePairKey.Create("a", "face0", "pair" + pair),
                Route = new List<FlowPos> { new FlowPos(0, pair * 2), new FlowPos(1, pair * 2), new FlowPos(2, pair * 2), new FlowPos(3, pair * 2) } },
                new CandidateBattleConditions { PreferenceRevision = 1, ItemUseEnabled = true }, pair + 1, new RandomSamplingBudget(Math()));
            Assert.IsTrue(result.IsAccepted, result.RejectionCode + " " + result.FieldPath); Run = result.NextRun;
            var history = CandidateHistoryOperations.Append(History, Run, "anchor" + pair, Math());
            Assert.IsTrue(history.IsAccepted, history.FieldPath); History = history.Next; return result;
        }
        internal CandidateFixedBaseReward Finish()
        {
            var report = Run.FinalReport;
            var end = CandidateProgression.EndAttempt(Progression, new CandidateProgressionEndFacts { PlayerId = "p", LevelId = "test:L1",
                LevelVersion = "1", Context = Context, ChallengeId = "challenge", AttemptId = "a", EntryBaselineId = "baseline", EndReceiptId = "end",
                Kind = CandidateProgressionEndKind.NormalVictory, SettlementId = "settle", FinalReportFingerprint = report.Fingerprint, NewAttemptId = null }, Progression.StateRevision, Math());
            Assert.IsTrue(end.IsAccepted, end.FieldPath); Progression = end.Next;
            CandidateRewardResult reward;
            using (var scope = new ExactEvaluationScope(new ExactEvaluationBudget(Math(), 1048576, 4096)))
                reward = CandidateBaseRewards.FixNormal(CandidateBaseRewards.CreateCandidate("p", Math()).Next, report, RewardDefinition, end.EndReceipt,
                    new CandidateRewardFixRequest { PlayerId = "p", AttemptId = "a", ChallengeId = "challenge", EntryBaselineId = "baseline",
                        SettlementId = "settle", FinalReportFingerprint = report.Fingerprint, ExpectedStateRevision = 1 }, scope);
            Assert.IsTrue(reward.IsAccepted, reward.FieldPath);
            var growth = CandidateCharacterGrowth.ApplyBaseReward(Character, new CandidateBaseExperience { PlayerId = "p", CharacterId = "W",
                AttemptId = "a", SettlementId = "settle", Context = Context, Amount = reward.BaseReward.Experience[0].Amount }, Character.StateRevision, Math());
            Assert.IsTrue(growth.IsAccepted, growth.FieldPath); Character = growth.Next;
            var inventory = CandidateInventory.End(Inventory, new CandidateInventoryEndIntent { PlayerId = "p", AttemptId = "a", EntryBaselineId = "baseline",
                EndReceiptId = "end", Context = Context, Kind = CandidateInventoryEndKind.NormalVictory, SettlementId = "settle", NewAttemptId = null,
                Remaining = new List<CandidateInventoryRemainingInput>(), Rewards = new List<CandidateInventoryQuantityInput> {
                    new CandidateInventoryQuantityInput { ItemId = "tin", Quantity = reward.BaseReward.Materials[0].Amount } } }, Inventory.StateRevision, Math());
            Assert.IsTrue(inventory.IsAccepted, inventory.FieldPath); Inventory = inventory.Next; return reward.BaseReward;
        }
    }

    // Frozen pre-P2A FMBR01 schema oracle, from CandidateBattleReportFingerprint.cs
    // SHA256 bb35f4fb3c6943ec4f6e8f17a688d95fcd4f596592fd0839ed596915c798654e.
    // Only the test class/constructor name changes; production formal encoding is never called for expected bytes.
    internal sealed class LegacyCandidateReportWriter
    {
        private readonly Stream output;
        private readonly ExactMathBudget math;
        private readonly bool ignoreRevision;
        internal LegacyCandidateReportWriter(Stream output, ExactMathBudget math, bool ignoreRevision = false)
        {
            this.output = output; this.math = math; this.ignoreRevision = ignoreRevision;
            foreach (var b in new byte[] { 70, 77, 66, 82, 48, 49, 10 }) output.WriteByte(b);
        }
        private void U32(uint value)
        { for (var i = 0; i < 4; i++) output.WriteByte((byte)(value >> (8 * i))); }
        private void Length(int count) { math.CheckInteger(count); U32((uint)count); }
        private void Integer(BigInteger value)
        {
            math.CheckInteger(value); output.WriteByte(2);
            var bytes = Encoding.ASCII.GetBytes(value.ToString(CultureInfo.InvariantCulture));
            Length(bytes.Length); output.Write(bytes, 0, bytes.Length);
        }
        internal void Record(params object[] fields)
        {
            output.WriteByte(6); Length(fields.Length / 2);
            for (var i = 0; i < fields.Length; i += 2) { Value((string)fields[i]); Value(fields[i + 1]); }
        }
        private byte[] Core(Pcg32CoreState state)
        {
            math.CheckInteger(state.State); math.CheckInteger(state.Increment);
            var bytes = new byte[16];
            for (var i = 0; i < 8; i++) { bytes[i] = (byte)(state.State >> (8 * i)); bytes[i + 8] = (byte)(state.Increment >> (8 * i)); }
            return bytes;
        }
        private byte[] Words(IReadOnlyList<uint> words)
        {
            var bytes = new byte[checked(words.Count * 4)];
            for (var i = 0; i < words.Count; i++)
            { math.CheckInteger(words[i]); for (var j = 0; j < 4; j++) bytes[4 * i + j] = (byte)(words[i] >> (8 * j)); }
            return bytes;
        }

        internal void Value(object value)
        {
            if (value == null) { output.WriteByte(0); return; }
            switch (value)
            {
                case bool v: output.WriteByte(1); output.WriteByte(v ? (byte)1 : (byte)0); return;
                case BigInteger v: Integer(v); return;
                case int v: Integer(v); return;
                case uint v: Integer(v); return;
                case ulong v: Integer(v); return;
                case ExactRational v: output.WriteByte(3); Integer(v.Numerator); Integer(v.Denominator); return;
                case string v:
                    output.WriteByte(4); Length(v.Length);
                    foreach (var unit in v) { output.WriteByte((byte)unit); output.WriteByte((byte)(unit >> 8)); }
                    return;
                case byte[] v: output.WriteByte(7); Length(v.Length); output.Write(v, 0, v.Length); return;
                case CandidateFinalAttemptReport v:
                    Record("Binding", v.Binding, "Baseline", v.Baseline, "InitialSnapshot", v.InitialSnapshot,
                        "Operations", v.Operations, "FinalSnapshot", v.FinalSnapshot, "Contributions", v.Contributions,
                        "Outcome", v.Outcome, "EndedAtUnixMilliseconds", v.EndedAtUnixMilliseconds,
                        "TerminalOperationId", v.TerminalOperationId, "ConsumptionCoverage", v.ConsumptionCoverage,
                        "WholeLevelInitialEnemyHp", v.WholeLevelInitialEnemyHp, "CommitEligible", v.CommitEligible); return;
                case CandidateRandomBinding v:
                    Record("SourceCapabilityId", v.SourceCapabilityId, "MappingId", v.MappingId, "Battle", v.Battle,
                        "BaseReward", v.BaseReward, "Bonus", v.Bonus); return;
                case CandidateRandomDomain v:
                    Record("Purpose", v.Purpose, "InitState", v.InitState, "InitSequence", v.InitSequence, "Initial", v.Initial); return;
                case BattleEntryBaseline v:
                    Record("Entry", v.Entry, "RandomInitials", v.RandomInitials, "PrdInitialStates", v.PrdInitialStates); return;
                case BattleRandomInitials v: Record("Battle", v.Battle, "BaseReward", v.BaseReward, "Bonus", v.Bonus); return;
                case PreparedBattleEntry v:
                    var keys = new List<BattleCombatantKey>();
                    foreach (var member in v.ReadyParticipants) keys.Add(BattleCombatantKey.ForParticipant(v.AttemptId, member.CharacterId));
                    Record("PlayerId", v.PlayerId, "ChallengeId", v.ChallengeId, "AttemptId", v.AttemptId,
                        "EntryBaselineId", v.EntryBaselineId, "Context", v.Context, "Level", v.Level, "Members", v.Members,
                        "ReadyParticipants", keys, "CarryMode", v.CarryMode, "RequiredFeatures", v.RequiredFeatures); return;
                case PreparedCandidateContext v:
                    Record("DraftId", v.DraftId, "DraftRevision", v.DraftRevision, "ContentFingerprint", v.ContentFingerprint,
                        "RuleVersion", v.RuleVersion, "NumericContractVersion", v.NumericContractVersion,
                        "RandomContractVersion", v.RandomContractVersion, "SourceNotes", v.SourceNotes); return;
                case PreparedLevel v:
                    Record("LevelId", v.LevelId, "LevelVersion", v.LevelVersion, "RecommendedLevel", v.RecommendedLevel, "Faces", v.Faces); return;
                case PreparedFace v: Record("FaceId", v.FaceId, "Width", v.Width, "Height", v.Height, "Pairs", v.Pairs); return;
                case PreparedPair v:
                    Record("PairId", v.PairId, "GeometryColorId", v.GeometryColorId, "EndpointA", v.EndpointA,
                        "EndpointB", v.EndpointB, "Enemy", v.Enemy); return;
                case PreparedEnemy v:
                    Record("EnemyInstanceKey", v.EnemyInstanceKey, "EnemyDefinitionId", v.EnemyDefinitionId,
                        "OriginalSlot", v.OriginalSlot, "StableOrder", v.StableOrder, "Behavior", v.Behavior,
                        "Stats", v.Stats, "IntentCycle", v.IntentCycle); return;
                case PreparedEnemyIntent v:
                    Record("Kind", v.Kind, "Targeting", v.Targeting, "DamageKind", v.DamageKind, "DamageCoefficient", v.DamageCoefficient); return;
                case PreparedStats v:
                    Record("MaxHp", v.MaxHp, "Attack", v.Attack, "PhysicalDefense", v.PhysicalDefense,
                        "MagicDefense", v.MagicDefense, "Evasion", v.Evasion, "AttackRange", v.AttackRange); return;
                case PreparedMember v:
                    Record("CharacterId", v.CharacterId, "ClassId", v.ClassId, "ClassKind", v.ClassKind,
                        "OriginalSlot", v.OriginalSlot, "Level", v.Level, "IsReady", v.IsReady, "StatsOrigin", v.StatsOrigin,
                        "StatsContext", v.StatsContext, "Stats", v.Stats, "EntryHp", v.EntryHp,
                        "LearnedSkills", v.LearnedSkills, "Crit", v.Crit); return;
                case PreparedWarriorCrit v:
                    Record("PassiveDefinitionId", v.PassiveDefinitionId, "TargetProbability", v.TargetProbability,
                        "C", v.C, "Multiplier", v.Multiplier); return;
                case BattleSnapshot v:
                    Record("EntryBaselineId", v.Baseline.Entry.EntryBaselineId, "SceneRevision", ignoreRevision ? BigInteger.Zero : v.SceneRevision,
                        "EffectiveActionsCompleted", v.EffectiveActionsCompleted, "EnemyPhasesCompleted", v.EnemyPhasesCompleted,
                        "CurrentFaceIndex", v.CurrentFaceIndex, "Phase", v.Phase, "CarryMode", v.CarryMode,
                        "Board", v.Board, "Members", v.Members, "Enemies", v.Enemies, "Random", v.Random, "Contributions", v.Contributions); return;
                case BattleBoardState v: Record("FaceId", v.Face.FaceId, "LockedRoutes", v.LockedRoutes, "PendingLinks", v.PendingLinks); return;
                case BattleLockedRoute v: Record("PairKey", v.PairKey, "Route", v.Route); return;
                case BattleMemberState v: Record("CombatantKey", v.CombatantKey, "OriginalSlot", v.OriginalSlot, "Hp", v.Hp); return;
                case BattleEnemyState v:
                    Record("CombatantKey", v.CombatantKey, "PairKey", v.PairKey, "OriginalSlot", v.OriginalSlot,
                        "StableOrder", v.StableOrder, "Hp", v.Hp, "IntentCursor", v.IntentCursor); return;
                case BattleContributionTotals v:
                    Record("CombatantKey", v.CombatantKey, "EffectiveDamageDealtHp", v.EffectiveDamageDealtHp,
                        "EffectiveDamageTakenHp", v.EffectiveDamageTakenHp); return;
                case BattleRandomSnapshot v: Record("Stream", v.Stream, "PrdStates", v.PrdStates); return;
                case BattlePrdState v: Record("CombatantKey", v.CombatantKey, "Crit", v.Crit, "FailureCount", v.FailureCount); return;
                case Pcg32StreamState v:
                    Record("InitialCore", Core(v.Initial), "CurrentCore", Core(v.Current), "WordsConsumed", v.WordsConsumed); return;
                case BattleCombatantKey v:
                    Record("AttemptId", v.AttemptId, "Kind", v.Kind, "CharacterId", v.CharacterId,
                        "FaceId", v.FaceId, "EnemyInstanceKey", v.EnemyInstanceKey); return;
                case BattlePairKey v: Record("AttemptId", v.AttemptId, "FaceId", v.FaceId, "PairId", v.PairId); return;
                case FlowPos v: Record("X", v.x, "Y", v.y); return;
                case CandidateBattleOperationRecord v:
                    Record("Kind", v.Kind, "OperationId", v.OperationId, "OccurredAtUnixMilliseconds", v.OccurredAtUnixMilliseconds,
                        "BeforeSnapshot", v.BeforeSnapshot, "AfterSnapshot", v.AfterSnapshot, "Request", v.Request,
                        "Conditions", v.Conditions, "DirectFacts", v.DirectAttack == null ? (object)Array.Empty<BattleDamageFact>() : v.DirectAttack.DamageFacts,
                        "EnemyFacts", v.EnemyPhase == null ? (object)Array.Empty<CandidateEnemyIntentFact>() : v.EnemyPhase.OrderedIntents,
                        "StageFacts", v.StageDecision.OrderedFacts, "ContributionSegments", v.ContributionSegments,
                        "ConsumptionCoverage", v.ConsumptionCoverage); return;
                case CandidateStageSource v:
                    Record("PlayerId", v.PlayerId, "AttemptId", v.AttemptId, "OperationId", v.OperationId,
                        "ExpectedSceneRevision", v.ExpectedSceneRevision, "Actor", v.Actor, "Pair", v.Pair, "Route", v.Route); return;
                case CandidateBattleConditionValues v:
                    Record("PreferenceRevision", v.PreferenceRevision, "ItemUseEnabled", v.ItemUseEnabled); return;
                case BattleDamageFact v:
                    Record("Baseline", v.Baseline.Entry.EntryBaselineId, "PlayerId", v.PlayerId, "AttemptId", v.AttemptId,
                        "FaceId", v.FaceId, "OperationId", v.OperationId, "SceneRevision", v.SceneRevision,
                        "ActionOrdinal", v.ActionOrdinal, "Actor", v.Actor, "Target", v.Target, "Pair", v.Pair,
                        "EffectIndex", v.EffectIndex, "DamageKind", v.DamageKind, "Attack", v.Attack,
                        "PhysicalDefense", v.PhysicalDefense, "Multiplier", v.Multiplier, "RawDamage", v.RawDamage,
                        "MitigatedDamage", v.MitigatedDamage, "RoundedDamage", v.RoundedDamage,
                        "BlockPrevented", v.BlockPrevented, "ShieldAbsorbed", v.ShieldAbsorbed, "HpBefore", v.HpBefore,
                        "HpAfter", v.HpAfter, "HpLoss", v.HpLoss, "Overflow", v.Overflow,
                        "DefeatedTarget", v.DefeatedTarget, "Crit", v.Crit); return;
                case CandidateCritFact v:
                    Record("Actor", v.Actor, "Target", v.Target, "OpportunityOrdinal", v.OpportunityOrdinal,
                        "Parameters", v.Parameters, "Probability", v.Probability, "FailureCountBefore", v.FailureCountBefore,
                        "FailureCountAfter", v.FailureCountAfter, "Triggered", v.Triggered, "StreamBefore", v.StreamBefore,
                        "StreamAfter", v.StreamAfter, "Words", Words(v.Words)); return;
                case CandidateEnemyIntentFact v:
                    Record("Baseline", v.Baseline.Entry.EntryBaselineId, "PlayerId", v.PlayerId, "AttemptId", v.AttemptId,
                        "OperationId", v.OperationId, "FaceId", v.FaceId, "SceneRevision", v.SceneRevision,
                        "ActionOrdinal", v.ActionOrdinal, "EnemyPhaseOrdinal", v.EnemyPhaseOrdinal, "SegmentIndex", v.SegmentIndex,
                        "EnemyKey", v.EnemyKey, "Pair", v.Pair, "StableOrder", v.StableOrder, "IntentKind", v.IntentKind,
                        "CursorBefore", v.CursorBefore, "CursorAfter", v.CursorAfter, "Damage", v.Damage); return;
                case CandidateEnemyDamageFact v:
                    Record("ActorEnemy", v.ActorEnemy, "TargetMember", v.TargetMember, "DamageKind", v.DamageKind,
                        "Attack", v.Attack, "DamageCoefficient", v.DamageCoefficient, "PhysicalDefense", v.PhysicalDefense,
                        "RawDamage", v.RawDamage, "MitigatedDamage", v.MitigatedDamage, "RoundedDamage", v.RoundedDamage,
                        "BlockPrevented", v.BlockPrevented, "ShieldAbsorbed", v.ShieldAbsorbed, "HpBefore", v.HpBefore,
                        "HpAfter", v.HpAfter, "HpLoss", v.HpLoss, "Overflow", v.Overflow, "DefeatedTarget", v.DefeatedTarget); return;
                case CandidateStageFact v:
                    Record("Kind", v.Kind, "OperationId", v.OperationId, "SceneRevision", v.SceneRevision, "FaceId", v.FaceId,
                        "Pair", v.Pair, "SegmentIndex", v.SegmentIndex, "NextFaceId", v.NextFaceId, "Phase", v.Phase); return;
                case CandidateContributionSegment v:
                    Record("OperationId", v.OperationId, "SceneRevision", v.SceneRevision, "FaceId", v.FaceId,
                        "RuleSegment", v.RuleSegment, "SegmentIndex", v.SegmentIndex, "Actor", v.Actor, "Target", v.Target,
                        "Beneficiary", v.Beneficiary, "Kind", v.Kind, "HpLoss", v.HpLoss, "FactIndex", v.FactIndex); return;
                // These three fragment shapes are only for internal exact comparisons, not additional report fields.
                case CandidateEnemyPhaseFrame v:
                    Record("EnemyPhaseOrdinal", v.EnemyPhaseOrdinal, "Members", v.Members, "Enemies", v.Enemies,
                        "Contributions", v.Contributions, "Random", v.Random, "OrderedIntents", v.OrderedIntents); return;
                case CandidateStageDecision v:
                    Record("Kind", v.Source.Kind, "Source", v.Source, "FinalHp", v.FinalHp, "Board", v.Board,
                        "NextFaceIndex", v.NextFaceIndex, "NextPhase", v.NextPhase, "NextFace", v.NextFace, "OrderedFacts", v.OrderedFacts); return;
                case CandidateFinalHpValues v: Record("MemberHp", v.MemberHp, "EnemyHp", v.EnemyHp); return;
                case CandidateHpValue v: Record("CombatantKey", v.CombatantKey, "Hp", v.Hp); return;
                case IEnumerable values:
                    var rows = new List<object>(); foreach (var row in values) rows.Add(row);
                    output.WriteByte(5); Length(rows.Count); foreach (var row in rows) Value(row); return;
            }
            Value(EnumName(value));
        }

        private static string EnumName(object value)
        {
            switch (value)
            {
                case EntryCarryMode.Unspecified: return "Unspecified";
                case EntryCarryMode.Empty: return "Empty";
                case EntryCarryMode.NonEmpty: return "NonEmpty";
                case CharacterClassKind.Unspecified: return "Unspecified";
                case CharacterClassKind.Warrior: return "Warrior";
                case BaseStatsOrigin.Unspecified: return "Unspecified";
                case BaseStatsOrigin.ComputedBaseStats: return "ComputedBaseStats";
                case EnemyBehavior.Unspecified: return "Unspecified";
                case EnemyBehavior.NormalStrike: return "NormalStrike";
                case EnemyBehavior.ChargeHeavy: return "ChargeHeavy";
                case EnemyIntentKind.Unspecified: return "Unspecified";
                case EnemyIntentKind.Charge: return "Charge";
                case EnemyIntentKind.Strike: return "Strike";
                case EnemyTargeting.Unspecified: return "Unspecified";
                case EnemyTargeting.FirstLiving: return "FirstLiving";
                case EntryDamageKind.Unspecified: return "Unspecified";
                case EntryDamageKind.Physical: return "Physical";
                case BattlePhase.AwaitAction: return "AwaitAction";
                case BattlePhase.AwaitLinks: return "AwaitLinks";
                case BattlePhase.AwaitRescue: return "AwaitRescue";
                case BattlePhase.WonPendingSettlement: return "WonPendingSettlement";
                case BattlePhase.Closed: return "Closed";
                case BattleCombatantKind.Participant: return "Participant";
                case BattleCombatantKind.Enemy: return "Enemy";
                case CandidateRandomPurpose.Battle: return "Battle";
                case CandidateRandomPurpose.BaseReward: return "BaseReward";
                case CandidateRandomPurpose.Bonus: return "Bonus";
                case CandidateBattleOperationKind.Attack: return "Attack";
                case CandidateBattleOperationKind.Link: return "Link";
                case CandidateBattleFactKind.DirectAttack: return "DirectAttack";
                case CandidateBattleFactKind.EnemyIntent: return "EnemyIntent";
                case CandidateBattleFactKind.Stage: return "Stage";
                case CandidateContributionKind.DamageDealtHp: return "DamageDealtHp";
                case CandidateContributionKind.DamageTakenHp: return "DamageTakenHp";
                case CandidateConsumptionCoverage.EmptyCarryNoUse: return "EmptyCarryNoUse";
                case CandidateBattleOutcome.NormalVictory: return "NormalVictory";
                case CandidateStageOperation.AfterAttack: return "AfterAttack";
                case CandidateStageOperation.CompleteLink: return "CompleteLink";
                case CandidateStageFactKind.TemporaryRouteRemoved: return "TemporaryRouteRemoved";
                case CandidateStageFactKind.RouteLocked: return "RouteLocked";
                case CandidateStageFactKind.PendingLinkAdded: return "PendingLinkAdded";
                case CandidateStageFactKind.PendingLinkRemoved: return "PendingLinkRemoved";
                case CandidateStageFactKind.FaceChanged: return "FaceChanged";
                case CandidateStageFactKind.PhaseSelected: return "PhaseSelected";
                default: throw new ArgumentException("Unknown value in the fixed FMBR01 schema.", nameof(value));
            }
        }
    }
}
