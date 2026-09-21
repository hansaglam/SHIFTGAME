using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Shift.Game
{
    // One small neutral sprite per shape, owned by the UI canvas. Tints remain live.
    [ExecuteAlways]
    public sealed class IdentityResources : MonoBehaviour
    {
        private readonly Dictionary<bool, Sprite> surfaces = new Dictionary<bool, Sprite>();
        public static Sprite Surface(Transform owner, bool round = false)
        {
            var canvas = owner.GetComponentInParent<Canvas>();
            var root = canvas != null ? canvas.gameObject : owner.root.gameObject;
            var resources = root.GetComponent<IdentityResources>() ?? root.AddComponent<IdentityResources>();
            return resources.Get(round);
        }
        private Sprite Get(bool round)
        {
            if (surfaces.TryGetValue(round, out var cached)) return cached;
            const int size = 192;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            { name = round ? "SHIFT porcelain disc" : "SHIFT satin panel", filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.DontSave };
            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++) for (int x = 0; x < size; x++)
            {
                var p = new Vector2(x + .5f - size / 2f, y + .5f - size / 2f);
                var q = new Vector2(Mathf.Abs(p.x), Mathf.Abs(p.y)) - Vector2.one * 53;
                float distance = round ? p.magnitude - 94 : new Vector2(Mathf.Max(q.x, 0), Mathf.Max(q.y, 0)).magnitude + Mathf.Min(Mathf.Max(q.x, q.y), 0) - 41;
                float edge = -distance, light = Mathf.Clamp01(.55f + (p.y - p.x) / 180);
                float value = edge < 1.5f ? .68f : edge < 3.5f ? Mathf.Lerp(.76f, 1, light) : edge < 6 ? .84f : Mathf.Lerp(.88f, 1, (float)y / size);
                if (round && edge > 6) value += .08f * Mathf.Exp(-((p.x + 25) * (p.x + 25) / 650 + (p.y - 40) * (p.y - 40) / 160));
                pixels[y * size + x] = new Color(value, value, value, Mathf.Clamp01(.5f - distance));
            }
            texture.SetPixels32(pixels); texture.Apply(false, true);
            var sprite = Sprite.Create(texture, new Rect(0, 0, size, size), Vector2.one * .5f, 100, 0,
                SpriteMeshType.FullRect, round ? Vector4.zero : Vector4.one * 48);
            sprite.name = texture.name; sprite.hideFlags = HideFlags.DontSave;
            surfaces.Add(round, sprite); return sprite;
        }
        private void OnDestroy()
        {
            foreach (var sprite in surfaces.Values)
            {
                if (sprite == null) continue;
                if (Application.isPlaying) { Destroy(sprite.texture); Destroy(sprite); }
                else { DestroyImmediate(sprite.texture); DestroyImmediate(sprite); }
            }
            surfaces.Clear();
        }
    }
}
