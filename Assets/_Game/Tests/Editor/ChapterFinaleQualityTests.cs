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
                Is.LessThan(4),
                "Level 77 must not regress to a repetitive straight corridor.");

            var route69 = Chapter4ResonanceExtendedDrafts.Get(69).KnownSolution;
            var route72 = Chapter4ResonanceExtendedDrafts.Get(72).KnownSolution;
            Assert.That(LevelRouteSimilarityAudit.Evaluate(69, 72, false,
                route69, route72), Is.Null,
                "New Levels 69 and 72 must not regress to the same vertical route.");
        }

        [Test]
        public void ResonanceAuditMustConfirmUsefulLevel77Pulse()
        {
            var level77 = Chapter4ResonanceExtendedDrafts.Get(77);
            Assert.That(level77, Is.Not.Null);
            Post40QualityAudit.CountResonanceReceivers(level77,
                level77.KnownSolution, out int pulses77, out int occupied77);
            Assert.That(pulses77, Is.EqualTo(1),
                "The Level 77 intended route should have one clear pulse.");
            Assert.That(occupied77, Is.EqualTo(1),
                "Level 77's signal must actually reach a waiting receiver.");
            Assert.That(Post40QualityAudit.CountProductiveResonanceMoves(
                level77, level77.KnownSolution), Is.EqualTo(1),
                "The waiting blue receiver must actually move during the pulse.");

            var finale = Chapter4ResonanceExtendedDrafts.Get(80);
            Assert.That(finale, Is.Not.Null);
            Post40QualityAudit.CountResonanceReceivers(finale,
                finale.KnownSolution, out int pulses80, out int occupied80);
            Assert.That(pulses80, Is.GreaterThanOrEqualTo(2));
            Assert.That(occupied80, Is.GreaterThanOrEqualTo(2),
                "Both finale signal channels must activate real remote pieces.");
            Assert.That(Post40QualityAudit.CountProductiveResonanceMoves(
                finale, finale.KnownSolution), Is.GreaterThanOrEqualTo(2),
                "Finale needs real remote moves, not occupied-but-blocked signals.");
        }

        [Test]
        public void Level77ReturnSignalMustBeARealReciprocalChain()
        {
            var level = Chapter4ResonanceExtendedDrafts.Get(77);
            Assert.That(level, Is.Not.Null);
            Assert.That(level.KnownSolutionLength, Is.EqualTo(8));
            Assert.That(level.MoveLimit, Is.EqualTo(9));
            Assert.That(level.ResolvedTargetColors.Count, Is.EqualTo(2));
            Assert.That(LevelValidation.VerifySolution(level, out string error, out _),
                Is.True, error);
            Assert.That(Chapter4ResonanceExtendedDrafts.CheckAlternate(level, 77, out error),
                Is.True, error);
            var proof = CampaignCertification.Prove(level, 77);
            Assert.That(proof.complete, Is.True, proof.error);
            Assert.That(proof.minimum, Is.EqualTo(8));

            var board = new BoardManager();
            board.Load(level);
            bool remoteBlueMoved = false, redDelivered = false, gateOpened = false;
            int sharedTaps = 0, pulseTaps = 0;
            foreach (var tap in level.KnownSolution)
            {
                Assert.That(board.RequestMove(tap), Is.True, "Invalid tap: " + tap);
                Assert.That(board.LastReactionWasCancelled, Is.False);
                bool pulseOccurred = false;
                var moved = new HashSet<int>();
                foreach (var action in board.Actions)
                {
                    if (action.Type == BoardActionType.ResonancePulse)
                    {
                        pulseOccurred = true;
                        pulseTaps++;
                    }
                    if (action.Type == BoardActionType.Move)
                    {
                        moved.Add(action.PieceId);
                        if (pulseOccurred && board.Pieces[action.PieceId].Color == PieceColor.Blue)
                            remoteBlueMoved = true;
                    }
                    if (action.Type == BoardActionType.GateOpened) gateOpened = true;
                    if (action.Type == BoardActionType.Deliver &&
                        board.Pieces[action.PieceId].Color == PieceColor.Red)
                        redDelivered = true;
                    if (action.Type == BoardActionType.GateClosed)
                        Assert.That(redDelivered, Is.True,
                            "Closing the red route under the red target obscures feedback.");
                }
                if (moved.Count >= 2) sharedTaps++;
                board.CompleteResolution();
            }
            Assert.That(board.State, Is.EqualTo(GameState.Won));
            Assert.That(remoteBlueMoved, Is.True,
                "The remote pulse must MOVE the waiting blue piece.");
            Assert.That(gateOpened, Is.True,
                "Blue's chain must prepare the route for red.");
            Assert.That(redDelivered, Is.True);
            Assert.That(pulseTaps, Is.EqualTo(1));
            Assert.That(sharedTaps, Is.GreaterThanOrEqualTo(5));
            Assert.That(level.VerifiedOptimalMoveCount, Is.Null,
                "Keep Perfect Shift disabled until final chapter approval.");
        }

        [TestCase(66, 5, 3)]
        [TestCase(69, 6, 3)]
        [TestCase(71, 6, 1)]
        [TestCase(72, 6, 2)]
        [TestCase(74, 6, 1)]
        [TestCase(76, 7, 2)]
        [TestCase(78, 8, 7)]
        [TestCase(79, 8, 4)]
        public void QualityRevisionsMustUseRealRemoteMovementAndDistinctPaths(
            int number, int shortest, int minimumMultiPieceTaps)
        {
            var level = Chapter4ResonanceExtendedDrafts.Get(number);
            Assert.That(level, Is.Not.Null);
            Assert.That(level.ValidatePlayable(out string error), Is.True, error);
            Assert.That(level.KnownSolutionLength, Is.EqualTo(shortest));
            Assert.That(level.MoveLimit, Is.GreaterThanOrEqualTo(shortest + 1));
            Assert.That(LevelValidation.VerifySolution(level, out error, out _),
                Is.True, error);
            Assert.That(Chapter4ResonanceExtendedDrafts.CheckAlternate(
                level, number, out error), Is.True, error);
            var proof = CampaignCertification.Prove(level, number);
            Assert.That(proof.complete, Is.True, proof.error);
            Assert.That(proof.minimum, Is.EqualTo(shortest));

            Assert.That(Post40QualityAudit.CountProductiveResonanceMoves(
                level, level.KnownSolution), Is.GreaterThanOrEqualTo(1),
                "A visual pulse must move a real remote normal piece.");
            Assert.That(LevelRouteSimilarityAudit.LongestSameAxisRun(
                level.KnownSolution), Is.LessThan(4),
                "The recorded optimal route must not be a repeated straight-lane run.");

            var board = new BoardManager();
            board.Load(level);
            int multiPieceTaps = 0, gateOpenings = 0, remotePulses = 0;
            foreach (var tap in level.KnownSolution)
            {
                Assert.That(board.RequestMove(tap), Is.True,
                    "Noncommitting tap in " + level.name + ": " + tap);
                Assert.That(board.LastReactionWasCancelled, Is.False);
                var moved = new HashSet<int>();
                foreach (var action in board.Actions)
                {
                    if (action.Type == BoardActionType.Move)
                        moved.Add(action.PieceId);
                    if (action.Type == BoardActionType.GateOpened)
                        gateOpenings++;
                    if (action.Type == BoardActionType.ResonancePulse)
                        remotePulses++;
                }
                if (moved.Count >= 2) multiPieceTaps++;
                board.CompleteResolution();
            }
            Assert.That(board.State, Is.EqualTo(GameState.Won));
            Assert.That(multiPieceTaps, Is.GreaterThanOrEqualTo(minimumMultiPieceTaps));
            Assert.That(remotePulses, Is.GreaterThan(0));
            Assert.That(gateOpenings, Is.GreaterThanOrEqualTo(1));
            Assert.That(level.VerifiedOptimalMoveCount, Is.Null,
                "Release-only Perfect fingerprints must remain unavailable.");
        }

        [Test]
        public void Level71AlternateRouteMustChooseManualProgressInsteadOfThePulse()
        {
            var level = Chapter4ResonanceExtendedDrafts.Get(71);
            Assert.That(level, Is.Not.Null);
            var route = Chapter4ResonanceExtendedDrafts.AlternateWinningRoute(71);
            Assert.That(route, Is.Not.Null);
            var board = new BoardManager();
            board.Load(level);
            var remoteBlueMoved = false;
            foreach (var tap in route)
            {
                Assert.That(board.RequestMove(tap), Is.True);
                Assert.That(board.LastReactionWasCancelled, Is.False);
                foreach (var action in board.Actions)
                    if (action.Type == BoardActionType.ResonancePulse)
                        foreach (var piece in board.Pieces)
                            if (piece.Active && piece.Type == PieceType.Normal &&
                                piece.Color == PieceColor.Blue &&
                                piece.Position == action.To)
                                remoteBlueMoved = true;
                board.CompleteResolution();
            }
            Assert.That(board.State, Is.EqualTo(GameState.Won));
            Assert.That(remoteBlueMoved, Is.False,
                "The longer route should manually move the blue piece before signal.");
        }

        [Test]
        public void Level78SharedExitMustClearTheHelperAndKeepTheGateReadable()
        {
            var level = Chapter4ResonanceExtendedDrafts.Get(78);
            Assert.That(level, Is.Not.Null);
            Assert.That(level.ResolvedTargetColors.Count, Is.EqualTo(2));
            int sharedExitCount = 0;
            foreach (var piece in level.Placements)
                if (piece.type == PieceType.Exit && piece.color == PieceColor.None)
                    sharedExitCount++;
            Assert.That(sharedExitCount, Is.EqualTo(1),
                "The same neutral exit must accept the yellow helper and blue target.");

            var board = new BoardManager();
            board.Load(level);
            bool helperDelivered = false, blueDelivered = false, gateOpened = false;
            foreach (var tap in level.KnownSolution)
            {
                Assert.That(board.RequestMove(tap), Is.True);
                Assert.That(board.LastReactionWasCancelled, Is.False);
                bool gateClosed = false;
                foreach (var action in board.Actions)
                {
                    if (action.Type == BoardActionType.GateOpened) gateOpened = true;
                    if (action.Type == BoardActionType.GateClosed) gateClosed = true;
                    if (action.Type == BoardActionType.Deliver)
                    {
                        if (action.PieceId == 3) helperDelivered = true;
                        if (board.Pieces[action.PieceId].Color == PieceColor.Blue)
                        {
                            Assert.That(helperDelivered, Is.True,
                                "Blue must share the exit only after the helper clears it.");
                            blueDelivered = true;
                        }
                    }
                }
                if (gateClosed)
                    Assert.That(board.GetOccupant(new GridPosition(3, 3)), Is.Null,
                        "Never close the gate underneath a movable piece.");
                board.CompleteResolution();
            }
            Assert.That(gateOpened, Is.True);
            Assert.That(helperDelivered, Is.True);
            Assert.That(blueDelivered, Is.True);
            Assert.That(board.State, Is.EqualTo(GameState.Won));
        }

        [Test]
        public void AllChapter4ExtendedLevelsMustHaveARealRemoteMove()
        {
            for (int n = 66; n <= 80; n++)
            {
                var level = Chapter4ResonanceExtendedDrafts.Get(n);
                Assert.That(level, Is.Not.Null, "Missing Level " + n);
                Assert.That(Post40QualityAudit.CountProductiveResonanceMoves(
                    level, level.KnownSolution), Is.GreaterThanOrEqualTo(1),
                    "Level " + n + " must not use decorative-only Resonance.");
            }
        }

        [TestCase(71)]
        [TestCase(74)]
        [TestCase(76)]
        [TestCase(79)]
        public void SlowerWinningRouteMayReplaceTheRemoteShortcutWithManualMoves(int n)
        {
            var level = Chapter4ResonanceExtendedDrafts.Get(n);
            Assert.That(level, Is.Not.Null);
            var slower = Chapter4ResonanceExtendedDrafts.AlternateWinningRoute(n);
            Assert.That(slower, Is.Not.Null);
            Assert.That(slower.Length, Is.EqualTo(level.KnownSolutionLength + 1));
            Assert.That(Chapter4ResonanceExtendedDrafts.CheckAlternate(
                level, n, out string error), Is.True, error);
            Assert.That(Post40QualityAudit.CountProductiveResonanceMoves(
                level, slower), Is.EqualTo(0),
                "The longer path should win by manual work instead of the remote shortcut.");
            Assert.That(Post40QualityAudit.CountProductiveResonanceMoves(
                level, level.KnownSolution), Is.GreaterThanOrEqualTo(1));
        }

        [Test]
        public void Level73MustUseBothSignalsToUnlockTheFinalGate()
        {
            var level = Chapter4ResonanceExtendedDrafts.Get(73);
            Assert.That(level, Is.Not.Null);
            Assert.That(level.KnownSolutionLength, Is.EqualTo(6));
            Assert.That(level.ResolvedTargetColors.Count, Is.EqualTo(3));
            Assert.That(LevelValidation.VerifySolution(level, out string error, out _),
                Is.True, error);
            Assert.That(Chapter4ResonanceExtendedDrafts.CheckAlternate(level, 73, out error),
                Is.True, error);
            var proof = CampaignCertification.Prove(level, 73);
            Assert.That(proof.complete, Is.True, proof.error);
            Assert.That(proof.minimum, Is.EqualTo(6));
            Assert.That(Post40QualityAudit.CountProductiveResonanceMoves(
                level, level.KnownSolution), Is.EqualTo(2));

            var board = new BoardManager();
            board.Load(level);
            var channelByTap = new List<int>();
            int gateOpenedAt = 0, blueMoves = 0, yellowMoves = 0;
            int step = 0;
            foreach (var tap in level.KnownSolution)
            {
                step++;
                Assert.That(board.RequestMove(tap), Is.True);
                Assert.That(board.LastReactionWasCancelled, Is.False);
                foreach (var action in board.Actions)
                {
                    if (action.Type == BoardActionType.ResonancePulse)
                        channelByTap.Add(board.Pieces[action.PieceId].Channel);
                    if (action.Type == BoardActionType.GateOpened)
                        gateOpenedAt = step;
                    if (action.Type == BoardActionType.Move && action.PieceId == 1)
                        blueMoves++;
                    if (action.Type == BoardActionType.Move && action.PieceId == 2)
                        yellowMoves++;
                }
                board.CompleteResolution();
            }
            Assert.That(board.State, Is.EqualTo(GameState.Won));
            Assert.That(channelByTap, Is.EqualTo(new[] { 1, 2 }));
            Assert.That(gateOpenedAt, Is.EqualTo(3),
                "The second signal should move yellow onto its switch.");
            Assert.That(blueMoves, Is.GreaterThanOrEqualTo(2));
            Assert.That(yellowMoves, Is.GreaterThanOrEqualTo(4));
            Assert.That(LevelRouteSimilarityAudit.LongestSameAxisRun(
                level.KnownSolution), Is.LessThan(4));
            Assert.That(level.VerifiedOptimalMoveCount, Is.Null);
        }

        [Test]
        public void Level75MustPushRedForwardThenSignalBlueToOpenBothGates()
        {
            var level = Chapter4ResonanceExtendedDrafts.Get(75);
            Assert.That(level, Is.Not.Null);
            Assert.That(level.KnownSolutionLength, Is.EqualTo(7));
            Assert.That(level.ResolvedTargetColors.Count, Is.EqualTo(3));
            Assert.That(LevelValidation.VerifySolution(level, out string error, out _),
                Is.True, error);
            Assert.That(Chapter4ResonanceExtendedDrafts.CheckAlternate(level, 75, out error),
                Is.True, error);
            var proof = CampaignCertification.Prove(level, 75);
            Assert.That(proof.complete, Is.True, proof.error);
            Assert.That(proof.minimum, Is.EqualTo(7));
            Assert.That(Post40QualityAudit.CountProductiveResonanceMoves(
                level, level.KnownSolution), Is.EqualTo(1));

            var board = new BoardManager();
            board.Load(level);
            var opened = new HashSet<int>();
            int redFirstTapMoves = 0, signalStep = 0, multiPieceTaps = 0;
            int step = 0;
            foreach (var tap in level.KnownSolution)
            {
                step++;
                Assert.That(board.RequestMove(tap), Is.True);
                Assert.That(board.LastReactionWasCancelled, Is.False);
                var moved = new HashSet<int>();
                foreach (var action in board.Actions)
                {
                    if (action.Type == BoardActionType.Move)
                    {
                        moved.Add(action.PieceId);
                        if (step == 1 && action.PieceId == 1)
                            redFirstTapMoves++;
                    }
                    if (action.Type == BoardActionType.ResonancePulse)
                        signalStep = step;
                    if (action.Type == BoardActionType.GateOpened)
                        opened.Add(board.Pieces[action.PieceId].Channel);
                }
                if (moved.Count >= 2) multiPieceTaps++;
                board.CompleteResolution();
            }
            Assert.That(board.State, Is.EqualTo(GameState.Won));
            Assert.That(redFirstTapMoves, Is.EqualTo(2),
                "Yellow's push should set red on the correct trajectory.");
            Assert.That(signalStep, Is.EqualTo(2),
                "Signal should be a planned second decision, not an opening auto-win.");
            Assert.That(opened.SetEquals(new[] { 1, 2 }), Is.True);
            Assert.That(multiPieceTaps, Is.GreaterThanOrEqualTo(2));
            Assert.That(LevelRouteSimilarityAudit.LongestSameAxisRun(
                level.KnownSolution), Is.LessThan(4));
            Assert.That(level.VerifiedOptimalMoveCount, Is.Null);
        }

        [Test]
        public void Level70MustDeliverTwoDistinctRemoteReactionsOnSeparateTaps()
        {
            var level = Chapter4ResonanceExtendedDrafts.Get(70);
            Assert.That(level, Is.Not.Null);
            Assert.That(level.ResolvedTargetColors.Count, Is.EqualTo(3));
            Assert.That(level.KnownSolutionLength, Is.EqualTo(6));
            Assert.That(LevelValidation.VerifySolution(level, out string error, out _),
                Is.True, error);
            Assert.That(Chapter4ResonanceExtendedDrafts.CheckAlternate(
                level, 70, out error), Is.True, error);
            var proof = CampaignCertification.Prove(level, 70);
            Assert.That(proof.complete, Is.True, proof.error);
            Assert.That(proof.minimum, Is.EqualTo(6));

            var board = new BoardManager();
            board.Load(level);
            var distinctChannels = new HashSet<int>();
            var pulseSteps = new List<int>();
            int multiPieceTaps = 0;
            int step = 0;
            foreach (var tap in level.KnownSolution)
            {
                step++;
                Assert.That(board.RequestMove(tap), Is.True);
                Assert.That(board.LastReactionWasCancelled, Is.False);
                var moved = new HashSet<int>();
                foreach (var action in board.Actions)
                {
                    if (action.Type == BoardActionType.Move)
                        moved.Add(action.PieceId);
                    if (action.Type == BoardActionType.ResonancePulse)
                    {
                        pulseSteps.Add(step);
                        distinctChannels.Add(board.Pieces[action.PieceId].Channel);
                    }
                }
                if (moved.Count >= 2) multiPieceTaps++;
                board.CompleteResolution();
            }
            Assert.That(board.State, Is.EqualTo(GameState.Won));
            Assert.That(pulseSteps, Is.EqualTo(new[] { 2, 5 }),
                "Two signals must require separate player decisions.");
            Assert.That(distinctChannels.SetEquals(new[] { 1, 2 }), Is.True);
            Assert.That(multiPieceTaps, Is.GreaterThanOrEqualTo(2));
            Assert.That(Post40QualityAudit.CountProductiveResonanceMoves(
                level, level.KnownSolution), Is.EqualTo(2));
            var rhythm = Post40QualityAudit.AnalyzeRhythm(level, level.KnownSolution);
            Assert.That(rhythm.firstPulseStep, Is.EqualTo(2));
            Assert.That(rhythm.lastPulseStep, Is.EqualTo(5));
            Assert.That(rhythm.separatePulseSteps, Is.EqualTo(2));
            Assert.That(rhythm.longestSinglePieceRun, Is.LessThanOrEqualTo(2));
            Assert.That(level.VerifiedOptimalMoveCount, Is.Null);
        }

        [Test]
        public void Level80MustFinishWithGatedBlueToBlueHandoff()
        {
            var level = Chapter4ResonanceExtendedDrafts.Get(80);
            Assert.That(level, Is.Not.Null);
            Assert.That(level.KnownSolutionLength, Is.EqualTo(8));
            Assert.That(Chapter4ResonanceExtendedDrafts.CheckAlternate(
                level, 80, out string error), Is.True, error);
            Assert.That(LevelValidation.VerifySolution(level, out error, out _),
                Is.True, error);
            var proof = CampaignCertification.Prove(level, 80);
            Assert.That(proof.complete, Is.True, proof.error);
            Assert.That(proof.minimum, Is.EqualTo(8));
            int blueTargets = 0;
            foreach (var piece in level.Placements)
                if (piece.type == PieceType.Normal && piece.color == PieceColor.Blue)
                    blueTargets++;
            Assert.That(blueTargets, Is.EqualTo(2),
                "The finale must preserve two blue targets, not a decorative helper.");

            var board = new BoardManager();
            board.Load(level);
            int step = 0, deliveredBlue = 0;
            bool lastBlueWasPushed = false;
            foreach (var tap in level.KnownSolution)
            {
                step++;
                Assert.That(board.RequestMove(tap), Is.True);
                Assert.That(board.LastReactionWasCancelled, Is.False);
                bool bluePushed = false, blueFollowed = false;
                foreach (var action in board.Actions)
                {
                    if (action.Type == BoardActionType.Deliver &&
                        board.Pieces[action.PieceId].Color == PieceColor.Blue)
                        deliveredBlue++;
                    if (step == 7 && action.Type == BoardActionType.Move &&
                        action.PieceId == 14 &&
                        action.From == new GridPosition(6, 3) &&
                        action.To == new GridPosition(7, 3))
                        bluePushed = true;
                    if (step == 7 && action.Type == BoardActionType.Move &&
                        action.PieceId == 1 &&
                        action.From == new GridPosition(5, 3) &&
                        action.To == new GridPosition(6, 3))
                        blueFollowed = true;
                }
                if (step == 7)
                    lastBlueWasPushed = bluePushed && blueFollowed;
                board.CompleteResolution();
            }
            Assert.That(board.State, Is.EqualTo(GameState.Won));
            Assert.That(deliveredBlue, Is.EqualTo(2));
            Assert.That(lastBlueWasPushed, Is.True,
                "The endgame must push the prepared second blue target out.");
            Assert.That(Post40QualityAudit.CountProductiveResonanceMoves(
                level, level.KnownSolution), Is.GreaterThanOrEqualTo(2));
            var rhythm = Post40QualityAudit.AnalyzeRhythm(level, level.KnownSolution);
            Assert.That(rhythm.finalThirdMultiPieceTaps, Is.GreaterThanOrEqualTo(1));
            Assert.That(rhythm.separatePulseSteps, Is.EqualTo(2));
            Assert.That(level.VerifiedOptimalMoveCount, Is.Null);
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
            Assert.That(multiPieceTaps, Is.GreaterThanOrEqualTo(3));
            Assert.That(finale.VerifiedOptimalMoveCount, Is.Null);
        }
    }
}
