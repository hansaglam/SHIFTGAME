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
            PlaceholderVisuals.Label("Settings Title", transform, font, "SETTINGS", 64, VisualTheme.Ink, new Vector2(.1f,.78f), new Vector2(.9f,.9f));
            Add("Sound", .59f, () => settings.Sound, () => settings.Save(!settings.Sound, settings.Haptics, settings.ReducedMotion));
            Add("Haptics", .45f, () => settings.Haptics, () => settings.Save(settings.Sound, !settings.Haptics, settings.ReducedMotion));
            Add("Reduced Motion", .31f, () => settings.ReducedMotion, () => settings.Save(settings.Sound, settings.Haptics, !settings.ReducedMotion));
            ChapterSelect.CreateButton("Close Settings", font, rounded, transform, new Vector2(.25f,.12f), new Vector2(.75f,.21f), close, out var back);
            back.text = "BACK";
            foreach (var motion in GetComponentsInChildren<PresentationMotion>()) motion.Settings = feel;
            gameObject.SetActive(false);
            void Add(string name, float y, Func<bool> read, Action toggle)
            {
                Text label = null;
                ChapterSelect.CreateButton(name + " Setting", font, rounded, transform, new Vector2(.12f,y), new Vector2(.88f,y+.1f), () =>
                { toggle(); settings.Apply(feel,audio); label.text = name.ToUpperInvariant() + ": " + (read() ? "ON" : "OFF"); }, out label);
                label.text = name.ToUpperInvariant() + ": " + (read() ? "ON" : "OFF");
            }
        }
        public void Open() { transform.SetAsLastSibling(); gameObject.SetActive(true); }
        public void Close() => gameObject.SetActive(false);
    }
}
