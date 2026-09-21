using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shift.Game
{
    [Serializable]
    public struct PiecePlacement
    {
        public PieceType type;
        public PieceColor color;
        public Direction direction;
        public GridPosition position;
        public int channel;
        public bool initialOpen;

        public PiecePlacement(PieceType type, PieceColor color, Direction direction, GridPosition position, int channel = 0, bool initialOpen = false)
        { this.type = type; this.color = color; this.direction = direction; this.position = position; this.channel = channel; this.initialOpen = initialOpen; }
    }

    [CreateAssetMenu(fileName = "Level", menuName = "SHIFT/Level")]
    public sealed class LevelData : ScriptableObject
    {
        [SerializeField, Range(1, 64)] private int width = 6;
        [SerializeField, Range(1, 64)] private int height = 6;
        [SerializeField, Min(1)] private int moveLimit = 3;
        [SerializeField] private PieceColor targetColor = PieceColor.Red;
        [SerializeField] private List<PieceColor> targetColors = new List<PieceColor>();
        [SerializeField] private List<PiecePlacement> placements = new List<PiecePlacement>();
        [SerializeField] private string displayTitle;
        [SerializeField, TextArea] private string hint;
        [SerializeField, Tooltip("Editor verification only: board coordinates tapped in order.")]
        private List<GridPosition> knownSolution = new List<GridPosition>();
        public int Width => width;
        public int Height => height;
        public int MoveLimit => moveLimit;
        public PieceColor TargetColor => targetColor;
        public IReadOnlyList<PieceColor> ResolvedTargetColors => targetColors != null && targetColors.Count > 0
            ? targetColors.AsReadOnly() : Array.AsReadOnly(new[] { targetColor });
        public bool IsTargetColor(PieceColor color) => targetColors != null && targetColors.Count > 0
            ? targetColors.Contains(color) : color == targetColor;
        public IReadOnlyList<PiecePlacement> Placements => placements;
        public string DisplayTitle => string.IsNullOrEmpty(displayTitle) ? name : displayTitle;
        public string Hint => hint;
        public IReadOnlyList<GridPosition> KnownSolution => knownSolution;
        public int KnownSolutionLength => knownSolution.Count;
        public int? VerifiedOptimalMoveCount => VerifiedOptimality.Get(this);
        public LevelDesign Design => LevelDesignCatalog.Current == null ? null : LevelDesignCatalog.Current.Find(this);

        public bool ValidatePlayable(out string error)
        {
            if (!Validate(out error)) return false;
            foreach (var color in ResolvedTargetColors)
            {
                bool exit = false;
                foreach (var p in placements)
                    if (p.type == PieceType.Exit && (p.color == PieceColor.None || p.color == color)) { exit = true; break; }
                if (!exit) { error = "A playable level needs an exit accepting each target color."; return false; }
            }
            return true;
        }

        public void Configure(int boardWidth, int boardHeight, int moves, PieceColor target, IEnumerable<PiecePlacement> pieces)
        {
            width = boardWidth; height = boardHeight; moveLimit = moves; targetColor = target;
            targetColors = new List<PieceColor>();
            placements = new List<PiecePlacement>(pieces);
        }
        public void ConfigureTargetColors(IEnumerable<PieceColor> colors)
            => targetColors = colors == null ? new List<PieceColor>() : new List<PieceColor>(colors);

        public bool Validate(out string error)
        {
            error = null;
            if (width < 1 || height < 1 || width > 64 || height > 64 || moveLimit < 1)
                error = "Board dimensions must be 1�64 and move limit must be positive.";
            else if (!Enum.IsDefined(typeof(PieceColor), targetColor) || targetColor == PieceColor.None)
                error = "Choose a target color.";
            if (error != null) return false;
            var objective = new HashSet<PieceColor>();
            foreach (var color in ResolvedTargetColors)
                if (color == PieceColor.None || !Enum.IsDefined(typeof(PieceColor), color) || !objective.Add(color))
                { error = "Target colors must be valid, distinct colors."; return false; }
            if (objective.Count > 3) { error = "Use at most three target colors."; return false; }
            var occupied = new HashSet<GridPosition>();
            var terrain = new HashSet<GridPosition>();
            var walls = new HashSet<GridPosition>();
            var switches = new HashSet<int>(); var gates = new HashSet<int>();
            var targets = new HashSet<PieceColor>();
            if (placements == null) { error = "Missing placements."; return false; }
            foreach (var p in placements)
            {
                if (!Enum.IsDefined(typeof(PieceType), p.type) || !Enum.IsDefined(typeof(PieceColor), p.color) ||
                    !Enum.IsDefined(typeof(Direction), p.direction)) { error = "Unknown piece enum value."; return false; }
                if (p.position.x < 0 || p.position.y < 0 || p.position.x >= width || p.position.y >= height)
                { error = $"Piece outside board at {p.position}."; return false; }
                bool stateTile = p.type == PieceType.Switch || p.type == PieceType.Gate;
                if (stateTile && (p.channel < 1 || p.channel > 2 || p.color != PieceColor.None || p.direction != Direction.None))
                { error = "Switch/Gate requires channel 1 or 2, no color and no direction."; return false; }
                if ((!stateTile && p.channel != 0) || (p.type != PieceType.Gate && p.initialOpen))
                { error = "Channel/open fields are only supported on Switch/Gate."; return false; }
                if (p.type == PieceType.Switch) switches.Add(p.channel);
                if (p.type == PieceType.Gate) gates.Add(p.channel);
                bool movable = p.type == PieceType.Normal || p.type == PieceType.PushBlock;
                if (!(movable ? occupied : terrain).Add(p.position))
                { error = $"Duplicate piece on the same layer at {p.position}."; return false; }
                if ((p.type == PieceType.Normal || p.type == PieceType.Direction) && p.direction == Direction.None)
                { error = $"Normal and Direction pieces require a direction at {p.position}."; return false; }
                if (p.type == PieceType.Wall || (p.type == PieceType.Gate && !p.initialOpen)) walls.Add(p.position);
                if (p.type == PieceType.Normal && IsTargetColor(p.color)) targets.Add(p.color);
            }
            if (!switches.SetEquals(gates)) { error = "Each switch channel needs a gate and each gate needs a switch."; return false; }
            if (walls.Overlaps(occupied)) error = "A movable piece cannot overlap a wall.";
            else if (!targets.SetEquals(objective)) error = "Place at least one Normal piece of each target color.";
            return error == null;
        }
    }
}
