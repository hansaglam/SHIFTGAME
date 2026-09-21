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
    public sealed class LevelWiringTests
    {
        private const string Prefix = "SHIFT.Tests.Wiring.";
        private static readonly string[] Expected = {
            "Level01_FirstTap", "Level02_FindTheExit", "Level03_ClearTheBox", "Level04_FirstChain",
            "Level05_BlockedIsFree", "Level06_TurnTheCorner", "Level07_MakeRoom", "Level08_Clockwise",
            "Level09_PushAndTurn", "Level10_SetTheChain", "Level11_OpenTheLane", "Level12_CrossingOrder",
            "Level13_SecondAct", "Level14_TwoCorners", "Level15_TheLongSetup", "Level16_TransformAhead",
            "Level17_SharedCorridor", "Level18_ClearTriggerFinish", "Level19_SharedDelivery", "Level20_TheFinalShift",
            "Level21_FirstSwitch", "Level22_OpenForRed", "Level23_ToggleTwice", "Level24_ClosedLane", "Level25_BeforeYouGo", "Level26_ThreeEntries", "Level27_BoxDoesNotPress", "Level28_TurnThrough", "Level29_ClockworkGate", "Level30_ClearThenOpen", "Level31_OnePadTwoGates", "Level32_OppositeStates", "Level33_SharedDeliveryGate", "Level34_ClearTheCorridor", "Level35_TwoChannels", "Level36_CrossBeforeClosing", "Level37_OpenTheSecondPad", "Level38_LiftAndTurn", "Level39_PrepareToggleRedirect", "Level40_TheStateOfShift" };
        [TearDown] public void Cleanup() => new SaveService(Prefix).Reset();

        private static void AssertWiring(PrototypeGame game)
        {
            var list = new SerializedObject(game).FindProperty("levels");
            Assert.That(list.arraySize, Is.EqualTo(40));
            var unique = new HashSet<Object>();
            for (int i = 0; i < 40; i++)
            {
                var asset = list.GetArrayElementAtIndex(i).objectReferenceValue;
                Assert.That(asset, Is.Not.Null, "Index " + i);
                Assert.That(unique.Add(asset), Is.True, "Duplicate at " + i);
                Assert.That(asset.name, Is.EqualTo(Expected[i]), "Index " + i);
                Assert.That(asset.name.Substring(5, 2), Is.EqualTo((i + 1).ToString("00")));
            }
        }
        [Test] public void SavedSceneHasFortyUniqueSequentialReferences()
        {
            EditorSceneManager.OpenScene(PrototypeSetup.ScenePath);
            AssertWiring(Object.FindFirstObjectByType<PrototypeGame>());
        }
        [Test] public void NewObjectSetupAssignsEveryReference()
        {
            var root = new GameObject("Wiring test");
            try { var game = root.AddComponent<PrototypeGame>(); ChapterLevelWiring.Assign(game); AssertWiring(game); }
            finally { Object.DestroyImmediate(root); }
        }
        [Test] public void ExistingSceneRestorationRepairsResizedLastElementDuplicatesAndSaves()
        {
            EditorSceneManager.OpenScene(PrototypeSetup.ScenePath);
            var game = Object.FindFirstObjectByType<PrototypeGame>();
            var serialized = new SerializedObject(game); var list = serialized.FindProperty("levels");
            list.arraySize = 10; serialized.ApplyModifiedPropertiesWithoutUndo();
            serialized.Update(); list.arraySize = 20; serialized.ApplyModifiedPropertiesWithoutUndo();
            serialized.Update();
            for (int i = 10; i < 20; i++)
                Assert.That(list.GetArrayElementAtIndex(i).objectReferenceValue, Is.EqualTo(list.GetArrayElementAtIndex(9).objectReferenceValue));
            Assert.That(ChapterLevelWiring.IsCorrect(game), Is.False);
            PrototypeSetup.CreateMissingAssets();
            AssertWiring(game);
            EditorSceneManager.OpenScene(PrototypeSetup.ScenePath);
            AssertWiring(Object.FindFirstObjectByType<PrototypeGame>());
            PrototypeSetup.CreateMissingAssets(); // Idempotent restoration of an already-correct scene.
            AssertWiring(Object.FindFirstObjectByType<PrototypeGame>());
        }
        [UnityTest] public IEnumerator CompletingTenAndElevenLoadsElevenAndTwelve()
        {
            EditorSceneManager.OpenScene(PrototypeSetup.ScenePath);
            ProgressionPresentationTests.IsolateSave(Prefix);
            var p = new LevelProgression(20, new SaveService(Prefix));
            for (int i = 0; i < 9; i++) { p.Complete(i); p.Next(); }
            yield return new EnterPlayMode(); yield return null;
            var game = Object.FindFirstObjectByType<PrototypeGame>();
            Assert.That(game.CurrentLevel.name, Is.EqualTo(Expected[9]));
            for (int index = 9; index <= 10; index++)
            {
                foreach (var tap in game.CurrentLevel.KnownSolution)
                {
                    Piece chosen = null;
                    foreach (var piece in Object.FindObjectsByType<Piece>(FindObjectsSortMode.None))
                        if (piece.Data.Type == PieceType.Normal && piece.Data.Position == tap) { chosen = piece; break; }
                    Assert.That(chosen, Is.Not.Null);
                    chosen.GetComponentInChildren<Button>().onClick.Invoke();
                    float deadline = Time.realtimeSinceStartup + 4;
                    while (game.Board.State == GameState.Resolving && Time.realtimeSinceStartup < deadline) yield return null;
                    Assert.That(game.Board.State, Is.Not.EqualTo(GameState.Resolving));
                }
                Assert.That(game.Board.State, Is.EqualTo(GameState.Won));
                Assert.That(game.NextLevel(), Is.True); yield return null;
                Assert.That(game.CurrentLevel.name, Is.EqualTo(Expected[index + 1]));
                Assert.That(game.Progress.Current, Is.EqualTo(index + 1));
                Assert.That(game.Board.State, Is.EqualTo(GameState.Playing));
            }
            yield return new ExitPlayMode();
        }
    }
}
