using System;
using System.Collections.Generic;
using System.Linq;
using FlowPuzzle.Core;
using FlowPuzzle.Generation;
using NUnit.Framework;

namespace FlowPuzzle.Tests.Generation
{
    [TestFixture]
    public class RandomizedDfsPathGenerationStrategyTests
    {
        [Test]
        public void TryGeneratePath_ExactTargetLength()
        {
            var board = new FlowBoard(5, 5);
            var strategy = new RandomizedDfsPathGenerationStrategy();
            var random = new SystemFlowRandom(42);

            var ok = strategy.TryGeneratePath(board, 0, 4, 100, 0f, 0f, random, out var path);

            Assert.IsTrue(ok);
            Assert.IsNotNull(path);
            Assert.AreEqual(4, path.cells.Count);
            Assert.AreEqual(0, path.colorId);
        }

        [Test]
        public void TryGeneratePath_OrthogonalSteps()
        {
            var board = new FlowBoard(5, 5);
            var strategy = new RandomizedDfsPathGenerationStrategy();
            var random = new SystemFlowRandom(42);

            strategy.TryGeneratePath(board, 0, 5, 100, 0f, 0f, random, out var path);

            for (var i = 1; i < path.cells.Count; i++)
                Assert.IsTrue(FlowPathUtility.AreAdjacent(path.cells[i - 1], path.cells[i]));
        }

        [Test]
        public void TryGeneratePath_AvoidsOccupiedCells()
        {
            var board = new FlowBoard(5, 5);
            board.Set(new FlowPos(2, 0), 9);
            board.Set(new FlowPos(2, 1), 9);

            var strategy = new RandomizedDfsPathGenerationStrategy();
            var random = new SystemFlowRandom(42);

            // only 2 cells occupied, plenty of room for a short path
            var ok = strategy.TryGeneratePath(board, 0, 3, 200, 0f, 0f, random, out var path);

            Assert.IsTrue(ok);
            foreach (var cell in path.cells)
                Assert.IsTrue(board.IsEmpty(cell) || board.Get(cell) == 0,
                    $"Cell {cell.x},{cell.y} should be empty or our color");
        }

        [Test]
        public void TryGeneratePath_Success_CommitsCells()
        {
            var board = new FlowBoard(5, 5);
            var strategy = new RandomizedDfsPathGenerationStrategy();
            var random = new SystemFlowRandom(42);

            strategy.TryGeneratePath(board, 1, 4, 100, 0f, 0f, random, out var path);

            foreach (var cell in path.cells)
                Assert.AreEqual(1, board.Get(cell));
            Assert.AreEqual(4, board.OccupiedCellCount);
        }

        [Test]
        public void TryGeneratePath_SameSeed_SamePath()
        {
            var board1 = new FlowBoard(6, 6);
            var board2 = new FlowBoard(6, 6);

            var strategy = new RandomizedDfsPathGenerationStrategy();
            strategy.TryGeneratePath(board1, 0, 5, 100, 0f, 0f, new SystemFlowRandom(42), out var path1);
            strategy.TryGeneratePath(board2, 0, 5, 100, 0f, 0f, new SystemFlowRandom(42), out var path2);

            Assert.IsTrue(path1.cells.SequenceEqual(path2.cells));
        }

        [Test]
        public void TryGeneratePath_ImpossibleLength_FailsWithoutMutation()
        {
            var board = new FlowBoard(2, 2);
            var occupiedBefore = board.OccupiedCellCount;

            var strategy = new RandomizedDfsPathGenerationStrategy();
            // 5 cells on a 2x2 board → impossible
            var ok = strategy.TryGeneratePath(board, 0, 5, 50, 0f, 0f, new SystemFlowRandom(1), out var path);

            Assert.IsFalse(ok);
            Assert.IsNull(path);
            Assert.AreEqual(occupiedBefore, board.OccupiedCellCount);
        }

        [Test]
        public void TryGeneratePath_Failure_LeavesExistingCells()
        {
            var board = new FlowBoard(5, 5);
            board.Set(new FlowPos(0, 0), 9);
            board.Set(new FlowPos(4, 4), 9);

            var strategy = new RandomizedDfsPathGenerationStrategy();
            var random = new SystemFlowRandom(1);
            // length = 24 > empty cells (25-2=23) → impossible
            var ok = strategy.TryGeneratePath(board, 0, 24, 20, 0f, 0f, random, out var path);

            Assert.IsFalse(ok);
            Assert.AreEqual(9, board.Get(new FlowPos(0, 0)));
            Assert.AreEqual(9, board.Get(new FlowPos(4, 4)));
        }

        [Test]
        public void TryGeneratePath_MaxAttemptZero_FailsImmediately()
        {
            var board = new FlowBoard(5, 5);
            var strategy = new RandomizedDfsPathGenerationStrategy();

            var ok = strategy.TryGeneratePath(board, 0, 3, 0, 0f, 0f, new SystemFlowRandom(1), out var path);

            Assert.IsFalse(ok);
            Assert.IsNull(path);
        }

        [Test]
        public void TryGeneratePath_TurnPreference_CanBeExercised()
        {
            var board = new FlowBoard(6, 6);
            var strategy = new RandomizedDfsPathGenerationStrategy();
            // positive turn preference → should still produce a valid path
            var ok = strategy.TryGeneratePath(board, 0, 4, 200, 2f, 0f, new SystemFlowRandom(42), out var path);

            Assert.IsTrue(ok);
            Assert.IsNotNull(path);
        }

        [Test]
        public void TryGeneratePath_NullBoard_Throws()
        {
            var strategy = new RandomizedDfsPathGenerationStrategy();
            Assert.Throws<ArgumentNullException>(() =>
                strategy.TryGeneratePath(null, 0, 3, 10, 0f, 0f, new SystemFlowRandom(1), out _));
        }

        [Test]
        public void TryGeneratePath_NullRandom_Throws()
        {
            var strategy = new RandomizedDfsPathGenerationStrategy();
            Assert.Throws<ArgumentNullException>(() =>
                strategy.TryGeneratePath(new FlowBoard(3, 3), 0, 3, 10, 0f, 0f, null, out _));
        }

        [Test]
        public void TryGeneratePath_NegativeColorId_Throws()
        {
            var strategy = new RandomizedDfsPathGenerationStrategy();
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                strategy.TryGeneratePath(new FlowBoard(3, 3), -1, 3, 10, 0f, 0f, new SystemFlowRandom(1), out _));
        }

        [Test]
        public void TryGeneratePath_TargetBelowTwo_Fails()
        {
            var board = new FlowBoard(5, 5);
            var strategy = new RandomizedDfsPathGenerationStrategy();
            var ok = strategy.TryGeneratePath(board, 0, 1, 10, 0f, 0f, new SystemFlowRandom(1), out var path);

            Assert.IsFalse(ok);
            Assert.IsNull(path);
        }

        [Test]
        public void TryGeneratePath_NoDuplicates()
        {
            var board = new FlowBoard(5, 5);
            var strategy = new RandomizedDfsPathGenerationStrategy();
            strategy.TryGeneratePath(board, 0, 5, 100, 0f, 0f, new SystemFlowRandom(42), out var path);

            Assert.IsFalse(FlowPathUtility.HasDuplicateCells(path.cells));
        }
    }
}
