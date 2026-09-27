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
    public class BattleEntryPreparerTests
    {
        [TestCase(1, 1)]
        [TestCase(1, 2)]
        [TestCase(3, 1)]
        [TestCase(3, 2)]
        public void SourceCandidates_Preserve006GeometryMappingsAndExactStats(int stage, int direction)
        {
            var fixture = DemoContentFixture.CaptureSource(stage, 1, (SourceCoordinateConvention)direction);
            var input = Candidate(stage, direction);
            var result = Prepare(input);
            Assert.IsTrue(result.IsAccepted);
            Assert.IsNull(result.RejectionCode);
            Assert.IsNull(result.FieldPath);
            var entry = result.Entry;
            var geometry = fixture.CopyLevel();
            Assert.AreEqual("candidate:L" + stage, entry.Level.LevelId);
            Assert.AreEqual("candidate-r1", entry.Level.LevelVersion);
            Assert.AreEqual(BigInteger.One, entry.Level.RecommendedLevel);
            Assert.AreEqual(1, entry.Level.Faces.Count);
            var face = entry.Level.Faces[0];
            Assert.AreEqual("face:1", face.FaceId);
            Assert.AreEqual(4, face.Width);
            Assert.AreEqual(4, face.Height);
            Assert.AreEqual(2, face.Pairs.Count);
            for (var i = 0; i < 2; i++)
            {
                var pair = face.Pairs[i];
                var binding = fixture.Bindings[i];
                Assert.AreEqual(binding.SourcePair, pair.PairId);
                Assert.AreEqual(binding.ColorId, pair.GeometryColorId);
                Assert.AreEqual(geometry.pairs[i].endpointA, pair.EndpointA);
                Assert.AreEqual(geometry.pairs[i].endpointB, pair.EndpointB);
                Assert.AreEqual(binding.SourcePair, pair.Enemy.EnemyInstanceKey);
                Assert.AreEqual(binding.EnemyAlias, pair.Enemy.EnemyDefinitionId);
                Assert.AreEqual(binding.OriginalSlot, pair.Enemy.OriginalSlot);
                Assert.AreEqual(i, pair.Enemy.StableOrder);
                var heavy = binding.EnemyAlias == "E02";
                AssertStats(pair.Enemy.Stats, heavy ? 20 : 15, 10, heavy ? 20 : 0, 0);
                Assert.AreEqual(heavy ? EnemyBehavior.ChargeHeavy : EnemyBehavior.NormalStrike, pair.Enemy.Behavior);
                Assert.AreEqual(heavy ? 2 : 1, pair.Enemy.IntentCycle.Count);
                if (heavy)
                {
                    Assert.AreEqual(EnemyIntentKind.Charge, pair.Enemy.IntentCycle[0].Kind);
                    Assert.AreEqual(EnemyTargeting.FirstLiving, pair.Enemy.IntentCycle[0].Targeting);
                    Assert.IsNull(pair.Enemy.IntentCycle[0].DamageKind);
                    Assert.IsNull(pair.Enemy.IntentCycle[0].DamageCoefficient);
                }
                var strike = pair.Enemy.IntentCycle[heavy ? 1 : 0];
                Assert.AreEqual(EnemyIntentKind.Strike, strike.Kind);
                Assert.AreEqual(EnemyTargeting.FirstLiving, strike.Targeting);
                Assert.AreEqual(EntryDamageKind.Physical, strike.DamageKind);
                Value(strike.DamageCoefficient, heavy ? 13 : 3, heavy ? 10 : 5);
            }
            var member = entry.Members[0];
            AssertStats(member.Stats, 100, 20, 10, 6);
            Value(member.EntryHp, 100);
            Value(member.Crit.TargetProbability, 1, 5);
            Value(member.Crit.C, 1, 4);
            Value(member.Crit.Multiplier, 3, 2);
            Assert.AreSame(member, entry.ReadyParticipants[0]);
            Assert.AreEqual(EntryCarryMode.Empty, entry.CarryMode);
            Assert.IsEmpty(entry.RequiredFeatures);
            Assert.IsEmpty(member.LearnedSkills);
            Assert.AreEqual(Describe(input.Context), Describe(entry.Context));
            StringAssert.Contains(fixture.SourceSha256, entry.Context.SourceNotes[0]);
            StringAssert.Contains(fixture.CoordinateTransform, entry.Context.SourceNotes[1]);
            StringAssert.Contains("synthetic C=1/4", entry.Context.SourceNotes[3]);
            Assert.AreEqual("isolated:player", entry.PlayerId);
            Assert.AreEqual(input.ChallengeId, entry.ChallengeId);
            Assert.AreEqual(input.AttemptId, entry.AttemptId);
            Assert.AreEqual(input.EntryBaselineId, entry.EntryBaselineId);
        }

        [Test]
        public void IndependentFaces_KeepLocalIdentitiesOrderHolesAndUnsolvableGeometry()
        {
            var input = Candidate();
            var first = input.Level.Faces[0];
            first.FaceId = "z-first";
            first.Width = first.Height = 2;
            first.Pairs[0].EndpointA = new FlowPos(0, 0);
            first.Pairs[0].EndpointB = new FlowPos(1, 1);
            first.Pairs[1].EndpointA = new FlowPos(1, 0);
            first.Pairs[1].EndpointB = new FlowPos(0, 1);
            first.Pairs[0].Enemy.OriginalSlot = 8;
            first.Pairs[0].Enemy.StableOrder = 9;
            first.Pairs.Reverse();
            var second = Candidate(3).Level.Faces[0];
            second.FaceId = "a-second";
            input.Level.Faces.Add(second);
            var entry = Prepare(input).Entry;
            Assert.NotNull(entry);
            CollectionAssert.AreEqual(new[] { "z-first", "a-second" }, entry.Level.Faces.Select(f => f.FaceId));
            CollectionAssert.AreEqual(new[] { "B", "A" }, entry.Level.Faces[0].Pairs.Select(p => p.PairId));
            Assert.AreEqual(8, entry.Level.Faces[0].Pairs[1].Enemy.OriginalSlot);
            Assert.AreEqual(9, entry.Level.Faces[0].Pairs[1].Enemy.StableOrder);
            Assert.AreEqual("A", entry.Level.Faces[1].Pairs[0].Enemy.EnemyInstanceKey);
            Assert.AreNotSame(entry.Level.Faces[0].Pairs[1].Enemy, entry.Level.Faces[1].Pairs[0].Enemy);
        }

        [Test]
        public void ComputedW4AndHugeLevels_AreRetainedWithoutGrowthOrEnemyScaling()
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
            input.Context.SourceNotes.Add("Explicit computed W4 override; synthetic C retained, not calibrated.");
            member.StatsContext = CopyContext(input.Context);
            var first = Prepare(input).Entry;
            Value(first.Members[0].Stats.MaxHp, 124);
            Value(first.Members[0].Stats.Attack, 118, 5);
            Value(first.Members[0].Stats.PhysicalDefense, 29, 2);
            Value(first.Members[0].Stats.MagicDefense, 21, 2);
            Value(first.Members[0].EntryHp, 247, 2);
            Value(first.Members[0].Crit.TargetProbability, 43, 200);
            var huge = (BigInteger.One << 80) + 1;
            input.Level.RecommendedLevel = huge;
            member.Level = huge + 2;
            input.Context.DraftRevision = huge + 4;
            input.Context.SourceNotes.Add("Explicit huge level/revision override retaining computed W4 stats for domain verification.");
            member.StatsContext = CopyContext(input.Context);
            var next = Prepare(input).Entry;
            Assert.AreEqual(huge, next.Level.RecommendedLevel);
            Assert.AreEqual(huge + 2, next.Members[0].Level);
            Assert.AreEqual(huge + 4, next.Context.DraftRevision);
            Assert.AreEqual(Describe(first.Level.Faces), Describe(next.Level.Faces));
            Assert.AreEqual(Describe(first.Members[0].Stats), Describe(next.Members[0].Stats));
        }

        [TestCase("PlayerId")]
        [TestCase("ChallengeId")]
        [TestCase("AttemptId")]
        [TestCase("EntryBaselineId")]
        [TestCase("Context")]
        [TestCase("Context.DraftId")]
        [TestCase("Context.ContentFingerprint")]
        [TestCase("Context.RuleVersion")]
        [TestCase("Context.NumericContractVersion")]
        [TestCase("Context.RandomContractVersion")]
        [TestCase("Context.SourceNotes")]
        [TestCase("Context.SourceNotes[0]")]
        [TestCase("RequiredFeatures")]
        [TestCase("Level")]
        [TestCase("Level.LevelId")]
        [TestCase("Level.LevelVersion")]
        [TestCase("Level.Faces")]
        [TestCase("Level.Faces[0]")]
        [TestCase("Level.Faces[0].FaceId")]
        [TestCase("Level.Faces[0].Pairs")]
        [TestCase("Level.Faces[0].Pairs[0]")]
        [TestCase("Level.Faces[0].Pairs[0].PairId")]
        [TestCase("Level.Faces[0].Pairs[0].Enemy")]
        [TestCase("Level.Faces[0].Pairs[0].Enemy.EnemyInstanceKey")]
        [TestCase("Level.Faces[0].Pairs[0].Enemy.EnemyDefinitionId")]
        [TestCase("Level.Faces[0].Pairs[0].Enemy.Stats")]
        [TestCase("Level.Faces[0].Pairs[0].Enemy.IntentCycle")]
        [TestCase("Level.Faces[0].Pairs[0].Enemy.IntentCycle[0]")]
        [TestCase("Level.Faces[0].Pairs[0].Enemy.IntentCycle[0].DamageKind")]
        [TestCase("Level.Faces[0].Pairs[0].Enemy.IntentCycle[0].DamageCoefficient")]
        [TestCase("Members")]
        [TestCase("Members[0]")]
        [TestCase("Members[0].CharacterId")]
        [TestCase("Members[0].ClassId")]
        [TestCase("Members[0].StatsContext")]
        [TestCase("Members[0].StatsContext.SourceNotes")]
        [TestCase("Members[0].Stats")]
        [TestCase("Members[0].EntryHp")]
        [TestCase("Members[0].LearnedSkills")]
        [TestCase("Members[0].Crit")]
        [TestCase("Members[0].Crit.PassiveDefinitionId")]
        [TestCase("Members[0].Crit.TargetProbability")]
        [TestCase("Members[0].Crit.C")]
        [TestCase("Members[0].Crit.Multiplier")]
        public void MissingData_IsRejectedWithExactPath(string path)
        {
            var input = Candidate();
            Set(input, path, null);
            Reject(input, MissingField, path);
        }

        [TestCase("Level.Faces[0].Pairs[0].GeometryColorId")]
        [TestCase("Level.Faces[0].Pairs[0].EndpointA")]
        [TestCase("Level.Faces[0].Pairs[0].EndpointB")]
        [TestCase("Level.Faces[0].Pairs[0].Enemy.OriginalSlot")]
        [TestCase("Level.Faces[0].Pairs[0].Enemy.StableOrder")]
        [TestCase("Level.Faces[0].Pairs[0].Enemy.IntentCycle[0].DamageKind")]
        [TestCase("Level.Faces[0].Pairs[0].Enemy.IntentCycle[0].DamageCoefficient")]
        [TestCase("Members[0].OriginalSlot")]
        [TestCase("Members[0].IsReady")]
        public void OmittedDefaultableFields_AreNotSilentlyZeroFalseOrNull(string path)
        {
            var input = Candidate(3);
            var split = path.LastIndexOf('.');
            var parentPath = path.Substring(0, split);
            var field = path.Substring(split + 1);
            var original = Get(input, parentPath);
            var omitted = Activator.CreateInstance(original.GetType());
            foreach (var property in original.GetType().GetProperties())
                if (property.Name != field) property.SetValue(omitted, property.GetValue(original));
            Set(input, parentPath, omitted);
            Reject(input, MissingField, path);
        }

        [TestCase("")]
        [TestCase(" \t\r\n")]
        public void BlankStrings_AreMissing(string value)
        {
            var input = Candidate();
            input.PlayerId = value;
            Reject(input, MissingField, "PlayerId");
        }

        [Test]
        public void ValidStrings_AreNotTrimmedCaseFoldedOrParsedAsIds()
        {
            var input = Candidate();
            input.PlayerId = "  Mixed Case ID  ";
            input.Context.ContentFingerprint = " Provided Opaque Fingerprint ";
            input.Members[0].StatsContext = CopyContext(input.Context);
            input.Level.Faces[0].Pairs[1].PairId = "a";
            input.Level.Faces[0].Pairs[1].Enemy.EnemyInstanceKey = "a";
            var entry = Prepare(input).Entry;
            Assert.AreEqual(input.PlayerId, entry.PlayerId);
            Assert.AreEqual(input.Context.ContentFingerprint, entry.Context.ContentFingerprint);
        }

        [TestCase("Context.SourceNotes")]
        [TestCase("Level.Faces")]
        [TestCase("Level.Faces[0].Pairs")]
        [TestCase("Level.Faces[0].Pairs[0].Enemy.IntentCycle")]
        public void RequiredNonemptyLists_RejectEmpty(string path)
        {
            var input = Candidate();
            ((IList)Get(input, path)).Clear();
            Reject(input, InvalidValue, path);
        }

        [TestCase("PairId")]
        [TestCase("GeometryColorId")]
        [TestCase("Enemy.EnemyInstanceKey")]
        [TestCase("Enemy.OriginalSlot")]
        [TestCase("Enemy.StableOrder")]
        public void DuplicateMappingIdentity_IsRejected(string suffix)
        {
            var input = Candidate();
            Set(input, "Level.Faces[0].Pairs[1]." + suffix, Get(input, "Level.Faces[0].Pairs[0]." + suffix));
            Reject(input, DuplicateIdentity, "Level.Faces[0].Pairs[1]." + suffix);
        }

        [Test]
        public void DuplicateFace_IsRejected()
        {
            var input = Candidate();
            input.Level.Faces.Add(Candidate(3).Level.Faces[0]);
            Reject(input, DuplicateIdentity, "Level.Faces[1].FaceId");
        }

        [TestCase("EndpointA")]
        [TestCase("EndpointB")]
        public void OverlappingEndpoint_IsRejected(string endpoint)
        {
            var input = Candidate();
            Set(input, "Level.Faces[0].Pairs[1]." + endpoint, input.Level.Faces[0].Pairs[0].EndpointB);
            Reject(input, DuplicateIdentity, "Level.Faces[0].Pairs[1]." + endpoint);
        }

        [TestCase(-1, 0)]
        [TestCase(0, -1)]
        [TestCase(4, 0)]
        [TestCase(0, 4)]
        public void OutOfBoundsEndpoint_IsRejected(int x, int y)
        {
            var input = Candidate();
            input.Level.Faces[0].Pairs[0].EndpointA = new FlowPos(x, y);
            Reject(input, InvalidValue, "Level.Faces[0].Pairs[0].EndpointA");
        }

        [Test]
        public void EqualEndpoints_AreInvalid()
        {
            var input = Candidate();
            var pair = input.Level.Faces[0].Pairs[0];
            pair.EndpointB = pair.EndpointA;
            Reject(input, InvalidValue, "Level.Faces[0].Pairs[0].EndpointB");
        }

        [TestCase("Level.Faces[0].Width", 0)]
        [TestCase("Level.Faces[0].Height", -1)]
        [TestCase("Level.Faces[0].Pairs[0].Enemy.OriginalSlot", -1)]
        [TestCase("Level.Faces[0].Pairs[0].Enemy.StableOrder", -1)]
        [TestCase("Level.Faces[0].Pairs[0].Enemy.Stats.AttackRange", 0)]
        [TestCase("Members[0].OriginalSlot", -1)]
        [TestCase("Members[0].OriginalSlot", 3)]
        [TestCase("Members[0].Stats.AttackRange", -1)]
        public void InvalidMachineInteger_IsRejected(string path, int value)
        {
            var input = Candidate();
            Set(input, path, value);
            Reject(input, InvalidValue, path);
        }

        [TestCase("Context.DraftRevision")]
        [TestCase("Level.RecommendedLevel")]
        [TestCase("Members[0].Level")]
        [TestCase("Members[0].StatsContext.DraftRevision")]
        public void InvalidBigInteger_IsRejected(string path)
        {
            foreach (var value in new[] { BigInteger.Zero, -BigInteger.One })
            {
                var input = Candidate();
                Set(input, path, value);
                Reject(input, InvalidValue, path);
            }
        }

        [TestCase("Members[0].Stats.MaxHp", 0)]
        [TestCase("Members[0].Stats.Attack", -1)]
        [TestCase("Members[0].Stats.PhysicalDefense", -1)]
        [TestCase("Members[0].Stats.MagicDefense", -1)]
        [TestCase("Members[0].Stats.Evasion", -1)]
        [TestCase("Members[0].Stats.Evasion", 2)]
        [TestCase("Members[0].EntryHp", 0)]
        [TestCase("Members[0].EntryHp", 101)]
        [TestCase("Members[0].Crit.TargetProbability", 0)]
        [TestCase("Members[0].Crit.TargetProbability", 2)]
        [TestCase("Members[0].Crit.C", -1)]
        [TestCase("Members[0].Crit.C", 0)]
        [TestCase("Members[0].Crit.C", 2)]
        [TestCase("Members[0].Crit.Multiplier", 0)]
        [TestCase("Level.Faces[0].Pairs[0].Enemy.IntentCycle[0].DamageCoefficient", 0)]
        [TestCase("Level.Faces[0].Pairs[0].Enemy.Stats.MaxHp", -1)]
        public void InvalidRationalDomain_IsRejected(string path, int value)
        {
            var input = Candidate();
            Set(input, path, R(value));
            Reject(input, InvalidValue, path);
        }

        [Test]
        public void SupportedDomainEdges_AreAcceptedWithoutCalibration()
        {
            var input = Candidate();
            input.Members[0].Stats.Attack = R(0);
            input.Members[0].Stats.PhysicalDefense = R(0);
            input.Members[0].Stats.MagicDefense = R(0);
            input.Members[0].Crit.TargetProbability = R(1);
            input.Members[0].Crit.C = R(1);
            input.Members[0].Crit.Multiplier = R(1, 9);
            input.Members[0].EntryHp = R(1, 97);
            input.Context.SourceNotes.Add("Synthetic edge overrides: zero attack/defenses, p=C=1, multiplier=1/9, EntryHp=1/97.");
            input.Members[0].StatsContext = CopyContext(input.Context);
            Assert.IsTrue(Prepare(input).IsAccepted);
        }

        [TestCase("CarryMode", 2)]
        [TestCase("CarryMode", 99)]
        [TestCase("Members[0].ClassKind", 99)]
        [TestCase("Members[0].StatsOrigin", 99)]
        [TestCase("Level.Faces[0].Pairs[0].Enemy.Behavior", 99)]
        [TestCase("Level.Faces[0].Pairs[0].Enemy.IntentCycle[0].Kind", 99)]
        [TestCase("Level.Faces[0].Pairs[0].Enemy.IntentCycle[0].Targeting", 99)]
        [TestCase("Level.Faces[0].Pairs[0].Enemy.IntentCycle[0].DamageKind", 99)]
        public void UnknownOrUnsupportedEnum_IsExplicitlyRejected(string path, int value)
        {
            var input = Candidate();
            Set(input, path, Enum.ToObject(Get(input, path).GetType(), value));
            Reject(input, UnsupportedBinding, path);
        }

        [TestCase("CarryMode")]
        [TestCase("Members[0].ClassKind")]
        [TestCase("Members[0].StatsOrigin")]
        [TestCase("Level.Faces[0].Pairs[0].Enemy.Behavior")]
        [TestCase("Level.Faces[0].Pairs[0].Enemy.IntentCycle[0].Kind")]
        [TestCase("Level.Faces[0].Pairs[0].Enemy.IntentCycle[0].Targeting")]
        public void UnspecifiedEnum_IsMissing(string path)
        {
            var input = Candidate();
            Set(input, path, Enum.ToObject(Get(input, path).GetType(), 0));
            Reject(input, MissingField, path);
        }

        [TestCase("Members[0].Stats.Evasion")]
        [TestCase("Level.Faces[0].Pairs[0].Enemy.Stats.Evasion")]
        public void NonzeroEvasion_IsUnsupported(string path)
        {
            var input = Candidate();
            Set(input, path, R(1, 100));
            Reject(input, UnsupportedBinding, path);
        }

        [TestCase("RequiredFeatures")]
        [TestCase("Members[0].LearnedSkills")]
        public void UnknownFeatureOrLearnedActive_IsUnsupported(string path)
        {
            var input = Candidate();
            ((IList)Get(input, path)).Add("skill:warrior-taunt");
            Reject(input, UnsupportedBinding, path + "[0]");
        }

        [TestCase("normalCharge", ".IntentCycle[0].Kind")]
        [TestCase("normalExtra", ".IntentCycle")]
        [TestCase("heavyShort", ".IntentCycle")]
        [TestCase("heavyReversed", ".IntentCycle[0].Kind")]
        [TestCase("chargeDamageKind", ".IntentCycle[0].DamageKind")]
        [TestCase("chargeZeroCoefficient", ".IntentCycle[0].DamageCoefficient")]
        public void MalformedIntentShape_IsRejected(string kind, string field)
        {
            var input = Candidate(kind.StartsWith("normal", StringComparison.Ordinal) ? 1 : 3);
            var enemy = input.Level.Faces[0].Pairs[0].Enemy;
            if (kind == "normalCharge") enemy.IntentCycle[0].Kind = EnemyIntentKind.Charge;
            if (kind == "normalExtra") enemy.IntentCycle.Add(Strike(3, 5));
            if (kind == "heavyShort") enemy.IntentCycle.RemoveAt(1);
            if (kind == "heavyReversed") enemy.IntentCycle.Reverse();
            if (kind == "chargeDamageKind") enemy.IntentCycle[0].DamageKind = EntryDamageKind.Physical;
            if (kind == "chargeZeroCoefficient") enemy.IntentCycle[0].DamageCoefficient = R(0);
            Reject(input, InvalidValue, "Level.Faces[0].Pairs[0].Enemy" + field);
        }

        [TestCase("DraftId")]
        [TestCase("DraftRevision")]
        [TestCase("ContentFingerprint")]
        [TestCase("RuleVersion")]
        [TestCase("NumericContractVersion")]
        [TestCase("RandomContractVersion")]
        [TestCase("SourceNotes[0]")]
        [TestCase("SourceNotes")]
        public void SingleContextDifference_IsInconsistent(string field)
        {
            var input = Candidate();
            var path = "Members[0].StatsContext." + field;
            if (field == "DraftRevision") Set(input, path, new BigInteger(2));
            else if (field == "SourceNotes") input.Members[0].StatsContext.SourceNotes.RemoveAt(0);
            else Set(input, path, Get(input, path) + " ");
            Reject(input, InconsistentBinding, path);
        }

        [Test]
        public void SourceNoteOrder_IsPartOfBinding()
        {
            var input = Candidate();
            input.Members[0].StatsContext.SourceNotes.Reverse();
            Reject(input, InconsistentBinding, "Members[0].StatsContext.SourceNotes[0]");
        }

        [Test]
        public void ContextCaseDifference_IsInconsistent()
        {
            var input = Candidate();
            input.Members[0].StatsContext.ContentFingerprint = input.Context.ContentFingerprint.ToUpperInvariant();
            Reject(input, InconsistentBinding, "Members[0].StatsContext.ContentFingerprint");
        }

        [TestCase("CharacterId")]
        [TestCase("ClassId")]
        [TestCase("OriginalSlot")]
        public void DuplicateMemberIdentity_IsRejected(string field)
        {
            var input = Candidate();
            var second = Candidate().Members[0];
            second.CharacterId = "isolated:other";
            second.ClassId = "class:other";
            second.OriginalSlot = 1;
            input.Members.Add(second);
            Set(input, "Members[1]." + field, Get(input, "Members[0]." + field));
            Reject(input, DuplicateIdentity, "Members[1]." + field);
        }

        [Test]
        public void MultipleWarriorsWithoutIdentityDuplicates_AreUnsupported()
        {
            var input = Candidate();
            var second = Candidate().Members[0];
            second.CharacterId = "isolated:other";
            second.ClassId = "class:other";
            second.OriginalSlot = 1;
            input.Members.Add(second);
            Reject(input, UnsupportedBinding, "Members[1]");
        }

        [TestCase(true)]
        [TestCase(false)]
        public void NoReadyMember_IsExplicit(bool empty)
        {
            var input = Candidate();
            if (empty) input.Members.Clear();
            else input.Members[0].IsReady = false;
            Reject(input, NoReadyMember, "Members");
        }

        [Test]
        public void SlotTwo_IsRetainedAndNoMissingSlotIsFilled()
        {
            var input = Candidate();
            input.Members[0].OriginalSlot = 2;
            var entry = Prepare(input).Entry;
            Assert.AreEqual(1, entry.Members.Count);
            Assert.AreEqual(1, entry.ReadyParticipants.Count);
            Assert.AreEqual(2, entry.Members[0].OriginalSlot);
            Assert.AreEqual(2, entry.ReadyParticipants[0].OriginalSlot);
        }

        [Test]
        public void ValidationOrder_IsRootThenContextThenFacesThenMembersThenReadiness()
        {
            var input = Candidate();
            input.PlayerId = null;
            input.Context.RuleVersion = null;
            input.Level.Faces[0].Width = 0;
            input.Members[0].EntryHp = null;
            input.Members[0].IsReady = false;
            Reject(input, MissingField, "PlayerId");
            input.PlayerId = "isolated:player";
            Reject(input, MissingField, "Context.RuleVersion");
            input.Context.RuleVersion = input.Members[0].StatsContext.RuleVersion;
            Reject(input, InvalidValue, "Level.Faces[0].Width");
            input.Level.Faces[0].Width = 4;
            Reject(input, MissingField, "Members[0].EntryHp");
            input.Members[0].EntryHp = R(100);
            Reject(input, NoReadyMember, "Members");
        }

        [TestCase("Context.DraftRevision")]
        [TestCase("Level.RecommendedLevel")]
        [TestCase("Members[0].Level")]
        [TestCase("Members[0].StatsContext.DraftRevision")]
        public void EveryBigInteger_IsRecheckedAgainstCallBudget(string path)
        {
            var input = Candidate();
            Set(input, path, (BigInteger.One << 80) + 1);
            var before = Describe(input);
            var error = Assert.Throws<ExactMathLimitException>(() => Prepare(input, new ExactMathBudget(64)));
            Assert.AreEqual("IntegerBits", error.ReasonCode);
            Assert.AreEqual(before, Describe(input));
        }

        private static IEnumerable<string> RationalPaths()
        {
            foreach (var prefix in new[] { "Members[0]", "Level.Faces[0].Pairs[0].Enemy", "Level.Faces[0].Pairs[1].Enemy" })
                foreach (var field in new[] { "MaxHp", "Attack", "PhysicalDefense", "MagicDefense", "Evasion" })
                    yield return prefix + ".Stats." + field;
            yield return "Members[0].EntryHp";
            yield return "Members[0].Crit.TargetProbability";
            yield return "Members[0].Crit.C";
            yield return "Members[0].Crit.Multiplier";
            yield return "Level.Faces[0].Pairs[0].Enemy.IntentCycle[0].DamageCoefficient";
            yield return "Level.Faces[0].Pairs[1].Enemy.IntentCycle[0].DamageCoefficient";
        }

        [TestCaseSource(nameof(RationalPaths))]
        public void EveryRational_RequiresValueAndRechecksNumeratorAndDenominator(string path)
        {
            var missing = Candidate();
            Set(missing, path, null);
            Reject(missing, MissingField, path);
            foreach (var denominator in new[] { false, true })
            {
                var input = Candidate();
                var huge = (BigInteger.One << 80) + 1;
                Set(input, path, ExactRational.Create(denominator ? BigInteger.One : huge,
                    denominator ? huge : BigInteger.One, new ExactMathBudget()));
                var before = Describe(input);
                var error = Assert.Throws<ExactMathLimitException>(() => Prepare(input, new ExactMathBudget(64)));
                Assert.AreEqual("IntegerBits", error.ReasonCode);
                Assert.AreEqual(before, Describe(input));
            }
        }

        [Test]
        public void SharedStepBudget_FailsAtomicallyAndFreshLargerBudgetRetriesSameInput()
        {
            var input = Candidate(3);
            var other = Candidate().Level.Faces[0];
            other.FaceId = "face:2";
            input.Level.Faces.Add(other);
            var before = Describe(input);
            var full = new ExactMathBudget();
            var first = Prepare(input, full).Entry;
            var used = checked((int)full.PrimitiveStepsUsed);
            Assert.Greater(used, 1);
            var tight = new ExactMathBudget(maxPrimitiveSteps: used - 1);
            BattleEntryPreparationResult partial = null;
            var error = Assert.Throws<ExactMathLimitException>(() => partial = Prepare(input, tight));
            Assert.AreEqual("PrimitiveSteps", error.ReasonCode);
            Assert.IsNull(partial);
            Assert.AreEqual(before, Describe(input));
            var retry = Prepare(input, new ExactMathBudget(maxPrimitiveSteps: used)).Entry;
            Assert.AreEqual(Describe(first), Describe(retry));
            Assert.Throws<ExactMathLimitException>(() => Prepare(input, new ExactMathBudget(maxPrimitiveSteps: 0)));
        }

        [Test]
        public void NullRootArguments_ThrowBeforeValidation()
        {
            Assert.AreEqual("input", Assert.Throws<ArgumentNullException>(() => Prepare(null)).ParamName);
            Assert.AreEqual("budget", Assert.Throws<ArgumentNullException>(() =>
                new BattleEntryPreparer().PrepareCandidate(Candidate(), null)).ParamName);
            Assert.AreEqual("input", Assert.Throws<ArgumentNullException>(() =>
                new BattleEntryPreparer().PrepareCandidate(null, null)).ParamName);
        }

        [Test]
        public void PreparedResults_AreDeeplyIsolatedAndAllCollectionsReadOnly()
        {
            var input = Candidate(3);
            var first = Prepare(input).Entry;
            var second = Prepare(input).Entry;
            var before = Describe(first);
            Assert.AreNotSame(first.Level.Faces[0], second.Level.Faces[0]);
            Assert.AreNotSame(first.Members[0].Stats, second.Members[0].Stats);
            AssertReadOnlyGraph(first);
            input.PlayerId = "changed";
            input.Context.DraftRevision = 99;
            input.Context.SourceNotes.Clear();
            input.Members[0].StatsContext.SourceNotes[0] = "changed";
            input.Members[0].Stats.MaxHp = R(999);
            input.Members[0].Crit.C = R(1);
            input.Members[0].LearnedSkills.Add("changed");
            input.Members.Clear();
            var face = input.Level.Faces[0];
            face.Width = 99;
            face.Pairs[0].EndpointA = new FlowPos(99, 99);
            face.Pairs[0].Enemy.Stats.Attack = R(999);
            face.Pairs[0].Enemy.IntentCycle[1].DamageCoefficient = R(999);
            face.Pairs[0].Enemy.IntentCycle.Clear();
            face.Pairs.Clear();
            input.Level.Faces.Clear();
            input.RequiredFeatures.Add("changed");
            var cell = first.Level.Faces[0].Pairs[0].EndpointA;
            cell.x = 999;
            Assert.AreEqual(before, Describe(first));
            Assert.AreEqual(before, Describe(second));
            Assert.AreEqual(before, Describe(Prepare(Candidate(3)).Entry));
        }

        private static BattleEntryInput Candidate(int stage = 1, int direction = 1)
        {
            var fixture = DemoContentFixture.CaptureSource(stage, 1, (SourceCoordinateConvention)direction);
            var geometry = fixture.CopyLevel();
            var context = new CandidateContext
            {
                DraftId = "isolated:" + fixture.FixtureKey, DraftRevision = fixture.Revision,
                ContentFingerprint = "candidate:" + fixture.FixtureKey + ":direction" + direction + ":W1:C1/4",
                RuleVersion = "candidate:E01-E02", NumericContractVersion = "candidate:Exact003",
                RandomContractVersion = "candidate:RC01-unapproved",
                SourceNotes = new List<string>
                {
                    fixture.SourcePath + ":" + fixture.SourceLocator + ":SHA256=" + fixture.SourceSha256,
                    "Unconfirmed source orientation candidate: " + fixture.CoordinateTransform,
                    "content-validation sections 2/3: isolated computed W1; E01/E02 final stats; A then B; empty carry, no active.",
                    "synthetic C=1/4, p=1/5, multiplier=3/2; not calibrated W1 C or formal random replay."
                }
            };
            var face = new FaceInput { FaceId = "face:1", Width = geometry.width, Height = geometry.height, Pairs = new List<PairInput>() };
            for (var i = 0; i < fixture.Bindings.Count; i++)
            {
                var binding = fixture.Bindings[i];
                var pair = geometry.pairs.Single(p => p.colorId == binding.ColorId);
                var heavy = binding.EnemyAlias == "E02";
                var cycle = new List<EnemyIntentInput>();
                if (heavy) cycle.Add(new EnemyIntentInput
                {
                    Kind = EnemyIntentKind.Charge, Targeting = EnemyTargeting.FirstLiving,
                    DamageKind = null, DamageCoefficient = null
                });
                cycle.Add(heavy ? Strike(13, 10) : Strike(3, 5));
                face.Pairs.Add(new PairInput
                {
                    PairId = binding.SourcePair, GeometryColorId = binding.ColorId,
                    EndpointA = pair.endpointA, EndpointB = pair.endpointB,
                    Enemy = new EnemyInput
                    {
                        EnemyInstanceKey = binding.SourcePair, EnemyDefinitionId = binding.EnemyAlias,
                        OriginalSlot = binding.OriginalSlot, StableOrder = i,
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
                    CharacterId = "isolated:W", ClassId = "class:warrior", ClassKind = CharacterClassKind.Warrior,
                    OriginalSlot = 0, Level = 1, IsReady = true, StatsOrigin = BaseStatsOrigin.ComputedBaseStats,
                    StatsContext = CopyContext(context), Stats = Stats(100, 20, 10, 6), EntryHp = R(100),
                    LearnedSkills = new List<string>(), Crit = new WarriorCritInput
                    { PassiveDefinitionId = "candidate:warrior-crit", TargetProbability = R(1, 5), C = R(1, 4), Multiplier = R(3, 2) }
                } }
            };
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

        private static EnemyIntentInput Strike(int numerator, int denominator)
        {
            return new EnemyIntentInput { Kind = EnemyIntentKind.Strike, Targeting = EnemyTargeting.FirstLiving, DamageKind = EntryDamageKind.Physical, DamageCoefficient = R(numerator, denominator) };
        }

        private static ExactRational R(int n, int d = 1) { return ExactRational.Create(n, d, new ExactMathBudget()); }
        private static void Value(ExactRational value, int n, int d = 1) { Assert.AreEqual(new BigInteger(n), value.Numerator); Assert.AreEqual(new BigInteger(d), value.Denominator); }
        private static BattleEntryPreparationResult Prepare(BattleEntryInput input, ExactMathBudget budget = null)
        { return new BattleEntryPreparer().PrepareCandidate(input, budget ?? new ExactMathBudget()); }

        private static void AssertStats(PreparedStats stats, int hp, int attack, int physical, int magic)
        {
            Value(stats.MaxHp, hp); Value(stats.Attack, attack); Value(stats.PhysicalDefense, physical);
            Value(stats.MagicDefense, magic); Value(stats.Evasion, 0); Assert.AreEqual(1, stats.AttackRange);
        }

        private static void Reject(BattleEntryInput input, BattleEntryRejectionCode code, string path)
        {
            var before = Describe(input);
            var result = Prepare(input);
            Assert.IsFalse(result.IsAccepted);
            Assert.IsNull(result.Entry);
            Assert.AreEqual(code, result.RejectionCode);
            Assert.AreEqual(path, result.FieldPath);
            Assert.AreEqual(before, Describe(input));
        }

        private static string[] Parts(string path) { return path.Replace("[", ".").Replace("]", "").Split('.'); }
        private static object Get(object root, string path)
        {
            foreach (var part in Parts(path)) root = root is IList list ? list[int.Parse(part)] : root.GetType().GetProperty(part).GetValue(root);
            return root;
        }

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
