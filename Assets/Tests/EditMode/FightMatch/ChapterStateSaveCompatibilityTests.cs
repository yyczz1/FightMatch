using System;
using System.Linq;
using System.IO;
using FightMatch.Application;
using NUnit.Framework;
using Data = FightMatch.Core.Tests.PlayerSessionTestData;

namespace FightMatch.Core.Tests
{
    public sealed class ChapterStateSaveCompatibilityTests
    {
        [Test]
        public void NewProfileAndTeachingWithoutBattleDeclareStateCapabilityInTheirOwners()
        {
            var fixture = new ChapterTeachingFixture(initiallyOpenBoss: true);
            using (var rig = new ChapterTeachingRig(fixture))
            {
                CheckNoBattle(rig, fixture);
                rig.Gift();
                CheckNoBattle(rig, fixture);
                rig.Learn(); rig.Explain();
                CheckNoBattle(rig, fixture);
            }
        }

        private static void CheckNoBattle(ChapterTeachingRig rig, ChapterTeachingFixture fixture)
        {
            Assert.IsNull(rig.Head.Business.ActiveHistory);
            var envelope = Data.Envelope(rig.Storage, rig.Head.Header.CommitId);
            foreach (var slice in envelope.SliceDirectory.Where(x => x.Contract.OwnerId == "M02" || x.Contract.OwnerId == "M03" ||
                x.Contract.OwnerId == "M04" || x.Contract.OwnerId == "M05"))
                CollectionAssert.Contains(slice.Requirements.FeatureIds, ChapterRuleDefinitions.StateFeatureId);
            var closure = Data.Closure(fixture.Publication);
            var decoded = Data.TakeCore(CandidateApplicationSaveCodec.DecodePublished(envelope, closure, Data.Codec()));
            Assert.IsNull(decoded.Business.ActiveHistory);
            Assert.AreEqual(rig.Head.Business.Progression.GetPermanentEffects().Count, decoded.Business.Progression.GetPermanentEffects().Count);
            var body = Data.TakeCore(CandidateBusinessSaveCodec.EncodePublished(decoded.Business, decoded.Header, closure, Data.Codec()));
            var restored = Data.TakeCore(CandidateBusinessSaveCodec.DecodePublished(body, closure, Data.Codec()));
            CollectionAssert.AreEqual(Data.Encode(body), Data.Encode(Data.TakeCore(CandidateBusinessSaveCodec.EncodePublished(restored, decoded.Header, closure, Data.Codec()))));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void MissingOrUnknownStateCapabilityRejectsWithoutTouchingOriginalEnvelope(bool unknown)
        {
            var fixture = new ChapterTeachingFixture(initiallyOpenBoss: true);
            using (var rig = new ChapterTeachingRig(fixture))
            {
                var envelope = Data.Envelope(rig.Storage, rig.Head.Header.CommitId);
                var original = Data.Encode(envelope);
                var slices = envelope.SliceDirectory.Select((row, i) => new SaveSliceInput {
                    Contract = row.Contract, Bytes = envelope.SliceBytes[i].ToArray(), Requirements = row.Requirements }).ToArray();
                var requirements = slices[0].Requirements;
                var features = requirements.FeatureIds.Where(x => x != ChapterRuleDefinitions.StateFeatureId).ToList();
                if (unknown) features.Add("CH1.TauntBossStateV2");
                slices[0].Requirements = new SaveRequirements(requirements.Bindings, requirements.RuleVersions,
                    requirements.NumericContractVersions, requirements.RandomContractVersions, features);
                var altered = Data.TakeCore(SaveEnvelopeCodec.Prepare(new SaveEnvelopeInput {
                    Purpose = envelope.Purpose, PlayerId = envelope.PlayerId, SaveGeneration = envelope.SaveGeneration,
                    CommitId = envelope.CommitId, ParentCommitId = envelope.ParentCommitId, CommitIndex = envelope.CommitIndex,
                    RequiredSliceContracts = slices.Select(x => x.Contract).ToArray(), Slices = slices }, Data.Codec()));
                var rejected = CandidateApplicationSaveCodec.DecodePublished(altered, Data.Closure(fixture.Publication), Data.Codec());
                Assert.IsFalse(rejected.IsAccepted); Assert.AreEqual("UnsupportedBinding", rejected.RejectionCode);
                CollectionAssert.AreEqual(original, Data.Encode(envelope));
                CollectionAssert.AreEqual(original, Data.Encode(Data.Envelope(rig.Storage, rig.Head.Header.CommitId)));
            }
        }

        [Test]
        public void SkillOnlyFactsWindAndCooldownSurviveActualApplicationColdRecovery()
        {
            var fixture = new ChapterTeachingFixture(initiallyOpenBoss: true);
            Data.MemorySave storage;
            PublishedContentTestData.MemoryStorage locator;
            byte[] expected;
            using (var rig = new ChapterTeachingRig(fixture))
            {
                rig.CompleteTeaching(); rig.Enter(16); rig.Act(pair: 1);
                var first = rig.Head.Business.ActiveHistory.CurrentRun.Records.Single();
                Assert.IsEmpty(first.DirectAttack.DamageFacts);
                Assert.AreEqual(1, first.DirectAttack.SkillCasts.Count);
                Assert.IsTrue(first.EnemyPhase.OrderedIntents.Any(x => x.Wind != null));
                Assert.AreEqual(2, first.AfterSnapshot.ChapterState.MemberSkills.Single().CooldownRemaining);
                rig.Act(pair: 0);
                Assert.AreEqual(1, rig.Head.Business.ActiveHistory.CurrentRun.CurrentSnapshot.ChapterState.MemberSkills.Single().CooldownRemaining);
                var run = rig.Head.Business.ActiveHistory.CurrentRun;
                expected = CandidateBattleReportFingerprint.Encode(new object[] { run.CurrentSnapshot, run.Records }, Data.Codec().Math);
                storage = rig.Storage; locator = rig.LocatorStorage;
            }
            using (var restored = new ChapterTeachingRig(fixture, storage, locator))
            {
                var run = restored.Head.Business.ActiveHistory.CurrentRun;
                CollectionAssert.AreEqual(expected, CandidateBattleReportFingerprint.Encode(new object[] { run.CurrentSnapshot, run.Records }, Data.Codec().Math));
                Assert.IsTrue(run.Baseline.Entry.RequiredFeatures.Contains(ChapterRuleDefinitions.StateFeatureId));
                Assert.AreEqual(2, run.Baseline.Entry.Members.Single().BattleSkills.Single().CooldownTurns);
                Assert.IsTrue(run.Records[0].OrderedFacts.Any(x => x.Kind == CandidateBattleFactKind.SkillCast));
                Assert.IsTrue(run.Records[1].OrderedFacts.Any(x => x.Kind == CandidateBattleFactKind.Cooldown));
            }
        }

        [Test]
        public void CompleteStateValueLayoutPreservesTauntWindowAndExactFractionalWindFacts()
        {
            var binding = ChapterBattleTestData.Bind(ChapterBattleTestData.StateInput());
            var entry = binding.Start.Baseline.Entry;
            var snapshot = binding.Start.Snapshot;
            var initial = CandidateChapterBattleRules.InitialState(entry);
            var target = snapshot.Enemies[0].CombatantKey;
            var caster = snapshot.Members.Single(x => x.Member.ClassKind == CharacterClassKind.Warrior).CombatantKey;
            var window = new ChapterVulnerabilityWindow(target, ChapterBattleTestData.R(5, 4), 4);
            var state = new ChapterBattleState(3, initial.MemberSkills.Select(x =>
                new ChapterMemberSkillState(x.CombatantKey, x.SkillId, 1)),
                new[] { new ChapterTauntState(target, caster, 4) }, new[] { window });
            var fact = new ChapterVulnerabilityFact(ChapterVulnerabilityChange.Created, window);
            var wind = new ChapterWindFact(target, ChapterBattleTestData.R(1, 10), ChapterBattleTestData.R(101),
                ChapterBattleTestData.R(86), ChapterBattleTestData.R(961, 10), ChapterBattleTestData.R(101, 10), 2, 1);
            using (var stream = new MemoryStream())
            {
                var writer = new BusinessFields(stream, false, Data.Codec()) { SchemaVersion = 5 };
                var codec = new CandidateChapterSaveCodec(writer);
                codec.State(state, entry, "State"); codec.Vulnerability(fact, "Window"); codec.Wind(wind, "Wind");
                var bytes = stream.ToArray(); stream.Position = 0;
                var reader = new BusinessFields(stream, true, Data.Codec(), (ulong)bytes.Length) { SchemaVersion = 5 };
                codec = new CandidateChapterSaveCodec(reader);
                var restored = codec.State(null, entry, "State");
                var restoredFact = codec.Vulnerability(null, "Window");
                var restoredWind = codec.Wind(null, "Wind"); reader.End("StateLayout");
                Assert.IsTrue(CandidateBattleReportFingerprint.Equal(state, restored, Data.Codec().Math));
                Assert.IsTrue(CandidateBattleReportFingerprint.Equal(fact, restoredFact, Data.Codec().Math));
                Assert.IsTrue(CandidateBattleReportFingerprint.Equal(wind, restoredWind, Data.Codec().Math));
                Assert.AreEqual(4, (int)restored.Taunts.Single().AppliesToEnemyPhaseOrdinal);
                Assert.AreEqual(1, restoredWind.UsesAfter);
            }
        }

        [Test]
        public void TamperedCooldownCannotBeEncodedAsAValidRetainedOperation()
        {
            var fixture = new ChapterTeachingFixture(initiallyOpenBoss: true);
            using (var rig = new ChapterTeachingRig(fixture))
            {
                rig.CompleteTeaching(); rig.Enter(16); rig.Act(pair: 0);
                var run = rig.Head.Business.ActiveHistory.CurrentRun;
                var original = run.Records.Single(); var after = original.AfterSnapshot;
                var wrong = new ChapterBattleState(after.ChapterState.FaceEnemyPhasesCompleted,
                    after.ChapterState.MemberSkills.Select(x => new ChapterMemberSkillState(x.CombatantKey, x.SkillId, 0)),
                    after.ChapterState.Taunts, after.ChapterState.Vulnerabilities);
                var changed = new BattleSnapshot(after.Baseline, after.SceneRevision, after.EffectiveActionsCompleted,
                    after.EnemyPhasesCompleted, after.CurrentFaceIndex, after.Phase, after.Board, after.Members,
                    after.Enemies, after.Random, after.Contributions, wrong);
                var record = new CandidateBattleOperationRecord(original.Kind, original.OccurredAtUnixMilliseconds,
                    original.BeforeSnapshot, changed, original.Conditions, original.DirectAttack, original.EnemyPhase,
                    original.StageDecision, original.OrderedFacts, original.ContributionSegments);
                var rejected = CandidateBattleOperations.CheckRecord(run.Binding, record, "Restore.Test", Data.Codec().Math);
                Assert.NotNull(rejected); Assert.AreEqual("IncompleteHistory", rejected.RejectionCode);
                Assert.AreEqual(2, after.ChapterState.MemberSkills.Single().CooldownRemaining);
            }
        }
    }
}
