using System;
using UnityEngine;
using UnityEngine.UI;

namespace Shift.Game
{
    public enum MoraHomeMood { Idle, Happy, Curious }

    /// <summary>Pure interaction rules. No currency, save data, gameplay actions or timers.</summary>
    public static class MoraHabitatInteractionPolicy
    {
        public const float TapCooldownSeconds = .28f;
        public const float ExpressionHoldSeconds = 1.55f;
        public const float FaceBlendSeconds = .22f;

        public static bool CanAcceptTap(float now, float nextAllowedAt)
            => now >= nextAllowedAt;

        // Fixed-duration, clamped visual blend; the opaque Idle image underneath never fades.
        public static float FaceBlendWeight(float elapsed)
            => Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / FaceBlendSeconds));

        public static MoraHomeMood MoodForTouch(int acceptedTouches)
            => acceptedTouches <= 0 ? MoraHomeMood.Idle
                : acceptedTouches % 2 == 1 ? MoraHomeMood.Happy : MoraHomeMood.Curious;

        public static string Greeting(bool turkish, int acceptedTouches)
        {
            switch (MoodForTouch(acceptedTouches))
            {
                case MoraHomeMood.Happy:
                    return turkish ? "MORA seni gördüğüne sevindi!" : "MORA is happy to see you!";
                case MoraHomeMood.Curious:
                    return turkish ? "MORA seni merakla izliyor." : "MORA looks at you curiously.";
                default:
                    return turkish ? "MORA'ya dokun, seni selamlasın." : "Tap MORA to say hello.";
            }
        }
    }

    /// <summary>
    /// Opt-in, non-persistent habitat preview. Only the character state changes on tap;
    /// the character GameObject is never hidden, recreated, or disabled by interactions.
    /// The entire panel is closed only with Back to Chapters.
    /// </summary>
    public sealed class MoraHomePanel : MonoBehaviour
    {
        private GameFeelSettings feel;
        private Image moraBase;
        private Image moraBlend;
        private Image moraIncoming;
        private float outgoingAlpha;
        private Sprite incomingExpression;
        private RectTransform moraRect;
        private Sprite idle, curious, happy;
        private bool usesPrimarySeedArt;

        // The reference-board seed creature is MORA. The tested teal creature is a
        // separate special-companion prototype until its unlock design is approved.
        public static string CharacterSpritePath(bool primary, MoraHomeMood mood)
        {
            string stem = primary ? "Mora/MoraSeed_" : "Mora/Mora_";
            return stem + (mood == MoraHomeMood.Happy ? "Happy"
                : mood == MoraHomeMood.Curious ? "Curious" : "Idle");
        }
        private Text response;
        private Vector3 restScale = Vector3.one;
        private float currentScale = 1f;
        private float scaleVelocity;
        private float bounceEnergy;
        private float nextAllowedTouchAt;
        private float expressionUntil;
        private float faceBlendElapsed;
        private bool isFaceBlending;
        private int acceptedTouches;
        private bool isOpen;
        private MoraHomeMood mood;

        public bool IsOpen => isOpen;
        public int AcceptedTouches => acceptedTouches;
        public MoraHomeMood Mood => mood;
        private static bool Turkish => GameLanguageService.Shared.CurrentLanguage == GameLanguage.Turkish;

        public static MoraHomePanel TryCreate(RectTransform safeArea, Font font, Sprite rounded,
            Sprite circle, GameFeelSettings feel, Action onClose)
        {
            Sprite seed = Resources.Load<Sprite>(CharacterSpritePath(true, MoraHomeMood.Idle));
            Sprite sprite = seed != null ? seed
                : Resources.Load<Sprite>(CharacterSpritePath(false, MoraHomeMood.Idle));
            if (sprite == null) return null;
            bool primary = seed != null;

            var root = PlaceholderVisuals.Rect("MORA Habitat", safeArea, Vector2.zero, Vector2.one);
            var panel = root.gameObject.AddComponent<MoraHomePanel>();
            panel.feel = feel;
            panel.usesPrimarySeedArt = primary;
            panel.idle = sprite;
            panel.curious = Resources.Load<Sprite>(CharacterSpritePath(primary, MoraHomeMood.Curious));
            panel.happy = Resources.Load<Sprite>(CharacterSpritePath(primary, MoraHomeMood.Happy));

            // Full-screen scenic setting, not a card overlay on the puzzle.
            var blocking = root.gameObject.AddComponent<Image>();
            blocking.color = new Color(.035f, .12f, .16f, 1f);
            blocking.raycastTarget = true;
            VisualTheme.Background(root, circle);
            var veil = PlaceholderVisuals.Rect("Habitat Soft Veil", root, Vector2.zero, Vector2.one)
                .gameObject.AddComponent<Image>();
            veil.color = new Color(.035f, .12f, .16f, .16f);
            veil.raycastTarget = false;

            var title = PlaceholderVisuals.Label("Habitat Title", root, font, "", 58,
                Color.white, new Vector2(.09f, .898f), new Vector2(.91f, .958f));
            title.fontStyle = FontStyle.Bold;
            LocalizedLabel.Bind(title, () => primary ? "MORA"
                : Turkish ? "ÖZEL KARAKTER" : "SPECIAL COMPANION");
            var subtitle = PlaceholderVisuals.Label("Habitat Subtitle", root, font, "", 31,
                new Color32(242, 253, 255, 255), new Vector2(.08f, .855f), new Vector2(.92f, .898f));
            LocalizedLabel.Bind(subtitle, () => primary
                ? (Turkish ? "MORA'NIN YAŞAM ALANI" : "MORA'S LITTLE HABITAT")
                : (Turkish ? "KOLEKSİYON KARAKTERİ · TEST" : "COLLECTIBLE CHARACTER · TEST"));

            // A genuine transparent island PNG is loaded if installed. Without it,
            // a subdued procedural ground preserves the UI/test flow.
            var island = Resources.Load<Sprite>("Mora/Mora_Habitat");
            if (island != null)
            {
                var stage = PlaceholderVisuals.Rect("MORA Island Artwork", root,
                    new Vector2(.015f, .275f), new Vector2(.985f, .575f));
                var art = stage.gameObject.AddComponent<Image>();
                art.sprite = island;
                art.preserveAspect = true;
                art.raycastTarget = false;
            }
            else
            {
                var moss = VisualTheme.Surface("Fallback Moss Island", root, circle,
                    new Color(.19f, .39f, .30f, .90f),
                    new Vector2(.08f, .30f), new Vector2(.92f, .42f));
                moss.type = Image.Type.Simple;
                var stone = VisualTheme.Surface("Fallback Stone Base", root, circle,
                    new Color(.27f, .42f, .45f, .83f),
                    new Vector2(.12f, .282f), new Vector2(.88f, .359f));
                stone.type = Image.Type.Simple;
            }

            var ambient = PlaceholderVisuals.Rect("Habitat Ambient Life", root, Vector2.zero, Vector2.one)
                .gameObject.AddComponent<MoraHabitatAmbient>();
            ambient.Initialize(circle, feel);

            // Tiny natural contact shadow. Never a large flat light disc.
            var contact = VisualTheme.Surface("MORA Contact Shadow", root, circle,
                new Color(.035f, .16f, .19f, .24f),
                new Vector2(.34f, .321f), new Vector2(.66f, .348f));
            contact.type = Image.Type.Simple;

            var portrait = PlaceholderVisuals.Rect("MORA Character", root,
                new Vector2(.12f, .327f), new Vector2(.88f, .79f));
            panel.moraRect = portrait;
            panel.moraBase = portrait.gameObject.AddComponent<Image>();
            panel.moraBase.sprite = sprite;
            panel.moraBase.preserveAspect = true;
            panel.moraBase.raycastTarget = true;

            // Cross-fade to the next face instead of blanking or recreating the image.
            var blendRect = PlaceholderVisuals.Rect("MORA Expression Blend", portrait, Vector2.zero, Vector2.one);
            panel.moraBlend = blendRect.gameObject.AddComponent<Image>();
            panel.moraBlend.sprite = sprite;
            panel.moraBlend.preserveAspect = true;
            panel.moraBlend.color = new Color(1f, 1f, 1f, 0f);
            panel.moraBlend.raycastTarget = false;
            // Two expression overlays cross-fade over a permanently visible Idle portrait.
            // Even if an optional expression PNG has bad alpha, MORA does not disappear.
            var incomingRect = PlaceholderVisuals.Rect("MORA Incoming Expression",
                portrait, Vector2.zero, Vector2.one);
            panel.moraIncoming = incomingRect.gameObject.AddComponent<Image>();
            panel.moraIncoming.sprite = sprite;
            panel.moraIncoming.preserveAspect = true;
            panel.moraIncoming.color = new Color(1f, 1f, 1f, 0f);
            panel.moraIncoming.raycastTarget = false;

            var touch = portrait.gameObject.AddComponent<Button>();
            touch.targetGraphic = panel.moraBase;
            touch.transition = Selectable.Transition.None;
            touch.onClick.AddListener(panel.OnMoraTouched);

            // A slim warm caption rather than a large rectangular HUD card.
            var note = VisualTheme.Surface("MORA Speech Ribbon", root, rounded,
                new Color(.96f, .94f, .84f, .91f), new Vector2(.15f, .178f),
                new Vector2(.85f, .242f));
            IdentityStyle.Material(note);
            note.color = new Color(.96f, .94f, .84f, .91f);
            panel.response = PlaceholderVisuals.Label("MORA Greeting", note.transform,
                font, "", 36, VisualTheme.Ink, new Vector2(.055f, .10f), new Vector2(.945f, .90f));
            panel.ShowGreeting();

            var preview = PlaceholderVisuals.Label("Habitat Preview Notice", root, font, "", 24,
                new Color32(238, 249, 249, 235),
                new Vector2(.10f, .131f), new Vector2(.90f, .164f));
            LocalizedLabel.Bind(preview, () => primary
                ? (Turkish ? "Ön izleme · Gelişim sistemi henüz etkin değil"
                    : "Preview · Growth is not active yet")
                : (Turkish ? "Ön izleme · Ödül sistemi henüz etkin değil"
                    : "Preview · Reward unlock is not active yet"));

            var exit = ChapterSelect.CreateButton("Return to Chapters", font, rounded, root,
                new Vector2(.24f, .045f), new Vector2(.76f, .112f), onClose, out var exitLabel);
            exit.targetGraphic.color = IdentityStyle.Navy;
            exitLabel.fontSize = 36;
            exitLabel.resizeTextMaxSize = 36;
            LocalizedLabel.Bind(exitLabel, () => Turkish ? "Bölümlere Dön" : "Back to Chapters");
            exit.GetComponent<PresentationMotion>().Settings = feel;

            panel.gameObject.SetActive(false);
            return panel;
        }

        // Retains the v0.6.1 API for local language/interaction regression tests.
        public static string GreetingText(bool turkish, int touches)
            => MoraHabitatInteractionPolicy.Greeting(turkish, touches);

        private void ShowGreeting()
            => LocalizedLabel.Bind(response, () => usesPrimarySeedArt
                ? GreetingText(Turkish, acceptedTouches)
                : (Turkish
                    ? (acceptedTouches == 0 ? "Karaktere dokun, seni selamlasın."
                       : acceptedTouches % 2 == 1 ? "Özel karakter sana sevindi!"
                       : "Özel karakter seni merakla izliyor.")
                    : (acceptedTouches == 0 ? "Tap the companion to say hello."
                       : acceptedTouches % 2 == 1 ? "The companion is happy to see you!"
                       : "The companion looks at you curiously.")));

        public void Open()
        {
            if (isOpen) return;
            isOpen = true;
            acceptedTouches = 0;
            nextAllowedTouchAt = float.NegativeInfinity;
            expressionUntil = float.NegativeInfinity;
            bounceEnergy = 0f;
            currentScale = 1f;
            scaleVelocity = 0f;
            mood = MoraHomeMood.Idle;
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            RestoreImmediately();
            ShowGreeting();
        }

        public void Close()
        {
            if (!isOpen) return;
            isOpen = false;
            RestoreImmediately();
            gameObject.SetActive(false);
        }

        private void OnMoraTouched()
        {
            if (!isOpen) return;
            float now = Time.unscaledTime;
            if (!MoraHabitatInteractionPolicy.CanAcceptTap(now, nextAllowedTouchAt))
                return; // Ignore rapid repeat, keep existing pose and sprite visible.

            nextAllowedTouchAt = now + MoraHabitatInteractionPolicy.TapCooldownSeconds;
            acceptedTouches++;
            mood = MoraHabitatInteractionPolicy.MoodForTouch(acceptedTouches);
            expressionUntil = now + MoraHabitatInteractionPolicy.ExpressionHoldSeconds;
            Sprite selected = mood == MoraHomeMood.Happy
                ? (happy != null ? happy : idle)
                : (curious != null ? curious : idle);
            TransitionTo(selected);
            bounceEnergy = Mathf.Min(1.35f, bounceEnergy + .82f);
            ShowGreeting();
        }

        private void TransitionTo(Sprite selected)
        {
            if (moraBase == null || moraBlend == null || moraIncoming == null) return;
            Sprite target = selected != null ? selected : idle;
            if (!isFaceBlending && moraBlend.sprite == target)
                return;

            // A tap during a transition keeps whichever overlay is most visible.
            // The bottom image is always Idle, full opacity. Never change it for reactions.
            if (isFaceBlending && moraIncoming.color.a > moraBlend.color.a)
            {
                moraBlend.sprite = moraIncoming.sprite;
                moraBlend.color = new Color(1f, 1f, 1f, moraIncoming.color.a);
            }
            outgoingAlpha = moraBlend.color.a;
            incomingExpression = target;
            moraIncoming.sprite = target;
            moraIncoming.color = new Color(1f, 1f, 1f, 0f);
            faceBlendElapsed = 0f;
            isFaceBlending = true;
        }

        private void Update()
        {
            if (!isOpen || moraRect == null) return;
            float dt = Time.unscaledDeltaTime;

            if (isFaceBlending)
            {
                faceBlendElapsed += dt;
                float amount = MoraHabitatInteractionPolicy.FaceBlendWeight(faceBlendElapsed);
                moraBlend.color = new Color(1f, 1f, 1f, outgoingAlpha * (1f - amount));
                float targetOpacity = incomingExpression == idle ? 0f : amount;
                moraIncoming.color = new Color(1f, 1f, 1f, targetOpacity);
                if (amount >= 1f)
                {
                    // Commit only the overlay, never replace the guaranteed-visible Idle base.
                    moraBlend.sprite = incomingExpression;
                    moraBlend.color = new Color(1f, 1f, 1f,
                        incomingExpression == idle ? 0f : 1f);
                    moraIncoming.color = new Color(1f, 1f, 1f, 0f);
                    isFaceBlending = false;
                }
            }

            if (mood != MoraHomeMood.Idle && Time.unscaledTime >= expressionUntil)
            {
                mood = MoraHomeMood.Idle;
                TransitionTo(idle);
            }

            bool reducedMotion = feel != null && feel.reducedMotion;
            if (reducedMotion)
            {
                currentScale = 1f;
                scaleVelocity = 0f;
                bounceEnergy = 0f;
            }
            else
            {
                bounceEnergy = Mathf.MoveTowards(bounceEnergy, 0f, 1.65f * dt);
                float breathing = .009f * Mathf.Sin(Time.unscaledTime * 1.55f);
                float desired = 1f + breathing + bounceEnergy * .075f;
                currentScale = Mathf.SmoothDamp(currentScale, desired, ref scaleVelocity,
                    .14f, Mathf.Infinity, dt);
            }
            moraRect.localScale = restScale * currentScale;
        }

        private void RestoreImmediately()
        {
            mood = MoraHomeMood.Idle;
            isFaceBlending = false;
            faceBlendElapsed = 0f;
            currentScale = 1f;
            bounceEnergy = 0f;
            scaleVelocity = 0f;
            if (moraRect != null) moraRect.localScale = restScale;
            if (moraBase != null) { moraBase.sprite = idle; moraBase.color = Color.white; }
            if (moraBlend != null) { moraBlend.sprite = idle; moraBlend.color = new Color(1f, 1f, 1f, 0f); }
            if (moraIncoming != null) { moraIncoming.sprite = idle; moraIncoming.color = new Color(1f, 1f, 1f, 0f); }
            incomingExpression = idle;
            outgoingAlpha = 0f;
        }

        private void OnDisable()
        {
            // Only executes when leaving the habitat, never when tapping the character.
            RestoreImmediately();
        }
    }

    /// <summary>
    /// Very light, deterministic environmental motion. Does not allocate every frame,
    /// use particle systems, create progress, or react to gameplay.
    /// </summary>
    public sealed class MoraHabitatAmbient : MonoBehaviour
    {
        private struct LightDot
        {
            public RectTransform Rect;
            public Image Image;
            public float Phase;
            public bool Water;
        }

        private LightDot[] dots;
        private GameFeelSettings feel;

        public void Initialize(Sprite circle, GameFeelSettings settings)
        {
            feel = settings;
            if (circle == null) return;
            // Sparse points near the island, rather than noisy particles all over the display.
            var positions = new[]
            {
                new Vector2(.16f,.47f), new Vector2(.25f,.525f),
                new Vector2(.79f,.50f), new Vector2(.86f,.42f),
                new Vector2(.34f,.58f), new Vector2(.68f,.59f),
                new Vector2(.19f,.312f), new Vector2(.81f,.316f)
            };
            dots = new LightDot[positions.Length];
            for (int i = 0; i < positions.Length; i++)
            {
                bool water = i >= 6;
                float width = water ? .028f : .010f;
                float height = water ? .003f : .005f;
                var pos = positions[i];
                var rect = PlaceholderVisuals.Rect(water ? "Water Shimmer" : "Glow Motif",
                    transform, pos - new Vector2(width, height),
                    pos + new Vector2(width, height));
                var image = rect.gameObject.AddComponent<Image>();
                image.sprite = circle;
                image.type = Image.Type.Simple;
                image.raycastTarget = false;
                dots[i] = new LightDot { Rect = rect, Image = image, Phase = i * 1.13f, Water = water };
            }
            Apply(true);
        }

        private void Update() => Apply(feel != null && feel.reducedMotion);

        private void Apply(bool still)
        {
            if (dots == null) return;
            float time = Time.unscaledTime;
            for (int i = 0; i < dots.Length; i++)
            {
                var dot = dots[i];
                if (dot.Rect == null || dot.Image == null) continue;
                float wave = still ? 0f : Mathf.Sin(time * (dot.Water ? .85f : .56f) + dot.Phase);
                dot.Rect.anchoredPosition = still ? Vector2.zero
                    : new Vector2(dot.Water ? wave * 1.5f : wave * 3f,
                        dot.Water ? 0f : wave * 4f);
                float alpha = dot.Water ? .20f + .08f * wave : .38f + .16f * wave;
                dot.Image.color = new Color(dot.Water ? .46f : 1f, .95f, dot.Water ? 1f : .65f,
                    Mathf.Clamp01(alpha));
            }
        }
    }
}
