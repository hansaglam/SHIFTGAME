using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shift.Game
{
    public interface IAnalyticsService { void Track(AnalyticsEvent value); }
    public sealed class NullAnalyticsService : IAnalyticsService { public void Track(AnalyticsEvent value) { } }

    [Serializable]
    public sealed class AnalyticsEvent
    {
        public int schemaVersion = 3;
        public string name, reason;
        public int pageIndex;
        public int perfectCount;
        public bool mastered;
        public string language, source;
        public double sessionSeconds;
        public int sequence, reactionDepth;
        public int hintStage, freeHintsRemaining;
        public int chapterIndex, chapterPerfectCount, chapterTotalCount, chapterEligibleCount;
        public bool firstTime;
        public string dateKey, dailyObjectiveColors, dailyDifficulty;
        public int sourceLevelIndex, dailyPoolVersion;
        public bool isToday, completedBefore, perfectBefore;
        public string reactionClass;
        public string reactionTier;
        public AttemptMetrics attempt;
        public SessionMetrics session;
    }
    // Bounded, local-only. No identifiers, files, network or release console output.
    public sealed class LocalAnalyticsService : IAnalyticsService
    {
        private readonly Queue<AnalyticsEvent> events = new Queue<AnalyticsEvent>();
        public IEnumerable<AnalyticsEvent> Events => events;
        public bool Verbose { get; set; }
        public void Track(AnalyticsEvent value)
        {
            if (events.Count == 128) events.Dequeue();
            events.Enqueue(value);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (Verbose) Debug.Log("SHIFT telemetry " + JsonUtility.ToJson(value));
#endif
        }
    }
    [Serializable]
    public sealed class AttemptMetrics
    {
        public string levelId, title, targetColor;
        // Additive objective metadata. The legacy targetColor field retains its meaning.
        public string targetColors;
        public int levelIndex, levelNumber, width, height, budget, attemptNumber, restarts;
        public int successfulMoves, taps, blockedTaps, cancelledTaps, remainingMoves;
        public int switchActivations, gateOpens, gateCloses;
        public int maxDepth, totalDepth, chains, strongChains, knownSolutionLength;
        // -1 is explicitly unknown in JSON; never inferred from the recorded solution.
        public int verifiedOptimalMoves = -1, moveDifference;
        public bool hasOptimal, perfectShift, completed;
        public string archetype, difficultyBand, reasoningStyle, creativeHook, maxReactionTier = "Normal";
        public bool hasPayoffMove, creativeCandidate, payoffReached;
        public int expectedPayoffDepth;
        public double duration, efficiency, blockedRatio, averageDepth;
        public AttemptMetrics Copy() => (AttemptMetrics)MemberwiseClone();
    }
    [Serializable]
    public sealed class SessionMetrics
    {
        public int levelsStarted, levelsCompleted, highestLevel, restarts, successfulMoves, chains;
        public double duration;
        public SessionMetrics Copy() => (SessionMetrics)MemberwiseClone();
    }

    // Pure coordination model. The injected monotonic clock permits deterministic tests.
    // Each visit has one or more attempts; restart count survives retry, not level selection.
    public sealed class TelemetryTracker
    {
        private readonly IAnalyticsService provider;
        private readonly Func<double> clock;
        private readonly Dictionary<string, int> attempts = new Dictionary<string, int>();
        private AttemptMetrics current;
        private SessionMetrics session = new SessionMetrics();
        private bool running, terminal, paused;
        private double origin, pausedAt, excluded, attemptStart, finishedAt, endedAt;
        private int sequence;
        public int ProviderFailures { get; private set; }
        public AttemptMetrics Result { get; private set; }
        public TelemetryTracker(IAnalyticsService provider, Func<double> clock)
        { this.provider = provider ?? new NullAnalyticsService(); this.clock = clock ?? throw new ArgumentNullException(nameof(clock)); }
        private double Now => !running ? endedAt : Math.Max(0, (paused ? pausedAt : clock()) - origin - excluded);
        public void StartSession()
        {
            if (running) EndSession();
            origin = clock(); excluded = 0; paused = false; current = null; Result = null;
            session = new SessionMetrics(); attempts.Clear(); sequence = 0; running = true;
            Emit("session_start");
        }
        public void Pause(bool value)
        {
            if (!running || paused == value) return;
            if (value) { pausedAt = clock(); paused = true; Emit("session_pause"); }
            else { excluded += clock() - pausedAt; paused = false; Emit("session_resume"); }
        }
        public void EndSession()
        {
            if (!running) return;
            if (current != null && !terminal) Emit("level_abandon", "session_end");
            Emit("session_end"); endedAt = Now; running = false;
        }
        public void StartLevel(LevelData level, int index, int? optimal, bool restart = false, string reason = "start")
        {
            if (!running) StartSession();
            int restarts = 0;
            if (current != null)
            {
                if (restart)
                {
                    restarts = current.restarts + 1; current.restarts = restarts; session.restarts++;
                    Emit("level_restart", reason);
                }
                else if (!terminal) Emit("level_abandon", reason);
            }
            attempts.TryGetValue(level.name, out int attempt);
            attempts[level.name] = ++attempt;
            current = new AttemptMetrics { levelId = level.name, title = level.DisplayTitle, levelIndex = index,
                levelNumber = index + 1, width = level.Width, height = level.Height, budget = level.MoveLimit,
                remainingMoves = level.MoveLimit, targetColor = level.TargetColor.ToString(), attemptNumber = attempt,
                targetColors = string.Join("+", level.ResolvedTargetColors),
                restarts = restarts, knownSolutionLength = level.KnownSolution.Count,
                hasOptimal = optimal.HasValue, verifiedOptimalMoves = optimal ?? -1 };
            var design = level.Design;
            if (design != null)
            {
                current.archetype = design.primary.ToString(); current.difficultyBand = design.difficultyBand.ToString();
                current.reasoningStyle = design.reasoningStyle.ToString(); current.creativeHook = design.creativeHook.ToString();
                current.hasPayoffMove = design.hasPayoffMove; current.expectedPayoffDepth = design.expectedPayoffDepth;
                current.creativeCandidate = design.creativeCandidate;
            }
            terminal = false; Result = null; attemptStart = Now;
            session.levelsStarted++; session.highestLevel = Math.Max(session.highestLevel, index + 1);
            Emit("level_start", reason);
        }
        public void OnboardingEvent(string name, int pageIndex)
        {
            var value = new AnalyticsEvent { name = name, pageIndex = pageIndex,
                language = GameLanguageService.Shared.CurrentLanguage.ToString(), source = "first_launch",
                sequence = ++sequence, sessionSeconds = Now };
            try { provider.Track(value); } catch (Exception) { ProviderFailures++; }
        }
        public void CampaignEndingEvent(string name, int perfectCount)
        {
            var value = new AnalyticsEvent { name = name, perfectCount = perfectCount, mastered = perfectCount == 40,
                language = GameLanguageService.Shared.CurrentLanguage.ToString(), source = "level40",
                sequence = ++sequence, sessionSeconds = Now };
            try { provider.Track(value); } catch (Exception) { ProviderFailures++; }
        }
        public void SelectLevel() { if (running) Emit("level_select"); }
        // Call exactly once after an accepted RequestMove; ignored/locked input never reaches this method.
        public void AcceptedTap(int depth, int remaining, bool cancelled)
        {
            if (!running || current == null || terminal || paused) return;
            current.taps++; current.remainingMoves = remaining;
            if (cancelled) { current.cancelledTaps++; Emit("piece_tap", "cancelled"); return; }
            if (depth == 0) { current.blockedTaps++; Emit("blocked_tap", "blocked"); return; }
            current.successfulMoves++; session.successfulMoves++;
            current.totalDepth += depth; current.maxDepth = Math.Max(current.maxDepth, depth);
            current.maxReactionTier = ReactionPresentation.Classify(current.maxDepth).ToString();
            current.payoffReached |= current.hasPayoffMove && current.expectedPayoffDepth > 0 && depth >= current.expectedPayoffDepth;
            if (depth >= 2) { current.chains++; session.chains++; }
            if (depth >= 4) current.strongChains++;
            Emit("piece_tap", "successful"); Emit("chain", null, depth);
        }
        public void StateActions(System.Collections.Generic.IReadOnlyList<BoardAction> actions)
        {
            if (!running || current == null || terminal || paused) return;
            foreach (var action in actions)
            {
                if (action.Type == BoardActionType.SwitchActivated) current.switchActivations++;
                else if (action.Type == BoardActionType.GateOpened) current.gateOpens++;
                else if (action.Type == BoardActionType.GateClosed) current.gateCloses++;
            }
        }
        public void Finish(bool won)
        {
            if (!running || current == null || terminal) return;
            terminal = true; finishedAt = Now; current.completed = won;
            if (won) session.levelsCompleted++;
            Result = Snapshot(); Emit(won ? "level_complete" : "level_fail", won ? "delivered" : "move_exhaustion");
        }
        public void UndoUsed(int remaining, int stage = 0, int hints = 0)
        {
            if (current == null) return;
            // Physical state is restored; actual player effort and Perfect Shift accounting are not rewritten.
            terminal = false; Result = null; current.completed = false; current.remainingMoves = remaining;
            Utility("undo_used",stage,hints,remaining);
        }
        public void Utility(string name,int stage,int hints,int remaining)
        {
            if (!running || current == null) return;
            var attempt = Snapshot(); attempt.remainingMoves = remaining;
            var value = new AnalyticsEvent { name=name, sequence=++sequence, sessionSeconds=Now,
                attempt=attempt, session=SessionSnapshot(), hintStage=stage, freeHintsRemaining=hints };
            try { provider.Track(value); } catch (Exception) { ProviderFailures++; }
        }
        public void MasteryEvent(string name, ChapterMastery chapter, bool firstTime = false)
        {
            if (!running || current == null) return;
            var value = new AnalyticsEvent { name = name, sequence = ++sequence, sessionSeconds = Now,
                attempt = Snapshot(), session = SessionSnapshot(), chapterIndex = chapter.Chapter,
                chapterPerfectCount = chapter.Perfect, chapterTotalCount = chapter.Total,
                chapterEligibleCount = chapter.Eligible, firstTime = firstTime };
            try { provider.Track(value); } catch (Exception) { ProviderFailures++; }
        }
        public void DailyEvent(string name,DailyPuzzle puzzle,DailyRecord before,DateTime today)
        {
            if(!running||puzzle==null)return;
            var value=new AnalyticsEvent{name=name,sequence=++sequence,sessionSeconds=Now,attempt=Snapshot(),session=SessionSnapshot(),
                dateKey=puzzle.DateKey,sourceLevelIndex=puzzle.SourceIndex,dailyPoolVersion=puzzle.PoolVersion,isToday=puzzle.Date==today.Date,
                completedBefore=before.completed,perfectBefore=before.perfect,dailyObjectiveColors=string.Join("+",puzzle.Level.ResolvedTargetColors),dailyDifficulty=puzzle.Level.Design?.difficultyBand.ToString()};
            try{provider.Track(value);}catch(Exception){ProviderFailures++;}
        }
        public AttemptMetrics Snapshot()
        {
            if (current == null) return null;
            var a = current.Copy(); a.duration = Math.Max(0, (terminal ? finishedAt : Now) - attemptStart);
            a.blockedRatio = a.taps == 0 ? 0 : (double)a.blockedTaps / a.taps;
            a.averageDepth = a.successfulMoves == 0 ? 0 : (double)a.totalDepth / a.successfulMoves;
            a.moveDifference = a.hasOptimal ? a.successfulMoves - a.verifiedOptimalMoves : 0;
            a.perfectShift = a.completed && a.hasOptimal && a.moveDifference == 0;
            a.efficiency = a.completed && a.hasOptimal && a.successfulMoves > 0 ? (double)a.verifiedOptimalMoves / a.successfulMoves : 0;
            return a;
        }
        public SessionMetrics SessionSnapshot() { var s = session.Copy(); s.duration = Now; return s; }
        private void Emit(string name, string reason = null, int depth = 0)
        {
            var value = new AnalyticsEvent { name = name, reason = reason, sequence = ++sequence, sessionSeconds = Now,
                attempt = Snapshot(), session = SessionSnapshot(), reactionDepth = depth,
                reactionTier = ReactionPresentation.Classify(depth).ToString(),
                reactionClass = depth == 0 ? null : depth == 1 ? "simple" : depth < 4 ? "chain" : depth < 6 ? "strong" : "major" };
            try { provider.Track(value); } catch (Exception) { ProviderFailures++; } // Optional diagnostics must never break play.
        }
    }
}
