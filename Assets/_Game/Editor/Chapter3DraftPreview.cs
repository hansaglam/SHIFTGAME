using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Shift.Game.Editor
{
    /// <summary>
    /// Creates a disposable Editor-only chapter preview copy. The original scene and
    /// published 40-level wiring are never saved over.
    /// </summary>
    public sealed class Chapter3DraftPreview : EditorWindow
    {
        private const string PreviewScene = "Assets/_Game/Scenes/Chapter3_DraftPreview.unity";
        private int selectedLevel = 41;

        [MenuItem("SHIFT/Chapter 3/Create Isolated Preview Scene")]
        public static void CreatePreviewScene()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("SHIFT Chapter 3", "Exit Play Mode first.", "OK");
                return;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (File.Exists(PreviewScene) &&
                !EditorUtility.DisplayDialog("Recreate Chapter 3 Preview?",
                    "Only the isolated preview scene will be replaced. Original Prototype.unity will not change.",
                    "Replace Preview", "Cancel")) return;
            for (int n = 41; n <= 60; n++)
                if (Chapter3DraftAudit.Get(n) == null)
                {
                    Debug.LogError("Missing Chapter 3 draft Level " + n + ". Re-run the draft importer.");
                    return;
                }
            File.Copy(PrototypeSetup.ScenePath, PreviewScene, true);
            AssetDatabase.ImportAsset(PreviewScene);
            var scene = EditorSceneManager.OpenScene(PreviewScene);
            var game = Object.FindFirstObjectByType<PrototypeGame>();
            if (game == null)
            {
                Debug.LogError("No PrototypeGame found in isolated preview scene.");
                return;
            }
            var serialized = new SerializedObject(game);
            var levels = serialized.FindProperty("levels");
            if (levels == null || levels.arraySize != 40)
            {
                Debug.LogError("Expected existing 40-level baseline. Preview not saved.");
                return;
            }
            levels.arraySize = 60;
            for (int n = 41; n <= 60; n++)
                levels.GetArrayElementAtIndex(n - 1).objectReferenceValue = Chapter3DraftAudit.Get(n);
            serialized.FindProperty("useLevelSet").boolValue = true;
            serialized.FindProperty("selectedLevel").intValue = 40;
            serialized.FindProperty("saveKeyPrefix").stringValue = "SHIFT.Chapter3.DraftPreview.Progress.";
            serialized.FindProperty("settingsKeyPrefix").stringValue = "SHIFT.Chapter3.DraftPreview.Settings.";
            serialized.FindProperty("analyticsEnabled").boolValue = false;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            if (!EditorSceneManager.SaveScene(scene))
            {
                Debug.LogError("Isolated preview scene could not be saved.");
                return;
            }
            Debug.Log("Created isolated Chapter 3 preview. Enter Play Mode then SHIFT > Chapter 3 > Preview Controls.");
            ShowControls();
        }

        [MenuItem("SHIFT/Chapter 3/Preview Controls")]
        public static void ShowControls()
        {
            GetWindow<Chapter3DraftPreview>("SHIFT Chapter 3");
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Chapter 3 — Level 41–60", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Editor-only preview. Save keys are isolated. Do not build or publish this test scene.",
                MessageType.Info);
            selectedLevel = EditorGUILayout.IntSlider("Level", selectedLevel, 41, 60);
            var game = Object.FindFirstObjectByType<PrototypeGame>();
            var problem = PreviewProblem(game, selectedLevel);
            if (problem != null)
                EditorGUILayout.HelpBox(problem, MessageType.Warning);
            else
                EditorGUILayout.HelpBox("Preview scene is ready for Level " + selectedLevel + ".", MessageType.Info);

            GUI.enabled = EditorApplication.isPlaying;
            if (GUILayout.Button("Load Draft Level " + selectedLevel, GUILayout.Height(42)))
            {
                problem = PreviewProblem(game, selectedLevel);
                if (problem != null)
                    Debug.LogError("Chapter 3 preview: " + problem);
                else if (!game.LoadLevel(selectedLevel - 1))
                    Debug.LogError("Chapter 3 Level " + selectedLevel +
                        " could not be loaded despite valid scene references. Check Console exceptions.");
                else
                    Debug.Log("Loaded Chapter 3 Level " + selectedLevel +
                        " in isolated debug mode (campaign progression not saved).");
            }
            GUI.enabled = true;
            if (!EditorApplication.isPlaying)
                EditorGUILayout.HelpBox("Press Play after creating the isolated preview scene.", MessageType.None);
        }

        // A precise, non-mutating diagnostic for stale previews and missing references.
        // The old generic error hid whether the scene still had only 50 levels.
        private static string PreviewProblem(PrototypeGame game, int number)
        {
            var active = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
            if (active != PreviewScene)
                return "Wrong scene (" + active +
                    "). Exit Play Mode and run SHIFT > Chapter 3 > Create Isolated Preview Scene.";
            if (game == null)
                return "PrototypeGame not found in the active preview scene.";
            var serialized = new SerializedObject(game);
            var levels = serialized.FindProperty("levels");
            if (levels == null)
                return "PrototypeGame.levels property could not be read.";
            if (levels.arraySize != 60)
                return "This preview has " + levels.arraySize +
                    " levels; Chapter 3 now needs 60. Exit Play Mode, run " +
                    "SHIFT > Chapter 3 > Create Isolated Preview Scene and choose Replace Preview.";
            if (number < 41 || number > 60)
                return "Draft level out of range: " + number + ".";
            var asset = levels.GetArrayElementAtIndex(number - 1)
                .objectReferenceValue as LevelData;
            if (asset == null)
                return "Level " + number + " is not assigned in the preview. " +
                    "Recreate the isolated preview scene and check the draft assets.";
            if (!asset.ValidatePlayable(out string error))
                return "Level " + number + " is assigned but invalid: " + error;
            var expected = Chapter3DraftAudit.Get(number);
            if (expected == null)
                return "Level " + number + " asset is missing from Chapter3_Drafts.";
            if (asset != expected)
                return "Level " + number + " references a different asset. Recreate the preview scene.";
            return null;
        }
    }
}
