using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using FightMatch.Application;
using FlowPuzzle.Core;
using NUnit.Framework;
using static FightMatch.Core.Tests.PlayerSessionTestData;
using static FightMatch.Core.Tests.PlayerRosterTestData;

namespace FightMatch.Core.Tests
{
    public class CandidateRosterBattleTests
    {
        [Test] public void CA05_ActorKeyAndFrontSlotStayIndependentAcrossDamageDeathAndPrd()
        {
            var f = new IsolatedRoster(40); f.Commit(f.EntryRequest()); var initial = f.Head.Business.ActiveHistory.CurrentRun;
            CollectionAssert.AreEqual(new[] { "C", "A", "B" }, initial.Baseline.Entry.Members.Select(x => x.CharacterId));
            Assert.AreEqual(3, f.IdCalls); Assert.AreEqual(1, f.EntropyCalls);
            var first = f.Attack("B", 0);
            Assert.AreEqual("B", first.DirectAttack.Action.Actor.CharacterId);
            CollectionAssert.AreEqual(new[] { "C", "A" }, first.EnemyPhase.OrderedIntents.Select(x => x.Damage.TargetMember.CharacterId));
            Assert.IsTrue(first.AfterSnapshot.Members.Single(x => x.Member.CharacterId == "C").Hp.Numerator.IsZero);
            foreach (var member in first.AfterSnapshot.Members)
            {
                var total = first.AfterSnapshot.Contributions.Single(x => x.CombatantKey.Equals(member.CombatantKey));
                Assert.AreEqual(member.Member.CharacterId == "B" ? 1 : 0, total.EffectiveDamageDealtHp.Numerator.Sign);
                Assert.AreEqual(member.Member.CharacterId == "B" ? 0 : 1, total.EffectiveDamageTakenHp.Numerator.Sign);
                var prd = first.AfterSnapshot.Random.PrdStates.Single(x => x.CombatantKey.Equals(member.CombatantKey));
                if (member.Member.CharacterId != "B") Assert.AreEqual(BigInteger.Zero, prd.FailureCount);
            }
            var second = f.Attack("B", 0);
            Assert.AreEqual("A", second.EnemyPhase.OrderedIntents.Single().Damage.TargetMember.CharacterId);
            Assert.IsTrue(second.AfterSnapshot.Members.Single(x => x.Member.CharacterId == "A").Hp.Numerator.IsZero);
            Assert.AreEqual(first.AfterSnapshot.Random.PrdStates.Single(x => x.CombatantKey.CharacterId == "B").FailureCount,
                second.DirectAttack.DamageFacts[0].Crit.FailureCountBefore);
            Assert.AreEqual(new BigInteger(2), second.AfterSnapshot.Random.Stream.WordsConsumed);
            Assert.AreEqual(1, f.EntropyCalls);
            Assert.AreEqual(0, second.AfterSnapshot.Contributions.Single(x => x.CombatantKey.CharacterId == "B").EffectiveDamageTakenHp.Numerator.Sign);
        }
        [Test] public void CA06_WholeLevelScoreAndEndReceiptsArePerOriginalParticipantWhileMaterialsCommitOnce()
        {
            var f = new IsolatedRoster(40); f.Commit(f.EntryRequest());
            f.Attack(); f.Attack(); f.Attack("B", 1); f.Attack("B", 1);
            var report = f.Head.Business.ActiveHistory.CurrentRun.FinalReport;
            Assert.IsNotNull(report); Assert.AreEqual(BattlePhase.WonPendingSettlement, f.Head.Business.ActiveHistory.CurrentRun.CurrentSnapshot.Phase);
            var originalEntry = report.Baseline.Entry; var request = f.Settle(); var b = f.Head.Business;
            var reward = b.Rewards.BaseRewards.Single(); Assert.AreEqual(report.Fingerprint, reward.FinalReportFingerprint);
            Assert.AreEqual(3, reward.Experience.Count); Assert.AreEqual(1, b.Progression.FirstClears.Count);
            Assert.AreEqual(new BigInteger(2), b.Inventory.Holdings.Single(x => x.ItemId == "tin").T);
            foreach (var xp in reward.Experience)
            {
                var total = report.FinalSnapshot.Contributions.Single(x => x.CombatantKey.CharacterId == xp.CharacterId);
                Assert.AreEqual(0, xp.Dealt.Compare(total.EffectiveDamageDealtHp, Codec().Math));
                Assert.AreEqual(0, xp.Taken.Compare(total.EffectiveDamageTakenHp, Codec().Math));
                Assert.AreEqual(originalEntry.Members.Single(x => x.CharacterId == xp.CharacterId).Level, xp.EntryLevel);
                using (var scope = new ExactEvaluationScope(new ExactEvaluationBudget(Codec().Math, 1048576, 4096)))
                {
                    var d = reward.Definition;
                    var contribution = total.EffectiveDamageDealtHp.Multiply(d.DamageWeight, scope.Budget.Math)
                        .Add(total.EffectiveDamageTakenHp.Multiply(d.TakenWeight, scope.Budget.Math), scope.Budget.Math);
                    var expected = ExactScoreCalculator.Evaluate(d.BaseExperience, R(1), d.CurveBase, d.CurveLog, contribution,
                        report.WholeLevelInitialEnemyHp.Divide(d.ReferenceHpDivisor, scope.Budget.Math), scope);
                    Assert.AreEqual(expected.Amount, xp.Amount);
                }
                Assert.AreEqual(xp.Amount, b.Roster.Find(xp.CharacterId).BaseRewards.Single().Amount);
                Assert.AreEqual(new BigInteger(3), b.Roster.Find(xp.CharacterId).StateRevision);
            }
            Assert.IsEmpty(b.Roster.Find("D").BaseRewards); Assert.IsEmpty(b.Roster.Find("D").ProcessedEnds);
            Assert.AreEqual(BigInteger.One, b.Roster.Find("D").StateRevision);
            var lookup = TakeCore(CandidateApplicationProtocol.Lookup(f.Head, request.Intent, Codec()));
            Assert.AreEqual(3, lookup.CharacterEnds.Count); Assert.AreEqual(3, lookup.CharacterExperiences.Count);
            Assert.AreEqual(1, lookup.CharacterEnds.Select(x => x.EndReceiptId).Distinct().Count());
            Assert.Throws<InvalidOperationException>(() => { var single = lookup.CharacterEnd; });
            Assert.Throws<InvalidOperationException>(() => { var single = lookup.CharacterExperience; });
            Assert.IsNotNull(b.Roster.Find("C").ActiveRecovery); Assert.IsNotNull(b.Roster.Find("A").ActiveRecovery);
            Assert.IsNull(b.Roster.Find("B").ActiveRecovery);
            var bytes = Encode(f.Envelope);
            Assert.AreEqual(lookup.OriginalCommitId, TakeCore(CandidateApplicationProtocol.Lookup(f.Head, request.Intent, Codec())).OriginalCommitId);
            var refused = CandidateApplicationProtocol.Propose(f.Head, b, request.Intent, new CandidateApplicationResultInput(), null, Codec());
            Assert.AreEqual("OperationAlreadyRecorded", refused.RejectionCode); CollectionAssert.AreEqual(bytes, Encode(f.Envelope));
        }
        [Test] public void CA07_CA10_JointRecoveryKeepsNotDueSlotAndRestartNeverAddsLaterRecoveredMember()
        {
            var f = new IsolatedRoster(40); f.Commit(f.EntryRequest()); f.Attack(); f.Attack(); f.End();
            var before = f.Head; var originalC = before.Business.Roster.Find("C").ActiveRecovery;
            var request = f.EntryRequest(Time(180000)); var bytes = request.Intent.CanonicalBytes.ToArray();
            Assert.AreEqual(0, before.Business.Roster.Find("C").ActiveRecovery.Elapsed.Numerator.Sign);
            f.Commit(request); var entered = f.Head.Business.ActiveHistory.CurrentRun;
            CollectionAssert.AreEqual(new[] { "C", "B" }, entered.Baseline.Entry.Members.Select(x => x.CharacterId));
            Assert.IsNotNull(f.Head.Business.Roster.Find("A").ActiveRecovery);
            Assert.AreEqual(originalC.RecoveryId, f.Head.Records.Last().Result.RecoveryResults.Single(x => x.CharacterId == "C").RecoveryId);
            CollectionAssert.AreEqual(bytes, request.Intent.CanonicalBytes);
            var charA = f.Head.Business.Roster.Find("A");
            var recover = Prepared(CandidateLifecyclePreparation.Prepare(new CandidateLifecycleDraft { PlayerId = f.Head.Business.PlayerId,
                ExpectedCommitId = f.Head.Header.CommitId, Kind = CandidateApplicationKind.AdvanceRecovery, Content = f.Content,
                AdvanceRecovery = new CandidateApplicationRecoveryInput { CharacterId = "A", RecoveryId = charA.ActiveRecovery.RecoveryId,
                    ExpectedCharacterRevision = charA.StateRevision, TimeSample = Time(360000) } }, Codec(), true));
            f.Commit(recover); Assert.IsTrue(f.Head.Business.Roster.Find("A").IsReady);
            var entropy = f.EntropyCalls; f.End(true); var restarted = f.Head.Business.ActiveHistory.CurrentRun;
            CollectionAssert.AreEqual(new[] { "C", "B" }, restarted.Baseline.Entry.Members.Select(x => x.CharacterId));
            CollectionAssert.AreEqual(new[] { 0, 2 }, restarted.Baseline.Entry.Members.Select(x => x.OriginalSlot));
            Assert.AreEqual(entered.Baseline.Entry.EntryBaselineId, restarted.Baseline.Entry.EntryBaselineId);
            Assert.AreNotEqual(entered.Baseline.Entry.AttemptId, restarted.Baseline.Entry.AttemptId);
            for (var i = 0; i < entered.Baseline.Entry.Members.Count; i++)
            {
                var a = entered.Baseline.Entry.Members[i]; var b = restarted.Baseline.Entry.Members[i];
                Assert.AreEqual(a.CharacterId, b.CharacterId); Assert.AreEqual(a.Level, b.Level);
                Assert.IsTrue(CandidateBattleReportFingerprint.Equal(a.Stats, b.Stats, Codec().Math));
                Assert.AreEqual(0, a.EntryHp.Compare(b.EntryHp, Codec().Math));
            }
            foreach (var pair in new[] { new[] { entered.Binding.Battle, restarted.Binding.Battle },
                new[] { entered.Binding.BaseReward, restarted.Binding.BaseReward }, new[] { entered.Binding.Bonus, restarted.Binding.Bonus } })
            { Assert.AreEqual(pair[0].InitState, pair[1].InitState); Assert.AreEqual(pair[0].InitSequence, pair[1].InitSequence); }
            Assert.AreEqual(entropy, f.EntropyCalls); Assert.AreEqual(BigInteger.Zero, restarted.CurrentSnapshot.Random.Stream.WordsConsumed);
            f.End(); f.Commit(f.EntryRequest(Time(360000)));
            Assert.AreEqual(entropy + 1, f.EntropyCalls); Assert.AreEqual(3, f.Head.Business.ActiveHistory.CurrentRun.Baseline.Entry.Members.Count);
        }
        [TestCase("members-order")] [TestCase("prd-order")] [TestCase("prd-missing")] [TestCase("prd-duplicate")]
        [TestCase("totals-order")] [TestCase("foreign-member")]
        public void CA05_MalformedCollectionProofCannotSubstituteFirstMember(string defect)
        {
            var f = new IsolatedRoster(); f.Commit(f.EntryRequest()); var run = f.Head.Business.ActiveHistory.CurrentRun;
            var s = run.CurrentSnapshot; var members = s.Members.ToList(); var prd = s.Random.PrdStates.ToList(); var totals = s.Contributions.ToList();
            if (defect == "members-order") members.Reverse();
            if (defect == "prd-order") prd.Reverse();
            if (defect == "prd-missing") prd.RemoveAt(1);
            if (defect == "prd-duplicate") prd[1] = prd[0];
            if (defect == "totals-order") totals.Reverse();
            if (defect == "foreign-member") members[1] = new BattleMemberState(members[0].CombatantKey, members[1].Member, members[1].Hp);
            var bad = new BattleSnapshot(s.Baseline, s.SceneRevision, s.EffectiveActionsCompleted, s.EnemyPhasesCompleted, s.CurrentFaceIndex,
                s.Phase, s.Board, members, s.Enemies, new BattleRandomSnapshot(s.Random.Stream, prd), totals);
            var broken = new CandidateBattleRun(run.Binding, run.Baseline, run.InitialSnapshot, bad, run.Records, null);
            var bytes = Encode(f.Envelope);
            var result = CandidateBattleOperations.EvaluateAttack(broken, new CandidateAttackRequest { PlayerId = f.Head.Business.PlayerId,
                AttemptId = s.Baseline.Entry.AttemptId, OperationId = "malformed", ExpectedSceneRevision = s.SceneRevision,
                Actor = s.Members.Last().CombatantKey, Pair = s.Enemies[0].PairKey,
                Route = new List<FlowPos> { new FlowPos(0, 0), new FlowPos(1, 0), new FlowPos(2, 0), new FlowPos(3, 0) } },
                new CandidateBattleConditions { PreferenceRevision = 1, ItemUseEnabled = false }, 101, new RandomSamplingBudget(Codec().Math));
            Assert.IsFalse(result.IsAccepted); Assert.IsNotEmpty(result.FieldPath); CollectionAssert.AreEqual(bytes, Encode(f.Envelope));
        }
        [Test] public void CA04_CA18_NonEmptyCarryAndMissingCoefficientRejectBeforeIdentityEntropyOrRecoveryPublication()
        {
            var f = new IsolatedRoster(tactical: true); var before = f.Head; var b = before.Business;
            var equipped = CandidateInventory.Equip(b.Inventory, new CandidateEquipIntent { CharacterId = "B", ItemId = "item:B", L = 0 },
                b.Inventory.StateRevision, Codec().Math);
            Assert.IsTrue(equipped.IsAccepted);
            var business = TakeCore(CandidateBusinessSaveCodec.Prepare(new CandidateBusinessInput(b.PlayerId, b.Roster, equipped.Next,
                b.Progression, b.Rewards, null, b.RetainedRuns, b.RetainedRollbacks, b.Format), SavePurpose.PlayerSave, Codec()));
            // A negative unsupported business candidate only; this is never committed or presented as a verified M02/session head.
            f.Head = new CandidateApplicationSnapshot(business, before.Records.ToList(), before.Continuation, before.Header, before.Descriptor);
            var blocked = f.Build(f.EntryRequest()); Assert.IsFalse(blocked.IsAccepted);
            Assert.AreEqual("CarryMode", blocked.Diagnostic.FieldPath); Assert.AreEqual(0, f.IdCalls); Assert.AreEqual(0, f.EntropyCalls);
            f.Head = before; f.Content.CritCoefficients = Array.Empty<CandidateCritCoefficient>();
            blocked = f.Build(f.EntryRequest()); Assert.IsFalse(blocked.IsAccepted); Assert.AreEqual("UnsupportedBinding", blocked.Diagnostic.Code);
            Assert.AreEqual(0, f.IdCalls); Assert.AreEqual(0, f.EntropyCalls); Assert.AreSame(before, f.Head);
        }
    }
}
