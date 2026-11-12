using System;
using System.Collections.Generic;

namespace Shift.Game
{
    // Authoritative model: no Transform, animation, input, or scene dependencies.
    public sealed class BoardManager
    {
        private const int ReactionLimit = 256;
        private const int RecursionLimit = 128;
        // A visible pair stays linked at most three Manhattan grid steps apart.
        private const int TetherLength = 3;
        public static int TetherMaxDistance => TetherLength;
        private readonly List<BoardAction> actions = new List<BoardAction>(ReactionLimit);
        private readonly IReadOnlyList<BoardAction> readOnlyActions;
        private BoardPiece[] pieces;
        private IReadOnlyList<BoardPiece> readOnlyPieces;
        private BoardPiece[,] occupants;
        private BoardPiece[,] terrain;
        private BoardPiece[] resonancePartners;
        private BoardPiece[] tetherPartners;
        private readonly bool[] signaled = new bool[3];
        private Snapshot[] snapshots;
        private Snapshot[] undoSnapshot;
        private int undoMoves;
        public bool CanUndo => undoSnapshot != null && (State == GameState.Playing || State == GameState.Lost);
        public void ClearUndo() => undoSnapshot = null;
        public bool Undo()
        {
            if (!CanUndo) return false;
            Array.Clear(occupants, 0, occupants.Length);
            for (int i = 0; i < pieces.Length; i++)
            {
                var p = pieces[i]; var saved = undoSnapshot[i];
                p.Position = saved.Position; p.Direction = saved.Direction; p.Active = saved.Active; p.GateOpen = saved.GateOpen;
                if (p.Active && p.Movable) occupants[p.Position.x, p.Position.y] = p;
            }
            MovesRemaining = undoMoves; State = GameState.Playing;
            actions.Clear(); LastReactionWasCancelled = false; overflow = false; operations = 0;
            Array.Clear(moving, 0, moving.Length);
            Array.Clear(signaled, 0, signaled.Length);
            ClearUndo(); return true;
        }
        private bool[] moving;
        private HashSet<PieceColor> targetColors;
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
            ClearUndo();
            Width = level.Width; Height = level.Height; MovesRemaining = level.MoveLimit; targetColors = new HashSet<PieceColor>(level.ResolvedTargetColors);
            occupants = new BoardPiece[Width, Height]; terrain = new BoardPiece[Width, Height];
            pieces = new BoardPiece[level.Placements.Count]; snapshots = new Snapshot[pieces.Length];
            moving = new bool[pieces.Length];
            readOnlyPieces = Array.AsReadOnly(pieces);
            resonancePartners = new BoardPiece[pieces.Length];
            tetherPartners = new BoardPiece[pieces.Length];
            for (int i = 0; i < pieces.Length; i++)
            {
                var piece = new BoardPiece(i, level.Placements[i]); pieces[i] = piece;
                (piece.Movable ? occupants : terrain)[piece.Position.x, piece.Position.y] = piece;
            }
            for (int i = 0; i < pieces.Length; i++)
            {
                if (pieces[i].Type != PieceType.Resonance) continue;
                for (int j = 0; j < pieces.Length; j++)
                    if (i != j && pieces[j].Type == PieceType.Resonance &&
                        pieces[i].Channel == pieces[j].Channel)
                    {
                        resonancePartners[i] = pieces[j];
                        break;
                    }
            }
            // Link normal pieces by authored tetherId, not by current board
            // position. The connection persists when either endpoint moves,
            // and disappears only when an endpoint is delivered.
            for (int i = 0; i < pieces.Length; i++)
            {
                if (pieces[i].Type != PieceType.Normal || pieces[i].TetherId == 0)
                    continue;
                for (int j = 0; j < pieces.Length; j++)
                    if (j != i && pieces[j].Type == PieceType.Normal &&
                        pieces[j].TetherId == pieces[i].TetherId)
                    {
                        tetherPartners[i] = pieces[j];
                        break;
                    }
            }
            Array.Clear(signaled, 0, signaled.Length);
            actions.Clear(); LastReactionWasCancelled = false; State = GameState.Playing;
        }

        public bool IsValid(GridPosition p) => p.x >= 0 && p.y >= 0 && p.x < Width && p.y < Height;
        public BoardPiece GetOccupant(GridPosition p) => IsValid(p) && occupants != null ? occupants[p.x, p.y] : null;
        public BoardPiece GetTerrain(GridPosition p) => IsValid(p) && terrain != null ? terrain[p.x, p.y] : null;
        public BoardPiece GetTetherPartner(int id) =>
            tetherPartners != null && id >= 0 && id < tetherPartners.Length
                ? tetherPartners[id] : null;

        public bool RequestMove(GridPosition position)
        {
            if (State != GameState.Playing) return false;
            var piece = GetOccupant(position);
            if (piece == null || piece.Type != PieceType.Normal) return false;
            State = GameState.Resolving;
            actions.Clear(); overflow = false; operations = 0; LastReactionWasCancelled = false;
            Array.Clear(moving, 0, moving.Length);
            Array.Clear(signaled, 0, signaled.Length);
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
            if (actions.Count > 0)
            {
                undoSnapshot = (Snapshot[])snapshots.Clone(); undoMoves = MovesRemaining;
                MovesRemaining--;
            }
            return true;
        }

        public void CompleteResolution()
        {
            if (State != GameState.Resolving) return;
            bool remaining = false;
            foreach (var p in pieces)
                if (p.Active && p.Type == PieceType.Normal && targetColors.Contains(p.Color)) { remaining = true; break; }
            State = !remaining ? GameState.Won : MovesRemaining == 0 ? GameState.Lost : GameState.Playing;
        }

