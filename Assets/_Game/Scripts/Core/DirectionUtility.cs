namespace Shift.Game
{
    public static class DirectionUtility
    {
        public static GridPosition ToOffset(Direction direction) => direction switch
        {
            Direction.Up => new GridPosition(0, 1),
            Direction.Down => new GridPosition(0, -1),
            Direction.Left => new GridPosition(-1, 0),
            Direction.Right => new GridPosition(1, 0),
            _ => new GridPosition(0, 0)
        };

        public static Direction RotateClockwise(Direction direction) => direction switch
        {
            Direction.Up => Direction.Right,
            Direction.Right => Direction.Down,
            Direction.Down => Direction.Left,
            Direction.Left => Direction.Up,
            _ => Direction.None
        };
    }
}
