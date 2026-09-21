using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Shift.Game.Editor
{
    public static class DesignValidation
    {
        public static bool ValidateCatalog(out string error)
        {
            var catalog = LevelDesignCatalog.Current;
            var levels = LevelValidation.AllLevels();
            if (catalog == null || catalog.Entries.Count != 40 || levels.Length != 40 || levels.Distinct().Count() != 40)
            { error = "Expected 40 unique campaign references and design entries."; return false; }
            if (catalog.Entries.Select(e => e.level).Distinct().Count() != 40)
            { error = "Duplicate design references."; return false; }
            foreach (var level in levels)
            {
                var d = catalog.Find(level);
                if (d == null || !Enum.IsDefined(typeof(PuzzleArchetype), d.primary) ||
                    !Enum.IsDefined(typeof(DifficultyBand), d.difficultyBand) || d.reasoningStyle == ReasoningStyle.None ||
                    ((int)d.reasoningStyle & ~63) != 0 || string.IsNullOrWhiteSpace(d.designRationale) ||
                    d.secondary.Any(a => !Enum.IsDefined(typeof(PuzzleArchetype), a) || a == d.primary) ||
                    d.secondary.Distinct().Count() != d.secondary.Count ||
                    !Enum.IsDefined(typeof(CreativeHookType), d.creativeHook) ||
                    d.creativeCandidate != (d.creativeHook != CreativeHookType.None) ||
                    d.hasPayoffMove != (d.expectedPayoffDepth >= 4) || (!d.hasPayoffMove && d.expectedPayoffDepth != 0))
                { error = level.name + ": incomplete or inconsistent design metadata."; return false; }
                if (!LevelValidation.VerifySolution(level, out error, out int depth)) return false;
                if (d.hasPayoffMove && depth < d.expectedPayoffDepth)
                { error = level.name + ": recorded solution misses the authored payoff."; return false; }
            }
            int candidates = catalog.Entries.Count(e => e.creativeCandidate);
            if (candidates < 4 || candidates > 6 || levels[39].Design.difficultyBand != DifficultyBand.Finale || !levels[39].Design.hasPayoffMove)
            { error = "Review creative coverage (4-6) and finale metadata."; return false; }
            error = null; return true;
        }
        [MenuItem("SHIFT/Validate Level Design 2.0")]
        public static void ValidateAll()
        {
            if (!ValidateCatalog(out string error)) Debug.LogError(error);
            else Debug.Log("SHIFT: all 40 designs, ordered references and recorded solutions validated.");
        }

        // Replay actual BoardManager states; no alternate simulation or unproven route-length claim.
        // An exceeded node bound throws, so it can never produce a certificate.
        public static bool CanWin(LevelData level, List<GridPosition> path, int budget, ref int nodes)
        {
            if (++nodes > 200000) throw new InvalidOperationException("Search bound reached; optimum remains unknown.");
            var board = new BoardManager(); board.Load(level);
            foreach (var tap in path)
            {
                if (!board.RequestMove(tap) || board.Actions.Count == 0 || board.LastReactionWasCancelled) return false;
                board.CompleteResolution();
            }
            if (board.State == GameState.Won) return true;
            if (path.Count >= budget || board.State != GameState.Playing) return false;
            foreach (var piece in board.Pieces)
            {
                if (!piece.Active || piece.Type != PieceType.Normal) continue;
                path.Add(piece.Position);
                bool won = CanWin(level, path, budget, ref nodes);
                path.RemoveAt(path.Count - 1);
                if (won) return true;
            }
            return false;
        }
    }
}
