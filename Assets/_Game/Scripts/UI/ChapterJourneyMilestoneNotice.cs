using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Shift.Game
{
    // Optional one-shot, non-blocking restoration cue for draft chapters.
    // Uses the existing saved completion frontier; grants no rewards or unlocks.
    public sealed class ChapterJourneyMilestoneNotice : MonoBehaviour
    {
        private CanvasGroup group;
        private Image background;
        private Text title;
        private Text detail;
        private GameFeelSettings settings;
        private Coroutine active;
        private ChapterJourneyCatalog.Beat beat;
        private ChapterJourneyCatalog.Chapter chapter;
        private Image leftConvergence, rightConvergence;
        private bool terminalConvergence;

        // Special finale feedback is reserved for a recorded first win of 100.
        // It provides no reward, unlock, input gate or extra player action.
        public static bool IsFinalConvergence(ChapterJourneyCatalog.Beat milestone)
        {
            return milestone != null && milestone.level == 100;
        }

        public bool FinalConvergenceVisible =>
            gameObject.activeSelf && terminalConvergence;

        public static ChapterJourneyCatalog.Beat NewMilestone(
            LevelProgression progress, int completedLevelIndex, int previousHighestCompleted)
        {
            if (progress == null || completedLevelIndex < 0 ||
                completedLevelIndex <= previousHighestCompleted ||
                progress.HighestCompleted != completedLevelIndex)
                return null;
            var world = ChapterJourneyCatalog.ForLevel(completedLevelIndex + 1);
            if (world == null || progress.LevelCount < world.endLevel)
                return null;
            foreach (var stage in world.beats)
                if (stage.level == completedLevelIndex + 1)
                    return stage;
            return null;
        }

        public void Build(Font font, Sprite rounded, GameFeelSettings feel)
        {
            settings = feel;
            background = gameObject.AddComponent<Image>();
            background.sprite = rounded;
            background.type = Image.Type.Sliced;
            background.color = new Color32(244, 239, 218, 248);
            background.raycastTarget = false;
            group = gameObject.AddComponent<CanvasGroup>();
            group.blocksRaycasts = false;
            group.interactable = false;
            group.alpha = 0f;
            title = PlaceholderVisuals.Label("Journey Milestone Title", transform,
                font, "", 30, VisualTheme.Ink,
                new Vector2(.04f, .55f), new Vector2(.96f, .94f));
            title.fontStyle = FontStyle.Bold;
            detail = PlaceholderVisuals.Label("Journey Milestone Detail", transform,
                font, "", 21, VisualTheme.Muted,
                new Vector2(.04f, .04f), new Vector2(.96f, .56f));
            title.raycastTarget = detail.raycastTarget = false;

            var left = PlaceholderVisuals.Rect("Confluence Left Stream",
                transform, new Vector2(.08f, .945f), new Vector2(.08f, .965f));
            var right = PlaceholderVisuals.Rect("Confluence Right Stream",
                transform, new Vector2(.92f, .945f), new Vector2(.92f, .965f));
            leftConvergence = left.gameObject.AddComponent<Image>();
            rightConvergence = right.gameObject.AddComponent<Image>();
            leftConvergence.raycastTarget = false;
            rightConvergence.raycastTarget = false;
            leftConvergence.gameObject.SetActive(false);
            rightConvergence.gameObject.SetActive(false);
            gameObject.SetActive(false);
        }

        public bool ShowNew(LevelProgression progress,
            int completedLevelIndex, int previousHighestCompleted)
        {
            var newBeat = NewMilestone(progress, completedLevelIndex,
                previousHighestCompleted);
            if (newBeat == null || group == null) return false;

            beat = newBeat;
            chapter = ChapterJourneyCatalog.ForLevel(completedLevelIndex + 1);
            terminalConvergence = IsFinalConvergence(beat) &&
                chapter != null && chapter.endLevel == 100;
            leftConvergence.gameObject.SetActive(terminalConvergence);
            rightConvergence.gameObject.SetActive(terminalConvergence);
            SetConvergence(0f);
            Color accent;
            if (ColorUtility.TryParseHtmlString(chapter.paletteEnd, out accent))
            {
                title.color = accent;
                leftConvergence.color = accent;
                rightConvergence.color = accent;
            }
            LocalizedLabel.Bind(title, () =>
                GameLanguageService.Shared.CurrentLanguage == GameLanguage.Turkish
                    ? beat.nameTr : beat.nameEn);
            LocalizedLabel.Bind(detail, () =>
                GameLanguageService.Shared.CurrentLanguage == GameLanguage.Turkish
                    ? (terminalConvergence ? chapter.finaleTr : beat.worldTr)
                    : (terminalConvergence ? chapter.finaleEn : beat.worldEn));
            if (active != null) StopCoroutine(active);
            gameObject.SetActive(true);
            active = StartCoroutine(ShowRoutine());
            return true;
        }

        private void SetConvergence(float t)
        {
            if (leftConvergence == null || rightConvergence == null) return;
            t = Mathf.Clamp01(t);
            var leftRect = leftConvergence.rectTransform;
            var rightRect = rightConvergence.rectTransform;
            leftRect.anchorMax = new Vector2(.08f + .40f * t, .965f);
            rightRect.anchorMin = new Vector2(.92f - .40f * t, .945f);
        }

        private IEnumerator ShowRoutine()
        {
            bool still = settings != null && settings.reducedMotion;
            if (!still)
            {
                for (float t = 0; t < .16f; t += Time.unscaledDeltaTime)
                {
                    group.alpha = Mathf.Clamp01(t / .16f);
                    yield return null;
                }
            }
            group.alpha = 1f;
            if (terminalConvergence)
            {
                if (still) SetConvergence(1f);
                else
                {
                    // Two quiet paths close toward the center only once,
                    // after a real newly recorded Level 100 win.
                    for (float elapsed = 0f; elapsed < .42f;
                        elapsed += Time.unscaledDeltaTime)
                    {
                        SetConvergence(Mathf.SmoothStep(0f, 1f,
                            Mathf.Clamp01(elapsed / .42f)));
                        yield return null;
                    }
                    SetConvergence(1f);
                }
            }
            yield return new WaitForSecondsRealtime(terminalConvergence ?
                2.1f : 1.6f);
            if (!still)
            {
                for (float t = 0; t < .22f; t += Time.unscaledDeltaTime)
                {
                    group.alpha = 1f - Mathf.Clamp01(t / .22f);
                    yield return null;
                }
            }
            group.alpha = 0f;
            gameObject.SetActive(false);
            active = null;
        }

        public void Dismiss()
        {
            if (active != null) StopCoroutine(active);
            active = null;
            terminalConvergence = false;
            if (group != null) group.alpha = 0f;
            gameObject.SetActive(false);
        }
    }
}
