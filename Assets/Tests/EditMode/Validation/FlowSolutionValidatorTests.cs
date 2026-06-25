using System.Collections.Generic;
using FlowPuzzle.Core;
using FlowPuzzle.Validation;
using NUnit.Framework;

namespace FlowPuzzle.Tests.Validation
{
    [TestFixture]
    public class FlowSolutionValidatorTests
    {
        private FlowLevelData MakeLevel(int w, int h, params (int colorId, int ax, int ay, int bx, int by)[] endpoints)
        {
            var level = new FlowLevelData { width = w, height = h };
            foreach (var (id, ax, ay, bx, by) in endpoints)
                level.pairs.Add(new FlowPairData
                {
                    colorId = id,
                    endpointA = new FlowPos(ax, ay),
                    endpointB = new FlowPos(bx, by)
                });
            return level;
        }

        private FlowSolutionData MakeSolution(params (int colorId, FlowPos[] cells)[] paths)
        {
            var solution = new FlowSolutionData();
            foreach (var (id, cells) in paths)
                solution.paths.Add(new FlowPathData
                {
                    colorId = id,
                    cells = new List<FlowPos>(cells)
                });
            return solution;
        }

        [Test]
        public void ValidSolution_WithUnusedCells_ReturnsValid()
        {
            var level = MakeLevel(3, 3, (0, 0, 0, 2, 0));
            var solution = MakeSolution((0, new[] { new FlowPos(0, 0), new FlowPos(1, 0), new FlowPos(2, 0) }));

            var result = new FlowSolutionValidator().Validate(level, solution);

            Assert.IsTrue(result.isValid);
            Assert.IsNull(result.errorCode);
            Assert.IsNull(result.errorMessage);
        }

        [Test]
        public void ValidSolution_ReversedEndpoints_ReturnsValid()
        {
            var level = MakeLevel(3, 3, (0, 0, 0, 2, 0));
            var solution = MakeSolution((0, new[] { new FlowPos(2, 0), new FlowPos(1, 0), new FlowPos(0, 0) }));

            var result = new FlowSolutionValidator().Validate(level, solution);

            Assert.IsTrue(result.isValid);
        }

        [Test]
        public void NullLevel_ReturnsNullLevel()
        {
            var solution = MakeSolution((0, new[] { new FlowPos(0, 0), new FlowPos(1, 0) }));

            var result = new FlowSolutionValidator().Validate(null, solution);

            Assert.IsFalse(result.isValid);
            Assert.AreEqual("NullLevel", result.errorCode);
        }

        [Test]
        public void NullSolution_ReturnsNullSolution()
        {
            var level = MakeLevel(3, 3, (0, 0, 0, 2, 0));

            var result = new FlowSolutionValidator().Validate(level, null);

            Assert.IsFalse(result.isValid);
            Assert.AreEqual("NullSolution", result.errorCode);
        }

        [Test]
        public void NullPairs_ReturnsMissingPairs()
        {
            var level = new FlowLevelData { width = 3, height = 3, pairs = null };
            var solution = MakeSolution((0, new[] { new FlowPos(0, 0), new FlowPos(1, 0) }));

            var result = new FlowSolutionValidator().Validate(level, solution);

            Assert.IsFalse(result.isValid);
            Assert.AreEqual("MissingPairs", result.errorCode);
        }

        [Test]
        public void NullPaths_ReturnsMissingPaths()
        {
            var level = MakeLevel(3, 3, (0, 0, 0, 2, 0));
            var solution = new FlowSolutionData { paths = null };

            var result = new FlowSolutionValidator().Validate(level, solution);

            Assert.IsFalse(result.isValid);
            Assert.AreEqual("MissingPaths", result.errorCode);
        }

        [Test]
        public void InvalidDimensions_ZeroWidth_ReturnsError()
        {
            var level = MakeLevel(0, 3, (0, 0, 0, 1, 0));
            var solution = MakeSolution((0, new[] { new FlowPos(0, 0), new FlowPos(1, 0) }));

            var result = new FlowSolutionValidator().Validate(level, solution);

            Assert.IsFalse(result.isValid);
            Assert.AreEqual("InvalidDimensions", result.errorCode);
        }

