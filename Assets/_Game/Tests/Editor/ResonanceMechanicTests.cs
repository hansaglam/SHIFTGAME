using System.Linq;
using NUnit.Framework;
using UnityEngine;
using Shift.Game.Editor;

namespace Shift.Game.Tests
{
    public sealed class ResonanceMechanicTests
    {
        private LevelData scratch;

        private static PiecePlacement P(PieceType type, PieceColor color, Direction direction,
            int x, int y, int channel = 0)
            => new PiecePlacement(type, color, direction, new GridPosition(x, y), channel);

        private BoardManager Build(int width, int height, PieceColor goal, params PiecePlacement[] placements)
        {
            scratch = ScriptableObject.CreateInstance<LevelData>();
            scratch.Configure(width, height, 5, goal, placements);
            Assert.That(scratch.ValidatePlayable(out string error), Is.True, error);
            var board = new BoardManager();
            board.Load(scratch);
            return board;
        }

        [TearDown]
        public void Cleanup()
        {
            if (scratch != null) Object.DestroyImmediate(scratch);
            scratch = null;
        }

        [Test]
        public void RequiresExactlyTwoNodesPerResonanceChannel()
        {
            scratch = ScriptableObject.CreateInstance<LevelData>();
            scratch.Configure(5, 5, 3, PieceColor.Red, new[]
            {
                P(PieceType.Normal, PieceColor.Red, Direction.Right, 0, 1),
                P(PieceType.Resonance, PieceColor.None, Direction.None, 1, 1, 1),
                P(PieceType.Exit, PieceColor.Red, Direction.None, 2, 1)
            });
            Assert.That(scratch.Validate(out string error), Is.False);
            StringAssert.Contains("exactly two", error);
        }

        [Test]
        public void OneTapCanTriggerBothColorsAndBothSourceAndReceiverFinish()
        {
            var level = Chapter4ResonanceDrafts.Get(61);
            Assert.That(level, Is.Not.Null);
            var board = new BoardManager();
            board.Load(level);
            Assert.That(board.RequestMove(new GridPosition(0, 1)), Is.True);
            Assert.That(board.LastReactionWasCancelled, Is.False);
            var events = board.Actions.ToList();
            Assert.That(events.Count(a => a.Type == BoardActionType.ResonancePulse), Is.EqualTo(1));
            int pulse = events.FindIndex(a => a.Type == BoardActionType.ResonancePulse);
            int blueDelivery = events.FindIndex(a => a.Type == BoardActionType.Deliver && a.PieceId == 1);
            int redDelivery = events.FindIndex(a => a.Type == BoardActionType.Deliver && a.PieceId == 0);
            Assert.That(pulse, Is.GreaterThan(0));
            Assert.That(blueDelivery, Is.GreaterThan(pulse));
            Assert.That(redDelivery, Is.GreaterThan(blueDelivery),
                "Receiver moves before the source resumes and delivers.");
            Assert.That(board.Pieces[0].Active, Is.False);
            Assert.That(board.Pieces[1].Active, Is.False);
            Assert.That(board.MovesRemaining, Is.EqualTo(level.MoveLimit - 1));
            board.CompleteResolution();
            Assert.That(board.State, Is.EqualTo(GameState.Won));
        }

        [Test]
        public void SourceContinuesAndUndoRestoresBothMovements()
        {
            var level = Chapter4ResonanceDrafts.Get(62);
            var board = new BoardManager();
            board.Load(level);
            Assert.That(board.RequestMove(new GridPosition(0, 2)), Is.True);
            Assert.That(board.Actions.Count(a => a.Type == BoardActionType.ResonancePulse), Is.EqualTo(1));
            Assert.That(board.Pieces[0].Position, Is.EqualTo(new GridPosition(2, 2)));
            Assert.That(board.Pieces[1].Position, Is.EqualTo(new GridPosition(4, 3)));
            board.CompleteResolution();
            Assert.That(board.State, Is.EqualTo(GameState.Playing));
            Assert.That(board.Undo(), Is.True);
            Assert.That(board.Pieces[0].Position, Is.EqualTo(new GridPosition(0, 2)));
            Assert.That(board.Pieces[1].Position, Is.EqualTo(new GridPosition(3, 3)));
            Assert.That(board.MovesRemaining, Is.EqualTo(level.MoveLimit));
            Assert.That(board.ReactionDepth, Is.Zero);
            // After Undo, the same tap must replay the same result.
            Assert.That(board.RequestMove(new GridPosition(0, 2)), Is.True);
            Assert.That(board.Pieces[0].Position, Is.EqualTo(new GridPosition(2, 2)));
            Assert.That(board.Pieces[1].Position, Is.EqualTo(new GridPosition(4, 3)));
        }

