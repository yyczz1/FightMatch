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
            for (int i = 0; i < colors; i++) d.pairs.Add(new FlowDraftPairData { colorId = i });
            return d;
        }

        // ── Endpoint operations ──

        [Test] public void PlaceEndpoint_SetsAndMarksDirty()
        {
            var d = MakeDraft(); d.isSolutionDirty = false; d.isValidated = true;
            var r = d.PlaceEndpoint(0, true, new FlowPos(1, 2));
            Assert.IsTrue(r.success); Assert.AreEqual(1, d.pairs[0].endpointA.Value.x); Assert.IsTrue(d.isSolutionDirty); Assert.IsFalse(d.isValidated);
        }

        [Test] public void PlaceEndpoint_OutOfBounds_Fails()
        {
            var d = MakeDraft();
            var snap = d.Clone();
            var r = d.PlaceEndpoint(0, true, new FlowPos(10, 10));
            Assert.IsFalse(r.success); Assert.AreEqual("OutOfBounds", r.errorCode);
            Assert.IsNull(d.pairs[0].endpointA, "Draft must be unchanged on failure");
            Assert.IsFalse(d.isSolutionDirty, "Dirty flag must not change on failure");
        }

        [Test] public void PlaceEndpoint_DuplicateCell_Fails()
        {
            var d = MakeDraft();
            d.PlaceEndpoint(0, true, new FlowPos(0, 0));
            d.PlaceEndpoint(1, true, new FlowPos(4, 0));
            // Place on cell already occupied by color 0's endpointA
            var r = d.PlaceEndpoint(1, false, new FlowPos(0, 0));
            Assert.IsFalse(r.success); Assert.AreEqual("EndpointOverlap", r.errorCode);
        }

        [Test] public void MoveEndpoint_UpdatesPosition()
        {
            var d = MakeDraft(); d.PlaceEndpoint(0, true, new FlowPos(0, 0));
            var r = d.MoveEndpoint(0, true, new FlowPos(2, 2));
            Assert.IsTrue(r.success); Assert.AreEqual(2, d.pairs[0].endpointA.Value.x);
        }

        [Test] public void RemoveEndpoint_ClearsAndMarksDirty()
        {
            var d = MakeDraft(); d.PlaceEndpoint(0, true, new FlowPos(0, 0));
            d.isSolutionDirty = false; d.isValidated = true;
            var r = d.RemoveEndpoint(0, true);
            Assert.IsTrue(r.success); Assert.IsNull(d.pairs[0].endpointA); Assert.IsTrue(d.isSolutionDirty);
        }

        [Test] public void HasCompleteEndpoints_VariousConfigs()
        {
            var d = MakeDraft(2);
            Assert.IsFalse(d.HasCompleteEndpoints);
            d.PlaceEndpoint(0, true, new FlowPos(0, 0));  d.PlaceEndpoint(0, false, new FlowPos(4, 0));
            d.PlaceEndpoint(1, true, new FlowPos(0, 1)); d.PlaceEndpoint(1, false, new FlowPos(4, 1));
            Assert.IsTrue(d.HasCompleteEndpoints);
        }

        // ── Color operations ──

        [Test] public void AddColor_IncreasesCount()
        {
            var d = MakeDraft(2); d.colorCount = 2;
            var r = d.AddColor(); Assert.IsTrue(r.success); Assert.AreEqual(3, d.pairs.Count);
        }

        [Test] public void RemoveColor_RemovesPairAndUpdatesCount()
        {
            var d = MakeDraft(2);
            d.colorCount = 2;
            var r = d.RemoveColor(0);
            Assert.IsTrue(r.success); Assert.AreEqual(1, d.pairs.Count);
            // IDs compact: old color 1 moves to 0
            Assert.AreEqual(0, d.pairs[0].colorId, "Higher IDs must compact after removal");
        }

        // ── Resize ──

        [Test] public void Resize_RemovesOutOfBoundsEndpoints()
        {
            var d = MakeDraft(1);
            d.PlaceEndpoint(0, true, new FlowPos(0, 0)); d.PlaceEndpoint(0, false, new FlowPos(4, 0));
            var r = d.Resize(3, 3);
            Assert.IsTrue(r.success); Assert.AreEqual(3, d.width);
            Assert.AreEqual(0, d.pairs[0].endpointA.Value.x); // still in bounds
            Assert.IsNull(d.pairs[0].endpointB); // (4,0) out of bounds on width=3
        }

        [Test] public void Resize_InvalidDimensions_Fails()
        {
            var d = MakeDraft();
            var snap = d.Clone();
            var r = d.Resize(0, 5);
            Assert.IsFalse(r.success); Assert.AreEqual(5, d.width); // unchanged
        }

        // ── Clone / RestoreFrom ──

        [Test] public void Clone_DeepCopy_NoSharedLists()
        {
            var d = MakeDraft(1); d.PlaceEndpoint(0, true, new FlowPos(1, 2));
            var clone = d.Clone();
            clone.pairs[0].endpointA = new FlowPos(9, 9);
            Assert.AreEqual(1, d.pairs[0].endpointA.Value.x);
        }

        [Test] public void RestoreFrom_RestoresAllState()
        {
            var d = MakeDraft(1); d.PlaceEndpoint(0, true, new FlowPos(1, 2));
            d.isSolutionDirty = true; d.currentSolution = new FlowSolutionData { levelId = 99 };
            var snap = d.Clone();

            d.PlaceEndpoint(0, false, new FlowPos(3, 4));
            d.RestoreFrom(snap);
            Assert.AreEqual(1, d.pairs[0].endpointA.Value.x);
            Assert.IsNull(d.pairs[0].endpointB);
            Assert.IsTrue(d.isSolutionDirty);
            Assert.AreEqual(99, d.currentSolution.levelId);
        }

        // ── Mapper ──

        [Test] public void Mapper_FromGeneratedLevel_DeepCopy()
        {
            var ld = new FlowLevelData { levelId = 1, width = 3, height = 3 };
            ld.pairs.Add(new FlowPairData { colorId = 0, endpointA = new(0, 0), endpointB = new(2, 0) });
            var level = new FlowGeneratedLevel { levelData = ld, usedSeed = 42, solutionData = new FlowSolutionData { levelId = 1 }, difficultyReport = new FlowDifficultyReport { totalScore = 50f } };
            var draft = FlowDraftMapper.FromGeneratedLevel(level);
            draft.pairs[0].endpointA = new FlowPos(9, 9); // mutate draft
            Assert.AreEqual(0, level.levelData.pairs[0].endpointA.x, "Source must not be mutated");
        }

        [Test] public void Mapper_ToGeneratedLevel_DeepCopy()
        {
            var d = MakeDraft(1); d.PlaceEndpoint(0, true, new FlowPos(0, 0)); d.PlaceEndpoint(0, false, new FlowPos(2, 0));
            d.currentSolution = new FlowSolutionData { levelId = 1001 }; d.currentDifficulty = new FlowDifficultyReport { totalScore = 50f }; d.isValidated = true; d.isSolutionDirty = false;
            var result = FlowDraftMapper.ToGeneratedLevel(d);
            result.solutionData.levelId = 999; // mutate returned level
            Assert.AreEqual(1001, d.currentSolution.levelId, "Draft must not be affected by returned level mutation");
        }

        [Test] public void Mapper_ToGeneratedLevel_IncompleteEndpoints_Throws()
        {
            var d = MakeDraft(1); d.currentSolution = new FlowSolutionData();
            Assert.Throws<InvalidOperationException>(() => FlowDraftMapper.ToGeneratedLevel(d));
        }

        [Test] public void Mapper_ToGeneratedLevel_DirtySolution_Throws()
        {
            var d = MakeDraft(1); d.PlaceEndpoint(0, true, new FlowPos(0, 0)); d.PlaceEndpoint(0, false, new FlowPos(2, 0));
            d.currentSolution = new FlowSolutionData(); d.isSolutionDirty = true;
            Assert.Throws<InvalidOperationException>(() => FlowDraftMapper.ToGeneratedLevel(d));
        }
    }
}
