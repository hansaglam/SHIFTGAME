using System;
using UnityEngine;

namespace Shift.Game
{
    public sealed class SaveService
    {
        private readonly string prefix;
        public SaveService(string prefix = "SHIFT.Progress.v1.") { this.prefix = prefix; }
        public ProgressData Load(int levelCount)
        {
            int last = Math.Max(0, levelCount - 1);
            int highest = Mathf.Clamp(PlayerPrefs.GetInt(prefix + "Unlocked", 0), 0, last);
            int current = Mathf.Clamp(PlayerPrefs.GetInt(prefix + "Current", highest), 0, highest);
            int completed = Mathf.Clamp(PlayerPrefs.GetInt(prefix + "Completed", -1), -1, highest);
            // Old Chapter 1 finale saved 19/19/19. Extend its frontier without
            // resetting earlier replay selections or changing the existing key namespace.
            if (levelCount > 20 && highest == 19 && completed == 19)
            {
                highest = 20; if (current == 19) current = 20;
                var migrated = new ProgressData(highest, current, completed); Save(migrated); return migrated;
            }
            return new ProgressData(highest, current, completed);
        }
        public void Save(ProgressData data)
        {
            PlayerPrefs.SetInt(prefix + "Unlocked", data.HighestUnlocked);
            PlayerPrefs.SetInt(prefix + "Current", data.Current);
            PlayerPrefs.SetInt(prefix + "Completed", data.HighestCompleted);
            PlayerPrefs.Save();
        }
        // Additive versioned metadata: legacy progress keys and migration stay intact.
        public bool[] LoadPerfects(int levelCount)
        {
            var result = new bool[levelCount];
            string stored = PlayerPrefs.GetString(prefix + "Mastery.v1.Perfect", "");
            for (int i = 0; i < Math.Min(levelCount, stored.Length); i++) result[i] = stored[i] == '1';
            return result;
        }
        public void RecordPerfect(int index, int levelCount)
        {
            if (index < 0 || index >= levelCount) return;
            string prior = PlayerPrefs.GetString(prefix + "Mastery.v1.Perfect", "");
            var flags = new char[Math.Max(levelCount, Math.Min(prior.Length, 1024))];
            for (int i = 0; i < flags.Length; i++) flags[i] = i < prior.Length && prior[i] == '1' ? '1' : '0';
            flags[index] = '1'; PlayerPrefs.SetString(prefix + "Mastery.v1.Perfect", new string(flags));
            PlayerPrefs.Save();
        }
#if UNITY_EDITOR
        public void Reset()
        {
            PlayerPrefs.DeleteKey(prefix + "Unlocked"); PlayerPrefs.DeleteKey(prefix + "Current");
            PlayerPrefs.DeleteKey(prefix + "Completed"); PlayerPrefs.DeleteKey(prefix + "Mastery.v1.Perfect"); PlayerPrefs.Save();
        }
#endif
    }

    public readonly struct ProgressData
    {
        public int HighestUnlocked { get; }
        public int Current { get; }
        public int HighestCompleted { get; }
        public ProgressData(int unlocked, int current, int completed)
        { HighestUnlocked = unlocked; Current = current; HighestCompleted = completed; }
    }
}
