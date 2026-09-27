using System;
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
        private static FlowLevelDraft MakeDraft()
        {
            var d = new FlowLevelDraft { width = 5, height = 5, colorCount = 1, levelId = 1, seed = 42 };
            d.pairs.Add(new FlowDraftPairData { colorId = 0 });
            d.PlaceEndpoint(0, true, new FlowPos(0, 0)); d.PlaceEndpoint(0, false, new FlowPos(4, 0));
            d.currentSolution = new FlowSolutionData { levelId = 1 };
            d.currentSolution.paths.Add(new FlowPathData { colorId = 0, cells = new List<FlowPos> { new(0,0), new(1,0), new(2,0), new(3,0), new(4,0) } });
            d.isSolutionDirty = false;
            d.isValidated = true;
            return d;
        }

        private static FlowBoardView MakeView() { var v = new FlowBoardView(); v.GetType().GetField("debugContentRect", BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public)!.SetValue(v, new Rect(0, 0, 200, 200)); v.SetData(MakeDraft()); return v; }
        private static void DP(FlowBoardView v, Vector2 pos) => v.GetType().GetMethod("DoPointerDown", BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public)!.Invoke(v, new object[]{pos, 1});
        private static void DM(FlowBoardView v, Vector2 pos) => v.GetType().GetMethod("DoPointerMove", BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public)!.Invoke(v, new object[]{pos});
        private static void DU(FlowBoardView v, Vector2 pos) => v.GetType().GetMethod("DoPointerUp", BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public)!.Invoke(v, new object[]{pos, 1});
        private static void DC(FlowBoardView v) => v.GetType().GetMethod("DoPointerCancel", BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public)!.Invoke(v, null);
        // 200px / 5 cols = 40px per cell. Cell(2,0) = center at x=100
        private static FlowPos C(int x, int y) => new FlowPos(x, y);

        [Test] public void PointerStroke_DownMoveUp_EmitsOrderedUniqueCells()
        {
            var v = MakeView(); var cells = new List<FlowPos>(); v.CellStrokeCompleted += c => cells.AddRange(c);
            DP(v, new Vector2(23, 177)); DM(v, new Vector2(62, 177)); DM(v, new Vector2(100, 177)); DU(v, new Vector2(100, 177));
            Assert.AreEqual(3, cells.Count); Assert.AreEqual(C(0,0), cells[0]); Assert.AreEqual(C(1,0), cells[1]); Assert.AreEqual(C(2,0), cells[2]);
        }

        [Test] public void PointerStroke_RepeatedSameCell_Deduplicates()
        {
            var v = MakeView(); var cells = new List<FlowPos>(); v.CellStrokeCompleted += c => cells.AddRange(c);
            DP(v, new Vector2(23, 177)); DM(v, new Vector2(24, 177)); DM(v, new Vector2(26, 177)); DM(v, new Vector2(62, 177)); DU(v, new Vector2(62, 177));
            Assert.AreEqual(2, cells.Count); Assert.AreEqual(C(0,0), cells[0]); Assert.AreEqual(C(1,0), cells[1]);
        }

        [Test] public void PointerStroke_MoveOutsideThenBack_IgnoresOutsideCells()
        {
            var v = MakeView(); var cells = new List<FlowPos>(); v.CellStrokeCompleted += c => cells.AddRange(c);
            DP(v, new Vector2(23, 177)); DM(v, new Vector2(62, 177)); DM(v, new Vector2(-999, -999)); DM(v, new Vector2(100, 177)); DU(v, new Vector2(100, 177));
            Assert.AreEqual(3, cells.Count); Assert.AreEqual(C(0,0), cells[0]); Assert.AreEqual(C(1,0), cells[1]); Assert.AreEqual(C(2,0), cells[2]);
        }

        [Test] public void PointerStroke_Cancel_DoesNotEmitCompletedStroke()
        {
            var v = MakeView(); bool stroke = false, select = false; v.CellStrokeCompleted += _ => stroke = true; v.CellSelected += _ => select = true;
            DP(v, new Vector2(23, 177)); DM(v, new Vector2(62, 177)); DC(v);
            Assert.IsFalse(stroke); Assert.IsFalse(select);
        }

        [Test] public void PointerStroke_WithoutData_EmitsNothing()
        {
            var v = new FlowBoardView(); v.GetType().GetField("debugContentRect", BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public)!.SetValue(v, new Rect(0,0,200,200)); bool s = false, c = false; v.CellStrokeCompleted += _ => s = true; v.CellSelected += _ => c = true;
            DP(v, new Vector2(23, 177)); DU(v, new Vector2(23, 177));
            Assert.IsFalse(s); Assert.IsFalse(c);
        }

        [Test] public void SetData_DraftDeepCopiesConstraintsAndSolutions()
        {
            var draft = MakeDraft(); draft.ApplyConstraint(0, new List<FlowPos> { new(0,0), new(1,0), new(2,0) });
            draft.isSolutionDirty = false; draft.isValidated = true;
            var v = new FlowBoardView(); v.SetData(draft);
            var gk = typeof(FlowBoardView).GetMethod("GetDebugCellVisualKind", BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public);
            Assert.AreEqual(3, (int)gk!.Invoke(v, new object[]{C(0,0)}), "Endpoint"); Assert.AreEqual(2, (int)gk.Invoke(v, new object[]{C(1,0)}), "Constraint");
            Assert.AreEqual(1, (int)gk.Invoke(v, new object[]{C(3,0)}), "Solution");
            draft.fixedConstraints.Clear(); Assert.AreEqual(2, (int)gk.Invoke(v, new object[]{C(1,0)}), "Board unchanged after draft mutation");
        }

        [Test] public void RenderSnapshot_DistinguishesSolutionCellsAndConstraintCells()
        {
            var draft = MakeDraft(); draft.ApplyConstraint(0, new List<FlowPos> { new(0,0), new(1,0) });
            draft.isSolutionDirty = false; draft.isValidated = true;
            var v = new FlowBoardView(); v.SetData(draft);
            var gk = typeof(FlowBoardView).GetMethod("GetDebugCellVisualKind", BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public);
            Assert.AreEqual(1, (int)gk!.Invoke(v, new object[]{C(2,0)}), "Solution"); Assert.AreEqual(2, (int)gk.Invoke(v, new object[]{C(1,0)}), "Constraint");
            Assert.AreEqual(0, (int)gk.Invoke(v, new object[]{C(1,2)}), "Empty");
        }

        [Test] public void SetData_DirtyDraft_DoesNotRenderStaleSolution()
        {
            var draft = MakeDraft();
            draft.MarkDirty();
            var v = new FlowBoardView();
            v.SetData(draft);
            var gk = typeof(FlowBoardView).GetMethod("GetDebugCellVisualKind", BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public);

            Assert.AreEqual(0, (int)gk!.Invoke(v, new object[]{C(2,0)}), "A dirty recommended solution must be hidden");
            Assert.AreEqual(3, (int)gk.Invoke(v, new object[]{C(0,0)}), "Draft endpoints remain visible");
        }

        [Test] public void SetData_DraftWithOneEndpoint_DoesNotRenderMissingEndpointAtOrigin()
        {
            var draft = new FlowLevelDraft { width = 5, height = 5, colorCount = 1, levelId = 1, seed = 42 };
            draft.pairs.Add(new FlowDraftPairData { colorId = 0, endpointB = new FlowPos(4, 4) });
            var v = new FlowBoardView(); v.SetData(draft);
            var gk = typeof(FlowBoardView).GetMethod("GetDebugCellVisualKind", BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public);
            Assert.AreEqual(0, (int)gk!.Invoke(v, new object[]{C(0,0)}), "Missing Endpoint A must not appear at default origin");
            Assert.AreEqual(3, (int)gk.Invoke(v, new object[]{C(4,4)}), "Existing Endpoint B should still render");
        }

        [Test] public void DraftEndpoints_ExposeDistinctAAndBVisualRoles()
        {
            var v = MakeView();
            var role = typeof(FlowBoardView).GetMethod("GetDebugEndpointRole", BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public);
            Assert.IsNotNull(role);
            Assert.AreEqual("A", role!.Invoke(v, new object[] { C(0, 0) }).ToString());
            Assert.AreEqual("B", role.Invoke(v, new object[] { C(4, 0) }).ToString());
            Assert.AreEqual("None", role.Invoke(v, new object[] { C(2, 2) }).ToString());
        }
    }
}
