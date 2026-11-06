#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Shift.Game
{
    /// <summary>
    /// Editor-only art experiment, not a player progression or skeletal animation system.
    /// The approved composite keeps MORA physically grounded on the same illustrated island.
    /// </summary>
    public sealed class MoraApprovedPreview : MonoBehaviour
    {
        private readonly Sprite[] stages = new Sprite[4];
        private RectTransform rect;
        private Image current, incoming;
        private GameFeelSettings feel;
        private float blendTimer, reactionTimer;
        private bool transitioning;
        public int Stage { get; private set; } = 1;
        public static string ResourcePath(int stage)
            => "Mora/MoraScene_S" + Mathf.Clamp(stage, 1, 4);

        public static MoraApprovedPreview TryCreate(RectTransform parent,
            GameFeelSettings settings, Action tap)
        {
            var art = new Sprite[4];
            for (int i = 0; i < art.Length; i++)
            {
                art[i] = Resources.Load<Sprite>(ResourcePath(i + 1));
                if (art[i] == null) return null;
            }
            var root = PlaceholderVisuals.Rect("MORA Accepted Four-Form Preview", parent,
                new Vector2(.035f, .25f), new Vector2(.965f, .845f));
            var p = root.gameObject.AddComponent<MoraApprovedPreview>();
            p.rect = root;
            p.feel = settings;
            Array.Copy(art, p.stages, 4);
            p.current = root.gameObject.AddComponent<Image>();
            p.current.sprite = art[0];
            p.current.preserveAspect = true;
            p.current.raycastTarget = true;
            var rect2 = PlaceholderVisuals.Rect("MORA Incoming Form", root,
                Vector2.zero, Vector2.one);
            p.incoming = rect2.gameObject.AddComponent<Image>();
            p.incoming.sprite = art[0];
            p.incoming.preserveAspect = true;
            p.incoming.color = new Color(1f, 1f, 1f, 0f);
            p.incoming.raycastTarget = false;
            var button = root.gameObject.AddComponent<Button>();
            button.targetGraphic = p.current;
            button.transition = Selectable.Transition.None;
            button.onClick.AddListener(() => { if (tap != null) tap(); });
            return p;
        }

        public void ShowStage(int next)
        {
            next = Mathf.Clamp(next, 1, 4);
            if (next == Stage && !transitioning) return;
            if (transitioning && incoming.color.a >= .5f)
                current.sprite = incoming.sprite;
            Stage = next;
            incoming.sprite = stages[next - 1];
            incoming.color = new Color(1f, 1f, 1f, 0f);
            blendTimer = 0f;
            transitioning = true;
            if (feel != null && feel.reducedMotion) FinishBlend();
        }

        public void React() => reactionTimer = .5f;

        private void FinishBlend()
        {
            current.sprite = incoming.sprite;
            incoming.color = new Color(1f, 1f, 1f, 0f);
            transitioning = false;
        }

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;
            bool reduced = feel != null && feel.reducedMotion;
            if (transitioning)
            {
                if (reduced) FinishBlend();
                else
                {
                    blendTimer += dt;
                    float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(blendTimer / .38f));
                    incoming.color = new Color(1f, 1f, 1f, t);
                    if (blendTimer >= .38f) FinishBlend();
                }
            }
            reactionTimer = Mathf.Max(0f, reactionTimer - dt);
            float u = 1f - reactionTimer / .5f;
            // No full-body movement: this is combined character+habitat art.
            float pulse = reduced || reactionTimer <= 0f ? 0f :
                .012f * Mathf.Sin(Mathf.PI * Mathf.Clamp01(u));
            rect.localScale = Vector3.one * (1f + pulse);
        }

        private void OnDisable()
        {
            if (rect != null) rect.localScale = Vector3.one;
            reactionTimer = 0f;
            if (transitioning) FinishBlend();
        }
    }
}
#endif
