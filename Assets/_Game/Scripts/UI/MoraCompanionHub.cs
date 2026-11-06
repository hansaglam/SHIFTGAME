using System;
using UnityEngine;
using UnityEngine.UI;

namespace Shift.Game
{
    public enum MoraHubPage { Home, Feed, Play, Decor, Growth, Collection }

    /// <summary>
    /// New opt-in companion UI shell. Real buttons, isolated views, no invented currencies
    /// or persistent pet care. Puzzle simulation and original campaign are untouched.
    /// </summary>
    public sealed class MoraCompanionHub : MonoBehaviour
    {
        private static readonly Color DeepTeal = new Color32(15, 74, 86, 255);
        private static readonly Color Ocean = new Color32(22, 108, 119, 255);
        private static readonly Color SeaGlass = new Color32(150, 222, 209, 255);
        private static readonly Color Ivory = new Color32(255, 247, 217, 255);
        private static readonly Color Gold = new Color32(255, 214, 116, 255);

        private Font font;
        private Sprite rounded, circle;
        private GameFeelSettings feel;
        private RectTransform portraitRect;
        private Image portrait, transition;
        private Sprite[] stageArt;
        private Sprite baseSeed;
        private Text greeting, growthSummary;
        private Text feedMessage, playMessage, growthMessage;
        private Image growthThumbnail;
        private RectTransform feedToken, playToken;
        private float feedMotion, playMotion;
        private int growthInfoStage = 1;
        private GameObject overlay;
        private GameObject[] pages;
        private int stage = 1, feedTaps, playTaps, touches;
        private bool isOpen, inTransition;
        private float fadeTime, reactionTime, nextTapTime;
        private MoraHubPage page = MoraHubPage.Home;
        private MoraGrowthSnapshot snapshot;

        public bool IsOpen => isOpen;
        public MoraHubPage CurrentPage => page;
        public int VisualStage => stage;
        private static bool Turkish => GameLanguageService.Shared.CurrentLanguage == GameLanguage.Turkish;

        public static string StageArtPath(int index)
            => "Mora/MoraScene_S" + Mathf.Clamp(index, 1, 4);

        public static string TitleFor(MoraHubPage section, bool tr)
        {
            switch (section)
            {
                case MoraHubPage.Feed: return tr ? "Besle" : "Feed";
                case MoraHubPage.Play: return tr ? "Oyna" : "Play";
                case MoraHubPage.Decor: return tr ? "Dekor" : "Decor";
                case MoraHubPage.Growth: return tr ? "Gelişim" : "Growth";
                case MoraHubPage.Collection: return tr ? "Koleksiyon" : "Collection";
                default: return "MORA";
            }
        }

        public static MoraCompanionHub TryCreate(RectTransform safe, Font font,
            Sprite rounded, Sprite circle, GameFeelSettings feel, Action returnToChapters)
        {
            Sprite accepted = Resources.Load<Sprite>(StageArtPath(1));
            Sprite seed = Resources.Load<Sprite>("Mora/MoraSeed_Idle");
            // The teal special character is deliberately not used as primary MORA.
            if (accepted == null && seed == null) return null;

            var root = PlaceholderVisuals.Rect("MORA Companion Hub v1", safe,
                Vector2.zero, Vector2.one);
            var hub = root.gameObject.AddComponent<MoraCompanionHub>();
            hub.font = font;
            hub.rounded = rounded;
            hub.circle = circle;
            hub.feel = feel;
            hub.baseSeed = seed;
            hub.stageArt = new Sprite[4];
            for (int i = 0; i < 4; i++)
                hub.stageArt[i] = Resources.Load<Sprite>(StageArtPath(i + 1));
            if (hub.stageArt[0] == null) hub.stageArt[0] = seed;
            hub.snapshot = MoraGrowthPolicy.FromCampaign(-1, 40);
            hub.Build(returnToChapters);
            hub.gameObject.SetActive(false);
            return hub;
        }

