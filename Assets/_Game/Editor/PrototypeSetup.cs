using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Shift.Game.Editor
{
    public static class PrototypeSetup
    {
        public const string ScenePath = "Assets/_Game/Scenes/Prototype.unity";
        public const string LevelPath = "Assets/_Game/Data/Levels/FirstChain.asset";

        [MenuItem("SHIFT/Open Prototype")]
        public static void OpenPrototype()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.OpenScene(ScenePath);
        }

        [MenuItem("SHIFT/Create Missing Prototype Assets")]
        public static void CreateMissingAssets()
        {
            var level = AssetDatabase.LoadAssetAtPath<LevelData>(LevelPath);
            if (level == null)
            {
                level = ScriptableObject.CreateInstance<LevelData>();
                level.Configure(6, 6, 3, PieceColor.Red, new[]
                {
                    new PiecePlacement(PieceType.Normal, PieceColor.Yellow, Direction.Right, new GridPosition(1, 2)),
                    new PiecePlacement(PieceType.PushBlock, PieceColor.None, Direction.None, new GridPosition(2, 2)),
                    new PiecePlacement(PieceType.Normal, PieceColor.Red, Direction.Up, new GridPosition(3, 2)),
                    new PiecePlacement(PieceType.Exit, PieceColor.None, Direction.None, new GridPosition(4, 2))
                });
                AssetDatabase.CreateAsset(level, LevelPath);
            }
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) == null)
            {
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
                var root = new GameObject("SHIFT Prototype");
                SceneManager.MoveGameObjectToScene(root, scene);
                var game = root.AddComponent<PrototypeGame>();
                var serialized = new SerializedObject(game);
                serialized.FindProperty("level").objectReferenceValue = level; serialized.ApplyModifiedPropertiesWithoutUndo();
                ChapterLevelWiring.Assign(game);
                EditorSceneManager.SaveScene(scene, ScenePath); EditorSceneManager.CloseScene(scene, true);
            }
            RepairSceneReferences();
            var scenes = EditorBuildSettings.scenes.Where(s => s.path != ScenePath).ToList();
            scenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true)); EditorBuildSettings.scenes = scenes.ToArray();
            AssetDatabase.SaveAssets();
        }

        [MenuItem("SHIFT/Repair Prototype Level References")]
        public static void RepairSceneReferences()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new System.InvalidOperationException("Exit Play Mode before repairing the saved scene.");
            ChapterLevelWiring.LoadExpected();
            var scene = SceneManager.GetSceneByPath(ScenePath);
            bool openedHere = !scene.IsValid() || !scene.isLoaded;
            if (openedHere) scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            try
            {
                int count = 0;
                foreach (var root in scene.GetRootGameObjects())
                    foreach (var game in root.GetComponentsInChildren<PrototypeGame>(true))
                    { ChapterLevelWiring.Assign(game); count++; }
                if (count != 1) throw new System.InvalidOperationException("Expected one PrototypeGame in Prototype scene.");
                EditorSceneManager.MarkSceneDirty(scene);
                if (!EditorSceneManager.SaveScene(scene)) throw new System.InvalidOperationException("Could not save Prototype scene.");
            }
            finally { if (openedHere) EditorSceneManager.CloseScene(scene, true); }
        }
    }
}
