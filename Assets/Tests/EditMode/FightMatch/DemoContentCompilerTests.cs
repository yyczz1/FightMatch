using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using FightMatch.Content;
using FlowPuzzle.Core;
using NUnit.Framework;
using static FightMatch.Core.Tests.DemoContentTestData;

namespace FightMatch.Core.Tests
{
    public class DemoContentCompilerTests
    {
        [TestCase(1, DemoCoordinateCandidate.AssumedBottomLeft)] [TestCase(1, DemoCoordinateCandidate.AssumedTopLeft)]
        [TestCase(3, DemoCoordinateCandidate.AssumedBottomLeft)] [TestCase(3, DemoCoordinateCandidate.AssumedTopLeft)]
        public void CompleteSixOfSixteenGeometryAndClosedEntry(int stage, DemoCoordinateCandidate coordinates)
        {
            var c = Prepare(stage, coordinates); Assert.IsTrue(c.ReviewRequired); Assert.IsFalse(c.CommitEligible);
            Assert.AreEqual(2, c.Routes.Count); Assert.AreEqual(6, c.Routes.SelectMany(r => r.Cells).Distinct().Count());
            Assert.AreEqual(1, c.Entry.Level.Faces.Count); Assert.AreEqual(1, c.Entry.Members.Count); Assert.AreEqual(31, c.Parameters.Count);
            Same(c.Entry.Members[0].Stats.MaxHp, R(100)); Same(c.Entry.Members[0].Stats.Attack, R(20)); Same(c.Entry.Members[0].Stats.MagicDefense, R(6));
            Assert.AreEqual(EntryCarryMode.Empty, c.Entry.CarryMode); Assert.IsEmpty(c.Entry.Members[0].LearnedSkills);
            Assert.IsEmpty(c.Entry.RequiredFeatures); Assert.IsEmpty(c.Character.RecoveryPeriods); Assert.AreEqual(BigInteger.Zero, c.Character.Experience);
            foreach (var route in c.Routes)
                for (var i = 0; i < route.Cells.Count; i++) Assert.AreEqual(new FlowPos(route.SourceCells[i].x - 1,
                    coordinates == DemoCoordinateCandidate.AssumedBottomLeft ? route.SourceCells[i].y - 1 : 4 - route.SourceCells[i].y), route.Cells[i]);
        }
        [Test]
        public void BothCoordinatesAndLevelsHaveIndependentFingerprintsAndSourceLedger()
        {
            var candidates = new[] { Prepare(1), Prepare(1, DemoCoordinateCandidate.AssumedTopLeft), Prepare(3), Prepare(3, DemoCoordinateCandidate.AssumedTopLeft) };
            Assert.AreEqual(4, candidates.Select(c => c.Fingerprint).Distinct().Count());
            var sourcePaths = new[] { Config, Boards, "docs/game-design/balance/chapter_model.py", "docs/game-design/balance/calibrate.py",
                "docs/game-design/balance/README.md", "docs/game-design/balance/2026-09-19-calibration-inputs.md", "docs/game-design/balance/results/stages.csv",
                "docs/game-design/balance/results/enemies.csv", "docs/game-design/balance/results/prd.csv", "docs/game-design/balance/results/progression.csv",
                "docs/game-design/balance/results/manifest.json" };
            WriteEvidence("source-ledger.json", new { Status = "candidate-only; origin and identity mappings ReviewRequired",
                Sources = sourcePaths.Select(p => new { Path = p, Sha256 = Sha(p) }).ToArray(),
                Candidates = candidates.Select(c => new { c.Fingerprint, c.Coordinates, c.Sources, c.Routes, c.Entry, c.ComputedStats, c.Reward }).ToArray(),
                Unresolved = new[] { "source origin bottom-left vs top-left", "all proposed level/face/pair/enemy/behavior/class/passive/item/reward identities and versions",
                    "31 rational C values and epsilon 1/10^9", "first-clear zero rewards and no unlock effects", "new profile W Lv1 xp0; empty inventory; only L1 open" },
                ExplicitEmpty = new[] { "slots 1,2", "recovery", "active skills", "carry C/U", "cooldowns", "shield", "statuses", "permissions" },
                OldProgression = "L1 XP25/HP95 and L3 W1+53 / XP31/HP90 are historical, not new progression evidence" });
        }
        [TestCase(0, "MissingSourceCoordinateConvention")] [TestCase(1, "InvalidGeometry")] [TestCase(2, "MissingField")]
        [TestCase(3, "UnsupportedBinding")] [TestCase(4, "MissingField")] [TestCase(5, "InconsistentBinding")]
        [TestCase(6, "InconsistentBinding")] [TestCase(7, "MissingField")] [TestCase(8, "ToleranceExceeded")]
        [TestCase(9, "UnsupportedBinding")] [TestCase(10, "MissingField")] [TestCase(11, "InvalidValue")]
        public void RejectsMissingContradictoryAndUnsupportedBindings(int mutation, string code)
        {
            var i = MakeInput();
            switch (mutation)
            {
                case 0: i.Coordinates = DemoCoordinateCandidate.Unspecified; break;
                case 1: i.Level.Faces[0].Pairs[0].EndpointB = new FlowPos(3, 2); break;
                case 2: i.SourceRoutes.RemoveAt(1); break;
                case 3: i.LearnedSkills.Add("taunt"); break;
                case 4: i.Reward.Materials = null; break;
                case 5: i.Sources[1].Sha256 = new string('0', 64); break;
                case 6: i.Reward.LevelId = "other"; break;
                case 7: i.Parameters[0].C = null; break;
                case 8: i.Parameters[0].Epsilon = R(0); break;
                case 9: i.Growth.Context = new CandidateContext { ContentFingerprint = "arbitrary" }; break;
                case 10: i.RequiredFeatures = null; break;
                case 11: i.SourceRoutes[0].Cells[0] = new FlowPos(0, 1); break;
            }
            var result = DemoContentCompiler.PrepareCandidate(i, Job(), Math());
            Assert.AreEqual(code, result.RejectionCode, result.FieldPath); Assert.IsNull(result.Candidate); Assert.IsNotEmpty(result.FieldPath);
        }
        [Test]
        public void MissingExistingDtoPresenceFlagsAreStillRejectedBy007A()
        {
            var i = MakeInput(); var original = i.Level.Faces[0].Pairs[0];
            i.Level.Faces[0].Pairs[0] = new PairInput { PairId = original.PairId, EndpointA = original.EndpointA, EndpointB = original.EndpointB, Enemy = original.Enemy };
            var r = DemoContentCompiler.PrepareCandidate(i, Job(), Math()); Assert.AreEqual("MissingField", r.RejectionCode); StringAssert.EndsWith("GeometryColorId", r.FieldPath);
        }
        [Test]
        public void DeepCopiesEveryMutableSourceAndRoute()
        {
            var i = MakeInput(); var r = DemoContentCompiler.PrepareCandidate(i, Job(), Math()); Assert.IsTrue(r.IsAccepted); var c = r.Candidate;
            var original = Json(new { c.Entry, c.Sources, c.Routes, c.Parameters, c.Reward, c.Character });
            i.Sources[1].Bytes[0] = 0; i.Sources[0].Path = "mutated"; i.SourceNotes.Clear(); i.SourceRoutes[0].Cells.Clear();
            i.Level.Faces[0].Pairs[0].Enemy.IntentCycle.Clear(); i.Level.Faces.Clear(); i.Growth.BaseStats.Attack = R(999);
            i.Parameters[0].C = R(1); i.Reward.Materials[0].Amount = 999; i.LearnedSkills.Add("taunt");
            Assert.AreEqual(original, Json(new { c.Entry, c.Sources, c.Routes, c.Parameters, c.Reward, c.Character }));
            Assert.Throws<NotSupportedException>(() => ((IList<FlowPos>)c.Routes[0].Cells).Clear());
        }
        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)] [TestCase(5)] [TestCase(6)]
        public void FingerprintCoversSemanticValuesVersionsAndOrderedLists(int mutation)
        {
            var job = Job(); var a = MakeInput(); var b = MakeInput();
            switch (mutation)
            {
                case 0: b.RuleVersion += ":new"; break;
                case 1: b.SourceNotes.Add("additional provenance"); break;
                case 2: b.Reward.Materials.Reverse(); break;
                case 3: b.SourceRoutes.Reverse(); break;
                case 4: b.Parameters[0].Epsilon = R(2, 1000000000); break;
                case 5: b.Growth.RecoveryDurationMilliseconds = R(180001); break;
                case 6: b.Sources[0].Locator += ":different-field"; break;
            }
            var before = DemoContentCompiler.PrepareCandidate(a, job, Math()); var after = DemoContentCompiler.PrepareCandidate(b, job, Math());
            Assert.IsTrue(before.IsAccepted); Assert.IsTrue(after.IsAccepted, after.RejectionCode); Assert.AreNotEqual(before.Candidate.Fingerprint, after.Candidate.Fingerprint);
        }
        [Test]
        public void SourceLedgerOrderIsNotSemanticGameOrderAndBytesEqualDigest()
        {
            var job = Job(); var a = MakeInput(); var b = MakeInput(); b.Sources.Reverse();
            var source = b.Sources.Single(x => x.Bytes != null); source.Sha256 = Sha(source.Path); source.Bytes = null;
            Assert.AreEqual(DemoContentCompiler.PrepareCandidate(a, job, Math()).Candidate.Fingerprint,
                DemoContentCompiler.PrepareCandidate(b, job, Math()).Candidate.Fingerprint);
        }
        [Test]
        public void SameValueRevisionEightNineTenAndCancellationNeverAdoptOldResult()
        {
            var draft = new DemoContentDraft("revision-witness"); for (var n = 1; n < 8; n++) draft.Revise();
            var oldJob = draft.BeginJob(); var input = MakeInput(); var old = DemoContentCompiler.PrepareCandidate(input, oldJob, Math()); Assert.IsTrue(old.IsAccepted);
            draft.Revise(); input.RuleVersion += "changed"; draft.Revise(); input.RuleVersion = "candidate-demo-r1";
            Assert.AreEqual(new BigInteger(10), draft.Revision); Assert.IsFalse(old.Candidate.IsCurrent);
            Assert.AreEqual("StaleContext", DemoContentCompiler.PrepareCandidate(input, oldJob, Math()).RejectionCode);
            var current = DemoContentCompiler.PrepareCandidate(input, draft.BeginJob(), Math()); Assert.IsTrue(current.IsAccepted);
            Assert.AreNotEqual(old.Candidate.Fingerprint, current.Candidate.Fingerprint);
            using (var cancel = new CancellationTokenSource())
            { var job = draft.BeginJob(cancel.Token); cancel.Cancel(); Assert.AreEqual("Cancelled", DemoContentCompiler.PrepareCandidate(input, job, Math()).RejectionCode); }
        }
        [TestCase(4, 124, 21, 2)] [TestCase(32, 348, 105, 2)]
        public void GrowthComputedOnceAndBeyondThirtyOneUsesSameCap(int level, int hp, int mdn, int mdd)
        {
            var i = MakeInput(); i.CharacterLevel = level; var r = DemoContentCompiler.PrepareCandidate(i, Job(), Math()); Assert.IsTrue(r.IsAccepted, r.FieldPath);
            Same(r.Candidate.Entry.Members[0].Stats.MaxHp, R(hp)); Same(r.Candidate.Entry.Members[0].Stats.MagicDefense, R(mdn, mdd));
            Same(r.Candidate.Entry.Members[0].Crit.C, Candidates()[level > 31 ? 30 : level - 1].Proposed.C);
        }
        [Test]
        public void NullAndTinyBudgetCannotCreateCandidate()
        {
            Assert.AreEqual("MissingField", DemoContentCompiler.PrepareCandidate(null, Job(), Math()).RejectionCode);
            Assert.AreEqual("BudgetExceeded", DemoContentCompiler.PrepareCandidate(MakeInput(), Job(), new ExactMathBudget(maxPrimitiveSteps: 1)).RejectionCode);
            Assert.IsFalse(typeof(DemoContentCompiler).GetMethods().Any(m => m.Name == "Publish" || m.Name == "ResolveExact"));
            var references = typeof(DemoContentCompiler).Assembly.GetReferencedAssemblies().Select(a => a.Name).ToArray();
            Assert.IsFalse(references.Any(n => n.StartsWith("Unity") || n == "QFramework" || n == "FightMatch.Application"));
            Assert.IsTrue(references.Contains("FightMatch.Platform"));
        }

        [TestCase(0, TestName = "F1-D800")]
        [TestCase(1, TestName = "F1-D801")]
        public void RejectsUnpairedSurrogatePlayerIdentity(int selected)
        {
            var job = Job();
            var first = PrepareLoggedIdentity("P\uD800", job);
            var second = PrepareLoggedIdentity("P\uD801", job);
            Assert.IsTrue(job.IsCurrent);
            var result = selected == 0 ? first : second;
            Assert.AreEqual("InvalidValue", result.RejectionCode, "Unpaired surrogate must be rejected by the public result envelope.");
            Assert.IsFalse(result.IsAccepted);
            Assert.IsNull(result.Candidate);
            Assert.IsNotEmpty(result.FieldPath);
        }

        [TestCase(false, TestName = "F1-FFFD")]
        [TestCase(true, TestName = "F1-PAIR")]
        public void LegalUnicodePlayerIdentityIsPreservedAndFingerprintIsStable(bool pair)
        {
            var job = Job();
            var replacement = PrepareLoggedIdentity("P\uFFFD", job);
            var supplementary = PrepareLoggedIdentity("P\uD83D\uDE00", job);
            var ascii = PrepareLoggedIdentity("P", job);
            Assert.IsTrue(replacement.IsAccepted, replacement.RejectionCode);
            Assert.IsTrue(supplementary.IsAccepted, supplementary.RejectionCode);
            Assert.IsTrue(ascii.IsAccepted, ascii.RejectionCode);
            var repeated = PrepareLoggedIdentity(pair ? "P\uD83D\uDE00" : "P\uFFFD", job);
            Assert.IsTrue(repeated.IsAccepted, repeated.RejectionCode);
            Assert.IsTrue(job.IsCurrent);
            Assert.AreEqual((pair ? supplementary : replacement).Candidate.Fingerprint, repeated.Candidate.Fingerprint);
            Assert.AreNotEqual(replacement.Candidate.Fingerprint, supplementary.Candidate.Fingerprint);
            Assert.AreNotEqual(replacement.Candidate.Fingerprint, ascii.Candidate.Fingerprint);
            Assert.AreNotEqual(supplementary.Candidate.Fingerprint, ascii.Candidate.Fingerprint);
        }

        private static DemoContentPreparationResult PrepareLoggedIdentity(string identity, DemoContentJob job)
        {
            var input = MakeInput(); input.PlayerId = identity;
            DemoContentPreparationResult result = null;
            Assert.DoesNotThrow(() => result = DemoContentCompiler.PrepareCandidate(input, job, Math()));
            var candidate = result.Candidate;
            TestContext.Out.WriteLine("F1|UTF16=" + Utf16Units(identity) + "|Accepted=" + result.IsAccepted +
                "|Candidate=" + (candidate != null) + "|Rejection=" + (result.RejectionCode ?? "<null>") +
                "|Fingerprint=" + (candidate?.Fingerprint ?? "<null>") + "|Character=" + Utf16Units(candidate?.Character.PlayerId) +
                "|Entry=" + Utf16Units(candidate?.Entry.PlayerId) + "|JobCurrent=" + job.IsCurrent);
            if (candidate != null)
            {
                Assert.AreEqual(identity, candidate.Character.PlayerId);
                Assert.AreEqual(identity, candidate.Entry.PlayerId);
                Assert.IsTrue(candidate.IsCurrent);
            }
            return result;
        }

        private static string Utf16Units(string value)
        { return value == null ? "<null>" : string.Concat(value.Select(c => "\\u" + ((int)c).ToString("X4", System.Globalization.CultureInfo.InvariantCulture))); }
    }
}
