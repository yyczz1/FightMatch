using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using FlowPuzzle.Core;
using FlowPuzzle.Validation;
using NUnit.Framework;

namespace FightMatch.Core.Tests
{
    [TestFixture]
    internal class DemoContentFixtureTests
    {
        [Test]
        public void SourceFile_HasApprovedBytesOtherwiseRequiresSourceUpdate()
        {
            var path = Path.GetFullPath(DemoContentFixture.BoardsSourcePath);
            Assert.IsTrue(File.Exists(path), "Missing fixture source: " + path);
            using (var stream = File.OpenRead(path))
            using (var sha = SHA256.Create())
            {
                var actual = BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "");
                Assert.AreEqual(DemoContentFixture.BoardsSourceSha256, actual,
                    "SourceChanged: review and update the approved fixture source before continuing.");
            }
        }

        private static IEnumerable<TestCaseData> SourceCases()
        {
            yield return new TestCaseData(1, SourceCoordinateConvention.OneBasedBottomLeft,
                Cells(0, 0, 0, 1, 1, 1), Cells(0, 3, 1, 3, 2, 3)).SetName("Source_L1_BottomLeft");
            yield return new TestCaseData(1, SourceCoordinateConvention.OneBasedTopLeft,
                Cells(0, 3, 0, 2, 1, 2), Cells(0, 0, 1, 0, 2, 0)).SetName("Source_L1_TopLeft");
            yield return new TestCaseData(3, SourceCoordinateConvention.OneBasedBottomLeft,
                Cells(0, 3, 0, 2, 1, 2), Cells(0, 0, 1, 0, 2, 0)).SetName("Source_L3_BottomLeft");
            yield return new TestCaseData(3, SourceCoordinateConvention.OneBasedTopLeft,
                Cells(0, 0, 0, 1, 1, 1), Cells(0, 3, 1, 3, 2, 3)).SetName("Source_L3_TopLeft");
        }

        [TestCaseSource(nameof(SourceCases))]
        public void Source_PreservesProvenanceAndMapsEachPointOnce(int stage,
            SourceCoordinateConvention convention, List<FlowPos> expectedA, List<FlowPos> expectedB)
        {
            var fixture = DemoContentFixture.CaptureSource(stage, 8, convention);
            Assert.AreEqual("L" + stage, fixture.FixtureKey);
            Assert.AreEqual(8, fixture.Revision);
            Assert.AreEqual(FixtureSourceKind.SourceCandidate, fixture.SourceKind);
            Assert.AreEqual("docs/game-design/balance/results/boards.json", fixture.SourcePath);
            Assert.AreEqual("671467CAF72C52EB83A03B396AD82F5557A47B2F9CBD3F9DA6D0EC560A7D2AAD",
                fixture.SourceSha256);
            Assert.AreEqual("stage=" + stage + ",phase=1", fixture.SourceLocator);
            Assert.AreEqual(convention, fixture.SourceConvention);
            Assert.AreEqual(convention == SourceCoordinateConvention.OneBasedBottomLeft
                ? "C-up-v1:(x-1,y-1)" : "C-down-v1:(x-1,height-y)", fixture.CoordinateTransform);
            Assert.AreEqual(2, fixture.Bindings.Count);
            AssertBinding(fixture.Bindings[0], "A", 0, stage == 1 ? "E01" : "E02", 0);
            AssertBinding(fixture.Bindings[1], "B", 1, "E01", 1);
            Assert.AreEqual(2, fixture.OriginalRoutes.Count);
            CollectionAssert.AreEqual(stage == 1 ? Cells(1, 1, 1, 2, 2, 2) : Cells(1, 4, 1, 3, 2, 3),
                fixture.OriginalRoutes[0].Cells);
            CollectionAssert.AreEqual(stage == 1 ? Cells(1, 4, 2, 4, 3, 4) : Cells(1, 1, 2, 1, 3, 1),
                fixture.OriginalRoutes[1].Cells);

            for (var copyIndex = 0; copyIndex < 2; copyIndex++)
            {
                var level = fixture.CopyLevel();
                var solution = fixture.CopySolution();
                Assert.AreEqual(stage, level.levelId);
                Assert.AreEqual(stage, solution.levelId);
                Assert.AreEqual(4, level.width);
                Assert.AreEqual(4, level.height);
                Assert.AreEqual(2, level.pairs.Count);
                Assert.AreEqual(2, solution.paths.Count);
                var expected = new[] { expectedA, expectedB };
                for (var i = 0; i < 2; i++)
                {
                    Assert.AreEqual(i, level.pairs[i].colorId);
                    Assert.AreEqual(i, solution.paths[i].colorId);
                    Assert.AreEqual(i, fixture.OriginalRoutes[i].ColorId);
                    Assert.AreEqual(expected[i][0], level.pairs[i].endpointA);
                    Assert.AreEqual(expected[i][2], level.pairs[i].endpointB);
                    CollectionAssert.AreEqual(expected[i], solution.paths[i].cells);
                }
                Assert.AreEqual(6, solution.paths[0].cells.Count + solution.paths[1].cells.Count);
                Assert.IsTrue(new FlowSolutionValidator().Validate(level, solution).isValid);
            }
            var evidence = fixture.ValidateGeometry();
            Assert.IsTrue(evidence.IsValid);
            Assert.IsNull(evidence.ErrorCode);
            Assert.IsNull(evidence.ErrorMessage);
            Assert.AreSame(fixture, evidence.Capture);
        }