        [Test]
        public void InvalidDimensions_NegativeHeight_ReturnsError()
        {
            var level = MakeLevel(3, -1, (0, 0, 0, 1, 0));
            var solution = MakeSolution((0, new[] { new FlowPos(0, 0), new FlowPos(1, 0) }));

            var result = new FlowSolutionValidator().Validate(level, solution);

            Assert.IsFalse(result.isValid);
            Assert.AreEqual("InvalidDimensions", result.errorCode);
        }

        [Test]
        public void DuplicatePairColor_ReturnsError()
        {
            var level = MakeLevel(4, 4, (0, 0, 0, 2, 0), (0, 1, 1, 3, 1));
            var solution = MakeSolution(
                (0, new[] { new FlowPos(0, 0), new FlowPos(1, 0), new FlowPos(2, 0) }),
                (0, new[] { new FlowPos(1, 1), new FlowPos(2, 1), new FlowPos(3, 1) }));

            var result = new FlowSolutionValidator().Validate(level, solution);

            Assert.IsFalse(result.isValid);
            Assert.AreEqual("DuplicatePairColor", result.errorCode);
        }

        [Test]
        public void EndpointOutOfBounds_ReturnsError()
        {
            var level = MakeLevel(3, 3, (0, 0, 0, 5, 0));

            var result = new FlowSolutionValidator().Validate(level, MakeSolution());

            Assert.IsFalse(result.isValid);
            Assert.AreEqual("EndpointOutOfBounds", result.errorCode);
        }

        [Test]
        public void DuplicatePathColor_ReturnsError()
        {
            var level = MakeLevel(3, 3, (0, 0, 0, 2, 0));
            var solution = MakeSolution(
                (0, new[] { new FlowPos(0, 0), new FlowPos(1, 0) }),
                (0, new[] { new FlowPos(0, 0), new FlowPos(1, 0) }));

            var result = new FlowSolutionValidator().Validate(level, solution);

            Assert.IsFalse(result.isValid);
            Assert.AreEqual("DuplicatePathColor", result.errorCode);
        }

        [Test]
        public void MissingPath_ReturnsError()
        {
            var level = MakeLevel(3, 3, (0, 0, 0, 2, 0), (1, 1, 1, 1, 2));
            var solution = MakeSolution((0, new[] { new FlowPos(0, 0), new FlowPos(1, 0), new FlowPos(2, 0) }));

            var result = new FlowSolutionValidator().Validate(level, solution);

            Assert.IsFalse(result.isValid);
            Assert.AreEqual("MissingPath", result.errorCode);
        }

        [Test]
        public void ExtraPathColor_ReturnsError()
        {
            var level = MakeLevel(3, 3, (0, 0, 0, 2, 0));
            var solution = MakeSolution(
                (0, new[] { new FlowPos(0, 0), new FlowPos(1, 0), new FlowPos(2, 0) }),
                (9, new[] { new FlowPos(1, 1), new FlowPos(2, 1) }));

            var result = new FlowSolutionValidator().Validate(level, solution);

            Assert.IsFalse(result.isValid);
            Assert.AreEqual("ExtraPathColor", result.errorCode);
        }

        [Test]
        public void PathTooShort_ReturnsError()
        {
            var level = MakeLevel(3, 3, (0, 0, 0, 2, 0));
            var solution = MakeSolution((0, new[] { new FlowPos(0, 0) }));

            var result = new FlowSolutionValidator().Validate(level, solution);

            Assert.IsFalse(result.isValid);
            Assert.AreEqual("PathTooShort", result.errorCode);
        }

        [Test]
        public void EndpointMismatch_ReturnsError()
        {
            var level = MakeLevel(3, 3, (0, 0, 0, 2, 0));
            var solution = MakeSolution((0, new[] { new FlowPos(0, 0), new FlowPos(1, 0), new FlowPos(1, 1) }));

            var result = new FlowSolutionValidator().Validate(level, solution);

            Assert.IsFalse(result.isValid);
            Assert.AreEqual("EndpointMismatch", result.errorCode);
        }

