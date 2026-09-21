using UnityEngine;

namespace Shift.Game
{
    public sealed class SettingsService
    {
        private readonly string prefix;
        public bool Sound { get; private set; }
        public bool Haptics { get; private set; }
        public bool ReducedMotion { get; private set; }
        public SettingsService(string prefix = "SHIFT.Settings.v1.", bool defaultMotion = false, bool defaultHaptics = false)
        {
            this.prefix = prefix;
            Sound = PlayerPrefs.GetInt(prefix + "Sound", 1) == 1;
            Haptics = PlayerPrefs.GetInt(prefix + "Haptics", defaultHaptics ? 1 : 0) == 1;
            ReducedMotion = PlayerPrefs.GetInt(prefix + "ReducedMotion", defaultMotion ? 1 : 0) == 1;
        }
        public void Save(bool sound, bool haptics, bool reducedMotion)
        {
            Sound = sound; Haptics = haptics; ReducedMotion = reducedMotion;
            PlayerPrefs.SetInt(prefix + "Sound", sound ? 1 : 0);
            PlayerPrefs.SetInt(prefix + "Haptics", haptics ? 1 : 0);
            PlayerPrefs.SetInt(prefix + "ReducedMotion", reducedMotion ? 1 : 0); PlayerPrefs.Save();
        }
        public void Apply(GameFeelSettings feel, AudioManager audio)
        { feel.hapticsEnabled = Haptics; feel.reducedMotion = ReducedMotion; audio.Muted = !Sound; }
#if UNITY_EDITOR
        public void Reset()
        { PlayerPrefs.DeleteKey(prefix + "Sound"); PlayerPrefs.DeleteKey(prefix + "Haptics"); PlayerPrefs.DeleteKey(prefix + "ReducedMotion"); PlayerPrefs.Save(); }
#endif
    }
}
