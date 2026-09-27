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
        private Text continueLabel;
        private LevelProgression progressState;
        private int page;
        private bool open;
        public GameFeelSettings Settings { private get; set; } = new GameFeelSettings();
        public bool IsOpen => open;
        public void Build(Font font, Sprite rounded, int count, Action<int> select, Action close, Action playCurrent)
        {
            var shade = gameObject.AddComponent<Image>(); shade.color = new Color(.055f,.13f,.23f,.88f);
            var content = PlaceholderVisuals.Rect("Chapter Content", transform, Vector2.zero, Vector2.one);
            group = content.gameObject.AddComponent<CanvasGroup>();
            var card = VisualTheme.Surface("Chapter Card", content, rounded, IdentityStyle.Cream, new Vector2(.025f,.04f), new Vector2(.975f,.95f));
            IdentityStyle.Material(card); VisualTheme.Shadow(card,8);
            chapterTitle = PlaceholderVisuals.Label("Chapter Title", content, font, "CHAPTER 1", 64, VisualTheme.Ink, new Vector2(.08f,.83f),new Vector2(.92f,.91f));
            masterySummary = PlaceholderVisuals.Label("Chapter Hint", content, font, "", 30, VisualTheme.Ink,new Vector2(.05f,.79f),new Vector2(.95f,.83f));
            masteryLegend = PlaceholderVisuals.Label("Mastery Legend", content, font, "", 23, VisualTheme.Ink,new Vector2(.04f,.25f),new Vector2(.96f,.278f));
            masteryReplay = PlaceholderVisuals.Label("Mastery Replay", content, font, "", 23, VisualTheme.Ink,new Vector2(.05f,.225f),new Vector2(.95f,.25f));
            masteryMarkers = new Text[count];
            buttons = new Button[count]; labels = new Text[count]; backgrounds = new Image[count];
            for (int i = 0; i < count; i++)
            {
                int index = i, row = (i % CampaignChapters.LevelsPerChapter) / 5, col = i % 5;
                var min = new Vector2(.06f + col * .18f, .605f - row * .105f);
                buttons[i] = CreateButton("Level " + (i + 1), font, rounded, content, min, min + new Vector2(.16f,.087f), () => select(index), out labels[i]);
                backgrounds[i] = (Image)buttons[i].targetGraphic;
                masteryMarkers[i] = PlaceholderVisuals.Label("Mastery Marker", buttons[i].transform, font, "", 28, Color.white, new Vector2(.73f,.68f),new Vector2(.96f,.96f));
                masteryMarkers[i].raycastTarget = false;
            }
            CreateButton("Chapter 1 Tab",font,rounded,content,new Vector2(.12f,.70f),new Vector2(.48f,.745f),() => ShowPage(0),out var tabOne);
            LocalizedLabel.Bind(tabOne, "chapter.title", 1);
            secondTab = CreateButton("Chapter 2 Tab",font,rounded,content,new Vector2(.52f,.70f),new Vector2(.88f,.745f),() => ShowPage(1),out var tabTwo);
            LocalizedLabel.Bind(tabTwo, "chapter.title", 2);
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
            dailyEntry=CreateButton("Daily Shift Entry",font,rounded,transform.Find("Chapter Content"),new Vector2(.14f,.75f),new Vector2(.86f,.787f),open,out dailyLabel);
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
            LocalizedLabel.Bind(chapterTitle, () => summary.Mastered ? MasteryText.Mastered(Language) : GameLanguageService.Shared.Text("chapter.title", page + 1));
            if (summary.Mastered) chapterTitle.fontSize = 46; else chapterTitle.fontSize = 64;
            LocalizedLabel.Bind(masterySummary, () => MasteryText.Summary(summary, Language));
            LocalizedLabel.Bind(masteryLegend, () => MasteryText.Legend(Language));
            LocalizedLabel.Bind(masteryReplay, () => summary.Completed == summary.Total ? MasteryText.Remaining(summary, Language) : "");
            MasteryViewed?.Invoke(summary);
            secondTab.interactable = progressState.IsUnlocked(CampaignChapters.Start(1));
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
