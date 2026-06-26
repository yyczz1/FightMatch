using System;
using FlowPuzzle.Core;
using FlowPuzzle.Editor.UI;
using NUnit.Framework;
using UnityEngine;

namespace FlowPuzzle.Tests.Editor
{
    [TestFixture]
    public class FlowBoardViewGeometryTests
    {
        [Test] public void CalculateBoardRect_ZeroWidth_Throws()
            => Assert.Throws<ArgumentOutOfRangeException>(() => FlowBoardViewGeometry.CalculateBoardRect(new Rect(0, 0, 100, 100), 0, 5, 4f));
        [Test] public void CalculateBoardRect_NegativePadding_Throws()
            => Assert.Throws<ArgumentOutOfRangeException>(() => FlowBoardViewGeometry.CalculateBoardRect(new Rect(0, 0, 100, 100), 5, 5, -1f));

        [Test]
        public void CalculateBoardRect_SquareContent_SquareBoard()
        {
            var r = FlowBoardViewGeometry.CalculateBoardRect(new Rect(0, 0, 100, 100), 5, 5, 0f);
            Assert.AreEqual(0f, r.x, 0.01f); Assert.AreEqual(0f, r.y, 0.01f);
            Assert.AreEqual(100f, r.width, 0.01f); Assert.AreEqual(100f, r.height, 0.01f);
        }

        [Test]
        public void CalculateBoardRect_Padding_Insets()
        {
            var r = FlowBoardViewGeometry.CalculateBoardRect(new Rect(0, 0, 100, 100), 5, 5, 10f);
            Assert.AreEqual(10f, r.x, 0.01f); Assert.AreEqual(10f, r.y, 0.01f);
        }

        [Test]
        public void CalculateBoardRect_TallContent_AspectFit()
        {
            var r = FlowBoardViewGeometry.CalculateBoardRect(new Rect(0, 0, 50, 200), 5, 5, 0f);
            Assert.IsTrue(r.width <= 50f);
            Assert.IsTrue(r.height <= 200f);
            Assert.AreEqual(r.width, r.height, 0.01f); // square board in any rect
        }

        [Test]
        public void GetCellRect_Corners()
        {
            var br = new Rect(0, 0, 50, 60); // 5x3 board → cells 10x20
            var topLeft = FlowBoardViewGeometry.GetCellRect(br, 5, 3, new FlowPos(0, 0));
            Assert.AreEqual(0f, topLeft.x, 0.01f);
            Assert.AreEqual(40f, topLeft.y, 0.01f); // Y inverted: (0,0) is bottom-left

            var bottomRight = FlowBoardViewGeometry.GetCellRect(br, 5, 3, new FlowPos(4, 2));
            Assert.AreEqual(40f, bottomRight.x, 0.01f);
            Assert.AreEqual(0f, bottomRight.y, 0.01f);
        }

        [Test]
        public void TryGetCell_Outside_ReturnsFalse()
        {
            var br = new Rect(0, 0, 100, 100);
            Assert.IsFalse(FlowBoardViewGeometry.TryGetCell(br, 5, 5, new Vector2(-10, 50), out _));
            Assert.IsFalse(FlowBoardViewGeometry.TryGetCell(br, 5, 5, new Vector2(150, 50), out _));
        }

        [Test]
        public void TryGetCell_RoundTrip_CellCenter()
        {
            var br = new Rect(0, 0, 100, 100);
            var cell = new FlowPos(2, 1);
            var cellRect = FlowBoardViewGeometry.GetCellRect(br, 5, 5, cell);
            var center = new Vector2(cellRect.x + cellRect.width / 2f, cellRect.y + cellRect.height / 2f);
            Assert.IsTrue(FlowBoardViewGeometry.TryGetCell(br, 5, 5, center, out var result));
            Assert.AreEqual(cell, result);
        }
    }
}
