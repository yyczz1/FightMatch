using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using FlowPuzzle.Core;
using NUnit.Framework;
using static FightMatch.Core.CandidateRewardRejectionCode;

namespace FightMatch.Core.Tests
{
    [TestFixture]
    public class CandidateBaseRewardsTests
    {
        [TestCase(1, 1, 26, 125, 4, 15, 1, 1, 1)]
        [TestCase(1, 4, 14, 125, 4, 15, 1, 9, 16)]
        [TestCase(3, 1, 31, 75, 2, 35, 2, 1, 1)]
        public void PublicWholeLevelAndM05EndingProduceExactOriginalEntryReward(int level, int entryLevel, int amount,
            int cn, int cd, int jn, int jd, int gn, int gd)
        {
            var s = Make(level, entryLevel); var state = Empty(s); var result = Fix(state, s); Accepted(result);
            var reward = result.BaseReward; var row = reward.Experience.Single();
            Assert.AreEqual(amount, (int)row.Amount); Value(row.Contribution, cn, cd); Value(row.Reference, jn, jd);
            Value(row.LevelMultiplier, gn, gd); Assert.AreEqual(entryLevel, (int)row.EntryLevel);
            Assert.AreEqual(level == 1 ? 30 : 35, (int)row.Dealt.Numerator);
            Assert.AreEqual(level == 1 ? 5 : 10, (int)row.Taken.Numerator);
            Assert.AreEqual(row.Amount, row.Score.LowerBound.Floor(Math())); Assert.AreEqual(row.Amount, row.Score.UpperBound.Floor(Math()));
            Assert.Greater(row.Score.TermsUsed, 0); Assert.AreSame(s.Report, reward.Report); Assert.AreSame(s.Ending, reward.Ending);
            Assert.AreSame(s.Definition, reward.Definition); Assert.AreEqual(s.Report.Fingerprint, reward.FinalReportFingerprint);
            Assert.AreEqual(s.Report.AttemptId, row.CombatantKey.AttemptId); Assert.AreEqual(s.Character.OriginalSlot, row.OriginalSlot);
            Assert.AreEqual("candidate:tin", reward.Materials[0].ItemId); Assert.AreEqual(new BigInteger(2), reward.Materials[0].Amount);
            Assert.AreEqual(BigInteger.Zero, reward.Materials[1].Amount); Assert.AreEqual(new BigInteger(2), result.Next.StateRevision);
            Assert.IsEmpty(state.BaseRewards); Assert.IsFalse(reward.CommitEligible); Assert.IsFalse(result.Next.CommitEligible);
            Assert.IsTrue(s.Ending.IsFirstClear); Assert.AreEqual(2, reward.Materials.Count, "No inferred first-clear extras.");
        }

        [Test]
        public void G01CurrentGrowthAndInventoryConsumeTheSameFixed14WithoutChangingTheScore()
        {
            var s = Make(1, 4); var fixedResult = Fix(Empty(s), s); Accepted(fixedResult); var reward = fixedResult.BaseReward;
            var growth = CandidateCharacterGrowth.ApplyBaseReward(s.Character, Experience(reward), s.Character.StateRevision, Math());
            Assert.IsTrue(growth.IsAccepted, growth.FieldPath); Assert.AreEqual(new BigInteger(5), growth.Next.Level);
            Assert.AreEqual(new BigInteger(6), growth.Next.Experience); Assert.AreEqual(new BigInteger(14), growth.RewardReceipt.Amount);
            var later = CandidateCharacterGrowth.ApplyBaseReward(s.Character, new CandidateBaseExperience { PlayerId = s.Report.PlayerId,
                CharacterId = s.Character.CharacterId, AttemptId = "other", SettlementId = "other", Context = Context(s.Input.Context), Amount = 300 },
                s.Character.StateRevision, Math()); Assert.IsTrue(later.IsAccepted);
            var afterLater = CandidateCharacterGrowth.ApplyBaseReward(later.Next, Experience(reward), later.Next.StateRevision, Math());
            Assert.IsTrue(afterLater.IsAccepted); Assert.AreEqual(new BigInteger(14), afterLater.ExperienceAdded);
            Assert.AreEqual(new BigInteger(4), reward.Experience[0].EntryLevel); Assert.AreEqual(new BigInteger(14), reward.Experience[0].Amount);
            var items = CandidateInventory.PrepareDefinition(new CandidateInventoryDefinitionInput { Context = Context(s.Input.Context),
                Items = reward.Materials.Select(x => new CandidateInventoryItemInput { ItemId = x.ItemId, Kind = CandidateInventoryItemKind.OrdinaryMaterial }).ToList() }, Math());
            Assert.IsTrue(items.IsAccepted, items.FieldPath);
            var inventory = CandidateInventory.CreateCandidate(items.Definition, reward.PlayerId, new List<CandidateInventoryActorInput> {
                new CandidateInventoryActorInput { CharacterId = s.Character.CharacterId, ClassId = s.Character.Definition.ClassId,
                    ClassKind = CharacterClassKind.Warrior, OriginalSlot = s.Character.OriginalSlot } }, Math());
            Assert.IsTrue(inventory.IsAccepted);
            var grant = CandidateInventory.GrantOrdinary(inventory.Next, new CandidateOrdinaryGrant { PlayerId = reward.PlayerId,
                AttemptId = reward.AttemptId, SettlementId = reward.SettlementId, Context = Context(s.Input.Context),
                Source = CandidateOrdinaryGrantSource.OrdinaryBaseReward, Items = reward.Materials.Select(x =>
                    new CandidateInventoryQuantityInput { ItemId = x.ItemId, Quantity = x.Amount }).ToList() }, inventory.Next.StateRevision, Math());
            Assert.IsTrue(grant.IsAccepted, grant.FieldPath); Assert.AreEqual(new BigInteger(2), grant.Next.Holdings[0].T);
            Assert.AreEqual(reward.SettlementId, grant.GrantReceipt.SettlementId); Assert.AreEqual(reward.AttemptId, growth.RewardReceipt.AttemptId);
            Assert.AreEqual(reward.SettlementId, s.Ending.SettlementId); Assert.IsFalse(reward.CommitEligible);
            using (var scope = Scope(logTerms: 0, liveBits: 0))
            {
                var retry = CandidateBaseRewards.FixNormal(fixedResult.Next, s.Report, s.Definition, s.Ending, Request(s, 1), scope);
                Accepted(retry); Assert.AreSame(reward, retry.BaseReward); Assert.AreEqual(0, scope.Budget.LogTermsUsed);
                Assert.AreEqual(0, scope.Budget.PeakReservedIntegerBits);
            }
        }

