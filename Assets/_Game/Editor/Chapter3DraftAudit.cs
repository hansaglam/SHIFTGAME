using UnityEditor;
using UnityEngine;

namespace Shift.Game.Editor
{
    /// <summary>
    /// Isolated Chapter 3 draft validation. Does not write scenes, saves or old level assets.
    /// </summary>
    public static class Chapter3DraftAudit
    {
        public const string Folder = "Assets/_Game/Data/Levels/Chapter3_Drafts/";
        public static readonly string[] Names = {
            "Level41_FirstRipple",
            "Level42_ClearTheStream",
            "Level43_BorrowedCurrent",
            "Level44_ForkBeforeFlow",
            "Level45_SignalBeforeMotion",
            "Level46_TwinStreams",
            "Level47_ClosingWindow",
            "Level48_ResonantCrossing",
            "Level49_SecondPath",
            "Level50_WakeTheValley",
            "Level51_DelayedEcho",
            "Level52_DoubleThreshold",
            "Level53_BorrowedRoute",
            "Level54_TheQuietFork",
            "Level55_MirroredChain",
            "Level56_HiddenHandOff",
            "Level57_FalseShortcut",
            "Level58_ClosedCircuit",
            "Level59_ReverseReaction",
            "Level60_FinalThreshold"
        };

        public static LevelData Get(int levelNumber)
        {
            if (levelNumber < 41 || levelNumber > 60) return null;
            return AssetDatabase.LoadAssetAtPath<LevelData>(
                Folder + Names[levelNumber - 41] + ".asset");
        }

        [MenuItem("SHIFT/Chapter 3/Validate Draft Levels 41-60")]
        public static void ValidateAll()
        {
            int pass = 0, fail = 0;
            for (int number = 41; number <= 60; number++)
            {
                var level = Get(number);
                if (level == null) { fail++; Debug.LogError("Missing Chapter 3 Level " + number); continue; }
                if (!LevelValidation.VerifySolution(level, out string error, out int depth))
                {
                    fail++;
                    Debug.LogError(level.name + ": " + error, level);
                    continue;
                }
                var proof = CampaignCertification.Prove(level, number);
                if (!proof.complete || proof.minimum != level.KnownSolutionLength)
                {
                    fail++;
                    Debug.LogError(level.name + ": optimum not certified; " + proof.error +
                        ", proven minimum=" + proof.minimum + ", recorded=" +
                        level.KnownSolutionLength, level);
                    continue;
                }
                pass++;
                Debug.Log(level.name + " CERTIFIED: " + proof.minimum +
                    " moves, maximum route reaction " + depth + ", explored " +
                    proof.states + " states.", level);
            }
            Debug.Log("Chapter 3 draft audit: " + pass + "/20 certified, " + fail +
                " failed. Levels 1–40 untouched.");
        }
    }
}
