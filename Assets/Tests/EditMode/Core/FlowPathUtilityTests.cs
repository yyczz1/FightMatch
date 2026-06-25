using System.Collections.Generic;
using FlowPuzzle.Core;
using NUnit.Framework;

namespace FlowPuzzle.Tests.Core
{
    [TestFixture]
    public class FlowPathUtilityTests
    {
        [Test]
        public void AreAdjacent_Orthogonal_ReturnsTrue()
        {
            Assert.IsTrue(FlowPathUtility.AreAdjacent(new FlowPos(1, 2), new FlowPos(1, 3)));
            Assert.IsTrue(FlowPathUtility.AreAdjacent(new FlowPos(1, 2), new FlowPos(2, 2)));
        }

        [Test]
        public void AreAdjacent_Diagonal_ReturnsFalse()
        {
            Assert.IsFalse(FlowPathUtility.AreAdjacent(new FlowPos(1, 2), new FlowPos(2, 3)));
        }

        [Test]
        public void AreAdjacent_SameCell_ReturnsFalse()
        {
            Assert.IsFalse(FlowPathUtility.AreAdjacent(new FlowPos(1, 2), new FlowPos(1, 2)));
        }

        [Test]
        public void AreAdjacent_FarApart_ReturnsFalse()
        {
            Assert.IsFalse(FlowPathUtility.AreAdjacent(new FlowPos(0, 0), new FlowPos(0, 5)));
        }

        [Test]
        public void GetManhattanDistance_ReturnsDifference()
        {
            Assert.AreEqual(3, FlowPathUtility.GetManhattanDistance(new FlowPos(0, 0), new FlowPos(0, 3)));
            Assert.AreEqual(5, FlowPathUtility.GetManhattanDistance(new FlowPos(1, 2), new FlowPos(4, 4)));
        }

        [Test]
        public void GetMoveCount_SingleCell_ReturnsZero()
        {
            var cells = new List<FlowPos> { new FlowPos(0, 0) };
            Assert.AreEqual(0, FlowPathUtility.GetMoveCount(cells));
        }

        [Test]
        public void GetMoveCount_MultipleCells_ReturnsCountMinusOne()
        {
            var cells = new List<FlowPos>
            {
                new FlowPos(0, 0),
                new FlowPos(0, 1),
                new FlowPos(0, 2),
                new FlowPos(0, 3)
            };

            Assert.AreEqual(3, FlowPathUtility.GetMoveCount(cells));
        }

        [Test]
        public void CountTurns_StraightPath_ReturnsZero()
        {
            var cells = new List<FlowPos>
            {
                new FlowPos(0, 0),
                new FlowPos(1, 0),
                new FlowPos(2, 0),
                new FlowPos(3, 0)
            };

            Assert.AreEqual(0, FlowPathUtility.CountTurns(cells));
        }

        [Test]
        public void CountTurns_LPath_ReturnsOne()
        {
            var cells = new List<FlowPos>
            {
                new FlowPos(0, 0),
                new FlowPos(1, 0),
                new FlowPos(1, 1),
                new FlowPos(1, 2)
            };

            Assert.AreEqual(1, FlowPathUtility.CountTurns(cells));
        }

        [Test]
        public void CountTurns_MultiTurn_CountsEveryChange()
        {
            var cells = new List<FlowPos>
            {
                new FlowPos(0, 0),
                new FlowPos(1, 0), // go right
                new FlowPos(1, 1), // turn up
                new FlowPos(2, 1), // turn right
                new FlowPos(2, 2)  // turn up
            };

            Assert.AreEqual(3, FlowPathUtility.CountTurns(cells));
        }

        [Test]
        public void HasDuplicateCells_NoDuplicate_ReturnsFalse()
        {
            var cells = new List<FlowPos>
            {
                new FlowPos(0, 0),
                new FlowPos(1, 0),
                new FlowPos(2, 0)
            };

            Assert.IsFalse(FlowPathUtility.HasDuplicateCells(cells));
        }

        [Test]
        public void HasDuplicateCells_WithDuplicate_ReturnsTrue()
        {
            var cells = new List<FlowPos>
            {
                new FlowPos(0, 0),
                new FlowPos(1, 0),
                new FlowPos(0, 0)
            };

            Assert.IsTrue(FlowPathUtility.HasDuplicateCells(cells));
        }

        [Test]
        public void GetDetour_StraightPath_ReturnsZero()
        {
            var cells = new List<FlowPos>
            {
                new FlowPos(0, 0),
                new FlowPos(1, 0),
                new FlowPos(2, 0),
                new FlowPos(3, 0)
            };

            Assert.AreEqual(0, FlowPathUtility.GetDetour(cells));
        }

        [Test]
        public void GetDetour_LPath_ReturnsExtraMoves()
        {
            // (0,0)→(1,0)→(1,1)→(1,2)→(0,2): moves=4, manhattan(0,0→0,2)=2, detour=2
            var cells = new List<FlowPos>
            {
                new FlowPos(0, 0),
                new FlowPos(1, 0),
                new FlowPos(1, 1),
                new FlowPos(1, 2),
                new FlowPos(0, 2)
            };

            Assert.AreEqual(2, FlowPathUtility.GetDetour(cells));
        }

        [Test]
        public void GetMoveCount_Null_ReturnsZero()
        {
            Assert.AreEqual(0, FlowPathUtility.GetMoveCount(null));
        }

        [Test]
        public void GetMoveCount_Empty_ReturnsZero()
        {
            Assert.AreEqual(0, FlowPathUtility.GetMoveCount(new List<FlowPos>()));
        }

        [Test]
        public void CountTurns_Null_ReturnsZero()
        {
            Assert.AreEqual(0, FlowPathUtility.CountTurns(null));
        }

        [Test]
        public void CountTurns_Empty_ReturnsZero()
        {
            Assert.AreEqual(0, FlowPathUtility.CountTurns(new List<FlowPos>()));
        }

        [Test]
        public void CountTurns_SingleCell_ReturnsZero()
        {
            var cells = new List<FlowPos> { new FlowPos(0, 0) };
            Assert.AreEqual(0, FlowPathUtility.CountTurns(cells));
        }

        [Test]
        public void HasDuplicateCells_Null_ReturnsFalse()
        {
            Assert.IsFalse(FlowPathUtility.HasDuplicateCells(null));
        }

        [Test]
        public void HasDuplicateCells_Empty_ReturnsFalse()
        {
            Assert.IsFalse(FlowPathUtility.HasDuplicateCells(new List<FlowPos>()));
        }

        [Test]
        public void HasDuplicateCells_SingleCell_ReturnsFalse()
        {
            Assert.IsFalse(FlowPathUtility.HasDuplicateCells(new List<FlowPos> { new FlowPos(0, 0) }));
        }

        [Test]
        public void GetDetour_Null_ReturnsZero()
        {
            Assert.AreEqual(0, FlowPathUtility.GetDetour(null));
        }

        [Test]
        public void GetDetour_Empty_ReturnsZero()
        {
            Assert.AreEqual(0, FlowPathUtility.GetDetour(new List<FlowPos>()));
        }

        [Test]
        public void GetDetour_SingleCell_ReturnsZero()
        {
            Assert.AreEqual(0, FlowPathUtility.GetDetour(new List<FlowPos> { new FlowPos(0, 0) }));
        }
    }
}
