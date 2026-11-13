using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Shift.Game
{
    public sealed class ChapterSelect : MonoBehaviour
    {
        private Button[] buttons;
        private Text[] labels;
        private Image[] backgrounds;
        private CanvasGroup group;
        private Text chapterTitle, masterySummary, masteryLegend, masteryReplay;
        private Text[] masteryMarkers;
        public HintLanguage Language { get => GameLanguageService.Shared.HintLanguage; set => GameLanguageService.Shared.Select(value == HintLanguage.Turkish ? GameLanguage.Turkish : GameLanguage.English); }
        public Action<ChapterMastery> MasteryViewed { get; set; }
        private Button secondTab;
        private Button[] chapterTabs;
        private ChapterJourneyPanel journey;
        private ChapterWorldBackdrop worldBackdrop;
        private bool expanded;
        // A 103-level campaign is an opt-in RÖLE pilot, NOT a sixth
        // complete 20-level chapter. All 40/80/100 layouts stay unchanged.
        private bool relayPreview;
        private Text continueLabel;
        private LevelProgression progressState;
        private int page;
        private bool open;
        public GameFeelSettings Settings { private get; set; } = new GameFeelSettings();
        public bool IsOpen => open;
        public void Build(Font font, Sprite rounded, int count, Action<int> select, Action close, Action playCurrent)
        {
            // The shipped 40-level UI layout is left exactly as it was.
            // New chapter navigation exists only when full draft chapters are wired.
            expanded = count >= 60;
            relayPreview = count == 103;
            var shade = gameObject.AddComponent<Image>(); shade.color = new Color(.055f,.13f,.23f,.88f);
            var content = PlaceholderVisuals.Rect("Chapter Content", transform, Vector2.zero, Vector2.one);
            group = content.gameObject.AddComponent<CanvasGroup>();
            var card = VisualTheme.Surface("Chapter Card", content, rounded, IdentityStyle.Cream, new Vector2(.025f,.04f), new Vector2(.975f,.95f));
            IdentityStyle.Material(card); VisualTheme.Shadow(card,8);
            if (expanded)
            {
                // Scenic, low-contrast illustration behind buttons and labels.
                // Decorative-only: never capture a tap on a puzzle or tab.
                var worldRect = PlaceholderVisuals.Rect(
                    "Chapter World Landscape", content,
                    new Vector2(.06f, .28f), new Vector2(.94f, .65f));
                worldBackdrop = worldRect.gameObject
                    .AddComponent<ChapterWorldBackdrop>();
                worldBackdrop.raycastTarget = false;
                worldBackdrop.gameObject.SetActive(false);
            }
            chapterTitle = PlaceholderVisuals.Label("Chapter Title", content, font, "CHAPTER 1", 64, VisualTheme.Ink, new Vector2(.08f,.83f),new Vector2(.92f,.91f));
            masterySummary = PlaceholderVisuals.Label("Chapter Hint", content, font, "", 30, VisualTheme.Ink,new Vector2(.05f,.79f),new Vector2(.95f,.83f));
            if (expanded)
            {
                // Reserve a clean, non-overlapping header lane for the world track.
                chapterTitle.rectTransform.anchorMin = new Vector2(.08f, .872f);
                chapterTitle.rectTransform.anchorMax = new Vector2(.92f, .936f);
                chapterTitle.resizeTextForBestFit = true;
                chapterTitle.resizeTextMinSize = 21;
                chapterTitle.resizeTextMaxSize = 38;
                masterySummary.rectTransform.anchorMin = new Vector2(.05f, .820f);
                masterySummary.rectTransform.anchorMax = new Vector2(.95f, .867f);
                masterySummary.resizeTextForBestFit = true;
                masterySummary.resizeTextMinSize = 16;
                masterySummary.resizeTextMaxSize = 23;
            }
            masteryLegend = PlaceholderVisuals.Label("Mastery Legend", content, font, "", 23, VisualTheme.Ink,new Vector2(.04f,.25f),new Vector2(.96f,.278f));
            masteryReplay = PlaceholderVisuals.Label("Mastery Replay", content, font, "", 23, VisualTheme.Ink,new Vector2(.05f,.225f),new Vector2(.95f,.25f));
            masteryMarkers = new Text[count];
            buttons = new Button[count]; labels = new Text[count]; backgrounds = new Image[count];
            for (int i = 0; i < count; i++)
            {
                int index = i, row = (i % CampaignChapters.LevelsPerChapter) / 5, col = i % 5;
                var min = new Vector2(.06f + col * .18f,
                    (expanded ? .57f - row * .095f : .605f - row * .105f));
                buttons[i] = CreateButton("Level " + (i + 1), font, rounded,
                    content, min, min + new Vector2(.16f, expanded ? .082f : .087f),
                    () => select(index), out labels[i]);
                backgrounds[i] = (Image)buttons[i].targetGraphic;
                masteryMarkers[i] = PlaceholderVisuals.Label("Mastery Marker", buttons[i].transform, font, "", 28, Color.white, new Vector2(.73f,.68f),new Vector2(.96f,.96f));
                masteryMarkers[i].raycastTarget = false;
            }
            if (!expanded)
            {
                var first = CreateButton("Chapter 1 Tab",font,rounded,content,
                    new Vector2(.12f,.70f),new Vector2(.48f,.745f),
                    () => ShowPage(0),out var tabOne);
                LocalizedLabel.Bind(tabOne, "chapter.title", 1);
                secondTab = CreateButton("Chapter 2 Tab",font,rounded,content,
                    new Vector2(.52f,.70f),new Vector2(.88f,.745f),
                    () => ShowPage(1),out var tabTwo);
                LocalizedLabel.Bind(tabTwo, "chapter.title", 2);
                chapterTabs = new[] { first, secondTab };
            }
            else
            {
                int tabCount = relayPreview
                    ? 6 : Mathf.Min(5,
                        count / CampaignChapters.LevelsPerChapter);
                chapterTabs = new Button[tabCount];
                float gap = .015f, availableWidth = .88f;
                float width = (availableWidth - gap * (tabCount - 1)) / tabCount;
                for (int i = 0; i < tabCount; i++)
                {
                    int chapterIndex = i;
                    float left = .06f + (width + gap) * i;
                    chapterTabs[i] = CreateButton("Chapter " + (i + 1) + " Tab",
                        font, rounded, content,
                        new Vector2(left, .655f), new Vector2(left + width, .705f),
                        () => ShowPage(chapterIndex), out var tab);
                    if (relayPreview)
                    {
                        if (i == 5)
                            LocalizedLabel.Bind(tab, () =>
                                GameLanguageService.Shared.CurrentLanguage ==
                                GameLanguage.Turkish ? "6 · RÖLE" : "6 · RELAY");
                        else
                            LocalizedLabel.Bind(tab, () =>
                                (chapterIndex + 1).ToString());
                        tab.fontSize = i == 5 ? 16 : 23;
                    }
                    else
                    {
                        LocalizedLabel.Bind(tab, "chapter.title", i + 1);
                        tab.fontSize = tabCount == 5 ? 18
                            : tabCount == 4 ? 20 : 25;
                    }
                }
                secondTab = chapterTabs[1];
                journey = PlaceholderVisuals.Rect("Chapter Journey", content,
                    new Vector2(.07f, .755f), new Vector2(.93f, .814f))
                    .gameObject.AddComponent<ChapterJourneyPanel>();
                journey.Build(font, rounded);
            }
            CreateButton("Play Current", font, rounded, content, new Vector2(.18f,.135f),new Vector2(.82f,.21f),playCurrent,out var play).targetGraphic.color = new Color32(39,136,116,255);
            continueLabel = play;
            LocalizedLabel.Bind(play, "resume.continue");
            CreateButton("Close Chapter",font,rounded,content,new Vector2(.3f,.055f),new Vector2(.7f,.12f),close,out var back);
            LocalizedLabel.Bind(back, "common.back");
            foreach (var motion in GetComponentsInChildren<PresentationMotion>()) motion.Settings = Settings;
            gameObject.SetActive(false);
        }
        private Text dailyLabel;
        private Button dailyEntry;
        public void BuildDailyEntry(Font font,Sprite rounded,Action open)
        {
            dailyEntry=CreateButton("Daily Shift Entry",font,rounded,
                transform.Find("Chapter Content"),
                new Vector2(.14f, expanded ? .714f : .75f),
                new Vector2(.86f, expanded ? .751f : .787f),
                open,out dailyLabel);
            dailyLabel.fontSize=24;dailyEntry.GetComponent<PresentationMotion>().Settings=Settings;
        }
        public void RefreshDaily(DailyRecord today,HintLanguage language)
        {
            if(dailyLabel==null)return;
            dailyEntry.interactable = today != null;
            LocalizedLabel.Bind(dailyLabel, () => today == null ? GameLanguageService.Shared.Text("resume.daily_unavailable") : DailyText.Title(Language)+" · "+DailyText.State(today,Language));
            dailyEntry.targetGraphic.color=today != null && today.perfect?new Color32(181,137,48,255):IdentityStyle.Teal;
        }
        public void Open(LevelProgression progress)
        {
            progressState = progress;
            LocalizedLabel.Bind(continueLabel, () => GameLanguageService.Shared.Text(progress.ChapterComplete ? "resume.mastery" : "resume.continue_level", progress.Recommended + 1));
            for (int i = 0; i < buttons.Length; i++)
            {
                bool unlocked = progress.IsUnlocked(i);
                buttons[i].interactable = unlocked;
                int number = i + 1;
                string key = !unlocked ? "level.locked" : i <= progress.HighestCompleted ? "level.done" : i == progress.Recommended ? "level.play" : null;
                LocalizedLabel.Bind(labels[i], () => number + (key == null ? "" : "\n" + GameLanguageService.Shared.Text(key)));
                var state = progress.Mastery(i);
                masteryMarkers[i].text = state == LevelMasteryState.Perfect ? "◆" : state == LevelMasteryState.Completed ? "✓" : "";
                masteryMarkers[i].color = state == LevelMasteryState.Perfect ? new Color32(255,214,113,255) : new Color32(209,241,228,255);
                if (state == LevelMasteryState.Perfect || state == LevelMasteryState.Completed) LocalizedLabel.Bind(labels[i], () => number.ToString());
                backgrounds[i].color = !progress.ChapterComplete && i == progress.Recommended ? VisualTheme.Accent : unlocked ? new Color32(65,93,108,255) : new Color32(220,229,228,255);
                labels[i].color = unlocked ? Color.white : new Color32(96,116,125,255);
            }
            ShowPage(CampaignContinuation.Chapter(progress));
            transform.SetAsLastSibling(); gameObject.SetActive(true);
            open = true; group.blocksRaycasts = true; group.interactable = true;
            StopAllCoroutines(); StartCoroutine(Animate(true));
        }
        public void ShowPage(int chapter)
        {
            if (progressState == null || chapter < 0 || !CampaignChapters.Valid(chapter, buttons.Length) || !progressState.IsUnlocked(CampaignChapters.Start(chapter))) return;
            page = chapter;
            var summary = progressState.ChapterSummary(page);
            var chapterWorld = expanded ? ChapterJourneyCatalog.ForChapter(page) : null;
            if (journey != null && chapterWorld != null &&
                ChapterJourneyCatalog.Available(progressState, page))
            {
                chapterTitle.fontSize = 36;
                LocalizedLabel.Bind(chapterTitle, () =>
                    GameLanguageService.Shared.Text("chapter.title", page + 1) +
                    " · " + (Language == HintLanguage.Turkish
                        ? chapterWorld.titleTr : chapterWorld.titleEn));
                masterySummary.fontSize = 23;
                LocalizedLabel.Bind(masterySummary, () =>
                    Language == HintLanguage.Turkish
                        ? chapterWorld.purposeTr : chapterWorld.purposeEn);
                LocalizedLabel.Bind(masteryLegend, () => MasteryText.Legend(Language));
                LocalizedLabel.Bind(masteryReplay, () => MasteryText.Summary(summary, Language));
                journey.Show(progressState, page);
                if (worldBackdrop != null)
                    worldBackdrop.Show(progressState, page, Settings.reducedMotion);
            }
            else
            {
                if (journey != null) journey.Hide();
                if (worldBackdrop != null) worldBackdrop.Hide();
                if (relayPreview && page == 5)
                {
                    LocalizedLabel.Bind(chapterTitle, () =>
                        GameLanguageService.Shared.CurrentLanguage ==
                        GameLanguage.Turkish
                            ? "BÖLÜM 6 · RÖLE TESTİ"
                            : "CHAPTER 6 · RELAY PILOT");
                    LocalizedLabel.Bind(masterySummary, () =>
                        GameLanguageService.Shared.CurrentLanguage ==
                        GameLanguage.Turkish
                            ? "101–103 · Bekleyen hareket ve hamle sırası"
                            : "101–103 · Waiting moves and timing");
                    chapterTitle.fontSize = 34;
                    masterySummary.fontSize = 22;
                    LocalizedLabel.Bind(masteryLegend, () =>
                        MasteryText.Legend(Language));
                    LocalizedLabel.Bind(masteryReplay, () =>
                        MasteryText.Summary(summary, Language));
                }
                else
                {
                    LocalizedLabel.Bind(chapterTitle, () =>
                        summary.Mastered ? MasteryText.Mastered(Language)
                            : GameLanguageService.Shared.Text(
                                "chapter.title", page + 1));
                    chapterTitle.fontSize = expanded ? 38
                        : summary.Mastered ? 46 : 64;
                    masterySummary.fontSize = expanded ? 23 : 30;
                    LocalizedLabel.Bind(masterySummary, () =>
                        MasteryText.Summary(summary, Language));
                    LocalizedLabel.Bind(masteryLegend, () =>
                        MasteryText.Legend(Language));
                    LocalizedLabel.Bind(masteryReplay, () =>
                        summary.Completed == summary.Total
                            ? MasteryText.Remaining(summary, Language) : "");
                }
            }
            MasteryViewed?.Invoke(summary);
            if (chapterTabs != null)
                for (int i = 0; i < chapterTabs.Length; i++)
                    chapterTabs[i].interactable =
                        i == 0 || progressState.IsUnlocked(CampaignChapters.Start(i));
            for (int i = 0; i < buttons.Length; i++) buttons[i].gameObject.SetActive(CampaignChapters.Index(i) == page);
        }
        public void Close()
        {
            if (!gameObject.activeSelf) return;
            open = false; group.blocksRaycasts = false; group.interactable = false;
            StopAllCoroutines(); StartCoroutine(Animate(false));
        }
        private IEnumerator Animate(bool opening)
        {
            float duration = Mathf.Max(.01f, Settings.panelDuration);
            for (float elapsed = 0; elapsed < duration; elapsed += Time.unscaledDeltaTime)
            {
                float t = Mathf.Clamp01(elapsed / duration);
                group.alpha = opening ? Mathf.Lerp(.85f,1,t) : 1-t;
                group.transform.localScale = Vector3.one * (Settings.reducedMotion ? 1 : opening ? Mathf.Lerp(.97f,1,t) : Mathf.Lerp(1,.985f,t));
                yield return null;
            }
            group.alpha = opening ? 1 : 0; group.transform.localScale = Vector3.one;
            if (!opening) gameObject.SetActive(false);
        }
        public static Button CreateButton(string name, Font font, Sprite rounded, Transform parent, Vector2 min, Vector2 max, Action click, out Text label)
        {
            var rect = PlaceholderVisuals.Rect(name,parent,min,max);
            var image = rect.gameObject.AddComponent<Image>(); image.sprite = rounded; image.type = Image.Type.Sliced;
            image.color = IdentityStyle.Navy; IdentityStyle.Material(image);
            var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = image;
            button.transition = Selectable.Transition.ColorTint;
            var colors = button.colors; colors.highlightedColor = new Color(.94f,1,1); colors.pressedColor = new Color(.78f,.9f,.88f); colors.disabledColor = Color.white; colors.fadeDuration = .08f; button.colors = colors;
            VisualTheme.Shadow(image); rect.gameObject.AddComponent<PresentationMotion>(); button.onClick.AddListener(() => click());
            label = PlaceholderVisuals.Label("Label",rect,font,"",32,Color.white,new Vector2(.04f,.08f),new Vector2(.96f,.92f));
            return button;
        }
    }
}
