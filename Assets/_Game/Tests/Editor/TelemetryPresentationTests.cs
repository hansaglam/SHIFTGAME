using System.Collections;
using NUnit.Framework;
using Shift.Game.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Shift.Game.Tests
{
    public sealed class TelemetryPresentationTests
    {
        private const string Prefix = "SHIFT.Tests.TelemetryPlay.";
        [TearDown] public void Cleanup() { new SaveService(Prefix).Reset(); new SettingsService(Prefix + "Settings.").Reset(); }
        [UnityTest] public IEnumerator DisabledAnalyticsOverlaySettingsAndAcceptedTapsPreserveGameplay()
        {
            EditorSceneManager.OpenScene(PrototypeSetup.ScenePath); ProgressionPresentationTests.IsolateSave(Prefix);
            var serialized = new SerializedObject(Object.FindFirstObjectByType<PrototypeGame>());
            serialized.FindProperty("showTelemetryOverlay").boolValue = true;
            serialized.FindProperty("analyticsEnabled").boolValue = false; serialized.ApplyModifiedPropertiesWithoutUndo();
            yield return new EnterPlayMode(); yield return null;
            var game = Object.FindFirstObjectByType<PrototypeGame>();
            var overlay = Object.FindFirstObjectByType<TelemetryOverlay>(); Assert.That(overlay, Is.Not.Null);
            Assert.That(overlay.GetComponent<CanvasGroup>().blocksRaycasts, Is.False);
            foreach (var graphic in overlay.GetComponentsInChildren<Graphic>()) Assert.That(graphic.raycastTarget, Is.False);
            game.OpenSettings();
            GameObject.Find("Sound Setting").GetComponent<Button>().onClick.Invoke();
            GameObject.Find("Haptics Setting").GetComponent<Button>().onClick.Invoke();
            GameObject.Find("Reduced Motion Setting").GetComponent<Button>().onClick.Invoke();
            Assert.That(game.GetComponent<AudioManager>().Muted, Is.True);
            Assert.That(new SettingsService(Prefix + "Settings.").ReducedMotion, Is.True);
            SprintPresentationTests.Capture("sprint3-settings.png");
            game.CloseSettings(); game.Restart(); yield return null;
            Assert.That(game.Settings.Sound, Is.False); Assert.That(game.Settings.Haptics, Is.True);
            game.LoadLevel(4); yield return null;
            Button blocked = Find(new GridPosition(0,1)); blocked.onClick.Invoke(); blocked.onClick.Invoke();
            Assert.That(game.Telemetry.Snapshot().taps, Is.EqualTo(1));
            Assert.That(game.Telemetry.Snapshot().blockedTaps, Is.EqualTo(1));
            Assert.That(game.Telemetry.Snapshot().successfulMoves, Is.Zero);
            yield return new WaitForSecondsRealtime(.25f);
            foreach (var tap in game.CurrentLevel.KnownSolution)
            {
                Find(tap).onClick.Invoke(); double deadline = Time.realtimeSinceStartupAsDouble + 4;
                while (game.Board.State == GameState.Resolving && Time.realtimeSinceStartupAsDouble < deadline) yield return null;
                Assert.That(game.Board.State, Is.Not.EqualTo(GameState.Resolving));
            }
            Assert.That(game.Board.State, Is.EqualTo(GameState.Won));
            Assert.That(game.Telemetry.Result.successfulMoves, Is.EqualTo(game.CurrentLevel.MoveLimit - game.Board.MovesRemaining));
            Assert.That(game.Telemetry.Result.blockedTaps, Is.EqualTo(1));
            Assert.That(game.Telemetry.Result.perfectShift, Is.False);
            SprintPresentationTests.Capture("sprint3-overlay.png");
            yield return new ExitPlayMode();
        }
        private static Button Find(GridPosition position)
        {
            foreach (var piece in Object.FindObjectsByType<Piece>(FindObjectsSortMode.None))
                if (piece.Data.Type == PieceType.Normal && piece.Data.Position == position) return piece.GetComponentInChildren<Button>();
            Assert.Fail("Missing piece at " + position); return null;
        }
    }
}
