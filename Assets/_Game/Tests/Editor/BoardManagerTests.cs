using System;
using NUnit.Framework;
using UnityEngine;

namespace Shift.Game.Tests
{
    public sealed class BoardManagerTests
    {
        private LevelData level;
        private BoardManager board;
        private static PiecePlacement P(PieceType type, int x, int y, Direction direction = Direction.None, PieceColor color = PieceColor.None)
            => new PiecePlacement(type, color, direction, new GridPosition(x, y));
        private void Load(int moves, params PiecePlacement[] placements)
        {
            level = ScriptableObject.CreateInstance<LevelData>();
            level.Configure(6, 6, moves, PieceColor.Red, placements);
            board = new BoardManager(); board.Load(level);
        }
        [TearDown] public void Cleanup() { if (level != null) UnityEngine.Object.DestroyImmediate(level); }

        [Test] public void ChainDeliversRedAndLocksInputUntilPresentationCompletes()
        {
            Load(3, P(PieceType.Normal, 1, 2, Direction.Right, PieceColor.Yellow), P(PieceType.PushBlock, 2, 2),
                P(PieceType.Normal, 3, 2, Direction.Up, PieceColor.Red), P(PieceType.Exit, 4, 2));
            Assert.That(board.RequestMove(new GridPosition(1, 2)), Is.True);
            Assert.That(board.State, Is.EqualTo(GameState.Resolving));
            Assert.That(board.GetOccupant(new GridPosition(2, 2)).Color, Is.EqualTo(PieceColor.Yellow));
            Assert.That(board.GetOccupant(new GridPosition(3, 2)).Type, Is.EqualTo(PieceType.PushBlock));
            Assert.That(board.Pieces[2].Active, Is.False);
            Assert.That(board.ReactionDepth, Is.GreaterThanOrEqualTo(4));
            Assert.That(board.RequestMove(new GridPosition(2, 2)), Is.False);
            board.CompleteResolution(); Assert.That(board.State, Is.EqualTo(GameState.Won));
            Assert.That(board.MovesRemaining, Is.EqualTo(2));
        }

        [Test] public void LastMoveWinTakesPrecedenceOverLoss()
        {
            Load(1, P(PieceType.Normal, 0, 0, Direction.Right, PieceColor.Red), P(PieceType.Exit, 1, 0));
            board.RequestMove(new GridPosition(0, 0)); board.CompleteResolution();
            Assert.That(board.State, Is.EqualTo(GameState.Won));
        }

        [Test] public void BoundsBlockedTapIsFreeAndReturnsToPlaying()
        {
            Load(1, P(PieceType.Normal, 0, 0, Direction.Left, PieceColor.Red));
            board.RequestMove(new GridPosition(0, 0)); board.CompleteResolution();
            Assert.That(board.State, Is.EqualTo(GameState.Playing));
            Assert.That(board.MovesRemaining, Is.EqualTo(1));
            Assert.That(board.Actions, Is.Empty);
            Assert.That(board.Pieces[0].Position, Is.EqualTo(new GridPosition(0, 0)));
            Assert.That(board.RequestMove(new GridPosition(0, 0)), Is.True);
            board.CompleteResolution(); Assert.That(board.MovesRemaining, Is.EqualTo(1));
        }

        [Test] public void WallAtEndOfPushChainPreventsEveryMove()
        {
            Load(3, P(PieceType.Normal, 0, 0, Direction.Right, PieceColor.Red), P(PieceType.PushBlock, 1, 0), P(PieceType.Wall, 2, 0));
            board.RequestMove(new GridPosition(0, 0));
            Assert.That(board.Actions, Is.Empty); Assert.That(board.Pieces[1].Position.x, Is.EqualTo(1));
            Assert.That(board.MovesRemaining, Is.EqualTo(3));
        }

        [Test] public void EmptyAndPushBlockTapsDoNotSpendMoves()
        {
            Load(3, P(PieceType.Normal, 0, 0, Direction.Up, PieceColor.Red), P(PieceType.PushBlock, 1, 0));
            Assert.That(board.RequestMove(new GridPosition(5, 5)), Is.False);
            Assert.That(board.RequestMove(new GridPosition(1, 0)), Is.False);
            Assert.That(board.MovesRemaining, Is.EqualTo(3));
        }

