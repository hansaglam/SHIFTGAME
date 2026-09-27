using System;
using UnityEngine;
using UnityEngine.UI;
namespace Shift.Game
{
    // Retains a semantic text recipe, not a reverse-translation of rendered text.
    // Refreshing never invokes gameplay, telemetry, audio or presentation animations.
    public sealed class LocalizedLabel : MonoBehaviour
    {
        private Func<string> render;
        private GameLanguageService subscribed;
        public static void Bind(Text label, string key, params object[] args) => Bind(label, () => GameLanguageService.Shared.Text(key, args));
        public static void Bind(Text label, Func<string> render)
        {
            var binding = label.GetComponent<LocalizedLabel>() ?? label.gameObject.AddComponent<LocalizedLabel>();
            binding.render = render; binding.Refresh();
        }
        public static Func<string> Recipe(Text label)
        {
            var binding = label.GetComponent<LocalizedLabel>();
            string text = label.text;
            return binding != null && binding.render != null ? binding.render : () => text;
        }
        private void OnEnable() { subscribed = GameLanguageService.Shared; subscribed.LanguageChanged += Refresh; Refresh(); }
        private void OnDisable() { if (subscribed != null) subscribed.LanguageChanged -= Refresh; subscribed = null; }
        private void Refresh() { if (render != null) GetComponent<Text>().text = render(); }
    }
}
