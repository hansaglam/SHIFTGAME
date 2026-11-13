using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Shift.Game.Editor;
using UnityEngine;

namespace Shift.Game.Tests
{
    public sealed class Chapter6RelayPilotTests
    {
        [Test]
        public void RelayPrototypeMustNotContainAnySwitchesOrGates()
        {
            for (int n = 101; n <= 103; n++)
            {
                var level = Chapter6RelayPilot.Get(n);
                Assert.That(level, Is.Not.Null);
                Assert.That(level.ValidatePlayable(out string error),
                    Is.True, error);
                Assert.That(level.Placements.Any(p =>
                    p.type == PieceType.Switch || p.type == PieceType.Gate),
                    Is.False, "RÖLE cannot be a disguised gate puzzle.");
                Assert.That(level.Placements.Count(p =>
                    p.type == PieceType.Relay), Is.EqualTo(1),
                    "Exactly one unpaired hourglass in each pilot.");
                Assert.That(MechanicGuideCatalog.Contains(
                    level, MechanicTopic.Relay), Is.True);
            }
        }

        [Test]
        public void RelayCannotBeTappedDirectlyWhileWaiting()
        {
            var level = Chapter6RelayPilot.Get(101);
            var board = new BoardManager();
            board.Load(level);
            Assert.That(board.RequestMove(new GridPosition(0, 1)), Is.True);
            Assert.That(board.Pieces[0].RelayReady, Is.True);
            Assert.That(board.Actions.Count(a =>
                a.Type == BoardActionType.RelayArmed), Is.EqualTo(1));
            board.CompleteResolution();
            int before = board.MovesRemaining;
            Assert.That(board.RequestMove(new GridPosition(1, 1)), Is.False);
            Assert.That(board.MovesRemaining, Is.EqualTo(before));
            Assert.That(board.Pieces[0].RelayReady, Is.True);

            Assert.That(board.RequestMove(new GridPosition(0, 2)), Is.True);
            Assert.That(board.Pieces[0].Position,
                Is.EqualTo(new GridPosition(2, 1)));
            Assert.That(board.Pieces[0].RelayReady, Is.False);
            Assert.That(board.Pieces[1].Position,
                Is.EqualTo(new GridPosition(1, 2)));
            Assert.That(board.Actions.Count(a =>
                a.Type == BoardActionType.RelayReleased), Is.EqualTo(1));
            Assert.That(board.MovesRemaining, Is.EqualTo(before - 1),
                "Two pieces move, only one tap is consumed.");
        }

        [Test]
        public void UndoRestoresPendingStateAndExactReaction()
        {
            var level = Chapter6RelayPilot.Get(101);
            var board = new BoardManager();
            board.Load(level);
            Assert.That(board.RequestMove(new GridPosition(0, 1)), Is.True);
            board.CompleteResolution();
            Assert.That(board.RequestMove(new GridPosition(0, 2)), Is.True);
            var before = board.Actions.Select(a =>
                a.Type + ":" + a.PieceId + ":" + a.To).ToArray();
            board.CompleteResolution();

            Assert.That(board.Undo(), Is.True);
            Assert.That(board.Pieces[0].RelayReady, Is.True);
            Assert.That(board.Pieces[0].Position,
                Is.EqualTo(new GridPosition(1, 1)));
            Assert.That(board.Pieces[1].Position,
                Is.EqualTo(new GridPosition(0, 2)));
            Assert.That(board.RequestMove(new GridPosition(0, 2)), Is.True);
            var again = board.Actions.Select(a =>
                a.Type + ":" + a.PieceId + ":" + a.To).ToArray();
            Assert.That(again, Is.EqualTo(before));
        }

        [Test]
        public void BlockedOtherTapDoesNotReleaseWaitingStoneOrSpendMove()
        {
            var level = ScriptableObject.CreateInstance<LevelData>();
            try
            {
                level.Configure(4, 3, 4, PieceColor.Red,
                    new[] {
                        new PiecePlacement(PieceType.Normal,
                            PieceColor.Red, Direction.Right,
                            new GridPosition(0,1)),
                        new PiecePlacement(PieceType.Normal,
                            PieceColor.Blue, Direction.Left,
                            new GridPosition(0,2)),
                        new PiecePlacement(PieceType.Relay,
                            PieceColor.None, Direction.None,
                            new GridPosition(1,1)),
                        new PiecePlacement(PieceType.Exit,
                            PieceColor.Red, Direction.None,
                            new GridPosition(3,1))
                    });
                Assert.That(level.ValidatePlayable(out string error),
                    Is.True, error);
                var board = new BoardManager();
                board.Load(level);
                Assert.That(board.RequestMove(new GridPosition(0,1)),
                    Is.True);
                board.CompleteResolution();
                int before = board.MovesRemaining;
                Assert.That(board.RequestMove(new GridPosition(0,2)),
                    Is.True);
                Assert.That(board.Actions.Count, Is.Zero);
                Assert.That(board.MovesRemaining, Is.EqualTo(before));
                Assert.That(board.Pieces[0].RelayReady, Is.True);
                Assert.That(board.Pieces[0].Position,
                    Is.EqualTo(new GridPosition(1,1)));
            }
            finally { Object.DestroyImmediate(level); }
        }

