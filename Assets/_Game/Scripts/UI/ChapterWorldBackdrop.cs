using UnityEngine;
using UnityEngine.UI;

namespace Shift.Game
{
    // Quiet, decorative Chapter 3-5 world map, behind the selection buttons.
    // No additional textures, save data, network dependency, raycasts or input.
    // Completion data ONLY comes from the existing LevelProgression.
    [RequireComponent(typeof(RectTransform))]
    public sealed class ChapterWorldBackdrop : MaskableGraphic
    {
        private ChapterJourneyCatalog.Chapter chapter;
        private int fromBeats, toBeats;
        private float transition = 1f;
        private bool reducedMotion;
        private Color restoredColor;
        private Color mutedColor;
        private static readonly Vector2[] Valley =
        {
            new Vector2(.09f, .37f), new Vector2(.29f, .49f),
            new Vector2(.50f, .31f), new Vector2(.71f, .51f),
            new Vector2(.90f, .65f)
        };
        private static readonly Vector2[] Echo =
        {
            new Vector2(.09f, .48f), new Vector2(.29f, .68f),
            new Vector2(.50f, .29f), new Vector2(.71f, .64f),
            new Vector2(.90f, .42f)
        };
        private static readonly Vector2[] Confluence =
        {
            new Vector2(.09f, .58f), new Vector2(.29f, .39f),
            new Vector2(.50f, .66f), new Vector2(.71f, .35f),
            new Vector2(.90f, .61f)
        };

        protected override void Awake()
        {
            base.Awake();
            raycastTarget = false;
            color = Color.white;
        }

        public void Show(LevelProgression progress, int chapterIndex,
            bool motionReduced)
        {
            var definition = ChapterJourneyCatalog.ForChapter(chapterIndex);
            if (definition == null ||
                !ChapterJourneyCatalog.Available(progress, chapterIndex))
            {
                Hide();
                return;
            }
            int finished = ChapterJourneyCatalog.LastRestoredBeat(
                definition, progress.HighestCompleted) + 1;
            bool changedChapter = chapter != definition;
            if (changedChapter)
            {
                fromBeats = finished;
                toBeats = finished;
                transition = 1f;
            }
            else if (finished > toBeats)
            {
                fromBeats = toBeats;
                toBeats = finished;
                transition = motionReduced ? 1f : 0f;
            }
            else if (finished < toBeats)
            {
                // A development reset or restored save should not display
                // world-state milestones that are no longer completed.
                fromBeats = finished;
                toBeats = finished;
                transition = 1f;
            }

            chapter = definition;
            reducedMotion = motionReduced;
            if (reducedMotion) transition = 1f;
            if (!ColorUtility.TryParseHtmlString(
                definition.paletteEnd, out restoredColor))
                restoredColor = new Color32(79, 155, 151, 255);
            if (!ColorUtility.TryParseHtmlString(
                definition.paletteStart, out mutedColor))
                mutedColor = new Color32(58, 87, 109, 255);
            if (!gameObject.activeSelf) gameObject.SetActive(true);
            SetVerticesDirty();
        }

        public void Hide()
        {
            chapter = null;
            gameObject.SetActive(false);
        }

        private void Update()
        {
            if (transition >= 1f) return;
            if (reducedMotion)
                transition = 1f;
            else
                transition = Mathf.Min(1f, transition +
                    Time.unscaledDeltaTime / .42f);
            SetVerticesDirty();
        }

        private static Color WithAlpha(Color c, float a)
        {
            c.a = Mathf.Clamp01(a);
            return c;
        }

        private Vector2 Position(float x, float y)
        {
            var bounds = rectTransform.rect;
            return new Vector2(bounds.xMin + x * bounds.width,
                bounds.yMin + y * bounds.height);
        }

        private Vector2 Position(Vector2 normalized) =>
            Position(normalized.x, normalized.y);

        private static void Triangle(VertexHelper helper,
            Vector2 a, Vector2 b, Vector2 c, Color color)
        {
            int offset = helper.currentVertCount;
            var vertex = UIVertex.simpleVert;
            vertex.color = color;
            vertex.position = a;
            helper.AddVert(vertex);
            vertex.position = b;
            helper.AddVert(vertex);
            vertex.position = c;
            helper.AddVert(vertex);
            helper.AddTriangle(offset, offset + 1, offset + 2);
        }

        private static void Quad(VertexHelper helper,
            Vector2 a, Vector2 b, Vector2 c, Vector2 d, Color color)
        {
            Triangle(helper, a, b, c, color);
            Triangle(helper, a, c, d, color);
        }

