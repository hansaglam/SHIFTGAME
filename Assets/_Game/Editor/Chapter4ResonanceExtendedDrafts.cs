using System;
using UnityEditor;
using UnityEngine;

namespace Shift.Game.Editor
{
    // Editor-only candidates. No shipping progression, saves or Perfect registry changes.
    public static class Chapter4ResonanceExtendedDrafts
    {
        public const string Folder = "Assets/_Game/Data/Levels/Chapter4_ResonanceDrafts/";
        private static readonly string[] Names = {
            "Level66_SignalLock",
            "Level67_ThreeWayPulse",
            "Level68_TwinCircuit",
            "Level69_KeyBeforeGate",
            "Level70_FifthBeat",
            "Level71_ThreadedMotion",
            "Level72_DistantPriority",
            "Level73_CrossedSignals",
            "Level74_SilentDiversion",
            "Level75_HiddenOrder",
            "Level76_BeyondFirstPulse",
            "Level77_FalseLead",
            "Level78_DeferredChain",
            "Level79_TheLastKey",
            "Level80_HeartOfResonance"
        };

        public static LevelData Get(int number)
        {
            if (number < 66 || number > 80) return null;
            return AssetDatabase.LoadAssetAtPath<LevelData>(
                Folder + Names[number - 66] + ".asset");
        }

        // Known longer wins are private to editor QA, not hints or production data.
        public static GridPosition[] AlternateWinningRoute(int number)
        {
            switch (number)
            {
                case 66: return new[] { new GridPosition(6, 3), new GridPosition(5, 3), new GridPosition(1, 3), new GridPosition(3, 3), new GridPosition(2, 1), new GridPosition(1, 1) };
                case 67: return new[] { new GridPosition(3, 3), new GridPosition(0, 1), new GridPosition(2, 1), new GridPosition(3, 1), new GridPosition(5, 3), new GridPosition(1, 3) };
                case 68: return new[] { new GridPosition(5, 4), new GridPosition(6, 3), new GridPosition(5, 3), new GridPosition(3, 3), new GridPosition(5, 1), new GridPosition(2, 1), new GridPosition(1, 1) };
                case 69: return new[] { new GridPosition(0, 1), new GridPosition(1, 1), new GridPosition(2, 1), new GridPosition(4, 3), new GridPosition(3, 3), new GridPosition(3, 1), new GridPosition(4, 3) };
                case 70: return new[] { new GridPosition(1, 6), new GridPosition(1, 5), new GridPosition(1, 4), new GridPosition(3, 5), new GridPosition(3, 4), new GridPosition(3, 2), new GridPosition(5, 5) };
                case 71: return new[] { new GridPosition(4, 1), new GridPosition(6, 3), new GridPosition(3, 3), new GridPosition(1, 3), new GridPosition(3, 1), new GridPosition(2, 1), new GridPosition(1, 1) };
                case 72: return new[] { new GridPosition(5, 1), new GridPosition(3, 1), new GridPosition(2, 2), new GridPosition(4, 1), new GridPosition(2, 5), new GridPosition(1, 4), new GridPosition(1, 5) };
                case 73: return new[] { new GridPosition(3, 3), new GridPosition(1, 3), new GridPosition(5, 5), new GridPosition(6, 5), new GridPosition(4, 5), new GridPosition(5, 4), new GridPosition(5, 3), new GridPosition(5, 1) };
                case 74: return new[] { new GridPosition(1, 4), new GridPosition(0, 2), new GridPosition(2, 3), new GridPosition(3, 2), new GridPosition(4, 2), new GridPosition(6, 2), new GridPosition(2, 1) };
                case 75: return new[] { new GridPosition(3, 4), new GridPosition(0, 2), new GridPosition(3, 2), new GridPosition(4, 4), new GridPosition(5, 4), new GridPosition(5, 2), new GridPosition(6, 2), new GridPosition(6, 4) };
                case 76: return new[] { new GridPosition(2, 6), new GridPosition(2, 4), new GridPosition(4, 6), new GridPosition(4, 5), new GridPosition(4, 4), new GridPosition(2, 3), new GridPosition(2, 1), new GridPosition(4, 1) };
                case 77: return new[] { new GridPosition(3, 1), new GridPosition(2, 1), new GridPosition(4, 1), new GridPosition(1, 2), new GridPosition(3, 3), new GridPosition(3, 5), new GridPosition(1, 3), new GridPosition(1, 4), new GridPosition(1, 5) };
                case 78: return new[] { new GridPosition(3, 0), new GridPosition(3, 1), new GridPosition(3, 3), new GridPosition(1, 2), new GridPosition(3, 6), new GridPosition(1, 4), new GridPosition(1, 5), new GridPosition(3, 5), new GridPosition(3, 6) };
                case 79: return new[] { new GridPosition(1, 1), new GridPosition(0, 2), new GridPosition(1, 2), new GridPosition(2, 2), new GridPosition(1, 4), new GridPosition(2, 4), new GridPosition(4, 4), new GridPosition(5, 4), new GridPosition(6, 4) };
                case 80: return new[] { new GridPosition(0, 1), new GridPosition(2, 1), new GridPosition(3, 1), new GridPosition(1, 4), new GridPosition(4, 3), new GridPosition(1, 5), new GridPosition(5, 3), new GridPosition(6, 3), new GridPosition(1, 6) };
                default: return null;
            }
        }

