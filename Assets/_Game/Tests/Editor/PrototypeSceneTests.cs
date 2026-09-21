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
    public sealed class PrototypeSceneTests
    {
        [Test] public void SavedPrototypeContainsValidLevelReference()
        {
            var level = AssetDatabase.LoadAssetAtPath<LevelData>(PrototypeSetup.LevelPath);
            Assert.That(level, Is.Not.Null);
            Assert.That(level.Validate(out string error), Is.True, error);
            EditorSceneManager.OpenScene(PrototypeSetup.ScenePath);
            var game = Object.FindFirstObjectByType<PrototypeGame>();
            Assert.That(game, Is.Not.Null);
            Assert.That(new SerializedObject(game).FindProperty("level").objectReferenceValue, Is.EqualTo(level));
        }

        [UnityTest] public IEnumerator ScenePlaysChainAndCanRestartDuringAnimation()
        {
            EditorSceneManager.OpenScene(PrototypeSetup.ScenePath);
            var setup = new SerializedObject(Object.FindFirstObjectByType<PrototypeGame>());
            setup.FindProperty("useLevelSet").boolValue = false; setup.ApplyModifiedPropertiesWithoutUndo();
            yield return new EnterPlayMode();
            yield return null;
            var game = Object.FindFirstObjectByType<PrototypeGame>();
            Assert.That(Object.FindObjectsByType<GridCell>(FindObjectsSortMode.None).Length, Is.EqualTo(36));
            Piece yellow = FindYellow();
            yellow.GetComponentInChildren<Button>().onClick.Invoke();
            Assert.That(yellow.GetComponentInChildren<Button>().interactable, Is.False);
            game.Restart();
            yield return null;
            Assert.That(GameObject.Find("Moves").GetComponent<Text>().text, Is.EqualTo("3"));
            FindYellow().GetComponentInChildren<Button>().onClick.Invoke();
            yield return new WaitForSecondsRealtime(1f);
            Assert.That(GameObject.Find("Status").GetComponent<Text>().text, Does.StartWith("GOAL COMPLETE!"));
            Assert.That(GameObject.Find("Moves").GetComponent<Text>().text, Is.EqualTo("2"));
            game.Restart();
            yield return null;
            Assert.That(FindYellow().Data.Position, Is.EqualTo(new GridPosition(1, 2)));
            Assert.That(FindYellow().GetComponentInChildren<Button>().interactable, Is.True);
            yield return new ExitPlayMode();
        }

        private static Piece FindYellow()
        {
            foreach (var piece in Object.FindObjectsByType<Piece>(FindObjectsSortMode.None))
                if (piece.Data.Color == PieceColor.Yellow) return piece;
            Assert.Fail("Missing Yellow piece."); return null;
        }
    }
}
