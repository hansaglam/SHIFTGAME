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
        private RectTransform signalRect;
        private Image signalImage;
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
            // Bond curves are drawn after terrain and before movable pieces.
            // The link follows actual animated anchors; it never handles taps.
            int belowMovable = 0;
            foreach (var piece in board.Pieces)
                if (!piece.Movable) belowMovable++;
            // Three frame layers precede visible cells. Sculpted topology
            // skips wall cells, so compute the actual cell count.
            int insertionIndex = 3 + belowMovable;
            for (int y = 0; y < board.Height; y++)
                for (int x = 0; x < board.Width; x++)
                    if (!sculptedTopology ||
                        board.GetTerrain(new GridPosition(x, y))?.Type != PieceType.Wall)
                        insertionIndex++;
            foreach (var data in board.Pieces)
            {
                if (data.Type != PieceType.Normal || data.TetherId == 0)
                    continue;
                var partner = board.GetTetherPartner(data.Id);
                if (partner == null || partner.Id < data.Id)
                    continue;
                var linkRect = PlaceholderVisuals.Rect(
                    "Tether Bond " + data.TetherId, transform,
                    Vector2.zero, Vector2.one);
                var link = linkRect.gameObject.AddComponent<TetherLinkGraphic>();
                link.Bind((RectTransform)pieces[data.Id].transform,
                    (RectTransform)pieces[partner.Id].transform,
                    board.Width, board.Height, rounded);
                // The Canvas may not have calculated its size during Build;
                // the link computes its final geometry in LateUpdate.
                linkRect.SetSiblingIndex(Mathf.Min(insertionIndex,
                    transform.childCount - 1));
                insertionIndex++;
            }
            // Non-interactive channel badges remain readable above occupied switches AND gates.
            foreach (var data in board.Pieces)
            {
                if (data.Type != PieceType.Switch && data.Type != PieceType.Gate && data.Type != PieceType.Resonance) continue;
                var marker = PlaceholderVisuals.Rect(data.Type == PieceType.Gate ? "Gate Channel" : data.Type == PieceType.Resonance ? "Resonance Channel" : "Switch Channel", transform,
                    new Vector2((float)data.Position.x/board.Width,(float)data.Position.y/board.Height),
                    new Vector2((float)(data.Position.x+1)/board.Width,(float)(data.Position.y+1)/board.Height));
                bool resonant = data.Type == PieceType.Resonance;
                var badge = VisualTheme.Surface("Channel Badge",marker,
                    art.Surface(BoardSurface.Raised,resonant ? new Color32(92,225,228,255)
                        : new Color32(238,190,95,255)),Color.white,
                    resonant ? new Vector2(.44f,.77f) : data.Type == PieceType.Gate ? new Vector2(.16f,.76f) : new Vector2(.05f,.76f),
                    resonant ? new Vector2(.94f,.99f) : data.Type == PieceType.Gate ? new Vector2(.84f,.985f) : new Vector2(.59f,.985f));
                PieceAppearance.Channel(badge.transform,data.Channel,new Color32(25,42,62,255),new Vector2(.14f,.08f),new Vector2(.86f,.92f));
                var highlight=VisualTheme.Surface("Channel Feedback",badge.transform,rounded,Color.clear,Vector2.zero,Vector2.one);
                pieces[data.Id].BindChannelFeedback(highlight);
            }
            // One reusable overlay: no per-reaction GameObjects or full-board
            // animations. Raycast disabled so taps still reach the pieces.
            signalRect = PlaceholderVisuals.Rect("Resonance Signal Link", transform,
                Vector2.zero, Vector2.zero);
            signalRect.anchorMin = signalRect.anchorMax = Vector2.zero;
            signalRect.pivot = new Vector2(.5f, .5f);
            signalRect.sizeDelta = Vector2.zero;
            signalImage = signalRect.gameObject.AddComponent<Image>();
            signalImage.sprite = rounded;
            signalImage.raycastTarget = false;
            signalImage.color = Color.clear;
            signalRect.gameObject.SetActive(false);
        }
        public void SetInput(bool enabled) { foreach (var piece in pieces) piece.SetInput(enabled); }
        public void RestoreFromModel()
        {
            ResetSignal();
            shakeRemaining = successRemaining = 0; rect.anchoredPosition = Vector2.zero; rect.localScale = Vector3.one;
            foreach (var piece in pieces)
                if (piece.Data.Type != PieceType.Wall) piece.RestoreFromModel();
        }
        public void TapFeedback(int id, bool blocked, int depth = 0)
        {
            pieces[id].Feedback(feel.tapPunch, blocked);
            audioManager.Play(blocked ? AudioCue.Blocked : AudioCue.Tap);
            haptics.BeginReaction(blocked, depth);
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
        private void ResetSignal()
        {
            if (signalRect != null) signalRect.gameObject.SetActive(false);
            if (signalImage != null) signalImage.color = Color.clear;
        }

        private IEnumerator PlayResonanceSignal(BoardAction action, BoardManager board)
        {
            if (signalRect == null || signalImage == null) yield break;
            // Grid-to-canvas mapping remains correct at every portrait size,
            // including non-square boards and UI scale changes.
            var width = rect.rect.width;
            var height = rect.rect.height;
            var a = new Vector2((action.From.x + .5f) * width / board.Width,
                (action.From.y + .5f) * height / board.Height);
            var b = new Vector2((action.To.x + .5f) * width / board.Width,
                (action.To.y + .5f) * height / board.Height);
            var vector = b - a;
            var length = vector.magnitude;
            if (length < 1f) yield break;
            signalRect.localRotation = Quaternion.Euler(0f, 0f,
                Mathf.Atan2(vector.y, vector.x) * Mathf.Rad2Deg);
            var thickness = Mathf.Clamp(Mathf.Min(width / board.Width,
                height / board.Height) * .055f, 3f, 7f);
            var cyan = new Color(.46f, .94f, .96f, 1f);
            signalRect.gameObject.SetActive(true);
            if (feel.reducedMotion)
            {
                // Brief static connection: no translating or scaling animation.
                signalRect.anchoredPosition = (a + b) * .5f;
                signalRect.sizeDelta = new Vector2(length, thickness);
                signalImage.color = cyan;
                for (float hold = 0f; hold < .09f; hold += Time.unscaledDeltaTime)
                    yield return null;
            }
            else
            {
                const float duration = .17f;
                for (float elapsed = 0f; elapsed < duration; elapsed += Time.unscaledDeltaTime)
                {
                    float t = Mathf.Clamp01(elapsed / duration);
                    float spread = Mathf.SmoothStep(0f, 1f, t);
                    var tip = Vector2.Lerp(a, b, spread);
                    signalRect.anchoredPosition = (a + tip) * .5f;
                    signalRect.sizeDelta = new Vector2(length * spread, thickness);
                    cyan.a = .95f * (1f - .25f * t);
                    signalImage.color = cyan;
                    yield return null;
                }
            }
            ResetSignal();
        }

        public IEnumerator Play(BoardManager board, int tappedId, int lastDelivery = -1)
        {
            bool completed = false;
            try
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
                    audioManager.Reaction(rotated ? AudioCue.Rotate : AudioCue.DirectionChange, i + 1);
                }
                else if (action.Type == BoardActionType.ResonancePulse)
                {
                    piece.Feedback(feel.tapPunch);
                    foreach (var node in pieces)
                        if (node.Data.Type == PieceType.Resonance && node.Data.Position == action.To)
                            node.Feedback(feel.tapPunch);
                    audioManager.Reaction(AudioCue.SwitchActivate, i + 1);
                    haptics.Interaction();
                    yield return PlayResonanceSignal(action, board);
                }
                else if (action.Type == BoardActionType.TetherPull)
                {
                    // Bond tension is a real physical dependency, not a teleport.
                    // The following Move actions animate the partner and source.
                    piece.Feedback(feel.tapPunch);
                    audioManager.Reaction(AudioCue.Push, i + 1);
                    if (!feel.reducedMotion) haptics.Interaction();
                }
                else if (action.Type == BoardActionType.SwitchActivated)
                { piece.Feedback(feel.tapPunch); audioManager.Reaction(AudioCue.SwitchActivate, i + 1); haptics.Interaction(); }
                else if (action.Type == BoardActionType.GateOpened || action.Type == BoardActionType.GateClosed)
                { piece.ShowGate(action.Type == BoardActionType.GateOpened); piece.Feedback(feel.impactPunch); audioManager.Reaction(action.Type == BoardActionType.GateOpened ? AudioCue.GateOpen : AudioCue.GateClose, i + 1); haptics.Interaction(); }
                else if (action.Type == BoardActionType.Deliver)
                {
                    foreach (var tile in pieces)
                        if (tile.Data.Type == PieceType.Exit && tile.Data.Position == action.From) tile.Feedback(feel.tapPunch);
                    audioManager.Reaction(i == lastDelivery ? AudioCue.FinalExit : AudioCue.Exit, i + 1); haptics.Interaction(); shakeRemaining = feel.shakeDuration;
                    for (float elapsed = 0; elapsed < feel.exitDuration; elapsed += Time.unscaledDeltaTime)
                    { piece.SetExit(Mathf.Clamp01(elapsed / Mathf.Max(.001f, feel.exitDuration))); yield return null; }
                    piece.SetExit(1); piece.gameObject.SetActive(false);
                }
                else
                {
                    bool pushed = action.PieceId != tappedId;
                    audioManager.Reaction(pushed ? AudioCue.Push : AudioCue.Move, i + 1);
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
                hud.ShowChain(i + 1); haptics.Step(i + 1);
            }
            completed = true;
            }
            finally
            {
                // A disposed/interrupted playback must not retain an intermediate gate state.
                if (this != null) ResetSignal();
                if (!completed && this != null && pieces != null) RestoreFromModel();
            }
        }
    }
}
