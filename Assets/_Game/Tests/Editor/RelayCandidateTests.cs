using NUnit.Framework;
using Shift.Game.Editor;

namespace Shift.Game.Tests
{
    public sealed class RelayCandidateTests
    {
        [Test]
        public void PilotLevels61Through65ArePlayableAndHaveProvenShortestRoutes()
        {
            for (int n = 61; n <= 65; n++)
            {
                var level = Chapter4RelayDrafts.Get(n);
                Assert.That(level, Is.Not.Null, "Missing Level " + n);
                Assert.That(level.ValidatePlayable(out string reason), Is.True, reason);
                Assert.That(MechanicGuideCatalog.Contains(level, MechanicTopic.Relay), Is.True);
                Assert.That(LevelValidation.VerifySolution(level, out string error, out int depth),
                    Is.True, level.name + ": " + error);
                Assert.That(depth, Is.GreaterThanOrEqualTo(4));
                var proof = CampaignCertification.Prove(level, n);
                Assert.That(proof.complete, Is.True, level.name + ": " + proof.error);
                Assert.That(proof.minimum, Is.EqualTo(level.KnownSolutionLength),
                    level.name + " recorded route is not mathematically shortest.");
                Assert.That(level.VerifiedOptimalMoveCount, Is.Null,
                    "Experimental chapters must not grant speculative Perfect Shift.");
            }
        }

        [Test]
        public void LinkedCascadeNeedsPushBeforeRemoteGateIsOpened()
        {
            var level = Chapter4RelayDrafts.Get(65);
            Assert.That(level, Is.Not.Null);
            var board = new BoardManager(); board.Load(level);
            var gate = board.GetTerrain(new GridPosition(2, 1));
            Assert.That(gate.GateOpen, Is.False);

            // Tapping the red piece directly points into the pusher and cannot finish.
            Assert.That(board.RequestMove(new GridPosition(1, 2)), Is.True);
            board.CompleteResolution();
            Assert.That(gate.GateOpen, Is.False);
            // Restart explicitly, keeping this test independent from a blocked tap.
            board.Load(level);
            Assert.That(board.RequestMove(new GridPosition(0, 2)), Is.True);
            Assert.That(board.Actions, Has.Some.Matches<BoardAction>(
                a => a.Type == BoardActionType.Transfer));
            Assert.That(gate.GateOpen, Is.True);
            board.CompleteResolution();
        }
    }
}
