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
