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
            for (int n = 41; n <= 50; n++)
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
            levels.arraySize = 50;
            for (int n = 41; n <= 50; n++)
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
            EditorGUILayout.LabelField("Chapter 3 — Level 41–50", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Editor-only preview. Save keys are isolated. Do not build or publish this test scene.",
                MessageType.Info);
            selectedLevel = EditorGUILayout.IntSlider("Level", selectedLevel, 41, 50);
            GUI.enabled = EditorApplication.isPlaying;
            if (GUILayout.Button("Load Draft Level " + selectedLevel, GUILayout.Height(42)))
            {
                var game = Object.FindFirstObjectByType<PrototypeGame>();
                if (game == null || !game.LoadLevel(selectedLevel - 1))
                    Debug.LogError("Unable to load draft level. Open Chapter3_DraftPreview and enter Play Mode.");
                else
                    Debug.Log("Loaded Chapter 3 Level " + selectedLevel + " in debug mode (no campaign progress saved).");
            }
            GUI.enabled = true;
            if (!EditorApplication.isPlaying)
                EditorGUILayout.HelpBox("Open the isolated preview scene and enter Play Mode first.", MessageType.None);
        }
    }
}
