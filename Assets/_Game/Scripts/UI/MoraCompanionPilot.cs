using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Shift.Game
{
    /// <summary>Opt-in visual-only MORA pilot. Never changes board state, saves, or scores.</summary>
    public sealed class MoraCompanionPilot : MonoBehaviour
    {
        private CanvasGroup opacity;
        private Image image;
        private RectTransform rect;
        private GameFeelSettings feel;
        private Coroutine reaction;
        private Sprite idle, chain, happy, perfect;
        private Vector2 dockPosition;
        // Left of the text on the result card, clear of the Hint control.
        public static readonly Vector2 DockMin = new Vector2(.012f, .158f);
        public static readonly Vector2 DockMax = new Vector2(.168f, .258f);
        public static string OptionalSpriteResource(MoraMoment moment)
        {
            switch (moment)
            {
                case MoraMoment.Chain: return "Mora/Mora_Chain";
                case MoraMoment.Complete: return "Mora/Mora_Happy";
                case MoraMoment.Perfect: return "Mora/Mora_Perfect";
                default: return "Mora/Mora_Idle";
            }
        }
        public MoraMoment LastMoment { get; private set; }

        public static MoraCompanionPilot TryCreate(RectTransform safeArea, GameFeelSettings settings)
        {
            var sprite = Resources.Load<Sprite>("Mora/Mora_Idle");
            if (sprite == null) return null;
            var area = PlaceholderVisuals.Rect("MORA Pilot", safeArea,
                DockMin, DockMax);
            var pilot = area.gameObject.AddComponent<MoraCompanionPilot>();
            pilot.rect = area;
            pilot.feel = settings;
            pilot.idle = sprite;
            pilot.chain = Resources.Load<Sprite>("Mora/Mora_Chain");
            pilot.happy = Resources.Load<Sprite>("Mora/Mora_Happy");
            pilot.perfect = Resources.Load<Sprite>("Mora/Mora_Perfect");
            pilot.image = area.gameObject.AddComponent<Image>();
            pilot.image.sprite = sprite;
            pilot.image.preserveAspect = true;
            pilot.image.raycastTarget = false;
            pilot.opacity = area.gameObject.AddComponent<CanvasGroup>();
            pilot.opacity.alpha = 0f;
            pilot.opacity.blocksRaycasts = false;
            pilot.opacity.interactable = false;
            pilot.dockPosition = area.anchoredPosition;
            return pilot;
        }

        public void React(MoraMoment moment)
        {
            if (moment == MoraMoment.None || image == null) return;
            LastMoment = moment;
            if (reaction != null) StopCoroutine(reaction);
            reaction = StartCoroutine(Present(moment));
        }

        private IEnumerator Present(MoraMoment moment)
        {
            // No recurring normal-tap animation; meaningful moments get breathing room.
            float duration = moment == MoraMoment.Perfect ? 2.4f
                : moment == MoraMoment.Complete ? 1.9f : 1.3f;
            bool reduced = feel != null && feel.reducedMotion;
            Sprite expression = moment == MoraMoment.Chain ? chain
                : moment == MoraMoment.Perfect ? perfect
                : moment == MoraMoment.Complete ? happy : null;
            image.sprite = expression != null ? expression : idle;
            image.color = expression != null ? Color.white
                : moment == MoraMoment.Perfect ? new Color(1f, .96f, .82f)
                : moment == MoraMoment.Chain ? new Color(.87f, 1f, .96f) : Color.white;
            rect.anchoredPosition = dockPosition;
            rect.localRotation = Quaternion.identity;
            rect.localScale = Vector3.one;
            if (reduced)
            {
                rect.localScale = Vector3.one;
                opacity.alpha = 1f;
                yield return new WaitForSecondsRealtime(duration);
            }
            else
            {
                for (float elapsed = 0f; elapsed < duration; elapsed += Time.unscaledDeltaTime)
                {
                    float t = Mathf.Clamp01(elapsed / duration);
                    float fadeIn = Mathf.Clamp01(t * 6f);
                    float fadeOut = Mathf.Clamp01((1f - t) * 6f);
                    opacity.alpha = Mathf.Min(fadeIn, fadeOut);
                    // One soft rise + squash and tilt; avoid loops that distract from puzzles.
                    float impulse = Mathf.Sin(Mathf.PI * Mathf.Clamp01(t * 2.2f));
                    float settle = Mathf.Exp(-4f * t);
                    float lift = moment == MoraMoment.Perfect ? 11f : moment == MoraMoment.Chain ? 7f : 5f;
                    rect.anchoredPosition = dockPosition + Vector2.up * (impulse * lift);
                    float stretch = (moment == MoraMoment.Perfect ? .09f : .055f) * impulse * settle;
                    rect.localScale = new Vector3(1f - stretch * .35f, 1f + stretch, 1f);
                    float tilt = moment == MoraMoment.Chain ? 7f : 4f;
                    rect.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(t * Mathf.PI * 3f) * tilt * settle);
                    yield return null;
                }
            }
            ResetVisual();
            reaction = null;
        }

        private void ResetVisual()
        {
            if (opacity != null) opacity.alpha = 0f;
            if (rect != null)
            {
                rect.anchoredPosition = dockPosition;
                rect.localRotation = Quaternion.identity;
                rect.localScale = Vector3.one;
            }
            if (image != null) { image.sprite = idle; image.color = Color.white; }
        }
        private void OnDisable()
        {
            if (reaction != null) { StopCoroutine(reaction); reaction = null; }
            ResetVisual();
        }
    }
}
