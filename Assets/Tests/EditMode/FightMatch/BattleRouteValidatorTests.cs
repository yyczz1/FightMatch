using System;
using System.Collections.Generic;
using FightMatch.Core;
using FlowPuzzle.Core;
using NUnit.Framework;

namespace FightMatch.Core.Tests
{
    [TestFixture]
    public class BattleRouteValidatorTests
    {
        [TestCase(false)]
        [TestCase(true)]
        public void Validate_OnlyOnePairDrawnWithBlankCells_AcceptsBothDirections(bool reverse)
        {
            var cells = Cells(0, 0, 1, 0, 2, 0, 3, 0);
            if (reverse)
                cells.Reverse();

            AssertValid(Check(CreateLevel(), NoPaths(), Cells(), 7, cells));
        }

        [Test]
        public void Validate_TwoAdjacentEndpoints_AcceptsMinimumLength()
        {
            var level = CreateLevel();
            level.pairs[0].endpointB = new FlowPos(1, 0);

            AssertValid(Check(level, NoPaths(), Cells(), 7, Cells(0, 0, 1, 0)));
        }

        [Test]
        public void Validate_UnrelatedLockedPathAndBlockedCells_AcceptsDetour()
        {
            var cells = Cells(0, 0, 0, 1, 1, 1, 2, 1, 3, 1, 3, 0);

            AssertValid(Check(CreateLevel(), LockedB(), Cells(1, 0, 2, 0), 7, cells));
        }

        [Test]
        public void Validate_LegalRouteSeparatesRemainingPair_Accepts()
        {
            var level = new FlowLevelData
            {
                width = 4,
                height = 4,
                pairs = new List<FlowPairData>
                {
                    Pair(7, 1, 0, 1, 3),
                    Pair(23, 0, 0, 3, 3)
                }
            };

            AssertValid(Check(level, NoPaths(), Cells(), 7, Cells(1, 0, 1, 1, 1, 2, 1, 3)));
        }

        [Test]
        public void Validate_PairNotFound_PrecedesPathTooShort()
        {
            AssertInvalid(Check(CreateLevel(), LockedB(), Cells(), 99, Cells()), "PairNotFound");
        }

        [TestCase(false)]
        [TestCase(true)]
        public void Validate_PairAlreadyLocked_PrecedesPathTooShort(bool empty)
        {
            var locks = LockedB();
            locks.Add(new FlowPathData { colorId = 7, cells = Cells(0, 0, 1, 0, 2, 0, 3, 0) });
            var cells = empty ? Cells() : Cells(0, 0, 1, 0, 2, 0, 3, 0);

            AssertInvalid(Check(CreateLevel(), locks, Cells(), 7, cells), "PairAlreadyLocked");
        }

        [TestCase(0)]
        [TestCase(1)]
        public void Validate_PathTooShort_PrecedesEndpointMismatch(int count)
        {
            var cells = Cells(-1, -1).GetRange(0, count);

            AssertInvalid(Check(CreateLevel(), NoPaths(), Cells(), 7, cells), "PathTooShort");
        }

        [TestCaseSource(nameof(MismatchedEndpoints))]
        public void Validate_EndpointMismatch_PrecedesCellErrors(List<FlowPos> cells)
        {
            AssertInvalid(Check(CreateLevel(), NoPaths(), Cells(0, 0), 7, cells), "EndpointMismatch");
        }

        private static IEnumerable<TestCaseData> MismatchedEndpoints()
        {
            yield return new TestCaseData(Cells(0, 1, 1, 1, 2, 1, 3, 0));
            yield return new TestCaseData(Cells(0, 0, 1, 0, 2, 0, 3, 1));
            yield return new TestCaseData(Cells(0, 0, 1, 0, 0, 0));
            yield return new TestCaseData(Cells(int.MinValue, 0, 3, 0));
        }

