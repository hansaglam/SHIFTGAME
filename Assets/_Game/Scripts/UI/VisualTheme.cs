using UnityEngine;
using UnityEngine.UI;

namespace Shift.Game
{
    // One palette and reusable sliced surfaces; no per-frame texture generation.
    public static class VisualTheme
    {
        public static readonly Color Ink = new Color32(29, 53, 69, 255);
        public static readonly Color Muted = new Color32(83, 111, 126, 255);
        public static readonly Color Accent = new Color32(26, 133, 116, 255);
        public static Image Surface(string name, Transform parent, Sprite sprite, Color color, Vector2 min, Vector2 max)
        {
            var image = PlaceholderVisuals.Rect(name, parent, min, max).gameObject.AddComponent<Image>();
            image.sprite = sprite; image.type = Image.Type.Sliced; image.color = color; image.raycastTarget = false;
            return image;
        }
        public static void Shadow(Graphic graphic, float distance = 5)
        {
            var shadow = graphic.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(.04f, .12f, .16f, .18f); shadow.effectDistance = new Vector2(0, -distance);
        }
        public static void Background(Transform parent, Sprite circle)
        {
            var sky = PlaceholderVisuals.Rect("Atmosphere", parent, Vector2.zero, Vector2.one).gameObject.AddComponent<Atmosphere>();
            sky.raycastTarget = false;
            var art = Resources.Load<Texture2D>("Identity/AlpineLake");
            if (art != null)
                PlaceholderVisuals.Rect("Alpine Lake", sky.transform, Vector2.zero, Vector2.one)
                    .gameObject.AddComponent<ScenicBackdrop>().Initialize(art);
        }
    }
}
