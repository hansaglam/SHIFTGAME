using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Shift.Game.Editor
{
    public static class LevelValidation
    {
        public static LevelData[] SprintLevels() => ChapterLevels().Take(10).ToArray();
        public static LevelData[] AllLevels() => ChapterLevelWiring.LoadExpected();
        public static LevelData[] ChapterLevels() => AllLevels().Take(20).ToArray();
        [MenuItem("SHIFT/Validate Chapter 2 Levels")]
        public static void ValidateChapterTwo()
        { foreach (var level in AllLevels().Skip(20)) { if (!VerifySolution(level, out string error, out _)) Debug.LogError(level.name + ": " + error); } }

        public static bool VerifySolution(LevelData level, out string error, out int maximumDepth)
        {
            maximumDepth = 0;
            if (level == null) { error = "Missing level."; return false; }
            if (!level.ValidatePlayable(out error)) return false;
            if (level.KnownSolution == null || level.KnownSolution.Count == 0) { error = "Record a known solution."; return false; }
            var board = new BoardManager(); board.Load(level);
            foreach (var tap in level.KnownSolution)
            {
                int before = board.MovesRemaining;
                if (!board.RequestMove(tap) || board.LastReactionWasCancelled || board.ReactionDepth == 0 || board.MovesRemaining != before - 1)
                { error = $"Solution tap {tap} did not commit a reaction."; return false; }
                maximumDepth = Math.Max(maximumDepth, board.ReactionDepth); board.CompleteResolution();
            }
            if (board.State != GameState.Won) { error = "Recorded solution did not win within the move limit."; return false; }
            error = null; return true;
        }

        [MenuItem("SHIFT/Validate Sprint 1 Levels")]
        public static void ValidateAll()
        {
            var levels = SprintLevels();
            if (levels.Length != 10) { Debug.LogError($"Expected 10 Sprint levels, found {levels.Length}."); return; }
            foreach (var level in levels)
            {
                if (!VerifySolution(level, out string error, out int depth)) { Debug.LogError($"{level.name}: {error}", level); continue; }
                Debug.Log($"{level.DisplayTitle}: solved in {level.KnownSolution.Count} taps; maximum reaction depth {depth}.", level);
            }
        }
        [MenuItem("SHIFT/Validate Chapter 1 Levels")]
        public static void ValidateChapter()
        {
            var levels = ChapterLevels();
            if (levels.Length != 20) { Debug.LogError($"Expected 20 chapter levels, found {levels.Length}."); return; }
            foreach (var level in levels)
                if (!VerifySolution(level, out string error, out int depth)) Debug.LogError($"{level.name}: {error}", level);
                else Debug.Log($"{level.name}: {level.KnownSolution.Count} taps, max depth {depth}.", level);
        }
    }
}
