namespace FlowPuzzle.Editor.Commands
{
    public interface IFlowEditorCommand
    {
        string DisplayName { get; }
        bool Execute();
        bool Undo();
    }
}
