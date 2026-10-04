using System.Collections.Generic;
namespace Shift.Game
{
    // Exact structural comparison, with no per-frame allocation and no live model mutation.
    public sealed class HintBoardState
    {
        private readonly GridPosition[] positions;
        private readonly Direction[] directions;
        private readonly bool[] active, gates;
        private readonly int moves, width, height;
        private readonly GameState state;
        public HintBoardState(BoardManager board)
        {
            moves = board.MovesRemaining; width = board.Width; height = board.Height; state = board.State;
            int n = board.Pieces.Count; positions = new GridPosition[n]; directions = new Direction[n]; active = new bool[n]; gates = new bool[n];
            for (int i = 0; i < n; i++) { var p = board.Pieces[i]; positions[i] = p.Position; directions[i] = p.Direction; active[i] = p.Active; gates[i] = p.GateOpen; }
        }
        public bool Matches(BoardManager board)
        {
            if (board == null || board.State != state || board.MovesRemaining != moves || board.Width != width || board.Height != height || board.Pieces.Count != positions.Length) return false;
            for (int i = 0; i < positions.Length; i++) { var p = board.Pieces[i]; if (p.Position != positions[i] || p.Direction != directions[i] || p.Active != active[i] || p.GateOpen != gates[i]) return false; }
            return true;
        }
    }
    public sealed class HintVisualResolver
    {
        private readonly HintVisualCatalog.Entry entry;
        private readonly List<HintBoardState> path = new List<HintBoardState>();
        private readonly List<GridPosition> taps = new List<GridPosition>();
        public HintVisualResolver(LevelData level)
        {
            entry = HintVisualCatalog.Find(level);
            if (entry == null || !level.ValidatePlayable(out _) || !level.VerifiedOptimalMoveCount.HasValue || level.KnownSolution.Count == 0 || level.KnownSolution.Count > 64) return;
            // One bounded authored path on an isolated model. No branching or runtime solver search.
            var replay = new BoardManager(); replay.Load(level);
            foreach (var tap in level.KnownSolution)
            {
                if (replay.State != GameState.Playing) { path.Clear(); taps.Clear(); return; }
                var before = new HintBoardState(replay); int moves = replay.MovesRemaining;
                if (!replay.RequestMove(tap)) { path.Clear(); taps.Clear(); return; }
                replay.CompleteResolution();
                if (replay.LastReactionWasCancelled || replay.MovesRemaining != moves - 1) { path.Clear(); taps.Clear(); return; }
                path.Add(before); taps.Add(tap);
            }
            if (replay.State != GameState.Won) { path.Clear(); taps.Clear(); }
        }
        public HintVisualTarget Resolve(BoardManager board, int stage)
        {
            if (entry == null || board == null || board.State != GameState.Playing || stage < 1 || stage > 3) return null;
            if (stage == 3)
                for (int i = 0; i < path.Count; i++)
                    if (path[i].Matches(board) && board.GetOccupant(taps[i])?.Type == PieceType.Normal)
                        return new HintVisualTarget(stage, new[]{taps[i]}, true);
            var cells = new List<GridPosition>(3);
            foreach (int id in entry.Targets(stage))
            {
                if (id < 0 || id >= board.Pieces.Count) return null;
                var piece = board.Pieces[id];
                if (piece.Active && piece.Type != PieceType.Wall && board.IsValid(piece.Position) && !cells.Contains(piece.Position)) cells.Add(piece.Position);
            }
            return cells.Count == 0 ? null : new HintVisualTarget(stage, cells.ToArray());
        }
    }
}
