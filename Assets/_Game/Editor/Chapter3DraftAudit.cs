using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Shift.Game.Editor
{
    /// <summary>Read-only audit for candidate Levels 41-50; never rewrites Levels 1-40.</summary>
    public static class Chapter3DraftAudit
    {
        private const string DraftFolder = "Assets/_Game/Data/Levels/Chapter3_Drafts";
        [MenuItem("SHIFT/Chapter 3/Audit Level 41-50 Drafts")]
        public static void Audit()
        {
            if (!AssetDatabase.IsValidFolder(DraftFolder))
            {
                Debug.LogWarning("Chapter 3 has no authored draft folder yet: " + DraftFolder);
                return;
            }
            var levels = new Dictionary<int, List<LevelData>>();
            foreach (string guid in AssetDatabase.FindAssets("t:LevelData", new[] { DraftFolder }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string name = Path.GetFileNameWithoutExtension(path);
                if (!name.StartsWith("Level", StringComparison.Ordinal) || name.Length < 8 ||
                    name[7] != '_' || !int.TryParse(name.Substring(5, 2), out int number) ||
                    number < 41 || number > 50) continue;
                if (!levels.ContainsKey(number)) levels[number] = new List<LevelData>();
                levels[number].Add(AssetDatabase.LoadAssetAtPath<LevelData>(path));
            }
            int certified = 0, missing = 0, failed = 0;
            for (int i = 41; i <= 50; i++)
            {
                if (!levels.TryGetValue(i, out var list) || list.Count == 0)
                {
                    missing++;
                    Debug.LogWarning("Draft Level " + i + " not authored yet.");
                    continue;
                }
                if (list.Count != 1 || list[0] == null)
                {
                    failed++;
                    Debug.LogError("Level " + i + " has duplicate or broken assets.");
                    continue;
                }
                var level = list[0];
                if (!LevelValidation.VerifySolution(level, out string error, out int depth))
                {
                    failed++;
                    Debug.LogError(level.name + " failed real simulation: " + error, level);
                    continue;
                }
                var proof = CampaignCertification.Prove(level, i);
                if (!proof.complete)
                {
                    failed++;
                    Debug.LogError(level.name + " has no verified optimality proof: " + proof.error, level);
                    continue;
                }
                certified++;
                Debug.Log(level.name + " certified in " + proof.minimum +
                    " moves; reaction depth " + depth, level);
            }
            Debug.Log("Chapter 3: " + certified + "/10 certified, " + missing +
                " drafts missing, " + failed + " failed. No existing levels changed.");
        }
    }
}
