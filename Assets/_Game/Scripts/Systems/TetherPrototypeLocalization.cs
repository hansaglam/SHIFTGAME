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
            "A red move can pull blue onto a distant switch.",
            "If the pull hits a wall, move the partner to change the route.",
            "A bond can activate Resonance and move a third stone in one tap.",
            "The first gate controls the pull; the second controls the exit."
        };
        private static readonly string[] TurkishHints =
        {
            "Kırmızıyı ilerletirsen mavi eşini çekip kapıyı açabilir.",
            "Çekiş duvarda duruyorsa önce bağlı eşin yönünü düşün.",
            "Bir çekiş, uzaktaki üçüncü taşı da harekete geçirebilir.",
            "İlk kapı çekiş yolunu, ikinci kapı son geçişi kontrol eder."
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