        [Test]
        public void MissingPilotReferencesRecoverOnlyInIsolatedEditorPreview()
        {
            var list = Chapter6RelayPilot.Load103();
            var copy = (LevelData[])list.Clone();
            copy[100] = null;
            copy[101] = null;
            copy[102] = null;
            Assert.That(RelayPreviewAssetRepair.Restore(copy,
                "SHIFT.Progress.v1."), Is.Zero);
            Assert.That(copy[100], Is.Null);
            Assert.That(RelayPreviewAssetRepair.Restore(copy,
                Chapter6RelayPilot.SavePrefix), Is.EqualTo(3));
            for (int i = 0; i < 103; i++)
                Assert.That(copy[i], Is.SameAs(list[i]));
            Assert.That(RelayPreviewAssetRepair.Restore(copy,
                Chapter6RelayPilot.SavePrefix), Is.Zero);
        }

        [Test]
        public void ADeferredMoveCanPushAnotherNormalToItsExit()
        {
            var level = Chapter6RelayPilot.Get(102);
            var board = new BoardManager();
            board.Load(level);
            Assert.That(board.RequestMove(new GridPosition(0, 1)), Is.True);
            board.CompleteResolution();
            Assert.That(board.RequestMove(new GridPosition(0, 2)), Is.True);
            var moved = new HashSet<int>(board.Actions
                .Where(a => a.Type == BoardActionType.Move)
                .Select(a => a.PieceId));
            Assert.That(moved.SetEquals(new[] {0, 1, 2}), Is.True);
            Assert.That(board.Pieces[1].Active, Is.False,
                "Relay-driven red should push blue into its exit.");
            Assert.That(board.Pieces[2].Active, Is.False);
            Assert.That(board.Pieces[0].Position,
                Is.EqualTo(new GridPosition(2, 1)));
            Assert.That(board.Actions.Count(a =>
                a.Type == BoardActionType.RelayReleased), Is.EqualTo(1));
            board.CompleteResolution();
            Assert.That(board.RequestMove(new GridPosition(2, 1)), Is.True);
            board.CompleteResolution();
            Assert.That(board.State, Is.EqualTo(GameState.Won));
        }

        [Test]
        public void RelayAndResonanceTripleDeliveryRequiresOnlyTwoTaps()
        {
            var level = Chapter6RelayPilot.Get(103);
            var board = new BoardManager();
            board.Load(level);
            Assert.That(board.RequestMove(new GridPosition(0, 1)), Is.True);
            board.CompleteResolution();
            Assert.That(board.RequestMove(new GridPosition(0, 2)), Is.True);
            var moved = new HashSet<int>(board.Actions
                .Where(a => a.Type == BoardActionType.Move)
                .Select(a => a.PieceId));
            Assert.That(moved.SetEquals(new[] {0, 1, 2}), Is.True);
            Assert.That(board.Actions.Count(a =>
                a.Type == BoardActionType.ResonancePulse), Is.EqualTo(1));
            Assert.That(board.Actions.Count(a =>
                a.Type == BoardActionType.Deliver), Is.EqualTo(3));
            Assert.That(board.Actions.Count(a =>
                a.Type == BoardActionType.RelayReleased), Is.EqualTo(1));
            board.CompleteResolution();
            Assert.That(board.State, Is.EqualTo(GameState.Won));
        }

        [Test]
        public void PilotRoutesRequireTheRealEngineToValidateAndProveMinimum()
        {
            for (int n = 101; n <= 103; n++)
            {
                var level = Chapter6RelayPilot.Get(n);
                Assert.That(LevelValidation.VerifySolution(level,
                    out string error, out _), Is.True, error);
                Assert.That(Chapter6RelayPilot.VerifyAlternative(
                    n, out error), Is.True, error);
                var proof = CampaignCertification.Prove(level, n);
                Assert.That(proof.complete, Is.True, proof.error);
                Assert.That(proof.minimum,
                    Is.EqualTo(level.KnownSolutionLength));
                Assert.That(level.VerifiedOptimalMoveCount, Is.Null);
            }
        }

        [Test]
        public void OldOneHundredAssetReferencesRemainWithoutRelay()
        {
            var old = ChapterJourneyScenePreview.Load100();
            var preview = Chapter6RelayPilot.Load103();
            Assert.That(old.Length, Is.EqualTo(100));
            Assert.That(preview.Length, Is.EqualTo(103));
            for (int i = 0; i < 100; i++)
            {
                Assert.That(preview[i], Is.SameAs(old[i]));
                Assert.That(old[i].Placements.All(p =>
                    p.type != PieceType.Relay), Is.True);
            }
        }

        [TestCase(GameLanguage.English)]
        [TestCase(GameLanguage.Turkish)]
        public void RelayFirstEncounterAndLevelHintsAreLocalized(
            GameLanguage language)
        {
            foreach (var suffix in new[] {".name", ".body", ".example"})
            {
                string key = "guide.relay" + suffix;
                Assert.That(LocalizationCatalog.Format(key, language),
                    Is.Not.EqualTo(key));
            }
            for (int n = 101; n <= 103; n++)
            {
                var level = Chapter6RelayPilot.Get(n);
                Assert.That(RelayPilotLocalization.TryTitle(
                    level, language, out string title), Is.True);
                Assert.That(RelayPilotLocalization.TryHint(
                    level, language, out string hint), Is.True);
                Assert.That(title, Does.StartWith(n + " - "));
                Assert.That(hint.Length, Is.GreaterThan(25));
            }
        }
    }
}