        [TestCase(1, SourceCoordinateConvention.OneBasedBottomLeft)]
        [TestCase(1, SourceCoordinateConvention.OneBasedTopLeft)]
        [TestCase(3, SourceCoordinateConvention.OneBasedBottomLeft)]
        [TestCase(3, SourceCoordinateConvention.OneBasedTopLeft)]
        public void OneRoute_IsLegalStepButNotACompleteSolution(int stage, SourceCoordinateConvention convention)
        {
            var source = DemoContentFixture.CaptureSource(stage, 1, convention);
            var partial = source.CopySolution();
            partial.paths.RemoveAt(1);
            var fixture = DemoContentFixture.CaptureConstructed("partial", 1,
                source.CopyLevel(), partial, source.Bindings);
            Assert.IsTrue(new BattleRouteValidator().Validate(fixture.CopyLevel(),
                new FlowPathData[0], new FlowPos[0], 0, fixture.CopySolution().paths[0].cells).IsValid);
            var evidence = fixture.ValidateGeometry();
            Assert.IsFalse(evidence.IsValid);
            Assert.AreEqual("MissingPath", evidence.ErrorCode);
            Assert.IsNotEmpty(evidence.ErrorMessage);
            Assert.IsTrue(evidence.IsCurrentFor(fixture, false));
        }

        [Test]
        public void Constructed_CV05HasExplicitZeroBasedCoordinatesAndAllowsSeparatingRoute()
        {
            var level = new FlowLevelData { width = 4, height = 4 };
            level.pairs.Add(new FlowPairData { colorId = 0, endpointA = new FlowPos(1, 0), endpointB = new FlowPos(1, 3) });
            level.pairs.Add(new FlowPairData { colorId = 1, endpointA = new FlowPos(0, 0), endpointB = new FlowPos(3, 3) });
            var solution = new FlowSolutionData();
            solution.paths.Add(new FlowPathData { colorId = 0, cells = Cells(1, 0, 1, 1, 1, 2, 1, 3) });
            var fixture = DemoContentFixture.CaptureConstructed("CV05", 1, level, solution, Source().Bindings);
            Assert.AreEqual(FixtureSourceKind.Constructed, fixture.SourceKind);
            Assert.IsNull(fixture.SourcePath);
            Assert.IsNull(fixture.SourceSha256);
            Assert.IsNull(fixture.SourceLocator);
            Assert.IsNull(fixture.SourceConvention);
            Assert.AreEqual("ZeroBasedBottomLeft:identity", fixture.CoordinateTransform);
            CollectionAssert.AreEqual(Cells(1, 0, 1, 1, 1, 2, 1, 3), fixture.CopySolution().paths[0].cells);
            CollectionAssert.AreEqual(solution.paths[0].cells, fixture.OriginalRoutes[0].Cells);
            Assert.IsTrue(new BattleRouteValidator().Validate(fixture.CopyLevel(),
                new FlowPathData[0], new FlowPos[0], 0, fixture.CopySolution().paths[0].cells).IsValid);
            Assert.AreEqual("MissingPath", fixture.ValidateGeometry().ErrorCode);
        }

