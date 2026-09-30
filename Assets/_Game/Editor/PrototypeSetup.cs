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
                                serialized.FindProperty("audioClips.tap").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Game/Audio/SFX/shift_tap_move.wav");
                serialized.FindProperty("audioClips.move").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Game/Audio/SFX/shift_tap_move.wav");
                serialized.FindProperty("audioClips.push").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Game/Audio/SFX/shift_push.wav");
                serialized.FindProperty("audioClips.directionChange").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Game/Audio/SFX/shift_direction.wav");
                serialized.FindProperty("audioClips.exit").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Game/Audio/SFX/shift_exit.wav");
                serialized.FindProperty("audioClips.blocked").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Game/Audio/SFX/shift_blocked.wav");
                serialized.FindProperty("audioClips.win").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Game/Audio/SFX/shift_win.wav");
                serialized.FindProperty("audioClips.rotate").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Game/Audio/SFX/shift_rotator.wav");
                serialized.FindProperty("audioClips.switchActivate").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Game/Audio/SFX/shift_switch.wav");
                serialized.FindProperty("audioClips.gateOpen").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Game/Audio/SFX/shift_gate_open.wav");
                serialized.FindProperty("audioClips.gateClose").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Game/Audio/SFX/shift_gate_close.wav");
                serialized.FindProperty("audioClips.finalExit").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Game/Audio/SFX/shift_exit.wav");
                serialized.FindProperty("audioClips.chainStep").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Game/Audio/SFX/shift_chain_pulse.wav");
                serialized.FindProperty("audioClips.chainEscalation").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Game/Audio/SFX/shift_chain_pulse.wav");
                serialized.FindProperty("audioClips.bigShift").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Game/Audio/SFX/shift_big_shift.wav");
                serialized.FindProperty("audioClips.megaShift").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Game/Audio/SFX/shift_mega_shift.wav");
                serialized.FindProperty("audioClips.undo").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Game/Audio/SFX/shift_undo.wav");
                serialized.FindProperty("audioClips.hint").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Game/Audio/SFX/shift_hint.wav");
                serialized.FindProperty("audioClips.perfect").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Game/Audio/SFX/shift_perfect.wav");
                serialized.FindProperty("audioClips.firstPerfect").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Game/Audio/SFX/shift_perfect.wav");
                serialized.FindProperty("audioClips.dailyComplete").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Game/Audio/SFX/shift_win.wav");
                serialized.FindProperty("audioClips.dailyPerfect").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Game/Audio/SFX/shift_perfect.wav");
                serialized.FindProperty("audioClips.chapterMastered").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Game/Audio/SFX/shift_perfect.wav");
                serialized.FindProperty("audioClips.campaignComplete").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Game/Audio/SFX/shift_campaign_complete.wav");
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
