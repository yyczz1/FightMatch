using System.Collections.Generic;
using System.Linq;
using FlowPuzzle.Core;
using FlowPuzzle.Editor.Commands;
using FlowPuzzle.Editor.Draft;
using NUnit.Framework;

namespace FlowPuzzle.Tests.Editor
{
    [TestFixture]
    public class FlowConstraintEditingTests
    {
        private static FlowLevelDraft MakeDraft(int w = 5, int h = 5, int colors = 2)
        {
            var d = new FlowLevelDraft { width = w, height = h, colorCount = colors, levelId = 1, seed = 42 };
            for (int i = 0; i < colors; i++) d.pairs.Add(new FlowDraftPairData { colorId = i });
            return d;
        }

        [Test] public void ValidChain_AnchoredAtEndpoint_AndHasExtraCell_Accepted()
        {
            var d = MakeDraft();
            d.PlaceEndpoint(0, true, new FlowPos(0, 0));
            var cells = new List<FlowPos> { new(0, 0), new(1, 0), new(2, 0) };
            var r = d.ApplyConstraint(0, cells);
            Assert.IsTrue(r.success); Assert.AreEqual(1, d.fixedConstraints.Count);
        }

        [Test] public void Chain_NotAnchored_Rejected()
        {
            var d = MakeDraft(); d.PlaceEndpoint(0, true, new FlowPos(0, 0));
            var cells = new List<FlowPos> { new(2, 2), new(2, 3) };
            var r = d.ApplyConstraint(0, cells);
            Assert.IsFalse(r.success); Assert.AreEqual("ConstraintNotAnchored", r.errorCode);
            Assert.AreEqual(0, d.fixedConstraints.Count);
        }

        [Test] public void Chain_ReachesSecondEndpoint_Rejected()
        {
            var d = MakeDraft();
            d.PlaceEndpoint(0, true, new FlowPos(0, 0)); d.PlaceEndpoint(0, false, new FlowPos(3, 0));
            var cells = new List<FlowPos> { new(0, 0), new(1, 0), new(2, 0), new(3, 0) };
            var r = d.ApplyConstraint(0, cells);
            Assert.IsFalse(r.success); Assert.AreEqual("CompleteEndpointToEndpoint", r.errorCode);
        }

        [Test] public void Chain_DuplicateCell_Rejected()
        {
            var d = MakeDraft(); d.PlaceEndpoint(0, true, new FlowPos(0, 0));
            var cells = new List<FlowPos> { new(0, 0), new(1, 0), new(1, 1), new(1, 0) };
            var r = d.ApplyConstraint(0, cells);
            Assert.IsFalse(r.success); Assert.AreEqual("DuplicateCell", r.errorCode);
        }

        [Test] public void Chain_NonContiguous_Rejected()
        {
            var d = MakeDraft(); d.PlaceEndpoint(0, true, new FlowPos(0, 0));
            var cells = new List<FlowPos> { new(0, 0), new(1, 0), new(3, 0) };
            var r = d.ApplyConstraint(0, cells);
            Assert.IsFalse(r.success); Assert.AreEqual("InvalidAdjacency", r.errorCode);
        }

        [Test] public void Chain_CrossesForeignEndpoint_Rejected()
        {
            var d = MakeDraft();
            d.PlaceEndpoint(0, true, new FlowPos(0, 0)); d.PlaceEndpoint(1, true, new FlowPos(1, 0));
            var cells = new List<FlowPos> { new(0, 0), new(1, 0), new(2, 0) };
            var r = d.ApplyConstraint(0, cells);
            Assert.IsFalse(r.success); Assert.AreEqual("ForeignEndpointTraversal", r.errorCode);
        }

        [Test] public void Erase_TrimsChain_DoesNotFloat()
        {
            var d = MakeDraft(); d.PlaceEndpoint(0, true, new FlowPos(0, 0));
            d.ApplyConstraint(0, new List<FlowPos> { new(0, 0), new(1, 0), new(2, 0), new(3, 0) });
            var r = d.EraseConstraint(0, 2); // erase from index 2 (cell (2,0))
            Assert.IsTrue(r.success);
            Assert.AreEqual(2, d.fixedConstraints[0].cells.Count);
            Assert.AreEqual(new FlowPos(1, 0), d.fixedConstraints[0].cells.Last());
        }

        [Test] public void Erase_AtAnchor_ClearsChain()
        {
            var d = MakeDraft(); d.PlaceEndpoint(0, true, new FlowPos(0, 0));
            d.ApplyConstraint(0, new List<FlowPos> { new(0, 0), new(1, 0) });
            d.EraseConstraint(0, 0);
            Assert.AreEqual(0, d.fixedConstraints.Count);
        }

        [Test] public void StrokeCommand_DrawAndUndo_WholeStroke()
        {
            var d = MakeDraft(); d.PlaceEndpoint(0, true, new FlowPos(0, 0));
            var cmd = new DrawConstraintStrokeCommand(d, 0,
                new List<FlowPos> { new(0, 0), new(1, 0), new(2, 0) }, false);
            Assert.IsTrue(cmd.Execute()); Assert.AreEqual(1, d.fixedConstraints.Count);
            Assert.IsTrue(cmd.Undo()); Assert.AreEqual(0, d.fixedConstraints.Count);
        }

        [Test] public void StrokeCommand_InvalidStroke_ReturnsFalse_DoesNotMutate()
        {
            var d = MakeDraft(); d.PlaceEndpoint(0, true, new FlowPos(0, 0));
            var snap = d.Clone();
            var cmd = new DrawConstraintStrokeCommand(d, 0,
                new List<FlowPos> { new(1, 1), new(2, 1) }, false); // not anchored
            Assert.IsFalse(cmd.Execute());
            Assert.AreEqual(snap.fixedConstraints.Count, d.fixedConstraints.Count);
        }
    }
}
