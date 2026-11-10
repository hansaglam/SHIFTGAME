using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Shift.Game.Tests
{
    public sealed class RelayMechanicTests
    {
        private LevelData level;
        private static PiecePlacement P(PieceType type, int x, int y,
            Direction direction = Direction.None, PieceColor color = PieceColor.None, int channel = 0)
            => new PiecePlacement(type, color, direction, new GridPosition(x, y), channel);

        private BoardManager Make(int width, int height, int moves, params PiecePlacement[] pieces)
        {
            level = ScriptableObject.CreateInstance<LevelData>();
            level.Configure(width, height, moves, PieceColor.Red, pieces);
            Assert.That(level.ValidatePlayable(out string error), Is.True, error);
            var board = new BoardManager();
            board.Load(level);
            return board;
        }

        [TearDown] public void TearDown()
        {
            if (level != null) UnityEngine.Object.DestroyImmediate(level);
            level = null;
        }

        [Test]
        public void PairMustHaveExactlyTwoPortsAndCorrectChannel()
        {
            level = ScriptableObject.CreateInstance<LevelData>();
            level.Configure(6, 4, 3, PieceColor.Red, new[]
            {
                P(PieceType.Normal, 0, 1, Direction.Right, PieceColor.Red),
                P(PieceType.Relay, 1, 1, channel:1),
                P(PieceType.Exit, 5, 1, color:PieceColor.Red)
            });
            Assert.That(level.ValidatePlayable(out string missing), Is.False);
            StringAssert.Contains("exactly two", missing);

            level.Configure(6, 4, 3, PieceColor.Red, new[]
            {
                P(PieceType.Normal, 0, 1, Direction.Right, PieceColor.Red),
                P(PieceType.Relay, 1, 1, channel:1),
                P(PieceType.Relay, 4, 1, channel:1),
                P(PieceType.Relay, 3, 1, channel:1),
                P(PieceType.Exit, 5, 1, color:PieceColor.Red)
            });
            Assert.That(level.ValidatePlayable(out string duplicate), Is.False);
            StringAssert.Contains("exactly two", duplicate);
        }

        [Test]
        public void TransferKeepsColorAndDirectionThenDelivers()
        {
            var board = Make(6, 4, 3,
                P(PieceType.Normal, 0, 1, Direction.Right, PieceColor.Red),
                P(PieceType.Relay, 1, 1, channel:1),
                P(PieceType.Relay, 4, 1, channel:1),
                P(PieceType.Exit, 5, 1, color:PieceColor.Red));

            Assert.That(board.RequestMove(new GridPosition(0, 1)), Is.True);
            var transfer = board.Actions.Single(a => a.Type == BoardActionType.Transfer);
            Assert.That(transfer.From, Is.EqualTo(new GridPosition(1, 1)));
            Assert.That(transfer.To, Is.EqualTo(new GridPosition(4, 1)));
            Assert.That(transfer.Direction, Is.EqualTo(Direction.Right));
            Assert.That(board.Pieces[0].Color, Is.EqualTo(PieceColor.Red));
            Assert.That(board.Pieces[0].Direction, Is.EqualTo(Direction.Right));
            board.CompleteResolution();
            Assert.That(board.State, Is.EqualTo(GameState.Won));
            Assert.That(board.MovesRemaining, Is.EqualTo(2));
            Assert.That(board.Pieces[0].Active, Is.False);
        }

        [Test]
        public void OccupiedLinkedDestinationDoesNotCommitOrSpendTap()
        {
            var board = Make(6, 4, 3,
                P(PieceType.Normal, 0, 1, Direction.Right, PieceColor.Red),
                P(PieceType.Normal, 4, 1, Direction.Up, PieceColor.Blue),
                P(PieceType.Relay, 1, 1, channel:1),
                P(PieceType.Relay, 4, 1, channel:1),
                P(PieceType.Exit, 5, 1, color:PieceColor.Red));

            Assert.That(board.RequestMove(new GridPosition(0, 1)), Is.True);
            Assert.That(board.Actions, Is.Empty);
            board.CompleteResolution();
            Assert.That(board.MovesRemaining, Is.EqualTo(3));
            Assert.That(board.Pieces[0].Position, Is.EqualTo(new GridPosition(0, 1)));

            Assert.That(board.RequestMove(new GridPosition(4, 1)), Is.True);
            board.CompleteResolution();
            Assert.That(board.Pieces[1].Position, Is.EqualTo(new GridPosition(4, 2)));
            Assert.That(board.RequestMove(new GridPosition(0, 1)), Is.True);
            Assert.That(board.Actions.Any(a => a.Type == BoardActionType.Transfer), Is.True);
            board.CompleteResolution();
            Assert.That(board.State, Is.EqualTo(GameState.Won));
        }

        [Test]
        public void PushedBoxUsesRelayAndUndoRestoresOriginalPositions()
        {
            var board = Make(6, 5, 4,
                P(PieceType.Normal, 0, 1, Direction.Right, PieceColor.Yellow),
                P(PieceType.PushBlock, 1, 1),
                P(PieceType.Relay, 2, 1, channel:1),
                P(PieceType.Relay, 4, 3, channel:1),
                P(PieceType.Normal, 0, 4, Direction.Left, PieceColor.Red),
                P(PieceType.Exit, 5, 4, color:PieceColor.Red));

            Assert.That(board.RequestMove(new GridPosition(0, 1)), Is.True);
            Assert.That(board.Actions.Any(a => a.Type == BoardActionType.Transfer && a.PieceId == 1), Is.True);
            board.CompleteResolution();
            Assert.That(board.Pieces[1].Position, Is.EqualTo(new GridPosition(5, 3)));
            Assert.That(board.Pieces[1].Color, Is.EqualTo(PieceColor.None));
            Assert.That(board.Undo(), Is.True);
            Assert.That(board.Pieces[1].Position, Is.EqualTo(new GridPosition(1, 1)));
            Assert.That(board.Pieces[0].Position, Is.EqualTo(new GridPosition(0, 1)));
            Assert.That(board.MovesRemaining, Is.EqualTo(4));
        }

        [Test]
        public void TransferLoopCancelsEntireReactionAndRestoresMoveCount()
        {
            var board = Make(5, 4, 3,
                P(PieceType.Normal, 0, 1, Direction.Right, PieceColor.Red),
                P(PieceType.Relay, 1, 1, channel:1),
                P(PieceType.Relay, 3, 1, channel:1),
                P(PieceType.Relay, 2, 1, channel:2),
                P(PieceType.Relay, 4, 1, channel:2),
                P(PieceType.Exit, 0, 3, color:PieceColor.Red));
            Assert.That(board.RequestMove(new GridPosition(0, 1)), Is.True);
            Assert.That(board.LastReactionWasCancelled, Is.True);
            Assert.That(board.Actions, Is.Empty);
            board.CompleteResolution();
            Assert.That(board.Pieces[0].Position, Is.EqualTo(new GridPosition(0, 1)));
            Assert.That(board.Pieces[0].Direction, Is.EqualTo(Direction.Right));
            Assert.That(board.MovesRemaining, Is.EqualTo(3));
            Assert.That(board.State, Is.EqualTo(GameState.Playing));
        }
    }
}