        [Test]
        public void DuplicateReturnsOriginalFromTheLaterStateAndReplayHasItsOwnOrdinaryReward()
        {
            var first = Make(); var a = Fix(Empty(first), first); Accepted(a);
            var second = Make(attempt: "a2"); second.Ending = End(second.Report, second.Character, out _, first.Progress);
            Assert.IsFalse(second.Ending.IsFirstClear);
            var b = Fix(a.Next, second); Accepted(b); Assert.AreEqual(2, b.Next.BaseRewards.Count);
            var retry = Fix(b.Next, first, expected: 1); Accepted(retry);
            Assert.AreEqual(CandidateRewardOutcome.AlreadyIncluded, retry.Outcome); Assert.AreSame(b.Next, retry.Next);
            Assert.AreSame(a.BaseReward, retry.BaseReward); Assert.AreEqual(new BigInteger(3), retry.Next.StateRevision);
            var query = CandidateBaseRewards.FindByAttempt(retry.Next, first.Report.AttemptId, Math());
            Assert.AreSame(a.BaseReward, query.BaseReward); Assert.AreSame(retry.Next, query.Next);
            var absent = CandidateBaseRewards.FindByAttempt(retry.Next, "absent", Math());
            Assert.IsTrue(absent.IsAccepted); Assert.AreEqual(CandidateRewardOutcome.NotFound, absent.Outcome);
            Assert.IsNull(absent.BaseReward); Assert.AreSame(retry.Next, absent.Next);
            Assert.AreEqual(new BigInteger(2), b.BaseReward.Materials[0].Amount);
        }

        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)] [TestCase(5)]
        [TestCase(6)] [TestCase(7)] [TestCase(8)] [TestCase(9)]
        public void FixedIdentityReportDefinitionAndEndingCannotBeReplaced(int mutation)
        {
            var s = Make(); var fixedResult = Fix(Empty(s), s); Accepted(fixedResult);
            var report = s.Report; var definition = s.Definition; var end = s.Ending; var request = Request(s, 1);
            switch (mutation)
            {
                case 0: request.SettlementId = "other"; break;
                case 1: request.ChallengeId = "other"; break;
                case 2: request.EntryBaselineId = "other"; break;
                case 3: request.FinalReportFingerprint = "other"; break;
                case 4: var input = DefinitionInput(s); input.BaseExperience = 21; definition = Prepare(input); break;
                case 5: var changed = DefinitionInput(s); changed.Materials[0].Amount = 3; definition = Prepare(changed); break;
                case 6: report = Copy(report, time: 1000); break;
                case 7: end = CopyEnd(end, first: !end.IsFirstClear); break;
                case 8: end = CopyEnd(end, receipt: "other"); break;
                default:
                    var other = Make(attempt: "other"); report = other.Report; end = other.Ending; definition = other.Definition;
                    request = Request(other, 2); request.SettlementId = s.Ending.SettlementId; break;
            }
            using (var scope = Scope(logTerms: 0))
                Rejected(CandidateBaseRewards.FixNormal(fixedResult.Next, report, definition, end, request, scope), RewardConflict);
            Assert.AreEqual(1, fixedResult.Next.BaseRewards.Count); Assert.AreSame(s.Report, fixedResult.BaseReward.Report);
        }

        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)] [TestCase(5)] [TestCase(6)]
        public void EveryFixRequestFieldIsRequired(int field)
        {
            var s = Make(); var request = Request(s, 1);
            switch (field) { case 0: request.PlayerId = null; break; case 1: request.AttemptId = null; break;
                case 2: request.ChallengeId = null; break; case 3: request.EntryBaselineId = null; break;
                case 4: request.SettlementId = null; break; case 5: request.FinalReportFingerprint = null; break;
                default: request.ExpectedStateRevision = null; break; }
            using (var scope = Scope()) Rejected(CandidateBaseRewards.FixNormal(Empty(s), s.Report, s.Definition, s.Ending, request, scope), MissingField);
        }
        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)] [TestCase(5)] [TestCase(6)] [TestCase(7)]
        public void NewRequestMustMatchTheReportAndCurrentRewardState(int field)
        {
            var s = Make(); var request = Request(s, 1);
            switch (field) { case 0: request.PlayerId = "wrong"; break; case 1: request.AttemptId = "wrong"; break;
                case 2: request.ChallengeId = "wrong"; break; case 3: request.EntryBaselineId = "wrong"; break;
                case 4: request.SettlementId = "wrong"; break; case 5: request.FinalReportFingerprint = "wrong"; break;
                case 6: request.ExpectedStateRevision = 2; break; default: request.ExpectedStateRevision = -1; break; }
            using (var scope = Scope()) Rejected(CandidateBaseRewards.FixNormal(Empty(s), s.Report, s.Definition, s.Ending, request, scope),
                field == 6 ? StaleContext : field == 7 ? InvalidValue : InconsistentBinding);
        }

        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)] [TestCase(5)] [TestCase(6)] [TestCase(7)]
        public void M05EndingAndDefinitionMustDescribeThisExactNormalAttempt(int mutation)
        {
            var s = Make(); var end = s.Ending; var definition = s.Definition;
            if (mutation < 3) end = CopyEnd(end, kind: mutation == 0 ? CandidateProgressionEndKind.NormalExit :
                mutation == 1 ? CandidateProgressionEndKind.ImmediateRestart : (CandidateProgressionEndKind)99);
            else if (mutation == 3) end = CopyEnd(end, nextAttempt: "other");
            else if (mutation == 4) { var other = Make(attempt: "other"); end = other.Ending; }
            else if (mutation == 5) { var input = DefinitionInput(s); input.LevelVersion = "other"; definition = Prepare(input); }
            else if (mutation == 6) { var input = DefinitionInput(s); input.Context.ContentFingerprint = "other"; definition = Prepare(input); }
            else
            {
                var b = end.Begin; var character = Character(Context(s.Input.Context), 1, slot: 0);
                var participant = new CandidateProgressionParticipant(CandidateCharacterGrowth.ComputeBaseStats(character, Math()), character.StateRevision);
                end = new CandidateProgressionEndReceipt(new CandidateProgressionBeginReceipt(b.PlayerId, b.Level, b.Context,
                    b.ChallengeId, b.AttemptId, b.EntryBaselineId, participant), EndFacts(end), end.IsFirstClear);
            }
            using (var scope = Scope()) Rejected(CandidateBaseRewards.FixNormal(Empty(s), s.Report, definition, end, Request(s, 1), scope),
                mutation < 4 ? UnsupportedBinding : InconsistentBinding);
        }

        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)] [TestCase(5)]
        [TestCase(6)] [TestCase(7)] [TestCase(8)] [TestCase(9)] [TestCase(10)] [TestCase(11)]
        [TestCase(12)] [TestCase(13)] [TestCase(14)] [TestCase(15)] [TestCase(16)] [TestCase(17)]
        public void CorrectlyRehashedIncompleteOrAlteredReportIsStillRejected(int mutation)
        {
            var s = Make(); var r = s.Report; var records = r.Operations.ToList(); var first = records[0]; var final = r.FinalSnapshot;
            CandidateFinalAttemptReport changed;
            switch (mutation)
            {
                case 0: changed = Copy(r, records: records.Take(1)); break;
                case 1: changed = Copy(r, records: records.Skip(1)); break;
                case 2: changed = Copy(r, records: Array.Empty<CandidateBattleOperationRecord>()); break;
                case 3: records.Add(first); changed = Copy(r, records: records); break;
                case 4: changed = Copy(r, contributions: r.Contributions.Concat(new[] { r.Contributions[0] })); break;
                case 5: changed = Copy(r, contributions: r.Contributions.Skip(1)); break;
                case 6:
                    var segments = r.Contributions.ToList(); segments[0] = Segment(segments[0], beneficiary: BattleCombatantKey.ForParticipant(r.AttemptId, "other"));
                    changed = Copy(r, contributions: segments); break;
                case 7: changed = Copy(r, final: Snapshot(final, members: new[] { new BattleMemberState(final.Members[0].CombatantKey, final.Members[0].Member, R(96)) })); break;
                case 8: changed = Copy(r, time: r.EndedAtUnixMilliseconds + 1); break;
                case 9: changed = Copy(r, terminal: "other"); break;
                case 10: changed = Copy(r, whole: R(0)); break;
                case 11: changed = Copy(r, final: Snapshot(final, board: new BattleBoardState(final.Board.Face))); break;
                case 12: changed = Copy(r, initial: Snapshot(r.InitialSnapshot, revision: 2)); break;
                case 13:
                    var wrongSegments = first.ContributionSegments.ToList(); wrongSegments[0] = Segment(wrongSegments[0], factIndex: 1);
                    records[0] = Record(first, contributions: wrongSegments); changed = Copy(r, records: records,
                        contributions: records.SelectMany(x => x.ContributionSegments)); break;
                case 14:
                    records[0] = Record(first, contributions: first.ContributionSegments.Concat(new[] { first.ContributionSegments[0] }));
                    changed = Copy(r, records: records, contributions: records.SelectMany(x => x.ContributionSegments)); break;
                case 15:
                    records[0] = Record(first, facts: first.OrderedFacts.Skip(1)); changed = Copy(r, records: records); break;
                case 16: changed = Copy(r, final: Snapshot(final, totals: new[] { new BattleContributionTotals(final.Members[0].CombatantKey, R(31), R(5)) })); break;
                default: changed = Copy(r, binding: Make().Report.Binding); break;
            }
            Assert.AreEqual(CandidateBattleReportFingerprint.Compute(changed, Math()), changed.Fingerprint);
            s.Report = changed; s.Ending = End(changed, s.Character, out _);
            var state = Empty(s); var result = Fix(state, s); Rejected(result);
            Assert.IsEmpty(state.BaseRewards); Assert.AreEqual(BigInteger.One, state.StateRevision);
        }

        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)] [TestCase(5)]
        public void FragmentFactsCannotLoseContributionsOwnershipOrHpConservation(int mutation)
        {
            var s = Make(); var report = s.Report; var records = report.Operations.ToList(); var r = records[0];
            var direct = r.DirectAttack; var hit = direct.DamageFacts[0]; var target = r.BeforeSnapshot.Enemies[0];
            var loss = mutation == 0 ? R(14) : hit.HpLoss; var overflow = mutation == 1 ? R(0) : hit.Overflow;
            var forgedHit = new BattleDamageFact(r.BeforeSnapshot, direct.Action, target, hit.Attack, hit.Multiplier,
                hit.RawDamage, hit.MitigatedDamage, hit.RoundedDamage, hit.HpAfter, loss, overflow, R(0), hit.Crit);
            var forgedDirect = new CandidateCombatFrame(direct.Binding, direct.BeforeSnapshot, direct.Action,
                direct.Enemies, direct.Random, direct.Contributions, forgedHit);
            var enemy = r.EnemyPhase;
            var forgedEnemy = new CandidateEnemyPhaseFrame(forgedDirect, enemy.EnemyPhaseOrdinal, enemy.Members, enemy.Enemies,
                enemy.Contributions, mutation == 2 ? Array.Empty<CandidateEnemyIntentFact>() : enemy.OrderedIntents);
            var facts = r.OrderedFacts.ToList(); facts[0] = new CandidateBattleOrderedFact(0, forgedHit);
            var segments = r.ContributionSegments.ToList();
            if (mutation == 3) segments[0] = Segment(segments[0], beneficiary: BattleCombatantKey.ForParticipant(report.AttemptId, "other"));
            if (mutation == 4) segments[0] = Segment(segments[0], kind: (CandidateContributionKind)99);
            records[0] = new CandidateBattleOperationRecord(r.Kind, r.OccurredAtUnixMilliseconds, r.BeforeSnapshot, r.AfterSnapshot,
                r.Conditions, forgedDirect, forgedEnemy, r.StageDecision, facts, segments,
                mutation == 5 ? (CandidateConsumptionCoverage)99 : r.ConsumptionCoverage);
            s.Report = Copy(report, records: records, contributions: records.SelectMany(x => x.ContributionSegments), rehash: mutation != 4 && mutation != 5);
            s.Ending = End(s.Report, s.Character, out _);
            Rejected(Fix(Empty(s), s));
        }

        [Test]
        public void WholeLevelReferenceIncludesEveryFaceAndFirstFaceCannotSettle()
        {
            var s = Make(faces: 2); Value(s.Report.WholeLevelInitialEnemyHp, 60);
            var result = Fix(Empty(s), s); Accepted(result); Value(result.BaseReward.Experience[0].Reference, 30);
            var firstFace = s.Report.Operations[1].AfterSnapshot;
            Assert.AreEqual(1, firstFace.CurrentFaceIndex); Assert.AreEqual(BattlePhase.AwaitAction, firstFace.Phase);
            s.Report = Copy(s.Report, records: s.Report.Operations.Take(2), final: firstFace,
                contributions: s.Report.Operations.Take(2).SelectMany(x => x.ContributionSegments), time: s.Report.Operations[1].OccurredAtUnixMilliseconds,
                terminal: s.Report.Operations[1].OperationId);
            s.Ending = End(s.Report, s.Character, out _); Rejected(Fix(Empty(s), s));
        }

        [TestCase(0, 0)] [TestCase(1, 10)] [TestCase(2, 20)] [TestCase(3, 0)]
        public void ExplicitZeroAndExactPowerPoliciesKeepCompleteSource(int mode, int amount)
        {
            var s = Make(); var input = DefinitionInput(s);
            if (mode == 0) input.BaseExperience = 0;
            if (mode == 1) { input.DamageWeight = R(0); input.TakenWeight = R(0); input.Materials.Clear(); }
            if (mode == 2) { input.TakenWeight = R(0); input.ReferenceHpDivisor = R(1); }
            if (mode == 3) { input.CurveBase = R(0); input.CurveLog = R(0); }
            s.Definition = Prepare(input);
            using (var scope = Scope(logTerms: 0))
            {
                var result = CandidateBaseRewards.FixNormal(Empty(s), s.Report, s.Definition, s.Ending, Request(s, 1), scope);
                Accepted(result); Assert.AreEqual(new BigInteger(amount), result.BaseReward.Experience[0].Amount);
                Assert.AreEqual(0, result.BaseReward.Experience[0].Score.TermsUsed); Assert.AreSame(s.Report, result.BaseReward.Report);
                Assert.AreEqual(CandidateConsumptionCoverage.EmptyCarryNoUse, result.BaseReward.ConsumptionCoverage);
                if (mode == 1) Assert.IsEmpty(result.BaseReward.Materials);
            }
            s.Report = Copy(s.Report, records: s.Report.Operations.Take(1)); s.Ending = End(s.Report, s.Character, out _);
            Rejected(Fix(Empty(s), s), IncompleteReport);
        }

        [TestCase(0, 1, 10)] [TestCase(0, 4, 0)] [TestCase(1, 4, 10)]
        public void PenaltyZeroToTheZeroPowerIsOneAndOtherExactPowersRemainExact(int q, int level, int amount)
        {
            var s = Make(1, level); var input = DefinitionInput(s); input.LevelPenaltyBase = R(q); input.CurveLog = R(0);
            s.Definition = Prepare(input); var result = Fix(Empty(s), s); Accepted(result);
            Assert.AreEqual(new BigInteger(amount), result.BaseReward.Experience[0].Amount);
            Value(result.BaseReward.Experience[0].LevelMultiplier, q == 0 && level == 4 ? 0 : 1);
        }

        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)] [TestCase(5)]
        [TestCase(6)] [TestCase(7)] [TestCase(8)] [TestCase(9)] [TestCase(10)] [TestCase(11)]
        [TestCase(12)] [TestCase(13)] [TestCase(14)] [TestCase(15)] [TestCase(16)] [TestCase(17)]
        [TestCase(18)] [TestCase(19)] [TestCase(20)] [TestCase(21)] [TestCase(22)] [TestCase(23)]
        [TestCase(24)] [TestCase(25)] [TestCase(26)] [TestCase(27)] [TestCase(28)] [TestCase(29)]
        [TestCase(30)] [TestCase(31)] [TestCase(32)]
        public void DefinitionNeverDefaultsMissingInvalidOrUnsupportedParameters(int mutation)
        {
            var s = Make(); var input = DefinitionInput(s);
            switch (mutation)
            {
                case 0: input.Context = null; break; case 1: input.Context.SourceNotes = null; break;
                case 2: input.RewardDefinitionId = null; break; case 3: input.Version = null; break;
                case 4: input.LevelId = null; break; case 5: input.LevelVersion = null; break;
                case 6: input.BaseExperience = null; break; case 7: input.OverlevelGrace = null; break;
                case 8: input.DamageWeight = null; break; case 9: input.TakenWeight = null; break;
                case 10: input.CurveBase = null; break; case 11: input.CurveLog = null; break;
                case 12: input.ReferenceHpDivisor = R(0); break; case 13: input.LevelPenaltyBase = R(2); break;
                case 14: input.BaseExperience = -1; break; case 15: input.Materials[0].Amount = -1; break;
                case 16: input.Materials.Add(input.Materials[0]); break; case 17: input.Materials = null; break;
                case 18: input.RequiredFeatures = null; break; case 19: input.RequiredFeatures.Add("first-clear-card"); break;
                case 20: input.ZeroContributionPolicy = CandidateZeroContributionPolicy.Unspecified; break;
                case 21: input.ZeroContributionPolicy = (CandidateZeroContributionPolicy)99; break;
                case 22: input.DropMode = (CandidateRewardDropMode)99; break;
                case 23: input.DamageWeight = R(-1); break; case 24: input.Materials[0].Amount = null; break;
                case 25: input.Materials[0] = null; break; case 26: input.Materials[0].ItemId = null; break;
                case 27: input.DropMode = CandidateRewardDropMode.Unspecified; break;
                case 28: input.LevelPenaltyBase = null; break; case 29: input.LevelPenaltyBase = R(-1); break;
                case 30: input.OverlevelGrace = -1; break; case 31: input.Context.SourceNotes.Clear(); break;
                default: input.ReferenceHpDivisor = null; break;
            }
            var result = CandidateBaseRewards.PrepareDefinition(input, Math());
            Assert.IsFalse(result.IsAccepted); Assert.IsNull(result.Definition); Assert.AreNotEqual(None, result.RejectionCode);
            Assert.IsNotEmpty(result.FieldPath);
        }

        [Test]
        public void FixedTableUsesZeroWordsAndRetainsBothOtherRandomDomains()
        {
            var s = Make(); var input = DefinitionInput(s); var definition = Prepare(input); s.Definition = definition;
            var before = CandidateBattleReportFingerprint.Compute(s.Report, Math()); var result = Fix(Empty(s), s); Accepted(result);
            var reward = result.BaseReward; var evidence = reward.Random;
            Assert.AreEqual(CandidateRewardRandomUse.NotUsedFixedTable, evidence.Use); Assert.IsEmpty(evidence.Words);
            Assert.AreEqual(BigInteger.Zero, evidence.WordsConsumed); Assert.AreSame(s.Report.Binding.BaseReward, evidence.Domain);
            Assert.AreSame(evidence.Before, evidence.After); Assert.AreSame(s.Report.Binding.BaseReward.Initial, evidence.Before);
            Assert.AreEqual(s.Report.Binding.SourceCapabilityId, evidence.SourceCapabilityId); Assert.AreEqual(s.Report.Binding.MappingId, evidence.MappingId);
            Assert.AreSame(s.Report.Binding.Battle, reward.Report.Binding.Battle); Assert.AreSame(s.Report.Binding.Bonus, reward.Report.Binding.Bonus);
            input.Materials[0].Amount = 999; input.Materials.Clear(); input.Context.SourceNotes.Clear(); input.BaseExperience = 999;
            Assert.AreEqual(new BigInteger(2), reward.Materials[0].Amount); Assert.AreEqual(new BigInteger(20), definition.BaseExperience);
            Assert.AreEqual(before, CandidateBattleReportFingerprint.Compute(s.Report, Math()));
            AssertReadOnly(result.Next, new HashSet<object>());
        }

        [TestCase("Math")] [TestCase("LogTerms")] [TestCase("LiveIntegerBits")]
        public void SharedLimitsFailWithoutPublishingAnyFixedResult(string reason)
        {
            var s = Make(); var state = Empty(s);
            using (var scope = reason == "Math" ? Scope(math: new ExactMathBudget(32)) :
                reason == "LogTerms" ? Scope(logTerms: 0) : Scope(liveBits: 1))
            {
                var ex = Assert.Throws<ExactMathLimitException>(() => CandidateBaseRewards.FixNormal(state, s.Report, s.Definition, s.Ending, Request(s, 1), scope));
                if (reason != "Math") Assert.AreEqual(reason, ex.ReasonCode);
                Assert.AreEqual(0, scope.Budget.ReservedIntegerBits);
            }
            Assert.AreEqual(BigInteger.One, state.StateRevision); Assert.IsEmpty(state.BaseRewards);
            var retry = Fix(state, s); Accepted(retry); Assert.AreEqual(new BigInteger(26), retry.BaseReward.Experience[0].Amount);
        }

        [Test]
        public void CompleteOperationSharesTheCallersMathThroughTheFinalScoringStep()
        {
            var s = Make(); var state = Empty(s); long steps;
            using (var scope = Scope()) { Accepted(CandidateBaseRewards.FixNormal(state, s.Report, s.Definition, s.Ending, Request(s, 1), scope)); steps = scope.Budget.Math.PrimitiveStepsUsed; }
            Assert.Greater(steps, 1);
            using (var scope = Scope(math: new ExactMathBudget(32768, (int)steps - 1)))
            {
                Assert.Throws<ExactMathLimitException>(() => CandidateBaseRewards.FixNormal(state, s.Report, s.Definition, s.Ending, Request(s, 1), scope));
                Assert.AreEqual(0, scope.Budget.ReservedIntegerBits);
            }
            Assert.IsEmpty(state.BaseRewards);
            using (var scope = Scope(math: new ExactMathBudget(32768, (int)steps)))
                Accepted(CandidateBaseRewards.FixNormal(state, s.Report, s.Definition, s.Ending, Request(s, 1), scope));
        }

        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)]
        public void NewSmallBudgetsRecheckLargeNumbersRetainedInsideTheOldFixedResult(int field)
        {
            var s = Make(); var huge = BigInteger.One << 100; var input = DefinitionInput(s);
            if (field == 0) input.Materials[0].Amount = huge;
            if (field == 1) { input.BaseExperience = huge; input.CurveLog = R(0); }
            if (field == 2) input.OverlevelGrace = huge;
            if (field == 3)
            {
                var r = s.Report; var records = r.Operations.ToList(); var last = records.Last();
                records[records.Count - 1] = Record(last, time: huge); s.Report = Copy(r, records: records, time: huge);
                s.Ending = End(s.Report, s.Character, out _);
            }
            s.Definition = Prepare(input); var result = Fix(Empty(s), s); Accepted(result);
            Assert.Throws<ExactMathLimitException>(() => CandidateBaseRewards.FindByAttempt(result.Next, "absent", new ExactMathBudget(64)));
            using (var scope = Scope(math: new ExactMathBudget(64)))
                Assert.Throws<ExactMathLimitException>(() => CandidateBaseRewards.FixNormal(result.Next, s.Report, s.Definition, s.Ending, Request(s, 1), scope));
            var retry = Fix(result.Next, s, expected: 1); Accepted(retry); Assert.AreSame(result.BaseReward, retry.BaseReward);
        }

        [Test]
        public void OneEvaluationScopeAccumulatesOutputReservationsAndLogTermsAcrossRewards()
        {
            var a = Make(); var b = Make(attempt: "second"); long terms;
            using (var probe = Scope()) { Accepted(CandidateBaseRewards.FixNormal(Empty(a), a.Report, a.Definition, a.Ending, Request(a, 1), probe)); terms = probe.Budget.LogTermsUsed; }
            using (var scope = Scope(logTerms: (int)terms))
            {
                var first = CandidateBaseRewards.FixNormal(Empty(a), a.Report, a.Definition, a.Ending, Request(a, 1), scope); Accepted(first);
                var retained = scope.Budget.ReservedIntegerBits; Assert.Greater(retained, 0);
                Assert.Throws<ExactMathLimitException>(() => CandidateBaseRewards.FixNormal(first.Next, b.Report, b.Definition, b.Ending, Request(b, 2), scope));
                Assert.AreEqual(retained, scope.Budget.ReservedIntegerBits); Assert.AreEqual(1, first.Next.BaseRewards.Count);
            }
        }

        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)] [TestCase(5)]
        [TestCase(6)] [TestCase(7)] [TestCase(8)] [TestCase(9)] [TestCase(10)] [TestCase(11)] [TestCase(12)]
        public void RootNullArgumentsKeepTheirProgrammingErrorContract(int root)
        {
            var s = Make(); using (var scope = Scope())
            {
                Assert.Throws<ArgumentNullException>(() =>
                {
                    if (root < 2) CandidateBaseRewards.PrepareDefinition(root == 0 ? null : DefinitionInput(s), root == 1 ? null : Math());
                    else if (root < 4) CandidateBaseRewards.CreateCandidate(root == 2 ? null : "p", root == 3 ? null : Math());
                    else if (root < 7) CandidateBaseRewards.FindByAttempt(root == 4 ? null : Empty(s), root == 5 ? null : "a", root == 6 ? null : Math());
                    else CandidateBaseRewards.FixNormal(root == 7 ? null : Empty(s), root == 8 ? null : s.Report, root == 9 ? null : s.Definition,
                        root == 10 ? null : s.Ending, root == 11 ? null : Request(s, 1), root == 12 ? null : scope);
                });
            }
        }

        private sealed class Scenario
        {
            internal BattleEntryInput Input;
            internal CandidateCharacterState Character;
            internal CandidateFinalAttemptReport Report;
            internal CandidateRewardDefinition Definition;
            internal CandidateProgressionEndReceipt Ending;
            internal CandidateProgressionState Progress;
        }
        private static Scenario Make(int level = 1, int entryLevel = 1, string attempt = "attempt", int faces = 1)
        {
            var fixture = DemoContentFixture.CaptureSource(level, 1, SourceCoordinateConvention.OneBasedBottomLeft);
            var context = new CandidateContext { DraftId = "isolated:023", DraftRevision = 1, ContentFingerprint = "candidate-only",
                RuleVersion = "candidate-r1", NumericContractVersion = "exact-r1", RandomContractVersion = "pcg-r1",
                SourceNotes = new List<string> { fixture.SourcePath + ":" + fixture.SourceLocator + ":" + fixture.SourceSha256,
                    fixture.CoordinateTransform, "conditional public 012 source; explicit C=1/1000, not a production calibration" } };
            var character = Character(context, entryLevel); var stats = CandidateCharacterGrowth.ComputeBaseStats(character, Math());
            var geometry = fixture.CopyLevel(); var face = new FaceInput { FaceId = "face0", Width = geometry.width, Height = geometry.height, Pairs = new List<PairInput>() };
            foreach (var pair in geometry.pairs.OrderBy(x => x.colorId))
            {
                var heavy = level == 3 && pair.colorId == 0;
                face.Pairs.Add(new PairInput { PairId = "pair" + pair.colorId, GeometryColorId = pair.colorId, EndpointA = pair.endpointA,
                    EndpointB = pair.endpointB, Enemy = new EnemyInput { EnemyInstanceKey = "enemy" + pair.colorId, EnemyDefinitionId = heavy ? "E02" : "E01",
                        OriginalSlot = pair.colorId, StableOrder = pair.colorId, Behavior = heavy ? EnemyBehavior.ChargeHeavy : EnemyBehavior.NormalStrike,
                        Stats = Stats(heavy ? 20 : 15, 10, heavy ? 20 : 0, 0),
                        IntentCycle = heavy ? new List<EnemyIntentInput> { new EnemyIntentInput { Kind = EnemyIntentKind.Charge,
                            Targeting = EnemyTargeting.FirstLiving, DamageKind = null, DamageCoefficient = null }, Strike(13, 10) } :
                            new List<EnemyIntentInput> { Strike(3, 5) } } });
            }
            var input = new BattleEntryInput { PlayerId = character.PlayerId, AttemptId = attempt, ChallengeId = "challenge:" + attempt,
                EntryBaselineId = "baseline:" + attempt, Context = context, CarryMode = EntryCarryMode.Empty, RequiredFeatures = new List<string>(),
                Level = new LevelInput { LevelId = fixture.FixtureKey, LevelVersion = "candidate-r1", RecommendedLevel = 1, Faces = new List<FaceInput> { face } },
                Members = new List<MemberInput> { new MemberInput { CharacterId = character.CharacterId, ClassId = stats.ClassId,
                    ClassKind = CharacterClassKind.Warrior, OriginalSlot = stats.OriginalSlot, Level = stats.Level, IsReady = stats.IsReady,
                    StatsOrigin = stats.StatsOrigin, StatsContext = Context(context), Stats = CopyStats(stats.Stats), EntryHp = stats.EntryHp,
                    LearnedSkills = new List<string>(), Crit = new WarriorCritInput { PassiveDefinitionId = stats.PassiveDefinitionId,
                        TargetProbability = stats.TargetProbability, C = R(1, 1000), Multiplier = stats.CritMultiplier } } } };
            for (var i = 1; i < faces; i++) input.Level.Faces.Add(new FaceInput { FaceId = "face" + i,
                Width = face.Width, Height = face.Height, Pairs = face.Pairs });
            var prepared = new BattleEntryPreparer().PrepareCandidate(input, Math()); Assert.IsTrue(prepared.IsAccepted, prepared.FieldPath);
            var bytes = new byte[48]; for (var offset = 0; offset < 48; offset += 16) { bytes[offset] = 42; bytes[offset + 8] = 54; }
            var bound = CandidateRandomPreparer.Prepare(prepared.Entry, new CandidateSeedMaterial { Bytes = bytes,
                SourceCapabilityId = "isolated:023", MappingId = CandidateRandomPreparer.SupportedMappingId }, Math()); Assert.IsTrue(bound.IsAccepted, bound.FieldPath);
            var created = CandidateBattleOperations.CreateCandidate(bound.Binding, Math()); Assert.IsTrue(created.IsAccepted, created.FieldPath);
            var run = created.Run; var routes = fixture.CopySolution().paths.OrderBy(x => x.colorId).Select(x => new List<FlowPos>(x.cells)).ToList();
            var operation = 0;
            for (var f = 0; f < faces; f++)
                for (var a = 0; a < (level == 3 ? 3 : 2); a++)
                {
                    var p = level == 3 ? (a < 2 ? 0 : 1) : a; var current = run.CurrentSnapshot;
                    var result = CandidateBattleOperations.EvaluateAttack(run, new CandidateAttackRequest { PlayerId = input.PlayerId,
                        AttemptId = attempt, OperationId = "op" + operation++, ExpectedSceneRevision = current.SceneRevision,
                        Actor = current.Members[0].CombatantKey, Pair = BattlePairKey.Create(attempt, current.Board.Face.FaceId, "pair" + p), Route = routes[p] },
                        new CandidateBattleConditions { PreferenceRevision = 1, ItemUseEnabled = true }, operation, new RandomSamplingBudget(Math()));
                    Assert.IsTrue(result.IsAccepted, result.RejectionCode + " " + result.FieldPath);
                    Assert.IsFalse(result.Record.DirectAttack.DamageFacts[0].Crit.Triggered); run = result.NextRun;
                }
            Assert.NotNull(run.FinalReport);
            var scenario = new Scenario { Input = input, Character = character, Report = run.FinalReport };
            scenario.Definition = Prepare(DefinitionInput(scenario, level)); scenario.Ending = End(scenario.Report, character, out scenario.Progress);
            return scenario;
        }
        private static CandidateRewardDefinitionInput DefinitionInput(Scenario s, int level = 1)
        { return new CandidateRewardDefinitionInput { Context = Context(s.Input.Context), RewardDefinitionId = "reward:" + s.Input.Level.LevelId,
            Version = "candidate-r1", LevelId = s.Input.Level.LevelId, LevelVersion = s.Input.Level.LevelVersion, BaseExperience = level == 3 ? 24 : 20,
            DamageWeight = R(1), TakenWeight = R(1, 4), CurveBase = R(1, 2), CurveLog = R(1, 2), ReferenceHpDivisor = R(2),
            LevelPenaltyBase = R(3, 4), OverlevelGrace = 1, ZeroContributionPolicy = CandidateZeroContributionPolicy.CurveIntercept,
            DropMode = CandidateRewardDropMode.FixedOrdinaryMaterials, RequiredFeatures = new List<string>(),
            Materials = new List<CandidateRewardMaterialInput> { new CandidateRewardMaterialInput { ItemId = "candidate:tin", Amount = 2 },
                new CandidateRewardMaterialInput { ItemId = "candidate:wood", Amount = 0 } } }; }
        private static CandidateCharacterState Character(CandidateContext context, int level, int slot = 2)
        {
            var definition = CandidateCharacterGrowth.PrepareDefinition(new GrowthDefinitionInput { Context = Context(context), ClassId = "warrior",
                ClassKind = CharacterClassKind.Warrior, PassiveDefinitionId = "warrior:crit", BaseStats = Stats(100, 20, 10, 6),
                GrowthHp = R(2, 25), GrowthAttack = R(3, 50), GrowthDefense = R(3, 2), CritBase = R(1, 5), CritStep = R(1, 200),
                CritCap = R(7, 20), CritMultiplier = R(3, 2), XpBase = 60, XpLinear = 20, XpQuadratic = 5, RecoveryDurationMilliseconds = R(180000) }, Math());
            Assert.IsTrue(definition.IsAccepted, definition.FieldPath);
            var result = CandidateCharacterGrowth.CreateCandidate(definition.Definition, "player:023", "W", level, level == 4 ? 157 : 0, slot, Math());
            Assert.IsTrue(result.IsAccepted, result.FieldPath); return result.Next;
        }
        private static CandidateProgressionEndReceipt End(CandidateFinalAttemptReport report, CandidateCharacterState character,
            out CandidateProgressionState after, CandidateProgressionState existing = null)
        {
            var e = report.Baseline.Entry; var context = Context(e.Context);
            var definition = CandidateProgression.PrepareDefinition(new CandidateProgressionDefinitionInput { Context = context,
                Levels = new List<CandidateProgressionLevelInput> { new CandidateProgressionLevelInput { LevelId = e.Level.LevelId,
                    LevelVersion = e.Level.LevelVersion, UnlockRuleId = "explicit-initial", EntryKind = CandidateProgressionEntryKind.Ordinary,
                    UnlockKind = CandidateProgressionUnlockKind.InitiallyOpen, UnlockAfterLevelId = null, RequiredFeatures = new List<string>() } } }, Math());
            Assert.IsTrue(definition.IsAccepted, definition.FieldPath);
            var state = existing ?? CandidateProgression.CreateCandidate(definition.Definition, e.PlayerId, Math()).Next;
            var begin = CandidateProgression.BeginAttempt(state, new CandidateProgressionBeginIntent { PlayerId = e.PlayerId, LevelId = e.Level.LevelId,
                LevelVersion = e.Level.LevelVersion, Context = context, CharacterId = character.CharacterId, ExpectedCharacterRevision = character.StateRevision,
                ExpectedOriginalSlot = character.OriginalSlot, ChallengeId = e.ChallengeId, AttemptId = e.AttemptId, EntryBaselineId = e.EntryBaselineId },
                character, state.StateRevision, Math()); Assert.IsTrue(begin.IsAccepted, begin.FieldPath);
            var result = CandidateProgression.EndAttempt(begin.Next, new CandidateProgressionEndFacts { PlayerId = e.PlayerId,
                LevelId = e.Level.LevelId, LevelVersion = e.Level.LevelVersion, Context = context, ChallengeId = e.ChallengeId,
                AttemptId = e.AttemptId, EntryBaselineId = e.EntryBaselineId, EndReceiptId = "end:" + e.AttemptId, Kind = CandidateProgressionEndKind.NormalVictory,
                SettlementId = "settlement:" + e.AttemptId, FinalReportFingerprint = report.Fingerprint, NewAttemptId = null },
                begin.Next.StateRevision, Math()); Assert.IsTrue(result.IsAccepted, result.FieldPath); after = result.Next; return result.EndReceipt;
        }
        private static CandidateBaseExperience Experience(CandidateFixedBaseReward r)
        { return new CandidateBaseExperience { PlayerId = r.PlayerId, CharacterId = r.Experience[0].CharacterId, AttemptId = r.AttemptId,
            SettlementId = r.SettlementId, Context = Context(r.Definition.Context), Amount = r.Experience[0].Amount }; }
        private static CandidateRewardDefinition Prepare(CandidateRewardDefinitionInput input)
        { var r = CandidateBaseRewards.PrepareDefinition(input, Math()); Assert.IsTrue(r.IsAccepted, r.FieldPath); return r.Definition; }
        private static CandidateRewardState Empty(Scenario s)
        { var r = CandidateBaseRewards.CreateCandidate(s.Report.PlayerId, Math()); Assert.IsTrue(r.IsAccepted); return r.Next; }
        private static CandidateRewardFixRequest Request(Scenario s, BigInteger revision)
        { return new CandidateRewardFixRequest { PlayerId = s.Report.PlayerId, AttemptId = s.Report.AttemptId, ChallengeId = s.Report.ChallengeId,
            EntryBaselineId = s.Report.EntryBaselineId, SettlementId = s.Ending.SettlementId,
            FinalReportFingerprint = s.Report.Fingerprint, ExpectedStateRevision = revision }; }
        private static CandidateRewardResult Fix(CandidateRewardState state, Scenario s, BigInteger? expected = null)
        { using (var scope = Scope()) return CandidateBaseRewards.FixNormal(state, s.Report, s.Definition, s.Ending, Request(s, expected ?? state.StateRevision), scope); }
        private static ExactMathBudget Math() { return new ExactMathBudget(); }
        private static ExactEvaluationScope Scope(ExactMathBudget math = null, int logTerms = 4096, int liveBits = 1048576)
        { return new ExactEvaluationScope(new ExactEvaluationBudget(math ?? Math(), liveBits, logTerms)); }
        private static ExactRational R(BigInteger n) { return R(n, 1); }
        private static ExactRational R(BigInteger n, BigInteger d) { return ExactRational.Create(n, d, Math()); }
        private static void Value(ExactRational value, int numerator, int denominator = 1)
        { Assert.AreEqual(new BigInteger(numerator), value.Numerator); Assert.AreEqual(new BigInteger(denominator), value.Denominator); }
        private static void Accepted(CandidateRewardResult r)
        { Assert.IsTrue(r.IsAccepted, r.RejectionCode + " " + r.FieldPath); Assert.NotNull(r.Next); Assert.NotNull(r.BaseReward); Assert.IsNull(r.FieldPath); }
        private static void Rejected(CandidateRewardResult r, CandidateRewardRejectionCode? code = null)
        { Assert.IsFalse(r.IsAccepted); Assert.IsNull(r.Next); Assert.IsNull(r.BaseReward); Assert.IsNotEmpty(r.FieldPath);
            if (code.HasValue) Assert.AreEqual(code.Value, r.RejectionCode); }
        private static CandidateFinalAttemptReport Copy(CandidateFinalAttemptReport r, IEnumerable<CandidateBattleOperationRecord> records = null,
            BattleSnapshot initial = null, BattleSnapshot final = null, IEnumerable<CandidateContributionSegment> contributions = null,
            BigInteger? time = null, string terminal = null, ExactRational whole = null, CandidateRandomBinding binding = null, bool rehash = true)
        {
            var copy = new CandidateFinalAttemptReport(binding ?? r.Binding, r.Baseline, initial ?? r.InitialSnapshot, records ?? r.Operations,
                final ?? r.FinalSnapshot, contributions ?? r.Contributions, r.Outcome, time ?? r.EndedAtUnixMilliseconds,
                terminal ?? r.TerminalOperationId, r.ConsumptionCoverage, whole ?? r.WholeLevelInitialEnemyHp, "untrusted");
            if (!rehash) return copy;
            return new CandidateFinalAttemptReport(copy.Binding, copy.Baseline, copy.InitialSnapshot, copy.Operations, copy.FinalSnapshot,
                copy.Contributions, copy.Outcome, copy.EndedAtUnixMilliseconds, copy.TerminalOperationId, copy.ConsumptionCoverage,
                copy.WholeLevelInitialEnemyHp, CandidateBattleReportFingerprint.Compute(copy, Math()));
        }
        private static CandidateBattleOperationRecord Record(CandidateBattleOperationRecord r, IEnumerable<CandidateContributionSegment> contributions = null,
            IEnumerable<CandidateBattleOrderedFact> facts = null, BigInteger? time = null)
        { return new CandidateBattleOperationRecord(r.Kind, time ?? r.OccurredAtUnixMilliseconds, r.BeforeSnapshot, r.AfterSnapshot,
            r.Conditions, r.DirectAttack, r.EnemyPhase, r.StageDecision, facts ?? r.OrderedFacts, contributions ?? r.ContributionSegments, r.ConsumptionCoverage); }
        private static CandidateContributionSegment Segment(CandidateContributionSegment s, BattleCombatantKey beneficiary = null,
            int? factIndex = null, CandidateContributionKind? kind = null)
        { return new CandidateContributionSegment(s.OperationId, s.SceneRevision, s.FaceId, s.RuleSegment, s.SegmentIndex, s.Actor, s.Target,
            beneficiary ?? s.Beneficiary, kind ?? s.Kind, s.HpLoss, factIndex ?? s.FactIndex); }
        private static BattleSnapshot Snapshot(BattleSnapshot s, IEnumerable<BattleMemberState> members = null,
            IEnumerable<BattleContributionTotals> totals = null, BattleBoardState board = null, BigInteger? revision = null)
        { return new BattleSnapshot(s.Baseline, revision ?? s.SceneRevision, s.EffectiveActionsCompleted, s.EnemyPhasesCompleted,
            s.CurrentFaceIndex, s.Phase, board ?? s.Board, members ?? s.Members, s.Enemies, s.Random, totals ?? s.Contributions); }
        private static CandidateProgressionEndFacts EndFacts(CandidateProgressionEndReceipt end)
        { return new CandidateProgressionEndFacts { EndReceiptId = end.EndReceiptId, Kind = end.Kind, SettlementId = end.SettlementId,
            FinalReportFingerprint = end.FinalReportFingerprint, NewAttemptId = end.NewAttemptId }; }
        private static CandidateProgressionEndReceipt CopyEnd(CandidateProgressionEndReceipt end, CandidateProgressionEndKind? kind = null,
            string nextAttempt = null, bool? first = null, string receipt = null)
        { var facts = EndFacts(end); facts.Kind = kind ?? end.Kind; facts.NewAttemptId = nextAttempt ?? end.NewAttemptId;
            facts.EndReceiptId = receipt ?? end.EndReceiptId; return new CandidateProgressionEndReceipt(end.Begin, facts, first ?? end.IsFirstClear); }
        private static CandidateContext Context(RuleContext c)
        { return new CandidateContext { DraftId = c.DraftId, DraftRevision = c.DraftRevision, ContentFingerprint = c.ContentFingerprint,
            RuleVersion = c.RuleVersion, NumericContractVersion = c.NumericContractVersion, RandomContractVersion = c.RandomContractVersion,
            SourceNotes = new List<string>(c.SourceNotes) }; }
        private static CandidateContext Context(PreparedRuleContext c)
        { return new CandidateContext { DraftId = c.DraftId, DraftRevision = c.DraftRevision, ContentFingerprint = c.ContentFingerprint,
            RuleVersion = c.RuleVersion, NumericContractVersion = c.NumericContractVersion, RandomContractVersion = c.RandomContractVersion,
            SourceNotes = new List<string>(c.SourceNotes) }; }
        private static StatsInput Stats(int hp, int attack, int defense, int magic)
        { return new StatsInput { MaxHp = R(hp), Attack = R(attack), PhysicalDefense = R(defense), MagicDefense = R(magic), Evasion = R(0), AttackRange = 1 }; }
        private static StatsInput CopyStats(PreparedStats s)
        { return new StatsInput { MaxHp = s.MaxHp, Attack = s.Attack, PhysicalDefense = s.PhysicalDefense, MagicDefense = s.MagicDefense,
            Evasion = s.Evasion, AttackRange = s.AttackRange }; }
        private static EnemyIntentInput Strike(int n, int d)
        { return new EnemyIntentInput { Kind = EnemyIntentKind.Strike, Targeting = EnemyTargeting.FirstLiving, DamageKind = EntryDamageKind.Physical, DamageCoefficient = R(n, d) }; }
        private static void AssertReadOnly(object value, HashSet<object> seen)
        {
            if (value == null || value is string || value.GetType().IsValueType || !seen.Add(value)) return;
            if (value is IList list) { Assert.IsTrue(list.IsReadOnly); Assert.Throws<NotSupportedException>(() => list.Clear()); }
            if (value is IEnumerable rows) { foreach (var row in rows) AssertReadOnly(row, seen); return; }
            foreach (var property in value.GetType().GetProperties())
            { Assert.IsNull(property.GetSetMethod()); AssertReadOnly(property.GetValue(value), seen); }
        }
    }
}
