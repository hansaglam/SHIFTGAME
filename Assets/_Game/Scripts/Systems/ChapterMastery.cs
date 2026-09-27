using System;

namespace Shift.Game
{
    public static class CampaignChapters
    {
        public const int LevelsPerChapter = 20;
        public static int Count(int levels) => (levels + LevelsPerChapter - 1) / LevelsPerChapter;
        public static int Index(int level) => level / LevelsPerChapter;
        public static int Start(int chapter) => chapter * LevelsPerChapter;
        public static int End(int chapter, int levels) => Math.Min(Start(chapter + 1), levels);
        public static bool Valid(int chapter, int levels) => chapter >= 0 && chapter < Count(levels);
        public static bool IsLast(int level, int levels) => level >= 0 && level < levels && level == End(Index(level), levels) - 1;
    }
    public enum LevelMasteryState { Locked, Available, Completed, Perfect }
    public readonly struct ChapterMastery
    {
        public int Chapter { get; }
        public int Total { get; }
        public int Completed { get; }
        public int Perfect { get; }
        public int Eligible { get; }
        public int RemainingPerfect => Math.Max(0, Eligible - Perfect);
        // Product rule: finish every puzzle and perfect every certified puzzle.
        public bool Mastered => Total > 0 && Completed == Total && Eligible > 0 && Perfect == Eligible;
        public ChapterMastery(int chapter, int total, int completed, int perfect, int eligible)
        { Chapter = chapter; Total = total; Completed = completed; Perfect = perfect; Eligible = eligible; }
    }
    public static class MasteryText
    {
        public static string FirstPerfect(HintLanguage language) => LocalizationCatalog.ForHint("mastery.first", language);
        public static string Mastered(HintLanguage language) => LocalizationCatalog.ForHint("mastery.mastered", language);
        public static string Summary(ChapterMastery value, HintLanguage language) => LocalizationCatalog.ForHint("mastery.summary", language, value.Completed, value.Total, value.Perfect);
        public static string Remaining(ChapterMastery value, HintLanguage language) => value.Mastered ? Mastered(language) : value.RemainingPerfect == 0 ? "" : LocalizationCatalog.ForHint("mastery.remaining", language, value.RemainingPerfect);
        public static string Legend(HintLanguage language) => LocalizationCatalog.ForHint("mastery.legend", language);
    }
}
