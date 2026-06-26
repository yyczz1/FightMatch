using FlowPuzzle.Core;
using UnityEngine.UIElements;

namespace FlowPuzzle.Editor.UI
{
    public sealed class FlowDiagnosticsPanel
    {
        public HelpBox helpBox;

        public void Build(VisualElement root)
        {
            helpBox = new HelpBox("", HelpBoxMessageType.Info) { visible = false };
            root.Add(helpBox);
        }

        public void ShowError(string message)
        {
            helpBox.messageType = HelpBoxMessageType.Error;
            helpBox.text = message;
            helpBox.visible = true;
        }

        public void ShowInfo(string message)
        {
            helpBox.messageType = HelpBoxMessageType.Info;
            helpBox.text = message;
            helpBox.visible = true;
        }

        public void Clear()
        {
            helpBox.text = "";
            helpBox.visible = false;
        }
    }
}