        private void Build(Action returnToChapters)
        {
            var blocker = gameObject.AddComponent<Image>();
            blocker.color = DeepTeal;
            blocker.raycastTarget = true;
            VisualTheme.Background(transform, circle);
            var tint = PlaceholderVisuals.Rect("Lake Tint", transform,
                Vector2.zero, Vector2.one).gameObject.AddComponent<Image>();
            tint.color = new Color(.015f, .18f, .20f, .20f);
            tint.raycastTarget = false;

            var wordmark = Label("SHIFT Wordmark", transform, "S H I F T", 48, Color.white,
                new Vector2(.18f, .94f), new Vector2(.82f, .99f));
            wordmark.fontStyle = FontStyle.Bold;
            Button("Exit To Chapters Top", transform, new Vector2(.04f, .937f),
                new Vector2(.15f, .992f), DeepTeal, "", "←", "←", returnToChapters);

            BuildCharacterRail();
            var title = Label("MORA Name", transform, "MORA", 67, Ivory,
                new Vector2(.14f, .814f), new Vector2(.86f, .885f));
            title.fontStyle = FontStyle.Bold;
            Label("Habitat Label", transform, "", 28, Color.white,
                new Vector2(.10f, .789f), new Vector2(.90f, .823f),
                () => Turkish ? "MORA'NIN YAŞAM ALANI" : "MORA'S LITTLE HABITAT");

            // A single composite portrait keeps the creature and stone platform aligned.
            // When only the old seed image exists, draw a fallback island behind it.
            if (stageArt[0] == baseSeed && baseSeed != null)
            {
                Sprite island = Resources.Load<Sprite>("Mora/MoraHabitat_S1")
                    ?? Resources.Load<Sprite>("Mora/Mora_Habitat");
                if (island != null)
                {
                    var ground = PlaceholderVisuals.Rect("MORA Ground", transform,
                        new Vector2(.07f, .432f), new Vector2(.93f, .61f))
                        .gameObject.AddComponent<Image>();
                    ground.sprite = island;
                    ground.preserveAspect = true;
                    ground.raycastTarget = false;
                }
            }
            portraitRect = PlaceholderVisuals.Rect("MORA Grounded Portrait", transform,
                new Vector2(.04f, .431f), new Vector2(.96f, .811f));
            portrait = portraitRect.gameObject.AddComponent<Image>();
            portrait.sprite = stageArt[0];
            portrait.preserveAspect = true;
            portrait.raycastTarget = true;
            var next = PlaceholderVisuals.Rect("New MORA Form", portraitRect,
                Vector2.zero, Vector2.one);
            transition = next.gameObject.AddComponent<Image>();
            transition.sprite = stageArt[0];
            transition.preserveAspect = true;
            transition.color = new Color(1f, 1f, 1f, 0f);
            transition.raycastTarget = false;
            var characterTap = portraitRect.gameObject.AddComponent<Button>();
            characterTap.targetGraphic = portrait;
            characterTap.transition = Selectable.Transition.None;
            characterTap.onClick.AddListener(TouchMora);

            var speech = Surface("Companion Speech", transform, Ivory,
                new Vector2(.13f, .406f), new Vector2(.87f, .446f));
            VisualTheme.Shadow(speech, 3);
            greeting = Label("Companion Speech Text", speech.transform, "", 30, DeepTeal,
                new Vector2(.05f, .05f), new Vector2(.95f, .95f));
            RefreshGreeting();

            // The living-seed artwork extends high above the island. Keep this area
            // free of decorative copy so Stage 4 leaves cannot overlap another label.
            BuildGrowthRibbon();
            BuildActionNavigation(returnToChapters);
            // Modal must be built last to cover the complete home screen.
            BuildSections();
        }

