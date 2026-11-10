using System;
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
            => new PiecePlacement(type, color, direction, new GridPosition(x,y), channel);

        [TearDown]
        public void Cleanup()
        {
            if (scratch != null) UnityEngine.Object.DestroyImmediate(scratch);
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
            Assert.That(scratch.Validate(out string missing), Is.False);
            StringAssert.Contains("exactly two", missing);
        }

        [Test]
        public void RemotePieceMovesInItsOwnDirectionAndUndoRestoresBoth()
        {
            var level = Chapter4ResonanceDrafts.Get(61);
            Assert.That(level, Is.Not.Null);
            var board = new BoardManager(); board.Load(level);
            Assert.That(board.RequestMove(new GridPosition(0,1)), Is.True);
            Assert.That(board.LastReactionWasCancelled, Is.False);
            var pulse = board.Actions.Single(a => a.Type == BoardActionType.ResonancePulse);
            Assert.That(pulse.From, Is.EqualTo(new GridPosition(1,1)));
            Assert.That(pulse.To, Is.EqualTo(new GridPosition(3,3)));
            Assert.That(board.Pieces[1].Direction, Is.EqualTo(Direction.Right));
            Assert.That(board.Pieces[1].Active, Is.False);
            board.CompleteResolution();
            Assert.That(board.CanUndo, Is.True);
            Assert.That(board.Undo(), Is.True);
            Assert.That(board.Pieces[0].Position, Is.EqualTo(new GridPosition(0,1)));
            Assert.That(board.Pieces[1].Position, Is.EqualTo(new GridPosition(3,3)));
            Assert.That(board.Pieces[1].Active, Is.True);
            Assert.That(board.MovesRemaining, Is.EqualTo(level.MoveLimit));
        }

        [Test]
        public void BlockedReceiverStaysPutButSourceStillCommits()
        {
            scratch = ScriptableObject.CreateInstance<LevelData>();
            scratch.Configure(5, 5, 3, PieceColor.Red, new[]
            {
                P(PieceType.Normal, PieceColor.Red, Direction.Right, 0, 1),
                P(PieceType.Normal, PieceColor.Blue, Direction.Right, 3, 3),
                P(PieceType.Resonance, PieceColor.None, Direction.None, 1, 1, 1),
                P(PieceType.Resonance, PieceColor.None, Direction.None, 3, 3, 1),
                P(PieceType.Wall, PieceColor.None, Direction.None, 4, 3),
                P(PieceType.Exit, PieceColor.Red, Direction.None, 2, 1)
            });
            Assert.That(scratch.ValidatePlayable(out string error), Is.True, error);
            var board = new BoardManager(); board.Load(scratch);
            Assert.That(board.RequestMove(new GridPosition(0,1)), Is.True);
            Assert.That(board.Actions.Any(a => a.Type == BoardActionType.ResonancePulse), Is.True);
            Assert.That(board.Pieces[1].Position, Is.EqualTo(new GridPosition(3,3)));
            Assert.That(board.MovesRemaining, Is.EqualTo(2));
            board.CompleteResolution();
            Assert.That(board.State, Is.EqualTo(GameState.Playing));
        }

        [Test]
        public void IndependentChannelsCanFormTwoStageChainWithinOneTap()
        {
            var level = Chapter4ResonanceDrafts.Get(64);
            Assert.That(level, Is.Not.Null);
            var board = new BoardManager(); board.Load(level);
            Assert.That(board.RequestMove(new GridPosition(0,1)), Is.True);
            Assert.That(board.LastReactionWasCancelled, Is.False);
            Assert.That(board.Actions.Count(a => a.Type == BoardActionType.ResonancePulse),
                Is.EqualTo(2), "Source pulse should activate a second remote pulse.");
            Assert.That(board.Pieces[2].Active, Is.False, "Yellow should reach its exit in the chain.");
            board.CompleteResolution();
            Assert.That(board.Undo(), Is.True);
            Assert.That(board.Pieces[2].Active, Is.True);
        }

        [Test]
        public void Level62AllowsSlowerNormalWinAndAnOptimalRoute()
        {
            var level = Chapter4ResonanceDrafts.Get(62);
            Assert.That(level, Is.Not.Null);
            var board = new BoardManager(); board.Load(level);
            foreach (var tap in new[] {
                new GridPosition(3,3), new GridPosition(0,2),
                new GridPosition(1,2), new GridPosition(4,3) })
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
        public void AllDraftSolutionsAreSolvableAndShortest()
        {
            for (int n=61; n<=65; n++)
            {
                var level = Chapter4ResonanceDrafts.Get(n);
                Assert.That(level, Is.Not.Null, "Missing level " + n);
                Assert.That(MechanicGuideCatalog.Contains(level, MechanicTopic.Resonance), Is.True);
                Assert.That(LevelValidation.VerifySolution(level, out string error, out int maxDepth),
                    Is.True, "Level " + n + ": " + error);
                Assert.That(maxDepth, Is.GreaterThanOrEqualTo(3));
                var proof = CampaignCertification.Prove(level, n);
                Assert.That(proof.complete, Is.True, "Level " + n + ": " + proof.error);
                Assert.That(proof.minimum, Is.EqualTo(level.KnownSolutionLength));
                Assert.That(level.VerifiedOptimalMoveCount, Is.Null,
                    "Prototype must not award speculative Perfect Shift.");
            }
        }

        [Test]
        public void GuideHasBothLanguages()
        {
            foreach (var language in new[] {GameLanguage.English, GameLanguage.Turkish})
                foreach (var suffix in new[] {".name", ".body", ".example"})
                {
                    var key = "guide.resonance" + suffix;
                    Assert.That(LocalizationCatalog.Format(key, language), Is.Not.EqualTo(key));
                }
        }
    }
}