        [Test] public void DirectionTileContinuesMovement()
        {
            Load(3, P(PieceType.Normal, 0, 0, Direction.Right, PieceColor.Red), P(PieceType.Direction, 1, 0, Direction.Up));
            board.RequestMove(new GridPosition(0, 0));
            Assert.That(board.Pieces[0].Position, Is.EqualTo(new GridPosition(1, 1)));
            Assert.That(board.Pieces[0].Direction, Is.EqualTo(Direction.Up));
            Assert.That(board.GetTerrain(new GridPosition(1, 0)).Type, Is.EqualTo(PieceType.Direction));
        }

        [Test] public void RotatorTurnsClockwiseAndContinues()
        {
            Load(3, P(PieceType.Normal, 0, 0, Direction.Up, PieceColor.Red), P(PieceType.Rotator, 0, 1));
            board.RequestMove(new GridPosition(0, 0));
            Assert.That(board.Pieces[0].Position, Is.EqualTo(new GridPosition(1, 1)));
            Assert.That(board.Pieces[0].Direction, Is.EqualTo(Direction.Right));
        }

        [Test] public void WrongColorExitAndPushBlockAreBlocked()
        {
            Load(3, P(PieceType.Normal, 0, 0, Direction.Right, PieceColor.Red), P(PieceType.Exit, 1, 0, color: PieceColor.Blue));
            board.RequestMove(new GridPosition(0, 0)); Assert.That(board.Actions, Is.Empty);
            level.Configure(6, 6, 3, PieceColor.Red, new[] { P(PieceType.Normal, 0, 0, Direction.Right, PieceColor.Red), P(PieceType.PushBlock, 1, 0), P(PieceType.Exit, 2, 0) });
            board.Load(level); board.RequestMove(new GridPosition(0, 0)); Assert.That(board.Actions, Is.Empty);
        }

        [Test] public void DirectionCycleRollsBackWholeTransaction()
        {
            Load(3, P(PieceType.Normal, 0, 0, Direction.Right, PieceColor.Red),
                P(PieceType.Direction, 1, 0, Direction.Up), P(PieceType.Direction, 1, 1, Direction.Right),
                P(PieceType.Direction, 2, 1, Direction.Down), P(PieceType.Direction, 2, 0, Direction.Left));
            board.RequestMove(new GridPosition(0, 0));
            Assert.That(board.LastReactionWasCancelled, Is.True); Assert.That(board.Actions, Is.Empty);
            Assert.That(board.Pieces[0].Position, Is.EqualTo(new GridPosition(0, 0)));
            Assert.That(board.Pieces[0].Direction, Is.EqualTo(Direction.Right));
            Assert.That(board.GetOccupant(new GridPosition(0, 0)), Is.SameAs(board.Pieces[0]));
            Assert.That(board.GetOccupant(new GridPosition(1, 0)), Is.Null);
            Assert.That(board.MovesRemaining, Is.EqualTo(3));
            board.CompleteResolution(); Assert.That(board.State, Is.EqualTo(GameState.Playing));
        }

        [Test] public void SuccessfulNonWinningMoveConsumesLastMoveAndLoses()
        {
            Load(1, P(PieceType.Normal, 0, 0, Direction.Right, PieceColor.Red));
            board.RequestMove(new GridPosition(0, 0));
            Assert.That(board.MovesRemaining, Is.Zero); Assert.That(board.ReactionDepth, Is.EqualTo(1));
            Assert.That(board.Pieces[0].Position, Is.EqualTo(new GridPosition(1, 0)));
            board.CompleteResolution(); Assert.That(board.State, Is.EqualTo(GameState.Lost));
            Assert.That(board.RequestMove(new GridPosition(1, 0)), Is.False);
        }

        [Test] public void BlockedTapCannotBeResubmittedUntilFeedbackCompletes()
        {
            Load(3, P(PieceType.Normal, 0, 0, Direction.Left, PieceColor.Red));
            Assert.That(board.RequestMove(new GridPosition(0, 0)), Is.True);
            Assert.That(board.State, Is.EqualTo(GameState.Resolving));
            Assert.That(board.RequestMove(new GridPosition(0, 0)), Is.False);
            Assert.That(board.MovesRemaining, Is.EqualTo(3));
            board.CompleteResolution(); Assert.That(board.State, Is.EqualTo(GameState.Playing));
        }

