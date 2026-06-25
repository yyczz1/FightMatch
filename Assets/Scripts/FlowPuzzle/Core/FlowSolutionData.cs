using System;
using System.Collections.Generic;

namespace FlowPuzzle.Core
{
    [Serializable]
    public sealed class FlowSolutionData
    {
        public int levelId;
        public List<FlowPathData> paths = new List<FlowPathData>();
    }
}
