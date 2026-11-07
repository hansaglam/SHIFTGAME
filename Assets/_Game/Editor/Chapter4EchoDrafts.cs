using System;
using UnityEditor;
using UnityEngine;

namespace Shift.Game.Editor
{
    // Isolated mechanic-pilot assets. Does not alter campaign wiring or player saves.
    public static class Chapter4EchoDrafts
    {
        public const string Folder = "Assets/_Game/Data/Levels/Chapter4_EchoDrafts/";
        public static readonly string[] Names =
        {
            "Level61_FirstEcho",
            "Level62_HiddenPush",
            "Level63_GateInTheEcho",
            "Level64_TwoEchoes",
            "Level65_TheEfficientRipple"
        };

        public static LevelData Get(int number)
        {
            if (number < 61 || number > 65) return null;
            return AssetDatabase.LoadAssetAtPath<LevelData>(Folder + Names[number - 61] + ".asset");
        }

        [MenuItem("SHIFT/Chapter 4/Validate Echo Levels 61-65")]
        public static void Validate()
        {
            int passed = 0;
            for (int n = 61; n <= 65; n++)
            {
                var level = Get(n);
                if (level == null) { Debug.LogError("Missing Echo Level " + n); continue; }
                if (!LevelValidation.VerifySolution(level, out string error, out int depth))
                { Debug.LogError(level.name + " invalid: " + error, level); continue; }
                var result = CampaignCertification.Prove(level, n);
                if (!result.complete || result.minimum != level.KnownSolutionLength)
                {
                    Debug.LogError(level.name + " lacks a verified shortest solution: " +
                        result.error + " / minimum=" + result.minimum, level);
                    continue;
                }
                passed++;
                Debug.Log(level.name + ": valid shortest route " + result.minimum +
                    " taps, max reaction depth " + depth +
                    ", explored states " + result.states +
                    ". CANDIDATE ONLY: Perfect Shift registry is not active.", level);
            }
            Debug.Log("Chapter 4 Echo pilot validation: " + passed + "/5 certified. " +
                "No old level or save data modified.");
            if (passed < 5) Debug.LogError("Chapter 4 mechanic pilot has failing draft levels.");
        }
    }
}
