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
        private bool InputModalOpen => (chapter != null && chapter.IsOpen) || (settingsPanel != null && settingsPanel.IsOpen);
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
        private int activeLevelIndex;
        public LevelProgression Progress => progress;
        public BoardManager Board => board;
        public LevelData CurrentLevel => useLevelSet && levels != null && levels.Length > 0
            ? levels[Mathf.Clamp(activeLevelIndex, 0, levels.Length - 1)] : level;

        private void Start()
        {
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
                progress = new LevelProgression(levels.Length, saves); activeLevelIndex = selectedLevel = progress.Current;
            }
            if (EventSystem.current == null)
            {
                eventSystemObject = new GameObject("SHIFT Input", typeof(EventSystem));
                eventSystemObject.AddComponent<InputSystemUIInputModule>();
            }
            Rebuild("session_start", false);
        }

        public void Restart() => Rebuild("restart", true);
        private void Rebuild(string reason, bool restart)
        {
            StopAllCoroutines();
            audioManager.StopAll();
            if (canvasObject != null) { canvasObject.SetActive(false); Destroy(canvasObject); }
            var activeLevel = CurrentLevel;
            if (activeLevel == null || !activeLevel.ValidatePlayable(out _))
            { Debug.LogError("SHIFT requires a valid LevelData asset assigned to PrototypeGame.", this); return; }
            Telemetry.StartLevel(activeLevel, useLevelSet ? activeLevelIndex : -1, activeLevel.VerifiedOptimalMoveCount, restart, reason);
            board.Load(activeLevel);
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
                chapter.Build(font, rounded, levels.Length, index => SelectLevel(index), CloseChapter, () => SelectLevel(progress.Current));
            }
            var settingsRect = PlaceholderVisuals.Rect("Settings Panel", safe, Vector2.zero, Vector2.one);
            settingsPanel = settingsRect.gameObject.AddComponent<SettingsPanel>();
            settingsPanel.Build(font, rounded, Settings, gameFeel, audioManager, CloseSettings);
            LayoutControls.Settings(safe, circle, gameFeel, OpenSettings);
            foreach (var button in canvasObject.GetComponentsInChildren<Button>(true))
                if (button.GetComponentInParent<Piece>() == null) button.onClick.AddListener(() => audioManager.Play(AudioCue.UIButton));
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (showTelemetryOverlay)
            {
                var overlay = PlaceholderVisuals.Rect("Telemetry Overlay", safe, new Vector2(.06f,.77f), new Vector2(.94f,.89f));
                overlay.gameObject.AddComponent<TelemetryOverlay>().Initialize(Telemetry, font);
            }
#endif
            hud.Refresh(board);
        }

        private void OnTap(GridPosition position)
        {
            if (InputModalOpen) return;
            var tapped = board.GetOccupant(position);
            if (!board.RequestMove(position)) return;
            Telemetry.StateActions(board.Actions);
            Telemetry.AcceptedTap(board.ReactionDepth, board.MovesRemaining, board.LastReactionWasCancelled);
            view.SetInput(false); hud.BeginReaction(); hud.Refresh(board);
            view.TapFeedback(tapped.Id, board.Actions.Count == 0);
            StartCoroutine(Resolve(tapped.Id));
        }

        private IEnumerator Resolve(int tappedId)
        {
            yield return view.Play(board, tappedId);
            board.CompleteResolution();
            if (board.State == GameState.Won || board.State == GameState.Lost) Telemetry.Finish(board.State == GameState.Won);
            if (board.State == GameState.Won && progress != null && !debugLevel) progress.Complete(activeLevelIndex);
            if (chapter != null && chapter.IsOpen) chapter.Open(progress);
            hud.Refresh(board);
            hud.SetCompletion(board.State == GameState.Won, progress != null && (activeLevelIndex + 1) % 20 == 0,
                progress != null && !debugLevel && activeLevelIndex < levels.Length - 1, activeLevelIndex / 20 + 1);
            hud.FinishChain(); view.SetInput(board.State == GameState.Playing && !InputModalOpen);
            hud.ShowMastery(Telemetry.Result);
            if (board.State == GameState.Won) { view.SuccessPulse(); audioManager.Play(progress != null && (activeLevelIndex + 1) % 20 == 0 ? AudioCue.ChapterComplete : AudioCue.Win); haptics.Success(); }
            else if (board.State == GameState.Lost) { audioManager.Play(AudioCue.Lose); haptics.Failure(); }
        }

        public bool SelectLevel(int index)
        {
            if (!ValidLevel(index) || progress == null || !progress.Select(index)) return false;
            debugLevel = false;
            activeLevelIndex = selectedLevel = index; useLevelSet = true;
            Rebuild("select", false); Telemetry.SelectLevel();
            return true;
        }
        public bool NextLevel()
        {
            if (debugLevel || board.State != GameState.Won || progress == null || !ValidLevel(activeLevelIndex + 1) || !progress.Next()) return false;
            activeLevelIndex = selectedLevel = progress.Current; Rebuild("next", false); return true;
        }
        private bool ValidLevel(int index) => levels != null && index >= 0 && index < levels.Length && levels[index] != null && levels[index].ValidatePlayable(out _);
        private void Replay()
        {
            if (progress != null && !debugLevel) progress.Select(activeLevelIndex);
            Restart();
        }
        public void OpenChapter()
        {
            if (chapter == null) return;
            settingsPanel?.Close(); view.SetInput(false); chapter.Open(progress); audioManager.Play(AudioCue.PanelOpen);
        }
        public void CloseChapter()
        {
            if (chapter == null) return;
            chapter.Close(); view.SetInput(board.State == GameState.Playing && !InputModalOpen); audioManager.Play(AudioCue.PanelClose);
        }

        public void OpenSettings()
        {
            chapter?.Close(); view.SetInput(false); settingsPanel.Open(); audioManager.Play(AudioCue.PanelOpen);
        }
        public void CloseSettings()
        {
            settingsPanel.Close(); view.SetInput(board.State == GameState.Playing && !InputModalOpen); audioManager.Play(AudioCue.PanelClose);
        }
        private void OnApplicationPause(bool paused) => Telemetry?.Pause(paused);
        private void OnApplicationQuit() => Telemetry?.EndSession();

#if UNITY_EDITOR
        // Inspector/test-only bypass. It never writes progression and is absent from player builds.
        public bool LoadLevel(int index)
        {
            if (!ValidLevel(index)) return false;
            activeLevelIndex = selectedLevel = index; useLevelSet = true; debugLevel = true;
            if (Application.isPlaying && audioManager != null) Rebuild("debug_select", false);
            return true;
        }
        public void ResetProgress() { progress?.Reset(); debugLevel = false; activeLevelIndex = selectedLevel = 0; if (audioManager != null) Rebuild("reset_progress", false); }
        public void UnlockAllLevels() { progress?.UnlockAll(); }
        [ContextMenu("Load Selected Level")]
        private void LoadSelectedLevel() => LoadLevel(selectedLevel);
#endif

        private void OnDestroy()
        {
            Telemetry?.EndSession();
            if (eventSystemObject != null) Destroy(eventSystemObject);
            if (circle != null) { Destroy(circle.texture); Destroy(circle); }
            if (rounded != null) { Destroy(rounded.texture); Destroy(rounded); }
        }
    }
}
