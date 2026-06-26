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
        [Test] public void ZeroWidth_Throws() => Assert.Throws<ArgumentOutOfRangeException>(() => FlowBoardViewGeometry.CalculateBoardRect(new Rect(0,0,100,100), 0, 5, 0f));
        [Test] public void ZeroHeight_Throws() => Assert.Throws<ArgumentOutOfRangeException>(() => FlowBoardViewGeometry.CalculateBoardRect(new Rect(0,0,100,100), 5, 0, 0f));
        [Test] public void NegPadding_Throws() => Assert.Throws<ArgumentOutOfRangeException>(() => FlowBoardViewGeometry.CalculateBoardRect(new Rect(0,0,100,100), 5, 5, -1f));
        [Test] public void Padding_Insets() { var r = FlowBoardViewGeometry.CalculateBoardRect(new Rect(0,0,100,100), 5, 5, 10f); Assert.AreEqual(10f, r.x, 0.01f); Assert.AreEqual(10f, r.y, 0.01f); }
        [Test] public void SquareContent() { var r = FlowBoardViewGeometry.CalculateBoardRect(new Rect(0,0,100,100), 5, 5, 0f); Assert.AreEqual(0f, r.x, 0.01f); Assert.AreEqual(100f, r.width, 0.01f); }
        [Test] public void AspectFit_TallContent() { var r = FlowBoardViewGeometry.CalculateBoardRect(new Rect(0,0,50,200), 5, 5, 0f); Assert.IsTrue(r.width <= 50f); Assert.AreEqual(r.width, r.height, 0.01f); }

        [Test] public void GetCellRect_FourCorners_YInverted()
        {
            var br = new Rect(0, 0, 50, 60); // 5×3 → cells 10×20
            var r = FlowBoardViewGeometry.GetCellRect(br, 5, 3, new FlowPos(0, 0)); // bottom-left = top in UI
            Assert.AreEqual(0f, r.x, 0.01f); Assert.AreEqual(40f, r.y, 0.01f); // Y inverted
            r = FlowBoardViewGeometry.GetCellRect(br, 5, 3, new FlowPos(4, 2)); // top-right = bottom in UI
            Assert.AreEqual(40f, r.x, 0.01f); Assert.AreEqual(0f, r.y, 0.01f);
            r = FlowBoardViewGeometry.GetCellRect(br, 5, 3, new FlowPos(0, 2)); // top-left
            Assert.AreEqual(0f, r.x, 0.01f); Assert.AreEqual(0f, r.y, 0.01f);
            r = FlowBoardViewGeometry.GetCellRect(br, 5, 3, new FlowPos(4, 0)); // bottom-right
            Assert.AreEqual(40f, r.x, 0.01f); Assert.AreEqual(40f, r.y, 0.01f);
        }

        [Test] public void TryGetCell_OutsideLeft() => Assert.IsFalse(FlowBoardViewGeometry.TryGetCell(new Rect(0,0,100,100), 5, 5, new Vector2(-10,50), out _));
        [Test] public void TryGetCell_OutsideRight() => Assert.IsFalse(FlowBoardViewGeometry.TryGetCell(new Rect(0,0,100,100), 5, 5, new Vector2(150,50), out _));
        [Test] public void TryGetCell_MaxEdgeOutside() { var br = new Rect(0,0,100,100); Assert.IsFalse(FlowBoardViewGeometry.TryGetCell(br, 5, 5, new Vector2(100,100), out _)); }
        [Test] public void TryGetCell_Center_RoundTrip() { var br = new Rect(0,0,100,100); var cell = new FlowPos(2,1); var cr = FlowBoardViewGeometry.GetCellRect(br, 5, 5, cell); var c = new Vector2(cr.x + cr.width/2f, cr.y + cr.height/2f); Assert.IsTrue(FlowBoardViewGeometry.TryGetCell(br, 5, 5, c, out var result)); Assert.AreEqual(cell, result); }
    }
}
