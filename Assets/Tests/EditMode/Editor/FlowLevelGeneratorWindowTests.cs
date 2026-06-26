using System;
using System.IO;
using FlowPuzzle.Core;
using FlowPuzzle.Editor;
using FlowPuzzle.Editor.UI;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace FlowPuzzle.Tests.Editor
{
    [TestFixture]
    public class FlowLevelGeneratorWindowTests
    {
        private const string TestFolder = "Assets/Temp/FlowPuzzleEditorTests";
        private static readonly string JsonTestDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

        [SetUp] public void SetUp() { if (AssetDatabase.IsValidFolder(TestFolder)) AssetDatabase.DeleteAsset(TestFolder); }
        [TearDown] public void TearDown() { if (AssetDatabase.IsValidFolder(TestFolder)) AssetDatabase.DeleteAsset(TestFolder); if (AssetDatabase.IsValidFolder("Assets/Temp") && AssetDatabase.GetSubFolders("Assets/Temp").Length == 0) AssetDatabase.DeleteAsset("Assets/Temp"); if (Directory.Exists(JsonTestDir)) Directory.Delete(JsonTestDir, true); }

        // ── Parameter panel ──

        [Test] public void Panel_ReadConfig_Defaults() { var p = new FlowParameterPanel(); p.Build(new VisualElement()); var c = p.ReadConfig(); Assert.AreEqual(5, c.width); Assert.AreEqual(42, c.seed); }

        [Test] public void Panel_ApplyPreset_FullValues() { var p = new FlowParameterPanel(); p.Build(new VisualElement()); var cfg = new FlowGenerationConfig { width = 6, height = 6, colorCount = 3, minCoverageRatio = 0.3f, maxCoverageRatio = 0.7f, minPathLength = 2, maxPathLength = 7, turnPreference = 0.5f, interactionPreference = -0.3f, maxPathAttempt = 250, maxLevelAttempt = 100, useRandomSeed = false, seed = 99, useTargetDifficulty = true, targetDifficulty = FlowDifficultyTier.Normal, useTargetScoreRange = true, minTargetDifficultyScore = 60f, maxTargetDifficultyScore = 119.999f }; p.ApplyPresetValues(cfg); Assert.AreEqual(6, p.widthField.value); Assert.AreEqual(99, p.seedField.value); Assert.IsTrue(p.targetTierToggle.value); Assert.AreEqual(FlowDifficultyTier.Normal, p.targetTierField.value); Assert.AreEqual(250, p.pathAttemptField.value); Assert.AreEqual(60f, p.minScoreField.value, 0.001f); Assert.AreEqual(119.999f, p.maxScoreField.value, 0.001f); Assert.AreEqual(100, p.levelAttemptField.value); Assert.IsFalse(p.useRandomSeedToggle.value); Assert.AreEqual(0.5f, p.turnPrefField.value, 0.001f); Assert.AreEqual(-0.3f, p.interactionPrefField.value, 0.001f); }

        // ── Board geometry ──

        [Test] public void Geometry_ZeroWidth_Throws() => Assert.Throws<ArgumentOutOfRangeException>(() => FlowBoardViewGeometry.CalculateBoardRect(new Rect(0,0,100,100), 0, 5, 0f));
        [Test] public void Geometry_ZeroHeight_Throws() => Assert.Throws<ArgumentOutOfRangeException>(() => FlowBoardViewGeometry.CalculateBoardRect(new Rect(0,0,100,100), 5, 0, 0f));
        [Test] public void Geometry_NegPadding_Throws() => Assert.Throws<ArgumentOutOfRangeException>(() => FlowBoardViewGeometry.CalculateBoardRect(new Rect(0,0,100,100), 5, 5, -1f));
        [Test] public void Geometry_Padding_Insets() { var r = FlowBoardViewGeometry.CalculateBoardRect(new Rect(0,0,100,100), 5, 5, 10f); Assert.AreEqual(10f, r.x, 0.01f); Assert.AreEqual(10f, r.y, 0.01f); }
        [Test] public void Geometry_SquareContent() { var r = FlowBoardViewGeometry.CalculateBoardRect(new Rect(0,0,100,100), 5, 5, 0f); Assert.AreEqual(0f, r.x, 0.01f); Assert.AreEqual(100f, r.width, 0.01f); }
        [Test] public void Geometry_AspectFit_Tall() { var r = FlowBoardViewGeometry.CalculateBoardRect(new Rect(0,0,50,200), 5, 5, 0f); Assert.IsTrue(r.width <= 50f); Assert.AreEqual(r.width, r.height, 0.01f); }
        [Test] public void Geometry_FourCorners_YInverted() { var br = new Rect(0,0,50,60); Assert.AreEqual(0f, FlowBoardViewGeometry.GetCellRect(br,5,3,new FlowPos(0,0)).x, 0.01f); Assert.AreEqual(40f, FlowBoardViewGeometry.GetCellRect(br,5,3,new FlowPos(0,0)).y, 0.01f); Assert.AreEqual(40f, FlowBoardViewGeometry.GetCellRect(br,5,3,new FlowPos(4,2)).x, 0.01f); Assert.AreEqual(0f, FlowBoardViewGeometry.GetCellRect(br,5,3,new FlowPos(4,2)).y, 0.01f); }
        [Test] public void Geometry_OutsideLeft() => Assert.IsFalse(FlowBoardViewGeometry.TryGetCell(new Rect(0,0,100,100), 5, 5, new Vector2(-10,50), out _));
        [Test] public void Geometry_OutsideRight() => Assert.IsFalse(FlowBoardViewGeometry.TryGetCell(new Rect(0,0,100,100), 5, 5, new Vector2(150,50), out _));
        [Test] public void Geometry_MaxEdgeOutside() => Assert.IsFalse(FlowBoardViewGeometry.TryGetCell(new Rect(0,0,100,100), 5, 5, new Vector2(100,100), out _));
        [Test] public void Geometry_Center_RoundTrip() { var br = new Rect(0,0,100,100); var cell = new FlowPos(2,1); var cr = FlowBoardViewGeometry.GetCellRect(br,5,5,cell); Assert.IsTrue(FlowBoardViewGeometry.TryGetCell(br,5,5,new Vector2(cr.x+cr.width/2f,cr.y+cr.height/2f),out var r)); Assert.AreEqual(cell,r); }
    }
}
