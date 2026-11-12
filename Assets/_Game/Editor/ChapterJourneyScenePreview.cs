using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Shift.Game.Editor
{
    // Opt-in preview path, never an automatic mutation of the shipped scene.
    // The preview scene is intentionally NOT added to EditorBuildSettings.
    public static class ChapterJourneyScenePreview
    {
        public const string PreviewScenePath =
            "Assets/_Game/Scenes/SHIFT_Journey_80Level_Preview.unity";
        public const string PreviewProgressPrefix = "SHIFT.Preview.ChapterJourney.v1.";
        private const string PreviewSettingsPrefix =
            "SHIFT.Preview.ChapterJourney.Settings.v1.";

        public static LevelData[] Load80()
        {
            var first40 = ChapterLevelWiring.LoadExpected();
            if (first40.Length != 40)
                throw new InvalidOperationException("Expected the published first 40 levels.");
            var result = new LevelData[80];
            var unique = new HashSet<LevelData>();
            for (int i = 0; i < first40.Length; i++)
            {
                result[i] = first40[i];
                if (result[i] == null || !unique.Add(result[i]))
                    throw new InvalidOperationException("Missing/duplicate shipped level " + (i + 1));
            }
            for (int level = 41; level <= 80; level++)
            {
                LevelData asset = level <= 60 ? Chapter3DraftAudit.Get(level)
                    : level <= 65 ? Chapter4ResonanceDrafts.Get(level)
                    : Chapter4ResonanceExtendedDrafts.Get(level);
                if (asset == null || !unique.Add(asset))
                    throw new InvalidOperationException(
                        "Missing/duplicate Chapter Journey draft level " + level);
                result[level - 1] = asset;
            }
            return result;
        }

        [MenuItem("SHIFT/Chapter Journey/Create or Open Isolated 80-Level Preview Scene")]
        public static void CreateOrOpen()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play Mode first.");

            // Validate all assets before touching an existing scene.
            var levels = Load80();
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(PreviewScenePath) != null)
            {
                EditorSceneManager.OpenScene(PreviewScenePath, OpenSceneMode.Single);
                Debug.Log("Opened existing isolated Chapter Journey preview. " +
                    "Real Prototype scene was not modified.");
                return;
            }
            if (!AssetDatabase.CopyAsset(PrototypeSetup.ScenePath, PreviewScenePath))
                throw new InvalidOperationException(
                    "Could not copy original Prototype scene to an isolated preview path.");

            Scene scene = default;
            try
            {
                scene = EditorSceneManager.OpenScene(
                    PreviewScenePath, OpenSceneMode.Single);
                PrototypeGame game = null;
                int count = 0;
                foreach (var root in scene.GetRootGameObjects())
                    foreach (var component in
                        root.GetComponentsInChildren<PrototypeGame>(true))
                    {
                        game = component;
                        count++;
                    }
                if (count != 1)
                    throw new InvalidOperationException(
                        "Preview requires exactly one PrototypeGame (found " + count + ").");

                var serialized = new SerializedObject(game);
                var slots = serialized.FindProperty("levels");
                if (slots == null)
                    throw new InvalidOperationException("PrototypeGame levels array not found.");
                slots.arraySize = levels.Length;
                for (int i = 0; i < levels.Length; i++)
                    slots.GetArrayElementAtIndex(i).objectReferenceValue = levels[i];

                serialized.FindProperty("useLevelSet").boolValue = true;
                serialized.FindProperty("selectedLevel").intValue = 40;
                serialized.FindProperty("saveKeyPrefix").stringValue =
                    PreviewProgressPrefix;
                serialized.FindProperty("settingsKeyPrefix").stringValue =
                    PreviewSettingsPrefix;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                EditorSceneManager.MarkSceneDirty(scene);
                if (!EditorSceneManager.SaveScene(scene))
                    throw new InvalidOperationException("Preview scene could not be saved.");

                Debug.Log("Isolated 80-level Chapter Journey scene created. " +
                    "The build scene list and shipped Prototype scene were NOT changed. " +
                    "Use the explicit preview-only unlock command to browse later chapters.");
            }
            catch
            {
                // Never leave an incomplete copied scene mistaken for production.
                if (scene.IsValid() && scene.isLoaded)
                    EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                AssetDatabase.DeleteAsset(PreviewScenePath);
                throw;
            }
        }


        public const string Preview100ScenePath =
            "Assets/_Game/Scenes/SHIFT_Journey_100Level_Preview.unity";
        public const string Preview100ProgressPrefix =
            "SHIFT.Preview.Chapter5.v1.";
        private const string Preview100SettingsPrefix =
            "SHIFT.Preview.Chapter5.Settings.v1.";

        public static LevelData[] Load100()
        {
            var original = Load80();
            if (original.Length != 80)
                throw new InvalidOperationException("Draft Levels 1-80 are incomplete.");
            var all = new LevelData[100];
            var unique = new HashSet<LevelData>();
            for (int i = 0; i < original.Length; i++)
            {
                all[i] = original[i];
                if (all[i] == null || !unique.Add(all[i]))
                    throw new InvalidOperationException(
                        "Duplicated or missing original Level " + (i + 1));
            }
            for (int number = 81; number <= 100; number++)
            {
                var level = Chapter5Drafts.Get(number);
                if (level == null || !unique.Add(level))
                    throw new InvalidOperationException(
                        "Missing or repeated Chapter 5 Level " + number);
                all[number - 1] = level;
            }
            return all;
        }

        [MenuItem("SHIFT/Chapter Journey/Create or Open Isolated 100-Level Preview Scene")]
        public static void CreateOrOpen100()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play Mode first.");
            var levels = Load100(); // Fail before touching any scene.
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(Preview100ScenePath) != null)
            {
                EditorSceneManager.OpenScene(Preview100ScenePath, OpenSceneMode.Single);
                Debug.Log("Opened isolated Level 100 preview; shipped scene unchanged.");
                return;
            }
            if (!AssetDatabase.CopyAsset(PrototypeSetup.ScenePath,
                Preview100ScenePath))
                throw new InvalidOperationException("Could not copy shipped scene.");
            Scene scene = default;
            try
            {
                scene = EditorSceneManager.OpenScene(
                    Preview100ScenePath, OpenSceneMode.Single);
                PrototypeGame game = null;
                int found = 0;
                foreach (var root in scene.GetRootGameObjects())
                    foreach (var candidate in root.GetComponentsInChildren<PrototypeGame>(true))
                    {
                        game = candidate;
                        found++;
                    }
                if (found != 1)
                    throw new InvalidOperationException(
                        "Expected one PrototypeGame, found " + found);
                var serialized = new SerializedObject(game);
                var slots = serialized.FindProperty("levels");
                if (slots == null)
                    throw new InvalidOperationException("Missing levels array.");
                slots.arraySize = 100;
                for (int i = 0; i < levels.Length; i++)
                    slots.GetArrayElementAtIndex(i).objectReferenceValue = levels[i];
                serialized.FindProperty("useLevelSet").boolValue = true;
                // Runtime chapter selection loads current index from the isolated
                // SaveService, so keep the legacy Inspector's 0-39 debug slider safe.
                serialized.FindProperty("selectedLevel").intValue = 0;
                serialized.FindProperty("saveKeyPrefix").stringValue =
                    Preview100ProgressPrefix;
                serialized.FindProperty("settingsKeyPrefix").stringValue =
                    Preview100SettingsPrefix;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                EditorSceneManager.MarkSceneDirty(scene);
                if (!EditorSceneManager.SaveScene(scene))
                    throw new InvalidOperationException("Could not save preview scene.");
                Debug.Log("Level 1-100 isolated preview created, never in build scenes. " +
                    "Progress and settings are separated from player data.");
            }
            catch
            {
                if (scene.IsValid() && scene.isLoaded)
                    EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,
                        NewSceneMode.Single);
                AssetDatabase.DeleteAsset(Preview100ScenePath);
                throw;
            }
        }

        [MenuItem("SHIFT/Chapter Journey/Unlock Levels 81-100 in Preview Save Only")]
        public static void UnlockChapter5PreviewOnly()
        {
            if (!EditorUtility.DisplayDialog("SHIFT Level 100 Preview Only",
                "Open Levels 81-100 in an isolated developer preview? " +
                "Levels 1-80 will be marked completed ONLY in the preview save. " +
                "No real player data or Perfect Shift rewards will change.",
                "Unlock Preview", "Cancel"))
                return;
            var save = new SaveService(Preview100ProgressPrefix);
            var previous = save.Load(100);
            save.Save(new ProgressData(99, 80,
                Mathf.Max(79, previous.HighestCompleted)));
            Debug.Log("Preview Chapter 5 unlocked without restoring any of its " +
                "world connections. Final Level 100 beacon stays off until won.");
        }

        [MenuItem("SHIFT/Chapter Journey/Unlock Draft Chapters in Preview Save Only")]
        public static void UnlockPreviewOnly()
        {
            if (!EditorUtility.DisplayDialog("SHIFT Preview Progress Only",
                "Unlock draft levels 41-80 in the isolated Preview save? " +
                "This does not change the real player's save or certify solutions.",
                "Unlock Preview", "Cancel"))
                return;
            var saves = new SaveService(PreviewProgressPrefix);
            var previous = saves.Load(80);
            int completed = Mathf.Max(39, previous.HighestCompleted);
            saves.Save(new ProgressData(79, 40, completed));
            Debug.Log("Only the isolated Preview save was updated. " +
                "Levels 41-80 are navigable; world restoration still follows " +
                "the Preview save's HighestCompleted value.");
        }
    }
}
