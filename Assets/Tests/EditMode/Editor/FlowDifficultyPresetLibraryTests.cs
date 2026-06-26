using System;
using FlowPuzzle.Core;
using FlowPuzzle.Editor;
using NUnit.Framework;

namespace FlowPuzzle.Tests.Editor
{
    [TestFixture]
    public class FlowDifficultyPresetLibraryTests
    {
        [Test] public void NullTarget_Throws() => Assert.Throws<ArgumentNullException>(() => FlowDifficultyPresetLibrary.Apply(FlowDifficultyPreset.Easy, null));
        [Test] public void Custom_ChangesNothing() { var t = new FlowGenerationConfig { width = 99, seed = 123 }; FlowDifficultyPresetLibrary.Apply(FlowDifficultyPreset.Custom, t); Assert.AreEqual(99, t.width); Assert.AreEqual(123, t.seed); }

        [Test] public void Easy_SetsAllValues() { var t = new FlowGenerationConfig(); FlowDifficultyPresetLibrary.Apply(FlowDifficultyPreset.Easy, t); Assert.AreEqual(5, t.width); Assert.AreEqual(5, t.height); Assert.AreEqual(2, t.colorCount); Assert.AreEqual(0.25f, t.minCoverageRatio); Assert.AreEqual(0.50f, t.maxCoverageRatio); Assert.AreEqual(2, t.minPathLength); Assert.AreEqual(5, t.maxPathLength); Assert.AreEqual(-0.5f, t.turnPreference); Assert.AreEqual(-0.5f, t.interactionPreference); Assert.AreEqual(250, t.maxPathAttempt); Assert.AreEqual(100, t.maxLevelAttempt); Assert.IsFalse(t.useRandomSeed); Assert.IsFalse(t.useTargetDifficulty); Assert.AreEqual(FlowDifficultyTier.Easy, t.targetDifficulty); Assert.AreEqual(0f, t.minTargetDifficultyScore); Assert.AreEqual(59.999f, t.maxTargetDifficultyScore, 0.001f); }

        [Test] public void Normal_SetsAllValues() { var t = new FlowGenerationConfig(); FlowDifficultyPresetLibrary.Apply(FlowDifficultyPreset.Normal, t); Assert.AreEqual(6, t.width); Assert.AreEqual(6, t.height); Assert.AreEqual(3, t.colorCount); Assert.AreEqual(0.35f, t.minCoverageRatio); Assert.AreEqual(0.65f, t.maxCoverageRatio); Assert.AreEqual(2, t.minPathLength); Assert.AreEqual(7, t.maxPathLength); Assert.AreEqual(0f, t.turnPreference); Assert.AreEqual(0f, t.interactionPreference); Assert.AreEqual(60f, t.minTargetDifficultyScore); Assert.AreEqual(119.999f, t.maxTargetDifficultyScore, 0.001f); Assert.AreEqual(FlowDifficultyTier.Normal, t.targetDifficulty); }

        [Test] public void Hard_SetsAllValues() { var t = new FlowGenerationConfig(); FlowDifficultyPresetLibrary.Apply(FlowDifficultyPreset.Hard, t); Assert.AreEqual(7, t.width); Assert.AreEqual(7, t.height); Assert.AreEqual(4, t.colorCount); Assert.AreEqual(0.50f, t.minCoverageRatio); Assert.AreEqual(0.80f, t.maxCoverageRatio); Assert.AreEqual(120f, t.minTargetDifficultyScore); Assert.AreEqual(199.999f, t.maxTargetDifficultyScore, 0.001f); Assert.AreEqual(FlowDifficultyTier.Hard, t.targetDifficulty); }

        [Test] public void Expert_SetsAllValues() { var t = new FlowGenerationConfig(); FlowDifficultyPresetLibrary.Apply(FlowDifficultyPreset.Expert, t); Assert.AreEqual(8, t.width); Assert.AreEqual(8, t.height); Assert.AreEqual(5, t.colorCount); Assert.AreEqual(0.65f, t.minCoverageRatio); Assert.AreEqual(0.90f, t.maxCoverageRatio); Assert.AreEqual(200f, t.minTargetDifficultyScore); Assert.AreEqual(10000f, t.maxTargetDifficultyScore); Assert.AreEqual(FlowDifficultyTier.Expert, t.targetDifficulty); }

        [Test] public void Config_RemainsEditable() { var t = new FlowGenerationConfig(); FlowDifficultyPresetLibrary.Apply(FlowDifficultyPreset.Easy, t); t.width = 99; Assert.AreEqual(99, t.width); }
    }
}
