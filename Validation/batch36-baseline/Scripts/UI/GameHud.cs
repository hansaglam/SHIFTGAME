using System;
using UnityEngine;
using UnityEngine.UI;

namespace Shift.Game
{
    public sealed class GameHud : MonoBehaviour
    {
        private Text moves, status, chain, restartLabel;
        private CanvasGroup chainGroup, resultGroup;
        private RectTransform resultPanel;
        private Image resultBackground;
        private RewardGleam rewardGleam;
        private GameFeelSettings feel;
        private string hint;
        private int previousMoves = -1;
        private float movePulse, chainFade = -1, resultPulse, chainPop;
        private GameObject nextButton;
        private RectTransform restartRect;
        private int reactionDepth;
        private float masteryRemaining;
        private string completionText;
        private Color completionColor;
        public bool MasteryVisible => masteryRemaining > 0;
        public ReactionTier CurrentReactionTier => ReactionPresentation.Classify(reactionDepth);
        public void Build(Font font, LevelData level, Sprite rounded, GameFeelSettings settings, Action restart, Sprite circle = null)
        {
            feel = settings; hint = level.Hint;
            Wordmark.Build(transform, font);
            var subtitle = PlaceholderVisuals.Label("Level Title", transform, font, level.DisplayTitle, 28, new Color32(231,245,251,255), new Vector2(.1f, .875f), new Vector2(.9f, .901f));
            if (level.Design?.difficultyBand == DifficultyBand.Finale)
            { subtitle.color = new Color32(255,233,177,255); subtitle.fontStyle = FontStyle.Bold; }
            var panel = VisualTheme.Surface("Goal Moves Panel", transform, rounded, new Color32(48,72,104,255), new Vector2(.13f,.805f), new Vector2(.87f,.868f));
            IdentityStyle.Material(panel);
            VisualTheme.Shadow(panel);
            var target = VisualTheme.Surface("Target Color", panel.transform, circle ?? rounded, PlaceholderVisuals.ColorFor(level.TargetColor), new Vector2(.035f,.14f), new Vector2(.15f,.86f));
            target.type = Image.Type.Simple; target.preserveAspect = true;
            target.rectTransform.anchorMin = target.rectTransform.anchorMax = new Vector2(.093f,.5f);
            target.rectTransform.sizeDelta = new Vector2(78,78); target.rectTransform.anchoredPosition = Vector2.zero;
            VisualTheme.Shadow(target, 3);
            var targetArt = panel.gameObject.AddComponent<BoardVisualResources>();
            target.sprite = targetArt.Surface(BoardSurface.Disc, PieceAppearance.PieceTint(level.TargetColor));
            target.color = Color.white;
            var goal = PlaceholderVisuals.Label("Goal", panel.transform, font, $"Clear {level.TargetColor}", 36,
                Color.white, new Vector2(.18f,.15f), new Vector2(.53f,.85f)); goal.fontStyle = FontStyle.Bold;
            VisualTheme.Surface("Divider", panel.transform, rounded, new Color32(113,139,163,255), new Vector2(.555f,.21f), new Vector2(.558f,.79f));
            PlaceholderVisuals.Label("Moves Label",panel.transform,font,"Moves:",32,Color.white,new Vector2(.59f,.16f),new Vector2(.80f,.84f));
            moves = PlaceholderVisuals.Label("Moves", panel.transform, font, "", 58, new Color32(255,213,95,255), new Vector2(.81f,.08f), new Vector2(.96f,.92f));
            moves.fontStyle = FontStyle.Bold;
            chain = PlaceholderVisuals.Label("Chain", transform, font, "", 34, Color.white, new Vector2(.18f,.774f), new Vector2(.82f,.799f));
            chain.fontStyle = FontStyle.Bold;
            VisualTheme.Shadow(chain, 2);
            chainGroup = chain.gameObject.AddComponent<CanvasGroup>(); chainGroup.alpha = 0; chainGroup.blocksRaycasts = false;
            resultPanel = PlaceholderVisuals.Rect("Result Panel", transform, new Vector2(.12f, .155f), new Vector2(.88f, .244f));
            resultBackground = resultPanel.gameObject.AddComponent<Image>(); resultBackground.sprite = rounded;
            resultBackground.type = Image.Type.Sliced; resultBackground.raycastTarget = false;
            IdentityStyle.Material(resultBackground);
            VisualTheme.Shadow(resultBackground,4);
            resultGroup = resultPanel.gameObject.AddComponent<CanvasGroup>(); resultGroup.blocksRaycasts = false;
            status = PlaceholderVisuals.Label("Status", resultPanel, font, "", 30, VisualTheme.Ink, new Vector2(.065f, .12f), new Vector2(.935f, .88f));
            status.resizeTextMinSize = 26; status.horizontalOverflow = HorizontalWrapMode.Wrap;
            rewardGleam = resultPanel.gameObject.AddComponent<RewardGleam>(); rewardGleam.Initialize(feel);
            var button = ChapterSelect.CreateButton("Restart", font, rounded, transform, new Vector2(.32f,.077f), new Vector2(.68f,.133f), restart, out restartLabel);
            restartRect = (RectTransform)button.transform;
            button.GetComponent<PresentationMotion>().Settings = feel;
            restartLabel.text = "Restart"; restartLabel.fontStyle = FontStyle.Bold;
            IdentityStyle.Action(button, restartLabel, IdentitySymbol.Restart, true);
        }
        public void BuildNavigation(Font font, Sprite rounded, Action next, Action chapter, bool enabled)
        {
            if (!enabled) return;
            nextButton = ChapterSelect.CreateButton("Next Level", font, rounded, transform,
                new Vector2(.43f,.077f), new Vector2(.88f,.133f), next, out var nextLabel).gameObject;
            nextLabel.text = "Next Level"; nextLabel.fontStyle = FontStyle.Bold;
            IdentityStyle.Action(nextButton.GetComponent<Button>(), nextLabel, IdentitySymbol.Forward, true);
            nextButton.SetActive(false);
            var chapterButton = ChapterSelect.CreateButton("Open Chapter",font,rounded,transform,new Vector2(.36f,.020f),new Vector2(.64f,.068f),chapter,out var chapterLabel);
            IdentityStyle.Action(chapterButton, chapterLabel, IdentitySymbol.Levels, false);
            chapterLabel.text = "Levels"; chapterLabel.fontSize = 28;
            foreach (var motion in GetComponentsInChildren<PresentationMotion>(true)) motion.Settings = feel;
        }
        public void SetCompletion(bool won, bool final, bool canNext, int chapterNumber = 1)
        {
            if (nextButton != null) nextButton.SetActive(won && canNext);
            restartRect.GetComponent<Image>().color = won && canNext ? IdentityStyle.Navy : IdentityStyle.Teal;
            if (won && final) { status.text = chapterNumber == 1 ? "CHAPTER COMPLETE!\nChapter 2 is ready." : "CHAPTER 2 COMPLETE!\nSmall moves. Big reactions."; resultBackground.color = new Color32(255,234,171,255); }
            else if (won && canNext) status.text = "GOAL COMPLETE!\nReady for the next puzzle?";
            restartRect.anchorMin = won && canNext ? new Vector2(.12f,.077f) : new Vector2(.32f,.077f);
            restartRect.anchorMax = won && canNext ? new Vector2(.38f,.133f) : new Vector2(.68f,.133f);
            restartRect.offsetMin = restartRect.offsetMax = Vector2.zero;
        }
        public void ShowMastery(AttemptMetrics result)
        {
            if (result == null || !result.completed || !result.hasOptimal || !result.perfectShift) return;
            completionText = status.text; completionColor = resultBackground.color;
            string headline = completionText.Split('\n')[0];
            status.text = headline + "\nPERFECT SHIFT";
            resultBackground.color = new Color32(194, 239, 224, 255);
            resultPulse = feel.successDuration; masteryRemaining = 1.6f;
            rewardGleam.Reveal();
        }
        public void Refresh(BoardManager board)
        {
            if (previousMoves >= 0 && board.MovesRemaining < previousMoves) movePulse = feel.hudPulseDuration;
            previousMoves = board.MovesRemaining;
            moves.text = board.MovesRemaining.ToString();
            moves.color = board.MovesRemaining <= feel.lowMovesThreshold ? new Color32(255,167,132,255) : new Color32(255,213,95,255);
            bool won = board.State == GameState.Won, lost = board.State == GameState.Lost;
            status.text = board.State switch
            {
                GameState.Won => "GOAL COMPLETE!\nBeautiful chain. Play again?",
                GameState.Lost => "OUT OF MOVES\nOne more try?",
                GameState.Resolving => board.Actions.Count == 0 ? "Blocked · no move used" : "CHAIN REACTION",
                _ => board.LastReactionWasCancelled ? "Loop cancelled · no move used" : string.IsNullOrEmpty(hint) ? "Tap a circle. Push pieces into an exit." : hint
            };
            restartLabel.text = won ? "Replay" : lost ? "Retry" : "Restart";
            resultBackground.color = won ? new Color32(215, 244, 225, 255) : lost ? new Color32(250, 222, 211, 255) : IdentityStyle.Cream;
            status.fontStyle = won || lost ? FontStyle.Bold : FontStyle.Normal;
            resultGroup.alpha = 1;
            if (won || lost) resultPulse = feel.successDuration;
            if (won) rewardGleam.Reveal();
        }
        public void BeginReaction()
        {
            reactionDepth = 0;
            chainGroup.alpha = 0; chainFade = -1; chain.text = string.Empty;
            chain.transform.localScale = Vector3.one;
        }
        public void ShowChain(int depth)
        {
            reactionDepth = depth;
            if (depth < 2) return;
            if (depth == 2) chainPop = feel.hudPulseDuration;
            chain.text = $"CHAIN x{depth}"; chainGroup.alpha = 1; chainFade = -1;
        }
        public void FinishChain()
        {
            if (chainGroup.alpha <= 0) return;
            chain.text = ReactionPresentation.Label(CurrentReactionTier) + $" x{reactionDepth}";
            chain.color = CurrentReactionTier == ReactionTier.MegaShift ? new Color32(255,224,143,255) : Color.white;
            chainPop = feel.hudPulseDuration; chainFade = feel.chainFadeDuration;
        }
        private void Update()
        {
            if (masteryRemaining > 0)
            {
                masteryRemaining = Mathf.Max(0, masteryRemaining - Time.unscaledDeltaTime);
                if (masteryRemaining == 0) { status.text = completionText; resultBackground.color = completionColor; }
            }
            if (chainPop > 0)
            {
                chainPop = Mathf.Max(0, chainPop - Time.unscaledDeltaTime);
                float t = chainPop / Mathf.Max(.001f, feel.hudPulseDuration);
                chain.transform.localScale = Vector3.one * (feel.reducedMotion ? 1 : 1 + feel.hudPunch * t * t);
            }
            if (movePulse > 0)
            {
                movePulse = Mathf.Max(0, movePulse - Time.unscaledDeltaTime);
                float t = movePulse / Mathf.Max(.001f, feel.hudPulseDuration);
                moves.transform.localScale = Vector3.one * (1 + (feel.reducedMotion ? 0 : feel.hudPunch * Mathf.Sin(t * Mathf.PI)));
            }
            if (chainFade >= 0)
            {
                chainFade = Mathf.Max(0, chainFade - Time.unscaledDeltaTime);
                float t = chainFade / Mathf.Max(.001f, feel.chainFadeDuration);
                chainGroup.alpha = t;
                chain.transform.localScale = Vector3.one * (1 + (feel.reducedMotion ? 0 : feel.hudPunch * t));
                if (chainFade == 0) chainFade = -1;
            }
            if (resultPulse > 0)
            {
                resultPulse = Mathf.Max(0, resultPulse - Time.unscaledDeltaTime);
                float t = resultPulse / Mathf.Max(.001f, feel.successDuration);
                resultPanel.localScale = Vector3.one * (1 + (feel.reducedMotion ? 0 : feel.successScale * Mathf.Sin(t * Mathf.PI)));
                resultGroup.alpha = 1 - t * .25f;
            }
        }
    }
}