        [Test]
        public void BlockedReceiverDoesNotStopSourceFromFinishing()
        {
            var board = Build(5, 5, PieceColor.Red,
                P(PieceType.Normal, PieceColor.Red, Direction.Right, 0, 1),
                P(PieceType.Normal, PieceColor.Blue, Direction.Right, 3, 3),
                P(PieceType.Resonance, PieceColor.None, Direction.None, 1, 1, 1),
                P(PieceType.Resonance, PieceColor.None, Direction.None, 3, 3, 1),
                P(PieceType.Wall, PieceColor.None, Direction.None, 4, 3),
                P(PieceType.Exit, PieceColor.Red, Direction.None, 2, 1));
            Assert.That(board.RequestMove(new GridPosition(0, 1)), Is.True);
            Assert.That(board.Actions.Any(a => a.Type == BoardActionType.ResonancePulse), Is.True);
            Assert.That(board.Pieces[0].Active, Is.False);
            Assert.That(board.Pieces[1].Position, Is.EqualTo(new GridPosition(3, 3)));
            board.CompleteResolution();
            Assert.That(board.State, Is.EqualTo(GameState.Won));
        }

        [Test]
        public void TwoIndependentChannelsReactInTheSameSingleTap()
        {
            var level = Chapter4ResonanceDrafts.Get(64);
            var board = new BoardManager();
            board.Load(level);
            Assert.That(board.RequestMove(new GridPosition(0, 1)), Is.True);
            Assert.That(board.LastReactionWasCancelled, Is.False);
            Assert.That(board.Actions.Count(a => a.Type == BoardActionType.ResonancePulse), Is.EqualTo(2));
            Assert.That(board.Pieces[0].Position, Is.EqualTo(new GridPosition(2, 1)));
            Assert.That(board.Pieces[1].Position, Is.EqualTo(new GridPosition(5, 3)));
            Assert.That(board.Pieces[2].Position, Is.EqualTo(new GridPosition(1, 5)));
            board.CompleteResolution();
            Assert.That(board.State, Is.EqualTo(GameState.Playing));
        }

        [Test]
        public void CrossingBothPadsOfSameChannelCannotPulseTwiceInOneTap()
        {
            var board = Build(5, 5, PieceColor.Red,
                P(PieceType.Normal, PieceColor.Red, Direction.Right, 0, 1),
                P(PieceType.Normal, PieceColor.Blue, Direction.Right, 2, 1),
                P(PieceType.Resonance, PieceColor.None, Direction.None, 1, 1, 1),
                P(PieceType.Resonance, PieceColor.None, Direction.None, 2, 1, 1),
                P(PieceType.Exit, PieceColor.Red, Direction.None, 4, 4));
            Assert.That(board.RequestMove(new GridPosition(0, 1)), Is.True);
            Assert.That(board.LastReactionWasCancelled, Is.False);
            Assert.That(board.Actions.Count(a => a.Type == BoardActionType.ResonancePulse), Is.EqualTo(1));
            board.CompleteResolution();
            Assert.That(board.State, Is.EqualTo(GameState.Playing));
        }

