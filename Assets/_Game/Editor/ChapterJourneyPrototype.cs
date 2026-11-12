using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Shift.Game.Editor
{
    // Isolated UX concept. Does not alter Campaign, saves, onboarding, MORA,
    // or any shipping scene. Unlocking requires a separately approved runtime pass.
    public static class ChapterJourneyPrototype
    {
        [Serializable]
        public sealed class PreviewExport
        {
            public string status = "EDITOR PREVIEW - not shipping progression";
            public ChapterJourneyCatalog.Chapter[] chapters;
        }

        // The editor uses the SAME bilingual copy as the runtime UI.
        // No duplicated stage data or alternate source of progress truth.
        public static ChapterJourneyCatalog.Chapter[] Chapters => ChapterJourneyCatalog.Chapters;
        public static ChapterJourneyCatalog.Chapter ForLevel(int level) =>
            ChapterJourneyCatalog.ForLevel(level);

        public static int MilestoneIndex(ChapterJourneyCatalog.Chapter chapter, int level)
        {
            if (chapter == null) return -1;
            int index = -1;
            for (int i = 0; i < chapter.beats.Length; i++)
                if (level >= chapter.beats[i].level) index = i;
            return index;
        }

        public static bool IsBeatRestored(ChapterJourneyCatalog.Chapter chapter,
            ChapterJourneyCatalog.Beat beat, int level, bool currentLevelWon)
        {
            if (chapter == null || beat == null) return false;
            // A stage changes ONLY when its level was actually completed.
            // The preview's level is 1-based, not a persisted completion index.
            return level > beat.level || level == beat.level && currentLevelWon;
        }

        public static float PreviewProgress(int level, bool currentLevelWon)
        {
            var chapter = ForLevel(level);
            if (chapter == null) return 0f;
            int completedIndex = level - 2 + (currentLevelWon ? 1 : 0);
            return ChapterJourneyCatalog.Progress01(chapter, completedIndex);
        }

        [MenuItem("SHIFT/Chapter Journey/Preview 41-100")]
        private static void Open()
        {
            EditorWindow.GetWindow<ChapterJourneyPreviewWindow>(
                "SHIFT Chapter Journey");
        }

        [MenuItem("SHIFT/Chapter Journey/Export EN-TR Design Draft")]
        private static void Export()
        {
            var folder = Path.Combine(Application.dataPath, "../Validation");
            Directory.CreateDirectory(folder);
            var output = Path.Combine(folder, "chapter3_5_journey_editor_draft.json");
            File.WriteAllText(output, JsonUtility.ToJson(new PreviewExport
            {
                chapters = Chapters
            }, true));
            Debug.Log("Editor-only chapter journey concept exported: " + output +
                ". No runtime progression or save data changed.");
        }
    }

    public sealed class ChapterJourneyPreviewWindow : EditorWindow
    {
        private int level = 41;
        private bool currentLevelWon;
        private bool turkish = true;
        private bool reducedMotion;
        private Vector2 scroll;

        private void OnGUI()
        {
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("41-100 / PURPOSE & PROGRESSION",
                EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "DESIGN PREVIEW ONLY: no runtime HUD, save, chapter unlock, " +
                "game assets or Perfect Shift state is modified.",
                MessageType.Info);
            int nextLevel = EditorGUILayout.IntSlider("Preview level", level, 41, 100);
            if (nextLevel != level)
            {
                level = nextLevel;
                currentLevelWon = false;
            }
            currentLevelWon = EditorGUILayout.Toggle("Current level won", currentLevelWon);
            turkish = EditorGUILayout.Toggle("Turkish copy", turkish);
            reducedMotion = EditorGUILayout.Toggle("Reduced motion", reducedMotion);

            var chapter = ChapterJourneyPrototype.ForLevel(level);
            if (chapter == null) return;
            float progress = ChapterJourneyPrototype.PreviewProgress(level, currentLevelWon);
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField(
                turkish ? chapter.titleTr : chapter.titleEn,
                EditorStyles.largeLabel);
            EditorGUILayout.LabelField(
                turkish ? chapter.purposeTr : chapter.purposeEn,
                EditorStyles.wordWrappedLabel);
            var bar = GUILayoutUtility.GetRect(1, 20, GUILayout.ExpandWidth(true));
            EditorGUI.ProgressBar(bar, progress,
                Mathf.RoundToInt(progress * 100f) + "% / chapter route");
            EditorGUILayout.Space(8);

            scroll = EditorGUILayout.BeginScrollView(scroll);
            for (int i = 0; i < chapter.beats.Length; i++)
            {
                var beat = chapter.beats[i];
                // Merely opening the finale cannot visually restore the world.
                bool reached = ChapterJourneyPrototype.IsBeatRestored(
                    chapter, beat, level, currentLevelWon);
                Color c;
                var hex = reached ? chapter.paletteEnd : chapter.paletteStart;
                if (!ColorUtility.TryParseHtmlString(hex, out c))
                    c = Color.gray;
                var swatch = GUILayoutUtility.GetRect(1, 7, GUILayout.ExpandWidth(true));
                EditorGUI.DrawRect(swatch, reached ? c : new Color(.35f, .4f, .47f, 1f));
                EditorGUILayout.LabelField(
                    beat.level + "  " + (turkish ? beat.nameTr : beat.nameEn),
                    EditorStyles.boldLabel);
                EditorGUILayout.LabelField(
                    turkish ? beat.worldTr : beat.worldEn,
                    EditorStyles.wordWrappedLabel);
                EditorGUILayout.LabelField(
                    reducedMotion
                        ? (turkish ? beat.stillTr : beat.stillEn)
                        : (turkish ? beat.motionTr : beat.motionEn),
                    EditorStyles.wordWrappedMiniLabel);
                EditorGUILayout.Space(9);
            }
            if (level == chapter.endLevel && currentLevelWon)
            {
                EditorGUILayout.HelpBox(
                    turkish ? chapter.finaleTr : chapter.finaleEn,
                    MessageType.None);
            }
            EditorGUILayout.EndScrollView();
        }
    }
}
