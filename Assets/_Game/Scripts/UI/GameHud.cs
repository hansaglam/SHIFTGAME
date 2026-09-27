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
        private Func<string> hint;
        private HintLanguage Language => GameLanguageService.Shared.HintLanguage;
        private int previousMoves = -1;
        private float movePulse, chainFade = -1, resultPulse, chainPop;
        private GameObject nextButton;
        private Button undoButton, hintButton;
        private Text hintBadge, undoBadge;
        private bool completionLayout;
        public void BuildUtilities(Font font, Sprite rounded, Action undo, Action openHint)
        {
            undoButton = ChapterSelect.CreateButton("Undo", font, rounded, transform, new Vector2(.13f,.077f), new Vector2(.35f,.133f), undo, out var undoLabel);
            LocalizedLabel.Bind(undoLabel, "controls.undo"); IdentityStyle.Action(undoButton, undoLabel, IdentitySymbol.Undo, false);
            hintButton = ChapterSelect.CreateButton("Hint", font, rounded, transform, new Vector2(.65f,.077f), new Vector2(.87f,.133f), openHint, out var hintLabel);
            LocalizedLabel.Bind(hintLabel, "controls.hint"); IdentityStyle.Action(hintButton, hintLabel, IdentitySymbol.Hint, false);
            hintButton.GetComponent<Image>().color = new Color32(198,143,37,255);
            var badge = VisualTheme.Surface("Hint Badge", hintButton.transform, rounded, new Color32(255,216,108,255), new Vector2(.73f,.73f), new Vector2(1.09f,1.09f));
            IdentityStyle.Material(badge, true); badge.raycastTarget = false;
            hintBadge = PlaceholderVisuals.Label("Hint Count", badge.transform, font, "", 28, VisualTheme.Ink, Vector2.zero, Vector2.one);
            hintBadge.raycastTarget = false;
            var undoCounter = VisualTheme.Surface("Undo Badge", undoButton.transform, rounded, new Color32(116,190,200,255), new Vector2(.73f,.73f), new Vector2(1.09f,1.09f));
            IdentityStyle.Material(undoCounter, true); undoCounter.raycastTarget = false;
            undoBadge = PlaceholderVisuals.Label("Undo Count", undoCounter.transform, font, "", 28, VisualTheme.Ink, Vector2.zero, Vector2.one);
            undoBadge.raycastTarget = false;
            RoundAction(undoButton, undoLabel, .25f);
            RoundAction(hintButton, hintLabel, .75f);
            var levels = transform.Find("Open Chapter") as RectTransform;
            if (levels != null) { levels.anchorMin = new Vector2(.40f,.017f); levels.anchorMax = new Vector2(.60f,.055f); }
            undoLabel.fontSize = hintLabel.fontSize = restartLabel.fontSize = 26;
            foreach (var motion in GetComponentsInChildren<PresentationMotion>(true)) motion.Settings = feel;
            SetCompletion(false, false, false);
        }
        private static void RoundAction(Button button, Text label, float x)
        {
            button.targetGraphic.raycastPadding = Vector4.one * -22;
            var rect = (RectTransform)button.transform;
            rect.anchorMin = new Vector2(x,.08175f); rect.anchorMax = new Vector2(x,.14425f); rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(120,0);
            var aspect = button.GetComponent<AspectRatioFitter>();
            if (aspect == null) aspect = button.gameObject.AddComponent<AspectRatioFitter>();
            aspect.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth; aspect.aspectRatio = 1; aspect.enabled = true;
            if (button.GetComponent<Image>().type != Image.Type.Simple) IdentityStyle.Material(button.GetComponent<Image>(), true);
            label.rectTransform.anchorMin = new Vector2(-.25f,-.31f); label.rectTransform.anchorMax = new Vector2(1.25f,-.03f);
            var icon = button.transform.Find("Action Icon") as RectTransform;
            icon.anchorMin = new Vector2(.25f,.25f); icon.anchorMax = new Vector2(.75f,.75f);
        }
        public void RefreshUtilities(BoardManager board, int hints, bool modal, int undos = 0)
        {
            if (undoButton == null) return;
            bool won = board.State == GameState.Won;
            undoButton.gameObject.SetActive(!won); hintButton.gameObject.SetActive(!won);
            SetAvailable(undoButton, board.CanUndo && !modal);
            SetAvailable(hintButton, board.State == GameState.Playing && !modal);
            hintBadge.text = hints.ToString();
            undoBadge.text = undos.ToString();
        }
        public void NoMoreHints(HintLanguage language)
        { hint = () => GameLanguageService.Shared.Text("hint.exhausted"); LocalizedLabel.Bind(status, hint); }
        private static void SetAvailable(Button button, bool available)
        {
            button.interactable = available; button.targetGraphic.raycastTarget = available;
            var group = button.GetComponent<CanvasGroup>();
            if (group == null) group = button.gameObject.AddComponent<CanvasGroup>();
            group.alpha = available ? 1 : .42f; group.blocksRaycasts = available;
        }
        public void ShowHint(string text, int stage, LevelData level = null)
        {
            hint = () => GameLanguageService.Shared.Text("hint.stage", stage, level == null ? text : HintCatalog.Get(level, stage, Language)); LocalizedLabel.Bind(status, hint); resultPulse = feel.hudPulseDuration;
        }
        private RectTransform restartRect;
        private int reactionDepth;
        private float masteryRemaining;
        private Func<string> completionText;
        private Color completionColor;
        public bool MasteryVisible => masteryRemaining > 0;
        public ReactionTier CurrentReactionTier => ReactionPresentation.Classify(reactionDepth);
        public void Build(Font font, LevelData level, Sprite rounded, GameFeelSettings settings, Action restart, Sprite circle = null)
        {
            feel = settings; hint = () => LocalizationCatalog.Instruction(level);
            Wordmark.Build(transform, font);
            var subtitle = PlaceholderVisuals.Label("Level Title", transform, font, level.DisplayTitle, 28, new Color32(231,245,251,255), new Vector2(.1f, .875f), new Vector2(.9f, .901f));
            LocalizedLabel.Bind(subtitle, () => LocalizationCatalog.Title(level));
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
            var colors = level.ResolvedTargetColors;
            if (colors.Count > 1)
            {
                target.rectTransform.anchorMin = target.rectTransform.anchorMax = new Vector2(.06f,.5f);
                target.rectTransform.sizeDelta = new Vector2(42,42);
                target.sprite = targetArt.Surface(BoardSurface.Disc, PieceAppearance.PieceTint(colors[0]));
                for (int i = 1; i < colors.Count; i++)
                {
                    var disc = VisualTheme.Surface("Target Color " + i, panel.transform,
                        targetArt.Surface(BoardSurface.Disc, PieceAppearance.PieceTint(colors[i])), Color.white, Vector2.zero, Vector2.zero);
                    disc.type = Image.Type.Simple; disc.preserveAspect = true; disc.raycastTarget = false;
                    disc.rectTransform.anchorMin = disc.rectTransform.anchorMax = new Vector2(.06f + .059f * i,.5f);
                    disc.rectTransform.sizeDelta = new Vector2(42,42); disc.rectTransform.anchoredPosition = Vector2.zero;
                    VisualTheme.Shadow(disc, 3);
                }
                goal.rectTransform.anchorMin = new Vector2(.22f,.15f);
                goal.text = ObjectiveText.Format(colors); goal.fontSize = 30;
            }
            else if (colors[0] != level.TargetColor)
            {
                target.sprite = targetArt.Surface(BoardSurface.Disc, PieceAppearance.PieceTint(colors[0]));
                goal.text = ObjectiveText.Format(colors);
            }
            LocalizedLabel.Bind(goal, () => ObjectiveText.Format(colors));
            VisualTheme.Surface("Divider", panel.transform, rounded, new Color32(113,139,163,255), new Vector2(.555f,.21f), new Vector2(.558f,.79f));
            var movesLabel = PlaceholderVisuals.Label("Moves Label",panel.transform,font,"Moves:",32,Color.white,new Vector2(.59f,.16f),new Vector2(.80f,.84f));
            LocalizedLabel.Bind(movesLabel, "hud.moves");
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
            LocalizedLabel.Bind(restartLabel, "controls.restart"); restartLabel.fontStyle = FontStyle.Bold;
            IdentityStyle.Action(button, restartLabel, IdentitySymbol.Restart, true);
        }
        public void BuildNavigation(Font font, Sprite rounded, Action next, Action chapter, bool enabled)
        {
            if (!enabled) return;
            nextButton = ChapterSelect.CreateButton("Next Level", font, rounded, transform,
                new Vector2(.43f,.077f), new Vector2(.88f,.133f), next, out var nextLabel).gameObject;
            LocalizedLabel.Bind(nextLabel, "results.next"); nextLabel.fontStyle = FontStyle.Bold;
            IdentityStyle.Action(nextButton.GetComponent<Button>(), nextLabel, IdentitySymbol.Forward, true);
            nextButton.SetActive(false);
            var chapterButton = ChapterSelect.CreateButton("Open Chapter",font,rounded,transform,new Vector2(.36f,.020f),new Vector2(.64f,.068f),chapter,out var chapterLabel);
            IdentityStyle.Action(chapterButton, chapterLabel, IdentitySymbol.Levels, false);
            LocalizedLabel.Bind(chapterLabel, "controls.levels"); chapterLabel.fontSize = 28;
            foreach (var motion in GetComponentsInChildren<PresentationMotion>(true)) motion.Settings = feel;
        }
        public void SetCompletion(bool won, bool final, bool canNext, int chapterNumber = 1)
        {
            if (nextButton != null) nextButton.SetActive(won && canNext);
            if(backToDaily!=null)backToDaily.SetActive(false);
            if (undoButton != null && transform.Find("Open Chapter") is RectTransform levels)
            {
                levels.anchorMin = won ? new Vector2(.36f,.020f) : new Vector2(.40f,.017f);
                levels.anchorMax = won ? new Vector2(.64f,.068f) : new Vector2(.60f,.055f);
            }
            restartRect.GetComponent<Image>().color = won && canNext ? IdentityStyle.Navy : IdentityStyle.Teal;
            if (won && final) { LocalizedLabel.Bind(status, chapterNumber == 1 ? "results.chapter1" : "results.chapter2"); resultBackground.color = new Color32(255,234,171,255); }
            else if (won && canNext) LocalizedLabel.Bind(status, "results.ready");
            restartRect.anchorMin = won && canNext ? new Vector2(.12f,.077f) : new Vector2(.32f,.077f);
            restartRect.anchorMax = won && canNext ? new Vector2(.38f,.133f) : new Vector2(.68f,.133f);
            if (undoButton != null)
            {
                if (!won) { RoundAction(restartRect.GetComponent<Button>(), restartLabel, .5f); completionLayout = false; }
                else
                {
                    restartRect.GetComponent<Button>().targetGraphic.raycastPadding = Vector4.zero;
                    restartRect.GetComponent<AspectRatioFitter>().enabled = false;
                    if (!completionLayout) IdentityStyle.Material(restartRect.GetComponent<Image>());
                    completionLayout = true;
                    restartLabel.rectTransform.anchorMin = new Vector2(.25f,.12f); restartLabel.rectTransform.anchorMax = new Vector2(.92f,.88f);
                    var icon = restartRect.Find("Action Icon") as RectTransform;
                    icon.anchorMin = new Vector2(.08f,.27f); icon.anchorMax = new Vector2(.24f,.73f);
                    restartLabel.fontSize = 32;
                }
            }
            if (undoButton != null) { undoButton.gameObject.SetActive(!won); hintButton.gameObject.SetActive(!won); }
            if (won || undoButton == null) restartRect.offsetMin = restartRect.offsetMax = Vector2.zero;
        }
        public void ShowCampaignComplete(Font font, Sprite rounded, bool perfect, Action proceed)
        {
            // Final presentation supersedes the temporary Perfect/Chapter result, without changing it for replays.
            masteryRemaining = 0; resultPulse = 0; resultGroup.alpha = 1; resultPanel.localScale = Vector3.one;
            resultBackground.color = IdentityStyle.Cream;
            if (nextButton != null) nextButton.SetActive(false);
            status.fontSize = 27;
            status.rectTransform.anchorMin = new Vector2(.04f, perfect ? .25f : .07f);
            status.rectTransform.anchorMax = new Vector2(.96f,.94f);
            LocalizedLabel.Bind(status, () => GameLanguageService.Shared.Text("ending.complete") + "\n" + GameLanguageService.Shared.Text("ending.complete_body"));
            if (perfect)
            {
                var line = PlaceholderVisuals.Label("Perfect Final Shift",resultPanel,font,"",22,new Color32(139,100,24,255),new Vector2(.04f,.035f),new Vector2(.96f,.235f));
                LocalizedLabel.Bind(line,"ending.perfect_final");
            }
            restartRect.anchorMin = new Vector2(.12f,.077f); restartRect.anchorMax = new Vector2(.38f,.133f);
            restartRect.GetComponent<Image>().color = IdentityStyle.Navy;
            var button = ChapterSelect.CreateButton("Campaign Continue",font,rounded,transform,new Vector2(.43f,.077f),new Vector2(.88f,.133f),proceed,out var label);
            button.targetGraphic.color = IdentityStyle.Teal;button.GetComponent<PresentationMotion>().Settings=feel;
            LocalizedLabel.Bind(label,"ending.continue");label.fontStyle=FontStyle.Bold;
        }
        public void EndCampaignPresentation(ChapterMastery summary)
        {
            var continueControl = transform.Find("Campaign Continue");
            if (continueControl != null) continueControl.gameObject.SetActive(false);
            var perfectLine = resultPanel.Find("Perfect Final Shift");
            if (perfectLine != null) perfectLine.gameObject.SetActive(false);
            status.fontSize = 30; status.rectTransform.anchorMin = new Vector2(.065f,.12f); status.rectTransform.anchorMax = new Vector2(.935f,.88f);
            SetCompletion(true,true,false,2);ShowChapterMastery(summary);
        }
        private GameObject backToDaily;
        public void BuildDaily(Font font,Sprite rounded,DailyPuzzle puzzle,HintLanguage language,Action back)
        {
            LocalizedLabel.Bind(transform.Find("Level Title").GetComponent<Text>(), () => DailyText.Title(Language)+" · "+DailyText.Date(puzzle.Date,Language));
            backToDaily=ChapterSelect.CreateButton("Back to Daily",font,rounded,transform,new Vector2(.43f,.077f),new Vector2(.88f,.133f),back,out var label).gameObject;
            LocalizedLabel.Bind(label, () => DailyText.Back(Language));label.fontSize=30;backToDaily.GetComponent<Image>().color=IdentityStyle.Teal;
            backToDaily.GetComponent<PresentationMotion>().Settings=feel;backToDaily.SetActive(false);
        }
        public void ShowDailyCompletion(bool won,bool perfect,HintLanguage language)
        {
            SetCompletion(won,false,won);
            if(nextButton!=null)nextButton.SetActive(false);
            if(backToDaily!=null)backToDaily.SetActive(won);
            if(!won)return;
            masteryRemaining=0;LocalizedLabel.Bind(status, () => DailyText.Complete(perfect,Language));
            resultBackground.color=perfect?new Color32(255,234,171,255):new Color32(215,244,225,255);
        }
        public void ShowChapterMastery(ChapterMastery summary, HintLanguage language = HintLanguage.English)
        {
            LocalizedLabel.Bind(status, () => (summary.Mastered ? MasteryText.Mastered(Language) : GameLanguageService.Shared.Text(summary.Chapter == 0 ? "results.chapter_title1" : "results.chapter_title2")) + "\n" + MasteryText.Summary(summary, Language));
        }
        public void ShowMastery(AttemptMetrics result, bool firstPerfect = false, HintLanguage language = HintLanguage.English)
        {
            if (result == null || !result.completed || !result.hasOptimal || !result.perfectShift) return;
            completionText = LocalizedLabel.Recipe(status); completionColor = resultBackground.color;
            LocalizedLabel.Bind(status, () => completionText().Split('\n')[0] + "\n" + (firstPerfect ? MasteryText.FirstPerfect(Language) : GameLanguageService.Shared.Text("mastery.perfect")));
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
            string stateKey = board.State switch
            {
                GameState.Won => "results.goal",
                GameState.Lost => "results.lost",
                GameState.Resolving => board.Actions.Count == 0 ? "hud.blocked" : "hud.reacting",
                _ => board.LastReactionWasCancelled ? "hud.loop" : null
            };
            if (stateKey != null) LocalizedLabel.Bind(status, stateKey);
            else LocalizedLabel.Bind(status, hint ?? (() => GameLanguageService.Shared.Text("hud.instruction")));
            LocalizedLabel.Bind(restartLabel, won ? "results.replay" : lost ? "results.retry" : "controls.restart");
            resultBackground.color = won ? new Color32(215, 244, 225, 255) : lost ? new Color32(250, 222, 211, 255) : IdentityStyle.Cream;
            status.fontStyle = won || lost ? FontStyle.Bold : FontStyle.Normal;
            resultGroup.alpha = 1;
            if (won || lost) resultPulse = feel.successDuration;
            if (won) rewardGleam.Reveal();
        }
        public void BeginReaction()
        {
            reactionDepth = 0;
            chainGroup.alpha = 0; chainFade = -1; LocalizedLabel.Bind(chain, () => string.Empty);
            chain.transform.localScale = Vector3.one;
        }
        public void ShowChain(int depth)
        {
            reactionDepth = depth;
            if (depth < 2) return;
            if (depth == 2) chainPop = feel.hudPulseDuration;
            LocalizedLabel.Bind(chain, () => GameLanguageService.Shared.Text("chain.Chain") + $" x{depth}"); chainGroup.alpha = 1; chainFade = -1;
        }
        public void FinishChain()
        {
            if (chainGroup.alpha <= 0) return;
            LocalizedLabel.Bind(chain, () => GameLanguageService.Shared.Text("chain." + CurrentReactionTier) + $" x{reactionDepth}");
            chain.color = CurrentReactionTier == ReactionTier.MegaShift ? new Color32(255,224,143,255) : Color.white;
            chainPop = feel.hudPulseDuration; chainFade = feel.chainFadeDuration;
        }
        private void Update()
        {
            if (masteryRemaining > 0)
            {
                masteryRemaining = Mathf.Max(0, masteryRemaining - Time.unscaledDeltaTime);
                if (masteryRemaining == 0) { LocalizedLabel.Bind(status, completionText); resultBackground.color = completionColor; }
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