        [TestCase(-1, 0)]
        [TestCase(4, 0)]
        [TestCase(0, -1)]
        [TestCase(0, 4)]
        [TestCase(int.MinValue, 0)]
        [TestCase(int.MaxValue, 0)]
        [TestCase(0, int.MinValue)]
        [TestCase(0, int.MaxValue)]
        public void Validate_OutOfBounds_PrecedesAdjacencyAndBlockedCell(int x, int y)
        {
            var cells = Cells(0, 0, x, y, 3, 0);

            AssertInvalid(Check(CreateLevel(), NoPaths(), Cells(x, y), 7, cells), "CellOutOfBounds", 1);
        }

        [TestCase(1, 1)]
        [TestCase(2, 0)]
        [TestCase(0, 0)]
        [TestCase(0, 2)]
        public void Validate_NonAdjacent_PrecedesOtherCellErrors(int x, int y)
        {
            var cells = Cells(0, 0, x, y, 3, 0);
            var blocked = x == 0 && y == 0 ? Cells() : Cells(x, y);

            AssertInvalid(Check(CreateLevel(), LockedB(), blocked, 7, cells), "NonAdjacent", 1);
        }

        [TestCaseSource(nameof(SelfIntersectingPaths))]
        public void Validate_SelfIntersection_ReportsFirstRepeatedCell(List<FlowPos> cells, int index)
        {
            AssertInvalid(Check(CreateLevel(), NoPaths(), Cells(), 7, cells), "SelfIntersection", index);
        }