        private static int Manhattan(GridPosition a, GridPosition b) =>
            Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y);

        private static Direction Toward(GridPosition from, GridPosition to)
        {
            int dx = to.x - from.x, dy = to.y - from.y;
            // Always prefer the longer axis, X first on ties. No random
            // choice, swipe, or ambiguous diagonal pull.
            if (Math.Abs(dx) >= Math.Abs(dy))
                return dx >= 0 ? Direction.Right : Direction.Left;
            return dy >= 0 ? Direction.Up : Direction.Down;
        }

        private Snapshot[] CaptureLocal()
        {
            var current = new Snapshot[pieces.Length];
            for (int i = 0; i < pieces.Length; i++)
                current[i] = new Snapshot
                {
                    Position = pieces[i].Position,
                    Direction = pieces[i].Direction,
                    Active = pieces[i].Active,
                    GateOpen = pieces[i].GateOpen
                };
            return current;
        }

        private void RestoreLocal(Snapshot[] state, bool[] previousSignals,
            int actionCount)
        {
            Array.Clear(occupants, 0, occupants.Length);
            for (int i = 0; i < pieces.Length; i++)
            {
                var p = pieces[i];
                p.Position = state[i].Position;
                p.Direction = state[i].Direction;
                p.Active = state[i].Active;
                p.GateOpen = state[i].GateOpen;
                if (p.Active && p.Movable)
                    occupants[p.Position.x, p.Position.y] = p;
            }
            Array.Copy(previousSignals, signaled, signaled.Length);
            actions.RemoveRange(actionCount, actions.Count - actionCount);
        }

        private bool Move(BoardPiece piece, Direction direction, int depth,
            bool forcedTetherFollow = false)
        {
            if (overflow) return false;
            if (++operations > ReactionLimit || depth >= RecursionLimit) { overflow = true; return false; }
            var next = piece.Position + DirectionUtility.ToOffset(direction);
            if (direction == Direction.None || !IsValid(next)) return false;
            var tile = GetTerrain(next);
            if (tile != null && (tile.Type == PieceType.Wall || (tile.Type == PieceType.Gate && !tile.GateOpen) ||
                (tile.Type == PieceType.Exit && (piece.Type != PieceType.Normal ||
                (tile.Color != PieceColor.None && tile.Color != piece.Color))))) return false;
            var partner = piece.Type == PieceType.Normal && !forcedTetherFollow
                ? tetherPartners[piece.Id] : null;
            if (partner != null && partner.Active &&
                Manhattan(next, partner.Position) > TetherLength)
            {
                // A taut link creates a *real dependency*, unlike a remote
                // signal. The initiator cannot move unless the partner can
                // follow one legal step. Keep both movements atomic.
                if (moving[piece.Id] || moving[partner.Id])
                    return false;
                var before = CaptureLocal();
                var priorSignals = (bool[])signaled.Clone();
                int beforeActions = actions.Count;
                // Present tension BEFORE the partner visibly moves. Any
                // unsuccessful local transaction removes this cue as well.
                Record(BoardActionType.TetherPull, piece,
                    piece.Position, partner.Position);
                moving[piece.Id] = true;
                var directionToSource = Toward(partner.Position, next);
                bool followed = Move(partner, directionToSource,
                    depth + 1, true);
                moving[piece.Id] = false;
                if (overflow) return false; // RequestMove rolls back the tap.
                if (!followed || (partner.Active &&
                    Manhattan(next, partner.Position) > TetherLength))
                {
                    RestoreLocal(before, priorSignals, beforeActions);
                    return false;
                }
                // Include the bond action in the normal presentation sequence.
                // On failure, undo the entire local pull and its gate/signal
                // effects, without cancelling earlier successful auto-moves.
                bool advanced = MoveInto(piece, direction, depth, next, tile,
                    forcedTetherFollow);
                if (!advanced && !overflow)
                    RestoreLocal(before, priorSignals, beforeActions);
                return advanced;
            }
            return MoveInto(piece, direction, depth, next, tile,
                forcedTetherFollow);
        }

        private bool MoveInto(BoardPiece piece, Direction direction,
            int depth, GridPosition next, BoardPiece tile,
            bool forcedTetherFollow)
        {
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
                Move(piece, piece.Direction, depth + 1,
                    forcedTetherFollow);
            }
            else if (tile.Type == PieceType.Resonance)
            {
                // Resonance 2.0: entering a node signals a remote piece, then the
                // triggering piece continues on its ORIGINAL heading. This is a
                // single atomic tap, not a teleport or a second player move.
                if (!signaled[tile.Channel])
                {
                    signaled[tile.Channel] = true;
                    var partner = resonancePartners[tile.Id];
                    if (partner != null)
                    {
                        Record(BoardActionType.ResonancePulse, tile, tile.Position, partner.Position);
                        var receiver = GetOccupant(partner.Position);
                        if (receiver != null && receiver != piece && receiver.Type == PieceType.Normal)
                        {
                            // While the receiver runs, it must not push its still-active
                            // source. Keep existing cyclic rollback semantics intact.
                            moving[piece.Id] = true;
                            Move(receiver, receiver.Direction, depth + 1);
                            moving[piece.Id] = false;
                        }
                    }
                }
                // No second pulse when this channel has fired already; however
                // the surface is always traversable. A blocked next cell leaves
                // the source on the pad without undoing the successful signal.
                if (!overflow && piece.Active)
                    Move(piece, direction, depth + 1,
                        forcedTetherFollow);
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
