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
    public sealed class LevelDesignPresentationTests
    {
        private const string Prefix = "SHIFT.Tests.DesignPlay.";
        [TearDown] public void Cleanup() { new SaveService(Prefix).Reset(); new SettingsService(Prefix+"Settings.").Reset(); }
        [UnityTest] public IEnumerator SculptedBoardsAndMasteryRespectInputAndReducedMotion()
        {
            EditorSceneManager.OpenScene(PrototypeSetup.ScenePath); ProgressionPresentationTests.IsolateSave(Prefix);
            var serialized = new SerializedObject(Object.FindFirstObjectByType<PrototypeGame>());
            serialized.FindProperty("gameFeel").FindPropertyRelative("reducedMotion").boolValue = true;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            yield return new EnterPlayMode(); yield return null;
            var game = Object.FindFirstObjectByType<PrototypeGame>(); game.UnlockAllLevels();
            foreach (int index in new[] {29,31,33,35,37,38,39})
            {
                Assert.That(game.SelectLevel(index),Is.True); yield return new WaitForSecondsRealtime(.25f);
                var level = game.CurrentLevel;
                int walls = 0; foreach (var p in level.Placements) if(p.type==PieceType.Wall) walls++;
                Assert.That(Object.FindObjectsByType<GridCell>(FindObjectsSortMode.None).Length,Is.EqualTo(level.Width*level.Height-walls));
                SprintPresentationTests.Capture($"sprint5-level{index+1}-start.png");
            }
            Assert.That(game.SelectLevel(38),Is.True); yield return null;
            var hud = Object.FindFirstObjectByType<GameHud>();
            hud.ShowMastery(new AttemptMetrics { completed=true, hasOptimal=false, perfectShift=false }); Assert.That(hud.MasteryVisible,Is.False);
            hud.ShowMastery(new AttemptMetrics { completed=true, hasOptimal=true, perfectShift=false }); Assert.That(hud.MasteryVisible,Is.False);
            // The stronger tier is announced once after a reaction, not for each action.
            hud.BeginReaction(); hud.ShowChain(6); Assert.That(GameObject.Find("Chain").GetComponent<Text>().text,Does.StartWith("CHAIN"));
            hud.FinishChain(); Assert.That(GameObject.Find("Chain").GetComponent<Text>().text,Does.StartWith("MEGA SHIFT"));
            foreach (var tap in game.CurrentLevel.KnownSolution)
            {
                foreach (var piece in Object.FindObjectsByType<Piece>(FindObjectsSortMode.None))
                    if(piece.Data.Type==PieceType.Normal && piece.Data.Position==tap) { piece.GetComponentInChildren<Button>().onClick.Invoke(); break; }
                double deadline=Time.realtimeSinceStartupAsDouble+5;
                while(game.Board.State==GameState.Resolving && Time.realtimeSinceStartupAsDouble<deadline) yield return null;
                Assert.That(game.Board.State,Is.Not.EqualTo(GameState.Resolving));
            }
            Assert.That(game.Board.State,Is.EqualTo(GameState.Won)); Assert.That(hud.MasteryVisible,Is.True);
            Assert.That(GameObject.Find("Board").transform.localScale,Is.EqualTo(Vector3.one));
            SprintPresentationTests.Capture("sprint5-perfect-shift.png");
            Assert.That(GameObject.Find("Next Level").GetComponent<Button>().interactable,Is.True);
            Assert.That(game.NextLevel(),Is.True); yield return null;
            Assert.That(game.CurrentLevel.name,Does.StartWith("Level40")); Assert.That(Object.FindFirstObjectByType<GameHud>().MasteryVisible,Is.False);
            yield return new ExitPlayMode();
        }
    }
}
