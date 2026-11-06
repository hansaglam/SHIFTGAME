using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Shift.Game
{
    /// <summary>
    /// An opt-in, non-persistent Living World preview. It never grants rewards or saves
    /// companion state; puzzle simulation stays entirely separate.
    /// </summary>
    public sealed class MoraHomePanel : MonoBehaviour
    {
        private GameFeelSettings feel;
        private Image mora;
        private RectTransform moraRect;
        private Sprite idle, curious, happy;
        private Text response;
        private Coroutine touchRoutine;
        private Vector3 restScale;
        private int interactions;
        private bool isOpen;

        public bool IsOpen => isOpen;
        private static bool Turkish => GameLanguageService.Shared.CurrentLanguage == GameLanguage.Turkish;

        public static MoraHomePanel TryCreate(RectTransform safeArea, Font font, Sprite rounded,
            Sprite circle, GameFeelSettings feel, Action onClose)
        {
            var sprite = Resources.Load<Sprite>("Mora/Mora_Idle");
            if (sprite == null) return null;

            var root = PlaceholderVisuals.Rect("MORA Home", safeArea, Vector2.zero, Vector2.one);
            var panel = root.gameObject.AddComponent<MoraHomePanel>();
            panel.feel = feel;
            panel.idle = sprite;
            panel.curious = Resources.Load<Sprite>("Mora/Mora_Curious");
            panel.happy = Resources.Load<Sprite>("Mora/Mora_Happy");

            // Shared visual language: alpine lake, translucent overlay, ivory content.
            var blocking = root.gameObject.AddComponent<Image>();
            blocking.color = new Color(.04f, .13f, .19f, 1f);
            blocking.raycastTarget = true;
            VisualTheme.Background(root, circle);
            var veil = PlaceholderVisuals.Rect("MORA Home Veil", root, Vector2.zero, Vector2.one)
                .gameObject.AddComponent<Image>();
            veil.color = new Color(.045f, .14f, .20f, .37f);
            veil.raycastTarget = false;

            var title = PlaceholderVisuals.Label("MORA Home Title", root, font, "MORA", 78,
                Color.white, new Vector2(.12f, .88f), new Vector2(.88f, .955f));
            title.fontStyle = FontStyle.Bold;
            var subtitle = PlaceholderVisuals.Label("MORA Home Subtitle", root, font, "", 31,
                new Color32(242, 253, 255, 255), new Vector2(.07f, .824f), new Vector2(.93f, .874f));
            LocalizedLabel.Bind(subtitle, () => Turkish ? "MORA'NIN YAŞAM ALANI" : "MORA'S LITTLE HOME");

            var glow = VisualTheme.Surface("Warm Halo", root, circle,
                new Color(1f, .83f, .44f, .27f), new Vector2(.20f, .38f), new Vector2(.80f, .72f));
            glow.type = Image.Type.Simple;

            var portrait = PlaceholderVisuals.Rect("MORA Character", root,
                new Vector2(.12f, .325f), new Vector2(.88f, .79f));
            panel.moraRect = portrait;
            panel.restScale = Vector3.one;
            panel.mora = portrait.gameObject.AddComponent<Image>();
            panel.mora.sprite = sprite;
            panel.mora.preserveAspect = true;
            panel.mora.raycastTarget = true;
            var touch = portrait.gameObject.AddComponent<Button>();
            touch.targetGraphic = panel.mora;
            touch.transition = Selectable.Transition.None;
            touch.onClick.AddListener(panel.OnMoraTouched);

            var card = VisualTheme.Surface("MORA Greeting Card", root, rounded,
                new Color32(255, 244, 218, 250), new Vector2(.08f, .205f), new Vector2(.92f, .318f));
            IdentityStyle.Material(card);
            VisualTheme.Shadow(card);
            panel.response = PlaceholderVisuals.Label("MORA Greeting", card.transform, font, "", 34,
                VisualTheme.Ink, new Vector2(.065f, .08f), new Vector2(.935f, .93f));
            panel.ShowGreeting(false);

            var note = PlaceholderVisuals.Label("MORA Preview Note", root, font, "", 26,
                Color.white, new Vector2(.09f, .132f), new Vector2(.91f, .191f));
            LocalizedLabel.Bind(note, () => Turkish
                ? "İlk deneme: büyüme ve ödüller henüz etkin değil."
                : "Early preview: growth and rewards aren't active yet.");

            var exit = ChapterSelect.CreateButton("Return to Chapters", font, rounded, root,
                new Vector2(.23f, .045f), new Vector2(.77f, .115f), onClose, out var exitLabel);
            exit.targetGraphic.color = IdentityStyle.Navy;
            LocalizedLabel.Bind(exitLabel, () => Turkish ? "Bölümlere Dön" : "Back to Chapters");
            exit.GetComponent<PresentationMotion>().Settings = feel;

            panel.gameObject.SetActive(false);
            return panel;
        }

        private void ShowGreeting(bool touched)
        {
            LocalizedLabel.Bind(response, () => Turkish
                ? (touched ? "MORA seni gördüğüne sevindi!" : "MORA'ya dokun, seni selamlasın.")
                : (touched ? "MORA is happy to see you!" : "Tap MORA to say hello."));
        }

        public void Open()
        {
            if (isOpen) return;
            interactions = 0;
            isOpen = true;
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            ResetMora();
            ShowGreeting(false);
        }

        public void Close()
        {
            if (!isOpen) return;
            isOpen = false;
            if (touchRoutine != null) { StopCoroutine(touchRoutine); touchRoutine = null; }
            ResetMora();
            gameObject.SetActive(false);
        }

        private void OnMoraTouched()
        {
            if (!isOpen) return;
            interactions++;
            ShowGreeting(true);
            if (touchRoutine != null) StopCoroutine(touchRoutine);
            touchRoutine = StartCoroutine(TouchReaction());
        }

        private IEnumerator TouchReaction()
        {
            mora.sprite = happy != null ? happy : curious != null ? curious : idle;
            const float duration = 1.05f;
            for (float elapsed = 0f; elapsed < duration; elapsed += Time.unscaledDeltaTime)
            {
                bool reduced = feel != null && feel.reducedMotion;
                if (reduced) moraRect.localScale = restScale;
                else
                {
                    float t = Mathf.Clamp01(elapsed / duration);
                    float bounce = Mathf.Sin(Mathf.PI * t);
                    moraRect.localScale = restScale * (1f + .085f * bounce);
                }
                yield return null;
            }
            moraRect.localScale = restScale;
            mora.sprite = idle;
            touchRoutine = null;
        }

        private void Update()
        {
            if (!isOpen || touchRoutine != null || moraRect == null) return;
            if (feel != null && feel.reducedMotion) { moraRect.localScale = restScale; return; }
            // Calm breathing only inside MORA's dedicated home, never on the puzzle HUD.
            float breath = .012f * Mathf.Sin(Time.unscaledTime * 1.6f);
            moraRect.localScale = restScale * (1f + breath);
        }

        private void ResetMora()
        {
            if (moraRect != null) moraRect.localScale = restScale;
            if (mora != null) mora.sprite = idle;
        }

        private void OnDisable()
        {
            if (touchRoutine != null) { StopCoroutine(touchRoutine); touchRoutine = null; }
            ResetMora();
        }
    }
}
