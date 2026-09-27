using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Shift.Game
{
    public sealed class PrototypeGame : MonoBehaviour
    {
        [SerializeField] private LevelData level;
        [SerializeField] private bool useLevelSet = true;
        [SerializeField] private LevelData[] levels;
        [SerializeField, Range(0, 39)] private int selectedLevel;
#if UNITY_EDITOR
        [SerializeField] private string saveKeyPrefix = "SHIFT.Progress.v1.";
        [SerializeField] private string settingsKeyPrefix = "SHIFT.Settings.v1.";
#endif
        [SerializeField] private GameFeelSettings gameFeel = new GameFeelSettings();
        [SerializeField] private AudioClips audioClips = new AudioClips();
        [SerializeField] private bool analyticsEnabled = true;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        [SerializeField] private bool verboseTelemetry;
        [SerializeField] private bool showTelemetryOverlay;
#endif
        public TelemetryTracker Telemetry { get; private set; }
        public SettingsService Settings { get; private set; }
        private SettingsPanel settingsPanel;
        private FirstLaunchOnboardingService onboarding;
        public bool IsOnboardingActive { get; private set; }
        public bool IsStartupWaiting { get; private set; }
        public int ContinueLevelIndex => CampaignContinuation.LevelIndex(progress);
        private string OnboardingKey
        {
            get
            {
#if UNITY_EDITOR
                if (saveKeyPrefix != "SHIFT.Progress.v1.") return saveKeyPrefix + "Onboarding.v1";
#endif
                return FirstLaunchOnboardingService.SaveKey;
            }
        }
        private CampaignEndingState campaignEnding;
        private CampaignEndingPanel endingPanel;
        private bool endingPending, endingRequested;
        private string EndingKey
        {
            get
            {
#if UNITY_EDITOR
                if (saveKeyPrefix != "SHIFT.Progress.v1.") return saveKeyPrefix + "CampaignEnding.Seen.v1";
#endif
                return CampaignEndingState.SaveKey;
            }
        }
        public bool IsCampaignEndingPending => endingPending;
        private int CampaignPerfectCount => progress == null ? 0 : progress.ChapterSummary(0).Perfect + progress.ChapterSummary(1).Perfect;

        private HintSession hints;
        private DailyAllowanceService allowances;
        public DailyAllowanceService Allowances => allowances ??= new DailyAllowanceService(
            Resources.Load<DailyAllowanceConfig>("DailyAllowanceConfig"), () => LocalNow(), AllowanceKey);
        public HintSession Hints => hints ??= new HintSession(Allowances);
        private string AllowanceKey
        {
            get
            {
#if UNITY_EDITOR
                if (saveKeyPrefix != "SHIFT.Progress.v1.") return saveKeyPrefix + "DailyAllowance.v1";
#endif
                return DailyAllowanceService.SaveKey;
            }
        }
        private RewardReason recoveryReason;
        private IRewardedAdService rewardedAds;
        public IRewardedAdService RewardedAds { get => rewardedAds ??= RewardedAdsBootstrap.Shared; set => rewardedAds = value; }
        private RewardedHintPanel hintPanel;
        private int rewardGeneration;
        private bool rewardPending;
        public System.Func<System.DateTime> LocalNow { get; set; } = () => System.DateTime.Now;
        public HintLanguage DailyLanguage { get => GameLanguageService.Shared.HintLanguage; set => GameLanguageService.Shared.Select(value == HintLanguage.Turkish ? GameLanguage.Turkish : GameLanguage.English); }
        public DailyPool DailyPool { get; private set; }
        public DailySaveService DailySaves { get; private set; }
        public DailyPuzzle ActiveDaily { get; private set; }
        public bool IsDaily => ActiveDaily != null;
        private DailyShiftPanel dailyPanel;
        private System.DateTime panelDate;
        private bool InputModalOpen => endingRequested || (endingPanel != null && endingPanel.IsOpen) || IsStartupWaiting || IsOnboardingActive || (dailyPanel != null && dailyPanel.IsOpen) || (hintPanel != null && hintPanel.IsOpen) || (chapter != null && chapter.IsOpen) || (settingsPanel != null && settingsPanel.IsOpen);
        private readonly BoardManager board = new BoardManager();
        private BoardView view;
        private GameHud hud;
        private GameObject canvasObject;
        private GameObject eventSystemObject;
        private Sprite circle;
        private Sprite rounded;
        private AudioManager audioManager;
        private HapticService haptics;
        private LevelProgression progress;
        private ChapterSelect chapter;
        private bool debugLevel;
        private bool returningContext;
        private int activeLevelIndex;
        public LevelProgression Progress => progress;
        public BoardManager Board => board;
        public LevelData CurrentLevel => IsDaily ? ActiveDaily.Level : useLevelSet && levels != null && levels.Length > 0
            ? levels[Mathf.Clamp(activeLevelIndex, 0, levels.Length - 1)] : level;

        private void Start()
        {
#if UNITY_EDITOR
            if (saveKeyPrefix != "SHIFT.Progress.v1.")
                GameLanguageService.UseForValidation(new GameLanguageService(Application.systemLanguage, saveKeyPrefix + "Language.v1"));
#endif
            _ = GameLanguageService.Shared;
            _ = RewardedAds; // Persistent service, independent of levels and scene rebuilds.
            Application.targetFrameRate = 60;
            Screen.orientation = ScreenOrientation.Portrait;
            var cameraObject = new GameObject("Background Camera", typeof(Camera));
            cameraObject.transform.SetParent(transform, false);
            var backgroundCamera = cameraObject.GetComponent<Camera>();
            backgroundCamera.clearFlags = CameraClearFlags.SolidColor;
            backgroundCamera.backgroundColor = new Color32(209, 235, 240, 255);
            backgroundCamera.cullingMask = 0;
            cameraObject.AddComponent<AudioListener>();
            circle = PlaceholderVisuals.CreateCircle();
            rounded = PlaceholderVisuals.CreateRounded();
            audioManager = gameObject.AddComponent<AudioManager>(); audioManager.Initialize(audioClips);
            haptics = new HapticService(gameFeel);
#if UNITY_EDITOR
            Settings = new SettingsService(settingsKeyPrefix, gameFeel.reducedMotion, gameFeel.hapticsEnabled);
#else
            Settings = new SettingsService(defaultMotion: gameFeel.reducedMotion, defaultHaptics: gameFeel.hapticsEnabled);
#endif
            Settings.Apply(gameFeel, audioManager);
            var localAnalytics = new LocalAnalyticsService();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            localAnalytics.Verbose = verboseTelemetry;
#endif
            Telemetry = new TelemetryTracker(analyticsEnabled ? (IAnalyticsService)localAnalytics : new NullAnalyticsService(), () => Time.realtimeSinceStartupAsDouble);
            Telemetry.StartSession();
            if (useLevelSet && levels != null && levels.Length > 0)
            {
#if UNITY_EDITOR
                var saves = new SaveService(saveKeyPrefix);
#else
                var saves = new SaveService();
#endif
                progress = new LevelProgression(levels.Length, saves, levels); activeLevelIndex = selectedLevel = progress.Current;
                try { DailyPool = new DailyPool(levels); }
                catch (System.ArgumentException) { DailyPool = null; } // Incomplete campaign configuration: campaign remains usable.
#if UNITY_EDITOR
                DailySaves = new DailySaveService(saveKeyPrefix == "SHIFT.Progress.v1." ? "SHIFT.Daily.v1.History" : saveKeyPrefix + "Daily.v1.History");
#else
                DailySaves = new DailySaveService();
#endif
            }
            if (EventSystem.current == null)
            {
                eventSystemObject = new GameObject("SHIFT Input", typeof(EventSystem));
                eventSystemObject.AddComponent<InputSystemUIInputModule>();
            }
            campaignEnding = new CampaignEndingState(EndingKey);
            onboarding = new FirstLaunchOnboardingService(OnboardingKey, Telemetry.OnboardingEvent);
            if (!onboarding.Completed && progress != null) ShowOnboarding();
            else
            {
#if UNITY_EDITOR
                if (saveKeyPrefix != "SHIFT.Progress.v1." && UnityEditor.SessionState.GetBool("SHIFT.DirectGameplayValidation." + saveKeyPrefix, false)) { Rebuild("session_start", false); return; }
#endif
                IsStartupWaiting = true;
                StartupShell(null);
                StartCoroutine(ReturningStartup());
            }
        }

        private IEnumerator ReturningStartup()
        {
            // Cold launch only. Foreground/ad callbacks never enter this path.
            while ((RewardedAds as IPrivacyChoices)?.IsBusy ?? false) yield return null;
            IsStartupWaiting = false;
            if (progress != null)
                activeLevelIndex = selectedLevel = ContinueLevelIndex >= 0 ? ContinueLevelIndex : progress.Current;
            if (CurrentLevel == null || !CurrentLevel.ValidatePlayable(out _))
            { StartupShell("resume.no_levels"); yield break; }
            Rebuild("session_start", false);
            returningContext = true;
            OpenChapter();
        }

        private void StartupShell(string message)
        {
            if (canvasObject != null) { canvasObject.SetActive(false); Destroy(canvasObject); }
            canvasObject = new GameObject("SHIFT UI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080,1920); scaler.matchWidthOrHeight = .5f;
            VisualTheme.Background(canvasObject.transform, circle);
            if (message == null) return;
            var label = PlaceholderVisuals.Label("Campaign Unavailable", canvasObject.transform,
                Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"), "", 32, VisualTheme.Ink, new Vector2(.1f,.4f), new Vector2(.9f,.6f));
            LocalizedLabel.Bind(label, message);
        }

        public void ContinueCampaign()
        {
            if (IsStartupWaiting || IsOnboardingActive || progress == null) return;
            returningContext = false;
            if (ContinueLevelIndex >= 0) SelectLevel(ContinueLevelIndex);
            else { OpenChapter(); chapter?.ShowPage(CampaignContinuation.Chapter(progress)); }
        }

        private bool TryDailyDate(out System.DateTime date)
        {
            date = default;
            if (DailyPool == null || DailySaves == null) return false;
            try { date = LocalNow().Date; return date >= System.DateTime.MinValue.AddDays(6); }
            catch (System.Exception) { return false; } // A failed platform clock must not block campaign navigation.
        }
        private void RefreshChapterDaily()
        {
            if (chapter == null) return;
            bool available = TryDailyDate(out var date);
            panelDate = date;
            chapter.RefreshDaily(available ? DailySaves.Get(DailyPool.For(date)) : null, DailyLanguage);
        }

        private void ShowOnboarding()
        {
            if (IsOnboardingActive) return;
            IsOnboardingActive = true;
            canvasObject = new GameObject("SHIFT UI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080,1920); scaler.matchWidthOrHeight = .5f;
            VisualTheme.Background(canvasObject.transform, circle);
            var safe = PlaceholderVisuals.Rect("Safe Area", canvasObject.transform, Vector2.zero, Vector2.one);
            safe.gameObject.AddComponent<SafeArea>();
            var panel = safe.gameObject.AddComponent<FirstLaunchOnboardingPanel>();
            panel.Build(onboarding, Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"), circle, rounded, levels, gameFeel,
                () => (RewardedAds as IPrivacyChoices)?.IsBusy ?? false,
                () => { IsOnboardingActive = false; Rebuild("session_start", false); },
                () => audioManager.Play(AudioCue.UIButton));
        }

        public void Restart() { if (IsStartupWaiting || IsOnboardingActive || hud == null) return; Rebuild("restart", true); audioManager.Play(AudioCue.Restart); }
        private void Rebuild(string reason, bool restart)
        {
            endingPending = endingRequested = false;
            rewardGeneration++; rewardPending = false;
            StopAllCoroutines();
            audioManager.BeginLevel(); haptics.Cancel();
            if (canvasObject != null) { canvasObject.SetActive(false); Destroy(canvasObject); }
            var activeLevel = CurrentLevel;
            if (activeLevel == null || !activeLevel.ValidatePlayable(out _))
            { Debug.LogError("SHIFT requires a valid LevelData asset assigned to PrototypeGame.", this); return; }
            Telemetry.StartLevel(activeLevel, IsDaily ? ActiveDaily.SourceIndex : useLevelSet ? activeLevelIndex : -1, activeLevel.VerifiedOptimalMoveCount, restart, reason);
            board.Load(activeLevel);
            if(IsDaily)
            {
                var before=DailySaves.Get(ActiveDaily);
                if(restart)Telemetry.DailyEvent("daily_shift_replayed",ActiveDaily,before,LocalNow());
                Telemetry.DailyEvent("daily_shift_started",ActiveDaily,before,LocalNow());
            }
            canvasObject = new GameObject("SHIFT UI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920); scaler.matchWidthOrHeight = .5f;
            var background = PlaceholderVisuals.Rect("Background", canvasObject.transform, Vector2.zero, Vector2.one).gameObject.AddComponent<Image>();
            background.color = new Color32(209, 235, 240, 255); background.raycastTarget = false;
            VisualTheme.Background(canvasObject.transform, circle);
            var safe = PlaceholderVisuals.Rect("Safe Area", canvasObject.transform, Vector2.zero, Vector2.one);
            safe.gameObject.AddComponent<SafeArea>();
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            hud = safe.gameObject.AddComponent<GameHud>(); hud.Build(font, activeLevel, rounded, gameFeel, Replay, circle);
            hud.BuildNavigation(font, rounded, () => NextLevel(), OpenChapter, progress != null);
            hud.BuildUtilities(font, rounded, () => Undo(), OpenHint);
            var area = PlaceholderVisuals.Rect("Board Area", safe, new Vector2(.045f, .257f), new Vector2(.955f, .770f));
            var grid = PlaceholderVisuals.Rect("Board", area, Vector2.zero, Vector2.one);
            grid.gameObject.AddComponent<PanelArrival>().Initialize(gameFeel);
            view = grid.gameObject.AddComponent<BoardView>(); view.Build(board, font, circle, rounded, gameFeel, audioManager, haptics, hud, OnTap, activeLevel.Design?.sculptedTopology ?? false);
            // Compensate for the smaller board presentation without changing piece visuals or callbacks.
            foreach (var piece in grid.GetComponentsInChildren<Piece>())
            {
                var targetButton = piece.GetComponentInChildren<Button>();
                if (targetButton != null) targetButton.targetGraphic.raycastPadding = Vector4.one * -8;
            }
            if (progress != null)
            {
                var modal = PlaceholderVisuals.Rect("Chapter Select", safe, Vector2.zero, Vector2.one);
                chapter = modal.gameObject.AddComponent<ChapterSelect>();
                chapter.Settings = gameFeel;
                chapter.MasteryViewed = summary => Telemetry.MasteryEvent("level_mastery_viewed", summary);
                chapter.Build(font, rounded, levels.Length, index => SelectLevel(index), CloseChapter, ContinueCampaign);
                chapter.BuildDailyEntry(font,rounded,OpenDaily);
                RefreshChapterDaily();
                var dailyRect=PlaceholderVisuals.Rect("Daily Shift Panel",safe,Vector2.zero,Vector2.one);
                dailyPanel=dailyRect.gameObject.AddComponent<DailyShiftPanel>();
                dailyPanel.Build(font,rounded,gameFeel,(p,archive)=>StartDaily(p.Date,archive),CloseDaily);
                if(IsDaily)hud.BuildDaily(font,rounded,ActiveDaily,DailyLanguage,OpenDaily);
            }
            var settingsRect = PlaceholderVisuals.Rect("Settings Panel", safe, Vector2.zero, Vector2.one);
            settingsPanel = settingsRect.gameObject.AddComponent<SettingsPanel>();
            settingsPanel.Build(font, rounded, Settings, gameFeel, audioManager, CloseSettings);
            var hintRect = PlaceholderVisuals.Rect("Rewarded Hint Panel", safe, Vector2.zero, Vector2.one);
            hintPanel = hintRect.gameObject.AddComponent<RewardedHintPanel>();
            hintPanel.Build(font, rounded, gameFeel, RequestRewardedHint, CloseHint);
            LayoutControls.Settings(safe, circle, gameFeel, OpenSettings);
            foreach (var button in canvasObject.GetComponentsInChildren<Button>(true))
                if (button.GetComponentInParent<Piece>() == null && button.name != "Undo" && button.name != "Hint" && button.name != "Restart" && button.name != "Replay" && button.name != "Open Chapter" && button.name != "Open Settings" && button.name != "Close Settings" && button.name != "Close Chapter")
                    button.onClick.AddListener(() => audioManager.Play(AudioCue.UIButton));
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (showTelemetryOverlay)
            {
                var overlay = PlaceholderVisuals.Rect("Telemetry Overlay", safe, new Vector2(.06f,.77f), new Vector2(.94f,.89f));
                overlay.gameObject.AddComponent<TelemetryOverlay>().Initialize(Telemetry, font);
            }
#endif
            hud.Refresh(board); RefreshControls();
        }

        private void OnTap(GridPosition position)
        {
            if (InputModalOpen) return;
            var tapped = board.GetOccupant(position);
            if (!board.RequestMove(position)) return;
            Telemetry.StateActions(board.Actions);
            Telemetry.AcceptedTap(board.ReactionDepth, board.MovesRemaining, board.LastReactionWasCancelled);
            view.SetInput(false); hud.BeginReaction(); hud.Refresh(board); RefreshControls();
            view.TapFeedback(tapped.Id, board.Actions.Count == 0, board.Actions.Count);
            StartCoroutine(Resolve(tapped.Id));
        }

        private IEnumerator Resolve(int tappedId)
        {
            yield return view.Play(board, tappedId, FinalTargetDelivery());
            board.CompleteResolution();
            if (board.State == GameState.Won || board.State == GameState.Lost) Telemetry.Finish(board.State == GameState.Won);
            bool firstPerfect = false, chapterMasteredNow = false;
            int chapterIndex = CampaignChapters.Index(activeLevelIndex);
            if (board.State == GameState.Won && progress != null && !debugLevel && !IsDaily)
            {
                bool wasMastered = progress.ChapterSummary(chapterIndex).Mastered;
                if (progress.Complete(activeLevelIndex)) firstPerfect = progress.RecordPerfect(activeLevelIndex, Telemetry.Result);
                var summary = progress.ChapterSummary(chapterIndex);
                if (firstPerfect) Telemetry.MasteryEvent("perfect_shift_first_earned", summary, true);
                chapterMasteredNow = !wasMastered && summary.Mastered;
                if (chapterMasteredNow) Telemetry.MasteryEvent("chapter_mastery_completed", summary, true);
            }
            if(board.State==GameState.Won&&IsDaily)
            {
                var before=DailySaves.Get(ActiveDaily);
                if(DailySaves.Record(ActiveDaily,Telemetry.Result,out bool firstComplete,out bool firstDailyPerfect))
                {
                    if(firstComplete)Telemetry.DailyEvent("daily_shift_completed",ActiveDaily,before,LocalNow());
                    if(firstDailyPerfect)Telemetry.DailyEvent("daily_shift_perfect",ActiveDaily,before,LocalNow());
                }
            }
            if (chapter != null && chapter.IsOpen) chapter.Open(progress);
            hud.Refresh(board); RefreshControls();
            hud.SetCompletion(board.State == GameState.Won, !IsDaily && progress != null && CampaignChapters.IsLast(activeLevelIndex, levels.Length),
                !IsDaily && progress != null && !debugLevel && activeLevelIndex < levels.Length - 1, chapterIndex + 1);
            hud.FinishChain(); audioManager.ChainPayoff(board.Actions.Count); view.SetInput(board.State == GameState.Playing && !InputModalOpen);
            if (board.State == GameState.Won && progress != null && !debugLevel && !IsDaily && CampaignChapters.IsLast(activeLevelIndex, levels.Length))
                hud.ShowChapterMastery(progress.ChapterSummary(chapterIndex));
            if(IsDaily)hud.ShowDailyCompletion(board.State==GameState.Won,Telemetry.Result?.perfectShift ?? false,DailyLanguage);
            else hud.ShowMastery(Telemetry.Result, firstPerfect);
            if (board.State == GameState.Won || board.State == GameState.Lost)
            {
                if (board.State == GameState.Won) view.SuccessPulse();
                audioManager.PlayOutcome(AudioManager.Outcome(board.State == GameState.Won,
                    Telemetry.Result?.perfectShift ?? false, firstPerfect, IsDaily, chapterMasteredNow), haptics);
            }
            else haptics.EndReaction();
            if (campaignEnding.Eligible(activeLevelIndex, board.State == GameState.Won, IsDaily, debugLevel,
                progress != null && progress.LevelCount == 40 && progress.ChapterComplete))
            {
                endingPending = true;
                hud.ShowCampaignComplete(Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"), rounded,
                    Telemetry.Result?.perfectShift ?? false, OpenCampaignEnding);
            }
        }

        public void OpenCampaignEnding()
        {
            if (!endingPending || endingRequested || (endingPanel != null && endingPanel.IsOpen)) return;
            endingRequested = true;
            Telemetry.CampaignEndingEvent("campaign_ending_continue", CampaignPerfectCount);
            TryRevealEnding();
        }
        private void TryRevealEnding()
        {
            if (!endingRequested || ((RewardedAds as IPrivacyChoices)?.IsBusy ?? false)) return;
            if (campaignEnding.Seen) { endingRequested = endingPending = false; return; }
            endingRequested = endingPending = false;
            chapter?.Close(); dailyPanel?.Close(); settingsPanel?.Close(); hintPanel?.Close(); view.SetInput(false);
            var rect = PlaceholderVisuals.Rect("Campaign Ending",hud.transform,Vector2.zero,Vector2.one);
            endingPanel = rect.gameObject.AddComponent<CampaignEndingPanel>();
            endingPanel.Build(Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"),rounded,circle,gameFeel,CampaignPerfectCount,
                () => EndingDestination("mastery"), () => EndingDestination("daily"), () => EndingDestination("levels"));
            endingPanel.Open();
            campaignEnding.MarkSeen();
            Telemetry.CampaignEndingEvent("campaign_ending_viewed",CampaignPerfectCount);
            audioManager.Play(AudioCue.Win);haptics.Success();RefreshControls();
        }
        private void EndingDestination(string destination)
        {
            if (endingPanel == null || !endingPanel.IsOpen) return;
            endingPanel.Close();
            hud.EndCampaignPresentation(progress.ChapterSummary(1));
            if (destination == "daily") { Telemetry.CampaignEndingEvent("campaign_ending_daily_selected",CampaignPerfectCount);OpenDaily(); }
            else
            {
                if (destination == "mastery") Telemetry.CampaignEndingEvent("campaign_ending_mastery_selected",CampaignPerfectCount);
                OpenChapter();
            }
        }

        private int FinalTargetDelivery()
        {
            bool Target(PieceColor color)
            { foreach (var target in CurrentLevel.ResolvedTargetColors) if (target == color) return true; return false; }
            foreach (var piece in board.Pieces)
                if (piece.Active && piece.Type == PieceType.Normal && Target(piece.Color)) return -1;
            for (int i = board.Actions.Count - 1; i >= 0; i--)
                if (board.Actions[i].Type == BoardActionType.Deliver && Target(board.Pieces[board.Actions[i].PieceId].Color)) return i;
            return -1;
        }

        public bool SelectLevel(int index)
        {
            if (IsStartupWaiting || IsOnboardingActive || !ValidLevel(index) || progress == null || !progress.Select(index)) return false;
            returningContext = false; ActiveDaily = null; debugLevel = false;
            activeLevelIndex = selectedLevel = index; useLevelSet = true;
            Rebuild("select", false); Telemetry.SelectLevel();
            return true;
        }
        public bool NextLevel()
        {
            if (IsDaily || debugLevel || board.State != GameState.Won || progress == null || !ValidLevel(activeLevelIndex + 1) || !progress.Next()) return false;
            activeLevelIndex = selectedLevel = progress.Current; Rebuild("next", false); return true;
        }
        private bool ValidLevel(int index) => levels != null && index >= 0 && index < levels.Length && levels[index] != null && levels[index].ValidatePlayable(out _);
        private void Replay()
        {
            if (progress != null && !debugLevel && !IsDaily) progress.Select(activeLevelIndex);
            Restart();
        }
        private void RefreshControls() => hud?.RefreshUtilities(board, Hints.Remaining, InputModalOpen, Allowances.UndosRemaining);
        public bool Undo()
        {
            if (InputModalOpen || !board.CanUndo) { RefreshControls(); return false; }
            if (Allowances.UndosRemaining == 0) { OpenRecovery(RewardReason.Undo); return false; }
            if (!Allowances.TryUndo(board.Undo)) { RefreshControls(); return false; }
            audioManager.RearmOutcome(); audioManager.Play(AudioCue.Undo); haptics.Medium();
            view.RestoreFromModel(); view.SetInput(true); hud.BeginReaction(); hud.Refresh(board);
            hud.SetCompletion(false, false, false); Telemetry.UndoUsed(board.MovesRemaining, Hints.Stage(CurrentLevel), Hints.Remaining); RefreshControls();
            return true;
        }
        private void HintEvent(string name) => Telemetry.Utility(name, Hints.Stage(CurrentLevel), Hints.Remaining, board.MovesRemaining);
        public void OpenHint()
        {
            if (InputModalOpen || board.State != GameState.Playing) return;
            HintEvent("hint_opened");
            if (!Hints.HasNext(CurrentLevel)) { hud.NoMoreHints(DailyLanguage); RefreshControls(); return; }
            if (Hints.Remaining > 0) { UseHint(); return; }
            HintEvent("hint_exhausted"); OpenRecovery(RewardReason.Hint);
        }
        private void UseHint()
        {
            if (!Hints.TryUse(CurrentLevel, out string text)) return;
            audioManager.Play(AudioCue.Hint); haptics.Light();
            hud.ShowHint(text, Hints.Stage(CurrentLevel), CurrentLevel); HintEvent("hint_used"); RefreshControls();
        }
        public void CloseHint()
        {
            rewardGeneration++; rewardPending = false; hintPanel.Close();
            view.SetInput(board.State == GameState.Playing && !InputModalOpen); RefreshControls();
        }
        private bool RecoveryValid => recoveryReason == RewardReason.Hint ?
            board.State == GameState.Playing && Hints.HasNext(CurrentLevel) : board.CanUndo;
        private void OpenRecovery(RewardReason reason)
        {
            recoveryReason = reason;
            hintPanel.Open(Allowances.RecoveryEnabled(reason) && RewardedAds.IsRewardedAdAvailable,
                reason, Allowances.RewardAmount(reason), DailyLanguage);
            view.SetInput(false); RefreshControls();
        }
        public void RequestRewardedHint()
        {
            if (hintPanel == null || !hintPanel.IsOpen || rewardPending) return;
            if (!RecoveryValid) { CloseHint(); return; }
            // Midnight while a modal was open: use the fresh free allowance instead of showing an ad.
            if ((recoveryReason == RewardReason.Hint ? Allowances.HintsRemaining : Allowances.UndosRemaining) > 0)
            { var reason = recoveryReason; CloseHint(); if (reason == RewardReason.Hint) UseHint(); return; }
            var requestedReason = recoveryReason;
            string eventPrefix = requestedReason == RewardReason.Hint ? "rewarded_hint_" : "rewarded_undo_";
            HintEvent(eventPrefix + "requested");
            if (!Allowances.RecoveryEnabled(requestedReason) || !RewardedAds.IsRewardedAdAvailable)
            { HintEvent(eventPrefix + "failed"); hintPanel.Unavailable(); return; }
            string requestDate = Allowances.DateKey;
            rewardPending = true; int generation = ++rewardGeneration; hintPanel.Pending();
            bool handled = false;
            void Completed(bool success)
            {
                if (handled) return; handled = true;
                if (this == null || generation != rewardGeneration) return;
                rewardPending = false;
                bool granted = success && RecoveryValid && Allowances.Grant(requestedReason, requestDate);
                HintEvent(eventPrefix + (granted ? "completed" : "failed"));
                if (!granted) { hintPanel.Unavailable(); RefreshControls(); return; }
                CloseHint();
                if (requestedReason == RewardReason.Hint) UseHint();
            }
            try { RewardedAds.ShowRewardedAd(requestedReason, Completed); }
            catch (System.Exception) { Completed(false); }
        }
        public void OpenChapter()
        {
            if (chapter == null || rewardPending) return;
            if (hintPanel != null && hintPanel.IsOpen) CloseHint();
            dailyPanel?.Close(); settingsPanel?.Close(); view.SetInput(false); chapter.Open(progress);
            RefreshChapterDaily(); audioManager.Play(AudioCue.PanelOpen); RefreshControls();
        }
        public void CloseChapter()
        {
            if (chapter == null) return;
            if (returningContext && ContinueLevelIndex >= 0) { ContinueCampaign(); return; }
            returningContext = false;
            chapter.Close(); view.SetInput(board.State == GameState.Playing && !InputModalOpen); audioManager.Play(AudioCue.PanelClose); RefreshControls();
        }

        public void OpenSettings()
        {
            if (IsStartupWaiting || IsOnboardingActive || settingsPanel == null || rewardPending) return;
            if (hintPanel != null && hintPanel.IsOpen) CloseHint();
            dailyPanel?.Close(); chapter?.Close(); view.SetInput(false); settingsPanel.Open(); audioManager.Play(AudioCue.PanelOpen); RefreshControls();
        }
        public void CloseSettings()
        {
            settingsPanel.Close(); view.SetInput(board.State == GameState.Playing && !InputModalOpen); audioManager.Play(AudioCue.PanelClose); RefreshControls();
        }
        public void OpenDaily()
        {
            if(IsStartupWaiting||IsOnboardingActive||!TryDailyDate(out var date)||rewardPending||board.State==GameState.Resolving)return;
            chapter?.Close();settingsPanel?.Close();if(hintPanel!=null&&hintPanel.IsOpen)CloseHint();
            bool wasOpen=dailyPanel.IsOpen;
            panelDate=date;dailyPanel.Open(DailyPool,DailySaves,panelDate,DailyLanguage);
            var today=DailyPool.For(panelDate);var before=DailySaves.Get(today);
            if(!wasOpen){Telemetry.DailyEvent("daily_shift_opened",today,before,panelDate);Telemetry.DailyEvent("daily_archive_opened",today,before,panelDate);}
            view.SetInput(false);RefreshControls();
        }
        public void CloseDaily()
        {
            dailyPanel?.Close();view.SetInput(board.State==GameState.Playing&&!InputModalOpen);RefreshControls();
        }
        public bool StartDaily(System.DateTime date,bool archive=false)
        {
            if(IsStartupWaiting||IsOnboardingActive||!TryDailyDate(out var today)||rewardPending||board.State==GameState.Resolving)return false;
            date=date.Date;
            if(date>today||date<today.AddDays(-6)){OpenDaily();return false;}
            var puzzle=DailyPool.For(date);var before=DailySaves.Get(puzzle);
            if(archive)Telemetry.DailyEvent("daily_archive_selected",puzzle,before,today);
            if(before.completed)Telemetry.DailyEvent("daily_shift_replayed",puzzle,before,today);
            ActiveDaily=puzzle;Rebuild("daily_select",false);return true;
        }
        private void Update()
        {
            if (endingRequested) TryRevealEnding();
            if (hintPanel != null && hintPanel.IsOpen && !rewardPending)
                hintPanel.RefreshAvailability(Allowances.RecoveryEnabled(recoveryReason) && RewardedAds.IsRewardedAdAvailable, DailyLanguage);
            if(chapter!=null&&chapter.IsOpen)
            { TryDailyDate(out var date); if (date != panelDate) RefreshChapterDaily(); }
            if(dailyPanel!=null&&dailyPanel.IsOpen&&TryDailyDate(out var dailyDate)&&dailyDate!=panelDate)
            {panelDate=dailyDate;dailyPanel.Open(DailyPool,DailySaves,panelDate,DailyLanguage);}
        }
        private void OnApplicationPause(bool paused)
        {
            Telemetry?.Pause(paused); audioManager?.Pause(paused); haptics?.Pause(paused);
            if (!paused && hud != null) RefreshControls();
        }
        private void OnApplicationFocus(bool focused) { audioManager?.Focus(focused); haptics?.Focus(focused); }
        private void OnApplicationQuit() => Telemetry?.EndSession();

#if UNITY_EDITOR
        // Test navigation setup is transient Editor state: never serialized into the scene or player save.
        public static void SetDirectGameplayForValidation(string prefix, bool enabled) =>
            UnityEditor.SessionState.SetBool("SHIFT.DirectGameplayValidation." + prefix, enabled);

        // Inspector/test-only bypass. It never writes progression and is absent from player builds.
        public bool LoadLevel(int index)
        {
            if (!ValidLevel(index)) return false;
            returningContext = false; ActiveDaily = null; activeLevelIndex = selectedLevel = index; useLevelSet = true; debugLevel = true;
            if (Application.isPlaying && audioManager != null) Rebuild("debug_select", false);
            return true;
        }
        public void ResetProgress() { ActiveDaily = null; progress?.Reset(); debugLevel = false; activeLevelIndex = selectedLevel = 0; if (audioManager != null) Rebuild("reset_progress", false); }
        public void UnlockAllLevels() { progress?.UnlockAll(); }
        [ContextMenu("Load Selected Level")]
        private void LoadSelectedLevel() => LoadLevel(selectedLevel);
#endif

        private void OnDestroy()
        {
            rewardGeneration++; haptics?.Cancel(); audioManager?.StopAll();
            Telemetry?.EndSession();
            if (eventSystemObject != null) Destroy(eventSystemObject);
            if (circle != null) { Destroy(circle.texture); Destroy(circle); }
            if (rounded != null) { Destroy(rounded.texture); Destroy(rounded); }
        }
    }
}
