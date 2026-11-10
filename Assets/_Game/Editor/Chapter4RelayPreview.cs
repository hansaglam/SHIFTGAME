using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Shift.Game.Editor
{
    public sealed class Chapter4RelayPreview : EditorWindow
    {
        private const string PreviewScene = "Assets/_Game/Scenes/Chapter4_RelayPreview.unity";
        private int selection = 61;

        [MenuItem("SHIFT/Chapter 4/Create Isolated Relay Preview Scene")]
        public static void Create()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("SHIFT Relay", "Exit Play Mode first.", "OK");
                return;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (File.Exists(PreviewScene) &&
                !EditorUtility.DisplayDialog("Rebuild Relay Preview?",
                    "Only the isolated Chapter4_RelayPreview scene is replaced. " +
                    "Your original Prototype and Chapter 3 scenes stay untouched.",
                    "Replace Preview", "Cancel")) return;

            for (int n = 41; n <= 60; n++)
                if (Chapter3DraftAudit.Get(n) == null)
                {
                    Debug.LogError("Missing Chapter 3 draft Level " + n);
                    return;
                }
            for (int n = 61; n <= 65; n++)
                if (Chapter4RelayDrafts.Get(n) == null)
                {
                    Debug.LogError("Missing Relay draft Level " + n);
                    return;
                }

            File.Copy(PrototypeSetup.ScenePath, PreviewScene, true);
            AssetDatabase.ImportAsset(PreviewScene);
            var scene = EditorSceneManager.OpenScene(PreviewScene);
            var game = Object.FindFirstObjectByType<PrototypeGame>();
            if (game == null)
            {
                Debug.LogError("Missing PrototypeGame in isolated preview.");
                return;
            }

            var serialized = new SerializedObject(game);
            var levels = serialized.FindProperty("levels");
            if (levels == null || levels.arraySize != 40)
            {
                Debug.LogError("Locked 40-level Prototype baseline not found; preview NOT saved.");
                return;
            }
            levels.arraySize = 65;
            for (int n = 41; n <= 60; n++)
                levels.GetArrayElementAtIndex(n - 1).objectReferenceValue = Chapter3DraftAudit.Get(n);
            for (int n = 61; n <= 65; n++)
                levels.GetArrayElementAtIndex(n - 1).objectReferenceValue = Chapter4RelayDrafts.Get(n);
            serialized.FindProperty("useLevelSet").boolValue = true;
            serialized.FindProperty("selectedLevel").intValue = 60;
            serialized.FindProperty("saveKeyPrefix").stringValue = "SHIFT.RelayPreview.Progress.";
            serialized.FindProperty("settingsKeyPrefix").stringValue = "SHIFT.RelayPreview.Settings.";
            serialized.FindProperty("analyticsEnabled").boolValue = false;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            if (!EditorSceneManager.SaveScene(scene))
            {
                Debug.LogError("Unable to save isolated Relay preview scene.");
                return;
            }
            Debug.Log("Chapter4_RelayPreview is ready. Play > SHIFT > Chapter 4 > Relay Preview Controls.");
            ShowControls();
        }

        [MenuItem("SHIFT/Chapter 4/Relay Preview Controls")]
        public static void ShowControls() => GetWindow<Chapter4RelayPreview>("Relay Levels");

        private static string ErrorFor(PrototypeGame game, int selected)
        {
            var current = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
            if (current != PreviewScene)
                return "Wrong scene: " + current + ". Exit Play Mode and Create Isolated Relay Preview Scene.";
            if (game == null) return "PrototypeGame not found in Relay preview scene.";
            var serial = new SerializedObject(game);
            var levels = serial.FindProperty("levels");
            if (levels == null || levels.arraySize != 65)
                return "Relay preview needs 65 references. Recreate the preview scene.";
            if (selected < 61 || selected > 65) return "Invalid Relay Level " + selected;
            var asset = levels.GetArrayElementAtIndex(selected - 1).objectReferenceValue as LevelData;
            if (asset == null || asset != Chapter4RelayDrafts.Get(selected))
                return "Level " + selected + " asset is missing or stale. Recreate the preview scene.";
            if (!asset.ValidatePlayable(out string problem))
                return "Level " + selected + " cannot load: " + problem;
            return null;
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Chapter 4 · Linked Passages", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("TEST SCENE ONLY. First 60 levels, real saves and MORA are unchanged. " +
                "Perfect Shift is not yet certified for Relay candidates.", MessageType.Info);
            selection = EditorGUILayout.IntSlider("Level", selection, 61, 65);
            var game = Object.FindFirstObjectByType<PrototypeGame>();
            var error = ErrorFor(game, selection);
            if (error != null) EditorGUILayout.HelpBox(error, MessageType.Warning);
            GUI.enabled = EditorApplication.isPlaying;
            if (GUILayout.Button("Load Relay Level " + selection, GUILayout.Height(44)))
            {
                error = ErrorFor(game, selection);
                if (error != null) Debug.LogError("Relay preview: " + error);
                else if (!game.LoadLevel(selection - 1))
                    Debug.LogError("Relay Level " + selection + " failed. Inspect Console errors.");
                else Debug.Log("Loaded Relay Level " + selection + " with separate debug save keys.");
            }
            GUI.enabled = true;
        }
    }
}
