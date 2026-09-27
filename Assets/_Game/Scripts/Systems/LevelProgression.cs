using System;

namespace Shift.Game
{
    // Chapter progress only. Board state and puzzle rules remain in BoardManager.
    public sealed class LevelProgression
    {
        private readonly SaveService saves;
        private readonly bool[] perfects;
        private readonly LevelData[] masteryLevels;
        private readonly bool[] eligible;
        public int LevelCount { get; }
        public int HighestUnlocked { get; private set; }
        public int Current { get; private set; }
        public int HighestCompleted { get; private set; }
        public bool IsChapterComplete(int chapter) => CampaignChapters.Valid(chapter, LevelCount) && HighestCompleted >= CampaignChapters.End(chapter, LevelCount) - 1;
        public bool ChapterComplete => HighestCompleted == LevelCount - 1;
        public int Recommended => Math.Min(HighestCompleted + 1, HighestUnlocked);
        public LevelProgression(int levelCount, SaveService saves, LevelData[] levels = null)
        {
            if (levelCount < 1) throw new ArgumentOutOfRangeException(nameof(levelCount));
            LevelCount = levelCount; this.saves = saves ?? throw new ArgumentNullException(nameof(saves));
            var data = saves.Load(levelCount);
            HighestUnlocked = data.HighestUnlocked; Current = data.Current; HighestCompleted = data.HighestCompleted;
            perfects = saves.LoadPerfects(levelCount); eligible = new bool[levelCount];
            masteryLevels = levels == null ? null : (LevelData[])levels.Clone();
            for (int i = 0; i < levelCount; i++)
                eligible[i] = masteryLevels != null && i < masteryLevels.Length && masteryLevels[i] != null && masteryLevels[i].VerifiedOptimalMoveCount.HasValue;
        }
        public bool IsPerfectEligible(int index) => index >= 0 && index < LevelCount && eligible[index];
        public bool IsPerfect(int index) => IsPerfectEligible(index) && index <= HighestCompleted && perfects[index];
        public LevelMasteryState Mastery(int index) => !IsUnlocked(index) ? LevelMasteryState.Locked
            : IsPerfect(index) ? LevelMasteryState.Perfect : index <= HighestCompleted ? LevelMasteryState.Completed : LevelMasteryState.Available;
        public ChapterMastery ChapterSummary(int chapter)
        {
            if (!CampaignChapters.Valid(chapter, LevelCount)) return default;
            int complete = 0, perfect = 0, available = 0;
            for (int i = CampaignChapters.Start(chapter); i < CampaignChapters.End(chapter, LevelCount); i++)
            { if (i <= HighestCompleted) complete++; if (IsPerfect(i)) perfect++; if (IsPerfectEligible(i)) available++; }
            return new ChapterMastery(chapter, CampaignChapters.End(chapter, LevelCount) - CampaignChapters.Start(chapter), complete, perfect, available);
        }
        public bool RecordPerfect(int index, AttemptMetrics result)
        {
            if (index != Current || !IsUnlocked(index) || index > HighestCompleted || !IsPerfectEligible(index) || IsPerfect(index) || result == null) return false;
            var level = masteryLevels[index];
            // Existing telemetry is the truth source; never infer optimality from a known route.
            if (!result.completed || !result.hasOptimal || !result.perfectShift || result.levelIndex != index || result.levelId != level.name
                || result.verifiedOptimalMoves != level.VerifiedOptimalMoveCount) return false;
            perfects[index] = true; saves.RecordPerfect(index, LevelCount); return true;
        }
        public bool IsUnlocked(int index) => index >= 0 && index < LevelCount && index <= HighestUnlocked;
        public bool Select(int index)
        {
            if (!IsUnlocked(index)) return false;
            Current = index; Persist(); return true;
        }
        public bool Complete(int index)
        {
            if (index != Current || !IsUnlocked(index)) return false;
            HighestCompleted = Math.Max(HighestCompleted, index);
            HighestUnlocked = Math.Max(HighestUnlocked, Math.Min(index + 1, LevelCount - 1));
            // On relaunch recommend the newly unlocked level, even if Next has not been pressed.
            saves.Save(new ProgressData(HighestUnlocked, Math.Min(index + 1, HighestUnlocked), HighestCompleted));
            return true;
        }
        public bool Next()
        {
            if (Current > HighestCompleted || Current + 1 >= LevelCount) return false;
            return Select(Current + 1);
        }
        private void Persist() => saves.Save(new ProgressData(HighestUnlocked, Current, HighestCompleted));
#if UNITY_EDITOR
        public void Reset() { saves.Reset(); HighestUnlocked = Current = 0; HighestCompleted = -1; Array.Clear(perfects, 0, perfects.Length); }
        public void UnlockAll() { HighestUnlocked = LevelCount - 1; Persist(); }
#endif
    }
}
