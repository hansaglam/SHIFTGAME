using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Shift.Game.Editor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Shift.Game.Tests
{
    // This suite is deliberately mechanical, not a player enjoyment score.
    // The first 100 published/draft puzzle rules must remain unaffected.
    public sealed class TetherMechanicTests
    {
        private LevelData scratch;
        private static GridPosition G(int x, int y) => new GridPosition(x, y);
        private static PiecePlacement P(PieceType type, PieceColor color,
            Direction direction, int x, int y,
            int channel = 0, int tetherId = 0)
            => new PiecePlacement(type, color, direction, G(x, y),
                channel: channel, tetherId: tetherId);

        [TearDown]
        public void TearDown()
        {
            if (scratch != null) Object.DestroyImmediate(scratch);
            scratch = null;
        }

        private LevelData Level(params PiecePlacement[] p)
        {
            scratch = ScriptableObject.CreateInstance<LevelData>();
            scratch.Configure(7, 5, 7, PieceColor.Red, p);
            return scratch;
        }

        [Test]
        public void TetherRequiresTwoNormalEndpointsWithinThreeManhattanCells()
        {
            var level = Level(
                P(PieceType.Normal, PieceColor.Red, Direction.Right,
                    3, 1, tetherId: 1),
                P(PieceType.Exit, PieceColor.Red, Direction.None, 6, 1));
            Assert.That(level.Validate(out string error), Is.False);
            Assert.That(error, Does.Contain("exactly two"));

            level.Configure(7, 5, 5, PieceColor.Red, new[]
            {
                P(PieceType.Normal, PieceColor.Red, Direction.Right,
                    6, 1, tetherId: 1),
                P(PieceType.Normal, PieceColor.Blue, Direction.Up,
                    0, 1, tetherId: 1),
                P(PieceType.Exit, PieceColor.Red, Direction.None, 6, 4)
            });
            Assert.That(level.Validate(out error), Is.False);
            Assert.That(error, Does.Contain("1-3"));

            level.Configure(7, 5, 5, PieceColor.Red, new[]
            {
                P(PieceType.Normal, PieceColor.Red, Direction.Right,
                    2, 1),
                P(PieceType.Direction, PieceColor.None, Direction.Up,
                    3, 1, tetherId: 1),
                P(PieceType.Exit, PieceColor.Red, Direction.None, 6, 1)
            });
            Assert.That(level.Validate(out error), Is.False);
            Assert.That(error, Does.Contain("only belong to a Normal"));
        }

        [Test]
        public void TetheredFirstTapOpensSwitchAndMovesBothPiecesAtOneTapCost()
        {
            var level = Chapter6TetherPrototypes.Get(101);
            Assert.That(level, Is.Not.Null);
            var board = new BoardManager();
            board.Load(level);
            Assert.That(BoardManager.TetherMaxDistance, Is.EqualTo(3));
            Assert.That(board.GetTetherPartner(0), Is.SameAs(board.Pieces[1]));
            Assert.That(board.GetTetherPartner(1), Is.SameAs(board.Pieces[0]));
            Assert.That(board.GetTetherPartner(2), Is.Null);
            Assert.That(board.GetTerrain(G(5, 1)).GateOpen, Is.False);

            Assert.That(board.RequestMove(G(3, 1)), Is.True);
            Assert.That(board.LastReactionWasCancelled, Is.False);
            Assert.That(board.Pieces[0].Position, Is.EqualTo(G(4, 1)));
            Assert.That(board.Pieces[1].Position, Is.EqualTo(G(1, 1)));
            Assert.That(board.GetTerrain(G(5, 1)).GateOpen, Is.True);
            Assert.That(board.Actions.Count(a =>
                a.Type == BoardActionType.TetherPull), Is.EqualTo(1));
            Assert.That(board.Actions.Count(a =>
                a.Type == BoardActionType.Move), Is.EqualTo(2));
            Assert.That(board.MovesRemaining, Is.EqualTo(level.MoveLimit - 1));
        }

        [Test]
        public void UndoRestoresBothPartnersAndSwitchGateAtomically()
        {
            var level = Chapter6TetherPrototypes.Get(101);
            var board = new BoardManager();
            board.Load(level);
            Assert.That(board.RequestMove(G(3, 1)), Is.True);
            var first = board.Actions.Select(a =>
                a.Type + ":" + a.PieceId + ":" + a.From + ":" + a.To)
                .ToArray();
            board.CompleteResolution();
            Assert.That(board.Undo(), Is.True);
            Assert.That(board.Pieces[0].Position, Is.EqualTo(G(3, 1)));
            Assert.That(board.Pieces[1].Position, Is.EqualTo(G(0, 1)));
            Assert.That(board.GetTerrain(G(5, 1)).GateOpen, Is.False);
            Assert.That(board.MovesRemaining, Is.EqualTo(level.MoveLimit));
            Assert.That(board.RequestMove(G(3, 1)), Is.True);
            var repeated = board.Actions.Select(a =>
                a.Type + ":" + a.PieceId + ":" + a.From + ":" + a.To)
                .ToArray();
            Assert.That(repeated, Is.EqualTo(first),
                "Same board + same tap must have the exact same action order.");
        }

        [Test]
        public void BlockedPartnerPreventsPullWithoutConsumingAnyTap()
        {
            var level = Chapter6TetherPrototypes.Get(102);
            var board = new BoardManager();
            board.Load(level);
            Assert.That(board.RequestMove(G(3, 1)), Is.True);
            Assert.That(board.Actions, Is.Empty);
            Assert.That(board.MovesRemaining, Is.EqualTo(level.MoveLimit));
            Assert.That(board.Pieces[0].Position, Is.EqualTo(G(3, 1)));
            Assert.That(board.Pieces[1].Position, Is.EqualTo(G(0, 1)));
            board.CompleteResolution();
            Assert.That(board.State, Is.EqualTo(GameState.Playing));

            Assert.That(board.RequestMove(G(0, 1)), Is.True);
            Assert.That(board.LastReactionWasCancelled, Is.False);
            Assert.That(board.Pieces[1].Position, Is.EqualTo(G(0, 2)));
            Assert.That(board.Pieces[0].Position, Is.EqualTo(G(2, 2)),
                "Tether pull must carry Red through the Up direction tile.");
            Assert.That(board.Pieces[0].Direction, Is.EqualTo(Direction.Up));
            Assert.That(board.Actions.Any(a =>
                a.Type == BoardActionType.TetherPull), Is.True);
            board.CompleteResolution();
        }

        [Test]
        public void FailedPushAfterAValidPullRollsBackSwitchAndEveryPiece()
        {
            var level = Level(
                P(PieceType.Normal, PieceColor.Red, Direction.Right,
                    3, 1, tetherId: 1),
                P(PieceType.Normal, PieceColor.Blue, Direction.Right,
                    0, 1, tetherId: 1),
                P(PieceType.PushBlock, PieceColor.None, Direction.None, 4, 1),
                P(PieceType.Wall, PieceColor.None, Direction.None, 5, 1),
                P(PieceType.Switch, PieceColor.None, Direction.None, 1, 1, 1),
                P(PieceType.Gate, PieceColor.None, Direction.None, 3, 3, 1),
                P(PieceType.Exit, PieceColor.Red, Direction.None, 6, 1)
            );
            Assert.That(level.ValidatePlayable(out string error), Is.True, error);
            var board = new BoardManager();
            board.Load(level);
            Assert.That(board.RequestMove(G(3, 1)), Is.True);
            Assert.That(board.LastReactionWasCancelled, Is.False);
            Assert.That(board.Actions, Is.Empty,
                "A failed source push cannot leave a committed pull/switch.");
            Assert.That(board.Pieces[0].Position, Is.EqualTo(G(3, 1)));
            Assert.That(board.Pieces[1].Position, Is.EqualTo(G(0, 1)));
            Assert.That(board.Pieces[2].Position, Is.EqualTo(G(4, 1)));
            Assert.That(board.GetTerrain(G(3, 3)).GateOpen, Is.False);
            Assert.That(board.MovesRemaining, Is.EqualTo(level.MoveLimit));
        }

        [Test]
        public void TetherAndResonanceCanCauseThreeDifferentNormalMoves()
        {
            var level = Chapter6TetherPrototypes.Get(103);
            Assert.That(level, Is.Not.Null);
            var board = new BoardManager();
            board.Load(level);
            Assert.That(board.RequestMove(G(3, 1)), Is.True);
            Assert.That(board.LastReactionWasCancelled, Is.False);
            var moved = new HashSet<int>(board.Actions
                .Where(a => a.Type == BoardActionType.Move)
                .Select(a => a.PieceId));
            Assert.That(moved.SetEquals(new[] { 0, 1, 2 }), Is.True);
            Assert.That(board.Actions.Count(a =>
                a.Type == BoardActionType.ResonancePulse), Is.EqualTo(1));
            Assert.That(board.Actions.Count(a =>
                a.Type == BoardActionType.TetherPull), Is.EqualTo(1));
            Assert.That(board.GetTerrain(G(5, 1)).GateOpen, Is.True);
            Assert.That(board.Pieces[1].Active, Is.False);
            Assert.That(board.Pieces[2].Position, Is.EqualTo(G(1, 3)));
            board.CompleteResolution();
            Assert.That(board.State, Is.EqualTo(GameState.Playing));
        }

        [Test]
        public void TwoGateChallengeMustBlockTetherBeforeFirstSwitch()
        {
            var level = Chapter6TetherPrototypes.Get(104);
            var board = new BoardManager();
            board.Load(level);
            Assert.That(board.RequestMove(G(3, 1)), Is.True);
            Assert.That(board.Actions, Is.Empty);
            board.CompleteResolution();
            Assert.That(board.RequestMove(G(0, 3)), Is.True);
            Assert.That(board.GetTerrain(G(1, 1)).GateOpen, Is.True);
            board.CompleteResolution();
            Assert.That(board.RequestMove(G(3, 1)), Is.True);
            Assert.That(board.Pieces[0].Position, Is.EqualTo(G(4, 1)));
            Assert.That(board.Pieces[1].Position, Is.EqualTo(G(1, 1)));
            board.CompleteResolution();
            Assert.That(board.RequestMove(G(1, 1)), Is.True);
            Assert.That(board.GetTerrain(G(5, 1)).GateOpen, Is.True);
            board.CompleteResolution();
            Assert.That(board.RequestMove(G(4, 1)), Is.True);
            board.CompleteResolution();
            Assert.That(board.RequestMove(G(5, 1)), Is.True);
            Assert.That(board.Actions.Count(a =>
                a.Type == BoardActionType.Deliver), Is.EqualTo(2),
                "Last link pull should deliver Blue then Red on one tap.");
            board.CompleteResolution();
            Assert.That(board.State, Is.EqualTo(GameState.Won));
        }

        [Test]
        public void EachPrototypeHasARealTetherPullAndProvenWinningRoutes()
        {
            for (int number = 101; number <= 104; number++)
            {
                var level = Chapter6TetherPrototypes.Get(number);
                Assert.That(level, Is.Not.Null, "Missing pilot " + number);
                Assert.That(level.ValidatePlayable(out string error), Is.True, error);
                Assert.That(LevelValidation.VerifySolution(level,
                    out error, out int depth), Is.True, error);
                Assert.That(Chapter6TetherPrototypes.VerifyAlternative(
                    level, number, out error), Is.True, error);
                var proof = CampaignCertification.Prove(level, number);
                Assert.That(proof.complete, Is.True, proof.error);
                Assert.That(proof.minimum, Is.EqualTo(level.KnownSolutionLength));
                Assert.That(depth, Is.GreaterThan(2));
                Assert.That(level.VerifiedOptimalMoveCount, Is.Null);
                Assert.That(MechanicGuideCatalog.Contains(level,
                    MechanicTopic.Tether), Is.True);
            }
        }

        [Test]
        public void AllExistingHundredLevelAssetsRemainWithoutTetherLinks()
        {
            var old = ChapterJourneyScenePreview.Load100();
            Assert.That(old.Length, Is.EqualTo(100));
            foreach (var level in old)
            {
                Assert.That(level, Is.Not.Null);
                Assert.That(level.Placements.All(p => p.tetherId == 0),
                    Is.True, "Legacy puzzle must never receive an implicit bond: " +
                    level.name);
            }
        }

        [TestCase(GameLanguage.English)]
        [TestCase(GameLanguage.Turkish)]
        public void TetherRulesAreFullyTranslated(GameLanguage language)
        {
            foreach (var suffix in new[] { ".name", ".body", ".example" })
            {
                string key = "guide.tether" + suffix;
                Assert.That(LocalizationCatalog.Format(key, language),
                    Is.Not.EqualTo(key));
            }
            var example = MechanicGuideCatalog.Example(MechanicTopic.Tether);
            Assert.That(example.Length, Is.InRange(3, 4));
            Assert.That(example.Count(p => p.tetherId == 1), Is.EqualTo(2));
        }
    }
}
