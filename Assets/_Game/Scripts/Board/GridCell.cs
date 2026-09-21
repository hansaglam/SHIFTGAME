using UnityEngine;

namespace Shift.Game
{
    public sealed class GridCell : MonoBehaviour
    {
        public GridPosition Position { get; private set; }
        public void Initialize(GridPosition position) { Position = position; }
    }
}
