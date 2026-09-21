using System;
using System.Collections.Generic;

namespace Shift.Game
{
    // Authoritative model: no Transform, animation, input, or scene dependencies.
    public sealed class BoardManager
    {
        private const int ReactionLimit = 256;
        private const int RecursionLimit = 128;
        private readonly List<BoardAction> actions = new List<BoardAction>(ReactionLimit);
        private readonly IReadOnlyList<BoardAction> readOnlyActions;
        private BoardPiece[] pieces;
        private IReadOnlyList<BoardPiece> readOnlyPieces;
        private BoardPiece[,] occupants;
        private BoardPiece[,] terrain;
        private Snapshot[] snapshots;
        private bool[] moving;
        private PieceColor targetColor;
        private bool overflow;
        private int operations;
        private struct Snapshot { public GridPosition Position; public Direction Direction; public bool Active; public bool GateOpen; }

        public int Width { get; private set; }
        public int Height { get; private set; }
        public int MovesRemaining { get; private set; }
        public GameState State { get; private set; } = GameState.Loading;
        public IReadOnlyList<BoardPiece> Pieces => readOnlyPieces;
        public IReadOnlyList<BoardAction> Actions => readOnlyActions;
        public int ReactionDepth => actions.Count;
        public bool LastReactionWasCancelled { get; private set; }

        public BoardManager() { readOnlyActions = actions.AsReadOnly(); }

        public void Load(LevelData level)
        {
            if (level == null) throw new ArgumentNullException(nameof(level));
            if (!level.Validate(out string error)) throw new ArgumentException(error, nameof(level));
            State = GameState.Loading;
            Width = level.Width; Height = level.Height; MovesRemaining = level.MoveLimit; targetColor = level.TargetColor;
            occupants = new BoardPiece[Width, Height]; terrain = new BoardPiece[Width, Height];
            pieces = new BoardPiece[level.Placements.Count]; snapshots = new Snapshot[pieces.Length];
            moving = new bool[pieces.Length];
            readOnlyPieces = Array.AsReadOnly(pieces);
            for (int i = 0; i < pieces.Length; i++)
            {
                var piece = new BoardPiece(i, level.Placements[i]); pieces[i] = piece;
                (piece.Movable ? occupants : terrain)[piece.Position.x, piece.Position.y] = piece;
            }
            actions.Clear(); LastReactionWasCancelled = false; State = GameState.Playing;
        }

        public bool IsValid(GridPosition p) => p.x >= 0 && p.y >= 0 && p.x < Width && p.y < Height;
        public BoardPiece GetOccupant(GridPosition p) => IsValid(p) && occupants != null ? occupants[p.x, p.y] : null;
        public BoardPiece GetTerrain(GridPosition p) => IsValid(p) && terrain != null ? terrain[p.x, p.y] : null;

        public bool RequestMove(GridPosition position)
        {
            if (State != GameState.Playing) return false;
            var piece = GetOccupant(position);
            if (piece == null || piece.Type != PieceType.Normal) return false;
            State = GameState.Resolving;
            actions.Clear(); overflow = false; operations = 0; LastReactionWasCancelled = false;
            Array.Clear(moving, 0, moving.Length);
            for (int i = 0; i < pieces.Length; i++)
                snapshots[i] = new Snapshot { Position = pieces[i].Position, Direction = pieces[i].Direction, Active = pieces[i].Active, GateOpen = pieces[i].GateOpen };
            Move(piece, piece.Direction, 0);
            if (overflow)
            {
                // Cancel the entire transaction on a cyclic/oversized reaction, never leave a partial board.
                Array.Clear(occupants, 0, occupants.Length);
                for (int i = 0; i < pieces.Length; i++)
                {
                    var p = pieces[i]; p.Position = snapshots[i].Position; p.Direction = snapshots[i].Direction; p.Active = snapshots[i].Active; p.GateOpen = snapshots[i].GateOpen;
                    if (p.Active && p.Movable) occupants[p.Position.x, p.Position.y] = p;
                }
                actions.Clear(); LastReactionWasCancelled = true;
            }
            // Accepted taps stay Resolving for feedback, but only committed reactions spend a move.
            if (actions.Count > 0) MovesRemaining--;
            return true;
        }

        public void CompleteResolution()
        {
            if (State != GameState.Resolving) return;
            bool remaining = false;
            foreach (var p in pieces)
                if (p.Active && p.Type == PieceType.Normal && p.Color == targetColor) { remaining = true; break; }
            State = !remaining ? GameState.Won : MovesRemaining == 0 ? GameState.Lost : GameState.Playing;
        }

        private bool Move(BoardPiece piece, Direction direction, int depth)
        {
            if (overflow) return false;
            if (++operations > ReactionLimit || depth >= RecursionLimit) { overflow = true; return false; }
            var next = piece.Position + DirectionUtility.ToOffset(direction);
            if (direction == Direction.None || !IsValid(next)) return false;
            var tile = GetTerrain(next);
            if (tile != null && (tile.Type == PieceType.Wall || (tile.Type == PieceType.Gate && !tile.GateOpen) ||
                (tile.Type == PieceType.Exit && (piece.Type != PieceType.Normal ||
                (tile.Color != PieceColor.None && tile.Color != piece.Color))))) return false;
            var other = GetOccupant(next);
            if (other != null)
            {
                if (moving[other.Id]) { overflow = true; return false; }
                moving[piece.Id] = true;
                bool pushed = Move(other, direction, depth + 1);
                moving[piece.Id] = false;
                if (!pushed) return false;
            }
            if (overflow) return false;
            // A redirected pushed piece may have returned to the cell we need.
            if (GetOccupant(next) != null) { overflow = true; return false; }
            // A pushed piece may toggle the destination gate closed. Roll back the
            // whole reaction rather than retain a partial push through a newly closed gate.
            if (tile != null && tile.Type == PieceType.Gate && !tile.GateOpen) { overflow = true; return false; }
            var previous = piece.Position;
            occupants[previous.x, previous.y] = null;
            if (piece.Direction != direction)
            {
                piece.Direction = direction;
                Record(BoardActionType.Turn, piece, previous, previous);
            }
            piece.Position = next; occupants[next.x, next.y] = piece;
            Record(BoardActionType.Move, piece, previous, next);
            if (tile == null) return true;
            if (tile.Type == PieceType.Exit)
            {
                occupants[next.x, next.y] = null; piece.Active = false;
                Record(BoardActionType.Deliver, piece, next, next);
            }
            else if (tile.Type == PieceType.Switch && piece.Type == PieceType.Normal)
            {
                Record(BoardActionType.SwitchActivated, tile, next, next);
                foreach (var gate in pieces)
                {
                    if (gate.Type != PieceType.Gate || gate.Channel != tile.Channel) continue;
                    gate.GateOpen = !gate.GateOpen;
                    Record(gate.GateOpen ? BoardActionType.GateOpened : BoardActionType.GateClosed, gate, gate.Position, gate.Position);
                }
            }
            else if (tile.Type == PieceType.Direction || tile.Type == PieceType.Rotator)
            {
                piece.Direction = tile.Type == PieceType.Direction ? tile.Direction : DirectionUtility.RotateClockwise(direction);
                Record(BoardActionType.Turn, piece, next, next);
                Move(piece, piece.Direction, depth + 1);
            }
            return true;
        }

        private void Record(BoardActionType type, BoardPiece piece, GridPosition from, GridPosition to)
        {
            if (actions.Count >= ReactionLimit) { overflow = true; return; }
            actions.Add(new BoardAction(type, piece.Id, from, to, piece.Direction));
        }
    }
}
