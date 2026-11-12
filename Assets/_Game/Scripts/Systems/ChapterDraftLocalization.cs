using System;

namespace Shift.Game
{
    // Draft content is intentionally isolated from the published 1-40
    // localization keys. All 41-100 headers and hints remain bilingual.
    public static class ChapterDraftLocalization
    {
        private static readonly string[] TurkishTitles =
        {
            "İlk Dalga",
            "Akışı Temizle",
            "Ödünç Akıntı",
            "Akıştan Önce Ayrım",
            "Hareketten Önce Sinyal",
            "İkiz Akış",
            "Kapanan Fırsat",
            "Yankılı Geçiş",
            "İkinci Yol",
            "Vadiyi Uyandır",
            "Gecikmeli Yankı",
            "Çift Eşik",
            "Ödünç Rota",
            "Sessiz Ayrım",
            "Aynalı Zincir",
            "Gizli Aktarım",
            "Sahte Kısayol",
            "Kapalı Devre",
            "Ödünç İvme",
            "Son Eşik",
            "İlk Sinyal",
            "Sessiz Kısayol",
            "Uzak Düğme",
            "Sinyal Zinciri",
            "Uzak Zincir",
            "Sinyal Kilidi",
            "Üçlü Darbe",
            "İkiz Devre",
            "Uzak Varış",
            "İki Aşamalı Yankı",
            "Gecikmeli Aktarım",
            "Yankıyı Hazırla",
            "Çapraz Sinyaller",
            "Çapraz Öncelik",
            "Gizli Sıra",
            "Rezonansta Bekleyiş",
            "Geri Dönen Sinyal",
            "Gecikmeli Devir",
            "Son Anahtar",
            "Rezonansın Kalbi",
            "İlk Bağlantı",
            "Ters Akıntı",
            "Ayrılan Yankı",
            "Uzak Hazırlık",
            "Kesişen Yollar",
            "Bekleyen Hareket",
            "Ödünç İtiş",
            "Ortak Geçit",
            "Dönüş Yolu",
            "İki Anahtar",
            "Üç Akış",
            "Sessiz Aktarım",
            "Gizli Kesişim",
            "İkinci Şans",
            "Kenetlenen Hatlar",
            "Geri Dönüş",
            "Son Aktarım",
            "Son Geri Dönüş",
            "Birleşme Sınavı",
            "Açık Ufuk"
        };

        private static readonly string[] TurkishHints41To60 =
        {
            "İlk akış bir dönüşle başlar.",
            "Sinyali göndermeden önce yer aç.",
            "İki renk dar bir geçidi paylaşır.",
            "Doğrudan yol tek cevap değildir.",
            "Son itişten önce düğmeyi hazırla.",
            "Bir akış diğerini hazırlasın.",
            "Kapalı kapı planın bir parçası olabilir.",
            "Bir zincir diğerini hazırlamalı.",
            "İki yol, tek kritik sıra.",
            "Bilinçli bir zincirle vadiyi uyandır.",
            "Yalnızca ilk hamleyi değil, hazırladığın yolu öngör.",
            "Bir yolu açmak diğerini değiştirebilir.",
            "İki renk aynı geçidi farklı sırayla kullanabilir.",
            "Aynı hedefe birden fazla sırayla ulaşabilirsin.",
            "Bir dönüş sıradaki taşı hazırlayabilir.",
            "Bir zincir sonraki kararı hazırlar.",
            "Göze çarpan kısayol en verimlisi olmayabilir.",
            "Kapıyı açmak yetmez; zamanlama önemli.",
            "Hedef olmayan taş da zinciri harekete geçirebilir.",
            "Doğru hamle iki ayrı tepkiyi hazırlayabilir."
        };

        private static readonly string[] EnglishHints61To80 =
        {
            "One tap, two results: enter the node, trigger the distant piece, then continue.",
            "You can move both pieces separately. Can a signal save a tap?",
            "Let the signal activate the distant switch first. Then finish the route.",
            "The first signal may trigger the next. Set up three pieces in a single tap.",
            "Red is blocked along its own route. The right push signal opens the distant gate.",
            "The distant switch might be the key to the gate.",
            "Predict how three pieces can work together.",
            "Trigger the two links at different times. One piece can move another.",
            "A piece facing the wrong way can start another chain.",
            "Two signals may be useful in sequence, not at once.",
            "Use a remote signal to save a tap instead of waiting.",
            "Open the distant route first, then use the signal at the right moment.",
            "The second signal can move the key to a distant gate.",
            "Think about how a distant move can open another gate.",
            "Red's signal can open the first gate for blue.",
            "Prepare the remote piece first. The next signal completes its move.",
            "Waking one piece can clear the way for another.",
            "The signal sets up the helper. Plan the gate and shared exit in order.",
            "A signal can position the right piece for the next push.",
            "Trigger two signals in the right order; both gates matter."
        };

        private static readonly string[] EnglishHints81To100 =
        {
            "The first link takes two decisions, not a single piece.",
            "A piece facing the wrong way might start the right chain.",
            "Time the two distant signals separately.",
            "Opening one gate may depend on another piece's route.",
            "Which piece needs to reach the distant node?",
            "Sometimes leaving a piece in place is the setup.",
            "The first push may only pay off for the last piece.",
            "Clear the route before you try to use the exit.",
            "A distant move can set up the piece beside it.",
            "The two gates need a sequence, not a simultaneous trigger.",
            "Three colors share one connected current.",
            "Find the route before waking the waiting piece.",
            "A turn far away can change the decision here.",
            "Prepare the next chain before the first one ends.",
            "The second gate's key waits beyond the first.",
            "The obvious shortcut may not be the right route.",
            "Set up the last move with your first decision.",
            "Link two distant consequences with one tap.",
            "Bring two signals together for a single payoff.",
            "Find where the two distant signals meet."
        };

        public static bool TryNumber(LevelData level, out int number)
        {
            number = 0;
            if (level == null || string.IsNullOrEmpty(level.name) ||
                !level.name.StartsWith("Level", StringComparison.Ordinal))
                return false;
            int start = 5, end = start;
            while (end < level.name.Length && char.IsDigit(level.name[end])) end++;
            if (end == start || end >= level.name.Length ||
                level.name[end] != '_' ||
                !int.TryParse(level.name.Substring(start, end - start), out number))
                return false;
            return number >= 41 && number <= 100;
        }

        public static bool TryTitle(LevelData level, GameLanguage language,
            out string title)
        {
            title = null;
            if (!TryNumber(level, out int number)) return false;
            if (language == GameLanguage.Turkish)
                title = number + " - " + TurkishTitles[number - 41];
            else
                title = level.DisplayTitle;
            return !string.IsNullOrEmpty(title);
        }

        public static bool TryInstruction(LevelData level, GameLanguage language,
            out string instruction)
        {
            instruction = null;
            if (!TryNumber(level, out int number)) return false;
            if (number <= 60)
                instruction = language == GameLanguage.Turkish
                    ? TurkishHints41To60[number - 41] : level.Hint;
            else if (number <= 80)
                instruction = language == GameLanguage.Turkish
                    ? level.Hint : EnglishHints61To80[number - 61];
            else
                instruction = language == GameLanguage.Turkish
                    ? level.Hint : EnglishHints81To100[number - 81];
            return !string.IsNullOrEmpty(instruction);
        }
    }
}
