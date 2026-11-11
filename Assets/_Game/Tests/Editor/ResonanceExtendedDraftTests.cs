using NUnit.Framework;
using Shift.Game.Editor;

namespace Shift.Game.Tests
{
    public sealed class ResonanceExtendedDraftTests
    {
        [Test]
        public void EveryLevelHasValidCertifiedMinimumAndLongerWin()
        {
            for (int n = 66; n <= 80; n++)
            {
                var level = Chapter4ResonanceExtendedDrafts.Get(n);
                Assert.That(level, Is.Not.Null, "Missing Level " + n);
                Assert.That(level.ValidatePlayable(out string message), Is.True,
                    level.name + ": " + message);
                Assert.That(MechanicGuideCatalog.Contains(level, MechanicTopic.Resonance),
                    Is.True, "Mechanic missing: " + level.name);
                Assert.That(LevelValidation.VerifySolution(level, out string error, out int depth),
                    Is.True, level.name + ": " + error);
                Assert.That(depth, Is.GreaterThanOrEqualTo(4),
                    "No convincing chain reaction in " + level.name);
                var proof = CampaignCertification.Prove(level, n);
                Assert.That(proof.complete, Is.True, level.name + ": " + proof.error);
                Assert.That(proof.minimum, Is.EqualTo(level.KnownSolutionLength),
                    "Recorded route not shortest in " + level.name);
                Assert.That(Chapter4ResonanceExtendedDrafts.CheckAlternate(level, n, out error),
                    Is.True, level.name + ": " + error);
                Assert.That(level.VerifiedOptimalMoveCount, Is.Null,
                    "Do not award an unreviewed Perfect Shift in " + level.name);
            }
        }

        [Test]
        public void EveryShortestPathActuallyUsesResonance()
        {
            for (int n = 66; n <= 80; n++)
            {
                var level = Chapter4ResonanceExtendedDrafts.Get(n);
                Assert.That(level, Is.Not.Null);
                var board = new BoardManager(); board.Load(level);
                var channels = new System.Collections.Generic.HashSet<int>();
                foreach (var tap in level.KnownSolution)
                {
                    Assert.That(board.RequestMove(tap), Is.True, level.name);
                    Assert.That(board.LastReactionWasCancelled, Is.False, level.name);
                    foreach (var action in board.Actions)
                        if (action.Type == BoardActionType.ResonancePulse)
                            channels.Add(board.Pieces[action.PieceId].Channel);
                    board.CompleteResolution();
                }
                Assert.That(board.State, Is.EqualTo(GameState.Won), level.name);
                Assert.That(channels.Count, Is.GreaterThanOrEqualTo(1),
                    "Resonance was decorative in " + level.name);
                var usedChannels = new System.Collections.Generic.HashSet<int>();
                foreach (var p in level.Placements)
                    if (p.type == PieceType.Resonance) usedChannels.Add(p.channel);
                Assert.That(channels.SetEquals(usedChannels), Is.True,
                    "Every Resonance pair should matter on the optimal route: " + level.name);
            }
        }
    }
}
