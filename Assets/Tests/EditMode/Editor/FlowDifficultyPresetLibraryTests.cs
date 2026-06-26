using System;
using FlowPuzzle.Core;
using FlowPuzzle.Editor;
using NUnit.Framework;

namespace FlowPuzzle.Tests.Editor
{
    [TestFixture]
    public class FlowDifficultyPresetLibraryTests
    {
        [Test]
        public void NullTarget_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => FlowDifficultyPresetLibrary.Apply(FlowDifficultyPreset.Easy, null));
        }

        [Test]
        public void Custom_ChangesNothing()
        {
            var target = new FlowGenerationConfig { width = 99, height = 88, seed = 123 };
            var clone = new FlowGenerationConfig { width = 99, height = 88, seed = 123 };
            FlowDifficultyPresetLibrary.Apply(FlowDifficultyPreset.Custom, target);
            Assert.AreEqual(clone.width, target.width);
            Assert.AreEqual(clone.height, target.height);
            Assert.AreEqual(clone.seed, target.seed);
        }

        [Test]
        public void Easy_SetsCorrectValues()
        {
            var t = new FlowGenerationConfig();
            FlowDifficultyPresetLibrary.Apply(FlowDifficultyPreset.Easy, t);
            Assert.AreEqual(5, t.width); Assert.AreEqual(5, t.height); Assert.AreEqual(2, t.colorCount);
            Assert.AreEqual(0.25f, t.minCoverageRatio); Assert.AreEqual(0.50f, t.maxCoverageRatio);
            Assert.AreEqual(2, t.minPathLength); Assert.AreEqual(5, t.maxPathLength);
            Assert.AreEqual(-0.5f, t.turnPreference); Assert.AreEqual(-0.5f, t.interactionPreference);
            Assert.AreEqual(250, t.maxPathAttempt); Assert.AreEqual(100, t.maxLevelAttempt);
            Assert.IsFalse(t.useRandomSeed);
            Assert.AreEqual(0f, t.minTargetDifficultyScore); Assert.AreEqual(59.999f, t.maxTargetDifficultyScore, 0.001f);
        }

        [Test]
        public void Normal_SetsCorrectValues()
        {
            var t = new FlowGenerationConfig();
            FlowDifficultyPresetLibrary.Apply(FlowDifficultyPreset.Normal, t);
            Assert.AreEqual(6, t.width); Assert.AreEqual(6, t.height); Assert.AreEqual(3, t.colorCount);
            Assert.AreEqual(0.35f, t.minCoverageRatio); Assert.AreEqual(0.65f, t.maxCoverageRatio);
            Assert.AreEqual(2, t.minPathLength); Assert.AreEqual(7, t.maxPathLength);
            Assert.AreEqual(0f, t.turnPreference); Assert.AreEqual(0f, t.interactionPreference);
            Assert.AreEqual(60f, t.minTargetDifficultyScore); Assert.AreEqual(119.999f, t.maxTargetDifficultyScore, 0.001f);
        }

        [Test]
        public void Hard_SetsCorrectValues()
        {
            var t = new FlowGenerationConfig();
            FlowDifficultyPresetLibrary.Apply(FlowDifficultyPreset.Hard, t);
            Assert.AreEqual(7, t.width); Assert.AreEqual(7, t.height); Assert.AreEqual(4, t.colorCount);
            Assert.AreEqual(0.50f, t.minCoverageRatio); Assert.AreEqual(0.80f, t.maxCoverageRatio);
            Assert.AreEqual(120f, t.minTargetDifficultyScore); Assert.AreEqual(199.999f, t.maxTargetDifficultyScore, 0.001f);
        }

        [Test]
        public void Expert_SetsCorrectValues()
        {
            var t = new FlowGenerationConfig();
            FlowDifficultyPresetLibrary.Apply(FlowDifficultyPreset.Expert, t);
            Assert.AreEqual(8, t.width); Assert.AreEqual(8, t.height); Assert.AreEqual(5, t.colorCount);
            Assert.AreEqual(0.65f, t.minCoverageRatio); Assert.AreEqual(0.90f, t.maxCoverageRatio);
            Assert.AreEqual(200f, t.minTargetDifficultyScore); Assert.AreEqual(10000f, t.maxTargetDifficultyScore);
        }

        [Test]
        public void Config_RemainsEditable_AfterApply()
        {
            var t = new FlowGenerationConfig();
            FlowDifficultyPresetLibrary.Apply(FlowDifficultyPreset.Easy, t);
            t.width = 99; // editable
            Assert.AreEqual(99, t.width);
        }
    }
}