        private void BuildCharacterRail()
        {
            var rail = Surface("Companion Rail", transform,
                new Color(.04f, .28f, .36f, .79f),
                new Vector2(.045f, .887f), new Vector2(.955f, .932f));
            var select = Button("Choose MORA", rail.transform,
                new Vector2(.012f, .09f), new Vector2(.345f, .91f),
                Ocean, "", "MORA", "MORA", () => ShowPage(MoraHubPage.Home));
            select.GetComponentInChildren<Text>().fontStyle = FontStyle.Bold;

            Sprite avatar = baseSeed ?? stageArt[0];
            if (avatar != null)
            {
                var icon = PlaceholderVisuals.Rect("MORA Face Thumbnail", select.transform,
                    new Vector2(.03f, .08f), new Vector2(.35f, .93f))
                    .gameObject.AddComponent<Image>();
                icon.sprite = avatar;
                icon.preserveAspect = true;
                icon.raycastTarget = false;
            }
            var selectedLabel = select.GetComponentInChildren<Text>();
            selectedLabel.rectTransform.anchorMin = new Vector2(.34f, .08f);
            selectedLabel.rectTransform.anchorMax = new Vector2(.98f, .92f);
            Button("Future Companion 1", rail.transform,
                new Vector2(.365f, .09f), new Vector2(.585f, .91f),
                new Color(.18f, .45f, .51f, .95f), "", "?", "?",
                () => ShowPage(MoraHubPage.Collection));
            Button("Future Companion 2", rail.transform,
                new Vector2(.60f, .09f), new Vector2(.82f, .91f),
                new Color(.18f, .45f, .51f, .95f), "", "?", "?",
                () => ShowPage(MoraHubPage.Collection));
            Button("Future Companion Add", rail.transform,
                new Vector2(.835f, .09f), new Vector2(.985f, .91f),
                Ocean, "", "+", "+", () => ShowPage(MoraHubPage.Collection));
        }

        private void BuildGrowthRibbon()
        {
            var ribbon = Surface("Growth Ribbon", transform,
                new Color(.06f, .42f, .47f, .86f),
                new Vector2(.06f, .307f), new Vector2(.94f, .399f));
            var header = Label("Growth Ribbon Header", ribbon.transform, "", 30, Ivory,
                new Vector2(.06f, .67f), new Vector2(.94f, .99f),
                () => Turkish ? "MORA'NIN GELİŞİMİ" : "MORA'S GROWTH");
            header.fontStyle = FontStyle.Bold;
            for (int i = 0; i < 4; i++)
            {
                int stageNumber = i + 1;
                float x = .065f + i * .225f;
                var indicator = Surface("Growth Step " + stageNumber, ribbon.transform,
                    i == 0 ? Gold : new Color(.70f, .85f, .81f, .57f),
                    new Vector2(x, .30f), new Vector2(x + .19f, .65f));
                Label("Growth Number " + stageNumber, indicator.transform,
                    stageNumber.ToString("00"), 27, DeepTeal, Vector2.zero, Vector2.one);
            }
            growthSummary = Label("Growth Ribbon Note", ribbon.transform, "", 21, Ivory,
                new Vector2(.04f, .015f), new Vector2(.96f, .24f));
            LocalizedLabel.Bind(growthSummary, () => Turkish
                ? "Yeni bölümlerle gelişecek · Ön izleme"
                : "Grows with future chapters · Preview");
        }

        private void BuildActionNavigation(Action exit)
        {
            Tile("Feed Tile", "Besle", "Feed", IdentitySymbol.Spark, .05f, .205f, .337f,
                () => ShowPage(MoraHubPage.Feed));
            Tile("Play Tile", "Oyna", "Play", IdentitySymbol.Forward, .357f, .205f, .643f,
                () => ShowPage(MoraHubPage.Play));
            Tile("Decor Tile", "Dekor", "Decor", IdentitySymbol.Levels, .663f, .205f, .95f,
                () => ShowPage(MoraHubPage.Decor));

            Button("Growth Menu", transform, new Vector2(.07f, .123f),
                new Vector2(.475f, .192f), new Color32(192, 235, 216, 250),
                "", "GELİŞİM", "GROWTH", () => ShowPage(MoraHubPage.Growth));
            Button("Collection Menu", transform, new Vector2(.525f, .123f),
                new Vector2(.93f, .192f), new Color32(192, 235, 216, 250),
                "", "KOLEKSİYON", "COLLECTION", () => ShowPage(MoraHubPage.Collection));

            Button("Return to Chapters", transform, new Vector2(.20f, .034f),
                new Vector2(.80f, .101f), DeepTeal, "", "BÖLÜMLERE DÖN",
                "BACK TO CHAPTERS", exit);
        }

