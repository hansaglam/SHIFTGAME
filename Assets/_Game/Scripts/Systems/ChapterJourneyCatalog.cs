using System;
using UnityEngine;

namespace Shift.Game
{
    // Single source of truth for Chapter Journey copy and visual states.
    // Read-only projection of LevelProgression; no PlayerPrefs, unlocks, or new rewards.
    public static class ChapterJourneyCatalog
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


        public static Chapter ForLevel(int number)
        {
            foreach (var chapter in Chapters)
                if (number >= chapter.startLevel && number <= chapter.endLevel)
                    return chapter;
            return null;
        }

        public static Chapter ForChapter(int chapterIndex)
        {
            foreach (var chapter in Chapters)
                if (CampaignChapters.Index(chapter.startLevel - 1) == chapterIndex)
                    return chapter;
            return null;
        }

        public static bool Available(LevelProgression progress, int chapterIndex)
        {
            var chapter = ForChapter(chapterIndex);
            return progress != null && chapter != null &&
                progress.LevelCount >= chapter.endLevel &&
                progress.IsUnlocked(chapter.startLevel - 1);
        }

        public static int Completed(Chapter chapter, int highestCompletedIndex)
        {
            if (chapter == null) return 0;
            return Mathf.Clamp(highestCompletedIndex - chapter.startLevel + 2,
                0, chapter.endLevel - chapter.startLevel + 1);
        }

        public static float Progress01(Chapter chapter, int highestCompletedIndex)
        {
            if (chapter == null) return 0f;
            return (float)Completed(chapter, highestCompletedIndex) /
                (chapter.endLevel - chapter.startLevel + 1);
        }

        public static bool Restored(Beat beat, int highestCompletedIndex)
        {
            return beat != null && highestCompletedIndex >= beat.level - 1;
        }

        public static int LastRestoredBeat(Chapter chapter, int highestCompletedIndex)
        {
            if (chapter == null) return -1;
            int result = -1;
            for (int i = 0; i < chapter.beats.Length; i++)
                if (Restored(chapter.beats[i], highestCompletedIndex))
                    result = i;
            return result;
        }
    }
}
