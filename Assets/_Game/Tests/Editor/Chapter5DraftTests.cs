using System.Collections.Generic;
using NUnit.Framework;
using Shift.Game.Editor;

namespace Shift.Game.Tests
{
    // Engine-level proof contracts for 20 new design candidates.
    // Real player enjoyment and device acceptance are later release gates.
    public sealed class Chapter5DraftTests
    {
        [Test]
        public void TwentyLevelsMustHaveShortestProofAndLongerWinningRoute()
        {
            var layouts = new HashSet<string>();
            for (int n = 81; n <= 100; n++)
            {
                var level = Chapter5Drafts.Get(n);
                Assert.That(level, Is.Not.Null, "Missing Level " + n);
                Assert.That(level.ValidatePlayable(out string message), Is.True,
                    "Level " + n + ": " + message);
                Assert.That(level.MoveLimit, Is.EqualTo(level.KnownSolutionLength + 2));
                Assert.That(LevelValidation.VerifySolution(level, out message,
                    out int maxReactionDepth), Is.True, message);
                Assert.That(maxReactionDepth, Is.GreaterThanOrEqualTo(3),
                    "No meaningful chain presentation in " + n);
                var proof = CampaignCertification.Prove(level, n);
                Assert.That(proof.complete, Is.True,
                    "Level " + n + " not proven: " + proof.error);
                Assert.That(proof.minimum, Is.EqualTo(level.KnownSolutionLength),
                    "Level " + n + " recorded solution is not shortest.");
                Assert.That(Chapter5Drafts.CheckAlternate(level, n, out message),
                    Is.True, "Level " + n + ": " + message);
                Assert.That(Post40QualityAudit.CountProductiveResonanceMoves(
                    level, level.KnownSolution), Is.GreaterThanOrEqualTo(1),
                    "The remote signal must move an actual normal piece: Level " + n);
                var profile = Post40QualityAudit.AnalyzeRhythm(
                    level, level.KnownSolution);
                Assert.That(profile.longestSinglePieceRun, Is.LessThan(4),
                    "Long filler run in Level " + n);
                Assert.That(level.VerifiedOptimalMoveCount, Is.Null,
                    "Do not enable Perfect rewards for unreviewed content.");

                // Layout must actually be different, not a new file name
                // attached to identical tiles from a prior draft level.
                var signature = level.Width + "x" + level.Height + ":";
                foreach (var tile in level.Placements)
                    signature += (int)tile.type + "," + (int)tile.color +
                        "," + (int)tile.direction + "," + tile.position.x +
                        "," + tile.position.y + "," + tile.channel + "," +
                        tile.initialOpen + ";";
                Assert.That(layouts.Add(signature), Is.True,
                    "Duplicated Chapter 5 board at level " + n);
            }
        }

        [Test]
        public void DifficultyShouldRiseThroughChoicesNotHugeMoveBudgets()
        {
            for (int n = 81; n <= 100; n++)
            {
                var l = Chapter5Drafts.Get(n);
                Assert.That(l, Is.Not.Null);
                Assert.That(l.Width, Is.LessThanOrEqualTo(8));
                Assert.That(l.Height, Is.LessThanOrEqualTo(8));
                Assert.That(l.KnownSolutionLength, Is.InRange(7, 10));
                Assert.That(l.MoveLimit, Is.LessThanOrEqualTo(12));
                Assert.That(l.ResolvedTargetColors.Count, Is.InRange(2, 3));
            }
        }

