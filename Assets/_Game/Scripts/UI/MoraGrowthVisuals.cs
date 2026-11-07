using UnityEngine;
using UnityEngine.UI;

namespace Shift.Game
{
    /// <summary>
    /// Small additive habitat changes. The main island art and MORA identity stay consistent.
    /// This is a visual prototype, not full stage-specific artwork.
    /// </summary>
    public sealed class MoraGrowthVisuals : MonoBehaviour
    {
        private Image aura;
        private GameObject[] blossoms;
        private int stage = 1;
        public int Stage => stage;

        public void Initialize(Sprite circle)
        {
            if (circle == null) return;
            var glow = PlaceholderVisuals.Rect("Growth Core Aura", transform,
                new Vector2(.31f, .407f), new Vector2(.69f, .685f));
            aura = glow.gameObject.AddComponent<Image>();
            aura.sprite = circle;
            aura.type = Image.Type.Simple;
            aura.color = new Color(.47f, .98f, .83f, 0f);
            aura.raycastTarget = false;

            // Positioned on the island's near-side foliage, behind the avatar sprite.
            var anchors = new[]
            {
                new Vector2(.19f,.363f), new Vector2(.81f,.365f),
                new Vector2(.25f,.391f), new Vector2(.75f,.394f),
                new Vector2(.31f,.341f), new Vector2(.69f,.342f)
            };
            blossoms = new GameObject[anchors.Length];
            for (int i = 0; i < anchors.Length; i++)
            {
                var pos = anchors[i];
                var petal = PlaceholderVisuals.Rect("Growth Blossom " + (i + 1), transform,
                    pos - new Vector2(.012f, .006f), pos + new Vector2(.012f, .006f));
                var image = petal.gameObject.AddComponent<Image>();
                image.sprite = circle;
                image.type = Image.Type.Simple;
                image.color = i % 2 == 0
                    ? new Color(.95f, .72f, .75f, .78f)
                    : new Color(1f, .91f, .54f, .85f);
                image.raycastTarget = false;
                blossoms[i] = petal.gameObject;
            }
            SetStage(1);
        }

        public void SetStage(int nextStage)
        {
            stage = Mathf.Clamp(nextStage, 1, 4);
            if (aura != null)
            {
                float alpha = stage == 4 ? .095f : stage == 3 ? .06f : stage == 2 ? .025f : 0f;
                aura.color = new Color(.47f, .98f, .83f, alpha);
            }
            if (blossoms == null) return;
            int count = stage == 4 ? 6 : stage == 3 ? 4 : stage == 2 ? 2 : 0;
            for (int i = 0; i < blossoms.Length; i++)
                if (blossoms[i] != null) blossoms[i].SetActive(i < count);
        }
    }
}
