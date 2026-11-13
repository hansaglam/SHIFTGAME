#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace Shift.Game
{
    // Only the isolated 103-level Chapter6 experiment. Never in a build.
    // Fixes Unity's known preview-copy issue where the last ScriptableObject
    // slots can be null on entering Play despite a valid Editor asset.
    public static class RelayPreviewAssetRepair
    {
        private const string SavePrefix =
            "SHIFT.Preview.Chapter6Relay.v1.";
        private const string Folder =
            "Assets/_Game/Data/Levels/Chapter6_RelayPrototypes/";
        private static readonly string[] Files =
        {
            "Level101_TheWaitingMove.asset",
            "Level102_OneTapTwoPaths.asset",
            "Level103_DelayedTriple.asset"
        };

        public static int Restore(LevelData[] levels, string prefix)
        {
            if (prefix != SavePrefix) return 0;
            if (levels == null || levels.Length != 103)
                throw new InvalidOperationException(
                    "RÖLE preview must contain 103 levels.");
            int repaired = 0;
            for (int i = 0; i < Files.Length; i++)
            {
                var level = AssetDatabase.LoadAssetAtPath<LevelData>(
                    Folder + Files[i]);
                string error = null;
                if (level == null || !level.ValidatePlayable(out error))
                    throw new InvalidOperationException(
                        "RÖLE asset unavailable " + (101 + i) + ": " +
                        (error ?? "missing"));
                if (levels[100 + i] == null)
                {
                    levels[100 + i] = level;
                    repaired++;
                }
                else if (levels[100 + i] != level)
                    throw new InvalidOperationException(
                        "RÖLE unexpected reference at " + (101 + i));
            }
            if (repaired > 0)
                Debug.LogWarning("RÖLE preview restored " + repaired +
                    " lost LevelData references (test only).");
            return repaired;
        }
    }
}
#endif
