namespace Shift.Game
{
    public enum BoardActionType { Move, Turn, Deliver, SwitchActivated, GateOpened, GateClosed }

    public readonly struct BoardAction
    {
        public BoardActionType Type { get; }
        public int PieceId { get; }
        public GridPosition From { get; }
        public GridPosition To { get; }
        public Direction Direction { get; }
        public BoardAction(BoardActionType type, int pieceId, GridPosition from, GridPosition to, Direction direction)
        { Type = type; PieceId = pieceId; From = from; To = to; Direction = direction; }
    }

    public sealed class BoardPiece
    {
        public int Id { get; }
        public PieceType Type { get; }
        public PieceColor Color { get; }
        public GridPosition Position { get; internal set; }
        public Direction Direction { get; internal set; }
        public int Channel { get; }
        public bool GateOpen { get; internal set; }
        public bool Active { get; internal set; } = true;
        public bool Movable => Type == PieceType.Normal || Type == PieceType.PushBlock;
        internal BoardPiece(int id, PiecePlacement placement)
        { Id = id; Type = placement.type; Color = placement.color; Position = placement.position; Direction = placement.direction; Channel = placement.channel; GateOpen = placement.initialOpen; }
    }
}
