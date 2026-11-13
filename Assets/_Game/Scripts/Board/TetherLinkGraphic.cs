using UnityEngine;
using UnityEngine.UI;

namespace Shift.Game
{
    // A clear, persistent bond between the two linked stones.
    //
    // The original custom Mesh graphic did not appear in the Game view on
    // the Chapter 6 pilot. Use ordinary Unity UI Images instead (the same
    // rendering path as the already visible Resonance signal). Three passive
    // overlapping strips provide a dark outline, a warm glow and a bright
    // center, all safely behind the actual piece graphics.
    [RequireComponent(typeof(RectTransform))]
    public sealed class TetherLinkGraphic : MonoBehaviour
    {
        private RectTransform endpointA, endpointB;
        private RectTransform overlay;
        private Image outline, halo, core, knot;
        private int boardWidth, boardHeight;
        private bool ready;

        public bool BondVisible => core != null && core.enabled;
        public bool IsNonInteractive =>
            outline != null && !outline.raycastTarget &&
            halo != null && !halo.raycastTarget &&
            core != null && !core.raycastTarget &&
            knot != null && !knot.raycastTarget;

        public void Bind(RectTransform a, RectTransform b,
            int width, int height, Sprite rounded)
        {
            if (a == null || b == null || width <= 0 || height <= 0)
            {
                Debug.LogError("Cannot draw BAĞ: invalid endpoint or board.", this);
                enabled = false;
                return;
            }

            endpointA = a;
            endpointB = b;
            boardWidth = width;
            boardHeight = height;
            overlay = (RectTransform)transform;

            // UI components, rather than a hand-authored mesh, also obey
            // CanvasScaler, Canvas clipping and ordinary sibling ordering.
            outline = MakeStrip("Bond Dark Outline", rounded);
            halo = MakeStrip("Bond Gold Halo", rounded);
            core = MakeStrip("Bond Bright Center", rounded);
            knot = MakeStrip("Bond Center Knot", rounded);

            var group = GetComponent<CanvasGroup>();
            if (group == null) group = gameObject.AddComponent<CanvasGroup>();
            group.interactable = false;
            group.blocksRaycasts = false;

            ready = true;
            Refresh();
        }

        private Image MakeStrip(string name, Sprite rounded)
        {
            var rect = PlaceholderVisuals.Rect(name, transform,
                new Vector2(.5f, .5f), new Vector2(.5f, .5f));
            rect.pivot = new Vector2(.5f, .5f);
            var graphic = rect.gameObject.AddComponent<Image>();
            graphic.sprite = rounded;
            graphic.type = Image.Type.Simple;
            graphic.raycastTarget = false;
            return graphic;
        }

        private static void PlaceStrip(Image image, Vector2 middle,
            float length, float thickness, float angle)
        {
            RectTransform rect = image.rectTransform;
            rect.localRotation = Quaternion.Euler(0f, 0f, angle);
            rect.anchoredPosition = middle;
            rect.sizeDelta = new Vector2(length, thickness);
        }

        // A public refresh makes the actual UI geometry testable in EditMode.
        // It is also used during the frame when the board is initially built.
        public void Refresh()
        {
            if (!ready || overlay == null) return;
            bool show = endpointA != null && endpointB != null &&
                endpointA.gameObject.activeInHierarchy &&
                endpointB.gameObject.activeInHierarchy;

            outline.enabled = halo.enabled = core.enabled = knot.enabled = show;
            if (!show) return;

            // TransformPoint / InverseTransformPoint includes actual layout,
            // board pivot and in-flight Piece.Place animations. Computing only
            // from the anchors can put lines in the wrong local space.
            Vector2 a = overlay.InverseTransformPoint(
                endpointA.TransformPoint(endpointA.rect.center));
            Vector2 b = overlay.InverseTransformPoint(
                endpointB.TransformPoint(endpointB.rect.center));
            Vector2 delta = b - a;
            float distance = delta.magnitude;
            if (distance < 1f)
            {
                outline.enabled = halo.enabled = core.enabled =
                    knot.enabled = false;
                return;
            }

            Vector2 center = (a + b) * .5f;
            float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
            float cell = Mathf.Min(overlay.rect.width / boardWidth,
                overlay.rect.height / boardHeight);
            float stroke = Mathf.Clamp(cell * .105f, 3.5f, 7f);

            // Manhattan distance is deliberately consistent with the
            // actual BoardManager TetherMaxDistance rule.
            Vector2 aCell = (endpointA.anchorMin + endpointA.anchorMax) * .5f;
            Vector2 bCell = (endpointB.anchorMin + endpointB.anchorMax) * .5f;
            float manhattan = Mathf.Abs(aCell.x - bCell.x) * boardWidth +
                Mathf.Abs(aCell.y - bCell.y) * boardHeight;
            bool taut = manhattan >= BoardManager.TetherMaxDistance - .15f;

            PlaceStrip(outline, center, distance + stroke * .5f,
                stroke * 3.15f, angle);
            PlaceStrip(halo, center, distance + stroke * .25f,
                stroke * 2.1f, angle);
            PlaceStrip(core, center, distance, stroke, angle);

            var knotRect = knot.rectTransform;
            knotRect.anchoredPosition = center;
            knotRect.localRotation = Quaternion.Euler(0f, 0f, 45f);
            knotRect.sizeDelta = Vector2.one * (stroke * 1.65f);

            outline.color = new Color32(15, 31, 43, 235);
            halo.color = taut ? new Color32(249, 177, 65, 180) :
                new Color32(244, 190, 87, 137);
            core.color = taut ? new Color32(255, 231, 145, 255) :
                new Color32(250, 207, 109, 244);
            knot.color = taut ? new Color32(255, 224, 124, 255) :
                new Color32(251, 198, 93, 229);
        }

        private void LateUpdate() => Refresh();
    }
}
