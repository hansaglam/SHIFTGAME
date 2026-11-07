using NUnit.Framework;
using Shift.Game.Editor;

namespace Shift.Game.Tests
{
    // These tests must run against the real Unity BoardManager before any 41+ release.
    public sealed class Chapter3AlternativeRouteTests
    {
        [Test]
        public void Levels51Through60HaveAnOptimalRouteAndALongerWinningRoute()
        {
            for (int number = 51; number <= 60; number++)
            {
                var level = Chapter3DraftAudit.Get(number);
                Assert.That(level, Is.Not.Null, "Missing Level " + number);
                Assert.That(LevelValidation.VerifySolution(level, out string error, out int depth),
                    Is.True, level.name + ": " + error);
                Assert.That(depth, Is.GreaterThanOrEqualTo(4), level.name + " lacks meaningful chain payoff");

                var alternate = Chapter3AlternateSolutions.Get(number);
                Assert.That(alternate, Is.Not.Null, "Missing alternate route: " + level.name);
                Assert.That(alternate.Length, Is.EqualTo(level.KnownSolutionLength + 1),
                    level.name + " must allow a longer, valid winning path");
                Assert.That(alternate.Length, Is.LessThanOrEqualTo(level.MoveLimit),
                    level.name + " slower route exceeds its move limit");

                var board = new BoardManager();
                board.Load(level);
                foreach (var tap in alternate)
                {
                    int before = board.MovesRemaining;
                    Assert.That(board.RequestMove(tap), Is.True, level.name + ": rejected tap " + tap);
                    Assert.That(board.LastReactionWasCancelled, Is.False, level.name + ": cyclic reaction");
                    Assert.That(board.ReactionDepth, Is.GreaterThan(0), level.name + ": noncommitting tap " + tap);
                    Assert.That(board.MovesRemaining, Is.EqualTo(before - 1));
                    board.CompleteResolution();
                }
                Assert.That(board.State, Is.EqualTo(GameState.Won), level.name + " longer route did not win");
                var proof = CampaignCertification.Prove(level, number);
                Assert.That(proof.complete, Is.True, level.name + ": " + proof.error);
                Assert.That(proof.minimum, Is.EqualTo(level.KnownSolutionLength),
                    level.name + " saved solution was not proven shortest");
            }
        }
    }
}
