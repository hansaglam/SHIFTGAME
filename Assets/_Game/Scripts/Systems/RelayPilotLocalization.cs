using System;

namespace Shift.Game
{
    // Only the three experimental 101–103 assets.
    public static class RelayPilotLocalization
    {
        private static readonly string[] EnNames =
        {
            "The Waiting Move", "One Tap, Two Paths", "Delayed Triple"
        };
        private static readonly string[] TrNames =
        {
            "Bekleyen Hamle", "İki Yol, Tek Dokunuş", "Gecikmeli Üçlü"
        };
        private static readonly string[] EnHints =
        {
            "Red waits on the hourglass. Tap blue; red will move too.",
            "Queue red first. Yellow's tap pushes both red and blue.",
            "Queue red. Blue's next tap triggers Resonance and releases red."
        };
        private static readonly string[] TrHints =
        {
            "Kırmızı kum saatinde bekler. Maviye dokununca kırmızı da ilerler.",
            "Önce kırmızıyı beklet. Sarının hamlesi kırmızı ve maviyi iter.",
            "Kırmızıyı beklet. Mavinin hamlesi Rezonans'ı ve kırmızıyı tetikler."
        };

        private static int Index(LevelData level)
        {
            if (level == null || string.IsNullOrEmpty(level.name))
                return -1;
            string name = level.name;
            if (!name.StartsWith("Level10", StringComparison.Ordinal) ||
                name.Length < 9 || name[8] != '_') return -1;
            char digit = name[7];
            return digit >= '1' && digit <= '3' ? digit - '1' : -1;
        }

        public static bool TryTitle(LevelData level,
            GameLanguage language, out string value)
        {
            int index = Index(level);
            value = index < 0 ? null
                : (index + 101) + " - " +
                  (language == GameLanguage.Turkish
                    ? TrNames[index] : EnNames[index]);
            return index >= 0;
        }

        public static bool TryHint(LevelData level,
            GameLanguage language, out string value)
        {
            int index = Index(level);
            value = index < 0 ? null : language == GameLanguage.Turkish
                ? TrHints[index] : EnHints[index];
            return index >= 0;
        }
    }
}