        [Test]
        public void RemoteCycleCancelsWholeTapWithoutSpendingMoves()
        {
            var board = Build(5, 5, PieceColor.Red,
                P(PieceType.Normal, PieceColor.Red, Direction.Right, 0, 1),
                P(PieceType.Normal, PieceColor.Blue, Direction.Left, 2, 1),
                P(PieceType.Resonance, PieceColor.None, Direction.None, 1, 1, 1),
                P(PieceType.Resonance, PieceColor.None, Direction.None, 2, 1, 1),
                P(PieceType.Exit, PieceColor.Red, Direction.None, 4, 4));
            Assert.That(board.RequestMove(new GridPosition(0, 1)), Is.True);
            Assert.That(board.LastReactionWasCancelled, Is.True);
            Assert.That(board.ReactionDepth, Is.Zero);
            Assert.That(board.MovesRemaining, Is.EqualTo(5));
            Assert.That(board.Pieces[0].Position, Is.EqualTo(new GridPosition(0, 1)));
            Assert.That(board.Pieces[1].Position, Is.EqualTo(new GridPosition(2, 1)));
        }

        [Test]
        public void Level62HasLongerWinAndShorterResonanceRoute()
        {
            var level = Chapter4ResonanceDrafts.Get(62);
            var board = new BoardManager();
            board.Load(level);
            foreach (var tap in new[] {
                new GridPosition(3, 3), new GridPosition(0, 2),
                new GridPosition(2, 2), new GridPosition(4, 3) })
            {
                Assert.That(board.RequestMove(tap), Is.True);
                Assert.That(board.LastReactionWasCancelled, Is.False);
                Assert.That(board.ReactionDepth, Is.GreaterThan(0));
                board.CompleteResolution();
            }
            Assert.That(board.State, Is.EqualTo(GameState.Won));
            Assert.That(level.KnownSolutionLength, Is.EqualTo(3));
        }

        [Test]
        public void FirstMoveOf65PushesHelperAndOpensRemoteGate()
        {
            var level = Chapter4ResonanceDrafts.Get(65);
            var board = new BoardManager();
            board.Load(level);
            Assert.That(board.GetTerrain(new GridPosition(5, 4)).GateOpen, Is.False);
            Assert.That(board.RequestMove(new GridPosition(1, 2)), Is.True);
            Assert.That(board.ReactionDepth, Is.Zero, "Red begins blocked; an assisted push is required.");
            board.CompleteResolution();
            Assert.That(board.RequestMove(new GridPosition(0, 2)), Is.True);
            Assert.That(board.Actions.Any(a => a.Type == BoardActionType.ResonancePulse), Is.True);
            Assert.That(board.GetTerrain(new GridPosition(5, 4)).GateOpen, Is.True);
            Assert.That(board.Pieces[1].Position, Is.EqualTo(new GridPosition(3, 2)));
            Assert.That(board.Pieces[2].Position, Is.EqualTo(new GridPosition(4, 4)));
            Assert.That(board.Pieces[0].Position, Is.EqualTo(new GridPosition(1, 2)));
        }

        [Test]
        public void AllFiveDraftSolutionsAreValidAndProvenShortestInUnity()
        {
            for (int n = 61; n <= 65; n++)
            {
                var level = Chapter4ResonanceDrafts.Get(n);
                Assert.That(level, Is.Not.Null, "Missing Level " + n);
                Assert.That(MechanicGuideCatalog.Contains(level, MechanicTopic.Resonance), Is.True);
                Assert.That(LevelValidation.VerifySolution(level, out string error, out int maxDepth),
                    Is.True, "Level " + n + ": " + error);
                Assert.That(maxDepth, Is.GreaterThanOrEqualTo(4));
                var proof = CampaignCertification.Prove(level, n);
                Assert.That(proof.complete, Is.True, "Level " + n + ": " + proof.error);
                Assert.That(proof.minimum, Is.EqualTo(level.KnownSolutionLength));
                Assert.That(level.VerifiedOptimalMoveCount, Is.Null,
                    "The pilot must not grant speculative Perfect Shift.");
            }
        }

        [Test]
        public void TutorialIsLocalizedToBothLanguages()
        {
            foreach (var language in new[] { GameLanguage.English, GameLanguage.Turkish })
                foreach (var suffix in new[] { ".name", ".body", ".example" })
                {
                    var key = "guide.resonance" + suffix;
                    Assert.That(LocalizationCatalog.Format(key, language), Is.Not.EqualTo(key));
                }
        }
    }
}
