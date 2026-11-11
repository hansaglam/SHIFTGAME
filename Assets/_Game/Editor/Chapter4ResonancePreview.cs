using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Shift.Game.Editor
{
    /// <summary>
    /// Editor-only experiment; never rewires the shipping Prototype scene.
    /// </summary>
    public sealed class Chapter4ResonancePreview : EditorWindow
    {
        private const string PreviewScene = "Assets/_Game/Scenes/Chapter4_ResonancePreview.unity";
        private int selection = 66;

        private static LevelData Draft(int number) => number <= 65
            ? Chapter4ResonanceDrafts.Get(number)
            : Chapter4ResonanceExtendedDrafts.Get(number);

        [MenuItem("SHIFT/Chapter 4/Create Isolated Resonance Preview Scene")]
        public static void Create()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("SHIFT Resonance", "Exit Play Mode first.", "OK");
                return;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (File.Exists(PreviewScene) &&
                !EditorUtility.DisplayDialog("Rebuild Resonance Preview?",
                    "Only Chapter4_ResonancePreview.unity will be replaced. " +
                    "Prototype.unity and Chapter3_DraftPreview.unity remain unchanged.",
                    "Replace Preview", "Cancel")) return;

            for (int n=41; n<=60; n++)
                if (Chapter3DraftAudit.Get(n) == null)
                {
                    Debug.LogError("Missing Chapter 3 Level " + n);
                    return;
                }
            for (int n=61; n<=80; n++)
                if (Draft(n) == null)
                {
                    Debug.LogError("Missing Resonance Level " + n);
                    return;
                }
            File.Copy(PrototypeSetup.ScenePath, PreviewScene, true);
            AssetDatabase.ImportAsset(PreviewScene);
            var scene = EditorSceneManager.OpenScene(PreviewScene);
            var game = Object.FindFirstObjectByType<PrototypeGame>();
            if (game == null)
            {
                Debug.LogError("No PrototypeGame found in the preview scene.");
                return;
            }
            var serialized = new SerializedObject(game);
            var levels = serialized.FindProperty("levels");
            if (levels == null || levels.arraySize != 40)
            {
                Debug.LogError("Expected locked 40-level Prototype baseline. Preview was NOT saved.");
                return;
            }
            levels.arraySize = 80;
            for (int n=41; n<=60; n++)
                levels.GetArrayElementAtIndex(n-1).objectReferenceValue = Chapter3DraftAudit.Get(n);
            for (int n=61; n<=80; n++)
                levels.GetArrayElementAtIndex(n-1).objectReferenceValue = Draft(n);
            serialized.FindProperty("useLevelSet").boolValue = true;
            serialized.FindProperty("selectedLevel").intValue = 60;
            serialized.FindProperty("saveKeyPrefix").stringValue = "SHIFT.Chapter4.ResonancePreview.Progress.";
            serialized.FindProperty("settingsKeyPrefix").stringValue = "SHIFT.Chapter4.ResonancePreview.Settings.";
            serialized.FindProperty("analyticsEnabled").boolValue = false;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            if (!EditorSceneManager.SaveScene(scene))
            {
                Debug.LogError("Unable to save Resonance preview scene.");
                return;
            }
            Debug.Log("Created isolated 80-level Resonance preview. " +
                "Play Mode > SHIFT > Chapter 4 > Resonance Preview Controls. " +
                "No real campaign progress is saved.");
            ShowControls();
        }

        [MenuItem("SHIFT/Chapter 4/Resonance Preview Controls")]
        public static void ShowControls() =>
            GetWindow<Chapter4ResonancePreview>("SHIFT Resonance");

        private static string Problem(PrototypeGame game, int number)
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
            if (scene != PreviewScene)
                return "Wrong active scene: " + scene + ". Exit Play Mode then run Create Isolated Resonance Preview Scene.";
            if (game == null) return "PrototypeGame is missing from the preview scene.";
            var serialized = new SerializedObject(game);
            var levels = serialized.FindProperty("levels");
            if (levels == null || levels.arraySize != 80)
                return "This preview has stale level references; 80 required. " +
                    "Exit Play Mode and recreate the isolated Resonance preview.";
            if (number < 61 || number > 80) return "Invalid Resonance level number.";
            var level = levels.GetArrayElementAtIndex(number - 1).objectReferenceValue as LevelData;
            if (level == null || level != Draft(number))
                return "Missing or mismatched Level " + number + " asset. Recreate the preview scene.";
            if (!level.ValidatePlayable(out string error))
                return "Invalid Level " + number + ": " + error;
            return null;
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Chapter 4 — Resonance 61–80", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Experimental scene only. The first 60 levels, MORA, " +
                "production saves and Perfect rewards remain unchanged.", MessageType.Info);
            selection = EditorGUILayout.IntSlider("Level", selection, 61, 80);
            var game = Object.FindFirstObjectByType<PrototypeGame>();
            var problem = Problem(game, selection);
            if (problem != null) EditorGUILayout.HelpBox(problem, MessageType.Warning);
            else EditorGUILayout.HelpBox("Preview is ready for Level " + selection + ".", MessageType.Info);
            GUI.enabled = EditorApplication.isPlaying;
            if (GUILayout.Button("Load Resonance Level " + selection, GUILayout.Height(44)))
            {
                problem = Problem(game, selection);
                if (problem != null) Debug.LogError("Resonance preview: " + problem);
                else if (!game.LoadLevel(selection - 1))
                    Debug.LogError("Resonance Level " + selection + " load failed. Check Console exceptions.");
                else Debug.Log("Loaded Resonance Level " + selection + " with isolated test save keys.");
            }
            GUI.enabled = true;
        }
    }
}
