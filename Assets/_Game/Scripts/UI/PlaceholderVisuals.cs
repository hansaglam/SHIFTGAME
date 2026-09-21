using UnityEngine;
using UnityEngine.UI;

namespace Shift.Game
{
    public static class PlaceholderVisuals
    {
        public static Color ColorFor(PieceColor color) => color switch
        {
            PieceColor.Red => new Color32(245, 90, 107, 255),
            PieceColor.Blue => new Color32(78, 166, 250, 255),
            PieceColor.Yellow => new Color32(255, 208, 83, 255),
            PieceColor.Green => new Color32(89, 218, 166, 255),
            _ => new Color32(153, 167, 189, 255)
        };

        public static string Arrow(Direction direction) => direction switch
        {
            Direction.Up => "↑", Direction.Down => "↓", Direction.Left => "←", Direction.Right => "→", _ => ""
        };

        public static RectTransform Rect(string name, Transform parent, Vector2 min, Vector2 max)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false); rect.anchorMin = min; rect.anchorMax = max;
            rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
            return rect;
        }

        public static Text Label(string name, Transform parent, Font font, string value, int size, Color color, Vector2 min, Vector2 max)
        {
            var text = Rect(name, parent, min, max).gameObject.AddComponent<Text>();
            text.font = font; text.text = value; text.fontSize = size; text.color = color;
            text.alignment = TextAnchor.MiddleCenter; text.raycastTarget = false;
            text.resizeTextForBestFit = true; text.resizeTextMinSize = 12; text.resizeTextMaxSize = size;
            return text;
        }

        public static Sprite CreateCircle()
        {
            const int size = 64;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { name = "SHIFT placeholder circle", filterMode = FilterMode.Bilinear };
            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x + .5f, y + .5f), new Vector2(size * .5f, size * .5f));
                    pixels[y * size + x] = new Color(1, 1, 1, Mathf.Clamp01(size * .5f - distance));
                }
            texture.SetPixels32(pixels); texture.Apply(false, true);
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(.5f, .5f), size);
        }

        public static Sprite CreateRounded()
        {
            const int size = 64;
            const float radius = 14;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { name = "SHIFT rounded tile", filterMode = FilterMode.Bilinear };
            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    var q = new Vector2(Mathf.Abs(x + .5f - size / 2f), Mathf.Abs(y + .5f - size / 2f)) - Vector2.one * (size / 2f - radius);
                    float distance = new Vector2(Mathf.Max(q.x, 0), Mathf.Max(q.y, 0)).magnitude + Mathf.Min(Mathf.Max(q.x, q.y), 0) - radius;
                    pixels[y * size + x] = new Color(1, 1, 1, Mathf.Clamp01(.5f - distance));
                }
            texture.SetPixels32(pixels); texture.Apply(false, true);
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(.5f, .5f), 100, 0,
                SpriteMeshType.FullRect, Vector4.one * radius);
        }
    }
}
