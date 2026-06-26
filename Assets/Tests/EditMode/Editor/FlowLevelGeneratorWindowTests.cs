using FlowPuzzle.Core;
using FlowPuzzle.Editor.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

namespace FlowPuzzle.Tests.Editor
{
    [TestFixture]
    public class FlowLevelGeneratorWindowTests
    {
        private static void AssertApprox(float expected, float actual, float delta = 0.001f)
            => Assert.AreEqual(expected, actual, delta);

        // ── Parameter Panel ──

        [Test]
        public void ParameterPanel_ReadConfig_ReturnsDefaults()
        {
            var panel = new FlowParameterPanel();
            var root = new VisualElement(); panel.Build(root);
            var config = panel.ReadConfig();
            Assert.AreEqual(5, config.width); Assert.AreEqual(5, config.height);
            Assert.AreEqual(2, config.colorCount); Assert.AreEqual(42, config.seed);
        }

        [Test]
        public void ParameterPanel_ApplyPresetValues_SetsFields()
        {
            var panel = new FlowParameterPanel();
            var root = new VisualElement(); panel.Build(root);
            var config = new FlowGenerationConfig { width = 6, height = 6, colorCount = 3, turnPreference = 0.5f };
            panel.ApplyPresetValues(config);
            Assert.AreEqual(6, panel.widthField.value);
            Assert.AreEqual(3, panel.colorCountField.value);
            AssertApprox(0.5f, panel.turnPrefField.value);
        }

        // ── Board Geometry ──

        [Test] public void CalculateBoardRect_NegPadding_Throws()
            => Assert.Throws<System.ArgumentOutOfRangeException>(() => FlowBoardViewGeometry.CalculateBoardRect(new Rect(0, 0, 100, 100), 5, 5, -1f));
        [Test]
        public void CalculateBoardRect_ReturnsWithinContent()
        {
            var r = FlowBoardViewGeometry.CalculateBoardRect(new Rect(0, 0, 100, 60), 10, 5, 0f);
            Assert.IsTrue(r.width <= 100f); Assert.IsTrue(r.height <= 60f);
        }
        [Test]
        public void GetCellRect_YInverted()
        {
            var br = new Rect(0, 0, 50, 60);
            var topLeft = FlowBoardViewGeometry.GetCellRect(br, 5, 3, new FlowPos(0, 0));
            AssertApprox(40f, topLeft.y); // Y inverted: row 0 is at bottom
        }
        [Test]
        public void TryGetCell_Outside_ReturnsFalse()
        {
            Assert.IsFalse(FlowBoardViewGeometry.TryGetCell(new Rect(0, 0, 100, 100), 5, 5, new Vector2(-10, 50), out _));
        }
        [Test]
        public void TryGetCell_Center_RoundTrip()
        {
            var br = new Rect(0, 0, 100, 100);
            var cell = new FlowPos(2, 1);
            var cr = FlowBoardViewGeometry.GetCellRect(br, 5, 5, cell);
            var center = new Vector2(cr.x + cr.width / 2f, cr.y + cr.height / 2f);
            Assert.IsTrue(FlowBoardViewGeometry.TryGetCell(br, 5, 5, center, out var result));
            Assert.AreEqual(cell, result);
        }
    }
}
