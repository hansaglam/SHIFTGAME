using System;

namespace Shift.Game
{
    [Serializable]
    public struct GridPosition : IEquatable<GridPosition>
    {
        public int x;
        public int y;
        public GridPosition(int x, int y) { this.x = x; this.y = y; }
        public bool Equals(GridPosition other) => x == other.x && y == other.y;
        public override bool Equals(object obj) => obj is GridPosition other && Equals(other);
        public override int GetHashCode() => unchecked((x * 397) ^ y);
        public static GridPosition operator +(GridPosition a, GridPosition b) => new GridPosition(a.x + b.x, a.y + b.y);
        public static bool operator ==(GridPosition a, GridPosition b) => a.Equals(b);
        public static bool operator !=(GridPosition a, GridPosition b) => !a.Equals(b);
        public override string ToString() => $"({x}, {y})";
    }
}