        [Test]
        public void Constructed_OriginalAndReturnedNestedMutationsLeaveCapturesAndEvidenceUnchanged()
        {
            var source = Source();
            var level = source.CopyLevel();
            level.levelId = 71;
            level.difficulty = (FlowDifficultyTier)1;
            level.difficultyScore = 12.5f;
            var solution = source.CopySolution();
            solution.levelId = 72;
            var bindings = new List<FixturePairBinding>(source.Bindings);
            var first = DemoContentFixture.CaptureConstructed("isolated", 8, level, solution, bindings);
            var second = DemoContentFixture.CaptureConstructed("isolated", 8, level, solution, bindings);
            var expectedLevel = first.CopyLevel();
            var expectedSolution = first.CopySolution();
            var evidence = first.ValidateGeometry();
            Assert.AreNotSame(bindings[0], first.Bindings[0]);
            DestroyCopies(level, solution);
            bindings[0] = new FixturePairBinding("changed", 99, "changed", 9);
            bindings.Clear();
            AssertData(expectedLevel, expectedSolution, first);
            AssertData(expectedLevel, expectedSolution, second);
            DestroyCopies(first.CopyLevel(), first.CopySolution());
            AssertData(expectedLevel, expectedSolution, first);
            AssertData(expectedLevel, expectedSolution, second);
            AssertBinding(first.Bindings[0], "A", 0, "E01", 0);
            CollectionAssert.AreEqual(expectedSolution.paths[0].cells, first.OriginalRoutes[0].Cells);
            Assert.IsTrue(evidence.IsValid);
            Assert.IsTrue(evidence.IsCurrentFor(first, false));
            Assert.IsFalse(evidence.IsCurrentFor(second, false));
            Assert.IsTrue(first.ValidateGeometry().IsValid);
        }

        [Test]
        public void OriginalRoutesAndBindings_DoNotExposeWritableCollectionsOrCells()
        {
            var fixture = Source();
            var cell = fixture.OriginalRoutes[0].Cells[0];
            cell.x = 99;
            Assert.Throws<NotSupportedException>(() =>
                ((IList<FlowPos>)fixture.OriginalRoutes[0].Cells)[0] = cell);
            Assert.Throws<NotSupportedException>(() => ((IList<FixtureRoute>)fixture.OriginalRoutes).Clear());
            Assert.Throws<NotSupportedException>(() => ((IList<FixturePairBinding>)fixture.Bindings).Clear());
            Assert.AreEqual(new FlowPos(1, 1), fixture.OriginalRoutes[0].Cells[0]);
            Assert.IsTrue(fixture.ValidateGeometry().IsValid);
        }

        [Test]
        public void Evidence_RevisionEightCannotBeReusedAfterNineAndSameBytesTen()
        {
            var eight = Source(8);
            var evidence = eight.ValidateGeometry();
            var nine = DemoContentFixture.CaptureSource(1, 9, SourceCoordinateConvention.OneBasedTopLeft);
            var ten = Source(10);
            Assert.IsTrue(evidence.IsValid);
            Assert.IsTrue(evidence.IsCurrentFor(eight, false));
            Assert.IsFalse(evidence.IsCurrentFor(nine, false));
            Assert.IsFalse(evidence.IsCurrentFor(ten, false));
            AssertData(eight.CopyLevel(), eight.CopySolution(), ten);
            Assert.IsTrue(ten.ValidateGeometry().IsCurrentFor(ten, false));
            Assert.IsTrue(ten.ValidateGeometry().IsValid);
        }

        [Test]
        public void Evidence_SameKeyRevisionAndBytesNewCaptureOrCancellationAreNotCurrent()
        {
            var first = Source(8);
            var second = Source(8);
            var evidence = first.ValidateGeometry();
            Assert.AreEqual(first.FixtureKey, second.FixtureKey);
            Assert.AreEqual(first.Revision, second.Revision);
            AssertData(first.CopyLevel(), first.CopySolution(), second);
            Assert.IsFalse(evidence.IsCurrentFor(second, false));
            Assert.IsFalse(evidence.IsCurrentFor(first, true));
            Assert.IsFalse(evidence.IsCurrentFor(null, false));
            Assert.IsTrue(second.ValidateGeometry().IsCurrentFor(second, false));
        }