        private void Tile(string name, string tr, string en, IdentitySymbol symbol,
            float minX, float minY, float maxX, Action action)
        {
            var button = Button(name, transform,
                new Vector2(minX, minY), new Vector2(maxX, .298f),
                Ivory, "", tr, en, action);
            var label = button.GetComponentInChildren<Text>();
            label.fontStyle = FontStyle.Bold;
            label.rectTransform.anchorMin = new Vector2(.08f, .055f);
            label.rectTransform.anchorMax = new Vector2(.92f, .43f);
            IdentityIcon.Create("Action Motif", button.transform, symbol, Ocean,
                new Vector2(.38f, .49f), new Vector2(.62f, .90f));
        }

        private void BuildSections()
        {
            var root = PlaceholderVisuals.Rect("MORA Room Submenu", transform,
                Vector2.zero, Vector2.one);
            overlay = root.gameObject;
            var blocker = overlay.AddComponent<Image>();
            blocker.color = new Color(.025f, .13f, .18f, .83f);
            blocker.raycastTarget = true;
            var card = Surface("MORA Room Panel", root, Ivory,
                new Vector2(.075f, .12f), new Vector2(.925f, .86f));
            VisualTheme.Shadow(card, 8);
            pages = new GameObject[5];
            for (int i = 0; i < pages.Length; i++)
            {
                var section = (MoraHubPage)(i + 1);
                var view = PlaceholderVisuals.Rect(section + " View", card.transform,
                    Vector2.zero, Vector2.one);
                pages[i] = view.gameObject;
                var heading = Label(section + " Heading", view, "", 54, DeepTeal,
                    new Vector2(.08f, .85f), new Vector2(.92f, .94f),
                    () => TitleFor(section, Turkish));
                heading.fontStyle = FontStyle.Bold;
                Button(section + " Close", view, new Vector2(.23f, .035f),
                    new Vector2(.77f, .115f), DeepTeal, "", "MORA'YA DÖN",
                    "RETURN TO MORA", () => ShowPage(MoraHubPage.Home));
            }

            BuildFeed(pages[0].transform);
            BuildPlay(pages[1].transform);
            BuildDecor(pages[2].transform);
            BuildGrowth(pages[3].transform);
            BuildCollection(pages[4].transform);
            ShowPage(MoraHubPage.Home);
        }

        private void BuildFeed(Transform parent)
        {
            Label("Feed Note", parent, "", 30, VisualTheme.Muted,
                new Vector2(.10f, .745f), new Vector2(.90f, .84f),
                () => Turkish ? "MORA'ya küçük bir hediye sun."
                              : "Give MORA a little treat.");
            var fruit = Surface("Golden Berry", parent, Gold, new Vector2(.36f, .51f),
                new Vector2(.64f, .69f));
            feedToken = fruit.rectTransform;
            Label("Berry Mark", parent, "●", 90, Ocean, new Vector2(.37f, .53f),
                new Vector2(.63f, .68f));
            feedMessage = Label("Feed Result", parent, "", 30, DeepTeal,
                new Vector2(.08f, .37f), new Vector2(.92f, .48f));
            RefreshFeedMessage();
            Button("Give Berry", parent, new Vector2(.20f, .24f),
                new Vector2(.80f, .335f), Ocean, "", "MEYVE VER",
                "GIVE BERRY", Feed);
            Label("Feed Preview Notice", parent, "", 23, VisualTheme.Muted,
                new Vector2(.09f, .145f), new Vector2(.91f, .205f),
                () => Turkish ? "Deneme etkileşimi · Enerji veya ödül verilmez"
                              : "Preview interaction · No energy or rewards");
        }

