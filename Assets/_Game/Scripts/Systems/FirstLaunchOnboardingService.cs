using System;
using UnityEngine;

namespace Shift.Game
{
    // Owns only onboarding persistence/navigation. Never reads or writes game saves.
    public sealed class FirstLaunchOnboardingService
    {
        public const string SaveKey = "SHIFT.Onboarding.v1";
        public const int PageCount = 4;
        private readonly string key;
        private readonly Action<string, int> track;
        private readonly bool[] viewed = new bool[PageCount];
        private bool started;
        public bool Completed { get; private set; }
        public int PageIndex { get; private set; }
        public FirstLaunchOnboardingService(string key = SaveKey, Action<string, int> track = null)
        {
            this.key = key; this.track = track;
            Completed = PlayerPrefs.GetString(key, "") == "Completed";
        }
        public bool Begin()
        {
            if (Completed || started) return false;
            started = true; Emit("onboarding_started"); View(); return true;
        }
        public bool Next()
        {
            if (!started || Completed || PageIndex >= PageCount - 1) return false;
            PageIndex++; View(); return true;
        }
        public bool Back()
        {
            if (!started || Completed || PageIndex == 0) return false;
            PageIndex--; View(); return true;
        }
        public bool Finish(bool skip, Action continuePlaying)
        {
            if (!started || Completed || (skip ? PageIndex != 0 : PageIndex != PageCount - 1)) return false;
            Completed = true;
            PlayerPrefs.SetString(key, "Completed"); PlayerPrefs.Save();
            Emit(skip ? "onboarding_skipped" : "onboarding_completed");
            continuePlaying?.Invoke(); return true;
        }
        private void View() { if (viewed[PageIndex]) return; viewed[PageIndex] = true; Emit("onboarding_page_viewed"); }
        private void Emit(string name) { track?.Invoke(name, PageIndex); }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        // Recreate the scene/service after resetting. Never exposed in release Settings.
        public static void ResetForTesting(string key = SaveKey) { PlayerPrefs.DeleteKey(key); PlayerPrefs.Save(); }
#endif
    }
}