        private static void Line(VertexHelper helper,
            Vector2 a, Vector2 b, float width, Color color)
        {
            Vector2 delta = b - a;
            if (delta.sqrMagnitude < .0001f) return;
            Vector2 perpendicular = new Vector2(-delta.y, delta.x)
                .normalized * width * .5f;
            Quad(helper, a - perpendicular, a + perpendicular,
                b + perpendicular, b - perpendicular, color);
        }

        private static void Ellipse(VertexHelper helper,
            Vector2 center, float xRadius, float yRadius, Color color,
            int segments = 20)
        {
            float angleStep = Mathf.PI * 2f / segments;
            Vector2 previous = center + new Vector2(xRadius, 0f);
            for (int i = 1; i <= segments; i++)
            {
                float angle = angleStep * i;
                Vector2 next = center + new Vector2(
                    Mathf.Cos(angle) * xRadius,
                    Mathf.Sin(angle) * yRadius);
                Triangle(helper, center, previous, next, color);
                previous = next;
            }
        }

        private Vector2[] Route()
        {
            if (chapter.startLevel == 41) return Valley;
            if (chapter.startLevel == 61) return Echo;
            return Confluence;
        }

        private float Reveal(int milestoneIndex)
        {
            if (milestoneIndex < fromBeats) return 1f;
            if (milestoneIndex >= toBeats) return 0f;
            return transition >= 1f ? 1f
                : Mathf.SmoothStep(0f, 1f, transition);
        }

        protected override void OnPopulateMesh(VertexHelper helper)
        {
            helper.Clear();
            if (chapter == null) return;
            Rect bounds = rectTransform.rect;
            if (bounds.width <= 0f || bounds.height <= 0f) return;

            // Existing SHIFT alpine-language silhouettes: light enough to
            // preserve level-tile contrast and puzzle-first visual hierarchy.
            Color distant = WithAlpha(mutedColor, .065f);
            Color nearby = WithAlpha(mutedColor, .11f);
            Triangle(helper, Position(-.07f, .48f), Position(.18f, .94f),
                Position(.48f, .48f), distant);
            Triangle(helper, Position(.28f, .49f), Position(.63f, 1.02f),
                Position(.93f, .49f), distant);
            Triangle(helper, Position(.61f, .48f), Position(.89f, .87f),
                Position(1.06f, .48f), nearby);

            Vector2 lake = Position(.49f, .21f);
            Ellipse(helper, lake, bounds.width * .51f,
                bounds.height * .19f, WithAlpha(restoredColor, .07f));
            Line(helper, Position(.09f, .24f), Position(.89f, .24f),
                Mathf.Max(1f, bounds.height * .006f),
                WithAlpha(mutedColor, .10f));

            var route = Route();
            float stroke = Mathf.Clamp(bounds.height * .014f, 1f, 3.5f);
            for (int i = 0; i < route.Length - 1; i++)
            {
                Vector2 a = Position(route[i]);
                Vector2 b = Position(route[i + 1]);
                // Dormant network is present but broken / muted.
                Line(helper, a, b, stroke,
                    WithAlpha(mutedColor, .15f));
                float strength = Mathf.Min(Reveal(i), Reveal(i + 1));
                if (strength > 0f)
                {
                    Line(helper, a, b, stroke * 1.7f,
                        WithAlpha(restoredColor, .27f * strength));
                }
            }

            // World nodes appear permanently connected only after their real
            // milestone level is recorded complete, never merely unlocked.
            for (int i = 0; i < route.Length; i++)
            {
                Vector2 point = Position(route[i]);
                float radius = Mathf.Clamp(bounds.height * .025f, 3f, 9f);
                float restored = Reveal(i);
                Ellipse(helper, point, radius * 1.5f, radius * 1.5f,
                    WithAlpha(restoredColor, .095f * restored));
                Ellipse(helper, point, radius, radius,
                    WithAlpha(mutedColor, .16f));
                if (restored > 0f)
                    Ellipse(helper, point, radius * 1.16f, radius * 1.16f,
                        WithAlpha(restoredColor, .55f * restored));
            }

            // Reaching 60/80/100 yields one visible horizon/beacon change.
            // The final state is present with motion disabled too.
            float finale = Reveal(4);
            if (finale > 0f)
            {
                Vector2 horizon = Position(.86f, .82f);
                Ellipse(helper, horizon, bounds.width * .11f,
                    bounds.height * .08f,
                    WithAlpha(restoredColor, .055f * finale));
                Line(helper, Position(.72f, .80f), Position(.99f, .80f),
                    stroke * 1.8f,
                    WithAlpha(restoredColor, .31f * finale));
            }
        }
    }
}
