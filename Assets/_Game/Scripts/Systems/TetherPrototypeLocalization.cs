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
            "The two gold 1 badges mean BONDED. Tap red: as it moves right, it pulls blue onto the switch.",
            "The bond cannot pull through a wall. Tap blue first to bring red toward the up-arrow tile.",
            "Tap red. Its gold bond pulls blue onto the cyan node, which then moves yellow remotely.",
            "Open the first gate so blue can be pulled. Then use blue to open the second gate."
        };
        private static readonly string[] TurkishHints =
        {
            "Altın 1 işaretli kırmızı ve mavi BAĞ’lı. Kırmızıya dokun: sağa giderken maviyi düğmeye çeker.",
            "BAĞ duvardan çekemez. Önce maviye dokun; kırmızı yukarı okuna çekilsin.",
            "Kırmızıya dokun: BAĞ maviyi turkuaz Rezonans’a çeker; Rezonans sarıyı uzaktan hareket ettirir.",
            "Önce ilk kapıyı aç ki mavi çekilebilsin. Sonra maviyle ikinci kapıyı aç."
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
