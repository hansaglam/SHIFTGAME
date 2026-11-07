using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Shift.Game.Editor
{
    /// <summary>
    /// Creates a test-only scene from the known 40-level baseline. Chapter 3 and
    /// Echo pilot assets are attached to the copy, not the release scene.
    /// </summary>
    public sealed class Chapter4EchoPreview : EditorWindow
    {
        private const string PreviewScene = "Assets/_Game/Scenes/Chapter4_EchoPreview.unity";
        private int selectedLevel = 61;

        [MenuItem("SHIFT/Chapter 4/Create Isolated Echo Preview Scene")]
        public static void Create()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("SHIFT Echo Pilot", "Exit Play Mode first.", "OK");
                return;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (File.Exists(PreviewScene) &&
                !EditorUtility.DisplayDialog("Recreate Echo Preview?",
                "This replaces only Chapter4_EchoPreview.unity. Prototype.unity and " +
                "Chapter3_DraftPreview.unity will not be changed.",
                "Replace Preview", "Cancel")) return;

            for (int n = 41; n <= 60; n++)
            {
                if (Chapter3DraftAudit.Get(n) == null)
                {
                    Debug.LogError("Missing Chapter 3 draft Level " + n);
                    return;
                }
            }
            for (int n = 61; n <= 65; n++)
            {
                if (Chapter4EchoDrafts.Get(n) == null)
                {
                    Debug.LogError("Missing Chapter 4 Echo draft Level " + n);
                    return;
                }
            }

            File.Copy(PrototypeSetup.ScenePath, PreviewScene, true);
            AssetDatabase.ImportAsset(PreviewScene);
            var scene = EditorSceneManager.OpenScene(PreviewScene);
            var game = Object.FindFirstObjectByType<PrototypeGame>();
            if (game == null)
            {
                Debug.LogError("Missing PrototypeGame in source scene.");
                return;
            }
            var serialized = new SerializedObject(game);
            var levels = serialized.FindProperty("levels");
            if (levels == null || levels.arraySize != 40)
            {
                Debug.LogError("Expected locked 40-level source scene. Echo preview not saved.");
                return;
            }

            levels.arraySize = 65;
            for (int n = 41; n <= 60; n++)
                levels.GetArrayElementAtIndex(n - 1).objectReferenceValue = Chapter3DraftAudit.Get(n);
            for (int n = 61; n <= 65; n++)
                levels.GetArrayElementAtIndex(n - 1).objectReferenceValue = Chapter4EchoDrafts.Get(n);

            serialized.FindProperty("useLevelSet").boolValue = true;
            serialized.FindProperty("selectedLevel").intValue = 60;
            serialized.FindProperty("saveKeyPrefix").stringValue = "SHIFT.Chapter4.EchoPilot.Progress.";
            serialized.FindProperty("settingsKeyPrefix").stringValue = "SHIFT.Chapter4.EchoPilot.Settings.";
            serialized.FindProperty("analyticsEnabled").boolValue = false;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            if (!EditorSceneManager.SaveScene(scene))
            {
                Debug.LogError("Unable to save the isolated Echo preview.");
                return;
            }
            Debug.Log("Created isolated Chapter 4 Echo preview with 65 level references. " +
                "Enter Play Mode and open SHIFT > Chapter 4 > Preview Controls.");
            ShowControls();
        }

        [MenuItem("SHIFT/Chapter 4/Preview Controls")]
        public static void ShowControls() =>
            GetWindow<Chapter4EchoPreview>("SHIFT Echo Pilot");

        private static string Problem(PrototypeGame game, int levelNumber)
        {
            var scenePath = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
            if (scenePath != PreviewScene)
                return "Wrong scene. Exit Play Mode, open SHIFT > Chapter 4 > " +
                    "Create Isolated Echo Preview Scene.";
            if (game == null) return "PrototypeGame component is missing.";
            var serial = new SerializedObject(game);
            var levels = serial.FindProperty("levels");
            if (levels == null || levels.arraySize != 65)
                return "Echo preview needs 65 level references; recreate the preview scene.";
            var asset = levels.GetArrayElementAtIndex(levelNumber - 1).objectReferenceValue as LevelData;
            if (asset == null || asset != Chapter4EchoDrafts.Get(levelNumber))
                return "Echo Level " + levelNumber + " is missing or points to a stale asset.";
            if (!asset.ValidatePlayable(out var error))
                return "Echo Level " + levelNumber + " invalid: " + error;
            return null;
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Chapter 4 — Echo (61–65)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Editor-only prototype. Never build or publish this test scene. " +
                "Perfect Shift rewards remain disabled until certified.", MessageType.Info);
            selectedLevel = EditorGUILayout.IntSlider("Level", selectedLevel, 61, 65);
            var game = Object.FindFirstObjectByType<PrototypeGame>();
            var problem = Problem(game, selectedLevel);
            if (problem != null) EditorGUILayout.HelpBox(problem, MessageType.Warning);
            GUI.enabled = EditorApplication.isPlaying;
            if (GUILayout.Button("Load Echo Level " + selectedLevel, GUILayout.Height(42)))
            {
                problem = Problem(game, selectedLevel);
                if (problem != null)
                    Debug.LogError("Echo pilot: " + problem);
                else if (!game.LoadLevel(selectedLevel - 1))
                    Debug.LogError("Echo Level " + selectedLevel + " failed to load. Inspect Console.");
                else
                    Debug.Log("Loaded Echo Level " + selectedLevel +
                        " in isolated debug mode. Real campaign progress is unchanged.");
            }
            GUI.enabled = true;
        }
    }
}
