namespace Shift.Game
{
    // Derived only: replay selection and Daily history never replace the campaign frontier.
    public static class CampaignContinuation
    {
        public static int LevelIndex(LevelProgression progress) => progress == null || progress.ChapterComplete ? -1 : progress.Recommended;
        public static int Chapter(LevelProgression progress)
        {
            if (progress == null) return 0;
            if (!progress.ChapterComplete) return CampaignChapters.Index(progress.Recommended);
            for (int i = 0; CampaignChapters.Valid(i, progress.LevelCount); i++)
                if (!progress.ChapterSummary(i).Mastered) return i;
            return 0;
        }
    }
}
