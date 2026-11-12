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
        public void Level59MustUseHelperAndAvoidRepeatingLevel58()
        {
            var previous = Chapter3DraftAudit.Get(58);
            var level = Chapter3DraftAudit.Get(59);
            Assert.That(previous, Is.Not.Null);
            Assert.That(level, Is.Not.Null);
            Assert.That(level.Width, Is.EqualTo(6));
            Assert.That(level.Height, Is.EqualTo(4));
            Assert.That(level.KnownSolutionLength, Is.EqualTo(10));
            Assert.That(level.MoveLimit, Is.EqualTo(11));
            Assert.That(LevelRouteSimilarityAudit.Evaluate(58, 59, false,
                previous.KnownSolution, level.KnownSolution), Is.Null,
                "Level 59 should not reuse the previous level's route pattern.");
            Assert.That(LevelRouteSimilarityAudit.Evaluate(58, 59, true,
                Chapter3AlternateSolutions.Get(58),
                Chapter3AlternateSolutions.Get(59)), Is.Null,
                "The longer-winning path must not copy Level 58 either.");

            Assert.That(LevelValidation.VerifySolution(level, out string error, out _),
                Is.True, error);
            var proof = CampaignCertification.Prove(level, 59);
            Assert.That(proof.complete, Is.True, proof.error);
            Assert.That(proof.minimum, Is.EqualTo(10));
            var board = new BoardManager();
            board.Load(level);
            var movedPieces = new HashSet<int>();
            int multiPieceTaps = 0, openings = 0;
            foreach (var tap in level.KnownSolution)
            {
                Assert.That(board.RequestMove(tap), Is.True);
                Assert.That(board.LastReactionWasCancelled, Is.False);
                var movedThisTap = new HashSet<int>();
                foreach (var action in board.Actions)
                {
                    if (action.Type == BoardActionType.Move)
                    {
                        movedPieces.Add(action.PieceId);
                        movedThisTap.Add(action.PieceId);
                    }
                    if (action.Type == BoardActionType.GateOpened) openings++;
                }
                if (movedThisTap.Count >= 2) multiPieceTaps++;
                board.CompleteResolution();
            }
            Assert.That(board.State, Is.EqualTo(GameState.Won));
            Assert.That(multiPieceTaps, Is.GreaterThanOrEqualTo(6),
                "The helper-chain interplay must not become decorative.");
            Assert.That(openings, Is.GreaterThanOrEqualTo(1));
            // Blue piece is an active helper rather than an objective; original index 2.
            Assert.That(movedPieces.Contains(2), Is.True,
                "The blue helper must participate in the recorded route.");
            Assert.That(level.VerifiedOptimalMoveCount, Is.Null);
        }

        [Test]
        public void DuplicateRouteAuditMustSurfaceExistingPatterns()
        {
            // The former 58/59 duplicate has been redesigned; it must stay absent.
            var alternateRisk = LevelRouteSimilarityAudit.Evaluate(58, 59, true,
                Chapter3AlternateSolutions.Get(58),
                Chapter3AlternateSolutions.Get(59));
            Assert.That(alternateRisk, Is.Null,
                "The redesigned Level 59 must not reintroduce Level 58's longer route.");

            Assert.That(LevelRouteSimilarityAudit.LongestSameAxisRun(
                Chapter3DraftAudit.Get(59).KnownSolution), Is.LessThan(4),
                "The redesigned Level 59 must not rely on a long tap corridor.");
            Assert.That(LevelRouteSimilarityAudit.LongestSameAxisRun(
                Chapter4ResonanceExtendedDrafts.Get(77).KnownSolution),
                Is.GreaterThanOrEqualTo(5),
                "Level 77 needs an explicit redesign; it is still too corridor-like.");

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