        private void BuildPlay(Transform parent)
        {
            Label("Play Note", parent, "", 30, VisualTheme.Muted,
                new Vector2(.09f, .755f), new Vector2(.91f, .835f),
                () => Turkish ? "MORA ile kısa bir oyun oyna."
                              : "Enjoy a quick moment with MORA.");
            var ball = Surface("Play Ball", parent, SeaGlass,
                new Vector2(.355f, .505f), new Vector2(.645f, .70f));
            playToken = ball.rectTransform;
            Label("Ball Mark", parent, "★", 78, DeepTeal,
                new Vector2(.37f, .53f), new Vector2(.63f, .665f));
            playMessage = Label("Play Result", parent, "", 29, DeepTeal,
                new Vector2(.10f, .37f), new Vector2(.90f, .48f));
            RefreshPlayMessage();
            Button("Bounce Ball", parent, new Vector2(.20f, .24f),
                new Vector2(.80f, .335f), Ocean, "", "TOPU AT",
                "TOSS THE BALL", Play);
            Label("Play Preview Notice", parent, "", 23, VisualTheme.Muted,
                new Vector2(.09f, .145f), new Vector2(.91f, .205f),
                () => Turkish ? "Deneme etkileşimi · Mini oyun daha sonra"
                              : "Preview interaction · Mini-game comes later");
        }

        private void BuildDecor(Transform parent)
        {
            Label("Decor Note", parent, "", 28, VisualTheme.Muted,
                new Vector2(.07f, .75f), new Vector2(.93f, .845f),
                () => Turkish ? "MORA'nın yuvası da zamanla canlanacak."
                              : "MORA's habitat will come alive over time.");
            Picture("Current Habitat", parent, stageArt[0],
                new Vector2(.17f, .34f), new Vector2(.83f, .73f));
            Label("Decor Status", parent, "", 30, DeepTeal,
                new Vector2(.09f, .235f), new Vector2(.91f, .35f),
                () => Turkish ? "Mevcut habitat · Yeni dekorlar hazırlanıyor"
                              : "Current habitat · More decor is in development");
            Label("Decor Preview Notice", parent, "", 23, VisualTheme.Muted,
                new Vector2(.10f, .14f), new Vector2(.90f, .21f),
                () => Turkish ? "Henüz satın alma ve envanter sistemi yok"
                              : "Purchases and inventory are not available yet");
        }

        private void BuildGrowth(Transform parent)
        {
            Label("Growth Note", parent, "", 28, VisualTheme.Muted,
                new Vector2(.07f, .77f), new Vector2(.93f, .85f),
                () => Turkish ? "Dört form; ilerleme yeni bölümlerle açılacak."
                              : "Four forms; future chapters will unlock growth.");
            for (int i = 0; i < 4; i++)
            {
                int selected = i + 1;
                float left = .06f + i * .225f;
                var item = Button("Growth Stage " + selected, parent,
                    new Vector2(left, .49f), new Vector2(left + .205f, .735f),
                    selected == 1 ? new Color32(204, 234, 207, 255)
                                  : new Color32(231, 238, 218, 255),
                    "", selected.ToString(), selected.ToString(),
                    () => PreviewStage(selected));
                var caption = item.GetComponentInChildren<Text>();
                caption.rectTransform.anchorMin = new Vector2(.08f, .04f);
                caption.rectTransform.anchorMax = new Vector2(.92f, .24f);
                Sprite asset = stageArt[i];
                if (asset != null) Picture("Form Art " + selected, item.transform, asset,
                    new Vector2(.06f, .26f), new Vector2(.94f, .97f));
            }
            growthThumbnail = Picture("Selected Growth Artwork", parent, stageArt[0],
                new Vector2(.22f, .235f), new Vector2(.78f, .48f));
            growthMessage = Label("Growth Stage Status", parent, "", 27, DeepTeal,
                new Vector2(.07f, .13f), new Vector2(.93f, .225f));
            RefreshGrowthMessage();
        }

