using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Shift.Game.Editor
{
    /// <summary>
    /// Editorial screening only. A common tap coordinate may mean a completely
    /// different thing on another board; this is a review queue, not a quality grade.
    /// Both optimal and longer winning routes are compared for copy/paste patterns.
    /// </summary>
    public static class LevelRouteSimilarityAudit
    {
        [Serializable]
        public sealed class Risk
        {
            public int firstLevel, secondLevel, matchingTaps, commonOpening;
            public float routeOverlap;
            public string routeKind, priority, note;
        }

        [Serializable]
        public sealed class LaneMotifRisk
        {
            public int level, longestAxisRun, shortestRouteTaps;
            public string note;
        }

        [Serializable]
        public sealed class Report
        {
            public string status = "EDITOR CANDIDATE SCREENING / HUMAN REVIEW REQUIRED";
            public int evaluatedLevelPairs;
            public int suspectedPatternPairs, laneMotifLevels;
            public List<Risk> risks = new List<Risk>();
            public List<LaneMotifRisk> laneMotifs = new List<LaneMotifRisk>();
        }

        private static LevelData Get(int n)
        {
            if (n >= 41 && n <= 60) return Chapter3DraftAudit.Get(n);
            if (n >= 61 && n <= 65) return Chapter4ResonanceDrafts.Get(n);
            if (n >= 66 && n <= 80) return Chapter4ResonanceExtendedDrafts.Get(n);
            return null;
        }

        private static IReadOnlyList<GridPosition> Route(int n, bool alternate)
        {
            if (!alternate) return Get(n)?.KnownSolution;
            if (n >= 51 && n <= 60) return Chapter3AlternateSolutions.Get(n);
            if (n >= 66 && n <= 80)
                return Chapter4ResonanceExtendedDrafts.AlternateWinningRoute(n);
            return null;
        }

        public static int LongestCommonSubsequence(IReadOnlyList<GridPosition> a,
            IReadOnlyList<GridPosition> b)
        {
            if (a == null || b == null) return 0;
            var dp = new int[a.Count + 1, b.Count + 1];
            for (int i = 1; i <= a.Count; i++)
                for (int j = 1; j <= b.Count; j++)
                    dp[i, j] = a[i - 1] == b[j - 1]
                        ? dp[i - 1, j - 1] + 1
                        : Math.Max(dp[i - 1, j], dp[i, j - 1]);
            return dp[a.Count, b.Count];
        }

        public static int CommonOpening(IReadOnlyList<GridPosition> a,
            IReadOnlyList<GridPosition> b)
        {
            if (a == null || b == null) return 0;
            int count = 0;
            while (count < a.Count && count < b.Count && a[count] == b[count])
                count++;
            return count;
        }

        public static Risk Evaluate(int first, int second, bool alternate,
            IReadOnlyList<GridPosition> a, IReadOnlyList<GridPosition> b)
        {
            if (a == null || b == null || a.Count == 0 || b.Count == 0) return null;
            int matched = LongestCommonSubsequence(a, b);
            int opening = CommonOpening(a, b);
            float overlap = (float)matched / Math.Max(a.Count, b.Count);
            if ((matched < 4 || overlap < 0.62f) && opening < 4) return null;
            return new Risk
            {
                firstLevel = first,
                secondLevel = second,
                matchingTaps = matched,
                commonOpening = opening,
                routeOverlap = overlap,
                routeKind = alternate ? "longer-win" : "optimal",
                priority = opening >= 5 || overlap >= 0.78f ? "HIGH" : "REVIEW",
                note = "Shared tap coordinates are a heuristic. Inspect the actual " +
                       "mechanic insight and chain reactions, then blind-playtest both."
            };
        }

        public static int LongestSameAxisRun(IReadOnlyList<GridPosition> route)
        {
            if (route == null || route.Count == 0) return 0;
            int sameColumn = 1, sameRow = 1, maximum = 1;
            for (int i = 1; i < route.Count; i++)
            {
                sameColumn = route[i].x == route[i - 1].x ? sameColumn + 1 : 1;
                sameRow = route[i].y == route[i - 1].y ? sameRow + 1 : 1;
                maximum = Math.Max(maximum, Math.Max(sameColumn, sameRow));
            }
            return maximum;
        }

        [MenuItem("SHIFT/Quality/Export Route Similarities 41-80")]
        public static void Export()
        {
            var report = new Report();
            for (int i = 41; i <= 80; i++)
                for (int j = i + 1; j <= 80; j++)
                {
                    report.evaluatedLevelPairs++;
                    for (int variant = 0; variant < 2; variant++)
                    {
                        bool alternate = variant == 1;
                        var risk = Evaluate(i, j, alternate,
                            Route(i, alternate), Route(j, alternate));
                        if (risk != null) report.risks.Add(risk);
                    }
                }
            for (int n = 41; n <= 80; n++)
            {
                var route = Route(n, false);
                int longest = LongestSameAxisRun(route);
                if (longest < 4) continue;
                report.laneMotifs.Add(new LaneMotifRisk
                {
                    level = n,
                    longestAxisRun = longest,
                    shortestRouteTaps = route.Count,
                    note = "Four or more consecutive taps share a row or column. " +
                           "Inspect whether these are repetitive manual advancement, " +
                           "or genuinely distinct choices/reactions."
                });
            }
            report.laneMotifLevels = report.laneMotifs.Count;
            report.risks.Sort((a, b) =>
            {
                int severity = string.CompareOrdinal(a.priority, b.priority);
                if (severity != 0) return severity;
                return b.routeOverlap.CompareTo(a.routeOverlap);
            });
            report.suspectedPatternPairs = report.risks.Count;
            string directory = Path.Combine(Application.dataPath, "../Validation");
            Directory.CreateDirectory(directory);
            string output = Path.Combine(directory, "level41_80_route_similarity.json");
            File.WriteAllText(output, JsonUtility.ToJson(report, true));
            Debug.Log("SHIFT 41-80 route screening: " +
                report.evaluatedLevelPairs + " level pairs, " +
                report.suspectedPatternPairs + " suspicious route similarities, " +
                report.laneMotifLevels + " corridor-run flags. " +
                "Human review required; report at " + output);
        }
    }
}
