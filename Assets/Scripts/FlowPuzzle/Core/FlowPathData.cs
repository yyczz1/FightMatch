using System;
using System.Collections.Generic;

namespace FlowPuzzle.Core
{
    [Serializable]
    public sealed class FlowPathData
    {
        public int colorId;
        public List<FlowPos> cells = new List<FlowPos>();
    }
}
