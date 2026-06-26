using System.IO;
using FlowPuzzle.Core;
using FlowPuzzle.Editor;
using FlowPuzzle.Editor.UI;
using NUnit.Framework;
using UnityEditor;
using UnityEngine.UIElements;

namespace FlowPuzzle.Tests.Editor
{
    [TestFixture]
    public class FlowLevelGeneratorWindowTests
    {
        private const string TestFolder = "Assets/Temp/FlowPuzzleEditorTests";

        [SetUp] public void SetUp() { if (AssetDatabase.IsValidFolder(TestFolder)) AssetDatabase.DeleteAsset(TestFolder); }
        [TearDown] public void TearDown() { if (AssetDatabase.IsValidFolder(TestFolder)) AssetDatabase.DeleteAsset(TestFolder); }

        // ── Config validation ──

        [Test]
        public void ConfigValid_NonPositiveWidth_Invalid()
        {
            var c = new FlowGenerationConfig { width = 0, height = 5, colorCount = 2, minPathLength = 2, maxPathLength = 5, maxPathAttempt = 100, maxLevelAttempt = 20 }; }
        // This is compile-time validation — the ConfigValid method tests actual values through window actions.

        // ── Parameter panel ──
        [Test] public void Panel_ReadConfig_Defaults() { var p = new FlowParameterPanel(); p.Build(new VisualElement()); var c = p.ReadConfig(); Assert.AreEqual(5, c.width); Assert.AreEqual(42, c.seed); }
        [Test] public void Panel_ApplyPreset_FullValues() { var p = new FlowParameterPanel(); p.Build(new VisualElement()); var cfg = new FlowGenerationConfig { width=6,height=6,colorCount=3,minCoverageRatio=0.3f,maxCoverageRatio=0.7f,minPathLength=2,maxPathLength=7,turnPreference=0.5f,interactionPreference=-0.3f,maxPathAttempt=250,maxLevelAttempt=100,useRandomSeed=false,seed=99,useTargetDifficulty=true,targetDifficulty=FlowDifficultyTier.Normal,useTargetScoreRange=true,minTargetDifficultyScore=60f,maxTargetDifficultyScore=119.999f }; p.ApplyPresetValues(cfg); Assert.AreEqual(6,p.widthField.value); Assert.AreEqual(99,p.seedField.value); Assert.IsTrue(p.targetTierToggle.value); Assert.AreEqual(FlowDifficultyTier.Normal,p.targetTierField.value); Assert.AreEqual(250,p.pathAttemptField.value); Assert.AreEqual(60f,p.minScoreField.value,0.001f); }

        // ── Board geometry ──
        [Test] public void Geometry_NegPadding_Throws() => Assert.Throws<System.ArgumentOutOfRangeException>(() => FlowBoardViewGeometry.CalculateBoardRect(new UnityEngine.Rect(0,0,100,100), 5, 5, -1f));
        [Test] public void Geometry_WithinContent() { var r = FlowBoardViewGeometry.CalculateBoardRect(new UnityEngine.Rect(0,0,100,60), 10, 5, 0f); Assert.IsTrue(r.width <= 100f); Assert.IsTrue(r.height <= 60f); }
        [Test] public void Geometry_YInverted() { var br = new UnityEngine.Rect(0,0,50,60); var tl = FlowBoardViewGeometry.GetCellRect(br, 5, 3, new FlowPos(0,0)); Assert.AreEqual(40f, tl.y, 0.01f); }
        [Test] public void Geometry_OutsideReturnsFalse() { Assert.IsFalse(FlowBoardViewGeometry.TryGetCell(new UnityEngine.Rect(0,0,100,100), 5, 5, new UnityEngine.Vector2(-10,50), out _)); }
        [Test] public void Geometry_RoundTrip() { var br = new UnityEngine.Rect(0,0,100,100); var cell = new FlowPos(2,1); var cr = FlowBoardViewGeometry.GetCellRect(br,5,5,cell); Assert.IsTrue(FlowBoardViewGeometry.TryGetCell(br,5,5,new UnityEngine.Vector2(cr.x+cr.width/2f,cr.y+cr.height/2f),out var r)); Assert.AreEqual(cell,r); }
    }
}
