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
            Color accent;
            if (ColorUtility.TryParseHtmlString(chapter.paletteEnd, out accent))
                title.color = accent;
            LocalizedLabel.Bind(title, () =>
                GameLanguageService.Shared.CurrentLanguage == GameLanguage.Turkish
                    ? beat.nameTr : beat.nameEn);
            LocalizedLabel.Bind(detail, () =>
                GameLanguageService.Shared.CurrentLanguage == GameLanguage.Turkish
                    ? beat.worldTr : beat.worldEn);
            if (active != null) StopCoroutine(active);
            gameObject.SetActive(true);
            active = StartCoroutine(ShowRoutine());
            return true;
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
            yield return new WaitForSecondsRealtime(1.6f);
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
            if (group != null) group.alpha = 0f;
            gameObject.SetActive(false);
        }
    }
}