        [MenuItem("SHIFT/Chapter 4/Validate Resonance Levels 66-80")]
        public static void ValidateAll()
        {
            int pass = 0;
            for (int number = 66; number <= 80; number++)
            {
                var level = Get(number);
                if (level == null)
                {
                    Debug.LogError("Chapter 4: Level " + number + " asset missing.");
                    continue;
                }
                if (!LevelValidation.VerifySolution(level, out string error, out int reactionDepth))
                {
                    Debug.LogError(level.name + ": " + error, level);
                    continue;
                }
                var proof = CampaignCertification.Prove(level, number);
                if (!proof.complete || proof.minimum != level.KnownSolutionLength)
                {
                    Debug.LogError(level.name + ": not proven shortest. " + proof.error +
                        " (minimum " + proof.minimum + ", recorded " +
                        level.KnownSolutionLength + ")", level);
                    continue;
                }
                if (!CheckAlternate(level, number, out error))
                {
                    Debug.LogError(level.name + ": longer winning solution invalid: " + error, level);
                    continue;
                }
                pass++;
                Debug.Log(level.name + " | shortest " + proof.minimum + " taps | longer " +
                    AlternateWinningRoute(number).Length + " taps | max chain " +
                    reactionDepth + " | explored states " + proof.states +
                    " | pilot only, Perfect Shift disabled", level);
            }
            Debug.Log("Resonance 66-80: " + pass + "/15 candidates certified.");
            if (pass != 15) Debug.LogError("Chapter 4 release gate FAILED. Do not publish.");
        }

        public static bool CheckAlternate(LevelData level, int number, out string error)
        {
            error = null;
            var route = AlternateWinningRoute(number);
            if (route == null || route.Length != level.KnownSolutionLength + 1 ||
                route.Length > level.MoveLimit)
            {
                error = "Alternate route must be exactly one tap longer but within the limit.";
                return false;
            }
            var board = new BoardManager(); board.Load(level);
            foreach (var tap in route)
            {
                int before = board.MovesRemaining;
                if (!board.RequestMove(tap) || board.LastReactionWasCancelled ||
                    board.ReactionDepth == 0 || board.MovesRemaining != before - 1)
                {
                    error = "Alternate tap " + tap + " did not commit.";
                    return false;
                }
                board.CompleteResolution();
            }
            if (board.State != GameState.Won)
            {
                error = "Alternate path did not end in a win.";
                return false;
            }
            return true;
        }

        [MenuItem("SHIFT/Chapter 4/Analyze Resonance Decisions 66-80")]
        public static void AnalyzeAll()
        {
            int ok = 0;
            for (int n = 66; n <= 80; n++)
            {
                var level = Get(n);
                if (level == null) { Debug.LogError("Missing Level " + n); continue; }
                try
                {
                    var result = new PuzzleBatchAnalysis(level).Analyze(level.KnownSolution);
                    if (result.minimum != level.KnownSolutionLength)
                    {
                        Debug.LogError(level.name + ": authored route not shortest!", level);
                        continue;
                    }
                    Debug.Log(level.name + " | min=" + result.minimum +
                        " | meaningful=" + result.meaningfulDecisions +
                        " | efficiency=" + result.efficiencyDecisions +
                        " | winningPaths=" + result.solutionPaths +
                        " | chanceUniform=" + result.randomSolveProbability.ToString("F3") +
                        " | states=" + result.states +
                        " | maxReaction=" + result.routeMaxDepth, level);
                    ok++;
                }
                catch (Exception ex)
                {
                    Debug.LogError(level.name + ": search failed: " + ex.Message, level);
                }
            }
            Debug.Log("Resonance decision profile " + ok + "/15 analyzed. " +
                "The probability is theoretical, NOT real player retention.");
        }
    }
}
