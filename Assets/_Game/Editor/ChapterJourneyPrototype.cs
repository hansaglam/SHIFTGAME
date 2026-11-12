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
        public sealed class Beat
        {
            public int level;
            public string nameTr, nameEn;
            public string worldTr, worldEn;
            public string motionTr, motionEn;
            public string stillTr, stillEn;

            public Beat(int level, string nameTr, string nameEn,
                string worldTr, string worldEn,
                string motionTr, string motionEn,
                string stillTr, string stillEn)
            {
                this.level = level;
                this.nameTr = nameTr; this.nameEn = nameEn;
                this.worldTr = worldTr; this.worldEn = worldEn;
                this.motionTr = motionTr; this.motionEn = motionEn;
                this.stillTr = stillTr; this.stillEn = stillEn;
            }
        }

        [Serializable]
        public sealed class Chapter
        {
            public int startLevel, endLevel;
            public string titleTr, titleEn, purposeTr, purposeEn;
            public string finaleTr, finaleEn;
            public string paletteStart, paletteEnd;
            public Beat[] beats;

            public Chapter(int start, int end, string tr, string en,
                string purposeTr, string purposeEn, string finaleTr, string finaleEn,
                string paletteStart, string paletteEnd, Beat[] beats)
            {
                startLevel = start; endLevel = end;
                titleTr = tr; titleEn = en;
                this.purposeTr = purposeTr; this.purposeEn = purposeEn;
                this.finaleTr = finaleTr; this.finaleEn = finaleEn;
                this.paletteStart = paletteStart; this.paletteEnd = paletteEnd;
                this.beats = beats;
            }
        }

        [Serializable]
        public sealed class PreviewExport
        {
            public string status = "EDITOR CONCEPT ONLY - not runtime progression";
            public Chapter[] chapters;
        }

        public static readonly Chapter[] Chapters =
        {
            new Chapter(41, 60, "Kopan Hatlar", "Fractured Lines",
                "Dağın altındaki kopmuş akış hatlarını yeniden birbirine bağla.",
                "Reconnect the broken flow beneath the valley.",
                "Hat yeniden çalışıyor. Uzakta yeni bir sinyal belirdi.",
                "The line flows again. A distant signal has appeared.",
                "#24465F", "#5AA9A5", new[]
                {
                    new Beat(41, "İlk Kıvılcım", "First Spark",
                        "Göl kıyısındaki hatlar soluk ve kopuk.",
                        "Faint, disconnected traces along the lakeshore.",
                        "Bir bağlantı çizgisi kısa süre aydınlanır.",
                        "One connection glows briefly.",
                        "Bir çizgi açık renkte görünür.",
                        "One connection remains bright."),
                    new Beat(45, "Akış Başlıyor", "Flow Returns",
                        "Kıyı boyunca ilk sürekli ışık yolu belirir.",
                        "The first continuous light trail appears near the water.",
                        "Işık yolu tek bir kez takip edilir.",
                        "A single light sweep follows the route.",
                        "Hat sabit aydınlığa geçer.",
                        "The route becomes steadily illuminated."),
                    new Beat(50, "Bağlantı Merkezi", "Relay Junction",
                        "Uzak düğüm ve yakın kıyı birbirine bağlanır.",
                        "The distant junction connects to the near shore.",
                        "Uzak düğüme tek bir ışık aktarımı yapılır.",
                        "One light pulse reaches the distant junction.",
                        "İki düğüm eşit parlaklıkta görünür.",
                        "Both junctions remain equally bright."),
                    new Beat(55, "Son Kopuk Hat", "Final Broken Link",
                        "Tek bir eksik bağlantı kalan akışı engeller.",
                        "One missing connection blocks the remaining flow.",
                        "Eksik bağlantı ritmik olmayan yumuşak ışıkla belirtilir.",
                        "The gap receives a gentle, nonflashing highlight.",
                        "Eksik hat ince bir konturla belirtilir.",
                        "The gap has a clear static outline."),
                    new Beat(60, "Akış Tamamlandı", "Flow Restored",
                        "Merkez ışık etkinleşir; uzak alanın yolu görünür.",
                        "The central beacon lights up, revealing the distant region.",
                        "Bitişten sonra tek kısa ışık yayılması olur.",
                        "A single light expansion follows the finale.",
                        "Merkez doğrudan aydınlık duruma geçer.",
                        "The beacon switches directly to its completed state.")
                }),
            new Chapter(61, 80, "Yankı Alanı", "Echo Field",
                "Bir taşın hareketinin uzaktaki dünyayı nasıl uyandırdığını keşfet.",
                "Discover how one movement can wake a distant part of the world.",
                "Yankı ağı uyandı. İki uzak kıyı artık birlikte ışıldıyor.",
                "The echo network has awakened. Two distant shores now glow together.",
                "#293852", "#8C7CBD", new[]
                {
                    new Beat(61, "İlk Yankı", "First Echo",
                        "Göl üstünde tek bir sönük yankı halkası vardır.",
                        "One faint echo ring rests above the lake.",
                        "Kısa ve tek bir dalga görünür.",
                        "One brief, soft ripple appears.",
                        "Halka doğrudan görünür durumda kalır.",
                        "The ring appears immediately and remains visible."),
                    new Beat(65, "Karşılıklı Bağ", "Linked Nodes",
                        "İki uzak sinyal düğümü arasında iz oluşur.",
                        "A visible trace links two distant signal nodes.",
                        "İki düğüm bir kez sırayla aydınlanır.",
                        "Both nodes illuminate once in sequence.",
                        "İki düğüm ve bağ çizgisi sabit görünür.",
                        "Both nodes and the linking trace remain static."),
                    new Beat(70, "İki Aşamalı Sinyal", "Two-Stage Signal",
                        "Gölün iki yakasındaki sinyal çizgileri etkinleşir.",
                        "Signal routes on both shores become active.",
                        "Birinci ve ikinci sinyal ayrı kısa tepkiler verir.",
                        "First and second signals respond with separate short pulses.",
                        "İki hat ayrı ve sabit vurgularla görünür.",
                        "Both routes use distinct, static highlights."),
                    new Beat(75, "Gizli Sıra", "Hidden Order",
                        "Kapı ışıkları ancak doğru sırayla birbirine bağlanır.",
                        "Gate lights connect only in the right order.",
                        "İki kapı ışığı art arda, bir kez yanar.",
                        "Two gate lights activate once, in sequence.",
                        "İki açık kapı durumu sabit olarak gösterilir.",
                        "Both open-gate states are shown without animation."),
                    new Beat(80, "Yankının Kalbi", "Heart of Resonance",
                        "İki sinyal kanalı ve gölün merkezi birleşir.",
                        "Both signal channels converge on the lake's center.",
                        "Final kazanınca tek bir birleşme tepkisi oynatılır.",
                        "One final convergence plays after victory.",
                        "Tamamlanan alan anında aydınlanır.",
                        "The restored region lights up instantly.")
                })
        };

        public static Chapter ForLevel(int level)
        {
            foreach (var chapter in Chapters)
                if (level >= chapter.startLevel && level <= chapter.endLevel)
                    return chapter;
            return null;
        }

        public static int MilestoneIndex(Chapter chapter, int level)
        {
            if (chapter == null) return -1;
            int index = -1;
            for (int i = 0; i < chapter.beats.Length; i++)
                if (level >= chapter.beats[i].level) index = i;
            return index;
        }

        public static bool IsBeatRestored(Chapter chapter, Beat beat,
            int level, bool currentLevelWon)
        {
            if (chapter == null || beat == null || level < beat.level)
                return false;
            return beat.level != chapter.endLevel || currentLevelWon;
        }

        // A finale is completed only if the final level is explicitly marked
        // won; merely opening Level 60/80 must never award chapter completion.
        public static float PreviewProgress(int level, bool currentLevelWon)
        {
            var chapter = ForLevel(level);
            if (chapter == null) return 0f;
            int cleared = level - chapter.startLevel + (currentLevelWon ? 1 : 0);
            return Mathf.Clamp01((float)cleared / (chapter.endLevel - chapter.startLevel + 1));
        }

        [MenuItem("SHIFT/Chapter Journey/Preview 41-80")]
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
            var output = Path.Combine(folder, "chapter3_4_journey_editor_draft.json");
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
            EditorGUILayout.LabelField("41-80 / PURPOSE & PROGRESSION",
                EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "DESIGN PREVIEW ONLY: no runtime HUD, save, chapter unlock, " +
                "game assets or Perfect Shift state is modified.",
                MessageType.Info);
            int nextLevel = EditorGUILayout.IntSlider("Preview level", level, 41, 80);
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
