using System.Collections;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using NUnit.Framework;
using Shift.Game.Editor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Shift.Game.Tests
{
    public sealed class FinalIdentityTests
    {
        private const string Prefix="SHIFT.Tests.FinalIdentity.";
        [TearDown] public void Cleanup() { new SaveService(Prefix).Reset(); new SettingsService(Prefix+"Settings.").Reset(); }
        [Test] public void GameplayBoardArtScenesLevelsSavesAndTelemetryRemainByteIdentical()
        {
            foreach(var line in File.ReadAllLines(Path.Combine(Application.dataPath,"_Game/Tests/Editor/IdentityFrozenFiles.txt")))
            {
                var pair=line.Split('|'); using var sha=SHA256.Create();
                var hash=System.BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(Path.Combine(Application.dataPath,pair[0])))).Replace("-","");
                Assert.That(hash,Is.EqualTo(pair[1]),pair[0]);
            }
        }
        [Test] public void IdentityMaterialsReleaseWithCanvasAndBackdropCropsWithoutStretching()
        {
            var root=new GameObject("Identity lifetime",typeof(RectTransform),typeof(Canvas));
            var image=root.AddComponent<Image>();IdentityStyle.Material(image);
            var sprite=image.sprite;var texture=sprite.texture;
            Assert.That(IdentityResources.Surface(root.transform),Is.SameAs(sprite));
            var art=Resources.Load<Texture2D>("Identity/AlpineLake");Assert.That(art,Is.Not.Null);
            var backdrop=PlaceholderVisuals.Rect("Scenery",root.transform,Vector2.zero,Vector2.zero).gameObject.AddComponent<ScenicBackdrop>();
            backdrop.Initialize(art);
            foreach(var size in new[]{new Vector2(1080,1920),new Vector2(1080,2400),new Vector2(1536,2048)})
            {
                backdrop.rectTransform.sizeDelta=size;Canvas.ForceUpdateCanvases();
                var uv=backdrop.uvRect;
                Assert.That(uv.width*art.width/(uv.height*art.height),Is.EqualTo(size.x/size.y).Within(.001f));
                Assert.That(uv.xMin,Is.GreaterThanOrEqualTo(0));Assert.That(uv.yMin,Is.GreaterThanOrEqualTo(0));
                Assert.That(uv.xMax,Is.LessThanOrEqualTo(1));Assert.That(uv.yMax,Is.LessThanOrEqualTo(1));
            }
            Object.DestroyImmediate(root);Assert.That(sprite==null,Is.True);Assert.That(texture==null,Is.True);
        }
        [UnityTest] public IEnumerator IdentityScreensPreserveNavigationSettingsMotionAndCompletion()
        {
            EditorSceneManager.OpenScene(PrototypeSetup.ScenePath);ProgressionPresentationTests.IsolateSave(Prefix);
            yield return new EnterPlayMode();yield return null;
            var game=Object.FindFirstObjectByType<PrototypeGame>();game.UnlockAllLevels();
            foreach(int index in new[]{9,31,39})
            {
                Assert.That(game.SelectLevel(index),Is.True);yield return new WaitForSecondsRealtime(.3f);
                Assert.That(GameObject.Find("Title").GetComponent<Text>().text,Is.EqualTo("SHIFT"));
                Assert.That(GameObject.Find("Level Title").GetComponent<Text>().text,Is.EqualTo(game.CurrentLevel.DisplayTitle));
                Assert.That(GameObject.Find("Status").GetComponent<Text>().text,Is.EqualTo(game.CurrentLevel.Hint));
                Assert.That(GameObject.Find("Alpine Lake").GetComponent<ScenicBackdrop>().texture,Is.Not.Null);
                foreach(var icon in Object.FindObjectsByType<IdentityIcon>(FindObjectsSortMode.None)) Assert.That(icon.raycastTarget,Is.False);
                Assert.That(GameObject.Find("Hint"),Is.Null);Assert.That(GameObject.Find("Undo"),Is.Null);
                SprintPresentationTests.Capture($"identity-level{index+1}.png");
            }
            var restart=GameObject.Find("Restart").GetComponent<Button>();
            var motion=restart.GetComponent<PresentationMotion>();var pointer=new PointerEventData(EventSystem.current);
            motion.OnPointerDown(pointer);yield return new WaitForSecondsRealtime(.12f);
            Assert.That(restart.transform.localScale.x,Is.LessThan(.99f));
            game.OpenSettings();GameObject.Find("Reduced Motion Setting").GetComponent<Button>().onClick.Invoke();
            // Allow an Update after the test runner resumes; yield-null can precede Update in EditMode play tests.
            yield return new WaitForSecondsRealtime(.05f);
            Assert.That(game.Settings.ReducedMotion,Is.True);Assert.That(restart.transform.localScale,Is.EqualTo(Vector3.one));
            SprintPresentationTests.Capture("identity-settings.png");
            GameObject.Find("Close Settings").GetComponent<Button>().onClick.Invoke();motion.OnPointerUp(pointer);
            yield return Solve(game);yield return new WaitForSecondsRealtime(.3f);
            Assert.That(game.Telemetry.Result.perfectShift,Is.True);
            Assert.That(GameObject.Find("Result Panel").transform.localScale,Is.EqualTo(Vector3.one));
            foreach(Transform spark in GameObject.Find("Reward Gleam").transform) Assert.That(spark.localScale,Is.EqualTo(Vector3.one));
            SprintPresentationTests.Capture("identity-perfect-shift.png");
            yield return new WaitForSecondsRealtime(1.7f);
            Assert.That(GameObject.Find("Status").GetComponent<Text>().text,Does.StartWith("CHAPTER 2 COMPLETE"));
            SprintPresentationTests.Capture("identity-chapter-complete.png");
            GameObject.Find("Restart").GetComponent<Button>().onClick.Invoke();yield return null;
            Assert.That(game.Board.State,Is.EqualTo(GameState.Playing));
            game.SelectLevel(9);yield return new WaitForSecondsRealtime(.2f);
            yield return Solve(game);yield return new WaitForSecondsRealtime(.3f);
            var next=GameObject.Find("Next Level").GetComponent<Button>();
            Assert.That(next.interactable,Is.True);Assert.That(next.targetGraphic.color,Is.EqualTo(IdentityStyle.Teal));
            Assert.That(GameObject.Find("Restart").GetComponent<Image>().color,Is.EqualTo(IdentityStyle.Navy));
            SprintPresentationTests.Capture("identity-completion.png");
            next.onClick.Invoke();yield return null;Assert.That(game.CurrentLevel.name,Does.StartWith("Level11"));
            GameObject.Find("Open Chapter").GetComponent<Button>().onClick.Invoke();yield return new WaitForSecondsRealtime(.25f);
            Assert.That(Object.FindFirstObjectByType<ChapterSelect>().IsOpen,Is.True);
            SprintPresentationTests.Capture("identity-levels.png");
            yield return new ExitPlayMode();
        }
        private static IEnumerator Solve(PrototypeGame game)
        {
            foreach(var tap in game.CurrentLevel.KnownSolution)
            {
                var piece=Object.FindObjectsByType<Piece>(FindObjectsSortMode.None).First(p=>p.Data.Type==PieceType.Normal && p.Data.Position==tap);
                piece.GetComponentInChildren<Button>().onClick.Invoke();double deadline=Time.realtimeSinceStartupAsDouble+5;
                while(game.Board.State==GameState.Resolving && Time.realtimeSinceStartupAsDouble<deadline) yield return null;
                Assert.That(game.Board.State,Is.Not.EqualTo(GameState.Resolving));
            }
            Assert.That(game.Board.State,Is.EqualTo(GameState.Won));
        }
    }
}
