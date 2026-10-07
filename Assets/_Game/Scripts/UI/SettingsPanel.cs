using System;
using UnityEngine;
using UnityEngine.UI;

namespace Shift.Game
{
    public sealed class SettingsPanel : MonoBehaviour
    {
        private Func<IPrivacyChoices> privacyProvider;
        private Button privacy;
        private RectTransform[] rows;
        private bool requesting, layoutReady;
        private Action<string> openExternalUrl;
        public const string PrivacyPolicyUrl = "https://hansaglam.github.io/shift-legal/privacy-policy/";
        public bool IsOpen => gameObject.activeSelf;
        public void Build(Font font, Sprite rounded, SettingsService settings, GameFeelSettings feel, AudioManager audio, Action close, Func<IPrivacyChoices> privacyProvider = null, Action<string> openUrl = null)
        {
            this.privacyProvider = privacyProvider;
            openExternalUrl = openUrl ?? Application.OpenURL;
            gameObject.AddComponent<Image>().color = new Color(.055f,.13f,.23f,.88f);
            IdentityStyle.Modal(transform, rounded, "Settings Card");
            var title = PlaceholderVisuals.Label("Settings Title", transform, font, "SETTINGS", 64, VisualTheme.Ink, new Vector2(.1f,.78f), new Vector2(.9f,.9f));
            LocalizedLabel.Bind(title, "settings.title");
            Add("Sound", "sound", .63f, () => settings.Sound, () => settings.Save(!settings.Sound, settings.Haptics, settings.ReducedMotion));
            Add("Haptics", "haptics", .50f, () => settings.Haptics, () => settings.Save(settings.Sound, !settings.Haptics, settings.ReducedMotion));
            Add("Reduced Motion", "reduced", .37f, () => settings.ReducedMotion, () => settings.Save(settings.Sound, settings.Haptics, !settings.ReducedMotion));
            ChapterSelect.CreateButton("Close Settings", font, rounded, transform, new Vector2(.25f,.12f), new Vector2(.75f,.21f), close, out var back);
            LocalizedLabel.Bind(back, "common.back");
            ChapterSelect.CreateButton("Language Setting", font, rounded, transform, new Vector2(.12f,.24f), new Vector2(.88f,.34f), () =>
                GameLanguageService.Shared.Select(GameLanguageService.Shared.CurrentLanguage == GameLanguage.English ? GameLanguage.Turkish : GameLanguage.English), out var language);
            LocalizedLabel.Bind(language, "settings.language");
            ChapterSelect.CreateButton("Privacy Policy", font, rounded, transform,
                new Vector2(.12f,.23f), new Vector2(.88f,.315f), () => openExternalUrl(PrivacyPolicyUrl), out var policyLabel);
            LocalizedLabel.Bind(policyLabel, "settings.privacy_policy");
            privacy = ChapterSelect.CreateButton("Privacy Choices", font, rounded, transform,
                new Vector2(.12f,.23f), new Vector2(.88f,.315f), OpenPrivacyOptions, out var privacyLabel);
            LocalizedLabel.Bind(privacyLabel, "settings.privacy_choices");
            rows = new[] { (RectTransform)transform.Find("Sound Setting"), (RectTransform)transform.Find("Haptics Setting"),
                (RectTransform)transform.Find("Reduced Motion Setting"), (RectTransform)transform.Find("Language Setting"), (RectTransform)transform.Find("Privacy Policy") };
            privacy.gameObject.SetActive(false);
            RefreshPrivacyOptions();
            foreach (var motion in GetComponentsInChildren<PresentationMotion>()) motion.Settings = feel;
            gameObject.SetActive(false);
            void Add(string name, string key, float y, Func<bool> read, Action toggle)
            {
                Text label = null;
                ChapterSelect.CreateButton(name + " Setting", font, rounded, transform, new Vector2(.12f,y), new Vector2(.88f,y+.1f), () =>
                { toggle(); settings.Apply(feel,audio); LocalizedLabel.Bind(label, () => GameLanguageService.Shared.Text("settings." + key + (read() ? "_on" : "_off"))); }, out label);
                LocalizedLabel.Bind(label, () => GameLanguageService.Shared.Text("settings." + key + (read() ? "_on" : "_off")));
            }
        }
        private void Update() => RefreshPrivacyOptions();
        public void RefreshPrivacyOptions()
        {
            if (privacy == null) return;
            bool required = false, busy = false;
            try
            {
                var provider = privacyProvider?.Invoke();
                required = provider != null && provider.IsPrivacyOptionsRequired;
                busy = provider != null && provider.IsBusy;
            }
            catch (Exception) { required = false; }
            if (!layoutReady || privacy.gameObject.activeSelf != required)
            {
                layoutReady = true;
                privacy.gameObject.SetActive(required);
                var privacyRect = (RectTransform)privacy.transform;
                privacyRect.anchorMin = new Vector2(.12f, .22f);
                privacyRect.anchorMax = new Vector2(.88f, .295f);
                for (int i = 0; i < rows.Length; i++)
                {
                    float y = required ? .68f - i * .092f : .65f - i * .105f;
                    rows[i].anchorMin = new Vector2(.12f, y);
                    rows[i].anchorMax = new Vector2(.88f, y + (required ? .075f : .085f));
                }
            }
            privacy.interactable = required && !busy && !requesting;
        }
        private void OpenPrivacyOptions()
        {
            RefreshPrivacyOptions();
            if (!privacy.gameObject.activeSelf || !privacy.interactable) return;
            requesting = true; privacy.interactable = false;
            try
            {
                var provider = privacyProvider?.Invoke();
                if (provider == null) { requesting = false; RefreshPrivacyOptions(); return; }
                provider.ShowPrivacyOptions(_ =>
                {
                    requesting = false;
                    // The existing consent gate refreshes live UMP permission and invalidates ads.
                    // Read the current SDK-backed requirement; never set consent or request a reward here.
                    if (this != null) RefreshPrivacyOptions();
                });
            }
            catch (Exception) { requesting = false; RefreshPrivacyOptions(); }
        }
        public void Open() { transform.SetAsLastSibling(); gameObject.SetActive(true); RefreshPrivacyOptions(); }
        public void Close() => gameObject.SetActive(false);
    }
}
