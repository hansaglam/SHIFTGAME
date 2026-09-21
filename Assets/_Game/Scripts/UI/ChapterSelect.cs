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
        private Text chapterTitle;
        private Button secondTab;
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
            PlaceholderVisuals.Label("Chapter Hint", content, font, "Choose an unlocked puzzle", 30, VisualTheme.Muted,new Vector2(.05f,.79f),new Vector2(.95f,.83f));
            buttons = new Button[count]; labels = new Text[count]; backgrounds = new Image[count];
            for (int i = 0; i < count; i++)
            {
                int index = i, row = (i % 20) / 5, col = i % 5;
                var min = new Vector2(.06f + col * .18f, .63f - row * .115f);
                buttons[i] = CreateButton("Level " + (i + 1), font, rounded, content, min, min + new Vector2(.16f,.095f), () => select(index), out labels[i]);
                backgrounds[i] = (Image)buttons[i].targetGraphic;
            }
            CreateButton("Chapter 1 Tab",font,rounded,content,new Vector2(.12f,.735f),new Vector2(.48f,.785f),() => ShowPage(0),out var tabOne);
            tabOne.text = "CHAPTER 1";
            secondTab = CreateButton("Chapter 2 Tab",font,rounded,content,new Vector2(.52f,.735f),new Vector2(.88f,.785f),() => ShowPage(1),out var tabTwo);
            tabTwo.text = "CHAPTER 2";
            CreateButton("Play Current", font, rounded, content, new Vector2(.18f,.15f),new Vector2(.82f,.23f),playCurrent,out var play).targetGraphic.color = new Color32(39,136,116,255);
            play.text = "PLAY CURRENT LEVEL";
            CreateButton("Close Chapter",font,rounded,content,new Vector2(.3f,.055f),new Vector2(.7f,.12f),close,out var back);
            back.text = "BACK";
            foreach (var motion in GetComponentsInChildren<PresentationMotion>()) motion.Settings = Settings;
            gameObject.SetActive(false);
        }
        public void Open(LevelProgression progress)
        {
            progressState = progress;
            for (int i = 0; i < buttons.Length; i++)
            {
                bool unlocked = progress.IsUnlocked(i);
                buttons[i].interactable = unlocked;
                labels[i].text = (i + 1) + (unlocked ? i <= progress.HighestCompleted ? "\nDONE" : i == progress.Recommended ? "\nPLAY" : "" : "\nLOCKED");
                backgrounds[i].color = i == progress.Recommended ? VisualTheme.Accent : unlocked ? new Color32(65,93,108,255) : new Color32(220,229,228,255);
                labels[i].color = unlocked ? Color.white : new Color32(96,116,125,255);
            }
            ShowPage(progress.Current / 20);
            transform.SetAsLastSibling(); gameObject.SetActive(true);
            open = true; group.blocksRaycasts = true; group.interactable = true;
            StopAllCoroutines(); StartCoroutine(Animate(true));
        }
        public void ShowPage(int chapter)
        {
            if (progressState == null || chapter < 0 || chapter > 1 || (chapter == 1 && !progressState.IsUnlocked(20))) return;
            page = chapter; chapterTitle.text = "CHAPTER " + (page + 1);
            secondTab.interactable = progressState.IsUnlocked(20);
            for (int i = 0; i < buttons.Length; i++) buttons[i].gameObject.SetActive(i / 20 == page);
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
