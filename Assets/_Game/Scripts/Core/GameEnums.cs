namespace Shift.Game
{
    public enum PieceType { Normal, PushBlock, Wall, Direction, Rotator, Exit, Switch, Gate }
    public enum PieceColor { None, Red, Blue, Yellow, Green }
    public enum Direction { None, Up, Down, Left, Right }
    public enum GameState { Loading, Playing, Resolving, Won, Lost }
}
