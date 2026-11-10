using UnityEditor;
using UnityEngine;

namespace Shift.Game.Editor
{
    // Isolated Relay candidate certification. Nothing is added to the shipping campaign.
    public static class Chapter4RelayDrafts
    {
        public const string Folder = "Assets/_Game/Data/Levels/Chapter4_RelayDrafts/";
        private static readonly string[] Names =
        {
            "Level61_TwoShores",
            "Level62_OccupiedLanding",
            "Level63_DistantSwitch",
            "Level64_DoubleLink",
            "Level65_LinkedCascade"
        };

        public static LevelData Get(int levelNumber)
        {
            if (levelNumber < 61 || levelNumber > 65) return null;
            return AssetDatabase.LoadAssetAtPath<LevelData>(
                Folder + Names[levelNumber - 61] + ".asset");
        }

        [MenuItem("SHIFT/Chapter 4/Validate Relay Levels 61-65")]
        public static void ValidateAll()
        {
            int pass = 0;
            for (int n = 61; n <= 65; n++)
            {
                var level = Get(n);
                if (level == null)
                {
                    Debug.LogError("Missing Relay draft Level " + n);
                    continue;
                }
                if (!LevelValidation.VerifySolution(level, out string error, out int depth))
                {
                    Debug.LogError(level.name + ": " + error, level);
                    continue;
                }
                var proof = CampaignCertification.Prove(level, n);
                if (!proof.complete || proof.minimum != level.KnownSolutionLength)
                {
                    Debug.LogError(level.name + ": shortest solution not certified. " +
                        proof.error + ", found min=" + proof.minimum +
                        ", recorded=" + level.KnownSolutionLength, level);
                    continue;
                }
                pass++;
                Debug.Log(level.name + " verified: minimum " + proof.minimum +
                    " committed taps, max reaction depth " + depth +
                    ", " + proof.states + " solver states. Pilot only.", level);
            }
            Debug.Log("Chapter 4 Relay candidates: " + pass + "/5 verified.");
            if (pass != 5) Debug.LogError("Relay pilot validation FAILED; do not ship.");
        }
    }
}
