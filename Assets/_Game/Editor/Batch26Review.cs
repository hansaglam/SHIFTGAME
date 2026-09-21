using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Shift.Game.Editor
{
    public static class Batch26Review
    {
        // Authored decision themes are review metadata, not automated measures of player thought.
        // Repeated legal alternatives in the graph must not be counted as new strategic insights.
        public static readonly string[][] DecisionThemes = {
            new[]{"Vacate the upper landing before freeing the lower lane", "Move the support under its own direction rather than pushing it", "Stop using the support once the target lane is free"},
            new[]{"Choose the north pocket rather than the east recess", "Park beyond the first empty cell to vacate the crossing", "Keep the lower return available until the target reaches it"},
            new[]{"Choose the north entry rather than the short east step", "Hand off to the target or continue the supporting push after the upper turn", "Predict the lower turn before admitting the wrong color"},
            new[]{"Choose the independent loop or group redirection", "Merge the returning target at the shared landing or deliver it separately", "Keep the remaining convoy together instead of opening gaps"},
            new[]{"Prepare parking despite the apparently open gate", "Keep the lower entrant out of the box staging bay", "Leave the switch accessible after moving the box", "Restore gate state without occupying the shared gate", "Commit the rear target so both targets share the passage"}
        };
        public static readonly GridPosition[] Longer29 = {
            new GridPosition(2,1),new GridPosition(2,2),new GridPosition(4,1),
            new GridPosition(0,1),new GridPosition(1,1),new GridPosition(2,1),new GridPosition(3,1),new GridPosition(4,1)
        };
        public static readonly GridPosition[] Merged29 = {
            new GridPosition(2,1),new GridPosition(2,2),new GridPosition(0,1),new GridPosition(1,1),
            new GridPosition(2,1),new GridPosition(3,1),new GridPosition(4,1)
        };
        [MenuItem("SHIFT/Analyze Levels 26-30")]
        public static void Analyze()
        {
            string dir=Path.GetFullPath(Path.Combine(Application.dataPath,"../Validation/batch26")); Directory.CreateDirectory(dir);
            foreach(var level in LevelValidation.AllLevels().Skip(25).Take(5))
            {
                var r=new PuzzleBatchAnalysis(level).Analyze(level.KnownSolution);
                File.WriteAllText(Path.Combine(dir,level.name+"-analysis.json"),JsonUtility.ToJson(r,true));
                Debug.Log($"{level.DisplayTitle}: minimum {r.minimum}, budget {level.MoveLimit}, states {r.states}, paths {r.solutionPaths}, " +
                    $"random valid solve {r.randomSolveProbability:P4}, optimal discovery {r.optimalDiscoveryProbability:P4}, final/max depth {r.finalDepth}/{r.maxDepth}.");
            }
        }
    }
}
