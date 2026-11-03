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
            if (sprite == null) return null; // Missing art means zero changes to current UI.
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
            float duration = moment == MoraMoment.Perfect ? 2.1f
                : moment == MoraMoment.Complete ? 1.65f : 1.05f;
            bool reduced = feel != null && feel.reducedMotion;
            image.color = moment == MoraMoment.Perfect
                ? new Color(1f, .96f, .82f, 1f) : Color.white;
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
                    opacity.alpha = Mathf.Min(Mathf.Clamp01(t * 7f), Mathf.Clamp01((1f - t) * 6f));
                    float bump = moment == MoraMoment.Perfect ? .085f : .042f;
                    rect.localScale = Vector3.one * (1f + Mathf.Sin(t * Mathf.PI * 4f) * bump * (1f - t));
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
