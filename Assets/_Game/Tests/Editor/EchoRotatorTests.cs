using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using Shift.Game.Editor;

namespace Shift.Game.Tests
{
    public sealed class EchoRotatorTests
    {
        private LevelData scratch;

        private static PiecePlacement P(PieceType type, PieceColor color, Direction direction,
            int x, int y, int channel = 0, bool initialOpen = false)
            => new PiecePlacement(type, color, direction, new GridPosition(x, y), channel, initialOpen);

        [TearDown]
        public void CleanUp()
        {
            if (scratch != null) UnityEngine.Object.DestroyImmediate(scratch);
            scratch = null;
        }

        [Test]
        public void FirstAndSecondEntriesTurnOppositeWaysAndUndoRestoresState()
        {
            scratch = ScriptableObject.CreateInstance<LevelData>();
            scratch.Configure(5, 5, 3, PieceColor.Red, new[]
            {
                P(PieceType.Normal, PieceColor.Red, Direction.Right, 1, 2),
                P(PieceType.Normal, PieceColor.Blue, Direction.Down, 2, 3),
                P(PieceType.EchoRotator, PieceColor.None, Direction.None, 2, 2),
                P(PieceType.Exit, PieceColor.Red, Direction.None, 2, 1),
                P(PieceType.Exit, PieceColor.Blue, Direction.None, 3, 2)
            });
            scratch.ConfigureTargetColors(new[] { PieceColor.Red, PieceColor.Blue });
            Assert.That(scratch.ValidatePlayable(out var err), Is.True, err);
            var board = new BoardManager();
            board.Load(scratch);
            var echo = board.GetTerrain(new GridPosition(2, 2));
            Assert.That(echo.EchoNextClockwise, Is.True);

            Assert.That(board.RequestMove(new GridPosition(1, 2)), Is.True);
            Assert.That(board.Actions.Any(a=>a.Type==BoardActionType.EchoFlipped &&
                a.Direction==Direction.Left), Is.True);
            Assert.That(echo.EchoNextClockwise, Is.False);
            board.CompleteResolution();
            Assert.That(board.Undo(), Is.True);
            Assert.That(echo.EchoNextClockwise, Is.True);
            Assert.That(board.MovesRemaining, Is.EqualTo(3));

            Assert.That(board.RequestMove(new GridPosition(1, 2)), Is.True);
            board.CompleteResolution();
            Assert.That(board.RequestMove(new GridPosition(2, 3)), Is.True);
            Assert.That(board.Actions.Any(a=>a.Type==BoardActionType.EchoFlipped &&
                a.Direction==Direction.Right), Is.True);
            Assert.That(echo.EchoNextClockwise, Is.True);
            board.CompleteResolution();
            Assert.That(board.State, Is.EqualTo(GameState.Won));
            Assert.That(board.MovesRemaining, Is.EqualTo(1));
        }

        [Test]
        public void BlockedPushDoesNotToggleEchoOrSpendMove()
        {
            scratch = ScriptableObject.CreateInstance<LevelData>();
            scratch.Configure(5, 5, 3, PieceColor.Red, new[]
            {
                P(PieceType.Normal, PieceColor.Red, Direction.Right, 1, 2),
                P(PieceType.PushBlock, PieceColor.None, Direction.None, 2, 2),
                P(PieceType.EchoRotator, PieceColor.None, Direction.None, 2, 2),
                P(PieceType.Wall, PieceColor.None, Direction.None, 3, 2),
                P(PieceType.Exit, PieceColor.Red, Direction.None, 4, 4)
            });
            Assert.That(scratch.ValidatePlayable(out var err), Is.True, err);
            var board = new BoardManager(); board.Load(scratch);
            Assert.That(board.RequestMove(new GridPosition(1, 2)), Is.True);
            Assert.That(board.ReactionDepth, Is.Zero);
            Assert.That(board.MovesRemaining, Is.EqualTo(3));
            Assert.That(board.GetTerrain(new GridPosition(2, 2)).EchoNextClockwise, Is.True);
        }

        [Test]
        public void PushingATargetThroughEchoChangesItsState()
        {
            var level = Chapter4EchoDrafts.Get(62);
            Assert.That(level, Is.Not.Null);
            var board = new BoardManager(); board.Load(level);
            Assert.That(board.RequestMove(new GridPosition(0, 2)), Is.True);
            Assert.That(board.Actions.Any(a => a.Type == BoardActionType.EchoFlipped), Is.True);
            Assert.That(board.GetTerrain(new GridPosition(2, 2)).EchoNextClockwise, Is.False);
            board.CompleteResolution();
            Assert.That(board.Undo(), Is.True);
            Assert.That(board.GetTerrain(new GridPosition(2, 2)).EchoNextClockwise, Is.True);
        }

        [Test]
        public void AllFiveDraftsHaveProvenShortestRoutes()
        {
            for (int n = 61; n <= 65; n++)
            {
                var level = Chapter4EchoDrafts.Get(n);
                Assert.That(level, Is.Not.Null, "Missing Level " + n);
                Assert.That(MechanicGuideCatalog.Contains(level, MechanicTopic.Echo), Is.True);
                Assert.That(LevelValidation.VerifySolution(level, out var error, out int depth),
                    Is.True, level.name + ": " + error);
                Assert.That(depth, Is.GreaterThanOrEqualTo(3));
                var proof = CampaignCertification.Prove(level, n);
                Assert.That(proof.complete, Is.True, level.name + ": " + proof.error);
                Assert.That(proof.minimum, Is.EqualTo(level.KnownSolutionLength),
                    "Uncertified shortest route in " + level.name);
                Assert.That(level.VerifiedOptimalMoveCount, Is.Null,
                    "Pilot must not grant an unverified Perfect Shift.");
            }
        }

        [Test]
        public void Level62HasLongerWinningNonPerfectSolution()
        {
            var level = Chapter4EchoDrafts.Get(62);
            var board = new BoardManager(); board.Load(level);
            foreach (var tap in new[] {
                new GridPosition(1, 2), new GridPosition(0, 2),
                new GridPosition(2, 3), new GridPosition(1, 2) })
            {
                Assert.That(board.RequestMove(tap), Is.True);
                Assert.That(board.ReactionDepth, Is.GreaterThan(0));
                Assert.That(board.LastReactionWasCancelled, Is.False);
                board.CompleteResolution();
            }
            Assert.That(board.State, Is.EqualTo(GameState.Won));
            Assert.That(board.MovesRemaining, Is.EqualTo(1));
            Assert.That(level.KnownSolutionLength, Is.EqualTo(3));
        }

        [Test]
        public void EchoHasLocalizedRulesInBothLanguages()
        {
            foreach(var lang in new[]{GameLanguage.English, GameLanguage.Turkish})
                foreach(var suffix in new[]{".name", ".body", ".example"})
                {
                    string key = "guide.echo" + suffix;
                    Assert.That(LocalizationCatalog.Format(key, lang), Is.Not.EqualTo(key));
                }
        }
    }
}
