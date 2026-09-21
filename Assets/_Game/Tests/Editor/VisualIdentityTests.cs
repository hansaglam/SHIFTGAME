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
    public sealed class VisualIdentityTests
    {
        [TearDown] public void Cleanup() => new SaveService("SHIFT.Tests.VisualIdentity.").Reset();

        [UnityTest] public IEnumerator ReducedMotionModalAndRestartPreserveInputAndLayout()
        {
            EditorSceneManager.OpenScene(PrototypeSetup.ScenePath);
            ProgressionPresentationTests.IsolateSave("SHIFT.Tests.VisualIdentity.");
            var serialized = new SerializedObject(Object.FindFirstObjectByType<PrototypeGame>());
            serialized.FindProperty("gameFeel").FindPropertyRelative("reducedMotion").boolValue = true;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            yield return new EnterPlayMode(); yield return new WaitForSecondsRealtime(.25f);
            var game = Object.FindFirstObjectByType<PrototypeGame>();
            Assert.That(GameObject.Find("Board").transform.localScale, Is.EqualTo(Vector3.one));
            Assert.That(GameObject.Find("Goal Moves Panel"), Is.Not.Null);
            foreach (var image in GameObject.Find("Atmosphere").GetComponentsInChildren<Graphic>())
                Assert.That(image.raycastTarget, Is.False, image.name);
            game.OpenChapter(); yield return new WaitForSecondsRealtime(.25f);
            var chapter = Object.FindFirstObjectByType<ChapterSelect>();
            Assert.That(chapter.IsOpen, Is.True);
            Assert.That(chapter.transform.localScale, Is.EqualTo(Vector3.one));
            foreach (var piece in Object.FindObjectsByType<Piece>(FindObjectsSortMode.None))
                if (piece.Data.Type == PieceType.Normal) Assert.That(piece.GetComponentInChildren<Button>().interactable, Is.False);
            game.CloseChapter(); yield return new WaitForSecondsRealtime(.25f);
            Assert.That(chapter.gameObject.activeSelf, Is.False);
            foreach (var piece in Object.FindObjectsByType<Piece>(FindObjectsSortMode.None))
                if (piece.Data.Type == PieceType.Normal) Assert.That(piece.GetComponentInChildren<Button>().interactable, Is.True);
            game.Restart(); yield return new WaitForSecondsRealtime(.25f);
            Assert.That(Object.FindObjectsByType<GridCell>(FindObjectsSortMode.None).Length, Is.EqualTo(16));
            Assert.That(game.Board.MovesRemaining, Is.EqualTo(1));
            Assert.That(GameObject.Find("Board").transform.localScale, Is.EqualTo(Vector3.one));
            SprintPresentationTests.Capture("sprint25-reduced-motion.png");
            yield return new ExitPlayMode();
        }
    }
}