        [Test]
        public void CellOutOfBounds_ReturnsError()
        {
            // (3,0) is out of bounds (width=3), placed between two in-bounds adjacent cells
            var level = MakeLevel(3, 4, (0, 0, 0, 2, 0));
            var solution = MakeSolution((0, new[]
            {
                new FlowPos(0, 0), new FlowPos(0, 1), new FlowPos(0, 2),
                new FlowPos(1, 2), new FlowPos(2, 2), new FlowPos(3, 2),
                new FlowPos(3, 1), new FlowPos(3, 0), new FlowPos(2, 0)
            }));

            var result = new FlowSolutionValidator().Validate(level, solution);

            Assert.IsFalse(result.isValid);
            Assert.AreEqual("CellOutOfBounds", result.errorCode);
        }

        [Test]
        public void InvalidAdjacency_Diagonal_ReturnsError()
        {
            var level = MakeLevel(3, 3, (0, 0, 0, 2, 2));
            var solution = MakeSolution((0, new[] { new FlowPos(0, 0), new FlowPos(1, 1), new FlowPos(2, 2) }));

            var result = new FlowSolutionValidator().Validate(level, solution);

            Assert.IsFalse(result.isValid);
            Assert.AreEqual("InvalidAdjacency", result.errorCode);
        }

        [Test]
        public void InvalidAdjacency_Disconnected_ReturnsError()
        {
            var level = MakeLevel(4, 4, (0, 0, 0, 3, 0));
            var solution = MakeSolution((0, new[] { new FlowPos(0, 0), new FlowPos(1, 0), new FlowPos(3, 0) }));

            var result = new FlowSolutionValidator().Validate(level, solution);

            Assert.IsFalse(result.isValid);
            Assert.AreEqual("InvalidAdjacency", result.errorCode);
        }

        [Test]
        public void SelfIntersection_ReturnsError()
        {
            var level = MakeLevel(4, 4, (0, 0, 0, 1, 2));
            var solution = MakeSolution((0, new[]
            {
                new FlowPos(0, 0), new FlowPos(0, 1), new FlowPos(1, 1),
                new FlowPos(1, 0), new FlowPos(1, 1), new FlowPos(1, 2)
            }));

            var result = new FlowSolutionValidator().Validate(level, solution);

            Assert.IsFalse(result.isValid);
            Assert.AreEqual("SelfIntersection", result.errorCode);
        }

        [Test]
        public void PathOverlap_ReturnsError()
        {
            var level = MakeLevel(4, 4, (0, 0, 0, 3, 0), (1, 1, 1, 3, 1));
            var solution = MakeSolution(
                (0, new[] { new FlowPos(0, 0), new FlowPos(1, 0), new FlowPos(2, 0), new FlowPos(3, 0) }),
                (1, new[] { new FlowPos(1, 1), new FlowPos(1, 0), new FlowPos(2, 0), new FlowPos(2, 1), new FlowPos(3, 1) }));

            var result = new FlowSolutionValidator().Validate(level, solution);

            Assert.IsFalse(result.isValid);
            Assert.AreEqual("PathOverlap", result.errorCode);
        }

        [Test]
        public void ForeignEndpointTraversal_ReturnsError()
        {
            var level = MakeLevel(4, 4, (0, 0, 0, 3, 0), (1, 1, 0, 1, 2));
            var solution = MakeSolution(
                (0, new[] { new FlowPos(0, 0), new FlowPos(1, 0), new FlowPos(2, 0), new FlowPos(3, 0) }),
                (1, new[] { new FlowPos(1, 0), new FlowPos(1, 1), new FlowPos(1, 2) }));

            var result = new FlowSolutionValidator().Validate(level, solution);

            Assert.IsFalse(result.isValid);
            Assert.AreEqual("ForeignEndpointTraversal", result.errorCode);
        }

