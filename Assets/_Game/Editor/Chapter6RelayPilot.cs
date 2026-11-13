using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Shift.Game.Editor
{
    // Experimental Chapter 6 timing mechanic, never a full 20-level chapter.
    public static class Chapter6RelayPilot
    {
        public const string Folder =
            "Assets/_Game/Data/Levels/Chapter6_RelayPrototypes/";
        public const string ScenePath =
            "Assets/_Game/Scenes/SHIFT_Relay_101_103_Preview.unity";
        public const string SavePrefix = "SHIFT.Preview.Chapter6Relay.v1.";
        public const string SettingsPrefix =
            "SHIFT.Preview.Chapter6Relay.Settings.v1.";

        private static readonly string[] Names =
        {
            "Level101_TheWaitingMove",
            "Level102_OneTapTwoPaths",
            "Level103_DelayedTriple"
        };

        public static LevelData Get(int level) =>
            level >= 101 && level <= 103
                ? AssetDatabase.LoadAssetAtPath<LevelData>(
                    Folder + Names[level - 101] + ".asset")
                : null;

        // Distinct legal orders: not synthetic extra solo taps.
        public static GridPosition[] Alternative(int level)
        {
            if (level == 101)
                return new[] {
                    new GridPosition(0,2), new GridPosition(0,1),
                    new GridPosition(1,2), new GridPosition(2,1) };
            if (level == 102)
                return new[] {
                    new GridPosition(2,1), new GridPosition(0,1),
                    new GridPosition(0,2), new GridPosition(2,1) };
            return null; // Delayed Triple has an intentional two-tap reveal.
        }

        public static bool VerifyAlternative(int level, out string error)
        {
            error = null;
            var route = Alternative(level);
            if (route == null) return true;
            var data = Get(level);
            if (data == null || route.Length > data.MoveLimit)
            {
                error = "Invalid RÖLE alternative asset or move budget.";
                return false;
            }
            var board = new BoardManager();
            board.Load(data);
            foreach (var tap in route)
            {
                if (!board.RequestMove(tap) || board.LastReactionWasCancelled ||
                    board.ReactionDepth == 0)
                {
                    error = "Alternative tap did not commit: " + tap;
                    return false;
                }
                board.CompleteResolution();
            }
            if (board.State != GameState.Won)
                error = "RÖLE alternate did not win.";
            return error == null;
        }

        public static LevelData[] Load103()
        {
            var first100 = ChapterJourneyScenePreview.Load100();
            if (first100.Length != 100)
                throw new InvalidOperationException("Missing first 100 levels.");
            var result = new LevelData[103];
            var unique = new HashSet<LevelData>();
            for (int i = 0; i < 100; i++)
            {
                result[i] = first100[i];
                if (result[i] == null || !unique.Add(result[i]))
                    throw new InvalidOperationException(
                        "Old campaign missing/duplicate: " + (i + 1));
            }
            for (int i = 100; i < 103; i++)
            {
                result[i] = Get(i + 1);
                if (result[i] == null || !unique.Add(result[i]))
                    throw new InvalidOperationException(
                        "RÖLE prototype missing/duplicate: " + (i + 1));
            }
            return result;
        }

        [MenuItem("SHIFT/Chapter 6 RÖLE/Validate 101-103")]
        public static void Validate()
        {
            int passed = 0;
            for (int n = 101; n <= 103; n++)
            {
                var level = Get(n);
                string error = null; // Explicitly initialized before OR.
                if (level == null || !level.ValidatePlayable(out error))
                {
                    Debug.LogError("RÖLE " + n + ": " +
                        (error ?? "Missing LevelData"));
                    continue;
                }
                if (!LevelValidation.VerifySolution(level, out error,
                    out int reactionDepth))
                {
                    Debug.LogError("RÖLE " + n + " route: " + error);
                    continue;
                }
                if (!VerifyAlternative(n, out error))
                {
                    Debug.LogError("RÖLE " + n +
                        " alternative failed: " + error);
                    continue;
                }
                var proof = CampaignCertification.Prove(level, n);
                if (!proof.complete ||
                    proof.minimum != level.KnownSolutionLength)
                {
                    Debug.LogError("RÖLE " + n + " minimum mismatch: " +
                        proof.minimum + " / " + proof.error);
                    continue;
                }
                var board = new BoardManager();
                board.Load(level);
                int armed = 0, released = 0, tripleTaps = 0;
                foreach (var tap in level.KnownSolution)
                {
                    if (!board.RequestMove(tap) ||
                        board.LastReactionWasCancelled)
                        throw new InvalidOperationException(
                            "RÖLE failed action: " + tap);
                    var moved = new HashSet<int>();
                    foreach (var action in board.Actions)
                    {
                        if (action.Type == BoardActionType.RelayArmed)
                            armed++;
                        if (action.Type == BoardActionType.RelayReleased)
                            released++;
                        if (action.Type == BoardActionType.Move)
                            moved.Add(action.PieceId);
                    }
                    if (moved.Count >= 3) tripleTaps++;
                    board.CompleteResolution();
                }
                if (board.State != GameState.Won || armed == 0 ||
                    released == 0 || (n == 103 && tripleTaps < 1))
                {
                    Debug.LogError("RÖLE " + n +
                        ": waiting motion not meaningfully used.");
                    continue;
                }
                passed++;
                Debug.Log("RÖLE " + n + " passed, shortest=" +
                    proof.minimum + ", states=" + proof.states +
                    ", armed=" + armed + ", released=" + released +
                    ", triple=" + tripleTaps + ".", level);
            }
            Debug.Log("RÖLE Unity checks " + passed + "/3. " +
                "Never merge without Game-view visual/player validation.");
            if (passed != 3)
                Debug.LogError("RÖLE pilot failed. Do not expand Level 104+.");
        }

        private static void Wire(Scene scene, LevelData[] levels)
        {
            PrototypeGame game = null;
            foreach (var root in scene.GetRootGameObjects())
                foreach (var found in
                    root.GetComponentsInChildren<PrototypeGame>(true))
                {
                    if (game != null)
                        throw new InvalidOperationException(
                            "Multiple PrototypeGame objects.");
                    game = found;
                }
            if (game == null)
                throw new InvalidOperationException(
                    "Preview lacks PrototypeGame.");
            var serialized = new SerializedObject(game);
            serialized.Update();
            var array = serialized.FindProperty("levels");
            var enabled = serialized.FindProperty("useLevelSet");
            var selected = serialized.FindProperty("selectedLevel");
            var key = serialized.FindProperty("saveKeyPrefix");
            var settings = serialized.FindProperty("settingsKeyPrefix");
            if (array == null || enabled == null || selected == null ||
                key == null || settings == null)
                throw new InvalidOperationException(
                    "Preview campaign fields missing.");
            array.arraySize = levels.Length;
            for (int i = 0; i < levels.Length; i++)
                array.GetArrayElementAtIndex(i).objectReferenceValue =
                    levels[i];
            enabled.boolValue = true;
            selected.intValue = 0;
            key.stringValue = SavePrefix;
            settings.stringValue = SettingsPrefix;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene))
                throw new InvalidOperationException(
                    "Cannot save isolated RÖLE preview.");
        }

        [MenuItem("SHIFT/Chapter 6 RÖLE/Create or Repair Preview")]
        public static void Preview()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("Stop Play Mode before opening RÖLE.");
                return;
            }
            var levels = Load103();
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;
            bool exists =
                AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null;
            if (!exists &&
                !AssetDatabase.CopyAsset(PrototypeSetup.ScenePath, ScenePath))
                throw new InvalidOperationException(
                    "Unable to copy prototype scene.");
            Scene scene = EditorSceneManager.OpenScene(ScenePath,
                OpenSceneMode.Single);
            Wire(scene, levels);
            if (!exists)
            {
                // Separate local-only preview save; no live save keys.
                var save = new SaveService(SavePrefix);
                save.Save(new ProgressData(102, 100, 99));
            }
            Debug.Log("RÖLE preview repaired with 103 levels. " +
                "Select a pilot level while NOT in Play Mode.");
        }

        private static void Select(int number)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("Stop Play Mode before switching RÖLE.");
                return;
            }
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) == null)
            {
                Preview();
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath)
                    == null) return;
            }
            var levels = Load103();
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;
            Scene scene = EditorSceneManager.OpenScene(ScenePath,
                OpenSceneMode.Single);
            Wire(scene, levels);
            var save = new SaveService(SavePrefix);
            save.Save(new ProgressData(102, number - 1,
                Math.Max(99, number - 2)));
            Debug.Log("RÖLE Level " + number +
                " selected. Press Play now.");
        }

        [MenuItem("SHIFT/Chapter 6 RÖLE/Preview Level 101")]
        public static void Level101() => Select(101);
        [MenuItem("SHIFT/Chapter 6 RÖLE/Preview Level 102")]
        public static void Level102() => Select(102);
        [MenuItem("SHIFT/Chapter 6 RÖLE/Preview Level 103")]
        public static void Level103() => Select(103);

        [MenuItem("SHIFT/Chapter 6 RÖLE/Create or Repair Preview", true)]
        [MenuItem("SHIFT/Chapter 6 RÖLE/Preview Level 101", true)]
        [MenuItem("SHIFT/Chapter 6 RÖLE/Preview Level 102", true)]
        [MenuItem("SHIFT/Chapter 6 RÖLE/Preview Level 103", true)]
        public static bool IsPreviewSafe() =>
            !EditorApplication.isPlayingOrWillChangePlaymode;
    }
}
