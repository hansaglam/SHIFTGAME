using System.Collections;
using NUnit.Framework;
using Shift.Game.Editor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Shift.Game.Tests
{
    public sealed class LayoutStepTwoTests
    {
        private const string Prefix = "SHIFT.Tests.Layout2.";
        [TearDown] public void Cleanup() { new SaveService(Prefix).Reset(); new SettingsService(Prefix+"Settings.").Reset(); }
        [UnityTest] public IEnumerator CampaignHeaderPortraitAndLanguageCoverage()
        {
            EditorSceneManager.OpenScene(PrototypeSetup.ScenePath); ProgressionPresentationTests.IsolateSave(Prefix);
            yield return new EnterPlayMode(); yield return null;
            var game=Object.FindFirstObjectByType<PrototypeGame>();game.UnlockAllLevels();
            System.IO.Directory.CreateDirectory(System.IO.Path.Combine(Application.dataPath,"../Validation/header"));
            foreach(var language in new[]{GameLanguage.English,GameLanguage.Turkish})
            {
                GameLanguageService.Shared.Select(language);
                foreach(int index in new[]{0,7,28,39,21,29})
                {
                    game.SelectLevel(index);yield return null;
                    SprintPresentationTests.Capture($"header/{language}-level{index+1}.png",1080,1920,CheckHeader);
                }
                foreach(int height in new[]{1440,2400})
                {
                    game.SelectLevel(language==GameLanguage.English?21:29);yield return null;
                    var safe=GameObject.Find("Safe Area").GetComponent<RectTransform>();
                    var min=safe.anchorMin;var max=safe.anchorMax;
                    safe.anchorMin=new Vector2(.025f,.025f);safe.anchorMax=new Vector2(.975f,.955f);
                    SprintPresentationTests.Capture($"header/{language}-{height}-inset.png",1080,height,CheckHeader);
                    safe.anchorMin=min;safe.anchorMax=max;
                }
            }
            GameLanguageService.Shared.Select(GameLanguage.English);
            game.OpenSettings();GameObject.Find("Reduced Motion Setting").GetComponent<Button>().onClick.Invoke();game.CloseSettings();yield return null;
            Assert.That(game.Settings.ReducedMotion,Is.True);
            SprintPresentationTests.Capture("header/reduced-motion.png",1080,1920,CheckHeader);
            yield return new ExitPlayMode();
        }
        private static void CheckHeader()
        {
            var title=GameObject.Find("Level Title").GetComponent<Text>();var number=GameObject.Find("Level Number").GetComponent<Text>();
            Rect Bounds(string name)
            {
                var r=GameObject.Find(name).GetComponent<RectTransform>();var v=new Vector3[4];r.GetWorldCorners(v);
                return Rect.MinMaxRect(v[0].x,v[0].y,v[2].x,v[2].y);
            }
            Assert.That(Bounds("Level Title").yMin,Is.GreaterThan(Bounds("Goal Moves Panel").yMax));
            Assert.That(Bounds("Level Badge").yMin,Is.GreaterThan(Bounds("Level Title").yMax));
            Assert.That(Bounds("Level Badge").yMax,Is.LessThanOrEqualTo(Bounds("SHIFT Wordmark").yMin));
            foreach(var text in new[]{title,number})
            {
                var settings=text.GetGenerationSettings(text.rectTransform.rect.size);
                text.cachedTextGenerator.Populate(text.text,settings);
                Assert.That(text.cachedTextGenerator.characterCountVisible,Is.EqualTo(text.text.Length),text.text);
                Assert.That(text.cachedTextGenerator.lineCount,Is.EqualTo(1),text.text);
                Assert.That(text.raycastTarget,Is.False);
                Assert.That(text.transform.localScale,Is.EqualTo(Vector3.one));
            }
            Assert.That(Bounds("Level Title").xMin,Is.GreaterThan(Bounds("Safe Area").xMin));
            Assert.That(Bounds("Level Title").xMax,Is.LessThan(Bounds("Safe Area").xMax));
            var panel=GameObject.Find("Goal Moves Panel").GetComponent<RectTransform>();
            Assert.That(panel.anchorMax-panel.anchorMin,Is.EqualTo(new Vector2(.74f,.063f)).Using(UnityEngine.TestTools.Utils.Vector2ComparerWithEqualsOperator.Instance));
        }
        [UnityTest] public IEnumerator HeaderControlsAndInstructionRemainReadableAndFunctional()
        {
            EditorSceneManager.OpenScene(PrototypeSetup.ScenePath); ProgressionPresentationTests.IsolateSave(Prefix);
            yield return new EnterPlayMode(); yield return null;
            var game = Object.FindFirstObjectByType<PrototypeGame>(); game.UnlockAllLevels();
            var atmosphere = GameObject.Find("Atmosphere");
            Assert.That(atmosphere.GetComponent<CanvasRenderer>(),Is.Not.Null,"Gradient needs a renderer, not just a generated mesh.");
            Canvas.ForceUpdateCanvases();
            var mesh = atmosphere.GetComponent<CanvasRenderer>().GetMesh();
            Assert.That(mesh,Is.Not.Null); Assert.That(mesh.vertexCount,Is.EqualTo(4));
            foreach(int index in new[] {0,31,39})
            {
                Assert.That(game.SelectLevel(index),Is.True); yield return new WaitForSecondsRealtime(.3f);
                Assert.That(GameObject.Find("Moves").GetComponent<Text>().text,Is.EqualTo(game.Board.MovesRemaining.ToString()));
                Assert.That(GameObject.Find("Moves Label").GetComponent<Text>().text,Is.EqualTo("Moves:"));
                Assert.That(GameObject.Find("Goal").GetComponent<Text>().text,Is.EqualTo(index==39?"Clear Both":"Clear Red"));
                Assert.That(GameObject.Find("Status").GetComponent<Text>().text,Is.EqualTo(game.CurrentLevel.Hint));
                var area = GameObject.Find("Board Area").GetComponent<RectTransform>();
                Assert.That(area.anchorMax.x-area.anchorMin.x,Is.EqualTo(.91f).Within(.001f));
                var settings = GameObject.Find("Open Settings").GetComponent<RectTransform>();
                Assert.That(settings.anchorMin,Is.EqualTo(Vector2.one)); Assert.That(settings.sizeDelta.x,Is.EqualTo(112));
                Assert.That(settings.GetComponent<Button>().targetGraphic,Is.EqualTo(GameObject.Find("Settings Face").GetComponent<Image>()));
                foreach(var piece in Object.FindObjectsByType<Piece>(FindObjectsSortMode.None))
                {
                    var button = piece.GetComponentInChildren<Button>();
                    if(button!=null) Assert.That(button.targetGraphic.raycastPadding.x,Is.LessThanOrEqualTo(-8));
                }
                SprintPresentationTests.Capture($"layout2-level{index+1}.png");
            }
            GameObject.Find("Open Settings").GetComponent<Button>().onClick.Invoke();
            Assert.That(Object.FindFirstObjectByType<SettingsPanel>().IsOpen,Is.True);
            bool prior = game.Settings.Sound; GameObject.Find("Sound Setting").GetComponent<Button>().onClick.Invoke();
            Assert.That(game.Settings.Sound,Is.EqualTo(!prior));
            SprintPresentationTests.Capture("layout2-settings.png");
            GameObject.Find("Close Settings").GetComponent<Button>().onClick.Invoke();
            game.OpenChapter(); yield return new WaitForSecondsRealtime(.25f); SprintPresentationTests.Capture("layout2-chapters.png");
            game.CloseChapter(); yield return new WaitForSecondsRealtime(.25f);
            // Exercise inset safe-area geometry without replacing SafeArea's platform logic.
            var safe = GameObject.Find("Safe Area").GetComponent<RectTransform>(); var min=safe.anchorMin; var max=safe.anchorMax;
            safe.anchorMin = new Vector2(.025f,.035f); safe.anchorMax = new Vector2(.975f,.945f);
            Canvas.ForceUpdateCanvases(); SprintPresentationTests.Capture("layout2-inset-safe-area.png");
            safe.anchorMin=min; safe.anchorMax=max;
            yield return new ExitPlayMode();
        }
    }
}
