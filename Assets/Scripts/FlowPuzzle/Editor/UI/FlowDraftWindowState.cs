using System;
using FlowPuzzle.Editor.Draft;

namespace FlowPuzzle.Editor.UI
{
    [Serializable]
    public sealed class FlowDraftWindowState
    {
        public int selectedColorId;
        public FlowDraftEditTool selectedTool;
        public bool isEndpointA = true;
        public string draftJson; // serialized copy via JsonUtility of Draft-like snapshot
        public string loadedAssetGuid;
    }
}
