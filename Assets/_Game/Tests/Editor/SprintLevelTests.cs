using System.Collections.Generic;
using NUnit.Framework;
using Shift.Game.Editor;
using UnityEditor;

namespace Shift.Game.Tests
{
    public sealed class SprintLevelTests
    {
        [Test] public void ExactlyTenHandcraftedLevelsExist() => Assert.That(LevelValidation.SprintLevels().Length, Is.EqualTo(10));

        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)]
        [TestCase(5)] [TestCase(6)] [TestCase(7)] [TestCase(8)] [TestCase(9)]
        public void LevelValidatesLoadsAndKnownSolutionWins(int index)
        {
            var level = LevelValidation.SprintLevels()[index];
            Assert.That(level.Width, Is.InRange(4, 6)); Assert.That(level.Height, Is.InRange(4, 6));
            Assert.That(level.ValidatePlayable(out string error), Is.True, error);
            var board = new BoardManager(); board.Load(level);
            Assert.That(board.State, Is.EqualTo(GameState.Playing));
            Assert.That(board.Pieces.Count, Is.EqualTo(level.Placements.Count));
            Assert.That(LevelValidation.VerifySolution(level, out error, out int depth), Is.True, error);
            if (index == 9) { Assert.That(level.KnownSolution.Count, Is.InRange(3, 5)); Assert.That(depth, Is.GreaterThanOrEqualTo(3)); }
        }

        [TestCase(6, 3)] [TestCase(8, 2)] [TestCase(9, 4)]
        public void PlanningLevelsHaveNoShorterSolution(int index, int expected)
        {
            var level = LevelValidation.SprintLevels()[index];
            Assert.That(CanWin(level, new List<GridPosition>(), expected - 1), Is.False);
            Assert.That(CanWin(level, new List<GridPosition>(), expected), Is.True);
        }

        // Tiny bounded search for authored planning levels only, not a runtime solver.
        private static bool CanWin(LevelData level, List<GridPosition> path, int budget)
        {
            var board = new BoardManager(); board.Load(level);
            foreach (var tap in path)
            {
                if (!board.RequestMove(tap) || board.Actions.Count == 0) return false;
                board.CompleteResolution();
            }
            if (board.State == GameState.Won) return true;
            if (path.Count >= budget || board.State != GameState.Playing) return false;
            foreach (var piece in board.Pieces)
            {
                if (!piece.Active || piece.Type != PieceType.Normal) continue;
                path.Add(piece.Position);
                bool won = CanWin(level, path, budget); path.RemoveAt(path.Count - 1);
                if (won) return true;
            }
            return false;
        }

        [Test] public void LevelFiveBlockedPieceDoesNotUseSolutionBudget()
        {
            var board = new BoardManager(); board.Load(LevelValidation.SprintLevels()[4]);
            for (int i = 0; i < 4; i++)
            {
                Assert.That(board.RequestMove(new GridPosition(0, 1)), Is.True);
                Assert.That(board.Actions, Is.Empty); board.CompleteResolution();
            }
            Assert.That(board.MovesRemaining, Is.EqualTo(2));
        }

        [Test] public void PlayableValidationRequiresAnExitForTargetColor()
        {
            var level = UnityEngine.ScriptableObject.CreateInstance<LevelData>();
            try
            {
                level.Configure(4, 4, 1, PieceColor.Red, new[]
                {
                    new PiecePlacement(PieceType.Normal, PieceColor.Red, Direction.Right, new GridPosition(0, 0)),
                    new PiecePlacement(PieceType.Exit, PieceColor.Blue, Direction.None, new GridPosition(1, 0))
                });
                Assert.That(level.ValidatePlayable(out _), Is.False);
                // Simulation fixtures can still exercise deliberately incomplete boards.
                Assert.That(level.Validate(out _), Is.True);
            }
            finally { UnityEngine.Object.DestroyImmediate(level); }
        }
    }
}
