using System;
using System.Collections.Generic;
using FlowPuzzle.Core;
using FlowPuzzle.Editor.Draft;
using NUnit.Framework;

namespace FlowPuzzle.Tests.Editor
{
    [TestFixture]
    public class FlowLevelDraftTests
    {
        private static FlowLevelDraft MakeDraft(int colors = 2)
        {
            var d = new FlowLevelDraft { width = 5, height = 5, colorCount = colors, levelId = 1001, seed = 42 };
            for (int i = 0; i < colors; i++)
                d.pairs.Add(new FlowDraftPairData { colorId = i });
            return d;
        }

        [Test] public void HasCompleteEndpoints_AllPlaced_True()
        {
            var d = MakeDraft();
            d.pairs[0].endpointA = new FlowPos(0,0); d.pairs[0].endpointB = new FlowPos(4,0);
            d.pairs[1].endpointA = new FlowPos(0,1); d.pairs[1].endpointB = new FlowPos(4,1);
            Assert.IsTrue(d.HasCompleteEndpoints);
        }

        [Test] public void HasCompleteEndpoints_MissingEndpoint_False()
        {
            var d = MakeDraft();
            d.pairs[0].endpointA = new FlowPos(0,0);
            d.pairs[1].endpointA = new FlowPos(0,1); d.pairs[1].endpointB = new FlowPos(4,1);
            Assert.IsFalse(d.HasCompleteEndpoints);
        }

        [Test] public void Clone_DeepCopy_NoSharedReferences()
        {
            var d = MakeDraft(1);
            d.pairs[0].endpointA = new FlowPos(1,2);
            var clone = d.Clone();
            clone.pairs[0].endpointA = new FlowPos(9,9);
            Assert.AreEqual(1, d.pairs[0].endpointA.Value.x);
            Assert.AreEqual(9, clone.pairs[0].endpointA.Value.x);
        }

        [Test] public void RestoreFrom_CompleteRoundTrip()
        {
            var d1 = MakeDraft(1);
            d1.pairs[0].endpointA = new FlowPos(1,2); d1.pairs[0].endpointB = new FlowPos(3,4);
            d1.isSolutionDirty = true;

            var d2 = MakeDraft(2);
            d2.RestoreFrom(d1.Clone());

            Assert.AreEqual(1, d2.colorCount);
            Assert.AreEqual(1, d2.pairs[0].endpointA.Value.x);
            Assert.IsTrue(d2.isSolutionDirty);
        }

        [Test] public void MarkDirty_SetsSolutionDirtyAndInvalidated()
        {
            var d = MakeDraft();
            d.isValidated = true; d.isSolutionDirty = false;
            d.MarkDirty();
            Assert.IsTrue(d.isSolutionDirty);
            Assert.IsFalse(d.isValidated);
        }

        [Test] public void LowestUnusedColorId_ReturnsNext()
        {
            var d = MakeDraft(3);
            d.pairs[0].colorId = 0; d.pairs[1].colorId = 2; d.pairs[2].colorId = 5;
            Assert.AreEqual(1, d.LowestUnusedColorId());
        }

        [Test] public void Mapper_FromGeneratedLevel_DeepCopy()
        {
            var ld = new FlowLevelData { levelId = 1, width = 3, height = 3 };
            ld.pairs.Add(new FlowPairData { colorId = 0, endpointA = new(0,0), endpointB = new(2,0) });
            var sd = new FlowSolutionData { levelId = 1 };
            sd.paths.Add(new FlowPathData { colorId = 0, cells = new List<FlowPos> { new(0,0), new(1,0), new(2,0) } });
            var level = new FlowGeneratedLevel { levelData = ld, solutionData = sd, usedSeed = 42, difficultyReport = new FlowDifficultyReport() };

            var draft = FlowDraftMapper.FromGeneratedLevel(level);
            Assert.AreEqual(1, draft.colorCount);
            Assert.AreEqual(0, draft.pairs[0].endpointA.Value.x, 0);
            Assert.IsFalse(draft.isSolutionDirty);
            Assert.IsTrue(draft.isValidated);

            // Source not mutated
            draft.pairs[0].endpointA = new FlowPos(9,9);
            Assert.AreEqual(0, level.levelData.pairs[0].endpointA.x);
        }

        [Test] public void Mapper_ToGeneratedLevel_Valid()
        {
            var d = MakeDraft(1);
            d.pairs[0].endpointA = new FlowPos(0,0); d.pairs[0].endpointB = new FlowPos(2,0);
            d.currentSolution = new FlowSolutionData { levelId = 1001 };
            d.currentDifficulty = new FlowDifficultyReport { totalScore = 50f };
            d.isSolutionDirty = false; d.isValidated = true;

            var result = FlowDraftMapper.ToGeneratedLevel(d);
            Assert.AreEqual(1001, result.levelData.levelId);
            Assert.AreEqual(1, result.levelData.pairs.Count);
        }

        [Test] public void Mapper_ToGeneratedLevel_IncompleteEndpoints_Throws()
        {
            var d = MakeDraft(2);
            d.pairs[0].endpointA = new FlowPos(0,0);
            d.currentSolution = new FlowSolutionData();
            Assert.Throws<InvalidOperationException>(() => FlowDraftMapper.ToGeneratedLevel(d));
        }

        [Test] public void Mapper_ToGeneratedLevel_DirtySolution_Throws()
        {
            var d = MakeDraft(1);
            d.pairs[0].endpointA = new FlowPos(0,0); d.pairs[0].endpointB = new FlowPos(2,0);
            d.currentSolution = new FlowSolutionData();
            d.isSolutionDirty = true;
            Assert.Throws<InvalidOperationException>(() => FlowDraftMapper.ToGeneratedLevel(d));
        }
    }
}
