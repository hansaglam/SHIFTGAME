using System.Collections.Generic;
using NUnit.Framework;
using Shift.Game.Editor;

namespace Shift.Game.Tests
{
    // Design regression guards. These prove mechanics/solutions, not enjoyment.
    public sealed class ChapterFinaleQualityTests
    {
        [Test]
        public void Chapter3FinaleMustNotRepeatLevel55AndMustDependOnBothGateChannels()
        {
            var previous = Chapter3DraftAudit.Get(55);
            var finale = Chapter3DraftAudit.Get(60);
            Assert.That(previous, Is.Not.Null);
            Assert.That(finale, Is.Not.Null);
            Assert.That(finale.ResolvedTargetColors.Count, Is.EqualTo(2));
            Assert.That(finale.KnownSolutionLength, Is.EqualTo(11));
            Assert.That(finale.MoveLimit, Is.GreaterThan(finale.KnownSolutionLength));

            int matchingOpening = 0;
            while (matchingOpening < previous.KnownSolutionLength &&
                   matchingOpening < finale.KnownSolutionLength &&
                   previous.KnownSolution[matchingOpening] == finale.KnownSolution[matchingOpening])
                matchingOpening++;
            Assert.That(matchingOpening, Is.LessThan(3),
                "The chapter finale must not reuse the opening solution from Level 55.");

            Assert.That(LevelValidation.VerifySolution(finale, out string error, out _),
                Is.True, error);
            var proof = CampaignCertification.Prove(finale, 60);
            Assert.That(proof.complete, Is.True, proof.error);
            Assert.That(proof.minimum, Is.EqualTo(11));

            var board = new BoardManager();
            board.Load(finale);
            var openedChannels = new HashSet<int>();
            int multiPieceTaps = 0, totalOpenings = 0;
            foreach (var tap in finale.KnownSolution)
            {
                Assert.That(board.RequestMove(tap), Is.True, "Noncommitting tap " + tap);
                Assert.That(board.LastReactionWasCancelled, Is.False);
                var moved = new HashSet<int>();
                foreach (var action in board.Actions)
                {
                    if (action.Type == BoardActionType.Move) moved.Add(action.PieceId);
                    if (action.Type == BoardActionType.GateOpened)
                    {
                        totalOpenings++;
                        openedChannels.Add(board.Pieces[action.PieceId].Channel);
                    }
                }
                if (moved.Count >= 2) multiPieceTaps++;
                board.CompleteResolution();
            }
            Assert.That(board.State, Is.EqualTo(GameState.Won));
            Assert.That(openedChannels.SetEquals(new[] { 1, 2 }), Is.True,
                "Both gate channels must be involved in the intended solution.");
            Assert.That(totalOpenings, Is.GreaterThanOrEqualTo(3));
            Assert.That(multiPieceTaps, Is.GreaterThanOrEqualTo(3));
            Assert.That(finale.VerifiedOptimalMoveCount, Is.Null,
                "Keep Perfect Shift disabled until human design approval.");
        }

        [Test]
        public void DuplicateRouteAuditMustSurfaceExistingPatterns()
        {
            var longer58 = Chapter3AlternateSolutions.Get(58);
            var longer59 = Chapter3AlternateSolutions.Get(59);
            var alternateRisk = LevelRouteSimilarityAudit.Evaluate(58, 59, true,
                longer58, longer59);
            Assert.That(alternateRisk, Is.Not.Null,
                "Near-identical alternate solutions must not escape review.");
            Assert.That(alternateRisk.commonOpening, Is.GreaterThanOrEqualTo(6));
            Assert.That(alternateRisk.priority, Is.EqualTo("HIGH"));

            var route69 = Chapter4ResonanceExtendedDrafts.Get(69).KnownSolution;
            var route72 = Chapter4ResonanceExtendedDrafts.Get(72).KnownSolution;
            Assert.That(LevelRouteSimilarityAudit.Evaluate(69, 72, false,
                route69, route72), Is.Not.Null,
                "Repeated vertical-lane family must remain in the review queue.");
        }

        [Test]
        public void Chapter4FinaleMustRetainTwoSignalsTwoGatesAndThreeColors()
        {
            var finale = Chapter4ResonanceExtendedDrafts.Get(80);
            Assert.That(finale, Is.Not.Null);
            Assert.That(finale.ResolvedTargetColors.Count, Is.EqualTo(3));
            Assert.That(finale.KnownSolutionLength, Is.EqualTo(8));
            var board = new BoardManager();
            board.Load(finale);
            var pulses = new HashSet<int>();
            var gates = new HashSet<int>();
            int multiPieceTaps = 0;
            foreach (var tap in finale.KnownSolution)
            {
                Assert.That(board.RequestMove(tap), Is.True);
                Assert.That(board.LastReactionWasCancelled, Is.False);
                var moved = new HashSet<int>();
                foreach (var action in board.Actions)
                {
                    if (action.Type == BoardActionType.Move) moved.Add(action.PieceId);
                    if (action.Type == BoardActionType.ResonancePulse)
                        pulses.Add(board.Pieces[action.PieceId].Channel);
                    if (action.Type == BoardActionType.GateOpened)
                        gates.Add(board.Pieces[action.PieceId].Channel);
                }
                if (moved.Count >= 2) multiPieceTaps++;
                board.CompleteResolution();
            }
            Assert.That(board.State, Is.EqualTo(GameState.Won));
            Assert.That(pulses.SetEquals(new[] { 1, 2 }), Is.True);
            Assert.That(gates.SetEquals(new[] { 1, 2 }), Is.True);
            Assert.That(multiPieceTaps, Is.GreaterThanOrEqualTo(2));
            Assert.That(finale.VerifiedOptimalMoveCount, Is.Null);
        }
    }
}