        [Test] public void DepthMatchesRecordedMoveTurnAndDeliveryActions()
        {
            Load(2, P(PieceType.Normal, 0, 0, Direction.Right, PieceColor.Red),
                P(PieceType.Direction, 1, 0, Direction.Up), P(PieceType.Exit, 1, 1));
            board.RequestMove(new GridPosition(0, 0));
            Assert.That(board.ReactionDepth, Is.EqualTo(4));
            Assert.That(board.Actions[0].Type, Is.EqualTo(BoardActionType.Move));
            Assert.That(board.Actions[1].Type, Is.EqualTo(BoardActionType.Turn));
            Assert.That(board.Actions[2].Type, Is.EqualTo(BoardActionType.Move));
            Assert.That(board.Actions[3].Type, Is.EqualTo(BoardActionType.Deliver));
            Assert.That(board.MovesRemaining, Is.EqualTo(1));
        }

        [Test] public void SuccessfulEntryWithBlockedContinuationStillCostsOneMove()
        {
            Load(3, P(PieceType.Normal, 0, 0, Direction.Right, PieceColor.Red),
                P(PieceType.Direction, 1, 0, Direction.Up), P(PieceType.Wall, 1, 1));
            board.RequestMove(new GridPosition(0, 0));
            Assert.That(board.Pieces[0].Position, Is.EqualTo(new GridPosition(1, 0)));
            Assert.That(board.Pieces[0].Direction, Is.EqualTo(Direction.Up));
            Assert.That(board.ReactionDepth, Is.EqualTo(2)); Assert.That(board.MovesRemaining, Is.EqualTo(2));
            board.CompleteResolution(); board.RequestMove(new GridPosition(1, 0));
            Assert.That(board.Actions, Is.Empty); Assert.That(board.MovesRemaining, Is.EqualTo(2));
            board.CompleteResolution(); Assert.That(board.State, Is.EqualTo(GameState.Playing));
        }

        [Test] public void AllTargetPiecesMustBeDelivered()
        {
            Load(3, P(PieceType.Normal, 0, 0, Direction.Right, PieceColor.Red), P(PieceType.Exit, 1, 0),
                P(PieceType.Normal, 0, 1, Direction.Right, PieceColor.Red), P(PieceType.Exit, 1, 1));
            board.RequestMove(new GridPosition(0, 0)); board.CompleteResolution();
            Assert.That(board.State, Is.EqualTo(GameState.Playing));
            board.RequestMove(new GridPosition(0, 1)); board.CompleteResolution();
            Assert.That(board.State, Is.EqualTo(GameState.Won));
        }

        [TestCase(4)] [TestCase(5)] [TestCase(6)] [TestCase(7)]
        public void VariableBoardSizeAndRestartAreDataDriven(int size)
        {
            Load(3, P(PieceType.Normal, 0, 0, Direction.Right, PieceColor.Red));
            level.Configure(size, size, 3, PieceColor.Red, level.Placements);
            board.Load(level); board.RequestMove(new GridPosition(0, 0)); board.Load(level);
            Assert.That(board.Width, Is.EqualTo(size)); Assert.That(board.MovesRemaining, Is.EqualTo(3));
            Assert.That(board.State, Is.EqualTo(GameState.Playing)); Assert.That(board.Actions, Is.Empty);
            Assert.That(board.Pieces[0].Position, Is.EqualTo(new GridPosition(0, 0)));
        }

        [Test] public void RepeatedInputProducesIdenticalActionSequence()
        {
            Load(3, P(PieceType.Normal, 0, 0, Direction.Right, PieceColor.Red), P(PieceType.Direction, 1, 0, Direction.Up));
            board.RequestMove(new GridPosition(0, 0));
            var expected = new BoardAction[board.Actions.Count]; for (int i = 0; i < expected.Length; i++) expected[i] = board.Actions[i];
            board.Load(level); board.RequestMove(new GridPosition(0, 0));
            CollectionAssert.AreEqual(expected, board.Actions);
        }

        [Test] public void InvalidLevelRejectedBeforeReplacingCurrentBoard()
        {
            Load(3, P(PieceType.Normal, 0, 0, Direction.Right, PieceColor.Red));
            level.Configure(6, 6, 3, PieceColor.Red, new[] { P(PieceType.Normal, 0, 0, Direction.Up, PieceColor.Red), P(PieceType.Wall, 0, 0) });
            Assert.Throws<ArgumentException>(() => board.Load(level));
            Assert.That(board.State, Is.EqualTo(GameState.Playing));
        }
    }
}