        [Test]
        public void Level97MustUseBothRemoteSignalsAndTheYellowConvoy()
        {
            var level = Chapter5Drafts.Get(97);
            Assert.That(level, Is.Not.Null);
            Assert.That(level.KnownSolutionLength, Is.EqualTo(9));
            Assert.That(level.MoveLimit, Is.EqualTo(11));
            Assert.That(Chapter5Drafts.AlternateWinningRoute(97).Length,
                Is.EqualTo(10));
            Assert.That(LevelValidation.VerifySolution(level,
                out string error, out _), Is.True, error);
            Assert.That(Chapter5Drafts.CheckAlternate(level, 97,
                out error), Is.True, error);
            var proof = CampaignCertification.Prove(level, 97);
            Assert.That(proof.complete, Is.True, proof.error);
            Assert.That(proof.minimum, Is.EqualTo(9));
            Assert.That(Post40QualityAudit.CountProductiveResonanceMoves(
                level, level.KnownSolution), Is.EqualTo(2));

            var board = new BoardManager();
            board.Load(level);
            var signalChannels = new HashSet<int>();
            var pulseSteps = new List<int>();
            var yellowDeliveries = new List<int>();
            int multi = 0, yellowTargets = 0, tapNumber = 0;
            foreach (var p in level.Placements)
                if (p.type == PieceType.Normal &&
                    p.color == PieceColor.Yellow)
                    yellowTargets++;
            Assert.That(yellowTargets, Is.EqualTo(2));
            foreach (var tap in level.KnownSolution)
            {
                tapNumber++;
                Assert.That(board.RequestMove(tap), Is.True);
                Assert.That(board.LastReactionWasCancelled, Is.False);
                var moved = new HashSet<int>();
                foreach (var action in board.Actions)
                {
                    if (action.Type == BoardActionType.Move)
                        moved.Add(action.PieceId);
                    if (action.Type == BoardActionType.ResonancePulse)
                    {
                        signalChannels.Add(board.Pieces[action.PieceId].Channel);
                        pulseSteps.Add(tapNumber);
                    }
                    if (action.Type == BoardActionType.Deliver &&
                        board.Pieces[action.PieceId].Color == PieceColor.Yellow)
                        yellowDeliveries.Add(tapNumber);
                }
                if (moved.Count > 1) multi++;
                board.CompleteResolution();
            }
            Assert.That(board.State, Is.EqualTo(GameState.Won));
            Assert.That(multi, Is.GreaterThanOrEqualTo(5));
            Assert.That(signalChannels.SetEquals(new[] { 1, 2 }), Is.True);
            Assert.That(pulseSteps, Is.EqualTo(new[] { 1, 6 }));
            Assert.That(yellowDeliveries, Is.EqualTo(new[] { 8, 9 }));
            Assert.That(Post40QualityAudit.AnalyzeRhythm(
                level, level.KnownSolution).longestSinglePieceRun,
                Is.LessThanOrEqualTo(2));
            var overlap = Chapter5Drafts.ScanMotifSimilarities(.75f);
            Assert.That(overlap.candidatePairs.Exists(pair =>
                pair.first == 95 && pair.second == 97), Is.False,
                "Do not restore identical 95/97 player-decision rhythms.");
        }

        [Test]
        public void HundredMustConcludeWithThreeColorsTwoSignalsTwoGatesAndRealPushes()
        {
            var l = Chapter5Drafts.Get(100);
            Assert.That(l, Is.Not.Null);
            Assert.That(l.KnownSolutionLength, Is.EqualTo(9));
            Assert.That(Chapter5Drafts.AlternateWinningRoute(100).Length,
                Is.EqualTo(10));
            Assert.That(l.ResolvedTargetColors.Count, Is.EqualTo(3));
            var resonance = new HashSet<int>();
            var switches = new HashSet<int>();
            var gates = new HashSet<int>();
            int targets = 0;
            foreach (var p in l.Placements)
            {
                if (p.type == PieceType.Resonance) resonance.Add(p.channel);
                if (p.type == PieceType.Switch) switches.Add(p.channel);
                if (p.type == PieceType.Gate) gates.Add(p.channel);
                if (p.type == PieceType.Normal && l.IsTargetColor(p.color)) targets++;
            }
            Assert.That(resonance.SetEquals(new[] { 1, 2 }), Is.True);
            Assert.That(switches.SetEquals(new[] { 1, 2 }), Is.True);
            Assert.That(gates.SetEquals(new[] { 1, 2 }), Is.True);
            Assert.That(targets, Is.EqualTo(5));
            Assert.That(Post40QualityAudit.CountProductiveResonanceMoves(
                l, l.KnownSolution), Is.GreaterThanOrEqualTo(2));
            var board = new BoardManager();
            board.Load(l);
            int multipieceTaps = 0, opened = 0, delivered = 0;
            foreach (var tap in l.KnownSolution)
            {
                Assert.That(board.RequestMove(tap), Is.True);
                Assert.That(board.LastReactionWasCancelled, Is.False);
                var moved = new HashSet<int>();
                foreach (var action in board.Actions)
                {
                    if (action.Type == BoardActionType.Move)
                        moved.Add(action.PieceId);
                    if (action.Type == BoardActionType.GateOpened) opened++;
                    if (action.Type == BoardActionType.Deliver) delivered++;
                }
                if (moved.Count >= 2) multipieceTaps++;
                board.CompleteResolution();
            }
            Assert.That(board.State, Is.EqualTo(GameState.Won));
            Assert.That(multipieceTaps, Is.GreaterThanOrEqualTo(4));
            Assert.That(opened, Is.GreaterThanOrEqualTo(2));
            Assert.That(delivered, Is.EqualTo(5));
        }
    }
}
