using System.Collections.Generic;

namespace FlowPuzzle.Editor.Commands
{
    public sealed class FlowEditorCommandHistory
    {
        private readonly List<IFlowEditorCommand> undoStack = new List<IFlowEditorCommand>();
        private int currentIndex = -1;

        public bool CanUndo => currentIndex >= 0;
        public bool CanRedo => currentIndex < undoStack.Count - 1;

        public bool Execute(IFlowEditorCommand command)
        {
            if (command == null) return false;

            if (!command.Execute())
                return false;

            // Truncate redo stack after current position
            while (undoStack.Count > currentIndex + 1)
                undoStack.RemoveAt(undoStack.Count - 1);

            undoStack.Add(command);
            currentIndex = undoStack.Count - 1;
            return true;
        }

        public bool Undo()
        {
            if (!CanUndo) return false;
            var ok = undoStack[currentIndex].Undo();
            if (ok) currentIndex--;
            return ok;
        }

        public bool Redo()
        {
            if (!CanRedo) return false;
            var ok = undoStack[currentIndex + 1].Execute();
            if (ok) currentIndex++;
            return ok;
        }

        public void Clear()
        {
            undoStack.Clear();
            currentIndex = -1;
        }
    }
}