        [TestCase(1)]
        [TestCase(3)]
        public void Source_MissingConventionIsExplicitlyRejected(int stage)
        {
            var error = Assert.Throws<ArgumentException>(() =>
                DemoContentFixture.CaptureSource(stage, 1, SourceCoordinateConvention.Unspecified));
            Assert.AreEqual("convention", error.ParamName);
            StringAssert.Contains("MissingSourceCoordinateConvention", error.Message);
        }

        [TestCase(0)]
        [TestCase(2)]
        [TestCase(4)]
        public void Source_UnsupportedStageIsRejected(int stage)
        {
            Assert.AreEqual("stage", Assert.Throws<ArgumentOutOfRangeException>(() =>
                DemoContentFixture.CaptureSource(stage, 1, SourceCoordinateConvention.OneBasedBottomLeft)).ParamName);
        }

        [TestCase(-1)]
        [TestCase(3)]
        public void Source_UnknownConventionIsRejected(int convention)
        {
            Assert.AreEqual("convention", Assert.Throws<ArgumentOutOfRangeException>(() =>
                DemoContentFixture.CaptureSource(1, 1, (SourceCoordinateConvention)convention)).ParamName);
        }

        [TestCase(0L)]
        [TestCase(-1L)]
        public void BothCaptures_RejectNonpositiveRevision(long revision)
        {
            var source = Source();
            Assert.AreEqual("revision", Assert.Throws<ArgumentOutOfRangeException>(() => Source(revision)).ParamName);
            Assert.AreEqual("revision", Assert.Throws<ArgumentOutOfRangeException>(() =>
                DemoContentFixture.CaptureConstructed("test", revision,
                    source.CopyLevel(), source.CopySolution(), source.Bindings)).ParamName);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void Constructed_RejectsMissingKey(string key)
        {
            var source = Source();
            Assert.AreEqual("fixtureKey", Assert.Throws<ArgumentException>(() =>
                DemoContentFixture.CaptureConstructed(key, 1,
                    source.CopyLevel(), source.CopySolution(), source.Bindings)).ParamName);
        }

        [TestCase("level")]
        [TestCase("solution")]
        [TestCase("bindings")]
        public void Constructed_RejectsMissingRequiredObject(string field)
        {
            var source = Source();
            Assert.AreEqual(field, Assert.Throws<ArgumentNullException>(() =>
                DemoContentFixture.CaptureConstructed("test", 1,
                    field == "level" ? null : source.CopyLevel(),
                    field == "solution" ? null : source.CopySolution(),
                    field == "bindings" ? null : source.Bindings)).ParamName);
        }

        [TestCase("empty")]
        [TestCase("nullEntry")]
        [TestCase("missingB")]
        public void Constructed_RejectsMissingPairBinding(string kind)
        {
            var source = Source();
            var bindings = new List<FixturePairBinding>(source.Bindings);
            if (kind == "empty") bindings.Clear();
            if (kind == "nullEntry") bindings[1] = null;
            if (kind == "missingB") bindings.RemoveAt(1);
            var error = Assert.Throws<ArgumentException>(() => DemoContentFixture.CaptureConstructed(
                "test", 1, source.CopyLevel(), source.CopySolution(), bindings));
            Assert.AreEqual("bindings", error.ParamName);
            StringAssert.Contains("MissingPairBinding", error.Message);
        }

        [Test]
        public void Binding_RejectsMissingMappingFields()
        {
            Assert.Throws<ArgumentException>(() => new FixturePairBinding(null, 0, "E01", 0));
            Assert.Throws<ArgumentException>(() => new FixturePairBinding("A", 0, "", 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new FixturePairBinding("A", 0, "E01", -1));
        }

        [TestCase("missingPath", "MissingPath")]
        [TestCase("nullPairs", "MissingPairs")]
        [TestCase("nullPaths", "MissingPaths")]
        [TestCase("nullCells", "PathTooShort")]
        [TestCase("dimensions", "InvalidDimensions")]
        [TestCase("nonAdjacent", "InvalidAdjacency")]
        public void GeometryFailure_IsCapturedAndReportedByExistingValidator(string kind, string code)
        {
            var source = Source();
            var level = source.CopyLevel();
            var solution = source.CopySolution();
            if (kind == "missingPath") solution.paths.RemoveAt(1);
            if (kind == "nullPairs") level.pairs = null;
            if (kind == "nullPaths") solution.paths = null;
            if (kind == "nullCells") solution.paths[0].cells = null;
            if (kind == "dimensions") level.width = 0;
            if (kind == "nonAdjacent") solution.paths[0].cells[1] = new FlowPos(3, 3);
            var fixture = DemoContentFixture.CaptureConstructed("invalid-geometry", 1, level, solution, source.Bindings);
            var actual = new FlowSolutionValidator().Validate(level, solution);
            var evidence = fixture.ValidateGeometry();
            Assert.IsFalse(evidence.IsValid);
            Assert.AreEqual(code, evidence.ErrorCode);
            Assert.AreEqual(actual.errorMessage, evidence.ErrorMessage);
            actual.isValid = true;
            actual.errorCode = "changed";
            Assert.AreEqual(code, evidence.ErrorCode);
            Assert.IsFalse(evidence.IsValid);
        }

        [TestCase("pair", "MissingPairObject")]
        [TestCase("path", "MissingPathObject")]
        public void Constructed_RejectsMissingNestedObjectsWithDiagnostic(string kind, string diagnostic)
        {
            var source = Source();
            var level = source.CopyLevel();
            var solution = source.CopySolution();
            if (kind == "pair") level.pairs[0] = null;
            if (kind == "path") solution.paths[0] = null;
            StringAssert.Contains(diagnostic, Assert.Throws<ArgumentException>(() =>
                DemoContentFixture.CaptureConstructed("test", 1, level, solution, source.Bindings)).Message);
        }

        private static DemoContentFixture Source(long revision = 1)
        {
            return DemoContentFixture.CaptureSource(1, revision, SourceCoordinateConvention.OneBasedBottomLeft);
        }

        private static List<FlowPos> Cells(params int[] coordinates)
        {
            var cells = new List<FlowPos>();
            for (var i = 0; i < coordinates.Length; i += 2)
                cells.Add(new FlowPos(coordinates[i], coordinates[i + 1]));
            return cells;
        }

        private static void AssertBinding(FixturePairBinding binding, string pair, int color, string enemy, int slot)
        {
            Assert.AreEqual(pair, binding.SourcePair);
            Assert.AreEqual(color, binding.ColorId);
            Assert.AreEqual(enemy, binding.EnemyAlias);
            Assert.AreEqual(slot, binding.OriginalSlot);
        }

        private static void DestroyCopies(FlowLevelData level, FlowSolutionData solution)
        {
            level.levelId = -1;
            level.width = 1;
            level.height = 1;
            level.difficulty = (FlowDifficultyTier)2;
            level.difficultyScore = -1;
            level.pairs[0].colorId = 99;
            level.pairs[0].endpointA = new FlowPos(99, 99);
            level.pairs[0].endpointB = new FlowPos(98, 98);
            level.pairs.Clear();
            solution.levelId = -1;
            solution.paths[0].colorId = 99;
            solution.paths[0].cells[0] = new FlowPos(99, 99);
            solution.paths[0].cells.Clear();
            solution.paths.Clear();
        }

        private static void AssertData(FlowLevelData expectedLevel, FlowSolutionData expectedSolution,
            DemoContentFixture fixture)
        {
            var level = fixture.CopyLevel();
            var solution = fixture.CopySolution();
            Assert.AreEqual(expectedLevel.levelId, level.levelId);
            Assert.AreEqual(expectedLevel.width, level.width);
            Assert.AreEqual(expectedLevel.height, level.height);
            Assert.AreEqual(expectedLevel.difficulty, level.difficulty);
            Assert.AreEqual(expectedLevel.difficultyScore, level.difficultyScore);
            Assert.AreEqual(expectedLevel.pairs.Count, level.pairs.Count);
            Assert.AreEqual(expectedSolution.levelId, solution.levelId);
            Assert.AreEqual(expectedSolution.paths.Count, solution.paths.Count);
            for (var i = 0; i < level.pairs.Count; i++)
            {
                Assert.AreEqual(expectedLevel.pairs[i].colorId, level.pairs[i].colorId);
                Assert.AreEqual(expectedLevel.pairs[i].endpointA, level.pairs[i].endpointA);
                Assert.AreEqual(expectedLevel.pairs[i].endpointB, level.pairs[i].endpointB);
            }
            for (var i = 0; i < solution.paths.Count; i++)
            {
                Assert.AreEqual(expectedSolution.paths[i].colorId, solution.paths[i].colorId);
                CollectionAssert.AreEqual(expectedSolution.paths[i].cells, solution.paths[i].cells);
            }
        }
    }
}
