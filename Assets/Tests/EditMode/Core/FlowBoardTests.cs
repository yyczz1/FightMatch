using System;
using FlowPuzzle.Core;
using NUnit.Framework;

namespace FlowPuzzle.Tests.Core
{
    [TestFixture]
    public class FlowBoardTests
    {
        [Test]
        public void Constructor_ZeroWidth_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new FlowBoard(0, 5));
        }

        [Test]
        public void Constructor_NegativeHeight_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new FlowBoard(5, -1));
        }

        [Test]
        public void Constructor_StoresDimensions()
        {
            var board = new FlowBoard(5, 7);

            Assert.AreEqual(5, board.Width);
            Assert.AreEqual(7, board.Height);
        }

        [Test]
        public void NewBoard_AllCellsEmpty()
        {
            var board = new FlowBoard(3, 3);

            for (var x = 0; x < board.Width; x++)
            for (var y = 0; y < board.Height; y++)
                Assert.IsTrue(board.IsEmpty(new FlowPos(x, y)));
        }

        [Test]
        public void NewBoard_OccupiedCountIsZero()
        {
            var board = new FlowBoard(3, 3);

            Assert.AreEqual(0, board.OccupiedCellCount);
        }

        [Test]
        public void IsInside_ReturnsTrueForBounds()
        {
            var board = new FlowBoard(3, 3);

            Assert.IsTrue(board.IsInside(new FlowPos(0, 0)));
            Assert.IsTrue(board.IsInside(new FlowPos(2, 2)));
        }

        [Test]
        public void IsInside_ReturnsFalseForOutOfBounds()
        {
            var board = new FlowBoard(3, 3);

            Assert.IsFalse(board.IsInside(new FlowPos(-1, 0)));
            Assert.IsFalse(board.IsInside(new FlowPos(0, 3)));
            Assert.IsFalse(board.IsInside(new FlowPos(3, 0)));
        }

        [Test]
        public void Get_OnEmptyCell_ReturnsEmptyColorId()
        {
            var board = new FlowBoard(3, 3);

            Assert.AreEqual(FlowBoard.EmptyColorId, board.Get(new FlowPos(1, 1)));
        }

        [Test]
        public void Get_OutOfBounds_Throws()
        {
            var board = new FlowBoard(3, 3);

            Assert.Throws<ArgumentOutOfRangeException>(() => board.Get(new FlowPos(-1, 0)));
        }

        [Test]
        public void Set_NegativeColorId_Throws()
        {
            var board = new FlowBoard(3, 3);

            Assert.Throws<ArgumentOutOfRangeException>(() => board.Set(new FlowPos(1, 1), -2));
        }

        [Test]
        public void Set_OutOfBounds_Throws()
        {
            var board = new FlowBoard(3, 3);

            Assert.Throws<ArgumentOutOfRangeException>(() => board.Set(new FlowPos(3, 0), 1));
        }

        [Test]
        public void SetAndGet_RoundTrip()
        {
            var board = new FlowBoard(3, 3);
            board.Set(new FlowPos(1, 2), 5);

            Assert.AreEqual(5, board.Get(new FlowPos(1, 2)));
            Assert.IsFalse(board.IsEmpty(new FlowPos(1, 2)));
        }

        [Test]
        public void Clear_RestoresEmptyState()
        {
            var board = new FlowBoard(3, 3);
            board.Set(new FlowPos(1, 2), 5);
            board.Clear(new FlowPos(1, 2));

            Assert.AreEqual(FlowBoard.EmptyColorId, board.Get(new FlowPos(1, 2)));
            Assert.IsTrue(board.IsEmpty(new FlowPos(1, 2)));
        }

        [Test]
        public void Clear_Idempotent()
        {
            var board = new FlowBoard(3, 3);
            board.Clear(new FlowPos(1, 2));
            board.Clear(new FlowPos(1, 2));

            Assert.IsTrue(board.IsEmpty(new FlowPos(1, 2)));
        }

        [Test]
        public void Clear_OutOfBounds_Throws()
        {
            var board = new FlowBoard(3, 3);

            Assert.Throws<ArgumentOutOfRangeException>(() => board.Clear(new FlowPos(3, 3)));
        }

        [Test]
        public void OccupiedCellCount_IncreasesOnEmptyToOccupied()
        {
            var board = new FlowBoard(3, 3);
            Assert.AreEqual(0, board.OccupiedCellCount);

            board.Set(new FlowPos(0, 0), 1);
            Assert.AreEqual(1, board.OccupiedCellCount);

            board.Set(new FlowPos(1, 1), 2);
            Assert.AreEqual(2, board.OccupiedCellCount);
        }

        [Test]
        public void OccupiedCellCount_UnchangedOnReplaceOccupied()
        {
            var board = new FlowBoard(3, 3);
            board.Set(new FlowPos(0, 0), 1);
            Assert.AreEqual(1, board.OccupiedCellCount);

            board.Set(new FlowPos(0, 0), 3);
            Assert.AreEqual(1, board.OccupiedCellCount);
        }

        [Test]
        public void OccupiedCellCount_DecreasesOnClear()
        {
            var board = new FlowBoard(3, 3);
            board.Set(new FlowPos(0, 0), 1);
            board.Set(new FlowPos(1, 1), 2);
            Assert.AreEqual(2, board.OccupiedCellCount);

            board.Clear(new FlowPos(0, 0));
            Assert.AreEqual(1, board.OccupiedCellCount);
        }

        [Test]
        public void OccupiedCellCount_UnchangedOnClearEmpty()
        {
            var board = new FlowBoard(3, 3);
            board.Clear(new FlowPos(0, 0));

            Assert.AreEqual(0, board.OccupiedCellCount);
        }

        [Test]
        public void GetNeighbors_Center_ReturnsFourInOrder()
        {
            var board = new FlowBoard(5, 5);
            var neighbors = board.GetNeighbors(new FlowPos(2, 2));

            Assert.AreEqual(4, neighbors.Count);
            Assert.AreEqual(new FlowPos(3, 2), neighbors[0]); // right
            Assert.AreEqual(new FlowPos(1, 2), neighbors[1]); // left
            Assert.AreEqual(new FlowPos(2, 3), neighbors[2]); // up
            Assert.AreEqual(new FlowPos(2, 1), neighbors[3]); // down
        }

        [Test]
        public void GetNeighbors_Corner_ReturnsTwo()
        {
            var board = new FlowBoard(5, 5);
            var neighbors = board.GetNeighbors(new FlowPos(0, 0));

            Assert.AreEqual(2, neighbors.Count);
            Assert.AreEqual(new FlowPos(1, 0), neighbors[0]); // right
            Assert.AreEqual(new FlowPos(0, 1), neighbors[1]); // up
        }

        [Test]
        public void GetNeighbors_Edge_ReturnsThree()
        {
            var board = new FlowBoard(5, 5);
            var neighbors = board.GetNeighbors(new FlowPos(0, 2));

            Assert.AreEqual(3, neighbors.Count);
            Assert.AreEqual(new FlowPos(1, 2), neighbors[0]); // right
            Assert.AreEqual(new FlowPos(0, 3), neighbors[1]); // up
            Assert.AreEqual(new FlowPos(0, 1), neighbors[2]); // down
        }

        [Test]
        public void GetNeighbors_OutOfBounds_Throws()
        {
            var board = new FlowBoard(3, 3);

            Assert.Throws<ArgumentOutOfRangeException>(() => board.GetNeighbors(new FlowPos(-1, 0)));
        }
    }
}