        private void BuildCollection(Transform parent)
        {
            Label("Collection Note", parent, "", 27, VisualTheme.Muted,
                new Vector2(.07f, .765f), new Vector2(.93f, .845f),
                () => Turkish ? "Karakterler zamanla koleksiyonuna katılacak."
                              : "More companions will join your collection.");
            var avatarCard = Surface("Owned MORA", parent,
                new Color32(205, 233, 209, 255),
                new Vector2(.11f, .505f), new Vector2(.89f, .735f));
            Picture("MORA Owned Avatar", avatarCard.transform, baseSeed ?? stageArt[0],
                new Vector2(.04f, .10f), new Vector2(.46f, .93f));
            var owned = Label("Owned MORA Status", avatarCard.transform, "", 30, DeepTeal,
                new Vector2(.44f, .08f), new Vector2(.96f, .92f),
                () => Turkish ? "MORA\nİlk yol arkadaşın"
                              : "MORA\nYour first companion");
            owned.fontStyle = FontStyle.Bold;
            for (int i = 0; i < 2; i++)
            {
                float left = .11f + i * .41f;
                var locked = Surface("Future Companion " + (i + 1), parent,
                    new Color32(213, 226, 217, 255),
                    new Vector2(left, .265f), new Vector2(left + .37f, .47f));
                Label("Future Character Symbol", locked.transform, "?", 73,
                    Ocean, new Vector2(.10f, .30f), new Vector2(.90f, .93f));
                Label("Future Character Label", locked.transform, "", 24, DeepTeal,
                    new Vector2(.05f, .08f), new Vector2(.95f, .30f),
                    () => Turkish ? "Henüz Kilitli" : "Not Yet Unlocked");
            }
            Label("Collection Preview Notice", parent, "", 23, VisualTheme.Muted,
                new Vector2(.07f, .16f), new Vector2(.93f, .24f),
                () => Turkish ? "Özel karakter ödülleri daha sonra eklenecek"
                              : "Special companion rewards are coming later");
        }

        private Image Picture(string name, Transform parent, Sprite sprite,
            Vector2 min, Vector2 max)
        {
            var image = PlaceholderVisuals.Rect(name, parent, min, max)
                .gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.enabled = sprite != null;
            image.preserveAspect = true;
            image.raycastTarget = false;
            return image;
        }

        private Image Surface(string name, Transform parent, Color color,
            Vector2 min, Vector2 max)
        {
            var image = VisualTheme.Surface(name, parent, rounded, color, min, max);
            image.raycastTarget = false;
            return image;
        }

        private Text Label(string name, Transform parent, string value, int size,
            Color color, Vector2 min, Vector2 max, Func<string> translated = null)
        {
            var label = PlaceholderVisuals.Label(name, parent, font, value, size, color, min, max);
            if (translated != null) LocalizedLabel.Bind(label, translated);
            return label;
        }

        private Button Button(string name, Transform parent, Vector2 min, Vector2 max,
            Color color, string glyph, string tr, string en, Action click)
        {
            var rect = PlaceholderVisuals.Rect(name, parent, min, max);
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = rounded;
            image.type = Image.Type.Sliced;
            image.color = color;
            image.raycastTarget = true;
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.transition = Selectable.Transition.ColorTint;
            button.onClick.AddListener(() => { if (click != null) click(); });
            var title = Label(name + " Label", rect, "", 29, DeepTeal,
                new Vector2(.045f, .05f), new Vector2(.955f, .95f),
                () => Turkish ? tr : en);
            if (color == DeepTeal || color == Ocean) title.color = Ivory;
            title.fontStyle = FontStyle.Bold;
            var press = rect.gameObject.AddComponent<PresentationMotion>();
            press.Settings = feel;
            return button;
        }

