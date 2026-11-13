using UnityEngine;
using UnityEngine.UI;

namespace Shift.Game
{
    // Chapter 6 visual experiment: a discreet curved bond over the EMPTY
    // lane beside linked stones, never a thick bar through puzzle symbols.
    // The link is presentation only: BoardManager owns the actual rule.
    [RequireComponent(typeof(RectTransform))]
    public sealed class TetherLinkGraphic : MonoBehaviour
    {
        private const int Segments = 12;
        private readonly Image[] edge = new Image[Segments];
        private readonly Image[] filament = new Image[Segments];

        private RectTransform endpointA, endpointB, overlay;
        private int boardWidth, boardHeight;
        private bool ready;

        public bool BondVisible => filament[0] != null && filament[0].enabled;
        public bool IsNonInteractive
        {
            get
            {
                var group = GetComponent<CanvasGroup>();
                if (group == null || group.blocksRaycasts || group.interactable)
                    return false;
                for (int i = 0; i < Segments; i++)
                    if (edge[i] == null || filament[i] == null ||
                        edge[i].raycastTarget || filament[i].raycastTarget)
                        return false;
                return true;
            }
        }

        public void Bind(RectTransform a, RectTransform b,
            int width, int height, Sprite rounded)
        {
            if (a == null || b == null || width <= 0 || height <= 0)
            {
                Debug.LogError("BAĞ cannot draw: invalid endpoint or board.",
                    this);
                enabled = false;
                return;
            }

            endpointA = a;
            endpointB = b;
            boardWidth = width;
            boardHeight = height;
            overlay = (RectTransform)transform;

            // Use UI Images (known to work in the SHIFT Canvas), but keep
            // them small and restrained: two narrow strokes, no glow bars
            // and no center ornament over the gate/exit icons.
            for (int i = 0; i < Segments; i++)
            {
                edge[i] = CreateStrip("Bond Arc Edge " + i, rounded);
                filament[i] = CreateStrip("Bond Arc Filament " + i, rounded);
            }

            var group = GetComponent<CanvasGroup>();
            if (group == null) group = gameObject.AddComponent<CanvasGroup>();
            group.interactable = false;
            group.blocksRaycasts = false;
            ready = true;
            Refresh();
        }

        private Image CreateStrip(string name, Sprite rounded)
        {
            var element = PlaceholderVisuals.Rect(name, transform,
                new Vector2(.5f, .5f), new Vector2(.5f, .5f));
            element.pivot = new Vector2(.5f, .5f);
            var image = element.gameObject.AddComponent<Image>();
            image.sprite = rounded;
            image.type = Image.Type.Simple;
            image.raycastTarget = false;
            return image;
        }

        private static Vector2 Curve(Vector2 a, Vector2 control,
            Vector2 b, float t)
        {
            // Quadratic Bezier: always deterministic, no runtime allocations.
            Vector2 left = Vector2.LerpUnclamped(a, control, t);
            Vector2 right = Vector2.LerpUnclamped(control, b, t);
            return Vector2.LerpUnclamped(left, right, t);
        }

        private static void Segment(Image image, Vector2 a,
            Vector2 b, float width)
        {
            Vector2 delta = b - a;
            RectTransform rect = image.rectTransform;
            rect.anchoredPosition = (a + b) * .5f;
            rect.localRotation = Quaternion.Euler(0f, 0f,
                Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
            rect.sizeDelta = new Vector2(delta.magnitude + width * .22f,
                width);
        }

        public void Refresh()
        {
            if (!ready || overlay == null) return;

            bool show = endpointA != null && endpointB != null &&
                endpointA.gameObject.activeInHierarchy &&
                endpointB.gameObject.activeInHierarchy &&
                overlay.rect.width > 1f && overlay.rect.height > 1f;

            for (int i = 0; i < Segments; i++)
            {
                edge[i].enabled = show;
                filament[i].enabled = show;
            }
            if (!show) return;

            Vector2 aCenter = overlay.InverseTransformPoint(
                endpointA.TransformPoint(endpointA.rect.center));
            Vector2 bCenter = overlay.InverseTransformPoint(
                endpointB.TransformPoint(endpointB.rect.center));
            float cell = Mathf.Min(overlay.rect.width / boardWidth,
                overlay.rect.height / boardHeight);
            if (cell < 1f) return;

            // Choose the spacious edge of the board. The 101–104 pilots
            // deliberately have a clear lane ABOVE their linked stones.
            // When future layouts place stones at the top, route below them.
            float upperRoom = overlay.rect.yMax -
                Mathf.Max(aCenter.y, bCenter.y);
            float lowerRoom = Mathf.Min(aCenter.y, bCenter.y) -
                overlay.rect.yMin;
            bool above = upperRoom >= cell * .5f ||
                upperRoom >= lowerRoom;
            float sign = above ? 1f : -1f;

            // Attach near each stone's rim rather than crossing arrow faces.
            Vector2 a = aCenter + Vector2.up * (sign * cell * .30f);
            Vector2 b = bCenter + Vector2.up * (sign * cell * .30f);
            float controlY = above
                ? Mathf.Min(overlay.rect.yMax - cell * .15f,
                    Mathf.Max(a.y, b.y) + cell * 1.20f)
                : Mathf.Max(overlay.rect.yMin + cell * .15f,
                    Mathf.Min(a.y, b.y) - cell * 1.20f);
            Vector2 control = new Vector2((a.x + b.x) * .5f, controlY);

            // Use a soft, desaturated brass filament from SHIFT's existing
            // gold accents. At portrait scale it is ~1–2 physical pixels,
            // with a navy separation stroke against both board and sky.
            float stroke = Mathf.Clamp(cell * .038f, 2.5f, 4.5f);
            Vector2 aa = (endpointA.anchorMin + endpointA.anchorMax) * .5f;
            Vector2 bb = (endpointB.anchorMin + endpointB.anchorMax) * .5f;
            float manhattan = Mathf.Abs(aa.x - bb.x) * boardWidth +
                Mathf.Abs(aa.y - bb.y) * boardHeight;
            bool taut = manhattan >= BoardManager.TetherMaxDistance - .1f;

            Color32 edgeColor = new Color32(19, 37, 52, 190);
            Color32 filamentColor = taut
                ? new Color32(246, 217, 149, 233)
                : new Color32(218, 192, 136, 178);
            float lineWidth = stroke * (taut ? 1.15f : 1f);

            for (int i = 0; i < Segments; i++)
            {
                Vector2 from = Curve(a, control, b,
                    (float)i / Segments);
                Vector2 to = Curve(a, control, b,
                    (float)(i + 1) / Segments);
                Segment(edge[i], from, to, lineWidth * 1.9f);
                Segment(filament[i], from, to, lineWidth);
                edge[i].color = edgeColor;
                filament[i].color = filamentColor;
            }
        }

        private void LateUpdate() => Refresh();
    }
}
