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
            Assert.That(hud.MasteryVisible,Is.True,"Perfect remains visible after its entrance animation");
            Assert.That(GameObject.Find("Status").GetComponent<Text>().text,Does.StartWith("CHAPTER 2 COMPLETE"));
            SprintPresentationTests.Capture("chapter2-complete-settled.png");
            game.Restart(); yield return null;
            foreach(var p in game.Board.Pieces) if(p.Type==PieceType.Gate) Assert.That(p.GateOpen,Is.False);
            yield return new ExitPlayMode();
        }
        [UnityTest] public IEnumerator CompletionVariantsRemainReadablePersistentAndNavigable()
        {
            EditorSceneManager.OpenScene(PrototypeSetup.ScenePath);ProgressionPresentationTests.IsolateSave(Prefix);
            yield return new EnterPlayMode();yield return null;
            var game=Object.FindFirstObjectByType<PrototypeGame>();game.UnlockAllLevels();game.SelectLevel(9);yield return null;
            System.IO.Directory.CreateDirectory(System.IO.Path.Combine(Application.dataPath,"../Validation/completion"));
            var first=game.CurrentLevel.KnownSolution[0];
            foreach(var piece in Object.FindObjectsByType<Piece>(FindObjectsSortMode.None))
                if(piece.Data.Type==PieceType.Normal && piece.Data.Position==first) { piece.GetComponentInChildren<Button>().onClick.Invoke();break; }
            float deadline=Time.realtimeSinceStartup+12;
            while(game.Board.State==GameState.Resolving && Time.realtimeSinceStartup<deadline) yield return null;
            Assert.That(game.Board.State,Is.Not.EqualTo(GameState.Resolving));
            Assert.That(game.Undo(),Is.True);yield return Solve(game);CaptureEntrance();yield return new WaitForSecondsRealtime(.4f);
            Assert.That(game.Telemetry.Result.perfectShift,Is.False);
            Assert.That(game.Telemetry.Result.successfulMoves,Is.EqualTo(game.CurrentLevel.VerifiedOptimalMoveCount.Value+1));
            yield return Screens("normal");
            game.Restart();yield return null;yield return Solve(game);CaptureEntrance();yield return new WaitForSecondsRealtime(.4f);
            Assert.That(game.Progress.IsPerfect(9),Is.True);
            Assert.That(GameObject.Find("Status").GetComponent<Text>().text,Does.Contain("NEW PERFECT SHIFT"));
            yield return Screens("first-perfect");
            yield return new WaitForSecondsRealtime(2f);
            CheckPersistent();
            game.Restart();yield return null;Assert.That(Object.FindFirstObjectByType<GameHud>().MasteryVisible,Is.False);
            yield return Solve(game);CaptureEntrance();yield return new WaitForSecondsRealtime(.4f);
            Assert.That(GameObject.Find("Status").GetComponent<Text>().text,Does.Contain("PERFECT SHIFT").And.Not.Contain("NEW"));
            yield return Screens("repeated-perfect");
            foreach(int height in new[]{1440,2400})
            {
                var safe=GameObject.Find("Safe Area").GetComponent<RectTransform>();var min=safe.anchorMin;var max=safe.anchorMax;
                safe.anchorMin=new Vector2(.025f,.025f);safe.anchorMax=new Vector2(.975f,.955f);
                SprintPresentationTests.Capture($"completion/portrait-{height}.png",1080,height,CheckCompletion);
                safe.anchorMin=min;safe.anchorMax=max;
            }
            game.Restart();yield return null;game.OpenSettings();GameObject.Find("Reduced Motion Setting").GetComponent<Button>().onClick.Invoke();game.CloseSettings();
            yield return Solve(game);CaptureEntrance();yield return new WaitForSecondsRealtime(.4f);
            Assert.That(game.Settings.ReducedMotion,Is.True);Assert.That(GameObject.Find("Result Panel").transform.localScale,Is.EqualTo(Vector3.one));
            SprintPresentationTests.Capture("completion/reduced-motion.png",1080,1920,CheckCompletion);
            GameObject.Find("Open Chapter").GetComponent<Button>().onClick.Invoke();Assert.That(Object.FindFirstObjectByType<ChapterSelect>().IsOpen,Is.True);game.CloseChapter();
            GameObject.Find("Next Level").GetComponent<Button>().onClick.Invoke();yield return null;
            Assert.That(game.CurrentLevel.name,Does.StartWith("Level11"));Assert.That(Object.FindFirstObjectByType<GameHud>().MasteryVisible,Is.False);
            yield return new ExitPlayMode();
            IEnumerator Screens(string name)
            {
                foreach(var language in new[]{GameLanguage.English,GameLanguage.Turkish})
                {
                    GameLanguageService.Shared.Select(language);yield return null;
                    SprintPresentationTests.Capture($"completion/{name}-{language}.png",1080,1920,CheckCompletion);
                }
                GameLanguageService.Shared.Select(GameLanguage.English);yield return null;
            }
        }
        [UnityTest] public IEnumerator RapidNavigationDuringSuccessEntranceRunsOnceAndResetsOldHud()
        {
            EditorSceneManager.OpenScene(PrototypeSetup.ScenePath); ProgressionPresentationTests.IsolateSave(Prefix);
            yield return new EnterPlayMode(); yield return null;
            var game=Object.FindFirstObjectByType<PrototypeGame>();game.UnlockAllLevels();game.SelectLevel(9);yield return null;
            yield return Solve(game);
            var panel=GameObject.Find("Result Panel").GetComponent<RectTransform>();var group=panel.GetComponent<CanvasGroup>();
            var replay=GameObject.Find("Restart").GetComponent<Button>();
            int visits=game.Telemetry.SessionSnapshot().levelsStarted;
            replay.onClick.Invoke(); replay.onClick.Invoke();
            Assert.That(game.Telemetry.SessionSnapshot().levelsStarted,Is.EqualTo(visits+1));
            Assert.That(group.alpha,Is.EqualTo(1));Assert.That(panel.anchoredPosition,Is.EqualTo(Vector2.zero));Assert.That(panel.localScale,Is.EqualTo(Vector3.one));
            yield return null;Assert.That(game.Board.State,Is.EqualTo(GameState.Playing));
            yield return Solve(game);
            var next=GameObject.Find("Next Level").GetComponent<Button>();visits=game.Telemetry.SessionSnapshot().levelsStarted;
            next.onClick.Invoke();next.onClick.Invoke();
            Assert.That(game.Telemetry.SessionSnapshot().levelsStarted,Is.EqualTo(visits+1));
            yield return null;Assert.That(game.CurrentLevel.name,Does.StartWith("Level11"));
            Assert.That(game.Board.State,Is.EqualTo(GameState.Playing));
            yield return new ExitPlayMode();
        }
        private static int entranceCapture;
        private static void CaptureEntrance()
        {
            var hud=Object.FindFirstObjectByType<GameHud>();
            var flags=System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance;
            var elapsed=typeof(GameHud).GetField("completionElapsed",flags);
            var advance=typeof(GameHud).GetMethod("AdvanceCompletion",flags);
            string variant=++entranceCapture+"-"+(hud.MasteryVisible?"perfect":"normal");
            System.IO.Directory.CreateDirectory(System.IO.Path.Combine(Application.dataPath,"../Validation/completion/entrance"));
            // Controlled timestamps of the real post-resolution presentation, independent of capture I/O time.
            foreach(float time in new[]{0f,.13f,.26f})
            {
                elapsed.SetValue(hud,time);advance.Invoke(hud,new object[]{0f});
                SprintPresentationTests.Capture($"completion/entrance/{variant}-{time:F2}.png");
            }
        }
        private static void CheckPersistent()
        {
            Assert.That(Object.FindFirstObjectByType<GameHud>().MasteryVisible,Is.True);
            Assert.That(GameObject.Find("Status").GetComponent<Text>().text,Does.Contain("NEW PERFECT SHIFT"));
        }
        private static void CheckCompletion()
        {
            Assert.That(GameObject.Find("Undo"),Is.Null);Assert.That(GameObject.Find("Hint"),Is.Null);
            Assert.That(GameObject.Find("Completion Moves"),Is.Not.Null);
            var game=Object.FindFirstObjectByType<PrototypeGame>();
            var language=GameLanguageService.Shared;
            Assert.That(GameObject.Find("Status").GetComponent<Text>().text,Does.StartWith(language.Text("results.complete", "10")));
            Assert.That(GameObject.Find("Completion Moves").GetComponent<Text>().text,Is.EqualTo(language.Text("results.completed_moves", game.Telemetry.Result.successfulMoves)));
            foreach(string name in new[]{"Status","Completion Moves"})
            {
                var text=GameObject.Find(name).GetComponent<Text>();var settings=text.GetGenerationSettings(text.rectTransform.rect.size);
                Assert.That(text.cachedTextGenerator.Populate(text.text,settings),Is.True,name);
                Assert.That(text.cachedTextGenerator.characterCountVisible,Is.GreaterThan(0),name);
                // preferredHeight ignores best-fit shrinking; inspect the rendered glyph geometry instead.
                var rect=text.rectTransform.rect;
                foreach(var vertex in text.cachedTextGenerator.verts)
                {
                    var point=vertex.position/text.pixelsPerUnit;
                    Assert.That(point.x,Is.InRange(rect.xMin-1,rect.xMax+1),name+" horizontal clipping");
                    Assert.That(point.y,Is.InRange(rect.yMin-1,rect.yMax+1),name+" vertical clipping");
                }
                Assert.That(text.raycastTarget,Is.False);
            }
            foreach(string name in new[]{"Restart","Next Level","Open Chapter"})Assert.That(GameObject.Find(name).GetComponent<Button>().interactable,Is.True);
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
