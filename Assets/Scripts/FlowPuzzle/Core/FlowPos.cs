using System;

namespace FlowPuzzle.Core
{
    [Serializable]
    public struct FlowPos : IEquatable<FlowPos>
    {
        public int x;
        public int y;

        public FlowPos(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public bool Equals(FlowPos other)
        {
            return x == other.x && y == other.y;
        }

        public override bool Equals(object obj)
        {
            return obj is FlowPos other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (x * 397) ^ y;
            }
        }

        public static bool operator ==(FlowPos a, FlowPos b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(FlowPos a, FlowPos b)
        {
            return !a.Equals(b);
        }
    }
}