        public void SetGrowth(MoraGrowthSnapshot growth)
        {
            snapshot = growth; // Read only; never writes a save or grants a level reward.
            RefreshGrowthMessage();
        }

        public void Open()
        {
            if (isOpen) return;
            isOpen = true;
            feedTaps = playTaps = touches = 0;
            growthInfoStage = 1;
            feedMotion = playMotion = 0f;
            if (feedToken != null) feedToken.localScale = Vector3.one;
            if (playToken != null) playToken.localScale = Vector3.one;
            nextTapTime = float.NegativeInfinity;
            fadeTime = reactionTime = 0f;
            gameObject.SetActive(true);
            ShowStageInstant(1);
            ShowPage(MoraHubPage.Home);
            RefreshGreeting();
            RefreshFeedMessage();
            RefreshPlayMessage();
        }

        public void Close()
        {
            if (!isOpen) return;
            isOpen = false;
            ShowPage(MoraHubPage.Home);
            ShowStageInstant(1);
            reactionTime = 0f;
            gameObject.SetActive(false);
        }

        private void ShowPage(MoraHubPage target)
        {
            page = target;
            if (overlay == null || pages == null) return;
            overlay.SetActive(target != MoraHubPage.Home);
            for (int i = 0; i < pages.Length; i++)
                pages[i].SetActive(target == (MoraHubPage)(i + 1));
        }

        private void TouchMora()
        {
            if (Time.unscaledTime < nextTapTime) return;
            nextTapTime = Time.unscaledTime + .28f;
            touches++;
            reactionTime = .56f;
            RefreshGreeting();
        }

        private void Feed()
        {
            feedTaps++;
            reactionTime = .56f;
            feedMotion = .48f;
            RefreshFeedMessage();
        }

        private void Play()
        {
            playTaps++;
            reactionTime = .56f;
            playMotion = .48f;
            RefreshPlayMessage();
        }

        private void RefreshGreeting()
        {
            if (greeting == null) return;
            LocalizedLabel.Bind(greeting, () =>
                touches == 0 ? (Turkish ? "MORA'ya dokun, seni selamlasın." : "Tap MORA to say hello.")
                : touches % 2 == 1 ? (Turkish ? "MORA seni gördüğüne sevindi!" : "MORA is happy to see you!")
                : (Turkish ? "MORA seni merakla izliyor." : "MORA is watching you curiously."));
        }

        private void RefreshFeedMessage()
        {
            if (feedMessage == null) return;
            LocalizedLabel.Bind(feedMessage, () => feedTaps <= 0
                ? (Turkish ? "MORA meyveyi merakla bekliyor." : "MORA is curious about the treat.")
                : (Turkish ? "MORA hediyene sevindi!" : "MORA loved your little gift!"));
        }

        private void RefreshPlayMessage()
        {
            if (playMessage == null) return;
            LocalizedLabel.Bind(playMessage, () => playTaps <= 0
                ? (Turkish ? "Birlikte kısa bir oyun oynayalım." : "Let's play together for a moment.")
                : (Turkish ? "MORA seninle oynadığına sevindi!" : "MORA enjoyed playing with you!"));
        }

        private void RefreshGrowthMessage()
        {
            if (growthMessage == null) return;
            LocalizedLabel.Bind(growthMessage, () =>
            {
#if UNITY_EDITOR
                if (stage > 1) return Turkish
                    ? "Görsel test · Gerçek oyuncu gelişimi değişmez"
                    : "Art preview · Player progress is unchanged";
#endif
                if (growthInfoStage > 1) return Turkish
                    ? "Evre " + growthInfoStage + " henüz kilitli · Yeni bölümler gerekli"
                    : "Stage " + growthInfoStage + " is locked · More chapters required";
                return snapshot.HasPostFortyLevels
                    ? (Turkish ? "Yeni evreler ileri bölümlerle açılacak"
                               : "Later chapters will unlock new forms")
                    : (Turkish ? "Gelişim Level 41 sonrasında başlayacak"
                               : "Growth begins after Level 40");
            });
        }

