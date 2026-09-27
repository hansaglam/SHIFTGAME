using System;
using UnityEngine;
using UnityEngine.UI;

namespace Shift.Game
{
    public sealed class Piece : MonoBehaviour
    {
        private RectTransform rect, body, trailRect;
        private PieceAppearance appearance;
        public bool GateShownOpen => appearance != null && appearance.GateShownOpen;
        private Button button;
        private Image image, trail, exitGlow;
        private CanvasGroup opacity;
        private Color baseColor;
        private GameFeelSettings feel;
        private int width, height;
        private float pulseTime, pulseStrength, exitScale = 1;
        private bool pulsing, blocked;
        public BoardPiece Data { get; private set; }
        public Transform VisualTransform => transform;

        public void Initialize(BoardPiece data, int boardWidth, int boardHeight, Font font, Sprite circle,
            Sprite rounded, GameFeelSettings settings, Action<GridPosition> onTap, BoardVisualResources art = null)
        {
            Data = data; width = boardWidth; height = boardHeight; feel = settings;
            rect = (RectTransform)transform; Place(data.Position);
            opacity = gameObject.AddComponent<CanvasGroup>();
            var shadow = PlaceholderVisuals.Rect("Shadow", transform, new Vector2(.13f, .08f), new Vector2(.89f, .84f)).gameObject.AddComponent<Image>();
            shadow.sprite = data.Type == PieceType.Normal ? circle : rounded; shadow.type = Image.Type.Sliced;
            shadow.color = new Color(0, 0, 0, .23f); shadow.raycastTarget = false;
            trailRect = PlaceholderVisuals.Rect("Motion Accent", transform, new Vector2(.16f, .16f), new Vector2(.84f, .84f));
            trail = trailRect.gameObject.AddComponent<Image>(); trail.sprite = circle; trail.raycastTarget = false;
            trail.color = Color.clear;
            body = PlaceholderVisuals.Rect("Body", transform, new Vector2(.12f, .12f), new Vector2(.88f, .88f));
            image = body.gameObject.AddComponent<Image>(); image.sprite = rounded; image.type = Image.Type.Sliced;
            image.color = PlaceholderVisuals.ColorFor(data.Color); image.raycastTarget = data.Type == PieceType.Normal;
            baseColor = PieceAppearance.PieceTint(data.Color);
            if (art == null) art = gameObject.AddComponent<BoardVisualResources>();
            appearance = new PieceAppearance(data,image,font,circle,rounded,art);
            if (data.Type == PieceType.Exit)
            {
                exitGlow = PlaceholderVisuals.Rect("Exit Glow",transform,new Vector2(.065f,.065f),new Vector2(.935f,.935f)).gameObject.AddComponent<Image>();
                exitGlow.transform.SetSiblingIndex(0); exitGlow.sprite = rounded; exitGlow.type = Image.Type.Sliced;
                exitGlow.raycastTarget = false;
                var glowColor = data.Color == PieceColor.None ? (Color)new Color32(51,190,142,255) : PieceAppearance.PieceTint(data.Color);
                exitGlow.color = new Color(glowColor.r,glowColor.g,glowColor.b,.16f);
            }
            if (data.Type == PieceType.Normal)
            {
                button = body.gameObject.AddComponent<Button>(); button.targetGraphic = image;
                button.transition = Selectable.Transition.None;
                button.onClick.AddListener(() => onTap(Data.Position));
            }
        }

        public void ShowGate(bool open)
        {
            if (Data.Type != PieceType.Gate) return;
            appearance.ShowGate(open);
        }
        public void RestoreFromModel()
        {
            pulsing = false; pulseTime = 0; exitScale = 1;
            gameObject.SetActive(Data.Active); Place(Data.Position); ShowDirection(Data.Direction); ShowGate(Data.GateOpen);
            opacity.alpha = 1; body.anchoredPosition = Vector2.zero; appearance.Highlight(0); body.localScale = Vector3.one; trail.color = Color.clear; image.color = Color.white;
        }
        public void Feedback(float strength, bool isBlocked = false)
        {
            pulseTime = 0; pulseStrength = strength; blocked = isBlocked; pulsing = true;
            UpdateFeedback(0);
        }
        private void Update()
        {
            if (exitGlow != null)
            {
                var tint = exitGlow.color;
                tint.a = feel.reducedMotion ? .2f : .2f + .07f * Mathf.Sin(Time.unscaledTime * Mathf.PI) + (pulsing ? .3f * (1 - Mathf.Clamp01(pulseTime / Mathf.Max(.001f, feel.piecePulseDuration))) : 0);
                exitGlow.color = tint;
            }
            if (!pulsing) return;
            pulseTime += Time.unscaledDeltaTime;
            float duration = blocked ? feel.blockedDuration : feel.piecePulseDuration;
            float t = Mathf.Clamp01(pulseTime / Mathf.Max(.001f, duration));
            UpdateFeedback(t);
            if (t >= 1) pulsing = false;
        }
        private void UpdateFeedback(float t)
        {
            float envelope = 1 - t;
            float punch = feel.reducedMotion ? 0 : pulseStrength * Mathf.Cos(t * Mathf.PI * 2) * envelope;
            body.localScale = Vector3.one * (exitScale * (1 + punch));
            body.anchoredPosition = blocked && !feel.reducedMotion
                ? new Vector2(Mathf.Sin(t * Mathf.PI * 4) * feel.blockedPixels * envelope, 0) : Vector2.zero;
            appearance.Highlight(envelope * feel.highlightStrength);
        }
        public void MotionAccent(Vector2 direction, float progress)
        {
            trailRect.anchoredPosition = new Vector2(-direction.x * rect.rect.width, -direction.y * rect.rect.height) * .22f;
            var tint = baseColor; tint.a = feel.reducedMotion ? 0 : feel.trailOpacity * Mathf.Sin(progress * Mathf.PI);
            trail.color = tint;
        }
        public void SetExit(float progress)
        {
            exitScale = feel.reducedMotion ? 1 : 1 - progress;
            body.localScale = Vector3.one * exitScale; opacity.alpha = 1 - progress;
        }
        public void SetInput(bool enabled) { if (button != null) button.interactable = enabled; }
        public void ShowDirection(Direction direction)
        { if (Data.Type == PieceType.Normal || Data.Type == PieceType.Direction) appearance.Direction(direction); }
        public void Place(GridPosition position) => Place(new Vector2(position.x, position.y));
        public void Place(Vector2 position)
        {
            rect.anchorMin = new Vector2(position.x / width, position.y / height);
            rect.anchorMax = new Vector2((position.x + 1) / width, (position.y + 1) / height);
            rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
        }
    }
}
