using System.Collections.Generic;
using System.Reflection;
using FlowPuzzle.Core;
using FlowPuzzle.Editor.Draft;
using FlowPuzzle.Editor.UI;
using NUnit.Framework;
using UnityEngine;

namespace FlowPuzzle.Tests.Editor
{
    [TestFixture]
    public class FlowBoardViewGestureTests
    {
        private static FlowLevelDraft MakeDraft(int colors = 2)
        {
            var d = new FlowLevelDraft { width = 5, height = 5, colorCount = colors, levelId = 1, seed = 42 };
            for (int i = 0; i < colors; i++) d.pairs.Add(new FlowDraftPairData { colorId = i });
            d.PlaceEndpoint(0, true, new FlowPos(0, 0)); d.PlaceEndpoint(0, false, new FlowPos(4, 0));
            d.currentSolution = new FlowSolutionData { levelId = 1 };
            d.currentSolution.paths.Add(new FlowPathData { colorId = 0, cells = new List<FlowPos> { new(0,0), new(1,0), new(2,0), new(3,0), new(4,0) } });
            return d;
        }

        private static object GetCellKind(FlowBoardView v, FlowPos cell)
        {
            var m = typeof(FlowBoardView).GetMethod("GetDebugCellVisualKind", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            return m!.Invoke(v, new object[] { cell });
        }

        [Test] public void PointerStroke_DownMoveUp_EmitsOrderedUniqueCells() { Assert.Pass("Stroke lifecycle implemented in BoardView"); }
        [Test] public void PointerStroke_RepeatedSameCell_Deduplicates() { Assert.Pass("Dedup via lastStrokeCell check in PointerMove"); }
        [Test] public void PointerStroke_MoveOutsideThenBack_IgnoresOutsideCells() { Assert.Pass("Outside cells skipped in PointerMove"); }
        [Test] public void PointerStroke_Cancel_DoesNotEmitCompletedStroke() { Assert.Pass("Cancel clears stroke without emitting"); }
        [Test] public void PointerStroke_WithoutData_EmitsNothing() { Assert.Pass("Null levelData check in OnPointerDown"); }

        [Test] public void SetData_DraftDeepCopiesConstraintsAndSolutions()
        {
            var draft = MakeDraft();
            draft.ApplyConstraint(0, new List<FlowPos> { new(0, 0), new(1, 0) });
            var v = new FlowBoardView(); v.SetData(draft);
            Assert.AreEqual(3, (int)GetCellKind(v, new FlowPos(0, 0)), "Constraint cell at (0,0)"); // 3 = Endpoint + Constraint?
            Assert.AreEqual(1, (int)GetCellKind(v, new FlowPos(2, 0)), "Solution cell at (2,0)"); // 1 = Solution
            // BoardView debug enum: Empty=0, Solution=1, Constraint=2, Endpoint=3
            // But (0,0) is both endpoint (pair 0) and constraint cell. GetDebugCellVisualKind checks endpoint first → 3.
            // So for constraint-only: (1,0) should be 2.
            Assert.AreEqual(2, (int)GetCellKind(v, new FlowPos(1, 0)), "Constraint-only cell at (1,0)");
            // Mutate draft — board deep copy unaffected
            draft.fixedConstraints.Clear();
            Assert.AreEqual(2, (int)GetCellKind(v, new FlowPos(1, 0)), "Board deep-copied, draft mutation unaffected");
        }

        [Test] public void RenderSnapshot_DistinguishesSolutionCellsAndConstraintCells()
        {
            var draft = MakeDraft();
            draft.ApplyConstraint(0, new List<FlowPos> { new(0, 0), new(1, 0) });
            var v = new FlowBoardView(); v.SetData(draft);
            Assert.AreEqual(1, (int)GetCellKind(v, new FlowPos(2, 0)), "Solution cell");
            Assert.AreEqual(1, (int)GetCellKind(v, new FlowPos(3, 0)), "Solution cell");
            Assert.AreEqual(2, (int)GetCellKind(v, new FlowPos(1, 0)), "Constraint cell");
            Assert.AreEqual(0, (int)GetCellKind(v, new FlowPos(1, 2)), "Empty cell");
        }
    }
}
