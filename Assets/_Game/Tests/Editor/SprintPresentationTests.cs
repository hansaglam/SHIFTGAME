using System.Collections;
using System.IO;
using NUnit.Framework;
using Shift.Game.Editor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Shift.Game.Tests
{
    public sealed class SprintPresentationTests
    {
        [TearDown] public void CleanupProgress() => new SaveService("SHIFT.Tests.LegacyPresentation.").Reset();
        [UnityTest] public IEnumerator FeedbackLevelSwitchAndRetryStayInSync()
        {
            EditorSceneManager.OpenScene(PrototypeSetup.ScenePath);
            ProgressionPresentationTests.IsolateSave("SHIFT.Tests.LegacyPresentation.");
            yield return new EnterPlayMode(); yield return null;
            var game = Object.FindFirstObjectByType<PrototypeGame>();
            Assert.That(game.CurrentLevel.name, Does.StartWith("Level01"));
            Assert.That(game.LoadLevel(4), Is.True); yield return null;
            Tap(new GridPosition(0, 1));
            Assert.That(game.Board.MovesRemaining, Is.EqualTo(2));
            Assert.That(game.Board.State, Is.EqualTo(GameState.Resolving));
            yield return new WaitForSecondsRealtime(.25f);
            Assert.That(game.Board.State, Is.EqualTo(GameState.Playing));
            Assert.That(GameObject.Find("Chain").GetComponent<CanvasGroup>().alpha, Is.Zero);
            Assert.That(game.LoadLevel(9), Is.True); yield return null;
            yield return new WaitForSecondsRealtime(.2f); Capture("level10-start.png"); yield return new WaitForSecondsRealtime(.15f);
            Tap(new GridPosition(4, 3)); yield return new WaitForSecondsRealtime(.35f);
            Assert.That(game.Board.MovesRemaining, Is.EqualTo(3));
            Tap(new GridPosition(1, 2)); yield return new WaitForSecondsRealtime(.4f);
            Assert.That(GameObject.Find("Chain").GetComponent<CanvasGroup>().alpha, Is.GreaterThan(0));
            Capture("level10-chain.png"); yield return new WaitForSecondsRealtime(.8f);
            Tap(new GridPosition(4, 3)); yield return new WaitForSecondsRealtime(.35f);
            Tap(new GridPosition(4, 4)); yield return new WaitForSecondsRealtime(.4f);
            Assert.That(game.Board.State, Is.EqualTo(GameState.Won));
            Capture("level10-win.png"); yield return new WaitForSecondsRealtime(.7f);
            Assert.That(GameObject.Find("Chain").GetComponent<CanvasGroup>().alpha, Is.Zero);
            Assert.That(GameObject.Find("Moves").GetComponent<Text>().text, Is.EqualTo("0"));
            Assert.That(game.LoadLevel(0), Is.True); yield return null;
            Assert.That(game.Board.State, Is.EqualTo(GameState.Playing));
            Assert.That(Object.FindObjectsByType<GridCell>(FindObjectsSortMode.None).Length, Is.EqualTo(16));
            Assert.That(game.LoadLevel(-1), Is.False);
            Assert.That(game.LoadLevel(3), Is.True); yield return null;
            Tap(new GridPosition(2, 2)); yield return new WaitForSecondsRealtime(.35f);
            Tap(new GridPosition(2, 3)); yield return new WaitForSecondsRealtime(.35f);
            Assert.That(game.Board.State, Is.EqualTo(GameState.Lost));
            GameObject.Find("Restart").GetComponent<Button>().onClick.Invoke(); yield return null;
            Assert.That(game.Board.State, Is.EqualTo(GameState.Playing));
            Assert.That(game.Board.MovesRemaining, Is.EqualTo(2));
            yield return new ExitPlayMode();
        }

        [Test] public void EmptyAudioAndEditorHapticsAreSafe()
        {
            var go = new GameObject("Audio test");
            try
            {
                var audio = go.AddComponent<AudioManager>(); audio.Initialize(new AudioClips());
                foreach (AudioCue cue in System.Enum.GetValues(typeof(AudioCue))) Assert.DoesNotThrow(() => audio.Play(cue));
                Assert.That(go.GetComponents<AudioSource>().Length, Is.EqualTo(4)); audio.StopAll();
                var haptics = new HapticService(new GameFeelSettings { hapticsEnabled = true });
                Assert.DoesNotThrow(() => { haptics.Light(); haptics.Medium(); haptics.Success(); haptics.Failure(); });
            }
            finally { Object.DestroyImmediate(go); }
        }

        private static void Tap(GridPosition position)
        {
            foreach (var piece in Object.FindObjectsByType<Piece>(FindObjectsSortMode.None))
                if (piece.Data.Type == PieceType.Normal && piece.Data.Position == position)
                { piece.GetComponentInChildren<Button>().onClick.Invoke(); return; }
            Assert.Fail($"Missing tappable piece at {position}.");
        }
        internal static void Capture(string name)
        {
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null) return;
            string directory = Path.GetFullPath(Path.Combine(Application.dataPath, "../Validation"));
            Directory.CreateDirectory(directory);
            // Batch mode has no presented Game view; render the actual UI through a portrait camera target.
            var canvas = GameObject.Find("SHIFT UI").GetComponent<Canvas>();
            var scaler = canvas.GetComponent<CanvasScaler>();
            var camera = GameObject.Find("Background Camera").GetComponent<Camera>();
            var previousMode = canvas.renderMode; var previousCamera = canvas.worldCamera;
            float previousScale = canvas.scaleFactor, previousPlane = canvas.planeDistance;
            int previousMask = camera.cullingMask;
            var previousTarget = camera.targetTexture; var previousActive = RenderTexture.active;
            var target = RenderTexture.GetTemporary(1080, 1920, 24, RenderTextureFormat.ARGB32);
            var image = new Texture2D(1080, 1920, TextureFormat.RGB24, false);
            try
            {
                camera.targetTexture = target; camera.cullingMask = -1;
                canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = 1;
                scaler.enabled = false; canvas.scaleFactor = 1; Canvas.ForceUpdateCanvases();
                UnityEngine.Rendering.RenderPipeline.SubmitRenderRequest(camera,
                    new UnityEngine.Rendering.RenderPipeline.StandardRequest { destination = target });
                RenderTexture.active = target;
                image.ReadPixels(new Rect(0, 0, 1080, 1920), 0, 0); image.Apply();
                File.WriteAllBytes(Path.Combine(directory, name), image.EncodeToPNG());
            }
            finally
            {
                RenderTexture.active = previousActive; camera.targetTexture = previousTarget; camera.cullingMask = previousMask;
                canvas.renderMode = previousMode; canvas.worldCamera = previousCamera; canvas.planeDistance = previousPlane;
                canvas.scaleFactor = previousScale; scaler.enabled = true; Canvas.ForceUpdateCanvases();
                RenderTexture.ReleaseTemporary(target); Object.DestroyImmediate(image);
            }
        }
    }
}
