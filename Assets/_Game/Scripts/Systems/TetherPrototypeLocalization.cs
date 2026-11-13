using System;

namespace Shift.Game
{
    // Pilot-only 101-104 copy. No edits to published 1-100 keys or levels.
    public static class TetherPrototypeLocalization
    {
        private static readonly string[] EnglishTitles =
        {
            "The First Bond", "Blocked Tension",
            "Triple Reaction", "Two Keys, One Bond"
        };
        private static readonly string[] TurkishTitles =
        {
            "İlk Bağ", "Gerilen Yol",
            "Üçlü Tepki", "İki Anahtar, Tek Bağ"
        };
        private static readonly string[] EnglishHints =
        {
            "Gold 1 stones are BONDED. Tap red to pull blue onto the switch.",
            "A wall blocks the bond. Tap blue first to pull red upward.",
            "Tap red: the bond moves blue; Resonance then moves yellow.",
            "Open gate one to pull blue. Then use blue to open gate two."
        };
        private static readonly string[] TurkishHints =
        {
            "Altın 1'ler BAĞ'lı. Kırmızıya dokun: mavi düğmeye çekilir.",
            "BAĞ duvardan çekemez. Önce maviye dokun; kırmızıyı yukarı çek.",
            "Kırmızıya dokun: BAĞ maviyi, Rezonans ise sarıyı hareket ettirir.",
            "İlk kapıyı aç ki mavi çekilsin. Sonra maviyle ikinci kapıyı aç."
        };

        public static bool TryNumber(LevelData level, out int number)
        {
            number = 0;
            if (level == null || string.IsNullOrEmpty(level.name) ||
                !level.name.StartsWith("Level", StringComparison.Ordinal))
                return false;
            int i = 5;
            while (i < level.name.Length && char.IsDigit(level.name[i]))
                i++;
            if (i >= level.name.Length || level.name[i] != '_' ||
                !int.TryParse(level.name.Substring(5, i - 5), out number))
                return false;
            return number >= 101 && number <= 104;
        }

        public static bool TryTitle(LevelData level, GameLanguage language,
            out string title)
        {
            title = null;
            if (!TryNumber(level, out int number)) return false;
            int index = number - 101;
            title = number + " - " + (language == GameLanguage.Turkish
                ? TurkishTitles[index] : EnglishTitles[index]);
            return true;
        }

        public static bool TryInstruction(LevelData level,
            GameLanguage language, out string hint)
        {
            hint = null;
            if (!TryNumber(level, out int number)) return false;
            hint = language == GameLanguage.Turkish
                ? TurkishHints[number - 101] : EnglishHints[number - 101];
            return true;
        }
    }
}