        private static IEnumerable<TestCaseData> SelfIntersectingPaths()
        {
            yield return new TestCaseData(Cells(0, 0, 1, 0, 1, 1, 1, 0, 2, 0, 3, 0), 3);
            yield return new TestCaseData(Cells(0, 0, 1, 0, 0, 0, 1, 0, 2, 0, 3, 0), 2);
            yield return new TestCaseData(Cells(0, 0, 1, 0, 2, 0, 3, 0, 3, 1, 3, 0), 5);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void Validate_ForeignEndpoint_PrecedesLockedOverlapAndBlockedCell(bool reverse)
        {
            var cells = Cells(0, 0, 0, 1, 0, 2, 1, 2, 2, 2, 3, 2, 3, 1, 3, 0);
            if (reverse)
                cells.Reverse();

            AssertInvalid(Check(CreateLevel(), LockedB(), Cells(0, 2, 3, 2), 7, cells),
                "ForeignEndpoint", 2);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void Validate_LockedOverlap_PrecedesBlockedCell(bool alsoBlocked)
        {
            var cells = Cells(0, 0, 1, 0, 1, 1, 1, 2, 2, 2, 2, 1, 2, 0, 3, 0);
            var blocked = alsoBlocked ? Cells(1, 2) : Cells();

            AssertInvalid(Check(CreateLevel(), LockedB(), blocked, 7, cells), "LockedOverlap", 3);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(3)]
        public void Validate_BlockedCell_ReportsStartMiddleOrEnd(int index)
        {
            var cells = Cells(0, 0, 1, 0, 2, 0, 3, 0);

            AssertInvalid(Check(CreateLevel(), NoPaths(), new[] { cells[index] }, 7, cells),
                "BlockedCell", index);
        }

        [Test]
        public void Validate_EarlierBlockedCell_PrecedesLaterOutOfBounds()
        {
            var cells = Cells(0, 0, 1, 0, int.MinValue, 0, 3, 0);

            AssertInvalid(Check(CreateLevel(), NoPaths(), Cells(1, 0), 7, cells), "BlockedCell", 1);
        }

        [TestCase("level")]
        [TestCase("lockedPaths")]
        [TestCase("blockedCells")]
        [TestCase("cells")]
        public void Validate_NullRequiredArgument_ThrowsArgumentNullException(string parameter)
        {
            var level = parameter == "level" ? null : CreateLevel();
            var locks = parameter == "lockedPaths" ? null : NoPaths();
            var blocked = parameter == "blockedCells" ? null : Cells();
            var cells = parameter == "cells" ? null : Cells(0, 0, 1, 0, 2, 0, 3, 0);

            var exception = Assert.Throws<ArgumentNullException>(() =>
                new BattleRouteValidator().Validate(level, locks, blocked, 7, cells));

            Assert.AreEqual(parameter, exception.ParamName);
        }

        [Test]
        public void Validate_LaterInputMutation_DoesNotChangePreviousResults()
        {
            var level = CreateLevel();
            var locks = LockedB();
            var blocked = Cells();
            var cells = Cells(0, 0, 1, 0, 2, 0, 3, 0);
            var valid = Check(level, locks, blocked, 7, cells);
            blocked.Add(cells[1]);
            var invalid = Check(level, locks, blocked, 7, cells);

            level.width = 1;
            level.pairs[0].colorId = 99;
            level.pairs.Clear();
            locks[0].colorId = 99;
            locks[0].cells.Clear();
            locks.Clear();
            blocked.Clear();
            cells.Clear();

            AssertValid(valid);
            AssertInvalid(invalid, "BlockedCell", 1);
        }

        private static RouteValidationResult Check(
            FlowLevelData level, IReadOnlyList<FlowPathData> locks,
            IReadOnlyCollection<FlowPos> blocked, int colorId, IReadOnlyList<FlowPos> cells)
        {
            var before = Snapshot(level, locks, blocked, cells);
            var validator = new BattleRouteValidator();
            var first = validator.Validate(level, locks, blocked, colorId, cells);
            CollectionAssert.AreEqual(before, Snapshot(level, locks, blocked, cells));
            var second = validator.Validate(level, locks, blocked, colorId, cells);
            CollectionAssert.AreEqual(before, Snapshot(level, locks, blocked, cells));
            Assert.AreEqual(first.IsValid, second.IsValid);
            Assert.AreEqual(first.ReasonCode, second.ReasonCode);
            Assert.AreEqual(first.CellIndex, second.CellIndex);
            return first;
        }

        private static object[] Snapshot(
            FlowLevelData level, IReadOnlyList<FlowPathData> locks,
            IReadOnlyCollection<FlowPos> blocked, IReadOnlyList<FlowPos> cells)
        {
            var values = new List<object>
            {
                level.levelId, level.width, level.height, level.difficulty, level.difficultyScore,
                level.pairs.Count, locks.Count, blocked.Count, cells.Count
            };
            foreach (var pair in level.pairs)
            {
                values.Add(pair.colorId);
                values.Add(pair.endpointA);
                values.Add(pair.endpointB);
            }
            foreach (var path in locks)
            {
                values.Add(path.colorId);
                values.Add(path.cells.Count);
                foreach (var cell in path.cells)
                    values.Add(cell);
            }
            foreach (var cell in blocked)
                values.Add(cell);
            foreach (var cell in cells)
                values.Add(cell);
            return values.ToArray();
        }

        private static void AssertValid(RouteValidationResult result)
        {
            Assert.IsTrue(result.IsValid);
            Assert.AreEqual(string.Empty, result.ReasonCode);
            Assert.IsNull(result.CellIndex);
        }

        private static void AssertInvalid(RouteValidationResult result, string reason, int? index = null)
        {
            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(reason, result.ReasonCode);
            Assert.AreEqual(index, result.CellIndex);
        }

        private static FlowLevelData CreateLevel()
        {
            return new FlowLevelData
            {
                levelId = 42,
                width = 4,
                height = 4,
                difficultyScore = 12.5f,
                pairs = new List<FlowPairData> { Pair(7, 0, 0, 3, 0), Pair(23, 0, 2, 3, 2) }
            };
        }

        private static FlowPairData Pair(int colorId, int ax, int ay, int bx, int by)
        {
            return new FlowPairData
            {
                colorId = colorId,
                endpointA = new FlowPos(ax, ay),
                endpointB = new FlowPos(bx, by)
            };
        }

        private static List<FlowPathData> NoPaths()
        {
            return new List<FlowPathData>();
        }

        private static List<FlowPathData> LockedB()
        {
            return new List<FlowPathData>
            {
                new FlowPathData { colorId = 23, cells = Cells(0, 2, 1, 2, 2, 2, 3, 2) }
            };
        }

        private static List<FlowPos> Cells(params int[] coordinates)
        {
            var cells = new List<FlowPos>();
            for (var i = 0; i < coordinates.Length; i += 2)
                cells.Add(new FlowPos(coordinates[i], coordinates[i + 1]));
            return cells;
        }
    }
}
