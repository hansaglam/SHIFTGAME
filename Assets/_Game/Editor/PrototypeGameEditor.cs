using UnityEditor;
using UnityEngine;

namespace Shift.Game.Editor
{
    [CustomEditor(typeof(PrototypeGame))]
    public sealed class PrototypeGameEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "levels");
            using (new EditorGUI.DisabledScope(true))
                EditorGUILayout.PropertyField(serializedObject.FindProperty("levels"), true);
            serializedObject.ApplyModifiedProperties();
            if (!ChapterLevelWiring.IsCorrect((PrototypeGame)target))
                EditorGUILayout.HelpBox("Chapter references are invalid. Repair before playing.", MessageType.Error);
            using (new EditorGUI.DisabledScope(Application.isPlaying))
                if (GUILayout.Button("Repair and Save Chapter References"))
                    PrototypeSetup.RepairSceneReferences();
            if (Application.isPlaying && GUILayout.Button("Load Selected Level"))
                ((PrototypeGame)target).LoadLevel(serializedObject.FindProperty("selectedLevel").intValue);
            if (GUILayout.Button("Reset Progress"))
            {
                if (Application.isPlaying) ((PrototypeGame)target).ResetProgress();
                else new SaveService(serializedObject.FindProperty("saveKeyPrefix").stringValue).Reset();
            }
            if (GUILayout.Button("Unlock All Levels"))
            {
                if (Application.isPlaying) ((PrototypeGame)target).UnlockAllLevels();
                else new LevelProgression(LevelValidation.AllLevels().Length, new SaveService(serializedObject.FindProperty("saveKeyPrefix").stringValue)).UnlockAll();
            }
            EditorGUILayout.HelpBox("Index 0–19. Play resumes saved progress; Load Selected Level is an Editor-only bypass that never saves completion. Disable Use Level Set for FirstChain. Reset affects only SHIFT progress keys.", MessageType.Info);
        }
    }
}
