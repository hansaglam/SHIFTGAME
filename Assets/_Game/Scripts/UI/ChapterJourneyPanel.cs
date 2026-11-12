using UnityEngine;
using UnityEngine.UI;

namespace Shift.Game
{
    // Lightweight Chapter 3/4 restoration track. Reads LevelProgression only.
    // Nothing is saved here; the UI is recreated safely after a scene rebuild.
    public sealed class ChapterJourneyPanel : MonoBehaviour
    {
        private readonly Image[] nodes = new Image[5];
        private readonly Text[] numbers = new Text[5];
        private Image fill;
        private Text status;
        private LevelProgression progressState;
        private int chapterIndex = -1;
        private bool built;

        public void Build(Font font, Sprite rounded)
        {
            if (built) return;
            built = true;
            var back = gameObject.AddComponent<Image>();
            back.color = new Color(.09f, .19f, .26f, .08f);
            back.raycastTarget = false;
            var bar = PlaceholderVisuals.Rect("Journey Track Line", transform,
                new Vector2(.07f, .13f), new Vector2(.93f, .23f));
            var muted = bar.gameObject.AddComponent<Image>();
            muted.color = new Color(.31f, .40f, .45f, .45f);
            muted.raycastTarget = false;
            var completeBar = PlaceholderVisuals.Rect("Journey Restored Line", transform,
                new Vector2(.07f, .13f), new Vector2(.07f, .23f));
            fill = completeBar.gameObject.AddComponent<Image>();
            fill.color = new Color32(73, 163, 147, 255);
            fill.raycastTarget = false;

            for (int i = 0; i < nodes.Length; i++)
            {
                float x = .07f + .86f * i / (nodes.Length - 1f);
                var node = PlaceholderVisuals.Rect("Journey Beat " + i, transform,
                    new Vector2(x - .028f, .08f), new Vector2(x + .028f, .30f));
                nodes[i] = node.gameObject.AddComponent<Image>();
                nodes[i].sprite = rounded;
                nodes[i].type = Image.Type.Sliced;
                nodes[i].color = new Color32(114, 132, 141, 255);
                nodes[i].raycastTarget = false;
                numbers[i] = PlaceholderVisuals.Label("Journey Beat Number",
                    node, font, "", 18, Color.white,
                    new Vector2(0f, 0f), new Vector2(1f, 1f));
                numbers[i].raycastTarget = false;
            }

            status = PlaceholderVisuals.Label("Journey Milestone Status", transform,
                font, "", 22, VisualTheme.Ink,
                new Vector2(.02f, .38f), new Vector2(.98f, .98f));
            status.alignment = TextAnchor.MiddleCenter;
            status.resizeTextForBestFit = true;
            status.resizeTextMinSize = 16;
            status.resizeTextMaxSize = 22;
            status.raycastTarget = false;
            GameLanguageService.Shared.LanguageChanged += OnLanguageChanged;
            gameObject.SetActive(false);
        }

        public void Show(LevelProgression progress, int chapter)
        {
            progressState = progress;
            chapterIndex = chapter;
            var definition = ChapterJourneyCatalog.ForChapter(chapter);
            if (!built || !ChapterJourneyCatalog.Available(progress, chapter) ||
                definition == null || definition.beats.Length != nodes.Length)
            {
                gameObject.SetActive(false);
                return;
            }
            if (!gameObject.activeSelf) gameObject.SetActive(true);
            bool turkish = GameLanguageService.Shared.CurrentLanguage == GameLanguage.Turkish;
            int completed = ChapterJourneyCatalog.Completed(definition,
                progress.HighestCompleted);
            var range = fill.rectTransform;
            var right = range.anchorMax;
            right.x = .07f + .86f * completed / 20f;
            range.anchorMax = right;
            Color restored;
            if (!ColorUtility.TryParseHtmlString(definition.paletteEnd, out restored))
                restored = new Color32(73, 163, 147, 255);
            fill.color = restored;

            int last = ChapterJourneyCatalog.LastRestoredBeat(definition,
                progress.HighestCompleted);
            var current = definition.beats[Mathf.Max(0, last)];
            string milestone = completed == 20
                ? (turkish ? definition.finaleTr : definition.finaleEn)
                : last < 0
                    ? (turkish ? definition.purposeTr : definition.purposeEn)
                    : (turkish ? current.nameTr : current.nameEn);
            status.text = completed + "/20 · " + milestone;
            for (int i = 0; i < nodes.Length; i++)
            {
                var beat = definition.beats[i];
                bool finished = ChapterJourneyCatalog.Restored(beat,
                    progress.HighestCompleted);
                nodes[i].color = finished ? restored : new Color32(114, 132, 141, 255);
                numbers[i].text = finished ? "✓" : beat.level.ToString();
            }
        }

        private void OnLanguageChanged()
        {
            if (progressState != null && chapterIndex >= 2)
                Show(progressState, chapterIndex);
        }

        private void OnDestroy()
        {
            GameLanguageService.Shared.LanguageChanged -= OnLanguageChanged;
        }
    }
}
