using System;

namespace Shift.Game
{
    // Chapter progress only. Board state and puzzle rules remain in BoardManager.
    public sealed class LevelProgression
    {
        private readonly SaveService saves;
        public int LevelCount { get; }
        public int HighestUnlocked { get; private set; }
        public int Current { get; private set; }
        public int HighestCompleted { get; private set; }
        public bool IsChapterComplete(int chapter) => chapter >= 0 && HighestCompleted >= Math.Min((chapter + 1) * 20, LevelCount) - 1;
        public bool ChapterComplete => HighestCompleted == LevelCount - 1;
        public int Recommended => Math.Min(HighestCompleted + 1, HighestUnlocked);
        public LevelProgression(int levelCount, SaveService saves)
        {
            if (levelCount < 1) throw new ArgumentOutOfRangeException(nameof(levelCount));
            LevelCount = levelCount; this.saves = saves ?? throw new ArgumentNullException(nameof(saves));
            var data = saves.Load(levelCount);
            HighestUnlocked = data.HighestUnlocked; Current = data.Current; HighestCompleted = data.HighestCompleted;
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
        public void Reset() { saves.Reset(); HighestUnlocked = Current = 0; HighestCompleted = -1; }
        public void UnlockAll() { HighestUnlocked = LevelCount - 1; Persist(); }
#endif
    }
}
