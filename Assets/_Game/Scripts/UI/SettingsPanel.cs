using System;
using UnityEngine;
using UnityEngine.UI;

namespace Shift.Game
{
    public sealed class SettingsPanel : MonoBehaviour
    {
        public bool IsOpen => gameObject.activeSelf;
        public void Build(Font font, Sprite rounded, SettingsService settings, GameFeelSettings feel, AudioManager audio, Action close)
        {
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
        public void Open() { transform.SetAsLastSibling(); gameObject.SetActive(true); }
        public void Close() => gameObject.SetActive(false);
    }
}
