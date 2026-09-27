using System;
using System.Globalization;
using UnityEngine;
namespace Shift.Game
{
    public enum GameLanguage { English, Turkish }
    // The only mutable language state. Compatibility enums are projections of this service.
    public sealed class GameLanguageService
    {
        public const string SaveKey = "SHIFT.Settings.Language.v1";
        private static GameLanguageService shared;
        public static GameLanguageService Shared => shared ??= new GameLanguageService(Application.systemLanguage);
        private readonly string key;
        public GameLanguage CurrentLanguage { get; private set; }
        public HintLanguage HintLanguage => CurrentLanguage == GameLanguage.Turkish ? HintLanguage.Turkish : HintLanguage.English;
        public CultureInfo Culture => CultureInfo.GetCultureInfo(CurrentLanguage == GameLanguage.Turkish ? "tr-TR" : "en-US");
        public event Action LanguageChanged;
        public GameLanguageService(SystemLanguage device, string key = SaveKey)
        {
            this.key = key;
            string saved = PlayerPrefs.GetString(key, "");
            CurrentLanguage = saved == "English" ? GameLanguage.English : saved == "Turkish" ? GameLanguage.Turkish : Resolve(device);
            // Resolve only on first launch (or a malformed save), then retain the resolved choice.
            if (saved != "English" && saved != "Turkish")
            { PlayerPrefs.SetString(key, CurrentLanguage.ToString()); PlayerPrefs.Save(); }
        }
        public static GameLanguage Resolve(SystemLanguage device) => device == SystemLanguage.Turkish ? GameLanguage.Turkish : GameLanguage.English;
        public void Select(GameLanguage language)
        {
            if (!Enum.IsDefined(typeof(GameLanguage), language)) throw new ArgumentOutOfRangeException(nameof(language));
            PlayerPrefs.SetString(key, language.ToString()); PlayerPrefs.Save();
            if (CurrentLanguage == language) return;
            CurrentLanguage = language; LanguageChanged?.Invoke();
        }
        public string Text(string key, params object[] args) => LocalizationCatalog.Format(key, CurrentLanguage, args);
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetRuntime() { shared = null; }
#if UNITY_EDITOR
        public static void UseForValidation(GameLanguageService service) { shared = service; }
        public int ListenerCount => LanguageChanged?.GetInvocationList().Length ?? 0;
#endif
    }
}
