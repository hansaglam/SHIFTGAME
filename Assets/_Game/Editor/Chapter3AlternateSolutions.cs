namespace Shift.Game.Editor
{
    // Editor-only test data. These longer routes are not rewards or player-facing hints.
    public static class Chapter3AlternateSolutions
    {
        public static GridPosition[] Get(int number)
        {
            switch (number)
            {
                case 51: return new[] { new GridPosition(2, 3), new GridPosition(5, 2), new GridPosition(2, 2), new GridPosition(3, 3), new GridPosition(3, 2), new GridPosition(2, 1), new GridPosition(4, 1), new GridPosition(1, 1) };
                case 52: return new[] { new GridPosition(1, 1), new GridPosition(0, 2), new GridPosition(1, 3), new GridPosition(2, 3), new GridPosition(2, 4), new GridPosition(4, 2), new GridPosition(3, 4), new GridPosition(3, 3) };
                case 53: return new[] { new GridPosition(1, 1), new GridPosition(2, 4), new GridPosition(4, 2), new GridPosition(3, 4), new GridPosition(0, 4), new GridPosition(1, 3), new GridPosition(3, 3), new GridPosition(4, 3) };
                case 54: return new[] { new GridPosition(2, 0), new GridPosition(2, 1), new GridPosition(2, 2), new GridPosition(1, 2), new GridPosition(2, 2), new GridPosition(3, 2), new GridPosition(1, 1), new GridPosition(4, 1) };
                case 55: return new[] { new GridPosition(1, 2), new GridPosition(2, 2), new GridPosition(2, 4), new GridPosition(3, 3), new GridPosition(2, 1), new GridPosition(2, 2), new GridPosition(4, 3), new GridPosition(3, 3), new GridPosition(4, 1) };
                case 56: return new[] { new GridPosition(2, 4), new GridPosition(2, 1), new GridPosition(3, 3), new GridPosition(2, 2), new GridPosition(3, 2), new GridPosition(1, 2), new GridPosition(4, 3), new GridPosition(3, 3), new GridPosition(2, 2) };
                case 57: return new[] { new GridPosition(3, 3), new GridPosition(2, 3), new GridPosition(2, 2), new GridPosition(5, 0), new GridPosition(4, 0), new GridPosition(3, 0), new GridPosition(3, 2), new GridPosition(2, 1), new GridPosition(2, 0), new GridPosition(1, 1) };
                case 58: return new[] { new GridPosition(2, 2), new GridPosition(3, 2), new GridPosition(2, 2), new GridPosition(1, 2), new GridPosition(3, 4), new GridPosition(1, 2), new GridPosition(2, 2), new GridPosition(0, 3), new GridPosition(1, 1), new GridPosition(2, 1) };
                case 59: return new[] { new GridPosition(2, 0), new GridPosition(2, 1), new GridPosition(4, 2), new GridPosition(4, 0), new GridPosition(3, 3), new GridPosition(3, 1), new GridPosition(1, 3), new GridPosition(1, 2), new GridPosition(2, 2), new GridPosition(2, 3), new GridPosition(1, 1) };
                case 60: return new[] { new GridPosition(2, 4), new GridPosition(3, 3), new GridPosition(4, 2), new GridPosition(4, 1), new GridPosition(2, 1), new GridPosition(1, 2), new GridPosition(4, 3), new GridPosition(3, 3), new GridPosition(2, 2), new GridPosition(3, 3), new GridPosition(4, 2), new GridPosition(4, 3) };
                default: return null;
            }
        }
    }
}
