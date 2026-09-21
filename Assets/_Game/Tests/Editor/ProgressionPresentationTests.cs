using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Shift.Game.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Shift.Game.Tests
{
    public sealed class ProgressionPresentationTests
    {
        private const string Prefix="SHIFT.Tests.ProgressionPresentation.";
        internal static void IsolateSave(string prefix)
        {
            new SaveService(prefix).Reset();
            var serialized=new SerializedObject(Object.FindFirstObjectByType<PrototypeGame>());
            serialized.FindProperty("settingsKeyPrefix").stringValue=prefix + "Settings.";
            new SettingsService(prefix + "Settings.").Reset();
            serialized.FindProperty("saveKeyPrefix").stringValue=prefix; serialized.ApplyModifiedPropertiesWithoutUndo();
        }
        [TearDown] public void Cleanup() { new SaveService(Prefix).Reset(); new SaveService("SHIFT.Tests.LegacyPresentation.").Reset(); }
        [UnityTest] public IEnumerator PlayerProgressionModalSwitchAndFinale()
        {
            EditorSceneManager.OpenScene(PrototypeSetup.ScenePath); IsolateSave(Prefix);
            yield return new EnterPlayMode(); yield return null;
            var game=Object.FindFirstObjectByType<PrototypeGame>();
            Assert.That(game.Progress.Current,Is.Zero); Assert.That(game.SelectLevel(1),Is.False);
            game.OpenChapter();
            Assert.That(GameObject.Find("Level 2").GetComponent<Button>().interactable,Is.False);
            Assert.That(GameObject.Find("Level 1").GetComponent<Button>().interactable,Is.True);
            yield return new WaitForSecondsRealtime(.2f); SprintPresentationTests.Capture("chapter-locked.png"); game.CloseChapter();
            Tap(new GridPosition(1,1)); yield return new WaitForSecondsRealtime(.4f);
            Assert.That(game.Board.State,Is.EqualTo(GameState.Won)); Assert.That(game.Progress.HighestUnlocked,Is.EqualTo(1));
            Assert.That(GameObject.Find("Next Level"),Is.Not.Null);
            yield return new WaitForSecondsRealtime(.2f); SprintPresentationTests.Capture("progression-next.png");
            GameObject.Find("Next Level").GetComponent<Button>().onClick.Invoke(); yield return null;
            Assert.That(game.CurrentLevel.name,Does.StartWith("Level02"));
            Tap(new GridPosition(0,1)); game.OpenChapter();
            Assert.That(game.SelectLevel(0),Is.True); yield return new WaitForSecondsRealtime(.5f);
            Assert.That(game.CurrentLevel.name,Does.StartWith("Level01")); Assert.That(game.Board.MovesRemaining,Is.EqualTo(1));
            Assert.That(game.Board.State,Is.EqualTo(GameState.Playing));
            Assert.That(game.Progress.HighestUnlocked,Is.EqualTo(1));
            Assert.That(Object.FindObjectsByType<GridCell>(FindObjectsSortMode.None).Length,Is.EqualTo(16));
            game.UnlockAllLevels(); game.OpenChapter();
            Assert.That(GameObject.Find("Level 20").GetComponent<Button>().interactable,Is.True);
            yield return new WaitForSecondsRealtime(.2f); SprintPresentationTests.Capture("chapter-unlocked.png");
            Assert.That(game.SelectLevel(19),Is.True); yield return null;
            yield return new WaitForSecondsRealtime(.2f); SprintPresentationTests.Capture("level20-start.png");
            var cues = new HashSet<AudioCue>();
            game.GetComponent<AudioManager>().CuePlayed += cue => cues.Add(cue);
            foreach(var tap in game.CurrentLevel.KnownSolution)
            {
                Tap(tap);
                // Measure from this tap, not the previous frame (which may contain a costly capture).
                float deadline=Time.realtimeSinceStartup+4;
                while(game.Board.State==GameState.Resolving && Time.realtimeSinceStartup<deadline) yield return null;
                Assert.That(game.Board.State,Is.Not.EqualTo(GameState.Resolving),"Animation did not finish within four seconds.");
            }
            Assert.That(game.Board.State,Is.EqualTo(GameState.Won)); Assert.That(game.Progress.IsChapterComplete(0),Is.True);
            Assert.That(cues, Does.Contain(AudioCue.Rotate));
            Assert.That(cues, Does.Contain(AudioCue.DirectionChange));
            Assert.That(cues, Does.Contain(AudioCue.Exit));
            Assert.That(cues, Does.Contain(AudioCue.ChapterComplete));
            Assert.That(GameObject.Find("Status").GetComponent<Text>().text,Does.StartWith("CHAPTER COMPLETE!"));
            yield return new WaitForSecondsRealtime(.2f); SprintPresentationTests.Capture("chapter-complete.png");
            Assert.That(GameObject.Find("Next Level"),Is.Not.Null); Assert.That(game.NextLevel(),Is.True);
            Assert.That(game.CurrentLevel.name,Does.StartWith("Level21"));
            Assert.That(game.SelectLevel(19),Is.True); yield return null;

            GameObject.Find("Restart").GetComponent<Button>().onClick.Invoke(); yield return null;
            Assert.That(game.Board.State,Is.EqualTo(GameState.Playing)); Assert.That(game.Progress.HighestUnlocked,Is.EqualTo(39));
            Assert.That(new LevelProgression(20,new SaveService(Prefix)).Current,Is.EqualTo(19));
            game.ResetProgress(); yield return null;
            Assert.That(game.Progress.Current,Is.Zero); Assert.That(game.CurrentLevel.name,Does.StartWith("Level01"));
            yield return new ExitPlayMode();
        }
        private static void Tap(GridPosition position)
        {
            foreach(var piece in Object.FindObjectsByType<Piece>(FindObjectsSortMode.None))
                if(piece.Data.Type==PieceType.Normal&&piece.Data.Position==position)
                { piece.GetComponentInChildren<Button>().onClick.Invoke(); return; }
            Assert.Fail("Missing piece at "+position);
        }
    }
}
