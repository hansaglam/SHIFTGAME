#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace Shift.Game
{
    // Editor-only safety net for an ISOLATED mechanic test scene.
    // Unity can retain an array size of 104 while losing references to its
    // final 4 newly created ScriptableObject assets after scene recreation.
    // We load ONLY the four authored preview assets, by their exact paths.
    // Never runs in a player build, and never changes any 1-100 asset.
    public static class TetherPreviewAssetRepair
    {
        public const string PreviewSavePrefix =
            "SHIFT.Preview.Chapter6Tether.v1.";

        private const string Folder =
            "Assets/_Game/Data/Levels/Chapter6_TetherPrototypes/";

        private static readonly string[] Names =
        {
            "Level101_FirstBond.asset",
            "Level102_BlockedTension.asset",
            "Level103_TripleReaction.asset",
            "Level104_TwoGateTether.asset"
        };

        public static int RestoreMissing(LevelData[] levels, string prefix)
        {
            if (prefix != PreviewSavePrefix)
                return 0;
            if (levels == null || levels.Length != 104)
                throw new InvalidOperationException(
                    "BAĞ-only preview expected exactly 104 level slots. " +
                    "Other campaigns must never use the debug resolver.");

            int restored = 0;
            for (int i = 0; i < Names.Length; i++)
            {
                int index = 100 + i;
                var actual = AssetDatabase.LoadAssetAtPath<LevelData>(
                    Folder + Names[i]);
                if (actual == null ||
                    !actual.ValidatePlayable(out string _))
                    throw new InvalidOperationException(
                        "BAĞ Level " + (index + 1) +
                        " is missing or invalid in the AssetDatabase.");

                if (levels[index] == null)
                {
                    levels[index] = actual;
                    restored++;
                }
                else if (levels[index] != actual)
                    throw new InvalidOperationException(
                        "BAĞ preview Level " + (index + 1) +
                        " references a different asset; refusing to " +
                        "replace a non-null level silently.");
            }

            if (restored > 0)
                Debug.LogWarning("BAĞ isolated preview recovered " +
                    restored + " missing LevelData references (101–104) " +
                    "from AssetDatabase in memory. The authored scene and " +
                    "all player save namespaces remain unchanged.");
            return restored;
        }
    }
}
#endif
