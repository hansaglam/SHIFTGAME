using UnityEditor;
using UnityEngine;

namespace Shift.Game.Editor
{
    // Isolated 61–65 prototype. Does not modify production levels or unlock Perfect.
    public static class Chapter4ResonanceDrafts
    {
        public const string Folder = "Assets/_Game/Data/Levels/Chapter4_ResonanceDrafts/";
        public static readonly string[] Names =
        {
            "Level61_FirstSignal",
            "Level62_QuietShortcut",
            "Level63_DistantSwitch",
            "Level64_ChainOfSignals",
            "Level65_RemoteCascade"
        };

        public static LevelData Get(int number)
        {
            if (number < 61 || number > 65) return null;
            return AssetDatabase.LoadAssetAtPath<LevelData>(
                Folder + Names[number - 61] + ".asset");
        }

        [MenuItem("SHIFT/Chapter 4/Validate Resonance Levels 61-65")]
        public static void ValidateAll()
        {
            int pass = 0;
            for (int n = 61; n <= 65; n++)
            {
                var level = Get(n);
                if (level == null)
                {
                    Debug.LogError("Resonance: missing Level " + n);
                    continue;
                }
                if (!LevelValidation.VerifySolution(level, out string error, out int maxDepth))
                {
                    Debug.LogError(level.name + ": " + error, level);
                    continue;
                }
                var proof = CampaignCertification.Prove(level, n);
                if (!proof.complete || proof.minimum != level.KnownSolutionLength)
                {
                    Debug.LogError(level.name + ": minimum is not proven, " + proof.error +
                        "; found " + proof.minimum + " versus recorded " +
                        level.KnownSolutionLength, level);
                    continue;
                }
                pass++;
                Debug.Log(level.name + ": proven shortest " + proof.minimum +
                    " committed taps; maximum reaction depth " + maxDepth +
                    ", states " + proof.states + " (pilot only, no Perfect rewards).", level);
            }
            Debug.Log("Resonance pilot 61-65: " + pass + "/5 certified.");
            if (pass != 5) Debug.LogError("Resonance validation failed: DO NOT ship these levels.");
        }

        [MenuItem("SHIFT/Chapter 4/Analyze Resonance Decision Quality 61-65")]
        public static void AnalyzeDecisions()
        {
            int pass = 0;
            for (int n = 61; n <= 65; n++)
            {
                var level = Get(n);
                if (level == null) { Debug.LogError("Missing Resonance Level " + n); continue; }
                try
                {
                    var result = new PuzzleBatchAnalysis(level).Analyze(level.KnownSolution);
                    if (result.minimum != level.KnownSolutionLength)
                    {
                        Debug.LogError(level.name + ": known route is not the shortest. " +
                            "Proven " + result.minimum + " vs authored " +
                            level.KnownSolutionLength, level);
                        continue;
                    }
                    Debug.Log(level.name + " | min=" + result.minimum +
                        " | explored=" + result.states +
                        " | winningPaths=" + result.solutionPaths +
                        " | meaningfulDecisions=" + result.meaningfulDecisions +
                        " | efficiencyDecisions=" + result.efficiencyDecisions +
                        " | maxReactionDepth=" + result.routeMaxDepth +
                        " | randomWinProbability=" +
                        result.randomSolveProbability.ToString("F3") +
                        " (uniform-choice model, NOT player analytics)", level);
                    pass++;
                }
                catch (System.Exception ex)
                {
                    Debug.LogError(level.name + " decision analysis failed: " + ex.Message, level);
                }
            }
            Debug.Log("Resonance decision report: " + pass + "/5 analyzed.");
        }
    }
}
