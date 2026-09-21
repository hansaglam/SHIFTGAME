using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Shift.Game
{
    public sealed class BoardView : MonoBehaviour
    {
        private Piece[] pieces;
        private GameFeelSettings feel;
        private AudioManager audioManager;
        private HapticService haptics;
        private GameHud hud;
        private RectTransform rect;
        private float shakeRemaining, successRemaining;
        public void Build(BoardManager board, Font font, Sprite circle, Sprite rounded, GameFeelSettings settings,
            AudioManager audio, HapticService hapticService, GameHud gameHud, Action<GridPosition> onTap, bool sculptedTopology = false)
        {
            feel = settings; audioManager = audio; haptics = hapticService; hud = gameHud; rect = (RectTransform)transform;
            var art = gameObject.AddComponent<BoardVisualResources>();
            var backing = gameObject.AddComponent<Image>(); backing.sprite = rounded; backing.type = Image.Type.Sliced;
            backing.color = new Color32(25,42,62,255); backing.raycastTarget = false;
            var frame = VisualTheme.Surface("Outer Frame", transform, art.Surface(BoardSurface.Frame,new Color32(48,76,109,255)), Color.white, Vector2.zero, Vector2.one);
            frame.rectTransform.offsetMin = new Vector2(-18,-18); frame.rectTransform.offsetMax = new Vector2(18,18);
            frame.pixelsPerUnitMultiplier = .6f; VisualTheme.Shadow(frame, 10);
            var lip = VisualTheme.Surface("Inner Lip",transform,art.Surface(BoardSurface.Well,new Color32(15,28,45,255)),Color.white,Vector2.zero,Vector2.one);
            lip.rectTransform.offsetMin=new Vector2(-5,-5); lip.rectTransform.offsetMax=new Vector2(5,5); lip.pixelsPerUnitMultiplier=.7f;
            VisualTheme.Surface("Inner Well", transform, rounded, new Color32(18,31,46,255), Vector2.zero, Vector2.one);
            var aspect = gameObject.AddComponent<AspectRatioFitter>();
            aspect.aspectMode = AspectRatioFitter.AspectMode.FitInParent; aspect.aspectRatio = (float)board.Width / board.Height;
            for (int y = 0; y < board.Height; y++)
                for (int x = 0; x < board.Width; x++)
                {
                    if (sculptedTopology && board.GetTerrain(new GridPosition(x, y))?.Type == PieceType.Wall) continue;
                    var cell = PlaceholderVisuals.Rect($"Cell {x},{y}", transform,
                        new Vector2((float)x / board.Width, (float)y / board.Height), new Vector2((float)(x + 1) / board.Width, (float)(y + 1) / board.Height));
                    cell.offsetMin = new Vector2(4, 4); cell.offsetMax = new Vector2(-4, -4);
                    cell.gameObject.AddComponent<GridCell>().Initialize(new GridPosition(x, y));
                    var image = cell.gameObject.AddComponent<Image>(); image.sprite = art.Surface(BoardSurface.Cell,new Color32(60,82,106,255)); image.type = Image.Type.Sliced;
                    image.color = Color.white; image.raycastTarget = false;
                }
            pieces = new Piece[board.Pieces.Count];
            for (int layer = 0; layer < 2; layer++)
                foreach (var data in board.Pieces)
                {
                    if (data.Movable != (layer == 1)) continue;
                    var pieceRect = PlaceholderVisuals.Rect($"{data.Type} {data.Id}", transform, Vector2.zero, Vector2.one);
                    var piece = pieceRect.gameObject.AddComponent<Piece>();
                    piece.Initialize(data, board.Width, board.Height, font, circle, rounded, feel, onTap, art); pieces[data.Id] = piece;
                    if (sculptedTopology && data.Type == PieceType.Wall) pieceRect.gameObject.SetActive(false);
                }
            // Keep authored switch channels visible even under a box or normal piece.
            foreach (var data in board.Pieces)
            {
                if (data.Type != PieceType.Switch) continue;
                var marker = PlaceholderVisuals.Rect("Switch Channel", transform,
                    new Vector2((float)data.Position.x/board.Width,(float)data.Position.y/board.Height),
                    new Vector2((float)(data.Position.x+1)/board.Width,(float)(data.Position.y+1)/board.Height));
                var badge = VisualTheme.Surface("Channel Badge",marker,art.Surface(BoardSurface.Raised,new Color32(238,190,95,255)),Color.white,new Vector2(.05f,.76f),new Vector2(.59f,.985f));
                PieceAppearance.Channel(badge.transform,data.Channel,new Color32(68,47,22,255),new Vector2(.14f,.08f),new Vector2(.86f,.92f));
            }
        }
        public void SetInput(bool enabled) { foreach (var piece in pieces) piece.SetInput(enabled); }
        public void TapFeedback(int id, bool blocked)
        {
            pieces[id].Feedback(feel.tapPunch, blocked);
            audioManager.Play(blocked ? AudioCue.Blocked : AudioCue.Tap);
            if (!blocked) haptics.Light();
        }
        public void SuccessPulse() { successRemaining = feel.successDuration; }
        private void Update()
        {
            if (shakeRemaining > 0)
            {
                shakeRemaining = Mathf.Max(0, shakeRemaining - Time.unscaledDeltaTime);
                float t = shakeRemaining / Mathf.Max(.001f, feel.shakeDuration);
                rect.anchoredPosition = feel.reducedMotion ? Vector2.zero : new Vector2(Mathf.Sin(t * Mathf.PI * 4), Mathf.Sin(t * Mathf.PI * 2)) * (feel.shakePixels * t);
            }
            if (successRemaining > 0)
            {
                successRemaining = Mathf.Max(0, successRemaining - Time.unscaledDeltaTime);
                float t = successRemaining / Mathf.Max(.001f, feel.successDuration);
                rect.localScale = Vector3.one * (1 + (feel.reducedMotion ? 0 : feel.successScale * Mathf.Sin(t * Mathf.PI)));
            }
        }
        public IEnumerator Play(BoardManager board, int tappedId)
        {
            bool blocked = board.Actions.Count == 0;
            float anticipation = blocked ? feel.blockedDuration : feel.tapAnticipation;
            for (float elapsed = 0; elapsed < anticipation; elapsed += Time.unscaledDeltaTime) yield return null;
            for (int i = 0; i < board.Actions.Count; i++)
            {
                var action = board.Actions[i]; var piece = pieces[action.PieceId];
                if (action.Type == BoardActionType.Turn)
                {
                    piece.ShowDirection(action.Direction); piece.Feedback(feel.impactPunch);
                    bool rotated = false;
                    foreach (var tile in pieces)
                        if (i > 0 && board.Actions[i-1].Type == BoardActionType.Move && board.Actions[i-1].PieceId == action.PieceId && !tile.Data.Movable && tile.Data.Position == action.To && (tile.Data.Type == PieceType.Direction || tile.Data.Type == PieceType.Rotator))
                        { tile.Feedback(feel.tapPunch); rotated = tile.Data.Type == PieceType.Rotator; }
                    audioManager.Play(rotated ? AudioCue.Rotate : AudioCue.DirectionChange);
                }
                else if (action.Type == BoardActionType.SwitchActivated)
                { piece.Feedback(feel.tapPunch); audioManager.Play(AudioCue.SwitchActivate); haptics.Impact(); }
                else if (action.Type == BoardActionType.GateOpened || action.Type == BoardActionType.GateClosed)
                { piece.ShowGate(action.Type == BoardActionType.GateOpened); piece.Feedback(feel.impactPunch); audioManager.Play(action.Type == BoardActionType.GateOpened ? AudioCue.GateOpen : AudioCue.GateClose); }
                else if (action.Type == BoardActionType.Deliver)
                {
                    foreach (var tile in pieces)
                        if (tile.Data.Type == PieceType.Exit && tile.Data.Position == action.From) tile.Feedback(feel.tapPunch);
                    audioManager.Play(AudioCue.Exit); haptics.Exit(); shakeRemaining = feel.shakeDuration;
                    for (float elapsed = 0; elapsed < feel.exitDuration; elapsed += Time.unscaledDeltaTime)
                    { piece.SetExit(Mathf.Clamp01(elapsed / Mathf.Max(.001f, feel.exitDuration))); yield return null; }
                    piece.SetExit(1); piece.gameObject.SetActive(false);
                }
                else
                {
                    bool pushed = action.PieceId != tappedId;
                    audioManager.Play(pushed ? AudioCue.Push : AudioCue.Move);
                    if (pushed) haptics.Impact();
                    var from = new Vector2(action.From.x, action.From.y); var to = new Vector2(action.To.x, action.To.y);
                    for (float elapsed = 0; elapsed < feel.moveDuration; elapsed += Time.unscaledDeltaTime)
                    {
                        float t = Mathf.Clamp01(elapsed / Mathf.Max(.001f, feel.moveDuration));
                        piece.Place(Vector2.Lerp(from, to, Mathf.SmoothStep(0, 1, t)));
                        piece.MotionAccent(to - from, t); yield return null;
                    }
                    piece.Place(action.To); piece.MotionAccent(Vector2.zero, 0); piece.Feedback(feel.impactPunch);
                    if (pushed && board.ReactionDepth >= 3) shakeRemaining = feel.shakeDuration;
                }
                hud.ShowChain(i + 1);
            }
        }
    }
}
