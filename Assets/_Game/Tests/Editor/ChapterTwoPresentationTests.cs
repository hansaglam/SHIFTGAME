using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Shift.Game.Editor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Shift.Game.Tests
{
    public sealed class ChapterTwoPresentationTests
    {
        private const string Prefix="SHIFT.Tests.ChapterTwoPlay.";
        [TearDown] public void Cleanup() { new SaveService(Prefix).Reset(); new SettingsService(Prefix+"Settings.").Reset(); }
        [UnityTest] public IEnumerator ChapterBoundaryTabsFinaleAndStateCues()
        {
            EditorSceneManager.OpenScene(PrototypeSetup.ScenePath); ProgressionPresentationTests.IsolateSave(Prefix);
            new SaveService(Prefix).Save(new ProgressData(19,19,18));
            yield return new EnterPlayMode(); yield return null;
            var game=Object.FindFirstObjectByType<PrototypeGame>();
            game.OpenChapter(); Assert.That(GameObject.Find("Chapter 2 Tab").GetComponent<Button>().interactable,Is.False); game.CloseChapter();
            yield return new WaitForSecondsRealtime(.2f);
            yield return Solve(game);
            Assert.That(game.Progress.HighestUnlocked,Is.EqualTo(20)); Assert.That(game.NextLevel(),Is.True); yield return null;
            Assert.That(game.CurrentLevel.name,Does.StartWith("Level21"));
            game.OpenChapter(); Assert.That(GameObject.Find("Level 21"),Is.Not.Null); Assert.That(GameObject.Find("Level 1"),Is.Null);
            Assert.That(GameObject.Find("Chapter 2 Tab").GetComponent<Button>().interactable,Is.True);
            yield return new WaitForSecondsRealtime(.2f); SprintPresentationTests.Capture("chapter2-select.png");
            game.CloseChapter(); game.UnlockAllLevels(); Assert.That(game.SelectLevel(39),Is.True); yield return new WaitForSecondsRealtime(.2f);
            Assert.That(GameObject.Find("Switch Channel"),Is.Not.Null);
            Assert.That(GameObject.Find("Channel Badge").GetComponent<Image>().raycastTarget,Is.False);
            SprintPresentationTests.Capture("level40-start.png");
            var cues=new HashSet<AudioCue>(); game.GetComponent<AudioManager>().CuePlayed += cue=>cues.Add(cue);
            yield return Solve(game);
            Assert.That(game.Progress.ChapterComplete,Is.True); Assert.That(game.Progress.IsChapterComplete(1),Is.True);
            Assert.That(game.NextLevel(),Is.False); Assert.That(GameObject.Find("Next Level"),Is.Null);
            Assert.That(GameObject.Find("Status").GetComponent<Text>().text,Does.StartWith("CHAPTER 2 COMPLETE"));
            Assert.That(cues,Does.Contain(AudioCue.SwitchActivate)); Assert.That(cues,Does.Contain(AudioCue.GateOpen));
            Assert.That(game.Telemetry.Result.perfectShift,Is.True);
            Assert.That(game.Telemetry.Result.switchActivations,Is.EqualTo(1)); Assert.That(game.Telemetry.Result.gateOpens,Is.EqualTo(1));
            var hud = Object.FindFirstObjectByType<GameHud>();
            Assert.That(hud.MasteryVisible,Is.True);
            Assert.That(hud.CurrentReactionTier,Is.EqualTo(ReactionTier.MegaShift));
            Assert.That(GameObject.Find("Status").GetComponent<Text>().text,Does.Contain("PERFECT SHIFT"));
            SprintPresentationTests.Capture("chapter2-complete.png");
            yield return new WaitForSecondsRealtime(1.7f);
            Assert.That(hud.MasteryVisible,Is.False);
            Assert.That(GameObject.Find("Status").GetComponent<Text>().text,Does.StartWith("CHAPTER 2 COMPLETE"));
            game.Restart(); yield return null;
            foreach(var p in game.Board.Pieces) if(p.Type==PieceType.Gate) Assert.That(p.GateOpen,Is.False);
            yield return new ExitPlayMode();
        }
        private static IEnumerator Solve(PrototypeGame game)
        {
            foreach(var tap in game.CurrentLevel.KnownSolution)
            {
                Piece chosen=null;
                foreach(var piece in Object.FindObjectsByType<Piece>(FindObjectsSortMode.None))
                    if(piece.Data.Type==PieceType.Normal && piece.Data.Position==tap) { chosen=piece; break; }
                Assert.That(chosen,Is.Not.Null); chosen.GetComponentInChildren<Button>().onClick.Invoke();
                double deadline=Time.realtimeSinceStartupAsDouble+4;
                while(game.Board.State==GameState.Resolving && Time.realtimeSinceStartupAsDouble<deadline) yield return null;
                Assert.That(game.Board.State,Is.Not.EqualTo(GameState.Resolving));
            }
            Assert.That(game.Board.State,Is.EqualTo(GameState.Won));
        }
    }
}