        [Test]
        public void MultiColor_ValidSolution_ReturnsValid()
        {
            var level = MakeLevel(3, 3, (0, 0, 0, 0, 2), (1, 2, 0, 2, 2));
            var solution = MakeSolution(
                (0, new[] { new FlowPos(0, 0), new FlowPos(0, 1), new FlowPos(0, 2) }),
                (1, new[] { new FlowPos(2, 0), new FlowPos(2, 1), new FlowPos(2, 2) }));

            var result = new FlowSolutionValidator().Validate(level, solution);

            Assert.IsTrue(result.isValid);
        }

        [Test]
        public void NonUniqueBoard_WithValidPaths_IsNotRejected()
        {
            var level = MakeLevel(3, 3, (0, 0, 0, 2, 0));
            var solution = MakeSolution((0, new[] { new FlowPos(0, 0), new FlowPos(1, 0), new FlowPos(2, 0) }));

            var result = new FlowSolutionValidator().Validate(level, solution);

            Assert.IsTrue(result.isValid);
        }

        [Test]
        public void ExtraPathColor_FirstUnknownByPathOrder()
        {
            var level = MakeLevel(3, 3, (0, 0, 0, 2, 0));
            // two unknown colors: 9 appears first in path list
            var solution = MakeSolution(
                (9, new[] { new FlowPos(0, 1), new FlowPos(1, 1) }),
                (8, new[] { new FlowPos(0, 2), new FlowPos(1, 2) }),
                (0, new[] { new FlowPos(0, 0), new FlowPos(1, 0), new FlowPos(2, 0) }));

            var result = new FlowSolutionValidator().Validate(level, solution);

            Assert.IsFalse(result.isValid);
            Assert.AreEqual("ExtraPathColor", result.errorCode);
            Assert.IsTrue(result.errorMessage.Contains("9"));
        }

        [Test]
        public void ForeignEndpointBeforeLaterInvalidAdjacency()
        {
            // color 0 path uses (1,1) which is color 1's endpoint → ForeignEndpointTraversal
            // color 2 has diagonal step, but ForeignEndpoint should fire first
            var level = MakeLevel(4, 4,
                (0, 0, 0, 3, 0),
                (1, 1, 1, 1, 2),
                (2, 2, 1, 2, 3));
            var solution = MakeSolution(
                (0, new[] { new FlowPos(0, 0), new FlowPos(1, 0), new FlowPos(1, 1), new FlowPos(2, 1),
                    new FlowPos(2, 0), new FlowPos(3, 0) }),
                (1, new[] { new FlowPos(1, 1), new FlowPos(1, 2) }),
                (2, new[] { new FlowPos(2, 1), new FlowPos(2, 3) }));

            var result = new FlowSolutionValidator().Validate(level, solution);

            Assert.IsFalse(result.isValid);
            Assert.AreEqual("ForeignEndpointTraversal", result.errorCode);
        }

        [Test]
        public void InvalidAdjacencyBeforeSelfIntersection()
        {
            // cell (0,0) repeated via non-adjacent step (1,1)→(0,0) → InvalidAdjacency, not SelfIntersection
            var level = MakeLevel(4, 4, (0, 0, 0, 2, 2));
            var solution = MakeSolution((0, new[]
            {
                new FlowPos(0, 0), new FlowPos(1, 0),   // valid
                new FlowPos(1, 1),                       // valid
                new FlowPos(0, 0),                       // diagonal from (1,1) AND (0,0) already visited → InvalidAdjacency
                new FlowPos(2, 2)                        // would reach endpoint, but shouldn't get here
            }));

            var result = new FlowSolutionValidator().Validate(level, solution);

            Assert.IsFalse(result.isValid);
            Assert.AreEqual("InvalidAdjacency", result.errorCode);
        }

        [Test]
        public void FlowValidationResult_Valid_Factory()
        {
            var r = FlowValidationResult.Valid();

            Assert.IsTrue(r.isValid);
            Assert.IsNull(r.errorCode);
            Assert.IsNull(r.errorMessage);
        }

        [Test]
        public void FlowValidationResult_Invalid_Factory()
        {
            var r = FlowValidationResult.Invalid("ERR", "msg");

            Assert.IsFalse(r.isValid);
            Assert.AreEqual("ERR", r.errorCode);
            Assert.AreEqual("msg", r.errorMessage);
        }
    }
}
