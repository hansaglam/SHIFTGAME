using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Shift.Game
{
    /// <summary>
    /// Non-interactive MORA cameo. Opt-in pilot; never mutates puzzle logic or player saves.
    /// </summary>
    public sealed class MoraCompanionPilot : MonoBehaviour
    {
        private CanvasGroup opacity;
        private Image image;
        private RectTransform rect;
        private GameFeelSettings feel;
        private Coroutine reaction;
        private Sprite idle, chain, happy, perfect;
        private Vector2 dockPosition;

        // Preserve the user-tested v0.4.1 HUD placement.
        public static readonly Vector2 DockMin = new Vector2(0f, .135f);
        public static readonly Vector2 DockMax = new Vector2(.22f, .277f);

        public MoraMoment LastMoment { get; private set; }

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

        // The teal prototype must not be shown as an unearned default companion.
        // Primary seed art is opt-in and may be absent until its HUD expression pack ships.
        public static string PrimarySpriteResource(MoraMoment moment)
        {
            switch (moment)
            {
                case MoraMoment.Chain: return "Mora/MoraSeed_Chain";
                case MoraMoment.Complete: return "Mora/MoraSeed_Happy";
                case MoraMoment.Perfect: return "Mora/MoraSeed_Perfect";
                default: return "Mora/MoraSeed_Idle";
            }
        }

        public static MoraCompanionPilot TryCreate(RectTransform safeArea, GameFeelSettings settings,
            bool primaryOnly = false)
        {
            var sprite = Resources.Load<Sprite>(primaryOnly
                ? PrimarySpriteResource(MoraMoment.None)
                : OptionalSpriteResource(MoraMoment.None));
            if (sprite == null) return null;
            var area = PlaceholderVisuals.Rect("MORA Pilot", safeArea, DockMin, DockMax);
            var pilot = area.gameObject.AddComponent<MoraCompanionPilot>();
            pilot.rect = area;
            pilot.feel = settings;
            pilot.idle = sprite;
            pilot.chain = Resources.Load<Sprite>(primaryOnly
                ? PrimarySpriteResource(MoraMoment.Chain) : OptionalSpriteResource(MoraMoment.Chain));
            pilot.happy = Resources.Load<Sprite>(primaryOnly
                ? PrimarySpriteResource(MoraMoment.Complete) : OptionalSpriteResource(MoraMoment.Complete));
            pilot.perfect = Resources.Load<Sprite>(primaryOnly
                ? PrimarySpriteResource(MoraMoment.Perfect) : OptionalSpriteResource(MoraMoment.Perfect));
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
            if (MoraMotionProfile.Duration(moment) <= 0f || image == null || !isActiveAndEnabled)
                return;
            LastMoment = moment;
            if (reaction != null) StopCoroutine(reaction);
            reaction = StartCoroutine(Present(moment));
        }

        private IEnumerator Present(MoraMoment moment)
        {
            Sprite expression = moment == MoraMoment.Chain ? chain
                : moment == MoraMoment.Perfect ? perfect
                : moment == MoraMoment.Complete ? happy : null;
            image.sprite = expression != null ? expression : idle;
            image.color = expression != null ? Color.white
                : moment == MoraMoment.Perfect ? new Color(1f, .96f, .82f)
                : moment == MoraMoment.Chain ? new Color(.87f, 1f, .96f) : Color.white;

            float duration = MoraMotionProfile.Duration(moment);
            float elapsed = 0f;
            // Use the user's accessibility setting and unscaled time for visual-only effects.
            while (elapsed < duration)
            {
                bool reducedMotion = feel != null && feel.reducedMotion;
                var pose = MoraMotionProfile.Sample(moment, elapsed / duration, reducedMotion);
                ApplyPose(pose);
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
            ResetVisual();
            reaction = null;
        }

        private void ApplyPose(MoraMotionFrame pose)
        {
            opacity.alpha = pose.Opacity;
            rect.anchoredPosition = dockPosition + Vector2.up * pose.VerticalOffset;
            rect.localRotation = Quaternion.Euler(0f, 0f, pose.TiltDegrees);
            rect.localScale = new Vector3(pose.ScaleX, pose.ScaleY, 1f);
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

    /// <summary>Simple deterministic output pose, independent of frame rate and board state.</summary>
    public readonly struct MoraMotionFrame
    {
        public readonly float Opacity;
        public readonly float VerticalOffset;
        public readonly float TiltDegrees;
        public readonly float ScaleX;
        public readonly float ScaleY;

        public MoraMotionFrame(float opacity, float verticalOffset, float tiltDegrees,
            float scaleX, float scaleY)
        {
            Opacity = opacity;
            VerticalOffset = verticalOffset;
            TiltDegrees = tiltDegrees;
            ScaleX = scaleX;
            ScaleY = scaleY;
        }
    }

    /// <summary>
    /// v0.5 personality motion: one bounce for Chain, gentle double bounce for Complete,
    /// and an emphasized double bounce for Perfect. Reduced Motion is static.
    /// </summary>
    public static class MoraMotionProfile
    {
        public static float Duration(MoraMoment moment)
        {
            switch (moment)
            {
                case MoraMoment.Chain: return 1.3f;
                case MoraMoment.Complete: return 1.9f;
                case MoraMoment.Perfect: return 2.4f;
                default: return 0f;
            }
        }

        private static float Pulse(float time, float start, float length)
        {
            float phase = (time - start) / length;
            if (phase <= 0f || phase >= 1f) return 0f;
            return Mathf.Sin(Mathf.PI * phase);
        }

        public static MoraMotionFrame Sample(MoraMoment moment, float normalizedTime, bool reducedMotion)
        {
            if (Duration(moment) <= 0f) return new MoraMotionFrame(0f, 0f, 0f, 1f, 1f);

            float t = Mathf.Clamp01(normalizedTime);
            if (reducedMotion) return new MoraMotionFrame(1f, 0f, 0f, 1f, 1f);

            float enter = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / .12f));
            float leave = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((1f - t) / .17f));
            float opacity = Mathf.Min(enter, leave);
            float anticipation = Pulse(t, 0f, .17f);
            float first = Pulse(t, .10f, moment == MoraMoment.Chain ? .32f : .28f);
            float second = moment == MoraMoment.Chain ? 0f : Pulse(t, .37f, .24f);

            float height = moment == MoraMoment.Perfect ? 36f
                : moment == MoraMoment.Complete ? 28f : 25f;
            float lift = height * first + height * .38f * second;
            float sway = Mathf.Sin(t * Mathf.PI * 3f) * Mathf.Pow(1f - t, 2f);
            float tilt = (moment == MoraMoment.Chain ? 7f : 5f) * sway;
            float strength = moment == MoraMoment.Perfect ? .11f
                : moment == MoraMoment.Complete ? .085f : .07f;
            float jump = first + second * .58f;
            float scaleX = 1f + strength * .55f * anticipation - strength * .35f * jump;
            float scaleY = 1f - strength * .65f * anticipation + strength * jump;

            return new MoraMotionFrame(opacity, lift, tilt, scaleX, scaleY);
        }
    }
}
