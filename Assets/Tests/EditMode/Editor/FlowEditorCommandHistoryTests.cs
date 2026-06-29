using FlowPuzzle.Core;
using FlowPuzzle.Editor.Draft;
using FlowPuzzle.Editor.Commands;
using NUnit.Framework;

namespace FlowPuzzle.Tests.Editor
{
    [TestFixture]
    public class FlowEditorCommandHistoryTests
    {
        private static FlowLevelDraft MakeDraft(int colors = 2)
        {
            var d = new FlowLevelDraft { width = 5, height = 5, colorCount = colors, levelId = 1, seed = 42 };
            for (int i = 0; i < colors; i++) d.pairs.Add(new FlowDraftPairData { colorId = i });
            return d;
        }

        [Test] public void Execute_Undo_Redo_RoundTrip()
        {
            var draft = MakeDraft(); var history = new FlowEditorCommandHistory();
            var cmd = MoveEndpointCommand.Place(draft, 0, true, new FlowPos(1, 2));
            Assert.IsTrue(history.Execute(cmd)); Assert.AreEqual(1, draft.pairs[0].endpointA.Value.x);
            Assert.IsTrue(history.CanUndo);
            Assert.IsTrue(history.Undo()); Assert.IsNull(draft.pairs[0].endpointA);
            Assert.IsTrue(history.CanRedo);
            Assert.IsTrue(history.Redo()); Assert.AreEqual(1, draft.pairs[0].endpointA.Value.x);
        }

        [Test] public void EmptyHistory_CannotUndoRedo()
        {
            var h = new FlowEditorCommandHistory();
            Assert.IsFalse(h.CanUndo); Assert.IsFalse(h.CanRedo);
            Assert.IsFalse(h.Undo()); Assert.IsFalse(h.Redo());
        }

        [Test] public void NewCommandAfterUndo_ClearsRedo()
        {
            var draft = MakeDraft(); var h = new FlowEditorCommandHistory();
            h.Execute(MoveEndpointCommand.Place(draft, 0, true, new FlowPos(1, 2)));
            h.Execute(MoveEndpointCommand.Place(draft, 0, false, new FlowPos(3, 4)));
            h.Undo();
            Assert.IsNull(draft.pairs[0].endpointB);
            // New command after undo
            h.Execute(MoveEndpointCommand.Place(draft, 0, false, new FlowPos(2, 2)));
            Assert.IsFalse(h.CanRedo, "Redo should be cleared after new command following undo");
        }

        [Test] public void FailedCommand_DoesNotEnterHistory()
        {
            var draft = MakeDraft(); var h = new FlowEditorCommandHistory();
            var cmd = MoveEndpointCommand.Move(draft, 99, true, new FlowPos(5, 5));
            Assert.IsFalse(h.Execute(cmd)); Assert.IsFalse(h.CanUndo);
        }

        [Test] public void MoveEndpoint_MoveAndRemove_RoundTrip()
        {
            var draft = MakeDraft(); var h = new FlowEditorCommandHistory();
            h.Execute(MoveEndpointCommand.Place(draft, 0, true, new FlowPos(0, 0)));
            h.Execute(MoveEndpointCommand.Move(draft, 0, true, new FlowPos(2, 2)));
            Assert.AreEqual(2, draft.pairs[0].endpointA.Value.x);
            h.Undo(); Assert.AreEqual(0, draft.pairs[0].endpointA.Value.x);
            h.Execute(MoveEndpointCommand.Remove(draft, 0, true));
            Assert.IsNull(draft.pairs[0].endpointA);
            h.Undo(); Assert.AreEqual(0, draft.pairs[0].endpointA.Value.x);
        }

        [Test] public void ResizeBoard_RoundTrip()
        {
            var draft = MakeDraft(1);
            draft.PlaceEndpoint(0, true, new FlowPos(0, 0));
            draft.PlaceEndpoint(0, false, new FlowPos(4, 0));
            var h = new FlowEditorCommandHistory();
            var cmd = new ResizeBoardCommand(draft, 3, 3);
            Assert.IsTrue(h.Execute(cmd));
            Assert.AreEqual(3, draft.width); Assert.IsNull(draft.pairs[0].endpointB);
            Assert.IsTrue(draft.isSolutionDirty);
            Assert.IsTrue(h.Undo());
            Assert.AreEqual(5, draft.width); Assert.AreEqual(4, draft.pairs[0].endpointB.Value.x);
        }

        [Test] public void SnapshotCommand_RestoresFullState()
        {
            var draft = MakeDraft(1);
            draft.PlaceEndpoint(0, true, new FlowPos(1, 1));
            var before = draft.Clone();
            var after = draft.Clone(); after.PlaceEndpoint(0, false, new FlowPos(3, 3));
            var h = new FlowEditorCommandHistory();
            var cmd = new FlowSnapshotCommand(draft, before, after, "Test");
            Assert.IsTrue(h.Execute(cmd)); Assert.AreEqual(3, draft.pairs[0].endpointB.Value.y);
            Assert.IsTrue(h.Undo()); Assert.IsNull(draft.pairs[0].endpointB);
        }
    }
}