        private void PreviewStage(int nextStage)
        {
            growthInfoStage = Mathf.Clamp(nextStage, 1, 4);
#if UNITY_EDITOR
            if (Application.isEditor && stageArt[growthInfoStage - 1] != null)
                SetStage(growthInfoStage); // Art inspection, never gameplay unlocking.
#endif
            // In non-editor builds the selection still explains the locked stage.
            RefreshGrowthMessage();
        }

        private void SetStage(int nextStage)
        {
            nextStage = Mathf.Clamp(nextStage, 1, 4);
            if (stageArt[nextStage - 1] == null || portrait == null || transition == null) return;
            if (stage == nextStage && !inTransition) return;
            if (inTransition && transition.color.a > .5f)
                portrait.sprite = transition.sprite;
            stage = nextStage;
            transition.sprite = stageArt[stage - 1];
            fadeTime = 0f;
            transition.color = new Color(1f, 1f, 1f, 0f);
            inTransition = true;
            if (feel != null && feel.reducedMotion) FinishBlend();
            if (growthThumbnail != null) growthThumbnail.sprite = stageArt[stage - 1];
        }

        private void ShowStageInstant(int nextStage)
        {
            stage = nextStage;
            inTransition = false;
            if (portrait != null) portrait.sprite = stageArt[stage - 1];
            if (transition != null) transition.color = new Color(1f, 1f, 1f, 0f);
            if (portraitRect != null) portraitRect.localScale = Vector3.one;
            if (growthThumbnail != null) growthThumbnail.sprite = stageArt[stage - 1];
            RefreshGrowthMessage();
        }

        private void FinishBlend()
        {
            if (portrait != null && transition != null)
            {
                portrait.sprite = transition.sprite;
                transition.color = new Color(1f, 1f, 1f, 0f);
            }
            inTransition = false;
        }

        private void Update()
        {
            if (!isOpen || portraitRect == null) return;
            float dt = Time.unscaledDeltaTime;
            bool reduced = feel != null && feel.reducedMotion;
            if (inTransition)
            {
                if (reduced) FinishBlend();
                else
                {
                    fadeTime += dt;
                    float opacity = Mathf.SmoothStep(0f, 1f,
                        Mathf.Clamp01(fadeTime / .35f));
                    transition.color = new Color(1f, 1f, 1f, opacity);
                    if (fadeTime >= .35f) FinishBlend();
                }
            }
            reactionTime = Mathf.Max(0f, reactionTime - dt);
            feedMotion = Mathf.Max(0f, feedMotion - dt);
            playMotion = Mathf.Max(0f, playMotion - dt);
            if (feedToken != null)
            {
                float amount = reduced ? 0f : .10f * Mathf.Sin(Mathf.PI *
                    (1f - feedMotion / .48f)) * (feedMotion > 0f ? 1f : 0f);
                feedToken.localScale = Vector3.one * (1f + amount);
            }
            if (playToken != null)
            {
                float amount = reduced ? 0f : .10f * Mathf.Sin(Mathf.PI *
                    (1f - playMotion / .48f)) * (playMotion > 0f ? 1f : 0f);
                playToken.localScale = Vector3.one * (1f + amount);
            }
            float reaction = reduced || reactionTime <= 0f ? 0f
                : .045f * Mathf.Sin(Mathf.PI * (1f - reactionTime / .56f));
            float breathing = reduced ? 0f : .008f * Mathf.Sin(Time.unscaledTime * 1.4f);
            portraitRect.localScale = Vector3.one * (1f + reaction + breathing);
        }

        private void OnDisable()
        {
            reactionTime = 0f;
            if (portraitRect != null) portraitRect.localScale = Vector3.one;
            if (feedToken != null) feedToken.localScale = Vector3.one;
            if (playToken != null) playToken.localScale = Vector3.one;
            if (inTransition) FinishBlend();
        }
    }
}
