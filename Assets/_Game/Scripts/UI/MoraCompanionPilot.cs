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
        public MoraMoment LastMoment { get; private set; }

        public static MoraCompanionPilot TryCreate(RectTransform safeArea, GameFeelSettings settings)
        {
            var sprite = Resources.Load<Sprite>("Mora/Mora_Idle");
            if (sprite == null) return null;
            var area = PlaceholderVisuals.Rect("MORA Pilot", safeArea,
                new Vector2(.815f, .171f), new Vector2(.955f, .255f));
            var pilot = area.gameObject.AddComponent<MoraCompanionPilot>();
            pilot.rect = area;
            pilot.feel = settings;
            pilot.image = area.gameObject.AddComponent<Image>();
            pilot.image.sprite = sprite;
            pilot.image.preserveAspect = true;
            pilot.image.raycastTarget = false;
            pilot.opacity = area.gameObject.AddComponent<CanvasGroup>();
            pilot.opacity.alpha = 0f;
            pilot.opacity.blocksRaycasts = false;
            pilot.opacity.interactable = false;
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
            image.color = moment == MoraMoment.Perfect
                ? new Color(1f, .96f, .82f, 1f)
                : moment == MoraMoment.Chain ? new Color(.87f, 1f, .96f, 1f) : Color.white;
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
                    float bump = moment == MoraMoment.Perfect ? .09f
                        : moment == MoraMoment.Chain ? .075f : .035f;
                    rect.localScale = Vector3.one *
                        (1f + Mathf.Sin(t * Mathf.PI * 4f) * bump * (1f - t));
                    yield return null;
                }
            }
            opacity.alpha = 0f;
            rect.localScale = Vector3.one;
            reaction = null;
        }

        private void OnDisable()
        {
            if (reaction != null) { StopCoroutine(reaction); reaction = null; }
            if (opacity != null) opacity.alpha = 0f;
            if (rect != null) rect.localScale = Vector3.one;
        }
    }
}
